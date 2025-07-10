using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public class RefCusCodeListWithLanguageMapper : SimpleRefCusCodeListMapper
	{
		protected override string GetDescription(PuescBasedDictionaryElement item)
		{
			return !string.IsNullOrEmpty(item.DescriptionEng) ? item.DescriptionEng : RefDataConstants.DefaultDescription;
		}

		protected override RefCusCodeListLanguage[] GetRefCusCodeListLanguages(PuescBasedDictionaryElement item)
		{
			return new[] { new RefCusCodeListLanguage { ZXA_Description = !string.IsNullOrEmpty(item.Description) ? item.Description : RefDataConstants.DefaultDescription } };
		}

		protected override IReadOnlyList<RefCusCodeList> GetMergedAndGroupedData(DictionaryData dictionaryData, IReadOnlyList<RefCusCodeList> notMergedData)
		{
			return
			(
				from item in notMergedData
				group item by item.ZZD_Code into groupedData
				select new RefCusCodeList
				{
					ZZD_Code = groupedData.Key,
					ZZD_EndDate = groupedData.OrderBy(o => o.ZZD_EndDate).Last().ZZD_EndDate,
					ZZD_StartDate = groupedData.OrderBy(o => o.ZZD_StartDate).First().ZZD_StartDate,
					ZZD_Description = groupedData.OrderBy(o => o.ZZD_StartDate).Last().ZZD_Description,
					ZZD_ZZK_NKCodeType = dictionaryData.CW1Code,
					RefCusCodeListLanguages = groupedData.OrderBy(o => o.ZZD_StartDate).Select(x => x.RefCusCodeListLanguages).Last()
				}
			).ToList();
		}
	}
}
