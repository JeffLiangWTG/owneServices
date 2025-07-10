using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class CommodityInvoiceLine : IInvoiceLine
	{
		public CommodityInvoiceLine(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		public ZString ChargesTypeCode => ZString.Empty;

		public ZString CurrencyTypeCode => ZString.Empty;

		public virtual ZDecimal UnitPriceAmount => entryLine.CL_EntryLineUnitPrice;

		public virtual ZDecimal ItemChargeAmount => entryLine.CL_CustomsValue;

		public ZDecimal SubTotalAmount => ZDecimal.Zero;

		readonly CusEntryLine entryLine;
	}
}
