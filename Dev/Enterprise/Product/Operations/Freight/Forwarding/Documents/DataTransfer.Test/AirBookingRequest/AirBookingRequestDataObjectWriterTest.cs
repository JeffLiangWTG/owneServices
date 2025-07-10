using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class AirBookingRequestDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var context = new CommonContext(Factory.GetCachedReadOnlyFactory());
			var airBooking = new AirBookingRequest("ForwardingConsol", "CCN1406309");

			airBooking.BookingReferenceNumber = "123";
			airBooking.Agent = "Agent";

			airBooking.RequiresTemperatureControl = true;
			airBooking.TemperatureMaximum = new Measurement
			{
				Value = 12,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			airBooking.TemperatureMinimum = new Measurement
			{
				Value = 20,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			airBooking.Carrier = new DummyCodeDescription
			{
				Code = "SG",
				Description = "Singapore Airlines"
			};

			airBooking.OriginAirport = new DummyUnloco
			{
				Code = "SYD",
				Name = "Sydney"
			};

			airBooking.DestinationAirport = new DummyUnloco
			{
				Code = "SIN",
				Name = "Singapore"
			};

			airBooking.TotalPieces = 11;
			airBooking.TotalWeight = new DummyMeasurement
			{
				Value = 10,
				Unit = new DummyCodeDescription
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};
			airBooking.TotalVolume = new DummyMeasurement
			{
				Value = 20,
				Unit = new DummyCodeDescription
				{
					Code = Core.Constants.Volume.CubicMetres
				}
			};
			airBooking.GoodsDescription = "Frozen ducks";

			var statusList = new CodeDescriptionPairList();
			statusList.AddPair(Core.Constants.TransportStatus.Planned, Core.Constants.TransportStatusDescriptions.Planned);

			var transportTypeList = new CodeDescriptionPairList();
			transportTypeList.AddPair(Core.Constants.TransportPlanningType.Flight1, "Flight 1");

			airBooking.FlightDetails = new[]
			{
				new FlightDetail("zzz")
				{
					FlightNumber = "SG001",
					TransportType = new CodeDescription(transportTypeList)
					{
						Code = Core.Constants.TransportPlanningType.Flight1
					},
					PortOfLoading = Unloco.Create(context, RefUNLOCO.LoadFromIATA(Factory, "SYD")),
					PortOfDischarge = Unloco.Create(context, RefUNLOCO.LoadFromIATA(Factory, "SIN")),
					Status = new FlightStatus(statusList)
					{
						Code = Core.Constants.TransportStatus.Planned
					},
					ETD = new CargoWise.Types.ZDateTime(2020, 7, 25, 12, 13, 14),
					ETA = new CargoWise.Types.ZDateTime(2020, 7, 26, 12, 13, 14)
				}
			};

			airBooking.Dimensions = new[]
			{
				new PackingLine("zzz", Factory)
				{
					Quantity = 7,
					Length = new Measurement
					{
						Value = 1,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Width = new Measurement
					{
						Value = 2,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Height = new Measurement
					{
						Value = 3,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Weight = new Measurement
					{
						Value = 5,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Weight.Kilograms
						}
					}
				}
			};

			airBooking.Ulds = new[]
			{
				new ULD("zzz")
				{
					ContainerCount = 1,
					Type = new UldContainerType
					{
						Code = "A00"
					},
					Number = "123",
					TareWeight = new Measurement
					{
						Value = 1000,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Weight.Kilograms
						}
					},
					GoodsWeight = new Measurement
					{
						Value = 20,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Weight.Kilograms
						}
					},
					GrossWeight = new Measurement
					{
						Value = 1020,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Weight.Kilograms
						}
					},
					IsNonOperativeReefer = false
				}
			};

			airBooking.SpecialInstructions = "special instructions";
			airBooking.DangerousGoodsHandlingInformation = "dangerous goods handling information";
			airBooking.GoodsHandlingInstructions = "goods handling instructions";

			airBooking.CarrierContractNumbers = new[]
			{
				new ReferenceNumber
				{
					Value = "1111",
					Type = new DummyCodeDescription
					{
						Code = "CON",
						Description = "Carrier Contract Number"
					},
					CountryOfIssue = new DummyCountry
					{
						Code = "AU"
					}
				}
			};

			airBooking.Commodity = "Commodity";

			airBooking.ProductList = new CodeDescriptionPairList
			{
				new AirlineConfigProduct { Code = "MDV", Description = "Medical Devices" }
			};
			airBooking.Product = "Medical Devices";
			airBooking.CarrierBookingReference = "CBkRef123";

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new AirBookingRequestDataObjectWriter(manager, MessageType.Unspecified);
			var dataObject = writer.GetDataObject(airBooking);

			AssertUXml(dataObject, expectedXml);
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>CCN1406309</Key>
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

    <BookingConfirmationReference>123</BookingConfirmationReference>
    <GoodsDescription>Frozen ducks</GoodsDescription>
    <PortOfDestination Name=""Singapore""></PortOfDestination>
    <PortOfOrigin Name=""Sydney""></PortOfOrigin>
    <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
    <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
    <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <TotalNoOfPacks>11</TotalNoOfPacks>
    <TotalVolume>20</TotalVolume>
    <TotalVolumeUnit>M3</TotalVolumeUnit>
    <TotalWeight>10</TotalWeight>
    <TotalWeightUnit>KG</TotalWeightUnit>
    <WayBillNumber></WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>CommodityCode</Key>
        <Value>Commodity</Value>
      </AddInfo>
      <AddInfo>
        <Key>ProductCode</Key>
        <Value>MDV</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingReference</Key>
        <Value>CBkRef123</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>1111</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>123</ContainerNumber>
        <ContainerType>
          <Code>A00</Code>
          <Description></Description>
        </ContainerType>
        <GoodsWeight>20</GoodsWeight>
        <GrossWeight>1020</GrossWeight>
        <NonOperatingReefer>false</NonOperatingReefer>
        <TareWeight>1000</TareWeight>
        <WeightUnit>KG</WeightUnit>
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
        <CompanyName>Singapore Airlines</CompanyName>
        <OrganizationCode>SG</OrganizationCode>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Agent</AddressType>
        <CompanyName>Agent</CompanyName>
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
        <Height>3</Height>
        <Length>1</Length>
        <LengthUnit>M</LengthUnit>
        <PackQty>7</PackQty>
        <Weight>5</Weight>
        <WeightUnit>KG</WeightUnit>
        <Width>2</Width>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <BookingStatus Description=""Requested"">RQD</BookingStatus>
        <EstimatedArrival>2020-07-26T12:13:14</EstimatedArrival>
        <EstimatedDeparture>2020-07-25T12:13:14</EstimatedDeparture>
        <LegType>Flight1</LegType>
        <TransportMode>Air</TransportMode>
        <VoyageFlightNo>SG001</VoyageFlightNo>

        <CustomizedFieldCollection>
          <CustomizedField>
            <DataType>String</DataType>
            <Key>AllotmentId</Key>
            <Value></Value>
          </CustomizedField>
        </CustomizedFieldCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";
	}
}
