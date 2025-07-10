using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Path = System.IO.Path;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class ACECargoReleaseSEInputValidationRulesParser
	{
		public ACECargoReleaseSEInputValidationRulesParser(IDownLoadService serviceClient, DateTime processDate)
		{
			ServiceClient = serviceClient;
			ProcessDate = processDate;
		}

		readonly IDownLoadService ServiceClient;
		readonly DateTime ProcessDate;

		public string DownloadAndConvertCodesToXMLFile(string outPutFilePath)
		{
			ErrorBuilder.Clear();
			var htmlNode = ServiceClient.FindNode(ApplicationConfig.Instance.CargoReleaseConditionCodeURL, node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.Contains(KeyWord));
			var fileDownLoaded = ServiceClient.DownloadFile(htmlNode?.OuterHtml ?? string.Empty, ApplicationConfig.Instance.CustomsBorderProtectionGoverment, DownloadFilePath);
			if (fileDownLoaded)
			{
				ExportToXMLFile(outPutFilePath);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to download file from the website. Processing failed.");
			}

			return ErrorBuilder.ToString();
		}
		const string KeyWord = "Cargo Release Condition Codes";

		public void ExportToXMLFile(string outPutFilePath)
		{
			var refCusCodeLists = GetRefCusCodeLists();
			if (refCusCodeLists.Any())
			{
				var xmlWriterConfiguration = XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.ProgramFunctions.ACEValidationRules, enableAttribute: false);
				XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outPutFilePath, OutPutFileName), xmlWriterConfiguration, ProcessDate, refCusCodeLists);
				ErrorBuilder.AppendLine("Processing end.");
			}
			else
			{
				ErrorBuilder.AppendLine("No RefCusCodeList data defined in the file.");
			}
		}

		public static List<RefCusCodeList> GetRefCusCodeLists()
		{
			var result = new List<RefCusCodeList>();
			using (var reader = new PdfReader(DownloadFilePath))
			using (var pdfDocument = new PdfDocument(reader))
			{
				for (var page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
				{
					var strategy = new SimpleTextExtractionStrategy();
					var currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);

					var mainBoby = currentText.Split(new string[] { "Severity Error Message" }, StringSplitOptions.RemoveEmptyEntries);
					if (mainBoby.Length == 2)
					{
						var sections = mainBoby[1].Trim().Split(new string[] { "Rejection", "Pending" }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var section in sections)
						{
							var sectionTrim = section.Trim();
							if (!Regex.IsMatch(sectionTrim, "[a-z]"))
							{
								continue;
							}
							else if (sectionTrim.Length >= 3)
							{
								var code = sectionTrim.Substring(sectionTrim.Length - 3, 3);
								var misc = sectionTrim.Substring(0, sectionTrim.Length - 3).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
								var description = new StringBuilder();
								var haveLowerCaseChar = false;
								foreach (var s in misc)
								{
									if (Regex.IsMatch(s, "[a-z]"))
									{
										haveLowerCaseChar = true;
									}

									if (haveLowerCaseChar && !string.IsNullOrEmpty(s.Trim()))
									{
										if (!string.IsNullOrEmpty(description.ToString()))
										{
											description.Append(' ');
										}
										description.Append(s.Trim().Replace("  ", " "));
									}
								}

								var refCusCodeList = new RefCusCodeList()
								{
									ZZD_Code = code,
									ZZD_Description = description.ToString()
								};
								result.Add(refCusCodeList);
							}
						}
					}
				}
			}

			return result;
		}

		static string XMLWriterDataSource => "ACE Cargo Release (SE) Input Validation Rules";

		static string OutPutFileName => "Cargo_Release_Condition_Codes.xml";

		static string DownloadFilePath => Path.Combine(Path.GetTempPath(), "Cargo Release Condition Codes.pdf");

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
