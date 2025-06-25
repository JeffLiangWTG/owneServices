using System;
using CargoWise.Billing.API;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public sealed class TimeStampedTransaction
	{
		public TimeStampedTransaction(DateTime timeStamp, BillingTransaction billingTransaction, UsageTransaction? usageTransaction = null)
		{
			TimeStamp = timeStamp;
			BillingTransaction = billingTransaction;
			UsageTransaction = usageTransaction;
		}
		
		public BillingTransaction BillingTransaction { get; }

		public UsageTransaction? UsageTransaction { get; }

		public DateTime TimeStamp { get; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((TimeStampedTransaction) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((BillingTransaction != null ? BillingTransaction.GetHashCode() : 0) * 397) ^ TimeStamp.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("TimeStamp [{0}], Transaction[{1}]", TimeStamp, BillingTransaction);
		}

		bool Equals(TimeStampedTransaction other)
		{
			return Equals(BillingTransaction, other.BillingTransaction) && TimeStamp.Equals(other.TimeStamp);
		}
	}
}
