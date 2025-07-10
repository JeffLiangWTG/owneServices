using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Spire.Pdf;
using Spire.Pdf.Texts;
using Spire.Pdf.Utilities;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public static partial class NCExchangeRatePDFParser
{
	public static ExchangeRates ParsePDF(string downloadAbsolutePath)
	{
		using var pdfDocument = new PdfDocument(downloadAbsolutePath);
		pdfDocument.LoadFromFile(downloadAbsolutePath);
		var (startDate, endDate) = ExtractDate(pdfDocument);
		if (startDate == DateTime.MinValue)
		{
			return new ExchangeRates(startDate, endDate, []);
		}

		var exchangeRates = ExtractExchangeRates(pdfDocument);
		return new ExchangeRates(startDate, endDate, exchangeRates);
	}

	static (DateTime StartDate, DateTime EndDate) ExtractDate(PdfDocument document)
	{
		var page = document.Pages[0];
		var textExtractor = new PdfTextExtractor(page);
		var extractOptions = new PdfTextExtractOptions
		{
			IsExtractAllText = true
		};

		var text = textExtractor.ExtractText(extractOptions);
		string dateLine;
		using (StringReader reader = new StringReader(text))
		{
			while ((dateLine = reader.ReadLine()) != null)
			{

				if (dateLine.Contains(DateLineIdentifier))
				{
					break;
				}
			}
		}

		if (dateLine == null)
		{
			Console.Error.WriteLine("No date was found in the document.");
			return (DateTime.MinValue, DateTime.MinValue);
		}

		dateLine = dateLine.Replace(DateLineIdentifier, "").Trim();
		dateLine = DateConvertor().Replace(dateLine, "$1");
		if (!DateTime.TryParse(dateLine, cultureInfo, out DateTime startDate))
		{
			var errroMessage = $"Unexpected date fomart of {dateLine}";
			Console.Error.WriteLine(errroMessage);
			return (DateTime.MinValue, DateTime.MinValue);
		}

		int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
		var endDate = new DateTime(startDate.Year, startDate.Month, daysInMonth);
		return (startDate, endDate);
	}

	static List<ExchangeRate> ExtractExchangeRates(PdfDocument document)
	{
		using (var extractor = new PdfTableExtractor(document))
		{
			var table = extractor.ExtractTable(0)?[1];
			var rowCount = table?.GetRowCount() ?? 0;

			if (table == null || rowCount <= 1)
			{
				Console.Error.WriteLine($"Unexpected format of PDF.");
				return [];
			}

			var exchangeRates = new List<ExchangeRate>();
			for (var rowIndex = 1; rowIndex < rowCount; rowIndex++)
			{
				var code = table.GetText(rowIndex, 0).Trim();
				if (string.IsNullOrWhiteSpace(code))
				{
					continue;
				}

				if (code.Length != 3)
				{
					var errroMessage = $"Find code {code} with unexpected length.";
					Console.Error.WriteLine(errroMessage);
					return [];
				}

				var rateText = table.GetText(rowIndex, 2).Trim();
				rateText = CleanRateText().Replace(rateText, "");

				if (!decimal.TryParse(rateText, cultureInfo, out decimal rate))
				{
					var errroMessage = $"Find rate {rate} with unexpected format.";
					Console.Error.WriteLine(errroMessage);
					return [];
				}
				exchangeRates.Add(new ExchangeRate(code, rate));
			}
			return exchangeRates;
		}
	}

	const string DateLineIdentifier = "applicables au";
	readonly static CultureInfo cultureInfo = new("fr-FR");

	[GeneratedRegex(@"(\d+)(er|ème|ère)?\b")]
	private static partial Regex DateConvertor();

	[GeneratedRegex(@"[^\d,]")]
	private static partial Regex CleanRateText();
}
