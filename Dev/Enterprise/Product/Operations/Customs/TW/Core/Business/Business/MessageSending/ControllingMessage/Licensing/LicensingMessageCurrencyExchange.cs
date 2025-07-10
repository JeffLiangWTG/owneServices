using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageCurrencyExchange : ICurrencyExchange
	{
		JobDeclaration Declaration { get; }

		public LicensingMessageCurrencyExchange(CusTWControllingMessageHeader header)
		{
			Declaration = header.Declaration;
		}

		public ZString CurrencyTypeCode
		{
			get
			{
				var currencies = Declaration?.Invoices.Select(c => c.JZ_RX_NKInvoice_Currency).Distinct();
				return currencies.Count() > 1 ? new ZString(Core.Constants.CurrencyCodes.Taiwan) : currencies.FirstOrDefault();
			}
		}

		public ZDecimal RateNumeric => Declaration?.Invoices.FirstOrDefault(invoice => !invoice.JZ_InvoiceCurrExRate.IsEmpty)?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;
	}
}
