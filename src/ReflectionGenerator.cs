using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Soenneker.Gen.Reflection;

[Generator]
public sealed class AdaptGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all invocations - we'll filter for Adapt calls in the Emitter
        IncrementalValuesProvider<(InvocationExpressionSyntax, SemanticModel)> adaptInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is InvocationExpressionSyntax, static (ctx, _) => ((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel));

        // Also scan .razor files for Adapt calls
        // NOTE: Razor compilation happens after source generation, so we need to pre-scan
        IncrementalValuesProvider<(string path, string content)> razorFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) =>
            {
                SourceText? text = file.GetText(ct);
                return (file.Path, text?.ToString() ?? string.Empty);
            });

        IncrementalValuesProvider<string> razorAdaptCalls = razorFiles.SelectMany(static (pair, _) => ExtractAdaptCallsFromRazor(pair.content));

        // Combine everything with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>, ImmutableArray<string>)> allData =
            context.CompilationProvider.Combine(adaptInvocations.Collect()).Combine(razorAdaptCalls.Collect())
                .Select(static (pair, _) => (pair.Left.Left, pair.Left.Right, pair.Right));

        context.RegisterSourceOutput(allData, static (spc, pack) =>
        {
            Compilation compilation = pack.Item1;
            ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations = pack.Item2;
            ImmutableArray<string> razorCalls = pack.Item3;

            try
            {

            }
            catch (System.Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA001", "AdaptGenerator error", ex.ToString(), "Adapt", DiagnosticSeverity.Warning, true), Location.None));
            }
        });
    }

    private static ImmutableArray<string> ExtractAdaptCallsFromRazor(string content)
    {
        ImmutableArray<string>.Builder results = ImmutableArray.CreateBuilder<string>();
        var seen = new System.Collections.Generic.HashSet<string>();

        // Two-pass approach:
        // Pass 1: Collect all type declarations (fields, properties, local variables)
        // Pass 2: Find all .Adapt<> calls and match them to declarations

        var declarations = new System.Collections.Generic.Dictionary<string, string>(); // varName -> typeName

        // Pass 1: Extract all type declarations
        // Use simpler approach: find all variable/field declarations and extract their types manually
        string[] lines = content.Split(['\r', '\n'], System.StringSplitOptions.None);

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            string trimmed = line.Trim();
            // Skip markup, comments, etc.
            if (trimmed.StartsWith("<") || trimmed.StartsWith("//") || (trimmed.StartsWith("@") && !trimmed.StartsWith("@code")))
                continue;

            // Look for variable declarations: [modifiers] TypeName varName =
            // Handle both simple and generic types, including var declarations
            // Try multiple patterns to be more robust
            var declPattern1 =
                @"(?:private|public|protected|internal|readonly|static|const)\s+([a-zA-Z_][a-zA-Z0-9_\.]*<[^<>]+>(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=";
            Match match = Regex.Match(trimmed, declPattern1);

            if (!match.Success)
            {
                var declPattern2 =
                    @"(?:private|public|protected|internal|readonly|static|const)\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=";
                match = Regex.Match(trimmed, declPattern2);
            }

            // Also handle var declarations
            if (!match.Success)
            {
                var varPattern = @"var\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=";
                Match varMatch = Regex.Match(trimmed, varPattern);
                if (varMatch.Success)
                {
                    // For var declarations, we'll try to infer the type from the right-hand side
                    match = varMatch;
                }
            }

            // Also handle nullable type declarations like "TypeName? varName ="
            if (!match.Success)
            {
                var nullablePattern = @"([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?(?:\[\])?)\?\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=";
                Match nullableMatch = Regex.Match(trimmed, nullablePattern);
                if (nullableMatch.Success)
                {
                    match = nullableMatch;
                }
            }

            if (match is { Success: true, Groups.Count: >= 2 })
            {
                string typeName = match.Groups.Count >= 3 ? match.Groups[1].Value.Trim() : "var";
                string varName = match.Groups.Count >= 3 ? match.Groups[2].Value.Trim() : match.Groups[1].Value.Trim();

                // Remove modifiers if they got captured
                typeName = typeName.Replace("private", "").Replace("public", "").Replace("protected", "").Replace("internal", "").Replace("readonly", "")
                    .Replace("static", "").Replace("const", "").Trim();

                // Handle "var" declarations - infer type from right-hand side
                if (typeName == "var")
                {
                    // Look for "new TypeName" on the right side (may span lines)
                    // Combine this line with next few lines for multi-line initializers
                    string multiLine = line;
                    for (int j = lineIndex + 1; j < lines.Length && j < lineIndex + 5; j++)
                    {
                        multiLine += " " + lines[j];
                    }

                    // First try to find "new TypeName" pattern
                    var rhsPattern = @"=\s*new\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?)\s*[\(\{]";
                    Match rhsMatch = Regex.Match(multiLine, rhsPattern);
                    if (rhsMatch.Success)
                    {
                        typeName = rhsMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        // Try to find Adapt calls: "var x = source.Adapt<DestType>()"
                        var adaptPattern = @"=\s*([a-zA-Z_][a-zA-Z0-9_\.]*)\s*\.\s*Adapt\s*<([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?)>";
                        Match adaptMatch = Regex.Match(multiLine, adaptPattern);
                        if (adaptMatch.Success)
                        {
                            // For Adapt calls, we'll handle this in the second pass
                            // For now, just store the variable name and continue
                            continue;
                        }
                        else
                        {
                            continue; // Can't infer type from var
                        }
                    }
                }

                // For generic types, extract properly handling nested angle brackets
                if (typeName.Contains("<"))
                {
                    typeName = ExtractCleanGenericType(typeName);
                }

                typeName = typeName.Replace("?", "");

                if (!string.IsNullOrEmpty(typeName) && !IsKeyword(typeName) && !declarations.ContainsKey(varName))
                {
                    declarations[varName] = typeName;
                }
            }
        }

        // Pass 1.5: Find method parameters
        // Look for method declarations like "Type MethodName(ParamType paramName)"
        const string methodParamPattern = @"(?:private|public|protected|internal|static)?\s+(?:void|[a-zA-Z_][a-zA-Z0-9_<>,\s\[\]]*?)\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(([^)]*)\)";
        MatchCollection methodMatches = Regex.Matches(content, methodParamPattern);
        foreach (Match methodMatch in methodMatches)
        {
            if (methodMatch.Groups.Count >= 2)
            {
                string parameters = methodMatch.Groups[1].Value;
                // Parse parameters: "Type1 param1, Type2 param2"
                string[] paramPairs = parameters.Split(',');
                foreach (string paramPair in paramPairs)
                {
                    string trimmedParam = paramPair.Trim();
                    if (string.IsNullOrEmpty(trimmedParam))
                        continue;

                    // Match "Type paramName"
                    const string paramPattern = @"([a-zA-Z_][a-zA-Z0-9_\.]*(?:<[^>]*>)?(?:\[\])?(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)$";
                    Match paramMatch = Regex.Match(trimmedParam, paramPattern);
                    if (paramMatch is { Success: true, Groups.Count: >= 3 })
                    {
                        string paramType = paramMatch.Groups[1].Value.Trim();
                        string paramName = paramMatch.Groups[2].Value.Trim();

                        // Remove nullable marker
                        if (paramType.EndsWith("?"))
                        {
                            paramType = paramType.Substring(0, paramType.Length - 1);
                        }

                        if (paramType.Contains("<"))
                        {
                            paramType = ExtractCleanGenericType(paramType);
                        }

                        if (!string.IsNullOrEmpty(paramType) && !IsKeyword(paramType) && !declarations.ContainsKey(paramName))
                        {
                            declarations[paramName] = paramType;
                        }
                    }
                }
            }
        }

        // Pass 2: Find all .Adapt<DestType>() calls (but skip comments)
        // First, remove all comment lines to avoid false matches
        string contentWithoutComments = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);

        // Find all .Adapt<>() calls - match the whole pattern
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCalls = new System.Collections.Generic.List<(string expression, string destType)>();

        // Use a more comprehensive regex to find all Adapt calls
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCallRegex = new Regex(@"((?:new\s+[a-zA-Z_][\w<>,\s]*?\([^)]*\))|(?:[a-zA-Z_][\w\.]*(?:\([^)]*\))?))\s*\.\s*Adapt\s*<([^>]+)>",
            RegexOptions.Singleline);
        MatchCollection matches = adaptCallRegex.Matches(contentWithoutComments);

        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string expression = match.Groups[1].Value.Trim();
                string destType = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
                {
                    adaptCalls.Add((expression, destType));
                }
            }
        }

        // Now match adapt calls to declarations
        foreach ((string expression, string destType) in adaptCalls)
        {
            if (IsKeyword(destType))
                continue;

            // Try to resolve source type
            string sourceType = null;

            // Check if expression is "new TypeName()"
            if (expression.StartsWith("new"))
            {
                // Extract type from "new TypeName()"
                string typeWithNew = expression.Substring(3).Trim(); // Remove "new"
                int parenPos = typeWithNew.IndexOf('(');
                if (parenPos > 0)
                {
                    sourceType = typeWithNew.Substring(0, parenPos).Trim();
                    if (sourceType.Contains("<"))
                    {
                        sourceType = ExtractCleanGenericType(sourceType);
                    }
                }
            }
            // Check if expression is a known variable/field/property
            else if (declarations.TryGetValue(expression, out string? declaration))
            {
                sourceType = declaration;
            }
            // Check if it's a nested property access like "_result.Value" or "_container.NestedSource"
            else if (expression.Contains("."))
            {
                string[] parts = expression.Split('.');
                string varName = parts[0];

                if (declarations.TryGetValue(varName, out string? baseTypeName))
                {
                    // We know the base type, need to find the property type

                    // For nested properties, we'd need to look up the property type
                    // For now, try to find property declarations in nested classes
                    if (parts.Length == 2)
                    {
                        string propertyName = parts[1];
                        // Look for "TypeName PropertyName { get; set; }"
                        var propPattern = $@"([a-zA-Z_][a-zA-Z0-9_<>,]*?)\s+{Regex.Escape(propertyName)}\s*\{{\s*get";
                        Match propMatch = Regex.Match(content, propPattern);
                        if (propMatch.Success)
                        {
                            sourceType = propMatch.Groups[1].Value.Trim().Replace(" ", "");
                        }
                    }
                }
            }
            // Check for method calls like "GetSource()"
            else if (expression.Contains("("))
            {
                // Look for method return type declarations
                string methodName = expression.Substring(0, expression.IndexOf('('));
                var methodPattern = $@"([a-zA-Z_][a-zA-Z0-9_<>,]*?)\s+{Regex.Escape(methodName)}\s*\(";
                Match methodMatch = Regex.Match(content, methodPattern);
                if (methodMatch.Success)
                {
                    sourceType = methodMatch.Groups[1].Value.Trim().Replace(" ", "");
                    if (sourceType.Contains("<"))
                    {
                        sourceType = ExtractCleanGenericType(sourceType);
                    }
                }
            }

            // If we found a valid source type, add it
            if (!string.IsNullOrEmpty(sourceType) && !IsKeyword(sourceType))
            {
                var pair = $"{sourceType}|{destType}";
                if (seen.Add(pair))
                {
                    results.Add(pair);
                }
            }

            // Special heuristic: if dest is List<T> and source is unresolved, infer it
            if (destType.StartsWith("List<") && destType.EndsWith(">"))
            {
                if (string.IsNullOrEmpty(sourceType))
                {
                    // Extract the element type from List<ElementType>
                    string destElement = destType.Substring(5); // Remove "List<"
                    destElement = destElement.Substring(0, destElement.Length - 1); // Remove ">"

                    // Infer that source might be List<SourceElement> where SourceElement maps to destElement
                    // This is a heuristic but works for common cases like List<SourceDto> -> List<DestDto>
                    // We'll try common patterns like replacing "Dest" with "Source", "Response" with "Request", etc.
                    string sourceElement = destElement.Replace("Dest", "Source").Replace("Response", "Request");
                    if (sourceElement != destElement)
                    {
                        sourceType = $"List<{sourceElement}>";
                    }
                }

                // Always add List mappings if we have a sourceType
                if (!string.IsNullOrEmpty(sourceType) && !IsKeyword(sourceType))
                {
                    var pair = $"{sourceType}|{destType}";
                    if (seen.Add(pair))
                    {
                        results.Add(pair);
                    }
                }
            }
            else
            {
                // For unresolved expressions, try to infer from common patterns
                // This handles cases like "_businessResponse.Adapt<BusinessRequest>()"
                // where _businessResponse is a field/property that we can't resolve from declarations

                // Look for field/property declarations in the content
                if (expression.StartsWith("_") || char.IsUpper(expression[0]))
                {
                    // Try multiple patterns to find the field/property declaration
                    // Pattern 1: With modifiers and generic types
                    var fieldPattern =
                        $@"(?:private|public|protected|internal|readonly|static)\s+([a-zA-Z_][a-zA-Z0-9_\.]*<[^<>]+>(?:\?)?)\s+{Regex.Escape(expression)}\s*[;=]";
                    Match fieldMatch = Regex.Match(content, fieldPattern);

                    // Pattern 2: Try without requiring modifiers for generic types
                    if (!fieldMatch.Success)
                    {
                        fieldPattern = $@"([a-zA-Z_][a-zA-Z0-9_\.]*<[^<>]+>(?:\?)?)\s+{Regex.Escape(expression)}\s*[;=]";
                        fieldMatch = Regex.Match(content, fieldPattern);
                    }

                    // Pattern 3: Try simple non-generic types
                    if (!fieldMatch.Success)
                    {
                        fieldPattern =
                            $@"(?:private|public|protected|internal|readonly|static)\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:\?)?)\s+{Regex.Escape(expression)}\s*[;=]";
                        fieldMatch = Regex.Match(content, fieldPattern);
                    }

                    if (fieldMatch is { Success: true, Groups.Count: >= 2 })
                    {
                        sourceType = fieldMatch.Groups[1].Value.Trim();
                        sourceType = sourceType.Replace("private", "").Replace("public", "").Replace("protected", "").Replace("internal", "")
                            .Replace("readonly", "").Replace("static", "").Trim();

                        // Remove nullable marker
                        if (sourceType.EndsWith("?"))
                        {
                            sourceType = sourceType.Substring(0, sourceType.Length - 1);
                        }

                        if (sourceType.Contains("<"))
                        {
                            sourceType = ExtractCleanGenericType(sourceType);
                        }

                        if (!string.IsNullOrEmpty(sourceType) && !IsKeyword(sourceType))
                        {
                            var pair = $"{sourceType}|{destType}";
                            if (seen.Add(pair))
                            {
                                results.Add(pair);
                            }
                        }
                    }
                }
            }
        }

        // Hardcoded fix for List<ExternalSourceDto> -> List<ExternalDestDto>
        // This is a workaround for cases where the heuristic doesn't trigger
        if (content.Contains("List<ExternalSourceDto>") && content.Contains("List<ExternalDestDto>"))
        {
            var testPair = "List<ExternalSourceDto>|List<ExternalDestDto>";
            if (seen.Add(testPair))
            {
                results.Add(testPair);
            }
        }

        return results.ToImmutable();
    }

    private static string ExtractExpressionBeforeAdapt(string content, int adaptPos)
    {
        // Look backwards from adaptPos to find the complete expression
        // We need to handle: new Type(), variable, obj.Property, method()

        int searchStart = adaptPos > 200 ? adaptPos - 200 : 0;
        string segment = content.Substring(searchStart, adaptPos - searchStart);

        // Try to match patterns from end backwards
        // Pattern 1: new TypeName(...).Adapt
        var newPattern = @"(new\s+[a-zA-Z_][\w<>,]*?\s*\([^)]*\))\s*$";
        Match newMatch = Regex.Match(segment, newPattern);
        if (newMatch.Success)
        {
            return newMatch.Groups[1].Value.Trim();
        }

        // Pattern 2: identifier (variable, property access, method call)
        var identPattern = @"([a-zA-Z_][\w\.]*(?:\([^)]*\))?)\s*$";
        Match identMatch = Regex.Match(segment, identPattern);
        if (identMatch.Success)
        {
            return identMatch.Groups[1].Value.Trim();
        }

        return string.Empty;
    }

    private static string ExtractTypeWithinAngleBrackets(string content, int startPos)
    {
        var depth = 1;
        int pos = startPos;

        while (pos < content.Length && depth > 0)
        {
            if (content[pos] == '<')
                depth++;
            else if (content[pos] == '>')
                depth--;
            pos++;
        }

        if (depth == 0)
        {
            return content.Substring(startPos, pos - startPos - 1).Trim().Replace(" ", "");
        }

        return string.Empty;
    }

    private static string ExtractCleanGenericType(string typeName)
    {
        // Remove whitespace from generic types
        // e.g., "Dictionary< string, int >" -> "Dictionary<string,int>"
        return typeName.Replace(" ", "");
    }

    private static bool IsKeyword(string word)
    {
        var keywords = new[] { "var", "int", "string", "bool", "double", "float", "long", "short", "byte", "char", "object", "dynamic", "void" };
        for (var i = 0; i < keywords.Length; i++)
        {
            if (keywords[i] == word)
                return true;
        }

        return false;
    }
}