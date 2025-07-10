using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDebtorGroupBankDefaultValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP6_ABIsNotEmpty()
		{
			OrgDebtorGroupBankDefault bankDefault = Factory.NewWithValidTestData<OrgDebtorGroupBankDefault>();
			bankDefault.P6_OverrideRegistryCurrencyToBankSetting = true;
			bankDefault.P6_AB = ZGuid.Empty;
			AssertHasError("Must have error", bankDefault.P6_ABInfo, "Please enter a Bank Account.");

			bankDefault.P6_OverrideRegistryCurrencyToBankSetting = false;
			bankDefault.P6_AB = ZGuid.Empty;
			AssertNoErrors("Must not have error", bankDefault.P6_ABInfo);
		}
	}
}
