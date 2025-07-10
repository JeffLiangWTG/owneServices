using System.Collections.Generic;
using System.Security.Claims;
using CargoWise.Blazor.SessionBroker.Authentication;
using CargoWise.Blazor.SessionBroker.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class CargoWiseAuthenticatorTest : TestWithDatabase
	{
		[Test]
		public void VerifyUserByOidcClaims(
			[Values(true, false)]
			bool expectResult)
		{
			var config = new DeserializedOIDCConfig
			{
				ClaimsMappings = new List<DeserializedOIDCClaimsMapping> {
					new DeserializedOIDCClaimsMapping
					{
						ClaimName = "unique_name",
						Identifier = "GlbStaff.GS_LoginName",
					},
					new DeserializedOIDCClaimsMapping
					{
						ClaimName = "user_code",
						Identifier = "GlbStaff.GS_EmailAddress"
					},
				}
			};

			IEnumerable<Claim> claims = new List<Claim>
			{
				new Claim("unique_name", "WTG.Test.Name"),
				new Claim("user_code", "email@test.test"),
				new Claim("company_code", "WTG")
			};

			var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithOIDCMock();
			dbAccessorMock.Setup(databaseAccessor =>
				databaseAccessor.ExecuteScalar<string>(
					It.IsAny<string>(),
					System.Data.CommandType.Text,
					It.IsAny<(string ParameterName, object Value)[]>()))
				.Returns(expectResult ? "WTG.Test.Name" : null);
			var authenticator = new CargoWiseAuthenticator(
				dbAccessorMock.Object,
				new AuthenticationDatabaseAccessor(dbAccessorMock.Object, NullLogger.Instance),
				ConfigWithOIDCSettingsHelper.WithRegistryAccessor(config),
				NullLogger.Instance,
				Mock.Of<ISiteOfflineChecker>());
			var result = authenticator.VerifyUserByOidcClaims(config, claims);
			Assert.That(result.isSuccess, Is.EqualTo(expectResult));
			if (expectResult)
			{
				Assert.That(result.userID, Is.EqualTo("WTG.Test.Name"));
			}
		}
	}
}
