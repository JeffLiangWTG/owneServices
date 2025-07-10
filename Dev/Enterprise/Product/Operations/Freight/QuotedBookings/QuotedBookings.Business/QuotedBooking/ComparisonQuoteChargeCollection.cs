using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ComparisonQuoteChargeCollection : NonPersistentBusinessObjectCollection<ComparisonQuoteCharge>
	{
		#region Methods

		public ComparisonQuoteChargeCollection GetChargesBasedOnCurrency(ZString currency)
		{
			ComparisonQuoteChargeCollection result = new ComparisonQuoteChargeCollection();
			foreach (ComparisonQuoteCharge charge in this)
			{
				if (charge.OSSellCurrency == currency)
				{
					result.Add(charge);
				}
			}
			return result;
		}

		public List<ZString> Currencies
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (ComparisonQuoteCharge charge in this)
				{
					if (!result.Contains(charge.OSSellCurrency))
					{
						result.Add(charge.OSSellCurrency);
					}
				}
				return result;
			}
		}

		public ZDecimal TotalOSSellAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ComparisonQuoteCharge charge in this)
				{
					result += charge.OSSellAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalLocalSellAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ComparisonQuoteCharge charge in this)
				{
					result += charge.LocalSellAmount;
				}
				return result;
			}
		}

		public void RemoveZeroValueCharges()
		{
			for (int i = 0; i < this.Count; i++)
			{
				ComparisonQuoteCharge charge = this[i];
				if (charge.OSSellAmount == 0)
				{
					this.Remove(charge);
					i--;
				}
			}
		}

		public void Add(JobCharge charge)
		{
			Add(new ComparisonQuoteCharge(charge.JR_Desc, charge.JR_OSSellAmt, (charge.SellCurrency != null ? charge.JR_RX_NKSellCurrency : ZString.Empty), charge.JR_LocalSellAmt));
		}

		public void Add(ComparisonQuoteCharge result)
		{
			Add((BusinessObject)result);
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComparisonQuoteCharge();
		}

		#endregion
	}
}
