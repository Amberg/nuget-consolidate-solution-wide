using NuGet.ProjectModel;
using NuGet.Versioning;

namespace NugetConsolidate.Service
{
	internal class RequiredNugetUpdate
	{
		public RequiredNugetUpdate(string projectPath, LockFileTargetLibrary library, NuGetVersion targetVersion, bool directReference, string rootReferenceName, string upateCausedBy)
		{
			ProjectPath = projectPath;
			Library = library;
			TargetVersion = targetVersion;
			DirectReference = directReference;
			RootReferenceName = rootReferenceName;
			UpateCausedBy = upateCausedBy;
		}

		public string ProjectPath
		{
			get;
		}

		public bool DirectReference
		{
			get;
		}

		public string RootReferenceName
		{
			get;
		}

		public string UpateCausedBy { get; }

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
