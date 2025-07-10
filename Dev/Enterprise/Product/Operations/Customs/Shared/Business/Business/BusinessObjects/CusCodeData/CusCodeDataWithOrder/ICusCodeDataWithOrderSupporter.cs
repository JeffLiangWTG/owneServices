using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public interface ICusCodeDataWithOrderSupporter
	{
		TariffView Tariff { get; }
		IZZRateSelectionCriteria RateSelectionCriteria { get; }
		ZString GetCountryCodeForCodeProvider();
		CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions { get; }
		void OnCodesChanged();
	}
}
