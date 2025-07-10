using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Authentication;
using Enterprise.Registry.Business;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Rating.Web.Test.Authentication
{
	class OIDCValidationMiddlewareTest : TestCaseWithFactory
	{
		public void TestLoggerUserAccessTokenValidationAsync()
		{
			var logger = new LoggerExtendedForTest();
			var owinContext = PrepareOwinContext("eyJhbGciOiJSUzI1NiIsImtpZCI6IkpNSy1zaEdWd0dLZm02WURZSnRXbUxMdjRaR3pkZnlDUkQ0b0dWbk83azQiLCJ0eXAiOiJKV1QifQ.eyJ0aWQiOiI4YjQ5Mzk4NS1lMWI0LTRiOTUtYWRlNi05OGFjYWZkYmRiMDEiLCJjb21wYW55X2NvZGUiOiJXVEciLCJ1c2VyX25hbWUiOiJqYXkud2FuZyIsInVuaXF1ZV9uYW1lIjoiV1RHLmpheS53YW5nIiwic3ViIjoiODhkNTg4MDItMTRlZC00NWYxLWE0OTEtMGE4NDZlMzNhYWU3Iiwib2lkIjoiODhkNTg4MDItMTRlZC00NWYxLWE0OTEtMGE4NDZlMzNhYWU3IiwiYXpwIjoiMzQ5MmIxNTQtYThjZS00Y2UzLWE5NTYtNTExMjc2Y2JkOWU2IiwidmVyIjoiMS4wIiwiaWF0IjoxNjkxNDYxNTI4LCJhdWQiOiIzNDkyYjE1NC1hOGNlLTRjZTMtYTk1Ni01MTEyNzZjYmQ5ZTYiLCJleHAiOjE2OTE0NjUxMjgsImlzcyI6Imh0dHBzOi8vY2FyZ293aXNlYjJjMDEuYjJjbG9naW4uY29tLzFiMjBiODdlLWNlYmQtNDNjYy05N2JkLWJkZDQxYTJmNWNmMS92Mi4wLyIsIm5iZiI6MTY5MTQ2MTUyOH0.UpEUW1pcZJlvZSOCm03nVg02FVsHAJXUVM7TS5WAKLDOsUtl5HoLluDWKmnywsqfPEleW-TKsw0O7JpJ9gRQsPeDdXbnu1fESdpmyIxMCM1CaOJrBBXCeCFmJGoAaRNW2B6x0AobVkZwkQB66_ZHKoQka9YdHJJCSqXzZOtw_WpOw6BPhmvP4VUDvJ9m3wGO6mNam3qWna7n75vFHwXZe8c_x-Teax718bf89GWk9R3ZHqlXZYivQQLwaFAiY3QGyAfIqrH4sywALaINFJ5FFIgz-I-RckzTbg7pU_50RHPtPexaDp-tEWN4zhdQTDJojddWYnu1j15wtqjG1PvM1Q");
			var tokenAuthorizeAttribute = new OIDCValidationMiddlewareForTest(logger);

			var result = tokenAuthorizeAttribute.IsAuthorizedForTest(owinContext).Result;

			AssertEquals(false, result);
			AssertEquals(1, logger.GetAllLogs().Count());
			AssertEquals("OpenID Connect is not enabled.", logger.GetAllLogs().First());
			logger.Clear();

			var newConfig = new OIDCConfig() { IsOIDCEnabled = true, AuthorityURL = "https://invalid.com", ClientIdentifier = "3F899034-ABE1-4223-B08D-A9F306E13B36" };
			using (ObjectFactory.Substitute<IOIDCConfig>(newConfig))
			{
				result = tokenAuthorizeAttribute.IsAuthorizedForTest(owinContext).Result;

				AssertEquals(false, result);
				AssertEquals(2, logger.GetAllLogs().Count());
				AssertContains("Unable to obtain configuration from https://invalid.com, The remote name could not be resolved: 'invalid.com'", logger.GetAllLogs().First());
				AssertContains("No discovery document.", logger.GetAllLogs().Last());
			}
		}

		public void TestLoggerClientAccessTokenValidationAsync()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JWA";
			Factory.Save();

			var logger = new LoggerExtendedForTest();
			var httpActionContext = PrepareOwinContext("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIzZTk3NGYxZC05MzUzLTQwMDQtYTkxNi0xNGEzNTFhZjZlMDUiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxL3YyLjAiLCJpYXQiOjE2OTEzODU1MzcsIm5iZiI6MTY5MTM4NTUzNywiZXhwIjoxNjkxMzg5NDM3LCJhaW8iOiJBU1FBMi84VUFBQUFCMkhhcjZqOXJuNTg4dWxMcXd6ZzNCTFNRMHlJelB3b1pTMUxWdW9EMUhNPSIsImF6cCI6IjNlOTc0ZjFkLTkzNTMtNDAwNC1hOTE2LTE0YTM1MWFmNmUwNSIsImF6cGFjciI6IjIiLCJvaWQiOiIzZjdiMWNiYy01NWViLTQzODctODNkYy1mYTJlMzNlNmUyNzgiLCJyaCI6IjAuQVQ4QXozQktnR0ZLQ0VxUWpxTXJMUC1CTVIxUGx6NVRrd1JBcVJZVW8xR3ZiZ1ZBQUFBLiIsInJvbGVzIjpbIjE6ZGpsRFNFbDJjVTFFZEVwUE5XZHFNR3d2TVdaVVdqVkdTbGRoT0hBeGJEVkNhblY0VEdkdWNHdDJia1JKUWtKcldXRkdUbGRTYTI1NVNqUXlkVGRzWW10dmVIVjFaRzFOYmxKdWRsRkJiVkl2VjBoM2RVcHZaSGhoZUU4M0sxbGlPV1p5Vm5NeGJqaDZVRmR3VmtZd1QzZHNVMFJ0ZDJ0VE5FRjJPRVUwWnpCdlZVWnZPV3QwWm14MFdEYzRlbTFDYVRKemRtTTViak5xUTNCeFRGQlRTVVpVZWpCdE1uQnFTMHRLTjBSYWRsZHhkejA5UEM5TmIyUjEiLCIyOmJIVnpQanhGZUhCdmJtVnVkRDVCVVVGQ1BDOUZlSEJ2Ym1WdWRENDhMMUpUUVV0bGVWWmhiSFZsUGc9PSIsIjA6UEZKVFFVdGxlVlpoYkhWbFBqeE5iMlIxYkhWelBteDJiR3RRWTNoRk5GZEZSaXRvU0VaSkwzVm5PVU14VVRCMlRWRmlVV2xhYm5vM2FVUnFOVlp2WkRka1JWUlFOek4yUzBWM056aDFabXRhZFZwWE9YWlFVMHgxTmtveGEwOVBkSEJ6UnpSV1prOUhZVk5rT1U4MlMwRnZRVUZUWWpsbWNVOXZTRFpYT1RSMVlWcEpkVWhvVUhOS1NEVkdSRko0UW1GSlpYRlJTV1JyYkhOWlVHc3dlR1oxYzNVckswNHhNRTFSZFcxMVdXZ3JWVFJTY21OclprcEIiXSwic3ViIjoiM2Y3YjFjYmMtNTVlYi00Mzg3LTgzZGMtZmEyZTMzZTZlMjc4IiwidGlkIjoiODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxIiwidXRpIjoidnN3ZHVyeHFtVXF0N3EteFctRUdBQSIsInZlciI6IjIuMCJ9.W6JLsDp4NokosVLl6A5U0YoIAxCj125A7CiEJaJ3nUWhMQy1xjrxm720IWImqoRaO2ezisHjJbpixW_1BIDt2GFv--QwjFJbyWoGiESlegqD8CyuJ1Mwn-qgpmlktRPPsaHg4UUIp5I4wUc_Ggr6PguEw1yER-Mr6t3rzQmv0YFGmPAWt6fm2O3i7nJGJDA8lw4moMlaT7L8VtsRci-oHLuYh-t143NM7k2m4BrqxVLWpXJeG9pDcJVKYxdAZHnUHatG_2S-IM9MAFFxJX2XVSUvA2wjo3B6-sDBm7cyRX5FBK_8heqdirg4btJm6-Q5qIgc3GTdSqBb4ed5lpBzag");
			var tokenAuthorizeAttribute = new OIDCValidationMiddlewareForTest(logger);

			var result = tokenAuthorizeAttribute.IsAuthorizedForTest(httpActionContext).Result;

			AssertEquals(false, result);
			AssertEquals(1, logger.GetAllLogs().Count());
			AssertEquals("Registry 'AutoRating -> Rating Web Services -> Token For Rating Authentication' is not defined.", logger.GetAllLogs().First());
			logger.Clear();

			var ratingTokenAuthenticationCollection = new RatingTokenAuthenticationCollection { new RatingTokenAuthentication() { Endpoint = "https://invalid.com", ClientId = "3e974f1d-9353-4004-a916-14a351af6e05", StaffCode = "JWA" } };
			using (RatingDataRegistry.Instance.RatingTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ratingTokenAuthenticationCollection))
			{
				result = tokenAuthorizeAttribute.IsAuthorizedForTest(httpActionContext).Result;

				AssertEquals(false, result);
				AssertEquals(2, logger.GetAllLogs().Count());
				AssertContains("Unable to obtain configuration from https://invalid.com, The remote name could not be resolved: 'invalid.com'", logger.GetAllLogs().First());
				AssertContains("No discovery document.", logger.GetAllLogs().Last());
			}
		}

		public void TestClientAccessTokenTakenClientIdFromAzpRatherThanAud()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JWA";
			Factory.Save();

			var logger = new LoggerExtendedForTest();
			var httpActionContext = PrepareOwinContext("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiJiODA4YzZlMy1kNGNjLTQ3NTQtYTM2NS04M2EyMzU1YzVmN2YiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxL3YyLjAiLCJpYXQiOjE2OTQ1NzQzNjIsIm5iZiI6MTY5NDU3NDM2MiwiZXhwIjoxNjk0NTc4MjYyLCJhaW8iOiJFMkZnWUxEZHJlZTZTMFZPOEcxSVVrZkN4cysxVTc4bmFBWGNMRWxvWE5KOXYrSEJGRVlBIiwiYXpwIjoiMjM5ZTMyNTUtODdlZi00MjA2LThmMmQtNmUxY2M2YmJmYjQ2IiwiYXpwYWNyIjoiMiIsIm9pZCI6ImJjOGM2MThjLWUzZjEtNDU3NC04MDExLTQxN2E0YWU2ZjUzZCIsInJoIjoiMC5BVDhBejNCS2dHRktDRXFRanFNckxQLUJNZVBHQ0xqTTFGUkhvMldEb2pWY1gzOUFBQUEuIiwic3ViIjoiYmM4YzYxOGMtZTNmMS00NTc0LTgwMTEtNDE3YTRhZTZmNTNkIiwidGlkIjoiODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxIiwidXRpIjoiNVdGV0hwV1YwMHFYeTlhcXpzNEJBQSIsInZlciI6IjIuMCJ9.sr1j8Z-1xw_U-jgnTiFtZFoass1dG62j36QqNPGo3pWiAo67zF_id9GLM6FbqoEX6vz_PLGdUXUkFZUoJDG7XRbKod-6eZ0f43MVutVwkGWzNjnPYsNXk-sxgZk56do2gReGJn2t8nE5V8lqwd6o_fUtd9rqUAkG0UXvCZQSiNV8cof_BTpwMlfK202akgKM0SW8ups_40qh0JS090o3SwpDylVR4vqurEmD2-b0VTskvppcQ73G6cx7E9poG1P05bpncgTLNO3tnNobyJLVPL63QhICYpEm_WWo0Wp8pl51-P8iY3UXNcfxeJCxa_zsNK2dCRc2E5_251oAL6X_dw");

			var tokenAuthorizeAttribute = new OIDCValidationMiddlewareForTest(logger);
			var ratingTokenAuthenticationCollection = new RatingTokenAuthenticationCollection { new RatingTokenAuthentication() { Endpoint = "https://invalid.com", ClientId = "b808c6e3-d4cc-4754-a365-83a2355c5f7f", StaffCode = "JWA" } };
			using (RatingDataRegistry.Instance.RatingTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ratingTokenAuthenticationCollection))
			{
				var result = tokenAuthorizeAttribute.IsAuthorizedForTest(httpActionContext).Result;

				AssertEquals(false, result);
				AssertEquals(1, logger.GetAllLogs().Count());
				AssertContains("No matched 'AutoRating -> Rating Web Services -> Token For Rating Authentication' item for client id '239e3255-87ef-4206-8f2d-6e1cc6bbfb46'.", logger.GetAllLogs().First());
				logger.Clear();
			}

			ratingTokenAuthenticationCollection = new RatingTokenAuthenticationCollection { new RatingTokenAuthentication() { Endpoint = "https://invalid.com", ClientId = "239e3255-87ef-4206-8f2d-6e1cc6bbfb46", StaffCode = "JWA" } };
			using (RatingDataRegistry.Instance.RatingTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ratingTokenAuthenticationCollection))
			{
				var result = tokenAuthorizeAttribute.IsAuthorizedForTest(httpActionContext).Result;

				AssertEquals(false, result);
				AssertEquals(2, logger.GetAllLogs().Count());
				AssertContains("Unable to obtain configuration from https://invalid.com, The remote name could not be resolved: 'invalid.com'", logger.GetAllLogs().First());
				AssertContains("No discovery document.", logger.GetAllLogs().Last());
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			ConfigurationHelper.ClearConfigurationCache();
		}

		IOwinContext PrepareOwinContext(string accessToken)
		{
			var mockOwinContext = new Mock<IOwinContext>();
			var mockAuthentication = new Mock<IAuthenticationManager>();
			mockOwinContext.Setup(c => c.Authentication).Returns(mockAuthentication.Object);

			IHeaderDictionary x = new HeaderDictionary(new Dictionary<string, string[]>());
			x.Add("Authorization", new[] { $"Bearer {accessToken}" });
			var requestMock = new Mock<IOwinRequest>();
			requestMock.Setup(r => r.Headers).Returns(x);

			mockOwinContext.Setup(c => c.Request).Returns(requestMock.Object);

			return mockOwinContext.Object;
		}
	}

	class LoggerExtendedForTest : ILoggerExtended
	{
		internal LoggerExtendedForTest()
		{
			logger = new List<(LogType, string)>();
		}

		readonly List<(LogType, string)> logger;

		public IEnumerable<string> GetAllLogs() => logger.Select(log => log.Item2);

		public IEnumerable<string> GetErrorsAndWarnings()
		{
			return logger.Where(log => log.Item1 == LogType.Error || log.Item1 == LogType.Warning).Select(log => log.Item2);
		}

		public void Log(LogType type, string message)
		{
			logger.Add((type, message));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			logger.Add((type, message + ex.Message));
		}

		public void Clear()
		{
			logger.Clear();
		}
	}

	class OIDCValidationMiddlewareForTest : OIDCValidationMiddleware
	{
		public OIDCValidationMiddlewareForTest(ILoggerExtended logger) : base(logger) { }

		public async Task<bool> IsAuthorizedForTest(IOwinContext actionContext)
		{
			await Invoke(actionContext, () => Task.CompletedTask);
			return actionContext.Authentication.User != null;
		}
	}
}
