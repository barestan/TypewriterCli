using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynDelegateMetadata : IDelegateMetadata
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly IMethodSymbol _methodSymbol;

        public RoslynDelegateMetadata(INamedTypeSymbol symbol)
        {
            _symbol = symbol;
            _methodSymbol = symbol.DelegateInvokeMethod;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.GetFullName();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public ITypeMetadata Type => _methodSymbol == null ? null : RoslynTypeMetadata.FromTypeSymbol(_methodSymbol.ReturnType);
        public bool IsAbstract => false;
        public bool IsGeneric => _symbol.TypeParameters.Any();
        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);
        public IEnumerable<IParameterMetadata> Parameters => _methodSymbol == null ? new IParameterMetadata[0] : RoslynParameterMetadata.FromParameterSymbols(_methodSymbol.Parameters);

        public static IEnumerable<IDelegateMetadata> FromNamedTypeSymbols(IEnumerable<INamedTypeSymbol> symbols)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public).Select(s => new RoslynDelegateMetadata(s));
        }
    }
}