using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class ExchangeHedgeMethodofPaymentCodeListProgram : BaseCheckUpdateProgram
	{
		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new ExchangeHedgeMethodofPaymentCodeListDownloader();
				var bFile = downloader.Download(client);
				return bFile;
			}
		}

		protected override string DataSourceFriendlyName => "Method of Payment XML";

		protected override string LogFileSuffix => Constants.MethodOfPaymentLogName;

		protected override void ExportToXMLFile(byte[] bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsMethodOfPayment.Code}.xml");
			var typeOutputFileName = GetOutputFilePath($"RefCusCodeType_BR_{Constants.RefCusCodeTypes.CustomsMethodOfPayment.Code}.xml");

			using (var stream = new MemoryStream(bFile))
			{
				new ExchangeHedgeMethodofPaymentCodeListParser("BR Exchange Hedge Method of Payment Code List").ExportToXMLFile(stream, outputFileName, typeOutputFileName);
			}
		}
	}
}
