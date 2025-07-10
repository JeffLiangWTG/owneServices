using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchTestWithoutBusinessObject : TestCaseWithFactory
	{
		public void TestIsRegisteredVATInChinaFlag()
		{
			// Arrange
			var chineseCo = Factory.New<GlbCompany>();
			chineseCo.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			chineseCo.GC_Code = "001";
			var nonChineseCo = Factory.New<GlbCompany>();
			nonChineseCo.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonChineseCo.GC_Code = "AAA";

			var beijingBranchOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shanghaiBranchOrg = Factory.NewWithValidTestData<OrgHeader>();
			var nonChineseBranchOrg = Factory.NewWithValidTestData<OrgHeader>();

			var beijingBranch = chineseCo.Branches.AddNew();
			beijingBranch.GB_Code = "BEJ";
			beijingBranch.GB_OH_OrgProxy = beijingBranchOrg.PK;
			var shanghaiBranch = chineseCo.Branches.AddNew();
			shanghaiBranch.GB_Code = "SHA";
			shanghaiBranch.GB_OH_OrgProxy = shanghaiBranchOrg.PK;
			var nonChineseBranch = nonChineseCo.Branches.AddNew();
			nonChineseBranch.GB_Code = "BRS";
			nonChineseBranch.GB_OH_OrgProxy = nonChineseBranchOrg.PK;

			var shanghaiVatCustomsCode = Factory.NewWithValidTestData<OrgCusCode>();
			shanghaiVatCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			shanghaiVatCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			shanghaiVatCustomsCode.OK_CustomsRegNo = "111";
			shanghaiVatCustomsCode.OK_OH = shanghaiBranchOrg.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			beijingBranch = factory2.Load<GlbBranch>(beijingBranch.PK);
			shanghaiBranch = factory2.Load<GlbBranch>(shanghaiBranch.PK);
			nonChineseBranch = factory2.Load<GlbBranch>(nonChineseBranch.PK);

			AssertEquals(false, beijingBranch.IsRegisteredVATInChina);
			AssertEquals(true, shanghaiBranch.IsRegisteredVATInChina);
			AssertEquals(false, nonChineseBranch.IsRegisteredVATInChina);
		}
	}
}
