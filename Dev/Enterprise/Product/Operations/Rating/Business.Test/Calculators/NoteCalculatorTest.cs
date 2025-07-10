using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class NoteCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var line = rateEntry.RateLines.AddNew();
			AssertNull(line.RateLineItems.FindByTM_Type(NoteCalculator.Items.ShowOnBillingWithoutPrefix));
			line.TL_RateCalculator = NoteCalculator.Code;
			AssertNotNull(line.RateLineItems.FindByTM_Type(NoteCalculator.Items.ShowOnBillingWithoutPrefix));
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public void TestAutoRateDescription()
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			((RateLine)TestCalculator.Line).TL_RateDescLocal = "Test Rate Local";
			AssertEquals("Rate Description", new ZString("RATE NOTE: Test Rate Local"), TestCalculator.AutoRateDescription(parameters, isLocalDescription: true));

			((RateLine)TestCalculator.Line).OverrideChargeDescription = true;
			((RateLine)TestCalculator.Line).TL_RateDescLocal = "";
			AssertEquals("RateLine.Description", "Test Charge", TestCalculator.Line.TL_RateDesc);
			AssertEquals
			(
				"WHEN RateLine.LocalDescription is empty THEN default to RateLine.Description",
				new ZString("RATE NOTE: Test Charge"),
				TestCalculator.AutoRateDescription(parameters, isLocalDescription: true)
			);
		}

		public void TestShowOnBillingWithoutPrefix()
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = NoteCalculator.Code;
			AssertEquals(false, ((NoteCalculator)line.Calculator).ShowOnBillingWithoutPrefix);
			((NoteCalculator)line.Calculator).ShowOnBillingWithoutPrefix = true;
			AssertEquals(true, ((NoteCalculator)line.Calculator).ShowOnBillingWithoutPrefix);

			RatingDataRegistry.Instance.UseShowOnBillingWithoutPrefixDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = NoteCalculator.Code;
			AssertEquals(true, ((NoteCalculator)line.Calculator).ShowOnBillingWithoutPrefix);
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Text = "Test 1";

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Text = "Test 2";
			item2.TM_RelevantValue = 60m;

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Test 1|||", quotationLines[1].ToString());
			AssertEquals("Test 2|USD|60.00|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Test 1|||", quotationLines[1].ToString());
			AssertEquals("Test 2|USD|60.00|", quotationLines[2].ToString());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddNoteRateLine(rateEntry, "FRT", "KG", "AUD", ("Note 10", 10));
			var rateLine11 = AddNoteRateLine(rateEntry, "FRT", "KG", "AUD", ("Note 10", 11));
			var rateLine12 = AddNoteRateLine(rateEntry, "FRT", "KG", "AUD", ("Note 12", 12));

			var rateLine20 = AddNoteRateLine(rateEntry, "FRT", "KG", "USD", ("Note 20", 20));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Note 10|AUD|21.00|",
					"Note 12|AUD|12.00|",
					"Note 20|USD|20.00|"
				}
			);
		}

		static RateLine AddNoteRateLine(RateEntry rateEntry, ZString chargeCode, string unit, string currency, params (string Text, decimal Value)[] notes)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, NoteCalculator.Code, unit, currency);

			foreach (var note in notes)
			{
				var item = rateLine.RateLineItems.AddNew();
				item.TM_Text = note.Text;
				item.TM_RelevantValue = note.Value;
			}

			return rateLine;
		}

		public void TestCalculation()
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertCalculation(parameters, 0m, "Base Rate AUD 0.00");
			AssertEquals("Rate Description", new ZString("RATE NOTE: Test Rate"), TestCalculator.AutoRateDescription(parameters));

			var item1 = Line.RateLineItems.AddNew();
			item1.TM_Text = "Test 1";

			var item2 = Line.RateLineItems.AddNew();
			item2.TM_Text = "Test 2";
			item2.TM_RelevantValue = 60m;

			AssertCalculation(parameters, 0m, "Base Rate AUD 0.00");
			AssertEquals("Rate Description", new ZString("RATE NOTE: Test Rate" + System.Environment.NewLine + "\tTest 1" + System.Environment.NewLine + "\tTest 2\t $60"), TestCalculator.AutoRateDescription(parameters));

			var gbp = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom))[0];
			Line.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;

			AssertCalculation(parameters, 0m, "Base Rate GBP 0.00");
			AssertEquals("Rate Description", new ZString("RATE NOTE: Test Rate" + System.Environment.NewLine + "\tTest 1" + System.Environment.NewLine + "\tTest 2\t " + gbp.RX_Symbol + "60"), TestCalculator.AutoRateDescription(parameters));

			Line.TL_RateCalculator = NoteCalculator.Code;
			((NoteCalculator)Line.Calculator).ShowOnBillingWithoutPrefix = true;

			AssertCalculation(parameters, 0m, "Base Rate GBP 0.00");
			AssertEquals("Rate Description", new ZString("Test Rate" + System.Environment.NewLine + "\tTest 1" + System.Environment.NewLine + "\tTest 2\t " + gbp.RX_Symbol + "60"), TestCalculator.AutoRateDescription(parameters));
		}

		public override void TestGetCloneLineItems()
		{
			var item1 = (TestCalculator.Line as RateLine).RateLineItems.AddNew();
			item1.TM_Text = "first";
			item1.TM_BreakWeightVolume = "HR";
			item1.TM_RelevantValue = 100;

			var item2 = (TestCalculator.Line as RateLine).RateLineItems.AddNew();
			item2.TM_Text = "second";
			item2.TM_BreakWeightVolume = "HR";
			item2.TM_RelevantValue = 200;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.Percent = 50m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			var firstItem = clonedLine.GetCalculator<NoteCalculator>().RateLineItems
				.Cast<RateLineItem>()
				.FirstOrDefault(x => x.TM_Text == "first");

			var secondItem = clonedLine.GetCalculator<NoteCalculator>().RateLineItems
				.Cast<RateLineItem>()
				.FirstOrDefault(x => x.TM_Text == "second");

			AssertNotNull(firstItem);
			AssertNotNull(secondItem);

			AssertEquals(230m, firstItem.TM_RelevantValue);
			AssertEquals(380m, secondItem.TM_RelevantValue);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode("NTE");
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(NoteCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return NoteCalculator.Code; }
		}

		new NoteCalculator TestCalculator
		{
			get { return (NoteCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
