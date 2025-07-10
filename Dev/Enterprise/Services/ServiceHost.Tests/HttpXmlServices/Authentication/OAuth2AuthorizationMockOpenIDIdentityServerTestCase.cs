using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;

namespace Enterprise.Services.ServiceHost.Tests
{
	abstract class OAuth2AuthorizationMockOpenIDIdentityServerTestCase : TransactionedTestCase
	{
		protected MockOpenIDIdentityServer authServer;

		protected override void SetUp()
		{
			base.SetUp();
			authServer = new MockOpenIDIdentityServer();
		}

		protected override void TearDown()
		{
			authServer.Dispose();
			base.TearDown();
		}
	}
}
