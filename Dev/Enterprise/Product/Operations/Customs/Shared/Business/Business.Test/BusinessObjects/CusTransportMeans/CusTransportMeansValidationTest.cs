using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPM_TransportState()
		{
			CombineAssertions(() =>
			{
				parent.TPM_TransportState = "N/A";
				AssertHasErrorContaining("Invalid Code according to DB Constraints", parent.TPM_TransportStateInfo, ListValidation.InvalidCodeError);

				parent.TPM_TransportState = CusUnloadedStateList.Codes.DEC;
				AssertNoErrors("no error", parent.TPM_TransportStateInfo);

				parent.TPM_TransportState = CusUnloadedStateList.Codes.DIF;
				AssertNoErrors("no error", parent.TPM_TransportStateInfo);

				parent.TPM_TransportState = CusUnloadedStateList.Codes.MIS;
				AssertNoErrors("no error", parent.TPM_TransportStateInfo);

				parent.TPM_TransportState = CusUnloadedStateList.Codes.NEW;
				AssertNoErrors("no error", parent.TPM_TransportStateInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<CusTransportMeans>();
		}
		CusTransportMeans parent;
	}
}
