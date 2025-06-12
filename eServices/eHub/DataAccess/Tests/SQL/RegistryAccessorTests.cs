using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class RegistryAccessorTests
	{
		[Test]
		public void SelectRegistryValue_CallsSharedDataAccess()
		{
			var sharedRegistryAccessor = new Mock<eServices.eHubDataAccess.Integration.IRegistryAccessor>();
			var eHubRegistryAccessor = new CargoWise.eHub.DataAccess.Sql.RegistryAccessor(sharedRegistryAccessor.Object);
			eHubRegistryAccessor
				.SelectRegistryValue("clientId", "applicationCode", "name");
			sharedRegistryAccessor.Verify(x => x
				.SelectRegistryValue("clientId", "applicationCode", "name"), Times.Once);
		}

		[Test]
		public void InsertMessageReference_CallsSharedDataAccess()
		{
			var sharedRegistryAccessor = new Mock<eServices.eHubDataAccess.Integration.IRegistryAccessor>();
			var eHubRegistryAccessor = new CargoWise.eHub.DataAccess.Sql.RegistryAccessor(sharedRegistryAccessor.Object);
			eHubRegistryAccessor
				.InsertMessageReference("clientId", "applicationCode", "messageReference");
			sharedRegistryAccessor.Verify(x => x
				.InsertMessageReference("clientId", "applicationCode", "messageReference"), Times.Once);
		}

		[Test]
		public void ResolveMessageReference_CallsSharedDataAccess()
		{
			var sharedRegistryAccessor = new Mock<eServices.eHubDataAccess.Integration.IRegistryAccessor>();
			var eHubRegistryAccessor = new CargoWise.eHub.DataAccess.Sql.RegistryAccessor(sharedRegistryAccessor.Object);
			eHubRegistryAccessor
				.ResolveMessageReference("reference", "applicationCode");
			sharedRegistryAccessor.Verify(x => x
				.ResolveMessageReference("reference", "applicationCode"), Times.Once);
		}

		[Test]
		public void SelectRegistryIsProd_CallsSharedDataAccess()
		{
			var sharedRegistryAccessor = new Mock<eServices.eHubDataAccess.Integration.IRegistryAccessor>();
			var eHubRegistryAccessor = new CargoWise.eHub.DataAccess.Sql.RegistryAccessor(sharedRegistryAccessor.Object);
			eHubRegistryAccessor
				.SelectRegistryIsProd("clientId", "applicationCode", "name");
			sharedRegistryAccessor.Verify(x => x
				.SelectRegistryIsProd("clientId", "applicationCode", "name"), Times.Once);
		}
	}
}
