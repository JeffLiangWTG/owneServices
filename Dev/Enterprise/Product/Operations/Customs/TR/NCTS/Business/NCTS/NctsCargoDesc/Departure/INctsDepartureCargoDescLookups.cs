using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public interface INctsDepartureCargoDescLookups
	{
		CodeDescriptionPairList WeightUnitList { get; }
		CodeDescriptionPairList DeclarationTypeList { get; }
		CodeDescriptionPairList CountryOfDispatchList { get; }
		CodeDescriptionPairList CountryOfDestinationList { get; }
		CodeDescriptionPairList CountryOfOriginList { get; }
		CodeDescriptionPairList TransportChargesModeOfPaymentList { get; }
		CodeDescriptionPairList ExportDeclarationTypeList { get; }
		ConsigneeCollection ConsigneeList { get; }
		ZZRefCusCodeListCombinedCollection CusCodeList { get; }
		RefCurrencyCollection LinePriceCurrencies { get; }
		RefCurrencyCollection Currencies { get; }
		CodeDescriptionPairList CustomsUnitOfQuantityList { get; }
		CodeDescriptionPairList TaxOrFeeCodeList { get; }
		CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions { get; }
		TariffViewCollection Tariffs { get; }
		CodeDescriptionPairList AdditionalCodeList { get; }
		Customs.Business.OrgSupplierPartCollection Parts { get; }
		CodeDescriptionPairList BondedWhsUnitQtyList { get; }
	}
}
