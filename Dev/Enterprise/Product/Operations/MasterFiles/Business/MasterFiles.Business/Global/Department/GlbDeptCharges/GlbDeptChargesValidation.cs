using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbDeptChargesValidation : AutoGlbDeptChargesValidation
	{
		public GlbDeptChargesValidation(AutoGlbDeptCharges parent) : base(parent)
		{
		}

		protected override void CheckGD_AC()
		{
			base.CheckGD_AC();
			if (!Parent.GD_ACInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(GlbDeptChargesSchema.GD_AC, Parent.GD_AC);
				filter.AddToFilter(JoinCondition.And, GlbDeptChargesSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JoinCondition.And, GlbDeptChargesSchema.GD_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(JoinCondition.And, GlbDeptChargesSchema.GD_GE, SQLComparisonOperator.Equal, Parent.GD_GE);
				GlbDeptCharges deptCharge = Parent.Factory.LoadTop1(typeof(GlbDeptCharges), filter) as GlbDeptCharges;
				if (deptCharge != null) // adding would violate business rule
				{
					Parent.GD_ACInfo.AddError(Res.GetString("0cf918b0-cf42-4859-b54f-257796336a7a", "Charge code must be unique in any given company"));
				}
			}

			if (!Parent.GD_ACInfo.HasErrors())
			{
				AccChargeCode chargeCode = Parent.Factory.Load(typeof(AccChargeCode), Parent.GD_AC) as AccChargeCode;
				if (chargeCode != null && chargeCode.AC_ChargeType != Core.Constants.ChargeType.Margin && chargeCode.AC_ChargeType != Core.Constants.ChargeType.Revenue && chargeCode.AC_ChargeType != Core.Constants.ChargeType.Disbursement && chargeCode.AC_ChargeType != Core.Constants.ChargeType.ManualJobAccrual)
				{
					Parent.GD_ACInfo.AddError(Res.GetString("4523ff1f-1ccd-42c8-b894-89aac7de9199", "Charge type must be either MRG, REV, DSB or MJA"));
				}
			}
		}

		protected override void CheckGD_SequenceNumber()
		{
			base.CheckGD_SequenceNumber();

			if (Parent.Department.DeptCharges.Any(x => x.GD_SequenceNumber == Parent.GD_SequenceNumber && x.PK != Parent.PK))
			{
				Parent.GD_SequenceNumberInfo.AddError(Res.GetString("0cf918b0-cf42-4859-b54f-257796336a7b", "All sequence numbers must be unique"));
			}
		}
	}
}
