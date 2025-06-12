using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	public class ClientSystemRegistrationAccessorTests
	{
		[Test]
		public void UpdateSystemRegistrationFlag1_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.UpdateSystemRegistrationFlag1("systemId", "registrationType", "qualifier", "code", 1);
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.UpdateSystemRegistrationFlag1("systemId", "registrationType", "qualifier", "code", 1), Times.Once);
		}

		[Test]
		public void UpdateSystemRegistrationConfigXml_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.UpdateSystemRegistrationConfigXml("systemId", "registrationType", "qualifier", "code", "xmlString");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.UpdateSystemRegistrationConfigXml("systemId", "registrationType", "qualifier", "code", "xmlString"), Times.Once);
		}

		[Test]
		public void GetSystemRegistrationFlag1_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.GetSystemRegistrationFlag1("systemId", "registrationType", "qualifier", "code");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.GetSystemRegistrationFlag1("systemId", "registrationType", "qualifier", "code"), Times.Once);
		}

		[Test]
		public void GetSystemRegistrationConfigXml_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.GetSystemRegistrationConfigXml("systemId", "registrationType", "qualifier", "code");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.GetSystemRegistrationConfigXml("systemId", "registrationType", "qualifier", "code"), Times.Once);
		}

		[Test]
		public void InsertSystemRegistration_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.InsertSystemRegistration("systemId", "registrationType", "qualifier", "code", 1, "xmlString", "attr1", "attr2", DateTime.MinValue, DateTime.MaxValue);
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.InsertSystemRegistration("systemId", "registrationType", "qualifier", "code", 1, "xmlString", "attr1", "attr2", DateTime.MinValue, DateTime.MaxValue), Times.Once);
		}

		[Test]
		public void UpdateSystemRegistrationFlag1AndConfigXml_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.UpdateSystemRegistrationFlag1AndConfigXml("systemId", "registrationType", "qualifier", "code", 1, "xmlString");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.UpdateSystemRegistrationFlag1AndConfigXml("systemId", "registrationType", "qualifier", "code", 1, "xmlString"), Times.Once);
		}

		[Test]
		public void DoesSystemRegistrationExist_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.DoesSystemRegistrationExist("systemId", "registrationType", "qualifier", "code");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.DoesSystemRegistrationExist("systemId", "registrationType", "qualifier", "code"), Times.Once);
		}

		[Test]
		public void GetSystemRegistrationsCode_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.GetSystemRegistrationsCode("systemId", "registrationType", "qualifier");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.GetSystemRegistrationsCode("systemId", "registrationType", "qualifier"), Times.Once);
		}

		[Test]
		public void DeleteSystemRegistration_CallsSharedDataAccess()
		{
			var sharedClientSystemRegistrationAccessor = new Mock<eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor>();
			var eHubClientSystemRegistrationAccessor = new CargoWise.eHub.DataAccess.Sql.ClientSystemRegistrationAccessor(sharedClientSystemRegistrationAccessor.Object);
			eHubClientSystemRegistrationAccessor
				.DeleteSystemRegistration("systemId", "registrationType", "qualifier", "code");
			sharedClientSystemRegistrationAccessor.Verify(x => x
				.DeleteSystemRegistration("systemId", "registrationType", "qualifier", "code"), Times.Once);
		}
	}
}
