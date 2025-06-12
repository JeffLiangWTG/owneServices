using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class EHubClientSystemAccessorTests
	{
		[Test]
		public void GeteHubDbConnection_CallsSharedDataAccess()
		{
			var sharedEHubClientSystemAccessor = new Mock<eServices.eHubDataAccess.Sql.EHubClientSystemAccessor>();
			var eHubEHubClientSystemAccessor = new CargoWise.eHub.DataAccess.Sql.EHubClientSystemAccessor(sharedEHubClientSystemAccessor.Object);
			eHubEHubClientSystemAccessor
				.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage("systemID", "url", Guid.Empty, "senderID");
			sharedEHubClientSystemAccessor.Verify(x => x
				.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage("systemID", "url", Guid.Empty, "senderID"), Times.Once);
		}
	}
}
