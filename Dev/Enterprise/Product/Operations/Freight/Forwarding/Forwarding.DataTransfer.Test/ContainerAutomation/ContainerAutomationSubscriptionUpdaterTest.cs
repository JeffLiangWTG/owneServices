using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	abstract class ContainerAutomationSubscriptionUpdaterTest<T> : LogSubscriberTest<T> where T : LogSubscriber, new()
	{
		protected OrgHeader Org => org ?? (org = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCD", "US"));
		OrgHeader org;

		protected void ProcessLogs_UserDepartmentAndBranchAreSerialized(Func<IBusiness> createBusinessObject)
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.CompanyName = "testCompany";
			testCompany.GC_Code = "HMC";

			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_BranchName = "testBranch";
			testBranch.GB_Code = "HMB";
			testBranch.GB_GC = testCompany.PK;

			var testDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			testDepartment.GE_Code = "HMD";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "XXX";
			staff.GS_GB_HomeBranch = testBranch.PK;

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var obj = createBusinessObject();
				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), testBranch.PK.ToGuid(), testDepartment.PK.ToGuid()))
				{
					Factory.Save();
				}

				RunLogWalkerCycleForTest();

				var xmlEvent = (IXmlEDIMessage)GetLastMessage();
				var content = xmlEvent.Content.ToString(SaveOptions.DisableFormatting);

				AssertContains("xml event branch is the SBR log creation branch",
					"<EventBranch Name=\"testBranch\">HMB</EventBranch>", content);

				AssertContains("xml event department is the SBR log creation department",
					"<EventDepartment>HMD</EventDepartment>", content);

				AssertContains("xml event user is the SBR log creation user",
					"<EventUser Name=\"testStaff\">XXX</EventUser>", content);

				var objLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, ((BusinessObject)obj).PK));
				var dex = objLogs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode);

				AssertNotNull("DEX has been been created", dex);
				AssertEquals("DEX branch is the same as xml event branch", dex.SL_GB_NKBranch, testBranch.GB_Code);

				AssertEquals("EDIMessage is Sent", xmlEvent.EM_Status, EDIMessageStatusList.Codes.Sent);
				AssertEquals("EDIMessage branch is the same as xml event branch", xmlEvent.EM_GB, testBranch.PK);
			}
		}

		protected static IDisposable SetEHubId()
		{
			return FreightDataRegistry.Instance.ContainerAutomationEHubID.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"McLaren");
		}

		protected static IDisposable SetContainerAutomationEnabled()
		{
			return FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				true);
		}

		protected abstract EnterpriseBusinessObject GetBusinessObjectInstance();

		protected static string GetSubContextValue(Context context, UniversalEvent.ContextTypes type)
		{
			return context.SubContextCollection.FirstOrDefault(x => x.Type == type.ToString())?.Value;
		}

		protected static void AssertTransportLegContext(
			Context context,
			byte legOrder,
			string transportMode,
			string origin = null,
			string destination = null,
			DateTime? departureDate = null,
			DateTime? arrivalDate = null,
			string vesselName = null,
			string lloydsNumber = null,
			string voyageNumber = null)
		{
			AssertEquals("TransportLegOrder", legOrder.ToString(), context.Value);
			AssertEquals("TransportMode", transportMode, GetSubContextValue(context, UniversalEvent.ContextTypes.TransportMode));
			AssertEquals("LegOriginUnloco", origin, GetSubContextValue(context, UniversalEvent.ContextTypes.LegOriginUNLOCO));
			AssertEquals("LegDestinationUNLOCO", destination, GetSubContextValue(context, UniversalEvent.ContextTypes.LegDestinationUNLOCO));
			AssertEquals("EstimatedTimeOfDeparture", departureDate?.ToString("s"), GetSubContextValue(context, UniversalEvent.ContextTypes.EstimatedTimeOfDeparture));
			AssertEquals("EstimatedTimeOfArrival", arrivalDate?.ToString("s"), GetSubContextValue(context, UniversalEvent.ContextTypes.EstimatedTimeOfArrival));
			if (transportMode == Constants.TransportModes.Sea)
			{
				AssertEquals("VesselName", vesselName, GetSubContextValue(context, UniversalEvent.ContextTypes.VesselName));
				AssertEquals("LloydsNumber", lloydsNumber, GetSubContextValue(context, UniversalEvent.ContextTypes.LloydsNumber));
				AssertEquals("VoyageNumber", voyageNumber, GetSubContextValue(context, UniversalEvent.ContextTypes.VoyageNumber));
			}
		}

		protected UniversalEvent GetLastMessageAsUniversalEvent()
		{
			var message = GetLastMessage();

			var xmlEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			AssertNotNull(xmlEvent);
			return xmlEvent;
		}

		protected IEDIMessage GetLastMessage()
		{
			var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)
			{
				OrderBy = "EM_SystemCreateTimeUtc desc" // this is a column name
			};
			var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
			AssertNotNull(message);
			return message;
		}

		protected void AddOrUpdateSeaTransportWithEmptyValues(BusinessObjectCollection transportCollection)
		{
			var transport = transportCollection.Count >= 1 ? (Transport)transportCollection.ElementAt(0) : (Transport)transportCollection.AddNew(typeof(Transport));
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_RL_NKLoadPort = null;
			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_RL_NKDiscPort = null;
			transport.JW_VoyageFlight = null;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = null;
			vessel.RV_OH = Org.PK;
			vessel.RV_LloydsNumber = null;
			transport.JW_Vessel = vessel.RV_FK;
		}

		protected void AddOrUpdateSeaTransport(BusinessObjectCollection transportCollection)
		{
			var transport = transportCollection.Count >= 1 ? (Transport)transportCollection.ElementAt(0) : (Transport)transportCollection.AddNew(typeof(Transport));
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_ETD = EtdOrigin;
			transport.JW_RL_NKLoadPort = UnlocoOrigin;
			transport.JW_ETA = EtaTransshipment;
			transport.JW_RL_NKDiscPort = UnlocoTransshipment;
			transport.JW_VoyageFlight = VoyageNumber;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = VesselName;
			vessel.RV_OH = Org.PK;
			vessel.RV_LloydsNumber = LloydsNumber;
			transport.JW_Vessel = vessel.RV_FK;
		}

		protected static void AddAirTransport(BusinessObjectCollection transportCollection)
		{
			var transport = (Transport)transportCollection.AddNew(typeof(Transport));
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_ETD = EtdTransshipment;
			transport.JW_RL_NKLoadPort = UnlocoTransshipment;
			transport.JW_ETA = EtaDestination;
			transport.JW_RL_NKDiscPort = UnlocoDestination;
			transport.JW_VoyageFlight = FlightNumber;
			transportCollection.Add(transport);
		}

		protected static void AssertSecondLeg(IReadOnlyList<Context> transportLegs)
		{
			AssertTransportLegContext(
				transportLegs[1],
				2,
				Constants.TransportModes.Air,
				UnlocoTransshipment,
				UnlocoDestination,
				EtdTransshipment,
				EtaDestination);
		}

		protected static void AssertFirstLeg(IReadOnlyList<Context> transportLegs)
		{
			AssertTransportLegContext(
				transportLegs[0],
				1,
				Constants.TransportModes.Sea,
				UnlocoOrigin,
				UnlocoTransshipment,
				EtdOrigin,
				EtaTransshipment,
				VesselName,
				LloydsNumber,
				VoyageNumber);
		}

		protected static void AssertFirstEmptyLeg(IReadOnlyList<Context> transportLegs)
		{
			AssertTransportLegContext(
				transportLegs[0],
				1,
				Constants.TransportModes.Sea);
			AssertEquals(1, transportLegs[0].SubContextCollection.Count);
		}

		protected const string VesselName = "OLEG THE ALMIGHTY";
		protected const string LloydsNumber = "123123";
		protected const string VoyageNumber = "036E";
		protected const string FlightNumber = "QF001";
		protected const string UnlocoOrigin = "DEHAM";
		protected const string UnlocoTransshipment = "SGSIN";
		protected const string UnlocoDestination = "AUSYD";
		protected static readonly DateTime EtdOrigin = DateTime.Today;
		protected static readonly DateTime EtaTransshipment = EtdOrigin.AddDays(1);
		protected static readonly DateTime EtdTransshipment = EtaTransshipment.AddDays(1);
		protected static readonly DateTime EtaDestination = EtdTransshipment.AddDays(1);

		public void TestProcessLogs_GlobalTrackingIsDisabled_DoNotProcessLogs()
		{
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var businessObject = GetBusinessObjectInstance();
				businessObject.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as Global Container Tracking is disabled.", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_eHubIDIsNotSpecified_DoNotProcessLogs()
		{
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.ContainerAutomationEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var businessObject = GetBusinessObjectInstance();
				businessObject.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as eHub ID is not set.", GetLogsAsString(Notifier));
			}
		}

		protected string GetLogsAsString(ILogger logger)
		{
			var logPrefix = string.Format("[{0}]", LogSubscriber.FriendlyName);
			var logs = ((LoggerForTesting)logger).NotifiedEventList.Where(l => l.StartsWith(logPrefix)).Select(l => l.Substring(logPrefix.Length + 1));

			return string.Join(System.Environment.NewLine, logs);
		}
	}
}
