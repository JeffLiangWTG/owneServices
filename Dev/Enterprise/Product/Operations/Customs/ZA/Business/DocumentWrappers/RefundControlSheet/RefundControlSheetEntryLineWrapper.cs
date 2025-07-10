using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class RefundControlSheetEntryLineWrapper : DA63LineDetailWrapper
	{
		public RefundControlSheetEntryLineWrapper(CusEntryLine line)
			: base(line)
		{
			Argument.NotNull(line, "Line");
			header = line.Header;
			Argument.NotNull(header, "Header");
		}

		readonly CusEntryHeader header;

		public CusEntryHeader Header => header;

		#region Import Entry Line Data

		public ZString OriginalMRN => PreviousProcedureMRN;
		public ZString ImportLineNo => PreviousMRNLineNumber;
		public ZString DistrictOfficeCode => PreviousProcedureMRN.Left(3);
		public ZString CustomsDuty => DA63CustomsDutyExcluding12B.Round(2).ToString();
		public ZString DutySchedule1Part2B => DA63S1P2BDuty.ToString(2);
		public ZString Vat => DA63ValueAddedTax.Round(2).ToString();
		public ZString Other => TotalOtherAmount.ToString(2);

		#endregion

		#region Export Entry Line Data

		public ZString ExportDeclarationNumber => Header.MovementReferenceNumber;
		public ZString ExportDeclarationDate => Header.MovementReferenceNumberIssueDate.ToShortDateString();
		public ZString ExportLineNo => EntryLine.CL_LineNumber.ToString();

		#endregion
	}
}
