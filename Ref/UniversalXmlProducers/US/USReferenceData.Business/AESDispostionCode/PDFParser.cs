using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode
{
	public class PDFParser
	{
		public PDFParser(string filePath, string exportFilePath, DateTime publishDate)
		{
			this.filePath = filePath;
			this.exportFilePath = exportFilePath;
			this.publishDate = publishDate;
		}
		readonly string filePath;
		readonly string exportFilePath;
		readonly DateTime publishDate;
		StringBuilder LogBuilder => logBuilder ?? (logBuilder = new StringBuilder());
		StringBuilder logBuilder;

		static string XMLWriterDataSource => "US AES Dispostion Code";

		static string OutPutFileName => "AES_Dispostio_Codes.xml";

		public string ReadPDFAndExportXML()
		{
			var dispostionList = ReadAndParsePDF();
			if (dispostionList.Any())
			{
				var refCusCodeLists = PopulateRefCusCodeList(dispostionList);
				if (string.IsNullOrEmpty(LogBuilder.ToString()))
				{
					var xmlWriterConfiguration = XmlWriterHelper.GetRefCusCodeListWriterConfiguration(CodeType.AESCD, isKeyColumnForAttributeValue: false);
					XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, System.IO.Path.Combine(exportFilePath, OutPutFileName), xmlWriterConfiguration, publishDate, refCusCodeLists);
					LogBuilder.AppendLine(CultureInfo.InvariantCulture, $"Processed {refCusCodeLists.Count} AES Dispostion Codes.");
				}
				else
				{
					LogBuilder.AppendLine("AES Dispostio Codes parsing failed");
					
				}
			}
			else
			{
				LogBuilder.AppendLine("No AES Dispostion code data defined in the file.");
			}
			return LogBuilder.ToString();
		}

		List<AESDispostionCode> ReadAndParsePDF()
		{
			List<AESDispostionCode> dispostionList = new List<AESDispostionCode>();
			AESDispostionCode dispostionCode = null;
			var isMultipleLines = false;

			using (var reader = new PdfReader(filePath))
			using (var pdfDocument = new PdfDocument(reader))
			{
				for (var pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
				{
					var strategy = new SimpleTextExtractionStrategy();
					var currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum), strategy);
					var lines = currentText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(6);

					foreach(var line in lines)
					{
						var text = line.TrimStart();
						if (text.StartsWith(AESDispostionCodeMatchingText.ResponseCode, StringComparison.Ordinal))
						{
							dispostionCode = new AESDispostionCode()
							{
								ResponseCode = text.Replace(AESDispostionCodeMatchingText.ResponseCode, "").Trim()
							};
							dispostionList.Add(dispostionCode);
							isMultipleLines = false;
						}
						else if (text.StartsWith(AESDispostionCodeMatchingText.NarrativeText, StringComparison.Ordinal))
						{
							dispostionCode.NarrativeText = text.Replace(AESDispostionCodeMatchingText.NarrativeText, "").TrimStart();
							isMultipleLines = true;
						}
						else if (text.StartsWith(AESDispostionCodeMatchingText.Severity, StringComparison.Ordinal))
						{
							dispostionCode.Severity = text.Replace(AESDispostionCodeMatchingText.Severity, "").Trim();
							isMultipleLines = false;
						}
						else if (text.StartsWith(AESDispostionCodeMatchingText.Reason, StringComparison.Ordinal)
							|| text.StartsWith(AESDispostionCodeMatchingText.Reasons, StringComparison.Ordinal))
						{
							dispostionCode.Reason = text.Replace(AESDispostionCodeMatchingText.Reason, "").Replace(AESDispostionCodeMatchingText.Reasons, "").TrimStart();
							isMultipleLines = true;
						}
						else if (text.StartsWith(AESDispostionCodeMatchingText.Resolution, StringComparison.Ordinal))
						{
							dispostionCode.Resolution = text.Replace(AESDispostionCodeMatchingText.Resolution, "").TrimStart();
							isMultipleLines = true;
						}
						else if (string.IsNullOrEmpty(text))
						{
							isMultipleLines = false;
						}
						else if (isMultipleLines)
						{
							if (dispostionCode.Resolution != null)
							{
								dispostionCode.Resolution = text;
							}
							else if (dispostionCode.Reason != null)
							{
								dispostionCode.Reason = text;
							}
							else if (dispostionCode.NarrativeText != null && dispostionCode.Severity == null)
							{
								dispostionCode.NarrativeText = text;
							}
						}
					}
				}
				return dispostionList;
			}
		}

		List<RefCusCodeList> PopulateRefCusCodeList(List<AESDispostionCode> dispostionCodes)
		{
			List<RefCusCodeList> refCusCodeLists = new List<RefCusCodeList>();
			foreach(var dispostionCode in dispostionCodes)
			{
				var code = dispostionCode.ResponseCode;
				if (!string.IsNullOrEmpty(code))
				{
					if (code.Length > 3)
					{
						if (code.ToUpper(CultureInfo.InvariantCulture).Contains("NOT CURRENTLY ACTIVE"))
						{
							code = code.Substring(0, 3);
						}
						else
						{
							if (dispostionCode.NarrativeText != null || dispostionCode.Severity != null || dispostionCode.Reason != null || dispostionCode.Resolution != null)
							{
								LogBuilder.AppendLine(CultureInfo.InvariantCulture, $"ResponseCode format is incorrect, ResponseCode: {code}");
							}
							continue;
						}
					}

					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = Regex.Replace(dispostionCode.NarrativeText ?? string.Empty, @"\s+", " ").Trim()
					};

					var attributelList = new List<RefCusCodeListAttribute>();

					var severity = GetSeverityCode(dispostionCode.Severity);
					if (!string.IsNullOrEmpty(severity))
					{
						attributelList.Add(CreateNewAttribute(AttributeNames.AESSeverity, severity));
					}
					else
					{
						LogBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to parse severity, ResponseCode: {code}, Severity: {dispostionCode.Severity}");
					}

					if (!string.IsNullOrEmpty(dispostionCode.Reason))
					{
						attributelList.Add(CreateNewAttribute(AttributeNames.AESReason, dispostionCode.Reason));
					}
					if (!string.IsNullOrEmpty(dispostionCode.Resolution))
					{
						attributelList.Add(CreateNewAttribute(AttributeNames.AESResolution, dispostionCode.Resolution));
					}
					refCusCodeList.RefCusCodeListAttributes = attributelList.ToArray();
					refCusCodeLists.Add(refCusCodeList);
				}
			}
			return refCusCodeLists;
		}

		static string GetSeverityCode(string severity)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(severity))
			{
				severity = severity.Trim().ToUpper(CultureInfo.InvariantCulture);
				severity = Regex.Replace(severity, "[^A-Z]", "");


				if (severity.StartsWith("FATAL", StringComparison.Ordinal))
				{
					result = SeverityCodes.F;
				}
				else if (severity.StartsWith("INFORMATIONAL", StringComparison.Ordinal) || severity.StartsWith("INACTIVE", StringComparison.Ordinal))
				{
					result = SeverityCodes.I;
				}
				else if (severity.StartsWith("VERIFY", StringComparison.Ordinal) || severity.StartsWith("VERIFICATION", StringComparison.Ordinal))
				{
					result = SeverityCodes.V;
				}
				else if (severity.StartsWith("COMPIANCE", StringComparison.Ordinal) || severity.StartsWith("COMPLIANCE", StringComparison.Ordinal))
				{
					result = SeverityCodes.C;
				}
				else if (severity.StartsWith("WARNING", StringComparison.Ordinal))
				{
					result = SeverityCodes.W;
				}
			}
			return result;
		}

		static RefCusCodeListAttribute CreateNewAttribute(string attributeName, string attributeValue)
		{
			var value = Regex.Replace(attributeValue, @"\s+", " ").Trim();
			return new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = attributeName,
				ZZE_Value = value.Length > 255 ? value.Substring(0,252) + "..." : value
			};
		}
	}
}
