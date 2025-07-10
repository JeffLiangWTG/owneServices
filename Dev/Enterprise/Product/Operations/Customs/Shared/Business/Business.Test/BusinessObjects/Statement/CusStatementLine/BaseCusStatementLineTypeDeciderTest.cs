using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusStatementLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "BLA";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;

			var cusStatementHeader = Factory.New<TetsCusStatementHeader>();
			cusStatementHeader.B2_GC = testCompany.PK;
			var cusStatementLine = Factory.New<TetsCusStatementLine>();
			cusStatementLine.B3_B2 = cusStatementHeader.PK;
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			var cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct default type", ObjectFactory.GetType<Integration.Customs.US.ICusStatementLine>(), cusStatementLineInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for CA", ObjectFactory.GetType<Integration.Customs.CA.ICusStatementLine>(), cusStatementLineInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for US", ObjectFactory.GetType<Integration.Customs.US.ICusStatementLine>(), cusStatementLineInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for KR", ObjectFactory.GetType<Integration.Customs.KR.ICusStatementLine>(), cusStatementLineInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusStatementLine.B3_EntryType = "DCG";
			cusStatementLine.B3_BrokerReference = "B00000001";
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for FR when B3_EntryType is DCG", ObjectFactory.GetType<Integration.Customs.FR.ICusStatementChargesDetail>(), cusStatementLineInFactory1.GetType());

			cusStatementLine.B3_EntryType = "EXP";
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for FR when B3_EntryType is Import", ObjectFactory.GetType<Integration.Customs.FR.ICusStatementEntry>(), cusStatementLineInFactory1.GetType());

			cusStatementLine.B3_EntryType = "IMP";
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for FR when B3_EntryType is Export", ObjectFactory.GetType<Integration.Customs.FR.ICusStatementEntry>(), cusStatementLineInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineInFactory1 = factory1.Load<BaseCusStatementLine>(cusStatementLine.PK);
			AssertEquals("Correct type for TR", ObjectFactory.GetType<Integration.Customs.TR.ICusStatementLine>(), cusStatementLineInFactory1.GetType());
		}
	}
}
