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
                Imports: attributeAttachedMethods.Imports.Aggregate(new List<ImportMethodDefinition>(), (prev, m) =>
                {
                    var importAttribute = m.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .First(attr => attr.Name.ToString() == Constants.JSImportAttribute);
                    var argumentsCount = importAttribute.ArgumentList.Arguments.Count;
                    if (argumentsCount == 1)
                    {
                        // NOTE: currently, not supported globalThis.some.module
                        return prev;
                    }
                    var attributeArgs = importAttribute.ArgumentList.Arguments.Select(v => v.Expression).Cast<LiteralExpressionSyntax>().Select(v => v.Token.Value).Cast<string>().ToArray();
                    var args = m.ParameterList.Parameters.Select(p => p.Identifier.ToString()).Select(n => new Argument(Name: n)).ToImmutableArray();

                    return new List<ImportMethodDefinition>(prev)
                    {
                        new ImportMethodDefinition(ModuleName: attributeArgs[1], FunctionName: attributeArgs[0], Arguments: args)
                    };
                }),
                Exports: attributeAttachedMethods.Exports.Select(m =>
                {
                    var fullName = string.Join(".", DigParentName(m).AsEnumerable().Reverse()) + "." + m.Identifier.ToString();
                    var args = m.ParameterList.Parameters.Select(p => p.Identifier.ToString()).Select(n => new Argument(Name: n)).ToImmutableArray();
                    return new ExportMethodDefinition(FullName: fullName, Arguments: args);
                })
            );

            var exportSymbolDict = binds.Exports.Aggregate(new Dictionary<string, dynamic>(), (prev, curr) =>
            {
                return Utils.SplitIntoDictionary(prev, (curr.FullName, curr.Arguments), '.');
            });
            var importSymbolDict = binds.Imports.GroupBy((v) => v.ModuleName).Select(v =>
                new
                {
                    ModuleName = v.Key,
                    Values = v.Aggregate(new Dictionary<string, dynamic>(), (prev, curr) =>
                {
                    return Utils.SplitIntoDictionary(prev, (curr.FunctionName, curr.Arguments), '.');
                }
            )
                });

            var exportJsonString = TypeScriptDefinitionGenerator.GetInstance().Serialize(exportSymbolDict);
            var importModuleWithImpl = importSymbolDict
                .Select(v => new { ModuleName = v.ModuleName, Value = TypeScriptDefinitionGenerator.GetInstance().Serialize(v.Values) });
            var importModuleNames = "type ImportModuleNames = " + string.Join("|", importModuleWithImpl.Select(v => v.ModuleName).Select(v => "'" + v + "'"));
            var importModuleValues = "type ImportModuleValues<T extends ImportModuleNames> = " + string.Join("\n: ", importModuleWithImpl.Select(v => $"T extends '{v.ModuleName}'\n? {v.Value}").Concat(new[] { "never;" }));
            var importImplValue = $@"
export const setTypedModuleImports: <T extends ImportModuleNames>(
    originalSetModuleImports: (moduleName: string, moduleImports: any) => void,
    moduleName: T,
    moduleImports: ImportModuleValues<T>
) => void;";

            var _template_ = $$""""
            /** Generated: for {{asmName}}.dll */
            export const getTypedAssemblyExports: (originalGetAssemblyExports: Promise<any>) => Promise<
            /* AutoGeneratedExportsHelperStart */
            {{exportJsonString}}
            /* AutoGeneratedExportsHelperEnd */
            >;

            """";

            if (importSymbolDict.Count() != 0)
            {
                _template_ += string.Join("\n", new[] {
                    "/* AutoGeneratedImportsHelperStart */",
                    importModuleNames, importModuleValues, importImplValue,
                    "/* AutoGeneratedImportsHelperEnd */"
                     });
            }
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(Constants.BuildPropertyJSPortOverrideTypeDefinitionOutputDir, out var jsPortOverrideTypeDefinitionOutputDir);
            if (string.IsNullOrEmpty(jsPortOverrideTypeDefinitionOutputDir))
            {
                throw new Exception($"`{Constants.JSPortOverrideTypeDefinitionOutputDir}` property must be set, set absolute path.");
            }

            File.WriteAllText(Path.Combine(jsPortOverrideTypeDefinitionOutputDir, $"index.d.ts"), _template_);
            File.WriteAllText(Path.Combine(jsPortOverrideTypeDefinitionOutputDir, $"index.js"),
                $@"export const getTypedAssemblyExports = (originalGetAssemblyExports) => originalGetAssemblyExports;
export const setTypedModuleImports = (originalSetModuleImports, moduleName, moduleImports) => originalSetModuleImports(moduleName, moduleImports);
");
            File.WriteAllText(Path.Combine(jsPortOverrideTypeDefinitionOutputDir, $"package.json"), $$"""{"name":"dotnet-webassembly-type-helper","description":"Generated files","version":"1.0.0","main":"index.js","types":"index.d.ts","private":true,"type":"module"}""");
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
