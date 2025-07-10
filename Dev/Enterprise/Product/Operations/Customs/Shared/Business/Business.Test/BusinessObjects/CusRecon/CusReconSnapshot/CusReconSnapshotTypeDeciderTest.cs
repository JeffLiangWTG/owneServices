using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconSnapshotTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadForKR()
		{
			var decider = new CusReconSnapshotTypeDecider();
			var snapshot = Factory.New<CusReconSnapshot>();
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var entryForKR = Factory.New<CusReconEntry>();
			entryForKR.CRE_GB_Branch = branch.PK;
			var entryLineForKR = Factory.New<CusReconEntryLine>();
			entryLineForKR.CRL_CRE = entryForKR.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Base Type", typeof(CusReconSnapshot), decider.GetTypeForLoad(((INeedRow)snapshot).Row, Factory));

				snapshot.CRS_CRL_Line = entryLineForKR.PK;
				AssertEquals("KR Type", ObjectFactory.GetType<Integration.Customs.KR.ICusReconSnapshot>(), decider.GetTypeForLoad(((INeedRow)snapshot).Row, Factory));
			});
		}
	}
}
