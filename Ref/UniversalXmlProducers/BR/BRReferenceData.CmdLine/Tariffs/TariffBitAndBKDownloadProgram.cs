using System;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffBitAndBKDownloadProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var handler = GetClientHandler())
			using (var client = GetHttpClientWithHandler(handler))
			{
				var bFile = TariffBitAndBkDownloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Bit and BK Json file";

		protected override string LogFileSuffix => Constants.BitAndBKLogName;

		protected virtual HttpClient GetHttpClientWithHandler(HttpClientHandler handler)
		{
			return HttpClientUtils.New(handler);
		}

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new NotImplementedException();
		}

		static HttpClientHandler GetClientHandler()
		{
			var handler = new HttpClientHandler
			{
				ClientCertificateOptions = ClientCertificateOption.Manual,
				ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
			};
			return handler;
		}
	}
}
