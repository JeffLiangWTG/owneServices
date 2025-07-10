using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Business;
	using NUnit.Framework;

	class GSTCalculatorTest : TestCaseWithFactory
	{
		public void TestIsDutyOnlyGST()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			testObjects.InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;
			testObjects.Declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			var gSTCalc = new GSTCalculator(testObjects.EntryLine);
			Assert(testObjects.InvoiceLine.IsDutyOnlyGST);
			AssertEquals("GSTCalc.GSTAmount", 0m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);

			testObjects.InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.No;
			gSTCalc = new GSTCalculator(testObjects.EntryLine);
			Assert(!testObjects.InvoiceLine.IsDutyOnlyGST);
			AssertEquals("GSTCalc.GSTAmount", 451.50m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);

			testObjects.InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.Yes;
			gSTCalc = new GSTCalculator(testObjects.InvoiceLine);
			Assert(testObjects.InvoiceLine.IsDutyOnlyGST);
			AssertEquals("GSTCalc.GSTAmount", 30.00m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);

			testObjects.InvoiceHeader.JZ_IsGSTPrePaid = YesNoList.Codes.No;
			gSTCalc = new GSTCalculator(testObjects.InvoiceLine);
			Assert(!testObjects.InvoiceLine.IsDutyOnlyGST);
			AssertEquals("GSTCalc.GSTAmount", 481.50m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);
		}

		[TestDate(2010, 10, 17)]
		public void TestGSTGetsNewRateOnInvoiceLinesAfterOctober2010()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 17);
			AssertEquals("Precondition: InvoiceLine.DateForDutyRate", new ZDateTime(2010, 10, 17), testObjects.InvoiceLine.DateForDutyRate);
			testObjects.InvoiceLine.JI_ConcessionCode = "100101J";
			GSTCalculator gSTCalc = new GSTCalculator(testObjects.EntryLine);
			AssertEquals("GSTCalc.GSTAmount", 451.50m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);
		}

		[TestDate(2010, 12, 17)]
		public void TestGSTGetsNewRateOnEntryLinesAfterOctober2010()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_DateOfArrival = new ZDateTime(2010, 12, 17);
			AssertEquals("Precondition: InvoiceLine.DateForDutyRate", new ZDateTime(2010, 12, 17), testObjects.InvoiceLine.DateForDutyRate);
			GSTCalculator gSTCalc = new GSTCalculator(testObjects.InvoiceLine);
			AssertEquals("GSTCalc.GSTAmount", 481.50m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.15m, gSTCalc.GSTRate);
		}

		public void TestZeroRatingFlagWorksOnUsingJobComInvoiceLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_IsZeroRatedAll = YesNoList.Codes.Yes;

			GSTCalculator gSTCalc = new GSTCalculator(testObjects.InvoiceLine);

			AssertEquals("GSTCalc.GSTAmount", 0.00m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.00m, gSTCalc.GSTRate);
		}

		public void TestZeroRatingFlagWorksOnDeclarationUsingCusEntryLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.Declaration.JE_IsZeroRatedAll = YesNoList.Codes.Yes;

			GSTCalculator gSTCalc = new GSTCalculator(testObjects.EntryLine);

			AssertEquals("GSTCalc.GSTAmount", 0.00m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.00m, gSTCalc.GSTRate);
		}

		[TestDate(2010, 07, 11)]
		public void TestPublicInterfaceUsingCusEntryLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			testObjects.InvoiceLine.JI_ConcessionCode = "100101J";
			GSTCalculator gSTCalc = new GSTCalculator(testObjects.EntryLine);
			AssertEquals("GSTCalc.GSTAmount", 376.25m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.125m, gSTCalc.GSTRate);
		}

		[TestDate(2010, 04, 17)]
		public void TestPublicInterfaceUsingJobComInvoiceLine()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			GSTCalculator gSTCalc = new GSTCalculator(testObjects.InvoiceLine);
			AssertEquals("GSTCalc.GSTAmount", 420.00m, gSTCalc.GSTAmount);
			AssertEquals("GSTCalc.GSTRate", 0.125m, gSTCalc.GSTRate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp(Factory);
		}
	}
}
