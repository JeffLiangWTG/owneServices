using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryOriginalChargeCollection : Customs.Business.CusCodeDataCollection<ReconEntryOriginalCharge>, IFees
	{
		public ReconEntryOriginalChargeCollection(IReconOriginalChargeParent parent)
			: base(parent.ParentAsBusinessObject, CusCodeDataTypeList.Codes.ReconEntryOriginalCharge)
		{
			this.parent = parent;
		}

		internal void UpdateFromReadOnlyState(bool readOnly)
		{
			foreach (ReconEntryOriginalCharge charge in this)
			{
				charge.CY_IsOverridden = !readOnly;
			}
		}

		public ReconEntryOriginalCharge AddNew(ZString chargeType, ZDecimal chargeAmount)
		{
			var result = AddNew();
			result.CY_Amount = chargeAmount;
			result.CY_Code = chargeType;
			return result;
		}

		public ReconEntryOriginalCharge GetCharge(string chargeType)
		{
			ReconEntryOriginalCharge result = null;
			foreach (ReconEntryOriginalCharge charge in this)
			{
				if (charge.CY_Code == chargeType)
				{
					result = charge;
					break;
				}
			}
			return result;
		}

		public ZDecimal TotalOriginalAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ReconEntryOriginalCharge charge in this)
				{
					if (charge.CY_Code != Core.Constants.USCustoms.FeeCodes.MPC)
					{
						result += charge.CY_Amount;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalFeeAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ReconEntryOriginalCharge charge in this)
				{
					if (Registry.Business.Customs.US.EntryChargeTypeList.IsFeeType(charge.CY_Code))
					{
						result += charge.CY_Amount;
					}
				}
				return result;
			}
		}

		public ZDecimal GetAmount(string chargeType)
		{
			var result = GetCharge(chargeType);
			return result == null ? ZDecimal.Zero : result.CY_Amount;
		}

		public ZDecimal GetTotal(IEnumerable<string> codeTypes)
		{
			var result = ZDecimal.Zero;

			var passedCodeTypes = new List<string>(codeTypes);

			foreach (ReconEntryOriginalCharge charge in this)
			{
				if (passedCodeTypes.Contains(charge.CY_Code.ToString()))
				{
					result += charge.CY_Amount;
				}
			}

			return result;
		}

		public ReconEntryOriginalCharge GetFirstFeeOtherThanTaxHMFOrMPF()
		{
			return this.Cast<ReconEntryOriginalCharge>().FirstOrDefault(x => x.CY_Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
						  x.CY_Code != Core.Constants.USCustoms.FeeCodes.HMF &&
						  !CusFeeCodeConstants.IsExciseTax(x.CY_Code));
		}

		public void SetAmount(ZString chargeType, ZDecimal amount)
		{
			var charge = GetCharge(chargeType);

			if (charge == null && amount > 0m)
			{
				charge = AddNew(chargeType);
			}

			if (charge != null)
			{
				charge.CY_Amount = amount;
			}
		}

		public bool HasDuplicate(string chargeType)
		{
			var allCharges = GetElementsHaving(chargeType);
			return allCharges.Length > 1;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			((ReconEntryOriginalCharge)child).CY_IsOverridden = parent.DefaultValueForOverridenForNewChild;
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			parent.SynchroniseOnNonCommittedAdded((ReconEntryOriginalCharge)bizOAdded);
		}

		protected override bool AllowNewCore => base.AllowNewCore && parent.DefaultValueForOverridenForNewChild;

		protected override bool AllowRemoveCore => base.AllowRemoveCore && parent.DefaultValueForOverridenForNewChild;

		readonly IReconOriginalChargeParent parent;

		#region IFees Members

		IFee IFees.GetFeeFor(ZString code)
		{
			return GetCharge(code);
		}

		IFee IFees.AddNew()
		{
			return AddNew();
		}

		#endregion
	}
}
