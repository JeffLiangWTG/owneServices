using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DA63AdditionalDutyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var duty = Factory.New<DA63AdditionalDuty>();
			AssertEquals("12A, 12B, 13A, 13B, 13C, 13D, 15A, 15B, 2P1, 2P2, 2P3, FOR, PEN, PPA, PPC, PPE, PPG, PPR, PPT", duty.Lookups.CY_CodeList.CodesAsString);
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
		}
	}
}
