using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(BillOfLadingDataContextManager))]
	sealed class BillOfLadingDataContextManagerTest : AgencyShipmentDataContextManagerTest<BillOfLadingDataContextManager, BillOfLading>
	{
		public void TestDataContextType()
		{
			var manager = GetNewDataContextManager();
			AssertEquals(DataContextType.BillOfLading, manager.DataContextType);
		}

		public void TestImportUniversalShipmentWithEmptyShipmentStatus()
		{
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>BillOfLading</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>");
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
No matching BillOfLading found, creating new BillOfLading.
Populating BillOfLading...
Added Shipping Bill of Lading  from UniversalShipment.
Successfully saved Shipping Bill of Lading V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipmentWithBookingStatus()
		{
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>BillOfLading</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <ShipmentStatus>
      <Code>BKD</Code>
      <Description>Booked</Description>
    </ShipmentStatus>
  </Shipment>
</UniversalShipment>");
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var preimportCount = Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking));
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Error - Cannot populate BillOfLading because:
Data object contains shipment status [BKD] that is invalid in this scope.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
			AssertEquals("no bookings have been added to the database", preimportCount, Factory.BOFactory.GetDatabaseCount(typeof(AgencyBooking)));
		}

		public void TestImportUniversalShipmentWithBillOfLadingStatus()
		{
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>BillOfLading</Type>
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
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
No matching BillOfLading found, creating new BillOfLading.
Populating BillOfLading...
Added Shipping Bill of Lading  from UniversalShipment.
Successfully saved Shipping Bill of Lading V00001000.
".Trim(), message.GetLogNoteText());
		}

		public void TestImportUniversalShipment_ServiceCodeSIN()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>CAR</Code>
				<Description>Carrier</Description>
				<ServiceCode>SIN</ServiceCode>
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
No matching BillOfLading found, creating new BillOfLading.
Populating BillOfLading...
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
		<DataTargetCollection>
			<DataTarget>
				<Type>BillOfLading</Type>
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
Error - Cannot populate BillOfLading because:
XML file contains VGM service code and cannot find a matched Agency Shipment.
Successfully saved, but nothing was reported as being updated.".Trim(), message.GetLogNoteText());
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

		public void TestImportUniversalShipment_WithAgentReferenceOnly()
		{
			#region message
			var message = GetQueuedUniversalShipmentMessage(@"<UniversalShipment>
  <Shipment>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Type>BillOfLading</Type>
			</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<AgentsReference>AGTREF</AgentsReference>
	<BookingConfirmationReference></BookingConfirmationReference>
	<WayBillNumber></WayBillNumber>
	<OrganizationAddressCollection>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>");
			#endregion
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("import log", @"
Error - Cannot populate BillOfLading because:
XML file does not contains any Ocean Bill Number or Carrier Booking Reference Number.The Booking Party is required when only Agent Reference Number provided.
Successfully saved, but nothing was reported as being updated.
".Trim(), message.GetLogNoteText());
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
				var message = GetQueuedUniversalShipmentMessage(xml);
				var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
				manager.Process(message);

				AssertMultilineASCIIEquals(@"Error - Cannot populate AgencyBooking because:
[*VGM message with Bill Number BL00008 cannot find a matching Bill of Lading to update. Message rejected.*]
Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());

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
					return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.BillOfLading.TestFiles.FullyPopulatedBillOfLading_UniversalShipment.xml");
				}
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new RecipientRoleType[] { RecipientRoleType.SPM, RecipientRoleType.CAR };

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return base.SupportedRecipientServices(recipientRole).Where(s => !ServiceCodeType.BRQ.Equals(s)).ToArray();
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
