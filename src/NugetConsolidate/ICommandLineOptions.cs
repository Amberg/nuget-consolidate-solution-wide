namespace NugetConsolidate
{
	internal interface ICommandLineOptions
	{
		string SolutionFile { get; set; }
		string MsBuildPath { get; set; }
		bool Verbose { get; set; }
	}
}