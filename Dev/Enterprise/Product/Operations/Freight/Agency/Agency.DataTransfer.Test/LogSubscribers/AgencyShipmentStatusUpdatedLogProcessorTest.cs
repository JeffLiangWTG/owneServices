using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentStatusUpdatedLogSubscriber))]
	class AgencyShipmentStatusUpdatedLogProcessorTest : LogSubscriberTest<ShipmentStatusUpdatedLogSubscriber>
	{
		public void TestEnable()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, MostRecentSTULog(agencyShipment.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);

				agencyShipment.Confirm();
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		#region AgencyBooking

		public void TestShouldNotProcessWithoutHIRNumber_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyShipment.Numbers.RemoveAndDeleteAll();
				Factory.Save();

				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, MostRecentSTULog(agencyShipment.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestStuEventWithoutShipmentStatusParameters_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				var log = agencyShipment.Logs.AddNew();
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
		}

		public void TestMAAUniversalEventCreatedFromBKDEvent_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, MostRecentSTULog(agencyShipment.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(agencyShipment, AutoEvents.MessageAcceptedCode);

				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);

				agencyShipment.Confirm();
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreatedFromBKJEvent_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				Factory.Save();

				AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingRejected, MostRecentSTULog(agencyShipment.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(agencyShipment, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);

				agencyShipment.Confirm();
				Factory.Save();

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMWAUniversalEventCreated_Withdraw_FromBKXEvent_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
				Factory.Save();

				var log = MostRecentSTULog(agencyShipment.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingCancelled, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(agencyShipment, AutoEvents.MessageWithdrawCancelAcceptedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithBKDEvent_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();

				var log = MostRecentSTULog(agencyShipment.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(agencyShipment, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithEBKEvent_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();

				var log = MostRecentSTULog(agencyShipment.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.ElectronicBooking, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(agencyShipment, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestBranchIsSameAsSTULog_AgencyBooking()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var originalCurrentBranchPK = GlbBranch.CurrentBranch.PK;

				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalCurrentBranchPK));
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
					Factory.Save();
				}

				RunLogWalkerCycleForTest();
				Thread.Sleep(10);

				agencyShipment.Logs.GetAllLogs().Reload(true);
				var mostRecentDEXLog = agencyShipment.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);

				var otherBranch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { originalCurrentBranchPK, otherBranch.PK }));
				otherBranch2.GB_IsActive = false;
				Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
					Factory.Save();
				}

				RunLogWalkerCycleForTest();

				agencyShipment.Logs.GetAllLogs().Reload(true);
				mostRecentDEXLog = agencyShipment.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch2.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
			}
		}

		#endregion

		#region BillOfLading

		public void TestShouldNotProcessWithoutHIRNumber_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyShipment.Numbers.RemoveAndDeleteAll();
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, MostRecentSTULog(billOfLading.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertEquals(ediMessageCount, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestStuEventWithoutShipmentStatusParameters_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var agencyShipment = CreateAgencyShipment<BillOfLading>();
				var log = agencyShipment.Logs.AddNew();
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
		}

		public void TestMAAUniversalEventCreatedFromBKDEvent_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, MostRecentSTULog(billOfLading.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(billOfLading, AutoEvents.MessageAcceptedCode);

				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreatedFromBKJEvent_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingRejected;
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingRejected, MostRecentSTULog(billOfLading.Logs).Parameters[Params.New]);

				RunLogWalkerCycleForTest();
				AssertUniversalShipmentSentToRecipient(billOfLading, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMWAUniversalEventCreated_Withdraw_FromBKXEvent_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				var log = MostRecentSTULog(billOfLading.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.BookingCancelled, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(billOfLading, AutoEvents.MessageWithdrawCancelAcceptedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithBKDEvent_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				var log = MostRecentSTULog(billOfLading.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.Booked, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(billOfLading, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestMRJUniversalEventCreated_Withdraw_RejectedWithEBKEvent_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				agencyShipment.Confirm();
				Factory.Save();

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				var log = MostRecentSTULog(billOfLading.Logs);
				AssertEquals("Check event new status", ShipmentStatusList.Codes.ElectronicBooking, log.Parameters[Params.New]);
				AssertEquals("Check event old status", ShipmentStatusList.Codes.EBookingCancellationRequest, log.Parameters[Params.Old]);

				RunLogWalkerCycleForTest();

				AssertUniversalShipmentSentToRecipient(billOfLading, AutoEvents.MessageRejectedCode);
				AssertEquals(ediMessageCount + 1, Factory.Load<Messaging.Integration.IEDIMessage>(new ZQuery()).Length);
			}
		}

		public void TestBranchIsSameAsSTULog_BillOfLading()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var originalCurrentBranchPK = GlbBranch.CurrentBranch.PK;

				var agencyShipment = CreateAgencyShipment<AgencyBooking>();
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				Factory.Save();

				var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalCurrentBranchPK));
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
					agencyShipment.Confirm();
					Factory.Save();
				}

				RunLogWalkerCycleForTest();
				Thread.Sleep(10);

				var billOfLading = Factory.Load<BillOfLading>(agencyShipment.PK);
				billOfLading.Logs.GetAllLogs().Reload(true);

				var mostRecentDEXLog = billOfLading.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);

				var otherBranch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { originalCurrentBranchPK, otherBranch.PK }));
				otherBranch2.GB_IsActive = false;
				Factory.Save();

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
					Factory.Save();
				}

				RunLogWalkerCycleForTest();

				billOfLading.Logs.GetAllLogs().Reload(true);
				mostRecentDEXLog = billOfLading.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.DataExportCode);
				AssertEquals(otherBranch2.GB_Code, mostRecentDEXLog.SL_GB_NKBranch);
			}
		}

		#endregion

		#region Implementations

		T CreateAgencyShipment<T>() where T : AgencyShipment
		{
			var shipment = Factory.New<T>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_FileFormat = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "HYEDAUIKB";
			mode.EK_Module = "QBK";
			org.EDICommunicationsModes.Add(mode);
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL_A";

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

			shipment.JS_TransportMode = "SEA";
			shipment.JS_HouseBill = "HOUSE_BILL_NUMBER";
			shipment.JS_JX = sailing.PK;

			shipment.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			return shipment;
		}

		void AssertUniversalShipmentSentToRecipient(AgencyShipment shipment, string purposeCode, string documentName = "Booking Confirmation")
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
			AssertEquals(shipment.GetType().Name, shipmentDataSource?.Elements(ns + "Type").Single().Value);
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
