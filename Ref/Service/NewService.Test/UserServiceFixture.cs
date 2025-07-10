using System.Security.Claims;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class UserServiceFixture
	{
		[Test]
		public void GetUserId_HttpContextAccessor()
		{
			var accessor = new Mock<IHttpContextAccessor>();
			accessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("UserA");
			accessor.Setup(x => x.HttpContext.User.Claims).Returns(new Claim[] { new Claim("azp", "123") });
			var userService = new UserService(accessor.Object);

			accessor.Setup(x => x.HttpContext.User.Identity.AuthenticationType).Returns(AuthType.BasicAuth);
			Assert.AreEqual("UserA", userService.GetUserId());

			accessor.Setup(x => x.HttpContext.User.Identity.AuthenticationType).Returns(AuthType.TokenAuth);
			Assert.AreEqual("123", userService.GetUserId());
		}
	}
}
