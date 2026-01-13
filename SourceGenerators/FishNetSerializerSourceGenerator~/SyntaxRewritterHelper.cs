using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FishNetSerializerSourceGenerator
{
    public static class SyntaxRewritterHelper
    {
        public static bool IsListType(ITypeSymbol type, out ITypeSymbol elementType)
        {
            elementType = null;

            if (type is not INamedTypeSymbol named)
                return false;

            // List?
            if (named.Name != "List")
                return false;

            if (named.ContainingNamespace.ToDisplayString() != "System.Collections.Generic")
                return false;

            // TValue
            if (named.TypeArguments.Length != 1)
                return false;

            elementType = named.TypeArguments[0];
            return true;
        }

        public static bool IsDictionary(ITypeSymbol type, out ITypeSymbol keyType, out ITypeSymbol valueType)
        {
            keyType = null;
            valueType = null;

            if (type is not INamedTypeSymbol named)
                return false;

            // Dictionary?
            if (named.Name != "Dictionary")
                return false;

            if (named.ContainingNamespace.ToDisplayString() != "System.Collections.Generic")
                return false;

            // TKey, TValue
            if (named.TypeArguments.Length != 2)
                return false;

            keyType = named.TypeArguments[0];
            valueType = named.TypeArguments[1];
            return true;
        }

        public static bool IsReaderMethod(DiagnosticReporter reporter, SemanticModel model, InvocationExpressionSyntax node)
        {
            var symbol = model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (symbol == null)
                return false;

            // Only target Reader.GetX calls
            if (!symbol.Name.StartsWith("Get"))
                return false;

            if (!symbol.IsExtensionMethod)
            {
                return symbol.ContainingType.Name == "NetDataReader";
            }
            else
            {
                return symbol.ReceiverType.Name == "NetDataReader";
            }
        }

        public static bool IsWriterMethod(DiagnosticReporter reporter, SemanticModel model, InvocationExpressionSyntax node, out ArgumentSyntax arg, out ITypeSymbol argType)
        {
            arg = null;
            argType = null;
            var symbol = model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (symbol == null)
                return false;

            // Match Writer.Put(...)
            if (!symbol.Name.StartsWith("Put"))
                return false;

            if (!symbol.IsExtensionMethod)
            {
                if (symbol.ContainingType.Name != "NetDataWriter")
                    return false;

                // Expect exactly one argument
                if (node.ArgumentList.Arguments.Count != 1)
                    return false;

                var valueArgument = node.ArgumentList.Arguments[0];
                arg = valueArgument;
                argType = model.GetTypeInfo(valueArgument.Expression).Type;
            }
            else
            {
                if (symbol.ReceiverType.Name != "NetDataWriter")
                    return false;

                // Expect exactly one argument
                if (node.ArgumentList.Arguments.Count != 1)
                    return false;

                var valueArgument = node.ArgumentList.Arguments[0];
                arg = valueArgument;
                argType = model.GetTypeInfo(valueArgument.Expression).Type;
            }

            if (argType == null)
                return false;

            return true;
        }
    }
}
