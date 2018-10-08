using System.Collections.Generic;
using System.Linq;
using Typewriter.CodeModel.Collections;
using Typewriter.Metadata.Interfaces;
using static Typewriter.CodeModel.Helpers;

namespace Typewriter.CodeModel.Implementation
{
    public sealed class InterfaceImpl : Interface
    {
        private readonly IInterfaceMetadata _metadata;

        private InterfaceImpl(IInterfaceMetadata metadata, Item parent)
        {
            _metadata = metadata;
            Parent = parent;
        }

        public override Item Parent { get; }

        public override string name => CamelCase(_metadata.Name.TrimStart('@'));
        public override string Name => _metadata.Name.TrimStart('@');
        public override string FullName => _metadata.FullName;
        public override string Namespace => _metadata.Namespace;
        public override bool IsGeneric => _metadata.IsGeneric;

        private Type _type;
        protected override Type Type => _type ?? (_type = TypeImpl.FromMetadata(_metadata.Type, Parent));

        private IAttributeCollection _attributes;
        public override IAttributeCollection Attributes => _attributes ?? (_attributes = AttributeImpl.FromMetadata(_metadata.Attributes, this));

        private DocComment _docComment;
        public override DocComment DocComment => _docComment ?? (_docComment = DocCommentImpl.FromXml(_metadata.DocComment, this));

        private IEventCollection _events;
        public override IEventCollection Events => _events ?? (_events = EventImpl.FromMetadata(_metadata.Events, this));

        private INterfaceCollection _interfaces;
        public override INterfaceCollection Interfaces => _interfaces ?? (_interfaces = FromMetadata(_metadata.Interfaces, this));

        private IMethodCollection _methods;
        public override IMethodCollection Methods => _methods ?? (_methods = MethodImpl.FromMetadata(_metadata.Methods, this));

        private IPropertyCollection _properties;
        public override IPropertyCollection Properties => _properties ?? (_properties = PropertyImpl.FromMetadata(_metadata.Properties, this));

        private ITypeParameterCollection _typeParameters;
        public override ITypeParameterCollection TypeParameters => _typeParameters ?? (_typeParameters = TypeParameterImpl.FromMetadata(_metadata.TypeParameters, this));

        private ITypeCollection _typeArguments;
        public override ITypeCollection TypeArguments => _typeArguments ?? (_typeArguments = TypeImpl.FromMetadata(_metadata.TypeArguments, this));

        private Class _containingClass;
        public override Class ContainingClass => _containingClass ?? (_containingClass = ClassImpl.FromMetadata(_metadata.ContainingClass, this));

        public override string ToString()
        {
            return Name;
        }

        public static INterfaceCollection FromMetadata(IEnumerable<IInterfaceMetadata> metadata, Item parent)
        {
            return new InterfaceCollectionImpl(metadata.Select(i => new InterfaceImpl(i, parent)));
        }
    }
}