using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class CalculationResultTest : TestCaseWithFactory
	{
		#region TestFreightChargeCodeCalculationLog

		public void TestFreightChargeCodeCalculationLog()
		{
			var calculatorOutput = new CalculatorOutput(null, new Integration.CalculationLog());
			calculatorOutput.CalculationLog.CalculatorCode = "XXX";
			var result = new CalculationResult(null, calculatorOutput);
			AssertNull(result.FreightChargeCodeCalculationLog);

			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			rateLine.TL_AC = Factory.New<AccChargeCode>().PK;

			result = new CalculationResult(rateLine, calculatorOutput);
			AssertNull("Calculation log not copied as rateline's charge code is not FRT", result.FreightChargeCodeCalculationLog);

			rateLine.TL_AC = Env.Registry.FreightChargeCode;
			result = new CalculationResult(rateLine, calculatorOutput);
			AssertEquals("Calculation log copied from calculator", "XXX", result.FreightChargeCodeCalculationLog.CalculatorCode);
		}

		#endregion

		#region Description List

		#region TestCartageZoneDescription

		public void TestCartageZoneDescription()
		{
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			AssertEquals(null, result.CartageZoneDescription);

			result = new CalculationResult(rateLine, new CalculatorOutput(new List<PaymentBasis> { criteria.CreatePaymentBasis(RateInfo.CreateFLT(10, "UAH", null, "ZONE"), default) }));
			AssertEquals("ZONE", result.CartageZoneDescription);
		}

		#endregion

		#endregion

		#region TestAddAttributesShouldNotInsertDuplicateValues

		public void TestNoDuplicateAttributes()
		{
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, new TestRatingCriteria());
			result.AddAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "10");
			result.AddAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "10");

			var expectedAttributes1 = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "10")
			};

			AssertContainsExactElementsInAnyOrder(expectedAttributes1, result.Attributes.Attributes);

			result.AddAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "8");

			var expectedAttributes2 = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "8"),
				new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, "10")
			};

			AssertContainsExactElementsInAnyOrder(expectedAttributes2, result.Attributes.Attributes);
		}

		#endregion

		#region Helper

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}
