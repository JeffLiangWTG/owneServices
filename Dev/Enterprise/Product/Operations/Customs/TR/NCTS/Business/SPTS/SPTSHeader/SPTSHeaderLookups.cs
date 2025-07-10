using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSHeaderLookups : CusInBondHeaderLookups
	{
		public SPTSHeaderLookups(SPTSHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SPTSTransportModes => Factory.GetCachedValue<SPTSTransportModeList>();

		public CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<SPTSMessageStatusList>();
	}
}
