using System;
using System.IO;
using System.Linq;
using System.Text;
using Dat.Integration;
using NUnit.Framework;

namespace eHub.DatImplementation.MavenDeployment.Tests
{
	[TestFixture]
	public class MavenBuildDeployerTests
	{
		[Test]
		[Property("DAT:CapabilityRequirements", "Maven")]
		public void TestDeployOnDemand_Success()
		{
			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var logs = new StringBuilder();
			var deploymentConfiguration = "BAT=$(BINPATH)\\MavenDeployment\\Deployment\\TestProject\\test.zip;Profile=test;Command=mvn clean validate";
			try
			{
				using (var writer = new StringWriter(logs))
				{
					Console.SetOut(writer);
					Console.SetError(writer);

					new BuildDeployer(new TaskLogger())
						.DeployOnDemand(
							null,
							deploymentConfiguration,
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
			Assert.IsTrue(output.Contains("[INFO] BUILD SUCCESS"));
		}

		[TestCase(@"Profile=test;Project=TestProject", "Command")]
		[TestCase(@"Command=mvn validate;Project=TestProject", "Profile")]
		[TestCase(@"Profile=test;Command=mvn 1", "Project")]
		public void TestDeployOnDemand_ThrowsError_IfDeploymentConfigMissingFlags(string deploymentConfig, string missingConfig)
		{
			Assert.Throws<NotSupportedException>(() =>
				new BuildDeployer(new TaskLogger())
					.DeployOnDemand(
						null,
						deploymentConfig,
						$@"{TestContext.CurrentContext.TestDirectory}\MavenDeployment\Deployment",
						$@"{TestContext.CurrentContext.TestDirectory}\MavenDeployment\Deployment"),
			$"No {missingConfig} found from the DeploymentConfiguration: {deploymentConfig}");
		}

		public void TestDeployOnDemand_ThrowsError_IfCommandFlagIsNotMaven()
		{
			var deploymentConfig = @"BAT=$(BinPath)\\test\\test.zip;Profile=test;Command=msbuild15 -t:dat:deploy";
			Assert.Throws<NotSupportedException>(() =>
				new BuildDeployer(new TaskLogger())
					.DeployOnDemand(
						null,
						deploymentConfig,
						$@"{TestContext.CurrentContext.TestDirectory}\MavenDeployment\Deployment",
						$@"{TestContext.CurrentContext.TestDirectory}\MavenDeployment\Deployment"),
			$"None Maven command found in DeploymentConfiguration: {deploymentConfig}");
		}
	}
}
