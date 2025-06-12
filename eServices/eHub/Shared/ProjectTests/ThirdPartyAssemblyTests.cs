using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CargoWise.eHub.Shared.ProjectTests
{
	[TestFixture]
	public class ThirdPartyAssemblyTests
	{
		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckNoProjectReferencesAreDirectlyPointingAtPackagedAssemblies(DirectoryInfo directory)
		{
			var projectFiles = SourceFileHelpers.GetCSAndBTProjects(directory);
			foreach (var projectFile in projectFiles)
			{
				var projectXml = new XmlDocument();
				projectXml.Load(projectFile);
				var referencesInProject = projectXml.GetElementsByTagName("Reference");
				foreach (var reference in referencesInProject)
				{
					var refElement = (XmlElement)reference;
					if (SourceFileHelpers.TryFindChildByName(refElement, "HintPath", out var propertyNode))
					{
						var hintPath = propertyNode.ChildNodes[0].Value;
						Assert.That(!hintPath.Contains("\\packages\\"), "File {0} contains a direct reference to an assembly in a package.  The assembly should be copied to the bin directory and referenced there instead.", projectFile);
					}
				}
			}
		}

		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckSqlprojectReferencesNotContainsCommonLogging(DirectoryInfo directory)
		{
			var projectFiles = SourceFileHelpers.GetFiles(directory, "*.sqlproj");
			foreach (var projectFile in projectFiles)
			{
				var projectXml = new XmlDocument();
				projectXml.Load(projectFile);
				var referencesInProject = projectXml.GetElementsByTagName("Reference");
				foreach (XmlElement reference in referencesInProject)
				{
					var include = reference.Attributes["Include"]?.Value ?? string.Empty;
					Assert.That(include, Does.Not.StartWith("Common.Logging.Core"), "SQL Server CLR only work for certain set of framework, Common.Logging.Core after 3.4.0 only provide framework net40, which is not compatible with CREATE/ALTER ASSEMBLY statement.");
				}
			}
		}

		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckProjectReferencesToPackagedAssemblies(DirectoryInfo directory)
		{
			var thirdPartyReferences = GetMatchingProjectReferences(directory, AssemblyIsThirdPartyFromPackage);
			var buildXml = SourceFileHelpers.LoadBuildXML(SourceFileHelpers.HubSourceDirectory.Value);
			var otherFiles = (XmlElement)buildXml.GetElementsByTagName("OtherFiles")[0];
			var dependencies = (XmlElement)buildXml.GetElementsByTagName("Dependencies")[0];

			foreach (var referencePair in thirdPartyReferences)
			{
				XmlElement firstReference = null;
				string firstFullHintPath = null;

				foreach (var reference in referencePair.Value)
				{
					AssertProjectReferenceProperty(reference, referencePair.Key, "SpecificVersion", false);
					var hintPath = AssertProjectReferencePropertyIsConfigured(reference, referencePair.Key, "HintPath");

					if (firstReference == null)
					{
						firstReference = reference;
						firstFullHintPath = ResolveFullPathToAssembly(hintPath, reference.BaseURI);
						if (!firstFullHintPath.StartsWith("$(BTSINSTALLPATH)", StringComparison.InvariantCultureIgnoreCase))
						{
							Assert.That(firstFullHintPath.ToLower().Contains(("\\bin\\" + referencePair.Key).ToLower()), string.Format("Assembly {0} is referenced from location = {1} instead of the bin directory", referencePair.Key, firstFullHintPath));

							if (!SourceFileHelpers.TryFindChildCopyToBinEntry(otherFiles, referencePair.Key + ".dll", out var buildXmlCopyElement) &&
								!SourceFileHelpers.TryFindChildDependencyEntry(dependencies, referencePair.Key + ".dll", out var buildXmlDependencyElement))
							{
								Assert.Fail(string.Format("No entry in Build.xml for copying assembly {0} to the bin directory", referencePair.Key));
							}
						}
					}
					else
					{
						Assert.That(firstFullHintPath.Equals(ResolveFullPathToAssembly(hintPath, reference.BaseURI), StringComparison.InvariantCultureIgnoreCase), string.Format("Conflicting HintPaths for assembly {0} in projects {1} and {2}", referencePair.Key, firstReference.BaseURI, reference.BaseURI));
					}
				}
			}
		}

		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckProjectReferencesToMicrosoftAssembliesThatDoNotRequireSpecificVersion(DirectoryInfo directory)
		{
			var thirdPartyReferences = GetMatchingProjectReferences(directory, AssemblyIsMicrosoftAndDoesNotRequireSpecificVersion);
			foreach (var referencePair in thirdPartyReferences)
			{
				foreach (var reference in referencePair.Value)
				{
					AssertProjectReferenceProperty(reference, referencePair.Key, "SpecificVersion", false);
				}
			}
		}

		[Test]
		[TestCaseSource(typeof(SourceFileHelpers), "TopLevelDirectories")]
		public void CheckProjectReferencesToBiztalkTestTools(DirectoryInfo directory)
		{
			var thirdPartyReferences = GetMatchingProjectReferences(directory, (n) => n.Equals(BizTalkTestToolsAssembly, StringComparison.InvariantCultureIgnoreCase));
			foreach (var projectReferenceList in thirdPartyReferences.Values)
			{
				foreach (var projectReference in projectReferenceList)
				{
					AssertProjectReferenceProperty(projectReference, BizTalkTestToolsAssembly, "HintPath", "$(BTSINSTALLPATH)Developer Tools\\Microsoft.Biztalk.TestTools.dll");
					AssertProjectReferenceProperty(projectReference, BizTalkTestToolsAssembly, "Private", true);
					AssertProjectReferenceProperty(projectReference, BizTalkTestToolsAssembly, "SpecificVersion", false);
				}
			}
		}

		[Test]
		public void CheckNoConflictingCopiesOfDllsInOtherFilesAndDependencies()
		{
			var buildXml = SourceFileHelpers.LoadBuildXML(SourceFileHelpers.HubSourceDirectory.Value);
			var otherFiles = (XmlElement)buildXml.GetElementsByTagName("OtherFiles")[0];
			var dependencies = (XmlElement)buildXml.GetElementsByTagName("Dependencies")[0];
			var copiedFiles = new Dictionary<string, string>();

			foreach (var otherFilesChild in otherFiles.ChildNodes)
			{
				var otherFileNode = (XmlElement)otherFilesChild;
				AssertAddCopiedFile(copiedFiles, otherFileNode.FirstChild.Value, otherFileNode.GetAttribute("CopyFrom"));
			}

			foreach (var dependency in dependencies.ChildNodes)
			{
				var dependencyNode = (XmlElement)dependency;
				var source = dependencyNode.GetAttribute("Path");

				foreach (var fileChild in dependencyNode.ChildNodes)
				{
					var fileChildNode = (XmlElement)fileChild;
					AssertAddCopiedFile(copiedFiles, fileChildNode.GetAttribute("Source"), source, fileChildNode.GetAttribute("Target"));
				}
			}
		}

		void AssertAddCopiedFile(Dictionary<string, string> copiedFiles, string binary, string source, string target = null)
		{
			target = string.IsNullOrEmpty(target) ? "bin" : target;
			var binaryWithTarget = string.Format($"{target.ToLower()}\\{binary.ToLower()}");
			if (copiedFiles.TryGetValue(binaryWithTarget, out var foundSource))
			{
				Assert.Fail("Duplicate other files entries detected in build xml for binary {0}, source locations {1} and {2}", binaryWithTarget, source, foundSource);
			}
			copiedFiles.Add(binaryWithTarget, source);
		}

		[Test]
		[Ignore("Unfortunately DAT does not copy in the packages directory when running tests, so this test can only be run through visual studio.")]
		public void CheckCorrectVersionOfAssembliesInBin()
		{
			const string resolutionHint = "NOTE: Try running the test FindSolutionWithBadOrMissingAssemblyReferences to identify which solution is causing this test to fail.  Most likely a project will be missing a reference to a dll it has an indirect dependance on.";

			var buildXml = SourceFileHelpers.LoadBuildXML(SourceFileHelpers.HubSourceDirectory.Value);
			var otherFiles = buildXml.GetElementsByTagName("OtherFiles")[0];

			foreach (var fileChild in otherFiles.ChildNodes)
			{
				var fileElement = (XmlElement)fileChild;
				var assemblyFile = fileElement.ChildNodes[0].Value;
				var copyFrom = fileElement.GetAttribute("CopyFrom");
				var assemblyInBinVerInfo = FileVersionInfo.GetVersionInfo(Path.Combine(HubBinDirectory.Value.FullName, assemblyFile));
				var assemblyInCopyVerInfo = FileVersionInfo.GetVersionInfo(Path.Combine(HubBinariesDirectory.Value.FullName, copyFrom, assemblyFile));
				Assert.AreEqual(assemblyInCopyVerInfo.FileMajorPart, assemblyInBinVerInfo.FileMajorPart, string.Format("Assembly {0} has changed during the build.  The assembly there now has a different major version. {1}", assemblyFile, resolutionHint));
				Assert.AreEqual(assemblyInCopyVerInfo.FileMinorPart, assemblyInBinVerInfo.FileMinorPart, string.Format("Assembly {0} has changed during the build.  The assembly there now has a different minor version. {1}", assemblyFile, resolutionHint));
				var assemblyInBinFileInfo = HubBinDirectory.Value.GetFiles(assemblyFile)[0];
				var assemblyInCopyFileInfo = HubBinariesDirectory.Value.GetDirectories(copyFrom, SearchOption.AllDirectories)[0].GetFiles(assemblyFile).FirstOrDefault();
				if (assemblyInCopyFileInfo != null)
				{
					Assert.AreEqual(assemblyInBinFileInfo.Length, assemblyInCopyFileInfo.Length, string.Format("Assembly {0} has changed during the build.  The assembly there is a different size. {1}", assemblyFile, resolutionHint));
				}
			}
		}

		[Test]
		[Ignore("This test is very slow and is a dodgy hack.  It is just here to help speed up identification of problems that might cause CheckCorrectVersionOfAssembliesInBin() to fail by identifying the problematic solution file.")]
		public void FindSolutionWithBadOrMissingAssemblyReferences()
		{
			try
			{
				CheckCorrectVersionOfAssembliesInBin();
			}
			catch (Exception ex)
			{
				Assert.Fail(string.Format("Check assemblies failed before running the test with exception {0}", ex.ToString()));
			}

			foreach (var directory in SourceFileHelpers.TopLevelDirectories)
			{
				var solutionFiles = SourceFileHelpers.GetSolutions(directory);
				foreach (var solution in solutionFiles)
				{
					var startInfo = new ProcessStartInfo("C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\MSBuild\\Current\\Bin\\MSBuild.exe", solution + " /t:Rebuild")
					{
						WindowStyle = ProcessWindowStyle.Hidden
					};
					var buildProc = Process.Start(startInfo);
					buildProc.WaitForExit();

					try
					{
						CheckCorrectVersionOfAssembliesInBin();
					}
					catch (Exception ex)
					{
						Assert.Fail(string.Format("Check assemblies failed on solution {0} with exception {1}", solution, ex.ToString()));
					}
				}
			}
		}

		static readonly List<string> AssemblyPrefixesNotPackaged = new List<string>(new string[] { "CargoWise", "System", "Accessibility", "WindowsBase", "PresentationCore", "PresentationFramework", "ServiceBroker", "netstandard", "Microsoft" });
		static readonly List<string> AssembliesPrefixesThatArePackaged = new List<string>(new string[] { "CargoWise.eServices.Encryption", "System.Web.WebPages", "Microsoft.Web", "Microsoft.AI", "Microsoft.Owin", "Microsoft.Data.Schema", "Microsoft.SqlServer" });

		static readonly List<string> AssembliesThatArePackaged = new List<string>(new string[] {
			"Microsoft.VisualStudio.QualityTools.UnitTestFramework", "System.Collections.Immutable", "System.Runtime", "System.Net.Http.Formatting", "Microsoft.ApplicationInsights", "Microsoft.CodeDom.Providers.DotNetCompilerPlatform",
			"Microsoft.Rest.ClientRuntime", "System.Web.Mvc", "System.Web.Razor", "System.Web.Helpers", "System.Web.Optimization", "Microsoft.BizTalk.Operations", "CargoWise.eServices.Server.Decryption", "Microsoft.AspNet.TelemetryCorrelation",
			"Microsoft.CodeDom.Providers.DotNetCompilerPlatform", "System.Diagnostics.DiagnosticSource", "System.ValueTuple", "Microsoft.BizTalk.Edi.EdiPipelines"});

		static readonly List<string> MicrosoftAssembliesInDotNetFramework = new List<string>(new string[] { "Microsoft.CSharp", "Microsoft.JScript", "Microsoft.RuleEngine" });
		static readonly List<string> MicrosoftAssembliesInBizTalk = new List<string>(new string[] { "Microsoft.BizTalk", "Microsoft.XLANGs" });

		const string BizTalkTestToolsAssembly = "Microsoft.Biztalk.TestTools";

		static void AssertProjectReferenceProperty(XmlElement projectReference, string assemblyName, string propertyName, string expectedValue)
		{
			var actualValue = AssertProjectReferencePropertyIsConfigured(projectReference, assemblyName, propertyName);
			Assert.IsTrue(actualValue.Equals(expectedValue, StringComparison.InvariantCultureIgnoreCase), "Project {0} has incorrect value for property {1} of {2}", projectReference.BaseURI, propertyName, actualValue);
		}

		static void AssertProjectReferenceProperty(XmlElement projectReference, string assemblyName, string propertyName, bool expectedValue)
		{
			var actualValue = Convert.ToBoolean(AssertProjectReferencePropertyIsConfigured(projectReference, assemblyName, propertyName));
			Assert.AreEqual(expectedValue, actualValue, "Project {0} has incorrect value for property {1}", projectReference.BaseURI, propertyName);
		}

		static string AssertProjectReferencePropertyIsConfigured(XmlElement projectReference, string assemblyName, string propertyName)
		{
			if (!SourceFileHelpers.TryFindChildByName(projectReference, propertyName, out var propertyNode))
			{
				Assert.Fail(string.Format("No {0} configuration for {1} in project {2}", propertyName, assemblyName, projectReference.BaseURI));
			}
			return propertyNode.ChildNodes[0].Value;
		}

		static bool AssemblyIsThirdPartyFromPackage(string assemblyName)
		{
			if (AssembliesThatArePackaged.Any(sw => assemblyName.Equals(sw, StringComparison.InvariantCultureIgnoreCase)) ||
				AssembliesPrefixesThatArePackaged.Any(sw => assemblyName.StartsWith(sw, StringComparison.InvariantCultureIgnoreCase)))
			{
				return true;
			}

			if (AssemblyPrefixesNotPackaged.Any(sw => assemblyName.StartsWith(sw, StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}

			return true;
		}

		static string ResolveFullPathToAssembly(string hintPath, string projectURI)
		{
			if (!hintPath.StartsWith("..\\"))
			{
				return hintPath;
			}

			return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectURI.Substring(8)), hintPath));
		}

		static bool AssemblyIsMicrosoftAndDoesNotRequireSpecificVersion(string assemblyName)
		{
			if (!assemblyName.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			if (MicrosoftAssembliesInBizTalk.Any(sw => assemblyName.StartsWith(sw, StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}

			if (MicrosoftAssembliesInDotNetFramework.Any(sw => assemblyName.StartsWith(sw, StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}

			return true;
		}

		static Dictionary<string, List<XmlElement>> GetMatchingProjectReferences(DirectoryInfo directory, Func<string, bool> assemblyNameMatchesFilter)
		{
			var projectFiles = SourceFileHelpers.GetCSAndBTProjects(directory);
			var thirdPartyReferences = new Dictionary<string, List<XmlElement>>();

			foreach (var projectFile in projectFiles)
			{
				var projectXml = new XmlDocument();
				projectXml.Load(projectFile);
				var referencesInProject = projectXml.GetElementsByTagName("Reference");
				foreach (var reference in referencesInProject)
				{
					var refElement = (XmlElement)reference;
					var assemblyName = refElement.GetAttribute("Include").Split(',')[0];
					if (assemblyNameMatchesFilter(assemblyName))
					{
						if (!thirdPartyReferences.TryGetValue(assemblyName, out var projectRefs))
						{
							projectRefs = new List<XmlElement>();
							thirdPartyReferences.Add(assemblyName, projectRefs);
						}
						projectRefs.Add(refElement);
					}
				}
			}

			return thirdPartyReferences;
		}

		readonly Lazy<DirectoryInfo> HubBinariesDirectory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent);
		readonly Lazy<DirectoryInfo> HubBinDirectory = new Lazy<DirectoryInfo>(() => new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent);
	}
}
