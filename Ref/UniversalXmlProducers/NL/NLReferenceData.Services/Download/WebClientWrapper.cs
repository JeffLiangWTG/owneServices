using System.Net.Http;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class WebClientWrapper : FileDownloaderWrapper, IWebClientWrapper
	{
		public WebClientWrapper()
		{
		}

		public WebClientWrapper(HttpClient httpClientOverride, HttpClientRetryHandler httpClientRetryHandlerOverride) : base(httpClientOverride, httpClientRetryHandlerOverride)
		{
		}

		public XmlDocument DownloadFileAsXmlDocument(string remoteUrl)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(remoteUrl);
			return xmlDoc;
		}
	}
}
