using System;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public abstract class ZipDataCollectorProgram : DataCollectorProgram
	{
		public virtual bool IsUCC6 => false;

		protected override void PrepareDataSource(ref Errors error, out DateTime publicationDate)
		{
			var baseUrl = IsUCC6 ? ApplicationConfig.Instance.UCC6BaseUrl : ApplicationConfig.Instance.DeltaGBaseUrl;
			var downloadDirectory = ApplicationConfig.Instance.DownloadDirectory;
			var downloadFileName = ApplicationConfig.Instance.DownloadFileName;

			var filesToExtract = GetGenerators().SelectMany(x => x.InputFiles).ToArray();

			var fileDownloader = new FileDownloader();

			DropFileDownloader.DownloadZipFromCustoms(fileDownloader, baseUrl, downloadDirectory, downloadFileName, ref error);
			DropFileDownloader.ExtractZipFile(downloadDirectory, downloadFileName, downloadDirectory, filesToExtract, out publicationDate, ref error);
		}
	}
}
