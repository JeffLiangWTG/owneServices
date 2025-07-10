using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	class ReconOrigEntryLineWrap : IReconOriginalEntryLine
	{
		public ReconOrigEntryLineWrap(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		#region IReconOriginalEntryLine  Members
		public ZString EntryFilerCode => invoiceLine.JI_Calc_Invoice.Left(3);
		public ZString EntryNumber => invoiceLine.JI_Calc_Invoice.Length > 3 ? invoiceLine.JI_Calc_Invoice.Substring(3) : ZString.Empty;
		public ZString EntryLineNumber => invoiceLine.US_R_OrigEntryLineNo.ToString();

		#endregion
	}
}
