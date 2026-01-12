using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FishNetSerializerSourceGenerator
{
    public sealed class ReaderSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;

        // Map from field type -> reader method
        private static readonly Dictionary<SpecialType, string> MethodMap =
            new Dictionary<SpecialType, string>()
            {
            { SpecialType.System_Byte, "ReadUInt8Unpacked" },
            { SpecialType.System_SByte, "ReadInt8Unpacked" },
            { SpecialType.System_Int32, "ReadInt32" },
            { SpecialType.System_UInt32, "ReadUInt32" },
            { SpecialType.System_Int16, "ReadInt16" },
            { SpecialType.System_UInt16, "ReadUInt16" },
            { SpecialType.System_Int64, "ReadInt64" },
            { SpecialType.System_UInt64, "ReadUInt64" },
            { SpecialType.System_Single, "ReadSingle" },
            { SpecialType.System_Double, "ReadDouble" },
            { SpecialType.System_Boolean, "ReadBoolean" },
            { SpecialType.System_Char, "ReadChar" },
            { SpecialType.System_String, "ReadStringAllocated" }
            };

        public ReaderSyntaxRewriter(SemanticModel model)
        {
            _model = model;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (symbol == null)
                return base.VisitInvocationExpression(node);

            // Only target Reader.GetX calls
            if (!symbol.Name.StartsWith("Get"))
                return base.VisitInvocationExpression(node);

            if (symbol.ContainingType.Name != "NetDataReader")
                return base.VisitInvocationExpression(node);

            // Determine field type from left-hand assignment
            var parent = node.Parent as AssignmentExpressionSyntax;
            if (parent == null)
                return base.VisitInvocationExpression(node);

            var leftSymbol = _model.GetSymbolInfo(parent.Left).Symbol;

            if (leftSymbol is IFieldSymbol field && !field.IsStatic)
            {
                // Map field type to reader method
                if (MethodMap.TryGetValue(field.Type.SpecialType, out var readerMethod))
                {
                    // Rebuild invocation: reader.Read{readerMethod}()
                    return SyntaxFactory.InvocationExpression(
                        // reader.Read{readerMethod}
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("reader"),
                            SyntaxFactory.IdentifierName(readerMethod)),
                        // () - zero arguments
                        SyntaxFactory.ArgumentList());
                }
                else if (field.Type.AllInterfaces.Any(i => i.Name == "INetSerializable"))
                {
                    // Keep generic type arguments
                    var genericArgs = symbol.TypeArguments;
                    // Rebuild invocation: reader.Read<T>()
                    return SyntaxFactory.InvocationExpression(
                        // reader.Read
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("reader.Read"),
                            // <T1, T2, ...>
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                    genericArgs.Select(t =>
                                        SyntaxFactory.ParseTypeName(t.ToDisplayString()))))),
                        // () - zero arguments
                        SyntaxFactory.ArgumentList());
                }
            }

            return base.VisitInvocationExpression(node);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol;

            // Rewrite instance fields -> data.field
            if (symbol is IFieldSymbol field && !field.IsStatic)
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("data"),
                    SyntaxFactory.IdentifierName(field.Name));
            }

            // Rewrite NetDataReader parameter
            if (symbol is IParameterSymbol param &&
                param.Type.Name == "NetDataReader")
            {
                return SyntaxFactory.IdentifierName("reader");
            }

            return base.VisitIdentifierName(node);
        }
    }
}
