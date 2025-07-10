using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PartialTariffValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidPartialTariff()
		{
			Assert(!PartialTariffValidator.IsValidPartialTariff(Factory, USCTariff.TariffNumberNotInDatabase.Substring(0, 4)));
			Assert(PartialTariffValidator.IsValidPartialTariff(Factory, USCTariff.FDAPriorNoticeRequiredTariff.Substring(0, 4)));
		}
	}
}
