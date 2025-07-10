using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	[TestedType(typeof(ShipmentStatusUpdatedLogSubscriber))]
	class QuotedBookingStatusUpdatedLogProcessorTest : LogSubscriberTest<ShipmentStatusUpdatedLogSubscriber>
	{
		public void TestStuEventWithoutShipmentStatusParameters()
		{
			var booking = CreateBooking();
			var log = booking.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.StatusUpdated.Code;
				log.SL_Reference = "Propagated: All Shipment Containers|DEP=Carrier|LOC=USLUI|TYP=Received for import transfer";
				log.SL_EventTime = DateTime.Now;
				log.SL_IsEstimate = false;
			}

			Factory.Save();
			RunLogWalkerCycleForTest();
			var notifierLogs = ((LoggerForTesting)Notifier).NotifiedEventList.Where(l => l.StartsWith($"[{LogSubscriber.FriendlyName}]"));
			var logs = string.Join(System.Environment.NewLine, notifierLogs);
			AssertNotContains("Object reference not set to an instance of an object.", logs);
		}

		public void TestMAAUniversalEventCreatedFromBKDEvent()
		{
			var booking = CreateBooking();
			booking.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Factory.Save();
			RunLogWalkerCycleForTest();
			var ediMessage = Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();
			AssertNull(ediMessage);
		}

		public void TestMRJUniversalEventCreatedFromBKJEvent()
		{
			var booking = CreateBooking();
			booking.ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
			Factory.Save();
			AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingRejected, MostRecentSTULog(booking.Logs).Parameters[Params.New]);
			RunLogWalkerCycleForTest();
			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageRejectedCode, "Booking Confirmation");
		}

		public void TestMAAUniversalEventCreatedFromCNFEvent_WithHIRReference()
		{
			var booking = CreateBooking();
			var shipment = booking.Booking;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();
			AssertEquals("Check event new status", ShipmentStatusList.Codes.Confirmed, MostRecentSTULog(shipment.Logs).Parameters[Params.New]);
			RunLogWalkerCycleForTest();
			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageAcceptedCode, "BL Data");
		}

		public void TestMAAUniversalEventCreatedFromCNFEvent_WithoutHIRReference()
		{
			var booking = CreateBooking(false);
			var shipment = booking.Booking;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();
			AssertEquals("Check event new status", ShipmentStatusList.Codes.Confirmed, MostRecentSTULog(shipment.Logs).Parameters[Params.New]);
			RunLogWalkerCycleForTest();
			var ediMessage = Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();
			AssertNull(ediMessage);
		}

		public void TestMRJUniversalEventCreatedFromSIJEvent()
		{
			var booking = CreateBooking();
			var shipment = booking.Booking;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
			Factory.Save();
			AssertEquals("Check event new status", ShipmentStatusList.Codes.SIRejected, MostRecentSTULog(shipment.Logs).Parameters[Params.New]);
			RunLogWalkerCycleForTest();
			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageRejectedCode, "BL Data");
		}

		public void TestMWAUniversalEventCreated_Withdraw_FromBKXEvent()
		{
			var booking = CreateBooking();
			booking.ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

			Factory.Save();

			Thread.Sleep(100);
			booking.LogStatusChangedEvent(ShipmentStatusList.Codes.BookingCancelled, string.Empty);

			Factory.Save();

			var log = MostRecentSTULog(booking.Logs);
			AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingCancelled, log.Parameters[Params.New]);
			AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

			RunLogWalkerCycleForTest();

			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageWithdrawCancelAcceptedCode, "Booking Confirmation");
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithBKDEvent()
		{
			var booking = CreateBooking();
			booking.ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

			Factory.Save();

			Thread.Sleep(100);
			booking.LogStatusChangedEvent(ShipmentStatusList.Codes.Booked, "Reject withdraw and rollback as previous status as BKD");

			Factory.Save();

			var log = MostRecentSTULog(booking.Logs);
			AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, log.Parameters[Params.New]);
			AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

			RunLogWalkerCycleForTest();

			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageRejectedCode, "Booking Confirmation");
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithEBKEvent()
		{
			var booking = CreateBooking();
			booking.ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

			Factory.Save();

			Thread.Sleep(100);
			booking.LogStatusChangedEvent(ShipmentStatusList.Codes.ElectronicBooking, "Reject withdraw and rollback as previous status as EBK");

			Factory.Save();

			var log = MostRecentSTULog(booking.Logs);
			AssertEquals("Check event new status", ShipmentStatusList.Codes.ElectronicBooking, log.Parameters[Params.New]);
			AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

			RunLogWalkerCycleForTest();

			AssertUniversalShipmentSentToRecipient(booking, AutoEvents.MessageRejectedCode, "Booking Confirmation");
		}

		public void TestBranchIsSameAsSTULog()
		{
			var originalCurrentBranchPK = GlbBranch.CurrentBranch.PK;
			QuotedBooking booking;

			var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalCurrentBranchPK));
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				booking = CreateBooking();
				var shipment = booking.Booking;

				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			Thread.Sleep(10);

			booking.Logs.GetAllLogs().Reload(true);
			var mostRecentDEXLog = booking.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
			AssertEquals(otherBranch.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);

			var otherBranch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { originalCurrentBranchPK, otherBranch.PK }));
			otherBranch2.GB_IsActive = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				booking.ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				Thread.Sleep(100);
				booking.LogStatusChangedEvent(ShipmentStatusList.Codes.ElectronicBooking, "Reject withdraw and rollback as previous status as EBK");
				Factory.Save();
			}

			RunLogWalkerCycleForTest();

			booking.Logs.GetAllLogs().Reload(true);
			mostRecentDEXLog = booking.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
			AssertEquals(otherBranch2.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
		}

		#region Implementations
		QuotedBooking CreateBooking(bool includeHIRReference = true)
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "HYEDAUIKB";
			mode.EK_Module = "QBK";
			org.EDICommunicationsModes.Add(mode);
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;
			if (includeHIRReference)
			{
				var entryNum = shipment.Numbers.AddNew();
				entryNum.CE_EntryIsSystemGenerated = true;
				entryNum.CE_EntryType = CusEntryNumLookups.HIR;
				entryNum.CE_EntryNum = "12345";
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL_A";

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "VOYAGE_A";
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = jobVoyage.PK;
			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_JV = jobVoyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			quotedBooking.Booking.JS_TransportMode = "SEA";
			quotedBooking.Booking.JS_HouseBill = "HOUSE_BILL_NUMBER";
			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;
			return quotedBooking;
		}

		void AssertUniversalShipmentSentToRecipient(QuotedBooking booking, string purposeCode, string documentName)
		{
			var ediMessage = Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).First();
			AssertNotNull(ediMessage);
			var interchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);
			AssertNotNull(interchange);
			AssertEquals("SHIPPING_INSTRUCTION", interchange.EI_To);
			var universalShipment = XDocument.Parse(ediMessage.EM_MessageText).Root;
			AssertNotNull(universalShipment);
			var ns = universalShipment?.GetDefaultNamespace();
			var dataContext = universalShipment?.Elements(ns + "Shipment").Elements(ns + "DataContext").Single();
			var shipmentDataSource = dataContext?.Elements(ns + "DataSource").Single();
			AssertNotNull(shipmentDataSource);
			AssertEquals(booking.QuotedBookingNumber, shipmentDataSource?.Elements(ns + "Key").Single().Value);
			AssertEquals("ForwardingBooking", shipmentDataSource?.Elements(ns + "Type").Single().Value);
			var recipientRole = dataContext?.Elements(ns + "Workflow").Elements(ns + "RecipientRoleCollection").Elements().Single().Value;
			AssertEquals(nameof(RecipientRoleType.NVO), recipientRole);
			var purposeCodeInDoc = dataContext?.Elements(ns + "DocumentaryOverride").Elements(ns + "Purpose").Single().Value;
			AssertEquals(purposeCode, purposeCodeInDoc);
			if (purposeCode == AutoEvents.MessageRejectedCode)
			{
				var rejectionReasonNote = universalShipment?.Element(ns + "Shipment")?.Elements(ns + "NoteCollection").Elements().Single();
				AssertNotNull(rejectionReasonNote);
				AssertEquals("Reason for Rejection", rejectionReasonNote?.Elements(ns + "Description").Single().Value);
			}

			var transportLeg = universalShipment?.Elements(ns + "Shipment")?.Elements(ns + "TransportLegCollection").Elements().SingleOrDefault();
			if (documentName == "BL Data")
			{
				AssertEquals("http://www.cargowise.com/Schemas/Universal/2012/11/BLData/1", ns.ToString());
				AssertNull(transportLeg);
			}
			else
			{
				AssertEquals("http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1", ns.ToString());
				AssertNotNull(transportLeg);
				AssertEquals("AUSYD", transportLeg?.Elements(ns + "PortOfLoading").Single().Value);
				AssertEquals("NZAKL", transportLeg?.Elements(ns + "PortOfDischarge").Single().Value);
			}
		}

		StmALog MostRecentSTULog(Logs logs)
		{
			return logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_EventTime).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode);
		}

		#endregion
	}
}
