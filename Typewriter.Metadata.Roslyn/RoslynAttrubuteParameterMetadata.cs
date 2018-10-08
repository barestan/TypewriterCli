using Microsoft.CodeAnalysis;
using System.Linq;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynAttrubuteArgumentMetadata : IAttributeArgumentMetadata
    {
        private TypedConstant _typeConstant;

        public RoslynAttrubuteArgumentMetadata(TypedConstant typeConstant)
        {
            _typeConstant = typeConstant;
        }

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_typeConstant.Type);

        public ITypeMetadata TypeValue => _typeConstant.Kind == TypedConstantKind.Type ? RoslynTypeMetadata.FromTypeSymbol((INamedTypeSymbol)_typeConstant.Value) : null;
        public object Value => _typeConstant.Kind == TypedConstantKind.Array ? _typeConstant.Values.Select(prop => prop.Value).ToArray() : _typeConstant.Value;
    }
}