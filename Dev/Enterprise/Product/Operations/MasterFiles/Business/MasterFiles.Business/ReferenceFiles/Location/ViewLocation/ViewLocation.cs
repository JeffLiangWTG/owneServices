using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ViewLocation : AutoViewLocation
	{
		public ViewLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocationType
		{
			get
			{
				switch (VLO_TableCode)
				{
					case RefUNLOCOSchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.UNLOCO;
					case RefCountrySchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.Country;
					case RefCountryStatesSchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.State;
					case RefCityTownSchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.City;
					case RefZoneHeaderSchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.InternationalZone;
					case RateTransportZonesSchema.Constants.Prefix:
						return ViewLocationTypeList.Codes.TransportZone;
					default:
						return ZString.Empty;
				}
			}
		}

		public RefCountry Country
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, VLO_CountryCode); }
		}

		public ZString UnlocoCode
		{
			get { return IsUNLOCO ? VLO_Code : ZString.Empty; }
		}

		public bool IsUNLOCO
		{
			get { return VLO_TableCode == RefUNLOCOSchema.Constants.Prefix; }
		}

		public bool IsCountry
		{
			get { return VLO_TableCode == RefCountrySchema.Constants.Prefix; }
		}

		public bool IsState
		{
			get { return VLO_TableCode == RefCountryStatesSchema.Constants.Prefix; }
		}

		public bool IsCityTown
		{
			get { return VLO_TableCode == RefCityTownSchema.Constants.Prefix; }
		}

		public bool IsInternationalZone
		{
			get { return VLO_TableCode == RefZoneHeaderSchema.Constants.Prefix; }
		}

		public bool IsTransportZone
		{
			get { return VLO_TableCode == RateTransportZonesSchema.Constants.Prefix; }
		}

		#region Contains

		public bool Contains(OrgHeader org)
		{
			if (IsUNLOCO)
			{
				return org.OH_RL_NKClosestPort.EqualsIgnoringCase(VLO_Code);
			}
			else if (IsCountry)
			{
				return org.CountryCode.EqualsIgnoringCase(VLO_Code);
			}
			else if (IsState)
			{
				return org.CountryCode.EqualsIgnoringCase(VLO_CountryCode) && org.MainAddress.OA_State.EqualsIgnoringCase(VLO_Code);
			}
			else if (IsCityTown)
			{
				return org.CountryCode.EqualsIgnoringCase(VLO_CountryCode) && org.MainAddress.OA_State.EqualsIgnoringCase(VLO_StateCode) && org.MainAddress.OA_City.EqualsIgnoringCase(VLO_Code);
			}
			else if (IsInternationalZone)
			{
				var zonePivotQuery = new ZQuery(RefZonePivotSchema.F2_FZ, PK);
				var countryOrUnlocoQuery = new ZQuery();
				var orgHasCountryOrUnloco = false;

				if (org.Country != null)
				{
					countryOrUnlocoQuery.AddToFilter(JoinCondition.Or, RefZonePivotSchema.F2_ParentID, org.Country.PK);
					orgHasCountryOrUnloco = true;
				}

				if (org.UNLOCO != null)
				{
					countryOrUnlocoQuery.AddToFilter(JoinCondition.Or, RefZonePivotSchema.F2_ParentID, org.UNLOCO.PK);
					orgHasCountryOrUnloco = true;
				}

				zonePivotQuery.AddToFilter(countryOrUnlocoQuery);
				return orgHasCountryOrUnloco && Factory.LoadTop1<RefZonePivot>(zonePivotQuery) != null;
			}
			else if (IsTransportZone)
			{
				return org.CountryCode.EqualsIgnoringCase(VLO_CountryCode);
			}

			return false;
		}

		public bool Contains(ViewLocation anotherLocation)
		{
			if (anotherLocation != null)
			{
				if (IsCountry)
				{
					if (anotherLocation.IsUNLOCO || anotherLocation.IsState || anotherLocation.IsCityTown || anotherLocation.IsTransportZone)
					{
						return anotherLocation.VLO_CountryCode.EqualsIgnoringCase(VLO_Code);
					}
				}
				else if (IsState)
				{
					if (anotherLocation.IsUNLOCO || anotherLocation.IsCityTown)
					{
						return anotherLocation.VLO_StateCode.EqualsIgnoringCase(VLO_Code) && anotherLocation.VLO_CountryCode.EqualsIgnoringCase(VLO_CountryCode);
					}
				}
				else if (IsInternationalZone)
				{
					if (anotherLocation.IsCountry || anotherLocation.IsUNLOCO)
					{
						var zonePivotQuery = new ZQuery(RefZonePivotSchema.F2_FZ, PK);
						zonePivotQuery.AddToFilter(RefZonePivotSchema.F2_ParentID, anotherLocation.PK);
						return Factory.LoadTop1<RefZonePivot>(zonePivotQuery) != null;
					}
				}
				else if (IsTransportZone)
				{
					if (anotherLocation.IsCityTown)
					{
						var zoneItemQuery = new ZQuery(RateTransportZoneItemSchema.TQ_TZ_DomesticZone, this.PK);
						zoneItemQuery.AddToFilter(RateTransportZoneItemSchema.TQ_R9_CityTown, anotherLocation.PK);
						var bizObjType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportZoneItemSchema.Constants.Prefix);
						return Factory.LoadTop1(bizObjType, zoneItemQuery) != null;
					}
				}
			}

			return false;
		}

		#endregion
	}
}
