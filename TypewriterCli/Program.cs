using System;
using System.IO;
using Mono.Options;
using Typewriter.CodeModel.Configuration;
using Typewriter.CodeModel.Implementation;
using Typewriter.Generation;
using Typewriter.Metadata.Roslyn;


namespace TypewriterCli
{
    public static class Program
    {
        static void Main(string[] args)
        {
            //var stopwatch = Stopwatch.StartNew();
            var showHelp = args == null || args.Length == 0;
            
            string templatePath = null;
            string sourcePath = null;

            var p = new OptionSet  {
                { "t|template=", "full path to template (*.tst) file.", v => templatePath =  v },
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
                
                if (templatePath == null)
                    throw new InvalidOperationException("Missing required option -t|template");

                if (sourcePath == null)
                    throw new InvalidOperationException("Missing required option -s|source");

                FileAttributes attr = File.GetAttributes(sourcePath);

                var settings = new SettingsImpl();
                var template = new Template(templatePath);
                var provider = new RoslynMetadataProvider();
//detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    foreach (var path in  Directory.GetFiles(sourcePath))
                    {
                        var file = new FileImpl(provider.GetFile(path, settings, null));
                        template.RenderFile(file);
                    }
                else
                {
                    var file = new FileImpl(provider.GetFile(sourcePath, settings, null));
                    template.RenderFile(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
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