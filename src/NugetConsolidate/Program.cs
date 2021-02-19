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
			var consolidateService = new ConsolidateService(options,
				new DependencyGraphReader(),
				new DependencyGraphAnalyzer(new LockFileService()),
				new PackageReferenceUpdater());
			consolidateService.ConsolidateTransitiveDependencies();
		}
	}
}
