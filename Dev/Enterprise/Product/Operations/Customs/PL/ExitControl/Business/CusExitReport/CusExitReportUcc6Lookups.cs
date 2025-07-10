using Enterprise.Customs.Common.PL;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitReportUcc6Lookups(AutoCusExitReport parent) : Enterprise.Customs.EU.ExitControl.Business.CusExitReportUcc6Lookups(parent)
{
	protected override CodeDescriptionPairList StatusListCore => Factory.GetCachedValue<AESEntryStatusList>();
}
