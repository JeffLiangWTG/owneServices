using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Constants;
using Api = WiseRates.Api;
using Constants = Enterprise.Core.Constants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Test
{
	public class ChooserRateRowTest : RatingTestCase
	{
		public void TestNullRate()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var row = new ChooserRateRow(containerInfo, null, null, null);
			AssertEquals(true, string.IsNullOrEmpty(row.Account));
			AssertEquals(true, string.IsNullOrEmpty(row.AddOn));
			AssertEquals(true, string.IsNullOrEmpty(row.AddOnOrCarrier));
			AssertEquals(true, string.IsNullOrEmpty(row.CarrierServiceLevel));
			AssertEquals(true, string.IsNullOrEmpty(row.Commodities));
			AssertEquals("GEN", row.CommodityCode);
			AssertEquals(0, row.CommodityExcluded.Length);
			AssertEquals(0, row.CommodityIncluded.Length);
			AssertEquals(true, string.IsNullOrEmpty(row.CommodityName));
			AssertEquals(containerInfo, row.ContainerInfo);
			AssertEquals(true, string.IsNullOrEmpty(row.CommodityType));
			AssertEquals("20GP", row.ContainerType);
			AssertEquals(true, string.IsNullOrEmpty(row.ContractNumber));
			AssertEquals(true, string.IsNullOrEmpty(row.ServiceProviderText));
			AssertEquals(true, string.IsNullOrEmpty(row.ServiceProviderToolTip));
			AssertEquals(ZDateTime.Empty, row.EffectiveDate);
			AssertEquals(ZDateTime.Empty, row.ExpiryDate);
			AssertNull(row.Rate);
			AssertEquals(true, string.IsNullOrEmpty(row.RateCategory));
			AssertEquals(true, string.IsNullOrEmpty(row.RateType));
			AssertEquals(true, string.IsNullOrEmpty(row.RateType2));
			AssertEquals(true, string.IsNullOrEmpty(row.RateType2OrConsignor));
			AssertEquals(true, string.IsNullOrEmpty(row.RoutingOrTransitTime));
			AssertEquals(true, string.IsNullOrEmpty(row.TradeLane));
			AssertEquals(true, string.IsNullOrEmpty(row.Vessel));
			AssertEquals(true, string.IsNullOrEmpty(row.VesselOrConsignee));
			AssertNull(row.CW1BillOfLadingCharges);
			AssertNull(row.CW1FreightCharges);
			AssertNull(row.OceanCharges);
			AssertNull(row.InlandCharges);
			AssertNull(row.OutlandCharges);

			foreach (var publicProperty in typeof(ChooserRateRow).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if (publicProperty.CanRead)
				{
					AssertNoExceptionThrown(publicProperty.Name, () => publicProperty.GetValue(row));
				}
			}
		}

		public void TestTruncateServiceProviderName()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m)
				.WithCurrency("USD");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR3", 7m)
				.WithCurrency("USD")
				.OfType(Api.Model.ChargeType.Optional);
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BAF", 150m)
				.WithCurrency("HKD");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BAF", 75m)
				.WithCurrency("NZD");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedRates = converter.Convert(response, null);

			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var modelRate = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedRates, services, null);
			var row = new ChooserRateRow(containerInfo, modelRate, null, null);

			var testCases = new (string actualName, string truncatedName)[]
			{
				("PAI", "PAI"),
				("A.b. Lob Lob Lob ", "A.b. Lob Lob Lob"),
				("1234567890 1234567890", "1234567890"),
				("12345678901234567890", "12345678901234567890"),
				("1234567890123456789 0", "1234567890123456789"),
				("123456789012345678901234567890", "123456789012345678901234567890"),
				("", ""),
				("   ", ""),
				(" 12345678901234567 89 0", "12345678901234567 89"),
			};

			foreach (var testCase in testCases)
			{
				carrier.OH_FullName = testCase.actualName;

				AssertEquals(row.ServiceProviderText, testCase.truncatedName + " (SCAC)");
			}
		}

		public void TestBaseCharges_CS_FRT_DescriptionMultipleChargeTypes()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var freightCharge = Helper.ChargeCodes["FRT"];
			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL, 3);
			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			frtCharge1.Comment = "Subject To Charges\r\nZAC = Some Zac Charge";
			var incCharge1 = new Api.Model.Charge
			{
				ChargeCode = "AAA",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.Included | Api.Model.ChargeType.SubjectTo,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = "CS1", Description = "CS1 Desc" },
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode
			};
			apiCosting1.Charges.Add(incCharge1);

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals(
				"Description does not match the expected format",
				freightCharge.AC_DescMultilingual + @"

Inclusive charges:
	CS1 - CS1 Desc (Universal Code: AAA) (Included, Subject To)",
				row.OceanCharges.BaseCharges.Charges.First().Description
			);
		}

		public void TestBaseCharges_CS_FRT_DescriptionIncludesNoAmountCharges()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var freightCharge = Helper.ChargeCodes["FRT"];
			var includedCharge1 = Helper.ChargeCodes.NewConsolChargeCode("INC", "Included Charge 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			includedCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL, 3);
			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			var incCharge1 = new Api.Model.Charge
			{
				ChargeCode = "INC",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.Included,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = "CS1", Description = "CS1 Desc" },
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode
			};
			apiCosting1.Charges.Add(incCharge1);

			var incCharge2 = new Api.Model.Charge
			{
				ChargeCode = "INC",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.Included,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode() { Code = "CS2", Description = "CS2 Desc" },
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode
			};
			apiCosting1.Charges.Add(incCharge2);

			var incChargeUnmapped = new Api.Model.Charge
			{
				ChargeCode = "AAA",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.Included,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode
				{
					Code = "CAAA",
					Description = "Carrier AAA"
				}
			};
			apiCosting1.Charges.Add(incChargeUnmapped);

			var incChargeUnmapped2 = new Api.Model.Charge
			{
				ChargeCode = "SSA",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.SubjectTo,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode
				{
					Code = "CSSA",
					Description = "Carrier SSA"
				}
			};
			apiCosting1.Charges.Add(incChargeUnmapped2);

			var incChargeUnmapped3 = new Api.Model.Charge
			{
				ChargeCode = "SSB",
				Currency = "AUD",
				ChargeType = Api.Model.ChargeType.NotApplicable,
				CustomCategory = WRConstants.ChargeCustomCategory.Ocean,
				FreightInclusiveCarriageCharge = frtCharge1.ChargeCode,
				CarrierChargeCodeInfo = new Api.Model.CarrierSpecificChargeCode
				{
					Code = "CSSB",
					Description = "Carrier SSB"
				}
			};
			apiCosting1.Charges.Add(incChargeUnmapped3);

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var row = CreateChooserRateRow(apiCosting1, containerInfo);

			var expectedDescription = @"International Freight

Inclusive charges:
	INC - Included Charge 1 (Included)
	CAAA - Carrier AAA (Universal Code: AAA) (Included)
	CSSA - Carrier SSA (Universal Code: SSA) (Subject To)
	CSSB - Carrier SSB (Universal Code: SSB) (Not Applicable)";

			AssertEquals(
				"Description of row's first charge should match the expected format.",
				expectedDescription,
				row.OceanCharges.BaseCharges.Charges.First().Description
			);
		}

		public void TestPricePerContainerError_MissingExchangeRate()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m, mapCodeWithCW1ChargeCode: true);
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD");
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FR3", 7m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD")
				.OfType(Api.Model.ChargeType.Optional);
			ChooserHelper.AddPerContainerCharge(apiCosting1, "BAF", 150m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("HKD");
			ChooserHelper.AddPerContainerCharge(apiCosting1, "BAF", 75m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("NZD");

			var row =
				CreateChooserRateRowWithCurrencyMissingRates(
					apiCosting1,
					carrier,
					defaultCurrency: "USD",
					currenciesMissingRates: new[] { "HKD", "NZD" }
				);

			var charges = row.OceanCharges.Charges.First(c => c.Group == ChargesViewModel.ChargesGroup.Base);

			AssertEquals(string.Empty, charges.TotalPriceString);
			AssertEquals("Missing exchange rate(s) for HKD, NZD to USD", charges.TotalPriceErrorString);
			AssertEquals(true, row.PlainLogs.Contains("Missing exchange rate(s) for HKD, NZD to USD"));
			AssertEquals(true, charges.TotalPriceErrorVisibility);
		}

		public void TestBOLPriceError_MissingExchangeRate()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Factory.Save(); // carrier mapping is a DBOnly query

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD");
			ChooserHelper.AddFlatCharge(apiCosting1, "BOL", 30m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD")
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			ChooserHelper.AddFlatCharge(apiCosting1, "BL2", 3m, mapCodeWithCW1ChargeCode: true)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL)
				.WithCurrency("HKD");
			ChooserHelper.AddFlatCharge(apiCosting1, "BL3", 44m, mapCodeWithCW1ChargeCode: true)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL)
				.WithCurrency("NZD");

			var row = CreateChooserRateRowWithCurrencyMissingRates(apiCosting1, carrier, defaultCurrency: "USD", currenciesMissingRates: new[] { "HKD", "NZD" });

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, row.BOLCharges.TotalPriceString);
				AssertEquals("Missing exchange rate(s) for HKD, NZD to USD", row.BOLCharges.TotalPriceErrorString);
				AssertEquals(true, row.BOLCharges.TotalPriceErrorVisibility);
			});
		}

		public void TestFRTCodes()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Helper.ChargeCodes.NewConsolChargeCode("EDS", "Door Surcharge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Helper.ChargeCodes.NewConsolChargeCode("LIN", "Alameda Corridor Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			Helper.ChargeCodes.NewConsolChargeCode("OPT", "Optional Freight Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			Factory.Save();

			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");

			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000, "FRT", "FRT", "Freight");
			var row = CreateChooserRateRow(apiCosting1, containerInfo);

			AssertEquals("FRT", row.OceanCharges.BaseCharges.ActiveChargeCodesString);

			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "EDS", 200, "DST", "EDSC", "Door Surcharge");
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals("EDS, FRT", row.OceanCharges.BaseCharges.ActiveChargeCodesString);

			RateChooserTestHelper.AddFlatCharge(apiCosting1, "LIN", 0m, "FRT", "ACC", "Alameda Corridor Charge")
				.OfType(Api.Model.ChargeType.Included)
				.IncludedIn("FRT");
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals("EDS, FRT", row.OceanCharges.BaseCharges.ActiveChargeCodesString);

			var optionalCharge = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "OPT", 70, "FRT", "OPT", "Optional Freight Charge")
				.OfType(Api.Model.ChargeType.Additional);
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertNotNull("PRE: Initialized", row.OceanCharges.AdditionalCharges);
			AssertEquals("EDS, FRT", row.OceanCharges.BaseCharges.ActiveChargeCodesString);

			row.Rate.SetActive(optionalCharge, true);
			AssertEquals("EDS, FRT", row.OceanCharges.BaseCharges.ActiveChargeCodesString);
		}

		public void TestAddCodes()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Helper.ChargeCodes.NewConsolChargeCode("EDS", "Door Surcharge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Helper.ChargeCodes.NewConsolChargeCode("OPT", "Optional Freight Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			Factory.Save();

			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");

			var frtCharge = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000, "FRT", "FRT", "Freight")
				.OfType(Api.Model.ChargeType.Additional);
			var row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertNotNull("PRE: Initialized", row.OceanCharges.AdditionalCharges);
			AssertEquals("", row.OceanCharges.AdditionalCharges.ActiveChargeCodesString);

			row.Rate.SetActive(frtCharge, true);
			AssertEquals("FRT", row.OceanCharges.AdditionalCharges.ActiveChargeCodesString);

			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "EDS", 200, "DST", "EDSC", "Door Surcharge");
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals("Non Add. charge not present", "", row.OceanCharges.AdditionalCharges.ActiveChargeCodesString);

			var optionalCharge = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "OPT", 70, "FRT", "OPT", "Optional Freight Charge")
				.OfType(Api.Model.ChargeType.Additional);
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertNotNull("PRE: Initialized", row.OceanCharges.AdditionalCharges);
			row.Rate.SetActive(optionalCharge, true);
			AssertEquals("OPT", row.OceanCharges.AdditionalCharges.ActiveChargeCodesString);

			row.Rate.SetActive(frtCharge, true);
			AssertEquals("FRT, OPT", row.OceanCharges.AdditionalCharges.ActiveChargeCodesString);
		}

		public void TestBOLCodes()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Helper.ChargeCodes.NewConsolChargeCode("DFD", "Doc Fee at Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Helper.ChargeCodes.NewConsolChargeCode("SEC", "Sec Doc Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			Helper.ChargeCodes.NewConsolChargeCode("OPT", "Optional Doc Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			Factory.Save();

			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			RateChooserTestHelper.AddFlatCharge(apiCosting1, "DFD", 50, "DST", "CDFD", "Doc Fee at Destination")
				.WithCustomCategory("BOL");

			var row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals("DFD", row.BOLCharges.ActiveChargeCodesString);

			RateChooserTestHelper.AddFlatCharge(apiCosting1, "SEC", 30, "FRT", "SCMC", "Security Compliance Management Charge")
				.WithCustomCategory("BOL");
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals("DFD, SEC", row.BOLCharges.ActiveChargeCodesString);

			var optionalCharge = RateChooserTestHelper.AddFlatCharge(apiCosting1, "OPT", 30, "DST", "OPT", "Optional Doc Charge")
				.WithCustomCategory("BOL")
				.OfType(Api.Model.ChargeType.Bol);
			optionalCharge.IsOptional = true;
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertNotNull("PRE: Initialized", row.BOLCharges);
			AssertEquals("Inactive, optional charge not included", "DFD, SEC", row.BOLCharges.ActiveChargeCodesString);

			row.Rate.SetActive(optionalCharge, true);
			AssertEquals("Active, optional charge is included", "DFD, OPT, SEC", row.BOLCharges.ActiveChargeCodesString);
		}

		public void TestFRTAndBOLCodes_CW1()
		{
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			Helper.ChargeCodes.NewConsolChargeCode("DFD", "Doc Fee at Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Helper.ChargeCodes.NewConsolChargeCode("SEC", "Sec Doc Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			var rateLine2 = rateEntry1.AddRateLine(Helper.ChargeCodes["DFD"], FlatCalculator.Code, "", "AUD");
			var rateLine3 = rateEntry1.AddRateLine(Helper.ChargeCodes["SEC"], FlatCalculator.Code, "", "AUD");
			Factory.Save();

			var calculatedResult = new AutoRateInfoCollection(Factory);
			calculatedResult.Add(new AutoRateInfo(Factory, rateLine1));
			calculatedResult.Add(new AutoRateInfo(Factory, rateLine2));
			calculatedResult.Add(new AutoRateInfo(Factory, rateLine3));

			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var modelRate = new ChooserRateEntry(Factory, rateEntry1, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), calculatedResult);
			var row = new ChooserRateRow(containerInfo, modelRate, null, null);
			AssertEquals("FRT", row.CW1FreightCharges.ActiveChargeCodesString);
			AssertEquals("DFD, SEC", row.CW1BillOfLadingCharges.ActiveChargeCodesString);
		}

		public void TestContractAllocationSimulationHyperlink_Visibility()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			Factory.Save();

			var containerInfo = new ChooserContainerCommodity(rateEntry1.Container, string.Empty, "GEN", 1);
			var mockFiltersUsed = new Mock<RateSelector.IRateSelectorFilterValueProvider>();
			var testCriteria = new TestRatingCriteria();
			testCriteria.SetAutoRatedFor(new Collection<IBusiness>() { Factory.NewWithValidTestData<ForwardingConsol>() });
			mockFiltersUsed.Setup(x => x.OriginalCriteria).Returns(testCriteria);

			var rateEntry = new ChooserRateEntry(Factory, null, rateEntry1, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), new AutoRateInfoCollection(Factory), mockFiltersUsed.Object);
			var row = new ChooserRateRow(containerInfo, rateEntry, null, null);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, row.ContractNumberAllocationHyperlinkVisibility);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, row.ContractNumberAllocationHyperlinkVisibility);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, row.ContractNumberAllocationHyperlinkVisibility);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, row.ContractNumberAllocationHyperlinkVisibility);
			}
		}

		[GuiTest]
		public void TestContractAllocationAttachForm_GivenConsolWithCW1Rate_ThenFormOpens()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
				var costing = Helper.NewCosting(carrier);
				costing.TH_GC = Env.CurrentCompanyPK;
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				Factory.Save();

				var containerInfo = new ChooserContainerCommodity(rateEntry1.Container, string.Empty, "GEN", 1);
				var mockFiltersUsed = new Mock<RateSelector.IRateSelectorFilterValueProvider>();
				var testCriteria = new TestRatingCriteria();
				testCriteria.SetAutoRatedFor(new Collection<IBusiness>() { Factory.NewWithValidTestData<ForwardingConsol>() });
				mockFiltersUsed.Setup(x => x.OriginalCriteria).Returns(testCriteria);

				var rateEntry = new ChooserRateEntry(Factory, null, rateEntry1, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), new AutoRateInfoCollection(Factory), mockFiltersUsed.Object);
				var row = new ChooserRateRow(containerInfo, rateEntry, null, null);
				var seenPopup = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is Form form)
					{
						AssertEquals("ContractAndAllocationsAttachForm", form.Name);

						seenPopup = true;
					}
				});

				row.PopupContractAndAllocationsAttachForm();

				AssertEquals(true, seenPopup);
			}
		}

		[GuiTest]
		public void TestContractAllocationAttachForm_GivenShipmentWithCW1Rate_ThenFormOpens()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
				var costing = Helper.NewCosting(carrier);
				costing.TH_GC = Env.CurrentCompanyPK;
				var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
				Factory.Save();

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = "FCL";

				var containerInfo = new ChooserContainerCommodity(rateEntry1.Container, string.Empty, "GEN", 1);
				var mockFiltersUsed = new Mock<RateSelector.IRateSelectorFilterValueProvider>();
				var testCriteria = new TestRatingCriteria();
				testCriteria.SetAutoRatedFor(new Collection<IBusiness>() { shipment });
				mockFiltersUsed.Setup(x => x.OriginalCriteria).Returns(testCriteria);

				var rateEntry = new ChooserRateEntry(Factory, null, rateEntry1, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), new AutoRateInfoCollection(Factory), mockFiltersUsed.Object);
				var row = new ChooserRateRow(containerInfo, rateEntry, null, null);
				var seenPopup = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is Form form)
					{
						AssertEquals("ContractAndAllocationsAttachForm", form.Name);

						seenPopup = true;
					}
				});

				row.PopupContractAndAllocationsAttachForm();

				AssertEquals(true, seenPopup);
			}
		}

		public void TestContractAllocationAttachForm_GivenConsolWithRateServiceRate_CarrierAssigned_ThenFormwOpens()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("CAR1");
			TestContractAllocationAttachForm_GivenConsolWithRateServiceRate_ThenFormOpens(carrier);
		}

		public void TestContractAllocationAttachForm_GivenConsolWithRateServiceRate_CarrierNotAssigned_ThenFormwOpens()
		{
			TestContractAllocationAttachForm_GivenConsolWithRateServiceRate_ThenFormOpens(null);
		}

		void TestContractAllocationAttachForm_GivenConsolWithRateServiceRate_ThenFormOpens(OrgHeader carrier)
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

				var containerInfo = new ChooserContainerCommodity(container, string.Empty, "GEN", 1);
				var mockFiltersUsed = new Mock<RateSelector.IRateSelectorFilterValueProvider>();
				var testCriteria = new TestRatingCriteria();
				testCriteria.SetAutoRatedFor(new Collection<IBusiness>() { Factory.NewWithValidTestData<ForwardingConsol>() });
				mockFiltersUsed.Setup(x => x.OriginalCriteria).Returns(testCriteria);

				var apiCosting = RateChooserTestHelper.CreateApiRate(GP20, carrier, "");
				apiCosting.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

				var wiseRates = new[] { apiCosting };
				var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
				var context = new WiseRatesConversionContext(response, testCriteria);
				var converter = new WiseRatesConverter(Factory, (new Mock<ILogger>()).Object);
				var converted = converter.Convert(context, new[] { response.Rates[0] });

				var rateEntry = new ChooserRateEntry(Factory, null, carrier, apiCosting, converted, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), new AutoRateInfoCollection(Factory), mockFiltersUsed.Object);
				Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed = true;
				var row = new ChooserRateRow(containerInfo, rateEntry, null, null);
				var seenPopup = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is Form form)
					{
						AssertEquals("ContractAndAllocationsAttachForm", form.Name);

						seenPopup = true;
					}
				});

				row.PopupContractAndAllocationsAttachForm();

				AssertEquals(true, seenPopup);
			}
		}

		#region Calculated Logs

		public void TestCalculatedLogs_CargoWiseFCL()
		{
			var carrier1 = ChooserHelper.CreateCarrierOrg("CAR1");
			var costing1 = Helper.NewCosting(carrier1);
			var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var carrier2 = ChooserHelper.CreateCarrierOrg("CAR2");
			var costing2 = Helper.NewCosting(carrier2);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN, "AUD");

			var consol = ChooserHelper.CreateConsol();
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry2));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerTab1 = viewModel.ContainerTabs.First();

			var chooserRateRow1 = containerTab1.Rates[0];
			AssertEquals(
				"Log message for the first rate row does not match.",
				@"Information: RateLine Found FRT-UNT-CN-20GP-Costing CAR1CARRIER
Information: Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Costing CAR1CARRIER
					Job's info:
					Container 20GP: 3 ContainerCount
",
				chooserRateRow1.PlainLogs
			);

			var chooserRateRow2 = containerTab1.Rates[1];
			AssertEquals(
				"Log message for the second rate row does not match.",
				@"Information: RateLine Found BAF-UNT-CN-20GP-Costing CAR2CARRIER
Information: Chargeable was added for
				RateLine BAF-UNT-CN-20GP-Costing CAR2CARRIER
					Job's info:
					Container 20GP: 3 ContainerCount
",
				chooserRateRow2.PlainLogs
			);
		}

		public void TestCalculatedLogs_CargoWiseLCL()
			{
				var carrier1 = ChooserHelper.CreateCarrierOrg("CAR1");
				var costing1 = Helper.NewCosting(carrier1);
				var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "USLAX", "HKHKG", "");
				rateEntry1.RateLines.RemoveAndDeleteAll();
				rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, "AUD");

				var carrier2 = ChooserHelper.CreateCarrierOrg("CAR2");
				var costing2 = Helper.NewCosting(carrier2);
				var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "USLAX", "HKHKG", "");
				rateEntry2.RateLines.RemoveAndDeleteAll();
				rateEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG, "AUD");

				var consol = ChooserHelper.CreateConsol();
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_ActualWeight = 5000;
				shipment1.JS_UnitOfWeight = "KG";

				var logger = new ElementaryLogger();
				var context = new RatingContext(logger);
				var autoRating = consol.RatingAdapter;
				var autoRatingInfo = new AutoRatingProxy(autoRating);
				var criteria = new RatingCriteria(autoRatingInfo, Factory);
				var model = new RateChooserModel(criteria, context);
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntry1));
				model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntry2));
				var viewModel = new RateChooserViewModel(model);
				viewModel.RefreshRates();

				var containerTab1 = viewModel.ContainerTabs.First();

				var chooserRateRow1 = containerTab1.Rates[0];
				AssertEquals(
					"Chooser rate row 1 logs should match the expected logs",
					@"Information: RateLine Found FRT-UNT-KG-Costing CAR1CARRIER
Information: Chargeable was added for
				RateLine FRT-UNT-KG-Costing CAR1CARRIER
					Job's info:
					: 0 Weight
					: 0 Volume
					: 5000 Weight
					: 0 Volume
",
					chooserRateRow1.PlainLogs
				);

				var chooserRateRow2 = containerTab1.Rates[1];
				AssertEquals(
					"Chooser rate row 2 logs should match the expected logs",
					@"Information: RateLine Found BAF-UNT-KG-Costing CAR2CARRIER
Information: Chargeable was added for
				RateLine BAF-UNT-KG-Costing CAR2CARRIER
					Job's info:
					: 0 Weight
					: 0 Volume
					: 5000 Weight
					: 0 Volume
",
					chooserRateRow2.PlainLogs
				);
			}

		public void TestCalculatedLogs_RateServiceFCL()
		{
			var carrier1 = ChooserHelper.CreateCarrierOrg("CAR1");
			var apiCosting1 = RateChooserTestHelper.CreateApiRate(GP20, carrier1, "");
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var carrier2 = ChooserHelper.CreateCarrierOrg("CAR2");
			var apiCosting2 = RateChooserTestHelper.CreateApiRate(GP20, carrier2, "");
			apiCosting2.Charges.Add(new Api.Model.Charge { ChargeCode = "BAF", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });

			var consol = ChooserHelper.CreateConsol();
			ChooserHelper.AddContainer(consol, "20GP", "GEN", "FCL", 3);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerTab1 = viewModel.ContainerTabs.First();

			var chooserRateRow1 = containerTab1.Rates[0];
			AssertEquals(
				@"Information: RateLine Found FRT-UNT-CN-20GP-Wise Costing
Information: Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Wise Costing
					Job's info:
					Container 20GP: 3 ContainerCount
",
				chooserRateRow1.PlainLogs
			);

			var chooserRateRow2 = containerTab1.Rates[1];
			AssertEquals(
				@"Information: RateLine Found BAF-UNT-CN-20GP-Wise Costing
Information: Chargeable was added for
				RateLine BAF-UNT-CN-20GP-Wise Costing
					Job's info:
					Container 20GP: 3 ContainerCount
",
				chooserRateRow2.PlainLogs
			);
		}

		public void TestCalculatedLogs_RateServiceLCL()
		{
			var carrier1 = ChooserHelper.CreateCarrierOrg("CAR1");
			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier1, "", Constants.ContainerModes.LCL);
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var carrier2 = ChooserHelper.CreateCarrierOrg("CAR2");
			var apiCosting2 = ChooserHelper.CreateApiRate("", carrier2, "", Constants.ContainerModes.LCL);
			apiCosting2.Charges.Add(new Api.Model.Charge { ChargeCode = "BAF", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var consol = ChooserHelper.CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 5000;
			shipment1.JS_UnitOfWeight = "KG";

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			model.AddWiseRatesForTest(response);
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerTab1 = viewModel.ContainerTabs.First();

			var chooserRateRow1 = containerTab1.Rates[0];
			AssertEquals(
				"Plain logs do not match for the first rate row",
				@"Information: RateLine Found FRT-CMB-KG-Wise Costing
Information: Chargeable was added for
				RateLine FRT-CMB-KG-Wise Costing
					Job's info:
					: 0 Weight
					: 0 Volume
					: 5000 Weight
					: 0 Volume
",
				chooserRateRow1.PlainLogs
			);

			var chooserRateRow2 = containerTab1.Rates[1];
			AssertEquals(
				"Plain logs do not match for the second rate row",
				@"Information: RateLine Found BAF-CMB-KG-Wise Costing
Information: Chargeable was added for
				RateLine BAF-CMB-KG-Wise Costing
					Job's info:
					: 0 Weight
					: 0 Volume
					: 5000 Weight
					: 0 Volume
",
				chooserRateRow2.PlainLogs
			);
		}

		public void TestTotalNotification_ShouldNotHaveError_WhenChargesAreFiltered()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var carrier1 = ChooserHelper.CreateCarrierOrg("CAR1");
			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier1, "", Constants.ContainerModes.LCL);
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 12m });

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL, 3);

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var row = CreateChooserRateRow(apiCosting1, containerInfo);

			AssertNullOrEmpty("row.TotalIconToolTip", row.TotalIconToolTip);
		}

		#endregion

		public void TestCommodityInfoVisible_WiseRate()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var apiCosting1 = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting1.Provider = "URS";
			apiCosting1.TransportMode = "SEA";

			var row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals(true, row.CommodityInfoVisibility);

			apiCosting1.TransportMode = "AIR";
			row = CreateChooserRateRow(apiCosting1, containerInfo);
			AssertEquals(false, row.CommodityInfoVisibility);

			apiCosting1.Provider = "CGSP";
			AssertEquals(true, row.CommodityInfoVisibility);

			apiCosting1.Provider = "CGGD";
			AssertEquals(false, row.CommodityInfoVisibility);
		}

		ChooserRateRow CreateChooserRateRow(Api.Model.Rate costing, ChooserContainerCommodity containerInfo)
		{
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(costing);
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedRates = converter.Convert(response, null);
			var modelRate = new ChooserRateEntry(Factory, serviceProvider: null, costing, convertedRates, new RateChooserServices(Factory, ZDateTime.Today, "AUD"), null);
			return new ChooserRateRow(containerInfo, modelRate, null, null);
		}

		ChooserRateRow CreateChooserRateRowWithCurrencyMissingRates(Api.Model.Rate costing, OrgHeader carrier, ZString defaultCurrency, string[] currenciesMissingRates)
		{
			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = defaultCurrency;

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerInfo = new ChooserContainerCommodity(refContainer20, string.Empty, "GEN", 1);
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(costing);
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedRates = converter.Convert(response, null);

			var results = ChooserHelper.GetCalculatedResultFromEntries(convertedRates, carrier, refContainer20, logger);
			var services = new DummyRateChooserServices(Factory, currenciesMissingRates);
			var modelRate = new ChooserRateEntry(Factory, serviceProvider: carrier, costing, convertedRates, services, results);
			return new ChooserRateRow(containerInfo, modelRate, null, null);
		}

		protected RateChooserTestHelper ChooserHelper
		{
			get { return helper ?? (helper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper helper;
	}
}
