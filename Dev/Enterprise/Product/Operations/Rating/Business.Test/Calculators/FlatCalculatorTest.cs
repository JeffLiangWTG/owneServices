using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business.Testing
{
	public class FlatCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			InitialiseTestCalculator();
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertEquals(1, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
		}

		public override void TestMapping()
		{
			TestMapping(Calculator.Items.Operator.BAS, "Decimal1");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);
			var rateLine1 = rateEntry.AddFlatRateLine("FRT", 10, "AUD");
			var rateLine2 = rateEntry.AddFlatRateLine("FRT", 11, "AUD");
			var rateLine3 = rateEntry.AddFlatRateLine("FRT", 20, "USD");
			var rateLine4 = rateEntry.AddFlatRateLine("FRT", 30, "NZD");

			var docLineAmount = rateLine1.Calculator.GetDocLineAmount()
				+ rateLine2.Calculator.GetDocLineAmount()
				+ rateLine3.Calculator.GetDocLineAmount()
				+ rateLine4.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[] { "|AUD|21.00|", "|USD|20.00|" , "|NZD|30.00|" }
			);
		}

		public override void TestQuotationLines()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge||Not Charged|", quotationLines[0].ToString());

			TestCalculator.BaseRate = 200.5m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|200.50|", quotationLines[0].ToString());

			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			Line.Parent.TI_OH_Consignee = consignee.PK;
			Line.Parent.TI_OH_Consignor = consignor.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(ZString.Format(@"Test Rate 
-  From {0} To {1}|USD|200.50|", consignor.OH_FullName, consignee.OH_FullName), quotationLines[0].ToString());
		}

		public void TestQuotationLines_LocalLanguageUnit()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			var info = typeof(RawDataRegistry).GetProperty("FlatFeeText", BindingFlags.GetProperty | BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var registryItem = info.GetValue(Env.Registry.RawRegistry, null) as MultilingualStringRegistryItem;
			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Flat Rate (English)");

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Code BBB";
			chargeCode.AC_ShowOnQuotation = true;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RX_NKCurrency = "AUD";
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateLine.Calculator).BaseRate = 120m;

			var quotationLine = rateLine.Calculator.GetQuotationLines(parentEntry)[0];
			AssertEquals("Charge Code BBB|AUD|120.00|Flat Rate (English)", quotationLine.ToString());

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				var key = ((ResourceString)registryItem.Value).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "Flat Rate (Chinese - Simplified)"));

				AssertEquals("Charge Code BBB|AUD|120.00|Flat Rate (Chinese - Simplified)", quotationLine.ToString());
			}
		}

		public void TestCalculation()
		{
			Line.TL_RX_NKCurrency = "AUD";
			TestCalculator.BaseRate = 50m;
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertCalculation(parameters, 50m, "Base Rate AUD 50.00");

			TestCalculator.BaseRate = 0m;
			AssertCalculation(parameters, 0m, "Base Rate AUD 0.00");

			Line.TL_RX_NKCurrency = "USD";
			TestCalculator.BaseRate = 12.335m;
			var result = AssertCalculation(parameters, 12.34m, "Base Rate USD 12.335");
			AssertEquals("", result.ChargeUnit);
		}

		public void TestCalculationBaseRateTextWhenOverridden()
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertCalculation(parameters, 0m, "Base Rate AUD 0.00");

			RatingDataRegistry.Instance.BaseRateText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Rate");
			try
			{
				AssertCalculation(parameters, 0m, "Test Rate AUD 0.00");
			}
			finally
			{
				RatingDataRegistry.Instance.BaseRateText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RatingDataRegistry.Instance.BaseRateText.DefaultValue);
			}
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.BaseRate = 25m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(105m, clonedLine.GetCalculator<FlatCalculator>().BaseRate);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(110m, clonedLine.GetCalculator<FlatCalculator>().BaseRate);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(126m, clonedLine.GetCalculator<FlatCalculator>().BaseRate);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.BaseRate = 50m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(1, items.Count);
			AssertChargesSummaryItem(items[0], "BAS", 50m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 50m, "AUD");

			AssertEquals("AUD 50.0000", rateEntry.AllInCost());
			AssertEquals("", rateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(FlatCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return FlatCalculator.Code; }
		}

		protected new FlatCalculator TestCalculator
		{
			get { return (FlatCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
