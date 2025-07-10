using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsExportToOpenLookups : CusSupportingInfoLookups
	{
		public NctsExportToOpenLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<DeclarationTypeList>();
	}
}
