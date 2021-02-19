using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetConsolidate.Service
{
	internal class ConsolidateService
	{
		private readonly ICommandLineOptions m_options;
		private readonly IDependencyGraphReader m_dependencyGraphReader;
		private readonly IDependencyGraphAnalyzer m_dependencyGraphAnalyzer;
		private readonly IPackageReferenceUpdater m_packageReferenceUpdater;

		public ConsolidateService(ICommandLineOptions options,
			IDependencyGraphReader dependencyGraphReader,
			IDependencyGraphAnalyzer dependencyGraphAnalyzer,
			IPackageReferenceUpdater packageReferenceUpdater)
		{
			m_options = options;
			m_dependencyGraphReader = dependencyGraphReader;
			m_dependencyGraphAnalyzer = dependencyGraphAnalyzer;
			m_packageReferenceUpdater = packageReferenceUpdater;
		}

		public int ConsolidateTransitiveDependencies(bool checkOnly)
		{
			RestoreSolution();

			ColorConsole.WriteInfo($"Generate dependency graph for {m_options.SolutionFile} please wait....");
			var graphSpec = m_dependencyGraphReader.GenerateDependencyGraph(m_options.SolutionFile, m_options.MsBuildPath);
			var graph = m_dependencyGraphAnalyzer.AnalyzeDependencyGraph(graphSpec);
			var requiredUpdates = graph.IdentifyRequiredNugetUpdates(m_options.Verbose).ToList();
			int result = 0; // success

			if (requiredUpdates.Any())
			{
				if (checkOnly)
				{
					var packages = requiredUpdates.GroupBy(x => x.Library.Name).ToList();
					ColorConsole.WriteWarning($"{packages.Count} packages are referenced with different versions:");
					foreach (var package in packages)
					{
						var firstOccurrence = package.First();
						ColorConsole.WriteWarning($"Upgrade of {firstOccurrence.Library.Name} to version {firstOccurrence.TargetVersion} required in {package.Count()} projects");
					}
					ColorConsole.WriteWarning("run this command again with the fix option to fix this");
					result = requiredUpdates.Count;
				}
				else
				{
					UpdatePackageReferences(requiredUpdates);
				}
			}
			else
			{
				ColorConsole.WriteInfo("all packages are referenced with the same version");
			}
			return result; // success
		}

		private void RestoreSolution()
		{
			var runner = new ProcessRunner();
			ColorConsole.WriteInfo($"Restore {m_options.SolutionFile}");
			var result = runner.Run("dotnet", null, new[] { "restore", m_options.SolutionFile });
			if (!result.IsSuccess)
			{
				ColorConsole.WriteError($"restoring failed {result.Errors} ");
			}
		}

		private void UpdatePackageReferences(IReadOnlyCollection<RequiredNugetUpdate> requiredNugetUpdates)
		{
			// check entry in build targets file
			if (requiredNugetUpdates.Any(x => !x.DirectReference))
			{
				m_packageReferenceUpdater.IncludeGeneratedPackageReferenceInBuildTargets(m_options.SolutionFile);
			}

			foreach (var directUpdate in requiredNugetUpdates
				.GroupBy(x => x.Library.Name))
			{
				ColorConsole.WriteWrappedHeader($"Update {directUpdate.Key} to {directUpdate.Max(x => x.TargetVersion)} used in {directUpdate.First().UpateCausedBy}", headerColor: ConsoleColor.Green);
				foreach (var update in directUpdate.Where(x => x.DirectReference))
				{
					m_packageReferenceUpdater.UpdateDirectPackageReference(update);
				}

				foreach (var update in directUpdate.Where(x => !x.DirectReference))
				{
					m_packageReferenceUpdater.UpdatePackageReferenceFromTransitiveDependency(update, m_options.SolutionFile);
				}
			}
		}
	}
}
