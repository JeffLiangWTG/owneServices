using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	[UseSnapshotProtection]
	public class eAdaptorStreamedServiceForThreadingTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestUsingDbConnectionInSendStream()
		{
			SendXMLViaStreamedServiceInThread(eAdaptorStreamedServiceTest.UniversalShipmentXML, "http://www.cargowise.com/Schemas/Native#UniversalInterchange");
		}

		public static void SendXMLViaStreamedServiceInThread(string xmlMessageText, string schemaName)
		{
			Exception exception = null;

			TestCaseHelper.ClearTable("EDIMessage");
			TestCaseHelper.ClearTable("EDIInterchange");
			((IFactoryProvider)Env.CurrentCompany).Factory.ThreadSentry.RelinquishThreadOwnership();

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var sendID = Guid.NewGuid();
					var messageID = Guid.NewGuid();

					var messageEncoded = new UTF8Encoding().GetBytes(xmlMessageText);
					using (var messageStream = new MemoryStream(messageEncoded))
					using (var compressedMessageStream = messageStream.CompressAndEncode())
					{
						var messageStreamWrapper = new eHubGatewayMessage()
						{
							ApplicationCode = "NotUsedForXML",
							ClientID = GlbCompany.CurrentCompany.LicenceKeyIdentifier,
							EmailSubject = "Have some XML",
							FileName = "XmlFile.xml",
							MessageTrackingID = messageID,
							SchemaType = MessageSchemaType.Xml,
							SchemaName = schemaName,
							MessageStream = compressedMessageStream,
						};

						var sentStreamWrapper = new SendStreamRequest(sendID, new[] { messageStreamWrapper });

						var service = new eAdaptorStreamedServiceTest.eAdaptorStreamedServiceForTesting();
						AssertEquals("service.Ping()", true, service.Ping());

						try
						{
							service.SendStream(sentStreamWrapper);
						}
						catch (Exception ex)
						{
							exception = ex;
						}
					}
				}
			});

			thread.Start();
			thread.Join();

			((IFactoryProvider)Env.CurrentCompany).Factory.ThreadSentry.TakeThreadOwnership();

			if (exception != null)
			{
				throw exception;
			}
		}
	}

	public class eAdaptorStreamedServiceTest : TestCaseWithFactory
	{
		public void TestUniversalInterchangeWorksFineWithNativeNamespace()
		{
			SendXMLViaStreamedService(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Native#UniversalInterchange");

			AssertUniversalShipmentImportResults();
		}

		public void TestUniversalInterchangeWorksFineWithUnversionedUniversalNamespace()
		{
			SendXMLViaStreamedService(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal#UniversalInterchange");

			AssertUniversalShipmentImportResults();
		}

		public void TestUniversalInterchangeWorksFineWithVersionedUniversalNamespace()
		{
			SendXMLViaStreamedService(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange");

			AssertUniversalShipmentImportResults();
		}

		[ExpectNoExceptions]
		public void TestXmlInterchange_NotThrowCannotUseClosedStream()
		{
			SendXMLViaStreamedService(XmlInterchange, "http://www.edi.com.au/EnterpriseService/#XmlInterchange");
		}

		[ExpectException(typeof(FaultException))]
		public void TestCommunication_FaultExceptionIsThrown()
		{
			SendXMLViaStreamedService(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", "ABCDEFGHI");
		}

		public void TestCommunication_NullReferenceExceptionIsHandledAndErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new NullReferenceException("Test"));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertEquals("NullReferenceException reported", true, ErrorReporter.LastExceptionReported is NullReferenceException);
			ErrorReporter.Clear();
		}

		public void TestCommunication_TransactionExceptionIsHandledAndErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new TransactionException("Test"));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertEquals("TransactionException reported", true, ErrorReporter.LastExceptionReported is TransactionException);
		}

		public void TestCommunication_ZSaveExceptionIsHandledAndErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new ZSaveException(new ZDataException(MakeSqlException(), (Factory.New<DummyBusinessObject>() as INeedRow).Row, Db.Connection), Factory));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertEquals("ZSaveException reported", true, ErrorReporter.LastExceptionReported is ZSaveException);
		}

		internal SqlException MakeSqlException()
		{
			SqlException exception = null;
			try
			{
				SqlConnection conn = new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1");
				conn.Open();
			}
			catch (SqlException ex)
			{
				exception = ex;
			}
			return (exception);
		}

		public void TestCommunication_SqlExceptionIsHandledAndErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(MakeSqlException());
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertEquals("SqlException reported", true, ErrorReporter.LastExceptionReported is SqlException);
		}

		public void TestCommunication_HttpExceptionClientAbortedConnectionIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new HttpException("test", unchecked((int)0x80070040)));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestCommunication_HttpExceptionInnerCOMExceptionIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new HttpException("test", new COMException()));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestCommunication_HttpExceptionWithMessageBelowIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new HttpException("The client is disconnected because the underlying request has been completed."));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestCommunication_InvalidOperationExceptionTimeoutExpiredIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new InvalidOperationException("Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool."));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestCommunication_InvalidOperationExceptionInactiveConnectionIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new InvalidOperationException("The transaction no longer has an active connection."));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestMessageHandlerExceptionIsHandledAndNotErrorReported()
		{
			var sentStreamWrapper = GetTestSendStreamRequest(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", null);
			var service = new eAdaptorStreamedServiceForExceptionHandlingTesting(new MessageHandlerException("RecipientID should be 9 characters in length"));
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestRetrieveDataInUniversalXML()
		{
			var response = RetrieveXMLViaStreamedService(UniversalShipmentXML, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange");
			foreach (var message in response.Messages)
			{
				var memoryStream = new MemoryStream();
				message.MessageStream.DecodeAndDecompress().CopyTo(memoryStream);
				var messageDecoded = new UTF8Encoding().GetString(memoryStream.ToArray());
				message.Dispose();
				AssertEquals(UniversalShipmentXML, messageDecoded);
				AssertEquals("MUSE", message.ClientID);
			}
		}

		public void TestSendXMLViaStreamedServiceInvalidSendStreamRequestErrorHandling()
		{
			var sentStreamWrapper = new SendStreamRequest(Guid.NewGuid(), null);
			var service = new eAdaptorStreamedServiceForTesting();
			AssertExceptionThrown<FaultException>(() => service.SendStream(sentStreamWrapper));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		#region string XmlInterchange

		public static string XmlInterchange = @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <Payload>
    <Consols>
      <Consol>
        <ConsolDetail>
        	<PlannedLegs>
            <PlannedLeg>
              <TransportMode>SEA</TransportMode>
              <PortOfLoading>
                <Port>BGVAR</Port>
                <EstimatedDateTime>2016-04-10T00:00:00</EstimatedDateTime>
                <ActualDateTime>2016-04-18T22:41:00</ActualDateTime>
              </PortOfLoading>
              <PortOfDischarge>
                <Port>GRPIR</Port>
                <EstimatedDateTime>2016-04-14T00:00:00</EstimatedDateTime>
                <ActualDateTime>2016-04-26T00:00:00</ActualDateTime>
              </PortOfDischarge>
              <TransportType>MainVessel</TransportType>
              <LegOrderNumber>1</LegOrderNumber>
              <Vessel>
                <ETD>2016-04-10T00:00:00</ETD>
                <ETA>2016-04-14T00:00:00</ETA>
                <ATD>2016-04-18T22:41:00</ATD>
                <ATA>2016-04-26T00:00:00</ATA>
                <VesselName>MICHIGAN TRADER</VesselName>
                <VoyageNo>013S /S</VoyageNo>
              </Vessel>
            </PlannedLeg>
            <PlannedLeg>
              <TransportMode>SEA</TransportMode>
              <PortOfLoading>
                <Port>GRPIR</Port>
                <EstimatedDateTime>2016-05-07T00:00:00</EstimatedDateTime>
                <ActualDateTime>2016-05-10T07:38:00</ActualDateTime>
              </PortOfLoading>
              <PortOfDischarge>
                <Port>CNNGB</Port>
                <EstimatedDateTime>2016-06-01T00:00:00</EstimatedDateTime>
                <ActualDateTime>2016-06-02T00:00:00</ActualDateTime>
              </PortOfDischarge>
              <TransportType>Other</TransportType>
              <LegOrderNumber>2</LegOrderNumber>
              <Vessel>
                <ETD>2016-05-07T00:00:00</ETD>
                <ETA>2016-06-01T00:00:00</ETA>
                <ATD>2016-05-10T07:38:00</ATD>
                <ATA>2016-06-02T00:00:00</ATA>
                <VesselName>YM UNIFORM</VesselName>
                <VoyageNo>041 /E</VoyageNo>
              </Vessel>
            </PlannedLeg>
            <PlannedLeg>
              <TransportMode>SEA</TransportMode>
              <PortOfLoading>
                <Port>CNNGB</Port>
              </PortOfLoading>
              <PortOfDischarge>
                <Port>CNSHA</Port>
                <EstimatedDateTime>2016-06-01T00:00:00</EstimatedDateTime>
              </PortOfDischarge>
              <TransportType>Other</TransportType>
              <LegOrderNumber>3</LegOrderNumber>
              <Vessel>
                <ETA>2016-06-01T00:00:00</ETA>
                <VesselName>EVER LIVIN</VesselName>
              </Vessel>
            </PlannedLeg>
        	</PlannedLegs>
          	<AgentReference>CBGSE16000426</AgentReference>
          	<ExternalAgentReference>CBGSE16000426</ExternalAgentReference>
        </ConsolDetail>
      </Consol>
    </Consols>
  </Payload>
</XmlInterchange>";

		#endregion

		#region string universalShipmentXML

		public const string UniversalShipmentXML = @"<?xml version=""1.0"" encoding=""utf-8""?><UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>HYEDUSDAT</RecipientID>
  </Header>
  <Body>
	  <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001117</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
	<DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001117</Key>
	</DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DUS</Code>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Name>U.S. Demo Company</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2012-01-10T10:47:22.067</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
    </DataContext>

    <ActualChargeable>1500.000</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>1500.000</DocumentedChargeable>
    <DocumentedVolume>0.000</DocumentedVolume>
    <DocumentedWeight>1500.000</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>USD</Code>
      <Description>United States of America, Dollars</Description>
    </GoodsValueCurrency>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <InsuranceValue>0.0000</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>USD</Code>
      <Description>United States of America, Dollars</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsBuyersConsolMaster>false</IsBuyersConsolMaster>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsCoload>false</IsCoload>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsSellersConsolMaster>false</IsSellersConsolMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <JobCosting>
      <Branch>
        <Code>CHI</Code>
        <Name>Chicago</Name>
      </Branch>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Currency>
        <Code>USD</Code>
        <Description>United States of America, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <ManifestedChargeable>1500.000</ManifestedChargeable>
    <ManifestedVolume>0.000</ManifestedVolume>
    <ManifestedWeight>1500.000</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code>AUALX</Code>
      <Name>Alexandria</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USORD</Code>
      <Name>O'Hare Apt/Chicago</Name>
    </PortOfOrigin>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>1500.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber>S00001117</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryEquipmentNeeded>
        <Code>PSL</Code>
        <Description>Premise Supplies Lift</Description>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupEquipmentNeeded>
        <Code>PSL</Code>
        <Description>Premise Supplies Lift</Description>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2011-07-16T13:00:00</Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AddressShortCode>Sydney Office</AddressShortCode>
        <OrganizationCode>SYDOFFALX</OrganizationCode>
        <Address1>Sydney Office</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>Alexandria</City>
        <CompanyName>Sydney Office</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUALX</Code>
          <Name>Alexandria</Name>
        </Port>
        <Postcode>2015</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AddressShortCode>Sydney Office</AddressShortCode>
        <OrganizationCode>SYDOFFALX</OrganizationCode>
        <Address1>Sydney Office</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>Alexandria</City>
        <CompanyName>Sydney Office</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUALX</Code>
          <Name>Alexandria</Name>
        </Port>
        <Postcode>2015</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AddressShortCode>1515 E. WOODFIELD RD</AddressShortCode>
        <OrganizationCode>CARGOWORD</OrganizationCode>
        <Address1>1515 E. WOODFIELD RD</Address1>
        <Address2>SUITE 820</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SCHAUMBURG</City>
        <CompanyName>CARGOWISE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>12-123345600</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Phone>847-364-5600</Phone>
        <Port>
          <Code>USORD</Code>
          <Name>O'Hare Apt/Chicago</Name>
        </Port>
        <Postcode>60173</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AddressShortCode>1515 E. WOODFIELD RD</AddressShortCode>
        <OrganizationCode>CARGOWORD</OrganizationCode>
        <Address1>1515 E. WOODFIELD RD</Address1>
        <Address2>SUITE 820</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SCHAUMBURG</City>
        <CompanyName>CARGOWISE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>12-123345600</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Phone>847-364-5600</Phone>
        <Port>
          <Code>USORD</Code>
          <Name>O'Hare Apt/Chicago</Name>
        </Port>
        <Postcode>60173</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>LocalClient</AddressType>
        <AddressShortCode>1515 E. WOODFIELD RD</AddressShortCode>
        <OrganizationCode>CARGOWORD</OrganizationCode>
        <Address1>1515 E. WOODFIELD RD</Address1>
        <Address2>SUITE 820</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SCHAUMBURG</City>
        <CompanyName>CARGOWISE</CompanyName>
        <Contact>Mike</Contact>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>mike.sverdlov@cargowise.com</Email>
        <Fax></Fax>
        <GovRegNum>12-123345600</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone>847-364-5600</Phone>
        <Port>
          <Code>USORD</Code>
          <Name>O'Hare Apt/Chicago</Name>
        </Port>
        <Postcode>60173</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerNumber></ContainerNumber>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0.000</Height>
        <ItemNo>0</ItemNo>
        <Length>0.000</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0.0000</LinePrice>
        <LoadingMeters>0.000</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0.000</OutturnedHeight>
        <OutturnedLength>0.000</OutturnedLength>
        <OutturnedVolume>0.000</OutturnedVolume>
        <OutturnedWeight>0.000</OutturnedWeight>
        <OutturnedWidth>0.000</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>0.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>1500.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0.000</Width>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>
";

		#endregion

		void AssertUniversalShipmentImportResults()
		{
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			CombineAssertions(delegate
			{
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_From", "MUSE", interchange.EI_From);
				AssertEquals("interchange.EI_To", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			});

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("message.EM_TransportType", interchange.EI_TransportType, message.EM_TransportType);

				AssertContains("message.EM_MessageText", "<Key>S00001117</Key>", message.EM_MessageText);
			});
		}

		#region Implementation

		static void SendXMLViaStreamedService(string xmlMessageText, string schemaName, string clientId = null)
		{
			TestCaseHelper.ClearTable("EDIMessage");
			TestCaseHelper.ClearTable("EDIInterchange");

			var messageEncoded = new UTF8Encoding().GetBytes(xmlMessageText);
			using (var messageStream = new MemoryStream(messageEncoded))
			using (var compressedMessageStream = messageStream.CompressAndEncode())
			{
				var sentStreamWrapper = GetTestSendStreamRequest(xmlMessageText, schemaName, compressedMessageStream, clientId);

				var service = new eAdaptorStreamedServiceForTesting();
				AssertEquals("service.Ping()", true, service.Ping());

				try
				{
					service.SendStream(sentStreamWrapper);
				}
				catch (Exception)
				{
					ErrorReporter.Clear();
					throw;
				}
			}
		}

		static RetrieveStreamResponse RetrieveXMLViaStreamedService(string xmlMessageText, string schemaName)
		{
			RetrieveStreamResponse result;
			TestCaseHelper.ClearTable("EDIMessage");
			TestCaseHelper.ClearTable("EDIInterchange");

			var messageEncoded = new UTF8Encoding().GetBytes(xmlMessageText);
			using (var messageStream = new MemoryStream(messageEncoded))
			using (var compressedMessageStream = messageStream.CompressAndEncode())
			{
				var sentStreamWrapper = GetTestSendStreamRequest(xmlMessageText, schemaName, compressedMessageStream);

				var service = new eAdaptorStreamedServiceForTesting();
				AssertEquals("service.Ping()", true, service.Ping());

				result = service.ProcessStream(sentStreamWrapper);

				return result;
			}
		}

		static SendStreamRequest GetTestSendStreamRequest(string xmlMessageText, string schemaName, Stream compressedMessageStream, string clientId = null)
		{
			var sendID = Guid.NewGuid();
			var messageID = Guid.NewGuid();
			var messageStreamWrapper = new eHubGatewayMessage()
			{
				ApplicationCode = "NotUsedForXML",
				ClientID = clientId ?? GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				EmailSubject = "Have some XML",
				FileName = "XmlFile.xml",
				MessageTrackingID = messageID,
				SchemaType = MessageSchemaType.Xml,
				SchemaName = schemaName,
				MessageStream = compressedMessageStream,
			};

			return new SendStreamRequest(sendID, new[] { messageStreamWrapper });
		}

		public class eAdaptorStreamedServiceForTesting : eAdapterStreamedService
		{
			protected override string GetSenderID()
			{
				return "MUSE";
			}
		}

		public class eAdaptorStreamedServiceForExceptionHandlingTesting : eAdapterStreamedService
		{
			public Exception ex;

			public eAdaptorStreamedServiceForExceptionHandlingTesting(Exception exception)
			{
				ex = exception;
			}
			protected override string GetSenderID()
			{
				throw ex;
			}
		}

		#endregion
	}
}
