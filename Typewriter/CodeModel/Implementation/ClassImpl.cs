using System.Collections.Generic;
using System.Linq;
using Typewriter.CodeModel.Collections;
using Typewriter.Metadata.Interfaces;
using static Typewriter.CodeModel.Helpers;

namespace Typewriter.CodeModel.Implementation
{
    public sealed class ClassImpl : Class
    {
        private readonly IClassMetadata _metadata;

        private ClassImpl(IClassMetadata metadata, Item parent)
        {
            _metadata = metadata;
            Parent = parent;
        }

        public override Item Parent { get; }

        public override string name => CamelCase(_metadata.Name.TrimStart('@'));
        public override string Name => _metadata.Name.TrimStart('@');
        public override string FullName => _metadata.FullName;
        public override string Namespace => _metadata.Namespace;
        public override bool IsAbstract => _metadata.IsAbstract;
        public override bool IsGeneric => _metadata.IsGeneric;

        private Type _type;
        protected override Type Type => _type ?? (_type = TypeImpl.FromMetadata(_metadata.Type, Parent));
        
        private IAttributeCollection _attributes;
        public override IAttributeCollection Attributes => _attributes ?? (_attributes = AttributeImpl.FromMetadata(_metadata.Attributes, this));

        private IConstantCollection _constants;
        public override IConstantCollection Constants => _constants ?? (_constants = ConstantImpl.FromMetadata(_metadata.Constants, this));

        private IDelegateCollection _delegates;
        public override IDelegateCollection Delegates => _delegates ?? (_delegates = DelegateImpl.FromMetadata(_metadata.Delegates, this));

        private DocComment _docComment;
        public override DocComment DocComment => _docComment ?? (_docComment = DocCommentImpl.FromXml(_metadata.DocComment, this));

        private IEventCollection _events;
        public override IEventCollection Events => _events ?? (_events = EventImpl.FromMetadata(_metadata.Events, this));

        private IFieldCollection _fields;
        public override IFieldCollection Fields => _fields ?? (_fields = FieldImpl.FromMetadata(_metadata.Fields, this));

        private Class _baseClass;
        public override Class BaseClass => _baseClass ?? (_baseClass = FromMetadata(_metadata.BaseClass, this));

        private Class _containingClass;
        public override Class ContainingClass => _containingClass ?? (_containingClass = FromMetadata(_metadata.ContainingClass, this));

        private INterfaceCollection _interfaces;
        public override INterfaceCollection Interfaces => _interfaces ?? (_interfaces = InterfaceImpl.FromMetadata(_metadata.Interfaces, this));

        private IMethodCollection _methods;
        public override IMethodCollection Methods => _methods ?? (_methods = MethodImpl.FromMetadata(_metadata.Methods, this));

        private IPropertyCollection _properties;
        public override IPropertyCollection Properties => _properties ?? (_properties = PropertyImpl.FromMetadata(_metadata.Properties, this));

        private ITypeParameterCollection _typeParameters;
        public override ITypeParameterCollection TypeParameters => _typeParameters ?? (_typeParameters = TypeParameterImpl.FromMetadata(_metadata.TypeParameters, this));

        private ITypeCollection _typeArguments;
        public override ITypeCollection TypeArguments => _typeArguments ?? (_typeArguments = TypeImpl.FromMetadata(_metadata.TypeArguments, this));

        private IClassCollection _nestedClasses;
        public override IClassCollection NestedClasses => _nestedClasses ?? (_nestedClasses = FromMetadata(_metadata.NestedClasses, this));

        private IEnumCollection _nestedEnums;
        public override IEnumCollection NestedEnums => _nestedEnums ?? (_nestedEnums = EnumImpl.FromMetadata(_metadata.NestedEnums, this));

        private INterfaceCollection _nestedInterfaces;
        public override INterfaceCollection NestedInterfaces => _nestedInterfaces ?? (_nestedInterfaces = InterfaceImpl.FromMetadata(_metadata.NestedInterfaces, this));

        public override string ToString()
        {
            return Name;
        }

        public static IClassCollection FromMetadata(IEnumerable<IClassMetadata> metadata, Item parent)
        {
            return new ClassCollectionImpl(metadata.Select(c => new ClassImpl(c, parent)));
        }

        public static Class FromMetadata(IClassMetadata metadata, Item parent)
        {
            return metadata == null ? null : new ClassImpl(metadata, parent);
        }
    }
}
