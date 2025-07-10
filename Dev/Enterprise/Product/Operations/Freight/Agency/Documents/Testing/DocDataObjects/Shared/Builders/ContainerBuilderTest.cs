using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class ContainerBuilderTest : TestCaseWithFactory
	{
		#region PopulateGeneralInfo

		public void TestPopulateGeneralInfo()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<AgencyShipment>();

			var containerBO = Factory.New<AgencyShipmentContainer>();

			containerBO.JC_ContainerNum = "C00012345";
			containerBO.JC_ContainerCount = 2;
			containerBO.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			containerBO.JC_DeliverySequence = 1;
			containerBO.JC_IsEmptyContainer = false;
			containerBO.JC_IsShipperOwned = true;
			containerBO.JC_IsDamaged = true;
			containerBO.JC_IsSealOk = true;
			containerBO.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			containerBO.JC_IsNonOperativeReefer = true;
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			containerBO.JC_ContainerQuality = "XYZ";
			containerBO.JC_ContainerStatus = "APP";
			containerBO.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.General;
			containerBO.JC_GoodsValue = 100m;
			containerBO.JC_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 3m;
			packline1.JL_ActualVolumeUQ = "CF";
			packline1.JL_PackageCount = 1;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5m;
			packline2.JL_ActualVolumeUQ = "CF";
			packline2.JL_PackageCount = 2;

			containerBO.PackLines.Add(packline1);
			containerBO.PackLines.Add(packline2);

			var packLineBuilder = new PackingLineBuilder();
			var container = new ContainerBuilder().Build(containerBO, context, shipment.OuterPackLines.Cast<AgencyShipmentPackLine>().Select(p => packLineBuilder.Build(p)).ToArray(), "KG", "CF");

			AssertEquals("C00012345", container.Number);
			AssertEquals(2, container.ContainerCount);
			AssertEquals(Core.Constants.DeliveryModes.Codes.CFS_CFS, container.DeliveryMode);
			AssertEquals((ZShort)1, container.DeliverySequence);
			AssertEquals(3, container.PackCount);
			Assert(!container.IsEmpty);
			Assert(!container.IsPartOf);
			Assert(container.IsShipperOwned);
			Assert(container.IsDamaged);
			Assert(container.IsSealOk);
			AssertEquals("20GP", container.Type.Code);
			AssertEquals("22G0", container.Type.ISOCode);
			AssertEquals("DRY", container.Type.Type.Code);
			Assert(container.IsNonOperativeReefer);
			AssertEquals(Core.Constants.ContainerModes.FCL, container.ContainerMode.Code);
			AssertEquals("XYZ", container.ContainerQuality.Code);
			AssertEquals("APP", container.ContainerStatus.Code);
			AssertEquals(Core.Constants.CargoTypes.General, container.Commodity.Code);
			AssertEquals("100.00 AUD", container.GoodsValue.ToString());
		}

		#endregion

		#region PopulateSeals

		public void TestPopulateSeals()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_SealNum = "SEAL";
			containerBO.JC_SealParty = Core.Constants.ContainerSealParties.Codes.Quarantine;
			containerBO.JC_AdditionalSealNum = "SEAL2";
			containerBO.JC_AdditionalSealParty = Core.Constants.ContainerSealParties.Codes.Customs;
			containerBO.JC_Additional2SealNum = "SEAL3";
			containerBO.JC_Additional2SealParty = Core.Constants.ContainerSealParties.Codes.Terminal;

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("SEAL", container.Seal);
			AssertEquals(Core.Constants.ContainerSealParties.Codes.Quarantine, container.SealPartyType.Code);
			AssertEquals("SEAL2", container.SecondSeal);
			AssertEquals(Core.Constants.ContainerSealParties.Codes.Customs, container.SecondSealPartyType.Code);
			AssertEquals("SEAL3", container.ThirdSeal);
			AssertEquals(Core.Constants.ContainerSealParties.Codes.Terminal, container.ThirdSealPartyType.Code);
		}

		#endregion

		#region PopulateWeightAndVolume

		public void TestPopulateWeightAndVolume()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_GrossVolume = 26.801m;
			containerBO.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			var container = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("GrossVolumeValue", 26.801m, container.GrossVolume.Value);
			AssertEquals("GrossVolumeUnit", Core.Constants.Volume.CubicMetres, container.GrossVolume.Unit.Code);

			container = new ContainerBuilder().Build(containerBO, context, unitOfVolume: Core.Constants.Volume.CubicFeet);
			Assert("GrossVolumeValue", Core.Constants.Volume.Convert(26.801m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicFeet) == container.GrossVolume.Value);
			AssertEquals("GrossVolumeUnit", Core.Constants.Volume.CubicFeet, container.GrossVolume.Unit.Code);
		}

		#endregion

		#region PopulateRefrigeration

		public void TestPopulateRefrigeration()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_IsControlledAtmosphere = true;
			containerBO.JC_TempRecorderSerialNo = "123";
			containerBO.JC_SetPointTemp = 2m;
			containerBO.JC_SetPointTempUnit = Core.Constants.Temperature.Fahrenheit;
			containerBO.JC_HumidityPercent = 33;
			containerBO.JC_AirVentFlow = 12m;
			containerBO.JC_AirVentFlowRateUnit = AirFlowRateUnits.Codes.Percent;
			containerBO.JC_IsControlledAtmosphere = true;
			containerBO.JC_RefrigGeneratorID = "RG11081";

			var container = new ContainerBuilder().Build(containerBO, context);

			Assert(container.HasControlledAtmosphere);
			AssertEquals("123", container.TemperatureRecorderSerialNumber);
			AssertEquals("RG11081", container.RefrigGeneratorID);
			AssertEquals("2.00 F", container.SetTemperature.ToString());
			AssertEquals("33.00 %", container.Humidity.ToString());
			AssertEquals("12.00 P1", container.AirVentFlow.ToString());
			Assert(container.Genset);
		}

		#endregion

		#region PopulateMeasures

		public void TestPopulateMeasures()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_OverhangBack = 1.1m;
			containerBO.JC_OverhangRight = 2.1m;
			containerBO.JC_TotalHeight = 200m;
			containerBO.JC_TotalWidth = 300m;
			containerBO.JC_TotalLength = 400m;

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("398.90 FT", container.OverhangFront.ToString());
			AssertEquals("1.10 FT", container.OverhangBack.ToString());
			AssertEquals("297.90 FT", container.OverhangLeft.ToString());
			AssertEquals("2.10 FT", container.OverhangRight.ToString());
			AssertEquals("200.00 FT", container.OverhangHeight.ToString());
			AssertEquals("200.00 FT", container.TotalHeight.ToString());
			AssertEquals("300.00 FT", container.TotalWidth.ToString());
			AssertEquals("400.00 FT", container.TotalLength.ToString());
		}

		#endregion

		#region PopulateExportInfo

		public void TestPopulateExportInfo()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			containerBO.JC_EmptyRequired = new ZDateTime(2024, 01, 01);
			containerBO.JC_ReleaseNum = "123";

			containerBO.JC_DepartureCartageAdvised = new ZDateTime(2024, 01, 03);
			containerBO.JC_DepartureCartageRef = "3333";
			containerBO.JC_DepartureCartageComplete = new ZDateTime(2024, 01, 04);
			containerBO.DepartureTruckWaitCost = 400m;
			containerBO.DepartureTruckWaitTime = new ZDateTime(2024, 01, 05);
			containerBO.JC_DepartureDeliveryByRail = true;
			containerBO.JC_DepartureSlotDateTime = new ZDateTime(2024, 01, 06);
			containerBO.JC_DepartureSlotReference = "2222";
			containerBO.JC_DepartureEstimatedPickup = new ZDateTime(2024, 01, 07);
			containerBO.JC_ExportDepotCustomsReference = "1111";

			containerBO.JC_FCLWharfGateIn = new ZDateTime(2024, 01, 08);
			containerBO.JC_FCLOnBoardVessel = new ZDateTime(2024, 01, 09);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Ziggy Z";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "Unit 13";
			orgHeader.MainAddress.Address2 = "4 Lost Lane";
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			containerBO.JC_OA_DepartureContainerYardAddress = orgHeader.MainAddress.PK;

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals(new ZDateTime(2024, 01, 01), container.EmptyRequired);
			AssertEquals("123", container.ReleaseNumber);
			AssertEquals(containerBO.JC_ContainerYardEmptyPickupGateOut, container.ContainerParkEmptyPickupGateOut);
			AssertEquals(new ZDateTime(2024, 01, 03), container.DepartureCartageAdvised);
			AssertEquals("3333", container.DepartureCartageReference);
			AssertEquals(new ZDateTime(2024, 01, 04), container.DepartureCartageComplete);
			AssertEquals(400m, container.DepartureTruckWaitCost);
			AssertEquals((ZDateTime)TimeSpan.FromDays(4), container.DepartureTruckWaitTime);
			Assert(container.DepartureDeliveryByRail);
			AssertEquals(new ZDateTime(2024, 01, 06), container.DepartureSlotDateTime);
			AssertEquals("2222", container.DepartureSlotReference);
			AssertEquals(new ZDateTime(2024, 01, 07), container.DepartureEstimatedPickup);
			AssertEquals("ZIGGY Z\r\nUNIT 13\r\n4 LOST LANE\r\nSYDNEY NSW 2000\r\nAUSTRALIA", container.DepartureContainerYard.AddressFormatted);
			AssertEquals("1111", container.ExportDepotCustomsReference);
			AssertEquals(new ZDateTime(2024, 01, 08), container.FCLWharfGateIn);
			AssertEquals(new ZDateTime(2024, 01, 09), container.FCLOnBoardVessel);
		}

		#endregion

		#region PopulateImportInfo

		public void TestPopulateImportInfo()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			containerBO.JC_EmptyReadyForReturn = new ZDateTime(2024, 01, 01);
			containerBO.JC_EmptyReturnedBy = new ZDateTime(2024, 01, 02);
			containerBO.JC_EmptyReturnReference = "123";

			containerBO.JC_ArrivalPickupByRail = true;
			containerBO.JC_ArrivalSlotDateTime = new ZDateTime(2024, 01, 03);
			containerBO.JC_ArrivalSlotReference = "1233";
			containerBO.JC_ArrivalCartageAdvised = new ZDateTime(2024, 01, 04);
			containerBO.JC_ArrivalCartageRef = "12333";
			containerBO.JC_ArrivalCartageComplete = new ZDateTime(2024, 01, 05);
			containerBO.ArrivalTruckWaitCost = 2m;
			containerBO.ArrivalTruckWaitTime = new ZDateTime(2024, 01, 07);
			containerBO.JC_ArrivalDeliveryRequiredBy = new ZDateTime(2024, 01, 08);
			containerBO.JC_ArrivalEstimatedDelivery = new ZDateTime(2024, 01, 09);

			containerBO.JC_PackDate = new ZDateTime(2024, 01, 10);
			containerBO.JC_LCLUnpack = new ZDateTime(2024, 01, 11);
			containerBO.JC_LCLAvailable = new ZDateTime(2024, 01, 12);
			containerBO.JC_LCLStorageCommences = new ZDateTime(2024, 01, 13);
			containerBO.JC_ContainerImportDORelease = "123333";
			containerBO.JC_ImportDepotCustomsReference = "1233333";

			containerBO.ArrivalCTOStorageCost = 3m;
			containerBO.ArrivalCTOStorageDays = 4;
			containerBO.JC_FCLWharfGateOut = new ZDateTime(2024, 01, 14);
			containerBO.JC_FCLAvailable = new ZDateTime(2024, 01, 15);
			containerBO.JC_ArrivalCTOStorageStartDate = new ZDateTime(2024, 01, 16);
			containerBO.JC_FCLUnloadFromVessel = new ZDateTime(2024, 01, 17);
			containerBO.JC_FCLHeldInTransitStaging = true;

			containerBO.ArrivalCarrierDetentionCost = 34m;
			containerBO.ArrivalCarrierDetentionDays = 6;
			containerBO.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2024, 01, 18);

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals(new ZDateTime(2024, 01, 01), container.EmptyReadyForReturn);
			AssertEquals(new ZDateTime(2024, 01, 02), container.EmptyReturnedBy);
			AssertEquals("123", container.EmptyReturnReference);

			Assert(container.ArrivalPickupByRail);
			AssertEquals(new ZDateTime(2024, 01, 03), container.ArrivalSlotDateTime);
			AssertEquals("1233", container.ArrivalSlotReference);
			AssertEquals(new ZDateTime(2024, 01, 04), container.ArrivalCartageAdvised);
			AssertEquals("12333", container.ArrivalCartageReference);
			AssertEquals(new ZDateTime(2024, 01, 05), container.ArrivalCartageComplete);
			AssertEquals(2m, container.ArrivalTruckWaitCost);
			AssertEquals((ZDateTime)TimeSpan.FromDays(6), container.ArrivalTruckWaitTime);
			AssertEquals(new ZDateTime(2024, 01, 08), container.ArrivalDeliveryRequiredBy);
			AssertEquals(new ZDateTime(2024, 01, 09), container.ArrivalEstimatedDelivery);

			AssertEquals(new ZDateTime(2024, 01, 10), container.PackDate);
			AssertEquals(new ZDateTime(2024, 01, 11), container.LCLUnpack);
			AssertEquals(new ZDateTime(2024, 01, 12), container.LCLAvailable);
			AssertEquals(new ZDateTime(2024, 01, 13), container.LCLStorageCommences);
			AssertEquals("123333", container.ContainerImportDORelease);
			AssertEquals("1233333", container.ImportDepotCustomsReference);

			AssertEquals(3m, container.ArrivalCTOStorageCost);
			AssertEquals((ZByte)4, container.ArrivalCTOStorageDays);
			AssertEquals(new ZDateTime(2024, 01, 14), container.FCLWharfGateOut);
			AssertEquals(new ZDateTime(2024, 01, 15), container.FCLAvailable);
			AssertEquals(new ZDateTime(2024, 01, 16), container.ArrivalCTOStorageStartDate);
			AssertEquals(new ZDateTime(2024, 01, 17), container.FCLUnloadFromVessel);
			Assert(container.FCLHeldInTransitStaging);

			AssertEquals(34m, container.ArrivalCarrierDetentionCost);
			AssertEquals((ZByte)6, container.ArrivalCarrierDetentionDays);
			AssertEquals(containerBO.JC_ContainerYardEmptyReturnGateIn, container.ContainerParkEmptyReturnGateIn);
		}

		#endregion

		#region PopulateVerification

		public void TestPopulateVerification()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			containerBO.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			containerBO.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			containerBO.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			containerBO.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
			containerBO.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
			containerBO.GrossWeightVerifiedByAddress.E2_Address2 = "Address 2";
			containerBO.GrossWeightVerifiedByAddress.E2_City = "Mascot";
			containerBO.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
			containerBO.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "";
			containerBO.JC_GrossWeightVerificationDateTime = new ZDateTime(2024, 01, 01);

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals(containerBO.JC_GrossWeightVerificationStatus, container.VerifiedStatus.Code);
			AssertEquals(containerBO.JC_GrossWeightVerificationType, container.VerifiedMethod.Code);
			AssertEquals("CONTAINER WEIGHTING LTD\r\nADDRESS 1\r\nADDRESS 2\r\nMASCOT 2025", container.VerifiedByAddress.AddressFormatted);
			AssertEquals(containerBO.JC_GrossWeightVerificationDateTime, container.VerifiedDate);
		}

		#endregion

		#region PopulateCollections

		public void TestPopulateNumbers()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();

			AssertEquals(0, new ContainerBuilder().Build(containerBO, context).Numbers.Count);

			var number = containerBO.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference;
			number.CE_EntryNum = "1";

			AssertEquals(1, new ContainerBuilder().Build(containerBO, context).Numbers.Count);
		}

		public void TestPopulateAdditionalServices()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();

			AssertEquals(0, new ContainerBuilder().Build(containerBO, context).AdditionalServices.Count);

			containerBO.Services.AddNew();

			AssertEquals(1, new ContainerBuilder().Build(containerBO, context).AdditionalServices.Count);
		}

		public void TestPopulateMilestones()
		{
			var context = new CommonContext(Factory);

			var containerBO = Factory.New<AgencyShipmentContainer>();

			AssertEquals(0, new ContainerBuilder().Build(containerBO, context).Milestones.Count);

			((IWorkflowProvider)containerBO).WorkflowItems.Milestones.AddNew();

			AssertEquals(1, new ContainerBuilder().Build(containerBO, context).Milestones.Count);
		}

		#endregion

		#region PopulateContainerMode

		public void TestPopulateContainerMode()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, container.ContainerMode.Code);
		}

		#endregion

		#region RenameDemurrageAndStorage

		public void TestRenameDemurrageAndStorage()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<AgencyShipmentContainer>();

			containerBO.DepartureTruckWaitCost = 200.12m;
			containerBO.DepartureTruckWaitTime = new ZDateTime(2019, 10, 1);
			containerBO.ArrivalTruckWaitCost = 220.34m;
			containerBO.ArrivalTruckWaitTime = new ZDateTime(2019, 10, 3);
			containerBO.ArrivalCTOStorageCost = 230.56m;
			containerBO.JC_ArrivalCTOStorageStartDate = new ZDateTime(2019, 10, 5);
			containerBO.ArrivalCTOStorageDays = 3;
			containerBO.ArrivalCarrierDetentionCost = 240.78m;
			containerBO.ArrivalCarrierDetentionDays = 9;

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("DepartureTruckWaitCost", 200.12m, container.DepartureTruckWaitCost);
			AssertEquals("DepartureTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 1), container.DepartureTruckWaitTime);
			AssertEquals("ArrivalTruckWaitCost", 220.34m, container.ArrivalTruckWaitCost);
			AssertEquals("ArrivalTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 3), container.ArrivalTruckWaitTime);
			AssertEquals("ArrivalCTOStorageCost", 230.56m, container.ArrivalCTOStorageCost);
			AssertEquals("ArrivalCTOStorageDays", (ZByte)3, container.ArrivalCTOStorageDays);
			AssertEquals("ArrivalCTOStorageStartDate", new ZDateTime(2019, 10, 5), container.ArrivalCTOStorageStartDate);
			AssertEquals("ArrivalCarrierDetentionCost", 240.78m, container.ArrivalCarrierDetentionCost);
			AssertEquals("ArrivalCarrierDetentionDays", (ZByte)9, container.ArrivalCarrierDetentionDays);
		}

		#endregion

		#region PopulateVolumeFromPackLines

		public void TestPopulateVolumeFromPackLines()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var containerBO = Factory.New<AgencyShipmentContainer>();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 3m;
			packline1.JL_ActualVolumeUQ = "CF";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5m;
			packline2.JL_ActualVolumeUQ = "CF";

			containerBO.PackLines.Add(packline1);
			containerBO.PackLines.Add(packline2);

			var packLineBuilder = new PackingLineBuilder();
			var container = new ContainerBuilder().Build(
				containerBO,
				context,
				shipment.OuterPackLines.Cast<AgencyShipmentPackLine>().Select(p => packLineBuilder.Build(p)).ToArray(), "KG", "CF");

			AssertEquals(8m, container.Volume.Value);
		}

		#endregion

		#region PopulateDepotCustomsReferences

		public void TestPopulateDepotCustomsReferences()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<AgencyShipmentContainer>();
			containerBO.JC_ExportDepotCustomsReference = "EXREF";
			containerBO.JC_ImportDepotCustomsReference = "IMREF";

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("EXREF", container.ExportDepotCustomsReference);
			AssertEquals("IMREF", container.ImportDepotCustomsReference);
		}

		#endregion

		#region ContainerID

		public void TestContainerID()
		{
			var context = new CommonContext(Factory);
			var expectedIdentifier = "ContainerID";
			var containerBO = Factory.New<AgencyShipmentContainer>();

			var container1 = new ContainerBuilder().Build(containerBO, context, containerID: expectedIdentifier);
			AssertEquals("Should use the passed in identifier", expectedIdentifier, container1.Identifier);

			var container2 = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("Should use the container PK if no identifier passed", containerBO.PK, container2.Identifier);
		}

		#endregion

		#region PopulateCustomerLoadReference

		public void TestPopulateCustomerLoadReference()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<AgencyShipmentContainer>();

			AssertNullOrEmpty(new ContainerBuilder().Build(containerBO, context).CustomerLoadReference);

			AddAdditionalReferenceNumber("NUM1_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
			AddAdditionalReferenceNumber("NUM2_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC);
			AddAdditionalReferenceNumber("NUM3_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UBR);
			AddAdditionalReferenceNumber("NUM1_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference);
			AddAdditionalReferenceNumber("NUM2_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoAdvice);
			AddAdditionalReferenceNumber("NUM3_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference);

			AddAdditionalReferenceNumber("NUM1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM2", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM3", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM2", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM3", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM4", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM5", ZString.Empty, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM1", ZString.Empty, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);

			AssertEquals("NUM1, NUM2, NUM3, NUM4, NUM5", new ContainerBuilder().Build(containerBO, context).CustomerLoadReference);

			void AddAdditionalReferenceNumber(ZString entryNum, ZString countryCode, ZString entryType)
			{
				var additionalReferenceNumber = containerBO.AdditionalReferenceNumbers.AddNew();
				additionalReferenceNumber.CE_EntryNum = entryNum;
				additionalReferenceNumber.CE_RN_NKCountryCode = countryCode;
				additionalReferenceNumber.CE_EntryType = entryType;
			}
		}

		#endregion
	}
}
