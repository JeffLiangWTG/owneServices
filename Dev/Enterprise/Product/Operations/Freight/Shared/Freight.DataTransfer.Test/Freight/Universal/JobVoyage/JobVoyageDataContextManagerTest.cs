using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(JobVoyageDataContextManager))]
	sealed class JobVoyageDataContextManagerTest : DataContextManagerTestCase<JobVoyageDataContextManager, JobVoyage>
	{
		public void TestDataContextKey()
		{
			var voyage = GetNewBusinessObjectForTesting();

			var manager = new JobVoyageDataContextManager();
			((IDataContextManager)manager).Init(voyage);

			AssertEquals(ZString.Empty, manager.DataContextKey);
		}

		public void TestManagesEventsAndSchedules()
		{
			var manager = new JobVoyageDataContextManager();
			AssertEquals(true, manager.ManagesEvents);
			AssertEquals(true, manager.ManagesSchedules);
		}

		public void TestPopulateUniversalEventContextReferences()
		{
			AssertPopulateUniversalEventContextReferences_Sea();
			AssertPopulateUniversalEventContextReferences_Air();
			AssertPopulateUniversalEventContextReferences_Rail();
			AssertPopulateUniversalEventContextReferences_Road();
		}

		void AssertPopulateUniversalEventContextReferences_Sea()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";
			vessel.RV_RadioCallSign = "LIAM";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "LTH0", "US").PK;
			voyage.JV_FlightDate = new ZDateTime(2014, 8, 8, 12, 0, 0);

			var manager = new JobVoyageDataContextManager();
			((IDataContextManager)manager).Init(voyage);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"TransportMode|SEA", "VesselName|LIAM HANRAHAN", "VoyageNumber|1100", "FlightDate|08-Aug-14 12:00:00", "LloydsNumber|LIAMTH", "VesselCallSign|LIAM", "CarrierCode|LTH0", "IsCharter|N"
			},
			Format(manager.EventContextValues));
		}

		void AssertPopulateUniversalEventContextReferences_Air()
		{
			var voyage = UniversalTestHelper.CreateAirVoyage(Factory.BOFactory, "QF001", true);
			voyage.JV_FlightDate = new ZDateTime(2014, 8, 8, 12, 0, 0);

			var manager = new JobVoyageDataContextManager();
			((IDataContextManager)manager).Init(voyage);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"TransportMode|AIR", "FlightNumber|QF001", "FlightDate|08-Aug-14 12:00:00", "IsCharter|N"
			},
			Format(manager.EventContextValues));
		}

		void AssertPopulateUniversalEventContextReferences_Rail()
		{
			var voyage = UniversalTestHelper.CreateRailVoyage(Factory.BOFactory, "PACIFIC EXPRESS", "69");

			var manager = new JobVoyageDataContextManager();
			((IDataContextManager)manager).Init(voyage);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"TransportMode|RAI", "VesselName|PACIFIC EXPRESS", "VoyageNumber|69", "IsCharter|N"
			},
			Format(manager.EventContextValues));
		}

		void AssertPopulateUniversalEventContextReferences_Road()
		{
			var voyage = UniversalTestHelper.CreateRoadVoyage(Factory.BOFactory, "T001");

			var manager = new JobVoyageDataContextManager();
			((IDataContextManager)manager).Init(voyage);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"TransportMode|ROA", "VoyageNumber|T001", "IsCharter|N"
			},
			Format(manager.EventContextValues));
		}

		protected override JobVoyage GetNewBusinessObjectForTesting()
		{
			return UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, "ADMIRAL KUZNETSOV", "11435");
		}

		public void TestManagesShipments()
		{
			var voyage = UniversalTestHelper.CreateRoadVoyage(Factory.BOFactory, "T001");
			var manager = new JobVoyageDataContextManager() as IShipmentDataContextManager;
			AssertEquals(true, manager.ManagesShipments);
			AssertType<JobVoyageShipmentDataObjectWriter>(manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, voyage))));
		}

		public void TestImportDepartureEvent_WithInvalidDepartureArrivalTime_UpdateETDRegardless_DoesNotReportError()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>SailingSchedule</Type>
        </DataTarget>
      </DataTargetCollection>
	</DataContext>

    <EventTime>2025-01-04T10:57:09.973</EventTime>
    <EventType>DEP</EventType>
    <IsEstimate>true</IsEstimate>
	<EventReference>Dummy Description|LOC=AUSYD</EventReference>

    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>LIAM HANRAHAN</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>1100</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>LIAMTH</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion
			ErrorReporter.Clear();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2025, 1, 1);

			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "NZAKL";
			dest1.JB_E_ARV = new ZDateTime(2025, 1, 3);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			voyage.GenerateSailings();
			Factory.SaveForTesting();
			Factory.FireCleanupAfterSaving();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Departure time should be updated regardless", new ZDateTime("2025-01-04T10:57:09.973"), origin.JA_E_DEP);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestImportArrivalEvent_WithInvalidDepartureArrivalTime_UpdateATDRegardless_DoesNotReportError()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>SailingSchedule</Type>
        </DataTarget>
      </DataTargetCollection>
	</DataContext>

    <EventTime>2025-01-01T10:57:09.973</EventTime>
    <EventType>ARV</EventType>
    <IsEstimate>true</IsEstimate>
	<EventReference>Dummy Description|LOC=NZAKL</EventReference>

    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>LIAM HANRAHAN</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>1100</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>LIAMTH</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion
			ErrorReporter.Clear();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;
			voyage.EnableOrphanedVoyageReportingForTests = true;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2025, 1, 2);

			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "NZAKL";
			dest1.JB_E_ARV = new ZDateTime(2025, 1, 3);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			voyage.GenerateSailings();
			Factory.SaveForTesting();
			Factory.FireCleanupAfterSaving();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("Departure time should be updated.", new ZDateTime("2025-01-01T10:57:09.973"), dest1.JB_E_ARV);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Transform

		public void TestVoyageEventTransform_DepartureToStatusUpdated()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>SailingSchedule</Type>
        </DataTarget>
      </DataTargetCollection>
	</DataContext>

    <EventTime>2018-11-13T10:57:09.973</EventTime>
    <EventType>DEP</EventType>
    <IsEstimate>true</IsEstimate>
	<EventReference>Dummy Description|FAC=CTO|LOC=AUSYD</EventReference>

    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>LIAM HANRAHAN</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>1100</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>LIAMTH</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			origin1.JA_A_DEP = ZDateTime.Now;

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_A_DEP = ZDateTime.Now;

			var dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = "HKHKG";

			Factory.SaveForTesting();

			var departureQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
			var departureQueryLogs = voyage.Logs.Find(departureQuery);
			AssertEquals($"Departure event should be added to {voyage.HumanReadableName}", 2, departureQueryLogs.Length);

			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);
			var statusUpdatedLogs = voyage.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should not be added to {voyage.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			departureQueryLogs = voyage.Logs.Find(departureQuery);
			AssertEquals($"No new Departure event was added to {voyage.HumanReadableName}", 2, departureQueryLogs.Length);
			statusUpdatedLogs = voyage.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should be added to {voyage.HumanReadableName}", 1, statusUpdatedLogs.Length);

			var reference = statusUpdatedLogs.First().SL_Reference;
			var parameters = StmALog.GetParametersFromReference(reference);
			Assert("TYP parameter", parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETD rejected")));
			Assert("RES parameter", parameters.Contains(new KeyValuePair<string, string>("RES", "ETD received after ATD")));
			Assert("LOC parameter", parameters.Contains(new KeyValuePair<string, string>("LOC", "AUSYD")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(reference));
		}

		public void TestVoyageEventTransform_ArrivalToStatusUpdated()
		{
			#region XML Message

			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>SailingSchedule</Type>
        </DataTarget>
      </DataTargetCollection>
	</DataContext>

    <EventTime>2018-11-13T10:57:09.973</EventTime>
    <EventType>ARV</EventType>
    <IsEstimate>true</IsEstimate>
	<EventReference>Dummy Description|FAC=CTO|LOC=HKHKG</EventReference>

    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>LIAM HANRAHAN</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>1100</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>LIAMTH</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory.BOFactory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "NZAKL";
			dest1.JB_A_ARV = ZDateTime.Now;

			var dest2 = voyage.Destinations.AddNew();
			dest2.JB_RL_NKPortOfDischarge = "HKHKG";
			dest2.JB_A_ARV = ZDateTime.Now;

			Factory.SaveForTesting();

			var arrivalQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
			var arrivalQueryLogs = voyage.Logs.Find(arrivalQuery);
			AssertEquals($"Arrival event should be added to {voyage.HumanReadableName}", 2, arrivalQueryLogs.Length);

			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);
			var statusUpdatedLogs = voyage.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should not be added to {voyage.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			arrivalQueryLogs = voyage.Logs.Find(arrivalQuery);
			AssertEquals($"No new Arrival event was added to {voyage.HumanReadableName}", 2, arrivalQueryLogs.Length);
			statusUpdatedLogs = voyage.Logs.Find(statusUpdatedQuery);
			AssertEquals($"StatusUpdated event should be added to {voyage.HumanReadableName}", 1, statusUpdatedLogs.Length);

			var reference = statusUpdatedLogs.First().SL_Reference;
			var parameters = StmALog.GetParametersFromReference(reference);
			Assert("TYP parameter", parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETA rejected")));
			Assert("RES parameter", parameters.Contains(new KeyValuePair<string, string>("RES", "ETA received after ATA")));
			Assert("LOC parameter", parameters.Contains(new KeyValuePair<string, string>("LOC", "HKHKG")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(reference));
		}

		#endregion

		#region Implementation

		IEnumerable<string> Format(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues.Select(Format);
		}

		string Format(KeyValuePair<TypeWithDescription, IZType> contextValue)
		{
			return string.Format("{0}|{1}", contextValue.Key.Type, contextValue.Value);
		}

		#endregion
	}
}
