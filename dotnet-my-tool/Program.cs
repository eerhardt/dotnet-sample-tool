using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace My.Tool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, bool.TrueString); // always run in verbose
            
            HandleVerboseOption(ref args);
            
            if(!args.Contains("--dispatch"))
            {
                var app = CreateApp();
                return app.Execute(args);
            }
            else
            {
                Reporter.Output.WriteLine("Pass. Called inside the project. :) ");
                return 0;
            }
        }
        
        private static CommandLineApplication CreateApp()
        {
            var app = new CommandLineApplication()
            {
                Name = "dotnet-my-tool"
            };
            
            var buildBasePath = app.Option("-b|--build-base-path", 
                   "Build base path",
                   CommandOptionType.SingleValue);
            
            app.OnExecute(()=> {
                Reporter.Output.WriteLine("Called as a tool. Dispatching to dependency command inside project.");
                var project = ProjectReader.GetProject(Directory.GetCurrentDirectory());
            
                var framework = project.GetTargetFrameworks().Select(f=>f.FrameworkName).First();
                
                var projectContext = new ProjectContextBuilder()
                    .WithProject(project)
                    .WithTargetFramework(framework)
                    .Build();
                    
                var buildIt = Command.CreateDotNet("build",
                    new [] { "--build-base-path", buildBasePath.Value() ?? Directory.GetCurrentDirectory() },
                    framework,
                    Constants.DefaultConfiguration);
                
                return new ProjectDependenciesCommandFactory(
                        projectContext.TargetFramework,
                        Constants.DefaultConfiguration,
                        outputPath: null,
                        buildBasePath: buildBasePath.Value(),
                        projectDirectory: projectContext.ProjectDirectory
                    )
                    .Create("dotnet-my-tool", 
                            new [] {"--dispatch", "--verbose"}, 
                            projectContext.TargetFramework, 
                            Constants.DefaultConfiguration)
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute()
                    .ExitCode;
            });
            
            return app;
        }
        
        private static void HandleVerboseOption(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();

                    return;
                }
            }
        }
    }
}
