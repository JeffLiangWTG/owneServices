using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;
using LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public partial class ShipmentDataObjectWriterTest : BaseShipmentDataObjectWriterTest
	{
		public void TestCompanyTariffLevelOverride()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_CompanyTariffLevelOverride = 1;
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("CompanyTariffLevelOverride should be exported", (ZByte)1, shipmentData.CompanyTariffLevelOverride);
		}

		[TestDate(2021, 12, 1)]
		public void TestLocalProcessingConditionalExportedToShipmentPenalties()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();

			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();

			var durationBase = ZDateTime.DefaultDurationEpoch;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.Shipments.RemoveAndDeleteAll();
			{
				var shipment = consol.Shipments.AddNew();
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				var container = consol.Containers.AddNew();
				shipment.OuterPackLines.AddNew();

				shipment.DocsAndCartage.JP_FCLPickupDetentionFreeDays = 1;
				shipment.DocsAndCartage.JP_FCLPickupDetentionDays = 2;
				shipment.DocsAndCartage.JP_FCLPickupDetentionCharge = 3;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionFreeDays = 4;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 5;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge = 6;
				shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 7;
				shipment.DocsAndCartage.JP_LCLAirStorageCharge = 8;

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), false, false);
				var shipmentData = writer.GetDataObject(shipment);
				var penalties = shipmentData.ContainerCollection.First().ContainerPenaltyCollection;
				CombineAssertions(() =>
				{
					AssertEquals((ZByte)1, shipmentData.LocalProcessing.FCLPickupDetentionFreeDays);
					AssertEquals((ZByte)2, shipmentData.LocalProcessing.FCLPickupDetentionDays);
					AssertEquals((ZDecimal)3, shipmentData.LocalProcessing.FCLPickupDetentionCharge);
					AssertEquals((ZByte)4, shipmentData.LocalProcessing.FCLDeliveryDetentionFreeDays);
					AssertEquals((ZByte)5, shipmentData.LocalProcessing.FCLDeliveryDetentionDays);
					AssertEquals((ZDecimal)6, shipmentData.LocalProcessing.FCLDeliveryDetentionCharge);
					AssertEquals((ZByte)7, shipmentData.LocalProcessing.LCLAirStorageDaysOrHours);
					AssertEquals((ZDecimal)8, shipmentData.LocalProcessing.LCLAirStorageCharge);
					AssertEquals(3, penalties.Count);
					Assert(penalties.Any(x => x.ProcessType.Code == (ZString?)"PIC" && x.PenaltyType.Code == (ZString?)"DET" && x.CreditorType.Code == (ZString?)"CAR" && x.FreeTime == durationBase.AddDays(1) && x.Duration == durationBase.AddDays(2) && x.PerUnitCost == (ZDecimal)3));
					Assert(penalties.Any(x => x.ProcessType.Code == (ZString?)"DLV" && x.PenaltyType.Code == (ZString?)"DET" && x.CreditorType.Code == (ZString?)"CAR" && x.FreeTime == durationBase.AddDays(4) && x.Duration == durationBase.AddDays(5) && x.PerUnitCost == (ZDecimal)6));
					Assert(penalties.Any(x => x.ProcessType.Code == (ZString?)"DLV" && x.PenaltyType.Code == (ZString?)"STO" && x.CreditorType.Code == (ZString?)"CAR" && x.FreeTime == ZDateTime.Empty && x.Duration == durationBase.AddDays(7) && x.PerUnitCost == (ZDecimal)8));
				});
			}

			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.Shipments.RemoveAndDeleteAll();
			{
				var shipment = consol.Shipments.AddNew();
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				var container = consol.Containers.AddNew();
				shipment.OuterPackLines.AddNew();

				shipment.DocsAndCartage.JP_FCLPickupDetentionFreeDays = 1;
				shipment.DocsAndCartage.JP_FCLPickupDetentionDays = 2;
				shipment.DocsAndCartage.JP_FCLPickupDetentionCharge = 3;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionFreeDays = 4;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 5;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge = 6;
				shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 7;
				shipment.DocsAndCartage.JP_LCLAirStorageCharge = 8;

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), false, false);
				var shipmentData = writer.GetDataObject(shipment);
				var penalties = shipmentData.ContainerCollection.First().ContainerPenaltyCollection;

				CombineAssertions(() =>
				{
					AssertEquals((ZByte)1, shipmentData.LocalProcessing.FCLPickupDetentionFreeDays);
					AssertEquals((ZByte)2, shipmentData.LocalProcessing.FCLPickupDetentionDays);
					AssertEquals((ZDecimal)3, shipmentData.LocalProcessing.FCLPickupDetentionCharge);
					AssertEquals((ZByte)4, shipmentData.LocalProcessing.FCLDeliveryDetentionFreeDays);
					AssertEquals((ZByte)5, shipmentData.LocalProcessing.FCLDeliveryDetentionDays);
					AssertEquals((ZDecimal)6, shipmentData.LocalProcessing.FCLDeliveryDetentionCharge);
					AssertEquals((ZByte)7, shipmentData.LocalProcessing.LCLAirStorageDaysOrHours);
					AssertEquals((ZDecimal)8, shipmentData.LocalProcessing.LCLAirStorageCharge);

					AssertEquals(0, penalties?.Count ?? 0);
				});
			}
		}

		public void TestOutboundUSXML_TransitWarehouse()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.New<OrgHeader>();
				var consignee = Factory.New<OrgHeader>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "SHIPME";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CNSHA";
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;

				var refC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "C10011991";
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "BAFK2337238";
				container1.JC_ContainerMode = Constants.ContainerModes.FCL;
				container1.JC_RC = refC.PK;
				container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
				container1.JC_ContainerCount = 101;
				container1.JC_TotalLength = 102;
				container1.JC_TotalHeight = 103;
				container1.JC_TotalUnitOfMeasure = Constants.Length.Metres;
				container1.JC_TareWeight = 104;
				container1.JC_DunnageWeight = 105;
				container1.JC_GrossWeight = 106;
				container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				container1.JC_VolumeCapacity = 107;
				container1.JC_VolumeCapacityUQ = Constants.Volume.CubicMetres;
				container1.JC_IsSealOk = true;
				container1.JC_AdditionalSealNum = "SEAL108";
				container1.JC_AdditionalSealParty = "CRD";
				container1.JC_AirVentFlow = 109;
				container1.JC_AirVentFlowRateUnit = "MQH";
				container1.JC_HumidityPercent = 15;
				container1.JC_IsControlledAtmosphere = true;
				container1.JC_RefrigGeneratorID = "RG11081";
				container1.JC_SetPointTemp = 11.01;
				container1.JC_SetPointTempUnit = "C";
				container1.JC_TempRecorderSerialNo = "111";
				container1.JC_IsDamaged = true;
				container1.JC_IsEmptyContainer = false;
				container1.JC_IsShipperOwned = false;
				container1.JC_ContainerQuality = "GEN";
				container1.JC_ContainerStatus = "AVL";
				container1.JC_HarmonisedCode = "112";
				container1.JC_RH_NKContainerCommodityCode = "IRON";
				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "QRBI7004190";
				container2.JC_ContainerMode = Constants.ContainerModes.FCL;
				container2.JC_RC = refC.PK;
				container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
				container2.JC_ContainerCount = 201;
				container2.JC_TotalLength = 202;
				container2.JC_TotalHeight = 203;
				container2.JC_TotalUnitOfMeasure = Constants.Length.Metres;
				container2.JC_TareWeight = 204;
				container2.JC_DunnageWeight = 205;
				container2.JC_GrossWeight = 206;
				container2.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				container2.JC_VolumeCapacity = 207;
				container2.JC_VolumeCapacityUQ = Constants.Volume.CubicMetres;
				container2.JC_IsSealOk = false;
				container2.JC_AdditionalSealNum = "SEAL208";
				container2.JC_AdditionalSealParty = "CRD";
				container2.JC_AirVentFlow = 209;
				container2.JC_AirVentFlowRateUnit = "MQH";
				container2.JC_HumidityPercent = 25;
				container2.JC_IsControlledAtmosphere = true;
				container2.JC_RefrigGeneratorID = "RG21081";
				container2.JC_SetPointTemp = 21.01;
				container2.JC_SetPointTempUnit = "C";
				container2.JC_TempRecorderSerialNo = "211";
				container2.JC_IsDamaged = true;
				container2.JC_IsEmptyContainer = false;
				container2.JC_IsShipperOwned = false;
				container2.JC_ContainerQuality = "GEN";
				container2.JC_ContainerStatus = "AVL";
				container2.JC_HarmonisedCode = "212";
				container2.JC_RH_NKContainerCommodityCode = "GEN";

				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;

				var packLine11 = shipment.OuterPackLines.AddNew();
				packLine11.JL_PackageCount = 1;
				packLine11.JL_OutturnedWeight = 12m;
				packLine11.JL_OutturnedVolume = 13m;
				packLine11.JL_OutturnedWidth = 14;
				packLine11.JL_OutturnedLength = 117;
				packLine11.JL_OutturnedHeight = 118;
				packLine11.JL_OutturnComment = "119";
				packLine11.JL_Outturn = 15;
				packLine11.JL_ActualWeight = 16m;
				packLine11.JL_ActualWeightUQ = "KG";
				packLine11.JL_ActualVolume = 17m;
				packLine11.JL_ActualVolumeUQ = "M3";
				packLine11.JL_PackageCount = 18;
				packLine11.JL_Pillaged = 19;
				packLine11.JL_Damaged = 110;
				packLine11.JL_LoadingMeters = 111;
				packLine11.JL_JC = container1.PK;
				packLine11.JL_MarksAndNumbers = "112";
				packLine11.JL_RefNumber = "PCK000113";
				packLine11.JL_Width = 114;
				packLine11.JL_Description = "DESC 115";
				packLine11.JL_HarmonisedCode = "116";
				packLine11.JL_RH_NKCommodityCode = "GEN";
				packLine11.JL_ImportRefNumber = "IMP120";
				packLine11.JL_ExportRefNumber = "IMP121";
				packLine11.JL_RequiresTemperatureControl = true;
				packLine11.JL_RequiredTemperatureMinimum = 1.22;
				packLine11.JL_RequiredTemperatureMaximum = 123;
				packLine11.JL_RequiredTemperatureUnit = "C";
				packLine11.JL_Height = 124;
				packLine11.JL_JC = container1.PK;
				var pkg11 = packLine11.PkgPackageCollection.AddNew();
				pkg11.KP_F3_NKPackType = "CNT";
				pkg11.KP_DimensionUQ = "M";
				pkg11.KP_Height = 31m;
				pkg11.KP_Length = 32m;
				pkg11.KP_PackageID = "PACKAGE33";
				pkg11.KP_PackageQty = 34;
				pkg11.KP_VolumeUQ = "M3";
				pkg11.KP_Weight = 35m;
				pkg11.KP_WeightUQ = "T";
				pkg11.KP_Width = 36m;
				pkg11.KP_MarksAndNumbers = "MARK37";
				pkg11.KP_TransportRef = "TRANSPORT REF38";
				pkg11.KP_GoodsDescription = "GOODS DESC39";
				pkg11.KP_HSCode = "HARMON CODE310";
				pkg11.KP_Volume = 311m;
				pkg11.KP_ExternalReference = "PLI11";
				pkg11.KP_KJ_ParentPackageJob = packageJob.PK;
				var pkg12 = packLine11.PkgPackageCollection.AddNew();
				pkg12.KP_F3_NKPackType = "CTN";
				pkg12.KP_DimensionUQ = "IN";
				pkg12.KP_Height = 41m;
				pkg12.KP_Length = 42m;
				pkg12.KP_PackageID = "PACKAGE43";
				pkg12.KP_PackageQty = 44;
				pkg12.KP_VolumeUQ = "M3";
				pkg12.KP_Weight = 45m;
				pkg12.KP_WeightUQ = "T";
				pkg12.KP_Width = 46m;
				pkg12.KP_MarksAndNumbers = "MARK47";
				pkg12.KP_TransportRef = "TRANSPORT REF48";
				pkg12.KP_GoodsDescription = "GOODS DESC49";
				pkg12.KP_HSCode = "HARMON CODE410";
				pkg12.KP_Volume = 411m;
				pkg12.KP_ExternalReference = "PLI12";
				pkg12.KP_KJ_ParentPackageJob = packageJob.PK;
				var packLine21 = shipment.OuterPackLines.AddNew();
				packLine21.JL_PackageCount = 2;
				packLine21.JL_OutturnedWeight = 22m;
				packLine21.JL_OutturnedVolume = 23m;
				packLine21.JL_OutturnedWidth = 24;
				packLine21.JL_OutturnedLength = 217;
				packLine21.JL_OutturnedHeight = 218;
				packLine21.JL_OutturnComment = "219";
				packLine21.JL_Outturn = 25;
				packLine21.JL_ActualWeight = 26m;
				packLine21.JL_ActualWeightUQ = "KG";
				packLine21.JL_ActualVolume = 27m;
				packLine21.JL_ActualVolumeUQ = "M3";
				packLine21.JL_PackageCount = 28;
				packLine21.JL_Pillaged = 29;
				packLine21.JL_Damaged = 210;
				packLine21.JL_LoadingMeters = 211;
				packLine21.JL_JC = container1.PK;
				packLine21.JL_MarksAndNumbers = "212";
				packLine21.JL_RefNumber = "PCK000213";
				packLine21.JL_Width = 214;
				packLine21.JL_Description = "DESC 215";
				packLine21.JL_HarmonisedCode = "216";
				packLine21.JL_RH_NKCommodityCode = "GEN";
				packLine21.JL_ImportRefNumber = "IMP220";
				packLine21.JL_ExportRefNumber = "IMP221";
				packLine21.JL_RequiresTemperatureControl = false;
				packLine21.JL_RequiredTemperatureMinimum = 2.22;
				packLine21.JL_RequiredTemperatureMaximum = 223;
				packLine21.JL_RequiredTemperatureUnit = "C";
				packLine21.JL_Height = 224;
				packLine21.JL_JC = container2.PK;
				var pkg21 = packLine21.PkgPackageCollection.AddNew();
				pkg21.KP_F3_NKPackType = "CNT";
				pkg21.KP_DimensionUQ = "M";
				pkg21.KP_Height = 51m;
				pkg21.KP_Length = 52m;
				pkg21.KP_PackageID = "PACKAGE53";
				pkg21.KP_PackageQty = 54;
				pkg21.KP_VolumeUQ = "M3";
				pkg21.KP_Weight = 55m;
				pkg21.KP_WeightUQ = "T";
				pkg21.KP_Width = 56m;
				pkg21.KP_MarksAndNumbers = "MARK57";
				pkg21.KP_TransportRef = "TRANSPORT REF58";
				pkg21.KP_GoodsDescription = "GOODS DESC59";
				pkg21.KP_HSCode = "HARMON CODE510";
				pkg21.KP_Volume = 511m;
				pkg21.KP_ExternalReference = "PLI21";
				pkg21.KP_KJ_ParentPackageJob = packageJob.PK;
				var pkg22 = packLine21.PkgPackageCollection.AddNew();
				pkg22.KP_F3_NKPackType = "CTN";
				pkg22.KP_DimensionUQ = "IN";
				pkg22.KP_Height = 61m;
				pkg22.KP_Length = 62m;
				pkg22.KP_PackageID = "PACKAGE63";
				pkg22.KP_PackageQty = 64;
				pkg22.KP_VolumeUQ = "M3";
				pkg22.KP_Weight = 65m;
				pkg22.KP_WeightUQ = "T";
				pkg22.KP_Width = 66m;
				pkg22.KP_MarksAndNumbers = "MARK67";
				pkg22.KP_TransportRef = "TRANSPORT REF68";
				pkg22.KP_GoodsDescription = "GOODS DESC69";
				pkg22.KP_HSCode = "HARMON CODE610";
				pkg22.KP_Volume = 611m;
				pkg22.KP_ExternalReference = "PLI22";
				pkg22.KP_KJ_ParentPackageJob = packageJob.PK;

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				CombineAssertions(() =>
				{
					var packLines = shipmentData.SubShipmentCollection[0].PackingLineCollection;
					AssertEquals(4, packLines.Count);

					AssertEquals("PACKAGE33", packLines[0].ReferenceNumber);
					AssertEquals(31m, packLines[0].Height);
					AssertEquals(1, packLines[0].ContainerLink);
					AssertEquals("BAFK2337238", packLines[0].ContainerNumber);
					AssertEquals("PLI11", packLines[0].PackingLineID);

					AssertEquals("PACKAGE43", packLines[1].ReferenceNumber);
					AssertEquals(41m, packLines[1].Height);
					AssertEquals(1, packLines[1].ContainerLink);
					AssertEquals("BAFK2337238", packLines[1].ContainerNumber);
					AssertEquals("PLI12", packLines[1].PackingLineID);

					AssertEquals("PACKAGE53", packLines[2].ReferenceNumber);
					AssertEquals(51m, packLines[2].Height);
					AssertEquals(2, packLines[2].ContainerLink);
					AssertEquals("QRBI7004190", packLines[2].ContainerNumber);
					AssertEquals("PLI21", packLines[2].PackingLineID);

					AssertEquals("PACKAGE63", packLines[3].ReferenceNumber);
					AssertEquals(61m, packLines[3].Height);
					AssertEquals(2, packLines[3].ContainerLink);
					AssertEquals("QRBI7004190", packLines[3].ContainerNumber);
					AssertEquals("PLI22", packLines[3].PackingLineID);
				});

				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CNR, shipment)), true, true);
				shipmentData = writer.GetDataObject(shipment);
				CombineAssertions(() =>
				{
					var packLines = shipmentData.SubShipmentCollection[0].PackingLineCollection;
					AssertEquals(2, packLines.Count);
					AssertEquals("PCK000113", packLines[0].ReferenceNumber);
					AssertEquals(124m, packLines[0].Height);
					AssertEquals(1, packLines[0].ContainerLink);
					AssertEquals("BAFK2337238", packLines[0].ContainerNumber);

					AssertEquals("PCK000213", packLines[1].ReferenceNumber);
					AssertEquals(224m, packLines[1].Height);
					AssertEquals(2, packLines[1].ContainerLink);
					AssertEquals("QRBI7004190", packLines[1].ContainerNumber);
				});
			}
		}

		public void TestHAWBHandlingInformationExtraText()
		{
			using (FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"\"{First}\"\" == \"\"1\"\").First()>"))
			{
				var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
				shipmentBO.JS_TransportMode = TransportModes.Air;
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				AssertExceptionThrown<DataObjectValidationException>("Should not throw ExportAWBHeaderReplaceMacrosException", () => writer.GetDataObject(shipmentBO));
			}
		}

		#region Multiple consols

		public void Test2011Schema_MultipleConsols_NonCurrentOnesAreExportedToParentShipmentCollection()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;

			var container11 = consol1.Containers.AddNew();
			container11.JC_ContainerNum = "CON11";

			var container12 = consol1.Containers.AddNew();
			container12.JC_ContainerNum = "CON12";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(5);

			var container21 = consol2.Containers.AddNew();
			container21.JC_ContainerNum = "CON21";

			var container22 = consol2.Containers.AddNew();
			container22.JC_ContainerNum = "CON22";

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(10);

			var container31 = consol3.Containers.AddNew();
			container31.JC_ContainerNum = "CON31";

			var container32 = consol3.Containers.AddNew();
			container32.JC_ContainerNum = "CON32";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "SHIPME";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol1, container11);
			packline1.SetContainer(consol2, container21);
			packline1.SetContainer(consol3, container31);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol1, container12);
			packline2.SetContainer(consol2, container22);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.PCA, shipment)), true, true);
			var shipmentData = writer.GetDataObject(shipment);

			CombineAssertions(() =>
			{
				AssertEquals("Pickup recipient role: Earliest consol used as current one",
					"ForwardingConsol [CONSOL1], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());

				AssertContainsExactElementsInAnyOrder("Current consol has it's containers exported",
					new ZString[] { "CON11", "CON12", },
					shipmentData.ContainerCollection.Select(container => container.ContainerNumber));

				var subShipment = shipmentData.SubShipmentCollection.Single();

				AssertContainsExactElementsInAnyOrder("ShipmentBO exported as SubShipment and does have all containers it's packed into",
					new ZString[] { "CON11", "CON12", "CON21", "CON22", "CON31" },
					subShipment.ContainerCollection.Select(container => container.ContainerNumber));

				var parentConsols = subShipment.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("Non-current consols exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL2]", "ForwardingConsol [CONSOL3]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));

				AssertEquals("Non-current consols does not have subshipments", true, parentConsols.All(parentConsol => parentConsol.SubShipmentCollection == null));
				AssertEquals("Non-current consols does not have containers", true, parentConsols.All(parentConsol => parentConsol.ContainerCollection == null));
			});

			writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, shipment)), true, true);
			shipmentData = writer.GetDataObject(shipment);

			CombineAssertions(() =>
			{
				AssertEquals("Delivery recipient role: Latest consol used as current one",
					"ForwardingConsol [CONSOL3], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());

				AssertContainsExactElementsInAnyOrder("Current consol has it's containers exported",
					new ZString[] { "CON31", "CON32", },
					shipmentData.ContainerCollection.Select(container => container.ContainerNumber));

				var subShipment = shipmentData.SubShipmentCollection.Single();

				AssertContainsExactElementsInAnyOrder("ShipmentBO exported as SubShipment and does have all containers it's packed into",
					new ZString[] { "CON11", "CON12", "CON21", "CON22", "CON31" },
					subShipment.ContainerCollection.Select(container => container.ContainerNumber));

				var parentConsols = subShipment.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("Non-current consols exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL1]", "ForwardingConsol [CONSOL2]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));

				AssertEquals("Non-current consols does not have subshipments", true, parentConsols.All(parentConsol => parentConsol.SubShipmentCollection == null));
				AssertEquals("Non-current consols does not have containers", true, parentConsols.All(parentConsol => parentConsol.ContainerCollection == null));
			});
		}

		public void Test2011Schema_MultipleConsols_CurrentConsolMatchingFallbacks()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2017, 5, 10);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_UniqueConsignRef = "CONSOL2";

			var transport21 = consol2.Transports.AddNew();
			transport21.JW_TransportMode = Constants.TransportModes.Air;
			transport21.JW_VoyageFlight = "NZ200";
			transport21.JW_ETD = new ZDateTime(2017, 5, 11);
			transport21.JW_ETA = new ZDateTime(2017, 5, 12);

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2017, 5, 15);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "SHIPME";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			CombineAssertions("First level: matching by explicitly specified Pickup/Delivery recipient role", () =>
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				AssertEquals("Delivery recipient role: Latest consol used as current one",
					"ForwardingConsol [CONSOL3], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());

				var subShipment = shipmentData.SubShipmentCollection.Single();
				var parentConsols = subShipment.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("Non-current consols exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL1]", "ForwardingConsol [CONSOL2]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));
			});

			CombineAssertions("Second level: matching by event", () =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					triggeringEvent.SL_Table = "JobConsol";
					triggeringEvent.SL_Parent = consol2.PK;
				}

				var actionInfo = new ActionInfo(null, shipment);
				actionInfo.TriggeringEvent = triggeringEvent;

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(actionInfo), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				AssertEquals("Matched by event parent details",
					"ForwardingConsol [CONSOL2], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());

				var subShipment = shipmentData.SubShipmentCollection.Single();
				var parentConsols = subShipment.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("Non-current consols exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL1]", "ForwardingConsol [CONSOL3]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));
			});

			CombineAssertions("Third level: using default consol - earliest one", () =>
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				AssertEquals("Earliest consol used as current one",
					"ForwardingConsol [CONSOL1], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());

				var subShipment = shipmentData.SubShipmentCollection.Single();
				var parentConsols = subShipment.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("Non-current consols exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL2]", "ForwardingConsol [CONSOL3]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void Test2011Schema_MultipleConsols_CurrentConsolMatchingByEvent_Fallbacks()
		{
			var today = ZDateTime.Today;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.Transports.MostInterestingTransport.JW_ETD = today.AddDays(-10);
			consol1.Transports.MostInterestingTransport.JW_ETA = today.AddDays(-5);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.Transports.MostInterestingTransport.JW_ETD = today;
			consol2.Transports.MostInterestingTransport.JW_ETA = today.AddDays(5);

			var transport22 = consol2.Transports.AddNew();
			transport22.JW_TransportMode = Constants.TransportModes.Air;
			transport22.JW_RL_NKLoadPort = "NZWLG";
			transport22.JW_VoyageFlight = "NZ200";
			transport22.JW_ETD = today;

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.Transports.MostInterestingTransport.JW_ETD = today.AddDays(10);
			consol3.Transports.MostInterestingTransport.JW_ETA = today.AddDays(15);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_UniqueConsignRef = "SHIPME";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: LatestConsol", consol3, shipment.Consols.GetLatestConsol());

			Action<string, ForwardingConsol, Action<StmALog>> assertMatching = (message, expectedParentConsol, setupTriggeringEvent) =>
			{
				var triggeringEvent = Factory.New<StmALog>();
				using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
				{
					setupTriggeringEvent(triggeringEvent);
				}

				var actionInfo = new ActionInfo(null, shipment);
				actionInfo.TriggeringEvent = triggeringEvent;

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(actionInfo), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				AssertEquals("Matched by event parent details",
					$"ForwardingConsol [{expectedParentConsol.JK_UniqueConsignRef}], ForwardingShipment [SHIPME]",
					shipmentData.DataContext.GetDataSources());
			};

			assertMatching("Matching by explicit event parent", consol2,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Table = "JobConsol";
					triggeringEvent.SL_Parent = consol2.PK;
				});

			assertMatching("Matching by reference: by facility", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|FAC=CNE";
				});

			assertMatching("Matching by reference: by location on consol's legs", consol2,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|LOC=NZWLG";
				});

			assertMatching("Matching by reference: by flight details", consol2,
				(triggeringEvent) =>
				{
					string todayAsString = today.ToString("dd-MMM-yy");

					triggeringEvent.SL_SE_NKEvent = Events.DepartureCode;
					triggeringEvent.SL_Reference = $"|VFL=NZ200|FDT={todayAsString}";
				});

			assertMatching("Matching by reference: by location on shipment ports", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|LOC=DEHAM";
				});

			assertMatching("Matching by reference: by location country on shipment ports", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|LOC=DEBRE";
				});

			assertMatching("Matching by reference: by location on consol ports", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|LOC=USLAX";
				});

			assertMatching("Matching by reference: by location country on consol ports", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_Reference = "|LOC=USSEA";
				});

			assertMatching("Matching by event time", consol3,
				(triggeringEvent) =>
				{
					triggeringEvent.SL_EventTime = today.AddDays(13);
				});
		}

		public void Test2011Schema_SubShipment_MultipleConsols()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(5);

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_UniqueConsignRef = "CONSOL3";
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(10);

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_TransportMode = Constants.TransportModes.Sea;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_UniqueConsignRef = "MASTERSHIP";

			masterShipment.Consols.Add(consol1);
			masterShipment.Consols.Add(consol2);

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_TransportMode = Constants.TransportModes.Sea;
			subShipment.JS_UniqueConsignRef = "SUBSHIP";
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			subShipment.Consols.Add(consol3);

			AssertContainsExactElementsInAnyOrder("Pre-requisite: master shipment consols",
				new[] { consol1, consol2 }, masterShipment.Consols);

			AssertContainsExactElementsInAnyOrder("Pre-requisite: sub shipment consols",
				new[] { consol1, consol2, consol3 }, subShipment.Consols);

			AssertEquals("Pre-requisite: LatestConsol on master shipment", consol2, masterShipment.Consols.GetLatestConsol());
			AssertEquals("Pre-requisite: LatestConsol on sub shipment", consol3, subShipment.Consols.GetLatestConsol());

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DCA, subShipment)), true, true);
			var shipmentData = writer.GetDataObject(subShipment);

			CombineAssertions(() =>
			{
				AssertEquals("Delivery recipient role: LatestConsol from MASTER shipment is used as current one",
					"ForwardingConsol [CONSOL2], ForwardingShipment [SUBSHIP]", shipmentData.DataContext.GetDataSources());

				var masterShipmentData = shipmentData.SubShipmentCollection.Single();
				AssertEquals("Master shipment in first level SubShipmentCollection",
					"ForwardingShipment [MASTERSHIP]", masterShipmentData.DataContext.GetDataSources());

				var subShipmentData = masterShipmentData.SubShipmentCollection.Single();
				AssertEquals("Sub shipment in second level SubShipmentCollection (on master shipment)",
					"ForwardingShipment [SUBSHIP]", subShipmentData.DataContext.GetDataSources());

				var parentConsols = subShipmentData.ParentShipmentCollection;

				AssertContainsExactElementsInAnyOrder("All non-current consols from sub shipment are exported to ParentShipmentCollection",
					new[] { "ForwardingConsol [CONSOL1]", "ForwardingConsol [CONSOL3]" },
					parentConsols.Select(parentConsol => parentConsol.DataContext.GetDataSources()));
			});
		}

		public void Test2011Schema_SubShipment_SingleConsol()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CONSOL1";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CONSOL2";

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_TransportMode = Constants.TransportModes.Sea;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_UniqueConsignRef = "MASTERSHIP";

			masterShipment.Consols.Add(consol1);

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_TransportMode = Constants.TransportModes.Sea;
			subShipment.JS_UniqueConsignRef = "SUBSHIP";
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.SaveForTesting();

			// Replicating adhoc issue when subshipment had a single consol different to a master's single consol

			subShipment.Consols.Remove(consol1);
			subShipment.Consols.Add(consol2);

			AssertContainsExactElementsInAnyOrder("Pre-requisite: master shipment consols",
				new[] { consol1 }, masterShipment.Consols);

			AssertContainsExactElementsInAnyOrder("Pre-requisite: sub shipment consols",
				new[] { consol2 }, subShipment.Consols);

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, subShipment)), true, true);
			var shipmentData = writer.GetDataObject(subShipment);

			AssertEquals("Consol from MASTER shipment is used as current one",
				"ForwardingConsol [CONSOL1], ForwardingShipment [SUBSHIP]", shipmentData.DataContext.GetDataSources());
		}

		public void TestMultipleConsols_DefaultConsol_IsChosenDependingOnUniversalSchemaVersion()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_UniqueConsignRef = "CONSOL1";
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;

			var container11 = consol1.Containers.AddNew();
			container11.JC_ContainerNum = "CONTCONSOL1";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_UniqueConsignRef = "CONSOL2";
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today.AddDays(5);

			var container21 = consol2.Containers.AddNew();
			container21.JC_ContainerNum = "CONTCONSOL2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_UniqueConsignRef = "SHIPME";

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol1, container11);
			packline1.SetContainer(consol2, container21);

			AssertEquals("Pre-requisite: EarliestConsol", consol1, shipment.Consols.GetEarliestConsol());
			AssertEquals("Pre-requisite: DepartureConsol", consol2, shipment.DepartureConsol);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				AssertEquals("Schema 2011_11: Earliest consol used as a default current one",
					"ForwardingConsol [CONSOL1], ForwardingShipment [SHIPME]", shipmentData.DataContext.GetDataSources());
			}

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				// Namespace_2012_11 does not include current consol as top level shipment
				// But we could figure out which consol was used through packinglines-containers

				var packingLines = shipmentData.PackingLineCollection;

				AssertEquals("PackingLines from current consol", 1, packingLines.Count);
				AssertEquals("Schema 2012_11: Departure consol used as a default current one",
					"CONTCONSOL2", packingLines[0].ContainerNumber);
			}

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.PCA, shipment)), true, true);
				var shipmentData = writer.GetDataObject(shipment);

				var packingLines = shipmentData.PackingLineCollection;

				AssertEquals("PackingLines from current consol", 1, packingLines.Count);
				AssertEquals("Schema 2012_11: Departure consol always used as current one even when explicit Delivery recipient role specified",
					"CONTCONSOL2", packingLines[0].ContainerNumber);
			}
		}

		#endregion

		#region TestDocDataIsNotExported

		public void TestDocDataIsNotExported()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertDocDataIsNotExported();
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertDocDataIsNotExported();
		}
		public void AssertDocDataIsNotExported()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var docData = DocumentNote.LoadNote(shipment);
			var sdf = docData.SystemDefinedFieldWrappers.Cast<StmSystemDefinedFieldWrapper>().FirstOrDefault();
			if (sdf != null)
			{
				sdf.S1_Value = "System text";
			}
			var udf = docData.UserDefinedFieldList.OfType<FilterFieldValueSerialisable>().FirstOrDefault();
			if (udf != null)
			{
				udf.ValueAsStringForSerialisation = "UDF text";
			}

			Assert("Precondition: need at least one sdf or udf", sdf != null || udf != null);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AAD, shipment)), true, false).GetDataObject(shipment);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipmentData, stream);
				using (var reader = new StreamReader(stream))
				{
					var xmlAsString = reader.ReadToEnd();
					AssertNotContains("<DocData> should not be exported into USXML", "<DocData>", xmlAsString);
				}
			}
		}

		#endregion

		public void TestForwardingOrdersAreExported()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var warehouseOrder = (BusinessObject)Factory.BOFactory.New<IWhsOrder>();
			warehouseOrder.FillWithValidTestData();
			shipment.AttachedWarehouseOrders.Add(warehouseOrder);

			var order = shipment.AttachedOrders.AddNew();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = new ZByte(2);

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), checkSubShipments: true, checkForParent: false);
			var shipmentData = writer.GetDataObject(shipment);

			AssertNotNull(shipmentData);
			AssertNotNull(shipmentData.RelatedShipmentCollection);
			AssertEquals(1, shipmentData.RelatedShipmentCollection.Count);

			var orderData = shipmentData.RelatedShipmentCollection[0];
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(2), orderData.Order.OrderNumberSplit);
		}

		(ForwardingShipment, Order) BuildShipmentFromSupplierBooking()
		{
			var (order, _, _, _, _, loadListLine, _, _) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine.PK;
			packLine.JL_JC = loadListLine.CLL_JC_Container;

			Factory.SaveForTesting();

			return (shipment, order);
		}

		public void TestAttachedOrdersFromSupplierBookingAreExported_OrderLinkedViaSupplierBooking()
		{
			var (shipment, order) = BuildShipmentFromSupplierBooking();

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);
			var orderData = shipmentData.RelatedShipmentCollection.Single();

			AssertEquals(0, shipment.AttachedOrders.Count);
			AssertEquals(order.PK, shipment.AttachedOrdersFromSupplierBooking.Single().PK);
			AssertEquals("SBK ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(3), orderData.Order.OrderNumberSplit);
		}

		public void TestAttachedOrdersFromSupplierBookingAreExported_OrderLinkedViaLegacyForeighKey()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = new ZByte(5);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			order.JD_JS = shipment.PK;

			Factory.SaveForTesting();

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);
			var orderData = shipmentData.RelatedShipmentCollection.Single();

			AssertEquals(0, shipment.AttachedOrdersFromSupplierBooking.Count);
			AssertEquals(order.PK, shipment.AttachedOrders.Single().PK);
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(5), orderData.Order.OrderNumberSplit);
		}

		public void TestAttachedOrdersFromSupplierBookingAreExported_OrdersLinkedViaLegacyForeighKeyAndSupplierBooking()
		{
			var (shipment, orderForSBK) = BuildShipmentFromSupplierBooking();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = new ZByte(5);
			order.JD_JS = shipment.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);
			var orderData = shipmentData.RelatedShipmentCollection[0];
			var orderDataForSBK = shipmentData.RelatedShipmentCollection[1];

			AssertEquals(order.PK, shipment.AttachedOrders.Single().PK);
			AssertEquals(orderForSBK.PK, shipment.AttachedOrdersFromSupplierBooking.Single().PK);
			AssertEquals(2, shipmentData.RelatedShipmentCollection.Count);
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(5), orderData.Order.OrderNumberSplit);
			AssertEquals("SBK ORDER ME", orderDataForSBK.Order.OrderNumber);
			AssertEquals(new ZByte(3), orderDataForSBK.Order.OrderNumberSplit);
		}

		public void TestAttachedOrdersFromSupplierBookingAreExported_SameOrderLinkedViaLegacyForeighKeyAndSupplierBooking()
		{
			var (shipment, orderForSBK) = BuildShipmentFromSupplierBooking();
			orderForSBK.JD_JS = shipment.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);
			var orderData = shipmentData.RelatedShipmentCollection.Single();

			AssertEquals(orderForSBK.PK, shipment.AttachedOrders.Single().PK);
			AssertEquals(orderForSBK.PK, shipment.AttachedOrdersFromSupplierBooking.Single().PK);
			AssertEquals("SBK ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(3), orderData.Order.OrderNumberSplit);
		}

		public void TestConvertedBookingCustomFields()
		{
			var shipment = Factory.New<ForwardingShipment>();

			CustomFieldTestHelper.AddCustomField(shipment, "customField", "a");
			var bookingCustomField = CustomFieldTestHelper.AddCustomField(shipment, "customField", "b");
			bookingCustomField.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), checkSubShipments: true, checkForParent: false);
			var shipmentData = writer.GetDataObject(shipment);

			CombineAssertions("Custom field collection is correct", () =>
			{
				var customFields = shipmentData.CustomizedFieldCollection;
				AssertEquals(1, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.String, "customField", "a");
			});
		}

		public void TestPacklinesOnShipmentAndSubShipmentsHaveUniquePackageLinkValues()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.PkgPackageCollection.AddNew();

			var shipment2 = Factory.New<ForwardingShipment>();
			var packingLine2 = shipment2.OuterPackLines.AddNew();
			packingLine2.PkgPackageCollection.AddNew();

			var shipment3 = Factory.New<ForwardingShipment>();
			var packingLine3 = shipment3.OuterPackLines.AddNew();
			packingLine3.PkgPackageCollection.AddNew();

			shipment1.CoLoadShipments.AddRange(shipment2, shipment3);

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment1)), checkSubShipments: true, checkForParent: false);
			var shipmentData = writer.GetDataObject(shipment1);

			AssertNotNull(shipmentData);
			AssertNotNull(shipmentData.SubShipmentCollection);
			AssertEquals(2, shipmentData.SubShipmentCollection.Count);

			var packingLineLinks = shipmentData.PackingLineCollection.Select(p => p.Link).ToList();
			packingLineLinks.AddRange(shipmentData.SubShipmentCollection[0].PackingLineCollection.Select(p => p.Link).ToList());
			packingLineLinks.AddRange(shipmentData.SubShipmentCollection[1].PackingLineCollection.Select(p => p.Link).ToList());

			var expectedPackingLineLinks = new List<ZInt?>() { 1, 2, 3 };

			AssertContainsExactElementsInAnyOrder(expectedPackingLineLinks, packingLineLinks);
		}

		public void Test2012NamespaceLinkPackingLinesToDepartureConsolsContainer()
		{
			var consol1 = SetupConsolWithShipments();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "CNSHA";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CON001";

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "CON002";

			var shipment = consol1.GridShipments[0];
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "CON003";

			var container4 = consol2.Containers.AddNew();
			container4.JC_ContainerNum = "CON004";

			shipment.Consols.Add(consol2);

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.SetContainer(consol1, container1);
			packingLine.SetContainer(consol2, container4);

			UniversalShipment topLevelShipment;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, true);
				topLevelShipment = writer.GetDataObject(shipment);
			}

			CombineAssertions(() =>
			{
				AssertEquals(1, topLevelShipment.PackingLineCollection[0].ContainerLink);
				AssertEquals(container4.JC_ContainerNum, topLevelShipment.PackingLineCollection[0].ContainerNumber);
			});

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.ParentShipmentCollection.Count", 2, topLevelShipment.ParentShipmentCollection.Count);

				var parentConsol1 = topLevelShipment.ParentShipmentCollection[0];
				AssertEquals(2, parentConsol1.ContainerCollection[0].Link);
				AssertEquals(3, parentConsol1.ContainerCollection[1].Link);

				var parentConsol2 = topLevelShipment.ParentShipmentCollection[1];
				AssertEquals(4, parentConsol2.ContainerCollection[0].Link);
				AssertEquals(1, parentConsol2.ContainerCollection[1].Link);
			});
		}

		public void Test2012NamespaceMakesMultipleParentConsolsWork()
		{
			var consol1BO = SetupConsolWithShipments();
			consol1BO.JK_RL_NKLoadPort = "AUSYD";
			consol1BO.JK_RL_NKDischargePort = "NZAKL";
			var leg1 = consol1BO.Transports.MostInterestingTransport;
			leg1.JW_Vessel = "BUNGA DELIMA";
			leg1.JW_ATD = new ZDateTime(2012, 06, 1);
			leg1.JW_ATA = new ZDateTime(2012, 06, 5);

			var shipmentBO = consol1BO.GridShipments[0];

			var consol2BO = Factory.New<ForwardingConsol>();
			consol2BO.JK_TransportMode = Constants.TransportModes.Sea;
			consol2BO.JK_UniqueConsignRef = "C00010001";
			consol2BO.JK_MasterBillNum = "M2345678";
			consol2BO.JK_RL_NKLoadPort = "NZAKL";
			consol2BO.JK_RL_NKDischargePort = "FJSUV";
			var leg2 = consol1BO.Transports.MostInterestingTransport;
			leg2.JW_Vessel = "BUNGA TERRATAI 3";
			leg2.JW_ATD = new ZDateTime(2012, 06, 7);
			leg2.JW_ATA = new ZDateTime(2012, 06, 12);

			shipmentBO.Consols.Add(consol2BO);

			UniversalShipment topLevelShipment;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				topLevelShipment = writer.GetDataObject(shipmentBO);
			}

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001]", topLevelShipment.DataContext.GetDataSources());
				AssertNotNull("topLevelShipment.ParentShipmentCollection", topLevelShipment.ParentShipmentCollection);
				AssertNotNull("topLevelShipment.SubShipmentCollection", topLevelShipment.SubShipmentCollection);
			});

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.ParentShipmentCollection.Count", 2, topLevelShipment.ParentShipmentCollection.Count);
				var parentConsol1 = topLevelShipment.ParentShipmentCollection[0];
				AssertEquals("parentConsol1.DataContext.GetDataSources()", "ForwardingConsol [C00010000]", parentConsol1.DataContext.GetDataSources());
				var parentConsol2 = topLevelShipment.ParentShipmentCollection[1];
				AssertEquals("parentConsol1.DataContext.GetDataSources()", "ForwardingConsol [C00010001]", parentConsol2.DataContext.GetDataSources());
			});
		}

		public void Test2012NamespaceHasConsolInParentShipmentCollection()
		{
			var consolBO = SetupConsolWithShipments();
			var shipmentBO = consolBO.GridShipments[0];
			UniversalShipment topLevelShipment;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				topLevelShipment = writer.GetDataObject(shipmentBO);
			}

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001]", topLevelShipment.DataContext.GetDataSources());
				AssertNotNull("topLevelShipment.ParentShipmentCollection", topLevelShipment.ParentShipmentCollection);
				AssertNotNull("topLevelShipment.SubShipmentCollection", topLevelShipment.SubShipmentCollection);
			});

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.ParentShipmentCollection.Count", 1, topLevelShipment.ParentShipmentCollection.Count);
				var parentShipment = topLevelShipment.ParentShipmentCollection[0];
				AssertEquals("parentShipment.DataContext.GetDataSources()", "ForwardingConsol [C00010000]", parentShipment.DataContext.GetDataSources());

				AssertEquals("topLevelShipment.SubShipmentCollection.Count", 2, topLevelShipment.SubShipmentCollection.Count);
				var subShipment1 = topLevelShipment.SubShipmentCollection[0];
				AssertEquals("subShipment1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-1]", subShipment1.DataContext.GetDataSources());
				var subShipment2 = topLevelShipment.SubShipmentCollection[1];
				AssertEquals("subShipment2.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2]", subShipment2.DataContext.GetDataSources());
			});
		}

		public void Test2012NamespaceHasCoLoadMasterInParentShipmentCollection()
		{
			var consolBO = SetupConsolWithShipments();
			var coLoadShipmentBO = consolBO.GridShipments[0];
			var shipmentBO = coLoadShipmentBO.CoLoadShipments[0];
			UniversalShipment topLevelShipment;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				topLevelShipment = writer.GetDataObject(shipmentBO);
			}

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001-1]", topLevelShipment.DataContext.GetDataSources());
				AssertNotNull("topLevelShipment.ParentShipmentCollection", topLevelShipment.ParentShipmentCollection);
			});

			CombineAssertions(() =>
			{
				AssertEquals("topLevelShipment.ParentShipmentCollection.Count", 1, topLevelShipment.ParentShipmentCollection.Count);
				var parentShipment = topLevelShipment.ParentShipmentCollection[0];
				AssertEquals("parentShipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001]", parentShipment.DataContext.GetDataSources());

				AssertEquals("parentShipment.ParentShipmentCollection.Count", 1, parentShipment.ParentShipmentCollection.Count);
				var parentConsol = parentShipment.ParentShipmentCollection[0];
				AssertEquals("parentConsol.DataContext.GetDataSources()", "ForwardingConsol [C00010000]", parentConsol.DataContext.GetDataSources());
			});
		}

		public void TestDeliveryLocalTransportAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB(AddressTypes.DeliveryLocalCartage,
				shipmentData.OrganizationAddressCollection.Single(),
				AddressTypes.DeliveryLocalCartage);

			#endregion
		}

		public void TestDeliveryCFSAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_OA_ImportReleaseDepot = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB("ArrivalCFSAddress",
				shipmentData.OrganizationAddressCollection.Single(),
				"ArrivalCFSAddress");

			#endregion
		}

		public void TestBookedShippingLineAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_OA_BookedShippingLineAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB("ShippingLineAddress",
				shipmentData.OrganizationAddressCollection.Single(),
				"ShippingLineAddress");

			#endregion
		}

		public void TestCreditorAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_OH_Creditor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB("Creditor",
				shipmentData.OrganizationAddressCollection.Single(),
				"Creditor");

			#endregion
		}

		public void TestWithBothTypesOfCustomFields()
		{
			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("First Date", ""));
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Last Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Deci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("+ 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Textual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Customs are customary", ""));

			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

			shipmentBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			shipmentBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			shipmentBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			shipmentBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			shipmentBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			shipmentBO.DocsAndCartage.JP_CustomAttrib1 = "HELLO";
			shipmentBO.DocsAndCartage.JP_CustomAttrib2 = "GOODBYE";
			shipmentBO.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2011, 1, 1);
			shipmentBO.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2011, 1, 2);
			shipmentBO.DocsAndCartage.JP_CustomDecimal1 = 0.3;
			shipmentBO.DocsAndCartage.JP_CustomDecimal2 = 1.3;
			shipmentBO.DocsAndCartage.JP_CustomFlag1 = true;
			shipmentBO.DocsAndCartage.JP_CustomFlag2 = true;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 13, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.String, "Textual context", "HELLO");
				customFields.AssertCustomFieldWasExported(DataType.String, "Customs are customary", "GOODBYE");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "First Date", new ZDateTime(2011, 1, 1).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "Last Date", new ZDateTime(2011, 1, 2).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "Deci Deca", "0.3");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "+ 1 point zero", "1.3");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flag this!", "true");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flagger", "true");
			});
		}

		public void TestWorkflowCustomFieldsOnShipmentAreExported()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

			shipmentBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			shipmentBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			shipmentBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			shipmentBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			shipmentBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		[TestDate(2013, 6, 5, 12, 30, 10, 500)] // for fields set to ZDateTime.Now by writer
		public void TestExportAllCustomFieldsToUniversalShipment()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "CON";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "CustomBlaBool";
			genCustomColumnBool.XC_Type = "BOO";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnEmptyBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnEmptyBool.XC_Name = "CustomBlaEmptyBool";
			genCustomColumnEmptyBool.XC_Type = "BOO";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnEmptyBool);

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "CustomBlaString";
			genCustomColumnString.XC_Type = "STR";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "CustomBlaInt";
			genCustomColumnInt.XC_Type = "INT";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnCbo = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnCbo.XC_Name = "Combo time";
			genCustomColumnCbo.XC_Type = "CBO";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnCbo);

			Factory.SaveForTesting();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var customFields = ((ICustomFieldProvider)consol).GetCustomBusinessObject();
			customFields[((IDynamicBusinessObject)customFields).PropertyNames[0]] = "asdf";
			customFields[((IDynamicBusinessObject)customFields).PropertyNames[1]] = "hjkl";
			customFields[((IDynamicBusinessObject)customFields).PropertyNames[2]] = true;

			Factory.SaveForTesting();

			var manager = consol.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));

			var universalShipment = (UniversalShipment)writer.GetDataObject(consol);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalShipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertEquals(ConsolXML, result);
					}
				}
			}
		}

		#region ConsolXML
		const string ConsolXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>26FYS0PM3GCY3VHFAX26</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <AgentsReference></AgentsReference>
    <AWBServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </AWBServiceLevel>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CarrierContractNumber></CarrierContractNumber>
    <ChargeableRate>0</ChargeableRate>
    <ConsolCommodity>
      <Code></Code>
    </ConsolCommodity>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code></Code>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <ElectronicBillOfLadingReference></ElectronicBillOfLadingReference>
    <EventBranchHomePort>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </EventBranchHomePort>
    <FreightRate>0</FreightRate>
    <FreightRateCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </FreightRateCurrency>
    <GatewayServiceLevel>
      <Code></Code>
    </GatewayServiceLevel>
    <GreenhouseGasEmission>
      <CO2e>0</CO2e>
      <CO2eDescriptiveStatus>
        <Code>NON</Code>
        <Description>Not Calculated</Description>
      </CO2eDescriptiveStatus>
      <CO2eUnit>
        <Code>KG</Code>
        <Description>Kilograms</Description>
      </CO2eUnit>
    </GreenhouseGasEmission>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHazardous>false</IsHazardous>
    <IsNeutralMaster>false</IsNeutralMaster>
    <LloydsIMO></LloydsIMO>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <MaximumAllowablePackageHeight>0</MaximumAllowablePackageHeight>
    <MaximumAllowablePackageLength>0</MaximumAllowablePackageLength>
    <MaximumAllowablePackageLengthUnit>
      <Code></Code>
    </MaximumAllowablePackageLengthUnit>
    <MaximumAllowablePackageWidth>0</MaximumAllowablePackageWidth>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <PaymentMethod>
      <Code></Code>
    </PaymentMethod>
    <PlaceOfDelivery>
      <Code></Code>
    </PlaceOfDelivery>
    <PlaceOfIssue>
      <Code></Code>
    </PlaceOfIssue>
    <PlaceOfReceipt>
      <Code></Code>
    </PlaceOfReceipt>
    <PortFirstForeign>
      <Code></Code>
    </PortFirstForeign>
    <PortLastForeign>
      <Code></Code>
    </PortLastForeign>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfFirstArrival>
      <Code></Code>
    </PortOfFirstArrival>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <ReceivingForwarderHandlingType>
      <Code></Code>
    </ReceivingForwarderHandlingType>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <RequiresTemperatureControl>false</RequiresTemperatureControl>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <SendingForwarderHandlingType>
      <Code></Code>
    </SendingForwarderHandlingType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TotalPreallocatedChargeable>0</TotalPreallocatedChargeable>
    <TotalPreallocatedVolume>0</TotalPreallocatedVolume>
    <TotalPreallocatedVolumeUnit>
      <Code></Code>
    </TotalPreallocatedVolumeUnit>
    <TotalPreallocatedWeight>0</TotalPreallocatedWeight>
    <TotalPreallocatedWeightUnit>
      <Code></Code>
    </TotalPreallocatedWeightUnit>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>Combo timePART1</Key>
        <Value>asdf</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>Combo timePART2</Key>
        <Value>hjkl</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>CustomBlaBool</Key>
        <Value>true</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Boolean</DataType>
        <Key>CustomBlaEmptyBool</Key>
        <Value>false</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Integer</DataType>
        <Key>CustomBlaInt</Key>
        <Value>0</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>CustomBlaString</Key>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>
    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>CutOffDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DepartureReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ArrivalReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DepartureDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ArrivalDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
    <MilestoneCollection>
      <Milestone>
        <Description>Departure from First Load Port</Description>
        <EventCode>DEP</EventCode>
        <Sequence>1</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;FirstLeg.Origin&gt;,FAC=CTO</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Arrival at Final Discharge Port</Description>
        <EventCode>ARV</EventCode>
        <Sequence>6</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference>LOC=&lt;LastLeg.Destination&gt;,FAC=CTO</ConditionReference>
        <ConditionType>RFP</ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <AircraftType>
          <Code></Code>
        </AircraftType>
        <BookingStatus>
          <Code>CNF</Code>
          <Description>Confirmed</Description>
        </BookingStatus>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <DocumentCutOff></DocumentCutOff>
        <EmptyCutOff></EmptyCutOff>
        <EmptyReceivalCommences></EmptyReceivalCommences>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture></EstimatedDeparture>
        <FCLAvailability></FCLAvailability>
        <FCLCutOff></FCLCutOff>
        <FCLReceivalCommences></FCLReceivalCommences>
        <FCLStorage></FCLStorage>
        <GreenhouseGasEmission>
          <CO2e>0</CO2e>
          <CO2eDescriptiveStatus>
            <Code>NON</Code>
            <Description>Not Calculated</Description>
          </CO2eDescriptiveStatus>
          <CO2eUnit>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </CO2eUnit>
        </GreenhouseGasEmission>
        <HazzardCutOffDate></HazzardCutOffDate>
        <HazzardReceivalCommences></HazzardReceivalCommences>
        <IsCargoOnly>true</IsCargoOnly>
        <LCLAvailability></LCLAvailability>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LCLStorageDate></LCLStorageDate>
        <LegNotes></LegNotes>
        <ReeferCutOff></ReeferCutOff>
        <ReeferReceivalCommences></ReeferReceivalCommences>
        <ScheduledArrival></ScheduledArrival>
        <ScheduledDeparture></ScheduledDeparture>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName></VesselName>
        <VGMCutOff></VGMCutOff>
        <VoyageFlightNo></VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion

		[TestDate]
		public void TestExportCustomFieldsWithSameNameToUniversalShipment()
		{
			TestDateAttribute.Date = DateTime.Today;
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "CustomBlaString";
			genCustomColumnString.XC_Type = "STR";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "CustomBlaInt";
			genCustomColumnInt.XC_Type = "INT";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			Factory.SaveForTesting();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_UniqueConsignRef = "S00000000";
			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("CustomBlaString", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("CustomBlaString", ""));
			shipment.DocsAndCartage.JP_CustomAttrib1 = "abc";

			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)));

			var universalShipment = (UniversalShipment)writer.GetDataObject(shipment);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalShipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertXMLEquals(ShipmentXMLWithSameNameCustomFields(shipment.JS_HouseBillIssueDate), result);
					}
				}
			}
		}

		#region ShipmentXMLWithSameNameCustomFields
		string ShipmentXMLWithSameNameCustomFields(ZDateTime houseBillIssueDate) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00000000</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <ActualChargeable>0</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <CommunityTransitStatus>
      <Code></Code>
    </CommunityTransitStatus>
    <CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LCL</Code>
      <Description>Less Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <EventBranchHomePort>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </EventBranchHomePort>
    <FMCTariffID></FMCTariffID>
    <FreightRate>0</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <GreenhouseGasEmission>
      <CO2e>0</CO2e>
      <CO2eDescriptiveStatus>
        <Code>NON</Code>
        <Description>Not Calculated</Description>
      </CO2eDescriptiveStatus>
      <CO2eUnit>
        <Code>KG</Code>
        <Description>Kilograms</Description>
      </CO2eUnit>
    </GreenhouseGasEmission>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <HouseBillOfLadingType>
      <Code></Code>
    </HouseBillOfLadingType>
    <InsuranceValue>0</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHighRisk>false</IsHighRisk>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code></Code>
    </PortOfDestination>
    <PortOfOrigin>
      <Code></Code>
    </PortOfOrigin>
    <RateCommodity>
      <Code></Code>
    </RateCommodity>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <ScreeningStatus>
      <Code>CLR</Code>
      <Description>Clear</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code></Code>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>CustomBlaString</Key>
        <Value>abc</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>CustomBlaString</Key>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Integer</DataType>
        <Key>CustomBlaInt</Key>
        <Value>0</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>RevisedDeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>{houseBillIssueDate.ToString("yyyy-MM-ddTHH:mm:ss")}</Value>
      </Date>
      <Date>
        <Type>PickupReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <PackingLineCollection Content=""Complete"">
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion

		public void TestCustomFieldsOnShipmentAreExported()
		{
			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("First Date", ""));
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Last Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Deci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("+ 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Textual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Customs are customary", ""));

			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.DocsAndCartage.JP_CustomAttrib1 = "HELLO";
			shipmentBO.DocsAndCartage.JP_CustomAttrib2 = "GOODBYE";
			shipmentBO.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2011, 1, 1);
			shipmentBO.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2011, 1, 2);
			shipmentBO.DocsAndCartage.JP_CustomDecimal1 = 0.3;
			shipmentBO.DocsAndCartage.JP_CustomDecimal2 = 1.3;
			shipmentBO.DocsAndCartage.JP_CustomFlag1 = true;
			shipmentBO.DocsAndCartage.JP_CustomFlag2 = true;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 8, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.String, "Textual context", "HELLO");
				customFields.AssertCustomFieldWasExported(DataType.String, "Customs are customary", "GOODBYE");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "First Date", new ZDateTime(2011, 1, 1).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "Last Date", new ZDateTime(2011, 1, 2).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "Deci Deca", "0.3");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "+ 1 point zero", "1.3");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flag this!", "true");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flagger", "true");
			});
		}

		public void TestDataSourceOnChildShipments()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var subShipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var subSubShipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

			subShipmentBO.CoLoadShipments.Add(subSubShipmentBO);
			shipmentBO.CoLoadShipments.Add(subShipmentBO);

			Factory.SaveForTesting();

			AssertEquals("Precondition: shipmentBO.JS_UniqueConsignRef.IsEmpty", false, shipmentBO.JS_UniqueConsignRef.IsEmpty);
			AssertEquals("Precondition: subShipmentBO.JS_UniqueConsignRef.IsEmpty", false, subShipmentBO.JS_UniqueConsignRef.IsEmpty);
			AssertEquals("Precondition: subSubShipmentBO.JS_UniqueConsignRef.IsEmpty", false, subSubShipmentBO.JS_UniqueConsignRef.IsEmpty);

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var dataContext = shipmentData.DataContext;
			AssertNotNull("dataContext", dataContext);
			AssertNotNull("DataSourceCollection contains data context reference", dataContext.DataSourceCollection.FirstOrDefault(r => r.Key.GetValueOrDefault() == shipmentBO.JS_UniqueConsignRef && r.Type.GetValueOrDefault() == "ForwardingShipment"));

			dataContext = shipmentData.SubShipmentCollection[0].DataContext;
			AssertNotNull("dataContext", dataContext);
			AssertNotNull("DataSourceCollection contains data context reference", dataContext.DataSourceCollection.FirstOrDefault(r => r.Key.GetValueOrDefault() == subShipmentBO.JS_UniqueConsignRef && r.Type.GetValueOrDefault() == "ForwardingShipment"));

			dataContext = shipmentData.SubShipmentCollection[0].SubShipmentCollection[0].DataContext;
			AssertNotNull("dataContext", dataContext);
			AssertNotNull("DataSourceCollection contains data context reference", dataContext.DataSourceCollection.FirstOrDefault(r => r.Key.GetValueOrDefault() == subSubShipmentBO.JS_UniqueConsignRef && r.Type.GetValueOrDefault() == "ForwardingShipment"));
		}

		public void TestBasicShipmentLevelFieldMappings()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_AdditionalTerms = "Add Me Some Terms";
			shipmentBO.JS_BookingReference = "BOOK ME";
			shipmentBO.JS_CartageWaybill = "CARTAGE BILL";
			shipmentBO.JS_CFSReference = "CFS Book Ref";
			shipmentBO.JS_DocumentedVolume = 3.45m;
			shipmentBO.JS_DocumentedWeight = 4.56m;
			shipmentBO.JS_UnitFreightRate = 67.89m;
			shipmentBO.JS_RX_NKFrtRateCurrency = Enterprise.Core.Constants.CurrencyCodes.CzechRepublic;
			shipmentBO.JS_GoodsDescription = "RAT HATS";
			shipmentBO.JS_GoodsValue = 5.67m;
			shipmentBO.JS_RX_NKGoodsValueCurr = Enterprise.Core.Constants.CurrencyCodes.Ghana;
			shipmentBO.JS_HBLAWBChargesDisplay = "SHW";
			shipmentBO.JS_HBLContainerPackModeOverride = "FAR";
			shipmentBO.JS_InsuranceValue = 6.78m;
			shipmentBO.JS_RX_NKInsuranceCurrency = Enterprise.Core.Constants.CurrencyCodes.Kenya;
			shipmentBO.JS_InterimReceipt = "IR Text";

			shipmentBO.JS_ManifestedVolume = 8.90;
			shipmentBO.JS_ManifestedWeight = 9.01;
			shipmentBO.JS_OuterPacks = 44;
			shipmentBO.JS_F3_NKPackType = "VF";
			shipmentBO.JS_OverrideWaybillDefaults = true;

			shipmentBO.JS_PackingOrder = 1;
			shipmentBO.JS_ReleaseType = "CAD"; // Cash Against Documents
			shipmentBO.JS_ScreeningStatus = "UNK"; // Unknown
			shipmentBO.JS_RS_NKServiceLevel = "STD"; // Standard
			shipmentBO.JS_INCO = "CIF"; // Cost, Insurance And Freight
			shipmentBO.JS_ShippedOnBoard = "LDN"; // Laden

			shipmentBO.JS_ShipperCODAmount = 12.34m;
			shipmentBO.JS_ShipperCODPayMethod = "COC"; // Company Check

			shipmentBO.JS_TotalPackageCount = 45;
			shipmentBO.JS_F3_NKTotalCountPackType = "KEG"; // Keg

			shipmentBO.JS_ActualVolume = 23.45m;
			shipmentBO.JS_UnitOfVolume = "CF"; // Cubic Feet
			shipmentBO.JS_ActualWeight = 34.56m;
			shipmentBO.JS_UnitOfWeight = "KT"; // Kilotons

			shipmentBO.JS_TranshipToOtherCFS = true;

			var consolBO = shipmentBO.Consols.AddNew();
			shipmentBO.JS_RL_NKOrigin = "NZDUD"; // Dunedin
			consolBO.JK_RL_NKLoadPort = "NZCHC"; // Christchurch
			consolBO.JK_RL_NKPortOfFirstArrival = "AUNTL"; // Newcastle
			consolBO.JK_RL_NKDischargePort = "AUSYD"; // Sydney
			shipmentBO.JS_RL_NKDestination = "AUBDG"; // Bendigo

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA DELIMA TEST";
			vessel.RV_LloydsNumber = "8907993";

			var leg = consolBO.Transports[0];
			leg.JW_Vessel = vessel.RV_FK;
			leg.JW_VoyageFlight = "343L";
			leg.JW_ETD = new ZDateTime(2011, 3, 4);
			leg.JW_ETA = new ZDateTime(2011, 3, 6);

			shipmentBO.JS_WarehouseLocation = "HOME";
			shipmentBO.JS_HouseBill = "MYHOUSE";

			shipmentBO.JS_ActualChargeable = 123.45m;
			shipmentBO.JS_DocumentedChargeable = 2.34m;
			shipmentBO.JS_ManifestedChargeable = 7.89;

			shipmentBO.JS_NoCopyBills = new ZByte(3);
			shipmentBO.JS_NoOriginalBills = new ZByte(4);

			shipmentBO.JS_RH_NKRateCommodity = "WOOD";
			shipmentBO.JS_FMCTariffID = "BBBB";

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);

			#region Check Contents of shipmentData object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentData.ActualChargeable", 123.45m, shipmentData.ActualChargeable);
				AssertEquals("shipmentData.AdditionalTerms", "Add Me Some Terms", shipmentData.AdditionalTerms);
				AssertEquals("shipmentData.AgentsReference", null, shipmentData.AgentsReference);
				AssertEquals("shipmentData.AWBServiceLevel", null, shipmentData.AWBServiceLevel);
				AssertEquals("shipmentData.BookingConfirmationReference", "BOOK ME", shipmentData.BookingConfirmationReference);
				AssertEquals("shipmentData.CartageWaybillNumber", "CARTAGE BILL", shipmentData.CartageWaybillNumber);
				AssertEquals("shipmentData.CFSReference", "CFS Book Ref", shipmentData.CFSReference);
				AssertEquals("shipmentData.ConsolidatedCargoStatus", null, shipmentData.ConsolidatedCargoStatus);
				AssertEquals("shipmentData.ContainerCount", 0, shipmentData.ContainerCount);
				AssertEquals("shipmentData.ContainerMode.Code", "LCL", shipmentData.ContainerMode.Code);
				AssertEquals("shipmentData.ContainerMode.Description", "Less Container Load", shipmentData.ContainerMode.Description);
				AssertEquals("shipmentData.CountryOfSupply", null, shipmentData.CountryOfSupply);
				AssertEquals("shipmentData.DocumentedChargeable", 2.34m, shipmentData.DocumentedChargeable);
				AssertEquals("shipmentData.DocumentedVolume", 3.45m, shipmentData.DocumentedVolume);
				AssertEquals("shipmentData.DocumentedWeight", 4.56m, shipmentData.DocumentedWeight);
				AssertEquals("shipmentData.EFTMode", null, shipmentData.EFTMode);
				AssertEquals("shipmentData.EntryStatus", null, shipmentData.EntryStatus);
				AssertEquals("shipmentData.ExportGoodsType", null, shipmentData.ExportGoodsType);
				AssertEquals("shipmentData.FirstBuyerContact", null, shipmentData.FirstBuyerContact);
				AssertEquals("shipmentData.Folio", null, shipmentData.Folio);

				AssertEquals("shipmentData.FreightRate", 67.89m, shipmentData.FreightRate);
				AssertEquals("shipmentData.FreightRateCurrency.Code", "CZK", shipmentData.FreightRateCurrency.Code);
				AssertEquals("shipmentData.FreightRateCurrency.Description", "Czech Koruna", shipmentData.FreightRateCurrency.Description);
				AssertEquals("shipmentData.GoodsDescription", "RAT HATS", shipmentData.GoodsDescription);
				AssertEquals("shipmentData.GoodsValue", 5.67m, shipmentData.GoodsValue);
				AssertEquals("shipmentData.GoodsValueCurrency.Code", "GHS", shipmentData.GoodsValueCurrency.Code);
				AssertEquals("shipmentData.GoodsValueCurrency.Description", "Ghana Cedi", shipmentData.GoodsValueCurrency.Description);
				AssertEquals("shipmentData.HBLAWBChargesDisplay.Code", "SHW", shipmentData.HBLAWBChargesDisplay.Code);
				AssertEquals("shipmentData.HBLAWBChargesDisplay.Description", "Show Collect Charges", shipmentData.HBLAWBChargesDisplay.Description);
				AssertEquals("shipmentData.HBLContainerPackModeOverride", "FAR", shipmentData.HBLContainerPackModeOverride);

				AssertEquals("shipmentData.InsuranceValue", 6.78m, shipmentData.InsuranceValue);
				AssertEquals("shipmentData.InsuranceValueCurrency.Code", "KES", shipmentData.InsuranceValueCurrency.Code);
				AssertEquals("shipmentData.InsuranceValueCurrency.Description", "Kenyan Shilling", shipmentData.InsuranceValueCurrency.Description);
				AssertEquals("shipmentData.InterimReceiptNumber", "IR Text", shipmentData.InterimReceiptNumber);
				AssertEquals("shipmentData.IsBooking", false, shipmentData.IsBooking);
				AssertEquals("shipmentData.IsCFSRegistered", false, shipmentData.IsCFSRegistered);
				AssertEquals("shipmentData.IsDirectBooking", false, shipmentData.IsDirectBooking);
				AssertEquals("shipmentData.IsForwardRegistered", true, shipmentData.IsForwardRegistered);
				AssertEquals("shipmentData.IsNeutralMaster", false, shipmentData.IsNeutralMaster.Value);
				AssertEquals("shipmentData.IsPersonalEffects", null, shipmentData.IsPersonalEffects);
				AssertEquals("shipmentData.IsShipping", false, shipmentData.IsShipping);
				AssertEquals("shipmentData.IsSplitShipment", false, shipmentData.IsSplitShipment);
				AssertEquals("shipmentData.LloydsIMO", "8907993", shipmentData.LloydsIMO);
				AssertEquals("shipmentData.ManifestedChargeable", 7.89m, shipmentData.ManifestedChargeable);
				AssertEquals("shipmentData.ManifestedVolume", 8.90m, shipmentData.ManifestedVolume);
				AssertEquals("shipmentData.ManifestedWeight", 9.01m, shipmentData.ManifestedWeight);
				AssertEquals("shipmentData.MergeBy", null, shipmentData.MergeBy);
				AssertEquals("shipmentData.MessageStatus", null, shipmentData.MessageStatus);
				AssertEquals("shipmentData.MessageSubType", null, shipmentData.MessageSubType);
				AssertEquals("shipmentData.MessageType", null, shipmentData.MessageType);
				AssertEquals("shipmentData.NoCopyBills", new ZByte(3), shipmentData.NoCopyBills);
				AssertEquals("shipmentData.NoOriginalBills", new ZByte(4), shipmentData.NoOriginalBills);
				AssertEquals("shipmentData.OperationalStatus", null, shipmentData.OperationalStatus);
				AssertEquals("shipmentData.OuterPacks", 44, shipmentData.OuterPacks);
				AssertEquals("shipmentData.OuterPacksPackageType.Code", "VF", shipmentData.OuterPacksPackageType.Code);

				AssertEquals("shipmentData.OwnerRef", null, shipmentData.OwnerRef);
				AssertEquals("shipmentData.PackingOrder", 1, shipmentData.PackingOrder);
				AssertEquals("shipmentData.PaymentMethod", null, shipmentData.PaymentMethod);
				AssertEquals("shipmentData.QuoteNumber", null, shipmentData.QuoteNumber);
				AssertEquals("shipmentData.ReleaseType.Code", "CAD", shipmentData.ReleaseType.Code);
				AssertEquals("shipmentData.ReleaseType.Description", "Cash Against Documents", shipmentData.ReleaseType.Description);
				AssertEquals("shipmentData.ScreeningStatus.Code", "UNK", shipmentData.ScreeningStatus.Code);
				AssertEquals("shipmentData.ScreeningStatus.Description", "Unknown", shipmentData.ScreeningStatus.Description);
				AssertEquals("shipmentData.SecondBuyerContact", null, shipmentData.SecondBuyerContact);
				AssertEquals("shipmentData.ServiceLevel.Code", "STD", shipmentData.ServiceLevel.Code);
				AssertEquals("shipmentData.ServiceLevel.Description", "Standard", shipmentData.ServiceLevel.Description);
				AssertEquals("shipmentData.ShipmentIncoTerm.Code", "CIF", shipmentData.ShipmentIncoTerm.Code);
				AssertEquals("shipmentData.ShipmentIncoTerm.Description", "Cost, Insurance And Freight", shipmentData.ShipmentIncoTerm.Description);
				AssertEquals("shipmentData.ShipmentStatus", null, shipmentData.ShipmentStatus);
				AssertEquals("shipmentData.ShipmentType.Code", "STD", shipmentData.ShipmentType.Code);
				AssertEquals("shipmentData.ShipmentType.Description", "Standard House", shipmentData.ShipmentType.Description);
				AssertEquals("shipmentData.ShippedOnBoard.Code", "LDN", shipmentData.ShippedOnBoard.Code);
				AssertEquals("shipmentData.ShippedOnBoard.Description", "Laden", shipmentData.ShippedOnBoard.Description);

				AssertEquals("shipmentData.ShipperCODAmount", 12.34m, shipmentData.ShipperCODAmount);
				AssertEquals("shipmentData.ShipperCODPayMethod.Code", "COC", shipmentData.ShipperCODPayMethod.Code);
				AssertEquals("shipmentData.ShipperCODPayMethod.Description", "Company Check", shipmentData.ShipperCODPayMethod.Description);
				AssertEquals("shipmentData.TotalNoOfPacks", 45, shipmentData.TotalNoOfPacks);
				AssertEquals("shipmentData.TotalNoOfPacksDecimal", null, shipmentData.TotalNoOfPacksDecimal);
				AssertEquals("shipmentData.TotalNoOfPacksPackageType.Code", "KEG", shipmentData.TotalNoOfPacksPackageType.Code);
				AssertEquals("shipmentData.TotalNoOfPacksPackageType.Description", "Keg", shipmentData.TotalNoOfPacksPackageType.Description);
				AssertEquals("shipmentData.TotalNoOfPieces", null, shipmentData.TotalNoOfPieces);
				AssertEquals("shipmentData.TotalVolume", 23.45m, shipmentData.TotalVolume);
				AssertEquals("shipmentData.TotalVolumeUnit.Code", "CF", shipmentData.TotalVolumeUnit.Code);
				AssertEquals("shipmentData.TotalVolumeUnit.Description", "Cubic Feet", shipmentData.TotalVolumeUnit.Description);
				AssertEquals("shipmentData.TotalWeight", 34.56m, shipmentData.TotalWeight);
				AssertEquals("shipmentData.TotalWeightUnit.Code", "KT", shipmentData.TotalWeightUnit.Code);
				AssertEquals("shipmentData.TotalWeightUnit.Description", "Kilotons", shipmentData.TotalWeightUnit.Description);
				AssertEquals("shipmentData.TranshipToOtherCFS", true, shipmentData.TranshipToOtherCFS);

				AssertEquals("shipmentData.TransportMode.Code", "SEA", shipmentData.TransportMode.Code);
				AssertEquals("shipmentData.TransportMode.Description", "Sea Freight", shipmentData.TransportMode.Description);
				AssertEquals("shipmentData.PortOfOrigin.Code", "NZDUD", shipmentData.PortOfOrigin.Code);
				AssertEquals("shipmentData.PortOfOrigin.Name", "Dunedin", shipmentData.PortOfOrigin.Name);
				AssertEquals("shipmentData.PortOfLoading.Code", "NZCHC", shipmentData.PortOfLoading.Code);
				AssertEquals("shipmentData.PortOfLoading.Name", "Christchurch", shipmentData.PortOfLoading.Name);
				AssertEquals("shipmentData.PortOfFirstArrival.Code", "AUNTL", shipmentData.PortOfFirstArrival.Code);
				AssertEquals("shipmentData.PortOfFirstArrival.Name", "Newcastle", shipmentData.PortOfFirstArrival.Name);
				AssertEquals("shipmentData.PortOfDischarge.Code", "AUSYD", shipmentData.PortOfDischarge.Code);
				AssertEquals("shipmentData.PortOfDischarge.Name", "Sydney", shipmentData.PortOfDischarge.Name);
				AssertEquals("shipmentData.PortOfDestination.Code", "AUBDG", shipmentData.PortOfDestination.Code);
				AssertEquals("shipmentData.PortOfDestination.Name", "Bendigo", shipmentData.PortOfDestination.Name);

				AssertEquals("shipmentData.VesselName", "BUNGA DELIMA TEST", shipmentData.VesselName);
				AssertEquals("shipmentData.VesselLloydsNumber", "8907993", shipmentData.LloydsIMO);
				AssertEquals("shipmentData.VoyageFlightNo", "343L", shipmentData.VoyageFlightNo);
				AssertEquals("shipmentData.WarehouseLocation", "HOME", shipmentData.WarehouseLocation);
				AssertEquals("shipmentData.WarehouseReleaseStatus", null, shipmentData.WarehouseReleaseStatus);
				AssertEquals("shipmentData.WayBillNumber", "MYHOUSE", shipmentData.WayBillNumber);
				AssertEquals("shipmentData.WayBillType.Code", "HWB", shipmentData.WayBillType.Code);
				AssertEquals("shipmentData.WayBillType.Description", "House Waybill", shipmentData.WayBillType.Description);

				AssertEquals("shipmentData.RateCommodity", "WOOD", shipmentData.RateCommodity.Code);
				AssertEquals("shipmentData.FMCTariffID", "BBBB", shipmentData.FMCTariffID);
			});

			#endregion
		}

		public void TestPortsOfLoadingAndDischargeWhenTransportsIncludingRelatedIsEmpty()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_RL_NKLoadPort = "AUSYD";
			shipmentBO.JS_RL_NKDischargePort = "USLAX";

			var actionInfo = new ActionInfo(RecipientRoleType.ORP, shipmentBO);
			var dataWritingManager = new DataWritingManager(actionInfo);
			var shipmentData = new ShipmentDataObjectWriter(dataWritingManager, true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.PortOfLoading", shipmentData.PortOfLoading);
			AssertNotNull("shipmentData.PortOfDischarge", shipmentData.PortOfDischarge);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentData.PortOfLoading.Code", "AUSYD", shipmentData.PortOfLoading.Code);
				AssertEquals("shipmentData.PortOfLoading.Name", "Sydney", shipmentData.PortOfLoading.Name);
				AssertEquals("shipmentData.PortOfDischarge.Code", "USLAX", shipmentData.PortOfDischarge.Code);
				AssertEquals("shipmentData.PortOfDischarge.Name", "Los Angeles", shipmentData.PortOfDischarge.Name);
			});
		}

		public void TestPortsOfLoadingAndDischargeWhenTransportsIncludingRelatedIsNotEmpty()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_RL_NKLoadPort = "AUSYD"; //Sydney
			shipmentBO.JS_RL_NKDischargePort = "USLAX"; //Los Angeles

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "NZCHC"; // Christchurch
			consolBO.JK_RL_NKPortOfFirstArrival = "AUNTL"; // Newcastle
			consolBO.JK_RL_NKDischargePort = "AUSYD"; // Sydney

			var actionInfo = new ActionInfo(RecipientRoleType.ORP, shipmentBO);
			var dataWritingManager = new DataWritingManager(actionInfo);
			var shipmentData = new ShipmentDataObjectWriter(dataWritingManager, true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.PortOfLoading", shipmentData.PortOfLoading);
			AssertNotNull("shipmentData.PortOfDischarge", shipmentData.PortOfDischarge);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentData.PortOfLoading.Code", "NZCHC", shipmentData.PortOfLoading.Code);
				AssertEquals("shipmentData.PortOfLoading.Name", "Christchurch", shipmentData.PortOfLoading.Name);
				AssertEquals("shipmentData.PortOfDischarge.Code", "AUSYD", shipmentData.PortOfDischarge.Code);
				AssertEquals("shipmentData.PortOfDischarge.Name", "Sydney", shipmentData.PortOfDischarge.Name);
			});
		}

		[TestDate(2017, 07, 03, 13, 01, 01)]
		public void TestWithContainersAndPackLines()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
				shipmentBO.JS_PackingMode = "LCL";
				shipmentBO.JS_ShipmentType = "STD"; // Standard House

				var consolBO = shipmentBO.Consols.AddNew();
				consolBO.JK_RL_NKDischargePort = "NZAKL";
				consolBO.JK_RL_NKLoadPort = "AUMEL";

				var commodity1 = GetCommodity(Factory.BOFactory);

				var container1 = consolBO.Containers.AddNew();
				ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

				var packLine1 = shipmentBO.OuterPackLines.AddNew();
				PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
				packLine1.SetContainer(container1.PK);
				packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

				AssertEquals("Precondition: packLine1.JL_OutturnedVolume", 37.539m, packLine1.JL_OutturnedVolume);
				AssertEquals("Precondition: packLine1.JL_ActualVolume", 87.943m, packLine1.JL_ActualVolume);

				AssertEquals("Precondition: container1.GoodsWeightForBinding", 5158.817m, container1.GoodsWeightForBinding);
				AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);

				AssertNotNull("shipmentData.ContainerCollection", shipmentData.ContainerCollection);
				AssertEquals("shipmentData.ContainerCollection.Count", 1, shipmentData.ContainerCollection.Count);
				CombineAssertions(delegate
				{
					AssertEquals("containerData1.GrossWeight", 146.90m, shipmentData.ContainerCollection[0].GrossWeight);
					AssertEquals("containerData1.GoodsWeight", 5158.817m, shipmentData.ContainerCollection[0].GoodsWeight);
					ContainerDataObjectWriterTest.AssertContainer1(shipmentData.ContainerCollection[0]);
				});

				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packLineDataObject = shipmentData.PackingLineCollection[0];
					PackingLineDataObjectWriterTest.AssertPackLine(packLineDataObject);
					AssertEquals("packingLineData.ContainerNumber", "OOCL0000027", packLineDataObject.ContainerNumber);
					AssertEquals("packingLineData.Commodity.Code", "FGFG", packLineDataObject.Commodity.Code);
					AssertEquals("packingLineData.Commodity.Description", "Fudge Guts Fingers Gone", packLineDataObject.Commodity.Description);
				});
			}
		}

		[TestDate(2017, 07, 03, 13, 01, 01)]
		public void Test2012NamespaceWithContainersAndPackLines()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
				shipmentBO.JS_PackingMode = "LCL";
				shipmentBO.JS_ShipmentType = "STD"; // Standard House

				var consolBO = shipmentBO.Consols.AddNew();
				consolBO.JK_RL_NKDischargePort = "NZAKL";
				consolBO.JK_RL_NKLoadPort = "AUMEL";

				var commodity1 = GetCommodity(Factory.BOFactory);

				var container1 = consolBO.Containers.AddNew();
				ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

				var packLine1 = shipmentBO.OuterPackLines.AddNew();
				PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
				packLine1.SetContainer(container1.PK);
				packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

				AssertEquals("Precondition: packLine1.JL_OutturnedVolume", 37.539m, packLine1.JL_OutturnedVolume);
				AssertEquals("Precondition: packLine1.JL_ActualVolume", 87.943m, packLine1.JL_ActualVolume);

				AssertEquals("Precondition: container1.GoodsWeightForBinding", 5158.817m, container1.GoodsWeightForBinding);
				AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

				UniversalShipment shipmentData;
				using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
				{
					var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
					shipmentData = writer.GetDataObject(shipmentBO);
				}

				AssertNotNull("Precondition: shipmentData", shipmentData);

				AssertNull("shipmentData.ContainerCollection", shipmentData.ContainerCollection);

				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packLineDataObject = shipmentData.PackingLineCollection[0];
					PackingLineDataObjectWriterTest.AssertPackLine(packLineDataObject);
					AssertEquals("packingLineData.ContainerNumber", "OOCL0000027", packLineDataObject.ContainerNumber);
					AssertEquals("packingLineData.Commodity.Code", "FGFG", packLineDataObject.Commodity.Code);
					AssertEquals("packingLineData.Commodity.Description", "Fudge Guts Fingers Gone", packLineDataObject.Commodity.Description);
				});

				AssertEquals("shipmentData.ParentShipmentCollection.Count", 1, shipmentData.ParentShipmentCollection.Count);
				var consolData = shipmentData.ParentShipmentCollection[0];
				AssertEquals("consolData.ContainerCollection.Count", 1, consolData.ContainerCollection.Count);

				CombineAssertions(delegate
				{
					var containerData = consolData.ContainerCollection[0];
					AssertEquals("containerData1.GrossWeight", 146.90m, containerData.GrossWeight);
					AssertEquals("containerData1.GoodsWeight", 5158.817m, containerData.GoodsWeight);
					ContainerDataObjectWriterTest.AssertContainer1(containerData);
				});
			}
		}

		public void TestPackLineToContainerLink_PackLinesHasContainers_PackLinesShouldHaveLinksToContainers()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();

			var commodity1 = GetCommodity(Factory.BOFactory, "HAM", "McLaren - CHEMPION!");
			var commodity2 = GetCommodity(Factory.BOFactory, "BUT", "McLaren - CHEMPION!");

			var container1 = consolBO.Containers.AddNew();
			var container2 = consolBO.Containers.AddNew();

			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);
			ContainerDataObjectWriterTest.PopulateContainer1(container2, commodity2, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);

			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

			packLine2.SetContainer(container2.PK);
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull("shipmentData.ContainerCollection", shipmentData.ContainerCollection);
			AssertEquals("shipmentData.ContainerCollection.Count", 2, shipmentData.ContainerCollection.Count);
			AssertEquals("Container1 has link", true, shipmentData.ContainerCollection[0].Link.HasValue);
			AssertEquals("Container2 has link", true, shipmentData.ContainerCollection[1].Link.HasValue);

			AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
			AssertEquals("shipmentData.PackingLineCollection.Count", 2, shipmentData.PackingLineCollection.Count);
			AssertEquals("PackLine1 has link to container", true, shipmentData.PackingLineCollection[0].ContainerLink.HasValue);
			AssertEquals("PackLine2 has link to container", true, shipmentData.PackingLineCollection[1].ContainerLink.HasValue);
			AssertEquals("PackLine1 is linked to appropriate container", shipmentData.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "HAM").Link, shipmentData.PackingLineCollection[0].ContainerLink);
			AssertEquals("PackLine2 is linked to appropriate container", shipmentData.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "BUT").Link, shipmentData.PackingLineCollection[1].ContainerLink);
		}

		public void TestPackLineToContainerLink_PackLinesHasContainers_PackLinesShouldHaveLinksToContainers_2012Namespace()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();

			var commodity1 = GetCommodity(Factory.BOFactory, "HAM", "McLaren - CHEMPION!");
			var commodity2 = GetCommodity(Factory.BOFactory, "BUT", "McLaren - CHEMPION!");

			var container1 = consolBO.Containers.AddNew();
			var container2 = consolBO.Containers.AddNew();

			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);
			ContainerDataObjectWriterTest.PopulateContainer1(container2, commodity2, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);

			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

			packLine2.SetContainer(container2.PK);
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;

			UniversalShipment shipmentData;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				shipmentData = writer.GetDataObject(shipmentBO);
			}

			// containers
			AssertNull("shipmentData.ContainerCollection", shipmentData.ContainerCollection);
			AssertNotNull("shipmentData.ParentShipmentCollection", shipmentData.ParentShipmentCollection);
			AssertEquals("shipmentData.ParentShipmentCollection.Count", 1, shipmentData.ParentShipmentCollection.Count);

			var consolData = shipmentData.ParentShipmentCollection[0];
			AssertNotNull("shipmentData.ContainerCollection", consolData.ContainerCollection);
			AssertEquals("shipmentData.ContainerCollection.Count", 2, consolData.ContainerCollection.Count);
			AssertEquals("Container1 has link", true, consolData.ContainerCollection[0].Link.HasValue);
			AssertEquals("Container2 has link", true, consolData.ContainerCollection[1].Link.HasValue);

			// packlines
			AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
			AssertEquals("shipmentData.PackingLineCollection.Count", 2, shipmentData.PackingLineCollection.Count);
			AssertEquals("PackLine1 has link to container", true, shipmentData.PackingLineCollection[0].ContainerLink.HasValue);
			AssertEquals("PackLine2 has link to container", true, shipmentData.PackingLineCollection[1].ContainerLink.HasValue);
			AssertEquals("PackLine1 is linked to appropriate container", consolData.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "HAM").Link, shipmentData.PackingLineCollection[0].ContainerLink);
			AssertEquals("PackLine2 is linked to appropriate container", consolData.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "BUT").Link, shipmentData.PackingLineCollection[1].ContainerLink);
		}

		public void TestPackLineToContainerLink_PackLinesLinkToCorrectContainers()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "STD";

			var consolBO = shipmentBO.Consols.AddNew();
			var containerBO1 = consolBO.Containers.AddNew();
			var containerBO2 = consolBO.Containers.AddNew();
			var containerBO3 = consolBO.Containers.AddNew();
			var containerBO4 = consolBO.Containers.AddNew();
			containerBO1.JC_ContainerNum = "CONT1111111";
			containerBO2.JC_ContainerNum = "CONT2222222";
			containerBO3.JC_ContainerNum = "LAST9999999";
			containerBO4.JC_ContainerNum = "NOTPACKED";

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			var packLine3 = shipmentBO.OuterPackLines.AddNew();
			var packLine4 = shipmentBO.OuterPackLines.AddNew();
			var packLine5 = shipmentBO.OuterPackLines.AddNew();
			packLine1.JL_Description = "PACK1";
			packLine4.JL_Description = "PACK4";
			packLine1.SetContainer(containerBO2.PK);
			packLine2.SetContainer(containerBO2.PK);
			packLine3.SetContainer(containerBO1.PK);
			packLine4.JL_JC = ZGuid.Empty;
			packLine5.SetContainer(containerBO3.PK);

			Factory.SaveForTesting();

			var consolWriter = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = consolWriter.GetDataObject(consolBO);

			AssertConsolShipmentCorrectPackLinesAndContainerLink(consolData);

			var shipmentWriter = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var shipmenData = shipmentWriter.GetDataObject(shipmentBO);

			AssertConsolShipmentCorrectPackLinesAndContainerLink(shipmenData, true);
		}

		public void TestPackLinesWithDepartureTransitWarehouseExcludedWithDTW()
		{
			var shipmentBO = SetupShipmentForPackLinesWithDepartureTransitWarehouseExcluded();
			var writer = GetNewShipmentDataObjectWriter(shipmentBO, RecipientRoleType.DTW);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
			AssertEquals("shipmentData.PackingLineCollection.Count", 2, shipmentData.PackingLineCollection.Count);
			CombineAssertions(() =>
			{
				var packLineDataObject = shipmentData.PackingLineCollection[0];
				AssertEquals("packingLineData1", "good one", packLineDataObject.GoodsDescription);

				packLineDataObject = shipmentData.PackingLineCollection[1];
				AssertEquals("packingLineData3", "good three", packLineDataObject.GoodsDescription);
			});
		}

		public void TestPackLinesWithDepartureTransitWarehouseExcludedWithNonDTW()
		{
			var shipmentBO = SetupShipmentForPackLinesWithDepartureTransitWarehouseExcluded();
			var writer = GetNewShipmentDataObjectWriter(shipmentBO, RecipientRoleType.ORP);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
			AssertEquals("shipmentData.PackingLineCollection.Count", 3, shipmentData.PackingLineCollection.Count);
			CombineAssertions(() =>
			{
				var packLineDataObject = shipmentData.PackingLineCollection[0];
				AssertEquals("packingLineData1", "good one", packLineDataObject.GoodsDescription);

				packLineDataObject = shipmentData.PackingLineCollection[1];
				AssertEquals("packingLineData2", "good two", packLineDataObject.GoodsDescription);

				packLineDataObject = shipmentData.PackingLineCollection[2];
				AssertEquals("packingLineData3", "good three", packLineDataObject.GoodsDescription);
			});
		}

		ForwardingShipment SetupShipmentForPackLinesWithDepartureTransitWarehouseExcluded()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();

			var commodity1 = GetCommodity(Factory.BOFactory);

			var container1 = consolBO.Containers.AddNew();
			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine1.JL_DepartureTransitWarehouseExcluded = false;
			packLine1.JL_Description = "good one";

			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);
			packLine2.SetContainer(container1.PK);
			packLine2.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine2.JL_DepartureTransitWarehouseExcluded = true;
			packLine2.JL_Description = "good two";

			var packLine3 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine3);
			packLine3.SetContainer(container1.PK);
			packLine3.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine3.JL_DepartureTransitWarehouseExcluded = false;
			packLine3.JL_Description = "good three";

			return shipmentBO;
		}

		void AssertConsolShipmentCorrectPackLinesAndContainerLink(UniversalShipment consolData, bool wasBuiltFromShipment = false)
		{
			AssertNotNull("consolData", consolData);
			AssertNotNull("Consol data object has a shipment collection", consolData.SubShipmentCollection);
			AssertNotNull("Consol data object has containers", consolData.ContainerCollection);
			AssertEquals("Consol data object contains all expected containers", 4, consolData.ContainerCollection.Count);

			var shipmentData = consolData.SubShipmentCollection.First();
			AssertEquals("Shipment data object contains all expected packing lines", 5, shipmentData.PackingLineCollection.Count);

			if (wasBuiltFromShipment)
			{
				AssertNotNull("Should be included as when it was built, consol didn't exist", shipmentData.ContainerCollection);
				AssertEquals("Should only contain containers that were packed to this shipment", 3, shipmentData.ContainerCollection.Count);
			}
			else
			{
				AssertNull("Container collection should not have been created as consol already have same containers attached", shipmentData.ContainerCollection);
			}

			var container1 = consolData.ContainerCollection.First(c => c.ContainerNumber.Equals("CONT1111111"));
			var container2 = consolData.ContainerCollection.First(c => c.ContainerNumber.Equals("CONT2222222"));
			var container3 = consolData.ContainerCollection.First(c => c.ContainerNumber.Equals("LAST9999999"));

			AssertEquals("PackingLine1 has Container2's number", container2.ContainerNumber, shipmentData.PackingLineCollection[0].ContainerNumber);
			AssertEquals("PackingLine1 has link to Container2", container2.Link, shipmentData.PackingLineCollection[0].ContainerLink);

			AssertEquals("PackingLine2 has Container2's number", container2.ContainerNumber, shipmentData.PackingLineCollection[1].ContainerNumber);
			AssertEquals("PackingLine2 has link to Container2", container2.Link, shipmentData.PackingLineCollection[1].ContainerLink);

			AssertEquals("PackingLine3 has Container1's number", container1.ContainerNumber, shipmentData.PackingLineCollection[2].ContainerNumber);
			AssertEquals("PackingLine3 has link to Container1", container1.Link, shipmentData.PackingLineCollection[2].ContainerLink);

			AssertEquals("PackingLine4 should not have a container number", "", shipmentData.PackingLineCollection[3].ContainerNumber);
			AssertEquals("PackingLine4 should not be linked to any container", false, shipmentData.PackingLineCollection[3].ContainerLink.HasValue);

			AssertEquals("PackingLine5 has Container3's number", container3.ContainerNumber, shipmentData.PackingLineCollection[4].ContainerNumber);
			AssertEquals("PackingLine5 has link to Container3", container3.Link, shipmentData.PackingLineCollection[4].ContainerLink);
		}

		public void TestPackLineToContainerLink_HasCoLoadShipments()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "ASM";

			var shipmentBOSub1 = shipmentBO.CoLoadShipments.AddNew();
			shipmentBOSub1.JS_TransportMode = "SEA";
			shipmentBOSub1.JS_PackingMode = "FCL";
			shipmentBOSub1.JS_ShipmentType = "STD";

			var shipmentBOSub2 = shipmentBO.CoLoadShipments.AddNew();
			shipmentBOSub2.JS_TransportMode = "SEA";
			shipmentBOSub2.JS_PackingMode = "FCL";
			shipmentBOSub2.JS_ShipmentType = "STD";

			var consolBO = shipmentBO.Consols.AddNew();
			var containerBO1 = consolBO.Containers.AddNew();
			var containerBO2 = consolBO.Containers.AddNew();
			var containerBO3 = consolBO.Containers.AddNew();
			containerBO1.JC_ContainerNum = "FIRS1111111";
			containerBO2.JC_ContainerNum = "CONT2222222";
			containerBO3.JC_ContainerNum = "LAST3333333";

			var packLine1 = shipmentBOSub1.OuterPackLines.AddNew();
			var packLine2 = shipmentBOSub2.OuterPackLines.AddNew();
			packLine1.SetContainer(containerBO2.PK);
			packLine2.SetContainer(containerBO3.PK);

			Factory.SaveForTesting();

			var consolWriter = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = consolWriter.GetDataObject(consolBO);

			AssertConsolShipmentCorrectPackLinesAndContainerLink_HasCoLoadShipments(consolData);

			var shipmentWriter = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var shipmenData = shipmentWriter.GetDataObject(shipmentBO);

			AssertConsolShipmentCorrectPackLinesAndContainerLink_HasCoLoadShipments(shipmenData, true);
		}

		void AssertConsolShipmentCorrectPackLinesAndContainerLink_HasCoLoadShipments(UniversalShipment consolData, bool wasBuiltFromShipment = false)
		{
			AssertNotNull("consolData", consolData);
			AssertNotNull("Consol data object has a shipment collection", consolData.SubShipmentCollection);
			AssertNotNull("Consol data object has containers", consolData.ContainerCollection);
			AssertEquals("Consol data object contains all expected containers", 3, consolData.ContainerCollection.Count);

			var shipmentData = consolData.SubShipmentCollection.First();
			var shipmentDataSub1 = shipmentData.SubShipmentCollection[0];
			var shipmentDataSub2 = shipmentData.SubShipmentCollection[1];
			AssertEquals("shipmentData doesn't have packing lines", 0, shipmentData.PackingLineCollection.Count);
			AssertEquals("shipmentDataSub1 has a packing line", 1, shipmentDataSub1.PackingLineCollection.Count);
			AssertEquals("shipmentDataSub2 has a packing line", 1, shipmentDataSub2.PackingLineCollection.Count);

			var container2 = consolData.ContainerCollection.First(c => c.ContainerNumber.Equals("CONT2222222"));
			var container3 = consolData.ContainerCollection.First(c => c.ContainerNumber.Equals("LAST3333333"));

			AssertEquals("PackingLine1 has Container2's number", container2.ContainerNumber, shipmentDataSub1.PackingLineCollection[0].ContainerNumber);
			AssertEquals("PackingLine1 has link to Container2", container2.Link, shipmentDataSub1.PackingLineCollection[0].ContainerLink);

			AssertEquals("PackingLine2 has Container3's number", container3.ContainerNumber, shipmentDataSub2.PackingLineCollection[0].ContainerNumber);
			AssertEquals("PackingLine2 has link to Container3", container3.Link, shipmentDataSub2.PackingLineCollection[0].ContainerLink);
		}

		public void TestLinkOrderLineWithPackedItem()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Set up

			var order = shipmentBO.AttachedOrders.AddNew();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = 2;

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 2;
			orderLine1.JO_LineSplitNumber = 3;

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;
			orderLine2.JO_SubLineNo = 2;
			orderLine2.JO_LineSplitNumber = 3;

			var orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;
			orderLine3.JO_SubLineNo = 2;
			orderLine3.JO_LineSplitNumber = 3;

			var packLine = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine);

			var product1 = packLine.Products.AddNew();
			product1.D2_JO = orderLine1.PK;
			product1.D2_ProductCode = "MACARONS - GREEN TEA";
			product1.D2_ProductQuantity = 100m;
			product1.D2_ProductUnitOfQty = "BOX";

			var product2 = packLine.Products.AddNew();
			product2.D2_JO = orderLine2.PK;
			product2.D2_ProductCode = "MACARONS - STRAWBERRY AND CREAM";
			product2.D2_ProductQuantity = 200m;
			product2.D2_ProductUnitOfQty = "BOX";

			var product3 = packLine.Products.AddNew();
			product3.D2_JO = orderLine3.PK;
			product3.D2_ProductCode = "MACARONS - LEMON";
			product3.D2_ProductQuantity = 300m;
			product3.D2_ProductUnitOfQty = "BOX";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			#endregion

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals(1, shipmentData.PackingLineCollection.Count);
			AssertEquals(3, shipmentData.PackingLineCollection[0].PackedItemCollection.Count);

			AssertEquals(100m, shipmentData.PackingLineCollection[0].PackedItemCollection[0].PackedQuantity);
			AssertEquals(1, shipmentData.PackingLineCollection[0].PackedItemCollection[0].OrderLineLink);
			AssertEquals(200m, shipmentData.PackingLineCollection[0].PackedItemCollection[1].PackedQuantity);
			AssertEquals(2, shipmentData.PackingLineCollection[0].PackedItemCollection[1].OrderLineLink);
			AssertEquals(300m, shipmentData.PackingLineCollection[0].PackedItemCollection[2].PackedQuantity);
			AssertEquals(3, shipmentData.PackingLineCollection[0].PackedItemCollection[2].OrderLineLink);

			AssertEquals(1, shipmentData.RelatedShipmentCollection.Count);
			var orderData = shipmentData.RelatedShipmentCollection[0];
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(2), orderData.Order.OrderNumberSplit);

			AssertEquals(3, orderData.Order.OrderLineCollection.Count);
			AssertEquals(1, orderData.Order.OrderLineCollection[0].LineNumber);
			AssertEquals(1, orderData.Order.OrderLineCollection[0].Link);
			AssertEquals(2, orderData.Order.OrderLineCollection[1].LineNumber);
			AssertEquals(2, orderData.Order.OrderLineCollection[1].Link);
			AssertEquals(3, orderData.Order.OrderLineCollection[2].LineNumber);
			AssertEquals(3, orderData.Order.OrderLineCollection[2].Link);
		}

		public void TestWithLegs()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_AgentType = "AGT";
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_ConsolMode = "FCL";

			shipmentBO.JS_RL_NKOrigin = "NZDUD"; // Dunedin
			consolBO.JK_RL_NKLoadPort = "NZCHC"; // Christchurch
			consolBO.JK_RL_NKPortOfFirstArrival = "AUNTL"; // Newcastle
			consolBO.JK_RL_NKDischargePort = "AUSYD"; // Sydney
			shipmentBO.JS_RL_NKDestination = "AUBDG"; // Bendigo

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA DELIMA TEST";
			vessel.RV_LloydsNumber = "8907993";

			var leg = consolBO.Transports[0];
			leg.JW_Vessel = vessel.RV_FK;
			leg.JW_VoyageFlight = "343L";

			leg.JW_RL_NKLoadPort = "NZCHC"; // Christchurch
			leg.JW_ETD = new ZDateTime(2011, 3, 4);
			leg.JW_ATD = new ZDateTime(2011, 3, 5);

			leg.JW_RL_NKDiscPort = "AUSYD"; // Sydney
			leg.JW_ETA = new ZDateTime(2011, 3, 6);
			leg.JW_ATA = new ZDateTime(2011, 3, 7);

			leg.CarrierPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			leg.JW_CarrierBookingReference = "BOOKMEUP";

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);

			#region Check Contents of shipmentData object

			var legData1 = shipmentData.TransportLegCollection[0];
			CombineAssertions(delegate
			{
				AssertEquals("legData1.LegOrder", new ZByte(1), legData1.LegOrder);
				AssertEquals("legData1.TransportMode", TransportMode.Sea, legData1.TransportMode);
				AssertEquals("legData1.ActualArrival", new ZDateTime(2011, 3, 7), legData1.ActualArrival);
				AssertEquals("legData1.ActualDeparture", new ZDateTime(2011, 3, 5), legData1.ActualDeparture);
				AssertEquals("legData1.CarrierBookingReference", "BOOKMEUP", legData1.CarrierBookingReference);
				AssertEquals("legData1.EstimatedArrival", new ZDateTime(2011, 3, 6), legData1.EstimatedArrival);
				AssertEquals("legData1.EstimatedDeparture", new ZDateTime(2011, 3, 4), legData1.EstimatedDeparture);
				AssertEquals("legData1.LegType", LegType.Main, legData1.LegType);
				AssertEquals("legData1.PortOfDischarge.Code", "AUSYD", legData1.PortOfDischarge.Code);
				AssertEquals("legData1.PortOfDischarge.Name", "Sydney", legData1.PortOfDischarge.Name);
				AssertEquals("legData1.PortOfLoading.Code", "NZCHC", legData1.PortOfLoading.Code);
				AssertEquals("legData1.PortOfLoading.Name", "Christchurch", legData1.PortOfLoading.Name);
				AssertEquals("legData1.VesselName", "BUNGA DELIMA TEST", legData1.VesselName);
				AssertEquals("legData1.VesselLloydsNumber", "8907993", legData1.VesselLloydsIMO);
				AssertEquals("legData1.VoyageFlightNo", "343L", legData1.VoyageFlightNo);
			});

			var carrierData = legData1.Carrier;
			AssertNotNull("legData1.Carrier", carrierData);

			CombineAssertions(delegate
			{
				AssertEquals("carrierData.AddressType", "Carrier", carrierData.AddressType);
				AssertEquals("carrierData.OrganizationCode", "WUFSHIJNB", carrierData.OrganizationCode);
				AssertEquals("carrierData.Address1", "Level 2, Building G", carrierData.Address1);
				AssertEquals("carrierData.Address2", "34 Dock Lane", carrierData.Address2);
				AssertEquals("carrierData.AddressOverride", ZBool.False, carrierData.AddressOverride);
				AssertEquals("carrierData.City", "Johannesburg", carrierData.City);
				AssertEquals("carrierData.CompanyName", "WUFU SHIPPING LINE", carrierData.CompanyName);
				AssertEquals("carrierData.Contact", null, carrierData.Contact);
				AssertEquals("carrierData.Country.Code", "ZA", carrierData.Country.Code);
				AssertEquals("carrierData.Country.Name", "South Africa", carrierData.Country.Name);
				AssertEquals("carrierData.Email", "", carrierData.Email);
				AssertEquals("carrierData.Fax", "", carrierData.Fax);
				AssertEquals("carrierData.GovRegNum", "TAXME", carrierData.GovRegNum);
				AssertEquals("carrierData.GovRegNumType.Code", "ABN", carrierData.GovRegNumType.Code);
				AssertEquals("carrierData.GovRegNumType.Description", "Australian Business Number (GST Registration Code)", carrierData.GovRegNumType.Description);
				AssertEquals("carrierData.Mobile", null, carrierData.Mobile);
				AssertEquals("carrierData.Phone", "", carrierData.Phone);
				AssertEquals("carrierData.Postcode", "12345", carrierData.Postcode);
				AssertEquals("carrierData.ScreeningStatus.Code", "UNK", carrierData.ScreeningStatus.Code);
				AssertEquals("carrierData.ScreeningStatus.Description", "Unknown", carrierData.ScreeningStatus.Description);
				AssertEquals("carrierData.State", ZString.Empty, carrierData.State);
				AssertEquals("carrierData.UniversalNettingCode", "GOFISH", carrierData.UniversalNettingCode);
				AssertEquals("carrierData.UniversalOfficeCode", "AWESOME", carrierData.UniversalOfficeCode);
			});

			AssertNotNull("carrierData.RegistrationNumberCollection", carrierData.RegistrationNumberCollection);
			AssertEquals("carrierData.RegistrationNumberCollection.Count", 1, carrierData.RegistrationNumberCollection.Count);

			var registrationNumber = carrierData.RegistrationNumberCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("registrationNumber.CountryOfIssue.Code", "ZA", registrationNumber.CountryOfIssue.Code);
				AssertEquals("registrationNumber.CountryOfIssue.Name", "South Africa", registrationNumber.CountryOfIssue.Name);
				AssertEquals("registrationNumber.Type.Code", "CCC", registrationNumber.Type.Code);
				AssertEquals("registrationNumber.Type.Description", "Customs Carrier Code", registrationNumber.Type.Description);
				AssertEquals("registrationNumber.Value", "FLOG", registrationNumber.Value);
			});

			#endregion
		}

		public void Test2012NamespaceWithLegs()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_AgentType = "AGT";
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_ConsolMode = "FCL";

			shipmentBO.JS_RL_NKOrigin = "NZDUD"; // Dunedin
			consolBO.JK_RL_NKLoadPort = "NZCHC"; // Christchurch
			consolBO.JK_RL_NKPortOfFirstArrival = "AUNTL"; // Newcastle
			consolBO.JK_RL_NKDischargePort = "AUSYD"; // Sydney
			shipmentBO.JS_RL_NKDestination = "AUBDG"; // Bendigo

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA DELIMA TEST";
			vessel.RV_LloydsNumber = "8907993";

			var shipmentLeg = shipmentBO.Transports.AddNew();
			shipmentLeg.JW_Vessel = vessel.RV_FK;
			shipmentLeg.JW_VoyageFlight = "343L";

			shipmentLeg.JW_RL_NKLoadPort = "NZCHC"; // Christchurch
			shipmentLeg.JW_ETD = new ZDateTime(2011, 3, 4);
			shipmentLeg.JW_ATD = new ZDateTime(2011, 3, 5);

			shipmentLeg.JW_RL_NKDiscPort = "AUSYD"; // Sydney
			shipmentLeg.JW_ETA = new ZDateTime(2011, 3, 6);
			shipmentLeg.JW_ATA = new ZDateTime(2011, 3, 7);

			var consolLeg = consolBO.Transports[0];
			consolLeg.JW_Vessel = vessel.RV_FK;
			consolLeg.JW_VoyageFlight = "454L";

			consolLeg.JW_RL_NKLoadPort = "AUNTL"; // Newcastle
			consolLeg.JW_ETD = new ZDateTime(2011, 3, 14);
			consolLeg.JW_ATD = new ZDateTime(2011, 3, 15);

			consolLeg.JW_RL_NKDiscPort = "AUSYD"; // Sydney
			consolLeg.JW_ETA = new ZDateTime(2011, 3, 16);
			consolLeg.JW_ATA = new ZDateTime(2011, 3, 17);

			#endregion

			UniversalShipment shipmentData;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				shipmentData = writer.GetDataObject(shipmentBO);
			}

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("shipmentData.TransportLegCollection.Count", 1, shipmentData.TransportLegCollection.Count);
			var shipmentLegData = shipmentData.TransportLegCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("shipmentLegData.LegOrder", new ZByte(0), shipmentLegData.LegOrder);
				AssertEquals("shipmentLegData.TransportMode", TransportMode.Sea, shipmentLegData.TransportMode);
				AssertEquals("shipmentLegData.ActualArrival", new ZDateTime(2011, 3, 7), shipmentLegData.ActualArrival);
				AssertEquals("shipmentLegData.ActualDeparture", new ZDateTime(2011, 3, 5), shipmentLegData.ActualDeparture);
				AssertEquals("shipmentLegData.EstimatedArrival", new ZDateTime(2011, 3, 6), shipmentLegData.EstimatedArrival);
				AssertEquals("shipmentLegData.EstimatedDeparture", new ZDateTime(2011, 3, 4), shipmentLegData.EstimatedDeparture);
				AssertEquals("shipmentLegData.LegType", LegType.Main, shipmentLegData.LegType);
				AssertEquals("shipmentLegData.PortOfDischarge.Code", "AUSYD", shipmentLegData.PortOfDischarge.Code);
				AssertEquals("shipmentLegData.PortOfDischarge.Name", "Sydney", shipmentLegData.PortOfDischarge.Name);
				AssertEquals("shipmentLegData.PortOfLoading.Code", "NZCHC", shipmentLegData.PortOfLoading.Code);
				AssertEquals("shipmentLegData.PortOfLoading.Name", "Christchurch", shipmentLegData.PortOfLoading.Name);
				AssertEquals("shipmentLegData.VesselName", "BUNGA DELIMA TEST", shipmentLegData.VesselName);
				AssertEquals("shipmentLegData.VesselLloydsNumber", "8907993", shipmentLegData.VesselLloydsIMO);
				AssertEquals("shipmentLegData.VoyageFlightNo", "343L", shipmentLegData.VoyageFlightNo);
			});

			AssertEquals("shipmentData.ParentShipmentCollection.Count", 1, shipmentData.ParentShipmentCollection.Count);
			var consolData = shipmentData.ParentShipmentCollection[0];
			AssertEquals("consolData.TransportLegCollection.Count", 1, consolData.TransportLegCollection.Count);
			var consolLegData = consolData.TransportLegCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("consolLegData.LegOrder", new ZByte(1), consolLegData.LegOrder);
				AssertEquals("consolLegData.TransportMode", TransportMode.Sea, consolLegData.TransportMode);
				AssertEquals("consolLegData.ActualArrival", new ZDateTime(2011, 3, 17), consolLegData.ActualArrival);
				AssertEquals("consolLegData.ActualDeparture", new ZDateTime(2011, 3, 15), consolLegData.ActualDeparture);
				AssertEquals("consolLegData.EstimatedArrival", new ZDateTime(2011, 3, 16), consolLegData.EstimatedArrival);
				AssertEquals("consolLegData.EstimatedDeparture", new ZDateTime(2011, 3, 14), consolLegData.EstimatedDeparture);
				AssertEquals("consolLegData.LegType", LegType.Main, consolLegData.LegType);
				AssertEquals("consolLegData.PortOfDischarge.Code", "AUSYD", consolLegData.PortOfDischarge.Code);
				AssertEquals("consolLegData.PortOfDischarge.Name", "Sydney", consolLegData.PortOfDischarge.Name);
				AssertEquals("consolLegData.PortOfLoading.Code", "AUNTL", consolLegData.PortOfLoading.Code);
				AssertEquals("consolLegData.PortOfLoading.Name", "Newcastle", consolLegData.PortOfLoading.Name);
				AssertEquals("consolLegData.VesselName", "BUNGA DELIMA TEST", consolLegData.VesselName);
				AssertEquals("consolLegData.VesselLloydsNumber", "8907993", consolLegData.VesselLloydsIMO);
				AssertEquals("consolLegData.VoyageFlightNo", "454L", consolLegData.VoyageFlightNo);
			});

			#endregion
		}

		public void TestClientContractNumber()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			shipmentBO.CreateShipmentJobHeaderWithMutex();
			shipmentBO.ShipmentJobHeader.JH_ClientContractNumber = "1234";
			shipmentBO.ShipmentJobHeader.Dispose();

			Factory.SaveForTesting();

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(shipmentData.JobCosting.ClientContractNumber, "1234");
		}

		public void TestOrganizations_NoDefaultContactInfo()
		{
			#region Setup shipmentBO

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_ConsolMode = "LCL";

			consolBO.JK_OA_SendingForwarderAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			var pickupAddress = shipmentBO.ConsignorPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_Address1 = "FOO";
			pickupAddress.E2_Address2 = "FIGHTERS";
			pickupAddress.E2_City = "ROCK THIS TOWN";
			pickupAddress.E2_State = "BLOWNAWAY";
			pickupAddress.E2_Postcode = "2344";
			pickupAddress.E2_RN_NKCountryCode = "DE";

			shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;

			var deliveryAddress = shipmentBO.ConsigneeDeliveryAddress;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_Contact = "JOHNNY ROTTEN";
			deliveryAddress.E2_Email = "rotten@sexpistols.com";
			deliveryAddress.E2_Phone = "432890234";
			deliveryAddress.E2_Fax = "54309823";
			deliveryAddress.E2_Mobile = "3452432890";

			shipmentBO.CreateShipmentJobHeaderWithMutex();
			shipmentBO.ShipmentJobHeader.Dispose(); // Unlocks Mutex

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);

			AssertOrganizationBO_WUFSHIJNB("ConsignorDocumentaryAddress",
				shipmentData.OrganizationAddressCollection.ElementAt(0),
				"ConsignorDocumentaryAddress",
				true);

			AssertAddress(shipmentData.OrganizationAddressCollection, 1
				, "ConsignorPickupDeliveryAddress", null, "WUFU SHIPPING LINE", true
				, "FOO", "FIGHTERS", "ROCK THIS TOWN", "BLOWNAWAY", "2344", "DE"
				, "Benny Banana", "benny.banana@wufu.co.za", "0011 54 392 2921", "0011 289 392 2900", "0011 54 392 2900");

			AssertOrganizationBO_CRAHOLSYD("ConsigneeDocumentaryAddress",
				shipmentData.OrganizationAddressCollection.ElementAt(2),
				"ConsigneeDocumentaryAddress",
				true);

			AssertAddress(shipmentData.OrganizationAddressCollection, 3
				, "ConsigneePickupDeliveryAddress", null, "CRACKERJACK HOLDINGS", true
				, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU"
				, "JOHNNY ROTTEN", "rotten@sexpistols.com", "54309823", "3452432890", "432890234");

			AssertOrganizationBO_CRAHOLSYD("NotifyParty",
				shipmentData.OrganizationAddressCollection.ElementAt(4),
				"NotifyParty",
				true);

			AssertOrganizationBO_CRAHOLSYD("SendersLocalClient",
				shipmentData.OrganizationAddressCollection.ElementAt(5),
				"SendersLocalClient");

			AssertOrganizationBO_INTHEMSYD("SendersOverseasAgent",
				shipmentData.OrganizationAddressCollection.ElementAt(6),
				"SendersOverseasAgent");

			#endregion
		}

		public void TestLocalProcessing()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var docsBO = shipmentBO.DocsAndCartage;

			docsBO.JP_FCLPickupEquipmentNeeded = "TRL";
			docsBO.JP_EstimatedPickup = new ZDateTime(2011, 5, 1);
			docsBO.JP_PickupRequiredBy = new ZDateTime(2011, 5, 2);
			docsBO.JP_PickupCartageAdvised = new ZDateTime(2011, 5, 3);
			docsBO.JP_ArrivalCartageRef = "ARRCARTREF";
			docsBO.JP_PickupCartageCompleted = new ZDateTime(2011, 5, 4);
			docsBO.JP_PickupLabourTime = new ZDateTime(2011, 5, 5);
			docsBO.JP_PickupLabourCharge = 1.11m;
			docsBO.JP_PickupTruckWaitTime = new ZDateTime(2011, 5, 6);
			docsBO.JP_PickupTruckWaitCharge = 2.22m;
			docsBO.JP_PrintOptionForPackagesOnAWB = "ALL";
			docsBO.JP_FCLDeliveryEquipmentNeeded = "WUP";
			docsBO.JP_FCLAvailable = new ZDateTime(2011, 5, 7);
			docsBO.JP_FCLStorageCommences = new ZDateTime(2011, 5, 8);
			docsBO.JP_LCLAvailable = new ZDateTime(2011, 5, 9);
			docsBO.JP_LCLStorageCommences = new ZDateTime(2011, 5, 10);
			docsBO.JP_LCLAirStorageDaysOrHours = new ZByte(12);
			docsBO.JP_LCLAirStorageCharge = 3.33m;
			docsBO.JP_EstimatedDelivery = new ZDateTime(2011, 5, 11);
			docsBO.JP_DeliveryRequiredBy = new ZDateTime(2011, 5, 12);
			docsBO.JP_DeliveryCartageAdvised = new ZDateTime(2011, 5, 13);
			docsBO.JP_DeliveryCartageCompleted = new ZDateTime(2011, 5, 14);
			docsBO.JP_DeliveryLabourTime = new ZDateTime(2011, 5, 15);
			docsBO.JP_DeliveryLabourCharge = 4.44m;
			docsBO.JP_DeliveryTruckWaitTime = new ZDateTime(2011, 5, 16);
			docsBO.JP_DeliveryTruckWaitCharge = 5.55m;
			docsBO.JP_HasProhibitedPackaging = ZBool.True;
			docsBO.JP_InsuranceRequired = ZBool.False;
			docsBO.JP_IsContingencyRelease = ZBool.True;
			docsBO.JP_LCLDatesOverrideConsol = ZBool.True;
			docsBO.JP_ExportStatement = "DEF";

			var orderBO = docsBO.OrderItems.AddNew();
			orderBO.JT_Sequence = 1;
			orderBO.JT_OrderReference = "ORDER_FF";

			var attachedOrder = shipmentBO.AttachedOrders.AddNew();
			attachedOrder.JD_OrderNumber = "ORDER_AA";

			var serviceBO = docsBO.Services.AddNew();
			serviceBO.ES_ServiceCode = "TAI";
			serviceBO.ES_Booked = new ZDateTime(2011, 6, 1);
			serviceBO.ES_Completed = new ZDateTime(2011, 6, 2);
			serviceBO.ES_Duration = new ZDateTime(2011, 6, 3);
			serviceBO.ES_ServiceCount = 1.23m;
			serviceBO.ES_ServiceNote = "BLAH BLAH BLAH BLAH Hotdog BLAH BLAH BLAH.";
			serviceBO.ES_References = "GREAT!";

			serviceBO.ES_OH_Contractor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			serviceBO.ES_OA_Location = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			var localProcessing = shipmentData.LocalProcessing;
			AssertNotNull("shipmentData.LocalProcessing", localProcessing);

			CombineAssertions("Checking all fields on LocalProcessing", delegate
			{
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Code", "TRL", localProcessing.FCLPickupEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Description", "Drop Trailer", localProcessing.FCLPickupEquipmentNeeded.Description);
				AssertEquals("localProcessing.EstimatedPickup", new ZDateTime(2011, 5, 1), localProcessing.EstimatedPickup);
				AssertEquals("localProcessing.PickupRequiredBy", new ZDateTime(2011, 5, 2), localProcessing.PickupRequiredBy);
				AssertEquals("localProcessing.PickupCartageAdvised", new ZDateTime(2011, 5, 3), localProcessing.PickupCartageAdvised);
				AssertEquals("localProcessing.ArrivalCartageRef", "ARRCARTREF", localProcessing.ArrivalCartageRef);
				AssertEquals("localProcessing.PickupCartageCompleted", new ZDateTime(2011, 5, 4), localProcessing.PickupCartageCompleted);
				AssertEquals("localProcessing.PickupLabourTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 5), localProcessing.PickupLabourTime);
				AssertEquals("localProcessing.PickupLabourCharge", 1.11m, localProcessing.PickupLabourCharge);
				AssertEquals("localProcessing.DemurrageOnPickupTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 6), localProcessing.DemurrageOnPickupTime);
				AssertEquals("localProcessing.PickupTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 6), localProcessing.PickupTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnPickupCharge", 2.22m, localProcessing.DemurrageOnPickupCharge);
				AssertEquals("localProcessing.PickupTruckWaitCharge", 2.22m, localProcessing.PickupTruckWaitCharge);
				AssertEquals("localProcessing.PrintOptionForPackagesOnAWB.Code", "ALL", localProcessing.PrintOptionForPackagesOnAWB.Code);
				AssertEquals("localProcessing.PrintOptionForPackagesOnAWB.Description", "Both Dimensions and Volume if available", localProcessing.PrintOptionForPackagesOnAWB.Description);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Code", "WUP", localProcessing.FCLDeliveryEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Description", "Wait for Pack/Unpack", localProcessing.FCLDeliveryEquipmentNeeded.Description);
				AssertEquals("localProcessing.FCLAvailable", new ZDateTime(2011, 5, 7), localProcessing.FCLAvailable);
				AssertEquals("localProcessing.FCLStorageCommences", new ZDateTime(2011, 5, 8), localProcessing.FCLStorageCommences);
				AssertEquals("localProcessing.LCLAvailable", new ZDateTime(2011, 5, 9), localProcessing.LCLAvailable);
				AssertEquals("localProcessing.LCLStorageCommences", new ZDateTime(2011, 5, 10), localProcessing.LCLStorageCommences);
				AssertEquals("localProcessing.LCLAirStorageDaysOrHours", new ZByte(12), localProcessing.LCLAirStorageDaysOrHours);
				AssertEquals("localProcessing.LCLAirStorageCharge", 3.33m, localProcessing.LCLAirStorageCharge);
				AssertEquals("localProcessing.EstimatedDelivery", new ZDateTime(2011, 5, 11), localProcessing.EstimatedDelivery);
				AssertEquals("localProcessing.DeliveryRequiredBy", new ZDateTime(2011, 5, 12), localProcessing.DeliveryRequiredBy);
				AssertEquals("localProcessing.DeliveryCartageAdvised", new ZDateTime(2011, 5, 13), localProcessing.DeliveryCartageAdvised);
				AssertEquals("localProcessing.DeliveryCartageCompleted", new ZDateTime(2011, 5, 14), localProcessing.DeliveryCartageCompleted);
				AssertEquals("localProcessing.DeliveryLabourTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 15), localProcessing.DeliveryLabourTime);
				AssertEquals("localProcessing.DeliveryLabourCharge", 4.44m, localProcessing.DeliveryLabourCharge);
				AssertEquals("localProcessing.DemurrageOnDeliveryTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 16), localProcessing.DemurrageOnDeliveryTime);
				AssertEquals("localProcessing.DeliveryTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 5, 16), localProcessing.DeliveryTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnDeliveryCharge", 5.55m, localProcessing.DemurrageOnDeliveryCharge);
				AssertEquals("localProcessing.DeliveryTruckWaitCharge", 5.55m, localProcessing.DeliveryTruckWaitCharge);
				AssertEquals("localProcessing.HasProhibitedPackaging", ZBool.True, localProcessing.HasProhibitedPackaging);
				AssertEquals("localProcessing.InsuranceRequired", ZBool.False, localProcessing.InsuranceRequired);
				AssertEquals("localProcessing.IsContingencyRelease", ZBool.True, localProcessing.IsContingencyRelease);
				AssertEquals("localProcessing.LCLDatesOverrideConsol", ZBool.True, localProcessing.LCLDatesOverrideConsol);
				AssertEquals("localProcessing.ExportStatement.Code", "DEF", localProcessing.ExportStatement.Code);
				AssertContains("localProcessing.ExportStatement.Description", "Exporter Statements are defined in the System Registry", localProcessing.ExportStatement.Description.Value);

				AssertNotNull("localProcessing.OrderNumberCollection", localProcessing.OrderNumberCollection);
				AssertNotNull("localProcessing.AdditionalServiceCollection", localProcessing.AdditionalServiceCollection);
			});

			AssertEquals("localProcessing.OrderNumberCollection.Count", 2, localProcessing.OrderNumberCollection.Count);
			var orderNumber1 = localProcessing.OrderNumberCollection[0];
			AssertEquals("orderNumber.Sequence", new ZShort(1), orderNumber1.Sequence);
			AssertEquals("orderNumber.OrderReference", "ORDER_FF", orderNumber1.OrderReference);

			var orderNumber2 = localProcessing.OrderNumberCollection[1];
			AssertEquals("orderNumber.Sequence", new ZShort(2), orderNumber2.Sequence);
			AssertEquals("orderNumber.OrderReference", "ORDER_AA", orderNumber2.OrderReference);

			AssertEquals("localProcessing.AdditionalServiceCollection.Count", 1, localProcessing.AdditionalServiceCollection.Count);
			var additionalService = localProcessing.AdditionalServiceCollection[0];

			CombineAssertions("Checking all fields on AdditionalService", delegate
			{
				AssertEquals("additionalService.ServiceCode.Code", "TAI", additionalService.ServiceCode.Code);
				AssertEquals("additionalService.ServiceCode.Description", "Tailgate", additionalService.ServiceCode.Description);
				AssertEquals("additionalService.Booked", new ZDateTime(2011, 6, 1), additionalService.Booked);
				AssertEquals("additionalService.Completed", new ZDateTime(2011, 6, 2), additionalService.Completed);
				AssertEquals("additionalService.Duration", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 3), additionalService.Duration);
				AssertEquals("additionalService.ServiceCount", 1.23m, additionalService.ServiceCount);
				AssertEquals("additionalService.ServiceNote", "BLAH BLAH BLAH BLAH Hotdog BLAH BLAH BLAH.", additionalService.ServiceNote);
				AssertEquals("additionalService.References", "GREAT!", additionalService.References);
			});

			AssertOrganizationBO_WUFSHIJNB("additionalService.Contractor", additionalService.Contractor, "Contractor");
			AssertOrganizationBO_CRAHOLSYD("additionalService.Location", additionalService.Location, "Location");

			#endregion
		}

		public void TestLocalProcessing_OrderNumberCollectionFromLinkedOrder()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var order1BO = shipmentBO.AttachedOrders.AddNew();
			order1BO.JD_OrderNumber = "number1";
			var order2BO = shipmentBO.AttachedOrders.AddNew();
			order2BO.JD_OrderNumber = "number2";

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			var localProcessing = shipmentData.LocalProcessing;
			AssertNotNull("shipmentData.LocalProcessing", localProcessing);

			AssertEquals("localProcessing.OrderNumberCollection.Count", 2, localProcessing.OrderNumberCollection.Count);
			var orderNumber1 = localProcessing.OrderNumberCollection[0];
			AssertEquals("first orderNumber.Sequence", new ZShort(1), orderNumber1.Sequence);
			AssertEquals("first orderNumber.OrderReference", "number1", orderNumber1.OrderReference);
			var orderNumber2 = localProcessing.OrderNumberCollection[1];
			AssertEquals("second orderNumber.Sequence", new ZShort(2), orderNumber2.Sequence);
			AssertEquals("second orderNumber.OrderReference", "number2", orderNumber2.OrderReference);

			#endregion
		}

		public void TestNotes()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			var noteBO1 = shipmentBO.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			var noteBO2 = shipmentBO.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.NoteCollection", shipmentData.NoteCollection);
			AssertEquals("shipmentData.NoteCollection.Count", 2, shipmentData.NoteCollection.Count);

			var note1 = shipmentData.NoteCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = shipmentData.NoteCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});

			#endregion
		}

		public void TestDates()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_A_BKD = new ZDateTime(2011, 3, 11);
			shipmentBO.JS_A_RCV = new ZDateTime(2011, 3, 12);
			shipmentBO.JS_E_DEP = new ZDateTime(2011, 3, 13);
			shipmentBO.JS_E_ARV = new ZDateTime(2011, 3, 14);
			shipmentBO.JS_ShippedOnBoardDate = new ZDateTime(2011, 3, 15);
			shipmentBO.JS_HouseBillIssueDate = new ZDateTime(2011, 3, 16);
			shipmentBO.JS_ExportReceivingDepotReceiptRequested = new ZDateTime(2016, 7, 20);
			shipmentBO.JS_ImportReleaseDepotReceiptRequested = new ZDateTime(2016, 7, 23);
			shipmentBO.JS_ExportReceivingDepotDispatchRequested = new ZDateTime(2016, 7, 24);
			shipmentBO.JS_ImportReleaseDepotDispatchRequested = new ZDateTime(2016, 7, 25);
			shipmentBO.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "For Test";
			shipmentBO.JS_DeliveryDueDate = new ZDateTime(2016, 7, 25);
			shipmentBO.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2016, 7, 26);

			#endregion

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.DateCollection", shipmentData.DateCollection);

			CombineAssertions("Checking all dates in DateCollection", delegate
			{
				shipmentData.DateCollection.AssertDateExists(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 3, 11));
				shipmentData.DateCollection.AssertDateExists(DateType.Received, ZBool.False, new ZDateTime(2011, 3, 12));
				shipmentData.DateCollection.AssertDateExists(DateType.Departure, ZBool.True, new ZDateTime(2011, 3, 13));
				shipmentData.DateCollection.AssertDateExists(DateType.Arrival, ZBool.True, new ZDateTime(2011, 3, 14));
				shipmentData.DateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, new ZDateTime(2011, 3, 15));
				shipmentData.DateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, new ZDateTime(2011, 3, 16));
				shipmentData.DateCollection.AssertDateExists(DateType.PickupReceiptRequested, ZBool.False, new ZDateTime(2016, 7, 20));
				shipmentData.DateCollection.AssertDateExists(DateType.DeliveryReceiptRequested, ZBool.False, new ZDateTime(2016, 7, 23));
				shipmentData.DateCollection.AssertDateExists(DateType.PickupDispatchRequested, ZBool.False, new ZDateTime(2016, 7, 24));
				shipmentData.DateCollection.AssertDateExists(DateType.DeliveryDispatchRequested, ZBool.False, new ZDateTime(2016, 7, 25));
				shipmentData.DateCollection.AssertDateExists(DateType.DeliveryDueDate, ZBool.False, new ZDateTime(2016, 7, 25));
				shipmentData.DateCollection.AssertDateExists(DateType.RevisedDeliveryDueDate, ZBool.False, new ZDateTime(2016, 7, 26));

				AssertEquals("Checking dates left, and found dates not expected.", 0, shipmentData.DateCollection.Count);
			});

			#endregion
		}

		public void TestCreationOfParentConsolParentIfShipmentHasConsol()
		{
			var consolBO = SetupConsolWithShipments();
			var shipmentBO = consolBO.GridShipments[0];
			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertEquals("dataObject.WayBillNumber", "M1234567", dataObject.WayBillNumber);
			AssertEquals("dataObject.DataContext.GetDataSources()", "ForwardingConsol [C00010000], ForwardingShipment [S00010001]", dataObject.DataContext.GetDataSources());

			AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);
			AssertEquals("dataObject.SubShipmentCollection[0].WayBillNumber", "H1234567", dataObject.SubShipmentCollection[0].WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection.Count", 2, dataObject.SubShipmentCollection[0].SubShipmentCollection.Count);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection[0].WayBillNumber", "SSSS1111", dataObject.SubShipmentCollection[0].SubShipmentCollection[0].WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection[1].WayBillNumber", "H7654321", dataObject.SubShipmentCollection[0].SubShipmentCollection[1].WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection.Count", 2, dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection.Count);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection[0].WayBillNumber", "SUBSUB11", dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection[0].WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection[1].WayBillNumber", "SUBSUB22", dataObject.SubShipmentCollection[0].SubShipmentCollection[1].SubShipmentCollection[1].WayBillNumber);
		}

		public void TestCreationOfParentShipmentIfShipmentHasParentShipment()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_HouseBill = "H1234567";

			var subShipmentBO = shipmentBO.CoLoadShipments.AddNew();
			subShipmentBO.JS_HouseBill = "H7654321";

			Factory.SaveForTesting();

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, subShipmentBO)), true, true);
			var dataObject = writer.GetDataObject(subShipmentBO);

			AssertEquals("dataObject.WayBillNumber", "H1234567", dataObject.WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);
			AssertEquals("dataObject.SubShipmentCollection[0].WayBillNumber", "H7654321", dataObject.SubShipmentCollection[0].WayBillNumber);
		}

		public void TestCreationOfParentShipmentExcludeHighVolumeLowValueMasterShipment()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_HouseBill = "H1234567";
			shipmentBO.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;

			var subShipmentBO = shipmentBO.CoLoadShipments.AddNew();
			subShipmentBO.JS_HouseBill = "H7654321";
			subShipmentBO.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			Factory.SaveForTesting();

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, subShipmentBO)), true, true);
			var dataObject = writer.GetDataObject(subShipmentBO);

			AssertEquals("dataObject.WayBillNumber", "H7654321", dataObject.WayBillNumber);
			AssertEquals("dataObject.SubShipmentCollection.Count", 0, dataObject.SubShipmentCollection.Count);
		}

		public void TestCreationOfParentConsolIfSubShipmentHasAShipmentParentWhichHasAConsol()
		{
			var consolBO = SetupConsolWithShipments();
			var shipmentBO = consolBO.GridShipments[0].CoLoadShipments[1];
			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);

			var consolDataObject = writer.GetDataObject(shipmentBO);
			AssertEquals("consolDataObject.WayBillNumber", "M1234567", consolDataObject.WayBillNumber);
			AssertEquals("consolDataObject.DataContext.GetDataSources()", "ForwardingConsol [C00010000], ForwardingShipment [S00010001-2]", consolDataObject.DataContext.GetDataSources());
			AssertEquals("consolDataObject.SubShipmentCollection.Count", 1, consolDataObject.SubShipmentCollection.Count);

			var shipmentDataObject = consolDataObject.SubShipmentCollection[0];
			AssertEquals("shipmentDataObject.WayBillNumber", "H1234567", shipmentDataObject.WayBillNumber);
			AssertEquals("shipmentDataObject.DataContext.GetDataSources()", "ForwardingShipment [S00010001]", shipmentDataObject.DataContext.GetDataSources());
			AssertEquals("shipmentDataObject.SubShipmentCollection.Count", 1, shipmentDataObject.SubShipmentCollection.Count);

			var subShipmentDataObject = shipmentDataObject.SubShipmentCollection[0];
			AssertEquals("subShipmentDataObject.WayBillNumber", "H7654321", subShipmentDataObject.WayBillNumber);
			AssertEquals("subShipmentDataObject.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2]", subShipmentDataObject.DataContext.GetDataSources());
			AssertEquals("subShipmentDataObject.SubShipmentCollection.Count", 2, subShipmentDataObject.SubShipmentCollection.Count);

			var subSubShipment1DataObject = subShipmentDataObject.SubShipmentCollection[0];
			AssertEquals("subSubShipment1DataObject.WayBillNumber", "SUBSUB11", subSubShipment1DataObject.WayBillNumber);
			AssertEquals("subSubShipment1DataObject.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-1]", subSubShipment1DataObject.DataContext.GetDataSources());

			var subSubShipment2DataObject = subShipmentDataObject.SubShipmentCollection[1];
			AssertEquals("subSubShipment2DataObject.WayBillNumber", "SUBSUB22", subSubShipment2DataObject.WayBillNumber);
			AssertEquals("subSubShipment2DataObject.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-2]", subSubShipment2DataObject.DataContext.GetDataSources());
		}

		public void TestDataSourceIsAppliedCorrectlyWhenExportingASubShipmentWithAConsolAtTopLevel()
		{
			var consolBO = SetupConsolWithShipments();
			var shipmentBO = consolBO.GridShipments[0].CoLoadShipments[1];
			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertEquals("dataObject.DataContext.GetDataSources()", "ForwardingConsol [C00010000], ForwardingShipment [S00010001-2]", dataObject.DataContext.GetDataSources());

			var subSubShipmentContext = dataObject.SubShipmentCollection[0].SubShipmentCollection[0].DataContext;

			AssertEquals("subSubShipmentContext.GetDataSources()", "ForwardingShipment [S00010001-2]", subSubShipmentContext.GetDataSources());
		}

		public void TestDeclarationAreExportedAsSubShipment()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Constants.TransportModes.Sea;
			consolBO.JK_UniqueConsignRef = "C00010000";
			consolBO.JK_MasterBillNum = "M1234567";

			var shipmentBO1 = consolBO.GridShipments.AddNew();
			shipmentBO1.JS_UniqueConsignRef = "S00010001";
			shipmentBO1.JS_HouseBill = "H1234567";
			var declarationBO1 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1.JE_DeclarationReference = "S00010001";
			declarationBO1.JE_JS = shipmentBO1.PK;
			var shipmentBO1Sub1 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub1.JS_UniqueConsignRef = "S00010001-1";
			shipmentBO1Sub1.JS_HouseBill = "SSSS1111";
			var shipmentBO1Sub2 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub2.JS_UniqueConsignRef = "S00010001-2";
			shipmentBO1Sub2.JS_HouseBill = "H7654321";
			var declarationBO1Sub2 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1Sub2.JE_DeclarationReference = "S00010001-2";
			declarationBO1Sub2.JE_JS = shipmentBO1Sub2.PK;
			var shipmentBO1Sub2Sub1 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub1.JS_UniqueConsignRef = "S00010001-2-1";
			shipmentBO1Sub2Sub1.JS_HouseBill = "SUBSUB11";
			var shipmentBO1Sub2Sub2 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub2.JS_UniqueConsignRef = "S00010001-2-2";
			shipmentBO1Sub2Sub2.JS_HouseBill = "SUBSUB22";
			var declarationBO1Sub2Sub2 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1Sub2Sub2.JE_DeclarationReference = "S00010001-2-2";
			declarationBO1Sub2Sub2.JE_JS = shipmentBO1Sub2Sub2.PK;
			Factory.SaveForTesting();

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO1)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO1);
			AssertEquals("dataObject.DataContext.GetDataSources()", "ForwardingConsol [C00010000], ForwardingShipment [S00010001], CustomsDeclaration [S00010001]", dataObject.DataContext.GetDataSources());
			AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);

			var shipment = dataObject.SubShipmentCollection[0];
			AssertEquals("shipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001], CustomsDeclaration [S00010001]", shipment.DataContext.GetDataSources());
			AssertEquals("shipment.SubShipmentCollection.Count", 2, shipment.SubShipmentCollection.Count);

			var subShip1 = shipment.SubShipmentCollection[0];
			AssertEquals("subShip1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-1]", subShip1.DataContext.GetDataSources());
			AssertNull("subShip1.SubShipmentCollection", subShip1.SubShipmentCollection);

			var subShip2 = shipment.SubShipmentCollection[1];
			AssertEquals("subShip2.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2], CustomsDeclaration [S00010001-2]", subShip2.DataContext.GetDataSources());
			AssertEquals("subShip2.SubShipmentCollection.Count", 2, subShip2.SubShipmentCollection.Count);

			var subShip2_1 = subShip2.SubShipmentCollection[0];
			AssertEquals("subShip2_1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-1]", subShip2_1.DataContext.GetDataSources());
			AssertNull("subShip2_1.SubShipmentCollection", subShip2_1.SubShipmentCollection);

			var subShip2_2 = subShip2.SubShipmentCollection[1];
			AssertEquals("subShip2_2.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-2], CustomsDeclaration [S00010001-2-2]", subShip2_2.DataContext.GetDataSources());
			AssertNull("subShip2_2.SubShipmentCollection", subShip2_2.SubShipmentCollection);
		}

		public void TestDeclarationAreNotExportedAsSubShipmentForPCARecipientRole()
		{
			DeclarationAreNotExportedAsSubShipmentForSpecificRecipientRoleTestCore(RecipientRoleType.PCA);
		}

		public void TestDeclarationAreNotExportedAsSubShipmentForDCARecipientRole()
		{
			DeclarationAreNotExportedAsSubShipmentForSpecificRecipientRoleTestCore(RecipientRoleType.DCA);
		}

		void DeclarationAreNotExportedAsSubShipmentForSpecificRecipientRoleTestCore(RecipientRoleType recipientRoleType)
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Constants.TransportModes.Sea;
			consolBO.JK_UniqueConsignRef = "C00010000";
			consolBO.JK_MasterBillNum = "M1234567";

			var shipmentBO1 = consolBO.GridShipments.AddNew();
			shipmentBO1.JS_UniqueConsignRef = "S00010001";
			shipmentBO1.JS_HouseBill = "H1234567";
			var declarationBO1 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1.JE_DeclarationReference = "S00010001";
			declarationBO1.JE_JS = shipmentBO1.PK;
			var shipmentBO1Sub1 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub1.JS_UniqueConsignRef = "S00010001-1";
			shipmentBO1Sub1.JS_HouseBill = "SSSS1111";
			var shipmentBO1Sub2 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub2.JS_UniqueConsignRef = "S00010001-2";
			shipmentBO1Sub2.JS_HouseBill = "H7654321";
			var declarationBO1Sub2 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1Sub2.JE_DeclarationReference = "S00010001-2";
			declarationBO1Sub2.JE_JS = shipmentBO1Sub2.PK;
			var shipmentBO1Sub2Sub1 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub1.JS_UniqueConsignRef = "S00010001-2-1";
			shipmentBO1Sub2Sub1.JS_HouseBill = "SUBSUB11";
			var shipmentBO1Sub2Sub2 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub2.JS_UniqueConsignRef = "S00010001-2-2";
			shipmentBO1Sub2Sub2.JS_HouseBill = "SUBSUB22";
			var declarationBO1Sub2Sub2 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1Sub2Sub2.JE_DeclarationReference = "S00010001-2-2";
			declarationBO1Sub2Sub2.JE_JS = shipmentBO1Sub2Sub2.PK;
			Factory.SaveForTesting();

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, shipmentBO1)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO1);
			AssertEquals("dataObject.DataContext.GetDataSources()", "ForwardingConsol [C00010000], ForwardingShipment [S00010001]", dataObject.DataContext.GetDataSources());
			AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);

			var shipment = dataObject.SubShipmentCollection[0];
			AssertEquals("shipment.DataContext.GetDataSources()", "ForwardingShipment [S00010001]", shipment.DataContext.GetDataSources());
			AssertEquals("shipment.SubShipmentCollection.Count", 2, shipment.SubShipmentCollection.Count);

			var subShip1 = shipment.SubShipmentCollection[0];
			AssertEquals("subShip1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-1]", subShip1.DataContext.GetDataSources());
			AssertNull("subShip1.SubShipmentCollection", subShip1.SubShipmentCollection);

			var subShip2 = shipment.SubShipmentCollection[1];
			AssertEquals("subShip2.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2]", subShip2.DataContext.GetDataSources());
			AssertEquals("subShip2.SubShipmentCollection.Count", 2, subShip2.SubShipmentCollection.Count);

			var subShip2SubShip1 = subShip2.SubShipmentCollection[0];
			AssertEquals("subShip1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-1]", subShip2SubShip1.DataContext.GetDataSources());
			AssertNull("subShip1.SubShipmentCollection", subShip2SubShip1.SubShipmentCollection);

			var subShip2SubShip2 = subShip2.SubShipmentCollection[1];
			AssertEquals("subShip1.DataContext.GetDataSources()", "ForwardingShipment [S00010001-2-2]", subShip2SubShip2.DataContext.GetDataSources());
			AssertNull("subShip1.SubShipmentCollection", subShip2SubShip2.SubShipmentCollection);
		}

		public void TestExportTransportBooking_Pickup()
		{
			SystemDataRegistry.Instance.IncludeTransportBookingInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipmentBO = Factory.New<ForwardingShipment>();
			var dtbConsolidation = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation.KB_ParentID = shipmentBO.PK;
			dtbConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = Factory.BOFactory.New<IDtbBooking>();
			booking.KM_KB_Booking = dtbConsolidation.PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertEquals(1, dataObject.PreCarriageShipmentCollection.Count);
			AssertNull(dataObject.PostCarriageShipmentCollection);
		}

		public void TestExportTransportBooking_Delivery()
		{
			SystemDataRegistry.Instance.IncludeTransportBookingInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipmentBO = Factory.New<ForwardingShipment>();
			var dtbConsolidation = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation.KB_ParentID = shipmentBO.PK;
			dtbConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			var booking = Factory.BOFactory.New<IDtbBooking>();
			booking.KM_KB_Booking = dtbConsolidation.PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertNull(dataObject.PreCarriageShipmentCollection);
			AssertEquals(1, dataObject.PostCarriageShipmentCollection.Count);
		}

		public void TestExportTransportBooking_DisableRegistry()
		{
			SystemDataRegistry.Instance.IncludeTransportBookingInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipmentBO = Factory.New<ForwardingShipment>();
			var dtbConsolidation1 = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation1.KB_ParentID = shipmentBO.PK;
			dtbConsolidation1.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking1 = Factory.BOFactory.New<IDtbBooking>();
			booking1.KM_KB_Booking = dtbConsolidation1.PK;

			var dtbConsolidation2 = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation2.KB_ParentID = shipmentBO.PK;
			dtbConsolidation2.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation2.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			var booking2 = Factory.BOFactory.New<IDtbBooking>();
			booking2.KM_KB_Booking = dtbConsolidation2.PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertNull(dataObject.PreCarriageShipmentCollection);
			AssertNull(dataObject.PostCarriageShipmentCollection);
		}

		public void TestExportTransportBooking_DisableRegistryWithAlwaysExportTBIsTrue()
		{
			SystemDataRegistry.Instance.IncludeTransportBookingInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipmentBO = Factory.New<ForwardingShipment>();
			var dtbConsolidation1 = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation1.KB_ParentID = shipmentBO.PK;
			dtbConsolidation1.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking1 = Factory.BOFactory.New<IDtbBooking>();
			booking1.KM_KB_Booking = dtbConsolidation1.PK;

			var dtbConsolidation2 = Factory.BOFactory.New<IDtbBookingConsolidation>();
			dtbConsolidation2.KB_ParentID = shipmentBO.PK;
			dtbConsolidation2.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			dtbConsolidation2.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			var booking2 = Factory.BOFactory.New<IDtbBooking>();
			booking2.KM_KB_Booking = dtbConsolidation2.PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true, alwaysExportTB: true);
			var dataObject = writer.GetDataObject(shipmentBO);

			AssertEquals(1, dataObject.PreCarriageShipmentCollection.Count);
			AssertEquals(1, dataObject.PostCarriageShipmentCollection.Count);
		}

		public void TestExportingOfInBondData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var inBondHeader = Factory.BOFactory.New<US.InBond.ICusInBondHeader>();
				inBondHeader.BH_ParentID = shipment.PK;
				inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
				var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);
				AssertNotNull("shipmentData.SubShipmentCollection", shipmentData.SubShipmentCollection);
				AssertNotNull("shipmentData.SubShipmentCollection should contain InBond", shipmentData.SubShipmentCollection.FirstOrDefault(x => x.GetMatchingDataSource(DataContextType.InBond) != null));
			}
		}

		public void TestAviationSecurityInspectionTypeFieldMappings()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";
			shipmentBO.JS_InspectionTypeCode = "XRY";

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNull("Aviation Security Inspection Type is not added for Sea shipments", shipmentData.AviationSecurityInspectionType);

			shipmentBO.JS_TransportMode = "AIR";
			shipmentBO.JS_InspectionTypeCode = "XRY";
			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("Aviation Security Inspection Type is added for Air shipments", "XRY", shipmentData.AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray", shipmentData.AviationSecurityInspectionType.Description);

			shipmentBO.JS_InspectionTypeCode = "UNK";
			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNull("Aviation Security Inspection Type is not added for default 'UNK' code", shipmentData.AviationSecurityInspectionType);
		}

		public void TestAviationSecurityAdditionalInspectionTypeFieldMappings()
		{
			// Arrange
			var shipmentBO = Factory.New<ForwardingShipment>();

			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";
			shipmentBO.JS_IsHighRisk = true;
			shipmentBO.JS_AdditionalInspectionTypeCode = "XRY";

			// Act & Assert
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNull("Aviation Security Additional Inspection Type is not added for Sea shipments", shipmentData.AviationSecurityAdditionalInspectionType);

			shipmentBO.JS_TransportMode = "AIR";
			shipmentBO.JS_AdditionalInspectionTypeCode = "XRY";
			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("Aviation Security Additional Inspection Type is added for Air shipments", "XRY", shipmentData.AviationSecurityAdditionalInspectionType.Code);
			AssertEquals("X-Ray equipment", shipmentData.AviationSecurityAdditionalInspectionType.Description);

			shipmentBO.JS_AdditionalInspectionTypeCode = "UNK";
			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertNull("Aviation Security Inspection Type is not added for default 'UNK' code", shipmentData.AviationSecurityAdditionalInspectionType);
		}

		public void TestHVLShipment()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var header = Factory.BOFactory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipmentBO.PK)) as BusinessObject;

			var consignment1 = (BusinessObject)Factory.BOFactory.New<IHVLVConsignment>();
			consignment1.FillWithValidTestData();
			consignment1[HVLVConsignmentSchema.HVC_GoodsDescription] = "CONSIGN1";
			consignment1[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;

			var item1 = (BusinessObject)Factory.BOFactory.New<IHVLVItem>();
			item1[HVLVItemSchema.HVI_HVC_Consignment] = consignment1.PK;
			item1[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipmentBO.PK;

			var cusEntryNum1 = Factory.BOFactory.New<ICusEntryNumber>();
			cusEntryNum1.Parent = consignment1;
			cusEntryNum1.CE_EntryNum = "1234";
			cusEntryNum1.CE_Category = "OTH";

			var consignment2 = (BusinessObject)Factory.BOFactory.New<IHVLVConsignment>();
			consignment2.FillWithValidTestData();
			consignment2[HVLVConsignmentSchema.HVC_GoodsDescription] = "CONSIGN2";
			consignment2[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;

			var item2 = (BusinessObject)Factory.BOFactory.New<IHVLVItem>();
			item2[HVLVItemSchema.HVI_HVC_Consignment] = consignment2.PK;
			item2[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipmentBO.PK;

			Factory.SaveForTesting();

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO));

			UniversalShipment shipmentData;
			using (writingManager.SetIsPublishingInternally())
			{
				shipmentData = new ShipmentDataObjectWriter(writingManager, true, false).GetDataObject(shipmentBO);
			}

			AssertEquals(Constants.ShipmentTypes.HighVolumeLowValue, shipmentData.ShipmentType.Code);
			AssertEquals(2, shipmentData.SubShipmentCollection.Count);

			var subShipment1 = shipmentData.SubShipmentCollection[0];
			AssertEquals("CONSIGN1", subShipment1.GoodsDescription);
			AssertNull(subShipment1.AdditionalReferenceCollection);

			var subShipment2 = shipmentData.SubShipmentCollection[1];
			AssertEquals("CONSIGN2", subShipment2.GoodsDescription);

			using (writingManager.SetIsPublishingInternally())
			{
				shipmentData = new ShipmentDataObjectWriter(writingManager, true, false, includeAdditionalReferenceCollectionForSubshipments: true).GetDataObject(shipmentBO);
			}

			subShipment1 = shipmentData.SubShipmentCollection[0];
			AssertEquals("CONSIGN1", subShipment1.GoodsDescription);
			AssertNotNull("AdditionalReferenceCollection exists", subShipment1.AdditionalReferenceCollection);
			AssertEquals("1234", subShipment1.AdditionalReferenceCollection[0].ReferenceNumber);
		}

		public void TestHVLShipment_PopulateCommercialInfo()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var header = Factory.BOFactory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipmentBO.PK)) as BusinessObject;

			var consignment1 = (BusinessObject)Factory.BOFactory.New<IHVLVConsignment>();
			consignment1.FillWithValidTestData();
			consignment1[HVLVConsignmentSchema.HVC_WaybillNumber] = "Waybill1";
			consignment1[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;

			var consignment2 = (BusinessObject)Factory.BOFactory.New<IHVLVConsignment>();
			consignment2.FillWithValidTestData();
			consignment2[HVLVConsignmentSchema.HVC_WaybillNumber] = "Waybill2";
			consignment2[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRI, shipmentBO));
			writingManager.FilteredDataContextType = DataContextType.CustomsDeclaration;

			UniversalShipment shipmentData;
			using (writingManager.SetIsPublishingInternally())
			{
				shipmentData = new ShipmentDataObjectWriter(writingManager, true, false).GetDataObject(shipmentBO);
			}

			AssertEquals(Constants.ShipmentTypes.HighVolumeLowValue, shipmentData.ShipmentType.Code);
			AssertNull(shipmentData.SubShipmentCollection);
			AssertEquals(2, shipmentData.CommercialInfo.CommercialInvoiceCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Waybill1", "Waybill2" }, shipmentData.CommercialInfo.CommercialInvoiceCollection.Select(invoiceHeader => invoiceHeader.InvoiceNumber.ToString()));
		}

		public void TestCTStatus()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_CommunityTransitStatus = "C";

			Factory.SaveForTesting();

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals("C", shipmentData.CommunityTransitStatus.Code);
			AssertEquals("Intra-Community movement of Community status goods consigned from one EU airport to another EU destination", shipmentData.CommunityTransitStatus.Description);
		}

		[TestDate]
		public void TestCTStatus_WhenCTHasMoreThan3Characters()
		{
			TestDateAttribute.Date = DateTime.Today;
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000000";
			shipment.JS_CommunityTransitStatus = Enterprise.Customs.Common.EU.ImportCommunityTransitStatusList.Codes.T2LSM;
			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)));

			var universalShipment = (UniversalShipment)writer.GetDataObject(shipment);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalShipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertXMLEquals(ShipmentXMLWithLongCommunityStatusCode(ZDateTime.Today), result);
					}
				}
			}
		}

		#region ShipmentXMLWithLongCommunityStatusCode
		string ShipmentXMLWithLongCommunityStatusCode(ZDateTime houseBillIssueDate) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00000000</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <ActualChargeable>0</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <CommunityTransitStatus>
      <Code>T2LSM</Code>
      <Description>Goods destined for San Marino -- Article 2 of Decision 4/92 of the EEC-San Marin</Description>
    </CommunityTransitStatus>
    <CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LCL</Code>
      <Description>Less Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>0</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>0</DocumentedWeight>
    <EventBranchHomePort>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </EventBranchHomePort>
    <FMCTariffID></FMCTariffID>
    <FreightRate>0</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <GreenhouseGasEmission>
      <CO2e>0</CO2e>
      <CO2eDescriptiveStatus>
        <Code>NON</Code>
        <Description>Not Calculated</Description>
      </CO2eDescriptiveStatus>
      <CO2eUnit>
        <Code>KG</Code>
        <Description>Kilograms</Description>
      </CO2eUnit>
    </GreenhouseGasEmission>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <HouseBillOfLadingType>
      <Code></Code>
    </HouseBillOfLadingType>
    <InsuranceValue>0</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHighRisk>false</IsHighRisk>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <ManifestedChargeable>0</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>0</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code></Code>
    </PortOfDestination>
    <PortOfOrigin>
      <Code></Code>
    </PortOfOrigin>
    <RateCommodity>
      <Code></Code>
    </RateCommodity>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code></Code>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>RevisedDeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>{houseBillIssueDate.ToString("yyyy-MM-ddTHH:mm:ss")}</Value>
      </Date>
      <Date>
        <Type>PickupReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <PackingLineCollection Content=""Complete"">
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion

		public void TestConsolidatedCargoStatus()
		{
			var consolAir = Factory.New<ForwardingConsol>();
			consolAir.JK_RL_NKDischargePort = "AUSYD";
			var shipmentAir = consolAir.Shipments.AddNew();
			shipmentAir.JS_TransportMode = Constants.TransportModes.Air;
			AssertNull(shipmentAir.AUCusHAWB);
			var auMawb = Factory.BOFactory.New<AU.ICusMAWB>();
			var auHawb = Factory.BOFactory.New<AU.ICusHAWB>();
			auHawb.CS_CM = auMawb.PK;
			auHawb.CS_JS = shipmentAir.PK;
			auHawb.CS_CustomsStatus = Enterprise.Customs.Common.AU.CMR.CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(auHawb, shipmentAir.AUCusHAWB);
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentAir)), true, false).GetDataObject(shipmentAir);
			AssertEquals(Enterprise.Customs.Common.AU.CMR.CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, shipmentData.ConsolidatedCargoStatus.Code);
			AssertEquals(Enterprise.Customs.Common.AU.CMR.CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, shipmentData.ConsolidatedCargoStatus.Description);

			var consolSea = Factory.New<ForwardingConsol>();
			consolSea.JK_RL_NKDischargePort = "AUSYD";
			var shipmentSea = consolSea.Shipments.AddNew();
			shipmentSea.JS_TransportMode = Constants.TransportModes.Sea;
			AssertNull(shipmentSea.AUCusSCAHouse);
			var auOceanBill = Factory.BOFactory.New<AU.ICusSCAOceanBill>();
			var auScaHouseBill = Factory.BOFactory.New<AU.ICusSCAHouse>();
			auScaHouseBill.CA_CB = auOceanBill.PK;
			auScaHouseBill.CA_JS = shipmentSea.PK;
			auScaHouseBill.CA_ShipmentStatus = Enterprise.Customs.Common.AU.CMR.CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(auScaHouseBill, shipmentSea.AUCusSCAHouse);
			var shipmentData2 = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentSea)), true, false).GetDataObject(shipmentSea);
			AssertEquals(Enterprise.Customs.Common.AU.CMR.CMRBaseStatuses.Codes.OriginalAccepted, shipmentData2.ConsolidatedCargoStatus.Code);
			AssertEquals(Enterprise.Customs.Common.AU.CMR.CMRBaseStatuses.Descriptions.OriginalAccepted, shipmentData2.ConsolidatedCargoStatus.Description);
		}

		#region TestPopulateCarriageShipmentCollection

		public void TestCreatingDiffrentCarriageShipmentForDifferentTransportCo_Pickup()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var transportCoAddress2 = Factory.NewWithValidTestData<OrgAddress>();

			var consol = shipmentBO.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			var confirm1 = container1.Confirms.AddNew();
			confirm1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm1.EU_OA_TransportProvider = transportCoAddress1.PK;

			var container2 = consol.Containers.AddNew();
			var confirm2 = container2.Confirms.AddNew();
			confirm2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm2.EU_OA_TransportProvider = transportCoAddress1.PK;

			var container3 = consol.Containers.AddNew();
			var confirm3 = container3.Confirms.AddNew();
			confirm3.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm3.EU_OA_TransportProvider = transportCoAddress2.PK;

			shipmentBO.OuterPackLines.AddNew().SetContainer(container1.PK);
			shipmentBO.OuterPackLines.AddNew().SetContainer(container2.PK);
			shipmentBO.OuterPackLines.AddNew().SetContainer(container3.PK);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(2, shipmentData.PreCarriageShipmentCollection.Count);
		}

		public void TestCreatingDiffrentCarriageShipmentForDifferentTransportCo_Delivery()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var transportCoAddress2 = Factory.NewWithValidTestData<OrgAddress>();

			var consol = shipmentBO.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			var confirm1 = container1.Confirms.AddNew();
			confirm1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			confirm1.EU_OA_TransportProvider = transportCoAddress1.PK;

			var container2 = consol.Containers.AddNew();
			var confirm2 = container2.Confirms.AddNew();
			confirm2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			confirm2.EU_OA_TransportProvider = transportCoAddress1.PK;

			var container3 = consol.Containers.AddNew();
			var confirm3 = container3.Confirms.AddNew();
			confirm3.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			confirm3.EU_OA_TransportProvider = transportCoAddress2.PK;

			shipmentBO.OuterPackLines.AddNew().SetContainer(container1.PK);
			shipmentBO.OuterPackLines.AddNew().SetContainer(container2.PK);
			shipmentBO.OuterPackLines.AddNew().SetContainer(container3.PK);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(2, shipmentData.PostCarriageShipmentCollection.Count);
		}

		public void TestPopulateCarriageShipmentInstruction_Pickup()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipmentBO.Consols.AddNew();

			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var now = ZDateTime.Now;

			var confirm = container.Confirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm.EU_OA_TransportProvider = transportCoAddress.PK;
			confirm.ConfirmAddress.OrganisationPK = header.PK;
			confirm.ConfirmAddressOverride = true;
			confirm.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm.EU_JC = container.PK;
			confirm.EU_VehicleRegistration = "AAA111";
			confirm.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			confirm.EU_PlannedPickupDeliveryTime = now;
			confirm.EU_RequestedPickupDeliveryTime = now.AddDays(1);
			confirm.EU_PickupDeliveryTime = now.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();
			var link = instruction.InstructionContainerLinkCollection.First();
			var confirmationData = link.ConfirmationCollection.First();
			var transportProviderData = preCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, instruction.DropMode.Code);
			AssertEquals(FCLEquipmentNeededList.Descriptions.SideLoader, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals("PIC", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}

		public void TestPopulateCarriageShipmentInstruction_Delivery()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipmentBO.Consols.AddNew();

			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var now = ZDateTime.Now;

			var confirm = container.Confirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.FCLEquipmentNeeded.SideLoader, now);
			confirm.EU_JC = container.PK;
			confirm.ConfirmAddress.OrganisationPK = header.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();
			var link = instruction.InstructionContainerLinkCollection.First();
			var confirmationData = link.ConfirmationCollection.First();
			var transportProviderData = postCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, instruction.DropMode.Code);
			AssertEquals(FCLEquipmentNeededList.Descriptions.SideLoader, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals("DLV", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}

		public void TestPopulateCarriageShipmentInstruction_LinkCreation()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress1 = Factory.NewWithValidTestData<OrgAddress>();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipmentBO.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			var confirm1 = container1.Confirms.AddNew();
			confirm1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm1.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm1.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm1.EU_JC = container1.PK;
			confirm1.ConfirmAddress.OrganisationPK = header.PK;
			confirm1.ConfirmAddressOverride = false;
			confirm1.EU_VehicleRegistration = "AAA111";
			confirm1.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container1.PK);

			var container2 = consol.Containers.AddNew();
			var confirm2 = container2.Confirms.AddNew();
			confirm2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm2.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm2.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm2.EU_JC = container2.PK;
			confirm2.ConfirmAddress.OrganisationPK = header.PK;
			confirm2.ConfirmAddressOverride = false;
			confirm2.EU_VehicleRegistration = "AAA111";
			confirm2.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container2.PK);

			var container3 = consol.Containers.AddNew();
			var confirm3 = container3.Confirms.AddNew();
			confirm3.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm3.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm3.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm3.EU_JC = container3.PK;
			confirm3.ConfirmAddress.OrganisationPK = header.PK;
			confirm3.ConfirmAddressOverride = false;
			confirm3.EU_VehicleRegistration = "Diff";
			confirm3.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container3.PK);

			var container4 = consol.Containers.AddNew();
			var confirm4 = container4.Confirms.AddNew();
			confirm4.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm4.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm4.EU_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			confirm4.EU_JC = container4.PK;
			confirm4.ConfirmAddress.OrganisationPK = header.PK;
			confirm4.ConfirmAddressOverride = false;
			confirm4.EU_VehicleRegistration = "AAA111";
			confirm4.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container4.PK);

			var container5 = consol.Containers.AddNew();
			var confirm5 = container5.Confirms.AddNew();
			confirm5.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm5.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm5.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm5.ConfirmAddressOverride = true;
			confirm5.EU_JC = container5.PK;
			confirm5.ConfirmAddress.OrganisationPK = header.PK;
			confirm5.EU_VehicleRegistration = "AAA111";
			confirm5.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container5.PK);

			var container6 = consol.Containers.AddNew();
			var confirm6 = container6.Confirms.AddNew();
			confirm6.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm6.EU_OA_TransportProvider = transportCoAddress1.PK;
			confirm6.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm6.EU_JC = container6.PK;
			confirm6.ConfirmAddress.OrganisationPK = header.PK;
			confirm6.ConfirmAddressOverride = false;
			confirm6.EU_VehicleRegistration = "AAA111";
			confirm6.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			shipmentBO.OuterPackLines.AddNew().SetContainer(container6.PK);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(1, shipmentData.PreCarriageShipmentCollection.Count);
			AssertEquals(6, shipmentData.PreCarriageShipmentCollection.First().InstructionCollection.Count);
		}

		public void TestCreatingDiffrentCarriageShipmentForDifferentTransportCo_LCL_Pickup()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentBO.OuterPackLines.AddNew();

			var transportCoAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var transportCoAddress2 = Factory.NewWithValidTestData<OrgAddress>();

			var confirm1 = shipmentBO.PickupConfirms.AddNew();
			confirm1.EU_OA_TransportProvider = transportCoAddress1.PK;

			var confirm2 = shipmentBO.PickupConfirms.AddNew();
			confirm2.EU_OA_TransportProvider = transportCoAddress1.PK;

			var confirm3 = shipmentBO.PickupConfirms.AddNew();
			confirm3.EU_OA_TransportProvider = transportCoAddress2.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(2, shipmentData.PreCarriageShipmentCollection.Count);
			AssertEquals(2, shipmentData.PreCarriageShipmentCollection.First().InstructionCollection.Count);
			AssertEquals(1, shipmentData.PreCarriageShipmentCollection.ElementAt(1).InstructionCollection.Count);
		}

		public void TestGreenhouseGasEmission()
		{
			var shipmentBO = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			shipmentBO.SetTotalCO2e(1m);
			shipmentBO.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertNotNull("shipmentData.GreenhouseGasEmission", shipmentData.GreenhouseGasEmission);
			AssertEquals(1m, shipmentData.GreenhouseGasEmission.CO2e);
			CombineAssertions("GreenhouseGasEmission.CO2eUnit", () =>
			{
				AssertEquals("Code", "KG", shipmentData.GreenhouseGasEmission.CO2eUnit.Code);
				AssertEquals("Description", "Kilograms", shipmentData.GreenhouseGasEmission.CO2eUnit.Description);
			});
			AssertNull("Should not write into GreenhouseGasEmission.CO2eStatus", shipmentData.GreenhouseGasEmission.CO2eStatus);
			CombineAssertions("GreenhouseGasEmission.CO2eDescriptiveStatus", () =>
			{
				AssertEquals("Code", CO2eStatusList.Codes.Current, shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Code);
				AssertEquals("Description", CO2eHelper.GetCO2eStatusShortDescription(CO2eStatusList.Codes.Current), shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Description);
			});
		}

		public void TestShipmentAdditionalAddressInfo()
		{
			var factory = Factory;
			var shipmentBO = factory.New<ForwardingShipment>();

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.JS_OA_ExportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.JS_OA_ImportReleaseDepot = Factory.NewWithValidTestData<OrgAddress>().PK;

			shipmentBO.PickupByTransportMode = "ROA";
			shipmentBO.DeliveryByTransportMode = "RAI";
			shipmentBO.CFSDepartureByTransportMode = "IWT";
			shipmentBO.CFSArrivalByTransportMode = "ROA";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull("shipmentData.AdditionalAddressInfoCollection is not null", shipmentData.AdditionalAddressInfoCollection);
			AssertEquals("shipmentData.AdditionalAddressInfoCollection.Count", 4, shipmentData.AdditionalAddressInfoCollection.Count);

			var additionalAddressInfo1 = shipmentData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.ConsignorPickupDeliveryAddress));
			AssertNotNull("additionalAddressInfo1 is not null", additionalAddressInfo1);
			AssertEquals("additionalAddressInfo1.TransportMode.Code", "ROA", additionalAddressInfo1.TransportMode.Code);

			var additionalAddressInfo2 = shipmentData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.ConsigneePickupDeliveryAddress));
			AssertNotNull("additionalAddressInfo2 is not null", additionalAddressInfo2);
			AssertEquals("additionalAddressInfo2.TransportMode.Code", "RAI", additionalAddressInfo2.TransportMode.Code);

			var additionalAddressInfo3 = shipmentData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.DepartureCFSAddress));
			AssertNotNull("additionalAddressInfo3 is not null", additionalAddressInfo3);
			AssertEquals("additionalAddressInfo3.TransportMode.Code", "IWT", additionalAddressInfo3.TransportMode.Code);

			var additionalAddressInfo4 = shipmentData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.ArrivalCFSAddress));
			AssertNotNull("additionalAddressInfo4 is not null", additionalAddressInfo4);
			AssertEquals("additionalAddressInfo4.TransportMode.Code", "ROA", additionalAddressInfo4.TransportMode.Code);
		}

		public void TestCreatingDiffrentCarriageShipmentForDifferentTransportCo_LCL_Delivery()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentBO.OuterPackLines.AddNew();

			var transportCoAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var transportCoAddress2 = Factory.NewWithValidTestData<OrgAddress>();

			var confirm1 = shipmentBO.DeliveryConfirms.AddNew();
			confirm1.EU_OA_TransportProvider = transportCoAddress1.PK;

			var confirm2 = shipmentBO.DeliveryConfirms.AddNew();
			confirm2.EU_OA_TransportProvider = transportCoAddress1.PK;

			var confirm3 = shipmentBO.DeliveryConfirms.AddNew();
			confirm3.EU_OA_TransportProvider = transportCoAddress2.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			AssertEquals(2, shipmentData.PostCarriageShipmentCollection.Count);
			AssertEquals(2, shipmentData.PostCarriageShipmentCollection.First().InstructionCollection.Count);
			AssertEquals(1, shipmentData.PostCarriageShipmentCollection.ElementAt(1).InstructionCollection.Count);
		}

		public void TestPopulatePickupInstruction_LCL()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var now = ZDateTime.Now;

			var confirm = shipmentBO.PickupConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.OriginPickup, Constants.LCLAIREquipmentNeeded.HandHaulier, now);
			confirm.ConfirmAddress.OrganisationPK = header.PK;

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();
			var transportProviderData = preCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));
			var link = instruction.InstructionPackingLineLinkCollection.First();

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, instruction.DropMode.Code);
			AssertEquals(LCLAIREquipmentNeededList.Descriptions.HandHaulier, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals(1, link.PackingLineLink);
			AssertEquals(10, link.Quantity);

			var confirmationData = link.ConfirmationCollection.First();

			AssertEquals("PIC", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}

		public void TestPopulateCarriageShipmentInstruction_PickupDoesNotCreateConfirmIfNotExist()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipmentBO.Consols.AddNew();

			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var now = ZDateTime.Now;

			var confirm = container.Confirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			confirm.EU_OA_TransportProvider = transportCoAddress.PK;
			confirm.ConfirmAddress.OrganisationPK = header.PK;
			confirm.ConfirmAddressOverride = true;
			confirm.EU_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			confirm.EU_JC = container.PK;
			confirm.EU_VehicleRegistration = "AAA111";
			confirm.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			confirm.EU_PlannedPickupDeliveryTime = now;
			confirm.EU_RequestedPickupDeliveryTime = now.AddDays(1);
			confirm.EU_PickupDeliveryTime = now.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();
			var link = instruction.InstructionContainerLinkCollection.First();
			var confirmationData = link.ConfirmationCollection.First();
			var transportProviderData = preCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, instruction.DropMode.Code);
			AssertEquals(FCLEquipmentNeededList.Descriptions.SideLoader, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals("PIC", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);

			confirm.Delete();
			Factory.SaveForTesting();

			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var confirms = container.Confirms.Where(x => x.EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup);
			AssertEquals(0, confirms.Count());
		}

		public void TestPopulateCarriageShipmentInstruction_DeliveryDoesNotCreateConfirmIfNotExist()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipmentBO.Consols.AddNew();

			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var now = ZDateTime.Now;

			var confirm = container.Confirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.FCLEquipmentNeeded.SideLoader, now);
			confirm.EU_JC = container.PK;
			confirm.ConfirmAddress.OrganisationPK = header.PK;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();
			var link = instruction.InstructionContainerLinkCollection.First();
			var confirmationData = link.ConfirmationCollection.First();
			var transportProviderData = postCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, instruction.DropMode.Code);
			AssertEquals(FCLEquipmentNeededList.Descriptions.SideLoader, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals("DLV", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);

			confirm.Delete();
			Factory.SaveForTesting();

			shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var confirms = container.Confirms.Where(x => x.EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			AssertEquals(0, confirms.Count());
		}

		public void TestPopulateDeliveryInstruction_LCL()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var now = ZDateTime.Now;

			var confirm = shipmentBO.DeliveryConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.LCLAIREquipmentNeeded.HandHaulier, now);
			confirm.ConfirmAddress.OrganisationPK = header.PK;

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();
			var transportProviderData = postCarriageShipmentData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.ToString() == nameof(DocAddressType.TransportCompanyDocumentaryAddress));
			var link = instruction.InstructionPackingLineLinkCollection.First();

			AssertEquals(transportCoAddress.Header.OH_Code, transportProviderData.OrganizationCode);
			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, instruction.DropMode.Code);
			AssertEquals(LCLAIREquipmentNeededList.Descriptions.HandHaulier, instruction.DropMode.Description);
			AssertEquals("AAA111", instruction.Equipment);
			AssertEquals("pick up delivery instruction", instruction.ServiceInstruction);
			AssertNotNull(instruction.Address);

			AssertEquals(1, link.PackingLineLink);
			AssertEquals(10, link.Quantity);

			var confirmationData = link.ConfirmationCollection.First();

			AssertEquals("DLV", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}

		public void TestPacklineLinkAllocation()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;

			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 20;

			var packline2 = shipmentBO.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 10;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var now = ZDateTime.Now;

			var confirm = shipmentBO.PickupConfirms.AddNew();

			confirm.EU_OA_TransportProvider = transportCoAddress.PK;
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);

			AssertEquals(1, shipmentData.PackingLineCollection.First().Link);
			AssertEquals(2, shipmentData.PackingLineCollection.ElementAt(1).Link);
			AssertEquals(2, shipmentData.PreCarriageShipmentCollection.First().InstructionCollection.First().InstructionPackingLineLinkCollection.Count);
			AssertEquals(1, shipmentData.PreCarriageShipmentCollection.First().InstructionCollection.First().InstructionPackingLineLinkCollection.First().PackingLineLink);
			AssertEquals(2, shipmentData.PreCarriageShipmentCollection.First().InstructionCollection.First().InstructionPackingLineLinkCollection.ElementAt(1).PackingLineLink);
		}

		#endregion

		#region TestFindParentConsol

		public void TestFindParentConsol_Pic()
		{
			AssertFindParentConsol(RecipientRoleType.PCA);
		}

		public void TestFindParentConsol_Dlv()
		{
			AssertFindParentConsol(RecipientRoleType.DCA);
		}

		void AssertFindParentConsol(RecipientRoleType roleType)
		{
			var now = ZDateTime.Now;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA"; // Sea Freight
			shipment.JS_PackingMode = "FCL";

			var consolPic = shipment.Consols.AddNew();
			consolPic.JK_TransportMode = "SEA";
			consolPic.JK_UniqueConsignRef = "C0001PIC";
			var transportPic = consolPic.Transports[0];
			transportPic.JW_IsLinked = true;
			transportPic.JW_ETD = now.AddDays(-20);
			transportPic.JW_ETA = now.AddDays(-10);

			var consolDlv = shipment.Consols.AddNew();
			consolDlv.JK_TransportMode = "SEA";
			consolPic.JK_UniqueConsignRef = "C0002DLV";
			var transportDlv = consolDlv.Transports[0];
			transportDlv.JW_IsLinked = true;
			transportDlv.JW_ETD = now.AddDays(-10);
			transportDlv.JW_ETA = now;

			var containerPic1 = consolPic.Containers.AddNew();
			containerPic1.JC_ContainerNum = "CONTSAME";
			var containerDlv1 = consolDlv.Containers.AddNew();
			containerDlv1.JC_ContainerNum = "CONTSAME";
			var containerPic2 = consolPic.Containers.AddNew();
			containerPic2.JC_ContainerNum = "CONTPIC2";
			var containerDlv2 = consolDlv.Containers.AddNew();
			containerDlv2.JC_ContainerNum = "CONTDLV2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			containerPic1.PackLines.Add(packLine1);
			containerDlv1.PackLines.Add(packLine1);

			var packLine2 = shipment.OuterPackLines.AddNew();
			containerPic2.PackLines.Add(packLine2);
			containerDlv2.PackLines.Add(packLine2);

			Factory.SaveForTesting();

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(roleType, shipment)), true, true);
			var shipmentData = writer.GetDataObject(shipment);

			AssertEquals($"Should be 2 {(roleType == RecipientRoleType.PCA ? "Pickup" : "Delivery")} containers", 2, shipmentData.ContainerCount);
			AssertEquals(1, shipmentData.ContainerCollection.Count(c => c.ContainerNumber.Equals("CONTSAME")));
			AssertEquals(1, shipmentData.ContainerCollection.Count(c => c.ContainerNumber.Equals(roleType == RecipientRoleType.PCA ? "CONTPIC2" : "CONTDLV2")));
		}

		#endregion

		public void TestEventBranchHomePort()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, false).GetDataObject(shipment);

			AssertEquals(GlbBranch.CurrentBranch.HomePort.Code, shipmentData.EventBranchHomePort.Code);
			AssertEquals(GlbBranch.CurrentBranch.HomePort.RL_PortName, shipmentData.EventBranchHomePort.Name);
		}

		#region TestDestinationGoodsValueAndExchangeRate

		public void TestDestinationGoodsValueAndExchangeRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.UnitedStates))
			{
				var exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = Constants.CurrencyCodes.Canada;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = new ZDateTime(2024, 06, 01);
				exchangeRate.RE_ExpiryDate = new ZDateTime(2024, 06, 30);
				exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_SellRate = 0.736431m;

				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_GoodsValue = 1000m;
				shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.UnitedStates;
				shipmentBO.JS_RL_NKOrigin = "HKHKG";
				shipmentBO.JS_RL_NKDestination = "CAVAN";
				shipmentBO.JS_E_DEP = new ZDateTime(2024, 06, 15);
				Factory.SaveForTesting();

				var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
				AssertNull("No DestinationGoodsValue element populated when registry is turned off", shipmentData.DestinationGoodsValue);
				AssertNull("No DestinationGoodsValueCurrency element populated when registry is turned off", shipmentData.DestinationGoodsValueCurrency);
				AssertNull("No DestinationExchangeRate element populated when registry is turned off", shipmentData.DestinationExchangeRate);

				using (FreightDataRegistry.Instance.ExportDestinationValueInUXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
					AssertEquals(1357.9m, shipmentData.DestinationGoodsValue);
					AssertEquals("CAD", shipmentData.DestinationGoodsValueCurrency.Code);
					AssertEquals(1.3579m, shipmentData.DestinationExchangeRate);
				}
			}
		}

		#endregion

		#region PopulateCarriageShipmentInstruction With Empty ConfirmAddress

		public void TestPopulatePickupShipmentInstruction_FCL_WithEmptyConfirmAddress_WithConsignorPickupAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = header.MainAddress.PK;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var consol = shipmentBO.Consols.AddNew();
			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var confirm = container.Confirms.AddNew();
			confirm.EU_JC = container.PK;
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.OriginPickup, Constants.FCLEquipmentNeeded.SideLoader, ZDateTime.Now);

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulatePickupShipmentInstruction_FCL_WithEmptyConfirmAddress_WithoutConsignorPickupAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var consol = shipmentBO.Consols.AddNew();
			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var confirm = container.Confirms.AddNew();
			confirm.EU_JC = container.PK;
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.OriginPickup, Constants.FCLEquipmentNeeded.SideLoader, ZDateTime.Now);

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulateDeliveryShipmentInstruction_FCL_WithEmptyConfirmAddress_WithConsigneeDeliveryAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = header.MainAddress.PK;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var consol = shipmentBO.Consols.AddNew();
			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var confirm = container.Confirms.AddNew();
			confirm.EU_JC = container.PK;
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.FCLEquipmentNeeded.SideLoader, ZDateTime.Now);

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulateDeliveryShipmentInstruction_FCL_WithEmptyConfirmAddress_WithoutConsigneeDeliveryAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var consol = shipmentBO.Consols.AddNew();
			var container = consol.Containers.AddNew();
			shipmentBO.OuterPackLines.AddNew().SetContainer(container.PK);

			var confirm = container.Confirms.AddNew();
			confirm.EU_JC = container.PK;
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.FCLEquipmentNeeded.SideLoader, ZDateTime.Now);

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulatePickupInstruction_LCL_WithEmptyConfirmAddress_WithConsignorPickupAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = header.MainAddress.PK;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var confirm = shipmentBO.PickupConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.OriginPickup, Constants.LCLAIREquipmentNeeded.HandHaulier, ZDateTime.Now);

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulatePickupInstruction_LCL_WithEmptyConfirmAddress_WithoutConsignorPickupAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var confirm = shipmentBO.PickupConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.OriginPickup, Constants.LCLAIREquipmentNeeded.HandHaulier, ZDateTime.Now);

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var preCarriageShipmentData = shipmentData.PreCarriageShipmentCollection.First();
			var instruction = preCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, preCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("PIC", instruction.Type.Code);
			AssertEquals("Pickup", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulateDeliveryInstruction_LCL_WithEmptyConfirmAddress_WithConsigneeDeliveryAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = header.MainAddress.PK;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var confirm = shipmentBO.DeliveryConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.LCLAIREquipmentNeeded.HandHaulier, ZDateTime.Now);

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		public void TestPopulateDeliveryInstruction_LCL_WithEmptyConfirmAddress_WithoutConsigneeDeliveryAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Constants.ContainerModes.Loose;

			var packline = shipmentBO.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var confirm = shipmentBO.DeliveryConfirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.LCLAIREquipmentNeeded.HandHaulier, ZDateTime.Now);

			var divot = confirm.Divots[0];
			divot.J8_PackagesDelivered = 10;

			Assert(!IsConfirmAddressInitialized(confirm));
			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false).GetDataObject(shipmentBO);
			Assert(!IsConfirmAddressInitialized(confirm));

			var postCarriageShipmentData = shipmentData.PostCarriageShipmentCollection.First();
			var instruction = postCarriageShipmentData.InstructionCollection.First();

			AssertEquals(1, postCarriageShipmentData.InstructionCollection.Count);
			AssertEquals("DLV", instruction.Type.Code);
			AssertEquals("Delivery", instruction.Type.Description);
			AssertNull(instruction.Address);
		}

		void SetupConfirmData(CommonPickupDeliveryConfirm confirm, OrgAddress transportCoAddress, ZString pickupDeliveryType, ZString dropMode, ZDateTime plannedTime)
		{
			AssertNotNull(confirm);
			AssertNotNull(transportCoAddress);

			confirm.EU_PickupDeliveryType = pickupDeliveryType;
			confirm.EU_OA_TransportProvider = transportCoAddress.PK;
			confirm.EU_DropMode = dropMode;
			confirm.EU_VehicleRegistration = "AAA111";
			confirm.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			confirm.EU_PlannedPickupDeliveryTime = plannedTime;
			confirm.EU_RequestedPickupDeliveryTime = plannedTime.AddDays(1);
			confirm.EU_PickupDeliveryTime = plannedTime.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";
		}

		bool IsConfirmAddressInitialized(CommonPickupDeliveryConfirm confirm)
		{
			AssertNotNull(confirm);

			var fieldInfo = typeof(CommonPickupDeliveryConfirm).GetField("fConfirmAddress", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(fieldInfo);

			var fConfirmAddress = fieldInfo.GetValue(confirm) as JobDocAddress;
			return fConfirmAddress != null;
		}

		#endregion

		#region Populate Gateways

		public void TestGatewaysArePopulated()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var gateway2 = shipment.Gateways.AddNew();
			var gateway1 = shipment.Gateways.AddNew();
			var gateway3 = shipment.Gateways.AddNew();

			SetGatewaySequenceAndAddress(gateway1, 1, "ONE");
			SetGatewaySequenceAndAddress(gateway2, 2, "TWO");
			SetGatewaySequenceAndAddress(gateway3, 3, "THREE");

			Factory.SaveForTesting();

			var loadedShipment = NewUniversalObjectFactory().Load<ForwardingShipment>(shipment.PK);

			var shipmentData = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, loadedShipment)), true, true).GetDataObject(loadedShipment);

			AssertEquals("GatewayInfos Count", 3, shipmentData.GatewayInfoCollection.Count);

			var gatewayCollectionAsTuples = shipmentData.GatewayInfoCollection.Select(g => ((int)g.Order, g.Forwarder.AddressShortCode.Value.ToString()));

			AssertCollectionContains((1, "ONE"), gatewayCollectionAsTuples);
			AssertCollectionContains((2, "TWO"), gatewayCollectionAsTuples);
			AssertCollectionContains((3, "THREE"), gatewayCollectionAsTuples);
		}

		void SetGatewaySequenceAndAddress(ShipmentGateway gateway, ZByte sequence, ZString addressCode)
		{
			gateway.JSG_Sequence = sequence;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = addressCode;
			gateway.JSG_OA_ForwarderAddress = orgAddress.PK;
		}

		#endregion

		#region Implementation

		ForwardingConsol SetupConsolWithShipments()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Constants.TransportModes.Sea;
			consolBO.JK_UniqueConsignRef = "C00010000";
			consolBO.JK_MasterBillNum = "M1234567";

			var shipmentBO1 = consolBO.GridShipments.AddNew();
			shipmentBO1.JS_UniqueConsignRef = "S00010001";
			shipmentBO1.JS_HouseBill = "H1234567";
			var shipmentBO1Sub1 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub1.JS_UniqueConsignRef = "S00010001-1";
			shipmentBO1Sub1.JS_HouseBill = "SSSS1111";

			var shipmentBO1Sub2 = shipmentBO1.CoLoadShipments.AddNew();
			shipmentBO1Sub2.JS_UniqueConsignRef = "S00010001-2";
			shipmentBO1Sub2.JS_HouseBill = "H7654321";
			var shipmentBO1Sub2Sub1 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub1.JS_UniqueConsignRef = "S00010001-2-1";
			shipmentBO1Sub2Sub1.JS_HouseBill = "SUBSUB11";
			var shipmentBO1Sub2Sub2 = shipmentBO1Sub2.CoLoadShipments.AddNew();
			shipmentBO1Sub2Sub2.JS_UniqueConsignRef = "S00010001-2-2";
			shipmentBO1Sub2Sub2.JS_HouseBill = "SUBSUB22";

			var shipmentBO2 = consolBO.GridShipments.AddNew();
			shipmentBO2.JS_UniqueConsignRef = "S00010002";
			shipmentBO2.JS_HouseBill = "S1111111";
			var shipmentBO2Sub1 = shipmentBO2.CoLoadShipments.AddNew();
			shipmentBO2Sub1.JS_UniqueConsignRef = "S00010002-1";
			shipmentBO2Sub1.JS_HouseBill = "IGNORED1";
			var shipmentBO2Sub2 = shipmentBO2.CoLoadShipments.AddNew();
			shipmentBO2Sub2.JS_UniqueConsignRef = "S00010002-2";
			shipmentBO2Sub2.JS_HouseBill = "IGNORED2";

			Factory.SaveForTesting();

			return consolBO;
		}

		protected override BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO)
		{
			return new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)), true, false);
		}

		BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO, RecipientRoleType recipientRoleType)
		{
			return new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, topLevelBO)), true, false);
		}

		#endregion
	}
}
