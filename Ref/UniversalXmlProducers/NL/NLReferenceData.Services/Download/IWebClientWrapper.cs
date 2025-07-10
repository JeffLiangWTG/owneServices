using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface IWebClientWrapper : IFileDownloaderWrapper
	{
		XmlDocument DownloadFileAsXmlDocument(string remoteUrl);
	}
}
