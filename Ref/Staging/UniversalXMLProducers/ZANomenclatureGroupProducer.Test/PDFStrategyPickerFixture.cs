using iText.Kernel.Pdf;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.Test
{
	[TestFixture]
	internal class PDFStrategyPickerFixture
	{
		[Test]
		public void VersionPicker()
		{
			var pdfVersion = PDFStrategyPicker.GetByPdfVersion(PdfVersion.PDF_1_7);
			Assert.IsNotNull(pdfVersion);
			Assert.That(pdfVersion, Is.TypeOf<PDFVersion7LocationTextExtractionStrategyZA>());

			Assert.That(() => PDFStrategyPicker.GetByPdfVersion(PdfVersion.PDF_1_5), Throws.Exception.With.Message.EqualTo("PDF Version: /1.5 not supported"));
		}
	}
}
