using System.Collections.Generic;

namespace NetWebAssemblyTSTypeGenerator
{
    internal sealed record Binds(IEnumerable<MethodDefinition> Imports, IEnumerable<MethodDefinition> Exports);
    internal sealed record MethodDefinition(string FullName, IEnumerable<Argument> Arguments /* TODO: ReturnType */);
    internal sealed record Argument(string Name /* TODO: Type */);
}