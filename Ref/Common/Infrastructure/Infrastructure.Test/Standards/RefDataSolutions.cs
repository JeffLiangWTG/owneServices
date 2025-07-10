using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test.Standards
{
	[TestFixture]
	public class RefDataSolutions
	{
		private enum ConfigurationType
		{
			Debug,
			Release,
			UAT
		}

		private const string PaketReferencesFileName = "paket.references";

		/// <summary>
		/// Relative paths (from repo root folder) of folders that should be ignored.
		/// </summary>
		private static readonly string[] IgnoredFolderPaths =
		{
			".GIT",
			"OBJ",
			"BIN"
		};

		/// <summary>
		/// Relative paths (from repo root folder) of solution files that should be ignored.
		/// </summary>
		private static readonly string[] IgnoredSolutionPaths =
		{
			@"Service\NewService.Test\DummySlnFileForMvcTesting.sln"
		};

		/// <summary>
		/// Relative paths (from repo root folder) of projects that are explicitly marked as non-test.
		/// (Some projects may reference NUnit but are not test projects)
		/// </summary>
		private static readonly string[] ForcedNonTestProjectPaths =
		{
			@"Common\Infrastructure\Utils\Utils.csproj"
		};

		[Test]
		public void ShouldExcludeTestProjectsFromReleaseBuild()
		{
			var solutionFiles = GetSolutionFiles();

			foreach (var solutionFile in solutionFiles)
			{
				Assert.Multiple(() =>
				{
					foreach (var project in solutionFile.ProjectsInOrder)
					{
						AssertProjectConfiguration(project);
					}
				});
			}
		}

		private static void AssertProjectConfiguration(ProjectInSolution project)
		{
			var isTestProject = IsTestProject(project);
			var projectName = $"{(isTestProject ? "Test" : "Non-Test")} project {project.ProjectName} ({project.AbsolutePath})";

			foreach (var configuration in project.ProjectConfigurations)
			{
				var configurationType = GetConfigurationType(configuration.Key);
				switch (configurationType)
				{
					case ConfigurationType.Debug:
						Assert.That(configuration.Value.IncludeInBuild, Is.True,
							$"{projectName} is not included in build for {configurationType} configuration");
						break;
					case ConfigurationType.Release:
					case ConfigurationType.UAT:
						if (isTestProject)
						{
							Assert.That(configuration.Value.IncludeInBuild, Is.False,
								$"{projectName} is included in build for {configurationType} configuration");
						}
						else
						{
							Assert.That(configuration.Value.IncludeInBuild, Is.True,
								$"{projectName} is not included in build for {configurationType} configuration");
							;
						}

						break;
				}
			}
		}

		private static ConfigurationType GetConfigurationType(string configurationKey)
		{
			if (configurationKey.Contains("Debug", StringComparison.InvariantCultureIgnoreCase))
			{
				return ConfigurationType.Debug;
			}

			if (configurationKey.Contains("Release", StringComparison.InvariantCultureIgnoreCase))
			{
				return ConfigurationType.Release;
			}

			if (configurationKey.Contains("UAT", StringComparison.InvariantCultureIgnoreCase))
			{
				return ConfigurationType.UAT;
			}

			throw new Exception("Unknown configuration type");
		}

		/// <summary>
		/// A test project is a project that has a name containing "Test" or has a paket file with NUnit package.
		/// </summary>
		/// <param name="project"></param>
		/// <returns></returns>
		private static bool IsTestProject(ProjectInSolution project)
		{
			var projectFilePath = project.AbsolutePath;
			var forcedNonTestProject = ForcedNonTestProjectPaths.Any(x =>
				projectFilePath.Contains(x, StringComparison.InvariantCultureIgnoreCase));
			if (forcedNonTestProject)
			{
				return false;
			}

			var hasTestSuffix =
				project.ProjectName.Contains("Test", StringComparison.InvariantCultureIgnoreCase);
			if (hasTestSuffix)
			{
				return true;
			}

			var projectDirectory = Path.GetDirectoryName(projectFilePath);
			var paketFilePath = Path.Join(projectDirectory, PaketReferencesFileName);
			var paketFileExist = File.Exists(paketFilePath);
			if (!paketFileExist)
			{
				return false;
			}

			var paketFileContent = File.ReadAllLines(paketFilePath);
			var hasNUnitPackage = paketFileContent.Any(x =>
				x.Contains("NUnit", StringComparison.InvariantCultureIgnoreCase));
			return hasNUnitPackage;
		}

		private static IEnumerable<SolutionFile> GetSolutionFiles()
		{
			var rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var ignoreFolderPaths = IgnoredFolderPaths
				.Select(f => Path.Join(rootPath, f))
				.Select(Path.GetFullPath);

			var solutionFilePaths = Directory.GetFiles(rootPath, "*.sln", SearchOption.AllDirectories)
				.Select(Path.GetFullPath)
				.Where(x => !ignoreFolderPaths.Any(i =>
					x.StartsWith(i, StringComparison.InvariantCultureIgnoreCase)))
				.Where(x => !IgnoredSolutionPaths.Any(i =>
					x.Contains(i, StringComparison.InvariantCultureIgnoreCase)));

			return solutionFilePaths.Select(SolutionFile.Parse);
		}
	}
}
