using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer.Testing
{
	sealed class PortMessagingImporterTest : TestCaseWithFactory
	{
		public void TestExportToDakosy()
		{
			using (Factory.AddDisposableService())
			{
				var consol = Factory.New<ForwardingConsol>();

				var importer = new PortMessagingImporter();
				importer.Import(consol, PortMessagingManager.MessageType.PortOrderWithHDS, "AAA");
				Factory.Save();

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobConsol", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", consol.PK, message.EM_LinkUniqueID);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);
				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "DAKOSYHAM", interchange.EI_To);
					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		public void TestExportToDakosy_CustomType()
		{
			using (Factory.AddDisposableService())
			{
				var consol = Factory.New<CustomConsol>();

				var importer = new PortMessagingImporter();
				AssertNoExceptionThrown(() => importer.Import(consol, PortMessagingManager.MessageType.PortOrderWithHDS, "AAA"));

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);
				Factory.Save();
			}
		}

		class CustomConsol : ForwardingConsol
		{
			public CustomConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}
	}
}
