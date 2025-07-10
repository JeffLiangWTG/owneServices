using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsSolutionTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoInlineUnitTestsInReleaseProjects()
		{
			var excludedSolutions = GetBaseLine("NoInlineUnitTestsInReleaseProjects");

			var solutions = BuildXml.Instance.GetAllSolutionFileNames()
				.Where(p => p.Contains(CustomsDirectory) && !excludedSolutions.Contains(p));

			bool isCSProject(ProjectInformation p) => p.ProjectPath.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase);
			bool isTestProject(ProjectInformation p) => p.ProjectName.ToUpperInvariant().Contains(".TEST");

			CombineAssertions(() =>
			{
				foreach (var solution in solutions)
				{
					try
					{
						var solutionFile = BaseSourcePath + solution;
						var sln = new SolutionFile(solutionFile);
						var dir = Directory.GetParent(solutionFile).FullName;
						var projectsToCheck = sln.Projects
							.Where(p => isCSProject(p) && !isTestProject(p));

						foreach (var pi in projectsToCheck)
						{
							var projectFile = Path.Combine(dir, pi.ProjectPath);
							var content = File.ReadAllText(projectFile).ToUpperInvariant();
							Assert($"Project {projectFile} Contains NUnit reference", !content.Contains("NUNITCORE.") && !content.Contains("NUNIT."));
						}
					}
					catch (Exception e)
					{
						// If an exception occurs we need the test to fail but we
						// don't want the entire test to stop at the moment it
						// does - so we record it as a failure but let the test
						// continue by capturing the exception and telling the
						Fail($"An exception occured during the test: {e.Message}");
					}
				}
			});
		}

		public void TestNoMessagingSerializerAssemblies()
		{
			var excludedAssemblies = GetBaseLine("NoMessagingSerializerAssemblies");

			var allCustomsXmlSerializerAssemblies = new HashSet<string>();
			foreach (var solutionName in BuildXml.Instance.GetAllSolutionFileNames().Where(p => p.Contains(CustomsDirectory)))
			{
				foreach (var xmlSerializerAssembly in BuildXml.Instance.GetAllAssembliesInSolution(solutionName).Where(name => name.ToUpperInvariant().EndsWith(".XMLSERIALIZERS.DLL")))
				{
					var slashIndex = xmlSerializerAssembly.IndexOf('\\');

					var updatedName = slashIndex != -1 && xmlSerializerAssembly.StartsWith("net")
						? xmlSerializerAssembly.Substring(slashIndex + 1).Trim()
						: xmlSerializerAssembly;

					allCustomsXmlSerializerAssemblies.Add(updatedName);
				}
			}

			CombineAssertions(() =>
			{
				foreach (var xmlSerializerAssembly in allCustomsXmlSerializerAssemblies)
				{
					Assert($@"XmlSerializer assembly {xmlSerializerAssembly} for Customs Messaging.
The correct location for message builders and serialization assemblies is the Customs Respository.
If this is the extension of existing customs messaging, create new WI's and start the move to the Customs repository.
Do not add to the exclusion list as this just makes the problem bigger later.", excludedAssemblies.Contains(xmlSerializerAssembly));
				}

				foreach (var excludedAssembly in excludedAssemblies)
				{
					Assert($@"The excluded assembly {excludedAssembly} has been moved to the Customs repository.
Please delete from the excluded list in this test.", allCustomsXmlSerializerAssemblies.Contains(excludedAssembly));
				}
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoProjectFolderPrefix()
		{
			var solutions = BuildXml.Instance.GetAllSolutionFileNames()
				.Where(p => p.Contains(CustomsDirectory));

			CombineAssertions(() =>
			{
				foreach (var solution in solutions)
				{
					var solutionFile = BaseSourcePath + solution;
					var foldersInSolutionPath = solution.Split(Path.DirectorySeparatorChar);
					var moduleDirectory = foldersInSolutionPath[Array.FindIndex(foldersInSolutionPath, s => s.Equals("Customs", StringComparison.OrdinalIgnoreCase)) + 1];
					var sln = new SolutionFile(solutionFile);
					var dir = Directory.GetParent(solutionFile).FullName;

					foreach (var project in sln.Projects)
					{
						var projectFile = Path.Combine(dir, project.ProjectPath);
						var projectParentFolder = Directory.GetParent(projectFile).Name;
						AssertEquals($"Project {projectFile} folder has prefix Enterprise.Customs.", false, projectParentFolder.StartsWith("Enterprise.Customs.", StringComparison.OrdinalIgnoreCase));
						AssertEquals($"Project {projectFile} folder has prefix of its module folder {moduleDirectory}", false, projectParentFolder.StartsWith(moduleDirectory));
					}
				}
			});
		}

		HashSet<string> GetBaseLine(string baseLineFileName)
		{
			var resourceName = "BaseLine." + baseLineFileName;
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var baseLine = resourceRetriever.GetString($@"Enterprise.Customs.Business.Testing.CustomsSolutionTests.BaseLines.{resourceName}.txt");
			return baseLine
					.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
					.ToHashSet();
		}

		const string CustomsDirectory = @"Enterprise\Product\Operations\Customs\";
	}
}
