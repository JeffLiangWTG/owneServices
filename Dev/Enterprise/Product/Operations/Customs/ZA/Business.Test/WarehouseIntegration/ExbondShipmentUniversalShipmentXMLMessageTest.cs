using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ExbondShipmentUniversalShipmentXMLMessage))]
	class ExbondShipmentUniversalShipmentXMLMessageTest : EDIMessageTest
	{
		public void TestAssignsMessageNumberForReceive()
		{
			var ediMessage = Factory.New<ExbondShipmentUniversalShipmentXMLMessage>();
			Factory.Save();
			AssertEquals("Pre-requisite: Direction must be Receive", EDIInterchange.Direction.Receive, ediMessage.EM_ReceiveTransmit);
			AssertNotEquals("EM_MessageNum", string.Empty, ediMessage.EM_MessageNum);
		}

		public void TestDefaultValues()
		{
			var ediMessage = Factory.New<ExbondShipmentUniversalShipmentXMLMessage>();
			AssertEquals(nameof(ediMessage.EM_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, ediMessage.EM_ApplicationCode);
			AssertEquals(nameof(ediMessage.EM_MessageType), EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
			AssertEquals(nameof(ediMessage.EM_MessageSubType), EDIMessageSubTypeList.Codes.XmlUniversalShipment, ediMessage.EM_MessageSubType);
			AssertEquals(nameof(ediMessage.EM_ReceiveTransmit), EDIInterchange.Direction.Receive, ediMessage.EM_ReceiveTransmit);
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
