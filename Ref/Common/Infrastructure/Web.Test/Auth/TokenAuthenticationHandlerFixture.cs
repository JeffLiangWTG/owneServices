using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Common.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class TokenAuthenticationHandlerFixture
	{
		[Test]
		public async Task AuthenticateAsync()
		{
			var request = new Mock<HttpRequest>();
			var context = new Mock<HttpContext>();
			context.SetupGet(x => x.Request).Returns(request.Object);

			var handler = new TokenAuthenticationHandler(logWrapper.Object, new List<ITokenValidationHelper> {tokenValidationHelper.Object});
			await handler.InitializeAsync(scheme, context.Object);

			request.SetupGet(x => x.Headers.Authorization).Returns(new StringValues("abc"));
			var result = await handler.AuthenticateAsync();
			Assert.AreEqual("Unauthorized: Invalid access token", result.Failure.Message);

			var token = AccessTokenGenerator.GenerateS2SAccessToken("abc", DateTime.Now);
			request.SetupGet(x => x.Headers.Authorization).Returns(new StringValues(token));
			tokenValidationHelper.Setup(x => x.ShouldHandle(token)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(token, ConfigurationHelper.ConfigurationManagerCache, It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult<JwtSecurityToken>(null));
			result = await handler.AuthenticateAsync();
			Assert.AreEqual("Unauthorized: Invalid access token", result.Failure.Message);

			token = AccessTokenGenerator.GenerateS2SAccessToken("abc", DateTime.Now);
			request.SetupGet(x => x.Headers.Authorization).Returns(new StringValues($"Bearer {token}"));
			tokenValidationHelper.Setup(x => x.ShouldHandle(token)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(token, ConfigurationHelper.ConfigurationManagerCache, It.IsAny<ILogger>(), CancellationToken.None)).Throws(new Exception("wrong user"));
			result = await handler.AuthenticateAsync();
			Assert.AreEqual("Unauthorized: Invalid access token", result.Failure.Message);
			log.Verify(x => x.Error(result.Failure.Message, It.Is<Exception>(e => e.Message == "wrong user")));

			token = AccessTokenGenerator.GenerateS2SAccessToken("test", DateTime.Now);
			request.SetupGet(x => x.Headers.Authorization).Returns(new StringValues($"Bearer {token}"));
			tokenValidationHelper.Setup(x => x.ShouldHandle(token)).Returns(true);
			tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(token, ConfigurationHelper.ConfigurationManagerCache, It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(new JwtSecurityToken()));
			result = await handler.AuthenticateAsync();
			Assert.True(result.Succeeded);
			Assert.NotNull(result.Ticket);
		}

		[Test]
		public async Task ChallengeAsync()
		{
			var context = new DefaultHttpContext();
			context.Response.Body = new MemoryStream(100);
			context.Response.StatusCode = (int)HttpStatusCode.OK;

			var handler = new TokenAuthenticationHandler(logWrapper.Object, new List<ITokenValidationHelper> {tokenValidationHelper.Object});
			await handler.InitializeAsync(scheme, context);
			Assert.DoesNotThrowAsync(async () => await handler.ChallengeAsync(new AuthenticationProperties()));
			Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
			Assert.True(context.Response.Headers.ContainsKey("WWW-Authenticate"));
			context.Response.Body.Position = 0;
			using (var reader = new StreamReader(context.Response.Body))
			{
				var responseBody = reader.ReadToEnd();
				Assert.AreEqual("Unauthorized: Invalid access token", responseBody);
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

			tokenValidationHelper = new Mock<ITokenValidationHelper>();
		}

		Mock<ILog> log;
		Mock<ILogWrapper> logWrapper;
		Mock<ITokenValidationHelper> tokenValidationHelper;
		AuthenticationScheme scheme = new AuthenticationScheme(AuthType.TokenAuth, AuthType.TokenAuth, typeof(TokenAuthenticationHandler));
	}
}
