using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Typewriter.CodeModel;

namespace Typewriter.Lexing.Roslyn
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ShadowClass
    {
        #region Constants

        private const string StartTemplate = @"namespace __Typewriter
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Typewriter.CodeModel;
    using Typewriter.Configuration;
    using Attribute = Typewriter.CodeModel.Attribute;
    using Enum = Typewriter.CodeModel.Enum;
    using Type = Typewriter.CodeModel.Type;
    ";

        private const string ClassTemplate = @"
    public class Template
    {
";

        private const string EndClassTemplate = @"
    }";

        private const string EndTemplate = @"
}
";
        #endregion

        private readonly ShadowWorkspace _workspace;
        private readonly DocumentId _documentId;
        private readonly List<Snippet> _snippets = new List<Snippet>();
        private readonly HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();
        private int _offset;
        private bool _classAdded;
        public string Source { get; }

        public ShadowClass(string source)
        {
            Source = source;
            AddDefaultReferencedAssemblies();
            _workspace = new ShadowWorkspace();
            _documentId = _workspace.AddProjectWithDocument("ShadowClass.cs", "");
        }

        public IEnumerable<Snippet> Snippets => _snippets;

        public IEnumerable<Assembly> ReferencedAssemblies => _referencedAssemblies;

        private void AddDefaultReferencedAssemblies()
        {
            _referencedAssemblies.Add(typeof(Class).Assembly);
        }

        internal void ResetReferencedAssemblies()
        {
            _referencedAssemblies.Clear();
            AddDefaultReferencedAssemblies();
            _workspace.SetMetadataReferences(_documentId, _referencedAssemblies);
        }

        public void AddReference(string pathOrName)
        {
            var asm = pathOrName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                ? Assembly.LoadFile(pathOrName)
                : Assembly.Load(pathOrName);
            if (_referencedAssemblies.Add(asm))
                _workspace.SetMetadataReferences(_documentId, _referencedAssemblies);
        }

        public void AddUsing(string code, int startIndex)
        {
            _snippets.Add(Snippet.Create(SnippetType.Using, code, _offset, startIndex, startIndex + code.Length));
            _offset += code.Length;
        }

        public void AddBlock(string code, int startIndex)
        {
            if (_classAdded == false)
            {
                _snippets.Add(Snippet.Create(SnippetType.Class, ClassTemplate));
                _offset += ClassTemplate.Length;
                _classAdded = true;
            }

            _snippets.Add(Snippet.Create(SnippetType.Code, code, _offset, startIndex, startIndex + code.Length));
            _offset += code.Length;
        }

        public void AddLambda(string code, string type, string name, int startIndex)
        {
            if (_classAdded == false)
            {
                _snippets.Add(Snippet.Create(SnippetType.Class, ClassTemplate));
                _offset += ClassTemplate.Length;
                _classAdded = true;
            }

            var method = $"bool __{startIndex} ({type} {name}) {{ return ";
            var index = code.IndexOf("=>", StringComparison.Ordinal) + 2;
            code = code.Remove(0, index);

            _snippets.Add(Snippet.Create(SnippetType.Class, method));
            _offset += method.Length;

            _snippets.Add(Snippet.Create(SnippetType.Lambda, code, _offset, startIndex, startIndex + code.Length, index));
            _offset += code.Length;

            _snippets.Add(Snippet.Create(SnippetType.Class, ";}"));
            _offset += 2;
        }

        public void Clear()
        {
            _snippets.Clear();
            _snippets.Add(Snippet.Create(SnippetType.Class, StartTemplate));
            _offset = StartTemplate.Length;
            _classAdded = false;

            ResetReferencedAssemblies();
        }

        public void Parse()
        {
            if (_classAdded == false)
            {
                _snippets.Add(Snippet.Create(SnippetType.Class, ClassTemplate));
                _offset += ClassTemplate.Length;
                _classAdded = true;
            }

            _snippets.Add(Snippet.Create(SnippetType.Class, EndClassTemplate));
            _snippets.Add(Snippet.Create(SnippetType.Class, EndTemplate));

            var code = string.Join(string.Empty, _snippets.Select(s => s.Code));
            _workspace.UpdateText(_documentId, code);
        }

        public IEnumerable<Token> GetTokens()
        {
            var tokens = _snippets.Where(s => s.Type != SnippetType.Class).SelectMany(s =>
            {
                var classifiedSpans = _workspace.GetClassifiedSpans(_documentId, s.Offset, s.Length);

                return classifiedSpans.Select(span => new Token
                {
                    Classification = GetClassification(span.ClassificationType),
                    Start = s.FromShadowIndex(span.TextSpan.Start),
                    Length = span.TextSpan.Length
                });
            });

            return tokens.Where(t => t.Classification != null);
        }

        public IEnumerable<Token> GetErrorTokens()
        {
            var tokens = _snippets.Where(s => s.Type != SnippetType.Class).SelectMany(s =>
            {
                var diagnostics = _workspace.GetDiagnostics(_documentId, s.Offset, s.Length);

                return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning).Select(diagnostic =>
                {
                    var quickInfo = diagnostic.GetMessage();
                    var isError = diagnostic.Severity == DiagnosticSeverity.Error;

                    quickInfo = quickInfo.Replace("__Typewriter.", string.Empty);
                    //quickInfo = quickInfo.Replace("__Code.", string.Empty);

                    return new Token
                    {
                        QuickInfo = quickInfo,
                        IsError = isError,
                        Start = s.FromShadowIndex(diagnostic.Location.SourceSpan.Start),
                        Length = diagnostic.Location.SourceSpan.Length
                    };
                });
            });

            return tokens;
        }

        public IEnumerable<TemporaryIdentifier> GetIdentifiers(Contexts contexts)
        {
            var methods = _workspace.GetMethods(_documentId);

            foreach (var method in methods)
            {
                var returnType = method.ReturnType.ToString();
                var identifier = method.Identifier.ToString();
                var parameter = method.ParameterList.Parameters.FirstOrDefault()?.Type.ToString();

                if (identifier.StartsWith("__") == false && parameter != null)
                {
                    var context = contexts.Find(parameter);
                    if (context != null)
                    {
                        var isBoolean = returnType == "bool" || returnType == "Boolean";

                        var contextType = ExtraxtContextType(returnType);
                        var childContext = contexts.Find(contextType)?.Name;
                        var isCollection = childContext != null && contextType != returnType;

                        yield return new TemporaryIdentifier(context, new Identifier
                        {
                            Name = identifier,
                            QuickInfo = "(extension) " + (isCollection ? contextType + "Collection" : contextType) + " " + identifier,
                            Context = childContext,
                            HasContext = childContext != null,
                            IsBoolean = isBoolean,
                            IsCollection = isCollection,
                            RequireTemplate = isCollection,
                            IsCustom = true
                        });
                    }
                }
            }
        }

        private static string ExtraxtContextType(string returnType)
        {
            if (returnType.EndsWith("[]")) return returnType.Substring(0, returnType.Length - 2);

            var prefixes = new[] { "ICollection<", "IEnumerable<", "List<", "IList<" };
            var match = prefixes.FirstOrDefault(returnType.StartsWith);

            if (match != null)
            {
                var length = match.Length;
                return returnType.Substring(length, returnType.Length - length - 1);
            }

            return returnType;
        }

        public ISymbol GetSymbol(int position)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Contains(position));

            if (snippet == null) return null;

            return _workspace.GetSymbol(_documentId, snippet.ToShadowIndex(position));
        }

        public IEnumerable<ISymbol> GetRecommendedSymbols(int position)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Contains(position));

            if (snippet == null) return new ISymbol[0];

            return _workspace.GetRecommendedSymbols(_documentId, snippet.ToShadowIndex(position)).Where(s => s.Name.StartsWith("__") == false);
        }

        public EmitResult Compile(string outputPath)
        {
            _workspace.ChangeAllMethodsToPublicStatic(_documentId);
            return _workspace.Compile(_documentId, outputPath);
        }

        private static string GetClassification(string classificationType)
        {
            switch (classificationType)
            {
                case "keyword":
                    return Classifications.Keyword;

                case "class name":
                    return Classifications.ClassSymbol;

                case "interface name":
                    return Classifications.InterfaceSymbol;

                case "identifier":
                    return Classifications.Identifier;

                case "string":
                    return Classifications.String;

                case "comment":
                    return Classifications.Comment;
            }

            return null;
        }
    }
}