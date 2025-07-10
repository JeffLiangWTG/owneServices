using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbBookingConsolidationDataContextManager))]
	sealed class DtbBookingConsolidationDataContextManagerTest : ShipmentDataContextManagerTestCase<DtbBookingConsolidationDataContextManager, DtbBookingConsolidation>
	{
		public void TestReadFromTestFile_TransportBooking_UnlockBillingWithCharges()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				AssertNull("Precondition.", Factory.BOFactory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000098")));
				var chargeCode = Factory.BOFactory.NewWithValidTestData(ObjectFactory.GetType<IAccChargeCode>());
				chargeCode[AccChargeCodeSchema.Constants.AC_Code] = "TFRT";
				chargeCode[AccChargeCodeSchema.Constants.AC_Desc] = "Road Freight";
				chargeCode[AccChargeCodeSchema.Constants.AC_ChargeType] = "MRG";
				var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
				var booking = testDataHelper.CreateBooking();
				booking.KM_JobID = "TB00000098";
				new JobHeader.Loader(Factory.BOFactory, booking).TryCreate();
				var unlockBillingTabWithCharges = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - UnlockBillingTabWithCharges.xml");
				var message = GetQueuedUniversalShipmentMessage(unlockBillingTabWithCharges);
				var interchange = Factory.BOFactory.New<IEDIInterchange>();
				message.EM_EI = interchange.PK;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_From = "SMARTFREIGHT_EAD";
				interchange.EI_To = "HELLOITSME";
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				Factory.SaveForTesting();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);
				manager.Process(message);

				var tb = new BusinessObjectFactory().LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000098"));
				Assert("Booking should be marked as agent booking.", tb.KM_IsAgentBooking);

				var job = tb.Job as Job;
				AssertNotNull("Must have job header.", job);
				AssertGreaterThan("Must import with charges.", job.Charges.Count, 0);
			}
		}

		public void TestReadFromTestFile_TransportBooking_ShipmentWith2Consols()
		{
			var shipmentWithTwoConsols = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Shipment w 2 Consols.xml");
			var message = GetQueuedUniversalShipmentMessage(shipmentWithTwoConsols);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(1, consolidation.PackageJob.Packages.Count(p => p.IsContainer));
		}

		public void TestReadFromTestFile_TransportBooking_Loose()
		{
			var containerModeLoose = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Loose.xml");
			var message = GetQueuedUniversalShipmentMessage(containerModeLoose);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 4 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 4 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(2, consolidation.PackageJob.Packages.Count);
		}

		public void TestReadFromTestFile_TransportBooking_DataTarget_TB()
		{
			// data target on subshipment is Transport Booking
			// TB 2 TB we export the subshipment as datasource of Transport Booking, with no target, therefore we should handle both
			var dataTargetTB = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DataTarget TB.xml");
			var message = GetQueuedUniversalShipmentMessage(dataTargetTB);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status); // exceeded length

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 9 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 18 x DtbBookingInstructionPkgDivot, 3 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Line 138: <Shipment>.<SubShipmentCollection>.<SubShipment>.<InstructionCollection>.<Instruction>.<Address>.<OrganizationCode> exceeded its maximum length of 12 characters. 15 characters were found.
Warning - Line 255: <Shipment>.<SubShipmentCollection>.<SubShipment>.<InstructionCollection>.<Instruction>.<Address>.<OrganizationCode> exceeded its maximum length of 12 characters. 17 characters were found.
Warning - Could not find Sending Party Organisation with eHub Client ID 'MITRE10'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'MITRE10'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'MITRE10'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Warning - Matching 'ClientRequestedBillingParty':- No match found for '[Org. Code: MITR01]'.
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageExporter':- No match found for '[Org. Code: MITRE 10 QLD DC; Company Name: MITRE 10 QLD DC; Address 1: 131 Beenleigh Rd; City: ACACIA RIDGE]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageImporter':- No match found for '[Org. Code: CHERMSIDE MITRE10; Company Name: CHERMSIDE MITRE10; Address 1: 616 Rode Road; City: CHERMSIDE]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 9 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 18 x DtbBookingInstructionPkgDivot, 3 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(9, consolidation.PackageJob.Packages.Count);
			AssertEquals(1, consolidation.Bookings.Count);

			var booking = consolidation.Bookings[0];
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(9, booking.FirstPickup.PackageDivots.Count);
			AssertEquals(9, booking.LastDelivery.PackageDivots.Count);
			AssertEquals(1, booking.FirstPickup.Confirmations.Count);
			AssertEquals("Service Level should be correctly set", "EXP", booking.KM_RS_NKServiceLevel);
			AssertEquals("Should have Delivery and Connote Confirmation.", 2, booking.LastDelivery.Confirmations.Count);
		}

		public void TestReadFromTestFile_TransportBooking_DataTarget_TB_DoubleLogged()
		{
			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
			var packingHelper = new PackingTestHelper(Factory.BOFactory);

			var consolidation = testDataHelper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var packingJob = testDataHelper.CreatePackageJob(consolidation);
			var container = packingHelper.CreatePackage(packingJob, "CON1", 1, Constants.PkgUnit.Container, containerType: "20GP");

			var booking2 = testDataHelper.CreateBooking(consolidation);
			booking2.KM_JobID = "TB00000002";

			var picInstruction = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			testDataHelper.CreatePackageDivot(picInstruction, container, 1);

			Factory.SaveForTesting();

			var noDoubleLogBooking = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - ShouldNotDoubleLogBooking.xml");
			var message = GetQueuedUniversalShipmentMessage(noDoubleLogBooking);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			var taskLog = serviceTaskLog.ToString();
			var logCount = Regex.Matches(taskLog, Regex.Escape("Updated Transport Booking TB00000002 from UniversalShipment.")).Count;
			AssertEquals("Should only log once", 1, logCount);

			var logNoteText = message.GetLogNoteText();
			var notesCount = Regex.Matches(logNoteText, Regex.Escape("Populating DtbBooking...")).Count;
			AssertEquals("Should only log notes once", 1, logCount);

			var consolidationNewFactory = new BusinessObjectFactory().Load<DtbBookingConsolidation>(consolidation.PK);
			var booking = consolidationNewFactory.Bookings.Single(b => b.KM_JobID == "TB00000002");
			var importEvents = booking.Logs.Find(x => x.SL_SE_NKEvent == "DIM");

			AssertEquals("Should only be one DIM event", 1, importEvents.Count());
		}

		public void TestReadFromTestFile_TransportBooking_DataTarget_TB_SomePackagesWithNoWeightsAndVolumes()
		{
			var noPacakgeWeightsAndVolumes = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DataTarget TB - No Package WeightsAndVolumes.xml");
			var message = GetQueuedUniversalShipmentMessage(noPacakgeWeightsAndVolumes);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Warning - Unknown Address Type [ConsigneeDocumentaryAddress] found. Job Document Address not imported.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Warning - Unknown Address Type [ConsigneePickupDeliveryAddress] found. Job Document Address not imported.
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Warning - Unknown Address Type [ConsignorDocumentaryAddress] found. Job Document Address not imported.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Warning - Unknown Address Type [ConsignorPickupDeliveryAddress] found. Job Document Address not imported.
Matching 'SendersLocalClient':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals("Must import all three packages into the package job.", 3, consolidation.PackageJob.Packages.Count);

			var booking = consolidation.Bookings.Single();
			AssertEquals("Precondition", 2, booking.Instructions.Count);
			AssertEquals("All three packages must be assigned to pick up instruction.", 3, booking.FirstPickup.PackageDivots.Count);
			AssertEquals("All three packages must be assigned to delivery instruction.", 3, booking.LastDelivery.PackageDivots.Count);
		}

		public void TestReadFromTestFile_TransportBooking_DataTarget_Subshipment_TB()
		{
			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
			var packingHelper = new PackingTestHelper(Factory.BOFactory);

			var consolidation = testDataHelper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var packingJob = testDataHelper.CreatePackageJob(consolidation);
			var container = packingHelper.CreatePackage(packingJob, "CON1", 1, Constants.PkgUnit.Container, containerType: "20GP");

			var booking1 = testDataHelper.CreateBooking(consolidation);
			booking1.KM_JobID = "TB00000001";
			var booking2 = testDataHelper.CreateBooking(consolidation);
			booking2.KM_JobID = "TB00000002";
			var booking3 = testDataHelper.CreateBooking(consolidation);
			booking3.KM_JobID = "TB00000003";

			var picInstruction = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			testDataHelper.CreatePackageDivot(picInstruction, container, 1);

			Factory.SaveForTesting();

			var dataTargetFlipped = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Data Target Flipped - TransportBooking.xml");
			var message = GetQueuedUniversalShipmentMessage(dataTargetFlipped);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 1 x DtbBooking.
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 1 x DtbBooking.
Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidationNewFactory = new BusinessObjectFactory().Load<DtbBookingConsolidation>(consolidation.PK);
			AssertNull("TB00000001 is not specified in UXML therefore must not be updated.", consolidationNewFactory.Bookings.Single(b => b.KM_JobID == "TB00000001").ServiceLevel);
			var booking = consolidationNewFactory.Bookings.Single(b => b.KM_JobID == "TB00000002");
			AssertEquals("Only the booking specified in UXML must be updated.", "STD", booking.ServiceLevel.RS_Code);
			AssertEquals(2, booking.Instructions.Count);
		}

		public void TestReadFromTestFile_TransportBooking_PickupCarrierOnRouting()
		{
			var pickupCarrierOnRouting = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - PickupCarrierOnRouting.xml");
			var message = GetQueuedUniversalShipmentMessage(pickupCarrierOnRouting);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = Factory.Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Imported Carrier", "CARRIER", booking.Address.E2_CompanyName);
			AssertEquals("Imported Carrier Transport Ref", "PRE111", booking.KM_TransportReference);
			AssertEquals("Imported Carrier Service Level", "STD", booking.KM_PL_NKCarrierServiceLevel);
		}

		public void TestReadFromTestFile_TransportBooking_DeliveryCarrierOnRouting()
		{
			var deliveryCarrierOnRouting = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DeliveryCarrierOnRouting.xml");
			var message = GetQueuedUniversalShipmentMessage(deliveryCarrierOnRouting);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = Factory.Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Imported Carrier", "TCARRIER", booking.Address.E2_CompanyName);
			AssertEquals("Imported Carrier Transport Ref", "PRE2", booking.KM_TransportReference);
			AssertEquals("Imported Carrier Service Level", "WHT", booking.KM_PL_NKCarrierServiceLevel);
		}

		public void TestReadFromTestFile_TransportBooking_PickupCarrierOnRoutingWithContainer()
		{
			var pickupCarrierOnRoutingWithContainer = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - PickupCarrierOnRoutingWithContainer.xml");
			var message = GetQueuedUniversalShipmentMessage(pickupCarrierOnRoutingWithContainer);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = Factory.Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Imported Carrier", "CARRIER", booking.Address.E2_CompanyName);
			AssertEquals("Imported Carrier Transport Ref", "PRE1111", booking.KM_TransportReference);
			AssertEquals("Imported Carrier Service Level", "STD", booking.KM_PL_NKCarrierServiceLevel);

			booking.KM_TransportReference = "SomethingElse";
			booking.KM_PL_NKCarrierServiceLevel = "ELS";
			booking.Address.Delete();
			Factory.SaveForTesting();

			var targetSameConsolidationXML = pickupCarrierOnRoutingWithContainer.Replace("<Type>TransportBookingConsolidation</Type>", "<Type>TransportBookingConsolidation</Type><Key>CM00000001</Key>");

			manager.Process(GetQueuedUniversalShipmentMessage(targetSameConsolidationXML));
			var bookingAgain = new BusinessObjectFactory().Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Imported Carrier", "CARRIER", bookingAgain.Address.E2_CompanyName);
			AssertEquals("Should have imported Carrier Transport Ref again", "PRE1111", bookingAgain.KM_TransportReference);
			AssertEquals("Should have Imported Carrier Service Level again", "STD", bookingAgain.KM_PL_NKCarrierServiceLevel);
		}

		public void TestReadFromTestFile_TransportBooking_Containerised_TB2TB_NoPackingLines()
		{
			var containerisedNoPackingLines = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Containerised - TB2TB - NoPackingLines.xml");
			var message = GetQueuedUniversalShipmentMessage(containerisedNoPackingLines);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 3 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Warning - Matching 'TransportCompanyDocumentaryAddress':- No match found for '[Org. Code: TITPLASYD; Company Name: TITAN PLAY (ALPHA) TRUCKING; Address Code: SETUP TO COMMUNICATE TB T; Address 1: SETUP TO COMMUNICATE TB TO TITAN PLAY UAT SYSTEMS; City: DO NOT CHANGE]'.
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: PATDARSYD; Company Name: PATRICK DARLING HARBOUR; Address Code: terminal; Address 1: 10 DARLING DRIVE; City: SYDNEY]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageImporter':- No match found for '[Org. Code: BEAONRSYD; Company Name: BEADS ON THE RUN; Address Code: MELBOURNE; Address 1: 99 TULLAMARINE ROAD; City: MELBOURNE]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CONPARSYD; Company Name: CONTAINER PARK; Address Code: 1770 BOTANY ROAD; Address 1: 1770 BOTANY ROAD; City: BOTANY]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 3 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(1, consolidation.PackageJob.Packages.Count);
		}

		public void TestReadFromTestFile_TransportBooking_TB2TB_withRouting_Source_Booking()
		{
			var routingSourceBooking = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - TB2TB with Routing - Source Booking.xml");
			var message = GetQueuedUniversalShipmentMessage(routingSourceBooking);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			var collection = consolidation.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consolidation.PK));
			AssertEquals(1, collection.Length);
		}

		public void TestReadFromTestFile_TransportBooking_TB2TB_withRouting_Source_Consolidation()
		{
			var routingSourceConsolidation = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - TB2TB with Routing - Source Consolidation.xml");
			var message = GetQueuedUniversalShipmentMessage(routingSourceConsolidation);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			var collection = consolidation.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consolidation.PK));
			AssertEquals(1, collection.Length);
		}

		public void TestReadFromTestFile_TransportBooking_Containerised_TB2TB_TPC()
		{
			var containerisedTPC = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Containerised - TB2TB - TPC.xml");
			var message = GetQueuedUniversalShipmentMessage(containerisedTPC);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			//AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBookingInstructionPkgDivot, 4 x DtbBookingConfirmation, 3 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEDATDAU'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Warning - Matching 'TransportCompanyDocumentaryAddress':- No match found for '[Org. Code: ARROWN; Company Name: ARRANGED OWN CARTAGE; Address Code: OFC: PLS NOTE FOR EXPORTS; Address 1: PLS NOTE FOR EXPORTS REFERENCE ONLY; City: 1]'.
Matching 'SendersLocalClient':- Matched to 'BAGNTH' address 'Pick Up Address' with a score of 140.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: PORBOTSYD; Company Name: PORT BOTANY CONTAINER TERMINAL; Address Code: PORT BOTANY; Address 1: PORT BOTANY; City: SYDNEY]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Matching 'LocalCartageImporter':- Matched to 'BAGNTH' address 'Pick Up Address' with a score of 140.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CONPARSYD; Company Name: CONTAINER PARK; Address Code: 22 BROWN ST; Address 1: 22 BROWN ST; City: BOTANY]'.
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBookingInstructionPkgDivot, 4 x DtbBookingConfirmation, 3 x DtbBookingInstruction, 1 x DtbBooking.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(1, consolidation.PackageJob.Packages.Count);
		}

		public void TestReadFromTestFile_TransportBooking_RequiredToDateComingFromShipmentCombined()
		{
			var containersWithRequiredToDateCombined = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - ContainersWithRequiredToDate Combined.xml");
			var message = GetQueuedUniversalShipmentMessage(containersWithRequiredToDateCombined);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 6 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: NZAKL
Attempting to get Schedule for the Transport Leg
Warning - Matching 'Carrier':- No match found for '[Org. Code: CARRIESYD; Company Name: CARRIER; Address Code: AUSYD; Address 1: AUSYD; City: AUSYD]'.
An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Carrier':- No match found for '[Org. Code: CARRIESYD; Company Name: CARRIER; Address Code: AUSYD; Address 1: AUSYD; City: AUSYD]'.
Transport Leg updated.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 6 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));

			consolidation.PackageJob.AssertPackageTree("Ensure no Total Package was added (as no weights or volumes are on the Job).", @"
- 1 CNT 2280.000 KG (C1)
  - 1 PLT
- 1 CNT 2280.000 KG (C2)
  - 1 PLT
- 1 CNT 2280.000 KG (C3)
  - 1 PLT
");

			AssertEquals(1, consolidation.Bookings.Count);
			var cnrInstruction = consolidation.Bookings[0].Instructions.First(i => i.OrganisationType == "CNR");
			var deliveryInstructions = cnrInstruction.Confirmations.Where(c => c.IsDelivery);
			AssertEquals(3, deliveryInstructions.Count());

			var requiredToDates = deliveryInstructions.Select(c => c.KK_RequiredTo);
			AssertContainsExactElementsInAnyOrder(new[] { new ZDateTime(2016, 1, 7, 14, 26, 0), new ZDateTime(2016, 1, 8, 14, 26, 0), new ZDateTime(2016, 1, 9, 14, 26, 0) }, requiredToDates);
		}

		public void TestReadFromTestFile_TransportBooking_RequiredToDateComingFromShipmentSeparateBookings()
		{
			var containersWithRequiredToDateSeperateBookings = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - ContainersWithRequiredToDate SeperateBookings.xml");
			var message = GetQueuedUniversalShipmentMessage(containersWithRequiredToDateSeperateBookings);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking from UniversalShipment.
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 6 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBooking, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: AUSYD Destination: NZAKL
Attempting to get Schedule for the Transport Leg
Warning - Matching 'Carrier':- No match found for '[Org. Code: CARRIESYD; Company Name: CARRIER; Address Code: AUSYD; Address 1: AUSYD; City: AUSYD]'.
An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Carrier':- No match found for '[Org. Code: CARRIESYD; Company Name: CARRIER; Address Code: AUSYD; Address 1: AUSYD; City: AUSYD]'.
Transport Leg updated.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 6 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 3 x DtbBooking, 1 x Transport.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			consolidation.PackageJob.AssertPackageTree("Ensure no Total Package was added (as no weights or volumes are on the Job).", @"
- 1 CNT 2280.000 KG (C1)
  - 1 PLT
- 1 CNT 2280.000 KG (C2)
  - 1 PLT
- 1 CNT 2280.000 KG (C3)
  - 1 PLT
");

			AssertEquals(3, consolidation.Bookings.Count);

			var booking1ReqToDate = consolidation.Bookings[0].Instructions.First(i => i.OrganisationType == "CNR").Confirmations.Single(c => c.IsDelivery).KK_RequiredTo;
			var booking2ReqToDate = consolidation.Bookings[1].Instructions.First(i => i.OrganisationType == "CNR").Confirmations.Single(c => c.IsDelivery).KK_RequiredTo;
			var booking3ReqToDate = consolidation.Bookings[2].Instructions.First(i => i.OrganisationType == "CNR").Confirmations.Single(c => c.IsDelivery).KK_RequiredTo;
			AssertEquals(false, booking1ReqToDate.IsEmpty);
			AssertEquals(false, booking2ReqToDate.IsEmpty);
			AssertEquals(false, booking3ReqToDate.IsEmpty);
			AssertContainsExactElementsInAnyOrder(new[] { new ZDateTime(2016, 1, 7, 14, 26, 0), new ZDateTime(2016, 1, 8, 14, 26, 0), new ZDateTime(2016, 1, 9, 14, 26, 0) }, new[] { booking1ReqToDate, booking2ReqToDate, booking3ReqToDate });
		}

		public void TestReadFromTestFile_TransportBooking_ImportingSameUxmlFileMultipleTimes()
		{
			// Arrange

			var importingSameUxmlFileMultipleTimes = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - ImportingSameUxmlFileMultipleTimes.xml");
			var message = GetQueuedUniversalShipmentMessage(importingSameUxmlFileMultipleTimes);
			var serviceTaskLog1 = new ServiceTaskLogForTesting();
			var serviceTaskLog2 = new ServiceTaskLogForTesting();

			var manager1 = new UniversalMessageProcessingManager(serviceTaskLog1);
			var manager2 = new UniversalMessageProcessingManager(serviceTaskLog2);

			var branchOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, message.Branch.OrganisationPK));
			var patternOverride = branchOrganisation.CreatePatternMatchOverrideForTest();
			patternOverride.OO_ForeignCode = "HYEUA1DAU";
			patternOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;

			var mappedOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "HAIFEN"));
			patternOverride.OO_LocalGuid = mappedOrganisation.PK;

			Factory.SaveForTesting();

			var consolidationQuery = new ZDBOnlyQuery(typeof(DtbBookingConsolidation));
			consolidationQuery.AddToFilter(JoinCondition.And, DtbBookingConsolidationSchema.KB_JobID, SQLComparisonOperator.StartsWith, "CM");

			var existingConsolidations = Factory.Load<DtbBookingConsolidation>(consolidationQuery);
			AssertEquals("Precondition: Expecting 0 booking consolidation before UXML import", 0, existingConsolidations.Length);

			// Act

			manager1.Process(message);
			serviceTaskLog1.ClearLogs();

			manager1.Process(message); // Load UXML using a same manager
			manager2.Process(message); // Load UXML using a different manager

			// Assert

			var consolidations = Factory.Load<DtbBookingConsolidation>(consolidationQuery);
			AssertEquals("Expecting 1 booking consolidation after importing UXML multiple times", 1, consolidations.Length);

			var consolidation = consolidations.Single();
			AssertEquals("Expecting booking consolidation with correct Job ID", "CM00000001", consolidation.KB_JobID);
			AssertEquals("Expecting 1 booking", 1, consolidation.Bookings.Count);
			AssertEquals("Expecting 3 booking instructions", 3, consolidation.Bookings.Single().Instructions.Count);

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			packageQuery.AddToFilter(JoinCondition.And, PkgPackageSchema.KP_KJ_ParentPackageJob, consolidation.PackageJob.PK);
			packageQuery.AddToFilter(JoinCondition.And, PkgPackageSchema.KP_KP_ParentPackage, null);

			var packages = Factory.Load<PkgPackage>(packageQuery);
			AssertEquals("Expecting 3 packages from booking instruction", 3, packages.Length);

			var packageIds = packages.Select(package => package.KP_PackageID.ToString()).ToArray();
			AssertEquals("Expecting package with ID CON14251", true, packageIds.Contains("CON14251"));
			AssertEquals("Expecting package with ID CON14252", true, packageIds.Contains("CON14252"));
			AssertEquals("Expecting package with ID CON14253", true, packageIds.Contains("CON14253"));

			foreach (var package in packages)
			{
				AssertEquals("Expecting correct Package Qty", 1, package.KP_PackageQty);
				AssertEquals("Expecting correct Pack Type", "CNT", package.KP_F3_NKPackType);
				AssertEquals("Expecting correct Weight", 2300m, package.KP_Weight);
				AssertEquals("Expecting correct Weight Unit", "KG", package.KP_WeightUQ);
				AssertEquals("Expecting correct Volume", 0m, package.KP_Volume);
				AssertEquals("Expecting correct Volume Unit", "M3", package.KP_VolumeUQ);

				var innerPackages = package.GetAllPackages();
				AssertEquals("Expecting 1 inner package for each package", 1, innerPackages.Count);

				var innerPackage = innerPackages.Single();
				AssertEquals("Expecting correct Pack Type", "PLT", innerPackage.KP_F3_NKPackType);
				AssertEquals("Expecting correct Weight", 20m, innerPackage.KP_Weight);
				AssertEquals("Expecting correct Weight Unit", "KG", innerPackage.KP_WeightUQ);
				AssertEquals("Expecting correct Volume", 20m, innerPackage.KP_Volume);
				AssertEquals("Expecting correct Volume Unit", "M3", innerPackage.KP_VolumeUQ);
			}

			foreach (var serviceTaskLog in new[] { serviceTaskLog1, serviceTaskLog2 })
			{
				AssertEquals(
					"Expecting correct task log message",
@"Updated Transport Booking TB00000001 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 1 x CusEntryNumber, 6 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 9 x DtbBookingInstructionPkgDivot, 4 x DtbBookingConfirmation, 3 x DtbBookingInstruction, 1 x DtbBooking.",
					serviceTaskLog.ToString());
			}
		}

		public void TestReadFromTestFile_TransportBooking_Customs_Container3Packs()
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var customsContainerThreePacks = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Customs - Container and 3 packs.xml");
			var message = GetQueuedUniversalShipmentMessage(customsContainerThreePacks);
			manager.Process(message);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));
			AssertEquals(1, consolidation.Bookings.Count);
			consolidation.PackageJob.AssertPackageTree("Job doesn't have weight or volume, so import as is (no Totals Package).", @"
- 1 CNT 2000.000 KG (TM100316)
  - 10 PKG
  - 20 PKG
  - 20 PLT
");
		}

		public void TestReadFromTestFile_TransportBooking_EstimatedDeliveryAndPackingDates_Export()
		{
			var deliveryAndPackingDatesExport = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DeliveryAndPackingDates - Export.xml");
			var message = GetQueuedUniversalShipmentMessage(deliveryAndPackingDatesExport);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: NZAKL Destination: AUMEL
Attempting to get Schedule for the Transport Leg
Warning - Matching 'Carrier':- No match found for '[Org. Code: TESCARMEL; Company Name: TEST CARRIER; Address Code: TEST CARRIER; Address 1: TEST CARRIER]'.
An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Carrier':- No match found for '[Org. Code: TESCARMEL; Company Name: TEST CARRIER; Address Code: TEST CARRIER; Address 1: TEST CARRIER]'.
Transport Leg updated.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));

			var instruction = consolidation.Bookings.Single().Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Multi);
			var pickupConfirmation = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			AssertEquals("Estimated date should be populated from consol's export details.", new ZDateTime("11-OCT-16 13:40"), pickupConfirmation.KK_Estimated);

			var deliveryInstrcutionToCTO = consolidation.Bookings.Single().Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery && i.IsCTO);
			var deliveryConfirmationToCTO = deliveryInstrcutionToCTO.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);
			AssertEquals("Slot date should be populated from consol's export details.", new ZDateTime("22-OCT-16 08:38"), deliveryConfirmationToCTO.KK_SlotDateTime);
			AssertEquals("Slot reference should be populated from consol's export details.", "DepartureRef", deliveryConfirmationToCTO.KK_SlotReference);

			AssertEquals("Reference number should be populated from consol's export details.", "RN2", deliveryConfirmationToCTO.KK_ReferenceNum);
		}

		public void TestReadFromTestFile_TransportBooking_EstimatedDeliveryAndPackingDates_Import()
		{
			var deliveryAndPackingDatesImport = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - DeliveryAndPackingDates - Import.xml");
			var message = GetQueuedUniversalShipmentMessage(deliveryAndPackingDatesImport);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"
Added Transport Booking from UniversalShipment.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
No matching DtbBooking found, creating new DtbBooking.
Populating DtbBooking...
Added Transport Booking from UniversalShipment.
No matching Transport found, creating new Transport.
Populating Transport...
Transport Leg: Origin: NZAKL Destination: AUMEL
Attempting to get Schedule for the Transport Leg
Warning - Matching 'Carrier':- No match found for '[Org. Code: TESCARMEL; Company Name: TEST CARRIER; Address Code: TEST CARRIER; Address 1: TEST CARRIER]'.
An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Carrier':- No match found for '[Org. Code: TESCARMEL; Company Name: TEST CARRIER; Address Code: TEST CARRIER; Address 1: TEST CARRIER]'.
Transport Leg updated.
Added Transport Booking Consolidation  from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 2 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBooking, 1 x Transport.
".Trim(), logNoteText);

			var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobID, "CM00000001"));

			var instruction = consolidation.Bookings.Single().Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Multi);
			var deliveryConfirmation = instruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery);
			AssertEquals("Estimated date should be populated from consol's import details.", new ZDateTime("13-OCT-16 13:41"), deliveryConfirmation.KK_Estimated);

			var pickupInstrcutionFromCTO = consolidation.Bookings.Single().Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp && i.IsCTO);
			var pickupConfirmationFromCTO = pickupInstrcutionFromCTO.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp);
			AssertEquals("Slot date should be populated from consol's import details.", new ZDateTime("27-OCT-16 15:33"), pickupConfirmationFromCTO.KK_SlotDateTime);
			AssertEquals("Slot reference should be populated from consol's export details.", "ArrivalRef", pickupConfirmationFromCTO.KK_SlotReference);
			AssertEquals("Reference should be populated from consol's import details.", "DORelease2", pickupConfirmationFromCTO.KK_ReferenceNum);
		}

		public void TestReadFromTestFile_TransportBooking_Containerised_CollectionContentIsComplete()
		{
			var collectionContentIsComplete = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Containerised - CollectionContentIsComplete -Import.xml");
			AssertReadFromTestFile_TransportBooking_Containerised_CollectionContentCore(collectionContentIsComplete, CollectionContent.Complete);
		}

		public void TestReadFromTestFile_TransportBooking_Containerised_CollectionContentIsPartial()
		{
			var collectionContentIsPartial = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Containerised - CollectionContentIsPartial -Import.xml");
			AssertReadFromTestFile_TransportBooking_Containerised_CollectionContentCore(collectionContentIsPartial, CollectionContent.Partial);
		}

		public void TestReadFromTestFile_TransportBooking_Containerised_CollectionContentIsEmpty()
		{
			var collectionContentIsEmpty = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Containerised - CollectionContentIsEmpty -Import.xml");
			AssertReadFromTestFile_TransportBooking_Containerised_CollectionContentCore(collectionContentIsEmpty);
		}

		public void TestReadFromTestFile_TransportBooking_LoosePackage_CollectionContentIsComplete()
		{
			var collectionContentIsComplete = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - LoosePackage - CollectionContentIsComplete - Import.xml");
			AssertReadFromTestFile_TransportBooking_LoosePackage_CollectionContentCore(collectionContentIsComplete, CollectionContent.Complete);
		}

		public void TestReadFromTestFile_TransportBooking_LoosePackage_CollectionContentIsPartial()
		{
			var collectionContentIsPartial = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - LoosePackage - CollectionContentIsPartial - Import.xml");
			AssertReadFromTestFile_TransportBooking_LoosePackage_CollectionContentCore(collectionContentIsPartial, CollectionContent.Partial);
		}

		public void TestReadFromTestFile_TransportBooking_LoosePackage_CollectionContentIsEmpty()
		{
			var collectionContentIsEmpty = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - LoosePackage - CollectionContentIsEmpty - Import.xml");
			AssertReadFromTestFile_TransportBooking_LoosePackage_CollectionContentCore(collectionContentIsEmpty);
		}

		public void TestUnmatchedOrgsFunctionality()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, null);
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{ OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.TransportCompanyDocumentaryAddress) });

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var message = GetQueuedUniversalShipmentMessage(shipment);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var booking = (DtbBooking)results.Single(r => r.DataContextType == DataContextType.TransportBooking).GetBizOForTesting(shipment, Factory.BOFactory);

			AssertEquals("Transport Company should be overriden with an unmatched org.", "UNMATCHED ORGANISATION", booking.Address.E2_CompanyName);

			var unmatchedOrganisationNotes = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Unmatched organisation note should have been created", 1, unmatchedOrganisationNotes.Length);

			AssertMultilineASCIIEquals("Note should be added to the Declaration", @"Organisation Type: Carrier
Owner Code: 
EDI Code: CRAHOLSYD
Organisation Name: CRACKERJACK HOLDINGS
Address Line 1: 1804 Fudrucker Way
Address Line 2: 
City: BOTANY
Post Code: 2035
State or Province: NSW
Country: AU
Doc Address Type:", unmatchedOrganisationNotes[0].ST_NoteText.TrimEnd());
		}

		[TestDate(2010, 1, 1)]
		public void TestFieldsCanBeUpdatedFromUniversalEvent()
		{
			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);

			var booking = testDataHelper.CreateBooking();
			var pickUpInstruction = testDataHelper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);

			Factory.SaveForTesting();

			AssertEquals("Precondition - no confirmations exist on pickUpInstruction", 0, pickUpInstruction.Confirmations.Count);

			string eventXmlText = DtbBookingConfirmationEventParentFinderTest.GetTestEventXmlText(booking.KM_JobID, 1, "Jack The Ripper", "SLICE101");
			var message = GetQueuedUniversalEventMessage(eventXmlText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("Precondition: If no confirmation that much instruction type exist, then new one should be created.", 1, pickUpInstruction.Confirmations.Count);
				var confirmation = pickUpInstruction.Confirmations[0];

				var logs = confirmation.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("logs.Length", 1, logs.Length);
				var log = logs[0];
				AssertEquals("log.SL_EventTime", new ZDateTime(2010, 7, 10, 18, 0, 0), log.SL_EventTime);

				AssertEquals("confirmation.KK_ReceivedBy", "Jack The Ripper", confirmation.KK_ReceivedBy);
				AssertEquals("confirmation.KK_ReferenceNum", "SLICE101", confirmation.KK_ReferenceNum);
			});
		}

		public void TestContextInformationIsAllThere()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			booking.KM_TransportReference = "BB123987123";
			booking.KM_JobID = "TB01234567";

			var manager = booking.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
TransportBookingJobID - TB01234567
TransportReference - BB123987123
".Trim(), eventContextValues);
		}

		public void TestSetupRelationshipsWithImportedJobs()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			parent.Z0_Description = "DUM456";
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			PublishUniversalXmlResult events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[] { RecipientRoleType.TPC }, parent);
				factory.Save();
			}

			AssertEquals(3, events.Length);

			var tbEvent = events[0];
			var consolidationEvent = events[1];
			var linkedJobEvent = events[2];
			AssertEquals(Events.DataImportCode, tbEvent.EventType);
			AssertEquals(Events.DataImportCode, consolidationEvent.EventType);
			AssertEquals(Events.JobsLinkedCode, linkedJobEvent.EventType);

			var dataSource1 = linkedJobEvent.GetMatchingDataSource(DataContextType.DummyBusinessObject);
			AssertNotNull(dataSource1);
			AssertEquals("DUM456", dataSource1.Key);

			var dataSource2 = linkedJobEvent.GetMatchingDataSource(DataContextType.TransportBookingConsolidation);
			AssertNotNull(dataSource2);
			AssertEquals("CM00000001", dataSource2.Key);
		}

		public void TestRecipientRoleTargettedToThisModuleExternalWhenTestCbaIdNotSet()
		{
			CombineAssertions(() =>
			{
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, null, internalTransmission: false, setTestCbaId: false, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBookingConsolidation, internalTransmission: false, setTestCbaId: false, true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBooking, internalTransmission: false, setTestCbaId: false, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, null, internalTransmission: false, setTestCbaId: false, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBookingConsolidation, internalTransmission: false, setTestCbaId: false, false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBooking, internalTransmission: false, setTestCbaId: false, expectedTargetted: false);
			});
		}

		public void TestRecipientRoleTargettedToThisModuleInternalWhenTestCbaIdNotSet()
		{
			CombineAssertions(() =>
			{
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, null, internalTransmission: true, setTestCbaId: false, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBookingConsolidation, internalTransmission: true, setTestCbaId: false, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBooking, internalTransmission: true, setTestCbaId: false, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, null, internalTransmission: true, setTestCbaId: false, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBookingConsolidation, internalTransmission: true, setTestCbaId: false, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBooking, internalTransmission: true, setTestCbaId: false, expectedTargetted: false);
			});
		}

		public void TestRecipientRoleTargettedToThisModuleExternalWhenTestCbaIdIsSet()
		{
			CombineAssertions(() =>
			{
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, null, internalTransmission: false, setTestCbaId: true, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBookingConsolidation, internalTransmission: false, setTestCbaId: true, true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBooking, internalTransmission: false, setTestCbaId: true, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, null, internalTransmission: false, setTestCbaId: true, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBookingConsolidation, internalTransmission: false, setTestCbaId: true, true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBooking, internalTransmission: false, setTestCbaId: true, expectedTargetted: true);
			});
		}

		public void TestRecipientRoleTargettedToThisModuleInternalWhenTestCbaIdIsSet()
		{
			CombineAssertions(() =>
			{
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, null, internalTransmission: true, setTestCbaId: true, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBookingConsolidation, internalTransmission: true, setTestCbaId: true, false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.TPC, DataContextType.TransportBooking, internalTransmission: true, setTestCbaId: true, expectedTargetted: false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, null, internalTransmission: true, setTestCbaId: true, expectedTargetted: true);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBookingConsolidation, internalTransmission: true, setTestCbaId: true, false);
				RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType.BKP, DataContextType.TransportBooking, internalTransmission: true, setTestCbaId: true, expectedTargetted: false);
			});
		}

		public void TestShipmentType()
		{
			var singleJobConsolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair { Code = TransportConsolidationJobTypes.Codes.Booking, Description = TransportConsolidationJobTypes.Descriptions.Booking },
			};
			singleJobConsolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) });

			var multiJobConsolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair { Code = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, Description = TransportConsolidationJobTypes.Descriptions.BookingTransportConsolidation },
			};
			multiJobConsolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) });

			// no shipment type - should fall back to single job consolidation if there is no shipment type specified
			var legacySingleJobConsolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContextManager = new DtbBookingConsolidationDataContextManagerForTest();

			var reader1 = dataContextManager.GetShipmentDataObjectReaderForTest(singleJobConsolidationDataObject);
			var consolidation1 = reader1.ReadIntoBusinessObject();
			AssertEquals("Should be single job consolidation", false, consolidation1.IsMultiBooking);

			var reader2 = dataContextManager.GetShipmentDataObjectReaderForTest(multiJobConsolidationDataObject);
			var consolidation2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Should be multi job consolidation", true, consolidation2.IsMultiBooking);

			var reader3 = dataContextManager.GetShipmentDataObjectReaderForTest(legacySingleJobConsolidationDataObject);
			var consolidation3 = reader3.ReadIntoBusinessObject();
			AssertEquals("Should fall back to single job consolidation", false, consolidation3.IsMultiBooking);
		}

		public void TestReadFromTestFile_TransportBooking_Instruction_CollectionContentIsComplete()
		{
			var collectionContentIsComplete = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Instruction - CollectionContentIsComplete.xml");
			AssertReadFromTestFile_TransportBooking_Instruction_CollectionContentCore(collectionContentIsComplete, CollectionContent.Complete);
		}

		public void TestReadFromTestFile_TransportBooking_Instruction_CollectionContentIsPartial()
		{
			var collectionContentIsPartial = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Instruction - CollectionContentIsPartial.xml");
			AssertReadFromTestFile_TransportBooking_Instruction_CollectionContentCore(collectionContentIsPartial, CollectionContent.Partial);
		}

		public void TestReadFromTestFile_TransportBooking_Instruction_CollectionContentIsEmpty()
		{
			var collectionContentIsEmpty = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Instruction - CollectionContentIsEmpty.xml");
			AssertReadFromTestFile_TransportBooking_Instruction_CollectionContentCore(collectionContentIsEmpty);
		}

		public void TestCheckUpdateTransportBooking_SenderIsTransportCompany_BookingOverrideTrue()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var transportCompany = helper.CreateOrganisation("ABC");
			var consolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(consolidation);

			consolidation.KB_IsOverridden = true;
			booking.Address.OrganisationPK = transportCompany.PK;
			Factory.SaveForTesting();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_JobID);
			consolidationDataObject.DataContext.DataProviderForCodeMapping = "ABC";

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m };
			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, ReferenceNumber = "DEF", Weight = 200m };
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData1, packageData2 }));

			var message = GetQueuedUniversalShipmentMessage(consolidationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Weight should be 350m", 350m, booking.PackageJob.Weight);
		}

		public void TestCheckUpdateTransportBooking_SenderIsCarrierBookingAgent_BookingOverrideTrue()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var carrierBookingAgent = helper.CreateOrganisation("ABC");
			var consolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(consolidation);

			consolidation.KB_IsOverridden = true;
			booking.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent.PK;
			Factory.SaveForTesting();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_JobID);
			consolidationDataObject.DataContext.DataProviderForCodeMapping = "ABC";

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m };
			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, ReferenceNumber = "DEF", Weight = 400m };
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData1, packageData2 }));

			var message = GetQueuedUniversalShipmentMessage(consolidationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Weight should be 550m", 550m, booking.PackageJob.Weight);
		}

		public void TestCheckUpdateTransportBooking_SenderIsNotCarrierBookingAgentOrTransportCompany_BookingOverrideTrue()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var carrierBookingAgent = helper.CreateOrganisation("ABC");
			var transportCompany = helper.CreateOrganisation("DEF");
			var sender = helper.CreateOrganisation("XYZ");
			var consolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(consolidation);

			consolidation.KB_IsOverridden = true;
			booking.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent.PK;
			booking.Address.OrganisationPK = transportCompany.PK;
			Factory.SaveForTesting();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_JobID);
			consolidationDataObject.DataContext.DataProviderForCodeMapping = "XYZ";

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m };
			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, ReferenceNumber = "DEF", Weight = 400m };
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData1, packageData2 }));

			var message = GetQueuedUniversalShipmentMessage(consolidationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Weight should be 0m", 0m, booking.PackageJob.Weight);
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			base.MakeShipmentUsableForThisRole(recipientRoleType, shipmentWithRecipientRole);

			if (recipientRoleType == RecipientRoleType.TPC)
			{
				shipmentWithRecipientRole.DataContext = DataContextFactory.New();
				shipmentWithRecipientRole.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = recipientRoleType } } });
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return "<UniversalShipment></UniversalShipment>"; }
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return new RecipientRoleType[] { RecipientRoleType.TPC, RecipientRoleType.CTG }; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithDtbBooking.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		void AssertReadFromTestFile_TransportBooking_Containerised_CollectionContentCore(string universalMessageText, CollectionContent? content = null)
		{
			// UXML contains data to ONLY update Transport Booking 2 and the ContainerCollection contains Container1, Container2, Container23.
			// If the UXML ContainerCollection has CollectionContent of Partial or Complete, the result will be the same even when targetting a Transport Booking.
			// ie. We can not delete a Container that is assigned to another booking.

			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
			var packingHelper = new PackingTestHelper(Factory.BOFactory);

			var consolidation = testDataHelper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var packingJob = testDataHelper.CreatePackageJob(consolidation);
			var container1 = packingHelper.CreatePackage(packingJob, "CON1", 1, Constants.PkgUnit.Container, containerType: "20GP");
			var container2 = packingHelper.CreatePackage(packingJob, "CON2", 1, Constants.PkgUnit.Container, containerType: "40GP");
			var container22 = packingHelper.CreatePackage(packingJob, "CON22", 1, Constants.PkgUnit.Container, containerType: "20FR");
			var container3 = packingHelper.CreatePackage(packingJob, "CON3", 1, Constants.PkgUnit.Container, containerType: "40GP");

			var booking1 = testDataHelper.CreateBooking(consolidation);
			booking1.KM_JobID = "TB00000001";
			var booking2 = testDataHelper.CreateBooking(consolidation);
			booking2.KM_JobID = "TB00000002";
			var booking3 = testDataHelper.CreateBooking(consolidation);
			booking3.KM_JobID = "TB00000003";

			var picInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction1, container1, 1);

			var picInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction2, container2, 1);
			testDataHelper.CreatePackageDivot(dlvInstruction2, container22, 1);

			var picInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction3, container3, 1);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(universalMessageText);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 3 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 1 x DtbBooking.".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
Successfully loaded matching PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
Successfully loaded matching PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching Container Type.
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingInstruction, 3 x DtbBookingInstructionPkgDivot, 2 x DtbBookingConfirmation, 1 x DtbBooking.".Trim(), logNoteText);

			var newFactory = new BusinessObjectFactory();
			var booking1_InNewFactory = newFactory.Load<DtbBooking>(booking1.PK);
			var booking2_InNewFactory = newFactory.Load<DtbBooking>(booking2.PK);
			var booking3_InNewFactory = newFactory.Load<DtbBooking>(booking3.PK);
			AssertEquals("Should have updated the existing Package Job.", packingJob.PK, booking1_InNewFactory.PackageJob.PK);
			AssertEquals("Booking1 should still have its existing Container (Container1).", container1.PK, booking1_InNewFactory.Containers.Single().Package.PK);
			AssertEquals("Booking3 should still have its existing Container (Container3).", container3.PK, booking3_InNewFactory.Containers.Single().Package.PK);

			var containers = booking2_InNewFactory.Containers;
			AssertEquals(3, containers.Count());

			var container2_AfterImport = containers.Single(p => p.Package.KP_PackageID == "CON2").Package;
			AssertEquals("Booking2 should still have its existing Container (Container2). It's in the UXML.", container2.PK, container2_AfterImport.PK);
			AssertContainer(container2_AfterImport, "CON2", "20GP");

			AssertEquals("CON22 is not in the UXML for TB2 (unmatched), should be removed.", false, containers.Any(p => p.Package.KP_PackageID == "CON22"));
			var container22_InNewFactory = newFactory.Load<PkgPackage>(container22.PK);
			if (content == CollectionContent.Complete)
			{
				AssertNull("Container22 is not used on any other Bookings, so should be deleted.", container22_InNewFactory);
			}
			else
			{
				AssertNotNull("Although Container22 is not used on any other Bookings, but CollectionContent is NOT Complete, so should not be deleted.", container22_InNewFactory);
				AssertEquals("Container22 should still be on the package job.", packingJob.PK, container22_InNewFactory.PackageJob.PK);
			}

			var newContainer23 = containers.Single(p => p.Package.KP_PackageID == "CON23").Package;
			AssertContainer(newContainer23, "CON23", "20GP");

			var container1_AfterImport = containers.Single(p => p.Package.KP_PackageID == "CON1").Package;
			AssertEquals("The existing Container1 (from another booking, same consolidation) should also be linked to TB2.", container1.PK, container1_AfterImport.PK);
			AssertContainer(container1_AfterImport, "CON1", "20GP");
		}

		void AssertContainer(PkgPackage container, ZString expectedContainerNumber, ZString expectedContainerType)
		{
			AssertEquals("Container Number", expectedContainerNumber, container.KP_PackageID);
			AssertEquals("Container Type", expectedContainerType, container.Container.ContainerType.RC_Code);
		}

		void AssertReadFromTestFile_TransportBooking_LoosePackage_CollectionContentCore(string universalMessageText, CollectionContent? content = null)
		{
			// UXML contains data to ONLY update Transport Booking 2 and the PackingLineCollection contains Package1, Package2, Package23.
			// If the UXML PackingLineCollection has CollectionContent of Partial or Complete, the result will be the same even when targetting a Transport Booking.
			// ie. We can not delete a Package that is assigned to another booking.

			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
			var packingHelper = new PackingTestHelper(Factory.BOFactory);
			var consolidation = testDataHelper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking1 = testDataHelper.CreateBooking(consolidation);
			booking1.KM_JobID = "TB00000001";
			var booking2 = testDataHelper.CreateBooking(consolidation);
			booking2.KM_JobID = "TB00000002";
			var booking3 = testDataHelper.CreateBooking(consolidation);
			booking3.KM_JobID = "TB00000003";

			var packingJob = testDataHelper.CreatePackageJob(consolidation);
			var package1 = (PkgPackage)packingHelper.CreatePackage(packingJob.PK, "P1", 1, Constants.PkgUnit.Package, 5m, Constants.Volume.CubicMetres, 5m, Constants.Weight.Kilograms);
			var package2 = (PkgPackage)packingHelper.CreatePackage(packingJob.PK, "P2", 1, Constants.PkgUnit.Box, 2m, Constants.Volume.Litre, 2m, Constants.Weight.Pounds);
			var package22 = (PkgPackage)packingHelper.CreatePackage(packingJob.PK, "P22", 1, Constants.PkgUnit.Carton, 2m, Constants.Volume.Litre, 2m, Constants.Weight.Pounds);
			var package3 = (PkgPackage)packingHelper.CreatePackage(packingJob.PK, "P3", 1, Constants.PkgUnit.Pallet, 3m, Constants.Volume.CubicCentimeters, 3m, Constants.Weight.Grams);

			var picInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction1, package1).KD_Quantity = 1;

			var picInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction2, package2).KD_Quantity = 1;
			testDataHelper.CreatePackageDivot(dlvInstruction2, package22).KD_Quantity = 1;

			var picInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			testDataHelper.CreatePackageDivot(dlvInstruction3, package3).KD_Quantity = 1;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(universalMessageText);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 3 x DtbBookingInstructionPkgDivot, 1 x DtbBooking.".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
Successfully loaded matching PkgPackage.
Populating PkgPackage...
Package with ID P2 has updated Weight: From 2.000 to 10.000, Weight Unit: From LB to G, Volume: From 2.000 to 10.000, Volume Unit: From L to CC.
Successfully loaded matching PkgPackage.
Populating PkgPackage...
Package with ID P1 did not update weight, volume or dimension.
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingInstructionPkgDivot found, creating new DtbBookingInstructionPkgDivot.
Populating DtbBookingInstructionPkgDivot...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 2 x DtbBookingConfirmation, 2 x DtbBookingInstruction, 3 x DtbBookingInstructionPkgDivot, 1 x DtbBooking.".Trim(), logNoteText);

			var newFactory = new BusinessObjectFactory();
			var newBooking1_InNewFactory = newFactory.Load<DtbBooking>(booking1.PK);
			var newBooking2_InNewFactory = newFactory.Load<DtbBooking>(booking2.PK);
			var newBooking3_InNewFactory = newFactory.Load<DtbBooking>(booking3.PK);
			AssertEquals("Should have updated the existing Package Job.", packingJob.PK, newBooking1_InNewFactory.PackageJob.PK);
			AssertEquals("Booking1 should still have its existing Container (Container1).", package1.PK, newBooking1_InNewFactory.AssignedPackages.Single().PK);
			AssertEquals("Booking3 should still have its existing Container (Container3).", package3.PK, newBooking3_InNewFactory.AssignedPackages.Single().PK);

			var packages = newBooking2_InNewFactory.AssignedPackages;
			AssertEquals(3, packages.Count);
			var package2_AfterImport = packages.Single(p => p.KP_PackageID == "P2");
			AssertEquals("Booking2 should still have its existing Container (Container2). It's in the UXML.", package2.PK, package2_AfterImport.PK);
			AssertPackage(package2_AfterImport, "P2", 1, Constants.PkgUnit.Pallet, 10m, Constants.Volume.CubicCentimeters, 10m, Constants.Weight.Grams);

			AssertEquals("P22 is unmatched, should be removed", false, packages.Any(p => p.KP_PackageID == "P22"));
			var package22_InNewFactory = newFactory.Load<PkgPackage>(package22.PK);
			if (content == CollectionContent.Complete)
			{
				AssertNull("Package22 is not used on any other Bookings, so should be deleted.", package22_InNewFactory);
			}
			else
			{
				AssertNotNull("Although Package22 is not used on any other Bookings, but CollectionContent is NOT Complete, so should not be deleted.", package22_InNewFactory);
				AssertEquals("Package22 should still be on the package job.", packingJob.PK, package22_InNewFactory.PackageJob.PK);
			}

			var newPackage23 = packages.Single(p => p.KP_PackageID == "P23");
			AssertPackage(newPackage23, "P23", 1, Constants.PkgUnit.Pallet, 10m, Constants.Volume.CubicCentimeters, 10m, Constants.Weight.Grams);

			var package1AfterImport = packages.Single(p => p.KP_PackageID == "P1");
			AssertEquals("The existing Package1 (from another booking, same consolidation) should also be linked to TB2.", package1.PK, package1AfterImport.PK);
		}

		void AssertPackage(PkgPackage package, ZString expectedID, ZInt expectedQty, ZString expectedPkgType, ZDecimal expectedVolume, ZString expectedVolumeUnit, ZDecimal expectedWeight, ZString expectedWeightUnit)
		{
			CombineAssertions("Actual Dimensions", () =>
			{
				AssertEquals("Package ID", expectedID, package.KP_PackageID);
				AssertEquals("Package Qty", expectedQty, package.KP_PackageQty);
				AssertEquals("Package Pack Type", expectedPkgType, package.KP_F3_NKPackType);
				AssertEquals("Package Volume", expectedVolume, package.KP_Volume);
				AssertEquals("Package Volume Unit", expectedVolumeUnit, package.KP_VolumeUQ);
				AssertEquals("Package Weight", expectedWeight, package.KP_Weight);
				AssertEquals("Package Weight Unit", expectedWeightUnit, package.KP_WeightUQ);
			});

			CombineAssertions("Booked Dimensions", () =>
			{
				AssertEquals("Booked Package Dimensions FK", package.PK, package.BookedDimensions.KPB_KP_Package);
				AssertEquals("Booked Package Qty", expectedQty, package.BookedDimensions.KPB_PackageQty);
				AssertEquals("Booked Package Volume", expectedVolume, package.BookedDimensions.KPB_Volume);
				AssertEquals("Booked Package Volume Unit", expectedVolumeUnit, package.BookedDimensions.KPB_VolumeUQ);
				AssertEquals("Booked Package Weight", expectedWeight, package.BookedDimensions.KPB_Weight);
				AssertEquals("Booked Package Weight Unit", expectedWeightUnit, package.BookedDimensions.KPB_WeightUQ);
			});
		}

		void RecipientRoleTargettedToThisModuleCoreTestCase(RecipientRoleType recipientRoleType, DataContextType? dataContextType, bool internalTransmission, bool setTestCbaId, bool expectedTargetted)
		{
			IShipmentDataContextManager manager = new DtbBookingConsolidationDataContextManager();
			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = recipientRoleType } } });
			if (dataContextType.HasValue)
			{
				dataContext.AddDataSource(dataContextType.Value, string.Empty);
			}
			var outboundSessionTracker = internalTransmission ? new DataWritingManager(new ActionInfo(recipientRoleType, Factory.New<DummyBusinessObject>())) : null;
			IDisposable disposableRegistrySet = null;

			try
			{
				if (setTestCbaId)
				{
					disposableRegistrySet = TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTCBAID");
				}
				manager.DefaultDataTargetFromRecipientRole(dataContext, new DummyXmlSessionTracker(outboundSessionTracker));
			}
			finally
			{
				disposableRegistrySet?.Dispose();
			}

			var message = $@"{(internalTransmission ? "Internal Transmission" : "External Transmission")}, Recipient Role Type: {recipientRoleType.ToString()}, DataSource {(dataContextType.HasValue ? dataContextType.Value.ToString() : "null")} {(setTestCbaId ? ", Registry Test Cba Id set to 'TESTCBAID' " : string.Empty)}, Should {(expectedTargetted ? string.Empty : "not ")}send Universal Shipment to the Transport Booking Module";
			if (expectedTargetted)
			{
				AssertNotNull(message, dataContext.DataTargetCollection);
			}
			else
			{
				AssertNull(message, dataContext.DataTargetCollection);
			}
		}

		void AssertReadFromTestFile_TransportBooking_Instruction_CollectionContentCore(string universalMessageText, CollectionContent? content = null)
		{
			var testDataHelper = new TransportBookingTestHelper(Factory.BOFactory);
			var packingHelper = new PackingTestHelper(Factory.BOFactory);
			var consolidation = testDataHelper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking1 = testDataHelper.CreateBooking(consolidation);
			booking1.KM_JobID = "TB00000001";
			var booking2 = testDataHelper.CreateBooking(consolidation);
			booking2.KM_JobID = "TB00000002";
			var booking3 = testDataHelper.CreateBooking(consolidation);
			booking3.KM_JobID = "TB00000003";

			var picInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction1 = testDataHelper.CreateInstruction(booking1, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var picInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			picInstruction2.KN_ServiceInstruction = "TB 2 Sequence 1 old service instruction";
			var dlvInstruction2 = testDataHelper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var picInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, null);
			var dlvInstruction3 = testDataHelper.CreateInstruction(booking3, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(universalMessageText);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			AssertMultilineASCIIEquals("Service Task Log", @"Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBookingConfirmation, 1 x DtbBookingInstruction, 1 x DtbBooking.".Trim(), serviceTaskLog.ToString());

			var logNoteText = message.GetLogNoteText();

			var newFactory = new BusinessObjectFactory();
			var booking2_InNewFactory = newFactory.Load<DtbBooking>(booking2.PK);
			var dlvInstruction2_InNewFactory = newFactory.Load<DtbBookingInstruction>(dlvInstruction2.PK);
			if (content == CollectionContent.Complete || content == null)
			{
				var picInstruction2_InNewFactory = booking2_InNewFactory.Instructions.Single();

				CombineAssertions("Assertions for a complete collection or no collection content value", () =>
				{
					AssertMultilineASCIIEquals("message.GetLogNoteText() should indicate no matching DtbBookingInstruction, as they all get deleted in DtbBookingConsolidationDataObjectReader method PopulateBusinessObject", @"Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBookingConfirmation, 1 x DtbBookingInstruction, 1 x DtbBooking.".Trim(), logNoteText);

					AssertEquals("Second instruction should be deleted", 1, booking2_InNewFactory.Instructions.Count);
					AssertNull("Second instruction should be deleted", dlvInstruction2_InNewFactory);

					AssertNotEquals("Instruction PK should change, because all instructions are deleted in DtbBookingConsolidationDataObjectReader method PopulateBusinessObject before getting populated again", picInstruction2.PK, picInstruction2_InNewFactory.PK);
					AssertEquals("Service instruction should match new value from the XML", "TB 2 Sequence 1 new service instruction", picInstruction2_InNewFactory.ServiceInstruction);
				});
			}
			else
			{
				var picInstruction2_InNewFactory = newFactory.Load<DtbBookingInstruction>(picInstruction2.PK);

				CombineAssertions("Assertions for partial collection", () =>
				{
					AssertMultilineASCIIEquals("message.GetLogNoteText() should include successfully matching DtbBookingInstruction", @"Successfully loaded matching DtbBookingConsolidation.
Populating DtbBookingConsolidation...
Warning - Could not find Sending Party Organisation with eHub Client ID 'HYEUA1DAU'.
Successfully loaded matching DtbBookingConsolidationPkgPackageJob.
Populating DtbBookingConsolidationPkgPackageJob...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
No matching PkgPackage found, creating new PkgPackage.
Populating PkgPackage...
Successfully loaded matching DtbBooking.
Populating DtbBooking...
Successfully loaded matching DtbBookingInstruction.
Populating DtbBookingInstruction...
No matching DtbBookingConfirmation found, creating new DtbBookingConfirmation.
Populating DtbBookingConfirmation...
Updated Transport Booking TB00000002 from UniversalShipment.
Updated Transport Booking Consolidation CM00000001 from UniversalShipment.
Successfully saved Transport Booking Consolidation CM00000001 with 3 x PkgPackage, 1 x DtbBookingConsolidationPkgPackageJob, 1 x DtbBookingConfirmation, 1 x DtbBookingInstruction, 1 x DtbBooking.".Trim(), logNoteText);

					AssertEquals("Both instructions should be available, even if a partial collection does not mention the second instruction", 2, booking2_InNewFactory.Instructions.Count);
					AssertNotNull("Both instructions should be available, even if a partial collection does not mention the second instruction", dlvInstruction2_InNewFactory);

					AssertEquals("Instruction PK should remain the same", picInstruction2.PK, picInstruction2_InNewFactory.PK);
					AssertEquals("Service instruction should be updated", "TB 2 Sequence 1 new service instruction", picInstruction2_InNewFactory.ServiceInstruction);
				});
			}
		}

		sealed class DummyXmlSessionTracker : IXmlSessionTracker
		{
			public DummyXmlSessionTracker(IDataWritingManager outboundSessionTracker)
				: base()
			{
				this.outboundSessionTracker = outboundSessionTracker;
			}
			readonly IDataWritingManager outboundSessionTracker;

			public IEDIMessage SourceMessage { get; set; }

			public bool HasErrors { get; private set; }

			public bool HasWarnings { get; private set; }

			public IUserContext SessionUserContext { get; set; }

			IEnumerable<IImportResult> IXmlSessionTracker.ImportResults => throw new NotImplementedException();

			void IXmlSessionTracker.LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey) => throw new NotImplementedException();

			void IXmlSessionTracker.LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID) => throw new NotImplementedException();

			IDataWritingManager IXmlSessionTracker.OutboundSessionTracker => outboundSessionTracker;

			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

			INotificationEmailManager IXmlSessionTracker.NotificationEmailManager => notificationEmailManager ?? (notificationEmailManager = new NotificationEmailManager());
			INotificationEmailManager notificationEmailManager;

			void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject targetBO) => throw new NotImplementedException();

			bool IXmlImportLogger.OrgMatchingDisabled => false;

			bool IXmlImportLogger.HasIgnoredModule
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			bool IXmlImportLogger.IsUpdatingConsol
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			void IXmlImportLogger.LogBoth(Integration.LogType type, string message) => throw new NotImplementedException();

			void IXmlImportLogger.LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) => throw new NotImplementedException();

			void IXmlImportLogger.LogErrorToServiceTaskOnly(string message) => throw new NotImplementedException();

			ITopLevelDataObject IXmlImportLogger.TopLevelDataObject => throw new NotImplementedException();

			IDataContextDataObject IXmlImportLogger.TopLevelDataContext => throw new NotImplementedException();

			public void StartProcessingASubShipment() => throw new NotImplementedException();

			public void EndProcessingASubShipment() => throw new NotImplementedException();

			public void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType) => throw new NotImplementedException();

			public void RecordAUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType) => throw new NotImplementedException();

			public void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType) => throw new NotImplementedException();

			void ISimpleLogger.Log(Integration.LogType type, string message) => throw new NotImplementedException();

			public IDisposable SetCurrentMessageContext(ZGuid messagePK)
			{
				throw new NotImplementedException();
			}

			public bool IsCurrentMessageContextSet(ZGuid messagePK)
			{
				throw new NotImplementedException();
			}

			IEnumerable<ISimpleLog> ISimpleLogResult.Logs => throw new NotImplementedException();
		}

		sealed class DtbBookingConsolidationDataContextManagerForTest : DtbBookingConsolidationDataContextManager
		{
			public ShipmentDataObjectReader<DtbBookingConsolidation> GetShipmentDataObjectReaderForTest(UniversalShipment universalShipment)
			{
				return GetShipmentDataObjectReader(universalShipment, new DummyLogger(), new UniversalObjectFactory()) as ShipmentDataObjectReader<DtbBookingConsolidation>;
			}
		}

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new DtbBookingConsolidationDataContextManager().DataContextType);
		}

		DataContextType ExpectedDataContextType => DataContextType.TransportBookingConsolidation;

		public void TestDataContextKey()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			consolidation.KB_JobID = "CM00001";
			AssertEquals("CM00001", consolidation.GetUniversalDataContextManager().DataContextKey);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var consolidation = new TransportBookingTestHelper(Factory.BOFactory).CreateBooking().ConsolidationSingleJob;
			consolidation.KB_JobID = "CM00001";
			Factory.SaveForTesting();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(ExpectedDataContextType, consolidation.KB_JobID);
			consolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var message = GetQueuedUniversalShipmentMessage(consolidationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var importedConsolidation = (DtbBookingConsolidation)importResults.Single(i => i.DataContextType == ExpectedDataContextType).GetBizOForTesting(consolidationDataObject, Factory.BOFactory);
			AssertEquals("Should have matched existing Consolidation by Job Number.", consolidation.PK, importedConsolidation.PK);
			AssertContains("Service Task Log should mention *Update*.", string.Format(@"
Updated {0} from UniversalShipment.
Successfully saved {0}".Trim(), importedConsolidation.HumanReadableName), serviceTaskLog.ToString());
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbBookingConsolidationDataContextManager().DefaultOutputDirectory);
		}

		public void TestManagesShipments()
		{
			AssertEquals(true, new DtbBookingConsolidationDataContextManager().ManagesShipments);
		}

		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbBookingConsolidationDataContextManager();
			AssertEquals(typeof(DtbBookingConsolidationDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		public void TestEventParentFinder()
		{
			IEventDataContextManager manager = new DtbBookingConsolidationDataContextManager();
			AssertNull(manager.GetLogParentsForEvent(new UniversalEvent(), Factory.BOFactory, new TestErrorLogger()));
		}
	}
}
