using Dat.Integration;
using Dat.Integration.Deployment;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	internal class DATDeployerFixture
	{
		[Test]
		public void TeardownTestedShelfDoesNotLogDeletingWebsitesWhenNoWebSiteNameOption()
		{
			var logMock = new Mock<ITaskLogger>();
			var datDeployer = new DATDeployer(logMock.Object);

			datDeployer.TeardownTestedShelf("any", "any", "any", "any", new TaskInfo("shelf1", "me", "no website name"));

			logMock.Verify(x => x.RecordInfo(It.Is<string>(s => s.Contains("Deleting websites"))), Times.Never, "Should not delete websites without the TestRigWebSiteName option");
		}
	}
}
