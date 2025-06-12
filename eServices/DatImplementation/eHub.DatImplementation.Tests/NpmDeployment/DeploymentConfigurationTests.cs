using eHub.DatImplementation.NpmDeployment;
using NUnit.Framework;

namespace eHub.DatImplementation.Tests.NpmDeployment
{
    [TestFixture]
    public class DeploymentConfigurationTests
    {
        [Test]
        public void TestCorrectDeploymentConfiguration()
        {
            // Arrange
            const string deploymentConfiguration = "Dir=TestProject;Command=npm run deploy";

            // Act
            var configuration = new DeploymentConfiguration(deploymentConfiguration);

            // Assert
            Assert.That(configuration.Directory, Is.EqualTo("TestProject"));
            Assert.That(configuration.Command, Is.EqualTo("npm run deploy"));
        }

        [Test]
        public void TestIncorrectDeploymentConfiguration()
        {
            // Arrange
            const string deploymentConfiguration = "key1=value1";

            // Act
            var configuration = new DeploymentConfiguration(deploymentConfiguration);

            // Assert
            Assert.That(configuration.Directory, Is.Null);
            Assert.That(configuration.Command, Is.Null);
        }
    }
}
