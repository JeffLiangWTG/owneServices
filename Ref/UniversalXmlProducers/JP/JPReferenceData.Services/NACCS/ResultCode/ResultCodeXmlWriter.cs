using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CsvHelper;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ResultCodeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "NACCS Result Code";

		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ResultCode";

		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ResultCodeZipDownloadUrl;

		protected static string ResultCodeHomePageUrl => AppConfig.NACCS.CodeLists.ResultCodeHomePageUrl;

		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ResultCodeParser();


		protected override bool GetRecords(string filePath, out IList<string> records)
		{
			records = new List<string>();

			using (var stream = new FileStream(filePath, FileMode.Open))
			using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
			{
				var entries = zip.Entries;
				foreach (var fileName in fileNamesToExtract)
				{
					var entry = zip.GetEntry(fileName) ?? entries.FirstOrDefault(c => c.Name.Equals($@"{fileName}", StringComparison.OrdinalIgnoreCase));
					using (var unzippedFileStream = entry.Open())
					{
#pragma warning disable CA1031 // Do not catch general exception types
						try
						{
							Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
							using (var streamReader = new StreamReader(unzippedFileStream, Encoding.GetEncoding("Shift-JIS")))
							{
								var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture) { IgnoreQuotes = true };
								using (var csv = new CsvReader(streamReader, config))
								{
									while (csv.Read())
									{
										records.Add(csv.Context.RawRecord);
									}
								}
							}
						}

						catch (Exception ex)
						{
							ErrorWriter.WriteException(ex);
							records = null;
							return false;
						}
#pragma warning restore CA1031 // Do not catch general exception types
					}
				}

				records = records.Select(record => record.Replace("\r", "").Replace("\n", "")).ToList();
				return true;
			}
		}

		protected override bool GetPublicationDate(IHttpClientHelper httpClientHelper, out DateTime publicationDate)
		{
			var htmlDocument = new HtmlDocument();
			var html = httpClientHelper.GetWebPageAsync(ResultCodeHomePageUrl).GetAwaiter().GetResult();
			htmlDocument.LoadHtml(html);
			var webTextLines = htmlDocument.Text.Split('\n');

			for (int i = 0; i < webTextLines.Length; i++)
			{
				var line = webTextLines[i];
				if (line.StartsWith("〇エラーメッセージ", StringComparison.Ordinal))
				{
					var dateStartIndex = line.IndexOf('（') + 1;
					var dateEndIndex = line.IndexOf('）') - 3;
					var dateString = line.Substring(dateStartIndex, dateEndIndex - dateStartIndex + 1);
					var dateHalfWidthString = dateString.Normalize(NormalizationForm.FormKC);
					publicationDate = DateTime.Parse(dateHalfWidthString, CultureInfo.InvariantCulture);
					return true;
				}
			}

			publicationDate = new DateTime();
			return false;
		}

		readonly string[] fileNamesToExtract = new string[] {
			"ida_err.csv",
			"ida01_err.csv",
			"eda_err.csv",
			"eda01_err.csv",
			"msx_err.csv",
			"hch01_err.csv",
			"hdf01_err.csv",
			"idc_err.csv",
			"edc_err.csv",
			"cew_err.csv",
			"1ce_err.csv",
			"3ew_err.csv",
			"1ed_err.csv",
			"3ed_err.csv",
			"nvc01_err.csv",
			"nvc02_err.csv",
			"ecr_err.csv",
			"vae_err.csv",
			"van_err.csv",
			"ede_err.csv",
			"ide_err.csv",
			"eaa_err.csv",
			"eac_err.csv",
			"1ea_err.csv",
			"mec_err.csv",
			"mee_err.csv",
			"1me_err.csv",
			"3me_err.csv",
			"mic_err.csv",
			"1mi_err.csv",
			"maf_err.csv",
			"ema_err.csv",
			"hys_err.csv",
		};
	}
}
