
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ICompanyTariff
	{
		ZByte TH_GlobalRateLevel { get; }
		ZString TH_GlobalRateDescription { get; }
		ZPropertyInfo TH_GlobalRateDescriptionInfo { get; }
		MultilingualString TH_GlobalRateDescriptionMultilingual { get; }

		ZGuid TH_GC { get; }

		CodeDescriptionPairList CompanyTariffTypes { get; }
		CodeDescriptionPairList GetCompanyTransportModes(ZString tariffType);
		CodeDescriptionPairList GetApplicableDirections(ZString tariffType);
	}
}
