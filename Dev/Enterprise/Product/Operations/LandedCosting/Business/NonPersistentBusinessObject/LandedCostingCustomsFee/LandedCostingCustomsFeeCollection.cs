using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingCustomsFeeCollection : NonPersistentBusinessObjectCollection<LandedCostingCustomsFee>
	{
		public LandedCostingCustomsFeeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LandedCostingCustomsFee this[string feeType]
		{
			get { return Find(feeType); }
		}

		public void LoadCollection(IEnumerable<LandedCostHistory> histories)
		{
			foreach (LandedCostHistory history in histories)
			{
				if (history.UltimateDistributee != null)
				{
					LoadCollection(history.UltimateDistributee.Fees);
				}
			}
		}

		public void LoadCollection(IEnumerable<ICustomsFee> customsFees)
		{
			foreach (ICustomsFee customsFee in customsFees)
			{
				UpdateAmount(customsFee);
			}
		}

		public LandedCostingCustomsFee Find(string feeType)
		{
			foreach (LandedCostingCustomsFee fee in this)
			{
				if (fee.FeeType == feeType)
				{
					return fee;
				}
			}

			return null;
		}

		public void UpdateAmount(ICustomsFee customsFee)
		{
			LandedCostingCustomsFee result = Find(customsFee.FeeCode);

			if (result == null)
			{
				result = new LandedCostingCustomsFee(Factory);
				result.FeeType = customsFee.FeeCode;
				Add(result);
			}

			result.FeeAmount += customsFee.AmountInLocalCurrency;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LandedCostingCustomsFee(Factory);
		}

		#endregion
	}
}
