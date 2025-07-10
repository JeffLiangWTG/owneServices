//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccDraftInvoiceExRateValidation
//
//    This class should be used for overriding validation in AutoAccDraftInvoiceExRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class AccDraftInvoiceExRateValidation : AutoAccDraftInvoiceExRateValidation
	{
		public AccDraftInvoiceExRateValidation(AutoAccDraftInvoiceExRate parent) : base(parent)
		{
		}

		protected override void CheckAIE_ExchangeRate()
		{
			base.CheckAIE_ExchangeRate();

			if (Parent.AIE_ExchangeRate <= ZDecimal.Zero)
			{
				Parent.AIE_ExchangeRateInfo.AddError(Res.GetString("FFC4D392-2471-4B05-9C11-1D19B81A9545", "Exchange Rate must be greater than zero."));
			}
		}

		protected override void CheckAIE_RX_NKRateCurrency()
		{
			base.CheckAIE_RX_NKRateCurrency();

			MandatoryValidation.CheckEntered(Parent.AIE_RX_NKRateCurrencyInfo);

			var localCurrency = Parent.Header?.Company?.GC_RX_NKLocalCurrency ?? ZString.Empty;
			if (!localCurrency.IsEmpty && Parent.AIE_RX_NKRateCurrency == localCurrency)
			{
				Parent.AIE_RX_NKRateCurrencyInfo.AddError(Res.GetString("FFE06D86-968F-40B4-9CD6-B099FC5CACE8", "Please select a foreign currency from the list."));
			}

			if (Parent.Header?.ExchangeRates.Cast<AccDraftInvoiceExRate>().Any(c => c.PK != Parent.PK && c.AIE_RX_NKRateCurrency == Parent.AIE_RX_NKRateCurrency) ?? false)
			{
				Parent.AIE_RX_NKRateCurrencyInfo.AddError(Res.GetString("317529E3-92F2-4E85-9B20-DA1647A451C3", "At least one more record already sets exchange rate for the same currency."));
			}
		}
	}
}
