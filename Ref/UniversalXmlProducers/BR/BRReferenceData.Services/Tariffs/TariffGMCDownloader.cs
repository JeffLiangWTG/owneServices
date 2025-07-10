
namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffGMCDownloader : BaseDownloaderWithHrefSearch
	{
		protected override string Url => ConfigurationProvider.Configuration.GetSection("URL_CURRENT_LISTS_TARIFF_RATES").Value;

		protected override string InnerHtml => "Anexo IV";
	}
}
