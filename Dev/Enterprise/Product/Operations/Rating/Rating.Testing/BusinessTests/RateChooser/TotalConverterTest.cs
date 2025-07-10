using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateChooser.Converters;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Test
{
	class TotalConverterTest : RatingTestCase
	{
		public void TestConvertNoOptionalCharges()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.ChargeCodes.NewConsolChargeCode("BL1", "BOL Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee FRT", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL3", "BOL Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL4", "Doc Fee ORG", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL5", "BOL Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL6", "Doc Fee DST", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, companyPK: GlbCompany.CurrentCompany.PK);

			var chargesViewModel = SetupChargesViewModel(new[]
			{
				("BL1", false, 10m),
				("BL2", false, 20m),
				("BL3", false, 100m),
				("BL4", false, 200m),
				("BL5", false, 1000m),
				("BL6", false, 2000m)
			});

			Assert("Precondition: All charges are required.", chargesViewModel.Charges.All(c => c.IsActive));

			CombineAssertions("GroupTotal should only sum enabled charges", () =>
			{
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 30m, expectedORGTotal: 300m, expectedDSTTotal: 3000m, assertionMessage: "All charges are enabled");

				// FRT charges
				var chargeViewModel1 = chargesViewModel.Charges.Single(charge => charge.Code == "BL1");
				var chargeViewModel2 = chargesViewModel.Charges.Single(charge => charge.Code == "BL2");
				chargeViewModel1.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 20m, expectedORGTotal: 300m, expectedDSTTotal: 3000m, assertionMessage: "FRT: Disabling BL1");
				chargeViewModel2.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 0m, expectedORGTotal: 300m, expectedDSTTotal: 3000m, assertionMessage: "FRT: Disabling BL1 & BL2");
				chargeViewModel1.IsActive = true;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 300m, expectedDSTTotal: 3000m, assertionMessage: "FRT: Re-enabling BL1 (BL2 is disabled)");

				// ORG charges
				var chargeViewModel3 = chargesViewModel.Charges.Single(charge => charge.Code == "BL3");
				var chargeViewModel4 = chargesViewModel.Charges.Single(charge => charge.Code == "BL4");
				chargeViewModel3.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 200m, expectedDSTTotal: 3000m, assertionMessage: "ORG: Disabling BL3");
				chargeViewModel4.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 0m, expectedDSTTotal: 3000m, assertionMessage: "ORG: Disabling BL3 & BL4");
				chargeViewModel3.IsActive = true;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 100m, expectedDSTTotal: 3000m, assertionMessage: "ORG: Re-enabling BL3 (BL4 is disabled)");

				// DST charges
				var chargeViewModel5 = chargesViewModel.Charges.Single(charge => charge.Code == "BL5");
				var chargeViewModel6 = chargesViewModel.Charges.Single(charge => charge.Code == "BL6");
				chargeViewModel5.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 100m, expectedDSTTotal: 2000m, assertionMessage: "DST: Disabling BL5");
				chargeViewModel6.IsActive = false;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 100m, expectedDSTTotal: 0m, assertionMessage: "DST: Disabling BL5 & BL6");
				chargeViewModel5.IsActive = true;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 100m, expectedDSTTotal: 1000m, assertionMessage: "DST: Re-enabling BL5 (BL6 is disabled)");
			});
		}

		static void AssertChargeGroupTotalConverter(IEnumerable<ChargesViewModel.ChargeGroupView> chargeGroupCollection, decimal expectedFRTTotal, decimal expectedORGTotal, decimal expectedDSTTotal, string assertionMessage = default)
		{
			var converter = new TotalConverter();

			var frtChargeGroup = chargeGroupCollection.SingleOrDefault(cg => cg.ChargeGroup == "FRT");
			var totalFRT = converter.Convert(frtChargeGroup?.Charges, frtChargeGroup?.Parent);
			AssertEquals($"{assertionMessage}: Total FRT", expectedFRTTotal > 0 ? frtChargeGroup.Parent.ConvertToCurrentCompanyFormat(expectedFRTTotal) : string.Empty, totalFRT);

			var orgChargeGroup = chargeGroupCollection.SingleOrDefault(cg => cg.ChargeGroup == "ORG");
			var totalORG = converter.Convert(orgChargeGroup?.Charges, orgChargeGroup?.Parent);
			AssertEquals($"{assertionMessage}: Total ORG", expectedORGTotal > 0 ? orgChargeGroup.Parent.ConvertToCurrentCompanyFormat(expectedORGTotal) : string.Empty, totalORG);

			var dstChargeGroup = chargeGroupCollection.SingleOrDefault(cg => cg.ChargeGroup == "DST");
			var totalDST = converter.Convert(dstChargeGroup?.Charges, dstChargeGroup?.Parent);
			AssertEquals($"{assertionMessage}: Total DST", expectedDSTTotal > 0 ? dstChargeGroup.Parent.ConvertToCurrentCompanyFormat(expectedDSTTotal) : string.Empty, totalDST);
		}

		public void TestConvertOptionalCharges()
		{
			var testHelper = new TestHelper(Factory);

			testHelper.ChargeCodes.NewConsolChargeCode("BL1", "BOL Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL2", "Optional Fee BOL FRT 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);
			testHelper.ChargeCodes.NewConsolChargeCode("BL3", "Optional Fee BOL FRT 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, companyPK: GlbCompany.CurrentCompany.PK);

			var chargesViewModel = SetupChargesViewModel(new[]
			{
				("BL1", false, 10m),
				("BL2", true, 100m),
				("BL3", true, 1000m),
			});

			var chargeViewModel1 = chargesViewModel.Charges.Single(charge => charge.Code == "BL1");
			var chargeViewModel2 = chargesViewModel.Charges.Single(charge => charge.Code == "BL2");
			var chargeViewModel3 = chargesViewModel.Charges.Single(charge => charge.Code == "BL3");

			CombineAssertions("GroupTotal should only sum enabled charges", () =>
			{
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 10m, expectedORGTotal: 0m, expectedDSTTotal: 0m, assertionMessage: "FRT: Only not optional charge is active");

				chargeViewModel2.IsActive = true;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 110m, expectedORGTotal: 0m, expectedDSTTotal: 0m, assertionMessage: "FRT: Enabling BL2");

				chargeViewModel3.IsActive = true;
				AssertChargeGroupTotalConverter(chargesViewModel.ChargeGroupsView, expectedFRTTotal: 1110m, expectedORGTotal: 0m, expectedDSTTotal: 0m, assertionMessage: "FRT: Enabling BL3");
			});
		}

		public void TestConvert()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			consol.JK_ConsolMode = "LCL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var charge1 = ChooserHelper.AddFlatCharge(apiCosting1, "CH1", 100m, mapCodeWithCW1ChargeCode: true);
			var charge2 = ChooserHelper.AddFlatCharge(apiCosting1, "CH2", 10m, mapCodeWithCW1ChargeCode: true);
			var charge3 = ChooserHelper.AddFlatCharge(apiCosting1, "CH3", 7m, mapCodeWithCW1ChargeCode: true);
			var charge4 = ChooserHelper.AddFlatCharge(apiCosting1, "CH4", 999m, mapCodeWithCW1ChargeCode: true);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedEntries = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(convertedEntries, consol.RatingAdapter.OperationalJobCode);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedEntries, services, results);

			var convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new[]
			{
				new ChargeViewModel(charge1, convertedLines.First(l => l.WiseCharge == charge1), chooserRateEntry, true),
				new ChargeViewModel(charge2, convertedLines.First(l => l.WiseCharge == charge2), chooserRateEntry, true),
				new ChargeViewModel(charge3, convertedLines.First(l => l.WiseCharge == charge3), chooserRateEntry, true),
				new ChargeViewModel(charge4, convertedLines.First(l => l.WiseCharge == charge4), chooserRateEntry, true)
			};
			var chargesGroup = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, charges);

			TotalConverter totalConverter = new TotalConverter();

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, totalConverter.Convert(null, null));
				AssertEquals(string.Empty, totalConverter.Convert(Enumerable.Empty<ChargeViewModel>(), null));
				AssertEquals(string.Empty, totalConverter.Convert(null, chargesGroup));
				AssertEquals("1116", totalConverter.Convert(charges, null));
				AssertEquals("AUD $1116.00", totalConverter.Convert(charges, chargesGroup));
			});
		}

		#region Implementation

		protected RateChooserTestHelper ChooserHelper => chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory));
		RateChooserTestHelper chooserHelper;

		ChargesViewModel SetupChargesViewModel((string code, bool isOptional, decimal price)[] cw1ChargeCodes)
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			var adapterId = consol.RatingAdapter.OperationalJobCode;

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var apiCharges = new List<Charge>();
			foreach (var cw1ChargeCode in cw1ChargeCodes)
			{
				// CarrierChargeCodeInfo.Group(s) do not affect test results. Just use default group value.
				var apiCharge = RateChooserTestHelper.AddFlatCharge(apiCosting, cw1ChargeCode.code, cw1ChargeCode.price);
				if (cw1ChargeCode.isOptional)
				{
					apiCharge.ChargeType |= ChargeType.Optional;
					apiCharge.IsOptional = true;
				}
				apiCharges.Add(apiCharge);
			}

			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var converter = new WiseRatesConverter(Factory, new ElementaryLogger());
			var convertedRates = converter.Convert(response, null);
			var convertedLines = convertedRates
				.SelectMany(c => c.ChildRateLines)
				.OfType<WiseLine>().ToList();

			var rateInfoCollection = new AutoRateInfoCollection(Factory);
			foreach (var apiCharge in apiCharges)
			{
				var chargeCode = apiCharge.ChargeCode;
				var rateInfo = new AutoRateInfo(Factory)
				{
					ChargeCode = Helper.ChargeCodes[chargeCode],
					Currency = "USD",
					ChargeUnit = "KG"
				};
				rateInfo.SetLine_ForTest(convertedLines.First(rl => rl.ChargeCode.AC_Code == chargeCode));
				rateInfo.AddFlatPaymentBasis(apiCharge.FlatRate.Value, adapterId);

				rateInfoCollection.Add(rateInfo);
			}

			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting, convertedRates, services, rateInfoCollection);
			var chargeViewModels = apiCharges.Select(apiCharge => new ChargeViewModel(apiCharge, convertedLines.First(wiseLIne => wiseLIne.WiseCharge == apiCharge), chooserRateEntry, true));
			return new ChargesViewModel(ChargesViewModel.ChargesGroup.BOL, services, chargeViewModels);
		}

		#endregion
	}
}
