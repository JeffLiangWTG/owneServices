using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingConsolValueObjectDataAdapter))]
	public class ForwardingConsolWithShipmentValueObjectDataAdapterTest : ForwardingConsolValueObjectDataAdapterTest
	{
		public void TestExportOnlyShipmentContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA256426";
			consol.Containers.AddNew();
			consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container1.PK;

			var adapter = new ForwardingConsolWithShipmentValueObjectDataAdapterForTest(shipment);
			var consolValue = new Xsd.Consol();
			adapter.ExportContainers(consol, consolValue, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, consolValue.ConsolDetail.Containers.Count);
			AssertEquals("AAAA256426", consolValue.ConsolDetail.Containers[0].ContainerNumber);
		}

		public void TestDEXEventIsNotAddedToConsol()
		{
			ForwardingShipment shipment = GetShipment("HB1");
			Consol.Shipments.Add(shipment);
			StmALog log = Consol.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNull("Precondition", log);

			NotificationBuffer notify = new NotificationBuffer();
			ForwardingConsolWithShipmentValueObjectDataAdapter adapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment);

			adapter.ExportToValueObject(Consol, new ValueObjectExportContext(notify));
			Factory.Save();

			log = Consol.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNull("DEX Event should not have been created", log);
		}

		[TestDate(2013, 11, 7, 17, 07, 15)]
		public void TestExportShipmentWithConsol()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = GetShipment("HB1");
			Consol.Shipments.Add(shipment);
			Consol.Shipments.Add(GetShipment("HB2"));

			shipment.Logs.MostRecentLogByEventTime(Events.Attached).Delete();

			NotificationBuffer notify = new NotificationBuffer();
			ForwardingConsolWithShipmentValueObjectDataAdapter adapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment);

			string fileName = Path.Combine(EnvProxy.Instance.TempPath, "ExportedFile.xml");
			try
			{
				using (Stream toFile = File.OpenWrite(fileName))
				{
					XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(adapter.ValueObjectType);
					serialiser.ExportXmlData(toFile, adapter, new BusinessObject[] { Consol }, new ValueObjectExportContext(notify));
				}
				Assert(!notify.HasErrors);

				using (StreamReader reader = new StreamReader(fileName))
				{
					AssertXMLEquals("Output should be the same"
						, XmlToCompare.Trim()
							.Replace(@"<User>C</User>", @"<User>" + GlbStaff.CurrentUser.GS_Code + @"</User>")
							.Replace(@"<UserName>Developer (C)</UserName>",
								@"<UserName>" + GlbStaff.CurrentUser.GS_FullName + " (" + GlbStaff.CurrentUser.GS_Code + @")</UserName>")
						, reader.ReadToEnd().Trim());
				}
			}
			finally
			{
				DeleteIfExists(fileName);
			}
		}

		#region Implementation

		ForwardingShipment GetShipment(ZString houseBill)
		{
			var result = Factory.New<ForwardingShipment>();
			result.JS_HouseBill = houseBill;

			var testOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "XLINDU"));

			if (testOrg == null || !testOrg.OH_IsConsignee || !testOrg.OH_IsConsignor)
			{
				Fail("This test has failed because Factory could not find the appropriate organization that was used in template XML.");
			}

			result.ConsigneePK = testOrg.PK;
			result.ConsignorPK = testOrg.PK;

			result.JS_RL_NKOrigin = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			result.JS_RL_NKDestination = "XXABC";

			return result;
		}

		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = NewBusinessObject();
					fConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					fConsol.JK_MasterBillNum = "masterbill";

					Transport transport = Consol.Transports[0];
					transport.JW_RL_NKLoadPort = "DEFRA";
					transport.JW_RL_NKDiscPort = "AUSYD";
					transport.JW_Vessel = "ADMIRALENGRACHT";
					transport.JW_VoyageFlight = "voyageno";
				}
				return fConsol;
			}
		}
		ForwardingConsol fConsol;

		#region Xml To Compare

		const string XmlToCompare = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>
    <Date>2013-11-07T17:07:15+02:00</Date>
    <XmlType>Verbose</XmlType>
    <Source>
      <EnterpriseCode>EDI</EnterpriseCode>
      <CompanyCode>EDI</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>CWSupport</LoginName>
    </Source>
    <Target />
    <EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS"">
      <OrganisationDetails>
        <Name>EDI CUSTOMS BROKERS</Name>
        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressLine2>ALBION  QLD</AddressLine2>
            <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
            <PostCode>4010</PostCode>
            <Language>EN</Language>
            <Location>AUBNE</Location>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
          <Address>
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressCode>Pick Up Address</AddressCode>
            <CityOrSuburb>ALBION</CityOrSuburb>
            <StateOrProvince>QLD</StateOrProvince>
            <Sequence>2</Sequence>
            <AddressCapabilities>
              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <Consols>
      <Consol>
        <Events>
          <Event>
            <Source>JobConsol</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2013-11-07T17:07:15+02:00</DateTime>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
        </Events>
        <ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">masterbill</ConsolIdentifier>
        <ConsolDetail>
          <ConsolType>Agent</ConsolType>
          <ContainerMode>FCL</ContainerMode>
          <TransportMode>SEA</TransportMode>
          <Vessel>
            <VesselName>ADMIRALENGRACHT</VesselName>
            <VoyageNo>voyageno</VoyageNo>
            <LloydsNo>8811924</LloydsNo>
          </Vessel>
          <PaymentType>PPD</PaymentType>
          <PlannedLegs>
            <PlannedLeg>
              <TransportMode>SEA</TransportMode>
              <PortOfLoading>
                <Port Country=""Germany"" City=""Frankfurt am Main"">DEFRA</Port>
              </PortOfLoading>
              <PortOfDischarge>
                <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
              </PortOfDischarge>
              <TransportType>MainVessel</TransportType>
              <LegOrderNumber>1</LegOrderNumber>
              <Vessel>
                <VesselName>ADMIRALENGRACHT</VesselName>
                <LloydsNo>8811924</LloydsNo>
                <VoyageNo>voyageno</VoyageNo>
              </Vessel>
            </PlannedLeg>
          </PlannedLegs>
          <NumberOfOriginalBills>3</NumberOfOriginalBills>
          <NumberOfCopyBills>3</NumberOfCopyBills>
        </ConsolDetail>
        <Shipments>
          <Shipment>
            <ShipmentIdentifier ShipmentIdentifierType=""Housebill"">HB1</ShipmentIdentifier>
            <ShipmentDetails>
              <TransportMode>SEA</TransportMode>
              <PortOfOrigin>
                <Port Country=""Australia"" City=""Brisbane"">AUBNE</Port>
              </PortOfOrigin>
              <PortofDestination>
                <Port>XXABC</Port>
              </PortofDestination>
              <Consignee EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                <OrganisationDetails>
                  <Name>X L INDUSTRIES</Name>
                  <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                  <Addresses>
                    <Address AddressType=""MAIN"">
                      <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                      <AddressLine2>WACOL, QLD</AddressLine2>
                      <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                      <PostCode>4076</PostCode>
                      <Language>EN</Language>
                      <Location>AUBNE</Location>
                      <Sequence>1</Sequence>
                      <AddressCapabilities>
                        <AddressCapability AddressType=""MAIN"" />
                        <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                      </AddressCapabilities>
                    </Address>
                    <Address>
                      <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                      <AddressLine2>WACOL                             QLD</AddressLine2>
                      <AddressCode>Pick Up Address</AddressCode>
                      <CityOrSuburb>QLD QLD</CityOrSuburb>
                      <StateOrProvince>QLD</StateOrProvince>
                      <PostCode>4076</PostCode>
                      <Sequence>2</Sequence>
                      <AddressCapabilities>
                        <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                      </AddressCapabilities>
                    </Address>
                  </Addresses>
                  <Contacts>
                    <Contact>
                      <Name>KERRY</Name>
                      <NotifyMode>PRN</NotifyMode>
                      <Sequence>1</Sequence>
                    </Contact>
                    <Contact>
                      <Name>NICK KORGANOW</Name>
                      <NotifyMode>PRN</NotifyMode>
                      <JobTitle>MANAGING DIRECTOR</JobTitle>
                      <Phone>07 3271 4166</Phone>
                      <Fax>07 3271 5025</Fax>
                      <Sequence>2</Sequence>
                    </Contact>
                  </Contacts>
                </OrganisationDetails>
              </Consignee>
              <Consignor EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                <OrganisationDetails>
                  <Name>X L INDUSTRIES</Name>
                  <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                  <Addresses>
                    <Address AddressType=""MAIN"">
                      <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                      <AddressLine2>WACOL, QLD</AddressLine2>
                      <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                      <PostCode>4076</PostCode>
                      <Language>EN</Language>
                      <Location>AUBNE</Location>
                      <Sequence>1</Sequence>
                      <AddressCapabilities>
                        <AddressCapability AddressType=""MAIN"" />
                        <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                      </AddressCapabilities>
                    </Address>
                    <Address>
                      <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                      <AddressLine2>WACOL                             QLD</AddressLine2>
                      <AddressCode>Pick Up Address</AddressCode>
                      <CityOrSuburb>QLD QLD</CityOrSuburb>
                      <StateOrProvince>QLD</StateOrProvince>
                      <PostCode>4076</PostCode>
                      <Sequence>2</Sequence>
                      <AddressCapabilities>
                        <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                      </AddressCapabilities>
                    </Address>
                  </Addresses>
                  <Contacts>
                    <Contact>
                      <Name>KERRY</Name>
                      <NotifyMode>PRN</NotifyMode>
                      <Sequence>1</Sequence>
                    </Contact>
                    <Contact>
                      <Name>NICK KORGANOW</Name>
                      <NotifyMode>PRN</NotifyMode>
                      <JobTitle>MANAGING DIRECTOR</JobTitle>
                      <Phone>07 3271 4166</Phone>
                      <Fax>07 3271 5025</Fax>
                      <Sequence>2</Sequence>
                    </Contact>
                  </Contacts>
                </OrganisationDetails>
              </Consignor>
              <PackingMode>LCL</PackingMode>
              <ForwardingShipmentType>STD</ForwardingShipmentType>
              <TotalInnerPacksQty DimensionType=""CTN"">0</TotalInnerPacksQty>
              <TotalOuterPacksQty DimensionType=""PLT"">0</TotalOuterPacksQty>
              <Weight DimensionType=""KG"">0</Weight>
              <Volume DimensionType=""M3"">0</Volume>
              <GoodsValue CurrencyCode=""AUD"">0</GoodsValue>
              <InsuranceValue CurrencyCode=""AUD"">0</InsuranceValue>
              <ChargeableWeight DimensionType=""M3"">0</ChargeableWeight>
              <ServiceLevel>STD</ServiceLevel>
              <Incoterm>FOB</Incoterm>
              <ReleaseType>OBR</ReleaseType>
              <ShippedOnBoardType>SHP</ShippedOnBoardType>
              <NoOriginalBills>3</NoOriginalBills>
              <NoCopyBills>3</NoCopyBills>
              <Deliver>
                <Address>
                  <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                  <AddressLine2>WACOL, QLD</AddressLine2>
                  <PostCode>4076</PostCode>
                </Address>
                <CFS />
              </Deliver>
              <Pickup>
                <Address>
                  <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                  <AddressLine2>WACOL                             QLD</AddressLine2>
                  <CityOrSuburb>QLD QLD</CityOrSuburb>
                  <StateOrProvince>QLD</StateOrProvince>
                  <PostCode>4076</PostCode>
                </Address>
                <CFS />
              </Pickup>
              <Custom>
                <Decimal1>0</Decimal1>
                <Decimal2>0</Decimal2>
                <Flag1>false</Flag1>
                <Flag2>false</Flag2>
              </Custom>
              <DocAddresses>
                <DocAddress AddressType=""CED"">
                  <AddressReference>
                    <AddressSequenceRef>1</AddressSequenceRef>
                    <Organisation EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                      <OrganisationDetails>
                        <Name>X L INDUSTRIES</Name>
                        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                        <Addresses>
                          <Address AddressType=""MAIN"">
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                            <AddressLine2>WACOL, QLD</AddressLine2>
                            <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                            <PostCode>4076</PostCode>
                            <Language>EN</Language>
                            <Location>AUBNE</Location>
                            <Sequence>1</Sequence>
                            <AddressCapabilities>
                              <AddressCapability AddressType=""MAIN"" />
                              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                            </AddressCapabilities>
                          </Address>
                          <Address>
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                            <AddressLine2>WACOL                             QLD</AddressLine2>
                            <AddressCode>Pick Up Address</AddressCode>
                            <CityOrSuburb>QLD QLD</CityOrSuburb>
                            <StateOrProvince>QLD</StateOrProvince>
                            <PostCode>4076</PostCode>
                            <Sequence>2</Sequence>
                            <AddressCapabilities>
                              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                            </AddressCapabilities>
                          </Address>
                        </Addresses>
                        <Contacts>
                          <Contact>
                            <Name>KERRY</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <Sequence>1</Sequence>
                          </Contact>
                          <Contact>
                            <Name>NICK KORGANOW</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <JobTitle>MANAGING DIRECTOR</JobTitle>
                            <Phone>07 3271 4166</Phone>
                            <Fax>07 3271 5025</Fax>
                            <Sequence>2</Sequence>
                          </Contact>
                        </Contacts>
                      </OrganisationDetails>
                    </Organisation>
                  </AddressReference>
                </DocAddress>
                <DocAddress AddressType=""CEG"">
                  <AddressReference>
                    <AddressSequenceRef>1</AddressSequenceRef>
                    <Organisation EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                      <OrganisationDetails>
                        <Name>X L INDUSTRIES</Name>
                        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                        <Addresses>
                          <Address AddressType=""MAIN"">
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                            <AddressLine2>WACOL, QLD</AddressLine2>
                            <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                            <PostCode>4076</PostCode>
                            <Language>EN</Language>
                            <Location>AUBNE</Location>
                            <Sequence>1</Sequence>
                            <AddressCapabilities>
                              <AddressCapability AddressType=""MAIN"" />
                              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                            </AddressCapabilities>
                          </Address>
                          <Address>
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                            <AddressLine2>WACOL                             QLD</AddressLine2>
                            <AddressCode>Pick Up Address</AddressCode>
                            <CityOrSuburb>QLD QLD</CityOrSuburb>
                            <StateOrProvince>QLD</StateOrProvince>
                            <PostCode>4076</PostCode>
                            <Sequence>2</Sequence>
                            <AddressCapabilities>
                              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                            </AddressCapabilities>
                          </Address>
                        </Addresses>
                        <Contacts>
                          <Contact>
                            <Name>KERRY</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <Sequence>1</Sequence>
                          </Contact>
                          <Contact>
                            <Name>NICK KORGANOW</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <JobTitle>MANAGING DIRECTOR</JobTitle>
                            <Phone>07 3271 4166</Phone>
                            <Fax>07 3271 5025</Fax>
                            <Sequence>2</Sequence>
                          </Contact>
                        </Contacts>
                      </OrganisationDetails>
                    </Organisation>
                  </AddressReference>
                </DocAddress>
                <DocAddress AddressType=""CRD"">
                  <AddressReference>
                    <AddressSequenceRef>1</AddressSequenceRef>
                    <Organisation EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                      <OrganisationDetails>
                        <Name>X L INDUSTRIES</Name>
                        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                        <Addresses>
                          <Address AddressType=""MAIN"">
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                            <AddressLine2>WACOL, QLD</AddressLine2>
                            <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                            <PostCode>4076</PostCode>
                            <Language>EN</Language>
                            <Location>AUBNE</Location>
                            <Sequence>1</Sequence>
                            <AddressCapabilities>
                              <AddressCapability AddressType=""MAIN"" />
                              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                            </AddressCapabilities>
                          </Address>
                          <Address>
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                            <AddressLine2>WACOL                             QLD</AddressLine2>
                            <AddressCode>Pick Up Address</AddressCode>
                            <CityOrSuburb>QLD QLD</CityOrSuburb>
                            <StateOrProvince>QLD</StateOrProvince>
                            <PostCode>4076</PostCode>
                            <Sequence>2</Sequence>
                            <AddressCapabilities>
                              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                            </AddressCapabilities>
                          </Address>
                        </Addresses>
                        <Contacts>
                          <Contact>
                            <Name>KERRY</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <Sequence>1</Sequence>
                          </Contact>
                          <Contact>
                            <Name>NICK KORGANOW</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <JobTitle>MANAGING DIRECTOR</JobTitle>
                            <Phone>07 3271 4166</Phone>
                            <Fax>07 3271 5025</Fax>
                            <Sequence>2</Sequence>
                          </Contact>
                        </Contacts>
                      </OrganisationDetails>
                    </Organisation>
                  </AddressReference>
                </DocAddress>
                <DocAddress AddressType=""CRG"">
                  <AddressReference>
                    <AddressSequenceRef>2</AddressSequenceRef>
                    <Organisation EDICode=""XLINDU"" OwnerCode=""XLINDU"">
                      <OrganisationDetails>
                        <Name>X L INDUSTRIES</Name>
                        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
                        <Addresses>
                          <Address AddressType=""MAIN"">
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVENUE</AddressLine1>
                            <AddressLine2>WACOL, QLD</AddressLine2>
                            <AddressCode>PST: BOUNDARY RD &amp; CAMPBE</AddressCode>
                            <PostCode>4076</PostCode>
                            <Language>EN</Language>
                            <Location>AUBNE</Location>
                            <Sequence>1</Sequence>
                            <AddressCapabilities>
                              <AddressCapability AddressType=""MAIN"" />
                              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                            </AddressCapabilities>
                          </Address>
                          <Address>
                            <AddressLine1>BOUNDARY RD &amp; CAMPBELL AVE</AddressLine1>
                            <AddressLine2>WACOL                             QLD</AddressLine2>
                            <AddressCode>Pick Up Address</AddressCode>
                            <CityOrSuburb>QLD QLD</CityOrSuburb>
                            <StateOrProvince>QLD</StateOrProvince>
                            <PostCode>4076</PostCode>
                            <Sequence>2</Sequence>
                            <AddressCapabilities>
                              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
                            </AddressCapabilities>
                          </Address>
                        </Addresses>
                        <Contacts>
                          <Contact>
                            <Name>KERRY</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <Sequence>1</Sequence>
                          </Contact>
                          <Contact>
                            <Name>NICK KORGANOW</Name>
                            <NotifyMode>PRN</NotifyMode>
                            <JobTitle>MANAGING DIRECTOR</JobTitle>
                            <Phone>07 3271 4166</Phone>
                            <Fax>07 3271 5025</Fax>
                            <Sequence>2</Sequence>
                          </Contact>
                        </Contacts>
                      </OrganisationDetails>
                    </Organisation>
                  </AddressReference>
                </DocAddress>
              </DocAddresses>
            </ShipmentDetails>
            <DocData>
              <UserDefinedData>
                <Name>As Agent Option</Name>
                <Value>AS CARRIER</Value>
              </UserDefinedData>
              <UserDefinedData>
                <Name>Shipment Limitations</Name>
                <Value>-</Value>
              </UserDefinedData>
            </DocData>
          </Shipment>
        </Shipments>
      </Consol>
    </Consols>
  </Payload>
</XmlInterchange>";

		#endregion

		class ForwardingConsolWithShipmentValueObjectDataAdapterForTest : ForwardingConsolWithShipmentValueObjectDataAdapter
		{
			public ForwardingConsolWithShipmentValueObjectDataAdapterForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public new void ExportContainers(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
			{
				base.ExportContainers(consol, consolValue, context);
			}
		}

		#endregion
	}
}
