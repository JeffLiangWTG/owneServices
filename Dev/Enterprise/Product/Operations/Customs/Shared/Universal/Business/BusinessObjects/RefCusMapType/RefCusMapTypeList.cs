using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusMapTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string REL = "REL";
			public const string ZADOC = "ZADOC";
			public const string Preference = "PREF";
			public const string CTYPE = "CTYPE";
			public const string STYPE = "STYPE";
			public const string EXPSTA = "EXPST";
			public const string IMPSTA = "IMPST";
			public const string CMODE = "CMODE";
			public const string MSELT = "MSELT";
			public const string HAFEE = "HAFEE";
			public const string EUCTY = "EUCTY";
			public const string TWI2C = "TWI2C";
			public const string Country = "CNTRY";
			public const string Currency = "CURR";
			public const string FRDTY = "FRDTY";
			public const string FRCCS = "FRCCS";
			public const string BORDERWISE = "BOR";
			public const string PNTSS = "PNTSS";
			public const string RCODE = "RCODE";
			public const string AUTDC = "AUTDC";
			public const string ChargeCode = "CHG";
			public const string MUQCO = "MUQCO";
			public const string VUQCO = "VUQCO";
			public const string RateType = "RATET";
			public const string STATE = "STATE";
		}

		public static RefCusMapTypeList GetList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UniveralRefCusMapTypeList", () =>
			{
				var result = new RefCusMapTypeList();
				var query = new ZQuery
				{
					OrderBy = RefCusMapTypeSchema.Constants.ZZP_MapType
				};
				var types = factory.Load<RefCusMapType>(query);
				if (types != null)
				{
					result.AddRange(types);
				}
				return result;
			});
		}

		public static RefCusMapTypeList GetListOfEditableTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UniveralRefCusMapTypeList_Readonly", () =>
			{
				var result = new RefCusMapTypeList();
				var query = new ZQuery(RefCusMapTypeSchema.ZZP_IsReadonly, ZBool.False)
				{
					OrderBy = RefCusMapTypeSchema.Constants.ZZP_MapType
				};
				var types = factory.Load<RefCusMapType>(query);
				if (types != null)
				{
					result.AddRange(types);
				}
				return result;
			});
		}
	}
}
