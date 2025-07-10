using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.OpenIDConnect.Token;
using static Enterprise.Services.ServiceHost.Tests.AuthenticationTestHelper;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Enterprise.Services.ServiceHost.Tests
{
	class OAuth2AuthorizationGateManagementTest : OAuth2AuthorizationMockOpenIDIdentityServerTestCase
	{
		const string SampleJWTAccessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImNiZDhkOTRkLTNlNTYtNGFjMi1iZDVlLWVmZTI1NGM0NWMyYiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwNzQwYjAyNi1iN2E0LTQ4YWYtOGNmNC03MDE3MzA5YTMzZDkiLCJpYXQiOjE2NjM2MzA0MTAsInVzZXIiOiJDVzFTZXJ2aWNlIiwibmJmIjoxNjYzNjMwNDA4LCJleHAiOjE2NjM2MzE2MTAsImlzcyI6Imh0dHBzOi8vY3BnODNtMy53dGcuem9uZTo0OTIxNSIsImF1ZCI6IkNXMVNlcnZpY2UifQ.gn0nb1Ham8WwbRMYOfDaWqSy96_vrRduzx3lmD3A1fERqzIC9qX2osN5OeY2W0DPkJ6qkK93nce808H03fIH1aw-BM7HDQPr2M2XzDW60VoG_159aDVHaI2z_3F9_AZiLRJ_mH88sqLyxLQAvPzxl3ytOucra-wOLGbMQkaCrp3P4R5qyDhww4jC3psARKsBM7iNHQrAAkht7lBrUKZnOtEqgMlcinJn0ju4iS7qihinp15vJt38xe_fm2-u8jWY1I1a50Tn2nqMRVVXi05u0A3Eb1Mtt8lvGCsMGfs4YZzEq2sLvUN0D3iwRZXoLnZ6a4wI0RuTMyxrEg_fbYshog";

		HttpActionContext Authenticate(string scheme, string accessToken) => Authenticate(scheme, accessToken, CancellationToken.None);

		HttpActionContext Authenticate(string scheme, string accessToken, CancellationToken cancellationToken)
		{
			var authorization = new AuthenticationHeaderValue(scheme, accessToken);
			return AuthenticationTestHelper.Authenticate(authorization, cancellationToken, () => new GateManagementOAuth2AuthorizationAttribute());
		}

		void AssertUnauthorizedResponse(HttpResponseMessage response, string expectedReasonPhrase)
		{
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals(expectedReasonPhrase, response.ReasonPhrase);
			AssertEquals("Bearer", response.Headers.WwwAuthenticate.SingleOrDefault()?.Scheme);
			AssertEquals(@"realm=""WiseTech Global""", response.Headers.WwwAuthenticate.SingleOrDefault()?.Parameter);
		}

		public void TestNoAuthenticationHeader()
		{
			var httpActionContext = AuthenticationTestHelper.Authenticate(null, CancellationToken.None, () => new GateManagementOAuth2AuthorizationAttribute());
			var response = httpActionContext.Response;
			AssertUnauthorizedResponse(response, "Invalid authorization data");
		}

		public void TestOnlyAcceptBearerScheme()
		{
			var httpActionContext = Authenticate("NotBearer", "sdsds");
			var response = httpActionContext.Response;
			AssertUnauthorizedResponse(response, "Invalid authorization data");
		}

		public void TestHttp()
		{
			var authorization = new AuthenticationHeaderValue("Bearer", SampleJWTAccessToken);
			var result = AuthenticationTestHelper.Authenticate(authorization, CancellationToken.None, () => new GateManagementOAuth2AuthorizationAttribute(), "http://www.uhoh.com");
			AssertEquals(HttpStatusCode.Unauthorized, result.Response.StatusCode);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestInvalidAccessToken()
		{
			authServer.ClientIdentifier = "CWService";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			SetWarehouseDataRegistryItems(new[] { authorityUrl }, new[] { authServer.ClientIdentifier });

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var notAToken = SampleJWTAccessToken.Substring(1, SampleJWTAccessToken.Length - 1);
					var httpActionContext = Authenticate("Bearer", notAToken);
					var response = httpActionContext.Response;
					AssertUnauthorizedResponse(response, "Invalid access token");
				}
			}).GetAwaiter().GetResult();
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestValidAccessToken()
		{
			authServer.ClientIdentifier = "CWService";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			SetWarehouseDataRegistryItems(new[] { authorityUrl.ToUpper() }, new[] { authServer.ClientIdentifier, "SomeOtherClientID", "AnotherClientID" });

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var token = GetAccessTokenFromMockServer(authServer);

					var httpActionContext = Authenticate("Bearer", token);
					AssertValidResponse(httpActionContext);
				}
			}).GetAwaiter().GetResult();
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestInvalidClientID()
		{
			authServer.ClientIdentifier = "UnregisteredClientID";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			SetWarehouseDataRegistryItems(new[] { authorityUrl }, new[] { "CWService" });

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token);
					var response = httpActionContext.Response;
					AssertUnauthorizedResponse(response, "Invalid access token");
				}
			}).GetAwaiter().GetResult();

			//ClientId is case sensitive
			SetWarehouseDataRegistryItems(new[] { authorityUrl }, new[] { authServer.ClientIdentifier.ToLower() });

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token);
					var response = httpActionContext.Response;
					AssertUnauthorizedResponse(response, "Invalid access token");
				}
			}).GetAwaiter().GetResult();
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestInvalidAuthorityURL()
		{
			authServer.ClientIdentifier = "CWService";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			SetWarehouseDataRegistryItems(new[] { "https://www.noplace.com" }, new[] { "CWService" });
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token);
					var response = httpActionContext.Response;
					AssertUnauthorizedResponse(response, "Invalid access token");
				}
			}).GetAwaiter().GetResult();
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestExpiredAccessToken()
		{
			authServer.ClientIdentifier = "CWService";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			authServer.TokensAreExpired = true;
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			SetWarehouseDataRegistryItems(new[] { authorityUrl }, new[] { authServer.ClientIdentifier });

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var token = GetAccessTokenFromMockServer(authServer);

					var httpActionContext = Authenticate("Bearer", token);
					var response = httpActionContext.Response;
					AssertUnauthorizedResponse(response, "Access token expired");
				}
			}).GetAwaiter().GetResult();
		}

		void AssertAuthServerReturnsBadResponse(HttpStatusCode code)
		{
			using (var authServer = new SimpleMockServer(code))
			{
				var authorityUrl = $"https://{authServer.HostName}:{authServer.Port}";
				WarehouseDataRegistry.Instance.GateManagementInboundOAuthAuthorityUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { authorityUrl });
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var httpActionContext = Authenticate("Bearer", SampleJWTAccessToken);
						var response = httpActionContext.Response;
						AssertUnauthorizedResponse(response, "Invalid access token");
					}
				}).GetAwaiter().GetResult();
			}
		}

		public void TestAuthServerReturnsBadResponse()
		{
			using (Db.DisposableActionForDbConnection())
			{
				AssertAuthServerReturnsBadResponse(HttpStatusCode.BadRequest);
				AssertAuthServerReturnsBadResponse(HttpStatusCode.ServiceUnavailable);
				AssertAuthServerReturnsBadResponse(HttpStatusCode.InternalServerError);
				AssertAuthServerReturnsBadResponse(HttpStatusCode.NotFound);
				AssertAuthServerReturnsBadResponse(HttpStatusCode.NotAcceptable);
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestCancellationTokenCancelled()
		{
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";
			authServer.ClientIdentifier = "CWService";
			SetWarehouseDataRegistryItems(new[] { authorityUrl }, new[] { authServer.ClientIdentifier });
			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var cancellationToken = new CancellationTokenSource();
					cancellationToken.Cancel();
					var token = GetAccessTokenFromMockServer(authServer);
					var httpActionContext = Authenticate("Bearer", token, cancellationToken.Token);
					var response = httpActionContext.Response;
					AssertEquals(HttpStatusCode.InternalServerError, response.StatusCode);
				}
			}).GetAwaiter().GetResult();
		}

		public void TestReadJwtToken()
		{
			var logger = new TestLogger();
			AssertNull("Invalid token", TokenValidator.ReadJwtToken("XXX", logger));
			AssertEquals(0, logger.Logs.Count);

			logger = new TestLogger();
			AssertNull("Invalid token", TokenValidator.ReadJwtToken(SampleJWTAccessToken.Substring(1, SampleJWTAccessToken.Length - 1), logger));
			AssertEquals(1, logger.Exceptions.Count);
			AssertType<ArgumentException>(logger.Exceptions[0]);

			logger = new TestLogger();
			AssertNotNull("Valid token", TokenValidator.ReadJwtToken(SampleJWTAccessToken, logger));
			AssertEquals(0, logger.Logs.Count + logger.Exceptions.Count);
		}

		class TestLogger : ILogger
		{
			public List<(LogType, string)> Logs { get; } = new List<(LogType, string)>();
			public List<Exception> Exceptions { get; } = new List<Exception>();

			public IDisposable BeginScope<TState>(TState state)
			{
				return NullDisposable.Instance;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return logLevel >= LogLevel.Information;
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				var message = formatter(state, exception);

				if (logLevel == LogLevel.Information)
				{
					Logs.Add((LogType.Information, message));
				}
				else if (logLevel == LogLevel.Warning)
				{
					Logs.Add((LogType.Warning, message));
				}
				else if (logLevel == LogLevel.Error && exception == null)
				{
					Logs.Add((LogType.Error, message));
				}
				else if (logLevel == LogLevel.Error && exception != null)
				{
					AddException(LogType.Error, message, exception);
				}
			}

			void AddException(LogType type, string message, Exception ex)
			{
				Logs.Add((type, message));
				Exceptions.Add(ex);
			}
		}

		class SimpleMockServer : IDisposable
		{
			readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
			readonly ManualResetEvent threadStopped = new ManualResetEvent(false);
			int port;
			bool isDisposed;

			public int Port => port;

			public string HostName => Dns.GetHostEntry(System.Environment.MachineName).HostName.ToLowerInvariant();

			public string Response { get; }

			public HttpStatusCode ResponseStatusCode { get; }

			public SimpleMockServer(HttpStatusCode responseStatusCode, string response)
			{
				var httpListener = CreateAndStartListener();
				ProcessRequests(httpListener);
				Response = response;
				ResponseStatusCode = responseStatusCode;
			}

			public SimpleMockServer(HttpStatusCode responseStatusCode)
				: this(responseStatusCode, string.Empty)
			{ }

			public void Dispose()
			{
				if (!isDisposed)
				{
					tokenSource.Cancel();
					tokenSource.Dispose();
					isDisposed = true;
					threadStopped.WaitOne();
				}
			}

			HttpListener CreateAndStartListener()
			{
				const int MinPort = 49215;
				const int MaxPort = 65535;
				var hostName = HostName;
				HttpListener httpListener = null;

				for (port = MinPort; port < MaxPort; port++)
				{
					httpListener = new HttpListener();
					httpListener.Prefixes.Add($"https://{hostName}:{port}/");
					try
					{
						httpListener.Start();
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}

				if (!httpListener.IsListening)
				{
					throw new Exception("Couldn't start listener");
				}

				return httpListener;
			}

			void ProcessRequests(HttpListener httpListener)
			{
				Task.Run(() =>
				{
					var cancelToken = tokenSource.Token;
					try
					{
						while (true)
						{
							var task = httpListener.GetContextAsync();

							try
							{
								Task.WaitAny(new[] { task }, cancelToken);
							}
							catch (OperationCanceledException)
							{
								httpListener.Stop();
								try
								{
									task.GetAwaiter().GetResult();
								}
								catch (Exception e) when (!e.IsCriticalException())
								{
								}

								return;
							}

							var context = task.GetAwaiter().GetResult();

							context.Response.SendChunked = false;
							context.Response.StatusCode = 200;
							context.Response.ContentType = "text/HTML";

							var bytes = Encoding.UTF8.GetBytes(Response);
							context.Response.ContentLength64 = bytes.Length;
							context.Response.OutputStream.Write(bytes, 0, bytes.Length);
							context.Response.Close();
						}
					}
					finally
					{
						threadStopped.Set();
					}
				});
			}
		}
	}
}
