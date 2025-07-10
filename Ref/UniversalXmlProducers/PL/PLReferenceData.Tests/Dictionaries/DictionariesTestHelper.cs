using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries
{
	static class DictionariesTestHelper
	{
		public const string TextXmlPath3041InvalidDates = "CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries.TestFiles.Input.3041_invalidDates.xml";
		public const string TextXmlPathCL008AES = "CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries.TestFiles.Input.CL008AES_Small.xml";
		public const string TextXmlPath3041 = "CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries.TestFiles.Input.3041_Small.xml";
		public const string TextXmlPath034 = "CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries.TestFiles.Input.034_Small.xml";
		
		public static Business.Dictionaries.Dictionaries DictionariesForTest(string textXmlPath)
		{
			return new Business.Dictionaries.Dictionaries((a, b) => new XmlTextReader(TestHelper.GetManifestResourceStream(textXmlPath)));
		}
	}
}
