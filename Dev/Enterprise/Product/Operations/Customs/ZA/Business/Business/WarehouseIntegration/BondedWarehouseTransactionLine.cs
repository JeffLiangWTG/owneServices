using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class BondedWarehouseTransactionLine : Customs.Business.BondedWarehouseTransactionLine
	{
		public BondedWarehouseTransactionLine(CusEntryLine line)
			: base(line)
		{
		}

		public BondedWarehouseTransactionLine(JobComInvoiceLine line)
			: base(line)
		{
		}

		public new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override ZString AddInfoString => InvoiceLine.GetAddInfoString();

		protected override ZShort GetEntryLineNumberFromInvoiceLine() => InvoiceLine.JI_PreviousEntryLineNumber;

		protected override ZString EntryKeyCore => new EntryKeyGenerator(EntryLine.EntryNumber, EntryLine.Header.CustomsOffice, EntryLine.Header.ClearanceDate).EntryKey;

		protected override OrgAddress GetWarehouseCore() => null;
	}
}
