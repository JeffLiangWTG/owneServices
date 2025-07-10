using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TariffCheckerTest : TestCaseWithFactory
	{
		public void TestMayRequireFCC()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			AssertEquals(false, TariffChecker.HasSpecifiedCode("FD1", tariff.OGARequirements));
			tariff.UE_OGACodes = "   FC3";
			AssertEquals(true, TariffChecker.HasSpecifiedCode("FC3", tariff.OGARequirements));
			AssertEquals(false, TariffChecker.HasSpecifiedCode("FC3", tariff.PGARequirements));

			tariff.UE_PGACodes = "   DT2";
			AssertEquals(true, TariffChecker.HasSpecifiedCode("DT2", tariff.PGARequirements));
		}
	}
}
