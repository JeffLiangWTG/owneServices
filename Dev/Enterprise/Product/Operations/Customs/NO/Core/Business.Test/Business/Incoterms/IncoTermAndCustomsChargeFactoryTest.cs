using System.IO;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public void TestChargeCodeOverseasFreight()
		{
			var chargeCode = incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight);
			CombineAssertions(() =>
			{
				AssertEquals("IsDutiable", expected: true, chargeCode.IsDutiable);
				AssertEquals("IsDutiableDeemedForThisCharge", expected: true, chargeCode.IsDutiableDeemedForThisCharge);
			});
		}

		public void TestChargeCodeOverseasInsurance()
		{
			var chargeCode = incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance);
			CombineAssertions(() =>
			{
				AssertEquals("IsDutiable", expected: true, chargeCode.IsDutiable);
				AssertEquals("IsDutiableDeemedForThisCharge", expected: true, chargeCode.IsDutiableDeemedForThisCharge);
			});
		}

		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 12 charges", 12, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeCodeProvider.OverseasFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeCodeProvider.OverseasInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NO\Core\Business.Test\Business\IncoTerms\Testing\NOIncoTermAndCustomsChargeConfiguration.csv");
	}
}
