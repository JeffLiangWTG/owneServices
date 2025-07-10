using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCompanyWrapper))]
	public class CustomsNumberViewStmNumsCompanyWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsBranchLevel_Value()
		{
			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.PK, true, true);
			stmNums.SN_Owner = GlbCompany.CurrentCompany.PK;
			var companyLevelWrapper = new CustomsNumberViewStmNumsCompanyWrapper(stmNums);
			AssertEquals(false, companyLevelWrapper.IsBranchLevel);

			stmNums.SN_Owner = GlbBranch.CurrentBranch.PK;
			var branchLevelWrapper = new CustomsNumberViewStmNumsCompanyWrapper(stmNums);
			AssertEquals(true, branchLevelWrapper.IsBranchLevel);
		}

		public void TestIsBranchLevel_MetaData()
		{
			AssertEquals(true, wrapper.IsBranchLevelInfo.ReadOnly);
			AssertEquals("Is Branch Level", DataBoundResourceStrings.GetDataForProperty(wrapper.IsBranchLevelInfo).Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			stmNums.Provider = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.PK, true, true);
			stmNums.SN_Owner = GlbCompany.CurrentCompany.PK;
			wrapper = new CustomsNumberViewStmNumsCompanyWrapper(stmNums);
		}

		CustomsNumberViewStmNumsCompanyWrapper wrapper;
	}
}
