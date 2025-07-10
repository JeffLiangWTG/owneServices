using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseJobVoyDestinationValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateJB_RL_NKPortOfDischarge

		public void TestValidateJB_RL_NKPortOfDischarge()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobVoyage testVoyage = testFactory.New(typeof(JobVoyage)) as JobVoyage;
			VoyageDestination testDestination = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination testDestination2 = testFactory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageOrigin testOrigin = testFactory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			JobSailing testSailing = testFactory.New(typeof(JobSailing)) as JobSailing;
			RefUNLOCO loco = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO loco2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USSEA") as RefUNLOCO;
			RefUNLOCO loco3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USSEA") as RefUNLOCO;

			testVoyage.Origins.Add(testOrigin);
			testVoyage.Destinations.Add(testDestination);
			testVoyage.Destinations.Add(testDestination2);

			testDestination.JB_RL_NKPortOfDischarge = "AAAAA";
			Assert("Invalid UNLOCO code, error expected", testDestination.JB_RL_NKPortOfDischargeInfo.HasErrors());

			testOrigin.JA_RL_NKPortOfLoading = loco.RL_Code;
			testDestination.JB_RL_NKPortOfDischarge = loco2.RL_Code;
			testDestination2.JB_RL_NKPortOfDischarge = loco3.RL_Code;

			testVoyage.GenerateSailings();
			Assert("All UNLOCOs are unique, no error on Test Destination", !testDestination.JB_RL_NKPortOfDischargeInfo.HasErrors());

			testDestination2.JB_RL_NKPortOfDischarge = loco2.RL_Code;
			Assert("Duplicate UNLOCOs as destination, error expected", testDestination2.JB_RL_NKPortOfDischargeInfo.HasErrors());
		}

		public void TestValidateJB_RL_NKPortOfDischarge_PortCodeWarnings()
		{
			SailingsForTestClasses helper = new SailingsForTestClasses(new BusinessObjectFactory());

			VoyageDestination laxSeaDest = helper.LaxVoyDestination;

			laxSeaDest.PortOfDischarge.RL_HasSeaport = true;
			laxSeaDest.Validation.ValidateJB_RL_NKPortOfDischarge();
			Assert("Lax has seaport, not expecting warnings.", !laxSeaDest.HasWarnings);

			laxSeaDest.PortOfDischarge.RL_HasSeaport = false;
			laxSeaDest.Validation.ValidateJB_RL_NKPortOfDischarge();
			Assert("Lax does not have seaport, expecting warnings.", laxSeaDest.HasWarnings);

			VoyageDestination laxAirDest = helper.LaxFlightDestination;

			laxAirDest.PortOfDischarge.RL_HasAirport = true;
			laxAirDest.Validation.ValidateJB_RL_NKPortOfDischarge();
			Assert("Lax has airport, not expecting warnings.", !laxAirDest.HasWarnings);

			laxAirDest.PortOfDischarge.RL_HasAirport = false;
			laxAirDest.Validation.ValidateJB_RL_NKPortOfDischarge();
			Assert("Lax does not have airport, expecting warnings.", laxAirDest.HasWarnings);
		}

		#endregion

		#region TestValidateJB_RL_NKPortOfDischargeAgainstTradeLane

		public void TestValidateJB_RL_NKPortOfDischargeAgainstTradeLane()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination destination = voyage.Destinations.AddNew();
			VoyageOrigin origin = voyage.Origins.AddNew();

			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLane.EJ_Location1 = "HKHKG";
			tradeLane.EJ_Location2 = "AUSYD";
			tradeLane.EJ_OH_RelatedOrg = principal.PK;
			JobTradeLaneVoyage tradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			tradeLaneVoyage.NB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "HKHKG";

			AssertHasError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			tradeLane.EJ_Direction = DirectionTypeList.Codes.BothWays;
			destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			tradeLane.EJ_Direction = DirectionTypeList.Codes.OneWay;
			destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			AssertHasError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			tradeLaneVoyage.NB_EJ = ZGuid.Empty;
			destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			tradeLane.EJ_Location2 = "AU";
			destination.JB_RL_NKPortOfDischarge = "AUPER";
			destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");

			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.Countries.Add(new RefCountry.Loader(Factory).LoadForCountry("AU"));
			zone.FZ_Code = "Zone";
			tradeLane.EJ_Location2 = zone.FZ_Code;
			destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			AssertNoError(destination.JB_RL_NKPortOfDischargeInfo, "This port does not match any of the specified trade lanes.");
		}

		#endregion

		#region TestValidateJB_E_ARV

		public void TestValidateJB_E_ARV_Sea()
		{
			int year = ZDateTime.Now.Year - 1;
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			//Departure Voyage - DEP date is mandatory

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Origin.JA_E_DEP = new ZDateTime(year, 11, 20);
			Origin2.JA_E_DEP = new ZDateTime(year, 11, 25);

			AssertEquals("Expecting Voyage to have two origins", 2, Voyage.Origins.Count);

			Destination.JB_E_ARV = new ZDateTime(year + 1, 6, 21, 18, 36, 8);
			Destination.Validation.ValidateJB_E_ARV();
			AssertHasErrors("Departure arrival is more than 6 months before departure, error expected", Destination.JB_E_ARVInfo);

			Destination.JB_E_ARV = ZDateTime.Empty;
			Destination.Validation.ValidateJB_E_ARV();
			Assert("ETA is not mandatory, warning expected", Destination.JB_E_ARVInfo.HasNotifications());
			Assert("ETA is not mandatory, no errors expected", !Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(year, 11, 23);
			Voyage.GenerateSailings();
			AssertEquals("Destination arrival is before departure, not expecting sailing to be generated.", 1, Voyage.Sailings.Count);

			Destination.JB_E_ARV = new ZDateTime(year, 11, 29);
			Voyage.GenerateSailings();
			AssertEquals("Destination arrival is not before departure, expecting sailing to be generated.", 2, Voyage.Sailings.Count);

			//Arrival Voyage -  Dep date isn't mandatory

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USSFO";
			Origin2.JA_RL_NKPortOfLoading = "USLAX";

			Destination.JB_E_ARV = ZDateTime.Empty;
			Assert("ETA cannot be empty, errors expected.", Destination.JB_E_ARVInfo.HasErrors());

			Origin.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(year, 11, 10);
			Voyage.GenerateSailings();
			AssertEquals("ETA is before ETD, not expecting sailing to be generated.", 1, Voyage.Sailings.Count);

			Destination.JB_E_ARV = new ZDateTime(year, 12, 10);
			Assert("ETA is valid, no errors expected.", !Destination.JB_E_ARVInfo.HasErrors());

			Origin2.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(year, 10, 25);
			Assert("ETA is valid, no errors expected.", !Destination.JB_E_ARVInfo.HasErrors());

			Voyage.CurrentCountry.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			Destination.JB_E_ARV = ZDateTime.Empty;
			AssertHasErrors("ETA is mandatory, expecting errors", Destination.JB_E_ARVInfo);
		}

		[TestDate(2013, 12, 1)]
		public void TestValidateJB_E_ARV_Air()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			//Departure Voyage - DEP date is mandatory

			Origin.JA_RL_NKPortOfLoading = "MYBAG";
			Origin2.JA_RL_NKPortOfLoading = "SGSIN";
			Destination.JB_RL_NKPortOfDischarge = "HKHKG";

			Destination.JB_IsTranshipment = ZBool.False;

			Origin.JA_E_DEP = new ZDateTime(2013, 1, 12, 15, 36, 8);
			Origin2.JA_E_DEP = new ZDateTime(2013, 1, 11, 22, 36, 8);

			Destination.JB_E_ARV = ZDateTime.Empty;
			Destination.Validation.ValidateJB_E_ARV();
			Assert("ETA is not mandatory, warning expected", Destination.JB_E_ARVInfo.HasNotifications());
			Assert("ETA is not mandatory, no errors expected", !Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 11, 8, 15, 0);
			Assert("Destination arrival (in UTC) is before one of the departures, error expected", Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 14, 18, 36, 8);
			Assert("Destination arrival is more than 6 months after departure, error expected", Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 13, 23, 30, 0);
			Assert("Destination arrival is more than 6 months after one of the departure times, error expected", Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 5, 9, 10, 0, 0);
			Assert("Destination arrival is less than 6 months after all departure times, no errors expected", !Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 12, 18, 15, 0);
			Assert("Destination arrival (in UTC) is after departure, no errors expected", !Destination.JB_E_ARVInfo.HasErrors());

			// Arrival voyage - DEP date is not mandatory

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USSFO";
			Origin2.JA_RL_NKPortOfLoading = "USLAX";

			Destination.JB_E_ARV = ZDateTime.Empty;
			Assert("ETA cannot be empty, errors expected.", Destination.JB_E_ARVInfo.HasErrors());

			Origin.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 16, 22, 36, 8);
			Assert("ETA is too far after ETD, errors expected.", Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 11, 23, 0, 0);
			Assert("ETA is valid, no errors expected.", !Destination.JB_E_ARVInfo.HasErrors());

			Origin2.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 25, 0, 0, 0);
			Assert("ETA is valid, no errors expected.", !Destination.JB_E_ARVInfo.HasErrors());

			// checking for invalid dates

			Origin.JA_E_DEP = ZDateTime.Invalid;
			Destination.JB_E_ARV = ZDateTime.Invalid;
			Assert("ETA is invalid, expecting errors.", Destination.JB_E_ARVInfo.HasErrors());

			Origin.JA_E_DEP = ZDateTime.Now;
			Assert("ETA is invalid, expecting errors.", Destination.JB_E_ARVInfo.HasErrors());

			Destination.JB_E_ARV = ZDateTime.Now;
			Assert("ETA/ETD are valid, not expecting errors.", !Destination.JB_E_ARVInfo.HasErrors());
		}

		public void TestValidateJB_E_ARV()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Voyage.JV_VoyageFlight = "QF69";

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			Voyage.Origins.Add(Origin);
			Voyage.Destinations.Add(Destination);
			Voyage.GenerateSailings();

			var sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);

			AssertHasError(Destination.JB_E_ARVInfo, "ETA is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 9, 22, 36, 8);

			AssertHasError(Destination.JB_E_ARVInfo, "ETA is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Factory.Save();

			Destination.JB_E_ARV = new ZDateTime(2020, 1, 9, 15, 36, 8);

			AssertHasError(Destination.JB_E_ARVInfo, "ETA is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);

			AssertHasError(Destination.JB_E_ARVInfo, "ETA is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");
		}
		#endregion

		#region TestValidateJB_A_ARV

		[TestDate(2011, 6, 26)]
		public void TestValidateJB_A_ARV()
		{
			var today = ZDateTime.Today;
			var estimateVerseActualMessage = "ATA must be within 1 month of ETA.";
			var futureDateMessage = "The Actual Time of Arrival cannot be set in the future. The date 26-Aug-11 00:00 is in the future for AUBNE (UTC+0).";

			Destination.JB_RL_NKPortOfDischarge = "AUBNE";
			Destination.JB_E_ARV = today.AddMonths(-1).AddDays(-1);
			Destination.JB_A_ARV = ZDateTime.Empty;
			AssertNoErrors("Pre-condition: ATA should not have errors.", Destination.JB_A_ARVInfo);

			Destination.JB_A_ARV = today;
			AssertHasError("Error expected as ETA and ATA dates are more than a month apart", Destination.JB_A_ARVInfo, estimateVerseActualMessage);

			Destination.JB_A_ARV = today.AddMonths(-6);
			AssertHasError("Error still expected as ETA and ATA dates are still more than a month apart", Destination.JB_A_ARVInfo, estimateVerseActualMessage);

			Destination.JB_A_ARV = today.AddMonths(-1).AddDays(1);
			AssertNoErrors("No error expected as expected as ETA and ATA dates are less than a month apart.", Destination.JB_A_ARVInfo);

			Destination.JB_A_ARV = today.AddMonths(2);
			AssertHasError("Error expected as ATA has been set to a future date", Destination.JB_A_ARVInfo, futureDateMessage);
			AssertHasError("Error expected as ETA and ATA dates are more than a month apart", Destination.JB_A_ARVInfo, estimateVerseActualMessage);

			Destination.JB_A_ARV = ZDateTime.Empty;
			AssertNoErrors("No error expected for an empty ATA.", Destination.JB_A_ARVInfo);
		}

		#endregion

		#region TestValidateScheduleArrivalDate

		public void TestValidateScheduleArrivalDate()
		{
			var today = ZDateTime.Today;

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Destination.JB_RL_NKPortOfDischarge = "AUBNE";
			Destination.JB_E_ARV = today;
			Destination.JB_S_ARV = ZDateTime.Empty;
			AssertNoWarnings("Pre-condition: STA should not have warnings.", Destination.JB_S_ARVInfo);

			Destination.JB_S_ARV = today;
			AssertNoWarnings("STA should not have warnings.", Destination.JB_S_ARVInfo);

			Destination.JB_E_ARV = today.AddDays(-1);
			AssertNoWarnings("STA should not have warnings.", Destination.JB_S_ARVInfo);

			Destination.JB_E_ARV = today.AddDays(1);
			AssertNoWarnings("STA should not have warnings.", Destination.JB_S_ARVInfo);

			Destination.JB_E_ARV = today.AddDays(-2);
			AssertHasWarning("STA should have warnings.", Destination.JB_S_ARVInfo, "STA is more than a day after ETA");

			Destination.JB_E_ARV = today.AddDays(2);
			AssertHasWarning("STA should have warnings.", Destination.JB_S_ARVInfo, "ETA is more than a day after STA");

			Destination.JB_S_ARV = today.AddDays(2);
			AssertNoWarnings("STA should not have warnings.", Destination.JB_S_ARVInfo);

			Destination.JB_S_ARV = today.AddDays(4);
			AssertHasWarning("STA should have warnings.", Destination.JB_S_ARVInfo, "STA is more than a day after ETA");

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Destination.Validation.ValidateJB_S_ARV();
			AssertNoWarnings("STA should not have warnings.", Destination.JB_S_ARVInfo);
		}

		#endregion

		#region TestValidateJB_AvailabilityDate

		public void TestValidateJB_AvailabilityDateFromATABeforeETA()
		{
			Destination.JB_E_ARV = new ZDateTime(2017, 1, 4);
			Destination.JB_AvailabilityDate = new ZDateTime(2017, 1, 3);
			AssertHasError("Availability Date must be after estimated time of arrival", Destination.JB_AvailabilityDateInfo, "CTO Availability date cannot be prior to the ETA.");

			Destination.JB_A_ARV = new ZDateTime(2017, 1, 2);
			AssertNoErrors("Availability Date is after actual time of arrival which takes precedence over estimated time of arrival, no error expected", Destination.JB_AvailabilityDateInfo);

			Destination.JB_A_ARV = ZDateTime.Empty;
			AssertHasError("Availability Date should be validated to ETA when ATA is empty", Destination.JB_AvailabilityDateInfo, "CTO Availability date cannot be prior to the ETA.");

			Destination.JB_E_ARV = new ZDateTime(2017, 1, 2);
			AssertNoErrors("ETA is now before Availability Date, no error expected", Destination.JB_AvailabilityDateInfo);

			Destination.JB_A_ARV = new ZDateTime(2017, 1, 4);
			AssertHasError("Availability Date is after ETA but before ATA", Destination.JB_AvailabilityDateInfo, "CTO Availability date cannot be prior to the ATA.");
		}

		#endregion

		#region TestValidateCircularRoute

		public void TestValidateCircularRouteAndDuplicateDestinations()
		{
			ZDateTime today = ZDateTime.Today;

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = today.AddDays(11);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.Validation.ValidateJB_E_ARV();

			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = today.AddDays(10);
			AssertNoErrors("OK for arrival at port before departure", destination.JB_E_ARVInfo);

			destination.JB_E_ARV = today.AddDays(12);
			AssertNoErrors("OK to arrive back at the same port", destination.JB_E_ARVInfo);

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_E_ARV = today.AddDays(14);
			AssertNoErrors(destination2.JB_RL_NKPortOfDischargeInfo);
			AssertNoErrors(destination2.JB_E_ARVInfo);

			destination2.JB_RL_NKPortOfDischarge = "AUBNE";
			AssertHasError("Can't discharge at the same port twice", destination2.JB_RL_NKPortOfDischargeInfo, "The Discharge Port has been duplicated and must be unique.");
		}

		#endregion

		#region TestValidateJB_E_ARVUnsafeChange

		public void TestValidateJB_E_ARVUnsafeChange_Air()
		{
			ZDateTime now = ZDateTime.Now;
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Origin.JA_RL_NKPortOfLoading = "SGSIN";
			Origin.JA_E_DEP = now.AddHours(25);
			Origin2.JA_RL_NKPortOfLoading = "MYBAG";
			Origin2.JA_E_DEP = now.AddHours(28);
			Destination.JB_RL_NKPortOfDischarge = "HKHKG";
			Destination.JB_E_ARV = now.AddHours(30);
			Voyage.GenerateSailings();
			Factory.Save();

			AssertEquals("Precondition: Expecting Flight to have 2 sailings", 2, Voyage.Sailings.Count);

			JobSailing sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("MYBAG", "HKHKG");

			AssertNotNull(sailing);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = sailing.PK;

			Destination.JB_E_ARV = now;

			Assert("Expecting an error.", Destination.JB_E_ARVInfo.HasErrors());

			AssertEquals("Expecting error:", "ETA is invalid for flight leg Port Pairs on this schedule: MYBAG -> HKHKG.", Destination.JB_E_ARVInfo.GetErrors().GetFirstMessage());

			Destination.JB_E_ARV = now.AddDays(-1);
			Destination.JB_E_ARV = now;

			Assert("Expecting an error.", Destination.JB_E_ARVInfo.HasErrors());

			AssertEquals("Expecting error:", "ETA is invalid for flight leg Port Pairs on this schedule: MYBAG -> HKHKG.", Destination.JB_E_ARVInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidateJB_E_ARVUnsafeChange_Sea_Road_Rail()
		{
			ZDateTime today = ZDateTime.Today;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin.JA_E_DEP = today;
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Origin2.JA_E_DEP = today.AddDays(5);
			Destination.JB_RL_NKPortOfDischarge = "HKHKG";
			Destination.JB_E_ARV = today.AddDays(7);
			Voyage.GenerateSailings();
			Factory.Save();

			AssertEquals("Precondition: Expecting Journey to have 2 sailings", 2, Voyage.Sailings.Count);

			JobSailing sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "HKHKG");

			AssertNotNull(sailing);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = sailing.PK;

			Destination.JB_E_ARV = today.AddDays(3);

			Assert("Expecting an error.", Destination.JB_E_ARVInfo.HasErrors());

			AssertEquals("Expecting error:", "ETA is invalid for sailing Port Pairs on this schedule: AUBNE -> HKHKG.", Destination.JB_E_ARVInfo.GetErrors().GetFirstMessage());
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

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin2 = Voyage.Origins.AddNew();

			Destination = Voyage.Destinations.AddNew();
		}

		#endregion
	}
}
