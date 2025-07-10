using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Services.Dictionaries;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries
{
	public static class DictionariesUniversalReferenceDataXmlGenerator
	{
		public static bool GenerateDictionariesUniversalReferenceData()
		{
			try
			{
				var dictionaries = new Dictionaries(DictionaryFromUrlProvider.GetDictionaryFromUrl);
				foreach (var item in SupportedDictionaryList.SupportedDictionaries)
				{
					var data = dictionaries.GetDictionariesAsRefCusCodeList(item);

					var defaultOutputFullPath = CommonHelper.GetOutputFilePath($"{DictionariesConstants.DictionariesUniversalReferenceDataXmlFilename}_{item.CW1Code}.xml");

					var publicationTime = Dictionaries.CurrentDictionaryPublicationDate;
					var dataSource = $"{DictionariesConstants.DataSource} {item.CW1Code}";

					XmlWriterConfig.ExportToXmlFile(dataSource, defaultOutputFullPath, item.RefDataType.GetXmlWriterConfiguration(), publicationTime, data);
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				return false;
			}

			return true;
		}
	}
}
