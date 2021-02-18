using System;
using System.IO;
using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal class DependencyGraphService : IDependencyGraphService
	{
		public DependencyGraphSpec GenerateDependencyGraph(string projectPath, string pathToMsBuild)
		{
			var dotNetRunner = new ProcessRunner();
			string outputFile = Path.GetTempFileName();
			try
			{
				string[] arguments = new[] { $"\"{projectPath}\"", "/t:GenerateRestoreGraphFile", $"/p:RestoreGraphOutputPath={outputFile}" };
				var runStatus = dotNetRunner.Run(pathToMsBuild, Path.GetDirectoryName(projectPath), arguments);
				if (runStatus.IsSuccess)
				{
					return DependencyGraphSpec.Load(outputFile);
				}
				else
				{
					throw new ArgumentException($"Unable to process the the project `{projectPath}. Are you sure this is a valid .NET Core or .NET Standard project type?" +
										$"\r\n\r\nHere is the full error message returned from the Microsoft Build Engine:\r\n\r\n" + runStatus.Output, nameof(projectPath));
				}
			}
			finally
			{
				File.Delete(outputFile);
			}
		}
	}
}
