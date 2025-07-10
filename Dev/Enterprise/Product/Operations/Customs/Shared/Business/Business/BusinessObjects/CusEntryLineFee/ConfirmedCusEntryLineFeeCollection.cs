using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusEntryLine), "ConfirmedFees")]
	public class ConfirmedCusEntryLineFeeCollection : DependentBusinessObjectCollection<CusEntryLineFee, CusEntryLine>
	{
		public ConfirmedCusEntryLineFeeCollection(CusEntryLine master)
			: base(master, new ZQuery(CusEntryLineFeeSchema.CF_Source, CusEntryLineFeeSourceCodeList.Codes.CUS))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var entryLineFee = child as CusEntryLineFee;
			entryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
		}

		public CusEntryLineFee AddOrUpdate(ZString feeType, ZDecimal amount)
		{
			return AddOrUpdate(feeType, amount, false);
		}

		public CusEntryLineFee AddOrUpdate(ZString feeType, ZDecimal amount, ZBool isLandedCostOnly)
		{
			var fee = GetElementWithThisCode(feeType, isLandedCostOnly);
			if (fee != null)
			{
				fee.CF_ChargeAmount = amount;
			}
			else
			{
				fee = AddNewCore(feeType, isLandedCostOnly, amount);
			}

			return fee;
		}

		public ZDecimal GetAmount(ZString feeType)
		{
			return GetElementWithThisCode(feeType)?.CF_ChargeAmount ?? ZDecimal.Zero;
		}

		public CusEntryLineFee GetElementWithThisCode(ZString feeType)
		{
			return GetElementWithThisCode(feeType, isLandedCostOnly: false);
		}

		public CusEntryLineFee GetElementWithThisCode(ZString feeType, bool isLandedCostOnly)
		{
			return Elements.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == feeType && x.CF_IsLandedCostOnly == isLandedCostOnly);
		}

		public void SetAmount(string feeType, ZDecimal amount)
		{
			CusEntryLineFee fee = GetElementWithThisCode(feeType);

			if (fee != null)
			{
				fee.CF_ChargeAmount = amount;
			}
			else if (amount > 0m)
			{
				fee = AddNewCore(feeType, false, amount);
			}
		}

		protected CusEntryLineFee AddNewCore(ZString feeType, ZBool isLandedCostOnly, ZDecimal amount)
		{
			var fee = AddNew();
			fee.CF_ChargeType = feeType;
			fee.CF_IsLandedCostOnly = isLandedCostOnly;
			fee.CF_ChargeAmount = amount;

			return fee;
		}
	}
}
