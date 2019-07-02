using Typewriter.Metadata.Interfaces;

namespace Typewriter.CodeModel.Implementation
{
    public sealed class FileImpl : File
    {
        private readonly IFileMetadata _metadata;

        public FileImpl(IFileMetadata metadata)
        {
            _metadata = metadata;
        }

        public override string Name => _metadata.Name;
        public override string FullName => _metadata.FullName;

        private IClassCollection _classes;
        public override IClassCollection Classes => _classes ?? (_classes = ClassImpl.FromMetadata(_metadata.Classes, this));

        private Class _baseClass;
        
        private IDelegateCollection _delegates;
        public override IDelegateCollection Delegates => _delegates ?? (_delegates = DelegateImpl.FromMetadata(_metadata.Delegates, this));

        private IEnumCollection _enums;
        public override IEnumCollection Enums => _enums ?? (_enums = EnumImpl.FromMetadata(_metadata.Enums, this));

        private INterfaceCollection _interfaces;
        public override INterfaceCollection Interfaces => _interfaces ?? (_interfaces = InterfaceImpl.FromMetadata(_metadata.Interfaces, this));

        public override string ToString()
        {
            return Name;
        }
    }
}