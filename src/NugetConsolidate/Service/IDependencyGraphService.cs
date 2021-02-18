using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal interface IDependencyGraphService
	{
		DependencyGraphSpec GenerateDependencyGraph(string projectPath, string pathToMsBuild);
	}
}