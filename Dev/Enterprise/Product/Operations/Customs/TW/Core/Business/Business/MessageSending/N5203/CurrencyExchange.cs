using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class CurrencyExchange : ICurrencyExchange
	{
		public CurrencyExchange(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		public ZString CurrencyTypeCode => entryHeader.FirstInvoiceCurrencyCode;

		public ZDecimal RateNumeric => entryHeader.FirstInvoiceCurrencyExRate;
	}
}
