using System.Reflection;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner.Test
{
	[TestFixture]
	public class IssueReportingConfigProviderFixture
	{
		[Test]
		public void GetIssueReportingExitCodeValue_DbNotExists()
		{
			var repoMock = new Mock<IStagingRepository>();
			repoMock.Setup(x => x.DatabaseExists).Returns(false);
			var configProvider = new IssueReportingConfigProvider(repoMock.Object);
			var actualValue = configProvider.GetIssueReportingExitCodeValue(Assembly.GetExecutingAssembly().Location);
			Assert.AreEqual("7", actualValue);
		}

		[Test]
		public void GetIssueReportingExitCodeValue_DbExists()
		{
			var repoMock = new Mock<IStagingRepository>();
			repoMock.Setup(x => x.DatabaseExists).Returns(true);
			repoMock.Setup(x => x.Get<RefApplicationAttribute>()).Returns(new[] {
				new RefApplicationAttribute
				{
					RAA_AttributeName = "IssueReportingExitCode",
					RAA_ConfigFilePath = "CargoWise.RefDbRepo.Staging.ProcessorRunner.Test.config.json",
					RAA_Value = "1"
				}
			}.AsQueryable());
			var configProvider = new IssueReportingConfigProvider(repoMock.Object) { IsRunningTests = false };
			var actualValue = configProvider.GetIssueReportingExitCodeValue(Assembly.GetExecutingAssembly().Location);
			Assert.AreEqual("1", actualValue);
		}
	}
}
