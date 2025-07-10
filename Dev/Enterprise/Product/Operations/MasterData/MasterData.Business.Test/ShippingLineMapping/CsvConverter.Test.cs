using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterData.Business.Tests
{
	public class CsvConverterTest : TestCaseWithFactory
	{
		public void TestConvertToXMl()
		{
			//Arrange

			string[] data = new string[] { "Y,,TestCarrierName,AAAA,BBBB,CCCC,DDDD,EEEE,FFF,GGG,Y,Y,," };

			//Act

			var xmlResult = CsvConverter.ConvertToXml(data);

			//Assert

			var carrier = xmlResult.Element("Carrier");

			AssertNotNullOrEmpty(carrier.Element("RSL_PK").Value);

			AssertEquals("1", carrier.Element("RSL_IsSystem").Value);
			AssertEquals("1", carrier.Element("RSL_IsActive").Value);
			AssertEquals("1", carrier.Element("RSL_IsNVO").Value);
			AssertEquals("TestCarrierName", carrier.Element("RSL_CarrierName").Value);
			AssertEquals("AAAA", carrier.Element("RSL_StandardCarrierAlphaCode").Value);
			AssertEquals("BBBB", carrier.Element("RSL_CargoWiseOneCode").Value);
			AssertEquals("1", carrier.Element("RSL_OceanCarrierMessagingAvailable").Value);
			AssertEquals("1", carrier.Element("RSL_GlobalSailingScheduleAvailable").Value);
			AssertEquals("1", carrier.Element("RSL_ContainerAutomationAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_CargoSphereRatesAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_InvoiceAvailable").Value);
		}

		public void TestConvertToXMlWithEmptyData()
		{
			//Arrange

			string[] data = new string[] { ",,,,,,,,,,,,,,,,,,,,,,,," };

			//Act

			var xmlResult = CsvConverter.ConvertToXml(data);

			//Assert

			var carrier = xmlResult.Element("Carrier");

			AssertNotNullOrEmpty(carrier.Element("RSL_PK").Value);

			AssertEquals("1", carrier.Element("RSL_IsSystem").Value);
			AssertEquals("1", carrier.Element("RSL_IsActive").Value);
			AssertEquals("0", carrier.Element("RSL_IsNVO").Value);

			AssertNullOrEmpty(carrier.Element("RSL_CarrierName").Value);

			AssertNotNullOrEmpty(carrier.Element("RSL_StandardCarrierAlphaCode").Value);
			AssertNotNullOrEmpty(carrier.Element("RSL_CargoWiseOneCode").Value);

			AssertEquals("0", carrier.Element("RSL_OceanCarrierMessagingAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_GlobalSailingScheduleAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_ContainerAutomationAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_CargoSphereRatesAvailable").Value);
			AssertEquals("0", carrier.Element("RSL_InvoiceAvailable").Value);
		}
	}
}
