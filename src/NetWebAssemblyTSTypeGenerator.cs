using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetWebAssemblyTSTypeGenerator
{
    [Generator]
    public class NetWebAssemblyTSTypeGenerator : ISourceGenerator
    {
        // TODO: tests
        public void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AssemblyName", out var assemblyName);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
            var asmName = assemblyName ?? rootNamespace;

            var nodes = context.Compilation.SyntaxTrees.SelectMany(v => v.GetRoot().DescendantNodes());
            var classes = nodes
                .Where(n => n.IsKind(SyntaxKind.ClassDeclaration))
                .OfType<ClassDeclarationSyntax>();

            var attributeAttachedMethods = classes
            .SelectMany(c => c.Members.Where(m => HasAttribute(m, Constants.JSExportAttribute) || HasAttribute(m, Constants.JSImportAttribute)))
            .Where(m => m is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .Aggregate((Imports: new List<MethodDeclarationSyntax>(), Exports: new List<MethodDeclarationSyntax>()), (prev, curr) =>
            {
                if (HasAttribute(curr, Constants.JSImportAttribute))
                {
                    return (new List<MethodDeclarationSyntax>(prev.Imports)
                    {
                        curr
                    }, prev.Exports);
                }
                if (HasAttribute(curr, Constants.JSExportAttribute))
                {
                    return (prev.Imports, new List<MethodDeclarationSyntax>(prev.Exports)
                    {
                        curr
                    });
                }

                throw new Exception();
            });

            var binds = new Binds
            (
                // see: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.InteropServices.JavaScript/src/System/Runtime/InteropServices/JavaScript/JSImportAttribute.cs
                Imports: new List<MethodDefinition>(), // TODO: Parse JSImportAttribute... functionName and moduleName...
                Exports: attributeAttachedMethods.Exports.Select(m =>
                {
                    var fullName = string.Join(".", DigParentName(m).AsEnumerable().Reverse()) + "." + m.Identifier.ToString();
                    var args = m.ParameterList.Parameters.Select(p => p.Identifier.ToString()).Select(n => new Argument(Name: n)).ToImmutableArray();
                    return new MethodDefinition(FullName: fullName, Arguments: args);
                })
            );

            var exportSymbolDict = binds.Exports.Aggregate(new Dictionary<string, dynamic>(), (prev, curr) =>
            {
                return Utils.SplitIntoDictionary(prev, (curr.FullName, curr.Arguments), '.');
            });

            var jsonString = TypeScriptDefinitionGenerator.GetInstance().Serialize(exportSymbolDict);

            var _template_ = $$""""
            import "@microsoft/dotnet-runtime"

            declare module "@microsoft/dotnet-runtime" {
                // Use this Type for typing {import("@microsoft/dotnet-runtime").APIType["getAssemblyExports"]}
                export type ExportsHelper = (assemblyName: "{{asmName + ".dll"}}") => Promise<
                        {{jsonString}}
                >;
            }
            """";
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.JSPortOverrideTypeDefinitionOutputDir", out var jsPortOverrideTypeDefinitionOutputDir);
            File.WriteAllText(Path.Combine(jsPortOverrideTypeDefinitionOutputDir, $"dotnet.{asmName}.override.d.ts"), _template_);
        }

        private IList<string> DigParentName(SyntaxNode decl, List<string> parentNames = null)
        {
            if (parentNames == null)
            {
                parentNames = new List<string>();
            }

            if (decl == null)
            {
                return parentNames;
            }

            var next = new List<string>(parentNames);
            switch (decl)
            {
                case ClassDeclarationSyntax classDecl:
                    next.Add(classDecl.Identifier.ToString());
                    break;
                case NamespaceDeclarationSyntax namespaceDecl:
                    next.Add(namespaceDecl.Name.ToString());
                    break;
                default:
                    break;
            }
            return DigParentName(decl.Parent, next);
        }

        private bool HasAttribute(MemberDeclarationSyntax node, string attributeName)
        {
            return node.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .Any(attr => attr.Name.ToString() == attributeName);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}