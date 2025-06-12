using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	public class TestingMessage : IBaseMessage
	{
		// Message Content Properties
		public string DocumentName = "Testing";
		public string UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/TestingNamespace/1";
		public string SCACUniShip = "TST";
		public string SCACCoLoadUniShip = "TST";
		public string ActionPurpose = "TST";
		public string ShipmentType = "";
		public string Port = "";
		public string PortCode = "TST";
		public string CarrierHandlingAgent = "";
		public string CarrierBookingAgent = "";
		public string RecipientCodeCHA_ENP = "";
		public string RecipientCodeCHA_NGB = "";
		public string RecipientCodeCBA_ENP = "";
		public string RecipientCodeSI_ENP = "";
		public string RecipientCodeSI_NGB = "";
		public string VGMMultipleRecipientsSplit = "";
		public string Purpose = "TST";

		// Original Data Wrapper Properties
		public string SenderID { get; set; }
		public string RecipientID { get; set; }

		// Other
		MessageFactory messageFactory = new MessageFactory();
		IBaseMessage message;

		public TestingMessage(string from, string to)
		{
			Init(from, to);
		}

		public TestingMessage(string from, string to, string content)
		{
			Init(from, to);
			ContentData = content;
		}

		private void Init(string from, string to)
		{
			this.SenderID = from;
			this.RecipientID = to;
			message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
		}

		/// <summary>
		/// Must call Reset before reusing this message for another evaluating Routing Rule.
		/// </summary>
		public void ClearMessageBodyCache()
		{
			ContentData = null;
			message.BodyPart.Data = null;
		}

		public void AddPart(string partName, IBaseMessagePart part, bool bBody)
		{
			message.AddPart(partName, part, bBody);
			message.Context = messageFactory.CreateMessageContext();
		}

		public IBaseMessagePart BodyPart
		{
			get
			{
				if(message.BodyPart.Data == null)
				{
					message.BodyPart.Data = OriginalDataStream;
				}
				return message.BodyPart;
			}
		}

		public string BodyPartName
		{
			get { return message.BodyPartName; }
		}

		public IBaseMessageContext Context
		{
			get { return message.Context; }
			set { message.Context = value; }
		}

		public Exception GetErrorInfo()
		{
			return message.GetErrorInfo();
		}

		public IBaseMessagePart GetPart(string partName)
		{
			return message.GetPart(partName);
		}

		public IBaseMessagePart GetPartByIndex(int index, out string partName)
		{
			return message.GetPartByIndex(index, out partName);
		}

		public void GetSize(out ulong lSize, out bool fImplemented)
		{
			message.GetSize(out lSize, out fImplemented);
		}

		public bool IsMutable
		{
			get { return message.IsMutable; }
		}

		public Guid MessageID
		{
			get { return message.MessageID; }
		}

		public int PartCount
		{
			get { return message.PartCount; }
		}

		public void RemovePart(string partName)
		{
			message.RemovePart(partName);
		}

		public void SetErrorInfo(Exception errInfo)
		{
			message.SetErrorInfo(errInfo);
		}

		#region ContentData

		public string ContentData
		{
			get
			{
				return !string.IsNullOrEmpty(contentData) ? contentData : contentData = string.Format(
				
@"<UniversalInterchange version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Body>
		<UniversalShipment version=""2.0"" xmlns=""{0}"">
			<Shipment>
				<DataContext>
					<DataSource>
						<DataProvider Type=""EnterpriseID"">HYEUATBNE</DataProvider>
						<Key>C00678696</Key>
						<Type>ForwardingConsol</Type>
					</DataSource>
					<DocumentaryOverride>
						<DataVersion>1</DataVersion>
						<DocumentName>{1}</DocumentName>
						<IsSystemDefined>true</IsSystemDefined>
						<Purpose>{16}</Purpose>
						<SubmissionVersion>4</SubmissionVersion>
					</DocumentaryOverride>
					<Workflow>
						<ActionPurpose Description=""As Per Payload"">{2}</ActionPurpose>
						<Company>
							<Code>BNE</Code>
							<Country Name=""Australia"">AU</Country>
							<Name>AU Demo Company - BNE</Name>
						</Company>
						<EventBranch Name=""Another Demo Company"">BN1</EventBranch>
						<EventDepartment Name=""Branch"">BRN</EventDepartment>
						<EventType/>
						<EventUser Name=""CargoWise One Support"">E</EventUser>
						<TriggerCount>1</TriggerCount>
						<TriggerDate>2016-08-08T17:15:44.42</TriggerDate>
						<TriggerDescription/>
						<TriggerType>Manual</TriggerType>
					</Workflow>
					<DataTargetCollection>
					  <DataTarget>
						<Key>{13}</Key>
						<Type>VGMMultipleRecipientsSplit</Type>
					  </DataTarget>
					</DataTargetCollection>
				</DataContext>
				<ShipmentType Description=""Co-Load"">{3}</ShipmentType>
				<AddInfoCollection>
					<AddInfo>
						<Key>OperationalPort_Code</Key>
						<Value>{17}</Value>
					</AddInfo>
				</AddInfoCollection>
				{4}{5}
				<OrganizationAddressCollection>
					<OrganizationAddress>
						<AddressType>ShippingLineAddress</AddressType>
						<Address1>26-32 PIRRAMA ROAD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>123 CHAPEL STREET</AddressShortCode>
						<City>PYRMONT</City>
						<CompanyName>CMA CGM</CompanyName>
						<Contact/>
						<Country Name=""Australia"">AU</Country>
						<Email/>
						<Fax/>
						<GovRegNum>52312161908</GovRegNum>
						<GovRegNumType Description=""Australian Business Number (GST Reg"">ABN</GovRegNumType>
						<OrganizationCode>CMACGM_AU</OrganizationCode>
						<Phone/>
						<Port Name=""Sydney"">{18}</Port>
						<Postcode>2009</Postcode>
						<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
						<State>NSW</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type Description=""Standard Carrier Alpha Code"">CCC</Type>
								<CountryOfIssue Name=""United States"">US</CountryOfIssue>
								<Value>{6}</Value>
							</RegistrationNumber>
							<RegistrationNumber>
								<Type Description=""INTTRA Code"">INT</Type>
								<CountryOfIssue Name=""Australia"">AU</CountryOfIssue>
								<Value>CMDU</Value>
							</RegistrationNumber>
							<RegistrationNumber>
								<Type Description=""1-Stop Trading Code"">ENP</Type>
								<CountryOfIssue Name=""Australia"">CN</CountryOfIssue>
								<Value>{8}</Value>
							</RegistrationNumber>
							<RegistrationNumber>
								<Type Description=""Ningbo Client ID"">NGB</Type>
								<CountryOfIssue Name=""China"">CN</CountryOfIssue>
								<Value>{9}</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</OrganizationAddress>
				<OrganizationAddress>
					<AddressType>CarrierHandlingAgent</AddressType>
					<AdditionalAddressInformation/>
					<Address1>NO. 355 FUTEXIYI ROAD ROOM 668</Address1>
					<Address2>PUDONG NEW AREA FREE TRADE DISTRICT</Address2>
					<AddressOverride>false</AddressOverride>
					<City>SHANGHAI</City>
					<CompanyName>SHANGHAI BROKER</CompanyName>
					<Contact>Agent Contact</Contact>
					<Country Name=""China"">CN</Country>
					<Email/>
					<Fax>+862161063335</Fax>
					<GovRegNum/>
					<Phone>+862161600156</Phone>
					<Port Name=""Shanghai Hongqiao International Apt"">CNSHA</Port>
					<Postcode>210131</Postcode>
					<State>Shanghai</State>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type Description=""Easipass Client ID"">ENP</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>{10}</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<Type Description=""Ningbo Client ID"">NGB</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>{11}</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<Type Description=""CargoWiseOne Carrier Code"">C1C</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>{14}</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<Type Description=""CargoWiseOne Carrier Code"">C2C</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>C2C</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
					</OrganizationAddress>
					<OrganizationAddress>
						<AddressType>CoLoadWith</AddressType>
						<Address1>DOLPHIN COVE BUSINESS PARK</Address1>
						<Address2>10 FREIGHT LANE</Address2>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>DOLPHIN COVE BUSINESS PAR</AddressShortCode>
						<City>Mangere</City>
						<CompanyName>Fast Freight Forwarders Pty Ltd</CompanyName>
						<Contact/>
						<Country Name=""New Zealand"">NZ</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>FASFREAKL</OrganizationCode>
						<Phone/>
						<Port Name=""Auckland"">NZAKL</Port>
						<Postcode>1009</Postcode>
						<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
						<State>AUK</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type Description=""Standard Carrier Alpha Code"">CCC</Type>
								<CountryOfIssue Name=""United States"">US</CountryOfIssue>
								<Value>{7}</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</OrganizationAddress>
				<OrganizationAddress>
					<AddressType>CarrierBookingAgent</AddressType>
					<AdditionalAddressInformation/>
					<Address1>NO. 355 FUTEXIYI ROAD ROOM 668</Address1>
					<Address2>PUDONG NEW AREA FREE TRADE DISTRICT</Address2>
					<AddressOverride>false</AddressOverride>
					<City>SHANGHAI</City>
					<CompanyName>SHANGHAI BROKER</CompanyName>
					<Contact>Agent Contact</Contact>
					<Country Name=""China"">CN</Country>
					<Email/>
					<Fax>+862161063335</Fax>
					<GovRegNum/>
					<Phone>+862161600156</Phone>
					<Port Name=""Shanghai Hongqiao International Apt"">CNSHA</Port>
					<Postcode>210131</Postcode>
					<State>Shanghai</State>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type Description=""Easipass Client ID"">ENP</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>{12}</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<Type Description=""CargoWiseOne Carrier Code"">C1C</Type>
							<CountryOfIssue Name=""China"">CN</CountryOfIssue>
							<Value>{15}</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
					</OrganizationAddress>
					<OrganizationAddress>
						<AddressType>CoLoadWith</AddressType>
						<Address1>DOLPHIN COVE BUSINESS PARK</Address1>
						<Address2>10 FREIGHT LANE</Address2>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>DOLPHIN COVE BUSINESS PAR</AddressShortCode>
						<City>Mangere</City>
						<CompanyName>Fast Freight Forwarders Pty Ltd</CompanyName>
						<Contact/>
						<Country Name=""New Zealand"">NZ</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>FASFREAKL</OrganizationCode>
						<Phone/>
						<Port Name=""Auckland"">NZAKL</Port>
						<Postcode>1009</Postcode>
						<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
						<State>AUK</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type Description=""Standard Carrier Alpha Code"">CCC</Type>
								<CountryOfIssue Name=""United States"">US</CountryOfIssue>
								<Value>{7}</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</OrganizationAddress>
				</OrganizationAddressCollection>
			</Shipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>",
					// 0					1				2				3			4			5			6				7					8					9					10						11					12						13							14					15					16	  17	18
				UShipmentNamespace, DocumentName, ActionPurpose, ShipmentType, Containers, SubShipments, SCACUniShip, SCACCoLoadUniShip, RecipientCodeSI_ENP, RecipientCodeSI_NGB, RecipientCodeCHA_ENP, RecipientCodeCHA_NGB, RecipientCodeCBA_ENP, VGMMultipleRecipientsSplit, CarrierHandlingAgent, CarrierBookingAgent, Purpose, Port, PortCode);
			}
			set { contentData = value; }
		}

		string contentData;

		public string Containers = @"<ContainerCollection>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MAEU0494852</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DunnageWeight>0</DunnageWeight>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>20240</GrossWeight>
        <GrossWeightVerificationDateTime>2018-05-21T09:30:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description='Method 2 - Packages'>PKG</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <Seal>3659788</Seal>
        <SealPartyType Description='Carrier/Shipping Line'>CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>2280</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <WeightUnit Description='Kilograms'>KG</WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <Address1>WESTINGHOUSE ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>MANCHESTER</City>
            <CompanyName>YOUR UNITED KINGDOM COMPANY (MANCHESTER)</CompanyName>
            <Contact>Operations</Contact>
            <Country Name='United Kingdom'>GB</Country>
            <Email>operations.gbmnc@youragent.com</Email>
            <Fax>+441618722222</Fax>
            <GovRegNum></GovRegNum>
            <Phone>+441618721111</Phone>
            <Port Name='Manchester'>GBMNC</Port>
            <Postcode>M17 1DP</Postcode>
            <State>Greater Manchester</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description='Legacy System Code'>LSC</Type>
                <CountryOfIssue Name='United Kingdom'>GB</CountryOfIssue>
                <Value>MNC</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
</ContainerCollection>";

		public string SubShipments = @"<SubShipmentCollection>
                    <SubShipment>
                        <DataContext>
                            <DataSource>
                                <Key>973808566/A</Key>
                                <Type>Booking</Type>
                            </DataSource>

                            <Workflow>
                                <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
                            </Workflow>
                        </DataContext>

                        <BookingConfirmationReference>973808566/A</BookingConfirmationReference>

                        <PackingLineCollection>
                            <PackingLine>
                                <ContainerLink>1</ContainerLink>
                                <ExportReferenceNumber>973808566/A</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>18545564</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>10</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>5.3</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>5000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                    <UNDG>
                                        <Contact>
                                            <FullName>Accounts</FullName>
                                            <Phone></Phone>
                                        </Contact>
                                        <FlashPoint>17.0</FlashPoint>
                                        <IMOClass>3</IMOClass>
                                        <MarinePollutant></MarinePollutant>
                                        <PackingGroup>II</PackingGroup>
                                        <PackQty>0</PackQty>
                                        <PackType></PackType>
                                        <ProperShippingName>TRIISOPROPYL BORATE</ProperShippingName>
                                        <SubLabel1></SubLabel1>
                                        <SubLabel2></SubLabel2>
                                        <TechicalName>TECHNICAL NAME</TechicalName>
                                        <UNDGCode>2616</UNDGCode>
                                        <Volume>0</Volume>
                                        <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
                                        <Weight>0</Weight>
                                        <WeightUQ Description=""Kilograms"">KG</WeightUQ>
                                    </UNDG>
                                </UNDGCollection>
                            </PackingLine>
                            <PackingLine>
                                <ContainerLink>2</ContainerLink>
                                <ExportReferenceNumber>973808566/A</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>8788895</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>5</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>2.6</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>3000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                </UNDGCollection>
                            </PackingLine>
                        </PackingLineCollection>
                    </SubShipment>
                  </SubShipmentCollection>";

		#endregion

		#region Original Data

		Stream OriginalDataStream
		{
			get
			{
				return GenerateStreamFromString(string.Format(@"<TypedPolling xmlns=""http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus"">
  <TypedPollingResultSet0>
    <TypedPollingResultSet0>
      <InboxPK>8fb1e522-5d6b-418e-86bc-f8acb103a9e8</InboxPK>
      <MessageTrackingID>e291e463-331a-4dfd-9aaf-68523e8c825c</MessageTrackingID>
      <EnvelopeTrackingID>cfa3d48d-dd71-4bbd-838b-721a5aafed78</EnvelopeTrackingID>
      <SenderID>{0}</SenderID>
      <RecipientID>{1}</RecipientID>
      <SourceMessageType/>
      <IsFlatFile>false</IsFlatFile>
      <EmailSubject>EMAIL SUBJECT</EmailSubject>
      <FileName>FILE_NAME</FileName>
      <Content>{2}</Content>
    </TypedPollingResultSet0>
  </TypedPollingResultSet0>
</TypedPolling>", SenderID, RecipientID, Zip(ContentData)));
			}
		}

		static Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		static string Zip(string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);

			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(mso, CompressionMode.Compress))
				{
					msi.CopyTo(gs);
				}

				return Convert.ToBase64String(mso.ToArray());
			}
		}

		#endregion

		#region Test Data

		public const string TwoContainersString = @"<ContainerCollection>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount> 
        <ContainerNumber>MAEU0494852</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>First Container</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DunnageWeight>0</DunnageWeight>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>20240</GrossWeight>
        <GrossWeightVerificationDateTime>2018-05-21T09:30:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description='Method 2 - Packages'>PKG</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <Seal>3659788</Seal>
        <SealPartyType Description='Carrier/Shipping Line'>CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>2280</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <WeightUnit Description='Kilograms'>KG</WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <Address1>WESTINGHOUSE ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>MANCHESTER</City>
            <CompanyName>YOUR UNITED KINGDOM COMPANY (MANCHESTER)</CompanyName>
            <Contact>Operations</Contact>
            <Country Name='United Kingdom'>GB</Country>
            <Email>operations.gbmnc@youragent.com</Email>
            <Fax>+441618722222</Fax>
            <GovRegNum></GovRegNum>
            <Phone>+441618721111</Phone>
            <Port Name='Manchester'>GBMNC</Port>
            <Postcode>M17 1DP</Postcode>
            <State>Greater Manchester</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description='Legacy System Code'>LSC</Type>
                <CountryOfIssue Name='United Kingdom'>GB</CountryOfIssue>
                <Value>MNC</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MAEU1234021</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Second Container</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DunnageWeight>0</DunnageWeight>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>16900</GrossWeight>
        <GrossWeightVerificationDateTime>2018-05-21T09:00:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description='Method 1 - Container'>CNT</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <Seal>3423423</Seal>
        <SealPartyType Description='Carrier/Shipping Line'>CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>0</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>2280</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType></ThirdSealPartyType>
        <WeightUnit Description='Kilograms'>KG</WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation></AdditionalAddressInformation>
            <Address1>UNIT 14, LIDDALL WAY</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>WEST DRAYTON</City>
            <CompanyName>YOUR UNITED KINGDOM COMPANY (LONDON)</CompanyName>
            <Contact>Operations</Contact>
            <Country Name='United Kingdom'>GB</Country>
            <Email>operations.gblhr@youragent.com</Email>
            <Fax>+441899999998</Fax>
            <GovRegNum></GovRegNum>
            <Phone>+441899999999</Phone>
            <Port Name='Heathrow Apt/London'>GBLHR</Port>
            <Postcode>UB7 8PG</Postcode>
            <State>Greater London</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description='VAT Business Registration Number'>VAT</Type>
                <CountryOfIssue Name='United Kingdom'>GB</CountryOfIssue>
                <Value>945390992</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>";

		public string ThreeSubShipmentsString = @"<SubShipmentCollection>
                    <SubShipment>
                        <DataContext>
                            <DataSource>
                                <Key>973808566/A</Key>
                                <Type>Booking</Type>
                            </DataSource>

                            <Workflow>
                                <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
                            </Workflow>
                        </DataContext>

                        <BookingConfirmationReference>973808566/A</BookingConfirmationReference>

                        <PackingLineCollection>
                            <PackingLine>
                                <ContainerLink>1</ContainerLink>
                                <ExportReferenceNumber>973808566/A</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>18545564</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>10</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>5.3</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>5000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                    <UNDG>
                                        <Contact>
                                            <FullName>Accounts</FullName>
                                            <Phone></Phone>
                                        </Contact>
                                        <FlashPoint>17.0</FlashPoint>
                                        <IMOClass>3</IMOClass>
                                        <MarinePollutant></MarinePollutant>
                                        <PackingGroup>II</PackingGroup>
                                        <PackQty>0</PackQty>
                                        <PackType></PackType>
                                        <ProperShippingName>TRIISOPROPYL BORATE</ProperShippingName>
                                        <SubLabel1></SubLabel1>
                                        <SubLabel2></SubLabel2>
                                        <TechicalName>TECHNICAL NAME</TechicalName>
                                        <UNDGCode>2616</UNDGCode>
                                        <Volume>0</Volume>
                                        <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
                                        <Weight>0</Weight>
                                        <WeightUQ Description=""Kilograms"">KG</WeightUQ>
                                    </UNDG>
                                </UNDGCollection>
                            </PackingLine>
                            <PackingLine>
                                <ContainerLink>2</ContainerLink>
                                <ExportReferenceNumber>973808566/A</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>8788895</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>5</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>2.6</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>3000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                </UNDGCollection>
                            </PackingLine>
                        </PackingLineCollection>
                    </SubShipment>
                    <SubShipment>
                        <DataContext>
                            <DataSource>
                                <Key>973808566/B</Key>
                                <Type>Booking</Type>
                            </DataSource>

                            <Workflow>
                                <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
                            </Workflow>
                        </DataContext>

                        <BookingConfirmationReference>973808566/B</BookingConfirmationReference>

                        <PackingLineCollection>
                            <PackingLine>
                                <ContainerLink>1</ContainerLink>
                                <ExportReferenceNumber>973808566/B</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>87955225</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>3</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>2.1</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>2000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                    <UNDG>
                                        <Contact>
                                            <FullName>Accounts</FullName>
                                            <Phone></Phone>
                                        </Contact>
                                        <FlashPoint>17.0</FlashPoint>
                                        <IMOClass>3</IMOClass>
                                        <MarinePollutant Description=""Severe Marine Pollutant"">S</MarinePollutant>
                                        <PackingGroup>II</PackingGroup>
                                        <PackQty>0</PackQty>
                                        <PackType></PackType>
                                        <ProperShippingName>TRIISOPROPYL BORATE</ProperShippingName>
                                        <SubLabel1></SubLabel1>
                                        <SubLabel2></SubLabel2>
                                        <TechicalName>TECHNICAL NAME</TechicalName>
                                        <UNDGCode>2616</UNDGCode>
                                        <Volume>0</Volume>
                                        <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
                                        <Weight>0</Weight>
                                        <WeightUQ Description=""Kilograms"">KG</WeightUQ>
                                    </UNDG>
                                </UNDGCollection>
                            </PackingLine>
                            <PackingLine>
                                <ContainerLink>2</ContainerLink>
                                <ExportReferenceNumber>973808566/B</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>8665656</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>12</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>6.7</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>7000</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                </UNDGCollection>
                            </PackingLine>
                        </PackingLineCollection>
                    </SubShipment>
                    <SubShipment>
                        <DataContext>
                            <DataSource>
                                <Key>973808566/C</Key>
                                <Type>Booking</Type>
                            </DataSource>

                            <Workflow>
                                <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
                            </Workflow>
                        </DataContext>

                        <BookingConfirmationReference>973808566/C</BookingConfirmationReference>

                        <PackingLineCollection>
                            <PackingLine>
                                <ContainerLink>2</ContainerLink>
                                <ExportReferenceNumber>973808566/C</ExportReferenceNumber>
                                <GoodsDescription>THE GOODS DESCRIPTION</GoodsDescription>
                                <HarmonisedCode>86454552</HarmonisedCode>
                                <ImportReferenceNumber></ImportReferenceNumber>
                                <MarksAndNos>THE MARKS AND NUMBERS</MarksAndNos>
                                <PackQty>7</PackQty>
                                <PackType Description=""Pallet"">PLT</PackType>
                                <ReferenceNumber></ReferenceNumber>
                                <Volume>3.6</Volume>
                                <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
                                <Weight>3500</Weight>
                                <WeightUnit Description=""Kilograms"">KG</WeightUnit>

                                <UNDGCollection>
                                </UNDGCollection>
                            </PackingLine>
                        </PackingLineCollection>
                    </SubShipment>
                </SubShipmentCollection>";

		#endregion
	}
}