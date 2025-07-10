using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class ContainerMovementMessageProcessorTest : BaseAgencyTest
	{
		#region Implementation
		DummyCMMProcessingAdapter GetProcessingAdapterWithDefaults()
		{
			Sender = Factory.NewWithValidTestData<OrgHeader>();
			Sender.OH_FullName = "TestSender";

			Vessel = Factory.NewWithValidTestData<RefVessel>();
			Vessel.RV_LloydsNumber = "TVessel";
			Vessel.RV_Name = "FOO";

			Voyage = Factory.NewWithValidTestData<JobVoyage>();
			Voyage.JV_VoyageFlight = "VoyNumber";
			Voyage.JV_RV_NKVessel = Vessel.RV_FK;
			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			Voyage.GenerateSailings();

			Sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NZAKL");

			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "22G0"));
			Container = Factory.New<RefContainerStock>();
			Container.R6_ContainerNum = "MWHE9999990";
			Container.R6_RC = containerType.PK;

			Shipment = Factory.NewWithValidTestData<AgencyShipment>();
			Shipment.JS_CFSReference = "BookRef";
			Shipment.JS_HouseBill = "BillOfLading";
			Shipment.JS_UniqueConsignRef = "ABCDEF54321";
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_JX = Sailing.PK;

			var ediMessage = Factory.NewWithValidTestData<EDIMessage>();

			var adapter = new DummyCMMProcessingAdapter
			{
				Factory = Factory,
				messageText = "Test Message Content",
				messageType = CMMMessageType.Load,
				senderAddress = Sender.MainAddress,
				lloydsNumber = "TVessel",
				voyageNumber = "VoyNumber",
				message = ediMessage,
				transportMode = "ROA",
				containers = new List<CMMMessageContainer>
				{
					new CMMMessageContainer
					{
						ContainerNumber = "MWHE9999990",
						ISOType = "22G0",
						BookingReference = "BookRef",
						BillOfLading = "BillOfLading",
						SealNumbers = new List<string>()
					}
				}
			};

			return adapter;
		}

		AgencyShipment Shipment;
		OrgHeader Sender;
		RefVessel Vessel;
		JobVoyage Voyage;
		JobSailing Sailing;
		RefContainerStock Container;

		#endregion
		public void TestNonExistentVesselCausesWarning()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.containers = new List<CMMMessageContainer>();
			adapter.voyageNumber = "DUMMY";
			adapter.lloydsNumber = "DUMMY";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull("Message contains warnings", email);
			AssertContains("Could not find a vessel in the database with the Lloyds number 'DUMMY'", email.Body);
		}

		public void TestNonExistentVoyageCausesWarning()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.containers = new List<CMMMessageContainer>();
			adapter.voyageNumber = "DUMMY";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull("Message contains warnings", email);
			AssertContains("This vessel-voyage combination could not be found or was matched to multiple voyages. The movements will not be auto attached to a Sailing Schedule.", email.Body);
		}

		public void TestMultipleMatchingVoyageVesselCausesWarning()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.containers = new List<CMMMessageContainer>();
			var anotherVoyage = Factory.NewWithValidTestData<JobVoyage>();
			anotherVoyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			anotherVoyage.JV_VoyageFlight = adapter.voyageNumber;
			anotherVoyage.JV_RV_NKVessel = adapter.Vessel.RV_FK;
			anotherVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			anotherVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "GBLON";
			anotherVoyage.GenerateSailings();
			Factory.Save();
			adapter.containers = new List<CMMMessageContainer>();
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull("As there are multiple matching voyages so a warning email should have been generated", email);
			AssertContains("This vessel-voyage combination could not be found or was matched to multiple voyages. The movements will not be auto attached to a Sailing Schedule.", email.Body);
		}

		public void TestMatchingVesselVoyage()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.containers = new List<CMMMessageContainer>(); // Reset container list for this test
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			AssertEquals("No Warnings to generate", null, emailBuilder.ToWarningEmail());
		}

		public void TestNonExistentContainerCausesWarnings()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			// Reset Container Num so it doesn't match
			Container.R6_ContainerNum = "MWHE0000009";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("This container was not found in the container manager module and therefore ignored.", email.Body);
		}

		public void TestMismatchedContainerTypeCausesWarnings()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.Containers[0].ISOType = "42G0";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("The ISO type recorded against this container ('22G0') is different to the ISO type in the message ('42G0').", email.Body);
		}

		public void TestContainerWithoutContainerNumberCannotBeProcessed()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.Containers[0].ContainerNumber = "";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull("notification email was created", email);
			AssertContains("Missing container number", email.Body);
			AssertContains("Container does not contain container number and therefore cannot be processed.", email.Body);
		}

		public void TestNonExistentContainerCreatesNewContainerAndType()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var adapter = GetProcessingAdapterWithDefaults();
			// Reset Container Num so it doesn't match
			Container.R6_ContainerNum = "MWHE0000009";
			adapter.Containers[0].ContainerNumber = "MWHE9999990";
			adapter.Containers[0].ISOType = "99MW";
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "99MW"));
			AssertNull("Precondition - Container Type Does not exist", refContainer);
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("This container was not found in the container manager module and so was added.", email.Body);
			AssertContains("No container type found with ISO code '99MW', adding new container type '99MW'", email.Body);
			refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "99MW"));
			AssertNotNull("Container Type has been created", refContainer);
		}

		public void TestCreateNewMovement()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			AssertEquals("Precondition - No Container Movements", 0, Container.Movements.Count);
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());

			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("Movement message", "\"Load\" container movement added", email.Body);
			AssertEquals("Movement was created", 1, Container.Movements.Count);
			var movement = Container.Movements[0];
			AssertEquals("Movement.JobVoyage", Voyage.PK, movement.E9_JV);
			AssertEquals("Movement.Depot", Sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("Movement.RefContainerStock", Container.PK, movement.E9_R6);
			AssertEquals("Movement.MovementType", ContainerMovementTypes.Codes.Load, movement.E9_MovementType);
			AssertEquals("Movement.TransportMode", "ROA", movement.TransportMode);
		}

		public void TestExistingDuplicateMovementsAreNotReplaced()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			var originalMovement = Container.Movements.AddNew();
			originalMovement.E9_JV = Voyage.PK;
			originalMovement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			originalMovement.E9_OA_Depot = Sender.MainAddress.PK;
			originalMovement.E9_MovementDate = new ZDateTime(2012, 11, 10);
			// Force identical dates for the messages
			adapter.Containers[0].PositioningDateTime = originalMovement.E9_MovementDate.ToDateTime();
			AssertEquals("Precondition - Existing Container Movement", 1, Container.Movements.Count);
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("Movement message", "This \"Load\" movement (2012-11-10 00:00) appears to be a duplicate of an already logged movement, discarding.", email.Body);
			AssertEquals("No Additional Movement was created", 1, Container.Movements.Count);
			AssertEquals("Original Movement is still attached", originalMovement.PK, Container.Movements[0].PK);
		}

		public void TestSyncShipmentHandlesMatchingShipment()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var processor = new ContainerMovementMessageProcessor(adapter);
			adapter.updateBookings = false;
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("With registry off, bookings are not updated", "Found booking 'ABCDEF54321' with the booking reference 'BookRef', but will not update it.", email.Body);
			adapter.updateBookings = true;
			emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			email = emailBuilder.ToAckEmail();
			AssertNotNull(email);
			AssertContains("With registry on, bookings are updated", "Found booking 'ABCDEF54321' with the booking reference 'BookRef'.", email.Body);
		}

		public void TestSyncShipmentHandlesNoMatchingShipments()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			// Reset shipment values so it doesn't match
			Shipment.JS_CFSReference = "FOO";
			Shipment.JS_BookingReference = "BAR";
			Shipment.JS_HouseBill = "BAZ";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("Unmatched Shipment", "No shipments were found with the bill of lading number 'BillOfLading' or booking reference 'BookRef'.", email.Body);
		}

		public void TestSyncShipmentHandlesMultipleMatchingShipments()
		{
			var shipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			shipment2.JS_HouseBill = "BillOfLading";
			shipment2.JS_UniqueConsignRef = "ZYXWV67890";
			shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			var adapter = GetProcessingAdapterWithDefaults();
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("Multiple Matching Shipments Error", "More than 1 shipment has the bill of lading number 'BillOfLading' or booking reference 'BookRef'.", email.Body);
		}

		public void TestContainerSealNumberUpdates()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var shipmentContainer = Shipment.RealContainers.AddNew();
			shipmentContainer.JC_SealNum = "SEALNUM3";
			shipmentContainer.JC_Additional2SealNum = "SEALNUM4";
			shipmentContainer.JC_JX = Sailing.PK;
			shipmentContainer.JC_ContainerNum = Container.R6_ContainerNum;
			shipmentContainer.JC_GrossWeight = 800m;
			adapter.Containers[0].SealNumbers = new List<string> { "SEALNUM1", "SEALNUM2", "SEALNUM3" };
			adapter.Containers[0].GrossWeightKG = 1000m;
			adapter.updateBookings = true;
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("Updating container weight from 800.000 to 1000.000.", email.Body);
			AssertContains("Added the seal number 'SEALNUM1'", email.Body);
			AssertContains("Unable to add the seal number 'SEALNUM2' as this container already has 3 seal numbers entered.", email.Body);
			AssertContains("This container is recorded as having the seal 'SEALNUM4', but that seal was not mentioned in the message.", email.Body);
		}

		public void TestContainerSealNumberUpdates_LengthExceedsMaximumLimit()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var shipmentContainer = Shipment.RealContainers.AddNew();
			shipmentContainer.JC_JX = Sailing.PK;
			shipmentContainer.JC_ContainerNum = Container.R6_ContainerNum;
			shipmentContainer.JC_GrossWeight = 800m;
			adapter.Containers[0].SealNumbers = ["SEALNUMSEALNUMSEALNUM1", "SEALNUMSEALNUMSEALNUM2", "SEALNUMSEALNUMSEALNUM3"];
			adapter.Containers[0].GrossWeightKG = 1000m;
			adapter.updateBookings = true;
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			AssertNotNull(email);
			AssertContains("The maximum length of 'Seal Number' has been exceeded, so it will not be updated. The maximum length of this property is 20 characters, but 22 were entered. New value: SEALNUMSEALNUMSEALNUM1", email.Body);
			AssertContains("The maximum length of 'Second Seal Number' has been exceeded, so it will not be updated. The maximum length of this property is 20 characters, but 22 were entered. New value: SEALNUMSEALNUMSEALNUM2", email.Body);
			AssertContains("The maximum length of 'Third Seal Number' has been exceeded, so it will not be updated. The maximum length of this property is 20 characters, but 22 were entered. New value: SEALNUMSEALNUMSEALNUM3", email.Body);
		}

		public void TestContainerTypeUpdates()
		{
			var container40gp = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "42G0"));
			var container20gp = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "22G0"));
			var adapter = GetProcessingAdapterWithDefaults();
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var shipmentContainer = Shipment.RealContainers.AddNew();
			shipmentContainer.JC_JX = Sailing.PK;
			shipmentContainer.JC_ContainerNum = Container.R6_ContainerNum;
			shipmentContainer.JC_RC = container40gp.PK;
			Container.R6_RC = container20gp.PK;
			adapter.updateBookings = true;
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToAckEmail();
			AssertNotNull(email);
			AssertContains("Updating container type from '40GP' to '20GP'", email.Body);
			AssertEquals("Container Type has updated", Container.R6_RC, shipmentContainer.JC_RC);
		}

		public void TestContainerWeightUpdates()
		{
			var adapter = GetProcessingAdapterWithDefaults();
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var shipmentContainer = Shipment.RealContainers.AddNew();
			shipmentContainer.JC_JX = Sailing.PK;
			shipmentContainer.JC_ContainerNum = Container.R6_ContainerNum;
			shipmentContainer.JC_GrossWeight = 800m;
			adapter.Containers[0].GrossWeightKG = 1000m;
			adapter.updateBookings = true;
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToAckEmail();
			AssertNotNull(email);
			AssertContains("Updating container weight from 800.000 to 1000.000.", email.Body);
			AssertEquals("Container Weight has updated", 1000m, (decimal)shipmentContainer.JC_GrossWeight);
		}

		public void TestProcessMessage_ContainerHasTooLongNumber_DoNotCreateContainer()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var tooLongContainerNumber = new string('s', RefContainerStockSchema.R6_ContainerNum.MaxLength + 5);
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.Containers[0].ContainerNumber = tooLongContainerNumber;
			adapter.Containers[0].ISOType = "55SV";
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			var createdContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "55SV"));
			var createdContainer = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, tooLongContainerNumber));
			var expectedWarningMessage = string.Format("The container number '{0}' was too long ({1} characters when {2} characters are allowed) and therefore container was not created", tooLongContainerNumber, tooLongContainerNumber.Length, RefContainerStockSchema.R6_ContainerNum.MaxLength);
			AssertNotNull(email);
			AssertNull(createdContainer);
			AssertNull(createdContainerType);
			AssertContains(expectedWarningMessage, email.Body);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public void TestProcessMessage_ContainerHasTooLongISOType_DoNotCreateContainer()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var tooLongContainerISOType = new string('s', RefContainerSchema.RC_ISOType.MaxLength + 5);
			var adapter = GetProcessingAdapterWithDefaults();
			adapter.Containers[0].ContainerNumber = "McLaren";
			adapter.Containers[0].ISOType = tooLongContainerISOType;
			var processor = new ContainerMovementMessageProcessor(adapter);
			var emailBuilder = processor.ProcessMessage(new CMMEmailGenerator());
			var email = emailBuilder.ToWarningEmail();
			var createdContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, tooLongContainerISOType));
			var createdContainer = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, "McLaren"));
			var expectedWarningMessage = $"The container ISO type '{tooLongContainerISOType}' was too long ({tooLongContainerISOType.Length} characters when {RefContainerSchema.RC_ISOType.MaxLength} characters are allowed) and therefore container was not created";
			AssertNotNull(email);
			AssertNull(createdContainer);
			AssertNull(createdContainerType);
			AssertContains(expectedWarningMessage, email.Body);
		}
	}
}
