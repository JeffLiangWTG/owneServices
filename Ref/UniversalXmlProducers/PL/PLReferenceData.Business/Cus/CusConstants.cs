namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cus
{
	public static class CusConstants
	{
		// url addresses
		public const string DirTxtFileUrl = @"https://static.nbp.pl/dane/kursy/xml/dir"; // contains all xml filenames from specified year ( dir.txt for current year, dir2019.txt for 2019, etc. etc. )
		public const string NbpXmlUrl = @"https://static.nbp.pl/dane/kursy/xml/";

		public const string CusUniversalReferenceDataXmlFilename = "PLExchangeRate_CUS.xml";
	}
}
