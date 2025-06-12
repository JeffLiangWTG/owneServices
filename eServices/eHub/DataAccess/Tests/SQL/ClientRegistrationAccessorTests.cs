using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	public class ClientRegistrationAccessorTests
	{
		[Test]
		public void UpdateRegistrationCode_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.UpdateRegistrationCode("value", (SqlTransaction)null, "client", "registrationType", "qualifier");
			sharedClientRegistrationAccessor.Verify(x => x
				.UpdateRegistrationCode("value", (SqlTransaction)null, "client", "registrationType", "qualifier"), Times.Once);
		}

		[Test]
		public void ReadRegistrations_ExternalConnection_NoFilter_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.ReadRegistrations((SqlTransaction)null, "client", "registrationType");
			sharedClientRegistrationAccessor.Verify(x => x
				.ReadRegistrations((SqlTransaction)null, "client", "registrationType"), Times.Once);
		}

		[Test]
		public void ReadRegistrations_ExternalConnection_FilterEqual_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.ReadRegistrations((SqlTransaction)null, "client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1);
			sharedClientRegistrationAccessor.Verify(x => x
				.ReadRegistrations((SqlTransaction)null, "client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1), Times.Once);
		}

		[Test]
		public void ReadRegistrations_InternalConnection_FilterEqual_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.ReadRegistrations("client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1);
			sharedClientRegistrationAccessor.Verify(x => x
				.ReadRegistrations("client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1), Times.Once);
		}

		[Test]
		public void ReadRegistrations_InternalConnection_FilterLike_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.ReadRegistrations("client", "registrationType", true, "qualifier", "code", "attr1", "password1", 1, 1);
			sharedClientRegistrationAccessor.Verify(x => x
				.ReadRegistrations("client", "registrationType", true, "qualifier", "code", "attr1", "password1", 1, 1), Times.Once);
		}

		[Test]
		public void ReadAttr1Password1FirstOrDefault_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.ReadAttr1Password1FirstOrDefault("client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1);
			sharedClientRegistrationAccessor.Verify(x => x
				.ReadAttr1Password1FirstOrDefault("client", "registrationType", "qualifier", "code", "attr1", "password1", 1, 1), Times.Once);
		}

		[Test]
		public void UpdateFlag1_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.UpdateFlag1("clientId", "registrationType", "qualifier", "code", 1);
			sharedClientRegistrationAccessor.Verify(x => x
				.UpdateFlag1("clientId", "registrationType", "qualifier", "code", 1), Times.Once);
		}

		[Test]
		public void UpdateConfigXml_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.UpdateConfigXml("clientId", "registrationType", "qualifier", "code", "xmlString");
			sharedClientRegistrationAccessor.Verify(x => x
				.UpdateConfigXml("clientId", "registrationType", "qualifier", "code", "xmlString"), Times.Once);
		}

		[Test]
		public void Insert_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.Insert("clientId", "registrationType", "qualifier", "code", 1, "xmlString", "attr1", "password1",
					DateTime.MinValue, DateTime.MaxValue);
			sharedClientRegistrationAccessor.Verify(x => x
				.Insert("clientId", "registrationType", "qualifier", "code", 1, "xmlString", "attr1", "password1",
					DateTime.MinValue, DateTime.MaxValue), Times.Once);
		}

		[Test]
		public void UpdateFlag1AndConfigXml_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.UpdateFlag1AndConfigXml("clientId", "registrationType", "qualifier", 1, "xmlString");
			sharedClientRegistrationAccessor.Verify(x => x
				.UpdateFlag1AndConfigXml("clientId", "registrationType", "qualifier", 1, "xmlString"), Times.Once);
		}

		[Test]
		public void Exists_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.Exists("clientId", "registrationType", "qualifier");
			sharedClientRegistrationAccessor.Verify(x => x
				.Exists("clientId", "registrationType", "qualifier"), Times.Once);
		}

		[Test]
		public void Delete_CallsSharedDataAccess()
		{
			var sharedClientRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientRegistrationAccessor>();
			var eHubClientRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientRegistrationAccessor(sharedClientRegistrationAccessor.Object);
			eHubClientRegistrationAccessor
				.Delete("clientId", "registrationType", "qualifier");
			sharedClientRegistrationAccessor.Verify(x => x
				.Delete("clientId", "registrationType", "qualifier"), Times.Once);
		}
	}
}
