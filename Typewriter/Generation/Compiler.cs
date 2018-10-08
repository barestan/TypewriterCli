using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
//using EnvDTE;
using Microsoft.CodeAnalysis;
using Typewriter.Lexing.Roslyn;

//using Typewriter.VisualStudio;

namespace Typewriter.Generation
{
    internal static class Compiler
    {
        public static Type Compile(ShadowClass shadowClass)
        {
            if (Directory.Exists(Constants.TempDirectory) == false)
            {
                Directory.CreateDirectory(Constants.TempDirectory);
            }

            Assembly asm;
            var cachePath =  Path.Combine(Constants.TempDirectory,Sha256(shadowClass.Source) + ".bin");
            if (!File.Exists(cachePath))
            {
                foreach (Assembly assembly in shadowClass.ReferencedAssemblies)
                {
                    if (assembly.GlobalAssemblyCache) continue;

                    var asmSourcePath = assembly.Location;
                    var asmDestPath = Path.Combine(Constants.TempDirectory, Path.GetFileName(asmSourcePath));
                    try
                    {
                        //File may be in use
                        File.Copy(asmSourcePath, asmDestPath, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Warn: {e}");
                        //Log.Warn(e.ToString());
                    }
                }

                var filname = Path.GetRandomFileName();
                var path = Path.Combine(Constants.TempDirectory, filname);

                var result = shadowClass.Compile(path);

                //ErrorList.Clear();

                var errors = result.Diagnostics.Where(diagnostic =>
                    diagnostic.Severity == DiagnosticSeverity.Error ||
                    diagnostic.Severity == DiagnosticSeverity.Warning);

                foreach (var error in errors)
                {
                    var message = error.GetMessage();

                    message = message.Replace("__Typewriter.", string.Empty);
                    //message = message.Replace("__Code.", string.Empty);
                    message = message.Replace("publicstatic", string.Empty);

                    //Log.Warn("Template error: {0} {1}", error.Id, message);
                    Console.WriteLine("Template error: {0} {1}", error.Id, message);

                    if (error.Severity == DiagnosticSeverity.Error || error.IsWarningAsError)
                    {
                        Console.WriteLine("Error: {0}", message);
                        //ErrorList.AddError(projectItem, message);
                    }
                    else
                    {
                        //ErrorList.AddWarning(projectItem, message);
                        Console.WriteLine($"Warn: {message}");
                    }
                }

//            if (hasErrors)
//                ErrorList.Show();

                if (!result.Success) throw new InvalidOperationException("Template compilation errors.");
                asm = Assembly.LoadFrom(path);
                File.Move(path, cachePath);
            }
            else
            {
                asm = Assembly.Load(File.ReadAllBytes(cachePath));
            }
            var type = asm.GetType("__Typewriter.Template");

            return type;
        }
        static string Sha256(string value)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(value));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }
}