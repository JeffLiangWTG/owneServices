using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.NetCore;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using GlowAuthenticationResult = CargoWise.Authentication.Primitives.AuthenticationResult;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlowTicketAuthenticationAttributeTest : TestCaseWithFactory
	{
		Mock<IConfiguration> _mockConfiguration;
		Mock<IServiceProvider> _mockServiceProvider;
		DefaultHttpContext _httpContext;

		protected override void SetUp()
		{
			base.SetUp();
			_mockConfiguration = new Mock<IConfiguration>();
			_mockConfiguration.Setup(c => c["AuthClockSkew"]).Returns("300");
			_mockServiceProvider = new Mock<IServiceProvider>();
			_mockServiceProvider
				.Setup(sp => sp.GetService(typeof(IConfiguration)))
				.Returns(_mockConfiguration.Object);

			_httpContext = new DefaultHttpContext
			{
				RequestServices = _mockServiceProvider.Object
			};
		}

		public void TestOnAuthorization_NoAuthorizationHeader()
		{
			var response = ExecuteRequest();

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G") },
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketBarrier()
		{
			_httpContext.Request.Headers["Authorization"] = "Bearer not valid";
			var response = ExecuteFilterAndReturnResponse(new GlowTicketAuthenticationAttribute());

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.AbnormalFailure.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketGarbageData()
		{
			var response = ExecuteFilterAndReturnResponse(new GlowTicketAuthenticationAttribute());
			_httpContext.Request.Headers["Authorization"] = "AAAAA BBBBBB";

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.TokenNotProvided.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketEncryptedWithDifferentKey()
		{
			var authenticationTicket = CreateValidTicket();
			var encryptionKey = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetNonZeroBytes(encryptionKey);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);

			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";

			var response = ExecuteFilterAndReturnResponse(new GlowTicketAuthenticationAttribute());

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.AbnormalFailure.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketAuthenticatedWithDifferentKey()
		{
			var authenticationTicket = CreateValidTicket();
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = new byte[32];

			using var rng = RandomNumberGenerator.Create();
			rng.GetNonZeroBytes(encryptionKey);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";

			var response = ExecuteFilterAndReturnResponse(new GlowTicketAuthenticationAttribute());

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.AbnormalFailure.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketEncryptedWithDifferentKeyAndAuthenticationKey()
		{
			var encryptionKey = new byte[32];
			var hmacKey = new byte[32];

			var authenticationTicket = CreateValidTicket();
			using var rng = RandomNumberGenerator.Create();
			rng.GetNonZeroBytes(encryptionKey);
			rng.GetNonZeroBytes(hmacKey);

			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute);

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.AbnormalFailure.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketSessionLimitReached()
		{
			var authenticationTicket = CreateValidTicket();
			authenticationTicket.AuthenticationResult = GlowAuthenticationResult.SessionLimitReached;

			var response = ExecuteRequest(authenticationTicket);

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.SessionLimitReached.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_InvalidTicketInvalidEndpoint()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid());
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);

			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";
			_httpContext.Request.Scheme = "http";
			_httpContext.Request.Host = new HostString("unit-testing");
			_httpContext.Request.Path = "/ABCDEF";

			var attribute = new GlowTicketAuthenticationAttribute() { StrictEndpoint = true };

			var response = ExecuteFilterAndReturnResponse(attribute);

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.InvalidToken.ToString("G") },
				}
			);
		}

		public void TestOnAuthorization_ValidEndpoint()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid(), "/ABC");
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);

			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";
			_httpContext.Request.Scheme = "http";
			_httpContext.Request.Host = new HostString("unit-testing");
			_httpContext.Request.Path = "/ABC";

			var attribute = new GlowTicketAuthenticationAttribute() { StrictEndpoint = true };
			var response = ExecuteFilterAndReturnResponse(attribute);

			AssertEquals((int)HttpStatusCode.OK, response.HttpContext.Response.StatusCode);
		}

		public void TestOnAuthorization_ShouldNotCheckEndpoint_WhenStrictEndpointIsFalse()
		{
			var authenticationTicket = CreateValidTicket(Guid.NewGuid(), Guid.NewGuid());
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validTicketValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";
			var attribute = new GlowTicketAuthenticationAttribute();

			var response = ExecuteFilterAndReturnResponse(attribute);

			AssertEquals((int)HttpStatusCode.OK, response.HttpContext.Response.StatusCode);
		}

		public void TestOnAuthorization_GlowAuthenticationEncryptionKeyLengthNotMatch()
		{
			var sqlText = $"UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} SET {StmDataSchema.Constants.SD_BinaryValue} = 0xAD141D7E57AC1EF99AEECE2F79BF9886EAD3BD19D96EFE9B0AFCFBD58EDC82 WHERE {StmDataSchema.Constants.SD_Name} = '{GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Name}'";
			using (Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sqlText))
			{
				Assert(cmd.ExecuteNonQuery() > 0);
			}

			_httpContext.Request.Headers["Authorization"] = "Bearer not valid";
			var attribute = new GlowTicketAuthenticationAttribute();

			var exception = AssertExceptionThrown<RegistryValidationException>(() => ExecuteFilterAndReturnResponse(attribute));
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

			_httpContext.Request.Headers["Authorization"] = "Bearer not valid";
			var attribute = new GlowTicketAuthenticationAttribute();

			var exception = AssertExceptionThrown<RegistryValidationException>(() => ExecuteFilterAndReturnResponse(attribute));
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
				ExpiresAtUtc = ZDateTime.UtcNow.AddSeconds(-1).ToDateTime()
			};

			var response = ExecuteRequest(authenticationTicket);

			AssertUnauthorizedResponse
			(
				response,
				new Dictionary<string, string>
				{
					{ ApiProxyConstants.Cw1AuthenticationResultHeaderName, ApiProxyAuthenticationResult.SessionExpired.ToString("G") },
					{ GlowAuthenticationResultHeaderName, GlowAuthenticationResult.SessionExpired.ToString("G") }
				}
			);
		}

		public void TestOnAuthorization_ResponseNotSuccess()
		{
			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.SessionExpired,
				ExpiresAtUtc = ZDateTime.UtcNow.AddMinutes(1).ToDateTime(),
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertEquals((int)HttpStatusCode.Unauthorized, response.HttpContext.Response.StatusCode);
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
			_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";

			var response = ExecuteRequest(authenticationTicket);

			var identity = response.HttpContext?.User?.Identity as GlowAuthenticationTicketIdentity;

			AssertNotNull(identity);
			AssertEquals(authenticationTicket.ProviderType, identity.ProviderType);
			AssertEquals("GlowTicket", identity.AuthenticationType);
			AssertEquals(authenticationTicket.ProviderKey, identity.ProviderKey);
			AssertEquals(authenticationTicket.Username, identity.Name);
			AssertEquals(authenticationTicket.InteropContextBranchKey, identity.BranchKey);
			AssertEquals(authenticationTicket.InteropContextDepartmentKey, identity.DepartmentKey);
			AssertEquals(expected: true, identity.IsAuthenticated);
		}

		public void TestOnAuthorization_TicketExpiryLaterThanNowMinusClockSkew()
		{
			_mockConfiguration.Setup(c => c[Constants.AuthClockSkewSettingName]).Returns("00:00:10");

			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-9)
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertNull(response.Result);
		}

		public void TestOnAuthorization_TicketExpiryEarlierThanNowMinusClockSkew()
		{
			_mockConfiguration.Setup(c => c[Constants.AuthClockSkewSettingName]).Returns("00:00:10");

			var authenticationTicket = new AuthenticationTicket
			{
				AuthenticationResult = GlowAuthenticationResult.Success,
				ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-11)
			};

			var response = ExecuteRequest(authenticationTicket);
			AssertEquals((int)HttpStatusCode.Unauthorized, response.HttpContext.Response.StatusCode);
		}

		void AssertUnauthorizedResponse(AuthorizationFilterContext response, Dictionary<string, string> expectedHeaderTypeAndErrorMessage)
		{
			AssertEquals((int)HttpStatusCode.Unauthorized, response.HttpContext.Response.StatusCode);

			foreach (var dict in expectedHeaderTypeAndErrorMessage)
			{
				var apiAuthResult = GetHeaderValue(response.HttpContext, dict.Key);

				if (!string.IsNullOrEmpty(dict.Value))
				{
					AssertEquals(apiAuthResult, dict.Value);
				}
			}
		}

		AuthorizationFilterContext ExecuteRequest(AuthenticationTicket ticket = null)
		{
			var attribute = new GlowTicketAuthenticationAttribute();

			if (ticket != null)
			{
				var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
				var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
				var validTicketValue = ticket.ToCookieValue(encryptionKey, hmacKey);
				_httpContext.Request.Headers["Authorization"] = $"Bearer {validTicketValue}";
			}

			return ExecuteFilterAndReturnResponse(attribute);
		}

		AuthorizationFilterContext ExecuteFilterAndReturnResponse(IAsyncAuthorizationFilter filter)
		{
			var actionContext = new ActionContext
			{
				HttpContext = _httpContext,
				RouteData = new RouteData(),
				ActionDescriptor = new ActionDescriptor()
			};

			var authFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
			var attribute = new GlowTicketAuthenticationAttribute();

			filter.OnAuthorizationAsync(authFilterContext);

			if (authFilterContext.Result != null)
			{
				authFilterContext.Result.ExecuteResultAsync(actionContext);
			}

			return authFilterContext;
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

		public static string GetHeaderValue(HttpContext context, string headerName) => context.Response.Headers.TryGetValue(headerName, out var values) ? values.FirstOrDefault() : null;

		const string GlowAuthenticationResultHeaderName = "Glow-Authentication-Result";
	}
}
