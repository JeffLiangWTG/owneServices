using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	sealed class CusSCAHouseDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReaderTrimsConsignorState()
		{
			var logger = new DummyLogger();
			var shipment = CusSCAOceanBillDataObjectReaderTest.ReadXMLIntoShipment(logger, TestXmlReader.GetFileContents("OceanBillShipment1"));

			BusinessObject targetBO = null;
			ITopLevelDataObjectReader reader = new CusSCAOceanBillDataObjectReader(shipment, null, logger, Factory);
			reader.ReadIntoBusinessObject(ref targetBO);
			Factory.SaveForTesting();
			var oceanBill = targetBO as CusSCAOceanBill;
			AssertNotNull("Ocean Bill was created", oceanBill);

			var houseBills = CusSCADataObjectHelper.LoadHouseBills<CusSCAHouse>(oceanBill, Factory.BOFactory).OrderBy(x => x.CA_HouseBill);
			var houseBill = houseBills.ElementAt(0);
			AssertEquals("It should take left 3 characters if the valueSource is too long.", "PIE", houseBill.CA_ConsignorState);
		}
	}
}
