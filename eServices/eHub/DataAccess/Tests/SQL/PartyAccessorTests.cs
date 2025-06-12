using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class PartyAccessorTests
	{
		[Test]
		public void ClientExists_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.ClientExists("id");
			sharedPartyAccessor.Verify(x => x
				.ClientExists("id"), Times.Once);
		}

		[Test]
		public void InsertClientEntry_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.InsertClientEntry("clientID", "friendlyName", "email", Guid.Empty);
			sharedPartyAccessor.Verify(x => x
				.InsertClientEntry("clientID", "friendlyName", "email", Guid.Empty), Times.Once);
		}

		[Test]
		public void InsertClientAndClientSystem_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.InsertClientAndClientSystem("clientID", "friendlyName", "email", Guid.Empty, "systemCategory");
			sharedPartyAccessor.Verify(x => x
				.InsertClientAndClientSystem("clientID", "friendlyName", "email", Guid.Empty, "systemCategory"), Times.Once);
		}

		[Test]
		public void GetClientDetails_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientDetails("clientID");
			sharedPartyAccessor.Verify(x => x
				.GetClientDetails("clientID"), Times.Once);
		}

		[Test]
		public void UpdateEmailAddress_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			var clientDetails = new CargoWise.eHub.DataAccess.Integration.ClientDetails("fullName", Guid.Empty, "email");
			eHubPartyAccessor
				.UpdateEmailAddress("clientID", clientDetails);
			sharedPartyAccessor.Verify(x => x
				.UpdateEmailAddress("clientID", It.Is<eServices.eHubDataAccess.Integration.ClientDetails>(c => c.FullName == "fullName" && c.EdiProdLink == Guid.Empty && c.Email == "email")), Times.Once);
		}

		[Test]
		public void GetEdiProdClientDetailsForSystem_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetEdiProdClientDetailsForSystem("clientID");
			sharedPartyAccessor.Verify(x => x
				.GetEdiProdClientDetailsForSystem("clientID"), Times.Once);
		}

		[Test]
		public void GetClientIDFromAirPIMA_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromAirPIMA("PIMAAddress", "serviceProviderID");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromAirPIMA("PIMAAddress", "serviceProviderID"), Times.Once);
		}

		[Test]
		public void GetClientIDFromAirlineCode_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromAirlineCode("airlineCode", "serviceProviderID");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromAirlineCode("airlineCode", "serviceProviderID"), Times.Once);
		}

		[Test]
		public void GetClientIDFromAS2Code_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromAS2Code("code");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromAS2Code("code"), Times.Once);
		}

		[Test]
		public void GetClientIDFromEmail_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromEmail("emailName", "emailAddress");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromEmail("emailName", "emailAddress"), Times.Once);
		}

		[Test]
		public void GetClientIDFromClientPIMA_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromClientPIMA("clientPIMA", "serviceProviderID");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromClientPIMA("clientPIMA", "serviceProviderID"), Times.Once);
		}

		[Test]
		public void IsCW1System_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.IsCW1System("clientID");
			sharedPartyAccessor.Verify(x => x
				.IsCW1System("clientID"), Times.Once);
		}
		
		[Test]
		public void IsUnrestrictedClient_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.IsUnrestrictedClient("clientID");
			sharedPartyAccessor.Verify(x => x
				.IsUnrestrictedClient("clientID"), Times.Once);
		}

		[Test]
		public void IsLegacyXmlAllowedClient_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.IsLegacyXmlAllowedClient("clientID");
			sharedPartyAccessor.Verify(x => x
				.IsLegacyXmlAllowedClient("clientID"), Times.Once);
		}

		[Test]
		public void IsXHubSystem_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.IsXHubSystem("clientID");
			sharedPartyAccessor.Verify(x => x
				.IsXHubSystem("clientID"), Times.Once);
		}

		[Test]
		public void IsXHSystem_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.IsXHSystem("clientID");
			sharedPartyAccessor.Verify(x => x
				.IsXHSystem("clientID"), Times.Once);
		}

		[Test]
		public void GetClientIDFromClientAWB_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientIDFromClientAWB("clientAWB", "serviceProviderID");
			sharedPartyAccessor.Verify(x => x
				.GetClientIDFromClientAWB("clientAWB", "serviceProviderID"), Times.Once);
		}

		[Test]
		public void GetAirlineCodeFromPrefix_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetAirlineCodeFromPrefix("prefix");
			sharedPartyAccessor.Verify(x => x
				.GetAirlineCodeFromPrefix("prefix"), Times.Once);
		}

		[Test]
		public void GetClientFromAirPIMACount_CallsSharedDataAccess()
		{
			var sharedPartyAccessor = new Mock<eServices.eHubDataAccess.Integration.IPartyAccessor>();
			var eHubPartyAccessor = new CargoWise.eHub.DataAccess.Sql.PartyAccessor(sharedPartyAccessor.Object);
			eHubPartyAccessor
				.GetClientFromAirPIMACount("clientPIMA", "serviceProviderID");
			sharedPartyAccessor.Verify(x => x
				.GetClientFromAirPIMACount("clientPIMA", "serviceProviderID"), Times.Once);
		}
	}
}
