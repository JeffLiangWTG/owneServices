using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FsisEstNumbersCrawler.Properties;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;
using FsisEstNumbersCrawler.Models;
using FsisEstNumbersCrawler.Services;
using HtmlAgilityPack;
using Path = System.IO.Path;
using RefCusCodeList = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCodeList;
using RefCusCodeListAttribute = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCodeListAttribute;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.FsisEstNumbersCrawler.Test")]
namespace FsisEstNumbersCrawler
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			var outputFilePath = GetOutputFilePath();
			ServicesProvider.Logger.Info($"The out put file path is {outputFilePath}");

			Run(outputFilePath);

			ServicesProvider.Logger.Succeed($"Job completed! The file has been saved to {outputFilePath}");
		}

		static void Run(string outputFilePath)
		{
			var dataFilelLink = string.Empty;
			var localDataFile = string.Empty;
			var establishments = new List<Establishment>();

#pragma warning disable SYSLIB0014 // Type or member is obsolete
			try
			{
				using (var webClient = new WebClient())
				{
					webClient.Headers.Add(HttpRequestHeader.Cookie, "cookies");

					ServicesProvider.Logger.Info("Start searching the data file node in the web page...");
					var dataFileNode = ServicesProvider.HtmlParser.FindNode(Settings.Default.ServiceUrl, IsDataFileNode, webClient);

					if (dataFileNode == null)
					{
						throw new ArgumentException("The data file node is not found in the web page, the page layout may have changed.");
					}

					ServicesProvider.Logger.Info("Successfuly found the data file node.");

					ServicesProvider.Logger.Info("Start searching find the publication time node in the web page...");
					var publicationTimeNode = ServicesProvider.HtmlParser.FindNode(dataFileNode, IsDateNode);

					if (publicationTimeNode == null)
					{
						throw new ArgumentException("The publication time node is not found in the web page, the page layout may have changed.");
					}

					ServicesProvider.Logger.Info("Successfully found the publication time.");

					var publicationTime = ParseInnerTime(publicationTimeNode);
					ServicesProvider.Logger.Info($"The publication time is {publicationTime}");

					dataFilelLink = ServicesProvider.HtmlParser.GetAttributeValue(dataFileNode, "href");

					if (string.IsNullOrEmpty(dataFilelLink))
					{
						throw new ArgumentException("The download link is not found int the web page, the page layout may have changed.");
					}

					if (Path.IsPathRooted(dataFilelLink))
					{
						var uri = new Uri(new Uri(Settings.Default.ServiceHost), dataFilelLink);
						dataFilelLink = uri.AbsoluteUri;
					}

					ServicesProvider.Logger.Info($"The data file download link is {dataFilelLink}");
				}
#pragma warning disable SYSLIB0014 // Type or member is obsolete

				ServicesProvider.Logger.Info("Start downloading data file...");
				localDataFile = GetTempFileName();
				ServicesProvider.Downloader.Download(dataFilelLink, localDataFile);
				ServicesProvider.Logger.Info($"The  data file has been downloaded to {localDataFile}");
			}
			catch (Exception ex)
			{
				ServicesProvider.Logger.Info("FSIS Est Numbers file download failed: " + ex.ToString());
			}

			try
			{
				establishments = ServicesProvider.CsvParser.Parse<Establishment>(localDataFile);
				ServicesProvider.Logger.Info("Successfully parsed the data to establishments.");

				ServicesProvider.Logger.Info("Start serializing the establishments to xml file.");
			}
			catch (Exception ex)
			{
				ServicesProvider.Logger.Info("FSIS csv file Parsing failed: " + ex.ToString());
			}

			try
			{
				ServicesProvider.Logger.Info("Start parsing the data in data file to establishments.");

				var fileName = Path.Combine(Settings.Default.OutputPath, "US_FSISEstNumbers.xml");
				var refCusCodeLists = GetRefCusCodeLists(establishments);
				if (refCusCodeLists.Any())
				{
					var xmlWriterConfiguration = GetRefCusCodeListWriterConfiguration(Settings.Default.ZZD_ZZK_NKCodeType);
					XmlWriterHelper.ExportToXMLFile(Settings.Default.DataSource, fileName, xmlWriterConfiguration, DateTime.Today, refCusCodeLists);
				}
			}
			catch (Exception ex)
			{
				ServicesProvider.Logger.Info("FSIS Est Numbers XML file generation failed: " + ex.ToString());
			}
		}

		public static List<RefCusCodeList> GetRefCusCodeLists(List<Establishment> lists)
		{
			var result = new List<RefCusCodeList>();
			if (lists.Count > 0)
			{
				foreach (Establishment oneItem in lists)
				{
					var multiCodes = oneItem.ToCodeLists();
					foreach (RefCusCodeList oneCode in multiCodes)
					{
						var refCusCodeList = new RefCusCodeList()
						{
							ZZD_Code = oneCode.ZZD_Code,
							ZZD_Description = oneCode.ZZD_Description
						};
						refCusCodeList.ZZD_StartDate = oneCode.ZZD_StartDate;
						refCusCodeList.ZZD_EndDate = oneCode.ZZD_EndDate;

						var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
						if (oneCode.RefCusCodeListAttributes != null)
						{
							foreach (var attribute in oneCode.RefCusCodeListAttributes)
							{
								var refCusCodeListAttribute = new RefCusCodeListAttribute()
								{
									ZZE_ZXE_NKName = attribute.ZZE_ZXE_NKName,
									ZZE_Value = attribute.ZZE_Value
								};

								refCusCodeListAttributes.Add(refCusCodeListAttribute);
							}

							refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
						}

						result.Add(refCusCodeList);
					}
				}
			}

			return result;
		}

		static string GetOutputFilePath()
		{
			var fileName = Path.Combine(Settings.Default.OutputPath, "US_FSISEstNumbers.xml");
			var dirName = Path.GetDirectoryName(fileName);
			Directory.CreateDirectory(dirName);

			return fileName;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, DateTime? startDate = null, bool enableAttribute = true, bool enableTransport = false, bool isKeyColumnForAttributeValue = true)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, startDate ?? new DateTime(1900, 01, 01, 0, 0, 0));
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "US");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			if (enableAttribute)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes);
				var refCusCodeListAttribute = new EntityTypeConfiguration<CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefCusCodeListAttribute>(true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, isKeyColumnForAttributeValue);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);
			}

			return writerConfiguration;
		}


		public static bool IsDataFileNode(HtmlNode node)
		{
			return (node.InnerText == "XLS" || node.InnerText == "XLSX" || node.InnerText == "CSV") && node.ParentNode.SelectSingleNode("a/strong").InnerText == "MPI Directory: Alphabetically by Establishment Name";
		}

		public static bool IsDateNode(HtmlNode node)
		{
			var previousSibling = node.PreviousSibling;
			return previousSibling != null && IsDataFileNode(previousSibling);
		}

		static DateTime ParseInnerTime(HtmlNode node)
		{
			var match = Regex.Match(HtmlEntity.DeEntitize(node.InnerText), "\\((.*?)\\)");
			if (match.Success)
			{
				if (DateTime.TryParse(match.Groups[1].Value, out var time))
				{
					return time;
				}
			}

			ServicesProvider.Logger.Warn($"Cannot parse publication time from the HTML node, the inner text is {node.InnerText}, use current time instead.");
			return DateTime.Now;
		}

		static string GetTempFileName()
		{
			return Path.GetTempPath() + "tempEstablishment" + DateTime.Now.Millisecond + ".csv";
		}
	}
}

