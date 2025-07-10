using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class ContainerDataObjectTestHelper : TestCase
	{
		public static Container SetupContainerWithVGM(UniversalObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new UniversalObjectFactory();
			}

			var container = SetupContainer(factory);
			var verifiedByOrg = factory.NewWithValidTestData<OrgHeader>();
			verifiedByOrg.OH_FullName = "VGM name";
			var verifiedByOrgAddress = verifiedByOrg.MainAddress;
			verifiedByOrgAddress.OA_Code = "VGM789";

			factory.SaveForTesting();

			container.GrossWeightVerificationType = new CodeDescriptionPair { Code = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, Description = Core.Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container };
			container.GrossWeightVerificationDateTime = new ZDateTime(2016, 04, 14, 10, 05, 30);
			container.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = verifiedByOrg.OH_Code, CompanyName = "VGM name", AddressType = nameof(DocAddressType.GrossWeightVerifiedBy) });

			return container;
		}
		public static Container SetupContainer(UniversalObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new UniversalObjectFactory();
			}

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var orgAddressData = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD("FOO");
			var orgAddress = new OrganisationDataObjectReader(orgAddressData, new TestErrorLogger(), factory).GetMatchedOrNewForTesting();

			var departureOrg = factory.NewWithValidTestData<OrgHeader>();
			var departureAddress = departureOrg.MainAddress;
			departureAddress.OA_Code = "DEP123";
			var arrivalOrg = factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = arrivalOrg.MainAddress;
			arrivalAddress.OA_Code = "ARV456";

			factory.SaveForTesting();

			containerDataObject.AirVentFlow = 12.34m;
			containerDataObject.AirVentFlowRateUnit = new CodeDescriptionPair() { Code = "MQH", Description = "Cubic meters per hour" };
			containerDataObject.ArrivalCartageAdvised = new ZDateTime(2011, 1, 1);
			containerDataObject.ArrivalCartageComplete = new ZDateTime(2011, 1, 2);
			containerDataObject.ArrivalCartageDemurrageCharge = 23.45m;
			containerDataObject.ArrivalCartageDemurrageTime = new ZDateTime(2011, 1, 3);
			containerDataObject.ArrivalCartageRef = "ARRCARTREF";
			containerDataObject.ArrivalDeliveryRequiredBy = new ZDateTime(2011, 1, 4);
			containerDataObject.ArrivalEstimatedDelivery = new ZDateTime(2011, 1, 5);
			containerDataObject.ArrivalPickupByRail = ZBool.True;
			containerDataObject.ArrivalSlotDateTime = new ZDateTime(2011, 1, 6);
			containerDataObject.ArrivalSlotReference = "ARRSLOTREF";
			containerDataObject.Commodity = new Commodity() { Code = "FGFG", Description = "Fudge Guts Fingers Gone" };
			containerDataObject.ContainerCount = 2;
			containerDataObject.ContainerDetentionCharge = 34.56m;
			containerDataObject.ContainerDetentionDays = new ZByte(2);
			containerDataObject.ContainerImportDORelease = "CTRIMPDOR";
			containerDataObject.ContainerNumber = "OOCL0000027";
			containerDataObject.ContainerParkEmptyPickupGateOut = new ZDateTime(2011, 1, 7);
			containerDataObject.ContainerParkEmptyReturnGateIn = new ZDateTime(2011, 1, 8);
			containerDataObject.ContainerQuality = new CodeDescriptionPair() { Code = "HID", Description = "Hides" };
			containerDataObject.ContainerType = new ContainerType() { Code = "ZW0W", Description = "Container Type WOW!!", ISOCode = "21G5" };
			containerDataObject.ContainerStatus = new CodeDescriptionPair() { Code = "INS", Description = "To Be Inspected" };
			containerDataObject.DeliveryMode = "DLVMODE";
			containerDataObject.DeliverySequence = 3;
			containerDataObject.DepartureCartageAdvised = new ZDateTime(2011, 1, 9);
			containerDataObject.DepartureCartageComplete = new ZDateTime(2011, 1, 10);
			containerDataObject.DepartureCartageDemurrageCharge = 45.67m;
			containerDataObject.DepartureCartageDemurrageTime = new ZDateTime(2011, 1, 11);
			containerDataObject.DepartureCartageRef = "DEPCARTREF";
			containerDataObject.DepartureDeliveryByRail = ZBool.False;
			containerDataObject.DepartureDockReceipt = "DOCKRECEIPT";
			containerDataObject.DepartureEstimatedPickup = new ZDateTime(2011, 1, 12);
			containerDataObject.DepartureSlotDateTime = new ZDateTime(2011, 1, 13);
			containerDataObject.DepartureSlotReference = "DEPSLOTREF";
			containerDataObject.DunnageWeight = 56.78m;
			containerDataObject.EmptyReadyForReturn = new ZDateTime(2011, 1, 14);
			containerDataObject.EmptyRequired = new ZDateTime(2011, 1, 15);
			containerDataObject.EmptyReturnedBy = new ZDateTime(2011, 1, 16);
			containerDataObject.EmptyReturnRef = "REEE123";
			containerDataObject.ExportDepotCustomsReference = "EXPDEPCUSR";
			containerDataObject.FCL_LCL_AIR = new ContainerMode() { Code = "LCL", Description = "Less Container Load" };
			containerDataObject.FCLAvailable = new ZDateTime(2011, 1, 17);
			containerDataObject.FCLHeldInTransitStaging = ZBool.True;
			containerDataObject.FCLOnBoardVessel = new ZDateTime(2011, 1, 18);
			containerDataObject.FCLStorageArrivedUnderbond = ZBool.False;
			containerDataObject.FCLStorageCharge = 67.89m;
			containerDataObject.FCLStorageCommences = new ZDateTime(2011, 1, 19);
			containerDataObject.FCLStorageDays = new ZByte(4);
			containerDataObject.FCLStorageModuleOnlyMaster = "FCLSTOMAST";
			containerDataObject.FCLStorageUnderbondCleared = new ZDateTime(2011, 1, 20);
			containerDataObject.FCLUnloadFromVessel = new ZDateTime(2011, 1, 21);
			containerDataObject.FCLWharfGateIn = new ZDateTime(2011, 1, 22);
			containerDataObject.FCLWharfGateOut = new ZDateTime(2011, 1, 23);
			containerDataObject.GrossWeight = 78.90m;
			containerDataObject.GoodsValue = 454.66m;
			containerDataObject.GoodsValueCurrency = new Currency() { Code = "EUR", Description = "Euro" };
			containerDataObject.GoodsWeight = 72.998m;
			containerDataObject.HumidityPercent = new ZByte(5);
			containerDataObject.IsCFSRegistered = ZBool.False;
			containerDataObject.IsControlledAtmosphere = ZBool.True;
			containerDataObject.IsDamaged = ZBool.True;
			containerDataObject.IsEmptyContainer = ZBool.False;
			containerDataObject.IsSealOk = ZBool.True;
			containerDataObject.IsShipperOwned = ZBool.False;
			containerDataObject.LCLAvailable = new ZDateTime(2011, 1, 24);
			containerDataObject.LCLStorageCommences = new ZDateTime(2011, 1, 25);
			containerDataObject.LCLUnpack = new ZDateTime(2011, 1, 26);
			containerDataObject.OverrideFCLAvailableStorage = ZBool.True;
			containerDataObject.OverrideLCLAvailableStorage = ZBool.True;
			containerDataObject.OverhangBack = 22.22m;
			containerDataObject.OverhangRight = 340.67m;
			containerDataObject.PackDate = new ZDateTime(2011, 1, 27);
			containerDataObject.RefrigGeneratorID = "REFGENID";
			containerDataObject.ReleaseNum = "RELNUM";
			containerDataObject.Seal = "SEAL1";
			containerDataObject.SecondSeal = "SEAL2";
			containerDataObject.SealPartyType = new CodeDescriptionPair { Code = "CAR", Description = "CAR Carrier/Shipping Line" };
			containerDataObject.SecondSealPartyType = new CodeDescriptionPair { Code = "CRD", Description = "Consignor/Shipper" };
			containerDataObject.SetPointTemp = 89.01m;
			containerDataObject.SetPointTempUnit = "F";
			containerDataObject.StowagePosition = "AABBCCDD";
			containerDataObject.TareWeight = 90.12m;
			containerDataObject.TempRecorderSerialNo = "TEMPRECSERNO";
			containerDataObject.ThirdSeal = "SEAL3";
			containerDataObject.ThirdSealPartyType = new CodeDescriptionPair { Code = "CUS", Description = "Terminal" };
			containerDataObject.TotalHeight = 123.45m;
			containerDataObject.TotalLength = 234.56m;
			containerDataObject.LengthUnit = new UnitOfLength() { Code = "CM", Description = "Centimeters" };
			containerDataObject.TotalWidth = 345.67m;
			containerDataObject.TrainWagonNumber = "TRNWAGNO";
			containerDataObject.UnpackGang = "UNPKGANG";
			containerDataObject.UnpackShed = "UNPKSHED";
			containerDataObject.VolumeCapacity = 456.78m;
			containerDataObject.VolumeUnit = new UnitOfVolume() { Code = "CI", Description = "Cubic Inches" };
			containerDataObject.WeightCapacity = 567.89m;
			containerDataObject.WeightUnit = new UnitOfWeight() { Code = "LB", Description = "Pounds" };

			var writeManager = new DataWritingManager(new ActionInfo(null, factory.New<DummyBusinessObject>()));
			containerDataObject.AddOrgAddress(writeManager, departureAddress, nameof(DocAddressType.ContainerYardEmptyPickupAddress));
			containerDataObject.AddOrgAddress(writeManager, arrivalAddress, nameof(DocAddressType.ContainerYardEmptyReturnAddress));

			var additionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = "FUM", Description = "Fumigation" },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = orgAddressData,
			};

			containerDataObject.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			containerDataObject.AdditionalServiceCollection.Add(additionalService);

			containerDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			containerDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = "AMS", Description = "AMS Number" }, ReferenceNumber = "AMS0001" });
			containerDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = "UBR", Description = "Under Bond Approval Reference Number" }, ReferenceNumber = "UBR0001" });

			return containerDataObject;
		}

		public static void AssertContents(CommonContainer containerBO)
		{
			AssertEquals("containerBO.JC_AirVentFlow", 12.3m, containerBO.JC_AirVentFlow);
			AssertEquals("containerBO.JC_AirVentFlowRateUnit", "MQH", containerBO.JC_AirVentFlowRateUnit);
			AssertEquals("containerBO.JC_ArrivalCartageAdvised", new ZDateTime(2011, 1, 1), containerBO.JC_ArrivalCartageAdvised);
			AssertEquals("containerBO.JC_ArrivalCartageComplete", new ZDateTime(2011, 1, 2), containerBO.JC_ArrivalCartageComplete);
			AssertEquals("containerBO.ArrivalTruckWaitCost", 23.45m, containerBO.ArrivalTruckWaitCost);
			AssertEquals("containerBO.ArrivalTruckWaitTime", (ZDateTime)TimeSpan.FromDays(2), containerBO.ArrivalTruckWaitTime);
			AssertEquals("containerBO.JC_ArrivalCartageRef", "ARRCARTREF", containerBO.JC_ArrivalCartageRef);
			AssertEquals("containerBO.JC_ArrivalDeliveryRequiredBy", new ZDateTime(2011, 1, 4), containerBO.JC_ArrivalDeliveryRequiredBy);
			AssertEquals("containerBO.JC_ArrivalEstimatedDelivery", new ZDateTime(2011, 1, 5), containerBO.JC_ArrivalEstimatedDelivery);
			AssertEquals("containerBO.JC_ArrivalPickupByRail", ZBool.True, containerBO.JC_ArrivalPickupByRail);
			AssertEquals("containerBO.JC_ArrivalSlotDateTime", new ZDateTime(2011, 1, 6), containerBO.JC_ArrivalSlotDateTime);
			AssertEquals("containerBO.JC_ArrivalSlotReference", "ARRSLOTREF", containerBO.JC_ArrivalSlotReference);

			AssertEquals("containerBO.JC_RH_NKContainerCommodityCode", "FGFG", containerBO.JC_RH_NKContainerCommodityCode);

			AssertEquals("containerBO.JC_ContainerCount", new ZShort(2), containerBO.JC_ContainerCount);
			AssertEquals("containerBO.ArrivalCarrierDetentionCost", 34.56m, containerBO.ArrivalCarrierDetentionCost);
			AssertEquals("containerBO.ArrivalCarrierDetentionDays", new ZByte(2), containerBO.ArrivalCarrierDetentionDays);
			AssertEquals("containerBO.JC_ContainerImportDORelease", "CTRIMPDOR", containerBO.JC_ContainerImportDORelease);
			AssertEquals("containerBO.JC_ContainerNum", "OOCL0000027", containerBO.JC_ContainerNum);
			AssertEquals("containerBO.JC_ContainerYardEmptyPickupGateOut", new ZDateTime(2011, 1, 7), containerBO.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("containerBO.JC_ContainerYardEmptyReturnGateIn", new ZDateTime(2011, 1, 8), containerBO.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("containerBO.JC_ContainerQuality", "HID", containerBO.JC_ContainerQuality);
			AssertEquals("containerBO.JC_ContainerStatus", "INS", containerBO.JC_ContainerStatus);

			if (containerBO.RefContainer != null)
			{
				AssertEquals("containerBO.RefContainer.RC_Code", "ZW0W", containerBO.RefContainer.RC_Code);
				AssertEquals("containerBO.RefContainer.RC_Description", "Container Type WOW!!", containerBO.RefContainer.RC_DescriptionMultilingual);
				AssertEquals("containerBO.RefContainer.RC_ISOType", "21G5", containerBO.RefContainer.RC_ISOType);
			}

			if (containerBO.RefContainer != null)
			{
				AssertEquals("containerBO.JC_RC", containerBO.RefContainer.PK, containerBO.JC_RC);
			}

			AssertEquals("containerBO.JC_DeliveryMode", "DLVMODE", containerBO.JC_DeliveryMode);
			AssertEquals("containerBO.JC_DeliverySequence", new ZShort(3), containerBO.JC_DeliverySequence);
			AssertEquals("containerBO.JC_DepartureCartageAdvised", new ZDateTime(2011, 1, 9), containerBO.JC_DepartureCartageAdvised);
			AssertEquals("containerBO.JC_DepartureCartageComplete", new ZDateTime(2011, 1, 10), containerBO.JC_DepartureCartageComplete);
			AssertEquals("containerBO.DepartureTruckWaitCost", 45.67m, containerBO.DepartureTruckWaitCost);
			AssertEquals("containerBO.DepartureTruckWaitTime", (ZDateTime)TimeSpan.FromDays(10), containerBO.DepartureTruckWaitTime);
			AssertEquals("containerBO.JC_DepartureCartageRef", "DEPCARTREF", containerBO.JC_DepartureCartageRef);
			AssertEquals("containerBO.JC_DepartureDeliveryByRail", ZBool.False, containerBO.JC_DepartureDeliveryByRail);
			AssertEquals("containerBO.JC_DepartureDockReceipt", "DOCKRECEIPT", containerBO.JC_DepartureDockReceipt);
			AssertEquals("containerBO.JC_DepartureEstimatedPickup", new ZDateTime(2011, 1, 12), containerBO.JC_DepartureEstimatedPickup);
			AssertEquals("containerBO.JC_DepartureSlotDateTime", new ZDateTime(2011, 1, 13), containerBO.JC_DepartureSlotDateTime);
			AssertEquals("containerBO.JC_DepartureSlotReference", "DEPSLOTREF", containerBO.JC_DepartureSlotReference);
			AssertEquals("containerBO.JC_DunnageWeight", 56.78m, containerBO.JC_DunnageWeight);
			AssertEquals("containerBO.JC_EmptyReadyForReturn", new ZDateTime(2011, 1, 14), containerBO.JC_EmptyReadyForReturn);
			AssertEquals("containerBO.JC_EmptyRequired", new ZDateTime(2011, 1, 15), containerBO.JC_EmptyRequired);
			AssertEquals("containerBO.JC_EmptyReturnedBy", new ZDateTime(2011, 1, 16), containerBO.JC_EmptyReturnedBy);
			AssertEquals("containerBO.JC_EmptyReturnReference", "REEE123", containerBO.JC_EmptyReturnReference);
			AssertEquals("containerBO.JC_ExportDepotCustomsReference", "EXPDEPCUSR", containerBO.JC_ExportDepotCustomsReference);
			AssertEquals("containerBO.JC_ContainerMode", Core.Constants.ContainerModes.LCL, containerBO.JC_ContainerMode);
			AssertEquals("containerBO.JC_OverrideFCLAvailableStorage", ZBool.True, containerBO.JC_OverrideFCLAvailableStorage);
			AssertEquals("containerBO.JC_FCLAvailable", new ZDateTime(2011, 1, 17), containerBO.JC_FCLAvailable);
			AssertEquals("containerBO.JC_FCLHeldInTransitStaging", ZBool.True, containerBO.JC_FCLHeldInTransitStaging);
			AssertEquals("containerBO.JC_FCLOnBoardVessel", new ZDateTime(2011, 1, 18), containerBO.JC_FCLOnBoardVessel);
			AssertEquals("containerBO.JC_FCLStorageArrivedUnderbond", ZBool.False, containerBO.JC_FCLStorageArrivedUnderbond);
			AssertEquals("containerBO.ArrivalCTOStorageCost", 67.89m, containerBO.ArrivalCTOStorageCost);
			AssertEquals("containerBO.JC_ArrivalCTOStorageStartDate", new ZDateTime(2011, 1, 19), containerBO.JC_ArrivalCTOStorageStartDate);
			AssertEquals("containerBO.ArrivalCTOStorageDays", new ZByte(4), containerBO.ArrivalCTOStorageDays);
			AssertEquals("containerBO.JC_FCLStorageModuleOnlyMaster", "FCLSTOMAST", containerBO.JC_FCLStorageModuleOnlyMaster);
			AssertEquals("containerBO.JC_FCLStorageUnderbondCleared", new ZDateTime(2011, 1, 20), containerBO.JC_FCLStorageUnderbondCleared);
			AssertEquals("containerBO.JC_FCLUnloadFromVessel", new ZDateTime(2011, 1, 21), containerBO.JC_FCLUnloadFromVessel);
			AssertEquals("containerBO.JC_FCLWharfGateIn", new ZDateTime(2011, 1, 22), containerBO.JC_FCLWharfGateIn);
			AssertEquals("containerBO.JC_FCLWharfGateOut", new ZDateTime(2011, 1, 23), containerBO.JC_FCLWharfGateOut);
			AssertEquals("containerBO.JC_GrossWeight", 78.90m, containerBO.JC_GrossWeight);
			AssertEquals("containerBO.JC_GrossWeightUQ", Core.Constants.Weight.Pounds, containerBO.JC_GrossWeightUQ);
			AssertEquals("containerBO.JC_HumidityPercent", new ZByte(5), containerBO.JC_HumidityPercent);
			AssertEquals("containerBO.JC_IsCFSRegistered", ZBool.False, containerBO.JC_IsCFSRegistered);
			AssertEquals("containerBO.JC_IsControlledAtmosphere", ZBool.True, containerBO.JC_IsControlledAtmosphere);
			AssertEquals("containerBO.JC_IsDamaged", ZBool.True, containerBO.JC_IsDamaged);
			AssertEquals("containerBO.JC_IsEmptyContainer", ZBool.False, containerBO.JC_IsEmptyContainer);
			AssertEquals("containerBO.JC_IsSealOk", ZBool.True, containerBO.JC_IsSealOk);
			AssertEquals("containerBO.JC_IsShipperOwned", ZBool.False, containerBO.JC_IsShipperOwned);
			AssertEquals("containerBO.JC_OverrideLCLAvailableStorage", ZBool.True, containerBO.JC_OverrideLCLAvailableStorage);
			AssertEquals("containerBO.JC_LCLAvailable", new ZDateTime(2011, 1, 24), containerBO.JC_LCLAvailable);
			AssertEquals("containerBO.JC_LCLStorageCommences", new ZDateTime(2011, 1, 25), containerBO.JC_LCLStorageCommences);
			AssertEquals("containerBO.JC_LCLUnpack", new ZDateTime(2011, 1, 26), containerBO.JC_LCLUnpack);
			AssertEquals("containerBO.JC_PackDate", new ZDateTime(2011, 1, 27), containerBO.JC_PackDate);
			AssertEquals("containerBO.JC_RefrigGeneratorID", "REFGENID", containerBO.JC_RefrigGeneratorID);
			AssertEquals("containerBO.JC_ReleaseNum", "RELNUM", containerBO.JC_ReleaseNum);
			AssertEquals("containerBO.JC_SealNum", "SEAL1", containerBO.JC_SealNum);
			AssertEquals("containerBO.JC_AdditionalSealNum", "SEAL2", containerBO.JC_AdditionalSealNum);
			AssertEquals("containerBO.JC_Additional2SealNum", "SEAL3", containerBO.JC_Additional2SealNum);
			AssertEquals("containerBO.JC_SealParty", "CAR", containerBO.JC_SealParty);
			AssertEquals("containerBO.JC_AdditionalSealParty", "CRD", containerBO.JC_AdditionalSealParty);
			AssertEquals("containerBO.JC_Additional2SealParty", "CUS", containerBO.JC_Additional2SealParty);
			AssertEquals("containerBO.JC_SetPointTemp", 89.01m, containerBO.JC_SetPointTemp);
			AssertEquals("containerBO.JC_SetPointTempUnit", "F", containerBO.JC_SetPointTempUnit);
			AssertEquals("containerBO.JC_StowagePosition", "AABBCCDD", containerBO.JC_StowagePosition);
			AssertEquals("containerBO.JC_TareWeight", 90.12m, containerBO.JC_TareWeight);
			AssertEquals("containerBO.JC_TempRecorderSerialNo", "TEMPRECSERNO", containerBO.JC_TempRecorderSerialNo);
			AssertEquals("containerBO.JC_TotalHeight", 123.45m, containerBO.JC_TotalHeight);
			AssertEquals("containerBO.JC_TotalLength", 234.56m, containerBO.JC_TotalLength);
			AssertEquals("containerBO.JC_TotalUnitOfMeasure", Core.Constants.Length.Centimetres, containerBO.JC_TotalUnitOfMeasure);
			AssertEquals("containerBO.JC_TotalWidth", 345.67m, containerBO.JC_TotalWidth);
			AssertEquals(212.34m, containerBO.JC_Calc_OverhangFront);
			AssertEquals(22.22m, containerBO.JC_OverhangBack);
			AssertEquals(5.00m, containerBO.JC_Calc_OverhangLeft);
			AssertEquals(340.67m, containerBO.JC_OverhangRight);
			AssertEquals(123.45m, containerBO.JC_Calc_OverhangHeight);
			AssertEquals("containerBO.JC_TrainWagonNumber", "TRNWAGNO", containerBO.JC_TrainWagonNumber);
			AssertEquals("containerBO.JC_UnpackGang", "UNPKGANG", containerBO.JC_UnpackGang);
			AssertEquals("containerBO.JC_UnpackShed", "UNPKSHED", containerBO.JC_UnpackShed);
			AssertEquals("containerBO.JC_VolumeCapacity", 456.78m, containerBO.JC_VolumeCapacity);
			AssertEquals("containerBO.JC_VolumeCapacityUQ", Core.Constants.Volume.CubicInches, containerBO.JC_VolumeCapacityUQ);
			AssertEquals("containerBO.JC_WeightCapacity", 567.89m, containerBO.JC_WeightCapacity);
			AssertEquals("containerBO.WeightUnitForBinding", Core.Constants.Weight.Pounds, containerBO.WeightUnitForBinding);
			AssertEquals("containerBO.JC_GoodsValue", 454.66m, containerBO.JC_GoodsValue);
			AssertEquals("containerBO.JC_RX_NKGoodsCurrency", "EUR", containerBO.JC_RX_NKGoodsCurrency);
			AssertEquals("containerBO.DepartureContainerYardAddress", "DEP123", containerBO.DepartureContainerYardAddress.OA_Code);
			AssertEquals("containerBO.ArrivalContainerYardAddress", "ARV456", containerBO.ArrivalContainerYardAddress.OA_Code);

			AssertEquals("containerBO.Services", 1, containerBO.Services.Count);
			var service = containerBO.Services[0];
			AssertEquals("service.ES_Booked", new ZDateTime(2011, 6, 1), service.ES_Booked);
			AssertEquals("service.ES_Completed", new ZDateTime(2011, 6, 2), service.ES_Completed);
			AssertEquals("service.ES_ServiceCode", "FUM", service.ES_ServiceCode);
			OrganizationAddressTestHelper.AssertAddressContentMatches_CRAHOLSYD(service.Contractor.MainAddress);

			var additionalReference1 = containerBO.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryType", "AMS", additionalReference1.CE_EntryType);
			AssertEquals("additionalReference1.CE_EntryNum", "AMS0001", additionalReference1.CE_EntryNum);

			var additionalReference2 = containerBO.AdditionalReferenceNumbers[1];
			AssertEquals("additionalReference2.CE_EntryType", "UBR", additionalReference2.CE_EntryType);
			AssertEquals("additionalReference2.CE_EntryNum", "UBR0001", additionalReference2.CE_EntryNum);
		}

		public static void AssertContentsWithVGM(CommonContainer containerBO)
		{
			AssertContents(containerBO);
			AssertEquals("containerBO.JC_GrossWeightVerificationType", Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, containerBO.JC_GrossWeightVerificationType);
			AssertEquals("containerBO.JC_GrossWeightVerificationDateTime", new ZDateTime(2016, 04, 14, 10, 05, 30), containerBO.JC_GrossWeightVerificationDateTime);
			AssertEquals("GrossWeightVerifiedBy address", "VGM name", containerBO.GrossWeightVerifiedByAddress.E2_CompanyName);
		}
	}
}
