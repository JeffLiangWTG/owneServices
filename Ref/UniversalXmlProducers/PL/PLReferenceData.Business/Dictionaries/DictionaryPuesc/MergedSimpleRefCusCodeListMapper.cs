using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public class MergedSimpleRefCusCodeListMapper : SimpleRefCusCodeListMapper
	{
		protected override IReadOnlyList<RefCusCodeList> GetMergedAndGroupedData(DictionaryData dictionaryData, IReadOnlyList<RefCusCodeList> notMergedData)
			=> notMergedData
			.GroupBy(item => item.ZZD_Code)
			.Select(groupedData =>
			{
				var firstItemOnStartDate = groupedData.OrderBy(o => o.ZZD_StartDate).First();
				var lastItemOnEndDate = groupedData.OrderBy(o => o.ZZD_EndDate).Last();
				return new RefCusCodeList
				{
					ZZD_Code = groupedData.Key,
					ZZD_EndDate = lastItemOnEndDate.ZZD_EndDate,
					ZZD_StartDate = firstItemOnStartDate.ZZD_StartDate,
					ZZD_Description = lastItemOnEndDate.ZZD_Description,
					ZZD_ZZK_NKCodeType = dictionaryData.CW1Code,
				};
			}).ToList();
	}
}
