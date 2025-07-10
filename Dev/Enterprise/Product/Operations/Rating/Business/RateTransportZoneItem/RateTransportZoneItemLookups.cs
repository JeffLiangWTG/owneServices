using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateTransportZoneItemLookups : AutoRateTransportZoneItemLookups
	{
		public RateTransportZoneItemLookups(AutoRateTransportZoneItem parent)
			: base(parent)
		{
		}

		RateTransportZoneItem ZoneItem => (RateTransportZoneItem)Parent;
		ZString CountryCode => ZoneItem.TQ_RN_NKCountry;

		#region PostCodes

		public RefPostCodeCollection PostCodes => Factory.GetCachedValue("PostCodes" + ZoneItem.TQ_R9_CityTown + CountryCode, GetPostCodeCollection);

		RefPostCodeCollection GetPostCodeCollection()
		{
			if (!ZoneItem.TQ_R9_CityTown.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(RefPostCode));
				var subQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_RK);
				subQuery.AddToFilter(RefCityPCodePivotSchema.R0_R9, ZoneItem.TQ_R9_CityTown);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return new RefPostCodeCollection(Factory, query, CountryCode, ZoneItem.CityTown?.R9_InternationalName, ZoneItem.CityTown?.R9_RW_NKState);
			}

			if (!string.IsNullOrEmpty(CountryCode))
			{
				return PostCodesByCountry;
			}

			return new RefPostCodeCollection(Factory, new ZQuery());
		}

		RefPostCodeCollection PostCodesByCountry => Factory.GetCachedValue("PostCodesByCountry" + CountryCode, () =>
		{
			var postCodeCollection = new RefPostCodeCollection(Factory, new ZQuery(), CountryCode, ZoneItem.CityTown?.R9_InternationalName, ZoneItem.CityTown?.R9_RW_NKState);
			postCodeCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", CountryCode));

			return postCodeCollection;
		});

		#endregion

		#region CityTowns

		public override RefCityTownCollection CityTowns => Factory.GetCachedValue("CityTowns" + ZoneItem.TQ_FromPostCode + CountryCode, GetCityTownCollection);

		RefCityTownCollection GetCityTownCollection()
		{
			var refPostCode = ZoneItem.FromPostCode;
			if (refPostCode != null)
			{
				var query = new ZDBOnlyQuery(typeof(RefCityTown));
				var subQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_R9);
				subQuery.AddToFilter(RefCityPCodePivotSchema.R0_RK, refPostCode.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return new RefCityTownCollection(Factory, query, CountryCode, ZoneItem.TQ_FromPostCode);
			}

			if (!string.IsNullOrEmpty(CountryCode))
			{
				return CityTownsByCountry;
			}

			return new RefCityTownCollection(Factory, new ZQuery());
		}

		RefCityTownCollection CityTownsByCountry => Factory.GetCachedValue("CityTownsByCountry" + CountryCode, () =>
		{
			var cityTownCollection = new RefCityTownCollection(Factory, new ZQuery(), CountryCode, ZoneItem.TQ_FromPostCode);
			cityTownCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CountryState", "Property1", CountryCode));

			return cityTownCollection;
		});

		#endregion
	}
}

