using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Globalization;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSMiscData
{
	public class DataReader
	{
		public DataReader(string filePath, string exportFilePath)
		{
			this.filePath = filePath;
			this.exportFilePath = exportFilePath;
			RefCusCodeLists = new List<RefCusCodeList>();
		}

		public bool ReadPDFAndExportXML()
		{
			var matche = string.Empty;

			using (var pdfReader = new PdfReader(filePath))
			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				for (var i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
				{
					var page = pdfDocument.GetPage(i);
					var currentText = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
					currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
					if (i == 1)
					{
						var pattern = new Regex("[A-Z][a-z]+ [0-9]+(th|st|nd|rd)*, [0-9]{4}");
						matche = pattern.Matches(currentText)[0].Value.Trim();
					}
					ExtractData(currentText);
				}
			}

			DateTime publicationTime = GetPublicationTime(matche);
			if (RefCusCodeLists.Any())
			{
				var records = new List<RefDataRepoModelEntityType>();
				var xmlWriter = new XmlWriter(XmlWriterHelper.GetRefCusCodelistConfiguration());
				xmlWriter.SetPublicationTime(publicationTime);
				xmlWriter.SetUpdateType(UpdateType.Full);
				xmlWriter.SetDataSource(Constants.DataSource.CFIAAIRSMiscellaneousCodes);
				records.AddRange(RefCusCodeLists);

				records.ForEach(x => xmlWriter.PopulateData(x));
				xmlWriter.SaveXml(exportFilePath);
			}
			return true;
		}

		void ExtractData(string text)
		{
			var pattern = new Regex("\n[0-9]+ ");
			var matches = pattern.Matches(text);
			var currentText = text;

			for (var i = matches.Count - 1; i >= 0; i--)
			{
				var match = matches[i];
				currentText = CreateRefCusCodeList(match, currentText);
			}

			if (currentText.Length > 0)
			{
				var first = Regex.Match(currentText, "[0-9]+ ");
				if (first != null && first.Length > 0 && first.Index == 0)
				{
					CreateRefCusCodeList(first, currentText);
				}
			}
		}

		string CreateRefCusCodeList(Match match, string text)
		{
			var leftText = text.Substring(0, match.Index);
			var codeString = match.Value.Trim();
			var description = text.Substring(match.Index + match.Length, text.Length - match.Index - match.Length).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
			var code = new RefCusCodeList();
			code.ZZD_Code = codeString;
			code.ZZD_Description = description;
			RefCusCodeLists.Add(code);
			return leftText;
		}

		static DateTime GetPublicationTime(string dateText)
		{
			var pattern1 = new Regex("[A-Z][a-z]+");
			var pattern2 = new Regex("[0-9]+");
			var pattern3 = new Regex("[0-9]{4}");
			var month = pattern1.Match(dateText).Value.Trim();
			var day = pattern2.Match(dateText).Value.Trim();
			var year = pattern3.Match(dateText).Value.Trim();
			var date = year + "-" + month + "-" + day + " 00:00:00";
			return Convert.ToDateTime(date, CultureInfo.InvariantCulture);
		}

		private string exportFilePath;
		private List<RefCusCodeList> RefCusCodeLists;
		readonly string filePath;
	}
}
