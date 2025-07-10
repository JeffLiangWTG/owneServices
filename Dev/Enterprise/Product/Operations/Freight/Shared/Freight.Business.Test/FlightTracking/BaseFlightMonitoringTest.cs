using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseFlightMonitoringTest : TestCaseWithFactory
	{
		public void TestIf_JobSailing_IsApplicable_ForFlightSubscription_When_ETDIsInThePast()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF8332";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			var flightMonitoring =  new FlightMonitoringSailingProvider(sailing);

			sailing.Origin.JA_E_DEP = 2.DaysAgo();
			sailing.Destination.JB_E_ARV = 2.DaysAgo().AddMinutes(1);

			flightMonitoring.UpdateFlightSubscriptionEvent();

			AssertEquals("Event type should be SBR", sailing.Logs.MostRecentLog.SL_SE_NKEvent, "SBR");
			AssertEquals("Log should be: USLAX -> SGSIN", ((Enterprise.Integration.IWorkflowTriggerSource)sailing.Logs.MostRecentLog).FriendlyTableName, "This Flight Port Pair (Load='SGSIN' Discharge='USLAX')");
		}

		public void TestIf_Transport_IsApplicable_ForFlightSubscription_When_ETDIsInThePast()
		{
			var transport = Factory.New<Transport>();
			var consol = Factory.New<CommonConsol>();

			consol.JK_MasterBillNum = "99000011126";

			transport.ParentType = typeof(CommonConsol);
			transport.JW_ParentGUID = consol.PK;
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETA = 2.DaysAgo().AddMinutes(1);
			transport.JW_ETD = 2.DaysAgo();
			transport.JW_ATA = 2.DaysAgo().AddMinutes(1);
			transport.JW_ATD = 3.DaysAgo();
			transport.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "QF8332";

			var flightMonitoring = new FlightMonitoringTransportProvider(transport);
			flightMonitoring.UpdateFlightSubscriptionEvent();

			AssertEquals("Event type should be SBR", ((CommonConsol)transport.Parent).Logs.MostRecentLog.SL_SE_NKEvent, "SBR");
			AssertEquals("Log should be: USLAX -> SGSIN", ((CommonConsol)transport.Parent).Logs.MostRecentLog.SL_TableFriendlyName, "This Consol (Master Bill='99000011126')");
		}

		public void TestIsArrivalOrDepartureDateRecent_Transport()
		{
			var transport = Factory.New<Transport>();
			transport.ParentType = typeof(CommonConsol);

			transport.JW_ETA = 2.DaysAgo().AddMinutes(1);
			var actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETA is recent since it is not older than 2 days", true, actual);

			transport.JW_ETA = 2.DaysAgo();
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETA is not recent since it is older than 2 days", false, actual);

			transport.JW_ETA = ZDate.Empty;
			transport.JW_ATA = 2.DaysAgo().AddMinutes(1);
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATA is recent since it is not older than 2 days", true, actual);

			transport.JW_ATA = 2.DaysAgo();
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATA is not recent since it is older than 2 days", false, actual);

			transport.JW_ETA = ZDate.Empty;
			transport.JW_ATA = ZDate.Empty;
			transport.JW_ETD = 3.DaysAgo().AddMinutes(1);
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETD is recent since it is not older than 3 days", true, actual);

			transport.JW_ETD = 3.DaysAgo();
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETD is not recent since it is older than 3 days", false, actual);

			transport.JW_ETA = ZDate.Empty;
			transport.JW_ATA = ZDate.Empty;
			transport.JW_ETD = ZDate.Empty;
			transport.JW_ATD = 3.DaysAgo().AddMinutes(1);
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATD is recent since it is not older than 3 days", true, actual);

			transport.JW_ATD = 3.DaysAgo();
			actual = transport.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATD is not recent since it is older than 3 days", false, actual);
		}

		public void TestIsArrivalOrDepartureDateRecent_JobSailing()
		{
			var sailing = Factory.New<JobSailing>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var voyage = Factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			sailing.Destination.JB_E_ARV = 2.DaysAgo().AddMinutes(1);
			var actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETA is recent since it is not older than 2 days", true, actual);

			sailing.Destination.JB_E_ARV = 2.DaysAgo();
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETA is not recent since it is older than 2 days", false, actual);

			sailing.Destination.JB_E_ARV = ZDate.Empty;
			sailing.Destination.JB_A_ARV = 2.DaysAgo().AddMinutes(1);
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATA is recent since it is not older than 2 days", true, actual);

			sailing.Destination.JB_A_ARV = 2.DaysAgo();
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATA is not recent since it is older than 2 days", false, actual);

			sailing.Destination.JB_E_ARV = ZDate.Empty;
			sailing.Destination.JB_A_ARV = ZDate.Empty;
			sailing.Origin.JA_E_DEP = 3.DaysAgo().AddMinutes(1);
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETD is recent since it is not older than 3 days", true, actual);

			sailing.Origin.JA_E_DEP = 3.DaysAgo();
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ETD is not recent since it is older than 3 days", false, actual);

			sailing.Destination.JB_E_ARV = ZDate.Empty;
			sailing.Destination.JB_A_ARV = ZDate.Empty;
			sailing.Origin.JA_E_DEP = ZDate.Empty;
			sailing.Origin.JA_A_DEP = 3.DaysAgo().AddMinutes(1);
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATD is recent since it is not older than 3 days", true, actual);

			sailing.Origin.JA_A_DEP = 3.DaysAgo();
			actual = sailing.AreArrivalAndDepartureDatesRecent();
			AssertEquals("ATD is not recent since it is older than 3 days", false, actual);
			AssertEquals(false, actual);
		}

		public void TestIsArrivalAndDepartureDatesRecent_JobSailing_WithoutDestinationOrOrigin()
		{
			var sailing = Factory.New<JobSailing>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var voyage = Factory.New<JobVoyage>();

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			sailing.JX_JA = ZGuid.Empty;
			sailing.JX_JB = destination.PK;

			AssertNoExceptionThrown(
				"Should not throw an exception without an origin",
				() => sailing.AreArrivalAndDepartureDatesRecent()
			);

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = ZGuid.Empty;

			AssertNoExceptionThrown(
				"Should not throw an exception without a destination",
				() => sailing.AreArrivalAndDepartureDatesRecent()
			);
		}

		public void TestConsolShouldCreateSBREvent()
		{
			CombineAssertions(() =>
			{
				TestCaseConsolShouldCreateSBREvent(enableAWBTracking: true, TransportModes.Air, 3.DaysAgo().AddMinutes(1), 2.DaysAgo().AddMinutes(1), expectToCreateSBR: true);
				TestCaseConsolShouldCreateSBREvent(enableAWBTracking: false, TransportModes.Air, 3.DaysAgo().AddMinutes(1), 2.DaysAgo().AddMinutes(1), expectToCreateSBR: false);
				TestCaseConsolShouldCreateSBREvent(enableAWBTracking: true, TransportModes.Air, 3.DaysAgo(), 2.DaysAgo(), expectToCreateSBR: false);
				TestCaseConsolShouldCreateSBREvent(enableAWBTracking: true, TransportModes.Sea, 300.DaysAgo(), 200.DaysAgo(), expectToCreateSBR: true);
			});
		}

		void TestCaseConsolShouldCreateSBREvent(bool enableAWBTracking, ZString transportMode, ZDateTime etd, ZDateTime eta, bool expectToCreateSBR)
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableAWBTracking))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = transportMode;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "SGSIN";
				consol.JK_MasterBillNum = "08187443521";
				consol.JK_BookingReference = "BKREF";
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>()
					.WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = etd;
				transport.JW_ETA = eta;

				var refCon = Factory.LoadTop1<RefContainer>(new CargoWise.EntityFramework.ZQuery());
				if (refCon == null)
				{
					refCon = Factory.New<RefContainer>();
					refCon.RC_TareWeight = 2000;
				}
				
				var consolContainer = consol.Containers.AddNew();
				consolContainer.JC_ContainerNum = "IRSU5676476";
				consolContainer.JC_RC = refCon.PK;

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals($"Should{(expectToCreateSBR ? " " : " NOT ")}create SBR event", expectToCreateSBR ? 1 : 0, logs.Count);

				if (expectToCreateSBR)
				{
					AssertContains(transportMode == TransportModes.Air ? "TYP=AWB Automation" : "TYP=Container Tracking", logs[0].SL_Reference);
				}

				consol.Delete();
				Factory.Save();
			}
		}
	}
}
