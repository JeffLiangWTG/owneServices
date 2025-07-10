using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Module
{
	public class RefHarbourRateFilterLookup : CommonFilterLookups
	{
		public RefHarbourRateFilterLookup(RefHarbourRateFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList ModeList => Factory.GetCachedValue<RefHarbourRateModeList>();
	}
}
