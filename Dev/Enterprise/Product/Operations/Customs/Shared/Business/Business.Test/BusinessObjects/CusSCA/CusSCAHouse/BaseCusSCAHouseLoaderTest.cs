using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusSCAHouse.Loader))]
	sealed class BaseCusSCAHouseLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseCusSCAHouse.Loader(Factory);
		}

		public void TestLoadFromShipmentAndApplicationCode()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			var testCusSCAOceanBill1 = Factory.New<TestCusSCAOceanBill>();
			var testCusSCAHouse1 = Factory.New<TestCusSCAHouse>();
			testCusSCAHouse1.CA_CB = testCusSCAOceanBill1.PK;
			testCusSCAOceanBill1.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testCusSCAHouse1.CA_JS = shipment1.PK;
			var testCusSCAOceanBill2 = Factory.New<TestCusSCAOceanBill>();
			var testCusSCAHouse2 = Factory.New<TestCusSCAHouse>();
			testCusSCAHouse2.CA_CB = testCusSCAOceanBill2.PK;
			testCusSCAOceanBill2.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
			testCusSCAHouse2.CA_JS = shipment1.PK;
			Factory.Save();
			BaseCusSCAHouse.Loader loader = new BaseCusSCAHouse.Loader(Factory);
			AssertEquals("Loads correct object", testCusSCAHouse1.PK, loader.LoadFromShipmentAndApplicationCode(shipment1.PK, new ZString[] { "CMR" }).PK);
			AssertEquals("Loads correct object", testCusSCAHouse2.PK, loader.LoadFromShipmentAndApplicationCode(shipment1.PK, new ZString[] { "TST" }).PK);
			AssertNull("Not found", loader.LoadFromShipmentAndApplicationCode(shipment1.PK, new ZString[] { "CCC" }));
			AssertNull("Not found", loader.LoadFromShipmentAndApplicationCode(shipment2.PK, new ZString[] { "CMR", "TST", "CCC" }));
			AssertEquals("Loads correct object", testCusSCAHouse2.PK, loader.LoadFromShipmentAndApplicationCode(shipment1.PK, new ZString[] { "CCC", "TST" }).PK);
		}

		public void TestLoadFromBGMReferenceAndApplicationCode()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			var testCusSCAOceanBill1 = Factory.New<TestCusSCAOceanBill>();
			var testCusSCAHouse1 = Factory.New<TestCusSCAHouse>();
			testCusSCAHouse1.CA_CB = testCusSCAOceanBill1.PK;
			testCusSCAOceanBill1.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testCusSCAHouse1.CA_JS = shipment1.PK;
			testCusSCAHouse1.CA_BGMReference = "BGM1";
			var testCusSCAOceanBill2 = Factory.New<TestCusSCAOceanBill>();
			var testCusSCAHouse2 = Factory.New<TestCusSCAHouse>();
			testCusSCAHouse2.CA_CB = testCusSCAOceanBill2.PK;
			testCusSCAOceanBill2.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
			testCusSCAHouse2.CA_JS = shipment1.PK;
			testCusSCAHouse2.CA_BGMReference = "BGM2";
			Factory.Save();
			BaseCusSCAHouse.Loader loader = new BaseCusSCAHouse.Loader(Factory);
			AssertEquals("Loads correct object", testCusSCAHouse1.PK, loader.LoadFromBGMReferenceAndApplicationCode("BGM1", new ZString[] { "CMR" }).PK);
			AssertNull("Not found", loader.LoadFromBGMReferenceAndApplicationCode("BGM1", new ZString[] { "TST" }));
			AssertEquals("Loads correct object", testCusSCAHouse2.PK, loader.LoadFromBGMReferenceAndApplicationCode("BGM2", new ZString[] { "TST" }).PK);
			AssertNull("Not found", loader.LoadFromBGMReferenceAndApplicationCode("BGM2", new ZString[] { "CMR" }));
		}
	}
}
