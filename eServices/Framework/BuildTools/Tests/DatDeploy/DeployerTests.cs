using System;
using System.Diagnostics;
using Dat.Integration;
using eServices.BuildTools.DatDeploy;
using Moq;
using NUnit.Framework;

namespace eServices.BuildTools.Tests.DatDeploy
{
	public class DeployerTests
	{
		[Test]
		public void DeployTest()
		{
			var deployer = new Deployer(new TaskLogger());
			Assert.Throws<ArgumentException>(() => deployer.Deploy(""));
		}

		[Test]
		public void DeployOnDemandTest()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = false };
			deployer.Object.DeployOnDemand("buildConfiguration", "deploymentConfiguration", "sourcePath", "binPath");
			deployer.Verify(d => d.Deploy("deploymentConfiguration", "sourcePath", "binPath", It.IsAny<DeployOptions>()));
		}

		[Test]
		public void AutoDeployLatestBuildTest()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = false };
			deployer.Object.AutoDeployLatestBuild("buildConfiguration", "deploymentConfiguration", "sourcePath", "binPath");
			deployer.Verify(d => d.Deploy("deploymentConfiguration", "sourcePath", "binPath", It.Is<DeployOptions>(o => o.MissingEnvironmentErrorAction == ErrorAction.Stop)));
		}

		[Test]
		public void AutoDeployTestedShelfTest()
			=> Assert.Throws<NotImplementedException>(() => new Deployer(new TaskLogger()).AutoDeployTestedShelf(null, null, null, null, null));

		[Test]
		public void TeardownTestedShelfTest()
			=> Assert.Throws<NotImplementedException>(() => new Deployer(new TaskLogger()).TeardownTestedShelf(null, null, null));

		[Test(Description = "A deployment of AutoDeployLatestBuild type is deployable in the current environment.")]
		public void Deploy_AutoDeploy_Valid()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Production.props", null));
			deployer.Setup(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProcessStartInfo>()));

			deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Production", "SOURCE_PATH", "BIN_PATH");

			deployer.Verify(d => d.ExecuteStep(It.IsAny<int>(), It.IsRegex(@"^DatDeploy\.TestDeploy_Production\.\d{8}\.\d{6}Z\.1\.cmd$"), "DEPLOY COMMAND", It.IsAny<ProcessStartInfo>()), Times.Once);
		}

		[Test(Description = "A deployment of AutoDeployLatestBuild type is NOT deployable in the current environment. Deployment should fail silently.")]
		public void Deploy_AutoDeploy_WrongEnvironment()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Default.props", null));
			deployer.Setup(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProcessStartInfo>()));

			deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Production", "SOURCE_PATH", "BIN_PATH");

			deployer.Verify(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProcessStartInfo>()), Times.Never);
		}

		[Test(Description = "A deployment running a Powershell script is successful.")]
		public void Deploy_Powershell_Success()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Default.props", null));
			var powershell = typeof(DeployerTests).GetResourceAsFile("Deploy_Powershell_Success.ps1");

			Assert.DoesNotThrow(() => deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Default", powershell.DirectoryName, "."));
		}

		[Test(Description = "A deployment running a Powershell script is unsuccessful.")]
		public void Deploy_Powershell_Failure()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Default.props", null));
			var powershell = typeof(DeployerTests).GetResourceAsFile("Deploy_Powershell_Failure.ps1");

			var ex = Assert.Throws<DeployerException>(() => deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Default", powershell.DirectoryName, "."));
			Assert.That(ex, Has.Message.EqualTo("Deployment failed at step 1. Exit code=1"));
		}

		[Test(Description = "Settings can be defined in the 'Settings' attribute of a target.")]
		public void Deploy_Settings_AsAttribute()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Production.props", null));
			deployer.Setup(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProcessStartInfo>()));

			deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Production", "SOURCE_PATH", "BIN_PATH");

			deployer.Verify(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<ProcessStartInfo>(x => x.EnvironmentVariables["SECRET_KEY"] == "SECRET DATA")), Times.Once);
		}

		[Test(Description = "Settings can be defined in the 'Settings' property element of a target.")]
		public void Deploy_Settings_AsProperty()
		{
			var deployer = new Mock<Deployer>(new TaskLogger()) { CallBase = true };
			deployer.Setup(d => d.OpenDeployFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("Build.Deploy.xml"));
			deployer.Setup(d => d.OpenBuildToolsPropsFile(It.IsAny<string>())).Returns(typeof(DeployerTests).GetResourceAsStream("eServices.BuildTools.Production.props", null));
			deployer.Setup(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProcessStartInfo>()));

			deployer.Object.AutoDeployLatestBuild("RELEASE", "TestDeploy-Production", "SOURCE_PATH", "BIN_PATH");

			deployer.Verify(d => d.ExecuteStep(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<ProcessStartInfo>(x => x.EnvironmentVariables["SECRET_KEY"] == "SECRET DATA")), Times.Once);
		}
	}
}
