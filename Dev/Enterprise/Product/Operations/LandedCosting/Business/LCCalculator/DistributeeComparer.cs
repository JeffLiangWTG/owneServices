using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	/// <summary>
	/// Compare based on Cost in local currency in a descending order
	/// </summary>
	public class LCHistoryCostDescendingComparer : IComparer<LandedCostHistory>
	{
		#region IComparer Members

		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(LCHistoryCostDescendingComparer);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int Compare(LandedCostHistory lCHistoryX, LandedCostHistory lCHistoryY)
		{
			if (lCHistoryX == null && lCHistoryY != null)
			{
				return -1;
			}
			else if (lCHistoryX != null && lCHistoryY == null)
			{
				return 1;
			}
			else if (lCHistoryX == null && lCHistoryY == null)
			{
				return 0;
			}

			if (lCHistoryX.UltimateDistributee == null && lCHistoryY.UltimateDistributee != null)
			{
				return -1;
			}
			else if (lCHistoryX.UltimateDistributee != null && lCHistoryY.UltimateDistributee == null)
			{
				return 1;
			}
			else if (lCHistoryX.UltimateDistributee == null && lCHistoryY.UltimateDistributee == null)
			{
				return 0;
			}

			return lCHistoryY.UltimateDistributee.CostInLocalCurrency.CompareTo(lCHistoryX.UltimateDistributee.CostInLocalCurrency);
		}

		#endregion
	}

	public class DistributeeLineComparer : IComparer
	{
		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(DistributeeLineComparer);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			LandedCostHistory lCHistoryX = x as LandedCostHistory;
			LandedCostHistory lCHistoryY = y as LandedCostHistory;

			if (lCHistoryX == null && lCHistoryY != null)
			{
				return -1;
			}
			else if (lCHistoryX != null && lCHistoryY == null)
			{
				return 1;
			}
			else if (lCHistoryX == null && lCHistoryY == null)
			{
				return 0;
			}

			if (lCHistoryX.UltimateDistributee == null && lCHistoryY.UltimateDistributee != null)
			{
				return -1;
			}
			else if (lCHistoryX.UltimateDistributee != null && lCHistoryY.UltimateDistributee == null)
			{
				return 1;
			}
			else if (lCHistoryX.UltimateDistributee == null && lCHistoryY.UltimateDistributee == null)
			{
				return 0;
			}

			int result = 0;
			ILandedCostHeader lCHost = lCHistoryX.LCHeader.Parent;
			if (lCHost != null && lCHost.LineComparer != null)
			{
				result = lCHost.LineComparer.Compare(lCHistoryX.UltimateDistributee, lCHistoryY.UltimateDistributee);
			}
			return result;
		}

		#endregion
	}
}
