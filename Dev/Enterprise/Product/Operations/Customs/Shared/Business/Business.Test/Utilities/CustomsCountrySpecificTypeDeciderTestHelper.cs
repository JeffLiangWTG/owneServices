using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public static class CustomsCountrySpecificTypeDeciderTestHelper
	{
		public static GlbBranch GetNewBranchForTesting(BusinessObjectFactory factory, ZString countryCode)
		{
			var newCompany = factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = countryCode;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = string.Format("{0}A", newCompany.GC_RN_NKCountryCode);
			var branchPort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode));
			newBranch.GB_RL_NKHomePort = branchPort != null ? branchPort.RL_Code : ZString.Empty;
			return newBranch;
		}
	}
}
