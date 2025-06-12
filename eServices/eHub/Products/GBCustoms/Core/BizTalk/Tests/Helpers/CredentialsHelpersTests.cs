using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers;
using NUnit.Framework;
using Rhino.Mocks;
using System;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers
{
	[TestFixture]
	public class CredentialsHelpersTests
	{
		[Test]
		public void TestGetAccessToken_PerProvider_CorrectToken()
		{
			// Arrange.

			var registrationTypePk = Guid.NewGuid();
			var clientSystemRegistrationPK = Guid.NewGuid();
			var clientSystemPK = Guid.NewGuid();

			var mockContext = MockRepository
				.GenerateMock<eHubTransactionsContext>()
				.WithRegistrationType(registrationTypePk, "GBCustomsAccessToken")
				.WithClientSystem(clientSystemPK, "ABCGHI")
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.CTC", "WrongToken", 0)
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.EM5", "CorrectToken", 0)
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.GVM", "WrongToken", 0);

			CredentialsHelpers.ContextFactory = () => mockContext;

			// Act.

			var token = CredentialsHelpers.GetAccessToken("ABCDEFGHI", "EmCs");

			// Assert.

			Assert.AreEqual("CorrectToken", token);
		}

		[Test]
		public void TestGetAccessToken_PerProvider_AcceptableToken()
		{
			// Arrange.

			var registrationTypePk = Guid.NewGuid();
			var clientSystemRegistrationPK = Guid.NewGuid();
			var clientSystemPK = Guid.NewGuid();

			var mockContext = MockRepository
				.GenerateMock<eHubTransactionsContext>()
				.WithRegistrationType(registrationTypePk, "GBCustomsAccessToken")
				.WithClientSystem(clientSystemPK, "ABCGHI")
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.CTC", "AcceptableToken", 0)
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.EM5", "InvalidStatusToken", 1)
				.WithClientSystemRegistration(clientSystemRegistrationPK, registrationTypePk, clientSystemPK, "GB123456798001.GVM", "AcceptableToken", 0);

			CredentialsHelpers.ContextFactory = () => mockContext;

			// Act.

			var token = CredentialsHelpers.GetAccessToken("ABCDEFGHI", "EmCs");

			// Assert.

			Assert.AreEqual("AcceptableToken", token);
		}
	}
}
