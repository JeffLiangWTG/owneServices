using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class CUSNumberSOAPResponseParser
	{
		public CUSNumberSOAPResponseParser(string responseData)
		{
			doc = XDocument.Parse(responseData);
		}

		public IEnumerable<CUSNumber> Parse() => doc.Descendants("result").Select(i => ParseItem(i)).ToList();

		static CUSNumber ParseItem(XElement i)
		{
			var (englishText, translations) = ParseNames(i.Element("names"));
			return new CUSNumber(i.Element("cus_number").Value,
				englishText,
				i.Element("cn_code")?.Value,
				i.Element("cas_rn")?.Value,
				i.Element("ec_number")?.Value,
				i.Element("un_number")?.Value,
				translations);
		}

		static (string englishDescription, IEnumerable<CUSTranslatedName> translations) ParseNames(XElement element)
		{
			var translations = new List<CUSTranslatedName>();
			var englishDescription = string.Empty;

			foreach (var name in element.Elements("name").Where(n => n.Element("level").Value == "Name" && n.Element("order").Value == "1"))
			{
				var lang = name.Element("lang_code").Value;
				var description = name.Element("description").Value;
				if (lang == "EN")
				{
					englishDescription = description;
				}
				else
				{
					translations.Add(new CUSTranslatedName(lang, description));
				}
			}

			return (englishDescription, translations);
		}

		readonly XDocument doc;
	}
}
