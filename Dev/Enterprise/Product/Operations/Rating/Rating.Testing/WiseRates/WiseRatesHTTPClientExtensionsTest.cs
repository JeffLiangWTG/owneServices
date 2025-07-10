using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Newtonsoft.Json;
using WiseRates.Tools;
using DTO = WiseRates.Api.Model;

namespace Enterprise.RatingTests.WiseRates
{
	public class WiseRatesHTTPClientExtensionsTest : TestCaseWithFactory
	{
		public void TestRatesMissingAndExtraFieldsFromJSON()
		{
			var jsonString = @"{
  ""Rates"": [
    {
      ""Id"": ""EDE0B91A-EC6A-4621-BC59-A7707CE8E858"",
      ""Origin"": ""AUSYD"",
      ""Via"": null,
      ""Destination"": ""USLAX"",
      ""Carrier"": ""NEWTESSYD"",
      ""Client"": ""NEWTESSYD"",
      ""Consignee"": """",
      ""ControllingCustomer"": """",
      ""ContractNumber"": null,
      ""ServiceLevel"": ""STD"",
      ""TransportMode"": ""SEA"",
      ""ContainerMode"": ""FCL"",
      ""Container"": null,
      ""Commodity"": """",
      ""UnmappedCommodity"": null,
      ""UnmappedCustomer"": null,
      ""TransitTime"": null,
      ""ProviderRateId"": null,
      ""StartDate"": ""2017-10-30"",
      ""ExpiryDate"": null,
      ""IssueDate"": ""2017-10-30"",
      ""Provider"": null,
      ""Charges"": [
        {
          ""RateLineID"": 1,
          ""ChargeCode"": ""FRT"",
          ""Unit"": null,
          ""ConversionFactorUnit"": null,
          ""ConversionFactorDenominatorUnit"": null,
          ""ConversionFactor"": null,
          ""FlatRate"": 900.0,
          ""MinRate"": null,
          ""MaxRate"": null,
          ""PerUnitRate"": null,
          ""Currency"": ""AUD"",
          ""Break"": null,
          ""BreakUnit"": null,
          ""BreakOperator"": null,
          ""Percentage"": null,
          ""PercentageAppliesTo"": null,
          ""Comment"": null,
          ""IsInclusive"": false,
          ""ProviderDescription"": null
        }
      ],
      ""ExtraField11"": ""11"",
      ""ExtraField12"": ""12"",
      ""ExtraField21"": {
        ""ExtraField211"": 211,
        ""ExtraField212"": ""212"",
      },
      ""ExtraField31"": [
        {
          ""ExtraField3111"": 3111,
        },
        {
          ""ExtraField3121"": 3121,
        }
      ]
    },
    {
      ""Id"": ""5F8E5302-6C6F-49F0-8610-17FB566ECE52"",
    },
  ],
  ""Warnings"": [""Warning1"", ""Warning2""],
  ""RatesSearchEnabled"": true
}";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "NTC1";
			carrier.OH_FullName = "New Test Client";
			carrier.MainAddress.OA_Address1 = "123 Fake Street";
			carrier.MainAddress.OA_City = "Sydney";
			carrier.MainAddress.OA_State = "NSW";
			carrier.MainAddress.OA_PostCode = "2000";
			carrier.OH_RL_NKClosestPort = "AUSYD";

			var rate1 = new DTO.Rate
			{
				Id = "EDE0B91A-EC6A-4621-BC59-A7707CE8E858",
				Client = carrier.OH_Code,
				Carrier = carrier.OH_Code,
				ControllingCustomer = "",
				Commodity = "",
				Charges = new List<DTO.Charge>(),
				ContainerMode = "FCL",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ServiceLevel = "STD",
				IssueDate = ZDateTime.FromSqlFormat("2017-10-30 00:00:00.000").ToDateTime(),
				StartDate = ZDateTime.FromSqlFormat("2017-10-30 00:00:00.000").ToDateTime()
			};
			rate1.Charges.Add(
				new DTO.Charge
				{
					RateLineID = 1,
					ChargeCode = "FRT",
					Unit = null,
					Currency = "AUD",
					FlatRate = 900,
					PerUnitRate = null
				}
			);

			var rate2 = new DTO.Rate { Id = "5F8E5302-6C6F-49F0-8610-17FB566ECE52" };

			var expectedResult = new DTO.RatesSearchResponse
			{
				Rates = new[] { rate1, rate2 },
				Warnings = new[] { "Warning1", "Warning2" },
			};

			using (var handler = new MockHttpMessageHandler(
				HttpStatusCode.OK,
				new StringContent(jsonString, Encoding.UTF8, "application/json")))
			using (var httpClient = new HttpClient(handler))
			{
				var ratesSearchResponse = httpClient.PostAsJsonAsync<DTO.RatesSearchResponse>(
					"http://mockserver/",
					JsonConvert.DeserializeObject("{\"RequestID\": \"mockvalue\"}")).Result;

				AssertContainsExactElementsInAnyOrder("Rates should ignore unmapped fields from JSON and set default values for missing fields",
					expectedResult.Rates.Select(x => x.ToJSON()),
					ratesSearchResponse.Rates.Select(x => x.ToJSON()));

				AssertContainsExactElementsInAnyOrder("Search result has Warnings list", expectedResult.Warnings, ratesSearchResponse.Warnings);

				AssertEquals("Search result has empty Carriers list", 0, ratesSearchResponse.Carriers.Length);
				AssertEquals("Search result has empty ChargeCodes list", 0, ratesSearchResponse.ChargeCodes.Length);
				AssertEquals("Search result has empty Providers list", 0, ratesSearchResponse.Providers.Length);
			}
		}

		#region Implementation

		class MockHttpMessageHandler : HttpMessageHandler
		{
			public MockHttpMessageHandler(HttpStatusCode statusCode, HttpContent content = null)
			{
				response = new HttpResponseMessage(statusCode);
				if (content != null)
				{
					response.Content = content;
				}
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var requestUri = request.RequestUri.ToString();
				if (!requestUri.StartsWith("http"))
				{
					throw new ArgumentException("Only 'http' and 'https' schemes are allowed.\r\nParameter name: requestUri");
				}
				return Task.FromResult(response);
			}

			readonly HttpResponseMessage response;
		}

		#endregion
	}
}
