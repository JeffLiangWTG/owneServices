using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class AccessTokenGeneratorFixture
	{
		[Test]
		public void TestGenerateS2SAccessToken()
		{
			const string clientId = "8367F1F1-AE02-4821-BAFF-6B428845941E";
			var expireDate = new DateTime(2024, 08, 13);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken(clientId, expireDate);
			Assert.IsNotNull(accessToken);

			var tokenReader = new JwtTokenReader(accessToken);
			Assert.That(clientId, Is.EqualTo(tokenReader.GetClaimValue(AuthClaimType.Azp)));
		}

		[Test]
		public void TestGenerateOIDCAccessToken()
		{
			const string uniqueName = "WTG.Test.User";
			var expireDate = new DateTime(2024, 08, 13);
			var accessToken = AccessTokenGenerator.GenerateOIDCAccessToken(uniqueName, expireDate);
			Assert.IsNotNull(accessToken);

			var tokenReader = new JwtTokenReader(accessToken);
			Assert.That(uniqueName, Is.EqualTo(tokenReader.GetClaimValue(AuthClaimType.UniqueName)));
		}
	}
}
