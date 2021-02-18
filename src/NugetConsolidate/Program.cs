using CommandLine;
using NugetConsolidate.Service;

namespace NugetConsolidate
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed<CommandLineOptions>(Run);
		}

		private static void Run(CommandLineOptions options)
		{
			var consolidateService = new ConsolidateService(options, new DependencyGraphService(), new DependencyGraphAnalyzer(new LockFileService()));
			consolidateService.ConsolidateTransitiveDependencies();
		}
	}
}
