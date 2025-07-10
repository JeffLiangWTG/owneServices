using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;

namespace Enterprise.Services.ServiceHost.Tests
{
	static class AuthenticationTestHelper
	{
		public static HttpRequestMessage CreateAuthRequest(AuthenticationHeaderValue authHeader, string requestUri)
		{
			var request = new HttpRequestMessage();
			request.Headers.Authorization = authHeader;
			request.RequestUri = new Uri(requestUri);

			return request;
		}

		public static HttpActionContext CreateHttpActionContext(HttpRequestMessage request)
		{
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var actionContext = new HttpActionContext();
			actionContext.ControllerContext = controllerContext;

			return actionContext;
		}

		public static void AssertUnauthorizedResponse(HttpResponseMessage response, string expectedReasonPhrase)
		{
			Assertion.AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			Assertion.AssertEquals(expectedReasonPhrase, response.ReasonPhrase);
			Assertion.AssertEquals("Bearer", response.Headers.WwwAuthenticate.SingleOrDefault()?.Scheme);
			Assertion.AssertEquals(@"realm=""WiseTech Global""", response.Headers.WwwAuthenticate.SingleOrDefault()?.Parameter);
		}

		public static HttpActionContext Authenticate<T>(AuthenticationHeaderValue authHeader, CancellationToken cancellationToken, Func<T> construct, string requestUri = "https://www.somewherecool.com") where T : OAuth2AuthorizationAttribute
		{
			var request = CreateAuthRequest(authHeader, requestUri);
			var actionContext = CreateHttpActionContext(request);

			var authentication = construct();
			var task = ((IAuthorizationFilter)authentication).ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, () => Task.FromResult(actionContext.Response));
			var response = task.Result;
			return actionContext;
		}

		public static void SetRegistryItems(string[] authUrls, string[] clientIDs)
		{
			eAdaptorRegistry.Instance.eAdaptorInboundOAuthAuthorityUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authUrls);
			eAdaptorRegistry.Instance.eAdaptorInboundOAuthClientIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientIDs);
		}

		public static void SetWarehouseDataRegistryItems(string[] authUrls, string[] clientIDs)
		{
			WarehouseDataRegistry.Instance.GateManagementInboundOAuthAuthorityUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authUrls);
			WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientIDs);
		}

		public static void AssertValidResponse(HttpActionContext httpActionContext)
		{
			Assertion.AssertNull(httpActionContext.Response);
			Assertion.AssertEquals(true, httpActionContext.RequestContext.Principal.Identity.IsAuthenticated);
			Assertion.AssertEquals("Bearer", httpActionContext.RequestContext.Principal.Identity.AuthenticationType);

			var claimsPrinciple = httpActionContext.RequestContext.Principal as ClaimsPrincipal;
			Assertion.AssertEquals(true, claimsPrinciple.HasClaim("aud", "CWService"));
			Assertion.AssertEquals(true, claimsPrinciple.HasClaim("user", "CWService"));
		}

		public static string GetAccessTokenFromMockServer(MockOpenIDIdentityServer mockServer)
		{
			var token = Task.Run(() =>
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{mockServer.Port}";
				var client = new HttpClient();
				var tokenRequest = client.GetAsync($"{authorityUrl}/connect/token").GetAwaiter().GetResult();
				var result = tokenRequest.Content.ReadAsStringAsync().Result;

				var regex = Regex.Match(result, @"access_token"":""(?<Token>[^""]*)""");
				return regex.Groups["Token"].Value;
			}).GetAwaiter().GetResult();

			return token;
		}

		public static IEDICommunicationPartyConfig AddOutboundConfiguration(IEDICommunicationParty party)
		{
			var factory = new BusinessObjectFactory();
			var editParty = factory.Load<EDICommunicationParty>(party.PK);
			var outboundCommunicationAuth = factory.New<EDICommunicationAuth>();
			var outboundCommunicationPartyConfig = factory.New<EDICommunicationPartyConfig>();
			outboundCommunicationPartyConfig.ECC_ECA_Auth = outboundCommunicationAuth.PK;
			outboundCommunicationAuth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.NoAuthentication;
			outboundCommunicationPartyConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			outboundCommunicationPartyConfig.ECC_IsActive = true;
			outboundCommunicationPartyConfig.ECC_ECP_Party = editParty.PK;
			factory.Save();

			return outboundCommunicationPartyConfig;
		}

		public static IEDICommunicationPartyConfig SetUpBasicAuthenticationUser(string username, string password, string name = "Test", bool configActive = true, bool clientActive = true, string applicationCode = eAdaptorNextApplicationDescriptor.ApplicationCode, bool includeOutboundConfig = false)
		{
			var factory = new BusinessObjectFactory();
			var communicationPartyConfig = factory.New<EDICommunicationPartyConfig>();
			var communicationParty = factory.NewWithValidTestData<EDICommunicationParty>();
			var communicationAuth = factory.New<EDICommunicationAuth>();
			communicationPartyConfig.ECC_Direction = "IN";
			communicationPartyConfig.ECC_IsActive = configActive;
			communicationParty.ECP_Name = name;
			communicationParty.ECP_IsActive = clientActive;
			communicationParty.ECP_ApplicationCode = applicationCode;
			communicationPartyConfig.ECC_ECP_Party = communicationParty.PK;
			communicationPartyConfig.ECC_ECA_Auth = communicationAuth.PK;
			communicationAuth.ECA_Username = username;
			communicationAuth.ECA_Password = password;
			communicationAuth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			factory.Save();

			return communicationPartyConfig;
		}

		public static IEDICommunicationPartyConfig SetConfigBranchDepComp(IEDICommunicationPartyConfig config)
		{
			var factory = new BusinessObjectFactory();
			var editConfig = factory.Load<EDICommunicationPartyConfig>(config.PK);

			var department = (BusinessObject)factory.New<IGlbDepartment>();
			department[GlbDepartmentSchema.GE_Code.Name] = "DEP";
			editConfig.ECC_GE_Department = department.PK;

			var company = (BusinessObject)factory.New<IGlbCompany>();
			company[GlbCompanySchema.GC_Code.Name] = "PUK";
			company[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "AU";

			var branch = (BusinessObject)factory.New<IGlbBranch>();
			branch[GlbBranchSchema.GB_Code.Name] = "PUK";
			branch[GlbBranchSchema.GB_GC.Name] = company.PK;
			branch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";
			editConfig.ECC_GB_Branch = branch.PK;

			factory.Save();

			return editConfig;
		}

		public static IEDICommunicationPartyConfig SetUpOAuthAuthentication(MockOpenIDIdentityServer authServer, bool configActive = true, bool clientActive = true, string applicationCode = eAdaptorNextApplicationDescriptor.ApplicationCode)
		{
			authServer.ClientIdentifier = "CWService";
			authServer.Claims = new List<Claim> { new Claim("user", "CWService") };
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{authServer.Port}";

			var factory = new BusinessObjectFactory();
			var communicationPartyConfig = factory.New<EDICommunicationPartyConfig>();
			var communicationParty = factory.NewWithValidTestData<EDICommunicationParty>();
			var communicationAuth = factory.New<EDICommunicationAuth>();
			communicationPartyConfig.ECC_Direction = "IN";
			communicationPartyConfig.ECC_IsActive = configActive;
			communicationParty.ECP_Name = "Test";
			communicationParty.ECP_IsActive = clientActive;
			communicationParty.ECP_ApplicationCode = applicationCode;
			communicationAuth.ECA_AuthorizationEndpoint = authorityUrl;
			communicationAuth.ECA_ClientID = authServer.ClientIdentifier;
			communicationPartyConfig.ECC_ECP_Party = communicationParty.PK;
			communicationPartyConfig.ECC_ECA_Auth = communicationAuth.PK;
			factory.Save();

			return communicationPartyConfig;
		}

		internal static GlbStaff SetSeurityProxyUser(IEDICommunicationPartyConfig config)
		{
			var factory = new BusinessObjectFactory();
			var partyConfigSecurityProxy = factory.NewWithValidTestData<GlbStaff>();
			((EDICommunicationParty)config.Party).ECP_GS_SecurityProxy = partyConfigSecurityProxy.PK;
			factory.Save();
			return partyConfigSecurityProxy;
		}

		internal static void ConfigureOutboundConfig(IEDICommunicationParty ediCommParty)
		{
			var factory = new BusinessObjectFactory();
			var outboundConfig = factory.New<EDICommunicationPartyConfig>();
			outboundConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
			outboundConfig.ECC_IsActive = true;
			outboundConfig.ECC_ECP_Party = ediCommParty.PK;
			var communicationAuth = factory.New<EDICommunicationAuth>();
			communicationAuth.ECA_Username = string.Empty;
			communicationAuth.ECA_Password = string.Empty;
			communicationAuth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.BasicAuthentication;
			outboundConfig.ECC_ECA_Auth = communicationAuth.PK;
			factory.Save();
		}
	}
}
