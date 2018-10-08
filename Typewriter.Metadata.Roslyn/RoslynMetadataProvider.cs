using System;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;
using Typewriter.Metadata.Providers;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynMetadataProvider : IMetadataProvider
    {
        public IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender)
        {
            /*var document = workspace.CurrentSolution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
            if (document != null)
            {
                return new RoslynFileMetadata(workspace.CurrentSolution.GetDocument(document), settings, requestRender);
            }

            return null;*/

            if (!System.IO.File.Exists(path)) return null;
            return GetFile(path, settings);
        }

        private static IFileMetadata GetFile(string path, Settings settings)
        {
            return new RoslynFileMetadata(path, settings, null);
        }
    }
}
