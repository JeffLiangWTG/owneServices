using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Common.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class BasicAuthenticationHandlerFixture
	{
		[Test]
		public async Task TestAuthenticateAsync()
		{
			var encoding = Encoding.GetEncoding("iso-8859-1");
			var data = encoding.GetBytes("hello:world");
			var authStr = new AuthenticationHeaderValue("basic", Convert.ToBase64String(data)).ToString();

			var context = new DefaultHttpContext();
			context.Request.Headers["Authorization"] = authStr;
			ClaimsPrincipal principal = null;
			authHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Returns(principal);

			var handler = new BasicAuthenticationHandler(logWrapper.Object, authHelper.Object);
			await handler.InitializeAsync(schema, context);
			var result = await handler.AuthenticateAsync();
			Assert.False(result.Succeeded);
			Assert.AreEqual("Unauthorized request", result.Failure.Message);

			principal = new ClaimsPrincipal(new GenericIdentity("hello", AuthType.BasicAuth));
			authHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Returns(principal);
			result = await handler.AuthenticateAsync();
			Assert.True(result.Succeeded);
			Assert.AreEqual(AuthType.BasicAuth, context.User.Identity.AuthenticationType);
			Assert.AreEqual("hello", context.User.Identity.Name);

			authHelper.Setup(x => x.GetClaimsPrincipal(It.IsAny<string>())).Throws(new Exception("an error"));
			result = await handler.AuthenticateAsync();
			Assert.False(result.Succeeded);
			log.Verify(x => x.Error(result.Failure.Message, It.Is<Exception>(e => e.Message == "an error")));
		}

		[Test]
		public async Task ChallengeAsync()
		{
			var context = new DefaultHttpContext();
			context.Response.Body = new MemoryStream(100);
			context.Response.StatusCode = (int)HttpStatusCode.OK;

			var handler = new BasicAuthenticationHandler(logWrapper.Object, authHelper.Object);
			await handler.InitializeAsync(schema, context);
			Assert.DoesNotThrowAsync(async () => await handler.ChallengeAsync(new AuthenticationProperties()));
			Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
			Assert.True(context.Response.Headers.ContainsKey("WWW-Authenticate"));
			context.Response.Body.Position = 0;
			using (var reader = new StreamReader(context.Response.Body))
			{
				var responseBody = reader.ReadToEnd();
				Assert.AreEqual("Unauthorized request", responseBody);
			}

			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("abcdefg123"));
			Assert.DoesNotThrowAsync(async () => await handler.ChallengeAsync(new AuthenticationProperties()));
			Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
			context.Response.Body.Position = 0;
			using (var reader = new StreamReader(context.Response.Body))
			{
				var responseBody = reader.ReadToEnd();
				Assert.AreEqual("abcdefg123", responseBody);
			}

			context.Response.StatusCode = (int)HttpStatusCode.OK;
			Assert.DoesNotThrowAsync(async () => await handler.ChallengeAsync(new AuthenticationProperties()));

			context.Response.Body = new MemoryStream(new byte[100], false);
			Assert.DoesNotThrowAsync(async () => await handler.ChallengeAsync(new AuthenticationProperties()));
			log.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>()));
		}

		[SetUp]
		public void Setup()
		{
			log = new Mock<ILog>();
			logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog(nameof(AuthenticationLogger))).Returns(log.Object);

			authHelper = new Mock<IAuthenticationHelper>();
		}

		Mock<ILog> log;
		Mock<ILogWrapper> logWrapper;
		Mock<IAuthenticationHelper> authHelper;
		AuthenticationScheme schema = new AuthenticationScheme(AuthType.BasicAuth, AuthType.BasicAuth, typeof(BasicAuthenticationHandler));
	}
}
