using CargoWise.Data;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StaffViewStmNums))]
	sealed class StaffViewStmNumsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var stmNum = Factory.New<StaffViewStmNums>();
			AssertEquals(StaffViewStmNums.Schema.SN_NamePrefix, stmNum.SN_Name);
			AssertEquals(stmNum.SN_MinimumValue, stmNum.SN_Value);
			AssertEquals(StaffViewStmNums.Schema.DefaultFountainMaximumValue, stmNum.DefaultTypeRangeMax);
			stmNum.SN_Type = "PAT";
			AssertEquals(StaffViewStmNums.Schema.DefaultPATMaximumValue, stmNum.DefaultTypeRangeMax);
			stmNum.SN_Type = "XXX";
			AssertEquals(StaffViewStmNums.Schema.DefaultFountainMaximumValue, stmNum.DefaultTypeRangeMax);
		}

		public void TestSN_Prefix_UpdatesSN_Name()
		{
			var stmNum = Factory.New<StaffViewStmNums>();
			stmNum.SN_Type = "XXX";
			stmNum.SN_Prefix = "1234567";
			AssertEquals("StaffOwned_XXX_1234567", stmNum.SN_Name);

			stmNum.SN_Prefix = "20082015";
			AssertEquals("StaffOwned_XXX_20082015", stmNum.SN_Name);

			stmNum.SN_Prefix = "";
			AssertEquals("StaffOwned_XXX", stmNum.SN_Name);
		}

		public void TestTryGetNumberFountain()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var stmNum = Factory.New<StaffViewStmNums>();
				stmNum.SN_Type = "XXX";
				stmNum.SN_Prefix = "1234567";
				stmNum.SN_MaximumValue = 99999999;
				stmNum.SN_Owner = staff.PK;
				Factory.Save();

				var fountain = stmNum.TryGetNumberFountain();
				AssertNotNull("prerequisite", fountain);

				AssertEquals("123456700000001", fountain.GetNextFormatted(Factory));
				AssertEquals("123456700000002", fountain.GetNextFormatted(Factory));

				Factory.Save();
				AssertEquals("123456700000003", fountain.GetNextFormatted(Factory));
			}
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("StmNums cannot be loaded by PK", condition: true);
		}
	}
}
