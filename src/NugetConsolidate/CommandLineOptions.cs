using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace NugetConsolidate
{
	[SuppressMessage("Microsoft.Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "instantiated by cmd line parser")]
	internal class CommandLineOptions : ICommandLineOptions
	{
		[Option('s', "sln", Required = true, HelpText = "Path to solution file")]
		public string SolutionFile { get; set; }

		[Option('m', "msbuild", Required = true, HelpText = "Path to msbuild")]
		public string MsBuildPath { get; set; }

		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

	}
}
