
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandCostInputCollection : DependentBusinessObjectCollection<LandCostInput, LandedCostHeader>
	{
		public LandCostInputCollection(LandedCostHeader header) : base(header)
		{
		}

		public LandCostInput[] GetLCInputsToDistributeTo(ILandedCostDistributeTo distributeTo)
		{
			ArrayList result = new ArrayList();

			foreach (LandCostInput lCInput in this)
			{
				if (lCInput.Parent == distributeTo)
				{
					result.Add(lCInput);
				}
			}

			return (LandCostInput[])result.ToArray(typeof(LandCostInput));
		}

		public void DeleteSystemDefaultedRowsOnly()
		{
			for (int index = Count - 1; index >= 0; index--)
			{
				if (!this[index].LI_IsUserEntered)
				{
					this[index].Delete();
				}
			}
		}

		#region Calculated amount

		public ZDecimal TotalLandingCost
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.CostAmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup1
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group1AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup2
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group2AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup3
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group3AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup4
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group4AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup5
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group5AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroup6
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.Group6AmountInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalGroupMisc
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandCostInput costInput in this)
				{
					result += costInput.GroupMiscAmountInLocalCurrency;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			LandCostInput newElement = (LandCostInput)child;

			newElement.LI_IsUserEntered = true;
			newElement.LI_RX_NKCostCurrency = Master.Company.GC_RX_NKLocalCurrency;
		}

		#endregion
	}
}
