using System;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode
{
	public class AESDispostionCodeParser : CBPPDFParser
	{
		public AESDispostionCodeParser(string rootUrl, string detialUrl, string outputPath, IDownLoadService downLoadService)
			: base(rootUrl, detialUrl, outputPath, downLoadService)
		{
		}

		protected override string DataSource => "US AES Dispostion Code";

		protected override bool IsPDFFileUrl(string href, string innerText)
		{
			return !string.IsNullOrEmpty(href) && (href.Contains(@"ACE%20APPENDIX%20A%20-%20COMMODITY%20FILING%20RESPONSE%20MESSAGES", StringComparison.OrdinalIgnoreCase) || innerText.Contains("COMMODITY FILING RESPONSE MESSAGES", StringComparison.OrdinalIgnoreCase)) && href.EndsWith(@"PDF", StringComparison.OrdinalIgnoreCase);
		}

		protected override string ParseToXml(string pdfPath, string outputPath, DateTime publishDate)
		{
			var parser = new PDFParser(pdfPath, outputPath, publishDate);
			var result = parser.ReadPDFAndExportXML();
			return result;
		}
	}
}
