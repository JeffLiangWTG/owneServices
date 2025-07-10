using System;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public interface IHttpHandler
	{
		Task DownloadToFile(Uri uri, string filePath);
		Task<CodeListMetadata> DownloadCodeListMetadata();
	}
}
