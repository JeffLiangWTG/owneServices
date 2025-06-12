using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests.Outbound
{
	[TestClass]
	public class CertifiedPickupResponseItemTest
	{
		[TestMethod]
		public void TestParseCertifiedPickupResponse()
		{
			var response1 = @"{""publicReferenceId"":""22de4b55-6d83-44ed-87e1-b5d1da77c98b"",
								""externalReferenceId"":""NXP0000000064""}";
			var certifiedPickupResponse1 = CertifiedPickupResponseitem.LoadFromJsonString(response1);
			Assert.AreEqual("22de4b55-6d83-44ed-87e1-b5d1da77c98b", certifiedPickupResponse1.PublicReferenceId);
			Assert.AreEqual("NXP0000000064", certifiedPickupResponse1.ExternalReferenceId);

			var response2 = @"{
						""statusCode"": 401,
						""message"": ""Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.""}";
			var certifiedPickupResponse2 = CertifiedPickupResponseitem.LoadFromJsonString(response2);
			Assert.AreEqual("401", certifiedPickupResponse2.StatusCode);
			Assert.AreEqual("Access denied due to invalid subscription key. Make sure to provide a valid key for an active subscription.", certifiedPickupResponse2.Message);

			var certifiedPickupResponse3 = CertifiedPickupResponseitem.LoadFromJsonString("1");
			Assert.AreEqual("500", certifiedPickupResponse3.StatusCode);
			Assert.AreEqual("Error JsonFormat for response!", certifiedPickupResponse3.Message);

		}

		[TestMethod]
		public void TestParseCertifiedPickupResponse_Error()
        {
			var responseString = @"{
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
			""title"": ""One or more validation errors occurred."",

	""status"": 400,
    ""traceId"": ""00 -6bcc2db5f45406418648f973e5efc963-2e30d1ca0b623f46-00"",
    ""errors"": {
				""$.billOfLadingNumbers"": [
		
			""The JSON value could not be converted to System.Collections.Generic.IEnumerable`1[System.String]. Path: $.billOfLadingNumbers | LineNumber: 5 | BytePositionInLine: 38.""
        ]

	}
		}";
			var responseJson = CertifiedPickupResponseitem.LoadFromJsonString(responseString);
			var result = responseJson.Errors.GetErrors();
			Assert.AreEqual("BillOfLadingNumbers: The JSON value could not be converted to System.Collections.Generic.IEnumerable`1[System.String]. Path: $.billOfLadingNumbers | LineNumber: 5 | BytePositionInLine: 38.;", result);

		}

	}
}
