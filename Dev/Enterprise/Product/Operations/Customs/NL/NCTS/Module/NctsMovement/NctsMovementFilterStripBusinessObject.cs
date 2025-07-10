using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Module;

public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
{
	protected override CodeDescriptionPairList NctsArrivalStatusListPhase5Core => Factory.GetCachedValue<NLNCTS5ArrivalCustomsStatusList>();
}
