using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Spire.Pdf;
using Spire.Pdf.Texts;
using System.Linq;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public static partial class NCTariffPDFParser
{
	public static List<Tariff> ParsePDF (string downloadAbsolutePath)
	{
		ArgumentNullException.ThrowIfNull(downloadAbsolutePath, nameof(downloadAbsolutePath));

		using var pdfDocument = new PdfDocument(downloadAbsolutePath);
		pdfDocument.LoadFromFile(downloadAbsolutePath);
		var startDate = ExtractDate(pdfDocument);
		if (startDate == DateTime.MinValue)
		{
			return [];
		}

		var tableData = ExtractTableData(pdfDocument);

		return ExtractTariffData(tableData, startDate);
	}

	static DateTime ExtractDate(PdfDocument document)
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
			return DateTime.MinValue;
		}

		dateLine = dateLine.Replace(DateLineIdentifier, "").Trim();
		dateLine = DateConvertor().Replace(dateLine, "$1");
		if (!DateTime.TryParse(dateLine, cultureInfo, out DateTime startDate))
		{
			var errroMessage = $"Unexpected date fomart of {dateLine}";
			Console.Error.WriteLine(errroMessage);
			return DateTime.MinValue;
		}

		return startDate;
	}

	static List<List<PdfTextFragment>> ExtractTableData(PdfDocument document)
	{
		var tableData = new List<List<PdfTextFragment>>();

		for(int pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
		{
			using (PdfTextFinder finder = new PdfTextFinder(document.Pages[pageIndex]))
			{
				var fragments = finder.FindAllText();
				var tableHeader = finder.Find("Désignation des marchandises");

				if (fragments == null || tableHeader.Count == 0)
				{
					continue;
				}

				double yTolerance = 2.0;
				var lines = fragments
					.GroupBy(f => fragments
						.Where(f2 => Math.Abs(f2.Bounds[0].Y - f.Bounds[0].Y) < yTolerance)
						.Select(f2 => f2.Bounds[0].Y)
						.FirstOrDefault())
					.OrderBy(g => g.Key)
					.ToList();

				foreach (var line in lines)
				{
					var sortedLine = line.OrderBy(f => f.Bounds[0].X).ToList();
					List<PdfTextFragment> row = sortedLine.Select(f => f).ToList();

					if (row.Count > 1 && (char.IsDigit(row.Select(f => f.Text).First()[0]) || row.Select(f => f.Text).First() == " "))
					{
						tableData.Add(row);
					}
				}
			}
		}

		return tableData;
	}

	static List<Tariff> ExtractTariffData(List<List<PdfTextFragment>> tableData, DateTime startDate)
	{
		var tariffList = new List<Tariff>();

		Stack<(string Text, float X, bool isHeading)> descriptionsStack = new Stack<(string Text, float X, bool isHeading)>();
		List<(string, decimal, bool)> descriptions = new List<(string, decimal, bool)>();

		for (int rowIndex = 0; rowIndex < tableData.Count; rowIndex++)
		{
			var row = tableData[rowIndex];

			string positionSH = row[0].Text;
			string UOM = row[row.Count - 2].Text;
			string code = row[row.Count - 1].Text.Replace(".", "");

			string descriptionText = "";
			for (int index = 1; index < row.Count - 2; index++)
			{
				descriptionText += row[index].Text;
			}

			(string Text, float X, bool isHeading)  description = (descriptionText, row[1].Bounds[0].X, false);

			if(positionSH.Length == 4)
			{
				descriptionsStack.Clear();
				description.isHeading = true;
				descriptionsStack.Push(description);
			}
			else if(code == " ")
			{
				while ((description.X < descriptionsStack.Peek().X || Math.Abs(description.X - descriptionsStack.Peek().X) <= 2.0) && !descriptionsStack.Peek().isHeading)
				{
					descriptionsStack.Pop();
				}
				descriptionsStack.Push(description);
			}
			else if(code.Length == 8)
			{
				while ((description.X < descriptionsStack.Peek().X || Math.Abs(description.X - descriptionsStack.Peek().X) <= 2.0) && !descriptionsStack.Peek().isHeading)
				{
					descriptionsStack.Pop();
				}
				descriptionsStack.Push(description);
				string rowDescription = string.Join(" ", descriptionsStack.Reverse().Select(d => d.Text));

				if(UOMCodeIdentifier().IsMatch(UOM))
				{
					var tariff = new Tariff(code, rowDescription, startDate, UOM);
					tariffList.Add(tariff);
				}
			}
		}

		return tariffList;
	}

	const string DateLineIdentifier = "Version : ";
	readonly static CultureInfo cultureInfo = new("fr-FR");

	[GeneratedRegex(@"^[A-Z]{3}$")]
	private static partial Regex UOMCodeIdentifier();

	[GeneratedRegex(@"(\d+)(er|ème|ère)?\b")]
	private static partial Regex DateConvertor();
}
