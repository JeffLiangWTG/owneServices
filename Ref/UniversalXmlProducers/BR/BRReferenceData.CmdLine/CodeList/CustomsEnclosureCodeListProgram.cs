using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class CustomsEnclosureCodeListProgram : BaseProgram
	{
		protected override void RunCore()
		{
			using (var client = HttpClientUtils.New())
			{
				var downloader = new CustomsEnclosureCodeListDownloader();
				var dto = downloader.Download(client);
				Argument.NotNull(dto, nameof(dto));
				Argument.NotNull(dto.data, nameof(dto.data));
				Argument.NotNull(dto.downloadedDate, nameof(dto.downloadedDate));

				var outputFileName = GetOutputFilePath($"RefCusCodeList_BR_{Constants.RefCusCodeTypes.CustomsEnclosure.Code}.xml");
				var outputTypeFileName = GetOutputFilePath($"CusCodeType_BR_{Constants.RefCusCodeTypes.CustomsEnclosure.Code}.xml");

				using (var streamFile = new MemoryStream(dto.data))
				{
					var parser = new CustomsEnclosureCodeListParser("BR Customs Enclosure Code List");
					parser.ExportToXMLFile(streamFile, outputFileName, outputTypeFileName);
					Console.WriteLine($"RefCusCodeList records generated to {outputFileName}");
				}
			}
		}
	}
}
