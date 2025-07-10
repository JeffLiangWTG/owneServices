using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsPackageConfiguration : EU.NCTS.Business.NctsPackageConfiguration
{
	protected override INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider()
		=> new NctsPackageDeparturePhase5ValidationDecider();

	protected override INctsPackagePhase5ValidationDecider GetArrivalPhase5ValidationDecider()
		=> new NctsPackageArrivalPhase5ValidationDecider();
}

