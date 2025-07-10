using System;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class TradeValueBreakdownKey : IEquatable<TradeValueBreakdownKey>
	{
		public TradeValueBreakdownKey(string currency, ZGuid companyPk)
		{
			this.Currency = currency;
			this.CompanyPk = companyPk;
		}

		public ZGuid CompanyPk { get; private set; }
		public string Currency { get; private set; }

		public override bool Equals(object obj)
		{
			return base.Equals(obj as TradeValueBreakdownKey);
		}

		public override int GetHashCode()
		{
			return Currency.GetHashCode() ^ CompanyPk.GetHashCode();
		}

		public bool Equals(TradeValueBreakdownKey other)
		{
			if (other == null)
			{ return false; }
			return Currency.Equals(other.Currency) && CompanyPk.Equals(other.CompanyPk);
		}
	}

	public class TradeValueBreakdown
	{
		public void AddValue(decimal revenue, decimal cost)
		{
			this.Revenue += revenue;
			this.Cost += cost;
		}

		public decimal Revenue { get; private set; }
		public decimal Cost { get; private set; }
	}
}
