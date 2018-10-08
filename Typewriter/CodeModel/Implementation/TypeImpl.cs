using System;
using System.Collections.Generic;
using System.Linq;
using Typewriter.CodeModel.Collections;
using Typewriter.Metadata.Interfaces;
using static Typewriter.CodeModel.Helpers;

namespace Typewriter.CodeModel.Implementation
{
    public sealed class TypeImpl : Type
    {
        private readonly ITypeMetadata _metadata;
        private readonly Lazy<string> _lazyName;
        private readonly Lazy<string> _lazyOriginalName;

        private TypeImpl(ITypeMetadata metadata, Item parent)
        {
            _metadata = metadata;
            Parent = parent;
            _lazyName = new Lazy<string>(() => GetTypeScriptName(metadata));
            _lazyOriginalName = new Lazy<string>(() => GetOriginalName(metadata));
        }

        public override Item Parent { get; }

        public override string name => CamelCase(_lazyName.Value.TrimStart('@'));
        public override string Name => _lazyName.Value.TrimStart('@');
        public override string OriginalName => _lazyOriginalName.Value;
        public override string FullName => _metadata.FullName;
        public override string Namespace => _metadata.Namespace;
        public override bool IsGeneric => _metadata.IsGeneric;
        public override bool IsEnum => _metadata.IsEnum;
        public override bool IsEnumerable => _metadata.IsEnumerable;
        public override bool IsNullable => _metadata.IsNullable;
        public override bool IsTask => _metadata.IsTask;
        public override bool IsPrimitive => IsPrimitive(_metadata);
        public override bool IsDate => Name == "Date";
        public override bool IsDefined => !IsPrimitive || _metadata.IsDefined;
        public override bool IsGuid => FullName == "Guid" || FullName == "Guid?";
        public override bool IsTimeSpan => FullName == "System.TimeSpan" || FullName == "System.TimeSpan?";
        public override bool IsValueTuple => _metadata.IsValueTuple;


        private IAttributeCollection _attributes;
        public override IAttributeCollection Attributes => _attributes ?? (_attributes = AttributeImpl.FromMetadata(_metadata.Attributes, this));

        private DocComment _docComment;
        public override DocComment DocComment => _docComment ?? (_docComment = DocCommentImpl.FromXml(_metadata.DocComment, this));

        private IConstantCollection _constants;
        public override IConstantCollection Constants => _constants ?? (_constants = ConstantImpl.FromMetadata(_metadata.Constants, this));

        private IDelegateCollection _delegates;
        public override IDelegateCollection Delegates => _delegates ?? (_delegates = DelegateImpl.FromMetadata(_metadata.Delegates, this));

        private IFieldCollection _fields;
        public override IFieldCollection Fields => _fields ?? (_fields = FieldImpl.FromMetadata(_metadata.Fields, this));

        private Class _baseClass;
        public override Class BaseClass => _baseClass ?? (_baseClass = ClassImpl.FromMetadata(_metadata.BaseClass, this));

        private Class _containingClass;
        public override Class ContainingClass => _containingClass ?? (_containingClass = ClassImpl.FromMetadata(_metadata.ContainingClass, this));

        private INterfaceCollection _interfaces;
        public override INterfaceCollection Interfaces => _interfaces ?? (_interfaces = InterfaceImpl.FromMetadata(_metadata.Interfaces, this));

        private IMethodCollection _methods;
        public override IMethodCollection Methods => _methods ?? (_methods = MethodImpl.FromMetadata(_metadata.Methods, this));

        private IPropertyCollection _properties;
        public override IPropertyCollection Properties => _properties ?? (_properties = PropertyImpl.FromMetadata(_metadata.Properties, this));

        private ITypeCollection _typeArguments;
        public override ITypeCollection TypeArguments => _typeArguments ?? (_typeArguments = FromMetadata(_metadata.TypeArguments, this));

        private ITypeParameterCollection _typeParameters;
        public override ITypeParameterCollection TypeParameters => _typeParameters ?? (_typeParameters = TypeParameterImpl.FromMetadata(_metadata.TypeParameters, this));

        private IFieldCollection _tupleElements;
        public override IFieldCollection TupleElements => _tupleElements ?? (_tupleElements = FieldImpl.FromMetadata(_metadata.TupleElements, this));

        private IClassCollection _nestedClasses;
        public override IClassCollection NestedClasses => _nestedClasses ?? (_nestedClasses = ClassImpl.FromMetadata(_metadata.NestedClasses, this));

        private IEnumCollection _nestedEnums;
        public override IEnumCollection NestedEnums => _nestedEnums ?? (_nestedEnums = EnumImpl.FromMetadata(_metadata.NestedEnums, this));

        private INterfaceCollection _nestedInterfaces;
        public override INterfaceCollection NestedInterfaces => _nestedInterfaces ?? (_nestedInterfaces = InterfaceImpl.FromMetadata(_metadata.NestedInterfaces, this));
        
        public override string ToString()
        {
            return Name;
        }

        public static ITypeCollection FromMetadata(IEnumerable<ITypeMetadata> metadata, Item parent)
        {
            return new TypeCollectionImpl(metadata.Select(t => new TypeImpl(t, parent)));
        }

        public static Type FromMetadata(ITypeMetadata metadata, Item parent)
        {
            return metadata == null ? null : new TypeImpl(metadata, parent);
        }
    }
}