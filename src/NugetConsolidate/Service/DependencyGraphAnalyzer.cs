﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal class DependencyGraphAnalyzer : IDependencyGraphAnalyzer
	{
		private readonly ILockFileService m_lockFileService;
		private AnalyzedDependencyGraph m_dependencyGraph;

		public DependencyGraphAnalyzer(ILockFileService lockFileService)
		{
			m_lockFileService = lockFileService;
		}

		public AnalyzedDependencyGraph AnalyzeDependencyGraph(DependencyGraphSpec dependencyGraphSpec)
		{
			m_dependencyGraph = new AnalyzedDependencyGraph(dependencyGraphSpec);
			var lockfileQueue = new TransformBlock<PackageSpec, (LockFile, PackageSpec)>(GetLockFile, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8, EnsureOrdered = false });
			var processLockFile = new ActionBlock<(LockFile, PackageSpec)>(ProcessLockFile, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = false });

			lockfileQueue.LinkTo(processLockFile, new DataflowLinkOptions() { PropagateCompletion = true });

			foreach (var project in dependencyGraphSpec.Projects.Where(p =>
				p.RestoreMetadata.ProjectStyle == ProjectStyle.PackageReference))
			{
				lockfileQueue.Post(project);
			}

			lockfileQueue.Complete();
			processLockFile.Completion.Wait();
			return m_dependencyGraph;
		}

		private void ProcessLockFile((LockFile lockFile, PackageSpec project) job)
		{
			Stack<string> dependencyChain = new Stack<string>();
			dependencyChain.Push(job.project.Name);
			ColorConsole.WriteLine($"Process - {job.project.Name}");
			foreach (var targetFramework in job.project.TargetFrameworks)
			{
				var lockFileTargetFramework = job.lockFile.Targets.FirstOrDefault(t => t.TargetFramework.Equals(targetFramework.FrameworkName));
				if (lockFileTargetFramework != null)
				{
					foreach (var dependency in targetFramework.Dependencies)
					{
						var projectLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == dependency.Name);
						ReportDependency(projectLibrary, lockFileTargetFramework, dependencyChain);
					}
				}
			}
		}

		private (LockFile, PackageSpec) GetLockFile(PackageSpec project)
		{
			ColorConsole.WriteLine($"Get lockfile for project: {project.Name}");
			return (m_lockFileService.GetLockFile(project.FilePath, project.RestoreMetadata.OutputPath), project);
		}

		private void ReportDependency(LockFileTargetLibrary projectLibrary, LockFileTarget lockFileTargetFramework, Stack<string> depChain)
		{
			depChain.Push($"{projectLibrary.Name}-{projectLibrary.Version}");
			m_dependencyGraph.AddDependency(projectLibrary, depChain);
			foreach (var childDependency in projectLibrary.Dependencies)
			{
				var childLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == childDependency.Id);
				if (childLibrary != null)
				{
					ReportDependency(childLibrary, lockFileTargetFramework, depChain);
				}
				else
				{
					ColorConsole.WriteWarning($"child {childDependency.Id} of {projectLibrary.Name} not found");
				}
			}
			depChain.Pop();
		}

	}
}
