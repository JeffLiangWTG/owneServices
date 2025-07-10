using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class TokenValidationHelperFixture
	{
		[Test]
		public async Task TestVerifyAccessToken()
		{
			ITokenValidationHelper validationHelper = new S2STrustTokenValidationHelper("", "");
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("12279B56-7612-4BC7-9D56-4AA565D716AF", DateTime.Now);
			var securityToken = await validationHelper.VerifyAccessTokenAsync(accessToken, ConfigurationHelper.ConfigurationManagerCache, It.IsAny<ILogger>(), CancellationToken.None);
			Assert.IsNull(securityToken);

			validationHelper = new OIDCTokenValidationHelper("", "", true);
			accessToken = AccessTokenGenerator.GenerateS2SAccessToken("E240B902-EAC7-482C-8CF6-11BE8D427069", DateTime.Now);
			securityToken = await validationHelper.VerifyAccessTokenAsync(accessToken, ConfigurationHelper.ConfigurationManagerCache, It.IsAny<ILogger>(), CancellationToken.None);
			Assert.IsNotNull(securityToken);
		}

		[Test]
		public void TestShouldHandleToken()
		{
			var s2sTokenValidationHelper = new S2STrustTokenValidationHelper("", "");
			var oidcTokenValidationHelper = new OIDCTokenValidationHelper("", "");

			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("12279B56-7612-4BC7-9D56-4AA565D716AF", DateTime.Now);
			Assert.That(s2sTokenValidationHelper.ShouldHandle(accessToken));
			Assert.That(!oidcTokenValidationHelper.ShouldHandle(accessToken));

			accessToken = AccessTokenGenerator.GenerateOIDCAccessToken("test name", DateTime.Now, "08F98D0F-B61B-4827-8567-CE1409E7D7DD");
			Assert.That(!s2sTokenValidationHelper.ShouldHandle(accessToken));
			Assert.That(oidcTokenValidationHelper.ShouldHandle(accessToken));
		}
	}
}
