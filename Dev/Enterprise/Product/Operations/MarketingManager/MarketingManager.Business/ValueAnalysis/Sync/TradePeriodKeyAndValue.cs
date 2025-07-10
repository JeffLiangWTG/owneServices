using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodKey : IEquatable<TradePeriodKey>
	{
		public TradePeriodKey(ZDate period, ZGuid orgPk, ZBool isJobValue)
		{
			Period = period;
			OrgPk = orgPk;
			IsJobValue = isJobValue;
		}

		public ZDate Period { get; private set; }
		public ZGuid OrgPk { get; private set; }
		public ZBool IsJobValue { get; private set; }

		public bool Equals(TradePeriodKey other)
		{
			if (other == null)
			{ return false; }
			return Period.Equals(other.Period) && OrgPk.Equals(other.OrgPk) && IsJobValue.Equals(other.IsJobValue);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TradeDetailKey);
		}

		public override int GetHashCode()
		{
			return Period.GetHashCode() ^ OrgPk.GetHashCode() ^ IsJobValue.GetHashCode();
		}
	}

	public class TradePeriodValue
	{
		public TradePeriodValue()
		{
			TradeValues = new Dictionary<TradeValueBreakdownKey, TradeValueBreakdown>(4);
		}
		public IDictionary<TradeValueBreakdownKey, TradeValueBreakdown> TradeValues { get; private set; }

		public ZDateTime PeriodLastTrade { get; private set; }
		public int NumberOfJobs { get; private set; }
		public int NumberOfPallets { get; private set; }
		public int NumberOfLines { get; private set; }
		public decimal WeightVolume { get; private set; }
		public decimal Weight { get; private set; }
		public decimal Volume { get; private set; }
		public decimal Chargeable { get; private set; }
		public string ChargeableUnits { get; private set; }
		public decimal TEU { get; private set; }

		internal void AddData(TradeLine line, bool isJobValue)
		{
			var key = new TradeValueBreakdownKey(line.Currency, line.ChargeCompanyPk);
			if (!TradeValues.TryGetValue(key, out TradeValueBreakdown tradeValue))
			{
				tradeValue = new TradeValueBreakdown();
				TradeValues.Add(key, tradeValue);
			}

			if (isJobValue)
			{
				tradeValue.AddValue(line.JobRevenue, line.JobCost);
			}
			else
			{
				tradeValue.AddValue(line.MainOrgRevenue, line.MainOrgCost);
			}

			PeriodLastTrade = line.PeriodLastTrade;
			PeriodLastTrade = line.PeriodLastTrade;
			NumberOfJobs = line.JobCount;
			NumberOfPallets = line.PalletCount;
			NumberOfLines = line.LineCount;
			WeightVolume = line.WeightVolume;
			Weight = line.WeightAmount;
			Volume = line.VolumeM3;
			Chargeable = line.ChargeableAmount;
			ChargeableUnits = line.ChargeableUnits;
			TEU = line.TEU;
		}
	}
}
