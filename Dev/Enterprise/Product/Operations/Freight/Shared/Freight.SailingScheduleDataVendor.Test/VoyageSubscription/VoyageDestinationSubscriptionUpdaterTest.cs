using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.SailingScheduleDataVendor.Testing
{
	[TestedType(typeof(VoyageDestinationSubscriptionUpdater))]
	sealed class VoyageDestinationSubscriptionUpdaterTest : ScheduleServiceSubscriptionUpdaterTest<VoyageDestinationSubscriptionUpdater>
	{
		public void TestProcessLogs_VoyageDestinationHasLog_ProcessLog()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACLU", "US").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				var document = new System.Xml.XmlDocument();
				document.LoadXml(message.EM_MessageText);
				var contextCollectionNode = document.SelectNodes("//*[local-name()='ContextCollection']")[0];
				AssertXMLEquals("<ContextCollection xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\"><Context><Type>TransportMode</Type><Value>SEA</Value></Context><Context><Type>VesselName</Type><Value>Sailing Vessel</Value></Context><Context><Type>VoyageNumber</Type><Value>SailingVoy</Value></Context><Context><Type>IsCharter</Type><Value>false</Value></Context><Context><Type>CarrierCode</Type><Value>ACLU</Value></Context><Context><Type>LegDestinationUNLOCO</Type><Value>USLAX</Value></Context><Context><Type>EstimatedTimeOfArrival</Type><Value>2016-03-21</Value></Context></ContextCollection>", contextCollectionNode.OuterXml);
				AssertMultilineASCIIEquals("Log", @"Processing Sailing Schedule (Vessel='Sailing Vessel', Voyage='SailingVoy', Carrier='XVBQP68SIYXQ'), Destination = 'USLAX'", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_IncorrectVoyageFlight()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "000";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACLU", "US").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The Voyage Number consists of spaces and/or zeros only: 000", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_IncorrectCarrierCode()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACL", "US").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The carrier code is not correct: ACL", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_CarrierCodeNotFound()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The carrier code not found", GetLogsAsString(Notifier));
			}

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = RefVessel.LookupVesselByName("Sailing Vessel", Factory).First();

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.InntraCode, "CMDU", "AU").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The carrier code not found", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_IncorrectUnloco()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACLU", "US").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USL";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The destination port has a wrong UNLOCO: USL", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_IncorrectTransportMode()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "SailingVoy";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = vessel.RV_FK;
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACLU", "US").PK;

				var voyageDestination = Factory.NewWithValidTestData<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = "USLAX";
				voyageDestination.JB_E_ARV = new DateTime(2016, 3, 21, 08, 11, 33);
				voyageDestination.JB_JV = voyage.PK;

				voyageDestination.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();

				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The Transport Mode is wrong: RAI", GetLogsAsString(Notifier));
			}
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance()
		{
			return Factory.NewWithValidTestData<VoyageDestination>();
		}
	}
}
