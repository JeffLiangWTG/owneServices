using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalShipmentImportHandlerTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;

			var messages = Factory.Load<EDIMessage>(messagesQuery);
			AssertEquals("Precondition: No existing messages", 0, messages.Length);

			var inboundXml =
$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>PO BOX 2623</Address1>
        <City>DUBAI</City>
        <CompanyName>AL TAYER TRENDS LLC</CompanyName>
        <Country>
          <Code>AE</Code>
        </Country>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>1450 REMINGTON ROAD</Address1>
        <City>BOLINGBROOK</City>
        <CompanyName>SCHWARTZ SUPPLY SOURCE</CompanyName>
        <Country>
          <Code>US</Code>
        </Country>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			var handler = new UniversalShipmentImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(inboundXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				AssertContains("<EventType>DIM</EventType>", responseMessageText);
				AssertNotContains("<AttachedDocumentCollection>", responseMessageText);
			}

			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, "SHOWMETHEWUGGETS");
			var reloadedShipment = new BusinessObjectFactory().LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals("SHOWMETHEWUGGETS", reloadedShipment.JS_HouseBill);
			AssertEquals("WINNER WINNER CHICKEN DINNER", reloadedShipment.JS_GoodsDescription);

			messages = Factory.Load<EDIMessage>(messagesQuery);
			AssertEquals("Should be one message for request, response comes later", 1, messages.Length);

			CombineAssertions("Message 1", () =>
			{
				var message = messages[0];
				AssertEquals(ApplicationCodeList.Codes.UniversalDataQuery, message.EM_ApplicationCode);
				AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			});
		}

		public void TestProcess_Processed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.JS_UniqueConsignRef = "S00002244";
			Factory.Save();
			TestProcess_Core(prsXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		public void TestProcess_Error()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.JS_UniqueConsignRef = "S00002244";
			Factory.Save();
			TestProcess_Core(errXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		public void TestProcess_Warning()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.JS_UniqueConsignRef = "S00002244";
			Factory.Save();
			TestProcess_Core(warXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		public void TestProcess_Discarded()
		{
			for (var i = 0; i < 2; i++)
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				((BusinessObject)shipment).FillWithValidTestData();
				shipment.JS_BookingReference = "S00002244";
			}
			Factory.Save();
			TestProcess_Core(dcdXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.ProcessedOK, processingResult.Status);
			});
		}

		public void TestProcess_Rejected()
		{
			TestProcess_Core(rejXML, (processingResult, message) =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);
				AssertNotNull(processingResult);
				AssertEquals(HttpXmlResultStatusList.Codes.Error, processingResult.Status);
			});
		}

		void TestProcess_Core(string xml, Action<IHttpXmlProcessingResult, EDIMessage> assert)
		{
			var handler = new UniversalShipmentImportHandler(new XmlSessionTracker(new SimpleLogger()));
			var request = handler.CreateRequestMessage();
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Recognised, ((EDIMessage)request).EM_Status);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			{
				request.Save();
				var message = (EDIMessage)request;
				message.ReloadSafe();
				assert(processingResult, message);
			}
		}

		#region XML

		readonly string prsXML = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00002244</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
      </Company>
      <EnterpriseID>{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}</EnterpriseID>
      <ServerID>{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}</ServerID>
    </DataContext>
  </Shipment>
</UniversalShipment>";

		readonly string warXML = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00002244</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
      </Company>
      <EnterpriseID>{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}</EnterpriseID>
      <ServerID>{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}</ServerID>
    </DataContext>

    <ActualChargeable>3</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>3</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>3000.000</DocumentedWeight>
    <FreightRate>0</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription>Tables</GoodsDescription>
    <GoodsValue>150000.00</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <InsuranceValue>150000.00</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </Branch>
      <Currency>
        <Code>AUD</Code>
        <Description>Australian Dollar</Description>
      </Currency>
      <Department>
        <Code>FIS</Code>
        <Name>Forwarding Import Sea</Name>
      </Department>
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
    <ManifestedChargeable>3</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>3000.000</ManifestedWeight>
    <NoCopyBills>1</NoCopyBills>
    <NoOriginalBills>0</NoOriginalBills>
    <OuterPacks>600</OuterPacks>
    <OuterPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code>AUMEL</Code>
      <Name>Melbourne</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>EBL</Code>
      <Description>Express Bill of Lading</Description>
    </ReleaseType>
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
    <ShipperCODAmount>0</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>3000.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DemurrageOnDeliveryCharge>0</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryEquipmentNeeded>
        <Code>WUP</Code>
        <Description>Wait for Pack/Unpack</Description>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupEquipmentNeeded>
        <Code>WUP</Code>
        <Description>Wait for Pack/Unpack</Description>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0</PickupLabourCharge>
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
        <Value></Value>
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

    <MilestoneCollection>
      <Milestone>
        <Description>All Import Documents Received</Description>
        <EventCode>AID</EventCode>
        <Sequence>10</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Delivery Cartage Complete/Finalised</Description>
        <EventCode>DCF</EventCode>
        <Sequence>15</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>235 JOHNSON STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>235 JOHNSON STREET</AddressShortCode>
        <City>LOS ANGELES</City>
        <CompanyName>BOOKS GALORE PTY LTD</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>39-384844800</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>BOOGALLAX</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>90019</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CA</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>DOCK DOOR 88</Address1>
        <Address2>332 EDMONDS AVENUE</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>DOCK DOOR 88</AddressShortCode>
        <City>LOS ANGELES</City>
        <CompanyName>BOOKS GALORE PTY LTD</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax>+13108489383</Fax>
        <GovRegNum>39-384844800</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>BOOGALLAX</OrganizationCode>
        <Phone>+13108483839</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>90019</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CA</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Contact>Operations</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>operations.aumel @bbit.com</Email>
        <Fax>+61333864512</Fax>
        <Mobile></Mobile>
        <OrganizationCode>BBITAUMEL</OrganizationCode>
        <Phone>+61333864512</Phone>
        <Port>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </Port>
        <Postcode>3207</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel @bbit.com</Email>
        <Fax>+61333864512</Fax>
        <OrganizationCode>BBITAUMEL</OrganizationCode>
        <Phone>+61333864512</Phone>
        <Port>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </Port>
        <Postcode>3207</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel @bbit.com</Email>
        <Fax>+61333864512</Fax>
        <OrganizationCode>BBITAUMEL</OrganizationCode>
        <Phone>+61333864512</Phone>
        <Port>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </Port>
        <Postcode>3207</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
<PackingLineCollection Content=""Complete"">
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>Tables</GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <ItemNo>0</ItemNo>
        <Length>0</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Metres</Description>
        </LengthUnit>
        <LinePrice>0</LinePrice>
        <Link>1</Link>
        <LoadingMeters>0</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0</OutturnedHeight>
        <OutturnedLength>0</OutturnedLength>
        <OutturnedVolume>0</OutturnedVolume>
        <OutturnedWeight>0</OutturnedWeight>
        <OutturnedWidth>0</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackQty>600</PackQty>
        <PackType>
          <Code>PKG</Code>
          <Description>Package</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>0</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
        </VolumeUnit>
        <Weight>3000.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0</Width>

        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		readonly string dcdXML = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00002244</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
      </Company>
      <EnterpriseID>{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}</EnterpriseID>
      <ServerID>{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}</ServerID>
    </DataContext>
    <GoodsDescription>ChangeMyUniqueIndexForZQuery</GoodsDescription>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		readonly string errXML = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
		<DataTarget>
			<Type>CustomsCommercialInvoice</Type>
		</DataTarget>
		<DataTarget>
			<Type>CustomsDeclaration</Type>
		</DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
      </Company>
      <EnterpriseID>{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}</EnterpriseID>
      <ServerID>{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}</ServerID>
    </DataContext>
    <CommercialInfo>
      <Name>All Invoices</Name>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber>S00001518</InvoiceNumber>
          <Buyer>
            <AddressType>Importer</AddressType>
            <OrganizationCode>AMYUSTNYC</OrganizationCode>
          </Buyer>
          <InvoiceAmount />
          <InvoiceCurrency>
            <Code>USD</Code>
          </InvoiceCurrency>
          <Supplier>
            <AddressType>Supplier</AddressType>
            <OrganizationCode>JARSUCSZG</OrganizationCode>
          </Supplier>
          <Volume>82.55</Volume>
          <VolumeUnit>
            <Code>M3</Code>
            <Description>Cubic Meters</Description>
          </VolumeUnit>
          <Weight>7067.68</Weight>
          <WeightUnit>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </WeightUnit>
          <AddInfoCollection>
            <AddInfo>
              <Key>FirstSale</Key>
              <Value>Y</Value>
            </AddInfo>
          </AddInfoCollection>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <HarmonisedCode>6108.32.0010</HarmonisedCode>
              <InvoiceQuantity>2436</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code>PCS</Code>
                <Description>Pieces</Description>
              </InvoiceQuantityUnit>
              <LinePrice />
              <PartNo>17MW316XLLTG</PartNo>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>2</LineNo>
              <HarmonisedCode>6108.32.0010</HarmonisedCode>
              <InvoiceQuantity>6492</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code>PCS</Code>
                <Description>Pieces</Description>
              </InvoiceQuantityUnit>
              <LinePrice />
              <PartNo>17PN278XLLTG</PartNo>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>3</LineNo>
              <HarmonisedCode>6108.32.0010</HarmonisedCode>
              <InvoiceQuantity>4080</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code>PCS</Code>
                <Description>Pieces</Description>
              </InvoiceQuantityUnit>
              <LinePrice />
              <PartNo>17MW316XLLTG</PartNo>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
  </Shipment>
</UniversalShipment>";

		readonly string rejXML = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>GAR</Code>
        <Name>BAG</Name>
      </Company>
      <EnterpriseID>OOK</EnterpriseID>
      <ServerID>EEK</ServerID>
    </DataContext>
  </Shipment>
</UniversalShipment>";

		#endregion
	}
}
