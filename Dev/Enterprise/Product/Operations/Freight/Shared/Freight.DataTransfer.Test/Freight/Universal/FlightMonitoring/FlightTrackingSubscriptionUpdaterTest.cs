using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring.Testing
{
	abstract class FlightTrackingSubscriptionUpdaterTest<T> : LogSubscriberTest<T> where T : LogSubscriber, new()
	{
		public void ProcessLogs_UserDepartmentAndBranchAreSerialized(Func<IBusiness> createBusinessObject)
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
			staff.GS_Code = "XYX";
			staff.GS_GB_HomeBranch = testBranch.PK;

			Factory.Save();

			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId("FMSEHubId"))
			{
				var obj = createBusinessObject();
				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), testBranch.PK.ToGuid(), testDepartment.PK.ToGuid()))
				{
					Factory.Save();
				}

				RunLogWalkerCycleForTest();

				var xmlEvent = (IXmlEDIMessage)GetLastMessage();
				var content = xmlEvent.Content.ToString();

				AssertContains("xml event branch is the SBR log creation branch",
					"<EventBranch Name=\"testBranch\">HMB</EventBranch>", content);

				AssertContains("xml event department is the SBR log creation department",
					"<EventDepartment>HMD</EventDepartment>", content);

				AssertContains("xml event user is the SBR log creation user",
					"<EventUser Name=\"testStaff\">XYX</EventUser>", content);

				var objLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, ((BusinessObject)obj).PK));
				var dex = objLogs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode);

				AssertNotNull("DEX has been been created", dex);
				AssertEquals("DEX branch is the same as xml event branch", dex.SL_GB_NKBranch, testBranch.GB_Code);

				AssertEquals("EDIMessage is Sent", xmlEvent.EM_Status, EDIMessageStatusList.Codes.Sent);
				AssertEquals("EDIMessage branch is the same as xml event branch", xmlEvent.EM_GB, testBranch.PK);
			}
		}

		public void TestProcessLogs_FlightTrackingFeedRequestIsDisabled_DoNotProcessLogs()
		{
			using (SetAwbTrackingEnabled(false))
			{
				var businessObject = GetBusinessObjectInstance();
				businessObject.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.AWBAutomation));

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as Flight Monitoring System is disabled.", GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_eHubIDIsNotSpecified_DoNotProcessLogs()
		{
			using (SetAwbTrackingEnabled(true))
			using (SetAwbTrackingEHubId(""))
			{
				var businessObject = GetBusinessObjectInstance();
				businessObject.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.AWBAutomation));

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Subscription requests cannot be sent as eHub ID is not set.", GetLogsAsString(Notifier));
			}
		}

		protected abstract EnterpriseBusinessObject GetBusinessObjectInstance();

		protected string GetLogsAsString(ILogger logger)
		{
			var logPrefix = string.Format("[{0}]", LogSubscriber.FriendlyName);
			var logs = ((LoggerForTesting)logger).NotifiedEventList.Where(l => l.StartsWith(logPrefix)).Select(l => l.Substring(logPrefix.Length + 1));

			return string.Join(System.Environment.NewLine, logs);
		}

		ZString? GetSubContextValue(Context context, UniversalEvent.ContextTypes type)
		{
			return context.SubContextCollection.FirstOrDefault(x => x.Type == type.ToString())?.Value;
		}

		protected void AssertTransportLegContext(Context context, byte legOrder, ZString flightNumber, ZString origin,
			ZString destination, ZString departureDate, ZString arrivalDate)
		{
			AssertEquals("TransportLegOrder", legOrder.ToString(), context.Value);
			AssertEquals("OriginIATAAirportCode", origin, GetSubContextValue(context, UniversalEvent.ContextTypes.OriginIATAAirportCode));
			AssertEquals("DestinationIATAAirportCode", destination, GetSubContextValue(context, UniversalEvent.ContextTypes.DestinationIATAAirportCode));
			AssertEquals("EstimatedTimeOfDeparture", departureDate, GetSubContextValue(context, UniversalEvent.ContextTypes.EstimatedTimeOfDeparture));
			AssertEquals("EstimatedTimeOfArrival", arrivalDate, GetSubContextValue(context, UniversalEvent.ContextTypes.EstimatedTimeOfArrival));
			AssertEquals("FlightNumber", flightNumber, GetSubContextValue(context, UniversalEvent.ContextTypes.FlightNumber));
		}

		protected static IDisposable SetAwbTrackingEHubId(string id)
		{
			return FreightDataRegistry.Instance.AWBTrackingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, id);
		}

		protected static IDisposable SetAwbTrackingEnabled(bool enabled)
		{
			return FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
		}

		IEDIMessage GetLastMessage()
		{
			var ediQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)
			{
				OrderBy = "EM_SystemCreateTimeUtc desc"
			};
			var message = Factory.LoadTop1<IEDIMessage>(ediQuery);
			AssertNotNull(message);
			return message;
		}
	}
}
