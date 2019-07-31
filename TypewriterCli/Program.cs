using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Options;
using Typewriter.CodeModel.Configuration;
using Typewriter.CodeModel.Implementation;
using Typewriter.Generation;
using Typewriter.Metadata.Roslyn;


namespace TypewriterCli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //var stopwatch = Stopwatch.StartNew();
            var showHelp = args == null || args.Length == 0;
            
            string templatePath = null;
            string sourcePath = null;
            bool generateIndex = false;

            var p = new OptionSet  {
                { "t|template=", "full path to template (*.tst) file.", v => templatePath =  v },
                { "i|index=", "should generrate index.", v => generateIndex =  !string.IsNullOrEmpty(v) && bool.Parse(v) },
                { "s|source=", "full path to source (*.cs) file.",v => sourcePath =  v },
                { "h|help",  "show this message and exit", v => showHelp = v != null }
            };

            try 
            {
                p.Parse(args);
            }
            catch (OptionException e) {
                Console.Write ("TypewriterCli: ");
                Console.WriteLine (e.Message);
                Console.WriteLine ("Try `dotnet TypewriterCli.dll --help' for more information.");
                return;
            }

            try
            {
                if (showHelp)
                {
                    ShowHelp(p);
                    return;
                }
                
                var cliArgs = new CliArgs(templatePath,sourcePath, generateIndex);
                
                if (cliArgs.TemplatePath == null)
                    throw new InvalidOperationException("Missing required option -t|template");

                if (cliArgs.SourcePath == null)
                    throw new InvalidOperationException("Missing required option -s|source");


                Generate(cliArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        public static void Generate(CliArgs cliArgs)
        {
            var settings = new SettingsImpl();
            var template = new Template(cliArgs.TemplatePath);
            var provider = new RoslynMetadataProvider();
            var indexBuilder = new StringBuilder();
//detect whether its a directory or file
            foreach (var path in GetFiles(cliArgs.SourcePath))
            {
                var file = new FileImpl(provider.GetFile(path, settings, null));
                var outputPath = template.RenderFile(file);
                if (outputPath != null)
                {
                    indexBuilder.Append(ExportStatement(outputPath));
                }
            }

            if (cliArgs.GenerateIndex)
            {
                var @join = Path.Join(Path.GetDirectoryName(cliArgs.TemplatePath), "index.ts");
                Console.WriteLine($"Outputting to {@join}");
                File.WriteAllText(@join, indexBuilder.ToString(), new UTF8Encoding(true));
            }
        }

        private static string[] GetFiles(string sourcePaths)
        {
            List<string> result = new List<string>();
            foreach (var sourcePath in sourcePaths.Split(","))
            {
                FileAttributes attr = File.GetAttributes(sourcePath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    result.AddRange(Directory.GetFiles(sourcePath));
                }
                else
                {
                    result.Add(sourcePath);
                }
             
            }
            return result.ToArray();
        }

        private static string ExportStatement(String outputPath)
        {
            return $"export * from \"./{Path.GetFileName(outputPath).Replace(".ts", "")}\";\n";
        }

        static void ShowHelp (OptionSet p)
        {
            Console.WriteLine ("Usage:  dotnet TypewriterCli.dll [OPTIONS]");
            Console.WriteLine ();
            Console.WriteLine ("TypewriterCli generates TypeScript files from c# code files using TypeScript Templates.");
            Console.WriteLine ("For more information about TypeScript Templates, see here: https://frhagn.github.io/Typewriter/index.html.");
            Console.WriteLine ();
            Console.WriteLine ("Options:");
            p.WriteOptionDescriptions (Console.Out);
        }
    }
}