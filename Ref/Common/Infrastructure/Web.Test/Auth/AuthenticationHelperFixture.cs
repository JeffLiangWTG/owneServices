using System;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.eServices.Authentication.ServiceClient;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class AuthenticationHelperFixture
	{
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void GetClaimsPrincipal(bool isValid, bool disableAuthentication)
		{
			var scheme = "CW1";
			var apiMock = new Mock<IAuthWebserviceApi>();
			apiMock.Setup(x => x.ValidateSystem("hello", "world")).Returns(isValid);
			var helper1 = new AuthenticationHelper(apiMock.Object, scheme, disableAuthentication);
			Assert.IsNull(helper1.GetClaimsPrincipal(null));

			var encoding = Encoding.GetEncoding("iso-8859-1");
			var data = encoding.GetBytes("hello:world");
			var authStr = new AuthenticationHeaderValue("basic", Convert.ToBase64String(data)).ToString();
			var principal1 = helper1.GetClaimsPrincipal(authStr);
			apiMock.Verify(x => x.ValidateSystem("hello", "world"));
			if (isValid || disableAuthentication)
			{
				Assert.IsNotNull(principal1);
				Assert.That(principal1.Identity.Name, Is.EqualTo("hello"));
				Assert.True(principal1.Identity.IsAuthenticated);
			}
			else
			{
				Assert.IsNull(principal1);
			}

			var helper2 = new AuthenticationHelper(null, scheme, disableAuthentication);
			var principal2 = helper2.GetClaimsPrincipal(authStr);
			Assert.IsNotNull(principal2);
			Assert.That(principal2.Identity.Name, Is.EqualTo("hello"));
			Assert.True(principal2.Identity.IsAuthenticated);
		}
	}
}
