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
        private readonly DiagnosticReporter _reporter;

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

        public ReaderSyntaxRewriter(SemanticModel model, DiagnosticReporter reporter)
        {
            _model = model;
            _reporter = reporter;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (symbol == null)
                return base.VisitInvocationExpression(node);

            if (!SyntaxRewritterHelper.IsReaderMethod(_reporter, _model, node))
                return base.VisitInvocationExpression(node);

            // -------------------------
            // ARRAY SUPPORT
            // -------------------------
            if (symbol.ReturnType is IArrayTypeSymbol arrayType)
            {
                // Keep generic type arguments
                var elementType = arrayType.ElementType;
                // Rebuild invocation: reader.ReadArrayAllocated<TType>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // reader.ReadArrayAllocated
                        SyntaxFactory.Identifier("reader.ReadArrayAllocated"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.ParseTypeName(
                                    elementType.ToDisplayString())))),
                    // () - zero arguments
                    SyntaxFactory.ArgumentList());
            }

            // -------------------------
            // LIST SUPPORT
            // -------------------------
            if (SyntaxRewritterHelper.IsListType(symbol.ReturnType, out var listElementType))
            {
                // Rebuild invocation: reader.ReadList<TType>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // reader.ReadList
                        SyntaxFactory.Identifier("reader.ReadList"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.ParseTypeName(
                                    listElementType.ToDisplayString())))),
                    // () - zero arguments
                    SyntaxFactory.ArgumentList());
            }

            // -------------------------
            // DICTIONARY SUPPORT
            // -------------------------
            if (SyntaxRewritterHelper.IsDictionary(symbol.ReturnType, out var dictKeyType, out var dictValueType))
            {
                // Rebuild invocation: reader.ReadDictionary<TType, TValue>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // reader.ReadDictionary
                        SyntaxFactory.Identifier("reader.ReadDictionary"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                            {
                                SyntaxFactory.ParseTypeName(dictKeyType.ToDisplayString()),
                                SyntaxFactory.ParseTypeName(dictValueType.ToDisplayString())
                            }))),
                    // () - zero arguments
                    SyntaxFactory.ArgumentList());
            }

            // Map field type to reader method
            if (MethodMap.TryGetValue(symbol.ReturnType.SpecialType, out var readerMethod))
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
            else if (symbol.ReturnType.AllInterfaces.Any(i => i.Name == "INetSerializable"))
            {
                // Keep generic type arguments
                var genericArgs = symbol.TypeArguments;
                // Rebuild invocation: reader.Read<T>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // reader.Read
                        SyntaxFactory.Identifier("reader.Read"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                genericArgs.Select(t =>
                                    SyntaxFactory.ParseTypeName(t.ToDisplayString()))))),
                    // () - zero arguments
                    SyntaxFactory.ArgumentList());
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
