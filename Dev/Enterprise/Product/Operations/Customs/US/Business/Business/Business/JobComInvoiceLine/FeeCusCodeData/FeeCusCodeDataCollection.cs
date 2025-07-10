using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FeeCusCodeDataCollection : Customs.Business.CusCodeDataCollection<FeeCusCodeData>, IFees
	{
		public FeeCusCodeDataCollection(BusinessObject parent)
			: base(parent, CusCodeDataTypeList.Codes.Fee)
		{
		}

		internal ZDecimal GetValue(ZString feeCode)
		{
			ZDecimal value = ZDecimal.Zero;
			foreach (FeeCusCodeData fee in this)
			{
				if (fee.CY_Code == feeCode)
				{
					value += fee.CY_FeeAmount;
				}
			}
			return value;
		}

		public void RemoveFee(ZString feeType)
		{
			FeeCusCodeData[] fees = GetElementsHaving(feeType);

			foreach (FeeCusCodeData fee in fees)
			{
				fee.Delete();
			}
		}

		public FeeCusCodeData GetCharge(string chargeType)
		{
			FeeCusCodeData result = null;
			foreach (FeeCusCodeData charge in this)
			{
				if (charge.CY_Code == chargeType)
				{
					result = charge;
					break;
				}
			}
			return result;
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
				charge.CY_FeeAmount = amount;
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.UpdateReconChargeDetails();
			}
		}

		#region IFees Members

		IFee IFees.GetFeeFor(ZString code)
		{
			return GetFirstElementHaving(code);
		}

		IFee IFees.AddNew()
		{
			return AddNew();
		}

		#endregion
	}
}
