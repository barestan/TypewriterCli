using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynFileMetadata : IFileMetadata
    {
        private readonly string _codePath;

        private readonly Action<string[]> _requestRender;
        //private Document _document;
        private SyntaxNode _root;
        private SemanticModel _semanticModel;

        public RoslynFileMetadata(Document document, Settings settings, Action<string[]> requestRender)
        {
            _requestRender = requestRender;
            _codePath = document.FilePath;
            LoadDocument(document);
            Settings = settings;
        }
        public RoslynFileMetadata(string codePath, Settings settings, Action<string[]> requestRender)
        {
            _codePath = codePath;
            _requestRender = requestRender;
            
            LoadFromFile();
            Settings = settings;
        }
        

        public Settings Settings { get; }
        public string Name => System.IO.Path.GetFileName(_codePath);
        public string FullName => _codePath;

        public IEnumerable<IClassMetadata> Classes => RoslynClassMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<ClassDeclarationSyntax>(), this);
        public IEnumerable<IDelegateMetadata> Delegates => RoslynDelegateMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<DelegateDeclarationSyntax>());
        public IEnumerable<IEnumMetadata> Enums => RoslynEnumMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<EnumDeclarationSyntax>());
        public IEnumerable<IInterfaceMetadata> Interfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<InterfaceDeclarationSyntax>(), this);

        private void LoadDocument(Document document)
        {
            //_document = document;
            _semanticModel = document.GetSemanticModelAsync().Result;
            _root = _semanticModel.SyntaxTree.GetRoot();
        }
        private void LoadFromFile()
        {
            if (!System.IO.File.Exists(_codePath)) 
                throw new Exception($"File not found {_codePath}");
            
            var code = System.IO.File.ReadAllText(_codePath);
            
            var tree = CSharpSyntaxTree.ParseText(code);
            _root = tree.GetRoot();
            
            List<string> usings = new List<string>()
            {
                "System"
            };

            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);
            CSharpCompilation compilation = CSharpCompilation.Create("output",new []{tree} , new List<MetadataReference>(), options);
            _semanticModel = compilation.GetSemanticModel(tree);
        }

        private IEnumerable<INamedTypeSymbol> GetNamespaceChildNodes<T>() where T : SyntaxNode
        {
            var symbols = _root.ChildNodes().OfType<T>().Concat(
                _root.ChildNodes().OfType<NamespaceDeclarationSyntax>().SelectMany(n => n.ChildNodes().OfType<T>()))
                .Select(c => _semanticModel.GetDeclaredSymbol(c) as INamedTypeSymbol);

            if (Settings.PartialRenderingMode == PartialRenderingMode.Combined)
            {
                return symbols.Where(s =>
                {
                    var locationToRender = s?.Locations.Select(l => l.SourceTree.FilePath).OrderBy(f => f).FirstOrDefault();
                    if (string.Equals(locationToRender, FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        if (locationToRender != null)
                            _requestRender?.Invoke(new[] { locationToRender });

                        return false;
                    }
                }).ToList();
            }

            return symbols;
        }
    }
}
