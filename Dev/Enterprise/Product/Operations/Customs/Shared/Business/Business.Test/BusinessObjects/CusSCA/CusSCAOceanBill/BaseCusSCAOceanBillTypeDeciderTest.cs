using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusSCAOceanBillTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetApplicationCodesForForwarding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, BaseCusSCAOceanBillTypeDecider.GetApplicationCodesForForwarding()[0]);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PapuaNewGuinea))
			{
				AssertEquals(0, BaseCusSCAOceanBillTypeDecider.GetApplicationCodesForForwarding().Length);
			}
		}

		public void TestGetTypeForNew()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Assert(Factory.New<BaseCusSCAOceanBill>() is Integration.Customs.AU.ICusSCAOceanBill);
			}
		}

		public void TestUniversalDataContext()
		{
			var cusSCAOceanBill = Factory.New<TestCusSCAOceanBill>();
			AssertEquals(DataContextType.SeaOceanBill, cusSCAOceanBill.GetUniversalDataContextManager().DataContextType);
		}

		public void TestCusSCAOceanBillTypeDecider()
		{
			var cusSCAOceanBill = Factory.New<TestCusSCAOceanBill>();
			Factory.Save();
			var factory1 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory1 = factory1.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory1.GetType(), typeof(TestCusSCAOceanBill));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory2 = factory2.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory2.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			Factory.Save();
			var factory3 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory3 = factory3.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory3.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			Factory.Save();
			var factory4 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory4 = factory4.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory4.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			Factory.Save();
			var factory5 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory5 = factory5.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory5.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			Factory.Save();
			var factory9 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory9 = factory9.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type - should be NZ oceanbill object", cusSCAOceanBillInFactory9.GetType(), ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();
			var factory6 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory6 = factory6.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Correct type", cusSCAOceanBillInFactory6.GetType(), ObjectFactory.GetType<Integration.Customs.AU.ICusSCAOceanBill>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy;
			Factory.Save();
			var factory7 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory7 = factory7.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Default type", cusSCAOceanBillInFactory7.GetType(), typeof(DefaultCusSCAOceanBill));

			cusSCAOceanBill.CB_ApplicationCode = "";
			Factory.Save();
			var factory8 = new BusinessObjectFactory();
			var cusSCAOceanBillInFactory8 = factory8.Load<BaseCusSCAOceanBill>(cusSCAOceanBill.PK);
			AssertEquals("Default type", cusSCAOceanBillInFactory8.GetType(), typeof(DefaultCusSCAOceanBill));
		}

		public void TestOceanBillIsValableForChina()
		{
			AssertNoExceptionThrown(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
				{
					var consol = Factory.New<ForwardingConsol>();
					var oceanBill = Factory.New<Integration.Customs.CA.ICusSCAOceanBill>() as BaseCusSCAOceanBill;
					oceanBill.CB_ParentId = consol.PK;
					oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
					oceanBill.CB_ApplicationCode = oceanBill.ApplicationCodesForBase[0];
					oceanBill.CB_OceanBill = "OC1";
					Factory.Save();
					AssertNullOrEmpty("No Last Message Reported", ErrorReporter.LastMessageReported);
				}
			});
		}
	}
}
