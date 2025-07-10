using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Test
{
	public sealed class FacilitiesAuthenticationAttributeTest : TransactionedTestCase
	{
		public void TestAllow()
		{
			using (WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "allowed_azp" }))
			{
				var authentication = new FacilitiesAuthenticationAttributeForTest();
				authentication.AzpClaim = "allowed_azp";

				var request = new HttpRequestMessage();
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "dummy_access_token");
				var controllerContext = new HttpControllerContext();
				controllerContext.Request = request;

				var context = new HttpActionContext();
				context.ControllerContext = controllerContext;
				var authenticationContext = new HttpAuthenticationContext(context, null);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

				AssertNull(authenticationContext.ErrorResult);
			}
		}

		public void TestDisallow_DisabledRegistry()
		{
			using (WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "allowed_azp" }))
			{
				var authentication = new FacilitiesAuthenticationAttributeForTest();
				authentication.AzpClaim = "allowed_azp";

				var request = new HttpRequestMessage();
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "dummy_access_token");
				var controllerContext = new HttpControllerContext();
				controllerContext.Request = request;

				var context = new HttpActionContext();
				context.ControllerContext = controllerContext;
				var authenticationContext = new HttpAuthenticationContext(context, null);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

				if (authenticationContext.ErrorResult is AuthenticationFailureResult failure)
				{
					AssertEquals("Expected notification", "Facilities Gate Web Service is disabled. You can enable it in Registry -> Freight -> Gate Booking -> Enable Facilities Gate Web Service", failure.ReasonPhrase);
				}
				else
				{
					Fail("Expected Auth Error");
				}
			}
		}

		public void TestDisallow_InvalidAzp()
		{
			using (WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "allowed_azp" }))
			{
				var authentication = new FacilitiesAuthenticationAttributeForTest();
				authentication.AzpClaim = "some_invalid_client_id";

				var request = new HttpRequestMessage();
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "dummy_access_token");
				var controllerContext = new HttpControllerContext();
				controllerContext.Request = request;

				var context = new HttpActionContext();
				context.ControllerContext = controllerContext;
				var authenticationContext = new HttpAuthenticationContext(context, null);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

				if (authenticationContext.ErrorResult is AuthenticationFailureResult failure)
				{
					AssertEquals("Expected failed authentication", "Invalid access token", failure.ReasonPhrase);
				}
				else
				{
					Fail("Expected Auth Error");
				}
			}
		}

		public void TestDisallow_InvalidToken()
		{
			using (WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "allowed_azp" }))
			{
				var authentication = new FacilitiesAuthenticationAttribute();

				var request = new HttpRequestMessage();
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "sectionA.sectionB.sectionC.sectionD.sectionE");
				var controllerContext = new HttpControllerContext();
				controllerContext.Request = request;

				var context = new HttpActionContext();
				context.ControllerContext = controllerContext;
				var authenticationContext = new HttpAuthenticationContext(context, null);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

				if (authenticationContext.ErrorResult is AuthenticationFailureResult failure)
				{
					AssertEquals("Expected failed authentication", "Invalid authorization data", failure.ReasonPhrase);
				}
				else
				{
					Fail("Expected Auth Error");
				}
			}
		}

		public void TestDisallow_NoAzpClaim()
		{
			using (WarehouseDataRegistry.Instance.EnableFacilitiesGateWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.GateManagementInboundOAuthClientIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "allowed_azp" }))
			{
				var authentication = new FacilitiesAuthenticationAttribute();

				var request = new HttpRequestMessage();
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityToken().ToString());
				var controllerContext = new HttpControllerContext();
				controllerContext.Request = request;

				var context = new HttpActionContext();
				context.ControllerContext = controllerContext;
				var authenticationContext = new HttpAuthenticationContext(context, null);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

				if (authenticationContext.ErrorResult is AuthenticationFailureResult failure)
				{
					AssertEquals("Expected failed authentication", "Invalid authorization data", failure.ReasonPhrase);
				}
				else
				{
					Fail("Expected Auth Error");
				}
			}
		}

		sealed class FacilitiesAuthenticationAttributeForTest : FacilitiesAuthenticationAttribute
		{
			protected override JwtSecurityToken GetJwtToken(string accessToken)
			{
				var claims = new List<Claim>
				{
					new Claim("azp", AzpClaim)
				};
				var token = new JwtSecurityToken(claims: claims);
				return token;
			}

			internal string AzpClaim { get; set; }
		}
	}
}
