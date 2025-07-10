using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ImportNonCondensedDeclarationCommodity : NX5105GovernmentAgencyGoodsItem_Commodity
	{
		public ImportNonCondensedDeclarationCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine) : base(entryLine, invoiceLine)
		{
		}

		public override ZString Description => InvoiceLine?.JI_DeclarationGoodsDescription ?? ZString.Empty;

		protected override IInvoiceLine GetInvoiceLine()
		{
			return new ImportNonCondensedDeclarationInvoiceLine(EntryLine, InvoiceLine);
		}

		protected override ICommodityDutyTaxFee GetDutyTaxFeeCore()
		{
			return new ImportNonCondensedDeclarationDutyTaxFee(EntryLine, InvoiceLine);
		}
	}
}
