using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusStatementLineChargeTypeDeciderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
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
			var cusStatementLineCharge = Factory.New<TetsCusStatementLineCharge>();
			cusStatementLineCharge.B4_B3 = cusStatementLine.PK;
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			var cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct default type", ObjectFactory.GetType<Integration.Customs.US.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct type for CA", ObjectFactory.GetType<Integration.Customs.CA.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct type for US", ObjectFactory.GetType<Integration.Customs.US.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct type for KR", ObjectFactory.GetType<Integration.Customs.KR.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusStatementLine.B3_EntryType = "DCG";
			cusStatementLine.B3_BrokerReference = "B00000001";
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct type for FR when statement line B3_EntryType is DCG", ObjectFactory.GetType<Integration.Customs.FR.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());

			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			Factory.Save();
			factory1 = new BusinessObjectFactory();
			cusStatementLineChargeInFactory1 = factory1.Load<BaseCusStatementLineCharge>(cusStatementLineCharge.PK);
			AssertEquals("Correct type for TR", ObjectFactory.GetType<Integration.Customs.TR.ICusStatementLineCharge>(), cusStatementLineChargeInFactory1.GetType());
		}
	}
}
