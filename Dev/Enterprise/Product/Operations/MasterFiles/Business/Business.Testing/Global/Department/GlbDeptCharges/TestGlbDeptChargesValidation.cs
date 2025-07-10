using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestGlbDeptChargesValidation : BusinessObjectValidationTestCase
	{
		public void TestValidateGD_AC()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			GlbDeptCharges charge = department.DeptCharges.AddNew();
			GlbDeptCharges charge1 = department.DeptCharges.AddNew();

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();

			charge.GD_AC = chargeCode1.PK;
			charge.GD_GC = GlbCompany.CurrentCompany.PK;

			charge1.GD_AC = charge.GD_AC;
			charge1.GD_GC = charge.GD_GC;

			charge.Validation.ValidateGD_AC();
			Assert("Cannot have two charge codes in the same department of the same company", charge.GD_ACInfo.HasErrors());

			AccChargeCode charge2 = Factory.New(typeof(AccChargeCode)) as AccChargeCode;
			charge2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			charge.GD_AC = charge2.PK;
			charge.Validation.ValidateGD_AC();
			AssertEquals("Charge type is allowed to be MRG", false, charge.GD_ACInfo.HasErrors());

			charge2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge.Validation.ValidateGD_AC();
			AssertEquals("Charge type is allowed to be REV", false, charge.GD_ACInfo.HasErrors());

			charge2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.Validation.ValidateGD_AC();
			AssertEquals("Charge type is allowed to be DSB", false, charge.GD_ACInfo.HasErrors());

			charge2.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			charge.Validation.ValidateGD_AC();
			AssertEquals("Charge type is allowed to be MJA", false, charge.GD_ACInfo.HasErrors());

			charge2.AC_ChargeType = "POP";
			charge.Validation.ValidateGD_AC();
			AssertEquals("Charge type must be one of MRG, REV, DSB or MJA", true, charge.GD_ACInfo.HasErrors());
		}
	}
}
