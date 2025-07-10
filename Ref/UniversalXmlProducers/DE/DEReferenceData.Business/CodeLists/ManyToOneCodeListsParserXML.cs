using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public abstract class ManyToOneCodeListsParserXML<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected ManyToOneCodeListsParserXML(string[] downloadLinks)
		{
			this.downloadLinks = downloadLinks;
		}
		readonly string[] downloadLinks;

		public async Task<string> DownloadAndConvertToRefCusCodeListXML(HttpClient client, string outPutFilePath)
		{
			ErrorBuilder.Clear();

			var dictionaryOfDownloadedCodeLists = new Dictionary<string, ParseResult<TKeyValues>>();
			foreach (var customsCodeListIdentifier in CustomsCodeListIdentifiers)
			{
				var downloadResult = await Download(client, customsCodeListIdentifier);
				var codeListXDoc = LoadAsXDocument(downloadResult);
				var keyValues = Populate(customsCodeListIdentifier, codeListXDoc);
				var publicationDate = ParsePublicationDate(downloadResult, codeListXDoc);
				dictionaryOfDownloadedCodeLists.Add(customsCodeListIdentifier, new ParseResult<TKeyValues>(customsCodeListIdentifier, keyValues, publicationDate));
			}

			var result = ConvertAndCombineCodeLists(dictionaryOfDownloadedCodeLists);
			if (result.Any())
			{
				var publicationDate = GetPublicationDate(dictionaryOfDownloadedCodeLists);
				Helper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outPutFilePath, OutputFileName), XmlWriterConfiguration, publicationDate, result);
			}

			return ErrorBuilder.ToString();
		}

		static DateTime ParsePublicationDate(DownloadResult downloadResult, XDocument codeListXDoc)
		{
			var (successfullyParsed, dateTime) = (codeListXDoc.Element("Codelist")?.Attribute("PublicationDate")?.Value).GetDateTime(CodeListsConstants.SourceDateFormatXML);
			var publicationDate = successfullyParsed ? dateTime : downloadResult?.LastModified.LocalDateTime ?? DateTime.MinValue;
			return publicationDate;
		}

		List<TKeyValues> Populate(string customsCodeListIdentifier, XDocument codeListXDoc)
		{
			var codesToImport = new Dictionary<string, TKeyValues>();
			foreach (var entry in codeListXDoc.Descendants("Entry"))
			{
				var keyValues = GetKeyValues(entry);
				if (ValidateData(keyValues))
				{
					AddOrUpdateCodesToImport(codesToImport, keyValues);
				}
				else
				{
					AppendInvalidDataErrorDetails(customsCodeListIdentifier, keyValues, entry);
				}
			}

			return codesToImport.Where(x => !x.Value.EndDateIsExpired).Select(code => code.Value).ToList();
		}

		async Task<DownloadResult> Download(HttpClient client, string customsCodeListIdentifier)
		{
			DownloadResult result = null;
			var downloadLink = downloadLinks.SingleOrDefault(s => s.Contains(customsCodeListIdentifier));
			if (downloadLink != null)
			{
				result = await CodeListsDownloaderXML.Download(client, downloadLink);
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"No download link found for {GetType()} code list: {customsCodeListIdentifier}");
			}

			return result;
		}

		static XDocument LoadAsXDocument(DownloadResult downloadResult)
		{
			var result = new XDocument();
			if (downloadResult != null)
			{
				using (var stream = new MemoryStream())
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(downloadResult.Content);
					writer.Flush();
					stream.Position = 0;

					result = XDocument.Load(stream);
				}
			}
			return result;
		}

		protected virtual void AddOrUpdateCodesToImport(Dictionary<string, TKeyValues> codesToImport, TKeyValues newKeyValues)
		{
			var resultKeyValues = newKeyValues;
			var codeIsDuplicate = codesToImport.TryGetValue(newKeyValues.UniqueCode, out var alreadyAddedRecord);
			if (codeIsDuplicate)
			{
				resultKeyValues = alreadyAddedRecord;
				if (newKeyValues.EndDate > alreadyAddedRecord.EndDate)
				{
					resultKeyValues = newKeyValues;
					if (alreadyAddedRecord.StartDate < newKeyValues.StartDate)
					{
						resultKeyValues.SetStartDateDetails(alreadyAddedRecord.StartDateSuccessfullyParsed, alreadyAddedRecord.StartDate);
					}
				}
				else if (newKeyValues.StartDate < alreadyAddedRecord.StartDate)
				{
					resultKeyValues.SetStartDateDetails(newKeyValues.StartDateSuccessfullyParsed, newKeyValues.StartDate);
				}
			}
			codesToImport[resultKeyValues.UniqueCode] = resultKeyValues;
		}

		protected DateTime GetPublicationDate(Dictionary<string, ParseResult<TKeyValues>> parseResults) => parseResults.Values.Max(x => x.PublicationDate);

		protected bool ValidateData(TKeyValues keyValues)
			=> !string.IsNullOrWhiteSpace(keyValues.Code) && !string.IsNullOrWhiteSpace(keyValues.Description) && keyValues.StartDateSuccessfullyParsed && keyValues.EndDateSuccessfullyParsed;

		protected abstract List<T> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<TKeyValues>> parseResults);
		protected abstract string[] CustomsCodeListIdentifiers { get; }
		protected abstract string CodeType { get; }
		protected abstract string OutputFileNameSuffix { get; }
		protected abstract XmlWriterConfiguration XmlWriterConfiguration { get; }

		protected virtual string CodeAttributeName => CodeListsConstants.XMLEntryElementNames.CODE;

		protected abstract TKeyValues GetKeyValues(XElement entry);

		protected virtual void AppendInvalidDataErrorDetails(string customsCodeListIdentifier, TKeyValues keyValues, XElement entry)
		{
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $@"Unable to import record from {customsCodeListIdentifier} due to empty Code, Description or invalid Date.");
			CodeListsHelper.GetBaseErrorDetailsXML(ErrorBuilder, keyValues, entry, CodeAttributeName);
		}

		protected virtual string XMLWriterDataSource => $"DE {CodeType}";
		protected StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
		string OutputFileName => $"{CodeListsConstants.OutputFileNamePrefix}{OutputFileNameSuffix}.xml";
	}

	public class ParseResult<TKeyValues> where TKeyValues : IKeyValues
	{
		public string CustomsCodeListIdentifier { get; }
		public List<TKeyValues> List { get; }
		public DateTime PublicationDate { get; }

		public ParseResult(string customsCodeListIdentifier, List<TKeyValues> list, DateTime publicationDate)
		{
			CustomsCodeListIdentifier = customsCodeListIdentifier;
			List = list;
			PublicationDate = publicationDate;
		}
	}
}
