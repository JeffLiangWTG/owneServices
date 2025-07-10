using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class SupportingDocumentsProvider : IDocuments
	{
		public SupportingDocumentsProvider(SupportingDocument document, ZInt lineNumber)
		{
			SupportingDocument = Argument.NotNull(document, nameof(document));
			LineNumber = Argument.NotNull(lineNumber, nameof(lineNumber));
		}
		SupportingDocument SupportingDocument { get; }
		ZInt LineNumber { get; }

		public int LineNo => LineNumber;
		public string Code => SupportingDocument.CSI_Code;
		public string Verification => SupportingDocument.CSI_Status;
		public string DocumentDate => SupportingDocument.CSI_DateOfIssue.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear);
		public string Reference => SupportingDocument.CSI_ReferenceNumber;
		public string VisaDate => SupportingDocument.CSI_DateOfExpiry.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear);
	}
}
