using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public interface INctsPreviousDocumentLookups
	{
		CodeDescriptionPairList IncotermList { get; }
		ZZRefCusCodeListCombinedCollection PreviousDocumentsCodeList { get; }
		RefCurrencyCollection Currencies { get; }
		CodeDescriptionPairList WeightUQList { get; }
		CodeDescriptionPairList NatureOfBusinessList { get; }
		CodeDescriptionPairList SubTypeList { get; }
		RefCountryCollection Countries { get; }
		CodeDescriptionPairList UnitOfQuantityList { get; }
	}
}
