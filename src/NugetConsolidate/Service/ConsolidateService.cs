using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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

		public void ConsolidateTransitiveDependencies()
		{
			ColorConsole.WriteInfo($"Generate dependency graph for {m_options.SolutionFile}");
			var graphSpec = m_dependencyGraphReader.GenerateDependencyGraph(m_options.SolutionFile, m_options.MsBuildPath);
			var graph = m_dependencyGraphAnalyzer.AnalyzeDependencyGraph(graphSpec);
			var requiredUpdates = graph.IdentifyRequiredNugetUpdates();
			UpdatePackageReferences(requiredUpdates.ToList());
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
				ColorConsole.WriteWrappedHeader($"Update {directUpdate.Key} to {directUpdate.Max(x => x.TargetVersion)}", headerColor: ConsoleColor.Green);
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
