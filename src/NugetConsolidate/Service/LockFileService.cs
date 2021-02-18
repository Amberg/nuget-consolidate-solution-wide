using System.IO;
using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal class LockFileService : ILockFileService
	{
		public LockFile GetLockFile(string projectPath, string outputPath)
		{
			// Run the restore command
			var dotNetRunner = new ProcessRunner();
			string[] arguments = new[] { "restore", $"\"{projectPath}\"" };
			var runStatus = dotNetRunner.Run("dotnet", Path.GetDirectoryName(projectPath), arguments);

			// Load the lock file
			string lockFilePath = Path.Combine(outputPath, "project.assets.json");
			return LockFileUtilities.GetLockFile(lockFilePath, NuGet.Common.NullLogger.Instance);
		}
    }
}
