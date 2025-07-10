using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CurrencyUpdate)]
	[TopLevel(typeof(CURPercent1), typeof(CURPercent2))]
	class CurrencyProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			CURPercent1 lastPercent1 = null;
			foreach (MessageBlock block in messageBlocks)
			{
				CURPercent1 percent1 = block as CURPercent1;
				if (percent1 != null)
				{
					lastPercent1 = percent1;
					PostRate(percent1.ISOCurrencyCode, percent1.ExchangeRateDate1, percent1.Indicator1, percent1.ExchangeRate1);
				}
				//TODO: Sent email requesting info from Phyllis (BS 24/05/07)
				//else
				//{
				//  CURPercent2 percent2 = block as CURPercent2;
				//  PostRate(lastPercent1.ISOCurrencyCode, percent2.ExchangeRateDate, percent2.Indicator2, percent2.ExchangeRate2);
				//  PostRate(lastPercent1.ISOCurrencyCode, percent2.ExchangeRateDate, percent2.Indicator3, percent2.ExchangeRate3);
				//}
			}
		}

		void PostRate(ZString currencyCode, ZDate exchangeRateDate, ZString indicator, ZDecimal exchangeRate)
		{
			if (indicator == "Q" || indicator == "D")
			{
				RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
				if (currency != null)
				{
					ZDateTime endingDate = exchangeRateDate;
					currency.SetCustomsRate(exchangeRateDate, endingDate, exchangeRate);
				}
			}
		}
	}
}
