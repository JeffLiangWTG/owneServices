using Dat.Integration;
using eHub.DatImplementation.NpmDeployment;
using Moq;
using NUnit.Framework;

namespace eHub.DatImplementation.Tests.NpmDeployment
{
    [TestFixture]
    public class BuildDeployerTests
    {
        [Test]
        public void TestDeployOnDemand_Success()
        {
            // Arrange
            const string deploymentConfiguration = "Dir=NpmDeployment\\TestProject;Command=npm run deploy";
            var mockLogger = new Mock<ITaskLogger>();

            var buildDeployer = new BuildDeployer(mockLogger.Object);

            // Act
            buildDeployer.DeployOnDemand(null, deploymentConfiguration, null, TestContext.CurrentContext.TestDirectory);

            // Assert
            mockLogger.Verify(x => x.RecordInfo("SUCCESS"), Times.Once);
        }

        [Test]
        public void TestDeployOnDemand_Fail_When_TestProjectDoesNotExist()
        {
            // Arrange
            const string deploymentConfiguration = "Dir=TestProjectDoesNotExist;Command=npm run deploy";
            var mockLogger = new Mock<ITaskLogger>();

            var buildDeployer = new BuildDeployer(mockLogger.Object);

            // Act && Assert
            Assert.That(() => buildDeployer.DeployOnDemand(null, deploymentConfiguration, null, TestContext.CurrentContext.TestDirectory),
                Throws.TypeOf<System.ComponentModel.Win32Exception>().With.Message.EqualTo("The directory name is invalid"));
        }

        [Test]
        public void TestDeployOnDemand_When_IncorrectCommand()
        {
            // Arrange
            const string deploymentConfiguration = "Dir=NpmDeployment\\TestProject;Command=npm run incorrectDeploy";
            var mockLogger = new Mock<ITaskLogger>();

            var buildDeployer = new BuildDeployer(mockLogger.Object);

            // Act && Assert
            Assert.That(() => buildDeployer.DeployOnDemand(null, deploymentConfiguration, null, TestContext.CurrentContext.TestDirectory),
                Throws.TypeOf<System.InvalidOperationException>().With.Message.EqualTo("Deployment failed with exit code 1"));
        }
    }
}
