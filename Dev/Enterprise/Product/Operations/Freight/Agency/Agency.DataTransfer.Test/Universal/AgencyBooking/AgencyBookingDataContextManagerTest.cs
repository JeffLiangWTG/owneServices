using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AgencyBookingDataContextManager))]
	sealed class AgencyBookingDataContextManagerTest : AgencyShipmentDataContextManagerTest<AgencyBookingDataContextManager, AgencyBooking>
	{
		public void TestDataContextType()
		{
			var manager = GetNewDataContextManager();
			AssertEquals(DataContextType.AgencyBooking, manager.DataContextType);
		}

		public void TestImportUniversalShipmentWithEmptyShipmentStatus()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<BookingConfirmationReference>A</BookingConfirmationReference>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipmentWithBookingStatus()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<ShipmentStatus>
	  <Code>BKD</Code>
	  <Description>Booked</Description>
	</ShipmentStatus>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipmentWithBillOfLadingStatus()
		{
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<ShipmentStatus>
	  <Code>CNF</Code>
	  <Description>Confirmed</Description>
	</ShipmentStatus>
  </Shipment>
</UniversalShipment>");
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var preimportCount = Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking));
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Error - Cannot populate AgencyBooking because:
XML file contains shipment status [CNF] that is invalid in this scope.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
			AssertEquals("no bookings have been added to the database", preimportCount, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestImportUniversalShipment_ServiceCodeBRQ()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>BRQ</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Added Shipping Shipment  from UniversalShipment.
Successfully saved Shipping Shipment V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_ServiceCodeVGM()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertXMLContains(@"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate AgencyBooking because:
[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_ServiceCodeVGM_IsNotCarrierVGM()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>AgencyBooking</Type>
			</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CBA</Code>
				<Description>Carrier Booking Agent</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertXMLContains(@"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate AgencyBooking because:
XML file contains VGM service code and cannot find a matched Agency Shipment.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_ServiceCodeVGM_WithoutBookingParty()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>AgencyBooking</Type>
			</DataTarget>
		</DataTargetCollection>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CBA</Code>
				<Description>Carrier Booking Agent</Description>
				<ServiceCode>VGM</ServiceCode>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertXMLContains(@"
Error - Cannot populate AgencyBooking because:
XML file contains VGM service code and cannot find a matched Agency Shipment.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			});
		}

		public void TestImportUniversalShipment_ServiceCodeIsEmpty()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>A</BookingConfirmationReference>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertXMLContains(@"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate BillOfLading because:
Cannot determine which type of Agency Shipment to create.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
			AssertXMLContains(@"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Error - Cannot populate AgencyBooking because:
Cannot determine which type of Agency Shipment to create.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_BookingPartyIsEmpty()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<AgentsReference>AGT001</AgentsReference>
	<BookingConfirmationReference></BookingConfirmationReference>
	<WayBillNumber></WayBillNumber>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>SGSIN</Code>
		  <Name>Singapore</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>1</LegOrder>
		<LegType>Main</LegType>
		<TransportMode>Sea</TransportMode>
		<VesselName>MAERSK VESSEL</VesselName>
		<VoyageFlightNo>V001</VoyageFlightNo>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertEquals("Precondition: no booking in database.", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			manager.Process(message);
			Factory.SaveForTesting();
			AssertMultilineASCIIEquals("import log", @"
Error - Cannot populate AgencyBooking because:
XML file does not contains any Ocean Bill Number or Carrier Booking Reference Number.The Booking Party is required when only Agent Reference Number provided.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
			AssertEquals("Booking Party is empty, should not create new booking", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestImportUniversalShipment_MatchedExistingBooking_WithAgentReferenceOnly()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			var agentBooking = Factory.NewWithValidTestData<AgencyBooking>();
			agentBooking.JS_HouseBill = "";
			agentBooking.JS_CFSReference = "";
			agentBooking.JS_BookingReference = "AGTREF";
			agentBooking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			agentBooking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			AssertEquals("New booking have been added to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
				<Shipment>
					<DataContext>
						<DataTargetCollection>
						<DataTarget>
							<Type>AgencyBooking</Type>
						</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<AgentsReference>AGTREF</AgentsReference>
					<BookingConfirmationReference></BookingConfirmationReference>
					<WayBillNumber></WayBillNumber>
					<OrganizationAddressCollection>
					</OrganizationAddressCollection>
					<TransportLegCollection>
						<TransportLeg>
						<PortOfDischarge>
							<Code>SGSIN</Code>
							<Name>Singapore</Name>
						</PortOfDischarge>
						<PortOfLoading>
							<Code>AUSYD</Code>
							<Name>Sydney</Name>
						</PortOfLoading>
						<LegOrder>1</LegOrder>
						<LegType>Main</LegType>
						<TransportMode>Sea</TransportMode>
						<VesselName>MAERSK VESSEL</VesselName>
						<VoyageFlightNo>V001</VoyageFlightNo>
						</TransportLeg>
					</TransportLegCollection>
				</Shipment>
			</UniversalShipment>");
			#endregion
			UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			AssertMultilineASCIIEquals("import log", @"
Successfully loaded matching AgencyBooking.
Error - Cannot populate AgencyBooking because:
XML file does not contains any Ocean Bill Number or Carrier Booking Reference Number.The Booking Party is required when only Agent Reference Number provided.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_SendingForwarderAddress()
		{
			var forwarderAddress = Factory.New<OrgHeader>();
			forwarderAddress.MainAddress.OA_Address1 = "Forwarder Address 1";
			forwarderAddress.OH_Code = "SYFORWA";
			forwarderAddress.OH_FullName = "FORWADER COMPANY PTY LTD";
			Factory.SaveForTesting();
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>SendingForwarderAddress</AddressType>
		<OrganizationCode>SYFORWA</OrganizationCode>
		<Address1>Forwarder Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<BookingConfirmationReference>A</BookingConfirmationReference>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Should add new booking to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			AssertMultilineASCIIEquals("import log", @"
Matching 'SendingForwarderAddress':- Matched to 'SYFORWA' by code, address 'Forwarder Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'SendingForwarderAddress':- Matched to 'SYFORWA' by code, address 'Forwarder Address 1' (only address).
Matching 'BookingPartyDocumentaryAddress':- Matched to 'SYFORWA' by code, address 'Forwarder Address 1' (only address).
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_CreateNewBookingWithSalling()
		{
			AssertCreateNewAgencyBookingWithSailing(true);
		}

		public void TestImportUniversalShipment_CreateNewBookingWithoutSalling()
		{
			AssertCreateNewAgencyBookingWithSailing(false);
		}

		void AssertCreateNewAgencyBookingWithSailing(bool createSailing)
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<AgentsReference>AGT001</AgentsReference>
	<BookingConfirmationReference></BookingConfirmationReference>
	<WayBillNumber>HBL001</WayBillNumber>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<CompanyName>BKG Company Pty Ltd</CompanyName>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>SGSIN</Code>
		  <Name>Singapore</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>1</LegOrder>
		<LegType>Main</LegType>
		<TransportMode>Sea</TransportMode>
		<VesselName>MAERSK VESSEL</VesselName>
		<VoyageFlightNo>V001</VoyageFlightNo>
		<LCLAvailability></LCLAvailability>
		<LCLCutOff></LCLCutOff>
		<LCLReceivalCommences></LCLReceivalCommences>
		<LCLStorageDate>30-Aug-2016</LCLStorageDate>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var sailing = createSailing ? UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001") : null;
			if (sailing != null)
			{
				sailing.JX_DepotCutOff = new ZDateTime(2016, 8, 25);
				sailing.JX_DepotReceivalCommences = new ZDateTime(2016, 8, 20);
				sailing.JX_DepotAvailabilityDate = new ZDateTime(2016, 8, 20);
				sailing.JX_DepotStorageDate = new ZDateTime(2016, 9, 25);
			}

			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertEquals("Precondition: no booking in database.", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			manager.Process(message);
			Factory.SaveForTesting();
			var agencyBooking = Factory.LoadTop1<AgencyBooking>(new ZQuery());
			AssertEquals("Shipper's Ref", "AGT001", agencyBooking.JS_BookingReference);
			AssertEquals("Ocean Bill Number", "HBL001", agencyBooking.JS_HouseBill);
			AssertEquals("Booking Party", "BKGPARTY", agencyBooking.BookingParty.OH_Code);
			AssertEquals("Booking Party", sailing, agencyBooking.Sailing);
			AssertEquals("New booking have been added to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			AssertMultilineASCIIEquals("import log", (createSailing ? logsWithSailing : logsWithoutSailing).Trim(), message.GetLogNoteText());
			manager.Process(message);
			Factory.SaveForTesting();
			AssertEquals("Should not add new booking to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			agencyBooking = Factory.LoadTop1<AgencyBooking>(new ZQuery());
			AssertEquals("Shipper's Ref", "AGT001", agencyBooking.JS_BookingReference);
			AssertEquals("Ocean Bill Number", "HBL001", agencyBooking.JS_HouseBill);
			AssertEquals("Booking Party", "BKGPARTY", agencyBooking.BookingParty.OH_Code);
			if (sailing != null)
			{
				var anotherFactory = new BusinessObjectFactory();
				var sailing2 = anotherFactory.LoadTop1<JobSailing>(new ZQuery(JobSailingSchema.PK, sailing.PK));
				AssertNotNull("Should linked to existing Sailing Schedule", agencyBooking.Sailing);
				AssertEquals(sailing2.PK, agencyBooking.Sailing.PK);
				AssertEquals(sailing2.JX_DepotCutOff, agencyBooking.Sailing.JX_DepotCutOff);
				AssertEquals(sailing2.JX_DepotReceivalCommences, agencyBooking.Sailing.JX_DepotReceivalCommences);
				AssertEquals(sailing2.JX_DepotAvailabilityDate, agencyBooking.Sailing.JX_DepotAvailabilityDate);
				AssertEquals(sailing2.JX_DepotStorageDate, agencyBooking.Sailing.JX_DepotStorageDate);
			}
			else
			{
				AssertNull("Should not create linked Sailing Schedule", agencyBooking.Sailing);
			}
		}

		const string logsWithSailing = @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: SGSIN
Attempting to get Schedule for the Transport Leg
A Schedule will not be created. Attempting to find an existing Schedule...
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000 with 1 x Transport.";
		const string logsWithoutSailing = @"Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: SGSIN
Attempting to get Schedule for the Transport Leg
An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
An existing Schedule could not be found. The Transport Leg will not be linked.
Transport Leg updated.
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000 with 1 x Transport.
";
		public void TestImportUniversalShipment_AgencyBookingExistsWithBookingRef()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<AgentsReference>AGT001</AgentsReference>
	<BookingConfirmationReference></BookingConfirmationReference>
	<WayBillNumber></WayBillNumber>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>SGSIN</Code>
		  <Name>Singapore</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>1</LegOrder>
		<LegType>Main</LegType>
		<TransportMode>Sea</TransportMode>
		<VesselName>MAERSK VESSEL</VesselName>
		<VoyageFlightNo>V001</VoyageFlightNo>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			AssertEquals("Precondition: no booking in database.", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			var agentBooking = Factory.NewWithValidTestData<AgencyBooking>();
			agentBooking.JS_CFSReference = "BKG001";
			agentBooking.JS_BookingReference = "AGT001";
			agentBooking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			agentBooking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			AssertEquals("New booking have been added to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: SGSIN
Attempting to get Schedule for the Transport Leg
A Schedule will not be created. Attempting to find an existing Schedule...
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Updated Shipping Booking EBM22Q33TU475BXH3P60 from UniversalShipment.
Successfully saved Shipping Booking EBM22Q33TU475BXH3P60 with 1 x Transport.
".Trim(), message.GetLogNoteText());
			AssertEquals("Should not add new booking to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestImportUniversalShipment_CarrierBookingRefAgentRef()
		{
			AssertNewAgencyBookingWithReference("AGTREF001", "BKGREF001");
		}

		public void TestImportUniversalShipment_CarrierBookingRefIsEmpty()
		{
			AssertNewAgencyBookingWithReference("AGTREF001", ZString.Empty);
		}

		public void TestImportUniversalShipment_AgentRefIsEmpty()
		{
			AssertNewAgencyBookingWithReference(ZString.Empty, "BKGREF001");
		}

		public void TestImportUniversalShipment_CarrierBookingRefAgentRefAreEmpty()
		{
			AssertNewAgencyBookingWithReference(ZString.Empty, ZString.Empty);
		}

		void AssertNewAgencyBookingWithReference(ZString agentReference, ZString bookingReference)
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(string.Format(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<AgentsReference>{0}</AgentsReference>
	<BookingConfirmationReference>{1}</BookingConfirmationReference>
	<WayBillNumber></WayBillNumber>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>SGSIN</Code>
		  <Name>Singapore</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>1</LegOrder>
		<LegType>Main</LegType>
		<TransportMode>Sea</TransportMode>
		<VesselName>MAERSK VESSEL</VesselName>
		<VoyageFlightNo>V001</VoyageFlightNo>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>", agentReference, bookingReference));
			#endregion
			AssertEquals("Precondition: no booking in database.", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			Factory.SaveForTesting();
			AssertEquals("Should not add new booking to the database", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching AgencyBooking found, creating new AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: SGSIN
Attempting to get Schedule for the Transport Leg
A Schedule will not be created. Attempting to find an existing Schedule...
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Added Shipping Booking  from UniversalShipment.
Successfully saved Shipping Booking V00001000 with 1 x Transport.
".Trim(), message.GetLogNoteText());
			AssertEquals("Should not add new booking to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestImportUniversalShipment_AgencyBookingExistsWithOverrideBookingPartyAddress()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>AgencyBooking</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<AgentsReference>AGT001</AgentsReference>
	<BookingConfirmationReference></BookingConfirmationReference>
	<WayBillNumber></WayBillNumber>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>BookingPartyDocumentaryAddress</AddressType>
		<CompanyName>BKG PARty pty ltd</CompanyName>
		<OrganizationCode>BKGPARTY</OrganizationCode>
		<Address1>Booking Party Address 1</Address1>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<TransportLegCollection>
	  <TransportLeg>
		<PortOfDischarge>
		  <Code>SGSIN</Code>
		  <Name>Singapore</Name>
		</PortOfDischarge>
		<PortOfLoading>
		  <Code>AUSYD</Code>
		  <Name>Sydney</Name>
		</PortOfLoading>
		<LegOrder>1</LegOrder>
		<LegType>Main</LegType>
		<TransportMode>Sea</TransportMode>
		<VesselName>MAERSK VESSEL</VesselName>
		<VoyageFlightNo>V001</VoyageFlightNo>
	  </TransportLeg>
	</TransportLegCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			AssertEquals("Precondition: no booking in database.", 0, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "SGSIN", "MAERSK VESSEL", "V001");
			var agentBooking1 = Factory.NewWithValidTestData<AgencyBooking>();
			agentBooking1.JS_CFSReference = "BKG001";
			agentBooking1.JS_BookingReference = "AGT001";
			agentBooking1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			agentBooking1.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			agentBooking1.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG PARTY PTY LTD";
			agentBooking1.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			AssertMultilineASCIIEquals("import log", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching AgencyBooking.
Populating AgencyBooking...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: SGSIN
Attempting to get Schedule for the Transport Leg
A Schedule will not be created. Attempting to find an existing Schedule...
A Schedule has been found and linked to the Transport Leg.
Transport Leg updated.
Updated Shipping Booking EBM22Q33TU475BXH3P60 from UniversalShipment.
Successfully saved Shipping Booking EBM22Q33TU475BXH3P60 with 1 x Transport.
".Trim(), message.GetLogNoteText());
			AssertEquals("Should not add new booking to the database", 1, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestIsProcessing()
		{
			#region xml

			var xml = @"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>VGM</ServiceCode>
				<ServiceDescription>Verified Gross Container Weight</ServiceDescription>
			</RecipientRole>
		</RecipientRoleCollection>
	</DataContext>
	<BookingConfirmationReference>BK00008</BookingConfirmationReference>
	<WayBillNumber>BL00008</WayBillNumber>
	<ContainerCollection Content=""Partial"">
		<Container>
			<ContainerNumber>TEST8888888</ContainerNumber>
			<ContainerType>
				<Code>20GP</Code>
				<ISOCode>20GP</ISOCode>
			</ContainerType>
			<GrossWeight>5000.000</GrossWeight>
			<GrossWeightVerificationDateTime>2024-03-13T08:45:00</GrossWeightVerificationDateTime>
			<GrossWeightVerificationType>
				<Code>CNT</Code>
			</GrossWeightVerificationType>
			<Seal />
			<WeightUnit>
				<Code>KG</Code>
			</WeightUnit>
			<OrganizationAddressCollection>
				<OrganizationAddress>
					<AddressType>GrossWeightVerifiedBy</AddressType>
					<OrganizationCode>BKGPARTY</OrganizationCode>
					<Address1>Booking Party Address 1</Address1>
				</OrganizationAddress>
			</OrganizationAddressCollection>
		</Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agentBooking = Factory.NewWithValidTestData<AgencyBooking>();
				agentBooking.JS_CFSReference = "BK00008";
				agentBooking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				var agentBookingContainer = agentBooking.RealContainers.AddNew();
				agentBookingContainer.JC_ContainerNum = "TEST8888888";
				agentBookingContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				agentBookingContainer.JC_TareWeight = 2000;
				agentBookingContainer.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				agentBookingContainer.JC_GrossWeight = 4500m;

				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(xml);
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching AgencyBooking.
Populating AgencyBooking...
Successfully loaded matching AgencyBookingContainer.
Populating AgencyBookingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipping Booking EBM22Q33TU475BXH3P60 from UniversalShipment.
Successfully saved Shipping Booking EBM22Q33TU475BXH3P60 with 1 x AgencyBookingContainer.", message.GetLogNoteText());

				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				billOfLading.JS_HouseBill = "BL00008";
				billOfLading.JS_CFSReference = "BK00008";
				billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

				var container = billOfLading.RealContainers.AddNew();
				container.JC_ContainerNum = "TEST8888888";
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_TareWeight = 2000;
				container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				container.JC_GrossWeight = 4500m;

				Factory.SaveForTesting();

				manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				message = GetQueuedUniversalShipmentMessage(xml);
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Successfully loaded matching BillOfLading.
Populating BillOfLading...
Successfully loaded matching BillOfLadingContainer.
Populating BillOfLadingContainer...
Successfully loaded matching Container Type.
Matching 'GrossWeightVerifiedBy':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Updated Shipping Bill of Lading V00001000 from UniversalShipment.
Successfully saved Shipping Bill of Lading V00001000 with 1 x BillOfLadingContainer.", message.GetLogNoteText());
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				using (var retriever = new EmbeddedResourceRetriever())
				{
					return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyBooking.TestFiles.FullyPopulatedAgencyBooking_UniversalShipment.xml");
				}
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return new[] { RecipientRoleType.CAR };
			}
		}

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return base.SupportedRecipientServices(recipientRole).Where(s => !ServiceCodeType.SIN.Equals(s)).ToArray();
		}

		protected override void SetUp()
		{
			base.SetUp();
			bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			Factory.SaveForTesting();
		}

		OrgHeader bookingParty;
	}
}
