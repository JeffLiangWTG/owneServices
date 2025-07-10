using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	internal static class TradeLinesValueHelper
	{
		public static TradeLaneKey CreateActualTradeLaneKey(OrgSales sales)
		{
			return new TradeLaneKey(sales);
		}

		public static TradeDetailKey CreateTradeDetailKey(OrgTradeDetail tradeDetail)
		{
			return new TradeDetailKey(tradeDetail);
		}

		public static TradePeriodKey CreateTradePeriodKey(OrgTradePeriod tradePeriod)
		{
			return new TradePeriodKey(tradePeriod.PAS_Period, tradePeriod.PAS_OH_Client, tradePeriod.PAS_IsJobValue);
		}

		public static TradeValueBreakdownKey CreateTradeValueKey(OrgTradeValue tradeValue)
		{
			return new TradeValueBreakdownKey(tradeValue.PAV_RX_NKCurrency, tradeValue.PAV_GC);
		}

		public static IEnumerable<T> AsEnumerable<T>(T item)
		{
			yield return item;
		}
	}
}
