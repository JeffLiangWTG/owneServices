using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class HSNTariffDutyRatePdfProgram : GenericCheckUpdateProgram<List<byte[]>>
	{
		protected override string DataSourceFriendlyName => "Tariff Vigent Rate files";

		protected override void CheckDowloadContent(List<byte[]> downloadedContent)
		{
			if (downloadedContent == null || downloadedContent.Count != 5 || downloadedContent.Where(t => t.Length == 0).Any())
			{
				throw new InvalidOperationException($"The application was unable to download all Tariff Rate files.");
			}
		}

		protected override List<byte[]> DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var downloader = new HSNTariffDutyRatePdfDownloader();
				var bFiles = downloader.Download(client);
				var cleanedContents = new List<byte[]>();
				foreach (var bFile in bFiles)
				{
					cleanedContents.Add(XlsFileCleaning(bFile));
				}
				return bFiles;
			}
		}

		protected override void ExportToXMLFile(List<byte[]> bFile)
		{
			using (var stream1 = new MemoryStream(bFile[0]))
			using (var stream2 = new MemoryStream(bFile[1]))
			using (var stream3 = new MemoryStream(bFile[2]))
			using (var stream4 = new MemoryStream(bFile[3]))
			using (var stream5 = new MemoryStream(bFile[4]))
			{
				var streams = new List<Stream>() { stream1, stream2, stream3, stream4, stream5 };
				new HSNTariffDutyRatePdfParser("BR HSN Tariff Duty Rate").ExportToXMLFile(streams, GetOutputFilePath($"RefCusTariffVigentRateList_BR_{Constants.TariffRateTypes.DTY}.xml"), DateTime.Now);
			}
		}

		public void UpdateLogFile(byte[] downloadedContent)
		{
			File.WriteAllBytes(LogFilePath, downloadedContent);
		}

		protected override string LogFileSuffix => Constants.AllTariffRatesLogName;

		protected static byte[] XlsFileCleaning(byte[] file)
		{
			using (var stream = new MemoryStream(file))
			{
				var xls = new XlsFile(stream, true);
				Contract.Assume(xls != null);

				xls.ActiveSheet = 1;
				var cellStartAddress = xls.Find("NCM", xls.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, false, false, false);
				var startDateColumn = xls.Find("início", new FlexCel.Core.TXlsCellRange(cellStartAddress.Row, 1, cellStartAddress.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(cellStartAddress.Row, 1), false, true, false, false);
				if (startDateColumn != null)
				{
					var date = DateTime.Now;
					var endDateColumn = xls.Find("término", new FlexCel.Core.TXlsCellRange(cellStartAddress.Row, 1, cellStartAddress.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(cellStartAddress.Row, 1), false, true, false, false);
					var exColumn = xls.Find("nº ex", new FlexCel.Core.TXlsCellRange(cellStartAddress.Row, 1, cellStartAddress.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(cellStartAddress.Row, 1), false, true, false, true);
					var quotaColumn = xls.Find("quota", new FlexCel.Core.TXlsCellRange(cellStartAddress.Row, 1, cellStartAddress.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(cellStartAddress.Row, 1), false, true, false, true);
					var rowCount = xls.GetRowCount(xls.ActiveSheet);

					for (var rowId = cellStartAddress.Row + 1; rowId <= rowCount; rowId++)
					{
						var exString = exColumn != null ? xls.GetCellValue(rowId, exColumn.Col)?.ToString() : string.Empty;
						var quotaString = quotaColumn != null ? xls.GetCellValue(rowId, quotaColumn.Col)?.ToString() : string.Empty;
						if ((string.IsNullOrWhiteSpace(exString) || string.Equals(exString, "-", StringComparison.Ordinal)) && string.IsNullOrWhiteSpace(quotaString))
						{
							var startDateString = xls.GetCellValue(rowId, startDateColumn.Col)?.ToString();
							var endDateString = endDateColumn != null ? xls.GetCellValue(rowId, endDateColumn.Col)?.ToString() : string.Empty;
							if (DateTime.TryParseExact(startDateString, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
							{
								if (startDate <= date)
								{
									if (DateTime.TryParseExact(endDateString, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
									{
										if (endDate.AddMinutes(1439) < date)
										{
											xls.DeleteRange(new FlexCel.Core.TXlsCellRange(rowId, 1, rowId, xls.GetColCount(xls.ActiveSheet, true)), FlexCel.Core.TFlxInsertMode.NoneDown);
										}
									}
								}
								else
								{
									xls.DeleteRange(new FlexCel.Core.TXlsCellRange(rowId, 1, rowId, xls.GetColCount(xls.ActiveSheet, true)), FlexCel.Core.TFlxInsertMode.NoneDown);
								}
							}
						}
						else
						{
							xls.DeleteRange(new FlexCel.Core.TXlsCellRange(rowId, 1, rowId, xls.GetColCount(xls.ActiveSheet, true)), FlexCel.Core.TFlxInsertMode.NoneDown);
						}
					}
				}

				using (var output = new MemoryStream())
				{
					xls.Save(output, FlexCel.Core.TFileFormats.Xlsx);
					output.Position = 0;
					return output.ToArray();
				}
			}
		}

		protected override bool CompareDownloadedContentWithLog(List<byte[]> downloadedContent)
		{
			return !CompareUtils.AreEquals(File.ReadAllBytes(LogFilePath), BuildLogContent(downloadedContent));
		}

		public override void UpdateLogFile(List<byte[]> downloadedContent)
		{
			File.WriteAllBytes(LogFilePath, BuildLogContent(downloadedContent));
		}

		static byte[] BuildLogContent(List<byte[]> bFiles)
		{
			var allContent = new byte[] { };
			foreach (var bFile in bFiles)
			{
				allContent = allContent.Concat(bFile).ToArray();
			}
			return allContent;
		}

		static readonly string[] DateTimeFormats = new string[] { "dd/MM/yyyy", "yyyy-MM-dd" };
	}
}
