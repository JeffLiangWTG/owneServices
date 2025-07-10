using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public abstract class CodeListsParserTSV<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected CodeListsParserTSV(Dictionary<string, string> downloadLinks)
		{
			this.downloadLinks = downloadLinks;
		}
		readonly Dictionary<string, string> downloadLinks;

		public string DownloadAndConvertToRefCusCodeListXML(HttpClient client, string outputFilePath, IDateTimeProvider dateTimeProvider)
		{
			ErrorBuilder.Clear();
			(var success, var downloadUrl) = GetDownloadLink();
			if (success)
			{
				var codeListArray = GetCodeList(client, downloadUrl.ToString());
				var result = new List<T>();
				var codesToImport = new Dictionary<string, TKeyValues>();

				if (AnyValidRecords(codeListArray))
				{
					foreach (var record in codeListArray)
					{
						var splittedRecord = record.Split(new string[] { "\t" }, StringSplitOptions.None);
						var keyValues = GetKeyValues(splittedRecord);
						if (ValidateData(keyValues))
						{
							AddOrUpdateCodesToImport(codesToImport, keyValues);
						}
						else
						{
							AppendInvalidDataErrorDetails(splittedRecord);
						}
					}

					foreach (var code in codesToImport.Where(x => !x.Value.EndDateIsExpired))
					{
						result.Add(CreateRefList(code.Value));
					}

					Helper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outputFilePath, OutputFileName), XmlWriterConfiguration, dateTimeProvider.CurrentLocalDate, result);
				}
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"No download link found for {GetType()} code list: {CustomsCodeListIdentifier}");
			}

			return ErrorBuilder.ToString();
		}

		protected string[] DownloadEMCSAndExtractTSVCodeList(HttpClient client, string downloadUrl, string customsCodeListIdentifier)
		{
			var tempFile = Path.GetTempFileName();
			CodeListsDownloaderTSV.Download(client, downloadUrl, tempFile).Wait();
			var codeListArray = new string[0];
			using (var zipArchives = ZipFile.Open(tempFile, ZipArchiveMode.Read))
			{
				var tsvZipEntry = zipArchives.Entries.Single(s => s.Name.EndsWith($"{customsCodeListIdentifier}.tsv", StringComparison.InvariantCulture));
				var encoding = GetEncoding(tsvZipEntry);

				using (var zipStream = tsvZipEntry.Open())
				using (var reader = new StreamReader(zipStream, encoding))
				{
					codeListArray = reader.ReadToEndAsync().Result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				}
			}

			File.Delete(tempFile);
			return codeListArray.Skip(1).ToArray();
		}

		protected virtual bool ValidateData(TKeyValues keyValues) => !string.IsNullOrWhiteSpace(keyValues.Code) && !string.IsNullOrWhiteSpace(keyValues.Description) && keyValues.StartDateSuccessfullyParsed && keyValues.EndDateSuccessfullyParsed;

		protected virtual bool AnyValidRecords(string[] codeListArray) => true;

		protected abstract string CustomsCodeListIdentifier { get; }

		protected abstract string CodeType { get; }

		protected abstract string HtmlElementIdentifier { get; }

		protected abstract string OutputFileNameSuffix { get; }

		protected abstract XmlWriterConfiguration XmlWriterConfiguration { get; }

		protected abstract TKeyValues GetKeyValues(string[] record);

		protected abstract T CreateRefList(TKeyValues keyValues);

		protected virtual IEnumerable<string> AdditionalAttributes => new List<string>();

		protected virtual void AppendInvalidDataErrorDetails(string[] record)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.");
			CodeListsHelper.GetBaseErrorDetailsTSV(ErrorBuilder, record);
		}

		protected virtual string XMLWriterDataSource => $"DE {CodeType}";

		protected abstract string[] GetCodeList(HttpClient client, string downloadUrl);

		protected StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		string OutputFileName => $"{CodeListsConstants.OutputFileNamePrefix}{OutputFileNameSuffix}.xml";

		static void AddOrUpdateCodesToImport(Dictionary<string, TKeyValues> codesToImport, TKeyValues keyValues)
		{
			var combinedKeyValues = keyValues;
			var codeToImportLocated = codesToImport.TryGetValue(keyValues.UniqueCode, out var currentlyRecordedKeyValues);
			if (codeToImportLocated)
			{
				combinedKeyValues = currentlyRecordedKeyValues;
				if (keyValues.EndDate > currentlyRecordedKeyValues.EndDate)
				{
					combinedKeyValues = keyValues;
					if (currentlyRecordedKeyValues.StartDate < keyValues.StartDate)
					{
						combinedKeyValues.SetStartDateDetails(currentlyRecordedKeyValues.StartDateSuccessfullyParsed, currentlyRecordedKeyValues.StartDate);
					}
				}
				else if (keyValues.StartDate < currentlyRecordedKeyValues.StartDate)
				{
					combinedKeyValues.SetStartDateDetails(keyValues.StartDateSuccessfullyParsed, keyValues.StartDate);
				}
			}
			codesToImport[combinedKeyValues.UniqueCode] = combinedKeyValues;
		}

		(bool successfullDownloadUrl, Uri downloadUrl) GetDownloadLink()
		{
			Uri downloadUrl = null;
			var successfullDownloadUrl = downloadLinks.TryGetValue(HtmlElementIdentifier, out var linkToDownload);
			if (successfullDownloadUrl)
			{
				successfullDownloadUrl = Uri.TryCreate(new Uri(ApplicationConfig.CustomsBaseURL), linkToDownload, out downloadUrl);
			}
			return (successfullDownloadUrl, downloadUrl);
		}

		static Encoding GetEncoding(ZipArchiveEntry zipArchiveEntry)
		{
			using (var stream = zipArchiveEntry.Open())
			{
				Ude.CharsetDetector charsetDetector = new Ude.CharsetDetector();
				charsetDetector.Feed(stream);
				charsetDetector.DataEnd();
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				return Encoding.GetEncoding(charsetDetector.Charset);
			}
		}
	}
}
