using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalShipmentXmlWriterTest : TestCaseWithFactory
	{
		public void TestSendUniversalShipmentXml()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				Factory.Save();

				var logger = new TestLogger();
				var processor = new UniversalShipmentXmlWriter(new DummyShipmentDataObjectWriter(), shipmentBO, "DAKOSYHAM", string.Format("S00001010_{0:yyyyMMddHHmmssZ}.txt", ZDateTime.UtcNow));

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				processor.Process(logger);
				Factory.Save();
			}
			AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
				, @"".Trim()
				, logger.GetAllLogsAsString());

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
					AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", shipmentBO.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", ExpectedUniversalShipmentMessage.Trim(), message.EM_MessageText);

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
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "DAKOSYHAM", interchange.EI_To);

					AssertMultilineASCIIEquals("interchange.EI_BodyText", ExpectedUniversalShipmentInterchange.Trim(), interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		public void TestSendUniversalShipmentXml_JobNumberForWorkflow()
		{
			TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_JobType = "BKG";
			consolidation.KB_ParentTableCode = shipmentBO.TablePrefix;
			consolidation.KB_ParentID = shipmentBO.PK;

			var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			booking.KM_KB_Booking = consolidation.PK;
			booking.KM_JobID = "TB00000001";

			Factory.Save();

			var logger = new TestLogger();
			var processor = new UniversalShipmentXmlWriter(new DummyShipmentDataObjectWriter(), (BusinessObject)booking, "DAKOSYHAM", string.Format("(*JobNumber*).txt", ZDateTime.UtcNow));

			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				processor.Process(logger);
				Factory.Save();
			}
			AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
				, @"".Trim()
				, logger.GetAllLogsAsString());

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
				AssertEquals("message.EM_LinkTable", "DtbBooking", message.EM_LinkTable);
				AssertEquals("message.EM_LinkUniqueID", booking.PK, message.EM_LinkUniqueID);

				AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
			});

			var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			AssertEquals("HeaderText", "<EDIDelivery><FileName>TB00000001.txt</FileName><EmailSubject></EmailSubject></EDIDelivery>", interchanges[0].EI_HeaderText);
		}

		#region const string ExpectedUniversalShipmentMessage

		const string ExpectedUniversalShipmentMessage = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
";

		const string ExpectedUniversalShipmentInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>DAKOSYHAM</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>";
		#endregion

		#region Implementation

		class DummyShipmentDataObjectWriter : ITopLevelDataObjectWriter
		{
			ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
			{
				var dataContext = DataContextFactory.New(sourceBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11);
				var dataSource = dataContext.DataSourceCollection.First();
				dataSource.Type = "ForwardingConsol";
				dataSource.Key = "C00001000";
				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
				};
				return universalShipment;
			}

			DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
			{
				get { return DataContextType.ForwardingShipment; }
			}

			ZString ITopLevelDataObjectWriter.EDIMessageSubType
			{
				get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
			}

			ZString ITopLevelDataObjectWriter.RootElementName
			{
				get { return "UniversalShipment"; }
			}
		}

		#endregion
	}
}
