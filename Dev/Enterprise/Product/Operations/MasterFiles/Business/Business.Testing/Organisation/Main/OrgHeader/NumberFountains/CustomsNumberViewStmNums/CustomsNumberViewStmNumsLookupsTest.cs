using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsNumberViewStmNumsLookupsTest : BusinessObjectLookupsTestCase
	{
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

			var stmNums = new CustomsNumberViewStmNumsCompanyProviderForTest(Factory, Core.Constants.CountryCodes.UnitedStates, usCompany.PK, true, true).NewCustomsNumber();
			var lookups = stmNums.Lookups;
			AssertEquals("lookups.OwnerCollection", typeof(GlbCompanyCollection), lookups.OwnerCollection.GetType());

			stmNums.SN_Owner = usBranch.PK;
			var collection = lookups.OwnerCollection;
			AssertEquals("lookups.OwnerCollection", typeof(GlbBranchNotCurrentCompanyRelatedCollection), collection.GetType());
			var companyDefault = ((BusinessObjectCollection)collection).FilterBusinessObjectDefaults["Company:Property"];
			AssertNotNull(companyDefault);
			AssertEquals(usCompany.PK, companyDefault.Value);
			AssertEquals(false, companyDefault.IsRemovable);

			stmNums.SN_Owner = usCompany.PK;
			AssertEquals("lookups.OwnerCollection", typeof(GlbCompanyCollection), lookups.OwnerCollection.GetType());
		}
	}
}
