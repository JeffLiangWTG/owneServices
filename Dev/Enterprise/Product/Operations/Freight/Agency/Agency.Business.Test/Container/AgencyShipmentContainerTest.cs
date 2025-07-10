using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerTest : BaseFreightTest
	{
		public void TestShouldUseAgencyUNDGDataItemCollection()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			AssertType<AgencyUNDGDataItemCollection>(container.UNDGs);
		}

		public void TestNoteTypes()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			AssertNotNull("Container shoud not be null", container);
			var expected = new[] { PredefinedNoteTypes.Instance.SpecialInstructions, PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes };
			AssertContainsExactElementsInAnyOrder("Should only have 2 noteType", expected, container.NoteTypes.Cast<PredefinedNoteType>());
		}

		public void TestDefaultWeightUnit()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			CommonContainer container1 = shipment.RealContainers.AddNew();
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			CommonContainer container2 = shipment.RealContainers.AddNew();
			AssertEquals("Container1", Constants.Weight.Kilograms, container1.JC_GrossWeightUQ);
			AssertEquals("Container2", Constants.Weight.Tonnes, container2.JC_GrossWeightUQ);
		}

		public void TestDefaultVolumeUnit()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			CommonContainer container1 = shipment.RealContainers.AddNew();
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicYards);
			CommonContainer container2 = shipment.RealContainers.AddNew();
			AssertEquals("Container1", Constants.Volume.CubicMetres, container1.JC_GrossVolumeUQ);
			AssertEquals("Container2", Constants.Volume.CubicYards, container2.JC_GrossVolumeUQ);
		}

		public void TestVolumeCalculatedFromDimensions()
		{
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AssertVolumeCalculatedFromDimensions(mode);
			}
		}

		void AssertVolumeCalculatedFromDimensions(ZString containerMode)
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = containerMode;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			container.JC_ContainerCount = 1;
			container.JC_TotalLength = 2m;
			container.JC_TotalWidth = 4m;
			container.JC_TotalHeight = 8m;
			AssertEquals("Volume calculated from dimensions", 64m, container.JC_GrossVolume);
			container.JC_TotalLength = 0m;
			container.JC_TotalWidth = 4m;
			container.JC_TotalHeight = 8m;
			AssertEquals("Volume is not recalculated from zero dimension(s)", 64m, container.JC_GrossVolume);
			container.JC_TotalLength = 2m;
			container.JC_TotalWidth = 0m;
			container.JC_TotalHeight = 8m;
			AssertEquals("Volume is not recalculated from zero dimension(s)", 64m, container.JC_GrossVolume);
			container.JC_TotalLength = 2m;
			container.JC_TotalWidth = 4m;
			container.JC_TotalHeight = 0m;
			AssertEquals("Volume is not recalculated from zero dimension(s)", 64m, container.JC_GrossVolume);
			container.JC_TotalLength = 3m;
			container.JC_TotalWidth = 6m;
			container.JC_TotalHeight = 9m;
			AssertEquals("Volume calculated from dimensions", 162m, container.JC_GrossVolume);
			container.JC_TotalUnitOfMeasure = Constants.Length.Feet;
			AssertEquals(4.587m, container.JC_GrossVolume);
			container.JC_ContainerCount = 2;
			AssertEquals(9.175m, container.JC_GrossVolume);
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			container.JC_ContainerCount = 3;
			AssertEquals("Volume is not recalculated for non-top level pack containers", 9.175m, container.JC_GrossVolume);
		}

		public void TestHasHazardous()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_RH_NKContainerCommodityCode = "GEN";
			Assert(!container.HasHazardous);
			container.JC_RH_NKContainerCommodityCode = "MTHZ";
			Assert(container.HasHazardous);
			container.JC_RH_NKContainerCommodityCode = "GEN";
			Assert(!container.HasHazardous);
			container.UNDGs.AddNew();
			Assert(container.HasHazardous);
		}

		public void TestDocManagerInfo()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertEquals(Constants.DocManagerCodes.AgencyBillContainers, container.DocManagerInfo.DocManagerCode);
		}

		public void TestMovementsUpdate()
		{
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage2.GenerateSailings();
			ContainerMovement movement11 = stock1.Movements.AddNew();
			movement11.E9_JV = voyage1.PK;
			movement11.E9_OtherLocation = "movement11";
			ContainerMovement movement12 = stock1.Movements.AddNew();
			movement12.E9_JV = voyage2.PK;
			movement12.E9_OtherLocation = "movement12";
			ContainerMovement movement21 = stock2.Movements.AddNew();
			movement21.E9_JV = voyage1.PK;
			movement21.E9_OtherLocation = "movement21";
			ContainerMovement movement22 = stock2.Movements.AddNew();
			movement22.E9_JV = voyage2.PK;
			movement22.E9_OtherLocation = "movement22";
			Factory.Save();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage1.Sailings[0].PK;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = stock1.R6_ContainerNum;
			AssertContainsExactElementsInAnyOrder("Stock1, Voyage1", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement11 }, container.Movements);
			container.JC_ContainerNum = "";
			AssertContainsExactElementsInAnyOrder("no stock, Voyage1", (m) => m.E9_OtherLocation, Array.Empty<ContainerMovement>(), container.Movements);
			container.JC_ContainerNum = stock2.R6_ContainerNum;
			AssertContainsExactElementsInAnyOrder("Stock2, Voyage1", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement21 }, container.Movements);
			shipment.JS_JX = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("Stock2, no voyage", (m) => m.E9_OtherLocation, Array.Empty<ContainerMovement>(), container.Movements);
			shipment.JS_JX = voyage2.Sailings[0].PK;
			AssertContainsExactElementsInAnyOrder("Stock2, Voyage2", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement22 }, container.Movements);
		}

		public void TestMovementDateProperties()
		{
			ZDateTime today = ZDateTime.Today;
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "BLAT4100011";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage exportVoyage = Factory.New<JobVoyage>();
			exportVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			exportVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			exportVoyage.GenerateSailings();
			JobVoyage importVoyage = Factory.New<JobVoyage>();
			importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			importVoyage.GenerateSailings();
			OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
			aklDepot.OH_Code = "AKL";
			aklDepot.OH_RL_NKClosestPort = "NZAKL";
			OrgHeader bneDepot = Factory.NewWithValidTestData<OrgHeader>();
			bneDepot.OH_Code = "BNE";
			bneDepot.OH_RL_NKClosestPort = "AUBNE";
			OrgHeader amsDepot = Factory.NewWithValidTestData<OrgHeader>();
			amsDepot.OH_Code = "AMS";
			amsDepot.OH_RL_NKClosestPort = "NLAMS";
			OrgHeader kyivDepot = Factory.NewWithValidTestData<OrgHeader>();
			kyivDepot.OH_Code = "KYV";
			kyivDepot.OH_RL_NKClosestPort = "UAIEV";
			OrgHeader melbourneDepot = Factory.NewWithValidTestData<OrgHeader>();
			melbourneDepot.OH_Code = "MEL";
			melbourneDepot.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			string[] movementTypes = { ContainerMovementTypes.Codes.YardGateOut, ContainerMovementTypes.Codes.WharfGateIn, ContainerMovementTypes.Codes.Load, ContainerMovementTypes.Codes.Discharge, ContainerMovementTypes.Codes.WharfGateOut, ContainerMovementTypes.Codes.YardGateIn, };
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = exportVoyage.Sailings[0].PK;
			Transport transport = bill.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = importVoyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "BLAT4100011";
			container.JC_OA_ArrivalContainerYardAddress = kyivDepot.MainAddress.PK;
			container.JC_OA_DepartureContainerYardAddress = melbourneDepot.MainAddress.PK;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < movementTypes.Length; j++)
				{
					ContainerMovement movementB = stock.Movements.AddNew();
					movementB.E9_JV = importVoyage.PK;
					movementB.E9_MovementType = movementTypes[j];
					movementB.E9_MovementDate = today.AddDays(i * 10 + j);
					if (movementTypes[j] == ContainerMovementTypes.Codes.WharfGateOut)
					{
						movementB.E9_OA_Depot = amsDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.WharfGateIn)
					{
						movementB.E9_OA_Depot = aklDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.YardGateIn)
					{
						movementB.E9_OA_Depot = kyivDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.YardGateOut)
					{
						movementB.E9_OA_Depot = melbourneDepot.MainAddress.PK;
					}
					else if (ContainerMovementTypes.GetDirection(movementTypes[j]) == MovementDirection.Codes.Departure)
					{
						movementB.E9_OA_Depot = aklDepot.MainAddress.PK;
					}
					else
					{
						movementB.E9_OA_Depot = bneDepot.MainAddress.PK;
					}
				}
			}

			CombineAssertions(delegate
			{
				AssertEquals("YGO Date", today.AddDays(0), container.JC_ContainerYardEmptyPickupGateOut);
				AssertEquals("WGI Date", today.AddDays(1), container.JC_FCLWharfGateIn);
				AssertEquals("LOD Date", today.AddDays(2), container.JC_FCLOnBoardVessel);
				AssertEquals("DIS Date", today.AddDays(13), container.JC_FCLUnloadFromVessel);
				AssertEquals("WGO Date", today.AddDays(14), container.JC_FCLWharfGateOut);
				AssertEquals("YGI Date", today.AddDays(15), container.JC_ContainerYardEmptyReturnGateIn);
				AssertEquals("YGO Port", "AUMEL", container.JC_RL_NKContainerYardEmptyPickupGateOutPort);
				AssertEquals("WGI Port", "NZAKL", container.JC_RL_NKFCLWharfGateInPort);
				AssertEquals("LOD Port", "NZAKL", container.JC_RL_NKFCLOnBoardVesselPort);
				AssertEquals("DIS Port", "AUBNE", container.JC_RL_NKFCLUnloadFromVesselPort);
				AssertEquals("WGO Port", "NLAMS", container.JC_RL_NKFCLWharfGateOutPort);
				AssertEquals("YGI Port", "UAIEV", container.JC_RL_NKContainerYardEmptyReturnGateInPort);
			});
			movementTypes[movementTypes.Length - 1] = ContainerMovementTypes.Codes.ReturnToWharf;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < movementTypes.Length; j++)
				{
					ContainerMovement movementA = stock.Movements.AddNew();
					movementA.E9_JV = exportVoyage.PK;
					movementA.E9_MovementType = movementTypes[j];
					movementA.E9_MovementDate = today.AddDays(i * 10 + j + 50);
					if (movementTypes[j] == ContainerMovementTypes.Codes.WharfGateOut)
					{
						movementA.E9_OA_Depot = amsDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.WharfGateIn)
					{
						movementA.E9_OA_Depot = aklDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.ReturnToWharf)
					{
						movementA.E9_OA_Depot = kyivDepot.MainAddress.PK;
					}
					else if (movementTypes[j] == ContainerMovementTypes.Codes.YardGateOut)
					{
						movementA.E9_OA_Depot = melbourneDepot.MainAddress.PK;
					}
					else if (ContainerMovementTypes.GetDirection(movementTypes[j]) == MovementDirection.Codes.Departure)
					{
						movementA.E9_OA_Depot = bneDepot.MainAddress.PK;
					}
					else
					{
						movementA.E9_OA_Depot = amsDepot.MainAddress.PK;
					}
				}
			}

			CombineAssertions(delegate
			{
				AssertEquals("YGO Date", today.AddDays(0), container.JC_ContainerYardEmptyPickupGateOut);
				AssertEquals("WGI Date", today.AddDays(1), container.JC_FCLWharfGateIn);
				AssertEquals("LOD Date", today.AddDays(2), container.JC_FCLOnBoardVessel);
				AssertEquals("DIS Date", today.AddDays(63), container.JC_FCLUnloadFromVessel);
				AssertEquals("WGO Date", today.AddDays(64), container.JC_FCLWharfGateOut);
				AssertEquals("YGI Date", today.AddDays(65), container.JC_ContainerYardEmptyReturnGateIn);
				AssertEquals("YGO Port", "AUMEL", container.JC_RL_NKContainerYardEmptyPickupGateOutPort);
				AssertEquals("WGI Port", "NZAKL", container.JC_RL_NKFCLWharfGateInPort);
				AssertEquals("LOD Port", "NZAKL", container.JC_RL_NKFCLOnBoardVesselPort);
				AssertEquals("DIS Port", "NLAMS", container.JC_RL_NKFCLUnloadFromVesselPort);
				AssertEquals("WGO Port", "NLAMS", container.JC_RL_NKFCLWharfGateOutPort);
				AssertEquals("YGI Port", "UAIEV", container.JC_RL_NKContainerYardEmptyReturnGateInPort);
			});
		}

		public void TestMovementDateProperties_NotRealContainers()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AssertMovementDateProperties_NotRealContainers(mode);
			}
		}

		public void AssertMovementDateProperties_NotRealContainers(ZString mode)
		{
			ZDateTime today = ZDateTime.Today;
			var bill = Factory.New<BillOfLading>();
			bill.JS_PackingMode = mode;
			var container = bill.RealContainers.AddNew();
			AssertEquals(false, container.JC_FCLWharfGateInInfo.ReadOnly);
			AssertEquals(false, container.JC_FCLOnBoardVesselInfo.ReadOnly);
			AssertEquals(false, container.JC_FCLUnloadFromVesselInfo.ReadOnly);
			AssertEquals(false, container.JC_FCLWharfGateOutInfo.ReadOnly);
			container.JC_FCLWharfGateIn = today.AddDays(1);
			container.JC_FCLOnBoardVessel = today.AddDays(2);
			container.JC_FCLUnloadFromVessel = today.AddDays(13);
			container.JC_FCLWharfGateOut = today.AddDays(14);
			AssertEquals(today.AddDays(1), container.JC_FCLWharfGateIn);
			AssertEquals(today.AddDays(2), container.JC_FCLOnBoardVessel);
			AssertEquals(today.AddDays(13), container.JC_FCLUnloadFromVessel);
			AssertEquals(today.AddDays(14), container.JC_FCLWharfGateOut);
		}

		public void TestDefaultContainerModeIsFCL()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipment>().RealContainers.AddNew();
			AssertEquals("FCL", container.JC_ContainerMode);
		}

		public void TestDetentionFreeDays()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.ConsignorContainerPenalties.DeleteAll();
			var consignorDetention = consignor.ConsignorContainerPenalties.AddNew();
			consignorDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			consignorDetention.PD_FreeDays = 6;
			consignorDetention.PD_OH_Carrier = carrier.PK;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.ConsigneeContainerPenalties.DeleteAll();
			var consigneeDetention = consignee.ConsigneeContainerPenalties.AddNew();
			consigneeDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			consigneeDetention.PD_FreeDays = 7;
			consigneeDetention.PD_OH_Carrier = carrier.PK;
			var container = Factory.New<AgencyShipment>().BookedContainers.AddNew();
			container.Booking.BookedShippingLinePK = carrier.PK;
			container.Booking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			container.Booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			AssertEquals("Import Detention Free Days", 7, container.JC_Calc_ImportDetentionFreeDays);
			AssertEquals("Export Detention Free Days", 6, container.JC_Calc_ExportDetentionFreeDays);
		}

		public void TestDetentionOverdueDays()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100011";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Sailings.AddNew();
			JobSailing sailing = voyage.Sailings[0];
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			ContainerMovement movement1 = stock.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_DetentionDays = 6;
			ContainerMovement movement2 = stock.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			movement2.E9_DetentionDays = 7;
			ContainerMovement movement3 = stock.Movements.AddNew();
			movement3.E9_JV = voyage.PK;
			movement3.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement3.E9_DetentionDays = 8;
			AssertEquals("Import Detention Days", 6, container.JC_Calc_ImportDetentionOverdueDays);
			AssertEquals("Export Detention Days", 8, container.JC_Calc_ExportDetentionOverdueDays);
		}

		public void TestFieldsReadonlyWhenReleased()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			Converter<AgencyShipmentContainer, ZPropertyInfo[]> getInfos = delegate(AgencyShipmentContainer target)
			{
				return new ZPropertyInfo[] { container.JC_RCInfo, container.JC_ContainerCountInfo, container.JC_ContainerStatusInfo, container.JC_ContainerQualityInfo, container.JC_OA_DepartureContainerYardAddressInfo, };
			};
			container.JC_ReleaseNum = "";
			ZPropertyInfo[] propertiesReadonly = Array.FindAll(getInfos(container), (i) => i.ReadOnly);
			container.JC_ReleaseNum = "Blaticus";
			ZPropertyInfo[] propertiesNotReadonly = Array.FindAll(getInfos(container), (i) => !i.ReadOnly);
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			if (propertiesReadonly.Length > 0)
			{
				result.Add("These properties are readonly when not released.", Array.ConvertAll(propertiesReadonly, (i) => i.Name));
			}

			if (propertiesNotReadonly.Length > 0)
			{
				result.Add("These properties are not readonly when released.", Array.ConvertAll(propertiesNotReadonly, (i) => i.Name));
			}

			AssertGroupedErrorList(result);
		}

		public void TestJC_RC()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_SetPointTemp = 0;
			container.JC_HumidityPercent = 0;
			container.JC_AirVentFlow = 0;
			AssertNoWarnings("should not have any warnings", container.JC_SetPointTempInfo);
			AssertNoWarnings("should not have any warnings", container.JC_HumidityPercentInfo);
			AssertNoWarnings("should not have any warnings", container.JC_AirVentFlowInfo);
			container.JC_RC = RC_20RE_PK;
			AssertHasWarnings("should have a warning", container.JC_SetPointTempInfo);
			AssertHasWarnings("should have a warning", container.JC_HumidityPercentInfo);
			AssertHasWarnings("should have a warning", container.JC_AirVentFlowInfo);
			container.JC_SetPointTemp = 1;
			container.JC_HumidityPercent = 1;
			container.JC_AirVentFlow = 1;
			AssertNoWarnings("should not have a warning anymore", container.JC_SetPointTempInfo);
			AssertNoWarnings("should not have a warning anymore", container.JC_HumidityPercentInfo);
			AssertNoWarnings("should not have a warning anymore", container.JC_AirVentFlowInfo);
		}

		public void TestICanDelete_EIDO()
		{
			const string error = "A container can only be deleted if no E-IDO messages have been successfully sent for it or if it has been successfully withdrawn.";
			ZDateTime now = ZDateTime.Now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			EIDOMessage lastSentMessage;
			int offset = 1;
			AssertCanDelete("can be deleted if nothing sent", container);
			// first message
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			AssertCannotDelete("has sent a message", container, error);
			lastSentMessage.EM_Status = EIDOMessage.Status.Failed;
			AssertCanDelete("can delete if the message failed to be sent", container);
			ResponseMessage(lastSentMessage, EIDOResponseType.Rejected);
			AssertCanDelete("can delete after send message was rejected", container);
			// second message
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			AssertCannotDelete("cannot delete as a new message was sent", container, error);
			ResponseMessage(lastSentMessage, EIDOResponseType.Received);
			AssertCannotDelete("cannot delete as the message was received", container, error);
			ResponseMessage(lastSentMessage, EIDOResponseType.Accepted);
			AssertCannotDelete("cannot delete as the message was accepted", container, error);
			// first withdraw
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Cancelation, now, offset++);
			AssertCannotDelete("cannot delete as a cancelation has been sent", container, error);
			lastSentMessage.EM_Status = EIDOMessage.Status.Failed;
			AssertCannotDelete("cannot delete as the cancelation failed to be sent", container, error);
			ResponseMessage(lastSentMessage, EIDOResponseType.Rejected);
			AssertCannotDelete("cannot delete as the cancelation was rejected", container, error);
			// second withdraw
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Cancelation, now, offset++);
			AssertCannotDelete("cannot delete as a new cancelation has been sent", container, error);
			ResponseMessage(lastSentMessage, EIDOResponseType.Accepted);
			AssertCanDelete("can delete once withdrawn", container);
		}

		public void TestICanDelete_ContainerRelease()
		{
			const string error = "This container has been released, you will need to replace the release without this container before you can delete it.";
			ZDateTime now = ZDateTime.Now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			AssertEquals("can be deleted", true, ((ICanDelete)container).CanDelete);
			AssertEquals("can be deleted", "", ((ICanDelete)container).ReasonForNotAbleToDelete);
			container.JC_ReleaseNum = "Ref-1";
			AssertEquals("has been released", false, ((ICanDelete)container).CanDelete);
			AssertEquals("has been released", error, ((ICanDelete)container).ReasonForNotAbleToDelete);
		}

		public void TestJC_EIDOStatus()
		{
			ZDateTime now = ZDateTime.Now;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			EIDOMessage lastSentMessage;
			int offset = 1;
			AssertEquals("No messages sent yet.", ReleaseImportOrderMessageStatusList.Codes.NotSent, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			AssertEquals("Original message sent.", ReleaseImportOrderMessageStatusList.Codes.OriginalSent, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Rejected);
			AssertEquals("Original message rejected.", ReleaseImportOrderMessageStatusList.Codes.Rejected, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			AssertEquals("Another original message sent.", ReleaseImportOrderMessageStatusList.Codes.OriginalSent, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Received);
			AssertEquals("Message received.", ReleaseImportOrderMessageStatusList.Codes.Acknowledged, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Accepted);
			AssertEquals("Message accepted.", ReleaseImportOrderMessageStatusList.Codes.Accepted, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Cancelation, now, offset++);
			AssertEquals("Withdraw sent.", ReleaseImportOrderMessageStatusList.Codes.WithdrawSent, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Rejected);
			AssertEquals("Withdraw rejected.", ReleaseImportOrderMessageStatusList.Codes.Rejected, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Cancelation, now, offset++);
			AssertEquals("Another withdraw sent.", ReleaseImportOrderMessageStatusList.Codes.WithdrawSent, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Accepted);
			AssertEquals("Withdraw accepted.", ReleaseImportOrderMessageStatusList.Codes.Withdrawn, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			AssertEquals("3rd original sent.", ReleaseImportOrderMessageStatusList.Codes.OriginalSent, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Received);
			AssertEquals("Original message acknowledged.", ReleaseImportOrderMessageStatusList.Codes.Acknowledged, container.JC_ImportReleaseOrderStatus);
			ResponseMessage(lastSentMessage, EIDOResponseType.Accepted);
			AssertEquals("Original message accepted.", ReleaseImportOrderMessageStatusList.Codes.Accepted, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Original, now, offset++);
			lastSentMessage.EM_Status = EIDOMessage.Status.Failed;
			AssertEquals("Original Failed", ReleaseImportOrderMessageStatusList.Codes.Failed, container.JC_ImportReleaseOrderStatus);
			lastSentMessage = SendEIDOMessage(container, EIDOMessageFunction.Cancelation, now, offset++);
			lastSentMessage.EM_Status = EIDOMessage.Status.Failed;
			AssertEquals("Cancelation Failed", ReleaseImportOrderMessageStatusList.Codes.Failed, container.JC_ImportReleaseOrderStatus);
		}

		public void TestJC_ImportReleaseOrderStatus_CountryIsNZ()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.Company.GC_RN_NKCountryCode = "NZ";
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
				AssertEquals("JC_ImportReleaseOrderStatus", ReleaseImportOrderMessageStatusList.Codes.NotSent, container.JC_ImportReleaseOrderStatus);
				container.Logs.AddNew(Events.MessageSent, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrder));
				Factory.Save();
				AssertEquals("JC_ImportReleaseOrderStatus", ReleaseImportOrderMessageStatusList.Codes.OriginalSent, container.JC_ImportReleaseOrderStatus);
				container.Logs.AddNew(Events.MessageWithdrawCancelRequest, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal));
				Factory.Save();
				AssertEquals("JC_ImportReleaseOrderStatus", ReleaseImportOrderMessageStatusList.Codes.WithdrawSent, container.JC_ImportReleaseOrderStatus);
				container.Logs.AddNew(Events.InterchangeRejected, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrder));
				Factory.Save();
				AssertEquals("JC_ImportReleaseOrderStatus", ReleaseImportOrderMessageStatusList.Codes.Rejected, container.JC_ImportReleaseOrderStatus);
			}
		}

		public void TestJC_EIDOStatusMaxLength()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertEquals("MaxLength", new ReleaseImportOrderMessageStatusList().MaxCodeLength, container.JC_ImportReleaseOrderStatusInfo.MaxLength);
		}

		public void TestCantSetContainerCountToANonPositiveValue()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerCount = 4;
			AssertEquals("Container.JC_ContainerCount", 4, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = -4;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 5;
			AssertEquals("Container.JC_ContainerCount", 5, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 0;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
		}

		public void TestContainerImportDOReleaseIsReadOnly()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertEquals("Should be readonly", true, container.JC_ContainerImportDOReleaseInfo.ReadOnly);
		}

		public void TestCustomsEntryNumbers()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertEquals("CusEntryNumbers should be Empty", 0, container.CusEntryNumbers.Count);
			AssertEquals("CustomsEntryNumberType should be Empty", ZString.Empty, container.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber should be Empty", ZString.Empty, container.CustomsEntryNumber);
			AssertEquals("CustomsEntryNumberInfo should NOT be ReadOnly", false, container.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("HasChanges should be false", false, container.HasChanges);
			container.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;
			AssertEquals("Should be CusEntryNumber", 1, container.CusEntryNumbers.Count);
			AssertEquals("CE_EntryType should be 'CCN'", "CCN", container.CusEntryNumbers[0].CE_EntryType);
			AssertEquals("CustomsEntryNumberType should be 'CCN'", "CCN", container.CustomsEntryNumberType);
			AssertEquals("CE_EntryNum should be Empty", ZString.Empty, container.CusEntryNumbers[0].CE_EntryNum);
			AssertEquals("CustomsEntryNumberInfo should NOT be ReadOnly", false, container.CustomsEntryNumberInfo.ReadOnly);
			AssertEquals("HasChanges should be true", true, container.HasChanges);
			container.CustomsEntryNumber = "777";
			AssertEquals("CE_EntryNum should be '777'", "777", container.CusEntryNumbers[0].CE_EntryNum);
			AssertEquals("CustomsEntryNumber should be '777'", "777", container.CustomsEntryNumber);
			container.CustomsEntryNumberType = CMRExportExemptionCodes.EXDD.Code;
			AssertEquals("CE_EntryType should be 'XDD'", "XDD", container.CusEntryNumbers[0].CE_EntryType);
			AssertEquals("CustomsEntryNumberType should be 'EXDD'", "EXDD", container.CustomsEntryNumberType);
			AssertEquals("CE_EntryNum should be Empty", ZString.Empty, container.CusEntryNumbers[0].CE_EntryNum);
			AssertEquals("CustomsEntryNumber should be Empty", ZString.Empty, container.CustomsEntryNumber);
			AssertEquals("CustomsEntryNumberInfo should be ReadOnly", true, container.CustomsEntryNumberInfo.ReadOnly);
			container.CustomsEntryNumberType = "";
			AssertEquals("CusEntryNumbers should be Empty", 0, container.CusEntryNumbers.Count);
			AssertEquals("CustomsEntryNumberType should be Empty", ZString.Empty, container.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber should be Empty", ZString.Empty, container.CustomsEntryNumber);
			AssertEquals("CustomsEntryNumberInfo should NOT be ReadOnly", false, container.CustomsEntryNumberInfo.ReadOnly);
			container.CustomsEntryNumber = "555";
			AssertEquals("Should be CusEntryNumber", 1, container.CusEntryNumbers.Count);
			AssertEquals("CE_EntryType should be 'CAN'", "CAN", container.CusEntryNumbers[0].CE_EntryType);
			AssertEquals("CustomsEntryNumberType should be 'CAN'", "CAN", container.CustomsEntryNumberType);
			AssertEquals("CE_EntryNum should be '555'", "555", container.CusEntryNumbers[0].CE_EntryNum);
			AssertEquals("CustomsEntryNumber should be '555'", "555", container.CustomsEntryNumber);
		}

		public void TestCustomsEntryNumberType_List()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();

			AssertEquals(36, container.CustomsEntryNumberType_List.Count);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.Company.GC_RN_NKCountryCode = "NZ";
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var container2 = shipment.RealContainers.AddNew();

				AssertEquals(FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value.Count, container2.CustomsEntryNumberType_List.Count);
			}
		}

		public void TestBillContainersEntryNumbers()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			cusEntryNumber.CE_EntryNum = "123";
			AssertEquals("Should be Entry Number Type from Shipment level", "CAN", container.BillContainersEntryNumberType);
			AssertEquals("Should be Entry Number from Shipment level", "123", container.BillContainersEntryNumber);
			shipment = Factory.NewWithValidTestData<AgencyShipment>();
			container = shipment.RealContainers.AddNew();
			container.CustomsEntryNumber = "123";
			AssertEquals("Should be Entry Number Type from Container level", "CAN", container.BillContainersEntryNumberType);
			AssertEquals("Should be Entry Number from Container level", "123", container.BillContainersEntryNumber);
		}

		public void TestValidation()
		{
			AgencyShipmentContainer container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			AssertEquals("Validation should be AgencyShipmentContainerValidation", typeof(AgencyShipmentContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(typeof(AgencyRORContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(typeof(AgencyTopLevelPackValidation), container.Validation.GetType());
		}

		public void TestGetNewValidation()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
				AgencyShipmentContainer container = shipment.RealContainers.AddNew();
				AssertNoError("CustomsEntryNumberInfo should has NO error", container.CustomsEntryNumberInfo, "CAN is available on either Shipment OR Container level");
				container.CustomsEntryNumberType = "XXX";
				AssertHasError("CustomsEntryNumberTypeInfo should has error", container.CustomsEntryNumberTypeInfo, "Enter a valid selection.");
				shipment.CustomsEntryNumberType = "CAN";
				shipment.CustomsEntryNumber = "111";
				container.CustomsEntryNumberType = "CCN";
				AssertNoError("CustomsEntryNumberTypeInfo should has NO error", container.CustomsEntryNumberTypeInfo, "Enter a valid selection.");
				AssertHasError("CustomsEntryNumberInfo should has error", container.CustomsEntryNumberInfo, "CAN is available on either Shipment OR Container level");
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}

		public void TestJC_ContainerMode()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("Expected mode to be set as FCL is a valid mode when the shipment is FCL", Constants.ContainerModes.FCL, container.JC_ContainerMode);
			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			AssertEquals("Expected mode to be emptied as liquid isn't a FCL type", ZString.Empty, container.JC_ContainerMode);
			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals("Expected mode to be set as LCL is a valid mode when the shipment is FCL", Constants.ContainerModes.LCL, container.JC_ContainerMode);
			container.JC_ContainerMode = "XXX";
			AssertEquals("Expected mode to be emptied as mode isn't valid", ZString.Empty, container.JC_ContainerMode);
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var vehicle = shipment.RealContainers.AddNew();
			vehicle.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("Expected mode to emptied as FCL isn't a valid mode for RORO", ZString.Empty, vehicle.JC_ContainerMode);
			vehicle.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals("Expected mode to be RORO", Constants.ContainerModes.RollOnRollOff, vehicle.JC_ContainerMode);
		}

		public void TestJC_ContainerMode_List()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			AssertType(typeof(AgencyShipmentContainerModeList), container.JC_ContainerMode_List);
		}

		public void TestIsRollOnRollOff()
		{
			var container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(false, container.IsRollOnRollOff);
			container.JC_ContainerMode = "XXX";
			AssertEquals(false, container.IsRollOnRollOff);
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(true, container.IsRollOnRollOff);
		}

		public void TestIsTopLevelPack()
		{
			var container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(false, container.IsTopLevelPack);
			container.JC_ContainerMode = "XXX";
			AssertEquals(false, container.IsTopLevelPack);
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(true, container.IsTopLevelPack);
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(true, container.IsTopLevelPack);
			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			AssertEquals(true, container.IsTopLevelPack);
			container.JC_ContainerMode = Constants.ContainerModes.Bulk;
			AssertEquals(true, container.IsTopLevelPack);
		}

		public void TestJC_DepartureCartageComplete_IsEventDateProperty()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			var property = EventDatePropertyAttribute.FindPropertyInfos(container, Events.PickedUp, EstimateActual.Actual).FirstOrDefault();
			AssertNotNull(property);
			AssertEquals(container.JC_DepartureCartageCompleteInfo.Name, property.Property.Name);
		}

		public void TestJC_DepartureCartageComplete_CreatesPickedUpEventLog()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_DepartureCartageComplete = ZDateTime.Now;
			AssertNotNull("PUP event log created", container.Logs.MostRecentLogByEventTime(Events.PickedUp));
		}

		public void TestJC_StowagePosition()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerMode = "FCL";
			container.JC_StowagePosition = "1234";
			AssertEquals("1234", container.JC_StowagePosition);
			container.JC_StowagePosition = "12345";
			AssertEquals("0012345", container.JC_StowagePosition);
			container.JC_StowagePosition = "123456";
			AssertEquals("0123456", container.JC_StowagePosition);
			container.JC_StowagePosition = "1234567";
			AssertEquals("1234567", container.JC_StowagePosition);
			container.JC_StowagePosition = "1A0123";
			AssertEquals("01A0123", container.JC_StowagePosition);
		}

		public void TestJobNumber()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			AssertEquals("D00001000", container.JobNumber);
		}

		public void TestJC_ArrivalCartageComplete_CreatesDeliveredEventLog()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_ArrivalCartageComplete = ZDateTime.Now;
			AssertNotNull("DLV event log created", container.Logs.MostRecentLogByEventTime(Events.Delivered));
		}

		public void TestJC_ArrivalCartageComplete_IsEventDateProperty()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			AssertEquals(container.JC_ArrivalCartageCompleteInfo.Name, EventDatePropertyAttribute.FindPropertyInfos(container, Events.Delivered, EstimateActual.Actual).FirstOrDefault().Property.Name);
		}

		public void TestVGMFieldsReadOnly()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "";
			Assert(container.JC_GrossWeightVerificationTypeInfo.ReadOnly);
			Assert(container.JC_GrossWeightVerificationDateTimeInfo.ReadOnly);
			Assert(container.GrossWeightVerifiedByNameOrPKInfo.ReadOnly);
			container.JC_ContainerNum = "CONT2837";
			Assert(!container.JC_GrossWeightVerificationTypeInfo.ReadOnly);
			Assert(!container.JC_GrossWeightVerificationDateTimeInfo.ReadOnly);
			Assert(!container.GrossWeightVerifiedByNameOrPKInfo.ReadOnly);
		}

		public void TestGrossWeightOverridableRegardlessOfVerificationType()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			Assert(!container.JC_GrossWeightInfo.ReadOnly);
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			Assert(!container.JC_GrossWeightInfo.ReadOnly);
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			Assert(!container.JC_GrossWeightInfo.ReadOnly);
		}

		public void TestRequireCalculateJC_EmptyReturnedBy()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.Shipments.Add(shipment);
			var transport = shipment.Transports.AddNew("AUSYD", "NZAKL");

			var container = Factory.NewWithValidTestData<AgencyShipmentContainer>();
			container.JC_ContainerNum = "TTLU5498666";
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;
			Factory.Save();

			container.JC_FCLWharfGateOut = ZDateTime.Today.AddDays(7);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLUnloadFromVessel = ZDateTime.Today.AddDays(7);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			transport.JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(10);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);
		}

		#region Event Locations
		public void TestEventParameters_FLO_TopLevelPack()
		{
			foreach (var topLevelPackCargoType in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AssertTopLevelPackEventParameters(topLevelPackCargoType, Events.FreightLoadedCode, "|FAC=CTO|LOC=NLAMS");
			}
		}

		public void TestEventParameters_FUL_TopLevelPack()
		{
			foreach (var topLevelPackCargoType in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AssertTopLevelPackEventParameters(topLevelPackCargoType, Events.FreightUnloadedCode, "|FAC=CTO|LOC=AUSYD");
			}
		}

		void AssertTopLevelPackEventParameters(ZString containerMode, ZString eventCode, string expectedParameters)
		{
			ZString propertyName = ZString.Empty;
			switch (eventCode)
			{
				case Events.FreightLoadedCode:
					propertyName = JobContainerSchema.Constants.JC_FCLOnBoardVessel;
					break;
				case Events.FreightUnloadedCode:
					propertyName = JobContainerSchema.Constants.JC_FCLUnloadFromVessel;
					break;
				default:
					Fail("there's no property supporting " + eventCode);
					break;
			}

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			const string voyageNum = "001";
			var loader = new JobVoyage.Loader(Factory);
			var voyage = loader.Load(Constants.TransportModes.Sea, vessel.RV_Name, voyageNum, ZGuid.Empty);
			if (voyage == null)
			{
				voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = voyageNum;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
				voyage.GenerateSailings();
			}

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = containerMode;
			shipment.JS_JX = voyage.Sailings[0].PK;
			var vehicle = shipment.Vehicles.AddNew();
			vehicle.JC_ContainerNum = "1M8GDM9A_KP042788";
			Factory.Save();
			vehicle[propertyName] = new ZDateTime(2012, 12, 1);
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode);
			var expectedLogEntry = string.Format("{0}*{1}*{2}", eventCode, "2012-12-01T00:00:00", expectedParameters);
			AssertContainsExactElementsInAnyOrder(eventCode + " movement logs for " + containerMode, new[] { expectedLogEntry }, vehicle.Logs.Find(filter).Select(FormatLog));
		}

		#endregion
		#region JC_FCLWharfGateIn
		public void TestJC_FCLWharfGateIn_GetFirstGateInDate()
		{
			ZDateTime today = ZDateTime.Today;
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TTLU5498666";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage exportVoyage = Factory.New<JobVoyage>();
			exportVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			exportVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			exportVoyage.GenerateSailings();
			JobVoyage importVoyage = Factory.New<JobVoyage>();
			importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			importVoyage.GenerateSailings();
			OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
			aklDepot.OH_Code = "AKL";
			aklDepot.OH_RL_NKClosestPort = "NZAKL";
			OrgHeader bneDepot = Factory.NewWithValidTestData<OrgHeader>();
			bneDepot.OH_Code = "BNE";
			bneDepot.OH_RL_NKClosestPort = "AUBNE";
			OrgHeader melbourneDepot = Factory.NewWithValidTestData<OrgHeader>();
			melbourneDepot.OH_Code = "MEL";
			melbourneDepot.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = exportVoyage.Sailings[0].PK;
			Transport transport = bill.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = importVoyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TTLU5498666";
			container.JC_OA_ArrivalContainerYardAddress = bneDepot.MainAddress.PK;
			container.JC_OA_DepartureContainerYardAddress = melbourneDepot.MainAddress.PK;
			ContainerMovement gateMovement1 = stock.Movements.AddNew();
			gateMovement1.E9_JV = exportVoyage.PK;
			gateMovement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			gateMovement1.E9_MovementDate = today.AddDays(3);
			gateMovement1.E9_OA_Depot = aklDepot.MainAddress.PK;
			ContainerMovement gateMovement2 = stock.Movements.AddNew();
			gateMovement2.E9_JV = exportVoyage.PK;
			gateMovement2.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			gateMovement2.E9_MovementDate = today.AddDays(4);
			gateMovement2.E9_OA_Depot = bneDepot.MainAddress.PK;
			AssertEquals("Should be equal to first Wharf Gate In Date", gateMovement1.E9_MovementDate, container.JC_FCLWharfGateIn);
			gateMovement2.E9_MovementDate = today.AddDays(1);
			AssertEquals("Should be equal to first Wharf Gate In Date", gateMovement2.E9_MovementDate, container.JC_FCLWharfGateIn);
		}

		public void TestJC_FCLWharfGateInSetter_ContainerIsNotContainerised_GenerateGateInEvent()
		{
			EnsureGateInEventCreated(Core.Constants.ContainerModes.BreakBulk, true);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.Bulk, true);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.RollOnRollOff, true);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.Liquid, true);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.FCL, false);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.LCL, false);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.ULD, false);
			EnsureGateInEventCreated(Core.Constants.ContainerModes.Loose, false);
			EnsureGateInEventCreated("", false);
		}

		public void EnsureGateInEventCreated(string containerMode, bool expectEvent)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = containerMode;
			shipment.Transports.AddNew("AUSYD", "USNYC");
			shipment.Transports.AddNew("USNYC", "UAIEV");
			var container = shipment.BookedContainers.AddNew();
			container.JC_ContainerMode = containerMode;
			container.JC_FCLWharfGateIn = 5.DaysAgo();
			var log = container.Logs.MostRecentLogByEventTime(Events.GateIn);
			AssertEquals(string.Format("Log with {0} event when container mode is {1} has been created", Events.GateIn.Code, containerMode), expectEvent, log != null);
			if (expectEvent)
			{
				var cTO = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
				AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
				AssertEquals("Free text", "", log.ReferenceFreeText);
				AssertEquals("Location", "AUSYD", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location]);
				AssertEquals("Facility", cTO, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility]);
			}
		}

		#endregion
		#region JC_FCLWharfGateOut
		public void TestJC_FCLWharfGateOut_GetLastGateOutDate()
		{
			ZDateTime today = ZDateTime.Today;
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TTLU5498666";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage exportVoyage = Factory.New<JobVoyage>();
			exportVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			exportVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			exportVoyage.GenerateSailings();
			JobVoyage importVoyage = Factory.New<JobVoyage>();
			importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			importVoyage.GenerateSailings();
			OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
			aklDepot.OH_Code = "AKL";
			aklDepot.OH_RL_NKClosestPort = "NZAKL";
			OrgHeader bneDepot = Factory.NewWithValidTestData<OrgHeader>();
			bneDepot.OH_Code = "BNE";
			bneDepot.OH_RL_NKClosestPort = "AUBNE";
			OrgHeader melbourneDepot = Factory.NewWithValidTestData<OrgHeader>();
			melbourneDepot.OH_Code = "MEL";
			melbourneDepot.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = exportVoyage.Sailings[0].PK;
			Transport transport = bill.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = importVoyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TTLU5498666";
			container.JC_OA_ArrivalContainerYardAddress = bneDepot.MainAddress.PK;
			container.JC_OA_DepartureContainerYardAddress = melbourneDepot.MainAddress.PK;
			ContainerMovement gateMovement1 = stock.Movements.AddNew();
			gateMovement1.E9_JV = exportVoyage.PK;
			gateMovement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			gateMovement1.E9_MovementDate = today.AddDays(3);
			gateMovement1.E9_OA_Depot = aklDepot.MainAddress.PK;
			ContainerMovement gateMovement2 = stock.Movements.AddNew();
			gateMovement2.E9_JV = exportVoyage.PK;
			gateMovement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			gateMovement2.E9_MovementDate = today.AddDays(4);
			gateMovement2.E9_OA_Depot = bneDepot.MainAddress.PK;
			AssertEquals("Should be equal to last Wharf Gate Out Date", gateMovement2.E9_MovementDate, container.JC_FCLWharfGateOut);
			gateMovement1.E9_MovementDate = today.AddDays(5);
			AssertEquals("Should be equal to last Wharf Gate Out Date", gateMovement1.E9_MovementDate, container.JC_FCLWharfGateOut);
		}

		public void TestJC_FCLWharfGateOutSetter_ContainerIsNotContainerised_GenerateGateOutEvent()
		{
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.BreakBulk, true);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.Bulk, true);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.RollOnRollOff, true);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.Liquid, true);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.FCL, false);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.LCL, false);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.ULD, false);
			EnsureGateOutEventCreated(Core.Constants.ContainerModes.Loose, false);
			EnsureGateOutEventCreated("", false);
		}

		public void EnsureGateOutEventCreated(string containerMode, bool expectEvent)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = containerMode;
			shipment.Transports.AddNew("AUSYD", "USNYC");
			shipment.Transports.AddNew("USNYC", "UAIEV");
			var container = shipment.BookedContainers.AddNew();
			container.JC_ContainerMode = containerMode;
			container.JC_FCLWharfGateOut = 5.DaysAgo();
			var log = container.Logs.MostRecentLogByEventTime(Events.GateOut);
			AssertEquals(string.Format("Log with {0} event when container mode is {1} has been created", Events.GateOut.Code, containerMode), expectEvent, log != null);
			if (expectEvent)
			{
				var cTO = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
				AssertEquals("Event date", 5.DaysAgo(), log.SL_EventTime);
				AssertEquals("Location", "UAIEV", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location]);
				AssertEquals("Facility", cTO, log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility]);
			}
		}

		#endregion
		#region Implementation

		void AssertCannotDelete(string message, ICanDelete deleteable, string reason)
		{
			AssertEquals(message, false, deleteable.CanDelete);
			AssertEquals(message, reason, deleteable.ReasonForNotAbleToDelete);
		}

		void AssertCanDelete(string message, ICanDelete deleteable)
		{
			AssertEquals(message, true, deleteable.CanDelete);
			AssertEquals(message, "", deleteable.ReasonForNotAbleToDelete);
		}

		EIDOMessage SendEIDOMessage(AgencyShipmentContainer container, EIDOMessageFunction function, ZDateTime now, int offset)
		{
			EIDOMessage message = EIDOMessage.New(container, function, "text");
			message.EM_SystemCreateTimeUtc = now.AddMinutes(offset);
			return message;
		}

		void ResponseMessage(EIDOMessage sentMessage, EIDOResponseType responseType)
		{
			EIDOMessage message = Factory.New<EIDOMessage>();
			message.EM_ApplicationCode = EIDOMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EIDOMessage.Direction.Receive;
			message.EM_MessageText = "text";
			message.EM_LinkTable = sentMessage.EM_LinkTable;
			message.EM_LinkUniqueID = sentMessage.EM_LinkUniqueID;
			message.EM_Status = EIDOMessage.Status.Recognised;
			switch (responseType)
			{
				case EIDOResponseType.Accepted:
					sentMessage.EM_Status = EIDOMessage.Status.Received;
					break;
				case EIDOResponseType.Received:
					sentMessage.EM_Status = EIDOMessage.Status.Acknowledged;
					break;
				case EIDOResponseType.Rejected:
					sentMessage.EM_Status = EIDOMessage.Status.Rejected;
					break;
			}
		}

		string FormatLog(StmALog log)
		{
			return string.Format("{0}*{1}*{2}", log.SL_SE_NKEvent, log.SL_EventTime.ToISO8601String(), log.SL_Reference);
		}
		#endregion
	}
}
