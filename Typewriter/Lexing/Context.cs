using System.Collections.Generic;
using System.Linq;

namespace Typewriter.Lexing
{
    public class Context 
    {
        private readonly Dictionary<string, Identifier> _identifiers = new Dictionary<string, Identifier>();
        private readonly Dictionary<string, Identifier> _extensionIdentifiers = new Dictionary<string, Identifier>();

        public Context(string name, System.Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; private set; }
        public System.Type Type { get; private set; }
        public ICollection<Identifier> Identifiers => _identifiers.Values;

        public void AddIdentifier(Identifier identifier)
        {
            _identifiers.Add(identifier.Name, identifier);
        }

        public void AddExtensionIdentifier(string extensionNamespace, Identifier identifier)
        {
            _extensionIdentifiers.Add(extensionNamespace + "." + identifier.Name, identifier);
        }

        public Identifier GetIdentifier(string name)
        {
            if (name == null) return null;

            Identifier i;
            return _identifiers.TryGetValue(name, out i) ? i : null;
        }

        public Identifier GetExtensionIdentifier(string extensionNamespace, string name)
        {
            if (name == null) return null;

            Identifier i;
            return _extensionIdentifiers.TryGetValue(extensionNamespace + "." + name, out i) ? i : null;
        }

        public IEnumerable<Identifier> GetExtensionIdentifiers(string extensionNamespace)
        {
            return _extensionIdentifiers.Where(i => i.Key.StartsWith(extensionNamespace + ".")).Select(i => i.Value);
        }
    }

    public enum ContextType
    {
        Template,
        CodeBlock,
        Lambda
    }
}