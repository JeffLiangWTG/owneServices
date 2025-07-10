using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;
using static Enterprise.Core.Constants.GateManagementConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteBookingDataContextManager))]
	public class GteBookingDataContextManagerTest : ShipmentDataContextManagerTestCase<GteBookingDataContextManager, GteBooking>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		public void TestMatchingByDataContextKey()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB00001204";

			var facilityCompany = Factory.NewWithValidTestData<OrgHeader>();
			facilityCompany.OH_Code = "XYZ";
			facilityCompany.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC123", string.Empty);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = facilityCompany.MainAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_IsActive = true;

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.GateBooking, "GB00001204");

			var transportCompanyAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transportCompanyAddress.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			transportCompanyAddress.OrganizationCode = "KALTEC";

			var facilityAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			facilityAddress.AddressType = nameof(DocAddressType.LocalCartageYard);
			facilityAddress.Address1 = "1 main st";
			facilityAddress.City = "Sydney";
			facilityAddress.Postcode = "2020";
			facilityAddress.State = "NSW";
			facilityAddress.Country = new Country() { Code = "AU" };

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				transportCompanyAddress,
				facilityAddress
			});

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC123" };
			facilityAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var vehicleMovement = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun = new VehicleRun();
			var vehicle = new Vehicle();
			var vehicleRegistration = new Registration();

			vehicleRegistration.Number = "TESTING123";
			vehicle.Registration = vehicleRegistration;
			vehicleRun.Vehicle = vehicle;
			vehicleMovement.VehicleRun = vehicleRun;

			shipment.SetPreCarriageShipmentCollection(() => new List<Shipment> { vehicleMovement });

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = "GateMovementBooking1";
			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };

			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });

			var crew = new Crew();
			crew.FullName = "Thomas Jefferson";
			crew.LicenseNumber = "12345678";
			crew.CrewType = CrewType.Driver;

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew> { crew });

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			CombineAssertions(() =>
			{
				AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added Gate Movement Booking from UniversalShipment.
Added GteVehicleMovementBooking from UniversalShipment.
Added GteVehicleDriverBooking from UniversalShipment.
Updated Gate Booking GB00001204 from UniversalShipment.
Successfully saved Gate Booking GB00001204 with 1 x GteGateMovementBooking, 1 x GteVehicleMovementBooking, 1 x GteVehicleDriverBooking.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Expected message to have the log note text: ", @"
Successfully loaded matching GteBooking.
Populating GteBooking...
Matching 'TransportCompanyDocumentaryAddress':- Matched to 'KALTEC' by code, main address used.
No matching GteGateMovementBooking found, creating new GteGateMovementBooking.
Populating GteGateMovementBooking...
Added Gate Movement Booking from UniversalShipment.
No matching GteVehicleMovementBooking found, creating new GteVehicleMovementBooking.
Populating GteVehicleMovementBooking...
Added GteVehicleMovementBooking from UniversalShipment.
No matching GteVehicleDriverBooking found, creating new GteVehicleDriverBooking.
Populating GteVehicleDriverBooking...
Added GteVehicleDriverBooking from UniversalShipment.
Matching 'LocalCartageYard':- Matched to address '#1' on 'XYZ' by Container Chain community code 'CC123'
Try link matching facility jobs
Imported UXML does not contain a Facility Type
Updated Gate Booking GB00001204 from UniversalShipment.
Successfully saved Gate Booking GB00001204 with 1 x GteGateMovementBooking, 1 x GteVehicleMovementBooking, 1 x GteVehicleDriverBooking.
".Trim(), logNoteText);
			});
		}

		#region Implementation

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override GteBooking GetNewBusinessObjectForTesting()
		{
			base.GetNewBusinessObjectForTesting();

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB00001204";

			return booking;
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB00001204";

			var facilityCompany = Factory.NewWithValidTestData<OrgHeader>();
			facilityCompany.OH_Code = "XYZ";
			facilityCompany.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC123", string.Empty);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = facilityCompany.MainAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_IsActive = true;

			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>GateBooking</Type>
          <Key>GB00001204</Key>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ATW</Code>
          <Description>Arrival Transit Warehouse</Description>
          <ServiceCode>GTB</ServiceCode>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <BookingConfirmationReference>GB00001204</BookingConfirmationReference>
    <Branch>
      <Code>A01</Code>
      <Name>AU - Branch 1</Name>
    </Branch>
    <FacilityJobType>
      <Code>CTR</Code>
      <Description>Container</Description>
    </FacilityJobType>
    <SlotDateTime>2022-05-01T08:00:00</SlotDateTime>
    <VehicleRun>
      <Vehicle>
        <Registration>
          <Number>TRUCK1</Number>
        </Registration>
        <VehicleType>
          <Code>RTR</Code>
          <Description>Truck</Description>
        </VehicleType>
      </Vehicle>

      <CrewCollection>
        <Crew>
          <CrewType>Driver</CrewType>
          <FullName>DRIVER1</FullName>
		  <LicenseNumber>DRIVER1</LicenseNumber>
        </Crew>
      </CrewCollection>
    </VehicleRun>

    <ContainerCollection Content=""Complete"">
	  <Container>
        <ContainerNumber>KKLU2367874</ContainerNumber>
        <ContainerQuality>
          <Code>CFG</Code>
          <Description>Container Clean - Food Grade</Description>
        </ContainerQuality>
        <ContainerStatus>
          <Code>AVL</Code>
          <Description>Available</Description>
        </ContainerStatus>
        <ContainerType>
          <Code>40GP</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>GENERAL PURPOSE CONT.</Description>
          <ISOCode>40GP</ISOCode>
        </ContainerType>
        <EmptyReturnedBy>2022-05-09T00:00:00</EmptyReturnedBy>
        <FCLStorageArrivedUnderbond>true</FCLStorageArrivedUnderbond>
        <FCLStorageCommences>2022-05-02T00:00:00</FCLStorageCommences>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <GateInDate></GateInDate>
        <GateOutDate></GateOutDate>
        <GrossWeight>15000.000</GrossWeight>
        <ImportDepotCustomsReference>CLR</ImportDepotCustomsReference>
        <IsControlledAtmosphere>true</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <LCLStorageCommences></LCLStorageCommences>
        <Link>1</Link>
        <ReleaseNum></ReleaseNum>
        <Seal>SEAL01</Seal>
        <SealPartyType>
          <Code></Code>
        </SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType>
          <Code></Code>
        </SecondSealPartyType>
        <SetPointTemp>4.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>25000.000</TareWeight>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType>
          <Code></Code>
        </ThirdSealPartyType>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ContainerOwnerAddress</AddressType>
            <Address1></Address1>
            <Address2></Address2>
            <AddressOverride></AddressOverride>
            <AddressShortCode></AddressShortCode>
            <City></City>
            <CompanyName>AU MANUFACTURING CORPORATION</CompanyName>
            <Country>
              <Code></Code>
              <Name></Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AUMNFGSYD</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
              <Name></Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code></Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State Description=""""></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ContainerNumber>KKLU2367827</ContainerNumber>
        <ContainerQuality>
          <Code>CFG</Code>
          <Description>Container Clean - Food Grade</Description>
        </ContainerQuality>
        <ContainerStatus>
          <Code>AVL</Code>
          <Description>Available</Description>
        </ContainerStatus>
        <ContainerType>
          <Code>22G1</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>GENERAL PURPOSE CONT.</Description>
          <ISOCode>22G1</ISOCode>
        </ContainerType>
        <EmptyReturnedBy>2022-04-09T00:00:00</EmptyReturnedBy>
        <FCLStorageArrivedUnderbond>true</FCLStorageArrivedUnderbond>
        <FCLStorageCommences>2022-04-02T00:00:00</FCLStorageCommences>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <GateInDate></GateInDate>
        <GateOutDate></GateOutDate>
        <GrossWeight>15000.000</GrossWeight>
        <ImportDepotCustomsReference>CLR</ImportDepotCustomsReference>
        <IsControlledAtmosphere>true</IsControlledAtmosphere>
        <IsEmptyContainer>true</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <LCLStorageCommences></LCLStorageCommences>
        <Link>2</Link>
        <ReleaseNum></ReleaseNum>
        <Seal>SEAL02</Seal>
        <SealPartyType>
          <Code></Code>
        </SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType>
          <Code></Code>
        </SecondSealPartyType>
        <SetPointTemp>4.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>25000.000</TareWeight>
        <ThirdSeal></ThirdSeal>
        <ThirdSealPartyType>
          <Code></Code>
        </ThirdSealPartyType>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ContainerOwnerAddress</AddressType>
            <Address1></Address1>
            <Address2></Address2>
            <AddressOverride></AddressOverride>
            <AddressShortCode></AddressShortCode>
            <City></City>
            <CompanyName>AU MANUFACTURING CORPORATION</CompanyName>
            <Country>
              <Code></Code>
              <Name></Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AUMNFGSYD</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
              <Name></Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code></Code>
              <Description></Description>
            </ScreeningStatus>
            <State Description=""""></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>TransportCompanyDocumentaryAddress</AddressType>
        <Address1></Address1>
        <Address2></Address2>
        <AddressOverride></AddressOverride>
        <AddressShortCode></AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Country>
          <Code></Code>
          <Name></Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>UNIULU</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
          <Name></Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code></Code>
          <Description></Description>
        </ScreeningStatus>
        <State Description=""""></State>
		<RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>C1R</Code>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUTRASYD</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Warehouse</AddressType>
        <Address1></Address1>
        <Address2></Address2>
        <AddressOverride></AddressOverride>
        <AddressShortCode></AddressShortCode>
        <City></City>
        <CompanyName>AU CONTAINER YARD</CompanyName>
        <Country>
          <Code></Code>
          <Name></Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AUCONTSYD</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code></Code>
          <Description></Description>
        </ScreeningStatus>
        <State Description=""""></State>
		<RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>C1R</Code>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUTRASYD</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
	  <OrganizationAddress>
        <AddressType>LocalCartageYard</AddressType>
        <Address1></Address1>
        <Address2></Address2>
        <AddressOverride></AddressOverride>
        <AddressShortCode></AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Country>
          <Code></Code>
          <Name></Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>XYZ</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code></Code>
          <Description></Description>
        </ScreeningStatus>
        <State Description=""""></State>
		<RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CC1</Code>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>CC123</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <BookingConfirmationReference>BKR01</BookingConfirmationReference>
        <SlotReference></SlotReference>
        <TransportBookingDirection>
          <Code>PIC</Code>
          <Description>Pickup</Description>
        </TransportBookingDirection>
        <WayBillNumber>MBL1</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>BookingPartyDocumentaryAddress</AddressType>
            <Address1></Address1>
            <Address2></Address2>
            <AddressOverride></AddressOverride>
            <AddressShortCode></AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Country>
              <Code></Code>
              <Name></Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AU11RESYD</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
              <Name></Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code></Code>
              <Description></Description>
            </ScreeningStatus>
            <State Description=""""></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection Content=""Complete"">
          <PackingLine>
            <Commodity>
              <Code>GENL</Code>
              <Description>GENERAL</Description>
            </Commodity>
            <ContainerLink>1</ContainerLink>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <BookingConfirmationReference>CBR0104B</BookingConfirmationReference>
        <SlotReference></SlotReference>
        <WayBillNumber>MBL0104B</WayBillNumber>
        <TransportBookingDirection>
          <Code>DLV</Code>
          <Description>Delivery</Description>
        </TransportBookingDirection>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>BookingPartyDocumentaryAddress</AddressType>
            <Address1></Address1>
            <Address2></Address2>
            <AddressOverride></AddressOverride>
            <AddressShortCode></AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Country>
              <Code></Code>
              <Name></Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AU11RESYD</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
              <Name></Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code></Code>
              <Description></Description>
            </ScreeningStatus>
            <State Description=""""></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection Content=""Complete"">
          <PackingLine>
            <Commodity>
              <Code>GENL</Code>
              <Description>GENERAL</Description>
            </Commodity>
            <ContainerLink>2</ContainerLink>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <PreCarriageShipmentCollection>
      <PreCarriageShipment>
        <VehicleRun>
          <Vehicle>
            <Registration>
              <Number>DEF-023</Number>
            </Registration>
            <VehicleType>
              <Code>RTRK</Code> 
              <Description>Truck</Description>
            </VehicleType>
          </Vehicle>
        </VehicleRun>
      </PreCarriageShipment>
    </PreCarriageShipmentCollection>
  </Shipment>
</UniversalShipment>";
	}

	#endregion Implementation
}
