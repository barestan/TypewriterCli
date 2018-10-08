using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynParameterMetadata : IParameterMetadata
    {
        private readonly IParameterSymbol _symbol;

        private RoslynParameterMetadata(IParameterSymbol symbol)
        {
            _symbol = symbol;
        }

        public string Name => _symbol.Name;
        public string FullName => _symbol.ToDisplayString();
        public bool HasDefaultValue => _symbol.HasExplicitDefaultValue;
        public string DefaultValue => GetDefaultValue();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.Type);

        private string GetDefaultValue()
        {
            if (_symbol.HasExplicitDefaultValue == false)
                return null;

            if (_symbol.ExplicitDefaultValue == null)
                return "null";

            var stringValue = _symbol.ExplicitDefaultValue as string;
            if (stringValue != null)
                return $"\"{stringValue.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

            if(_symbol.ExplicitDefaultValue is bool)
                return (bool)_symbol.ExplicitDefaultValue ? "true" : "false";

            return _symbol.ExplicitDefaultValue.ToString();
        }

        public static IEnumerable<IParameterMetadata> FromParameterSymbols(IEnumerable<IParameterSymbol> symbols)
        {
            return symbols.Select(s => new RoslynParameterMetadata(s));
        }
    }
}