using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynAttributeMetadata : IAttributeMetadata
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly string _name;
        private readonly string _value;

        private RoslynAttributeMetadata(AttributeData a)
        {
            var declaration = a.ToString();
            var index = declaration.IndexOf("(", StringComparison.Ordinal);

            _symbol = a.AttributeClass;
            _name = _symbol.Name;

            if (index > -1)
            {
                _value = declaration.Substring(index + 1, declaration.Length - index - 2);

                // Trim {} from params
                if (_value.EndsWith("\"}"))
                {
                    _value = _value.Remove(_value.LastIndexOf("{\"", StringComparison.Ordinal), 1);
                    _value = _value.TrimEnd('}');
                }
                else if (_value.EndsWith("}"))
                {
                    _value = _value.Remove(_value.LastIndexOf("{", StringComparison.Ordinal), 1);
                    _value = _value.TrimEnd('}');
                }
            }

            if (_name.EndsWith("Attribute"))
                _name = _name.Substring(0, _name.Length - 9);

            Arguments = a.ConstructorArguments.Concat(a.NamedArguments.Select(p=>p.Value)).Select(p => new RoslynAttrubuteArgumentMetadata(p));
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _name;
        public string FullName => _symbol.ToDisplayString();
        public string Value => _value;
        public IEnumerable<IAttributeArgumentMetadata> Arguments { get; private set; }

        public static IEnumerable<IAttributeMetadata> FromAttributeData(IEnumerable<AttributeData> attributes)
        {
            return attributes.Select(a => new RoslynAttributeMetadata(a));
        }
    }
}