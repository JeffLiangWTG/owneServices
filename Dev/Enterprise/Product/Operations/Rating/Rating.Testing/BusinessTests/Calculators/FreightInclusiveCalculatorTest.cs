using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Rating.Business.FreightInclusiveCalculator;

namespace Enterprise.Rating.Business.Testing
{
	public partial class FreightInclusiveCalculatorTest : CalculatorTest
	{
		protected ClientRate ClientRate => fClientRate ?? (fClientRate = Helper.NewClientRate(NewClient));
		ClientRate fClientRate;

		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			TestMapping(Items.FreightCalcType, "String1");

			var rateLineItem = Line.RateLineItems.AddNew();
			rateLineItem.TM_Type = Items.PreCarriageOnCarriageChargeType;

			var testGuid1 = ZGuid.NewZGuid();
			rateLineItem.TM_AC = testGuid1;
			AssertEquals(CalculatorConstants.MapTo.ChargeCode,
				testGuid1,
				CalculatorType.GetProperty(CalculatorConstants.MapTo.ChargeCode).GetValue(TestCalculator, null));

			var testGuid2 = ZGuid.NewZGuid();
			CalculatorType.GetProperty(CalculatorConstants.MapTo.ChargeCode).SetValue(TestCalculator, testGuid2, null);
			AssertEquals(Items.PreCarriageOnCarriageChargeType,
				testGuid2,
				Line.RateLineItems.FindByTM_Type(Items.PreCarriageOnCarriageChargeType).TM_AC);
		}

		public override void TestQuotationLines()
		{
			Assert(true);
		}

		public override void TestGetCloneCode()
		{
			Assert(true);
		}

		public override void TestGetCloneLineItems()
		{
			Assert(true);
		}

		public override void TestList1()
		{
			var expectedTypes = new[]
			{
				FreightCalcTypes.Included,
				FreightCalcTypes.SubjectTo,
				FreightCalcTypes.NotApplicable
			};

			AssertContainsExactElementsInAnyOrder(expectedTypes, TestCalculator.List1.GetAllCodes());
		}

		public void TestCalculation_ChargeCodeIsLocalAndRelatedLineHasLocalChargeCode_ShouldAddItselfToIncludedLines()
		{
			var helper = new TestHelper(Factory);
			TestCalculator.ChargeCode = helper.ChargeCodes["FRT"].PK;

			var entry = ClientRate.AddRateEntry("LCL", "FTL", "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var frtLine = entry.AddRateLine("FRT", FlatCalculator.Code);
			var bafLine = entry.AddRateLine("BAF", FlatCalculator.Code);

			var entry2 = ClientRate.AddRateEntry("ORG", "FTL", "AU", "");
			entry2.RateLines.RemoveAndDeleteAll();
			var cafLine = entry2.AddRateLine("CAF", FlatCalculator.Code);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { entry, entry2 }), new TestLogger());
			parameters.SetLinesToCalculate(linesRepo);

			TestCalculator.Calculate(parameters);

			var expectedFrtIncludedLines = new[] { "CCC" };
			var actualFrtIncludedLines = frtLine.IncludedLines.Select(s => (string)s.ChargeCode.AC_Code).ToArray();
			AssertContainsExactElementsInAnyOrder(
				"Expected FRT line to include specific charge codes",
				expectedFrtIncludedLines,
				actualFrtIncludedLines
			);

			AssertEquals(0, bafLine.IncludedLines.Count);
			AssertEquals(0, cafLine.IncludedLines.Count);
		}

		public void TestCalculation_ChargeCodeIsGlobalAndRelatedLineHasLocalChargeCode_ShouldAddItselfToIncludedLines()
		{
			var helper = new TestHelper(Factory);
			var globalFRT = helper.ChargeCodes.CreateGlobalCharge("GFRT");
			var localFRT = globalFRT.GetLocalChargeCode(LedgerTypes.AccountsPayable, null);

			Factory.Save();

			TestCalculator.ChargeCode = globalFRT.PK;

			var carrier = Factory.New<OrgHeader>();
			var costing = Helper.NewGlobalCosting(carrier);
			var entry = costing.AddRateEntry("LCL", "SEA", "AU", "UA");
			entry.RateLines.RemoveAndDeleteAll();
			var frtLine = entry.AddRateLine(localFRT);
			var bafLine = entry.AddRateLine("BAF", FlatCalculator.Code);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { entry }), new TestLogger());
			parameters.SetLinesToCalculate(linesRepo);

			TestCalculator.Calculate(parameters);

			var frtLineIncludedCodes = frtLine.IncludedLines.Select(s => (string)s.ChargeCode.AC_Code).ToArray();
			AssertContainsExactElementsInAnyOrder(
				"FRT line should include the correct charge codes",
				new[] { "CCC" },
				frtLineIncludedCodes
			);

			AssertEquals(
				"BAF line should not include any charge codes",
				0,
				bafLine.IncludedLines.Count);
		}

		public void TestCalculation_ChargeCodeIsLocalAndRelatedLineHasGlobalChargeCode_ShouldAddItselfToIncludedLines()
		{
			var helper = new TestHelper(Factory);
			var globalFRT = helper.ChargeCodes.CreateGlobalCharge("GFRT");
			var localFRT = globalFRT.GetLocalChargeCode(LedgerTypes.AccountsPayable, null);

			Factory.Save();

			TestCalculator.ChargeCode = localFRT.PK;

			var carrier = Factory.New<OrgHeader>();
			var costing = Helper.NewGlobalCosting(carrier);
			var entry = costing.AddRateEntry("LCL", "SEA", "AU", "UA");
			entry.RateLines.RemoveAndDeleteAll();
			var frtLine = entry.AddRateLine(globalFRT);
			var bafLine = entry.AddRateLine("BAF", FlatCalculator.Code);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { entry }), new TestLogger());
			parameters.SetLinesToCalculate(linesRepo);

			TestCalculator.Calculate(parameters);

			var actualFrtIncludedLines = frtLine.IncludedLines.Select(s => (string)s.ChargeCode.AC_Code).ToArray();
			var expectedFrtIncludedLines = new[] { "CCC" };
			AssertContainsExactElementsInAnyOrder(
				"FRT line should include expected charge codes",
				expectedFrtIncludedLines,
				actualFrtIncludedLines
			);

			AssertEquals("BAF line should not include any charge codes", 0, bafLine.IncludedLines.Count);
		}

		public void TestCalculation_ChargeCodeIsGlobalAndRelatedLineHasGlobalChargeCode_ShouldAddItselfToIncludedLines()
		{
			var helper = new TestHelper(Factory);
			var globalFRT = helper.ChargeCodes.CreateGlobalCharge("GFRT");

			Factory.Save();

			TestCalculator.ChargeCode = globalFRT.PK;

			var carrier = Factory.New<OrgHeader>();
			var costing = Helper.NewGlobalCosting(carrier);
			var entry = costing.AddRateEntry("LCL", "SEA", "AU", "UA");
			entry.RateLines.RemoveAndDeleteAll();
			var frtLine = entry.AddRateLine(globalFRT);
			var bafLine = entry.AddRateLine("BAF", FlatCalculator.Code);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { entry }), new TestLogger());
			parameters.SetLinesToCalculate(linesRepo);

			TestCalculator.Calculate(parameters);

			var expectedFRTIncludedLines = new[] { "CCC" };
			var actualFRTIncludedLines = frtLine.IncludedLines.Select(s => (string)s.ChargeCode.AC_Code);

			AssertContainsExactElementsInAnyOrder(
				"frtLine should have exactly one included line with ChargeCode 'CCC'.",
				expectedFRTIncludedLines,
				actualFRTIncludedLines
			);

			AssertEquals("bafLine should have no included lines.", 0, bafLine.IncludedLines.Count);
		}

		public void TestDefaultTM_Text()
		{
			var frtInclusiveCalculatorRateEntry = ClientRate.AddRateEntry("ORG", "FTL", "AU", "");
			frtInclusiveCalculatorRateEntry.RateLines.RemoveAndDeleteAll();
			var frtInclusiveCalculatorRateLine = frtInclusiveCalculatorRateEntry.AddRateLine("ODOC", Code);
			var lineItem = frtInclusiveCalculatorRateLine.RateLineItems.Cast<RateLineItem>().Single();

			AssertEquals("Expected TM_Text to match FreightCalcTypes.Included.", FreightCalcTypes.Included, lineItem.TM_Text);

			lineItem.Validation.ValidateTM_Text();
			AssertEquals("Expected TM_TextInfo.HasErrors() to be false.", false, lineItem.TM_TextInfo.HasErrors());
		}

		public void TestValidateFreightCalcType()
		{
			var frtInclusiveCalculatorRateEntry = ClientRate.AddRateEntry("ORG", "FTL", "AU", "");
			frtInclusiveCalculatorRateEntry.RateLines.RemoveAndDeleteAll();
			var frtInclusiveCalculatorRateLine = frtInclusiveCalculatorRateEntry.AddRateLine("ODOC", Code);
			var lineItem1 = frtInclusiveCalculatorRateLine.RateLineItems.AddNew();
			lineItem1.TM_Text = "";
			AssertEquals("Has Errors", true, lineItem1.TM_TextInfo.HasErrors());
			AssertEquals("Error Message", "Please enter a value.", lineItem1.TM_TextInfo.GetErrors().GetFirstMessage());

			lineItem1.TM_Text = FreightCalcTypes.Included;
			AssertEquals("No Errors", false, lineItem1.TM_TypeInfo.HasErrors());

			lineItem1.TM_Text = FreightCalcTypes.SubjectTo;
			AssertEquals("No Errors", false, lineItem1.TM_TypeInfo.HasErrors());

			lineItem1.TM_Text = FreightCalcTypes.NotApplicable;
			AssertEquals("No Errors", false, lineItem1.TM_TypeInfo.HasErrors());

			lineItem1.TM_Text = "SSD";
			AssertEquals("Has Errors", true, lineItem1.TM_TextInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid selection.", lineItem1.TM_TextInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidateChargeCodes()
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var cafChargeCode = Helper.ChargeCodes["CAF"];
			var fscChargeCode = Helper.ChargeCodes["FSC"];

			var entry = ClientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.RateLines.RemoveAndDeleteAll();

			// 1. BAF => CAF
			var rateLine1 = entry.AddRateLine(bafChargeCode, Code);
			var perCarriageOnCarriageRateLineItem1 = rateLine1.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem1.TM_Type = Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem1.TM_AC = cafChargeCode.PK;

			// After rate line #1 setup, there should not be any charge code errors.
			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!perCarriageOnCarriageRateLineItem1.TM_ACInfo.HasErrors());

			// 2. BAF => blank
			var rateLine2 = entry.AddRateLine(bafChargeCode, Code);

			// After rate line #2 setup, there should not be any charge code errors.
			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!perCarriageOnCarriageRateLineItem1.TM_ACInfo.HasErrors());
			Assert(!rateLine2.TL_ACInfo.HasErrors());

			// 3. CAF => blank
			var rateLine3 = entry.AddRateLine(cafChargeCode, Code);

			const string expectedErrorMessage = "Recursive setup for Freight Inclusive Calculator is not allowed.";

			// After rate line #3 setup, there should be charge code errors.
			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!perCarriageOnCarriageRateLineItem1.TM_ACInfo.HasErrors());
			Assert(!rateLine2.TL_ACInfo.HasErrors());
			Assert(rateLine3.TL_ACInfo.HasError(expectedErrorMessage));

			// 3. CAF => blank turns to FSC => CAF
			rateLine3.TL_AC = fscChargeCode.PK;
			rateLine3.TL_RateCalculator = Code;
			var perCarriageOnCarriageRateLineItem3 = rateLine3.RateLineItems.AddNew();
			perCarriageOnCarriageRateLineItem3.TM_Type = Items.PreCarriageOnCarriageChargeType;
			perCarriageOnCarriageRateLineItem3.TM_AC = cafChargeCode.PK;

			// After rate line #3 modification, there should be no charge code errors.
			Assert(!rateLine1.TL_ACInfo.HasErrors());
			Assert(!perCarriageOnCarriageRateLineItem1.TM_ACInfo.HasErrors());
			Assert(!rateLine2.TL_ACInfo.HasErrors());
			Assert(!rateLine3.TL_ACInfo.HasErrors());
			Assert(!perCarriageOnCarriageRateLineItem3.TM_ACInfo.HasErrors());

			// 3. FSC => CAF turns to FSC => BAF
			perCarriageOnCarriageRateLineItem3.TM_AC = bafChargeCode.PK;

			// After rate line #3 TM_AC modification, there should be charge code errors.
			Assert(rateLine1.TL_ACInfo.HasError(expectedErrorMessage));
			Assert(!perCarriageOnCarriageRateLineItem1.TM_ACInfo.HasErrors());
			Assert(rateLine2.TL_ACInfo.HasError(expectedErrorMessage));
			Assert(!rateLine3.TL_ACInfo.HasErrors());
			Assert(perCarriageOnCarriageRateLineItem3.TM_ACInfo.HasError(expectedErrorMessage));
		}

		public void TestCalculate_LineHasNoCurrency_ItsOk()
		{
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_RateCalculator = Code;
			Line.TL_RX_NKCurrency = ZString.Empty;

			var calc = (FreightInclusiveCalculator)Line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			var (result, error) = calc.Calculate(calcParams);

			AssertEquals(
				"Freight inclusive calculator doesn't have its own result",
				0,
				result.Count()
			);
			AssertNullOrEmpty(
				"Freight inclusive calculator doesn't need currency, so, it doesn't care if it exists",
				error
			);
		}

		public void TestCalculate_LineHasNoCurrency_ShouldFailWhenFreightLineHasNoCurrency()
		{
			Entry.RateLines.RemoveAndDeleteAll();

			var freightLine = Entry.AddFlatRateLine("FRT", 1);
			freightLine.TL_RX_NKCurrency = ZString.Empty;

			var line = Entry.AddRateLine(Helper.ChargeCodes["BAF"], "FRT");
			line.TL_RX_NKCurrency = ZString.Empty;

			var calc = (FreightInclusiveCalculator)line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			var linesRepo = new RateLinesRepository(Criteria, new List<IRateEntry>(new[] { Entry }), new TestLogger());
			calcParams.SetLinesToCalculate(linesRepo);

			var (result, error) = calc.Calculate(calcParams);

			AssertEquals("Freight inclusive calculator doesn't have its own result", 0, result.Count());
			AssertEquals("Related Freight Line has no currency.", "Related Freight Line has no currency.", error);
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(FreightInclusiveCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return Code; }
		}

		protected new FreightInclusiveCalculator TestCalculator
		{
			get { return (FreightInclusiveCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
