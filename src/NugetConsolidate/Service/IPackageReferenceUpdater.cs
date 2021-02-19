namespace NugetConsolidate.Service
{
	internal interface IPackageReferenceUpdater
	{
		void UpdateDirectPackageReference(RequiredNugetUpdate update);
		void UpdatePackageReferenceFromTransitiveDependency(RequiredNugetUpdate update, string solutionFolder);
		void IncludeGeneratedPackageReferenceInBuildTargets(string solutionFile);
	}
}