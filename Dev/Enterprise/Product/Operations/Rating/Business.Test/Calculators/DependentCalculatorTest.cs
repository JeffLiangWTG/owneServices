using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business.Testing
{
	public class DependentCalculatorTest : RatingTestCase
	{
		public void TestValidation()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR");

			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", FlatCalculator.Code);
			var line2 = entry.AddRateLine("WAR", PercentageCalculator.Code);
			var line3 = entry.AddRateLine("CAF", PercentageCalculator.Code);
			var line4 = entry.AddRateLine("BAF", PercentageCalculator.Code);

			line1.GetCalculator<FlatCalculator>().BaseRate = 300;

			var line2Calculator = line2.GetCalculator<PercentageCalculator>();
			var line3Calculator = line3.GetCalculator<PercentageCalculator>();
			var line4Calculator = line4.GetCalculator<PercentageCalculator>();
			line2Calculator.Percent = 2;
			line3Calculator.Percent = 4;
			line4Calculator.Percent = 6;

			var item1 = line2Calculator.AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			var item2 = line2Calculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item2.TM_AC = line1.TL_AC;
			var item3 = line3Calculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item3.TM_AC = line2.TL_AC;
			var item4 = line4Calculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item4.TM_AC = line3.TL_AC;

			entry.RunPreSaveValidation();

			Assert(item1.HasNotifications());
			Assert(!item2.HasNotifications());
			Assert(item3.HasNotifications());
			Assert(item4.HasNotifications());

			line2.SetCalculationOrder(1);
			entry.RunPreSaveValidation();

			Assert(!item1.HasNotifications());
			Assert(!item2.HasNotifications());
			Assert(!item3.HasNotifications());
			Assert(!item4.HasNotifications());
		}

		public void TestCheckTM_AC_LocalRatesUseLocalChargeCodes()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode, true, false, "GLBORG", "GLBORG");
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client);
			var rateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", PercentageCalculator.Code);
			var rateLineItem = rateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rateLineItem.TM_AC = localChargeCode.PK;

			var message = "Expecting no errors when setting a Local Charge Code as the Percentage amount of a Local Rate";
			AssertNoErrors(message, rateLineItem.TM_ACInfo);

			rateLineItem.TM_AC = globalChargeCode.PK;

			AssertHasError(rateLineItem.TM_ACInfo, ErrorMessages.LocalChargeCodeIsRequired);
		}

		public void TestCheckTM_AC_GlobalRatesUseGlobalChargeCodes()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode, true, false, "GLBORG", "GLBORG");
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var unlinkedGlobalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLB1");
			unlinkedGlobalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			unlinkedGlobalChargeCode.ChildChargeCodes.RemoveAllFromRelationship();
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var globalLine = globalEntry.AddRateLine(unlinkedGlobalChargeCode.AC_Code, PercentageCalculator.Code);
			var globalItem = globalLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			globalItem.TM_AC = globalChargeCode.PK;

			var message = "Expecting no errors when setting a Global Charge Code as the Percentage amount of a Global Rate";
			AssertNoErrors(message, globalItem.TM_ACInfo);

			globalItem.TM_AC = localChargeCode.PK;

			AssertHasError(globalItem.TM_ACInfo, ErrorMessages.GlobalChargeCodeIsRequired);
		}

		#region GetValueCalculatorAppliesTo

		public void TestGetValueCalculatorAppliesTo_WithFrtOrgCharges()
		{
			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(FrtLine.ChargeCode, "AUD", 10);
			charges.AddNew(OrgLine1.ChargeCode, "AUD", 11);
			charges.AddNew(OrgLine2.ChargeCode, "AUD", 12);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", false);
			AssertEquals("FRT 10.00 + 11 * 1.1 GST + 12 * 1.1 GST = $35.3 AUD", 35.3m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("FRT 10.00 = $10 AUD", 10m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("11 * 1.1 GST + 12 * 1.1 GST = $25.3 AUD", 25.3m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("0 Destination Charge = $0 AUD", 0m, result.Amount);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(FrtLine.ChargeCode, "NZD", 5);
			charges.AddNew(OrgLine1.ChargeCode, "NZD", 4);
			charges.AddNew(OrgLine2.ChargeCode, "NZD", 3);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", false);
			AssertEquals("FRT 5.00 + 4 * 1.1 GST + 3 * 1.1 GST = $12.7 NZD", 12.7m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("FRT 5.00 = $5 NZD", 5m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("3 * 1.1 GST + 4 * 1.1 GST = $7.7 NZD", 7.7m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("0 Destination Charge = $0 NZD", 0m, result.Amount);

			AssertNoExceptionThrown("Should correctly convert AppliesTo value to UpperCase for comparison", () => GetMoneyAmount(charges, CostSell.Cost, "NZD", "frt"));
		}

		public void TestGetValueCalculatorAppliesTo_WithFrtDstCharges()
		{
			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(FrtLine.ChargeCode, "AUD", 10);
			charges.AddNew(DstLine1.ChargeCode, "AUD", 11);
			charges.AddNew(DstLine2.ChargeCode, "AUD", 12);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", false);
			AssertEquals("FRT 10.00 + 11 * 1.1 GST + 12 * 1.1 GST = $35.3 AUD", 35.3m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("FRT 10.00 = $10 AUD", 10m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("0.0 Origin Charge = $0 AUD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("11 * 1.1 GST + 12 * 1.1 GST = $25.3 AUD", 25.3m, result.Amount);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(FrtLine.ChargeCode, "AUD", 5);
			charges.AddNew(DstLine1.ChargeCode, "NZD", 4);
			charges.AddNew(DstLine2.ChargeCode, "NZD", 3);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", false);
			AssertEquals("FRT 5.00 + (4 NZD / 1.2 * 1.1 GST) + (3 NZD / 1.2 * 1.1 GST) = $35.3 AUD", 11.413m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("FRT 5.00 = $5 AUD", 5m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("0.0 Origin Charge = $0 AUD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("3 * 1.1 GST + 4 * 1.1 GST = $7.7 AUD", 7.7m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);
		}

		public void TestGetValueCalculatorAppliesTo_WithOrgDstCharges()
		{
			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "NZD", 10);
			charges.AddNew(DstLine1.ChargeCode, "AUD", 11);
			charges.AddNew(OrgLine2.ChargeCode, "AUD", 12);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", false);
			AssertEquals("(10.00 * 1.1 GST)  + (11 AUD * 1.2 * 1.1 GST) + (12 AUD * 1.2 * 1.1 GST) = $41.36 NZD", 41.36m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("0.0 Freight Charge = $0 NZD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("(10.00 * 1.1 GST)  + (12 AUD * 1.2 * 1.1 GST) = $26.84 NZD", 26.84m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("(12 AUD  * 1.1 GST = $12.10 AUD", 12.10m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "NZD", 5);
			charges.AddNew(DstLine1.ChargeCode, "NZD", 4);
			charges.AddNew(OrgLine2.ChargeCode, "NZD", 3);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", false);
			AssertEquals("(5.0 * 1.1 GST)  + (4.0 * 1.1 GST) + (3.0 * 1.1 GST = $13.20 NZD", 13.20m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("0.0 Freight Charge = $0 NZD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("(5.0 * 1.1 GST)  + (3.0 * 1.1 GST) = $8.80 NZD", 8.80m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("4.0 * 1.1 GST = $4.40 NZD", 4.40m, result.Amount);
		}

		public void TestGetValueCalculatorAppliesTo_LoadingAndCustomsBrokerageCharges()
		{
			var chargeLOD1 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.Loading);
			var chargeLOD2 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.Loading);
			var chargeOBR1 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.OriginBrokerage);
			var chargeOBR2 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.OriginBrokerage);
			var chargeBRK1 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Brokerage);
			var chargeBRK2 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Brokerage);
			var chargeUNL1 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Unloading);
			var chargeUNL2 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Unloading);

			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(chargeLOD1.ChargeCode, "NZD", 10);
			charges.AddNew(chargeLOD2.ChargeCode, "AUD", 11);
			charges.AddNew(chargeOBR1.ChargeCode, "NZD", 12);
			charges.AddNew(chargeOBR2.ChargeCode, "AUD", 13);
			charges.AddNew(chargeBRK1.ChargeCode, "NZD", 14);
			charges.AddNew(chargeBRK2.ChargeCode, "AUD", 15);
			charges.AddNew(chargeUNL1.ChargeCode, "NZD", 16);
			charges.AddNew(chargeUNL2.ChargeCode, "AUD", 17);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", false);
			AssertEquals("(10.00 * 1.1 GST) + (11.00 AUD * 1.2 * 1.1 GST) + (12.00 * 1.1 GST) + (13.00 AUD * 1.2 * 1.1 GST) + (14.00 * 1.1 GST) + (15.00 AUD * 1.2 * 1.1 GST) + (16.00 * 1.1 GST) + (17.00 AUD * 1.2 * 1.1 GST) = $131.12 NZD", 131.12m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("0.0 Freight Charge = $0 NZD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.LoadingCharges);
			AssertEquals("(10.00 * 1.1 GST) + (11.00 AUD * 1.2 * 1.1 GST) = $25.52 NZD", 25.52m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.OriginCustomsBrokerageCharges);
			AssertEquals("((12.00 NZD / 1.2) * 1.1 GST) + (13.00 * 1.1 GST) = $25.3 AUD", 25.3m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.CustomsBrokerageCharges);
			AssertEquals("(14.00 * 1.1 GST) + (15.00 AUD * 1.2 * 1.1 GST) = $35.2 NZD", 35.2m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "NZD", CalculatorConstants.Text.UnloadingCharges);
			AssertEquals("(16.00 * 1.1 GST) + (17.00 AUD * 1.2 * 1.1 GST) = $40.04 NZD", 40.04m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(chargeLOD1.ChargeCode, "NZD", 5);
			charges.AddNew(chargeLOD2.ChargeCode, "AUD", 6);
			charges.AddNew(chargeOBR1.ChargeCode, "NZD", 7);
			charges.AddNew(chargeOBR2.ChargeCode, "AUD", 8);
			charges.AddNew(chargeBRK1.ChargeCode, "NZD", 9);
			charges.AddNew(chargeBRK2.ChargeCode, "AUD", 10);
			charges.AddNew(chargeUNL1.ChargeCode, "NZD", 11);
			charges.AddNew(chargeUNL2.ChargeCode, "AUD", 12);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", false);
			AssertEquals("(5.00 * 1.1 GST) + (6.00 AUD * 1.2 * 1.1 GST) + (7.00 * 1.1 GST) + (8.00 AUD * 1.2 * 1.1 GST) + (9.00 * 1.1 GST) + (10.00 AUD * 1.2 * 1.1 GST) + (11.00 * 1.1 GST) + (12.00 AUD * 1.2 * 1.1 GST) = $82.72 NZD", 82.72m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.FreightCharges);
			AssertEquals("0.0 Freight Charge = $0 NZD", 0m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.LoadingCharges);
			AssertEquals("(5.00 * 1.1 GST) + (6.00 AUD * 1.2 * 1.1 GST) = $13.42 NZD", 13.42m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.OriginCustomsBrokerageCharges);
			AssertEquals("(7.00 * 1.1 GST) + (8.00 AUD * 1.2 * 1.1 GST) = $18.26 NZD", 18.26m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", CalculatorConstants.Text.CustomsBrokerageCharges);
			AssertEquals("((9.00 NZD / 1.2) * 1.1 GST) + (10.00 * 1.1 GST) = $19.25 AUD", 19.25m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", CalculatorConstants.Text.UnloadingCharges);
			AssertEquals("(11.00 * 1.1 GST) + (12.00 AUD * 1.2 * 1.1 GST) = $27.94 NZD", 27.94m, result.Amount);
			AssertEquals("NZD was the requested currency", "NZD", result.Unit);
		}

		public void TestCalculateSellAndCostAmounts_AgentDeclared()
		{
			var charges = new AutoRateInfoCollection(Factory);
			var info = charges.AddNew(OrgLine1.ChargeCode, "NZD", 3);

			var result = GetMoneyAmount(charges, CostSell.Cost, "NZD", false);
			AssertEquals("3 * 1.1 GST = $3.30 NZD", 3.30m, result.Amount);

			RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", true);
			AssertEquals("3 * 1.1 GST = $3.30 NZD", 3.30m, result.Amount);

			RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", true);
			AssertEquals("Unspecified Cost should force Zero to be pulled through", 0m, result.Amount);

			info.AddAgentFlatPaymentBasis(9m, "NZD");
			result = GetMoneyAmount(charges, CostSell.Cost, "NZD", true);
			AssertEquals("9 * 1.1 GST = $9.90 NZD", 9.90m, result.Amount);
		}

		public void TestGetValueCalculatorAppliesTo_ExcludeGST()
		{
			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "AUD", 110);
			charges.AddNew(FrtLine.ChargeCode, "AUD", 300);
			charges.AddNew(DstLine1.ChargeCode, "AUD", 220);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", false);
			AssertEquals("300 FRT + 110 * 1.1 GST + 220 * 1.1 GST = $663.00 AUD", 663m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("110 * 1.1 GST= $121.00 AUD", 121m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Revenue, "AUD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("220 * 1.1 GST = $242.00 AUD", 242m, result.Amount);

			result = GetMoneyAmount(charges, false, CostSell.Revenue, "AUD", CalculatorConstants.Text.AllCharges, false);
			AssertEquals("300 FRT + 110 + 220 = $630.00 AUD", 630m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, false, CostSell.Revenue, "AUD", CalculatorConstants.Text.OriginCharges, false);
			AssertEquals("110 Origin Charge Excluding GST = $110.00 AUD", 110m, result.Amount);

			result = GetMoneyAmount(charges, false, CostSell.Revenue, "AUD", CalculatorConstants.Text.DestinationCharges, false);
			AssertEquals("220 Destination Charge Excluding GST = $220.00 AUD", 220m, result.Amount);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "AUD", 55);
			charges.AddNew(FrtLine.ChargeCode, "AUD", 200);
			charges.AddNew(DstLine1.ChargeCode, "AUD", 110);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", false);
			AssertEquals("55 * 1.1 GST + 200 Freight + 110 * 1.1 GST = $381.50 AUD", 381.5m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", CalculatorConstants.Text.OriginCharges);
			AssertEquals("55 * 1.1 GST = $60.50 AUD", 60.5m, result.Amount);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD", CalculatorConstants.Text.DestinationCharges);
			AssertEquals("110 * 1.1 GST = $121.00 AUD", 121m, result.Amount);

			result = GetMoneyAmount(charges, false, CostSell.Cost, "AUD", CalculatorConstants.Text.AllCharges, false);
			AssertEquals("55 + 200 Freight + 110 = $365.00 AUD", 365m, result.Amount);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, false, CostSell.Cost, "AUD", CalculatorConstants.Text.OriginCharges, false);
			AssertEquals("55 Origins Charges Excluding GST = $55.00 AUD", 55m, result.Amount);

			result = GetMoneyAmount(charges, false, CostSell.Cost, "AUD", CalculatorConstants.Text.DestinationCharges, false);
			AssertEquals("110 Destination Charges Excluding GST = $110.00 AUD", 110m, result.Amount);
		}

		public void TestGetValueCalculatorAppliesTo_WithCurrencyConversion()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "AUD", 110);
			charges.AddNew(FrtLine.ChargeCode, "EUR", 300);
			charges.AddNew(DstLine1.ChargeCode, "USD", 220);

			var result = GetMoneyAmount(charges, CostSell.Revenue, "AUD"); // Rounding is to be addressed on percentage calculator
			AssertEquals(
				"(110 / 1.0 * 1.1 GST) + (220 / 0.7 * 1.1 GST ) + (300 Freight / 0.5) = $1066.72 AUD",
				1066.719m,
				result.Amount
			);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "USD");
			AssertEquals(
				"(110 * 0.7 * 1.1 GST) + (220 * 1.1 GST ) + (300 Freight / 0.5 * 0.7) = 746.70 USD",
				746.7m,
				result.Amount
			);
			AssertEquals("USD was the requested currency", "USD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Revenue, "EUR"); // Rounding is to be addressed on percentage calculator
			AssertEquals(
				"(110 * 0.5 * 1.1 GST) + (220 / 0.7 * 0.5 * 1.1 GST ) + (300 Freight) = $533.35 EUR",
				533.354m,
				result.Amount
			);
			AssertEquals("EUR was the requested currency", "EUR", result.Unit);

			charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(OrgLine1.ChargeCode, "EUR", 50);
			charges.AddNew(FrtLine.ChargeCode, "USD", 200);
			charges.AddNew(DstLine1.ChargeCode, "AUD", 110);

			result = GetMoneyAmount(charges, CostSell.Cost, "AUD");
			AssertEquals(
				"(50 / 0.5 * 1.1 GST) + (200 / 0.7 Freight) + (110 * 1.1 GST) = $516.71 AUD",
				516.71m,
				result.Amount
			);
			AssertEquals("AUD was the requested currency", "AUD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "USD");
			AssertEquals(
				"(50 / 0.5 * 0.7 * 1.1 GST) + (200 Freight) + (110 * 0.7 * 1.1 GST) = $361.70 USD",
				361.70m,
				result.Amount
			);
			AssertEquals("USD was the requested currency", "USD", result.Unit);

			result = GetMoneyAmount(charges, CostSell.Cost, "EUR");
			AssertEquals(
				"(50 * 1.1 GST) + (200 / 0.7 * 0.5 Freight) + (110 * 0.5 * 1.1 GST) = 258.36 EUR",
				258.36m,
				result.Amount
			);
			AssertEquals("EUR was the requested currency", "EUR", result.Unit);
		}

		public void TestGetValueCalculatorAppliesTo_SourceForNoAmount()
		{
			var charges = new AutoRateInfoCollection(Factory);

			var result = GetMoneyAmount(charges, CostSell.Cost, Constants.CurrencyCodes.Australia);
			AssertEquals("No Charges = 0 AUD", 0m, result.Amount);
		}

		public void TestGetValueCalculatorAppliesTo_SourceForSingleAmount()
		{
			var charges = new AutoRateInfoCollection(Factory);
			charges.AddNew(Helper.ChargeCodes["FRT"], Constants.CurrencyCodes.Australia, 10);

			var result = GetMoneyAmount(charges, CostSell.Cost, Constants.CurrencyCodes.Australia, false);
			AssertEquals("10 Freight = $10 AUD", 10m, result.Amount);
		}

		public void TestGetValueCalculatorAppliesTo_SourceForManyAmounts()
		{
			var chargeCode1 = Helper.ChargeCodes["ABC123"];
			var chargeCode2 = Helper.ChargeCodes["DEF456"];

			Func<string, Quantity> getMoneySummaryForIDependantCalculator = country =>
			{
				GlbCompany.CurrentCompany.SetCountry(country);

				var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var charges = new AutoRateInfoCollection(Factory);
				charges.AddNew(chargeCode1, localCurrency, 20.202);
				charges.AddNew(chargeCode1, localCurrency, 100);
				charges.AddNew(chargeCode2, localCurrency, 10000);

				return GetMoneyAmount(charges, CostSell.Cost, localCurrency);
			};

			var result = getMoneySummaryForIDependantCalculator(Constants.CountryCodes.Australia);
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, result.Unit);

			AssertEquals("Pre-condition failed for Australia, decimals mismatch.", 2, currency.Decimals);
			AssertEquals("The amount should match AUD with decimals.", 10120.20m, result.Amount);
			AssertEquals("AUD was the requested currency.", "AUD", result.Unit);
			AssertEquals(
				"Result source should correctly sum and format charge items with AUD.",
				"AUD 10120.20 (ABC123 20.20 + ABC123 100.00 + DEF456 10000.00)",
				result.Source
			);

			result = getMoneySummaryForIDependantCalculator(Constants.CountryCodes.Japan);
			currency = RefCurrency.LoadFromCurrencyCode(Factory, result.Unit);

			AssertEquals("Pre-condition failed for Japan, decimals mismatch.", 0, currency.Decimals);
			AssertEquals("The amount should match JPY without decimals.", 10120m, result.Amount);
			AssertEquals("JPY was the requested currency.", "JPY", result.Unit);
			AssertEquals(
				"Result source should correctly sum and format charge items with JPY.",
				"JPY 10120 (ABC123 20 + ABC123 100 + DEF456 10000)",
				result.Source
			);

			result = getMoneySummaryForIDependantCalculator(Constants.CountryCodes.Jordan);
			currency = RefCurrency.LoadFromCurrencyCode(Factory, result.Unit);

			AssertEquals("Pre-condition failed for Jordan, decimals mismatch.", 3, currency.Decimals);
			AssertEquals("The amount should match JOD with thousandths decimals.", 10120.202m, result.Amount);
			AssertEquals("JOD was the requested currency.", "JOD", result.Unit);
			AssertEquals(
				"Result source should correctly sum and format charge items with JOD.",
				"JOD 10120.202 (ABC123 20.202 + ABC123 100.000 + DEF456 10000.000)",
				result.Source
			);
		}

		#endregion

		#region Implementation

		RatingHeader Header
		{
			get
			{
				if (header == null)
				{
					var org = Helper.NewOrgHeader();
					header = Helper.NewClientRate(org);
				}
				return header;
			}
		}
		RatingHeader header;

		RateEntry OrgEntry
		{
			get { return orgEntry ?? (orgEntry = Header.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX")); }
		}
		RateEntry orgEntry;

		RateEntry DstEntry
		{
			get { return dstEntry ?? (dstEntry = Header.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "AUSYD", "USLAX")); }
		}
		RateEntry dstEntry;

		RateLine OrgLine1
		{
			get { return orgLine1 ?? (orgLine1 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.Origin)); }
		}
		RateLine orgLine1;

		RateLine OrgLine2
		{
			get { return orgLine2 ?? (orgLine2 = AddRateLine(OrgEntry, ChargeCodeGroupList.Codes.Origin)); }
		}
		RateLine orgLine2;

		RateLine FrtLine
		{
			get { return frtLine ?? (frtLine = Header.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("FRT", FlatCalculator.Code)); }
		}
		RateLine frtLine;

		RateLine DstLine1
		{
			get { return dstLine1 ?? (dstLine1 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Destination)); }
		}
		RateLine dstLine1;

		RateLine DstLine2
		{
			get { return dstLine2 ?? (dstLine2 = AddRateLine(DstEntry, ChargeCodeGroupList.Codes.Destination)); }
		}
		RateLine dstLine2;

		RateLine AddRateLine(RateEntry rateEntry, string group)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = group;
			chargeCode.AC_AT_GSTRate = TaxRate.PK;

			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			return rateLine;
		}

		AccTaxRate taxRate;
		AccTaxRate TaxRate
		{
			get
			{
				if (taxRate == null)
				{
					taxRate = Factory.New<AccTaxRate>();
					taxRate.SetRateNumerator_ForTestOnly(10);
				}

				return taxRate;
			}
		}

		Quantity GetMoneyAmount(AutoRateInfoCollection collection, CostSell costOrSell, ZString currency, bool useAgentRates)
		{
			return GetMoneyAmount(collection, true, costOrSell, currency, CalculatorConstants.Text.AllCharges, useAgentRates);
		}

		Quantity GetMoneyAmount(AutoRateInfoCollection collection, CostSell costOrSell, ZString currency)
		{
			return GetMoneyAmount(collection, true, costOrSell, currency, CalculatorConstants.Text.AllCharges, false);
		}

		Quantity GetMoneyAmount(AutoRateInfoCollection collection, CostSell costOrSell, ZString currency, string applyTo)
		{
			return GetMoneyAmount(collection, true, costOrSell, currency, applyTo, false);
		}

		Quantity GetMoneyAmount(AutoRateInfoCollection collection, bool includeGST, CostSell costOrSell, ZString currency, string applyTo, bool useAgentRates)
		{
			var criteria = new TestRatingCriteria();

			criteria.CurrencyConverter = new TestCurrencyConverter(Factory);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.SetResults_ForTest(collection);

			RatingHeader ratingHeader = costOrSell == CostSell.Cost ? Factory.New<Costing>() : Factory.New<ClientRate>();
			var entry = ratingHeader.AddRateEntry("AIR");
			var applyToLine = entry.AddRateLine("BAF");
			applyToLine.ViewAgentRates = useAgentRates;
			applyToLine.RateLineItems.RemoveAndDeleteAll();
			applyToLine.TL_RateCalculator = PercentageCalculator.Code;
			applyToLine.TL_RX_NKCurrency = currency;

			var calculator = (PercentageCalculator)applyToLine.Calculator;
			calculator.AddApplyToItem(applyTo);

			IRateLineItem greaterChargeItem;
			return calculator.GetValueCalculatorAppliesTo(parameters, out greaterChargeItem, includeGST, calculator.GreaterCharge, true);
		}

		#endregion
	}
}
