using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace CargoWise.eHub.Shared.ProjectTests
{
	[TestFixture]
	public class SolutionFileTests
	{
		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckAllSolutionFilesInBuildXml(DirectoryInfo directory)
		{
			var buildXml = SourceFileHelpers.LoadBuildXML(SourceFileHelpers.HubSourceDirectory.Value);
			var solutionsInBuildXml = (XmlElement)buildXml.GetElementsByTagName("Solutions")[0];
			var solutionFiles = SourceFileHelpers.GetSolutions(directory);
			foreach (var solution in solutionFiles)
			{
				var solutionToLower = solution.ToLower();
				if (SolutionFilesExcludedFromBuild.Any((s) => solutionToLower.Contains(s.ToLower())))
				{
					continue;
				}

				if (!SourceFileHelpers.TryFindChildByAttributeThatContainsValue(solutionsInBuildXml, "Filename", solution, out var specificVersionNode))
				{
					Assert.Fail(string.Format("Cannot file solution {0} in build.xml", solution));
				}
			}
		}

		[Test]
		public void CheckExcludedSolutionFilesAreNotInBuildXml()
		{
			var buildXml = SourceFileHelpers.LoadBuildXML(SourceFileHelpers.HubSourceDirectory.Value);
			var solutionsInBuildXml = (XmlElement)buildXml.GetElementsByTagName("Solutions")[0];
			foreach (var solution in SolutionFilesExcludedFromBuild)
			{
				if (SourceFileHelpers.TryFindChildByAttributeThatContainsValue(solutionsInBuildXml, "Filename", solution, out var specificVersionNode))
				{
					Assert.Fail(string.Format("Excluded solution {0} was found in build.xml. If this solution shouldn't be excluded anymore, remove it from SolutionFilesExcludedFromBuild.", solution));
				}
			}
		}

		[Test]
		public void CheckExcludedSolutionFilesAreNotDeleted()
		{
			foreach (var solution in SolutionFilesExcludedFromBuild)
			{
				var filePath = Path.Combine(SourceFileHelpers.HubSourceDirectory.Value.FullName, solution);
				if (!File.Exists(filePath))
				{
					Assert.Fail(string.Format("Excluded solution {0} was not found in the source files. If this solution is being deleted, remove it from SolutionFilesExcludedFromBuild.", solution));
				}
			}
		}

		static readonly List<string> SolutionFilesExcludedFromBuild = new List<string>(new string[] { "DevScripts\\eServicesTools\\eServicesTools.sln" });
	}
}
