using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconEntryTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadForDE()
		{
			SetUpData(CountryCodes.Germany);
			var decider = new CusReconEntryTypeDecider();
			var reconEntry = Factory.New<CusReconEntry>();
			AssertEquals("Base Type", typeof(CusReconEntry), decider.GetTypeForLoad(((INeedRow)reconEntry).Row, Factory));

			var reconEntryForDE = Factory.New<CusReconEntry>();
			reconEntryForDE.CRE_GB_Branch = branch.PK;
			AssertEquals("DE Type", ObjectFactory.GetType<Integration.Customs.DE.ICusReconEntry>(), decider.GetTypeForLoad(((INeedRow)reconEntryForDE).Row, Factory));
		}

		public void TestGetTypeForLoadForKR()
		{
			SetUpData(CountryCodes.KoreaSouth);
			var decider = new CusReconEntryTypeDecider();
			var reconEntryForKR = Factory.New<CusReconEntry>();
			reconEntryForKR.CRE_GB_Branch = branch.PK;
			AssertEquals("KR Type", ObjectFactory.GetType<Integration.Customs.KR.ICusReconEntry>(), decider.GetTypeForLoad(((INeedRow)reconEntryForKR).Row, Factory));
		}

		void SetUpData(string countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
		}
		GlbBranch branch;
	}
}
