using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring.Testing
{
	[TestedType(typeof(FlightTrackingConsolSubscriptionUpdater))]
	sealed class ConsolSubscriptionUpdaterTest : FlightTrackingSubscriptionUpdaterTest<FlightTrackingConsolSubscriptionUpdater>
	{
		public void TestProcessLogs_UserDepartmentAndBranchAreSerialized()
		{
			ProcessLogs_UserDepartmentAndBranchAreSerialized(() =>
			{
				var consol = GetConsol();
				consol.JK_MasterBillNum = "46197135464";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport1.JW_IsLinked = true;
				transport1.JW_ETD = 3.DaysAgo();
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_ETA = 1.DaysAgo();
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_VoyageFlight = "QF001";

				Factory.Save();

				consol.JK_MasterBillNum = "08187443521";

				return consol;
			});
		}

		public void TestProcessLogs_TransportHasLog_ProcessLog()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				#region Setup Test Data

				var consol = GetConsol();
				consol.JK_MasterBillNum = "46197135463";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport1.JW_IsLinked = true;
				transport1.JW_ETD = 8.DaysAgo();
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_ETA = 5.DaysAgo();
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_VoyageFlight = "QF001";

				var transport2 = consol.Transports.AddNew(typeof(Transport));
				transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport2.JW_IsLinked = true;
				transport2.JW_ETD = 3.DaysAgo();
				transport2.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_ETA = 1.DaysAgo();
				transport2.JW_RL_NKDiscPort = "AUMEL";
				transport2.JW_VoyageFlight = "QF123";
				consol.Transports.Add(transport2);

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Precondition: new consol with linked transport leg should not create SBR log", 0, logs.Count);

				consol.JK_MasterBillNum = "08187443521";
				Factory.Save();

				logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should be created new SBR event", 1, logs.Count);

				#endregion

				RunLogWalkerCycleForTest();

				var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
				AssertNotNull(message);

				var xmlEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				AssertNotNull(xmlEvent);

				CombineAssertions(delegate
				{
					AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
					AssertEquals("EventReference", ZString.Empty, xmlEvent.EventReference ?? ZString.Empty);
					AssertEquals("Type", "AWB Automation", xmlEvent.EventParameters.Type);

					var transportLegs = xmlEvent.ContextCollection
						.Where(x => x.Type == EventContextType.TransportLeg)
						.ToArray();

					AssertEquals(2, transportLegs.Length);
					AssertTransportLegContext(transportLegs[0], 1, "QF001", "SYD", "LAX", transport1.JW_ETD.ToString("yyyy-MM-dd"), transport1.JW_ETA.ToString("yyyy-MM-dd"));
					AssertTransportLegContext(transportLegs[1], 2, "QF123", "BNE", "MEL", transport2.JW_ETD.ToString("yyyy-MM-dd"), transport2.JW_ETA.ToString("yyyy-MM-dd"));

					AssertMultilineASCIIEquals("Log", @"Processing Consol CS-1234567 (Master Bill='08187443521')", GetLogsAsString(Notifier));
				});
			}
		}

		public void TestProcessLogs_ConsolTypeChanged_AfterSBRCreated()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				#region Setup Test Data

				var consol = GetConsol();
				consol.JK_MasterBillNum = "08187443521";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport1.JW_IsLinked = false;
				transport1.JW_ETD = 3.DaysAgo();
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_ETA = 1.DaysAgo();
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_VoyageFlight = "QF001";

				Factory.Save();

				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Factory.Save();

				//There should be one SBR created when consol was AIR
				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should be created new SBR event", 1, logs.Count);

				#endregion

				RunLogWalkerCycleForTest();

				//SBR from a consol with type of SEA should not result in creation of any EDI Message
				var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
				AssertNull(message);
			}
		}

		public void TestProcessLogs_AirCoLoadConsol()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				#region Setup Test Data

				var consol = GetConsol();
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_CoLoadMasterBill = "CLDMBL";
				consol.JK_CoLoadBookingReference = "CLDBR";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "CODE";
				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_Code = "CREDITOR";
				coLoadWith.OH_FullName = "NAME";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should be created new SBR event", 1, logs.Count);

				#endregion

				RunLogWalkerCycleForTest();

				var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
				AssertNotNull(message);

				var xmlEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				AssertNotNull(xmlEvent);

				CombineAssertions(delegate
				{
					AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
					AssertEquals("EventReference", ZString.Empty, xmlEvent.EventReference ?? ZString.Empty);
					AssertEquals("Type", "AWB Automation", xmlEvent.EventParameters.Type);

					AssertEquals("CoLoadBillNumber", "CLDMBL", GetContextValue(xmlEvent, UniversalEvent.ContextTypes.CoLoadBillNumber));
					AssertEquals("CoLoadBookingReference", "CLDBR", GetContextValue(xmlEvent, UniversalEvent.ContextTypes.CoLoadBookingReference));
					AssertEquals("CoLoadWithC1CCode", "CODE", GetContextValue(xmlEvent, UniversalEvent.ContextTypes.CoLoadWithC1CCode));
					AssertEquals("CoLoadWithName", "NAME", GetContextValue(xmlEvent, UniversalEvent.ContextTypes.CoLoadWithName));

					AssertMultilineASCIIEquals("Log", @"Processing Consol CS-1234567", GetLogsAsString(Notifier));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestProcessLogs_AirCoLoadConsol_ShouldNotRaiseExceptionWhenCreditorIsNull()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				#region Setup Test Data

				var consol = GetConsol();
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_MasterBillNum = "17634567890";
				consol.JK_CoLoadMasterBill = "CLDMBL";
				consol.JK_CoLoadBookingReference = "CLDBR";

				var transport = consol.Transports[0];
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_ETA = 1.DaysAgo();

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should be created new SBR event", 1, logs.Count);

				#endregion

				RunLogWalkerCycleForTest();

				var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
				var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
				AssertNotNull(message);

				var xmlEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				AssertNotNull(xmlEvent);

				CombineAssertions(delegate
				{
					AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
					AssertEquals("EventReference", ZString.Empty, xmlEvent.EventReference ?? ZString.Empty);
					AssertEquals("Type", "AWB Automation", xmlEvent.EventParameters.Type);
				});
			}
		}

		ZString GetContextValue(UniversalEvent xmlEvent, UniversalEvent.ContextTypes contextType)
		{
			return xmlEvent.ContextCollection.Single(c => c.Type == contextType.ToString()).Value.Value;
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance()
		{
			return Factory.NewWithValidTestData<CommonConsol>();
		}

		CommonConsol GetConsol()
		{
			var consol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUPER";
			consol.JK_UniqueConsignRef = "CS-1234567";

			return consol;
		}
	}
}
