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
			var requiredUpdates = graph.IdentifyRequiredNugetUpdates();
			UpdateNugets(requiredUpdates);
		}

		private void UpdateNugets(IEnumerable<RequiredNugetUpdate> requiredNugetUpdates)
		{
			foreach (var directUpdate in requiredNugetUpdates
				.GroupBy(x => x.Library.Name))
			{
				ColorConsole.WriteWrappedHeader($"Update {directUpdate.Key} to {directUpdate.Max(x => x.TargetVersion)}", headerColor: ConsoleColor.Green);
				foreach (var update in directUpdate.Where(x => x.DirectReference))
				{
					UpdateNuget(update);
				}

				foreach (var update in directUpdate.Where(x => !x.DirectReference))
				{
					var dir = Path.GetDirectoryName(update.ProjectPath);
					var propsPath = Path.Combine(dir, "ApplicationRedirect.Build.props");
					ColorConsole.WriteEmbeddedColorLine($"Update {update.Library.Name}\t\t from [red]{update.Library.Version}[/red] to [green]{update.TargetVersion}[/green] in (Transitive) \t\t [green]{Path.GetFileName(update.ProjectPath)}[/green]");
				}
			}
		}

		internal void UpdateNuget(RequiredNugetUpdate update)
		{
			ColorConsole.WriteEmbeddedColorLine(
				$"Update {update.Library.Name}\t\t from [red]{update.Library.Version}[/red] to [green]{update.TargetVersion}[/green] in \t\t [green]{Path.GetFileName(update.ProjectPath)}[/green]");
			var doc = XDocument.Load(update.ProjectPath);
			var element = doc.Root?.Descendants("PackageReference").Where(e => e.Attribute("Include")?.Value == "NLog")?.SingleOrDefault();
			if (element != null)
			{
				var versionAttribute = element.Attribute("Version");
				if (versionAttribute != null)
				{
					if (versionAttribute.Value != update.Library.Version.ToString())
					{
						ColorConsole.WriteWarning($"Expected version {update.Library.Version} but {versionAttribute.Value} found");
					}
				}
				element.SetAttributeValue("Version", update.TargetVersion);
				doc.Save(update.ProjectPath);
			}
			else
			{
				ColorConsole.WriteError("No package reference node found");
			}
		}
	}
}
