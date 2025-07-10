using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	internal class BaseCusSCAPivotTest : TestCaseWithFactory
	{
		public void TestCusSCAPivotTypeDecider()
		{
			TestCusSCAOceanBill cusSCAOceanBill = Factory.New<TestCusSCAOceanBill>();
			TestCusSCAHouse cusSCAHouse = Factory.New<TestCusSCAHouse>();
			cusSCAHouse.CA_CB = cusSCAOceanBill.PK;
			TestCusSCAPivot cusSCAPivot = Factory.New<TestCusSCAPivot>();
			cusSCAPivot.CV_CA = cusSCAHouse.PK;
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var cusSCAPivotInFactory1 = factory1.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory1.GetType(), typeof(TestCusSCAPivot));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var cusSCAPivotInFactory2 = factory2.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory2.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAPivot>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			Factory.Save();
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var cusSCAPivotInFactory3 = factory3.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory3.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAPivot>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			Factory.Save();
			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			var cusSCAPivotInFactory4 = factory4.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory4.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAPivot>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			Factory.Save();
			BusinessObjectFactory factory5 = new BusinessObjectFactory();
			var cusSCAPivotInFactory5 = factory5.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory5.GetType(), ObjectFactory.GetType<Integration.Customs.CA.ICusSCAPivot>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();
			BusinessObjectFactory factory6 = new BusinessObjectFactory();
			var cusSCAPivotInFactory6 = factory6.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory6.GetType(), ObjectFactory.GetType<Integration.Customs.AU.ICusSCAPivot>());

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy;
			Factory.Save();
			BusinessObjectFactory factory7 = new BusinessObjectFactory();
			var cusSCAPivotInFactory7 = factory7.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Default type", cusSCAPivotInFactory7.GetType(), typeof(DefaultCusSCAPivot));

			cusSCAOceanBill.CB_ApplicationCode = "";
			Factory.Save();
			BusinessObjectFactory factory8 = new BusinessObjectFactory();
			var cusSCAPivotInFactory8 = factory8.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Default type", cusSCAPivotInFactory8.GetType(), typeof(DefaultCusSCAPivot));

			cusSCAOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			Factory.Save();
			var factory9 = new BusinessObjectFactory();
			var cusSCAPivotInFactory9 = factory9.Load<BaseCusSCAPivot>(cusSCAPivot.PK);
			AssertEquals("Correct type", cusSCAPivotInFactory9.GetType(), ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAPackingLine>());
		}
	}
}
