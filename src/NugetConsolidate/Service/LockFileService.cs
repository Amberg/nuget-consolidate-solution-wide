using System.IO;
using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal class LockFileService : ILockFileService
	{
		public LockFile GetLockFile(string projectPath, string outputPath)
		{
			string lockFilePath = Path.Combine(outputPath, "project.assets.json");
			if (!File.Exists(lockFilePath))
			{
				// Run the restore command
				var dotNetRunner = new ProcessRunner();
				string[] arguments = new[] { "restore", $"\"{projectPath}\"" };
				var runStatus = dotNetRunner.Run("dotnet", Path.GetDirectoryName(projectPath), arguments);
			}
			return LockFileUtilities.GetLockFile(lockFilePath, NuGet.Common.NullLogger.Instance);
		}
    }
}
