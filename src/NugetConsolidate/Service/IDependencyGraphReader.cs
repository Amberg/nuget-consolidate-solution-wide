using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal interface IDependencyGraphReader
	{
		DependencyGraphSpec GenerateDependencyGraph(string projectPath, string pathToMsBuild);
	}
}