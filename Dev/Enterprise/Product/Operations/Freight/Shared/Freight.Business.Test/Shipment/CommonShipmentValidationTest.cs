using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.EU;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonShipmentValidationTest : BaseFreightTest
	{
		public void TestValidateJS_RL_NKLoadPort_ListValidation()
		{
			var shipment = GetShipment();

			shipment.JS_RL_NKLoadPort = "XXXXX";
			AssertHasError(shipment.JS_RL_NKLoadPortInfo, "Enter a valid Planned Load.");

			shipment.JS_RL_NKLoadPort = "AU";
			AssertHasError(shipment.JS_RL_NKLoadPortInfo, "Enter a valid Planned Load.");

			shipment.JS_RL_NKLoadPort = string.Empty;
			AssertNoErrors(shipment.JS_RL_NKLoadPortInfo);

			shipment.JS_RL_NKLoadPort = "AUSYD";
			AssertNoErrors(shipment.JS_RL_NKLoadPortInfo);
		}

		public void TestValidateJS_RL_NKDischargePort_ListValidation()
		{
			var shipment = GetShipment();

			shipment.JS_RL_NKDischargePort = "XXXXX";
			AssertHasError(shipment.JS_RL_NKDischargePortInfo, "Enter a valid Planned Discharge.");

			shipment.JS_RL_NKDischargePort = "AU";
			AssertHasError(shipment.JS_RL_NKDischargePortInfo, "Enter a valid Planned Discharge.");

			shipment.JS_RL_NKDischargePort = string.Empty;
			AssertNoErrors(shipment.JS_RL_NKDischargePortInfo);

			shipment.JS_RL_NKDischargePort = "AUSYD";
			AssertNoErrors(shipment.JS_RL_NKDischargePortInfo);
		}

		public void TestValidateJS_RL_NKFreightRateDestination_ListValidation()
		{
			var shipment = GetShipment();

			shipment.JS_RL_NKFreightRateDestination = "XXXXX";
			AssertHasError(shipment.JS_RL_NKFreightRateDestinationInfo, "Enter a valid Rate Destination.");

			shipment.JS_RL_NKFreightRateDestination = "AU";
			AssertHasError(shipment.JS_RL_NKFreightRateDestinationInfo, "Enter a valid Rate Destination.");

			shipment.JS_RL_NKFreightRateDestination = string.Empty;
			AssertNoErrors(shipment.JS_RL_NKFreightRateDestinationInfo);

			shipment.JS_RL_NKFreightRateDestination = "AUSYD";
			AssertNoErrors(shipment.JS_RL_NKFreightRateDestinationInfo);
		}

		public void TestValidateJS_RL_NKFreightRateOrigin_ListValidation()
		{
			var shipment = GetShipment();

			shipment.JS_RL_NKFreightRateOrigin = "XXXXX";
			AssertHasError(shipment.JS_RL_NKFreightRateOriginInfo, "Enter a valid Rate Origin.");

			shipment.JS_RL_NKFreightRateOrigin = "AU";
			AssertHasError(shipment.JS_RL_NKFreightRateOriginInfo, "Enter a valid Rate Origin.");

			shipment.JS_RL_NKFreightRateOrigin = string.Empty;
			AssertNoErrors(shipment.JS_RL_NKFreightRateOriginInfo);

			shipment.JS_RL_NKFreightRateOrigin = "AUSYD";
			AssertNoErrors(shipment.JS_RL_NKFreightRateOriginInfo);
		}

		public void TestValidatePlaceOfReceiptAndDischarge()
		{
			var shipment = GetShipment();

			shipment.JS_RL_NKPlaceOfReceipt = "XXXXX";
			shipment.JS_RL_NKPlaceOfDischarge = "XXXXX";

			AssertHasError(shipment.JS_RL_NKPlaceOfReceiptInfo, "Enter a valid Place Of Receipt.");
			AssertHasError(shipment.JS_RL_NKPlaceOfDischargeInfo, "Enter a valid Place Of Discharge.");

			shipment.JS_RL_NKPlaceOfReceipt = "AU";
			shipment.JS_RL_NKPlaceOfDischarge = "SG";

			AssertHasError(shipment.JS_RL_NKPlaceOfReceiptInfo, "Enter a valid Place Of Receipt.");
			AssertHasError(shipment.JS_RL_NKPlaceOfDischargeInfo, "Enter a valid Place Of Discharge.");

			shipment.JS_RL_NKPlaceOfReceipt = string.Empty;
			shipment.JS_RL_NKPlaceOfDischarge = string.Empty;

			AssertNoErrors(shipment.JS_RL_NKPlaceOfReceiptInfo);
			AssertNoErrors(shipment.JS_RL_NKPlaceOfDischargeInfo);

			shipment.JS_RL_NKPlaceOfReceipt = "AUSYD";
			shipment.JS_RL_NKPlaceOfDischarge = "SGSIN";

			AssertNoErrors(shipment.JS_RL_NKPlaceOfReceiptInfo);
			AssertNoErrors(shipment.JS_RL_NKPlaceOfDischargeInfo);
		}

		public void TestValidateJS_HBLContainerPackModeOverride()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = "FCL";

			shipment.JS_HBLContainerPackModeOverride = "AAA/BBB";
			AssertEquals(false, shipment.JS_HBLContainerPackModeOverrideInfo.HasChanges);
			AssertEquals(true, !shipment.IsInDatabase);
			AssertHasError(shipment.JS_HBLContainerPackModeOverrideInfo, "Enter a valid HBL Delivery Mode.");

			shipment.JS_HBLContainerPackModeOverride = "CFS/CFS";
			shipment.RunPreSaveValidation();
			AssertNoErrors(shipment.JS_HBLContainerPackModeOverrideInfo);
			Factory.Save();

			shipment.JS_HBLContainerPackModeOverride = "XXX/XXX";
			AssertEquals(true, shipment.JS_HBLContainerPackModeOverrideInfo.HasChanges);
			AssertEquals(false, !shipment.IsInDatabase);
			AssertHasError(shipment.JS_HBLContainerPackModeOverrideInfo, "Enter a valid HBL Delivery Mode.");

			var lookup = shipment.Lookups.JS_HBLContainerPackModeOverride_List;
			AssertNotNull(lookup);

			shipment.JS_HBLContainerPackModeOverride = lookup[0].Code;
			AssertNoErrors(shipment.JS_HBLContainerPackModeOverrideInfo);
		}

		public void TestValidateJS_EFreightStatus()
		{
			var shipment = GetShipment();

			shipment.JS_EFreightStatus = "XXX";
			AssertHasError(shipment.JS_EFreightStatusInfo, "Enter a valid e-Freight Status.");

			shipment.JS_EFreightStatus = "NON";
			AssertNoErrors(shipment.JS_EFreightStatusInfo);

			shipment.JS_EFreightStatus = "";
			AssertNoErrors(shipment.JS_EFreightStatusInfo);
		}

		public void TestValidateJS_GatewayFreightSellRate()
		{
			var expectedNotification =
				@"There is a Gateway Sell amount entered for this shipment, but no Sending Agent identified as Gateway Agent on related consols.
Please setup the Sending Agent as the Gateway Agent for the transhipment port in Maintain>Reference Files>Organizations>Forwarder>Gateway Agent";

			CommonShipment shipment = GetShipment();
			shipment.JS_GatewayFreightSellRate = 10m;

			shipment.RunPreSaveValidation();
			AssertNoWarnings(shipment.JS_GatewayFreightSellRateInfo);

			shipment.Consols.AddNew();

			shipment.RunPreSaveValidation();
			AssertHasWarning(shipment.JS_GatewayFreightSellRateInfo, expectedNotification);
		}

		public void TestValidateCommunityTransitStatus_PostBrexit()
		{
			var uk = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			uk.RN_EconomicGrouping = ZString.Empty;

			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "LHR";
			ukCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;

			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "LHR";
			ukBranch.GB_RL_NKHomePort = "GBLHR";

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, RefUNLOCO.Regions.NorthernIreland, true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;
				ni.RW_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			}
			Factory.Save();

			using (DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid()))
			{
				//C Status, Job Created before Brexit
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "GBLHR", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31));
				AssertCommunityTransitStatusError_PostBrexit("GBLHR", "DEFRA", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31));
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "AUSYD", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "DEFRA", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("GBLHR", "AUSYD", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "GBLHR", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "FRBLV", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2020, 12, 31));

				//C Status, Job Created post Brexit
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "GBLHR", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("GBLHR", "DEFRA", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "AUSYD", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "DEFRA", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("GBLHR", "AUSYD", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "GBLHR", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1), "C-status is only for intra-EU movements");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "FRBLV", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1));
				AssertCommunityTransitStatusError_PostBrexit("GBBEL", "FRBLV", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1)); // Belfast in NI
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "GBBEL", ExportCommunityTransitStatusList.Codes.C, new ZDateTime(2021, 1, 1)); // Belfast in NI

				//X Status, Job Created before Brexit
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "DEFRA", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "GBLON", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "GBMNC", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "NLAMS", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31));
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "GBLON", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31));
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "NZAKL", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("NZAKL", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "AUMEL", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2020, 12, 31), "X-status is only for exports out of the customs territory");

				//X Status, Job Created after Brexit
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "DEFRA", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1));
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "GBLON", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1));
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "GBMNC", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "NLAMS", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("GBLON", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1));
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "GBLON", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("DEFRA", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1));
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "NZAKL", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("NZAKL", "AUSYD", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
				AssertCommunityTransitStatusError_PostBrexit("AUSYD", "AUMEL", ExportCommunityTransitStatusList.Codes.X, new ZDateTime(2021, 1, 1), "X-status is only for exports out of the customs territory");
			}
		}

		void AssertCommunityTransitStatusError_PostBrexit(ZString origin, ZString destination, ZString status, ZDateTime jobCreationTime, string errorMessage = null)
		{
			var shipment = GetShipment();
			shipment.JS_SystemCreateTimeUtc = jobCreationTime;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_CommunityTransitStatus = status;

			if (errorMessage == null)
			{
				AssertNoErrors(shipment.JS_CommunityTransitStatusInfo);
			}
			else
			{
				AssertHasError(shipment.JS_CommunityTransitStatusInfo, errorMessage);
			}
		}

		public void TestValidateCommunityTransitStatus()
		{
			var uk = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			uk.RN_EconomicGrouping = ZString.Empty;

			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DJC";  // Nice
			ukCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "DJC";
			ukBranch.GB_RL_NKHomePort = "GBLHR";

			Factory.Save();

			var shipment = GetShipment();

			using (DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid()))
			{
				AssertNoErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.C;
				AssertNoErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUSYD";
				AssertHasErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKDestination = "DEFRA";
				AssertHasErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "GBLHR";
				AssertHasErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKOrigin = "CHGVA";
				shipment.JS_RL_NKDestination = "DEFRA";
				AssertHasWarningContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKOrigin = "NLAMS";
				shipment.JS_RL_NKDestination = "CHGVA";
				AssertHasWarningContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.X;
				AssertNoErrorContaining(shipment.JS_CommunityTransitStatusInfo, "C-status");
				AssertNoWarningContaining(shipment.JS_CommunityTransitStatusInfo, "valid");
				shipment.JS_CommunityTransitStatus = "ZZZZ";
				AssertHasWarningContaining(shipment.JS_CommunityTransitStatusInfo, "valid");
			}

			shipment.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.X;  // X - exports - OK when sat in branch DJC (GBLHR) but should not give red error for importing branch
			AssertNoWarningContaining(shipment.JS_CommunityTransitStatusInfo, "valid"); // No validation for CT status when not sat in EU			
		}

		public void TestValidateCommunityTransitStatus_EuropeanUnionExport()
		{
			var uk = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			uk.RN_EconomicGrouping = ZString.Empty;

			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DJC";
			ukCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "DJC";
			ukBranch.GB_RL_NKHomePort = "GBLHR";

			Factory.Save();

			var shipment = GetShipment();

			using (DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid()))
			{
				AssertNoErrors(shipment.JS_CommunityTransitStatusInfo);

				shipment.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.X;
				AssertNoErrors(shipment.JS_CommunityTransitStatusInfo);

				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUSYD";
				AssertNoErrors(shipment.JS_CommunityTransitStatusInfo);

				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "DEFRA";
				AssertNoError(shipment.JS_CommunityTransitStatusInfo, "X-status is only for exports out of the customs territory");

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "DEFRA";
				AssertHasError(shipment.JS_CommunityTransitStatusInfo, "X-status is only for exports out of the customs territory");

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CNSHA";
				AssertHasError(shipment.JS_CommunityTransitStatusInfo, "X-status is only for exports out of the customs territory");

				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "GBLON";
				AssertHasError(shipment.JS_CommunityTransitStatusInfo, "X-status is only for exports out of the customs territory");
			}
		}

		public void TestValidateJS_ShipperCODPayMethod()
		{
			CommonShipment shipment = GetShipment();

			shipment.RunPreSaveValidation();
			AssertNoErrors(shipment.JS_ShipperCODPayMethodInfo);

			shipment.JS_ShipperCODAmount = 10m;
			shipment.RunPreSaveValidation();
			AssertHasErrors(shipment.JS_ShipperCODPayMethodInfo);

			shipment.JS_ShipperCODPayMethod = shipment.Lookups.ShipperCODPaymentTypes[0].Code;
			AssertNoErrors(shipment.JS_ShipperCODPayMethodInfo);

			shipment.JS_ShipperCODPayMethod = "XXX";
			AssertHasErrors(shipment.JS_ShipperCODPayMethodInfo);
		}

		public void TestValidateJS_ActualWeight()
		{
			var shipment = GetShipment();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			shipment.JS_ActualWeight = 5;
			Assert("5 should be valid.", !shipment.JS_ActualWeightInfo.HasNotifications());

			shipment.JS_ActualWeight = 0;
			AssertHasWarning("actual volume", shipment.JS_ActualWeightInfo, "You have not entered a Shipment Weight.");

			shipment.JS_ActualWeight = 1;
			AssertNoWarning("actual volume", shipment.JS_ActualWeightInfo, "You have not entered a Shipment Weight.");

			shipment.JS_ActualWeight = -10;
			AssertHasError(shipment.JS_ActualWeightInfo, "Please enter a 'Weight' greater than 0.");

			shipment.JS_ActualWeight = 1;
			AssertNoError(shipment.JS_ActualWeightInfo, "Please enter a 'Weight' greater than 0.");

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packline = shipment.OuterPackLines[0];
			packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline.JL_ActualWeight = 3;

			shipment.Validation.ValidateJS_ActualWeight();

			AssertHasWarning("total weight", shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.Validation.ValidateJS_ActualWeight();

			AssertHasWarning("total weight for HVL shipments", shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");

			shipment.JS_ActualWeight = 3;
			AssertNoWarning("total weight for HVL shipments", shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");
		}

		public void TestValidateJS_ActualWeight_HVLVLegacy()
		{
			var shipment = GetShipment();
			shipment.JS_ActualWeight = 1;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packline = shipment.OuterPackLines[0];
			packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline.JL_ActualWeight = 3;

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.Validation.ValidateJS_ActualWeight();
			AssertHasWarning("total weight for HLS shipments", shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");

			shipment.JS_ActualWeight = 3;
			AssertNoWarning("total weight for HLS shipments", shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");
		}

		public void TestValidateJS_ActualVolume()
		{
			var shipment = GetShipment();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			shipment.JS_ActualVolume = 0.5m;
			Assert("0.5 should be valid.", !shipment.JS_ActualVolumeInfo.HasNotifications());

			shipment.JS_ActualVolume = 0;
			AssertHasWarning("actual volume", shipment.JS_ActualVolumeInfo, "You have not entered a Shipment Volume.");

			shipment.JS_ActualVolume = 1;
			AssertNoWarning("actual volume", shipment.JS_ActualVolumeInfo, "You have not entered a Shipment Volume.");

			shipment.JS_ActualVolume = -1;
			AssertHasError(shipment.JS_ActualVolumeInfo, "Please enter a 'Volume' greater than 0.");

			shipment.JS_ActualVolume = 1;
			AssertNoError(shipment.JS_ActualVolumeInfo, "Please enter a 'Volume' greater than 0.");

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			var packline = shipment.OuterPackLines[0];
			packline.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline.JL_ActualVolume = 3;

			shipment.Validation.ValidateJS_ActualVolume();

			AssertHasWarning("total volume", shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.Validation.ValidateJS_ActualVolume();

			AssertHasWarning("total volume for HLS shipments", shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");

			shipment.JS_ActualVolume = 3;
			AssertNoWarning("total volume for HLS shipments", shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_ActualVolume = 1;
			AssertNoWarning("total volume for HLV shipments", shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");

			shipment.JS_ActualVolume = 3;
			AssertNoWarning("total volume for HLV shipments", shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			shipment.JS_ActualVolume = 0;
			AssertHasWarning(shipment.JS_ActualVolumeInfo, "You have not entered a Shipment Volume.");

			shipment.JS_ActualVolume = -1;
			AssertHasError(shipment.JS_ActualVolumeInfo, "Please enter a 'Volume' greater than 0.");

			shipment.JS_ActualVolume = 1;
			AssertNoNotifications(shipment.JS_ActualVolumeInfo);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FTL;

			shipment.JS_ActualVolume = 0;
			AssertHasWarning(shipment.JS_ActualVolumeInfo, "You have not entered a Shipment Volume.");

			shipment.JS_ActualVolume = -1;
			AssertHasError(shipment.JS_ActualVolumeInfo, "Please enter a 'Volume' greater than 0.");

			shipment.JS_ActualVolume = 1;
			AssertNoNotifications(shipment.JS_ActualVolumeInfo);
		}

		public void TestValidateJS_OuterPacks()
		{
			var shipment = GetShipment();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			shipment.JS_OuterPacks = -20;
			AssertHasError(shipment.JS_OuterPacksInfo, "Please enter a 'Packs' greater than or equal to 0.");

			shipment.JS_OuterPacks = 5;
			AssertNoError(shipment.JS_OuterPacksInfo, "Please enter a 'Packs' greater than or equal to 0.");

			var packline = shipment.OuterPackLines[0];
			packline.JL_PackageCount = 1;

			shipment.Validation.ValidateJS_OuterPacks();

			AssertHasWarning("total packs", shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.Validation.ValidateJS_OuterPacks();

			AssertHasWarning("total packs", shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");

			shipment.JS_OuterPacks = 1;
			AssertNoWarning("total packs", shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");
		}

		public void TestValidateJS_OuterPacks_HVLVLegacy()
		{
			var shipment = GetShipment();
			shipment.JS_OuterPacks = 5;

			var packline = shipment.OuterPackLines[0];
			packline.JL_PackageCount = 1;

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.Validation.ValidateJS_OuterPacks();
			AssertHasWarning("total packs", shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");

			shipment.JS_OuterPacks = 1;
			AssertNoWarning("total packs", shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");
		}

		public void TestCheckJS_RX_NKGoodValueCurr()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RX_NKGoodsValueCurr = "XXX";
			AssertHasErrors(shipment.JS_RX_NKGoodsValueCurrInfo);

			shipment.JS_RX_NKGoodsValueCurr = "USD";
			AssertNoErrors(shipment.JS_RX_NKGoodsValueCurrInfo);
		}

		public void TestCheckJS_RX_NKInsuranceCurrency()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_RX_NKInsuranceCurrency = "XXX";
			AssertHasErrors(shipment.JS_RX_NKInsuranceCurrencyInfo);

			shipment.JS_RX_NKInsuranceCurrency = "USD";
			AssertNoErrors(shipment.JS_RX_NKInsuranceCurrencyInfo);
		}

		public void TestValidateJS_HouseBill()
		{
			ZString consignRef = "S00000001";
			ZString duplicateHouseBill = "1";

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = consignRef;
			shipment1.JS_HouseBill = duplicateHouseBill;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_HouseBill = duplicateHouseBill;

			AssertEquals("Ship2 should have warnings on JS_HouseBill, duplicate entry", true, shipment2.JS_HouseBillInfo.HasWarnings());

			shipment2.JS_HouseBill = "2TEST1019209";
			AssertEquals("Ship2 should not have warnings on JS_HouseBill, not a duplicate entry", false, shipment2.JS_HouseBillInfo.HasWarnings());

			shipment1.JS_HouseBill = "";
			shipment2.JS_HouseBill = "";
			AssertEquals("Ship2 should not have warnings on JS_Housebill, empty", false, shipment2.JS_HouseBillInfo.HasWarnings());
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();

				var shipment1 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "WILD-ONE";
				shipment1.JS_HouseBill = "HELLO";
				shipment1.JS_IsForwardRegistered = true;
				shipment1.JS_IsCFSRegistered = false;

				var shipment2 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment2.JS_UniqueConsignRef = "WILD-TWO";
				shipment2.JS_HouseBill = "HELLO";
				shipment2.JS_IsForwardRegistered = false;
				shipment2.JS_IsCFSRegistered = true;

				var shipment3 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment3.JS_UniqueConsignRef = "WILD-THREE";
				shipment3.JS_HouseBill = "SOMETHING";

				creationFactory.Save();

				var localShipment1 = Factory.New<CommonShipment>();
				localShipment1.JS_UniqueConsignRef = "";
				localShipment1.JS_HouseBill = "HELLO";

				var localShipment2 = Factory.New<CommonShipment>();
				localShipment2.JS_UniqueConsignRef = "";
				localShipment2.JS_HouseBill = "IAMORIGINAL";

				var localShipment3 = Factory.New<CommonShipment>();
				localShipment3.JS_UniqueConsignRef = "";
				localShipment3.JS_HouseBill = "HELLO";

				var warningMessage = GetWarningMessageContent(localShipment3, "This House Bill number is already in use on:");
				AssertContains("Found shipments from both local cache and from database", "WILD-ONE", warningMessage);
				AssertContains("Found shipments from both local cache and from database", "WILD-TWO", warningMessage);
				AssertContains("Found shipments from both local cache and from database", "New Shipment", warningMessage);
			}
		}

		public void TestValidateJS_HouseBill_NoCheckForDuplicates_PendingAllocationSetBySystem()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creationFactory = new BusinessObjectFactory();

				var shipment1 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.IsPendingAllocationSetBySystem = false;
				shipment1.JS_HouseBill = "Pending Allocation..";

				creationFactory.Save();

				var shipment2 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.IsPendingAllocationSetBySystem = true;
				shipment2.JS_HouseBill = "Pending Allocation..";

				shipment2.Validation.ValidateJS_HouseBill();
				AssertNoWarnings(shipment2.JS_HouseBillInfo);

				shipment2.IsPendingAllocationSetBySystem = false;
				creationFactory.Save();

				Assert("Pre-requisite: JS_HouseBill has no changes", !shipment1.JS_HouseBillInfo.HasChanges);

				shipment1.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipment1.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				shipment2.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipment2.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nONE\r\n");
			}
		}

		public ZString GetWarningMessageContent(CommonShipment shipment, string generalMessage)
		{
			foreach (INotification warningMessage in shipment.JS_HouseBillInfo.GetWarnings())
			{
				if (warningMessage.Message.StartsWith(generalMessage))
				{
					return warningMessage.Message;
				}
			}

			return "";
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_HitDatabase()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();

				var shipment1 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";

				var shipment2 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";

				var shipment3 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment3.JS_UniqueConsignRef = "THREE";
				shipment3.JS_HouseBill = "HELLOWORLD";

				creationFactory.Save();

				var shipmentReloaded = Factory.Load<CommonShipment>(shipment1.PK);
				Assert("Pre-requisite: JS_HouseBill has no changes", !shipmentReloaded.JS_HouseBillInfo.HasChanges);

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				shipmentReloaded.JS_HouseBill = "HELLOWORLD";
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTHREE\r\n");

				var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobShipmentSchema.Constants.TableName, 3 },
					{ JobDeclarationSchema.Constants.TableName, 1 },
					{ JobShipmentPreplanningSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 1 }
				};
				AssertDbHits(expectedDBHits, Factory);
			}
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_ShouldNotRaiseWarningWhenItIsNotDuplicated()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();

				var shipment1 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";

				creationFactory.Save();

				var shipmentReloaded = Factory.Load<CommonShipment>(shipment1.PK);
				Assert("Pre-requisite: JS_HouseBill has no changes", !shipmentReloaded.JS_HouseBillInfo.HasChanges);

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertNoWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobShipmentSchema.Constants.TableName, 2 }
				};
				AssertDbHits(expectedDBHits, Factory);

				var shipment2 = Factory.NewWithValidTestData<CommonShipment>();

				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";
				shipment2.JS_IsForwardRegistered = true;

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				expectedDBHits = new Dictionary<string, int>()
				{
					{ JobShipmentSchema.Constants.TableName, 3 }
				};
				AssertDbHits(expectedDBHits, Factory);
			}
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_DatabaseLookupExcludeShipmentsFromFactoryCache()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();

				var shipment1 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";

				var shipment2 = creationFactory.NewWithValidTestData<CommonShipment>();
				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";

				creationFactory.Save();

				var shipment1Reloaded = Factory.Load<CommonShipment>(shipment1.PK);
				shipment1Reloaded.JS_HouseBill = "CHANGED";

				var localShipment = Factory.NewWithValidTestData<CommonShipment>();
				localShipment.JS_HouseBill = "HELLO";

				AssertHasWarning("Shipment should have warning", localShipment.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nTWO\r\n");
			}
		}

		public void TestValidateHouseBillForHVLVShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S0002001";
			shipment.JS_HouseBill = "BILL2001";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			Factory.Save();
			AssertNoErrors(shipment.JS_HouseBillInfo);

			shipment.JS_HouseBill = ZString.Empty;
			AssertHasErrors(string.Format("'{0}' shipment type should have House Bill Number.", Constants.ShipmentTypes.HighVolumeLowValueLegacy), shipment.JS_HouseBillInfo);

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "S0002002";
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment1.JS_HouseBill = ZString.Empty;

			var validation = new CommonShipmentValidation(shipment1);
			validation.ValidateJS_HouseBill();
			AssertNoErrors(shipment1.JS_HouseBillInfo);

			Factory.Save();
			AssertNoErrors(shipment1.JS_HouseBillInfo);

			shipment1.JS_HouseBill = "BILL2002";
			AssertNoErrors(shipment1.JS_HouseBillInfo);

			shipment1.JS_HouseBill = ZString.Empty;
			AssertHasErrors(string.Format("'{0}' shipment type should have House Bill Number.", Constants.ShipmentTypes.HighVolumeLowValueLegacy), shipment.JS_HouseBillInfo);
		}

		public void TestHasDuplicateHouseBillWarning()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ZString consignRef = "S00000001";
				ZString duplicateHouseBill = "1";
				CommonShipment ship1 = CommonShipment.New(Factory);
				ship1.JS_UniqueConsignRef = consignRef;
				ship1.JS_HouseBill = duplicateHouseBill;
				CommonShipment ship2 = CommonShipment.New(Factory);
				ship2.JS_HouseBill = duplicateHouseBill;
				AssertHasWarning("Ship2 should have warning on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");

				ship2.JS_HouseBill = "2TEST1019209";
				AssertNoWarning("Ship2 should not have warning on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");

				ship1.JS_HouseBill = "";
				ship2.JS_HouseBill = "";
				AssertNoWarning("Ship2 should not have warning on JS_HouseBill, duplicate entry", ship2.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nS00000001\r\n");
			}
		}

		public void TestValidateOriginAndDestinationIdentical()
		{
			CommonShipment shipment = GetShipment();

			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			AssertEquals("No errors when Origin is empty", false, shipment.JS_RL_NKOriginInfo.HasErrors());
			AssertEquals("No errors when Destination is empty", false, shipment.JS_RL_NKDestinationInfo.HasErrors());
			AssertEquals("No warnings when Origin is empty", false, shipment.JS_RL_NKOriginInfo.HasWarnings());
			AssertEquals("No warnings when Destination is empty", false, shipment.JS_RL_NKDestinationInfo.HasWarnings());

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("Destination same as Origin.", true, shipment.JS_RL_NKOriginInfo.HasWarnings());
			AssertEquals("Destination same as Origin.", true, shipment.JS_RL_NKDestinationInfo.HasWarnings());
			AssertEquals("Origin same as Destination - no error.", false, shipment.JS_RL_NKOriginInfo.HasErrors());
			AssertEquals("Destination same as Origin - no error.", false, shipment.JS_RL_NKDestinationInfo.HasErrors());

			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "AUBNE";
			AssertEquals("Destination same as Origin.", true, shipment.JS_RL_NKOriginInfo.HasWarnings());
			AssertEquals("Destination same as Origin.", true, shipment.JS_RL_NKDestinationInfo.HasWarnings());
			AssertEquals("Origin same as Destination - no error.", false, shipment.JS_RL_NKOriginInfo.HasErrors());
			AssertEquals("Destination same as Origin - no error.", false, shipment.JS_RL_NKDestinationInfo.HasErrors());

			ZString anotherLocation = "HKHKG";
			shipment.JS_RL_NKOrigin = anotherLocation;
			AssertEquals("Warnings on Origin should be cleared.", false, shipment.JS_RL_NKOriginInfo.HasWarnings());
		}

		public virtual void TestValidateOriginAndDestination()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSBY";

			AssertNoWarnings(shipment.JS_RL_NKOriginInfo);
			AssertNoWarnings(shipment.JS_RL_NKDestinationInfo);
			AssertNoErrors(shipment.JS_RL_NKOriginInfo);
			AssertNoErrors(shipment.JS_RL_NKDestinationInfo);

			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.IsDomesticFreight = true;

			shipment.JS_RL_NKOrigin = "AUXXX";
			shipment.JS_RL_NKDestination = "AUQQQ";

			AssertHasError(shipment.JS_RL_NKOriginInfo, "Enter a valid Origin.");
			AssertHasError(shipment.JS_RL_NKDestinationInfo, "Enter a valid Destination.");

			shipment.JS_RL_NKOrigin = "AU";
			shipment.JS_RL_NKDestination = "NZ";

			AssertNoWarnings(shipment.JS_RL_NKOriginInfo);
			AssertNoWarnings(shipment.JS_RL_NKDestinationInfo);
			AssertNoErrors(shipment.JS_RL_NKOriginInfo);
			AssertNoErrors(shipment.JS_RL_NKDestinationInfo);

			shipment.JS_RL_NKOrigin = "XX";
			shipment.JS_RL_NKDestination = "QQ";

			AssertHasError(shipment.JS_RL_NKOriginInfo, "Enter a valid Origin.");
			AssertHasError(shipment.JS_RL_NKDestinationInfo, "Enter a valid Destination.");
		}

		public void TestValidateReversedOriginAndDestination()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";

			var expectedErrorEnding = "but this Shipment is on a Consol moving in the opposite direction between the same ports. Origin and Destination may have been accidentally swapped.";
			AssertNoErrorContaining(shipment.JS_RL_NKOriginInfo, expectedErrorEnding);

			consol.JK_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "NZAKL";
			AssertHasErrorContaining(shipment.JS_RL_NKOriginInfo, expectedErrorEnding);
			AssertHasError(shipment.JS_RL_NKOriginInfo, $"Origin = NZAKL and Destination = AUSYD {expectedErrorEnding}");
		}

		public void TestHBLAWBChargesValidation()
		{
			const string errorMessage = "\"As Agreed\" option cannot be used for imports to Brazil";

			var shipment = Factory.New<CommonShipment>();

			shipment.JS_RL_NKOrigin = "BRRBA";
			shipment.JS_RL_NKDestination = "BRBAU";

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_RL_NKOrigin = "UAODS";
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertNoError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_RL_NKDestination = "BRBAU";
			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges;
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges;
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges;
			AssertHasError(shipment.JS_HBLAWBChargesDisplayInfo, errorMessage);

			shipment.JS_HBLAWBChargesDisplay = Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);
		}

		public void TestHBLAWBChargesValidation_Lookups()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_HBLAWBChargesDisplay = "PPD";
			AssertNoErrors(shipment.JS_HBLAWBChargesDisplayInfo);

			shipment.JS_HBLAWBChargesDisplay = "ABC";
			AssertHasErrors(shipment.JS_HBLAWBChargesDisplayInfo);
		}

		public void TestDateValidation()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;
			AssertEquals("No errors on ETD when empty.", false, shipment.JS_E_DEPInfo.HasErrors());
			AssertEquals("No errors on ETA when empty.", false, shipment.JS_E_ARVInfo.HasErrors());

			ZDateTime currentTime = ZDateTime.Now;
			shipment.JS_E_DEP = currentTime;
			AssertEquals("No errors on ETD when ETA is empty.", false, shipment.JS_E_DEPInfo.HasErrors());

			shipment.JS_E_ARV = currentTime;
			AssertEquals("ETA = ETD should be no errors.", false, shipment.JS_E_ARVInfo.HasErrors());

			shipment.JS_E_ARV = currentTime.AddDays(-1);
			AssertEquals("ETA 1 day before ETD. Should be error.", true, shipment.JS_E_ARVInfo.HasErrors());

			ZDateTime currentDate = ZDateTime.Today;
			shipment.JS_E_DEP = currentDate;
			shipment.JS_E_ARV = currentDate;
			AssertEquals("ETA = ETD, dates only. Should be no error.", false, shipment.JS_E_ARVInfo.HasErrors());

			shipment.JS_E_DEP = currentDate.AddDays(1);
			AssertEquals("ETD = ETA + 1 day. Should be error.", true, shipment.JS_E_DEPInfo.HasErrors());
		}

		public void TestDateValidationWhenTransportIsAir()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = "AIR";
			ZDateTime currentTime = ZDateTime.Now;

			shipment.JS_E_DEP = currentTime.AddDays(1);
			shipment.JS_E_ARV = currentTime;
			AssertEquals("ETA = ETD - 1 day for AIR. Should be no errors.", false, shipment.JS_E_ARVInfo.HasErrors());
			AssertEquals("ETD = ETA + 1 day for AIR. Should be no errors.", false, shipment.JS_E_DEPInfo.HasErrors());

			shipment.JS_E_DEP = currentTime.AddDays(1).AddSeconds(1);
			shipment.JS_E_ARV = currentTime;
			AssertEquals("ETA < ETD - 1 day for AIR. Should be error.", true, shipment.JS_E_ARVInfo.HasErrors());
			AssertEquals("ETD > ETA + 1 day for AIR. Should be error.", true, shipment.JS_E_DEPInfo.HasErrors());
		}

		public void TestDateValidationWhenIsNotAirAndETABeforeETDAndFromWesternSamoaToAmericanSamoa()
		{
			var transportModes = new string[]
			{
				TransportModes.Sea,
				TransportModes.SeaAir,
				TransportModes.Road,
				TransportModes.Rail,
				TransportModes.Courier
			};
			var dateList = new (ZDateTime Dep, ZDateTime Arv, bool ShouldHaveError)[]
			{
				(new ZDateTime(2022,2,4), new ZDateTime(2022,2,3), false),
				(new ZDateTime(2022,2,4), new ZDateTime(2022,2,2), true)
			};

			foreach (var mode in transportModes)
			{
				foreach (var item in dateList)
				{
					TestDateCheck(mode, item.Dep, item.Arv, item.ShouldHaveError);
				}
			}

			void TestDateCheck(string transportMode, ZDateTime dep, ZDateTime arv, bool shouldHaveError)
			{
				var shipment = GetShipment();
				shipment.JS_TransportMode = transportMode;
				shipment.JS_RL_NKOrigin = "WSABC";
				shipment.JS_RL_NKDestination = "ASXYZ";

				shipment.JS_E_DEP = dep;
				shipment.JS_E_ARV = arv;

				shipment.Validation.ValidateJS_E_DEP();
				shipment.Validation.ValidateJS_E_ARV();

				if (shouldHaveError)
				{
					AssertHasError(shipment.JS_E_DEPInfo, "ETD cannot be after Shipment ETA by more than allowable 1 day limit exception for route WSAPW to ASPPG due to crossing over the International Date line.");
					AssertHasError(shipment.JS_E_ARVInfo, "ETA cannot be before Shipment ETD by more than allowable 1 day limit exception for route WSAPW to ASPPG due to crossing over the International Date line.");
				}
				else
				{
					AssertNoErrors(shipment.JS_E_DEPInfo);
					AssertNoErrors(shipment.JS_E_ARVInfo);
				}
			}
		}

		#region ValidateOrignalAndCopyBills

		public void TestValidateOrignalAndCopyBills()
		{
			CommonShipment shipment = GetShipment();
			Assert("Default for Originals should be non-zero.", shipment.JS_NoOriginalBills > 0);
			Assert("Default for Copies should be non-zero.", shipment.JS_NoCopyBills > 0);

			shipment.JS_NoOriginalBills = 0;
			AssertEquals("Original 0, Copies not 0. No error.", false, shipment.JS_NoOriginalBillsInfo.HasErrors());

			shipment.JS_NoCopyBills = 0;
			AssertEquals("Orignal and Copies both 0. Error.", true, shipment.JS_NoOriginalBillsInfo.HasErrors());

			shipment.JS_NoOriginalBills = 2;
			AssertEquals("Original changed to non-zero. No error on Copies.", false, shipment.JS_NoCopyBillsInfo.HasErrors());
		}

		public void TestValidateOrignalAndCopyBills_IsNonForwardingBooking()
		{
			AssertValidateOrignalAndCopyBills_IsNonForwardingBooking(true, true, true);
			AssertValidateOrignalAndCopyBills_IsNonForwardingBooking(true, false, false);
			AssertValidateOrignalAndCopyBills_IsNonForwardingBooking(false, true, true);
			AssertValidateOrignalAndCopyBills_IsNonForwardingBooking(false, false, true);
		}

		void AssertValidateOrignalAndCopyBills_IsNonForwardingBooking(bool isBooking, bool isForwarderRegistered, bool expectedResult)
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_IsBooking = isBooking;
			shipment.JS_IsForwardRegistered = isForwarderRegistered;
			shipment.JS_NoOriginalBills = 0;
			shipment.JS_NoCopyBills = 0;

			AssertEquals(expectedResult, shipment.JS_NoOriginalBillsInfo.HasErrors());
			AssertEquals(expectedResult, shipment.JS_NoCopyBillsInfo.HasErrors());
		}

		public void TestValidateNumberOfOriginalsWhenReleaseTypeIsOriginalBill()
		{
			var shipment = GetShipment();

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			shipment.JS_NoOriginalBills = 0;
			shipment.JS_NoCopyBills = 2;
			AssertEquals("Original 0, Copies not 0, Release Type OBR. Warning.", true, shipment.JS_NoOriginalBillsInfo.HasWarnings());

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			shipment.JS_NoOriginalBills = 0;
			shipment.JS_NoCopyBills = 2;
			AssertEquals("Original 0, Copies not 0, Release Type OBO. Warning.", true, shipment.JS_NoOriginalBillsInfo.HasWarnings());

			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_NoOriginalBills = 0;
			shipment.JS_NoCopyBills = 2;
			AssertEquals("Original 0, Copies not 0, Release Type BRR. No warning.", false, shipment.JS_NoOriginalBillsInfo.HasWarnings());
		}

		#endregion

		public void TestValidateInactiveOrganisation()
		{
			CommonShipment shipment = GetShipment();
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "-CONSIGNOR-";
			consignor.OH_FullName = "INACTIVE CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsActive = false;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "-CONSIGNEE-";
			consignee.OH_FullName = "INACTIVE CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsActive = false;
			Factory.Save();

			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Should be error on inactive Consignor", true, shipment.ConsignorPKInfo.HasErrors());
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Should be error on inactive Consignee", true, shipment.ConsigneePKInfo.HasErrors());
		}

		public void TestValidateJS_OA_ExportReceivingDepot()
		{
			ZGuid guid = ZGuid.NewZGuid();
			CommonShipment shipment = GetShipment();

			shipment.JS_OA_ExportReceivingDepotInfo.AdditionalValidation += delegate
			{
				if (shipment.JS_OA_ExportReceivingDepot == guid)
				{
					shipment.JS_OA_ExportReceivingDepotInfo.AddError("No!");
				}
			};

			shipment.JS_OA_ExportReceivingDepot = guid;
			AssertHasError(shipment.JS_OA_ExportReceivingDepotInfo, "No!");

			shipment.JS_OA_ExportReceivingDepot = ZGuid.Empty;
			AssertNoErrors(shipment.JS_OA_ExportReceivingDepotInfo);
		}

		public void TestValidateControllingCustomerPK_ControllingCustomerFunctionalityAndValidations()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				shipment.ControllingCustomerAddress.OrganisationPK = controllingAgent.PK;
				AssertNoError(shipment.ControllingCustomerAddress.OrganisationPKInfo, "This organization is not a valid Controlling Customer.");

				shipment.ControllingCustomerAddress.E2_AddressOverride = true;
				AssertNoError(shipment.ControllingCustomerAddress.OrganisationPKInfo, "It is not possible to override a Controlling Customer. Amend the details on the linked organization.");
			}
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.ControllingCustomerAddress.OrganisationPK = controllingAgent.PK;
				AssertHasError(shipment.ControllingCustomerAddress.OrganisationPKInfo, "This organization is not a valid Controlling Customer.");

				shipment.ControllingCustomerAddress.E2_AddressOverride = true;
				AssertHasError(shipment.ControllingCustomerAddress.OrganisationPKInfo, "It is not possible to override a Controlling Customer. Amend the details on the linked organization.");
			}
		}

		#region JS_RS_NKGatewayServiceLevel

		public void TestValidateJS_RS_NKGatewayServiceLevel()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var agentPorts = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			agentPorts.O5_PortOrCountry = "AUSYD";
			agentPorts.O5_AgentDirection = "BTH";
			agentPorts.O5_SeaAgentStatus = "GTA";
			agentPorts.O5_AirAgentStatus = "GTA";
			agentPorts.O5_RailAgentStatus = "GTA";
			agentPorts.O5_RoadAgentStatus = "GTA";

			var gatewayService = agentPorts.ExclusiveGatewayServices.AddNew();
			gatewayService.O7_RS_NKGatewayService = "DIR";
			gatewayService.O7_RS_NKShipmentServiceLevel = "STD";

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", "STD", Factory);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			shipment.RequestPermissionByImpersonation = null;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONSOL1", "SEA", "AGT", "USLAX", "AUSYD", ZDateTime.Today, ZDateTime.Today.AddDays(5), Factory, shipment);
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			consol.RequestPermissionByImpersonation = null;

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;

			shipment.Validation.ValidateJS_RS_NKServiceLevel();
			AssertNoErrors(shipment.JS_RS_NKGatewayServiceLevelInfo); // pre-defined supporting shipment's gateway service level

			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertHasError(
				shipment.JS_RS_NKGatewayServiceLevelInfo,
				$"Gateway Service Level for {shipment.HumanReadableName} cannot be blank. Because it's attached to {consol.HumanReadableName} with Gateway Service Level 'DIR'."); // blank shipment gateway shipment level

			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertHasError(
				shipment.JS_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'DIR' to a consol with Gateway Service Level 'DIR'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.ReceivingForwarder.OH_Code}' or give you permission for this operation."); // the same gateway service levels and doesn't have permission

			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			AssertHasError(
				shipment.JS_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'DEF' to a consol with Gateway Service Level 'DIR'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.ReceivingForwarder.OH_Code}' or give you permission for this operation."); // different gateway service levels and doesn't have permission

			agentPorts.ExclusiveGatewayServices.RemoveAndDeleteAll();
			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			AssertNoErrors(shipment.JS_RS_NKGatewayServiceLevelInfo); // there is NO pre-defined supporting shipment's gateway service level
		}

		#endregion

		#region ValidateMasterShipmentWeight

		public void ValidateMasterShipmentWeight()
		{
			string error = "Total volumes are calculated from the master CommonShipment not sub-shipments. " +
							"Yet this master has an entered volume less than the total of it's sub shipments.";

			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment subShipment1 = Factory.New<CommonShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment1.JS_UnitOfWeight = Constants.Weight.Tonnes;
			subShipment1.JS_ActualWeight = 2;

			CommonShipment subShipment2 = Factory.New<CommonShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			subShipment2.JS_ActualWeight = 1500;

			masterShipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			masterShipment.JS_ActualWeight = 3501;
			AssertNoError(masterShipment.JS_ActualWeightInfo, error);

			masterShipment.JS_ActualWeight = 3499;
			AssertHasError(masterShipment.JS_ActualWeightInfo, error);

			masterShipment.JS_ActualWeight = 3500;
			AssertNoError(masterShipment.JS_ActualWeightInfo, error);
		}

		#endregion

		#region ValidateMasterShipmentVolume

		public void ValidateMasterShipmentVolume()
		{
			string error = "Total volumes are calculated from the master CommonShipment not sub-shipments. " +
							"Yet this master has an entered volume less than the total of it's sub shipments.";

			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			CommonShipment subShipment1 = Factory.New<CommonShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			subShipment1.JS_ActualVolume = 2;

			CommonShipment subShipment2 = Factory.New<CommonShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			subShipment2.JS_ActualVolume = 1500;

			masterShipment.JS_UnitOfVolume = Constants.Volume.Litre;
			masterShipment.JS_ActualVolume = 3501;
			AssertNoError(masterShipment.JS_ActualVolumeInfo, error);

			masterShipment.JS_ActualVolume = 3499;
			AssertHasError(masterShipment.JS_ActualVolumeInfo, error);

			masterShipment.JS_ActualVolume = 3500;
			AssertNoError(masterShipment.JS_ActualVolumeInfo, error);
		}

		#endregion

		#region Validate Coload Master

		public void TestJS_JS_ColoadMasterShipment_CoLoadMaster()
		{
			AssertJS_JS_ColoadMasterShipment(Constants.ShipmentTypes.CoLoadMaster);
			AssertJS_JS_ColoadMasterShipmentSCN(Constants.ShipmentTypes.CoLoadMaster);
		}

		public void TestJS_JS_ColoadMasterShipment_BlindCoLoadMaster()
		{
			AssertJS_JS_ColoadMasterShipment(Constants.ShipmentTypes.BlindCoLoadMaster);
			AssertJS_JS_ColoadMasterShipmentSCN(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		void AssertJS_JS_ColoadMasterShipment(ZString masterType)
		{
			var error = CargoWise.ComponentModel.NotificationType.Error;
			var warning = CargoWise.ComponentModel.NotificationType.Warning;

			var master = FreightTestHelper.GetShipment<CommonShipment>("MAS", masterType, Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_JS_ColoadMasterShipment = shipment.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "A Shipment cannot be its own Lead or Master.");

			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Only Standard House, Co-Load Master and Assembly Master shipments can be sub-shipments of a Lead or Master shipment.");

			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Only Standard House and High Volume Low Value shipments can be sub-shipments of a Co-Load Master shipment.");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			master.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "A Standard House shipment cannot be a Lead or Master Shipment.");

			master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			var sub1 = FreightTestHelper.GetShipment<CommonShipment>("SUB_1", Constants.ShipmentTypes.AssemblyMaster, Factory);
			sub1.JS_JS_ColoadMasterShipment = shipment.PK;
			shipment.JS_JS_ColoadMasterShipment = sub1.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master cannot be a Co-Load of this Shipment.");

			var sub2 = FreightTestHelper.GetShipment<CommonShipment>("SUB_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			sub2.JS_JS_ColoadMasterShipment = sub1.PK;
			shipment.JS_JS_ColoadMasterShipment = sub2.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master cannot be a Co-Load of this Shipment.");

			var superMaster = FreightTestHelper.GetShipment<CommonShipment>("SUPER_MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			master.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			master.JS_JS_ColoadMasterShipment = superMaster.PK;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(warning, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master is already a Co-Load of another Shipment.");

			master.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		void AssertJS_JS_ColoadMasterShipmentSCN(ZString masterType)
		{
			var error = CargoWise.ComponentModel.NotificationType.Error;
			var warning = CargoWise.ComponentModel.NotificationType.Warning;

			var master = FreightTestHelper.GetShipment<CommonShipment>("MAS", masterType, Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.ShippersConsolLead, Factory);
			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;

			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_JS_ColoadMasterShipment = shipment.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "A Shipment cannot be its own Lead or Master.");

			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Only Standard House, Co-Load Master and Assembly Master shipments can be sub-shipments of a Lead or Master shipment.");

			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Only Standard House and High Volume Low Value shipments can be sub-shipments of a Co-Load Master shipment.");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			master.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "A Standard House shipment cannot be a Lead or Master Shipment.");

			master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			var sub1 = FreightTestHelper.GetShipment<CommonShipment>("SUB_1", Constants.ShipmentTypes.AssemblyMaster, Factory);
			sub1.JS_JS_ColoadMasterShipment = shipment.PK;
			shipment.JS_JS_ColoadMasterShipment = sub1.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master cannot be a Co-Load of this Shipment.");

			var sub2 = FreightTestHelper.GetShipment<CommonShipment>("SUB_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			sub2.JS_JS_ColoadMasterShipment = sub1.PK;
			shipment.JS_JS_ColoadMasterShipment = sub2.PK;
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master cannot be a Co-Load of this Shipment.");

			var superMaster = FreightTestHelper.GetShipment<CommonShipment>("SUPER_MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			master.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			master.JS_JS_ColoadMasterShipment = superMaster.PK;
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			AssertHasNotifications(warning, shipment.JS_JS_ColoadMasterShipmentInfo, "Co-Load Master is already a Co-Load of another Shipment.");

			master.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			master.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestJS_JS_ColoadMasterShipment_Consols()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var error = CargoWise.ComponentModel.NotificationType.Error;

			string noValidMessage = "Co-Load Master is no longer valid. It may have been detached.";

			var master = FreightTestHelper.GetShipment<CommonShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.AssemblyMaster, Factory);

			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			#region Set up valid Consols

			var consol1 = FreightTestHelper.GetConsol<CommonConsol>("CON_1", Factory);
			var consol2 = FreightTestHelper.GetConsol<CommonConsol>("CON_2", Factory);
			var consol3 = FreightTestHelper.GetConsol<CommonConsol>("CON_3", Factory);
			var consol4 = FreightTestHelper.GetConsol<CommonConsol>("CON_4", Factory);
			consol1.JK_RL_NKLoadPort = "GBLON";
			consol1.JK_RL_NKDischargePort = "ESMAD";
			consol2.JK_RL_NKLoadPort = "ESMAD";
			consol2.JK_RL_NKDischargePort = "ZAJNB";
			consol3.JK_RL_NKLoadPort = "ZAJNB";
			consol3.JK_RL_NKDischargePort = "HKHKG";
			consol4.JK_RL_NKLoadPort = "HKHKG";
			consol4.JK_RL_NKDischargePort = "AUSYD";
			consol1.Transports[0].JW_IsLinked = false;
			consol2.Transports[0].JW_IsLinked = false;
			consol3.Transports[0].JW_IsLinked = false;
			consol4.Transports[0].JW_IsLinked = false;
			consol1.Transports[0].JW_VoyageFlight = "0001";
			consol2.Transports[0].JW_VoyageFlight = "0002";
			consol3.Transports[0].JW_VoyageFlight = "0003";
			consol4.Transports[0].JW_VoyageFlight = "0004";

			#endregion

			shipment.Consols.AddRange(consol2, consol3);
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			FreightTestHelper.AssertConsolCollection(master.Consols);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol2, consol3);
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			master.Consols.Add(consol1);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol1, consol2, consol3);
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.Consols.Remove(consol1);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol2, consol3);
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, noValidMessage);

			master.Consols.Add(consol2);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1, consol2);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol2, consol3);
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, noValidMessage);

			shipment.Consols.Add(consol1);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1, consol2);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol1, consol2, consol3);
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			shipment.Consols.RemoveAll();
			shipment.Consols.Add(consol4);

			ZDateTime today = ZDateTime.Today;
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			consol1.JK_AgentType = Constants.AgentType.Direct;
			consol2.JK_ConsolCutOffDate = today.AddDays(-1);

			string isDirectMessage = "Consol CON_1 cannot be attached to the Shipment SHP from its proposed Master/Lead MAS as the Consol is a Direct Consol and it already has a Direct Shipment MAS.";
			string cutOffDateMessage = string.Format(
				"Consol CON_2 cannot be attached to the Shipment SHP from its proposed Master/Lead MAS as the Consol Cut Off Date has now passed.\r\nSupervisor access is required to attach the Consol at this time.\r\n{0}",
				Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate));

			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1, consol2);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol4);
			AssertHasErrorContaining(shipment.JS_JS_ColoadMasterShipmentInfo, isDirectMessage);
			AssertHasErrorContaining(shipment.JS_JS_ColoadMasterShipmentInfo, cutOffDateMessage);

			consol1.JK_AgentType = Constants.AgentType.Agent;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasErrorContaining(shipment.JS_JS_ColoadMasterShipmentInfo, noValidMessage);
			AssertHasErrorContaining(shipment.JS_JS_ColoadMasterShipmentInfo, cutOffDateMessage);

			shipment.Consols.Add(consol1);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			FreightTestHelper.AssertConsolCollection(master.Consols, consol1, consol2);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol1, consol4);
			AssertHasErrorContaining(shipment.JS_JS_ColoadMasterShipmentInfo, cutOffDateMessage);

			consol2.JK_ConsolCutOffDateLocal = ZDateTime.Empty;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, noValidMessage);

			shipment.Consols.Add(consol2);
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestJS_JS_ColoadMasterShipment_NoErrorMessage_WhenMasterShipmentTypeIsBuyersConsolLead()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var error = CargoWise.ComponentModel.NotificationType.Error;

			var noValidMasterMessage = "Co-Load Master is no longer valid. It may have been detached.";

			var master = FreightTestHelper.GetShipment<CommonShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.StandardHouse, Factory);
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			var consol1 = FreightTestHelper.GetConsol<CommonConsol>("CON_1", Factory);
			consol1.JK_RL_NKLoadPort = "GBLON";
			consol1.JK_RL_NKDischargePort = "ESMAD";
			consol1.Transports[0].JW_IsLinked = false;
			consol1.Transports[0].JW_VoyageFlight = "0001";

			master.Consols.Add(consol1);
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			shipment.Consols.RemoveAll();
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, noValidMasterMessage);

			master.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			master.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestJS_JS_ColoadMasterShipment_NoErrorMessage_WhenMasterShipmentTypeIsShippersConsolLead()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var error = CargoWise.ComponentModel.NotificationType.Error;

			var noValidMasterMessage = "Co-Load Master is no longer valid. It may have been detached.";

			var master = FreightTestHelper.GetShipment<CommonShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.StandardHouse, Factory);
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);

			var consol1 = FreightTestHelper.GetConsol<CommonConsol>("CON_1", Factory);
			consol1.JK_RL_NKLoadPort = "GBLON";
			consol1.JK_RL_NKDischargePort = "ESMAD";
			consol1.Transports[0].JW_IsLinked = false;
			consol1.Transports[0].JW_VoyageFlight = "0001";

			master.Consols.Add(consol1);
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			shipment.Consols.RemoveAll();
			AssertHasNotifications(error, shipment.JS_JS_ColoadMasterShipmentInfo, noValidMasterMessage);

			master.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			master.JS_PackingMode = Core.Constants.ContainerModes.ShippersConsol;
			shipment.Validation.ValidateJS_JS_ColoadMasterShipment();
			AssertNoNotifications(shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestJS_JS_ColoadMasterShipment_DirectShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Shipment is direct", shipment.IsDirectShipment);

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertNoErrors("Direct shipment can have a Coload Master as long as it does not become a nested Master shipment.", shipment.JS_JS_ColoadMasterShipmentInfo);
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertHasError("Direct shipment can not have a Coload Master which makes it a nested Master shipment. Error expected.", shipment.JS_JS_ColoadMasterShipmentInfo, "A Shipment attached to a Direct Consol can not have a Master shipment.");

			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			consol.JK_AgentType = Constants.AgentType.Agent;
			Assert("Shipment is not direct", !shipment.IsDirectShipment);
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertNoErrors("Shipment is not direct. No error expected.", shipment.JS_JS_ColoadMasterShipmentInfo);
		}

		protected static void AssertHasNotifications(INotificationType notificationType, ZPropertyInfo info, params string[] expected)
		{
			AssertContainsExactElementsInAnyOrder(expected, info.Notifications.Where(n => n.Type == notificationType).Select(n => n.Message));
		}

		#endregion

		#region TestShipmentDisplaysErrorWhenAttachingMasterWithErrors

		public void TestShipmentDisplaysErrorWhenAttachingMasterWithErrors()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";
			masterShipment.JS_TransportMode = "BAD";

			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertHasErrorContaining(subShipment.JS_JS_ColoadMasterShipmentInfo, "This shipment cannot be saved while attached to Master/Lead Shipment (House Bill='MASTER') due to the following errors on the Master:");
			AssertHasErrorContaining(subShipment.JS_JS_ColoadMasterShipmentInfo, "Transport Mode - Enter a valid Transport Mode");
		}

		public void TestShipmentDisplaysErrorWhenAttachingMasterWithErrors_WeightAndVolumeAddedAfterMaster()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";

			var subShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			var subShipment2 = Factory.NewWithValidTestData<CommonShipment>();

			subShipment1.JS_ActualWeight = 10m;
			subShipment1.JS_ActualVolume = 200000m;

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			AssertNoErrors("Pre-condition: no error expected as Sub Shipment 1 alone will not cause an overflow error", masterShipment);
			AssertNoErrors("Pre-condition", subShipment1);
			AssertNoErrors("Pre-condition", subShipment2);

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			subShipment2.JS_ActualWeight = 999999.999m;
			subShipment2.JS_ActualVolume = 900000m;

			Assert("Expect the Master Shipment now to have too much weight and volume", masterShipment.HasErrors);

			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "This shipment cannot be saved while attached to Master/Lead Shipment S00001000 (House Bill='MASTER') due to the following errors on the Master:");
			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "Weight - The number 1,000,009.999 is too large, the maximum value allowed for Weight is 999,999.999.");
			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "Volume - The number 1,100,000 is too large, the maximum value allowed for Volume is 999,999.999.");

			AssertNoErrors("Sub Shipment 1 should remain uneffect as nothing has been saved", subShipment1.JS_JS_ColoadMasterShipmentInfo);
		}

		public void TestShipmentDisplaysErrorWhenAttachingMasterWithErrors_WeightAndVolumeAddedBeforeMaster()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";

			var subShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			var subShipment2 = Factory.NewWithValidTestData<CommonShipment>();

			subShipment1.JS_ActualWeight = 10m;
			subShipment1.JS_ActualVolume = 200000m;
			subShipment2.JS_ActualWeight = 999999.999m;
			subShipment2.JS_ActualVolume = 900000m;

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			AssertNoErrors("Pre-condition: no error expected as Sub Shipment 1 alone will not cause an overflow error", masterShipment);
			AssertNoErrors("Pre-condition", subShipment1);
			AssertNoErrors("Pre-condition", subShipment2);

			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Assert("Expect the Master Shipment now to have too much weight and volume", masterShipment.HasErrors);

			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "This shipment cannot be saved while attached to Master/Lead Shipment S00001000 (House Bill='MASTER') due to the following errors on the Master:");
			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "Weight - The number 1,000,009.999 is too large, the maximum value allowed for Weight is 999,999.999.");
			AssertHasErrorContaining(subShipment2.JS_JS_ColoadMasterShipmentInfo, "Volume - The number 1,100,000 is too large, the maximum value allowed for Volume is 999,999.999.");

			AssertNoErrors("Sub Shipment 1 should remain uneffect as nothing has been saved", subShipment1.JS_JS_ColoadMasterShipmentInfo);
		}

		#endregion

		#region Packline Totals

		#region Inner

		public void TestValidateTotalInnerPackLinePackages()
		{
			CommonShipment shipment = GetShipment();
			shipment.InnerPackLines.AddNew().JL_PackageCount = 2;
			shipment.InnerPackLines.AddNew().JL_PackageCount = 3;

			shipment.JS_TotalPackageCount = 2;
			shipment.Validation.ValidateTotalInnerPackLinePackages();
			Assert("Warning expected", shipment.TotalInnerPackLinePackagesInfo.HasWarnings());

			shipment.JS_TotalPackageCount = 5;
			shipment.Validation.ValidateTotalInnerPackLinePackages();
			Assert("No warnings expected", !shipment.TotalInnerPackLinePackagesInfo.HasWarnings());
		}

		public void TestValidateTotalInnerPackLineLoadingMeters()
		{
			CommonShipment shipment = GetShipment();
			shipment.InnerPackLines.AddNew().JL_LoadingMeters = 2;
			shipment.InnerPackLines.AddNew().JL_LoadingMeters = 3;

			AssertEquals("Precondition: RoadLoadingMeters disabled", false, shipment.IsRoadLoadingMetersEnabled);

			shipment.JS_LoadingMeters = 2;
			shipment.Validation.ValidateTotalInnerPackLineLoadingMeters();
			Assert("No warnings expected: RoadLoadingMeters disabled ", !shipment.TotalInnerPackLineLoadingMetersInfo.HasWarnings());

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Precondition: RoadLoadingMeters enabled", true, shipment.IsRoadLoadingMetersEnabled);

			shipment.JS_LoadingMeters = 2;
			shipment.Validation.ValidateTotalInnerPackLineLoadingMeters();
			Assert("Warning expected", shipment.TotalInnerPackLineLoadingMetersInfo.HasWarnings());

			shipment.JS_LoadingMeters = 5;
			shipment.Validation.ValidateTotalInnerPackLineLoadingMeters();
			Assert("No warnings expected", !shipment.TotalInnerPackLineLoadingMetersInfo.HasWarnings());
		}

		#endregion

		#region Outer

		public void TestValidateTotalOuterPacks()
		{
			CommonShipment shipment = GetShipment();
			shipment.OuterPackLines.AddNew().JL_PackageCount = 2;
			shipment.OuterPackLines.AddNew().JL_PackageCount = 3;

			shipment.JS_OuterPacks = 2;
			shipment.Validation.ValidateTotalOuterPacks();
			Assert("Warning expected", shipment.TotalOuterPacksInfo.HasWarnings());

			shipment.JS_OuterPacks = 5;
			shipment.Validation.ValidateTotalOuterPacks();
			Assert("No warnings expected", !shipment.TotalOuterPacksInfo.HasWarnings());
		}

		public void TestValidateTotalOuterPacksWeight()
		{
			CommonShipment shipment = GetShipment();
			shipment.OuterPackLines.AddNew().JL_ActualWeight = 2;
			shipment.OuterPackLines.AddNew().JL_ActualWeight = 3;

			shipment.JS_ActualWeight = 2;
			shipment.Validation.ValidateTotalOuterPacksWeight();
			Assert("Warning expected", shipment.TotalOuterPacksWeightInfo.HasWarnings());

			shipment.JS_ActualWeight = 5;
			shipment.Validation.ValidateTotalOuterPacksWeight();
			Assert("No warnings expected", !shipment.TotalOuterPacksWeightInfo.HasWarnings());
		}

		public void TestValidateTotalOuterPacksVolume()
		{
			CommonShipment shipment = GetShipment();
			shipment.OuterPackLines.AddNew().JL_ActualVolume = 2;
			shipment.OuterPackLines.AddNew().JL_ActualVolume = 3;

			shipment.JS_ActualVolume = 2;
			shipment.Validation.ValidateTotalOuterPacksVolume();
			Assert("Warning expected", shipment.TotalOuterPacksVolumeInfo.HasWarnings());

			shipment.JS_ActualVolume = 5;
			shipment.Validation.ValidateTotalOuterPacksVolume();
			Assert("No warnings expected", !shipment.TotalOuterPacksVolumeInfo.HasWarnings());
		}

		public void TestValidateTotalOuterPacksLoadingMeters()
		{
			CommonShipment shipment = GetShipment();
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 2;
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 3;

			AssertEquals("Precondition: RoadLoadingMeters disabled", false, shipment.IsRoadLoadingMetersEnabled);

			shipment.JS_LoadingMeters = 2;
			shipment.Validation.ValidateTotalOuterPacksLoadingMeters();
			Assert("No warnings expected: RoadLoadingMeters disabled ", !shipment.TotalOuterPacksLoadingMetersInfo.HasWarnings());

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Precondition: RoadLoadingMeters enabled", true, shipment.IsRoadLoadingMetersEnabled);

			shipment.JS_LoadingMeters = 2;
			shipment.Validation.ValidateTotalOuterPacksLoadingMeters();
			Assert("Warning expected", shipment.TotalOuterPacksLoadingMetersInfo.HasWarnings());

			shipment.JS_LoadingMeters = 5;
			shipment.Validation.ValidateTotalOuterPacksLoadingMeters();
			Assert("No warnings expected", !shipment.TotalOuterPacksLoadingMetersInfo.HasWarnings());
		}

		#endregion

		public void TestValidationDoesNotChangeData()
		{
			CommonShipment shipment = GetShipment();
			Factory.Save();
			shipment.RunPreSaveValidation();
			AssertEquals(false, shipment.HasChanges);
		}

		#endregion

		#region Test Validation Doesnt cause exception

		[ExpectNoExceptions]
		public void TestValidateJS_E_ARVDoesntCauseDuplicateMessageError()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.HomePort.Code;
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_ETA = ZDateTime.Today.AddDays(-2);
			transport.JW_ATD = ZDateTime.Today;
			transport.JW_ATA = ZDateTime.Today.AddDays(-2);

			CommonShipment shipment1 = CommonShipment.New(Factory);
			consol.Shipments.Add(shipment1);
			shipment1.JS_E_DEP = ZDateTime.Today;
			shipment1.JS_E_ARV = ZDateTime.Today.AddDays(-2);
			shipment1.JS_E_ARV = ZDateTime.Today.AddDays(-4);
			consol.RunPreSaveValidation();
			shipment1.RunPreSaveValidation();
			shipment1.JS_E_ARV = ZDateTime.Today.AddDays(-8);
			consol.RunPreSaveValidation();
			shipment1.RunPreSaveValidation();
			Factory.Save();
		}

		#endregion

		#region JS_INCO

		public void TestJS_INCO_Validation()
		{
			CommonShipment shipment = GetShipment();

			shipment.Validation.ValidateJS_INCO();
			AssertNoErrors(shipment.JS_INCOInfo);

			FreightConfigurationRegistry.Instance.MandatoryIncoTerm.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);

			shipment.Validation.ValidateJS_INCO();
			AssertNoErrors(shipment.JS_INCOInfo);

			shipment.JS_RL_NKOrigin = "ZZTOP";
			shipment.Validation.ValidateJS_INCO();
			AssertNoErrors(shipment.JS_INCOInfo);

			shipment.JS_INCO = "";
			AssertNoErrors("", shipment.JS_INCOInfo);

			shipment.JS_INCO = "XXX";
			AssertHasErrors("", shipment.JS_INCOInfo);

			shipment.JS_INCO = shipment.Lookups.JS_INCO_List[0].Code;
			AssertNoErrors(shipment.JS_INCOInfo);
		}

		[TestDate(2019, 1, 1)]
		public void TestJS_INCO_ValidationBefore2020()
		{
			var shipment = Factory.New<CommonShipment>();

			foreach (var incoterm in obsoleteIncoterms)
			{
				shipment.JS_INCO = incoterm;
				shipment.Validation.ValidateJS_INCO();
				AssertHasError(shipment.JS_INCOInfo, "Enter a valid Incoterm.");
			}

			foreach (var incoterm in expiringIncoterms2020)
			{
				shipment.JS_INCO = incoterm;
				shipment.Validation.ValidateJS_INCO();
				AssertNoNotifications(shipment.JS_INCOInfo);
			}

			foreach (var incoterm in newIncoterms2020)
			{
				shipment.JS_INCO = incoterm;
				shipment.Validation.ValidateJS_INCO();
				AssertHasError(shipment.JS_INCOInfo, "Enter a valid Incoterm.");
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestJS_INCO_ValidationAfter2020()
		{
			var shipment = Factory.New<CommonShipment>();

			foreach (var incoterm in obsoleteIncoterms)
			{
				shipment.JS_INCO = incoterm;
				AssertHasError(shipment.JS_INCOInfo, "Enter a valid Incoterm.");
			}

			foreach (var incoterm in expiringIncoterms2020)
			{
				shipment.JS_INCO = incoterm;
				AssertHasWarning(shipment.JS_INCOInfo, "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules.");
			}

			foreach (var incoterm in newIncoterms2020)
			{
				shipment.JS_INCO = incoterm;
				AssertNoNotifications(shipment.JS_INCOInfo);
			}
		}

		readonly IEnumerable<string> obsoleteIncoterms = Constants.IncoTerms.Incoterms2000.Except(Constants.IncoTerms.Incoterms2010);

		readonly IEnumerable<string> expiringIncoterms2020 = Constants.IncoTerms.Incoterms2010.Except(Constants.IncoTerms.Incoterms2020);

		readonly IEnumerable<string> newIncoterms2020 = Constants.IncoTerms.Incoterms2020.Except(Constants.IncoTerms.Incoterms2010);

		#endregion

		#region ConsolShipment Validation

		public void TestValidateJS_TransportModeAgainstConsol()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();

			consol.JK_TransportMode = "";
			shipment.Validation.ValidateJS_TransportMode();
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());

			ValidateAirTransport();
			ValidateSeaTransport();
			ValidateOtherTransport();
		}

		void ValidateAirTransport()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();

			consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert("Warning expected", shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			Assert("Warning expected", shipment.JS_TransportModeInfo.HasWarnings());
		}

		void ValidateSeaTransport()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("Warning expected", shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			Assert("Warning expected", shipment.JS_TransportModeInfo.HasWarnings());
		}

		void ValidateOtherTransport()
		{
			CommonShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();

			consol.JK_TransportMode = Constants.TransportModes.Road;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert("Warning expected", shipment.JS_TransportModeInfo.HasWarnings());

			consol.JK_TransportMode = Constants.TransportModes.Road;
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			Assert("No warning expected", !shipment.JS_TransportModeInfo.HasWarnings());
		}

		#endregion

		#region Shipment Weight Units

		[ExpectNoExceptions]
		public void TestValidateJS_ActualWeightWithInvalidMasterUnit()
		{
			CommonShipment shipmentMaster = Factory.New<CommonShipment>();
			shipmentMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipmentMaster.JS_UnitOfWeight = "X";

			CommonShipment shipmentChild = Factory.New<CommonShipment>();
			shipmentChild.JS_JS_ColoadMasterShipment = shipmentMaster.PK;

			shipmentMaster.Validation.ValidateJS_ActualWeight();
		}

		[ExpectNoExceptions]
		public void TestValidateJS_ActualWeightWithInvalidChildUnit()
		{
			CommonShipment shipmentMaster = Factory.New<CommonShipment>();

			CommonShipment shipmentChild = Factory.New<CommonShipment>();
			shipmentChild.JS_JS_ColoadMasterShipment = shipmentMaster.PK;
			shipmentChild.JS_UnitOfWeight = "X";

			shipmentMaster.Validation.ValidateJS_ActualWeight();
		}

		#endregion

		#region ShipmentType

		public void TestValidateJS_ShipmentType()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = "CRP";
			AssertHasErrors(shipment.JS_ShipmentTypeInfo);

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Direct;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertNoErrors("STD shipment can have Direct Consols. No error expected.", shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertNoErrors("ASM shipment can have Direct Consols. No error expected.", shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertNoErrors("HVL shipment can have Direct Consols. No error expected.", shipment.JS_ShipmentTypeInfo);

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertNoErrors("STD shipment with Coload Master can have Direct Consols. No error expected.", shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertNoErrors("HVL shipment with Coload Master can have Direct Consols. No error expected.", shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertHasError("ASM shipment with Coload Master can not have Direct Consols. Error expected.", shipment.JS_ShipmentTypeInfo, "This Shipment is attached to at least one Direct Consol and must be marked as STD, HVL or ASM");

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertHasError("HVL shipment with Coload Master with type HVM can not have Direct Consols. Error expected.", shipment.JS_ShipmentTypeInfo, "This Shipment is attached to at least one Direct Consol and must be marked as STD, HVL or ASM");
		}

		public void TestValidateJS_ShipmentType_3PT()
		{
			var shipment = CommonShipment.New(Factory);
			var error = "The Goods Manufacturer Address is mandatory if 3PT shipment type is selected, go to Addresses tab to add it.";

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ThirdPartyOwnershipHouse;
			AssertHasError(shipment.JS_ShipmentTypeInfo, error);

			var org = Factory.New<OrgHeader>();

			shipment.ManufacturerDocAddress.OrganisationPK = org.PK;
			shipment.Validation.ValidateJS_ShipmentType();
			AssertNoError(shipment.JS_ShipmentTypeInfo, error);
		}

		#endregion

		#region Freight Spot Rate Autorating Mode

		public void TestAutoratingMode()
		{
			var shipment = GetShipment();

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.StandardRate;

			AssertNoErrors(shipment.JS_FreightSpotRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightCostRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightGatewaySellRateAutoratingModeInfo);

			shipment.JS_UnitFreightRate = 101.2500m;
			shipment.JS_FreightCostRate = 121.5800m;
			shipment.JS_GatewayFreightSellRate = 150.6800m;

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;

			AssertNoErrors(shipment.JS_FreightSpotRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightCostRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightGatewaySellRateAutoratingModeInfo);

			shipment.JS_FreightSpotRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightCostRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_FreightGatewaySellRateAutoratingMode = Constants.FreightRateAutoratingModes.Code.AllInRate;

			AssertNoErrors(shipment.JS_FreightSpotRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightCostRateAutoratingModeInfo);
			AssertNoErrors(shipment.JS_FreightGatewaySellRateAutoratingModeInfo);

			shipment.JS_FreightSpotRateAutoratingMode = "XYZ";
			shipment.JS_FreightCostRateAutoratingMode = "XYZ";
			shipment.JS_FreightGatewaySellRateAutoratingMode = "XYZ";

			AssertHasErrors(shipment.JS_FreightSpotRateAutoratingModeInfo);
			AssertHasErrors(shipment.JS_FreightCostRateAutoratingModeInfo);
			AssertHasErrors(shipment.JS_FreightGatewaySellRateAutoratingModeInfo);

			shipment.JS_FreightSpotRateAutoratingMode = "";
			shipment.JS_FreightCostRateAutoratingMode = "";
			shipment.JS_FreightGatewaySellRateAutoratingMode = "";

			AssertHasErrors(shipment.JS_FreightSpotRateAutoratingModeInfo);
			AssertHasErrors(shipment.JS_FreightCostRateAutoratingModeInfo);
			AssertHasErrors(shipment.JS_FreightGatewaySellRateAutoratingModeInfo);
		}

		public void TestFreightRateCurrency()
		{
			var shipment = GetShipment();
			shipment.JS_RX_NKFrtRateCurrency = "ABC";
			shipment.JS_RX_NKFreightCostRateCurrency = "XYZ";
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "HEY";

			AssertHasErrors(shipment.JS_RX_NKFrtRateCurrencyInfo);
			AssertHasErrors(shipment.JS_RX_NKFreightCostRateCurrencyInfo);
			AssertHasErrors(shipment.JS_RX_NKGatewayFreightSellRateCurrencyInfo);

			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "NZD";

			AssertNoErrors(shipment.JS_RX_NKFrtRateCurrencyInfo);
			AssertNoErrors(shipment.JS_RX_NKFreightCostRateCurrencyInfo);
			AssertNoErrors(shipment.JS_RX_NKGatewayFreightSellRateCurrencyInfo);
		}

		#endregion

		#region Consignee/ConsignorRelatedParties

		public void TestConsigneeHasBeenChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			var address1 = org.MainAddress;
			address1.OA_Address1 = "ASDF lane 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SDFG lane 2";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = org.PK;

			Factory.Save();

			AssertEquals("Nothing changes", false, shipment.Validation.ConsigneeHasBeenChanged());

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
			Factory.Save();

			AssertEquals("Address is changed", false, shipment.Validation.ConsigneeHasBeenChanged());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = org1.PK;

			AssertEquals("Organization is changed", true, shipment.Validation.ConsigneeHasBeenChanged());
		}

		public void TestConsignorHasBeenChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			var address1 = org.MainAddress;
			address1.OA_Address1 = "ASDF lane 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SDFG lane 2";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsignorPK = org.PK;

			Factory.Save();

			AssertEquals("Nothing changes", false, shipment.Validation.ConsignorHasBeenChanged());

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
			Factory.Save();

			AssertEquals("Address is changed", false, shipment.Validation.ConsignorHasBeenChanged());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XYZW";
			shipment.ConsignorPK = org1.PK;

			AssertEquals("Organization is changed", true, shipment.Validation.ConsignorHasBeenChanged());
		}

		public void TestValidateConsigneeRelatedPartiesMatchReceivingSendingAgent()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns("message");
				shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
				AssertHasWarning(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo, "message");
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
				AssertNoWarnings(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo);
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns("message");
				shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
				AssertHasWarning(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo, "message");
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
				AssertNoWarnings(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo);
			}
		}

		public void TestValidateConsignorRelatedPartiesMatchReceivingSendingAgent()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns("message");
				shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
				AssertHasWarning(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, "message");
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
				AssertNoWarnings(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo);
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns("message");
				shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
				AssertHasWarning(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, "message");
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
				AssertNoWarnings(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo);
			}
		}

		#endregion

		#region CheckHasCriticalChangesOnProperty

		public void TestCheckHasCriticalChangesOnProperty_PackingMode()
		{
			var originalValue = new ZString(Constants.ContainerModes.LCL);
			var newValue = new ZString(Constants.ContainerModes.FCL);

			AssertHasCriticalErrorOnProperty(JobShipmentSchema.JS_PackingMode.Name, originalValue, newValue);
		}

		public void TestCheckHasCriticalChangesOnProperty_TransportMode()
		{
			var originalValue = new ZString(Constants.TransportModes.Sea);
			var newValue = new ZString(Constants.TransportModes.Air);

			AssertHasCriticalErrorOnProperty(JobShipmentSchema.JS_TransportMode.Name, originalValue, newValue);
		}

		public void TestCheckHasCriticalChangesOnProperty_ShipmentType()
		{
			var originalValue = new ZString(Constants.ShipmentTypes.StandardHouse);
			var newValue = new ZString(Constants.ShipmentTypes.CoLoadMaster);

			AssertHasCriticalErrorOnProperty(JobShipmentSchema.JS_ShipmentType.Name, originalValue, newValue);
		}

		void AssertHasCriticalErrorOnProperty(string propetyName, IZType originalValue, IZType newValue)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var info = shipment.FindPropertyInfo(propetyName);
			info.Value = originalValue;

			var expectedMessage = string.Format(@"Another user has made critical changes on the {0} that prevent your changes from being saved.
Please close and reopen this form in order to continue. Your changes might be lost.", info.HumanReadableName);

			Factory.Save();
			shipment.Validation.ValidateAll();

			AssertNoError(info, expectedMessage);

			var newFactory = NewFactory();
			var newShipment = newFactory.Load<CommonShipment>(shipment.PK);

			var newInfo = newShipment.FindPropertyInfo(propetyName);
			newInfo.Value = newValue;

			newFactory.Save();
			shipment.Validation.ValidateAll();

			AssertHasError(info, expectedMessage);
		}

		#endregion

		#region CheckJS_ScreeningStatus

		public void TestCheckJS_ScreeningStatus()
		{
			var shipment = GetShipment();
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = string.Empty;
			AssertHasError(shipment.JS_ScreeningStatusInfo, "Please enter a Screening Status.");

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(shipment.JS_ScreeningStatusInfo);

			shipment.JS_ScreeningStatus = "ZZZ";
			AssertHasError(shipment.JS_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		#endregion

		#region Implementation

		protected virtual CommonShipment GetShipment()
		{
			return Factory.New<CommonShipment>();
		}

		#endregion
	}
}
