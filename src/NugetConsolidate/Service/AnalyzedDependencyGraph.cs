using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace NugetConsolidate.Service
{
	internal class AnalyzedDependencyGraph
	{
		private readonly DependencyGraphSpec m_dependencyGraphSpec;
		private readonly Dictionary<string, NugetDependency> m_assemblyLookup;

		public AnalyzedDependencyGraph(DependencyGraphSpec dependencyGraphSpec)
		{
			m_dependencyGraphSpec = dependencyGraphSpec;
			m_assemblyLookup = new Dictionary<string, NugetDependency>();
		}

		public void AddDependency(LockFileTargetLibrary projectLibrary, Stack<string> depChain)
		{
			if (!m_assemblyLookup.TryGetValue(projectLibrary.Name, out var nugetDependency))
			{
				nugetDependency = new NugetDependency(projectLibrary.Name);
				m_assemblyLookup[projectLibrary.Name] = nugetDependency;
			}
			nugetDependency.AddOccurrence(projectLibrary.Version, depChain.Reverse().ToList());
		}

		public void LogSummary()
		{
			ColorConsole.WriteInfo($"{m_assemblyLookup.Count} dependencies found in {m_dependencyGraphSpec.Projects.Count} projects");
			if (m_assemblyLookup.Any(a => a.Value.OccurrenceList.Count > 1))
			{
				ColorConsole.WriteWrappedHeader("duplicate dependency found", headerColor: ConsoleColor.Red);
				foreach (var duplicateVersions in m_assemblyLookup.Where(a => a.Value.OccurrenceList.Count > 1))
				{
					ColorConsole.WriteLine($"Duplicate dependency: {duplicateVersions.Key}", ConsoleColor.Red);
					foreach (var occurrences in duplicateVersions.Value.OccurrenceList)
					{
						ColorConsole.WriteLine($"\t{occurrences.Key}: used in", ConsoleColor.Green);
						foreach (var projectAndOccurrence in occurrences.Value.GroupBy(x => x.Root))
						{
							ColorConsole.WriteLine($"\t\t{projectAndOccurrence.Key}:");
							foreach (var directReferencedNugets in projectAndOccurrence
								.GroupBy(x => x.DirectReference)
								.Select(x => new
								{
									MaxLvl = x.Max(v => v.Lvl),
									DirectAssembly = x.Key
								})
								.OrderBy(x => x.MaxLvl))
							{
								if (directReferencedNugets.MaxLvl == 0)
								{
									ColorConsole.WriteLine("\t\t\tdirect reference", ConsoleColor.Green);
								}
								else
								{
									ColorConsole.WriteEmbeddedColorLine($"\t\t\t{duplicateVersions.Key} transitive reference({directReferencedNugets.MaxLvl}) of [Yellow]{directReferencedNugets.DirectAssembly}[/Yellow]");
								}
							}
						}
					}
				}
			}
		}

		private class NugetDependency
		{
			public NugetDependency(string name)
			{
				OccurrenceList = new Dictionary<NuGetVersion, List<DependencyOccurrence>>();
				Name = name;
			}

			public Dictionary<NuGetVersion, List<DependencyOccurrence>> OccurrenceList { get; }

			public string Name
			{
				get;
			}

			public void AddOccurrence(NuGetVersion version, List<string> depChain)
			{
				if (!OccurrenceList.TryGetValue(version, out var rootList))
				{
					rootList = new List<DependencyOccurrence>();
					OccurrenceList[version] = rootList;
				}
				rootList.Add(new DependencyOccurrence(depChain));
			}
		}

		private class DependencyOccurrence
		{
			private readonly List<string> m_depChain;

			public DependencyOccurrence(List<string> depChain)
			{
				m_depChain = depChain;
			}
			public int Lvl => m_depChain.Count - 2;

			public string Root => m_depChain[0];

			public string DirectReference => m_depChain[1];
		}
	}
}
