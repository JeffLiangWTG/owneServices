using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyTaxFeeChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var dutyTaxFeeCharge = new DutyTaxFeeCharge(Factory.New<CusEntryHeader>());
			AssertType<DutyTaxFeeChargeValidation>(dutyTaxFeeCharge.Validation);
		}
	}
}
