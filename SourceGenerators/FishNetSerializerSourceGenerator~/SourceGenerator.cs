using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FishNetSerializerSourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Helpers.IsBuildTime)
                return;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!Helpers.IsBuildTime)
                return;

            Helpers.SetupContext(context);
            var diagnostic = new DiagnosticReporter(context);
            diagnostic.LogInfo($"Begin Processing assembly {context.Compilation.AssemblyName}");
            try
            {
                var codes = GenerateCodes(context.Compilation, diagnostic);
                AddGeneratedSources(context, codes);
            }
            catch (Exception e)
            {
                diagnostic.LogException(e);
            }
            diagnostic.LogInfo($"End Processing assembly {context.Compilation.AssemblyName}.");
        }

        private static void AddGeneratedSources(GeneratorExecutionContext context, string codes)
        {
            if (string.IsNullOrWhiteSpace(codes))
                return;
            
            context.CancellationToken.ThrowIfCancellationRequested();
            string fileName = $"{Helpers.ClassName}.{context.Compilation.AssemblyName}.generated.cs";
            var sourceText = SourceText.From(codes, Encoding.UTF8);
            var sourcePath = Path.Combine(Helpers.GetOutputPath(), fileName);
            Debug.LogInfo($"output {fileName} to {sourcePath}");
            try
            {
                if (Helpers.CanWriteFiles)
                    File.WriteAllText(sourcePath, sourceText.ToString());
            }
            catch (Exception e)
            {
                //In the rare event/occasion when this happen, at the very least don't bother the user and move forward
                Debug.LogWarning($"cannot write file {Path.Combine(Helpers.GetOutputPath(), sourcePath)}. An exception has been thrown:{e}");
            }
            context.AddSource(fileName, sourceText);
        }

        private static string GenerateCodes(Compilation compilation, DiagnosticReporter reporter)
        {
            var interfaceSymbol = compilation.GetTypeByMetadataName("LiteNetLib.Utils.INetSerializable");
            if (interfaceSymbol == null)
                return null;
            var sb = new StringBuilder();
            sb.AppendLine("// This one is auto-generated");
            sb.AppendLine($"// From assembly: {compilation.AssemblyName}");
            if (interfaceSymbol == null)
                return sb.ToString();

            sb.AppendLine($@"
using FishNet.Serializing;
using FishNet.Insthync.LiteNetLibSerializing;

namespace {Helpers.Namespace}
{{
    public static partial class {Helpers.ClassName}
    {{");
            bool functionsGenerated = false;
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);

                var nodes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<TypeDeclarationSyntax>();

                foreach (var node in nodes)
                {
                    var symbol = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                    if (symbol == null)
                        continue;

                    if (!symbol.AllInterfaces.Contains(interfaceSymbol))
                        continue;

                    // ===== WRITE =====
                    var serializeMethod = symbol.GetMembers("Serialize")
                        .OfType<IMethodSymbol>()
                        .FirstOrDefault();

                    var serializeSyntax = serializeMethod?
                        .DeclaringSyntaxReferences.FirstOrDefault()?
                        .GetSyntax() as MethodDeclarationSyntax;

                    if (serializeSyntax?.Body != null)
                    {
                        sb.Append($@"
        public static void Write{symbol.Name}(this Writer writer, {symbol.Name} data)
        {{");
                        var rewriterS = new WriterSyntaxRewriter(model, reporter);
                        foreach (var stmt in serializeSyntax.Body.Statements)
                        {
                            var rewritten = rewriterS.Visit(stmt);
                            sb.Append($@"
            {rewritten.ToFullString().Trim()}");
                        }

                        sb.AppendLine($@"
        }}");
                        functionsGenerated = true;
                    }

                    // ===== READ =====
                    var deserializeMethod = symbol.GetMembers("Deserialize")
                        .OfType<IMethodSymbol>()
                        .FirstOrDefault();

                    var deserializeSyntax = deserializeMethod?
                        .DeclaringSyntaxReferences.FirstOrDefault()?
                        .GetSyntax() as MethodDeclarationSyntax;

                    if (deserializeSyntax?.Body != null)
                    {
                        sb.Append($@"
        public static {symbol.Name} Read{symbol.Name}(this Reader reader)
        {{
            {symbol.Name} data = new {symbol.Name}();");
                        var rewriterD = new ReaderSyntaxRewriter(model, reporter);
                        foreach (var stmt in deserializeSyntax.Body.Statements)
                        {
                            var rewritten = rewriterD.Visit(stmt);
                            sb.Append($@"
            {rewritten.ToFullString().Trim()}");
                        }

                        sb.AppendLine($@"
            return data;
        }}");
                        functionsGenerated = true;
                    }
                }
            }

            if (!functionsGenerated)
                return null;

            sb.Append($@"
    }}
}}");
            return sb.ToString();
        }
    }
}
