using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineChargeCollection : DependentBusinessObjectCollection<CusStatementLineCharge, CusStatementLine>
	{
		public CusStatementLineChargeCollection(CusStatementLine statementLine)
			: base(statementLine)
		{
		}

		public new CusStatementLine Master
		{
			get { return base.Master; }
		}

		/// <summary>
		/// If there is an existing charge and amount == 0, then the charge is to be deleted
		/// If there is no charge, then create a new charge and update amount.
		/// </summary>
		public void UpdateLineChargeFor(ZString chargeType, ZDecimal amount)
		{
			CusStatementLineCharge result = GetFirstCharge(chargeType);

			if (result != null && amount == 0)
			{
				result.Delete();
			}
			else if (amount > 0)
			{
				if (result == null)
				{
					result = AddNew();
					result.B4_ChargeType = chargeType;
				}
				result.B4_ChargeAmount = amount;
			}
		}

		public ZDecimal GetTotalPayableAmount()
		{
			ZDecimal result = 0m;

			foreach (CusStatementLineCharge charge in this)
			{
				if (EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(Master.B3_EntryType, charge.B4_ChargeType))
				{
					result += charge.B4_ChargeAmount;
				}
			}

			return result;
		}

		public CusStatementLineCharge GetFirstCharge(ZString chargeType)
		{
			ZQuery query = new ZQuery(CusStatementLineChargeSchema.B4_ChargeType, chargeType);
			CusStatementLineCharge[] lines = (CusStatementLineCharge[])Find(query);

			CusStatementLineCharge result = lines.Length > 0 ? lines[0] : null;
			return result;
		}
	}
}
