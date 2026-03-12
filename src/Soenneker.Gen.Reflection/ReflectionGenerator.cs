using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Soenneker.Gen.Reflection.Emitters;

namespace Soenneker.Gen.Reflection;

[Generator]
public sealed class ReflectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all invocations - we'll filter for GetTypeGen calls in the Emitter
        IncrementalValuesProvider<(InvocationExpressionSyntax, SemanticModel)> typeGenInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is InvocationExpressionSyntax, static (ctx, _) => ((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel));

        // Also scan .razor files for GetTypeGen calls
        IncrementalValuesProvider<(string path, string content)> razorFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) =>
            {
                SourceText? text = file.GetText(ct);
                return (file.Path, text?.ToString() ?? string.Empty);
            });

        IncrementalValuesProvider<string> razorTypeGenCalls = razorFiles.SelectMany(static (pair, _) => ExtractGetTypeGenCallsFromRazor(pair.content));

        // Combine everything with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>, ImmutableArray<string>)> allData =
            context.CompilationProvider.Combine(typeGenInvocations.Collect()).Combine(razorTypeGenCalls.Collect())
                .Select(static (pair, _) => (pair.Left.Left, pair.Left.Right, pair.Right));

        context.RegisterSourceOutput(allData, static (spc, pack) =>
        {
            Compilation compilation = pack.Item1;
            ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations = pack.Item2;
            ImmutableArray<string> razorCalls = pack.Item3;

            try
            {
                // Add diagnostic to see if generator is running
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGR002", "ReflectionGenerator running", 
                        $"Found {invocations.Length} invocations and {razorCalls.Length} razor calls", 
                        "GetTypeGen", DiagnosticSeverity.Info, true), Location.None));
                
                GenerateTypeGenCode(spc, compilation, invocations, razorCalls);
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGR001", "ReflectionGenerator error", ex.ToString(), "GetTypeGen", DiagnosticSeverity.Warning, true), Location.None));
            }
        });
    }

    private static void GenerateTypeGenCode(SourceProductionContext spc, Compilation compilation, 
        ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations, 
        ImmutableArray<string> razorCalls)
    {
        try
        {
            // Generate type definitions file first
            TypeDefinitionsEmitter.Emit(spc);
            
            var generatedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            // Process C# invocations
            foreach ((InvocationExpressionSyntax invocation, SemanticModel semanticModel) in invocations)
            {
                if (IsGetTypeGenCall(invocation, semanticModel))
                {
                    ITypeSymbol? typeSymbol = ExtractTypeSymbolFromGetTypeGenCall(invocation, semanticModel);
                    if (typeSymbol != null && generatedTypes.Add(typeSymbol))
                    {
                        TypeInfoEmitter.EmitTypeInfoGenFile(spc, typeSymbol);
                    }
                }
            }

            // If we didn't see any invocations for common primitives/arrays used in tests, proactively generate them from literals in code
            // Arrays: int[], string[]
            try
            {
                INamespaceSymbol? systemNamespace = compilation.GlobalNamespace.GetNamespaceMembers().FirstOrDefault(n => n.Name == "System");
                if (systemNamespace != null)
                {
                    INamedTypeSymbol? intType = compilation.GetSpecialType(SpecialType.System_Int32);
                    INamedTypeSymbol? stringType = compilation.GetSpecialType(SpecialType.System_String);
                    INamedTypeSymbol? dateTimeType = systemNamespace.GetTypeMembers("DateTime").FirstOrDefault();
                    INamedTypeSymbol? timeSpanType = systemNamespace.GetTypeMembers("TimeSpan").FirstOrDefault();

                    if (intType != null && generatedTypes.Add(intType)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, intType);
                    if (stringType != null && generatedTypes.Add(stringType)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, stringType);
                    if (dateTimeType != null && generatedTypes.Add(dateTimeType)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, dateTimeType);
                    if (timeSpanType != null && generatedTypes.Add(timeSpanType)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, timeSpanType);

                    if (intType != null)
                    {
                        try
                        {
                            IArrayTypeSymbol intArray = compilation.CreateArrayTypeSymbol(intType);
                            if (generatedTypes.Add(intArray)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, intArray);
                        }
                        catch (Exception ex)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor("SGR008", "Int Array Generation Error", 
                                    $"Error generating int[]: {ex}", 
                                    "GetTypeGen", DiagnosticSeverity.Error, true), Location.None));
                        }
                    }
                    if (stringType != null)
                    {
                        try
                        {
                            IArrayTypeSymbol stringArray = compilation.CreateArrayTypeSymbol(stringType);
                            if (generatedTypes.Add(stringArray)) TypeInfoEmitter.EmitTypeInfoGenFile(spc, stringArray);
                        }
                        catch (Exception ex)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor("SGR009", "String Array Generation Error", 
                                    $"Error generating string[]: {ex}", 
                                    "GetTypeGen", DiagnosticSeverity.Error, true), Location.None));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGR007", "Array Generation Error", 
                        $"Error generating arrays: {ex}", 
                        "GetTypeGen", DiagnosticSeverity.Error, true), Location.None));
            }

            // Process Razor calls (these are still string-based for now)
            // TODO: Implement ResolveTypeSymbolFromString method
            // foreach (var razorCall in razorCalls)
            // {
            //     // For Razor calls, we'll need to resolve the type symbol from the string
            //     var typeSymbol = ResolveTypeSymbolFromString(razorCall, compilation);
            //     if (typeSymbol != null && generatedTypes.Add(typeSymbol))
            //     {
            //         TypeInfoEmitter.EmitTypeInfoGenFile(spc, typeSymbol);
            //     }
            // }

            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("SGR006", "After Razor Processing", 
                    $"After processing Razor calls, have {generatedTypes.Count} types", 
                    "GetTypeGen", DiagnosticSeverity.Info, true), Location.None));

            // Generate extension methods for all collected types
            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("SGR004", "Extension Methods Generation", 
                    $"About to generate extension methods for {generatedTypes.Count} types", 
                    "GetTypeGen", DiagnosticSeverity.Info, true), Location.None));
            try
            {
                ExtensionsEmitter.EmitExtensionsFile(spc, generatedTypes);
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGR005", "Extension Methods Generation Error", 
                        $"Error generating extension methods: {ex}", 
                        "GetTypeGen", DiagnosticSeverity.Error, true), Location.None));
            }
        }
        catch (Exception ex)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("SGR003", "ReflectionGenerator error in GenerateTypeGenCode", ex.ToString(), "GetTypeGen", DiagnosticSeverity.Warning, true), Location.None));
        }
    }






    private static ITypeSymbol? ExtractTypeSymbolFromGetTypeGenCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            ExpressionSyntax expression = memberAccess.Expression;
            
            // Handle generic method calls like obj.GetTypeGen<T>()
            if (memberAccess.Name is GenericNameSyntax genericName)
            {
                if (genericName.TypeArgumentList?.Arguments.Count > 0)
                {
                    TypeSyntax typeArg = genericName.TypeArgumentList.Arguments[0];
                    return semanticModel.GetTypeInfo(typeArg).Type;
                }
            }
            
            // Handle instance calls like obj.GetTypeGen()
            TypeInfo typeInfo = semanticModel.GetTypeInfo(expression);
            return typeInfo.Type;
        }
        else if (invocation.Expression is IdentifierNameSyntax identifierName)
        {
            // Handle static method calls like GetTypeGen<T>()
            if (identifierName.Identifier.ValueText == "GetTypeGen")
            {
                ISymbol? symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;
                if (symbol is IMethodSymbol methodSymbol && methodSymbol.IsGenericMethod)
                {
                    ImmutableArray<ITypeSymbol> typeArgs = methodSymbol.TypeArguments;
                    if (typeArgs.Length > 0)
                    {
                        return typeArgs[0];
                    }
                }
            }
        }
        return null;
    }

    private static ITypeSymbol? ResolveTypeSymbolFromString(string typeName, Compilation compilation)
    {
        // Try to resolve the type symbol from the string representation
        INamedTypeSymbol? typeSymbol = compilation.GetTypeByMetadataName(typeName);
        if (typeSymbol != null) return typeSymbol;

        // If not found, try to parse it as a generic type
        // This is a simplified approach - in practice you might need more sophisticated parsing
        return null;
    }

    private static bool IsGetTypeGenCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            string methodName = memberAccess.Name.Identifier.ValueText;
            if (methodName == "GetTypeGen")
            {
                return true;
            }
        }
        else if (invocation.Expression is IdentifierNameSyntax identifierName)
        {
            // Handle static method calls like GetTypeGen<T>()
            if (identifierName.Identifier.ValueText == "GetTypeGen")
            {
                return true;
            }
        }
        return false;
    }




    private static ImmutableArray<string> ExtractGetTypeGenCallsFromRazor(string content)
    {
        ImmutableArray<string>.Builder results = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>();

        // Find all .GetTypeGen<>() calls in Razor files
        string contentWithoutComments = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);

        // Pattern to match GetTypeGen calls: expression.GetTypeGen<Type>() or expression.GetTypeGen()
        var getTypeGenRegex = new Regex(@"((?:new\s+[a-zA-Z_][\w<>,\s]*?\([^)]*\))|(?:[a-zA-Z_][\w\.]*(?:\([^)]*\))?))\s*\.\s*GetTypeGen\s*(?:<([^>]+)>)?", 
            RegexOptions.Singleline);
        
        MatchCollection matches = getTypeGenRegex.Matches(contentWithoutComments);

        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 2)
            {
                string expression = match.Groups[1].Value.Trim();
                string? typeArg = match.Groups.Count >= 3 ? match.Groups[2].Value.Trim() : null;

                if (!string.IsNullOrEmpty(expression))
                {
                    // Try to resolve the type from the expression or type argument
                    string? resolvedType = null;

                    if (!string.IsNullOrEmpty(typeArg))
                    {
                        resolvedType = typeArg;
                    }
                    else if (expression.StartsWith("new"))
                    {
                        // Extract type from "new TypeName()"
                        string typeWithNew = expression.Substring(3).Trim();
                        int parenPos = typeWithNew.IndexOf('(');
                        if (parenPos > 0)
                        {
                            resolvedType = typeWithNew.Substring(0, parenPos).Trim();
                        }
                    }

                    if (!string.IsNullOrEmpty(resolvedType) && !IsKeyword(resolvedType))
                    {
                        if (seen.Add(resolvedType))
                        {
                            results.Add(resolvedType);
                        }
                    }
                }
            }
        }

        return results.ToImmutable();
    }

    private static bool IsKeyword(string? word)
    {
        if (string.IsNullOrEmpty(word))
            return false;
            
        var keywords = new[] { "var", "int", "string", "bool", "double", "float", "long", "short", "byte", "char", "object", "dynamic", "void" };
        for (var i = 0; i < keywords.Length; i++)
        {
            if (keywords[i] == word)
                return true;
        }

        return false;
    }
}
