#if NETFRAMEWORK
using System.Net.Http;
using System.Web.Http;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif
using System.Net;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Services.ServiceHost.NetCore;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class CustomsStatusControllerTest : TestCase
	{
		public void TestGetCustomsStatusReasonDescription_NoReasonCode()
		{
			var response = GetController().GetCustomsStatusReasonDescription(string.Empty, validCountryCode).GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Incorrect reason code () or country/region code (AU) length - reason code should be 3 characters long and country/region code should be 2 characters long.", message);
		}

		public void TestGetCustomsStatusReasonDescription_NoCountryCode()
		{
			var response = GetController().GetCustomsStatusReasonDescription(validCustomsStatusCode, string.Empty).GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Incorrect reason code (CLR) or country/region code () length - reason code should be 3 characters long and country/region code should be 2 characters long.", message);
		}

		public void TestGetCustomsStatusReasonDescription_IncorrectReasonCodeLength()
		{
			var response = GetController().GetCustomsStatusReasonDescription("ABCD", validCountryCode).GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Incorrect reason code (ABCD) or country/region code (AU) length - reason code should be 3 characters long and country/region code should be 2 characters long.", message);
		}

		public void TestGetCustomsStatusReasonDescription_IncorrectCountryCodeLength()
		{
			var response = GetController().GetCustomsStatusReasonDescription(validCustomsStatusCode, "AUS").GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Incorrect reason code (CLR) or country/region code (AUS) length - reason code should be 3 characters long and country/region code should be 2 characters long.", message);
		}

		public void TestGetCustomsStatusReasonDescription_ValidCodeAndCountry()
		{
			var response = GetController().GetCustomsStatusReasonDescription(validCustomsStatusCode, validCountryCode).GetResult();
			var message = response.GetMessage(HttpStatusCode.OK);

			AssertEquals((int)HttpStatusCode.OK, response.GetStatusCode());
			AssertContains("CLEAR - Cargo is free of any impediments and may be released.", message);
		}

		public void TestGetCustomsStatusReasonDescription_InvalidCountry()
		{
			var response = GetController().GetCustomsStatusReasonDescription(validCustomsStatusCode, "XX").GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Could not find customs status reason code mapping for the country/region 'XX'.", message);
		}

		public void TestGetCustomsStatusReasonDescription_InvalidCode()
		{
			var response = GetController().GetCustomsStatusReasonDescription("ZZZ", validCountryCode).GetResult();
			var message = response.GetMessage(HttpStatusCode.BadRequest);

			AssertEquals((int)HttpStatusCode.BadRequest, response.GetStatusCode());
			AssertContains("Did not find the reason description for code 'ZZZ' from country/region 'AU'.", message);
		}

		#region Implementation

		CustomsStatusController GetController()
		{
			var controller = new CustomsStatusController();
#if NETFRAMEWORK
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();
#else
			controller.ControllerContext = new ControllerContext()
			{
				HttpContext = new DefaultHttpContext()
			};
#endif
			return controller;
		}

		string validCustomsStatusCode => CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
		string validCountryCode => Core.Constants.CountryCodes.Australia;
		#endregion
	}
}
