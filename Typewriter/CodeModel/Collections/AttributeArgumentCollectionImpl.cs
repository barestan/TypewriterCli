using System.Collections.Generic;

namespace Typewriter.CodeModel.Collections
{
    public class AttributeArgumentCollectionImpl : ItemCollectionImpl<AttributeArgument>, IAttributeArgumentCollection
    {
        public AttributeArgumentCollectionImpl(IEnumerable<AttributeArgument> values) : base(values)
        {
        }
    }
}