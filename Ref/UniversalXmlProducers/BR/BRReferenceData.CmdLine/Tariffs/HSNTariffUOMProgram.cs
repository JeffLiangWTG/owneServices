using System;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class HSNTariffUOMProgram : BaseProgram
	{
		protected override void RunCore()
		{
			var publicationTime = DateTime.Now;
			var outputFileName = GetOutputFilePath("RefCusTariff_BR_HSN_UOM.xml");

			var parser = new HSNTariffUOMParser("BR HSN Tariff UOM");
			var ncmByteArray = DownloadNCMFile();
			using (Stream inputFile = new MemoryStream(ncmByteArray))
			{
				var ncmFileDate = GetNcmFileGenerationDate(ncmByteArray);

				if (IsGenerateXMLFile(ncmFileDate))
				{
					parser.ExportToXMLFile(inputFile, outputFileName, publicationTime);
					BrLogUtils.Instance.AddLog(Constants.BRNcmXlsLastUpdateDate, ncmFileDate);
					Console.WriteLine($"RefCusTariffUOM records generated to {outputFileName}");
				}
				else
				{
					Console.WriteLine($"No data source change from the last file downloaded, no xml generated! Date:{ncmFileDate}");
				}
			}
		}

		public static bool IsGenerateXMLFile(string ncmFileDate)
		{
			var lastNcmFileDate = BrLogUtils.Instance.GetKeyValueAsString(Constants.BRNcmXlsLastUpdateDate);
			return !ncmFileDate.Equals(lastNcmFileDate, StringComparison.Ordinal);
		}

		static byte[] DownloadNCMFile()
		{
			using (var httpClient = HttpClientUtils.New())
			{
				var downloader = new TariffTableNCMDownloader();
				var bFile = downloader.DownloadTableNCM(httpClient);
				return bFile;
			}
		}

		static string GetNcmFileGenerationDate(byte[] ncmFileBytes)
		{
			using (Stream inputFile = new MemoryStream(ncmFileBytes))
			{
				var xls = new XlsFile(inputFile, true);
				xls.ActiveSheet = 1;

				var generationDate = xls.GetCellValue(11, 1)?.ToString()?.Trim();
				generationDate = generationDate.KeepNumericsOnly();

				return generationDate;
			}
		}
	}
}
