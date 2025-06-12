using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dat.Integration;
using Moq;
using NUnit.Framework;

namespace eHub.DatImplementation.Deployment.Tests
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "VS2022")]
	public class BuildDeployerFixtures
	{
		[Test]
		public void TestDeployOnDemand()
		{
			const string configString = @"Project=$(BinPath)\Deployment\Deployment\Deploy.proj;p:Server=Server;t:dat:deploy";
			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var logs = new StringBuilder();

			try
			{
				using (var writer = new StringWriter(logs))
				{
					Console.SetOut(writer);
					Console.SetError(writer);

					new BuildDeployer(new TaskLogger())
						.DeployOnDemand(
							"DEBUG",
							configString,
							TestContext.CurrentContext.TestDirectory,
							TestContext.CurrentContext.TestDirectory);
				}
			}
			catch (Exception ex)
			{
				Assert.Fail($"{ex}{Environment.NewLine}{logs}");
			}
			finally
			{
				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}

			var output = logs.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			Assert.IsTrue(output.Contains("Starting\tDeployOnDemand"));
			Assert.IsTrue(output.Contains("0 Error(s)"));
			Assert.IsFalse(output.Any(line =>
				BuildDeployer.ErrorOutputLines.Any(
					pattern => Regex.Match(line, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Success)));
		}

		[Test]
		public void TestDeployOnDemand_NuGet_OneFile()
		{
			var localPackageSource = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(localPackageSource);
			var tempBinFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempBinFolder);

			try
			{
				// need to upload nupkg files as zip files since .gitignore nupkg, and then rename back during test
				foreach (var zipfile in Directory.EnumerateFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Deployment\Deployment"), "*.zip"))
				{
					var newFile = Path.Combine(tempBinFolder, Path.GetFileName(zipfile).Replace("zip", "nupkg"));
					File.Copy(zipfile, newFile);
				}

				var nugetInfoMock = new Mock<INugetInfo>();
				nugetInfoMock.Setup(x => x.ApiKey).Returns("whatever");
				nugetInfoMock.Setup(x => x.PackageSource).Returns(localPackageSource);

				var logger = new TestLogger();
				new BuildDeployer(logger, nugetInfoMock.Object).DeployOnDemand(
					"don't care",
					@"BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_310.Interface.*.nupkg",
					"any source path",
					tempBinFolder);

				Assert.AreEqual($@"Starting	DeployOnDemand
buildConfiguration: don't care, deploymentConfiguration: BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_310.Interface.*.nupkg, sourcePath: any source path, binPath: {tempBinFolder}
Pushing XH.CS.WYO.Uni_2_X12_310.Interface.2.1.1.nupkg to '{localPackageSource}'...
warn : The option to skip duplicates is not currently supported for this type of push.
Your package was pushed.

Finished	DeployOnDemand
", logger.Logs.ToString());

				var actualFiles = Directory.GetFiles(localPackageSource);
				Assert.AreEqual(1, actualFiles.Length);
				Assert.AreEqual("XH.CS.WYO.Uni_2_X12_310.Interface.2.1.1.nupkg", Path.GetFileName(actualFiles[0]));
			}
			finally
			{
				Directory.Delete(localPackageSource, true);
				Directory.Delete(tempBinFolder, true);
			}
		}

		[Test]
		public void TestDeployOnDemand_NuGet_TwoFiles()
		{
			var localPackageSource = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(localPackageSource);
			var tempBinFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempBinFolder);

			try
			{
				// need to upload nupkg files as zip files since .gitignore nupkg, and then rename back during test
				foreach (var zipfile in Directory.EnumerateFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Deployment\Deployment"), "*.zip"))
				{
					var newFile = Path.Combine(tempBinFolder, Path.GetFileName(zipfile).Replace("zip", "nupkg"));
					File.Copy(zipfile, newFile);
				}

				var nugetInfoMock = new Mock<INugetInfo>();
				nugetInfoMock.Setup(x => x.ApiKey).Returns("whatever");
				nugetInfoMock.Setup(x => x.PackageSource).Returns(localPackageSource);

				var logger = new TestLogger();
				new BuildDeployer(logger, nugetInfoMock.Object).DeployOnDemand(
					"don't care",
					@"BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg",
					"any source path",
					tempBinFolder);

				var logs = logger.Logs.ToString();

				Assert.True(logs.Contains($@"Starting	DeployOnDemand
buildConfiguration: don't care, deploymentConfiguration: BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg, sourcePath: any source path, binPath: {tempBinFolder}"));
				Assert.True(logs.Contains($@"Pushing XH.CS.WYO.Uni_2_X12_310.Interface.2.1.1.nupkg to '{localPackageSource}'...
warn : The option to skip duplicates is not currently supported for this type of push.
Your package was pushed."));
				Assert.True(logs.Contains($@"Pushing XH.CS.WYO.Uni_2_X12_315.Interface.1.0.0.nupkg to '{localPackageSource}'...
warn : The option to skip duplicates is not currently supported for this type of push.
Your package was pushed."));
				Assert.True(logs.Contains($@"Finished	DeployOnDemand"));

				var actualFiles = Directory.GetFiles(localPackageSource).Select(s => Path.GetFileName(s));
				Assert.AreEqual(2, actualFiles.Count());
				Assert.True(actualFiles.Contains("XH.CS.WYO.Uni_2_X12_310.Interface.2.1.1.nupkg"));
				Assert.True(actualFiles.Contains("XH.CS.WYO.Uni_2_X12_315.Interface.1.0.0.nupkg"));
			}
			finally
			{
				Directory.Delete(localPackageSource, true);
				Directory.Delete(tempBinFolder, true);
			}
		}

		[Test]
		public void TestAutoDeployLatestBuild_NuGet_WrongDeploymentConfiguration()
		{
			var nugetInfoMock = new Mock<INugetInfo>();
			nugetInfoMock.Setup(x => x.ApiKey).Returns("whatever");
			nugetInfoMock.Setup(x => x.PackageSource).Returns("whatever");

			var logger = new TestLogger();
			var ex = Assert.Throws<NotSupportedException>(() => new BuildDeployer(logger, nugetInfoMock.Object).AutoDeployLatestBuild(
				"don't care",
				@"HeyBATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg",
				"any source path",
				TestContext.CurrentContext.TestDirectory));
			Assert.AreEqual(@"No supported project found from the DeploymentConfiguration: HeyBATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg", ex.Message);

			ex = Assert.Throws<NotSupportedException>(() => new BuildDeployer(logger, nugetInfoMock.Object).AutoDeployLatestBuild(
				"don't care",
				@"BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkgYo",
				"any source path",
				TestContext.CurrentContext.TestDirectory));
			Assert.AreEqual(@"No supported project found from the DeploymentConfiguration: BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkgYo", ex.Message);

			Assert.AreEqual($@"Starting	AutoDeployLatestBuild
buildConfiguration: don't care, deploymentConfiguration: HeyBATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg, sourcePath: any source path, binPath: {TestContext.CurrentContext.TestDirectory}
Finished	AutoDeployLatestBuild
Starting	AutoDeployLatestBuild
buildConfiguration: don't care, deploymentConfiguration: BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkgYo, sourcePath: any source path, binPath: {TestContext.CurrentContext.TestDirectory}
Finished	AutoDeployLatestBuild
", logger.Logs.ToString());
		}

		[Test]
		public void TestAutoDeployLatestBuild_NuGet_NoRepo()
		{
			var tempBinFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempBinFolder);

			try
			{
				// need to upload nupkg files as zip files since .gitignore nupkg, and then rename back during test
				foreach (var zipfile in Directory.EnumerateFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Deployment\Deployment"), "*.zip"))
				{
					var newFile = Path.Combine(tempBinFolder, Path.GetFileName(zipfile).Replace("zip", "nupkg"));
					File.Copy(zipfile, newFile);
				}

				var localPackageSource = "NoRepo";
				var nugetInfoMock = new Mock<INugetInfo>();
				nugetInfoMock.Setup(x => x.ApiKey).Returns("whatever");
				nugetInfoMock.Setup(x => x.PackageSource).Returns(localPackageSource);

				var logger = new TestLogger();
				var ex = Assert.Throws<Exception>(() =>
				new BuildDeployer(logger, nugetInfoMock.Object).AutoDeployLatestBuild(
					"don't care",
					@"BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg",
					"any source path",
					tempBinFolder));

				Assert.That(ex.Message, Does.Match(@"""nuget push"" command failed...
Exit Code 1 returned from XH.CS.WYO.Uni_2_X12_31(0|5).Interface.(1.0.0|2.1.1).nupkg.
Exit Code 1 returned from XH.CS.WYO.Uni_2_X12_31(0|5).Interface.(1.0.0|2.1.1).nupkg.
For more details, please check the output."));
				Assert.That(ex.Message, Does.Contain(@"""nuget push"" command failed..."));
				Assert.That(ex.Message, Does.Contain(@"Exit Code 1 returned from XH.CS.WYO.Uni_2_X12_315.Interface.1.0.0.nupkg."));
				Assert.That(ex.Message, Does.Contain(@"Exit Code 1 returned from XH.CS.WYO.Uni_2_X12_310.Interface.2.1.1.nupkg."));
				Assert.That(ex.Message, Does.Contain(@"For more details, please check the output."));

				var logs = logger.Logs.ToString();

				Assert.True(logs.Contains($@"Starting	AutoDeployLatestBuild
buildConfiguration: don't care, deploymentConfiguration: BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_31*.nupkg, sourcePath: any source path, binPath: {tempBinFolder}"));
				Assert.True(logs.Contains($@"error: The specified source '{localPackageSource}' is invalid. Provide a valid source."));
				Assert.True(logs.Contains($@"Finished	AutoDeployLatestBuild"));
			}
			finally
			{
				Directory.Delete(tempBinFolder, true);
			}
		}

		[Test]
		public void TestDeployOnDemand_ForBackupAssemblies_ShouldNotCallDeploy()
		{
			var buildDeployer = new Mock<BuildDeployer>(new TaskLogger()) { CallBase = true };

			buildDeployer.Setup(b => b.Deploy(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(), It.IsAny<string>()));

			buildDeployer.Object.DeployOnDemand(
				"DEBUG",
				BuildDeployer.ManualDeploymentString,
				TestContext.CurrentContext.TestDirectory,
				TestContext.CurrentContext.TestDirectory);

			buildDeployer.Verify(b => b.Deploy(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void TestAutoDeployLatestBuild()
		{
			const string configString = @"Project=$(BinPath)\Deployment\Deployment\Deploy.proj;p:Server=Server;t:dat:deploy";
			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var logs = new StringBuilder();

			try
			{
				using (var writer = new StringWriter(logs))
				{
					Console.SetOut(writer);
					Console.SetError(writer);

					new BuildDeployer(new TaskLogger())
						.AutoDeployLatestBuild(
							"DEBUG",
							configString,
							TestContext.CurrentContext.TestDirectory,
							TestContext.CurrentContext.TestDirectory);
				}
			}
			finally
			{
				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}

			var output = logs.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			Assert.IsTrue(output.Contains("Starting\tAutoDeployLatestBuild"));
			Assert.IsTrue(output.Contains("0 Error(s)"));
			Assert.IsFalse(output.Any(line =>
				BuildDeployer.ErrorOutputLines.Any(
					pattern => Regex.Match(line, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Success)));
		}

		[Test]
		public void TestDeployOnDemand_NuGet_ProGet()
		{
			var tempBinFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempBinFolder);

			try
			{
				// need to upload nupkg files as zip files since .gitignore nupkg, and then rename back during test
				foreach (var zipfile in Directory.EnumerateFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Deployment\Deployment"), "*.zip"))
				{
					var newFile = Path.Combine(tempBinFolder, Path.GetFileName(zipfile).Replace("zip", "nupkg"));
					File.Copy(zipfile, newFile);
				}

				var logger = new TestLogger();
				new BuildDeployer(logger).DeployOnDemand(
					"don't care",
					@"BATnuGet=$(BinPath)\XH.CS.WYO.Uni_2_X12_310.Interface.*.nupkg",
					"any source path",
					tempBinFolder);

				var logDetails = logger.Logs.ToString();
				Assert.IsTrue(logDetails.Contains("already exists at feed"), $"\"already exists at feed\" is ignored without throwing any error. Log details: {logDetails}");
			}
			finally
			{
				Directory.Delete(tempBinFolder, true);
			}
		}

		[Test]
		public void TestResolveEnvironmentalVariables()
		{
			var environmentalVariableNames = new[]
			{
				"APPDATA",
				"COMPUTERNAME",
				"OS",
				"ProgramData",
				"ProgramFiles",
				"SYSTEMDRIVE",
				"SYSTEMROOT",
				"TEMP",
				"USERDOMAIN",
				"USERNAME",
				"USERPROFILE",
				"WINDIR"
			};

			var environmentalVariables = new Dictionary<string, string>();
			foreach (var name in environmentalVariableNames)
			{
				environmentalVariables.Add("$(" + name + ")", Environment.GetEnvironmentVariable(name));
			}

			var computerNameKey = "$(COMPUTERNAME)";
			var computerNameValue = environmentalVariables[computerNameKey];

			foreach (var kv in environmentalVariables)
			{
				var text = $"The value is {kv.Key}, should not be different {computerNameKey}";
				var expected = $"The value is {kv.Value}, should not be different {computerNameValue}";
				var actual = BuildDeployer.ResolveEnvionmentalVariables(text);
				Assert.That(Is.Equals(expected, actual));
			}
		}

		[Test]
		public void TestAutoDeployLatestBuild_NuGet_NoDeploymentFiles()
		{
			var dummyRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var packageSource = Path.Combine(dummyRoot, "PackageSource");
			var tempBinFolder = Path.Combine(dummyRoot, "TempBinFolder");
			Directory.CreateDirectory(packageSource);
			Directory.CreateDirectory(tempBinFolder);

			// Don't need to create any files as this test is about not finding packages to deploy.

			try
			{
				var ex = Assert.Throws<FileNotFoundException>(() =>
				{
					var nugetInfoMock = new Mock<INugetInfo>();
					nugetInfoMock.Setup(x => x.ApiKey).Returns("whatever");
					nugetInfoMock.Setup(x => x.PackageSource).Returns(packageSource);

					var logger = new TestLogger();
					new BuildDeployer(logger, nugetInfoMock.Object).DeployOnDemand(
						@"don't care",
						@"BATNuGet=$(BinPath)\XH.Acc.NonExistent.zip.*.nupkg",
						@"any source path",
						tempBinFolder);
				});

				Assert.That(ex.Message, Is.EqualTo("""
                    No deployment files could be found. Please check the following "DeploymentConfiguration" attribute in Build.xml against the path in the downloadable build or the local build: "BATNuGet=$(BinPath)\XH.Acc.NonExistent.zip.*.nupkg"
                    """));
			}
			finally
			{
				Directory.Delete(dummyRoot, true);
			}
		}

		class TestLogger : ITaskLogger
		{
			public StringBuilder Logs { get; } = new StringBuilder();

			public void RecordInfo(string message)
			{
				Logs.AppendLine(message);
			}

			public IDisposable RecordTask(string taskInfo)
			{
				return new RecordingTask(taskInfo, Logs);
			}

			class RecordingTask : IDisposable
			{
				readonly string taskInfo;
				StringBuilder logs;

				public RecordingTask(string taskInfo, StringBuilder logs)
				{
					logs.AppendLine($"Starting\t{taskInfo}");
					this.logs = logs;
					this.taskInfo = taskInfo ?? string.Empty;
				}

				public void Dispose()
				{
					logs.AppendLine($"Finished\t{taskInfo}");
				}
			}
		}
	}
}
