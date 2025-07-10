using System;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationSynchroniserTest : Customs.Business.Testing.SynchroniserTestCase
	{
		public void TestConsignorMiscOrgNotSynchronised()
		{
			Shipment.ConsignorPK = OrgHeader.LoadFromCode(Factory, "MISC").PK;
			Shipment.JS_RL_NKOrigin = "SGSIN";
			Shipment.JS_RL_NKDestination = "ZACPT";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			Assert(Declaration.JE_OH_Supplier.IsEmpty);
			Assert(Declaration.JE_OH_Exporter.IsEmpty);
		}

		public void TestConsigneeMiscOrgNotSynchronised()
		{
			Shipment.ConsigneePK = OrgHeader.LoadFromCode(Factory, "MISC").PK;
			Shipment.JS_RL_NKOrigin = "ZACPT";
			Shipment.JS_RL_NKDestination = "SGSIN";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			Assert(Declaration.JE_OH_Importer.IsEmpty);
		}

		public void TestIncoTermSynchronisation()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(Core.Constants.IncoTerms.CarriagePaidTo, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(UnitPriceTermTypeCodeList.Codes.CIF, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeAlongsideShip;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(UnitPriceTermTypeCodeList.Codes.FAS, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(UnitPriceTermTypeCodeList.Codes.EXW, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndInsurance;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(UnitPriceTermTypeCodeList.Codes.CNI, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(UnitPriceTermTypeCodeList.Codes.CFR, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyUnpaid;
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(Core.Constants.IncoTerms.DeliveredDutyUnpaid, Declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = "XXX";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("XXX", Declaration.JE_ShipmentIncoTerm);
		}

		public void TestShipmentSynchronisation_Transhipment()
		{
			Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "ZACPT";
			Shipment.JS_HouseBill = "HOUSEBILL";
			Shipment.JS_ActualWeight = 13.3;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			Shipment.JS_ActualVolume = 2;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			Shipment.JS_OuterPacks = 99;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			Shipment.JS_GoodsDescription = "GOODS DESCRIPTION";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Importer);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Exporter);
			AssertEquals("9", Declaration.JE_ContainerMode);
			AssertEquals(Shipment.JS_RL_NKOrigin, Declaration.JE_RL_NKOrigin);
			AssertEquals(Shipment.JS_RL_NKDestination, Declaration.JE_RL_NKFinalDestination);
			AssertEquals(Shipment.JS_HouseBill, Declaration.JE_HouseBill);
			AssertEquals(Shipment.JS_HouseBill, Declaration.SG_OutwardHAWB);
			AssertEquals(Shipment.JS_ActualWeight, Declaration.JE_TotalWeight);
			AssertEquals(Shipment.JS_UnitOfWeight, Declaration.JE_TotalWeightUnit);
			AssertEquals(Shipment.JS_ActualVolume, Declaration.JE_TotalVolume);
			AssertEquals(Shipment.JS_UnitOfVolume, Declaration.JE_TotalVolumeUnit);
			AssertEquals(Shipment.JS_OuterPacks, Declaration.JE_TotalNoOfPacks);
			AssertEquals(Shipment.JS_F3_NKPackType, Declaration.JE_TotalNoOfPacksPackType);
			AssertEquals(Shipment.JS_GoodsDescription, Declaration.JE_GoodsDescription);
		}

		public void TestShipmentSynchronisation_Import()
		{
			Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "SGSIN";
			Shipment.JS_HouseBill = "HOUSEBILL";
			Shipment.JS_ActualWeight = 13.3;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			Shipment.JS_ActualVolume = 2;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			Shipment.JS_OuterPacks = 99;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			Shipment.JS_GoodsDescription = "GOODS DESCRIPTION";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(Shipment.ConsigneePK, Declaration.JE_OH_Importer);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Exporter);
			AssertEquals("9", Declaration.JE_ContainerMode);
			AssertEquals(Shipment.JS_RL_NKOrigin, Declaration.JE_RL_NKOrigin);
			AssertEquals(Shipment.JS_RL_NKDestination, Declaration.JE_RL_NKFinalDestination);
			AssertEquals(Shipment.JS_HouseBill, Declaration.JE_HouseBill);
			AssertEquals(Shipment.JS_HouseBill, Declaration.SG_OutwardHAWB);
			AssertEquals(Shipment.JS_ActualWeight, Declaration.JE_TotalWeight);
			AssertEquals(Shipment.JS_UnitOfWeight, Declaration.JE_TotalWeightUnit);
			AssertEquals(Shipment.JS_ActualVolume, Declaration.JE_TotalVolume);
			AssertEquals(Shipment.JS_UnitOfVolume, Declaration.JE_TotalVolumeUnit);
			AssertEquals(Shipment.JS_OuterPacks, Declaration.JE_TotalNoOfPacks);
			AssertEquals(UnitOfQuantityCodeList.Codes.PAT, Declaration.JE_TotalNoOfPacksPackType);
			AssertEquals(Shipment.JS_GoodsDescription, Declaration.JE_GoodsDescription);
		}

		public void TestShipmentSynchronisation_Export()
		{
			Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			Shipment.JS_RL_NKOrigin = "SGSIN";
			Shipment.JS_RL_NKDestination = "ZACPT";
			Shipment.JS_HouseBill = "HOUSEBILL";
			Shipment.JS_ActualWeight = 13.3;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			Shipment.JS_ActualVolume = 2;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			Shipment.JS_OuterPacks = 99;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			Shipment.JS_GoodsDescription = "GOODS DESCRIPTION";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Importer);
			AssertEquals(Shipment.ConsignorPK, Declaration.JE_OH_Supplier);
			AssertEquals(Shipment.ConsignorPK, Declaration.JE_OH_Exporter);
			AssertEquals("9", Declaration.JE_ContainerMode);
			AssertEquals(Shipment.JS_RL_NKOrigin, Declaration.JE_RL_NKOrigin);
			AssertEquals(Shipment.JS_RL_NKDestination, Declaration.JE_RL_NKFinalDestination);
			AssertEquals(Shipment.JS_HouseBill, Declaration.JE_HouseBill);
			AssertEquals(Shipment.JS_HouseBill, Declaration.SG_OutwardHAWB);
			AssertEquals(Shipment.JS_ActualWeight, Declaration.JE_TotalWeight);
			AssertEquals(Shipment.JS_UnitOfWeight, Declaration.JE_TotalWeightUnit);
			AssertEquals(Shipment.JS_ActualVolume, Declaration.JE_TotalVolume);
			AssertEquals(Shipment.JS_UnitOfVolume, Declaration.JE_TotalVolumeUnit);
			AssertEquals(Shipment.JS_OuterPacks, Declaration.JE_TotalNoOfPacks);
			AssertEquals(Shipment.JS_F3_NKPackType, Declaration.JE_TotalNoOfPacksPackType);
			AssertEquals(Shipment.JS_GoodsDescription, Declaration.JE_GoodsDescription);
		}

		public void TestConsolSynchronisation_Transhipment()
		{
			Shipment.JS_RL_NKOrigin = "ZACPT";
			Shipment.JS_RL_NKDestination = "USLAX";
			InwardConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			InwardConsol.JK_MasterBillNum = "08111111111";
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;
			var transport = InwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 1, 1);
			transport.JW_VoyageFlight = "QF123";
			transport.JW_Vessel = "INWARDVESSEL";
			OutwardConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OutwardConsol.JK_MasterBillNum = "OCEANBILL";
			OutwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			transport = OutwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 2, 2);
			transport.JW_VoyageFlight = "987";
			transport.JW_Vessel = "OUTWARDVESSEL";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(InwardConsol.JK_TransportMode, Declaration.JE_TransportMode);
			AssertEquals(InwardConsol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(InwardConsol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(InwardConsol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals(InwardConsol.ShippingLinePK, Declaration.JE_OH_ShippingLine);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_InwardCarrierAgent);
			AssertEquals(InwardConsol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);
			AssertEquals(InwardConsol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(InwardConsol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(InwardConsol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals(OutwardConsol.JK_TransportMode, Declaration.SG_OutwardTransportMode);
			AssertEquals(OutwardConsol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);
			AssertEquals(OutwardConsol.JK_MasterBillNum, Declaration.SG_OutwardMAWB);
			AssertEquals(OutwardConsol.JK_JX_JV_VoyageFlight, Declaration.SG_OutwardVoyageFlightNo);
			AssertEquals("Outward Sea Consol should default the shipping line to the Outward Carrier Agent field as well", OutwardConsol.ShippingLinePK, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(OutwardConsol.JK_JX_JV_NKVessel, Declaration.SG_OutwardVesselName);
			AssertEquals(OutwardConsol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
		}

		public void TestConsolSynchronisation_TranshipmentTwoSeaConsols()
		{
			Shipment.JS_RL_NKOrigin = "ZACPT";
			Shipment.JS_RL_NKDestination = "USLAX";
			InwardConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			InwardConsol.JK_MasterBillNum = "OB240249";
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			var transport = InwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2013, 09, 15);
			transport.JW_VoyageFlight = "152W";
			transport.JW_Vessel = "INWARDVESSEL";
			OutwardConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OutwardConsol.JK_MasterBillNum = "OCEANBILL2";
			OutwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			AssertEquals("Pre-condition: shipment should be attached to 2 consols now", 2, Shipment.Consols.Count);
			AssertEquals(InwardConsol, Shipment.Consols[0]);
			AssertEquals(OutwardConsol, Shipment.Consols[1]);
			transport = OutwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2013, 09, 23);
			transport.JW_VoyageFlight = "987";
			transport.JW_Vessel = "OUTWARDVESSEL";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(InwardConsol.JK_TransportMode, Declaration.JE_TransportMode);
			AssertEquals(InwardConsol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(InwardConsol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(InwardConsol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals("Transhipments will default the Inward Shipping Line to Carrier", InwardConsol.ShippingLinePK, Declaration.JE_OH_ShippingLine);
			AssertEquals("For SEA jobs default Inward Shipping Line to Inward Carrier Agent as well as Carrier", InwardConsol.ShippingLine.PK, Declaration.JE_OH_InwardCarrierAgent);
			AssertEquals(InwardConsol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);
			AssertEquals(InwardConsol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(InwardConsol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(InwardConsol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals(OutwardConsol.JK_TransportMode, Declaration.SG_OutwardTransportMode);
			AssertEquals(OutwardConsol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);
			AssertEquals(OutwardConsol.JK_MasterBillNum, Declaration.SG_OutwardMAWB);
			AssertEquals(OutwardConsol.JK_JX_JV_VoyageFlight, Declaration.SG_OutwardVoyageFlightNo);
			AssertEquals("Outward Sea Consol should default the shipping line to the Outward Carrier Agent field as well", OutwardConsol.ShippingLinePK, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(OutwardConsol.JK_JX_JV_NKVessel, Declaration.SG_OutwardVesselName);
			AssertEquals(OutwardConsol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
		}

		public void TestConsolSynchronisation_TranshipmentTwoAirConsols()
		{
			Shipment.JS_RL_NKOrigin = "ZACPT";
			Shipment.JS_RL_NKDestination = "USLAX";
			InwardConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			InwardConsol.JK_MasterBillNum = "08111111111";
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;
			var transport = InwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2013, 09, 23);
			transport.JW_VoyageFlight = "QF220";
			OutwardConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			OutwardConsol.JK_MasterBillNum = "6184824576";
			OutwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			AssertEquals("Pre-condition: shipment should be attached to 2 consols now", 2, Shipment.Consols.Count);
			AssertEquals(InwardConsol, Shipment.Consols[0]);
			AssertEquals(OutwardConsol, Shipment.Consols[1]);
			transport = OutwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2013, 09, 23);
			transport.JW_VoyageFlight = "SQ019";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(InwardConsol.JK_TransportMode, Declaration.JE_TransportMode);
			AssertEquals(InwardConsol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(InwardConsol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(InwardConsol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals("Transhipments will default the Inward Shipping Line to the Carrier", InwardConsol.ShippingLinePK, Declaration.JE_OH_ShippingLine);
			AssertEquals("For AIR jobs the Inward Carrier Agent should not be defaulted", Guid.Empty, Declaration.JE_OH_InwardCarrierAgent);
			AssertEquals(string.Empty, Declaration.JE_VesselName);
			AssertEquals(InwardConsol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(InwardConsol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(InwardConsol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals(OutwardConsol.JK_TransportMode, Declaration.SG_OutwardTransportMode);
			AssertEquals(OutwardConsol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);
			AssertEquals(OutwardConsol.JK_MasterBillNum, Declaration.SG_OutwardMAWB);
			AssertEquals(OutwardConsol.JK_JX_JV_VoyageFlight, Declaration.SG_OutwardVoyageFlightNo);
			AssertEquals("For AIR jobs the Outward Carrier Agent should not be defaulted", Guid.Empty, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(string.Empty, Declaration.SG_OutwardVesselName);
			AssertEquals(OutwardConsol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
		}

		public void TestConsolSynchronisation_TranshipmentOneAirConsolTwoShipments()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "ZACPT";
			shipment1.JS_RL_NKDestination = "SGSIN";
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = "SGSIN";
			shipment2.JS_RL_NKDestination = "USLAX";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "6184824576";
			consol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			consol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			consol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			consol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			Declaration.JE_JS = shipment1.PK;
			var transport = consol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2013, 09, 23);
			transport.JW_VoyageFlight = "QF220";
			AssertEquals("Pre-condition: consol should have 2 shipments attached now", 2, consol.Shipments.Count);
			AssertEquals(shipment1, consol.Shipments[0]);
			AssertEquals(shipment2, consol.Shipments[1]);
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(consol.JK_TransportMode, Declaration.JE_TransportMode);
			AssertEquals(consol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(consol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(consol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals("Transhipments will default the Inward Shipping Line to the Carrier", consol.ShippingLinePK, Declaration.JE_OH_ShippingLine);
			AssertEquals("For AIR jobs the Inward Carrier Agent should not be defaulted", Guid.Empty, Declaration.JE_OH_InwardCarrierAgent);
			AssertEquals(string.Empty, Declaration.JE_VesselName);
			AssertEquals(consol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(consol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(consol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals("For AIR jobs the Outward Carrier Agent should not be defaulted", Guid.Empty, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(consol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Importer);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_Exporter);
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, Declaration.JE_ContainerMode);
			AssertEquals(shipment1.JS_RL_NKOrigin, Declaration.JE_RL_NKOrigin);
		}

		public void TestConsolSynchronisation_Import()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "SGSIN";
			//var shippingLine = Factory.New<OrgHeader>();
			InwardConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			InwardConsol.JK_MasterBillNum = "08111111111";
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			Transport transport = InwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 1, 1);
			transport.JW_VoyageFlight = "QF123";
			transport.JW_Vessel = "INWARDVESSEL";
			OutwardConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OutwardConsol.JK_MasterBillNum = "OCEANBILL";
			OutwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			transport = OutwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 2, 2);
			transport.JW_VoyageFlight = "987";
			transport.JW_Vessel = "OUTWARDVESSEL";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(InwardConsol.JK_TransportMode, Declaration.JE_TransportMode);
			AssertEquals(InwardConsol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(InwardConsol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(InwardConsol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals(InwardConsol.ShippingLinePK, Declaration.JE_OH_ShippingLine);
			AssertEquals(ZGuid.Empty, Declaration.JE_OH_InwardCarrierAgent);
			AssertEquals(InwardConsol.ReceivingForwarderPK, Declaration.JE_OH_Forwarder);
			AssertEquals(InwardConsol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);
			AssertEquals(InwardConsol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(InwardConsol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(InwardConsol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals("", Declaration.SG_OutwardTransportMode);
			AssertEquals(ZDateTime.Empty, Declaration.JE_ExportDate);
			AssertEquals(OutwardConsol.JK_MasterBillNum, Declaration.SG_OutwardMAWB);
			AssertEquals(OutwardConsol.JK_JX_JV_VoyageFlight, Declaration.SG_OutwardVoyageFlightNo);
			AssertEquals(OutwardConsol.ShippingLinePK, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(OutwardConsol.JK_JX_JV_NKVessel, Declaration.SG_OutwardVesselName);
			AssertEquals(OutwardConsol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
		}

		public void TestConsolSynchronisation_Export()
		{
			Shipment.JS_RL_NKOrigin = "SGSIN";
			Shipment.JS_RL_NKDestination = "USLAX";
			InwardConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			InwardConsol.JK_MasterBillNum = "08111111111";
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			InwardConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;
			InwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			Transport transport = InwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 1, 1);
			transport.JW_VoyageFlight = "QF123";
			transport.JW_Vessel = "INWARDVESSEL";
			OutwardConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OutwardConsol.JK_MasterBillNum = "OCEANBILL";
			OutwardConsol.SetDefaultShippingLineAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			OutwardConsol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			transport = OutwardConsol.Transports.AddNew();
			transport.JW_ATA = new ZDateTime(2007, 2, 2);
			transport.JW_VoyageFlight = "987";
			transport.JW_Vessel = "OUTWARDVESSEL";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", Declaration.JE_TransportMode);
			AssertEquals(InwardConsol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			AssertEquals(InwardConsol.JK_MasterBillNum, Declaration.JE_MasterBill);
			AssertEquals(InwardConsol.JK_JX_JV_VoyageFlight, Declaration.JE_VoyageFlightNo);
			AssertEquals(InwardConsol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);
			AssertEquals(InwardConsol.JK_RL_NKLoadPort, Declaration.JE_RL_NKPortOfLoading);
			AssertEquals(InwardConsol.JK_OA_ArrivalCTOAddress, Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(InwardConsol.JK_OA_UnpackDepotAddress, Declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals(OutwardConsol.JK_TransportMode, Declaration.SG_OutwardTransportMode);
			AssertEquals(OutwardConsol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);
			AssertEquals(OutwardConsol.JK_MasterBillNum, Declaration.SG_OutwardMAWB);
			AssertEquals(OutwardConsol.JK_JX_JV_VoyageFlight, Declaration.SG_OutwardVoyageFlightNo);
			AssertEquals(OutwardConsol.ShippingLinePK, Declaration.OutwardShippingLineForwarderPK);
			AssertEquals(OutwardConsol.JK_JX_JV_NKVessel, Declaration.SG_OutwardVesselName);
			AssertEquals(OutwardConsol.JK_RL_NKDischargePort, Declaration.JE_RL_NKPortOfArrival);
			AssertEquals(OutwardConsol.SendingForwarderPK, Declaration.JE_OH_Forwarder);
		}

		public void TestContainerSynchronisation_Export()
		{
			Shipment.JS_RL_NKOrigin = "SGSIN";
			Shipment.JS_RL_NKDestination = "USLAX";
			ForwardingContainer container = OutwardConsol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(OutwardConsol, container);
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertNotNull(Declaration.CusContainers.Find("CONTAINER"));
		}

		public void TestContainerSynchronisation_Import()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "SGSIN";
			ForwardingContainer container = InwardConsol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(InwardConsol, container);
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertNotNull(Declaration.CusContainers.Find("CONTAINER"));
		}

		public void TestPortsSynchronisation()
		{
			Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "USABE";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("USZZZ", Declaration.JE_RL_NKPortOfArrival);
			AssertEquals(Shipment.JS_RL_NKOrigin, Declaration.JE_RL_NKPortOfLoading);
			Shipment.JS_RL_NKOrigin = "USABE";
			Shipment.JS_RL_NKDestination = "USLAX";
			Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(Shipment.JS_RL_NKDestination, Declaration.JE_RL_NKPortOfArrival);
			AssertEquals("USZZZ", Declaration.JE_RL_NKPortOfLoading);
		}

		#region Synchroniser
		JobDeclarationSynchroniser Synchroniser
		{
			get
			{
				return synchroniser ?? (synchroniser = (JobDeclarationSynchroniser)Declaration.ShipmentSynchroniser);
			}
		}

		JobDeclarationSynchroniser synchroniser;
		#endregion
		#region Inward Consol
		ForwardingConsol InwardConsol
		{
			get
			{
				if (inwardConsol == null)
				{
					inwardConsol = Shipment.Consols.AddNew();
					inwardConsol.JK_RL_NKDischargePort = "SGSIN";
					inwardConsol.JK_RL_NKLoadPort = "USLAX";
				}

				return inwardConsol;
			}
		}

		ForwardingConsol inwardConsol;
		#endregion
		#region Outward Consol
		ForwardingConsol OutwardConsol
		{
			get
			{
				if (outwardConsol == null)
				{
					outwardConsol = Shipment.Consols.AddNew();
					outwardConsol.JK_RL_NKDischargePort = "USLAX";
					outwardConsol.JK_RL_NKLoadPort = "SGSIN";
				}

				return outwardConsol;
			}
		}

		ForwardingConsol outwardConsol;
		#endregion
		#region Shipment
		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;
		#endregion
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_JS = Shipment.PK;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "AUSYD", "SYDNEY");
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "USLAX", "LOS ANGELES");
			AddInfoCUSDECValidationTest.CreatePortCusCodeList(helper, "SGSIN", "SINGAPORE");
			Factory.Save();
		}
		#endregion
	}
}
