using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public class RefCusCodeListWithAttributeImportMapper : SimpleRefCusCodeListMapper
	{
		protected override RefCusCodeListAttribute[] GetRefCusCodeListAttributes()
		{
			return new[] { new RefCusCodeListAttribute { ZZE_Value = RefDataConstants.Import, ZZE_ZXE_NKName = RefDataConstants.Direction } };
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
					RefCusCodeListAttributes = groupedData.SelectMany(x => x.RefCusCodeListAttributes).Distinct(new RefCusCodeListAttributeComparer()).ToArray()
				}
			).ToList();
		}
	}
}
