using System;
using NuGet.ProjectModel;
using NuGet.Versioning;

namespace NugetConsolidate.Service
{
	internal class RequiredNugetUpdate : IEquatable<RequiredNugetUpdate>
	{
		public RequiredNugetUpdate(string projectPath, LockFileTargetLibrary library, NuGetVersion targetVersion, bool directReference, string rootReferenceName)
		{
			ProjectPath = projectPath;
			Library = library;
			TargetVersion = targetVersion;
			DirectReference = directReference;
			RootReferenceName = rootReferenceName;
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

		public LockFileTargetLibrary Library
		{
			get;
		}

		public NuGetVersion TargetVersion
		{
			get;
		}

		public bool Equals(RequiredNugetUpdate other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return ProjectPath == other.ProjectPath && DirectReference == other.DirectReference && Equals(Library, other.Library) && Equals(TargetVersion, other.TargetVersion);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((RequiredNugetUpdate)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ProjectPath, DirectReference, Library, TargetVersion);
		}
	}
}
