using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class HighestChargeCalculatorTest : CalculatorTest
	{
		public void TestCalculate()
		{
			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");

			var o200 = Helper.ChargeCodes.New("O200", "O200", "FLT");
			var o400 = Helper.ChargeCodes.New("O400", "O400", "FLT");
			var o600 = Helper.ChargeCodes.New("O600", "O600", "FLT");

			entry.AddFlatRateLine(o200.AC_Code, 200);
			entry.AddFlatRateLine(o400.AC_Code, 400);
			entry.AddFlatRateLine(o600.AC_Code, 600);

			CreateChargeLineItem(o200);
			CreateChargeLineItem(o400);
			CreateChargeLineItem(o600);

			var fAutoRateInfos = new AutoRateInfoCollection(Factory);
			fAutoRateInfos.AddNew(o200, "AUD", 200m);
			fAutoRateInfos.AddNew(o400, "AUD", 400m);
			fAutoRateInfos.AddNew(o600, "AUD", 600m);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, fAutoRateInfos);
			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { entry }), new TestLogger());
			parameters.SetLinesToCalculate(linesRepo);

			TestCalculator.Calculate(parameters);

			AssertEquals(1, parameters.Results.Count);
			AssertEquals(o600.PK, parameters.Results[0].ChargeCode.PK);
			AssertContains("HCC Over Charges: O200, O400", parameters.Results[0].CalculationDescription);
		}

		void CreateChargeLineItem(AccChargeCode accChargeCode)
		{
			var rli = TestCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			rli.TM_AC = accChargeCode.PK;
		}

		#region Implementation

		public override void TestCheckOrCreateItems()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			var o200 = Helper.ChargeCodes.New("O200", "O200", "FLT");
			var o400 = Helper.ChargeCodes.New("O400", "O400", "FLT");
			var o600 = Helper.ChargeCodes.New("O600", "O600", "FLT");

			CreateChargeLineItem(o200);
			CreateChargeLineItem(o400);
			CreateChargeLineItem(o600);

			AssertEquals(3, Line.RateLineItems.Count);
			var itemChargePks = Line.RateLineItems.Cast<RateLineItem>().Select(rli => rli.TM_AC);
			AssertContainsExactElementsInAnyOrder(new[] { o200.PK, o400.PK, o600.PK }, itemChargePks);

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(3, clonedLine.GetCalculator<HighestChargeCalculator>().RateLineItems.Count);
			var clonedLineItemChargePks = clonedLine.GetCalculator<HighestChargeCalculator>().RateLineItems.Cast<RateLineItem>().Select(rli => rli.TM_AC);
			AssertContainsExactElementsInAnyOrder(new[] { o200.PK, o400.PK, o600.PK }, clonedLineItemChargePks);
		}

		public override void TestMapping()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestQuotationLines()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestDocLineAmount()
		{
			Assert("Nothing printed because no amount", true);
		}

		protected override Type CalculatorType
		{
			get { return typeof(HighestChargeCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return HighestChargeCalculator.Code; }
		}

		new HighestChargeCalculator TestCalculator
		{
			get { return (HighestChargeCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
