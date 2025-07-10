using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public sealed class ItalyExchangeRateDataFetcher : IExchangeRateDataFetcher
	{
		readonly ExchangeRateMetaData _metaData;

		public ItalyExchangeRateDataFetcher(ExchangeRateMetaData metaData)
		{
			_metaData = metaData;
		}

		ExchangeRateData IExchangeRateDataFetcher.Fetch()
		{
			using (var pdfReader = new PdfReader(_metaData.DataLocation))
			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				var pages = new List<string>();

				for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
				{
					var page = pdfDocument.GetPage(i);
					string currentText = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
					pages.Add(currentText);
				}

				return new ExchangeRateData(pages, null);
			}
		}
	}
}
