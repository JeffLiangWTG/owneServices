using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusEntryLine
	{
		public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

		public new JobDeclaration Declaration => base.Declaration as JobDeclaration;

		public new CusEntryLineLookups Lookups => base.Lookups as CusEntryLineLookups;
	}
}
