using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test.Standards
{
	[TestFixture]
	class RefDataProjects
	{
		[Test]
		public void PaketDependenciesShoudNotPinVersion()
		{
			var paketDependenciesFile = Path.Combine(rootPath, "paket.dependencies");
			Assert.That(paketDependenciesFile, Is.Not.Null, "paket.dependencies file not found");

			var versionRegex = new Regex(@"\b(\d)+\.(\d)+");
			var allFileLines = File.ReadAllLines(paketDependenciesFile);
			foreach(var line in allFileLines)
			{
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.OrdinalIgnoreCase) || !line.StartsWith("nuget", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (versionRegex.IsMatch(line) && (!line.Contains('#') || string.IsNullOrWhiteSpace(line.Substring(line.IndexOf('#') + 1))))
				{
					var message = "Pls use \"paket update <package> --version\" or put the reason behind the package, e.g. nuget NUnit 3.13.3 # reason";
					Assert.Fail($"paket.dependencies file should not pin version for package: {line}.\r\n{message}");
				}
			}
		}

		[Test]
		public void DirectoryPackageReferenceUsesSameVersionAsPaket()
		{
			var paketLockFile = Directory.GetFiles(rootPath, "paket.lock", SearchOption.AllDirectories).FirstOrDefault();
			var paketLockFileContent = File.ReadAllText(paketLockFile);
			var allFiles = Directory.GetFiles(rootPath, "Directory.Build.props", SearchOption.AllDirectories).Union(Directory.GetFiles(rootPath, "Directory.Build.targets", SearchOption.AllDirectories));
			Assert.That(allFiles.Count(), Is.GreaterThan(2));
			Assert.Multiple(() =>
			{
				foreach (var propFile in allFiles)
				{
					var xml = XElement.Load(propFile);
					foreach (var packageRefElement in xml.Descendants().Where(x => x.Name.LocalName == "PackageReference"))
					{
						var pkgName = packageRefElement.Attribute("Include").Value;
						var pkgVer = packageRefElement.Attribute("Version").Value;
						Assert.True(paketLockFileContent.Contains(pkgName + " (" + pkgVer + ")"), $"Paket.lock does not contain {pkgName} {pkgVer} - File Path: [{propFile}]");
					}
				}
			});
		}

		[Test]
		public void UsePaketReference()
		{
			var results = new List<string>();
			foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
			{
				var fileContent = File.ReadAllText(projectFile);
				if (fileContent.Contains("PackageReference") && fileContent.Contains("<TargetFramework>net8.0</TargetFramework>"))
				{
					results.Add(projectFile);
				}
			}
			Assert.True(!results.Any(), $"The following net8.0 project files use PackageReference, pls use paket.references instead {string.Join(Environment.NewLine, results)}");
		}

		[Test]
		public void CheckCSharpLanguageVersion()
		{
			var results = new List<string>();
			foreach (var projectFile in csProjFiles)
			{
				var fileContent = File.ReadAllText(projectFile);
				if (fileContent.Contains("<LangVersion>"))
				{
					results.Add(projectFile);
				}
			}
			Assert.True(!results.Any(), $"Please remove LanVersion to keep it related to SDK");
		}

		[Test]
		public void AssemblyNameAllowed()
		{
			var results = new List<string>();
			foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
			{
				var fileContent = File.ReadAllText(projectFile);
				var match = Regex.Match(fileContent, AssemblyName);
				if (!match.Success)
				{
					results.Add(projectFile);
				}
			}
			Assert.True(!results.Any(), $"The following project assembly names do not start with CargoWise.RefDbRepo {string.Join(Environment.NewLine, results)}");
		}

		[Test]
		public void FrameworkVersionAllowed()
		{
			foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
			{
				var fileContent = File.ReadAllText(projectFile);
				var match = Regex.Match(fileContent, RegexFrameworkVersion);
				if (match.Success)
				{
					var version = match.Groups[1].Value;
					Assert.True(IsValidVersion(version), $"{projectFile} is running a non valid framework version ({version}). It should be {CurrentVersion} or higher.");
				}
				match = Regex.Match(fileContent, TargetFramework);
				if (match.Success)
				{
					var version = match.Groups[1].Value;
					Assert.True(ValidFrameworkVersions.Contains(version), $"{projectFile} should only target framework for net472 or net8.0 or netstandard2.0");
				}
				match = Regex.Match(fileContent, TargetFrameworks);
				if (match.Success)
				{
					var versions = match.Groups[1].Value;
					Assert.True(versions.Contains(";"), $"{projectFile} should target multiple frameworks as it's using node TargetFrameworks");
					foreach (var version in versions.Split(';'))
					{
						Assert.True(ValidFrameworkVersions.Contains(version), $"{projectFile} should only target frameworks for net472, net8.0, netstandard2.0");
					}
				}
			}
		}

		[Test]
		public void CheckBuildConfigurations()
		{
			var testProjectRegex = "Test(s)?.(.*?)csproj";
			foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
			{
				var projectFilePath = Path.GetFullPath(projectFile);
				var fileContent = File.ReadAllText(projectFilePath);
				var match = Regex.Match(fileContent, ConfigurationRegex);
				Assert.True(match.Success, $"{projectFilePath} should have <Configurations>");

				var configurations = match.Groups[1].Value;
				var configurationArray = configurations.Split(";", StringSplitOptions.RemoveEmptyEntries);
				Assert.Greater(configurationArray.Length, 0, $"{projectFilePath} should have at least 1 build configuration");
				if (Regex.Match(projectFilePath, testProjectRegex).Success)
				{
					Assert.True(configurationArray.Length == 1 && configurationArray[0].Equals("Debug", StringComparison.OrdinalIgnoreCase), $"{projectFilePath} should have Debug build configuration only");
				}
				else
				{
					Assert.GreaterOrEqual(configurationArray.Length, 3, $"{projectFilePath} should have at least 3 build configurations");
					var expectedConfigs = new[] { "Debug", "Release", "UAT" };
					foreach (var expectedConfig in expectedConfigs)
					{
						Assert.True(configurationArray.Contains(expectedConfig, StringComparer.OrdinalIgnoreCase), $"{projectFilePath} should contain {expectedConfig} build configuration");
					}
				}
			}
		}

		[Test]
		public void CopyLocalAlwaysFalseNetFramework()
		{
			var errors = new List<string>();
			foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
			{
				var fileContent = File.ReadAllText(projectFile);
				var match = Regex.Match(fileContent, RegexFrameworkVersion);
				if (match.Success)
				{
					using (var reader = XmlReader.Create(new StreamReader(projectFile)))
					{
						while (reader.Read())
						{
							if (reader.IsStartElement() && reader.NodeType == XmlNodeType.Element && reader.Name == "Reference" && !reader.IsEmptyElement)
							{
								var elementEnd = false;
								var copyLocalExists = false;
								var referenceInclude = reader.GetAttribute("Include");
								while (reader.Read() && !elementEnd && !referenceInclude.StartsWith("CargoWise", StringComparison.InvariantCultureIgnoreCase))
								{
									if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Reference"))
									{
										if (reader.NodeType == XmlNodeType.Element && reader.Name == "Private")
										{
											if (reader.ReadElementContentAsString() != "False")
											{
												errors.Add($"{projectFile} contains references where Copy Local is not set to false ({referenceInclude ?? string.Empty})");
											}
											copyLocalExists = true;
										}
									}
									else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Reference")
									{
										if (!copyLocalExists)
										{
											errors.Add($"{projectFile} contains references where Copy Local is not set to false ({referenceInclude ?? string.Empty})");
										}
										elementEnd = true;
									}
								}
							}
						}
					}
				}
			}
			if (errors.Count != 0)
			{
				Assert.Fail($"Copy local not set to false for the following project(s): {string.Join(Environment.NewLine, errors)}");
			}
		}

		[Test]
		public void CheckAllReferencePath()
		{
			Assert.Multiple(() =>
			{
				foreach (var projectFile in csProjFiles.Where(x => !x.Contains("ThirdParty")))
				{
					var fileContent = XDocument.Load(projectFile);
					if (projectFile.EndsWith("Customization.Fody.csproj", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					var fodyDllPath = @"Common\Fody\netstandard2.0";
					var fodyDllName = "CargoWise.RefDbRepo.Common.Customization.Fody.dll";
					var hintPathElement = fileContent.Descendants("Reference").Select(r => r.Element("HintPath").Value);
					var fodyReference = hintPathElement.FirstOrDefault(x => x.EndsWith(fodyDllName, StringComparison.OrdinalIgnoreCase));
					if (fodyReference != null)
					{
						Assert.That(fodyReference.EndsWith($"{fodyDllPath}\\{fodyDllName}", StringComparison.OrdinalIgnoreCase), $"Pls reference Fody dll in {fodyDllPath} in project {projectFile}");
					}
					hintPathElement = hintPathElement.Where(x => !x.EndsWith(fodyDllName, StringComparison.OrdinalIgnoreCase));
					if (hintPathElement.Any())
					{
						Assert.That(hintPathElement.All(x => x.Contains("net8.0", StringComparison.OrdinalIgnoreCase)), $"All reference path in {projectFile} should be net8.0");
					}
				}
			});
		}

		[Test]
		public void CheckReferencePathForFodyProject()
		{
			var fodyProjectFile = csProjFiles.First(x => x.EndsWith("Customization.Fody.csproj", StringComparison.OrdinalIgnoreCase));
			var fileContent = XDocument.Load(fodyProjectFile);
			var hintPathElement = fileContent.Descendants("Reference").Select(r => r.Element("HintPath").Value);
			var fodyReferencePath = @"Common\Fody\netstandard2.0";
			Assert.That(hintPathElement.All(x => x.Contains(fodyReferencePath, StringComparison.OrdinalIgnoreCase)), $"All reference path in Customization.Fody.csproj should be {fodyReferencePath}");
		}

		[Test]
		public void ValidateAllJsonConfigFiles()
		{
			var incorrect = new List<string>();
			var options = default(JsonDocumentOptions);
			options.AllowTrailingCommas = false;
			foreach (var file in jsonFiles)
			{
				try
				{
					JsonObject.Parse(File.ReadAllText(file), null, options);
				}
				catch
				{
					incorrect.Add(file);
				}
			}
			Assert.AreEqual(0, incorrect.Count, $"The {incorrect.Count} following files incorrect format.{Environment.NewLine} {string.Join(Environment.NewLine, incorrect)}");
		}

		[Test]
		public void UseHttpsInConfigFiles()
		{
			foreach (var configFile in jsonFiles.Concat(configFiles))
			{
				var configs = File.ReadAllLines(configFile);
				var httpUrlList = new List<string>();
				foreach (var config in configs)
				{
					var configString = config.Trim().ToUpperInvariant();
					if (string.IsNullOrEmpty(configString) || configString.Length < 10)
					{
						continue;
					}
					if (configString.Contains("HTTP:") && (configString.Contains("REFDBREPO") || configString.Contains("***UPDATESERVICE***"))
						&& !configString.Contains("LOCALHOST") && !configString.Contains("127.0.0.1"))
					{
						var key = config.Trim().Substring(0, configString.IndexOf(":")).Replace("\"", string.Empty);
						httpUrlList.Add(key);
					}
				}
				Assert.AreEqual(0, httpUrlList.Count, $"Please use https in {configFile}: {string.Join(", ", httpUrlList)}");
			}
		}

		[Test]
		public void ConfigFilesDontHaveRuntimeConfiguration()
		{
			var listOfFilesWithRuntime = (from configFile in configFiles.Where(x =>
				!x.Contains("THIRDPARTY", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"\BIN\", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"\BINARIES\", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"\PACKAGES\", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"\OBJ\", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"SERVICE\SERVICE\WEB.CONFIG", StringComparison.OrdinalIgnoreCase)
				&& !x.Contains(@"WEB\REFERENCEDATAUPDATESERVICE.WEB\WEB.CONFIG", StringComparison.OrdinalIgnoreCase))
										  .OrderBy(x => x)
										  let fileContent = File.ReadAllText(configFile)
										  where fileContent.Contains("<runtime>")
										  select configFile).ToList();
			if (listOfFilesWithRuntime.Any())
			{
				Assert.Fail($"The {listOfFilesWithRuntime.Count} following files have <runtime> configuration.{Environment.NewLine}The current runtime configuration is done by Infrastructure/Utils/AssemblyResolver.cs class. {Environment.NewLine} {string.Join(Environment.NewLine, listOfFilesWithRuntime)}");
			}
		}

		[Test]
		public void TestPaketReferenceFilesAreValid_NUnit()
		{
			var results = new List<string>();
			foreach (var paketReferenceFile in paketReferenceFiles)
			{
				var projectPath = Path.GetDirectoryName(paketReferenceFile) ?? string.Empty;
				if (projectPath.EndsWith("Test", StringComparison.OrdinalIgnoreCase) || projectPath.EndsWith("Tests", StringComparison.OrdinalIgnoreCase))
				{
					var fileContent = File.ReadAllText(paketReferenceFile);
					bool containsNUnit = fileContent.Contains("NUnit", StringComparison.OrdinalIgnoreCase);
					bool containsNUnit3TestAdapter = fileContent.Contains("NUnit3TestAdapter", StringComparison.OrdinalIgnoreCase);
					bool containsMicrosoftNetTestSdk = fileContent.Contains("microsoft.net.test.sdk", StringComparison.OrdinalIgnoreCase);
					if (containsNUnit && containsNUnit3TestAdapter && containsMicrosoftNetTestSdk)
					{
						continue;
					}
					results.Add(paketReferenceFile);
				}
			}
			Assert.True(!results.Any(), $"The following paket reference files are missing some of these test assemblies: {{NUnit, NUnit3TestAdapter, microsoft.net.test.sdk}}{Environment.NewLine}{string.Join(Environment.NewLine, results)}");
		}

		[Test]
		public void TestPaketReferenceFilesAreValid_EFCore()
		{
			var results = new List<string>();
			foreach (var paketReferenceFile in paketReferenceFiles)
			{
				var projectPath = Path.GetDirectoryName(paketReferenceFile) ?? string.Empty;
				var fileContent = File.ReadAllText(paketReferenceFile);
				if (fileContent.Contains("Microsoft.EntityFrameworkCore"))
				{
					bool containsDesign = fileContent.Contains("Microsoft.EntityFrameworkCore.Design", StringComparison.OrdinalIgnoreCase);
					bool containsSqlServer = fileContent.Contains("Microsoft.EntityFrameworkCore.SqlServer\r\n", StringComparison.OrdinalIgnoreCase)
						|| fileContent.Contains("Microsoft.EntityFrameworkCore.SqlServer\n", StringComparison.OrdinalIgnoreCase)
						|| fileContent.Contains("Microsoft.EntityFrameworkCore.SqlServer ", StringComparison.OrdinalIgnoreCase);
					bool containsNetTopologySuite = fileContent.Contains("Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite", StringComparison.OrdinalIgnoreCase);
					if (containsDesign && containsSqlServer && containsNetTopologySuite)
					{
						continue;
					}
					results.Add(paketReferenceFile);
				}
			}
			Assert.True(!results.Any(), $"The following paket reference files are missing some of these assemblies: {{Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite}}{Environment.NewLine}{string.Join(Environment.NewLine, results)}");
		}

		[Test]
		public void TestAllDeployedProjectsShouldNotUseProjectReference()
		{
			var deliveredProjectsCsprojFiles = new List<string>
			{
				"NewService.csproj",
				"NewSafeDataUpdateService.csproj",
				"ReferenceDataUpdateService.Web.csproj"
			};
			var csprojFilesNeededToCheck = csProjFiles.Where(csprojFilePath =>
				{
					var csprojFileName = Path.GetFileName(csprojFilePath);
					return deliveredProjectsCsprojFiles.Contains(csprojFileName, StringComparer.OrdinalIgnoreCase);
				});
			Assert.That(csprojFilesNeededToCheck.Count(), Is.EqualTo(4));

			var results = new List<string>();
			foreach (var projectFile in csprojFilesNeededToCheck)
			{
				var fileContent = File.ReadAllText(projectFile);
				if (fileContent.Contains("ProjectReference", StringComparison.OrdinalIgnoreCase))
				{
					results.Add(projectFile);
				}
			}
			Assert.True(!results.Any(), $@"The following deployed project use project reference, pls use dll reference instead:
{string.Join(Environment.NewLine, results)}");
		}

		[Test]
		public void AllConfigFilesHaveEmptyPasswords()
		{
			var credentialKeyWords = new[] { "password", "credential", "Passcode", "Security" };
			foreach (var configJsonFile in jsonFiles)
			{
				var configFileName = Path.GetFileName(configJsonFile);
				var jsonContent = File.ReadAllText(configJsonFile);
				var jsonContentObject = JObject.Parse(jsonContent);
				CheckConfigObject(jsonContentObject, configFileName, credentialKeyWords);
			}
		}

		void CheckConfigObject(JObject configObject, string configFileName, string[] credentialKeyWords)
		{
			foreach (var configItem in configObject.Properties())
			{
				switch (configItem.Value)
				{
					case JObject nestedObject:
						CheckConfigObject(nestedObject, configFileName, credentialKeyWords);
						break;
					case JValue value:
						{
							if (!configFileName.Contains("test", StringComparison.OrdinalIgnoreCase) && credentialKeyWords.Any(keyword => configItem.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) && !passwordShouldEmptyIgnoreList.Contains(configFileName))
							{
								Assert.That(string.IsNullOrEmpty(value.ToString(CultureInfo.InvariantCulture)), $"\"{configItem.Name}\" in {configFileName} should be empty, do not store secrets in config file, please use Credential type. For more details, check https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis/RefDataRepo.wiki/13255/Secure-Your-Passwords-on-Portal");
							}

							break;
						}
				}
			}
		}

		readonly IEnumerable<string> passwordShouldEmptyIgnoreList = Array.Empty<string>();

		static bool IsValidVersion(string version)
		{
			var currentVersion = CurrentVersion;
			if (version.Count(o => o == '.') == 1)
			{
				currentVersion = currentVersion.Substring(0, currentVersion.Length - 2);
			}

			return VersionToInt(version) >= VersionToInt(currentVersion);
		}

		static int VersionToInt(string version)
		{
			var v = version.Replace("v", "").Replace(".", "");
			return int.Parse(v, null);
		}

		const string CurrentVersion = "v4.7.2";
		const string RegexFrameworkVersion = "<TargetFrameworkVersion>(v.*?)</TargetFrameworkVersion>";
		const string TargetFramework = "<TargetFramework>([\\w\\d;.]*?)</TargetFramework>";
		const string TargetFrameworks = "<TargetFrameworks>([\\w\\d;.]*?)</TargetFrameworks>";
		const string AssemblyName = "<AssemblyName>CargoWise.RefDbRepo\\..*</AssemblyName>";
		const string ConfigurationRegex = "<Configurations>([A-Za-z;]+)</Configurations>";
		readonly string[] ValidFrameworkVersions = { "net472", "netstandard2.0", "net8.0" };

		static readonly string[] ignoreFoldersOnRootPath = new[] { @"\.GIT\", @"\OBJ\", @"\T4TESTRESOURCES" };
		static readonly string rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
		readonly string[] csProjFiles = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories).Where(x => !ignoreFoldersOnRootPath.Any(i => x.ToUpperInvariant().Contains(i))).ToArray();
		readonly string[] configFiles = Directory.GetFiles(rootPath, "*.config", SearchOption.AllDirectories).Where(x => !ignoreFoldersOnRootPath.Any(i => x.ToUpperInvariant().Contains(i))).ToArray();
		readonly string[] jsonFiles = Directory.GetFiles(rootPath, "deploy.*.config.json", SearchOption.AllDirectories).Concat(Directory.GetFiles(rootPath, "CargoWise.RefDbRepo.*.config.json", SearchOption.AllDirectories))
			.Where(x => !ignoreFoldersOnRootPath.Any(i => x.ToUpperInvariant().Contains(i))).ToArray();
		readonly string[] paketReferenceFiles = Directory.GetFiles(rootPath, "paket.references", SearchOption.AllDirectories)
			.Where(x => !ignoreFoldersOnRootPath.Any(i => x.ToUpperInvariant().Contains(i))).ToArray();

		[SetUp]
		public void SetUp()
		{
			if (csProjFiles.Length == 0 || jsonFiles.Length == 0)
			{
				var unavailableFiles = (csProjFiles.Any() ? "" : " CsProjFiles") + (jsonFiles.Any() ? "" : " JsonFiles");
				throw new InvalidOperationException($"Files{unavailableFiles} are not available for testing. RootPath is {rootPath}");
			}
		}
	}
}
