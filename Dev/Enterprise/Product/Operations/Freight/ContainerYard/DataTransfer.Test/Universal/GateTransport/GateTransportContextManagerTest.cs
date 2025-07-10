using System.Collections;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.ContainerYard.Business.Testing
{
	[TestedType(typeof(GateTransportContextManager))]
	sealed class GateTransportContextManagerTest : DataContextManagerTestCase<GateTransportContextManager, GateTransport>
	{
		public void TestExportGateTransportEventWithDataContextKey()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = "Tm2203";
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;

			Factory.SaveForTesting();

			var logBO = gateTransport.GetLogs().AddNew(Events.FreightUnloaded);
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
          <Type>GateTransport</Type>
          <Key>{gateTransport.GTT_JobNumber}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDateTime.BrettsBirthday.ToOffset(), () => 0)}</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>TransportReference</Type>
        <Value>{gateTransport.GTT_VehicleRegistration}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var manager = gateTransport.GetUniversalDataContextManager() as IEventDataContextManager;
			var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WHS, gateTransport)));
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

		public void TestExportGateTransportEventWithDataContextKey_WhenGateTransportHasCFSDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var yardUnit = Factory.NewWithValidTestData<YardUnitPersistentForTesting>();
			yardUnit.GTY_UnitNumber = "ULD012021B";

			var gateBooking = Factory.NewWithValidTestData<GateBooking>();

			var whsWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var bookingDetail = Factory.New<GateBookingDetail>();
			bookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
			bookingDetail.GTD_WW_Facility = whsWarehouse.PK;
			bookingDetail.GTD_GTB_GateBooking = gateBooking.PK;

			bookingDetail.GTD_OA_BookingPartyAddress = org.MainAddress.PK;

			Factory.SaveForTesting();

			var gateTransportCFSDetail = Factory.NewWithValidTestData<GateTransportCFSDetail>();
			gateTransportCFSDetail.GTF_GTD_GateBookingDetail = bookingDetail.PK;

			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = string.Empty;
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;
			gateTransport.GateTransportCFSDetails.Add(gateTransportCFSDetail);

			Factory.SaveForTesting();

			var logBO = gateTransport.GetLogs().AddNew(Events.FreightUnloaded);
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
          <Type>GateTransport</Type>
          <Key>{gateTransport.GTT_JobNumber}</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>{SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDateTime.BrettsBirthday.ToOffset(), () => 0)}</EventTime>
    <EventType>FUL</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>{yardUnit.GTY_UnitNumber}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var manager = gateTransport.GetUniversalDataContextManager() as IEventDataContextManager;
			var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WHS, gateTransport)));
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

		public void TestImportGateTransportEventWithDataContextKey()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = "Tm2203";
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;

			Factory.SaveForTesting();

			var eventXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>GateTransport</Type>
          <Key>{gateTransport.GTT_JobNumber}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>{gateTransport.GTT_TimeIn.SqlFormat.Replace(" ", "T")}</EventTime>
    <EventType>{Events.FreightLoadedCode}</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>TransportReference</Type>
        <Value>{gateTransport.GTT_VehicleRegistration}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to GateTransport.".Trim(), serviceTaskLog.ToString());
			var gateTransportLogs = gateTransport.Logs.GetAllLogs();
			AssertEquals(1, gateTransportLogs.Count);
			AssertEquals(Events.FreightLoadedCode, gateTransportLogs[0].SL_SE_NKEvent);
		}

		public void TestImportGateTransportEventWithDataContextKey_WhenGateTransportWithSpecifiedJobNumberDoesNotExist()
		{
			var eventXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
			<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <Event>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>GateTransport</Type>
			          <Key>RYK7OIF0CDYKJR0MN0I0</Key>
			        </DataTarget>
			      </DataTargetCollection>
			    </DataContext>

			    <EventTime>1971-09-18T00:00:00</EventTime>
			    <EventType>{Events.FreightLoadedCode}</EventType>
			    <IsEstimate>false</IsEstimate>

			    <ContextCollection>
			      <Context>
			        <Type>TransportReference</Type>
			        <Value>Tm2205</Value>
			      </Context>
			    </ContextCollection>
			  </Event>
			</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Warning - No Module found a Business Entity to link this Universal Event to.".Trim(), serviceTaskLog.ToString());

			var gateTransports = Factory.Load<GateTransport>(new ZQuery(GateTransportSchema.GTT_JobNumber, "RYK7OIF0CDYKJR0MN0I0"));
			AssertEquals(0, gateTransports.Length);
		}

		public void TestImportGateTransportEventWithDataContextKey_WhenGateTransportWithSpecifiedJobNumberDoesNotExist_ButGateTransportWithVehicleRegistrationNumberExists()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = "Tm2205";
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;

			Factory.SaveForTesting();

			var eventXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
			<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <Event>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>GateTransport</Type>
			          <Key>RYK7OIF0CDYKJR0MN0I0</Key>
			        </DataTarget>
			      </DataTargetCollection>
			    </DataContext>

			    <EventTime>1971-09-18T00:00:00</EventTime>
			    <EventType>{Events.FreightLoadedCode}</EventType>
			    <IsEstimate>false</IsEstimate>

			    <ContextCollection>
			      <Context>
			        <Type>TransportReference</Type>
			        <Value>Tm2205</Value>
			      </Context>
			    </ContextCollection>
			  </Event>
			</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to GateTransport.".Trim(), serviceTaskLog.ToString());
			var gateTransportLogs = gateTransport.Logs.GetAllLogs();
			AssertEquals(1, gateTransportLogs.Count);
			AssertEquals(Events.FreightLoadedCode, gateTransportLogs[0].SL_SE_NKEvent);
		}

		public void TestProcessingFULEventShouldAddFULEventToCorrespondingGateTransport()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = "Tm2203";
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(GateTransportEventParentFinderTest.UXmlCargoTruckEventSampleMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals(@"Linked Event to GateTransport.", serviceTaskLog.ToString());
				var gateTransportLogs = gateTransport.Logs.GetAllLogs();
				AssertEquals(1, gateTransportLogs.Count);
				AssertEquals(Events.FreightUnloadedCode, gateTransportLogs[0].SL_SE_NKEvent);
			});
		}

		public void TestProcessingFLOEventShouldAddFLOEventToCorrespondingGateTransport()
		{
			var transportDetail = Factory.NewWithValidTestData<GateTransportCYDetail>();

			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = string.Empty;
			gateTransport.GTT_TimeIn = ZDateTime.BrettsBirthday;
			gateTransport.GTT_TimeOut = ZDateTime.Empty;

			gateTransport.GateTransportCYDetails.Add(transportDetail);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(GateTransportEventParentFinderTest.UXmlContainerEventSampleMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals(@"Linked Event to GateTransport.", serviceTaskLog.ToString());
				var gateTransportLogs = gateTransport.Logs.GetAllLogs();
				AssertEquals(1, gateTransportLogs.Count);
				AssertEquals(Events.FreightLoadedCode, gateTransportLogs[0].SL_SE_NKEvent);
			});
		}

		public void TestLoadGateTransportContextManager()
		{
			var types = (Hashtable)ObjectFactory.Get("UniversalDataContextManagers");
			var objectHandle = (ObjectHandle)types["GateTransport"];
			AssertEquals(typeof(GateTransportContextManager), objectHandle.GetObjectType());
		}

		public void TestDataContextType()
		{
			AssertEquals(DataContextType.GateTransport, GetNewDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			var gateTransport = GetNewBusinessObjectForTesting();
			gateTransport.GTT_JobNumber = "GTT20002";

			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(gateTransport);

			AssertEquals(gateTransport.GTT_JobNumber, manager.DataContextKey);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(GetNewDataContextManager().DefaultOutputDirectory);
		}

		public void TestEventContextValues_ContainsCFSContainers()
		{
			var gateTransport = CreateGateTransportWithCFSDetail();

			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(gateTransport);

			var contextValues = manager.EventContextValues.ToList();

			AssertEquals(2, contextValues.Count);

			AssertEquals(nameof(Event.ContextTypes.ContainerNumber), contextValues[0].Key.ToString());
			AssertEquals(gateTransport.GateTransportCFSDetails[0].GateBookingDetail.YardUnit.GTY_UnitNumber, contextValues[0].Value);

			AssertEquals(nameof(Event.ContextTypes.ContainerNumber), contextValues[1].Key.ToString());
			AssertEquals(gateTransport.GateTransportCFSDetails[1].GateBookingDetail.YardUnit.GTY_UnitNumber, contextValues[1].Value);
		}

		public void TestEventContextValues_ContainsVehicleRegistrationNumber()
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = "12B34566";

			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(gateTransport);

			var contextValues = manager.EventContextValues.ToList();

			AssertEquals(1, contextValues.Count);

			AssertEquals(nameof(Event.ContextTypes.TransportReference), contextValues[0].Key.ToString());
			AssertEquals(gateTransport.GTT_VehicleRegistration, contextValues[0].Value);
		}

		#region Implementation

		protected override string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
		{
			return null;
		}

		GateTransportContextManager GetNewDataContextManager()
		{
			return new GateTransportContextManager();
		}

		protected override GateTransport GetNewBusinessObjectForTesting()
		{
			return Factory.New<GateTransport>();
		}

		GateTransport CreateGateTransportWithCFSDetail()
		{
			var firstTransportDetail = CreateCFSDetail("HYMU7875674");
			var secondTransportDetail = CreateCFSDetail("EGHU2008392");

			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GateTransportCFSDetails.Add(firstTransportDetail);
			gateTransport.GateTransportCFSDetails.Add(secondTransportDetail);

			return gateTransport;
		}

		GateTransportCFSDetail CreateCFSDetail(string containerNumber)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var yardUnit = Factory.NewWithValidTestData<YardUnitPersistentForTesting>();
			yardUnit.GTY_UnitNumber = containerNumber;

			var gateBooking = Factory.NewWithValidTestData<GateBooking>();

			var whsWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var bookingDetail = Factory.New<GateBookingDetail>();
			bookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
			bookingDetail.GTD_WW_Facility = whsWarehouse.PK;
			bookingDetail.GTD_GTB_GateBooking = gateBooking.PK;

			bookingDetail.GTD_OA_BookingPartyAddress = org.MainAddress.PK;

			var gateTransportCFSDetail = Factory.NewWithValidTestData<GateTransportCFSDetail>();
			gateTransportCFSDetail.GTF_GTD_GateBookingDetail = bookingDetail.PK;

			return gateTransportCFSDetail;
		}

		#endregion
	}
}
