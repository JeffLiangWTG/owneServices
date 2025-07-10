/* Temperary disabled
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class BuildOutputTest
{
	[Test]
	[Explicit("Test for Release Build")]
	public void TestPublishReadyToRunOnSessionBroker()
	{
		var projectName = "CargoWise.Blazor.SessionBroker";
		var testProjectPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
		var projectPath = Path.GetFullPath(Path.Combine(testProjectPath, "..", "..", "Winzor", "Infrastructure", projectName));
		var outputPath = Path.Combine(Path.GetTempPath(), projectName, "Release");

		using var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"publish --no-dependencies -c Release -p:DeployOnBuild=false -o {outputPath}",
				WorkingDirectory = projectPath,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			},
		};
		process.Start();
		process.WaitForExit();

		if (process.ExitCode != 0)
		{
			Assert.Fail($"dotnet publish failed with exit code {process.ExitCode}:\n{process.StandardOutput.ReadToEnd()}\n{process.StandardError.ReadToEnd()}");
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
}
*/
