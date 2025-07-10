using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101InvoiceLine : IInvoiceLine
	{
		public NX101InvoiceLine(ZString currencyTypeCode, ZDecimal itemChargeAmount)
		{
			this.currencyTypeCode = currencyTypeCode;
			this.itemChargeAmount = itemChargeAmount;
		}

		readonly ZString currencyTypeCode;

		readonly ZDecimal itemChargeAmount;

		ZString IInvoiceLine.ChargesTypeCode => ZString.Empty;

		ZString IInvoiceLine.CurrencyTypeCode => currencyTypeCode;

		ZDecimal IInvoiceLine.UnitPriceAmount => ZDecimal.Zero;

		ZDecimal IInvoiceLine.ItemChargeAmount => itemChargeAmount;

		ZDecimal IInvoiceLine.SubTotalAmount => ZDecimal.Zero;
	}
}
