using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class TradeLinesSummary
	{
		public TradeLinesSummary(TradeLinesSynchronizationRange syncRange)
		{
			this.syncRange = syncRange;
			ActualValues = new Dictionary<TradeLaneKey, TradeLaneValue>();
			ProspectValues = new Dictionary<TradeLaneKey, TradeLaneValue>();
		}
		readonly TradeLinesSynchronizationRange syncRange;

		public ZDate From => syncRange.From;
		public ZDate To => syncRange.To;
		public ZBool IncludeActuals => syncRange.IncludeActuals;
		public ZBool IncludeProspect => syncRange.IncludeProspect;

		public IDictionary<TradeLaneKey, TradeLaneValue> ActualValues { get; private set; }
		public IDictionary<TradeLaneKey, TradeLaneValue> ProspectValues { get; private set; }

		public void AddData(ZGuid viewPointOrgPk, TradeLine line)
		{
			var tradeLanes = (line.IsTraded ? ActualValues : ProspectValues);
			var tradeStatusType = (line.IsTraded ? TradeLaneKey.TradeType.Actual : TradeLaneKey.TradeType.Prospect);
			var key = new TradeLaneKey(tradeStatusType, line);

			if (!tradeLanes.TryGetValue(key, out TradeLaneValue tradeLane))
			{
				tradeLane = new TradeLaneValue();
				tradeLanes.Add(key, tradeLane);
			}

			tradeLane.AddData(viewPointOrgPk, line);
		}

		public void ResetActualValues()
		{
			ActualValues = new Dictionary<TradeLaneKey, TradeLaneValue>();
		}
	}

	public class TradeLinesSynchronizationRange
	{
		public TradeLinesSynchronizationRange(ZDate from, ZDate to, bool includeActuals = true, bool includeProspect = true)
		{
			From = from;
			To = to;
			IncludeActuals = includeActuals;
			IncludeProspect = includeProspect;
		}

		public readonly ZDate From;
		public readonly ZDate To;
		public readonly ZBool IncludeActuals;
		public readonly ZBool IncludeProspect;
	}
}
