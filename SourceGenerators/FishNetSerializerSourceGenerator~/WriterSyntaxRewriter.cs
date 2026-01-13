using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FishNetSerializerSourceGenerator
{
    public sealed class WriterSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;
        private readonly DiagnosticReporter _reporter;

        // Map from field type -> writer method
        private static readonly Dictionary<SpecialType, string> MethodMap =
            new Dictionary<SpecialType, string>()
            {
            { SpecialType.System_Byte, "WriteUInt8Unpacked" },
            { SpecialType.System_SByte, "WriteInt8Unpacked" },
            { SpecialType.System_Int32, "WriteInt32" },
            { SpecialType.System_UInt32, "WriteUInt32" },
            { SpecialType.System_Int16, "WriteInt16" },
            { SpecialType.System_UInt16, "WriteUInt16" },
            { SpecialType.System_Int64, "WriteInt64" },
            { SpecialType.System_UInt64, "WriteUInt64" },
            { SpecialType.System_Single, "WriteSingle" },
            { SpecialType.System_Double, "WriteDouble" },
            { SpecialType.System_Boolean, "WriteBoolean" },
            { SpecialType.System_Char, "WriteChar" },
            { SpecialType.System_String, "WriteString" },
            };

        public WriterSyntaxRewriter(SemanticModel model, DiagnosticReporter reporter)
        {
            _model = model;
            _reporter = reporter;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (symbol == null)
                return base.VisitInvocationExpression(node);

            ArgumentSyntax argument;
            ITypeSymbol argType = null;
            if (!SyntaxRewritterHelper.IsWriterMethod(_reporter, _model, node, out argument, out argType))
                return base.VisitInvocationExpression(node);

            // Rewrite instance fields -> data.field
            var rewrittenArg = (ExpressionSyntax)Visit(argument.Expression);

            // -------------------------
            // ARRAY SUPPORT
            // -------------------------
            if (argType is IArrayTypeSymbol arrayType)
            {
                // Keep generic type arguments
                var elementType = arrayType.ElementType;
                // Rebuild invocation: writer.WriteArray<T>()
                return SyntaxFactory.InvocationExpression(
                    // writer.WriteArray
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("writer.WriteArray"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.ParseTypeName(
                                    elementType.ToDisplayString())))),
                    // (arg1, arg2, ...)
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(rewrittenArg))));
            }

            // -------------------------
            // LIST SUPPORT
            // -------------------------
            if (SyntaxRewritterHelper.IsListType(argType, out var listElementType))
            {
                // Rebuild invocation: writer.WriteList<TType>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // writer.WriteList
                        SyntaxFactory.Identifier("writer.WriteList"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.ParseTypeName(
                                    listElementType.ToDisplayString())))),
                    // (arg1, arg2, ...)
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(rewrittenArg))));
            }

            // -------------------------
            // DICTIONARY SUPPORT
            // -------------------------
            if (SyntaxRewritterHelper.IsDictionary(argType, out var dictKeyType, out var dictValueType))
            {
                // Rebuild invocation: writer.WriteDictionary<TType, TValue>()
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        // writer.WriteDictionary
                        SyntaxFactory.Identifier("writer.WriteDictionary"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                            {
                                SyntaxFactory.ParseTypeName(dictKeyType.ToDisplayString()),
                                SyntaxFactory.ParseTypeName(dictValueType.ToDisplayString())
                            }))),
                    // (arg1, arg2, ...)
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(rewrittenArg))));
            }

            // Find matching writer method
            if (MethodMap.TryGetValue(argType.SpecialType, out var writerMethod))
            {
                // Rebuild invocation: writer.Write{writerMethod}()
                return SyntaxFactory.InvocationExpression(
                    // writer.Write{writerMethod}
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("writer"),
                        SyntaxFactory.IdentifierName(writerMethod)),
                    // (arg1, arg2, ...)
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(rewrittenArg))));
            }
            else if (argType.AllInterfaces.Any(i => i.Name == "INetSerializable"))
            {
                // Keep generic type arguments
                var genericArgs = symbol.TypeArguments;
                // Rebuild invocation: writer.Write<T>()
                return SyntaxFactory.InvocationExpression(
                    // writer.Write
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("writer.Write"),
                        // <T1, T2, ...>
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                genericArgs.Select(t =>
                                    SyntaxFactory.ParseTypeName(t.ToDisplayString()))))),
                    // (arg1, arg2, ...)
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(rewrittenArg))));
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

            // Rewrite NetDataWriter parameter
            if (symbol is IParameterSymbol param &&
                param.Type.Name == "NetDataWriter")
            {
                return SyntaxFactory.IdentifierName("writer");
            }

            return base.VisitIdentifierName(node);
        }
    }
}
