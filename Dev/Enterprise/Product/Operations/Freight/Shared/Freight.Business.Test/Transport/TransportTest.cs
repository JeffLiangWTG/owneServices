using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.MachineLearning.NLP;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportTest : BaseFreightTest
	{
		#region Events

		public void TestDatePropertiesUpdatedByEvents()
		{
			AssertEventUpdatesDateProperty(Events.Departure, EstimateActual.Estimate, JobConsolTransportSchema.JW_ETD, JobConsolTransportSchema.JW_RL_NKLoadPort);
			AssertEventUpdatesDateProperty(Events.Departure, EstimateActual.Actual, JobConsolTransportSchema.JW_ATD, JobConsolTransportSchema.JW_RL_NKLoadPort);
			AssertEventUpdatesDateProperty(Events.Arrival, EstimateActual.Estimate, JobConsolTransportSchema.JW_ETA, JobConsolTransportSchema.JW_RL_NKDiscPort);
			AssertEventUpdatesDateProperty(Events.Arrival, EstimateActual.Actual, JobConsolTransportSchema.JW_ATA, JobConsolTransportSchema.JW_RL_NKDiscPort);
		}

		void AssertEventUpdatesDateProperty(Event eventType, EstimateActual estimateActual, SchemaDateTimeColumn dateProperty, SchemaStringColumn portProperty)
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport[portProperty] = "AUSYD";

			Action<ZString, bool> assertPropertyUpdated = (eventLocation, shouldUpdate) =>
			{
				var eventTime = new ZDateTimeOffset(2016, 6, 1);
				transport[dateProperty] = ZDateTime.Empty;
				transport.Logs.RemoveAndDeleteAll();

				transport.Logs.CreateOrRecreateEventLog(eventType, estimateActual, eventTime, "BARRY WONG", new KeyValuePair<string, string>("LOC", eventLocation));
				AssertEquals(dateProperty.Name, shouldUpdate ? eventTime.ToZDateTime() : ZDateTime.Empty, transport[dateProperty]);
			};

			assertPropertyUpdated(ZString.Empty, true);
			assertPropertyUpdated("AUSYD", true);
			assertPropertyUpdated("NLAMS", false);
		}

		public void TestEventTimeZones()
		{
			void AssertTimeZone(Event eventType, SchemaDateTimeColumn dateProperty, SchemaStringColumn portProperty)
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();
				var transport = consol.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "NLAMS";
				var now = ZDateTime.Now;
				transport[dateProperty] = now.ToZDateTime();
				Factory.Save();
				var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, (ZString)transport[portProperty]);
				var nowOffset = now.ToDateTimeOffset(unloco);
				var logs = transport.Logs.GetAllLogs().Where(t => t.SL_SE_NKEvent == eventType.Code).ToList();
				AssertGreaterThan(logs.Count, 0);
				AssertEquals("Expecting all the event codes to have the correct offset", true, logs.All(t => t.SL_EventTimeOffset == nowOffset));
			}

			AssertTimeZone(Events.Departure, JobConsolTransportSchema.JW_ETD, JobConsolTransportSchema.JW_RL_NKLoadPort);
			AssertTimeZone(Events.Departure, JobConsolTransportSchema.JW_ATD, JobConsolTransportSchema.JW_RL_NKLoadPort);
			AssertTimeZone(Events.Arrival, JobConsolTransportSchema.JW_ETA, JobConsolTransportSchema.JW_RL_NKDiscPort);
			AssertTimeZone(Events.Arrival, JobConsolTransportSchema.JW_ATA, JobConsolTransportSchema.JW_RL_NKDiscPort);
			AssertTimeZone(Events.CargoAvailable, JobConsolTransportSchema.JW_DepotAvailabilityDate, JobConsolTransportSchema.JW_RL_NKDiscPort);
			AssertTimeZone(Events.ReceiptCommenced, JobConsolTransportSchema.JW_TerminalReceivalCommences, JobConsolTransportSchema.JW_RL_NKLoadPort);
			AssertTimeZone(Events.StorageCommenced, JobConsolTransportSchema.JW_TerminalStorageDate, JobConsolTransportSchema.JW_RL_NKDiscPort);
			AssertTimeZone(Events.CutOffDate, JobConsolTransportSchema.JW_DepotCutOff, JobConsolTransportSchema.JW_RL_NKLoadPort);
		}

		#endregion

		#region Factory save

		public void TestFactorySave_JW_ATAHasChanges_GenerateArrivalEvent()
		{
			Action<Transport, ZString> assert = (transportToTest, expectedEventLocation) =>
			{
				string actualLocation;

				transportToTest.JW_ATA = ZDateTime.Now;
				Factory.Save();

				var eventLog = transportToTest.Logs.MostRecentLogByEventTime(Events.Arrival);
				if (!eventLog.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out actualLocation))
				{
					actualLocation = string.Empty;
				}

				AssertEquals("Is log estimate", false, eventLog.SL_IsEstimate);
				AssertEquals("Event location", expectedEventLocation, actualLocation);
			};

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = ZString.Empty;
			assert(transport, string.Empty);

			transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "UAIEV";
			assert(transport, "UAIEV");
		}

		public void TestFactorySave_JW_ATDHasChanges_GenerateDepartureEvent()
		{
			Action<Transport, ZString> assert = (transportToTest, expectedEventLocation) =>
			{
				string actualLocation;

				transportToTest.JW_ATD = ZDateTime.Now;
				Factory.Save();

				var eventLog = transportToTest.Logs.MostRecentLogByEventTime(Events.Departure);
				if (!eventLog.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out actualLocation))
				{
					actualLocation = string.Empty;
				}

				AssertEquals("Is log estimate", false, eventLog.SL_IsEstimate);
				AssertEquals("Event location", expectedEventLocation, actualLocation);
			};

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = ZString.Empty;
			assert(transport, string.Empty);

			transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "UAIEV";
			assert(transport, "UAIEV");
		}

		#endregion

		#region IScreeningPartyProvider

		public void TestGetWorstScreeningStatus()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			AssertEquals(transport.JW_VesselScreeningStatus, (transport as IScreeningPartyProvider).GetWorstScreeningStatus());
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			AssertEquals(transport.JW_VesselScreeningStatus, (transport as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared());
		}

		public void TestScreeningParties()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			var parties = (transport as IScreeningPartyProvider).ScreeningParties;
			CombineAssertions(() =>
			{
				AssertEquals(1, parties.Length);
				AssertEquals(transport, parties.Single().Parent);
				AssertEquals(transport, parties.Single().NotLinkedVessel);
				AssertEquals("Vessel", parties.Single().Description);
			});
		}

		public void TestScreeningStatus()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			AssertEquals(transport.JW_VesselScreeningStatus, (transport as IScreeningPartyProvider).ScreeningStatus);
			(transport as IScreeningPartyProvider).ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(ScreeningStatusesList.Codes.Matched, transport.JW_VesselScreeningStatus);
		}

		#endregion

		#region IScreeningPartyForVessel

		public void TestIScreeningPartyForVessel_CodeAndCurrentScreeningStatus()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_Vessel = "123456";
			transport.JW_VesselScreeningStatus = "CLR";
			AssertNotNull("Precondition: Transport should be IScreeningPartyForVessel", transport);
			AssertEquals("Precondition", "123456", ((IScreeningPartyForVessel)transport).Code);
			AssertEquals("Precondition", "CLR", ((IScreeningPartyForVessel)transport).CurrentScreeningStatus);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestNoExceptionIsThrownInSavingProcess()
		{
			var factory1 = new BusinessObjectFactory();
			var consol = factory1.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 15, 0);
			transport.ReadOnly = false;
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var transportInDb = factory2.Load<Transport>(transport.PK);

			transportInDb.ParentType = typeof(CommonConsol);
			transportInDb.JW_RL_NKLoadPort = "AUSYD";
			transportInDb.JW_RL_NKDiscPort = "NLAMS";

			transport.Delete();
			factory2.Save();

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(factory1.Save, null, true));

			factory1.Save();
		}

		[ExpectNoExceptions]
		public void TestNoExceptionIsThrownInSavingProcessIfTransportIsDeleted()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "CONDOR";
			transport.JW_VoyageFlight = "012";
			transport.JW_ETD = new ZDateTime(2023, 02, 22);
			transport.JW_ETA = new ZDateTime(2023, 02, 22);
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			transport.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 22);
			transport.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 22);
			transport.JW_TerminalCutOff = new ZDateTime(2023, 02, 22);
			transport.JW_DepotCutOff = new ZDateTime(2023, 02, 22);
			transport.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 22);
			transport.JW_VGMCutOff = new ZDateTime(2023, 02, 22);

			Factory.RefreshEnabled = false;
			Factory.Save();
			AssertEquals("CONDOR", transport.JW_VesselOriginalValue.Value);
			transport.JW_Vessel = "CONDOR1";
			transport.UpdateVoyageRelatedOriginalValues();
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);

			transport.JW_Vessel = "CONDOR2";
			transport.Delete();
			transport.UpdateVoyageRelatedOriginalValues();
			AssertEquals("CONDOR1", transport.JW_VesselOriginalValue.Value);
			Factory.Save();
		}

		#region concurrency save issue

		[ExpectNoExceptions]
		public void TestNoConcurrencyExceptionIsThrownInSavingProcess()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_OH = carrier.PK;

			transport.JW_JX = NewSailing(vessel, "123", "AUSYD", "NZAKL").PK;
			sailingManager.Sailing = transport.Sailing;

			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var consolInFactory1 = factory1.Load<CommonConsol>(consol.PK);
			var transportInFactory1 = consolInFactory1.Transports[0];
			transportInFactory1.ParentType = typeof(CommonConsol);

			var consolInFactory2 = factory2.Load<CommonConsol>(consol.PK);
			var transportInFactory2 = consolInFactory2.Transports[0];
			transportInFactory2.ParentType = typeof(CommonConsol);
			transportInFactory2.JW_ETA = ZDateTime.Today.AddDays(9);

			consolInFactory1.Transports.RemoveAndDelete(transportInFactory1);
			var newTransportInFactory1 = consolInFactory1.Transports[0];
			newTransportInFactory1.JW_IsLinked = true;
			newTransportInFactory1.JW_JX = transport.Sailing.PK;
			var newSailingManager = GetSailingManager(newTransportInFactory1);
			newSailingManager.Sailing = newTransportInFactory1.Sailing;
			newTransportInFactory1.JW_ETA = ZDateTime.Today.AddDays(8);

			factory2.Save();

			try
			{
				factory1.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert("Save Concurrency Exception", true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				factory1.Save();
				Assert("Save Succeeded", true);
			}
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyExceptionIsThrownInSavingSailingFormProcess()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			var transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_ETD = ZDateTime.Today;
			var sailingManager2 = GetSailingManager(transport2);
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CARRIER2";
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsShippingProvider = true;
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Vessel2";
			vessel2.RV_OH = carrier2.PK;
			vessel2.RV_Code = "ABC";

			transport2.JW_JX = NewSailing(vessel2, "456", "SGSIN", "CNSHA").PK;
			sailingManager2.Sailing = transport2.Sailing;

			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var transport2InFactory1 = factory1.Load<Transport>(transport2.PK);
			transport2InFactory1.ParentType = typeof(CommonConsol);
			var voyageQuery = new ZQuery();
			voyageQuery.AddToFilter(JoinCondition.And, JobVoyageSchema.JV_RV_NKVessel, "ABC");
			voyageQuery.AddToFilter(JoinCondition.And, JobVoyageSchema.JV_VoyageFlight, "456");
			var voyageInFactory1 = factory1.LoadTop1<JobVoyage>(voyageQuery);
			AssertNotNull(voyageInFactory1);

			var transport2InFactory2 = factory2.Load<Transport>(transport2.PK);
			transport2InFactory2.ParentType = typeof(CommonConsol);
			transport2InFactory2.JW_VoyageFlight = "789";

			var consolInFactory1 = factory1.Load<CommonConsol>(consol.PK);
			consolInFactory1.Transports.RemoveAndDelete(transport2InFactory1);

			factory2.Save();

			try
			{
				factory1.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert("Save Concurrency Exception", true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				factory1.Save();
				Assert("Save Succeeded", true);
			}

			voyageInFactory1.Origins[0].JA_E_DEP = ZDateTime.Today.AddDays(5);
			factory1.Save();
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyExceptionIsThrownInSynchroniseConcurrencyMergedPropertiesSavingProcess()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_TransportMode = "SEA";
				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;

				var sailingManager = GetSailingManager(transport);
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_Code = "CARRIER";
				carrier.OH_IsShippingLine = true;
				carrier.OH_IsShippingProvider = true;
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";
				vessel.RV_OH = carrier.PK;

				transport.JW_JX = NewSailing(vessel, "123", "AUSYD", "NZAKL").PK;
				sailingManager.Sailing = transport.Sailing;

				Factory.Save();

				var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

				var consolInFactory1 = factory1.Load<CommonConsol>(consol.PK);
				var transportInFactory1 = consolInFactory1.Transports[0];
				transportInFactory1.ParentType = typeof(CommonConsol);
				transportInFactory1.BeforeSynchroniseConcurrencyMergedPropertiesForTest = transportInFactory1.Delete;

				var consolInFactory2 = factory2.Load<CommonConsol>(consol.PK);
				var transportInFactory2 = consolInFactory2.Transports[0];
				transportInFactory2.ParentType = typeof(CommonConsol);
				transportInFactory2.JW_ETA = ZDateTime.Today.AddDays(9);

				consolInFactory1.Transports.RemoveAndDelete(transportInFactory1);
				var newTransportInFactory1 = consolInFactory1.Transports[0];
				newTransportInFactory1.JW_IsLinked = true;
				newTransportInFactory1.JW_JX = transport.Sailing.PK;
				var newSailingManager = GetSailingManager(newTransportInFactory1);
				newSailingManager.Sailing = newTransportInFactory1.Sailing;
				newTransportInFactory1.JW_ETA = ZDateTime.Today.AddDays(8);
				newTransportInFactory1.BeforeSynchroniseConcurrencyMergedPropertiesForTest = newTransportInFactory1.Delete;

				factory2.Save();

				try
				{
					factory1.Save();
					Fail();
				}
				catch (ZSaveConcurrencyException ex)
				{
					Assert("Save Concurrency Exception", true);
					ZExceptionReporting.HandleSaveException(ex);
				}
				finally
				{
					factory1.Save();
					Assert("Save Succeeded", true);
				}
			}
		}

		#endregion

		public void TestTransportSailingManagerNeedToBeReleaseAfterTransportModeChanges()
		{
			ErrorReporter.Clear();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ABC";

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_ETDForBinding = new ZDateTime(2012, 1, 1);
			transport.JW_ETAForBinding = new ZDateTime(2012, 2, 15);
			transport.JW_IsLinked = true;
			transport.JW_VoyageFlightForBinding = "TEST1234";
			transport.JW_VesselForBinding = vessel.RV_Code;

			var voyageQuery = new ZQuery();
			voyageQuery.AddToFilter(JoinCondition.And, JobVoyageSchema.JV_RV_NKVessel, "ABC");
			voyageQuery.AddToFilter(JoinCondition.And, JobVoyageSchema.JV_VoyageFlight, "TEST1234");

			var voyage = Factory.LoadTop1<JobVoyage>(voyageQuery);
			AssertNotNull(voyage);

			voyage.EnableOrphanedVoyageReportingForTests = true;
			consol.JK_TransportMode = Constants.TransportModes.Road;

			Factory.Save();

			var reloadVoyage = Factory.LoadTop1<JobVoyage>(voyageQuery);
			AssertNull(reloadVoyage);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		[TestDate]
		public void TestGetStackTraceForUnexplainedATADate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "SOF";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BRN";
			branch.GB_RL_NKHomePort = "AUMEL";

			Factory.Save();

			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "ULV";
			ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = "VAR";
			var bpGuid = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "~BP")).PK.ToGuid();

			using (Env.SetTemporaryUserContext(bpGuid, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Pre-condition: this license code is required to trigger error reporter", "ULVSOFVAR", EnvProxy.Instance.CurrentCompany.GetLicenceCode());

				TestDateAttribute.Date = DateTime.Now.AddMinutes(-5);
				Transport.JW_ATA = DateTime.Now;
				Factory.Save();

				Assert("Expected no error to be reported as the JW_ATA > 1 minute before saving", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

				TestDateAttribute.Date = DateTime.Now;
				Transport.JW_ATA = DateTime.Now;
				Factory.Save();

				AssertEquals("ClientULVATAUpdateStackTrace", ErrorReporter.LastKeyReported);
				AssertContains("Contact the International Team (Work Item WI00104187).\r\n", ErrorReporter.LastMessageReported);
				AssertContains("Transport PK: " + Transport.PK, ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestSailingManagerHasSufficientInformation()
		{
			var transport = Factory.New<Transport>();
			AssertEquals("New Sailing Manager does not have sufficient information", false, transport.SailingManagerHasSufficientInformation());

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_IsLinked = true;
			transport.JW_Vessel = "SSDMITRY";
			transport.JW_VoyageFlight = "DM1";
			AssertEquals("Initialized Sailing Manager does have sufficient information", true, transport.SailingManagerHasSufficientInformation());
		}

		public void TestSailingManagerIsImportingData()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var consolTransport = consol.Transports.AddNew();

			AssertEquals("Pre-condition: Parent Consol is not importing data by default", false, ((ISupportDataImporting)consol).IsImportingData);
			AssertEquals("Transport IsImportingData should match Parent Consol", false, ((ISailingManaged)consolTransport).IsImportingData);

			((ISupportDataImporting)consol).IsImportingData = true;
			AssertEquals("Transport IsImportingData should match Parent Consol", true, ((ISailingManaged)consolTransport).IsImportingData);

			var shipment = Factory.New<CommonConsol>();
			shipment.JK_TransportMode = Constants.TransportModes.Air;
			var shipmentTransport = shipment.Transports.AddNew();

			AssertEquals("Pre-condition: Parent Shipment is not importing data by default", false, ((ISupportDataImporting)shipment).IsImportingData);
			AssertEquals("Transport IsImportingData should match Parent Shipment", false, ((ISailingManaged)shipmentTransport).IsImportingData);

			((ISupportDataImporting)shipment).IsImportingData = true;
			AssertEquals("Transport IsImportingData should match Parent Shipment", true, ((ISailingManaged)shipmentTransport).IsImportingData);
		}

		public void TestDataRefreshDeleteSetsSailingManagerToNull()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = voyage.Sailings[0].PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var loadedConsol = newFactory.Load<CommonConsol>(consol.PK);
			var loadedTransport = loadedConsol.Transports[1];
			loadedTransport.Delete();
			newFactory.Save();

			AssertNull(typeof(Transport).GetField("fSailingManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loadedTransport));
			AssertNull(typeof(Transport).GetField("fSailingManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transport1));
		}

		public void TestDateEventNotAddedIfAlreadyExist()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports.AddNew();
			var transport2 = consol.Transports.AddNew();

			transport1.Logs.AddNew(Events.Departure, "existing log", new ZDateTimeOffset(2010, 01, 17), false);

			transport1.JW_ATD = new ZDateTime(2010, 01, 17);
			transport2.JW_ATD = new ZDateTime(2010, 01, 17);

			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			filter.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			var logs1 = transport1.Logs.Find(filter);
			var logs2 = transport2.Logs.Find(filter);

			CombineAssertions(delegate
			{
				AssertEquals(1, logs1.Length);
				AssertEquals("Changed To: 17-Jan-10", logs1.First().ReferenceFreeText);
				Assert(!logs1.First().SL_IsCancelled);

				AssertEquals(1, logs2.Length);
				AssertEquals("Changed To: 17-Jan-10", logs2.First().ReferenceFreeText);
				Assert(!logs2.First().SL_IsCancelled);
			});
		}

		public void TestDateEvent_NotAddedIfAlreadyExistInAnotherFactory()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "FOX123";
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			origin.JA_E_DEP = new ZDateTime(2016, 05, 01);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = new ZDateTime(2016, 05, 15);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_JX = sailing.PK;
			transport1.JW_Vessel = "A";
			transport1.JW_ATD = new ZDateTime(2016, 05, 01);
			transport1.JW_ATA = new ZDateTime(2016, 05, 15);
			Factory.Save();

			var depFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code);
			depFilter.AddToFilter(StmALogSchema.SL_IsEstimate, false);
			var arvFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Arrival.Code);
			arvFilter.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			var depLogs = transport1.Logs.Find(depFilter);
			var arvLogs = transport1.Logs.Find(arvFilter);

			AssertEquals("Precodition: Should not have any Log", 1, depLogs.Length);
			AssertEquals("Precodition: Should not have any Log", 1, arvLogs.Length);

			var newFactory = new BusinessObjectFactory();
			var oldConsol = newFactory.Load<CommonConsol>(consol.PK);
			var oldTransport = (Transport)oldConsol.Transports.FindByPK(transport1.PK);

			oldConsol.JK_TransportMode = Constants.TransportModes.Air;
			oldTransport.JW_VoyageFlight = "FOX123";
			newFactory.Save();

			depLogs = transport1.Logs.Find(depFilter);
			arvLogs = transport1.Logs.Find(arvFilter);

			AssertEquals("Should not add an new Log", 1, depLogs.Length);
			AssertEquals("Should not add an new Log", 1, arvLogs.Length);

			Assert("Old log should not be cancelled", !depLogs[0].SL_IsCancelled);
			Assert("Old log should not be cancelled", !arvLogs[0].SL_IsCancelled);
		}

		public void TestUTCTimes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "BRRIO";
			transport.JW_ETD = new ZDateTime(2011, 6, 10, 12, 0, 0);
			transport.JW_ATD = new ZDateTime(2011, 6, 12, 8, 0, 0);
			transport.JW_STD = new ZDateTime(2011, 6, 12, 10, 0, 0);
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_ETA = new ZDateTime(2011, 6, 20, 12, 0, 0);
			transport.JW_ATA = new ZDateTime(2011, 6, 24, 16, 0, 0);
			transport.JW_STA = new ZDateTime(2011, 6, 20, 10, 0, 0);

			//BRRIO is 3 hours behind UTC
			AssertEquals("JW_ETD_UTC", new ZDateTime(2011, 6, 10, 15, 0, 0), transport.JW_ETD_UTC);
			AssertEquals("JW_ATD_UTC", new ZDateTime(2011, 6, 12, 11, 0, 0), transport.JW_ATD_UTC);
			AssertEquals("JW_STD_UTC", new ZDateTime(2011, 6, 12, 13, 0, 0), transport.JW_STD_UTC);

			//HKHKG is 8 hours ahead of UTC
			AssertEquals("JW_ETA_UTC", new ZDateTime(2011, 6, 20, 4, 0, 0), transport.JW_ETA_UTC);
			AssertEquals("JW_ATA_UTC", new ZDateTime(2011, 6, 24, 8, 0, 0), transport.JW_ATA_UTC);
			AssertEquals("JW_ETA_UTC", new ZDateTime(2011, 6, 20, 2, 0, 0), transport.JW_STA_UTC);
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, transport.JW_ETD.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, transport.JW_ATD.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, transport.JW_ETA.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, transport.JW_ATA.Kind);
		}

		public void TestAllowScheduleCreation()
		{
			var mappings = new[]
			{
				new { Mode = Constants.TransportModes.Sea, Checkpoint = Env.Security.SailingScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Air, Checkpoint = Env.Security.FlightScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Road, Checkpoint = Env.Security.TruckScheduleCreateFromJob },
				new { Mode = Constants.TransportModes.Rail, Checkpoint = Env.Security.RailScheduleCreateFromJob },
			};

			foreach (var map in mappings)
			{
				map.Checkpoint.IsAllowed = false;
			}

			foreach (var map in mappings)
			{
				Transport.JW_TransportMode = map.Mode;

				map.Checkpoint.IsAllowed = true;
				AssertEquals(map.Mode + ": allowed", true, ((ISailingManaged)Transport).AllowScheduleCreation);

				map.Checkpoint.IsAllowed = false;
				AssertEquals(map.Mode + ": allowed", false, ((ISailingManaged)Transport).AllowScheduleCreation);
			}
		}

		public void TestAllowScheduleDatesChanging()
		{
			var mappings = new[]
			{
				new { Mode = Constants.TransportModes.Sea, Checkpoint = Env.Security.SailingScheduleEdit },
				new { Mode = Constants.TransportModes.Air, Checkpoint = Env.Security.FlightScheduleEdit },
				new { Mode = Constants.TransportModes.Road, Checkpoint = Env.Security.TruckScheduleEdit },
				new { Mode = Constants.TransportModes.Rail, Checkpoint = Env.Security.RailScheduleEdit },
			};

			foreach (var map in mappings)
			{
				Transport.JW_TransportMode = map.Mode;

				map.Checkpoint.IsAllowed = true;
				AssertEquals(map.Mode + ": allowed", true, ((ISailingManaged)Transport).AllowScheduleDatesChanging);

				map.Checkpoint.IsAllowed = false;
				AssertEquals(map.Mode + ": allowed", false, ((ISailingManaged)Transport).AllowScheduleDatesChanging);
			}
		}

		public void TestSailingManagerChecksAllowScheduleDatesChanging()
		{
			Env.Security.SailingScheduleEdit.IsAllowed = false;

			var today = ZDateTime.Today;
			var etd1 = today;
			var eta1 = today.AddDays(25);
			var etd2 = today.AddDays(2);
			var eta2 = today.AddDays(30);
			var etd3 = today.AddDays(3);
			var eta3 = today.AddDays(32);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "GH67";
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = etd1;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = eta1;
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			Factory.Save();

			AssertEquals(transport.Sailing.PK, sailing.PK);
			AssertEquals(etd1, sailing.JX_JA_E_DEP);
			AssertEquals(eta1, sailing.JX_JB_E_ARV);

			transport.JW_ETD = etd2;
			transport.JW_ETA = eta2;

			AssertEquals(transport.Sailing.PK, sailing.PK);
			AssertEquals(etd1, sailing.JX_JA_E_DEP);
			AssertEquals(eta1, sailing.JX_JB_E_ARV);

			Env.Security.SailingScheduleEdit.IsAllowed = true;

			transport.JW_ETD = etd3;
			transport.JW_ETA = eta3;

			AssertEquals(transport.Sailing.PK, sailing.PK);
			AssertEquals(etd3, sailing.JX_JA_E_DEP);
			AssertEquals(eta3, sailing.JX_JB_E_ARV);
		}

		public void TestUpdateETDAndETAOnShipmentWhenSailingChangesOnTransport()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "GH67";
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new ZDateTime(2013, 12, 23);

			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";
			destination1.JB_E_ARV = new ZDateTime(2013, 12, 29);

			voyage1.GenerateSailings();
			var sailing1 = voyage1.Sailings[0];

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ADBCG";

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "NM98";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_E_DEP = new ZDateTime(2013, 12, 24);

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZWEL";
			destination2.JB_E_ARV = new ZDateTime(2013, 12, 31);

			voyage2.GenerateSailings();
			var sailing2 = voyage2.Sailings[0];

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.MostInterestingTransportForBinding[0].JW_JX = sailing1.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;

			Factory.Save();

			AssertEquals(sailing1, consol.Transports[0].Sailing);
			AssertEquals(origin1.JA_E_DEP, consol.Transports[0].JW_ETD);
			AssertEquals(destination1.JB_E_ARV, consol.Transports[0].JW_ETA);
			AssertEquals(origin1.JA_S_DEP, consol.Transports[0].JW_STD);
			AssertEquals(destination1.JB_S_ARV, consol.Transports[0].JW_STA);
			AssertEquals(ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals(ZDateTime.Empty, shipment.JS_E_ARV);

			consol.Transports.DeleteAll();
			consol.MostInterestingTransportForBinding[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.MostInterestingTransportForBinding[0].JW_JX = sailing2.PK;

			AssertEquals("Consol transport updated to new sailing", sailing2, consol.Transports[0].Sailing);
			AssertEquals("Consol ETD date gets updated", origin2.JA_E_DEP, consol.Transports[0].JW_ETD);
			AssertEquals("Consol ETA date gets updated", destination2.JB_E_ARV, consol.Transports[0].JW_ETA);
			AssertEquals("Consol STD date gets updated", origin2.JA_S_DEP, consol.Transports[0].JW_STD);
			AssertEquals("Consol STA date gets updated", destination2.JB_S_ARV, consol.Transports[0].JW_STA);
			AssertEquals("Shipment ETD date gets updated", origin2.JA_E_DEP, shipment.JS_E_DEP);
			AssertEquals("Shipment ETA date gets updated", destination2.JB_E_ARV, shipment.JS_E_ARV);
		}

		public void TestDoNotUpdateShipmentETDAndETAWhenTransportETDAndETARemoved()
		{
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.MostInterestingTransportForBinding[0].JW_RL_NKLoadPort = "USCHI";
			consol.MostInterestingTransportForBinding[0].JW_RL_NKDiscPort = "AUSYD";

			var workflowProvider = consol as IWorkflowProvider;

			var depTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			depTrigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			depTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.Constants.JW_ETD;

			var arvTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			arvTrigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			arvTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.Constants.JW_ETA;

			consol.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2013, 12, 25);
			consol.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2013, 12, 28);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;
			Factory.Save();

			consol.MostInterestingTransportForBinding[0].JW_ETD = ZDateTime.Empty;
			consol.MostInterestingTransportForBinding[0].JW_ETA = ZDateTime.Empty;

			AssertEquals("shipment departure date is not updated.", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("shipment arrival date is not updated.", ZDateTime.Empty, shipment.JS_E_ARV);
		}

		public void TestWhenDepotCutOffDateIsUpdated_EventCreatedHasReferenceFacilitySetToDepot_TerminalCutoffDateIsNotChanged()
		{
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "AUSYD";

			Factory.Save();

			transport.JW_DepotCutOff = ZDateTime.Now;
			Factory.Save();

			var log = transport.Logs.GetAllLogs().Where(t => t.SL_SE_NKEvent == Events.CutOffDate.Code).FirstOrDefault();
			var eventParameters = StmALog.GetParametersFromReference(log.SL_Reference);

			AssertEquals("Parameter facility is set to Depot.", EventConstants.Facilities.Code.Depot, eventParameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Terminal cutoff date is not updated.", ZDateTime.Empty, transport.JW_TerminalCutOff);
		}

		public void TestCreateDeniedBySecurity()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Factory.Save();

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = false;

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;
			Transport.JW_Vessel = "CONDOR";
			Transport.JW_VoyageFlight = "012";
			Transport.JW_RL_NKLoadPort = "AUBNE";
			Transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("Sailing exists", false, Transport.CreateDeniedBySecurity);

			Transport.JW_RL_NKDiscPort = "";
			AssertEquals("Insufficent information", false, Transport.CreateDeniedBySecurity);

			Transport.JW_RL_NKDiscPort = "NLAMS";
			AssertEquals("Sailing does not exist", true, Transport.CreateDeniedBySecurity);

			Transport.JW_IsLinked = false;
			AssertEquals("Sailing not linked", false, Transport.CreateDeniedBySecurity);

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = true;
			Transport.JW_IsLinked = true;
			AssertEquals("create rights granted", false, Transport.CreateDeniedBySecurity);
		}

		public void TestMakeNonPersistent()
		{
			AssertEquals(true, Transport.IsSavedByFactory);
			AssertEquals(true, Transport.IsPersistent);

			Transport.MakeNonPersistent();
			AssertEquals(false, Transport.IsSavedByFactory);
			AssertEquals(false, Transport.IsPersistent);
		}

		public void TestThrowIfParentTypeNotSet()
		{
			ErrorReporter.Clear();

			var transport = Factory.New<Transport>();

			foreach (ZPropertyInfo info in transport.ZPropertyInfoHash)
			{
				if (!info.HasSetter)
				{
					continue;
				}

				try
				{
					AssertExceptionThrown(typeof(InvalidOperationException), "Transport Parent Type Not Set Yet", () => Populate(info, 3));
					AssertEquals("UnableToCreateOrLoadTransportWithoutParentType", ErrorReporter.LastKeyReported);
					Assert(ErrorReporter.LastMessageReported.Contains("Cannot create or load transport without setting parent type."));
				}
				catch
				{
					// to catch an exception from ThrowIfNoParentTransportStrategy
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			transport.ParentType = typeof(CommonConsol);

			foreach (ZPropertyInfo info in transport.ZPropertyInfoHash)
			{
				if (!info.HasSetter)
				{
					continue;
				}

				AssertNoExceptionThrown(() => Populate(info, 4));
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestFactorySaved_Trigger_SyncProxyFieldsWithLinkedSailing()
		{
			var today = ZDateTime.Today;
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var departureCTO1 = Factory.NewWithValidTestData<OrgHeader>();
			var departureCTO2 = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCTO1 = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCTO2 = Factory.NewWithValidTestData<OrgHeader>();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			var sailing = NewSailing(vessel, "111S", "AUSYD", "NZAKL");
			sailing.Voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			sailing.Voyage.JV_IsChartered = false;
			sailing.Voyage.JV_IsCargoOnly = false;
			sailing.Voyage.JV_AircraftType = "AT1";
			sailing.Origin.JA_OA_DepartureCTOAddress = departureCTO1.MainAddress.PK;
			sailing.Origin.JA_S_DEP = today;
			sailing.Origin.JA_E_DEP = today;
			sailing.Origin.JA_A_DEP = today;
			sailing.Destination.JB_OA_ArrivalCTOAddress = arrivalCTO1.MainAddress.PK;
			sailing.Destination.JB_S_ARV = today.AddDays(5);
			sailing.Destination.JB_E_ARV = today.AddDays(5);
			sailing.Destination.JB_A_ARV = today.AddDays(5);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			AssertTransportProxyFieldsResult(transport, "VESSEL 111", "111S", false, false, "AT1", "AUSYD", today, departureCTO1.MainAddress.PK, "NZAKL", today.AddDays(5), arrivalCTO1.MainAddress.PK);

			var otherFactorySailing = otherFactory.Load<JobSailing>(sailing.PK);

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "VESSEL 222";

			otherFactorySailing.Voyage.JV_VoyageFlight = "222S";
			otherFactorySailing.Voyage.JV_RV_NKVessel = vessel2.RV_FK;
			otherFactorySailing.Voyage.JV_IsChartered = true;
			otherFactorySailing.Voyage.JV_IsCargoOnly = true;
			otherFactorySailing.Voyage.JV_AircraftType = "AT2";

			otherFactorySailing.Origin.JA_RL_NKPortOfLoading = "AUBNE";
			otherFactorySailing.Origin.JA_OA_DepartureCTOAddress = departureCTO2.MainAddress.PK;
			otherFactorySailing.Origin.JA_S_DEP = today.AddDays(1);
			otherFactorySailing.Origin.JA_E_DEP = today.AddDays(1);
			otherFactorySailing.Origin.JA_A_DEP = today.AddDays(1);

			otherFactorySailing.Destination.JB_RL_NKPortOfDischarge = "HKHKG";
			otherFactorySailing.Destination.JB_OA_ArrivalCTOAddress = arrivalCTO2.MainAddress.PK;
			otherFactorySailing.Destination.JB_S_ARV = today.AddDays(10);
			otherFactorySailing.Destination.JB_E_ARV = today.AddDays(10);
			otherFactorySailing.Destination.JB_A_ARV = today.AddDays(10);

			otherFactory.Save();
			Factory.Save();

			transport = otherFactory.Load<Transport>(transport.PK);
			AssertTransportProxyFieldsResult(transport, "VESSEL 222", "222S", true, true, "AT2", "AUBNE", today.AddDays(1), departureCTO2.MainAddress.PK, "HKHKG", today.AddDays(10), arrivalCTO2.MainAddress.PK);
		}

		public void TestIsCargoOnly_Trigger()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			var sailing = Factory.New<JobSailing>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var voyage = Factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			voyage.JV_IsChartered = false;
			voyage.JV_IsCargoOnly = false;

			transport.JW_JX = sailing.PK;

			Factory.Save();

			AssertEquals(voyage.JV_IsChartered, transport.JW_IsCharter);
			AssertEquals(voyage.JV_IsCargoOnly, transport.JW_IsCargoOnly);

			voyage.JV_IsChartered = true;
			voyage.JV_IsCargoOnly = true;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherTransport = otherFactory.Load<Transport>(transport.PK);

			AssertEquals(voyage.JV_IsChartered, otherTransport.JW_IsCharter);
			// Will be added in later workflow of this WI
			// AssertEquals(voyage.JV_IsCargoOnly, otherTransport.JW_IsCargoOnly);
		}

		void AssertTransportProxyFieldsResult(Transport transport, ZString expectedVessel, ZString expectedVoyage, ZBool expectedIsChartered, ZBool expectedIsCargoOnly, ZString expectedAircraftType, ZString expectedPortOfLoading, ZDateTime expectedETD, ZGuid expectedDepatureCTOAddressPK, ZString expectedPortOfDischarge, ZDateTime expectedETA, ZGuid expectedArrivalCTOAddressPK)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JW_Vessel", expectedVessel, transport.JW_Vessel);
				AssertEquals("JW_VoyageFlight", expectedVoyage, transport.JW_VoyageFlight);
				AssertEquals("JW_IsCharter", expectedIsChartered, transport.JW_IsCharter);
				// Will be added in later workflow of this WI
				// AssertEquals("JW_IsCargoOnly", expectedIsCargoOnly, transport.JW_IsCargoOnly);
				AssertEquals("JW_AircraftType", expectedAircraftType, transport.JW_AircraftType);

				AssertEquals("JW_RL_NKLoadPort", expectedPortOfLoading, transport.JW_RL_NKLoadPort);
				AssertEquals("JW_OA_DepartureLocation", expectedDepatureCTOAddressPK, transport.JW_OA_DepartureLocation);
				AssertEquals("JW_STD", expectedETD, transport.JW_STD);
				AssertEquals("JW_ETD", expectedETD, transport.JW_STD);
				AssertEquals("JW_ATD", expectedETD, transport.JW_ATD);

				AssertEquals("JW_RL_NKDiscPort", expectedPortOfDischarge, transport.JW_RL_NKDiscPort);
				AssertEquals("JW_OA_ArrivalLocation", expectedArrivalCTOAddressPK, transport.JW_OA_ArrivalLocation);
				AssertEquals("JW_STA", expectedETA, transport.JW_STA);
				AssertEquals("JW_ETA", expectedETA, transport.JW_ETA);
				AssertEquals("JW_ATA", expectedETA, transport.JW_ATA);
			});
		}

		public void TestOtherInfoForRoadConsolPersisted()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "FOX123";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(11);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.MostInterestingTransportForBinding[0].JW_JX = sailing.PK;
			consol.MostInterestingTransportForBinding[0].JW_Vessel = "A";

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Voyage);
			AssertEquals(consol.MostInterestingTransportForBinding[0].JW_Vessel, consol.Transports[0].Voyage.JV_RV_NKVessel);
			AssertEquals("A", consol.MostInterestingTransportForBinding[0].JW_Vessel);
			AssertEquals("A", consol.Transports[0].Voyage.JV_RV_NKVessel);

			Factory.Save();

			var pk = consol.PK;

			consol = Factory.Load<CommonConsol>(pk);

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Voyage);
			AssertEquals(consol.MostInterestingTransportForBinding[0].JW_Vessel, consol.Transports[0].Voyage.JV_RV_NKVessel);
			AssertEquals("A", consol.MostInterestingTransportForBinding[0].JW_Vessel);
			AssertEquals("A", consol.Transports[0].Voyage.JV_RV_NKVessel);

			consol.Transports[0].JW_Vessel = "AB";

			Factory.Save();

			consol = Factory.Load<CommonConsol>(pk);

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Voyage);
			AssertEquals(consol.MostInterestingTransportForBinding[0].JW_Vessel, consol.Transports[0].Voyage.JV_RV_NKVessel);
			AssertEquals("AB", consol.MostInterestingTransportForBinding[0].JW_Vessel);
			AssertEquals("AB", consol.Transports[0].Voyage.JV_RV_NKVessel);
		}

		public void TestRegistrationNumberPersisted()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "QF800";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(11);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];

			transport.JW_JX = sailing.PK;
			transport.JW_VoyageFlight = "QF800";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;

			transport.JW_JX_JV_RegistrationNo = "RegNo";
			AssertEquals("Registration Number stored directly for non-charter", "RegNo", voyage.JV_RegistrationNo);

			transport.JW_IsCharter = true;
			transport.JW_JX_JV_RegistrationNo = "RegNo2";
			AssertEquals("Registration Number used as key for Charter", "", voyage.JV_RegistrationNo);
		}

		public void TestTransportModeDoesNotDefaultTransportTypeWhenThereIsNoValidListOfTransportTypes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];

			AssertEquals("Precondition: consol.Transports.Count", 1, consol.Transports.Count);

			transport.JW_TransportMode = "_W_";
			transport.JW_TransportMode = "";

			AssertEquals("transport.JW_TransportType cannot be defaulted so should be empty.", "", transport.JW_TransportType);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("transport.JW_TransportType should now be defaulted.", Constants.TransportPlanningType.MainVessel, transport.JW_TransportType);
		}

		public void TestIsVoyageFlightMatched()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			transport.JW_VoyageFlight = "QF1";
			Assert(transport.IsVoyageFlightMatched("qf1"));
			Assert(transport.IsVoyageFlightMatched("QF01"));
			Assert(transport.IsVoyageFlightMatched("QF001"));
			Assert(transport.IsVoyageFlightMatched("QF0001"));
			Assert(transport.IsVoyageFlightMatched("QF01"));

			Assert(!transport.IsVoyageFlightMatched("AF1"));
			Assert(!transport.IsVoyageFlightMatched("QF10"));
		}

		public void TestIsVoyageFlightFuzzyMatched()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			Assert(!transport.IsVoyageFlightFuzzyMatched(ZString.Empty));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF1"));

			transport.JW_VoyageFlight = "QF1";

			Assert(transport.IsVoyageFlightFuzzyMatched("QF1T"));
			Assert(transport.IsVoyageFlightFuzzyMatched("qf1T"));
			Assert(transport.IsVoyageFlightFuzzyMatched("QF01T"));
			Assert(transport.IsVoyageFlightFuzzyMatched("QF001T"));
			Assert(transport.IsVoyageFlightFuzzyMatched("QF0001T"));
			Assert(transport.IsVoyageFlightFuzzyMatched("QF01T"));

			Assert(!transport.IsVoyageFlightFuzzyMatched(string.Empty));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF1"));
			Assert(!transport.IsVoyageFlightFuzzyMatched("AF1"));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF10"));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF1TT"));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF12"));
			Assert(!transport.IsVoyageFlightFuzzyMatched("QF1*"));
		}

		public void TestIsVesselMatched()
		{
			AssertIsVesselMatched(ZString.Empty, ZString.Empty, 0.50f, true, "VESSELNAME", "vesselname");
			AssertIsVesselMatched(ZString.Empty, ZString.Empty, 0.50f, true, "VESSELNAME", "VeSsElNaMe");
			AssertIsVesselMatched(ZString.Empty, "123456", 1.00f, true);
			AssertIsVesselMatched("123456", ZString.Empty, 0.86f, true);
			AssertIsVesselMatched(ZString.Empty, ZString.Empty, 0.84f, false);
			AssertIsVesselMatched(ZString.Empty, ZString.Empty, 0.50f, false);
			AssertIsVesselMatched("123456", "123456", 0.50f, true);
			AssertIsVesselMatched("123456", "654321", 1.00f, false);
		}

		void AssertIsVesselMatched(string lloydsNumber, string inputLloydsNumber, float similarity, bool expected, string vesselName = "VESSELNAME", string inputVesselName = "VESSEL")
		{
			var mockStringSimilarity = new Mock<IStringSimilarity>();
			mockStringSimilarity.Setup(s => s.CalculateSimilarity(It.IsAny<String>(), It.IsAny<String>()))
				.Returns(similarity);

			var refVessel = LoadOrCreateRefVessel(vesselName);
			refVessel.RV_LloydsNumber = lloydsNumber;
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];
			transport.JW_Vessel = refVessel.RV_Name;
			transport.StringSimilarity = mockStringSimilarity.Object;

			AssertEquals(expected, transport.IsVesselMatched(inputLloydsNumber, inputVesselName));
		}

		RefVessel LoadOrCreateRefVessel(string name)
		{
			var vessel = RefVessel.LookupVesselByName(name, Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Name = name;
			}

			return vessel;
		}

		public void TestIsDomestic()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals("it's not domestic", false, transport.IsDomestic);
			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("it's domestic", true, transport.IsDomestic);
			transport.JW_RL_NKDiscPort = "USCHI";
			AssertEquals("it's not domestic", false, transport.IsDomestic);
			transport.JW_RL_NKLoadPort = "USNYC";
			AssertEquals("it's domestic", true, transport.IsDomestic);
			transport.IsDomestic = false;
			AssertHasErrors("can't set is domestic to false when load discharge are the same countries", transport.IsDomesticInfo);
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.IsDomestic = true;
			AssertHasErrors("can't set is domestic to true when load discharge are different countries", transport.IsDomesticInfo);
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "A1AAA";
			AssertEquals(false, transport.IsDomestic);
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_RL_NKLoadPort = "USNYC";
			transport.JW_RL_NKDiscPort = "A1AAA";
			AssertEquals(false, transport.IsDomestic);

			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals(true, transport.IsDomestic);

			Factory.Save();
			Transport loadedTransport = new BusinessObjectFactory().Load<Transport>(transport.PK);
			AssertEquals(true, loadedTransport.IsDomestic);

			CommonConsol loadedConsol = new BusinessObjectFactory().Load<CommonConsol>(consol.PK);
			Transport consolTransport = (Transport)loadedConsol.Transports.FindByPK(transport.PK);
			AssertEquals(true, consolTransport.IsDomestic);
			AssertEquals(false, loadedConsol.HasChanges);
		}

		public void TestIsDomesticFromSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			Factory.Save();

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports.AddNew();

			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = "";
			AssertEquals("precondition:", false, transport.IsDomestic);

			transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "AUSYD").PK;
			AssertEquals("set to domestic sailing", true, transport.IsDomestic);

			transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NZAKL").PK;
			AssertEquals("set to export sailing", false, transport.IsDomestic);
		}

		public void TestChangingLegOrderValidatesTheEstimatedDates()
		{
			const string error = "The date order does not reflect the leg order.";
			ZDateTime now = ZDateTime.Now;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport1 = consol.Transports[0];
			transport1.JW_ETD = now.AddDays(15);
			transport1.JW_ETA = now.AddDays(20);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = now.AddDays(5);
			transport2.JW_ETA = now.AddDays(10);

			consol.RunPreSaveValidation();
			AssertHasError(transport1.JW_ETAInfo, error);
			AssertHasError(transport2.JW_ETDInfo, error);

			transport1.JW_LegOrder = 2;
			transport2.JW_LegOrder = 1;

			AssertNoError(transport1.JW_ETAInfo, error);
			AssertNoError(transport2.JW_ETDInfo, error);
		}

		public void TestIsLinkedChangesValidatesVoyageflight()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			var transport = consol.Transports.AddNew();
			AssertNoError(transport.JW_VoyageFlightInfo, "Please enter a journey number.");

			transport.JW_IsLinked = true;
			AssertHasError(transport.JW_VoyageFlightInfo, "Please enter a journey number.");
		}

		[ExpectNoExceptions]
		public void TestConcurrencyOnDeletedBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport = shipment.Transports.AddNew();
			Factory.Save();
			transport.JW_VoyageFlight = "Factory";

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Transport transportInNewFactory = newFactory.Load<Transport>(transport.PK);
			transportInNewFactory.ParentType = transport.ParentType;
			transportInNewFactory.Delete();

			using (GetFactoryIsolater(Factory))
			using (GetFactoryIsolater(newFactory))
			{
				newFactory.Save();
			}

			try
			{
				Factory.Save();
				Fail("save should throw a ZSaveConcurrencyException");
			}
			catch (ZSaveConcurrencyException ex)
			{
				// should not throw an exception while handling the ZSaveConcurrencyException.
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public void TestConcurrencyChangesOnProxiedFieldsForLinkedTransport()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			Transport.JW_IsLinked = true;
			Transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;

			Factory.Save();
			Assert("Precondition: Original Db value for IsLinked should be true", (ZBool)Transport.JW_IsLinkedInfo.OriginalValue);

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.New<CommonShipment>();
			var transportInNewFactory = newFactory.Load<Transport>(Transport.PK);
			shipment.Transports.Add(transportInNewFactory);

			GetProxiedInfoFieldsOnTransport(transportInNewFactory)
				.ForEach(fieldInfo => AssertEquals("Concurrency policy should change to ignore for: " + fieldInfo.Name, ConcurrencyPolicy.Ignore, fieldInfo.ConcurrencyPolicy));

			transportInNewFactory.JW_IsLinked = false;

			GetProxiedInfoFieldsOnTransport(transportInNewFactory)
				.ForEach(fieldInfo => AssertEquals("Concurrency policy should still be ignore for: " + fieldInfo.Name, ConcurrencyPolicy.Ignore, fieldInfo.ConcurrencyPolicy));

			newFactory.Save();
			Assert("Precondition: Original Db value for IsLinked should be false", !(ZBool)Transport.JW_IsLinkedInfo.OriginalValue);

			GetProxiedInfoFieldsOnTransport(transportInNewFactory)
				.ForEach(fieldInfo => AssertEquals("Concurrency policy should not be ignore for: " + fieldInfo.Name, ConcurrencyPolicy.Default, fieldInfo.ConcurrencyPolicy));
		}

		public void TestEmptyValueInProxiedFieldsDoNotCauseLoopSave()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			Transport.JW_IsLinked = true;
			Transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			Assert("Transport is not saved yet.", !Transport.IsInDatabase);

			Factory.Save();

			Assert("Precondition: Transport is saved in database.", Transport.IsInDatabase);
			Assert("Precondition: Transport has no changes.", !Transport.HasChanges);
			AssertNoMessageErrors("Precondition: Transport has no error messages.", Transport);
			AssertGreaterThanOrEqualTo("Precondition: Transport has empty proxied properties and saved as DBNull in database.",
				GetProxiedInfoFieldsOnTransport(Transport).Where(p => p.Value.IsEmpty && (Transport as INeedRow).Row[p.Name] == DBNull.Value).Count(),
				1);

			Transport.RunPreSaveValidation();

			AssertNoMessageErrors("Transport should have no error messages after pre-save validation.", Transport);
			Assert("Transport should still have no changes after pre-save validation.", !Transport.HasChanges);
		}

		[ExpectNoExceptions]
		public void TestParentTypeSetForConcurrencyResolution()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport = shipment.Transports.AddNew();
			Factory.Save();
			transport.JW_VoyageFlight = "Factory";

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Transport transportInNewFactory = newFactory.Load<Transport>(transport.PK);
			transportInNewFactory.ParentType = transport.ParentType;
			transportInNewFactory.JW_VoyageFlight = "newFactory";

			using (GetFactoryIsolater(Factory))
			using (GetFactoryIsolater(newFactory))
			{
				newFactory.Save();
			}

			try
			{
				Factory.Save();
				Fail("save should throw a ZSaveConcurrencyException");
			}
			catch (ZSaveConcurrencyException ex)
			{
				// should not throw an exception while handling the ZSaveConcurrencyException.
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public void TestSetParentConsol()
		{
			JobSailing sailing1 = NewSailing(TestVessel1, "Blah", HomePort, OverseasPort);
			JobSailing sailing2 = NewSailing(TestVessel1, "Npaj", HomePort, OverseasPort);
			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];

			AssertNull("precondition:", sailing1.Voyage.ParentConsol);
			AssertNull("precondition:", sailing2.Voyage.ParentConsol);

			transport.JW_JX = sailing1.PK;
			AssertEquals("Should have set ParentConsol for sailing1.", consol, sailing1.Voyage.ParentConsol);
			AssertNull("sailing2 is on a separate voyage.", sailing2.Voyage.ParentConsol);

			transport.JW_VoyageFlight = "Npaj";
			AssertEquals("Should have set ParentConsol for sailing2.", consol, sailing2.Voyage.ParentConsol);
		}

		public void TestRoadLegsDefaultTruckRefFromParent()
		{
			var consol = Factory.New<CommonConsol>();

			var seaLeg = consol.Transports[0];
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			seaLeg.JW_VoyageFlight = "";

			var roadLeg1 = consol.Transports.AddNew();
			roadLeg1.JW_TransportMode = Core.Constants.TransportModes.Road;
			roadLeg1.JW_VoyageFlight = "";

			var roadLeg2 = consol.Transports.AddNew();
			roadLeg2.JW_TransportMode = Core.Constants.TransportModes.Road;
			roadLeg2.JW_VoyageFlight = "Blah";

			Factory.Save();

			AssertEquals("JW_VoyageFlight on a sea transport leg should not be set to a default value when saving", "", seaLeg.JW_VoyageFlight);
			AssertEquals("JW_VoyageFlight on a road transport leg should not be set to a default value when saving", "", roadLeg1.JW_VoyageFlight);
			AssertEquals("JW_VoyageFlight should not be changed when saving", "Blah", roadLeg2.JW_VoyageFlight);
		}

		public void TestChangingKeyFieldsRefreshesBindingsOnlyOnce()
		{
			Transport.JW_IsLinked = true;

			ZPropertyInfo[] infos = new ZPropertyInfo[]
			{
				Transport.JW_VesselInfo,
				Transport.JW_VoyageFlightInfo,
				Transport.JW_RL_NKLoadPortInfo,
				Transport.JW_RL_NKDiscPortInfo,
				Transport.JW_ETDInfo,
				Transport.JW_ETAInfo
			};

			int count = 0;
			EventHandler handler = delegate
			{ count++; };

			foreach (ZPropertyInfo info in infos)
			{
				info.Value = GetValueForPopulation(info, 1);
				count = 0;

				info.ValueChanged += handler;
				info.Value = GetValueForPopulation(info, 2);
				info.ValueChanged -= handler;

				AssertEquals(info.Name, 1, count);
			}
		}

		public void TestHumanReadableName_NonConsolParent()
		{
			var shipment = Factory.New<CommonShipment>();
			var transport = shipment.Transports.AddNew();

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Transport Leg", "Transport Leg (Flight='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_VoyageFlight = "voy";
			transport.JW_Vessel = "ves";
			transport.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.NewZGuid());
			AssertEquals("Sailing schedule", "Transport Leg (Vessel='ves', Voyage='voy', Carrier='')", transport.HumanReadableName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CARRIER";
			transport.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(org.PK);
			AssertEquals("Sailing schedule", "Transport Leg (Vessel='ves', Voyage='voy', Carrier='CARRIER')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Trucking journey", "Transport Leg (Truck='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Rail journey", "Transport Leg (Journey='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = "xxx";
			AssertEquals("Default for when no transport mode", "Transport Leg", transport.HumanReadableName);
		}

		public void TestHumanReadableName_ConsolParent()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C0069420";

			var transport = consol.Transports[0];

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Transport Leg", "Transport Leg (Consol='C0069420', Flight='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_VoyageFlight = "voy";
			transport.JW_Vessel = "ves";
			transport.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.NewZGuid());
			AssertEquals("Sailing schedule", "Transport Leg (Consol='C0069420', Vessel='ves', Voyage='voy', Carrier='')", transport.HumanReadableName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CARRIER";
			transport.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(org.PK);
			AssertEquals("Sailing schedule", "Transport Leg (Consol='C0069420', Vessel='ves', Voyage='voy', Carrier='CARRIER')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Trucking journey", "Transport Leg (Consol='C0069420', Truck='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport.JW_VoyageFlight = "voy";
			AssertEquals("Rail journey", "Transport Leg (Consol='C0069420', Journey='voy')", transport.HumanReadableName);

			transport.JW_TransportMode = Core.Constants.TransportModes.Storage;
			AssertEquals("Default for when no transport mode", "Transport Leg (Consol='C0069420')", transport.HumanReadableName);
		}

		#region JW_OA_CarrierAddress - Set Creditor default value

		public void TestDefaultForwardingConsolRoutingCreditorFromRoutingCarrierRelatedPartyForPrepaid()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var mainTransport = consol.Transports[0];
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "AUMEL";
			mainTransport.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Preconditions: Forwarding Consol expected.", true, consol.JK_IsForwarding);
			AssertEquals("Preconditions: Creditor is empty.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			var legCarrier = Factory.NewWithValidTestData<OrgHeader>();
			legCarrier.OH_IsCreditor = false;
			mainTransport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("leg carrier is not payable, we don't set creditor", ZGuid.Empty, mainTransport.JW_OA_CreditorAddress);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			legCarrier.SetRelatedParty(orgHeader, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, mainTransport.JW_TransportMode, consol.JK_ConsolMode, mainTransport.JW_RL_NKLoadPort);

			legCarrier.OH_IsCreditor = true;

			Factory.Save();

			mainTransport.JW_OA_CreditorAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related part is not payable", false, orgHeader.OH_IsCreditor);
			AssertEquals("if related party is not payable, we shouldn't use it for defaulting and use leg carrier if it is payable", legCarrier.MainAddress.PK, mainTransport.JW_OA_CreditorAddress);

			orgHeader.OH_IsCreditor = true;
			Factory.Save();

			mainTransport.JW_OA_CreditorAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related party is payable: Set leg creditor address from leg carrier payable related party (Pickup And Delivery) using route set load location", orgHeader.MainAddress.PK, mainTransport.JW_OA_CreditorAddress);

			var parties = legCarrier.AllRelatedParties;
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;
			var partyRecord = parties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;
			partyRecord.PR_FreightTransportMode = mainTransport.JW_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = mainTransport.JW_RL_NKLoadPort;

			Factory.Save();

			mainTransport.JW_OA_CreditorAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = ZGuid.Empty;
			mainTransport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related party is payable: Set leg creditor address from leg carrier related party (Pickup) using route set load location", relatedParty.MainAddress.PK, mainTransport.JW_OA_CreditorAddress);

			var transport1 = consol.Transports.AddNew("AUMEL", "HKHKG");
			transport1.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Preconditions: transport and transport1 have the same route set number", mainTransport.RouteSetNumber, transport1.RouteSetNumber);

			transport1.JW_OA_CreditorAddress = ZGuid.Empty;
			transport1.JW_OA_CarrierAddress = ZGuid.Empty;
			transport1.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("Preconditions: we set related party for first Load of route set(AUSYD)", "AUSYD", partyRecord.PR_Location);
			AssertEquals("We should use first Load Port of  route set", relatedParty.MainAddress.PK, transport1.JW_OA_CreditorAddress);

			var transport2 = consol.Transports.AddNew("HKHKG", "USLAX");
			transport2.JW_CarrierBookingReference = "2";

			AssertEquals("Preconditions: transport2 route set is 2", 2, transport2.RouteSetNumber);

			transport2.JW_OA_CreditorAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("This is new route set (HKHKG -> USLAX) and we only have related parties for AUSYD (first route set load port), should fallback to Route>carrier", legCarrier.MainAddress.PK, transport2.JW_OA_CreditorAddress);

			partyRecord.PR_Location = transport2.JW_RL_NKLoadPort;
			Factory.Save();

			transport2.JW_OA_CreditorAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("Preconditions: we set related party for first Load of route set (HKHKG)", "HKHKG", partyRecord.PR_Location);
			AssertEquals("We should use first load port of route set 2", relatedParty.MainAddress.PK, transport2.JW_OA_CreditorAddress);
		}

		public void TestDefaultForwardingConsolRoutingCreditorFromRoutingCarrierRelatedPartyForCollect()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Preconditions: Forwarding Consol expected.", true, consol.JK_IsForwarding);
			AssertEquals("Preconditions: Creditor is empty.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			var legCarrier = Factory.NewWithValidTestData<OrgHeader>();
			legCarrier.OH_IsCreditor = false;
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("leg carrier is not payable, we don't set creditor", ZGuid.Empty, transport.JW_OA_CreditorAddress);

			legCarrier.OH_IsCreditor = true;

			var parties = legCarrier.AllRelatedParties;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var partyRecord = parties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;
			partyRecord.PR_FreightTransportMode = transport.JW_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = transport.JW_RL_NKDiscPort;

			Factory.Save();

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related part is not payable", false, relatedParty.OH_IsCreditor);
			AssertEquals("Set leg creditor address from leg carrier because related party is not payable and leg carrier is payable", legCarrier.MainAddress.PK, transport.JW_OA_CreditorAddress);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related party is payable: Set leg creditor address from leg carrier related party (Pickup And Delivery) using route set discharge location", relatedParty.MainAddress.PK, transport.JW_OA_CreditorAddress);

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty1.OH_IsCreditor = true;

			var partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;
			partyRecord1.PR_FreightTransportMode = transport.JW_TransportMode;
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = transport.JW_RL_NKDiscPort;

			Factory.Save();

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("related party is payable: Set leg creditor address from leg carrier related party (Delivery) using route set discharge location", relatedParty1.MainAddress.PK, transport.JW_OA_CreditorAddress);

			var transport1 = consol.Transports.AddNew("AUMEL", "HKHKG");
			transport1.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Preconditions: transport and transport1 have the same route set number", transport.RouteSetNumber, transport1.RouteSetNumber);
			AssertEquals("Preconditions: we set related party for first leg discharge port AUMEL", "AUMEL", partyRecord1.PR_Location);

			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_OA_CreditorAddress = ZGuid.Empty; // reset route leg creditor
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("We should use last discharge port of route set (HKHKG), so should not find any related party and should fallback to carrier", legCarrier.MainAddress.PK, transport.JW_OA_CreditorAddress);

			partyRecord1.PR_Location = transport1.JW_RL_NKDiscPort;
			Factory.Save();

			AssertEquals("Preconditions: we set related party for last route set discharge port HKHKG", "HKHKG", partyRecord1.PR_Location);

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("We should use last discharge port of route set (HKHKG) - even transport is from AUSYD -> AUMEL", relatedParty1.MainAddress.PK, transport.JW_OA_CreditorAddress);

			var transport2 = consol.Transports.AddNew("HKHKG", "USLAX");
			transport2.JW_CarrierBookingReference = "2";

			AssertEquals("Preconditions: transport2 route set is 2", 2, transport2.RouteSetNumber);

			transport2.JW_OA_CreditorAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("This is new route set (HKHKG -> USLAX) and we only have related parties for HKHKG (first route set discharge port), should fallback to carrier", legCarrier.MainAddress.PK, transport2.JW_OA_CreditorAddress);

			partyRecord1.PR_Location = transport2.JW_RL_NKDiscPort;
			Factory.Save();

			transport2.JW_OA_CreditorAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = ZGuid.Empty;
			transport2.JW_OA_CarrierAddress = legCarrier.MainAddress.PK;

			AssertEquals("Preconditions: we set related party for last discharge of route set (USLAX)", "USLAX", partyRecord1.PR_Location);
			AssertEquals("We should use last discharge port of route set 2", relatedParty1.MainAddress.PK, transport2.JW_OA_CreditorAddress);
		}

		public void TestATA_Get_ShouldNotHasChanges()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel 1";

			var sailing = NewSailing(vessel, "f", "l", "d");
			sailing.Voyage.JV_OH_Line = carrier.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_JX = sailing.PK;
			transport.JW_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			var loadedTransport = new BusinessObjectFactory().Load<Transport>(transport.PK);
			loadedTransport.ParentType = typeof(CommonConsol);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("loadedTransport.HasChanges", loadedTransport.HasChanges, false);
				AssertEquals("loadedTransport.JW_OA_CreditorAddress", loadedTransport.JW_OA_CreditorAddress, ZGuid.Empty);
			});

			AssertEquals("loadedTransport.JW_ATA", ZDateTime.Empty, loadedTransport.JW_ATA);
			AssertEquals("loadedTransport.HasChanges: Getting JW_ATA should not enable BizObj.HasChanges", loadedTransport.HasChanges, false);
		}

		public void TestSetJW_JX_ShouldNotRedefaultRouteCreditor()
		{
			const string transportMode = Constants.TransportModes.Sea;
			const string containerMode = Constants.ContainerModes.FCL;
			const string location = "AUSYD";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			// The related party of type ServiceProviderCreditor should eventually be the route creditor when linking route with sailing.
			var parties = carrier.AllRelatedParties;
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;
			var partyRecord = parties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;
			partyRecord.PR_FreightTransportMode = transportMode;
			partyRecord.PR_FreightContainerMode = containerMode;
			partyRecord.PR_Location = location;

			var anotherCarrier = Factory.NewWithValidTestData<OrgHeader>();
			anotherCarrier.OH_IsCreditor = true;

			// Carriers and the related party must be saved for later references despite the fact that we use the same BizOFactory.
			Factory.Save();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "v";

			var sailing = NewSailing(vessel, "f", "l", "d");
			sailing.Voyage.JV_OH_Line = carrier.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = containerMode;
			var route = consol.Transports[0];
			route.JW_TransportMode = transportMode;
			route.JW_IsLinked = true;
			route.JW_RL_NKLoadPort = location;

			AssertEquals(ZGuid.Empty, route.JW_OA_CreditorAddress);

			route.JW_JX = sailing.PK;
			AssertEquals("When linking with a sailing, route carrier should be set from voyage's carrier", carrier.MainAddress.PK, route.JW_OA_CarrierAddress);
			AssertEquals("Route creditor should not default", ZGuid.Empty, route.JW_OA_CreditorAddress);

			route.JW_JX = ZGuid.Empty;
			AssertEquals("When remove the link to the sailing, route carrier should not be change", carrier.MainAddress.PK, route.JW_OA_CarrierAddress);
			AssertEquals("Route creditor should not default", ZGuid.Empty, route.JW_OA_CreditorAddress);

			var anotherSailing = NewSailing(vessel, "f", "l", "d");
			anotherSailing.Voyage.JV_OH_Line = anotherCarrier.PK;

			route.JW_JX = anotherSailing.PK;
			AssertEquals("When linking with a new sailing, route carrier should be set from new voyage's carrier", anotherCarrier.MainAddress.PK, route.JW_OA_CarrierAddress);
			AssertEquals("Route creditor should not default", ZGuid.Empty, route.JW_OA_CreditorAddress);
		}

		public void TestRedefaultCreditor_WhenLocationsAreMissing_ShouldNotSetCreditor()
		{
			var creditorAU = Factory.New<OrgHeader>();
			creditorAU.OH_Code = "CREDITORAU";
			creditorAU.OH_IsCreditor = true;

			var creditorSG = Factory.New<OrgHeader>();
			creditorSG.OH_Code = "CREDITORSG";
			creditorSG.OH_IsCreditor = true;

			var creditorHK = Factory.New<OrgHeader>();
			creditorHK.OH_Code = "CREDITORHK";
			creditorHK.OH_IsCreditor = true;

			var creditorDE = Factory.New<OrgHeader>();
			creditorDE.OH_Code = "CREDITORDE";
			creditorDE.OH_IsCreditor = true;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.SetRelatedParty(creditorAU, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "AU");
			carrier.SetRelatedParty(creditorSG, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "SG");
			carrier.SetRelatedParty(creditorHK, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "HK");
			carrier.SetRelatedParty(creditorDE, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "DEHAM");

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			// The setter below calls SetDefaultRoutingCreditorFromRoutingCarrier, aka RedefaultCreditor so do not need to call it here explicitly.
			transport1.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			AssertNull(
				"AUSYD => _ matches Prepaid SPC location. No discharge port, should not re-default creditor.",
				transport1.Creditor);

			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.RedefaultCreditor();
			AssertEquals(
				"AUSYD => SGSIN matches Prepaid SPC location, should re-default creditor field to AU creditor",
				creditorAU.OH_Code,
				transport1.Creditor.OH_Code);

			consol.JK_PrepaidCollect = PaymentType.Collect;

			transport1.RedefaultCreditor();
			AssertEquals(
				"AUSYD => SGSIN matches Collect SPC location. However, creditor field is not empty, should not change it",
				creditorAU.OH_Code,
				transport1.Creditor.OH_Code);

			transport1.JW_OA_CreditorAddress = ZGuid.Empty;
			transport1.RedefaultCreditor();
			AssertEquals(
				"AUSYD => SGSIN matches Collect SPC location. Creditor field is empty, should re-default it to SG creditor",
				creditorSG.OH_Code,
				transport1.Creditor.OH_Code);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			transport1.RedefaultCreditor();
			AssertEquals(
				"AUSYD => HKHKG matches another Collect SPC location. However, creditor field is not empty, should not change it",
				creditorSG.OH_Code,
				transport1.Creditor.OH_Code);

			transport1.JW_OA_CreditorAddress = ZGuid.Empty;
			transport1.RedefaultCreditor();
			AssertEquals(
				"AUSYD => HKHKG matches another Collect SPC location. Creditor field is empty, should re-default it to HK creditor",
				creditorHK.OH_Code,
				transport1.Creditor.OH_Code);

			var routingCollection = ((IRoutingSupport)consol).TransportsIncludingRelated;
			AssertEquals("Precondition: 1 transport should also be a route set", 1, routingCollection.RouteSets.Count);

			var transport2 = consol.Transports.AddNew("HKHKG", "");
			AssertEquals("Precondition: 1 transport should also be a route set", 0, routingCollection.RouteSets.Count);

			transport1.RedefaultCreditor();
			AssertEquals(
				"(AUSYD => HKHKG, HKHKG => _) don't create a valid route set. Creditor should not be changed.",
				creditorHK.OH_Code,
				transport1.Creditor.OH_Code);

			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport1.RedefaultCreditor();
			AssertEquals(
				"(AUSYD => HKHKG, HKHKG => SGSIN) make a valid route set and match collect SPC location. However, creditor field is not empty, should not change it.",
				creditorHK.OH_Code,
				transport1.Creditor.OH_Code);

			transport1.JW_OA_CreditorAddress = ZGuid.Empty;
			transport1.RedefaultCreditor();
			AssertEquals(
				"(AUSYD => HKHKG, HKHKG => SGSIN) make a valid route set and match collect SPC location. Creditor field is empty, should re-default it to SG creditor.",
				creditorSG.OH_Code,
				transport1.Creditor.OH_Code);

			transport2.JW_RL_NKDiscPort = "DEHAM";
			transport1.RedefaultCreditor(shouldOverrideExistingCreditor: true);
			AssertEquals(
				"When we update from sailing schedule, RedefaultCreditor should still be able to force updating the creditor even when the field is not empty",
				creditorDE.OH_Code,
				transport1.Creditor.OH_Code);
		}

		public void TestRedefaultCreditor_WhenConsolPaymentTypeIsMissing_ShouldGetLocationFromDirection()
		{
			var creditorAU = Factory.New<OrgHeader>();
			creditorAU.OH_Code = "CREDITORAU";
			creditorAU.OH_IsCreditor = true;

			var creditorSGSIN = Factory.New<OrgHeader>();
			creditorSGSIN.OH_Code = "CREDITORSG";
			creditorSGSIN.OH_IsCreditor = true;

			var creditorDEHAM = Factory.New<OrgHeader>();
			creditorDEHAM.OH_Code = "CREDITORDE";
			creditorDEHAM.OH_IsCreditor = true;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.SetRelatedParty(creditorAU, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "AU");
			carrier.SetRelatedParty(creditorSGSIN, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "SGSIN");
			carrier.SetRelatedParty(creditorDEHAM, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.Sea, ContainerModes.FCL, "DEHAM");

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";

			// Precondition - Consol payment type is missing while trying to re-default creditor
			consol.JK_PrepaidCollect = ZString.Empty;

			var transport = consol.Transports[0];
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = ZString.Empty;
			transport.RedefaultCreditor();
			AssertEquals(
				"AUSYD (same as the home port AU) => _: When routing discharge port is missing, should not re-default creditor",
				ZGuid.Empty,
				transport.JW_OA_CreditorAddress);

			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.RedefaultCreditor();
			AssertEquals(
				"_ => SGSIN: When routing load port is missing, should not re-default creditor",
				ZGuid.Empty,
				transport.JW_OA_CreditorAddress);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.RedefaultCreditor();
			AssertEquals(
				"AUSYD (same as the home port AU) => SGSIN => Export: should re-default creditor to AU (origin) creditor",
				creditorAU.OH_Code,
				transport.Creditor.OH_Code);

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.RedefaultCreditor();
			AssertEquals(
				"AUSYD => AUBNE => Domestic: should re-default creditor to AU (origin) creditor",
				creditorAU.OH_Code,
				transport.Creditor.OH_Code);

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_RL_NKLoadPort = "DEHAM";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.RedefaultCreditor();
			AssertEquals(
				"DEHAM => SGSIN => Cross Trade: should re-default creditor to SG (destination) creditor",
				creditorSGSIN.OH_Code,
				transport.Creditor.OH_Code);

			transport.JW_OA_CreditorAddress = ZGuid.Empty;
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.RedefaultCreditor();
			AssertEquals(
				"DEHAM => AUSYD => Import: should re-default creditor to AU (destination) creditor",
				creditorAU.OH_Code,
				transport.Creditor.OH_Code);
		}

		#endregion

		public void TestSetDefaultValues()
		{
			FreightDataRegistry.Instance.DefaultRoutingLegStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.TransportStatus.Confirmed);
			fTransport = null;
			AssertEquals(Constants.TransportStatus.Confirmed, Transport.JW_Status);

			FreightDataRegistry.Instance.DefaultRoutingLegStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.TransportStatus.Planned);
			fTransport = null;
			AssertEquals(Constants.TransportStatus.Planned, Transport.JW_Status);
		}

		public void TestLoadAndDischargeSyncForStorage()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Storage;
			AssertEquals("precondition: JW_RL_NKLoadPort", "", Transport.JW_RL_NKLoadPort);
			AssertEquals("precondition: JW_RL_NKDiscPort", "", Transport.JW_RL_NKDiscPort);

			Transport.JW_RL_NKLoadPort = HomePort;
			AssertEquals("JW_RL_NKLoadPort", HomePort, Transport.JW_RL_NKLoadPort);
			AssertEquals("JW_RL_NKDiscPort", HomePort, Transport.JW_RL_NKDiscPort);

			Transport.JW_RL_NKDiscPort = OverseasPort;
			AssertEquals("JW_RL_NKLoadPort", OverseasPort, Transport.JW_RL_NKLoadPort);
			AssertEquals("JW_RL_NKDiscPort", OverseasPort, Transport.JW_RL_NKDiscPort);
		}

		public void TestLoadAndDischargeDontSyncForNonStorage()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("precondition: JW_RL_NKLoadPort", "", Transport.JW_RL_NKLoadPort);
			AssertEquals("precondition: JW_RL_NKDiscPort", "", Transport.JW_RL_NKDiscPort);

			Transport.JW_RL_NKLoadPort = HomePort;
			AssertEquals("JW_RL_NKLoadPort", HomePort, Transport.JW_RL_NKLoadPort);
			AssertEquals("JW_RL_NKDiscPort", "", Transport.JW_RL_NKDiscPort);

			Transport.JW_RL_NKDiscPort = OverseasPort;
			AssertEquals("JW_RL_NKLoadPort", HomePort, Transport.JW_RL_NKLoadPort);
			AssertEquals("JW_RL_NKDiscPort", OverseasPort, Transport.JW_RL_NKDiscPort);
		}

		public void TestStorageTransportMode()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_IsCharter = true;
			Transport.JW_Vessel = "XXX";
			Transport.JW_VoyageFlight = "YYY";
			Transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			Transport.JW_Status = Constants.TransportStatus.Confirmed;
			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.JW_AircraftType = "E90";

			Transport.JW_TransportMode = Constants.TransportModes.Storage;
			AssertEquals("JW_IsLinked should be false", false, Transport.JW_IsLinked);
			AssertEquals("JW_IsLinked should be readonly", true, Transport.JW_IsLinkedInfo.ReadOnly);

			AssertEquals("JW_IsCharter should be false", false, Transport.JW_IsCharter);
			AssertEquals("JW_IsCharter should be readonly", true, Transport.JW_IsCharterInfo.ReadOnly);

			AssertEquals("JW_AircraftType should be Empty", ZString.Empty, Transport.JW_AircraftType);
			AssertEquals("JW_AircraftType should be readonly", true, Transport.JW_AircraftTypeInfo.ReadOnly);

			AssertEquals("JW_TransportType should be Empty", "", Transport.JW_TransportType);
			AssertEquals("JW_TransportType should be readonly", true, Transport.JW_TransportTypeInfo.ReadOnly);

			AssertEquals("JW_Vessel should be Empty", "", Transport.JW_Vessel);
			AssertEquals("JW_Vessel should be readonly", true, Transport.JW_VesselInfo.ReadOnly);

			AssertEquals("JW_VoyageFlight should be Empty", "", Transport.JW_VoyageFlight);
			AssertEquals("JW_VoyageFlight should be readonly", true, Transport.JW_VoyageFlightInfo.ReadOnly);

			AssertEquals("JW_Status should be empty", "", Transport.JW_Status);
			AssertEquals("JW_Status should be readonly", true, Transport.JW_StatusInfo.ReadOnly);

			AssertEquals("JW_RL_NKLoadPort should be unchanged", HomePort, Transport.JW_RL_NKLoadPort);
			AssertEquals("JW_RL_NKDiscPort should be the same as the load port", HomePort, Transport.JW_RL_NKDiscPort);

			Transport.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("JW_IsLinked should not be readonly", false, Transport.JW_IsLinkedInfo.ReadOnly);
			AssertEquals("JW_IsCharter should not be readonly", false, Transport.JW_IsCharterInfo.ReadOnly);
			AssertEquals("JW_AircraftType should be readonly", false, Transport.JW_AircraftTypeInfo.ReadOnly);
			AssertEquals("JW_TransportType should not be readonly", false, Transport.JW_TransportTypeInfo.ReadOnly);
			AssertEquals("JW_Vessel should not be readonly", false, Transport.JW_VesselInfo.ReadOnly);
			AssertEquals("JW_VoyageFlight should not be readonly", false, Transport.JW_VoyageFlightInfo.ReadOnly);
			AssertEquals("JW_Status should not be readonly", false, Transport.JW_StatusInfo.ReadOnly);
		}

		public void TestInlandWaterwayTransportMode()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_IsCharter = true;
			Transport.JW_Vessel = "XXX";
			Transport.JW_VoyageFlight = "YYY";
			Transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			Transport.JW_Status = Constants.TransportStatus.Confirmed;
			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.JW_AircraftType = "E90";

			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("JW_IsLinked should be false", false, Transport.JW_IsLinked);
		}

		public void TestJW_ParentBillOfLading()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_MasterBillNum = "aoeu";
			AssertEquals("aoeu", consol.Transports[0].JW_ParentBillOfLading);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "SNTH";
			AssertEquals("SNTH", shipment.Transports.AddNew().JW_ParentBillOfLading);
		}

		public void TestJW_ParentConsignmentRef()
		{
			ZString consolRefNumber = "C00001007";
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = consolRefNumber;
			AssertEquals(consolRefNumber, consol.Transports[0].JW_ParentConsignmentRef);

			ZString shipmentRefNumber = "S00001017";
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = shipmentRefNumber;
			AssertEquals(shipmentRefNumber, shipment.Transports.AddNew().JW_ParentConsignmentRef);
		}

		public void TestJW_ParentContainerMode()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			AssertEquals(Constants.ContainerModes.Bulk, consol.Transports[0].JW_ParentContainerMode);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.Liquid;
			AssertEquals(Constants.ContainerModes.Liquid, shipment.Transports.AddNew().JW_ParentContainerMode);
		}

		public void TestJW_DistanceUnit()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			DistanceCalculationRegistry.Instance.DefaultDistanceUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Miles);
			Transport transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.Length.Miles, transport1.JW_DistanceUnit);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("", transport2.JW_DistanceUnit);

			DistanceCalculationRegistry.Instance.DefaultDistanceUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Kilometres);
			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.Length.Kilometres, transport3.JW_DistanceUnit);
		}

		public void TestDefaultTransportCreditorAddressToAPAddress()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR";
			creditor.OH_IsCreditor = true;

			OrgAddress officeAddress = creditor.Addresses.AddNew(OrgAddressType.Office, true);
			officeAddress.OA_Address1 = "Office Address1";

			transport.CreditorPK = ZGuid.Empty;
			transport.CreditorPK = creditor.PK;

			AssertEquals(officeAddress.PK, transport.JW_OA_CreditorAddress);
			AssertEquals(officeAddress.OA_Address1, transport.CreditorAddress.OA_Address1);

			OrgAddress postalAddress = creditor.Addresses.AddNew(OrgAddressType.Payables, true);
			postalAddress.OA_Address1 = "Postal Adress1";

			Factory.Save();

			transport.CreditorPK = ZGuid.Empty;
			transport.CreditorPK = creditor.PK;

			AssertEquals(postalAddress.PK, transport.JW_OA_CreditorAddress);
			AssertEquals(postalAddress.OA_Address1, transport.CreditorAddress.OA_Address1);

			OrgAddress payablesAddress = creditor.Addresses.AddNew(OrgAddressType.Payables, true);
			payablesAddress.OA_Address1 = "Payables Adress1";

			Factory.Save();

			transport.CreditorPK = ZGuid.Empty;
			transport.CreditorPK = creditor.PK;

			AssertEquals(payablesAddress.PK, transport.JW_OA_CreditorAddress);
			AssertEquals(payablesAddress.OA_Address1, transport.CreditorAddress.OA_Address1);
		}

		public void TestDefaultOfficeAddressTypes()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];

			AssertEquals("It will always default to Office Address", AddressType.OFC, transport.JW_OA_CarrierAddress_ZAddress.DefaultAddressType);
			AssertEquals("It will always default to Office Address", AddressType.APM, transport.JW_OA_CreditorAddress_ZAddress.DefaultAddressType);
		}

		public void TestUpdateProxiedFieldsWhenChangingSailingOnLinkedTransport()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "v";

			var firstSailing = NewSailing(vessel, "f", "l", "d");
			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = firstSailing.PK;

			Factory.Save();
			AssertEquals("Precondition: transport is linked to the sailing", firstSailing, transport.Sailing);

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel2.RV_FK;
			voyage.JV_VoyageFlight = "Flight";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.GenerateSailings();

			voyage.JV_IsChartered = true;
			voyage.JV_IsCargoOnly = true;
			voyage.JV_AircraftType = "E90";

			var newSailing = voyage.Sailings[0];

			var today = ZDateTime.Today;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = orgHeader.Addresses.AddNew();
			arrivalAddress.Address1 = "arrival";
			var departureAddress = orgHeader.Addresses.AddNew();
			departureAddress.Address1 = "destination";

			newSailing.JX_DepotReceivalCommences = today;
			newSailing.JX_DepotCutOff = today.AddDays(1);
			newSailing.JX_DepotAvailabilityDate = today.AddDays(2);
			newSailing.JX_DepotStorageDate = today.AddDays(3);
			newSailing.JX_ServiceString = "gstring";

			var origin = newSailing.Origin;
			origin.JA_OA_DepartureCTOAddress = departureAddress.PK;
			origin.JA_S_DEP = today.AddDays(4);
			origin.JA_E_DEP = today.AddDays(5);
			origin.JA_A_DEP = today.AddDays(6);
			origin.JA_ReceivalCommences = today.AddDays(7);
			origin.JA_CutOff = today.AddDays(8);
			origin.JA_DocumentaryCutoff = today.AddDays(9);
			origin.JA_VGMCutOff = today.AddDays(10);
			origin.JA_EmptyReceivalCommences = today.AddDays(11);
			origin.JA_EmptyCutOff = today.AddDays(12);
			origin.JA_ReeferReceivalCommences = today.AddDays(13);
			origin.JA_ReeferCutOff = today.AddDays(14);
			origin.JA_DGReceivalCommences = today.AddDays(15);
			origin.JA_DGCutOff = today.AddDays(16);

			var destination = newSailing.Destination;
			destination.JB_OA_ArrivalCTOAddress = arrivalAddress.PK;
			destination.JB_S_ARV = today.AddDays(11);
			destination.JB_E_ARV = today.AddDays(12);
			destination.JB_A_ARV = today.AddDays(13);
			destination.JB_AvailabilityDate = today.AddDays(14);
			destination.JB_StorageDate = today.AddDays(15);

			Factory.Save();

			transport.JW_JX = newSailing.PK;
			Factory.Save();

			AssertEquals("Precondition: transport is linked to the sailing", newSailing, transport.Sailing);

			var reloadedTransport = new BusinessObjectFactory().Load<Transport>(transport.PK);
			AssertEquals("Reloaded transport is linked to the sailing", newSailing.PK, reloadedTransport.Sailing.PK);

			var proxiedTransportFieldsToSailingFieldsDict = new Dictionary<ZPropertyInfo, ZString>()
			{
				{ reloadedTransport.JW_DepotReceivalCommencesInfo, JobSailingSchema.Constants.JX_DepotReceivalCommences },
				{ reloadedTransport.JW_DepotCutOffInfo, JobSailingSchema.Constants.JX_DepotCutOff },
				{ reloadedTransport.JW_DepotAvailabilityDateInfo, JobSailingSchema.Constants.JX_DepotAvailabilityDate },
				{ reloadedTransport.JW_DepotStorageDateInfo, JobSailingSchema.Constants.JX_DepotStorageDate },
				{ reloadedTransport.JW_ServiceStringInfo, JobSailingSchema.Constants.JX_ServiceString },
				{ reloadedTransport.JW_ArrivalPortRouteIdInfo, JobSailingSchema.Constants.JX_ArrivalPortRouteId },
				{ reloadedTransport.JW_DeparturePortRouteIdInfo, JobSailingSchema.Constants.JX_DeparturePortRouteId }
			};

			proxiedTransportFieldsToSailingFieldsDict.ForEach(pair => AssertEquals(pair.Value + " should propogate to linked transport on sailing change.", newSailing[pair.Value], pair.Key.Value));

			var proxiedTransportFieldsToVoyageFieldsDict = new Dictionary<ZPropertyInfo, ZString>()
			{
				{ reloadedTransport.JW_VesselInfo, JobVoyageSchema.Constants.JV_RV_NKVessel },
				{ reloadedTransport.JW_VoyageFlightInfo, JobVoyageSchema.Constants.JV_VoyageFlight },
				{ reloadedTransport.JW_IsCharterInfo, JobVoyageSchema.Constants.JV_IsChartered },
				{ reloadedTransport.JW_IsCargoOnlyInfo, JobVoyageSchema.Constants.JV_IsCargoOnly },
				{ reloadedTransport.JW_AircraftTypeInfo, JobVoyageSchema.Constants.JV_AircraftType }
			};

			proxiedTransportFieldsToVoyageFieldsDict.ForEach(pair => AssertEquals(pair.Value + " should propogate to linked transport on sailing change.", voyage[pair.Value], pair.Key.Value));

			var proxiedTransportFieldsToOriginFieldsDict = new Dictionary<ZPropertyInfo, ZString>()
			{
				{ reloadedTransport.JW_OA_DepartureLocationInfo, JobVoyOriginSchema.Constants.JA_OA_DepartureCTOAddress },
				{ reloadedTransport.JW_RL_NKLoadPortInfo, JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading },
				{ reloadedTransport.JW_STDInfo, JobVoyOriginSchema.Constants.JA_S_DEP },
				{ reloadedTransport.JW_ETDInfo, JobVoyOriginSchema.Constants.JA_E_DEP },
				{ reloadedTransport.JW_ATDInfo, JobVoyOriginSchema.Constants.JA_A_DEP },
				{ reloadedTransport.JW_TerminalReceivalCommencesInfo, JobVoyOriginSchema.Constants.JA_ReceivalCommences },
				{ reloadedTransport.JW_TerminalCutOffInfo, JobVoyOriginSchema.Constants.JA_CutOff },
				{ reloadedTransport.JW_DocumentaryCutOffInfo, JobVoyOriginSchema.Constants.JA_DocumentaryCutoff },
				{ reloadedTransport.JW_VGMCutOffInfo, JobVoyOriginSchema.Constants.JA_VGMCutOff },
				{ reloadedTransport.JW_EmptyReceivalCommencesInfo, JobVoyOriginSchema.Constants.JA_EmptyReceivalCommences },
				{ reloadedTransport.JW_EmptyCutOffInfo, JobVoyOriginSchema.Constants.JA_EmptyCutOff },
				{ reloadedTransport.JW_ReeferReceivalCommencesInfo, JobVoyOriginSchema.Constants.JA_ReeferReceivalCommences },
				{ reloadedTransport.JW_ReeferCutOffInfo, JobVoyOriginSchema.Constants.JA_ReeferCutOff },
				{ reloadedTransport.JW_DGReceivalCommencesInfo, JobVoyOriginSchema.Constants.JA_DGReceivalCommences },
				{ reloadedTransport.JW_DGCutOffInfo, JobVoyOriginSchema.Constants.JA_DGCutOff }
			};

			proxiedTransportFieldsToOriginFieldsDict.ForEach(pair => AssertEquals(pair.Value + " should propogate to linked transport on sailing change.", origin[pair.Value], pair.Key.OriginalValue));

			var proxiedTransportFieldsToDestFieldsDict = new Dictionary<ZPropertyInfo, ZString>()
			{
				{ reloadedTransport.JW_OA_ArrivalLocationInfo, JobVoyDestinationSchema.Constants.JB_OA_ArrivalCTOAddress },
				{ reloadedTransport.JW_RL_NKDiscPortInfo, JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge },
				{ reloadedTransport.JW_STAInfo, JobVoyDestinationSchema.Constants.JB_S_ARV },
				{ reloadedTransport.JW_ETAInfo, JobVoyDestinationSchema.Constants.JB_E_ARV },
				{ reloadedTransport.JW_ATAInfo, JobVoyDestinationSchema.Constants.JB_A_ARV },
				{ reloadedTransport.JW_TerminalStorageDateInfo, JobVoyDestinationSchema.Constants.JB_StorageDate },
				{ reloadedTransport.JW_TerminalAvailabilityDateInfo, JobVoyDestinationSchema.Constants.JB_AvailabilityDate }
			};

			proxiedTransportFieldsToDestFieldsDict.ForEach(pair => AssertEquals(pair.Value + " should propogate to linked transport on sailing change.", destination[pair.Value], pair.Key.OriginalValue));
		}

		public void TestDateErrorGetBestMatchingExistingVoyage()
		{
			var possibleVoyage = Factory.New<JobVoyage>();
			possibleVoyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			possibleVoyage.JV_VoyageFlight = "Flight";
			possibleVoyage.JV_FlightDate = new ZDateTime(2019, 12, 12);
			possibleVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			possibleVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			possibleVoyage.JV_AircraftType = "E90";
			Factory.Save();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "Flight";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.JV_AircraftType = "E90";
			voyage.GenerateSailings();

			var newSailing = voyage.Sailings[0];
			newSailing.Origin.JA_E_DEP = ZDateTime.Empty;
			newSailing.Destination.JB_E_ARV = ZDateTime.Empty;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var transport = shipment.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = newSailing.PK;
			AssertNoExceptionThrown(transport.RunPreSaveValidation);
		}

		public void TestUpdateProxiedFieldsOnPreSaveValidationWhenChangingSailingOnLinkedTransport()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "v";

			var firstSailing = NewSailing(vessel, "f", "l", "d");
			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = firstSailing.PK;
			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);

			Factory.Save();
			AssertEquals("Precondition: transport is linked to the sailing", firstSailing, transport.Sailing);

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Flight";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.GenerateSailings();

			voyage.JV_IsChartered = true;
			voyage.JV_IsCargoOnly = true;
			voyage.JV_AircraftType = "E90";

			var newSailing = voyage.Sailings[0];

			var today = ZDateTime.Today;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = orgHeader.Addresses.AddNew();
			arrivalAddress.Address1 = "arrival";
			var departureAddress = orgHeader.Addresses.AddNew();
			departureAddress.Address1 = "destination";

			newSailing.JX_DepotReceivalCommences = today;
			newSailing.JX_DepotCutOff = today.AddDays(1);
			newSailing.JX_DepotAvailabilityDate = today.AddDays(2);
			newSailing.JX_DepotStorageDate = today.AddDays(3);
			newSailing.JX_ServiceString = "myServiceString";
			newSailing.JX_ArrivalPortRouteId = "arrivalId";
			newSailing.JX_DeparturePortRouteId = "departureId";

			var origin = newSailing.Origin;
			origin.JA_OA_DepartureCTOAddress = departureAddress.PK;
			origin.JA_S_DEP = today.AddDays(4);
			origin.JA_E_DEP = today.AddDays(5);
			origin.JA_A_DEP = today.AddDays(6);
			origin.JA_ReceivalCommences = today.AddDays(7);
			origin.JA_CutOff = today.AddDays(8);
			origin.JA_DocumentaryCutoff = today.AddDays(9);
			origin.JA_VGMCutOff = today.AddDays(10);
			origin.JA_EmptyReceivalCommences = today.AddDays(9);
			origin.JA_EmptyCutOff = today.AddDays(10);
			origin.JA_DGReceivalCommences = today.AddDays(9);
			origin.JA_DGCutOff = today.AddDays(10);
			origin.JA_ReeferReceivalCommences = today.AddDays(9);
			origin.JA_ReeferCutOff = today.AddDays(10);

			var destination = newSailing.Destination;
			destination.JB_OA_ArrivalCTOAddress = arrivalAddress.PK;
			destination.JB_S_ARV = today.AddDays(11);
			destination.JB_E_ARV = today.AddDays(12);
			destination.JB_A_ARV = today.AddDays(13);
			destination.JB_AvailabilityDate = today.AddDays(14);
			destination.JB_StorageDate = today.AddDays(15);

			Factory.Save();

			transport.JW_JX = newSailing.PK;
			transport.RunPreSaveValidation();
			AssertEquals("Precondition: transport is linked to the sailing", newSailing, transport.Sailing);
			AssertEquals("Depot Receival Commences should propogate to linked transport on sailing change.", newSailing.JX_DepotReceivalCommences, transport.JW_DepotReceivalCommences);
			AssertEquals("Depot Cut Off should propogate to linked transport on sailing change.", newSailing.JX_DepotCutOff, transport.JW_DepotCutOff);
			AssertEquals("Depot Availability Date should propogate to linked transport on sailing change.", newSailing.JX_DepotAvailabilityDate, transport.JW_DepotAvailabilityDate);
			AssertEquals("Depot Storage Date should propogate to linked transport on sailing change.", newSailing.JX_DepotStorageDate, transport.JW_DepotStorageDate);
			AssertEquals("Service string should propagate.", newSailing.JX_ServiceString, transport.JW_ServiceString);
			AssertEquals("Arrival Port Route Id should propagate.", newSailing.JX_ArrivalPortRouteId, transport.JW_ArrivalPortRouteId);
			AssertEquals("Departure Port Route Id should propagate.", newSailing.JX_DeparturePortRouteId, transport.JW_DeparturePortRouteId);

			AssertEquals("Vessel should propogate to linked transport on sailing change.", voyage.JV_RV_NKVessel, transport.JW_Vessel);
			AssertEquals("Voyage should propogate to linked transport on sailing change.", voyage.JV_VoyageFlight, transport.JW_VoyageFlight);
			AssertEquals("Charter should propogate to linked transport on sailing change.", voyage.JV_IsChartered, transport.JW_IsCharter);
			AssertEquals("CargoOnly should propogate to linked transport on sailing change.", voyage.JV_IsCargoOnly, transport.JW_IsCargoOnly);
			AssertEquals("Aircraft Type should propogate to linked transport on sailing change.", voyage.JV_AircraftType, transport.JW_AircraftType);

			AssertEquals("DepartureCTO should propogate to linked transport on sailing change.", origin.JA_OA_DepartureCTOAddress, transport.JW_OA_DepartureLocation);
			AssertEquals("Load Port should propogate to linked transport on sailing change.", origin.JA_RL_NKPortOfLoading, transport.JW_RL_NKLoadPort);
			AssertEquals("STD should propogate to linked transport on sailing change.", origin.JA_S_DEP, transport.JW_STD);
			AssertEquals("ETD should propogate to linked transport on sailing change.", origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals("ATD should propogate to linked transport on sailing change.", origin.JA_A_DEP, transport.JW_ATD);
			AssertEquals("Terminal Receival Commences should propogate to linked transport on sailing change.", origin.JA_ReceivalCommences, transport.JW_TerminalReceivalCommences);
			AssertEquals("Terminal Cut Off should propogate to linked transport on sailing change.", origin.JA_CutOff, transport.JW_TerminalCutOff);
			AssertEquals("Documentary Cut Off should propogate to linked transport on sailing change.", origin.JA_DocumentaryCutoff, transport.JW_DocumentaryCutOff);
			AssertEquals("VGM Cut Off should propogate to linked transport on sailing change.", origin.JA_VGMCutOff, transport.JW_VGMCutOff);
			AssertEquals("Empty Receival Commences should propogate to linked transport on sailing change.", origin.JA_EmptyReceivalCommences, transport.JW_EmptyReceivalCommences);
			AssertEquals("Empty Cut Off should propogate to linked transport on sailing change.", origin.JA_EmptyCutOff, transport.JW_EmptyCutOff);
			AssertEquals("DG Receival Commences should propogate to linked transport on sailing change.", origin.JA_DGReceivalCommences, transport.JW_DGReceivalCommences);
			AssertEquals("DG Cut Off should propogate to linked transport on sailing change.", origin.JA_DGCutOff, transport.JW_DGCutOff);
			AssertEquals("Reefer Receival Commences should propogate to linked transport on sailing change.", origin.JA_ReeferReceivalCommences, transport.JW_ReeferReceivalCommences);
			AssertEquals("Reefer Cut Off should propogate to linked transport on sailing change.", origin.JA_ReeferCutOff, transport.JW_ReeferCutOff);

			AssertEquals("ArrivalCTO should propogate to linked transport on sailing change.", destination.JB_OA_ArrivalCTOAddress, transport.JW_OA_ArrivalLocation);
			AssertEquals("DiscPort should propogate to linked transport on sailing change.", destination.JB_RL_NKPortOfDischarge, transport.JW_RL_NKDiscPort);
			AssertEquals("STA should propogate to linked transport on sailing change.", destination.JB_S_ARV, transport.JW_STA);
			AssertEquals("ETA should propogate to linked transport on sailing change.", destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals("ATA should propogate to linked transport on sailing change.", destination.JB_A_ARV, transport.JW_ATA);
			AssertEquals("Terminal Availability Date should propogate to linked transport on sailing change.", destination.JB_AvailabilityDate, transport.JW_TerminalAvailabilityDate);
			AssertEquals("Terminal Storage Date should propogate to linked transport on sailing change.", destination.JB_StorageDate, transport.JW_TerminalStorageDate);
		}

		public void TestUpdateProxiedFieldsOnRelinkTransportToSameSchedule()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			var sailing = NewSailing(vessel, "V01", "AUSYD", "ZAJNB");
			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();
			AssertEquals("Precondition: transport is linked to the sailing", sailing, transport.Sailing);

			transport.JW_IsLinked = false;

			AssertNull("Transport is unlinked", transport.Sailing);

			transport.JW_IsLinked = true;

			Factory.Save();

			AssertEquals("Transport is still linked to the sailing", sailing, transport.Sailing);

			var factory2 = new BusinessObjectFactory();
			var row = ((INeedRow)factory2.Load<Transport>(transport.PK)).Row;

			AssertEquals("Vessel", row[Transport.Schema.JW_Vessel]);
			AssertEquals("V01", row[Transport.Schema.JW_VoyageFlight]);
			AssertEquals("AUSYD", row[Transport.Schema.JW_RL_NKLoadPort]);
			AssertEquals("ZAJNB", row[Transport.Schema.JW_RL_NKDiscPort]);
		}

		public void TestUpdateProxiedFieldsOnPreSaveValidation()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			var sailing = NewSailing(vessel, "V01", "AUSYD", "ZAJNB");
			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();
			AssertEquals("Precondition: transport is linked to the sailing", sailing, transport.Sailing);

			transport.JW_RL_NKDiscPort = "ZACPT";
			transport.RunPreSaveValidation();
			transport.JW_RL_NKDiscPort = "ZAJNB";

			Factory.Save();

			AssertEquals("Transport is still linked to the sailing", sailing, transport.Sailing);

			var factory2 = new BusinessObjectFactory();
			var row = ((INeedRow)factory2.Load<Transport>(transport.PK)).Row;

			AssertEquals("Vessel", row[Transport.Schema.JW_Vessel]);
			AssertEquals("V01", row[Transport.Schema.JW_VoyageFlight]);
			AssertEquals("AUSYD", row[Transport.Schema.JW_RL_NKLoadPort]);
			AssertEquals("ZAJNB", row[Transport.Schema.JW_RL_NKDiscPort]);
		}

		public void TestProxiedFieldsUpdatedByTrigger()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Vessel1";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Vessel2";

			var sailing1 = NewSailing(vessel1, "V01", "AUSYD", "ZAJNB");
			var sailing2 = NewSailing(vessel2, "V02", "AUSYD", "ZACPT");

			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing1.PK;

			Factory.Save();

			transport.JW_JX = sailing2.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var row = ((INeedRow)factory2.Load<Transport>(transport.PK)).Row;

			AssertEquals("Vessel2", row[Transport.Schema.JW_Vessel]);
			AssertEquals("V02", row[Transport.Schema.JW_VoyageFlight]);
			AssertEquals("AUSYD", row[Transport.Schema.JW_RL_NKLoadPort]);
			AssertEquals("ZACPT", row[Transport.Schema.JW_RL_NKDiscPort]);
		}

		public void TestFieldsWithSuppression()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				ZDateTime etd = ZDateTime.Now.AddDays(1);
				ZDateTime eta = ZDateTime.Now.AddDays(2);
				ZDateTime atd = ZDateTime.Now.AddDays(1);
				ZDateTime ata = ZDateTime.Now.AddDays(2);

				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_MasterBillNum = "BLAB";

				Transport transport = consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport.JW_VoyageFlight = "123456";
				transport.JW_ETD = etd;
				transport.JW_ETA = eta;
				transport.JW_ATD = atd;
				transport.JW_ATA = ata;

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
				AssertEquals("123456", transport.VoyageFlightWithSuppression);
				AssertEquals(etd, transport.ETDWithSuppression);
				AssertEquals(eta, transport.ETAWithSuppression);
				AssertEquals(atd, transport.ATDWithSuppression);
				AssertEquals(ata, transport.ATAWithSuppression);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "123456";

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
				AssertEquals("123456", transport.VoyageFlightWithSuppression);
				AssertEquals(etd, transport.ETDWithSuppression);
				AssertEquals(eta, transport.ETAWithSuppression);
				AssertEquals(atd, transport.ATDWithSuppression);
				AssertEquals(ata, transport.ATAWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals(Suppression.SuppressedString, transport.BillOfLadingWithSuppression);
				AssertEquals(Suppression.SuppressedString, transport.VoyageFlightWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ETDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ETAWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ATDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ATAWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, false);

				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_HouseBill = "BLAB";

				transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport.JW_VoyageFlight = "123456";
				transport.JW_ETD = etd;
				transport.JW_ETA = eta;
				transport.JW_ATD = atd;
				transport.JW_ATA = ata;

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
				AssertEquals("123456", transport.VoyageFlightWithSuppression);
				AssertEquals(etd, transport.ETDWithSuppression);
				AssertEquals(eta, transport.ETAWithSuppression);
				AssertEquals(atd, transport.ATDWithSuppression);
				AssertEquals(ata, transport.ATAWithSuppression);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "123456";

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
				AssertEquals("123456", transport.VoyageFlightWithSuppression);
				AssertEquals(etd, transport.ETDWithSuppression);
				AssertEquals(eta, transport.ETAWithSuppression);
				AssertEquals(atd, transport.ATDWithSuppression);
				AssertEquals(ata, transport.ATAWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

				AssertEquals("BLAB", transport.JW_ParentBillOfLading);
				AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
				AssertEquals(Suppression.SuppressedString, transport.VoyageFlightWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ETDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ETAWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ATDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, transport.ATAWithSuppression);
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		public void TestFieldsWithSuppression_NotWeb()
		{
			var etd = ZDateTime.Now.AddDays(1);
			var eta = ZDateTime.Now.AddDays(2);
			var atd = ZDateTime.Now.AddDays(1);
			var ata = ZDateTime.Now.AddDays(2);

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "BLAB";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_VoyageFlight = "123456";
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			transport.JW_ATD = atd;
			transport.JW_ATA = ata;

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "123456";

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, false);
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_HouseBill = "BLAB";

			transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_VoyageFlight = "123456";
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			transport.JW_ATD = atd;
			transport.JW_ATA = ata;

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "123456";

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);

			AssertEquals("BLAB", transport.JW_ParentBillOfLading);
			AssertEquals("BLAB", transport.BillOfLadingWithSuppression);
			AssertEquals("123456", transport.VoyageFlightWithSuppression);
			AssertEquals(etd, transport.ETDWithSuppression);
			AssertEquals(eta, transport.ETAWithSuppression);
			AssertEquals(atd, transport.ATDWithSuppression);
			AssertEquals(ata, transport.ATAWithSuppression);
		}

		#region JW_Calc_Status

		public void TestJW_Calc_StatusWithNeutralUTCTime_DepartureDatesOnly()
		{
			var transport = Factory.New<CommonConsol>().Transports[0];
			var now = ZDateTime.UtcNow;
			var transit = "In Transit";
			var pending = "Pending";

			transport.JW_ETD = now.AddDays(-1);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ATD = now.AddMinutes(-30);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddMinutes(30);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-1);
			transport.JW_ATD = now.AddMinutes(-30);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(1);
			transport.JW_ATD = now.AddMinutes(-30);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-1);
			transport.JW_ATD = now.AddDays(2);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddMinutes(30);
			transport.JW_ATD = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);
		}

		public void TestJW_Calc_StatusWithNeutralUTCTime_ArrivalDatesOnly()
		{
			var transport = Factory.New<CommonConsol>().Transports[0];
			var now = ZDateTime.UtcNow;
			var pending = "Pending";
			var delayed = "Delayed";
			var arrived = "Arrived";

			transport.JW_ETA = now.AddDays(-1);
			AssertEquals(delayed, transport.JW_Calc_Status);

			transport.JW_ETA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_ATA = now.AddDays(-1);
			AssertEquals(arrived, transport.JW_Calc_Status);

			transport.JW_ATA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETA = now.AddDays(-1);
			transport.JW_ATA = now.AddMinutes(-30);
			AssertEquals(arrived, transport.JW_Calc_Status);

			transport.JW_ETA = now.AddDays(1);
			transport.JW_ATA = now.AddMinutes(-30);
			AssertEquals(arrived, transport.JW_Calc_Status);

			transport.JW_ETA = now.AddDays(-1);
			transport.JW_ATA = now.AddDays(2);
			AssertEquals(delayed, transport.JW_Calc_Status);

			transport.JW_ETA = now.AddMinutes(30);
			transport.JW_ATA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);
		}

		public void TestJW_Calc_StatusWithNeutralUTCTime_ArrivalAndDepartureDates()
		{
			var now = ZDateTime.UtcNow;
			var transit = "In Transit";
			var pending = "Pending";
			var delayed = "Delayed";
			var arrived = "Arrived";

			var transport = Factory.New<CommonConsol>().Transports[0];
			AssertEquals("Pre-condition: ETD should be empty", ZDateTime.Empty, transport.JW_ETD);
			AssertEquals("Pre-condition: ATD should be empty", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Pre-condition: ETA should be empty", ZDateTime.Empty, transport.JW_ETA);
			AssertEquals("Pre-condition: ATA should be empty", ZDateTime.Empty, transport.JW_ATA);
			AssertEquals("Pre-condition: Default status should be Pending", pending, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-5);
			transport.JW_ETA = now.AddDays(-1);
			AssertEquals(delayed, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-2);
			transport.JW_ETA = now.AddDays(1);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(2);
			transport.JW_ETA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_ATD = now.AddDays(-5);
			transport.JW_ATA = now.AddDays(-1);
			AssertEquals(arrived, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddDays(-2);
			transport.JW_ATA = now.AddDays(1);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddDays(2);
			transport.JW_ATA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddDays(-2);
			transport.JW_ETA = now.AddDays(-2);
			transport.JW_ATA = ZDateTime.Empty;
			AssertEquals(delayed, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddDays(-2);
			transport.JW_ETA = now.AddDays(2);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ATD = now.AddDays(1);
			transport.JW_ETA = now.AddDays(2);
			AssertEquals(pending, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-2);
			transport.JW_ATD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_ATA = now.AddDays(-1);
			AssertEquals(arrived, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(-2);
			transport.JW_ATA = now.AddDays(1);
			AssertEquals(transit, transport.JW_Calc_Status);

			transport.JW_ETD = now.AddDays(2);
			transport.JW_ATA = now.AddDays(1);
			AssertEquals(pending, transport.JW_Calc_Status);
		}

		public void TestJW_Calc_StatusWithLocations()
		{
			var previousValueOfTestDataUseUnloco = TestDateAttribute.UseUNLOCO;
			TestDateAttribute.UseUNLOCO = true;

			var offsetUtcToHonolulu = ZDateTime.UtcNow.AddHours(-10);
			var offsetUtcToBrisbane = ZDateTime.UtcNow.AddHours(10);
			var now = ZDateTime.UtcNow; //CIABJ has no daylight savings and UTC+0

			ZString pending = "Pending";
			ZString delayed = "Delayed";
			ZString transit = "In Transit";

			var firstTransport = Factory.New<CommonConsol>().Transports[0];
			firstTransport.JW_RL_NKLoadPort = "USHNL";
			firstTransport.JW_RL_NKDiscPort = "CIABJ";
			var secondTransport = Factory.New<CommonConsol>().Transports[0];
			secondTransport.JW_RL_NKLoadPort = "CIABJ";
			secondTransport.JW_RL_NKDiscPort = "AUBNE";

			AssertEquals(pending, firstTransport.JW_Calc_Status);
			AssertEquals(pending, secondTransport.JW_Calc_Status);

			firstTransport.JW_ETD = offsetUtcToHonolulu.AddMinutes(15);
			secondTransport.JW_ETD = now.AddMinutes(-15);

			AssertEquals("ETD only, is yet to leave and no arrival estimated date has been set", pending, firstTransport.JW_Calc_Status);
			AssertEquals("ETD only, estimated to have departed", transit, secondTransport.JW_Calc_Status);

			firstTransport.JW_ATD = offsetUtcToHonolulu.AddHours(-2);
			secondTransport.JW_ATD = now.AddHours(2);

			AssertEquals("Actual depature date has been added", transit, firstTransport.JW_Calc_Status);
			AssertEquals("Transport has left late but no arrival time has been set", pending, secondTransport.JW_Calc_Status);

			firstTransport.JW_ATD = ZDateTime.Empty;
			firstTransport.JW_ETD = offsetUtcToHonolulu.AddHours(-12);
			firstTransport.JW_ETA = now.AddHours(-4);
			secondTransport.JW_ETA = offsetUtcToBrisbane.AddHours(4);

			AssertEquals("ETA in the past but has not actually departed or arrived.", delayed, firstTransport.JW_Calc_Status);
			AssertEquals("Has departed but no arrival dates entered", pending, secondTransport.JW_Calc_Status);

			firstTransport.JW_ATA = now.AddHours(8);
			secondTransport.JW_ATA = offsetUtcToBrisbane.AddHours(-8);

			AssertEquals("Transport ATA is in the future while the ETA is in the past,", delayed, firstTransport.JW_Calc_Status);
			AssertEquals("Transport has arrived", "Arrived", secondTransport.JW_Calc_Status);

			TestDateAttribute.UseUNLOCO = previousValueOfTestDataUseUnloco;
		}

		#endregion

		public void TestDateChangesAffectShipments()
		{
			string voyageFlight1 = "1234";
			string voyageFlight2 = "1235";

			ZDateTime now = ZDateTime.Now;
			now = now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond);

			JobSailing export1 = NewSailing(TestVessel1, voyageFlight1, HomePort, OverseasPort);
			export1.Origin.JA_E_DEP = now.AddDays(5);
			export1.Destination.JB_E_ARV = now.AddDays(10);

			JobSailing export2 = NewSailing(TestVessel1, voyageFlight2, HomePort, OverseasPort);
			export2.Origin.JA_E_DEP = now.AddDays(5).AddMonths(1);
			export2.Destination.JB_E_ARV = now.AddDays(5).AddMonths(1);

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_JX = export1.PK;

			CommonShipment shipment = consol.Shipments.AddNew();

			AssertEquals("precondition: ", now.AddDays(5), shipment.JS_E_DEP);
			AssertEquals("precondition: ", export1.Origin.JA_E_DEP, shipment.JS_E_DEP);
			AssertEquals("precondition: ", export1.Destination.JB_E_ARV, shipment.JS_E_ARV);

			transport.JW_VoyageFlight = "1235";
			AssertEquals("Should be using the second sailing now", export2.PK, transport.JW_JX);
			AssertEquals("CommonShipment should be updated to the new etd", export2.Origin.JA_E_DEP, shipment.JS_E_DEP);
			AssertEquals("CommonShipment should be updated to the new eta", export2.Destination.JB_E_ARV, shipment.JS_E_ARV);

			transport.JW_VoyageFlight = "1234";
			AssertEquals("Should be back to the first sailing now", export1.PK, transport.JW_JX);
			AssertEquals("CommonShipment etd should be updated again", export1.Origin.JA_E_DEP, shipment.JS_E_DEP);
			AssertEquals("CommonShipment eta should be updated again", export1.Destination.JB_E_ARV, shipment.JS_E_ARV);
		}

		public void TestClearingFieldsWhenTransportModeChanges()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_Vessel = "Vessel";
			Transport.JW_VoyageFlight = "V1234";
			Transport.JW_ServiceString = "myServiceString";
			Transport.JW_ArrivalPortRouteId = "arrivalId";
			Transport.JW_DeparturePortRouteId = "departureId";
			Transport.JW_TerminalReceivalCommences = ZDateTime.Invalid;
			Transport.JW_TerminalCutOff = ZDateTime.Invalid;
			Transport.JW_TerminalAvailabilityDate = ZDateTime.Invalid;
			Transport.JW_TerminalStorageDate = ZDateTime.Invalid;
			Transport.JW_DepotReceivalCommences = ZDateTime.Invalid;
			Transport.JW_DepotCutOff = ZDateTime.Invalid;
			Transport.JW_DepotAvailabilityDate = ZDateTime.Invalid;
			Transport.JW_DepotStorageDate = ZDateTime.Invalid;
			Transport.JW_DocumentaryCutOff = ZDateTime.Invalid;
			Transport.JW_VGMCutOff = ZDateTime.Invalid;
			Transport.JW_JX_Load_ETA = ZDateTime.Invalid;
			Transport.JW_JX_Load_ATA = ZDateTime.Invalid;
			Transport.JW_EmptyReceivalCommences = ZDateTime.Invalid;
			Transport.JW_EmptyCutOff = ZDateTime.Invalid;
			Transport.JW_ReeferReceivalCommences = ZDateTime.Invalid;
			Transport.JW_ReeferCutOff = ZDateTime.Invalid;
			Transport.JW_DGReceivalCommences = ZDateTime.Invalid;
			Transport.JW_DGCutOff = ZDateTime.Invalid;

			AssertHasWarnings("Warning expected on VoyageFlight.", Transport.JW_VoyageFlightInfo);

			AssertHasErrors("Error expected on Vessel.", Transport.JW_VesselInfo);
			AssertHasErrors("Error expected on CTOReceivalCommences.", Transport.JW_TerminalReceivalCommencesInfo);
			AssertHasErrors("Error expected on CTOCutOff.", Transport.JW_TerminalCutOffInfo);
			AssertHasErrors("Error expected on AvailabilityDate.", Transport.JW_TerminalAvailabilityDateInfo);
			AssertHasErrors("Error expected on StorageDate.", Transport.JW_TerminalStorageDateInfo);
			AssertHasErrors("Error expected on DepotReceivalCommences.", Transport.JW_DepotReceivalCommencesInfo);
			AssertHasErrors("Error expected on DepotCutOff.", Transport.JW_DepotCutOffInfo);
			AssertHasErrors("Error expected on DepotAvailabilityDate.", Transport.JW_DepotAvailabilityDateInfo);
			AssertHasErrors("Error expected on DepotStorageDate.", Transport.JW_DepotStorageDateInfo);
			AssertHasErrors("Error expected on Docs Cut Off.", Transport.JW_DocumentaryCutOffInfo);
			AssertHasErrors("Error expected on VGM Cut Off.", Transport.JW_VGMCutOffInfo);
			AssertHasErrors("Error expected on LoadETA.", Transport.JW_JX_Load_ETAInfo);
			AssertHasErrors("Error expected on LoadATA.", Transport.JW_JX_Load_ATAInfo);
			AssertHasErrors("Error expected on EmptyReceivalCommences.", Transport.JW_EmptyReceivalCommencesInfo);
			AssertHasErrors("Error expected on EmptyCutOff.", Transport.JW_EmptyCutOffInfo);
			AssertHasErrors("Error expected on ReeferReceivalCommences.", Transport.JW_ReeferReceivalCommencesInfo);
			AssertHasErrors("Error expected on ReeferCutOff.", Transport.JW_ReeferCutOffInfo);
			AssertHasErrors("Error expected on DGReceivalCommences.", Transport.JW_DGReceivalCommencesInfo);
			AssertHasErrors("Error expected on DGCutOff.", Transport.JW_DGCutOffInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Air;

			AssertCleared(Transport.JW_VesselInfo);
			AssertCleared(Transport.JW_VoyageFlightInfo);
			AssertCleared(Transport.JW_ServiceStringInfo);
			AssertCleared(Transport.JW_ArrivalPortRouteIdInfo);
			AssertCleared(Transport.JW_DeparturePortRouteIdInfo);
			AssertCleared(Transport.JW_TerminalReceivalCommencesInfo);
			AssertCleared(Transport.JW_TerminalCutOffInfo);
			AssertCleared(Transport.JW_TerminalAvailabilityDateInfo);
			AssertCleared(Transport.JW_TerminalStorageDateInfo);
			AssertCleared(Transport.JW_DepotReceivalCommencesInfo);
			AssertCleared(Transport.JW_DepotCutOffInfo);
			AssertCleared(Transport.JW_DepotAvailabilityDateInfo);
			AssertCleared(Transport.JW_DepotStorageDateInfo);
			AssertCleared(Transport.JW_DocumentaryCutOffInfo);
			AssertCleared(Transport.JW_VGMCutOffInfo);
			AssertCleared(Transport.JW_JX_Load_ETAInfo);
			AssertCleared(Transport.JW_JX_Load_ATAInfo);
			AssertCleared(Transport.JW_EmptyReceivalCommencesInfo);
			AssertCleared(Transport.JW_EmptyCutOffInfo);
			AssertCleared(Transport.JW_ReeferReceivalCommencesInfo);
			AssertCleared(Transport.JW_ReeferCutOffInfo);
			AssertCleared(Transport.JW_DGReceivalCommencesInfo);
			AssertCleared(Transport.JW_DGCutOffInfo);
		}

		public void TestDateEventsSpecifyIsEstimate_Linked()
		{
			DateEventsSpecifyIsEstimate(true);
		}

		public void TestDateEventsSpecifyIsEstimate_NotLinked()
		{
			DateEventsSpecifyIsEstimate(false);
		}

		void DateEventsSpecifyIsEstimate(bool isLinked)
		{
			Transport.JW_TransportMode = ExportSailing.Voyage.JV_AirSeaRoad;
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ExportSailing.PK;
			Transport.JW_IsLinked = isLinked;

			Transport.JW_ETD = new ZDateTime(2008, 12, 3, 2, 46, 0);
			Transport.JW_ETA = new ZDateTime(2008, 12, 15, 9, 38, 0);
			AssertNull("Precondition - no arrival log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Precondition - no departure log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Departure));

			Factory.Save();

			StmALog[] arrivalLogs = Transport.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Arrival.Code));
			StmALog[] departureLogs = Transport.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code));

			AssertEquals("should have found 1 arrival log", 1, arrivalLogs.Length);
			AssertNotNull("Arrival log should have been created", arrivalLogs[0]);
			AssertEquals("Arrival log should be an estimate", true, arrivalLogs[0].SL_IsEstimate);
			AssertEquals("Arrival log should have correct event date", new ZDateTime(2008, 12, 15, 9, 38, 0), arrivalLogs[0].SL_EventTime);

			AssertEquals("should have found 1 departure log", 1, departureLogs.Length);
			AssertNotNull("Departure log should have been created", departureLogs[0]);
			AssertEquals("Departure log should be an estimate", true, departureLogs[0].SL_IsEstimate);
			AssertEquals("Departure log should have correct event date", new ZDateTime(2008, 12, 3, 2, 46, 0), departureLogs[0].SL_EventTime);
		}

		public void DateEventsSpecifyIsNotEstimate_Linked()
		{
			DateEventsSpecifyIsNotEstimate(true);
		}

		public void DateEventsSpecifyIsNotEstimate_NotLinked()
		{
			DateEventsSpecifyIsNotEstimate(false);
		}

		void DateEventsSpecifyIsNotEstimate(bool isLinked)
		{
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ExportSailing.PK;
			Transport.JW_IsLinked = isLinked;

			Transport.JW_ATD = ZDateTime.Now.AddDays(1);
			Transport.JW_ATA = ZDateTime.Now.AddDays(11);
			AssertNull("Precondition - no arrival log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Precondition - no departure log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Departure));

			Factory.Save();

			StmALog arrivalLog = Transport.Logs.MostRecentLogByEventTime(Events.Arrival);
			StmALog departureLog = Transport.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertNotNull("Arrival log should have been created", arrivalLog);
			AssertEquals("Arrival log should not be an estimate", false, arrivalLog.SL_IsEstimate);
			AssertNotNull("Departure log should have been created", departureLog);
			AssertEquals("Departure log should not be an estimate", false, departureLog.SL_IsEstimate);
		}

		public void TestDateEventReferenceIsNotLocalized()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.ChineseSimplified))
			{
				Transport.JW_IsLinked = true;
				Transport.JW_JX = ExportSailing.PK;
				Transport.JW_ATD = ZDateTime.Now.AddDays(1);
				Transport.JW_ATA = ZDateTime.Now.AddDays(11);
				AssertNull("Precondition - no arrival log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Arrival));
				AssertNull("Precondition - no departure log exists yet", Transport.Logs.MostRecentLogByEventTime(Events.Departure));

				Factory.Save();

				StmALog arrivalLog = Transport.Logs.MostRecentLogByEventTime(Events.Arrival);
				StmALog departureLog = Transport.Logs.MostRecentLogByEventTime(Events.Departure);
				AssertNotNull("Arrival log should have been created", arrivalLog);
				AssertEquals("Arrival log should not have errors", false, arrivalLog.HasErrors);
				AssertEquals("Arrival log Reference should be in English", string.Format("Changed To: {0}", Transport.JW_ETA.ToShortDateString()), arrivalLog.ReferenceFreeText);
				AssertEquals("Location in arrival log", Transport.JW_RL_NKDiscPort, arrivalLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
				AssertNotNull("Departure log should have been created", departureLog);
				AssertEquals("Departure log should not have errors", false, departureLog.HasErrors);
				AssertEquals("Departure log Reference should be in English", string.Format("Changed To: {0}", Transport.JW_ETD.ToShortDateString()), departureLog.ReferenceFreeText);
				AssertEquals("Location in departure log", Transport.JW_RL_NKLoadPort, departureLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			}
		}

		public void TestBookingRefDefaultsFromSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "234";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(11);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_ReservedMasterBill = "Blah";

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "234";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;

			AssertEquals(sailing.JX_ReservedMasterBill, transport.JW_CarrierBookingReference);
			AssertEquals(sailing.JX_ReservedMasterBill, consol.JK_BookingReference);
		}

		public void TestUpdateCarrierWhenSailingChangesOnTransport()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "DEMCARBER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsAirLine = true;
			var airline = RefAirline.LoadFromAirlinePrefix(Factory, "020");
			carrier.MiscServ.OM_RM_Airline = airline.PK;

			AssertUpdateCarrierWhenSailingChangesOnTransport("020", carrier.PK);
			AssertUpdateCarrierWhenSailingChangesOnTransport("0", ZGuid.Empty);

			void AssertUpdateCarrierWhenSailingChangesOnTransport(ZString consolMasterBillNum, ZGuid expectedTransportCarrierPK)
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_MasterBillNum = consolMasterBillNum;

				var transport = consol.Transports.AddNew();

				var sailing = Factory.New<JobSailing>();
				var origin = Factory.New<VoyageOrigin>();
				var destination = Factory.New<VoyageDestination>();
				var voyage = Factory.New<JobVoyage>();

				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;

				origin.JA_JV = voyage.PK;
				destination.JB_JV = voyage.PK;

				AssertEquals("Precondition", ZGuid.Empty, voyage.JV_OH_Line);

				transport.JW_JX = sailing.PK;

				Factory.Save();

				AssertEquals("Transport Carrier should be defaulted when transport is linked to flight schedule", expectedTransportCarrierPK, transport.CarrierPK);
			}
		}

		public void TestJW_ParentDescription()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "aoeu";
			AssertEquals("aoeu", consol.Transports[0].JW_ParentDescription);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "snth";
			AssertEquals("snth", shipment.Transports.AddNew().JW_ParentDescription);
		}

		public void TestTransportModeList()
		{
			AssertEquals("AIR, SEA, ROA, RAI, STO, IWT", Transport.JW_TransportMode_List.CodesAsString);
		}

		public void TestJW_AdditionalTransportMode_List()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>("Transport.JW_AdditionalTransportMode_List_Rail");
			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			Transport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			AssertEquals("ROA", Transport.JW_AdditionalTransportMode_List.CodesAsString);

			Factory.ClearCachedValue<CodeDescriptionPairList>("Transport.JW_AdditionalTransportMode_List_InlandWaterwayTransport");
			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			Transport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			AssertEquals("RAI, ROA", Transport.JW_AdditionalTransportMode_List.CodesAsString);

			Factory.ClearCachedValue<CodeDescriptionPairList>("Transport.JW_AdditionalTransportMode_List_Rail");
			Factory.ClearCachedValue<CodeDescriptionPairList>("Transport.JW_AdditionalTransportMode_List_InlandWaterwayTransport");
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, Transport.JW_AdditionalTransportMode_List.CodesAsString);
		}

		public void TestTransportAirStatusList()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;

			AssertEquals("PLN, CNF, RQD, CRQ, UBL, QUE, FNO, CAN", transport.JW_Status_List.CodesAsString);
		}

		public void TestTransportStatusList()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("PLN, CNF, HLD", transport.JW_Status_List.CodesAsString);
		}

		public void TestSBREventLog_ShouldCreateWhenParentTypeIsConsol()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "SGSIN";
				consol.JK_MasterBillNum = "08187443521";

				Transport transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_ETA = 1.DaysAgo();

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Precondition: Should not create SBR event", 0, logs.Count);

				consol.JK_MasterBillNum = "46197135463";
				Factory.Save();
				logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should be created new SBR event", 1, logs.Count);
			}
		}

		public void TestSBREventLog_ShouldNotCreateWhenParentTypeIsNotConsol()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_HouseBill = "08187443521";

				var transport = shipment.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2017, 01, 01);
				transport.JW_ETA = new ZDateTime(2017, 01, 02);

				Factory.Save();

				var logs = shipment.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals(0, logs.Count);
			}
		}

		public void TestSBREventLog_WithNoIATACode()
		{
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals("Precondition: Load Port IATA Code", "SYD", loadPort.RL_IATA);

			var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			AssertEquals("Precondition: Discharge Port IATA Code", "SIN", dischargePort.RL_IATA);

			AssertSBREventLogWithNoIATACode("Load/Discharge Port have valid IATA Code", loadPort, dischargePort, 0, 1);

			loadPort.RL_IATA = "";
			AssertSBREventLogWithNoIATACode("Only Discharge Port has valid IATA Code", loadPort, dischargePort, 1, 1);

			loadPort.RL_IATA = "SYD";
			dischargePort.RL_IATA = "";
			AssertSBREventLogWithNoIATACode("Only Load Port has valid IATA Code", loadPort, dischargePort, 1, 1);

			loadPort.RL_IATA = "";
			dischargePort.RL_IATA = "";
			AssertSBREventLogWithNoIATACode("Load and Discharge Ports do not have valid IATA Code", loadPort, dischargePort, 0, 0);
		}

		public void TestSBREventWithOnlyArrivalDate()
		{
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals("Precondition: Load Port IATA Code", "SYD", loadPort.RL_IATA);

			var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			AssertEquals("Precondition: Discharge Port IATA Code", "SIN", dischargePort.RL_IATA);

			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort.RL_Code;
				consol.JK_RL_NKDischargePort = dischargePort.RL_Code;
				consol.JK_MasterBillNum = "08187443521";

				Transport transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = 1.DaysAgo();

				Factory.Save();

				var logs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals("Should not create new SBR event in transport", 0, logs.Count);

				logs = transport.Sailing.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals("Should create new SBR event in sailing", 1, logs.Count);
				AssertEquals($"|FDT={transport.JW_ETD:yyyy-MM-dd}|TYP=AWB Automation|VFL=QF001", logs[0].SL_Reference);
			}
		}

		public void TestDepartureEventAndArrivalEventForConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_ETD = new ZDateTime(2017, 01, 01);
			transport.JW_ETA = new ZDateTime(2017, 03, 25);

			Factory.Save();

			var depLogs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.DepartureCode && !x.IsCancelled).ToList();
			var arvLogs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.ArrivalCode && !x.IsCancelled).ToList();
			AssertEquals("Should create new DEP event in transport", 1, depLogs.Count);
			AssertEquals("Should create new ARV event in transport", 1, arvLogs.Count);
			AssertEquals("Changed To: 01-Jan-17|FAC=CTO|FDT=2017-01-01|LOC=AUSYD|MOD=AIR", depLogs[0].SL_Reference);
			AssertEquals("Changed To: 25-Mar-17|FAC=CTO|FDT=2017-03-25|LOC=SGSIN|MOD=AIR", arvLogs[0].SL_Reference);
		}

		public void TestOceanCarrierBookingByTEUCEventForConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_BookingRequestAvailable = true;
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsShippingLine = true;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAERSK";
			org.OH_RL_NKClosestPort = "DKAAL";
			org.MainAddress.Address1 = "Unit 13";
			org.MainAddress.Address2 = "4 Lost Lane";
			org.MainAddress.City = "Aalborg";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "DK";
			org.OH_IsShippingProvider = true;
			org.OH_IsShippingLine = true;
			org.OH_IsSeaWholesaler = false;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			consol.Containers.RemoveAndDeleteAll();

			var refcontainer1 = Factory.New<RefContainer>();
			refcontainer1.RC_Code = "Test";
			refcontainer1.RC_TEU = 5m;

			var refcontainer2 = Factory.New<RefContainer>();
			refcontainer2.RC_Code = "T_st";
			refcontainer2.RC_TEU = 0m;
			refcontainer2.RC_ISOType = "1";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA00000121";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 100;
			container1.JC_RC = refcontainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = refcontainer2.PK;

			var referenceParameters = new Dictionary<string, string>();
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.New, "1");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Maximum, "1");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Quantity, "0");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Type, "Document");

			consol.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.OceanCarrierBookingByTEU, isEstimate: false, eventTime: new ZDateTimeOffset(2017, 01, 25), parameters: referenceParameters));

			Factory.Save();

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Receiving Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "IN5PA";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			Factory.Save();

			Branch.GB_OH_OrgProxy = sendingForwarder.PK;
			Factory.Save();

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_ATA = new ZDateTime(2017, 01, 01);
			transport.JW_ATD = new ZDateTime(2017, 03, 25);

			Factory.Save();

			var ocbLogs = consol.Logs.GetAllLogs().Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.OceanCarrierBookingByTEUCode && !x.IsCancelled && string.Compare(x.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type), FreightConstants.EventParameterDescriptions.Departure, StringComparison.OrdinalIgnoreCase) == 0).ToList();
			AssertEquals(1, ocbLogs.Count);
			AssertEquals("|CMP=1234|MAX=500.5|NEW=500.5|OLD=1|QTY=499.5|STA=ORG|TYP=Departure", ocbLogs.First().SL_Reference);

			consol.Containers.RemoveAndDeleteAll();

			Factory.Save();

			transport.JW_ATD = new ZDateTime(2017, 03, 26);

			container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Test0010101";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 100;
			container1.JC_RC = refcontainer1.PK;

			container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "Test0010111";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = ZGuid.Empty;

			Factory.Save();

			ocbLogs = consol.Logs.GetAllLogs().Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.OceanCarrierBookingByTEUCode && !x.IsCancelled && string.Compare(x.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type), FreightConstants.EventParameterDescriptions.Departure, StringComparison.OrdinalIgnoreCase) == 0).ToList();
			AssertEquals(2, ocbLogs.Count);
			AssertEquals(2, ocbLogs.Count(x => x.SL_GB_NKBranch == Branch.GB_Code));
			AssertEquals("|CMP=1234|MAX=500.5|NEW=500|OLD=500.5|QTY=0.0|STA=AMD|TYP=Departure", ocbLogs.OrderByDescending(x => x.SL_PostedTimeUtc).First().SL_Reference);
		}

		public void TestOceanCarrierBookingByTEUEventForConsolWithChineseShippingOrder()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "CNSGH";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsShippingLine = true;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAERSK";
			org.OH_RL_NKClosestPort = "DKAAL";
			org.MainAddress.Address1 = "Unit 13";
			org.MainAddress.Address2 = "4 Lost Lane";
			org.MainAddress.City = "Aalborg";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "DK";
			org.OH_IsShippingProvider = true;
			org.OH_IsShippingLine = true;
			org.OH_IsSeaWholesaler = false;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			consol.Containers.RemoveAndDeleteAll();

			var refcontainer1 = Factory.New<RefContainer>();
			refcontainer1.RC_Code = "Test";
			refcontainer1.RC_TEU = 5m;

			var refcontainer2 = Factory.New<RefContainer>();
			refcontainer2.RC_Code = "T_st";
			refcontainer2.RC_TEU = 0m;
			refcontainer2.RC_ISOType = "1";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA00000121";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 100;
			container1.JC_RC = refcontainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = refcontainer2.PK;

			var referenceParameters = new Dictionary<string, string>();
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.New, "1");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Maximum, "1");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Quantity, "0");
			referenceParameters.Add(EventConstants.EventReferenceParameters.Codes.Type, "Document");

			consol.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.OceanCarrierBookingByTEU, isEstimate: false, eventTime: new ZDateTimeOffset(2017, 01, 25), parameters: referenceParameters));

			Factory.Save();

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Receiving Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "IN5PA";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			Factory.Save();

			Branch.GB_OH_OrgProxy = sendingForwarder.PK;
			Factory.Save();

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "CNSGH";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_ATA = new ZDateTime(2017, 01, 01);
			transport.JW_ATD = new ZDateTime(2017, 03, 25);

			Factory.Save();

			var ocbLogs = consol.Logs.GetAllLogs().Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.OceanCarrierBookingByTEUCode && !x.IsCancelled && string.Compare(x.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type), FreightConstants.EventParameterDescriptions.Departure, StringComparison.OrdinalIgnoreCase) == 0).ToList();
			AssertEquals(1, ocbLogs.Count);
			AssertEquals("|CMP=1234|MAX=500.5|NEW=500.5|OLD=1|QTY=499.5|STA=ORG|TYP=Departure", ocbLogs.First().SL_Reference);

			consol.Containers.RemoveAndDeleteAll();

			Factory.Save();

			transport.JW_ATD = new ZDateTime(2017, 03, 26);

			container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Test0010101";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 100;
			container1.JC_RC = refcontainer1.PK;

			container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "Test0010111";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = ZGuid.Empty;

			Factory.Save();

			ocbLogs = consol.Logs.GetAllLogs().Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.OceanCarrierBookingByTEUCode && !x.IsCancelled && string.Compare(x.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type), FreightConstants.EventParameterDescriptions.Departure, StringComparison.OrdinalIgnoreCase) == 0).ToList();
			AssertEquals(2, ocbLogs.Count);
			AssertEquals(2, ocbLogs.Count(x => x.SL_GB_NKBranch == Branch.GB_Code));
			AssertEquals("|CMP=1234|MAX=500.5|NEW=500|OLD=500.5|QTY=0.0|STA=AMD|TYP=Departure", ocbLogs.OrderByDescending(x => x.SL_PostedTimeUtc).First().SL_Reference);
		}

		public void TestDepartureEventAndArrivalEventForShipment()
		{
			var shipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport.JW_VoyageFlight = "QF232";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2017, 01, 01);
			transport.JW_ETA = new ZDateTime(2017, 03, 25);

			Factory.Save();

			var depLogs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.DepartureCode && !x.IsCancelled).ToList();
			var arvLogs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.ArrivalCode && !x.IsCancelled).ToList();
			AssertEquals("Should create new DEP event in transport", 1, depLogs.Count);
			AssertEquals("Should create new ARV event in transport", 1, arvLogs.Count);
			AssertEquals("Changed To: 01-Jan-17|FAC=CTO|FDT=2017-01-01|LOC=AUSYD|VFL=QF232", depLogs[0].SL_Reference);
			AssertEquals("Changed To: 25-Mar-17|FAC=CTO|FDT=2017-03-25|LOC=USLAX|VFL=QF232", arvLogs[0].SL_Reference);
		}

		public void TestSBREventWithOnlyDepartureDate()
		{
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals("Precondition: Load Port IATA Code", "SYD", loadPort.RL_IATA);

			var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			AssertEquals("Precondition: Discharge Port IATA Code", "SIN", dischargePort.RL_IATA);

			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort.RL_Code;
				consol.JK_RL_NKDischargePort = dischargePort.RL_Code;
				consol.JK_MasterBillNum = "08187443521";

				Transport transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETA = 1.DaysAgo();

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals("Should create new SBR event", 1, logs.Count);
				AssertEquals("|RFN=08187443521|TYP=AWB Automation", logs[0].SL_Reference);
			}
		}

		public void TestSBREvent_RecentDepartureAndArrivalDates()
		{
			void AssertSBRLog(CommonConsol consol, string fieldName, ZDateTime date, int expectedLogCoung)
			{
				var transport = consol.Transports[0];
				transport[fieldName] = date;
				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals("Should create new SBR event", expectedLogCoung, logs.Count);
				if (expectedLogCoung > 0)
				{
					AssertEquals("|RFN=08187443521|TYP=AWB Automation", logs[0].SL_Reference);
				}
			}
			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals("Precondition: Load Port IATA Code", "SYD", loadPort.RL_IATA);

			var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			AssertEquals("Precondition: Discharge Port IATA Code", "SIN", dischargePort.RL_IATA);

			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort.RL_Code;
				consol.JK_RL_NKDischargePort = dischargePort.RL_Code;
				consol.JK_MasterBillNum = "08187443521";

				Transport transport = consol.Transports[0];
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "QF001";
				Factory.Save();

				AssertSBRLog(consol, "JW_ETD", 3.DaysAgo(), 0);
				AssertSBRLog(consol, "JW_ETD", 3.DaysAgo().AddMinutes(1), 1);
				AssertSBRLog(consol, "JW_ETD", 2.DaysAgo(), 1);

				consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).Cancel();
				AssertSBRLog(consol, "JW_ATD", 3.DaysAgo(), 0);
				AssertSBRLog(consol, "JW_ATD", 3.DaysAgo().AddMinutes(1), 1);
				AssertSBRLog(consol, "JW_ATD", 2.DaysAgo(), 1);

				consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).Cancel();
				AssertSBRLog(consol, "JW_ETA", 2.DaysAgo(), 0);
				AssertSBRLog(consol, "JW_ETA", 2.DaysAgo().AddMinutes(1), 1);
				AssertSBRLog(consol, "JW_ETA", 1.DaysAgo(), 1);

				consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).Cancel();
				AssertSBRLog(consol, "JW_ATA", 2.DaysAgo(), 0);
				AssertSBRLog(consol, "JW_ATA", 2.DaysAgo().AddMinutes(1), 1);
				AssertSBRLog(consol, "JW_ATA", 1.DaysAgo(), 1);
			}
		}

		void AssertSBREventLogWithNoIATACode(string message, RefUNLOCO loadPort, RefUNLOCO dischargePort, int expectedLogCountAfterCreation, int expectedLogCountAfterEdit)
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort.RL_Code;
				consol.JK_RL_NKDischargePort = dischargePort.RL_Code;
				consol.JK_MasterBillNum = "08187443521";

				Transport transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF001";
				transport.JW_ETD = 3.DaysAgo();
				transport.JW_ETA = 1.DaysAgo();

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals($"Should create {expectedLogCountAfterCreation} SBR event(s)", expectedLogCountAfterCreation, logs.Count);

				consol.JK_MasterBillNum = "46197135463";
				Factory.Save();
				logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals($"Should create {expectedLogCountAfterEdit} new SBR event(s): {message}", expectedLogCountAfterEdit, logs.Count);

				consol.Delete();
				Factory.Save();
			}
		}

		public void TestSaving_CreateSBREventLog_ForDeletedTransport()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				AssertEquals("Precondition: Load Port IATA Code", "SYD", loadPort.RL_IATA);

				var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
				AssertEquals("Precondition: Discharge Port IATA Code", "SIN", dischargePort.RL_IATA);

				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort.RL_Code;
				consol.JK_RL_NKDischargePort = dischargePort.RL_Code;
				consol.JK_MasterBillNum = "08187443521";

				var leg1 = consol.Transports[0];
				leg1.JW_VoyageFlight = "QF1";
				leg1.JW_RL_NKLoadPort = "AUSYD";
				leg1.JW_RL_NKDiscPort = "AUMEL";
				leg1.JW_ETD = ZDateTime.Now;
				leg1.JW_IsLinked = false;

				var leg2 = consol.Transports.AddNew();
				leg2.JW_VoyageFlight = "QF2";
				leg2.JW_RL_NKLoadPort = "AUMEL";
				leg2.JW_RL_NKDiscPort = "AUPER";
				leg2.JW_ETD = ZDateTime.Now;
				leg2.JW_IsLinked = false;

				var leg3 = consol.Transports.AddNew();
				leg3.JW_VoyageFlight = "QF3";
				leg3.JW_RL_NKLoadPort = "AUPER";
				leg3.JW_RL_NKDiscPort = "SGSIN";
				leg3.JW_ETD = ZDateTime.Now;
				leg3.JW_IsLinked = false;

				Factory.Save();

				var logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled).ToList();
				AssertEquals("Should create new SBR event in console", 1, logs.Count);
				AssertEquals("|RFN=08187443521|TYP=AWB Automation", logs[0].SL_Reference);

				leg2.Delete();
				Factory.Save();

				logs = consol.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should cancelled existing SBR event", true, logs[0].IsCancelled);
				AssertEquals("Should create a new SBR event for deleted transport", 2, logs.Count);
				AssertEquals("|RFN=08187443521|TYP=AWB Automation", logs[1].SL_Reference);
			}
		}

		public void TestAutoTransportTypeSelection()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = shipment.Transports.AddNew();
			AssertEquals(Core.Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);

			Transport transport2 = shipment.Transports.AddNew();
			AssertEquals(Core.Constants.TransportPlanningType.Other, transport2.JW_TransportType);

			transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(Core.Constants.TransportPlanningType.MainVessel, transport1.JW_TransportType);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportPlanningType.Other, transport2.JW_TransportType);

			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportPlanningType.Flight1, transport1.JW_TransportType);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport3 = shipment.Transports.AddNew();
			AssertEquals(Core.Constants.TransportPlanningType.Flight3, transport3.JW_TransportType);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport4 = shipment.Transports.AddNew();
			AssertEquals(Core.Constants.TransportPlanningType.MainVessel, transport4.JW_TransportType);

			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportPlanningType.Other, transport4.JW_TransportType);
		}

		public void TestAutotransportTypeSelection_WhenTransportModeIsIWT()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var firstTransport = shipment.Transports.AddNew();
			AssertEquals(Constants.TransportPlanningType.MainVessel, firstTransport.JW_TransportType);

			var secondTransport = shipment.Transports.AddNew();
			AssertEquals(Constants.TransportPlanningType.Other, secondTransport.JW_TransportType);

			secondTransport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertNoError(secondTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");
			AssertEquals(Constants.TransportPlanningType.PreCarriage, secondTransport.JW_TransportType);

			firstTransport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(Constants.TransportPlanningType.MainVessel, firstTransport.JW_TransportType);
			AssertHasError(firstTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");

			secondTransport.JW_TransportMode = Constants.TransportModes.Sea;
			secondTransport.JW_TransportType = ZString.Empty;
			secondTransport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertNoError(secondTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");
			AssertEquals(Constants.TransportPlanningType.PreCarriage, secondTransport.JW_TransportType);

			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertNoError(firstTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");
			AssertEquals(Constants.TransportPlanningType.MainVessel, firstTransport.JW_TransportType);

			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.TransportPlanningType.PreCarriage, secondTransport.JW_TransportType);
			AssertNoError(secondTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");

			secondTransport.JW_TransportType = ZString.Empty;
			secondTransport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(Constants.TransportPlanningType.OnForwarding, secondTransport.JW_TransportType);
			AssertNoError(secondTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");

			secondTransport.JW_TransportMode = Constants.TransportModes.Sea;
			secondTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			AssertEquals(Constants.TransportPlanningType.MainVessel, secondTransport.JW_TransportType);
			AssertNoError(secondTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");

			firstTransport.JW_TransportType = ZString.Empty;
			firstTransport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(Constants.TransportPlanningType.PreCarriage, firstTransport.JW_TransportType);
			AssertNoError(firstTransport.JW_TransportModeInfo, "This transport mode is not applicable to Main legs.");
		}

		#region CargoAvailable event

		public void TestFactorySave_AvailabilityDateIsChanged_AddCargoAvailableEvent()
		{
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.FCL, t => t.JW_TerminalAvailabilityDate, EventConstants.Facilities.Code.Terminal);
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.ULD, t => t.JW_TerminalAvailabilityDate, EventConstants.Facilities.Code.Terminal);
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.LCL, t => t.JW_DepotAvailabilityDate, EventConstants.Facilities.Code.Depot);
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.Loose, t => t.JW_DepotAvailabilityDate, EventConstants.Facilities.Code.Depot);
		}

		void EnsureCargoAvailableEventCreated(ZString whenContainerMode, Func<Transport, ZDateTime> expectedEventTime, string expectedFacility)
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = whenContainerMode;

			var transport = consol.Transports.AddNew();
			transport.JW_TerminalAvailabilityDate = 5.DaysAgo();
			transport.JW_DepotAvailabilityDate = 10.DaysAgo();
			transport.JW_RL_NKDiscPort = "UAIEV";

			Factory.Save();

			var log = transport.Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log);
			AssertEquals("Event date", expectedEventTime(transport), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", expectedFacility, log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		public void TestCargoAvailableEventCreatedGroupage()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;

			var transport = consol.Transports[0];
			transport.JW_TerminalAvailabilityDate = 5.DaysAgo();
			transport.JW_DepotAvailabilityDate = 10.DaysAgo();
			transport.JW_RL_NKDiscPort = "UAIEV";

			Factory.Save();

			var logs = transport.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "CAV");

			var ctoLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Terminal);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), ctoLog);
			AssertEquals("Event date", transport.JW_TerminalAvailabilityDate, ctoLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", ctoLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, ctoLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);

			var cfsLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Depot);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), cfsLog);
			AssertEquals("Event date", transport.JW_DepotAvailabilityDate, cfsLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", cfsLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Depot, cfsLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		#endregion

		#region StorageCommenced & ReceiptCommenced

		public void TestFactorySave_StorageDateIsChanged_AddStorageCommencedEvent()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var transport = consol.Transports.AddNew();
			transport.JW_TerminalStorageDate = 5.DaysAgo();
			transport.JW_RL_NKDiscPort = "UAIEV";

			Factory.Save();

			var log = transport.Logs.MostRecentLogByEventTime(Events.StorageCommenced);
			AssertNotNull(string.Format("Log with {0} event", Events.StorageCommenced.Code), log);
			AssertEquals("Event date", transport.JW_TerminalStorageDate, log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		public void TestFactorySave_ReceivalCommencesDateIsChanged_AddReceiptCommencedEvent()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var transport = consol.Transports.AddNew();
			transport.JW_TerminalReceivalCommences = 5.DaysAgo();
			transport.JW_RL_NKLoadPort = "UAIEV";

			Factory.Save();

			var log = transport.Logs.MostRecentLogByEventTime(Events.ReceiptCommenced);
			AssertNotNull(string.Format("Log with {0} event", Events.ReceiptCommenced.Code), log);
			AssertEquals("Event date", transport.JW_TerminalReceivalCommences, log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		#endregion

		#region JW_OnlineScheduleStatus

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_UpdateStatusWithMatch()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;

			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var consol = Factory.New<CommonConsol>();

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				transport.TryMatchAgainstOnlineFlights();
				AssertEquals("Matching enabled: Transport should be matched", Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
				AssertNotEquals("Matched Schedule set", ScheduleInfo.Empty, transport.MatchedSchedule);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_AutomaticMatching()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;

			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var consol = Factory.New<CommonConsol>();

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETD = ZDate.Today;

				AssertEquals("Automatic matching enabled", Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
				AssertNotEquals("Matched Schedule set", ScheduleInfo.Empty, transport.MatchedSchedule);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_AutomaticMatching_Disabled_WhenNotUserInteractive()
		{
			var originalUserInteractive = Globals.IsUserInteractive;

			using (new DisposableAction(() => Globals.IsUserInteractive = false, () => Globals.IsUserInteractive = originalUserInteractive))
			{
				var mock = new Mock<IS8Matcher>();
				var matcher = mock.Object;

				mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
				mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
				mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

				using (ObjectFactory.Substitute(matcher))
				{
					var consol = Factory.New<CommonConsol>();

					var transport = consol.Transports.AddNew();
					transport.JW_TransportMode = Constants.TransportModes.Air;
					transport.JW_VoyageFlight = "QF1234";
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = "USJFK";
					transport.JW_ETD = ZDate.Today;

					AssertEquals("Not User Interactive: Transport should not be matched", Constants.FlightScheduleStatus.Unknown, transport.JW_OnlineScheduleStatus);
				}
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_AutomaticMatching_NotAirTransport()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;

			using (ObjectFactory.Substitute(matcher))
			{
				var consol = Factory.New<CommonConsol>();

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(3);

				AssertEquals("Transport is Sea: should not be matched", Constants.FlightScheduleStatus.Unknown, transport.JW_OnlineScheduleStatus);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_UpdatesOnETDChange()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var consol = Factory.New<CommonConsol>();
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF911";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_RL_NKDiscPort = "AUSYD";

				transport.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;
				AssertEquals("Pre: Transport online schedule status should be unknown initially.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Unmatched);

				transport.JW_ETD = ZDate.Today;
				AssertEquals("Setting JW_ETD should have refreshed the online schedule status.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Matched);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_UpdatesOnETAChange()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var consol = Factory.New<CommonConsol>();
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF420";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_RL_NKDiscPort = "AUSYD";

				transport.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;
				AssertEquals("Pre: Transport online schedule status should be unmatched initially.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Unmatched);

				transport.JW_ETA = ZDate.Today;
				AssertEquals("Setting JW_ETA should have refreshed the online schedule status.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Matched);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_OnlineScheduleStatus_UpdatesOnJW_JXChange()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF1234";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USJFK";
				destination.JB_E_ARV = ZDate.Today;

				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];
				sailing.JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;

				var consol = Factory.New<CommonConsol>();
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;

				transport.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;
				AssertEquals("Pre: Transport online schedule status should be unmatched initially.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Unmatched);

				transport.JW_IsLinked = true;
				transport.JW_JX = sailing.PK;
				AssertEquals("Transport online schedule status should be copied from linked sailing.", transport.JW_OnlineScheduleStatus, Constants.FlightScheduleStatus.Matched);
			});
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_Unsaved()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var transport = CreateTransportForMatching();

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Transport aircraft type should be populated from matched schedule.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		public void TestJW_AircraftType_UpdatesOnMatching_Saved()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var transport = CreateTransportForMatching();
				Factory.Save();

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Transport aircraft type should be populated from matched schedule.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_Linked()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.JV_VoyageFlight = "QF1234";

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = ZDate.Today;

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NZAKL";
				destination.JB_E_ARV = ZDate.Today;
				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];
				var consol = Factory.New<CommonConsol>();

				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_ETD = ZDate.Today;
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing.PK;

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Linked sailing aircraft type is populated.", aircraftType, sailing.JX_JV_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_UserUpdatedAircraftType()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var transport = CreateTransportForMatching();
				transport.JW_AircraftTypeForBinding = "15Z"; // User input

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Transport aircraft type should remained unchanged", "15Z", transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_SavedAircraftType()
		{
			// Arrange
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				var transport = CreateTransportForMatching();
				transport.JW_AircraftTypeForBinding = "15Z";
				Factory.Save();

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertNotEquals("Transport aircraft type should not be set to schedule aircraft type.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_EmptyUserAircraftType()
		{
			// First flight matching
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				// Arrange
				var transport = CreateTransportForMatching();

				// Act
				transport.TryMatchAgainstOnlineFlights();
				transport.JW_AircraftTypeForBinding = "";
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Transport aircraft type should be set to schedule aircraft type.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_SavedEmptyAircraftType()
		{
			// First flight matching
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				// Arrange
				var transport = CreateTransportForMatching();
				transport.JW_AircraftType = ZString.Empty;
				Factory.Save();

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertEquals("Transport aircraft type should be set to schedule aircraft type.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestJW_AircraftType_UpdatesOnMatching_ExistingAircraftType()
		{
			// First flight matching
			var aircraftType = new ZString("14Z");
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				// Arrange
				var consol = Factory.New<CommonConsol>();
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_AircraftType = "15Z"; // Mimic pre session aircraft type
				transport.JW_VoyageFlight = "QF1234";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "NZAKL";
				transport.JW_ETD = ZDate.Today;

				// Act
				transport.TryMatchAgainstOnlineFlights();

				// Assert
				AssertNotEquals("Transport aircraft type should not be set to schedule aircraft type.", aircraftType, transport.JW_AircraftType);
			}, aircraftType: aircraftType);
		}

		#endregion

		#region OnlineScheduleStatusDescription

		public void TestOnlineScheduleStatusDescription()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport1 = consol1.Transports.AddNew();
			transport1.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			var transport3 = consol1.Transports.AddNew();
			transport3.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Arrived;

			var transport4 = consol1.Transports.AddNew();
			transport4.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Departed;

			var transport5 = consol1.Transports.AddNew();
			transport5.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;

			var transport6 = consol1.Transports.AddNew();
			transport6.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unmatched;

			Factory.Save();

			AssertEquals("Matched", transport1.OnlineScheduleStatusDescription);
			AssertEquals("Partially Matched", transport2.OnlineScheduleStatusDescription);
			AssertEquals("Arrived", transport3.OnlineScheduleStatusDescription);
			AssertEquals("Departed", transport4.OnlineScheduleStatusDescription);
			AssertEquals("Unknown", transport5.OnlineScheduleStatusDescription);
			AssertEquals("Unmatched", transport6.OnlineScheduleStatusDescription);
		}

		#endregion

		public void TestETDAndETAChange_DontCauseModificationToATDorATA()
		{
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			AssertNotNull("IForwardingConsol is convertible to CommonConsol", consol);
			var transportLeg1 = consol.Transports.AddNew();
			var transportLeg2 = consol.Transports.AddNew();

			consol.JK_RL_NKLoadPort = "PGPOM";
			consol.JK_RL_NKDischargePort = "AUSYD";
			transportLeg1.JW_RL_NKLoadPort = "PGPOM";
			transportLeg1.JW_RL_NKDiscPort = "NZWLG";
			transportLeg2.JW_RL_NKLoadPort = "NZWLG";
			transportLeg1.JW_RL_NKDiscPort = "AUSYD";

			var workflowProvider = consol as IWorkflowProvider;
			AssertNotNull("Consol is convertible to IWorkflowProvider", workflowProvider);

			var depTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			depTrigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			depTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.Constants.JW_ATD;
			depTrigger.ReferenceCode = "PGPOM->NZWLG";

			var arvTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			arvTrigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			arvTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.Constants.JW_ATA;
			arvTrigger.ReferenceCode = "NZWLG->AUSYD";

			transportLeg1.JW_ETD = ZDateTime.Today;
			AssertEquals("ATD should not have been updated by milestone processing", ZDateTime.Empty, transportLeg1.JW_ATD);

			transportLeg2.JW_ETA = ZDateTime.Today;
			AssertEquals("ATA should not have been updated by milestone processing", ZDateTime.Empty, transportLeg2.JW_ATA);
		}

		public void TestJW_Vessel_WhenJW_IsLinkedFalse()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "NewCode";
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, transport.JW_VesselScreeningStatus);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)transport).ShouldUpdateScreeningStatus);
		}

		public void TestJW_Vessel_SeaLinked_DefaultCarrierFromVessel()
		{
			var carrier = Factory.New<OrgHeader>();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Visund";
			vessel.RV_OH = carrier.PK;

			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Constants.TransportModes.Sea;

			Transport.JW_Vessel = vessel.RV_FK;
			AssertEquals(carrier.PK, Transport.CarrierPK);

			Transport.CarrierPK = ZGuid.Empty;
			Transport.JW_Vessel = "";
			Transport.JW_IsLinked = false;

			Transport.JW_Vessel = vessel.RV_FK;
			AssertEquals(ZGuid.Empty, Transport.CarrierPK);
		}

		public void TestVesselLabel()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Vessel", Transport.VesselLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Other Info", Transport.VesselLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Vessel", Transport.VesselLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Journey Ref", Transport.VesselLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Storage;
			AssertEquals("Vessel", Transport.VesselLabel);

			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Vessel", Transport.VesselLabel);
		}

		public void TestVoyageLabel()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Flight", Transport.VoyageLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Truck Ref", Transport.VoyageLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Voyage", Transport.VoyageLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Journey Num.", Transport.VoyageLabel);

			Transport.JW_TransportMode = Constants.TransportModes.Storage;
			AssertEquals("Voy./Flight", Transport.VoyageLabel);

			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Voy./Flight", Transport.VoyageLabel);
		}

		public void TestJW_VesselReadOnlyness()
		{
			foreach (string mode in new[] { Constants.TransportModes.Air, Constants.TransportModes.Storage })
			{
				Transport.JW_TransportMode = mode;
				Assert(string.Format("Should be ReadOnly (Mode = {0})", mode), Transport.JW_VesselInfo.ReadOnly);
			}

			var notreadonlyModes = new[]
				{
					Constants.TransportModes.Sea,
					Constants.TransportModes.Road,
					Constants.TransportModes.Rail,
					Constants.TransportModes.Courier,
					Constants.TransportModes.Other,
				};

			foreach (string mode in notreadonlyModes)
			{
				Transport.JW_TransportMode = mode;
				Assert(string.Format("Should not be ReadOnly (Mode = {0})", mode), !Transport.JW_VesselInfo.ReadOnly);
			}
		}

		public void TestVessel_GetsActiveVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_IsActive = true;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "Voyage";

			Factory.Save();

			var attachedVessel = transport.Vessel;

			AssertNotNull(attachedVessel);
			AssertEquals("The created vessel and the attached vessel are the same", vessel.RV_Code, attachedVessel.RV_Code);
			Assert("The attached vessel is active", attachedVessel.RV_IsActive);
		}

		public void TestVessel_GetsInactiveVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_IsActive = false;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "Voyage";

			Factory.Save();

			var attachedVessel = transport.Vessel;

			AssertNotNull(attachedVessel);
			AssertEquals("The created vessel and the attached vessel are the same", vessel.RV_Code, attachedVessel.RV_Code);
			Assert("The attached vessel is inactive", !attachedVessel.RV_IsActive);
		}

		public void TestTrimVesselAndVoyage()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Constants.TransportModes.Sea;

			Transport.JW_VoyageFlight = "AB123 ";
			AssertEquals("AB123", Transport.JW_VoyageFlight);

			Transport.JW_Vessel = "HMS TRAILING SPACE ";
			AssertEquals("HMS TRAILING SPACE", Transport.JW_Vessel);
		}

		public void TestJW_IsCargoOnlyReadOnly()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, Transport.JW_IsCargoOnlyInfo.ReadOnly);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(true, Transport.JW_IsCargoOnlyInfo.ReadOnly);
		}

		public void TestJW_JX_JV_VoyageType()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(ZString.Empty, Transport.JW_JX_JV_VoyageType);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";

			var sailing = NewSailing(vessel, "123", "AUSYD", "USLAX");
			sailing.Voyage.JV_VoyageType = Constants.VoyageType.MainVoyage;

			Transport.JW_JX = sailing.PK;
			AssertEquals(Constants.VoyageType.MainVoyage, Transport.JW_JX_JV_VoyageType);
			AssertEquals(true, Transport.JW_JX_JV_VoyageTypeInfo.ReadOnly);

			sailing.Voyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			AssertEquals(Constants.VoyageType.SlotVoyage, Transport.JW_JX_JV_VoyageType);
			AssertEquals(true, Transport.JW_JX_JV_VoyageTypeInfo.ReadOnly);

			Transport.JW_IsLinked = false;
			AssertEquals(ZString.Empty, Transport.JW_JX_JV_VoyageType);
			AssertEquals(true, Transport.JW_JX_JV_VoyageTypeInfo.ReadOnly);
		}

		public void TestJW_TransportTypeReadOnly()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Assert("Should not be ReadOnly (Mode = Air)", !Transport.JW_TransportTypeInfo.ReadOnly);
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Assert("Should not be ReadOnly (Mode = Sea)", !Transport.JW_TransportTypeInfo.ReadOnly);
		}

		public void TestJW_TransportType_List()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Codes in list (Mode = Air)", "FL1, FL2, FL3, OTH", Transport.JW_TransportType_List.CodesAsString);

			Transport.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Codes in list (Mode = Road)", "MAI, PRE, ONF, OTH", Transport.JW_TransportType_List.CodesAsString);

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Codes in list (Mode = SEA)", "MAI, PRE, ONF, OTH", Transport.JW_TransportType_List.CodesAsString);

			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Codes in list (Mode = Rail)", "MAI, PRE, ONF, OTH", Transport.JW_TransportType_List.CodesAsString);

			Transport.JW_TransportMode = Constants.TransportModes.Storage;
			AssertEquals("Codes in list (Mode = Storage)", "", Transport.JW_TransportType_List.CodesAsString);

			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Codes in list (Mode = Rail)", "PRE, ONF", Transport.JW_TransportType_List.CodesAsString);
		}

		public void TestIsTransportMode()
		{
			Transport.JW_TransportMode = "";
			AssertTransportModeFlags("JW_TransportMode not set", false, false, false, false);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertTransportModeFlags("JW_TransportMode not set", true, false, false, false);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertTransportModeFlags("JW_TransportMode not set", false, true, false, false);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertTransportModeFlags("JW_TransportMode not set", false, false, true, false);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			AssertTransportModeFlags("JW_TransportMode not set", false, false, false, true);
		}

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = transport.CarrierServiceLevel; });
		}

		public void TestCarrierServiceLevels()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			OrgCarrierServiceLevel serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XXX";
			serviceLevel.PL_CarrierServiceLevelDescription = "XXX";
			serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "DEF";
			serviceLevel.PL_CarrierServiceLevelDescription = "DEF";

			AssertContainsExactElementsInAnyOrder("Service Levels", new ZString[] { "STD", "XXX", "DEF" }, transport.CarrierServiceLevel_List.Select(x => x.PL_Code));

			carrier.Delete();

			AssertContainsExactElementsInAnyOrder("Service Levels", new ZString[] { "STD" }, transport.CarrierServiceLevel_List.Select(x => x.PL_Code));
		}

		#endregion

		public void TestVesselFieldType()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Use a code find box for sea transports", nameof(FieldType.TextCodeFindBox), Transport.JW_VesselFieldType);

			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Use a text field for other transports", nameof(FieldType.Text), Transport.JW_VesselFieldType);

			Transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Use a text field for IWT transports", nameof(FieldType.TextCodeFindBox), Transport.JW_VesselFieldType);
		}

		public void TestGetParametersForEvent()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertParameterEvents(transport, Events.Arrival);
			AssertParameterEvents(transport, Events.Departure);
			AssertParameterEvents(transport, Events.CutOffDate);
			AssertParameterEvents(transport, Events.CargoAvailable);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertParameterEvents(transport, Events.Arrival);
			AssertParameterEvents(transport, Events.Departure);
		}

		void AssertParameterEvents(Transport transport, Event eventType)
		{
			var depParameters = transport.GetParametersForEvent(eventType);
			AssertEquals("Parameter : FAC", true, depParameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.Facility));
			AssertEquals("Parameter : LOC", true, depParameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.Location));
			if (transport.IsAir)
			{
				AssertEquals("Parameter : VFL", true, depParameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber));
				AssertEquals("Parameter : FDT", true, depParameters.Keys.Contains(EventConstants.EventReferenceParameters.Codes.FlightDate));
			}
		}

		public void TestGetParametersForEvent_FlightDateFormat()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_ETD = new ZDateTime(2016, 5, 2);
			transport.JW_ETA = new ZDateTime(2016, 5, 10);

			var parameters = transport.GetParametersForEvent(Events.Arrival);
			AssertEquals("2016-05-10", parameters[EventConstants.EventReferenceParameters.Codes.FlightDate]);

			parameters = transport.GetParametersForEvent(Events.Departure);
			AssertEquals("2016-05-02", parameters[EventConstants.EventReferenceParameters.Codes.FlightDate]);
		}

		public void TestIsFlightDateMatched()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ImportSailing.PK;
			Transport.JW_ETA = new ZDateTime(2016, 5, 1);
			Transport.JW_ETD = new ZDateTime(2016, 5, 2);

			AssertEquals(true, Transport.IsFlightDateMatched(true, new ZDate(2016, 5, 1)));
			AssertEquals(false, Transport.IsFlightDateMatched(true, new ZDate(2016, 5, 2)));

			AssertEquals(false, Transport.IsFlightDateMatched(false, new ZDate(2016, 5, 1)));
			AssertEquals(true, Transport.IsFlightDateMatched(false, new ZDate(2016, 5, 2)));

			AssertEquals(false, Transport.IsFlightDateMatched(true, ZDate.Empty));
			AssertEquals(false, Transport.IsFlightDateMatched(false, ZDate.Empty));
		}

		public void TestDepartureOrgs()
		{
			string uNLOCOToFilter = "Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";

			OrgHeaderCollection collection;

			collection = Transport.DepartureAddressOrgs;
			AssertEquals("no load port set yet (UNLOCOToFilter)", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(uNLOCOToFilter));

			Transport.JW_RL_NKLoadPort = "AUBNE";
			collection = Transport.DepartureAddressOrgs;
			AssertEquals("load port set (UNLOCOToFilter)", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(uNLOCOToFilter));
			AssertEquals("load port set (UNLOCOToFilter)", "AUBNE", collection.FilterBusinessObjectDefaults[uNLOCOToFilter].Value);
		}

		public void TestArrivalOrgs()
		{
			string uNLOCOToFilter = "Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";

			OrgHeaderCollection collection;

			collection = Transport.ArrivalAddressOrgs;
			AssertEquals("no load port set yet (UNLOCOToFilter)", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(uNLOCOToFilter));

			Transport.JW_RL_NKDiscPort = "AUBNE";
			collection = Transport.ArrivalAddressOrgs;
			AssertEquals("load port set (UNLOCOToFilter)", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(uNLOCOToFilter));
			AssertEquals("load port set (UNLOCOToFilter)", "AUBNE", collection.FilterBusinessObjectDefaults[uNLOCOToFilter].Value);
		}

		public void TestRefVessels_ShouldHaveBargeFilter_WhenTransportModeIsInlandWaterway()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(0, Transport.RefVessels.FilterBusinessObjectDefaults.Count);

			Transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(1, Transport.RefVessels.FilterBusinessObjectDefaults.Count);
		}

		public void TestIsCharterSet_AnyValue_ShouldUpdateValueOnFoundVoyage()
		{
			Transport.JW_IsCharter = false;
			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_Vessel = TestVessel1.RV_FK;
			Transport.JW_VoyageFlight = "Blah";
			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = OverseasPort;
			AssertNotNull(Transport.Sailing);
			AssertEquals("Sailing should not be charter", false, Transport.Sailing.Voyage.JV_IsChartered);

			var voyage = Transport.Sailing.Voyage.PK;
			Transport.JW_IsCharter = true;
			AssertNotNull(Transport.Sailing);
			AssertEquals("Should not create a new voyage", voyage, Transport.Sailing.Voyage.PK);
			AssertEquals("Sailing should be charter", true, Transport.Sailing.Voyage.JV_IsChartered);
		}

		public void TestIsCharterSet_WhenAirTransportIsLinked_IsPersisted()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "QF800";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(11);

			voyage.GenerateSailings();

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Charter;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsCharter = false;
			transport.JW_IsLinked = true;
			transport.JW_VoyageFlight = "FF1234";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_JX_JV_RegistrationNo = "666";
			Factory.Save();
			AssertNotNull(transport.Sailing);

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<CommonConsol>(consol.PK);
			AssertEquals("Sailing should not be charter", false, consol2.Transports[0].JW_IsCharter);
			AssertEquals("Sailing should not be charter", false, consol2.Transports[0].Sailing.Voyage.JV_IsChartered);
			consol2.Transports[0].JW_IsCharter = true;
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var consol3 = factory3.Load<CommonConsol>(consol.PK);
			AssertEquals("Sailing should be charter", true, consol3.Transports[0].JW_IsCharter);
			AssertEquals("Sailing should be charter", true, consol2.Transports[0].Sailing.Voyage.JV_IsChartered);
		}

		public void TestAircraftTypeSet_AnyValue_ShouldUpdateValueOnFoundVoyage()
		{
			Transport.JW_AircraftType = ZString.Empty;
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ExportSailing.PK;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Transport.JW_Vessel = TestVessel1.RV_FK;
			Transport.JW_VoyageFlight = "Blah";
			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = OverseasPort;
			AssertNotNull(Transport.Sailing);
			AssertEquals("Sailing JV_AircraftType should be empty", ZString.Empty, Transport.Sailing.Voyage.JV_AircraftType);

			var voyage = Transport.Sailing.Voyage.PK;
			Transport.JW_AircraftType = "E90";
			AssertNotNull(Transport.Sailing);
			AssertEquals("Should not create a new voyage", voyage, Transport.Sailing.Voyage.PK);
			AssertEquals("Sailing JV_AircraftType should be updated", "E90", Transport.Sailing.Voyage.JV_AircraftType);
		}

		public void TestSailingManagerNotLeftDirtyAfterChangingAPropertyValue()
		{
			Transport.JW_IsLinked = true;
			AssertNotDirty("Set to linked", Transport);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertNotDirty("Setting Transport Mode to Air", Transport);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNotDirty("Setting Transport Mode to Sea", Transport);

			Transport.JW_RL_NKLoadPort = HomePort;
			AssertNotDirty("Setting Load to HomePort", Transport);

			Transport.JW_RL_NKLoadPort = OverseasPort;
			AssertNotDirty("Setting Load to OverseasPort", Transport);

			Transport.JW_RL_NKDiscPort = HomePort;
			AssertNotDirty("Setting Discharge to HomePort", Transport);

			Transport.JW_RL_NKDiscPort = OverseasPort;
			AssertNotDirty("Setting Discharge to OverseasPort", Transport);

			Transport.JW_Vessel = TestVessel1.RV_FK;
			AssertNotDirty("Setting Vessel to TestVessel1", Transport);

			Transport.JW_Vessel = TestVessel2.RV_FK;
			AssertNotDirty("Setting Vessel to TestVessel2", Transport);

			Transport.JW_VoyageFlight = "Blah";
			AssertNotDirty("Setting VoyageFlight to Blah", Transport);

			Transport.JW_VoyageFlight = "Npaj";
			AssertNotDirty("Setting VoyageFlight to Npaj", Transport);

			Transport.JW_ETD = ZDateTime.Today;
			AssertNotDirty("Setting ETD to Today", Transport);

			Transport.JW_ETD = ZDateTime.Today.AddDays(-1);
			AssertNotDirty("Setting ETD to Yesterday", Transport);

			Transport.JW_ETA = ZDateTime.Today;
			AssertNotDirty("Setting ETA to Today", Transport);

			Transport.JW_ETA = ZDateTime.Today.AddDays(1);
			AssertNotDirty("Setting ETA to Tomorrow", Transport);

			Transport.JW_JX_Load_ETA = ZDateTime.Today.AddDays(-1);
			AssertNotDirty("Setting Load ETA to yesterday", Transport);

			Transport.JW_JX_Load_ETA = ZDateTime.Today.AddDays(-7);
			AssertNotDirty("Setting Load ETA to last week", Transport);

			Transport.JW_JX_Load_ATA = ZDateTime.Today.AddDays(-1);
			AssertNotDirty("Setting Load ATA to yesterday", Transport);

			Transport.JW_JX_Load_ATA = ZDateTime.Today.AddDays(-7);
			AssertNotDirty("Setting Load ATA to last week", Transport);

			Transport.JW_JX = ExportSailing.PK;
			AssertNotDirty("Setting Sailing", Transport);

			Transport.JW_TransportMode = "";
			AssertNotDirty("Clearing TransportMode", Transport);
		}

		public void TestDistanceReadOnly()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = Transport.ParentType;

			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Distance should be readonly", true, transport.JW_DistanceInfo.ReadOnly);
			AssertEquals("DistanceUnit should be readonly", true, transport.JW_DistanceUnitInfo.ReadOnly);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Distance should be readonly", true, transport.JW_DistanceInfo.ReadOnly);
			AssertEquals("DistanceUnit should be readonly", true, transport.JW_DistanceUnitInfo.ReadOnly);

			transport.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Distance should not be readonly", false, transport.JW_DistanceInfo.ReadOnly);
			AssertEquals("DistanceUnit should not be readonly", false, transport.JW_DistanceUnitInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			Transport transport = Factory.New<Transport>();
			Assert(transport.IsPersistent);
			transport.ReadOnly = false;
			Assert("transport should be deletable", transport.CanDelete);

			transport.ReadOnly = true;
			Assert("transport should be deletable", transport.CanDelete);

			transport.MakeNonPersistent();
			Assert("transport should be non persistent", !transport.IsPersistent);
			Assert("transport should not be deletable", !transport.CanDelete);
			AssertEquals("correct reason for not being able to delete", "This leg is read-only and cannot be deleted.", transport.ReasonForNotAbleToDelete);

			transport.ReadOnly = false;
			Assert("transport should not be deletable", transport.CanDelete);
		}

		public void TestValuePropergationOnMiscFactoryOperations_Vessel()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_Vessel);
		}

		public void TestValuePropergationOnMiscFactoryOperations_VoyageFlight()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_VoyageFlight);
		}

		public void TestValuePropergationOnMiscFactoryOperations_Load()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_RL_NKLoadPort);
		}

		public void TestValuePropergationOnMiscFactoryOperations_Discharge()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_RL_NKDiscPort);
		}

		public void TestValuePropergationOnMiscFactoryOperations_ETD()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_ETD);
		}

		public void TestValuePropergationOnMiscFactoryOperations_ETA()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_ETA);
		}

		public void TestValuePropergationOnMiscFactoryOperations_STA()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_STA);
		}

		public void TestValuePropergationOnMiscFactoryOperations_STD()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_STD);
		}

		public void TestValuePropergationOnMiscFactoryOperations_ATD()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_ATD);
		}

		public void TestValuePropergationOnMiscFactoryOperations_ATA()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(JobConsolTransportSchema.Constants.JW_ATA);
		}

		public void TestValuePropergationOnMiscFactoryOperations_RegistrationNo()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_JX_JV_RegistrationNo);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DocsCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DocumentaryCutOff);
		}

		public void TestValuePropagationOnMiscFactoryOperations_VGMCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_VGMCutOff);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DepotReceivalCommences()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DepotReceivalCommences);
		}

		public void TestValuePropergationOnMiscFactoryOperations_CTOReceivalCommences()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_TerminalReceivalCommences);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DepotCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DepotCutOff);
		}

		public void TestValuePropergationOnMiscFactoryOperations_CTOCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_TerminalCutOff);
		}

		public void TestValuePropergationOnMiscFactoryOperations_AvailabilityDate()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_TerminalAvailabilityDate);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DepotAvailabilityDate()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DepotAvailabilityDate);
		}

		public void TestValuePropergationOnMiscFactoryOperations_StorageDate()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_TerminalStorageDate);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DepotStorageDate()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DepotStorageDate);
		}

		public void TestValuePropergationOnMiscFactoryOperations_DepartureLocation()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_OA_DepartureLocation, typeof(OrgAddress));
		}

		public void TestValuePropergationOnMiscFactoryOperations_ArrivalLocation()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_OA_ArrivalLocation, typeof(OrgAddress));
		}

		public void TestValuePropergationOnMiscFactoryOperations_Load_ETA()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Transport.Schema.JW_JX_Load_ETA);
		}

		public void TestValuePropergationOnMiscFactoryOperations_Load_ATA()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Transport.Schema.JW_JX_Load_ATA);
		}

		public void TestValuePropagationOnMiscFactoryOperations_EmptyReceivalCommences()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_EmptyReceivalCommences);
		}

		public void TestValuePropagationOnMiscFactoryOperations_EmptyCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_EmptyCutOff);
		}

		public void TestValuePropagationOnMiscFactoryOperations_DGReceivalCommences()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DGReceivalCommences);
		}

		public void TestValuePropagationOnMiscFactoryOperations_DGCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DGCutOff);
		}

		public void TestValuePropagationOnMiscFactoryOperations_ReeferReceivalCommences()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_ReeferReceivalCommences);
		}

		public void TestValuePropagationOnMiscFactoryOperations_ReeferCutOff()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_ReeferCutOff);
		}

		public void TestValuePropagationOnMiscFactoryOperations_ServiceString()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_ServiceString);
		}

		public void TestValuePropagationOnMiscFactoryOperations_ArrivalPortRouteId()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_ArrivalPortRouteId);
		}

		public void TestValuePropagationOnMiscFactoryOperations_DeparturePortRouteId()
		{
			GenericValuePropergationOnMiscFactoryOperationsTest(Business.Transport.Schema.JW_DeparturePortRouteId);
		}

		void GenericValuePropergationOnMiscFactoryOperationsTest(string transportPropertyName, Type typeForConstraint = null, params IZType[] testValues)
		{
			var sailingProperty = GetSailingInfo(ExportSailing, transportPropertyName);

			var label = "(" + sailingProperty.Name + " => " + transportPropertyName + ")";
			var value1 = testValues.Length > 0 ? testValues[0] : GetValueForPopulation(sailingProperty, 1, typeForConstraint);
			var value2 = testValues.Length > 1 ? testValues[1] : GetValueForPopulation(sailingProperty, 2, typeForConstraint);

			sailingProperty.Value = value1;
			Factory.Save();

			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_JX = ExportSailing.PK;
			AssertEquals("Value should be available once the sailing is set." + label, value1, Transport[transportPropertyName]);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transportInAnotherFactory = anotherFactory.Load<Transport>(Transport.PK);
			transportInAnotherFactory.ParentType = Transport.ParentType;

			AssertEquals("JW_JX", Transport.JW_JX, transportInAnotherFactory.JW_JX);
			AssertEquals("Value should be available on first read after loading: " + label, value1, transportInAnotherFactory[transportPropertyName]);

			sailingProperty.Value = value2;
			Factory.Save();
			AssertEquals("should have updated with the data refresh bus: " + label, value2, transportInAnotherFactory[transportPropertyName]);
		}

		public void TestJW_JX_DepartOrArriveReference()
		{
			Transport.JW_IsLinked = true;

			ImportSailing.Destination.JB_ArrivalReference = "arrive";
			Transport.JW_JX = ImportSailing.PK;
			AssertEquals("Arrival reference for imports", "arrive", Transport.JW_JX_DepartOrArriveReference);

			ExportSailing.Origin.JA_DepartReference = "depart";
			Transport.JW_JX = ExportSailing.PK;
			AssertEquals("Departure reference for exports", "depart", Transport.JW_JX_DepartOrArriveReference);
		}

		public void TestJW_JX_DepartOrArriveBerth()
		{
			Transport.JW_IsLinked = true;

			ImportSailing.Destination.JB_Berth = "arrive";
			Transport.JW_JX = ImportSailing.PK;
			AssertEquals("Arrival berth for imports", "arrive", Transport.JW_JX_DepartOrArriveBerth);

			ExportSailing.Origin.JA_Berth = "depart";
			Transport.JW_JX = ExportSailing.PK;
			AssertEquals("Departure berth for exports", "depart", Transport.JW_JX_DepartOrArriveBerth);
		}

		public void TestJWIsLinkedSetsJW_JX_IsPublishedToFalse()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			Transport.JW_IsLinked = true;
			Transport.JW_JX = sailing.PK;

			sailing.JX_IsPublished = true;

			AssertEquals(true, Transport.JW_JX_IsPublished);

			Transport.JW_IsLinked = false;

			AssertEquals(false, Transport.JW_JX_IsPublished);
		}

		public void TestJW_JX_IsPublished()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			Transport.JW_JX = sailing.PK;
			AssertEquals(Transport.JW_JX_IsPublished, Transport.Sailing.JX_IsPublished);

			Transport.JW_JX_IsPublished = !Transport.JW_JX_IsPublished;

			AssertEquals(Transport.JW_JX_IsPublished, Transport.Sailing.JX_IsPublished);
		}

		public void TestInitialReadShouldNotClobberFreshChanges()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ExportSailing1.PK;
			Factory.Save();

			CheckProperty(Transport.JW_RL_NKLoadPortInfo, "ZZZZZ");
			CheckProperty(Transport.JW_RL_NKDiscPortInfo, "ZZZZZ");
			CheckProperty(Transport.JW_VoyageFlightInfo, "ZZZZZ");
			CheckProperty(Transport.JW_VesselInfo, "ZZZZZ");

			CheckProperty(Transport.JW_ETDInfo, ZDateTime.MinSmallDateTimeValue);
			CheckProperty(Transport.JW_ETAInfo, ZDateTime.MinSmallDateTimeValue);
			CheckProperty(Transport.JW_ATDInfo, ZDateTime.MinSmallDateTimeValue);
			CheckProperty(Transport.JW_ATAInfo, ZDateTime.MinSmallDateTimeValue);
		}

		public void TestJW_IsLinked_SetValueTrue()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_IsLinked = true;
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, transport.JW_VesselScreeningStatus);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)transport).ShouldUpdateScreeningStatus);
		}

		public void TestJW_IsLinked_SetValueTrue_ShouldRaiseError_WhenTransportModeIsInlandWaterway()
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			transport.JW_IsLinked = true;
			AssertHasError(transport.JW_IsLinkedInfo, "Leg cannot be linked when transport mode is Inland Waterway.");

			transport.JW_IsLinked = false;
			AssertNoError(transport.JW_IsLinkedInfo, "Leg cannot be linked when transport mode is Inland Waterway.");

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			AssertNoError(transport.JW_IsLinkedInfo, "Leg cannot be linked when transport mode is Inland Waterway.");

			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertHasError(transport.JW_TransportModeInfo, "Leg cannot be linked when transport mode is Inland Waterway.");
		}

		public void TestJW_IsLinkedShouldNotValidateMovedFields()
		{
			AssertNoNotifications("precondition: validation should not have run yet", Transport);

			Transport.JW_IsLinked = true;
			AssertNoNotifications("setting JW_IsLinked should only validate JW_IsLinked", Transport);

			Transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertHasErrors("if there is no error here after validating then there is something wrong with this test.", Transport.JW_RL_NKLoadPortInfo);
		}

		public void TestJW_IsLinked_SailingInfoRemainUnchanged()
		{
			var forwardingConsol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingConsol)));
			forwardingConsol.JK_TransportMode = Constants.TransportModes.Sea;

			var eta = new ZDateTime(2012, 01, 01);
			var etd = new ZDateTime(2012, 01, 03);

			var transport = forwardingConsol.Transports[0];
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "Blah";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;

			Factory.Save();

			AssertEquals(true, transport.JW_IsLinked);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var forwardingConsolInNewFactory = newFactory.Load<CommonConsol>(forwardingConsol.PK);
			var transportInNewFactory = forwardingConsolInNewFactory.Transports[0];

			transportInNewFactory.JW_IsLinked = false;

			AssertEquals("Blah", transportInNewFactory.JW_VoyageFlight);
			AssertEquals(TestVessel1.RV_FK, transportInNewFactory.JW_Vessel);
			AssertEquals(HomePort, transportInNewFactory.JW_RL_NKLoadPort);
			AssertEquals(OverseasPort, transportInNewFactory.JW_RL_NKDiscPort);
			AssertEquals(etd, transportInNewFactory.JW_ETD);
			AssertEquals(eta, transportInNewFactory.JW_ETA);
		}

		public void TestUnlinkingDeletesUnsavedSailing()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];

			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "Blah";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = ZDateTime.Today.AddDays(5);

			ZGuid sailingPK = transport.JW_JX;
			AssertNotNull("precondition: should have created a sailing", Factory.Load(typeof(JobSailing), sailingPK));

			transport.JW_IsLinked = false;
			AssertNull("Sailing should be deleted", Factory.Load(typeof(JobSailing), sailingPK));
			AssertEquals("JW_JX should now be empty", ZGuid.Empty, transport.JW_JX);
		}

		public void TestUnlinkingDoesNotOverrideConsolCarrierFromVesselCarrier()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");

			var vesselProvider = Factory.NewWithValidTestData<OrgHeader>();

			var refVessel = Factory.NewWithValidTestData<RefVessel>();
			refVessel.RV_OH = vesselProvider.PK;

			var voyage = helper.CreateSeaVoyage(refVessel.RV_Code, "123", carrier.PK, "AUSYD", "NZAKL");

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = true;
			mainTransport.JW_JX = voyage.Sailings[0].PK;
			AssertEquals("Consol carrier defaulted to sailing carrier", carrier.PK, consol.ShippingLinePK);

			mainTransport.JW_IsLinked = false;
			AssertEquals("Consol carrier remains after unlinking", carrier.PK, consol.ShippingLinePK);
		}

		public void TestLinkingOverrideConsolCarrierFromExistingSailing()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");

			var vesselProvider = Factory.NewWithValidTestData<OrgHeader>();

			var refVessel = Factory.NewWithValidTestData<RefVessel>();
			refVessel.RV_OH = vesselProvider.PK;

			var voyage = helper.CreateSeaVoyage(refVessel.RV_Code, "123", carrier.PK, "AUSYD", "NZAKL");
			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = false;
			mainTransport.JW_VoyageFlight = "123";
			mainTransport.JW_Vessel = refVessel.RV_Code;

			AssertEquals("Consol carrier defaulted to vessel shipping provider", vesselProvider.PK, consol.ShippingLinePK);
			mainTransport.JW_IsLinked = true;
			AssertEquals("Consol carrier is overriden to sailing carrier after linking", carrier.PK, consol.ShippingLinePK);
		}

		public void TestLinkingOverrideConsolCarrierFromNewSailing()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");
			var vesselProvider = Factory.NewWithValidTestData<OrgHeader>();
			var refVessel = Factory.NewWithValidTestData<RefVessel>();
			refVessel.RV_OH = vesselProvider.PK;

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = false;
			mainTransport.JW_VoyageFlight = "123";
			mainTransport.JW_Vessel = refVessel.RV_Code;

			AssertEquals("Consol carrier defaulted to vessel shipping provider", vesselProvider.PK, consol.ShippingLinePK);
			mainTransport.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			AssertEquals("Consol carrier remains when transport carrier changes", vesselProvider.PK, consol.ShippingLinePK);
			mainTransport.JW_IsLinked = true;
			AssertEquals("Consol carrier is overriden to sailing carrier after linking", carrier.PK, consol.ShippingLinePK);
		}

		public void TestChangingVesselOnUnlinkedTransportDoesNotOverrideExistingConsolCarrier()
		{
			var vesselProvider1 = Factory.NewWithValidTestData<OrgHeader>();
			var refVessel1 = Factory.NewWithValidTestData<RefVessel>();
			refVessel1.RV_OH = vesselProvider1.PK;

			var vesselProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			var refVessel2 = Factory.NewWithValidTestData<RefVessel>();
			refVessel2.RV_OH = vesselProvider2.PK;

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = false;
			mainTransport.JW_Vessel = refVessel1.RV_Code;

			AssertEquals("Consol carrier defaulted to vessel shipping provider", vesselProvider1.PK, consol.ShippingLinePK);
			mainTransport.JW_Vessel = refVessel2.RV_Code;
			AssertEquals("Consol carrier does not re-default when vessel changes", vesselProvider1.PK, consol.ShippingLinePK);
		}

		public void TestUnlinkingDoesNotClearTransportInfos()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];

			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "N95";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = ZDateTime.Today.AddDays(5);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var sailingPK = transport.JW_JX;
			AssertNotNull("precondition: should have created a sailing", Factory.Load(typeof(JobSailing), sailingPK));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolInNewFac = newFactory.Load<CommonConsol>(consol.PK);
			var transportInNewFac = consolInNewFac.Transports[0];
			transportInNewFac.JW_IsLinked = false;

			CombineAssertions("Unlinked transport infos", () =>
			{
				AssertEquals("Vessel", TestVessel1.RV_FK, transportInNewFac.JW_Vessel);
				AssertEquals("Voyage", "N95", transportInNewFac.JW_VoyageFlight);
				AssertEquals("Load Port", HomePort, transportInNewFac.JW_RL_NKLoadPort);
				AssertEquals("Discharge Port", OverseasPort, transportInNewFac.JW_RL_NKDiscPort);
				AssertEquals("ETD", ZDateTime.Today.AddDays(5), transportInNewFac.JW_ETD);
			});

			AssertNoExceptionThrown("Save is ok", newFactory.Save);
		}

		public void TestOtherFieldsRemainUnchangedWhenJW_IsLinkedChanges()
		{
			FreightDataRegistry.Instance.CargoOnlyVoyageDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Transport.JW_IsLinked = true;
			Transport.FillWithValidTestData(TestBusinessObjectKind.All & ~TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply, Array.Empty<System.ComponentModel.PropertyDescriptor>());

			Transport.Factory.Save();

			ZPropertyInfo[] infos = GetPercistedPropertiesThatShouldStayTheSameWhenIsLinkedChanges(Transport);
			Dictionary<string, IZType> oldValues = RecordPropertyValues(infos);

			AssertPropertyValues("precondition", oldValues, infos);

			Transport.JW_IsLinked = false;
			AssertPropertyValues("JW_IsLinked set to false", oldValues, infos);

			AssertCellEquals(JobConsolTransportSchema.Constants.JW_JX, DBNull.Value, Transport);

			Transport.JW_IsLinked = true;
			AssertPropertyValues("JW_IsLinked set to true", oldValues, infos);

			AssertCellEquals(JobConsolTransportSchema.Constants.JW_RL_NKLoadPort, "", Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_RL_NKDiscPort, "", Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_Vessel, "", Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_VoyageFlight, "", Transport);

			AssertCellEquals(JobConsolTransportSchema.Constants.JW_ETD, DBNull.Value, Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_ETA, DBNull.Value, Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_ATD, DBNull.Value, Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_ATA, DBNull.Value, Transport);

			AssertCellEquals(JobConsolTransportSchema.Constants.JW_OA_DepartureLocation, DBNull.Value, Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_OA_ArrivalLocation, DBNull.Value, Transport);
			AssertCellEquals(JobConsolTransportSchema.Constants.JW_OA_CarrierAddress, DBNull.Value, Transport);
		}

		public void TestIsLinkedUntickedOnSeaTransport_CarrierAddressIsUnchanged()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;
			Transport.JW_OA_CarrierAddress = address.PK;

			Transport.JW_IsLinked = false;

			AssertEquals(address, Transport.CarrierAddress);
		}

		public void TestIsLinkedTickedOnSeaTransport_CarrierAddressIsUnchanged()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = false;
			Transport.JW_OA_CarrierAddress = address.PK;

			Transport.JW_IsLinked = true;

			AssertEquals(address, Transport.CarrierAddress);
		}

		public void TestIsLinkedUntickedOnNonSeaTransport_CarrierAddressIsUnchanged()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_IsLinked = true;
			Transport.JW_OA_CarrierAddress = address.PK;

			Transport.JW_IsLinked = false;

			AssertEquals(address, Transport.CarrierAddress);
		}

		public void TestIsLinkedTickedOnNonSeaTransport_CarrierAddressIsUnchanged()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_IsLinked = false;
			Transport.JW_OA_CarrierAddress = address.PK;

			Transport.JW_IsLinked = true;

			AssertEquals(address, Transport.CarrierAddress);
		}

		public void TestWhenNonKeyDetailsChange_DocsCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DocumentaryCutOff);
		}

		public void TestWhenNonKeyDetailsChange_VGMCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_VGMCutOff);
		}

		public void TestWhenNonKeyDetailsChange_DepotReceivalCommences()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DepotReceivalCommences);
		}

		public void TestWhenNonKeyDetailsChange_CTOReceivalCommences()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_TerminalReceivalCommences);
		}

		public void TestWhenNonKeyDetailsChange_DepotCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DepotCutOff);
		}

		public void TestWhenNonKeyDetailsChange_CTOCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_TerminalCutOff);
		}

		public void TestWhenNonKeyDetailsChange_AvailabilityDate()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_TerminalAvailabilityDate);
		}

		public void TestWhenNonKeyDetailsChange_DepotAvailabilityDate()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DepotAvailabilityDate);
		}

		public void TestWhenNonKeyDetailsChange_StorageDate()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_TerminalStorageDate);
		}

		public void TestWhenNonKeyDetailsChange_DepotStorageDate()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DepotStorageDate);
		}

		public void TestWhenNonKeyDetailsChange_ATD()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ATD);
		}

		public void TestWhenNonKeyDetailsChange_ATA()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ATA);
		}

		public void TestWhenNonKeyDetailsChange_DepartureLocation()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_OA_DepartureLocation, typeof(OrgAddress));
		}

		public void TestWhenNonKeyDetailsChange_ArrivalLocation()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_OA_ArrivalLocation, typeof(OrgAddress));
		}

		public void TestWhenNonKeyDetailsChange_Load_ETA()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_JX_Load_ETA);
		}

		public void TestWhenNonKeyDetailsChange_Load_ATA()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_JX_Load_ATA);
		}

		public void TestWhenNonKeyDetailsChange_EmptyReceivalCommences()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_EmptyReceivalCommences);
		}

		public void TestWhenNonKeyDetailsChange_EmptyCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_EmptyCutOff);
		}

		public void TestWhenNonKeyDetailsChange_DGReceivalCommences()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DGReceivalCommences);
		}

		public void TestWhenNonKeyDetailsChange_DGCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DGCutOff);
		}

		public void TestWhenNonKeyDetailsChange_ReeferReceivalCommences()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ReeferReceivalCommences);
		}

		public void TestWhenNonKeyDetailsChange_ReeferCutOff()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ReeferCutOff);
		}

		public void TestWhenNonKeyDetailsChange_ServiceString()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ServiceString);
		}

		public void TestWhenNonKeyDetailsChange_ArrivalPortRouteId()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_ArrivalPortRouteId);
		}

		public void TestWhenNonKeyDetailsChange_DeparturePortRouteId()
		{
			GenericNonKeyDetailsChangeTest(Transport.Schema.JW_DeparturePortRouteId);
		}

		void GenericNonKeyDetailsChangeTest(string transportPropertyName, Type typeForConstraint = null, params IZType[] testValues)
		{
			var sailingProperty1 = GetSailingInfo(ExportSailing, transportPropertyName);
			var sailingProperty2 = GetSailingInfo(ExportSailing1, transportPropertyName);

			var testValue1 = testValues.Length > 0 ? testValues[0] : GetValueForPopulation(sailingProperty1, 5, typeForConstraint);
			var testValue2 = testValues.Length > 1 ? testValues[1] : GetValueForPopulation(sailingProperty1, 10, typeForConstraint);
			var testValue3 = testValues.Length > 2 ? testValues[2] : GetValueForPopulation(sailingProperty1, 15, typeForConstraint);
			var testValue4 = testValues.Length > 3 ? testValues[3] : GetValueForPopulation(sailingProperty1, 20, typeForConstraint);
			var testValue5 = testValues.Length > 4 ? testValues[4] : GetValueForPopulation(sailingProperty1, 25, typeForConstraint);

			sailingProperty1.Value = testValue1;
			sailingProperty2.Value = testValue2;
			Factory.Save(); // so the sailings are not deleted when JW_JX changes.

			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_JX = ExportSailing.PK;
			AssertEquals("properties proxied through", testValue1, Transport[transportPropertyName]);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			var oldValue = Transport[transportPropertyName];

			Transport.JW_IsLinked = false;
			AssertEquals("properties should not have been cleared when unlinking", oldValue, Transport[transportPropertyName]);

			Transport.JW_IsLinked = true;
			AssertEquals("properties should not have been cleared when relinking", oldValue, Transport[transportPropertyName]);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			Transport.JW_JX = ExportSailing1.PK;
			AssertEquals("properties should represent the new sailing", testValue2, Transport[transportPropertyName]);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			Transport[transportPropertyName] = testValue3;
			AssertEquals("setting the values should still update the sailing", testValue3, sailingProperty2.Value);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			Transport.JW_IsLinked = false;
			AssertEquals("the sailing should be rolled back to last save", testValue2, sailingProperty2.Value);
			AssertEquals("properties should not have been cleared when unlinking", testValue3, Transport[transportPropertyName]);

			Transport[transportPropertyName] = testValue4;
			AssertEquals("the transport was not linked so the sailing should be unaffected", testValue2, sailingProperty2.Value);

			Transport.JW_IsLinked = true;
			AssertEquals("the transport is now linked but the sailing already exists so dont change", testValue2, Transport[transportPropertyName]);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			Transport.JW_IsLinked = false;
			Transport.JW_VoyageFlight = "ZXYXZ";
			Transport[transportPropertyName] = testValue5;
			Transport.JW_IsLinked = true;
			AssertEquals("precondition: should have created a new sailing by Linking", false, Transport.Sailing.IsInDatabase);
			AssertEquals("values should not come from new sailings", testValue5, Transport[transportPropertyName]);
			AssertEquals("properties should be equal", Transport[transportPropertyName], GetSailingInfo(Transport.Sailing, transportPropertyName).Value);

			Transport.JW_Vessel = ExportSailing.Voyage.JV_RV_NKVessel;
			Transport.JW_VoyageFlight = ExportSailing.Voyage.JV_VoyageFlight;
			Transport.JW_RL_NKLoadPort = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			Transport.JW_ETD = ExportSailing.Origin.JA_E_DEP;
			Transport.JW_RL_NKDiscPort = ExportSailing.Destination.JB_RL_NKPortOfDischarge;
			Transport.JW_ETA = ExportSailing.Destination.JB_E_ARV;

			AssertEquals("precondition: should have found ExportSailing", ExportSailing.PK, Transport.JW_JX);
			AssertEquals("should *NOT* have overwritten ExportSailing's values", oldValue, Transport[transportPropertyName]);
		}

		public void TestPropertyChangeSubscriptionNotified_ForSailingNaturalKeyPropertiesWhenLinked_BecauseChangeIsntLoggedAnywhereOtherwise()
		{
			Transport.JW_Vessel = "Vessel";
			Transport.JW_VoyageFlight = "Voyage";
			Transport.JW_RL_NKLoadPort = "MYPKG";
			Transport.JW_RL_NKDiscPort = "AUSYD";
			Transport.JW_OA_CarrierAddress = Factory.New<OrgAddress>().PK;
			Transport.JW_IsLinked = true;

			ZPropertyValueChangedEventArgs lastPropertyChangeEvent = null;
			PropertyChangeSubscription.PropertyChanged += (sender, e) => lastPropertyChangeEvent = e;

			Transport.JW_Vessel = "Vessel2";
			AssertEquals("JW_Vessel change notified while transport linked", JobConsolTransportSchema.JW_Vessel.Name, lastPropertyChangeEvent.Property.Name);

			Transport.JW_VoyageFlight = "Voyage2";
			AssertEquals("JW_Voyage change notified while transport linked", JobConsolTransportSchema.JW_VoyageFlight.Name, lastPropertyChangeEvent.Property.Name);

			Transport.JW_RL_NKLoadPort = "MYPEN";
			AssertEquals("JW_RL_NKLoadPort change notified while transport linked", JobConsolTransportSchema.JW_RL_NKLoadPort.Name, lastPropertyChangeEvent.Property.Name);

			Transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("JW_RL_NKDiscPort change notified while transport linked", JobConsolTransportSchema.JW_RL_NKDiscPort.Name, lastPropertyChangeEvent.Property.Name);

			Transport.JW_OA_CarrierAddress = Factory.New<OrgAddress>().PK;
			AssertEquals("JW_OA_CarrierAddress change notified while transport linked", JobConsolTransportSchema.JW_OA_CarrierAddress.Name, lastPropertyChangeEvent.Property.Name);
		}

		public void TestDatePropergation_ETA()
		{
			GenericDatePropergationTest(Transport.JW_ETAInfo);
		}

		public void TestDatePropergation_ETD()
		{
			GenericDatePropergationTest(Transport.JW_ETDInfo);
		}

		public void TestDatePropergation_ATA()
		{
			GenericDatePropergationTest(Transport.JW_ATAInfo);
		}

		public void TestDatePropergation_ATD()
		{
			GenericDatePropergationTest(Transport.JW_ATDInfo);
		}

		void GenericDatePropergationTest(ZPropertyInfo transportDate)
		{
			ZPropertyInfo sailingDate = GetSailingInfo(ExportSailing, transportDate.Name);

			var date1 = ZDateTime.Now;
			var date2 = date1.AddDays(1);

			Transport.JW_TransportMode = ExportSailing.Voyage.JV_AirSeaRoad;
			Transport.JW_IsLinked = true;
			Transport.JW_JX = ExportSailing.PK;

			transportDate.Value = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, sailingDate.Value);

			transportDate.Value = date1;
			AssertEquals(date1, sailingDate.Value);

			transportDate.Value = date2;
			AssertEquals(date2, sailingDate.Value);

			Factory.Save();

			Transport.JW_IsLinked = false;

			transportDate.Value = date1;

			var expectedValue = date2.ToString("g");
			var actualValue = ((ZDateTime)sailingDate.Value).ToString("g");

			AssertEquals(expectedValue, actualValue);
		}

		public void TestKeyFieldsEditable_Air()
		{
			GenericKeyFieldsEditableTest(Core.Constants.TransportModes.Air);
		}

		public void TestKeyFieldsEditable_Sea()
		{
			GenericKeyFieldsEditableTest(Core.Constants.TransportModes.Sea);
		}

		public void TestKeyFieldsEditable_Road()
		{
			GenericKeyFieldsEditableTest(Core.Constants.TransportModes.Road);
		}

		public void TestKeyFieldsEditable_Rail()
		{
			GenericKeyFieldsEditableTest(Core.Constants.TransportModes.Rail);
		}

		public void GenericKeyFieldsEditableTest(string transportMode)
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();

			transport.JW_TransportMode = transportMode;

			AssertReadOnly(transport.JW_RL_NKLoadPortInfo, false);
			AssertReadOnly(transport.JW_RL_NKDiscPortInfo, false);
			AssertReadOnly(transport.JW_VoyageFlightInfo, false);
			AssertReadOnly(transport.JW_ETAInfo, false);
			AssertReadOnly(transport.JW_ETDInfo, false);

			bool requiresVessel = (transportMode != Core.Constants.TransportModes.Air);
			AssertReadOnly(transport.JW_VesselInfo, !requiresVessel);
		}

		public void TestSavingAnIncompleteTransport()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKLoadPort = HomePort;
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			Transport transportInAnotherFactory = anotherFactory.Load<Transport>(Transport.PK);
			transportInAnotherFactory.ParentType = Transport.ParentType;

			AssertEquals("Should be unlinked to preserve data", false, transportInAnotherFactory.JW_IsLinked);
			AssertEquals("Data should be preserved", HomePort, transportInAnotherFactory.JW_RL_NKLoadPort);
		}

		public void TestSetCalculatedDistance()
		{
			DistanceCalculationRegistry.Instance.DefaultDistanceUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Miles);
			OrgAddress consignorPickupAddress = Factory.New<OrgAddress>();
			consignorPickupAddress.OA_City = "Consignor";
			consignorPickupAddress.OA_Address1 = "Address";

			OrgAddress consigneeDeliveryAddress = Factory.New<OrgAddress>();
			consigneeDeliveryAddress.OA_City = "Consignee";
			consigneeDeliveryAddress.OA_Address1 = "Address";

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			NotificationBuffer notifications = new NotificationBuffer();
			Transport transport = shipment.Transports.AddNew();

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance calculation functionality is only available for Road transport mode.\r\n", notifications.AsString);

			notifications.Clear();
			transport.JW_TransportMode = Constants.TransportModes.Road;
			transport.SetCalculatedDistance(notifications);
			AssertEquals(false, notifications.AsString.Contains("Distance calculation functionality is only available for Road transport mode.\r\n"));

			notifications.Clear();
			transport.JW_RL_NKLoadPort = "USCHI";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance between UNLOCO", new ZDecimal("ChicagoILUnited StatesLos AngelesCAUnited States".Length), transport.JW_Distance);
			AssertEquals(Constants.Length.Miles, transport.JW_DistanceUnit);

			transport.JW_DistanceUnit = Constants.Length.Kilometres;
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance between UNLOCO", new ZDecimal("ChicagoILUnited StatesLos AngelesCAUnited States".Length), transport.JW_Distance);
			AssertEquals(Constants.Length.Kilometres, transport.JW_DistanceUnit);

			transport.JW_DistanceUnit = Constants.Length.Miles;
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance between UNLOCO", new ZDecimal("ChicagoILUnited StatesLos AngelesCAUnited States".Length), transport.JW_Distance);
			AssertEquals(Constants.Length.Miles, transport.JW_DistanceUnit);

			transport.JW_DistanceUnit = "";
			DistanceCalculationRegistry.Instance.DefaultDistanceUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Length.Miles);
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance between UNLOCO", new ZDecimal("ChicagoILUnited StatesLos AngelesCAUnited States".Length), transport.JW_Distance);
			AssertEquals(Constants.Length.Miles, transport.JW_DistanceUnit);

			shipment.ServiceLevel.RS_IsDoorToDoor = true;
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance DoorToDoor - from Consignor to Consignee", new ZDecimal("ConsignorAddressAustraliaConsigneeAddressAustralia".Length), transport.JW_Distance);

			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			transport.SetCalculatedDistance(notifications);
			AssertEquals("Distance between UNLOCO - not main routing", new ZDecimal("ChicagoILUnited StatesLos AngelesCAUnited States".Length), transport.JW_Distance);
		}

		public void TestClone()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = Transport.ParentType;
			transport.JW_TransportType = "Air";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUBNE";
			Transport newTransport = (Transport)transport.Clone();
			AssertEquals("transport.JW_TransportType should be Air", transport.JW_TransportType, newTransport.JW_TransportType);
			AssertEquals("transport.ParentType should be cloned", transport.ParentType, newTransport.ParentType);
			AssertEquals("transport.JW_RL_NKLoadPort should be cloned", "AUSYD", newTransport.JW_RL_NKLoadPort);
			AssertEquals("transport.JW_RL_NKDiscPort should be cloned", "AUBNE", newTransport.JW_RL_NKDiscPort);
			Assert("transport.IsDomestic should be cloned", newTransport.IsDomestic);
		}

		public void TestCloneLinkedTransport()
		{
			CommonShipment shipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport.JW_VoyageFlight = "QF232";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			Transport clonedTransport = (Transport)transport.Clone();
			AssertEquals(Constants.TransportParentTypes.Shipment, clonedTransport.JW_ParentType);
			AssertEquals(shipment.PK, clonedTransport.JW_ParentGUID);
			AssertEquals(Constants.TransportPlanningType.Flight1, clonedTransport.JW_TransportType);
			AssertEquals(true, clonedTransport.JW_IsLinked);
			AssertEquals(Core.Constants.TransportModes.Air, clonedTransport.JW_TransportMode);
			AssertEquals("QF232", clonedTransport.JW_VoyageFlight);
			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("USLAX", transport.JW_RL_NKDiscPort);
			AssertEquals(ZDateTime.Today, transport.JW_ETD);
			AssertEquals(ZDateTime.Today.AddDays(1), transport.JW_ETA);
		}

		public void TestTemplateCopy()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = Transport.ParentType;
			transport.JW_TransportType = "Air";
			transport.JW_Vessel = "AAA";
			transport.JW_VoyageFlight = "BBB";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;
			transport.JW_ATD = ZDateTime.Now;
			transport.JW_ATA = ZDateTime.Now;
			Transport newTransport = transport.TemplateCopy();
			AssertEquals("transport.JW_TransportType should be Air", transport.JW_TransportType, newTransport.JW_TransportType);
			AssertEquals("transport.ParentType should be cloned", transport.ParentType, newTransport.ParentType);
			AssertEquals("transport vessel should be empty", "", newTransport.JW_Vessel);
			AssertEquals("transport voyage should be empty", "", newTransport.JW_VoyageFlight);
			AssertEquals("transport date should be empty", ZDateTime.Empty, newTransport.JW_ETD);
			AssertEquals("transport date should be empty", ZDateTime.Empty, newTransport.JW_ETA);
			AssertEquals("transport date should be empty", ZDateTime.Empty, newTransport.JW_ATD);
			AssertEquals("transport date should be empty", ZDateTime.Empty, newTransport.JW_ATA);
		}

		public void TestTemplateCopy_ExcludesJW_ArrivalPortRouteId_ForIL()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "ILTLV";
			var transport = consol.Transports.AddNew();
			transport.JW_ArrivalPortRouteId = "111";

			var newTransport = transport.TemplateCopy();
			AssertNullOrEmpty("JW_ArrivalPortRouteId should be empty", newTransport.JW_ArrivalPortRouteId);
		}

		[TestDate(2023, 12, 25, 10, 0, 0)]
		public void TestTemplateCopy_UpdateETDandETA()
		{
			var initialFlightTime = ZDateTime.Now;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = ZGuid.NewZGuid().ToString().Substring(0, 16);
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			// Transport has nested ScheduleManager
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "BBB";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_IsLinked = true;
			transport.JW_ETD = initialFlightTime;
			transport.JW_ETA = initialFlightTime;

			Factory.Save();
			var newConsol = consol.TemplateCopy(true, true, false, null);
			var newTransport = newConsol.Transports[0];
			newTransport.JW_VoyageFlight = "CCC";
			newTransport.JW_ETD = initialFlightTime.AddHours(1);
			newTransport.JW_ETA = initialFlightTime.AddHours(3);

			newTransport.JW_ETD = initialFlightTime.AddHours(2);
			newTransport.JW_ETA = initialFlightTime.AddHours(4);

			Factory.Save();
			AssertEquals("New Voyage ETD should change", initialFlightTime.AddHours(2), newTransport.Sailing.Origin.JA_E_DEP);
			AssertEquals("New Voyage ETD should change", initialFlightTime.AddHours(4), newTransport.Sailing.Destination.JB_E_ARV);
		}

		public void TestTemplateCopyExcludeParent()
		{
			var consol = Factory.New<CommonConsol>();
			var consolTransport = consol.Transports.AddNew();

			var newTransport = consolTransport.TemplateCopy();
			Assert(newTransport.JW_ParentGUID.IsEmpty);
		}

		public void TestTemplateCopyToAnotherFactory()
		{
			var consol = Factory.New<CommonConsol>();
			var consolTransport = consol.Transports.AddNew();

			var factory2 = new BusinessObjectFactory();
			var newTransport = consolTransport.TemplateCopy(factory2);
			AssertEquals("Transport should be copied to factory2", factory2, newTransport.Factory);
		}

		public void TestJW_VesselParentNotification()
		{
			DummyTransportParent parent = Factory.New<DummyTransportParent>();
			Transport transport = parent.Transports.AddNew();

			parent.NotifyVesselCalled = 0;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = "aaa";
			AssertEquals(1, parent.NotifyVesselCalled);

			parent.NotifyVesselCalled = 0;
			transport.JW_TransportMode = Constants.TransportModes.Road;
			transport.JW_Vessel = "bbb";
			AssertEquals(0, parent.NotifyVesselCalled);

			transport.JW_TransportMode = Constants.TransportModes.Rail;
			transport.JW_Vessel = "ccc";
			AssertEquals(0, parent.NotifyVesselCalled);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_Vessel = "ddd";
			AssertEquals(0, parent.NotifyVesselCalled);
		}

		public void TestIsDepartureContainerModeFCLorULD()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.ULD;

			var consolTransport = consol.Transports.AddNew();
			var shipmentTransport = shipment.Transports.AddNew();

			Assert(consolTransport.IsDepartureContainerModeFCLorULD);
			Assert(shipmentTransport.IsDepartureContainerModeFCLorULD);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;

			Assert(!consolTransport.IsDepartureContainerModeFCLorULD);
			Assert(!shipmentTransport.IsDepartureContainerModeFCLorULD);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			Assert(consolTransport.IsDepartureContainerModeFCLorULD);
			Assert(shipmentTransport.IsDepartureContainerModeFCLorULD);
		}

		public void TestParentWorkflowProviders_ForConsol()
		{
			CommonConsol forwardingConsol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingConsol)));
			Transport transport = forwardingConsol.Transports.MostInterestingTransport;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			IWorkflowTriggerFieldChangeSource loadedTransport = newFactory.Load<Transport>(transport.PK);

			AssertEquals("ParentWorkflowProviders - consol", 1, loadedTransport.ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProviders - consol", forwardingConsol.GetType(), loadedTransport.ParentWorkflowProviders[0].GetType());
		}

		public void TestParentWorkflowProviders_ForShipment()
		{
			CommonShipment forwardingShipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			Transport transport = forwardingShipment.Transports.AddNew();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			IWorkflowTriggerFieldChangeSource loadedTransport = newFactory.Load<Transport>(transport.PK);

			AssertEquals("ParentWorkflowProviders - shipment", 1, loadedTransport.ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProviders - shipment", forwardingShipment.GetType(), loadedTransport.ParentWorkflowProviders[0].GetType());
		}

		public void TestReportParentTypeNullIssue()
		{
			ErrorReporter.Clear();
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];

			transport.JW_ETD = new ZDateTime(2012, 01, 01);
			transport.JW_ATD = new ZDateTime(2012, 01, 02);
			transport.JW_ETA = new ZDateTime(2012, 01, 03);
			transport.JW_ATA = new ZDateTime(2012, 01, 04);

			Factory.Save();

			object parent = transport.Parent;
			AssertEquals(false, ErrorReporter.LastKeyReported.Contains("WI00033391 International Logistics"));

			try
			{
				Transport.ForceParentNullIssueReporting = true;

				try
				{
					parent = transport.Parent;
				}
				catch (InvalidOperationException)
				{
					AssertEquals(true, ErrorReporter.LastKeyReported.Contains("WI00033391 International Logistics"));
				}

				string parentCollectionsTypes = "";

				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)transport).ParentCollections)
				{
					parentCollectionsTypes = parentCollectionsTypes + collection.GetType() + ", ";
				}

				string message = "PK: " + transport.PK
							+ "\r\nthis.GetType(): " + transport.GetType()
							+ "\r\nJW_ParentGUID: " + transport.JW_ParentGUID
							+ "\r\nJW_ParentType: " + transport.JW_ParentType
							+ "\r\nJW_IsLinked: N"
							+ "\r\nJW_ETD: 1/01/2012 12:00:00 AM"
							+ "\r\nfJW_ETD: "
							+ "\r\nJW_ATD: 2/01/2012 12:00:00 AM"
							+ "\r\nfJW_ATD: "
							+ "\r\nJW_ETA: 3/01/2012 12:00:00 AM"
							+ "\r\nfJW_ETA: "
							+ "\r\nJW_ATA: 4/01/2012 12:00:00 AM"
							+ "\r\nfJW_ATA: "
							+ "\r\nPreviousJW_ETD: 01-Jan-12 00:00:00"
							+ "\r\nPreviousJW_ATD: 02-Jan-12 00:00:00"
							+ "\r\nPreviousJW_ETA: 03-Jan-12 00:00:00"
							+ "\r\nPreviousJW_ATA: 04-Jan-12 00:00:00"
							+ "\r\nHasChanges: " + transport.HasChanges
							+ "\r\nIsInDatabase: " + transport.IsInDatabase
							+ "\r\nParentCollections Types: " + parentCollectionsTypes
							+ "\r\nParentTypeDebugLog: ";

				AssertContains(message, ErrorReporter.LastMessageReported);
			}
			finally
			{
				Transport.ForceParentNullIssueReporting = false;
				ExceptionReporterTestListener.Instance.Clear();
			}

			transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			AlterTransportProxyValues(transport, 1);

			Factory.Save();

			var factory1 = new BusinessObjectFactory();

			var transportFactory = factory1.Load<Transport>(transport.PK);

			AssertNotNull(transport.ParentType);
			AssertNull(transportFactory.ParentType);

			Assert(!transport.JW_ParentGUID.IsEmpty);
			Assert(!transportFactory.JW_ParentGUID.IsEmpty);

			try
			{
				parent = transport.Parent;
			}
			catch (InvalidOperationException)
			{
				AssertEquals("Issue from CheckParentTypeIsSet() should be reported last.", true, ErrorReporter.LastKeyReported.Contains("UnableToCreateOrLoadTransportWithoutParentType"));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestOnUniversalCopyFinish()
		{
			var transport = Factory.New<CommonConsol>().Transports[0];
			var transportRow = ((INeedRow)transport).Row;
			transport.JW_IsLinked = false;

			AssertEquals("Prerequisite - not linked", ZGuid.Empty, transport.JW_JX);
			AssertEquals("Prerequisite - not linked", false, transport.JW_IsLinked);

			transport.OnUniversalCopyFinish();

			AssertEquals("Still should be not linked", ZGuid.Empty, transport.JW_JX);
			AssertEquals("Still should be not linked", false, transport.JW_IsLinked);

			var sailing = Factory.New<JobSailing>();

			transportRow[JobConsolTransportSchema.Constants.JW_JX] = sailing.PK.ToGuid();

			AssertEquals("Unsync data", sailing.PK, transport.JW_JX);
			AssertEquals("Unsync data", false, transport.JW_IsLinked);

			transport.OnUniversalCopyFinish();

			AssertEquals("Should be linked", sailing.PK, transport.JW_JX);
			AssertEquals("Should linked", true, transport.JW_IsLinked);
		}

		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(Transport).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains("IsDomestic", ignoreElementAttributes[0].ElementNames);
		}

		public void TestTransportStatusShouldBePLNWhenTransportModeIsSetToAIR()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = (Transport)consol.Transports.First();
			Assert("precondition: default TransportMode is not Air", consol.TransportMode != Constants.TransportModes.Air);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			Assert("Transport status should be PLN for air transport", transport.JW_Status == Constants.TransportStatus.Planned);

			var transportNew = consol.Transports.AddNew();
			Assert("precondition: New default TransportMode is not Air", transportNew.TransportMode != Constants.TransportModes.Air);
			Assert("precondition: New Transport status is not PLN", transportNew.JW_Status != Constants.TransportStatus.Planned);

			transportNew.JW_TransportMode = Constants.TransportModes.Air;
			Assert("New Transport status should be PLN for air transport", transportNew.JW_Status == Constants.TransportStatus.Planned);
		}

		public void TestTransportStatusShouldBePLNWhenConsolTransportModeIsSetToAIR()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = (Transport)consol.Transports.First();
			Assert("precondition: default TransportMode is not Air", consol.TransportMode != Constants.TransportModes.Air);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			Assert("Transport status should be PLN for air transport", transport.JW_Status == Constants.TransportStatus.Planned);

			var transportNew = consol.Transports.AddNew();
			Assert("New Transport status should be PLN for air transport", transportNew.JW_Status == Constants.TransportStatus.Planned);
		}

		#region IWorkflowTriggerEventSource

		public void TestParentWorkflowProvidersGetter()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			var transport = (IWorkflowTriggerEventSource)consol.Transports.AddNew();
			var actualParents = transport.ParentWorkflowProviders.Select(p => p.PK);
			var expectedParents = new[] { consol.PK };

			AssertContainsExactElementsInAnyOrder("Parents", expectedParents, actualParents);

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			transport = shipment.Transports.AddNew();
			actualParents = transport.ParentWorkflowProviders.Select(p => p.PK);
			expectedParents = Array.Empty<ZGuid>();

			AssertContainsExactElementsInAnyOrder("Parents", expectedParents, actualParents);

			var declaration = (ITransportParent)Factory.New<IBaseJobDeclaration>();
			transport = declaration.Transports.AddNew();
			actualParents = transport.ParentWorkflowProviders.Select(p => p.PK);
			expectedParents = Array.Empty<ZGuid>();

			AssertContainsExactElementsInAnyOrder("Parents", expectedParents, actualParents);
		}

		public void TestParentWorkflowProvider_CanLoadConsolParentWithNullParentType()
		{
			var consolOriginal = (CommonConsol)Factory.New<IForwardingConsol>();
			var transportOriginal = consolOriginal.Transports.AddNew();
			transportOriginal.JW_ParentType = Constants.TransportParentTypes.Consol;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var transport = factory2.Load<Transport>(transportOriginal.PK);

			AssertEquals("Precondition: transport.ParentType", null, transport.ParentType);
			AssertEquals("Precondition: ErrorReporter.TotalErrorCount", 0, ErrorReporter.TotalErrorCount);

			var eventSource = transport as IWorkflowTriggerEventSource;
			var workflowProviders = eventSource.ParentWorkflowProviders;

			AssertEquals("workflowProviders.Length", 1, workflowProviders.Count);
			AssertEquals("workflowProviders[0].GetType().Name", "ForwardingConsol", workflowProviders[0].GetType().Name);
			AssertEquals("workflowProviders[0].PK", consolOriginal.PK, workflowProviders[0].PK);
			AssertEquals("transport.ParentType", null, transport.ParentType);
			AssertEquals("ErrorReporter.TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestParentWorfklowProvider_CannotLoadNonConsolParentWithNullParentType()
		{
			var shipmentOriginal = (CommonShipment)Factory.New<Integration.Agency.IAgencyShipment>();
			var transportOriginal = shipmentOriginal.Transports.AddNew();
			transportOriginal.JW_ParentType = Constants.TransportParentTypes.AgencyShipment;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var transport = factory2.Load<Transport>(transportOriginal.PK);

			AssertEquals("Precondition: transport.ParentType", null, transport.ParentType);
			AssertEquals("Precondition: ErrorReporter.TotalErrorCount", 0, ErrorReporter.TotalErrorCount);

			var eventSource = transport as IWorkflowTriggerEventSource;

			IReadOnlyList<IWorkflowProviderCore> workflowProviders = Array.Empty<IWorkflowProviderCore>();
			AssertNoExceptionThrown("calling IWorkflowTriggerEventSource should not throw an exception when ParentType == null", () => { workflowProviders = eventSource.ParentWorkflowProviders; });

			AssertEquals("workflowProviders.Length", 0, workflowProviders.Count);
			AssertEquals("ErrorReporter.TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("ErrorReporter.LastMessageReported", "WI00033391 International Logistics", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Arrival Location And Departure Location Address Has Correct Property Info

		public void TestArrivalLocationAndDepartureLocationAddressHasCorrectPropertyInfo()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = (Transport)consol.Transports.First();
			transport.JW_IsLinked = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";
			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;

			AssertEquals("Precondition", true, transport.JW_IsLinked && transport.Sailing?.Destination != null);
			AssertEquals(transport.JW_OA_ArrivalLocationForBinding_ZAddress.AddressFKInfo.InnerInfo, transport.JW_OA_ArrivalLocationForBindingInfo);
			AssertEquals(transport.JW_OA_DepartureLocationForBinding_ZAddress.AddressFKInfo.InnerInfo, transport.JW_OA_DepartureLocationForBindingInfo);
		}

		#endregion

		#region ProxyFieldSynchronisation

		public void TestProxiedConcurrencyMerge_SynchronisesBackToTransport()
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			AlterTransportProxyValues(transport, 1);

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var transportFactory1 = factory1.Load<Transport>(transport.PK);
			var transportFactory2 = factory2.Load<Transport>(transport.PK);

			transportFactory1.ParentType = transport.ParentType;
			transportFactory2.ParentType = transport.ParentType;

			AlterTransportProxyValues(transportFactory1, 3);
			AlterTransportProxyValues(transportFactory2, 6);

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert(true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				factory2.Save();
				AssertSynchronisedProxiedProperties(transportFactory2);
			}
		}

		void AlterTransportProxyValues(Transport transport, int offset)
		{
			var today = ZDateTime.Today;

			transport.JW_STD = today.AddDays(offset);
			transport.JW_ETD = today.AddDays(-offset);
			transport.JW_ATD = today.AddDays(offset + 1);

			transport.JW_STA = today.AddDays(offset - 1);
			transport.JW_ETA = today.AddDays(offset + 2);
			transport.JW_ATA = today.AddDays(offset - 2);
		}

		[TestDate(2021, 6, 15, 12, 13, 0)]
		public void TestProxiedConcurrencyMerge_HasWarnings()
		{
			var today = ZDateTime.Today.ToDateTime();
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var transportFactory1 = factory1.Load<Transport>(transport.PK);
			var transportFactory2 = factory2.Load<Transport>(transport.PK);

			transportFactory1.ParentType = transport.ParentType;
			transportFactory2.ParentType = transport.ParentType;

			transportFactory1.JW_ETD = today.AddDays(-3);
			transportFactory2.JW_ETD = today.AddDays(-6);

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert(true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				Assert(transportFactory2.JW_ETDForBindingInfo.HasWarnings());
				var message = $@"Another user (CargoWise Support @ {EnvProxy.Instance.Time.GetLocalTimeFromUtc(ZDateTime.Now.ToDateTime()).ToString("dd MMM yyyy HH:mm:ss")}) has changed this field.
Yours: '{today.AddDays(-6)}', Theirs: '{today.AddDays(-3)}'";
				AssertEquals(message, transportFactory2.JW_ETDForBindingInfo.GetWarnings().GetFirst().Message);
			}
		}

		public void TestProxiedConcurrencyMerge_SailingChangeUnhooksEvents()
		{
			var today = ZDateTime.Today;
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "Vessel";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "Vessel2";

			transport.JW_JX = NewSailing(vessel1, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var transportFactory1 = factory1.Load<Transport>(transport.PK);
			var transportFactory2 = factory2.Load<Transport>(transport.PK);

			transportFactory1.ParentType = transport.ParentType;
			transportFactory2.ParentType = transport.ParentType;

			transportFactory1.JW_ETD = today.AddDays(-3);
			transportFactory2.JW_ETD = today.AddDays(-6);

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert(true);
				transportFactory2.JW_JX = NewSailing(vessel2, "Flight2", AlternateHomePort, OverseasPort2).PK;

				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AssertNotEquals("JW_ETD should not have been merged due to change in Sailing", transportFactory2.JW_ETD, today.AddDays(-3));
			}
		}

		public void TestProxiedConcurrencyMerge_ReloadSailingIfNecessary()
		{
			var today = ZDateTime.Today;
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var consolFactory1 = factory1.Load<CommonConsol>(consol.PK);
			var consolFactory2 = factory2.Load<CommonConsol>(consol.PK);

			var transportFactory1 = consolFactory1.Transports[0];
			var transportFactory2 = consolFactory2.Transports[0];

			transportFactory1.ParentType = transport.ParentType;
			transportFactory2.ParentType = transport.ParentType;

			transportFactory1.JW_ETD = today.AddHours(3);
			transportFactory2.JW_LegOrder = 2;

			consolFactory1.JK_AgentsReference = "12345";
			consolFactory2.JK_AgentsReference = "99999";

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert("Save Concurrency Exception", true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AssertEquals("JW_ETD should have been updated", today.AddHours(3), transportFactory2.JW_ETD);
				AssertEquals("Leg order changes should remain", (ZByte)2, transportFactory2.JW_LegOrder);

				AssertEquals("Sailing should have been reloaded", transportFactory1.Sailing.JX_JA_E_DEP, transportFactory2.Sailing.JX_JA_E_DEP);

				factory2.Save();
				Assert("Save Succeeded", true);
			}
		}

		public void TestProxiedConcurrencyMerge_ScheduleAndTransportChangesAreCorrectlyMerged()
		{
			var today = ZDateTime.Today;
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;

			var sailingManager = GetSailingManager(transport);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			transport.JW_JX = NewSailing(vessel, "Flight", HomePort, OverseasPort).PK;
			sailingManager.Sailing = transport.Sailing;

			transport.JW_STA = today.AddDays(4);

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			var address3 = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var transportFactory1 = factory1.Load<Transport>(transport.PK);
			var transportFactory2 = factory2.Load<Transport>(transport.PK);

			transportFactory1.ParentType = transport.ParentType;
			transportFactory2.ParentType = transport.ParentType;

			transportFactory1.JW_ETD = today.AddHours(3);
			transportFactory1.JW_ATD = today.AddHours(4);
			transportFactory1.JW_OA_DepartureLocation = address2.PK;

			transportFactory2.JW_ETD = today.AddHours(7);
			transportFactory2.JW_ETA = today.AddDays(8);
			transportFactory2.JW_OA_ArrivalLocation = address3.PK;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert("Save Concurrency Exception", true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AssertEquals("JW_ETD: value changed by both users should take first saved", today.AddHours(3), transportFactory2.JW_ETD);
				AssertEquals("JW_ATD: value changed by user 1 should be copied across", today.AddHours(4), transportFactory2.JW_ATD);
				AssertEquals("JW_OA_DepartureLocation: value changed by user 1 should be copied across", address2.PK, transportFactory2.JW_OA_DepartureLocation);
				AssertEquals("JW_Vessel: value not changed by either user should remain", "Vessel", transportFactory2.JW_Vessel);
				AssertEquals("JW_ETA: value changed by user 2 should remain", today.AddDays(8), transportFactory2.JW_ETA);
				AssertEquals("JW_OA_ArrivalLocation: value changed by user 2 should remain", address3.PK, transportFactory2.JW_OA_ArrivalLocation);
				AssertEquals("JW_STA: value not changed by either user should remain", today.AddDays(4), transportFactory2.JW_STA);

				factory2.Save();
				Assert("Save Succeeded", true);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForPastETD()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(ScheduleInfo.Empty);
			matchResultMock.SetupGet(r => r.MatchErrorMessage).Returns("");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETD = new ZDateTime(2018, 12, 31);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Never);
				AssertEquals(Constants.FlightScheduleStatus.Unmatched, transport.JW_OnlineScheduleStatus);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForNormalETD()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			matchResultMock.SetupGet(r => r.MatchErrorMessage).Returns("");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETD = new ZDateTime(2019, 2, 1);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Exactly(2));
				AssertNoNotifications(transport.JW_OnlineScheduleStatusInfo);
				AssertEquals(Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForExceedingETD()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Unmatched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(ScheduleInfo.Empty);
			matchResultMock.SetupGet(r => r.MatchErrorMessage)
				.Returns("Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETD = new ZDateTime(2022, 2, 1);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Exactly(2));
				AssertEquals(Constants.FlightScheduleStatus.Unmatched, transport.JW_OnlineScheduleStatus);
				AssertHasWarning(transport.JW_OnlineScheduleStatusInfo,
					"Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.");
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForPastETA()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(ScheduleInfo.Empty);
			matchResultMock.SetupGet(r => r.MatchErrorMessage).Returns("");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETA = new ZDateTime(2018, 12, 31);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Never);
				AssertEquals(Constants.FlightScheduleStatus.Unmatched, transport.JW_OnlineScheduleStatus);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForNormalETA()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			matchResultMock.SetupGet(r => r.MatchErrorMessage).Returns("");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETD = new ZDateTime(2019, 2, 1);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Exactly(2));
				AssertNoNotifications(transport.JW_OnlineScheduleStatusInfo);
				AssertEquals(Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
			}
		}

		[MatchAgainstOnlineFlightsInUnitTest]
		[TestDate(2019, 1, 1)]
		public void TestFlightMatchForExceedingETA()
		{
			var matchMock = new Mock<IS8Matcher>();
			var matchResultMock = new Mock<IS8MatchResult>();
			matchResultMock.SetupGet(r => r.ScheduleStatus).Returns(Constants.FlightScheduleStatus.Unmatched);
			matchResultMock.SetupGet(r => r.MatchedSchedule).Returns(ScheduleInfo.Empty);
			matchResultMock.SetupGet(r => r.MatchErrorMessage)
				.Returns("Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.");
			matchMock.Setup(x => x.Match(It.IsAny<ScheduleInfo>())).Returns(matchResultMock.Object);

			using (ObjectFactory.Substitute(matchMock.Object))
			{
				var today = DateTime.Today;
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "QF800";
				transport.JW_RL_NKLoadPort = HomePort;
				transport.JW_RL_NKDiscPort = OverseasPort;

				var sailingManager = GetSailingManager(transport);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Vessel";

				transport.JW_JX = NewSailing(vessel, "QF800", HomePort, OverseasPort).PK;
				sailingManager.Sailing = transport.Sailing;

				transport.JW_ETA = new ZDateTime(2022, 2, 1);

				matchMock.Verify(x => x.Match(It.IsAny<ScheduleInfo>()), Moq.Times.Once);
				AssertEquals(Constants.FlightScheduleStatus.Unmatched, transport.JW_OnlineScheduleStatus);
				AssertHasWarning(transport.JW_OnlineScheduleStatusInfo,
					"Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.");
			}
		}

		public void TestProxiedConcurrencyMerge_SailingChangesSynchronisesBackToTransport()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var vessel = RefVessel.LookupVesselByName("CONDOR", factory1).First();

			var voyage = factory1.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDate(2019, 3, 1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDate(2019, 3, 15);

			voyage.GenerateSailings();

			factory1.Save();

			var consolFactory2 = factory2.New<CommonConsol>();
			consolFactory2.JK_TransportMode = "SEA";
			consolFactory2.JK_RL_NKLoadPort = "NZAKL";
			consolFactory2.JK_RL_NKDischargePort = "SGSIN";

			var transportFactory2 = consolFactory2.Transports[0];
			transportFactory2.JW_IsLinked = true;
			transportFactory2.JW_JX = voyage.Sailings[0].PK;
			transportFactory2.JW_RL_NKLoadPort = "NZAKL";
			transportFactory2.JW_ETA = new ZDateTime(2019, 3, 16);

			destination.JB_E_ARV = new ZDate(2019, 3, 17);

			voyage.GenerateSailings();

			factory1.Save();

			try
			{
				factory2.Save();
				Fail();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert(true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AssertEquals("Load port edited on consol", "NZAKL", transportFactory2.JW_RL_NKLoadPort);
				AssertEquals("ETA reloaded from schedule", new ZDate(2019, 3, 17), transportFactory2.JW_ETA);
				AssertSynchronisedProxiedProperties(transportFactory2);
			}
		}

		#endregion

		#region AircraftType_List

		public void TestAircraftType_List()
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			AssertEquals("AircraftType_List should be empty", 0, transport.AircraftType_List.Count);
		}

		#endregion

		#region Vessel Movements Url Supporter
		static readonly ZDateTime vesselMovementTestDateTimePlus1 = new ZDateTime(2014, 11, 1, 6, 24, 0);
		static readonly ZDateTime vesselMovementTestDateTime = new ZDateTime(2014, 10, 31, 5, 23, 0);
		static readonly ZDateTime vesselMovementTestDate = new ZDateTime(2014, 10, 31, 0, 0, 0);
		static readonly ZDateTime vesselMovementTestDateTimeMinus2 = new ZDateTime(2014, 10, 29, 3, 21, 0);
		static readonly ZDateTime vesselMovementTestDateTimeMinus3 = new ZDateTime(2014, 10, 28, 2, 20, 0);

		public void TestVesselMovementsUrl_ReturnsError_When_NotSeaTransportMode()
			=> AssertVesselMovementsModelErrorMessageFor("Routing leg Transport Mode must be SEA.", transportMode: Constants.TransportModes.Air);

		public void TestVesselMovementsUrl_ReturnsError_When_InvalidLoadPort()
			=> AssertVesselMovementsModelErrorMessageFor(
				"Routing leg has an invalid load port.",
				actualArrivalTime: vesselMovementTestDate,
				loadPortUnloco: "Z");

		public void TestVesselMovementsUrl_ReturnsError_When_InvalidDischargePort()
			=> AssertVesselMovementsModelErrorMessageFor(
				"Routing leg has an invalid discharge port.",
				actualArrivalTime: vesselMovementTestDate,
				dischargePortUnloco: "Z");

		public void TestVesselMovementsUrl_ReturnsUrl_For_ActualDatesPreferredOverEstimated()
			=> AssertVesselMovementsModelFor(
				new VesselMovementsUrlModel
				{
					LloydsNumber = "8507652",
					DepartureTime = vesselMovementTestDateTimeMinus3,
					ArrivalTime = vesselMovementTestDateTimeMinus2,
				},
				actualDepartureTime: vesselMovementTestDateTimeMinus3,
				actualArrivalTime: vesselMovementTestDateTimeMinus2,
				estimatedDepartureTime: vesselMovementTestDateTime,
				estimatedArrivalTime: vesselMovementTestDateTimePlus1);

		public void TestVesselMovementsUrl_ReturnsUrl_For_FallbackToEstimatedDatesWhenActualMissing()
			=> AssertVesselMovementsModelFor(
				new VesselMovementsUrlModel
				{
					LloydsNumber = "8507652",
					DepartureTime = vesselMovementTestDateTime,
					ArrivalTime = vesselMovementTestDateTimePlus1,
				},
				estimatedDepartureTime: vesselMovementTestDateTime,
				estimatedArrivalTime: vesselMovementTestDateTimePlus1);

		public void TestVesselMovementsUrl_ReturnsUrl_For_DifferentVessel()
			=> AssertVesselMovementsModelFor(
				new VesselMovementsUrlModel
				{
					LloydsNumber = "9255737",
					DepartureTime = vesselMovementTestDateTime,
					ArrivalTime = vesselMovementTestDateTimePlus1,
				},
				vesselIMO: "9255737",
				actualDepartureTime: vesselMovementTestDateTime,
				actualArrivalTime: vesselMovementTestDateTimePlus1);

		public void TestVesselMovementsUrl_ReturnsUrl_For_VoyageNumberAndCarrierCode()
			=> AssertVesselMovementsModelFor(
				new VesselMovementsUrlModel
				{
					LloydsNumber = "8507652",
					DepartureTime = vesselMovementTestDateTime,
					ArrivalTime = vesselMovementTestDateTimePlus1,
					CarrierCode = "CARC",
					VoyageNumber = "74N",
				},
				carrierCode: "CARC",
				voyageNumber: "74N",
				actualDepartureTime: vesselMovementTestDateTime,
				actualArrivalTime: vesselMovementTestDateTimePlus1);

		public void TestVesselMovementsUrl_ReturnsUrl_For_AllPossibleParameters()
			=> AssertVesselMovementsModelFor(
				new VesselMovementsUrlModel
				{
					LloydsNumber = "8507652",
					DepartureTime = vesselMovementTestDateTime,
					ArrivalTime = vesselMovementTestDateTimePlus1,
					CarrierCode = "CARC",
					VoyageNumber = "74N",
					DeparturePortUnloco = "AUPBT",
					ArrivalPortUnloco = "NZAKL",
				},
				actualDepartureTime: vesselMovementTestDateTime,
				actualArrivalTime: vesselMovementTestDateTimePlus1,
				carrierCode: "CARC",
				voyageNumber: "74N",
				loadPortUnloco: "AUPBT",
				dischargePortUnloco: "NZAKL");

		void ConfigureTransportForVesselMovementsUrlTest(
			string transportMode = Constants.TransportModes.Sea,
			string vesselIMO = "8507652",
			ZDateTime? actualDepartureTime = null,
			ZDateTime? actualArrivalTime = null,
			ZDateTime? estimatedDepartureTime = null,
			ZDateTime? estimatedArrivalTime = null,
			string carrierCode = null,
			string voyageNumber = null,
			string loadPortUnloco = null,
			string dischargePortUnloco = null)
		{
			Transport.JW_TransportMode = transportMode;
			if (vesselIMO != null)
			{
				var v = Factory.NewWithValidTestData<RefVessel>();
				v.RV_LloydsNumber = vesselIMO;
				Transport.JW_Vessel = v.RV_FK;
				AssertNotNull(Transport.Vessel);
			}
			if (actualDepartureTime != null)
			{
				Transport.JW_ATD = actualDepartureTime.Value;
			}
			if (actualArrivalTime != null)
			{
				Transport.JW_ATA = actualArrivalTime.Value;
			}
			if (estimatedDepartureTime != null)
			{
				Transport.JW_ETD = estimatedDepartureTime.Value;
			}
			if (estimatedArrivalTime != null)
			{
				Transport.JW_ETA = estimatedArrivalTime.Value;
			}
			if (carrierCode != null)
			{
				OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_IsShippingProvider = true;
				carrier.OH_FullName = "Carrier " + carrierCode;
				carrier.OH_Code = carrierCode;
				carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				carrier.SetCustomsCode(OrgCusCode.CodeTypes.CarrierCode, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates), carrierCode);
				Transport.CarrierPK = carrier.PK;
				AssertNotNull(Transport.Carrier);
			}
			Transport.JW_VoyageFlight = voyageNumber;
			if (loadPortUnloco != null)
			{
				Transport.JW_RL_NKLoadPort = loadPortUnloco;
			}
			if (dischargePortUnloco != null)
			{
				Transport.JW_RL_NKDiscPort = dischargePortUnloco;
			}

			Transport.Validation.ValidateAll();
		}

		void AssertVesselMovementsModelErrorMessageFor(
			string expectedErrorMessage,
			string transportMode = Constants.TransportModes.Sea,
			string vesselIMO = "8507652",
			ZDateTime? actualDepartureTime = null,
			ZDateTime? actualArrivalTime = null,
			ZDateTime? estimatedDepartureTime = null,
			ZDateTime? estimatedArrivalTime = null,
			string loadPortUnloco = null,
			string dischargePortUnloco = null)
		{
			ConfigureTransportForVesselMovementsUrlTest(
				transportMode: transportMode,
				vesselIMO: vesselIMO,
				actualDepartureTime: actualDepartureTime,
				actualArrivalTime: actualArrivalTime,
				estimatedDepartureTime: estimatedDepartureTime,
				estimatedArrivalTime: estimatedArrivalTime,
				loadPortUnloco: loadPortUnloco,
				dischargePortUnloco: dischargePortUnloco);

			var (model, errorMessage) = ((IVesselMovementsUrlSupporter)Transport).GetVesselMovementsUrlModel();

			CombineAssertions(() =>
			{
				AssertNull(model);
				AssertEquals(expectedErrorMessage, errorMessage);
			});
		}

		void AssertVesselMovementsModelFor(
			VesselMovementsUrlModel expectedModel,
			string transportMode = Constants.TransportModes.Sea,
			string vesselIMO = "8507652",
			ZDateTime? actualDepartureTime = null,
			ZDateTime? actualArrivalTime = null,
			ZDateTime? estimatedDepartureTime = null,
			ZDateTime? estimatedArrivalTime = null,
			string carrierCode = null,
			string voyageNumber = null,
			string loadPortUnloco = null,
			string dischargePortUnloco = null)
		{
			ConfigureTransportForVesselMovementsUrlTest(
				transportMode: transportMode,
				vesselIMO: vesselIMO,
				actualDepartureTime: actualDepartureTime,
				actualArrivalTime: actualArrivalTime,
				estimatedDepartureTime: estimatedDepartureTime,
				estimatedArrivalTime: estimatedArrivalTime,
				carrierCode: carrierCode,
				voyageNumber: voyageNumber,
				loadPortUnloco: loadPortUnloco,
				dischargePortUnloco: dischargePortUnloco);

			var (model, errorMessage) = ((IVesselMovementsUrlSupporter)Transport).GetVesselMovementsUrlModel();

			CombineAssertions(() =>
			{
				AssertNull(errorMessage);
				AssertEquals(expectedModel, model);
			});
		}
		#endregion

		#region GetParentSafe

		public void TestGetParentSafe()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];
			AssertNotNull(transport.GetParentSafe());
			AssertEquals(transport.Parent, transport.GetParentSafe());

			var transport1 = Factory.New<Transport>();
			AssertNull(transport1.ParentType);
			AssertNull(transport1.GetParentSafe());
		}

		#endregion

		#region ExceptionShouldBeCaughtWhenLoginFails

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestExceptionShouldBeCaughtWhenLoginFails()
		{
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s9testnotexist"))
			using (FreightDataRegistry.Instance.S8CargoWebServiceURL_Faillover.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s9testnotexist"))
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();

				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport = (Transport)consol.Transports.First();
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "MU8414";
				AssertEquals("NZAKL", transport.JW_RL_NKLoadPort);
				AssertEquals("AUSYD", transport.JW_RL_NKDiscPort);

				ErrorReporter.Clear();
				transport.JW_ETD = ZDateTime.Now;
				AssertNoExceptionThrown(() => transport.JW_ETD = ZDateTime.Now);
				Assert("No error report key is generated under DEBUG mode", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				Assert("No error report message is generated under DEBUG mode", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region OnlineFlightMatchingHelper

		[MatchAgainstOnlineFlightsInUnitTest]
		public void TestOnlineFlightMatchingHelperIsCreated()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.Transports.AddNew();

			var portOfLoading = consol.JK_JX_JA_RL_NKPortOfLoading;
			AssertOnlineFlightMatchingHelperIsCreated(transport, false);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertOnlineFlightMatchingHelperIsCreated(transport, true);
		}

		void AssertOnlineFlightMatchingHelperIsCreated(Transport transport, bool created)
		{
			var field = transport.GetType().GetField("onlineFlightMatchingHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(field);

			var fieldValue = field.GetValue(transport);
			if (created)
			{
				AssertNotNull(fieldValue);
			}
			else
			{
				AssertNull(fieldValue);
			}
		}

		#endregion

		public void TestBuildRowDeletedReport()
		{
			var transport = Factory.New<Transport>();
			transport.Delete();
			AssertNoExceptionThrown(() => { _ = transport.JW_IsLinked; });
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertContains("Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetPropertyValuesForDeletedRowReporting_SavingBeforeDeletion()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_Status = Constants.TransportStatus.Planned;
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_ETA = new ZDateTime(2020, 05, 10);
			Factory.Save();
			transport.Delete();

			var result = transport.GetPropertyValuesForDeletedRowReporting();

			var expectedJW_TransportMode = "JW_TransportMode: AIR";
			var expectedJW_Status = "JW_Status: PLN";
			var expectedJW_RL_NKLoadPort = "JW_RL_NKLoadPort: HKHKG";
			var expectedJW_RL_NKDiscPort = "JW_RL_NKDiscPort: SGSIN";
			var expectedJW_ETA = "JW_ETA: 10-May-20 00:00:00";

			AssertEquals("Precondition: IsInDatabase should be true", true, transport.IsInDatabase);
			AssertContains(expectedJW_TransportMode, result);
			AssertContains(expectedJW_Status, result);
			AssertContains(expectedJW_RL_NKLoadPort, result);
			AssertContains(expectedJW_RL_NKDiscPort, result);
			AssertContains(expectedJW_ETA, result);
		}

		public void TestGetPropertyValuesForDeletedRowReporting_WithoutSaving()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			transport.Delete();

			var result = transport.GetPropertyValuesForDeletedRowReporting();
			var expected = "Transport property values cannot be collected as IsInDataBase is false.";

			AssertEquals("Precondition: IsInDatabase should be false", false, transport.IsInDatabase);
			AssertEquals("Expected no properties", expected, result);
		}

		public void TestDateEventNotAddedIfParentTypeIsISF()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_ParentType = Constants.TransportParentTypes.ImporterSecurityFiling;
			transport.JW_ATD = new ZDateTime(2021, 02, 10);

			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			filter.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			var logs = transport.Logs.Find(filter);

			CombineAssertions(delegate
			{
				AssertEquals(0, logs.Length);
			});
		}

		public void TestTransportWithCarrierAddressHasOrgHeaderCarrierCodeAndName()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_FullName = "Carrier Organisation 1";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			AssertEquals("When Carrier Address is populated, Carrier Code property should show org code of carrier", carrier.OH_Code, transport.CarrierCode);
			AssertEquals("When Carrier Address is populated, Carrier Code property should show org name of carrier", "Carrier Organisation 1", transport.CarrierName);
		}

		public void TestTransportWithNoCarrierAddressHasEmptyCarrierCodeAndName()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();

			AssertEquals("Precondition: Carrier Address is empty", ZGuid.Empty, transport.JW_OA_CarrierAddress); // precondition

			AssertEquals("When Carrier Address is not populated, Carrier Code property should be empty", ZString.Empty, transport.CarrierCode);
			AssertEquals("When Carrier Address is not populated, Carrier Name property should be empty", ZString.Empty, transport.CarrierName);
		}

		#region JW_AdditionalTransportMode

		public void TestJW_AdditionalTransportModeIsReadOnlyAndNotLinked()
		{
			var tuples = new Tuple<string, string, string>[]
			{
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.OnForwarding, Constants.TransportModes.Road),
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.PreCarriage, Constants.TransportModes.Road),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.OnForwarding, Constants.TransportModes.Road),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.PreCarriage, Constants.TransportModes.Road),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.OnForwarding, Constants.TransportModes.Rail),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.PreCarriage, Constants.TransportModes.Rail),
			};

			foreach (var tuple in tuples)
			{
				var transport = Factory.New<CommonConsol>().Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_TransportMode = Constants.TransportModes.Rail;
				transport.JW_TransportType = Constants.TransportPlanningType.Other;
				transport.JW_AdditionalTransportMode = string.Empty;
				Assert(!transport.JW_IsLinkedInfo.ReadOnly);
				Assert(transport.JW_IsLinked);

				transport.JW_TransportMode = tuple.Item1;
				transport.JW_TransportType = tuple.Item2;
				transport.JW_AdditionalTransportMode = tuple.Item3;
				Assert(transport.JW_IsLinkedInfo.ReadOnly);
				Assert(!transport.JW_IsLinked);
			}
		}

		public void TestJW_AdditionalTransportModeChangesEmpty()
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();

			transport.JW_TransportMode = Constants.TransportModes.Rail;
			transport.JW_TransportType = Constants.TransportPlanningType.OnForwarding;
			transport.JW_AdditionalTransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.TransportModes.Road, transport.JW_AdditionalTransportMode);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(string.Empty, transport.JW_AdditionalTransportMode);

			transport.JW_AdditionalTransportMode = Constants.TransportModes.Road;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals(string.Empty, transport.JW_AdditionalTransportMode);
		}

		#endregion

		public void TestNoExceptionIsThrownWhenAccessingShippingLineInDeletedTransport()
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.Delete();
			var managedSailing = transport as ISailingManaged;

			AssertNoExceptionThrown("No exception is thrown when setting value to ShippingLine.", () =>
			{
				managedSailing.ShippingLine = ZGuid.NewZGuid();
			});
		}

		public void TestJW_JX_IsPublished_OnUpdatedByDataRefresh()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var consol = factory1.NewWithValidTestData<CommonConsol>();
			var sailing = factory1.NewWithValidTestData<JobSailing>();
			var transportInFactory1 = consol.Transports[0];
			transportInFactory1.JW_JX = sailing.PK;
			transportInFactory1.JW_IsLinked = true;
			factory1.Save();

			CombineAssertions(() =>
			{
				AssertEquals(true, transportInFactory1.JW_IsLinked);
				AssertEquals(true, transportInFactory1.JW_JX_IsPublished);
			});

			var consolInFactory2 = factory2.Load<CommonConsol>(consol.PK);
			consolInFactory2.Transports.OfType<Transport>().ForEach(x => x.JW_IsLinked = false);
			factory2.Save();

			CombineAssertions(() =>
			{
				AssertEquals(false, consolInFactory2.Transports[0].JW_IsLinked);
				AssertEquals(false, consolInFactory2.Transports[0].JW_JX_IsPublished);

				AssertEquals(false, transportInFactory1.JW_IsLinked);
				AssertEquals(false, transportInFactory1.JW_JX_IsPublished);
			});

			var factory3 = new BusinessObjectFactory();
			var transportInFactory3 = factory3.Load<Transport>(transportInFactory1.PK);

			CombineAssertions(() =>
			{
				AssertEquals(false, transportInFactory3.JW_IsLinked);
				AssertEquals(false, transportInFactory3.JW_JX_IsPublished);
			});
		}

		public void TestOnSavingWhenNotLinkedButJW_JXIsValid()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "DEPCTO";
			orgHeader1.OH_IsCreditor = true;
			orgHeader1.CompanyData.SetAPTaxApplicable(false);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "ARVCTO";
			orgHeader2.OH_IsCreditor = true;
			orgHeader2.CompanyData.SetAPTaxApplicable(false);

			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.JV_IsCargoOnly = true;
			voyage.JV_IsChartered = false;
			voyage.JV_AircraftType = "E90";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_OA_DepartureCTOAddress = orgHeader1.MainAddress.PK;
			origin.JA_S_DEP = new ZDateTime(2023, 02, 20);
			origin.JA_E_DEP = new ZDateTime(2023, 02, 20);
			origin.JA_A_DEP = new ZDateTime(2023, 02, 20);

			origin.JA_DocumentaryCutoff = new ZDateTime(2023, 02, 20);
			origin.JA_VGMCutOff = new ZDateTime(2023, 02, 20);
			origin.JA_ReceivalCommences = new ZDateTime(2023, 02, 20);
			origin.JA_CutOff = new ZDateTime(2023, 02, 20);

			origin.JA_EmptyReceivalCommences = new ZDateTime(2023, 02, 20);
			origin.JA_EmptyCutOff = new ZDateTime(2023, 02, 20);
			origin.JA_ReeferReceivalCommences = new ZDateTime(2023, 02, 20);
			origin.JA_ReeferCutOff = new ZDateTime(2023, 02, 20);
			origin.JA_DGReceivalCommences = new ZDateTime(2023, 02, 20);
			origin.JA_DGCutOff = new ZDateTime(2023, 02, 20);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_OA_ArrivalCTOAddress = orgHeader2.MainAddress.PK;
			destination.JB_S_ARV = new ZDateTime(2023, 02, 21);
			destination.JB_E_ARV = new ZDateTime(2023, 02, 21);
			destination.JB_A_ARV = new ZDateTime(2023, 02, 21);
			destination.JB_AvailabilityDate = new ZDateTime(2023, 02, 21);
			destination.JB_StorageDate = new ZDateTime(2023, 02, 21);

			voyage.GenerateSailings();

			voyage.Sailings[0].JX_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;
			voyage.Sailings[0].JX_DepotReceivalCommences = new ZDateTime(2023, 02, 21);
			voyage.Sailings[0].JX_DepotCutOff = new ZDateTime(2023, 02, 21);
			voyage.Sailings[0].JX_DepotAvailabilityDate = new ZDateTime(2023, 02, 21);
			voyage.Sailings[0].JX_DepotStorageDate = new ZDateTime(2023, 02, 21);

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings[0].PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("JW_Vessel", "CONDOR", transport.JW_Vessel);
				AssertEquals("JW_VoyageFlight", "012", transport.JW_VoyageFlight);
				Assert("JW_IsCharter", !transport.JW_IsCharter);
				Assert("JW_IsCargoOnly", transport.JW_IsCargoOnly);
				AssertEquals("JW_AircraftType", "E90", transport.JW_AircraftType);

				AssertEquals("JW_RL_NKLoadPort", "AUBNE", transport.JW_RL_NKLoadPort);
				AssertEquals("JW_OA_DepartureLocation", orgHeader1.MainAddress.PK, transport.JW_OA_DepartureLocation);
				AssertEquals("JW_STD", new ZDateTime(2023, 02, 20), transport.JW_STD);
				AssertEquals("JW_ETD", new ZDateTime(2023, 02, 20), transport.JW_ETD);
				AssertEquals("JW_ATD", new ZDateTime(2023, 02, 20), transport.JW_ATD);

				AssertEquals("JW_DocumentaryCutOff", new ZDateTime(2023, 02, 20), transport.JW_DocumentaryCutOff);
				AssertEquals("JW_VGMCutOff", new ZDateTime(2023, 02, 20), transport.JW_VGMCutOff);
				AssertEquals("JW_TerminalReceivalCommences", new ZDateTime(2023, 02, 20), transport.JW_TerminalReceivalCommences);
				AssertEquals("JW_TerminalCutOff", new ZDateTime(2023, 02, 20), transport.JW_TerminalCutOff);

				AssertEquals("JW_EmptyReceivalCommences", new ZDateTime(2023, 02, 20), transport.JW_EmptyReceivalCommences);
				AssertEquals("JW_EmptyCutOff", new ZDateTime(2023, 02, 20), transport.JW_EmptyCutOff);
				AssertEquals("JW_ReeferReceivalCommences", new ZDateTime(2023, 02, 20), transport.JW_ReeferReceivalCommences);
				AssertEquals("JW_ReeferCutOff", new ZDateTime(2023, 02, 20), transport.JW_ReeferCutOff);
				AssertEquals("JW_DGReceivalCommences", new ZDateTime(2023, 02, 20), transport.JW_DGReceivalCommences);
				AssertEquals("JW_DGCutOff", new ZDateTime(2023, 02, 20), transport.JW_DGCutOff);

				AssertEquals("JW_RL_NKDiscPort", "SGSIN", transport.JW_RL_NKDiscPort);
				AssertEquals("JW_OA_ArrivalLocation", orgHeader2.MainAddress.PK, transport.JW_OA_ArrivalLocation);
				AssertEquals("JW_STA", new ZDateTime(2023, 02, 21), transport.JW_STA);
				AssertEquals("JW_ETA", new ZDateTime(2023, 02, 21), transport.JW_ETA);
				AssertEquals("JW_ATA", new ZDateTime(2023, 02, 21), transport.JW_ATA);
				AssertEquals("JW_TerminalAvailabilityDate", new ZDateTime(2023, 02, 21), transport.JW_TerminalAvailabilityDate);
				AssertEquals("JW_TerminalStorageDate", new ZDateTime(2023, 02, 21), transport.JW_TerminalStorageDate);

				AssertEquals("JW_OnlineScheduleStatus", Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
				AssertEquals("JW_DepotReceivalCommences", new ZDateTime(2023, 02, 21), transport.JW_DepotReceivalCommences);
				AssertEquals("JW_DepotCutOff", new ZDateTime(2023, 02, 21), transport.JW_DepotCutOff);
				AssertEquals("JW_DepotAvailabilityDate", new ZDateTime(2023, 02, 21), transport.JW_DepotAvailabilityDate);
				AssertEquals("JW_DepotStorageDate", new ZDateTime(2023, 02, 21), transport.JW_DepotStorageDate);
			});

			Db.Connection.ExecuteNonQuery(@$"
UPDATE {Transport.Schema.TableName}
SET
	{Transport.Schema.JW_IsLinked} = 0,
	JW_SystemLastEditTimeUtc = GETUTCDATE(),
	JW_SystemLastEditUser = '~BP'
WHERE
	{Transport.Schema.PK} = '{transport.PK}'");

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var consolInNewFactory = factory.Load<CommonConsol>(consol.PK);
			var transportInNewFactory = consolInNewFactory.Transports.OfType<Transport>().First(x => x.PK == transport.PK);

			transportInNewFactory.JW_Vessel = "ARAFURA";
			transportInNewFactory.JW_VoyageFlight = "123";
			transportInNewFactory.JW_IsCharter = true;
			transportInNewFactory.JW_IsCargoOnly = false;
			transportInNewFactory.JW_AircraftType = "09E";

			transportInNewFactory.JW_RL_NKLoadPort = "AUSYD";
			transportInNewFactory.JW_OA_DepartureLocation = orgHeader2.MainAddress.PK;
			transportInNewFactory.JW_STD = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ETD = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ATD = new ZDateTime(2023, 02, 22);

			transportInNewFactory.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_VGMCutOff = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_TerminalCutOff = new ZDateTime(2023, 02, 22);

			transportInNewFactory.JW_EmptyReceivalCommences = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_EmptyCutOff = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ReeferReceivalCommences = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ReeferCutOff = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_DGReceivalCommences = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_DGCutOff = new ZDateTime(2023, 02, 22);

			transportInNewFactory.JW_RL_NKDiscPort = "CNSHG";
			transportInNewFactory.JW_OA_ArrivalLocation = orgHeader1.MainAddress.PK;
			transportInNewFactory.JW_STA = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ETA = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_ATA = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_TerminalAvailabilityDate = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_TerminalStorageDate = new ZDateTime(2023, 02, 22);

			transportInNewFactory.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;
			transportInNewFactory.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_DepotCutOff = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_DepotAvailabilityDate = new ZDateTime(2023, 02, 22);
			transportInNewFactory.JW_DepotStorageDate = new ZDateTime(2023, 02, 22);

			CombineAssertions(() =>
			{
				Assert("JW_JX is valid", transportInNewFactory.JW_JX.IsValid);
				Assert("JW_IsLinked is false", !transportInNewFactory.JW_IsLinked);
			});

			factory.Save();

			transportInNewFactory.Reload();

			CombineAssertions(() =>
			{
				Assert("JW_IsLinked is false", !transportInNewFactory.JW_IsLinked);
				Assert("JW_JX should be corrected", transportInNewFactory.JW_JX.IsEmpty);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_Vessel", "ARAFURA", transportInNewFactory.JW_Vessel);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_VoyageFlight", "123", transportInNewFactory.JW_VoyageFlight);
				Assert("Not trigger TG_JobConsolTransport - JW_IsCharter", transportInNewFactory.JW_IsCharter);
				Assert("Not trigger TG_JobConsolTransport - JW_IsCargoOnly", !transportInNewFactory.JW_IsCargoOnly);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_AircraftType", "09E", transportInNewFactory.JW_AircraftType);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_RL_NKLoadPort", "AUSYD", transportInNewFactory.JW_RL_NKLoadPort);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_OA_DepartureLocation", orgHeader2.MainAddress.PK, transportInNewFactory.JW_OA_DepartureLocation);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_STD", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_STD);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ETD", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ETD);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ATD", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ATD);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_DocumentaryCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DocumentaryCutOff);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_VGMCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_VGMCutOff);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_TerminalReceivalCommences", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_TerminalReceivalCommences);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_TerminalCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_TerminalCutOff);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_EmptyReceivalCommences", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_EmptyReceivalCommences);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_EmptyCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_EmptyCutOff);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ReeferReceivalCommences", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ReeferReceivalCommences);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ReeferCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ReeferCutOff);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DGReceivalCommences", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DGReceivalCommences);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DGCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DGCutOff);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_RL_NKDiscPort", "CNSHG", transportInNewFactory.JW_RL_NKDiscPort);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_OA_ArrivalLocation", orgHeader1.MainAddress.PK, transportInNewFactory.JW_OA_ArrivalLocation);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_STA", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_STA);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ETA", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ETA);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_ATA", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_ATA);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_TerminalAvailabilityDate", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_TerminalAvailabilityDate);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_TerminalStorageDate", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_TerminalStorageDate);

				AssertEquals("Not trigger TG_JobConsolTransport - JW_OnlineScheduleStatus", Constants.FlightScheduleStatus.Unknown, transportInNewFactory.JW_OnlineScheduleStatus);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DepotReceivalCommences", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DepotReceivalCommences);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DepotCutOff", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DepotCutOff);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DepotAvailabilityDate", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DepotAvailabilityDate);
				AssertEquals("Not trigger TG_JobConsolTransport - JW_DepotStorageDate", new ZDateTime(2023, 02, 22), transportInNewFactory.JW_DepotStorageDate);
			});
		}

		public void TestJobCO2eCollection()
		{
			// Arrange
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			AssertEquals("Pre-condition.", transport.JobCO2eCollection.Count, 0);
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			AssertEquals("Pre-condition.", sailing.JobCO2eCollection.Count, 0);

			// Act
			transport.SetCO2ePerTonneInKg(1m);
			// Assert
			AssertEquals(transport.JobCO2eCollection.Count, 1);
			AssertEquals(transport.GetCO2ePerTonneInKg(), 1m);

			// Act
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			// Assert
			AssertEquals(transport.JobCO2eCollection.Count, 0);
			AssertEquals(transport.GetCO2ePerTonneInKg(), 0m);

			// Act
			sailing.SetCO2ePerTonneInKg(2m);
			// Assert
			AssertEquals(transport.JobCO2eCollection.Count, 1);
			AssertEquals(transport.GetCO2ePerTonneInKg(), 2m);
			AssertEquals(sailing.JobCO2eCollection.Count, 1);
			AssertEquals(sailing.GetCO2ePerTonneInKg(), 2m);
		}

		public void TestTotalCO2eForSorting()
		{
			var supporterMock = new Mock<ICO2eLegBasedSupporter>();
			supporterMock.Setup(x => x.Weight).Returns(1m);
			supporterMock.Setup(x => x.UnitOfWeight).Returns("T");

			var transport1 = Factory.New<CommonConsol>().Transports.AddNew();
			((ICO2eLegProvider)transport1).CurrentCO2eCalcSupporter = supporterMock.Object;
			transport1.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(0m, transport1.TotalCO2eForSorting);

			var transport2 = Factory.New<CommonConsol>().Transports.AddNew();
			((ICO2eLegProvider)transport2).CurrentCO2eCalcSupporter = supporterMock.Object;
			transport2.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport2.SetCO2ePerTonneInKg(999.01m);
			AssertEquals(999.01m, transport2.TotalCO2eForSorting);

			var transport3 = Factory.New<CommonConsol>().Transports.AddNew();
			((ICO2eLegProvider)transport3).CurrentCO2eCalcSupporter = supporterMock.Object;
			transport3.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport3.SetCO2ePerTonneInKg(1234.5678m);
			AssertEquals(1234.5678m, transport3.TotalCO2eForSorting);

			var transport4 = Factory.New<CommonConsol>().Transports.AddNew();
			((ICO2eLegProvider)transport4).CurrentCO2eCalcSupporter = supporterMock.Object;
			transport4.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport4.SetCO2ePerTonneInKg(2345.6789m);
			AssertEquals(2345.6789m, transport4.TotalCO2eForSorting);

			Assert("Blank is smaller than 999.01", transport1.TotalCO2eForSorting < transport2.TotalCO2eForSorting);
			Assert("999.01 is smaller than 1,234.568", transport2.TotalCO2eForSorting < transport3.TotalCO2eForSorting);
			Assert("1,234.568 is smaller than 2,345.679", transport3.TotalCO2eForSorting < transport4.TotalCO2eForSorting);
		}

		public void TestTotalCO2eForSorting_DecimalPlaces()
		{
			var co2eForSortingDp = typeof(Transport)
				.GetProperty(nameof(Transport.TotalCO2eForSorting))
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			AssertEquals("TotalCO2eForSorting should display using 3dp", 3, co2eForSortingDp.DecimalPlaces);
		}

		public void TestTotalCO2eForSorting_ByWeight()
		{
			// Arrange
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			var supporterMock = new Mock<ICO2eLegBasedSupporter>();
			supporterMock.Setup(x => x.Weight).Returns(1m);
			supporterMock.Setup(x => x.UnitOfWeight).Returns("T");
			((ICO2eLegProvider)transport).CurrentCO2eCalcSupporter = supporterMock.Object;
			Assert("Precondition: RequireTEU is false", !(transport as ICO2eProvider).RequireTEU);
			AssertEquals("Precondition: CO2ePerTonneInKg default value is 0", 0m, transport.GetCO2ePerTonneInKg());
			AssertEquals("Precondition: CO2ePerTonneInKg default value is 0", 0m, transport.TotalCO2eForSorting);

			// Act & Assert
			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(0m, transport.TotalCO2eForSorting);

			transport.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.SetCO2ePerTonneInKg(1234.5678m);
			AssertEquals(1234.5678m, transport.TotalCO2eForSorting);

			transport.SetCO2ePerTonneInKg(0m);
			AssertEquals(0m, transport.TotalCO2eForSorting);
		}

		public void TestTotalCO2eForSorting_ByTEU()
		{
			// Arrange
			var consol = Factory.New<IForwardingConsol>();
			var transport = consol.Transports_AddNew() as Transport;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			var supporterMock = new Mock<ICO2eLegBasedSupporter>();
			supporterMock.Setup(x => x.GetNumberOfTEUForLeg(It.IsAny<ICO2eLegProvider>())).Returns(1m);
			((ICO2eLegProvider)transport).CurrentCO2eCalcSupporter = supporterMock.Object;
			Assert("Precondition: RequireTEU is false", !(transport as ICO2eProvider).RequireTEU);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.Containers.AddNew();

			Assert("Precondition: RequireTEU is true when Parent.RequireTEU is true", (transport as ICO2eProvider).RequireTEU);
			AssertEquals("Precondition: CO2ePerTEUInKg default value is 0", 0m, transport.GetCO2ePerTEUInKg());
			AssertEquals("Precondition: CO2ePerTonneInKg default value is 0", 0m, transport.TotalCO2eForSorting);

			// Act & Assert
			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
			AssertEquals(0m, transport.TotalCO2eForSorting);

			transport.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.SetCO2ePerTEUInKg(1234.5678m);
			AssertEquals(1234.5678m, transport.TotalCO2eForSorting);

			transport.SetCO2ePerTEUInKg(0m);
			AssertEquals(0m, transport.TotalCO2eForSorting);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenCO2eStatusCurrent() => UpdateCO2eStatusToNotCurrent(CO2eStatusList.Codes.Current);

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent_WhenCO2eStatusPending() => UpdateCO2eStatusToNotCurrent(CO2eStatusList.Codes.Pending);

		void UpdateCO2eStatusToNotCurrent(string originalStatus)
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertNoWarning(transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);

			Factory.Save();

			void UpdateRelatedPropertyAndAssert(string message, Action updateProperty, string stuReason)
			{
				TestDateAttribute.AddMinutes(1);
				transport.SetCO2eStatus(originalStatus);
				transport.Validation.ValidateTotalCO2eForSorting();
				AssertNoWarning("Pre-condition", transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);
				updateProperty.Invoke();
				AssertEquals(message, CO2eStatusList.Codes.NotCurrent, transport.GetCO2eStatus());
				AssertHasWarning(transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);
				CO2eTestHelper.AssertSTUEvent(transport, stuReason, originalStatus);
			}

			UpdateRelatedPropertyAndAssert("Transport Mode", () => transport.JW_TransportMode = Core.Constants.TransportModes.Sea,
				"JW_TransportMode [AIR]->[SEA]");
			UpdateRelatedPropertyAndAssert("Load Port", () => transport.JW_RL_NKLoadPort = "AUSYD",
				"JW_RL_NKLoadPort []->[AUSYD]");
			UpdateRelatedPropertyAndAssert("Discharge Port", () => transport.JW_RL_NKDiscPort = "NZAKL",
				"JW_RL_NKDiscPort []->[NZAKL]");
			UpdateRelatedPropertyAndAssert("Vessel", () => transport.JW_Vessel = "Vessel 1",
				"JW_Vessel []->[Vessel 1]");
			UpdateRelatedPropertyAndAssert("Voyage/Flight", () => transport.JW_VoyageFlight = "Voyage 1",
				"JW_VoyageFlight []->[Voyage 1]");
			UpdateRelatedPropertyAndAssert("Aircraft Type", () => transport.JW_AircraftType = "123",
				"JW_AircraftType []->[123]");

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			UpdateRelatedPropertyAndAssert("Carrier", () => transport.JW_OA_CarrierAddress = carrier.MainAddress.PK,
				$"JW_OA_CarrierAddress [{ZGuid.Empty}]->[{carrier.MainAddress.PK}]");
		}

		public void TestCO2eBindingIsRefreshedWhenLinkedSailingChanges()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "v";
			var sailing = NewSailing(vessel, "f", "l", "d");
			sailing.SetCO2eStatus(CO2eStatusList.Codes.Current);
			sailing.SetCO2ePerTonneInKg(100m);
			var transport = Factory.NewWithValidTestData<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			Factory.Save();

			AssertEquals("Pre-condition", CO2eStatusList.Codes.Current, transport.GetCO2eStatus());
			AssertEquals("Pre-condition", 100m, transport.GetCO2ePerTonneInKg());
			transport.Validation.ValidateTotalCO2eForSorting();
			AssertNoWarning("Pre-condition", transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);

			var newFac = new BusinessObjectFactory();
			var sailingInNewFac = newFac.Load<JobSailing>(sailing.PK);
			sailingInNewFac.Voyage.JV_VoyageFlight = "XXX";
			newFac.Save();
			AssertHasWarning(sailingInNewFac.CO2ePerTonneInKgForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertHasWarning(transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport.GetCO2eStatus());
			CO2eTestHelper.AssertSTUEvent(transport.Sailing, "JV_VoyageFlight [f]->[XXX]");
		}

		public void TestJobCO2eCollectionDoesNotAccessDeletedTransport()
		{
			// Arrange
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.SetCO2ePerTonneInKg(100);
			(consol as ICO2eParent).SetCO2ePerTonneInKg(100);

			Factory.Save();
			AssertEquals(1, transport.JobCO2eCollection.Count);

			// Act
			consol.Transports.RemoveAndDelete(transport);

			// Assert
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestJobCO2eIsDeletedWhenDeletingTransport()
		{
			// Arrange
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.SetCO2ePerTonneInKg(100);
			(consol as ICO2eParent).SetCO2ePerTonneInKg(100);

			Factory.Save();
			AssertEquals(1, transport.JobCO2eCollection.Count);
			var jobCO2e = transport.JobCO2eCollection.Cast<JobCO2e>().First();

			// Act
			consol.Transports.RemoveAndDelete(transport);
			Factory.Save();

			// Assert
			Assert(jobCO2e.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<JobCO2e>(jobCO2e.PK));
		}

		/// <summary>
		/// There are cases like UXML importing, fetching hints for related consols, or phase screening, a transport is created/loaded/saved when ParentType is null.
		/// Eventually, get_JW_SetAllScheduleFields and then set_ShippingLine are called.
		/// </summary>
		public void TestSetShippingLine_WhenParentTypeIsNull_ShouldSetValueWithoutThrowingException()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.Voyage.JV_OH_Line = carrier.PK;

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var transportInNewFactory = newFactory.Load<Transport>(transport.PK);

			AssertEquals("Pre-condition: ParentType should be null", null, transportInNewFactory.ParentType);
			((ISailingManaged)transportInNewFactory).ShippingLine = carrier.PK;

			AssertEquals("Carrier address should be set without exception", carrier.MainAddress.PK, transportInNewFactory.JW_OA_CarrierAddress);
		}

		#region Removing ETD/ETA Events

		public void TestTransports_ETDEventShouldNotBeCancelled()
		{
			var factory = new BusinessObjectFactory();

			var shipment1 = factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_RL_NKLoadPort = "CHBSL";
			shipment1.JS_RL_NKDischargePort = "USLAX";

			var shipment2 = factory.NewWithValidTestData<CommonShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_RL_NKLoadPort = "CHBSL";
			shipment2.JS_RL_NKDischargePort = "USLAX";

			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001210";
			consol.JK_RL_NKLoadPort = "CHBSL";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = factory.New<MockTransport>();
			consol.Transports.Add(transport);
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			transport.JW_VoyageFlight = "LH123T";
			transport.JW_RL_NKLoadPort = "CHBSL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 40, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 20, 15, 0, 0);

			shipment1.Consols.Add(consol);
			shipment2.Consols.Add(consol);

			factory.Save();

			transport.JW_ETD = new ZDateTime(2023, 5, 20, 9, 15, 0);

			factory.Save();

			AssertNotEquals("Precondition", ZDateTime.Empty, transport.JW_ETD);
		}

		class MockTransport : Transport
		{
			int Counter;
			public MockTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override StmALog CreateEvent(ZDateTimeOffset oldValue, ZDateTimeOffset newValue, Event eventType, bool isEstimate, IDictionary<string, string> referenceParameters, IStmALogParent logParent)
			{
				if (eventType.Code == Events.DepartureCode && Counter > 0)
				{
					return null;
				}
				else
				{
					if (eventType.Code == Events.DepartureCode)
					{
						Counter++;
					}
					return base.CreateEvent(oldValue, newValue, eventType, isEstimate, referenceParameters, logParent);
				}
			}
		}

		#endregion

		#region Shipment Details

		public void TestShipmentDetails()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			AddShipment(5, 200m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres);
			AddShipment(10, 500m, Constants.Weight.Kilograms, 40m, Constants.Volume.CubicMetres);

			var transport = consol.Transports.Cast<Transport>().FirstOrDefault();
			AssertNotNull(transport);

			AssertEquals(15, transport.TotalShipmentPieces);
			AssertEquals("700 KG", transport.TotalShipmentWeight);
			AssertEquals("43 M3", transport.TotalShipmentVolume);
			AssertEquals(ZDateTime.Empty, transport.ShipmentDeliveredTime);

			void AddShipment(ZInt outerPacks, ZDecimal actualWeight, ZString weightUnit, ZDecimal actualVolume, ZString volumeUnit)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_OuterPacks = outerPacks;
				shipment.JS_ActualWeight = actualWeight;
				shipment.JS_UnitOfWeight = weightUnit;
				shipment.JS_ActualVolume = actualVolume;
				shipment.JS_UnitOfVolume = volumeUnit;
			}
		}

		#endregion

		#region Customized Has Changes

		public void TestcustomizedOriginalValuesHasChanges()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			Transport.JW_IsLinked = false;
			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "CNSHG";
			Transport.JW_Vessel = "CONDOR1";
			Transport.JW_VoyageFlight = "0122";
			Transport.JW_ETD = new ZDateTime(2023, 02, 21);
			Transport.JW_ETA = new ZDateTime(2023, 02, 21);
			Transport.JW_OA_CarrierAddress = ZGuid.Empty;
			Transport.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 21);
			Transport.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 21);
			Transport.JW_TerminalCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_DepotCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_VGMCutOff = new ZDateTime(2023, 02, 21);

			Assert(!Transport.IsInDatabase);
			Assert(!Transport.JW_RL_NKLoadPortHasChanges());
			Assert(!Transport.JW_RL_NKDiscPortHasChanges());
			Assert(!Transport.JW_VesselHasChanges());
			Assert(!Transport.JW_VoyageFlightHasChanges());
			Assert(!Transport.JW_ETDHasChanges());
			Assert(!Transport.JW_ETAHasChanges());
			Assert(!Transport.JW_OA_CarrierAddressHasChanges());
			Assert(!Transport.JW_TerminalReceivalCommencesHasChanges());
			Assert(!Transport.JW_DepotReceivalCommencesHasChanges());
			Assert(!Transport.JW_TerminalCutOffHasChanges());
			Assert(!Transport.JW_DepotCutOffHasChanges());
			Assert(!Transport.JW_DocumentaryCutOffHasChanges());
			Assert(!Transport.JW_VGMCutOffHasChanges());

			Factory.Save();

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKLoadPort = "AUBNE";
			Transport.JW_RL_NKDiscPort = "SGSIN";
			Transport.JW_Vessel = "CONDOR";
			Transport.JW_VoyageFlight = "012";
			Transport.JW_ETD = new ZDateTime(2023, 02, 22);
			Transport.JW_ETA = new ZDateTime(2023, 02, 22);
			Transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			Transport.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 22);
			Transport.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 22);
			Transport.JW_TerminalCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_DepotCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_VGMCutOff = new ZDateTime(2023, 02, 22);

			Assert(Transport.IsInDatabase);
			Assert(Transport.JW_RL_NKLoadPortHasChanges());
			Assert(Transport.JW_RL_NKDiscPortHasChanges());
			Assert(Transport.JW_VesselHasChanges());
			Assert(Transport.JW_VoyageFlightHasChanges());
			Assert(Transport.JW_ETDHasChanges());
			Assert(Transport.JW_ETAHasChanges());
			Assert(Transport.JW_OA_CarrierAddressHasChanges());
			Assert(Transport.JW_TerminalReceivalCommencesHasChanges());
			Assert(Transport.JW_DepotReceivalCommencesHasChanges());
			Assert(Transport.JW_TerminalCutOffHasChanges());
			Assert(Transport.JW_DepotCutOffHasChanges());
			Assert(Transport.JW_DocumentaryCutOffHasChanges());
			Assert(Transport.JW_VGMCutOffHasChanges());

			Factory.Save();

			Assert(!Transport.JW_RL_NKLoadPortHasChanges());
			Assert(!Transport.JW_RL_NKDiscPortHasChanges());
			Assert(!Transport.JW_VesselHasChanges());
			Assert(!Transport.JW_VoyageFlightHasChanges());
			Assert(!Transport.JW_ETDHasChanges());
			Assert(!Transport.JW_ETAHasChanges());
			Assert(!Transport.JW_OA_CarrierAddressHasChanges());
			Assert(!Transport.JW_TerminalReceivalCommencesHasChanges());
			Assert(!Transport.JW_DepotReceivalCommencesHasChanges());
			Assert(!Transport.JW_TerminalCutOffHasChanges());
			Assert(!Transport.JW_DepotCutOffHasChanges());
			Assert(!Transport.JW_DocumentaryCutOffHasChanges());
			Assert(!Transport.JW_VGMCutOffHasChanges());
		}

		#endregion

		#region OnSaved

		public void TestUpdateOriginalValues()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			Factory.Save();

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKLoadPort = "AUBNE";
			Transport.JW_RL_NKDiscPort = "SGSIN";
			Transport.JW_Vessel = "CONDOR";
			Transport.JW_VoyageFlight = "012";
			Transport.JW_ETD = new ZDateTime(2023, 02, 22);
			Transport.JW_ETA = new ZDateTime(2023, 02, 22);
			Transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			Transport.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 22);
			Transport.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 22);
			Transport.JW_TerminalCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_DepotCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 22);
			Transport.JW_VGMCutOff = new ZDateTime(2023, 02, 22);

			Factory.Save();

			AssertEquals("AUBNE", Transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("SGSIN", Transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR", Transport.JW_VesselOriginalValue.Value);
			AssertEquals("012", Transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_ETDOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_ETAOriginalValue.Value);
			AssertEquals(carrier.MainAddress.PK, Transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_VGMCutOffOriginalValue.Value);

			Transport.JW_IsLinked = false;
			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "CNSHG";
			Transport.JW_Vessel = "CONDOR1";
			Transport.JW_VoyageFlight = "0122";
			Transport.JW_ETD = new ZDateTime(2023, 02, 21);
			Transport.JW_ETA = new ZDateTime(2023, 02, 21);
			Transport.JW_OA_CarrierAddress = ZGuid.Empty;
			Transport.JW_TerminalReceivalCommences = new ZDateTime(2023, 02, 21);
			Transport.JW_DepotReceivalCommences = new ZDateTime(2023, 02, 21);
			Transport.JW_TerminalCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_DepotCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_DocumentaryCutOff = new ZDateTime(2023, 02, 21);
			Transport.JW_VGMCutOff = new ZDateTime(2023, 02, 21);

			AssertEquals("AUBNE", Transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("SGSIN", Transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR", Transport.JW_VesselOriginalValue.Value);
			AssertEquals("012", Transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_ETDOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_ETAOriginalValue.Value);
			AssertEquals(carrier.MainAddress.PK, Transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 22), Transport.JW_VGMCutOffOriginalValue.Value);

			Factory.Save();

			AssertEquals("AUSYD", Transport.JW_RL_NKLoadPortOriginalValue.Value);
			AssertEquals("CNSHG", Transport.JW_RL_NKDiscPortOriginalValue.Value);
			AssertEquals("CONDOR1", Transport.JW_VesselOriginalValue.Value);
			AssertEquals("0122", Transport.JW_VoyageFlightOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_ETDOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_ETAOriginalValue.Value);
			AssertEquals(ZGuid.Empty, Transport.JW_OA_CarrierAddressOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_TerminalReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_DepotReceivalCommencesOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_TerminalCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_DepotCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_DocumentaryCutOffOriginalValue.Value);
			AssertEquals(new ZDateTime(2023, 02, 21), Transport.JW_VGMCutOffOriginalValue.Value);
		}

		#endregion

		#region CO2e Copying

		public void TestShouldNotUpdateCO2eStatusToNotCurrentWhenCopyingTransports()
		{
			var transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			((IBusinessObjectInternals)transport).IsCopying = false;
			AssertCO2eStatus(transport, CO2eStatusList.Codes.NotCurrent);

			var transport2 = Factory.New<CommonConsol>().Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			((IBusinessObjectInternals)transport2).IsCopying = true;
			AssertCO2eStatus(transport, CO2eStatusList.Codes.Pending);
		}

		void AssertCO2eStatus(Transport transport, string expectedCO2eStatus)
		{
			void UpdateRelatedPropertyAndAssert(string message, Action updateProperty)
			{
				transport.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				transport.Validation.ValidateTotalCO2eForSorting();
				updateProperty.Invoke();
				AssertEquals(message, expectedCO2eStatus, transport.GetCO2eStatus());
			}

			UpdateRelatedPropertyAndAssert("Transport Mode", () => transport.JW_TransportMode = Core.Constants.TransportModes.Sea);
			UpdateRelatedPropertyAndAssert("Load Port", () => transport.JW_RL_NKLoadPort = "AUSYD");
			UpdateRelatedPropertyAndAssert("Discharge Port", () => transport.JW_RL_NKDiscPort = "NZAKL");
			UpdateRelatedPropertyAndAssert("Vessel", () => transport.JW_Vessel = "Vessel 1");
			UpdateRelatedPropertyAndAssert("Voyage/Flight", () => transport.JW_VoyageFlight = "Voyage 1");
			UpdateRelatedPropertyAndAssert("Aircraft Type", () => transport.JW_AircraftType = "123");
		}

		#endregion

		#region Implementation

		void AssertReadOnly(ZPropertyInfo info, bool expected)
		{
			AssertEquals(info.Name + ".ReadOnly", expected, info.ReadOnly);
		}

		public void AssertTransportModeFlags(string message, bool expectedAir, bool expectedSea, bool expectedRoad, bool expectedRail)
		{
			AssertEquals(message + " : Air", expectedAir, Transport.IsAir);
			AssertEquals(message + " : Sea", expectedSea, Transport.IsSea);
			AssertEquals(message + " : Road", expectedRoad, Transport.IsRoad);
			AssertEquals(message + " : Rail", expectedRail, Transport.IsRail);
		}

		void CheckProperty(ZPropertyInfo info, object value)
		{
			// The business object must be freashly loaded to correctly test
			// for the problem. DO NOT "OPTOMISE"!

			BusinessObjectFactory freshFactory = new BusinessObjectFactory();

			Transport transport2 = freshFactory.Load<Transport>(info.BizObj.PK);
			transport2.ParentType = typeof(CommonConsol);
			transport2[info.Name] = value;
			AssertEquals(info.Name, value, transport2[info.Name]);
		}

		void AssertCellEquals(string fieldName, object expected, IBusinessObjectInternals bO)
		{
			AssertEquals(fieldName, expected, bO.Row[fieldName]);
		}

		ZPropertyInfo[] GetPercistedPropertiesThatShouldStayTheSameWhenIsLinkedChanges(Transport transport)
		{
			List<ZPropertyInfo> list = new List<ZPropertyInfo>();

			foreach (ZPropertyInfo info in transport.ZPropertyInfoHash)
			{
				if (
					info.HasSetter &&
					info.Name != JobConsolTransportSchema.Constants.JW_JX &&
					info.Name != JobConsolTransportSchema.Constants.JW_IsLinked &&
					typeof(AutoJobConsolTransport).GetProperty(info.Name) != null
					)
				{
					list.Add(info);
				}
			}

			return list.ToArray();
		}

		void AssertCleared(ZPropertyInfo info)
		{
			AssertEquals(info.Name + " should be empty.", Activator.CreateInstance(info.PropertyType), info.Value);
		}

		void AssertSynchronisedProxiedProperties(Transport transport)
		{
			var message = "property should be consistent with proxied property";

			AssertEquals("JW_STD " + message, transport.JW_STD, transport.Sailing.Origin.JA_S_DEP);
			AssertEquals("JW_ETD " + message, transport.JW_ETD, transport.Sailing.Origin.JA_E_DEP);
			AssertEquals("JW_ATD " + message, transport.JW_ATD, transport.Sailing.Origin.JA_A_DEP);
			AssertEquals("JW_RL_NKLoadPort " + message, transport.JW_RL_NKLoadPort, transport.Sailing.Origin.JA_RL_NKPortOfLoading);

			AssertEquals("JW_STA " + message, transport.JW_STA, transport.Sailing.Destination.JB_S_ARV);
			AssertEquals("JW_ETA " + message, transport.JW_ETA, transport.Sailing.Destination.JB_E_ARV);
			AssertEquals("JW_ATA " + message, transport.JW_ATA, transport.Sailing.Destination.JB_A_ARV);
			AssertEquals("JW_RL_NKDiscPort " + message, transport.JW_RL_NKDiscPort, transport.Sailing.Destination.JB_RL_NKPortOfDischarge);
		}

		JobSailing NewSailing(RefVessel vessel, ZString voyageFlight, ZString load, ZString discharge)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		Transport Transport
		{
			get
			{
				if (fTransport == null)
				{
					fTransport = Factory.New<Transport>();
					fTransport.ParentType = typeof(CommonShipment);
				}

				return fTransport;
			}
		}

		Transport fTransport;

		GlbBranch Branch
		{
			get
			{
				if (branch == null)
				{
					var company = Factory.New<GlbCompany>();
					company.GC_Code = "TSC";

					branch = company.Branches.AddNew();
					branch.GB_Code = "TST";
				}

				return branch;
			}
		}

		GlbBranch branch;

		ZPropertyInfo GetSailingInfo(JobSailing sailing, string transportProperty)
		{
			switch (transportProperty)
			{
				case JobConsolTransportSchema.Constants.JW_ATA:
					return sailing.Destination.JB_A_ARVInfo;

				case JobConsolTransportSchema.Constants.JW_ATD:
					return sailing.Origin.JA_A_DEPInfo;

				case JobConsolTransportSchema.Constants.JW_ETA:
					return sailing.Destination.JB_E_ARVInfo;

				case JobConsolTransportSchema.Constants.JW_ETD:
					return sailing.Origin.JA_E_DEPInfo;

				case JobConsolTransportSchema.Constants.JW_STA:
					return sailing.Destination.JB_S_ARVInfo;

				case JobConsolTransportSchema.Constants.JW_STD:
					return sailing.Origin.JA_S_DEPInfo;

				case JobConsolTransportSchema.Constants.JW_RL_NKDiscPort:
					return sailing.Destination.JB_RL_NKPortOfDischargeInfo;

				case JobConsolTransportSchema.Constants.JW_RL_NKLoadPort:
					return sailing.Origin.JA_RL_NKPortOfLoadingInfo;

				case JobConsolTransportSchema.Constants.JW_Vessel:
					return sailing.Voyage.JV_RV_NKVesselInfo;

				case JobConsolTransportSchema.Constants.JW_VoyageFlight:
					return sailing.Voyage.JV_VoyageFlightInfo;

				case Transport.Schema.JW_TerminalReceivalCommences:
					return sailing.Origin.JA_ReceivalCommencesInfo;

				case Transport.Schema.JW_DepotAvailabilityDate:
					return sailing.JX_DepotAvailabilityDateInfo;

				case Transport.Schema.JW_JX_JV_RegistrationNo:
					return sailing.Voyage.JV_RegistrationNoInfo;

				case Transport.Schema.JW_IsCargoOnly:
					return sailing.Voyage.JV_IsCargoOnlyInfo;

				case Transport.Schema.JW_DepotReceivalCommences:
					return sailing.JX_DepotReceivalCommencesInfo;

				case Transport.Schema.JW_TerminalCutOff:
					return sailing.Origin.JA_CutOffInfo;

				case Transport.Schema.JW_DepotCutOff:
					return sailing.JX_DepotCutOffInfo;

				case Transport.Schema.JW_DocumentaryCutOff:
					return sailing.Origin.JA_DocumentaryCutoffInfo;

				case Transport.Schema.JW_VGMCutOff:
					return sailing.Origin.JA_VGMCutOffInfo;

				case Transport.Schema.JW_TerminalAvailabilityDate:
					return sailing.Destination.JB_AvailabilityDateInfo;

				case Transport.Schema.JW_TerminalStorageDate:
					return sailing.Destination.JB_StorageDateInfo;

				case Transport.Schema.JW_DepotStorageDate:
					return sailing.JX_DepotStorageDateInfo;

				case Transport.Schema.JW_JX_Load_ETA:
					return sailing.Origin.JA_E_ARVInfo;

				case Transport.Schema.JW_JX_Load_ATA:
					return sailing.Origin.JA_A_ARVInfo;

				case Transport.Schema.JW_JX_IsPublished:
					return sailing.JX_IsPublishedInfo;

				case Transport.Schema.JW_OA_ArrivalLocation:
					return sailing.Destination.JB_OA_ArrivalCTOAddressInfo;

				case Transport.Schema.JW_OA_DepartureLocation:
					return sailing.Origin.JA_OA_DepartureCTOAddressInfo;

				case Transport.Schema.JW_EmptyReceivalCommences:
					return sailing.Origin.JA_EmptyReceivalCommencesInfo;

				case Transport.Schema.JW_EmptyCutOff:
					return sailing.Origin.JA_EmptyCutOffInfo;

				case Transport.Schema.JW_DGReceivalCommences:
					return sailing.Origin.JA_DGReceivalCommencesInfo;

				case Transport.Schema.JW_DGCutOff:
					return sailing.Origin.JA_DGCutOffInfo;

				case Transport.Schema.JW_ReeferReceivalCommences:
					return sailing.Origin.JA_ReeferReceivalCommencesInfo;

				case Transport.Schema.JW_ReeferCutOff:
					return sailing.Origin.JA_ReeferCutOffInfo;

				case Transport.Schema.JW_ServiceString:
					return sailing.JX_ServiceStringInfo;

				case Transport.Schema.JW_ArrivalPortRouteId:
					return sailing.JX_ArrivalPortRouteIdInfo;

				case Transport.Schema.JW_DeparturePortRouteId:
					return sailing.JX_DeparturePortRouteIdInfo;

				default:
					throw new ApplicationException("Dont know how to handle " + transportProperty);
			}
		}

		void Populate(ZPropertyInfo info, int seed)
		{
			info.Value = GetValueForPopulation(info, seed);
		}

		IZType GetValueForPopulation(ZPropertyInfo info, int seed)
		{
			return GetValueForPopulation(info, seed, null);
		}

		IZType GetValueForPopulation(ZPropertyInfo info, int seed, Type typeForConstraint)
		{
			if (info.PropertyType == typeof(ZString))
			{
				int maxLen;

				if (info.MaxLength < 0 || info.MaxLength > 50)
				{
					maxLen = 10;
				}
				else
				{
					maxLen = info.MaxLength;
				}

				return (ZString)seed.ToString().PadLeft(maxLen >> 1, '<').PadRight(maxLen, '>');
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				return ZDateTime.Today.AddDays(seed);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				return (ZBool)((seed & 1) != 0);
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				return (ZDecimal)seed;
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				return (ZInt)seed;
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				return (ZShort)seed;
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				return (ZByte)seed;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				if (typeForConstraint != null)
				{
					return Factory.NewWithValidTestData(typeForConstraint).PK;
				}

				return ZGuid.NewZGuid();
			}
			else
			{
				throw new ApplicationException("dont know how to populate " + info.PropertyType.Name);
			}
		}

		void AssertPropertyValues(string message, Dictionary<string, IZType> expected, ZPropertyInfo[] properties)
		{
			foreach (ZPropertyInfo info in properties)
			{
				AssertEquals(message + " : " + info.Name, expected[info.Name], info.Value);
			}
		}

		Dictionary<string, IZType> RecordPropertyValues(ZPropertyInfo[] properties)
		{
			Dictionary<string, IZType> result = new Dictionary<string, IZType>();

			foreach (ZPropertyInfo info in properties)
			{
				if (!result.ContainsKey(info.Name))
				{
					result.Add(info.Name, info.Value);
				}
			}

			return result;
		}

		BaseSailingManager GetSailingManager(Transport transport)
		{
			return (BaseSailingManager)typeof(Transport).GetField("fSailingManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transport);
		}

		void AssertNotDirty(string message, Transport transport)
		{
			BaseSailingManager manager = GetSailingManager(transport);
			AssertNotNull(message + " : No SailingManager found", manager);
			AssertEquals(message + " : SailingManager should not be dirty", false, manager.Dirty);
		}

		ReadOnlyCollection<ZPropertyInfo> GetProxiedInfoFieldsOnTransport(Transport transport)
		{
			return new ReadOnlyCollection<ZPropertyInfo>(new List<ZPropertyInfo>
			{
				transport.JW_VesselInfo,
				transport.JW_VoyageFlightInfo,
				transport.JW_IsCharterInfo,
				transport.JW_AircraftTypeInfo,
				transport.JW_OA_DepartureLocationInfo,
				transport.JW_RL_NKLoadPortInfo,
				transport.JW_STDInfo,
				transport.JW_ETDInfo,
				transport.JW_ATDInfo,
				transport.JW_RL_NKDiscPortInfo,
				transport.JW_OA_ArrivalLocationInfo,
				transport.JW_STAInfo,
				transport.JW_ETAInfo,
				transport.JW_ATAInfo
			});
		}

		Transport CreateTransportForMatching()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Air;
			transport.JW_VoyageFlight = "QF1234";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = ZDate.Today;
			return transport;
		}

		#endregion

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}
	}
}
