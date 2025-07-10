using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AirBookingCarrierConfigurationManagerTest : TransactionedTestCase
	{
		#region TestIsSupported

		public void TestIsSupported()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				AssertEquals("081 is not supported", false, AirBookingCarrierConfigurationManager.IsSupported("081", out _));
				AssertEquals("057 is supported", true, AirBookingCarrierConfigurationManager.IsSupported("057", out _));
				AssertEquals("bad request", false, AirBookingCarrierConfigurationManager.IsSupported(null, out _));
				AssertEquals("bad request", false, AirBookingCarrierConfigurationManager.IsSupported("0", out _));
			}
		}

		#endregion

		#region TestSupportedAirlines

		public void TestSupportedAirlines()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var supportedAirlines = AirBookingCarrierConfigurationManager
					.SupportedAirlines
					.Values
					.Select(info => $"{info.Prefix} - {info.Name}") // programmatic constant
					.ToArray();

				AssertContainsExactElementsInAnyOrder("",
					new[]
					{
						"020 - Lufthansa",
						"057 - Air France",
						"074 - KLM Royal Dutch Airlines",
						"607 - Etihad Airways",
						"618 - Singapore Airline"
					},
					supportedAirlines);
			}
		}

		public void TestSupportedAirlinesOptions()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;

				var singapore = airlinesDetails["618"];
				AssertNotNull("Singapore Airline - has option details", singapore.Options);
				AssertEquals("Singapore Airline - Commodity Code is required", AirlineConfigOptionValue.Required, singapore.Options.CommodityCode);

				var etihad = airlinesDetails["607"];
				AssertEquals("Etihad Airways - UldBooking details", AirlineConfigOptionValue.Optional, etihad.Options.UldBooking);
				AssertEquals("Etihad Airways - LooseBooking details", AirlineConfigOptionValue.Optional, etihad.Options.LooseBooking);
				AssertEquals("Etihad Airways - AllotmentBooking details", AirlineConfigOptionValue.Optional, etihad.Options.AllotmentBooking);
				AssertEquals("Etihad Airways - Commodity Code is Optional", AirlineConfigOptionValue.Optional, etihad.Options.CommodityCode);
			}
		}

		public void TestSupportedAirlinesCommodities()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;

				var airFrance = airlinesDetails["057"];
				AssertNull("Air France - null commodities", airFrance.Commodities);

				var klm = airlinesDetails["074"];
				AssertNull("KLM Royal Dutch Airlines - null commodities", klm.Commodities);

				var singapore = airlinesDetails["618"];
				AssertEquals("Singapore Airline - empty commodities", 0, singapore.Commodities.Count);

				var etihad = airlinesDetails["607"];

				string GetSpecialHandlingCodes(AirlineConfigCommodity commodity) => commodity.SpecialHandlingCodes != null
					? string.Join(", ", commodity.SpecialHandlingCodes)
					: null;

				var commodities = etihad
					.Commodities
					.Select(details =>
						$"{details.Code} - {details.Description} ({GetSpecialHandlingCodes(details) ?? "none"})");

				AssertContainsExactElementsInAnyOrder("Etihad Airways - commodities details",
					new[]
					{
						"GENERAL - GENERAL CARGO (GEN)",
						"ACCESRY - ACCESSORIES (GEN, HVY, HUM)",
						"ACE - AIRCRAFT ENGINES (GEN)",
						"ACPTS - AIRCRAFT PARTS (GEN)",
						"ADHE - ADHESIVE (GEN)",
						"AERO - AEROSOL (DGR)",
						"OTH - OTHER (none)"
					},
					commodities);
			}
		}

		public void TestGetCommodities()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airFranceCommodities = AirBookingCarrierConfigurationManager.GetAirlineConfig("057").Commodities;
				AssertNull("Airline with null commodities", airFranceCommodities);

				var etihadCommodities = AirBookingCarrierConfigurationManager.GetAirlineConfig("607").Commodities;
				AssertNotNull("Airline with valid commodities", etihadCommodities);

				var resultItems = etihadCommodities
					.Select(item => $"{item.Code} - {item.Description}");

				AssertContainsExactElementsInAnyOrder("Etihad Airways - commodities details",
					new[]
					{
						"GENERAL - GENERAL CARGO",
						"ACCESRY - ACCESSORIES",
						"ACE - AIRCRAFT ENGINES",
						"ACPTS - AIRCRAFT PARTS",
						"ADHE - ADHESIVE",
						"AERO - AEROSOL",
						"OTH - OTHER"
					}, resultItems);
			}
		}

		public void TestSupportedAirlinesProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;

				var airFrance = airlinesDetails["057"];
				AssertNull("Air France - null options details", airFrance.Products);

				var klm = airlinesDetails["074"];
				AssertNull("KLM Royal Dutch Airlines - null options details", klm.Products);

				var singapore = airlinesDetails["618"];
				AssertNotNull("Singapore Airline - empty list options details", singapore.Products);
				AssertEquals(0, singapore.Products.Count);

				var etihad = airlinesDetails["607"];

				var products = etihad
					.Products
					.Select(product =>
						$"{product.Code} - {product.Description}");

				AssertContainsExactElementsInAnyOrder("Etihad Airways - products",
					new[]
					{
						"AOG - Emirates AOG",
						"AWA - SkyWheels Premium Door-to-Door",
						"ACE - AIRCRAFT ENGINES"
					},
					products);
			}
		}

		public void TestGetProducts()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airFranceProducts = AirBookingCarrierConfigurationManager.GetAirlineConfig("057").Products;
				AssertNull("Airline with null products", airFranceProducts);

				var singaporeProducts = AirBookingCarrierConfigurationManager.GetAirlineConfig("618").Products;
				AssertNotNull("Airline with empty list products", singaporeProducts);
				AssertEquals(0, singaporeProducts.Count);

				var etihadProducts = AirBookingCarrierConfigurationManager.GetAirlineConfig("607").Products;
				AssertNotNull("Airline with valid products", etihadProducts);

				var resultItems = etihadProducts
					.Select(product => $"{product.Code} - {product.Description}");

				AssertContainsExactElementsInAnyOrder("Etihad Airways - products",
					new[]
					{
						"AOG - Emirates AOG",
						"AWA - SkyWheels Premium Door-to-Door",
						"ACE - AIRCRAFT ENGINES"
					}, resultItems);
			}
		}

		#endregion

		#region TestRequiresTermsAndConditons

		public void TestRequiresTermsAndConditons()
		{
			using (AirBookingTestHelper.TempEnableCarrierConfiguration())
			using (AirBookingTestHelper.TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;

				var etihad = airlinesDetails["607"];
				AssertEquals("Etihad does not require T&Cs", false, etihad.Options.RequiredTermsAgreement);

				var lufthansa = airlinesDetails["020"];
				AssertEquals("Lufthansa requires T&Cs", true, lufthansa.Options.RequiredTermsAgreement);
			}
		}

		#endregion
	}
}
