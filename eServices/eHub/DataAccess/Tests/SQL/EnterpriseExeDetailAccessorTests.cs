using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class EnterpriseExeDetailAccessorTests
	{
		[Test]
		public void GeteHubDbConnection_CallsSharedDataAccess()
		{
			var sharedEnterpriseExeDetailAccessor = new Mock<eServices.eHubDataAccess.Integration.IEnterpriseExeDetailAccessor>();
			var eHubEnterpriseExeDetailAccessor = new CargoWise.eHub.DataAccess.Sql.EnterpriseExeDetailAccessor(sharedEnterpriseExeDetailAccessor.Object);
			eHubEnterpriseExeDetailAccessor
				.GetExeDetail("senderID");
			sharedEnterpriseExeDetailAccessor.Verify(x => x
				.GetExeDetail("senderID"), Times.Once);
		}

		[Test]
		public void GetLicenceType_CallsSharedDataAccess()
		{
			var sharedEnterpriseExeDetailAccessor = new Mock<eServices.eHubDataAccess.Integration.IEnterpriseExeDetailAccessor>();
			var eHubEnterpriseExeDetailAccessor = new CargoWise.eHub.DataAccess.Sql.EnterpriseExeDetailAccessor(sharedEnterpriseExeDetailAccessor.Object);
			eHubEnterpriseExeDetailAccessor
				.GetLicenceType("senderID");
			sharedEnterpriseExeDetailAccessor.Verify(x => x
				.GetLicenceType("senderID"), Times.Once);
		}
	}
}
