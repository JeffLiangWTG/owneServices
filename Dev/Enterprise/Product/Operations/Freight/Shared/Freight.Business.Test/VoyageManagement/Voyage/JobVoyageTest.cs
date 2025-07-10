using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration.Test;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Agency;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobVoyageTest : BaseFreightTest
	{
		public void TestBusinessObjectsWithRelatedEvents()
		{
			// Arrange
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_A_DEP = ZDateTime.Today;
			var dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = USPort;
			dest.JB_A_ARV = ZDateTime.Today.AddDays(10);

			voyage.GenerateSailings();
			AssertEquals(1, voyage.Sailings.Count);
			var sailing = voyage.Sailings[0];

			// Act & Assert
			var businessObjectsWithRelatedEvents = voyage.BusinessObjectsWithRelatedEvents;
			AssertEquals(3, businessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { sailing, dest, origin }, businessObjectsWithRelatedEvents);
		}

		public void TestOnSaving_SBREventCreated_VoyageFlightChanged()
		{
			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDE";
			vessel.RV_LloydsNumber = "9463085";
			vessel.RV_OH = carrier.PK;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "ASD1234";

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyageOrigin.JA_E_DEP = DateTime.Today;
			voyageOrigin.JA_A_DEP = DateTime.Today;

			Factory.Save();

			var log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			var logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals("ASD1234", logVoyageOrigin.Voyage.JV_VoyageFlight);

			voyage.JV_VoyageFlight = "AAAAA";

			Factory.Save();

			var reloadedLog = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertNotEquals(log.PK, reloadedLog.PK);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals("AAAAA", logVoyageOrigin.Voyage.JV_VoyageFlight);
		}

		public void TestOnSaving_SBREventCreated_VesselChanged()
		{
			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDE";
			vessel.RV_LloydsNumber = "9463085";
			vessel.RV_OH = carrier.PK;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "ASD1234";

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyageOrigin.JA_E_DEP = DateTime.Today;
			voyageOrigin.JA_A_DEP = DateTime.Today;

			Factory.Save();

			var log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			var logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals("ABSDE", logVoyageOrigin.Voyage.JV_RV_NKVessel);

			var newVessel = Factory.NewWithValidTestData<RefVessel>();
			newVessel.RV_Name = "AAAAA";
			newVessel.RV_LloydsNumber = "9308390";
			voyage.JV_RV_NKVessel = newVessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;

			Factory.Save();

			var reloadedLog = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertNotEquals(log.PK, reloadedLog.PK);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals("AAAAA", logVoyageOrigin.Voyage.JV_RV_NKVessel);
		}

		public void TestOnSaving_SBREventCreated_CarrierChanged()
		{
			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDE";
			vessel.RV_LloydsNumber = "9463085";
			vessel.RV_OH = carrier.PK;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "ASD1234";

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyageOrigin.JA_E_DEP = DateTime.Today;
			voyageOrigin.JA_A_DEP = DateTime.Today;

			Factory.Save();

			var log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			var logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals("ABSDE", logVoyageOrigin.Voyage.JV_RV_NKVessel);

			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();
			newCarrier.OH_Code = "NewOrg";
			newCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			newCarrier.OH_Code = "AAAAA";
			voyage.JV_OH_Line = newCarrier.PK;

			Factory.Save();

			var reloadedLog = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertNotEquals(log.PK, reloadedLog.PK);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			logVoyageOrigin = Factory.Load<VoyageOrigin>(log.SL_Parent);
			AssertNotNull(logVoyageOrigin);
			AssertEquals(voyageOrigin.PK, logVoyageOrigin.PK);
			AssertNotNull(logVoyageOrigin.Voyage);
			AssertEquals(voyage.PK, logVoyageOrigin.Voyage.PK);
			AssertEquals(newCarrier.PK, logVoyageOrigin.Voyage.JV_OH_Line);
		}

		public void TestOnSaving_UpdateFlightSubscriptionEvent()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin1 = voyage.Origins.AddNew();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_E_DEP = 5.DaysAgo();

				var destination1 = voyage.Destinations.AddNew();
				destination1.JB_RL_NKPortOfDischarge = "SGSIN";
				destination1.JB_E_ARV = 1.DaysAgo();

				var origin2 = voyage.Origins.AddNew();
				origin2.JA_RL_NKPortOfLoading = "SGSIN";
				origin2.JA_E_DEP = 3.DaysAgo();

				var destination2 = voyage.Destinations.AddNew();
				destination2.JB_RL_NKPortOfDischarge = "HKHKG";
				destination2.JB_E_ARV = 1.DaysAgo();

				voyage.GenerateSailings();

				AssertEquals("Precondition", 3, voyage.Sailings.Count);

				Factory.Save();

				var sailing1 = voyage.Sailings[0];
				var sailing2 = voyage.Sailings[1];
				var sailing3 = voyage.Sailings[1];

				var expectedOrigin1EventReference = $"|FDT={origin1.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001";
				var expectedOrigin2EventReference = $"|FDT={origin2.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001";
				AssertFlightSubscriptionEvent("Precondition", sailing1, 1, expectedOrigin1EventReference);
				AssertFlightSubscriptionEvent("Precondition", sailing2, 1, expectedOrigin2EventReference);
				AssertFlightSubscriptionEvent("Precondition", sailing3, 1, expectedOrigin2EventReference);

				voyage.JV_VoyageFlight = "QF002";
				Factory.Save();

				expectedOrigin1EventReference = $"|FDT={origin1.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF002";
				expectedOrigin2EventReference = $"|FDT={origin2.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF002";
				AssertFlightSubscriptionEvent("Flight Number changed, should create a new SBR event", sailing1, 2, expectedOrigin1EventReference);
				AssertFlightSubscriptionEvent("Flight Number changed, should create a new SBR event", sailing2, 2, expectedOrigin2EventReference);
				AssertFlightSubscriptionEvent("Flight Number changed, should create a new SBR event", sailing3, 2, expectedOrigin2EventReference);

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_Code = "CARRIER";
				voyage.JV_OH_Line = carrier.PK;
				Factory.Save();

				AssertFlightSubscriptionEvent("Carrier changed, should not create a new SBR event", sailing1, 2, expectedOrigin1EventReference);
				AssertFlightSubscriptionEvent("Carrier changed, should not create a new SBR event", sailing2, 2, expectedOrigin2EventReference);
				AssertFlightSubscriptionEvent("Carrier changed, should not create a new SBR event", sailing3, 2, expectedOrigin2EventReference);
			}
		}

		void AssertFlightSubscriptionEvent(string message, JobSailing sailing, int expectedLogCount, string expectedEventReference)
		{
			var sailingLogs = sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

			AssertNotNull(sailingLogs);
			AssertEquals(expectedLogCount, sailingLogs.Count);
			AssertEquals("Event Reference", expectedEventReference, sailingLogs[0].SL_Reference);
			AssertEquals("Event IsCancelled", false, sailingLogs[0].SL_IsCancelled);

			for (int i = 1; i < sailingLogs.Count; i++)
			{
				var log = sailingLogs[i];
				AssertNotNull(log);
				AssertEquals("Old SBR event should be cancelled", true, log.SL_IsCancelled);
			}
		}

		public void TestOnSaved_ReportExceptionForVoyagesWithoutSailings()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			AssertEquals("0 origins before save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: False, HasChangesNotIncludingChildren: False"));

			ErrorReporter.Clear();
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWithContainerMoves()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var containerMovement = Factory.NewWithValidTestData(ObjectFactory.GetType("IContainerMovement"));
			containerMovement[JobContainerMoveSchema.Constants.E9_JV] = voyage.PK;

			AssertEquals("0 origins before save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWithVoyAccounts()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var voyageAccount = Factory.NewWithValidTestData(ObjectFactory.GetType("IVoyageAccount"));
			voyageAccount[JobVoyAccountSchema.Constants.NA_JV] = voyage.PK;

			AssertEquals("0 origins before save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWhenLogsAreAdded()
		{
			DoNotReportExceptionForOrphanedVoyagesOnSaved(voyage => voyage.Logs.AddNew());
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWhenTasksAreAdded()
		{
			DoNotReportExceptionForOrphanedVoyagesOnSavedWhenWorkflowItemsAreAdded(workflowProvider => workflowProvider.WorkflowItems.Tasks.AddNew());
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWhenMilestonesAreAdded()
		{
			DoNotReportExceptionForOrphanedVoyagesOnSavedWhenWorkflowItemsAreAdded(workflowProvider => workflowProvider.WorkflowItems.Milestones.AddNew());
		}

		public void TestOnSaved_DoNotReportExceptionForOrphanedVoyagesWhenTriggersAreAdded()
		{
			DoNotReportExceptionForOrphanedVoyagesOnSavedWhenWorkflowItemsAreAdded(workflowProvider => workflowProvider.WorkflowItems.Triggers.AddNew());
		}

		void DoNotReportExceptionForOrphanedVoyagesOnSavedWhenWorkflowItemsAreAdded(Func<IWorkflowProvider, ProcessTask> addTask)
		{
			var sequence = 1;
			DoNotReportExceptionForOrphanedVoyagesOnSaved(voyage =>
			{
				var workflowProvider = voyage as IWorkflowProvider;
				AssertNotNull(workflowProvider);

				var task = addTask(workflowProvider);
				task.P9_Description = $"Description {sequence}";
				task.P9_Sequence = sequence++;
				return task;
			});
		}

		void DoNotReportExceptionForOrphanedVoyagesOnSaved(Func<JobVoyage, BusinessObject> addBizo)
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailing before save", 1, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("No exception is reported to ErrorReporter.", 0, ErrorReporter.LastExceptionsReported().Count);
			voyage.OrphanedVoyageDebugLog.Clear();

			var bizo = addBizo(voyage);
			voyage.Sailings.RemoveAndDeleteAll();

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			CombineAssertions($"Remove sailings. LastMessageReported: {ErrorReporter.LastMessageReported}", () =>
			{
				Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
				Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: True, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains($"{bizo.GetType().Name} {bizo.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
				Assert(ErrorReporter.LastMessageReported.Contains("JobSailingCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			});

			ErrorReporter.Clear();
			voyage.OrphanedVoyageDebugLog.Clear();

			addBizo(voyage);

			Factory.Save();

			AssertEquals("No exception is reported to ErrorReporter.", 0, ErrorReporter.LastExceptionsReported().Count);
			voyage.OrphanedVoyageDebugLog.Clear();
		}

		public void TestOnSaved_DoNotReportExceptionForExistingVoyagesWithoutSailings()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			AssertEquals("0 origins before save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: False, HasChangesNotIncludingChildren: False"));

			ErrorReporter.Clear();

			Factory.Save();

			AssertEquals("No exception is reported to ErrorReporter.", 0, ErrorReporter.LastExceptionsReported().Count);
		}

		public void TestOnSaved_ReportExceptionForVoyagesWithOriginButWithoutSailings()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("0 destinations before save", 0, voyage.Destinations.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("0 destinations after save", 0, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			CombineAssertions($"Add an origin. LastMessageReported: {ErrorReporter.LastMessageReported}", () =>
			{
				Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
				Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains("VoyageOriginDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains($"VoyageOrigin {origin.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			});

			ErrorReporter.Clear();
			voyage.OrphanedVoyageDebugLog.Clear();

			voyage.Origins.RemoveAndDeleteAll();

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 destinations after save", 0, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			CombineAssertions($"Remove origins. LastMessageReported: {ErrorReporter.LastMessageReported}", () =>
			{
				Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
				Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: True, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains("VoyageOriginDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			});

			ErrorReporter.Clear();
			voyage.OrphanedVoyageDebugLog.Clear();
		}

		public void TestOnSaved_ReportExceptionForVoyagesWithDestinationButWithoutSailings()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			AssertEquals("0 origins before save", 0, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			CombineAssertions($"Add an destination. LastMessageReported: {ErrorReporter.LastMessageReported}", () =>
			{
				Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
				Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains("VoyageDestinationDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains($"VoyageDestination {destination.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			});

			ErrorReporter.Clear();
			voyage.OrphanedVoyageDebugLog.Clear();

			voyage.Destinations.RemoveAndDeleteAll();

			Factory.Save();

			AssertEquals("0 origins after save", 0, voyage.Origins.Count);
			AssertEquals("0 destinations after save", 0, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			CombineAssertions($"Remove destinations. LastMessageReported: {ErrorReporter.LastMessageReported}", () =>
			{
				Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
				Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: True, HasChanges: True, HasChangesNotIncludingChildren: False"));
				Assert(ErrorReporter.LastMessageReported.Contains("VoyageDestinationDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			});

			ErrorReporter.Clear();
			voyage.OrphanedVoyageDebugLog.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidPortPairs()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageOriginDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageOrigin {origin.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageDestinationDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageDestination {destination.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains($"GenerateSailings did not create any sailings for Voyage:{voyage.PK}"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Sailing not created SEA Voyage:{voyage.PK} for Origin:AUBNE and Destination:AUBNE"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Updating Voyage:{voyage.PK} with 0 fSailings and 0 updatedSailings"));

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidPortPairs_InvalidDateChangeForExistingPorts_ChangeOriginForExistingSailing()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			var oldETDValue = ZDateTime.Today.AddDays(1);
			origin.JA_E_DEP = oldETDValue;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailings after save", 1, voyage.Sailings.Count);

			origin.JA_E_DEP = ZDateTime.Today.AddDays(3);
			voyage.GenerateSailings();

			Factory.Save();

			AssertInvalidDateChangeErrorReporting(voyage, origin, destination, origin, oldETDValue);

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidPortPairs_InvalidDateChangeForExistingPorts_ChangeDestinationForExistingSailing()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			var oldETAValue = ZDateTime.Today.AddDays(2);
			destination.JB_E_ARV = oldETAValue;

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailings after save", 1, voyage.Sailings.Count);

			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();

			Factory.Save();

			AssertInvalidDateChangeErrorReporting(voyage, origin, destination, destination, oldETAValue);

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidPortPairs_InvalidDateChangeForExistingPorts_AddOriginForExistingSailing()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailings after save", 1, voyage.Sailings.Count);

			voyage.Origins.Remove(origin);

			origin.JA_E_DEP = ZDateTime.Today.AddDays(3);
			voyage.Origins.Add(origin);

			voyage.GenerateSailings();

			Factory.Save();

			AssertInvalidDateChangeErrorReporting(voyage, origin, destination, origin, null);

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidPortPairs_InvalidDateChangeForExistingPorts_AddDestinationForExistingSailing()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailings after save", 1, voyage.Sailings.Count);

			voyage.Destinations.Remove(destination);

			destination.JB_E_ARV = ZDateTime.Today;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();

			Factory.Save();

			AssertInvalidDateChangeErrorReporting(voyage, origin, destination, destination, null);

			ErrorReporter.Clear();
		}

		void AssertInvalidDateChangeErrorReporting(JobVoyage voyage, VoyageOrigin origin, VoyageDestination destination, BusinessObject changedBusinessObject, ZDateTime? oldEstimatedTimeValue = null)
		{
			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageOriginDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageOrigin {origin.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageDestinationDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageDestination {destination.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains($"GenerateSailings did not create any sailings for Voyage:{voyage.PK}"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Updating Voyage:{voyage.PK} with 1 fSailings and 0 updatedSailings"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Voyage info: PK {voyage.PK}, transport mode: {voyage.JV_AirSeaRoad}"));
			if (changedBusinessObject is VoyageOrigin)
			{
				Assert(ErrorReporter.LastMessageReported.Contains($"Invalid change of date value introduced from Origin: {origin.JA_RL_NKPortOfLoading} {origin.PK}. Current ETD/ETDUtc: {origin.JA_E_DEP} / {origin.JA_E_DEP_UTC}"));
				var changedInfoLog = oldEstimatedTimeValue == null ? "Changed info: new added." : $"Changed info: JA_E_DEP, new value {origin.JA_E_DEP}, old value {oldEstimatedTimeValue}.";
				Assert(ErrorReporter.LastMessageReported.Contains(changedInfoLog));
			}
			else if (changedBusinessObject is VoyageDestination)
			{
				Assert(ErrorReporter.LastMessageReported.Contains($"Invalid change of date value introduced from Destination: {destination.JB_RL_NKPortOfDischarge} {destination.PK}. Current ETA/ETAUtc: {destination.JB_E_ARV} / {destination.JB_E_ARV_UTC}"));
				var changedInfoLog = oldEstimatedTimeValue == null ? "Changed info: new added." : $"Changed info: JB_E_ARV, new value {destination.JB_E_ARV}, old value {oldEstimatedTimeValue}.";
				Assert(ErrorReporter.LastMessageReported.Contains(changedInfoLog));
			}
		}

		public void TestGenerateSailings_LogsDeletedSailingAttributes()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "SYDNE";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);

			var existingSailing = Factory.New<JobSailing>();
			existingSailing.JX_JA = origin.PK;
			existingSailing.JX_JB = destination.PK;

			voyage.Sailings.Add(existingSailing);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("1 sailing before save", 1, voyage.Sailings.Count);

			voyage.GenerateSailings();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			origin.JA_A_DEP = ZDateTime.Now.AddDays(2);

			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Now.AddDays(5);
			destination.JB_A_ARV = ZDateTime.Now.AddDays(6);

			var duplicateSailing = voyage.Sailings.AddNew();
			duplicateSailing.JX_JA = origin.PK;
			duplicateSailing.JX_JB = destination.PK;

			var attribute_JX_JA_E_ARV = duplicateSailing.JX_JA_E_ARV.ToString();
			var attribute_JX_JA_E_DEP = duplicateSailing.JX_JA_E_DEP.ToString();
			var attribute_JX_JB_E_ARV = duplicateSailing.JX_JB_E_ARV.ToString();
			var attribute_JX_JA_A_ARV = duplicateSailing.JX_JA_A_ARV.ToString();
			var attribute_JX_JA_A_DEP = duplicateSailing.JX_JA_A_DEP.ToString();
			var attribute_JX_JB_A_ARV = duplicateSailing.JX_JB_A_ARV.ToString();
			var attribute_JX_JA_S_DEP = duplicateSailing.JX_JA_S_DEP.ToString();
			var attribute_JX_JA_S_ARV = duplicateSailing.JX_JA_S_ARV.ToString();

			voyage.GenerateSailings();

			var messgaes = $"Deleted Sailing:PK={duplicateSailing.PK}, attributes:[\r\n  \"JX_JA_E_ARV :{attribute_JX_JA_E_ARV}\",\r\n  \"JX_JA_E_DEP :{attribute_JX_JA_E_DEP}\",\r\n  \"JX_JB_E_ARV :{attribute_JX_JB_E_ARV}\",\r\n  \"JX_JA_A_ARV :{attribute_JX_JA_A_ARV}\",\r\n  " +
					$"\"JX_JA_A_DEP :{attribute_JX_JA_A_DEP}\",\r\n  \"JX_JB_A_ARV :{attribute_JX_JB_A_ARV}\",\r\n  \"JX_JA_S_DEP :{attribute_JX_JA_S_DEP}\",\r\n  \"JX_JA_S_ARV :{attribute_JX_JA_S_ARV}\"\r\n]\r\n";

			Assert(voyage.OrphanedVoyageDebugLog.ToString().Contains(messgaes));

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForMissingSailingForVoyagesWithInvalidDates()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(-1);

			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains($"JobVoyage {voyage.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageOriginDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageOrigin {origin.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains("VoyageDestinationDependentCollection , IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: False"));
			Assert(ErrorReporter.LastMessageReported.Contains($"VoyageDestination {destination.PK}, IsInDatabase: False, HasChanges: True, HasChangesNotIncludingChildren: True"));
			Assert(ErrorReporter.LastMessageReported.Contains($"GenerateSailings did not create any sailings for Voyage:{voyage.PK}"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Sailing not created due to Invalid Date Combination for SEA Voyage:{voyage.PK}"));
			Assert(ErrorReporter.LastMessageReported.Contains($"Updating Voyage:{voyage.PK} with 0 fSailings and 0 updatedSailings"));

			ErrorReporter.Clear();
		}

		public void TestOnSaved_ReportExceptionForVoyagesWhereSailingDeleted()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			destination.JB_RL_NKPortOfDischarge = "JPOSA";
			voyage.GenerateSailings();

			AssertEquals("1 origin before delete", 1, voyage.Origins.Count);
			AssertEquals("1 destination before delete", 1, voyage.Destinations.Count);
			AssertEquals("1 sailing before delete", 1, voyage.Sailings.Count);

			voyage.Sailings.DeleteAll();

			AssertEquals("1 origin before save", 1, voyage.Origins.Count);
			AssertEquals("1 destination before save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings before save", 0, voyage.Sailings.Count);

			Factory.Save();

			AssertEquals("1 origin after save", 1, voyage.Origins.Count);
			AssertEquals("1 destination after save", 1, voyage.Destinations.Count);
			AssertEquals("0 sailings after save", 0, voyage.Sailings.Count);

			Assert(ErrorReporter.LastMessageReported.Contains("No sailings found for voyage"));
			Assert(ErrorReporter.LastMessageReported.Contains("Deleting JobSailing"));

			ErrorReporter.Clear();
		}

		public void TestActiveFilter()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = "SEA";
			voyage1.JV_IsActive = true;

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = "SEA";
			voyage2.JV_IsActive = false;

			var allSeaVoyages = Factory.Load<JobVoyage>(new ZQuery(JobVoyageSchema.JV_AirSeaRoad, "SEA"));
			AssertCollectionContains(voyage1, allSeaVoyages);
			AssertCollectionNotContains(voyage2, allSeaVoyages);
		}

		public void TestICanDelete()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			AssertEquals("Precondition", false, sailing.IsReferenced());
			AssertEquals(true, voyage.CanDelete);
			AssertEquals(ZString.Empty, voyage.ReasonForNotAbleToDelete);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = sailing.PK;

			AssertEquals("Precondition", true, sailing.IsReferenced());
			AssertEquals(false, voyage.CanDelete);
			AssertEquals(sailing.GetReferencingJobNumbers(), voyage.ReasonForNotAbleToDelete);
		}

		#region IsInvalidDateCombinationForSailing

		public void TestIsInvalidDateCombinationForSailing()
		{
			var alofiPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NUALO");
			var naruPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NRINU");

			AssertEquals("Pre-condition: NUALO should have a UTC offset of -11", (ZDecimal)(-11), alofiPort.StandardZoneUTCOffset);
			AssertEquals("Pre-condition: NRINU should have a UTC offset of +12", (ZDecimal)12, naruPort.StandardZoneUTCOffset);

			RefUNLOCO fakePort = Factory.New<RefUNLOCO>();
			fakePort.RL_Code = "PRTNT";
			fakePort.RL_R3 = ZGuid.Empty;

			Factory.Save();

			ZDateTime testTime = new ZDateTime(2013, 08, 20);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NRINU";
			origin.JA_E_DEP = testTime.AddDays(1);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NUALO";
			destination.JB_E_ARV = testTime;

			voyage.GenerateSailings();
			Factory.Save();

			AssertNotEquals("Pre-condition: ETD should not be the same for both local and UTC time", origin.JA_E_DEP, origin.JA_E_DEP_UTC);
			AssertNotEquals("Pre-condition: ETA should not be the same for both local and UTC time", destination.JB_E_ARV, destination.JB_E_ARV_UTC);
			AssertEquals("Pre-condition: When local time at the Western Port is Midnight 21 AUG, should be UTC 11AM 20 AUG", new ZDateTime(2013, 08, 20, 12, 00, 00), origin.JA_E_DEP_UTC);
			AssertEquals("Pre-condition: When local time at the Eastern Port is Midnight 20 AUG, should be UTC 11AM 20 AUG", new ZDateTime(2013, 08, 20, 11, 00, 00), destination.JB_E_ARV_UTC);

			Assert("Expected to NOT be invalid as the UTC date for both ports is the same", !JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad));

			origin.JA_E_DEP = testTime.AddDays(5);
			Factory.Save();

			Assert("Expected to be invalid as origin is way ahead of destination date", JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad));

			destination.JB_E_ARV = testTime.AddDays(7);
			Factory.Save();

			Assert("Expected to NOT be invalid as ETA come after ETD", !JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad));

			destination.JB_RL_NKPortOfDischarge = "NOTME";
			Factory.Save();

			Assert("Expected to NOT be invalid as ETA still comes after ETD, disregarding time zone information", !JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad));

			destination.JB_E_ARV = testTime.AddDays(2);
			Factory.Save();

			Assert("Expected to be invalid as ETA still comes after ETD, disregarding time zone information", JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, destination, voyage.JV_AirSeaRoad));
		}

		#endregion

		public void TestFindOtherVoyagesWithSameVesselVoyageCombination()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Asgard";

			var voyage1 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "AUBNE", "NZAKL", true);
			var voyage2 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "NZAKL", "JPOSA", true);
			var voyage3 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "JPOSA", "CNSHA", true);

			var voyage4 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "456", "CNSHA", "SGSIN", true);
			var voyage5 = CreateVoyage(Constants.TransportModes.Sea, "Asgard", "123", "SGSIN", "MYKUL", true);
			var voyage6 = CreateVoyage(Constants.TransportModes.Air, "Visund", "123", "MYKUL", "INBOM", true);

			AssertContainsExactElementsInAnyOrder(new[] { voyage2, voyage3 }, voyage1.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder(new[] { voyage1, voyage3 }, voyage2.FindOtherVoyagesWithSameVesselVoyageCombination());

			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage4.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage5.FindOtherVoyagesWithSameVesselVoyageCombination());
		}

		public void TestFindOtherVoyagesWithSameVesselVoyageCombination_NoSailing()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Asgard";

			var voyage1 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "AUBNE", "NZAKL");
			var voyage2 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "NZAKL", "JPOSA");
			var voyage3 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123", "JPOSA", "CNSHA");

			var voyage4 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "456", "CNSHA", "SGSIN");
			var voyage5 = CreateVoyage(Constants.TransportModes.Sea, "Asgard", "123", "SGSIN", "MYKUL");
			var voyage6 = CreateVoyage(Constants.TransportModes.Air, "Visund", "123", "MYKUL", "INBOM");

			AssertContainsExactElementsInAnyOrder("Precondition: voyage1 has no sailing", Array.Empty<JobSailing>(), voyage1.Sailings);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage2 has no sailing", Array.Empty<JobSailing>(), voyage2.Sailings);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage3 has no sailing", Array.Empty<JobSailing>(), voyage3.Sailings);

			AssertContainsExactElementsInAnyOrder("Precondition: voyage4 has no sailing", Array.Empty<JobSailing>(), voyage4.Sailings);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage5 has no sailing", Array.Empty<JobSailing>(), voyage5.Sailings);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage6 has no sailing", Array.Empty<JobSailing>(), voyage6.Sailings);

			AssertContainsExactElementsInAnyOrder("voyage1 does not match other voyages with missing sailing", Array.Empty<JobVoyage>(), voyage1.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder("voyage1 does not match other voyages with missing sailing", Array.Empty<JobVoyage>(), voyage2.FindOtherVoyagesWithSameVesselVoyageCombination());

			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage4.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage5.FindOtherVoyagesWithSameVesselVoyageCombination());
		}

		public void TestFindOtherVoyagesWithSameVesselVoyageCombination_NoOriginsOrDestinations()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Visund";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Asgard";

			var voyage1 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123");
			var voyage2 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123");
			var voyage3 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "123");

			var voyage4 = CreateVoyage(Constants.TransportModes.Sea, "Visund", "456");
			var voyage5 = CreateVoyage(Constants.TransportModes.Sea, "Asgard", "123");
			var voyage6 = CreateVoyage(Constants.TransportModes.Air, "Visund", "123");

			AssertContainsExactElementsInAnyOrder("Precondition: voyage1 has no origin", Array.Empty<VoyageOrigin>(), voyage1.Origins);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage2 has no origin", Array.Empty<VoyageOrigin>(), voyage2.Origins);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage3 has no origin", Array.Empty<VoyageOrigin>(), voyage3.Origins);

			AssertContainsExactElementsInAnyOrder("Precondition: voyage4 has no origin", Array.Empty<VoyageOrigin>(), voyage4.Origins);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage5 has no origin", Array.Empty<VoyageOrigin>(), voyage5.Origins);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage6 has no origin", Array.Empty<VoyageOrigin>(), voyage6.Origins);

			AssertContainsExactElementsInAnyOrder("Precondition: voyage1 has no destination", Array.Empty<VoyageDestination>(), voyage1.Destinations);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage2 has no destination", Array.Empty<VoyageDestination>(), voyage2.Destinations);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage3 has no destination", Array.Empty<VoyageDestination>(), voyage3.Destinations);

			AssertContainsExactElementsInAnyOrder("Precondition: voyage4 has no destination", Array.Empty<VoyageDestination>(), voyage4.Destinations);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage5 has no destination", Array.Empty<VoyageDestination>(), voyage5.Destinations);
			AssertContainsExactElementsInAnyOrder("Precondition: voyage6 has no destination", Array.Empty<VoyageDestination>(), voyage6.Destinations);

			AssertContainsExactElementsInAnyOrder("voyage1 does not match other voyages with missing origin / destination", Array.Empty<JobVoyage>(), voyage1.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder("voyage1 does not match other voyages with missing origin / destination", Array.Empty<JobVoyage>(), voyage2.FindOtherVoyagesWithSameVesselVoyageCombination());

			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage4.FindOtherVoyagesWithSameVesselVoyageCombination());
			AssertContainsExactElementsInAnyOrder(Array.Empty<JobVoyage>(), voyage5.FindOtherVoyagesWithSameVesselVoyageCombination());
		}

		JobVoyage CreateVoyage(ZString mode, ZString vesselNK, ZString voyageFlight, string loadPort = "", string dischargePort = "", bool createSailing = false)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = mode;
			voyage.JV_RV_NKVessel = vesselNK;
			voyage.JV_VoyageFlight = voyageFlight;

			if (!string.IsNullOrEmpty(loadPort))
			{
				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = loadPort;
				origin.JA_E_DEP = ZDate.Today.AddDays(1);
			}

			if (!string.IsNullOrEmpty(dischargePort))
			{
				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = dischargePort;
				destination.JB_E_ARV = ZDate.Today.AddDays(10);
			}

			if (createSailing)
			{
				voyage.GenerateSailings();
			}
			else
			{
				voyage.Sailings.DeleteAll();
			}

			return voyage;
		}

		public void TestIControllerIDProviderInterface()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			var provider = (IControllerIDProvider)voyage;
			AssertEquals(voyage.PK, provider.BusinessObjectPK);
			AssertEquals(ControllerIDs.JobSeaVoyage, provider.ControllerID);
			voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals(ControllerIDs.JobAirSailing, provider.ControllerID);
		}

		public void TestJV_RV_NKVessel_ReadOnly()
		{
			var voyage = Factory.New<JobVoyage>();
			Assert("Pre-condition", !voyage.JV_RV_NKVesselInfo.ReadOnly);

			voyage.JV_AirSeaRoad = "SEA";
			Assert("Read-Write for SEA", !voyage.JV_RV_NKVesselInfo.ReadOnly);

			voyage.JV_AirSeaRoad = "AIR";
			Assert("Vessel is read-only for AIR", voyage.JV_RV_NKVesselInfo.ReadOnly);
		}

		public void TestVesselVoyageLogging()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "AVRORA";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "T";
			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_Name = "TITANIC";

			var voyage = Factory.New<JobVoyage>();
			Factory.Save();

			voyage.JV_VoyageFlight = "233S";
			voyage.JV_RV_NKVessel = vessel1.RV_FK;

			Factory.Save();
			AssertLogExists(voyage, "Vessel/Voyage '*empty*' CHANGED TO 'AVRORA'/'233S'");

			voyage.JV_VoyageFlight = "2";
			voyage.JV_RV_NKVessel = vessel2.RV_FK;
			voyage.JV_VoyageFlight = "233N";
			voyage.JV_RV_NKVessel = vessel3.RV_FK;

			Factory.Save();
			AssertLogExists(voyage, "Vessel/Voyage 'AVRORA'/'233S' CHANGED TO 'TITANIC'/'233N'");

			voyage.JV_VoyageFlight = "";

			Factory.Save();
			AssertLogExists(voyage, "Voyage '233N' CHANGED TO '*empty*'");

			voyage.JV_RV_NKVessel = "";

			Factory.Save();
			AssertLogExists(voyage, "Vessel 'TITANIC' CHANGED TO '*empty*'");
		}

		public void TestFlightLogging()
		{
			var voyage = Factory.New<JobVoyage>();
			Factory.Save();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "555G";

			Factory.Save();
			AssertLogExists(voyage, "Flight '*empty*' CHANGED TO '555G'");

			voyage.JV_VoyageFlight = "";
			voyage.JV_VoyageFlight = "333N";

			Factory.Save();
			AssertLogExists(voyage, "Flight '555G' CHANGED TO '333N'");

			voyage.JV_VoyageFlight = "";

			Factory.Save();
			AssertLogExists(voyage, "Flight '333N' CHANGED TO '*empty*'");
		}

		void AssertLogExists(BusinessObject bizO, string reference, string nkEvent = "EDT")
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, nkEvent);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);

			AssertNotNull(Factory.LoadTop1<StmALog>(query));
		}

		public void TestSailingDBHits()
		{
			ZGuid voyagePK;
			{
				string[] ports =
				{
					"NZAKL", "NZWEL",
					"AUSYD", "AUBNE", "AUCNS",
					"SGSIN", "MYBAG", "GBLON",
				};

				BusinessObjectFactory createFactory = new BusinessObjectFactory();

				JobVoyage voyage = createFactory.New<JobVoyage>();

				for (int i = 1; i < ports.Length; i++)
				{
					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = ports[i - 1];
					origin.JA_E_DEP = ZDateTime.Now.AddDays((i << 2) + 1);

					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = ports[i];
					destination.JB_E_ARV = ZDateTime.Now.AddDays((i << 2) + 3);
				}

				voyage.GenerateSailings();
				voyagePK = voyage.PK;

				createFactory.Save();
			}

			{
				JobVoyage voyage = Factory.Load<JobVoyage>(voyagePK);

				Factory.ResetDatabaseLoadCount();
				AssertNotNull("lazy load", voyage.Sailings);

				// JobSailing: 2 -- Z will not merge queries on JX_JA with queries on JX_JB
				// JobVoyDestination: 1
				// JobVoyOrigin: 1
				AssertMaxDbHits(4, Factory);
			}
		}

		public void TestFlightDateSetOnSaving()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertEquals("Precondition", ZDateTime.Empty, voyage.JV_FlightDate);

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = new ZDateTime(2008, 12, 3, 2, 46, 0);

			Factory.Save();
			AssertEquals("Only generated for Air/Road", ZDateTime.Empty, voyage.JV_FlightDate);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Factory.Save();
			AssertEquals("Generated for Air", origin.JA_E_DEP, voyage.JV_FlightDate);

			origin.JA_E_DEP = new ZDateTime(2009, 2, 24, 2, 46, 0);

			Factory.Save();
			AssertEquals("Updated on subsequent saves", origin.JA_E_DEP, voyage.JV_FlightDate);
		}

		public void TestDefaultCargoOnly()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertEquals(true, voyage.JV_IsCargoOnly);

			FreightDataRegistry.Instance.CargoOnlyVoyageDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			voyage = Factory.New<JobVoyage>();
			AssertEquals(false, voyage.JV_IsCargoOnly);
		}

		public void TestCloneShouldResistInterferianceFrom1Stop()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "BlahBlah";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Now.AddDays(3);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Now.AddDays(4);

			voyage.GenerateSailings();

			ZDateTime oldDate = FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived;
			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now.AddMinutes(-5);
			try
			{
				JobVoyage clone = (JobVoyage)voyage.Clone();

				AssertEquals("Expecting 1 origin", 1, clone.Origins.Count);
				AssertEquals("Expecting 1 destination", 1, clone.Destinations.Count);
				AssertEquals("Expecting 1 sailing", 1, clone.Sailings.Count);

				JobSailingCollection sailingCollection = new JobSailingCollection(Factory);
				sailingCollection.Load();
				AssertEquals("Should be only sailings from voyage and clone", voyage.Sailings.Count + clone.Sailings.Count, sailingCollection.Count);
				for (int i = 0; i < sailingCollection.Count - 1; i++)
				{
					for (int j = i + 1; j < sailingCollection.Count; j++)
					{
						AssertNotEquals("Should be different Origin PKs", sailingCollection[i].JX_JA, sailingCollection[j].JX_JA);
						AssertNotEquals("Should be different Destination PKs", sailingCollection[i].JX_JB, sailingCollection[j].JX_JB);
					}
				}
			}
			finally
			{
				FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = oldDate;
			}
		}

		public void TestEditLogRaisedOnJobVoyageOnSave()
		{
			Voyage.Factory.Save();
			AssertEquals("No edit log initially", null, Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobVoyage loadedVoyage = newFactory.Load<JobVoyage>(Voyage.PK);
			loadedVoyage.JV_FlightDate = ZDateTime.Now;
			newFactory.Save();
			AssertNotNull("Edit log raised on edit of JobSailing", loadedVoyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));
		}

		public void TestHasSelectedTradeLanes()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			JobVoyage voyage = Factory.New<JobVoyage>();
			JobTradeLaneVoyage tradeLaneVoyage = voyage.TradeLanes.AddNew();
			tradeLaneVoyage.NB_OH = principal.PK;

			Assert(!voyage.HasSelectedTradeLanes);

			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLaneVoyage.NB_EJ = tradeLane.PK;

			Assert(voyage.HasSelectedTradeLanes);
		}

		public void TestTradeLanes()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			jobTradeLaneVoyage.NB_EJ = tradeLane.PK;
			jobTradeLaneVoyage.NB_JV = voyage.PK;

			AssertCollectionContains(jobTradeLaneVoyage, voyage.TradeLanes);
		}

		public void TestSettingVesselWillDefaultAssociatedCarrier()
		{
			var vessel = RefVessel.LookupVesselByName("Gabriela2", Factory).First();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Vessel1.RV_OH = carrier.PK;

			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			AssertEquals("Expecting Vessel to be set to Vessel1", Vessel1, Voyage.Vessel);
			AssertEquals("Carrier should be defaulted from the vessel", carrier, Voyage.Line);

			var anotherVessel = Factory.New<RefVessel>();
			anotherVessel.RV_Name = "Visund";

			Voyage.JV_RV_NKVessel = anotherVessel.RV_FK;
			AssertEquals(anotherVessel, Voyage.Vessel);
			AssertEquals(null, Voyage.Line);
		}

		public void TestSeaVoyages_DefaultingVoyageType()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Visund";

			var newVoyage = Factory.New<JobVoyage>();
			newVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			newVoyage.JV_RV_NKVessel = vessel.RV_FK;
			AssertEquals("Not enough data to default voyage type", "", newVoyage.JV_VoyageType);

			newVoyage.JV_VoyageFlight = "123";
			AssertEquals("Voyage type defaulted to MAIN", Constants.VoyageType.MainVoyage, newVoyage.JV_VoyageType);

			var anotherNewVoyage = Factory.New<JobVoyage>();
			anotherNewVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			anotherNewVoyage.JV_RV_NKVessel = vessel.RV_FK;
			anotherNewVoyage.JV_VoyageFlight = "123";
			AssertEquals("Voyage type defaulted to SLOT", Constants.VoyageType.SlotVoyage, anotherNewVoyage.JV_VoyageType);

			newVoyage.Delete();

			anotherNewVoyage.TryToDefaultVoyageType(false);
			AssertEquals(Constants.VoyageType.SlotVoyage, anotherNewVoyage.JV_VoyageType);

			anotherNewVoyage.TryToDefaultVoyageType(true);
			AssertEquals(Constants.VoyageType.MainVoyage, anotherNewVoyage.JV_VoyageType);
		}

		public void TestIsMainVoyage()
		{
			Voyage.JV_VoyageType = "";
			Assert(!Voyage.IsMainVoyage);

			Voyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			Assert(Voyage.IsMainVoyage);

			Voyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			Assert(!Voyage.IsMainVoyage);
		}

		public void TestJV_RV_NKVessel_CallsScheduleDataVendor()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Voyage.JV_RV_NKVessel = vessel.RV_FK;
				AssertEquals("Expected SailingScheduleDataVendor.UpdateAllVoyageSailings to be called", true, MockSailingScheduleDataVendor.Instance.UpdateAllVoyageSailingsCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestJV_VoyageFlight_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Voyage.JV_VoyageFlight = "Voyage";
				AssertEquals("Expected SailingScheduleDataVendor.UpdateAllVoyageSailings to be called", true, MockSailingScheduleDataVendor.Instance.UpdateAllVoyageSailingsCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestJV_OH_Line_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Voyage.JV_OH_Line = ZGuid.NewZGuid();
				AssertEquals("Expected SailingScheduleDataVendor.UpdateAllVoyageSailings to be called", true, MockSailingScheduleDataVendor.Instance.UpdateAllVoyageSailingsCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestCurrentCountry()
		{
			AssertNotNull("CurrentBranch must not be null", GlbBranch.CurrentBranch);
			AssertNotNull("CurrentBranch.Country must not be null", GlbBranch.CurrentBranch.Country);

			string countryCode = GlbBranch.CurrentBranch.Country.RN_Code;

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageCountry country1 = voyage.CurrentCountry;
			AssertNotNull("We should have a VoyageCountry", country1);
			AssertEquals("The Country should not have changes", false, country1.HasChanges);
			AssertEquals("The Country should have the correct code", countryCode, country1.J0_RN_NKCountry);

			VoyageCountry country2 = voyage.Countries.GetCountry(countryCode, false);
			AssertEquals("Should already be in the collection.", country1.PK, country2.PK);
		}

		public void TestMessages()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertNotNull("Messages", voyage.Messages);
		}

		[MemoryTestRetryCount(2)]
		public void TestAllowDelayAlertsToBeSentOnSave()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection()
			{
				new DelayAlertDeliveryRule()
			});

			CommonConsol importConsol = new JobsForTesting(Factory).ImportConsol;
			CommonShipment importShipment = importConsol.Shipments.AddNew();
			importShipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			importShipment.Consignee.MainAddress.OA_Email = "someone@example.com";

			JobSailing sailing = SailingsHelper.Voyage.Sailings[0];
			var transport = importConsol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			sailing.Destination.JB_E_ARV = ZDateTime.Now;
			Factory.Save();

			var queryProvider = new Mock<IScheduleUpdateQueryProvider>();
			ScheduleUpdateQueryProviderFactory.Set(Factory, () => queryProvider.Object);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(true, false)).Returns(false);
			sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(1);
			Factory.Save();
			var printJob = Factory.LoadTop1<StmPrintJob>(DelayAlertStmPrintJobQuery);
			AssertNull("Delay alert not delivered when AllowDelayAlertsToBeSentOnSave=false", printJob);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(true, false)).Returns(true);
			sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(2);
			Factory.Save();
			printJob = Factory.LoadTop1<StmPrintJob>(DelayAlertStmPrintJobQuery);
			AssertNotNull("Delay alert sent when AllowDelayAlertsToBeSentOnSave=true", printJob);
			printJob.Delete();

			queryProvider.VerifyAll();
		}

		public void TestAllowDelayAlertsToBeSentOnSave_Export()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection()
			{
				new DelayAlertDeliveryRule()
			});

			CommonConsol exportConsol = new JobsForTesting(Factory).ExportConsol;
			CommonShipment exportShipment = exportConsol.Shipments.AddNew();
			exportShipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			exportShipment.Consignor.MainAddress.OA_Email = "someone@example.com";

			JobSailing sailing = SailingsHelper.SydLaxSailing;
			var transport = exportConsol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			Factory.Save();

			var queryProvider = new Mock<IScheduleUpdateQueryProvider>();
			ScheduleUpdateQueryProviderFactory.Set(Factory, () => queryProvider.Object);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(false, true)).Returns(false);
			sailing.Origin.JA_E_DEP = sailing.Origin.JA_E_DEP.AddDays(1);
			Factory.Save();
			var printJob = Factory.LoadTop1<StmPrintJob>(DelayAlertStmPrintJobQuery);
			AssertNull("Delay alert not delivered when AllowDelayAlertsToBeSentOnSave=false", printJob);
			queryProvider.Setup(m => m.ShouldSendDelayAlerts(false, true)).Returns(true);
			sailing.Origin.JA_E_DEP = sailing.Origin.JA_E_DEP.AddDays(2);
			Factory.Save();
			printJob = Factory.LoadTop1<StmPrintJob>(DelayAlertStmPrintJobQuery);
			AssertNotNull("Delay alert sent when AllowDelayAlertsToBeSentOnSave=true", printJob);
			printJob.Delete();

			queryProvider.VerifyAll();
		}

		public void TestHasCountryAllocations()
		{
			VoyageCountry aU = Voyage.Countries.GetCountry("AU", true);

			aU.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AssertEquals(false, Voyage.HasCountryAllocations);

			aU.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			AssertEquals(false, Voyage.HasCountryAllocations);

			aU.J0_AllocationMethod = AllocationMethodList.Codes.Origin;
			AssertEquals(false, Voyage.HasCountryAllocations);

			aU.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			AssertEquals(true, Voyage.HasCountryAllocations);

			VoyageCountry nZ = Voyage.Countries.GetCountry("NZ", true);

			nZ.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			AssertEquals(true, Voyage.HasCountryAllocations);

			aU.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AssertEquals(true, Voyage.HasCountryAllocations);

			nZ.J0_AllocationMethod = AllocationMethodList.Codes.NotSet;
			AssertEquals(false, Voyage.HasCountryAllocations);
		}

		public void TestTemplateCopy()
		{
			JobVoyage voyage = GetPopulatedVoyage();

			JobVoyage copiedVoyage = (JobVoyage)voyage.TemplateCopy();

			AssertEquals("Origin Count", voyage.Origins.Count, copiedVoyage.Origins.Count);
			AssertEquals("Destinations Count", voyage.Destinations.Count, copiedVoyage.Destinations.Count);
			AssertEquals("Sailings Count", voyage.Sailings.Count, copiedVoyage.Sailings.Count);
			AssertEquals("Countrys should not be copyed accross", 0, copiedVoyage.Countries.Count);

			AssertEquals("Voyage Reference Cleared", ZString.Empty, copiedVoyage.JV_VoyageFlight);

			AssertEquals("Dates Cleared", ZDateTime.Empty, copiedVoyage.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("Destination Dates Cleared", ZDateTime.Empty, copiedVoyage.Destinations[0].JB_E_ARV);

			AssertEquals("Allocations Cleared", 0, copiedVoyage.Origins[0].SlotAllocations.Count);
		}

		public void TestClone()
		{
			JobVoyage voyage = GetPopulatedVoyage();

			JobVoyage copiedVoyage = (JobVoyage)voyage.Clone();

			AssertEquals("Origin Count", voyage.Origins.Count, copiedVoyage.Origins.Count);

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				VoyageOrigin copiedOrigin = copiedVoyage.Origins.GetOriginFromLoading(origin.JA_RL_NKPortOfLoading);
				AssertEquals("Origin Allocation Count (" + origin.JA_RL_NKPortOfLoading + ")", origin.SlotAllocations.Count, copiedOrigin.SlotAllocations.Count);
			}

			AssertEquals("Destinations Count", voyage.Destinations.Count, copiedVoyage.Destinations.Count);
			AssertEquals("Sailings Count", voyage.Sailings.Count, copiedVoyage.Sailings.Count);

			foreach (JobSailing sailing in voyage.Sailings)
			{
				JobSailing copiedSailing = copiedVoyage.Sailings.GetSailingFromLoadAndDischarge(sailing.JX_JA_RL_NKPortOfLoading, sailing.JX_JB_RL_NKPortOfDischarge);
				AssertEquals("Destination Allocation Count (" + sailing.JX_JA_RL_NKPortOfLoading + "-" + sailing.JX_JB_RL_NKPortOfDischarge + ")", sailing.SlotAllocations.Count, copiedSailing.SlotAllocations.Count);
			}

			AssertEquals("Countrys Count", voyage.Countries.Count, copiedVoyage.Countries.Count);

			foreach (VoyageCountry country in voyage.Countries)
			{
				VoyageCountry copiedCountry = voyage.Countries.GetCountry(country.J0_RN_NKCountry, false);
				AssertEquals("Country Allocation Count (" + country.J0_RN_NKCountry + ")", country.SlotAllocations.Count, copiedCountry.SlotAllocations.Count);
			}
		}

		public void TestCloneFromDifferentFactory()
		{
			var voyage = GetPopulatedVoyage();

			var factory2 = new BusinessObjectFactory();
			var cloneArgs = new BusinessObjectCloneArgs(factory2, Enumerable.Empty<string>(), null, true);
			var copiedVoyage = (JobVoyage)voyage.Clone(cloneArgs);

			AssertEquals(factory2, copiedVoyage.Factory);

			foreach (var collection in new BusinessObjectCollection[]
			{
				copiedVoyage.Countries,
				copiedVoyage.Origins,
				copiedVoyage.Destinations,
				copiedVoyage.Sailings
			})
			{
				foreach (var businessObject in collection)
				{
					AssertEquals(factory2, businessObject.Factory);
				}
			}
		}

		public void TestHumanReadableName()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ves";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voy";

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Flight schedule", "Flight Schedule (Flight='voy')", voyage.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertEquals("Sailing schedule", "Sailing Schedule (Vessel='ves', Voyage='voy', Carrier='')", voyage.HumanReadableName);

			voyage.JV_OH_Line = ZGuid.NewZGuid();
			AssertEquals("Sailing schedule", "Sailing Schedule (Vessel='ves', Voyage='voy', Carrier='')", voyage.HumanReadableName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CARRIER";
			voyage.JV_OH_Line = org.PK;
			AssertEquals("Sailing schedule", "Sailing Schedule (Vessel='ves', Voyage='voy', Carrier='CARRIER')", voyage.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Trucking journey", "Trucking Journey (Truck='voy')", voyage.HumanReadableName);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Rail journey", "Rail Journey (Journey='voy')", voyage.HumanReadableName);

			voyage.JV_AirSeaRoad = "xxx";
			AssertEquals("Default for when no transport mode", "Schedule", voyage.HumanReadableName);
		}

		public void TestSailingText()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertEquals("Expecting Sailing Text:", "Sailing", voyage.SailingText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Expecting Sailing Text:", "Flight leg", voyage.SailingText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Expecting Sailing Text:", "Journey leg", voyage.SailingText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Expecting Sailing Text:", "Journey leg", voyage.SailingText);
		}

		public void TestVoyageText()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertEquals("Expecting Voyage Text:", "Voyage", voyage.VoyageText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Expecting Voyage Text:", "Flight", voyage.VoyageText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Expecting Voyage Text:", "Journey", voyage.VoyageText);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Expecting Voyage Text:", "Journey", voyage.VoyageText);
		}

		public void TestSailingTextLowerCase()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AssertEquals("Expecting Sailing Text:", "sailing", voyage.SailingTextLowerCase);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Expecting Sailing Text:", "flight leg", voyage.SailingTextLowerCase);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Expecting Sailing Text:", "journey leg", voyage.SailingTextLowerCase);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Expecting Sailing Text:", "journey leg", voyage.SailingTextLowerCase);
		}

		/// <summary>
		/// Test Saving forces Origin, Destination, Sailing to be created before voyage can be saved.
		/// </summary>
		public void TestRunPreSaveValidation()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var vessel = RefVessel.LookupVesselByName("APL EMERALD", Factory).First();

			JobVoyage newVoyage = Factory.New<JobVoyage>();
			newVoyage.JV_VoyageFlight = "100";
			newVoyage.JV_RV_NKVessel = vessel.RV_FK;
			newVoyage.JV_OH_Line = carrier.PK;
			newVoyage.RunPreSaveValidation();

			Assert("Expecting voyage to have errors as it does not have at least one origin, destination and sailing.", newVoyage.HasErrors);

			VoyageOrigin newOrigin = newVoyage.Origins[0];
			newOrigin.JA_E_DEP = ZDateTime.Today.AddDays(3);
			newOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			newVoyage.RunPreSaveValidation();

			Assert("Expecting voyage to have errors as it does not have at least one destination and sailing.", newVoyage.HasErrors);

			VoyageDestination newDestination = newVoyage.Destinations[0];
			newDestination.JB_E_ARV = ZDateTime.Today;
			newDestination.JB_RL_NKPortOfDischarge = "USLAX";
			newVoyage.RunPreSaveValidation();

			Assert("Expecting voyage to have errors as it does not have at least one sailing.", newVoyage.HasErrors);

			newDestination.JB_E_ARV = ZDateTime.Today.AddDays(7);
			newVoyage.RunPreSaveValidation();

			Assert("Not expecting voyage to have errors as it has a sailing.", !newVoyage.HasErrors);
		}

		public void TestGenerateSailings()
		{
			JobVoyage voyage = GetPopulatedVoyage();

			AssertEquals("Expected 9 sailing schedules generated", 9, voyage.Sailings.Count);

			voyage.Origins.RemoveAndDelete(origin1);
			AssertEquals("Expected 6 sailing schedules generated", 6, voyage.Sailings.Count);

			voyage.Destinations.RemoveAndDelete(destination1);
			AssertEquals("Expected 4 sailing schedules generated", 4, voyage.Sailings.Count);

			VoyageOrigin o4 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			voyage.Origins.Add(o4); // will call GenerateSailings but nothing will be generated because LOCO is empty
									// can't add loco before because of PropertyIsUniqueInCollection validation...
									// catch 22!
			voyage.GenerateSailings();
			AssertEquals("Expected 6 sailing schedules generated", 6, voyage.Sailings.Count);

			AssertEquals(6, ((INeedDataSet)Factory).Data.Tables[JobSailing.Schema.TableName].Rows.Count);
		}

		[TestDate(2013, 05, 05)]
		public void TestGenerateSailings_Dates_Sea()
		{
			JobVoyage newVoyage = Factory.New<JobVoyage>();
			VoyageOrigin newOrigin = Factory.New<VoyageOrigin>();
			VoyageDestination newDestination = Factory.New<VoyageDestination>();

			newVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			newOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			newOrigin.JA_E_DEP = ZDateTime.Today;
			newDestination.JB_RL_NKPortOfDischarge = "USLAX";
			newDestination.JB_E_ARV = ZDateTime.Today;

			newVoyage.Origins.Add(newOrigin);
			newVoyage.Destinations.Add(newDestination);
			newVoyage.GenerateSailings();

			Assert("Expecting 1 Sailing to be generated.", newVoyage.Sailings.Count == 1);

			newOrigin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			newVoyage.GenerateSailings();

			Assert("Expecting 0 Sailings to be generated.", newVoyage.Sailings.Count == 0);
		}

		public void TestGenerateSailings_Dates_Air()
		{
			JobVoyage newVoyage = Factory.New<JobVoyage>();
			VoyageOrigin newOrigin = Factory.New<VoyageOrigin>();
			VoyageDestination newDestination = Factory.New<VoyageDestination>();

			ZDateTime time = ZDateTime.Now;

			newVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			newOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			newOrigin.JA_E_DEP = time;
			newDestination.JB_RL_NKPortOfDischarge = "USLAX";
			newDestination.JB_E_ARV = time;

			newVoyage.Origins.Add(newOrigin);
			newVoyage.Destinations.Add(newDestination);
			newVoyage.GenerateSailings();

			Assert("Expecting 1 Sailing to be generated.", newVoyage.Sailings.Count == 1);

			newOrigin.JA_E_DEP = time.AddDays(1).AddMinutes(1);
			newVoyage.GenerateSailings();

			Assert("Expecting 0 Sailings to be generated.", newVoyage.Sailings.Count == 0);

			newOrigin.JA_E_DEP = time;
			newDestination.JB_E_ARV = time.AddDays(90);
			newVoyage.GenerateSailings();

			Assert("Expecting 1 Sailing to be generated.", newVoyage.Sailings.Count == 1);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestGenerateSailings_OnlineScheduleStatus()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_VoyageFlight = "QF69";
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDateTime.Now;

				var destination = Factory.New<VoyageDestination>();
				destination.JB_RL_NKPortOfDischarge = "USLAX";
				destination.JB_E_ARV = ZDateTime.Now;

				voyage.Origins.Add(origin);
				voyage.Destinations.Add(destination);
				voyage.GenerateSailings();

				AssertEquals("Pre: Expected 1 Sailing to be generated.", 1, voyage.Sailings.Count);
				AssertEquals("Expected Online Flight Matching to be run on the generated Sailing.", Constants.FlightScheduleStatus.Matched, voyage.Sailings[0].JX_OnlineScheduleStatus);
			});
		}

		public void TestGenerateSailingsWithDupliatedPorts()
		{
			JobVoyage newVoyage = Factory.New<JobVoyage>();

			VoyageOrigin origin1 = newVoyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			VoyageOrigin origin2 = newVoyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageOrigin origin3 = newVoyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageOrigin origin4 = newVoyage.Origins.AddNew();
			origin4.JA_RL_NKPortOfLoading = "SGSIN";

			VoyageDestination dest1 = newVoyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "AUSYD";
			VoyageDestination dest2 = newVoyage.Destinations.AddNew();
			dest2.JB_RL_NKPortOfDischarge = "AUBNE";
			VoyageDestination dest3 = newVoyage.Destinations.AddNew();
			dest3.JB_RL_NKPortOfDischarge = "SGSIN";
			VoyageDestination dest4 = newVoyage.Destinations.AddNew();
			dest4.JB_RL_NKPortOfDischarge = "HKHKG";

			origin1.Validation.ValidateJA_E_DEP();
			Assert("Expecting Origin1 ETD to have errors.", origin1.JA_E_DEPInfo.HasErrors());
			origin2.Validation.ValidateJA_E_DEP();
			Assert("Expecting Origin2 ETD to have errors.", origin2.JA_E_DEPInfo.HasErrors());
			origin3.Validation.ValidateJA_E_DEP();
			Assert("Expecting Origin3 ETD to have errors.", origin3.JA_E_DEPInfo.HasErrors());
			origin4.Validation.ValidateJA_E_DEP();
			Assert("Expecting Origin4 ETD to have errors.", origin4.JA_E_DEPInfo.HasErrors());

			dest1.Validation.ValidateJB_E_ARV();
			Assert("Expecting Dest1 ETA to have errors.", dest1.JB_E_ARVInfo.HasErrors());
			dest2.Validation.ValidateJB_E_ARV();
			Assert("Expecting Dest2 ETA to have errors.", dest2.JB_E_ARVInfo.HasErrors());
			dest3.Validation.ValidateJB_E_ARV();
			Assert("Not expecting Dest3 ETA to have errors.", !dest3.JB_E_ARVInfo.HasErrors());
			dest4.Validation.ValidateJB_E_ARV();
			Assert("Not expecting Dest4 ETA to have errors.", !dest4.JB_E_ARVInfo.HasErrors());

			origin1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(13);
			origin3.JA_E_DEP = ZDateTime.Today.AddDays(17);
			origin4.JA_E_DEP = ZDateTime.Today.AddDays(36);

			dest1.JB_E_ARV = ZDateTime.Today.AddDays(11);
			dest2.JB_E_ARV = ZDateTime.Today.AddDays(15);

			newVoyage.GenerateSailings();

			AssertEquals("Expecting 10 sailings to be generated.", 10, newVoyage.Sailings.Count);
		}

		public void TestGenerateSailings_MultipleSailingsHaveSameOriginAndDestination()
		{
			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Now.AddDays(1);
			origin.JA_A_DEP = ZDateTime.Now.AddDays(2);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Now.AddDays(5);
			destination.JB_A_ARV = ZDateTime.Now.AddDays(6);

			AssertEquals("Pre-condition", 1, voyage.Sailings.Count);

			var duplicateSailing = voyage.Sailings.AddNew();
			duplicateSailing.JX_JA = origin.PK;
			duplicateSailing.JX_JB = destination.PK;

			AssertEquals("Duplicate sailing has been added", 2, voyage.Sailings.Count);

			voyage.GenerateSailings();

			AssertEquals("Calling GenerateSailings removes the duplicate", 1, voyage.Sailings.Count);
		}

		public void TestGenerateSailingsWithAutoGenerated()
		{
			JobVoyage voyage2 = Factory.New<JobVoyage>();

			VoyageOrigin origin1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_AutoCreated = true;
			VoyageDestination destination1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			destination1.JB_RL_NKPortOfDischarge = "USLAX";
			destination1.JB_AutoCreated = true;

			voyage2.Origins.Add(origin1);
			voyage2.Destinations.Add(destination1);

			voyage2.GenerateSailings();

			AssertEquals("Expecting 1 sailing to have been created.", 1, voyage2.Sailings.Count);

			VoyageOrigin origin2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			origin2.JA_RL_NKPortOfLoading = "AUBNE";

			voyage2.Origins.Add(origin2);

			voyage2.GenerateSailings();

			AssertEquals("Expecting 2 sailings to have been created.", 2, voyage2.Sailings.Count);

			VoyageDestination destination2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			destination2.JB_RL_NKPortOfDischarge = "USSFO";

			voyage2.Destinations.Add(destination2);

			voyage2.GenerateSailings();

			AssertEquals("Expecting 4 sailings to have been created.", 4, voyage2.Sailings.Count);
		}

		public void TestVoyageParentConsol()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2004, 04, 14);
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2004, 04, 16);
			voyage.GenerateSailings();
			Assert("Sailings.Count > 0", voyage.Sailings.Count > 0);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;

			JobVoyage voyageForTest = consol.Voyage;
			AssertEquals(consol.PK, voyageForTest.ParentConsol.PK);
		}

		public void TestRetreiveSailings()
		{
			// find existing number of sailings in DB to avoid failures on individual people's machines if they have data
			JobSailingCollection collection = new JobSailingCollection(new BusinessObjectFactory());
			collection.Load();
			int existingSailingsCount = collection.Count;

			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "GBLON") as RefUNLOCO;

			Voyage.Origins.Add(o1);
			Voyage.Origins.Add(o2);
			Voyage.Destinations.Add(d1);

			AssertEquals(2, Voyage.Origins.Count);
			AssertEquals(1, Voyage.Destinations.Count);

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			d1.JB_RL_NKPortOfDischarge = r3.RL_Code;

			Voyage.GenerateSailings();
			AssertEquals("Expected 2 sailing schedules generated", 2, Voyage.Sailings.Count);
			AssertEquals(2, Voyage.Origins.Count);
			AssertEquals(1, Voyage.Destinations.Count);

			ZGuid sailing1PK = Voyage.Sailings[0].PK;
			ZGuid sailing2PK = Voyage.Sailings[1].PK;

			JobSailingCollection sailingCollection = new JobSailingCollection(new BusinessObjectFactory());
			sailingCollection.Load();
			AssertEquals("Sailings should not have been saved yet; Expecting the same number of sailings that are already in the database.", existingSailingsCount, sailingCollection.Count);

			AssertEquals(2, ((INeedDataSet)Factory).Data.Tables[JobSailing.Schema.TableName].Rows.Count);
			AssertEquals(1, ((INeedDataSet)Factory).Data.Tables[JobVoyage.Schema.TableName].Rows.Count);
			AssertEquals(2, ((INeedDataSet)Factory).Data.Tables[VoyageOrigin.Schema.TableName].Rows.Count);
			AssertEquals(1, ((INeedDataSet)Factory).Data.Tables[VoyageDestination.Schema.TableName].Rows.Count);

			Factory.Save();

			AssertEquals(2, ((INeedDataSet)Factory).Data.Tables[JobSailing.Schema.TableName].Rows.Count);
			AssertEquals(1, ((INeedDataSet)Factory).Data.Tables[JobVoyage.Schema.TableName].Rows.Count);
			AssertEquals(2, ((INeedDataSet)Factory).Data.Tables[VoyageOrigin.Schema.TableName].Rows.Count);
			AssertEquals(1, ((INeedDataSet)Factory).Data.Tables[VoyageDestination.Schema.TableName].Rows.Count);

			sailingCollection = new JobSailingCollection(new BusinessObjectFactory());
			sailingCollection.Load();
			AssertEquals("Sailings saved; expecting two more than were originally in DB", existingSailingsCount + 2, sailingCollection.Count);

			AssertEquals(2, ((INeedDataSet)Factory).Data.Tables[JobSailing.Schema.TableName].Rows.Count);

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobVoyage retreivedVoyage = secondFactory.Load(typeof(JobVoyage), Voyage.PK) as JobVoyage;

			AssertEquals(2, Voyage.Origins.Count);
			AssertEquals(1, Voyage.Destinations.Count);

			AssertEquals("Expected 2 sailing schedules retreived", 2, retreivedVoyage.Sailings.Count);
			Assert("Sailing 1 retrieved from factory unsuccessfully", retreivedVoyage.Sailings.Contains(sailing1PK));
			Assert("Sailing 2 retrieved from factory unsuccessfully", retreivedVoyage.Sailings.Contains(sailing2PK));
		}

		public void TestUpdateProxiedFieldsOnLinkedConsolTransportWhenChangesNotYetInDB()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort2;
			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings[0];
			var sailing2 = voyage.Sailings[1];

			var transport1_1 = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport1_1.JW_IsLinked = true;
			transport1_1.JW_JX = sailing1.PK;

			var transport2_1 = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport2_1.JW_IsLinked = true;
			transport2_1.JW_JX = sailing2.PK;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Flight";
			voyage.JV_IsChartered = true;
			voyage.JV_IsCargoOnly = true;

			Assert("Precondition: transport 1 is not in db", !transport1_1.IsInDatabase);
			Assert("Precondition: transport 2 is not in db", !transport2_1.IsInDatabase);
			Assert("Precondition: voyage is not in db", !voyage.IsInDatabase);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transport1_2 = newFactory.Load<Transport>(transport1_1.PK);
			var transport2_2 = newFactory.Load<Transport>(transport2_1.PK);
			var transports = new Transport[] { transport1_2, transport2_2 };

			var proxiedTransportFieldsToVoyageFieldsDict = new Dictionary<ZString, ZPropertyInfo>()
			{
				{ JobConsolTransportSchema.Constants.JW_Vessel, voyage.JV_RV_NKVesselInfo },
				{ JobConsolTransportSchema.Constants.JW_VoyageFlight, voyage.JV_VoyageFlightInfo },
				{ JobConsolTransportSchema.Constants.JW_IsCharter, voyage.JV_IsCharteredInfo },
				{ JobConsolTransportSchema.Constants.JW_IsCargoOnly, voyage.JV_IsCargoOnlyInfo }
			};

			transports.ForEach(transport => proxiedTransportFieldsToVoyageFieldsDict.ForEach(pair =>
				AssertEquals(pair.Value.Name + " should propogate to linked transport when data not in db.", pair.Value.Value, ((ZPropertyInfo)transport[pair.Key + "Info"]).OriginalValue)));
		}

		public void TestCopySailingsDateDataToSimilarSailings()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();

			VoyageOrigin o1 = voyage1.Origins.AddNew();
			o1.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageOrigin o2 = voyage1.Origins.AddNew();
			o2.JA_RL_NKPortOfLoading = "AUMEL";

			VoyageOrigin o3 = voyage1.Origins.AddNew();
			o3.JA_RL_NKPortOfLoading = "AUBNE";

			VoyageDestination d1 = voyage1.Destinations.AddNew();
			d1.JB_RL_NKPortOfDischarge = "USLAX";

			voyage1.GenerateSailings();

			// just getting a sailing to look at
			JobSailing sydLaxSailing = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			JobSailing bneLaxSailing = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "USLAX");
			AssertNotNull("Not expecting SydLaxSailing to be null", sydLaxSailing);
			AssertNotNull("Not expecting BneLaxSailing to be null", bneLaxSailing);

			// Set the syd lax dates
			sydLaxSailing.JX_DepotCutOff = ZDateTime.Today.AddDays(5);
			sydLaxSailing.JX_DepotReceivalCommences = ZDateTime.Today;
			sydLaxSailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(6);
			sydLaxSailing.Origin.JA_ReceivalCommences = ZDateTime.Today;
			Assert("Expecting bnelax LCL Rec date to be empty", bneLaxSailing.JX_DepotReceivalCommences.IsEmpty);

			VoyageDestination d2 = voyage1.Destinations.AddNew();
			d2.JB_RL_NKPortOfDischarge = "USSEA";

			JobSailing sydSeaSailing = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USSEA");
			Assert("Not expecting SydSeaSailing to be null", sydSeaSailing != null);

			// check that SydSea has the same dates as sydlax
			AssertEquals("Expecting SydSea LCL Rec date to have be set to now", ZDateTime.Today, sydSeaSailing.JX_DepotReceivalCommences);
			AssertEquals("Expecting SydSea LCL end date to have be set to now + 5 days", ZDateTime.Today.AddDays(5), sydSeaSailing.JX_DepotCutOff);

			// change some of these dates to make sure a new sailing will get the correct ones

			sydSeaSailing.JX_DepotReceivalCommences = ZDateTime.Today.AddDays(1);
			sydSeaSailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(4);

			VoyageDestination d3 = voyage1.Destinations.AddNew();
			d3.JB_RL_NKPortOfDischarge = "USSAN";

			JobSailing sydSanSailing = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USSAN");
			Assert("Not expecting SydSanSailing to be null", sydSanSailing != null);

			AssertEquals("Expecting SydSan LCL Rec date to have be set to tomorrow", ZDateTime.Today.AddDays(1), sydSanSailing.JX_DepotReceivalCommences);
			AssertEquals("Expecting SydSan LCL end date to have be set to now + 5 days", ZDateTime.Today.AddDays(5), sydSanSailing.JX_DepotCutOff);

			bneLaxSailing.JX_IsPublished = true;
			bneLaxSailing.JX_DepotAvailabilityDate = ZDateTime.Today.AddDays(30);
			AssertEquals("Expecting SydLax LCL Avail Date to be set to today + 30 days", ZDateTime.Today.AddDays(30), sydLaxSailing.JX_DepotAvailabilityDate);
		}

		public void TestReasonForSailingNotGenerated()
		{
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "NZAKL";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.Origins.Add(origin);
			voyage.Destinations.Add(destination);

			var reasonForNotGenerated = voyage.GenerateSailings();
			AssertEquals("The Load (NZAKL) and Discharge (NZAKL) cannot be the same for SEA voyage.", reasonForNotGenerated);
			AssertEquals("Should have no sailings", 0, voyage.Sailings.Count);

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Now.AddDays(2);
			destination.JB_E_ARV = ZDateTime.Now.AddDays(1);

			reasonForNotGenerated = voyage.GenerateSailings();
			AssertEquals($"The date combination is invalid for SEA voyage from Origin (AUSYD {origin.JA_E_DEP}) to Destination (NZAKL {destination.JB_E_ARV}).", reasonForNotGenerated);
			AssertEquals("Should have no sailings", 0, voyage.Sailings.Count);

			destination.JB_E_ARV = ZDateTime.Now.AddDays(3);

			reasonForNotGenerated = voyage.GenerateSailings();
			AssertEquals("Should be Empty", ZString.Empty, reasonForNotGenerated);
			AssertEquals("Should generate 1 sailing", 1, voyage.Sailings.Count);

			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_RL_NKPortOfLoading = "NZAKL";
			sailing.Destination.JB_RL_NKPortOfDischarge = "NZAKL";

			reasonForNotGenerated = voyage.GenerateSailings();
			AssertEquals($"The existing sailing is invalid for SEA voyage from Origin (NZAKL {origin.JA_E_DEP}) to Destination (NZAKL {destination.JB_E_ARV}).", reasonForNotGenerated);
			AssertEquals("Should have no sailings", 0, voyage.Sailings.Count);
		}

		public void TestJV_AircraftTypeForBinding_MaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(JobVoyage), nameof(JobVoyage.JV_AircraftTypeForBinding), false, a => a.MaxLength == 3);
		}

		public void TestSourcePK()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			AssertEquals("Should be PK", voyage.PK, ((IExchangeRateSource)voyage).SourcePK);
		}

		public void TestSourceController()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			IExchangeRateSource rateSource = voyage;

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Should be Air Controller", ControllerIDs.JobAirSailing, rateSource.SourceController);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Should be Rail Controller", ControllerIDs.JobRailSailing, rateSource.SourceController);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Should be Road Controller", ControllerIDs.JobRoadSailing, rateSource.SourceController);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be Sea Controller", ControllerIDs.JobSeaVoyage, rateSource.SourceController);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Other;
			AssertEquals("Should be Sea Controller", ControllerIDs.JobSeaVoyage, rateSource.SourceController);
		}

		public void TestDescription()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			IExchangeRateSource rateSource = voyage;

			string expected = voyage.JV_RV_NKVessel + "/" + voyage.JV_VoyageFlight;

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be Description", expected, rateSource.Description);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			AssertEquals("Should be Description", expected, rateSource.Description);

			expected = voyage.JV_VoyageFlight;

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals("Should be Description", expected, rateSource.Description);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			AssertEquals("Should be Description", expected, rateSource.Description);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Other;
			AssertEquals("Should be Empty", ZString.Empty, rateSource.Description);
		}

		public void TestGetEnumerator()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();

			List<IExchangeRate> expected = new List<IExchangeRate>();

			VoyageExRate rate = Factory.New<VoyageExRate>();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 1.54m;
			voyage.ExRates.Add(rate);
			expected.Add(rate);

			rate = Factory.New<VoyageExRate>();
			rate.E8_RX_NKExCurrency = "UAH";
			rate.E8_VoyageExchangeRate = 0.2m;
			voyage.ExRates.Add(rate);
			expected.Add(rate);

			AssertNotNull("Should be Enumerator", ((IExchangeRateSource)voyage).GetEnumerator());
			AssertContainsExactElementsInAnyOrder(expected, voyage);
		}

		public void TestGetExchangeRate()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var exRateSource = (IExchangeRateSource)voyage;

			var rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 1.22m;
			rate.E8_RL_NKPort = "ITMIL";

			rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 1.11m;

			rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "SGD";
			rate.E8_VoyageExchangeRate = 1.66m;

			rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "SGD";
			rate.E8_VoyageExchangeRate = 1.77m;
			rate.E8_RL_NKPort = "SGSIN";

			rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "AUD";
			rate.E8_VoyageExchangeRate = 1.55m;
			rate.E8_RL_NKPort = "ITROM";

			rate = voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "CAD";
			rate.E8_VoyageExchangeRate = 1.55m;

			AssertEquals(1.11m, exRateSource.GetExchangeRate("USD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals(1.66m, exRateSource.GetExchangeRate("SGD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull(exRateSource.GetExchangeRate("AUD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals(1.55m, exRateSource.GetExchangeRate("CAD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull(exRateSource.GetExchangeRate("CNY", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
		}

		public void TestPiggybackedValidationExists()
		{
			Factory.Save();

			var provider = new Mock<IScheduleValidationProvider>();

			using (ObjectFactory.Substitute(ScheduleValidationProviderFactoryTest.ObjectName, new ArrayList(new IScheduleValidationProvider[] { provider.Object })))
			{
				JobVoyage loadedVoyage = new BusinessObjectFactory().Load<JobVoyage>(Voyage.PK);
				DummyVoyageValidation dummyValidation = new DummyVoyageValidation(loadedVoyage);

				provider.Setup(m => m.GetExtraVoyageValidation(loadedVoyage)).Returns(dummyValidation);
				JobVoyageValidation validation = loadedVoyage.Validation;

				AssertEquals("Should have added the piggy back validation from the validation provider.", true, validation.ContainsPiggybackedValidation(typeof(DummyVoyageValidation)));
			}
		}

		public void TestPiggybackedValidationNotExists()
		{
			Factory.Save();

			var provider = new Mock<IScheduleValidationProvider>();

			using (ObjectFactory.Substitute(ScheduleValidationProviderFactoryTest.ObjectName, new ArrayList(new[] { provider.Object })))
			{
				var loadedVoyage = new BusinessObjectFactory().Load<JobVoyage>(Voyage.PK);
				var dummyValidation = new DummyVoyageValidation(loadedVoyage);

				provider.Setup(m => m.GetExtraVoyageValidation(loadedVoyage)).Returns((JobVoyageValidation)null);
				AssertNoExceptionThrown("Should not throw exception", delegate
				{
					JobVoyageValidation validation = loadedVoyage.Validation;
				});
			}
		}

		public void TestOnLoaded_IsSeaAndNoCarrier_WillMarkAsNeedingValidation()
		{
			var creationFactory = new BusinessObjectFactory();
			var voyage = creationFactory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "123";

			creationFactory.Save();

			var voyageReloaded = Factory.Load<JobVoyage>(voyage.PK);
			voyageReloaded.JV_VoyageFlight = "456";

			voyageReloaded.RunPreSaveValidation();
			AssertHasErrors("Validation was enforced on carrier", voyageReloaded.JV_OH_LineInfo);
		}

		public void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var today = ZDateTime.Today;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins[0].JA_E_DEP = today.AddDays(1);
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations[0].JB_E_ARV = today.AddDays(2);
			voyage.GenerateSailings();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var container = consol.Containers.AddNew();
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP")).PK;
			container.JC_JX = voyage.Sailings[0].PK;
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;

			container.JC_GrossWeight = 3625.237m;
			container.JC_TareWeight = 47.236m;
			container.JC_DunnageWeight = 162.463m;
			container.JC_TotalHeight = 5.23m;
			container.JC_TotalLength = 4.34m;
			container.JC_TotalWidth = 6.18m;

			voyage.Sailings[0].Containers.Load();

			AssertEquals(1554.936m, container.JC_GrossWeight);
			AssertEquals(1507.700m, container.JC_Calc_NetWeight);
			AssertEquals(47.236m, container.JC_TareWeight);
			AssertEquals(162.463m, container.JC_DunnageWeight);
			AssertEquals(209.699m, container.JC_Calc_ActualGrossWeightInKgs);
			AssertEquals(33.200m, container.JC_Calc_ContainerCapacity);
			AssertEquals(3.972m, container.JC_Calc_ActualCapacity);
			AssertEquals(24000m, container.JC_Calc_MaxGrossWeight);
			AssertEquals(2280.000m, container.JC_Calc_TareWeight);

			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalWeight);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalWeightInKgs);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalVolume);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalVolumeInM3);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.GoodsWeightForBinding);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(1554.95m, container.JC_GrossWeight);
			AssertEquals(1507.71m, container.JC_Calc_NetWeight);
			AssertEquals(47.24m, container.JC_TareWeight);
			AssertEquals(162.47m, container.JC_DunnageWeight);
			AssertEquals(209.71m, container.JC_Calc_ActualGrossWeightInKgs);
			AssertEquals(3.97m, container.JC_Calc_ActualCapacity);
			AssertEquals(33.20m, container.JC_Calc_ContainerCapacity);
			AssertEquals(24000.00m, container.JC_Calc_MaxGrossWeight);
			AssertEquals(2280.00m, container.JC_Calc_TareWeight);

			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalWeight);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalWeightInKgs);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalVolume);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.JC_Calc_TotalVolumeInM3);
			AssertEquals("Empty value, as there are no packlines.", 0m, container.GoodsWeightForBinding);
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = voyage.Sailings[0].PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.JS_JX = voyage.Sailings[0].PK;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = voyage.Sailings[0].PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = voyage.Sailings[0].PK;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.JS_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.JS_JX = voyage.Sailings[0].PK;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			Factory.Save();

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "GABRIELA";
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				Factory.Save();

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				voyage.JV_VoyageFlight = "1234";
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				voyage.JV_FlightDate = new ZDateTime(2023, 09, 01);
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(3, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(3, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent_NonSea()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = voyage.Sailings[0].PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.JS_JX = voyage.Sailings[0].PK;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = voyage.Sailings[0].PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = voyage.Sailings[0].PK;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.JS_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.JS_JX = voyage.Sailings[0].PK;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			Factory.Save();

			Assert(voyage.IsAir);
			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
				Factory.Save();

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "GABRIELA";
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				voyage.JV_VoyageFlight = "1234";
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				voyage.JV_FlightDate = new ZDateTime(2023, 09, 01);
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		#region RequiresSailingGeneration

		public void TestRequiresSailingGeneration()
		{
			var voyage = Factory.New<JobVoyage>();
			Assert("RequiresSailingGeneration default value", !voyage.RequiresSailingGeneration);

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "1234";

			var origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = ZDateTime.Today;
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);
			voyage.RequiresSailingGeneration = false;

			origin.JA_RL_NKPortOfLoading = "AUBNE";
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);
			voyage.RequiresSailingGeneration = false;

			var dest = voyage.Destinations.AddNew();
			dest.JB_E_ARV = ZDateTime.Today.AddDays(1);
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);
			voyage.RequiresSailingGeneration = false;

			dest.JB_RL_NKPortOfDischarge = "HKHKG";
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Factory.Save();
			}
			Assert("RequiresSailingGeneration should be reset to false.", !voyage.RequiresSailingGeneration);
		}

		public void TestGenerateSailingWhenRequiresSailingGenerationIsSet()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "1234";

			var origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = ZDateTime.Today;
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightDataRegistry.Instance.EnableSailingGenerationOnServiceTaskSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
			}
			AssertEquals("No sailing was created.", 0, voyage.Sailings.Count);
			Assert("RequiresSailingGeneration should be reset to false.", !voyage.RequiresSailingGeneration);

			var dest = voyage.Destinations.AddNew();
			dest.JB_E_ARV = ZDateTime.Today.AddDays(1);
			dest.JB_RL_NKPortOfDischarge = "HKHKG";
			Assert("RequiresSailingGeneration should be set to true.", voyage.RequiresSailingGeneration);

			voyage.Sailings.RemoveAndDeleteAll();
			AssertEquals("No sailing exists.", 0, voyage.Sailings.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightDataRegistry.Instance.EnableSailingGenerationOnServiceTaskSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Factory.Save();
			}
			AssertEquals("No sailing was created.", 0, voyage.Sailings.Count);
			Assert("RequiresSailingGeneration should be reset to false.", !voyage.RequiresSailingGeneration);

			dest.JB_RL_NKPortOfDischarge = "NZAKL";
			dest.JB_E_ARV = ZDateTime.Today.AddDays(2);

			voyage.Sailings.RemoveAndDeleteAll();
			AssertEquals("No sailing exists.", 0, voyage.Sailings.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightDataRegistry.Instance.EnableSailingGenerationOnServiceTaskSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
			}
			AssertEquals("One sailing was created.", 1, voyage.Sailings.Count);
			Assert("RequiresSailingGeneration should be reset to false.", !voyage.RequiresSailingGeneration);
		}

		#endregion

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestSailingOnlineMatchStatusRefreshedOnVoyageFlightUpdate()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF69";

				var origin1 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;
				var origin2 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;

				var destination1 = Factory.New<VoyageDestination>();
				destination1.JB_E_ARV = ZDate.Today;

				var refUNLOCO1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var refUNLOCO2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
				var refUNLOCO3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");

				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.Add(origin1);
				voyage.Origins.Add(origin2);
				voyage.Destinations.Add(destination1);

				origin1.JA_RL_NKPortOfLoading = refUNLOCO1.RL_Code;
				origin2.JA_RL_NKPortOfLoading = refUNLOCO2.RL_Code;
				destination1.JB_RL_NKPortOfDischarge = refUNLOCO3.RL_Code;

				voyage.GenerateSailings();
				AssertEquals("Pre: Expected 2 sailing schedules generated.", 2, voyage.Sailings.Count);

				voyage.Sailings.ForEach(sailing => ((JobSailing)sailing).JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched);
				voyage.JV_VoyageFlight = "QF69";
				CombineAssertions("Expected all sailings to have their online schedule status updated when voyage flight changed.", () =>
				{
					voyage.Sailings.ForEach(sailing => AssertEquals(Constants.FlightScheduleStatus.Matched, (sailing as JobSailing).JX_OnlineScheduleStatus));
				});
			});
		}

		public void TestShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var voyage = Factory.New<JobVoyage>();
			AssertEquals("Should update audit fields if only children have changes", true, ((IUpdateAuditFields)voyage).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		#region Implementation

		ZQuery DelayAlertStmPrintJobQuery
		{
			get
			{
				if (fDelayAlertStmPrintJobQuery == null)
				{
					fDelayAlertStmPrintJobQuery = new ZDBOnlyQuery(typeof(StmPrintJob));
					fDelayAlertStmPrintJobQuery.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
					var fDelayAlertStmPrintJobSubQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
					fDelayAlertStmPrintJobSubQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, @"TO");
					fDelayAlertStmPrintJobSubQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, "someone@example.com");
					fDelayAlertStmPrintJobQuery.AddSubQuery(fDelayAlertStmPrintJobSubQuery, JoinCondition.And);
				}
				return fDelayAlertStmPrintJobQuery;
			}
		}
		ZDBOnlyQuery fDelayAlertStmPrintJobQuery;

		VoyageOrigin origin1;
		VoyageDestination destination1;
		JobVoyage Voyage;
		RefVessel Vessel1;
		SailingsForTestClasses SailingsHelper;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			SailingsHelper = new SailingsForTestClasses(Factory);

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_VoyageFlight = "123456";

			Vessel1 = Factory.New<RefVessel>();
			Vessel1.RV_Name = "Gabriela2";
		}

		JobVoyage GetPopulatedVoyage()
		{
			const string AUSYD = "AUSYD";
			const string AUMEL = "AUMEL";
			const string AUBNE = "AUBNE";
			const string USLAX = "USLAX";
			const string USSEA = "USSEA";
			const string USSAN = "USSAN";

			ZDateTime now = ZDateTime.Now;
			JobVoyage voyage = Factory.New<JobVoyage>();

			origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = AUSYD;
			origin1.JA_E_DEP = now.AddDays(1);
			origin1.JA_A_DEP = now.AddDays(2);

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AUMEL;
			origin2.JA_E_DEP = now.AddDays(3);
			origin2.JA_A_DEP = now.AddDays(4);

			VoyageOrigin origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = AUBNE;

			destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = USLAX;
			destination1.JB_E_ARV = now.AddDays(5);
			destination1.JB_A_ARV = now.AddDays(6);

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = USSEA;
			destination2.JB_E_ARV = now.AddDays(7);
			destination2.JB_A_ARV = now.AddDays(8);

			VoyageDestination destination3 = voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = USSAN;

			Voyage.GenerateSailings();

			foreach (VoyageOrigin origin in new VoyageOrigin[] { origin1, origin2, origin3 })
			{
				SlotAllocation allocation = origin.SlotAllocations.GetAllocation(ZGuid.Empty);
				allocation.SetAspect("AS1", 150);
			}

			SlotAllocation countryAllocation = Voyage.CurrentCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			countryAllocation.SetAspect("AS1", 150);

			foreach (JobSailing sailing in Voyage.Sailings)
			{
				SlotAllocation sailingAllocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
				sailingAllocation.SetAspect("AS1", 450);
			}

			return voyage;
		}

		class DummyVoyageValidation : JobVoyageValidation
		{
			public DummyVoyageValidation(JobVoyage parent)
				: base(parent) { }
		}

		#endregion
	}
}
