using System.Collections.Generic;

namespace Typewriter.CodeModel.Collections
{
    public class EnumValueCollectionImpl : ItemCollectionImpl<EnumValue>, IEnumValueCollection
    {
        public EnumValueCollectionImpl(IEnumerable<EnumValue> values) : base(values)
        {
        }

        protected override IEnumerable<string> GetAttributeFilter(EnumValue item)
        {
            foreach (var attribute in item.Attributes)
            {
                yield return attribute.Name;
                yield return attribute.FullName;
            }
        }

        protected override IEnumerable<string> GetItemFilter(EnumValue item)
        {
            yield return item.Name;
            yield return item.FullName;
        }
    }
}