using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEventMetadata : IEventMetadata
    {
        private readonly IEventSymbol _symbol;

        public RoslynEventMetadata(IEventSymbol symbol)
        {
            _symbol = symbol;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.ToDisplayString();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.Type);

        public static IEnumerable<IEventMetadata> FromEventSymbols(IEnumerable<IEventSymbol> symbols)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public && s.IsStatic == false).Select(s => new RoslynEventMetadata(s));
        }
    }
}
