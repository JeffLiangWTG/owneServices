using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class HttpContextExtensionFixture
	{
		[Test]
		public void GetUserId()
		{
			var context = new DefaultHttpContext { User = null };
			Assert.Null(context.GetUserId());

			var principle = new Mock<IPrincipal>();
			var identity = new Mock<IIdentity>();
			identity.Setup(x => x.Name).Returns("XX");
			principle.Setup(x => x.Identity).Returns(identity.Object);
			context.User = new ClaimsPrincipal(principle.Object);
			Assert.AreEqual("XX", context.GetUserId());

			var claims = new[] { new Claim("user_name", "ABC.XYZ"), new Claim("unique_name", "WTG.ABC.XYZ") };
			context.User = new ClaimsPrincipal(new[] { new ClaimsIdentity(claims, "AA") });
			Assert.AreEqual("WTG.ABC.XYZ", context.GetUserId());
		}
	}
}
