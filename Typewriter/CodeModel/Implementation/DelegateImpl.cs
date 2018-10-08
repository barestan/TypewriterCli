using System.Collections.Generic;
using System.Linq;
using Typewriter.CodeModel.Collections;
using Typewriter.Metadata.Interfaces;
using static Typewriter.CodeModel.Helpers;

namespace Typewriter.CodeModel.Implementation
{
    public sealed class DelegateImpl : Delegate
    {
        private readonly IDelegateMetadata _metadata;

        private DelegateImpl(IDelegateMetadata metadata, Item parent)
        {
            _metadata = metadata;
            Parent = parent;
        }

        public override Item Parent { get; }

        public override string name => CamelCase(_metadata.Name.TrimStart('@'));
        public override string Name => _metadata.Name.TrimStart('@');
        public override string FullName => _metadata.FullName;
        public override bool IsGeneric => _metadata.IsGeneric;

        private IAttributeCollection _attributes;
        public override IAttributeCollection Attributes => _attributes ?? (_attributes = AttributeImpl.FromMetadata(_metadata.Attributes, this));

        private DocComment _docComment;
        public override DocComment DocComment => _docComment ?? (_docComment = DocCommentImpl.FromXml(_metadata.DocComment, this));

        private ITypeParameterCollection _typeParameters;
        public override ITypeParameterCollection TypeParameters => _typeParameters ?? (_typeParameters = TypeParameterImpl.FromMetadata(_metadata.TypeParameters, this));

        private IParameterCollection _parameters;
        public override IParameterCollection Parameters => _parameters ?? (_parameters = ParameterImpl.FromMetadata(_metadata.Parameters, this));

        private Type _type;
        public override Type Type => _type ?? (_type = TypeImpl.FromMetadata(_metadata.Type, this));

        public override string ToString()
        {
            return Name;
        }

        public static IDelegateCollection FromMetadata(IEnumerable<IDelegateMetadata> metadata, Item parent)
        {
            return new DelegateCollectionImpl(metadata.Select(d => new DelegateImpl(d, parent)));
        }
    }
}