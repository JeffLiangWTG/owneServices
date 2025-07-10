using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestDoNotCreatePackingLineWithDeletingContainer()
		{
			#region XML String
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>BillOfLading</Type>
          <Key>V00001319</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DIE</Code>
        <Country>
          <Code>IE</Code>
          <Name>Ireland</Name>
        </Country>
        <Name>IE Demo Company</Name>
      </Company>
      <DataProvider>HYEUA3DIE</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>DUB</Code>
        <Name>Cargowise EDI - Dublin (IE)</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>UA3</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2014-02-04T00:57:05.52</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <BookingConfirmationReference></BookingConfirmationReference>
    <CFSReference></CFSReference>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsDescription>TRAILER PART</GoodsDescription>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsShipping>true</IsShipping>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>1</OuterPacks>
    <OuterPacksPackageType>
      <Code>PCE</Code>
      <Description>Piece</Description>
    </OuterPacksPackageType>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PortOfDestination>
      <Code>IEDUB</Code>
      <Name>Dublin</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>CNGUA</Code>
      <Name>Guangdong</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>OBR</Code>
      <Description>Ocean bill required at destination</Description>
    </ReleaseType>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard Service</Description>
    </ServiceLevel>
    <ShipmentStatus>
      <Code>CNF</Code>
      <Description>Confirmed</Description>
    </ShipmentStatus>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <TotalVolume>1.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>C1121212</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit>
          <Code></Code>
        </AirVentFlowRateUnit>
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
        <Commodity>
          <Code></Code>
        </Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>1212</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality>
          <Code></Code>
        </ContainerQuality>
        <ContainerStatus>
          <Code></Code>
        </ContainerStatus>
        <ContainerType>
          <Code>1000</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>GEN PURPOSE CONT</Description>
          <ISOCode>1000</ISOCode>
        </ContainerType>
        <DeliveryMode></DeliveryMode>
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
        <FCL_LCL_AIR>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </FCL_LCL_AIR>
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
        <GoodsValueCurrency>
          <Code></Code>
        </GoodsValueCurrency>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>0.000</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit>
          <Code>FT</Code>
          <Description>Feet</Description>
        </LengthUnit>
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
        <TareWeight>0.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>0.000</TotalHeight>
        <TotalLength>0.000</TotalLength>
        <TotalWidth>0.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
        </VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-02-03T00:00:00</Value>
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

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>SOMETHING</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>IE</City>
        <CompanyName>AXES ANS SUSPEND</CompanyName>
        <Contact></Contact>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>SOMETHING</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>IE</City>
        <CompanyName>AXES ANS SUSPEND</CompanyName>
        <Contact></Contact>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>MANUFACTURING CO., LTD</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>LELIU, SHUDE</City>
        <CompanyName>GUANGDONG FUWA ENGINEER</CompanyName>
        <Contact></Contact>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>MANUFACTURING CO., LTD</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>LELIU, SHUDE</City>
        <CompanyName>GUANGDONG FUWA ENGINEER</CompanyName>
        <Contact></Contact>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Principal</AddressType>
        <AddressShortCode>LEVEL 3</AddressShortCode>
        <OrganizationCode>AAASHI_WW</OrganizationCode>
        <Address1>LEVEL 3</Address1>
        <Address2>184 BOURKE ROAD</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ALEXANDRIA</City>
        <CompanyName>AAA SHIPPING LINE</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>12003709608</GovRegNum>
        <GovRegNumType>
          <Code>ABN</Code>
          <Description>Australian Business Number (VAT Reg</Description>
        </GovRegNumType>
        <Phone></Phone>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>2015</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>1ST</Code>
              <Description>1-Stop Trading Code</Description>
            </Type>
            <Value>aaa</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CMP</Code>
              <Description>Customs Manifest Provider Code</Description>
            </Type>
            <Value>C008493924</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Standard Carrier Alpha Code</Description>
            </Type>
            <Value>test</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>C003709608</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>1212</ContainerNumber>
        <ContainerPackingOrder>1</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription>TRAILER PART</DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>1.000</Height>
        <ItemNo>0</ItemNo>
        <Length>1.000</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Metres</Description>
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
        <PackQty>1</PackQty>
        <PackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>1.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
        </VolumeUnit>
        <Weight>0.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>1.000</Width>

        <CustomizedFieldCollection>
          <CustomizedField>
            <Key>CONTOUR</Key>
            <DataType>String</DataType>
            <Value></Value>
          </CustomizedField>
        </CustomizedFieldCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

			#endregion
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var message = GetQueuedUniversalShipmentMessage(xml);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				AssertNoExceptionThrown(() => manager.Process(message));
			}
		}

		public void TestDoNotCreateDuplicatePackingGroups()
		{
			#region XML String
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
		<DataContext>
      <DataTargetCollection>
	  <DataTarget>
	  <Type>CustomsDeclaration</Type>
	  </DataTarget>
	  </DataTargetCollection>


      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>HOU</Code>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Name>CEVA International Inc.</Name>
      </Company>
      <DataProvider>CEVTSTHOU</DataProvider>
      <EnterpriseID>CEV</EnterpriseID>
      <EventBranch>
        <Code>125</Code>
        <Name>ATL</Name>
      </EventBranch>
      <EventDepartment>
        <Code>ALL</Code>
        <Name>All Modes</Name>
      </EventDepartment>
      

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
	
	<AgentsReference></AgentsReference>
	<Branch>
      <Code>157</Code>
      <Name>LBC</Name>
    </Branch>
	<ConsolidatedCargoStatus>
      <Code></Code>
    </ConsolidatedCargoStatus>
	<ContainerCount>1</ContainerCount>
	<CustomsBroker>
      <Code>BAL</Code>
      <Name>Brandon Allen</Name>
    </CustomsBroker>
	<CustomsContainerMode>
	  <Code>CNT</Code>
      <Description>Containerized (Trans. Mode: 11, 21,</Description>
    </CustomsContainerMode>
	
	<LloydsIMO></LloydsIMO>
    <MergeBy>
      <Code>NON</Code>
      <Description>No Merge</Description>
    </MergeBy>
    <MessageStatus>
      <Code>CEO</Code>
      <Description>Clear Entry Summary Add</Description>
    </MessageStatus>
    <MessageSubType>
      <Code></Code>
    </MessageSubType>
    <MessageType>
      <Code>IMP</Code>
      <Description>Import</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>ACE</Code>
      <Description>ACE ABI</Description>
    </MessagingApplicationCode>
    <OperationalStatus>
      <Code></Code>
    </OperationalStatus>
    <OwnerRef></OwnerRef>
	<JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>0.0000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>011</Code>
        <Name>011 Damco Customs Services Inc.</Name>
      </Branch>
      <Currency>
        <Code>USD</Code>
        <Description>United States Dollar</Description>
      </Currency>
      <Department>
        <Code>CIS</Code>
        <Name>Clearance Import Sea</Name>
      </Department>
      <HomeBranch>
        <Code>011</Code>
        <Name>011 Damco Customs Services Inc.</Name>
      </HomeBranch>
      <LocalClientRevenue>64.5000</LocalClientRevenue>
      <OperationsStaff>
        <Code>BAL</Code>
        <Name>Brandon Allen</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0.0000</TotalAccrual>
      <TotalCost>0.0000</TotalCost>
      <TotalJobProfit>64.5000</TotalJobProfit>
      <TotalRevenue>64.5000</TotalRevenue>
      <TotalWIP>0.0000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>0.0000</WIPRecognized>

      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>011</Code>
            <Name>011 Damco Customs Services Inc.</Name>
          </Branch>
          <ChargeCode>
            <Code>201</Code>
            <Description>CUSTOMS DEFERRED</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>BRK</Code>
            <Description>Customs Brokerage / Agency / Entry Fees</Description>
          </ChargeCodeGroup>
          <CostAPInvoiceNumber>54487765</CostAPInvoiceNumber>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0.0000</CostOSGSTVATAmount>
          <Creditor>
            <Type>Organization</Type>
            <Key>4156979</Key>
          </Creditor>
          <Debtor>
            <Type>Organization</Type>
            <Key>US48733060</Key>
          </Debtor>
          <Department>
            <Code>CIS</Code>
            <Name>Clearance Import Sea</Name>
          </Department>
          <Description>CUSTOMS DEFERRED
  Merchandise Processing Fee                107.08
  Harbor Maintenance Fee                     38.64</Description>
          <DisplaySequence>1</DisplaySequence>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>true</SellIsPosted>
          <SellLocalAmount>0.0000</SellLocalAmount>
          <SellOSAmount>0.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0.0000</SellOSGSTVATAmount>
          <SellPostedTransactionNumber>00146100</SellPostedTransactionNumber>
          <SellPostedTransactionType>INV</SellPostedTransactionType>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>011</Code>
            <Name>011 Damco Customs Services Inc.</Name>
          </Branch>
          <ChargeCode>
            <Code>600</Code>
            <Description>CUSTOMS ENTRY SERVICES</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>BRK</Code>
            <Description>Customs Brokerage / Agency / Entry Fees</Description>
          </ChargeCodeGroup>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0.0000</CostOSGSTVATAmount>
          <Debtor>
            <Type>Organization</Type>
            <Key>US48733060</Key>
          </Debtor>
          <Department>
            <Code>CIS</Code>
            <Name>Clearance Import Sea</Name>
          </Department>
          <Description>CUSTOMS ENTRY SERVICES</Description>
          <DisplaySequence>2</DisplaySequence>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>true</SellIsPosted>
          <SellLocalAmount>57.0000</SellLocalAmount>
          <SellOSAmount>57.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0.0000</SellOSGSTVATAmount>
          <SellPostedTransactionNumber>00146100</SellPostedTransactionNumber>
          <SellPostedTransactionType>INV</SellPostedTransactionType>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>011</Code>
            <Name>011 Damco Customs Services Inc.</Name>
          </Branch>
          <ChargeCode>
            <Code>602</Code>
            <Description>INITIAL ISF FILING</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>BRK</Code>
            <Description>Customs Brokerage / Agency / Entry Fees</Description>
          </ChargeCodeGroup>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>0.0000</CostLocalAmount>
          <CostOSAmount>0.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0.0000</CostOSGSTVATAmount>
          <Debtor>
            <Type>Organization</Type>
            <Key>US48733060</Key>
          </Debtor>
          <Department>
            <Code>CIS</Code>
            <Name>Clearance Import Sea</Name>
          </Department>
          <Description>INITIAL ISF FILING</Description>
          <DisplaySequence>4</DisplaySequence>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>true</SellIsPosted>
          <SellLocalAmount>7.5000</SellLocalAmount>
          <SellOSAmount>7.5000</SellOSAmount>
          <SellOSCurrency>
            <Code>USD</Code>
            <Description>United States Dollar</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0.0000</SellOSGSTVATAmount>
          <SellPostedTransactionNumber>00146100</SellPostedTransactionNumber>
          <SellPostedTransactionType>INV</SellPostedTransactionType>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
<LloydsIMO></LloydsIMO>
    <MergeBy>
      <Code>NON</Code>
      <Description>No Merge</Description>
    </MergeBy>
    <MessageStatus>
      <Code>CEO</Code>
      <Description>Clear Entry Summary Add</Description>
    </MessageStatus>
    <MessageSubType>
      <Code></Code>
    </MessageSubType>
    <MessageType>
      <Code>IMP</Code>
      <Description>Import</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>ACE</Code>
      <Description>ACE ABI</Description>
    </MessagingApplicationCode>
    <OperationalStatus>
      <Code></Code>
    </OperationalStatus>
    <OwnerRef></OwnerRef>
    <PaymentMethod>
      <Code>IMP</Code>
      <Description>Importer</Description>
    </PaymentMethod>
    <PortOfDestination>
      <Code>USEWR</Code>
      <Name>Newark</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>USEWR</Code>
      <Name>Newark</Name>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code>CNYTN</Code>
      <Name>Yantian Pt</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>CNYTN</Code>
      <Name>Yantian Pt</Name>
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
	<TotalNoOfPacks>268</TotalNoOfPacks>
    <TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
    <TotalNoOfPacksPackageType>
      <Code>CT</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalNoOfPieces>0</TotalNoOfPieces>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>10854.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea (Non Container, Container) (10, 11)</Description>
    </TransportMode>
    <VesselName>EVER LAWFUL</VesselName>
    <VoyageFlightNo>08770</VoyageFlightNo>
    <WarehouseReleaseStatus>
      <Code></Code>
    </WarehouseReleaseStatus>
    <WayBillNumber>249700427546</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
	<AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>249700427546</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <IssueDate></IssueDate>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
        <NoOfPacks>268.0000</NoOfPacks>
        <PackType>
          <Code>CT</Code>
          <Description>Carton</Description>
        </PackType>

        <AddInfoCollection>
          <AddInfo>
            <Key>UI_NKBillIssuerSCAC</Key>
            <Value>EGLV</Value>
          </AddInfo>
        </AddInfoCollection>

        <AddInfoGroupCollection>
          <AddInfoGroup>
            <Type>
              <Code>UDP</Code>
              <Description>Disposition</Description>
            </Type>

            <AddInfoCollection>
              <AddInfo>
                <Key>Code</Key>
                <Value>94</Value>
              </AddInfo>
              <AddInfo>
                <Key>DispositionDate</Key>
                <Value>2017-03-22 02:05:00.000</Value>
              </AddInfo>
              <AddInfo>
                <Key>MessageSequenceNumber</Key>
                <Value>2</Value>
              </AddInfo>
              <AddInfo>
                <Key>Order</Key>
                <Value>2</Value>
              </AddInfo>
              <AddInfo>
                <Key>Source</Key>
                <Value>SO</Value>
              </AddInfo>
            </AddInfoCollection>
          </AddInfoGroup>
          <AddInfoGroup>
            <Type>
              <Code>UDP</Code>
              <Description>Disposition</Description>
            </Type>

            <AddInfoCollection>
              <AddInfo>
                <Key>Code</Key>
                <Value>95</Value>
              </AddInfo>
              <AddInfo>
                <Key>DispositionDate</Key>
                <Value>2017-03-27 22:51:00.000</Value>
              </AddInfo>
              <AddInfo>
                <Key>MessageSequenceNumber</Key>
                <Value>3</Value>
              </AddInfo>
              <AddInfo>
                <Key>Order</Key>
                <Value>3</Value>
              </AddInfo>
              <AddInfo>
                <Key>Source</Key>
                <Value>SO</Value>
              </AddInfo>
            </AddInfoCollection>
          </AddInfoGroup>
          <AddInfoGroup>
            <Type>
              <Code>UDP</Code>
              <Description>Disposition</Description>
            </Type>

            <AddInfoCollection>
              <AddInfo>
                <Key>Code</Key>
                <Value>94</Value>
              </AddInfo>
              <AddInfo>
                <Key>DispositionDate</Key>
                <Value>2017-03-21 14:37:00.000</Value>
              </AddInfo>
              <AddInfo>
                <Key>MessageSequenceNumber</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>Order</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>Source</Key>
                <Value>SO</Value>
              </AddInfo>
            </AddInfoCollection>
          </AddInfoGroup>
        </AddInfoGroupCollection>
      </AdditionalBill>
    </AdditionalBillCollection>
	
	<ContainerCollection Content=""Complete"">
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit>
          <Code></Code>
        </AirVentFlowRateUnit>
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
        <Commodity>
          <Code></Code>
        </Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>SEGU5463335</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality>
          <Code></Code>
        </ContainerQuality>
        <ContainerStatus>
          <Code></Code>
        </ContainerStatus>
        <ContainerType>
          <Code>40HC</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>Forty foot high cube</Description>
          <ISOCode>45G0</ISOCode>
        </ContainerType>
        <CustomsContainerSize>
          <Code></Code>
        </CustomsContainerSize>
        <DeliveryMode></DeliveryMode>
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
        <FCL_LCL_AIR>
          <Code></Code>
        </FCL_LCL_AIR>
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
        <GoodsValueCurrency>
          <Code></Code>
        </GoodsValueCurrency>
        <GoodsWeight>0.000</GoodsWeight>
        <GrossWeight>10854.000</GrossWeight>
        <GrossWeightVerificationType>
          <Code>NON</Code>
          <Description>Not Verified</Description>
        </GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit>
          <Code>FT</Code>
          <Description>Feet</Description>
        </LengthUnit>
        <MessageStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </MessageStatus>
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
        <Seal>EMCFPV4826</Seal>
        <SealPartyType>
          <Code></Code>
        </SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType>
          <Code></Code>
        </SecondSealPartyType>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>3980.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType>
          <Code></Code>
        </ThirdSealPartyType>
        <TotalHeight>9.500</TotalHeight>
        <TotalLength>40.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </Container>
    </ContainerCollection>
	
	<PackingLineCollection>
      <PackingLine>
        <BillNumber>249700427546</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <CustomsOuterPacks>0</CustomsOuterPacks>
        <InBondPackQty>0</InBondPackQty>
        <MarksAndNos></MarksAndNos>
        <PackQty>268</PackQty>
        <PackType>
          <Code>CT</Code>
        </PackType>
        <ShippingSymbol></ShippingSymbol>
      </PackingLine>
    </PackingLineCollection>
	
	 <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge>
          <Code>USEWR</Code>
          <Name>Newark</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>CNYTN</Code>
          <Name>Yantian Pt</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture></EstimatedDeparture>
        <LegNotes></LegNotes>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>EVER LAWFUL</VesselName>
        <VoyageFlightNo>08770</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
	</Shipment>
</UniversalShipment>";

			#endregion
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var options = new RoutingIntegrationOptions();

				options.AlwaysLink = false;
				options.NeverLink = true;
				options.ConditionalLink = false;

				CustomsDataRegistry.Instance.RoutingIntegrationOptions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var message = GetQueuedUniversalShipmentMessage(xml);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);

				manager.Process(message);

				CombineAssertions(delegate
				{
					var logNoteText = message.GetLogNoteText();

					AssertNotContains("The value of House Bill/Master Bill + Container No. must be unique on CusDecHouseContainerPivot", logNoteText);
					AssertContains("Successfully saved Declaration B00001000 with 1 x Bill, 1 x ForwardingContainer, 1 x CusContainer, 1 x Transport, 1 x Package.", logNoteText);
				});
			}
		}

		public void TestImportingPackingLineWithoutBills_CS00243251()
		{
			var declarationDataObject = SetupDeclaration(null, null, null);
			declarationDataObject.GoodsDescription = "HELLO WORLD";
			declarationDataObject.SetAdditionalBillCollection(() => null);
			var packingLineDataObject = SetupPackingLine(null, null, null);
			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject }));
			declarationDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertAllLinesStartWith("serviceTaskLog.ToString()", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Logs", @"No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Populating BaseJobDeclaration...
Warning - Cannot add a package as there is no bill record to attach the packages to
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.", logNoteText);

				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GoodsDescription, "HELLO WORLD"));
				AssertNotNull(declaration);
				AssertEquals("declaration.PackingGroups.Count", 0, declaration.PackingGroups.Count);
			});
		}

		public void TestImportingMultiplePackingLines()
		{
			var declarationDataObject = SetupDeclaration(null, "HB3236", new WayBillType() { Code = WayBillTypeList.Codes.House });
			var masterBillDataObject = SetupAdditionalBill("MB234", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			var houseBillDataObject = SetupAdditionalBill("HB3236", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB234", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
				{
					masterBillDataObject, houseBillDataObject
				}));
			var packingLineDataObject1 = SetupPackingLine("HB3236", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
			var packingLineDataObject2 = SetupPackingLine2("HB3236", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.Load<BaseJobDeclaration>(declarationBO.PK);
			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertEquals("declaration.PackingGroups.Count", 1, declaration.PackingGroups.Count);
			var packingGroup = declaration.PackingGroups[0];
			AssertEquals("packingGroup.CR_CU_HouseBill", primaryHouseBill.PK, packingGroup.CR_CU_HouseBill);
			AssertEquals("packingGroup.Packages.Count", 2, packingGroup.Packages.Count);
			var package1 = packingGroup.Packages[0];
			var package2 = packingGroup.Packages[1];
			if (package2.CW_PackQty == 12)
			{
				package1 = packingGroup.Packages[1];
				package2 = packingGroup.Packages[0];
			}
			AssertContents(package1, primaryHouseBill.CU_BillUniqueCode, ZString.Empty);
			AssertContents2(package2, primaryHouseBill.CU_BillUniqueCode, ZString.Empty);
		}

		public void TestDeletingBillWithPackingDetails()
		{
			Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MasterBill = "MB234";
			declaration.JE_HouseBill = "HB3236";
			Factory.SaveForTesting();
			var declarationDataObject = SetupDeclaration(null, "HB3236", new WayBillType() { Code = WayBillTypeList.Codes.House });
			var masterBillDataObject = SetupAdditionalBill("MB234", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			var houseBillDataObject = SetupAdditionalBill("HB3236", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB234", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			var houseBillToBeDeletedDataObject = SetupAdditionalBill("HB2DELETE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB234", null, 16m, Core.Constants.PkgUnit.Piece, "Piece");
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
				{
					masterBillDataObject, houseBillDataObject, houseBillToBeDeletedDataObject
				}));
			var packingLineDataObject = SetupPackingLine("HB2DELETE", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);
			declarationBO.Bills.Load();
			AssertEquals(2, declarationBO.Bills.Count);
		}

		public void TestImportingPackingDetailsForXMLWithContainers_ShouldDeleteContainersIsTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_MasterBill = "MB234";
				declaration.JE_HouseBill = "HB3236";
				declaration.JE_GoodsDescription = "goods desc";
				declaration["US_EntryType"] = "01";

				AssertEquals(2, declaration.Bills.Count);

				var bill = declaration.PrimaryHouseBill;
				var container = declaration.CusContainers.AddNew();

				var packingGroup1 = declaration.PackingGroups[0];
				var packingGroup2 = bill.PackingGroups.AddNew();
				packingGroup2.CR_CO_Container = container.PK;

				AssertEquals(2, bill.PackingGroups.Count);

				Assert(!declaration.ShouldDeleteContainers);

				Factory.SaveForTesting();

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				(declaration.Factory as IExternalFetchHintSupporter).SetupCreator();
				var declarationData = writer.GetDataObject(declaration);

				declarationData.GoodsDescription = "goods desc updated";

				declaration.JE_ContainerMode = "NCT";
				declaration["US_EntryType"] = "06";
				Assert(declaration.ShouldDeleteContainers);
				Factory.SaveForTesting();

				var reader = new JobDeclarationDataObjectReader(declarationData, logger, Factory);

				var declarationPK = declaration.PK;
				var declarationCopy = reader.ReadIntoBusinessObject();

				AssertEquals(declarationPK, declarationCopy.PK);
				AssertEquals("goods desc updated", declarationCopy.JE_GoodsDescription);

				var newFactory = new BusinessObjectFactory();
				var billCopy = newFactory.Load<Bill>(bill.PK);

				AssertNotNull(billCopy);
				AssertEquals(1, billCopy.PackingGroups.Count);
				AssertEquals(ZGuid.Empty, billCopy.PackingGroups[0].CR_CO_Container);
			}
		}

		public void TestImportingPackingDetailsForXMLWithContainers_ShouldDeleteContainersIsFalse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_MasterBill = "MB234";
				declaration.JE_HouseBill = "HB3236";
				declaration.JE_GoodsDescription = "goods desc";
				declaration["US_EntryType"] = "01";

				AssertEquals(2, declaration.Bills.Count);

				var bill = declaration.PrimaryHouseBill;
				var container = declaration.CusContainers.AddNew();

				var packingGroup1 = declaration.PackingGroups[0];
				var packingGroup2 = bill.PackingGroups.AddNew();
				packingGroup2.CR_CO_Container = container.PK;

				AssertEquals(2, bill.PackingGroups.Count);

				Assert(!declaration.ShouldDeleteContainers);

				Factory.SaveForTesting();

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				(declaration.Factory as IExternalFetchHintSupporter).SetupCreator();
				var declarationData = writer.GetDataObject(declaration);

				declarationData.GoodsDescription = "goods desc updated";

				var reader = new JobDeclarationDataObjectReader(declarationData, logger, Factory);

				var declarationCopy = reader.ReadIntoBusinessObject();

				AssertEquals(declaration.PK, declarationCopy.PK);
				AssertEquals("goods desc updated", declarationCopy.JE_GoodsDescription);

				var newFactory = new BusinessObjectFactory();
				var billCopy = newFactory.Load<Bill>(bill.PK);

				AssertNotNull(billCopy);
				AssertEquals(2, billCopy.PackingGroups.Count);
				AssertEquals(1, billCopy.PackingGroups.Where(x => !x.CR_CO_Container.IsEmpty).Count());
			}
		}

		public void TestImportingPackingLineWithNoBillDetail()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MasterBill = "MB32";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
			AssertEquals("declaration.CusContainers.Count", 0, declaration.CusContainers.Count);
			AssertEquals("declaration.PackingGroups.Count", 0, declaration.PackingGroups.Count);
			Factory.SaveForTesting();
			var shipmentDataObject = SetupDeclaration(null, "MB32", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			var packingLineDataObject1 = SetupPackingLine(null, null, "OOCL0000027");
			var packingLineDataObject2 = SetupPackingLine2(null, null, ZString.Empty);

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory);
			var declarationPK = declaration.PK;
			declaration = reader.ReadIntoBusinessObject();
			AssertEquals(declarationPK, declaration.PK);
			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				declaration.Bills.Load();
				AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
				declaration.CusContainers.Load();
				AssertEquals("declaration.CusContainers.Count", 1, declaration.CusContainers.Count);
				var container = declaration.CusContainers.Find("OOCL0000027");
				AssertNotNull("A Container should be added from the packing line detail", container);

				declaration.PackingGroups.Load();
				AssertEquals("declaration.PackingGroups.Count", 2, declaration.PackingGroups.Count);
				var packingGroup1 = declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(primaryMasterBill, container);
				AssertNotNull("packingGroup1 should be added from the packing line detail", packingGroup1);
				var packingGroup2 = declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(primaryMasterBill, null);
				AssertNotNull("packingGroup2 should be added from the packing line detail", packingGroup2);

				declaration.Packages.Load();
				var packingLineBO1 = declaration.Packages.OfType<BasePackage>().FirstOrDefault(x => x.CW_ContainerNoOrEquipmentNo == "OOCL0000027");
				var packingLineBO2 = declaration.Packages.OfType<BasePackage>().FirstOrDefault(x => x.CW_ContainerNoOrEquipmentNo.IsEmpty);
				AssertContents(packingLineBO1, primaryMasterBill.CU_BillUniqueCode, "OOCL0000027");
				AssertEquals("packingLineBO1.PackingGroup", packingGroup1, packingLineBO1.PackingGroup);
				AssertContents2(packingLineBO2, primaryMasterBill.CU_BillUniqueCode, ZString.Empty);
				AssertEquals("packingLineBO2.PackingGroup", packingGroup2, packingLineBO2.PackingGroup);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Warning - Cannot find Container (OOCL0000027) for packing line; new Container added.
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Information - Updated Declaration B00001000 from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestImportingContainersAndBillsViaPackingLineDetails()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.SaveForTesting();
			var shipmentDataObject = SetupDeclaration(null, null, null);
			var packingLineDataObject1 = SetupPackingLine("MB32", new WayBillType() { Code = WayBillTypeList.Codes.Master }, "OOCL0000027");
			var packingLineDataObject2 = SetupPackingLine2("HB33", new WayBillType() { Code = WayBillTypeList.Codes.House }, "OOCL0000028");

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory);
			declaration = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				AssertEquals("declaration.CusContainers.Count", 2, declaration.CusContainers.Count);
				var container1 = declaration.CusContainers.Find("OOCL0000027");
				var container2 = declaration.CusContainers.Find("OOCL0000028");
				AssertNotNull("A Container 'OOCL0000027' should be added from the packing line detail", container1);
				AssertNotNull("A Container 'OOCL0000028' should be added from the packing line detail", container2);
				AssertEquals("declaration.Packages.Count", 2, declaration.Packages.Count);
				var package1 = declaration.Packages[0];
				var package2 = declaration.Packages[1];
				if (package2.CW_PackQty == 12)
				{
					package2 = declaration.Packages[0];
					package1 = declaration.Packages[1];
				}
				var packingGroup1 = package1.PackingGroup;
				AssertEquals("packingGroup1.CR_CO_Container", container1.PK, packingGroup1.CR_CO_Container);
				AssertEquals("packingGroup1.Bill.CU_BillNum", "MB32", packingGroup1.Bill.CU_BillNum);
				var packingGroup2 = package2.PackingGroup;
				AssertEquals("packingGroup2.CR_CO_Container", container2.PK, packingGroup2.CR_CO_Container);
				AssertEquals("packingGroup2.Bill.CU_BillNum", "HB33", packingGroup2.Bill.CU_BillNum);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Cannot find Bill (Type:'MB', Number:'MB32') for packing line; new Bill added.
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Warning - Cannot find Container (OOCL0000027) for packing line; new Container added.
Warning - Cannot find Bill (Type:'HB', Number:'HB33') for packing line; new Bill added.
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Warning - Cannot find Container (OOCL0000028) for packing line; new Container added.
Information - Added Declaration (Master Bill='MB32' House Bill='HB33') from UniversalShipment.".Trim(), logger.Logs);
				AssertEquals("CO_DataModel", string.Empty, container1.CO_DataModel);
				Factory.SaveForTesting();
				AssertEquals("CO_DataModel", "ER", container1.CO_DataModel);
			});
		}

		public void TestImportPackingLineCreateMissingContainers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_HouseBill = "BILL1234";
			var houseBill = declaration.PrimaryHouseBill;
			var packingLineDataObject = SetupPackingLine("BILL1234", new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House }, "OOCL0000027");
			var reader = new CustomsPackingLineDataObjectReader(packingLineDataObject, logger, CurrentCompanyHelper, declaration, houseBill, declaration.SupportsParentPackage);
			declaration.CusContainers.RemoveAndDeleteAll();
			var packingLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull(packingLineBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("declaration.CusContainers.Count", 1, declaration.CusContainers.Count);
				var container = declaration.CusContainers[0];
				AssertEquals("container.CO_ContainerNumber", "OOCL0000027", container.CO_ContainerNumber);
				var packingGroup = packingLineBO.PackingGroup;
				AssertEquals("packingGroup.CR_CO_Container", container.PK, packingGroup.CR_CO_Container);
			});

			#endregion
		}

		public void TestBasicPackingLineLevelFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MasterBill = "BILL1234";
			var masterBill = declaration.PrimaryMasterBill;
			declaration.JE_HouseBill = "BILL1234";
			var houseBill = declaration.PrimaryHouseBill;
			var subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "BILL1234";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABCD0000028";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCL0000027";
			houseBill.PackingGroups.RemoveAndDeleteAll();
			declaration.PackingGroups.RemoveAndDeleteAll();
			var packingLineDataObject = SetupPackingLine("BILL1234", new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House }, "OOCL0000027");
			var reader = new CustomsPackingLineDataObjectReader(packingLineDataObject, logger, CurrentCompanyHelper, declaration, houseBill, declaration.SupportsParentPackage);
			var packingLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull(packingLineBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(packingLineBO, houseBill.CU_BillUniqueCode, "OOCL0000027");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
".Trim(), logger.Logs);
				AssertEquals("houseBill.PackingGroups.Count", 1, houseBill.PackingGroups.Count);
				var packingGroup = houseBill.PackingGroups[0];
				AssertEquals("packingLineBO.CW_CR_HouseContainer", packingGroup.PK, packingLineBO.CW_CR_HouseContainer);
				AssertCollectionContains("declaration.PackingGroups", packingGroup, declaration.PackingGroups);
			});

			#endregion
		}

		public void TestImportingPackingDetails_DoesNotSupportParentPackage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				//Test Children Packlines are ignored when flag is false
				var declarationDataObject = SetupParentPackingLineDeclaration();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var reader = new CustomsShipmentDataObjectReaderProvider().GetReader(declarationDataObject, logger, Factory, null);
				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (BaseJobDeclaration)bizObj;
				Factory.SaveForTesting();
				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.Load<BaseJobDeclaration>(declarationBO.PK);

				Assert(!declaration.SupportsParentPackage);
				AssertEquals("declaration.PackingGroups.Count", 1, declaration.PackingGroups.Count);
				var packingGroup = declaration.PackingGroups[0];
				AssertEquals("packingGroup.Packages.Count", 1, packingGroup.Packages.Count);
				Assert("!packingGroup.Packages[0].Children.Any()", !packingGroup.Packages[0].Children.Any());
			}
		}

		public void TestImportingChildrenPackingDetails_SupportsParentPackage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declarationDataObject = SetupParentPackingLineDeclaration();
				declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
				declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());
				declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = CAAddInfoSchema.CA_ServiceOption.Name.Substring(3), Value = "IID" });

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var reader = new CustomsShipmentDataObjectReaderProvider().GetReader(declarationDataObject, logger, Factory, null);
				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (BaseJobDeclaration)bizObj;
				Factory.SaveForTesting();
				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.Load<BaseJobDeclaration>(declarationBO.PK);

				//Test Children Packlines are added to the correct parent
				Assert(declaration.SupportsParentPackage);
				AssertEquals("declaration.PackingGroups.Count", 1, declaration.PackingGroups.Count);
				var packingGroup = declaration.PackingGroups[0];
				AssertEquals("packingGroup.CR_CU_HouseBill", declaration.PrimaryHouseBill.PK, packingGroup.CR_CU_HouseBill);
				AssertEquals("packingGroup.Packages.Count", 3, packingGroup.Packages.Count);
				AssertEquals("packingGroup.Packages[0].Children.Count()", 1, packingGroup.Packages[0].Children.Count());

				//Test details are correct on the first child packline
				var packingGroupChild = packingGroup.Packages[0].Children.First();
				AssertEquals("CW_CW_Parent matches Parent PK", packingGroup.Packages[0].PK, packingGroupChild.CW_CW_Parent);
				AssertContents2(packingGroupChild, "HB:HWB8181 (MB:MWB9191)", null);

				//Test details are correct on the first child->child packline
				var packingGroupChild2 = packingGroup.Packages[1].Children.First();
				AssertEquals("CW_CW_Parent matches Parent PK", packingGroupChild.PK, packingGroupChild2.CW_CW_Parent);
				AssertContents3(packingGroupChild2, "HB:HWB8181 (MB:MWB9191)", null);
			}
		}

		public void TestDefaultingFieldsPopulatedAndCanSave_Shipment()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MasterBill = "MB32";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
			AssertEquals("declaration.CusContainers.Count", 0, declaration.CusContainers.Count);
			AssertEquals("declaration.PackingGroups.Count", 0, declaration.PackingGroups.Count);
			Factory.SaveForTesting();
			var shipmentDataObject = SetupDeclaration(null, "MB32", new WayBillType() { Code = WayBillTypeList.Codes.Master });
			var packingLineDataObject1 = SetupPackingLine(null, null, "OOCL0000027");
			var packingLineDataObject2 = SetupPackingLine2(null, null, ZString.Empty);

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory);
			var declarationPK = declaration.PK;
			declaration = reader.ReadIntoBusinessObject();
			AssertEquals(declarationPK, declaration.PK);

			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(logger));

			#region Check Contents Of Business Object

			CombineAssertions(() =>
			{
				declaration.Bills.Load();
				AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
				declaration.CusContainers.Load();
				AssertEquals("declaration.CusContainers.Count", 1, declaration.CusContainers.Count);
				var container = declaration.CusContainers.Find("OOCL0000027");
				AssertNotNull("A Container should be added from the packing line detail", container);
				AssertEquals("CO_DataModel", "ER", container.CO_DataModel);

				AssertContains("logger.Logs", @"Warning - Cannot find Container (OOCL0000027) for packing line; new Container added.", logger.Logs);
			});

			#endregion
		}

		UniversalShipment SetupParentPackingLineDeclaration()
		{
			var declarationDataObject = SetupDeclaration(null, "HWB8181", new WayBillType() { Code = WayBillTypeList.Codes.House });
			var masterBillDataObject = SetupAdditionalBill("MWB9191", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2017, 10, 16), null, null, 1m, Core.Constants.PkgUnit.Box, "BOX");
			var houseBillDataObject = SetupAdditionalBill("HWB8181", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2017, 10, 16), "MWB9191", null, 2m, Core.Constants.PkgUnit.Piece, "Piece");
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
				{
					masterBillDataObject, houseBillDataObject
				}));
			var packingLineDataObject1 = SetupPackingLine("HWB8181", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
			packingLineDataObject1.SetPackingLineCollection(() =>
			{
				var packingLineDataObject2 = SetupPackingLine2("HWB8181", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
				packingLineDataObject2.SetPackingLineCollection(() =>
				{
					var packingLineDataObject3 = SetupPackingLine3("HWB8181", new WayBillType() { Code = WayBillTypeList.Codes.House }, null);
					return new List<PackingLine>(new[] { packingLineDataObject3 });
				});
				return new List<PackingLine>(new[] { packingLineDataObject2 });
			});

			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1 })
			{
				Content = CollectionContent.Complete
			});
			return declarationDataObject;
		}
	}
}
