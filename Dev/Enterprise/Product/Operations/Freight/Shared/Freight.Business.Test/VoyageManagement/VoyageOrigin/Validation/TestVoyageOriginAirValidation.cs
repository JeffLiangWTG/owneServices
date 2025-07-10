using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginAirValidationTest : BaseJobVoyOriginValidationTest
	{
		#region JA_RL_NKPortOfLoading_PortCodeWarnings

		public void TestValidateJA_RL_NKPortOfLoading_PortCodeWarnings()
		{
			SailingsForTestClasses helper = new SailingsForTestClasses(new BusinessObjectFactory());
			VoyageOrigin melAirOrigin = helper.MelFlightOrigin;

			melAirOrigin.PortOfLoading.RL_HasAirport = true;
			melAirOrigin.Validation.ValidateJA_RL_NKPortOfLoading();
			Assert("Mel has airport, not expecting warnings.", !melAirOrigin.HasWarnings);

			melAirOrigin.PortOfLoading.RL_HasAirport = false;
			melAirOrigin.Validation.ValidateJA_RL_NKPortOfLoading();
			Assert("Mel does not have airport, expecting warnings.", melAirOrigin.HasWarnings);
		}

		#endregion

		#region TestValidateJA_E_DEP

		[TestDate(2013, 12, 1)]
		public void TestValidateJA_E_DEP_Air()
		{
			Voyage.JV_AirSeaRoad = TransportTypeCode;

			//Departure Voyage - DEP date is mandatory

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Destination.JB_IsTranshipment = ZBool.False;

			Origin.JA_E_DEP = new ZDateTime(2013, 1, 12, 15, 36, 8);
			Origin2.JA_E_DEP = new ZDateTime(2013, 1, 11, 22, 36, 8);

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 10, 22, 35, 7);
			Origin2.Validation.ValidateJA_E_DEP();
			Assert("Destination arrival is more than a day before departure, error expected", Origin2.JA_E_DEPInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 11, 8, 15, 0);
			Origin.Validation.ValidateJA_E_DEP();
			Assert("Destination arrival is more than a day before one of the departures, error expected", Origin.JA_E_DEPInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 14, 18, 36, 8);
			Origin.Validation.ValidateJA_E_DEP();
			Assert("Destination arrival is more than 6 months after departure, error expected", Origin.JA_E_DEPInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 13, 23, 30, 0);
			Origin2.Validation.ValidateJA_E_DEP();
			Assert("Destination arrival is more than 6 months after one of the departure times, error expected", Origin2.JA_E_DEPInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 5, 9, 10, 0, 0);
			Origin2.Validation.ValidateJA_E_DEP();
			Origin.Validation.ValidateJA_E_DEP();
			Assert("Destination arrival is less than 6 months after all departure times, no errors expected", !Origin2.JA_E_DEPInfo.HasErrors());

			// Arrival voyage - DEP date is not mandatory

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USSFO";
			Origin2.JA_RL_NKPortOfLoading = "USLAX";

			Destination.JB_E_ARV = ZDateTime.Empty;
			Assert("ETA cannot be empty, errors expected.", Destination.JB_E_ARVInfo.HasErrors());

			Origin.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(2013, 7, 16, 22, 36, 8);
			Origin2.Validation.ValidateJA_E_DEP();
			Assert("ETA is too far after ETD, errors expected.", Origin2.JA_E_DEPInfo.HasErrors());

			Destination.JB_E_ARV = new ZDateTime(2013, 4, 8, 23, 0, 0);
			Origin2.Validation.ValidateJA_E_DEP();
			Assert("ETA is valid, no errors expected.", !Origin2.JA_E_DEPInfo.HasErrors());

			Origin2.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(2013, 1, 25, 0, 0, 0);
			Origin2.Validation.ValidateJA_E_DEP();
			Assert("ETA is valid, no errors expected.", !Origin2.JA_E_DEPInfo.HasErrors());

			// checking for invalid dates

			Origin.JA_E_DEP = ZDateTime.Invalid;
			Destination.JB_E_ARV = ZDateTime.Invalid;
			Assert("ETD is invalid, expecting errors.", Origin.JA_E_DEPInfo.HasErrors());

			Origin.JA_E_DEP = ZDateTime.Now;
			Assert("ETA/ETD are valid, not expecting errors.", !Origin.JA_E_DEPInfo.HasErrors());
		}

		#endregion

		#region TestValidateJB_E_ARVUnsafeChange

		public override void TestValidateJB_E_ARVUnsafeChange()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobVoyage journey = factory.New<JobVoyage>();
			journey.JV_AirSeaRoad = Constants.TransportModes.Air;

			VoyageOrigin o1 = factory.New<VoyageOrigin>();
			o1.JA_RL_NKPortOfLoading = "AUSYD";
			o1.JA_E_DEP = now;
			journey.Origins.Add(o1);

			VoyageDestination d1 = factory.New<VoyageDestination>();
			d1.JB_RL_NKPortOfDischarge = "HKHKG";
			d1.JB_E_ARV = now.AddHours(2);
			journey.Destinations.Add(d1);

			VoyageDestination d2 = factory.New<VoyageDestination>();
			d2.JB_RL_NKPortOfDischarge = "JPTYO";
			d2.JB_E_ARV = now.AddHours(30);
			journey.Destinations.Add(d2);

			factory.Save();
			AssertEquals("Precondition: Expecting Journey to have 2 sailings", 2, journey.Sailings.Count);

			JobSailing sailing = journey.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "HKHKG");

			AssertNotNull(sailing);

			CommonShipment shipment = factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_JX = sailing.PK;

			o1.JA_E_DEP = now.AddHours(27);

			Assert("Expecting error:", o1.JA_E_DEPInfo.HasErrors());

			AssertEquals("Expecting error:", "ETD is invalid for flight leg Port Pairs on this schedule: AUSYD -> HKHKG.", o1.JA_E_DEPInfo.GetErrors().GetFirstMessage());
		}

		#endregion

		#region TestValidateJA_S_ARV

		public void TestValidateScheduleArrivalDate()
		{
			AssertEquals("Precondition", true, Voyage.IsAir);

			var today = ZDateTime.Today;

			Origin.JA_E_ARV = today;
			Origin.JA_S_ARV = ZDateTime.Empty;
			AssertNoWarnings("Precondition: STA should not have warnings.", Origin.JA_S_ARVInfo);

			Origin.JA_S_ARV = today;
			AssertNoWarnings("STA should not have warnings.", Origin.JA_S_ARVInfo);

			Origin.JA_E_ARV = today.AddDays(-1);
			AssertNoWarnings("STA should not have warnings.", Origin.JA_S_ARVInfo);

			Origin.JA_E_ARV = today.AddDays(1);
			AssertNoWarnings("STA should not have warnings.", Origin.JA_S_ARVInfo);

			Origin.JA_E_ARV = today.AddDays(-2);
			AssertHasWarning("STA should have warnings.", Origin.JA_S_ARVInfo, "STA is more than a day after ETA");

			Origin.JA_E_ARV = today.AddDays(2);
			AssertHasWarning("STA should have warnings.", Origin.JA_S_ARVInfo, "ETA is more than a day after STA");

			Origin.JA_S_ARV = today.AddDays(2);
			AssertNoWarnings("STA should not have warnings.", Origin.JA_S_ARVInfo);

			Origin.JA_S_ARV = today.AddDays(4);
			AssertHasWarning("STA should have warnings", Origin.JA_S_ARVInfo, "STA is more than a day after ETA");
		}

		#endregion

		#region TestValidateJA_S_DEP

		public void TestValidateScheduleDepartureDate()
		{
			AssertEquals("Precondition", true, Voyage.IsAir);

			var today = ZDateTime.Today;

			Origin.JA_E_DEP = today;
			Origin.JA_S_DEP = ZDateTime.Empty;
			AssertNoErrors("Precondition: ATA should not have errors.", Origin.JA_S_DEPInfo);

			Origin.JA_S_DEP = today;
			AssertNoWarnings("STA should not have errors.", Origin.JA_S_DEPInfo);

			Origin.JA_E_DEP = today.AddDays(-1);
			AssertNoWarnings("STA should not have errors.", Origin.JA_S_DEPInfo);

			Origin.JA_E_DEP = today.AddDays(1);
			AssertNoWarnings("STA should not have errors.", Origin.JA_S_DEPInfo);

			Origin.JA_E_DEP = today.AddDays(-2);
			AssertHasWarning("STA should have errors.", Origin.JA_S_DEPInfo, "STD is more than a day after ETD");

			Origin.JA_E_DEP = today.AddDays(2);
			AssertHasWarning("STA should have errors.", Origin.JA_S_DEPInfo, "ETD is more than a day after STD");

			Origin.JA_S_DEP = today.AddDays(2);
			AssertNoWarnings("STA should not have errors.", Origin.JA_S_DEPInfo);

			Origin.JA_S_DEP = today.AddDays(4);
			AssertHasWarning("STA should have errors.", Origin.JA_S_DEPInfo, "STD is more than a day after ETD");
		}

		#endregion

		#region Implementation

		protected override ZString TransportTypeCode
		{
			get { return Constants.TransportModes.Air; }
		}

		protected override Type ValidationType
		{
			get { return typeof(VoyageOriginAirValidation); }
		}

		#endregion
	}
}
