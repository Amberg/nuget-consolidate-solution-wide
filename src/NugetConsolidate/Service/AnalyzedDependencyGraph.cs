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

		public void AddDependency(LockFileTargetLibrary projectLibrary, Stack<LockFileTargetLibrary> depChain, PackageSpec project)
		{
			if (!m_assemblyLookup.TryGetValue(projectLibrary.Name, out var nugetDependency))
			{
				nugetDependency = new NugetDependency(projectLibrary.Name);
				m_assemblyLookup[projectLibrary.Name] = nugetDependency;
			}
			nugetDependency.AddOccurrence(projectLibrary.Version, depChain.Reverse().ToList(), project);
		}

		public IEnumerable<RequiredNugetUpdate> IdentifyRequiredNugetUpdates(bool verbose)
		{
			if (verbose)
			{
				if (m_assemblyLookup.Any(a => a.Value.OccurrenceList.Count > 1))
				{
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
										ColorConsole.WriteEmbeddedColorLine(
											$"\t\t\t{duplicateVersions.Key} transitive reference({directReferencedNugets.MaxLvl})" +
											$" of [Yellow]{directReferencedNugets.DirectAssembly.Name}-{directReferencedNugets.DirectAssembly.Version}[/Yellow]");
									}
								}
							}
						}
					}
				}
			}
			ColorConsole.WriteInfo($"{m_assemblyLookup.Count} dependencies found in {m_dependencyGraphSpec.Projects.Count} projects");
			var duplicates = m_assemblyLookup.Where(a => a.Value.OccurrenceList.Count > 1).ToList();
			var maxVersionLookup = duplicates.ToDictionary(x => x.Key, x => x.Value.OccurrenceList.Keys.Max());
			var projectWithMaxVersion = duplicates.ToDictionary(x => x.Key, x => x.Value.ProjectWithMaxVersion);

			var updateRequired = duplicates.SelectMany(kvp =>
				kvp.Value.OccurrenceList.Where(x => x.Key < maxVersionLookup[kvp.Key]).SelectMany(x => x.Value)).ToList();

			return updateRequired.Select(x =>
				new RequiredNugetUpdate(x.ProjectPath, x.Target, maxVersionLookup[x.Target.Name], x.Lvl == 0, x.DirectReference.Name, projectWithMaxVersion[x.Target.Name]))
				.Distinct(new RequiredNugetUpdateEqualityComparer());
		}

		private class RequiredNugetUpdateEqualityComparer : IEqualityComparer<RequiredNugetUpdate>
		{
			public bool Equals(RequiredNugetUpdate x, RequiredNugetUpdate y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return x.ProjectPath == y.ProjectPath && x.DirectReference == y.DirectReference && x.Library.Equals(y.Library) && x.TargetVersion.Equals(y.TargetVersion);
			}

			public int GetHashCode(RequiredNugetUpdate obj)
			{
				return HashCode.Combine(obj.ProjectPath, obj.DirectReference, obj.Library, obj.TargetVersion);
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

			public string ProjectWithMaxVersion
			{
				get
				{
					var occurrencesWithMaxVersion = OccurrenceList
						.OrderByDescending(x => x.Key) // order by version
						.First().Value
						.OrderBy(oc => oc.Lvl).ToList();
					var shownOccurrence = occurrencesWithMaxVersion.First();
					var result = shownOccurrence.Root;
					if (shownOccurrence.Lvl > 0)
					{
						result += $" (ref by {shownOccurrence.DirectReference.Name})";
					}
					if (occurrencesWithMaxVersion.Count > 1)
					{
						result += $" and {occurrencesWithMaxVersion.Count - 1} other projects";
					}
					return result;
				}
			}

			private string Name
			{
				get;
			}

			public void AddOccurrence(NuGetVersion version, List<LockFileTargetLibrary> depChain, PackageSpec project)
			{
				if (!OccurrenceList.TryGetValue(version, out var rootList))
				{
					rootList = new List<DependencyOccurrence>();
					OccurrenceList[version] = rootList;
				}
				rootList.Add(new DependencyOccurrence(depChain, project));
			}
		}

		private class DependencyOccurrence
		{
			private readonly List<LockFileTargetLibrary> m_depChain;
			private readonly PackageSpec m_project;

			public DependencyOccurrence(List<LockFileTargetLibrary> depChain, PackageSpec project)
			{
				m_depChain = depChain;
				m_project = project;
			}

			public string ProjectPath => m_project.FilePath;

			public int Lvl => m_depChain.Count - 1;

			public string Root => m_project.Name;

			public LockFileTargetLibrary Target => m_depChain.Last();

			public LockFileTargetLibrary DirectReference => m_depChain[0];
		}
	}
}
