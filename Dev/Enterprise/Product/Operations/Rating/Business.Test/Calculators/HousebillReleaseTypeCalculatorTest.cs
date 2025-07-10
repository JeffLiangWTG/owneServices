using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class HousebillReleaseTypeCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";
			Line.TL_RateCalculator = HousebillReleaseTypeCalculator.Code;

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			var item = Line.RateLineItems.AddNew();
			item.TM_Type = "STD";

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = "CAD";
			item2.TM_RelevantValue = 60m;

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Standard Release Type||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Cash Against Documents|USD|60.00|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Standard Release Type||Not Charged|", quotationLines[1].ToString());
			AssertEquals("Cash Against Documents|USD|60.00|", quotationLines[2].ToString());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddHousebillReleaseTypeRateLine(rateEntry, "FRT", QuantityUnit.PK, "AUD", ("CAD", 101), ("STD", 102));
			var rateLine11 = AddHousebillReleaseTypeRateLine(rateEntry, "FRT", QuantityUnit.PK, "AUD", ("CAD", 111), ("EBL", 112));

			var rateLine20 = AddHousebillReleaseTypeRateLine(rateEntry, "FRT", QuantityUnit.PK, "USD", ("CAD", 201));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Cash Against Documents|AUD|212.00|",
					"Standard Release Type|AUD|102.00|",
					"Express Bill of Lading|AUD|112.00|",
					"Cash Against Documents|USD|201.00|"
				}
			);
		}

		static RateLine AddHousebillReleaseTypeRateLine(RateEntry rateEntry, ZString chargeCode, string unit, string currency, params (string Code, decimal Amount)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, HousebillReleaseTypeCalculator.Code, unit, currency);

			foreach (var item in items)
			{
				var item101 = rateLine.RateLineItems.AddNew();
				item101.TM_Type = item.Code;
				item101.TM_RelevantValue = item.Amount;
			}

			return rateLine;
		}

		public void TestCalculation()
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria) { Criteria = { HousebillReleaseType = "EBL" } };
			AssertCalculation(parameters, 0m, "No match found for h/b with release: Express Bill of Lading (EBL)");

			Line.TL_RX_NKCurrency = "AUD";
			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Type = "EBL";
			item1.TM_RelevantValue = 20m;
			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Type = "STD";
			item2.TM_RelevantValue = 10m;
			var item3 = Line.RateLineItems.AddNew();
			item3.TM_Type = "ORQ";
			item3.TM_RelevantValue = 30m;

			AssertCalculation(parameters, 20m, "1 Express Bill of Lading h/b release @ Base Rate AUD 20.00");

			parameters.Criteria.HousebillReleaseType = "LOI";
			AssertCalculation(parameters, 10m, "1 Standard h/b release @ Base Rate AUD 10.00");
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.AddRateLineItem("EBL", 0m, 100m);
			TestCalculator.AddRateLineItem("CAD", 0m, 200m);

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.Percent = 50m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(230m, clonedLine.GetCalculator<HousebillReleaseTypeCalculator>().FindRateLineItem("EBL").TM_RelevantValue);
			AssertEquals(380m, clonedLine.GetCalculator<HousebillReleaseTypeCalculator>().FindRateLineItem("CAD").TM_RelevantValue);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode("HRT");
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(HousebillReleaseTypeCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return HousebillReleaseTypeCalculator.Code; }
		}

		new HousebillReleaseTypeCalculator TestCalculator
		{
			get { return (HousebillReleaseTypeCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
