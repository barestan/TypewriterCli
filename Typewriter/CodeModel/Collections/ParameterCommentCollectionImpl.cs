using System.Collections.Generic;

namespace Typewriter.CodeModel.Collections
{
    public class ParameterCommentCollectionImpl : ItemCollectionImpl<ParameterComment>, IParameterCommentCollection
    {
        public ParameterCommentCollectionImpl(IEnumerable<ParameterComment> values) : base(values)
        {
        }

        protected override IEnumerable<string> GetItemFilter(ParameterComment item)
        {
            yield return item.Name;
        }
    }
}