using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PartialPGARequirementIndicatorTest : TestCaseWithFactory
	{
		public void TestHasRequirement()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1020304010";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "   EP2EP4FS4EP6FD2FD4";

			var supTariff = Factory.New<USCTariff>();
			supTariff.UE_Tariff = "1010101010";
			supTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			supTariff.UE_DateTo = ZDateTime.Today;
			supTariff.UE_PGACodes = "FD2EP4AL2";

			AssertEquals(false, PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { tariff, supTariff }, x => x.DoesRequireDOT));
			AssertEquals(true, PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { tariff, supTariff }, x => x.DoesRequireACEFDA));
			AssertEquals(true, PartialPGARequirementIndicator.HasRequirement(new USCTariff[] { tariff, supTariff }, x => x.DoesRequireFSIS));
		}
	}
}
