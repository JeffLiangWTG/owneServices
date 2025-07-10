using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryModelMark;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries
{
	public class Dictionaries
	{
		public Dictionaries(Func<string, bool, XmlTextReader> xmlDictionaryProvider)
		{
			XmlDictionaryProvider = xmlDictionaryProvider;
		}

		public Func<string, bool, XmlTextReader> XmlDictionaryProvider { get; }
		public static DateTime CurrentDictionaryPublicationDate { get; private set; }

		public IReadOnlyList<RefCusCodeList> GetDictionariesAsRefCusCodeList(DictionaryData item)
		{
			using (var xmlDoc = XmlDictionaryProvider.Invoke(item.Code, item.PublishedByTestPuesc))
			{
				CurrentDictionaryPublicationDate = DateTime.UtcNow;
				switch (item.Code)
				{
					case DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes:
						return OnModelAndMarkBasedDictionary(xmlDoc, item);
					default:
						return OnPuescBasedDictionary(xmlDoc, item);
				}
			}
		}

		static IReadOnlyList<RefCusCodeList> OnModelAndMarkBasedDictionary(XmlTextReader xmlDoc, DictionaryData dictionary)
		{
			var data = ModelAndMarkBasedDictionaryXmlParser.ParseXml(xmlDoc);
			var temp = new List<RefCusCodeList>();

			foreach (var item in data)
			{
				if (item.Code.Any(char.IsWhiteSpace))
				{
					continue;
				}

				temp.Add(
					new RefCusCodeList
					{
						ZZD_Code = item.Code,
						ZZD_EndDate = DictionaryHelper.GetDataTime(item.ValidTo, false),
						ZZD_StartDate = DictionaryHelper.GetDataTime(item.ValidFrom, true),
						ZZD_Description = $"{item.Mark},{item.Model}",
						ZZD_ZZK_NKCodeType = dictionary.CW1Code
					}
				);
			}

			return temp.GroupBy(x => x.ZZD_Code).Select(g => g.OrderBy(o => o.ZZD_StartDate).Last()).ToList();
		}

		IReadOnlyList<RefCusCodeList> OnPuescBasedDictionary(XmlTextReader xmlDoc, DictionaryData dictionaryData)
		{
			var mapper = dictionaryData.RefDataType.GetPuescRefCusCodeListMapper();
			return mapper.MapDictionaryElementsToRefData(this, xmlDoc, dictionaryData);
		}
	}
}
