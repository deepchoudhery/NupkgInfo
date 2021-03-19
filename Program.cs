using System;
using System.IO;
using System.IO.Compression;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Process = System.Diagnostics.Process;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NupkgInfo
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = NupkgInfo();
            rootCommand.Handler = CommandHandler.Create<ToolOptions>(HandleNupkgInfo);
            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.UseDefaults();
            var parser = commandLineBuilder.Build();
            return await parser.InvokeAsync(args);
        }

        private static RootCommand NupkgInfo()=>
            new RootCommand(
                description: "Gives nupkg info including folder/file tree, size, etc.")
            {
                PathOption(), FileOption()
            };

        private static Option PathOption()=>
            new Option<string>(
                aliases: new [] {"-p", "--path"},
                description: "Path to the nupkg")
            {
                IsRequired = true
            };

        private static Option FileOption()=>
            new Option<bool>(
                aliases: new [] {"-f", "--file"},
                description: "Includes files in tree")
            {
                IsRequired = false
            };

        private static void HandleNupkgInfo(ToolOptions toolOptions)
        {
            FileInfo fi = new FileInfo(toolOptions.Path);  
            string nugetPackageName = fi.Name;
            long size = fi.Length;
            Console.WriteLine($"Nuget package name - {nugetPackageName}");
            Console.WriteLine($"Nuget package size (bytes) - {size}");

            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string fileOption = toolOptions.File ? "/f" : string.Empty;
            Directory.CreateDirectory(tempDirectory);
            ZipFile.ExtractToDirectory(toolOptions.Path, tempDirectory);
            
            //Process to call 'tree' for directory tree.
            Process treeProcess = new Process();
            treeProcess.StartInfo.FileName = "cmd.exe";
            treeProcess.StartInfo.Arguments = @"/c " + $"tree {tempDirectory} {fileOption}";
            treeProcess.StartInfo.RedirectStandardInput = false;
            treeProcess.StartInfo.RedirectStandardOutput = true;
            treeProcess.StartInfo.RedirectStandardError = true;
            treeProcess.StartInfo.UseShellExecute = false;
            treeProcess.Start();

            var output = new List<string>();
            while (!treeProcess.StandardOutput.EndOfStream)
            {
                output.Add(treeProcess.StandardOutput.ReadLine());
            }
            while (treeProcess.StandardError.Peek() > -1)
            {
                output.Add(treeProcess.StandardError.ReadLine());
            }

            //print all output
            foreach (var line in output)
            {
                Console.WriteLine(line);
            }

            treeProcess.WaitForExit();

            //clean temp directory
            Console.WriteLine($"\nDeleting temp directory {tempDirectory}");
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
