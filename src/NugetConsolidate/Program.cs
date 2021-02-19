using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CommandLine;
using NugetConsolidate.Service;

namespace NugetConsolidate
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<CommandLineOptionsCheck, CommandLineOptionsFix>(args)
				.MapResult(
					(CommandLineOptionsCheck options) => Check(options),
					(CommandLineOptionsFix options) => Fix(options),
					errors => 1);
		}

		private static int Check(CommandLineOptionsCheck options)
		{
			var consolidateService = CreateConsolidateService(options);
			return consolidateService.ConsolidateTransitiveDependencies(true);
		}

		private static int Fix(CommandLineOptionsFix options)
		{
			var consolidateService = CreateConsolidateService(options);
			if (options.Clean)
			{
				var filesToDelete = Directory.EnumerateFiles(Path.GetDirectoryName(options.SolutionFile),
					PackageReferenceUpdater.PROJECT_FILE_PROPS_FILE_NAME, SearchOption.AllDirectories).ToList();
				ColorConsole.WriteInfo($"Started with clean flag. Deleting {PackageReferenceUpdater.PROJECT_FILE_PROPS_FILE_NAME} files");
				foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(options.SolutionFile), PackageReferenceUpdater.PROJECT_FILE_PROPS_FILE_NAME, SearchOption.AllDirectories))
				{
					if (options.Verbose)
					{
						Console.WriteLine($"Delete {file}");
					}
					File.Delete(file);
				}
				ColorConsole.WriteInfo($"{filesToDelete.Count} files deleted");
			}
			return consolidateService.ConsolidateTransitiveDependencies(false);
		}

		private static ConsolidateService CreateConsolidateService(CommandLineOptions options)
		{
			if (!File.Exists(options.MsBuildPath))
			{
				var runner = new ProcessRunner();
				var result = runner.Run(Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"),
					null,
					new[] { @"-latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe" });
				if (result.IsSuccess)
				{
					options.MsBuildPath = result.Output;
					ColorConsole.WriteInfo($"MS build found at {options.MsBuildPath}");
				}
				else
				{
					ColorConsole.WriteError($"MS build not found {result.Errors}");
					ColorConsole.WriteError($"specify msbuild path manually with -m flag");
				}
			}

			var consolidateService = new ConsolidateService(options,
				new DependencyGraphReader(),
				new DependencyGraphAnalyzer(new LockFileService()),
				new PackageReferenceUpdater());
			return consolidateService;
		}
	}
}
