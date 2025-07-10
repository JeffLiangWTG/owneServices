using System;
using System.Collections.Generic;
using System.Linq;
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
	[TestedType(typeof(ContainerMoveFilterStrip))]
	internal class ContinerMoveFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestDetentionJobNumberFilter()
		{
			Detention1.NC_JobNumber = "DI1";
			Detention1.NC_GC = GlbCompany.CurrentCompany.PK;
			Detention2.NC_JobNumber = "DI2";
			Detention2.NC_GC = OtherCompany.PK;
			Movement1A.E9_NC = Detention1.PK;
			Movement1B.E9_NC = Detention2.PK;
			Movement1C.E9_NC = ZGuid.Empty;
			Factory.Save();
			Asserter.AddFieldOfInterest("Detention.NC_JobNumber");
			Asserter.AddFieldOfInterest("Detention.Company.GC_Code");
			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DetentionJobNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B, Movement1C);
			filter.Property = "DI1";
			Asserter.AssertMatches("D1", filter, Movement1A);
			filter.Property = "DI2";
			Asserter.AssertMatches("D2", filter, Movement1B);
		}

		public void TestDetentionCompany()
		{
			Detention1.NC_JobNumber = "DI1";
			Detention1.NC_GC = GlbCompany.CurrentCompany.PK;
			Detention2.NC_JobNumber = "DI2";
			Detention2.NC_GC = OtherCompany.PK;
			Movement1A.E9_NC = Detention1.PK;
			Movement1B.E9_NC = Detention2.PK;
			Movement1C.E9_NC = ZGuid.Empty;
			Factory.Save();
			Asserter.AddFieldOfInterest("Detention.NC_JobNumber");
			Asserter.AddFieldOfInterest("Detention.Company.GC_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DetentionCompany];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B, Movement1C);
			filter.Property = GlbCompany.CurrentCompany.PK;
			Asserter.AssertMatches("D1", filter, Movement1A);
		}

		public void TestDepotPort()
		{
			Depot1.OH_RL_NKClosestPort = "AUBNE";
			Depot2.OH_RL_NKClosestPort = "AUSYD";
			Depot3.OH_RL_NKClosestPort = "NLAMS";
			OrgAddress address1A = Depot1.Addresses.AddNew();
			address1A.OA_Address1 = "address1A";
			address1A.OA_RL_NKRelatedPortCode = "";
			OrgAddress address1B = Depot1.Addresses.AddNew();
			address1B.OA_Address1 = "address1B";
			address1B.OA_RL_NKRelatedPortCode = "AUSYD";
			OrgAddress address2A = Depot2.Addresses.AddNew();
			address2A.OA_Address1 = "address2A";
			address2A.OA_RL_NKRelatedPortCode = "";
			OrgAddress address2B = Depot2.Addresses.AddNew();
			address2B.OA_Address1 = "address2B";
			address2B.OA_RL_NKRelatedPortCode = "AUBNE";
			OrgAddress address3A = Depot3.Addresses.AddNew();
			address3A.OA_Address1 = "address3A";
			address3A.OA_RL_NKRelatedPortCode = "";
			OrgAddress address3B = Depot3.Addresses.AddNew();
			address3B.OA_Address1 = "address3A";
			address3B.OA_RL_NKRelatedPortCode = "GBLON";
			Movement1A.E9_OA_Depot = address1A.PK;
			Movement1B.E9_OA_Depot = address1B.PK;
			Movement2A.E9_OA_Depot = address2A.PK;
			Movement2B.E9_OA_Depot = address2B.PK;
			Movement1C.E9_OA_Depot = address3A.PK;
			Movement2C.E9_OA_Depot = address3B.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Depot.OA_RL_NKRelatedPortCode");
			Asserter.AddFieldOfInterest("Depot.Header.OH_RL_NKClosestPort");
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DepotPort];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B, Movement2A, Movement2B, Movement1C, Movement2C);
			filter.Property = "AUBNE";
			Asserter.AssertMatches("AUBNE", filter, Movement1A, Movement2B);
			filter.Property = "AUSYD";
			Asserter.AssertMatches("AUSYD", filter, Movement1B, Movement2A);
			filter.Property = "AU";
			Asserter.AssertMatches("AU", filter, Movement1A, Movement1B, Movement2A, Movement2B);
			filter.Property = "NL";
			Asserter.AssertMatches("NL", filter, Movement1C);
			filter.Property = "GB";
			Asserter.AssertMatches("GB", filter, Movement2C);
		}

		public void TestOriginAndDestination()
		{
			var shipment1 = Factory.New<AgencyShipment>();
			var shipment2 = Factory.New<AgencyShipment>();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_JX = Voyage1.Sailings[0].PK;
			shipment2.JS_RL_NKOrigin = "AUMEL";
			shipment2.JS_RL_NKDestination = "HKHKG";
			shipment2.JS_JX = Voyage2.Sailings[0].PK;
			Movement1A.E9_JV = Voyage1.PK;
			Movement2A.E9_JV = Voyage2.PK;
			var jobContainer1 = Factory.New<AgencyShipmentContainer>();
			jobContainer1.JC_ContainerNum = Stock1.R6_ContainerNum;
			jobContainer1.JC_JS_FCLBookingOnlyLink = shipment1.PK;
			var jobContainer2 = Factory.New<AgencyShipmentContainer>();
			jobContainer2.JC_ContainerNum = Stock2.R6_ContainerNum;
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Origin];
			Asserter.AssertMatches("Empty Filter", filter, Movement1A, Movement2A);
			filter.Property = "AU";
			Asserter.AssertMatches("AU", filter, Array.Empty<ContainerMovement>());
			filter.Property = "AUSYD";
			Asserter.AssertMatches("AUSYD", filter, Movement1A);
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Array.Empty<ContainerMovement>());
			filter = (ModuleNkFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Destination];
			Asserter.AssertMatches("Empty Filter", filter, Movement1A, Movement2A);
			filter.Property = "NZ";
			Asserter.AssertMatches("NZ", filter, Array.Empty<ContainerMovement>());
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Movement1A);
			filter.Property = "HKHKG";
			Asserter.AssertMatches("HKHKG", filter, Array.Empty<ContainerMovement>());
			jobContainer2.JC_JS_FCLBookingOnlyLink = shipment2.PK;
			Factory.Save();
			Asserter.AssertMatches("HKHKG", filter, Movement2A);
		}

		public void TestLoadPortAndDischargePort()
		{
			Voyage1.Origins[0].JA_RL_NKPortOfLoading = "AUSYD";
			Voyage1.Destinations[0].JB_RL_NKPortOfDischarge = "CNSHA";
			Voyage2.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";
			Voyage2.Destinations[0].JB_RL_NKPortOfDischarge = "NZAKL";
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.LoadPort];
			Asserter.AssertMatches("Empty Filter", filter, Movement1A, Movement1B);
			filter.Property = "AU";
			Asserter.AssertMatches("AU", filter, Array.Empty<ContainerMovement>());
			filter.Property = "AUSYD";
			Asserter.AssertMatches("AUSYD", filter, Movement1A);
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Array.Empty<ContainerMovement>());
			filter = (ModuleNkFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DischargePort];
			Asserter.AssertMatches("Empty Filter", filter, Movement1A, Movement1B);
			filter.Property = "NZ";
			Asserter.AssertMatches("NZ", filter, Array.Empty<ContainerMovement>());
			filter.Property = "NZAKL";
			Asserter.AssertMatches("NZAKL", filter, Movement1B);
			filter.Property = "HKHKG";
			Asserter.AssertMatches("HKHKG", filter, Array.Empty<ContainerMovement>());
		}

		public void TestOwner()
		{
			Movement1A.Stock.R6_OH_Owner = Owner1.PK;
			Movement2A.Stock.R6_OH_Owner = Owner2.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Depot.Header.OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Owner];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement2A);
			filter.Property = owner1.PK;
			Asserter.AssertMatches("Empty", filter, Movement1A);
		}

		public void TestOwnerType()
		{
			Movement1A.Stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			Movement2A.Stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			Asserter.AddFieldOfInterest("Stock.R6_OwnerType");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.OwnerType];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement2A);
			filter.Property = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			Asserter.AssertMatches("Empty", filter, Movement1A);
		}

		public void TestContainerCondition()
		{
			Movement1A.E9_ContainerCondition = DefaultContainerDamageList.Codes.Available;
			Movement1B.E9_ContainerCondition = DefaultContainerDamageList.Codes.MediumDamage;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_ContainerCondition");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.ContainerCondition];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B);
			filter.Property = DefaultContainerDamageList.Codes.Available;
			Asserter.AssertMatches("Empty", filter, Movement1A);
		}

		public void TestContainerQuality()
		{
			Movement1A.E9_ContainerQuality = DefaultContainerCleanList.Codes.Hay;
			Movement1B.E9_ContainerQuality = DefaultContainerCleanList.Codes.Milk;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_ContainerQuality");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.ContainerQuality];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B);
			filter.Property = DefaultContainerCleanList.Codes.Hay;
			Asserter.AssertMatches("Empty", filter, Movement1A);
		}

		public void TestContainerType()
		{
			Movement1A.Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Movement2A.Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Stock.Container.RC_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.ContainerType];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement2A);
			filter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Asserter.AssertMatches("20GP", filter, Movement1A);
		}

		public void TestIsoType()
		{
			Movement1A.Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Movement2A.Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Stock.Container.RC_Code");
			Asserter.AddFieldOfInterest("Stock.Container.RC_ISOType");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.IsoType];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement2A);
			filter.Property = "22G0";
			Asserter.AssertMatches("22G0", filter, Movement1A);
		}

		public void TestMovedAsEmpty()
		{
			Movement1A.E9_ContainerIsEmpty = true;
			Movement1B.E9_ContainerIsEmpty = false;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_ContainerIsEmpty");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.MovedAsEmpty];
			filter.Property = "";
			Asserter.AssertMatches("Not Specified", filter, Movement1A, Movement1B);
			filter.Property = ContainerMoveFilterStrip.MovedAsEmptyFilter.IsEmpty;
			Asserter.AssertMatches("Only Empty", filter, Movement1A);
			filter.Property = ContainerMoveFilterStrip.MovedAsEmptyFilter.IsNotEmpty;
			Asserter.AssertMatches("Only Non-Empty", filter, Movement1B);
		}

		public void TestVoyageVessel()
		{
			Voyage1.JV_VoyageFlight = "001S";
			Voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Voyage2.JV_VoyageFlight = "001N";
			Voyage2.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			Voyage3.JV_VoyageFlight = "";
			Voyage3.JV_RV_NKVessel = "";
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Movement1C.E9_JV = ZGuid.Empty;
			Movement2A.E9_JV = Voyage3.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("Voyage.JV_RV_NKVessel");
			Asserter.AddFieldOfInterest("Voyage.JV_VoyageFlight");
			var filter = (VoyageVesselModuleFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty", filter, Movement1A, Movement1B, Movement1C, Movement2A);
			filter.VoyageFlightNo = "001S";
			filter.Vessel = "";
			Asserter.AssertMatches("Voyage", filter, Movement1A);
			filter.VoyageFlightNo = "";
			filter.Vessel = "BANOWATI";
			Asserter.AssertMatches("Vessel", filter, Movement1B);
			filter.VoyageFlightNo = "";
			filter.Vessel = "BANO";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Vessel Starts with filter", filter, Movement1B);
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank filter", filter, Movement1A, Movement1B);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank filter", filter, Movement2A);
		}

		public void TestGetDetentionableTypeFilter()
		{
			Movement1A.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Movement1B.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			Movement1C.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			Movement2A.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			Movement2B.E9_MovementType = ContainerMovementTypes.Codes.ReShipRequested;
			Movement2C.E9_MovementType = ContainerMovementTypes.Codes.ReturnedUnshipped;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_MovementType");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DetentionableType];
			filter.Property = "";
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement1C, Movement2A, Movement2B, Movement2C);
			filter.Property = DetentionInvoiceType.Codes.Export;
			Asserter.AssertMatches("export", filter, Movement1A, Movement2C);
			filter.Property = DetentionInvoiceType.Codes.Import;
			asserter.AssertMatches("import", filter, Movement1C, Movement2B);
		}

		public void TestContainerNumberFilter()
		{
			Stock1.R6_ContainerNum = "TEST4100013";
			Stock2.R6_ContainerNum = "TEST4100027";
			AssertNotNull(Movement1A);
			AssertNotNull(Movement2A);
			Factory.Save();
			Asserter.AddFieldOfInterest("Stock.R6_ContainerNum");
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "";
			Asserter.AssertMatches("empty", filter, Movement1A, Movement2A);
			filter.Property = "TEST4100013";
			Asserter.AssertMatches("TEST4100013", filter, Movement1A);
		}

		public void TestShipmentNumberFilter()
		{
			Shipment1.JS_JX = Voyage1.Sailings[0].PK;
			Shipment1.JS_UniqueConsignRef = "V00000101";
			Shipment2.JS_JX = Voyage1.Sailings[0].PK;
			Shipment2.JS_UniqueConsignRef = "V00000102";
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock2.R6_ContainerNum;
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Movement1C.E9_JV = ZGuid.Empty;
			Movement2A.E9_JV = Voyage1.PK;
			Movement2B.E9_JV = Voyage2.PK;
			Movement2C.E9_JV = ZGuid.Empty;
			Factory.Save();
			Asserter.AddFieldOfInterest("RelatedInfo.ShipmentNumbers");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.ShipmentNumber];
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement1C, Movement2A, Movement2B, Movement2C);
			filter.Property = "V00000101";
			Asserter.AssertMatches("shipment1", filter, Movement1A);
		}

		public void TestBillOfLadingFilter()
		{
			Shipment1.JS_JX = Voyage1.Sailings[0].PK;
			Shipment1.JS_HouseBill = "bill1";
			Shipment2.JS_JX = Voyage1.Sailings[0].PK;
			Shipment2.JS_HouseBill = "bill2";
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock2.R6_ContainerNum;
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Movement1C.E9_JV = ZGuid.Empty;
			Movement2A.E9_JV = Voyage1.PK;
			Movement2B.E9_JV = Voyage2.PK;
			Movement2C.E9_JV = ZGuid.Empty;
			Factory.Save();
			Asserter.AddFieldOfInterest("RelatedInfo.BillsOfLading");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.BillOfLading];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement1C, Movement2A, Movement2B, movement2C);
			filter.Property = "bill1";
			Asserter.AssertMatches("bill11", filter, Movement1A);
		}

		public void TestMovementTypeFilter()
		{
			Movement1A.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			Movement1B.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_MovementType");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.MovementType];
			filter.Property = "";
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B);
			filter.Property = ContainerMovementTypes.Codes.YardGateIn;
			Asserter.AssertMatches("YGI", filter, Movement1A);
		}

		public void TestMovementLeaseContractNoFilter()
		{
			Movement1A.E9_LeaseNumber = "CONTRACT1";
			Movement1B.E9_LeaseNumber = "CONTRACT2";
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_LeaseNumber");
			var filter = (ModuleNumberFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.LeaseContractNo];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = string.Empty;
			Asserter.AssertMatches("Two movements should be matched.", filter, Movement1A, Movement1B);
			filter.Property = "CONTRACT1";
			Asserter.AssertMatches("CONTRACT1 should be matched.", filter, Movement1A);
			filter.Property = "CONTRACT2";
			Asserter.AssertMatches("CONTRACT2 should be matched.", filter, Movement1B);
			filter.Property = "C";
			Asserter.AssertMatches("CONTRACT2 should be matched.", filter, Movement1A, Movement1B);
			filter.Property = "D";
			Asserter.AssertMatches("No movement should be matched.", filter);
		}

		public void TestMovementDateFilter()
		{
			ZDateTime now = ZDateTime.Now;
			Movement1A.E9_MovementDate = now.AddDays(1);
			Movement1B.E9_MovementDate = now.AddDays(2);
			Movement2A.E9_MovementDate = now.AddDays(3);
			Movement2B.E9_MovementDate = now.AddDays(4);
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_MovementDate");
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.MovementDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement2A, Movement2B);
			filter.Property1 = now.AddDays(2);
			Asserter.AssertMatches("lower bound", filter, Movement1B, Movement2A, Movement2B);
			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("lower and upper bound", filter, Movement1B, Movement2A);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("upper bound", filter, Movement1A, Movement1B, Movement2A);
		}

		#region CreatedTimeFilter
		[TestDate]
		public void TestCreatedTimeFilter_SearchOptionIsHasDateEntered_ReturnAllMovements()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, DateTime.Now)).ToArray();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			// Testing
			Asserter.AssertMatches("All movements are returned", filter, movements);
		}

		[TestDate]
		public void TestCreatedTimeFilter_SearchOptionIsHasNoDateEntered_ReturnEmptyListOfMovements()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, DateTime.Now)).ToArray();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			// Testing
			Asserter.AssertMatches("0 movements were returned", filter);
		}

		[TestDate]
		public void TestCreatedTimeFilter_FromAndToDatesAreEmpty_ReturnAllMovements()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, DateTime.Now)).ToArray();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			// Testing
			Asserter.AssertMatches("All movements are returned", filter, movements);
		}

		[TestDate]
		public void TestCreatedTimeFilter_FromDateIsEmpty_ReturnAllMovementsTillToDate()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var startDate = new DateTime(2012, 6, 6);
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, startDate.AddDays(i))).ToList();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = movements.ElementAt(testMovementsCount / 2).CreatedTimeLocal;
			// Testing
			var expectedMovements = movements.Where(m => m.CreatedTimeLocal <= filter.Property2).ToArray();
			Asserter.AssertMatches("Expected movements are returned", filter, expectedMovements);
		}

		[TestDate]
		public void TestCreatedTimeFilter_ToDateIsEmpty_ReturnAllMovementsFromFromDate()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var startDate = new DateTime(2012, 6, 6);
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, startDate.AddDays(i))).ToList();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = movements.ElementAt(testMovementsCount / 2).CreatedTimeLocal;
			filter.Property2 = ZDateTime.Empty;
			// Testing
			var expectedMovements = movements.Where(m => m.CreatedTimeLocal >= filter.Property1).ToArray();
			Asserter.AssertMatches("Expected movements are returned", filter, expectedMovements);
		}

		[TestDate]
		public void TestCreatedTimeFilter_ToDateAndFromDatesAreValid_ReturnAllMovementsFromTheRange()
		{
			// Configuring test movements
			const int testMovementsCount = 5;
			var stock = CreateContainerStock();
			var startDate = new DateTime(2012, 6, 6);
			var movements = Enumerable.Range(0, testMovementsCount).Select(i => AddContainerMovement(stock, startDate.AddDays(i))).ToList();
			// Configuring the filter
			var filter = (ModuleDateFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = movements.First().CreatedTimeLocal;
			filter.Property2 = movements.ElementAt(testMovementsCount / 2).CreatedTimeLocal;
			// Testing
			var expectedMovements = movements.Where(m => m.CreatedTimeLocal >= filter.Property1 && m.CreatedTimeLocal <= filter.Property2).ToArray();
			Asserter.AssertMatches("Expected movements are returned", filter, expectedMovements);
		}

		#endregion
		public void TestDepotFilter()
		{
			OrgHeader depot1 = Factory.NewWithValidTestData<OrgHeader>();
			depot1.OH_Code = "Depot1";
			OrgHeader depot2 = Factory.NewWithValidTestData<OrgHeader>();
			depot2.OH_Code = "Depot2";
			Movement1A.E9_OA_Depot = depot1.MainAddress.PK;
			Movement1B.E9_OA_Depot = depot2.MainAddress.PK;
			Movement2A.E9_OA_Depot = ZGuid.Empty;
			Factory.Save();
			Asserter.AddFieldOfInterest("Depot.Header.OH_Code");
			Asserter.AddFieldOfInterest("Depot.OA_Address1");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Depot];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement2A);
			filter.Property = depot1.PK;
			Asserter.AssertMatches("depot1", filter, Movement1A);
		}

		public void TestClientFilter()
		{
			Shipment1.JS_JX = Voyage1.Sailings[0].PK;
			Shipment2.JS_JX = Voyage1.Sailings[0].PK;
			Shipment3.JS_JX = Voyage2.Sailings[0].PK;
			Job1.JH_OA_LocalChargesAddr = Client1.MainAddress.PK;
			Job2.JH_OA_LocalChargesAddr = Client2.MainAddress.PK;
			Job3.JH_OA_LocalChargesAddr = Client1.MainAddress.PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container3A.JC_ContainerNum = Stock2.R6_ContainerNum;
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Movement1C.E9_JV = ZGuid.Empty;
			Movement2A.E9_JV = Voyage1.PK;
			Movement2B.E9_JV = Voyage2.PK;
			Movement2C.E9_JV = ZGuid.Empty;
			Movement2B.E9_OH_ResponsibleParty = client2.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("RelatedInfo.ShipmentNumbers");
			Asserter.AddFieldOfInterest("ResponsibleParty.OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Client];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement1C, Movement2A, Movement2B, Movement2C);
			filter.Property = Client1.PK;
			Asserter.AssertMatches("client1", filter, Movement1A);
			filter.Property = Client2.PK;
			Asserter.AssertMatches("client2", filter, Movement2A, Movement2B);
		}

		public void TestPrincipalFilter()
		{
			Shipment1.JS_JX = Voyage1.Sailings[0].PK;
			Shipment1.JS_OH_DeliveryAgent = Principal1.PK;
			Shipment2.JS_JX = Voyage1.Sailings[0].PK;
			Shipment2.JS_OH_DeliveryAgent = Principal2.PK;
			Shipment3.JS_JX = Voyage2.Sailings[0].PK;
			Shipment3.JS_OH_DeliveryAgent = Principal1.PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container3A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Movement1A.E9_JV = Voyage1.PK;
			Movement1B.E9_JV = Voyage2.PK;
			Movement1C.E9_JV = ZGuid.Empty;
			Movement2A.E9_JV = Voyage1.PK;
			Movement2B.E9_JV = Voyage2.PK;
			Movement2C.E9_JV = ZGuid.Empty;
			Movement1B.E9_OH_Principal = Principal2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Principal];
			Asserter.AddFieldOfInterest("RelatedInfo.ShipmentNumbers");
			Asserter.AddFieldOfInterest("Principal.OH_Code");
			Asserter.AddFieldOfInterest("RelatedInfo.Principal");
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B, Movement1C, Movement2A, Movement2B, Movement2C);
			filter.Property = Principal1.PK;
			Asserter.AssertMatches("principal1", filter, Movement1A);
			filter.Property = Principal2.PK;
			asserter.AssertMatches("principal2", filter, Movement1B, movement2A);
		}

		public void TestDetentionInvoicedFilter()
		{
			Movement1A.E9_NC = Detention1.PK;
			Movement1B.E9_NC = ZGuid.Empty;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.DetentionInvoiced];
			filter.Property = "";
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B);
			filter.Property = ContainerMoveFilterStrip.InvoicedList.Invoiced;
			Asserter.AssertMatches("invoiced", filter, Movement1A);
			filter.Property = ContainerMoveFilterStrip.InvoicedList.NotInvoiced;
			Asserter.AssertMatches("not invoiced", filter, Movement1B);
		}

		public void TestDetentionableFilter()
		{
			Movement1A.E9_DetentionDays = 0;
			Movement1B.E9_DetentionDays = 1;
			Factory.Save();
			Asserter.AddFieldOfInterest("E9_DetentionDays");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerMoveFilterStrip.Descriptions.Detentionable];
			filter.Property = "";
			Asserter.AssertMatches("empty", filter, Movement1A, Movement1B);
			filter.Property = ContainerMoveFilterStrip.DetentionableList.Detentionable;
			Asserter.AssertMatches("detentionable", filter, Movement1B);
			filter.Property = ContainerMoveFilterStrip.DetentionableList.NotDetentionable;
			Asserter.AssertMatches("not detentionable", filter, Movement1A);
		}

		#region Implementation
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter("OrgAddress", "Depot Port"));
			result.Add(TableFilter("OrgHeader", "Depot Port"));
			result.Add(TableFilter("JobContainer", "Principal"));
			result.Add(TableFilter("JobContainerMove", "Principal"));
			result.Add(TableFilter("JobSailing", "Principal"));
			result.Add(TableFilter("JobShipment", "Principal"));
			result.Add(TableFilter("JobVoyOrigin", "Principal"));
			result.Add(TableFilter("RefContainer", "Principal"));
			result.Add(TableFilter("RefContainerStock", "Principal"));
			result.Add(TableFilter("JobHeader", "Client"));
			result.Add(TableFilter("JobSailing", "Client"));
			result.Add(TableFilter("JobContainer", "Client"));
			result.Add(TableFilter("JobContainerMove", "Client"));
			result.Add(TableFilter("JobShipment", "Client"));
			result.Add(TableFilter("JobVoyOrigin", "Client"));
			result.Add(TableFilter("OrgAddress", "Client"));
			result.Add(TableFilter("RefContainer", "Client"));
			result.Add(TableFilter("RefContainerStock", "Client"));
			result.Add(TableFilter("RefContainerStock", "Shipment #"));
			result.Add(TableFilter("RefContainerStock", "Container #"));
			result.Add(TableFilter("RefContainerStock", "Container Type"));
			result.Add(TableFilter("RefContainerStock", "ISO Type"));
			result.Add(TableFilter("RefContainerStock", "Bill Of Lading"));
			result.Add(TableFilter("RefContainerStock", "Owner"));
			result.Add(TableFilter("RefContainerStock", "OwnerType"));
			return result;
		}

		public FilterStripAsserter<ContainerMovement> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<ContainerMovement>(Factory, (m) => m.E9_OtherLocation));
			}
		}

		FilterStripAsserter<ContainerMovement> asserter;
		ContainerMoveFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new ContainerMoveFilterStrip();
				}

				return filterStrip;
			}
		}

		ContainerMoveFilterStrip filterStrip;
		GlbCompany OtherCompany
		{
			get
			{
				if (otherCompany == null)
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
					otherCompany = Factory.LoadTop1<GlbCompany>(filter);
				}

				return otherCompany;
			}
		}

		GlbCompany otherCompany;
		RefContainerStock Stock1
		{
			get
			{
				if (stock1 == null)
				{
					stock1 = Factory.New<RefContainerStock>();
					stock1.R6_ContainerNum = "FAKE4100013";
					stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock1;
			}
		}

		RefContainerStock stock1;
		RefContainerStock Stock2
		{
			get
			{
				if (stock2 == null)
				{
					stock2 = Factory.New<RefContainerStock>();
					stock2.R6_ContainerNum = "FAKE4100029";
					stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock2;
			}
		}

		RefContainerStock stock2;
		ContainerMovement Movement1A
		{
			get
			{
				if (movement1A == null)
				{
					movement1A = Stock1.Movements.AddNew();
					movement1A.E9_OtherLocation = "Movement1A";
					Asserter.AddToScope(movement1A);
				}

				return movement1A;
			}
		}

		ContainerMovement movement1A;
		ContainerMovement Movement1B
		{
			get
			{
				if (movement1B == null)
				{
					movement1B = Stock1.Movements.AddNew();
					movement1B.E9_OtherLocation = "Movement1B";
					Asserter.AddToScope(movement1B);
				}

				return movement1B;
			}
		}

		ContainerMovement movement1B;
		ContainerMovement Movement1C
		{
			get
			{
				if (movement1C == null)
				{
					movement1C = Stock1.Movements.AddNew();
					movement1C.E9_OtherLocation = "Movement1C";
					Asserter.AddToScope(movement1C);
				}

				return movement1C;
			}
		}

		ContainerMovement movement1C;
		ContainerMovement Movement2A
		{
			get
			{
				if (movement2A == null)
				{
					movement2A = Stock2.Movements.AddNew();
					movement2A.E9_OtherLocation = "Movement2A";
					Asserter.AddToScope(movement2A);
				}

				return movement2A;
			}
		}

		ContainerMovement movement2A;
		ContainerMovement Movement2B
		{
			get
			{
				if (movement2B == null)
				{
					movement2B = Stock2.Movements.AddNew();
					movement2B.E9_OtherLocation = "Movement2B";
					Asserter.AddToScope(movement2B);
				}

				return movement2B;
			}
		}

		ContainerMovement movement2B;
		ContainerMovement Movement2C
		{
			get
			{
				if (movement2C == null)
				{
					movement2C = Stock2.Movements.AddNew();
					movement2C.E9_OtherLocation = "Movement2C";
					Asserter.AddToScope(movement2C);
				}

				return movement2C;
			}
		}

		ContainerMovement movement2C;
		OrgHeader Principal1
		{
			get
			{
				if (principal1 == null)
				{
					principal1 = Factory.NewWithValidTestData<OrgHeader>();
					principal1.OH_Code = "Principal1";
				}

				return principal1;
			}
		}

		OrgHeader principal1;
		OrgHeader Principal2
		{
			get
			{
				if (principal2 == null)
				{
					principal2 = Factory.NewWithValidTestData<OrgHeader>();
					principal2.OH_Code = "Principal2";
				}

				return principal2;
			}
		}

		OrgHeader principal2;
		OrgHeader Depot1
		{
			get
			{
				if (depot1 == null)
				{
					depot1 = Factory.NewWithValidTestData<OrgHeader>();
					depot1.OH_Code = "Depot1";
				}

				return depot1;
			}
		}

		OrgHeader depot1;
		OrgHeader Depot2
		{
			get
			{
				if (depot2 == null)
				{
					depot2 = Factory.NewWithValidTestData<OrgHeader>();
					depot2.OH_Code = "Depot2";
				}

				return depot2;
			}
		}

		OrgHeader depot2;
		OrgHeader Depot3
		{
			get
			{
				if (depot3 == null)
				{
					depot3 = Factory.NewWithValidTestData<OrgHeader>();
					depot3.OH_Code = "Depot3";
				}

				return depot3;
			}
		}

		OrgHeader depot3;
		OrgHeader Owner1
		{
			get
			{
				if (owner1 == null)
				{
					owner1 = Factory.NewWithValidTestData<OrgHeader>();
					owner1.OH_Code = "Depot1";
				}

				return owner1;
			}
		}

		OrgHeader owner1;
		OrgHeader Owner2
		{
			get
			{
				if (owner2 == null)
				{
					owner2 = Factory.NewWithValidTestData<OrgHeader>();
					owner2.OH_Code = "Owner2";
				}

				return owner2;
			}
		}

		OrgHeader owner2;
		OrgHeader Client1
		{
			get
			{
				if (client1 == null)
				{
					client1 = Factory.NewWithValidTestData<OrgHeader>();
					client1.OH_Code = "Client1";
				}

				return client1;
			}
		}

		OrgHeader client1;
		OrgHeader Client2
		{
			get
			{
				if (client2 == null)
				{
					client2 = Factory.NewWithValidTestData<OrgHeader>();
					client2.OH_Code = "Client2";
				}

				return client2;
			}
		}

		OrgHeader client2;
		ContainerDetention Detention1
		{
			get
			{
				if (detention1 == null)
				{
					detention1 = Factory.New<ContainerDetention>();
					detention1.NC_OH_Client = Client1.PK;
					detention1.NC_OH_Principal = Principal1.PK;
				}

				return detention1;
			}
		}

		ContainerDetention detention1;
		ContainerDetention Detention2
		{
			get
			{
				if (detention2 == null)
				{
					detention2 = Factory.New<ContainerDetention>();
					detention2.NC_OH_Client = Client2.PK;
					detention2.NC_OH_Principal = Principal2.PK;
				}

				return detention2;
			}
		}

		ContainerDetention detention2;
		JobVoyage Voyage1
		{
			get
			{
				if (voyage1 == null)
				{
					voyage1 = Factory.New<JobVoyage>();
					voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
					voyage1.GenerateSailings();
				}

				return voyage1;
			}
		}

		JobVoyage voyage1;
		JobVoyage Voyage2
		{
			get
			{
				if (voyage2 == null)
				{
					voyage2 = Factory.New<JobVoyage>();
					voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
					voyage2.GenerateSailings();
				}

				return voyage2;
			}
		}

		JobVoyage voyage2;
		JobVoyage Voyage3
		{
			get
			{
				if (voyage3 == null)
				{
					voyage3 = Factory.New<JobVoyage>();
					voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
					voyage3.GenerateSailings();
				}

				return voyage3;
			}
		}

		JobVoyage voyage3;
		BillOfLading Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<BillOfLading>();
					shipment1.JS_UniqueConsignRef = "Shipment1";
				}

				return shipment1;
			}
		}

		BillOfLading shipment1;
		BillOfLading Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<BillOfLading>();
					shipment2.JS_UniqueConsignRef = "Shipment2";
				}

				return shipment2;
			}
		}

		BillOfLading shipment2;
		BillOfLading Shipment3
		{
			get
			{
				if (shipment3 == null)
				{
					shipment3 = Factory.New<BillOfLading>();
					shipment3.JS_UniqueConsignRef = "Shipment3";
				}

				return shipment3;
			}
		}

		BillOfLading shipment3;
		JobHeader Job1
		{
			get
			{
				return job1 ?? (job1 = new JobHeader.Loader(Shipment1).TryLoadOrCreate());
			}
		}

		JobHeader job1;
		JobHeader Job2
		{
			get
			{
				return job2 ?? (job2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate());
			}
		}

		JobHeader job2;
		JobHeader Job3
		{
			get
			{
				return job3 ?? (job3 = new JobHeader.Loader(Shipment3).TryLoadOrCreate());
			}
		}

		JobHeader job3;
		BillOfLadingContainer Container1A
		{
			get
			{
				return container1A ?? (container1A = Shipment1.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container1A;
		BillOfLadingContainer Container2A
		{
			get
			{
				return container2A ?? (container2A = Shipment2.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container2A;
		BillOfLadingContainer Container3A
		{
			get
			{
				return container3A ?? (container3A = Shipment3.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container3A;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerMoveFilterStrip();
		}

		RefContainerStock CreateContainerStock(string containerNumber = "FAKE1234569", string containerType = "20GP")
		{
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = containerNumber;
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			return stock;
		}

		ContainerMovement AddContainerMovement(RefContainerStock containerStock, DateTime timestamp)
		{
			TestDateAttribute.Date = timestamp;
			var movement = containerStock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			Asserter.AddToScope(movement);
			Factory.Save();
			return movement;
		}
		#endregion
	}
}
