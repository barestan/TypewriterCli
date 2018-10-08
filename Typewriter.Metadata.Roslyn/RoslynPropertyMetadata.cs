using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynPropertyMetadata : IPropertyMetadata
    {
        private readonly IPropertySymbol _symbol;

        private RoslynPropertyMetadata(IPropertySymbol symbol)
        {
            _symbol = symbol;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.ToDisplayString();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.Type);
        public bool IsAbstract => _symbol.IsAbstract;
        public bool HasGetter => _symbol.GetMethod != null && _symbol.GetMethod.DeclaredAccessibility == Accessibility.Public;
        public bool HasSetter => _symbol.SetMethod != null && _symbol.SetMethod.DeclaredAccessibility == Accessibility.Public;
        
        public static IEnumerable<IPropertyMetadata> FromPropertySymbol(IEnumerable<IPropertySymbol> symbols)
        {
            return symbols.Where(p => p.DeclaredAccessibility == Accessibility.Public && p.IsStatic == false).Select(p => new RoslynPropertyMetadata(p));
        }
    }
}