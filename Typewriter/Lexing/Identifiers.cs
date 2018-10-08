using System.Collections.Generic;
using System.Linq;

namespace Typewriter.Lexing
{
    public class Identifiers
    {
        private readonly Dictionary<Context, Identifier[]> _identifiers = new Dictionary<Context, Identifier[]>();
        
        public IEnumerable<Identifier> GetTempIdentifiers(Context context)
        {
            return _identifiers.ContainsKey(context) ? _identifiers[context] : new Identifier[0];
        }
        
        public void Add(IEnumerable<TemporaryIdentifier> temporaryIdentifiers)
        {
            foreach (var identifier in temporaryIdentifiers.GroupBy(t => t.Context))
            {
                _identifiers.Add(identifier.Key, identifier.Select(grouping => grouping.Identifier).ToArray());
            }
        }
    }
}