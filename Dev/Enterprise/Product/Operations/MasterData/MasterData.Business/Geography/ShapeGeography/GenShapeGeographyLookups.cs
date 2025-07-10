using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class GenShapeGeographyLookups : AutoGenShapeGeographyLookups
	{
		public GenShapeGeographyLookups(AutoGenShapeGeography parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SHG_ParentTableCode_List
		{
			get { return Factory.GetCachedValue("GenShapeGeographyLookups|SHG_ParentTableCode_List", () => SHG_ParentTableCode_ListCore); }
		}

		public object SHG_ParentID_List
		{
			get
			{
				object result;

				switch (((GenShapeGeography)Parent).SHG_ParentTableCode)
				{
					case RefCountrySchema.Constants.Prefix:
						result = RefCountries;
						break;
					case RefCityTownSchema.Constants.Prefix:
						result = RefCityTowns;
						break;
					default:
						result = RefZoneHeaders;
						break;
				}

				return result;
			}
		}

		public CodeDescriptionPairList GeographyTypeList
		{
			get
			{
				return Factory.GetCachedValue("GenShapeGeographyLookups.GeographyTypeList", () => GeographyType.CodePairList);
			}
		}

		CodeDescriptionPairList SHG_ParentTableCode_ListCore
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(RefCountrySchema.Constants.Prefix, Res.GetString("E05ECE75-1CD6-4061-A27A-AD50F60FDA67", "Country/Region"));
				list.AddPair(RefCityTownSchema.Constants.Prefix, Res.GetString("0CEA97E4-7654-46B4-A768-FF56B3B2FB96", "City/Town"));
				list.AddPair(RefZoneHeaderSchema.Constants.Prefix, Res.GetString("AD7F7251-8138-40B5-AC37-1AA462C4692D", "Zone"));

				return list;
			}
		}

		RefCountryCollection RefCountries
		{
			get { return Factory.GetCachedValue("GenShapeGeographyLookups|RefCountries", () => new RefCountryCollection(Factory)); }
		}

		RefCityTownCollection RefCityTowns
		{
			get { return Factory.GetCachedValue("GenShapeGeographyLookups|RefCityTowns", () => new RefCityTownCollection(Factory)); }
		}

		RefZoneHeaderCollection RefZoneHeaders
		{
			get { return Factory.GetCachedValue("GenShapeGeographyLookups|RefZoneHeaders", () => new RefZoneHeaderCollection(Factory)); }
		}
	}
}
