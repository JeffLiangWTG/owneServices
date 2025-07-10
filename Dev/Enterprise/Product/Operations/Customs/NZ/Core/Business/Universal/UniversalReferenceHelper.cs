using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.NZ.Business;

public static class UniversalReferenceHelper
{
	public static ZDecimal GetTaxOrFee(BusinessObjectFactory factory, ZString rateOrFeeCode, ZDateTime valuationDate)
	{
		return new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.NewZealand, rateOrFeeCode, valuationDate)?.ZZF_Value ?? ZDecimal.Zero;
	}

	public static ZDecimal GetTaxOrFee(this RefCusTaxOrFee.Loader loader, ZString rateOrFeeCode, ZDateTime valuationDate)
	{
		return loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.NewZealand, rateOrFeeCode, valuationDate)?.ZZF_Value ?? ZDecimal.Zero;
	}

	public static CodeDescriptionPairList GetUNEPackageTypeList(BusinessObjectFactory factory)
	{
		return RefCusCodeListTypes.GetCachedListValidBeforeDate(factory, Constants.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Constants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);
	}

	public static bool UNEPackageTypeIsBulk(BusinessObjectFactory factory, ZString code)
	{
		return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory,
			Constants.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Constants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			UniversalReferenceConstants.UNPackTypeStartDate,
			matchIfAttributeNotExists: false,
			Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk).ContainsCode(code);
	}
}
