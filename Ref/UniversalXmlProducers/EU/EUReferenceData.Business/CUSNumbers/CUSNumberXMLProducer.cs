using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business.CUSNumbers;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class CUSNumberXMLProducer
	{
		public CUSNumberXMLProducer(IHttpClientHelper httpClientHelper)
		{
			this.httpClientHelper = httpClientHelper;
		}

		public string DownloadAndConvertToCUSNumberXML(string outputPath, int pagesPerBatch)
		{
			ErrorBuilder.Clear();
			var publicationTime = DateTime.MinValue;
			try
			{
				// publication date from index page
				var indexPage = httpClientHelper.GetWebPageAsync(ApplicationConfig.Instance.CUSNumberEntryPointUrl).GetAwaiter().GetResult();
				if (!string.IsNullOrEmpty(indexPage))
				{
					publicationTime = new IndexPageParser(indexPage).ExtractPublicationTime();
				}
				if (publicationTime == DateTime.MinValue)
				{
					ProcessInvalidData(indexPage, outputPath);
				}
			}
			catch (Exception e)
			{
				throw new CUSNumbersException($"Could not get publication date from index page: {e.GetBaseException()}");
			}

			var cusCodeAttributeProvider = new CachedCusCodeAttributeProvider(ErrorBuilder, httpClientHelper, publicationTime);
			var refCusCodeListProducer = new CUSNumberRefCusCodeListProducer(cusCodeAttributeProvider);

			var batchNumber = 1;
			var eof = false;
			while (!eof)
			{
				eof = DownloadAndConvertToCUSNumberXMLBatch(batchNumber, pagesPerBatch, publicationTime, outputPath, refCusCodeListProducer);
				batchNumber++;
			}
			return ErrorBuilder.ToString();
		}

		void ProcessInvalidData(string indexPage, string outputPath)
		{
			var message = "Could not find publication date.";
			try
			{
				var path = Path.Combine(outputPath, "DumpFiles", $"RefCusCodeListZZ_DownloadAndConvertToCUSNumberXML_{ProcessInvalidDataTimeStamp:yyyyMMdd_HHmmssfffff}_IndexPage_Error.htm");
				CleanUpDumpFiles(path);

				File.WriteAllText(path, indexPage);
				message += $" Please see index page dump file: {path}";
			}
			catch (Exception ex)
			{
				message += $" Error trying to process invalid data: {ex.Message}";
			}

			throw new InvalidDataException(message);
		}

		static void CleanUpDumpFiles(string path)
		{
			var dirInfo = Directory.CreateDirectory(Path.GetDirectoryName(path));
			var files = dirInfo.GetFiles("RefCusCodeListZZ_DownloadAndConvertToCUSNumberXML_*_IndexPage_Error.htm");
			foreach (var file in files)
			{
				if (file.Exists && file.CreationTime < DateTime.UtcNow.AddDays(-14))
				{
					file.Delete();
				}
			}
		}

		bool DownloadAndConvertToCUSNumberXMLBatch(int batchNumber, int pagesPerBatch, DateTime publicationTime, string outputPath, CUSNumberRefCusCodeListProducer refCusCodeListProducer)
		{
			var cusCodes = new List<string>(pagesPerBatch * Constants.CUSNumbers.ItemsPerPage);

			// it's  2370 pages of 25 items each, 59248 in total.

			var (startIndex, endIndex) = BatchCalculator.CalculateFromToIndexForBatch(batchNumber, pagesPerBatch);
			string pageContent;
			string error;
			bool eof;

			// get the list of all CUS numbers
			var url = string.Empty;
			var offset = startIndex;
			try
			{
				do
				{
					url = GetPageUrl(offset);
					(pageContent, eof, error) = GetListPage(url);
					if (!string.IsNullOrEmpty(error))
					{
						ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"error getting page {url} for index {offset}: {error}");
						eof = true;
						break;
					}
					if (!eof)
					{
						if (ProcessContent(pageContent, cusCodes, url))
						{
							offset += Constants.CUSNumbers.ItemsPerPage;
						}
						else
						{
							eof = true;
							break;
						}
					}

					if (eof || (offset >= endIndex))
					{
						if (cusCodes.Any())
						{
							var cusnumbers = GetDetailsViaSOAP(cusCodes);
							var generatedXML = refCusCodeListProducer.GenerateRefCusCodeListItems(cusnumbers);
							var outputFile = Path.Combine(outputPath, $"RefCusCodeListZZ_CUSNumbers_{publicationTime:yyyyMMdd}_Batch{batchNumber:00000}.xml");
							XmlWriterHelper.ExportToXMLFile($"EU ECICS {batchNumber:00000}", outputFile, XmlWriterConfig, publicationTime, generatedXML, Common.UniversalXmlWriter.UpdateType.Partial);
						}

						break;
					}
				} while (true);
			}
			catch (Exception e)
			{
				throw new CUSNumbersException($"Could not get data from page {url}: {e.GetBaseException()}");
			}
			return eof;
		}

		(string content, bool eof, string error) GetListPage(string url)
		{
			var eof = false;
			var error = string.Empty;
			var content = httpClientHelper.GetWebPageAsync(url).GetAwaiter().GetResult();

			if (content.Contains(Constants.CUSNumbers.NoDataFoundText))
			{
				eof = true;
			}
			else if (!content.Contains(Constants.CUSNumbers.TableText))
			{
				error = "No table found";
			}

			return (content, eof, error);
		}

		bool ProcessContent(string pageContent, ICollection<string> cusnumbers, string url)
		{
			var result = 0;
			var doc = new HtmlDocument();
			doc.LoadHtml(pageContent);

			if (doc.DocumentNode.HasChildNodes)
			{
				var table = doc.DocumentNode.SelectNodes("//*[@id='tblData']").FirstOrDefault();
				if (table is HtmlNode)
				{
					foreach (HtmlNode row in table.Descendants("tr").Skip(1))
					{
						var cells = row.Descendants("td").ToArray();
						if (cells.Length >= 7)
						{
							var code = cells[0].CellValue();

							cusnumbers.Add(code);
							result++;
						}
					}
				}
				else
				{
					ErrorBuilder.Append(CultureInfo.InvariantCulture, $"no tblData in document {url}");
				}
			}
			else
			{
				ErrorBuilder.Append(CultureInfo.InvariantCulture, $"no nodes in document {url}");
			}

			return result > 0;
		}

		IEnumerable<CUSNumber> GetDetailsViaSOAP(List<string> cusCodes)
		{
			var result = new List<CUSNumber>(cusCodes.Count);

			var partitions = EnumerableExtensions.Chunk(cusCodes, ApplicationConfig.Instance.CUSNumberSoapServiceNumberOfItemsPerCall);

			foreach (var partition in partitions)
			{
				var soapRequest = CusNumberSOAPService.CreateRequest(partition);
				try
				{
					var soapCallResult = httpClientHelper.PostAndReadAsAsyncString(ApplicationConfig.Instance.CUSNumberSoapServiceUrl, soapRequest, System.Net.Mime.MediaTypeNames.Text.Xml).GetAwaiter().GetResult();
					var parser = new CUSNumberSOAPResponseParser(soapCallResult);
					var soapCallResults = parser.Parse();

					result.AddRange(soapCallResults.OrderBy(c => c.Code));
				}
				catch (Exception e)
				{
					ErrorBuilder.Append(CultureInfo.InvariantCulture, $"error getting details via SOAP for CUSNumbers {string.Join(", ", partition)} exception: {e.GetBaseException().Message}");
				}
			}

			return result;
		}

		static string GetPageUrl(int offset) => string.Format(CultureInfo.InvariantCulture, ApplicationConfig.Instance.CUSNumberListUrlPattern, offset);

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfig => xmlWriterConfig ?? (xmlWriterConfig = GetRefCusCodeListConfigurationForCUSNumbers());
		Common.UniversalXmlWriter.XmlWriterConfiguration xmlWriterConfig;

		readonly IHttpClientHelper httpClientHelper;

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForCUSNumbers()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ECICS");
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages, false);

			var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, false);

			var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);

			return writerConfiguration;
		}

		protected virtual DateTime ProcessInvalidDataTimeStamp => DateTime.UtcNow;
	}
}
