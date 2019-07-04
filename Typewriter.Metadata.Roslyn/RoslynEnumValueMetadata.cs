using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEnumValueMetadata : IEnumValueMetadata
    {
        private static readonly Int64Converter Converter = new Int64Converter();

        private readonly IFieldSymbol _symbol;
        
        private Regex CONSTANT_VALUE_SYNTAX  = new Regex(@"\w+\s*=\s*(\d+)");

        private RoslynEnumValueMetadata(IFieldSymbol symbol)
        {
            _symbol = symbol;
        }

        public string DocComment => _symbol.GetDocumentationCommentXml();
        public string Name => _symbol.Name;
        public string FullName => _symbol.ToDisplayString();
        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes());
        public long Value => (long?)Converter.ConvertFromString( TryGetConstantValue()) ?? -1;

        private string TryGetConstantValue()
        {
            if (_symbol.ConstantValue == null)
            {
                var node = _symbol.GetType().GetProperty("SyntaxNode", typeof(EnumMemberDeclarationSyntax));
                var propertyText = node.GetValue(_symbol) + "";
                if (propertyText != null && CONSTANT_VALUE_SYNTAX.IsMatch(propertyText))
                {
                    var value = CONSTANT_VALUE_SYNTAX.Match(propertyText);
                    return (value.Groups[1] + "").Trim('\'');
                }
            }
            return _symbol.ConstantValue.ToString().Trim('\'');
        }

        internal static IEnumerable<IEnumValueMetadata> FromFieldSymbols(IEnumerable<IFieldSymbol> symbols)
        {
            return symbols.Select(s => new RoslynEnumValueMetadata(s));
        }
    }
}