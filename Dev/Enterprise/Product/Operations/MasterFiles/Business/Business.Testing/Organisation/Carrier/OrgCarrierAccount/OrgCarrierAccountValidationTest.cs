using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCarrierAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOAN_AccountNumber()
		{
			var carrier = Factory.New<OrgHeader>();
			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = carrier.PK;
			var can2 = Factory.New<OrgCarrierAccount>();
			can2.OAN_OH_Carrier = carrier.PK;

			can1.OAN_AccountNumber = "ABC";
			can2.OAN_AccountNumber = "XYZ";

			AssertNoErrors("Precondition", can1.OAN_AccountNumberInfo);
			AssertNoErrors("Precondition", can2.OAN_AccountNumberInfo);

			can2.OAN_AccountNumber = "ABC";
			AssertHasErrors(can2.OAN_AccountNumberInfo);
		}
	}
}
