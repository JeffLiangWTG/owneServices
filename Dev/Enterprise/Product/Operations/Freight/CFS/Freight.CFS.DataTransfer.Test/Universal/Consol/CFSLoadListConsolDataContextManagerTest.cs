using Enterprise.Freight.CFS.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CFSLoadListConsolDataContextManager))]
	class CFSLoadListConsolDataContextManagerTest : ShipmentDataContextManagerTestCase<CFSLoadListConsolDataContextManager, CFSLoadListConsol>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.ACF, RecipientRoleType.DCF }; }
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, Shipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.ACF || recipientRoleType == RecipientRoleType.DCF)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.CFSLoadListConsol, null);
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.ForwardingConsol, null);
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return sampleXML; }
		}

		#region sampleXML

		const string sampleXML = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>L00001004</Key>
        <Type>CFSLoadListConsol</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2015-01-16T13:01:18.343</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>

        <RecipientRoleCollection>
          <RecipientRole Description=""Organization Proxy"">ORP</RecipientRole>
        </RecipientRoleCollection>
      </Workflow>
    </DataContext>

    <AgentsReference></AgentsReference>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CFSReference></CFSReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <JobCosting>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch Name=""BN - AUBNE"">BNE</Branch>
      <Currency Description=""Australian Dollar"">AUD</Currency>
      <Department Name=""Depot/CFS Unpack Sea"">DIS</Department>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff Name=""CargoWise Support"">E</OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <PortOfDischarge Name=""Cairns"">AUCNS</PortOfDischarge>
    <PortOfLoading Name=""Chicago"">USCHI</PortOfLoading>
    <TransportMode Description=""Sea Freight"">SEA</TransportMode>
    <WayBillNumber></WayBillNumber>

    <ContainerCollection Content=""Complete"">
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ArrivalCartageAdvised></ArrivalCartageAdvised>
        <ArrivalCartageComplete></ArrivalCartageComplete>
        <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
        <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
        <ArrivalCartageRef></ArrivalCartageRef>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
        <ArrivalPickupByRail>false</ArrivalPickupByRail>
        <ArrivalSlotDateTime></ArrivalSlotDateTime>
        <ArrivalSlotReference></ArrivalSlotReference>
        <Commodity></Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>CCCC1112223</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality></ContainerQuality>
        <ContainerStatus></ContainerStatus>
        <ContainerType>
          <Code>20GP</Code>
          <Category Description=""Dry Storage"">DRY</Category>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <DeliverySequence>0</DeliverySequence>
        <DepartureCartageAdvised></DepartureCartageAdvised>
        <DepartureCartageComplete></DepartureCartageComplete>
        <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
        <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
        <DepartureCartageRef></DepartureCartageRef>
        <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
        <DepartureDockReceipt></DepartureDockReceipt>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <DepartureSlotReference></DepartureSlotReference>
        <DunnageWeight>0.000</DunnageWeight>
        <EmptyReadyForReturn></EmptyReadyForReturn>
        <EmptyRequired></EmptyRequired>
        <EmptyReturnedBy></EmptyReturnedBy>
		<EmptyReturnRef></EmptyReturnRef>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <FCL_LCL_AIR Description=""Full Container Load"">FCL</FCL_LCL_AIR>
        <FCLAvailable></FCLAvailable>
        <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
        <FCLOnBoardVessel></FCLOnBoardVessel>
        <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
        <FCLStorageCharge>0.0000</FCLStorageCharge>
        <FCLStorageCommences></FCLStorageCommences>
        <FCLStorageDays>0</FCLStorageDays>
        <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <FCLUnloadFromVessel></FCLUnloadFromVessel>
        <FCLWharfGateIn></FCLWharfGateIn>
        <FCLWharfGateOut></FCLWharfGateOut>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency></GoodsValueCurrency>
        <GoodsWeight>0.6</GoodsWeight>
        <GrossWeight>2280.600</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>true</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <OverhangBack>0.000</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0.000</OverhangRight>
        <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
        <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
        <PackDate></PackDate>
        <RefrigGeneratorID></RefrigGeneratorID>
        <ReleaseNum></ReleaseNum>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>2280.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>8.500</TotalHeight>
        <TotalLength>20.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </Container>
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ArrivalCartageAdvised></ArrivalCartageAdvised>
        <ArrivalCartageComplete></ArrivalCartageComplete>
        <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
        <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
        <ArrivalCartageRef></ArrivalCartageRef>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
        <ArrivalPickupByRail>false</ArrivalPickupByRail>
        <ArrivalSlotDateTime></ArrivalSlotDateTime>
        <ArrivalSlotReference></ArrivalSlotReference>
        <Commodity></Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>DDDD1112223</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality></ContainerQuality>
        <ContainerStatus></ContainerStatus>
        <ContainerType>
          <Code>40GP</Code>
          <Category Description=""Dry Storage"">DRY</Category>
          <Description>Forty foot general purpose</Description>
          <ISOCode>42G0</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <DeliverySequence>0</DeliverySequence>
        <DepartureCartageAdvised></DepartureCartageAdvised>
        <DepartureCartageComplete></DepartureCartageComplete>
        <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
        <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
        <DepartureCartageRef></DepartureCartageRef>
        <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
        <DepartureDockReceipt></DepartureDockReceipt>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <DepartureSlotReference></DepartureSlotReference>
        <DunnageWeight>0.000</DunnageWeight>
        <EmptyReadyForReturn></EmptyReadyForReturn>
        <EmptyRequired></EmptyRequired>
        <EmptyReturnedBy></EmptyReturnedBy>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <FCL_LCL_AIR Description=""Full Container Load"">FCL</FCL_LCL_AIR>
        <FCLAvailable></FCLAvailable>
        <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
        <FCLOnBoardVessel></FCLOnBoardVessel>
        <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
        <FCLStorageCharge>0.0000</FCLStorageCharge>
        <FCLStorageCommences></FCLStorageCommences>
        <FCLStorageDays>0</FCLStorageDays>
        <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <FCLUnloadFromVessel></FCLUnloadFromVessel>
        <FCLWharfGateIn></FCLWharfGateIn>
        <FCLWharfGateOut></FCLWharfGateOut>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency></GoodsValueCurrency>
        <GoodsWeight>0.4</GoodsWeight>
        <GrossWeight>3830.400</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>true</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <OverhangBack>0.000</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0.000</OverhangRight>
        <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
        <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
        <PackDate></PackDate>
        <RefrigGeneratorID></RefrigGeneratorID>
        <ReleaseNum></ReleaseNum>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>3830.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>8.500</TotalHeight>
        <TotalLength>40.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AddressShortCode>PST: 98 COMMERCIAL ROAD</AddressShortCode>
        <OrganizationCode>OBMPTY</OrganizationCode>
        <Address1>98 COMMERCIAL ROAD</Address1>
        <Address2>NEWSTEAD  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ABC</City>
        <CompanyName>OBM PTY LTD</CompanyName>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4006</Postcode>
        <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
        <State>QLD</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AddressShortCode>45 ABC ST</AddressShortCode>
        <OrganizationCode>PRIORGLOL</OrganizationCode>
        <Address1>45 ABC ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>LOLOLVILLE</City>
        <CompanyName>PRINCIPAL ORG</CompanyName>
        <Contact>ss</Contact>
        <Country Name=""United States"">US</Country>
        <Email>abc@ab.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port Name=""Lovelock"">USLOL</Port>
        <Postcode>48484</Postcode>
        <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
        <State>NV</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""1-Stop Trading Code"">1ST</Type>
            <CountryOfIssue Name=""Australia"">AU</CountryOfIssue>
            <Value>PRIO</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepotAddress</AddressType>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <OrganizationCode>EDICUS</OrganizationCode>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ALBION</City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port></Port>
        <Postcode></Postcode>
        <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
        <State>QLD</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>H00001004</Key>
            <Type>CFSShipment</Type>
          </DataSource>
        </DataContext>

        <AgentsReference>F</AgentsReference>
        <CartageWaybillNumber></CartageWaybillNumber>
        <GoodsDescription></GoodsDescription>
        <InterimReceiptNumber></InterimReceiptNumber>
        <OuterPacks>5</OuterPacks>
        <OuterPacksPackageType Description=""Package"">PKG</OuterPacksPackageType>
        <PortOfDestination Name=""Cairns"">AUCNS</PortOfDestination>
        <PortOfOrigin Name=""Chicago"">USCHI</PortOfOrigin>
        <ServiceLevel Description=""Standard"">STD</ServiceLevel>
        <ShipmentType Description=""Standard House"">STD</ShipmentType>
        <TotalVolume>0.000</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>1.000</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <TranshipToOtherCFS>true</TranshipToOtherCFS>
        <TransportMode Description=""Sea Freight"">SEA</TransportMode>
        <WarehouseLocation></WarehouseLocation>
        <WayBillNumber></WayBillNumber>

        <LocalProcessing>
          <LCLAvailable></LCLAvailable>
          <LCLStorageCommences></LCLStorageCommences>

          <AdditionalServiceCollection>
            <AdditionalService>
              <ServiceCode Description=""Fumigation"">FUM</ServiceCode>
              <Booked>2015-01-07T15:01:00</Booked>
              <Completed></Completed>
              <Duration></Duration>
              <References></References>
              <ServiceCount>0.000</ServiceCount>
              <ServiceNote></ServiceNote>
            </AdditionalService>
          </AdditionalServiceCollection>
        </LocalProcessing>

        <DateCollection>
          <Date>
            <Type>BookingConfirmed</Type>
            <IsEstimate>false</IsEstimate>
            <Value></Value>
          </Date>
        </DateCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Forwarder</AddressType>
            <AddressShortCode>Pick Up Address</AddressShortCode>
            <OrganizationCode>ACAINT</OrganizationCode>
            <Address1>2ND FLOOR, 482 KINGSFORD SMITH DRIVE</Address1>
            <Address2>HAMILTON                          QLD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>QLD QLD</City>
            <CompanyName>ACA INTERNATIONAL PTY LTD</CompanyName>
            <Country Name=""Australia"">AU</Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port Name=""Brisbane"">AUBNE</Port>
            <Postcode>4007</Postcode>
            <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
            <State>QLD</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""Dakosy Participant Code"">DPC</Type>
                <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
                <Value>545445</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type Description=""Carrier code for Hamburg"">ZAP</Type>
                <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
                <Value>4848484848</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>PST: C/- PHOENIX INTERNAT</AddressShortCode>
            <OrganizationCode>FABCHI</OrganizationCode>
            <Address1>C/- PHOENIX INTERNATIONAL</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>CHICAGO</City>
            <CompanyName>THE FABRIC GROUP</CompanyName>
            <Country Name=""United States"">US</Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port Name=""Chicago"">USCHI</Port>
            <Postcode>0000</Postcode>
            <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
            <State>IL</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
            <OrganizationCode>RAFIKI</OrganizationCode>
            <Address1>6 FLEMING STREET</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>EDGEHILL</City>
            <CompanyName>RAFIKI QLD PTY LTD</CompanyName>
            <Country Name=""Australia"">AU</Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port Name=""Cairns"">AUCNS</Port>
            <Postcode></Postcode>
            <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
            <State>QLD</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
            <OrganizationCode>FABCHI</OrganizationCode>
            <Address1>C/- PHOENIX INTERNATIONAL</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>OOO</City>
            <CompanyName>THE FABRIC GROUP</CompanyName>
            <Country Name=""United States"">US</Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port></Port>
            <Postcode></Postcode>
            <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
            <State>IL</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
            <OrganizationCode>RAFIKI</OrganizationCode>
            <Address1>6 FLEMING STREET</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>EDGEHILL</City>
            <CompanyName>RAFIKI QLD PTY LTD</CompanyName>
            <Country Name=""Australia"">AU</Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port Name=""Cairns"">AUCNS</Port>
            <Postcode></Postcode>
            <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
            <State>QLD</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <Commodity Description=""General"">GEN</Commodity>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>DDDD1112223</ContainerNumber>
            <ContainerPackingOrder>1</ContainerPackingOrder>
            <CountryOfOrigin></CountryOfOrigin>
            <DetailedDescription></DetailedDescription>
            <EndItemNo>0</EndItemNo>
            <GoodsDescription>stuff</GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <Height>0.000</Height>
            <ItemNo>0</ItemNo>
            <Length>0.000</Length>
            <LengthUnit Description=""Meters"">M</LengthUnit>
            <LinePrice>0.0000</LinePrice>
            <LoadingMeters>0.000</LoadingMeters>
            <MarksAndNos>FFF</MarksAndNos>
            <OutturnComment></OutturnComment>
            <OutturnDamagedQty>0</OutturnDamagedQty>
            <OutturnedHeight>0.000</OutturnedHeight>
            <OutturnedLength>0.000</OutturnedLength>
            <OutturnedVolume>0.000</OutturnedVolume>
            <OutturnedWeight>0.000</OutturnedWeight>
            <OutturnedWidth>0.000</OutturnedWidth>
            <OutturnPillagedQty>0</OutturnPillagedQty>
            <OutturnQty>0</OutturnQty>
            <PackQty>2</PackQty>
            <PackType Description=""Package"">PKG</PackType>
            <ReferenceNumber></ReferenceNumber>
            <Volume>0.000</Volume>
            <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
            <Weight>0.400</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <Width>0.000</Width>
          </PackingLine>
          <PackingLine>
            <Commodity Description=""General"">GEN</Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>CCCC1112223</ContainerNumber>
            <ContainerPackingOrder>1</ContainerPackingOrder>
            <CountryOfOrigin></CountryOfOrigin>
            <DetailedDescription></DetailedDescription>
            <EndItemNo>0</EndItemNo>
            <GoodsDescription>stuff</GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <Height>0.000</Height>
            <ItemNo>0</ItemNo>
            <Length>0.000</Length>
            <LengthUnit Description=""Meters"">M</LengthUnit>
            <LinePrice>0.0000</LinePrice>
            <LoadingMeters>0.000</LoadingMeters>
            <MarksAndNos>FFFG</MarksAndNos>
            <OutturnComment></OutturnComment>
            <OutturnDamagedQty>0</OutturnDamagedQty>
            <OutturnedHeight>0.000</OutturnedHeight>
            <OutturnedLength>0.000</OutturnedLength>
            <OutturnedVolume>0.000</OutturnedVolume>
            <OutturnedWeight>0.000</OutturnedWeight>
            <OutturnedWidth>0.000</OutturnedWidth>
            <OutturnPillagedQty>0</OutturnPillagedQty>
            <OutturnQty>0</OutturnQty>
            <PackQty>3</PackQty>
            <PackType Description=""Package"">PKG</PackType>
            <ReferenceNumber></ReferenceNumber>
            <Volume>0.000</Volume>
            <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
            <Weight>0.600</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
            <Width>0.000</Width>
          </PackingLine>
        </PackingLineCollection>

        <ParentShipmentCollection>
          <ParentShipment>
            <DataContext>
              <DataSource>
                <Key>L00001005</Key>
                <Type>CFSLoadListConsol</Type>
              </DataSource>
            </DataContext>

            <AgentsReference></AgentsReference>
            <BookingConfirmationReference></BookingConfirmationReference>
            <CFSReference></CFSReference>
            <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
            <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
            <PortOfLoading Name=""Los Angeles"">USLAX</PortOfLoading>
            <TransportMode Description=""Sea Freight"">SEA</TransportMode>
            <WayBillNumber></WayBillNumber>

            <ContainerCollection Content=""Complete"">
              <Container>
                <AirVentFlow>0.0</AirVentFlow>
                <AirVentFlowRateUnit></AirVentFlowRateUnit>
                <ArrivalCartageAdvised></ArrivalCartageAdvised>
                <ArrivalCartageComplete></ArrivalCartageComplete>
                <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
                <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
                <ArrivalCartageRef></ArrivalCartageRef>
                <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
                <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
                <ArrivalPickupByRail>false</ArrivalPickupByRail>
                <ArrivalSlotDateTime></ArrivalSlotDateTime>
                <ArrivalSlotReference></ArrivalSlotReference>
                <Commodity></Commodity>
                <ContainerCount>1</ContainerCount>
                <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
                <ContainerDetentionDays>0</ContainerDetentionDays>
                <ContainerImportDORelease></ContainerImportDORelease>
                <ContainerNumber>HHHH5555555</ContainerNumber>
                <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
                <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
                <ContainerQuality></ContainerQuality>
                <ContainerStatus></ContainerStatus>
                <ContainerType>
                  <Code>20GP</Code>
                  <Category Description=""Dry Storage"">DRY</Category>
                  <Description>Twenty foot general purpose</Description>
                  <ISOCode>22G0</ISOCode>
                </ContainerType>
                <DeliveryMode>CY/CY</DeliveryMode>
                <DeliverySequence>0</DeliverySequence>
                <DepartureCartageAdvised></DepartureCartageAdvised>
                <DepartureCartageComplete></DepartureCartageComplete>
                <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
                <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
                <DepartureCartageRef></DepartureCartageRef>
                <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
                <DepartureDockReceipt></DepartureDockReceipt>
                <DepartureEstimatedPickup></DepartureEstimatedPickup>
                <DepartureSlotDateTime></DepartureSlotDateTime>
                <DepartureSlotReference></DepartureSlotReference>
                <DunnageWeight>0.000</DunnageWeight>
                <EmptyReadyForReturn></EmptyReadyForReturn>
                <EmptyRequired></EmptyRequired>
                <EmptyReturnedBy></EmptyReturnedBy>
                <ExportDepotCustomsReference></ExportDepotCustomsReference>
                <FCL_LCL_AIR Description=""Full Container Load"">FCL</FCL_LCL_AIR>
                <FCLAvailable></FCLAvailable>
                <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
                <FCLOnBoardVessel></FCLOnBoardVessel>
                <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
                <FCLStorageCharge>0.0000</FCLStorageCharge>
                <FCLStorageCommences></FCLStorageCommences>
                <FCLStorageDays>0</FCLStorageDays>
                <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
                <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
                <FCLUnloadFromVessel></FCLUnloadFromVessel>
                <FCLWharfGateIn></FCLWharfGateIn>
                <FCLWharfGateOut></FCLWharfGateOut>
                <GoodsValue>0.0000</GoodsValue>
                <GoodsValueCurrency></GoodsValueCurrency>
                <GoodsWeight>0.4</GoodsWeight>
                <GrossWeight>2280.400</GrossWeight>
                <HumidityPercent>0</HumidityPercent>
                <IsCFSRegistered>true</IsCFSRegistered>
                <IsControlledAtmosphere>false</IsControlledAtmosphere>
                <IsDamaged>false</IsDamaged>
                <IsEmptyContainer>false</IsEmptyContainer>
                <IsSealOk>true</IsSealOk>
                <IsShipperOwned>false</IsShipperOwned>
                <LCLAvailable></LCLAvailable>
                <LCLStorageCommences></LCLStorageCommences>
                <LCLUnpack></LCLUnpack>
                <LengthUnit Description=""Feet"">FT</LengthUnit>
                <Link>3</Link>
                <OverhangBack>0.000</OverhangBack>
                <OverhangFront>0</OverhangFront>
                <OverhangHeight>0</OverhangHeight>
                <OverhangLeft>0</OverhangLeft>
                <OverhangRight>0.000</OverhangRight>
                <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
                <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
                <PackDate></PackDate>
                <RefrigGeneratorID></RefrigGeneratorID>
                <ReleaseNum></ReleaseNum>
                <Seal></Seal>
                <SecondSeal></SecondSeal>
                <SetPointTemp>0.000</SetPointTemp>
                <SetPointTempUnit>C</SetPointTempUnit>
                <StowagePosition></StowagePosition>
                <TareWeight>2280.000</TareWeight>
                <TempRecorderSerialNo></TempRecorderSerialNo>
                <ThirdSeal></ThirdSeal>
                <TotalHeight>8.500</TotalHeight>
                <TotalLength>20.000</TotalLength>
                <TotalWidth>8.000</TotalWidth>
                <TrainWagonNumber></TrainWagonNumber>
                <UnpackGang></UnpackGang>
                <UnpackShed></UnpackShed>
                <VolumeCapacity>0.000</VolumeCapacity>
                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                <WeightCapacity>0.000</WeightCapacity>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
              </Container>
            </ContainerCollection>

            <OrganizationAddressCollection>
              <OrganizationAddress>
                <AddressType>Forwarder</AddressType>
                <AddressShortCode>PST: PO BOX 818</AddressShortCode>
                <OrganizationCode>EARBNE</OrganizationCode>
                <Address1>PO BOX 818</Address1>
                <Address2>HAMILTON CENTRAL, QLD</Address2>
                <AddressOverride>false</AddressOverride>
                <City>AAAA</City>
                <CompanyName>E A ROCKE</CompanyName>
                <Contact>BRAD PHILLIPS</Contact>
                <Country Name=""Australia"">AU</Country>
                <Email></Email>
                <Fax>3860 4168</Fax>
                <Mobile></Mobile>
                <Phone>3860 4166</Phone>
                <Port Name=""Brisbane"">AUBNE</Port>
                <Postcode>4007</Postcode>
                <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
                <State>QLD</State>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
                <OrganizationCode>MAGSPA</OrganizationCode>
                <Address1>VIA CARPI RAVARINO 108</Address1>
                <Address2>LIMIDI DI</Address2>
                <AddressOverride>false</AddressOverride>
                <City>SOLIERA</City>
                <CompanyName>MAGIC SPA</CompanyName>
                <Country Name=""Italy"">IT</Country>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <Port Name=""Trieste"">ITTRS</Port>
                <Postcode>2000</Postcode>
                <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
                <State>AG</State>

                <RegistrationNumberCollection>
                  <RegistrationNumber>
                    <Type Description=""Standard Carrier Alpha Code"">CCC</Type>
                    <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                    <Value>merr</Value>
                  </RegistrationNumber>
                </RegistrationNumberCollection>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>DepotAddress</AddressType>
                <AddressShortCode>Pick Up Address</AddressShortCode>
                <OrganizationCode>EDICUS</OrganizationCode>
                <Address1>10 HUTCHESON STREET</Address1>
                <Address2></Address2>
                <AddressOverride>false</AddressOverride>
                <City>ALBION</City>
                <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
                <Country Name=""Australia"">AU</Country>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <Port></Port>
                <Postcode></Postcode>
                <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
                <State>QLD</State>
              </OrganizationAddress>
            </OrganizationAddressCollection>

            <TransportLegCollection>
              <TransportLeg>
                <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
                <PortOfLoading Name=""Los Angeles"">USLAX</PortOfLoading>
                <LegOrder>1</LegOrder>
                <ActualArrival></ActualArrival>
                <ActualArrivalInPortOfLoading></ActualArrivalInPortOfLoading>
                <ActualDeparture></ActualDeparture>
                <ArrivalBerth></ArrivalBerth>
                <ArrivalReference></ArrivalReference>
                <Carrier>
                  <AddressType>Carrier</AddressType>
                  <AddressShortCode>Pickup and Delivery Addre</AddressShortCode>
                  <OrganizationCode>MAGSPA</OrganizationCode>
                  <Address1>VIA CARPI RAVARINO 108</Address1>
                  <Address2>LIMIDI DI</Address2>
                  <AddressOverride>false</AddressOverride>
                  <City>SOLIERA</City>
                  <CompanyName>MAGIC SPA</CompanyName>
                  <Country Name=""Italy"">IT</Country>
                  <Email></Email>
                  <Fax></Fax>
                  <Phone></Phone>
                  <Port Name=""Trieste"">ITTRS</Port>
                  <Postcode>2000</Postcode>
                  <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
                  <State>AG</State>

                  <RegistrationNumberCollection>
                    <RegistrationNumber>
                      <Type Description=""Standard Carrier Alpha Code"">CCC</Type>
                      <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                      <Value>merr</Value>
                    </RegistrationNumber>
                  </RegistrationNumberCollection>
                </Carrier>
                <CarrierBookingReference></CarrierBookingReference>
                <CarrierServiceLevel></CarrierServiceLevel>
                <DepartureBerth></DepartureBerth>
                <DepartureReference></DepartureReference>
                <DocumentCutOff></DocumentCutOff>
                <EstimatedArrival>2014-07-29T08:05:00</EstimatedArrival>
                <EstimatedArrivalInPortOfLoading></EstimatedArrivalInPortOfLoading>
                <EstimatedDeparture></EstimatedDeparture>
                <FCLAvailability></FCLAvailability>
                <FCLCutOff></FCLCutOff>
                <FCLReceivalCommences></FCLReceivalCommences>
                <FCLStorage></FCLStorage>
                <HazzardCutOffDate></HazzardCutOffDate>
                <HazzardReceivalCommences></HazzardReceivalCommences>
                <IsCargoOnly>true</IsCargoOnly>
                <LCLAvailability></LCLAvailability>
                <LCLCutOff></LCLCutOff>
                <LCLReceivalCommences></LCLReceivalCommences>
                <LCLStorageDate></LCLStorageDate>
                <LegNotes></LegNotes>
                <LegType>Main</LegType>
                <TransportMode>Sea</TransportMode>
                <VesselLloydsIMO>8318001</VesselLloydsIMO>
                <VesselName>QIU HE</VesselName>
                <VoyageFlightNo>1111</VoyageFlightNo>
              </TransportLeg>
            </TransportLegCollection>
          </ParentShipment>
        </ParentShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge Name=""Cairns"">AUCNS</PortOfDischarge>
        <PortOfLoading Name=""Chicago"">USCHI</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualArrivalInPortOfLoading></ActualArrivalInPortOfLoading>
        <ActualDeparture></ActualDeparture>
        <ArrivalBerth></ArrivalBerth>
        <ArrivalReference></ArrivalReference>
        <Carrier>
          <AddressType>Carrier</AddressType>
          <AddressShortCode>45 ABC ST</AddressShortCode>
          <OrganizationCode>PRIORGLOL</OrganizationCode>
          <Address1>45 ABC ST</Address1>
          <Address2></Address2>
          <AddressOverride>false</AddressOverride>
          <City>LOLOLVILLE</City>
          <CompanyName>PRINCIPAL ORG</CompanyName>
          <Contact>ss</Contact>
          <Country Name=""United States"">US</Country>
          <Email>abc@ab.com</Email>
          <Fax></Fax>
          <Mobile></Mobile>
          <Phone></Phone>
          <Port Name=""Lovelock"">USLOL</Port>
          <Postcode>48484</Postcode>
          <ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
          <State>NV</State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type Description=""1-Stop Trading Code"">1ST</Type>
              <CountryOfIssue Name=""Australia"">AU</CountryOfIssue>
              <Value>PRIO</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </Carrier>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel></CarrierServiceLevel>
        <DepartureBerth></DepartureBerth>
        <DepartureReference></DepartureReference>
        <DocumentCutOff></DocumentCutOff>
        <EstimatedArrival>2014-07-29T08:05:00</EstimatedArrival>
        <EstimatedArrivalInPortOfLoading></EstimatedArrivalInPortOfLoading>
        <EstimatedDeparture></EstimatedDeparture>
        <FCLAvailability></FCLAvailability>
        <FCLCutOff></FCLCutOff>
        <FCLReceivalCommences></FCLReceivalCommences>
        <FCLStorage></FCLStorage>
        <HazzardCutOffDate></HazzardCutOffDate>
        <HazzardReceivalCommences></HazzardReceivalCommences>
        <IsCargoOnly>true</IsCargoOnly>
        <LCLAvailability></LCLAvailability>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LCLStorageDate></LCLStorageDate>
        <LegNotes></LegNotes>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9228758</VesselLloydsIMO>
        <VesselName>CAI YUN HE</VesselName>
        <VoyageFlightNo>RRRRR</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		public void TestPopulateUniversalShipmentXML_ForwardingConsolDataSource_AvoidImportingTwice()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ACF } }
			});
			dataContext.AddDataSource(DataContextType.ForwardingConsol, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				PortOfOrigin = new UNLOCO() { Code = "AUMEL" },
				PortOfDestination = new UNLOCO() { Code = "USCHI" },
				WayBillNumber = "House1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};

			var consolData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				VesselName = "WENDY VESSEL",
				PortOfLoading = new UNLOCO() { Code = "AUSYD" },
				PortOfDischarge = new UNLOCO() { Code = "USCHI" },
				AgentsReference = "AGENTREF001",
				WayBillNumber = "Master1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
			consolData.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { shipmentData }));

			var message = GetQueuedUniversalShipmentMessage(consolData);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HOUSE1111111') from UniversalShipment.
Added Load List from UniversalShipment.
Successfully saved Load List with 1 x CFSShipment.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestPopulateUniversalShipmentXML_CFSLoadListDataSource_AvoidImportingTwice()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.ACF } }
			});
			dataContext.AddDataSource(DataContextType.CFSLoadListConsol, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				PortOfOrigin = new UNLOCO() { Code = "AUMEL" },
				PortOfDestination = new UNLOCO() { Code = "USCHI" },
				WayBillNumber = "House1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};

			var consolData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				VesselName = "WENDY VESSEL",
				PortOfLoading = new UNLOCO() { Code = "AUSYD" },
				PortOfDischarge = new UNLOCO() { Code = "USCHI" },
				AgentsReference = "AGENTREF001",
				WayBillNumber = "Master1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
			consolData.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { shipmentData }));

			var message = GetQueuedUniversalShipmentMessage(consolData);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HOUSE1111111') from UniversalShipment.
Added Load List from UniversalShipment.
Successfully saved Load List with 1 x CFSShipment.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestPopulateUniversalShipmentXML_CFSConsolAndShipmentDataTargets_AvoidImportingTwice()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataTarget(DataContextType.CFSLoadListConsol, null);
			dataContext.AddDataTarget(DataContextType.CFSShipment, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				PortOfOrigin = new UNLOCO() { Code = "AUMEL" },
				PortOfDestination = new UNLOCO() { Code = "USCHI" },
				WayBillNumber = "House1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};

			var consolData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				VesselName = "WENDY VESSEL",
				PortOfLoading = new UNLOCO() { Code = "AUSYD" },
				PortOfDischarge = new UNLOCO() { Code = "USCHI" },
				AgentsReference = "AGENTREF001",
				WayBillNumber = "Master1111111",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
			consolData.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { shipmentData }));

			var message = GetQueuedUniversalShipmentMessage(consolData);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='HOUSE1111111') from UniversalShipment.
Added Load List from UniversalShipment.
Successfully saved Load List with 1 x CFSShipment.
".Trim(), serviceTaskLog.ToString());
		}
	}
}
