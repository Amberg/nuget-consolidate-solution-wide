using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal interface IDependencyGraphAnalyzer
	{
		AnalyzedDependencyGraph AnalyzeDependencyGraph(DependencyGraphSpec dependencyGraph);
	}
}