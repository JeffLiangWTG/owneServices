using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommodityInvoiceLineTests : CommodityInvoiceLineAbstractTests<CommodityInvoiceLine>
	{
		protected override IInvoiceLine CommodityInvoiceLine => new CommodityInvoiceLine(EntryLine);
	}
}
