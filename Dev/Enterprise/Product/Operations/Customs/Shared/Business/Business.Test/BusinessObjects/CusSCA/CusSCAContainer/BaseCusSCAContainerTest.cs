using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	internal class BaseCusSCAContainerTest : TestCaseWithFactory
	{
		public void TestCusSCAContainerTypeDecider()
		{
			TestCusSCAOceanBill cusSCAOceanBill = Factory.New<TestCusSCAOceanBill>();
			TestCusSCAContainer cusSCAContainer = Factory.New<TestCusSCAContainer>();
			cusSCAContainer.CN_CB = cusSCAOceanBill.PK;
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var cusSCAContainerInFactory1 = factory1.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type", cusSCAContainerInFactory1.GetType(), typeof(TestCusSCAContainer));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var cusSCAContainerInFactory2 = factory2.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - CanadaACIAir", cusSCAContainerInFactory2.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAContainer>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			Factory.Save();
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var cusSCAContainerInFactory3 = factory3.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - CanadaACISea", cusSCAContainerInFactory3.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAContainer>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			Factory.Save();
			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			var cusSCAContainerInFactory4 = factory4.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - CanadaACIRoad", cusSCAContainerInFactory4.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAContainer>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			Factory.Save();
			BusinessObjectFactory factory5 = new BusinessObjectFactory();
			var cusSCAContainerInFactory5 = factory5.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - CanadaACIRail", cusSCAContainerInFactory5.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAContainer>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();
			BusinessObjectFactory factory6 = new BusinessObjectFactory();
			var cusSCAContainerInFactory6 = factory6.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - AU", cusSCAContainerInFactory6.GetType(), (ObjectFactory.GetType<Integration.Customs.AU.ICusSCAContainer>()));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy;
			Factory.Save();
			BusinessObjectFactory factory7 = new BusinessObjectFactory();
			var cusSCAContainerInFactory7 = factory7.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Default type", cusSCAContainerInFactory7.GetType(), typeof(DefaultCusSCAContainer));

			cusSCAOceanBill.CB_ApplicationCode = "";
			Factory.Save();
			BusinessObjectFactory factory8 = new BusinessObjectFactory();
			var cusSCAContainerInFactory8 = factory8.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Default type", cusSCAContainerInFactory8.GetType(), typeof(DefaultCusSCAContainer));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			Factory.Save();
			BusinessObjectFactory factory9 = new BusinessObjectFactory();
			var cusSCAContainerInFactory9 = factory9.Load<BaseCusSCAContainer>(cusSCAContainer.PK);
			AssertEquals("Correct type - should be NZ CusSCAContainer object", cusSCAContainerInFactory9.GetType(), ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAContainer>());
		}
	}
}
