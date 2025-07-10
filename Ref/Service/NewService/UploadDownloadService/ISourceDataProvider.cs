using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace CargoWise.RefDbRepo.NewService.UploadDownloadService
{
	public interface ISourceDataProvider
	{
		Task UploadFileToServer(string source, string fileType, string contentType, string contacts, string fileName, FileMultipartSection fileSection);
		Task<(string, Stream)> GetFileFromServer(Guid sourceDataPK);
	}
}
