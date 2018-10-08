using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynMethodMetadata : IMethodMetadata
    {
        private IMethodSymbol _symbol;

        public RoslynMethodMetadata(IMethodSymbol symbol)
        {
            _symbol = symbol;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.GetFullName();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.ReturnType);
        public bool IsAbstract => _symbol.IsAbstract;
        public bool IsGeneric => _symbol.IsGenericMethod;
        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);
        public IEnumerable<IParameterMetadata> Parameters => RoslynParameterMetadata.FromParameterSymbols(_symbol.Parameters);

        public static IEnumerable<IMethodMetadata> FromMethodSymbols(IEnumerable<IMethodSymbol> symbols)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public && s.MethodKind == MethodKind.Ordinary && s.IsStatic == false).Select(p => new RoslynMethodMetadata(p));
        }
    }
}