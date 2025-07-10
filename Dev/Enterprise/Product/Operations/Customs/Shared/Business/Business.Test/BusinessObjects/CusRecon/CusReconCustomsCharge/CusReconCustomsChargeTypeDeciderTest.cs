using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconCustomsChargeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadForKR()
		{
			var decider = new CusReconCustomsChargeTypeDecider();
			var charge = Factory.New<CusReconCustomsCharge>();
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
				AssertEquals("Base Type", typeof(CusReconCustomsCharge), decider.GetTypeForLoad(((INeedRow)charge).Row, Factory));

				charge.CRC_CRL_Line = entryLineForKR.PK;
				AssertEquals("KR Type", ObjectFactory.GetType<Integration.Customs.KR.ICusReconCustomsCharge>(), decider.GetTypeForLoad(((INeedRow)charge).Row, Factory));
			});
		}
	}
}
