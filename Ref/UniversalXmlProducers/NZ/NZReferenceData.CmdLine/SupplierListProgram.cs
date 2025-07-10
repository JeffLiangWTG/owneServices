using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
{
	public class SupplierListProgram : NZProgram
	{
		public override string ProgramName => "Supplier List";

		protected override void RunCore()
		{
			using (var client = new HttpClient())
			{
				RunCore(client, new SupplierListFileDownloder(), ApplicationConfig.SupplierListUrl, Logger);
			}
		}

		public static void RunCore(HttpClient client, SupplierListFileDownloder downloader, string supplierUrl, ILogger logger)
		{
			var localFilePath = Path.GetTempFileName();
			try
			{
				var result = downloader.Download(client, supplierUrl, localFilePath);
				if (result.IsSuccess)
				{
					SupplierParser.ExportToXMLFile(localFilePath, ApplicationConfig.OutputDirectory, result.PublishDate);

					File.Copy(localFilePath, SupplierParser.DataFilePath, true);

					logger.LogInfo($"Supplier records are generated completed!");
				}
			}
			finally
			{
				if (File.Exists(localFilePath))
				{
					File.Delete(localFilePath);
				}
			}
		}
	}
}
