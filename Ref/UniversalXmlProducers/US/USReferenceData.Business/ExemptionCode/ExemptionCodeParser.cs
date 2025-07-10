using System;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExemptionCode
{
	public class ExemptionCodeParser : CBPPDFParser
	{
		public ExemptionCodeParser(string rootUrl, string detialUrl, string outputPath, IDownLoadService downLoadService)
			: base(rootUrl, detialUrl, outputPath, downLoadService)
		{
		}

		protected override string DataSource => "US DDTC ITAR Exemption Codes";

		protected override bool IsPDFFileUrl(string href, string innerText)
		{
			return !string.IsNullOrEmpty(href) && (href.Contains(@"ACE%20APPENDIX%20O%20%E2%80%93%20DDTC%20ITAR%20EXEMPTION%20CODES", StringComparison.OrdinalIgnoreCase) || innerText.Contains("DDTC ITAR EXEMPTION CODES", StringComparison.OrdinalIgnoreCase)) && href.EndsWith(@"PDF", StringComparison.OrdinalIgnoreCase);
		}

		protected override string ParseToXml(string pdfPath, string outputPath, DateTime publishDate)
		{
			var parser = new PDFParser(pdfPath, outputPath, publishDate);
			var result = parser.ReadPDFAndExportXML();
			return result;
		}
	}
}
