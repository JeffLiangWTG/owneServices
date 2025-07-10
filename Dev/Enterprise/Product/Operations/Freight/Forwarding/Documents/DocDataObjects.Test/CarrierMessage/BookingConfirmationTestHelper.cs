using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	public static class BookingConfirmationTestHelper
	{
		public static IEDIMessage CreateBookingConfirmationMessage(this BusinessObjectFactory factory, string bookingConfirmationUXML = "")
		{
			var builder = new InterchangeBuilder(factory);
			if (string.IsNullOrEmpty(bookingConfirmationUXML))
			{
				bookingConfirmationUXML = BookingConfirmationUXML_LinkOnly;
			}

			var uXML = bookingConfirmationUXML.WrapInInterchange();

			if (!builder.TryParseUniversalXml(uXML, InterchangeBuilder.MessageDirection.Receive, out var interchange))
			{
				throw new InvalidOperationException("Interchange could not be created for booking confirmation message");
			}

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			messageQuery.FetchOnlyFromLocalCache = true;

			var message = factory.Load<IEDIMessage>(messageQuery).Single();
			message.EM_Status = EDIMessageStatusList.Codes.Linked;

			return message;
		}

		#region UXML

		public const string BookingConfirmationUXMLMissingContainerType_LinkOnly = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C01329220</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <CoLoadBookingConfirmationReference>CLDCR123456</CoLoadBookingConfirmationReference>
        <CoLoadMasterBillNumber>CLDBL123456</CoLoadMasterBillNumber>
        <BookingConfirmationReference>ANT1042970</BookingConfirmationReference>
        <WayBillNumber>BL091042970</WayBillNumber>
        <PortFirstForeign Name=""SYDNEY"">AUSYD</PortFirstForeign>
        <PortLastForeign Name=""HONG KONG"">HKHKG</PortLastForeign>
        <PortOfFirstArrival Name=""LOS ANGELES"">USLAX</PortOfFirstArrival>
        <DeliveryMode Description=""Peer To Peer"">PTP</DeliveryMode>
        <PlaceOfDelivery Name=""ANTWERPEN"">BEANR</PlaceOfDelivery>
        <PlaceOfReceipt Name=""JEBEL ALI"">AEJEA</PlaceOfReceipt>
        <PortOfDischarge Name=""ANTWERPEN"">BEANR</PortOfDischarge>
        <PortOfLoading Name=""JEBEL ALI"">AEJEA</PortOfLoading>
        <ShipmentType>CLD</ShipmentType>
        <LloydsIMO>6969</LloydsIMO>
        <VesselName>Ye Olde Vessel</VesselName>
        <VoyageFlightNo>69420</VoyageFlightNo>
        <ContainerMode>FCL</ContainerMode>
        
        <DateCollection>
            <Date>
                <Type>FirstArrivalInCountry</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2017-11-28T10:00:00</Value>
            </Date>
        </DateCollection>
        <NoteCollection Content=""Partial"">
            <Note>
                <Description>Booking Confirmation Notes</Description>
                <IsCustomDescription>true</IsCustomDescription>
                <NoteText>GENERAL INFO FOR THE BOOKING CONFIRMATION</NoteText>
            </Note>
        </NoteCollection>

        <ContainerCollection Content=""Partial"">
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
                <TareWeight>1000000</TareWeight>
                <GrossWeight>1350000</GrossWeight>
                <WeightUnit Description=""Grams"">G</WeightUnit>
            </Container>
        </ContainerCollection>
    </Shipment>
</UniversalShipment>";

		public const string BookingConfirmationUXML_LinkOnly = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C01329220</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <CoLoadBookingConfirmationReference>CLDCR123456</CoLoadBookingConfirmationReference>
        <CoLoadMasterBillNumber>CLDBL123456</CoLoadMasterBillNumber>
        <BookingConfirmationReference>ANT1042970</BookingConfirmationReference>
        <WayBillNumber>BL091042970</WayBillNumber>
        <PortFirstForeign Name=""SYDNEY"">AUSYD</PortFirstForeign>
        <PortLastForeign Name=""HONG KONG"">HKHKG</PortLastForeign>
        <PortOfFirstArrival Name=""LOS ANGELES"">USLAX</PortOfFirstArrival>
        <DeliveryMode Description=""Peer To Peer"">PTP</DeliveryMode>
        <PlaceOfDelivery Name=""ANTWERPEN"">BEANR</PlaceOfDelivery>
        <PlaceOfReceipt Name=""JEBEL ALI"">AEJEA</PlaceOfReceipt>
        <PortOfDischarge Name=""ANTWERPEN"">BEANR</PortOfDischarge>
        <PortOfLoading Name=""JEBEL ALI"">AEJEA</PortOfLoading>
        <ShipmentType>CLD</ShipmentType>
        <LloydsIMO>6969</LloydsIMO>
        <VesselName>Ye Olde Vessel</VesselName>
        <VoyageFlightNo>69420</VoyageFlightNo>
		<ContainerMode>FCL</ContainerMode>
        
        <DateCollection>
            <Date>
                <Type>FirstArrivalInCountry</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2017-11-28T10:00:00</Value>
            </Date>
        </DateCollection>
        <NoteCollection Content=""Partial"">
            <Note>
                <Description>Booking Confirmation Notes</Description>
                <IsCustomDescription>true</IsCustomDescription>
                <NoteText>GENERAL INFO FOR THE BOOKING CONFIRMATION</NoteText>
            </Note>
        </NoteCollection>

        <ContainerCollection Content=""Partial"">
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <ContainerType>
                    <Code></Code>
                    <ISOCode>22R0</ISOCode>
                </ContainerType>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
				<TareWeight>1000000</TareWeight>
				<GrossWeight>1350000</GrossWeight>
				<WeightUnit Description=""Grams"">G</WeightUnit>
            </Container>
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerNumber>1234567</ContainerNumber>
                <ContainerType>
                    <Code>6XX9</Code>
                    <ISOCode></ISOCode>
                </ContainerType>
                <DeliveryMode>CFS/CFS</DeliveryMode>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
            </Container>
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerType>
                    <Code>6XX9</Code>
                    <ISOCode></ISOCode>
                </ContainerType>
                <DeliveryMode>CFS/CFS</DeliveryMode>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
            </Container>
        </ContainerCollection>

        <AddInfoCollection>
            <AddInfo>
                <Key>EventTime</Key>
                <Value>2016-09-01T15:13:30</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventType</Key>
                <Value>MAA</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventReference</Key>
                <Value>Booking Confirmed</Value>
            </AddInfo>
            <AddInfo>
                <Key>OutOfGaugeFreight</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>HazardousCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>TemperatureControlledCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>EnvironmentalPollutantCargo</Key>
                <Value>Y</Value>
            </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
            <AdditionalReference>
                <Type Description=""Freight Forwarder Reference"">FFW</Type>
                <ReferenceNumber>111</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Carrier Contract Number"">CON</Type>
                <ReferenceNumber>222</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Shipper Reference"">SHP</Type>
                <ReferenceNumber>333</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Contract Named Account"">NAC</Type>
                <ReferenceNumber>444</ReferenceNumber>
            </AdditionalReference>
        </AdditionalReferenceCollection>

        <OrganizationAddressCollection>
            <OrganizationAddress>
                <AddressType>ConsignorDocumentaryAddress</AddressType>
                <CompanyName>Consignor Documentary Address Company</CompanyName>
                <Address1>TEOLLISUUSTIE</Address1>
                <Address2></Address2>
                <City>KEITELE</City>
                <Postcode>72600</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <CompanyName>Shipping Line Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
				<Port Name=""Helsinki"">FIHEL</Port>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneeDocumentaryAddress</AddressType>
                <CompanyName>Consignee Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>NotifyParty</AddressType>
                <CompanyName>Notify Party Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>DepartureCTOAddress</AddressType>
                <CompanyName>Departure CTO Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                <CompanyName>Consignor Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                <CompanyName>Consignee Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
            <PackingLine>
                <ContainerLink></ContainerLink>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <DetailedDescription>
                        WHITEWOOD
                        HS CODE 440712
                </DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <HarmonisedCode></HarmonisedCode>
                <Height></Height>
                <ImportReferenceNumber></ImportReferenceNumber>
                <Length></Length>
                <LengthUnit Description=""Meters""></LengthUnit>
                <MarksAndNos>
                        DESTINATION DOC FEE and THC PREPAID
                        FREIGHT PREPAID
                        HPL CONTAINER
                </MarksAndNos>
                <PackQty>100</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber></ReferenceNumber>
                <Volume>500000</Volume>
                <VolumeUnit Description=""Cubic Decimeters"">D3</VolumeUnit>
                <Weight>265000</Weight>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                <Width></Width>
                <UNDGCollection>
                </UNDGCollection>
            </PackingLine>
			<PackingLine>
                <ContainerLink></ContainerLink>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <DetailedDescription>Test packing line</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <HarmonisedCode></HarmonisedCode>
                <Height></Height>
                <ImportReferenceNumber></ImportReferenceNumber>
                <Length></Length>
                <LengthUnit Description=""Meters""></LengthUnit>
                <MarksAndNos>
                        Ye Olde marks and nos
                </MarksAndNos>
                <PackQty>69</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber></ReferenceNumber>
                <Volume>420000</Volume>
                <VolumeUnit Description=""Cubic Decimeters"">D3</VolumeUnit>
                <Weight>6969</Weight>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                <Width></Width>
                <UNDGCollection>
                </UNDGCollection>
            </PackingLine>
        </PackingLineCollection>

        <TransportLegCollection>
            <TransportLeg>
                <PortOfDischarge Name=""Jebel Ali"">AEJEA</PortOfDischarge>
                <PortOfLoading Name=""Antwerp"">BEANR</PortOfLoading>
                <LegOrder>1</LegOrder>
                <EstimatedArrival>2016-09-02T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-01T22:00:00</EstimatedDeparture>
                <LegType>PreCarriage</LegType>
                <TransportMode>Sea</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Sydney"">AUSYD</PortOfDischarge>
                <PortOfLoading Name=""Perth"">AUPER</PortOfLoading>
                <LegOrder>2</LegOrder>
                <EstimatedArrival>2016-09-07T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-03T22:00:00</EstimatedDeparture>
                <LegType>Main</LegType>
                <TransportMode>Rail</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
                <PortOfLoading Name=""Auckland"">NZAKL</PortOfLoading>
                <LegOrder>3</LegOrder>
                <EstimatedArrival>2016-09-19T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-08T22:00:00</EstimatedDeparture>
                <LegType>OnForwarding</LegType>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
        </TransportLegCollection>
    </Shipment>
</UniversalShipment>";

		public const string BookingConfirmationUXMLEmptyCollections_LinkOnly = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C01329220</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <CoLoadBookingConfirmationReference>CLDCR123456</CoLoadBookingConfirmationReference>
        <CoLoadMasterBillNumber>CLDBL123456</CoLoadMasterBillNumber>
        <BookingConfirmationReference>ANT1042970</BookingConfirmationReference>
        <WayBillNumber>BL091042970</WayBillNumber>
        <PortFirstForeign Name=""SYDNEY"">AUSYD</PortFirstForeign>
        <PortLastForeign Name=""HONG KONG"">HKHKG</PortLastForeign>
        <PortOfFirstArrival Name=""LOS ANGELES"">USLAX</PortOfFirstArrival>
        <DeliveryMode Description=""Peer To Peer"">PTP</DeliveryMode>
        <PlaceOfDelivery Name=""ANTWERPEN"">BEANR</PlaceOfDelivery>
        <PlaceOfReceipt Name=""JEBEL ALI"">AEJEA</PlaceOfReceipt>
        <PortOfDischarge Name=""ANTWERPEN"">BEANR</PortOfDischarge>
        <PortOfLoading Name=""JEBEL ALI"">AEJEA</PortOfLoading>
        <ShipmentType>CLD</ShipmentType>
        <LloydsIMO>6969</LloydsIMO>
        <VesselName>Ye Olde Vessel</VesselName>
        <VoyageFlightNo>69420</VoyageFlightNo>
		<ContainerMode>FCL</ContainerMode>
        
        <DateCollection>
            <Date>
                <Type>FirstArrivalInCountry</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2017-11-28T10:00:00</Value>
            </Date>
        </DateCollection>

        <AddInfoCollection>
            <AddInfo>
                <Key>EventTime</Key>
                <Value>2016-09-01T15:13:30</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventType</Key>
                <Value>MAA</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventReference</Key>
                <Value>Booking Confirmed</Value>
            </AddInfo>
            <AddInfo>
                <Key>OutOfGaugeFreight</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>HazardousCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>TemperatureControlledCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>EnvironmentalPollutantCargo</Key>
                <Value>Y</Value>
            </AddInfo>
        </AddInfoCollection>

        <OrganizationAddressCollection>
            <OrganizationAddress>
                <AddressType>ConsignorDocumentaryAddress</AddressType>
                <CompanyName>Consignor Documentary Address Company</CompanyName>
                <Address1>TEOLLISUUSTIE</Address1>
                <Address2></Address2>
                <City>KEITELE</City>
                <Postcode>72600</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <CompanyName>Shipping Line Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
				<Port Name=""Helsinki"">FIHEL</Port>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneeAddress</AddressType>
                <CompanyName>Consignee Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>NotifyParty</AddressType>
                <CompanyName>Notify Party Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>DepartureCTOAddress</AddressType>
                <CompanyName>Departure CTO Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                <CompanyName>Consignor Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                <CompanyName>Consignee Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
        </OrganizationAddressCollection>

        <TransportLegCollection>
            <TransportLeg>
                <PortOfDischarge Name=""Jebel Ali"">AEJEA</PortOfDischarge>
                <PortOfLoading Name=""Antwerp"">BEANR</PortOfLoading>
                <LegOrder>1</LegOrder>
                <EstimatedArrival>2016-09-02T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-01T22:00:00</EstimatedDeparture>
                <LegType>PreCarriage</LegType>
                <TransportMode>Sea</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Sydney"">AUSYD</PortOfDischarge>
                <PortOfLoading Name=""Perth"">AUPER</PortOfLoading>
                <LegOrder>2</LegOrder>
                <EstimatedArrival>2016-09-07T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-03T22:00:00</EstimatedDeparture>
                <LegType>Main</LegType>
                <TransportMode>Road</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
                <PortOfLoading Name=""Auckland"">NZAKL</PortOfLoading>
                <LegOrder>3</LegOrder>
                <EstimatedArrival>2016-09-19T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-08T22:00:00</EstimatedDeparture>
                <LegType>OnForwarding</LegType>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
        </TransportLegCollection>
    </Shipment>
</UniversalShipment>";

		public const string BookingConfirmationUXMLWithUnlocoMappings_LinkOnly = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Shipment>
        <DataContext>
            <Action>LinkOnly</Action>
            <DocumentaryOverride>
                <DocumentName>Booking Confirmation</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>C01329220</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <CoLoadBookingConfirmationReference>CLDCR123456</CoLoadBookingConfirmationReference>
        <CoLoadMasterBillNumber>CLDBL123456</CoLoadMasterBillNumber>
        <BookingConfirmationReference>ANT1042970</BookingConfirmationReference>
        <WayBillNumber>BL091042970</WayBillNumber>
        <PortFirstForeign Name=""SYDNEY"">AUSYD</PortFirstForeign>
        <PortLastForeign Name=""HONG KONG"">HKHKG</PortLastForeign>
        <PortOfFirstArrival Name=""LOS ANGELES"">USLAX</PortOfFirstArrival>
        <DeliveryMode Description=""Peer To Peer"">PTP</DeliveryMode>
        <PlaceOfDelivery Name=""ANTWERPEN"">BEANR</PlaceOfDelivery>
        <PlaceOfReceipt Name=""JEBEL ALI"">AEZZZ</PlaceOfReceipt>
        <PortOfDischarge Name=""ANTWERPEN"">BEANR</PortOfDischarge>
        <PortOfLoading Name=""JEBEL ALI"">AEZZZ</PortOfLoading>
        <ShipmentType>CLD</ShipmentType>
        <LloydsIMO>6969</LloydsIMO>
        <VesselName>Ye Olde Vessel</VesselName>
        <VoyageFlightNo>69420</VoyageFlightNo>
		<ContainerMode>FCL</ContainerMode>
        
        <DateCollection>
            <Date>
                <Type>FirstArrivalInCountry</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2017-11-28T10:00:00</Value>
            </Date>
        </DateCollection>
        <NoteCollection Content=""Partial"">
            <Note>
                <Description>Booking Confirmation Notes</Description>
                <IsCustomDescription>true</IsCustomDescription>
                <NoteText>GENERAL INFO FOR THE BOOKING CONFIRMATION</NoteText>
            </Note>
        </NoteCollection>

        <ContainerCollection Content=""Partial"">
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <ContainerType>
                    <Code></Code>
                    <ISOCode>22R0</ISOCode>
                </ContainerType>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
				<TareWeight>1000000</TareWeight>
				<GrossWeight>1350000</GrossWeight>
				<WeightUnit Description=""Grams"">G</WeightUnit>
            </Container>
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerNumber>1234567</ContainerNumber>
                <ContainerType>
                    <Code>6XX9</Code>
                    <ISOCode></ISOCode>
                </ContainerType>
                <DeliveryMode>CFS/CFS</DeliveryMode>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
            </Container>
            <Container>
                <ContainerCount>1</ContainerCount>
                <ContainerType>
                    <Code>6XX9</Code>
                    <ISOCode></ISOCode>
                </ContainerType>
                <DeliveryMode>CFS/CFS</DeliveryMode>
                <EmptyRequired>2017-11-29T15:00:00</EmptyRequired>
                <DepartureEstimatedPickup>2017-11-29T15:00:00</DepartureEstimatedPickup>
                <ArrivalDeliveryRequiredBy>2017-11-29T15:00:00</ArrivalDeliveryRequiredBy>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
                        <CompanyName>AUSTRALIAN AIR EXPRESS PTY LTD</CompanyName>
                        <Address1>DRYANDRA ROAD</Address1>
                        <Address2>BRISBANE, QLD</Address2>
                        <City>BRISBANE</City>
                        <Postcode>4007</Postcode>
                        <State></State>
                        <Country>AU</Country>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
            </Container>
        </ContainerCollection>

        <AddInfoCollection>
            <AddInfo>
                <Key>EventTime</Key>
                <Value>2016-09-01T15:13:30</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventType</Key>
                <Value>MAA</Value>
            </AddInfo>
            <AddInfo>
                <Key>EventReference</Key>
                <Value>Booking Confirmed</Value>
            </AddInfo>
            <AddInfo>
                <Key>OutOfGaugeFreight</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>HazardousCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>TemperatureControlledCargo</Key>
                <Value>Y</Value>
            </AddInfo>
            <AddInfo>
                <Key>EnvironmentalPollutantCargo</Key>
                <Value>Y</Value>
            </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
            <AdditionalReference>
                <Type Description=""Freight Forwarder Reference"">FFW</Type>
                <ReferenceNumber>111</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Carrier Contract Number"">CON</Type>
                <ReferenceNumber>222</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Shipper Reference"">SHP</Type>
                <ReferenceNumber>333</ReferenceNumber>
            </AdditionalReference>
            <AdditionalReference>
                <Type Description=""Contract Named Account"">NAC</Type>
                <ReferenceNumber>444</ReferenceNumber>
            </AdditionalReference>
        </AdditionalReferenceCollection>

        <OrganizationAddressCollection>
            <OrganizationAddress>
                <AddressType>ConsignorDocumentaryAddress</AddressType>
                <CompanyName>Consignor Documentary Address Company</CompanyName>
                <Address1>TEOLLISUUSTIE</Address1>
                <Address2></Address2>
                <City>KEITELE</City>
                <Postcode>72600</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <CompanyName>Shipping Line Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
				<Port Name=""Helsinki"">FIHEL</Port>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneeDocumentaryAddress</AddressType>
                <CompanyName>Consignee Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>NotifyParty</AddressType>
                <CompanyName>Notify Party Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>DepartureCTOAddress</AddressType>
                <CompanyName>Departure CTO Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                <CompanyName>Consignor Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                <CompanyName>Consignee Pickup Delivery Address Company</CompanyName>
                <Address1>RUOHOLAHDENKATU 21</Address1>
                <Address2></Address2>
                <City>HELSINKI</City>
                <Postcode>00180</Postcode>
                <State></State>
                <Country Name=""Finland"">FI</Country>
                <Contact></Contact>
                <Email></Email>
                <Fax></Fax>
                <Phone></Phone>
                <RegistrationNumberCollection>
                    <RegistrationNumber>
                        <Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
                        <CountryOfIssue Name=""United States"">US</CountryOfIssue>
                        <Value>MSCU</Value>
                    </RegistrationNumber>
                </RegistrationNumberCollection>
            </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
            <PackingLine>
                <ContainerLink></ContainerLink>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <DetailedDescription>
                        WHITEWOOD
                        HS CODE 440712
                </DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <HarmonisedCode></HarmonisedCode>
                <Height></Height>
                <ImportReferenceNumber></ImportReferenceNumber>
                <Length></Length>
                <LengthUnit Description=""Meters""></LengthUnit>
                <MarksAndNos>
                        DESTINATION DOC FEE and THC PREPAID
                        FREIGHT PREPAID
                        HPL CONTAINER
                </MarksAndNos>
                <PackQty>100</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber></ReferenceNumber>
                <Volume>500000</Volume>
                <VolumeUnit Description=""Cubic Decimeters"">D3</VolumeUnit>
                <Weight>265000</Weight>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                <Width></Width>
                <UNDGCollection>
                </UNDGCollection>
            </PackingLine>
			<PackingLine>
                <ContainerLink></ContainerLink>
                <ContainerNumber>TCLU1234567</ContainerNumber>
                <DetailedDescription>Test packing line</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <HarmonisedCode></HarmonisedCode>
                <Height></Height>
                <ImportReferenceNumber></ImportReferenceNumber>
                <Length></Length>
                <LengthUnit Description=""Meters""></LengthUnit>
                <MarksAndNos>
                        Ye Olde marks and nos
                </MarksAndNos>
                <PackQty>69</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber></ReferenceNumber>
                <Volume>420000</Volume>
                <VolumeUnit Description=""Cubic Decimeters"">D3</VolumeUnit>
                <Weight>6969</Weight>
                <WeightUnit Description=""Kilograms"">KG</WeightUnit>
                <Width></Width>
                <UNDGCollection>
                </UNDGCollection>
            </PackingLine>
        </PackingLineCollection>

        <TransportLegCollection>
            <TransportLeg>
                <PortOfDischarge Name=""Jebel Ali"">AEZZZ</PortOfDischarge>
                <PortOfLoading Name=""Antwerp"">BEANR</PortOfLoading>
                <LegOrder>1</LegOrder>
                <EstimatedArrival>2016-09-02T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-01T22:00:00</EstimatedDeparture>
                <LegType>PreCarriage</LegType>
                <TransportMode>Sea</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Sydney"">AUSYD</PortOfDischarge>
                <PortOfLoading Name=""Perth"">AUPER</PortOfLoading>
                <LegOrder>2</LegOrder>
                <EstimatedArrival>2016-09-07T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-03T22:00:00</EstimatedDeparture>
                <LegType>Main</LegType>
                <TransportMode>Rail</TransportMode>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
            <TransportLeg>
                <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
                <PortOfLoading Name=""Auckland"">NZAKL</PortOfLoading>
                <LegOrder>3</LegOrder>
                <EstimatedArrival>2016-09-19T23:00:00</EstimatedArrival>
                <EstimatedDeparture>2016-09-08T22:00:00</EstimatedDeparture>
                <LegType>OnForwarding</LegType>
                <VGMCutOff>2016-09-01T21:00:00</VGMCutOff>
                <DocumentCutOff>2016-09-02T21:00:00</DocumentCutOff>
                <FCLCutOff>2016-09-02T21:00:00</FCLCutOff>
                <VesselLloydsIMO>9398400</VesselLloydsIMO>
                <VesselName>CCNI ARAUCO</VesselName>
                <VoyageFlightNo>004EHE</VoyageFlightNo>
                <Carrier>
                    <AddressType>Carrier</AddressType>
                    <RegistrationNumberCollection>
                        <RegistrationNumber>
                            <Type>CCC</Type>
                            <CountryOfIssue>US</CountryOfIssue>
                            <Value>SUDU</Value>
                        </RegistrationNumber>
                    </RegistrationNumberCollection>
                </Carrier>
            </TransportLeg>
        </TransportLegCollection>
    </Shipment>
</UniversalShipment>";

		#endregion
	}
}
