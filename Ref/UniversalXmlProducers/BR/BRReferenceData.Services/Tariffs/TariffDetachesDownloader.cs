
namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffDetachesDownloader : BaseDownloaderWithHrefSearch
	{
		protected override string Url => ConfigurationProvider.Configuration.GetSection("URL_TARIFF_DETACHES").Value;

		protected override string InnerHtml => "I. Tratamento Administrativo Anuente Web";
	}
}
