using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Typewriter.Lexing.Roslyn
{
    internal class XmlDocumentationProvider : DocumentationProvider
    {
        private static readonly ConcurrentDictionary<string, XmlDocumentationProvider> DocumentationProviders = new ConcurrentDictionary<string, XmlDocumentationProvider>();

        public static XmlDocumentationProvider GetDocumentationProvider(Assembly assembly)
        {
            var path = Path.ChangeExtension(assembly.Location, "xml");
            return DocumentationProviders.GetOrAdd(path, p => new XmlDocumentationProvider(p));
        }

        private readonly string _filePath;
        private readonly Lazy<Dictionary<string, string>> _docComments;
        
        public XmlDocumentationProvider(string filePath)
        {
            _filePath = filePath;
            _docComments = new Lazy<Dictionary<string, string>>(CreateDocComments, true);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XmlDocumentationProvider;
            return other != null && _filePath == other._filePath;
        }

        public override int GetHashCode()
        {
            return _filePath.GetHashCode();
        }

        protected override string GetDocumentationForSymbol(string documentationMemberId, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
        {
            string docComment;
            return _docComments.Value.TryGetValue(documentationMemberId, out docComment) ? docComment : "";
        }

        public string GetDocumentationForSymbol(string documentationMemberId)
        {
            string docComment;
            return _docComments.Value.TryGetValue(documentationMemberId, out docComment) ? docComment : "";
        }

        private Dictionary<string, string> CreateDocComments()
        {
            var commentsDictionary = new Dictionary<string, string>();
            try
            {
                var foundPath = GetDocumentationFilePath(_filePath);
                if (!string.IsNullOrEmpty(foundPath))
                {
                    var document = XDocument.Load(foundPath);

                    foreach (var element in document.Descendants("member"))
                    {
                        if (element.Attribute("name") != null)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            commentsDictionary[element.Attribute("name").Value] = string.Concat(element.Nodes());
                        }
                    }
                }
            }
            catch
            {
                //ignored
            }
            return commentsDictionary;
        }

        private static string GetDocumentationFilePath(string path)
        {
            if (File.Exists(path)) return path;

            var fileName = Path.GetFileName(path);
            if (fileName == null) return null;

            path = Path.Combine(Constants.ResourcesDirectory, fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(Constants.ReferenceAssembliesDirectory, @"v4.5.2", fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(Constants.ReferenceAssembliesDirectory, @"v4.5.1", fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(Constants.ReferenceAssembliesDirectory, @"v4.5", fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(Constants.ReferenceAssembliesDirectory, @"v4.6", fileName);
            if (File.Exists(path)) return path;

            return null;
        }
    }
}
