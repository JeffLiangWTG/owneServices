using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class PlaceOfSupplyHelper
	{
		public static ILocation TryConvertToLocation(GlbCompany company, string placeOfSupply)
		{
			ILocation location = null;

			if (company != null && !string.IsNullOrEmpty(placeOfSupply))
			{
				var factory = company.Factory;
				var placeOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, placeOfSupply);
				var key = FormattableString.Invariant($"{company.Country.RN_Code}|{placeOfSupplyType}|{placeOfSupply}");
				location = factory.GetCachedValue<ILocation>(key, () =>
				{
					if (placeOfSupplyType == PlaceOfSupplyTypes.State.Code)
					{
						var query = new ZQuery(RefCountryStatesSchema.RW_Code, placeOfSupply);
						query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, company.GC_RN_NKCountryCode);
						return company.Factory.LoadTop1<RefCountryStates>(query);
					}
					else if (placeOfSupplyType == PlaceOfSupplyTypes.TaxZone.Code)
					{
						var query = new ZQuery(RefZoneHeaderSchema.FZ_Code, placeOfSupply);
						query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.Tax);
						return company.Factory.LoadTop1<RefZoneHeader>(query);
					}
					else if (placeOfSupplyType == PlaceOfSupplyTypes.Country.Code)
					{
						var query = new ZQuery(RefCountrySchema.RN_Code, placeOfSupply);
						return company.Factory.LoadTop1<RefCountry>(query);
					}
					else if (placeOfSupplyType == PlaceOfSupplyTypes.PredefinedRule.Code)
					{
						return new LocationRule(company, placeOfSupply);
					}

					return null;
				});
			}

			return location;
		}

		public static bool IsPlaceOfSupplyEnabled(GlbCompany company = null)
		{
			var companyOrCurrent = company ?? Env.CurrentCompany;
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.GetValueWithoutFallback(companyOrCurrent.PK, Guid.Empty, Guid.Empty);
			var enabledTypes = posTypes.GetActiveCodeDescriptionPairList();
			return enabledTypes.Count > 0;
		}

		public static string[] GetEnabledPlaceOfSupplyCodes(GlbCompany company = null)
		{
			var companyOrCurrent = company ?? Env.CurrentCompany;
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.GetValueWithoutFallback(companyOrCurrent.PK, Guid.Empty, Guid.Empty);
			return posTypes.GetActiveCodeDescriptionPairList().GetAllCodes();
		}

#if DEBUG
		public static IDisposable SetPOSTypesEnabled_ForTestOnly(params string[] posTypesToEnable) => SetPOSTypesEnabled_ForTestOnly(null, posTypesToEnable);

		public static IDisposable SetPOSTypesEnabled_ForTestOnly(GlbCompany company, params string[] posTypesToEnable)
		{
			var companyPK = company?.PK.ToGuid() ?? Env.CurrentCompanyPK;
			var newValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			posTypesToEnable.Where(x => newValue.ContainsCode(x)).ForEach(x => newValue.Set(x, true));

			return AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, newValue);
		}
#endif
	}
}
