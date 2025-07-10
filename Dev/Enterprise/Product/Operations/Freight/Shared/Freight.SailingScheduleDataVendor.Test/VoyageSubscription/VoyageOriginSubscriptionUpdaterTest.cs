using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.SailingScheduleDataVendor.Testing
{
	[TestedType(typeof(VoyageOriginSubscriptionUpdater))]
	sealed class VoyageOriginSubscriptionUpdaterTest : ScheduleServiceSubscriptionUpdaterTest<VoyageOriginSubscriptionUpdater>
	{
		public void TestProcessLogs_VoyageOriginHasLog_ProcessLog()
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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();
				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				var document = new System.Xml.XmlDocument();
				document.LoadXml(message.EM_MessageText);
				AssertEquals("XML namespace", UniversalXmlInfo.Namespace_2012_11, document.DocumentElement.NamespaceURI);
				var contextCollectionNode = document.SelectNodes("//*[local-name()='ContextCollection']")[0];
				AssertEquals("<ContextCollection xmlns=\"http://www.cargowise.com/Schemas/Universal/2012/11\"><Context><Type>TransportMode</Type><Value>SEA</Value></Context><Context><Type>VesselName</Type><Value>Sailing Vessel</Value></Context><Context><Type>VoyageNumber</Type><Value>SailingVoy</Value></Context><Context><Type>IsCharter</Type><Value>false</Value></Context><Context><Type>CarrierCode</Type><Value>ACLU</Value></Context><Context><Type>LegOriginUNLOCO</Type><Value>AUBNE</Value></Context><Context><Type>EstimatedTimeOfDeparture</Type><Value>2016-02-16</Value></Context></ContextCollection>", contextCollectionNode.OuterXml);

				AssertMultilineASCIIEquals("Log", @"Processing Sailing Schedule (Vessel='Sailing Vessel', Voyage='SailingVoy', Carrier='XVBQP68SIYXQ'), Origin = 'AUBNE'", GetLogsAsString(Notifier));
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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

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
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUB";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();
				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The origin port has a wrong UNLOCO: AUB", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_IncorrectTransportMode()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ScheduleFeedTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SFSEHubId"))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Sailing Vessel";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "SailingVoy";
				voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "ACLU", "US").PK;
				var voyageOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
				voyageOrigin.JA_E_DEP = new DateTime(2016, 2, 16, 10, 30, 45);
				voyageOrigin.JA_JV = voyage.PK;

				voyageOrigin.Logs.AddNew(Events.SubscriptionRequested, Params.Type.AsKeyFor(Types.ScheduleFeed));

				Factory.Save();
				RunLogWalkerCycleForTest();

				var message = Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				Assert(message == null);

				AssertContains("Log", @"The Transport Mode is wrong: RAI", GetLogsAsString(Notifier));
			}
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance()
		{
			return Factory.NewWithValidTestData<VoyageOrigin>();
		}
	}
}
