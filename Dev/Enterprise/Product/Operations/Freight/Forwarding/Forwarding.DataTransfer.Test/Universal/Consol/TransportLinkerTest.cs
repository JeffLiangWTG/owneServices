using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class TransportLinkerTest : TestCaseWithFactory
	{
		public void TestProcessUniversalEventByUMIServiceTask_UsingFirstOrDefaultMatch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			var leg0 = consol.Transports[0];
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("PRE: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
				AssertEquals("PRE: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
				AssertEquals("PRE: Consol should have 3 legs", 3, consol.Transports.Count);
				AssertEquals("PRE: Consol should have 4 events", 4, consol.Logs.GetAllLogs().Count);
				AssertEquals("PRE: Transport Leg #0 number count should be 0", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("PRE: Transport Leg #1 number count should be 0", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("PRE: Transport Leg #2 number count should be 0", 0, consol.Transports[2].Logs.GetAllLogs().Count);
			});

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = @"<UniversalEvent>
    <Event>
        <EventType>CCD</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>6852S</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>
";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();

			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: Message should be recognized", "PRS", message.EM_Status);
				AssertEquals("CHK: Consol event number count should be 4", 4, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #0 event number count should be 1", 1, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count should be 0", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 event number count should be 0", 0, consol.Transports[2].Logs.GetAllLogs().Count);
			});

			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: Message should be recognized", "PRS", message.EM_Status);
				AssertEquals("CHK: Consol event number count should be 4", 4, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #0 event number count should be 1", 1, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count should be 1", 1, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 event number count should be 0", 0, consol.Transports[2].Logs.GetAllLogs().Count);
			});

			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: Message should be recognized", "PRS", message.EM_Status);
				AssertEquals("CHK: Transport Leg #0 event number count should be 1", 1, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count should be 1", 1, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 event number count should be 1", 1, consol.Transports[2].Logs.GetAllLogs().Count);
			});

			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: Message should be recognized", "PRS", message.EM_Status);
				AssertEquals("CHK: Transport Leg #0 event number count should be 2", 2, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count should be 1", 1, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 event number count should be 1", 1, consol.Transports[2].Logs.GetAllLogs().Count);
			});

			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: Message should be recognized", "PRS", message.EM_Status);
				AssertEquals("CHK: Transport Leg #0 event number count should be 3", 3, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count should be 1", 1, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 event number count should be 1", 1, consol.Transports[2].Logs.GetAllLogs().Count);
			});
		}

		public void TestProcessUniversalEventByUMIServiceTask_UsingUNLOCOMatch_DEPMatchBothPortsAndFlightDate_EventReference()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var sailing = CreateSailing("QF123", "AUSYD", "CNFOC", new ZDateTime(2016, 4, 01));

			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			consol.JK_UniqueConsignRef = "C00009999";
			((ISupportDataImporting)consol).IsImportingData = true;

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNFOC";
			leg0.JW_ETD = new ZDateTime(2016, 3, 10, 22, 40, 0);

			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNFOC";
			leg1.JW_RL_NKDiscPort = "CNNKG";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNNKG";
			leg2.JW_RL_NKDiscPort = "CNSHP";

			Factory.Save();

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText
			message.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>C00009999</Key>
				</DataTarget>
			</DataTargetCollection>
			<DataProvider>FSU Message</DataProvider>
		</DataContext>
		<EventType>DEP</EventType>
		<EventTime>2016-02-26T14:11:59.6059548</EventTime>
		<IsEstimate>false</IsEstimate>
		<EventReference>|FAC=CTO|FDT=2016-03-10|LOC=AUSYD|VFL=QF123</EventReference>
	</Event>
</UniversalEvent>
";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
			AssertEquals("Pre: Transport Leg #0 Logs count", 1, consol.Transports[0].Logs.GetAllLogs().Count);
			AssertEquals("Pre: Transport Leg #1 Logs count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
			AssertEquals("Pre: Transport Leg #2 Logs count", 0, consol.Transports[2].Logs.GetAllLogs().Count);

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);

			AssertEquals("Transport Leg #0 Logs count", 2, consol.Transports[0].Logs.GetAllLogs().Count);
			AssertEquals("Transport Leg #1 Logs count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
			AssertEquals("Transport Leg #2 Logs count", 0, consol.Transports[2].Logs.GetAllLogs().Count);

			var transportlog = consol.Transports[0].Logs.GetAllLogs();
			AssertEquals("SL_Reference", "Changed To: 10-Mar-16|FAC=CTO|FDT=2016-03-10|LOC=AUSYD|MOD=AIR|VFL=QF123", transportlog[0].SL_Reference);
			AssertEquals("SL_Reference", "|FAC=CTO|FDT=2016-03-10|LOC=AUSYD|VFL=QF123", transportlog[1].SL_Reference);
		}

		public void TestProcessUniversalEventByUMIServiceTask_UsingUNLOCOMatch_DEPMatchBothPortsAndFlightNumberDate()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var sailing = CreateSailing("QF123", "AUSYD", "CNFOC", new ZDateTime(2016, 4, 01));

			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			((ISupportDataImporting)consol).IsImportingData = true;

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNFOC";
			leg0.JW_ETA = new ZDateTime(2016, 3, 10, 01, 40, 0);
			leg0.JW_ETD = new ZDateTime(2016, 3, 10, 22, 40, 0);

			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNFOC";
			leg1.JW_RL_NKDiscPort = "CNNKG";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNNKG";
			leg2.JW_RL_NKDiscPort = "CNSHP";

			Factory.Save();

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText
			message.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>FSU Message</DataProvider>
		</DataContext>
		<EventType>DEP</EventType>
		<EventTime>2016-02-26T14:11:59.6059548</EventTime>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-12347893</Value>
			</Context>
			<Context>
				<Type>MAWBNumberOfPieces</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>SourceEventCode</Type>
				<Value>DEP</Value>
			</Context>
			<Context>
				<Type>NumberOfPieces</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>WeightOfGoods</Type>
				<Value>109KG</Value>
			</Context>
			<Context>
				<Type>IATACarrierCode</Type>
				<Value>LH</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>QF123</Value>
			</Context>
			<Context>
				<Type>FlightDate</Type>
				<Value>2016-03-10</Value>
			</Context>
			<Context>
				<Type>OriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>DestinationIATAAirportCode</Type>
				<Value>FOC</Value>
			</Context>
			<Context>
				<Type>LegOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>LegDestinationUNLOCO</Type>
				<Value>CNNKG</Value>
			</Context>
			<Context>
				<Type>TimeOfDeparture</Type>
				<Value>03-Mar-2016 22:00</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
				<AdditionalFieldsToUpdate>
					<Type>JobConsolTransport.JW_ETD</Type>
					<Value>2016-03-11T14:40:00.000</Value>
				</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);

			AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
			AssertEquals("CHK: ETD for Transport Leg #0 should be updated", "11-Mar-16 14:40:00", consol.Transports[0].JW_ETD.ToString());
			AssertEquals("CHK: ETA for Transport Leg #0 should be no change", "10-Mar-16 01:40:00", consol.Transports[0].JW_ETA.ToString());
			AssertEquals(ZString.Empty, consol.Transports[1].JW_ETA.ToString());
			AssertEquals(ZString.Empty, consol.Transports[1].JW_ETA.ToString());
			AssertEquals(ZString.Empty, consol.Transports[2].JW_ETA.ToString());
			AssertEquals(ZString.Empty, consol.Transports[2].JW_ETA.ToString());
		}

		public void TestProcessUniversalEventByUMIServiceTask_UsingUNLOCOMatch_MatchFlightNumberOnly()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var sailing = CreateSailing("QF123", "AUSYD", "CNFOC", new ZDateTime(2016, 4, 01));

			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			((ISupportDataImporting)consol).IsImportingData = true;

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNFOC";
			leg0.JW_ETA = new ZDateTime(2016, 3, 10, 01, 40, 0);
			leg0.JW_ETD = new ZDateTime(2016, 3, 10, 22, 40, 0);

			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNFOC";
			leg1.JW_RL_NKDiscPort = "CNNKG";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNNKG";
			leg2.JW_RL_NKDiscPort = "CNSHP";

			Factory.Save();

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			#region message.EM_MessageText

			message.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>FSU Message</DataProvider>
		</DataContext>
		<EventType>DEP</EventType>
		<EventTime>2016-02-26T14:11:59.6059548</EventTime>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-12347893</Value>
			</Context>
			<Context>
				<Type>MAWBNumberOfPieces</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>SourceEventCode</Type>
				<Value>DEP</Value>
			</Context>
			<Context>
				<Type>IATACarrierCode</Type>
				<Value>LH</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>QF123</Value>
			</Context>
			<Context>
				<Type>OriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>DestinationIATAAirportCode</Type>
				<Value>FOC</Value>
			</Context>
			<Context>
				<Type>LegOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>LegDestinationUNLOCO</Type>
				<Value>CNNKG</Value>
			</Context>
			<Context>
				<Type>TimeOfDeparture</Type>
				<Value>03-Mar-2016 22:00</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
				<AdditionalFieldsToUpdate>
					<Type>JobConsolTransport.JW_ETD</Type>
					<Value>2016-03-11T14:40:00.000</Value>
				</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			#endregion

			Factory.Save();

			AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
			AssertEquals("Pre: Transport Leg #0 Logs count", 2, consol.Transports[0].Logs.GetAllLogs().Count);
			AssertEquals("Pre: Transport Leg #1 Logs count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
			AssertEquals("Pre: Transport Leg #2 Logs count", 0, consol.Transports[2].Logs.GetAllLogs().Count);

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);

			var consolLogs = consol.Logs.GetAllLogs();
			AssertEquals("DEP Event should created in Consol Logs", consolLogs[consolLogs.Count - 1].SL_Parent, consol.PK);

			AssertEquals("Transport Leg #0 Logs count", 2, consol.Transports[0].Logs.GetAllLogs().Count);
			AssertEquals("Transport Leg #1 Logs count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
			AssertEquals("Transport Leg #2 Logs count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
		}

		public void TestProcessUniversalEventByUMIServiceTask_UsingUNLOCOMatch_MatchByLoadPortWhenNoMatchDiscPort()
		{
			var sailing0 = CreateSailing("QF123", "AUSYD", "CNNKG", new ZDateTime(2012, 3, 22));
			var sailing1 = CreateSailing("QF123", "CNNKG", "CNFOC", new ZDateTime(2012, 3, 23));
			var sailing2 = CreateSailing("QF123", "CNFOC", "CNSHP", new ZDateTime(2012, 3, 24));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing0.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			leg0.JW_ETD = new ZDateTime(2012, 3, 22);

			var leg1 = consol.Transports.AddNew();
			leg1.JW_IsLinked = true;
			leg1.JW_JX = sailing1.PK;
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_IsLinked = true;
			leg2.JW_JX = sailing2.PK;
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			Factory.Save();

			AssertEquals("PRE: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
			AssertEquals("PRE: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
			AssertEquals("PRE: Consol should have 3 legs", 3, consol.Transports.Count);

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText
			message.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<EventType>DEP</EventType>
		<EventTime>2012-04-12T14:11:59.6059548</EventTime>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>081-12347893</Value>
			</Context>
			<Context>
				<Type>MAWBNumberOfPieces</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>SourceEventCode</Type>
				<Value>DEP</Value>
			</Context>
			<Context>
				<Type>NumberOfPieces</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>WeightOfGoods</Type>
				<Value>109KG</Value>
			</Context>
			<Context>
				<Type>IATACarrierCode</Type>
				<Value>LH</Value>
			</Context>
			<Context>
				<Type>FlightNumber</Type>
				<Value>QF123</Value>
			</Context>
			<Context>
				<Type>FlightDate</Type>
				<Value>2012-03-22</Value>
			</Context>
			<Context>
				<Type>OriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>DestinationIATAAirportCode</Type>
				<Value>AKL</Value>
			</Context>
			<Context>
				<Type>LegOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>LegDestinationUNLOCO</Type>
				<Value>NZAKL</Value>
			</Context>
			<Context>
				<Type>TimeOfDeparture</Type>
				<Value>A 22-Mar-2012 22:00</Value>
			</Context>
		</ContextCollection>
		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobConsolTransport.JW_ATD</Type>
				<Value>2012-04-12T14:11:59.8403643</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			AssertEquals("CHK: ATD for Transport Leg #0 should be updated", "12-Apr-12 14:11:59", consol.Transports[0].JW_ATD.ToString());
			AssertNotEquals("CHK: ATD for Transport Leg #1 should not be updated", "12-Apr-12 14:11:59", consol.Transports[1].JW_ATD.ToString());
			AssertNotEquals("CHK: ATD for Transport Leg #2 should not be updated", "12-Apr-12 14:11:59", consol.Transports[2].JW_ATD.ToString());
		}

		public void TestProcessUniversalEventByUMIServiceTask_UsingUNLOCOMatch_DEPMatchOnLoadPort()
		{
			var sailing0 = CreateSailing("QF123", "AUSYD", "CNNKG", new ZDateTime(2012, 3, 22));
			var sailing1 = CreateSailing("QF123", "CNNKG", "CNFOC", new ZDateTime(2012, 3, 23));
			var sailing2 = CreateSailing("QF123", "CNFOC", "CNSHP", new ZDateTime(2012, 3, 24));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing0.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			leg0.JW_ETA = new ZDateTime(2012, 3, 22);
			leg0.JW_ETD = new ZDateTime(2012, 3, 23);

			var leg1 = consol.Transports.AddNew();
			leg1.JW_IsLinked = true;
			leg1.JW_JX = sailing1.PK;
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			leg1.JW_ETA = new ZDateTime(2012, 3, 23);
			leg1.JW_ETD = new ZDateTime(2012, 3, 24);

			var leg2 = consol.Transports.AddNew();
			leg2.JW_IsLinked = true;
			leg2.JW_JX = sailing2.PK;
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			leg2.JW_ETA = new ZDateTime(2012, 3, 24);
			leg2.JW_ETD = new ZDateTime(2012, 3, 25);
			Factory.Save();

			AssertEquals("Pre: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
			AssertEquals("Pre: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
			AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF123</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-23</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>SYD</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>FOC</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>AUSYD</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNFOC</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>
";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			AssertEquals("CHK: ATD for Transport Leg #0 should be updated", "12-Apr-12 14:11:59", consol.Transports[0].JW_ATD.ToString());
			AssertEquals("CHK: ATD for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATD.ToString());
			AssertEquals("CHK: ATD for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATD.ToString());
		}

		public void TestFSUMessage_WithoutMatchingPortSkipDefaultMatchingResultInNoEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			var leg0 = consol.Transports[0];
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Pre: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
				AssertEquals("Pre: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
				AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
				AssertEquals("CHK: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 4, consol.Logs.GetAllLogs().Count);
			});

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText =
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>6852S</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>AKL</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>AKL</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>NZAKL</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>NZAKL</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>
				";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATD.ToString());
				AssertEquals("CHK: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 5, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: EDIMessage should be recognised", "WAR", message.EM_Status);
			});
		}

		public void TestFSUMessage_WithoutMatchingPortUNLOCOAndNotMatchIATASkipDefaultMatchingResultInNoEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			var leg0 = consol.Transports[0];
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Pre: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
				AssertEquals("Pre: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
				AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
				AssertEquals("CHK: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 4, consol.Logs.GetAllLogs().Count);
			});

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText =
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>6852S</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>AKL</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>FOC</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATD.ToString());
				AssertEquals("CHK: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #2 number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 5, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: EDIMessage should be recognised", "WAR", message.EM_Status);
			});
		}

		public void TestFSUMessage_WithoutMatchingPortUNLOCOButMatchIATAWithPortSwitchMatching()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region BuildingConsolForTesting
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsNeutralMaster = false;
				consol.JK_TransportMode = "AIR";
				consol.MasterBillAirlinePrefix = "081";
				consol.MasterBillMAWB = "12347893";
				var leg0 = consol.Transports[0];
				leg0.JW_TransportMode = "AIR";
				leg0.JW_LegOrder = 1;
				leg0.JW_TransportType = "FL1";
				leg0.JW_VoyageFlight = "QF123";
				leg0.JW_RL_NKLoadPort = "AUSYD";
				leg0.JW_RL_NKDiscPort = "CNNKG";
				var leg1 = consol.Transports.AddNew();
				leg1.JW_TransportMode = "AIR";
				leg1.JW_LegOrder = 2;
				leg1.JW_TransportType = "FL2";
				leg1.JW_VoyageFlight = "QF123";
				leg1.JW_RL_NKLoadPort = "CNNKG";
				leg1.JW_RL_NKDiscPort = "CNFOC";
				var leg2 = consol.Transports.AddNew();
				leg2.JW_TransportMode = "AIR";
				leg2.JW_LegOrder = 3;
				leg2.JW_TransportType = "FL3";
				leg2.JW_VoyageFlight = "QF123";
				leg2.JW_RL_NKLoadPort = "CNFOC";
				leg2.JW_RL_NKDiscPort = "CNSHP";
				leg2.JW_ETD = new ZDateTime(2012, 3, 22);
				Factory.Save();
				CreateWorkflowExceptions();
				#endregion

				CombineAssertions(() =>
				{
					AssertEquals("Pre: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
					AssertEquals("Pre: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
					AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);
					AssertEquals("CHK: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #1 number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #2 number count", 1, consol.Transports[2].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Consol number count", 7, consol.Logs.GetAllLogs().Count);
				});

				var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				message.EM_MessageSubType = "XUE";
				message.EM_ReceiveTransmit = EDIMessage.Status.Received;
				message.EM_Status = EDIMessage.Status.Queued;
				message.EM_GB = GlbBranch.CurrentBranch.PK;
				#region message.EM_MessageText =
				message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF123</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>FOC</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>XXXXX</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
				#endregion
				message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				var logger = new SimpleLogger();
				new UniversalMessageProcessingManager(logger).Process(message);
				CombineAssertions(() =>
				{
					AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
					AssertEquals("CHK: ATD for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATD.ToString());
					AssertEquals("CHK: ATD for Transport Leg #2 should be updated", "12-Apr-12 14:11:59", consol.Transports[2].JW_ATD.ToString());
					AssertEquals("CHK: Transport Leg #0 logs number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #1 logs number count", 0, consol.Transports[1].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #2 logs number count", 2, consol.Transports[2].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Consol number count", 8, consol.Logs.GetAllLogs().Count);
					AssertEquals("CHK: EDIMessage should be recognised", "WAR", message.EM_Status);
				});
			}
		}

		public void TestFSUMessage_PortSwitchMatching()
		{
			#region BuildingConsolForTesting
			var sailing0 = CreateSailing("QF123", "AUSYD", "CNNKG", new ZDateTime(2012, 3, 22));
			var sailing1 = CreateSailing("QF223", "CNNKG", "CNFOC", new ZDateTime(2012, 3, 23));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";

			var leg0 = consol.Transports[0];
			leg0.JW_IsLinked = true;
			leg0.JW_JX = sailing0.PK;
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";

			var leg1 = consol.Transports.AddNew();
			leg1.JW_IsLinked = true;
			leg1.JW_JX = sailing1.PK;
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF223";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			leg1.JW_Vessel = "TestCode";
			leg1.JW_ETD = new ZDateTime(2012, 3, 22);
			leg1.JW_ETA = new ZDateTime(2012, 3, 22);
			Factory.Save();
			CreateWorkflowExceptions();
			#endregion

			CombineAssertions(() =>
			{
				AssertEquals("CHK: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
				AssertEquals("CHK: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
				AssertEquals("CHK: Consol should have 2 legs", 2, consol.Transports.Count);
				AssertEquals("CHK: Transport Leg #0 event number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 event number count", 2, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 10, consol.Logs.GetAllLogs().Count);
			});

			#region BuildingMessageForTesting
			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText =
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF223</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			#endregion

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #1 should be updated", "12-Apr-12 14:11:59", consol.Transports[1].JW_ATD.ToString());
				AssertEquals("CHK: ATA for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATA.ToString());
				AssertEquals("CHK: ATA for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATA.ToString());
				AssertEquals("CHK: Transport Leg #0 logs number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 logs number count", 3, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 11, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: EDIMessage should be recognised", "WAR", message.EM_Status);
			});

			#region BuildingMessageForTesting
			var message_1 = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message_1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message_1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message_1.EM_MessageSubType = "XUE";
			message_1.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message_1.EM_Status = EDIMessage.Status.Queued;
			message_1.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message_1.EM_MessageText =
			message_1.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>ARV</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF223</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>SYD</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>FOC</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>AUSYD</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNFOC</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATA</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
			#endregion
			message_1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			#endregion
			new UniversalMessageProcessingManager(logger).Process(message_1);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
				AssertEquals("CHK: ATD for Transport Leg #1 should be updated", "12-Apr-12 14:11:59", consol.Transports[1].JW_ATD.ToString());
				AssertEquals("CHK: ATA for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATA.ToString());
				AssertEquals("CHK: ATA for Transport Leg #1 should be updated", "12-Apr-12 14:11:59", consol.Transports[1].JW_ATA.ToString());
				AssertEquals("CHK: Transport Leg #0 logs number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Transport Leg #1 logs number count", 4, consol.Transports[1].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 13, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: EDIMessage should be recognised", "WAR", message_1.EM_Status);
			});
		}

		public void TestFSUMessage_PortSwitchMatching_Comprehensive()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region BuildingConsolForTesting

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsNeutralMaster = false;
				consol.JK_TransportMode = "AIR";
				consol.MasterBillAirlinePrefix = "081";
				consol.MasterBillMAWB = "12347893";
				var leg0 = consol.Transports[0];
				leg0.JW_TransportMode = "AIR";
				leg0.JW_LegOrder = 1;
				leg0.JW_TransportType = "FL1";
				leg0.JW_VoyageFlight = "QF123";
				leg0.JW_RL_NKLoadPort = "AUSYD";
				leg0.JW_RL_NKDiscPort = "CNNKG";
				leg0.JW_ETA = new ZDateTime(2012, 3, 22);
				var leg1 = consol.Transports.AddNew();
				leg1.JW_TransportMode = "AIR";
				leg1.JW_LegOrder = 2;
				leg1.JW_TransportType = "FL2";
				leg1.JW_VoyageFlight = "QF223";
				leg1.JW_RL_NKLoadPort = "CNNKG";
				leg1.JW_RL_NKDiscPort = "CNFOC";
				leg1.JW_ETD = new ZDateTime(2012, 3, 22);
				leg1.JW_ETA = new ZDateTime(2012, 3, 22);
				var leg2 = consol.Transports.AddNew();
				leg2.JW_TransportMode = "AIR";
				leg2.JW_LegOrder = 3;
				leg2.JW_TransportType = "FL3";
				leg2.JW_VoyageFlight = "QF323";
				leg2.JW_RL_NKLoadPort = "CNFOC";
				leg2.JW_RL_NKDiscPort = "CNSHP";
				Factory.Save();
				CreateWorkflowExceptions();
				#endregion

				#region PRE-test Assertion
				CombineAssertions(() =>
				{
					AssertEquals("PRE: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
					AssertEquals("PRE: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
					AssertEquals("PRE: Consol should have 3 legs", 3, consol.Transports.Count);
					AssertEquals("PRE: Transport Leg #0 event number count", 1, consol.Transports[0].Logs.GetAllLogs().Count);
					AssertEquals("PRE: Transport Leg #1 event number count", 2, consol.Transports[1].Logs.GetAllLogs().Count);
					AssertEquals("PRE: Transport Leg #2 event number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
					AssertEquals("PRE: Consol Event Number count", 11, consol.Logs.GetAllLogs().Count);
				});
				#endregion

				#region BuildingMessageForTesting
				#region DEPMessage
				var dEPMessage = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				dEPMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				dEPMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				dEPMessage.EM_MessageSubType = "XUE";
				dEPMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
				dEPMessage.EM_Status = EDIMessage.Status.Queued;
				dEPMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				#region DEPmessage.EM_MessageText =
				dEPMessage.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF223</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>SHP</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNSHP</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
				#endregion
				dEPMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();
				#endregion
				#region MANMessage
				var mANMessage = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				mANMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				mANMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				mANMessage.EM_MessageSubType = "XUE";
				mANMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
				mANMessage.EM_Status = EDIMessage.Status.Queued;
				mANMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				#region MANMessage.EM_MessageText =
				mANMessage.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>FLM</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF223</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>SHP</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNSHP</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";
				#endregion
				mANMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();
				#endregion
				#region BKDMessage
				var bKDMessage = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				bKDMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				bKDMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				bKDMessage.EM_MessageSubType = "XUE";
				bKDMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
				bKDMessage.EM_Status = EDIMessage.Status.Queued;
				bKDMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				#region BKDMessage.EM_MessageText =
				bKDMessage.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>BKD</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF223</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>SHP</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNSHP</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";
				#endregion
				bKDMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();
				#endregion
				#region ARVMessage
				var aRVMessage = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
				aRVMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				aRVMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				aRVMessage.EM_MessageSubType = "XUE";
				aRVMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
				aRVMessage.EM_Status = EDIMessage.Status.Queued;
				aRVMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				#region ARVMessage.EM_MessageText =
				aRVMessage.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>ARV</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>ARR</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF123</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>NKG</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>SHP</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>CNSHP</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNNKG</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATA</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>";
				#endregion
				aRVMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();
				#endregion
				#endregion

				var logger = new SimpleLogger();
				var messageProcessingManager = new UniversalMessageProcessingManager(logger);
				messageProcessingManager.Process(dEPMessage);
				messageProcessingManager.Process(bKDMessage);
				messageProcessingManager.Process(mANMessage);
				messageProcessingManager.Process(aRVMessage);

				#region Checking Assertion
				CombineAssertions(() =>
				{
					AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: ATD for Transport Leg #1 should be updated", "2012-04-12 14:11:59", consol.Transports[1].JW_ATD.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: ATD for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATD.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: ATA for Transport Leg #0 should be updated", "2012-04-12 14:11:59", consol.Transports[0].JW_ATA.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: ATA for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATA.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: ATA for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATA.ToString("yyyy-MM-dd HH:mm:ss"));
					AssertEquals("CHK: Transport Leg #0 logs number count", 2, consol.Transports[0].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #1 logs number count", 5, consol.Transports[1].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Transport Leg #2 logs number count", 0, consol.Transports[2].Logs.GetAllLogs().Count);
					AssertEquals("CHK: Consol number count", 13, consol.Logs.GetAllLogs().Count);
					AssertEquals("CHK: DEPMessage should be recognised", "WAR", dEPMessage.EM_Status);
					AssertEquals("CHK: BKDMessage should be recognised", "WAR", bKDMessage.EM_Status);
					AssertEquals("CHK: MANMessage should be recognised", "WAR", mANMessage.EM_Status);
					AssertEquals("CHK: ARVMessage should be recognised", "WAR", aRVMessage.EM_Status);
				});
				#endregion
			}
		}

		public void TestFSUMessage_UsingUNLOCOMatch_DEPMatchOnLoadPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			var leg0 = consol.Transports[0];
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			leg0.JW_ETD = new ZDateTime(2012, 3, 22);
			var leg1 = consol.Transports.AddNew();
			leg1.JW_TransportMode = "AIR";
			leg1.JW_LegOrder = 2;
			leg1.JW_TransportType = "FL2";
			leg1.JW_VoyageFlight = "QF123";
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = "AIR";
			leg2.JW_LegOrder = 3;
			leg2.JW_TransportType = "FL3";
			leg2.JW_VoyageFlight = "QF123";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";
			Factory.Save();
			AssertEquals("Pre: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
			AssertEquals("Pre: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
			AssertEquals("Pre: Consol should have 3 legs", 3, consol.Transports.Count);

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>DEP</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>QF123</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
                <Type>OriginIATAAirportCode</Type>
                <Value>SYD</Value>
            </Context>
            <Context>
                <Type>DestinationIATAAirportCode</Type>
                <Value>FOC</Value>
            </Context>
            <Context>
                <Type>LegOriginUNLOCO</Type>
                <Value>AUSYD</Value>
            </Context>
            <Context>
                <Type>LegDestinationUNLOCO</Type>
                <Value>CNFOC</Value>
            </Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
        <AdditionalFieldsToUpdateCollection>
            <AdditionalFieldsToUpdate>
                <Type>JobConsolTransport.JW_ATD</Type>
                <Value>2012-04-12T14:11:59.8403643</Value>
            </AdditionalFieldsToUpdate>
        </AdditionalFieldsToUpdateCollection>
    </Event>
</UniversalEvent>
";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			AssertEquals("CHK: ATD for Transport Leg #0 should be updated", "12-Apr-12 14:11:59", consol.Transports[0].JW_ATD.ToString());
			AssertEquals("CHK: ATD for Transport Leg #1 should not be updated", string.Empty, consol.Transports[1].JW_ATD.ToString());
			AssertEquals("CHK: ATD for Transport Leg #2 should not be updated", string.Empty, consol.Transports[2].JW_ATD.ToString());
		}

		public void TestFSUMessage_NonLegRelatedEvent()
		{
			#region BuildingConsolForTesting
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsNeutralMaster = false;
			consol.JK_TransportMode = "AIR";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "12347893";
			var leg0 = consol.Transports[0];
			leg0.JW_TransportMode = "AIR";
			leg0.JW_LegOrder = 1;
			leg0.JW_TransportType = "FL1";
			leg0.JW_VoyageFlight = "QF123";
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			Factory.Save();
			#endregion
			CombineAssertions(() =>
			{
				AssertEquals("PRE: MAWB should be 08112347893", "08112347893", consol.JK_MasterBillNum);
				AssertEquals("PRE: Consol Transport Mode should be AIR", "AIR", consol.TransportMode);
				AssertEquals("PRE: Consol should have 1 legs", 1, consol.Transports.Count);
				AssertEquals("PRE: Transport Leg #0 number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("PRE: Consol number count", 4, consol.Logs.GetAllLogs().Count);
			});

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = "XUE";
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message.EM_MessageText =
			message.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>CCD</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DIS</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
				<Type>IATAAirportCode</Type>
                <Value>FOC</Value>
			</Context>
			<Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";
			#endregion
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var logger = new SimpleLogger();
			new UniversalMessageProcessingManager(logger).Process(message);
			CombineAssertions(() =>
			{
				AssertEquals("CHK: ATD for Transport Leg #0 should not be updated", string.Empty, consol.Transports[0].JW_ATD.ToString());
				AssertEquals("CHK: Transport Leg #0 logs number count", 0, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK: Consol number count", 5, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK: EDIMessage should be recognised", "WAR", message.EM_Status);
			});

			var message_1 = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message_1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message_1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message_1.EM_MessageSubType = "XUE";
			message_1.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message_1.EM_Status = EDIMessage.Status.Queued;
			message_1.EM_GB = GlbBranch.CurrentBranch.PK;
			#region message_1.EM_MessageText =
			message_1.EM_MessageText = @"
<UniversalEvent>
    <Event>
        <DataContext>
            <DataProvider>FSU Message</DataProvider>
        </DataContext>
        <EventType>FOH</EventType>
        <EventTime>2012-04-12T14:11:59.6059548</EventTime>
        <ContextCollection>
            <Context>
                <Type>MAWBNumber</Type>
                <Value>081-12347893</Value>
            </Context>
            <Context>
                <Type>MAWBNumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>SourceEventCode</Type>
                <Value>DEP</Value>
            </Context>
            <Context>
                <Type>NumberOfPieces</Type>
                <Value>1</Value>
            </Context>
            <Context>
                <Type>WeightOfGoods</Type>
                <Value>109KG</Value>
            </Context>
            <Context>
                <Type>IATACarrierCode</Type>
                <Value>LH</Value>
            </Context>
            <Context>
                <Type>FlightNumber</Type>
                <Value>6852S</Value>
            </Context>
            <Context>
                <Type>FlightDate</Type>
                <Value>2012-03-22</Value>
            </Context>
            <Context>
				<Type>IATAAirportCode</Type>
                <Value>FOC</Value>
			</Context>
            <Context>
                <Type>TimeOfDeparture</Type>
                <Value>A 22-Mar-2012 22:00</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";
			#endregion
			message_1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			new UniversalMessageProcessingManager(logger).Process(message_1);
			CombineAssertions(() =>
			{
				AssertEquals("CHK2: Transport Leg #0 logs number count", 1, consol.Transports[0].Logs.GetAllLogs().Count);
				AssertEquals("CHK2: Consol number count", 5, consol.Logs.GetAllLogs().Count);
				AssertEquals("CHK2: EDIMessage should be recognised", "WAR", message_1.EM_Status);
			});
		}

		#region implementatino

		JobSailing CreateSailing(string voyageFlight, string portOfLoading, string portOfDischarge, ZDateTime flightDate)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_FlightDate = flightDate;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = portOfLoading;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;

			var sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			return sailing;
		}

		void CreateWorkflowExceptions()
		{
			GetProcessTasks(true).Concat(GetProcessTasks(false)).ForEach(milestone =>
			{
				if (milestone.P9_ActualDate.IsEmpty)
				{
					milestone.CreateMilestoneException();
				}
			});
		}

		IList<ProcessTask> GetProcessTasks(bool isExpired)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			if (SystemDataRegistry.Instance.WorkflowExceptionGenerationHWM.Value != DateTime.MinValue)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.GreaterThanOrEqualTo, SystemDataRegistry.Instance.WorkflowExceptionGenerationHWM.Value.AddMinutes(-5));
			}

			if (isExpired)
			{
				var expiredQuery = new ZQuery(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow);
				expiredQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, SQLComparisonOperator.Equal, ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired);

				query.AddToFilter(expiredQuery);
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcToday);
			}

			query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			query.AddToFilter(ProcessTasksSchema.P9_ActualDate, ZDateTime.Empty);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(ProcessTasksSchema.P9_MilestoneExceptionAdded, ZDateTime.Empty);

			return Factory.Load<ProcessTask>(query);
		}

		#endregion
	}
}
