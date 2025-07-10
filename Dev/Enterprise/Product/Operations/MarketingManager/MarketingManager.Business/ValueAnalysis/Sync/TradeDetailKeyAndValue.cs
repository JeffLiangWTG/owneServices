using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradeDetailKey : IEquatable<TradeDetailKey>
	{
		public TradeDetailKey(TradeLine line)
		{
			Mode = line.TradeMode;
			Type = line.TradeType;
			SupplierPartPk = line.SupplierPartPk;
		}

		public TradeDetailKey(OrgTradeDetail tradeDetail)
		{
			Mode = tradeDetail.PA_TradeMode;
			Type = tradeDetail.PA_TradeType;
			SupplierPartPk = tradeDetail.PA_OP;
		}

		public ZString Mode { get; private set; }
		public ZString Type { get; private set; }
		public ZGuid SupplierPartPk { get; private set; }

		public bool Equals(TradeDetailKey other)
		{
			if (other == null)
			{ return false; }
			return
				Mode.Equals(other.Mode)
				&& Type.Equals(other.Type)
				&& SupplierPartPk.Equals(other.SupplierPartPk);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TradeDetailKey);
		}

		public override int GetHashCode()
		{
			return
				Mode.GetHashCode()
				^ Type.GetHashCode()
				^ SupplierPartPk.GetHashCode();
		}
	}

	public class TradeDetailValue
	{
		public TradeDetailValue()
		{
			TradePeriods = new Dictionary<TradePeriodKey, TradePeriodValue>(4);
			RelatedRateEntryPks = new List<ZGuid>(1);
		}

		public TradeDetailValue(ZString statusCode) : this()
		{
			StatusCode = statusCode;
		}

		public IDictionary<TradePeriodKey, TradePeriodValue> TradePeriods { get; private set; }

		public IList<ZGuid> RelatedRateEntryPks { get; private set; }

		public ZString StatusCode { get; private set; }

		public void AddData(ZGuid viewPointOrgPk, TradeLine line)
		{
			if (line.MainOrg == viewPointOrgPk)
			{
				if (line.IsTraded)
				{
					AddOrgPeriodData(line, viewPointOrgPk, true);
				}
				AddOrgPeriodData(line, viewPointOrgPk, false);
			}
			StatusCode = line.StatusCode;
			RelatedRateEntryPks.Add(line.RelatedRateEntryPk);
		}

		void AddOrgPeriodData(TradeLine line, ZGuid orgPk, bool isJobValue)
		{
			var key = new TradePeriodKey(line.PeriodStart.Date, orgPk, isJobValue);
			if (!TradePeriods.TryGetValue(key, out TradePeriodValue tradePeriodValue))
			{
				tradePeriodValue = new TradePeriodValue();
				TradePeriods.Add(key, tradePeriodValue);
			}
			tradePeriodValue.AddData(line, isJobValue);
		}
	}
}
