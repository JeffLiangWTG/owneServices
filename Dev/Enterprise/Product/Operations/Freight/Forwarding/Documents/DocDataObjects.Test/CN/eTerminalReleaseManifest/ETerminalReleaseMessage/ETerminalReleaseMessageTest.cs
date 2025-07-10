using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ETerminalReleaseMessage))]
	sealed class ETerminalReleaseMessageTestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendMessage_NoConsolSelected()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Q1234B";

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "CNNBO";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			Factory.Save();

			var releaseMessage = new ETerminalReleaseMessage(voyage);
			var consol = CreateVoyageRelatedConsol(sailing, "C00001000");

			var messageConsol = new ETerminalReleaseMessageConsol(consol);
			releaseMessage.MessageConsols.Add(messageConsol);

			var notifications = new DummyProgressNotifications();
			releaseMessage.SendMessage(notifications);
			AssertEquals("No consol is selected for sending.", notifications.ToString());
		}

		public void TestSendMessage_MessageHasBeenSent()
		{
			using (Factory.AddDisposableService())
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Vessel";

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "Q1234B";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "CNNBO";

				var destination = Factory.New<VoyageDestination>();
				destination.JB_RL_NKPortOfDischarge = "AUSYD";

				var sailing = Factory.New<JobSailing>();
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;

				origin.JA_JV = voyage.PK;
				destination.JB_JV = voyage.PK;

				var consol = CreateConsolForSendingMessage(sailing, "C00001000", true);
				var eventParameters = new[]
				{
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, "eTerminal Release Manifest"),
				};

				consol.Logs.AddNew(Events.MessageSent, eventParameters);

				Factory.Save();

				var releaseMessage = new ETerminalReleaseMessage(voyage);

				var messageConsol = new ETerminalReleaseMessageConsol(consol);
				messageConsol.Send = true;
				releaseMessage.MessageConsols.Add(messageConsol);

				var notifications = new DummyProgressNotifications();
				releaseMessage.SendMessage(notifications);
				AssertEquals(@"[BROKEN-HL C00001000]:
eTerminal Release Manifest message for this Consol has been sent but no reply has been received from WTG eHub.
Ningbo EDI Center can only process original messages, and therefore if you need to send this Consol again, please reset its status to Original from within this Consol's > Electronic Messaging > Port Messaging menu.

There are Consols with eTerminal Release errors on this voyage.
View errors by opening each Consol with errors from the list, go to Consol > Electronic Messaging > Port Messaging > eTerminal Release.
Note you can send eTerminal Release Manifest from each Consol individually."
					, notifications.ToString());

				consol.Logs.AddNew(Events.InterchangeRejected, eventParameters);

				Thread.Sleep(1);
				Factory.Save();

				notifications.Notifications.Clear();

				releaseMessage = new ETerminalReleaseMessage(voyage);
				messageConsol = new ETerminalReleaseMessageConsol(consol);
				messageConsol.Send = true;
				releaseMessage.MessageConsols.Add(messageConsol);

				releaseMessage.SendMessage(notifications);

				AssertEquals(@"[BROKEN-HL C00001000]:
eTerminal Release Manifest message has be sent successfully!
"
				, notifications.ToString());
			}
		}

		public void TestSendMessage_SendingMessageSuccessfully()
		{
			using (Factory.AddDisposableService())
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Vessel";

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "Q1234B";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "CNNBO";

				var destination = Factory.New<VoyageDestination>();
				destination.JB_RL_NKPortOfDischarge = "AUSYD";

				var sailing = Factory.New<JobSailing>();
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;

				origin.JA_JV = voyage.PK;
				destination.JB_JV = voyage.PK;

				Factory.Save();

				var releaseMessage = new ETerminalReleaseMessage(voyage);
				var consol = CreateConsolForSendingMessage(sailing, "C00001000", true);

				var messageConsol = new ETerminalReleaseMessageConsol(consol);
				messageConsol.Send = true;
				releaseMessage.MessageConsols.Add(messageConsol);

				var notifications = new DummyProgressNotifications();
				releaseMessage.SendMessage(notifications);
				AssertEquals(@"[BROKEN-HL C00001000]:
eTerminal Release Manifest message has be sent successfully!
"
					, notifications.ToString());

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, "eTerminalReleaseManifest");
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

				var logs = (documentDataStorage as IStmALogParent)
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode || l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msnEvent = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|DEP=Terminal|MST=eTerminal Release Manifest", msnEvent.SL_Reference);

				var dexEvent = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dexEvent.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_SendingMessage_ApplyOverriddenValues()
		{
			using (Factory.AddDisposableService())
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = "Vessel";
				voyage.JV_VoyageFlight = "Q1234B";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "CNNBO";

				var destination = Factory.New<VoyageDestination>();
				destination.JB_RL_NKPortOfDischarge = "AUSYD";

				var sailing = Factory.New<JobSailing>();
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;

				origin.JA_JV = voyage.PK;
				destination.JB_JV = voyage.PK;

				Factory.Save();

				var releaseMessage = new ETerminalReleaseMessage(voyage);
				var consol = CreateConsolForSendingMessage(sailing, "C00001000", true);

				var messageConsol = new ETerminalReleaseMessageConsol(consol);
				messageConsol.Send = true;
				releaseMessage.MessageConsols.Add(messageConsol);

				var xml = XDocument.Parse(string.Format(OverriddenXML, consol.PK));

				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
				documentData.JDD_Name = "eTerminalReleaseManifest";
				documentData.JDD_OverriddenData = xml.ToString(SaveOptions.DisableFormatting);
				documentData.JDD_ParentID = consol.PK;

				var notifications = new DummyProgressNotifications();
				releaseMessage.SendMessage(notifications);
				AssertEquals(@"[BROKEN-HL C00001000]:
eTerminal Release Manifest message has be sent successfully!
"
					, notifications.ToString());

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, "eTerminalReleaseManifest");
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

				var logs = (documentDataStorage as IStmALogParent)
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode || l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msnEvent = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|DEP=Terminal|MST=eTerminal Release Manifest", msnEvent.SL_Reference);

				var dexEvent = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dexEvent.RelatedEDIMessage;

				var flashPoint = "<FlashPoint>15.0</FlashPoint>".PadLeft(53, ' ');
				var expectedUXMLForOverridden = GetExpectedUXMLForOverridden(flashPoint, message.Message);

				AssertNotNull("EDI message has been created", message);
				AssertMultilineASCIIEquals("FCL BillOfLadingClause is generated by overriding the default value", expectedUXMLForOverridden, message.Message.EM_MessageText);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_SendingMessage_ApplyOverriddenValuesWithCombustibleEqualFalse()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			using (Factory.AddDisposableService())
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = "Vessel";
				voyage.JV_VoyageFlight = "Q1234B";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "CNNBO";

				var destination = Factory.New<VoyageDestination>();
				destination.JB_RL_NKPortOfDischarge = "AUSYD";

				var sailing = Factory.New<JobSailing>();
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;

				origin.JA_JV = voyage.PK;
				destination.JB_JV = voyage.PK;

				Factory.Save();

				var releaseMessage = new ETerminalReleaseMessage(voyage);
				var consol = CreateConsolForSendingMessage(sailing, "C00001000", false);

				var messageConsol = new ETerminalReleaseMessageConsol(consol);
				messageConsol.Send = true;
				releaseMessage.MessageConsols.Add(messageConsol);

				var xml = XDocument.Parse(string.Format(OverriddenXML, consol.PK));

				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
				documentData.JDD_Name = "eTerminalReleaseManifest";
				documentData.JDD_OverriddenData = xml.ToString(SaveOptions.DisableFormatting);
				documentData.JDD_ParentID = consol.PK;

				var notifications = new DummyProgressNotifications();
				releaseMessage.SendMessage(notifications);
				AssertEquals(@"[BROKEN-HL C00001000]:
eTerminal Release Manifest message has be sent successfully!
"
					, notifications.ToString());

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, "eTerminalReleaseManifest");
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

				var logs = (documentDataStorage as IStmALogParent)
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode || l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msnEvent = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|DEP=Terminal|MST=eTerminal Release Manifest", msnEvent.SL_Reference);

				var dexEvent = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dexEvent.RelatedEDIMessage;

				var expectedUXMLForOverridden = GetExpectedUXMLForOverridden(null, message.Message);

				AssertNotNull("EDI message has been created", message);
				AssertMultilineASCIIEquals("FCL BillOfLadingClause is generated by overriding the default value", expectedUXMLForOverridden, message.Message.EM_MessageText);
			}
		}

		const string OverriddenXML = @"
<Entity>
  <Id>{0}</Id>
  <Property Name=""IsFreightPrepaid"">
    <Value>N</Value>
  </Property>
  <Property Name=""IsFreightCollect"">
    <Value>Y</Value>
  </Property>
</Entity>";

		string GetExpectedUXMLForOverridden(string flashpoint, Messaging.Integration.IEDIMessage message) => $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/TerminalRelease/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>eTerminal Release Manifest</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""China"">CN</Country>
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

    <LloydsIMO>8907993</LloydsIMO>
    <VesselName>BUNGA XYLIMA</VesselName>
    <VoyageFlightNo>F9999</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNNBO</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Ningbo</Value>
      </AddInfo>
    </AddInfoCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Aalborg</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Denmark"">DK</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Aalborg"">DKAAL</Port>
        <Postcode>2000</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
            <CountryOfIssue Name=""United States"">US</CountryOfIssue>
            <Value>CARCCC</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact>CargoWise Support</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>

        <PortOfDischarge Name=""Sydney"">AUSYD</PortOfDischarge>
        <PortOfLoading Name=""Ningbo"">CNNBO</PortOfLoading>

        <SubShipmentCollection>
          <SubShipment>

            <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
            <NoCopyBills>22</NoCopyBills>
            <NoOriginalBills>11</NoOriginalBills>
            <PlaceOfDelivery Name=""Singapore"">SGSIN</PlaceOfDelivery>
            <PlaceOfIssue Name=""Aalborg"">DKAAL</PlaceOfIssue>
            <ReleaseType Description=""BOL Original"">BOL</ReleaseType>
            <WayBillNumber>BookingRef001</WayBillNumber>

            <AddInfoCollection>
              <AddInfo>
                <Key>FreightPayableAt_Code</Key>
                <Value>CNNBO</Value>
              </AddInfo>
              <AddInfo>
                <Key>FreightPayableAt_Name</Key>
                <Value>Ningbo</Value>
              </AddInfo>
              <AddInfo>
                <Key>MasterSONumber</Key>
                <Value>CBR</Value>
              </AddInfo>
            </AddInfoCollection>

            <AdditionalReferenceCollection>
              <AdditionalReference>
                <Type Description=""Shipper Reference"">SHP</Type>
                <ReferenceNumber>AgentRef001</ReferenceNumber>
              </AdditionalReference>
              <AdditionalReference>
                <Type Description=""Freight Forwarder Reference"">FFW</Type>
                <ReferenceNumber>C00001000</ReferenceNumber>
              </AdditionalReference>
            </AdditionalReferenceCollection>

            <BillOfLadingClauseCollection>
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
                <ContainerNumber>CONT1111111</ContainerNumber>
                <ContainerQuality Description=""Food"">FOD</ContainerQuality>
                <ContainerType>
                  <Code>WU8CM2QZNK</Code>
                  <Category></Category>
                  <Description></Description>
                  <ISOCode>22P1</ISOCode>
                </ContainerType>
                <DepartureEstimatedPickup></DepartureEstimatedPickup>
                <DunnageWeight>1000</DunnageWeight>
                <EmptyRequired></EmptyRequired>
                <ExportDepotCustomsReference></ExportDepotCustomsReference>
                <GoodsWeight>223</GoodsWeight>
                <GrossWeight>1223</GrossWeight>
                <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
                <GrossWeightVerificationType Description=""Not Verified"">NON</GrossWeightVerificationType>
                <HumidityPercent>0</HumidityPercent>
                <ImportDepotCustomsReference></ImportDepotCustomsReference>
                <IsControlledAtmosphere>false</IsControlledAtmosphere>
                <IsEmptyContainer>false</IsEmptyContainer>
                <IsShipperOwned>true</IsShipperOwned>
                <LengthUnit Description=""Feet"">FT</LengthUnit>
                <Link>1</Link>
                <NonOperatingReefer>false</NonOperatingReefer>
                <OverhangBack>0</OverhangBack>
                <OverhangFront>0</OverhangFront>
                <OverhangHeight>0</OverhangHeight>
                <OverhangLeft>0</OverhangLeft>
                <OverhangRight>0</OverhangRight>
                <Seal>Seal 1</Seal>
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
                <Type>BillRequiredBy</Type>
                <Value>2018-10-01T00:00:00</Value>
              </Date>
            </DateCollection>

            <NoteCollection>
              <Note>
                <Description>Special Instructions</Description>
                <NoteText>consol special instructions</NoteText>
              </Note>
              <Note>
                <Description>Freight Payable At</Description>
                <NoteText>CNNBO</NoteText>
              </Note>
            </NoteCollection>

            <OrganizationAddressCollection>
              <OrganizationAddress>
                <AddressType>BookingPartyDocumentaryAddress</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 200</Address1>
                <Address2>55 Why Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Conficious Ave</City>
                <CompanyName>I'm Sending Stuff</CompanyName>
                <Contact></Contact>
                <Country Name=""China"">CN</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Nanjing"">CNNJI</Port>
                <Postcode>10000</Postcode>
                <State></State>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>ConsignorDocumentaryAddress</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 200</Address1>
                <Address2>55 Why Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Conficious Ave</City>
                <CompanyName>I'm Sending Stuff</CompanyName>
                <Contact></Contact>
                <Country Name=""China"">CN</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Nanjing"">CNNJI</Port>
                <Postcode>10000</Postcode>
                <State></State>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>ConsigneeDocumentaryAddress</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 399</Address1>
                <Address2>50 What Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Sydney</City>
                <CompanyName>I'm Receiving Stuff</CompanyName>
                <Contact></Contact>
                <Country Name=""Australia"">AU</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Sydney"">AUSYD</Port>
                <Postcode>5023</Postcode>
                <State>NSW</State>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 13</Address1>
                <Address2>4 Lost Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Aalborg</City>
                <CompanyName>MAERSK</CompanyName>
                <Contact></Contact>
                <Country Name=""Denmark"">DK</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Aalborg"">DKAAL</Port>
                <Postcode>2000</Postcode>
                <State></State>

                <RegistrationNumberCollection>
                  <RegistrationNumber>
                    <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                    <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                    <Value>CARCCC</Value>
                  </RegistrationNumber>
                </RegistrationNumberCollection>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>NotifyParty</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 888</Address1>
                <Address2>8 What Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Auckland</City>
                <CompanyName>I'm Notifying 1</CompanyName>
                <Contact></Contact>
                <Country Name=""New Zealand"">NZ</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Auckland"">NZAKL</Port>
                <Postcode>5012</Postcode>
                <State>AUK</State>
              </OrganizationAddress>
              <OrganizationAddress>
                <AddressType>NotifyParty2</AddressType>
                <AdditionalAddressInformation></AdditionalAddressInformation>
                <Address1>Unit 2</Address1>
                <Address2>60 What Lane</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Sydney</City>
                <CompanyName>I'm Notifying 2</CompanyName>
                <Contact></Contact>
                <Country Name=""Australia"">AU</Country>
                <Email></Email>
                <Fax></Fax>
                <GovRegNum></GovRegNum>
                <Phone></Phone>
                <Port Name=""Sydney"">AUSYD</Port>
                <Postcode>2023</Postcode>
                <State>NSW</State>
              </OrganizationAddress>
            </OrganizationAddressCollection>

            <PaymentHandlingInstructionCollection>
              <PaymentHandlingInstruction>
                <Category Description=""Freight"">FRT</Category>
                <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
              </PaymentHandlingInstruction>
            </PaymentHandlingInstructionCollection>

            <SubShipmentCollection>
              <SubShipment>
                <DataContext>
                  <DataSource>
                    <Key>ExportRef001</Key>
                    <Type>Booking</Type>
                  </DataSource>

                  <DocumentaryOverride>
                    <Purpose Description=""Original"">ORG</Purpose>
                  </DocumentaryOverride>
                </DataContext>

                <BookingConfirmationReference>ExportRef001</BookingConfirmationReference>

                <PackingLineCollection>
                  <PackingLine>
                    <Commodity Description=""General"">GEN</Commodity>
                    <ContainerLink>1</ContainerLink>
                    <ContainerNumber></ContainerNumber>
                    <DetailedDescription>pack1-1</DetailedDescription>
                    <ExportReferenceNumber>ExportRef001</ExportReferenceNumber>
                    <GoodsDescription>pack1-1</GoodsDescription>
                    <HarmonisedCode>HS11</HarmonisedCode>
                    <Height>0</Height>
                    <ImportReferenceNumber></ImportReferenceNumber>
                    <Length>0</Length>
                    <LengthUnit Description=""Meters"">M</LengthUnit>
                    <MarksAndNos>marks &amp; numbers S1</MarksAndNos>
                    <OutturnComment></OutturnComment>
                    <PackingLineID></PackingLineID>
                    <PackQty>11</PackQty>
                    <PackType Description=""Pallet"">PLT</PackType>
                    <ReferenceNumber></ReferenceNumber>
                    <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                    <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                    <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
                    <RequiresTemperatureControl>false</RequiresTemperatureControl>
                    <Volume>311</Volume>
                    <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                    <Weight>111</Weight>
                    <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                    <Width>0</Width>

                    <ClassificationCollection>
                      <Classification>
                        <Code>1234</Code>
                        <Country Name=""Singapore"">SG</Country>
                        <Type Description=""Harmonized Code"">HSC</Type>
                      </Classification>
                    </ClassificationCollection>

                    <UNDGCollection>
                      <UNDG>
                        <Contact>
                          <FullName>Handsome</FullName>
                          <Email>BLBLA0NOIXP57W5L8P610U6DP6IF3A3R6OWGLNJQ5TEGQKQGI813QVAPO75C</Email>
                          <Phone>1234567</Phone>
                        </Contact>
                        <ExceptedQuantityCode></ExceptedQuantityCode>
{flashpoint}
                        <IMOClass></IMOClass>
                        <MarinePollutant>N</MarinePollutant>
                        <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                        <PackingGroup>GRO</PackingGroup>
                        <PackQty>5</PackQty>
                        <PackType Description=""Bag"">BAG</PackType>
                        <ProperShippingName>DG SHIPPER NAME</ProperShippingName>
                        <Standard>IMO</Standard>
                        <SubLabel1>sub1</SubLabel1>
                        <SubLabel2>su2</SubLabel2>
                        <TechicalName>WHATEVER</TechicalName>
                        <UNDGCode>6666</UNDGCode>
                        <Volume>2</Volume>
                        <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
                        <Weight>200</Weight>
                        <WeightUQ Description=""Kilograms"">KG</WeightUQ>
                      </UNDG>
                    </UNDGCollection>
                  </PackingLine>
                </PackingLineCollection>
              </SubShipment>
              <SubShipment>
                <DataContext>
                  <DataSource>
                    <Key>ExportRef002</Key>
                    <Type>Booking</Type>
                  </DataSource>

                  <DocumentaryOverride>
                    <Purpose Description=""Original"">ORG</Purpose>
                  </DocumentaryOverride>
                </DataContext>

                <BookingConfirmationReference>ExportRef002</BookingConfirmationReference>

                <PackingLineCollection>
                  <PackingLine>
                    <Commodity Description=""General"">GEN</Commodity>
                    <ContainerLink>1</ContainerLink>
                    <ContainerNumber></ContainerNumber>
                    <DetailedDescription>pack1-2</DetailedDescription>
                    <ExportReferenceNumber>ExportRef002</ExportReferenceNumber>
                    <GoodsDescription>pack1-2</GoodsDescription>
                    <HarmonisedCode>HS12</HarmonisedCode>
                    <Height>0</Height>
                    <ImportReferenceNumber></ImportReferenceNumber>
                    <Length>0</Length>
                    <LengthUnit Description=""Meters"">M</LengthUnit>
                    <MarksAndNos>marks &amp; numbers S1</MarksAndNos>
                    <OutturnComment></OutturnComment>
                    <PackingLineID></PackingLineID>
                    <PackQty>12</PackQty>
                    <PackType Description=""Pallet"">PLT</PackType>
                    <ReferenceNumber></ReferenceNumber>
                    <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                    <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                    <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
                    <RequiresTemperatureControl>false</RequiresTemperatureControl>
                    <Volume>312</Volume>
                    <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                    <Weight>112</Weight>
                    <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                    <Width>0</Width>

                    <ClassificationCollection>
                    </ClassificationCollection>

                    <UNDGCollection>
                    </UNDGCollection>
                  </PackingLine>
                </PackingLineCollection>
              </SubShipment>
            </SubShipmentCollection>
          </SubShipment>
        </SubShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		ForwardingConsol CreateVoyageRelatedConsol(JobSailing sailing, ZString consolRef)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolRef;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			return consol;
		}

		ForwardingConsol CreateConsolForSendingMessage(JobSailing sailing, ZString consolRef, bool isCombustible)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_NoOriginalBills = 11;
			consol.JK_NoCopyBills = 22;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_BookingReference = "BookingRef001";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);
			consol.JK_MasterBillNum = "BookingRef001";
			consol.JK_AgentsReference = "AgentRef001";
			consol.JK_UniqueConsignRef = consolRef;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			PopulateAddresses(consol);

			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNNBO";
			transport.JW_RL_NKDiscPort = "AUSYD";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "F9999";

			var carrierContractNumber = consol.Numbers.AddNew();
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "CarrierContractNumber001";

			var consolInstruction = consol.Notes.AddNew();
			consolInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consolInstruction.ST_NoteText = "consol special instructions";

			#region Containers

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_ISOType = "22P1";

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_ISOType = "40RE";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";
			container1.JC_DeliveryMode = "CFS/CY";
			container1.JC_IsShipperOwned = true;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 1000;
			container1.JC_RC = refContainer1.PK;
			container1.JC_SealNum = "Seal 1";
			container1.JC_ContainerQuality = "FOD";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_IsShipperOwned = false;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 2000;
			container2.JC_DunnageWeight = 2000;
			container2.JC_RC = refContainer2.PK;
			container2.JC_IsControlledAtmosphere = true;
			container2.JC_SetPointTemp = -18.0m;
			container2.JC_SetPointTempUnit = "C";
			container2.JC_HumidityPercent = 50;
			container2.JC_AirVentFlow = 90m;
			container2.JC_AirVentFlowRateUnit = AirFlowRateUnits.Codes.Percent;
			container2.JC_SealNum = "Seal 2";
			container2.JC_ContainerQuality = "FOD";

			#endregion

			#region Shipment

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001000011";
			shipment.JS_HouseBill = "HBL 001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description S1";
			shipment.JS_MarksAndNumbers = "marks & numbers S1";
			shipment.JS_BookingReference = "BKG00000S1";
			shipment.JS_NoOriginalBills = 3;
			shipment.JS_NoCopyBills = 3;

			PopulateShipmentAddresses(shipment);

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var shipmentInstruction = shipment.Notes.AddNew();
			shipmentInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			shipmentInstruction.ST_NoteText = "shipment special instructions";

			var number = shipment.Numbers.AddNew();
			number.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			number.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			number.CE_EntryNum = "ShippingOrderNumber001";

			var packline11 = shipment.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 11;
			packline11.JL_F3_NKPackType = "PLT";
			packline11.JL_ActualWeight = 111;
			packline11.JL_ActualWeightUQ = "KG";
			packline11.JL_ActualVolume = 311;
			packline11.JL_ActualVolumeUQ = "M3";
			packline11.JL_HarmonisedCode = "HS11";
			packline11.JL_DetailedDescription = "pack1-1";
			packline11.JL_ExportRefNumber = "ExportRef001";

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "SG";
			hc.JLH_Code = "1234";

			packline11.HarmonisedCodes.Add(hc);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = packline11.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = isCombustible;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var packline12 = shipment.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 12;
			packline12.JL_F3_NKPackType = "PLT";
			packline12.JL_ActualWeight = 112;
			packline12.JL_ActualWeightUQ = "KG";
			packline12.JL_ActualVolume = 312;
			packline12.JL_ActualVolumeUQ = "M3";
			packline12.JL_HarmonisedCode = "HS12";
			packline12.JL_DetailedDescription = "pack1-2";
			packline12.JL_ExportRefNumber = "ExportRef002";

			container1.PackLines.Add(packline11);
			container1.PackLines.Add(packline12);

			#endregion

			return consol;
		}

		void PopulateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CARCCC", Constants.CountryCodes.UnitedStates);

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "I'm Notifying 1";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying 2";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ETerminalReleaseMessage(Factory.New<JobVoyage>());
		}
	}

	sealed class DummyProgressNotifications : IProgressNotifications
	{
		public ICollection<INotification> Notifications
		{
			get { return notifications ?? (notifications = new List<INotification>()); }
		}

		List<INotification> notifications;

		public void Add(INotification notification)
		{
			Notifications.Add(notification);
		}

		public void Notify(INotificationType notificationType, string message)
		{
			Notifications.Add(new Notification(notificationType, message));
		}

		public void NotifyFormat(INotificationType notificationType, string format, params object[] args)
		{
			Notifications.Add(new Notification(notificationType, string.Format(format, args)));
		}

		public void BumpProgress()
		{
		}

		public void SetProgressMax(int max)
		{
		}

		public new string ToString()
		{
			return string.Join(System.Environment.NewLine, Notifications.Select(x => x.Message));
		}
	}
}
