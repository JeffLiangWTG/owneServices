using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranch.Loader))]
	sealed class GlbBranchLoaderTest : LoaderTestCase
	{
		public void TestIBranchImplementation()
		{
			var orgProxy = Factory.New<OrgHeader>();
			var glbCompany = Factory.New<GlbCompany>();
			var glbBranch = glbCompany.Branches.AddNew();
			glbBranch.GB_Code = "WWW";
			glbBranch.GB_BranchName = "WEE WILLY WINKY INK";
			glbBranch.GB_RL_NKHomePort = "AUDAR";
			glbBranch.GB_OH_OrgProxy = orgProxy.PK;
			glbBranch.GB_Phone = "1234567890";
			glbBranch.GB_State = "PANIC";

			IBranch branch = glbBranch;
			AssertEquals("WWW", branch.Code);
			AssertEquals(glbCompany.PK, branch.CompanyPK);
			AssertEquals("WEE WILLY WINKY INK", branch.Name);
			AssertEquals("AUDAR", branch.NKUNLOCO);
			AssertEquals(orgProxy.PK, branch.OrganisationPK);
			AssertEquals("1234567890", branch.Phone);
			AssertEquals(glbBranch.PK, branch.PK);
			AssertEquals("PANIC", branch.State);
			AssertEquals(glbCompany.PK, branch.Company.PK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new GlbBranch.Loader(this.Factory);
		}

		public void TestBranchGetter()
		{
			GlbBranch.Loader loader = (GlbBranch.Loader)GetNewLoaderToTest();
			CreateBranchesAndCompanies();
			Factory.Save();
			AssertEquals("Null if org header not specified", null, loader.LoadActiveMatchingBranchInThisCountry(null, Core.Constants.CountryCodes.UnitedKingdom));
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			youngs.GC_OH_OrgProxy = orgHeader.PK;
			Factory.Save();
			AssertEquals("First active branch", doubleChocoloateStout, loader.LoadActiveMatchingBranchInThisCountry(orgHeader, Core.Constants.CountryCodes.UnitedKingdom));
			youngs.GC_OH_OrgProxy = ZGuid.Empty;
			doubleChocoloateStout.GB_OH_OrgProxy = orgHeader.PK;
			AssertEquals("Branch with org proxy", doubleChocoloateStout, loader.LoadActiveMatchingBranchInThisCountry(orgHeader, Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals(4, loader.LoadAllBranchesInThisCountry(Core.Constants.CountryCodes.UnitedKingdom, true).Length);
			bombadier.GB_IsActive = false;
			Factory.Save();
			AssertEquals(3, loader.LoadAllBranchesInThisCountry(Core.Constants.CountryCodes.UnitedKingdom, true).Length);
			charlesWells.GC_IsActive = false;
			Factory.Save();
			AssertEquals(2, loader.LoadAllBranchesInThisCountry(Core.Constants.CountryCodes.UnitedKingdom, true).Length);
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.VietNam;
			Factory.Save();
			AssertEquals(0, loader.LoadAllBranchesInThisCountry(Core.Constants.CountryCodes.UnitedKingdom, true).Length);
		}

		GlbCompany youngs;
		GlbCompany charlesWells;
		GlbCompany carltonAndUnitedFosters;
		GlbBranch kewGold;
		GlbBranch doubleChocoloateStout;
		GlbBranch bombadier;
		GlbBranch waggleDance;
		GlbBranch victoriaBitter;

		void CreateBranchesAndCompanies()
		{
			//Today's theme is beer.

			youngs = Factory.New<GlbCompany>();
			charlesWells = Factory.New<GlbCompany>();
			carltonAndUnitedFosters = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			youngs.GC_Code = "YNG";
			charlesWells.GC_Code = "CHW";
			carltonAndUnitedFosters.GC_Code = "CUF";

			kewGold = youngs.Branches.AddNew();
			kewGold.GB_Code = "YKG";
			doubleChocoloateStout = youngs.Branches.AddNew();
			doubleChocoloateStout.GB_Code = "YCS";

			bombadier = charlesWells.Branches.AddNew();
			bombadier.GB_Code = "CWB";
			waggleDance = charlesWells.Branches.AddNew();
			waggleDance.GB_Code = "CWD";

			victoriaBitter = carltonAndUnitedFosters.Branches.AddNew();
			victoriaBitter.GB_Code = "FVB";
		}
	}
}
