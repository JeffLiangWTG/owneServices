using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDYardUnitStateDataContextManager))]
	public class CYDYardUnitStateDataContextManagerTest : DataContextManagerTestCase<CYDYardUnitStateDataContextManager, CYDYardUnitState>
	{
		public void TestGivenExistingYardUnit_WhenIncomingXUE_ThenMatchByYardUnitID()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.CYDYardUnitState, "CNT0000");
			universalEvent.EventType = Events.GateIn.Code;
			universalEvent.EventTime = new ZDateTimeOffset(2024, 2, 2);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("Expected DCD message status if there is no matching Yard unit", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			universalEvent.DataContext.DataTargetCollection.Single().Key = "CNT1234";
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("Service Task Log", "Linked Event to Yard Unit CNT1234.", serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEquals("Message Log", "Linked Event to Yard Unit CNT1234.", logNoteText);

				var yardUnit = new BusinessObjectFactory().Load<CYDYardUnitState>(gatedInYardUnit.PK);
				var importedEvent = yardUnit.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GateIn.Code).Single();
				AssertEquals(new ZDateTime(2024, 2, 2), importedEvent.SL_EventTime);
			});
		}

		public void TestEventContextValues()
		{
			gatedInYardUnit.GetLogs().AddNew(Events.GateIn);
			var expectedContextValues = new[]
			{
				"ContainerAcceptanceNumber - abcersducewcowivi",
				"GateInTime - 03-Sep-24 10:30:00 +10:00",
				"ContainerNumber - CNT1234",
				"ContainerISOCode - 22G0",
				"IsEmptyContainer - N",
				"ContainerGrossWeight - 24000.000",
				"ContainerWeightUnit - KG",
				"TransportReference - ADCEDS",
				"PortUNLOCO - AUBNE",
				"WarehouseCode - WHS",
				"ClientCode - WHTEST",
			};

			var manager = ((IEventDataContextManagerWithTriggeringLog)gatedInYardUnit.GetUniversalDataContextManager());
			var log = gatedInYardUnit.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.GateIn)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = log;
			var actualContextValues = manager.EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(expectedContextValues, actualContextValues);

			expectedContextValues = new[]
			{
				"ContainerReleaseNumber - releaseNumber",
				"GateOutTime - 06-Sep-24 15:30:00 +10:00",
				"ContainerNumber - CNT5678",
				"ContainerISOCode - 22P1",
				"IsEmptyContainer - Y",
				"ContainerGrossWeight - 30480.000",
				"ContainerWeightUnit - KG",
				"TransportReference - BWEXCWE",
				"PortUNLOCO - AUBNE",
				"WarehouseCode - WHS",
				"ClientCode - WHTEST",
			};

			gatedOutYardUnit.GetLogs().AddNew(Events.GateOut);
			manager = ((IEventDataContextManagerWithTriggeringLog)gatedOutYardUnit.GetUniversalDataContextManager());
			log = gatedOutYardUnit.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.GateOut)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = log;
			actualContextValues = manager.EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(expectedContextValues, actualContextValues);
		}

		public void TestExportGateInEventWithDataContextKey()
		{
			var isEmptyString = gatedInYardUnit.ReceiveAdviceLine.UnitLineItem.YLI_IsEmpty == true ? "true" : "false";

			var logBO = gatedInYardUnit.GetLogs().AddNew(Events.GateIn);
			using (logBO.LockForUpdatingKeyFieldsForTesting())
			{
				logBO.SL_EventTime = ZDateTime.BrettsBirthday;
			}

			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CYDYardUnitState</Type>
          <Key>{gatedInYardUnit.YUS_UnitID}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDateTime.BrettsBirthday.ToOffset(), () => 0)}</EventTime>
    <EventType>GIN</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>{gatedInYardUnit.YUS_UnitID}</Value>
      </Context>
      <Context>
        <Type>ContainerISOCode</Type>
        <Value>{gatedInYardUnit.Container.RC_ISOType}</Value>
      </Context>
      <Context>
        <Type>ContainerGrossWeight</Type>
        <Value>{gatedInYardUnit.Container.RC_GrossWeight}</Value>
      </Context>
      <Context>
        <Type>ContainerWeightUnit</Type>
        <Value>KG</Value>
      </Context>
      <Context>
        <Type>PortUNLOCO</Type>
        <Value>AUBNE</Value>
      </Context>
      <Context>
        <Type>WarehouseCode</Type>
        <Value>WHS</Value>
      </Context>
      <Context>
        <Type>ClientCode</Type>
        <Value>WHTEST</Value>
      </Context>
      <Context>
        <Type>ContainerAcceptanceNumber</Type>
        <Value>{gatedInYardUnit.ReceiveAdviceLine.ReceiveAdvice.YRA_AcceptanceNumber}</Value>
      </Context>
      <Context>
        <Type>GateInTime</Type>
        <Value>{gatedInYardUnit.ReceiveTransportationUnit.YTU_GateInTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")}</Value>
      </Context>
      <Context>
        <Type>IsEmptyContainer</Type>
        <Value>{isEmptyString}</Value>
      </Context>
      <Context>
        <Type>TransportReference</Type>
        <Value>{gatedInYardUnit.ReceiveTransportationUnit.YTU_TransportationReference}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var manager = gatedInYardUnit.GetUniversalDataContextManager() as IEventDataContextManager;
			var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WHS, gatedInYardUnit)));
			var universalEvent = writer.GetDataObject(logBO);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalEvent, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Expected Universal Shipment Message", expectedXml, result);
					}
				}
			}
		}

		public void TestExportGateOutEventWithDataContextKey()
		{
			var isEmptyString = gatedOutYardUnit.ReleaseAdviceLine.UnitLineItem.YLI_IsEmpty == true ? "true" : "false";

			var logBO = gatedOutYardUnit.GetLogs().AddNew(Events.GateOut);
			using (logBO.LockForUpdatingKeyFieldsForTesting())
			{
				logBO.SL_EventTime = ZDateTime.BrettsBirthday;
			}

			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CYDYardUnitState</Type>
          <Key>{gatedOutYardUnit.YUS_UnitID}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDateTime.BrettsBirthday.ToOffset(), () => 0)}</EventTime>
    <EventType>GOU</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>{gatedOutYardUnit.YUS_UnitID}</Value>
      </Context>
      <Context>
        <Type>ContainerISOCode</Type>
        <Value>{gatedOutYardUnit.Container.RC_ISOType}</Value>
      </Context>
      <Context>
        <Type>ContainerGrossWeight</Type>
        <Value>{gatedOutYardUnit.Container.RC_GrossWeight}</Value>
      </Context>
      <Context>
        <Type>ContainerWeightUnit</Type>
        <Value>KG</Value>
      </Context>
      <Context>
        <Type>PortUNLOCO</Type>
        <Value>AUBNE</Value>
      </Context>
      <Context>
        <Type>WarehouseCode</Type>
        <Value>WHS</Value>
      </Context>
      <Context>
        <Type>ClientCode</Type>
        <Value>WHTEST</Value>
      </Context>
      <Context>
        <Type>ContainerReleaseNumber</Type>
        <Value>{gatedOutYardUnit.ReleaseAdviceLine.ReleaseAdvice.YRE_ReleaseNumber}</Value>
      </Context>
      <Context>
        <Type>GateOutTime</Type>
        <Value>{gatedOutYardUnit.DispatchTransportationUnit.YTU_GateOutTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")}</Value>
      </Context>
      <Context>
        <Type>IsEmptyContainer</Type>
        <Value>{isEmptyString}</Value>
      </Context>
      <Context>
        <Type>TransportReference</Type>
        <Value>{gatedOutYardUnit.DispatchTransportationUnit.YTU_TransportationReference}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var manager = gatedOutYardUnit.GetUniversalDataContextManager() as IEventDataContextManager;
			var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WHS, gatedOutYardUnit)));
			var universalEvent = writer.GetDataObject(logBO);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalEvent, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Expected Universal Shipment Message", expectedXml, result);
					}
				}
			}
		}

		public void TestGivenExistingYardUnit_WhenIncoming_ThenTriggerGINLog()
		{
			gatedInYardUnit.GetLogs().AddNew(Events.GateIn);
			var expectedContextValues = new[]
			{
				"ContainerAcceptanceNumber - abcersducewcowivi",
				"GateInTime - 03-Sep-24 10:30:00 +10:00",
				"ContainerNumber - CNT1234",
				"ContainerISOCode - 22G0",
				"IsEmptyContainer - N",
				"ContainerGrossWeight - 24000.000",
				"ContainerWeightUnit - KG",
				"TransportReference - ADCEDS",
				"PortUNLOCO - AUBNE",
				"WarehouseCode - WHS",
				"ClientCode - WHTEST",
			};

			var manager = (IEventDataContextManagerWithTriggeringLog)gatedInYardUnit.GetUniversalDataContextManager();
			var log = gatedInYardUnit.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.GateIn)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = log;
			var actualContextValues = manager.EventContextValues.Select(e => $"{e.Key.Type} - {e.Value}");
			var actualContextValuesArray = actualContextValues.ToArray();

			AssertEquals("Should have 11 lines of GIN log.", 11, actualContextValuesArray.Length);
			AssertEquals("Should have a GIN event type.", EventCodes.GateIn, manager.TriggeringLogForUseInPopulatingEventContext.SL_SE_NKEvent);
			AssertContains("Should have a 'GateInTime' element in GIN event.", nameof(UniversalEvent.ContextTypes.GateInTime) + " - 03-Sep-24 10:30:00 +10:00", actualContextValuesArray[8]);
			AssertContainsExactElementsInAnyOrder("Should have exact triggering GIN log.", expectedContextValues, actualContextValues);
		}

		public void TestGivenExistingYardUnit_WhenOutgoing_ThenTriggerGOULog()
		{
			gatedOutYardUnit.GetLogs().AddNew(Events.GateOut);
			var expectedContextValues = new[]
			{
				"ContainerReleaseNumber - releaseNumber",
				"GateOutTime - 06-Sep-24 15:30:00 +10:00",
				"ContainerNumber - CNT5678",
				"ContainerISOCode - 22P1",
				"IsEmptyContainer - Y",
				"ContainerGrossWeight - 30480.000",
				"ContainerWeightUnit - KG",
				"TransportReference - BWEXCWE",
				"PortUNLOCO - AUBNE",
				"WarehouseCode - WHS",
				"ClientCode - WHTEST",
			};

			var manager = (IEventDataContextManagerWithTriggeringLog)gatedOutYardUnit.GetUniversalDataContextManager();
			var log = gatedOutYardUnit.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.GateOut)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = log;
			var actualContextValues = manager.EventContextValues.Select(e => $"{e.Key.Type} - {e.Value}");
			var actualContextValuesArray = actualContextValues.ToArray();

			AssertEquals("Should have 11 lines of GOU log.", 11, actualContextValuesArray.Length);
			AssertEquals("Should have a GOU event type.", EventCodes.GateOut, manager.TriggeringLogForUseInPopulatingEventContext.SL_SE_NKEvent);
			AssertContains("Should have a 'GateOutTime' element in GOU event.", nameof(UniversalEvent.ContextTypes.GateOutTime) + " - 06-Sep-24 15:30:00 +10:00", actualContextValuesArray[8]);
			AssertContainsExactElementsInAnyOrder("Should have exact triggering GOU log.", expectedContextValues, actualContextValues);
		}

		public void TestGivenExistingYardUnit_WhenNotGINGOU_ThenNoGINGOUTriggeringLog()
		{
			gatedInYardUnit.GetLogs().AddNew(Events.ChangeOfIdentifier);
			var expectedContextValues = new[]
			{
				"ContainerNumber - CNT1234",
				"ContainerISOCode - 22G0",
				"ContainerGrossWeight - 24000.000",
				"ContainerWeightUnit - KG",
				"PortUNLOCO - AUBNE",
				"WarehouseCode - WHS",
				"ClientCode - WHTEST",
			};

			var manager = (IEventDataContextManagerWithTriggeringLog)gatedInYardUnit.GetUniversalDataContextManager();
			var log = gatedInYardUnit.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, EventCodes.ChangeOfIdentifier)).Single();
			manager.TriggeringLogForUseInPopulatingEventContext = log;
			var actualContextValues = manager.EventContextValues.Select(e => $"{e.Key.Type} - {e.Value}");

			AssertEquals("Should have 7 lines of log.", 7, actualContextValues.ToArray().Length);
			AssertEquals("Should not have a GIN or GOU event type.", EventCodes.ChangeOfIdentifier, manager.TriggeringLogForUseInPopulatingEventContext.SL_SE_NKEvent);
			AssertNotContains("Should not have a GateInTime element in a non GIN event.", nameof(UniversalEvent.ContextTypes.GateInTime), actualContextValues.ToString());
			AssertNotContains("Should not have a GateOutTime element in a non GOU event.", nameof(UniversalEvent.ContextTypes.GateOutTime), actualContextValues.ToString());
			AssertContainsExactElementsInAnyOrder("Should have default triggering log when not a GIN or GOU event.", expectedContextValues, actualContextValues);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CYDYardUnitState does not implement IJobNumber", true);
		}

		protected override CYDYardUnitState GetNewBusinessObjectForTesting()
		{
			return gatedInYardUnit;
		}

		#region Implementation

		CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory.BOFactory)); }
		}

		CYDYardTestHelper helper;

		CYDYardUnitState gatedInYardUnit;
		CYDYardUnitState gatedOutYardUnit;

		protected override void SetUp()
		{
			base.SetUp();

			var client = Helper.CreateClient();
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var recieveAdvice = Helper.CreateReceiveAdvice(client, yard);
			recieveAdvice.YRA_AcceptanceNumber = "abcersducewcowivi";
			
			gatedInYardUnit = Helper.AddReceiveAdviceLine(recieveAdvice, "CNT1234", "20GP", false);
			gatedOutYardUnit = Helper.AddReceiveAdviceLine(recieveAdvice, "CNT5678", "20FR", true);

			var receiveTransportationUnit = Helper.CreateTransportationUnit("ADCEDS");
			Helper.AddDelivery(receiveTransportationUnit, gatedInYardUnit);
			Helper.AddDelivery(receiveTransportationUnit, gatedOutYardUnit);
			Helper.GateInTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(2024, 9, 3, 10, 30, 0), location);
			Helper.GateOutTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(2024, 9, 3, 12, 30, 0));

			var releaseAdvice = Helper.CreateReleaseAdvice(client, yard);
			releaseAdvice.YRE_ReleaseNumber = "releaseNumber";
			Helper.AddReleaseAdviceLine(releaseAdvice, gatedInYardUnit);
			Helper.AddReleaseAdviceLine(releaseAdvice, gatedOutYardUnit);

			var dispatchTransportationUnit = Helper.CreateTransportationUnit("BWEXCWE");
			Helper.AddPickup(dispatchTransportationUnit, gatedOutYardUnit);
			Helper.GateInTransportationUnit(dispatchTransportationUnit, new ZDateTimeOffset(2024, 9, 6, 12, 30, 0), location);
			Helper.GateOutTransportationUnit(dispatchTransportationUnit, new ZDateTimeOffset(2024, 9, 6, 15, 30, 0));

			Factory.SaveForTesting();
		}

		#endregion
	}
}
