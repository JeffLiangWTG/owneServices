using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Dictionaries
{
	public static class DictionaryFromUrlProvider
	{
		public static XmlTextReader GetDictionaryFromUrl(string dictionaryCode, bool publishedByTestPuesc) => publishedByTestPuesc ?
			new XmlTextReader(ApplicationConfig.Instance.TestPuescXmlDictionariesUrl + dictionaryCode) : new XmlTextReader(ApplicationConfig.Instance.PuescXmlDictionariesUrl + dictionaryCode);
	}
}
