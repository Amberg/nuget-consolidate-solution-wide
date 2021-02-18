namespace NugetConsolidate.Service
{
	internal class ConsolidateService
	{
		private readonly ICommandLineOptions m_options;
		private readonly IDependencyGraphService m_dependencyGraphService;
		private readonly IDependencyGraphAnalyzer m_dependencyGraphAnalyzer;

		public ConsolidateService(ICommandLineOptions options, IDependencyGraphService dependencyGraphService, IDependencyGraphAnalyzer dependencyGraphAnalyzer)
		{
			m_options = options;
			m_dependencyGraphService = dependencyGraphService;
			m_dependencyGraphAnalyzer = dependencyGraphAnalyzer;
		}

		public void ConsolidateTransitiveDependencies()
		{
			ColorConsole.WriteInfo($"Generate dependency graph for {m_options.SolutionFile}");
			var graphSpec = m_dependencyGraphService.GenerateDependencyGraph(m_options.SolutionFile, m_options.MsBuildPath);
			var graph = m_dependencyGraphAnalyzer.AnalyzeDependencyGraph(graphSpec);
			graph.LogSummary();
		}
	}
}
