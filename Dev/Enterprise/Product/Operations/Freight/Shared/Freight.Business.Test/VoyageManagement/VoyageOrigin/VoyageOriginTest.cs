using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Test;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Agency;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageOrigin))]
	sealed class VoyageOriginTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2016, 3, 3)]
		public void TestOnSaving_SBREventCreated()
		{
			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDE";
			vessel.RV_LloydsNumber = "9463085";
			vessel.RV_OH = carrier.PK;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "ASD1234";

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyageOrigin.JA_E_DEP = DateTime.Today;
			voyageOrigin.JA_A_DEP = DateTime.Today;

			Factory.Save();

			var log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUBNE", log.Parameters[Params.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[Params.Type]);

			var previousLogPk = log.PK;
			voyageOrigin.JA_RL_NKPortOfLoading = "AUSYD";

			Factory.Save();

			log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUSYD|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertNotEquals(previousLogPk, log.PK);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUSYD", log.Parameters[Params.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[Params.Type]);

			previousLogPk = log.PK;
			voyageOrigin.JA_E_DEP = 1.DaysAgo();
			voyageOrigin.JA_A_DEP = 1.DaysAgo();

			Factory.Save();

			log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUSYD|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals(previousLogPk, log.PK);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUSYD", log.Parameters[Params.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[Params.Type]);

			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			voyageOrigin.JA_RL_NKPortOfLoading = "AUMEL";

			Factory.Save();

			log = voyageOrigin.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUMEL|TYP=Schedule Feed");
			AssertNull(log);
		}

		[TestDate(2023, 04, 13)]
		public void TestOnSaving_DEP_EventCreated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ABSDE";
			vessel.RV_LloydsNumber = "9463085";
			vessel.RV_OH = carrier.PK;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "ASD1234";
			voyage.JV_AirSeaRoad = "SEA";

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyageOrigin.JA_E_DEP = ZDateTime.Today;
			voyageOrigin.JA_A_DEP = ZDateTime.Today;

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Departure, "|FAC=CTO|LOC=AUBNE|MOD=SEA");
			AssertNotNull("DEP event", log);

			var previousLogPk = log.PK;
			voyageOrigin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			voyageOrigin.JA_A_DEP = ZDateTime.Today.AddDays(1);

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure, "|FAC=CTO|LOC=AUBNE|MOD=SEA");
			AssertNotNull("DEP event", log);
			AssertNotEquals(previousLogPk, log.PK);
		}

		public void TestOnSaving_UpdateFlightSubscriptionEvent_OriginHasChanges()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = 2.DaysAgo();

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "SGSIN";
				destination.JB_E_ARV = 1.DaysAgo();

				voyage.GenerateSailings();
				Factory.Save();

				var expectedEventReference = $"|FDT={origin.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001";
				AssertEquals("Precondition", 1, voyage.Sailings.Count);
				var sailing = voyage.Sailings[0];
				AssertFlightSubscriptionEvent("Precondition", sailing, 1, expectedEventReference);

				origin.JA_RL_NKPortOfLoading = "NZAKL";
				Factory.Save();
				AssertFlightSubscriptionEvent("Loading Port changed, should create a new SBR event", sailing, 2, expectedEventReference);

				origin.JA_E_DEP = 2.DaysAgo().AddMinutes(1);
				Factory.Save();
				AssertFlightSubscriptionEvent("ETD changed, should create a new SBR event", sailing, 3, $"|FDT={origin.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001");

				origin.JA_A_DEP = ZDateTime.Now;
				Factory.Save();
				AssertFlightSubscriptionEvent("ATD changed, should create a new SBR event", sailing, 4, $"|FDT={origin.JA_A_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001");

				origin.JA_E_DEP = ZDateTime.Empty;
				Factory.Save();
				AssertFlightSubscriptionEvent("ETD is empty but ETA is not, SBR event should not be cancelled", sailing, 5, $"|FDT={origin.JA_A_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001");
			}
		}

		public void TestUpdateProxiedFieldsOnLinkedConsolTransportWhenChangesNotYetInDB()
		{
			var today = ZDateTime.Today;
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings[0];
			var sailing2 = voyage.Sailings[1];

			var transport1_1 = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport1_1.JW_IsLinked = true;
			transport1_1.JW_JX = sailing1.PK;

			var transport2_1 = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport2_1.JW_IsLinked = true;
			transport2_1.JW_JX = sailing2.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = orgHeader.Addresses.AddNew();
			arrivalAddress.Address1 = "arrival";

			var origin = (VoyageOrigin)voyage.Origins.Single();
			origin.JA_OA_DepartureCTOAddress = arrivalAddress.PK;
			origin.JA_RL_NKPortOfLoading = "LOAD";
			origin.JA_S_DEP = today;
			origin.JA_E_DEP = today.AddDays(1);
			origin.JA_A_DEP = today.AddDays(2);

			Assert("Precondition: transport 1 is not in db", !transport1_1.IsInDatabase);
			Assert("Precondition: transport 2 is not in db", !transport2_1.IsInDatabase);
			Assert("Precondition: voyage is not in db", !voyage.IsInDatabase);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transport1_2 = newFactory.Load<Transport>(transport1_1.PK);
			var transport2_2 = newFactory.Load<Transport>(transport2_1.PK);
			var transports = new Transport[] { transport1_2, transport2_2 };

			var proxiedTransportFieldsToOriginFieldsDict = new Dictionary<ZString, ZPropertyInfo>()
			{
				{ JobConsolTransportSchema.Constants.JW_OA_DepartureLocation, origin.JA_OA_DepartureCTOAddressInfo },
				{ JobConsolTransportSchema.Constants.JW_RL_NKLoadPort, origin.JA_RL_NKPortOfLoadingInfo },
				{ JobConsolTransportSchema.Constants.JW_STD, origin.JA_S_DEPInfo },
				{ JobConsolTransportSchema.Constants.JW_ETD, origin.JA_E_DEPInfo },
				{ JobConsolTransportSchema.Constants.JW_ATD, origin.JA_A_DEPInfo },
			};

			transports.ForEach(transport => proxiedTransportFieldsToOriginFieldsDict.ForEach(pair =>
				AssertEquals(pair.Value.Name + " should propogate to linked transport when data not in db.", pair.Value.Value, ((ZPropertyInfo)transport[pair.Key + "Info"]).OriginalValue)));
		}

		public void TestOnSaving_SBREventLog_OriginWithNoIATACode()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = 5.DaysAgo();

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "SGSIN";
				destination.JB_E_ARV = 1.DaysAgo();

				voyage.GenerateSailings();
				Factory.Save();

				AssertEquals("Precondition", 1, voyage.Sailings.Count);
				var sailing = voyage.Sailings[0];

				var logs = sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();
				AssertEquals("Precondition: should be created SBR event", 1, logs.Count);
				AssertEquals("Precondition: SBR event is not cancelled", false, logs[0].IsCancelled);
				AssertEquals("Precondition: load port IATA Code", "SYD", origin.PortOfLoading.RL_IATA);
				AssertEquals("Precondition: discharge port IATA Code", "SIN", destination.PortOfDischarge.RL_IATA);

				origin.PortOfLoading.RL_IATA = "";
				origin.JA_E_DEP = 3.DaysAgo();
				Factory.Save();

				logs = sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();
				AssertEquals("Should not create new SBR event", 1, logs.Count);
				AssertEquals("Old SBR event is cancelled", true, logs[0].IsCancelled);
			}
		}

		void AssertFlightSubscriptionEvent(string message, JobSailing sailing, int expectedLogCount, string expectedEventReference, bool expectedEventIsCancelled = false)
		{
			var sailingLogs = sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

			AssertNotNull(sailingLogs);
			AssertEquals(expectedLogCount, sailingLogs.Count);
			AssertEquals("Event Reference", expectedEventReference, sailingLogs[0].SL_Reference);
			AssertEquals("Event IsCancelled", expectedEventIsCancelled, sailingLogs[0].SL_IsCancelled);

			for (int i = 1; i < sailingLogs.Count; i++)
			{
				var log = sailingLogs[i];
				AssertNotNull(log);
				AssertEquals("Old SBR event should be cancelled", true, log.SL_IsCancelled);
			}
		}

		public void TestDepartureEventHasBeenAddedToVoyage_UpdateJA_A_DEP()
		{
			Action<JobVoyage, VoyageOrigin, ZString, ZDateTime, ZDateTime> assert = (voyage, origin, eventLocation, eventDate, expectedPropertyValue) =>
			{
				origin.JA_A_DEP = ZDateTime.Empty;
				voyage.Logs.RemoveAndDeleteAll();

				voyage.Logs.CreateOrRecreateEventLog(Events.Departure, EstimateActual.Actual, eventDate.ToOffset(), "MCLAREN", new KeyValuePair<string, string>("LOC", eventLocation));
				AssertEquals("JA_A_DEP", expectedPropertyValue, origin.JA_A_DEP);
			};

			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			var origin1 = voyage1.Origins.AddNew().In("UAIEV");
			var origin2 = voyage1.Origins.AddNew().In("AUSYD");

			assert(voyage1, origin1, "UAIEV", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 1));
			assert(voyage1, origin2, "AUSYD", new ZDateTime(2013, 2, 2), new ZDateTime(2013, 2, 2));
		}

		#region CutOff event

		public void TestSave_CutOffDateUpdated_GenerateCutOffEvent()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_CutOff = 5.DaysAgo();

			// No CutOff event, create it
			Factory.Save();
			var log = voyage.Logs.MostRecentLogByEventTime(Events.CutOffDate);
			AssertNotNull("COF event", log);
			AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[Params.Facility]);

			// Location modified, cancel old event and create new
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			Factory.Save();

			Assert("Old event is cancelled", log.IsCancelled);

			var newLog = voyage.Logs.MostRecentLogByEventTime(Events.CutOffDate);
			AssertEquals("Location", "AUSYD", newLog.Parameters[Params.Location]);
		}

		#endregion

		#region ReceiptCommenced Event

		public void TestSave_ReceivalCommencedDateUpdated_GenerateReceiptCommencedEvent()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_ReceivalCommences = 5.DaysAgo();

			Factory.Save();
			var log = voyage.Logs.MostRecentLogByEventTime(Events.ReceiptCommenced);
			Assert("Is Actual", !log.SL_IsEstimate);
			AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[Params.Facility]);
		}

		#endregion

		#region EstimatedDeparture Event

		public void TestSave_DepartureEstimatedDateUpdated_GenerateDepartureEstimatedEvent()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_E_DEP = 5.DaysAgo();

			Factory.Save();
			var log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			Assert("Estimated DEP event", log.SL_IsEstimate);
			AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[Params.Facility]);
		}

		#endregion

		public void TestLogSTWReportingLicense()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.FillWithValidTestData();
			var destination = voyage.Destinations.AddNew();
			destination.FillWithValidTestData();
			origin.StowPlanMessageStatus = MessageStatusListSTW.Codes.Sending;

			Factory.Save();

			origin.StowPlanMessageStatus = MessageStatusListSTW.Codes.AcceptanceWithWarnings;
			var msg = Factory.New<StowPlanLicenceLoggingExtensionsTest.DummyStowPlanMessage>();
			msg.AcceptedContainersWhichPreviouslyNotAccepted = 1;
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg.EM_SystemCreateTimeUtc = ZDateTime.Now;
			origin.Messages.Add(msg);

			Factory.Save();

			var license = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.StowPlanReporting.Name))[0];
			AssertNotNull(license);
			Assert(license.IsInDatabase);
		}

		public void TestUTCDates()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "BRRIO";
			origin.JA_E_DEP = new ZDateTime(2011, 9, 10, 12, 0, 0);
			origin.JA_A_DEP = new ZDateTime(2011, 9, 12, 15, 0, 0);

			//BRRIO is 3 hours behind UTC
			AssertEquals("JA_E_DEP_UTC", new ZDateTime(2011, 9, 10, 15, 0, 0), origin.JA_E_DEP_UTC);
			AssertEquals("JA_A_DEP_UTC", new ZDateTime(2011, 9, 12, 18, 0, 0), origin.JA_A_DEP_UTC);
		}

		public void TestMarkVoyageAsNeedingValidationOnRemove()
		{
			ZDateTime now = ZDateTime.Now;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "003";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = now.AddDays(1);

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = now.AddDays(-1);

			voyage.RunPreSaveValidation();

			voyage.Origins.RemoveAndDelete(origin);

			AssertEquals("Should have no origins", 0, voyage.Origins.Count);

			voyage.RunPreSaveValidation();

			AssertEquals("Should have a origin added", 1, voyage.Origins.Count);
		}

		public void TestATDUpdateNotification()
		{
			var subscriber = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var queryProvider1 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var queryProvider2 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var providerDelegate = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>(MockBehavior.Strict);

			var list = new ArrayList { subscriber.Object };

			JobVoyage voyage;
			{
				var createFactory = new BusinessObjectFactory();

				var createVoyage = createFactory.New<JobVoyage>();

				VoyageOrigin origin1 = createVoyage.Origins.AddNew();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_A_DEP = new ZDateTime(2009, 07, 08);

				VoyageOrigin origin2 = createVoyage.Origins.AddNew();
				origin2.JA_RL_NKPortOfLoading = "AUBNE";
				origin2.JA_A_DEP = new ZDateTime(2009, 07, 13);

				VoyageDestination destination = createVoyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NLAMS";

				createFactory.Save();

				voyage = Factory.Load<JobVoyage>(createVoyage.PK);
			}

			voyage.GenerateSailings();

			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate.Object);

			VoyageOrigin originSYD = voyage.Origins.GetOriginFromLoading("AUBNE");

			using (ObjectFactory.Substitute(ScheduleUpdateSubscriberFactoryTest.ObjectName, list))
			{
				subscriber
					.Setup(m => m.ATDChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						originSYD,
						new ZDateTime(2009, 07, 13)))
					.Callback(
						(ScheduleUpdateSubscriberFactoryTest.DoETDChanged)delegate (
							IScheduleUpdateServices services,
							VoyageOrigin origin,
							ZDateTime oldETD)
						{
							AssertEquals(true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
							AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
						});

				providerDelegate.Setup(m => m()).Returns(queryProvider1.Object);
				originSYD.JA_A_DEP = new ZDateTime(2009, 07, 14);
				subscriber
					.Setup(m => m.ATDChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						originSYD,
						new ZDateTime(2009, 07, 14)))
					.Callback(
						(ScheduleUpdateSubscriberFactoryTest.DoETDChanged)delegate (
							IScheduleUpdateServices services,
							VoyageOrigin origin,
							ZDateTime oldETD)
						{
							AssertEquals(true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
							AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
						});

				// cache the query providers within calls to ATDChanged but not between calls.
				providerDelegate.Setup(m => m()).Returns(queryProvider2.Object);
				originSYD.JA_A_DEP = new ZDateTime(2009, 07, 15);
			}
		}

		public void TestATDUpdate_Sea_DoNotSyncroniseOriginsWithTheSameVesselVoyage()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Visund";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "123";

			var oldATD = ZDateTime.Now;

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_A_DEP = oldATD.AddDays(11);

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			voyage2.JV_VoyageFlight = "123";

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_A_DEP = oldATD.AddDays(21);

			AssertEquals("ATD date was not synchronised on matching voyage", oldATD.AddDays(11), origin1.JA_A_DEP);
		}

		public void TestATDUpdate_NonSea_DoNotSyncroniseOriginsWithTheSameVesselVoyage()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Visund";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage1.JV_VoyageFlight = "123";
			voyage1.JV_RV_NKVessel = vessel.RV_FK;

			ZDateTime aTD = ZDateTime.Now;

			var origin11 = voyage1.Origins.AddNew();
			origin11.JA_RL_NKPortOfLoading = "AUSYD";
			origin11.JA_A_DEP = aTD;

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage2.JV_VoyageFlight = "123";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;

			var origin21 = voyage2.Origins.AddNew();
			origin21.JA_RL_NKPortOfLoading = "AUSYD";
			origin21.JA_A_DEP = aTD.AddDays(10);

			AssertEquals("ATD not syncronised for non-sea voyages", aTD, origin11.JA_A_DEP);
		}

		public void TestETDUpdateNotification()
		{
			var subscriber = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var queryProvider1 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var queryProvider2 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var providerDelegate = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>(MockBehavior.Strict);

			ArrayList list = new ArrayList { subscriber.Object };

			JobVoyage voyage;
			{
				var createFactory = new BusinessObjectFactory();

				JobVoyage createVoyage = createFactory.New<JobVoyage>();

				VoyageOrigin origin1 = createVoyage.Origins.AddNew();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_E_DEP = new ZDateTime(2009, 07, 08);

				VoyageOrigin origin2 = createVoyage.Origins.AddNew();
				origin2.JA_RL_NKPortOfLoading = "AUBNE";
				origin2.JA_E_DEP = new ZDateTime(2009, 07, 13);

				VoyageDestination destination = createVoyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NLAMS";

				createFactory.Save();

				voyage = Factory.Load<JobVoyage>(createVoyage.PK);
			}

			voyage.GenerateSailings();

			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate.Object);

			VoyageOrigin originSYD = voyage.Origins.GetOriginFromLoading("AUBNE");

			using (ObjectFactory.Substitute(ScheduleUpdateSubscriberFactoryTest.ObjectName, list))
			{
				subscriber
					.Setup(m => m.ETDChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						originSYD,
						new ZDateTime(2009, 07, 13)))
					.Callback((ScheduleUpdateSubscriberFactoryTest.DoETDChanged)delegate (
						IScheduleUpdateServices services,
						VoyageOrigin origin,
						ZDateTime oldETD)
					{
						AssertEquals(true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
						AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
					});

				providerDelegate.Setup(m => m()).Returns(queryProvider1.Object);
				originSYD.JA_E_DEP = new ZDateTime(2009, 07, 14);

				subscriber
					.Setup(m => m.ETDChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						originSYD,
						new ZDateTime(2009, 07, 14)))
					.Callback((ScheduleUpdateSubscriberFactoryTest.DoETDChanged)delegate (
						IScheduleUpdateServices services,
						VoyageOrigin origin,
						ZDateTime oldETD)
					{
						AssertEquals(true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
						AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
					});

				// cache the query providers within calls to ETDChanged but not between calls.
				providerDelegate.Setup(m => m()).Returns(queryProvider2.Object);
				originSYD.JA_E_DEP = new ZDateTime(2009, 07, 15);
			}
		}

		public void TestDeleteAlsoDeletesAttachedMessages()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			EDIMessage message = origin.Messages.AddNew();

			origin.Delete();
			AssertEquals("the message should be deleted", true, message.IsDeleted);
		}

		public void TestDefaultingCTO()
		{
			OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			OrgCarrierAppointedAgentPorts map = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			map.O5_PortOrCountry = "AUBNE";
			map.O5_OA_AgentOfficeAddress = cto.MainAddress.PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_OH_Line = carrier.PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals(cto.MainAddress, origin.DepartureCTOAddress);
		}

		public void TestISlotAllocationParentCode()
		{
			ISlotAllocationParent parent = Factory.New<VoyageOrigin>();
			AssertEquals(JobVoyOriginSchema.Constants.Prefix, parent.Code);
		}

		public void TestJA_DepartReferenceFieldType()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			var origin = voyage.Origins.AddNew();

			origin.JA_RL_NKPortOfLoading = "ZACAS";
			AssertEquals(nameof(FieldType.Text), origin.JA_DepartReferenceFieldType);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				origin.JA_RL_NKPortOfLoading = "AUBNE";
				AssertEquals(nameof(FieldType.Text), origin.JA_DepartReferenceFieldType);

				origin.JA_RL_NKPortOfLoading = "ZACAS";
				AssertEquals(nameof(FieldType.TextCodeFindBox), origin.JA_DepartReferenceFieldType);

				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				AssertEquals(nameof(FieldType.Text), origin.JA_DepartReferenceFieldType);
			}
		}

		public void TestHumanReadableName()
		{
			VoyageOrigin origin = Factory.New<JobVoyage>().Origins.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals("Sailing Schedule (Vessel='', Voyage='', Carrier=''), Origin = 'AUBNE'", origin.HumanReadableName);

			origin.JA_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("Sailing Schedule (Vessel='', Voyage='', Carrier=''), Origin = 'NZAKL'", origin.HumanReadableName);
		}

		public void TestClone()
		{
			Origin.JA_E_DEP = ZDateTime.Now;

			SlotAllocation allocation = Origin.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect("AS1", 5);

			VoyageOrigin clonedOrigin = (VoyageOrigin)Origin.Clone();
			AssertEquals("Should clone properties", Origin.JA_E_DEP, clonedOrigin.JA_E_DEP);

			SlotAllocation allocationClone = clonedOrigin.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertEquals(5m, allocationClone.GetAspect("AS1"));

			AssertNotEquals("Should exclude JA_JV", clonedOrigin.JA_JV, Origin.JA_JV);
		}

		public void TestScheduleDateChangeLogged()
		{
			Origin.JA_RL_NKPortOfLoading = "MYPKG";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Factory.Save();

			Origin.JA_E_DEP = new ZDateTime(2000, 1, 1);
			Factory.Save();
			JobScheduleChangeLoggerTest.AssertLastDateChangeLogged(Factory, ScheduleDateTypes.Codes.ETD, ZDateTime.Empty, Origin.JA_E_DEP);
		}

		public void TestEditLogRaisedOnJobVoyageOnSave()
		{
			Origin.Factory.Save();
			AssertEquals("No edit log initially", null, Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			VoyageOrigin loadedOrigin = newFactory.Load<VoyageOrigin>(Origin.PK);
			loadedOrigin.JA_E_DEP = ZDateTime.Now;
			newFactory.Save();
			AssertNotNull("Edit log raised on edit of VoyageOrigin", loadedOrigin.Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));
		}

		public void TestVoyage()
		{
			AssertEquals("Origin.JA_JV and Voyage are equal, no error expected", Origin.JA_JV, Origin.Voyage.PK);
			Assert("Origin.Voyage.PK and Voyage2 are not equal, error expected", Origin.Voyage.PK != Voyage2.PK);
			Assert("Origin.JA_JV and Voyage2 are not equal, error expected", Origin.JA_JV != Voyage2.PK);
		}

		public void TestMessages()
		{
			AssertNotNull("Messages", Origin.Messages);
		}

		public void TestJB_Calc_ArrivalCTOPremiseID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = header.PK;
			address.OA_Code = "CUCKOOSQUEAKER";
			address.LocalControlledPremisesID = "9914N";
			Origin.JA_OA_DepartureCTOAddress = address.PK;
			AssertEquals("9914N", Origin.JA_Calc_DepartureCTOPremiseID);
		}

		public void TestJA_RL_NKPortOfLoading_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Origin.JA_JV = Voyage.PK;
				Origin.JA_RL_NKPortOfLoading = "USLAX";
				AssertEquals("Expected SailingScheduleDataVendor.UpdateVoyageOrigin to be called", true, MockSailingScheduleDataVendor.Instance.UpdateVoyageOriginCalled);
			}
			finally
			{
				MockSailingScheduleDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestScheduleChangeParent()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			IScheduleChangeParent parent = voyage.Origins[0];

			CombineAssertions(delegate
			{
				AssertEquals("DestinationPort", "", parent.DestinationPort);
				AssertEquals("Factory", Factory, parent.Factory);
				AssertEquals("OriginPort", "AUBNE", parent.OriginPort);
				AssertEquals("PK", voyage.Origins[0].PK, parent.PK);
				AssertEquals("SailingRefColumn", JobSailingSchema.JX_JA, parent.SailingRefColumn);
				AssertEquals("TablePrefix", JobVoyOriginSchema.Constants.Prefix, parent.TablePrefix);
				AssertEquals("Voyage", voyage, parent.Voyage);
			});
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_A_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_A_DEP.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_E_DEP.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_DGReceivalCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_DGCutOff.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_E_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_CutOff.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_ReceivalCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Origin.JA_DocumentaryCutoff.Kind);
		}

		public void TestDepartureEvent()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNull("DEP log not yet created", log);

			origin.JA_A_DEP = ZDateTime.Today.AddDays(10);
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("DEP log created", log);
			AssertEquals("event date matches origin departure date", origin.JA_A_DEP, log.SL_EventTime);
			AssertEquals("location in log", "AUSYD", log.Parameters[Params.Location]);

			origin.JA_A_DEP = ZDateTime.Empty;
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNull("DEP log cancelled", log);

			origin.JA_A_DEP = ZDateTime.Today.AddDays(11);
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("DEP log created", log);
			AssertEquals("event date matches origin departure date", origin.JA_A_DEP, log.SL_EventTime);
			AssertEquals("location in log", "AUSYD", log.Parameters[Params.Location]);

			origin.JA_RL_NKPortOfLoading = "AUMEL";
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("DEP log created", log);
			AssertEquals("event date matches origin departure date", origin.JA_A_DEP, log.SL_EventTime);
			AssertEquals("location in log", "AUMEL", log.Parameters[Params.Location]);

			voyage = Factory.New<JobVoyage>();

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNull("ARV log not yet created", log);

			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_A_DEP = ZDateTime.Today.AddDays(11);

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("DEP log created", log);
			AssertEquals("event date matches origin departure date", origin.JA_A_DEP, log.SL_EventTime);
			AssertEquals("location in log", "AUSYD", log.Parameters[Params.Location]);
		}

		public void TestDepartureEvent_FlightDatePapameter()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = new ZDateTime(2017, 06, 27);

			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2017, 06, 26);

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertEquals("2017-06-27", log.Parameters[Params.FlightDate]);
		}

		public void TestJA_E_DEP_IsEventDateProperty()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = new ZDateTime(2017, 06, 27);

			var origin = voyage.Origins.AddNew();

			AssertEquals(origin.JA_E_DEPInfo.Name, EventDatePropertyAttribute.FindPropertyInfos(origin, Events.Departure, EstimateActual.Estimate).FirstOrDefault().Property.Name);
		}

		public void TestDEPAndCOFEventsAreCancelledAndRecreatedWhenLoadPortChanges()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_CutOff = ZDate.Today.AddDays(-5);
			origin.JA_E_DEP = ZDate.Today.AddDays(1);

			Factory.Save();

			var cutOffLog = voyage.Logs.MostRecentLogByEventTime(Events.CutOffDate);
			AssertNotNull("COF event", cutOffLog);
			AssertEquals("Event date", ZDate.Today.AddDays(-5), cutOffLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", cutOffLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, cutOffLog.Parameters[Params.Facility]);

			var depLog = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("DEP event", depLog);
			AssertEquals("Event date", ZDate.Today.AddDays(1), depLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", depLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, depLog.Parameters[Params.Facility]);

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			Factory.Save();

			Assert("Old Cutoff event is cancelled", cutOffLog.IsCancelled);
			Assert("Old Dep event is cancelled", depLog.IsCancelled);

			var newCutOffLog = voyage.Logs.MostRecentLogByEventTime(Events.CutOffDate);
			AssertEquals("Location", "AUSYD", newCutOffLog.Parameters[Params.Location]);

			var newDepLog = voyage.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertEquals("Location", "AUSYD", newCutOffLog.Parameters[Params.Location]);
		}

		public void TestParentWorkflowProviders()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();

			IWorkflowTriggerFieldChangeSource workflowTriggerChangeSource = origin;

			AssertContainsExactElementsInAnyOrder(workflowTriggerChangeSource.ParentWorkflowProviders, new[] { voyage });
		}

		public void TestDeletedVoyageOrigin()
		{
			var origin = Factory.New<VoyageOriginSendersMessageReferenceTest.TestHelperVoyageOrigin>();
			origin.Delete();

			AssertNoExceptionThrown("Exception should not be thrown here", origin.InvokeOnFactorySavingBeforeTransactionCore);
		}

		public void TestBerthCodeDefaultsFromCTODepartureAddressOrganisation()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAODS";
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "BSL", Constants.CountryCodes.Germany);

			AssertEquals("Prerequisite: JA_Berth is empty", ZString.Empty, origin.JA_Berth);
			origin.JA_OA_DepartureCTOAddress = orgHeader.MainAddress.PK;

			AssertEquals("Should not be populated for UAODS", ZString.Empty, origin.JA_Berth);

			origin.JA_OA_DepartureCTOAddress = ZGuid.Empty;
			origin.JA_RL_NKPortOfLoading = "DEHAM";
			AssertEquals("Prerequisite: JA_Berth is empty", ZString.Empty, origin.JA_Berth);

			origin.JA_OA_DepartureCTOAddress = ZGuid.NewZGuid();
			AssertEquals("JA_Berth is still empty", ZString.Empty, origin.JA_Berth);

			origin.JA_OA_DepartureCTOAddress = orgHeader.MainAddress.PK;
			AssertEquals("JA_Berth should be set", "BSL", origin.JA_Berth);

			orgCusCode.OK_CustomsRegNo = "123456789012345";
			origin.JA_OA_DepartureCTOAddress = ZGuid.Empty;
			origin.JA_Berth = string.Empty;
			AssertEquals("Prerequisite: JA_Berth is empty", ZString.Empty, origin.JA_Berth);

			origin.JA_OA_DepartureCTOAddress = orgHeader.MainAddress.PK;
			AssertEquals("JA_Berth should be truncated", "1234567890", origin.JA_Berth);
		}

		public override void TestFetchForLoad()
		{
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();
			factoryForLoad.ResetDatabaseLoadCount();
			factoryForLoad.LoadTop1<VoyageOrigin>(new ZQuery(JobVoyOriginSchema.PK, Origin.PK));

			AssertMaxDbHits("With a clean factory only Origin is loaded", 1, factoryForLoad);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestSailingOnlineMatchStatusRefreshedOn_E_DEP_Update()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF69";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_E_DEP = ZDate.Today;

				var destination1 = Factory.New<VoyageDestination>();
				destination1.JB_E_ARV = ZDate.Today;
				var destination2 = Factory.New<VoyageDestination>();
				destination2.JB_E_ARV = ZDate.Today;

				var refUNLOCO1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var refUNLOCO2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
				var refUNLOCO3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");

				voyage.Origins.Add(origin);
				voyage.Destinations.Add(destination1);
				voyage.Destinations.Add(destination2);

				origin.JA_RL_NKPortOfLoading = refUNLOCO1.RL_Code;
				destination1.JB_RL_NKPortOfDischarge = refUNLOCO2.RL_Code;
				destination2.JB_RL_NKPortOfDischarge = refUNLOCO3.RL_Code;

				voyage.GenerateSailings();
				AssertEquals("Pre: Expected 2 sailing schedules generated.", 2, voyage.Sailings.Count);

				var originSailings = origin.FetchSailings();
				AssertEquals("Pre: Expected 2 sailing schedules for this origin.", 2, originSailings.Length);

				originSailings.ForEach(sailing => sailing.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched);
				origin.JA_E_DEP = new ZDate(2019, 1, 1);
				CombineAssertions("Expected all sailings to have their online schedule status updated when estimated departure date changed.", () =>
				{
					originSailings.ForEach(sailing => AssertEquals(Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus));
				});
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestSailingOnlineMatchStatusRefreshedOn_PortOfLoading_Update()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF69";

				var origin = Factory.New<VoyageOrigin>();
				origin.JA_E_DEP = ZDate.Today;

				var destination1 = Factory.New<VoyageDestination>();
				destination1.JB_E_ARV = ZDate.Today;
				var destination2 = Factory.New<VoyageDestination>();
				destination2.JB_E_ARV = ZDate.Today;

				var refUNLOCO1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var refUNLOCO2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
				var refUNLOCO3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
				var refUNLOCO4 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

				voyage.Origins.Add(origin);
				voyage.Destinations.Add(destination1);
				voyage.Destinations.Add(destination2);

				origin.JA_RL_NKPortOfLoading = refUNLOCO1.RL_Code;
				destination1.JB_RL_NKPortOfDischarge = refUNLOCO2.RL_Code;
				destination2.JB_RL_NKPortOfDischarge = refUNLOCO3.RL_Code;

				voyage.GenerateSailings();
				AssertEquals("Pre: Expected 2 sailing schedules generated.", 2, voyage.Sailings.Count);

				var originSailings = origin.FetchSailings();
				AssertEquals("Pre: Expected 2 sailing schedules for this origin.", 2, originSailings.Length);

				originSailings.ForEach(sailing => sailing.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched);
				origin.JA_RL_NKPortOfLoading = refUNLOCO4.RL_Code;
				CombineAssertions("Expected all sailings to have their online schedule status updated when eport of loading changed.", () =>
				{
					originSailings.ForEach(sailing => AssertEquals(Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus));
				});
			});
		}

		public void TestAdditionalValidation()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "INBOM";

			var transport = consol.Transports[0];
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111S";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			AssertNotNull(sailing.Origin.AdditionalValidation);
			AssertType(typeof(TransportVoyOriginAdditionalValidation), sailing.Origin.AdditionalValidation);
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = voyage.Sailings[0].PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var mainSeaLeg = agencyBooking1.Transports.Cast<Transport>()
				.FirstOrDefault(transport =>
					transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel &&
					transport.JW_TransportMode == Core.Constants.TransportModes.Sea);
			mainSeaLeg.JW_ATD = ZDateTime.Today;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.JS_JX = voyage.Sailings[0].PK;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.JS_JX = voyage.Sailings[0].PK;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.JS_JX = voyage.Sailings[0].PK;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking4.JS_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.JS_JX = voyage.Sailings[0].PK;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			Factory.Save();

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);
				registry.Setup(m => m.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(ZGuid.Empty)).Returns(true);

				origin.JA_RL_NKPortOfLoading = "AUSYD";

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

				Factory.Save();

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_ReceivalCommences = new ZDateTime(2023, 09, 01);
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_CutOff = new ZDateTime(2023, 09, 02);
				Factory.Save();

				AssertEquals(3, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(3, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_E_DEP = new ZDateTime(2023, 09, 03);
				Factory.Save();

				AssertEquals(4, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(4, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_A_DEP = new ZDateTime(2023, 09, 04);
				Factory.Save();

				AssertEquals(4, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(4, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_DocumentaryCutoff = new ZDateTime(2023, 09, 05);
				Factory.Save();

				AssertEquals(5, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(5, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_VGMCutOff = new ZDateTime(2023, 09, 06);
				Factory.Save();

				AssertEquals(6, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(6, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent_SailingLinkToNonMainSeaTransport()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencybooking1MainTrasnport = agencyBooking1.Transports.AddNew();
			agencybooking1MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking1MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking1MainTrasnport.JW_IsLinked = false;

			var agencybooking1OtherTrasnport = agencyBooking1.Transports.AddNew();
			agencybooking1OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking1OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking1OtherTrasnport.JW_IsLinked = true;
			agencybooking1OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking2 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking2.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencybooking2MainTrasnport = agencyBooking2.Transports.AddNew();
			agencybooking2MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking2MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking2MainTrasnport.JW_IsLinked = false;

			var agencybooking2OtherTrasnport = agencyBooking2.Transports.AddNew();
			agencybooking2OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking2OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking2OtherTrasnport.JW_IsLinked = true;
			agencybooking2OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking3 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			agencyBooking3.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencybooking3MainTrasnport = agencyBooking3.Transports.AddNew();
			agencybooking3MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking3MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking3MainTrasnport.JW_IsLinked = false;

			var agencybooking3OtherTrasnport = agencyBooking3.Transports.AddNew();
			agencybooking3OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking3OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking3OtherTrasnport.JW_IsLinked = true;
			agencybooking3OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var billOfLading1 = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			billOfLading1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var billOfLading1MainTrasnport = billOfLading1.Transports.AddNew();
			billOfLading1MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			billOfLading1MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			billOfLading1MainTrasnport.JW_IsLinked = false;

			var billOfLading1OtherTrasnport = billOfLading1.Transports.AddNew();
			billOfLading1OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			billOfLading1OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			billOfLading1OtherTrasnport.JW_IsLinked = true;
			billOfLading1OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking4 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking4.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var agencybooking4MainTrasnport = agencyBooking4.Transports.AddNew();
			agencybooking4MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking4MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking4MainTrasnport.JW_IsLinked = false;

			var agencybooking4OtherTrasnport = agencyBooking4.Transports.AddNew();
			agencybooking4OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking4OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking4OtherTrasnport.JW_IsLinked = true;
			agencybooking4OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			var agencyBooking5 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			agencyBooking5.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

			var agencybooking5MainTrasnport = agencyBooking5.Transports.AddNew();
			agencybooking5MainTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			agencybooking5MainTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking5MainTrasnport.JW_IsLinked = false;

			var agencybooking5OtherTrasnport = agencyBooking5.Transports.AddNew();
			agencybooking5OtherTrasnport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			agencybooking5OtherTrasnport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			agencybooking5OtherTrasnport.JW_IsLinked = true;
			agencybooking5OtherTrasnport.JW_JX = voyage.Sailings[0].PK;

			Factory.Save();

			AssertEquals(ZGuid.Empty, agencyBooking1.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking2.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking3.JS_JX);
			AssertEquals(ZGuid.Empty, billOfLading1.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking4.JS_JX);
			AssertEquals(ZGuid.Empty, agencyBooking5.JS_JX);

			AssertEquals(voyage.Sailings[0].PK, agencybooking1OtherTrasnport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencybooking2OtherTrasnport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencybooking3OtherTrasnport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, billOfLading1OtherTrasnport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencybooking4OtherTrasnport.JW_JX);
			AssertEquals(voyage.Sailings[0].PK, agencybooking5OtherTrasnport.JW_JX);

			Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
				.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.ElectronicBookingAndShippingInstructions).Returns(true);

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_DocumentaryCutoff = new ZDateTime(2023, 09, 05);
				Factory.Save();

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				origin.JA_VGMCutOff = new ZDateTime(2023, 09, 06);
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				origin.JA_ReceivalCommences = new ZDateTime(2023, 09, 01);
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				origin.JA_CutOff = new ZDateTime(2023, 09, 02);
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				Assert(!agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				origin.JA_RL_NKPortOfLoading = "AUSYD";
				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				Factory.Save();

				AssertEquals(1, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(1, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));

				agencyBooking5.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				agencyBooking5.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Booked),
					new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				origin.JA_E_DEP = new ZDateTime(2023, 09, 03);
				Factory.Save();

				AssertEquals(2, agencyBooking1.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				AssertEquals(2, agencyBooking2.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking3.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!billOfLading1.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking4.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
				Assert(!agencyBooking5.Logs.GetAllLogs().Cast<StmALog>()
					.Any(x => x.Parameters.TryGetValue(Params.LogSubscriber, out var logSubscriber) && logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber));
			}
		}

		#region Implementation

		VoyageOrigin Origin;
		VoyageDestination Destination;
		JobVoyage Voyage;
		JobVoyage Voyage2;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Voyage2 = Factory.New<JobVoyage>();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Voyage.Origins.AddNew();

			Destination = Voyage.Destinations.AddNew();
			MockSailingScheduleDataVendor.Instance.IsVendorDataCurrent = true;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var voyage = factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			voyage.Destinations.AddNew();

			return origin;
		}

		protected override BusinessObject GetLogParentForEventDateProperty()
		{
			return Voyage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Origin;
		}

		#endregion
	}
}
