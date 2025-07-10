using System;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business.DISCodes
{
	public class DISCodeParser : CBPPDFParser
	{
		public DISCodeParser(string rootUrl, string detialUrl, string outputPath, IDownLoadService downLoadService)
			: base(rootUrl, detialUrl, outputPath, downLoadService)
		{
		}

		protected override string DataSource => "US DIS Code";

		protected override bool IsPDFFileUrl(string href, string innerText)
		{
			return !string.IsNullOrEmpty(href) && (href.Contains(@"ACE%20DIS%20XML%20IMPLEMENTATION%20GUIDE", StringComparison.OrdinalIgnoreCase) || innerText.Contains("ACE DIS XML IMPLEMENTATION GUIDE", StringComparison.OrdinalIgnoreCase)) && href.EndsWith(@"PDF", StringComparison.OrdinalIgnoreCase);
		}

		protected override string ParseToXml(string pdfPath, string outputPath, DateTime publishDate)
		{
			var parser = new PDFParser(pdfPath, outputPath, publishDate);
			var result = parser.ReadPDFAndExportXML();
			return result;
		}
	}
}
