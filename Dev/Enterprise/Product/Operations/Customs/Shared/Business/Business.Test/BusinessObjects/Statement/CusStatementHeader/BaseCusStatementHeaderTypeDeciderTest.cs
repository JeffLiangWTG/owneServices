using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusStatementHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "BLA";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;

			var cusStatementHeader = Factory.New<TetsCusStatementHeader>();
			cusStatementHeader.B2_GC = testCompany.PK;
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			var cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct default type", ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct type for CA", ObjectFactory.GetType<Integration.Customs.CA.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct type for US", ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct type for KR", ObjectFactory.GetType<Integration.Customs.KR.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct type for FR", ObjectFactory.GetType<Integration.Customs.FR.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementHeaderInFactory1 = factory1.Load<BaseCusStatementHeader>(cusStatementHeader.PK);
			AssertEquals("Correct type for TR", ObjectFactory.GetType<Integration.Customs.TR.ICusStatementHeader>(), cusStatementHeaderInFactory1.GetType());
		}

		public void TestProcessTaskType()
		{
			var cusStatementHeader = Factory.New<TetsCusStatementHeader>();
			AssertEquals(typeof(StatementProcessTask), cusStatementHeader.ProcessTaskType);
		}
	}
}
