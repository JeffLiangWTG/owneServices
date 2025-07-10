using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlowAuthenticationTicketIdentityTest : TestCaseWithFactory
	{
		public void TestNotAuthenticatedIdentity()
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(false);

			var result = identityMock.Object.GetContact(Factory);
			AssertNull("null when identity is not authenticated", result);
		}

		public void TestStaffIdentity()
		{
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns(GlbStaffSchema.Constants.Prefix);
			identityMock.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());

			var result = identityMock.Object.GetContact(Factory);
			AssertNull("null when identity is Staff", result);
		}

		public void TestContactIdentity()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var providerKey = contact.PK.ToGuid();
			var identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			identityMock.SetupGet(x => x.ProviderKey).Returns(providerKey);

			var result = identityMock.Object.GetContact(Factory);
			AssertEquals("loads contact when identity is Contact", result.PK, providerKey);
		}
	}
}
