using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefCusCodeTypeList
	{
		public static CodeDescriptionPairList GetListByCountry(BusinessObjectFactory factory, ZString dataGroupingCode, bool excludeReadonly, bool includeParentDataGroupings = false)
			=> GetListByCountry(factory, new[] { dataGroupingCode }, excludeReadonly, includeParentDataGroupings);

		public static CodeDescriptionPairList GetListByCountry(BusinessObjectFactory factory, ZString[] dataGroupingCodes, bool excludeReadonly, bool includeParentDataGroupings = false)
		{
			var languageCode = TranslationHelper.GetCurrentLanguageCode();
			dataGroupingCodes = dataGroupingCodes.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToArray();
			return factory.GetCachedValue("GetListByCountry_" + string.Join(",", dataGroupingCodes) + excludeReadonly + includeParentDataGroupings + languageCode, () =>
			{
				var dataGrouping = dataGroupingCodes;
				if (includeParentDataGroupings)
				{
					var dataGroupingList = new List<ZString>();
					foreach (var dataGroupingCode in dataGroupingCodes)
					{
						dataGroupingList.AddRange(MasterFiles.Business.RefDataGrouping.GetDataGroupingIncludingParent(factory, dataGroupingCode));
					}
					dataGrouping = dataGroupingList.Distinct().OrderBy(x => x).ToArray();
				}
				var result = new CodeDescriptionPairList();
				var collection = new DynamicBusinessObjectCollection(factory);
				var @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@DataGrouping", dataGrouping, RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, true));
				@params.Add("@ExcludeReadonly", excludeReadonly, RefCusCodeTypeSchema.ZZK_IsReadonly);
				@params.Add("@Language", languageCode, RefCusCodeTypeLanguageSchema.ZXI_ZX6_NKLanguage);
				collection.Load(GetSqlText(), @params);
				foreach (DynamicBusinessObject data in collection)
				{
					var code = data[RefCusCodeType.Schema.ZZK_CodeType].ToString();
					var description = data[RefCusCodeType.Schema.ZZK_Description].ToString();
					result.AddPairIfNotExist(code, description);
				}
				result.SortByDescription();
				return result;
			});
		}

		static string GetSqlText()
		{
			return $@"SELECT ZZK_CodeType, ISNULL(ZXI_Description, ZZK_Description) AS ZZK_Description
					FROM RefDatabase_RefCusCodeType
					LEFT JOIN RefDatabase_RefCusCodeTypeLanguage ON ZZK_PK = ZXI_ZZK_CodeType and ZXI_ZX6_NKLanguage = @Language
					WHERE ZZK_ZZZ_NKDataGrouping IN (SELECT Value FROM @DataGrouping) AND (@ExcludeReadonly = 0 OR ZZK_IsReadonly = 0)";
		}
	}
}
