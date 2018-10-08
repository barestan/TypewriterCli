using System.Collections.Generic;
using System.Linq;
using Typewriter.CodeModel.Collections;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.CodeModel.Implementation
{
    public class AttributeArgumentImpl : AttributeArgument
    {
        private IAttributeArgumentMetadata _metadata;
        private readonly Item _parent;

        public AttributeArgumentImpl(IAttributeArgumentMetadata metadata, Item parent)
        {
            _metadata = metadata;
            _parent = parent;
        }
        public override Type Type => TypeImpl.FromMetadata(_metadata.Type, _parent);

        public override Type TypeValue => TypeImpl.FromMetadata(_metadata.TypeValue, _parent);

        public override object Value => _metadata.Value;

        public static IAttributeArgumentCollection FromMetadata(IEnumerable<IAttributeArgumentMetadata> metadata, Item parent)
        {
            return new AttributeArgumentCollectionImpl(metadata.Select(a => new AttributeArgumentImpl(a, parent)));
        }
    }
}
