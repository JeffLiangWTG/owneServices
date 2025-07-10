using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_LineNo()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(cargoDesc.BY_LineNoInfo);
		}

		public void TestCheckBY_NetWeight_CannotBeNegative()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(cargoDesc.BY_NetWeightInfo);
		}

		public void TestCheckBY_NetWeightUnit_MustBeEnteredIfNetWeightIsEntered()
		{
			cargoDesc.BY_NetWeight = 1m;
			ValidationTestHelper.AssertErrorIfNotEntered(cargoDesc.BY_NetWeightUnitInfo);
		}

		public void TestCheckBY_NetWeightUnit_NotMandatory()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(cargoDesc.BY_NetWeightUnitInfo);
		}

		public void TestCheckBY_TransportChargesMethodOfPayment()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(cargoDesc.BY_TransportChargesMethodOfPaymentInfo, "~", TransportChargesModeOfPayment.Codes.AccountHolderWithCarrier);
		}

		public void TestCheckBY_CustomsSecondQuantity()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(cargoDesc.BY_CustomsSecondQuantityInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cargoDesc = Factory.New<CusInBondCargoDescForTest>();
		}
		CusInBondCargoDesc cargoDesc;
	}
}
