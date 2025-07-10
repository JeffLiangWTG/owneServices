using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using CargoWise.RefDbRepo.XmlProducer.Common;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using NDesk.Options;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	class Program
	{
		#region Enums

		enum ItemOption
		{
			MODULE_EXCHANGE_RATE,
			WEB_TARIFF,
			WEB_NOMENCLATURE,
			WEB_TRADE_GROUP,
			LOCAL_TWRR,
			LOCAL_TWRF,
			LOCAL_TWCR,
			WEB_PACKING_HOUSE,
			WEB_TWCIU,
			WEB_SCECA,
			WEB_ICI,
			WEB_FAC,
			WEB_TWER,
			WEB_TWIR,
			WEB_TWCA,
			WEB_CUSOF,
			WEB_CAACC,
			WEB_CAAC,
			LOCAL_TWET,
			WEB_TWREJ,
			WEB_TWREQ,
			WEB_TWREW,
		}

		#endregion

		static int Main(string[] args)
		{
			return (int)ProduceXml(args);
		}

		static ProducerStatus ProduceXml(string[] args)
		{
			bool showHelp = false;
			bool showItemHelp = false;
			bool forceDownload = false;
			HashSet<string> items = new HashSet<string>();
			HashSet<string> fileNames = new HashSet<string>();

			var optionSet = new OptionSet()
			{
				{ "I|ITEM:", "the {ITEM} to download.", v => {if (Enum.GetNames(typeof(ItemOption)).Contains(v)) items.Add(v); } },
				{ "F|FILE=", "the {PATH} to store XML file.", v => fileNames.Add(v) },
				{ "h|help",  "show this message and exit.", v => showHelp = v != null },
				{ "H|Help",  "show list of available items", v => showItemHelp = v != null },
				{ "f|force",  "force download", v => forceDownload = v != null },
			};

			optionSet.Parse(args);

			if (showHelp)
			{
				ShowHelp(optionSet);
				return ProducerStatus.Failure;
			}

			if (showItemHelp)
			{
				Console.WriteLine("Valid options for -i|ITEM are:");
				foreach (var itemOption in Enum.GetNames(typeof(ItemOption)))
				{
					Console.WriteLine(itemOption);
				}
				return ProducerStatus.Failure;
			}

			if (items.Count == 0)
			{
				foreach (var itemOption in Enum.GetNames(typeof(ItemOption)))
				{
					items.Add(itemOption);
				}
			}

			if (fileNames.Count == 0 || fileNames.Count != items.Count)
			{
				Console.WriteLine("Missing required value for option '-F'.");
				PrintHelpMessage();
				return ProducerStatus.Failure;
			}

			var configurationBuilder = new ConfigurationBuilder();
			var config = configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.TaiwanReferenceData.config.json").Build();

			var binding = new System.ServiceModel.BasicHttpBinding();
			var endpointAddress = new System.ServiceModel.EndpointAddress(config["endpoint"]);
			var client = new UpdateInfoServiceClient(binding, endpointAddress);

			try
			{
				foreach (var item in items)
				{
					var concurrentDownloading = false;

					var publicationDateTime = SystemContext.Now();
					var info = new updateInfoBean();
					var responseString = string.Empty;
					switch (Enum.Parse(typeof(ItemOption), item, true))
					{
						case ItemOption.WEB_TARIFF:
							info.downloadURL = string.Join(";", new string[] { AppConfig.Tariff.DownloadUrl.TariffColumn1And3, AppConfig.Tariff.DownloadUrl.TariffColumn2, AppConfig.Tariff.DownloadUrl.TariffChineseDescription, AppConfig.Nomenclature.DownloadUrl.HsSectionsAndChapters, AppConfig.Tariff.DownloadUrl.EnvironmentalProtectionTariffs, AppConfig.Tariff.DownloadUrl.TariffEnglishDescription });
							publicationDateTime = Utility.GetTariffLastUpdateDateTimeFromHtml(Utility.GetResponseString(AppConfig.Tariff.DownloadUrl.PublicationDateTime));
							break;
						case ItemOption.WEB_NOMENCLATURE:
							info.downloadURL = string.Join(";", new string[] { AppConfig.Nomenclature.DownloadUrl.HsSectionsAndChapters, AppConfig.Nomenclature.DownloadUrl.HsNomenclatureDescription, AppConfig.Nomenclature.DownloadUrl.HsNomenclatureEnglishDescription });
							break;
						case ItemOption.WEB_TRADE_GROUP:
							info.downloadURL = string.Join(";", new string[] { AppConfig.TradeGroup.DownloadUrl.Countries, AppConfig.TradeGroup.DownloadUrl.WtoFtaCountries, AppConfig.Tariff.DownloadUrl.TariffColumn2 });
							break;
						case ItemOption.MODULE_EXCHANGE_RATE:
							info.downloadURL = string.Join(";", new string[] { AppConfig.ExchangeRate.DownloadUrl.ExchangeRateText, AppConfig.ExchangeRate.DownloadUrl.ExchangeRateJSON });
							break;
						case ItemOption.WEB_PACKING_HOUSE:
							info.downloadURL = Utility.GetPackingHouseListFileUrlFromHtml(Utility.GetResponseString(AppConfig.CodeLists.TaiwanPackingHouse.Url.DownloadUrl));
							break;
						case ItemOption.WEB_TWCIU:
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanUnitsOfMeasurement.DownloadUrl);
							info.downloadURL = Utility.GetUnitsOfMeasurementFileUrlFromHtml(responseString);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
						case ItemOption.WEB_TWCA:
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanControllingAgency.DownloadUrl);
							info.downloadURL = Utility.GetAttachmentFileUrlFromHtml(responseString);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
						case ItemOption.WEB_CUSOF:
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanCustomsOffices.DownloadUrl);
							info.downloadURL = Utility.GetAttachmentFileUrlFromHtml(responseString);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
						case ItemOption.WEB_CAACC:
							var downloadPageUrl = AppConfig.CodeLists.TaiwanAircraftPartCAACodeCategory.DownloadUrl;
							responseString = Utility.GetResponseString(downloadPageUrl);
							info.downloadURL = downloadPageUrl;
							publicationDateTime = Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml(responseString);
							break;
						case ItemOption.WEB_CAAC:
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanAircraftPartCAACodeCategory.DownloadUrl);
							info.downloadURL = GetTaiwanAircraftPartCAACodeDownloadURL(responseString);
							publicationDateTime = Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml(responseString);
							concurrentDownloading = true;
							break;
						case ItemOption.WEB_SCECA:
							using (var webClient = new WebClientWrapper())
							{
								responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanSCECA.DownloadUrl);
								info.downloadURL = Utility.GetAttachmentFileUrlFromHtml(responseString, "file-pdf");
								publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							}
							break;
						case ItemOption.WEB_ICI:
							using (var webClient = new WebClientWrapper())
							{
								var iciDownloadURLAndPublishDateTime = GetCommodityInspectionDownloadURLAndPublishDateTime();
								info.downloadURL = iciDownloadURLAndPublishDateTime.downloadURL;
								publicationDateTime = iciDownloadURLAndPublishDateTime.publishDateTime;
							}
							concurrentDownloading = true;
							break;
						case ItemOption.WEB_FAC:
							var facilityDownloadURLAndPublishDateTime = GetFacilityDownloadURLAndPublishDateTime();
							info.downloadURL = facilityDownloadURLAndPublishDateTime.downloadURL;
							publicationDateTime = facilityDownloadURLAndPublishDateTime.publishDateTime;
							concurrentDownloading = true;
							break;
						case ItemOption.WEB_TWER:
							info.downloadURL = AppConfig.CodeLists.TaiwanExportRegulations.DownloadUrl;
							break;
						case ItemOption.WEB_TWIR:
							info.downloadURL = AppConfig.CodeLists.TaiwanImportRegulations.DownloadUrl;
							break;
						case ItemOption.LOCAL_TWET:
							publicationDateTime = DateTime.Parse(AppConfig.Tariff.ExciseTaxesFileNamePublicationTime, CultureInfo.InvariantCulture);
							break;
						case ItemOption.WEB_TWREJ:
							info.downloadURL = AppConfig.CodeLists.TaiwanCPT016RejectionReason.DownloadUrl;
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanCPT016RejectionReason.PublicationDateTime);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
						case ItemOption.WEB_TWREQ:
							info.downloadURL = AppConfig.CodeLists.TWCPT_017_Error_DocumentOrRequiredFormalities.DownloadUrl;
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TWCPT_017_Error_DocumentOrRequiredFormalities.PublicationDateTime);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
						case ItemOption.WEB_TWREW:
							info.downloadURL = AppConfig.CodeLists.TaiwanTWCPT_018_ResponseToWarehouse.DownloadUrl;
							responseString = Utility.GetResponseString(AppConfig.CodeLists.TaiwanTWCPT_018_ResponseToWarehouse.PublicationDateTime);
							publicationDateTime = Utility.GetPublicationDateTimeUseRegex(responseString);
							break;
					}
					info.updateItem = item;
					var versionBuilder = new StringBuilder();
					byte[] downloadedData = null;

					IEnumerable<byte[]> ordereddownloadedDatas = Enumerable.Empty<byte[]>();
					if (concurrentDownloading)
					{
						var downloadUrl = info.downloadURL;
						var urls = downloadUrl.Split(';');
						var downloadedDatas = new ConcurrentBag<(int Index, byte[] Data)>();

						var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };
						Parallel.ForEach(urls, options, url =>
						{
							using (var webClient = new WebClientWrapper())
							{
								downloadedData = webClient.DownloadData(url);
								downloadedDatas.Add((downloadUrl.IndexOf(url), downloadedData));
							}
						});

						ordereddownloadedDatas = downloadedDatas.OrderBy(x => x.Index).Select(x => x.Data);
						foreach (var data in ordereddownloadedDatas)
						{
							using (var file = new MemoryStream(data))
							using (var md5 = MD5.Create())
							using (var reader = new StreamReader(file))
							{
								var content = reader.ReadToEnd();
								versionBuilder.Append(BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(content))).Replace("-", "").ToLowerInvariant());
							}
						}
					}
					else if(info.downloadURL != null)
					{
						foreach (var url in info.downloadURL.Split(';'))
						{
							using (var webClient = new WebClientWrapper())
							{
								downloadedData = webClient.DownloadData(url);
								using (var file = new MemoryStream(downloadedData))
								using (var md5 = MD5.Create())
								using (var reader = new StreamReader(file))
								{
									var content = reader.ReadToEnd();
									if (url == AppConfig.TradeGroup.DownloadUrl.WtoFtaCountries)
									{
										content = TradeGroupUpdateInfo.ProcessWtoFtaTable(content);
									}

									versionBuilder.Append(BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(content))).Replace("-", "").ToLowerInvariant());
								}
							}
						}
					}

					info.version = versionBuilder.ToString();

					if (!forceDownload && File.Exists(item))
					{
						using (var file = File.Open(item, FileMode.Open))
						{
							var serializer = new XmlSerializer(typeof(updateInfoBean));
							if (((updateInfoBean)serializer.Deserialize(file)).version == info.version)
							{
								Console.WriteLine(string.Format("Skip {0}", item));
								continue;
							}
						}
					}
					RefDataUpdateInfo updateInfo = null;
					switch (Enum.Parse(typeof(ItemOption), item, true))
					{
						case ItemOption.MODULE_EXCHANGE_RATE:
							updateInfo = new ExchangeRateUpdateInfo(info, fileNames.ElementAt(items.ToList().IndexOf(item)));
							break;
						case ItemOption.WEB_TARIFF:
							updateInfo = new TariffUpdateInfo(info, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime);
							break;
						case ItemOption.WEB_NOMENCLATURE:
							updateInfo = new NomenclatureUpdateInfo(info, fileNames.ElementAt(items.ToList().IndexOf(item)));
							break;
						case ItemOption.WEB_TRADE_GROUP:
							updateInfo = new TradeGroupUpdateInfo(info, fileNames.ElementAt(items.ToList().IndexOf(item)));
							break;
						case ItemOption.LOCAL_TWRR:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.RejectionReason, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime);
							break;
						case ItemOption.LOCAL_TWRF:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.RequiredFormalities, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime);
							break;
						case ItemOption.LOCAL_TWCR:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCustomsRequirements, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime);
							break;
						case ItemOption.WEB_TWIR:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanImportRegulations, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_TWER:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanExportRegulations, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_PACKING_HOUSE:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanPackingHouse, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_TWCIU:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanUnitsOfMeasurement, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_TWCA:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanControllingAgency, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_CUSOF:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCustomsOffices, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_CAACC:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanAircraftPartCAACodeCategory, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime, responseString);
							break;
						case ItemOption.WEB_CAAC:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanAircraftPartCAACCode, fileNames.ElementAt(items.ToList().IndexOf(item)), ordereddownloadedDatas, publicationDateTime);
							break;
						case ItemOption.WEB_SCECA:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanSCECA, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_ICI:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCommodityInspection, fileNames.ElementAt(items.ToList().IndexOf(item)), ordereddownloadedDatas, publicationDateTime);
							break;
						case ItemOption.WEB_FAC:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, fileNames.ElementAt(items.ToList().IndexOf(item)), ordereddownloadedDatas, publicationDateTime);
							break;
						case ItemOption.LOCAL_TWET:
							var filePath = Path.Combine(FolderHelper.GetBinFolder(), AppConfig.Tariff.ExciseTaxesFileName);
							updateInfo = new TaiwanExciseTaxesUpdateInfo(info, filePath, fileNames.ElementAt(items.ToList().IndexOf(item)), publicationDateTime);
							break;
						case ItemOption.WEB_TWREJ:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TWCPT016RejectionReason, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_TWREQ:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TWCPT_017_Error_DocumentOrRequiredFormalities, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
						case ItemOption.WEB_TWREW:
							updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.CPT018ResponseToWarehouse, fileNames.ElementAt(items.ToList().IndexOf(item)), downloadedData, publicationDateTime);
							break;
					}

					updateInfo?.Run();

					using (var file = File.Create(item))
					{
						var serializer = new XmlSerializer(typeof(updateInfoBean));
						serializer.Serialize(file, info);
					}
				}
			}
			finally
			{
				Directory.Delete(Utility.TempDirectory, true);
			}
			return ProducerStatus.Success;
		}

		static (string downloadURL, DateTime publishDateTime) GetCommodityInspectionDownloadURLAndPublishDateTime()
		{
			var downloadURLList = new List<string>();
			var publishDateTimeList = new List<DateTime>();
			var commodityInspectionDownloadPageUrls = new string[] { AppConfig.CodeLists.TaiwanCommodityInspection.DownloadUrl.ExamingZoneKeelung, AppConfig.CodeLists.TaiwanCommodityInspection.DownloadUrl.ExamingZoneTaipei, AppConfig.CodeLists.TaiwanCommodityInspection.DownloadUrl.ExamingZoneTaichung, AppConfig.CodeLists.TaiwanCommodityInspection.DownloadUrl.ExamingZoneKaohsiung };
			foreach (var downloadPageUrl in commodityInspectionDownloadPageUrls)
			{
				var responseString = Utility.GetResponseString(downloadPageUrl);
				downloadURLList.Add(Utility.GetAttachmentFileUrlFromHtml(responseString));
				publishDateTimeList.Add(Utility.GetPublicationDateTimeUseRegex(responseString));
			}
			return (string.Join(";", downloadURLList), publishDateTimeList.Max());
		}

		static (string downloadURL, DateTime publishDateTime) GetFacilityDownloadURLAndPublishDateTime()
		{
			var responseStringArray = new string[] { Utility.GetResponseString(AppConfig.CodeLists.FacilityCode.DownloadUrl.DischargingStoringKeelung), Utility.GetResponseString(AppConfig.CodeLists.FacilityCode.DownloadUrl.DischargingStoringTaipei), Utility.GetResponseString(AppConfig.CodeLists.FacilityCode.DownloadUrl.DischargingStoringTaichung), Utility.GetResponseString(AppConfig.CodeLists.FacilityCode.DownloadUrl.DischargingStoringKaohsiung) };
			var taiwanCustomsUrl = AppConfig.CodeLists.Shared.BaseUrl;
			var downloadURL = string.Join(";",
				Utility.GetDischargingStoringFileUrlsFromHtml(responseStringArray[0], taiwanCustomsUrl),
				Utility.GetDischargingStoringFileUrlsFromHtml(responseStringArray[1], taiwanCustomsUrl),
				Utility.GetDischargingStoringFileUrlsFromHtml(responseStringArray[2], taiwanCustomsUrl),
				Utility.GetDischargingStoringFileUrlsFromHtml(responseStringArray[3], taiwanCustomsUrl));
			var publishDateTimeArray = new DateTime[] { Utility.GetPublicationDateTimeUseRegex(responseStringArray[0]), Utility.GetPublicationDateTimeUseRegex(responseStringArray[1]), Utility.GetPublicationDateTimeUseRegex(responseStringArray[2]), Utility.GetPublicationDateTimeUseRegex(responseStringArray[3]) };
			return (downloadURL, publishDateTimeArray.Max());
		}

		static string GetTaiwanAircraftPartCAACodeDownloadURL(string html)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);

			var downloadurls = new HashSet<string>();
			var downloadFilebaseNodes = htmlDocument.DocumentNode.SelectNodes($"//a[@class='download-filebase']");
			var codeWithDescriptionRegex = new Regex("第(?<chineseNumber>.*)類：(.*)(.(?i)odt)");
			foreach (var node in downloadFilebaseNodes)
			{
				var title = node.Attributes["title"].Value;
				var match = codeWithDescriptionRegex.Match(title);
				if (match.Success)
				{
					downloadurls.Add($"{AppConfig.CodeLists.TaiwanAircraftPartCAACCode.DownloadUrl}{node.Attributes["href"].Value}");
				}
			}
			return string.Join(";", downloadurls);
		}

		static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: TaiwanReferenceData.exe [OPTIONS]+ message");
			Console.WriteLine("Greet a list of individuals with an optional message.");
			Console.WriteLine("If no message is specified, a generic greeting is used.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}

		static void PrintHelpMessage()
		{
			Console.WriteLine("Try 'TaiwanReferenceData.exe --help' for more information.");
		}
	}
}
