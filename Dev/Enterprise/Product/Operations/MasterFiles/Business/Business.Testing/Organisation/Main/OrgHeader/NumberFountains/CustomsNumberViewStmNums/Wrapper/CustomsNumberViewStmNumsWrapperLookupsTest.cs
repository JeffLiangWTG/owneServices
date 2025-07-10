namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsNumberViewStmNumsWrapperLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var stmNum = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory).NewCustomsNumber();
			var wrapper = stmNum.Wrapper;
			var lookups = wrapper.Lookups;
			var list = stmNum.Lookups.TypeList;
			AssertEquals(true, object.ReferenceEquals(list, wrapper.Lookups.TypeList));
		}

		public void TestOwnerCollection()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "US$";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "US%";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var stmNum = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory).NewCustomsNumber();
			var wrapper = stmNum.Wrapper;
			var lookups = wrapper.Lookups;
			AssertEquals("lookups.OwnerCollection", typeof(GlbCompanyCollection), lookups.OwnerCollection.GetType());

			stmNum.SN_Owner = usBranch.PK;
			AssertEquals("lookups.OwnerCollection", typeof(GlbBranchNotCurrentCompanyRelatedCollection), lookups.OwnerCollection.GetType());
			stmNum.SN_Owner = usCompany.PK;
			AssertEquals("lookups.OwnerCollection", typeof(GlbCompanyCollection), lookups.OwnerCollection.GetType());
		}
	}
}
