using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public class SimpleRefCusCodeListMapper : IRefCusCodeListMapper
	{
		public IReadOnlyList<RefCusCodeList> MapDictionaryElementsToRefData(Dictionaries dictionaries, XmlTextReader xmlDoc, DictionaryData dictionaryData)
		{
			var dictionaryElements = PuescDictionaryXmlParser.ParseXml(xmlDoc);
			var excludedCodes = GetExcludedCodes(dictionaries, dictionaryData);
			var refDataList = new List<RefCusCodeList>();

			foreach (var item in dictionaryElements)
			{
				if (item.Code.Any(char.IsWhiteSpace) || excludedCodes.Contains(item.Code)
					|| !dictionaryData.SearchedCodes.DefaultIfEmpty(item.Code).Contains(item.Code))
				{
					continue;
				}
				var refData = new RefCusCodeList
				{
					ZZD_Code = item.Code,
					ZZD_EndDate = GetDataTime(item.ValidTo, false),
					ZZD_StartDate = GetDataTime(item.ValidFrom, true),
					ZZD_Description = TruncateString(GetDescription(item), DictionariesConstants.MaxDescriptionLen),
					ZZD_ZZK_NKCodeType = dictionaryData.CW1Code,
					RefCusCodeListAttributes = GetRefCusCodeListAttributes(),
					RefCusCodeListLanguages = GetRefCusCodeListLanguages(item)
				};

				refDataList.Add(refData);
			}

			foreach (var item in dictionaryData.AdditionalDictionaries)
			{
				refDataList.AddRange(dictionaries.GetDictionariesAsRefCusCodeList(item));
			}

			return GetMergedAndGroupedData(dictionaryData, refDataList);
		}

		protected virtual string GetDescription(PuescBasedDictionaryElement item)
		{
			return !string.IsNullOrEmpty(item.Description) ? item.Description
					: !string.IsNullOrEmpty(item.DescriptionEng) ? item.DescriptionEng : RefDataConstants.DefaultDescription;
		}

		protected virtual RefCusCodeListAttribute[] GetRefCusCodeListAttributes() => null;

		protected virtual RefCusCodeListLanguage[] GetRefCusCodeListLanguages(PuescBasedDictionaryElement item) => null;

		protected virtual IReadOnlyList<RefCusCodeList> GetMergedAndGroupedData(DictionaryData dictionaryData, IReadOnlyList<RefCusCodeList> notMergedData)
		{
			return notMergedData.GroupBy(x => x.ZZD_Code).Select(g => g.OrderBy(o => o.ZZD_StartDate).Last()).ToList();
		}

		static IReadOnlyList<string> GetExcludedCodes(Dictionaries dictionaries, DictionaryData dictionaryData)
		{
			var excludedCodes = new List<string>();
			foreach (var item in dictionaryData.ExcludedCodesList)
			{
				var dictionaryCode = item.DictionaryCode;
				if (string.IsNullOrEmpty(dictionaryCode))
				{
					excludedCodes.AddRange(item.ExcludedCodes);
				}
				else
				{
					using (var xmlDoc = dictionaries.XmlDictionaryProvider.Invoke(dictionaryCode, false))
					{
						var data = PuescDictionaryXmlParser.ParseXml(xmlDoc);
						foreach (var dataItem in data)
						{
							if (dataItem.ValidTo > DateTime.Today)
							{
								excludedCodes.Add(dataItem.Code);
							}
						}
					}
				}
			}
			return excludedCodes;
		}
	}
}
