using System;
using iText.Kernel.Pdf;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public static class PDFStrategyPicker
	{
		public static PDFLocationTextExtraction GetByPdfVersion(PdfVersion pdfVersion)
		{
			if (pdfVersion != PdfVersion.PDF_1_7)
			{
				throw new NotSupportedException($"PDF Version: {pdfVersion.ToPdfName()} not supported");
			}
			return new PDFVersion7LocationTextExtractionStrategyZA();
		}
	}
}
