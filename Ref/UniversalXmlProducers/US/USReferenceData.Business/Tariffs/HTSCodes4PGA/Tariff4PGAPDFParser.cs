using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class Tariff4PGAPDFParser
	{
		readonly string filePath;
		const string tariffCodesMultiplePattern = @"\d{4}(?:\.\d{2}(?:\.\d{4})?)?";
		const string tariffCodesFirstPattern = @"^\d{4}(?:\.\d{2}(?:\.\d{4})?)?";
		List<Tariff4PGA> tariff4PGAList;
		StringBuilder logMessage;

		public Tariff4PGAPDFParser(List<Tariff4PGA> tariff4PGAList, string filePath)
		{
			this.filePath = filePath;
			this.tariff4PGAList = tariff4PGAList;
			logMessage = new StringBuilder();
		}

		public string Parse()
		{
			ReadAndParsePDF();
			return logMessage.ToString();
		}

		void ReadAndParsePDF()
		{
			using (var reader = new PdfReader(filePath))
			using (var pdfDocument = new PdfDocument(reader))
			{
				var totalPage = pdfDocument.GetNumberOfPages();
				var pgaCodesInfos = GetPgaCodesInfos();
				PgaCodesInfo currentPgaCodesInfo = new PgaCodesInfo();
				var mandatory = true;
				for (var pageNum = 2; pageNum <= totalPage; pageNum++)
				{
					var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum));
					var tempPgaCodesInfo = pgaCodesInfos.FirstOrDefault(f => pageText.Contains(f.Title));
					if (!tempPgaCodesInfo.IsEmpty)
					{
						if (currentPgaCodesInfo.PGACode != tempPgaCodesInfo.PGACode)
						{
							mandatory = true;
						}
						currentPgaCodesInfo = tempPgaCodesInfo;
					}
					if (currentPgaCodesInfo.IsEmpty || !currentPgaCodesInfo.NeedImport)
					{
						logMessage.AppendLine(CultureInfo.InvariantCulture, $"Page {pageNum}/{totalPage} PGA = {currentPgaCodesInfo.PGACode} ignored.");
						continue;
					}
					var pageTextLines = pageText.Split('\n');
					for (int lineIndex = 0; lineIndex < pageTextLines.Length; lineIndex++)
					{
						var lineText = pageTextLines[lineIndex];
						if (mandatory)
						{
							mandatory = !currentPgaCodesInfo.Switch2Optional(lineText);
						}
						var tariffCodes = currentPgaCodesInfo.ParseTariffCodes(lineText);
						if (tariffCodes != null)
						{
							tariff4PGAList.AddRange(tariffCodes.Select(s => new Tariff4PGA(s, mandatory, currentPgaCodesInfo.PGACode)));
						}
					}
				}
			}
		}

		Func<string, bool> neverSwitch2Optional = lineText => false;

		Func<string, string[]> parseMultipleCodesInOneLine = lineText =>
		{
			lineText = lineText.TrimStart();
			string[] result = null;
			var matches = Regex.Matches(lineText, tariffCodesMultiplePattern);
			if (matches.Count > 0 && matches[0].Index == 0)
			{
				result = matches.Select(s => s.Value.Trim()).ToArray();
			}
			return result;
		};

		Func<string, string[]> parseCodesInTableLine = lineText =>
		{
			lineText = lineText.TrimStart();
			var match = Regex.Match(lineText, tariffCodesFirstPattern);
			return match.Success ? new string[] { match.Value } : null;
		};

		struct PgaCodesInfo
		{
			public string PGACode { get; }
			public string Title { get; }
			public bool NeedImport { get; }
			public Func<string, bool> Switch2Optional { get; }
			public Func<string, string[]> ParseTariffCodes { get; }
			public bool IsEmpty { get => string.IsNullOrEmpty(PGACode); }

			public PgaCodesInfo(string pGACode, string title) : this()
			{
				PGACode = pGACode;
				Title = title;
			}

			public PgaCodesInfo(string pGACode, string title, Func<string, bool> switch2Optional, Func<string, string[]> parseTariffCodes) : this(pGACode, title)
			{
				NeedImport = true;
				Switch2Optional = switch2Optional;
				ParseTariffCodes = parseTariffCodes;
			}
		}

		List<PgaCodesInfo> GetPgaCodesInfos()
		{
			return new List<PgaCodesInfo>()
			{
				new PgaCodesInfo(Constants.PGACodes.AMS, "AMS – Agricultural Marketing Service", neverSwitch2Optional, parseMultipleCodesInOneLine),
				new PgaCodesInfo(Constants.PGACodes.ATF, "ATF – Alcohol Tobacco and Firearms and Explosives"),
				new PgaCodesInfo(Constants.PGACodes.DEA, "DEA – Drug Enforcement Administration"),
				new PgaCodesInfo(Constants.PGACodes.EPA, "EPA – Environmental Protection Agency", lineText => lineText.Contains("Allowed for EPA", StringComparison.OrdinalIgnoreCase), parseMultipleCodesInOneLine),
				new PgaCodesInfo(Constants.PGACodes.FWS, "FWS – Fish & Wildlife Service"),
				new PgaCodesInfo(Constants.PGACodes.NMFS, "NMFS - National Oceanic and Atmospheric Administration", neverSwitch2Optional, parseCodesInTableLine),
				new PgaCodesInfo(Constants.PGACodes.TTB, "TTB – Alcohol and Tobacco Tax and Trade Bureau", neverSwitch2Optional, parseCodesInTableLine),
			};
		}
	}
}
