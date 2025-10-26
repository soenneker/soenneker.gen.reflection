using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace Soenneker.Gen.Reflection.Emitters;

/// <summary>
/// Generates perfect hash switches for fast name-based lookups
/// </summary>
internal static class PerfectHashSwitchGenerator
{
    /// <summary>
    /// Generates a perfect hash switch for field names
    /// </summary>
    public static string GenerateFieldSwitch(IFieldSymbol[] fields, string typeId)
    {
        if (fields.Length == 0)
            return "return null;";

        var sb = new StringBuilder();
        
        // Group by length first for better performance
        var byLength = fields.GroupBy(f => f.Name.Length).OrderBy(g => g.Key);
        
        sb.AppendLine("switch (name.Length)");
        sb.AppendLine("{");
        
        foreach (var lengthGroup in byLength)
        {
            sb.AppendLine($"    case {lengthGroup.Key}:");
            sb.AppendLine("    {");
            
            if (lengthGroup.Count() == 1)
            {
                var field = lengthGroup.First();
                sb.AppendLine($"        if (name == \"{field.Name}\")");
                sb.AppendLine($"            return FieldRegistry.GetField({GetFieldId(field)}UL);");
                sb.AppendLine("        break;");
            }
            else
            {
                // Use first character for additional disambiguation
                var byFirstChar = lengthGroup.GroupBy(f => f.Name[0]);
                
                foreach (var charGroup in byFirstChar)
                {
                    sb.AppendLine($"        if (name[0] == '{charGroup.Key}')");
                    sb.AppendLine("        {");
                    
                    if (charGroup.Count() == 1)
                    {
                        var field = charGroup.First();
                        sb.AppendLine($"            if (name == \"{field.Name}\")");
                        sb.AppendLine($"                return FieldRegistry.GetField({GetFieldId(field)}UL);");
                    }
                    else
                    {
                        // Use hash for final disambiguation
                        foreach (var field in charGroup)
                        {
                            var hash = GetStringHash(field.Name);
                            sb.AppendLine($"            if (name.GetHashCode() == {hash} && name == \"{field.Name}\")");
                            sb.AppendLine($"                return FieldRegistry.GetField({GetFieldId(field)}UL);");
                        }
                    }
                    
                    sb.AppendLine("        }");
                }
            }
            
            sb.AppendLine("    }");
        }
        
        sb.AppendLine("}");
        sb.AppendLine("return null;");
        
        return sb.ToString();
    }

    /// <summary>
    /// Generates a perfect hash switch for property names
    /// </summary>
    public static string GeneratePropertySwitch(IPropertySymbol[] properties, string typeId)
    {
        if (properties.Length == 0)
            return "return null;";

        var sb = new StringBuilder();
        
        // Group by length first for better performance
        var byLength = properties.GroupBy(p => p.Name.Length).OrderBy(g => g.Key);
        
        sb.AppendLine("switch (name.Length)");
        sb.AppendLine("{");
        
        foreach (var lengthGroup in byLength)
        {
            sb.AppendLine($"    case {lengthGroup.Key}:");
            sb.AppendLine("    {");
            
            if (lengthGroup.Count() == 1)
            {
                var property = lengthGroup.First();
                sb.AppendLine($"        if (name == \"{property.Name}\")");
                sb.AppendLine($"            return PropertyRegistry.GetProperty({GetPropertyId(property)}UL);");
                sb.AppendLine("        break;");
            }
            else
            {
                // Use first character for additional disambiguation
                var byFirstChar = lengthGroup.GroupBy(p => p.Name[0]);
                
                foreach (var charGroup in byFirstChar)
                {
                    sb.AppendLine($"        if (name[0] == '{charGroup.Key}')");
                    sb.AppendLine("        {");
                    
                    if (charGroup.Count() == 1)
                    {
                        var property = charGroup.First();
                        sb.AppendLine($"            if (name == \"{property.Name}\")");
                        sb.AppendLine($"                return PropertyRegistry.GetProperty({GetPropertyId(property)}UL);");
                    }
                    else
                    {
                        // Use hash for final disambiguation
                        foreach (var property in charGroup)
                        {
                            var hash = GetStringHash(property.Name);
                            sb.AppendLine($"            if (name.GetHashCode() == {hash} && name == \"{property.Name}\")");
                            sb.AppendLine($"                return PropertyRegistry.GetProperty({GetPropertyId(property)}UL);");
                        }
                    }
                    
                    sb.AppendLine("        }");
                }
            }
            
            sb.AppendLine("    }");
        }
        
        sb.AppendLine("}");
        sb.AppendLine("return null;");
        
        return sb.ToString();
    }

    /// <summary>
    /// Generates a perfect hash switch for method names
    /// </summary>
    public static string GenerateMethodSwitch(IMethodSymbol[] methods, string typeId)
    {
        if (methods.Length == 0)
            return "return null;";

        var sb = new StringBuilder();
        
        // Group by length first for better performance
        var byLength = methods.GroupBy(m => m.Name.Length).OrderBy(g => g.Key);
        
        sb.AppendLine("switch (name.Length)");
        sb.AppendLine("{");
        
        foreach (var lengthGroup in byLength)
        {
            sb.AppendLine($"    case {lengthGroup.Key}:");
            sb.AppendLine("    {");
            
            if (lengthGroup.Count() == 1)
            {
                var method = lengthGroup.First();
                sb.AppendLine($"        if (name == \"{method.Name}\")");
                sb.AppendLine($"            return MethodRegistry.GetMethod({GetMethodId(method)}UL);");
                sb.AppendLine("        break;");
            }
            else
            {
                // Use first character for additional disambiguation
                var byFirstChar = lengthGroup.GroupBy(m => m.Name[0]);
                
                foreach (var charGroup in byFirstChar)
                {
                    sb.AppendLine($"        if (name[0] == '{charGroup.Key}')");
                    sb.AppendLine("        {");
                    
                    if (charGroup.Count() == 1)
                    {
                        var method = charGroup.First();
                        sb.AppendLine($"            if (name == \"{method.Name}\")");
                        sb.AppendLine($"                return MethodRegistry.GetMethod({GetMethodId(method)}UL);");
                    }
                    else
                    {
                        // Use hash for final disambiguation
                        foreach (var method in charGroup)
                        {
                            var hash = GetStringHash(method.Name);
                            sb.AppendLine($"            if (name.GetHashCode() == {hash} && name == \"{method.Name}\")");
                            sb.AppendLine($"                return MethodRegistry.GetMethod({GetMethodId(method)}UL);");
                        }
                    }
                    
                    sb.AppendLine("        }");
                }
            }
            
            sb.AppendLine("    }");
        }
        
        sb.AppendLine("}");
        sb.AppendLine("return null;");
        
        return sb.ToString();
    }

    private static string GetFieldId(IFieldSymbol field)
    {
        // In a real implementation, you'd track the actual field IDs
        return ((ulong)field.GetHashCode()).ToString();
    }

    private static string GetPropertyId(IPropertySymbol property)
    {
        // In a real implementation, you'd track the actual property IDs
        return ((ulong)property.GetHashCode()).ToString();
    }

    private static string GetMethodId(IMethodSymbol method)
    {
        // In a real implementation, you'd track the actual method IDs
        return ((ulong)method.GetHashCode()).ToString();
    }

    private static int GetStringHash(string str)
    {
        // Use the same hash algorithm as string.GetHashCode()
        return str.GetHashCode();
    }
}
