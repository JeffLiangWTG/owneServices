using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public class PDFParser
	{
		public DateTime PublicationTime { get; private set; }

		public List<RefCusCodeList> RefCusCodeLists { get; private set; }

		readonly string _localPdfFileSavePath;

		public PDFParser(string localPdfFileSavePath)
		{
			Argument.NotNullOrEmpty(localPdfFileSavePath, nameof(localPdfFileSavePath));
			_localPdfFileSavePath = localPdfFileSavePath;
		}

		public void Parse(DateTime publicationTime)
		{
			var pages = FetchPages();

			PublicationTime = publicationTime;
			RefCusCodeLists = new List<RefCusCodeList>();

			foreach (var page in pages)
			{
				ExtractOnePage(page);
			}
		}

		IList<string> FetchPages()
		{
			using (var pdfReader = new PdfReader(_localPdfFileSavePath))
			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				var pages = new List<string>();

				for (int i = 0; i < pdfDocument.GetNumberOfPages(); i++)
				{
					var currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i + 1), new SimpleTextExtractionStrategy());
					pages.Add(currentText);
				}
				return pages;
			}
		}

		static DateTime ParseDateTime(string page)
		{
			Argument.NotNull(page, nameof(page));
			var dateTime = DateTime.UtcNow;

			var pattern = @"AESTIR.+?([A-Z].+?\d{1,},\s+\d{4})";
			var match = Regex.Match(page, pattern);
			if (match.Groups.Count == 2)
			{
				var dateTimeString = match.Groups[1].Value;
				dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
			}

			return dateTime;
		}

		void ExtractOnePage(string page)
		{
			Argument.NotNull(page, nameof(page));

			var pattern = @"Code CBP Port Location Vessel Air Rail Road Fixed \n(.+)";
			var matches = Regex.Matches(page, pattern, RegexOptions.Singleline);
			if (matches.Count > 0)
			{
				var tableContent = matches[0].Groups[1].Value;
				var previousPageLeftOverMatch = Regex.Match(tableContent, @"^([^0-9]+)\s\n\d{4}");
				if (previousPageLeftOverMatch.Groups.Count == 2)
				{
					var lastPortFromPreviousPage = RefCusCodeLists.LastOrDefault();
					if (lastPortFromPreviousPage != null)
					{
						lastPortFromPreviousPage.ZZD_Description += previousPageLeftOverMatch.Groups[1].Value;
					}
				}

				var lines = Regex.Matches(tableContent, @"((\d{4})(.+?)([YN]\s){5}(?:$|\n))", RegexOptions.Singleline);
				foreach (Match line in lines)
				{
					var refCusCodeList = new RefCusCodeList();

					var portCode = line.Groups[2].Value;
					var portLocation = line.Groups[3].Value.Trim();
					portLocation = Regex.Replace(portLocation, @"\n|\r", "");
					var vessel = line.Groups[4].Captures[0].Value.Trim();
					var air = line.Groups[4].Captures[1].Value.Trim();
					var rail = line.Groups[4].Captures[2].Value.Trim();
					var road = line.Groups[4].Captures[3].Value.Trim();
					var fixedFlag = line.Groups[4].Captures[4].Value.Trim();

					refCusCodeList.ZZD_Code = portCode;
					refCusCodeList.ZZD_Description = portLocation;
					refCusCodeList.ZZD_StartDate = PublicationTime;
					var modes = new List<string>();
					if (vessel == "Y")
					{ modes.Add("SEA"); }
					if (air == "Y")
					{ modes.Add("AIR"); }
					if (rail == "Y")
					{ modes.Add("RAI"); }
					if (road == "Y")
					{ modes.Add("ROA"); }
					if (fixedFlag == "Y")
					{ modes.Add("FIX"); }

					var transportModes = new List<RefCusCodeOrAttributeTransportMode>();
					foreach (var mode in modes)
					{
						transportModes.Add(new RefCusCodeOrAttributeTransportMode { ZZU_TransportMode = mode });
					}

					if (transportModes.Any())
					{
						refCusCodeList.RefCusCodeOrAttributeTransportModes = transportModes.ToArray();
					}

					refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { new RefCusCodeListAttribute() };

					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}
	}
}
