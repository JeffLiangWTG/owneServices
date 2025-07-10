using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class UnitCalculatorTest : CalculatorTest
	{
		public void TestShouldValueBeDiscounted()
		{
			AssertEquals(true, TestCalculator.ShouldValueBeDiscounted(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT)));
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			InitialiseTestCalculator();
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT));
			AssertEquals(1, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
		}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.UNT, "Decimal1");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = rateEntry.AddUnitRateLine("FRT", 10, "KG", "AUD");
			var rateLine11 = rateEntry.AddUnitRateLine("FRT", 11, "KG", "AUD");

			var rateLine20 = rateEntry.AddUnitRateLine("FRT", 20, "KG", "USD");

			var rateLine30 = rateEntry.AddUnitRateLine("FRT", 30, "CTN", "USD");

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount()
				+ rateLine30.Calculator.GetDocLineAmount();
			var quotationLineList = docLineAmount.GetQuotationLineList(rateLine10);

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"|AUD|21.00|per KG",
					"|USD|20.00|per KG",
					"|USD|30.00|per Carton"
				}
			);
		}

		public void TestQuotationLinesAlternativeFomat()
		{
			TestCalculator.PerUnit = 2.95m;

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var testContainer = Factory.New<RefContainer>();
			testContainer.RC_Code = "11AA";

			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			Line.TL_WeightVolume = RatingConstants.Units.CN;
			Line.Parent.TI_RC = testContainer.PK;

			var alternativeFormat = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				var quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.AlternativeFormat, parentEntry);

				AssertEquals(1, quotationLines.Count);
				AssertEquals(string.Format("Test Rate|AUD|2.95|per {0} Container", testContainer.RC_Code), quotationLines[0].ToString());
			}
			finally
			{
				DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, alternativeFormat);
			}
		}

		public override void TestQuotationLines()
		{
			Line.Parent.TI_Mode = Core.Constants.RateMode.AIR;
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge||Not Charged|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 2.95m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|2.95|per KG (1 M3 = 1000 KG)", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit ({0} - {1})|AUD|2.95|per KG (1 M3 = 1000 KG)", ZDateTime.Today.ToShortDateString(), ZDateTime.Today.AddMonths(6).ToShortDateString()), quotationLines[0].ToString());

			Line.Parent.TI_RateEndDate = ZDate.Empty;
			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit (from {0})|AUD|2.95|per KG (1 M3 = 1000 KG)", ZDateTime.Today.ToShortDateString()), quotationLines[0].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 2.95m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|2.95|per W/M", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit ({0} - {1})|AUD|2.95|per W/M", ZDateTime.Today.ToShortDateString(), ZDateTime.Today.AddMonths(6).ToShortDateString()), quotationLines[0].ToString());

			Line.Parent.TI_RateEndDate = ZDate.Empty;
			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit (from {0})|AUD|2.95|per W/M", ZDateTime.Today.ToShortDateString()), quotationLines[0].ToString());
		}

		public void TestQuotationLines_NonWeightVolumeUnit()
		{
			Line.TL_WeightVolume = "HB";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.PerUnit = 2.95m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|2.95|per House Bill", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit ({0} - {1})|AUD|2.95|per House Bill", ZDateTime.Today.ToShortDateString(), ZDateTime.Today.AddMonths(6).ToShortDateString()), quotationLines[0].ToString());

			Line.Parent.TI_RateEndDate = ZDate.Empty;
			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowStartEndDates, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format("Test Rate Per Unit (from {0})|AUD|2.95|per House Bill", ZDateTime.Today.ToShortDateString()), quotationLines[0].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator.PerUnit = 5m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 0, "");
			AssertCalculation(parameters, 0m, "0 Kilogram(s) @ USD 5.00/KG");

			Line.TL_Rounding = RatingRoundingTypes.UpTo1;
			parameters.ChargeableAmount = new Quantity(14.6M, QuantityUnit.KG);
			var result = AssertCalculation(parameters, 75m, "15 Kilogram(s) @ USD 5.00/KG");
			AssertEquals(15m, result.PaymentBases.ChargeableAmount());
			AssertEquals(14.6m, result.PaymentBases.UnroundedChargeableAmount());
			AssertEquals(QuantityUnit.KG, result.ChargeUnit);

			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRateUnit, "KG"), result.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.UnroundedItemsToRate, "14.6"), result.Attributes.Attributes);

			parameters.ChargeableAmount = new Quantity(100M, QuantityUnit.KG);
			AssertCalculation(parameters, 500m, "100 Kilogram(s) @ USD 5.00/KG");

			Line.TL_WeightVolume = QuantityUnit.CN;
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.CN);
			AssertCalculation(parameters, 50m, "10 Container(s) @ USD 5.00/Container");

			Line.TL_WeightVolume = QuantityUnit.CN;

			var container = Factory.New<RefContainer>();
			container.RC_Code = "20ZZ";
			Line.Parent.TI_RC = container.PK;
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.CN);
			result = AssertCalculation(parameters, 50m, "10 20ZZ Container(s) @ USD 5.00/Container");
			AssertEquals(10m, result.PaymentBases.ChargeableAmount());
			AssertEquals(10m, result.PaymentBases.UnroundedChargeableAmount());
			AssertEquals(QuantityUnit.CN, result.ChargeUnit);

			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRateUnit, "CN"), result.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.UnroundedItemsToRate, "10"), result.Attributes.Attributes);

			Line.TL_WeightVolume = QuantityUnit.HR;
			parameters.SetTime(0, 9);
			AssertCalculation(parameters, 45m, "9 Hour(s) @ USD 5.00/Hour");
		}

		public void TestCalculation_ServiceOccurrence()
		{
			ChargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Cleaning;
			Line.TL_WeightVolume = "SV";
			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.PerUnit = 5m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.JobServices = new JobServicesCollection();

			var serviceInfo = new JobServiceInfo(true, "ORG", Constants.FreightServiceType.Codes.Cleaning, "Origin Cleaning", 3m);
			Criteria.JobServices.Add(serviceInfo);
			parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertCalculation(parameters, 15m, "3 Origin Cleaning @ USD 5.00/Origin Cleaning");
		}

		public void TestCalculation_JobWeightVolume()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.PerUnit = 5m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.Weight, 30m, "LB");
			parameters.Criteria.RateableMeasures.SetQuantity(MeasureType.JobWeight, 100m, "LB");

			Line.TL_IsWhsJobLevelCharge = true;
			AssertCalculation(parameters, 226.80m, "45.359 Kilogram(s) @ USD 5.00/KG");

			Line.TL_IsWhsJobLevelCharge = false;
			AssertCalculation(parameters, 68.04m, "13.608 Kilogram(s) @ USD 5.00/KG");
		}

		public override void TestGetCloneCode()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.BaseRate = 10m;
			AssertEquals(FlatPlusPerUnitCalculator.Code, TestCalculator.GetCloneCode(source));
			source.BaseRate = 0m;
			AssertEquals(CalculatorCode, TestCalculator.GetCloneCode(source));
			source["+45"] = (ZDecimal)5m;
			AssertEquals(CombinedCalculator.Code, TestCalculator.GetCloneCode(source));
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.PerUnit = 5m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(11m, clonedLine.GetCalculator<UnitCalculator>().PerUnit);

			source.BaseRate = 80m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(11m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(80m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.PerUnit = 0m;
			source.PerUnitPercent = 25m;
			source.Percent = 25m;
			source.PerUnitPercent = 25m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(6.25m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(80m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(6.25m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().PerUnit);
			AssertEquals(100m, clonedLine.GetCalculator<FlatPlusPerUnitCalculator>().BaseRate);

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source["-45"] = (ZDecimal)4m;
			source["+45"] = (ZDecimal)3m;
			source["+100"] = (ZDecimal)2m;
			source["+250"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(80m, clonedLine.GetCalculator<CombinedCalculator>().BaseRate);
			AssertEquals(9m, clonedLine.Calculator["-45"]);
			AssertEquals(8m, clonedLine.Calculator["+45"]);
			AssertEquals(7m, clonedLine.Calculator["+100"]);
			AssertEquals(6m, clonedLine.Calculator["+250"]);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.PerUnit = 5m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(1, items.Count);
			AssertChargesSummaryItem(items[0], "UNT", 5m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateLine = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", UnitCalculator.Code, "KG", "AUD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			AssertEquals("AUD 5.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		public void TestFreightRatePerChargeableUnit_DifferentUnits()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "KG", "AUD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "Box", "AUD");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 3m;

			var rateLine3 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "PLT", "USD");
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 7m;

			AssertEquals("AUD 8.0000 + USD 7.0000", rateEntry.AllInCost());
			AssertEquals("AUD 5.0000/KG + AUD 3.0000/Box + USD 7.0000/PLT", rateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(UnitCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return UnitCalculator.Code; }
		}

		new UnitCalculator TestCalculator
		{
			get { return (UnitCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
