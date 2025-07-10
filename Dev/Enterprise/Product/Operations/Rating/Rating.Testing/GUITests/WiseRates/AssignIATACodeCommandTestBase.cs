using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignIATACodeCommandTestBase : BaseRatingIntegrationTest
	{
		protected WiseEntryView CreateWiseEntryView(OrgHeader carrier, string iataCode, bool isAir = true, bool carrierFound = false)
		{
			var costing = CreateTestRate
			(
				category: "AIR",
				mode: "LCL",
				origin: "AUSYD",
				destination: "USLAX",
				serviceLevel: "",
				commodityCode: "",
				containerType: "",
				carrier: carrier.OH_Code,
				client: "",
				controllingCustomer: ""
			);

			var entry = new WiseEntry(costing, Factory)
			{
				TI_Mode = isAir ? Core.Constants.RateMode.AIR : Core.Constants.RateMode.SEA,
			};

			var header = new WiseHeader(Factory);
			if (carrierFound)
			{
				header.TH_OH = carrier.PK;
			}
			header.ChildRateEntries = new[] { entry };

			var response = new RatesSearchResponse
			{
				Carriers = new[]
				{
					new RefCarrier { Code = carrier.OH_Code, IATACode = iataCode, Name = carrier.OH_FullName }
				}
			};

			return new WiseEntryView(entry, response);
		}

		protected RefAirline CreateRefAirline(string twoCharCode, string eagleAddedAirlinePrefixOrAccountingCode = "")
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = eagleAddedAirlinePrefixOrAccountingCode;
			airline.RM_TwoCharacterCode = twoCharCode;
			Factory.Save();

			return airline;
		}
	}
}
