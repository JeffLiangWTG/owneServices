using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconEntryLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadForDE()
		{
			SetUpData(CountryCodes.Germany);
			var decider = new CusReconEntryLineTypeDecider();
			var entryLine = Factory.New<CusReconEntryLine>();
			AssertEquals("Base Type", typeof(CusReconEntryLine), decider.GetTypeForLoad(((INeedRow)entryLine).Row, Factory));

			var entryForDE = Factory.New<CusReconEntry>();
			entryForDE.CRE_GB_Branch = branch.PK;
			entryLine.CRL_CRE = entryForDE.PK;
			AssertEquals("DE Type", ObjectFactory.GetType<Integration.Customs.DE.ICusReconEntryLine>(), decider.GetTypeForLoad(((INeedRow)entryLine).Row, Factory));
		}

		public void TestGetTypeForLoadForKR()
		{
			SetUpData(CountryCodes.KoreaSouth);
			var decider = new CusReconEntryLineTypeDecider();
			var entryForKR = Factory.New<CusReconEntry>();
			entryForKR.CRE_GB_Branch = branch.PK;
			var entryLine = Factory.New<CusReconEntryLine>();
			entryLine.CRL_CRE = entryForKR.PK;
			AssertEquals("KR Type", ObjectFactory.GetType<Integration.Customs.KR.ICusReconEntryLine>(), decider.GetTypeForLoad(((INeedRow)entryLine).Row, Factory));
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
