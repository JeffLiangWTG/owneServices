using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginSeaValidationTest : BaseJobVoyOriginValidationTest
	{
		#region TestJA_RL_NKPortOfLoading_PortCodeErrors

		public void TestJA_RL_NKPortOfLoading_PortCodeErrors()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLane.EJ_Location1 = "AUSYD";
			tradeLane.EJ_Location2 = "HKHKG";
			tradeLane.EJ_OH_RelatedOrg = principal.PK;
			tradeLane.EJ_Direction = DirectionTypeList.Codes.OneWay;
			JobTradeLaneVoyage tradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			tradeLaneVoyage.NB_JV = Voyage.PK;
			tradeLaneVoyage.NB_OH = principal.PK;
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Origin.JA_RL_NKPortOfLoading = "HKHKG";

			AssertHasError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			tradeLane.EJ_Direction = DirectionTypeList.Codes.BothWays;
			Origin.Validation.ValidateJA_RL_NKPortOfLoading();
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			tradeLane.EJ_Direction = DirectionTypeList.Codes.OneWay;
			Origin.Validation.ValidateJA_RL_NKPortOfLoading();
			AssertHasError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			tradeLaneVoyage.NB_EJ = ZGuid.Empty;
			Origin.Validation.ValidateJA_RL_NKPortOfLoading();
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Origin.JA_RL_NKPortOfLoading = "HKHKG";
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			tradeLane.EJ_Location1 = "AU";
			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");

			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.Countries.Add(new RefCountry.Loader(Factory).LoadForCountry("AU"));
			zone.FZ_Code = "Zone";
			tradeLane.EJ_Location1 = zone.FZ_Code;
			Origin.Validation.ValidateJA_RL_NKPortOfLoading();
			AssertNoError(Origin.JA_RL_NKPortOfLoadingInfo, "This port does not match any of the specified trade lanes.");
		}

		#endregion

		#region JA_RL_NKPortOfLoading_PortCodeWarnings

		public void TestValidateJA_RL_NKPortOfLoading_PortCodeWarnings()
		{
			SailingsForTestClasses helper = new SailingsForTestClasses(new BusinessObjectFactory());
			VoyageOrigin melSeaOrigin = helper.MelVoyOrigin;

			melSeaOrigin.PortOfLoading.RL_HasSeaport = true;
			melSeaOrigin.Validation.ValidateJA_RL_NKPortOfLoading();
			Assert("Mel has seaport, not expecting warnings.", !melSeaOrigin.HasWarnings);

			melSeaOrigin.PortOfLoading.RL_HasSeaport = false;
			melSeaOrigin.Validation.ValidateJA_RL_NKPortOfLoading();
			Assert("Mel does not have seaport, expecting warnings.", melSeaOrigin.HasWarnings);
		}

		public void TestCheckJA_DGCutOffWithDate()
		{
			ZDateTime today = ZDateTime.Today;

			Sailing.JX_IsPublished = true;
			Origin.Validation.ValidateJA_DGCutOff();
			AssertNoErrors("Should be NO errors", Origin.JA_DGCutOffInfo);

			Origin.JA_E_DEP = today.AddDays(3);
			Origin.JA_DGCutOff = today.AddDays(3).AddHours(9);
			AssertNoErrors("Should be No Errors", Origin.JA_DGCutOffInfo);
		}

		public void TestCheckJA_DGReceivalCommencesWithDate()
		{
			ZDateTime today = ZDateTime.Today;

			Sailing.JX_IsPublished = true;
			Origin.Validation.ValidateJA_DGReceivalCommences();
			AssertNoErrors("Should be NO errors", Origin.JA_DGReceivalCommencesInfo);

			Origin.JA_E_DEP = today.AddDays(3);
			Origin.JA_DGReceivalCommences = today.AddDays(3).AddHours(8);
			AssertNoErrors("Should be No Errors", Origin.JA_DGReceivalCommencesInfo);
		}

		public void TestCheckJA_CutOffWithDate()
		{
			ZDateTime today = ZDateTime.Today;
			Sailing.JX_IsPublished = true;

			Origin.Validation.ValidateJA_CutOff();
			Assert("FCL is empty, no warning expected", !Origin.JA_CutOffInfo.HasWarnings());

			Origin.JA_E_DEP = today.AddDays(1);
			Origin.JA_CutOff = today.AddDays(1).AddHours(4);
			AssertNoErrors("Should Be No Errors", Origin.JA_CutOffInfo);
		}

		public void TestCheckJA_ReceivalCommencesWithDate()
		{
			ZDateTime today = ZDateTime.Today;
			Sailing.JX_IsPublished = true;

			Origin.Validation.ValidateJA_ReceivalCommences();
			Assert("FCL is empty, no warning expected", !Origin.JA_ReceivalCommencesInfo.HasWarnings());

			Origin.JA_E_DEP = today.AddDays(1);
			Origin.JA_ReceivalCommences = today.AddDays(1).AddHours(4);
			AssertNoErrors("Should Be No Errors", Origin.JA_ReceivalCommencesInfo);
		}
		#endregion

		#region TestValidateJA_E_DEP

		public void TestValidateJA_E_DEP_Sea()
		{
			//Departure Voyage - DEP date is mandatory

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Origin.JA_E_DEP = new ZDateTime(2013, 11, 20);
			Origin2.JA_E_DEP = new ZDateTime(2013, 11, 25);

			AssertEquals("Expecting Voyage to have two origins", 2, Voyage.Origins.Count);
			AssertEquals("Expecting Voyage to have one destination", 1, Voyage.Destinations.Count);

			Origin.JA_E_DEP = ZDateTime.Empty;
			Assert("ETD cannot be empty, errors expected.", Origin.JA_E_DEPInfo.HasErrors());

			Origin.JA_E_DEP = new ZDateTime(2013, 11, 20);
			Destination.JB_E_ARV = new ZDateTime(2013, 11, 23);
			Voyage.GenerateSailings();
			AssertEquals("Destination arrival is before departure, not expecting sailing to be generated.", 1, Voyage.Sailings.Count);

			Destination.JB_E_ARV = new ZDateTime(2013, 11, 25);
			Voyage.GenerateSailings();
			AssertEquals("Destination arrival is same as departure, expecting sailing to be generated.", 2, Voyage.Sailings.Count);

			Destination.JB_E_ARV = new ZDateTime(2013, 11, 29);
			Voyage.GenerateSailings();
			AssertEquals("Destination arrival is after departure, expecting sailing to be generated.", 2, Voyage.Sailings.Count);

			//Arrival Voyage -  Dep date isn't mandatory

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USSFO";
			Origin2.JA_RL_NKPortOfLoading = "USLAX";

			Origin.JA_E_DEP = ZDateTime.Empty;
			Assert("ETD is not mandatory, warning expected", Origin.JA_E_DEPInfo.HasNotifications());
			Assert("ETD is not mandatory, no errors expected", !Origin.JA_E_DEPInfo.HasErrors());

			Origin2.JA_E_DEP = ZDateTime.Empty;
			Origin.JA_E_DEP = new ZDateTime(2013, 11, 20);
			Destination.JB_E_ARV = new ZDateTime(2013, 11, 10);
			Voyage.GenerateSailings();
			AssertEquals("ETA is before ETD, not expecting sailing to be generated.", 1, Voyage.Sailings.Count);

			Destination.JB_E_ARV = new ZDateTime(2013, 12, 10);
			Voyage.GenerateSailings();
			AssertEquals("ETA is valid, not expecting sailing to be generated.", 2, Voyage.Sailings.Count);

			Origin2.JA_E_DEP = ZDateTime.Empty;
			Origin.JA_E_DEP = ZDateTime.Empty;

			Destination.JB_E_ARV = new ZDateTime(2013, 10, 25);
			Origin.Validation.ValidateJA_E_DEP();
			Assert("ETA is valid, no errors expected.", !Origin.JA_E_DEPInfo.HasErrors());
		}

		#endregion

		#region Implementation

		protected override ZString TransportTypeCode
		{
			get { return Constants.TransportModes.Sea; }
		}

		protected override Type ValidationType
		{
			get { return typeof(VoyageOriginSeaValidation); }
		}

		#endregion

	}
}
