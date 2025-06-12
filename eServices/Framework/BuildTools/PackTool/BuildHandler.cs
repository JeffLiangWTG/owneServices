using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using static eServices.BuildTools.PackTool.PackCommandService;

namespace eServices.BuildTools.PackTool
{
	public interface IBuildHandler
	{
		string? PackageId { get; }
		string? PackageVersion { get; }
		string? PackOutputPath { get; }
		string? NuspecOutputPath { get; }
		FileInfo ProjectFile { get; }

		void BuildPackage();
		void BuildPackageVersion(string version, string releaseNotes);
		void DeletePackage(string packagePath);
	}

	public class BuildHandler : IBuildHandler
	{
		private readonly BuildLogger buildLogger;
		private Project? buildProj;

		public string? PackageId { get; private set; }
		public string? PackageVersion { get; private set; }
		public string? PackOutputPath { get; private set; }
		public string? NuspecOutputPath { get; private set; }
		public FileInfo ProjectFile { get; }

		public BuildHandler(FileInfo projectFile, ILogger<BuildHandler> logger)
		{
			ProjectFile = projectFile;
			buildLogger = new BuildLogger(logger) { Verbosity = LoggerVerbosity.Normal };
		}

		public void BuildPackage()
		{
			buildProj = new Project(ProjectFile.FullName);
			PackageId = buildProj.GetPropertyValue("PackageId");
			PackageVersion = buildProj.GetPropertyValue("PackageVersion");
			NuspecOutputPath = buildProj.GetPropertyValue("NuspecOutputPath");
			buildProj.SetGlobalProperty("Configuration", "Release");
			buildProj.SetGlobalProperty("NoBuild", "True");
			buildProj.ReevaluateIfNecessary();
			if (!buildProj.Build("Pack", [buildLogger]))
				throw new InvalidOperationException($"Pack build failed for '{ProjectFile}'.");
			PackOutputPath = Path.Combine(ProjectFile.DirectoryName!, buildProj.GetPropertyValue("PackageOutputPath"));
		}

		public void BuildPackageVersion(string version, string releaseNotes)
		{
			buildProj!.SetProperty("PackageVersion", version);
			buildProj!.SetProperty("PackageReleaseNotes", releaseNotes);
			buildProj!.ReevaluateIfNecessary();
			buildProj!.Build("Pack", [buildLogger]);
		}

		public void DeletePackage(string packagePath)
		{
			if (File.Exists(packagePath))
				File.Delete(packagePath);
		}
	}
}
