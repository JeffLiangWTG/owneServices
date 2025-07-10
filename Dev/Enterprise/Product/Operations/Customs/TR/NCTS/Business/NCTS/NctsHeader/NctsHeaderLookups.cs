using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StampDutyStatusCodeList => Factory.GetCachedValue<StampDutyStatusCodeList>();
	}
}
