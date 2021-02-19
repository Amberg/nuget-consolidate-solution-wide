using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace NugetConsolidate
{
	[SuppressMessage("Microsoft.Performance", "CA1812: Avoid uninstantiated internal classes",
		Justification = "instantiated by cmd line parser")]
	internal class CommandLineOptions : ICommandLineOptions
	{
		[Option('s', "sln", Required = true, HelpText = "Path to solution file")]
		public string SolutionFile { get; set; }

		[Option('m', "msbuild", Required = true, HelpText = "Path to msbuild")]
		public string MsBuildPath { get; set; }

		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

	}

	[SuppressMessage("Microsoft.Performance", "CA1812", Justification = "instantiated by cmd line parser")]
	[SuppressMessage("Microsoft.Performance", "SA1402", Justification = "Empty class belongs to CommandLineOptions")]
	[SuppressMessage("Microsoft.Performance", "HG1011", Justification = "Empty class belongs to CommandLineOptions")]
	[Verb("check", HelpText = "Checks that packages are always referenced with the same version")]
	internal class CommandLineOptionsCheck : CommandLineOptions
	{
	}

	[SuppressMessage("Microsoft.Performance", "CA1812", Justification = "instantiated by cmd line parser")]
	[SuppressMessage("Microsoft.Performance", "SA1402", Justification = "Empty class belongs to CommandLineOptions")]
	[SuppressMessage("Microsoft.Performance", "HG1011", Justification = "Empty class belongs to CommandLineOptions")]
	[Verb("fix", HelpText = "References packages always with the same version")]
	internal class CommandLineOptionsFix : CommandLineOptions
	{
	}
}
