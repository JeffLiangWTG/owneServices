using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CompanyTariffOrCostBasedCalculatorTest : CalculatorTest
	{
		[ExpectNoExceptions]
		public void TestGetCloneDoesNotCauseException()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "NZAKL");
			var rateLine = entry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.CN);
			var item1 = rateLine.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.MIN;
			item1.TM_RelevantValue = 30m;

			var item2 = rateLine.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.Minus;
			item2.TM_Break = 45.0m;
			item2.TM_BreakWeightVolume = QuantityUnit.KG;
			item2.TM_RelevantValue = 30m;

			var item3 = rateLine.RateLineItems.AddNew();
			item3.TM_Type = Calculator.Items.Operator.Plus;
			item3.TM_Break = 45.0m;
			item3.TM_RelevantValue = 30m;

			var dummyRate = Factory.New<ClientRate>();
			var dummyEntry = dummyRate.AddRateEntry(RatingConstants.RateCategory.ORG);

			var dummyRateLine = dummyEntry.RateLines.AddNew();
			dummyRateLine.IsBulkRateUpdateActionLine = true;
			dummyRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(dummyRateLine);
			AssertNoExceptionThrown(() => cloneHelper.CreateClone(rateLine));
		}

		public void TestGetCloneCopiesNoteText()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");

			var tariffLine = tariffEntry.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			tariffLine.ChargeInformationNoteText = "Tariff Text";
			((UnitCalculator)tariffLine.Calculator).PerUnit = 50;

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var quoteEntry = testQuote.AddRateEntry("DST", "AIR", "", "AUSYD");

			var quoteLine = quoteEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			quoteLine.ChargeInformationNoteText = "Quote Text";

			var calc = (CompanyTariffOrCostBasedCalculator)quoteLine.Calculator;
			calc.Percent = -25m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(quoteLine);
			var clone = cloneHelper.CreateClone(tariffLine);

			AssertEquals("Quote Text", clone.ChargeInformationNoteText);
		}

		public void TestGetCloneCopiesMessageTypeAndSubType()
		{
			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("DST", "AIR", "", "AUSYD");

			var tariffLine = tariffEntry.AddRateLine("CCLR", AgencyCalculator.Code);
			tariffLine.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			tariffLine.Calculator.MessageSubType = "FRM";
			tariffLine.GetCalculator<AgencyCalculator>().AgencyFeeType = RateFeeTypeList.Codes.PerShipment;
			tariffLine.GetCalculator<AgencyCalculator>().AgencyLineType = RateLineTypeList.Codes.FLAT;
			tariffLine.GetCalculator<AgencyCalculator>().AgencyRate = 150m;

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var quoteEntry = testQuote.AddRateEntry("DST", "AIR", "", "AUSYD");

			var quoteLine = quoteEntry.AddRateLine("CCLR", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			quoteLine.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			quoteLine.Calculator.MessageSubType = "SAC";
			quoteLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -25m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(quoteLine);
			var clone = cloneHelper.CreateClone(tariffLine);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, clone.Calculator.MessageType);
			AssertEquals("SAC", clone.Calculator.MessageSubType);

			quoteLine.Calculator.MessageType = ZString.Empty;
			quoteLine.Calculator.MessageSubType = ZString.Empty;

			clone = cloneHelper.CreateClone(tariffLine);
			AssertEquals(ZString.Empty, clone.Calculator.MessageType);
			AssertEquals(ZString.Empty, clone.Calculator.MessageSubType);
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNull(Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder));
			AssertNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType));
			AssertNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType));
			AssertEquals(9, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, TestCalculator.Decimal3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU).TM_RelevantValueInfo, TestCalculator.Decimal5Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo, TestCalculator.Decimal4Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder).TM_TextInfo, TestCalculator.String1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_TextInfo, TestCalculator.String2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageType).TM_TextInfo, TestCalculator.String3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.MessageSubType).TM_TextInfo, TestCalculator.String4Info);
		}

		[ExpectNoExceptions]
		public void TestCostBasedOnCost_PreventStackOverflow()
		{
			var cost = Factory.New<Costing>();
			cost.TH_OH = Factory.New<OrgHeader>().PK;
			cost.Header.OH_Code = "TESTZUB";

			var entry = cost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var criteria = CreateCriteria();
			criteria.SetSupplier(cost.Header);

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			entry.RateLines[0].Calculator.GetBaseCalculator(parameters);
		}

		[ExpectNoExceptions]
		public void TestTariffBasedOnTariff_PreventStackOverflow()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			entry.RateLines[0].Calculator.GetBaseCalculator(parameters);
		}

		////public void TestSetDefaultMarkUp()
		////{
		////	GlbCompany.CurrentCompany.SetCountry("AU");

		////	Env.Registry.Rating.MarkUpPercentages = "AIO|USLAX|16|50|1.5;AIR|USLAX|15|50|1.5";

		////	fCalculatorCode = CompanyTariffOrCostBasedCalculator.CostBasedCode;
		////	Line.Parent.TI_OriginLRC = "AUSYD";
		////	Line.Parent.TI_DestinationLRC = "USLAX";
		////	Line.Parent.TI_RateCategory = "AIR";

		////	AssertEquals(16m, TestCalculator.Percent);
		////	AssertEquals(0m, TestCalculator.Minimum);
		////	AssertEquals(0m, TestCalculator.PerUnit);
		////	AssertEquals(true, TestCalculator.BaseOnApportionment);

		////	Line.TL_AC = Env.Registry.FreightChargeCode;
		////	Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

		////	AssertEquals(0m, ((CompanyTariffOrCostBasedCalculator)Line.Calculator).Percent);
		////	AssertEquals(50m, ((CompanyTariffOrCostBasedCalculator)Line.Calculator).Minimum);
		////	AssertEquals(1.5m, ((CompanyTariffOrCostBasedCalculator)Line.Calculator).PerUnit);
		////	AssertEquals(false, TestCalculator.BaseOnApportionment);
		////}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.BAS, "Decimal1");
			TestMapping(CalculatorConstants.Type.PER, "Decimal2");
			TestMapping(Calculator.Items.Operator.UNT, "Decimal3");
			TestMapping(Calculator.Items.Operator.MIN, "Decimal4");
			TestMapping(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder, "String1");
			TestMapping(CartageCalculator.Items.EquipmentType, "String2");
			TestMapping(AgencyCalculator.Items.MessageType, "String3");
			TestMapping(AgencyCalculator.Items.MessageSubType, "String4");
		}

		public override void TestList1()
		{
			AssertEquals(typeof(CodeDescriptionPairList), TestCalculator.List1.GetType());
			Assert("Count > 0", TestCalculator.List1.Count > 1);
		}

		public override void TestList2()
		{
			AssertEquals(typeof(CodeDescriptionPairList), TestCalculator.List2.GetType());
		}

		public override void TestList3()
		{
			AssertEquals("List3", typeof(Customs.Common.AU.AUJobMessageTypeList), TestCalculator.List3.GetType());
			AssertEquals("Count", 8, TestCalculator.List3.Count);
		}

		public override void TestList4()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals("List4", typeof(CodeDescriptionPairList), TestCalculator.List4.GetType());
				AssertEquals("Count", 3, TestCalculator.List4.Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public override void TestQuotationLines()
		{
			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var quote = Helper.NewQuote(NewClient);
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");

			InitialiseTestCalculator();

			Factory.Save();

			AssertExceptionThrown(typeof(NotSupportedException), () => TestCalculator.GetQuotationLines(parentEntry));
		}

		#region Calculation

		#region Apply increase to cost and company tariff calculated with Min rate

		public void TestCalculation_CostOrTariffCalculatedWithMinRate_ApplyIncrease_1()
		{
			AssertIncreaseApplied(
				chargeMinRate: 2000,
				chargePerUnitRate: 10,
				chargeableAmount: 100,

				minimumIncrease: 500,
				baseIncrease: 200,
				perUnitIncrease: 10,

				expectedAmount: 2500,
				expectedDescription: "Minimum AUD 2500.00",
				message: @"
$2000 Cost + $500 Revenue = $2500
VS
$200 Revenue + 100 * ($10 Cost + $10 Revenue) = $2200");
		}

		public void TestCalculation_CostOrTariffCalculatedWithMinRate_ApplyIncrease_2()
		{
			AssertIncreaseApplied(
				chargeMinRate: 2000,
				chargePerUnitRate: 10,
				chargeableAmount: 100,

				minimumIncrease: 500,
				baseIncrease: 1000,
				perUnitIncrease: 10,

				expectedAmount: 3000,
				expectedDescription: "Base Rate AUD 1000.00 + 100 Kilogram(s) @ AUD 20.00/KG",
				message: @"
$2000 Cost + $500 Revenue = $2500
VS
$1000 Revenue + 100 * ($10 Cost + $10 Revenue) = $3000");
		}

		public void TestCalculation_CostOrTariffCalculatedWithMinRate_ApplyIncrease_3()
		{
			AssertIncreaseApplied(
				chargeMinRate: 2000,
				chargePerUnitRate: 10,
				chargeableAmount: 100,

				minimumIncrease: 500,
				baseIncrease: 200,
				perUnitIncrease: 20,

				expectedAmount: 3200,
				expectedDescription: "Base Rate AUD 200.00 + 100 Kilogram(s) @ AUD 30.00/KG",
				message: @"
$2000 Cost + $500 Revenue = $2500
VS
$200 Revenue + 100 * ($10 Cost + $20 Revenue) = $3200");
		}

		#endregion

		#region Apply increase to cost calculated with Max rate

		public void TestCalculation_CostCalculatedWithMaxRate_ApplyIncrease_1()
		{
			AssertCostBasedCalculation(
				chargeMaxRate: 5000,

				minimumIncrease: 100,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 5720,
				expectedDescription: "Base Rate AUD 220.00 + 110.00% of (Base Rate AUD 5000.00)",
				message: @"Costs with MAX amout will be converted as flat amount since we don't save per unit payment basis
if MAX amount was used and therefore we can't properly apply per unit change");

			AssertCostBasedCalculation(
				chargeMaxRate: 5000,

				minimumIncrease: 100,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 5700,
				expectedDescription: "Base Rate AUD 200.00 + 110.00% of (Base Rate AUD 5000.00)",
				message: @"Costs with MAX amout will be converted as flat amount since we don't save per unit payment basis
if MAX amount was used and therefore we can't properly apply per unit change");
		}

		#endregion

		#region Apply increase to tariff calculated with Max rate

		public void TestCalculation_TariffCalculatedWithMaxRate_ApplyIncrease_1()
		{
			AssertCompanyTariffBasedCalculation(
				chargeableAmount: 100,
				chargePerUnitRate: 70,
				chargeMaxRate: 5000,

				minimumIncrease: 100,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 1,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 5500,
				expectedDescription: "Maximum AUD 5500.00",
				message: "110% of (Maximum 5000 AUD)");
		}

		public void TestCalculation_TariffCalculatedWithMaxRate_ApplyIncrease_2()
		{
			AssertCompanyTariffBasedCalculation(
				chargeableAmount: 100,
				chargePerUnitRate: 70,
				chargeMaxRate: 5000,

				minimumIncrease: 100,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 1,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 5500,
				expectedDescription: "Maximum AUD 5500.00",
				message: "110% of (Maximum 5000 AUD)");
		}

		#endregion

		#region Apply increase to cost or tariff calculated with Per Unit rate

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_1()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 1000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 1770,
				expectedDescription: "Base Rate AUD 210.00 + 100 Kilogram(s) @ AUD 15.60/KG",
				message: "105% of (Base AUD 0 + Base AUD 200) + 100 KG * (120% of (Per Unit AUD 10 + Per Unit AUD 3)");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_2()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 1000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 1700,
				expectedDescription: "Base Rate AUD 200.00 + 100 Kilogram(s) @ AUD 15.00/KG",
				message: "105% of Base AUD 0 + Base AUD 200 + 100 KG * (120% of Per Unit AUD 10) + 100 KG * Per Unit AUD 3");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_3()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 5000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 5250,
				expectedDescription: "Minimum AUD 5250.00",
				message: "105% of (Minimum 0 AUD + Minimum AUD 5000)");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_4()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 5000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 5000,
				expectedDescription: "Minimum AUD 5000.00",
				message: "105% of Minimum 0 AUD + Minimum AUD 5000");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_5()
		{
			AssertCostBasedCalculation(
				chargeMinRate: 500,
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 100,
				baseIncrease: 200,
				perUnitIncrease: 10,

				expectedAmount: 2200,
				expectedDescription: "Base Rate AUD 200.00 + 100 Kilogram(s) @ AUD 20.00/KG",
				message: @"
$500 Cost + $100 Revenue = $600
VS
$200 Revenue + 100 * ($10 Cost + $10 Revenue) = $2200");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitRate_ApplyIncrease_6()
		{
			AssertCostBasedCalculation(
				chargeMinRate: 500,
				chargeableAmount: 100,
				chargePerUnitRate: 10,

				minimumIncrease: 2000,
				baseIncrease: 200,
				perUnitIncrease: 10,

				expectedAmount: 2500,
				expectedDescription: "Minimum AUD 2500.00",
				message: @"
$2000 Cost + $500 Revenue = $2500
VS
$200 Revenue + 100 * ($10 Cost + $10 Revenue) = $2200");
		}

		#endregion

		#region Apply increase to cost or tariff calculated with Flat rate

		public void TestCalculation_CostOrTariffCalculatedWithFlatRate_ApplyIncrease_1()
		{
			AssertIncreaseApplied(
				chargeBaseRate: 1000,

				minimumIncrease: 500,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 1320,
				expectedDescription: "Base Rate AUD 220.00 + 110.00% of (Base Rate AUD 1000.00)",
				message: "110% of (Base AUD 1000 + Base AUD 200)");
		}

		public void TestCalculation_CostOrTariffCalculatedWithFlatRate_ApplyIncrease_2()
		{
			AssertIncreaseApplied(
				chargeBaseRate: 1000,

				minimumIncrease: 500,
				baseIncrease: 200,
				percentageIncrease: 10,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 1300,
				expectedDescription: "Base Rate AUD 200.00 + 110.00% of (Base Rate AUD 1000.00)",
				message: "110% of Base AUD 1000 + Base AUD 200");
		}

		public void TestCalculation_CostOrTariffCalculatedWithFlatRate_ApplyIncrease_3()
		{
			AssertIncreaseApplied(
				chargeBaseRate: 1000,

				minimumIncrease: 5000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 5250,
				expectedDescription: "Minimum AUD 5250.00",
				message: "105% of (Minimum 0 AUD + Minimum AUD 5000)");
		}

		public void TestCalculation_CostOrTariffCalculatedWithFlatRate_ApplyIncrease_4()
		{
			AssertIncreaseApplied(
				chargeBaseRate: 1000,

				minimumIncrease: 5000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 5000,
				expectedDescription: "Minimum AUD 5000.00",
				message: "105% of Minimum 0 AUD + Minimum AUD 5000");
		}

		#endregion

		#region Apply increase to cost or tariff calculated with flat and per unit rate

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitAndFlatRate_ApplyIncrease_1()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,
				chargeBaseRate: 300,

				minimumIncrease: 1000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 2085,
				expectedDescription: "Base Rate AUD 210.00 + 100 Kilogram(s) @ AUD 15.60/KG + 105.00% of (Base Rate AUD 300.00)",
				message: "105% of (Base AUD 300 + Base AUD 200) + 100 KG * (120% of (Per Unit AUD 10 + Per Unit AUD 3)");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitAndFlatRate_ApplyIncrease_2()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,
				chargeBaseRate: 300,

				minimumIncrease: 1000,
				baseIncrease: 200,
				percentageIncrease: 5,
				perUnitIncrease: 3,
				perUnitPercentageIncrease: 20,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,

				expectedAmount: 2015,
				expectedDescription: "Base Rate AUD 200.00 + 100 Kilogram(s) @ AUD 15.00/KG + 105.00% of (Base Rate AUD 300.00)",
				message: "105% of Base AUD 300 + Base AUD 200 + 100 KG * (120% of Per Unit AUD 10) + 100 KG * Per Unit AUD 3");
		}

		public void TestCalculation_CostOrTariffCalculatedWithPerUnitAndFlatRate_ApplyIncrease_3()
		{
			AssertIncreaseApplied(
				chargeableAmount: 100,
				chargePerUnitRate: 10,
				chargeBaseRate: -300,

				baseIncrease: 200,
				perUnitIncrease: 3,
				calculationOrder: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,

				expectedAmount: 1200,
				expectedDescription: "Base Rate AUD -300.00 + Base Rate AUD 200.00 + 100 Kilogram(s) @ AUD 13.00/KG",
				message: "Should properly apply change to a negative flat rate on a cost");
		}

		#endregion

		void AssertIncreaseApplied(
			decimal chargeableAmount = 0,
			decimal chargePerUnitRate = 0,
			decimal chargeBaseRate = 0,
			decimal chargeMinRate = 0,
			decimal chargeMaxRate = 0,
			decimal minimumIncrease = 0,
			decimal baseIncrease = 0,
			decimal percentageIncrease = 0,
			decimal perUnitIncrease = 0,
			decimal perUnitPercentageIncrease = 0,
			string calculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
			decimal expectedAmount = 0,
			string expectedDescription = "",
			string message = default)
		{
			AssertCostBasedCalculation(
				chargeableAmount,
				chargePerUnitRate,
				chargeBaseRate,
				chargeMinRate,
				chargeMaxRate,
				minimumIncrease,
				baseIncrease,
				percentageIncrease,
				perUnitIncrease,
				perUnitPercentageIncrease,
				calculationOrder,
				expectedAmount,
				expectedDescription,
				message);

			AssertCompanyTariffBasedCalculation(
				chargeableAmount,
				chargePerUnitRate,
				chargeBaseRate,
				chargeMinRate,
				chargeMaxRate,
				minimumIncrease,
				baseIncrease,
				percentageIncrease,
				perUnitIncrease,
				perUnitPercentageIncrease,
				calculationOrder,
				expectedAmount,
				expectedDescription,
				message);
		}

		void AssertCostBasedCalculation(
			decimal chargeableAmount = 0,
			decimal chargePerUnitRate = 0,
			decimal chargeBaseRate = 0,
			decimal chargeMinRate = 0,
			decimal chargeMaxRate = 0,
			decimal minimumIncrease = 0,
			decimal baseIncrease = 0,
			decimal percentageIncrease = 0,
			decimal perUnitIncrease = 0,
			decimal perUnitPercentageIncrease = 0,
			string calculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
			decimal expectedAmount = 0,
			string expectedDescription = "",
			string message = default)
		{
			if (chargeMinRate > 0 && chargePerUnitRate != 0 && chargeBaseRate != 0 && chargeMaxRate != 0)
			{
				throw new ArgumentException(@"The charge amounts will lead to incorrect payment bases created.
We only save payment bases used for calculation of the charge amount.

So, the following combinations are allowed:
chargeMinRate - if the charge is calculated using Minimum rate (MIN payment basis will be created)
chargeMaxRate - if the charge is calculated using Maximum rate (MAX payment basis will be created)
chargePerUnitRate - if the charge is calculated using Per Unit rate (UNT payment basis will be created)
chargeBaseRate - if the charge is calculated using Flat rate (FLT payment basis will be created)
chargePerUnitRate and chargeBaseRate - if the charge is calculated with Flat and Per Unit rate (FLT and UNT payment bases will be created).");
			}

			// Cost
			var charge = CreateCostCharge();
			var baseAdded = false;

			if (chargeMinRate > 0 || chargePerUnitRate > 0)
			{
				var basis = charge.Factory.New<JobPaymentBasis>();
				basis.PBS_JR = charge.PK;
				basis.PBS_IsCost = true;
				basis.PBS_PerUnitRate = chargePerUnitRate;
				basis.PBS_MinRate = chargeMinRate;
				basis.PBS_MaxRate = chargeMaxRate;
				basis.PBS_RX_NKRateCurrency = "AUD";
				basis.PBS_RateUnit = "KG";
				basis.PBS_RateUnitMultiplier = 1;
				basis.PBS_ChargeableAmount = chargeableAmount;
				basis.PBS_ChargeableUnit = "KG";

				var perUnitAmount = basis.PBS_PerUnitRate * basis.PBS_ChargeableAmount;

				if (perUnitAmount > basis.PBS_MinRate)
				{
					basis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);
				}
				else
				{
					basis.PBS_RateReference = nameof(RateInfo.RateInfoType.MIN);
					basis.PBS_FlatRate = chargeBaseRate;

					baseAdded = true;
				}
			}

			if (chargeBaseRate != 0 && !baseAdded)
			{
				charge.AddFlatCalculation(chargeBaseRate, "AUD");
			}

			if (chargeMaxRate != 0)
			{
				charge.AddMaxCalculation(chargeMaxRate, "AUD");
			}

			Factory.Save();

			// Calculator
			var line = Entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateDesc = "Test Rate";
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var calculator = line.Calculator as CompanyTariffOrCostBasedCalculator;
			calculator.Minimum = minimumIncrease;
			calculator.BaseRate = baseIncrease;
			calculator.Percent = percentageIncrease;
			calculator.PerUnit = perUnitIncrease;
			calculator.PerUnitPercent = perUnitPercentageIncrease;
			calculator.CalculationOrder = calculationOrder;

			Criteria.SetExistingCharges(new[] { charge });

			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(message, expectedDescription, result.Description);
			AssertEquals(message, expectedAmount, result.PaymentBases.Calculate().amount);
		}

		void AssertCompanyTariffBasedCalculation(
			decimal chargeableAmount = 0,
			decimal chargePerUnitRate = 0,
			decimal chargeBaseRate = 0,
			decimal chargeMinRate = 0,
			decimal chargeMaxRate = 0,
			decimal minimumIncrease = 0,
			decimal baseIncrease = 0,
			decimal percentageIncrease = 0,
			decimal perUnitIncrease = 0,
			decimal perUnitPercentageIncrease = 0,
			string calculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
			decimal expectedAmount = 0,
			string expectedDescription = "",
			string message = default)
		{
			// Company Tariff
			var tariff = CreateTariff(CombinedCalculator.Code);
			var tariffCalculator = tariff.GetCalculator<CombinedCalculator>();

			if (chargePerUnitRate != 0)
			{
				tariffCalculator.PerUnit = chargePerUnitRate;
			}

			if (chargeBaseRate != 0)
			{
				tariffCalculator.BaseRate = chargeBaseRate;
			}

			if (chargeMinRate != 0)
			{
				tariffCalculator.Minimum = chargeMinRate;
			}

			if (chargeMaxRate != 0)
			{
				tariffCalculator.Maximum = chargeMaxRate;
			}

			Factory.Save();

			// Calculator
			Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			TestCalculator.Minimum = minimumIncrease;
			TestCalculator.BaseRate = baseIncrease;
			TestCalculator.Percent = percentageIncrease;
			TestCalculator.PerUnit = perUnitIncrease;
			TestCalculator.PerUnitPercent = perUnitPercentageIncrease;
			TestCalculator.CalculationOrder = calculationOrder;

			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(chargeableAmount, QuantityUnit.KG);

			AssertCalculation(
				parameters: parameters,
				expectedAmount: expectedAmount,
				expectedDescription: expectedDescription,
				message: message);
		}

		public void TestCalculation_CostInDifferentCurrency_ConvertToRateCurrency()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 10, "USD")
					.AddFlatCalculation(500, "USD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.BaseRate = 100;
			calculator.PerUnit = 2;
			calculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;

			// Test
			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"Descriptions should match the formatted expected string",
				"Base Rate AUD 250.00 + Base Rate AUD 100.00 + 100 Kilogram(s) @ AUD 7.00/KG",
				result.Description
			);
			AssertEquals(
				"Calculated amount should match the expected value",
				1050m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculation_PerUnitRateIsZeroAndInDifferentCurrency_ShouldApplyPerUnitChange()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge().AddPerUnitCalculation(100, "KG", 0, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.BaseRate = 100;
			calculator.PerUnit = 2;
			calculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;

			// Test
			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"Result description should match the expected format.",
				"Base Rate AUD 100.00 + 100 Kilogram(s) @ AUD 2.00/KG",
				result.Description
			);

			AssertEquals(
				"Result payment amount should be 300.",
				300m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculation_MultiplePerUnitBases_OrderIncreaseFirst_ShouldApplyPerUnitChangeOncePerChargeable()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 10, "AUD", chargeableReference: "Sugar")
					.AddPerUnitCalculation(100, "KG", 5, "AUD", chargeableReference: "Sugar")
					.AddPerUnitCalculation(100, "KG", 7, "AUD", chargeableReference: "Weed")
					.AddPerUnitCalculation(500, "KG", 3, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnitPercent = 10;
			calculator.PerUnit = 2;
			calculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;

			// Test
			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"Expected description does not match the calculated result description.",
				"100 Kilogram(s) @ AUD 18.70/KG + 100 Kilogram(s) @ AUD 9.90/KG + 500 Kilogram(s) @ AUD 5.50/KG",
				result.Description
			);

			AssertEquals(
				"Expected payment base amount calculation does not match.",
				5610m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculation_MultiplePerUnitBases_OrderPercentageFirst_ShouldApplyPerUnitChangeOncePerChargeable()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 10, "AUD", chargeableReference: "Sugar")
					.AddPerUnitCalculation(100, "KG", 5, "AUD", chargeableReference: "Sugar")
					.AddPerUnitCalculation(100, "KG", 7, "AUD", chargeableReference: "Weed")
					.AddPerUnitCalculation(500, "KG", 3, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnitPercent = 10;
			calculator.PerUnit = 2;
			calculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;

			// Test
			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"The resulting description should match",
				"100 Kilogram(s) @ AUD 18.50/KG + 100 Kilogram(s) @ AUD 9.70/KG + 500 Kilogram(s) @ AUD 5.30/KG",
				result.Description
			);

			AssertEquals(
				"The resulting payment base amount should match",
				5470m,
				result.PaymentBases.Calculate().amount
			);
		}

		public void TestCalculation_MultiplePerUnitBasesInDifferentUnits_FailCalculation()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 10, "AUD")
					.AddPerUnitCalculation(100, "M3", 10, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnitPercent = 10;
			calculator.PerUnit = 2;
			calculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;

			// Test
			var (results, error) = calculator.Calculate(Criteria);
			AssertEquals(0, results.Count());
			AssertEquals("Per Unit Increase can't be applied as related cost is calculated for multiple units", error);
		}

		public void TestCalculationCostBased_SlidingCalculator_ShouldApplyCorrectPerUnitChange()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 5, "AUD")
					.AddFlatCalculation(1000, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.BaseRate = 50;
			calculator["-50"] = (ZDecimal)4m;
			calculator["+50"] = (ZDecimal)3m;
			calculator["+150"] = (ZDecimal)2m;

			// Test
			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"Must use +$3 per unit change",
				"Base Rate AUD 1000.00 + Base Rate AUD 50.00 + 100 Kilogram(s) @ AUD 8.00/KG",
				result.Description
			);
			AssertEquals(1850m, result.PaymentBases.Calculate().amount);
		}

		public void TestCalculationCostBased_CostChargesDontExist_CalculateUsingRelatedCostRates()
		{
			// Cost
			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var costLine = costEntry.AddRateLine(ChargeCode.AC_Code, MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 100m;

			Factory.Save();

			// Calculator
			var clientRate = Entry.RateLines.AddNew();
			clientRate.TL_AC = ChargeCode.PK;
			clientRate.TL_RateDesc = "Test Rate";
			clientRate.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var calculator = clientRate.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calculator.PerUnit = 10;

			// Calculation
			var criteria = CreateCriteria(weightInKG: 20, volumeInM3: 0);
			var result = calculator.Calculate(criteria).results.Single();

			AssertEquals("Must use +10 per unit change", "20 Kilogram(s) @ AUD 110.00/KG", result.Description);
			AssertEquals(2200m, result.PaymentBases.Calculate().amount);
		}

		public void TestCalculationCostBased_CostChargeExistsButHasZeroAmount_CalculateUsingRelatedCostRates()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(amount: 0m)
			});

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var costLine = costEntry.AddRateLine(ChargeCode.AC_Code, MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 100m;

			Factory.Save();

			// Calculator
			var clientRate = Entry.RateLines.AddNew();
			clientRate.TL_AC = ChargeCode.PK;
			clientRate.TL_RateDesc = "Test Rate";
			clientRate.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var calculator = clientRate.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calculator.PerUnit = 10;
			calculator.Minimum = 3000;

			// Calculation
			var criteria = CreateCriteria(weightInKG: 20, volumeInM3: 0);
			criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(amount: 0m)
			});

			var result = calculator.Calculate(criteria).results.Single();
			AssertEquals("Expected description to match the minimum rate message.", "Minimum AUD 3000.00", result.Description);
			AssertEquals("Expected calculated payment amount to be the minimum threshold.", 3000m, result.PaymentBases.Calculate().amount);
		}

		public void TestCalculationCompanyTariff_SlidingCalculator_ShouldApplyCorrectPerUnitChange()
		{
			// Tariff
			var tariff = CreateTariff(CombinedCalculator.Code);
			var tariffCalculator = tariff.GetCalculator<CombinedCalculator>();
			tariffCalculator.BaseRate = 1000;
			tariffCalculator["-50"] = (ZDecimal)10m;
			tariffCalculator["+50"] = (ZDecimal)5m;
			tariffCalculator["+150"] = (ZDecimal)1m;

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			calculator.BaseRate = 50;
			calculator["-50"] = (ZDecimal)4m;
			calculator["+50"] = (ZDecimal)3m;
			calculator["+150"] = (ZDecimal)2m;

			// Test
			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(100m, QuantityUnit.KG);

			var result = calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);

			AssertEquals(
				"Must use +$3 per unit change",
				"Base Rate AUD 1000.00 + Base Rate AUD 50.00 + 100 Kilogram(s) @ AUD 8.00/KG",
				result.Description
			);
			AssertEquals(1850m, autoRateInfo.Amount);
		}

		public void TestChained_UNT_CST_CTB_Calculator()
		{
			// chain1
			var cost = CreateCostCharge().AddPerUnitCalculation(200, "KG", 5, "AUD");

			// chain2
			var tariff = CreateTariff(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariff.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 30m;

			Factory.Save();

			// chain3
			Line.Parent.TI_OH_TransportProvider = TransportProvider.PK;
			Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			TestCalculator.Percent = 50m;
			TestCalculator.BaseRate = 20m;
			TestCalculator.PerUnitPercent = 10m;
			TestCalculator.PerUnit = 4m;
			TestCalculator.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;

			var criteria = CreateCriteria();
			criteria.SetSupplier(TransportProvider);
			criteria.SetExistingCharges(new[] { cost });

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);

			AssertCalculation
			(
				message: "IncreaseFirst: 200*(5*1.3*1.1 + 4) + 20 = 2250",
				parameters: parameters,
				expectedAmount: 2250m,
				expectedDescription: "Base Rate AUD 20.00 + 200 Kilogram(s) @ AUD 11.15/KG"
			);
		}

		public void TestCalculationCostBased_MultipleCostsFound_ApplyChangeToApplicableCosts_ByContainerType()
		{
			// Cost
			var cost1 = CreateCostCharge(container: "20GP").AddPerUnitCalculation(100, "KG", 2, "AUD");
			var cost2 = CreateCostCharge(container: "40GP").AddPerUnitCalculation(200, "KG", 3, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode, containerCode: "40GP");
			calculator.BaseRate = 1000;
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });

			var (results, error) = calculator.Calculate(criteria);

			var calculations = results.Select(r => new
			{
				Charge = r.ChargePK,
				Amount = r.PaymentBases.Calculate().amount
			}).Select(r => $"{r.Charge}|{(int)r.Amount}").ToArray();

			var expectedCalculations = new[]
			{
				$"{cost2.PK}|2000" // $1000 + 200 KG * (Cost $3 + Revenue $2)
			};

			AssertNullOrEmpty(error);

			AssertContainsExactElementsInAnyOrder(
				"The calculations should match the expected results",
				expectedCalculations,
				calculations
			);
		}

		public void TestCalculationCostBased_MultipleCostsFound_ApplyChangeToApplicableCosts_ByContainerClass()
		{
			// Cost
			var fr20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			fr20.RC_HandlingRateClass = "SV20";

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			gp20.RC_HandlingRateClass = "SV20";

			var cost1 = CreateCostCharge(container: "20FR").AddPerUnitCalculation(100, "KG", 2, "AUD");
			var cost2 = CreateCostCharge(container: "40GP").AddPerUnitCalculation(200, "KG", 3, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode, containerCode: "20GP", matchContainerClass: true);
			calculator.BaseRate = 1000;
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });

			var (results, error) = calculator.Calculate(criteria);

			var calculations = results.Select(r => new
			{
				Charge = r.ChargePK,
				Amount = r.PaymentBases.Calculate().amount
			})
			.Select(r => $"{r.Charge}|{(int)r.Amount}")
			.ToArray();

			var expectedCalculations = new[]
			{
				$"{cost1.PK}|1400"
			};

			AssertNullOrEmpty("error", error);

			AssertContainsExactElementsInAnyOrder(
				"The calculated charges should match the expected charges",
				expectedCalculations,
				calculations
			);
		}

		public void TestCalculationCostBased_MultipleCostsFound_ApplyChangeToApplicableCosts_ByContainerNumber()
		{
			// Cost
			var cost1 = CreateCostCharge(containerNumber: "111").AddPerUnitCalculation(100, "KG", 5, "AUD");
			var cost2 = CreateCostCharge(containerNumber: "222").AddPerUnitCalculation(200, "KG", 10, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });
			criteria.RateableMeasures.AddContainerWithNumber(ZGuid.Empty, "222", new MeasureInfo.ContainerInfo());

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, calculator.Line, 0);
			var filteredParams = parameters.CreatedFilteredParametersByContainer_ForTest("222");

			var (results, error) = calculator.Calculate(filteredParams);
			AssertNullOrEmpty(error);

			var actual = results
				.Select(r => $"{r.ChargePK}|{(int)r.PaymentBases.Calculate().amount}")
				.ToArray();

			var expected = new[]
			{
				$"{cost2.PK}|2400"
			};

			AssertContainsExactElementsInAnyOrder(
				"Expected the calculated charges to match with the criteria by container number",
				expected,
				actual
			);
		}

		public void TestCalculationCostBased_MultipleCostsFound_ApplyChangeToApplicableCosts_ByCommodityCode()
		{
			// Cost
			var cost1 = CreateCostCharge(container: "20GP", commodity: "SALT").AddPerUnitCalculation(100, "KG", 2, "AUD");
			var cost2 = CreateCostCharge(container: "20GP", commodity: "ALUM").AddPerUnitCalculation(200, "KG", 3, "AUD");
			var cost3 = CreateCostCharge(container: "40GP", commodity: "ALUM").AddPerUnitCalculation(300, "KG", 4, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode, containerCode: "20GP", commodityCode: "ALUM");
			calculator.BaseRate = 1000;
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });

			var (results, error) = calculator.Calculate(criteria);

			var calculations = results.Select(r => $"{r.ChargePK}|{(int)r.PaymentBases.Calculate().amount}").ToArray();

			var expectedCalculations = new[]
			{
				$"{cost2.PK}|2000" // $1000 + 200 KG * (Cost $3 + Revenue $2)
			};

			AssertNullOrEmpty(error);
			AssertContainsExactElementsInAnyOrder("The calculations should match the expected results.", expectedCalculations, calculations);
		}

		public void TestCalculationCostBased_NoneOfFoundCostsIsApplicable_ReturnError()
		{
			// Cost
			var cost1 = CreateCostCharge(container: "20GP", commodity: "SALT").AddPerUnitCalculation(100, "KG", 2, "AUD");
			var cost2 = CreateCostCharge(container: "20GP", commodity: "ALUM").AddPerUnitCalculation(200, "KG", 3, "AUD");
			var cost3 = CreateCostCharge(container: "40GP", commodity: "ALUM").AddPerUnitCalculation(300, "KG", 4, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode, containerCode: "40GP", commodityCode: "SALT");
			calculator.BaseRate = 1000;
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });

			var (results, error) = calculator.Calculate(criteria);

			AssertEquals(
				"charge code is using the Cost Based Calculator, however there are conflicting or no costing rates found.",
				error
			);

			AssertEquals(0, results.Count());
		}

		public void TestCalculationCostBased_MultipleApplicableCostsFound_ApplyChangeToEachCost()
		{
			// Cost
			var cost1 = CreateCostCharge().AddPerUnitCalculation(100, "KG", 2, "AUD");
			var cost2 = CreateCostCharge().AddPerUnitCalculation(200, "KG", 3, "AUD");

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.BaseRate = 1000;
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost1, cost2 });

			var (results, error) = calculator.Calculate(criteria);

			var calculations = results.Select(r => new
			{
				Charge = r.ChargePK,
				Amount = r.PaymentBases.Calculate().amount
			})
			.Select(c => $"{c.Charge}|{(int)c.Amount}")
			.ToArray();

			var expectedCalculations = new[]
			{
				$"{cost1.PK}|1400",	// $1000 + 100 KG * (Cost $2 + Revenue $2)
				$"{cost2.PK}|2000"	// $1000 + 200 KG * (Cost $3 + Revenue $2)
			};

			AssertNullOrEmpty(error);
			AssertContainsExactElementsInAnyOrder(expectedCalculations, calculations);
		}

		public void TestCalculationCostBased_CostWithPerUnitRateWithMultiplierFound_ApplyChangeConsideringMultiplier()
		{
			// Cost
			var cost = CreateCostCharge().AddPerUnitCalculation(1000, "KG", 2, "AUD", unitMultiplier: 100m);

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnit = 100;

			// Test
			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { cost });

			var (results, error) = calculator.Calculate(criteria);

			var calculations = results.Select(r => $"{r.ChargePK}|{(int)r.PaymentBases.Calculate().amount}").ToArray();
			var expectedCalculations = new[]
			{
				$"{cost.PK}|3000"
			};

			AssertNullOrEmpty(error);
			AssertContainsExactElementsInAnyOrder(
				"Cost $200 + Revenue $100 @ 100 KG",
				expectedCalculations,
				calculations
			);
		}

		public void TestGetUnits_ExistingChargesExist_ReturnUnitsFromExistingCharges()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge().AddPerUnitCalculation(100, "KG", 10, "AUD"),
				CreateCostCharge().AddPerUnitCalculation(10, "M3", 100, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnit = 2;

			// Test
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, new FreightAutoRater(new RatingContext()));
			var units = calculator.GetUnits(parameters);

			var expectedUnits = new ZString[] { "KG", "M3" };
			AssertContainsExactElementsInAnyOrder("Units should match the expected collection", expectedUnits, units);
		}

		public void TestGetUnits_ExistingChargeExistsWithDifferentUnits_ReturnEmptyCollection()
		{
			// Cost
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge()
					.AddPerUnitCalculation(100, "KG", 10, "AUD")
					.AddPerUnitCalculation(10, "LB", 100, "AUD"),
				CreateCostCharge().AddPerUnitCalculation(10, "M3", 100, "AUD")
			});

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.PerUnit = 2;

			// Test
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, new FreightAutoRater(new RatingContext()));
			var units = calculator.GetUnits(parameters);

			var expectedUnits = new ZString[] { "M3" };
			AssertContainsExactElementsInAnyOrder(
				"The first charge is calculated for KG and LB and CST calculator can't be applied to a charge calculated for different units",
				expectedUnits,
				units
			);
		}

		public void TestGetUnits_ExistingTariffExists_ReturnUnitsFromExistingTariff()
		{
			// Cost
			var tariff = CreateTariff(UnitCalculator.Code);
			tariff.TL_WeightVolume = "KG";

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			calculator.PerUnit = 2;

			// Test
			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(100, QuantityUnit.KG);

			var units = calculator.GetUnits(parameters);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "KG" }, units);
		}

		public void TestCalculationCostBased_ExistingRevenueChargeFound_CurrencyCodeForCalculationFallbackToLocalCode()
		{
			var charge = CreateCostCharge(amount: 100, currency: ZString.Empty);
			charge.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;

			Factory.Save();

			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			calculator.Percent = 10;

			Criteria.SetExistingCharges(new[] { charge });

			AssertEquals(
				"Precondition: Revenue Charge should not have Cost currency.",
				ZString.Empty,
				charge.JR_RX_NKCostCurrency
			);
			AssertEquals(
				"Precondition: there should be current company local currency.",
				Constants.CurrencyCodes.Australia,
				Env.CurrentCompany.LocalCurrency.Code
			);

			var result = calculator.Calculate(Criteria).results.Single();

			AssertEquals(
				"Cost currency code should fallback to use local code. No exception should occur and stop calculation.",
				"AUD",
				result.Currency.Code
			);
			AssertEquals(
				"110.00% of (Base Rate AUD 100.00)",
				result.Description
			);
			AssertEquals(
				"110% of 100 should still be 110",
				110m,
				result.PaymentBases.Single().Amount
			);
		}

		#region Apply inrease to Accpeted cost

		public void TestCalculation_TariffHasAcceptedCost_ApplyIncreaseToTheAcceptedCost()
		{
			var cost = CreateCost("FLT");
			((RateEntry)cost.ParentRateEntry).TI_OriginLRC = "USNYC";
			cost.GetCalculator<FlatCalculator>().BaseRate = 500;

			// Calculator
			Entry.TI_OriginLRC = "USLAX"; // Make sure it doesn't match cost by attributes. It should match because it was accepted, even though attributes don't match.
			var line = Entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateDesc = "Test Rate";
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var calculator = line.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calculator.BaseRate = 100;

			AssertEquals("No cost matched", 0, calculator.Calculate(Criteria).results.Count());

			calculator.ApplyToLine = cost.PK.ToString();
			var result = calculator.Calculate(Criteria).results.Single();
			AssertEquals("Accepted cost matched", 600m, result.PaymentBases.Calculate().amount);
		}

		#endregion

		#region Chain CTB CST Unit Calculator

		public void TestChained_UNT_CST_CTB_Calculator_PER_PER()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = 8 * 1.3 + 40 = 50.4
			//	Base price = 0 * 1.1 + 20 = 20
			//Chain3 - CTB calculator:
			//	Unit Price = 50.4 * 1.8 + 90 = 180.72
			//	Base price = 20 * 1.6 + 70 = 102
			//200 * 180.72 + 102 = 36246m
			AssertChained_UNT_CST_CTB_Calculator
			(
				"GIVEN chain calculators: UNT > CST(PER) > CTB(PER)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				expectedAmount: 36246m,
				expectedDescription: "Base Rate AUD 70.00 + 200 Kilogram(s) @ AUD 180.72/KG + 160.00% of (Base Rate AUD 20.00)"
			);
		}

		public void TestChained_UNT_CST_CTB_Calculator_FIX_FIX()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = (8 + 40)* 1.3 = 62.4
			//	Base price = 20 * 1.1 = 22
			//Chain3 - CTB calculator:
			//	Unit Price = (62.4+90) * 1.8 = 274.32
			//	Base price = (22+70) * 1.6 = 147.2
			//200 * 274.32 + 147.2 = 55011.2
			AssertChained_UNT_CST_CTB_Calculator
			(
				"GIVEN chain calculators: UNT > CST(FIX) > CTB(FIX)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				expectedAmount: 55011.2m,
				expectedDescription: "Base Rate AUD 112.00 + 200 Kilogram(s) @ AUD 274.32/KG + 160.00% of (Base Rate AUD 22.00)"
			);
		}

		public void TestChained_UNT_CST_CTB_Calculator_FIX_PER()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = (8 + 40)* 1.3 = 62.4
			//	Base price = 20 * 1.1 = 22
			//Chain3 - CTB calculator:
			//	Unit Price = 62.4*1.8 + 90 = 202.32
			//	Base price = 22*1.6 + 70 = 105.2
			//200 * 202.32 + 105.2 = 40569.2
			AssertChained_UNT_CST_CTB_Calculator
			(
				"GIVEN chain calculators: UNT > CST(FIX) > CTB(PER)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				expectedAmount: 40569.2m,
				expectedDescription: "Base Rate AUD 70.00 + 200 Kilogram(s) @ AUD 202.32/KG + 160.00% of (Base Rate AUD 22.00)"
			);
		}

		public void TestChained_UNT_CST_CTB_Calculator_PER_FIX()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = 8 * 1.3 + 40 = 50.4
			//	Base price = 0 * 1.1 + 20 = 20
			//Chain3 - CTB calculator:
			//	Unit Price = (50.4+90) * 1.8 = 252.72
			//	Base price = (20+70) * 1.6 = 144
			//200 * 252.72 + 144 = 50688
			AssertChained_UNT_CST_CTB_Calculator
			(
				"GIVEN chain calculators: UNT > CST(PER) > CTB(FIX)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				expectedAmount: 50688m,
				expectedDescription: "Base Rate AUD 112.00 + 200 Kilogram(s) @ AUD 252.72/KG + 160.00% of (Base Rate AUD 20.00)"
			);
		}

		void AssertChained_UNT_CST_CTB_Calculator(string message, string calculationOrder1, string calculationOrder2, decimal expectedAmount, string expectedDescription)
		{
			// chain1
			var costing = CreateCostCharge().AddPerUnitCalculation(200, "KG", 8, "AUD");

			// chain2
			var tariff = CreateTariff(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var tariffCalculator = tariff.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			tariffCalculator.Percent = 10m;
			tariffCalculator.BaseRate = 20m;
			tariffCalculator.PerUnitPercent = 30m;
			tariffCalculator.PerUnit = 40m;
			tariffCalculator.CalculationOrder = calculationOrder1;

			Factory.Save();

			// chain3
			Line.Parent.TI_OH_TransportProvider = TransportProvider.PK;
			Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			TestCalculator.Percent = 60m;
			TestCalculator.BaseRate = 70m;
			TestCalculator.PerUnitPercent = 80m;
			TestCalculator.PerUnit = 90m;
			TestCalculator.CalculationOrder = calculationOrder2;

			var criteria = CreateCriteria();
			criteria.SetSupplier(TransportProvider);
			criteria.SetExistingCharges(new[] { costing });

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);

			AssertCalculation
			(
				parameters,
				expectedAmount,
				expectedDescription,
				message
			);
		}

		#endregion

		#region Chained CMB CST CTB Calculator

		public void TestChained_CMB_CST_CTB_Calculator_PER_PER()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = 8 * 1.3 + 40 = 50.4
			//	Base price = 100 * 1.1 + 20 = 130
			//Chain3 - CTB calculator:
			//	Unit Price = 50.4 * 1.8 + 90 = 180.72
			//	Base price = 130 * 1.6 + 70 = 278
			//200 * 180.72 + 278 = 36422
			AssertChained_CMB_CST_CTB_Calculator
			(
				"GIVEN chain calculators: CMB > CST(PER) > CTB(PER)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				expectedAmount: 36422m,
				expectedDescription: "Base Rate AUD 70.00 + 200 Kilogram(s) @ AUD 180.72/KG + 160.00% of (110.00% of (Base Rate AUD 100.00)) + 160.00% of (Base Rate AUD 20.00)"
			);
		}

		public void TestChained_CMB_CST_CTB_Calculator_FIX_FIX()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = (8 + 40)* 1.3 = 62.4
			//	Base price = (20+100) * 1.1 = 132
			//Chain3 - CTB calculator:
			//	Unit Price = (62.4+90) * 1.8 = 274.32
			//	Base price = (132+70) * 1.6 = 323.2
			//200 * 274.32 + 323.2 = 55187.2
			AssertChained_CMB_CST_CTB_Calculator
			(
				"GIVEN chain calculators: CMB > CST(FIX) > CTB(FIX)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				expectedAmount: 55187.2m,
				expectedDescription: "Base Rate AUD 112.00 + 200 Kilogram(s) @ AUD 274.32/KG + 160.00% of (110.00% of (Base Rate AUD 100.00)) + 160.00% of (Base Rate AUD 22.00)"
			);
		}

		public void TestChained_CMB_CST_CTB_Calculator_FIX_PER()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = (8 + 40)* 1.3 = 62.4
			//	Base price = (20+100) * 1.1 = 132
			//Chain3 - CTB calculator:
			//	Unit Price = 62.4*1.8 + 90 = 202.32
			//	Base price = 132*1.6 + 70 = 281.2
			//200 * 202.32 + 281.2 = 40745.2
			AssertChained_CMB_CST_CTB_Calculator
			(
				"GIVEN chain calculators: CMB > CST(FIX) > CTB(PER)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				expectedAmount: 40745.2m,
				expectedDescription: "Base Rate AUD 70.00 + 200 Kilogram(s) @ AUD 202.32/KG + 160.00% of (110.00% of (Base Rate AUD 100.00)) + 160.00% of (Base Rate AUD 22.00)"
			);
		}

		public void TestChained_CMB_CST_CTB_Calculator_PER_FIX()
		{
			//Chain1 - UNIT Calculator: 8
			//Chain2 - CST Calculator:
			//	Unit Price = 8 * 1.3 + 40 = 50.4
			//	Base price = 100 * 1.1 + 20 = 130
			//Chain3 - CTB calculator:
			//	Unit Price = (50.4+90) * 1.8 = 252.72
			//	Base price = (130+70) * 1.6 = 320
			//200 * 252.72 + 320 = 50864
			AssertChained_CMB_CST_CTB_Calculator
			(
				"GIVEN chain calculators: CMB > CST(PER) > CTB(FIX)",
				calculationOrder1: CompanyTariffOrCostBasedCalculator.Items.PercentFirst,
				calculationOrder2: CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst,
				expectedAmount: 50864m,
				expectedDescription: "Base Rate AUD 112.00 + 200 Kilogram(s) @ AUD 252.72/KG + 160.00% of (110.00% of (Base Rate AUD 100.00)) + 160.00% of (Base Rate AUD 20.00)"
			);
		}

		void AssertChained_CMB_CST_CTB_Calculator(string message, string calculationOrder1, string calculationOrder2, decimal expectedAmount, string expectedDescription)
		{
			// chain1
			var charge = CreateCostCharge()
				.AddPerUnitCalculation(200, "KG", 8, "AUD")
				.AddFlatCalculation(100, "AUD");

			// chain2
			var tariff = CreateTariff(CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var tariffCalculator = tariff.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			tariffCalculator.Percent = 10m;
			tariffCalculator.BaseRate = 20m;
			tariffCalculator.PerUnitPercent = 30m;
			tariffCalculator.PerUnit = 40m;
			tariffCalculator.CalculationOrder = calculationOrder1;

			Factory.Save();

			// chain3
			Line.Parent.TI_OH_TransportProvider = TransportProvider.PK;
			Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			TestCalculator.Percent = 60m;
			TestCalculator.BaseRate = 70m;
			TestCalculator.PerUnitPercent = 80m;
			TestCalculator.PerUnit = 90m;
			TestCalculator.CalculationOrder = calculationOrder2;

			var criteria = CreateCriteria();
			criteria.SetSupplier(TransportProvider);
			criteria.SetExistingCharges(new[] { charge });

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));
			parameters.ChargeableAmount = new Quantity(200m, QuantityUnit.KG);

			AssertCalculation
			(
				parameters,
				expectedAmount,
				expectedDescription,
				message
			);
		}

		#endregion

		#endregion

		public void TestCalculation_CompanyTariffBased_WithFRTCalculatorAndUnits()
		{
			CreateTariff(FreightInclusiveCalculator.Code);

			Factory.Save();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Line.Parent.Parent.TH_OH = NewClient.PK;
			TestCalculator.BaseRate = 5m;
			TestCalculator.Percent = 10m;
			TestCalculator.PerUnit = 2m;

			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));

			AssertCalculation(parameters, "charge code is using the Company Tariff Based Calculator, however there are conflicting or no tariff rates found.");
		}

		public void TestCalculation_AgentRates()
		{
			fCalculatorCode = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var charge = CreateCostCharge().AddFlatCalculation(100, "AUD");
			charge.JR_AgentDeclaredCostAmt = 200m;

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var costRateLine = costEntry.RateLines.AddNew();
			costRateLine.TL_AC = ChargeCode.PK;
			costRateLine.TL_RateCalculator = FlatCalculator.Code;

			((FlatCalculator)costRateLine.Calculator).BaseRate = 100m;
			costRateLine.ViewAgentRates = true;
			((FlatCalculator)costRateLine.Calculator).BaseRate = 200m;
			costRateLine.ViewAgentRates = false;

			Factory.Save();

			TestCalculator.Percent = 10m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Percent = 10m;
			TestCalculator.Line.ViewAgentRates = false;

			var criteria = CreateCriteria();
			criteria.SetExistingCharges(new[] { charge });

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			AssertCalculation(parameters, 110m, "110.00% of (Base Rate AUD 100.00)");

			Line.ViewAgentRates = true;
			AssertCalculation(parameters, 220m, "110.00% of (Base Rate AUD 200.00)");
		}

		public void TestCalculation_PercentageCalculatorBased()
		{
			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "CCC2";
			chargeCode2.AC_ChargeGroup = "ORG";

			fCalculatorCode = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var tariffCalculator = CreateTariff(PercentageCalculator.Code).GetCalculator<PercentageCalculator>();
			tariffCalculator.Percent = 5m;
			tariffCalculator.Minimum = 50m;
			tariffCalculator.Maximum = 1000m;
			tariffCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = chargeCode2.PK;

			Factory.Save();

			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Line.Parent.Parent.TH_OH = NewClient.PK;
			TestCalculator.BaseRate = 10m;
			TestCalculator.Percent = 20m;
			////TestCalculator.CalculationOrder = Calculator.Items.PercentFirst;

			var criteria = CreateCriteria();
			var @params = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			var autoRateInfos = @params.Results;
			autoRateInfos.AddNew(chargeCode2, "AUD", 1500M);
			AssertCalculation(@params, 100m, "Base Rate AUD 10.00 + 120.00% of (5.00% of (AUD 1500.00 (CCC2)))", "Base Rate AUD 10.00 + 6.00% of (AUD 1500.00 (CCC2))");
			@params.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfos);

			autoRateInfos.Clear();
			autoRateInfos.AddNew(chargeCode2, "AUD", 100M);
			AssertCalculation(@params, 60m, "Minimum AUD 60.00", "Minimum AUD 60.00");
			@params.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfos);

			autoRateInfos.Clear();
			autoRateInfos.AddNew(chargeCode2, "AUD", 30000M);
			AssertCalculation(@params, 1200m, "Maximum AUD 1200.00", "Maximum AUD 1200.00");
			@params.RatingContext.PercentageLinesApplied.Clear();
			ClearUsedByRateLines(autoRateInfos);

			tariffCalculator.IsPartThereof = true;
			tariffCalculator.ValueOrPartThereOf = 1000m;
			tariffCalculator.Rate = 10m;

			Factory.Save();
			@params = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			@params.Results.AddRange(autoRateInfos);
			AssertCalculation(
				@params,
				370m,
				"Base Rate AUD 10.00 + 120.00% of (30 AUD 1000.00 @ AUD 10.00/AUD 1000.00 (per AUD 1000.00 or part thereof for AUD 30000.00 (CCC2)))",
				"Base Rate AUD 10.00 + 30 AUD 1000.00 @ AUD 12.00/AUD 1000.00 (per AUD 1000.00 or part thereof for AUD 30000.00 (CCC2))");
		}

		public void TestCompanyTariffDiscountOnLoading()
		{
			var calculator = CreateTariff(UnitCalculator.Code).GetCalculator<UnitCalculator>();
			calculator.PerUnit = 100;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();

			AssertEquals("Pre-condition - 1 inherited rate entry", 1, tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);

			var tariff2Entry = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			AssertEquals("Tariff line marked as inherited", true, tariff2Entry.RateLines[0].IsTariffLineInherited);

			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 30M);

			AssertEquals("No Origin discount reflected in tariff 2 charge", (ZDecimal)100M, tariff2Entry.RateLines[0].RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValue);

			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20M);

			AssertEquals("Discount reflected in tariff 2 charge", (ZDecimal)80M, tariff2Entry.RateLines[0].RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValue);

			factory2.Save();

			AssertEquals("Discounted charge NOT saved to database", 100M, calculator.PerUnit);

			tariff2Entry.RateLines.OverrideTariffLines(new[] { tariff2Entry.RateLines[0] });
			AssertEquals("Tariff line NOT marked as inherited", false, tariff2Entry.RateLines[0].IsTariffLineInherited);

			AssertEquals("Tariff line using company tariff calc", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, tariff2Entry.RateLines[0].TL_RateCalculator);
			AssertEquals("Overidden tariff line set to use same discount as set on header - percent", -20M, tariff2Entry.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent);
			AssertEquals("Overidden tariff line set to use same discount as set on header - perUnitPercent", -20M, tariff2Entry.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent);
		}

		#region Validate Calculation Order

		public void TestValidateCalculationOrder()
		{
			TestCalculator.BaseRate = 100m;
			TestCalculator.Minimum = 20m;
			TestCalculator.PerUnit = 30m;

			TestCalculator.Percent = 0m;
			TestCalculator.CalculationOrder = ZString.Empty;

			var calculationOrderInfo = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder).TM_TextInfo;
			Assert("Percentage is zero.", !calculationOrderInfo.HasErrors());

			TestCalculator.Percent = 20m;
			TestCalculator.CalculationOrder = ZString.Empty;
			AssertEquals("Apply a percentage and fixed change without calculation order.", ErrorMessages.CalculationOrderShouldBeSetWhenPercentageAndFixedChangeAreSet, calculationOrderInfo.GetErrors().GetFirstMessage());

			TestCalculator.CalculationOrder = "PER";
			Assert("Apply a percentage and fixed change with calculation order.", !calculationOrderInfo.HasErrors());

			TestCalculator.CalculationOrder = "FIX";
			Assert("Apply a percentage and fixed changes with calculation order.", !calculationOrderInfo.HasErrors());

			TestCalculator.CalculationOrder = "XXX";
			AssertEquals("Enter a valid selection.", calculationOrderInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidateCalculationOrder_GivenPercentAndBaseRateAreSetButCalculationOrderIsEmpty_ThenShouldError()
		{
			// Percentage=0, baseRate=0, perUnit=0, calculationOrder=""
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(percent: 10m);
			var calculationOrderInfo = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder).TM_TextInfo;
			var baseRateInfo = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_ValueInfo;
			var percentInfo = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_ValueInfo;
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(baseRate: 10m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=10, baseRate=0, calculationOrder=""
			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(percent: 0m);
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(baseRate: 10m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");

			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=0, baseRate=10, calculationOrder=""
			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(percent: 10m);
			AssertCalculationOrderTest(percentInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(baseRate: 0m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=0, baseRate=0, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(percent: 10m);
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(baseRate: 10m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=10, baseRate=0, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(percent: 0m);
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(baseRate: 10m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 10m, baseRate: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=0, baseRate=10, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(percent: 10m);
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(baseRate: 0m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// Percentage=10, baseRate=10, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 10m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(percent: 0m);
			AssertCalculationOrderTest(percentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 10m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(baseRate: 0m);
			AssertCalculationOrderTest(baseRateInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 10m, baseRate: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");
		}

		public void TestValidateCalculationOrder_GivenPerUnitPercentAndPerUnitAreSetButCalculationOrderIsEmpty_ThenShouldError()
		{
			// Percentage=0, baseRate=0, perUnitPercent: 0, perUnit=0, calculationOrder=""
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnitPercent: 10m);
			var calculationOrderInfo = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder).TM_TextInfo;
			var baseRateInfo = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_ValueInfo;
			var perUnitPercentInfo = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU).TM_ValueInfo;
			var perUnitInfo = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_ValueInfo;
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnit: 10m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=10, perUnit=0, calculationOrder=""
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnitPercent: 0m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnit: 10m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=0, perUnit=10, calculationOrder=""
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnitPercent: 10m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(perUnit: 0m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: string.Empty);
			UpdateCalculationOrderTestParameters(calculationOrder: Calculator.Items.IncreaseFirst);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=0, perUnit=0, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnitPercent: 10m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnit: 10m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=10, perUnit=0, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnitPercent: 0m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnit: 10m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 0m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=0, perUnit=10, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnitPercent: 10m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnit: 0m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 0m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: null);

			// perUnitPercent=10, perUnit=10, calculationOrder="FIX"
			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnitPercent: 0m);
			AssertCalculationOrderTest(perUnitPercentInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(perUnit: 0m);
			AssertCalculationOrderTest(perUnitInfo, expectedError: null);

			SetupCalculationOrderTest(percent: 0m, baseRate: 0m, perUnitPercent: 10m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");
		}

		public void TestValidateCalculationOrder_PercentAndBaseRateAndPerUnit()
		{
			SetupCalculationOrderTest(percent: 10m, baseRate: 10m, perUnit: 10m, calculationOrder: Calculator.Items.IncreaseFirst);
			UpdateCalculationOrderTestParameters(calculationOrder: string.Empty);
			var calculationOrderInfo = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder).TM_TextInfo;
			AssertCalculationOrderTest(calculationOrderInfo, expectedError: "Calculation Order should be set when both Percentage and Fixed Change are set.");
		}

		void SetupCalculationOrderTest(decimal percent, decimal baseRate, decimal perUnit, decimal perUnitPercent = 0m, string calculationOrder = default(string))
		{
			TestCalculator.Percent = percent;
			TestCalculator.BaseRate = baseRate;
			TestCalculator.PerUnit = perUnit;
			TestCalculator.PerUnitPercent = perUnitPercent;
			TestCalculator.CalculationOrder = calculationOrder;

			var percentLineItem = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER);
			var baseRateLineItem = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			var perUnitLineItem = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			var perUnitPercentLineItem = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU);
			var calculationOrderLineItem = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder);

			percentLineItem.Validation.ValidateAll();
			baseRateLineItem.Validation.ValidateAll();
			perUnitLineItem.Validation.ValidateAll();
			perUnitPercentLineItem.Validation.ValidateAll();
			calculationOrderLineItem.Validation.ValidateAll();

			var percentInfo = percentLineItem.TM_ValueInfo;
			var baseRateInfo = baseRateLineItem.TM_ValueInfo;
			var perUnitInfo = perUnitLineItem.TM_ValueInfo;
			var perUnitPercentInfo = perUnitPercentLineItem.TM_ValueInfo;
			var calculationOrderInfo = calculationOrderLineItem.TM_TextInfo;

			CombineAssertions($"Precondition for test: Percent='{TestCalculator.Percent}', BaseRate='{TestCalculator.BaseRate}', PerUnit='{TestCalculator.PerUnit}',  PerUnitPercent='{TestCalculator.PerUnitPercent}', calculationOrder='{TestCalculator.CalculationOrder}", () =>
			{
				AssertNoErrors("percent", percentInfo);
				AssertNoErrors("baseRate", baseRateInfo);
				AssertNoErrors("perUnit", perUnitInfo);
				AssertNoErrors("perUnitPercent", perUnitPercentInfo);
				AssertNoErrors("CalculationOrder", calculationOrderInfo);
			});
		}

		void UpdateCalculationOrderTestParameters(decimal? percent = null, decimal? baseRate = null, decimal? perUnit = null, decimal? perUnitPercent = null, string calculationOrder = null)
		{
			if (percent != null)
			{
				TestCalculator.Percent = percent.Value;
			}

			if (baseRate != null)
			{
				TestCalculator.BaseRate = baseRate.Value;
			}

			if (perUnit != null)
			{
				TestCalculator.PerUnit = perUnit.Value;
			}

			if (perUnitPercent != null)
			{
				TestCalculator.PerUnitPercent = perUnitPercent.Value;
			}

			if (calculationOrder != null)
			{
				TestCalculator.CalculationOrder = calculationOrder;
			}
		}

		void AssertCalculationOrderTest(ZPropertyInfo properyInfo, string expectedError)
		{
			var message = $"WHEN modifying {((RateLineItem)properyInfo.BizObj).TM_Type} to: Percent='{TestCalculator.Percent}', BaseRate='{TestCalculator.BaseRate}', PerUnit='{TestCalculator.PerUnit}', calculationOrder='{TestCalculator.CalculationOrder}'";
			if (expectedError != null)
			{
				AssertHasError(message, properyInfo, expectedError);
			}
			else
			{
				AssertNoErrors(message, properyInfo);
			}
		}

		public void TestValidateCalculationOrder_GivenInvalidOldData_ThenShouldNotError()
		{
			var calculator = TestCalculator; // to initialize, otherwise NullReferenceException on next line

			var percentLineItem = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER);
			var baseRateLineItem = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			var perUnitPercentLineItem = Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU);
			var perUnitLineItem = Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			var calculationOrderLineItem = Line.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder);

			using (percentLineItem.GetValidationSuspender())
			using (baseRateLineItem.GetValidationSuspender())
			using (perUnitPercentLineItem.GetValidationSuspender())
			using (perUnitLineItem.GetValidationSuspender())
			using (calculationOrderLineItem.GetValidationSuspender())
			{
				SetupCalculationOrderTest(percent: 10m, baseRate: 10m, perUnit: 0m, calculationOrder: string.Empty);
			}

			Factory.Save();

			var percentInfo = percentLineItem.TM_ValueInfo;
			var baseRateInfo = baseRateLineItem.TM_ValueInfo;
			var perUnitPercentInfo = perUnitPercentLineItem.TM_ValueInfo;
			var perUnitInfo = perUnitLineItem.TM_ValueInfo;
			var calculationOrderInfo = calculationOrderLineItem.TM_TextInfo;

			percentLineItem.Validation.ValidateAll();
			baseRateLineItem.Validation.ValidateAll();
			perUnitPercentLineItem.Validation.ValidateAll();
			perUnitLineItem.Validation.ValidateAll();
			calculationOrderLineItem.Validation.ValidateAll();

			AssertNoErrors("percent", percentInfo);
			AssertNoErrors("baseRate", baseRateInfo);
			AssertNoErrors("perUnitPercent", perUnitPercentInfo);
			AssertNoErrors("perUnit", perUnitInfo);
			AssertNoErrors("calculationOrder", calculationOrderInfo);
		}

		#endregion

		public void TestValidateTM_Type()
		{
			var oldUntItem = TestCalculator.FindRateLineItem(Calculator.Items.Operator.UNT);

			var minusItem = TestCalculator.RateLineItems.AddNew();
			minusItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertHasError(minusItem.TM_TypeInfo, ErrorMessages.MoreLinesRequired);

			var plusItem = TestCalculator.RateLineItems.AddNew();
			plusItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertNoErrors(plusItem.TM_TypeInfo);

			minusItem.Validation.ValidateTM_Type();
			AssertNoErrors(minusItem.TM_TypeInfo);

			var untItem = TestCalculator.RateLineItems.AddNew();
			untItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(untItem.TM_TypeInfo, ErrorMessages.UNTNotAllowed);
			AssertNoErrors(oldUntItem.TM_TypeInfo);
		}

		public void TestValidateCostMarkUp()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Env.Registry.Rating.MinimumMarkUpPercentages = "AIO|USLAX|16|0|0;AIR|USLAX|0|50|1.5;AIR|GBLON|15|0|0";

			fCalculatorCode = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.Parent.TI_DestinationLRC = "USLAX";
			Line.Parent.TI_RateCategory = "AIR";

			InitialiseTestCalculator();
			Line.RunPreSaveValidation();
			AssertHasWarning(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo, "The minimum cost markup of 16% has not been met.");
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo);

			TestCalculator.Percent = 16m;
			Line.RunPreSaveValidation();
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo);

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			Line.RunPreSaveValidation();
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo);
			AssertHasWarning(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo, "The minimum cost markup has not been met.");
			AssertHasWarning(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, "The minimum cost markup has not been met.");

			Line.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			Line.RunPreSaveValidation();
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo);
			AssertHasWarning(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, "The minimum cost markup has not been met.");

			Line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;
			Line.RunPreSaveValidation();
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo);

			Line.Parent.TI_DestinationLRC = "GBLON";
			Line.RunPreSaveValidation();
			AssertHasWarning(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo, "The minimum cost markup of 15% has not been met.");
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo);
			AssertNoWarnings(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo);
		}

		#region Validate Equipment Type

		public void TestValidateEquipmentType_WithoutCompanyTariff()
		{
			var cartage = Factory.New<AccChargeCode>();
			cartage.AC_RateCalculator = CartageCalculator.Code;

			var testRate = Factory.New<ClientRate>();
			var oRGEntry = testRate.AddRateEntry("ORG", "FCL", "AUSYD", "");

			var companyTariffOrCostBasedLine1 = oRGEntry.RateLines.AddNew();
			companyTariffOrCostBasedLine1.TL_AC = cartage.PK;
			companyTariffOrCostBasedLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var calculator1 = (CompanyTariffOrCostBasedCalculator)companyTariffOrCostBasedLine1.Calculator;
			AssertNullOrEmpty(calculator1.EquipmentType);
			AssertNoErrors(calculator1.String2Info);

			var companyTariffOrCostBasedLine2 = oRGEntry.RateLines.AddNew();
			companyTariffOrCostBasedLine2.TL_AC = cartage.PK;
			companyTariffOrCostBasedLine2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			var calculator2 = (CompanyTariffOrCostBasedCalculator)companyTariffOrCostBasedLine2.Calculator;
			AssertNullOrEmpty(calculator2.EquipmentType);
			AssertNoErrors(calculator2.String2Info);
		}

		public void TestValidateEquipmentType_WithCompanyTariff()
		{
			var cartage = Factory.New<AccChargeCode>();
			cartage.AC_RateCalculator = CartageCalculator.Code;

			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "", removeLines: true);
			AddCartageRateLine(companyTariffRateEntry, cartage.AC_Code, QuantityUnit.CN, 10m);

			companyTariff.Factory.Save();

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costingRateEntry = costing.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "", removeLines: true);
			AddCartageRateLine(costingRateEntry, cartage.AC_Code, QuantityUnit.CN, 10m);

			Factory.Save();

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var oRGEntry = testRate.AddRateEntry("ORG", "FCL", "AUSYD", "");

			var companyTariffOrCostBasedLine1 = oRGEntry.RateLines.AddNew();
			companyTariffOrCostBasedLine1.TL_AC = cartage.PK;
			companyTariffOrCostBasedLine1.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var calculator1 = (CompanyTariffOrCostBasedCalculator)companyTariffOrCostBasedLine1.Calculator;
			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;

			var companyTariffOrCostBasedLine2 = oRGEntry.RateLines.AddNew();
			companyTariffOrCostBasedLine2.TL_AC = cartage.PK;
			companyTariffOrCostBasedLine2.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			var calculator2 = (CompanyTariffOrCostBasedCalculator)companyTariffOrCostBasedLine2.Calculator;
			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertHasError(calculator2.String2Info, "You have already entered this charge with this equipment type. Please choose another equipment type or charge code.");

			calculator2.EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;
			AssertNoErrors(calculator2.String2Info);

			calculator1.EquipmentType = "";
			AssertNoErrors(calculator1.String2Info);

			calculator2.EquipmentType = "";
			AssertNoErrors(calculator2.String2Info);

			calculator1.EquipmentType = Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertNoErrors(calculator1.String2Info);

			calculator2.EquipmentType = "SDL";
			AssertNoErrors(calculator2.String2Info);

			calculator1.EquipmentType = "ZZZ";
			AssertHasError(calculator1.String2Info, "Enter a valid selection.");

			companyTariffRateEntry.TI_Mode = Core.Constants.RateMode.AIR;
			companyTariff.Factory.Save();
			costingRateEntry.TI_Mode = Core.Constants.RateMode.AIR;
			Factory.Save();

			oRGEntry.TI_Mode = Core.Constants.RateMode.AIR;

			calculator1.EquipmentType = "";
			AssertNoErrors(calculator1.String2Info);
		}

		static RateLine AddCartageRateLine(RateEntry rateEntry, string chargeCode, string lineUnit, decimal perUnit)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageCalculator.Code, lineUnit);
			rateLine.GetCalculator<CartageCalculator>().PerUnit = perUnit;

			return rateLine;
		}

		#endregion

		public void TestShowMessageTypeSubType()
		{
			Assert(!TestCalculator.ShowMessageTypeSubType);

			Line.ChargeCode.AC_RateCalculator = AgencyCalculator.Code;
			Assert(TestCalculator.ShowMessageTypeSubType);

			Line.IsBulkRateUpdateActionLine = true;
			Assert(!TestCalculator.ShowMessageTypeSubType);
		}

		public void TestReadOnly()
		{
			TestCalculator.Minimum = 100m;
			TestCalculator.PerUnit = 0m;
			TestCalculator.BaseRate = 40m;
			TestCalculator.Percent = 10m;

			var item1 = TestCalculator.RateLineItems.AddNew();

			AssertEquals(100m, TestCalculator.Minimum);
			AssertEquals(40m, TestCalculator.BaseRate);
			AssertEquals(0m, TestCalculator.PerUnit);
			AssertEquals(0m, TestCalculator.Percent);
			Assert(!TestCalculator.Decimal4Info.ReadOnly);
			Assert(!TestCalculator.Decimal1Info.ReadOnly);
			Assert(TestCalculator.Decimal3Info.ReadOnly);

			var item2 = TestCalculator.RateLineItems.AddNew();
			Assert(!TestCalculator.Decimal4Info.ReadOnly);
			Assert(!TestCalculator.Decimal1Info.ReadOnly);
			Assert(TestCalculator.Decimal3Info.ReadOnly);
			Assert(TestCalculator.Decimal2Info.ReadOnly);

			TestCalculator.RateLineItems.RemoveAndDelete(item1);
			Assert(!TestCalculator.Decimal4Info.ReadOnly);
			Assert(!TestCalculator.Decimal1Info.ReadOnly);
			Assert(TestCalculator.Decimal3Info.ReadOnly);
			Assert(TestCalculator.Decimal2Info.ReadOnly);

			TestCalculator.RateLineItems.RemoveAndDelete(item2);
			Assert(!TestCalculator.Decimal4Info.ReadOnly);
			Assert(!TestCalculator.Decimal1Info.ReadOnly);
			Assert(!TestCalculator.Decimal3Info.ReadOnly);
			Assert(!TestCalculator.Decimal2Info.ReadOnly);
		}

		public void TestAcceptCostingCopiesCartageZones()
		{
			var provider = Helper.CreateRateTransportZoneSet(null, Constants.CountryCodes.Australia, zoneNames: new ZString[] { "zone1", "zone2", "zone3" });

			var costing = Helper.NewCosting(TransportProvider);
			var line = costing.AddRateEntry("DST", "FCL", "", "AUSYD").AddRateLine("DCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);

			line.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
			line.InitializeCalculator();

			var lineItem1 = line.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.BAS;
			lineItem1.TM_Value = 1m;
			lineItem1.TM_TZ_DomesticZone = provider.Zones[0].PK;

			var lineItem2 = line.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.BAS;
			lineItem2.TM_Value = 2m;
			lineItem2.TM_TZ_DomesticZone = provider.Zones[1].PK;

			var lineItem3 = line.RateLineItems.AddNew();
			lineItem3.TM_Type = Calculator.Items.Operator.Minus;
			lineItem3.TM_Value = 3m;
			lineItem3.TM_Break = 3m;
			lineItem3.TM_TZ_DomesticZone = provider.Zones[2].PK;

			var lineItem4 = line.RateLineItems.AddNew();
			lineItem4.TM_Type = Calculator.Items.Operator.Plus;
			lineItem4.TM_Value = 2.8m;
			lineItem4.TM_Break = 3m;
			lineItem4.TM_TZ_DomesticZone = provider.Zones[2].PK;

			var lineItem5 = line.RateLineItems.AddNew();
			lineItem5.TM_Type = Calculator.Items.Operator.Plus;
			lineItem5.TM_Value = 2.75m;
			lineItem5.TM_Break = 10m;
			lineItem5.TM_TZ_DomesticZone = provider.Zones[2].PK;

			Factory.Save();

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader(1));
			var quoteEntry = testQuote.AddRateEntry("DST", "FCL", "", "AUSYD");
			var quoteLine = quoteEntry.AddRateLine("DCART", CompanyTariffOrCostBasedCalculator.CostBasedCode);

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(quoteLine);
			var clone = cloneHelper.CreateClone(line);

			AssertNotNull(clone);
			AssertCalculatorZonedItemsResults(line, clone);
		}

		public void TestGetUnit()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "321";
			chargeCode.AC_ChargeGroup = "ORG";

			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
			var tariffRateLine = tariffEntry.RateLines.AddNew();
			tariffRateLine.TL_AC = chargeCode.PK;
			tariffRateLine.TL_WeightVolume = Constants.Volume.CubicMetres;
			tariffRateLine.TL_RateCalculator = UnitCalculator.Code;
			tariffRateLine.InitializeCalculator();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");

			var clientRateLine = clientRateEntry.RateLines.AddNew();
			clientRateLine.TL_AC = chargeCode.PK;
			clientRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			clientRateLine.InitializeCalculator();

			var clientRateLineItem = clientRateLine.RateLineItems.AddNew();
			clientRateLineItem.TM_Type = Calculator.Items.Operator.BAS;
			clientRateLineItem.TM_Value = 2m;

			Factory.Save();

			var tariffRateCalculator = tariffRateLine.Calculator;
			var clientRateCalculator = clientRateLine.Calculator;

			Assert("Pre-condition: client rate based on company tariffs should have no weight volume", clientRateLine.TL_WeightVolume.IsEmpty);

			var criteria = new TestRatingCriteria("AUSYD", "NZAKL", FreightMode.LSE, 3m, 1m, null);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));

			AssertEquals("Pre-condition: CTB calc is based on tariff calc", tariffRateCalculator.GetType(), clientRateCalculator.GetBaseCalculator(parameters).GetType());
			AssertEquals("Pre-condition: CTB calc is based on tariff calc", tariffRateCalculator.Line.PK, clientRateCalculator.GetBaseCalculator(parameters).Line.PK);
			AssertEquals("Expected to return M3", Constants.Volume.CubicMetres, tariffRateCalculator.GetUnit(parameters));
			AssertEquals("CTB calc should use same unit as the tariff it was based on rather than the default unit for this line (KG)",
				clientRateCalculator.GetUnit(parameters),
				tariffRateCalculator.GetUnit(parameters));
		}

		static void AssertCalculatorZonedItemsResults(RateLine expectedLine, RateLine actualLine)
		{
			var expectedCalculator = expectedLine.GetCalculator<CartageZoneDistanceCalculator>();
			var actualCalculator = actualLine.GetCalculator<CartageZoneDistanceCalculator>();
			AssertEquals("Expect lines to have same zone count", expectedCalculator.CartageZones.Count, actualCalculator.CartageZones.Count);

			for (var i = 0; i < actualCalculator.CartageZones.Count; i++)
			{
				var expectedZoneItems = expectedCalculator.CartageZones[i].ZoneRateLineItems;
				var actualZoneItems = actualCalculator.CartageZones[i].ZoneRateLineItems;

				AssertEquals("Expected zones to have same item count", expectedZoneItems.Count, actualZoneItems.Count);

				for (var j = 0; j < expectedZoneItems.Count; j++)
				{
					AssertEquals(expectedZoneItems[j].TM_Type, actualZoneItems[j].TM_Type);
					AssertEquals(expectedZoneItems[j].TM_Value, actualZoneItems[j].TM_Value);
					AssertEquals(expectedZoneItems[j].TM_Break, actualZoneItems[j].TM_Break);
				}
			}
		}

		public void TestValueIsReadOnlyWhereRateLineItemIsDeleted()
		{
			var rate = Line.RateLineItems.AddNew();
			rate.Delete();
			Assert(rate.IsDeleted);

			TestCalculator.ValueIsReadOnly(rate);
		}

		[TestDate(2023, 7, 27)]
		public void TestGetRelatedLines_GivenExpiredApplyToLineAndValidRelatedRateLineExist_ThenShouldReturnValidRelatedRateLine()
		{
			// Company Tariff
			var tariff = Factory.New<CompanyTariff>();
			var expiredEntry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line1 = expiredEntry.RateLines.AddNew();
			line1.TL_AC = ChargeCode.PK;
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.TL_WeightVolume = "KG";
			expiredEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			expiredEntry.TI_RateEndDate = new ZDate(2020, 3, 10);

			var entry2 = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line2 = entry2.RateLines.AddNew();
			line2.TL_AC = ChargeCode.PK;
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.TL_WeightVolume = "KG";
			entry2.TI_RateStartDate = new ZDate(2020, 3, 11);
			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			// Test
			var criteria = CreateCriteria();
			var jobDatesProvider = new Mock<IJobDatesProvider>();
			criteria.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(ZDate.Today.AddDays(5));
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>())).Returns(ZDate.Today);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));

			calculator.ApplyToLine = line1.PK.ToString();
			var originalLine = calculator.GetRelatedLines(parameters).FirstOrDefault();
			AssertEquals(
				"Given The accepted rate line is expired, and there is a valid related rate line in Company Tariff, When Call GetRelatedLines, Should return valid related rate line",
				line2.PK,
				originalLine.PK
			);
		}

		[TestDate(2023, 7, 27)]
		public void TestGetRelatedLines_GivenExpiredApplyToLineAndValidRelatedRateLineNotExist_ThenShouldReturnExpiredApplyToLine()
		{
			// Company Tariff
			var tariff = Factory.New<CompanyTariff>();
			var expiredEntry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line1 = expiredEntry.RateLines.AddNew();
			line1.TL_AC = ChargeCode.PK;
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.TL_WeightVolume = "KG";
			expiredEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			expiredEntry.TI_RateEndDate = new ZDate(2020, 3, 10);

			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			// Test
			var criteria = CreateCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));

			calculator.ApplyToLine = line1.PK.ToString();

			var originalLine = calculator.GetRelatedLines(parameters).FirstOrDefault();
			AssertEquals(
				"Given The accepted rate line is expired, and there is not a valid related rate line in Company Tariff, When Call GetRelatedLines, Should return expired accepted rate line",
				line1.PK,
				originalLine.PK
			);
		}

		[TestDate(2023, 7, 27)]
		public void TestGetRelatedLines_GivenValidApplyToLineAndOtherRelatedRateLineExist_ThenShouldReturnValidApplyToLine()
		{
			// Company Tariff
			var tariff = Factory.New<CompanyTariff>();
			var validRateEntry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line1 = validRateEntry.RateLines.AddNew();
			line1.TL_AC = ChargeCode.PK;
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.TL_WeightVolume = "KG";
			validRateEntry.TI_RateStartDate = new ZDate(2020, 2, 10);
			validRateEntry.TI_RateEndDate = new ZDate(2023, 7, 31);

			var entry2 = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line2 = entry2.RateLines.AddNew();
			line2.TL_AC = ChargeCode.PK;
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.TL_WeightVolume = "KG";
			entry2.TI_RateStartDate = new ZDate(2023, 8, 1);
			Factory.Save();

			// Calculator
			var calculator = CreateCalculatorToTest(CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			// Test
			var criteria = CreateCriteria();
			var jobDatesProvider = new Mock<IJobDatesProvider>();
			criteria.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(ZDate.Today.AddDays(5));
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>())).Returns(ZDate.Today);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext()));

			calculator.ApplyToLine = line1.PK.ToString();

			var originalLine = calculator.GetRelatedLines(parameters).FirstOrDefault();
			AssertEquals("Given the accepted rate line is valid, and there is another related rate line in the Company Tariff, When calling GetRelatedLines, it should return the accepted rate line", line1.PK, originalLine.PK);
		}

		#region Implementation

		OrgHeader TransportProvider
		{
			get
			{
				if (fTransportProvider == null)
				{
					fTransportProvider = Factory.New<OrgHeader>();
					fTransportProvider.OH_FullName = "Transport Provider 1";
					fTransportProvider.MainAddress.OA_Address1 = "123 Fake Street";
					fTransportProvider.MainAddress.OA_City = "Sydney";
					fTransportProvider.MainAddress.OA_State = "NSW";
					fTransportProvider.MainAddress.OA_PostCode = "2000";
					fTransportProvider.OH_RL_NKClosestPort = "AUSYD";
					fTransportProvider.OH_Code = "TRASPROV1";
				}
				return fTransportProvider;
			}
		}

		OrgHeader fTransportProvider;

		protected override Type CalculatorType
		{
			get { return typeof(CompanyTariffOrCostBasedCalculator); }
		}

		string fCalculatorCode = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
		protected override string CalculatorCode
		{
			get { return fCalculatorCode; }
		}

		new CompanyTariffOrCostBasedCalculator TestCalculator
		{
			get { return (CompanyTariffOrCostBasedCalculator)base.TestCalculator; }
		}

		RateLine CreateTariff(string calculator)
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line = entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateCalculator = calculator;
			line.TL_WeightVolume = "KG";

			return line;
		}

		RateLine CreateCost(string calculator)
		{
			var tariff = Factory.New<Costing>();
			var entry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line = entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateCalculator = calculator;
			line.TL_WeightVolume = "KG";

			return line;
		}

		CompanyTariffOrCostBasedCalculator CreateCalculatorToTest(string type, string containerCode = null, string commodityCode = null, bool matchContainerClass = false)
		{
			if (!string.IsNullOrEmpty(containerCode))
			{
				Entry.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode).PK;
			}

			Entry.TI_RH_NKCommodityCode = commodityCode;
			Entry.TI_MatchContainerRateClass = matchContainerClass;

			var line = Entry.RateLines.AddNew();
			line.TL_AC = ChargeCode.PK;
			line.TL_RateDesc = "Test Rate";
			line.TL_RateCalculator = type;
			line.TL_RX_NKCurrency = "AUD";

			return line.Calculator as CompanyTariffOrCostBasedCalculator;
		}

		TestRatingCriteria CreateCriteria(string origin = "AUSYD", string destination = "USLAX", decimal weightInKG = 15m, decimal volumeInM3 = 1)
		{
			return new TestRatingCriteria(origin, destination, FreightMode.LSE, weightInKG, volumeInM3, NewClient);
		}

		#endregion
	}

	public static class CompanyTariffOrCostBasedCalculatorTestExtensions
	{
		public static JobCharge AddPerUnitCalculation(this JobCharge charge, decimal chargeable, string unit, decimal rate, string currency, string chargeableReference = null, decimal unitMultiplier = 1)
		{
			var basis = charge.Factory.New<JobPaymentBasis>();
			basis.PBS_JR = charge.PK;
			basis.PBS_IsCost = true;
			basis.PBS_PerUnitRate = rate;
			basis.PBS_RX_NKRateCurrency = currency;
			basis.PBS_RateReference = "UNT";
			basis.PBS_RateUnit = unit;
			basis.PBS_RateUnitMultiplier = unitMultiplier;
			basis.PBS_ChargeableAmount = chargeable;
			basis.PBS_ChargeableUnit = unit;
			basis.PBS_ChargeableDescription = chargeableReference;

			return charge;
		}

		public static JobCharge AddFlatCalculation(this JobCharge charge, decimal rate, string currency)
		{
			var basis = charge.Factory.New<JobPaymentBasis>();
			basis.PBS_JR = charge.PK;
			basis.PBS_IsCost = true;
			basis.PBS_FlatRate = rate;
			basis.PBS_RX_NKRateCurrency = currency;
			basis.PBS_RateReference = "FLT";

			return charge;
		}

		public static JobCharge AddMinCalculation(this JobCharge charge, decimal rate, string currency)
		{
			var basis = charge.Factory.New<JobPaymentBasis>();
			basis.PBS_JR = charge.PK;
			basis.PBS_IsCost = true;
			basis.PBS_FlatRate = rate;
			basis.PBS_RX_NKRateCurrency = currency;
			basis.PBS_RateReference = "MIN";

			return charge;
		}

		public static JobCharge AddMaxCalculation(this JobCharge charge, decimal rate, string currency)
		{
			var basis = charge.Factory.New<JobPaymentBasis>();
			basis.PBS_JR = charge.PK;
			basis.PBS_IsCost = true;
			basis.PBS_FlatRate = rate;
			basis.PBS_RX_NKRateCurrency = currency;
			basis.PBS_RateReference = "MAX";

			return charge;
		}
	}
}
