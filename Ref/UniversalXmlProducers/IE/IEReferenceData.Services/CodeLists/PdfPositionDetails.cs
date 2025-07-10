namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	class PdfPositionDetails
	{
		public PdfPositionDetails(IRevenueCodeListDetails codeListDetails, int pageNoInPdf, string codeListIndexNumberInPdf, string nextCodeListIndexNumberInPdf)
		{
			CodeListDetails = codeListDetails;
			PageNoInPdf = pageNoInPdf;
			CodeListIndexNumberInPdf = codeListIndexNumberInPdf;

			// if length is too long it will include a line break and we won't find a match
			if (nextCodeListIndexNumberInPdf != null && nextCodeListIndexNumberInPdf.Length > 50)
			{
				nextCodeListIndexNumberInPdf = nextCodeListIndexNumberInPdf.Substring(0, 50);
			}
			NextCodeListIndexNumberInPdf = nextCodeListIndexNumberInPdf;
		}

		public IRevenueCodeListDetails CodeListDetails { get; }

		public int PageNoInPdf { get; }

		public string CodeListIndexNumberInPdf { get; }

		public string NextCodeListIndexNumberInPdf { get; }
	}
}
