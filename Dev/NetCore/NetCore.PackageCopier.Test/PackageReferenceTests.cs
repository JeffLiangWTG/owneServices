using System.Reflection;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace NetCore.PackageCopier.Test
{
	sealed class PackageReferenceTests : TestCase
	{
		const string ProjectPathErrorMessage = "ProjectPath path cannot be null or empty.";

		// This package is whitelisted for the following reasons:
		// 1. Does not contain any assemblies but only .targets/.props files.
		// 2. Only contains dependencies from other packages (e.g. bunit).
		// 3. Does not contain anything or empty - Microsoft.TestPlatform, MessagePackAnalyzer, NUnit.Analyzers
		readonly IEnumerable<string> whiteListedPackages = [
			"NUnit3TestAdapter",
			"Microsoft.TestPlatform",
			"Microsoft.NET.Test.Sdk",
			"bunit",
			"CargoWise.XmlSerializer.Generator",
			"Grpc.Tools",
			"NUnit",
			"Microsoft.Build",
			"MessagePackAnalyzer",
			"Microsoft.CodeAnalysis.NetAnalyzers",
			"Microsoft.VisualStudio.Threading.Analyzers",
			"NUnit.Analyzers",
			"QuickGetLatest.SignTool",
			"WTG.DevTools.CodeSigning",
			"WTG.Z.Blazor.Diagrams",
			"WTG.AppDomainWrappers.Core",
			"WTG.AppDomainWrappers.Net",
			"WTG.AppDomainWrappers.Runner",
			"WTG.BuildConfiguration.Analysis.SYSLIB"];

		// Handles scenarios where the
		// 1. Assembly name differs from the package name.
		// 2. More than 1 assembly is required to exist.
		// Dictionary Key = Package Name, Value = Assemblies Name.
		readonly Dictionary<string, IEnumerable<string>> assemblyNameIsNotSimilarWithPackageNameDict = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "Core.System.Configuration.Install", ["System.Configuration.Install.dll"] },
			{ "Core.System.ServiceProcess", ["System.ServiceProcess.Core.dll"] },
			{ "Microsoft.AspNet.WebApi.Client", ["System.Net.Http.Formatting.dll"] },
			{ "Serilog.Sinks.Confluent.Kafka", ["Serilog.Sinks.Kafka.dll"] },
			{ "Microsoft.TestPlatform.ObjectModel", ["Microsoft.VisualStudio.TestPlatform.ObjectModel.dll"] },
			{ "OxyPlot.Core", ["OxyPlot.dll"] },
			{ "WTG.Data.SqlClient.Both", ["WTG.Data.SqlClient.dll"] },
			{ "WTG.Data.SqlClient.System", ["WTG.Data.SqlClient.dll"] },
			{ "bblanchon.PDFium.Win32", ["runtimes\\win-x64\\native\\pdfium.dll", "runtimes\\win-x86\\native\\pdfium.dll"] },
			{ "Lib.Harmony", ["0Harmony.dll"] },
			{ "Swashbuckle.AspNetCore", ["Swashbuckle.AspNetCore.SwaggerGen.dll", "Swashbuckle.AspNetCore.Swagger.dll", "Swashbuckle.AspNetCore.SwaggerUI.dll"] }
		};

		protected override void SetUp()
		{
			if (Microsoft.Build.Locator.MSBuildLocator.CanRegister)
			{
				Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();
			}
		}

		// If this test fails, refer to the link for instructions and additional information.
		// https://github.com/WiseTechGlobal/Modernization.Content/blob/main/Services/netcore-packagecopier-package-update.md
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPackageReferencesShouldExistsInNetCoreBinFolder()
		{
			var projectPath = Path.Combine(BaseSourcePath, @"NetCore\NetCore.PackageCopier\NetCore.PackageCopier.csproj");

			if (!File.Exists(projectPath))
			{
				throw new FileNotFoundException($"Project does not exists or invalid : {projectPath}");
			}

			AssertPackageAssembliesExist(projectPath, whiteListedPackages);
		}

		void AssertPackageAssembliesExist(string projectPath, IEnumerable<string> whitelistedPackages)
		{
			if (string.IsNullOrWhiteSpace(projectPath))
			{
				throw new ArgumentException(ProjectPathErrorMessage);
			}

			try
			{
				using ProjectCollection projectCollection = new();
				var project = projectCollection.LoadProject(projectPath);

				// Create the ProjectInstance explicitly
				var projectInstance = project.CreateProjectInstance();

				// Execute the build
				var buildResult = BuildManager.DefaultBuildManager.Build(
					new BuildParameters(projectCollection),
					new BuildRequestData(projectInstance, []));

				// Get the package references from the project instance
				var packageReferences = projectInstance.Items
					.Where(item => item.ItemType.Equals("PackageReference", StringComparison.Ordinal)
						&& !whitelistedPackages.Contains(item.EvaluatedInclude))
					.Select(item => new
					{
						PackageName = item.EvaluatedInclude
					});

				var duplicates = packageReferences.GroupBy(a => a.PackageName).Where(g => g.Count() > 1);

				var binariesFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

				var assemblyIsMissing = false;
				var packageHasMissingAssemblies = false;

				CombineAssertions(delegate
				{
					foreach (var packageReference in packageReferences)
					{
						var reason = string.Empty;
						if (assemblyNameIsNotSimilarWithPackageNameDict.TryGetValue(packageReference.PackageName, out var assemblies))
						{
							var missingAssemblies = assemblies.Where(assembly => !File.Exists(Path.Combine(binariesFolder, assembly)));
							reason = $"Reason: One or more required assemblies [{string.Join(", ", missingAssemblies)}] are missing from the bin/net8.0 folder.";
							assemblyIsMissing = missingAssemblies.Any();
						}
						else
						{
							var assemblyName = $"{packageReference.PackageName}.dll";
							assemblyIsMissing = !File.Exists(Path.Combine(binariesFolder, assemblyName));
							reason = $"Reason: The required assembly {assemblyName} is missing from the bin/net8.0 folder.";
						}

						AssertEquals(message: $@"Package Reference Name : {packageReference.PackageName}{Environment.NewLine}{reason}", expected: false, actual: assemblyIsMissing);

						if (assemblyIsMissing)
						{
							packageHasMissingAssemblies = true;
						}
					}

					AssertEquals("Refer to this instruction to resolve the test error(s): https://github.com/WiseTechGlobal/Modernization.Content/blob/main/Services/netcore-packagecopier-package-update.md", expected: false, actual: packageHasMissingAssemblies);
				});
			}
			catch (Exception ex)
			{
				var message = $"Error processing project. Path: {projectPath}. Exception Message: {ex.Message}";
				throw new Exception(message, ex);
			}
		}
	}
}
