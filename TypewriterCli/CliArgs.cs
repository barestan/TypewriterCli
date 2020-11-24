using System;

namespace TypewriterCli
{
    public class CliArgs
    {
        private string templatePath;
        private string sourcePath;
        private readonly bool _generateIndex;

        public CliArgs(string templatePath, string sourcePath, bool generateIndex)
        {
            if (string.IsNullOrEmpty(templatePath))
                throw new ArgumentNullException(nameof(templatePath));

            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));

            this.templatePath = templatePath;
            this.sourcePath = sourcePath;
            _generateIndex = generateIndex;
        }

        public bool GenerateIndex => _generateIndex;

        public string TemplatePath => templatePath;

        public string SourcePath => sourcePath;

        public bool Recursive { get; set; }

        public string Regex { get; set; }
    }
}