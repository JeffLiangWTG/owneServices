using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(JobVoyageEventLogSynchronizerForTesting))]
	sealed class JobSailingEventLogSubscriberTest : LogSubscriberTest<JobVoyageEventLogSynchronizer>
	{
		[Serializable]
		class JobVoyageEventLogSynchronizerForTesting : JobVoyageEventLogSynchronizer
		{
			public JobVoyageEventLogSynchronizerForTesting() : base()
			{
			}

			public JobVoyageEventLogSynchronizerForTesting(int maximumElementsWithoutAllowTableValuedParameters) : base(maximumElementsWithoutAllowTableValuedParameters)
			{
			}

			public void ProcessLogQueueItemsExposed(IQueuedLog[] queuedLogs)
			{
				ProcessLogQueueItems(queuedLogs);
			}

			public new StmALog[] GetExistingTransportLogs(IEnumerable<Transport> transports)
			{
				return base.GetExistingTransportLogs(transports);
			}
		}

		public void TestProcessLogQueueItemsDoesNotSaveDataInDatabase()
		{
			var log = Factory.New<IQueuedLog>();
			var synchroniser = new JobVoyageEventLogSynchronizerForTesting();

			var savedCount = 0;
			Factory.Saved += (sender, e) => savedCount++;

			synchroniser.ProcessLogQueueItemsExposed(new[] { log });

			AssertEquals("ProcessLogQueueItems should not save the Factory. It will be saved automatically by Process Task architecture.", 0, savedCount);
		}

		public void TestSynchronize_ThrowsExceptionWhenTransportsIsNull()
		{
			try
			{
				var synchroniser = new JobVoyageEventLogSynchronizerForTesting();
				synchroniser.SyncTransportEvents(null);

				Fail("Null transports should throw exception");
			}
			catch (Exception e)
			{
				AssertEquals("Error is reported when transports is null", "Value cannot be null.\r\nParameter name: transports", e.Message);
			}
		}

		public void TestSynchronize_TransportLogs_WhenAnyTransportSupporterIsNull()
		{
			var etd = ZDateTime.Today.AddDays(-5);
			var eta = ZDateTime.Today.AddDays(-4);
			var atd = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Destination.JB_E_ARV = eta;
			Factory.Save();
			Factory.RefreshEnabled = false;

			CombineAssertions(delegate
			{
				AssertArvDepLogsDontExist("Precondition - ConsolTransport1 has no ARV+DEP events", ConsolTransport1);
				AssertArvDepLogsDontExist("Precondition - ConsolTransport2 has no ARV+DEP event", ConsolTransport2);
				AssertArvDepLogsDontExist("Precondition - ConsolTransport3 has no ARV+DEP event", ConsolTransport3);
				AssertArvDepLogsDontExist("Precondition - ShipmentTransport1 has no ARV+DEP events", ShipmentTransport1);
				AssertArvDepLogsDontExist("Precondition - ShipmentTransport1 has no ARV+DEP events", BookingTransport1);
			});

			// Trigger saves on a separate Factory to avoid onFactorySaving magic
			var newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			loadedConsolTransport1.ParentType = typeof(CommonConsol);
			loadedConsolTransport1.JW_ATD = atd.AddDays(-10);
			loadedConsolTransport1.JW_ATA = ata.AddDays(-10);
			loadedConsolTransport1.JW_ParentGUID = ZGuid.Empty;
			newFactory.Save();

			AssertEquals("TransportSupporterWithSchedule of loadedConsolTransport1 is null", null, loadedConsolTransport1.TransportSupporterWithSchedule);

			AssertArvDepLogsExist("ConsolTransport1 created it's own ARV+DEP events", loadedConsolTransport1);
			var transport1ArvLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Actual);
			var transport1DepLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Actual);

			newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);
			loadedConsolTransport3.ParentType = typeof(CommonConsol);

			// Dummy values to get multiple edit events on the Voyage
			loadedConsolTransport3.JW_ATD = ZDateTime.Today.AddDays(-16);
			loadedConsolTransport3.JW_ATA = ZDateTime.Today.AddDays(-15);
			newFactory.Save();
			loadedConsolTransport3.JW_ATD = ZDateTime.Empty;
			loadedConsolTransport3.JW_ATA = ZDateTime.Empty;
			newFactory.Save();
			loadedConsolTransport3.JW_ATD = atd;
			loadedConsolTransport3.JW_ATA = ata;
			newFactory.Save();

			AssertArvDepLogsExist("ConsolTransport3 created it's own ARV+DEP events", loadedConsolTransport3);
			var transport3ArvLog = MostRecentLog(loadedConsolTransport3, Events.Arrival, EstimateActual.Actual);
			var transport3DepLog = MostRecentLog(loadedConsolTransport3, Events.Departure, EstimateActual.Actual);

			AssertNoExceptionThrown(RunLogWalkerCycleForTest);

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleConsolTransport1 = postCycleFactory.Load<Transport>(ConsolTransport1.PK);
			var postCycleConsolTransport2 = postCycleFactory.Load<Transport>(ConsolTransport2.PK);
			var postCycleConsolTransport3 = postCycleFactory.Load<Transport>(ConsolTransport3.PK);
			var postCycleShipmentTransport1 = postCycleFactory.Load<Transport>(ShipmentTransport1.PK);
			var postCycleBookingTransport1 = postCycleFactory.Load<Transport>(BookingTransport1.PK);

			CombineAssertions(delegate
			{
				AssertArvDepLogsExist("ConsolTransport1 has ARV+DEP events", postCycleConsolTransport1);
				AssertArvDepLogsExist("ConsolTransport2 has ARV+DEP events", postCycleConsolTransport2);
				AssertArvDepLogsExist("ConsolTransport3 has ARV+DEP events", postCycleConsolTransport3);
				AssertArvDepLogsExist("ShipmentTransport1 has ARV+DEP events", postCycleShipmentTransport1);

				AssertEquals("ATA event on transport1 should have updated", ata, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on transport1 should have updated", atd, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATA event on transport2 should be set", ata, MostRecentLog(postCycleConsolTransport2, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on transport2 should be set", atd, MostRecentLog(postCycleConsolTransport2, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATA event on shipmentTransport should be set", ata, MostRecentLog(postCycleShipmentTransport1, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on shipmentTransport should be set", atd, MostRecentLog(postCycleShipmentTransport1, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertArvDepLogsDontExist("Transport Bookings Schedules haven't been implemented for this yet - BookingTransport1 has no ARV+DEP events", postCycleBookingTransport1);

				AssertNotEquals("Transport 1 - Arrival Log was Updated/Recreated", transport1ArvLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).PK);
				AssertNotEquals("Transport 1 - Departure Log was Updated/Recreated", transport1DepLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).PK);
				AssertEquals("Transport 3 - Arrival Log was not Recreated", transport3ArvLog.PK, MostRecentLog(postCycleConsolTransport3, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 3 - Departure Log was not Recreated", transport3DepLog.PK, MostRecentLog(postCycleConsolTransport3, Events.Departure, EstimateActual.Actual).PK);

				Assert("Transport1 JW_ATD has changed", loadedConsolTransport1.JW_ATD > ata.AddDays(-10));
			});
		}

		public void TestSynchronize_TransportLogs()
		{
			var etd = ZDateTime.Today.AddDays(-5);
			var eta = ZDateTime.Today.AddDays(-4);
			var atd = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Destination.JB_E_ARV = eta;
			Factory.Save();
			Factory.RefreshEnabled = false;

			CombineAssertions(delegate
			{
				AssertArvDepLogsDontExist("Precondition - ConsolTransport1 has no ARV+DEP events", ConsolTransport1);
				AssertArvDepLogsDontExist("Precondition - ConsolTransport2 has no ARV+DEP event", ConsolTransport2);
				AssertArvDepLogsDontExist("Precondition - ConsolTransport3 has no ARV+DEP event", ConsolTransport3);
				AssertArvDepLogsDontExist("Precondition - ShipmentTransport1 has no ARV+DEP events", ShipmentTransport1);
				AssertArvDepLogsDontExist("Precondition - ShipmentTransport1 has no ARV+DEP events", BookingTransport1);
			});

			// Trigger saves on a separate Factory to avoid onFactorySaving magic
			var newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			loadedConsolTransport1.ParentType = typeof(CommonConsol);
			loadedConsolTransport1.JW_ATD = atd.AddDays(-10);
			loadedConsolTransport1.JW_ATA = ata.AddDays(-10);
			newFactory.Save();
			AssertArvDepLogsExist("ConsolTransport1 created it's own ARV+DEP events", loadedConsolTransport1);
			var transport1ArvLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Actual);
			var transport1DepLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Actual);

			newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);
			loadedConsolTransport3.ParentType = typeof(CommonConsol);

			// Dummy values to get multiple edit events on the Voyage
			loadedConsolTransport3.JW_ATD = ZDateTime.Today.AddDays(-16);
			loadedConsolTransport3.JW_ATA = ZDateTime.Today.AddDays(-15);
			newFactory.Save();
			loadedConsolTransport3.JW_ATD = ZDateTime.Empty;
			loadedConsolTransport3.JW_ATA = ZDateTime.Empty;
			newFactory.Save();
			loadedConsolTransport3.JW_ATD = atd;
			loadedConsolTransport3.JW_ATA = ata;
			newFactory.Save();

			AssertArvDepLogsExist("ConsolTransport3 created it's own ARV+DEP events", loadedConsolTransport3);
			var transport3ArvLog = MostRecentLog(loadedConsolTransport3, Events.Arrival, EstimateActual.Actual);
			var transport3DepLog = MostRecentLog(loadedConsolTransport3, Events.Departure, EstimateActual.Actual);

			RunLogWalkerCycleForTest();

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleConsolTransport1 = postCycleFactory.Load<Transport>(ConsolTransport1.PK);
			var postCycleConsolTransport2 = postCycleFactory.Load<Transport>(ConsolTransport2.PK);
			var postCycleConsolTransport3 = postCycleFactory.Load<Transport>(ConsolTransport3.PK);
			var postCycleShipmentTransport1 = postCycleFactory.Load<Transport>(ShipmentTransport1.PK);
			var postCycleBookingTransport1 = postCycleFactory.Load<Transport>(BookingTransport1.PK);

			CombineAssertions(delegate
			{
				AssertArvDepLogsExist("ConsolTransport1 has ARV+DEP events", postCycleConsolTransport1);
				AssertArvDepLogsExist("ConsolTransport2 has ARV+DEP events", postCycleConsolTransport2);
				AssertArvDepLogsExist("ConsolTransport3 has ARV+DEP events", postCycleConsolTransport3);
				AssertArvDepLogsExist("ShipmentTransport1 has ARV+DEP events", postCycleShipmentTransport1);

				AssertEquals("ATA event on transport1 should have updated", ata, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on transport1 should have updated", atd, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATA event on transport2 should be set", ata, MostRecentLog(postCycleConsolTransport2, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on transport2 should be set", atd, MostRecentLog(postCycleConsolTransport2, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATA event on shipmentTransport should be set", ata, MostRecentLog(postCycleShipmentTransport1, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on shipmentTransport should be set", atd, MostRecentLog(postCycleShipmentTransport1, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertArvDepLogsDontExist("Transport Bookings Schedules haven't been implemented for this yet - BookingTransport1 has no ARV+DEP events", postCycleBookingTransport1);

				AssertNotEquals("Transport 1 - Arrival Log was Updated/Recreated", transport1ArvLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).PK);
				AssertNotEquals("Transport 1 - Departure Log was Updated/Recreated", transport1DepLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).PK);
				AssertEquals("Transport 3 - Arrival Log was not Recreated", transport3ArvLog.PK, MostRecentLog(postCycleConsolTransport3, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 3 - Departure Log was not Recreated", transport3DepLog.PK, MostRecentLog(postCycleConsolTransport3, Events.Departure, EstimateActual.Actual).PK);

				Assert("Transport1 JW_ATD has changed", loadedConsolTransport1.JW_ATD > ata.AddDays(-10));
			});
		}

		public void TestSynchronize_TransportLogs_SailingChangedDuringProcessing_NoNullReferenceException()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "IFC trigger for test";
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.ReferenceCode = "AUSYD->USLAX";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Transports.JW_JX>";
			action.PQ_FieldValue = "00000000-0000-0000-0000-000000000001";

			Factory.Save();

			var sailingCutOffDate = ZDateTime.Today.AddDays(10);
			Sailing.Destination.JB_A_ARV = sailingCutOffDate;

			Factory.Save();

			RunLogWalkerCycleForTest();

			Assert("No Null Reference Exception should be exposed", !NotifiedEventList.Contains("[Job Voyage Event Log Synchronizer] failed to process logs. Affected records will be processed again one-by-one.\r\nObject reference not set to an instance of an object."));
		}

		public void TestSyncchronize_TransportLegs_JW_IslinkedChangedByPreviousTransport_NoErrorReport()
		{
			ErrorReporter.Clear();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = Sailing.PK;

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "IFC trigger for test";
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.ReferenceCode = "AUSYD->USLAX";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Transports.Where(\"<JW_LegOrder>\"==\"2\").JW_IsLinked>";
			action.PQ_FieldValue = "false";

			Factory.Save();

			var sailingCutOffDate = ZDateTime.Today.AddDays(10);
			Sailing.Destination.JB_A_ARV = sailingCutOffDate;

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSynchronize_TransportLogsUsesTVP()
		{
			var transports = new List<Transport>();
			for (var i = 0; i < 11; i++)
			{
				var consol = Factory.New<CommonConsol>();
				var consolTransport = consol.Transports.AddNew();
				consolTransport.JW_IsLinked = true;
				consolTransport.JW_JX = Sailing.PK;
				transports.Add(consolTransport);
			}

			var synchronizer = new JobVoyageEventLogSynchronizerForTesting(10);
			synchronizer.GetExistingTransportLogs(transports);

			AssertContains(
				"Use TVP when maximum transport elements is exceeded",
				"SL_Parent in (SELECT Value FROM @CWO2_))",
				SqlEventTracker.Instance.LastSqlQuery
			);
		}

		public void TestSynchronize_TransportLogsDoesNotUseTVP()
		{
			var transports = new List<Transport>();
			for (var i = 0; i < 11; i++)
			{
				var consol = Factory.New<CommonConsol>();
				var consolTransport = consol.Transports.AddNew();
				consolTransport.JW_IsLinked = true;
				consolTransport.JW_JX = Sailing.PK;
				transports.Add(consolTransport);
			}

			var synchronizer = new JobVoyageEventLogSynchronizerForTesting(20);
			synchronizer.GetExistingTransportLogs(transports);

			var parameterRegex = string.Join(", ", transports.Select(transport => "@CWO.*")); // CWO1_, CWO2_, etc.. 

			AssertMatch(
				"Do not use TVP when transport elements is less than the maximum",
				new Regex($@"\(SL_Parent in \({parameterRegex}\)\)"),
				SqlEventTracker.Instance.LastSqlQuery
			);
		}

		public void TestSynchronize_DoNotSyncTransportEventLogWithEmptyValue()
		{
			var etd = ZDateTime.Today.AddDays(-5);
			var atd = ZDateTime.Today.AddDays(-3);
			var eta = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Origin.JA_A_DEP = atd;

			Sailing.Destination.JB_E_ARV = eta;
			Sailing.Destination.JB_A_ARV = ata;

			Factory.Save();
			Factory.RefreshEnabled = false;

			var newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);

			loadedConsolTransport1.ParentType = typeof(CommonConsol);
			loadedConsolTransport1.JW_ATD = atd.AddDays(-8);
			loadedConsolTransport1.JW_ETD = etd.AddDays(-6);

			loadedConsolTransport1.JW_ATA = ata.AddDays(-7);
			loadedConsolTransport1.JW_ETA = eta.AddDays(-5);

			loadedConsolTransport1.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
			loadedConsolTransport1.Sailing.Origin.JA_E_DEP = ZDateTime.Empty;

			loadedConsolTransport1.Sailing.Destination.JB_E_ARV = ZDateTime.Empty;
			loadedConsolTransport1.Sailing.Destination.JB_A_ARV = ZDateTime.Empty;

			newFactory.Save();
			var transportArvActualLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Actual);
			var transportArvEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Estimate);
			var transportDepActualLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Actual);
			var transportDepEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Estimate);

			loadedConsolTransport1.HasChanges = false;

			RunLogWalkerCycleForTest();

			AssertEquals("JA_A_DEP should be empty date time", ZDateTime.Empty, Sailing.Origin.JA_A_DEP);
			AssertEquals("JA_E_DEP should be empty date time", ZDateTime.Empty, Sailing.Origin.JA_E_DEP);
			AssertEquals("JB_E_ARV should be empty date time", ZDateTime.Empty, Sailing.Destination.JB_E_ARV);
			AssertEquals("JB_A_ARV should be empty date time", ZDateTime.Empty, Sailing.Destination.JB_A_ARV);

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleConsolTransport1 = postCycleFactory.Load<Transport>(ConsolTransport1.PK);

			var log = MostRecentLog(postCycleConsolTransport1, AutoEvents.Departure, EstimateActual.Actual);
			AssertNull("ATD event on transport1 should not have any log", log);
			AssertEquals("Old ATD event on transport1 should be cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportDepActualLog.PK).First().IsCancelled);

			log = MostRecentLog(postCycleConsolTransport1, AutoEvents.Departure, EstimateActual.Estimate);
			AssertNull("ETD event on transport1 should not have any log", log);
			AssertEquals("Old ETD event on transport1 should be cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportDepEstimateLog.PK).First().IsCancelled);

			log = MostRecentLog(postCycleConsolTransport1, AutoEvents.Arrival, EstimateActual.Actual);
			AssertNull("ATA event on transport1 should not have any log", log);
			AssertEquals("Old ATA event on transport1 should be cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportArvActualLog.PK).First().IsCancelled);

			log = MostRecentLog(postCycleConsolTransport1, AutoEvents.Arrival, EstimateActual.Estimate);
			AssertNull("ETA event on transport1 should not have any log", log);
			AssertEquals("Old ETA event on transport1 should be cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportArvEstimateLog.PK).First().IsCancelled);
		}

		[TestDate(2014, 01, 10)]
		public void TestGeneratedCOFEvent_FCLCutOffDate()
		{
			var today = ZDateTime.Today;
			Sailing.JX_DepotCutOff = ZDate.Empty;

			Consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol2.JK_TransportMode = Constants.TransportModes.Air;
			Consol2.JK_ConsolMode = Constants.ContainerModes.ULD;
			Consol3.JK_ConsolMode = Constants.ContainerModes.FCL;

			Factory.Save();

			AssertNull("Precondition: ConsolTransport1 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport1, Events.CutOffDate, EstimateActual.Estimate));
			AssertNull("Precondition: ConsolTransport2 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport2, Events.CutOffDate, EstimateActual.Estimate));
			AssertNull("Precondition: ConsolTransport3 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport3, Events.CutOffDate, EstimateActual.Estimate));

			ConsolTransport1.JW_TerminalCutOff = today.AddDays(-5);
			ConsolTransport2.JW_TerminalCutOff = today.AddDays(4);

			Factory.Save();

			AssertCofLogCreated("Expected an event to be added for ConsolTransport1", ConsolTransport1, today.AddDays(-5));
			AssertCofLogCreated("Expected an event to be added for ConsolTransport2", ConsolTransport2, today.AddDays(4));
			AssertNull("Expected that ConsolTransport3 should still have no estimated Cut Off event", MostRecentLog(ConsolTransport3, Events.CutOffDate, EstimateActual.Estimate));

			var sailingCutOffDate = today.AddDays(10);
			Sailing.Origin.JA_CutOff = sailingCutOffDate;

			Factory.Save();
			Factory.RefreshEnabled = false;

			RunLogWalkerCycleForTest();

			var newFactory = Factory.CreateNewFactory();
			var reloadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			var reloadedConsolTransport2 = newFactory.Load<Transport>(ConsolTransport2.PK);
			var reloadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);

			AssertCofLogCreated("Expected an event to be added for ConsolTransport1, overriding the old event from the sailing", reloadedConsolTransport1, sailingCutOffDate);
			AssertCofLogCreated("Expected an event to be added for ConsolTransport2, overriding the old event from the sailing", reloadedConsolTransport2, sailingCutOffDate);
			AssertCofLogCreated("Expected an event to be added for ConsolTransport3 now the service task has been run", reloadedConsolTransport3, sailingCutOffDate);
		}

		[TestDate(2014, 01, 10)]
		public void TestGeneratedSRCEvent_StorageCommencedDate()
		{
			Sailing.Destination.JB_StorageDate = ZDate.Empty;

			Consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol2.JK_TransportMode = Constants.TransportModes.Air;
			Consol2.JK_ConsolMode = Constants.ContainerModes.Loose;
			Consol3.JK_ConsolMode = Constants.ContainerModes.FCL;

			Factory.Save();

			AssertNull("Precondition: ConsolTransport1 has no existing estimated Storage Commenced events", MostRecentLog(ConsolTransport1, Events.StorageCommenced, EstimateActual.Actual));
			AssertNull("Precondition: ConsolTransport2 has no existing estimated Storage Commenced events", MostRecentLog(ConsolTransport2, Events.StorageCommenced, EstimateActual.Actual));
			AssertNull("Precondition: ConsolTransport3 has no existing estimated Storage Commenced events", MostRecentLog(ConsolTransport3, Events.StorageCommenced, EstimateActual.Actual));

			var today = ZDateTime.Today;
			ConsolTransport1.JW_TerminalStorageDate = today.AddDays(-5);
			ConsolTransport2.JW_TerminalStorageDate = today.AddDays(4);

			Factory.Save();

			AssertSrcLogCreated("Expected an event to be added for ConsolTransport1", ConsolTransport1, today.AddDays(-5));
			AssertSrcLogCreated("Expected an event to be added for ConsolTransport2", ConsolTransport2, today.AddDays(4));
			AssertNull("Expected that ConsolTransport3 should still have no actual Storage Commenced event", MostRecentLog(ConsolTransport3, Events.StorageCommenced, EstimateActual.Actual));

			var storageDate = today.AddDays(10);
			Sailing.Destination.JB_StorageDate = storageDate;

			Factory.Save();
			Factory.RefreshEnabled = false;

			RunLogWalkerCycleForTest();

			var newFactory = Factory.CreateNewFactory();
			var reloadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			var reloadedConsolTransport2 = newFactory.Load<Transport>(ConsolTransport2.PK);
			var reloadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);

			AssertSrcLogCreated("Expected an event to be added for ConsolTransport1, overriding the old event from the sailing", reloadedConsolTransport1, storageDate);
			AssertSrcLogCreated("Expected an event to be added for ConsolTransport2, overriding the old event from the sailing", reloadedConsolTransport2, storageDate);
			AssertSrcLogCreated("Expected an event to be added for ConsolTransport3 now the service task has been run", reloadedConsolTransport3, storageDate);
		}

		[TestDate(2014, 01, 10)]
		public void TestGeneratedRTCEvent_ReceivalCommencedDate()
		{
			Sailing.Origin.JA_ReceivalCommences = ZDate.Empty;

			Consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol2.JK_TransportMode = Constants.TransportModes.Air;
			Consol2.JK_ConsolMode = Constants.ContainerModes.Loose;
			Consol3.JK_ConsolMode = Constants.ContainerModes.FCL;

			Factory.Save();

			AssertNull("Precondition: ConsolTransport1 has no existing estimated Receipt Commenced events", MostRecentLog(ConsolTransport1, Events.ReceiptCommenced, EstimateActual.Actual));
			AssertNull("Precondition: ConsolTransport2 has no existing estimated Receipt Commenced events", MostRecentLog(ConsolTransport2, Events.ReceiptCommenced, EstimateActual.Actual));
			AssertNull("Precondition: ConsolTransport3 has no existing estimated Receipt Commenced events", MostRecentLog(ConsolTransport3, Events.ReceiptCommenced, EstimateActual.Actual));

			var today = ZDateTime.Today;
			ConsolTransport1.JW_TerminalReceivalCommences = today.AddDays(-5);
			ConsolTransport2.JW_TerminalReceivalCommences = today.AddDays(4);

			Factory.Save();

			AssertRtcLogCreated("Expected an event to be added for ConsolTransport1", ConsolTransport1, today.AddDays(-5));
			AssertRtcLogCreated("Expected an event to be added for ConsolTransport2", ConsolTransport2, today.AddDays(4));
			AssertNull("Expected that ConsolTransport3 should still have no actual Storage Commenced event", MostRecentLog(ConsolTransport3, Events.ReceiptCommenced, EstimateActual.Actual));

			var receiptCommencedDate = today.AddDays(10);
			Sailing.Origin.JA_ReceivalCommences = receiptCommencedDate;

			Factory.Save();
			Factory.RefreshEnabled = false;

			RunLogWalkerCycleForTest();

			var newFactory = Factory.CreateNewFactory();
			var reloadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			var reloadedConsolTransport2 = newFactory.Load<Transport>(ConsolTransport2.PK);
			var reloadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);

			AssertRtcLogCreated("Expected an event to be added for ConsolTransport1, overriding the old event from the sailing", reloadedConsolTransport1, receiptCommencedDate);
			AssertRtcLogCreated("Expected an event to be added for ConsolTransport2, overriding the old event from the sailing", reloadedConsolTransport2, receiptCommencedDate);
			AssertRtcLogCreated("Expected an event to be added for ConsolTransport3 now the service task has been run", reloadedConsolTransport3, receiptCommencedDate);
		}

		[TestDate(2014, 01, 10)]
		public void TestGeneratedCOFEvent_LCLCutOffDate()
		{
			var today = ZDateTime.Today;
			Sailing.Origin.JA_CutOff = ZDate.Empty;

			Consol1.JK_ConsolMode = Constants.ContainerModes.LCL;
			Consol2.JK_TransportMode = Constants.TransportModes.Air;
			Consol2.JK_ConsolMode = Constants.ContainerModes.Loose;
			Consol3.JK_ConsolMode = Constants.ContainerModes.LCL;

			Factory.Save();

			AssertNull("Precondition: ConsolTransport1 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport1, Events.CutOffDate, EstimateActual.Estimate));
			AssertNull("Precondition: ConsolTransport2 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport2, Events.CutOffDate, EstimateActual.Estimate));
			AssertNull("Precondition: ConsolTransport3 has no existing estimated Cut Off events", MostRecentLog(ConsolTransport3, Events.CutOffDate, EstimateActual.Estimate));

			ConsolTransport1.JW_DepotCutOff = today.AddDays(-5);
			ConsolTransport2.JW_DepotCutOff = today.AddDays(4);

			Factory.Save();

			AssertCofLogCreated("Expected an event to be added for ConsolTransport1", ConsolTransport1, today.AddDays(-5));
			AssertCofLogCreated("Expected an event to be added for ConsolTransport2", ConsolTransport2, today.AddDays(4));
			AssertNull("Expected that ConsolTransport3 should still have no estimated Cut Off event", MostRecentLog(ConsolTransport3, Events.CutOffDate, EstimateActual.Estimate));

			var sailingCutOffDate = today.AddDays(10);
			Sailing.JX_DepotCutOff = sailingCutOffDate;

			Factory.Save();
			Factory.RefreshEnabled = false;

			RunLogWalkerCycleForTest();

			var newFactory = Factory.CreateNewFactory();
			var reloadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			var reloadedConsolTransport2 = newFactory.Load<Transport>(ConsolTransport2.PK);
			var reloadedConsolTransport3 = newFactory.Load<Transport>(ConsolTransport3.PK);

			AssertCofLogCreated("Expected an event to be added for ConsolTransport1, overriding the old event from the sailing", reloadedConsolTransport1, sailingCutOffDate);
			AssertCofLogCreated("Expected an event to be added for ConsolTransport2, overriding the old event from the sailing", reloadedConsolTransport2, sailingCutOffDate);
			AssertCofLogCreated("Expected an event to be added for ConsolTransport3 now the service task has been run", reloadedConsolTransport3, sailingCutOffDate);
		}

		public void TestSynchronize_TransportLogsForAgencyShipment()
		{
			var shipment = (CommonShipment)Factory.New<Integration.Agency.IAgencyShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			Factory.Save();

			var etd = ZDateTime.Today.AddDays(-5);
			var eta = ZDateTime.Today.AddDays(-4);
			var atd = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Destination.JB_E_ARV = eta;

			Factory.Save();
			Factory.RefreshEnabled = false;

			AssertArvDepLogsDontExist("Precondition - ShipmentTransport1 has no ARV+DEP events", transport);

			// Trigger saves on a separate Factory to avoid onFactorySaving magic
			var newFactory = Factory.CreateNewFactory();
			var loadedtransport = newFactory.Load<Transport>(transport.PK);
			loadedtransport.ParentType = ObjectFactory.GetType<Integration.Agency.IAgencyShipment>();
			loadedtransport.JW_ATD = atd;
			loadedtransport.JW_ATA = ata;
			newFactory.Save();

			AssertArvDepLogsExist("Transport created it's own ARV+DEP events", loadedtransport);
			var transportArvLog = MostRecentLog(loadedtransport, Events.Arrival, EstimateActual.Actual);
			var transportDepLog = MostRecentLog(loadedtransport, Events.Departure, EstimateActual.Actual);

			RunLogWalkerCycleForTest();

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleTransport = postCycleFactory.Load<Transport>(transport.PK);

			CombineAssertions(delegate
			{
				AssertArvDepLogsExist("ShipmentTransport1 has ARV+DEP events", postCycleTransport);

				AssertEquals("ATA event on shipmentTransport should be set", ata, MostRecentLog(postCycleTransport, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on shipmentTransport should be set", atd, MostRecentLog(postCycleTransport, Events.Departure, EstimateActual.Actual).SL_EventTime);

				AssertEquals("Transport 1 - Arrival Log was created", transportArvLog.PK, MostRecentLog(postCycleTransport, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - Departure Log was created", transportDepLog.PK, MostRecentLog(postCycleTransport, Events.Departure, EstimateActual.Actual).PK);
			});
		}

		public void TestSynchronize_GenerateLogsInLinkedTransports()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var agencyShipment = (ITransportParent)Factory.New<Integration.Agency.IAgencyShipment>();
			var shipmentPreplanning = Factory.New<JobShipmentPreplanning>();
			shipmentPreplanning.BuyerPK = orgHeader.PK;

			AssertTransportsAreSynchronized(agencyShipment);
			AssertTransportsAreSynchronized(shipmentPreplanning);
		}

		void AssertTransportsAreSynchronized(ITransportParent parent)
		{
			var transport = parent.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var newSailing = newFactory.Load<JobSailing>(Sailing.PK);

			var etd = ZDateTime.Today.AddDays(-5);
			var eta = ZDateTime.Today.AddDays(-4);
			var atd = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			newSailing.Origin.JA_E_DEP = etd;
			newSailing.Origin.JA_A_DEP = atd;
			newSailing.Destination.JB_E_ARV = eta;
			newSailing.Destination.JB_A_ARV = ata;

			newFactory.Save();

			RunLogWalkerCycleForTest();

			var newtransport = newFactory.Load<Transport>(transport.PK);
			var transportATALog = MostRecentLog(newtransport, Events.Arrival, EstimateActual.Actual);
			var transportATDLog = MostRecentLog(newtransport, Events.Departure, EstimateActual.Actual);
			var transportETALog = MostRecentLog(newtransport, Events.Arrival, EstimateActual.Estimate);
			var transportETDLog = MostRecentLog(newtransport, Events.Departure, EstimateActual.Estimate);

			AssertEquals("ETA log event date", eta, transportETALog.SL_EventTime);
			AssertEquals("ETD log event date", etd, transportETDLog.SL_EventTime);
			AssertEquals("ATA log event date", ata, transportATALog.SL_EventTime);
			AssertEquals("ATD log event date", atd, transportATDLog.SL_EventTime);
		}

		[TestDate(2014, 07, 29)]
		public void TestSynchronization_SaillingAvailabilityDatesAreChanged_GenerateCargoAvailableEventOnLinkedTransport()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.Origins.AddNew("AUSYD");
			voyage.Destinations.AddNew("UAIEV");

			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "UAIEV");

			var transport1 = Factory.New<CommonConsol>().WithContainerMode(Constants.ContainerModes.FCL).Transports.AddNew().Linked().WithSailing(sailing.PK);
			var transport2 = Factory.New<CommonConsol>().WithContainerMode(Constants.ContainerModes.ULD).Transports.AddNew().Linked().WithSailing(sailing.PK);
			var transport3 = Factory.New<CommonConsol>().WithContainerMode(Constants.ContainerModes.LCL).Transports.AddNew().Linked().WithSailing(sailing.PK);
			var transport4 = Factory.New<CommonConsol>().WithContainerMode(Constants.ContainerModes.Loose).Transports.AddNew().Linked().WithSailing(sailing.PK);

			Factory.Save();

			sailing.Destination.JB_AvailabilityDate = 1.DaysAgo();
			sailing.JX_DepotAvailabilityDate = 2.DaysAgo();

			Factory.Save();

			RunLogWalkerCycleForTest();

			var newFactory = Factory.CreateNewFactory();

			var log1 = newFactory.Load<Transport>(transport1.PK).Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			var log2 = newFactory.Load<Transport>(transport2.PK).Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			var log3 = newFactory.Load<Transport>(transport3.PK).Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			var log4 = newFactory.Load<Transport>(transport4.PK).Logs.MostRecentLogByEventTime(Events.CargoAvailable);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log1);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log2);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log3);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log4);

			AssertEquals("Event date", 1.DaysAgo(), log1.SL_EventTime);
			AssertEquals("Event date", 1.DaysAgo(), log2.SL_EventTime);
			AssertEquals("Event date", 2.DaysAgo(), log3.SL_EventTime);
			AssertEquals("Event date", 2.DaysAgo(), log4.SL_EventTime);
		}

		public void TestSynchronize_TransportLogsForDeclaration()
		{
			var declaration = (ITransportParent)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var transport = declaration.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			Factory.Save();

			var etd = ZDateTime.Today.AddDays(-5);
			var eta = ZDateTime.Today.AddDays(-4);
			var atd = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Destination.JB_E_ARV = eta;

			Factory.Save();
			Factory.RefreshEnabled = false;

			AssertArvDepLogsDontExist("Precondition - DeclarationTransport1 has no ARV+DEP events", transport);

			// Trigger saves on a separate Factory to avoid onFactorySaving magic
			var newFactory = Factory.CreateNewFactory();
			var loadedtransport = newFactory.Load<Transport>(transport.PK);
			loadedtransport.ParentType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			loadedtransport.JW_ATD = atd;
			loadedtransport.JW_ATA = ata;
			newFactory.Save();

			AssertArvDepLogsExist("Transport created it's own ARV+DEP events", loadedtransport);
			var transportArvLog = MostRecentLog(loadedtransport, Events.Arrival, EstimateActual.Actual);
			var transportDepLog = MostRecentLog(loadedtransport, Events.Departure, EstimateActual.Actual);

			RunLogWalkerCycleForTest();

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleTransport = postCycleFactory.Load<Transport>(transport.PK);

			CombineAssertions(delegate
			{
				AssertArvDepLogsExist("DeclarationTransport1 has ARV+DEP events", postCycleTransport);

				AssertEquals("ATA event on shipmentTransport should be set", ata, MostRecentLog(postCycleTransport, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("ATD event on shipmentTransport should be set", atd, MostRecentLog(postCycleTransport, Events.Departure, EstimateActual.Actual).SL_EventTime);

				AssertEquals("Transport 1 - Arrival Log was created", transportArvLog.PK, MostRecentLog(postCycleTransport, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - Departure Log was created", transportDepLog.PK, MostRecentLog(postCycleTransport, Events.Departure, EstimateActual.Actual).PK);
			});
		}

		public void TestSynchronize_WhenLatestLogHasDifferentTimeOffset_CreateNewLogs()
		{
			var etd = ZDateTime.Today.AddDays(-5);
			var atd = ZDateTime.Today.AddDays(-3);
			var eta = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Origin.JA_A_DEP = atd;
			Sailing.Destination.JB_E_ARV = eta;
			Sailing.Destination.JB_A_ARV = ata;

			Factory.Save();
			Factory.RefreshEnabled = false;

			var newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);
			loadedConsolTransport1.ParentType = typeof(CommonConsol);

			AddLog(newFactory, loadedConsolTransport1, Events.Arrival, EstimateActual.Actual, ata.AddDays(1));
			AddLog(newFactory, loadedConsolTransport1, Events.Arrival, EstimateActual.Estimate, eta.AddDays(1));
			AddLog(newFactory, loadedConsolTransport1, Events.Departure, EstimateActual.Actual, atd.AddDays(1));
			AddLog(newFactory, loadedConsolTransport1, Events.Departure, EstimateActual.Estimate, etd.AddDays(1));

			loadedConsolTransport1.ParentType = typeof(CommonConsol);
			loadedConsolTransport1.JW_ATD = atd.AddDays(-8);
			loadedConsolTransport1.JW_ETD = etd.AddDays(-6);
			loadedConsolTransport1.JW_ATA = ata.AddDays(-7);
			loadedConsolTransport1.JW_ETA = eta.AddDays(-5);

			loadedConsolTransport1.Sailing.Origin.JA_A_DEP = atd.AddDays(1);
			loadedConsolTransport1.Sailing.Origin.JA_E_DEP = etd.AddDays(1);
			loadedConsolTransport1.Sailing.Destination.JB_E_ARV = eta.AddDays(1);
			loadedConsolTransport1.Sailing.Destination.JB_A_ARV = ata.AddDays(1);

			newFactory.Save();
			var transportArvActualLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Actual);
			var transportArvEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Estimate);
			var transportDepActualLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Actual);
			var transportDepEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Estimate);

			RunLogWalkerCycleForTest();

			CombineAssertions(() =>
			{
				AssertEquals("JA_A_DEP has been set correctly", atd.AddDays(1), Sailing.Origin.JA_A_DEP);
				AssertEquals("JA_E_DEP has been set correctly", etd.AddDays(1), Sailing.Origin.JA_E_DEP);
				AssertEquals("JB_E_ARV has been set correctly", eta.AddDays(1), Sailing.Destination.JB_E_ARV);
				AssertEquals("JB_A_ARV has been set correctly", ata.AddDays(1), Sailing.Destination.JB_A_ARV);
			});

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleConsolTransport1 = postCycleFactory.Load<Transport>(ConsolTransport1.PK);
			postCycleFactory.Save();

			CombineAssertions(() =>
			{
				AssertNotEquals("Transport 1 - New Actual Arrival Log has been created", transportArvActualLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - New Actual Arrival Log has been created", ata.AddDays(1), MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).SL_EventTime);
				AssertEquals("Transport 1 - Old Actual Arrival Log has been cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportArvActualLog.PK).First().IsCancelled);

				AssertNotEquals("Transport 1 - New Estimate Arrival Log has been created", transportArvEstimateLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Estimate).PK);
				AssertEquals("Transport 1 - New Estimate Arrival Log has been created", eta.AddDays(1), MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Estimate).SL_EventTime);
				AssertEquals("Transport 1 - Old Estimate Arrival Log has been cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportArvEstimateLog.PK).First().IsCancelled);

				AssertNotEquals("Transport 1 - New Actual Departure Log has been created", transportDepActualLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - New Actual Departure Log has been created", atd.AddDays(1), MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).SL_EventTime);
				AssertEquals("Transport 1 - Old Actual Departure Log has been cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportDepActualLog.PK).First().IsCancelled);

				AssertNotEquals("Transport 1 - New Estimate Departure Log has been created", transportDepEstimateLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Estimate).PK);
				AssertEquals("Transport 1 - New Estimate Departure Log has been created", etd.AddDays(1), MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Estimate).SL_EventTime);
				AssertEquals("Transport 1 - Old Estimate Departure Log has been cancelled", true, postCycleConsolTransport1.Logs.GetAllLogs().Where(x => x.PK == transportDepEstimateLog.PK).First().IsCancelled);
			});
		}

		public void TestSynchronize_LatestLogHasSameTimeOffset_DoesNotCreateNewLog()
		{
			var etd = ZDateTime.Today.AddDays(-5);
			var atd = ZDateTime.Today.AddDays(-3);
			var eta = ZDateTime.Today.AddDays(-2);
			var ata = ZDateTime.Today.AddDays(-1);

			Sailing.Origin.JA_E_DEP = etd;
			Sailing.Origin.JA_A_DEP = atd;
			Sailing.Destination.JB_E_ARV = eta;
			Sailing.Destination.JB_A_ARV = ata;

			Factory.Save();
			Factory.RefreshEnabled = false;

			var newFactory = Factory.CreateNewFactory();
			var loadedConsolTransport1 = newFactory.Load<Transport>(ConsolTransport1.PK);

			loadedConsolTransport1.ParentType = typeof(CommonConsol);
			loadedConsolTransport1.JW_ATD = atd.AddDays(-8);
			loadedConsolTransport1.JW_ETD = etd.AddDays(-6);
			loadedConsolTransport1.JW_ATA = ata.AddDays(-7);
			loadedConsolTransport1.JW_ETA = eta.AddDays(-5);

			loadedConsolTransport1.Sailing.Origin.JA_A_DEP = atd.AddDays(-8);
			loadedConsolTransport1.Sailing.Origin.JA_E_DEP = etd.AddDays(-6);
			loadedConsolTransport1.Sailing.Destination.JB_E_ARV = eta.AddDays(-5);
			loadedConsolTransport1.Sailing.Destination.JB_A_ARV = ata.AddDays(-7);

			newFactory.Save();
			var transportArvActualLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Actual);
			var transportArvEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Arrival, EstimateActual.Estimate);
			var transportDepActualLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Actual);
			var transportDepEstimateLog = MostRecentLog(loadedConsolTransport1, Events.Departure, EstimateActual.Estimate);

			RunLogWalkerCycleForTest();

			var postCycleFactory = Factory.CreateNewFactory();
			var postCycleConsolTransport1 = postCycleFactory.Load<Transport>(ConsolTransport1.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Transport 1 - No New Actual Arrival Log has been created", transportArvActualLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - No New Estimate Arrival Log has been created", transportArvEstimateLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Arrival, EstimateActual.Estimate).PK);
				AssertEquals("Transport 1 - No New Actual Departure Log has been created", transportDepActualLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Actual).PK);
				AssertEquals("Transport 1 - No New Estimate Departure Log has been created", transportDepEstimateLog.PK, MostRecentLog(postCycleConsolTransport1, Events.Departure, EstimateActual.Estimate).PK);
			});
		}

		#region Implementation

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return true; }
		}

		void AssertArvDepLogsExist(string message, EnterpriseBusinessObject bizObject)
		{
			AssertEquals(message + "(ARV)", true, MostRecentLog(bizObject, Events.Arrival, EstimateActual.Actual) != null);
			AssertEquals(message + "  - Correct number of events (ARV)", 1, NumberOfLogs(bizObject, Events.Arrival, EstimateActual.Actual));
			AssertEquals(message + "(DEP)", true, MostRecentLog(bizObject, Events.Departure, EstimateActual.Actual) != null);
			AssertEquals(message + "  - Correct number of events (ARV)", 1, NumberOfLogs(bizObject, Events.Departure, EstimateActual.Actual));
		}

		void AssertArvDepLogsDontExist(string message, EnterpriseBusinessObject bizObject)
		{
			AssertEquals(message + "(ARV)", false, MostRecentLog(bizObject, Events.Arrival, EstimateActual.Actual) != null);
			AssertEquals(message + "(DEP)", false, MostRecentLog(bizObject, Events.Departure, EstimateActual.Actual) != null);
		}

		void AssertCofLogCreated(string message, Transport consolTransport, ZDateTime expectedDate)
		{
			var mostRecentLog = MostRecentLog(consolTransport, Events.CutOffDate, EstimateActual.Estimate);
			AssertNotNull(message, mostRecentLog);
			AssertEquals(message + " and event time should be correct", expectedDate, mostRecentLog.SL_EventTime);
			AssertEquals(message + " and expected only one event to have been created", 1, NumberOfLogs(consolTransport, Events.CutOffDate, EstimateActual.Estimate));
		}

		void AssertSrcLogCreated(string message, Transport consolTransport, ZDateTime expectedDate)
		{
			var mostRecentLog = MostRecentLog(consolTransport, Events.StorageCommenced, EstimateActual.Actual);
			AssertNotNull(message, mostRecentLog);
			AssertEquals(message + " and event time should be correct", expectedDate, mostRecentLog.SL_EventTime);
			AssertEquals(message + " and expected only one event to have been created", 1, NumberOfLogs(consolTransport, Events.StorageCommenced, EstimateActual.Actual));
		}

		void AssertRtcLogCreated(string message, Transport consolTransport, ZDateTime expectedDate)
		{
			var mostRecentLog = MostRecentLog(consolTransport, Events.ReceiptCommenced, EstimateActual.Actual);
			AssertNotNull(message, mostRecentLog);
			AssertEquals(message + " and event time should be correct", expectedDate, mostRecentLog.SL_EventTime);
			AssertEquals(message + " and expected only one event to have been created", 1, NumberOfLogs(consolTransport, Events.ReceiptCommenced, EstimateActual.Actual));
		}

		StmALog MostRecentLog(IAutoAdminLogTarget bizObject, Event eventType, EstimateActual estimateOrActual)
		{
			var logFilter = new ZQuery(StmALogSchema.SL_IsEstimate, estimateOrActual == EstimateActual.Estimate);
			logFilter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);

			return bizObject.Logs.MostRecentLogByEventTime(eventType, logFilter);
		}

		int NumberOfLogs(IAutoAdminLogTarget bizObject, Event eventType, EstimateActual estimateOrActual)
		{
			var logFilter = new ZQuery(StmALogSchema.SL_IsEstimate, estimateOrActual == EstimateActual.Estimate);
			logFilter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);

			var logs = bizObject.Logs.Find(logFilter);
			return logs.Length;
		}

		void AddLog(BusinessObjectFactory factory, Transport transport, Event eventType, EstimateActual estimateOrActual, ZDateTime newDateTime)
		{
			Thread.Sleep(1000);
			var logReference = transport.GetReferenceFreeTextForEvent(eventType, ZDateTimeOffset.Empty, newDateTime.ToOffset());
			var logParameters = transport.GetParametersForEvent(eventType).ToArray();

			transport.Logs.AddNew(Events.Arrival, logReference, newDateTime.ToOffset(), estimateOrActual == EstimateActual.Estimate, logParameters);
			factory.Save();
		}
		protected override void SetUp()
		{
			base.SetUp();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Destination = Voyage.Destinations.AddNew();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");

			if (Sailing == null)
			{
				Fail("Sailing not set");
			}

			Consol1 = Factory.New<CommonConsol>();
			ConsolTransport1 = Consol1.Transports.AddNew();
			ConsolTransport1.JW_IsLinked = true;
			ConsolTransport1.JW_JX = Sailing.PK;

			Consol2 = Factory.New<CommonConsol>();
			ConsolTransport2 = Consol2.Transports.AddNew();
			ConsolTransport2.JW_IsLinked = true;
			ConsolTransport2.JW_JX = Sailing.PK;

			Consol3 = Factory.New<CommonConsol>();
			ConsolTransport3 = Consol3.Transports.AddNew();
			ConsolTransport3.JW_IsLinked = true;
			ConsolTransport3.JW_JX = Sailing.PK;

			Shipment = Factory.New<CommonShipment>();
			ShipmentTransport1 = Shipment.Transports.AddNew();
			ShipmentTransport1.JW_IsLinked = true;
			ShipmentTransport1.JW_JX = Sailing.PK;

			BookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var bookingTransports = new TransportCollection((ITransportParentCore)BookingConsolidation);
			BookingTransport1 = bookingTransports.AddNew();
			BookingTransport1.JW_IsLinked = true;
			BookingTransport1.JW_JX = Sailing.PK;

			Factory.Save();
		}

		JobSailing Sailing;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		JobVoyage Voyage;
		CommonConsol Consol1;
		CommonConsol Consol2;
		CommonConsol Consol3;
		Transport ConsolTransport1;
		Transport ConsolTransport2;
		Transport ConsolTransport3;
		Transport ShipmentTransport1;
		Transport BookingTransport1;
		CommonShipment Shipment;
		IDtbBookingConsolidation BookingConsolidation;

		#endregion

		#region MilestoneSailingDateSynchronizerTest (Move from MilestoneSailingDateSynchronizer)

		public void TestSynchronizeForDeclaration()
		{
			var synchronizer = new JobVoyageEventLogSynchronizer();

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var shippingLine = new VoyageTestHelper(Factory).CreateCarrier("MAERSK");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports.MostInterestingTransport.JW_Vessel = vessel.RV_FK;
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Voyage";
			consol.Transports.MostInterestingTransport.CarrierPK = shippingLine.PK;
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "SGSIN";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_Vessel = "Vessel2";
			transport2.JW_VoyageFlight = "222";
			transport2.JW_IsLinked = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			shipment.JS_TransportMode = consol.JK_TransportMode;

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "AUSYD";
			declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "SGSIN";
			declaration[JobDeclarationSchema.JE_VesselName] = vessel.RV_Name;
			declaration[JobDeclarationSchema.JE_LloydsIMO] = vessel.RV_LloydsNumber;
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
			declaration[JobDeclarationSchema.JE_OH_ShippingLine] = shippingLine.PK;

			var shipmentMilestone = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();

			var declarationMilestone = ((IWorkflowProvider)declaration).WorkflowItems.Milestones.AddNew();
			shipmentMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;

			declarationMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
			var declarationTransport = Factory.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, declaration.PK))[0];
			declarationMilestone.P9_ReferencedID = declarationTransport.PK;
			declarationMilestone.P9_ReferencedTableCode = declarationTransport.TablePrefix;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobSailing loadedSailing = newFactory.Load<JobSailing>(consol.Transports.MostInterestingTransport.Sailing.PK);
			loadedSailing.Destination.JB_A_ARV = ZDateTime.Now.AddDays(-3);
			loadedSailing.Destination.JB_AvailabilityDate = ZDateTime.Now.AddDays(-2);

			newFactory.Save();

			RunLogWalkerCycleForTest();

			ProcessTask loadedDeclarationMilestone = (new BusinessObjectFactory()).Load<ProcessTask>(declarationMilestone.PK);
			AssertEquals(loadedSailing.Destination.JB_AvailabilityDate.Date, ((ZDateTime)loadedDeclarationMilestone.P9_ActualDateInfo.PersistentValue).Date);
		}

		#endregion
	}
}
