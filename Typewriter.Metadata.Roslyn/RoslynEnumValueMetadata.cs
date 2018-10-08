using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEnumValueMetadata : IEnumValueMetadata
    {
        private static readonly Int64Converter Converter = new Int64Converter();

        private readonly IFieldSymbol _symbol;

        private RoslynEnumValueMetadata(IFieldSymbol symbol)
        {
            _symbol = symbol;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.ToDisplayString();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public long Value => (long?)Converter.ConvertFromString(_symbol.ConstantValue.ToString().Trim('\'')) ?? -1;

        internal static IEnumerable<IEnumValueMetadata> FromFieldSymbols(IEnumerable<IFieldSymbol> symbols)
        {
            return symbols.Select(s => new RoslynEnumValueMetadata(s));
        }
    }
}