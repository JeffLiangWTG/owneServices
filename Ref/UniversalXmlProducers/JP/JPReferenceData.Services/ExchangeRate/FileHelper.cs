namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class FileHelper : IFileHelper
	{
		public string GetFullPDFPath(string url) => AppConfig.ExchangeRate.PdfFileUrlPrefix + url;
	}
}
