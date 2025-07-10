using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Xml;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Configuration;
using Enterprise.Rating.Web.Controllers;
using Enterprise.Rating.Web.Model;
using Enterprise.Registry.Business;
using FluentValidation;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Testing.Controllers
{
	public abstract class RatesAPIControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReportUsage_GivenValidRequest_ThenOKIsLogged() => CheckReportsUsage(new RateQuery(), () => Array.Empty<Rate>(), HttpStatusCode.OK);

		[ExpectNoExceptions]
		public void TestReportsUsage_GivenValidationError_ThenBadRequestIsLogged() => CheckReportsUsage(new RateQuery(), () => throw new ValidationException(""), HttpStatusCode.BadRequest);

		[ExpectNoExceptions]
		public void TestReportsUsage_GivenEmptyQuery_ThenBadRequestIsLogged() => CheckReportsUsage(null, () => null, HttpStatusCode.BadRequest);

		void CheckReportsUsage(RateQuery query, Func<Rate[]> serviceCore, HttpStatusCode expectedStatusCode)
		{
			var username = Factory.NewWithValidTestData<GlbStaff>().GS_LoginName;
			Factory.Save(); // Staff is later loaded into the enviroment from a different factory
			var branchCode = GlbBranch.CurrentBranch.GB_Code;
			var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;

			var identity = new ClaimsIdentity(new[] { new Claim(WTGClaimTypes.UserCode, username) });
			var principalMock = new Mock<IPrincipal>();
			principalMock.Setup(p => p.Identity).Returns(identity);
			Thread.CurrentPrincipal = principalMock.Object;

			var mockCWServiceProvider = new Mock<ICWServiceProvider>();
			mockCWServiceProvider.Setup(s => s.GetCosts(username, branchCode, departmentCode, It.IsAny<RateQuery>(), It.IsAny<ILogger>())).Returns(serviceCore);
			var controller = new RatesAPIController(mockCWServiceProvider.Object, new ElementaryLogger());

			controller.Costing(branchCode, departmentCode, query);

			mockCWServiceProvider.Verify(s => s.ReportUsage(username, branchCode, departmentCode, SourceEndpoint.Costing, expectedStatusCode, It.IsAny<IEnumerable<Rate>>(), It.IsAny<TimeSpan>()), Times.Once());
		}

		public void TestEndpointIsProtectedWithAuthorization()
		{
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);

				var requestBuilder = server.CreateRequest(Endpoint);
				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			}
		}

		public void TestEndpointIsProtectedWithUserTokenAuthorization()
		{
			var mockOIDCConfig = new Mock<IOIDCConfig>();
			mockOIDCConfig.Setup(config => config.IsOIDCEnabled).Returns(true);
			mockOIDCConfig.Setup(config => config.AuthorityURL).Returns("https://wtg.zone");
			mockOIDCConfig.Setup(config => config.ClientIdentifier).Returns("57985030-DF96-4E65-805E-A6B6EF81EF09");
			using (var server = TestServer.Create<RatingAPIsStartup>())
			using (ObjectFactory.Substitute(mockOIDCConfig.Object))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);

				requestBuilder.And(configure => configure.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsImtpZCI6IkpNSy1zaEdWd0dLZm02WURZSnRXbUxMdjRaR3pkZnlDUkQ0b0dWbk83azQiLCJ0eXAiOiJKV1QifQ.eyJ0aWQiOiI4YjQ5Mzk4NS1lMWI0LTRiOTUtYWRlNi05OGFjYWZkYmRiMDEiLCJjb21wYW55X2NvZGUiOiJXVEciLCJ1c2VyX25hbWUiOiJqYXkud2FuZyIsInVuaXF1ZV9uYW1lIjoiV1RHLmpheS53YW5nIiwic3ViIjoiODhkNTg4MDItMTRlZC00NWYxLWE0OTEtMGE4NDZlMzNhYWU3Iiwib2lkIjoiODhkNTg4MDItMTRlZC00NWYxLWE0OTEtMGE4NDZlMzNhYWU3IiwiYXpwIjoiMzQ5MmIxNTQtYThjZS00Y2UzLWE5NTYtNTExMjc2Y2JkOWU2IiwidmVyIjoiMS4wIiwiaWF0IjoxNjkxNDYxNTI4LCJhdWQiOiIzNDkyYjE1NC1hOGNlLTRjZTMtYTk1Ni01MTEyNzZjYmQ5ZTYiLCJleHAiOjE2OTE0NjUxMjgsImlzcyI6Imh0dHBzOi8vY2FyZ293aXNlYjJjMDEuYjJjbG9naW4uY29tLzFiMjBiODdlLWNlYmQtNDNjYy05N2JkLWJkZDQxYTJmNWNmMS92Mi4wLyIsIm5iZiI6MTY5MTQ2MTUyOH0.UpEUW1pcZJlvZSOCm03nVg02FVsHAJXUVM7TS5WAKLDOsUtl5HoLluDWKmnywsqfPEleW-TKsw0O7JpJ9gRQsPeDdXbnu1fESdpmyIxMCM1CaOJrBBXCeCFmJGoAaRNW2B6x0AobVkZwkQB66_ZHKoQka9YdHJJCSqXzZOtw_WpOw6BPhmvP4VUDvJ9m3wGO6mNam3qWna7n75vFHwXZe8c_x-Teax718bf89GWk9R3ZHqlXZYivQQLwaFAiY3QGyAfIqrH4sywALaINFJ5FFIgz-I-RckzTbg7pU_50RHPtPexaDp-tEWN4zhdQTDJojddWYnu1j15wtqjG1PvM1Q"));
				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);

				mockOIDCConfig.Verify(mock => mock.IsOIDCEnabled, Times.Once());
				mockOIDCConfig.Verify(mock => mock.AuthorityURL, Times.Once());
				mockOIDCConfig.Verify(mock => mock.ClientIdentifier, Times.Once());
			}
		}

		public void TestEndpointIsProtectedWithClientTokenAuthorization()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JWA";
			factory.Save();

			var ratingTokenAuthenticationCollection = new RatingTokenAuthenticationCollection
			{
				new RatingTokenAuthentication() { ClientId = "3e974f1d-9353-4004-a916-14a351af6e05", StaffCode = "JWA" }
			};

			var systemToSystemTrustInfo = new SystemToSystemTrustInfo() { TenantId = "1B22C20A-7DCC-4452-9630-D8174BD4DA24" };

			using (var server = TestServer.Create<RatingAPIsStartup>())
			using (RatingDataRegistry.Instance.RatingTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ratingTokenAuthenticationCollection))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);

				requestBuilder.And(configure => configure.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiIzZTk3NGYxZC05MzUzLTQwMDQtYTkxNi0xNGEzNTFhZjZlMDUiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxL3YyLjAiLCJpYXQiOjE2OTEzODU1MzcsIm5iZiI6MTY5MTM4NTUzNywiZXhwIjoxNjkxMzg5NDM3LCJhaW8iOiJBU1FBMi84VUFBQUFCMkhhcjZqOXJuNTg4dWxMcXd6ZzNCTFNRMHlJelB3b1pTMUxWdW9EMUhNPSIsImF6cCI6IjNlOTc0ZjFkLTkzNTMtNDAwNC1hOTE2LTE0YTM1MWFmNmUwNSIsImF6cGFjciI6IjIiLCJvaWQiOiIzZjdiMWNiYy01NWViLTQzODctODNkYy1mYTJlMzNlNmUyNzgiLCJyaCI6IjAuQVQ4QXozQktnR0ZLQ0VxUWpxTXJMUC1CTVIxUGx6NVRrd1JBcVJZVW8xR3ZiZ1ZBQUFBLiIsInJvbGVzIjpbIjE6ZGpsRFNFbDJjVTFFZEVwUE5XZHFNR3d2TVdaVVdqVkdTbGRoT0hBeGJEVkNhblY0VEdkdWNHdDJia1JKUWtKcldXRkdUbGRTYTI1NVNqUXlkVGRzWW10dmVIVjFaRzFOYmxKdWRsRkJiVkl2VjBoM2RVcHZaSGhoZUU4M0sxbGlPV1p5Vm5NeGJqaDZVRmR3VmtZd1QzZHNVMFJ0ZDJ0VE5FRjJPRVUwWnpCdlZVWnZPV3QwWm14MFdEYzRlbTFDYVRKemRtTTViak5xUTNCeFRGQlRTVVpVZWpCdE1uQnFTMHRLTjBSYWRsZHhkejA5UEM5TmIyUjEiLCIyOmJIVnpQanhGZUhCdmJtVnVkRDVCVVVGQ1BDOUZlSEJ2Ym1WdWRENDhMMUpUUVV0bGVWWmhiSFZsUGc9PSIsIjA6UEZKVFFVdGxlVlpoYkhWbFBqeE5iMlIxYkhWelBteDJiR3RRWTNoRk5GZEZSaXRvU0VaSkwzVm5PVU14VVRCMlRWRmlVV2xhYm5vM2FVUnFOVlp2WkRka1JWUlFOek4yUzBWM056aDFabXRhZFZwWE9YWlFVMHgxTmtveGEwOVBkSEJ6UnpSV1prOUhZVk5rT1U4MlMwRnZRVUZUWWpsbWNVOXZTRFpYT1RSMVlWcEpkVWhvVUhOS1NEVkdSRko0UW1GSlpYRlJTV1JyYkhOWlVHc3dlR1oxYzNVckswNHhNRTFSZFcxMVdXZ3JWVFJTY21OclprcEIiXSwic3ViIjoiM2Y3YjFjYmMtNTVlYi00Mzg3LTgzZGMtZmEyZTMzZTZlMjc4IiwidGlkIjoiODA0YTcwY2YtNGE2MS00YTA4LTkwOGUtYTMyYjJjZmY4MTMxIiwidXRpIjoidnN3ZHVyeHFtVXF0N3EteFctRUdBQSIsInZlciI6IjIuMCJ9.W6JLsDp4NokosVLl6A5U0YoIAxCj125A7CiEJaJ3nUWhMQy1xjrxm720IWImqoRaO2ezisHjJbpixW_1BIDt2GFv--QwjFJbyWoGiESlegqD8CyuJ1Mwn-qgpmlktRPPsaHg4UUIp5I4wUc_Ggr6PguEw1yER-Mr6t3rzQmv0YFGmPAWt6fm2O3i7nJGJDA8lw4moMlaT7L8VtsRci-oHLuYh-t143NM7k2m4BrqxVLWpXJeG9pDcJVKYxdAZHnUHatG_2S-IM9MAFFxJX2XVSUvA2wjo3B6-sDBm7cyRX5FBK_8heqdirg4btJm6-Q5qIgc3GTdSqBb4ed5lpBzag"));
				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			}
		}

		public void TestReturn406WhenRequestedMediaTypeIsJsonAndItIsNotSupportedInConfig()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false)) // We only support XML. 
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);
				requestBuilder.AddHeader("Accept", "application/json"); // We do not support json here because related config is set to false

				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.NotAcceptable, response.StatusCode);
				AssertEquals("Acceptable media types are: application/xml", response.ReasonPhrase);
			}
		}

		public void TestReturn406WhenRequestedMediaTypeIsNotSupported()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // We support both XML and json. 
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);
				requestBuilder.AddHeader("Accept", "text/html"); // We don't support it

				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.NotAcceptable, response.StatusCode);
				AssertEquals("Acceptable media types are: application/xml, application/json", response.ReasonPhrase);
			}
		}

		public void TestResponseIsXMLWhenRequestedMediaTypeIsXMLWhenWeSuppportBothJsonAndXML()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // We support both XML and json. 
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);
				requestBuilder.AddHeader("Accept", "application/xml"); // We negotiate for XML response in our request

				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);

				var content = ReadResponseAsStringAsync(response);
				AssertNotNullOrEmpty(content);

				var xDoc = new XmlDocument();
				xDoc.LoadXml(content);
				AssertNotNull(xDoc.FirstChild);
			}
		}

		public void TestResponseIsJsonWhenRequestedMediaTypeIsJSONWhenWeSuppportBothJsonAndXML()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // We support both XML and json. 
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);
				requestBuilder.AddHeader("Accept", "application/json"); // We negotiate for JSON response in our request

				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);

				var content = ReadResponseAsStringAsync(response);
				AssertNotNullOrEmpty(content);

				var json = JsonConvert.DeserializeObject(content); // Will throw an exception if not a valid json
				AssertNotNull(json);
			}
		}

		public void TestResponseIsJSONWhenClientAcceptsAnyMediaType()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // We support both XML and json.
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest(Endpoint);
				requestBuilder.AddHeader("Accept", "*/*"); // Client accepts any thing

				var response = PostAsyncRequest(requestBuilder);
				Assert(!response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);

				var content = ReadResponseAsStringAsync(response);
				AssertNotNullOrEmpty(content);

				var json = JsonConvert.DeserializeObject(content); // Will throw an exception if not a valid json
				AssertNotNull(json);
			}
		}

		public void TestMediaTypeLimitIsOnlyForRatesAPIControllerEndpoints()
		{
			using (RatingDataRegistry.Instance.SupportJsonMediaType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // We support both XML and json. 
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest("/wtg/status");
				requestBuilder.AddHeader("Accept", "text/html");

				var response = GetAsyncRequest(requestBuilder);
				AssertEquals(true, response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}
		}

		protected string Endpoint = "";

		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}

		HttpResponseMessage GetAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.GetAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}

		HttpResponseMessage PostAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.PostAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}

		string ReadResponseAsStringAsync(HttpResponseMessage response)
		{
			return AsyncHelper.RunTask(() => response.Content.ReadAsStringAsync(), CancellationToken.None, "Read Endpoint Result Content").GetAwaiter().GetResult().Result;
		}
	}

	public class CostingRatesAPIControllerTest : RatesAPIControllerTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Endpoint = "/api/rating/costing/BCH/DEP";
		}
	}

	public class IntercompanyTariffsAPIControllerTest : RatesAPIControllerTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Endpoint = "/api/rating/intercompanytariffs/BCH/DEP";
		}
	}

	public class CompanyTariffsAPIControllerTest : RatesAPIControllerTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Endpoint = "/api/rating/companytariffs/BCH/DEP";
		}
	}

	public class ClientRatesAPIControllerTest : RatesAPIControllerTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Endpoint = "/api/rating/clientrates/BCH/DEP";
		}
	}

	public class JobChargesAPIControllerTest : RatesAPIControllerTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			Endpoint = "/api/rating/jobcharges/BCH/DEP";
		}
	}
}
