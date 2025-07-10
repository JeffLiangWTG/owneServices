using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureCargoDescPhase5Lookups : EU.NCTS.Business.NctsDepartureCargoDescPhase5Lookups, INctsDepartureCargoDescLookups
	{
		public NctsDepartureCargoDescPhase5Lookups(NctsDepartureCargoDesc parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ExportDeclarationTypeList => Factory.GetCachedValue<NctsDeclarationTypeList>();
		ConsigneeCollection INctsDepartureCargoDescLookups.ConsigneeList => base.ConsigneeList;
		ZZRefCusCodeListCombinedCollection INctsDepartureCargoDescLookups.CusCodeList => base.CusCodeList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.CountryOfDispatchList => base.CountryOfDispatchList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.CountryOfDestinationList => base.CountryOfDestinationList;
		RefCurrencyCollection INctsDepartureCargoDescLookups.LinePriceCurrencies => base.LinePriceCurrencies;
		RefCurrencyCollection INctsDepartureCargoDescLookups.Currencies => base.Currencies;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.CustomsUnitOfQuantityList => base.CustomsUnitOfQuantityList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.TaxOrFeeCodeList => base.TaxOrFeeCodeList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.WeightUnitList => base.WeightUnitList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.DeclarationTypeList => base.DeclarationTypeList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.CountryOfOriginList => base.CountryOfOriginList;
		CodeDescriptionPairList INctsDepartureCargoDescLookups.TransportChargesModeOfPaymentList => base.TransportChargesModeOfPaymentList;
	}
}
