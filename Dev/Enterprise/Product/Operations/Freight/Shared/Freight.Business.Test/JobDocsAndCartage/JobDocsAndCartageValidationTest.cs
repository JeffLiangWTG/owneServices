using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public class JobDocsAndCartageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJP_OA_DeliveryCartageCoAddrTracksInactiveOrgs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingProvider = true;
			org.OH_IsLocalTransport = true;
			Factory.Save();

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org.MainAddress.PK;

			shipment.DocsAndCartage.Validation.ValidateJP_OA_DeliveryCartageCoAddr();
			AssertNoWarnings(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);

			org.OH_IsActive = false;
			Factory.Save();

			shipment.DocsAndCartage.Validation.ValidateJP_OA_DeliveryCartageCoAddr();
			AssertHasWarnings(shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
		}

		public void TestJP_OA_PickupCartageCoAddrTracksInactiveOrgs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingProvider = true;
			org.OH_IsLocalTransport = true;
			Factory.Save();

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org.MainAddress.PK;

			shipment.DocsAndCartage.Validation.ValidateJP_OA_PickupCartageCoAddr();
			AssertNoWarnings(shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);

			org.OH_IsActive = false;
			Factory.Save();

			shipment.DocsAndCartage.Validation.ValidateJP_OA_PickupCartageCoAddr();
			AssertHasWarnings(shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
		}

		public void TestCheckJP_PickupCartageCompleted()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();
			cartage.JP_PickupCartageCompleted = ZDateTime.Now;
			confirm.Divots[0].J8_PackagesDelivered = 5;

			AssertHasErrors("Actual pickup date cannot be set before all pickup confirms have been completed error expected", cartage.JP_PickupCartageCompletedInfo);

			Factory.Save();
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			Assert("Has changes so don't show it as an error", !cartage.JP_PickupCartageCompletedInfo.HasErrors());
			Assert("Has changes so don't show it as an error, show it as a warning", cartage.JP_PickupCartageCompletedInfo.HasWarnings());

			shipment.PickupConfirms.DeleteAll();
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertHasErrors("Actual delivery date cannot be set before all delivery confirms have been completed error expected", cartage.JP_PickupCartageCompletedInfo);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertNoErrors("Don't add validation for assembly masters, they don't support direct confirmations", cartage.JP_PickupCartageCompletedInfo);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertNoErrors("Don't add validation for coload masters, they don't support direct confirmations", cartage.JP_PickupCartageCompletedInfo);

			shipment.JS_RL_NKOrigin = "CNSHA";
			cartage.JP_PickupCartageCompleted = shipment.Origin.LocationDateTime.AddSeconds(2);
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertHasError(cartage.JP_PickupCartageCompletedInfo, "Actual pickup date cannot be in the future.");
			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertNoErrors("When actual pickup is in the past, don't show error", cartage.JP_PickupCartageCompletedInfo);

			cartage.JP_PickupCartageCompleted = shipment.Origin.LocationDateTime.AddMinutes(-1);
			AssertNoErrors("When actual pickup is in the past, don't show error", cartage.JP_PickupCartageCompletedInfo);

			shipment.JS_RL_NKOrigin = ZString.Empty;
			AssertNoErrors("When UNLOCO is blank, don't show error", cartage.JP_PickupCartageCompletedInfo);

			shipment.JS_RL_NKOrigin = "CNSHA";
			cartage.JP_PickupCartageCompleted = shipment.Origin.LocationDateTime.AddMinutes(-1);
			AssertNoErrors("When actual pickup is in the past, don't show error", cartage.JP_PickupCartageCompletedInfo);

			var consignorOrg = CreateOrganisation("CNRORG", "GBLON");
			var pickupPort = ((ILocation)consignorOrg.MainAddress).UNLOCO;
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorOrg.MainAddress.PK;
			cartage.JP_PickupCartageCompleted = pickupPort.LocationDateTime.AddMinutes(1);
			AssertHasError(cartage.JP_PickupCartageCompletedInfo, "Actual pickup date cannot be in the future.");
		}

		public void TestCheckJP_PickupCartageCompleted_EmptyLocation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();
			cartage.JP_PickupCartageCompleted = ZDateTime.Now;
			confirm.Divots[0].J8_PackagesDelivered = 5;

			Factory.Save();
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			cartage.JP_PickupCartageCompleted = Env.Time.CurrentLocalDateTime.AddSeconds(2);
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertHasWarnings("The Actual pickup date is in the future.", cartage.JP_PickupCartageCompletedInfo);
			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertNoWarnings(cartage.JP_PickupCartageCompletedInfo);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_GB_HomeBranch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, (ZString)"SYD").PK;
			Factory.Save();
			cartage.JP_PickupCartageCompleted = GlbStaff.CurrentUser.HomeBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime().AddSeconds(2));
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertHasWarnings("The Actual pickup date is in the future.", cartage.JP_PickupCartageCompletedInfo);
			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_PickupCartageCompleted();
			AssertNoWarnings(cartage.JP_PickupCartageCompletedInfo);
		}

		public void TestCheckJP_DeliveryCartageCompleted()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			cartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			confirm.Divots[0].J8_PackagesDelivered = 5;

			AssertHasErrors("Actual delivery date cannot be set before all delivery confirms have been completed error expected", cartage.JP_DeliveryCartageCompletedInfo);

			Factory.Save();
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			Assert("Has changes so don't show it as an error", !cartage.JP_DeliveryCartageCompletedInfo.HasErrors());
			Assert("Has changes so don't show it as an error, show it as a warning", cartage.JP_DeliveryCartageCompletedInfo.HasWarnings());

			shipment.DeliveryConfirms.DeleteAll();
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertHasErrors("Actual delivery date cannot be set before all delivery confirms have been completed error expected", cartage.JP_DeliveryCartageCompletedInfo);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertNoErrors("Don't add validation for assembly masters, they don't support direct confirmations", cartage.JP_DeliveryCartageCompletedInfo);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertNoErrors("Don't add validation for coload masters, they don't support direct confirmations", cartage.JP_DeliveryCartageCompletedInfo);

			shipment.JS_RL_NKDestination = "CNSHA";
			cartage.JP_DeliveryCartageCompleted = shipment.Destination.LocationDateTime.AddSeconds(2);
			AssertHasError(cartage.JP_DeliveryCartageCompletedInfo, "Actual delivery date cannot be in the future.");

			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertNoErrors("When actual delivery is in the past, don't show error", cartage.JP_DeliveryCartageCompletedInfo);

			cartage.JP_DeliveryCartageCompleted = shipment.Destination.LocationDateTime.AddMinutes(-1);
			AssertNoErrors("When actual delivery is in the past, don't show error", cartage.JP_DeliveryCartageCompletedInfo);

			shipment.JS_RL_NKDestination = ZString.Empty;
			AssertNoErrors("When UNLOCO is blank, don't show error", cartage.JP_DeliveryCartageCompletedInfo);

			var consigneeOrg = CreateOrganisation("CNEORG", "GBLON");
			var deliveryPort = ((ILocation)consigneeOrg.MainAddress).UNLOCO;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeOrg.MainAddress.PK;
			cartage.JP_DeliveryCartageCompleted = deliveryPort.LocationDateTime.AddMinutes(1);
			AssertHasError(cartage.JP_DeliveryCartageCompletedInfo, "Actual delivery date cannot be in the future.");
		}

		public void TestCheckJP_DeliveryCartageCompleted_EmptyLocation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			cartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			confirm.Divots[0].J8_PackagesDelivered = 5;

			Factory.Save();
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			cartage.JP_DeliveryCartageCompleted = Env.Time.CurrentLocalDateTime.AddSeconds(2);
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertHasWarnings("The Actual pickup date is in the future.", cartage.JP_DeliveryCartageCompletedInfo);
			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertNoWarnings(cartage.JP_DeliveryCartageCompletedInfo);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_GB_HomeBranch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, (ZString)"SYD").PK;
			Factory.Save();
			cartage.JP_DeliveryCartageCompleted = GlbStaff.CurrentUser.HomeBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime().AddSeconds(2));
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertHasWarnings("The Actual pickup date is in the future.", cartage.JP_DeliveryCartageCompletedInfo);
			Thread.Sleep(2000);
			cartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertNoWarnings(cartage.JP_DeliveryCartageCompletedInfo);
		}

		public void TestCheckJP_ExportStatement()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			JobDocsAndCartage cartage = shipment.DocsAndCartage;

			CountryExportStatementSetting countrySetting = FreightDataRegistry.Instance.ExportStatementSettings.Value["US"];
			AssertNotNull(countrySetting);
			AssertNotEquals(0, cartage.Lookups.JP_ExportStatementList.Count);

			cartage.JP_ExportStatement = "GS";
			AssertHasError(cartage.JP_ExportStatementInfo, "Enter a valid " + cartage.JP_ExportStatementInfo.Description + ".");
			foreach (CodeDescriptionPair pair in cartage.Lookups.JP_ExportStatementList)
			{
				cartage.JP_ExportStatement = pair.Code;
				AssertNoError(cartage.JP_ExportStatementInfo, "Enter a valid " + cartage.JP_ExportStatementInfo.Description + ".");
			}
			cartage.JP_ExportStatement = "GH";
			Factory.Save();
			cartage.JP_ExportStatement = "GS";
			AssertHasError(cartage.JP_ExportStatementInfo, "Enter a valid " + cartage.JP_ExportStatementInfo.Description + ".");
			cartage.JP_ExportStatement = "GH";
			AssertNoError(cartage.JP_ExportStatementInfo, "Enter a valid " + cartage.JP_ExportStatementInfo.Description + ".");

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			cartage = (declaration as IShipmentWithDocsAndCartage).DocsAndCartage;
			cartage.JP_ExportStatement = "GS";
			AssertNoError(cartage.JP_ExportStatementInfo, "Enter a valid " + cartage.JP_ExportStatementInfo.Description + ".");
		}

		#region TestCheckJP_ExportStatement_DestinationExemption

		public void TestCheckJP_ExportStatement_DestinationExemption()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			var shipment = Factory.New<CommonShipment>();
			var cartage = shipment.DocsAndCartage;

			const string exportStatementExemptionMessage = "For exports to China, Hong Kong, Russia or Venezuela, using exemption NOEEI §30.37(a) indicates none of the commodities are other than EAR99.";

			void SetupData(string companyCountry = Constants.CountryCodes.UnitedStates, string origin = "USLAX", string destination = "CNSHA", string exportStatus = "LOW")
			{
				GlbCompany.CurrentCompany.SetCountry(companyCountry);
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;
				cartage.JP_ExportStatement = exportStatus;
				cartage.Validation.ValidateJP_ExportStatement();
			}

			SetupData(companyCountry: Constants.CountryCodes.Australia);
			AssertNoWarnings(cartage.JP_ExportStatementInfo);

			SetupData(origin: "AUSYD");
			AssertNoWarnings("Has no exemption message because origin country is not US or territory", cartage.JP_ExportStatementInfo);

			SetupData(destination: "AUSYD");
			AssertNoWarnings("Has no exemption message because destination country is not CN, HK, VE or RU", cartage.JP_ExportStatementInfo);

			SetupData(exportStatus: "TOT");
			AssertNoWarnings("Has no exemption message because export status is not LOW", cartage.JP_ExportStatementInfo);

			SetupData();
			AssertHasWarning("Has exemption message because all criteria are met (CN)", cartage.JP_ExportStatementInfo, exportStatementExemptionMessage);

			SetupData(destination: "HKKWN");
			AssertHasWarning("Has exemption message because all criteria are met (HK)", cartage.JP_ExportStatementInfo, exportStatementExemptionMessage);

			SetupData(destination: "VE9VA");
			AssertHasWarning("Has exemption message because all criteria are met (VE)", cartage.JP_ExportStatementInfo, exportStatementExemptionMessage);

			SetupData(destination: "RU7RS");
			AssertHasWarning("Has exemption message because all criteria are met (RU)", cartage.JP_ExportStatementInfo, exportStatementExemptionMessage);
		}

		#endregion

		public void TestCheckJP_EstimatedPickup()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment ship = CommonShipment.New(Factory);
			ship.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ship.JS_RL_NKDestination = (GlbBranch.CurrentBranch.GB_RL_NKHomePort == "SGSIN" ? "USLAX" : "SGSIN");
			ship.JS_E_DEP = currentTime;
			ship.DocsAndCartage.JP_EstimatedPickup = currentTime.AddDays(1);
			AssertEquals("There should be an error if the pickup date is after the ETD on an export", true, ship.DocsAndCartage.JP_EstimatedPickupInfo.HasErrors());
		}

		public void TestCheckJP_EstimatedPickupWhenExportBySeaAndSameAsETD()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_E_DEP = new ZDateTime(currentTime.Year, currentTime.Month, currentTime.Day);
			shipment.DocsAndCartage.JP_EstimatedPickup = shipment.JS_E_DEP.AddHours(1);

			AssertEquals("Prerequisite: shipment is export", true, shipment.IsExport());
			string pickupDateAfterETDMessage = "The Pickup date can not be after the ETD on an export shipment";
			AssertHasWarning("There should be a warning if the pickup date is same as the ETD on an export", shipment.DocsAndCartage.JP_EstimatedPickupInfo, pickupDateAfterETDMessage);

			AssertNoError(shipment.DocsAndCartage.JP_EstimatedPickupInfo, pickupDateAfterETDMessage);
			shipment.DocsAndCartage.JP_EstimatedPickup = shipment.JS_E_DEP.AddDays(1);
			AssertHasError("There should be an error if the pickup date is after the ETD on an export", shipment.DocsAndCartage.JP_EstimatedPickupInfo, pickupDateAfterETDMessage);
		}

		public void TestCheckJP_EstimatedDelivery()
		{
			ZDateTime currentTime = ZDateTime.Now;
			var ship = Factory.New<CommonShipment>();
			ship.JS_RL_NKOrigin = (GlbBranch.CurrentBranch.GB_RL_NKHomePort == "SGSIN" ? "USLAX" : "SGSIN");
			ship.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ship.JS_E_ARV = currentTime;
			ship.DocsAndCartage.JP_EstimatedDelivery = currentTime.AddDays(-1);
			AssertEquals("There should be an error if the Delivery date is beofre the ETA on an export", true, ship.DocsAndCartage.JP_EstimatedDeliveryInfo.HasErrors());
		}

		public void TestJP_FCLPickupEquipmentNeeded()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.ContainerModes.FCL;

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "HSL";
			AssertNoErrors("Valid code", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ZYX";
			AssertHasErrorContaining(shipment.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo, "Enter a valid Pickup Port Transport Drop Mode.");
			AssertNoMessageErrors("Has no message errors", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "";
			AssertNoErrorContaining(shipment.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo, "Enter a valid Pickup Port Transport Drop Mode.");
			AssertNoMessageErrors("Has no message errors", shipment.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);
		}

		public void TestJP_FCLPickupEquipmentNeededShouldNotBeValidedWhenInBooking()
		{
			var parent = Factory.New<CommonShipment>();
			parent.JS_TransportMode = Core.Constants.ContainerModes.FTL;

			parent.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "XYZ";
			AssertHasErrors("Should have an error when JP_FCLPickupEquipmentNeeded is invalid", parent.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);

			parent.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "HSL";
			AssertNoErrors("Should have no error when JP_FCLPickupEquipmentNeeded is valid", parent.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);

			parent.JS_IsBooking = true;
			parent.JS_IsForwardRegistered = false;
			parent.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "XYZ";
			AssertNoErrors("JP_FCLPickupEquipmentNeeded should NOT be validate if in booking", parent.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);
		}

		public void TestJP_FCLDeliveryEquipmentNeeded()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.ContainerModes.FCL;

			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "HSL";
			AssertNoErrors("Valid code", shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);

			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "ZYX";
			AssertHasErrorContaining(shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo, "Enter a valid Delivery Port Transport Drop Mode.");
			AssertNoMessageErrors("Has no message errors", shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);

			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "";
			AssertNoErrorContaining(shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo, "Enter a valid Delivery Port Transport Drop Mode.");
			AssertNoMessageErrors("Has no message errors", shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);
		}

		public void TestJP_FCLDeliveryEquipmentNeededShouldNotBeValidedWhenInBooking()
		{
			var parent = Factory.New<CommonShipment>();
			parent.JS_TransportMode = Core.Constants.ContainerModes.FTL;

			parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "XYZ";
			AssertHasErrors("Should have an error when JP_FCLDeliveryEquipmentNeeded is invalid", parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);

			parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "HSL";
			AssertNoErrors("Should have no error when JP_FCLDeliveryEquipmentNeeded is valid", parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);

			parent.JS_IsBooking = true;
			parent.JS_IsForwardRegistered = false;
			parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "XYZ";
			AssertNoErrors("JP_FCLDeliveryEquipmentNeeded should NOT be validate if in booking", parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);
		}

		public void TestCheckDeliveryCartageCoPK()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsLocalTransport = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_IsShippingProvider = false;
			creditor.OH_IsLocalTransport = false;
			creditor.OH_IsCreditor = true;
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			var parent = Factory.New<CommonShipment>();
			parent.DocsAndCartage.DeliveryCartageCoPK = carrier.PK;

			AssertNoErrors("The Carrier is a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoErrors("The Carrier is a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);

			parent.DocsAndCartage.DeliveryCartageCoPK = creditor.PK;
			AssertHasErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertHasErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			AssertNoWarnings("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoWarnings("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);

			Factory.Save();
			parent.DocsAndCartage.Validation.ValidateAll();
			AssertNoErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			AssertHasWarnings("Data is already saved - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertHasWarnings("Data is already saved - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
		}

		public void TestCheckPickupCartageCoPK()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsLocalTransport = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_IsShippingProvider = false;
			creditor.OH_IsLocalTransport = false;
			creditor.OH_IsCreditor = true;
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			var parent = Factory.New<CommonShipment>();
			parent.DocsAndCartage.PickupCartageCoPK = carrier.PK;

			AssertNoErrors("The Carrier is a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
			AssertNoErrors("The Carrier is a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);

			parent.DocsAndCartage.PickupCartageCoPK = creditor.PK;

			AssertHasErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
			AssertHasErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
			AssertNoWarnings("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
			AssertNoWarnings("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);

			Factory.Save();
			parent.DocsAndCartage.Validation.ValidateAll();
			AssertNoErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
			AssertNoErrors("The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
			AssertHasWarnings("Data is already saved - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
			AssertHasWarnings("Data is already saved - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
		}

		public void TestCheckDeliveryCartageCoPK_Web()
		{
			Globals.IsWeb = true;

			try
			{
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_IsShippingProvider = true;
				carrier.OH_IsLocalTransport = true;
				carrier.OH_FullName = "Carrier";
				carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

				var creditor = Factory.New<OrgHeader>();
				creditor.OH_IsShippingProvider = false;
				creditor.OH_IsLocalTransport = false;
				creditor.OH_IsCreditor = true;
				creditor.OH_FullName = "Creditor";
				creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				Factory.Save();

				var parent = Factory.New<CommonShipment>();
				parent.DocsAndCartage.DeliveryCartageCoPK = creditor.PK;
				CombineAssertions(() =>
				{
					AssertNoErrors("Web environment must allow saving", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
					AssertNoErrors("Web environment must allow saving", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
					AssertHasWarnings("Web environment must allow saving - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.DeliveryCartageCoPKInfo);
					AssertHasWarnings("Web environment must allow saving - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
				});
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestCheckPickupCartageCoPK_Web()
		{
			Globals.IsWeb = true;

			try
			{
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_IsShippingProvider = true;
				carrier.OH_IsLocalTransport = true;
				carrier.OH_FullName = "Carrier";
				carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

				var creditor = Factory.New<OrgHeader>();
				creditor.OH_IsShippingProvider = false;
				creditor.OH_IsLocalTransport = false;
				creditor.OH_IsCreditor = true;
				creditor.OH_FullName = "Creditor";
				creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				Factory.Save();

				var parent = Factory.New<CommonShipment>();
				parent.DocsAndCartage.PickupCartageCoPK = creditor.PK;

				CombineAssertions(() =>
				{
					AssertNoErrors("Web environment must allow saving", parent.DocsAndCartage.PickupCartageCoPKInfo);
					AssertNoErrors("Web environment must allow saving", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
					AssertHasWarnings("Web environment must allow saving - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.PickupCartageCoPKInfo);
					AssertHasWarnings("Web environment must allow saving - The Creditor is not a valid Carrier organization", parent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
				});
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestCheckJP_PickupRequiredFrom()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.JP_PickupRequiredBy = currentTime;
			shipment.DocsAndCartage.JP_PickupRequiredFrom = currentTime.AddDays(1);
			AssertHasErrors("There should be an error if the Pickup Required From is after the date for Pickup Required By field", shipment.DocsAndCartage.JP_PickupRequiredFromInfo);

			shipment.DocsAndCartage.JP_PickupRequiredFrom = currentTime.AddDays(-1);
			AssertNoErrors("There should be no error if the Pickup Required From is before the date for Pickup Required By field", shipment.DocsAndCartage.JP_PickupRequiredFromInfo);
		}

		public void TestCheckJP_PickupRequiredBy()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.JP_PickupRequiredFrom = currentTime.AddDays(1);
			shipment.DocsAndCartage.JP_PickupRequiredBy = currentTime;
			AssertHasErrors("There should be an error if the Pickup Required By is before the Pickup Required From field", shipment.DocsAndCartage.JP_PickupRequiredByInfo);

			shipment.DocsAndCartage.JP_PickupRequiredBy = currentTime.AddDays(2);
			AssertNoErrors("There should be no error if the Pickup Required By is after the Pickup Required From field", shipment.DocsAndCartage.JP_PickupRequiredByInfo);
		}

		public void TestCheckJP_DeliveryRequiredFrom()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = currentTime;
			shipment.DocsAndCartage.JP_DeliveryRequiredFrom = currentTime.AddDays(1);
			AssertHasErrors("There should be an error if the Delivery Required From is after the date for Delivery Required By field", shipment.DocsAndCartage.JP_DeliveryRequiredFromInfo);

			shipment.DocsAndCartage.JP_DeliveryRequiredFrom = currentTime.AddDays(-1);
			AssertNoErrors("There should be no error if the Delivery Required From is before the date for Delivery Required By field", shipment.DocsAndCartage.JP_DeliveryRequiredFromInfo);
		}

		public void TestCheckJP_DeliveryRequiredBy()
		{
			ZDateTime currentTime = ZDateTime.Now;
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.JP_DeliveryRequiredFrom = currentTime.AddDays(1);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = currentTime;
			AssertHasErrors("There should be an error if the Delivery Required By is before the Delivery Required From field", shipment.DocsAndCartage.JP_DeliveryRequiredByInfo);

			shipment.DocsAndCartage.JP_DeliveryRequiredBy = currentTime.AddDays(2);
			AssertNoErrors("There should be no error if the Delivery Required By is after the Delivery Required From field", shipment.DocsAndCartage.JP_DeliveryRequiredByInfo);
		}

		OrgHeader CreateOrganisation(ZString code, ZString portCode, string address1 = "Main st")
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.MainAddress.OA_Address1 = address1;
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = portCode;

			return orgHeader;
		}
	}
}
