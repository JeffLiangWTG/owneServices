using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using GlowAuthenticationResult = CargoWise.Authentication.Primitives.AuthenticationResult;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlowTicketAuthenticationAttributeTest : TestCaseWithFactory
	{
		public void TestOnAuthorization_NoAuthorizationHeader()
		{
			var response = ExecuteRequest();
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketBarier()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not valid");
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, request);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.AbnormalFailure.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketGarbageData()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("AAAAA", "BBBBBB");
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, request);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketEncryptedWithDifferentKey()
		{
			var authenticationTicket = CreateValidTicket();
			var encryptionKey = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetNonZeroBytes(encryptionKey);
			}
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, request);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.AbnormalFailure.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketAuthenticatedWithDifferentKey()
		{
			var authenticationTicket = CreateValidTicket();
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetNonZeroBytes(encryptionKey);
			}
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, request);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.AbnormalFailure.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketEncryptedWithDifferentKeyAndAuthenticationKey()
		{
			var authenticationTicket = CreateValidTicket();
			var encryptionKey = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetNonZeroBytes(encryptionKey);
			}
			var hmacKey = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetNonZeroBytes(hmacKey);
			}
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute, request);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.AbnormalFailure.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketSessionLimitReached()
		{
			var authenticationTicket = CreateValidTicket();
			authenticationTicket.AuthenticationResult = GlowAuthenticationResult.SessionLimitReached;

			var response = ExecuteRequest(authenticationTicket);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.SessionLimitReached.ToString("G"));
		}

		public void TestOnAuthorization_InvalidTicketInvalidEndpoint()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid());
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/ABCDEF");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute() { StrictEndpoint = true };
			var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
			actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);

			var response = ExecuteFilterAndReturnResponse(attribute, actionContext);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.InvalidToken.ToString("G"));
		}

		public void TestOnAuthorization_ValidEndpoint()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid(), "/ABC");
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/ABC");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute() { StrictEndpoint = true };
			var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
			actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);

			var response = ExecuteFilterAndReturnResponse(attribute, actionContext);

			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestOnAuthorization_ShouldNotCheckEndpoint_WhenStrictEndpointIsFalse()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid());
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/XYZ");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute();
			var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
			actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);

			var response = ExecuteFilterAndReturnResponse(attribute, actionContext);

			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestOnAuthorization_GlowAuthenticationEncryptionKeyLengthNotMatch()
		{
			var sqlText = $"UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} SET {StmDataSchema.Constants.SD_BinaryValue} = 0xAD141D7E57AC1EF99AEECE2F79BF9886EAD3BD19D96EFE9B0AFCFBD58EDC82 WHERE {StmDataSchema.Constants.SD_Name} = '{GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name}'";
			using (Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sqlText))
			{
				Assert(cmd.ExecuteNonQuery() > 0);
			}

			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not valid");
			var attribute = new GlowTicketAuthenticationAttribute();

			var exception = AssertExceptionThrown<RegistryValidationException>(() => ExecuteFilterAndReturnResponse(attribute, request));
			AssertEquals(exception.Message, @"Key must be 32 bytes (64 hexadecimal characters), but was 62 hexadecimal characters.
ErrorValue: AD141D7E57AC1EF99AEECE2F79BF9886EAD3BD19D96EFE9B0AFCFBD58EDC82
Length of DatabaseValue: 62 hexadecimal characters");
			AssertEquals(exception.InnerException.Message, "Key must be 32 bytes (64 hexadecimal characters), but was 62 hexadecimal characters.");
		}

		public void TestOnAuthorization_GlowAuthenticationEncryptionKeyIsNull()
		{
			var sqlText = $"UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} SET {StmDataSchema.Constants.SD_BinaryValue} = NULL WHERE {StmDataSchema.Constants.SD_Name} = '{GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name}'";
			using (Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sqlText))
			{
				Assert(cmd.ExecuteNonQuery() > 0);
			}

			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not valid");
			var attribute = new GlowTicketAuthenticationAttribute();

			var exception = AssertExceptionThrown<RegistryValidationException>(() => ExecuteFilterAndReturnResponse(attribute, request));
			AssertEquals(exception.Message, @"Key must be 32 bytes (64 hexadecimal characters), but was 0 hexadecimal characters.
ErrorValue: 
DatabaseValue: null");
			AssertEquals(exception.InnerException.Message, "Key must be 32 bytes (64 hexadecimal characters), but was 0 hexadecimal characters.");
		}

		public void TestOnAuthorization_TicketExpired()
		{
			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				ExpiresAtUtc = ZDateTime.UtcNow.AddSeconds(-1).ToDateTime(),
				NotBeforeUtc = ZDateTime.UtcNow.AddSeconds(-2).ToDateTime(),
				IssuedAtUtc = ZDateTime.UtcNow.AddSeconds(-2).ToDateTime(),
			};

			var response = ExecuteRequest(authenticationTicket);
			var apiAuthResult = GetHeaderValue(response, ApiProxyConstants.Cw1AuthenticationResultHeaderName);
			var glowAuthResult = GetHeaderValue(response, GlowAuthenticationResultHeaderName);

			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(apiAuthResult, ApiProxyAuthenticationResult.SessionExpired.ToString("G"));
			AssertEquals(glowAuthResult, GlowAuthenticationResult.SessionExpired.ToString("G"));
		}

		public void TestOnAuthorization_ResponseNotSuccess()
		{
			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.SessionExpired,
				ExpiresAtUtc = ZDateTime.UtcNow.AddMinutes(1).ToDateTime(),
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		public void TestOnAuthorization_ResponseSuccess()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "User";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var dept = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var authenticationTicket = CreateValidTicket(branch.PK.ToGuid(), dept.PK.ToGuid());
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			var attribute = new GlowTicketAuthenticationAttribute();
			var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
			actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);

			var response = ExecuteFilterAndReturnResponse(attribute, actionContext);
			var identity = actionContext.RequestContext?.Principal?.Identity as GlowAuthenticationTicketIdentity;

			AssertNotNull(identity);
			AssertEquals(authenticationTicket.ProviderType, identity.ProviderType);
			AssertEquals("GlowTicket", identity.AuthenticationType);
			AssertEquals(authenticationTicket.ProviderKey, identity.ProviderKey);
			AssertEquals(authenticationTicket.Username, identity.Name);
			AssertEquals(authenticationTicket.InteropContextBranchKey, identity.BranchKey);
			AssertEquals(authenticationTicket.InteropContextDepartmentKey, identity.DepartmentKey);
			AssertEquals(true, identity.IsAuthenticated);
			AssertEquals(actionContext.Response, response);
		}

		public void TestOnAuthorization_TicketExpiryLaterThanNowMinusClockSkew()
		{
			ConfigurationManager.AppSettings["AuthenticationClockSkew"] = "00:00:10";

			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-9),
				NotBeforeUtc = DateTime.UtcNow.AddSeconds(-20),
				IssuedAtUtc = DateTime.UtcNow.AddSeconds(-20),
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestOnAuthorization_TicketExpiryEarlierThanNowMinusClockSkew()
		{
			ConfigurationManager.AppSettings[Constants.AuthClockSkewSettingName] = "00:00:10";

			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-11),
				NotBeforeUtc = DateTime.UtcNow.AddSeconds(-20),
				IssuedAtUtc = DateTime.UtcNow.AddSeconds(-20),
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		#region Implementation

		string originalClockSkew;

		protected override void SetUp()
		{
			base.SetUp();
			originalClockSkew = ConfigurationManager.AppSettings[Constants.AuthClockSkewSettingName];
		}

		protected override void TearDown()
		{
			base.TearDown();
			ConfigurationManager.AppSettings[Constants.AuthClockSkewSettingName] = originalClockSkew;
		}

		AuthenticationTicket CreateValidTicket(Guid? branchPK = null, Guid? departmentPK = null, string endpoint = "")
		{
			return new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				Username = "User",
				ProviderKey = Guid.NewGuid(),
				ProviderType = "PT",
				ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
				InteropContextBranchKey = branchPK ?? Guid.NewGuid(),
				InteropContextDepartmentKey = departmentPK ?? Guid.NewGuid(),
				Endpoint = endpoint
			};
		}

		HttpResponseMessage ExecuteRequest(AuthenticationTicket ticket = null)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			var attribute = new GlowTicketAuthenticationAttribute();

			if (ticket != null)
			{
				var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
				var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
				var validTicketValue = ticket.ToCookieValue(encryptionKey, hmacKey);
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", validTicketValue);
			}

			return ExecuteFilterAndReturnResponse(attribute, request);
		}

		static HttpResponseMessage ExecuteFilterAndReturnResponse(IAuthenticationFilter filter, HttpRequestMessage request)
		{
			return ExecuteFilterAndReturnResponse(
				filter,
				new HttpActionContext
				{
					ControllerContext = new HttpControllerContext { Request = request },
					Response = new HttpResponseMessage(HttpStatusCode.OK)
				});
		}

		static HttpResponseMessage ExecuteFilterAndReturnResponse(IAuthenticationFilter filter, HttpActionContext actionContext)
		{
			var authenticationContext = new HttpAuthenticationContext(actionContext, null);
			filter.AuthenticateAsync(authenticationContext, new CancellationToken()).GetAwaiter().GetResult();
			actionContext.RequestContext.Principal = authenticationContext.Principal;
			if (authenticationContext.ErrorResult != null)
			{
				return authenticationContext.ErrorResult.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
			}
			return authenticationContext.ActionContext.Response;
		}

		static string GetHeaderValue(HttpResponseMessage response, string headerName) => response.Headers.GetValues(headerName).FirstOrDefault();

		const string GlowAuthenticationResultHeaderName = "Glow-Authentication-Result";

		#endregion
	}
}
