namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSStandingData
{
	public class WebTable
	{
		public WebTable(string headerXpath, string dataXpath)
		{
			HeaderXpath = headerXpath;
			DataXpath = dataXpath;
		}

		internal readonly string DataXpath;
		internal readonly string HeaderXpath;
	}
}
