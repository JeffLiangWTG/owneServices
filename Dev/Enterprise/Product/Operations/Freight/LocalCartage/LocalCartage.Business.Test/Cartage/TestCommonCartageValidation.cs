using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class TestCommonCartageValidation : BusinessObjectValidationTestCase
	{
		public void TestJJ_F3_NKPackType()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_F3_NKPackType = "";
			AssertEquals(true, cartage.JJ_F3_NKPackTypeInfo.HasErrors());
			cartage.JJ_F3_NKPackType = "PLT";
			AssertEquals(false, cartage.JJ_F3_NKPackTypeInfo.HasErrors());
			cartage.JJ_F3_NKPackType = "XXX";
			AssertEquals(false, cartage.JJ_F3_NKPackTypeInfo.HasErrors());
		}

		public void TestJJ_Weight_ContainerWeight()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualWeight = 20000;
			line1.JL_ActualWeightUQ = "KG";
			container1.PackLines.Add(line1);
			cartage.JJ_WeightUQ = "KG";
			cartage.JJ_Weight = 20000;
			AssertEquals(false, cartage.JJ_WeightInfo.HasWarnings());
			cartage.JJ_WeightUQ = "T";
			cartage.JJ_Weight = 20;
			AssertEquals(false, cartage.JJ_WeightInfo.HasWarnings());
			cartage.JJ_WeightUQ = "+1"; // invalid
			cartage.JJ_Weight = 1;
			AssertEquals(false, cartage.JJ_WeightInfo.HasWarnings());
			cartage.JJ_WeightUQ = "KG";
			cartage.JJ_Weight = 20;
			AssertEquals(true, cartage.JJ_WeightInfo.HasWarnings());
		}

		public void TestJJ_Volume_ContainerVolume()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualVolume = 20000;
			line1.JL_ActualVolumeUQ = "M3";
			container1.PackLines.Add(line1);
			AssertEquals("Precondition: expecting TotalContainerVolume to be populated", new ZDecimal(20000), cartage.TotalContainerVolume);
			AssertEquals("Precondition: expecting TotalContainerVolumeUnit to be populated", "M3", cartage.TotalContainerVolumeUnit);
			cartage.JJ_VolumeUQ = "M3";
			cartage.JJ_Volume = 20000;
			AssertEquals(false, cartage.JJ_VolumeInfo.HasWarnings());
			cartage.JJ_VolumeUQ = "ML";
			cartage.JJ_Volume = 20;
			AssertEquals(false, cartage.JJ_VolumeInfo.HasWarnings());
			cartage.JJ_VolumeUQ = "+1"; // invalid
			cartage.JJ_Volume = 1;
			AssertEquals(false, cartage.JJ_VolumeInfo.HasWarnings());
			cartage.JJ_VolumeUQ = "M3";
			cartage.JJ_Volume = 20;
			AssertEquals(true, cartage.JJ_VolumeInfo.HasWarnings());
		}

		public void TestJJ_EstimatedPickup()
		{
			ZDateTime now = ZDateTime.Now;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_EstimatedDelivery = now;
			cartage.JJ_EstimatedPickup = now.AddDays(-1);
			Assert("Before: Should be no errors", !cartage.JJ_EstimatedPickupInfo.HasErrors());
			cartage.JJ_EstimatedPickup = now;
			Assert("Same: Should be no errors", !cartage.JJ_EstimatedPickupInfo.HasErrors());
			cartage.JJ_EstimatedPickup = now.AddDays(1);
			Assert("After: Should be error", cartage.JJ_EstimatedPickupInfo.HasErrors());
		}

		public void TestJJ_EstimatedDelivery()
		{
			ZDateTime now = ZDateTime.Now;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_EstimatedPickup = now;
			cartage.JJ_EstimatedDelivery = now.AddDays(1);
			Assert("After: Should be no errors", !cartage.JJ_EstimatedDeliveryInfo.HasErrors());
			cartage.JJ_EstimatedDelivery = now;
			Assert("Same: Should be no errors", !cartage.JJ_EstimatedDeliveryInfo.HasErrors());
			cartage.JJ_EstimatedDelivery = now.AddDays(-1);
			Assert("Before: Should be error", cartage.JJ_EstimatedDeliveryInfo.HasErrors());
		}

		public void TestCheckJJ_E3_NKJobType()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			AssertNoErrors("Job Type is no longer mandatory", cartage.JJ_E3_NKJobTypeInfo);
			cartage.JJ_E3_NKJobType = "XXXX";
			AssertHasErrors("Job Type is invalid.", cartage.JJ_E3_NKJobTypeInfo);
			cartage.JJ_E3_NKJobType = new CartageBindToLists(Factory).NewCartageJobTypes[0].Code;
			AssertNoErrors("Job Type is invalid.", cartage.JJ_E3_NKJobTypeInfo);
		}

		public void TestCheckJJ_GB()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertNoErrors("Precondition - Branch is defaulted when cartage is created.", cartage.JJ_GBInfo);
			cartage.JJ_GB = ZGuid.Empty;
			AssertHasErrors("Branch is mandatory field.", cartage.JJ_GBInfo);
			cartage.JJ_GB = Env.CurrentBranch.PK;
			AssertNoErrors("Branch is set, no errors expected.", cartage.JJ_GBInfo);
		}

		public void TestJJ_A_JCL()
		{
			ZDateTime now = ZDateTime.Now;
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			Assert("Precondition.", !cartage.JJ_A_JCLInfo.HasErrors());
			cartage.JJ_A_JCL = now.AddDays(7);
			cartage.Validation.ValidateJJ_A_JCL();
			AssertHasError(cartage.JJ_A_JCLInfo, "Job completion date cannot be in the future.");
			cartage.JJ_A_JCL = now;
			cartage.Validation.ValidateJJ_A_JCL();
			AssertNoError(cartage.JJ_A_JCLInfo, "Job completion date cannot be in the future.");
			Assert("Completed Date can be set regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
			leg1.JU_AdditionalService = Constants.CartageAdditional.Futile;
			cartage.Validation.ValidateJJ_A_JCL();
			Assert("Completed Date can be set regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
			leg1.JU_AdditionalService = "";
			leg1.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Futile;
			cartage.Validation.ValidateJJ_A_JCL();
			Assert("Completed Date can be set regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
			leg1.JU_MessageStatus = "";
			cartage.Validation.ValidateJJ_A_JCL();
			Assert("Completed Date can be set regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
			leg1.JU_PickupTimeIn = now;
			leg1.JU_PickupTimeOut = now;
			leg1.JU_DeliverTimeIn = now;
			leg1.JU_DeliverTimeOut = now;
			cartage.Validation.ValidateJJ_A_JCL();
			Assert("Completed Date can be set regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
			cartage.JJ_A_JCL = ZDateTime.Empty;
			cartage.Validation.ValidateJJ_A_JCL();
			Assert("Completed Date can be empty regardless of the status of the legs.", !cartage.JJ_A_JCLInfo.HasErrors());
		}

		public void TestCheckJJ_ContainerMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = "AAA";
			AssertHasErrors("Should reject invalid container mode", cartage.JJ_ContainerModeInfo);
			cartage.JJ_ContainerMode = "CNT";
			AssertNoErrors("Should allow valid container mode", cartage.JJ_ContainerModeInfo);
			cartage.JJ_ContainerMode = "";
			AssertNoErrors("Should allow empty container mode", cartage.JJ_ContainerModeInfo);
		}

		public void TestDateValidationForAir()
		{
			var now = ZDateTime.Now;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			cartage.Vessel = Helper.TestVessel1.RV_Name;
			cartage.VoyageFlight = "7890";
			cartage.PortOfLoading = "AUSYD";
			cartage.PortOfDischarge = "USLAX";
			cartage.E_DEP = ZDateTime.Empty;
			cartage.E_ARV = ZDateTime.Empty;
			AssertNoErrors("No errors on ETD when empty.", cartage.E_DEPInfo);
			AssertNoErrors("No errors on ETA when empty.", cartage.E_ARVInfo);
			cartage.E_DEP = now;
			AssertHasErrors("Error on ETD when ETA is empty.", cartage.E_DEPInfo);
			cartage.E_ARV = now;
			AssertNoErrors("ETA = ETD should be no errors.", cartage.E_ARVInfo);
			AssertNoErrors("ETA = ETD should be no errors.", cartage.E_DEPInfo);
			cartage.E_ARV = now.AddDays(-1);
			AssertHasError("ETA 1 day before ETD. Should be error.", cartage.E_ARVInfo, "ETA cannot be more than a day before ETD.");
			ZDateTime currentDate = ZDateTime.Today;
			cartage.E_DEP = currentDate;
			cartage.E_ARV = currentDate;
			AssertNoErrors("ETA = ETD, dates only. Should be no error.", cartage.E_ARVInfo);
			cartage.E_DEP = currentDate.AddDays(1);
			AssertHasError("ETD >= ETA + 1 day. Should be error.", cartage.E_DEPInfo, "ETD cannot be more than a day after ETA.");
			cartage.E_DEP = now.AddDays(1).AddSeconds(-1);
			AssertNoErrors("ETA can be less than a day before ETD. Should be no error.", cartage.E_ARVInfo);
		}

		public void TestDateValidationForSea()
		{
			var now = ZDateTime.Now;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			cartage.Vessel = Helper.TestVessel1.RV_Name;
			cartage.VoyageFlight = "7890";
			cartage.PortOfLoading = "AUSYD";
			cartage.PortOfDischarge = "USLAX";
			cartage.E_DEP = ZDateTime.Empty;
			cartage.E_ARV = ZDateTime.Empty;
			AssertNoErrors("No errors on ETD when empty.", cartage.E_DEPInfo);
			AssertNoErrors("No errors on ETA when empty.", cartage.E_ARVInfo);
			cartage.E_DEP = now;
			AssertNoErrors("No errors on ETD when ETA is empty.", cartage.E_DEPInfo);
			cartage.E_ARV = now;
			AssertNoErrors("ETA = ETD should be no errors.", cartage.E_ARVInfo);
			cartage.E_ARV = now.AddDays(-1);
			AssertHasError("ETA 1 day before ETD. Should be error.", cartage.E_ARVInfo, "ETA cannot be before ETD.");
			ZDateTime currentDate = ZDateTime.Today;
			cartage.E_DEP = currentDate;
			cartage.E_ARV = currentDate;
			AssertNoErrors("ETA = ETD, dates only. Should be no error.", cartage.E_ARVInfo);
			cartage.E_DEP = currentDate.AddSeconds(1);
			AssertHasError("ETD = ETA + 1 second. Should be error.", cartage.E_DEPInfo, "ETD cannot be after ETA.");
		}

		public void TestValidSchedule()
		{
			var now = ZDateTime.Now;
			var cartage = Factory.New<CommonCartage>();
			cartage.Vessel = Helper.TestVessel1.RV_Name;
			cartage.VoyageFlight = "7890";
			cartage.Validation.ValidateAll();
			AssertEquals(true, cartage.VesselInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			AssertEquals(true, cartage.VoyageFlightInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			AssertEquals(false, cartage.PortOfLoadingInfo.HasErrors());
			AssertEquals(false, cartage.PortOfDischargeInfo.HasErrors());
			AssertEquals(false, cartage.E_DEPInfo.HasErrors());
			AssertEquals(false, cartage.E_ARVInfo.HasErrors());
			cartage.E_DEP = now;
			cartage.E_ARV = now.AddDays(3);
			cartage.Validation.ValidateAll();
			AssertEquals(true, cartage.VesselInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			AssertEquals(true, cartage.VoyageFlightInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			AssertEquals(false, cartage.PortOfLoadingInfo.HasErrors());
			AssertEquals(false, cartage.PortOfDischargeInfo.HasErrors());
			AssertEquals(true, cartage.E_DEPInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			AssertEquals(true, cartage.E_ARVInfo.HasError("This field will not be saved without having Vessel, Voyage, Port of Loading and Port of Discharge."));
			cartage.PortOfLoading = "AUSYD";
			cartage.PortOfDischarge = "AUSYD";
			cartage.Validation.ValidateAll();
			AssertEquals(true, cartage.VesselInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.VoyageFlightInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.PortOfLoadingInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.PortOfDischargeInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.E_DEPInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.E_ARVInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			cartage.E_DEP = ZDateTime.Empty;
			cartage.E_ARV = ZDateTime.Empty;
			cartage.Validation.ValidateAll();
			AssertEquals(true, cartage.VesselInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.VoyageFlightInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.PortOfLoadingInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(true, cartage.PortOfDischargeInfo.HasError("This field will not be saved because some of the schedule details are not valid."));
			AssertEquals(false, cartage.E_DEPInfo.HasErrors());
			AssertEquals(false, cartage.E_ARVInfo.HasErrors());
			cartage.PortOfLoading = "AUSYD";
			cartage.PortOfDischarge = "USLAX";
			cartage.Validation.ValidateAll();
			AssertEquals(false, cartage.VesselInfo.HasErrors());
			AssertEquals(false, cartage.VoyageFlightInfo.HasErrors());
			AssertEquals(false, cartage.PortOfLoadingInfo.HasErrors());
			AssertEquals(false, cartage.PortOfDischargeInfo.HasErrors());
			AssertEquals(false, cartage.E_DEPInfo.HasErrors());
			AssertEquals(false, cartage.E_ARVInfo.HasErrors());
		}

		[TestDate(2014, 10, 14)]
		public void TestCreateCartage_WithNewAndEditPermissions()
		{
			AssertCartageValidationForSailingPermissions(true, true);
		}

		[TestDate(2014, 10, 14)]
		public void TestCreateCartage_WithNewAndNotEditPermissions()
		{
			AssertCartageValidationForSailingPermissions(true, false);
		}

		[TestDate(2014, 10, 14)]
		public void TestCreateCartage_WithNotNewAndEditPermissions()
		{
			AssertCartageValidationForSailingPermissions(false, true);
		}

		[TestDate(2014, 10, 14)]
		public void TestCreateCartage_WithNotNewAndNotEditPermissions()
		{
			AssertCartageValidationForSailingPermissions(false, false);
		}

		void AssertCartageValidationForSailingPermissions(bool userHasNewPermission, bool userHasEditPermission)
		{
			var now = ZDateTime.Now;
			var originDepartureDate = now.AddDays(-3);
			var destinationArrivalDate = now.AddDays(4);
			var destinationFCLAvailabilityDateTime = now.AddDays(5).AddHours(5).AddMinutes(06);
			var destinationFCLStorageDateTime = now.AddDays(6).AddHours(6).AddMinutes(10);
			var destinationLCLAvailabilityDateTime = now.AddDays(7).AddHours(10).AddMinutes(05);
			var destinationLCLStorageDateTime = now.AddDays(8).AddHours(11).AddMinutes(15);
			var originFCLReceivalCommences = now.AddDays(9).AddHours(11).AddMinutes(15);
			var destinationLCLReceivalCommences = now.AddDays(10).AddHours(11).AddMinutes(15);
			var originFCLCutOff = now.AddDays(11).AddHours(11).AddMinutes(15);
			var destinationLCLCutOff = now.AddDays(12).AddHours(11).AddMinutes(15);
			// setup test user and deny sailing schedule edit permissions
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";
			var newSailingSchedule = Env.Security.SailingScheduleNew;
			var newSecurityRecord = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			newSecurityRecord.GU_SecurityRight = newSailingSchedule.Code;
			newSecurityRecord.GU_ItemGUID = newSailingSchedule.ItemGuid;
			newSecurityRecord.GU_SecurityItemIsAllowed = userHasNewPermission;
			newSecurityRecord.GU_GS = staff.PK;
			var editSailingSchedule = Env.Security.SailingScheduleEdit;
			var editsecurityRecord = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			editsecurityRecord.GU_SecurityRight = editSailingSchedule.Code;
			editsecurityRecord.GU_ItemGUID = editSailingSchedule.ItemGuid;
			editsecurityRecord.GU_SecurityItemIsAllowed = userHasEditPermission;
			editsecurityRecord.GU_GS = staff.PK;
			Factory.Save();
			// setup sailing schedule
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_FlightDate = now;
			voyage.JV_VoyageFlight = "QF281";
			voyage.JV_RV_NKVessel = Helper.TestVessel1.RV_FK;
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = originDepartureDate;
			origin.JA_ReceivalCommences = originFCLReceivalCommences;
			origin.JA_CutOff = originFCLCutOff;
			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "GBLON";
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = destinationArrivalDate;
			destination.JB_AvailabilityDate = destinationFCLAvailabilityDateTime;
			destination.JB_StorageDate = destinationFCLStorageDateTime;
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotAvailabilityDate = destinationLCLAvailabilityDateTime;
			sailing.JX_DepotStorageDate = destinationLCLStorageDateTime;
			sailing.JX_DepotReceivalCommences = destinationLCLReceivalCommences;
			sailing.JX_DepotCutOff = destinationLCLCutOff;
			Factory.Save();
			// test user without security rights
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var standaloneCartage = Helper.CreateCartage("ISM1", 1);
				standaloneCartage.Vessel = Helper.TestVessel1.RV_Name;
				standaloneCartage.VoyageFlight = "QF281";
				standaloneCartage.PortOfLoading = "AUSYD";
				standaloneCartage.PortOfDischarge = "GBLON";
				AssertEquals("Precondition - Populated automatically.", originDepartureDate, standaloneCartage.E_DEP);
				AssertEquals("Precondition - Populated automatically.", destinationArrivalDate, standaloneCartage.E_ARV);
				AssertEquals("Precondition - Populated automatically.", destinationFCLAvailabilityDateTime, standaloneCartage.FCLAvailabilityDate);
				AssertEquals("Precondition - Populated automatically.", destinationFCLStorageDateTime, standaloneCartage.FCLStorageDate);
				AssertEquals("Precondition - Populated automatically.", destinationLCLAvailabilityDateTime, standaloneCartage.LCLAvailabilityDate);
				AssertEquals("Precondition - Populated automatically.", destinationLCLStorageDateTime, standaloneCartage.LCLStorageDate);
				AssertEquals("Precondition - Populated automatically.", originFCLReceivalCommences, standaloneCartage.FCLReceivalCommences);
				AssertEquals("Precondition - Populated automatically.", originFCLCutOff, standaloneCartage.FCLCutOff);
				AssertEquals("Precondition - Populated automatically.", destinationLCLReceivalCommences, standaloneCartage.LCLReceivalCommences);
				AssertEquals("Precondition - Populated automatically.", destinationLCLCutOff, standaloneCartage.LCLCutOff);
				AssertEditingSailingSchedule(standaloneCartage, originDepartureDate, (cartage, date) => cartage.E_DEP = date, cartage => cartage.E_DEPInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationArrivalDate, (cartage, date) => cartage.E_ARV = date, cartage => cartage.E_ARVInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationFCLAvailabilityDateTime, (cartage, date) => cartage.FCLAvailabilityDate = date, cartage => cartage.FCLAvailabilityDateInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationFCLStorageDateTime, (cartage, date) => cartage.FCLStorageDate = date, cartage => cartage.FCLStorageDateInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationLCLAvailabilityDateTime, (cartage, date) => cartage.LCLAvailabilityDate = date, cartage => cartage.LCLAvailabilityDateInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationLCLStorageDateTime, (cartage, date) => cartage.LCLStorageDate = date, cartage => cartage.LCLStorageDateInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, originFCLReceivalCommences, (cartage, date) => cartage.FCLReceivalCommences = date, cartage => cartage.FCLReceivalCommencesInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, originFCLCutOff, (cartage, date) => cartage.FCLCutOff = date, cartage => cartage.FCLCutOffInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationLCLReceivalCommences, (cartage, date) => cartage.LCLReceivalCommences = date, cartage => cartage.LCLReceivalCommencesInfo, userHasEditPermission);
				AssertEditingSailingSchedule(standaloneCartage, destinationLCLCutOff, (cartage, date) => cartage.LCLCutOff = date, cartage => cartage.LCLCutOffInfo, userHasEditPermission);
			}
		}

		static void AssertEditingSailingSchedule(CommonCartage standaloneCartage, ZDateTime date, Action<CommonCartage, ZDateTime> setDate, Func<CommonCartage, ZPropertyInfo> getPropertyInfo, bool doesHavePermissionToEdit = false)
		{
			setDate(standaloneCartage, date.AddDays(5));
			if (!doesHavePermissionToEdit)
			{
				AssertHasError(getPropertyInfo(standaloneCartage), "The change you are making would affect an existing Sailing Schedule. You do not have rights to edit Sailing Schedules. If you require such rights please ask your supervisor to grant the right at Operate > Schedules > Sailing Schedule > Edit.");
			}
			else
			{
				AssertNoErrors(getPropertyInfo(standaloneCartage));
			}

			setDate(standaloneCartage, date);
			AssertNoErrors(getPropertyInfo(standaloneCartage));
		}

		public void TestDontValidateVesselIfParentAttached()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "ves";
			var booking = Factory.New<CommonShipment>();
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsBooking = true;
			booking.JS_JX = Helper.CreateSailing(vessel, "voy", "AUSYD", "AUPER", ZDateTime.Now, Constants.TransportModes.Rail).PK;
			var cartage = Helper.CreateInternalCartage(booking);
			cartage.Validation.ValidateAll();
			AssertEquals(false, cartage.VesselInfo.HasErrors());
			AssertEquals(false, cartage.VoyageFlightInfo.HasErrors());
		}

		public void TestDontValidateVesselForRail()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			cartage.Vessel = "testRail";
			cartage.Validation.ValidateVessel();
			AssertEquals(false, cartage.VesselInfo.HasErrors());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
