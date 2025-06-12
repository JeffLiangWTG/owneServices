using System.ServiceModel;
using CargoWise.eHub.Common;
using CargoWise.eServices.Authentication.ServiceClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class eHubUserNamePasswordValidatorTests
	{
		[Test]
		public void TestValidateEHubClient_NotValidClient_ThrowsFaultException()
		{
			var validator = MockRepository.GenerateMock<eHubUserNamePasswordValidator>();
			validator.Stub(_ => _.ValidatePassword(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(false);

			Assert.Throws<FaultException>(() => validator.ValidateEHubClient("ENTXYZSRV", "password"));
		}


		[Test]
		public void TestValidateEHubClient_ValidatePasswordIsCalledWithEncryptedPassword()
		{
			var validator = MockRepository.GenerateMock<eHubUserNamePasswordValidator>();
			validator.Stub(_ => _.ValidatePassword("ENTXYZSRV", SHA512Encryptor.Encrypt("ENTXYZSRV" + "password")))
				.Return(true);

			validator.ValidateEHubClient("ENTXYZSRV", "password");

			validator.AssertWasCalled(_ => _.ValidatePassword("ENTXYZSRV", SHA512Encryptor.Encrypt("ENTXYZSRV" + "password")));
		}

		[Test]
		public void TestValidateEHubClient_ValidateSystemIsCalledWithCorrectArguments()
		{
			var validator = MockRepository.GenerateMock<eHubUserNamePasswordValidator>();
			var authWsApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			validator.Expect(_ => _.AuthWsApi).Return(authWsApi);
			authWsApi.Stub(_ => _.ValidateSystem("ENT", "SRV", "password"));

			validator.ValidateCW1System("ENTXYZSRV", "password");

			authWsApi.AssertWasCalled(_ => _.ValidateSystem("ENT", "SRV", "password"));
		}

		[Test]
		public void TestValidate_ValidateSystemIsCalled()
		{
			var authWsApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWsApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

			var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator1.Expect(_ => _.AuthWsApi).Return(authWsApi).Repeat.Any();

			validator1.Validate("ENTXYZSRV", "password");

			var validator2 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator2.Expect(_ => _.AuthWsApi).Return(authWsApi).Repeat.Any();

			validator2.Validate("ENT___SRV", "password");

			authWsApi.VerifyAllExpectations();
			validator1.VerifyAllExpectations();
			validator2.VerifyAllExpectations();
		}
	}
}
