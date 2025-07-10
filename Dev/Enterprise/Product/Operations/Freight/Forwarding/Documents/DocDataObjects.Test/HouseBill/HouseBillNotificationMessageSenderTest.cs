using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using NotificationsHandler = Enterprise.DocumentVisualizer.Business.NotificationsHandler;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillNotificationMessageSenderTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new HouseBillNotificationMessageSender());

		IDocDataObjectMessageSender messageSender;

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.DocumentMenuCarrierBillOfLadingPK;

		protected override ZString DocumentName => ShipmentDocumentDataStoreNames.CarrierBillOfLading;

		protected override ZString MSNReference => "|DEP=Booking Party|MST=Draft Bill of Lading";

		protected override bool AllowSendMessageAmendment => true;

		[TestDate]
		public override void TestSendMessage()
		{
			using (Factory.AddDisposableService())
			{
				TestDateAttribute.Date = new DateTime(2021, 1, 1);
				var bizObj = SetBusinessObject();
				var notifications = new NotificationsHandler();
				var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Notifications.Count);

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, DocumentName);
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, bizObj.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);
				AssertNotNull("document data has been created", documentDataStorage);

				var logs = (documentDataStorage as IStmALogParent)
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode || l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", MSNReference, msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertMultilineASCIIEquals("UXml", string.Format(ExpectedMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum), ClearImageData(message.Message.EM_MessageText));

				if (AllowSendMessageAmendment)
				{
					TestDateAttribute.Date = new DateTime(2021, 1, 2);
					res = MessageSender.SendMessage(bizObj, MenuItem, notifications);
					Assert("message has been sent", res);
					logs = (documentDataStorage as IStmALogParent)
						.Logs
						.GetAllLogs()
						.Cast<StmALog>()
						.OrderByDescending(log => log.SL_PostedTimeUtc)
						.ToArray();

					msn = logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode);
					AssertEquals("MSN reference", MSNReference, msn.SL_Reference);

					dex = logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode);
					message = dex.RelatedEDIMessage;
					AssertNotNull("EDI message has been created", message);
					AssertMultilineASCIIEquals("UXml", string.Format(ExpectedAmendmentMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum), ClearImageData(message.Message.EM_MessageText));
				}
			}
		}

		protected override ZString ExpectedMessage =>
			@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/BLData/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>SH0001</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Draft Bill of Lading</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
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
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <CoLoadBookingConfirmationReference>SH0001</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>HOUSEBILL001</CoLoadMasterBillNumber>
    <ContainerCount>1</ContainerCount>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <GoodsDescription>goods description</GoodsDescription>
    <HouseBillOfLadingType Description=""FIATA HBL"">FIA</HouseBillOfLadingType>
    <NoCopyBills>1</NoCopyBills>
    <NoOriginalBills>2</NoOriginalBills>
    <PlaceOfDelivery Name=""Auckland"">NZAKL</PlaceOfDelivery>
    <PlaceOfIssue Name=""Brisbane"">AUBNE</PlaceOfIssue>
    <PlaceOfReceipt Name=""Sydney"">AUSYD</PlaceOfReceipt>
    <PortOfDestination Name=""Auckland"">NZAKL</PortOfDestination>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentIncoTerm Description=""Cost And Freight"">CFR</ShipmentIncoTerm>
    <TotalNoOfPacks>3</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <VesselName>Vessel</VesselName>
    <VoyageFlightNo>F9999</VoyageFlightNo>
    <WayBillNumber>HOUSEBILL001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>Freight Amount</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Declared Value</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Freight Payable at</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExcessValueDeclarationAmount</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeFrom</Key>
        <Value>CY</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeTo</Key>
        <Value>CY</Value>
      </AddInfo>
      <AddInfo>
        <Key>AsAgentOption</Key>
        <Value>CRR</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsignorShipperTerminology</Key>
        <Value>Consignor</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>SHP001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>Draft Bill (HOUSEBILL001).pdf</FileName>
        <ImageData></ImageData>
        <Type Description=""Draft Bill of Lading"">DBL</Type>
        <IsPublished>false</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Shipper Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <Commodity></Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAAA0000007</ContainerNumber>
        <ContainerQuality></ContainerQuality>
        <ContainerType>
          <Code></Code>
          <Category></Category>
          <Description></Description>
          <ISOCode></ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>0</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>2000</GoodsWeight>
        <GrossWeight>2000</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description=""Not Verified"">NON</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>0</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0</OverhangRight>
        <Seal></Seal>
        <SealPartyType></SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit></SetPointTempUnit>
        <TareWeight>0</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-02T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-03T00:00:00</Value>
      </Date>
    </DateCollection>
    <EntryNumberCollection>
      <EntryNumber>
        <Number>T7HRTXGXT</Number>
        <Type Description=""Contingency Customs Authority Numbe"">CCN</Type>
      </EntryNumber>
    </EntryNumberCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Marks &amp; Numbers</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>marks &amp; numbers</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2000</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 1</Address1>
        <Address2>4 What Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Auckland</City>
        <CompanyName>DUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""New Zealand"">NZ</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Auckland"">NZAKL</Port>
        <Postcode>5022</Postcode>
        <State>AUK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 888</Address1>
        <Address2>8 What Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Auckland</City>
        <CompanyName>FUNNY</CompanyName>
        <Contact></Contact>
        <Country Name=""New Zealand"">NZ</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Auckland"">NZAKL</Port>
        <Postcode>5012</Postcode>
        <State>AUK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>55 Why Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>YUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2000</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <Commodity Description=""General"">GEN</Commodity>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>I'm loose baby!</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>I'm loose baby!</GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>0</Length>
        <LengthUnit Description=""Meters"">M</LengthUnit>
        <MarksAndNos>marks &amp; numbers</MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID></PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>1.3</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>3000</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>0</Width>
        <ClassificationCollection>
        </ClassificationCollection>
        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
  </Shipment>
</UniversalShipment>";

		protected override ZString ExpectedAmendmentMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/BLData/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>SH0001</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Draft Bill of Lading</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
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
        <TriggerDate>2021-01-02T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <CoLoadBookingConfirmationReference>SH0001</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>HOUSEBILL001</CoLoadMasterBillNumber>
    <ContainerCount>1</ContainerCount>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <GoodsDescription>goods description</GoodsDescription>
    <HouseBillOfLadingType Description=""FIATA HBL"">FIA</HouseBillOfLadingType>
    <NoCopyBills>1</NoCopyBills>
    <NoOriginalBills>2</NoOriginalBills>
    <PlaceOfDelivery Name=""Auckland"">NZAKL</PlaceOfDelivery>
    <PlaceOfIssue Name=""Brisbane"">AUBNE</PlaceOfIssue>
    <PlaceOfReceipt Name=""Sydney"">AUSYD</PlaceOfReceipt>
    <PortOfDestination Name=""Auckland"">NZAKL</PortOfDestination>
    <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentIncoTerm Description=""Cost And Freight"">CFR</ShipmentIncoTerm>
    <TotalNoOfPacks>3</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <VesselName>Vessel</VesselName>
    <VoyageFlightNo>F9999</VoyageFlightNo>
    <WayBillNumber>HOUSEBILL001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>Freight Amount</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Declared Value</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>Freight Payable at</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExcessValueDeclarationAmount</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeFrom</Key>
        <Value>CY</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeTo</Key>
        <Value>CY</Value>
      </AddInfo>
      <AddInfo>
        <Key>AsAgentOption</Key>
        <Value>CRR</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsignorShipperTerminology</Key>
        <Value>Consignor</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>SHP001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>Draft Bill (HOUSEBILL001).pdf</FileName>
        <ImageData></ImageData>
        <Type Description=""Draft Bill of Lading"">DBL</Type>
        <IsPublished>false</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Shipper Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <Commodity></Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAAA0000007</ContainerNumber>
        <ContainerQuality></ContainerQuality>
        <ContainerType>
          <Code></Code>
          <Category></Category>
          <Description></Description>
          <ISOCode></ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>0</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>2000</GoodsWeight>
        <GrossWeight>2000</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description=""Not Verified"">NON</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>0</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0</OverhangRight>
        <Seal></Seal>
        <SealPartyType></SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit></SetPointTempUnit>
        <TareWeight>0</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-02T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-01-03T00:00:00</Value>
      </Date>
    </DateCollection>
    <EntryNumberCollection>
      <EntryNumber>
        <Number>T7HRTXGXT</Number>
        <Type Description=""Contingency Customs Authority Numbe"">CCN</Type>
      </EntryNumber>
    </EntryNumberCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Marks &amp; Numbers</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>marks &amp; numbers</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2000</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 1</Address1>
        <Address2>4 What Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Auckland</City>
        <CompanyName>DUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""New Zealand"">NZ</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Auckland"">NZAKL</Port>
        <Postcode>5022</Postcode>
        <State>AUK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 888</Address1>
        <Address2>8 What Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Auckland</City>
        <CompanyName>FUNNY</CompanyName>
        <Contact></Contact>
        <Country Name=""New Zealand"">NZ</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType></GovRegNumType>
        <Phone></Phone>
        <Port Name=""Auckland"">NZAKL</Port>
        <Postcode>5012</Postcode>
        <State>AUK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>55 Why Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>YUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2000</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <Commodity Description=""General"">GEN</Commodity>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>I'm loose baby!</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>I'm loose baby!</GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>0</Length>
        <LengthUnit Description=""Meters"">M</LengthUnit>
        <MarksAndNos>marks &amp; numbers</MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID>EDIDAT00000003</PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>1.3</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>3000</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>0</Width>
        <ClassificationCollection>
        </ClassificationCollection>
        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
  </Shipment>
</UniversalShipment>
";

		protected override BusinessObject SetBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var mainTransport = shipment.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var otherTransport = shipment.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = "SEA";
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "CNCAN";
			departureConsol.JK_BookingReference = "BookingRef";
			departureConsol.JK_CoLoadBookingReference = "CoLoadBookingRef";

			var consolTransport = departureConsol.Transports[0];
			consolTransport.JW_Vessel = "Vessel";
			consolTransport.JW_VoyageFlight = "F9999";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerMode = ContainerModes.FCL;

			var packline1 = (ForwardingPackLine)shipment.OuterPackLines.Single();
			packline1.JL_PackageCount = 1;
			packline1.JL_Description = "pack line 1";
			container.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 2000;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			packline2.JL_ActualVolume = 1.3;
			packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline2.JL_PackageCount = 2;
			packline2.JL_Description = "pack line 2";
			container.PackLines.Add(packline2);

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_ActualWeight = 3000;
			loosePackLine.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine.JL_ActualVolume = 1.3;
			loosePackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine.JL_PackageCount = 3;
			loosePackLine.JL_Description = "I'm loose baby!";
			loosePackLine.Containers.RemoveAll();

			PopulateShipmentAddresses(shipment);

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_TransportMode = "SEA";
			arrivalConsol.JK_RL_NKLoadPort = "CNCAN";
			arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			departureConsol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "JIMMY";
			receivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Auckland";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			departureConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 2;

			var hir = Factory.New<CusEntryNumber>();
			hir.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			hir.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			hir.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			hir.CE_EntryNum = "SHP001";
			shipment.Numbers.Add(hir);

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "MANY";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "TOO MUCH";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 686";
			notifyParty3.MainAddress.Address2 = "99 How Lane";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5038";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
		}

		ZString ClearImageData(string input)
		{
			string pattern = @"<ImageData>.*?</ImageData>";
			string replacement = "<ImageData></ImageData>";
			return Regex.Replace(input, pattern, replacement);
		}
	}
}
