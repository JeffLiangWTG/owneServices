namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class AutoPartsTariffDownloader : BaseDownloaderWithHrefSearch
	{
		protected override string Url => ConfigurationProvider.Configuration.GetSection("URL_AUTO_PARTS_LIST").Value;

		protected override string InnerHtml => "Ex-tarifários de Autopeças Vigentes";
	}
}
