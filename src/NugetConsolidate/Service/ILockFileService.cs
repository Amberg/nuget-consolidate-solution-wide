using NuGet.ProjectModel;

namespace NugetConsolidate.Service
{
	internal interface ILockFileService
	{
		LockFile GetLockFile(string projectPath, string outputPath);
	}
}