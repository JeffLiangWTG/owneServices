using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerManagerFilterStrip))]
	internal class ContainerManagerFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestDepotPort()
		{
			OrgAddress depot1 = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew();
			depot1.Header.OH_RL_NKClosestPort = "AUBNE";
			depot1.OA_RL_NKRelatedPortCode = "";
			depot1.OA_Address1 = "address";
			OrgAddress depot2 = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew();
			depot2.Header.OH_RL_NKClosestPort = "AUBNE";
			depot2.OA_RL_NKRelatedPortCode = "AUSYD";
			depot2.OA_Address1 = "address";
			OrgAddress depot3 = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew();
			depot3.Header.OH_RL_NKClosestPort = "SGSIN";
			depot3.OA_RL_NKRelatedPortCode = "";
			depot3.OA_Address1 = "address";
			ContainerMovement movement1 = Container1.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_MovementDate = ZDateTime.Now;
			movement1.E9_OA_Depot = depot1.PK;
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2.E9_MovementDate = ZDateTime.Now;
			movement2.E9_OA_Depot = depot2.PK;
			ContainerMovement movement3 = Container3.Movements.AddNew();
			movement3.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement3.E9_MovementDate = ZDateTime.Now;
			movement3.E9_OA_Depot = depot3.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("LastMovement.DepotPort");
			Asserter.AddFieldOfInterest("LastMovement.Depot.OA_RL_NKRelatedPortCode");
			Asserter.AddFieldOfInterest("LastMovement.Depot.Header.OH_RL_NKClosestPort");
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.DepotPort];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2, Container3);
			filter.Property = "AU";
			Asserter.AssertMatches("depot1", filter, Container1, Container2);
			filter.Property = "AUBNE";
			Asserter.AssertMatches("depot2", filter, Container1);
			filter.Property = "AUSYD";
			Asserter.AssertMatches("depot3", filter, Container2);
		}

		public void TestOriginAndDestination()
		{
			var shipment1 = Factory.New<AgencyShipment>();
			var shipment2 = Factory.New<AgencyShipment>();
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage1.GenerateSailings();
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage2.GenerateSailings();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_JX = voyage1.Sailings[0].PK;
			shipment2.JS_RL_NKOrigin = "AUMEL";
			shipment2.JS_RL_NKDestination = "HKHKG";
			shipment2.JS_JX = voyage2.Sailings[0].PK;
			ContainerMovement movement1 = Container1.Movements.AddNew();
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement1.E9_JV = voyage1.PK;
			movement2.E9_JV = voyage2.PK;
			var jobContainer1 = Factory.New<AgencyShipmentContainer>();
			jobContainer1.JC_ContainerNum = Container1.R6_ContainerNum;
			jobContainer1.JC_JS_FCLBookingOnlyLink = shipment1.PK;
			var jobContainer2 = Factory.New<AgencyShipmentContainer>();
			jobContainer2.JC_ContainerNum = Container2.R6_ContainerNum;
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.Origin];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "AU";
			Asserter.AssertMatches("AU", filter, Array.Empty<RefContainerStock>());
			filter.Property = "AUSYD";
			Asserter.AssertMatches("AUSYD", filter, Container1);
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Array.Empty<RefContainerStock>());
			filter = (ModuleNkFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.Destination];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "NZ";
			Asserter.AssertMatches("NZ", filter, Array.Empty<RefContainerStock>());
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Container1);
			filter.Property = "HKHKG";
			Asserter.AssertMatches("HKHKG", filter, Array.Empty<RefContainerStock>());
			jobContainer2.JC_JS_FCLBookingOnlyLink = shipment2.PK;
			Factory.Save();
			Asserter.AssertMatches("HKHKG", filter, Container2);
		}

		public void TestLoadPortAndDischargePort()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.GenerateSailings();
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.GenerateSailings();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			ContainerMovement movement1 = Container1.Movements.AddNew();
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement1.E9_JV = voyage1.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement1.E9_MovementDate = DateTime.Now.AddDays(-3);
			movement2.E9_JV = voyage2.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_MovementDate = DateTime.Now.AddDays(-3);
			var jobContainer1 = Factory.New<AgencyShipmentContainer>();
			jobContainer1.JC_ContainerNum = Container1.R6_ContainerNum;
			var jobContainer2 = Factory.New<AgencyShipmentContainer>();
			jobContainer2.JC_ContainerNum = Container2.R6_ContainerNum;
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.LoadPort];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "AU";
			Asserter.AssertMatches("AU", filter, Array.Empty<RefContainerStock>());
			filter.Property = "AUSYD";
			Asserter.AssertMatches("AUSYD", filter, Container1);
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Array.Empty<RefContainerStock>());
			filter = (ModuleNkFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.DischargePort];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "NZ";
			Asserter.AssertMatches("NZ", filter, Array.Empty<RefContainerStock>());
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Container2);
			filter.Property = "HKHKG";
			Asserter.AssertMatches("HKHKG", filter, Array.Empty<RefContainerStock>());
		}

		public void TestLocationCategory()
		{
			ZDateTime now = ZDateTime.Now;
			ContainerMovement movement1a = Container1.Movements.AddNew();
			movement1a.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1a.E9_MovementDate = now.AddDays(-4);
			ContainerMovement movement1b = Container1.Movements.AddNew();
			movement1b.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement1b.E9_MovementDate = now.AddDays(-2);
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_MovementDate = now.AddDays(-3);
			ContainerMovement movement3 = Container3.Movements.AddNew();
			movement3.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement3.E9_MovementDate = now.AddDays(-1);
			AssertNotNull("lazy-load", Container4);
			Factory.Save();
			Asserter.AddFieldOfInterest("LastMovement+ToLocationCategory");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.LocationCategory];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2, Container3, Container4);
			filter.Property = ContainerLocationCategoryList.Codes.OutOfGate;
			Asserter.AssertMatches("Wild", filter, Container1, Container3);
			filter.Property = ContainerLocationCategoryList.Codes.AtWharf;
			asserter.AssertMatches("Wharf", filter, Container2);
			filter.Property = ContainerLocationCategoryList.Codes.AtYard;
			asserter.AssertMatches("Yard", filter);
		}

		public void TestLocationCategory_WharfAwaitingForTranship_ShouldLoadDischargedContainersWhichHaveLoadAtTheSamePort()
		{
			var now = ZDateTime.Now.ToSmallDateTime();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Consignor";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "Consignee";
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "Client";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations[0].JB_AvailabilityDate = now.AddDays(-12);
			voyage.GenerateSailings();
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "V00000100";
			shipment.JS_HouseBill = "BNESYD01000";
			shipment.JS_CFSReference = "BOOKINGNUM";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "CNSHA";
			shipment.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "AUSYD").PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			var header = new JobHeader.Loader(shipment).TryLoadOrCreate();
			header.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = Container1.R6_ContainerNum;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_RL_NKClosestPort = "AUSYD";
			depot.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var seaVoyage = Factory.New<JobVoyage>();
			seaVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			seaVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			seaVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
			seaVoyage.Destinations[0].JB_AvailabilityDate = now.AddDays(-10);
			seaVoyage.GenerateSailings();
			var seaLeg = shipment.TransportsIncludingRelated.AddNew();
			seaLeg.JW_RL_NKLoadPort = "AUSYD";
			seaLeg.JW_RL_NKDiscPort = "CNSHA";
			seaLeg.JW_IsLinked = true;
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			seaLeg.JW_JX = seaVoyage.Sailings[0].PK;
			var movement = Container1.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			movement.E9_MovementType = ContainerMovementTypes.Codes.Discharge;
			movement.E9_OA_Depot = depot.MainAddress.PK;
			movement.E9_MovementDate = now.AddDays(-3);
			var movement2 = Container2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_MovementDate = now.AddDays(-3);
			var movement3 = Container3.Movements.AddNew();
			movement3.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement3.E9_MovementDate = now.AddDays(-3);
			Factory.Save();
			Asserter.AddFieldOfInterest("LastMovement+ToLocationCategory");
			var filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.LocationCategory];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2, Container3);
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			var lastMovement = Container1.Movements.AddNew();
			lastMovement.E9_JV = voyage.PK;
			lastMovement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			lastMovement.E9_OA_Depot = depot.MainAddress.PK;
			lastMovement.E9_MovementDate = now.AddDays(-2);
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter);
			lastMovement = Container1.Movements.AddNew();
			lastMovement.E9_JV = voyage.PK;
			lastMovement.E9_MovementType = ContainerMovementTypes.Codes.Discharge;
			lastMovement.E9_OA_Depot = depot.MainAddress.PK;
			lastMovement.E9_MovementDate = now.AddDays(-1);
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			seaLeg.JW_RL_NKLoadPort = "AUMEL";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter);
			seaLeg.JW_RL_NKLoadPort = "AUSYD";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter);
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			depot.OH_RL_NKClosestPort = "";
			depot.MainAddress.OA_RL_NKRelatedPortCode = "";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter);
			depot.MainAddress.OA_RL_NKRelatedPortCode = "";
			depot.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			depot.OH_RL_NKClosestPort = "";
			depot.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter, Container1);
			depot.OH_RL_NKClosestPort = "AUSYD";
			depot.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			Factory.Save();
			filter.Property = ContainerLocationCategoryList.Codes.AtWharfAwaitingTranshipment;
			Asserter.AssertMatches("AtWharfAwaitingTranshipment", filter);
		}

		public void TestMovementFilterProcessor()
		{
			var today = ZDate.Today;
			var depot1 = Factory.NewWithValidTestData<OrgHeader>();
			var depot2 = Factory.NewWithValidTestData<OrgHeader>();
			var depot3 = Factory.NewWithValidTestData<OrgHeader>();
			var depot4 = Factory.NewWithValidTestData<OrgHeader>();
			var movement1 = Container1.Movements.AddNew();
			movement1.E9_MovementDate = today;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement1.E9_OA_Depot = depot1.MainAddress.PK;
			var movement2 = Container1.Movements.AddNew();
			movement2.E9_MovementDate = today.AddDays(2);
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement2.E9_OA_Depot = depot2.MainAddress.PK;
			var movement3 = Container1.Movements.AddNew();
			movement3.E9_MovementDate = today.AddDays(2);
			movement3.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement3.E9_OA_Depot = depot3.MainAddress.PK;
			var movement4 = Container1.Movements.AddNew();
			movement4.E9_MovementDate = today.AddDays(2);
			movement4.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement4.E9_OA_Depot = depot4.MainAddress.PK;
			var movement5 = Container2.Movements.AddNew();
			movement5.E9_MovementDate = today;
			movement5.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement5.E9_OA_Depot = depot1.MainAddress.PK;
			Factory.Save();
			var movementDateFilter = (ModuleDateFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementDate];
			var movementFlagsFilter = (ModuleFlagsFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementFlags];
			var movementTypeFilter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementType];
			var depotFilter = (ModuleGuidFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.Depot];
			movementDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			movementDateFilter.Property1 = today;
			movementDateFilter.Property2 = ZDateTime.Empty;
			movementDateFilter.IsActive = true;
			movementFlagsFilter[ContainerManagerFilterStrip.Descriptions.MovementFlags_LastMovement] = true;
			movementFlagsFilter.IsActive = true;
			movementTypeFilter.Property = ContainerMovementTypes.Codes.WharfGateOut;
			movementTypeFilter.IsActive = true;
			depotFilter.Property = depot1.PK;
			depotFilter.IsActive = true;
			var results = new RefContainerStockCollection(Factory).Find(FilterStrip.Filter);
			AssertCollectionNotContains(Container1, results);
			AssertCollectionContains(Container2, results);
		}

		public void TestShipmentFilterProcessor()
		{
			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V0000999";
			bill1.JS_HouseBill = "BILL1";
			var bill2 = Factory.New<BillOfLading>();
			bill2.JS_UniqueConsignRef = "V0000899";
			bill2.JS_HouseBill = "BILL2";
			var bill3 = Factory.New<BillOfLading>();
			bill3.JS_UniqueConsignRef = "V0000898";
			bill3.JS_HouseBill = "BILL1";
			var jobContainer1 = bill1.RealContainers.AddNew();
			jobContainer1.JC_ContainerNum = Container1.R6_ContainerNum;
			var jobContainer2 = bill2.RealContainers.AddNew();
			jobContainer2.JC_ContainerNum = Container1.R6_ContainerNum;
			var jobContainer3 = bill3.RealContainers.AddNew();
			jobContainer3.JC_ContainerNum = Container2.R6_ContainerNum;
			Factory.Save();
			var billOfLadingFilter = (ModuleNumberFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.BillOfLading];
			var shipmentNumberFilter = (ModuleFountainFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ShipmentNumber];
			billOfLadingFilter.Property = "BILL1";
			billOfLadingFilter.IsActive = true;
			shipmentNumberFilter.Property = "V00008";
			shipmentNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			shipmentNumberFilter.IsActive = true;
			var results = new RefContainerStockCollection(Factory).Find(FilterStrip.Filter);
			AssertCollectionNotContains(Container1, results);
			AssertCollectionContains(Container2, results);
		}

		public void TestBillOfLading()
		{
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_HouseBill = "BILL1";
			BillOfLadingContainer jobContainer1 = bill1.RealContainers.AddNew();
			jobContainer1.JC_ContainerNum = Container1.R6_ContainerNum;
			BillOfLading bill2 = Factory.New<BillOfLading>();
			bill2.JS_HouseBill = "BILL2";
			BillOfLadingContainer jobContainer2 = bill2.RealContainers.AddNew();
			jobContainer2.JC_ContainerNum = Container2.R6_ContainerNum;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.BillOfLading];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "BILL1";
			Asserter.AssertMatches("BILL1", filter, Container1);
		}

		public void TestContainerNumber()
		{
			Container1.R6_ContainerNum = "FAKU4100011";
			Container2.R6_ContainerNum = "FAKU4100027";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "FAKU4100011";
			Asserter.AssertMatches("FAKU4100011", filter, Container1);
		}

		public void TestShipmentNumber()
		{
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V00000998";
			BillOfLadingContainer jobContainer1 = bill1.RealContainers.AddNew();
			jobContainer1.JC_ContainerNum = Container1.R6_ContainerNum;
			BillOfLading bill2 = Factory.New<BillOfLading>();
			bill2.JS_UniqueConsignRef = "V00000999";
			BillOfLadingContainer jobContainer2 = bill2.RealContainers.AddNew();
			jobContainer2.JC_ContainerNum = Container2.R6_ContainerNum;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ShipmentNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = "V00000998";
			Asserter.AssertMatches("BILL1", filter, Container1);
		}

		public void TestMovementDate()
		{
			ZDateTime today = ZDateTime.Today;
			ContainerMovement movement1 = Container1.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_MovementDate = today.AddDays(-9);
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2.E9_MovementDate = today.AddDays(-8);
			ContainerMovement movement3 = Container3.Movements.AddNew();
			movement3.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement3.E9_MovementDate = today.AddDays(-7);
			ContainerMovement movement4 = Container4.Movements.AddNew();
			movement4.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement4.E9_MovementDate = today.AddDays(-6);
			ContainerMovement movement5 = Container5.Movements.AddNew();
			movement5.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement5.E9_MovementDate = today.AddDays(-8).AddHours(10);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2, Container3, Container4, container5);
			filter.Property1 = today.AddDays(-8);
			Asserter.AssertMatches("-8 <= date", filter, Container2, Container3, Container4, Container5);
			filter.Property2 = today.AddDays(-7);
			Asserter.AssertMatches("-8 <= date <= -8", filter, Container2, Container3, Container5);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("date <= -8", filter, Container1, Container2, Container3, Container5);
			filter.Property1 = today.AddDays(-8);
			filter.Property2 = today.AddDays(-8);
			asserter.AssertMatches("date ~= -8", filter, Container2, Container5);
		}

		public void TestMovementType()
		{
			ContainerMovement movement1 = Container1.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementType];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = ContainerMovementTypes.Codes.WharfGateOut;
			Asserter.AssertMatches("YardGateIn", filter, Container1);
		}

		public void TestContainerCondition()
		{
			string code1 = AgencyRegistry.Instance.ContainerDamageCodes.Value[0].Code;
			string code2 = AgencyRegistry.Instance.ContainerDamageCodes.Value[1].Code;
			Container1.Movements.AddNew().E9_ContainerCondition = code1;
			Container2.Movements.AddNew().E9_ContainerCondition = code2;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ContainerCondition];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = code1;
			Asserter.AssertMatches("code1", filter, Container1);
		}

		public void TestContainerQuality()
		{
			string code1 = AgencyRegistry.Instance.ContainerCleanCodes.Value[0].Code;
			string code2 = AgencyRegistry.Instance.ContainerCleanCodes.Value[1].Code;
			Container1.Movements.AddNew().E9_ContainerQuality = code1;
			Container2.Movements.AddNew().E9_ContainerQuality = code2;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ContainerQuality];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = code1;
			Asserter.AssertMatches("code1", filter, Container1);
		}

		public void TestContainerEmpty()
		{
			ContainerMovement movement1 = Container1.Movements.AddNew();
			movement1.E9_ContainerIsEmpty = true;
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement2.E9_ContainerIsEmpty = false;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovedAsEmpty];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = ContainerManagerFilterStrip.MovedAsEmptyFilter.IsEmpty;
			Asserter.AssertMatches("Only Empty", filter, Container1);
			filter.Property = ContainerManagerFilterStrip.MovedAsEmptyFilter.IsNotEmpty;
			Asserter.AssertMatches("Only Non Empty", filter, Container2);
		}

		public void TestLastMovement()
		{
			ZDateTime today = ZDateTime.Today;
			ContainerMovement movement1A = Container1.Movements.AddNew();
			movement1A.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement1A.E9_MovementDate = today.AddDays(-10);
			ContainerMovement movement1B = Container1.Movements.AddNew();
			movement1B.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1B.E9_MovementDate = today.AddDays(-9);
			ContainerMovement movement2A = Container2.Movements.AddNew();
			movement2A.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2A.E9_MovementDate = today.AddDays(-9);
			ContainerMovement movement2B = Container2.Movements.AddNew();
			movement2B.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement2B.E9_MovementDate = today.AddDays(-8);
			Factory.Save();
			ZQuery yardGateIn = new ZQuery(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.Codes.YardGateIn);
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.MovementFlags];
			filter[ContainerManagerFilterStrip.Descriptions.MovementFlags_LastMovement] = false;
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			Asserter.AssertMatches("false", subGroup.GetSubQuery(new ZQuery(filter.Query, yardGateIn)), Container1, Container2);
			filter[ContainerManagerFilterStrip.Descriptions.MovementFlags_LastMovement] = true;
			Asserter.AssertMatches("true", subGroup.GetSubQuery(new ZQuery(filter.Query, yardGateIn)), Container1);
		}

		public void TestOwner()
		{
			Container1.R6_OH_Owner = Owner1.PK;
			Container2.R6_OH_Owner = Owner2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.Owner];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = Owner1.PK;
			Asserter.AssertMatches("Owner1", filter, Container1);
		}

		public void TestOwnerType()
		{
			Container1.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			Container2.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.OwnerType];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			Asserter.AssertMatches(Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned, filter.Query, Container1);
		}

		public void TestContainerType()
		{
			Container1.R6_RC = ContainerType1.PK;
			Container2.R6_RC = ContainerType2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.ContainerType];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = ContainerType1.PK;
			Asserter.AssertMatches(ContainerType1.RC_Code, filter, Container1);
		}

		public void TestISOType()
		{
			Container1.R6_RC = ContainerType1.PK;
			Container2.R6_RC = ContainerType2.PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.IsoType];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.Property = ContainerType1.RC_ISOType;
			Asserter.AssertMatches(ContainerType1.RC_ISOType, filter, Container1);
		}

		public void TestDepot()
		{
			OrgHeader depot1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader depot2 = Factory.NewWithValidTestData<OrgHeader>();
			Container1.Movements.AddNew().E9_OA_Depot = depot1.MainAddress.PK;
			Container2.Movements.AddNew().E9_OA_Depot = depot2.MainAddress.PK;
			Container3.Movements.AddNew().E9_OA_Depot = ZGuid.Empty;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.Depot];
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2, Container3);
			filter.Property = depot1.PK;
			Asserter.AssertMatches("depot1", filter, Container1);
		}

		public void TestVoyageVesselFilter()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel2test";
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "Voyage1";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "Voyage2";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage2.GenerateSailings();
			ContainerMovement movement1 = Container1.Movements.AddNew();
			ContainerMovement movement2 = Container2.Movements.AddNew();
			movement1.E9_JV = voyage1.PK;
			movement2.E9_JV = voyage2.PK;
			Factory.Save();
			var filter = (VoyageVesselModuleFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1, Container2);
			filter.VoyageFlightNo = "Voyage1";
			Asserter.AssertMatches("Voyage1", filter, Container1);
			filter.VoyageFlightNo = "Voyage2";
			Asserter.AssertMatches("Voyage2", filter, Container2);
			filter.VoyageFlightNo = "";
			filter.Vessel = "Vessel2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Starts with filter", filter, Container2);
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank Filter", filter, Container1, Container2);
		}

		public void TestLastLeaseContractNo()
		{
			var today = ZDateTime.Today;
			var movement1A = Container1.Movements.AddNew();
			movement1A.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement1A.E9_MovementDate = today.AddDays(-10);
			movement1A.E9_LeaseNumber = "CONTRACT1A";
			var movement1B = Container1.Movements.AddNew();
			movement1B.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1B.E9_MovementDate = today.AddDays(-9);
			movement1B.E9_LeaseNumber = "APPLE";
			var movement2A = Container2.Movements.AddNew();
			movement2A.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2A.E9_MovementDate = today.AddDays(-9);
			movement2A.E9_LeaseNumber = "BOOK2A";
			var movement2B = Container2.Movements.AddNew();
			movement2B.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement2B.E9_MovementDate = today.AddDays(-8);
			movement2B.E9_LeaseNumber = "ORANGE";
			Factory.Save();
			var filter = (ModuleNumberFilter)FilterStrip[ContainerManagerFilterStrip.Descriptions.LastLeaseContractNo];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = string.Empty;
			Asserter.AssertMatches("Two containers should be matched.", filter, Container1, Container2);
			filter.Property = "A";
			Asserter.AssertMatches("Container1 should be matched.", filter, Container1);
			filter.Property = "C";
			Asserter.AssertMatches("No container should be matched.", filter);
			filter.Property = "O";
			Asserter.AssertMatches("Container2 should be matched.", filter, Container2);
			filter.Property = "B";
			Asserter.AssertMatches("No container should be matched.", filter);
			filter.Property = "X";
			Asserter.AssertMatches("No container should be matched.", filter);
		}

		#region Implementation
		FilterStripAsserter<RefContainerStock> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<RefContainerStock>(Factory, (c) => c.R6_ContainerNum));
			}
		}

		FilterStripAsserter<RefContainerStock> asserter;
		RefContainerStock Container1
		{
			get
			{
				if (container1 == null)
				{
					container1 = Factory.New<RefContainerStock>();
					container1.R6_ContainerNum = "FAKU4100011";
					container1.R6_RC = ContainerType1.PK;
					Asserter.AddToScope(container1);
				}

				return container1;
			}
		}

		RefContainerStock container1;
		RefContainerStock Container2
		{
			get
			{
				if (container2 == null)
				{
					container2 = Factory.New<RefContainerStock>();
					container2.R6_ContainerNum = "FAKU4100027";
					container2.R6_RC = ContainerType2.PK;
					Asserter.AddToScope(container2);
				}

				return container2;
			}
		}

		RefContainerStock container2;
		RefContainerStock Container3
		{
			get
			{
				if (container3 == null)
				{
					container3 = Factory.New<RefContainerStock>();
					container3.R6_ContainerNum = "FAKU4100032";
					container3.R6_RC = ContainerType3.PK;
					Asserter.AddToScope(container3);
				}

				return container3;
			}
		}

		RefContainerStock container3;
		RefContainerStock Container4
		{
			get
			{
				if (container4 == null)
				{
					container4 = Factory.New<RefContainerStock>();
					container4.R6_ContainerNum = "FAKU4100048";
					container4.R6_RC = ContainerType4.PK;
					Asserter.AddToScope(container4);
				}

				return container4;
			}
		}

		RefContainerStock container4;
		RefContainerStock Container5
		{
			get
			{
				if (container5 == null)
				{
					container5 = Factory.New<RefContainerStock>();
					container5.R6_ContainerNum = "FAKU5100053";
					container5.R6_RC = ContainerType5.PK;
					Asserter.AddToScope(container5);
				}

				return container5;
			}
		}

		RefContainerStock container5;
		OrgHeader Owner1
		{
			get
			{
				return owner1 ?? (owner1 = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader owner1;
		OrgHeader Owner2
		{
			get
			{
				return owner2 ?? (owner2 = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader owner2;
		RefContainer ContainerType1
		{
			get
			{
				return containerType1 ?? (containerType1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP"));
			}
		}

		RefContainer containerType1;
		RefContainer ContainerType2
		{
			get
			{
				return containerType2 ?? (containerType2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP"));
			}
		}

		RefContainer containerType2;
		RefContainer ContainerType3
		{
			get
			{
				return containerType3 ?? (containerType3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE"));
			}
		}

		RefContainer containerType3;
		RefContainer ContainerType4
		{
			get
			{
				return containerType4 ?? (containerType4 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE"));
			}
		}

		RefContainer containerType4;
		RefContainer ContainerType5
		{
			get
			{
				return containerType5 ?? (containerType5 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20OT"));
			}
		}

		RefContainer containerType5;
		ContainerManagerFilterStrip FilterStrip
		{
			get
			{
				return filterStrip ?? (filterStrip = new ContainerManagerFilterStrip());
			}
		}

		ContainerManagerFilterStrip filterStrip;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerManagerFilterStrip();
		}
		#endregion
	}
}
