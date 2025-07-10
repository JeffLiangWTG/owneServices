using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using CargoWise.Blazor.Common;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test;

public class BuildOutputTest
{
	[Test, Explicit("WI00729199 will remove GenerateRuntimeConfigDevFile, .playwright generated before build since Microsoft.Playwright package will not copied to Bin folder")]
	public void NoPlaywrightFiles()
	{
		Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, ".playwright"), Does.Not.Exist);
	}

	/* Temperary disabled
	[Test]
	[Explicit("Test for AppServer Build, QGL with Release Configuration before running test")]
	public void TestPublishReadyToRunOnAppServer()
	{
		var projectName = "CargoWise.Winzor.AppServer";
		var testProjectPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? string.Empty;
		var projectPath = Path.GetFullPath(Path.Combine(testProjectPath, "..", "..", "Winzor", "Infrastructure", projectName));
		var outputPath = Path.Combine(Path.GetTempPath(), projectName, "Release");

		using var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"publish -c Release -p:DeployOnBuild=false -o {outputPath}",
				WorkingDirectory = projectPath,
				RedirectStandardError = true,
			},
		};
		process.Start();
		if (!process.WaitForExit(120_000))
		{
			process.Kill();
		}

		if (process.ExitCode != 0)
		{
			Assert.Fail($"dotnet publish failed with exit code {process.ExitCode}:\n{process.StandardError.ReadToEnd()}");
		}

		var dllPath = Directory.GetFiles(outputPath, $"{projectName}.dll", SearchOption.TopDirectoryOnly).First();
		var isDllR2R = IsAssemblyReadyToRun(dllPath);
		Assert.That(isDllR2R, Is.True, $"The published DLL at {dllPath} is not in R2R format.");
	}

	/// <summary>
	/// Check if the dll is R2R format.
	/// https://github.com/dotnet/runtime/blob/main/docs/design/coreclr/botr/readytorun-format.md#readytorun_headersignature
	/// Always set to 0x00525452 (ASCII encoding for RTR). The signature can be used to distinguish ReadyToRun images from other CLI images with ManagedNativeHeader (e.g. NGen images).
	/// </summary>
	static bool IsAssemblyReadyToRun(string dllPath)
	{
		const string signatureHex = "0x00525452";
		using (var peReader = new PEReader(File.OpenRead(dllPath)))
		{
			var peHeaders = peReader.PEHeaders;
			if (peHeaders.CorHeader != null)
			{
				var r2rHeaderDirectory = peHeaders.CorHeader.ManagedNativeHeaderDirectory;
				var r2rHeader = peReader.GetSectionData(r2rHeaderDirectory.RelativeVirtualAddress);
				if (!r2rHeader.Equals(default(PEMemoryBlock)))
				{
					var signature = Convert.ToUInt32(signatureHex, 16);

					var blobReader = r2rHeader.GetReader();
					var signatureFromReader = blobReader.ReadUInt32();
					return signatureFromReader == signature;
				}
			}
		}
		return false;
	}
	*/

	[Test]
	[Property("DAT:CapabilityRequirements", "SOURCE_CODE")]
	public void AppServerPublishDirectoryShouldNotIncludeIndirectReferencesUsedForRelease()
	{
		var publishDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(typeof(BuildOutputTest).Assembly.Location)), BuildFileSystem.AppServerBin.PublishDirectoryPath);
		var publishAssemblySet = Directory.GetFiles(publishDirectory, "*.dll", SearchOption.TopDirectoryOnly).Select(Path.GetFileNameWithoutExtension).ToHashSet();
		Assert.That(publishAssemblySet, Is.Not.Empty);

		var indirectReferencesFile = Path.Combine(BaseSourcePath, BuildFileSystem.AppServerProject.ProjectDirectoryPath, "Convert.Indirect.References.With.ReadyToRun.props");
		var xDocument = XDocument.Load(indirectReferencesFile);
		var referenceList = xDocument.Descendants("ItemGroup").Descendants("Reference").Attributes("Include").Select(attr => attr.Value).ToList();
		Assert.That(referenceList, Is.Not.Empty);

		foreach (var reference in referenceList)
		{
			Assert.That(publishAssemblySet, Does.Not.Contain(reference), $"We want to publish {reference}.dll in Release build for AppServer. If it is also used in Debug build, remove it from Convert.Indirect.References.With.ReadyToRun.props file.");
		}
	}

	//Loaded by Microsoft.SqlServer.Types for address verification
	[TestCase("SqlServerSpatial160.dll")]
	public void TestAppServerPublishDirectoryHasDll(string dll)
	{
		var publishDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(typeof(BuildOutputTest).Assembly.Location)), BuildFileSystem.AppServerBin.PublishDirectoryPath);
		var dlls = Directory.GetFiles(publishDirectory, "*.dll", SearchOption.AllDirectories)
			.Select(Path.GetFileName)
			.ToList();
		Assert.That(dlls, Does.Contain(dll));
	}

	static string BaseSourcePath
	{
		get
		{
			var baseSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
			if (string.IsNullOrEmpty(baseSourcePath))
			{
				baseSourcePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(BuildOutputTest).Assembly.Location)));
			}
			return baseSourcePath ?? string.Empty;
		}
	}
}
