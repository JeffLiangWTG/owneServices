using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Declaration_CurrencyExchange : ICurrencyExchange
	{
		readonly CusEntryHeader entryHeader;

		public NX5105Declaration_CurrencyExchange(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
		}

		ZString ICurrencyExchange.CurrencyTypeCode => entryHeader.FirstInvoiceCurrencyCode;

		ZDecimal ICurrencyExchange.RateNumeric => entryHeader.FirstInvoiceCurrencyExRate;
	}
}
