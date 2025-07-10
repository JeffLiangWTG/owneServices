using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public interface IWebResponseHandler
	{
		Task<ResponseResult> GetWebRepsonseForTariffAsync(string requestUrl, string cusTariffCode);
		Task<ResponseResult> GetWebResponseForPublicationDateAsync(string requestUrl);
		Task<ResponseResult> GetWebRepsonseForCertificateLinkAsync(string requestUrl, string requirementHtml);
		Task<ResponseResult> GetWebResponseForCertificateAsync(string requestUrl, string parameterString, string requirementHtml, string tariffCode);
	}
}
