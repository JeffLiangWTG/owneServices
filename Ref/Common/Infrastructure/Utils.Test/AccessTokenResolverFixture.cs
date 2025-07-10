using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class AccessTokenResolverFixture
	{
		[Test]
		public void TestResolveAccessTokenFromAuthHeader()
		{
			var authHeader = string.Empty;
			var token = AccessTokenResolver.ResolveAccessToken(authHeader);
			Assert.True(string.IsNullOrEmpty(token));

			authHeader = "Basic abcdefg";
			token = AccessTokenResolver.ResolveAccessToken(authHeader);
			Assert.True(string.IsNullOrEmpty(token));

			authHeader = "Bearer abcdefg";
			token = AccessTokenResolver.ResolveAccessToken(authHeader);
			Assert.AreEqual("abcdefg", token);
		}
	}
}
