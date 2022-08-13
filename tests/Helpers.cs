using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetWebAssemblyTSTypeGenerator.Tests
{
    internal class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        public static CustomAnalyzerConfigOptions Empty { get; } = new CustomAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);
        private readonly ImmutableDictionary<string, string> _dictImpl;
        public CustomAnalyzerConfigOptions(ImmutableDictionary<string, string> dictImpl)
        {
            _dictImpl = dictImpl;
        }

        // FIXME: Cannot use NotNullWhenAttribute...
        public override bool TryGetValue(string key, out string? value) => _dictImpl.TryGetValue(key, out value);
    }

    internal class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        public CustomAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions)
        {
            GlobalOptions = globalOptions;
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => throw new NotImplementedException();
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => throw new NotImplementedException();
    }
}
