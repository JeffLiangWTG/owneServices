using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring.Testing
{
	[TestedType(typeof(FlightTrackingDeclarationSubscriptionUpdater))]
	sealed class DeclarationSubscriptionUpdaterTest : FlightTrackingSubscriptionUpdaterTest<FlightTrackingDeclarationSubscriptionUpdater>
	{
		public void TestProcessLogs_UserDepartmentAndBranchAreSerialized()
		{
			ProcessLogs_UserDepartmentAndBranchAreSerialized(() =>
			{
				var declaration = (IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBaseJobDeclaration)));
				declaration["JE_TransportMode"] = Core.Constants.TransportModes.Air;
				declaration.JE_MasterBill = "46197135463";

				var transport = ((TransportCollection)declaration["Transports"]).AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETA = 1.DaysAgo();
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_VoyageFlight = "QF001";
				Factory.Save();

				declaration.JE_MasterBill = "08187443521";

				return declaration;
			});
		}

		public void TestProcessLogs_TransportHasLog_ProcessLog()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				#region Setup Test Data

				var declaration = (IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBaseJobDeclaration)));
				declaration["JE_TransportMode"] = Core.Constants.TransportModes.Air;
				declaration.JE_MasterBill = "46197135463";

				var transport = ((TransportCollection)declaration["Transports"]).AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETA = 1.DaysAgo();
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_VoyageFlight = "QF001";
				Factory.Save();

				var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Precondition: new declaration with linked transport leg should not create SBR log", 0, logs.Count);

				declaration.JE_MasterBill = "08187443521";
				Factory.Save();

				logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
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

					AssertEquals(1, transportLegs.Length);
					AssertTransportLegContext(transportLegs[0], 1, "QF001", "SYD", "LAX", transport.JW_ETD.ToString("yyyy-MM-dd"), transport.JW_ETA.ToString("yyyy-MM-dd"));

					AssertMultilineASCIIEquals("Log", @"Processing Declaration B00001000", GetLogsAsString(Notifier));
				});
			}
		}

		public void TestProcessLogs_DeclarationTransportTypeChanged_AfterSBRCreated()
		{
			#region Setup Test Data

			var declaration = (IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBaseJobDeclaration)));
			declaration["JE_TransportMode"] = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "46197135463";

			var transport = ((TransportCollection)declaration["Transports"]).AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_IsLinked = false;
			transport.JW_ETD = 3.DaysAgo();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ETA = 1.DaysAgo();
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_VoyageFlight = "QF001";
			Factory.Save();

			var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
			AssertEquals("Precondition: new declaration should create SBR log", 1, logs.Count);

			declaration["JE_TransportMode"] = Core.Constants.TransportModes.Sea;
			Factory.Save();

			#endregion

			RunLogWalkerCycleForTest();

			var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
			var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
			AssertNull(message);
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance()
		{
			return Factory.New<IBaseJobDeclaration>() as EnterpriseBusinessObject;
		}
	}
}
