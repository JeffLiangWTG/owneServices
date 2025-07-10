using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs.Parser
{
	internal class CARMParserHelper
	{
		readonly string workingDirectory;
		public CARMParserHelper(string workingDirectory)
		{
			this.workingDirectory = workingDirectory;
		}

		public Dictionary<string, List<TParsed>> ParseEntriesByFiles<TEntry, TParsed>(string queryType, Func<TEntry, TParsed> entryParser, Func<TEntry, string> dictionaryKeySupplier, string language = Constants.DefaultValues.ENLanguage, bool multiValues = false) where TEntry : CARMContentProperties
		{
			var dic = new Dictionary<string, List<TParsed>>();
			ParseEntriesByFilesCore(queryType, entryParser, (entryProperties, parsedObj) =>
			{
				var key = dictionaryKeySupplier(entryProperties);
				if (dic.ContainsKey(key))
				{
					if (multiValues)
					{ dic[key].Add(parsedObj); }
				}
				else
				{
					dic.Add(key, new List<TParsed>() { parsedObj });
				}
			}, language);
			return dic;
		}

		public void ParseEntriesByFilesCore<TEntry, TParsed>(string queryType, Func<TEntry, TParsed> entryParser, Action<TEntry, TParsed> parsedObjHandler, string language = Constants.DefaultValues.ENLanguage) where TEntry : CARMContentProperties
		{
			var files = Directory.GetFiles(workingDirectory, $"{queryType}_{language}_*.xml", SearchOption.TopDirectoryOnly);
			var serializer = new XmlSerializer(typeof(CARMResponseFeed<TEntry>));
			foreach (var fileItem in files)
			{
				try
				{
					using (var stream = new FileStream(fileItem, FileMode.Open))
					using (var reader = XmlReader.Create(stream))
					{
						var responseFeed = ((CARMResponseFeed<TEntry>)serializer.Deserialize(reader));
						foreach (var entry in responseFeed.Entry)
						{
							TEntry entryProperties = entry.Content.Properties;
							if (entryProperties != null && entryProperties.IsValid && entryParser != null)
							{
								TParsed parsedObj = entryParser(entryProperties);
								if (parsedObj != null && parsedObjHandler != null)
								{
									parsedObjHandler(entryProperties, parsedObj);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Parse {fileItem} error, " + ex.ToString());
				}
			}
		}
	}
}
