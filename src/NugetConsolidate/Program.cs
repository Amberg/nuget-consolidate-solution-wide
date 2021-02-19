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
			return consolidateService.ConsolidateTransitiveDependencies(false);
		}

		private static ConsolidateService CreateConsolidateService(CommandLineOptions options)
		{
			var consolidateService = new ConsolidateService(options,
				new DependencyGraphReader(),
				new DependencyGraphAnalyzer(new LockFileService()),
				new PackageReferenceUpdater());
			return consolidateService;
		}
	}
}
