using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.RefDbRepo.CAReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode
{
	public class CustomsOfficeCodeParser
	{
		public CustomsOfficeCodeParser(string exportFilePath, string usPortOfExitMappingFileName, DateTime configPublicationTime)
		{
			this.exportFilePath = exportFilePath;
			this.usPortOfExitMappingFileName = usPortOfExitMappingFileName;
			this.configPublicationTime = configPublicationTime;
			_xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusCodelistConfiguration(Constants.CodeType.CUSOF));
		}
		readonly string exportFilePath;
		readonly string usPortOfExitMappingFileName;
		readonly DateTime configPublicationTime;
		const string dataXpath = "/html/body/div[1]/div[3]/div/div[1]/div";
		readonly IXmlWriter _xmlWriter;

		public void ProduceData(string officeCodeUrl, string detailHtml = null)
		{
			var webpageHtml = detailHtml ?? WebScraper.GetUrlDownload(officeCodeUrl);
			var html = new HtmlDocument();
			html.LoadHtml(webpageHtml);

			PopulateData(html);
			_xmlWriter.SetPublicationTime(GetPublicationTime(html));
			_xmlWriter.SetUpdateType(UpdateType.Full);
			_xmlWriter.SetDataSource(Constants.DataSource.OfficeCode);
			_xmlWriter.SaveXml(exportFilePath);
			Console.WriteLine($"Final XML populated {exportFilePath}.");
		}

		void PopulateData(HtmlDocument html)
		{
			var officeCodeLists = ExtractOfficeCode(html);
			Console.WriteLine($"There will be {officeCodeLists.Count()} records populated into the final XML");
			var usPortOfExitMapping = CsvLoader.GetUSPortOfExitMapping(usPortOfExitMappingFileName);
			foreach (var officeCodeList in officeCodeLists)
			{
				var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
				var province = CAProvinceLookup.GetProvince(officeCodeList.ZZD_Code?.ToString() ?? string.Empty);
				if (!string.IsNullOrEmpty(province))
				{
					refCusCodeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = Constants.AttributeName.Province, ZZE_Value = province });
				}
				if (usPortOfExitMapping.TryGetValue(officeCodeList.ZZD_Code, out var usPort))
				{
					refCusCodeListAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = Constants.AttributeName.USPortOfExit, ZZE_Value = usPort });
				}
				if (refCusCodeListAttributes.Any())
				{
					officeCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
				}
				_xmlWriter.PopulateData(officeCodeList);
			}
		}

		DateTime GetPublicationTime(HtmlDocument html)
		{
			var paragraphs = html.DocumentNode.SelectSingleNode(dataXpath).SelectNodes("descendant::dl");
			_ = paragraphs.Reverse();
			DateTime newDate = configPublicationTime;
			foreach (var paragraph in paragraphs)
			{
				var innerText = paragraph.InnerText;
				if (innerText.Contains("Modified:"))
				{
					var newDateString = paragraph.SelectSingleNode("descendant::dd").InnerText.Trim();
					if (DateTime.TryParse(newDateString, out DateTime newPublishDate) && newPublishDate > newDate)
					{
						newDate = newPublishDate;
					}
					break;
				}
			}
			return newDate == DateTime.MinValue ? DateTime.Today : newDate;
		}

		static IEnumerable<RefCusCodeList> ExtractOfficeCode(HtmlDocument html)
		{
			var canadianPortCodes = html.DocumentNode.SelectSingleNode(dataXpath);
			if (canadianPortCodes != null)
			{
				var dtNodes = canadianPortCodes.SelectNodes("descendant::dt");
				var ddNodes = canadianPortCodes.SelectNodes("descendant::dd");
				var codeCount = Math.Min(dtNodes.Count, ddNodes.Count);
				for (int i = 0; i < codeCount; i++)
				{
					var dtInnerText = dtNodes[i].InnerText;
					var ddInnerText = ddNodes[i].InnerText;
					var codeSet = new HashSet<string>();
					if (dtInnerText.Contains(CustomOfficeName) && ddInnerText.Contains(Code))
					{
						var code = ddInnerText.Replace(Code, "").Trim();
						if (!string.IsNullOrEmpty(code) && !codeSet.Contains(code))
						{
							var codeList = new RefCusCodeList()
							{
								ZZD_Code = code,
								ZZD_Description = HttpUtility.HtmlDecode(dtInnerText.Replace(CustomOfficeName, "").Trim())
							};
							codeSet.Add(code);
							yield return codeList;
						}
					}
				}
			}
		}
		const string CustomOfficeName = "Custom Office Name:";
		const string Code = "Code:";
	}
}
