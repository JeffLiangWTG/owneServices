using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class EquipmentHireCalculatorTest : CalculatorTest
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

			var container = Factory.New<RefContainer>();
			container.RC_ShippingMode = "ROA";
			container.RC_Code = "RZUB";
			container.RC_Description = "Zubin Truck";

			var container2 = Factory.New<RefContainer>();
			container2.RC_ShippingMode = "ROA";
			container2.RC_Code = "RRAK";
			container2.RC_Description = "Rakhsh Truck";
			Factory.Save();

			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			TestCalculator.AddRateLineItem("", 0m, 0m, "DY").TM_Text = "RZUB";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Zubin Truck||Not Charged|", quotationLines[1].ToString());

			Line.RateLineItems[0].TM_RelevantValue = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Zubin Truck|USD|100.00|per Day(s)", quotationLines[1].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(2, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Zubin Truck|USD|100.00|per Day(s)", quotationLines[1].ToString());

			TestCalculator.AddRateLineItem("", 0m, 20m, "HR").TM_Text = "RRAK";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Zubin Truck|USD|100.00|per Day(s)", quotationLines[1].ToString());
			AssertEquals("Rakhsh Truck|USD|20.00|per Hour(s)", quotationLines[2].ToString());
		}

		public override void TestDocLineAmount()
		{
			Assert("EQH calculator is currently disabled in WI00338101_RemoveEQHCalculatorV3 Remove EQH option from Calculator of setting up rates under Tariffs & Rates modules as it has not been developed or tested", true);
		}

		public void TestValidateTM_Text()
		{
			var container = Factory.New<RefContainer>();
			container.RC_ShippingMode = "ROA";
			container.RC_Code = "RZUB";
			container.RC_Description = "Zubin Truck";
			Factory.Save();

			var item = TestCalculator.AddRateLineItem("", 0m, 500m, "DY");
			item.TM_Text = "###";
			AssertHasError(Line.RateLineItems[0].TM_TextInfo, "Enter a valid selection.");

			item = TestCalculator.AddRateLineItem("", 0m, 25m, "HR");
			item.TM_Text = "RZUB";
			AssertNoErrors(Line.RateLineItems[1].TM_TextInfo);

			Line.RateLineItems[0].TM_Text = "RZUB";
			AssertHasError(Line.RateLineItems[0].TM_TextInfo, "You can only specify one rate for each equipment type. You should not specify the same equipment type more than once.");
		}

		public void TestValidateTM_BreakWeightVolume()
		{
			TestCalculator.AddRateLineItem("TRK", 0m, 500m, "");
			AssertHasError(Line.RateLineItems[0].TM_BreakWeightVolumeInfo, "Please enter a " + Line.RateLineItems[0].TM_BreakWeightVolumeInfo.Description + ".");

			Line.RateLineItems[0].TM_BreakWeightVolume = "##";
			AssertHasError(Line.RateLineItems[0].TM_BreakWeightVolumeInfo, "Enter a valid " + Line.RateLineItems[0].TM_BreakWeightVolumeInfo.Description + ".");

			Line.RateLineItems[0].TM_BreakWeightVolume = "DY";
			AssertNoErrors(Line.RateLineItems[0].TM_BreakWeightVolumeInfo);
		}

		public override void TestGetCloneLineItems()
		{
			var item1 = TestCalculator.RateLineBizO.RateLineItems.AddNew();
			item1.TM_Text = "CHIP";
			item1.TM_BreakWeightVolume = "HR";
			item1.TM_RelevantValue = 100;

			var item2 = TestCalculator.RateLineBizO.RateLineItems.AddNew();
			item2.TM_Text = "OPEN";
			item2.TM_BreakWeightVolume = "HR";
			item2.TM_RelevantValue = 200;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.Percent = 50m;
			source.BaseRate = 80m;

			var clonedLine = new CompanyTariffOrCostLineCloneHelper(line).CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			var chipItem = clonedLine.GetCalculator<EquipmentHireCalculator>().RateLineItems
				.Cast<RateLineItem>()
				.FirstOrDefault(x => x.TM_Text == "CHIP");

			var openItem = clonedLine.GetCalculator<EquipmentHireCalculator>().RateLineItems
				.Cast<RateLineItem>()
				.FirstOrDefault(x => x.TM_Text == "OPEN");

			AssertNotNull(chipItem);
			AssertNotNull(openItem);

			AssertEquals(230m, chipItem.TM_RelevantValue);
			AssertEquals(380m, openItem.TM_RelevantValue);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode("EQH");
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(EquipmentHireCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return EquipmentHireCalculator.Code; }
		}

		new EquipmentHireCalculator TestCalculator
		{
			get { return (EquipmentHireCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
