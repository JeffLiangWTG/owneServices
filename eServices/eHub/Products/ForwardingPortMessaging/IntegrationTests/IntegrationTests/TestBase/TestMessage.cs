using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.IntegrationTests
{
	public class TestingMessage : IBaseMessage
	{
		private readonly string from;
		private readonly string to;
		public string DocumentName = "Container Load Plan";
		public string ShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerLoadPlan/1";
		public string ActionPurpose = "APP";
		public string OperationalPort = "";
		public string Port = "CNNGB";
		public string CarrierAgent = "ABCD";

		public TestingMessage(string from, string to)
		{
			this.from = from;
			this.to = to;
			Initial();
		}

		public TestingMessage(string from, string to, string content)
		{
			this.from = from;
			this.to = to;
			Initial();
			ContentData = content;
		}

		public virtual void Initial()
		{
			Context.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", from);
			Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", to);
		}

		/// <summary>
		/// Must call Reset before reusing this message for another evaluating Routing Rule.
		/// </summary>
		public void Reset()
		{
			context = null;
			bodyPart = null;
			ContentData = null;
			Initial();
		}

		public void AddPart(string partName, IBaseMessagePart part, bool bBody)
		{
			throw new NotImplementedException();
		}

		IBaseMessagePart bodyPart;

		public IBaseMessagePart BodyPart
		{
			get { return bodyPart ?? (bodyPart = new TestingMessagePart(ContentData)); }
		}

		public string BodyPartName
		{
			get { throw new NotImplementedException(); }
		}

		IBaseMessageContext context;

		public IBaseMessageContext Context
		{
			get { return context ?? (context = new TestingMessageContext()); }
			set { throw new NotImplementedException(); }
		}

		public Exception GetErrorInfo()
		{
			throw new NotImplementedException();
		}

		public IBaseMessagePart GetPart(string partName)
		{
			throw new NotImplementedException();
		}

		public IBaseMessagePart GetPartByIndex(int index, out string partName)
		{
			throw new NotImplementedException();
		}

		public void GetSize(out ulong lSize, out bool fImplemented)
		{
			throw new NotImplementedException();
		}

		public bool IsMutable
		{
			get { throw new NotImplementedException(); }
		}

		public Guid MessageID
		{
			get { throw new NotImplementedException(); }
		}

		public int PartCount
		{
			get { throw new NotImplementedException(); }
		}

		public void RemovePart(string partName)
		{
			throw new NotImplementedException();
		}

		public void SetErrorInfo(Exception errInfo)
		{
			throw new NotImplementedException();
		}

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
						<Purpose>SIN</Purpose>
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
				</DataContext>

				<AddInfoCollection>
          <AddInfo>
            <Key>PortOfTranship_Code</Key>
            <Value>CNSHA</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfTranship_Name</Key>
            <Value>Shanghai</Value>
          </AddInfo>
          <AddInfo>
            <Key>OperationalPort_Code</Key>
            <Value>{3}</Value>
          </AddInfo>
          <AddInfo>
            <Key>OperationalPort_Name</Key>
            <Value>Ningbo</Value>
          </AddInfo>
        </AddInfoCollection>
				
				<ShipmentType Description=""Co-Load""></ShipmentType>
				{4}
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
						<Port Name=""Sydney"">AUSYD</Port>
						<Postcode>2009</Postcode>
						<ScreeningStatus Description=""Unknown"">UNK</ScreeningStatus>
						<State>NSW</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type Description=""Standard Carrier Alpha Code"">CCC</Type>
								<CountryOfIssue Name=""United States"">US</CountryOfIssue>
								<Value></Value>
							</RegistrationNumber>
							<RegistrationNumber>
								<Type Description=""INTTRA Code"">INT</Type>
								<CountryOfIssue Name=""Australia"">AU</CountryOfIssue>
								<Value>CMDU</Value>
							</RegistrationNumber>
							<RegistrationNumber>
								<Type Description=""1-Stop Trading Code"">1ST</Type>
								<CountryOfIssue Name=""Australia"">AU</CountryOfIssue>
								<Value>CMA</Value>
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
								<Value></Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</OrganizationAddress>
				</OrganizationAddressCollection>
			</Shipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>", ShipmentNamespace, DocumentName, ActionPurpose, OperationalPort, Containers);
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
      <Container>
        <AirVentFlow>0</AirVentFlow>
        <AirVentFlowRateUnit></AirVentFlowRateUnit>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MAEU1234021</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
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

		public class TestingMessageContext : IBaseMessageContext
		{
			private List<ContextProperty> MessageContext;

			public TestingMessageContext()
			{
				MessageContext = new List<ContextProperty>();
			}

			public object Read(string strName, string strNamespace)
			{
				foreach (var contextItem in MessageContext)
				{
					if (HasContextItem(strName, strNamespace, contextItem))
					{
						return contextItem.Value;
					}
				}

				return null;
			}

			public void Write(string strName, string strNameSpace, object obj)
			{
				foreach (var contextItem in MessageContext)
				{
					if (HasContextItem(strName, strNameSpace, contextItem))
					{
						contextItem.Value = obj;
						return;
					}
				}

				MessageContext.Add(new ContextProperty(strName, strNameSpace, obj));
			}

			bool HasContextItem(string contextItemName, string contextItemNamespace, ContextProperty contextItem)
			{
				return contextItem.Name == contextItemName && contextItem.Namespace == contextItemNamespace;
			}

			public void AddPredicate(string strName, string strNameSpace, object obj)
			{
				throw new System.NotImplementedException();
			}

			public uint CountProperties
			{
				get { throw new System.NotImplementedException(); }
			}

			public ContextPropertyType GetPropertyType(string strName, string strNameSpace)
			{
				throw new System.NotImplementedException();
			}

			public bool IsPromoted(string strName, string strNameSpace)
			{
				throw new System.NotImplementedException();
			}

			public void Promote(string strName, string strNameSpace, object obj)
			{
				throw new System.NotImplementedException();
			}

			public object ReadAt(int index, out string strName, out string strNamespace)
			{
				throw new System.NotImplementedException();
			}

			public class ContextProperty
			{
				public string Name { get; set; }
				public string Namespace { get; set; }
				public object Value { get; set; }

				public ContextProperty(string contextName, string contextNamespace, object contextValue)
				{
					Name = contextName;
					Namespace = contextNamespace;
					Value = contextValue;
				}
			}
		}

		public class TestingMessagePart : IBaseMessagePart
		{
			private readonly string contentData;

			public TestingMessagePart(string contentData)
			{
				this.contentData = contentData;
			}

			public string Charset
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public string ContentType
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			System.IO.Stream data;

			public System.IO.Stream Data
			{
				get
				{
					if (data == null)
					{
						data = new MemoryStream();
						StreamWriter writer = new StreamWriter(data);
						writer.Write(contentData);
						writer.Flush();
						data.Position = 0;
						return data;
					}
					return data;
				}
				set { throw new NotImplementedException(); }
			}

			public System.IO.Stream GetOriginalDataStream()
			{
				throw new NotImplementedException();
			}

			public void GetSize(out ulong lSize, out bool fImplemented)
			{
				throw new NotImplementedException();
			}

			public bool IsMutable
			{
				get { throw new NotImplementedException(); }
			}

			public Guid PartID
			{
				get { throw new NotImplementedException(); }
			}

			public IBasePropertyBag PartProperties
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}
		}
	}
}