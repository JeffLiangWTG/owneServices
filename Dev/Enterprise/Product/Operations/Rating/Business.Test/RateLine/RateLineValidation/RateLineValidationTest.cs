using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLineValidationTest : RatingTestCase
	{
		#region TestContainerOwnership

		public void TestContainerOwnership()
		{
			TestLine.TL_WeightVolume = "CN";
			TestLine.TL_ContainerOwnership = string.Empty;
			AssertNoErrors(TestLine.TL_ContainerOwnershipInfo);

			TestLine.TL_ContainerOwnership = "XXX";
			AssertHasErrors(TestLine.TL_ContainerOwnershipInfo);

			TestLine.TL_ContainerOwnership = Constants.ContainerOwnership.Codes.ShipperOwned;
			AssertNoErrors(TestLine.TL_ContainerOwnershipInfo);
		}

		#endregion

		#region TL_ConditionalExpression

		public void TestTL_ConditionalExpression()
		{
			AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);

			TestLine.TL_Condition = RateLineConditions.UserDefined;
			AssertEquals(string.Empty, TestLine.TL_ConditionalExpression);
			AssertHasError(TestLine.TL_ConditionalExpressionInfo, "Please enter Expression");

			TestLine.TL_ConditionalExpression = "1!=1";
			AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);

			TestLine.TL_Condition = RateLineConditions.OwnBrokerage;
			AssertEquals(string.Empty, TestLine.TL_ConditionalExpression);
			AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);

			using (TestLine.GetValidationSuspender())
			{
				TestLine.TL_Condition = RateLineConditions.UserDefined;
				AssertEquals(string.Empty, TestLine.TL_ConditionalExpression);
				AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);
			}

			TestLine.TL_ConditionalExpression = "BLA=BLA";
			AssertHasErrors(TestLine.TL_ConditionalExpressionInfo);

			TestLine.TL_ConditionalExpression = "MOD=SEA";
			AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);

			TestLine.TL_ConditionalExpression = "\"<JobNumber>\"==\"Q000054321\"";
			AssertNoErrors(TestLine.TL_ConditionalExpressionInfo);
		}

		#endregion

		#region TL_Condition

		public void TestTL_Condition_NonIntercompanyTariff()
		{
			TestLine.TL_Condition = string.Empty;
			AssertNoErrors(TestLine.TL_ConditionInfo);

			TestLine.TL_Condition = "BBB";
			AssertHasErrors(TestLine.TL_ConditionInfo);

			TestLine.TL_Condition = "BRK";
			AssertNoErrors(TestLine.TL_ConditionInfo);

			TestLine.TL_Condition = RateLineConditions.OwnGateway;
			AssertNoErrors(TestLine.TL_ConditionInfo);
		}

		public void TestTL_Condition_IntercompanyTariff()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.ORG);
			var line = entry.RateLines.AddNew();

			line.TL_Condition = string.Empty;
			AssertNoErrors(line.TL_ConditionInfo);

			line.TL_Condition = "BBB";
			AssertHasErrors(line.TL_ConditionInfo);

			line.TL_Condition = RateLineConditions.OwnGateway;
			AssertHasErrors(line.TL_ConditionInfo);
		}

		#endregion

		#region Rounding

		public void TestValidateRounding()
		{
			AssertNoErrors(TestLine.TL_RoundingInfo);

			TestLine.TL_Rounding = "###";
			AssertHasError(TestLine.TL_RoundingInfo, "Enter a valid Rounding.");

			TestLine.TL_Rounding = "";
			AssertHasError(TestLine.TL_RoundingInfo, "Please enter a Rounding.");

			TestLine.TL_Rounding = RatingRoundingTypes.UpTo1;
			AssertNoErrors(TestLine.TL_RoundingInfo);
		}

		#endregion

		#region Rounding Factor

		public void TestValidateRoundingFactor()
		{
			AssertNoErrors(TestLine.TL_RoundingFactorInfo);

			TestLine.TL_Rounding = RatingRoundingTypes.Custom;

			var description = TestLine.TL_RoundingFactorInfo.HasUserDescription ? TestLine.TL_RoundingFactorInfo.Description : "value";

			TestLine.TL_RoundingFactor = -1;
			AssertHasError(TestLine.TL_RoundingFactorInfo, description + " cannot be negative.");

			TestLine.TL_RoundingFactor = 0;
			AssertHasError(TestLine.TL_RoundingFactorInfo, "Please enter a " + description + ".");

			TestLine.TL_RoundingFactor = 3;
			AssertNoErrors(TestLine.TL_RoundingFactorInfo);

			TestLine.TL_RoundingFactor = 0.1;
			AssertNoErrors(TestLine.TL_RoundingFactorInfo);
		}

		#endregion

		#region Rate Calculator

		public void TestUnitsDifferentToRateLineUnits()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var rateLine = testQuote.AddRateEntry("AIR").RateLines[0];
			rateLine.TL_RateCalculator = TimeCalculator.Code;

			var rateLineItem = rateLine.Calculator.AddRateLineItem("UNT", 0m, 3m);
			rateLineItem.TM_BreakWeightVolume = QuantityUnit.HR;

			rateLine.TL_WeightVolume = QuantityUnit.HR;
			AssertHasErrors(rateLine.TL_WeightVolumeInfo);

			rateLine.TL_WeightVolume = QuantityUnit.DY;
			AssertNoErrors(rateLineItem.TM_BreakWeightVolumeInfo);
		}

		#region Unique Charge Codes

		public void TestValidateChargeCodesUniqueOnRateLines()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var line1 = testEntry.RateLines[0];
			var line2 = testEntry.AddRateLine("FRT", FlatCalculator.Code);
			AssertNoErrors(line1.TL_ACInfo);
			AssertNoErrors(line2.TL_ACInfo);

			line2.TL_IsWhsJobLevelCharge = true;
			AssertHasErrors(line2.TL_ACInfo);

			line1.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertHasErrors(line1.TL_ACInfo);
			AssertHasErrors(line2.TL_ACInfo);

			line1.TL_IsWhsJobLevelCharge = true;
			AssertNoErrors(line1.TL_ACInfo);
			AssertHasErrors(line2.TL_ACInfo);

			line1.TL_RateCalculator = MinimumCalculator.Code;
			AssertNoErrors(line1.TL_ACInfo);
		}

		#endregion

		public void TestRateCalculatorValidation()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var quoteLine = testQuote.AddRateEntry("AIR").RateLines[0];

			var testGenericCosting = Helper.NewCosting(null);
			testGenericCosting.TH_OH = ZGuid.Empty;
			var genericCostingLine = testGenericCosting.AddRateEntry("AIR").RateLines[0];

			var testCosting = Helper.NewCosting(Helper.NewOrgHeader());
			var costingLine = testCosting.AddRateEntry("AIR").RateLines[0];

			var testGlobalRate = Helper.NewCompanyTariff();
			var globalRateLine = testGlobalRate.AddRateEntry("AIR").RateLines[0];
			testGlobalRate.Factory.Save();

			var testGlobalRate2 = Helper.NewCompanyTariff();
			var globalRateLine2 = testGlobalRate2.AddRateEntry("AIR").RateLines[0];
			testGlobalRate2.Factory.Save();

			var whsRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var whsLine = whsRate.AddRateEntry(RatingConstants.RateCategory.WHS, "ALL", "", "").AddRateLine("ODOC", CartageCalculator.Code, QuantityUnit.KG);

			var trwRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var trwLine = trwRate.AddRateEntry(RatingConstants.RateCategory.TRW, "ALL", "", "").AddRateLine("ODOC", CartageCalculator.Code, QuantityUnit.KG);

			var twuRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var twuLine = twuRate.AddRateEntry(RatingConstants.RateCategory.TWU, "ALL", "", "").AddRateLine("ODOC", CartageCalculator.Code, QuantityUnit.KG);

			quoteLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertNoErrors(quoteLine.TL_RateCalculatorInfo);
			quoteLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertNoErrors(quoteLine.TL_RateCalculatorInfo);

			costingLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertNoError(costingLine.TL_RateCalculatorInfo, ErrorMessages.CostBasedCalculatorNotAllowed);

			costingLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertHasError(costingLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorNotAllowed);

			genericCostingLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertHasError(genericCostingLine.TL_RateCalculatorInfo, ErrorMessages.CostBasedCalculatorNotAllowed);

			costingLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertHasWarning(costingLine.TL_RateCalculatorInfo, ErrorMessages.CostBasedCalculatorWarning);

			genericCostingLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertHasError(genericCostingLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorNotAllowed);

			globalRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertNoErrors(globalRateLine.TL_RateCalculatorInfo);
			globalRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertHasError(globalRateLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorNotAllowed);

			globalRateLine2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertNoErrors("No error message as comp tariff calc is allowed on level 2 tariff", globalRateLine2.TL_RateCalculatorInfo);

			var dSTEntry = testQuote.AddRateEntry("DST", Core.Constants.RateMode.SEA, "", "");
			var dSTLine = dSTEntry.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.KG);
			AssertHasError(dSTLine.TL_RateCalculatorInfo, ErrorMessages.CartageCalculatorNotAllowedOnSeaOrAll);

			dSTEntry.TI_Mode = Core.Constants.RateMode.ALL;
			dSTLine.TL_RateCalculator = "";
			dSTLine.TL_RateCalculator = CartageCalculator.Code;
			AssertHasError(dSTLine.TL_RateCalculatorInfo, ErrorMessages.CartageCalculatorNotAllowedOnSeaOrAll);

			dSTEntry.TI_Mode = Core.Constants.RateMode.ULD;
			dSTLine.TL_RateCalculator = "";
			dSTLine.TL_RateCalculator = CartageCalculator.Code;
			AssertNoErrors("Error Message regarding Cartage calc cleared", dSTLine.TL_RateCalculatorInfo);

			dSTEntry.TI_Mode = Core.Constants.RateMode.AIR;
			dSTLine.TL_RateCalculator = "";
			dSTLine.TL_RateCalculator = CartageCalculator.Code;
			AssertNoErrors("No error for Air, even though it is a general mode. ULD and LSE can have the same cartage", dSTLine.TL_RateCalculatorInfo);

			AssertNoErrors("Allow cartage for Warehouse", whsLine.TL_RateCalculatorInfo);
			AssertNoErrors("Allow cartage for Transit Warehouse", trwLine.TL_RateCalculatorInfo);
			AssertNoErrors("Allow cartage for Transit Warehouse Transportation Unit", twuLine.TL_RateCalculatorInfo);

			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = CartageCalculator.Code;
			dSTLine.TL_AC = dummyChargeCode.PK;
			dSTLine.TL_RateCalculator = UnitCalculator.Code;
			AssertHasWarning(dSTLine.TL_RateCalculatorInfo, ErrorMessages.NonCartageCalcOnCartageCharge);

			dSTLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertNoWarnings(dSTLine.TL_RateCalculatorInfo);

			dSTLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			AssertNoWarnings(dSTLine.TL_RateCalculatorInfo);

			dSTLine.TL_RateCalculator = CombinedCalculator.Code;
			AssertHasWarning(dSTLine.TL_RateCalculatorInfo, ErrorMessages.NonCartageCalcOnCartageCharge);

			dSTLine.TL_RateCalculator = CartageCalculator.Code;
			AssertNoWarnings(dSTLine.TL_RateCalculatorInfo);

			dSTLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			AssertNoWarnings(dSTLine.TL_RateCalculatorInfo);

			trwLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			AssertNoWarnings(trwLine.TL_RateCalculatorInfo);

			twuLine.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			AssertNoWarnings(twuLine.TL_RateCalculatorInfo);
		}

		public void TestValidateTariffBasedCalcDisallowedIfTariffLevelIsZero()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffRate = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "", "STD", "").AddRateLine("ODOC", FlatCalculator.Code);
			tariff.Factory.Save();

			var org1 = Helper.NewOrgHeader(0);
			var quoteLine = Helper.NewQuote(org1).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			quoteLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertHasWarning("When Org tariff level is set to 0 then default to Base Tariff", quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);

			org1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			quoteLine.Validation.ValidateTL_RateCalculator();
			AssertNoWarning("When Org tariff level is not set to 0 then use set level", quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);

			org1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			quoteLine.Validation.ValidateTL_RateCalculator();
			AssertNoWarning("When Org tariff level is not set to 0 then use set level", quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);

			quoteLine.Validation.ValidateTL_RateCalculator();
			AssertNoErrors(quoteLine.TL_RateCalculatorInfo);

			var org2 = Helper.NewOrgHeader(0);
			var clientRateLine = Helper.NewClientRate(org2).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			clientRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			AssertHasWarning(clientRateLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);

			org2.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			clientRateLine.Validation.ValidateTL_RateCalculator();
			AssertNoErrors(clientRateLine.TL_RateCalculatorInfo);

			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 0);
			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 0);
			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.ALL), "LSE", 0);

			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 1);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 1);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.ALL), "LSE", 1);

			quoteLine.Validation.ValidateTL_RateCalculator();
			clientRateLine.Validation.ValidateTL_RateCalculator();

			AssertHasWarning(quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);
			AssertNoErrors(clientRateLine.TL_RateCalculatorInfo);
			AssertNoWarning(clientRateLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);

			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LCL", 1);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.ALL), "LCL", 0);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LCL", 0);

			quoteLine.Validation.ValidateTL_RateCalculator();
			clientRateLine.Validation.ValidateTL_RateCalculator();

			AssertHasWarning(quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);
			AssertNoErrors(clientRateLine.TL_RateCalculatorInfo);

			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 1);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.ALL), "LSE", 0);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 0);

			quoteLine.Validation.ValidateTL_RateCalculator();
			clientRateLine.Validation.ValidateTL_RateCalculator();

			AssertNoErrors(quoteLine.TL_RateCalculatorInfo);
			AssertNoErrors(clientRateLine.TL_RateCalculatorInfo);

			org1.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 0);
			org2.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 0);
			clientRateLine.Parent.Parent.TH_OH = ZGuid.Empty;

			quoteLine.Validation.ValidateTL_RateCalculator();
			clientRateLine.Validation.ValidateTL_RateCalculator();

			AssertHasWarning(quoteLine.TL_RateCalculatorInfo, ErrorMessages.CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff);
			AssertNoErrors(clientRateLine.TL_RateCalculatorInfo);

			quoteLine.Parent.Parent.TH_OH = ZGuid.Empty;
			quoteLine.Validation.ValidateTL_RateCalculator();
			AssertNoErrors(quoteLine.TL_RateCalculatorInfo);
		}

		public void TestIDependentCalculatorValidation()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["BAF"].AC_Code, PercentageCalculator.Code);

			Assert("Should default to Percentage Calculator", rateLine.Uses(CalculatorType.Percentage));
			Assert("Should have an error", rateLine.HasRowErrors);

			rateLine.TL_RateCalculator = FlatCalculator.Code;
			Assert("Should have no errors", !rateLine.HasRowErrors);
		}

		public void TestFRTCalculatorOnFreightChargeValidation()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", "FRT");

			AssertHasError("Should have a validation error", rateLine.TL_RateCalculatorInfo, ErrorMessages.FRTCalculatorOnFreightCharge);
			rateLine.TL_RateCalculator = "FLT";

			AssertNoError("Should not have an error", rateLine.TL_RateCalculatorInfo, ErrorMessages.FRTCalculatorOnFreightCharge);
		}

		#endregion

		#region Agency Charge Code

		public void TestValidateAgencyChargeCodeNotAllowedOnFreightEntry()
		{
			var agencyChargeCode = Helper.ChargeCodes.New("ZZZZZZ", "Test Agency", AgencyCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var aIRLine = testQuote.AddRateEntry("AIR").RateLines.AddNew();
			var oRGLine = testQuote.AddRateEntry("ORG", "AIR", "", "").RateLines.AddNew();

			aIRLine.TL_AC = agencyChargeCode.PK;
			AssertEquals("Has Errors", true, aIRLine.TL_RateCalculatorInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.AgencyChargeOnFreightEntry, aIRLine.TL_RateCalculatorInfo.GetErrors().GetFirstMessage());

			oRGLine.TL_AC = agencyChargeCode.PK;
			AssertEquals("Has Errors", false, oRGLine.TL_RateCalculatorInfo.HasErrors());
		}

		#endregion

		#region Note Calculator

		public void TestValidateNoteCalculatorNotAllowedOnCosting()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var line = costing.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.NoteCalculatorOnCosting);

			line.TL_RateCalculator = NoteCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.NoteCalculatorOnCosting);
		}

		#endregion

		#region Profit Share / Rebate Calculator

		public void TestValidateProfitShareRebateCalculatorNotAllowedOnCosting()
		{
			var costing = Helper.NewCosting(null);
			var line = costing.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ProfitShareRebateCalculatorNotAllowedOnCosting);

			line.TL_RateCalculator = ProfitShareRebateCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.ProfitShareRebateCalculatorNotAllowedOnCosting);
		}

		#endregion

		#region Equalization Calculator

		public void TestEqualizationCalculatorValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var nonCostingEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var nonCostingLine = nonCostingEntry.RateLines.AddNew();
			nonCostingLine.TL_RateCalculator = EqualizationCalculator.Code;

			AssertHasError(nonCostingLine.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var testCost = Helper.NewCosting(Helper.NewOrgHeader());
			var airCostEntry = testCost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var airCostLine = airCostEntry.RateLines[0];
			airCostLine.TL_RateCalculator = EqualizationCalculator.Code;

			AssertNoErrors(airCostLine.TL_RateCalculatorInfo);

			var otherCostEntry = testCost.AddRateEntry("ORG", "", "AUSYD", "USLAX");
			var otherCostLine = otherCostEntry.RateLines.AddNew();
			otherCostLine.TL_RateCalculator = EqualizationCalculator.Code;

			AssertHasError(otherCostLine.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var airCostEntry2 = testCost.AddRateEntry("ORG", "LSE", "AUSYD", "USLAX");
			var airCostLine2 = airCostEntry2.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.KG);

			AssertNoError(airCostLine2.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var airCostEntry3 = testCost.AddRateEntry("DST", "LSE", "AUSYD", "USLAX");
			var airCostLine3 = airCostEntry3.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.KG);

			AssertNoError(airCostLine3.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var seaCostEntry = testCost.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			var seaCostLine = seaCostEntry.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG);

			AssertNoError(seaCostLine.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var bulkUpdaterCosting = Helper.NewCosting(Helper.NewOrgHeader());
			var bulkUpdaterCostingEntry = bulkUpdaterCosting.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var bulkUpdaterLine = bulkUpdaterCostingEntry.RateLines.AddNew();
			bulkUpdaterLine.IsBulkRateUpdateActionLine = true;
			bulkUpdaterLine.TL_RateCalculator = EqualizationCalculator.Code;

			AssertNoError(bulkUpdaterLine.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var companyTariffRateLine = companyTariffRateEntry.RateLines.AddNew();
			companyTariffRateLine.TL_RateCalculator = EqualizationCalculator.Code;

			AssertHasError(companyTariffRateLine.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var companyTariffRateLine2 = companyTariffRateEntry.RateLines.AddNew();
			companyTariffRateLine2.IsBulkRateUpdateActionLine = true;

			AssertNoError(companyTariffRateLine2.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var clientRate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientRateEntry1 = clientRate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var frtLine1 = clientRateEntry1.RateLines[0];
			frtLine1.TL_RateCalculator = EqualizationCalculator.Code;

			AssertHasError(frtLine1.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);

			var clientRate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientRateEntry2 = clientRate2.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var frtLine2 = clientRateEntry2.RateLines[0];
			frtLine2.IsBulkRateUpdateActionLine = true;
			frtLine2.TL_RateCalculator = EqualizationCalculator.Code;

			AssertHasError(frtLine2.TL_RateCalculatorInfo, ErrorMessages.EqualisationCalculatorNotAllowed);
		}

		#endregion

		#region Exclude from Company Tariffs Calculator

		public void TestValidateExcludeCompanyTariffsCalculatorAllowedOnClientRate()
		{
			//ClientRate
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code;
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			//Costing
			var costing = Helper.NewCosting(null);
			line = costing.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			//IntercompanyTariff
			var interCompanyTariff = Helper.NewIntercompanyTariff();
			line = interCompanyTariff.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			//CompanyTariff
			var companyTariff = Helper.NewCompanyTariff();
			line = companyTariff.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			//Quote
			var quote = Helper.NewQuote(null);
			line = quote.AddRateEntry("AIR").RateLines[0];
			AssertNoError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);

			line.TL_RateCalculator = ExcludeCompanyTariffsCalculator.Code;
			AssertHasError(line.TL_RateCalculatorInfo, ErrorMessages.ExcludeCompanyTariffsCalculatorNotAllowed);
		}

		#endregion

		#region Validate RateCalculator FCL Entry With Empty Container

		public void TestValidateRateCalculator_FCLEntryWithEmptyContainer_AddErrorIfCalculatorDoesntSupportEmptyContainer()
		{
			var expectedErrorMessage = "FCL Rates entry with blank Container/ULD can only use FLT or PER type of calculators.";
			var companyTariff = Helper.NewCompanyTariff();
			var containerPK = Helper.Containers["20GP"].PK;

			AssertCalculatorSupport(Category.FCL, hasContainer: true, calculator: UnitCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.FCL, hasContainer: false, calculator: UnitCalculator.Code, expectedError: expectedErrorMessage);
			AssertCalculatorSupport(Category.AIR, hasContainer: true, calculator: UnitCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.AIR, hasContainer: false, calculator: UnitCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.FCL, hasContainer: true, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.FCL, hasContainer: false, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.AIR, hasContainer: true, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.AIR, hasContainer: false, calculator: FlatCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.FCL, hasContainer: true, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.FCL, hasContainer: false, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.AIR, hasContainer: true, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.AIR, hasContainer: false, calculator: PercentageCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.FCL, hasContainer: false, calculator: UnitCalculator.Code, expectedError: null, isBulkRateUpdateActionLine: true);

			// Customs
			AssertCalculatorSupport(Category.CFC, hasContainer: true, calculator: UnitCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CFC, hasContainer: false, calculator: UnitCalculator.Code, expectedError: expectedErrorMessage);
			AssertCalculatorSupport(Category.CAI, hasContainer: true, calculator: UnitCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CAI, hasContainer: false, calculator: UnitCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.CFC, hasContainer: true, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CFC, hasContainer: false, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CAI, hasContainer: true, calculator: FlatCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CAI, hasContainer: false, calculator: FlatCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.CFC, hasContainer: true, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CFC, hasContainer: false, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CAI, hasContainer: true, calculator: PercentageCalculator.Code, expectedError: null);
			AssertCalculatorSupport(Category.CAI, hasContainer: false, calculator: PercentageCalculator.Code, expectedError: null);

			AssertCalculatorSupport(Category.CFC, hasContainer: false, calculator: UnitCalculator.Code, expectedError: null, isBulkRateUpdateActionLine: true);

			void AssertCalculatorSupport(string category, bool hasContainer, string calculator, string expectedError, bool isBulkRateUpdateActionLine = false)
			{
				var line = companyTariff.AddRateEntry(category).RateLines[0];
				line.Parent.TI_RC = hasContainer ? containerPK : ZGuid.Empty;
				line.TL_RateCalculator = calculator;
				line.IsBulkRateUpdateActionLine = isBulkRateUpdateActionLine;
				line.RunPreSaveValidation();

				if (string.IsNullOrEmpty(expectedError))
				{
					AssertNoErrors(line.TL_RateCalculatorInfo);
				}
				else
				{
					AssertHasError(line.TL_RateCalculatorInfo, expectedError);
				}
			}
		}

		#endregion

		#region Warehouse Location Type Calculator

		public void TestValidateWarehouseLocationTypeCalcOnlyAllowedOnWarehouseStorageCharges()
		{
			var warehouseStorageChargeCode = Helper.ChargeCodes.New("ZZZZZZ", "Test Warehouse", WarehouseLocationTypeCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage);
			var nonWarehouseStorageChargeCode = Helper.ChargeCodes.New("XXXXX", "Test Non Warehouse", WarehouseLocationTypeCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var line = testQuote.AddRateEntry("ORG", "AIR", "", "").RateLines.AddNew();

			line.TL_AC = nonWarehouseStorageChargeCode.PK;
			line.Validation.ValidateTL_RateCalculator();
			AssertHasErrors(line.TL_RateCalculatorInfo);
			line.TL_RateCalculator = UnitCalculator.Code;
			AssertNoErrors(line.TL_RateCalculatorInfo);
			line.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;
			AssertHasErrors(line.TL_RateCalculatorInfo);

			line.TL_AC = warehouseStorageChargeCode.PK;
			line.Validation.ValidateTL_RateCalculator();
			AssertNoErrors(line.TL_RateCalculatorInfo);
			line.TL_RateCalculator = UnitCalculator.Code;
			AssertNoErrors(line.TL_RateCalculatorInfo);
			line.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;
			AssertNoErrors(line.TL_RateCalculatorInfo);
		}

		#endregion

		#region Charge Code Group

		public void TestValidateChargeCodeGroup()
		{
			var expectedWarning = "Cannot add charge code as the group is not compatible with this rate line.";
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var frtLine = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			AssertNoErrors(frtLine.TL_ACInfo);
			frtLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			AssertHasError(frtLine.TL_ACInfo, expectedWarning);
			frtLine.TL_AC = Helper.ChargeCodes["DDOC"].PK;
			AssertHasError(frtLine.TL_ACInfo, expectedWarning);
			frtLine.TL_AC = Helper.ChargeCodes["BAF"].PK;
			AssertNoErrors(frtLine.TL_ACInfo);

			var orgLine = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			AssertNoErrors(orgLine.TL_ACInfo);
			orgLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertHasError(orgLine.TL_ACInfo, expectedWarning);
			orgLine.TL_AC = Helper.ChargeCodes["DDOC"].PK;
			AssertHasError(orgLine.TL_ACInfo, expectedWarning);
			orgLine.TL_AC = Helper.ChargeCodes["ONOTE"].PK;
			AssertNoErrors(orgLine.TL_ACInfo);
			orgLine.TL_AC = Helper.ChargeCodes["CCLR"].PK;
			AssertHasError(orgLine.TL_ACInfo, expectedWarning);
			orgLine.TL_AC = Helper.ChargeCodes.New("TORIGBR", "Test Origin Brokerage", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage).PK;
			AssertNoErrors(orgLine.TL_ACInfo);
			orgLine.TL_AC = Helper.ChargeCodes.New("TORBRON", "Test Origin Brokerage Only", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerageOnly).PK;
			AssertNoErrors(orgLine.TL_ACInfo);

			var dstLine = clientRate.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			AssertNoErrors(dstLine.TL_ACInfo);
			dstLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertHasError(dstLine.TL_ACInfo, expectedWarning);
			dstLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			AssertHasError(dstLine.TL_ACInfo, expectedWarning);
			dstLine.TL_AC = Helper.ChargeCodes["DNOTE"].PK;
			AssertNoErrors(dstLine.TL_ACInfo);
			dstLine.TL_AC = Helper.ChargeCodes["TORIGBR"].PK;
			AssertHasError(dstLine.TL_ACInfo, expectedWarning);
			dstLine.TL_AC = Helper.ChargeCodes["CCLR"].PK;
			AssertNoErrors(dstLine.TL_ACInfo);

			var newClientRate = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var newline = newClientRate.RateLines.AddNew();
			newline.IsBulkRateUpdateActionLine = true;
			newline.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertNoErrors(newline.TL_ACInfo);
			newline.TL_AC = Helper.ChargeCodes["DDOC"].PK;
			AssertNoErrors(newline.TL_ACInfo);
			newline.TL_AC = Helper.ChargeCodes["ONOTE"].PK;
			AssertNoErrors(newline.TL_ACInfo);
			newline.TL_AC = Helper.ChargeCodes["CFSSTOR"].PK;
			AssertHasError(newline.TL_ACInfo, expectedWarning);

			newClientRate = clientRate.AddRateEntry("CST", "SEA", "", "");
			newline = newClientRate.RateLines.AddNew();
			newline.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertHasError(newline.TL_ACInfo, expectedWarning);
			newline.TL_AC = Helper.ChargeCodes.New("TCONTST", "Test Container Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.ContainerStorage).PK;
			AssertNoErrors(newline.TL_ACInfo);
		}

		public void TestValidateChargeCodeGroup_USSpecificUnits()
		{
			var expectedWarning = "Cannot add charge code as the group must be Brokerage/Customs Duty based on the unit defined on this rate line.";
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var orgChargeCode = Helper.ChargeCodes.New("OTST", "Origin Charge", FirstPlusAdditionalCalculator.Code, "ORG");
			var dstChargeCode = Helper.ChargeCodes.New("DTST", "Destination Charge", FirstPlusAdditionalCalculator.Code, "DST");
			var brkChargeCode = Helper.ChargeCodes.New("DBRK", "Brokerage Charge", FirstPlusAdditionalCalculator.Code, "BRK");
			var bonChargeCode = Helper.ChargeCodes.New("DBON", "Brokerage Only Charge", FirstPlusAdditionalCalculator.Code, "BON");
			var cdsChargeCode = Helper.ChargeCodes.New("DCDS", "Custom Duty Charge", FirstPlusAdditionalCalculator.Code, "CDS");
			var oboChargeCode = Helper.ChargeCodes.New("OOBO", "Origin Brokerage Charge- No SHP", FirstPlusAdditionalCalculator.Code, "OBO");
			var obrChargeCode = Helper.ChargeCodes.New("OOBR", "Origin Brokerage Charge", FirstPlusAdditionalCalculator.Code, "OBR");

			var dstRateEntry = clientRate.AddRateEntry("DST", "ALL", "AUSYD", "USLAX");
			var dstRateLine = dstRateEntry.AddRateLine(dstChargeCode, FirstPlusAdditionalCalculator.Code, QuantityUnit.KG);

			var orgRateEntry = clientRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			var orgRateLine = orgRateEntry.AddRateLine(orgChargeCode, FirstPlusAdditionalCalculator.Code, QuantityUnit.KG);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				dstRateLine.TL_WeightVolume = QuantityUnit.CN;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.AF;
				AssertHasError(dstRateLine.TL_ACInfo, expectedWarning);

				dstRateLine.TL_AC = brkChargeCode.PK;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.N3;
				dstRateLine.TL_AC = dstChargeCode.PK;
				AssertHasError(dstRateLine.TL_ACInfo, expectedWarning);

				dstRateLine.TL_AC = bonChargeCode.PK;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.AM;
				dstRateLine.TL_AC = dstChargeCode.PK;
				AssertHasError(dstRateLine.TL_ACInfo, expectedWarning);

				dstRateLine.TL_AC = cdsChargeCode.PK;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.CN;
				dstRateLine.TL_AC = cdsChargeCode.PK;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.LY;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.CN;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.FW;
				AssertHasError(orgRateLine.TL_ACInfo, expectedWarning);

				orgRateLine.TL_AC = oboChargeCode.PK;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.TB;
				orgRateLine.TL_AC = orgChargeCode.PK;
				AssertHasError(orgRateLine.TL_ACInfo, expectedWarning);

				orgRateLine.TL_AC = obrChargeCode.PK;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.FC;
				orgRateLine.TL_AC = orgChargeCode.PK;
				AssertHasError(orgRateLine.TL_ACInfo, expectedWarning);

				orgRateLine.TL_AC = cdsChargeCode.PK;
				AssertNoErrors(orgRateLine.TL_ACInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				dstRateLine.TL_WeightVolume = QuantityUnit.CN;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.AF;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.N3;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.AM;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				dstRateLine.TL_WeightVolume = QuantityUnit.LY;
				AssertNoErrors(dstRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.CN;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.FW;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.TB;
				AssertNoErrors(orgRateLine.TL_ACInfo);

				orgRateLine.TL_WeightVolume = QuantityUnit.FC;
				AssertNoErrors(orgRateLine.TL_ACInfo);
			}
		}

		#endregion

		#region Product

		public void TestProductCommodity()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_RH_NKCommodityCode = "GEN";
			product.OP_PartNum = "123";
			
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_RH_NKCommodityCode = "HAZ";
			product2.OP_PartNum = "456";
			var orgHeader = Helper.NewOrgHeader();
			
			var testEntry = Helper.NewQuote(orgHeader).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			testEntry.TI_RH_NKCommodityCode = "GEN";
			var line1 = testEntry.RateLines[0];
			var productOrg = product.RelatedOrganisations.AddNew();
			productOrg.OU_OH = line1.Parent.Parent.TH_OH;

			var productOrg2 = product2.RelatedOrganisations.AddNew();
			productOrg2.OU_OH = line1.Parent.Parent.TH_OH;

			Factory.Save();

			line1.TL_OP_ProductNumber = product.PK;
			AssertNoErrors(line1.TL_OP_ProductNumberInfo);

			line1.TL_OP_ProductNumber = product2.PK;
			AssertHasErrors(line1.TL_OP_ProductNumberInfo);

			testEntry.TI_RH_NKCommodityCode = "HAZ";
			line1.Validation.ValidateTL_ParentID();
			AssertNoErrors(line1.TL_OP_ProductNumberInfo);

			testEntry.TI_RH_NKCommodityCode = "";
			line1.Validation.ValidateTL_ParentID();
			AssertNoErrors(line1.TL_OP_ProductNumberInfo);

			line1.TL_OP_ProductNumber = ZGuid.Empty;
			AssertNoErrors(line1.TL_OP_ProductNumberInfo);
		}

		public void TestProductLevelCharge()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line1 = testEntry.RateLines.AddNew();
			AssertNoErrors(line1.TL_IsWhsJobLevelChargeInfo);

			line1.TL_IsWhsJobLevelCharge = true;
			AssertNoErrors(line1.TL_IsWhsJobLevelChargeInfo);

			line1.TL_WeightVolume = "ZZZ";
			line1.TL_IsWhsJobLevelCharge = true;
			AssertNoErrors(line1.TL_IsWhsJobLevelChargeInfo);

			line1.TL_WeightVolume = RatingConstants.Units.LI;
			line1.TL_IsWhsJobLevelCharge = false;
			AssertHasErrors(line1.TL_IsWhsJobLevelChargeInfo);

			line1.TL_IsWhsJobLevelCharge = true;
			AssertNoErrors(line1.TL_IsWhsJobLevelChargeInfo);

			line1.TL_WeightVolume = "ZZZ";
			line1.TL_IsWhsJobLevelCharge = true;
			AssertNoErrors(line1.TL_IsWhsJobLevelChargeInfo);
		}

		#endregion

		#region Active Charge Codes

		public void TestValidateActiveChargeCodes()
		{
			var dummyChargeCode = Helper.ChargeCodes.New("DUM1", "Dummy", FlatCalculator.Code, "FRT", "");
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line1 = testEntry.RateLines.AddNew();

			dummyChargeCode.AC_IsActive = false;
			line1.TL_AC = dummyChargeCode.PK;
			AssertHasError(line1.TL_ACInfo, "This charge code is marked as inactive and cannot be included.");

			dummyChargeCode.AC_IsActive = true;
			line1.Validation.ValidateTL_AC();
			AssertNoErrors(line1.TL_ACInfo);

			dummyChargeCode.AC_IsActive = false;
			line1.Validation.ValidateTL_AC();
			AssertHasError(line1.TL_ACInfo, "This charge code is marked as inactive and cannot be included.");

			Factory.Save();
			testEntry.TI_RateStartDate = ZDate.Today.AddDays(-10);
			testEntry.TI_RateEndDate = ZDate.Today.AddDays(-5);

			line1.Validation.ValidateTL_AC();
			AssertNoErrors("Charge code is not in list but entry is invalid - this line should not be validated.", line1.TL_ACInfo);
		}

		public void TestValidateChargeCodeListValidation()
		{
			var dummyChargeCode = Helper.ChargeCodes.New("DUM1", "Dummy", FlatCalculator.Code, "FRT", "");
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line1 = testEntry.RateLines.AddNew();

			line1.TL_AC = dummyChargeCode.PK;
			AssertNoErrors("Charge code is in list", line1.TL_ACInfo);

			line1.Lookups.ChargeCodes.Load();
			line1.Lookups.ChargeCodes.RemoveAndDelete(dummyChargeCode);
			line1.Validation.ValidateTL_AC();
			AssertHasErrors("Charge code is not in list", line1.TL_ACInfo);

			dummyChargeCode = Helper.ChargeCodes.New("DUM1", "Dummy", FlatCalculator.Code, "FRT", "");
			line1.Lookups.ChargeCodes.Load();
			line1.TL_AC = dummyChargeCode.PK;

			Factory.Save();
			testEntry.TI_RateStartDate = ZDate.Today.AddDays(-10);
			testEntry.TI_RateEndDate = ZDate.Today.AddDays(-5);
			line1.Validation.ValidateTL_AC();
			AssertNoErrors("Charge code is not in list but entry is invalid - this line should not be validated.", line1.TL_ACInfo);
		}

		#endregion

		#region Cartage Charge Code

		public void TestCartageChargeCodeAllowedMoreThanOnce()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "");
			var cartageLine1 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			AssertNoErrors(cartageLine1.TL_ACInfo);

			var cartageLine2 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			AssertNoErrors(cartageLine1.TL_ACInfo);
			AssertNoErrors(cartageLine2.TL_ACInfo);

			var cartageLine3 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			AssertNoErrors(cartageLine1.TL_ACInfo);
			AssertNoErrors(cartageLine2.TL_ACInfo);
			AssertNoErrors(cartageLine3.TL_ACInfo);
		}

		#endregion

		#region Currency

		public void TestValidateCurrency()
		{
			TestLine.TL_RX_NKCurrency = "";
			AssertEquals("Has Errors", true, TestLine.TL_RX_NKCurrencyInfo.HasErrors());

			TestLine.TL_RX_NKCurrency = "USD";
			AssertEquals("Has Errors", false, TestLine.TL_RX_NKCurrencyInfo.HasErrors());

			TestLine.TL_RX_NKCurrency = "US";
			AssertEquals("Has Errors", true, TestLine.TL_RX_NKCurrencyInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid " + TestLine.TL_RX_NKCurrencyInfo.Description + ".", TestLine.TL_RX_NKCurrencyInfo.GetErrors().GetFirstMessage());

			TestLine.TL_RX_NKCurrency = "AUD";
			AssertEquals("Has Errors", false, TestLine.TL_RX_NKCurrencyInfo.HasErrors());

			TestLine.TL_RX_NKCurrency = null;
			AssertEquals("Has Errors", true, TestLine.TL_RX_NKCurrencyInfo.HasErrors());
		}

		public void TestValidateEmptyCurrencyWithFreightInclusiveCalculator()
		{
			TestLine.TL_RateCalculator = FreightInclusiveCalculator.Code;
			TestLine.TL_RX_NKCurrency = "";
			AssertEquals("A RateLine with FRT calculator should be able to have an empty currency", ZString.Empty, TestLine.TL_RX_NKCurrency);
			AssertNoWarnings("A RateLine with FRT calculator and empty currency should not have any warnings", TestLine.TL_RX_NKCurrencyInfo);
			AssertNoErrors("A RateLine with FRT calculator and empty currency should not have any errors", TestLine.TL_RX_NKCurrencyInfo);

			TestLine.TL_RX_NKCurrency = null;
			AssertEquals("A RateLine with FRT calculator should be able to have an empty currency", ZString.Empty, TestLine.TL_RX_NKCurrency);
			AssertNoWarnings("A RateLine with FRT calculator and empty currency should not have any warnings", TestLine.TL_RX_NKCurrencyInfo);
			AssertNoErrors("A RateLine with FRT calculator and empty currency should not have any errors", TestLine.TL_RX_NKCurrencyInfo);
		}

		public void TestValidateSameChargeCodeWithSameCurrencies()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "");
			var rateLine1 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			rateLine1.TL_RateStartDate = new ZDate(2020, 1, 1);
			rateLine1.TL_RateEndDate = new ZDate(2020, 1, 31);

			var rateLine2 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine2.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			rateLine2.TL_RateStartDate = new ZDate(2020, 1, 15);
			rateLine2.TL_RateEndDate = new ZDate(2020, 3, 31);

			AssertNoErrors(rateLine2.TL_RX_NKCurrencyInfo);
		}

		public void TestValidateSameChargeCodeWithDifferentCurrencies_OverlappingDates()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "");
			var rateLine1 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			rateLine1.TL_RateStartDate = new ZDate(2020, 1, 1);
			rateLine1.TL_RateEndDate = new ZDate(2020, 1, 31);

			var rateLine2 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine2.TL_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			rateLine2.TL_RateStartDate = new ZDate(2020, 1, 15);
			rateLine2.TL_RateEndDate = new ZDate(2020, 3, 31);

			AssertHasError(rateLine2.TL_RX_NKCurrencyInfo, "Rate lines with same charge code and different currencies should not have overlapping dates.");

			rateLine2.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			AssertNoErrors(rateLine2.TL_RX_NKCurrencyInfo);
		}

		public void TestValidateSameChargeCodeWithDifferentCurrencies_NonOverlappingDates()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "");
			var rateLine1 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			rateLine1.TL_RateStartDate = new ZDate(2020, 1, 1);
			rateLine1.TL_RateEndDate = new ZDate(2020, 1, 31);

			var rateLine2 = testEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine2.TL_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			rateLine2.TL_RateStartDate = new ZDate(2020, 2, 1);
			rateLine2.TL_RateEndDate = new ZDate(2020, 3, 31);

			AssertNoErrors(rateLine2.TL_RX_NKCurrencyInfo);
		}

		public void TestValidateSameChargeCodeCurrencyValidation_ChangingPropertiesRecalculates()
		{
			var expectedError = "Rate lines with same charge code and different currencies should not have overlapping dates.";

			var testEntry = Helper.NewCompanyTariff().AddRateEntry("ORG", "AIR", "AUSYD", "");
			testEntry.TI_RX_NKCurrency = Constants.CurrencyCodes.Australia;

			var rateLine1 = CreateRateLine("FRT", Constants.CurrencyCodes.Australia);
			rateLine1.TL_RateStartDate = new ZDate(2020, 1, 1);
			rateLine1.TL_RateEndDate = new ZDate(2020, 1, 31);

			var rateLine2 = CreateRateLine("FRT", Constants.CurrencyCodes.India);
			rateLine2.TL_RateStartDate = new ZDate(2020, 1, 15);
			rateLine2.TL_RateEndDate = new ZDate(2020, 3, 31);

			var rateLine3 = CreateRateLine("WAR", Constants.CurrencyCodes.Australia);
			rateLine3.TL_RateStartDate = new ZDate(2020, 1, 1);
			rateLine3.TL_RateEndDate = new ZDate(2020, 1, 31);

			var rateLine4 = CreateRateLine("WAR", Constants.CurrencyCodes.Australia);
			rateLine4.TL_RateStartDate = new ZDate(2020, 1, 15);
			rateLine4.TL_RateEndDate = new ZDate(2020, 3, 31);

			testEntry.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasError("Line1 with FRT(AUD): should have error as there are two currencies(INR and AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo, expectedError);
				AssertHasError("Line2 with FRT(INR): should have error as there are two currencies(INR and AUD) on FRT.", rateLine2.TL_RX_NKCurrencyInfo, expectedError);
				AssertNoErrors("Line3 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line4 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine4.TL_RX_NKCurrencyInfo);
			});

			// Date change
			rateLine2.TL_RateStartDate = new ZDate(2020, 2, 15);
			testEntry.RunPreSaveValidation();
			CombineAssertions("Date change", () =>
			{
				AssertNoErrors("Line1 with FRT(AUD): should not have error as dates are not overlapping.", rateLine1.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line2 with FRT(INR): should not have error as dates are not overlapping..", rateLine2.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line3 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line4 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine4.TL_RX_NKCurrencyInfo);
			});

			// Date change setting start date to empty
			rateLine2.TL_RateStartDate = ZDate.Empty;
			testEntry.RunPreSaveValidation();
			CombineAssertions("Empty start date is same as Minimum", () =>
			{
				AssertHasError("Line1 with FRT(AUD): should have error as there are two currencies(INR and AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo, expectedError);
				AssertHasError("Line2 with FRT(INR): should have error as there are two currencies(INR and AUD) on FRT.", rateLine2.TL_RX_NKCurrencyInfo, expectedError);
				AssertNoErrors("Line3 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line4 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine4.TL_RX_NKCurrencyInfo);
			});

			// Date change setting end date to empty
			rateLine1.TL_RateEndDate = ZDate.Empty;
			rateLine2.TL_RateStartDate = new ZDate(2020, 2, 15);
			testEntry.RunPreSaveValidation();
			CombineAssertions("Empty end date is same as Maximum", () =>
			{
				AssertHasError("Line1 with FRT(AUD): should have error as there are two currencies(INR and AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo, expectedError);
				AssertHasError("Line2 with FRT(INR): should have error as there are two currencies(INR and AUD) on FRT.", rateLine2.TL_RX_NKCurrencyInfo, expectedError);
				AssertNoErrors("Line3 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo);
				AssertNoErrors("Line4 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine4.TL_RX_NKCurrencyInfo);
			});

			//RateLine_CountChanged
			testEntry.RateLines.RemoveAndDelete(rateLine2);
			testEntry.RunPreSaveValidation();
			AssertNoErrors("Line1 with FRT(AUD): should return false now as there is single currency(AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo);

			rateLine4.TL_RX_NKCurrency = Constants.CurrencyCodes.India;
			testEntry.RunPreSaveValidation();
			CombineAssertions("Currency Change should refresh group validation cache.", () =>
			{
				AssertNoErrors("Line1 with FRT(AUD): should not have error as there is single currency(AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo);
				AssertHasError("Line3 with WAR(AUD): should have error as there are two currencies(INR and AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo, expectedError);
				AssertHasError("Line4 with WAR(INR): should have error as there are two currencies(INR and AUD) on WAR.", rateLine4.TL_RX_NKCurrencyInfo, expectedError);
			});

			var chargeCodeWithFLT = Helper.ChargeCodes["FRT"];
			chargeCodeWithFLT.AC_RateCalculator = FlatCalculator.Code;

			rateLine4.TL_AC = chargeCodeWithFLT.PK;
			AssertEquals("Precondition:Setting TL_AC calls SetRateCalculator which in some cases, set TL_RX_NKCurrency to rateEntry.TI_RX_NKCurrency which is AUD. " +
				"But to test whether TL_AC is calling InvalidateGroupValidation, we need to ensure that currency is not changed after setting TL_AC."
				, Constants.CurrencyCodes.India, rateLine4.TL_RX_NKCurrency);

			testEntry.RunPreSaveValidation();
			CombineAssertions("ChargeCode(TL_AC) change should refresh group validation cache.", () =>
			{
				AssertHasError("Line1 with FRT(AUD): should have error as there are two currencies(INR and AUD) on FRT.", rateLine1.TL_RX_NKCurrencyInfo, expectedError);
				AssertNoErrors("Line3 with WAR(AUD): should not have error as there is single currency(AUD) on WAR.", rateLine3.TL_RX_NKCurrencyInfo);
				AssertHasError("Line4 with FRT(INR): should have error as there are two currencies(INR and AUD) on FRT.", rateLine4.TL_RX_NKCurrencyInfo, expectedError);
			});

			RateLine CreateRateLine(ZString chargeCode, ZString currency)
			{
				var rateLine = testEntry.AddRateLine(chargeCode, FlatCalculator.Code);
				rateLine.TL_RX_NKCurrency = currency;
				return rateLine;
			}
		}

		#endregion

		#region TestValidateTL_WeightVolume

		public void TestValidateTL_WeightVolume()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			TestLine.TL_AC = dummyChargeCode.PK;

			TestLine.TL_RateCalculator = UnitCalculator.Code;
			TestLine.TL_WeightVolume = "";
			AssertHasErrors(TestLine.TL_WeightVolumeInfo);

			TestLine.TL_WeightVolume = "KG";
			AssertNoErrors(TestLine.TL_WeightVolumeInfo);

			TestLine.TL_WeightVolume = "oo";
			AssertHasError(TestLine.TL_WeightVolumeInfo, "Enter a valid " + TestLine.TL_WeightVolumeInfo.Description + ".");

			TestLine.TL_WeightVolume = "M3";
			AssertNoErrors(TestLine.TL_WeightVolumeInfo);

			TestLine.TL_WeightVolume = RatingConstants.Units.LM;
			AssertNoErrors(TestLine.TL_WeightVolumeInfo);
		}

		public void TestValidateTL_WeightVolume_PkgUnit()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("WHS", "ALL", "", "").AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);

			line.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoErrors(line.TL_WeightVolumeInfo);
			line.TL_WeightVolume = Constants.PkgUnit.Pallet;
			AssertNoErrors(line.TL_WeightVolumeInfo);
			line.TL_WeightVolume = Constants.PkgUnit.Bottle;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			var product = Helper.NewOrgSupplierPart(line.Parent.Parent.Header);
			line.TL_OP_ProductNumber = product.PK;

			line.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoErrors(line.TL_WeightVolumeInfo);
			line.TL_WeightVolume = Constants.PkgUnit.Bottle;
			AssertHasError(line.TL_WeightVolumeInfo, "Product ###1 has no BOT definition.");

			Helper.AddPartUnit(product, Constants.PkgUnit.Unit, Constants.PkgUnit.Bottle, 1m);
			line.Validation.ValidateTL_WeightVolume();
			AssertNoErrors(line.TL_WeightVolumeInfo);

			product = Helper.NewOrgSupplierPart(line.Parent.Parent.Header, Constants.PkgUnit.Bottle);
			line.TL_OP_ProductNumber = product.PK;

			line.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertHasError(line.TL_WeightVolumeInfo, "Product ###2 has no UNT definition.");
			line.TL_WeightVolume = Constants.PkgUnit.Bottle;
			AssertNoErrors(line.TL_WeightVolumeInfo);
			line.TL_WeightVolume = Constants.PkgUnit.Carton;
			AssertHasError(line.TL_WeightVolumeInfo, "Product ###2 has no CTN definition.");

			Helper.AddPartUnit(product, Constants.PkgUnit.Bottle, Constants.PkgUnit.Carton, 10m);
			line.Validation.ValidateTL_WeightVolume();
			AssertNoErrors(line.TL_WeightVolumeInfo);
		}

		public void TestValidateTL_WeightVolume_Unit()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("WHS", "ALL", "", "").AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line.TL_IsWhsJobLevelCharge = true;

			line.TL_WeightVolume = Constants.PkgUnit.Package;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Pallet;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = QuantityUnit.LI;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = QuantityUnit.KG;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = QuantityUnit.M3;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Bag;
			AssertHasError(line.TL_WeightVolumeInfo, "You can only specify a unit of Weight, Volume, Unit (UNT), Package (PKG), Pallet (PLT) or Line (LI) when the Job Level indicator is enabled.");

			line.TL_WeightVolume = Constants.PkgUnit.BaleCompressed;
			AssertHasError(line.TL_WeightVolumeInfo, "You can only specify a unit of Weight, Volume, Unit (UNT), Package (PKG), Pallet (PLT) or Line (LI) when the Job Level indicator is enabled.");

			line.TL_WeightVolume = Constants.PkgUnit.BaleUncompressed;
			AssertHasError(line.TL_WeightVolumeInfo, "You can only specify a unit of Weight, Volume, Unit (UNT), Package (PKG), Pallet (PLT) or Line (LI) when the Job Level indicator is enabled.");

			line.TL_IsWhsJobLevelCharge = false;
			line.TL_WeightVolume = Constants.PkgUnit.Package;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Pallet;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Case;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Coil;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_WeightVolume = Constants.PkgUnit.Box;
			AssertNoErrors(line.TL_WeightVolumeInfo);

			line.TL_IsWhsJobLevelCharge = true;
			AssertHasError(line.TL_WeightVolumeInfo, "You can only specify a unit of Weight, Volume, Unit (UNT), Package (PKG), Pallet (PLT) or Line (LI) when the Job Level indicator is enabled.");

			line.TL_IsWhsJobLevelCharge = false;
			AssertNoErrors(line.TL_WeightVolumeInfo);
		}

		public void TestValidateTL_WeightVolume_RateCalculator()
		{
			TestLine.TL_AC = Env.Registry.FreightChargeCode;

			TestLine.TL_RateCalculator = FlatCalculator.Code;
			AssertNoErrors(TestLine.TL_WeightVolumeInfo);

			TestLine.TL_RateCalculator = UnitCalculator.Code;
			AssertHasErrors(TestLine.TL_WeightVolumeInfo);
		}

		public void TestValidateTL_WeightVolume_WarehousePackTypeCalculator()
		{
			var expectedErrorMessage = "You can only use Unit (UNT) or Package (PK) when Warehouse Pack Type Calculator is used.";
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.RateLines.AddNew();
			AssertNoError("Precondition", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);

			rateLine.TL_RateCalculator = WarehousePackCalculator.Code;
			AssertEquals("Precondition", Constants.PkgUnit.Unit, rateLine.TL_WeightVolume);
			AssertNoError("Precondition", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);

			rateLine.TL_WeightVolume = Constants.PkgUnit.Bottle;
			AssertHasError("Only UNT and PK weight / volume is allowed.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);

			rateLine.TL_WeightVolume = QuantityUnit.PK;
			AssertNoError("Only UNT and PK weight / volume is allowed.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);

			rateLine.TL_WeightVolume = Constants.Weight.Kilograms;
			AssertHasError("Only UNT and PK weight / volume is allowed.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			AssertNoError("Any weight / volume is allowed for Unit Calculator.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
		}

		public void TestValidateTL_WeightVolume_UnitFactor_LPO()
		{
			var expectedErrorMessage = "You can only use Package (PK) when Loaded Packages Only (LPO) Unit Factor is used.";
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			var rateLine = rateEntry.RateLines.AddNew();
			AssertNoError("Precondition", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_RateCalculator = WarehousePackCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.PK;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;
			AssertNoError("Precondition", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertHasError("Only PK weight / volume is allowed.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_UnitFactor = UnitFactorList.Codes.BCN;
			AssertNoError("Should clear error on TL_WeightVolumeInfo.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_WeightVolume = QuantityUnit.PK;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.LoadedPackagesOnly;
			AssertNoError("Only PK weight / volume is allowed.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_UnitFactor = UnitFactorList.Codes.BCN;
			rateLine.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoError("Unit weight / volume is allowed when Unit Factor is BCN.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);

			rateLine.TL_UnitFactor = UnitFactorList.Codes.SCN;
			rateLine.TL_WeightVolume = Constants.PkgUnit.Unit;
			AssertNoError("Unit weight / volume is allowed when Unit Factor is SCN.", rateLine.TL_WeightVolumeInfo, expectedErrorMessage);
			AssertNoError("Precondition", rateLine.TL_UnitFactorInfo, expectedErrorMessage);
		}

		#endregion

		#region PercentageCalculator

		public void TestValidatePercentageCalculator()
		{
			TestLine.TL_RateCalculator = PercentageCalculator.Code;
			TestLine.RunPreSaveValidation();
			Assert(TestLine.HasRowErrors);
			AssertEquals(ErrorMessages.AtLeastOneChargeType, TestLine.RowErrors.GetFirstMessage());

			var item1 = TestLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item1.TM_AC = Helper.ChargeCodes["CART"].PK;
			var item2 = TestLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item2.TM_AC = Helper.ChargeCodes["CART"].PK;

			TestLine.RunPreSaveValidation();
			Assert(!TestLine.HasRowErrors);
			Assert(item1.HasRowErrors);
			AssertEquals(ErrorMessages.DoubledChargeCodes, item1.RowErrors.GetFirstMessage());
			Assert(item2.HasRowErrors);
			AssertEquals(ErrorMessages.DoubledChargeCodes, item2.RowErrors.GetFirstMessage());

			item2.TM_AC = Helper.ChargeCodes["FRT"].PK;
			TestLine.RunPreSaveValidation();
			Assert(!TestLine.HasRowErrors);
			Assert(!item1.HasRowErrors);
			Assert(!item2.HasRowErrors);

			item1.TM_Text = CalculatorConstants.Text.AllCharges;
			item2.TM_Text = CalculatorConstants.Text.AllCharges;
			TestLine.RunPreSaveValidation();
			Assert(!TestLine.HasRowErrors);
			Assert(item1.HasRowErrors);
			AssertEquals(ErrorMessages.DoubledChargeTypes, item1.RowErrors.GetFirstMessage());
			Assert(item2.HasRowErrors);
			AssertEquals(ErrorMessages.DoubledChargeTypes, item2.RowErrors.GetFirstMessage());

			item2.TM_Text = CalculatorConstants.Text.ChargeCode;
			TestLine.RunPreSaveValidation();
			Assert(!TestLine.HasRowErrors);
			Assert(!item1.HasRowErrors);
			Assert(!item2.HasRowErrors);
		}

		#endregion

		#region UnitMultipleAsString

		public void TestValidateUnitMultipleAsString()
		{
			Assert(!TestLine.UnitMultipleAsStringInfo.HasErrors());
			AssertEquals(0m, TestLine.TL_WeightVolumeMultiple);
			AssertEquals("", TestLine.UnitMultipleAsString);

			TestLine.UnitMultipleAsString = "123";
			Assert(!TestLine.UnitMultipleAsStringInfo.HasErrors());
			AssertEquals(123m, TestLine.TL_WeightVolumeMultiple);

			TestLine.UnitMultipleAsString = "ABC";
			Assert(TestLine.UnitMultipleAsStringInfo.HasErrors());
			AssertEquals(0m, TestLine.TL_WeightVolumeMultiple);
			AssertEquals("ABC", TestLine.UnitMultipleAsString);
		}

		#endregion

		#region Actual Percentage

		public void TestValidateActualPercentage()
		{
			TestLine.TL_ActualPercentage = 0;
			AssertNoErrors(TestLine.TL_ActualPercentageInfo);

			TestLine.TL_ActualPercentage = 50;
			AssertNoErrors(TestLine.TL_ActualPercentageInfo);

			TestLine.TL_ActualPercentage = 200;
			AssertHasError(TestLine.TL_ActualPercentageInfo, "Please enter a valid percent.");

			TestLine.TL_ActualPercentage = 100;
			AssertNoErrors(TestLine.TL_ActualPercentageInfo);

			TestLine.TL_ActualPercentage = 120;
			AssertHasError(TestLine.TL_ActualPercentageInfo, "Please enter a valid percent.");
		}

		#endregion

		#region TestUseOnlyActualWeightMeasure

		public void TestUseOnlyActualWeightMeasure()
		{
			const string expectedWarningMessageForStorage = "If storage rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the system registry at Warehouse -> Chargeable -> Chargeable Factor for Warehouse Storage. The greater of actual or calculated units will then be used as the basis for charging.";

			const string expectedWarningMessageForHandling = "If storage rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the system registry at Warehouse -> Chargeable -> Chargeable Factor for Warehouse Handling. The greater of actual or calculated units will then be used as the basis for charging.";

			var warehouseStorageChargeCode = Helper.ChargeCodes.New("WWHSSTO", "Warehouse Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSStorage, "");
			var warehouseReceiveHandlingChargeCode = Helper.ChargeCodes.New("WRECHAN", "Warehouse Receive Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseReceiveStorageChargeCode = Helper.ChargeCodes.New("WRECSTO", "Warehouse Receive Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var warehouseOrderHandlingChargeCode = Helper.ChargeCodes.New("WORDHAN", "Warehouse Order Handling", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, "");
			var warehouseOrderStorageChargeCode = Helper.ChargeCodes.New("WORDSTO", "Warehouse Order Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.UseOnlyActualWeightMeasure = false;
			AssertNoWarning(rateLine.UseOnlyActualWeightMeasureInfo, expectedWarningMessageForStorage);

			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, warehouseStorageChargeCode, expectedWarningMessageForStorage);
			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, warehouseReceiveHandlingChargeCode, expectedWarningMessageForHandling);
			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, warehouseReceiveStorageChargeCode, expectedWarningMessageForHandling);
			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, warehouseOrderHandlingChargeCode, expectedWarningMessageForHandling);
			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, warehouseOrderStorageChargeCode, expectedWarningMessageForHandling);
		}

		public void TestUseOnlyActualWeightMeasure_TransitTransportationUnit()
		{
			const string expectedWarningMessageForTransitTransporationUnit = @"If rating units are volume or weight, unchecking the 'Actual' flag will cause the chargeable volumetric weight calculation to be applied using parameters defined in the following system registries. The greater of actual or calculated units will then be used as the basis for charging.
Warehouse -> Transit Warehouse -> Chargeable Transportation Unit -> Chargeable Factor for Air
Warehouse -> Transit Warehouse -> Chargeable Transportation Unit -> Chargeable Factor for Sea
Warehouse -> Transit Warehouse -> Chargeable Transportation Unit -> Chargeable Factor for Road";

			var transitReceiveTransportationUnitChargeCode = Helper.ChargeCodes.New("TWRTUCHAN", "Transit RTU", UnitCalculator.Code, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, "");
			var transitDispatchTransportationUnitChargeCode = Helper.ChargeCodes.New("TWDTUCHAN", "Transit DTU", UnitCalculator.Code, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, "");

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.UseOnlyActualWeightMeasure = false;
			AssertNoWarning(rateLine.UseOnlyActualWeightMeasureInfo, expectedWarningMessageForTransitTransporationUnit);

			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, transitReceiveTransportationUnitChargeCode, expectedWarningMessageForTransitTransporationUnit);
			AssertWarningMessageOnUseOnlyActualWeightMeasure(rateLine, transitDispatchTransportationUnitChargeCode, expectedWarningMessageForTransitTransporationUnit);
		}

		static void AssertWarningMessageOnUseOnlyActualWeightMeasure(RateLine rateLine, AccChargeCode chargeCode, string expectedWarningMessage)
		{
			rateLine.TL_AC = chargeCode.PK;
			rateLine.UseOnlyActualWeightMeasure = false;
			AssertHasWarning(rateLine.UseOnlyActualWeightMeasureInfo, expectedWarningMessage);
			rateLine.UseOnlyActualWeightMeasure = true;
			AssertNoWarning(rateLine.UseOnlyActualWeightMeasureInfo, expectedWarningMessage);
		}

		#endregion

		public void TestValidateSequenceItem()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AUSYD", "");
			var rateLine = entry.AddRateLine("DDOC", PercentageCalculator.Code);
			rateLine.RateLineItems.RemoveAll();

			var allItem = rateLine.RateLineItems.AddNew();
			allItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			allItem.TM_Text = CalculatorConstants.Text.AllCharges;

			var sequenceItem = rateLine.RateLineItems.AddNew();
			sequenceItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			sequenceItem.TM_Text = Calculator.Items.Value.CalculationOrder;
			sequenceItem.TM_Value = 80;
			rateLine.Validation.ValidateTL_RateCalculator();

			AssertNoRowError(sequenceItem, ErrorMessages.DoubledChargeTypes);

			var sequenceItem2 = rateLine.RateLineItems.AddNew();
			sequenceItem2.TM_Type = CalculatorConstants.Type.ApplyTo;
			sequenceItem2.TM_Text = Calculator.Items.Value.CalculationOrder;
			sequenceItem2.TM_Value = 10;
			rateLine.Validation.ValidateTL_RateCalculator();

			AssertHasRowError(sequenceItem, ErrorMessages.DoubledChargeTypes);
			AssertHasRowError(sequenceItem2, ErrorMessages.DoubledChargeTypes);

			sequenceItem2.Delete();
			rateLine.Validation.ValidateTL_RateCalculator();

			AssertNoRowError(sequenceItem, ErrorMessages.DoubledChargeTypes);

			var rateLine2 = entry.AddRateLine("DDOC", PercentageCalculator.Code);
			rateLine2.RateLineItems.RemoveAll();

			var sequenceItem3 = rateLine2.RateLineItems.AddNew();
			sequenceItem3.TM_Type = CalculatorConstants.Type.ApplyTo;
			sequenceItem3.TM_Text = Calculator.Items.Value.CalculationOrder;
			sequenceItem3.TM_Value = 60;
			rateLine.Validation.ValidateTL_RateCalculator();
			rateLine2.Validation.ValidateTL_RateCalculator();

			AssertNoRowError(sequenceItem, ErrorMessages.DoubledChargeTypes);
			AssertNoRowError(sequenceItem3, ErrorMessages.DoubledChargeTypes);
		}

		public void TestValidateRateLineWithoutCalculatorDoesNotThowAnyException()
		{
			AssertNoExceptionThrown("Setting rate calculator to none throws exception", () =>
			{
				TestLine.TL_RateCalculator = TimeCalculator.Code;
				TestLine.TL_RateCalculator = string.Empty;
			});
		}

		public void TestValidateIsJobLevelAvailableIsConsistent()
		{
			var expectedError = "Charges using the same charge code must be either all Product level, or all Job level - but not a mixture.";

			var chargeCode = Helper.ChargeCodes["ODOC"];
			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AU", "");

			var rateLine1 = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine1.TL_IsWhsJobLevelCharge = false;

			var rateLine2 = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine2.TL_IsWhsJobLevelCharge = true;
			rateLine2.Validation.ValidateTL_AC();

			var message = "For the same charge code on the same entry, we should not allow a mix of 'Is Job Level Charge' being ticked";
			AssertHasError(message, rateLine2.TL_ACInfo, expectedError);

			rateLine1.TL_RateCalculator = MinimumCalculator.Code;
			rateLine2.Validation.ValidateTL_AC();
			message = "But we shouldn't check for inconsistency against the minimum calculator because the usual rules don't apply to it";
			AssertNoError(message, rateLine2.TL_ACInfo, expectedError);
		}

		public void TestValidateIsJobLevelAvailableIsConsistent_ChangingPropertiesRecalculates()
		{
			var expectedError = "Charges using the same charge code must be either all Product level, or all Job level - but not a mixture.";

			var testEntry = Helper.NewCosting(null).AddRateEntry("ORG", "AIR", "AUSYD", "");

			var rateLineFRT1 = CreateRateLine("FRT", FlatCalculator.Code, false);
			var rateLineFRT2 = CreateRateLine("FRT", FlatCalculator.Code, true);

			var rateLineWAR1 = CreateRateLine("WAR", FlatCalculator.Code, true);
			var rateLineWAR2 = CreateRateLine("WAR", FlatCalculator.Code, true);

			testEntry.RunPreSaveValidation();
			CombineAssertions("There is inconsistency as ratelines have different TL_IsWhsJobLevelCharge values for same charge code FRT.", () =>
			{
				AssertHasError(rateLineFRT1.TL_ACInfo, expectedError);
				AssertHasError(rateLineFRT2.TL_ACInfo, expectedError);
			});
			CombineAssertions("No inconsistency as both Rate Lines added for WAR have same value(True) for TL_IsWhsJobLevelCharge.", () =>
			{
				AssertNoError(rateLineWAR1.TL_ACInfo, expectedError);
				AssertNoError(rateLineWAR2.TL_ACInfo, expectedError);
			});

			//RateLine_CountChanged
			testEntry.RateLines.RemoveAndDelete(rateLineFRT2);
			testEntry.RunPreSaveValidation();
			AssertNoError("No inconsistency as there is single Rate Line for FRT.", rateLineFRT1.TL_ACInfo, expectedError);

			rateLineWAR2.TL_IsWhsJobLevelCharge = false;
			testEntry.RunPreSaveValidation();
			CombineAssertions("TL_IsWhsJobLevelCharge change should refresh group validation cache.", () =>
			{
				AssertHasError("inconsistency as ratelines have different TL_IsWhsJobLevelCharge values for same charge code WAR.", rateLineWAR1.TL_ACInfo, expectedError);
				AssertHasError("inconsistency as ratelines have different TL_IsWhsJobLevelCharge values for same charge code WAR.", rateLineWAR2.TL_ACInfo, expectedError);
			});

			RateLine CreateRateLine(ZString chargeCode, string calculatorCode, bool isWhsJobLevelCharge)
			{
				var rateLine = testEntry.AddRateLine(chargeCode, calculatorCode);
				rateLine.TL_IsWhsJobLevelCharge = isWhsJobLevelCharge;
				return rateLine;
			}
		}

		public void TestValidateFRTCalculatorRecursiveReference_ChangingPropertiesRecalculates()
		{
			const string expectedErrorMessage = "Recursive setup for Freight Inclusive Calculator is not allowed.";

			var testEntry = Helper.NewCompanyTariff().AddRateEntry("ORG", "AIR", "AUSYD", "");

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];
			var warChargeCode = Helper.ChargeCodes["WAR"];

			var rateLine1 = CreateFreightInclusiveCalculatorRateLine(testEntry, frtChargeCode, warChargeCode);
			var rateLine2 = CreateFreightInclusiveCalculatorRateLine(testEntry, bafChargeCode, frtChargeCode);
			var rateLine3 = CreateFreightInclusiveCalculatorRateLine(testEntry, cafChargeCode, bafChargeCode);

			testEntry.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasError("Line1 with FRT(WAR): should have error as there is a FreightInclusive calculator which is dependent on FRT charge code.", rateLine1.TL_ACInfo, expectedErrorMessage);
				AssertHasError("Line2 with BAF(FRT): should have error as there is a FreightInclusive calculator which is dependent on BAF charge code.", rateLine2.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line3 with CAF(BAF): should not have error as there is no FreightInclusive calculator which is dependent on CAF charge code.", rateLine3.TL_ACInfo, expectedErrorMessage);
			});

			testEntry.RateLines.RemoveAndDelete(rateLine3);
			testEntry.RunPreSaveValidation();
			CombineAssertions("RateLine_CountChanged should refresh group validation cache.", () =>
			{
				AssertHasError("Line1 with FRT(WAR): should have error as there is a FreightInclusive calculator which is dependent on FRT charge code.", rateLine1.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line2 with BAF(FRT): should not have error as there is no FreightInclusive calculator which is dependent on BAF charge code.", rateLine2.TL_ACInfo, expectedErrorMessage);
			});

			rateLine2.RateLineItems[0].TM_AC = warChargeCode.PK;
			testEntry.RunPreSaveValidation();
			CombineAssertions("TM_AC change should refresh group validation cache.", () =>
			{
				AssertNoError("Line1 with FRT(WAR): should not have error as there is no FreightInclusive calculator which is dependent on FRT charge code.", rateLine1.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line2 with BAF(WAR): should not have error as there is no FreightInclusive calculator which is dependent on BAF charge code.", rateLine2.TL_ACInfo, expectedErrorMessage);
			});

			var rateLine4 = CreateFreightInclusiveCalculatorRateLine(testEntry, cafChargeCode, bafChargeCode);
			testEntry.RunPreSaveValidation();
			CombineAssertions("RateLine_CountChanged should refresh group validation cache.", () =>
			{
				AssertNoError("Line1 with FRT(WAR): should not have error as there is no FreightInclusive calculator which is dependent on FRT charge code.", rateLine1.TL_ACInfo, expectedErrorMessage);
				AssertHasError("Line2 with BAF(WAR): should have error as there is a FreightInclusive calculator which is dependent on BAF charge code.", rateLine2.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line4 with CAF(BAF): should not have error as there is no FreightInclusive calculator which is dependent on CAF charge code.", rateLine4.TL_ACInfo, expectedErrorMessage);
			});

			rateLine4.RateLineItems.RemoveAndDeleteAll();
			testEntry.RunPreSaveValidation();
			CombineAssertions("RateLineItems_OnCountChanged: After removing RateLineItems it should refresh group validation cache.", () =>
			{
				AssertNoError("Line1 with FRT(WAR): should not have error as there is no FreightInclusive calculator which is dependent on FRT charge code.", rateLine1.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line2 with BAF(CAF): should not have error as there is no FreightInclusive calculator which is dependent on BAF charge code.", rateLine2.TL_ACInfo, expectedErrorMessage);
				AssertNoError("Line4 with CAF([NO ITEM]): should not have error as there is no FreightInclusive calculator which is dependent on CAF charge code.", rateLine4.TL_ACInfo, expectedErrorMessage);
			});

			RateLine CreateFreightInclusiveCalculatorRateLine(RateEntry rateEntry, AccChargeCode chargeCode, AccChargeCode dependentOnChargeCode)
			{
				var rateLine = rateEntry.AddRateLine(chargeCode, FreightInclusiveCalculator.Code);
				var lineItem = rateLine.RateLineItems[0];
				lineItem.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
				lineItem.TM_AC = dependentOnChargeCode.PK;

				return rateLine;
			}
		}

		public void TestValidateGlobalChargeCode()
		{
			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			var rateLine = globalCosting.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			AssertEquals("Should be only 1 notification", 1, rateLine.TL_ACInfo.Notifications.Count());
			AssertHasError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);

			globalCosting.TH_GC = GlbCompany.CurrentCompany.PK;
			rateLine.Validation.ValidateTL_AC();

			AssertNoError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);

			var globalRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			rateLine = globalRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			AssertEquals("Should be only 1 notification", 1, rateLine.TL_ACInfo.Notifications.Count());
			AssertHasError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);

			globalRate.TH_GC = GlbCompany.CurrentCompany.PK;
			rateLine.Validation.ValidateTL_AC();

			AssertNoError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);

			var globalTariff = Helper.NewGlobalTariff();
			rateLine = globalTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			AssertEquals("Should be only 1 notification", 1, rateLine.TL_ACInfo.Notifications.Count());
			AssertHasError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);

			globalTariff.TH_GC = GlbCompany.CurrentCompany.PK;
			rateLine.Validation.ValidateTL_AC();

			AssertNoError(rateLine.TL_ACInfo, ErrorMessages.InvalidGlobalChargeCode);
		}

		public void TestValidateOverrideChargeDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("FDDD", "Freight Charge", UnitCalculator.Code);

			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = cost.AddRateEntry("FCL");
			var line = entry.RateLines[0];

			line.TL_AC = chargeCode.PK;
			AssertEquals("Freight Charge", line.TL_RateDesc);

			line.OverrideChargeDescription = true;

			AssertHasWarning(line.OverrideChargeDescriptionInfo, "Overridden Cost Descriptions are only visible on billing jobs when the Revenue is autorated using a cost based calculator.");

			line.OverrideChargeDescription = false;

			AssertNoWarnings(line.OverrideChargeDescriptionInfo);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "AUBNE", chargeCode.AC_Code, 300m);
			rateEntry.RateLines[0].OverrideChargeDescription = true;

			AssertNoWarnings(rateEntry.RateLines[0].OverrideChargeDescriptionInfo);
		}

		#region Fees and Charges

		public void TestFeesAndChargesNotApplicableToClientRateOrgValidation()
		{
			var clientRateOrg = Factory.New<OrgHeader>();
			clientRateOrg.OH_Code = "Code";

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = clientRateOrg.PK;
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";

			var clientRateLine = clientRateEntry.RateLines.AddNew();
			clientRateLine.TL_FeeChargeType = "DWY";
			clientRateLine.TL_FeeChargeLevel = "STD";
			clientRateLine.TL_AC = chargeCode.PK;

			clientRateLine.Validation.ValidateTL_AC();
			AssertHasError(clientRateLine.TL_ACInfo, ErrorMessages.FeesAndChargesChargeCodeDoesNotBelongToOrganization);

			var feesAndCharges = clientRateOrg.CompanyData.RateFeeChargeLevels.AddNew();
			feesAndCharges.ORF_ServiceType = "DWY";
			feesAndCharges.ORF_Level = "STD";
			clientRateLine.Validation.ValidateTL_AC();
			AssertNoError(clientRateLine.TL_ACInfo, ErrorMessages.FeesAndChargesChargeCodeDoesNotBelongToOrganization);

			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_OH = clientRateOrg.PK;
			var companyTariffRateEntry = companyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var companyTariffRateLine = companyTariffRateEntry.RateLines.AddNew();
			companyTariffRateLine.TL_FeeChargeType = "INW";
			companyTariffRateLine.TL_FeeChargeLevel = "AAA";
			companyTariffRateLine.TL_AC = chargeCode.PK;

			AssertNoError(companyTariffRateLine.TL_ACInfo, ErrorMessages.FeesAndChargesChargeCodeDoesNotBelongToOrganization);
		}

		public void TestTL_FeeChargeType_ListValidation()
		{
			var entry = Helper.NewCompanyTariff().AddRateEntry("AIR", "LSE", "", "");
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_CompanyTariffLevel = 1;

			Assert("Pre-condition", rateLine.TL_FeeChargeType.IsEmpty);
			AssertNoErrors("Expect no error with default values", rateLine.TL_FeeChargeTypeInfo);

			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			AssertNoErrors("Expected no error as the rate is valid", rateLine.TL_FeeChargeTypeInfo);

			rateLine.TL_FeeChargeType = "XXX";

			AssertHasErrors("Expected and error as type is not from the valid look up list", rateLine.TL_FeeChargeTypeInfo);
		}

		public void TestTL_FeeChargeType_CorrectParent()
		{
			var wrongParentMessage = "Please do not enter a Fees and Charges Type on a Rate Line unless the line belongs to a Costing or Company Tariff Level 1.";

			var tariffEntry = Helper.NewCompanyTariff().AddRateEntry("AIR", "LSE", "AU", "");
			var tariffRateLine = tariffEntry.RateLines.AddNew();
			tariffRateLine.TL_CompanyTariffLevel = 1;

			Assert("Pre-condition", tariffRateLine.TL_FeeChargeType.IsEmpty);
			AssertNoError("Expect no error with default values", tariffRateLine.TL_FeeChargeTypeInfo, wrongParentMessage);

			tariffRateLine.TL_CompanyTariffLevel = 2;
			tariffRateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			AssertHasError(tariffRateLine.TL_FeeChargeTypeInfo, wrongParentMessage);

			var costingEntry = Helper.NewCosting(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "JP", "");
			var costingRateLine = costingEntry.RateLines.AddNew();

			costingRateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;

			AssertNoError("Expect no error on costing", costingRateLine.TL_FeeChargeTypeInfo, wrongParentMessage);
		}

		public void TestTL_FeeChargeLevel()
		{
			var entry = Helper.NewCompanyTariff().AddRateEntry("AIR", "LSE", "", "");
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_CompanyTariffLevel = 1;

			Assert("Pre-condition", rateLine.TL_FeeChargeType.IsEmpty);
			Assert("Pre-condition", rateLine.TL_FeeChargeLevel.IsEmpty);
			AssertNoErrors("Expect no error with default values", rateLine.TL_FeeChargeLevelInfo);

			rateLine.TL_FeeChargeType = ServiceTypeFromRegistry.Code;
			rateLine.Validation.ValidateTL_FeeChargeLevel();

			AssertHasErrors("Should have error about not being empty is type is entered", rateLine.TL_FeeChargeLevelInfo);

			rateLine.TL_FeeChargeLevel = "STD";

			AssertNoErrors(rateLine.TL_FeeChargeLevelInfo);

			rateLine.TL_FeeChargeLevel = "YYY";

			AssertHasErrors("Level is not a valid type", rateLine.TL_FeeChargeLevelInfo);
		}

		static FeeChargeType ServiceTypeFromRegistry
		{
			get
			{
				var registrySetting = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (registrySetting != null)
				{
					return registrySetting.FeeChargeTypes.FirstOrDefault() as FeeChargeType;
				}

				return null;
			}
		}

		#endregion

		#region Performance Tests

		public void TestValidationPerformance_ValidateTL_AC()
		{
			//Cache creation = 6 props hit/line = (RateLineGroupValidation - 5 prop hit/line) + (RateLineItemGroupValidation - 1 prop hit/line)
			//first line validation = Group Validation (50 x 6 = 300) + other TL_AC validations (20) = 320
			//subsequent line validation = Group Validation (0 as hitting cache) + other TL_AC validations (20) = 20
			AssertValidationPerformance_ValidateTL_AC(50, 320, 20);
		}

		public void TestValidationPerformance_ValidateTL_AC_2X()
		{
			//Cache creation = 6 props hit/line = (RateLineGroupValidation - 5 prop hit/line) + (RateLineItemGroupValidation - 1 prop hit/line)
			//first line validation = Group Validation (100 x 6 = 600) + other TL_AC validations (20) = 620
			//subsequent line validation = Group Validation (0 as hitting cache) + other TL_AC validations (20) = 20
			AssertValidationPerformance_ValidateTL_AC(100, 620, 20);
		}

		void AssertValidationPerformance_ValidateTL_AC(int totalRateLines, int expectedFirstLineValidationCount, int expectedSubsequentLineValidationCount)
		{
			AssertValidationPerformance(totalRateLines, expectedFirstLineValidationCount, expectedSubsequentLineValidationCount, (RateLine line) =>
			{
				var lineValidation = new ActualRateLinesValidation(line);
				lineValidation.ValidateTL_AC();
			});
		}

		public void TestValidationPerformance_ValidateTL_RX_NKCurrency()
		{
			//Cache creation = 6 props hit/line = (RateLineGroupValidation - 5 prop hit/line) + (RateLineItemGroupValidation - 1 prop hit/line)
			//first line validation = Group Validation (50 x 6 = 300) + other TL_RX_NKCurrency validations (22) = 322
			//subsequent line validation = Group Validation (0 as hitting cache) + other TL_RX_NKCurrency validations (22) = 22
			AssertValidationPerformance_ValidateTL_RX_NKCurrency(50, 322, 22);
		}

		public void TestValidationPerformance_ValidateTL_RX_NKCurrency_2X()
		{
			//Cache creation = 6 props hit/line = (RateLineGroupValidation - 5 prop hit/line) + (RateLineItemGroupValidation - 1 prop hit/line)
			//first line validation = Group Validation (100 x 6 = 600) + other TL_RX_NKCurrency validations (22) = 622
			//subsequent line validation = Group Validation (0 as hitting cache) + other TL_RX_NKCurrency validations (22) = 22
			AssertValidationPerformance_ValidateTL_RX_NKCurrency(100, 622, 22);
		}

		void AssertValidationPerformance_ValidateTL_RX_NKCurrency(int totalRateLines, int expectedFirstLineValidationCount, int expectedSubsequentLineValidationCount)
		{
			AssertValidationPerformance(totalRateLines, expectedFirstLineValidationCount, expectedSubsequentLineValidationCount, (RateLine line) =>
			{
				var lineValidation = new ActualRateLinesValidation(line);
				lineValidation.ValidateTL_RX_NKCurrency();
			});
		}

		void AssertValidationPerformance(int totalRateLines, int expectedFirstLineValidationCount, int expectedSubsequentLineValidationCount, Action<RateLine> codeToTest)
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var warChargeCode = Helper.ChargeCodes["WAR"];

			var rate = Helper.NewClientRate(Factory.NewWithValidTestData<OrgHeader>());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			RateLine line1;
			RateLine line2;
			RateLine line3;
			using (entry.GetValidationSuspender())
			{
				((ISupportDataImporting)entry).IsImportingData = true;

				line1 = CreateFreightInclusiveCalculatorRateLine(frtChargeCode, warChargeCode);
				line2 = CreateFreightInclusiveCalculatorRateLine(frtChargeCode, warChargeCode);
				line3 = CreateRateLine(frtChargeCode, FlatCalculator.Code, true);

				for (int i = 0; i < totalRateLines - 3; i++)
				{
					CreateRateLine(frtChargeCode, FlatCalculator.Code, true);
				}

				((ISupportDataImporting)entry).IsImportingData = false;
			}

			entry.InvalidateGroupValidation();

			int firstLineValidationCount = GetPersistentPropertiesHitCount(typeof(RateLine), () => codeToTest(line1));
			Assert($@"First Rateline group validation should not hit more than {expectedFirstLineValidationCount}, if it's more than {expectedFirstLineValidationCount} then there is something that requires code optimization for sure.
Current Hit count is {firstLineValidationCount}", firstLineValidationCount <= expectedFirstLineValidationCount);

			int secondLineValidationCount = GetPersistentPropertiesHitCount(typeof(RateLine), () => codeToTest(line2));
			Assert($@"Subsequent Rateline group validation should not hit more than {expectedSubsequentLineValidationCount}, if it's more than {expectedSubsequentLineValidationCount} then there is something that requires code optimization for sure.
Current Hit count {secondLineValidationCount}", secondLineValidationCount <= expectedSubsequentLineValidationCount);

			int thirdLineValidationCount = GetPersistentPropertiesHitCount(typeof(RateLine), () => codeToTest(line3));
			Assert($@"Subsequent Rateline group validation should not hit more than {expectedSubsequentLineValidationCount}, if it's more than {expectedSubsequentLineValidationCount} then there is something that requires code optimization for sure.
Current Hit count {secondLineValidationCount}", secondLineValidationCount <= expectedSubsequentLineValidationCount);

			RateLine CreateFreightInclusiveCalculatorRateLine(AccChargeCode chargeCode, AccChargeCode dependentOnChargeCode)
			{
				var rateLine = CreateRateLine(chargeCode, FreightInclusiveCalculator.Code, true);
				var lineItem = rateLine.RateLineItems[0];
				lineItem.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
				lineItem.TM_AC = dependentOnChargeCode.PK;

				return rateLine;
			}

			RateLine CreateRateLine(AccChargeCode chargeCode, string calculatorCode, bool isWhsJobLevelCharge)
			{
				var rateLine = entry.AddRateLine(chargeCode, calculatorCode);
				rateLine.TL_IsWhsJobLevelCharge = isWhsJobLevelCharge;
				return rateLine;
			}
		}

		#endregion

		#region Property Setter Calls RateEntry To InvalidateCache

		public void TestTM_AC_SetterCallsRateEntryToInvalidateCache()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_RateCalculator = FreightInclusiveCalculator.Code;
			var testEntry = Helper.NewCosting(null).AddRateEntry("ORG", "AIR", "AUSYD", "");
			testEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = testEntry.AddRateLine(chargeCode, FreightInclusiveCalculator.Code);
			var lineItem = rateLine.RateLineItems[0];
			lineItem.TM_Type = FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType;
			lineItem.TM_AC = chargeCode.PK;

			AssertGroupValidationCache("TM_AC", lineItem.Parent.Parent, lineItem, x => lineItem.TM_AC = x, lineItem.TM_AC, ZGuid.Empty);
		}

		public void TestTL_RX_NKCurrency_SetterCallsRateEntryToInvalidateCache()
		{
			(var testEntry, var rateLine) = CreateDataForGroupValidationTesting();
			AssertGroupValidationCache<ZString>("TL_RX_NKCurrency", testEntry, rateLine, x => rateLine.TL_RX_NKCurrency = x, rateLine.TL_RX_NKCurrency, Constants.CurrencyCodes.India);
		}

		public void TestTL_RateCalculator_SetterCallsRateEntryToInvalidateCache()
		{
			(var testEntry, var rateLine) = CreateDataForGroupValidationTesting();
			AssertGroupValidationCache<ZString>("TL_RateCalculator", testEntry, rateLine, x => rateLine.TL_RateCalculator = x, rateLine.TL_RateCalculator, CombinedCalculator.Code);
		}

		public void TestTL_AC_SetterCallsRateEntryToInvalidateCache()
		{
			(var testEntry, var rateLine) = CreateDataForGroupValidationTesting();
			AssertGroupValidationCache("TL_AC", testEntry, rateLine, x => rateLine.TL_AC = x, rateLine.TL_AC, ZGuid.Empty);
		}

		public void TestTL_IsWhsJobLevelCharge_SetterCallsRateEntryToInvalidateCache()
		{
			(var testEntry, var rateLine) = CreateDataForGroupValidationTesting();
			AssertGroupValidationCache<ZBool>("TL_IsWhsJobLevelCharge", testEntry, rateLine, x => rateLine.TL_IsWhsJobLevelCharge = x, rateLine.TL_IsWhsJobLevelCharge, !rateLine.TL_IsWhsJobLevelCharge);
		}

		(RateEntry, RateLine) CreateDataForGroupValidationTesting()
		{
			var testEntry = Helper.NewCosting(null).AddRateEntry("ORG", "AIR", "AUSYD", "");
			testEntry.RateLines.RemoveAndDeleteAll();

			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_RateCalculator = FreightInclusiveCalculator.Code;
			var rateLine = testEntry.AddRateLine(chargeCode, FreightInclusiveCalculator.Code);

			return (testEntry, rateLine);
		}

		void AssertGroupValidationCache<T>(string propName, RateEntry testEntry, BusinessObject item, Action<T> action, T sameValue, T differentValue)
		{
			using (item.GetValidationSuspender())
			{
				Assert("Pre-condition: validation should be suspended as validation code will hit RateLineGroupValidation property getter which will create object if null.", item.IsValidationSuspended);

				testEntry.HasFRTCalculatorItemsHaveSameChargeCode(null);
				Assert("Pre-condition: Calling HasFRTCalculatorItemsHaveSameChargeCode should have assigned RateLineGroupValidation object.", !testEntry.IsGroupValidationCacheMarkedForRefreshForTest);

				action(sameValue);
				Assert($"Setting same value of {propName} should not mark cache for refresh.", !testEntry.IsGroupValidationCacheMarkedForRefreshForTest);

				action(differentValue);
				Assert($"Setting different value of {propName} should mark cache for refresh.", testEntry.IsGroupValidationCacheMarkedForRefreshForTest);
			}
		}

		#endregion

		#region Start/End Date

		public void TestTL_RateStartDate()
		{
			var costing = Helper.NewCosting(null);
			var entry = costing.AddRateEntry("ORG", "AIR", "AUSYD", "", removeLines: true);
			entry.TI_RateEndDate = ZDate.Today.AddDays(30);
			var line = entry.AddFlatRateLine("FRT", 100);
			AssertEquals(ZDate.Empty, line.TL_RateStartDate);
			AssertEquals(ZDate.Empty, line.TL_RateEndDate);
			AssertNoNotifications(line.TL_RateStartDateInfo);

			line.TL_RateStartDate = entry.TI_RateStartDate.AddDays(-1);
			AssertHasError(line.TL_RateStartDateInfo, "The Start Date of Rate Line must be the same day or later than the Start Date of Rate Entry.");

			line.TL_RateStartDate = entry.TI_RateStartDate;
			AssertNoNotifications("OK if line start equals entry start", line.TL_RateStartDateInfo);

			line.TL_RateStartDate = entry.TI_RateEndDate;
			AssertNoNotifications("OK if line start equals entry end", line.TL_RateStartDateInfo);

			line.TL_RateStartDate = entry.TI_RateEndDate.AddDays(1);
			AssertHasErrors("error if line start after entry end", line.TL_RateStartDateInfo);

			line.TL_RateStartDate = ZDate.Empty;
			line.TL_RateEndDate = entry.TI_RateEndDate.AddDays(-10);
			line.TL_RateStartDate = line.TL_RateEndDate;
			AssertNoNotifications("OK if line start equals line end", line.TL_RateStartDateInfo);

			line.TL_RateStartDate = line.TL_RateEndDate.AddDays(1);
			AssertHasError(line.TL_RateStartDateInfo, "You cannot have a Start Date that is after the Expiry Date.");
		}

		public void TestTL_RateEndDate()
		{
			var costing = Helper.NewCosting(null);
			var entry = costing.AddRateEntry("ORG", "AIR", "AUSYD", "", removeLines: true);
			entry.TI_RateEndDate = ZDate.Today.AddDays(30);
			var line = entry.AddFlatRateLine("FRT", 100);
			AssertEquals(ZDate.Empty, line.TL_RateStartDate);
			AssertEquals(ZDate.Empty, line.TL_RateEndDate);
			AssertNoNotifications(line.TL_RateEndDateInfo);

			line.TL_RateStartDate = entry.TI_RateStartDate.AddDays(10);
			line.TL_RateEndDate = entry.TI_RateEndDate;
			AssertNoNotifications("OK if line end equals entry end", line.TL_RateEndDateInfo);

			line.TL_RateEndDate = entry.TI_RateEndDate.AddDays(1);
			AssertHasError(line.TL_RateEndDateInfo, "The Expiry Date of Rate Line must be the same day or earlier than the Expiry Date of Rate Entry.");

			line.TL_RateEndDate = line.TL_RateStartDate;
			AssertNoNotifications("OK if line end equals line start", line.TL_RateEndDateInfo);

			line.TL_RateEndDate = line.TL_RateStartDate.AddDays(-1);
			AssertHasError(line.TL_RateEndDateInfo, "You cannot have an Expiry Date that is before the Start Date.");

			line.TL_RateStartDate = ZDate.Empty;
			line.TL_RateEndDate = entry.TI_RateStartDate;
			AssertNoNotifications("OK if line end equals entry start", line.TL_RateEndDateInfo);

			line.TL_RateEndDate = entry.TI_RateStartDate.AddDays(-1);
			AssertHasErrors("error if line end before entry start", line.TL_RateEndDateInfo);
		}

		#endregion

		#region Implementation

		RateLine TestLine
		{
			get
			{
				if (testLine == null)
				{
					var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
					var testEntry = testRate.AddRateEntry("ORG");
					testLine = testEntry.RateLines.AddNew();
				}

				return testLine;
			}
		}

		RateLine testLine;

		#endregion
	}
}
