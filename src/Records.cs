using System.Collections.Generic;

namespace NetWebAssemblyTSTypeGenerator
{
    internal sealed record Binds(IEnumerable<ImportMethodDefinition> Imports, IEnumerable<ExportMethodDefinition> Exports);
    internal sealed record ExportMethodDefinition(string FullName, IEnumerable<Argument> Arguments /* TODO: ReturnType */);
    internal sealed record ImportMethodDefinition(string ModuleName, string FunctionName, IEnumerable<Argument> Arguments /* TODO: ReturnType */);
    internal sealed record Argument(string Name /* TODO: Type */);
}
