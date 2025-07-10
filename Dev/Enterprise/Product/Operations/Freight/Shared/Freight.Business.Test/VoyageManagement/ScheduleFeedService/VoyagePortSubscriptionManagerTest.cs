using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyagePortSubscriptionManagerTest : TestCaseWithFactory
	{
		public void TestUpdate_SCACEmpty_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_SCACIncorrect_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_VesselEmpty_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_VoyageNumberEmpty_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_VoyageNumberZeros_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "000";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_UnlocoEmpty_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_UnlocoIncorrect_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUS";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_DateEmpty_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_VesselImoInvalid_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "2222222";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_TransportModeIncorrect_SBRLogNotSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Rail;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(0, sbrLogs.Count());
		}

		public void TestUpdate_SBRLogSent()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";
			vessel.RV_LloydsNumber = "9308390";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_VoyageFlight = "321AV";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = DateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = DateTime.Today.AddDays(2);

			var subscriptionManager = new VoyagePortSubscriptionManager(origin, voyage, Factory);
			subscriptionManager.Update();

			var sbrLogs = origin.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code);

			AssertEquals(1, sbrLogs.Count());
		}
	}
}
