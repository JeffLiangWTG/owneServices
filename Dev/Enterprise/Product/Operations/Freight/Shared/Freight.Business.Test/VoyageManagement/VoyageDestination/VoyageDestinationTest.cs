using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
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
	[TestedType(typeof(VoyageDestination))]
	sealed class VoyageDestinationTest : EnterpriseBusinessObjectTestCase
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

			var voyageDestination = voyage.Destinations.AddNew();
			voyageDestination.JB_RL_NKPortOfDischarge = "AUBNE";
			voyageDestination.JB_E_ARV = DateTime.Today;
			voyageDestination.JB_A_ARV = DateTime.Today;

			Factory.Save();

			var log = voyageDestination.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUBNE|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUBNE", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			var previousLogPk = log.PK;
			voyageDestination.JB_RL_NKPortOfDischarge = "AUSYD";

			Factory.Save();

			log = voyageDestination.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUSYD|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertNotEquals(previousLogPk, log.PK);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUSYD", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			previousLogPk = log.PK;
			voyageDestination.JB_E_ARV = 1.DaysAgo();
			voyageDestination.JB_A_ARV = 1.DaysAgo();

			Factory.Save();

			log = voyageDestination.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUSYD|TYP=Schedule Feed");
			AssertNotNull("SBR event", log);
			AssertEquals(previousLogPk, log.PK);
			AssertEquals("Event date", new DateTime(2016, 3, 3), log.SL_EventTime);
			AssertEquals("Location", "AUSYD", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Type", Constants.EventReferenceParameterTypes.ScheduleFeed, log.Parameters[EventConstants.EventReferenceParameters.Codes.Type]);

			FreightDataRegistry.Instance.EnableScheduleFeedService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			voyageDestination.JB_RL_NKPortOfDischarge = "AUMEL";

			Factory.Save();

			log = voyageDestination.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, "|LOC=AUMEL|TYP=Schedule Feed");
			AssertNull(log);
		}

		[TestDate(2023, 04, 13)]
		public void TestOnSaving_ARV_EventCreated()
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

			var voyageDestination = voyage.Destinations.AddNew();
			voyageDestination.JB_RL_NKPortOfDischarge = "AUBNE";
			voyageDestination.JB_E_ARV = ZDateTime.Today;
			voyageDestination.JB_A_ARV = ZDateTime.Today;

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival, "|FAC=CTO|LOC=AUBNE|MOD=SEA");
			AssertNotNull("ARV event", log);

			var previousLogPk = log.PK;
			voyageDestination.JB_E_ARV = ZDateTime.Today.AddDays(1);
			voyageDestination.JB_A_ARV = ZDateTime.Today.AddDays(1);

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival, "|FAC=CTO|LOC=AUBNE|MOD=SEA");
			AssertNotNull("ARV event", log);
			AssertNotEquals(previousLogPk, log.PK);
		}

		public void TestOnSaving_UpdateFlightSubscriptionEvent_DestinationHasChanges()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = 3.DaysAgo();

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "SGSIN";
				destination.JB_E_ARV = 1.DaysAgo();

				voyage.GenerateSailings();
				Factory.Save();

				var expectedEventReference = $"|FDT={origin.JA_E_DEP:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001";

				AssertEquals("Precondition", 1, voyage.Sailings.Count);
				var sailing = voyage.Sailings[0];
				AssertFlightSubscriptionEvent("Precondition", sailing, 1, expectedEventReference);

				destination.JB_RL_NKPortOfDischarge = "NZAKL";
				Factory.Save();
				AssertFlightSubscriptionEvent("Discharge Port changed, should create a new SBR event", sailing, 2, expectedEventReference);

				destination.JB_E_ARV = ZDateTime.Now;
				Factory.Save();
				AssertFlightSubscriptionEvent("ETA changed, should create a new SBR event", sailing, 3, expectedEventReference);

				destination.JB_A_ARV = 1.DaysAgo();
				Factory.Save();
				AssertFlightSubscriptionEvent("ATA changed, should create a new SBR event", sailing, 4, expectedEventReference);

				destination.JB_E_ARV = ZDateTime.Empty;
				Factory.Save();
				AssertFlightSubscriptionEvent("ETA is empty, should create a new SBR event", sailing, 5, expectedEventReference);
			}
		}

		public void TestOnSaving_SBREventLog_DestinationWithNoIATACode()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF001";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = 3.DaysAgo();

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

				destination.PortOfDischarge.RL_IATA = "";
				destination.JB_E_ARV = ZDateTime.Now;
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

		public void TestArrivalEventHasBeenAddedToVoyage_UpdateJB_A_ARV()
		{
			Action<JobVoyage, VoyageDestination, ZString, ZDateTime, ZDateTime> assert = (voyage, destination, eventLocation, eventDate, expectedPropertyValue) =>
			{
				destination.JB_A_ARV = ZDateTime.Empty;
				voyage.Logs.RemoveAndDeleteAll();

				voyage.Logs.CreateOrRecreateEventLog(Events.Arrival, EstimateActual.Actual, eventDate.ToOffset(), "MCLAREN", new KeyValuePair<string, string>("LOC", eventLocation));
				AssertEquals("JB_A_ARV", expectedPropertyValue, destination.JB_A_ARV);
			};

			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			var destination1 = voyage1.Destinations.AddNew().In("UAIEV");
			var destination2 = voyage1.Destinations.AddNew().In("AUSYD");

			assert(voyage1, destination1, "UAIEV", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 1));
			assert(voyage1, destination2, "AUSYD", new ZDateTime(2013, 2, 2), new ZDateTime(2013, 2, 2));
		}

		public void TestCAVEventHasBeenAddedToVoyage_UpdateJB_AvailabilityDate()
		{
			Action<JobVoyage, VoyageDestination, ZString, ZDateTime, ZDateTime> assert = (voyage, destination, eventLocation, eventDate, expectedPropertyValue) =>
			{
				destination.JB_AvailabilityDate = ZDateTime.Empty;
				voyage.Logs.RemoveAndDeleteAll();

				voyage.Logs.CreateOrRecreateEventLog(Events.CargoAvailable, EstimateActual.Actual, eventDate.ToOffset(), "MCLAREN", "LOC".As(eventLocation));
				AssertEquals("JB_AvailabilityDate", expectedPropertyValue, destination.JB_AvailabilityDate);
			};

			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			var destination1 = voyage1.Destinations.AddNew().In("UAIEV");
			var destination2 = voyage1.Destinations.AddNew().In("AUSYD");

			assert(voyage1, destination1, "UAIEV", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 1));
			assert(voyage1, destination2, "AUSYD", new ZDateTime(2013, 2, 2), new ZDateTime(2013, 2, 2));
		}

		#region Factory Save

		public void TestSavingTheFactory_ShouldGenerateDateRelatedEvents()
		{
			AssertEventHasBeenAdded(VoyageDestination.Schema.JB_AvailabilityDate, Events.CargoAvailable);
			AssertEventHasBeenAdded(VoyageDestination.Schema.JB_A_ARV, Events.Arrival);
			AssertEventHasBeenAdded(VoyageDestination.Schema.JB_StorageDate, Events.StorageCommenced);
		}

		void AssertEventHasBeenAdded(ZString triggerProperty, Event expectedEvent)
		{
			Func<string, Func<StmALog, bool>> withLocation = (location) =>
			{
				Func<StmALog, bool> predicate = log =>
					{
						return
								log.SL_SE_NKEvent == expectedEvent.Code
							&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Location)
							&& log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] == location;
					};

				return predicate;
			};

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = "001";
			voyage.JV_FlightDate = ZDateTime.Today;

			var destination1 = voyage.Destinations.AddNew("UAIEV");
			var destination2 = voyage.Destinations.AddNew("AUSYD");

			destination1[triggerProperty] = 5.DaysAgo();
			destination2[triggerProperty] = 10.DaysAgo();

			Factory.Save();

			var log1 = voyage.Logs.Find(withLocation("UAIEV")).FirstOrDefault();
			var log2 = voyage.Logs.Find(withLocation("AUSYD")).FirstOrDefault();

			AssertNotNull("Generated log with an expected location", log1);
			AssertNotNull("Generated log with an expected location", log2);
			AssertEquals("Event time", 5.DaysAgo(), log1.SL_EventTime);
			AssertEquals("Event time", 10.DaysAgo(), log2.SL_EventTime);
			AssertEquals("Event facility", EventConstants.Facilities.Code.Terminal, log1.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Event facility", EventConstants.Facilities.Code.Terminal, log2.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);

			destination1[triggerProperty] = ZDateTime.Empty;
			destination2[triggerProperty] = ZDateTime.Empty;
			Factory.Save();

			log1 = voyage.Logs.Find(withLocation("UAIEV")).FirstOrDefault();
			log2 = voyage.Logs.Find(withLocation("AUSYD")).FirstOrDefault();

			AssertEquals("Log is cancelled", true, log1.SL_IsCancelled);
			AssertEquals("Log is cancelled", true, log2.SL_IsCancelled);
		}

		#endregion

		public void TestLogSTWReportingLicense()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.FillWithValidTestData();
			var destination = voyage.Destinations.AddNew();
			destination.FillWithValidTestData();
			destination.StowPlanMessageStatus = MessageStatusListSTW.Codes.Sending;

			Factory.Save();

			destination.StowPlanMessageStatus = MessageStatusListSTW.Codes.AcceptanceWithWarnings;
			destination.HasChanges = true;
			var msg = Factory.New<StowPlanLicenceLoggingExtensionsTest.DummyStowPlanMessage>();
			msg.AcceptedContainersWhichPreviouslyNotAccepted = 1;
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg.EM_SystemCreateTimeUtc = ZDateTime.Now;
			destination.Messages.Add(msg);

			Factory.Save();

			var license = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.StowPlanReporting.Name))[0];
			AssertNotNull(license);
			Assert(license.IsInDatabase);
		}

		public void TestUTCTimes()
		{
			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = new ZDateTime(2011, 12, 5, 15, 0, 0);
			destination.JB_A_ARV = new ZDateTime(2011, 12, 6, 20, 0, 0);
			destination.JB_S_ARV = new ZDateTime(2011, 12, 6, 22, 0, 0);

			//HKHKG is 8 hours ahead of UTC
			AssertEquals("JB_E_ARV_UTC", new ZDateTime(2011, 12, 5, 7, 0, 0), destination.JB_E_ARV_UTC);
			AssertEquals("JB_A_ARV_UTC", new ZDateTime(2011, 12, 6, 12, 0, 0), destination.JB_A_ARV_UTC);
			AssertEquals("JB_S_ARV_UTC", new ZDateTime(2011, 12, 6, 14, 0, 0), destination.JB_S_ARV_UTC);
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

			voyage.Destinations.RemoveAndDelete(destination);

			AssertEquals("Should have no destinations", 0, voyage.Destinations.Count);

			voyage.RunPreSaveValidation();

			AssertEquals("Should have a destination added", 1, voyage.Destinations.Count);
		}

		public void TestJB_ArrivalReferenceFieldType()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			var destination = voyage.Destinations.AddNew();

			destination.JB_RL_NKPortOfDischarge = "ZACAS";
			AssertEquals(nameof(FieldType.Text), destination.JB_ArrivalReferenceFieldType);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				destination.JB_RL_NKPortOfDischarge = "AUBNE";
				AssertEquals(nameof(FieldType.Text), destination.JB_ArrivalReferenceFieldType);

				destination.JB_RL_NKPortOfDischarge = "ZACAS";
				AssertEquals(nameof(FieldType.TextCodeFindBox), destination.JB_ArrivalReferenceFieldType);

				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				AssertEquals(nameof(FieldType.Text), destination.JB_ArrivalReferenceFieldType);
			}
		}

		public void TestETAUpdateNotification()
		{
			var subscriber = new Mock<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var queryProvider1 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var queryProvider2 = new Mock<IScheduleUpdateQueryProvider>(MockBehavior.Strict);
			var providerDelegate = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>(MockBehavior.Strict);

			var list = new ArrayList { subscriber.Object };

			JobVoyage voyage;
			{
				BusinessObjectFactory createFactory = new BusinessObjectFactory();

				JobVoyage createVoyage = createFactory.New<JobVoyage>();

				VoyageOrigin origin = createVoyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "NLAMS";

				VoyageDestination destination1 = createVoyage.Destinations.AddNew();
				destination1.JB_RL_NKPortOfDischarge = "AUSYD";
				destination1.JB_E_ARV = new ZDateTime(2009, 07, 13);

				VoyageDestination destination2 = createVoyage.Destinations.AddNew();
				destination2.JB_RL_NKPortOfDischarge = "AUBNE";
				destination2.JB_E_ARV = new ZDateTime(2009, 07, 08);

				createFactory.Save();

				voyage = Factory.Load<JobVoyage>(createVoyage.PK);
			}

			voyage.GenerateSailings();

			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate.Object);

			VoyageDestination destinationSYD = voyage.Destinations.GetDestinationFromDischarge("AUSYD");

			using (ObjectFactory.Substitute(ScheduleUpdateSubscriberFactoryTest.ObjectName, list))
			{
				subscriber
					.Setup(m => m.ETAChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						destinationSYD,
						new ZDateTime(2009, 07, 13)))
					.Callback(
						(ScheduleUpdateSubscriberFactoryTest.DoETAChanged)delegate (
							IScheduleUpdateServices services,
							VoyageDestination destination,
							ZDateTime oldETA)
						{
							AssertEquals(true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
							AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider1.Object, services.QueryProvider));
						}
					);

				providerDelegate.Setup(m => m()).Returns(queryProvider1.Object);
				destinationSYD.JB_E_ARV = new ZDateTime(2009, 07, 14);
				subscriber
					.Setup(m => m.ETAChanged(
						It.IsNotNull<IScheduleUpdateServices>(),
						destinationSYD,
						new ZDateTime(2009, 07, 14)))
					.Callback(
						(ScheduleUpdateSubscriberFactoryTest.DoETAChanged)delegate (
							IScheduleUpdateServices services,
							VoyageDestination destination,
							ZDateTime oldETA)
						{
							AssertEquals(true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
							AssertEquals("cache the query provider", true, object.ReferenceEquals(queryProvider2.Object, services.QueryProvider));
						}
					);

				// cache the query providers within calls to ETAChanged but not between calls.
				providerDelegate.Setup(m => m()).Returns(queryProvider2.Object);
				destinationSYD.JB_E_ARV = new ZDateTime(2009, 07, 15);
			}
		}

		public void TestATA_Sea_DoNotSyncroniseDestinationsWithTheSameVesselVoyage()
		{
			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", ZGuid.Empty);

			ZDateTime oldATA = ZDateTime.Now;

			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_A_ARV = oldATA.AddDays(11);

			var voyage2 = helper.CreateSeaVoyage("Visund", "123", ZGuid.Empty);

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_A_ARV = oldATA.AddDays(21);

			AssertEquals("ATA was not syncronised", oldATA.AddDays(11), destination1.JB_A_ARV);
		}

		public void TestATA_NonSea_DoNotSyncroniseDestinationsWithTheSameVesselVoyage()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Visund";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage1.JV_VoyageFlight = "123";
			voyage1.JV_RV_NKVessel = vessel.RV_FK;

			ZDateTime aTA = ZDateTime.Now;

			var destination11 = voyage1.Destinations.AddNew();
			destination11.JB_RL_NKPortOfDischarge = "AUSYD";
			destination11.JB_A_ARV = aTA;

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage2.JV_VoyageFlight = "123";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;

			var destination21 = voyage2.Destinations.AddNew();
			destination21.JB_RL_NKPortOfDischarge = "AUSYD";
			destination21.JB_A_ARV = aTA.AddDays(10);

			AssertEquals("ATA not syncronised for non-sea voyages", aTA, destination11.JB_A_ARV);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#region Test JB_AvailabilityDate

		public void TestValidateJB_AvailabilityDate()
		{
			Destination.JB_E_ARV = ZDateTime.Empty;
			Destination.JB_AvailabilityDate = ZDateTime.Empty;
			AssertNoWarnings("Availability Date is empty, no warning expected", Destination.JB_AvailabilityDateInfo);

			Destination.JB_E_ARV = ZDateTime.Today.AddDays(1);
			Destination.JB_AvailabilityDate = ZDateTime.Today;
			AssertHasErrors("Availability Date is before destination arrival, error expected", Destination.JB_AvailabilityDateInfo);

			Destination.JB_E_ARV = ZDateTime.Today;
			Destination.JB_AvailabilityDate = ZDateTime.Today;
			AssertNoErrors("Availability Date is same as destination date, no error expected", Destination.JB_AvailabilityDateInfo);

			Destination.JB_E_ARV = ZDateTime.Today;
			Destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(1);
			AssertNoErrors("Availability Date is after destination arrival date, no error expected", Destination.JB_AvailabilityDateInfo);
		}

		#region Detention Dates Updater

		public void TestDetentionDatesUpdaterIsCalledWithMocks()
		{
			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "234";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";

			var auDestination = voyage.Destinations.AddNew();
			auDestination.JB_RL_NKPortOfDischarge = "AUMEL";

			var nzDestination = voyage.Destinations.AddNew();
			nzDestination.JB_RL_NKPortOfDischarge = "NZCHC";

			voyage.GenerateSailings();

			Factory.Save();

			AssertEquals("Pre-condition: expected two sailings to have been generated as there are two destinations", 2, voyage.Sailings.Count);

			var updater = new Mock<IDetentionDatesUpdater>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(updater.Object))
			{
				updater.Setup(m => m.UpdateContainerDetentionDateFromSailing(It.IsAny<BusinessObject>()));
				var newFactory = new BusinessObjectFactory();
				var reloadedDestination = newFactory.Load<VoyageDestination>(nzDestination.PK);

				reloadedDestination.JB_AvailabilityDate = ZDateTime.Today;
				newFactory.Save();
				updater.Verify(m => m.UpdateContainerDetentionDateFromSailing(It.IsAny<BusinessObject>()), Times.Once());
			}
		}

		public void TestJB_AvailabilityDate_SetsDetentionDatesUpdater()
		{
			var agencyRegistry = ObjectFactory.Get<IAgencyRegistry>();
			agencyRegistry.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "Consignee";
			consignee.ConsigneeContainerPenalties.AddNew().PD_FreeDays = 10;

			Voyage.GenerateSailings();
			var sailing = Voyage.Sailings[0];

			Factory.Save();

			var billOfLading = (CommonShipment)Factory.New<IBillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V00000100";
			billOfLading.JS_ShipmentStatus = "CNF";
			billOfLading.JS_JX = sailing.PK;
			billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			billOfLading.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;

			var billOfLadingContainer = (CommonContainer)Factory.New<IBillOfLadingContainer>();
			billOfLadingContainer.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			billOfLadingContainer.JC_JS_FCLBookingOnlyLink = billOfLading.PK;
			billOfLadingContainer.JC_Purpose = "REL";

			var transport = billOfLading.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();

			AssertEquals("Pre-condition:", ZDateTime.Empty, transport.JW_TerminalAvailabilityDate);
			AssertEquals("Pre-condition:", ZDateTime.Empty, billOfLadingContainer.JC_EmptyReturnedBy);

			var date = new ZDateTime(2013, 11, 19);
			transport.JW_TerminalAvailabilityDate = date;

			Factory.Save();

			AssertEquals("Pre-condition:", date, sailing.Destination.JB_AvailabilityDate);
			AssertEquals("Detention Date Updater has been run and recalculated the empty return by date from client", date.AddDays(9), billOfLadingContainer.JC_EmptyReturnedBy);
		}

		#endregion

		#region JB_AvailabilityDate On Saving Logs

		[TestDate(2014, 01, 01)]
		public void TestJB_AvailabilityDateLogsChanged()
		{
			Destination.JB_RL_NKPortOfDischarge = "USBOS";
			AssertEquals("Pre-condition", false, Destination.JB_AvailabilityDateInfo.HasChanges);

			Destination.JB_AvailabilityDate = new ZDateTime(2014, 01, 19);

			AssertLatestLogForAvailablityDate("Availability Date for Discharge Port USBOS changed to 19-Jan-14.");

			Destination.JB_AvailabilityDate = ZDateTime.Empty;

			AssertLatestLogForAvailablityDate("Availability Date for Discharge Port USBOS deleted.");

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Destination.JB_AvailabilityDate = new ZDateTime(2014, 02, 10);

			AssertLatestLogForAvailablityDate("Availability Date for Discharge Port AUSYD changed to 10-Feb-14.");
		}

		void AssertLatestLogForAvailablityDate(ZString expectedReference)
		{
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, expectedReference);

			var latestLog = Voyage.Logs.Find(query);
			AssertNotNull("Expected to find an edit log with the expected reference", latestLog);
		}

		#endregion

		#endregion

		public void TestValidateJB_StorageDate()
		{
			Destination.JB_AvailabilityDate = ZDateTime.Empty;
			Destination.JB_StorageDate = ZDateTime.Empty;
			Assert("Storage date is empty, no warning expected", !Destination.JB_StorageDateInfo.HasWarnings());

			Destination.JB_AvailabilityDate = ZDateTime.Today;
			Destination.JB_StorageDate = ZDateTime.Today.AddDays(-1);
			Assert("Storage date is before availability date, error expected", Destination.JB_StorageDateInfo.HasErrors());

			Destination.JB_AvailabilityDate = ZDateTime.Now;
			Destination.JB_StorageDate = ZDateTime.Now;
			Assert("Storage date is same as availability date,no error expected", !Destination.JB_StorageDateInfo.HasErrors());

			Destination.JB_AvailabilityDate = ZDateTime.Now;
			Destination.JB_StorageDate = ZDateTime.Now.AddDays(1);
			Assert("Storage date is after availability date, no error expected", !Destination.JB_StorageDateInfo.HasErrors());
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Destination.JB_A_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Destination.JB_E_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Destination.JB_S_ARV.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Destination.JB_AvailabilityDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Destination.JB_StorageDate.Kind);
		}

		public void TestJB_S_ARV()
		{
			var date1 = new DateTime(2017, 06, 01, 12, 30, 00);
			var date2 = new DateTime(2017, 06, 02, 12, 30, 00);

			var voyage = Factory.New<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NLAMS";

			AssertEquals("Precondition", ZDateTime.Empty, destination.JB_E_ARV);
			AssertEquals("Precondition", ZDateTime.Empty, destination.JB_S_ARV);

			destination.JB_E_ARV = date1;
			AssertEquals("Schedule Arrival: Should have defaulted from JB_E_ARV", date1, destination.JB_S_ARV);

			destination.JB_E_ARV = ZDateTime.Empty;
			AssertEquals("Schedule Arrival: Should not change", date1, destination.JB_S_ARV);

			destination.JB_S_ARV = date2;
			AssertEquals("Estimated Arrival: Should have defaulted from JB_S_ARV", date2, destination.JB_E_ARV);

			destination.JB_E_ARV = date1;
			AssertEquals("Estimated Arrival", date1, destination.JB_E_ARV);
			AssertEquals("Schedule Arrival", date2, destination.JB_S_ARV);
		}

		public void TestDeleteAlsoDeletesAttachedMessages()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination destination = voyage.Destinations.AddNew();
			EDIMessage message = destination.Messages.AddNew();

			destination.Delete();
			AssertEquals("the message should be deleted", true, message.IsDeleted);
		}

		public void TestClone()
		{
			Destination.JB_E_ARV = ZDateTime.Now;

			VoyageDestination clonedDestination = (VoyageDestination)Destination.Clone();

			AssertEquals("Should clone properties", Destination.JB_E_ARV, clonedDestination.JB_E_ARV);
			AssertNotEquals("Should exclude JB_JV", Destination.JB_JV, clonedDestination.JB_JV);
		}

		public void TestScheduleDateChangeLogged()
		{
			Origin.JA_RL_NKPortOfLoading = "MYPKG";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Factory.Save();

			Destination.JB_E_ARV = new ZDateTime(2000, 1, 1);
			Factory.Save();
			JobScheduleChangeLoggerTest.AssertLastDateChangeLogged(Factory, ScheduleDateTypes.Codes.ETA, ZDateTime.Empty, Destination.JB_E_ARV);
		}

		public void TestUpdateProxiedFieldsOnLinkedConsolTransportWhenChangesNotYetInDB()
		{
			var today = ZDateTime.Today;
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
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
			var departureAddress = orgHeader.Addresses.AddNew();
			departureAddress.Address1 = "destination";

			var destination = (VoyageDestination)voyage.Destinations.Single();
			destination.JB_OA_ArrivalCTOAddress = departureAddress.PK;
			destination.JB_RL_NKPortOfDischarge = "DISC";
			destination.JB_S_ARV = today;
			destination.JB_E_ARV = today.AddDays(1);
			destination.JB_A_ARV = today.AddDays(2);

			Assert("Precondition: transport 1 is not in db", !transport1_1.IsInDatabase);
			Assert("Precondition: transport 2 is not in db", !transport2_1.IsInDatabase);
			Assert("Precondition: voyage is not in db", !voyage.IsInDatabase);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transport1_2 = newFactory.Load<Transport>(transport1_1.PK);
			var transport2_2 = newFactory.Load<Transport>(transport2_1.PK);
			var transports = new Transport[] { transport1_2, transport2_2 };

			var proxiedTransportFieldsToDestFieldsDict = new Dictionary<ZString, ZPropertyInfo>()
			{
				{ JobConsolTransportSchema.Constants.JW_OA_ArrivalLocation, destination.JB_OA_ArrivalCTOAddressInfo },
				{ JobConsolTransportSchema.Constants.JW_RL_NKDiscPort, destination.JB_RL_NKPortOfDischargeInfo },
				{ JobConsolTransportSchema.Constants.JW_STA, destination.JB_S_ARVInfo },
				{ JobConsolTransportSchema.Constants.JW_ETA, destination.JB_E_ARVInfo },
				{ JobConsolTransportSchema.Constants.JW_ATA, destination.JB_A_ARVInfo },
			};

			transports.ForEach(transport => proxiedTransportFieldsToDestFieldsDict.ForEach(pair =>
				AssertEquals(pair.Value.Name + " should propogate to linked transport when data not in db.", pair.Value.Value, ((ZPropertyInfo)transport[pair.Key + "Info"]).OriginalValue)));
		}

		public void TestEditLogRaisedOnJobVoyageOnSave()
		{
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.Factory.Save();
			AssertEquals("No edit log initially", null, Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			VoyageDestination loadedDestination = newFactory.Load<VoyageDestination>(Destination.PK);
			loadedDestination.JB_E_ARV = ZDateTime.Now;
			newFactory.Save();
			AssertNotNull("Edit log raised on edit of VoyageDestination", loadedDestination.Voyage.Logs.MostRecentLogByEventTime(Events.EditedARecord));
		}

		public void TestDefaultingCTO()
		{
			OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			OrgCarrierAppointedAgentPorts map = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			map.O5_PortOrCountry = "AUBNE";
			map.O5_OA_AgentOfficeAddress = cto.MainAddress.PK;
			map.O5_SeaAirCarrierOrForwarderType = Core.Constants.TransportModes.Air;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_OH_Line = carrier.PK;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			AssertEquals(cto.MainAddress, destination.ArrivalCTOAddress);
		}

		public void TestVoyage()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			JobVoyage testVoyage2 = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;

			testDestination.JB_JV = testVoyage.PK;
			AssertEquals("TestDestination.JB_JV and TestVoyage are equal, no error expected", testDestination.JB_JV, testDestination.Voyage.PK);
			Assert("TestDestination.Voyage.PK and TestVoyage2 are not equal, error expected", testDestination.Voyage.PK != testVoyage2.PK);
			Assert("TestDestination.JB_JV and TestVoyage2 are not equal, error expected", testDestination.JB_JV != testVoyage2.PK);
		}

		public void TestMessages()
		{
			AssertNotNull(Destination.Messages);
		}

		public void TestJB_Calc_ArrivalCTOPremiseID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = header.PK;
			address.OA_Code = "CUCKOOSQUEAKER";
			address.LocalControlledPremisesID = "9914N";
			Destination.JB_OA_ArrivalCTOAddress = address.PK;
			AssertEquals("9914N", Destination.JB_Calc_ArrivalCTOPremiseID);
		}

		public void TestJB_RL_NKPortOfDischarge_CallsScheduleDataVendor()
		{
			MockSailingScheduleDataVendor.RegisterThisSubTypeOverride();
			try
			{
				Destination.JB_RL_NKPortOfDischarge = "USLAX";
				AssertEquals("Expected SailingScheduleDataVendor.UpdateVoyageDestination to be called", true, MockSailingScheduleDataVendor.Instance.UpdateVoyageDestinationCalled);
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

			IScheduleChangeParent parent = voyage.Destinations[0];

			CombineAssertions(delegate
			{
				AssertEquals("DestinationPort", "NLAMS", parent.DestinationPort);
				AssertEquals("Factory", Factory, parent.Factory);
				AssertEquals("OriginPort", "", parent.OriginPort);
				AssertEquals("PK", voyage.Destinations[0].PK, parent.PK);
				AssertEquals("SailingRefColumn", JobSailingSchema.JX_JB, parent.SailingRefColumn);
				AssertEquals("TablePrefix", JobVoyDestinationSchema.Constants.Prefix, parent.TablePrefix);
				AssertEquals("Voyage", voyage, parent.Voyage);
			});
		}

		public void TestArrivalEvent()
		{
			var voyage = Factory.New<JobVoyage>();

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNull("ARV log not yet created", log);

			destination.JB_A_ARV = ZDateTime.Today.AddDays(10);
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("ARV log created", log);
			AssertEquals("event date matches destination arrival date", destination.JB_A_ARV, log.SL_EventTime);
			AssertEquals("location in log", "SGSIN", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			destination.JB_A_ARV = ZDateTime.Empty;
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNull("ARV log cancelled", log);

			destination.JB_A_ARV = ZDateTime.Today.AddDays(11);
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("ARV log created", log);
			AssertEquals("event date matches destination arrival date", destination.JB_A_ARV, log.SL_EventTime);
			AssertEquals("location in log", "SGSIN", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("ARV log created", log);
			AssertEquals("event date matches destination arrival date", destination.JB_A_ARV, log.SL_EventTime);
			AssertEquals("location in log", "NZAKL", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			voyage = Factory.New<JobVoyage>();

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNull("ARV log not yet created", log);

			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_A_ARV = ZDateTime.Today.AddDays(10);

			Factory.Save();

			log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("ARV log created", log);
			AssertEquals("event date matches destination arrival date", destination.JB_A_ARV, log.SL_EventTime);
			AssertEquals("location in log", "SGSIN", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		public void TestArrivalEvent_FlightDateParameter()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = new ZDateTime(2017, 06, 27);

			var destination = voyage.Destinations.AddNew();
			destination.JB_A_ARV = new ZDateTime(2017, 06, 28);

			Factory.Save();

			var log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertEquals("2017-06-27", log.Parameters[Params.FlightDate]);
		}

		public void TestARVAndCAVEventsAreCancelledAndRecreatedWhenDischargePortChanges()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_AvailabilityDate = ZDate.Today.AddDays(5);
			destination.JB_A_ARV = ZDate.Today.AddDays(-1);

			Factory.Save();

			var cargoAvailableLog = voyage.Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			AssertNotNull("CAV event", cargoAvailableLog);
			AssertEquals("Event date", ZDate.Today.AddDays(5), cargoAvailableLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", cargoAvailableLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, cargoAvailableLog.Parameters[Params.Facility]);

			var arvLog = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("ARV event", arvLog);
			AssertEquals("Event date", ZDate.Today.AddDays(-1), arvLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", arvLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, arvLog.Parameters[Params.Facility]);

			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Factory.Save();

			Assert("Old Cargo Available event is cancelled", cargoAvailableLog.IsCancelled);
			Assert("Old Arrival event is cancelled", arvLog.IsCancelled);

			var newCargoAvailableLog = voyage.Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			AssertEquals("Location", "AUSYD", newCargoAvailableLog.Parameters[Params.Location]);

			var newArvLog = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertEquals("Location", "AUSYD", newCargoAvailableLog.Parameters[Params.Location]);
		}

		public void TestCAVEventsCreatedGroupage()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var origin = Factory.NewWithValidTestData<VoyageOrigin>();
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var transport = consol.Transports.AddNew();

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			transport.JW_JX = sailing.PK;
			sailing.JX_JA = origin.PK;
			origin.JA_JV = voyage.PK;
			_ = consol.Voyage;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_AvailabilityDate = ZDate.Today.AddDays(5);
			destination.JB_A_ARV = ZDate.Today.AddDays(-1);

			Factory.Save();

			var logs = voyage.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "CAV");

			var ctoLog = logs
.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Terminal);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), ctoLog);
			AssertEquals("Event date", ZDate.Today.AddDays(5), ctoLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", ctoLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, ctoLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);

			var cfsLog = logs
.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Depot);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), cfsLog);
			AssertEquals("Event date", ZDate.Today.AddDays(5), cfsLog.SL_EventTime);
			AssertEquals("Location", "AUBNE", cfsLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Depot, cfsLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		public void TestParentWorkflowProviders()
		{
			var voyage = Factory.New<JobVoyage>();
			var destination = voyage.Destinations.AddNew();

			IWorkflowTriggerFieldChangeSource workflowTriggerChangeSource = destination;

			AssertContainsExactElementsInAnyOrder(workflowTriggerChangeSource.ParentWorkflowProviders, new[] { voyage });
		}

		public void TestDeletedVoyageDestination()
		{
			var destination = Factory.New<VoyageDestinationSendersMessageReferenceTest.TestHelperVoyageDestination>();
			destination.Delete();

			AssertNoExceptionThrown("No exception should be thrown here", destination.InvokeOnFactorySavingBeforeTransactionCore);
		}

		public override void TestFetchForLoad()
		{
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();
			factoryForLoad.ResetDatabaseLoadCount();
			factoryForLoad.LoadTop1<VoyageDestination>(new ZQuery(JobVoyDestinationSchema.PK, Destination.PK));

			AssertMaxDbHits("With a clean factory only Destination is loaded", 1, factoryForLoad);
		}

		public void TestHumanReadableName()
		{
			VoyageDestination destination = Factory.New<JobVoyage>().Destinations.AddNew();

			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			AssertEquals("Sailing Schedule (Vessel='', Voyage='', Carrier=''), Destination = 'AUBNE'", destination.HumanReadableName);

			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			AssertEquals("Sailing Schedule (Vessel='', Voyage='', Carrier=''), Destination = 'NZAKL'", destination.HumanReadableName);
		}

		#region EstimatedArrival Event

		public void TestSave_EstimatedArrivalDateUpdated_GenerateEstimatedArrivalEvent()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "UAIEV";
			destination.JB_E_ARV = 5.DaysAgo();

			Factory.Save();
			var log = voyage.Logs.MostRecentLogByEventTime(Events.Arrival);
			Assert("Estimated ARV event", log.SL_IsEstimate);
			AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		#endregion

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestSailingOnlineMatchStatusRefreshedOn_E_ARV_Update()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF69";

				var origin1 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;
				var origin2 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;

				var destination = Factory.New<VoyageDestination>();
				destination.JB_E_ARV = ZDate.Today;

				var refUNLOCO1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var refUNLOCO2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
				var refUNLOCO3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");

				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.Add(origin1);
				voyage.Origins.Add(origin2);
				voyage.Destinations.Add(destination);

				origin1.JA_RL_NKPortOfLoading = refUNLOCO1.RL_Code;
				origin2.JA_RL_NKPortOfLoading = refUNLOCO2.RL_Code;
				destination.JB_RL_NKPortOfDischarge = refUNLOCO3.RL_Code;

				voyage.GenerateSailings();
				AssertEquals("Pre: Expected 2 sailing schedules generated.", 2, voyage.Sailings.Count);

				var destinationSailings = destination.FetchSailings();
				AssertEquals("Pre: Expected 2 sailing schedules for this destination.", 2, destinationSailings.Length);

				destinationSailings.ForEach(sailing => sailing.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched);
				destination.JB_E_ARV = ZDate.Today.AddDays(2);
				CombineAssertions("Expected all sailings to have their online schedule status updated when estimated arrival date changed.", () =>
				{
					destinationSailings.ForEach(sailing => AssertEquals(Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus));
				});
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestSailingOnlineMatchStatusRefreshedOn_PortOfDischarge_Update()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF69";

				var origin1 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;
				var origin2 = Factory.New<VoyageOrigin>();
				origin1.JA_E_DEP = ZDate.Today;

				var destination = Factory.New<VoyageDestination>();
				destination.JB_E_ARV = ZDate.Today;

				var refUNLOCO1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var refUNLOCO2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
				var refUNLOCO3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
				var refUNLOCO4 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.Add(origin1);
				voyage.Origins.Add(origin2);
				voyage.Destinations.Add(destination);

				origin1.JA_RL_NKPortOfLoading = refUNLOCO1.RL_Code;
				origin2.JA_RL_NKPortOfLoading = refUNLOCO2.RL_Code;
				destination.JB_RL_NKPortOfDischarge = refUNLOCO3.RL_Code;

				voyage.GenerateSailings();
				AssertEquals("Pre: Expected 2 sailing schedules generated.", 2, voyage.Sailings.Count);

				var destinationSailings = destination.FetchSailings();
				AssertEquals("Pre: Expected 2 sailing schedules for this destination.", 2, destinationSailings.Length);

				destinationSailings.ForEach(sailing => sailing.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched);
				destination.JB_RL_NKPortOfDischarge = refUNLOCO4.RL_Code;
				CombineAssertions("Expected all sailings to have their online schedule status updated when port of discharge changed.", () =>
				{
					destinationSailings.ForEach(sailing => AssertEquals(Constants.FlightScheduleStatus.Matched, sailing.JX_OnlineScheduleStatus));
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

			AssertNotNull(sailing.Destination.AdditionalValidation);
			AssertType(typeof(TransportVoyDestinationAdditionalValidation), sailing.Destination.AdditionalValidation);
		}

		public void TestUpdateRealtedBookedAgencyBookingEvent()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var agencyBooking1 = (CommonShipment)Factory.New<IAgencyBooking>();
			agencyBooking1.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			agencyBooking1.JS_JX = voyage.Sailings[0].PK;
			agencyBooking1.Numbers.AddNew().CE_EntryType = CusEntryNumLookups.HIR;

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

				destination.JB_RL_NKPortOfDischarge = "CNSHG";
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
				destination.JB_E_ARV = new ZDateTime(2023, 09, 01);
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
				destination.JB_A_ARV = new ZDateTime(2023, 09, 02);
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

		#region PortMatching

		public void TestRelatedPortProperty_AttributeIsSet()
		{
			void AssertAttribute(string name, string relatedPortProperty)
			{
				var attribute = Destination.GetType()
				.GetProperty(name)
				.GetCustomAttributes(typeof(DateTimeOffsetRelatedPortAttribute), false)
				.SingleOrDefault() as DateTimeOffsetRelatedPortAttribute;

				AssertNotNull(attribute);
				AssertEquals(relatedPortProperty, attribute.RelatedPortProperty);
			}
			AssertAttribute("JB_FirstDischargePortETA", "JB_RL_NKFirstDischargePort");
			AssertAttribute("JB_LastForeignPortETD", "JB_RL_NKLastForeignPort");
		}

		public void TestRelatedPortProperty_OffsetUpdatedAccordingToPortUnloco()
		{
			var localDateTime = new DateTime(2023, 7, 1, 14, 0, 0);

			Destination.JB_RL_NKFirstDischargePort = string.Empty;
			Destination.JB_FirstDischargePortETA = new DateTimeOffset(localDateTime, TimeSpan.FromHours(1));
			AssertEquals(Destination.JB_FirstDischargePortETA.Offset, TimeSpan.FromHours(0));
			Destination.JB_RL_NKFirstDischargePort = "CNSHA";
			AssertEquals(Destination.JB_FirstDischargePortETA.Offset, TimeSpan.FromHours(8));

			Destination.JB_RL_NKLastForeignPort = string.Empty;
			Destination.JB_LastForeignPortETD = new DateTimeOffset(localDateTime, TimeSpan.FromHours(1));
			AssertEquals(Destination.JB_LastForeignPortETD.Offset, TimeSpan.FromHours(0));
			Destination.JB_RL_NKLastForeignPort = "CNSHA";
			AssertEquals(Destination.JB_LastForeignPortETD.Offset, TimeSpan.FromHours(8));
		}

		public void TestRelatedPortProperty_AmbiguousHour()
		{
			void AssertOffset(DateTime time)
			{
				Destination.JB_RL_NKFirstDischargePort = ZString.Empty;
				Destination.JB_FirstDischargePortETA = new DateTimeOffset(time, TimeSpan.FromHours(1));
				AssertEquals(TimeSpan.FromHours(0), Destination.JB_FirstDischargePortETA.Offset);

				Destination.JB_RL_NKFirstDischargePort = "AUSYD";
				var expectedOffset = Env.Time.GetUtcOffsetBasedOnLocal("AUSYD", time);
				AssertEquals($"{time} should have Offset: {expectedOffset.Hours} for AUSYD", expectedOffset, Destination.JB_FirstDischargePortETA.Offset);

				Destination.JB_RL_NKFirstDischargePort = "AUMEL";
				expectedOffset = Env.Time.GetUtcOffsetBasedOnLocal("AUMEL", time);
				AssertEquals($"{time} should have Offset: {expectedOffset.Hours} for AUMEL", expectedOffset, Destination.JB_FirstDischargePortETA.Offset);
			}
			CombineAssertions(() =>
			{
				AssertOffset(new DateTime(2023, 4, 1, 1, 0, 0));
				AssertOffset(new DateTime(2023, 4, 2, 1, 0, 0));
				AssertOffset(new DateTime(2023, 4, 2, 2, 0, 0));
				AssertOffset(new DateTime(2023, 4, 2, 2, 30, 0));
				AssertOffset(new DateTime(2023, 4, 2, 3, 0, 0));
				AssertOffset(new DateTime(2023, 4, 3, 1, 0, 0));
				AssertOffset(new DateTime(2023, 10, 1, 1, 0, 0));
				AssertOffset(new DateTime(2023, 10, 1, 2, 0, 0));
				AssertOffset(new DateTime(2023, 10, 1, 3, 0, 0));
			});
		}

		public void TestPortMatchingSupportIsUpdateRequired()
		{
			// Arrange
			IPortMatchingHandler portMatchingHandler = Destination;
			Assert("Pre-condition: mising 4 fields, update required", portMatchingHandler.IsUpdateRequired);

			// Act && Assert
			Destination.JB_RL_NKFirstDischargePort = "AUSYD";
			Assert("Update required, missing 3 fields", portMatchingHandler.IsUpdateRequired);
			Destination.JB_FirstDischargePortETA = new DateTime(2023, 7, 1, 14, 0, 0);
			Assert("Update required, missing 2 fields which is a pair (last foreign port and date)", portMatchingHandler.IsUpdateRequired);
			Destination.JB_RL_NKLastForeignPort = "CNSHA";
			Assert("No update required, missing 1 pairing field for last foreign port", !portMatchingHandler.IsUpdateRequired);
			Destination.JB_LastForeignPortETD = new DateTime(2023, 7, 3, 14, 0, 0);
			Assert("No update required, missing no field", !portMatchingHandler.IsUpdateRequired);
			Destination.JB_FirstDischargePortETA = ZDateTimeOffset.Empty;
			Assert("No update required, missing 1 pairing field for first discharge port", !portMatchingHandler.IsUpdateRequired);
			Destination.JB_RL_NKFirstDischargePort = ZString.Empty;
			Assert("Update required, missing 2 fields which is a pair (first discharge port and date)", portMatchingHandler.IsUpdateRequired);
		}

		void AssertSetFirstArrivalPort(VoyageDestination destination, string unlocoMsg, ZString destUnloco, ZString unloco, ZString expectedUnloco, string dateMsg, ZDateTimeOffset destDate, ZDateTimeOffset date, ZDateTimeOffset expectedDate)
		{
			IPortMatchingSupport portMatchingSupport = destination;
			destination.JB_RL_NKFirstDischargePort = destUnloco;
			destination.JB_FirstDischargePortETA = destDate;
			portMatchingSupport.SetFirstArrivalPort(unloco, date);
			CombineAssertions(() =>
			{
				AssertEquals(unlocoMsg, expectedUnloco, destination.JB_RL_NKFirstDischargePort);
				AssertEquals(dateMsg, expectedDate, destination.JB_FirstDischargePortETA);
			});
		}

		void AssertSetLastForeignPort(VoyageDestination destination, string unlocoMsg, ZString destUnloco, ZString unloco, ZString expectedUnloco, string dateMsg, ZDateTimeOffset destDate, ZDateTimeOffset date, ZDateTimeOffset expectedDate)
		{
			IPortMatchingSupport portMatchingSupport = destination;
			destination.JB_RL_NKLastForeignPort = destUnloco;
			destination.JB_LastForeignPortETD = destDate;
			portMatchingSupport.SetLastForeignPort(unloco, date);
			CombineAssertions(() =>
			{
				AssertEquals(unlocoMsg, expectedUnloco, destination.JB_RL_NKLastForeignPort);
				AssertEquals(dateMsg, expectedDate, destination.JB_LastForeignPortETD);
			});
		}

		[TestDate(2022, 2, 9)]
		public void TestPortMatchingSupport_SetFirstArrivalPort()
		{
			var destination = Destination;
			AssertSetFirstArrivalPort(destination
				, "First Arrival Port must be set if empty.", ZString.Empty, new ZString("AUSYD"), new ZString("AUSYD")
				, "First Arrival Date must be set if empty.", ZDateTimeOffset.Empty, ZDateTimeOffset.Today, ZDateTimeOffset.Today);

			AssertSetFirstArrivalPort(destination
				, "First Arrival Port must not be set if First Arrival Date is not empty.", ZString.Empty, new ZString("AUSYD"), ZString.Empty
				, "First Arrival Date must not be set if popuated.", ZDateTimeOffset.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddDays(1));

			AssertSetFirstArrivalPort(destination
				, "First Arrival Port must not be set if populated.", new ZString("AUMEL"), new ZString("AUSYD"), new ZString("AUMEL")
				, "First Arrival Date must not be set if First Arrival Port is not empty.", ZDateTimeOffset.Empty, ZDateTimeOffset.Today, ZDateTimeOffset.Empty);

			AssertSetFirstArrivalPort(destination
				, "First Arrival Port must not be set if populated.", new ZString("AUMEL"), new ZString("AUSYD"), new ZString("AUMEL")
				, "Date of Arrival to First Arrival Port must not be set if popuated.", ZDateTimeOffset.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddDays(1));
		}

		[TestDate(2022, 2, 9)]
		public void TestPortMatchingSupport_SetLastForeignPort()
		{
			var destination = Destination;
			AssertSetLastForeignPort(destination
				, "Last Foreign Port must be set if empty.", ZString.Empty, new ZString("TWTPE"), new ZString("TWTPE")
				, "Last Foreign Date must be set if empty.", ZDateTimeOffset.Empty, ZDateTimeOffset.Today, ZDateTimeOffset.Today);

			AssertSetLastForeignPort(destination
				, "Last Foreign Port must not be set if Last Foreign Date is not empty.", ZString.Empty, new ZString("TWTPE"), ZString.Empty
				, "Last Foreign Date must not be set if popuated.", ZDateTimeOffset.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddDays(1));

			AssertSetLastForeignPort(destination
				, "Last Foreign Port must not be set if populated.", new ZString("SGSIN"), new ZString("TWTPE"), new ZString("SGSIN")
				, "Last Foreign Date must not be set if Last Foreign Port is not empty.", ZDateTimeOffset.Empty, ZDateTimeOffset.Today, ZDateTimeOffset.Empty);

			AssertSetLastForeignPort(destination
				, "Last Foreign Port must not be set if populated.", new ZString("SGSIN"), new ZString("TWTPE"), new ZString("SGSIN")
				, "Last Foreign Date must not be set if popuated.", ZDateTimeOffset.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTimeOffset.Today.AddDays(1));
		}

		#endregion

		#region Implementation

		VoyageDestination Destination;
		VoyageOrigin Origin;
		VoyageOrigin Origin2;
		JobVoyage Voyage;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "SGSIN";

			Destination = Voyage.Destinations.AddNew();
		}

		protected override BusinessObject GetLogParentForEventDateProperty()
		{
			return Voyage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Destination;
		}

		#endregion
	}
}
