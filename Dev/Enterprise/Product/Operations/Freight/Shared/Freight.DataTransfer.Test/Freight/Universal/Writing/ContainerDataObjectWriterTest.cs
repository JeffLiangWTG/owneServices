using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class ContainerDataObjectWriterTest : TestCaseWithFactory
	{
		public static void PopulateContainer1(CommonContainer container1, RefCommodityCode containerCommodity, BusinessObjectFactory factory)
			=> PopulateContainer1(container1, containerCommodity, null, factory);

		public static void PopulateContainer1(CommonContainer container1, RefCommodityCode containerCommodity, RefCommodityCode ratingCommodity, BusinessObjectFactory factory)
		{
			container1.JC_AirVentFlow = 12.3m;
			container1.JC_AirVentFlowRateUnit = "MQH"; // Cubic meters per hour
			container1.JC_ArrivalCartageAdvised = new ZDateTime(2011, 1, 1);
			container1.JC_ArrivalCartageComplete = new ZDateTime(2011, 1, 2);
			container1.ArrivalTruckWaitCost = 23.45m;
			container1.ArrivalTruckWaitTime = new ZDateTime(2011, 1, 3, 4, 50, 0);
			container1.JC_ArrivalCartageRef = "ARRCARTREF";
			container1.JC_ArrivalDeliveryRequiredBy = new ZDateTime(2011, 1, 4);
			container1.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 1, 5);
			container1.JC_ArrivalPickupByRail = ZBool.True;
			container1.JC_ArrivalSlotDateTime = new ZDateTime(2011, 1, 6);
			container1.JC_ArrivalSlotReference = "ARRSLOTREF";

			container1.JC_RH_NKContainerCommodityCode = containerCommodity.RH_Code;
			container1.JC_RH_NKRatingCommodityCode = ratingCommodity?.RH_Code ?? ZString.Empty;

			container1.ArrivalCarrierDetentionCost = 34.56m;
			container1.JC_ContainerImportDORelease = "CTRIMPDOR";
			container1.JC_ContainerNum = "OOCL0000027";
			container1.JC_ContainerJobID = "D0001";
			container1.JC_ContainerYardEmptyPickupGateOut = new ZDateTime(2011, 1, 7);
			container1.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2011, 1, 8);
			container1.JC_ContainerQuality = "HID"; // Hides
			container1.JC_ContainerStatus = "INS"; // To Be Inspected

			var containerType = factory.New<RefContainer>();
			containerType.RC_Code = "ZW0W";
			containerType.RC_Description = "Container Type WOW!!";
			containerType.RC_ISOType = "21G5";
			container1.JC_RC = containerType.PK;

			container1.JC_DeliveryMode = "DLVMODE";
			container1.JC_DeliverySequence = 3;
			container1.JC_DepartureCartageAdvised = new ZDateTime(2011, 1, 9);
			container1.JC_DepartureCartageComplete = new ZDateTime(2011, 1, 10);
			container1.DepartureTruckWaitCost = 45.67m;
			container1.DepartureTruckWaitTime = new ZDateTime(2011, 1, 11);
			container1.JC_DepartureCartageRef = "DEPCARTREF";
			container1.JC_DepartureDeliveryByRail = ZBool.False;
			container1.JC_DepartureDockReceipt = "DOCKRECEIPT";
			container1.JC_DepartureEstimatedPickup = new ZDateTime(2011, 1, 12);
			container1.JC_DepartureSlotDateTime = new ZDateTime(2011, 1, 13);
			container1.JC_DepartureSlotReference = "DEPSLOTREF";
			container1.JC_DunnageWeight = 56.78m;
			container1.JC_EmptyReadyForReturn = new ZDateTime(2011, 1, 14);
			container1.JC_EmptyRequired = new ZDateTime(2011, 1, 15);
			container1.JC_EmptyReturnedBy = new ZDateTime(2011, 1, 16);
			container1.JC_EmptyReturnReference = "REEE123";
			container1.JC_ExportDepotCustomsReference = "EXPDEPCUSR";
			container1.JC_ImportDepotCustomsReference = "QWERTYUIOP";
			container1.JC_ContainerMode = Constants.ContainerModes.LCL; // Less Container Load (FCL_LCL_AIR)
			container1.JC_OverrideFCLAvailableStorage = ZBool.True; // Not sure if this should be here at all.
			container1.JC_FCLAvailable = new ZDateTime(2011, 1, 17);
			container1.JC_FCLHeldInTransitStaging = ZBool.True;
			container1.JC_FCLOnBoardVessel = new ZDateTime(2011, 1, 18);
			container1.JC_FCLStorageArrivedUnderbond = ZBool.False;
			container1.ArrivalCTOStorageCost = 67.89m;
			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(2011, 1, 19);
			container1.JC_FCLStorageModuleOnlyMaster = "FCLSTOMAST";
			container1.JC_FCLStorageUnderbondCleared = new ZDateTime(2011, 1, 20);
			container1.JC_FCLUnloadFromVessel = new ZDateTime(2011, 1, 21);
			container1.JC_FCLWharfGateIn = new ZDateTime(2011, 1, 22);
			container1.JC_FCLWharfGateOut = new ZDateTime(2011, 1, 23);
			container1.JC_GrossWeight = 78.90m;
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			container1.JC_IsGrossWeightOverridden = true;
			container1.JC_HumidityPercent = new ZByte(5);
			container1.JC_IsCFSRegistered = ZBool.False;
			container1.JC_IsControlledAtmosphere = ZBool.True;
			container1.JC_IsDamaged = ZBool.True;
			container1.JC_IsEmptyContainer = ZBool.False;
			container1.JC_IsSealOk = ZBool.True;
			container1.JC_IsShipperOwned = ZBool.False;
			container1.JC_OverrideLCLAvailableStorage = ZBool.True; // Not sure if this should be here at all.
			container1.JC_LCLAvailable = new ZDateTime(2011, 1, 24);
			container1.JC_LCLStorageCommences = new ZDateTime(2011, 1, 25);
			container1.JC_LCLUnpack = new ZDateTime(2011, 1, 26);
			container1.JC_PackDate = new ZDateTime(2011, 1, 27);
			container1.JC_PivotBreak = 1101m;
			container1.JC_RefrigGeneratorID = "REFGENID";
			container1.JC_ReleaseNum = "RELNUM";
			container1.JC_SealNum = "SEAL1";
			container1.JC_AdditionalSealNum = "SEAL2";
			container1.JC_Additional2SealNum = "SEAL3";
			container1.JC_SealParty = "CAR";
			container1.JC_AdditionalSealParty = "CRD";
			container1.JC_Additional2SealParty = "CUS";
			container1.JC_SetPointTemp = 89.01m;
			container1.JC_SetPointTempUnit = "F"; // Fahrenheit
			container1.JC_StowagePosition = "AABBCCDD";
			container1.JC_TareWeight = 90.12m;
			container1.JC_TempRecorderSerialNo = "TEMPRECSERNO";
			container1.JC_TotalHeight = 123.45m;
			container1.JC_TotalLength = 234.56m;
			container1.JC_TotalUnitOfMeasure = Constants.Length.Centimetres; // UnitOfLength.Empty;
			container1.JC_TotalWidth = 345.67m;
			container1.JC_TrainWagonNumber = "TRNWAGNO";
			container1.JC_UnpackGang = "UNPKGANG";
			container1.JC_UnpackShed = "UNPKSHED";
			container1.JC_VolumeCapacity = 456.78m;
			container1.JC_VolumeCapacityUQ = Constants.Volume.CubicInches; // UnitOfVolume.Empty;
			container1.JC_WeightCapacity = 567.89m;
			container1.WeightUnitForBinding = Constants.Weight.Pounds; // UnitOfWeight.Empty;
			container1.JC_OverhangBack = 22.22m;
			container1.JC_OverhangRight = 340.67m;
			container1.JC_GoodsValue = 899.44m;
			container1.JC_RX_NKGoodsCurrency = "GBP";

			container1.ArrivalCTOStorageDays = new ZByte(4);
			container1.ArrivalCarrierDetentionDays = new ZByte(2);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 4, 18, 11, 50, 30);

			container1.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "AMS", "AMS0001"));
			container1.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "UCR", "UCR0001"));

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "VGMCO";
			container1.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			var milestone1 = container1.WorkflowItems.AddNew();
			milestone1.P9_Type = "MIL";
			milestone1.P9_Sequence = 1;
			milestone1.P9_Description = "DESC";
			milestone1.TriggerConditions.TriggerEventCode = "ATH";
			milestone1.TriggerConditions.TriggerCondition = "MCR";
			milestone1.TriggerConditions.TriggerConditionValue = "Notes";
			milestone1.SetMilestoneActualDateForTest(ZDateTime.Now.Date.AddDays(-1));
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(ZDateTimeOffset.Now.Date.AddDays(-2)));
			milestone1.P9_IsPublished = true;

			var milestone2 = container1.WorkflowItems.AddNew();
			milestone2.P9_Type = "MIL";
			milestone2.P9_Sequence = 2;
			milestone2.P9_Description = "DESCRIPTION";
			milestone2.TriggerConditions.TriggerEventCode = "AAC";
			milestone2.TriggerConditions.TriggerCondition = "REF";
			milestone2.TriggerConditions.TriggerConditionValue = "Condition Value";
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-3));
			milestone2.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now.AddDays(-4));
			milestone2.P9_IsPublished = false;
		}

		public static void AssertContainer1(Container containerData1, string containerCommodity = "FGFG", string ratingCommodity = "")
		{
			AssertEquals("containerData1.AirVentFlow", 12.3m, containerData1.AirVentFlow);
			AssertEquals("containerData1.AirVentFlowRateUnit.Code", "MQH", containerData1.AirVentFlowRateUnit.Code);
			AssertEquals("containerData1.AirVentFlowRateUnit.Description", "Cubic meters per hour", containerData1.AirVentFlowRateUnit.Description);
			AssertEquals("containerData1.ArrivalCartageAdvised", new ZDateTime(2011, 1, 1), containerData1.ArrivalCartageAdvised);
			AssertEquals("containerData1.ArrivalCartageComplete", new ZDateTime(2011, 1, 2), containerData1.ArrivalCartageComplete);
			AssertEquals("containerData1.ArrivalCartageDemurrageCharge", 23.45m, containerData1.ArrivalCartageDemurrageCharge);
			AssertEquals("containerData1.ArrivalTruckWaitCost", 23.45m, containerData1.ArrivalTruckWaitCost);
			AssertEquals("containerData1.ArrivalCartageDemurrageTime", (ZDateTime)new TimeSpan(2, 4, 50, 0), containerData1.ArrivalCartageDemurrageTime);
			AssertEquals("containerData1.ArrivalTruckWaitTime", (ZDateTime)new TimeSpan(2, 4, 50, 0), containerData1.ArrivalTruckWaitTime);
			AssertEquals("containerData1.ArrivalCartageRef", "ARRCARTREF", containerData1.ArrivalCartageRef);
			AssertEquals("containerData1.ArrivalDeliveryRequiredBy", new ZDateTime(2011, 1, 4), containerData1.ArrivalDeliveryRequiredBy);
			AssertEquals("containerData1.ArrivalEstimatedDelivery", new ZDateTime(2011, 1, 5), containerData1.ArrivalEstimatedDelivery);
			AssertEquals("containerData1.ArrivalPickupByRail", ZBool.True, containerData1.ArrivalPickupByRail);
			AssertEquals("containerData1.ArrivalSlotDateTime", new ZDateTime(2011, 1, 6), containerData1.ArrivalSlotDateTime);
			AssertEquals("containerData1.ArrivalSlotReference", "ARRSLOTREF", containerData1.ArrivalSlotReference);
			AssertEquals("containerData1.Commodity.Code", containerCommodity, containerData1.Commodity.Code);
			AssertEquals("containerData1.RatingCommodity.Code", ratingCommodity, containerData1.RatingCommodity?.Code ?? string.Empty);
			AssertEquals("containerData1.Commodity.Description", "Fudge Guts Fingers Gone", containerData1.Commodity.Description);
			AssertEquals("containerData1.ContainerDetentionCharge", 34.56m, containerData1.ContainerDetentionCharge);
			AssertEquals("containerData1.ArrivalCarrierDetentionCost", 34.56m, containerData1.ArrivalCarrierDetentionCost);
			AssertEquals("containerData1.ContainerDetentionDays", new ZByte(2), containerData1.ContainerDetentionDays);
			AssertEquals("containerData1.ArrivalCarrierDetentionDays", new ZByte(2), containerData1.ArrivalCarrierDetentionDays);
			AssertEquals("containerData1.ContainerImportDORelease", "CTRIMPDOR", containerData1.ContainerImportDORelease);
			AssertEquals("containerData1.ContainerNumber", "OOCL0000027", containerData1.ContainerNumber);
			AssertEquals("containerData1.ContainerJobID", "D0001", containerData1.ContainerJobID);
			AssertEquals("containerData1.ContainerParkEmptyPickupGateOut", new ZDateTime(2011, 1, 7), containerData1.ContainerParkEmptyPickupGateOut);
			AssertEquals("containerData1.ContainerParkEmptyReturnGateIn", new ZDateTime(2011, 1, 8), containerData1.ContainerParkEmptyReturnGateIn);
			AssertEquals("containerData1.ContainerQuality.Code", "HID", containerData1.ContainerQuality.Code);
			AssertEquals("containerData1.ContainerQuality.Description", "Hides", containerData1.ContainerQuality.Description);
			AssertEquals("containerData1.ContainerType.Code", "ZW0W", containerData1.ContainerType.Code);
			AssertEquals("containerData1.ContainerType.Description", "Container Type WOW!!", containerData1.ContainerType.Description);
			AssertEquals("containerData1.ContainerType.ISOCode", "21G5", containerData1.ContainerType.ISOCode);
			AssertEquals("containerData1.ContainerStatus.Code", "INS", containerData1.ContainerStatus.Code);
			AssertEquals("containerData1.ContainerStatus.Description", "To Be Inspected", containerData1.ContainerStatus.Description);
			AssertEquals("containerData1.DeliveryMode", "DLVMODE", containerData1.DeliveryMode);
			AssertEquals("containerData1.DeliverySequence", new ZShort(3), containerData1.DeliverySequence);
			AssertEquals("containerData1.DepartureCartageAdvised", new ZDateTime(2011, 1, 9), containerData1.DepartureCartageAdvised);
			AssertEquals("containerData1.DepartureCartageComplete", new ZDateTime(2011, 1, 10), containerData1.DepartureCartageComplete);
			AssertEquals("containerData1.DepartureCartageDemurrageCharge", 45.67m, containerData1.DepartureCartageDemurrageCharge);
			AssertEquals("containerData1.DepartureTruckWaitCost", 45.67m, containerData1.DepartureTruckWaitCost);
			AssertEquals("containerData1.DepartureCartageDemurrageTime", (ZDateTime)TimeSpan.FromDays(10), containerData1.DepartureCartageDemurrageTime);
			AssertEquals("containerData1.DepartureTruckWaitTime", (ZDateTime)TimeSpan.FromDays(10), containerData1.DepartureTruckWaitTime);
			AssertEquals("containerData1.DepartureCartageRef", "DEPCARTREF", containerData1.DepartureCartageRef);
			AssertEquals("containerData1.DepartureDeliveryByRail", ZBool.False, containerData1.DepartureDeliveryByRail);
			AssertEquals("containerData1.DepartureDockReceipt", "DOCKRECEIPT", containerData1.DepartureDockReceipt);
			AssertEquals("containerData1.DepartureEstimatedPickup", new ZDateTime(2011, 1, 12), containerData1.DepartureEstimatedPickup);
			AssertEquals("containerData1.DepartureSlotDateTime", new ZDateTime(2011, 1, 13), containerData1.DepartureSlotDateTime);
			AssertEquals("containerData1.DepartureSlotReference", "DEPSLOTREF", containerData1.DepartureSlotReference);
			AssertEquals("containerData1.DunnageWeight", 56.78m, containerData1.DunnageWeight);
			AssertEquals("containerData1.EmptyReadyForReturn", new ZDateTime(2011, 1, 14), containerData1.EmptyReadyForReturn);
			AssertEquals("containerData1.EmptyRequired", new ZDateTime(2011, 1, 15), containerData1.EmptyRequired);
			AssertEquals("containerData1.EmptyReturnedBy", new ZDateTime(2011, 1, 16), containerData1.EmptyReturnedBy);
			AssertEquals("containerData1.EmptyReturnReference", "REEE123", containerData1.EmptyReturnRef);
			AssertEquals("containerData1.ExportDepotCustomsReference", "EXPDEPCUSR", containerData1.ExportDepotCustomsReference);
			AssertEquals("containerData1.ImportDepotCustomsReference", "QWERTYUIOP", containerData1.ImportDepotCustomsReference);
			AssertEquals("containerData1.FCL_LCL_AIR.Code", "LCL", containerData1.FCL_LCL_AIR.Code);
			AssertEquals("containerData1.FCL_LCL_AIR.Description", "Less Container Load", containerData1.FCL_LCL_AIR.Description);
			AssertEquals("containerData1.FCLAvailable", new ZDateTime(2011, 1, 17), containerData1.FCLAvailable);
			AssertEquals("containerData1.FCLHeldInTransitStaging", ZBool.True, containerData1.FCLHeldInTransitStaging);
			AssertEquals("containerData1.FCLOnBoardVessel", new ZDateTime(2011, 1, 18), containerData1.FCLOnBoardVessel);
			AssertEquals("containerData1.FCLStorageArrivedUnderbond", ZBool.False, containerData1.FCLStorageArrivedUnderbond);
			AssertEquals("containerData1.FCLStorageCharge", 67.89m, containerData1.FCLStorageCharge);
			AssertEquals("containerData1.ArrivalCTOStorageCost", 67.89m, containerData1.ArrivalCTOStorageCost);
			AssertEquals("containerData1.FCLStorageCommences should not be calculated based on CPY_FreeTime.", new ZDateTime(2011, 1, 19), containerData1.FCLStorageCommences);
			AssertEquals("containerData1.ArrivalCTOStorageStartDate should not be calculated based on CPY_FreeTime.", new ZDateTime(2011, 1, 19), containerData1.ArrivalCTOStorageStartDate);
			AssertEquals("containerData1.FCLStorageDays", new ZByte(4), containerData1.FCLStorageDays);
			AssertEquals("containerData1.ArrivalCTOStorageDays", new ZByte(4), containerData1.ArrivalCTOStorageDays);
			AssertEquals("containerData1.FCLStorageModuleOnlyMaster", "FCLSTOMAST", containerData1.FCLStorageModuleOnlyMaster);
			AssertEquals("containerData1.FCLStorageUnderbondCleared", new ZDateTime(2011, 1, 20), containerData1.FCLStorageUnderbondCleared);
			AssertEquals("containerData1.FCLUnloadFromVessel", new ZDateTime(2011, 1, 21), containerData1.FCLUnloadFromVessel);
			AssertEquals("containerData1.FCLWharfGateIn", new ZDateTime(2011, 1, 22), containerData1.FCLWharfGateIn);
			AssertEquals("containerData1.FCLWharfGateOut", new ZDateTime(2011, 1, 23), containerData1.FCLWharfGateOut);
			AssertEquals("containerData1.HumidityPercent", new ZByte(5), containerData1.HumidityPercent);
			AssertEquals("containerData1.IsCFSRegistered", ZBool.False, containerData1.IsCFSRegistered);
			AssertEquals("containerData1.IsControlledAtmosphere", ZBool.True, containerData1.IsControlledAtmosphere);
			AssertEquals("containerData1.IsDamaged", ZBool.True, containerData1.IsDamaged);
			AssertEquals("containerData1.IsEmptyContainer", ZBool.False, containerData1.IsEmptyContainer);
			AssertEquals("containerData1.IsGrossWeightOverridden", ZBool.True, containerData1.IsGrossWeightOverridden);
			AssertEquals("containerData1.IsSealOk", ZBool.True, containerData1.IsSealOk);
			AssertEquals("containerData1.IsShipperOwned", ZBool.False, containerData1.IsShipperOwned);
			AssertEquals("containerData1.LCLAvailable", new ZDateTime(2011, 1, 24), containerData1.LCLAvailable);
			AssertEquals("containerData1.LCLStorageCommences", new ZDateTime(2011, 1, 25), containerData1.LCLStorageCommences);
			AssertEquals("containerData1.LCLUnpack", new ZDateTime(2011, 1, 26), containerData1.LCLUnpack);
			AssertEquals("containerData1.OverrideFCLAvailableStorage", ZBool.True, containerData1.OverrideFCLAvailableStorage);
			AssertEquals("containerData1.OverrideLCLAvailableStorage", ZBool.True, containerData1.OverrideLCLAvailableStorage);
			AssertEquals("containerData1.PackDate", new ZDateTime(2011, 1, 27), containerData1.PackDate);
			AssertEquals("containerData1.PivotBreak", 1101m, containerData1.PivotBreak);
			AssertEquals("containerData1.RefrigGeneratorID", "REFGENID", containerData1.RefrigGeneratorID);
			AssertEquals("containerData1.ReleaseNum", "RELNUM", containerData1.ReleaseNum);
			AssertEquals("containerData1.Seal", "SEAL1", containerData1.Seal);
			AssertEquals("containerData1.SecondSeal", "SEAL2", containerData1.SecondSeal);
			AssertEquals("containerData1.SealPartyType", "CAR", containerData1.SealPartyType.Code);
			AssertEquals("containerData1.SecondSealPartyType", "CRD", containerData1.SecondSealPartyType.Code);
			AssertEquals("containerData1.SetPointTemp", 89.01m, containerData1.SetPointTemp);
			AssertEquals("containerData1.SetPointTempUnit", "F", containerData1.SetPointTempUnit);
			AssertEquals("containerData1.StowagePosition", "AABBCCDD", containerData1.StowagePosition);
			AssertEquals("containerData1.TareWeight", 90.12m, containerData1.TareWeight);
			AssertEquals("containerData1.TempRecorderSerialNo", "TEMPRECSERNO", containerData1.TempRecorderSerialNo);
			AssertEquals("containerData1.ThirdSeal", "SEAL3", containerData1.ThirdSeal);
			AssertEquals("containerData1.ThirdSealPartyType", "CUS", containerData1.ThirdSealPartyType.Code);
			AssertEquals("containerData1.TotalHeight", 123.45m, containerData1.TotalHeight);
			AssertEquals("containerData1.TotalLength", 234.56m, containerData1.TotalLength);
			AssertEquals("containerData1.LengthUnit.Code", "CM", containerData1.LengthUnit.Code);
			AssertEquals("containerData1.LengthUnit.Description", "Centimeters", containerData1.LengthUnit.Description);
			AssertEquals("containerData1.TotalWidth", 345.67m, containerData1.TotalWidth);
			AssertEquals("containerData1.TrainWagonNumber", "TRNWAGNO", containerData1.TrainWagonNumber);
			AssertEquals("containerData1.UnpackGang", "UNPKGANG", containerData1.UnpackGang);
			AssertEquals("containerData1.UnpackShed", "UNPKSHED", containerData1.UnpackShed);
			AssertEquals("containerData1.VolumeCapacity", 456.78m, containerData1.VolumeCapacity);
			AssertEquals("containerData1.VolumeUnit.Code", "CI", containerData1.VolumeUnit.Code);
			AssertEquals("containerData1.VolumeUnit.Description", "Cubic Inches", containerData1.VolumeUnit.Description);
			AssertEquals("containerData1.WeightCapacity", 567.89m, containerData1.WeightCapacity);
			AssertEquals("containerData1.WeightUnit.Code", "LB", containerData1.WeightUnit.Code);
			AssertEquals("containerData1.WeightUnit.Description", "Pounds", containerData1.WeightUnit.Description);
			AssertEquals(212.34m, containerData1.OverhangFront);
			AssertEquals(5.00m, containerData1.OverhangLeft);
			AssertEquals(123.45m, containerData1.OverhangHeight);
			AssertEquals(22.22m, containerData1.OverhangBack);
			AssertEquals(340.67m, containerData1.OverhangRight);
			AssertEquals("containerData1.GoodsValue", 899.44m, containerData1.GoodsValue);
			AssertEquals("containerData1.GoodsValueCurrency.Code", "GBP", containerData1.GoodsValueCurrency.Code);
			AssertEquals("containerData1.GrossWeightVerificationType", Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, containerData1.GrossWeightVerificationType.Code);
			AssertEquals("containerData1.GrossWeightVerificationDateTime", new ZDateTime(2016, 4, 18, 11, 50, 30), containerData1.GrossWeightVerificationDateTime);
			AssertEquals("containerData1.OrganizationAddressCollection.FirstOrDefault(DocAddressType.GrossWeightVerifiedBy.ToString()).OrganizationCode", "VGMCO", containerData1.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GrossWeightVerifiedBy)).OrganizationCode);

			AssertEquals(1, containerData1.MilestoneCollection.Count);
			AssertEquals(1, containerData1.MilestoneCollection[0].Sequence);
			AssertEquals("DESC", containerData1.MilestoneCollection[0].Description);
			AssertEquals("ATH", containerData1.MilestoneCollection[0].EventCode);
			AssertEquals("MCR", containerData1.MilestoneCollection[0].ConditionType);
			AssertEquals("Notes", containerData1.MilestoneCollection[0].ConditionReference);
			var todaysDateTimeOffsetWithZeroTime = ZDate.Today.ToZDateTime().ToOffset();
			AssertEquals("ActualDate", todaysDateTimeOffsetWithZeroTime.AddDays(-1), containerData1.MilestoneCollection[0].ActualDate);
			AssertEquals("EstimatedDate", todaysDateTimeOffsetWithZeroTime.AddDays(-2), containerData1.MilestoneCollection[0].EstimatedDate);

			var additionalReference1 = containerData1.AdditionalReferenceCollection[0];
			AssertEquals("AMS", additionalReference1.Type.Code);
			AssertEquals("AMS0001", additionalReference1.ReferenceNumber);

			var additionalReference2 = containerData1.AdditionalReferenceCollection[1];
			AssertEquals("UCR", additionalReference2.Type.Code);
			AssertEquals("UCR0001", additionalReference2.ReferenceNumber);

			Func<UniversalDataBuss.DataObjects.Universal.ContainerPenalty, bool> isConsolPenalty = p => Convert.ToString(p.ProcessType.Code) == ContainerPenaltyProcessType.Import || Convert.ToString(p.ProcessType.Code) == ContainerPenaltyProcessType.Export;
			AssertEquals("Total 5 consol penalities", 5, containerData1.ContainerPenaltyCollection.Where(p => isConsolPenalty(p)).Count());

			var penaltyDataObject = containerData1.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime && p.ProcessType.Code.GetValueOrDefault() == "IMP");
			AssertNotNull(penaltyDataObject);
			AssertEquals((ZDateTime)new TimeSpan(2, 4, 50, 0), penaltyDataObject.Duration);
			AssertEquals(23.45m, penaltyDataObject.PerUnitCost);

			penaltyDataObject = containerData1.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage && p.ProcessType.Code.GetValueOrDefault() == "IMP");
			AssertNotNull(penaltyDataObject);
			AssertEquals((ZDateTime)TimeSpan.FromDays(4), penaltyDataObject.Duration);
			AssertEquals(67.89m, penaltyDataObject.PerUnitCost);

			penaltyDataObject = containerData1.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention && p.ProcessType.Code.GetValueOrDefault() == "IMP");
			AssertNotNull(penaltyDataObject);
			AssertEquals((ZDateTime)TimeSpan.FromDays(2), penaltyDataObject.Duration);
			AssertEquals(34.56m, penaltyDataObject.PerUnitCost);

			penaltyDataObject = containerData1.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime && p.ProcessType.Code.GetValueOrDefault() == "EXP");
			AssertNotNull(penaltyDataObject);
			AssertEquals((ZDateTime)TimeSpan.FromDays(10), penaltyDataObject.Duration);
			AssertEquals(45.67m, penaltyDataObject.PerUnitCost);

			penaltyDataObject = containerData1.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention && p.ProcessType.Code.GetValueOrDefault() == "EXP");
			AssertNotNull(penaltyDataObject);
		}

		public static void AssertNotRealContainer(Container containerData1)
		{
			AssertNull("containerData1.AirVentFlow", containerData1.AirVentFlow);
			AssertNull("containerData1.AirVentFlowRateUnit.Code", containerData1.AirVentFlowRateUnit);
			AssertEquals("containerData1.ArrivalCartageAdvised", new ZDateTime(2011, 1, 1), containerData1.ArrivalCartageAdvised);
			AssertEquals("containerData1.ArrivalCartageComplete", new ZDateTime(2011, 1, 2), containerData1.ArrivalCartageComplete);
			AssertEquals("containerData1.ArrivalCartageDemurrageCharge", 23.45m, containerData1.ArrivalCartageDemurrageCharge);
			AssertEquals("containerData1.ArrivalTruckWaitCost", 23.45m, containerData1.ArrivalTruckWaitCost);
			AssertEquals("containerData1.ArrivalCartageDemurrageTime", (ZDateTime)new TimeSpan(2, 4, 50, 0), containerData1.ArrivalCartageDemurrageTime);
			AssertEquals("containerData1.ArrivalTruckWaitTime", (ZDateTime)new TimeSpan(2, 4, 50, 0), containerData1.ArrivalTruckWaitTime);
			AssertEquals("containerData1.ArrivalCartageRef", "ARRCARTREF", containerData1.ArrivalCartageRef);
			AssertEquals("containerData1.ArrivalDeliveryRequiredBy", new ZDateTime(2011, 1, 4), containerData1.ArrivalDeliveryRequiredBy);
			AssertEquals("containerData1.ArrivalEstimatedDelivery", new ZDateTime(2011, 1, 5), containerData1.ArrivalEstimatedDelivery);
			AssertEquals("containerData1.ArrivalPickupByRail", ZBool.True, containerData1.ArrivalPickupByRail);
			AssertEquals("containerData1.ArrivalSlotDateTime", new ZDateTime(2011, 1, 6), containerData1.ArrivalSlotDateTime);
			AssertEquals("containerData1.ArrivalSlotReference", "ARRSLOTREF", containerData1.ArrivalSlotReference);
			AssertEquals("containerData1.Commodity.Code", "FGFG", containerData1.Commodity.Code);
			AssertEquals("containerData1.Commodity.Description", "Fudge Guts Fingers Gone", containerData1.Commodity.Description);
			AssertNull("containerData1.ContainerDetentionCharge", containerData1.ContainerDetentionCharge);
			AssertNull("containerData1.ArrivalCarrierDetentionCost", containerData1.ArrivalCarrierDetentionCost);
			AssertNull("containerData1.ContainerDetentionDays", containerData1.ContainerDetentionDays);
			AssertNull("containerData1.ArrivalCarrierDetentionDays", containerData1.ArrivalCarrierDetentionDays);
			AssertEquals("containerData1.ContainerImportDORelease", "CTRIMPDOR", containerData1.ContainerImportDORelease);
			AssertEquals("containerData1.ContainerNumber", "OOCL0000027", containerData1.ContainerNumber);
			AssertNull("containerData1.ContainerParkEmptyPickupGateOut", containerData1.ContainerParkEmptyPickupGateOut);
			AssertNull("containerData1.ContainerParkEmptyReturnGateIn", containerData1.ContainerParkEmptyReturnGateIn);
			AssertEquals("containerData1.ContainerQuality.Code", "HID", containerData1.ContainerQuality.Code);
			AssertEquals("containerData1.ContainerQuality.Description", "Hides", containerData1.ContainerQuality.Description);
			AssertNull("containerData1.ContainerType.Code", containerData1.ContainerType);
			AssertEquals("containerData1.ContainerStatus.Code", "INS", containerData1.ContainerStatus.Code);
			AssertEquals("containerData1.ContainerStatus.Description", "To Be Inspected", containerData1.ContainerStatus.Description);
			AssertEquals("containerData1.DeliveryMode", "DLVMODE", containerData1.DeliveryMode);
			AssertEquals("containerData1.DeliverySequence", new ZShort(3), containerData1.DeliverySequence);
			AssertEquals("containerData1.DepartureCartageAdvised", new ZDateTime(2011, 1, 9), containerData1.DepartureCartageAdvised);
			AssertEquals("containerData1.DepartureCartageComplete", new ZDateTime(2011, 1, 10), containerData1.DepartureCartageComplete);
			AssertEquals("containerData1.DepartureCartageDemurrageCharge", 45.67m, containerData1.DepartureCartageDemurrageCharge);
			AssertEquals("containerData1.DepartureTruckWaitCost", 45.67m, containerData1.DepartureTruckWaitCost);
			AssertEquals("containerData1.DepartureCartageDemurrageTime", (ZDateTime)TimeSpan.FromDays(10), containerData1.DepartureCartageDemurrageTime);
			AssertEquals("containerData1.DepartureTruckWaitTime", (ZDateTime)TimeSpan.FromDays(10), containerData1.DepartureTruckWaitTime);
			AssertEquals("containerData1.DepartureCartageRef", "DEPCARTREF", containerData1.DepartureCartageRef);
			AssertEquals("containerData1.DepartureDeliveryByRail", ZBool.False, containerData1.DepartureDeliveryByRail);
			AssertEquals("containerData1.DepartureDockReceipt", "DOCKRECEIPT", containerData1.DepartureDockReceipt);
			AssertEquals("containerData1.DepartureEstimatedPickup", new ZDateTime(2011, 1, 12), containerData1.DepartureEstimatedPickup);
			AssertEquals("containerData1.DepartureSlotDateTime", new ZDateTime(2011, 1, 13), containerData1.DepartureSlotDateTime);
			AssertEquals("containerData1.DepartureSlotReference", "DEPSLOTREF", containerData1.DepartureSlotReference);
			AssertEquals("containerData1.DunnageWeight", 56.78m, containerData1.DunnageWeight);
			AssertNull("containerData1.EmptyReadyForReturn", containerData1.EmptyReadyForReturn);
			AssertNull("containerData1.EmptyRequired", containerData1.EmptyRequired);
			AssertNull("containerData1.EmptyReturnedBy", containerData1.EmptyReturnedBy);
			AssertNull("containerData1.EmptyReturnReference", containerData1.EmptyReturnRef);
			AssertEquals("containerData1.ExportDepotCustomsReference", "EXPDEPCUSR", containerData1.ExportDepotCustomsReference);
			AssertEquals("containerData1.ImportDepotCustomsReference", "QWERTYUIOP", containerData1.ImportDepotCustomsReference);
			AssertEquals("containerData1.FCL_LCL_AIR.Code", "BBK", containerData1.FCL_LCL_AIR.Code);
			AssertEquals("containerData1.FCL_LCL_AIR.Description", "Break Bulk", containerData1.FCL_LCL_AIR.Description);
			AssertEquals("containerData1.FCLAvailable", new ZDateTime(2011, 1, 17), containerData1.FCLAvailable);
			AssertEquals("containerData1.FCLHeldInTransitStaging", ZBool.True, containerData1.FCLHeldInTransitStaging);
			AssertEquals("containerData1.FCLOnBoardVessel", new ZDateTime(2011, 1, 18), containerData1.FCLOnBoardVessel);
			AssertEquals("containerData1.FCLStorageArrivedUnderbond", ZBool.False, containerData1.FCLStorageArrivedUnderbond);
			AssertEquals("containerData1.FCLStorageCharge", 67.89m, containerData1.FCLStorageCharge);
			AssertEquals("containerData1.ArrivalCTOStorageCost", 67.89m, containerData1.ArrivalCTOStorageCost);
			AssertEquals("containerData1.FCLStorageCommences", new ZDateTime(2011, 1, 19), containerData1.FCLStorageCommences);
			AssertEquals("containerData1.ArrivalCTOStorageStartDate", new ZDateTime(2011, 1, 19), containerData1.ArrivalCTOStorageStartDate);
			AssertEquals("containerData1.FCLStorageDays", new ZByte(4), containerData1.FCLStorageDays);
			AssertEquals("containerData1.ArrivalCTOStorageDays", new ZByte(4), containerData1.ArrivalCTOStorageDays);
			AssertEquals("containerData1.FCLStorageModuleOnlyMaster", "FCLSTOMAST", containerData1.FCLStorageModuleOnlyMaster);
			AssertEquals("containerData1.FCLStorageUnderbondCleared", new ZDateTime(2011, 1, 20), containerData1.FCLStorageUnderbondCleared);
			AssertEquals("containerData1.FCLUnloadFromVessel", new ZDateTime(2011, 1, 21), containerData1.FCLUnloadFromVessel);
			AssertEquals("containerData1.FCLWharfGateIn", new ZDateTime(2011, 1, 22), containerData1.FCLWharfGateIn);
			AssertEquals("containerData1.FCLWharfGateOut", new ZDateTime(2011, 1, 23), containerData1.FCLWharfGateOut);
			AssertNull("containerData1.HumidityPercent", containerData1.HumidityPercent);
			AssertEquals("containerData1.IsCFSRegistered", ZBool.False, containerData1.IsCFSRegistered);
			AssertNull("containerData1.IsControlledAtmosphere", containerData1.IsControlledAtmosphere);
			AssertEquals("containerData1.IsDamaged", ZBool.True, containerData1.IsDamaged);
			AssertNull("containerData1.IsEmptyContainer", containerData1.IsEmptyContainer);
			AssertEquals("containerData1.IsSealOk", ZBool.True, containerData1.IsSealOk);
			AssertNull("containerData1.IsShipperOwned", containerData1.IsShipperOwned);
			AssertEquals("containerData1.LCLAvailable", new ZDateTime(2011, 1, 24), containerData1.LCLAvailable);
			AssertEquals("containerData1.LCLStorageCommences", new ZDateTime(2011, 1, 25), containerData1.LCLStorageCommences);
			AssertEquals("containerData1.LCLUnpack", new ZDateTime(2011, 1, 26), containerData1.LCLUnpack);
			AssertEquals("containerData1.OverrideFCLAvailableStorage", ZBool.True, containerData1.OverrideFCLAvailableStorage);
			AssertEquals("containerData1.OverrideLCLAvailableStorage", ZBool.True, containerData1.OverrideLCLAvailableStorage);
			AssertEquals("containerData1.PackDate", new ZDateTime(2011, 1, 27), containerData1.PackDate);
			AssertNull("containerData1.RefrigGeneratorID", containerData1.RefrigGeneratorID);
			AssertEquals("containerData1.ReleaseNum", "RELNUM", containerData1.ReleaseNum);
			AssertEquals("containerData1.Seal", "SEAL1", containerData1.Seal);
			AssertEquals("containerData1.SecondSeal", "SEAL2", containerData1.SecondSeal);
			AssertEquals("containerData1.SealPartyType", "CAR", containerData1.SealPartyType.Code);
			AssertEquals("containerData1.SecondSealPartyType", "CRD", containerData1.SecondSealPartyType.Code);
			AssertNull("containerData1.SetPointTemp", containerData1.SetPointTemp);
			AssertNull("containerData1.SetPointTempUnit", containerData1.SetPointTempUnit);
			AssertEquals("containerData1.StowagePosition", "AABBCCDD", containerData1.StowagePosition);
			AssertEquals("containerData1.TareWeight", 90.12m, containerData1.TareWeight);
			AssertNull("containerData1.TempRecorderSerialNo", containerData1.TempRecorderSerialNo);
			AssertEquals("containerData1.ThirdSeal", "SEAL3", containerData1.ThirdSeal);
			AssertEquals("containerData1.ThirdSealPartyType", "CUS", containerData1.ThirdSealPartyType.Code);
			AssertEquals("containerData1.TotalHeight", 123.45m, containerData1.TotalHeight);
			AssertEquals("containerData1.TotalLength", 234.56m, containerData1.TotalLength);
			AssertEquals("containerData1.LengthUnit.Code", "CM", containerData1.LengthUnit.Code);
			AssertEquals("containerData1.LengthUnit.Description", "Centimeters", containerData1.LengthUnit.Description);
			AssertEquals("containerData1.TotalWidth", 345.67m, containerData1.TotalWidth);
			AssertEquals("containerData1.TrainWagonNumber", "TRNWAGNO", containerData1.TrainWagonNumber);
			AssertEquals("containerData1.UnpackGang", "UNPKGANG", containerData1.UnpackGang);
			AssertEquals("containerData1.UnpackShed", "UNPKSHED", containerData1.UnpackShed);
			AssertNull("containerData1.VolumeCapacity", containerData1.VolumeCapacity);
			AssertEquals("containerData1.VolumeUnit.Code", "CI", containerData1.VolumeUnit.Code);
			AssertEquals("containerData1.VolumeUnit.Description", "Cubic Inches", containerData1.VolumeUnit.Description);
			AssertNull("containerData1.WeightCapacity", containerData1.WeightCapacity);
			AssertEquals("containerData1.WeightUnit.Code", "LB", containerData1.WeightUnit.Code);
			AssertEquals("containerData1.WeightUnit.Description", "Pounds", containerData1.WeightUnit.Description);
			AssertNull("containerData1.OverhangFront", containerData1.OverhangFront);
			AssertNull("containerData1.OverhangLeft", containerData1.OverhangLeft);
			AssertNull("containerData1.OverhangHeight", containerData1.OverhangHeight);
			AssertNull("containerData1.OverhangBack", containerData1.OverhangBack);
			AssertNull("containerData1.OverhangRight", containerData1.OverhangRight);
		}

		public void TestPopulateContainerDataObject()
		{
			var consolBO = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as CommonConsol;
			var containerBO = Factory.New<Enterprise.Integration.Forwarding.IForwardingContainer>() as CommonContainer;
			consolBO.JK_RL_NKDischargePort = "NZAKL";
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_TransportMode = "SEA";
			consolBO.Containers.Add(containerBO);
			containerBO.JC_ContainerCount = 2;

			var containerCommodity = Factory.New<RefCommodityCode>();
			containerCommodity.RH_Code = "FGFG";
			containerCommodity.RH_Description = "Fudge Guts Fingers Gone";
			containerCommodity.RH_IsShipping = true;
			containerCommodity.RH_IsForwarding = true;
			containerCommodity.RH_IsLandTransport = false;

			var ratingCommodity = Factory.New<RefCommodityCode>();
			ratingCommodity.RH_Code = "XYXY";
			ratingCommodity.RH_Description = "xyxy";
			ratingCommodity.RH_IsShipping = true;
			ratingCommodity.RH_IsForwarding = true;
			ratingCommodity.RH_IsLandTransport = false;

			var unlimitedFreeDaysOptions = new Registry.Business.ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				PopulateContainer1(containerBO, containerCommodity, ratingCommodity, Factory);
			}

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerDataObject = writer.GetDataObject(containerBO);
			AssertContainer1(containerDataObject, "FGFG", "XYXY");
			AssertEquals("containerData1.ContainerCount", 2, containerDataObject.ContainerCount);
		}

		public void TestNonOperatingReefer()
		{
			var consolBO = Factory.New<CommonConsol>();
			var containerBO = consolBO.Containers.AddNew();
			var containerType = Factory.New<RefContainer>();
			containerBO.JC_RC = containerType.PK;
			containerBO.JC_IsNonOperativeReefer = true;

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerDataObject = writer.GetDataObject(containerBO);

			AssertEquals(containerDataObject.NonOperatingReefer, true);
			AssertEquals(containerDataObject.ContainerType.Category.Code, Constants.ContainerTypes.DryStorage);
			AssertEquals(containerDataObject.ContainerType.Category.Description, Constants.ContainerTypeDescriptions.DryStorage);

			consolBO = Factory.New<CommonConsol>();
			containerBO = consolBO.Containers.AddNew();
			containerBO.JC_IsNonOperativeReefer = false;
			containerType = Factory.New<RefContainer>();
			containerType.RC_ISOType = "22R1";
			containerType.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			containerBO.JC_RC = containerType.PK;

			writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			containerDataObject = writer.GetDataObject(containerBO);

			AssertEquals(containerDataObject.NonOperatingReefer, true);
			AssertEquals(containerDataObject.ContainerType.Category.Code, Constants.ContainerTypes.DryStorage);
			AssertEquals(containerDataObject.ContainerType.Category.Description, Constants.ContainerTypeDescriptions.DryStorage);

			consolBO = Factory.New<CommonConsol>();
			containerBO = consolBO.Containers.AddNew();
			containerBO.JC_IsNonOperativeReefer = false;
			containerType = Factory.New<RefContainer>();
			containerType.RC_ISOType = "40GP";
			containerType.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			containerBO.JC_RC = containerType.PK;

			writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			containerDataObject = writer.GetDataObject(containerBO);

			AssertEquals(containerDataObject.NonOperatingReefer, false);
		}

		public void TestPopulateContainerPenaltyCollection()
		{
			var consolBO = Factory.New<CommonConsol>();
			var containerBO = consolBO.Containers.AddNew();

			containerBO.ArrivalTruckWaitTime = new ZDateTime(2011, 1, 3, 4, 50, 0);
			containerBO.ArrivalTruckWaitCost = 23.45m;

			var penalty = containerBO.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_RL_NKLocation = "AUSYD";
			penalty.CPY_FreeTime = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			penalty.CPY_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 3);
			penalty.CPY_TimeUnit = Core.Constants.ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_PerUnitCost = 3.2m;
			penalty.CPY_TotalCost = 34.56m;
			penalty.CPY_RX_NKCurrency = "AUD";
			penalty.CPY_FirstFreeDay = new ZDateTimeOffset(ZDateTime.Now.Year, 1, 3);

			penalty = containerBO.ExportPenalties.AddNew();
			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_RL_NKLocation = "AUSYD";
			penalty.CPY_FreeTime = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			penalty.CPY_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 3);
			penalty.CPY_TimeUnit = Core.Constants.ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_PerUnitCost = 3.2m;
			penalty.CPY_TotalCost = 34.56m;
			penalty.CPY_RX_NKCurrency = "AUD";
			penalty.CPY_FirstFreeDay = new ZDateTimeOffset(ZDateTime.Now.Year, 1, 3);

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerDataObject = writer.GetDataObject(containerBO);

			AssertEquals(3, containerDataObject.ContainerPenaltyCollection.Count);

			var penaltyDataObject = containerDataObject.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime);
			AssertNotNull(penaltyDataObject);
			AssertEquals((ZDateTime)new TimeSpan(2, 4, 50, 0), penaltyDataObject.Duration);
			AssertEquals(23.45m, penaltyDataObject.PerUnitCost);

			penaltyDataObject = containerDataObject.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention && p.ProcessType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyProcessType.Import);
			AssertNotNull(penaltyDataObject);
			AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penaltyDataObject.CreditorType.Code);
			AssertNull(penaltyDataObject.Creditor);
			AssertEquals("AUSYD", penaltyDataObject.Location.Code);
			AssertEquals(ZDateTime.Empty, penaltyDataObject.FreeTime);
			AssertEquals((ZByte)0, penaltyDataObject.FreeTimeAmount);
			AssertEquals((ZDateTime)TimeSpan.FromDays(2), penaltyDataObject.Duration);
			AssertEquals((ZByte)2, penaltyDataObject.DurationAmount);
			AssertEquals(TimeUnit.Days, penaltyDataObject.TimeUnit);
			AssertEquals(3.2m, penaltyDataObject.PerUnitCost);
			AssertEquals(34.56m, penaltyDataObject.TotalCost);
			AssertEquals(0m, penaltyDataObject.PerUnitSell);
			AssertEquals(0m, penaltyDataObject.TotalSell);
			AssertEquals("AUD", penaltyDataObject.Currency.Code);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 3), penaltyDataObject.FirstFreeDay);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 2), penaltyDataObject.LastFreeDay);

			penaltyDataObject = containerDataObject.ContainerPenaltyCollection.FirstOrDefault(p => p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention && p.ProcessType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyProcessType.Export);
			AssertNotNull(penaltyDataObject);
			AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penaltyDataObject.CreditorType.Code);
			AssertNull(penaltyDataObject.Creditor);
			AssertEquals("AUSYD", penaltyDataObject.Location.Code);
			AssertEquals(ZDateTime.Empty, penaltyDataObject.FreeTime);
			AssertEquals((ZByte)0, penaltyDataObject.FreeTimeAmount);
			AssertEquals((ZDateTime)TimeSpan.FromDays(2), penaltyDataObject.Duration);
			AssertEquals((ZByte)2, penaltyDataObject.DurationAmount);
			AssertEquals(TimeUnit.Days, penaltyDataObject.TimeUnit);
			AssertEquals(3.2m, penaltyDataObject.PerUnitCost);
			AssertEquals(34.56m, penaltyDataObject.TotalCost);
			AssertEquals(0m, penaltyDataObject.PerUnitSell);
			AssertEquals(0m, penaltyDataObject.TotalSell);
			AssertEquals("AUD", penaltyDataObject.Currency.Code);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 3), penaltyDataObject.FirstFreeDay);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 2), penaltyDataObject.LastFreeDay);

			AssertEquals((ZDateTime)new TimeSpan(2, 4, 50, 0), containerDataObject.ArrivalTruckWaitTime);
			AssertEquals((ZDateTime)new TimeSpan(2, 4, 50, 0), containerDataObject.ArrivalCartageDemurrageTime);
			AssertEquals(23.45m, containerDataObject.ArrivalTruckWaitCost);
			AssertEquals(23.45m, containerDataObject.ArrivalCartageDemurrageCharge);
			AssertNull(containerDataObject.FCLStorageDays);
			AssertNull(containerDataObject.ArrivalCTOStorageDays);
			AssertNull(containerDataObject.FCLStorageCharge);
			AssertNull(containerDataObject.ArrivalCTOStorageCost);
			AssertEquals((ZByte)2, containerDataObject.ContainerDetentionDays);
			AssertEquals((ZByte)2, containerDataObject.ArrivalCarrierDetentionDays);
			AssertEquals(3.2m, containerDataObject.ContainerDetentionCharge);
			AssertEquals(3.2m, containerDataObject.ArrivalCarrierDetentionCost);
		}

		public void TestPopulateShipmentContainerPenaltyCollection()
		{
			var consolBO = Factory.New<CommonConsol>();
			var containerBO = consolBO.Containers.AddNew();

			var penalty1 = containerBO.DeliveryPenalties.AddNew();
			penalty1.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty1.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty1.CPY_RL_NKLocation = "AUSYD";
			penalty1.CPY_FreeTime = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			penalty1.CPY_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 3);
			penalty1.CPY_TimeUnit = Core.Constants.ContainerPenaltyTimeUnit.Codes.Days;
			penalty1.CPY_PerUnitCost = 3.2m;
			penalty1.CPY_TotalCost = 34.56m;
			penalty1.CPY_RX_NKCurrency = "AUD";
			penalty1.CPY_FirstFreeDay = new ZDateTimeOffset(ZDateTime.Now.Year, 1, 3);

			var penalty2 = containerBO.PickupPenalties.AddNew();
			penalty2.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty2.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty2.CPY_RL_NKLocation = "AUSYD";
			penalty2.CPY_FreeTime = new ZDateTime(ZDateTime.Now.Year, 1, 2);
			penalty2.CPY_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 4);
			penalty2.CPY_TimeUnit = Core.Constants.ContainerPenaltyTimeUnit.Codes.Days;
			penalty2.CPY_PerUnitCost = 4.2m;
			penalty2.CPY_TotalCost = 44.56m;
			penalty2.CPY_RX_NKCurrency = "AUD";
			penalty2.CPY_FirstFreeDay = new ZDateTimeOffset(ZDateTime.Now.Year, 1, 3);

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerDataObject = writer.GetDataObject(containerBO);

			AssertEquals(2, containerDataObject.ContainerPenaltyCollection.Count);

			var penaltyDataObject1 = containerDataObject.ContainerPenaltyCollection.FirstOrDefault(p => p.ProcessType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyProcessType.Delivery && p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertNotNull(penaltyDataObject1);
			AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penaltyDataObject1.CreditorType.Code);
			AssertNull(penaltyDataObject1.Creditor);
			AssertEquals("AUSYD", penaltyDataObject1.Location.Code);
			AssertEquals(ZDateTime.Empty, penaltyDataObject1.FreeTime);
			AssertEquals((ZByte)0, penaltyDataObject1.FreeTimeAmount);
			AssertEquals((ZDateTime)TimeSpan.FromDays(2), penaltyDataObject1.Duration);
			AssertEquals((ZByte)2, penaltyDataObject1.DurationAmount);
			AssertEquals(TimeUnit.Days, penaltyDataObject1.TimeUnit);
			AssertEquals(3.2m, penaltyDataObject1.PerUnitCost);
			AssertEquals(34.56m, penaltyDataObject1.TotalCost);
			AssertEquals(3.2m, penaltyDataObject1.PerUnitSell);
			AssertEquals(34.56m, penaltyDataObject1.TotalSell);
			AssertEquals("AUD", penaltyDataObject1.Currency.Code);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 3), penaltyDataObject1.FirstFreeDay);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 2), penaltyDataObject1.LastFreeDay);

			var penaltyDataObject2 = containerDataObject.ContainerPenaltyCollection.FirstOrDefault(p => p.ProcessType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyProcessType.Pickup && p.PenaltyType.Code.GetValueOrDefault() == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertNotNull(penaltyDataObject2);
			AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penaltyDataObject2.CreditorType.Code);
			AssertNull(penaltyDataObject2.Creditor);
			AssertEquals("AUSYD", penaltyDataObject2.Location.Code);
			AssertEquals((ZDateTime)TimeSpan.FromDays(1), penaltyDataObject2.FreeTime);
			AssertEquals((ZByte)1, penaltyDataObject2.FreeTimeAmount);
			AssertEquals((ZDateTime)TimeSpan.FromDays(3), penaltyDataObject2.Duration);
			AssertEquals((ZByte)3, penaltyDataObject2.DurationAmount);
			AssertEquals(TimeUnit.Days, penaltyDataObject2.TimeUnit);
			AssertEquals(4.2m, penaltyDataObject2.PerUnitCost);
			AssertEquals(44.56m, penaltyDataObject2.TotalCost);
			AssertEquals(4.2m, penaltyDataObject2.PerUnitSell);
			AssertEquals(44.56m, penaltyDataObject2.TotalSell);
			AssertEquals("AUD", penaltyDataObject2.Currency.Code);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 3), penaltyDataObject2.FirstFreeDay);
			AssertEquals(new ZDateTime(ZDateTime.Now.Year, 1, 3), penaltyDataObject2.LastFreeDay);
		}

		public void TestPopulateNotRealContainerDataObject()
		{
			var consolBO = Factory.New<CommonConsol>();
			var containerBO = consolBO.Containers.AddNew();

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "FGFG";
			commodity.RH_Description = "Fudge Guts Fingers Gone";
			commodity.RH_IsShipping = true;
			commodity.RH_IsForwarding = true;
			commodity.RH_IsLandTransport = false;

			PopulateContainer1(containerBO, commodity, Factory);
			containerBO.JC_ContainerMode = Constants.ContainerModes.BreakBulk;

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerDataObject = writer.GetDataObject(containerBO);
			AssertNotRealContainer(containerDataObject);
		}

		public void TestContainerAdditionalServicesMappings()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			var consolBO = shipmentBO.Consols.AddNew();
			var containerBO = consolBO.Containers.AddNew();
			var additionalService = containerBO.Services.AddNew();

			containerBO.Factory.Save();

			additionalService.ES_ServiceCode = "FUM";
			additionalService.ES_Booked = new ZDateTime(2011, 6, 1);
			additionalService.ES_Completed = new ZDateTime(2011, 6, 2);
			additionalService.ES_OH_Contractor = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(containerBO.Factory).PK;

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerData = writer.GetDataObject(containerBO);
			AssertNotNull("Precondition: shipmentData", containerData);

			var service = containerData.AdditionalServiceCollection[0];
			AssertEquals("service.Booked", new ZDateTime(2011, 6, 1), service.Booked);
			AssertEquals("service.Completed", new ZDateTime(2011, 6, 2), service.Completed);
			AssertEquals("service.ServiceCode", "FUM", service.ServiceCode.Code);
			OrganizationAddressTestHelper.AssertOrganizationBO_INTHEMSYD("service.Contractor", service.Contractor, "Contractor");
		}

		public void TestContainerYards()
		{
			var departureContainerYard = Factory.New<OrgHeader>();
			departureContainerYard.OH_Code = "DEP YARD";
			departureContainerYard.MainAddress.OA_Address1 = "12 DEPARTURE RD";
			var arrivalContainerYard = Factory.New<OrgHeader>();
			arrivalContainerYard.OH_Code = "ARV YARD";
			arrivalContainerYard.MainAddress.OA_Address1 = "24 ARRIVAL RD";

			var containerBO = Factory.New<CommonContainer>();
			containerBO.JC_OA_DepartureContainerYardAddress = departureContainerYard.MainAddress.PK;
			containerBO.JC_OA_ArrivalContainerYardAddress = arrivalContainerYard.MainAddress.PK;

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerData = writer.GetDataObject(containerBO);

			var departureAddress = containerData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyPickupAddress));
			var arrivalAddress = containerData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyReturnAddress));

			AssertEquals("DEP YARD", departureAddress.OrganizationCode);
			AssertEquals("12 DEPARTURE RD", departureAddress.Address1);
			AssertEquals("ARV YARD", arrivalAddress.OrganizationCode);
			AssertEquals("24 ARRIVAL RD", arrivalAddress.Address1);
		}

		public void TestContainerAdditionalAddressInfo()
		{
			var factory = Factory;
			var containerBO = factory.New<CommonContainer>();

			containerBO.JC_OA_DepartureContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			containerBO.JC_OA_ArrivalContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			containerBO.EmptyPickupByTransportMode = "ROA";
			containerBO.EmptyReturnToTransportMode = "RAI";

			var writer = new ContainerDataObjectWriter(BindToLists.GetCachedLists(factory), new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerBO)));
			var containerData = writer.GetDataObject(containerBO);

			AssertNotNull("containerData.AdditionalAddressInfoCollection should not be null", containerData.AdditionalAddressInfoCollection);
			AssertEquals("containerData.AdditionalAddressInfoCollection.Count", 2, containerData.AdditionalAddressInfoCollection.Count);

			var additionalAddressInfo1 = containerData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.DepartureCYDAddress));
			AssertNotNull("additionalAddressInfo1 should not be null", additionalAddressInfo1);
			AssertEquals("additionalAddressInfo1.TransportMode.Code", "ROA", additionalAddressInfo1.TransportMode.Code);

			var additionalAddressInfo2 = containerData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.ArrivalCYDAddress));
			AssertNotNull("additionalAddressInfo2 should not be null", additionalAddressInfo2);
			AssertEquals("additionalAddressInfo2.TransportMode.Code", "RAI", additionalAddressInfo2.TransportMode.Code);
		}
	}
}
