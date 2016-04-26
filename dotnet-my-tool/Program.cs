using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using NuGet.Frameworks;

namespace My.Tool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            HandleVerboseOption(ref args);
            
            if(!args.Contains("--dispatch"))
            {
                Reporter.Output.WriteLine("Called as a tool. Dispatching to dependency command inside project.");
                var project = ProjectReader.GetProject(Directory.GetCurrentDirectory());
            
                foreach(var framework in project.GetTargetFrameworks().Select(f=>f.FrameworkName))
                {
                    var projectContext = new ProjectContextBuilder()
                        .WithProject(project)
                        .WithTargetFramework(framework)
                        .Build();
                    
                    try
                    {
                        int result = new ProjectDependenciesCommandFactory(
                                projectContext.TargetFramework,
                                Constants.DefaultConfiguration,
                                null,
                                null,
                                projectContext.ProjectDirectory
                            )
                            .Create("dotnet-my-tool", 
                                    new []{"--dispatch", "--verbose"}, 
                                    projectContext.TargetFramework, 
                                    Constants.DefaultConfiguration)
                            .ForwardStdErr()
                            .ForwardStdOut()
                            .Execute()
                            .ExitCode;
                        if (result == 0)
                        {
                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        Reporter.Output.WriteLine($"Error occurred. Skipping framework '{framework}'");    
                    }
                }
                
                // no TargetFramework worked, error out and tell the user
                Reporter.Output.WriteLine("Failed to find an appropriate target framework in project '{project}'.");
                return 1;
            }
            else
            {
                Reporter.Output.WriteLine("Pass. Called inside the project. :) ");
                return 0;
            }
        }
        
        private static void HandleVerboseOption(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, bool.TrueString);
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();

                    return;
                }
            }
        }
    }
}
