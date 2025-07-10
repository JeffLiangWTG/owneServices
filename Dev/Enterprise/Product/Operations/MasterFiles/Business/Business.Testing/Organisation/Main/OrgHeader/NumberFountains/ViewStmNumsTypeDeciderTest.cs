using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ViewStmNumsTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new ViewStmNumsTypeDecider();
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new ViewStmNumsTypeDecider();
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var typeDecider = new ViewStmNumsTypeDecider();
			AssertEquals(typeof(OrganisationViewStmNums), typeDecider.GetTypeForLoad(OrganisationViewStmNums.Schema.SN_NamePrefix + "HELLO", Factory));
			AssertEquals(typeof(CustomsNumberViewStmNums), typeDecider.GetTypeForLoad(CustomsNumberViewStmNums.Schema.SN_NamePrefix + "HELLO", Factory));
			AssertNull(typeDecider.GetTypeForLoad("H" + OrganisationViewStmNums.Schema.SN_NamePrefix + "HELLO", Factory));
			AssertNull(typeDecider.GetTypeForLoad("H" + CustomsNumberViewStmNums.Schema.SN_NamePrefix + "HELLO", Factory));

			var orgStmNums = Factory.New<OrganisationViewStmNums>();
			orgStmNums.SN_Type = OrgStmNumsTypeList.Codes.SSCCBarCodeNumbers;
			orgStmNums.SN_Prefix = "BOB";
			AssertEquals(typeof(OrganisationViewStmNums), typeDecider.GetTypeForLoad(orgStmNums, Factory));
			AssertEquals(typeof(OrganisationViewStmNums), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)orgStmNums).Row, Factory));

			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			AssertEquals(typeof(CustomsNumberViewStmNums), typeDecider.GetTypeForLoad(stmNums, Factory));
			AssertEquals(typeof(CustomsNumberViewStmNums), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)stmNums).Row, Factory));

			var staffStmNums = Factory.New<StaffViewStmNums>();
			AssertEquals(typeof(StaffViewStmNums), typeDecider.GetTypeForLoad(staffStmNums, Factory));
			AssertEquals(typeof(StaffViewStmNums), typeDecider.GetTypeForLoad(((IBusinessObjectInternals)staffStmNums).Row, Factory));
		}
	}
}
