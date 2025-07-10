using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbDeptCharges))]
	sealed class GlbDeptChargesTest : EnterpriseBusinessObjectTestCase
	{
		#region Charge Code Filter

		public void TestChargeCodeFilter()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			GlbDeptCharges charge = department.DeptCharges.AddNew();

			AccChargeCode testChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode6 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode testChargeCode7 = Factory.NewWithValidTestData<AccChargeCode>();

			testChargeCode1.AC_DepartmentFilterList = "ALL";
			testChargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			testChargeCode1.AC_Code = "!!!";

			testChargeCode2.AC_DepartmentFilterList = "WHY";
			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			testChargeCode2.AC_Code = "@@@";

			testChargeCode3.AC_DepartmentFilterList = "BEA";
			testChargeCode3.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			testChargeCode3.AC_Code = "###";

			testChargeCode4.AC_DepartmentFilterList = "ALL";
			testChargeCode4.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			testChargeCode4.AC_Code = "$$$";

			testChargeCode5.AC_DepartmentFilterList = "WHY";
			testChargeCode5.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			testChargeCode5.AC_Code = "%%%";

			testChargeCode6.AC_DepartmentFilterList = "BEA";
			testChargeCode6.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			testChargeCode6.AC_Code = "^^^";

			testChargeCode7.AC_DepartmentFilterList = "ALL";
			testChargeCode7.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			testChargeCode7.AC_Code = "***";

			GlbDepartment testDept = Factory.New<GlbDepartment>();
			testDept.GE_Code = "BEA";
			charge.GD_AC = testChargeCode1.PK;
			charge.GD_GE = testDept.PK;

			Factory.Save();

			GlbDeptCharges testDeptCharge = testDept.DeptCharges.AddNew();
			testDeptCharge.AccChargeCodes.Load();
			Assert("should contain ChargeCode1", testDeptCharge.AccChargeCodes.Contains(testChargeCode1.PK));
			Assert("should not contain ChargeCode2", !testDeptCharge.AccChargeCodes.Contains(testChargeCode2.PK));
			Assert("should contain ChargeCode3", testDeptCharge.AccChargeCodes.Contains(testChargeCode3.PK));
			Assert("should not contain ChargeCode4", !testDeptCharge.AccChargeCodes.Contains(testChargeCode4.PK));
			Assert("should not contain ChargeCode5", !testDeptCharge.AccChargeCodes.Contains(testChargeCode5.PK));
			Assert("should not contain ChargeCode6", !testDeptCharge.AccChargeCodes.Contains(testChargeCode6.PK));
			Assert("should contain ChargeCode7", testDeptCharge.AccChargeCodes.Contains(testChargeCode7.PK));
		}

		#endregion

		#region Sequence Number

		public void TestSequenceNumber()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			GlbDeptCharges charge1 = department.DeptCharges.AddNew();
			GlbDeptCharges charge2 = department.DeptCharges.AddNew();
			GlbDeptCharges charge3 = department.DeptCharges.AddNew();

			AssertEquals((byte)1, charge1.GD_SequenceNumber);
			AssertEquals((byte)2, charge2.GD_SequenceNumber);
			AssertEquals((byte)3, charge3.GD_SequenceNumber);

			charge2.GD_SequenceNumber = 5;
			AssertEquals((byte)1, charge1.GD_SequenceNumber);
			AssertEquals((byte)5, charge2.GD_SequenceNumber);
			AssertEquals((byte)3, charge3.GD_SequenceNumber);

			charge3.GD_SequenceNumber = 5;
			AssertHasError(charge3.GD_SequenceNumberInfo, "All sequence numbers must be unique");

			department.DeptCharges.Delete(charge2);
			AssertEquals((byte)1, charge1.GD_SequenceNumber);
			AssertEquals((byte)5, charge3.GD_SequenceNumber);

			GlbDeptCharges charge4 = department.DeptCharges.AddNew();
			AssertEquals((byte)1, charge1.GD_SequenceNumber);
			AssertEquals((byte)5, charge3.GD_SequenceNumber);
			AssertEquals((byte)6, charge4.GD_SequenceNumber);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();
			return department.DeptCharges.AddNew();
		}

		#endregion
	}
}
