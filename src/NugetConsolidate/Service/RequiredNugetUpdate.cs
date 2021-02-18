using NuGet.ProjectModel;
using NuGet.Versioning;

namespace NugetConsolidate.Service
{
	internal class RequiredNugetUpdate
	{
		public RequiredNugetUpdate(string projectPath, LockFileTargetLibrary library, NuGetVersion targetVersion, bool directReference)
		{
			ProjectPath = projectPath;
			Library = library;
			TargetVersion = targetVersion;
			DirectReference = directReference;
		}

		public string ProjectPath
		{
			get;
		}

		public bool DirectReference
		{
			get;
		}

		public LockFileTargetLibrary Library
		{
			get;
		}

		public NuGetVersion TargetVersion
		{
			get;
		}
	}
}
