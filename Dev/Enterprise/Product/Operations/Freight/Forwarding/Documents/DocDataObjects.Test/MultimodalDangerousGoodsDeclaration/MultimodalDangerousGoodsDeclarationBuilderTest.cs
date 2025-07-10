using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class MultimodalDangerousGoodsDeclarationBuilderTest : TestCaseWithFactory
	{
		#region Build

		public void TestBuild()
		{
			var consol = CreateConsol();
			var container = consol.Containers.First();
			AssertNotNull("consol has a container", container);
			var parameters = new DummyDocDataObjectParameters() { Data = container };

			var builder = new MultimodalDangerousGoodsDeclarationBuilder(parameters);
			var decl = builder.Build();

			AssertNotNull("Multimodal Dangerous Goods Declaration", decl);
		}

		public void TestBuildWithoutAirAndSeaTransport()
		{
			var consol = CreateConsol();

			foreach (var transport in consol.Transports)
			{
				((Freight.Business.Transport)transport).JW_TransportMode = Core.Constants.TransportModes.Road;
			}

			var container = consol.Containers.First();
			AssertNotNull("consol has a container", container);
			var parameters = new DummyDocDataObjectParameters() { Data = container };

			var builder = new MultimodalDangerousGoodsDeclarationBuilder(parameters);

			AssertNoExceptionThrown(() =>
			{
				builder.Build();
			});
		}

		#endregion

		#region Shipper

		public void TestPopulateShipper()
		{
			var consol = CreateConsol();
			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertionHelper.AssertAddressData(consol.SendingForwarder, decl.Shipper);
		}

		#endregion

		#region Consignee

		public void TestPopulateConsignee()
		{
			var consol = CreateConsol();
			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertionHelper.AssertAddressData(consol.ReceivingForwarder, decl.Consignee);
		}

		#endregion

		#region TransportDocumentNumber

		public void TestPopulateTransportDocumentNumber()
		{
			var consol = CreateConsol();
			consol.JK_MasterBillNum = "MB12345";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.TransportDocumentNumber), "MB12345", decl.TransportDocumentNumber);
		}

		#endregion

		#region ShipperReference

		public void TestPopulateShipperReference()
		{
			var consol = CreateConsol();
			consol.JK_BookingReference = "AG12345";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.ShipperReference), "AG12345", decl.ShipperReference);
		}

		#endregion

		#region FreightForwarderReference

		public void TestPopulateFreightForwarderReference()
		{
			var consol = CreateConsol();
			consol.JK_AgentsReference = "FR12345";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.FreightForwarderReference), "FR12345", decl.FreightForwarderReference);
		}

		#endregion

		#region Vessel

		public void TestPopulateVessel()
		{
			var consol = CreateConsol();

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.Vessel), "FUA KAVENGA", decl.Vessel.Name);
		}

		#endregion

		#region ETD

		[TestDate(2020, 4, 28)]
		public void TestPopulateETD()
		{
			var consol = CreateConsol();
			var transport = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea);
			AssertNotNull("First sea port found in consol", transport);
			transport.JW_ETD = ZDateTime.Now;

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.ETD), ZDateTime.Now, decl.ETD);
		}

		#endregion

		#region PortOfLoading

		public void TestPopulatePortOfLoading()
		{
			var consol = CreateConsol();
			var transport = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea);
			AssertNotNull("First sea port found in consol", transport);
			transport.JW_RL_NKLoadPort = "AUSYD";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.PortOfLoading), "AUSYD", decl.PortOfLoading.Code);
		}

		#endregion

		#region PortOfDischarge

		public void TestPopulatePortOfDischarge()
		{
			var consol = CreateConsol();
			var transport = consol.Transports.Cast<Freight.Business.Transport>().Last(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea);
			AssertNotNull("Last sea port found in consol", transport);
			transport.JW_RL_NKLoadPort = "AUSYD";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals(nameof(MultimodalDangerousGoodsDeclaration.PortOfDischarge), "AUSYD", decl.PortOfDischarge.Code);
		}

		#endregion

		#region Destination

		public void TestPopulateDestination()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments.Cast<ForwardingShipment>().First();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKDestination = "AUMEL";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKDestination = "AUMEL";

			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();
			AssertEquals("use shipment port as destination when all the shipment have the same destination", "AUMEL", decl.Destination.Code);

			shipment1.JS_RL_NKDestination = "AUADL";

			decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();
			AssertEquals("fall back from consol when all the shipment do not have the same destination", "AUSYD", decl.Destination.Code);

			consol.JK_AgentType = Constants.AgentType.Direct;

			decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();
			AssertEquals("from first shiment's destination port when consol is direct", "AUADL", decl.Destination.Code);
		}

		#endregion

		#region CarrierName

		public void TestPopulateCarrierName()
		{
			Assert("", true);
		}

		#endregion

		#region Goods Details

		public void TestPopulateGoodsDetails_FilterUNDGsByTransportMode()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Road;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			transport.JW_RL_NKLoadPort = "CNNJG";
			transport.JW_RL_NKDiscPort = "CNSHA";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_Vessel = "HUA TAI HE";
			transport2.JW_VoyageFlight = "222";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance1.DG_UNNO = "6666";

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance2.DG_UNNO = "7777";

			var undgIMO = packline.UNDGs.AddNew();
			undgIMO.DI_DG = substance1.PK;
			undgIMO.DI_DGVolume = 2m;
			undgIMO.DI_UnitOfVolume = "M3";
			undgIMO.DI_DGWeight = 200m;
			undgIMO.DI_UnitOfWeight = "KG";
			undgIMO.DI_F3_NKPackType = "BAG";
			undgIMO.DI_PackageCount = 2;

			var undgIAT = packline.UNDGs.AddNew();
			undgIAT.DI_DG = substance2.PK;
			undgIAT.DI_DGVolume = 3m;
			undgIAT.DI_UnitOfVolume = "M3";
			undgIAT.DI_DGWeight = 300m;
			undgIAT.DI_UnitOfWeight = "KG";
			undgIAT.DI_F3_NKPackType = "BAG";
			undgIAT.DI_PackageCount = 3;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
										"                       2 BAG     UN6666 (0.0C c.c.)                               200.00 KG     2.00 M3";
			AssertEquals("Only IMO UNDGs are populated", expectedGoodsDetails, declaration.GoodsDetails);
		}

		public void TestPopulateGoodsDetails_WithStandards_JTT_ADN_CFR()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Road;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			transport.JW_RL_NKLoadPort = "CNNJG";
			transport.JW_RL_NKDiscPort = "CNSHA";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_Vessel = "HUA TAI HE";
			transport2.JW_VoyageFlight = "222";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT;
			substance1.DG_UNNO = "6666";

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN;
			substance2.DG_UNNO = "7777";

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			substance3.DG_UNNO = "8888";

			var undgJTT = packline.UNDGs.AddNew();
			undgJTT.DI_DG = substance1.PK;
			undgJTT.DI_DGVolume = 2m;
			undgJTT.DI_UnitOfVolume = "M3";
			undgJTT.DI_DGWeight = 200m;
			undgJTT.DI_UnitOfWeight = "KG";
			undgJTT.DI_F3_NKPackType = "BAG";
			undgJTT.DI_PackageCount = 2;

			var undgADN = packline.UNDGs.AddNew();
			undgADN.DI_DG = substance2.PK;
			undgADN.DI_DGVolume = 3m;
			undgADN.DI_UnitOfVolume = "M3";
			undgADN.DI_DGWeight = 300m;
			undgADN.DI_UnitOfWeight = "KG";
			undgADN.DI_F3_NKPackType = "BAG";
			undgADN.DI_PackageCount = 3;

			var undgCFR = packline.UNDGs.AddNew();
			undgCFR.DI_DG = substance3.PK;
			undgCFR.DI_DGVolume = 4m;
			undgCFR.DI_UnitOfVolume = "M3";
			undgCFR.DI_DGWeight = 400m;
			undgCFR.DI_UnitOfWeight = "KG";
			undgCFR.DI_F3_NKPackType = "BAG";
			undgCFR.DI_PackageCount = 4;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG     900.00 KG\r\n" +
										"                       2 BAG     UN6666 (0.0C c.c.)                               200.00 KG     2.00 M3\r\n" +
										"                       3 BAG     UN7777 (0.0C c.c.)                               300.00 KG     3.00 M3\r\n" +
										"                       4 BAG     UN8888 (0.0C c.c.)                               400.00 KG     4.00 M3";
			AssertEquals("UNDGs populated for all ADN, JTT and CFR substances", expectedGoodsDetails, declaration.GoodsDetails);
		}

		public void TestPopulateGoodsDetails_IncludeNumberAndTypeOfPackagesFromDGSubstance()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance1.DG_UNNO = "6666";

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGVolume = 2m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 200m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_F3_NKPackType = "BAG";
			undg1.DI_PackageCount = 20;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
										"                       20 BAG    UN6666 (0.0C c.c.)                               200.00 KG     2.00 M3";
			AssertEquals("One DG is populated", expectedGoodsDetails, declaration.GoodsDetails);

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance2.DG_UNNO = "6666";

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_DG = substance1.PK;
			undg2.DI_DGVolume = 0m;
			undg2.DI_UnitOfVolume = "M3";
			undg2.DI_DGWeight = 300m;
			undg2.DI_UnitOfWeight = "KG";
			undg2.DI_F3_NKPackType = "BAG";
			undg2.DI_F3_NKPackType = "PLT";
			undg2.DI_PackageCount = 40;

			declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			expectedGoodsDetails = "                       2 PLT                                        200.00 KG     500.00 KG\r\n" +
									"                       20 BAG    UN6666 (0.0C c.c.)                               200.00 KG     2.00 M3\r\n" +
									"                       40 PLT    UN6666 (0.0C c.c.)                               300.00 KG     0.00 M3";

			AssertEquals("Multiple DG are populated", expectedGoodsDetails, declaration.GoodsDetails);
		}

		public void TestPopulateGoodsDetails_IncludeFlashPoint()
		{
			#region Setup
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PopulateGoodsDetails_IncludeFlashPoint(true);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				PopulateGoodsDetails_IncludeFlashPoint(false);
			}

			void PopulateGoodsDetails_IncludeFlashPoint(bool activateIsCombustibleForDGItems)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C00001000";
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAAA0000007";
				container.JC_DeliveryMode = "CFS/CY";
				container.JC_IsShipperOwned = true;
				container.JC_GrossWeightUQ = "KG";
				container.JC_TareWeight = 1000;
				container.JC_DunnageWeight = 1000;
				container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SH0001000";
				shipment.JS_HouseBill = "HOUSEBILL001";
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_TransportMode = Constants.TransportModes.Sea;

				shipment.OuterPackLines.RemoveAndDeleteAll();

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 2;
				packline.JL_F3_NKPackType = "PLT";
				packline.JL_ActualWeight = 200;
				packline.JL_ActualWeightUQ = "KG";

				var substance1 = Factory.New<UNDGSubstance>();
				substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				substance1.DG_UNNO = "6666";
				substance1.DG_FlashPoint = "10";

				var undg1 = packline.UNDGs.AddNew();
				undg1.DI_DG = substance1.PK;
				undg1.DI_DGVolume = 2m;
				undg1.DI_UnitOfVolume = "M3";
				undg1.DI_DGWeight = 200m;
				undg1.DI_UnitOfWeight = "KG";
				undg1.DI_F3_NKPackType = "BAG";
				undg1.DI_PackageCount = 20;
				undg1.DI_DGFlashPoint = 1;
				undg1.DI_IsCombustible = false;

				container.PackLines.Add(packline);

				#endregion

				var parameters = new DummyDocDataObjectParameters() { Data = container };
				var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

				var expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
										"                       20 BAG    UN6666                                           200.00 KG     2.00 M3";

				if (!activateIsCombustibleForDGItems)
				{
					expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
										"                       20 BAG    UN6666 (1.0C c.c.)                               200.00 KG     2.00 M3";
				}
				AssertEquals("One DG is populated", expectedGoodsDetails, declaration.GoodsDetails);

				var substance2 = Factory.New<UNDGSubstance>();
				substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				substance2.DG_UNNO = "6666";

				var undg2 = packline.UNDGs.AddNew();
				undg2.DI_DG = substance1.PK;
				undg2.DI_DGVolume = 0m;
				undg2.DI_UnitOfVolume = "M3";
				undg2.DI_DGWeight = 300m;
				undg2.DI_UnitOfWeight = "KG";
				undg2.DI_F3_NKPackType = "BAG";
				undg2.DI_F3_NKPackType = "PLT";
				undg2.DI_PackageCount = 40;
				undg2.DI_DGFlashPoint = 2;
				undg2.DI_IsCombustible = true;

				declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

				if (activateIsCombustibleForDGItems)
				{
					expectedGoodsDetails = "                       2 PLT                                        200.00 KG     500.00 KG\r\n" +
										"                       20 BAG    UN6666                                           200.00 KG     2.00 M3\r\n" +
										"                       40 PLT    UN6666 (2.0C c.c.)                               300.00 KG     0.00 M3";
				}
				else
				{
					expectedGoodsDetails = "                       2 PLT                                        200.00 KG     500.00 KG\r\n" +
										"                       20 BAG    UN6666 (1.0C c.c.)                               200.00 KG     2.00 M3\r\n" +
										"                       40 PLT    UN6666 (2.0C c.c.)                               300.00 KG     0.00 M3";
				}

				AssertEquals("Multiple DG are populated", expectedGoodsDetails, declaration.GoodsDetails);
			}
		}

		public void TestPopulateGoodsDetails_IncludeNECWeightWhenSet()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN;
			substance1.DG_UNNO = "7777";

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			substance2.DG_UNNO = "8888";

			var undgADN = packline.UNDGs.AddNew();
			undgADN.DI_DG = substance1.PK;
			undgADN.DI_DGVolume = 3m;
			undgADN.DI_UnitOfVolume = "M3";
			undgADN.DI_DGWeight = 300m;
			undgADN.DI_UnitOfWeight = "KG";
			undgADN.DI_F3_NKPackType = "BAG";
			undgADN.DI_PackageCount = 3;
			undgADN.DI_NECWeight = 1;
			undgADN.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;

			var undgCFR = packline.UNDGs.AddNew();
			undgCFR.DI_DG = substance2.PK;
			undgCFR.DI_DGVolume = 4m;
			undgCFR.DI_UnitOfVolume = "M3";
			undgCFR.DI_DGWeight = 400m;
			undgCFR.DI_UnitOfWeight = "KG";
			undgCFR.DI_F3_NKPackType = "BAG";
			undgCFR.DI_PackageCount = 4;
			undgCFR.DI_NECWeight = 2;
			undgCFR.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG     700.00 KG\r\n" +
										"                       3 BAG     UN7777 (0.0C c.c.), NEC: 1.00 KG                 300.00 KG     3.00 M3\r\n" +
										"                       4 BAG     UN8888 (0.0C c.c.), NEC: 2.00 KG                 400.00 KG     4.00 M3";
			AssertEquals("NEC Weight is displayed on DG Goods Details when set", expectedGoodsDetails, declaration.GoodsDetails);
		}

		public void TestPopulateGoodsDetails_IncludeEMS()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance1.DG_UNNO = "6666";
			substance1.DG_FlashPoint = "10";

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGVolume = 2m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 200m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_F3_NKPackType = "BAG";
			undg1.DI_PackageCount = 20;
			undg1.DI_DGFlashPoint = 1;
			undg1.DI_IsCombustible = false;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
				"                       20 BAG    UN6666 (1.0C c.c.)                               200.00 KG     2.00 M3";

			AssertEquals("DG are populated Without EMS", expectedGoodsDetails, declaration.GoodsDetails);

			substance1.DG_EMS = "F-E, S-E";
			expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
				"                       20 BAG    UN6666 (1.0C c.c.) F-E, S-E                      200.00 KG     2.00 M3";

			declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals("DG are populated With EMS", expectedGoodsDetails, declaration.GoodsDetails);
		}

		public void TestPopulateGoodsDetails_NotIncludeUNVariantWithUNNO()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance1.DG_UNNO = "6666";
			substance1.DG_FlashPoint = "10";

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_DG = substance1.PK;
			undg1.DI_DGVolume = 2m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 200m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_F3_NKPackType = "BAG";
			undg1.DI_PackageCount = 20;
			undg1.DI_DGFlashPoint = 1;
			undg1.DI_IsCombustible = false;

			container.PackLines.Add(packline);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			var expectedGoodsDetails = "                       2 PLT                                        200.00 KG\r\n" +
				"                       20 BAG    UN6666 (1.0C c.c.)                               200.00 KG     2.00 M3";

			AssertEquals("DG are populated With UNNO Without Variant", expectedGoodsDetails, declaration.GoodsDetails);

			substance1.DG_Variant = "A";
			declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals("DG are populated With UNNO With Variant", expectedGoodsDetails, declaration.GoodsDetails);
		}

		#region MostInterestingTransportReference

		public void TestPopulateMostInterestingTransportReference()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Random Vesel";
			vessel.RV_LloydsNumber = "Lloy002";
			vessel.RV_VesselType = "CV";
			vessel.RV_RN_NKCountryOfReg = "BE";
			vessel.RV_RadioCallSign = "OX54F";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.FillWithValidTestData();
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.JW_Vessel = "Random Vesel";
			transport.JW_VoyageFlight = "FREIGHT001";
			transport.JW_ATD = new ZDateTime(2021, 3, 18);
			transport.JW_ATD = new ZDateTime(2021, 3, 16);

			#endregion

			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals("For sea mode", "Random Vesel / FREIGHT001 / Lloy002", declaration.MostInterestingTransportReference);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_Vessel = "Random Vesel";
			transport.JW_VoyageFlight = "FREIGHT001";
			declaration = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			AssertEquals("For air mode", "FREIGHT001 / 16-Mar", declaration.MostInterestingTransportReference);
		}

		#endregion

		#endregion

		#region AdditionalHandlingInformation

		public void TestPopulateAdditionalHandlingInformation()
		{
			var consol = CreateConsol();
			var container = consol.Containers.First();
			var parameters = new DummyDocDataObjectParameters() { Data = container };
			var decl = new MultimodalDangerousGoodsDeclarationBuilder(parameters).Build();

			Assert(nameof(MultimodalDangerousGoodsDeclaration.AdditionalHandlingInformation), decl.AdditionalHandlingInformation.IsEmpty);
		}

		#endregion

		#region	Visibility

		public void TestFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Multimodal Dangerous Goods Form")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, string.Empty);
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals("Precondition: Multimodal DG form exist.", 1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasContainerizedDGs",
				filterCondition);
		}

		#endregion

		#region Implementations

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";
			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var carrierContractNumber = consol.Numbers.AddNew();
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "11111";

			var letterOfCredit = consol.Numbers.AddNew();
			letterOfCredit.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber;
			letterOfCredit.CE_EntryNum = "22222";

			var sldNumber = consol.Numbers.AddNew();
			sldNumber.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			sldNumber.CE_EntryNum = "12345";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "TAIKO";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "HUA TAI HE";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			transport3.JW_Vessel = "FUA KAVENGA";
			transport3.JW_VoyageFlight = "333";

			PopulateAddresses(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";
			packline1.JL_ContainerPackingOrder = 1;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = packline1.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "1234";

			packline1.HarmonisedCodes.Add(hc);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ExportRefNumber = "BBB";
			packline2.JL_PackageCount = 5;
			packline2.JL_ExportRefNumber = "REF001";
			packline2.JL_DetailedDescription = "pack2";
			packline2.JL_ContainerPackingOrder = 2;

			var undg2 = packline2.UNDGs.AddNew();
			undg2.DI_PackageCount = 1;
			undg2.DI_F3_NKPackType = "BOX";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 20000;
			packline3.JL_ActualWeightUQ = "G";
			packline3.JL_ActualVolume = 300;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "EEEEE";
			packline3.JL_ExportRefNumber = "REF002";
			packline3.JL_DetailedDescription = "pack3";
			packline3.JL_ContainerPackingOrder = 3;

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);
			container.PackLines.Add(packline3);

			return consol;
		}

		void PopulateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_FullName = "I'm Handling Stuff";
			handlingAgent.OH_RL_NKClosestPort = "AUSYD";
			handlingAgent.MainAddress.Address1 = "Unit 2";
			handlingAgent.MainAddress.Address2 = "60 What Lane";
			handlingAgent.MainAddress.City = "Sydney";
			handlingAgent.MainAddress.Postcode = "2023";
			handlingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = handlingAgent.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Booking Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Stay in touch";
			notifyParty.OH_RL_NKClosestPort = "AUSYD";
			notifyParty.MainAddress.Address1 = "Unit 205";
			notifyParty.MainAddress.Address2 = "128 Why Lane";
			notifyParty.MainAddress.City = "Sydney";
			notifyParty.MainAddress.Postcode = "2000";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "AU";
			notifyParty.MainAddress.OA_Email = "stayintouch@test.com";

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying About Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "Notifying 3";
			notifyParty3.OH_RL_NKClosestPort = "AUSYD";
			notifyParty3.MainAddress.Address1 = "Unit 2";
			notifyParty3.MainAddress.Address2 = "60 What Kine";
			notifyParty3.MainAddress.City = "Sydney";
			notifyParty3.MainAddress.Postcode = "2029";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "I'm The Money";
			creditor.OH_RL_NKClosestPort = "AUMEL";
			creditor.MainAddress.Address1 = "Cashed up";
			creditor.MainAddress.Address2 = "1 Moolah St";
			creditor.MainAddress.City = "Moneyville";
			creditor.MainAddress.Postcode = "3999";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
		}

		#endregion
	}
}
