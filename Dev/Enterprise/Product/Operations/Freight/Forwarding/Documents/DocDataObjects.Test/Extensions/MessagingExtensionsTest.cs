using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Moq;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class MessagingExtensionsTest : TestCaseWithFactory
	{
		#region TestWrapInInterchange

		public void TestWrapInInterchange_NoReciepentId()
		{
			const string expectedInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <Key>CHAMDDEE04A000686457</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
            </Company>
            <EventBranch Name=""Brisbane"">BNE</EventBranch>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
        <PortOfOrigin>HAM</PortOfOrigin>
        <PortOfDestination>HKG</PortOfDestination>
        <WayBillNumber>000-12345678</WayBillNumber>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

			var interchange = uxml.WrapInInterchange();

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.WrapInInterchange),
				expectedInterchange, interchange);
		}

		public void TestWrapInInterchange_WithReciepentId()
		{
			const string expectedInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>RECEIPIENT_ID</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <Key>CHAMDDEE04A000686457</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
            </Company>
            <EventBranch Name=""Brisbane"">BNE</EventBranch>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
        <PortOfOrigin>HAM</PortOfOrigin>
        <PortOfDestination>HKG</PortOfDestination>
        <WayBillNumber>000-12345678</WayBillNumber>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

			var interchange = uxml.WrapInInterchange("RECEIPIENT_ID");

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.WrapInInterchange),
				expectedInterchange, interchange);
		}

		#endregion

		#region TestToUniversalXml

		public void TestToUniversalXml()
		{
			var airBookingRequest = CreateAirBookingRequest();
			var data = airBookingRequest.MakeDynamic();
			var document = new Mock<DocumentVisualizer.Core.IDocument>();
			document.Setup(d => d.Data).Returns(data);
			document.Setup(d => d.DataContext).Returns(DataContext.AirBookingRequest);

			var xml = document.Object.ToUniversalXml();

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.ToUniversalXml),
				expectedAirBookingXml, xml);
		}

		const string expectedAirBookingXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C000001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DocumentName>AirBooking</DocumentName>
      </DocumentaryOverride>
      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>12345</BookingConfirmationReference>
    <GoodsDescription>Frozen ducks</GoodsDescription>
    <PortOfDestination>AKL</PortOfDestination>
    <PortOfOrigin>SYD</PortOfOrigin>
    <RequiredTemperatureMaximum>25</RequiredTemperatureMaximum>
    <RequiredTemperatureMinimum>15</RequiredTemperatureMinimum>
    <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalVolume>2</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>2</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>081-00000006</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>CarrierContractNumber1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>CarrierContractNumber2</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ContainerCount>2</ContainerCount>
        <ContainerNumber></ContainerNumber>
        <ContainerType>
          <Code>A12</Code>
          <Description></Description>
        </ContainerType>
        <GoodsWeight>5</GoodsWeight>
        <GrossWeight>15</GrossWeight>
        <NonOperatingReefer>false</NonOperatingReefer>
        <TareWeight>10</TareWeight>
        <WeightUnit>M</WeightUnit>
      </Container>
    </ContainerCollection>

    <NoteCollection>
      <Note>
        <Description>SpecialServiceRequest</Description>
        <NoteText>special instructions
dangerous goods handling information</NoteText>
      </Note>
      <Note>
        <Description>OtherServiceInformation</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Airline</AddressType>
        <CompanyName>Qantas</CompanyName>
        <OrganizationCode>QF</OrganizationCode>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Agent</AddressType>
        <CompanyName>Umbrella Corporation</CompanyName>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""IATA CASS Number"">CAS</Type>
            <Value></Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <Height>4</Height>
        <Length>2</Length>
        <LengthUnit Description=""Meters"">M</LengthUnit>
        <PackQty>1</PackQty>
        <Weight>5</Weight>
        <WeightUnit>M</WeightUnit>
        <Width>3</Width>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge>AKL</PortOfDischarge>
        <PortOfLoading>SYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <BookingStatus Description=""Requested"">RQD</BookingStatus>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture></EstimatedDeparture>
        <LegType>Flight1</LegType>
        <TransportMode>Air</TransportMode>
        <VoyageFlightNo>QF001</VoyageFlightNo>

        <CustomizedFieldCollection>
          <CustomizedField>
            <DataType>String</DataType>
            <Key>AllotmentId</Key>
            <Value>allotment id</Value>
          </CustomizedField>
        </CustomizedFieldCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		AirBookingRequest CreateAirBookingRequest()
		{
			var context = new CommonContext(Factory);

			var request = new AirBookingRequest("ForwardingConsol", "C000001000");
			request.MasterAirWaybillNumber = "081-00000006";
			request.BookingReferenceNumber = "12345";
			request.GoodsDescription = "Frozen ducks";
			request.Carrier = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "QF",
				Description = "Qantas"
			};
			request.Agent = "Umbrella Corporation";
			request.OriginAirport = new DummyUnloco
			{
				IATACode = "SYD"
			};
			request.DestinationAirport = new DummyUnloco
			{
				IATACode = "AKL"
			};

			request.TotalPieces = 1;
			request.TotalVolume = new Measurement
			{
				Value = 2,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Core.Constants.Volume.CubicMetres
				}
			};
			request.TotalWeight = new Measurement
			{
				Value = 2,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};

			request.RequiresTemperatureControl = true;
			request.TemperatureMinimum = new Measurement
			{
				Value = 15,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = "C"
				}
			};

			request.TemperatureMaximum = new Measurement
			{
				Value = 25,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = "C"
				}
			};

			var statusList = new CodeDescriptionPairList();
			statusList.AddPair(Core.Constants.TransportStatus.Planned, Core.Constants.TransportStatusDescriptions.Planned);

			var transportTypeList = new CodeDescriptionPairList();
			transportTypeList.AddPair(Core.Constants.TransportPlanningType.Flight1, "Flight 1");

			request.FlightDetails = new[]
			{
						new FlightDetail("zzz")
						{
							FlightNumber = "QF001",
							TransportType = new CodeDescription(transportTypeList)
							{
								Code = Core.Constants.TransportPlanningType.Flight1
							},
							PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
							{
								IATACode = "SYD"
							},
							PortOfDischarge = new Unloco(context.Factory, context.Unlocos, context.Countries)
							{
								IATACode = "AKL"
							},
							Status = new FlightStatus(statusList)
							{
								Code = Core.Constants.TransportStatus.Planned
							},
							AllotmentId = "allotment id"
						}
					};

			request.Dimensions = new[]
			{
						new PackingLine("zzz", Factory)
						{
							Quantity = 1,
							Length = new Measurement
							{
								Value = 2,
								Unit = new CodeDescription(context.DimensionUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							Width = new Measurement
							{
								Value = 3,
								Unit = new CodeDescription(context.DimensionUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							Height = new Measurement
							{
								Value = 4,
								Unit = new CodeDescription(context.DimensionUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							Weight = new Measurement
							{
								Value = 5,
								Unit = new CodeDescription(context.WeightUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							}
						}
					};

			request.Ulds = new[]
			{
						new ULD("zzz")
						{
							ContainerCount = 2,
							Type = new UldContainerType
							{
								Code = "A12"
							},
							GoodsWeight = new Measurement
							{
								Value = 5,
								Unit = new CodeDescription(context.WeightUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							TareWeight = new Measurement
							{
								Value = 10,
								Unit = new CodeDescription(context.WeightUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							GrossWeight = new Measurement
							{
								Value = 15,
								Unit = new CodeDescription(context.WeightUnits)
								{
									Code = Core.Constants.Length.Metres
								}
							},
							IsNonOperativeReefer = false
						}
					};

			request.CarrierContractNumbers = new[]
			{
						new ReferenceNumber
						{
							Value = "CarrierContractNumber1",
							Type = new DummyCodeDescription
							{
								Code = "CON",
								Description = "Carrier Contract Number"
							},
							CountryOfIssue = new DummyCountry
							{
								Code = "AU",
								Name = "Australia"
							}
						},
						new ReferenceNumber
						{
							Value = "CarrierContractNumber2",
							Type = new DummyCodeDescription
							{
								Code = "CON",
								Description = "Carrier Contract Number"
							},
							CountryOfIssue = new DummyCountry
							{
								Code = "AU",
								Name = "Australia"
							}
						}
					};

			request.SpecialInstructions = "special instructions";
			request.DangerousGoodsHandlingInformation = "dangerous goods handling information";
			request.GoodsHandlingInstructions = "goods handling instructions";

			return request;
		}

		#endregion

		#region TestReplaceNamespace

		public void TestReplaceNamespace_Absolute()
		{
			const string ns = "http://www.bite.me";

			var updatedXml = uxml.ReplaceNamespace(ns);

			const string expectedUxml = @"<UniversalShipment xmlns=""http://www.bite.me"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>CHAMDDEE04A000686457</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
        </Company>
        <EventBranch Name=""Brisbane"">BNE</EventBranch>
      </Workflow>
    </DataContext>
    <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
    <PortOfOrigin>HAM</PortOfOrigin>
    <PortOfDestination>HKG</PortOfDestination>
    <WayBillNumber>000-12345678</WayBillNumber>
  </Shipment>
</UniversalShipment>";

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.WrapInInterchange),
				expectedUxml, updatedXml);
		}

		public void TestReplaceNamespace_Relative()
		{
			const string ns = "/BookingRequest/1";

			var updatedXml = uxml.ReplaceNamespace(ns);

			const string expectedUxml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>CHAMDDEE04A000686457</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
        </Company>
        <EventBranch Name=""Brisbane"">BNE</EventBranch>
      </Workflow>
    </DataContext>
    <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
    <PortOfOrigin>HAM</PortOfOrigin>
    <PortOfDestination>HKG</PortOfDestination>
    <WayBillNumber>000-12345678</WayBillNumber>
  </Shipment>
</UniversalShipment>";

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.WrapInInterchange),
				expectedUxml, updatedXml);
		}

		#endregion

		#region TestPopulateDataContext

		public void TestPopulateDataContext()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = "Dummy",
					Key = "C00001"
				}
			};

			var xml = shipmentDataObject.PopulateDataContext("Dummy Document").ToUniversalXml("/eManifest/1");

			const string expectedUxml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001</Key>
        <Type>Dummy</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Dummy Document</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			AssertMultilineASCIIEquals(nameof(MessagingExtensions.WrapInInterchange), expectedUxml, xml);
		}

		#endregion

		#region Implementation

		const string uxml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
    <Shipment>
        <DataContext>
            <DataSource>
                <Key>CHAMDDEE04A000686457</Key>
                <Type>ForwardingConsol</Type>
            </DataSource>
            <Workflow>
                <Company>
                    <Code>EDI</Code>
                    <Country Name=""Australia"">AU</Country>
                </Company>
                <EventBranch Name=""Brisbane"">BNE</EventBranch>
            </Workflow>
        </DataContext>
        <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
        <PortOfOrigin>HAM</PortOfOrigin>
        <PortOfDestination>HKG</PortOfDestination>
        <WayBillNumber>000-12345678</WayBillNumber>
    </Shipment>
</UniversalShipment>";

		#endregion
	}
}
