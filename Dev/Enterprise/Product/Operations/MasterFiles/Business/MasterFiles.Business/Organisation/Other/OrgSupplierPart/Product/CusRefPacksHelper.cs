using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class CusRefPacksHelper
	{
		public static CusRefPacks LoadRefPack(BusinessObjectFactory factory, ZString packUQ, ZString type, ZString country, bool useFallback = true, string customsPack = "")
		{
			CusRefPacks result = null;
			var refPacks = LoadFilteredRefPacks(factory, country, type, packUQ, useFallback, customsPack: customsPack);

			if (refPacks.Count == 1)
			{
				result = refPacks[0];
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		public static List<CusRefPacks> LoadFilteredRefPacks(BusinessObjectFactory factory, ZString country, ZString type, ZString commercialPack, bool useFallback = true, int conversionFactor = 0, string customsPack = "")
		{
			var refPacks = factory.Load<CusRefPacks>(GetRefPacksQuery(country, type, commercialPack, conversionFactor, customsPack));

			return type == RPTypeList.Codes.AllAreas ? refPacks.ToList() : FilterRefPacks(refPacks, type, useFallback);
		}

		public static ZQuery GetRefPacksQuery(ZString country, ZString type, ZString commercialPack, int conversionFactor = 0, string customsPack = "")
		{
			var result = new ZQuery(RefPacksSchema.RP_CustomsCountry, country);

			if (!commercialPack.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, RefPacksSchema.RP_CommercialPack, commercialPack);
			}

			if (conversionFactor != 0)
			{
				result.AddToFilter(JoinCondition.And, RefPacksSchema.RP_ConversionFactor, conversionFactor);
			}

			if (!new ZString(customsPack).IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, RefPacksSchema.RP_CustomsPack, customsPack);
			}

			var packTypeQuery = new ZQuery(RefPacksSchema.RP_Type, type);
			if (type != RPTypeList.Codes.AllAreas)
			{
				packTypeQuery.AddToFilter(JoinCondition.Or, RefPacksSchema.RP_Type, RPTypeList.Codes.AllAreas);
			}
			result.AddToFilter(packTypeQuery, JoinCondition.And);

			return result;
		}

		public static List<CusRefPacks> FilterRefPacks(CusRefPacks[] refPacks, ZString type, bool useFallback = true)
		{
			var result = new List<CusRefPacks>();

			result.AddRange(refPacks.Where(pack => pack.RP_Type == type).OrderBy(x => x.RP_CommercialPack.PadRight(BaseRefPacks.Schema.RP_CommercialPackMaxLength) + x.RP_CustomsPack));
			if (useFallback && type != RPTypeList.Codes.AllAreas)
			{
				var allAreasData = refPacks.Where(pack => pack.RP_Type == RPTypeList.Codes.AllAreas && !IsConversionAlreadyInPacks(result, pack)).OrderBy(x => x.RP_CommercialPack.PadRight(BaseRefPacks.Schema.RP_CommercialPackMaxLength) + x.RP_CustomsPack).ToArray();
				result.AddRange(allAreasData);
			}

			return result;
		}

		static bool IsConversionAlreadyInPacks(List<CusRefPacks> packs, CusRefPacks refPack)
		{
			return packs.Any(pack => pack.RP_CommercialPack == refPack.RP_CommercialPack && pack.RP_CustomsPack == refPack.RP_CustomsPack);
		}
	}
}
