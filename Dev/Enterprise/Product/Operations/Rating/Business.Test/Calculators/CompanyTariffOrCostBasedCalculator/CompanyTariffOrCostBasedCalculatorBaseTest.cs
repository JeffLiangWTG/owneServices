using System.Collections.Generic;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class CompanyTariffOrCostBasedCalculatorBaseTest : RatingTestCase
	{
		#region AutoRate

		#region Autorate DropMode

		public void TestAutorate_DropMode_BlankDropModeJob5() => TestAutorate_DropMode(originalRateDropMode: "ANY", rateDropMode: "", jobDropMode: "", expectedCharges: ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob6() => TestAutorate_DropMode("ANY", "ANY", "", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob7() => TestAutorate_DropMode("ANY", "HSL", "", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_BlankDropModeJob8() => TestAutorate_DropMode("ANY", "HUL", "", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_BlankDropModeJob9() => TestAutorate_DropMode("HSL", "", "", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob10() => TestAutorate_DropMode("HSL", "ANY", "", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob11() => TestAutorate_DropMode("HSL", "HSL", "", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_BlankDropModeJob12() => TestAutorate_DropMode("HSL", "HUL", "", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_BlankDropModeJob13() => TestAutorate_DropMode("HUL", "", "", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob14() => TestAutorate_DropMode("HUL", "ANY", "", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_BlankDropModeJob15() => TestAutorate_DropMode("HUL", "HSL", "", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_BlankDropModeJob16() => TestAutorate_DropMode("HUL", "HUL", "", ExpectedChargesForNoMatch);

		public void TestAutorate_DropMode_ANYDropModeJob5() => TestAutorate_DropMode("ANY", "", "ANY", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob6() => TestAutorate_DropMode("ANY", "ANY", "ANY", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob7() => TestAutorate_DropMode("ANY", "HSL", "ANY", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_ANYDropModeJob8() => TestAutorate_DropMode("ANY", "HUL", "ANY", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_ANYDropModeJob9() => TestAutorate_DropMode("HSL", "", "ANY", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob10() => TestAutorate_DropMode("HSL", "ANY", "ANY", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob11() => TestAutorate_DropMode("HSL", "HSL", "ANY", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_ANYDropModeJob12() => TestAutorate_DropMode("HSL", "HUL", "ANY", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_ANYDropModeJob13() => TestAutorate_DropMode("HUL", "", "ANY", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob14() => TestAutorate_DropMode("HUL", "ANY", "ANY", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_ANYDropModeJob15() => TestAutorate_DropMode("HUL", "HSL", "ANY", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_ANYDropModeJob16() => TestAutorate_DropMode("HUL", "HUL", "ANY", ExpectedChargesForNoMatch);

		public void TestAutorate_DropMode_HSLDropModeJob5() => TestAutorate_DropMode("ANY", "", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob6() => TestAutorate_DropMode("ANY", "ANY", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob7() => TestAutorate_DropMode("ANY", "HSL", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob8() => TestAutorate_DropMode("ANY", "HUL", "HSL", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_HSLDropModeJob9() => TestAutorate_DropMode("HSL", "", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob10() => TestAutorate_DropMode("HSL", "ANY", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob11() => TestAutorate_DropMode("HSL", "HSL", "HSL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob12() => TestAutorate_DropMode("HSL", "HUL", "HSL", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_HSLDropModeJob13() => TestAutorate_DropMode("HUL", "", "HSL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob14() => TestAutorate_DropMode("HUL", "ANY", "HSL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob15() => TestAutorate_DropMode("HUL", "HSL", "HSL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HSLDropModeJob16() => TestAutorate_DropMode("HUL", "HUL", "HSL", ExpectedChargesForNoMatch);

		public void TestAutorate_DropMode_HULDropModeJob5() => TestAutorate_DropMode("ANY", "", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob6() => TestAutorate_DropMode("ANY", "ANY", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob7() => TestAutorate_DropMode("ANY", "HSL", "HUL", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_HULDropModeJob8() => TestAutorate_DropMode("ANY", "HUL", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob9() => TestAutorate_DropMode("HSL", "", "HUL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob10() => TestAutorate_DropMode("HSL", "ANY", "HUL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob11() => TestAutorate_DropMode("HSL", "HSL", "HUL", ExpectedChargesForNoMatch);
		public void TestAutorate_DropMode_HULDropModeJob12() => TestAutorate_DropMode("HSL", "HUL", "HUL", ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob13() => TestAutorate_DropMode("HUL", "", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob14() => TestAutorate_DropMode("HUL", "ANY", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);
		public void TestAutorate_DropMode_HULDropModeJob15() => TestAutorate_DropMode("HUL", "HSL", "HUL", ExpectedChargesForNotMatchingRatesButMatchingOriginRate);
		public void TestAutorate_DropMode_HULDropModeJob16() => TestAutorate_DropMode("HUL", "HUL", "HUL", ExpectedChargesForMatchingClientRateAndOriginalRate);

		void TestAutorate_DropMode(string originalRateDropMode, string rateDropMode, string jobDropMode, SimpleArInfo[] expectedCharges)
		{
			var originalRateEntry = RatingHeader.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			AddCartageCalculatorRateLine(originalRateEntry, "ODOC", QuantityUnit.CN, perUnit: 10m, originalRateDropMode);

			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			clientRateEntry.TI_OH_TransportProvider = TransportProvider1.PK;
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: rateDropMode, applyToLine: null);
			clientRate.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, containers: new TestContainers(Factory, "20GP", 1), weightInKG: 100m, volumeInM3: 0m, clientRate.Header);
			testObject.Carrier = TransportProvider1;
			testObject.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string>() { "Provider" }));
			testObject.PickupCartageEquipment = jobDropMode;
			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var results = freightAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults
			(
				expected: expectedCharges,
				results
			);
		}

		SimpleArInfo[] ExpectedChargesForNoMatch => System.Array.Empty<SimpleArInfo>();

		SimpleArInfo[] ExpectedChargesForMatchingClientRateAndOriginalRate => new[] { new SimpleArInfo { Amount = 11m, CalculationSingleLineDescription = @"ODOC: 1 Container(s) @ AUD 11.00/Container", InvoiceLineDesc = "Origin Documentation Fee" } };

		protected abstract SimpleArInfo[] ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate { get; }

		protected abstract SimpleArInfo[] ExpectedChargesForNotMatchingRatesButMatchingOriginRate { get; }

		#endregion

		public void TestAutorate_NonApplyToLineRateLine_FLT()
		{
			var companyTariffOrCostRateEntry = RatingHeader.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			companyTariffOrCostRateEntry.AddFlatRateLine("ODOC", 10m);

			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			clientRateEntry.TI_OH_TransportProvider = TransportProvider1.PK;
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: Core.Constants.EquipmentNeeded.Any, applyToLine: null);
			clientRate.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, containers: new TestContainers(Factory, "20GP", 1), weightInKG: 100m, volumeInM3: 0m, clientRate.Header);
			testObject.Carrier = TransportProvider1;
			testObject.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string>() { "Provider" }));
			testObject.PickupCartageEquipment = FCLEquipmentNeeded.SideLoader;
			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var results = freightAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults
			(
				message: "Autorate should only return CompanyTariff/Cost that matches Job EquipmentType",
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 10m,
						InvoiceLineDesc = "Origin Documentation Fee",
						CalculationSingleLineDescription = @"ODOC: Base Rate AUD 10.00"
					}
				},
				results
			);
		}

		public void TestAutorate_ApplyToLineRateLine_FLT()
		{
			var companyTariffOrCostRateEntry = RatingHeader.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			var companyTariffOrCostRateLine = companyTariffOrCostRateEntry.AddFlatRateLine("ODOC", 10m);

			RatingHeader.Factory.Save();

			var billTo = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(billTo);
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: Core.Constants.EquipmentNeeded.Any, applyToLine: companyTariffOrCostRateLine.PK.ToString());

			clientRate.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, containers: new TestContainers(Factory, "20GP", 1), weightInKG: 100m, volumeInM3: 0m, billTo);
			testObject.PickupCartageEquipment = FCLEquipmentNeeded.SideLoader;

			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var results = freightAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults
			(
				message: "Autorate should only return CompanyTariff/Cost that matches Job EquipmentType",
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 10m,
						InvoiceLineDesc = "Origin Documentation Fee",
						CalculationSingleLineDescription = @"ODOC: Base Rate AUD 10.00"
					}
				},
				results
			);
		}

		public void TestAutorate_NonApplyToLineRateLine_CTG()
		{
			var companyTariffOrCostRateEntry = RatingHeader.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			AddCartageCalculatorRateLine(companyTariffOrCostRateEntry, "ODOC", QuantityUnit.CN, perUnit: 10m, FCLEquipmentNeeded.SideLoader);
			AddCartageCalculatorRateLine(companyTariffOrCostRateEntry, "ODOC", QuantityUnit.CN, perUnit: 20m, FCLEquipmentNeeded.WaitForUnpack);

			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			clientRateEntry.TI_OH_TransportProvider = TransportProvider1.PK;
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: FCLEquipmentNeeded.SideLoader, applyToLine: null);
			clientRate.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, containers: new TestContainers(Factory, "20GP", 1), weightInKG: 100m, volumeInM3: 0m, clientRate.Header);
			testObject.Carrier = TransportProvider1;
			testObject.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string>() { "Provider" }));
			testObject.PickupCartageEquipment = FCLEquipmentNeeded.SideLoader;
			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var results = freightAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults
			(
				message: "Autorate should only return CompanyTariff/Cost that matches Job EquipmentType",
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 11m,
						InvoiceLineDesc = "Origin Documentation Fee",
						CalculationSingleLineDescription = @"ODOC: 1 Container(s) @ AUD 11.00/Container"
					}
				},
				results
			);
		}

		public void TestAutorate_ApplyToLineRateLine_CTG()
		{
			var companyTariffOrCostRateEntry = RatingHeader.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			var companyTariffOrCostRateLine1 = AddCartageCalculatorRateLine(companyTariffOrCostRateEntry, "ODOC", QuantityUnit.CN, 10m, FCLEquipmentNeeded.SideLoader);
			var companyTariffOrCostRateLine2 = AddCartageCalculatorRateLine(companyTariffOrCostRateEntry, "ODOC", QuantityUnit.CN, 20m, FCLEquipmentNeeded.WaitForUnpack);

			RatingHeader.Factory.Save();

			var billTo = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(billTo);
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: FCLEquipmentNeeded.SideLoader, applyToLine: companyTariffOrCostRateLine1.PK.ToString());
			AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 20m, equipmentType: FCLEquipmentNeeded.WaitForUnpack, applyToLine: companyTariffOrCostRateLine2.PK.ToString());

			clientRate.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, containers: new TestContainers(Factory, "20GP", 1), weightInKG: 100m, volumeInM3: 0m, billTo);
			testObject.PickupCartageEquipment = FCLEquipmentNeeded.SideLoader;

			var freightAutoRater = new FreightAutoRater(new RatingContext());
			var results = freightAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults
			(
				message: "Autorate should only return CompanyTariff/Cost that matches Job EquipmentType",
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 11m,
						InvoiceLineDesc = "Origin Documentation Fee",
						CalculationSingleLineDescription = @"ODOC: 1 Container(s) @ AUD 11.00/Container"
					}
				},
				results
			);
		}

		RateLine AddCompanyTariffOrCostBaseCalculatorRateLine(RateEntry rateEntry, string chargeCode, decimal perUnitPercent, string equipmentType, string applyToLine)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, CalculatorCode);
			var calculator = rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calculator.PerUnitPercent = perUnitPercent;

			if (equipmentType != null)
			{
				calculator.EquipmentType = equipmentType;
			}

			if (applyToLine != null)
			{
				calculator.ApplyToLine = applyToLine;
			}

			return rateLine;
		}

		#endregion

		#region Validation

		public void TestValidate_DuplicateEmptyEquipmentType()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry(Category.ORG, RateMode.AIR, "AUSYD", "USLAX", removeLines: true);
			clientRateEntry.TI_OH_TransportProvider = TransportProvider1.PK;
			var rateLine1 = AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: "", applyToLine: null);
			var rateLine2 = AddCompanyTariffOrCostBaseCalculatorRateLine(clientRateEntry, "ODOC", perUnitPercent: 10m, equipmentType: "", applyToLine: null);

			AssertNoErrors(rateLine1.Calculator.String2Info);
			AssertNoErrors(rateLine2.Calculator.String2Info);
		}

		#endregion

		#region Show Equipment Type

		#region Apply to Line

		public void TestShowEquipmentType_GivenCTGWithApplyToLine()
		{
			var rateEntry = RatingHeader.AddRateEntry(Category.DST, RateMode.FCL, "AUSYD", "USLAX", "", "", removeLines: true);
			var rateLine = AddCartageCalculatorRateLine(rateEntry, "DDOC", QuantityUnit.CN, 10m);
			AssertEquals("Precondition: CompanyTariff CTG ShowEquipmentType", true, rateLine.Calculator.ShowEquipmentType);
			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateLine = AddRateEntryWithCTBOrCSTRateLine(clientRate, Category.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", perUnitPercent: 10m, applyToLine: rateLine);
			AssertEquals("ClientRate ShowEquipmentType", true, clientRateLine.Calculator.ShowEquipmentType);
		}

		public void TestShowEquipmentType_GivenFLTWithApplyToLine()
		{
			var rateEntry = RatingHeader.AddRateEntry(Category.DST, RateMode.FCL, "AUSYD", "USLAX", "", "", removeLines: true);
			var rateLine = rateEntry.AddFlatRateLine("DDOC", 10m);
			AssertEquals("Precondition: CompanyTariff FLT ShowEquipmentType", false, rateLine.Calculator.ShowEquipmentType);
			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateLine = AddRateEntryWithCTBOrCSTRateLine(clientRate, Category.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", perUnitPercent: 10m, applyToLine: rateLine);
			AssertEquals("ClientRate ShowEquipmentType", true, clientRateLine.Calculator.ShowEquipmentType);
		}

		#endregion

		#region No Apply To Line

		public void TestShowEquipmentType_GivenCTGWithoutApplyToLine()
		{
			var rateEntry = RatingHeader.AddRateEntry(Category.DST, RateMode.FCL, "AUSYD", "USLAX", "", "", removeLines: true);
			var rateLine = AddCartageCalculatorRateLine(rateEntry, "DDOC", QuantityUnit.CN, 10m);
			AssertEquals("Precondition: CompanyTariff CTG ShowEquipmentType", true, rateLine.Calculator.ShowEquipmentType);
			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateLine = AddRateEntryWithCTBOrCSTRateLine(clientRate, Category.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", perUnitPercent: 10m, applyToLine: null);
			AssertEquals("ClientRate ShowEquipmentType", true, clientRateLine.Calculator.ShowEquipmentType);
		}

		public void TestShowEquipmentType_GivenFLTWithoutApplyToLine()
		{
			var rateEntry = RatingHeader.AddRateEntry(Category.DST, RateMode.FCL, "AUSYD", "USLAX", "", "", removeLines: true);
			var rateLine = rateEntry.AddFlatRateLine("DDOC", 10m);
			AssertEquals("Precondition: CompanyTariff FLT ShowEquipmentType", false, rateLine.Calculator.ShowEquipmentType);
			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateLine = AddRateEntryWithCTBOrCSTRateLine(clientRate, Category.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", perUnitPercent: 10m, applyToLine: null);
			AssertEquals("ClientRate ShowEquipmentType", true, clientRateLine.Calculator.ShowEquipmentType);
		}

		public void TestShowEquipmentType_GivenCTGWithoutApplyToLineAndDifferentChargeCode()
		{
			var rateEntry = RatingHeader.AddRateEntry(Category.DST, RateMode.FCL, "AUSYD", "USLAX", "", "", removeLines: true);
			var rateLine1 = AddCartageCalculatorRateLine(rateEntry, "DCART", QuantityUnit.CN, 10m);
			var rateLine2 = rateEntry.AddFlatRateLine("DDOC", 20m);
			AssertEquals("Precondition: CompanyTariff CTG ShowEquipmentType", true, rateLine1.Calculator.ShowEquipmentType);
			AssertEquals("Precondition: CompanyTariff FLT ShowEquipmentType", false, rateLine2.Calculator.ShowEquipmentType);
			RatingHeader.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateLine = AddRateEntryWithCTBOrCSTRateLine(clientRate, Category.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", perUnitPercent: 10m, applyToLine: null);
			AssertEquals("ClientRate ShowEquipmentType", true, clientRateLine.Calculator.ShowEquipmentType);
		}

		#endregion

		#endregion

		#region Implementation

		protected static RateLine AddCartageCalculatorRateLine(RateEntry rateEntry, string chargeCode, string lineUnit, decimal perUnit, string equipmentType = default)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageCalculator.Code, lineUnit);
			var calculator = rateLine.GetCalculator<CartageCalculator>();
			calculator.PerUnit = perUnit;

			if (equipmentType != null)
			{
				calculator.EquipmentType = equipmentType;
			}

			return rateLine;
		}

		protected RateLine AddRateEntryWithCTBOrCSTRateLine(ClientRate clientRate, string category, string mode, string origin, string destination, string chargeCode, decimal perUnitPercent, RateLine applyToLine)
		{
			var clientRateEntry = clientRate.AddRateEntry(category, mode, origin, destination, "", "", removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CalculatorCode);
			var clientRateCalculator = clientRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			clientRateCalculator.PerUnitPercent = perUnitPercent;

			if (applyToLine != null)
			{
				clientRateCalculator.ApplyToLine = applyToLine.PK.ToString();
			}
			return clientRateLine;
		}

		#endregion

		protected abstract RatingHeader RatingHeader { get; }

		protected abstract string CalculatorCode { get; }
	}
}
