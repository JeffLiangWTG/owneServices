using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(AllocationRouteFilterBusinessObject))]
	public class AllocationRouteFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAllocationIDFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_AllocationLineID = "1234";
			allocation2.RCA_AllocationLineID = "2345";
			allocation3.RCA_AllocationLineID = "3456";
			allocation4.RCA_AllocationLineID = "4567";

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var allocationIDFilter = (ModuleTextFilter)filter["Allocation Route ID"];
			allocationIDFilter.Property = "12";
			allocationIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			allocationIDFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Route Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1 }, collection);

			allocationIDFilter.Property = "23";
			allocationIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2 }, collection);
		}

		public void TestAllocationIDFilterExclusivity()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_AllocationLineID = "1234";
			allocation1.RCA_AllocatedQuantity = 1;
			allocation2.RCA_AllocationLineID = "2345";
			allocation2.RCA_AllocatedQuantity = 10;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();

			var allocatedQuantityFilter = (ModuleNumberRangeFilter)filter["Allocated Quantity"];
			allocatedQuantityFilter.Property1 = 6;
			allocatedQuantityFilter.Property2 = 16;
			allocatedQuantityFilter.IsActive = true;

			var allocationIDFilter = (ModuleTextFilter)filter["Allocation Route ID"];
			allocationIDFilter.Property = "12";
			allocationIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			allocationIDFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Contract Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1 }, collection);
		}

		public void TestStartDateFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_StartDate = new ZDate(2021, 1, 27);
			allocation2.RCA_StartDate = new ZDate(2021, 4, 27);
			allocation3.RCA_StartDate = new ZDate(2021, 6, 27);
			allocation4.RCA_StartDate = new ZDate(2021, 10, 27);

			allocation1.RCA_ExpiryDate = new ZDate(2021, 2, 27);
			allocation2.RCA_ExpiryDate = new ZDate(2021, 5, 27);
			allocation3.RCA_ExpiryDate = new ZDate(2021, 7, 27);
			allocation4.RCA_ExpiryDate = new ZDate(2021, 11, 27);

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var startDateFilter = (ModuleDateFilter)filter["Start Date"];
			startDateFilter.Property1 = new ZDateTime(2021, 2, 27);
			startDateFilter.Property2 = new ZDateTime(2021, 7, 27);
			startDateFilter.IsActive = true;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3 }, collection);

			startDateFilter.Property1 = new ZDateTime(2021, 1, 10);
			startDateFilter.Property2 = new ZDateTime(2021, 11, 27);

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("4 Routes Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4 }, collection);
		}

		public void TestExpiryDateFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_StartDate = new ZDate(2021, 1, 15);
			allocation2.RCA_StartDate = new ZDate(2021, 4, 15);
			allocation3.RCA_StartDate = new ZDate(2021, 6, 15);
			allocation4.RCA_StartDate = new ZDate(2021, 10, 15);

			allocation1.RCA_ExpiryDate = new ZDate(2021, 1, 27);
			allocation2.RCA_ExpiryDate = new ZDate(2021, 4, 27);
			allocation3.RCA_ExpiryDate = new ZDate(2021, 6, 27);
			allocation4.RCA_ExpiryDate = new ZDate(2021, 10, 27);

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var expiryDateFilter = (ModuleDateFilter)filter["Expiry Date"];
			expiryDateFilter.Property1 = new ZDateTime(2021, 2, 27);
			expiryDateFilter.Property2 = new ZDateTime(2021, 7, 27);
			expiryDateFilter.IsActive = true;
			expiryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3 }, collection);

			expiryDateFilter.Property1 = new ZDateTime(2021, 1, 10);
			expiryDateFilter.Property2 = new ZDateTime(2021, 11, 27);

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("4 Routes Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4 }, collection);
		}

		public void TestContainerWeightLimitFilterOnlyRange()
		{
			using (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
				var allocation1 = dataCreator.CreateAllocation(4, "AVT", "g");
				var allocation2 = dataCreator.CreateAllocation(2, "AVT", "g");
				var allocation3 = dataCreator.CreateAllocation(3.2, "AVT", "g");
				var allocation4 = dataCreator.CreateAllocation(1, "AVT", "g");

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var hasContainerWeightLimitFilter = (AllocationContainerWeightLimitWithTypeFilter)filter
					.First(moduleFilter => moduleFilter is AllocationContainerWeightLimitWithTypeFilter);

				hasContainerWeightLimitFilter.Property1 = 2;
				hasContainerWeightLimitFilter.Property2 = 4;
				hasContainerWeightLimitFilter.Property = "g";
				hasContainerWeightLimitFilter.LimitType = "AVT";
				hasContainerWeightLimitFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory) { AdditionalFilter = filter.Filter };

				AssertEquals("3 Routes Loaded", 3, collection.Count);
				AssertContainsExactElementsInAnyOrder(
					[allocation1, allocation2, allocation3], collection);

				hasContainerWeightLimitFilter.Property1 = 0;
				hasContainerWeightLimitFilter.Property2 = 1;

				collection = new RatingContractAllocationLineCollection(Factory) { AdditionalFilter = filter.Filter };

				AssertEquals("1 Route Loaded", 1, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation4], collection);
			}
		}

		public void TestContainerWeightLimitFilterUnitsIntegrated()
		{
			using (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
				var allocation1 = dataCreator.CreateAllocation(4, "ABT", "g");
				var allocation2 = dataCreator.CreateAllocation(2, "ABT", "g");
				var allocation3 = dataCreator.CreateAllocation(3.2, "AVT", "g");
				var allocation4 = dataCreator.CreateAllocation(1, "AVT", "g");

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var hasContainerWeightLimitFilter = (AllocationContainerWeightLimitWithTypeFilter)filter
					.First(moduleFilter => moduleFilter is AllocationContainerWeightLimitWithTypeFilter);

				hasContainerWeightLimitFilter.Property1 = 0;
				hasContainerWeightLimitFilter.Property2 = 5;
				hasContainerWeightLimitFilter.LimitType = "ABT";
				hasContainerWeightLimitFilter.Property = "g";
				hasContainerWeightLimitFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory) { AdditionalFilter = filter.Filter };

				AssertEquals("2 Routes Loaded", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation1, allocation2], collection);
			}
		}

		public void TestContainerWeightLimitFilterUnitsAndTypeIntegrated_WithConversion()
		{
			using (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
				var allocation1 = dataCreator.CreateAllocation(4, "ABT", "KG");
				var allocation2 = dataCreator.CreateAllocation(2, "AVT", "G");
				var allocation3 = dataCreator.CreateAllocation(3.2, "ABT", "G");
				var allocation4 = dataCreator.CreateAllocation(1, "ABT", "G");

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var hasContainerWeightLimitFilter = (AllocationContainerWeightLimitWithTypeFilter)filter
					.First(moduleFilter => moduleFilter is AllocationContainerWeightLimitWithTypeFilter);

				hasContainerWeightLimitFilter.Property1 = 0;
				hasContainerWeightLimitFilter.Property2 = 5;
				hasContainerWeightLimitFilter.LimitType = "ABT";
				hasContainerWeightLimitFilter.Property = "KG";
				hasContainerWeightLimitFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory) { AdditionalFilter = filter.Filter };

				AssertEquals("All but one route Loaded", 3, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation1, allocation3, allocation4], collection);
			}
		}

		public void TestContainerWeightLimitFilterUnitsAndTypeIntegrated_WithoutConversion()
		{
			using (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
				var allocation1 = dataCreator.CreateAllocation(4, "ABT", "KG");
				var allocation2 = dataCreator.CreateAllocation(2, "ABT", "G");
				var allocation3 = dataCreator.CreateAllocation(3.2, "ABT", "G");
				var allocation4 = dataCreator.CreateAllocation(1, "ABT", "G");

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var hasContainerWeightLimitFilter = (AllocationContainerWeightLimitWithTypeFilter)filter
					.First(moduleFilter => moduleFilter is AllocationContainerWeightLimitWithTypeFilter);

				hasContainerWeightLimitFilter.Property1 = 0;
				hasContainerWeightLimitFilter.Property2 = 2;
				hasContainerWeightLimitFilter.LimitType = "ABT";
				hasContainerWeightLimitFilter.Property = "G";
				hasContainerWeightLimitFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory) { AdditionalFilter = filter.Filter };

				AssertEquals("2 Routes Loaded", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation2, allocation4], collection);
			}
		}

		public void TestTradeLaneFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var tradeLane1 = Factory.NewWithValidTestData<JobTradeLane>();
			tradeLane1.EJ_Code = "MACE";
			allocation1.RCA_EJ_TradeLane = tradeLane1.PK;
			var tradeLane2 = Factory.NewWithValidTestData<JobTradeLane>();
			tradeLane2.EJ_Code = "WHOA";
			allocation2.RCA_EJ_TradeLane = tradeLane2.PK;
			var tradeLane3 = Factory.NewWithValidTestData<JobTradeLane>();
			tradeLane3.EJ_Code = "LMAO";
			allocation3.RCA_EJ_TradeLane = tradeLane3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var hasTradeLaneFilter = (ModuleGuidFilter)filter["Trade Lane"];

			hasTradeLaneFilter.Property = tradeLane1.PK;
			hasTradeLaneFilter.IsActive = true;
			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 route loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder([allocation1], collection);

			hasTradeLaneFilter.Property = Guid.Empty;
			hasTradeLaneFilter.IsActive = true;
			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("All routes loaded", 3, collection.Count);
		}

		public void TestBookingLimitFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_HasBookingLimit = true;
			allocation2.RCA_HasBookingLimit = true;
			allocation3.RCA_HasBookingLimit = false;
			allocation4.RCA_HasBookingLimit = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var hasBookingLimitFilter = (ModuleFlagsFilter)filter["Has Booking Limit"];
			hasBookingLimitFilter.Property0 = true;
			hasBookingLimitFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2 }, collection);

			hasBookingLimitFilter.Property0 = false;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation3, allocation4 }, collection);
		}

		public void TestGatewayConsolFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_AllowGatewayConsolOnly = true;
			allocation2.RCA_AllowGatewayConsolOnly = true;
			allocation3.RCA_AllowGatewayConsolOnly = false;
			allocation4.RCA_AllowGatewayConsolOnly = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var gatewayConsolFilter = (ModuleFlagsFilter)filter["Gateway Consol"];
			gatewayConsolFilter.Property0 = true;
			gatewayConsolFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder([allocation1, allocation2], collection);

			gatewayConsolFilter.Property0 = false;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder([allocation3, allocation4], collection);
		}

		public void TestContainerOwnerFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_ContainerOwner = "SHP";
			allocation2.RCA_ContainerOwner = "SHP";
			allocation3.RCA_ContainerOwner = "CAR";
			allocation4.RCA_ContainerOwner = "CAR";

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var containerOwnerFilter = (ModuleTextFilter)filter["Shipper Owned Container"];
			containerOwnerFilter.Property = "SHP";
			containerOwnerFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder([allocation1, allocation2], collection);

			containerOwnerFilter.Property = "CAR";

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder([allocation3, allocation4], collection);
		}

		public void TestGroupageContainerModeFilter()
		{
			var groupageAllocation = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var nonGroupageAllocation = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			groupageAllocation.RCA_AllowGroupageOnly = true;
			nonGroupageAllocation.RCA_AllowGroupageOnly = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var gatewayConsolFilter = (ModuleFlagsFilter)filter["Groupage Container Mode"];
			gatewayConsolFilter.Property0 = true;
			gatewayConsolFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Route Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder([groupageAllocation], collection);

			gatewayConsolFilter.Property0 = false;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Route Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder([nonGroupageAllocation], collection);
		}

		public void TestPriorityFilter()
		{
			using (FreightConfigurationRegistry.Instance.EnablePrioritySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

				allocation1.RCA_Priority = 5;
				allocation2.RCA_Priority = 10;
				allocation3.RCA_Priority = 15;
				allocation4.RCA_Priority = 20;

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var priorityFilter = (ModuleNumberRangeFilter)filter["Priority"];
				priorityFilter.Property1 = 6;
				priorityFilter.Property2 = 16;
				priorityFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("2 Routes Loaded", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation2, allocation3], collection);

				priorityFilter.Property1 = 5;
				priorityFilter.Property2 = 25;

				collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("4 Routes Loaded", 4, collection.Count);
				AssertContainsExactElementsInAnyOrder([allocation1, allocation2, allocation3, allocation4], collection);
			}
		}

		public void TestFreightSpotRateFilter()
		{
			using (FreightConfigurationRegistry.Instance.EnableFreightSpotRateOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var freightSpotRate = Factory.NewWithValidTestData<RatingContractAllocationLine>();
				var nonFreightSpotRate = Factory.NewWithValidTestData<RatingContractAllocationLine>();

				freightSpotRate.RCA_AllowFreightSpotRate = true;
				nonFreightSpotRate.RCA_AllowFreightSpotRate = false;

				Factory.Save();

				var filter = new AllocationRouteFilterBusinessObject();
				var freightSpotRateFilter = (ModuleFlagsFilter)filter["Allow Freight Spot Rate"];
				freightSpotRateFilter.Property0 = true;
				freightSpotRateFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("1 Route Loaded", 1, collection.Count);
				AssertContainsExactElementsInAnyOrder([freightSpotRate], collection);

				freightSpotRateFilter.Property0 = false;

				collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("1 Route Loaded", 1, collection.Count);
				AssertContainsExactElementsInAnyOrder([nonFreightSpotRate], collection);
			}
		}

		public void TestBookingVarianceFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_BookingVariance = -10;
			allocation2.RCA_BookingVariance = -5;
			allocation3.RCA_BookingVariance = 5;
			allocation4.RCA_BookingVariance = 10;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var bookingVarianceFilter = (ModuleNumberRangeFilter)filter["Booking Variance"];
			bookingVarianceFilter.Property1 = -8;
			bookingVarianceFilter.Property2 = 8;
			bookingVarianceFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3 }, collection);

			bookingVarianceFilter.Property2 = 12;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("3 Routes Loaded", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3, allocation4 }, collection);
		}

		public void TestAllocatedQuantityFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_AllocatedQuantity = 5;
			allocation2.RCA_AllocatedQuantity = 10;
			allocation3.RCA_AllocatedQuantity = 15;
			allocation4.RCA_AllocatedQuantity = 20;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var allocatedQuantityFilter = (ModuleNumberRangeFilter)filter["Allocated Quantity"];
			allocatedQuantityFilter.Property1 = 6;
			allocatedQuantityFilter.Property2 = 16;
			allocatedQuantityFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("2 Routes Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3 }, collection);

			allocatedQuantityFilter.Property2 = 22;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("3 Routes Loaded", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3, allocation4 }, collection);
		}

		public void TestLocationFilters()
		{
			var query = new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUSR");
			var zone = Factory.LoadTop1<RefZoneHeader>(query);
			zone.FZ_ZoneType = "CON";

			query = new ZQuery(RefZoneHeaderSchema.FZ_Code, "NZDR");
			zone = Factory.LoadTop1<RefZoneHeader>(query);
			zone.FZ_ZoneType = "CON";

			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation5 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_LoadLocation = "AUSYD";
			allocation2.RCA_LoadLocation = "AUSR";
			allocation3.RCA_LoadLocation = "AU";
			allocation4.RCA_LoadLocation = "AUSYD";
			allocation5.RCA_LoadLocation = "AUBNE";

			allocation1.RCA_DischargeLocation = "NZAKL";
			allocation2.RCA_DischargeLocation = "NZDR";
			allocation3.RCA_DischargeLocation = "NZ";
			allocation4.RCA_DischargeLocation = "NZWLG";
			allocation5.RCA_DischargeLocation = "NZWLG";

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var locationFilter = (ModuleLocationFilter)filter["Load / Discharge"];
			locationFilter.Property1 = string.Empty;
			locationFilter.Property2 = string.Empty;
			locationFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("5 Routes Loaded", 5, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4, allocation5 }, collection);

			locationFilter.Property1 = "AUSYD";

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("4 Routes Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4 }, collection);

			locationFilter.Property2 = "NZAKL";

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("3 Routes Loaded", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3 }, collection);

			locationFilter.Property1 = "AU";

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Route Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation3 }, collection);
		}

		public void TestLocationFilters_PlaceOfReceiptAndDelivery()
		{
			var query = new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUSR");
			var zone = Factory.LoadTop1<RefZoneHeader>(query);
			zone.FZ_ZoneType = "CON";

			query = new ZQuery(RefZoneHeaderSchema.FZ_Code, "NZDR");
			zone = Factory.LoadTop1<RefZoneHeader>(query);
			zone.FZ_ZoneType = "CON";

			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation5 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_PlaceOfReceipt = "AUSYD";
			allocation2.RCA_PlaceOfReceipt = "AUSR";
			allocation3.RCA_PlaceOfReceipt = "AU";
			allocation4.RCA_PlaceOfReceipt = "AUSYD";
			allocation5.RCA_PlaceOfReceipt = "AUBNE";

			allocation1.RCA_PlaceOfDelivery = "NZAKL";
			allocation2.RCA_PlaceOfDelivery = "NZDR";
			allocation3.RCA_PlaceOfDelivery = "NZ";
			allocation4.RCA_PlaceOfDelivery = "NZWLG";
			allocation5.RCA_PlaceOfDelivery = "NZWLG";

			Factory.Save();
			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filter = new AllocationRouteFilterBusinessObject();
				var locationFilter = (ModuleLocationFilter)filter["Receipt / Delivery"];
				locationFilter.Property1 = string.Empty;
				locationFilter.Property2 = string.Empty;
				locationFilter.IsActive = true;

				var collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("5 Routes Loaded", 5, collection.Count);
				AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4, allocation5 }, collection);

				locationFilter.Property1 = "AUSYD";

				collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("4 Routes Loaded", 4, collection.Count);
				AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3, allocation4 }, collection);

				locationFilter.Property2 = "NZAKL";

				collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("3 Routes Loaded", 3, collection.Count);
				AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1, allocation2, allocation3 }, collection);

				locationFilter.Property1 = "AU";

				collection = new RatingContractAllocationLineCollection(Factory);
				collection.AdditionalFilter = filter.Filter;

				AssertEquals("1 Route Loaded", 1, collection.Count);
				AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation3 }, collection);
			}
		}

		public void TestServiceStringFilter()
		{
			var allocation1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocation1.RCA_ServiceLoop = "";
			allocation2.RCA_ServiceLoop = "AL2";
			allocation3.RCA_ServiceLoop = "AL3";
			allocation4.RCA_ServiceLoop = "AL4";

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var serviceStringFilter = (ModuleTextFilter)filter["Service String"];
			serviceStringFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			serviceStringFilter.Property = "";
			serviceStringFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Routes Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation1 }, collection);

			serviceStringFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			serviceStringFilter.Property = "AL";
			serviceStringFilter.IsActive = true;

			collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("3 Routes Loaded", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocation2, allocation3, allocation4 }, collection);
		}

		public void TestNamedAccountsFilter_AllMatch()
		{
			var (allocations, filter) = SetupNamedAccountsClientFilter(ModuleTextFilter.ComparisonConstants.AllMatch);
			var collection = new RatingContractAllocationLineCollection(Factory);
			var (allocationMatchingAnyAndAll, vacuouslyTrue1, vacuouslyTrue2) = (allocations[0], allocations[2], allocations[3]);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocationMatchingAnyAndAll, vacuouslyTrue1, vacuouslyTrue2 }, collection);
		}

		public void TestNamedAccountsFilter_AnyMatch()
		{
			var (allocations, filter) = SetupNamedAccountsClientFilter(ModuleTextFilter.ComparisonConstants.AnyMatch);
			var collection = new RatingContractAllocationLineCollection(Factory);
			var (allocationMatchingAnyAndAll, allocationMatchingAny) = (allocations[0], allocations[1]);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new RatingContractAllocationLine[] { allocationMatchingAnyAndAll, allocationMatchingAny }, collection);
		}

		(RatingContractAllocationLine[], AllocationRouteFilterBusinessObject) SetupNamedAccountsClientFilter(string locationFilterComparisonOperator)
		{
			var allocationMatchingAnyAndAll = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationMatchingAny = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocationMatchingAnyAndAll.RCA_LoadLocation = "AUSYD";
			allocationMatchingAny.RCA_LoadLocation = "AUSYD";
			allocation3.RCA_LoadLocation = "AUBNE";
			allocation4.RCA_LoadLocation = "AUBNE";

			allocationMatchingAnyAndAll.RCA_DischargeLocation = "NZAKL";
			allocationMatchingAny.RCA_DischargeLocation = "NZWLG";
			allocation3.RCA_DischargeLocation = "NZAKL";
			allocation4.RCA_DischargeLocation = "NZWLG";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			org2.OH_Code = "BBB";

			void AddNamedAccount(RatingContractAllocationLine route, OrgHeader org)
			{
				var pivot = route.NamedAccountPivots.AddNew() as RatingContractNamedAccountPivot;
				pivot.RNP_OH_NamedAccount = org.PK;
			}

			AddNamedAccount(allocationMatchingAnyAndAll, org1);
			AddNamedAccount(allocationMatchingAny, org1);
			AddNamedAccount(allocationMatchingAny, org2);

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var namedAccountsFilter = (NamedAccountsOfContractFilter)filter[AllocationRouteFilterConstants.NamedAccountClients];
			namedAccountsFilter.ComparisonOperator = locationFilterComparisonOperator;
			namedAccountsFilter.IsActive = true;

			// orgFilter: OrganisationFilterBusinessObject from MasterFiles.module
			var orgFilter = namedAccountsFilter.SelectedFilters;
			ModuleTextFilter orgCodeFilter = (ModuleTextFilter)orgFilter["Code"];
			orgCodeFilter.Property = "AAA";
			orgCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			orgCodeFilter.IsActive = true;

			return (new[] { allocationMatchingAnyAndAll, allocationMatchingAny, allocation3, allocation4 }, filter);
		}

		public void TestAgentsFilter_AllMatch()
		{
			var (allocations, filter) = SetupAgentsFilter(ModuleTextFilter.ComparisonConstants.AllMatch);
			var collection = new RatingContractAllocationLineCollection(Factory);
			var (allocationMatchingAnyAndAll, allocationRoute3, allocationRoute4) = (allocations[0], allocations[2], allocations[3]);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder([allocationMatchingAnyAndAll, allocationRoute3, allocationRoute4], collection);
		}

		public void TestAgentsFilter_AnyMatch()
		{
			var (allocations, filter) = SetupAgentsFilter(ModuleTextFilter.ComparisonConstants.AnyMatch);
			var collection = new RatingContractAllocationLineCollection(Factory);
			var (allocationMatchingAnyAndAll, allocationMatchingAny) = (allocations[0], allocations[1]);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder([allocationMatchingAnyAndAll, allocationMatchingAny], collection);
		}

		(RatingContractAllocationLine[], AllocationRouteFilterBusinessObject) SetupAgentsFilter(string locationFilterComparisonOperator)
		{
			var allocationMatchingAnyAndAll = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationMatchingAny = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocation4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			allocationMatchingAnyAndAll.RCA_LoadLocation = "AUSYD";
			allocationMatchingAny.RCA_LoadLocation = "AUSYD";
			allocation3.RCA_LoadLocation = "AUBNE";
			allocation4.RCA_LoadLocation = "AUBNE";

			allocationMatchingAnyAndAll.RCA_DischargeLocation = "NZAKL";
			allocationMatchingAny.RCA_DischargeLocation = "NZWLG";
			allocation3.RCA_DischargeLocation = "NZAKL";
			allocation4.RCA_DischargeLocation = "NZWLG";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			org2.OH_Code = "BBB";

			void AddAgent(RatingContractAllocationLine route, OrgHeader org)
			{
				var pivot = (AllocationRouteAgentPivot)route.AgentPivots.AddNew();
				pivot.ARA_OH_Agent = org.PK;
			}

			AddAgent(allocationMatchingAnyAndAll, org1);
			AddAgent(allocationMatchingAny, org1);
			AddAgent(allocationMatchingAny, org2);

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var agentsFilter = (AgentsOfAllocationRouteFilter)filter[AllocationRouteFilterConstants.Agents];
			agentsFilter.ComparisonOperator = locationFilterComparisonOperator;
			agentsFilter.IsActive = true;

			var orgFilter = agentsFilter.SelectedFilters;
			var orgCodeFilter = (ModuleTextFilter)orgFilter["Code"];
			orgCodeFilter.Property = "AAA";
			orgCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			orgCodeFilter.IsActive = true;

			return (new[] { allocationMatchingAnyAndAll, allocationMatchingAny, allocation3, allocation4 }, filter);
		}

		#region Sailing Schedule Proxy Filter Tests

		public void TestProxiedLocationsFilter_ShowRelatedUNLOCOsFalse()
		{
			var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var schedule1 = CreateSchedule("AUSYD", "NZAKL", "VOY1", "VES1", "BLAHAJ");
			var schedule2 = CreateSchedule("NZAKL", "NZTRG", "VOY2", "VES2", "DJUNGELSKOG");
			var schedule3 = CreateSchedule("NZTRG", "AUSYD", "VOY3", "VES3", "AFTONSPARV");

			allocationRoute1.RCA_JX_SailingSchedule = schedule1.PK;
			allocationRoute2.RCA_JX_SailingSchedule = schedule2.PK;
			allocationRoute3.RCA_JX_SailingSchedule = schedule3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.Property1 = "AUSYD";
			loadAndDischargeFilter.IsActive = true;
			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property1 = "NZAKL";
			loadAndDischargeFilter.Property2 = "AUSYD";

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up no route", 0, collection.Count);

			loadAndDischargeFilter.Property2 = "NZTRG";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);
		}

		public void TestProxiedLocationsFilter_ShowRelatedUNLOCOsTrue()
		{
			var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var schedule1 = CreateSchedule("AUSYD", "NZAKL", "VOY1", "VES1", "BLAHAJ");
			var schedule2 = CreateSchedule("NZAKL", "NZTRG", "VOY2", "VES2", "DJUNGELSKOG");
			var schedule3 = CreateSchedule("NZTRG", "AUSYD", "VOY3", "VES3", "AFTONSPARV");

			allocationRoute1.RCA_JX_SailingSchedule = schedule1.PK;
			allocationRoute2.RCA_JX_SailingSchedule = schedule2.PK;
			allocationRoute3.RCA_JX_SailingSchedule = schedule3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.Property1 = "AUSYD";
			loadAndDischargeFilter.IsActive = true;
			loadAndDischargeFilter.ShowRelatedUNLOCOs = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property1 = "NZAKL";
			loadAndDischargeFilter.Property2 = "AUSYD";

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up no route", 0, collection.Count);

			loadAndDischargeFilter.Property2 = "NZTRG";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);
		}

		public void TestProxiedVesselFilter()
		{
			var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var schedule1 = CreateSchedule("AUSYD", "NZAKL", "VOY1", "VES1", "BLAHAJ");
			var schedule2 = CreateSchedule("NZAKL", "NZTRG", "VOY2", "VES2", "YUMM");
			var schedule3 = CreateSchedule("NZTRG", "AUSYD", "VOY3", "VES3", "AFTONSPARV");

			allocationRoute1.RCA_JX_SailingSchedule = schedule1.PK;
			allocationRoute2.RCA_JX_SailingSchedule = schedule2.PK;
			allocationRoute3.RCA_JX_SailingSchedule = schedule3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var vesselFilter = (SoftModuleNkFilter)filter[AllocationRouteFilterConstants.Vessel];
			vesselFilter.Property = "VES1";
			vesselFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			vesselFilter.Property = "VES4";

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up no route", 0, collection.Count);

			vesselFilter.Property = "VES2";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);
		}

		public void TestProxiedVoyageNumberFilter()
		{
			var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var schedule1 = CreateSchedule("AUSYD", "NZAKL", "VOY1", "VES1", "BLAHAJ");
			var schedule2 = CreateSchedule("NZAKL", "NZTRG", "VOY2", "VES2", "YUMM");
			var schedule3 = CreateSchedule("NZTRG", "AUSYD", "VOY3", "VES3", "AFTONSPARV");

			allocationRoute1.RCA_JX_SailingSchedule = schedule1.PK;
			allocationRoute2.RCA_JX_SailingSchedule = schedule2.PK;
			allocationRoute3.RCA_JX_SailingSchedule = schedule3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var voyageFilter = (ModuleTextFilter)filter[AllocationRouteFilterConstants.VoyageNumber];
			voyageFilter.Property = "VOY1";
			voyageFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			voyageFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			voyageFilter.Property = "VOY4";

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up no route", 0, collection.Count);

			voyageFilter.Property = "VOY2";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);
		}

		public void TestProxiedServiceStringFilter()
		{
			var allocationRoute1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var allocationRoute3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var schedule1 = CreateSchedule("AUSYD", "NZAKL", "VOY1", "VES1", "BLAHAJ");
			var schedule2 = CreateSchedule("NZAKL", "NZTRG", "VOY2", "VES2", "YUMM");
			var schedule3 = CreateSchedule("NZTRG", "AUSYD", "VOY3", "VES3", "AFTONSPARV");

			allocationRoute1.RCA_JX_SailingSchedule = schedule1.PK;
			allocationRoute2.RCA_JX_SailingSchedule = schedule2.PK;
			allocationRoute3.RCA_JX_SailingSchedule = schedule3.PK;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var serviceStringFilter = (ModuleTextFilter)filter[AllocationRouteFilterConstants.ServiceString];
			serviceStringFilter.Property = "BLAHAJ";
			serviceStringFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			serviceStringFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			serviceStringFilter.Property = "WOOO";

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up no route", 0, collection.Count);

			serviceStringFilter.Property = "YUMM";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);
		}

		IJobSailing CreateSchedule(ZString load, ZString discharge, ZString voyageNumber, ZString vessel, ZString serviceString)
		{
			var voyage = Factory.New<IJobVoyage>();
			voyage.JV_VoyageFlight = voyageNumber;
			voyage.JV_RV_NKVessel = vessel;

			var origin = Factory.New<IVoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = load;

			var destination = Factory.New<IVoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = discharge;

			var schedule = Factory.New<IJobSailing>();
			schedule.JX_JA = origin.PK;
			schedule.JX_JB = destination.PK;
			schedule.JX_ServiceString = serviceString;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			return schedule;
		}

		#endregion

		#region Related Ports Test

		public void TestRelatedUNLOCOsForUnlinkedRoutes_LoadPort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB", "CCCCC", "DDDDD");
			dataCreator.CreateRelatedUNLOCOs(2, "EEEEE", "FFFFF", "GGGGG", "HHHHH");
			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute("AAAAA", "NZAKL");
			allocationRoute1.RCA_AllowRelatedPorts = true;
			var allocationRoute2 = dataCreator.CreateUnlinkedAllocationRoute("EEEEE", "NZAKL");
			allocationRoute2.RCA_AllowRelatedPorts = true;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.Property1 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property1 = "FFFFF";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);

			loadAndDischargeFilter.Property1 = "IIIII";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up no route", 0, collection.Count);
		}

		public void TestRelatedUNLOCOsForUnlinkedRoutes_DischargePort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB", "CCCCC", "DDDDD");
			dataCreator.CreateRelatedUNLOCOs(2, "EEEEE", "FFFFF", "GGGGG", "HHHHH");
			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute("NZAKL", "AAAAA");
			allocationRoute1.RCA_AllowRelatedPorts = true;
			var allocationRoute2 = dataCreator.CreateUnlinkedAllocationRoute("NZAKL", "EEEEE");
			allocationRoute2.RCA_AllowRelatedPorts = true;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.Property2 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property2 = "FFFFF";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);

			loadAndDischargeFilter.Property2 = "IIIII";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up no route", 0, collection.Count);
		}

		#endregion

		#region Allow Related UNLOCOs

		public void TestAllowRelatedUNLOCOs_AllowRelatedPortsOn()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			var allocationRoute = dataCreator.CreateUnlinkedAllocationRoute("EEEEE", "NZAKL");
			allocationRoute.RCA_AllowRelatedPorts = true;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var allowRelatedUNLOCOsFilter = (ModuleFlagsFilter)filter[AllocationRouteFilterConstants.AllowRelatedUNLOCOs];
			allowRelatedUNLOCOsFilter.Property0 = false;
			allowRelatedUNLOCOsFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up no route as allocationRoute1 has allow related ports set to true", 0, collection.Count);

			allowRelatedUNLOCOsFilter.Property0 = true;
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute }, collection);
		}

		public void TestAllowRelatedUNLOCOs_AllowRelatedPortsOff()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			var allocationRoute = dataCreator.CreateUnlinkedAllocationRoute("EEEEE", "NZAKL");
			allocationRoute.RCA_AllowRelatedPorts = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var allowRelatedUNLOCOsFilter = (ModuleFlagsFilter)filter[AllocationRouteFilterConstants.AllowRelatedUNLOCOs];
			allowRelatedUNLOCOsFilter.Property0 = true;
			allowRelatedUNLOCOsFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up no route as allocationRoute1 has allow related ports set to false", 0, collection.Count);

			allowRelatedUNLOCOsFilter.Property0 = false;
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should load up 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute }, collection);
		}

		#endregion

		#region Show Related UNLOCOs

		public void TestShowRelatedUNLOCOs_LoadPort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB");
			dataCreator.CreateRelatedUNLOCOs(2, "CCCCC", "DDDDD");

			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute(load: "AAAAA", discharge: "AUSYD");
			var allocationRoute2 = dataCreator.CreateUnlinkedAllocationRoute(load: "CCCCC", discharge: "NZAKL");

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = true;
			loadAndDischargeFilter.Property1 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should load 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property1 = "DDDDD";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should load 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);

			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should not load any routes as Show Related UNLOCOs has been set to false", 0, collection.Count);
		}

		public void TestShowRelatedUNLOCOs_DischargePort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB");
			dataCreator.CreateRelatedUNLOCOs(2, "CCCCC", "DDDDD");

			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute("AUSYD", "AAAAA");
			var allocationRoute2 = dataCreator.CreateUnlinkedAllocationRoute("AUSYD", "CCCCC");

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = true;
			loadAndDischargeFilter.Property2 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should load 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.Property2 = "DDDDD";
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should load 1 route", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { allocationRoute2 }, collection);

			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("The collection should not load any routes as Show Related UNLOCOs has been set to false", 0, collection.Count);
		}

		public void TestShowRelatedUNLOCOs_LoadPortDisregardsAllowRelatedUNLOCOs()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB");

			var allocationRoute = dataCreator.CreateUnlinkedAllocationRoute("AAAAA", "NZKAL");
			allocationRoute.RCA_AllowRelatedPorts = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			loadAndDischargeFilter.Property1 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should not load up any allocation routes", 0, collection.Count);

			allocationRoute.RCA_AllowRelatedPorts = true;

			Factory.Save();
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should not load up any allocation routes", 0, collection.Count);
		}

		public void TestShowRelatedUNLOCOs_DischargePortDisregardsAllowRelatedUNLOCOs()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "AAAAA", "BBBBB");

			var allocationRoute = dataCreator.CreateUnlinkedAllocationRoute("NZAKL", "AAAAA");
			allocationRoute.RCA_AllowRelatedPorts = false;

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			loadAndDischargeFilter.Property2 = "BBBBB";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should not load up any allocation routes", 0, collection.Count);

			allocationRoute.RCA_AllowRelatedPorts = true;

			Factory.Save();
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Collection should not load up any allocation routes", 0, collection.Count);
		}

		public void TestShowRelatedUNLOCOs_LoadPort_InternationalZoneRelatedPort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "CNSHA", "CNTAC");
			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute("ZSHA", "AUSYD");
			allocationRoute1.RCA_AllowRelatedPorts = true;

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			intZone.FZ_Code = "ZSHA";
			intZone.FZ_Description = "Zone with CNSHA";
			intZone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNSHA"));

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = true;
			loadAndDischargeFilter.Property1 = "CNTAC";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder("CNTAC is related to CNSHA, which is in zone ZSHA, so it should match", new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("The collection should not load any routes as Show Related UNLOCOs has been set to false", 0, collection.Count);
		}

		public void TestShowRelatedUNLOCOs_DischargePort_InternationalZoneRelatedPort()
		{
			var dataCreator = new AllocationRouteFilterBusinessObjectDataCreator(Factory);
			dataCreator.CreateRelatedUNLOCOs(1, "CNSHA", "CNTAC");
			var allocationRoute1 = dataCreator.CreateUnlinkedAllocationRoute("AUSYD", "ZSHA");
			allocationRoute1.RCA_AllowRelatedPorts = true;

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			intZone.FZ_Code = "ZSHA";
			intZone.FZ_Description = "Zone with CNSHA";
			intZone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNSHA"));

			Factory.Save();

			var filter = new AllocationRouteFilterBusinessObject();
			var loadAndDischargeFilter = (AllocationRouteCoveringLocationFilter)filter[AllocationRouteFilterConstants.LoadDischargePort];
			loadAndDischargeFilter.ShowRelatedUNLOCOs = true;
			loadAndDischargeFilter.Property2 = "CNTAC";
			loadAndDischargeFilter.IsActive = true;

			var collection = new RatingContractAllocationLineCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder("CNTAC is related to CNSHA, which is in zone ZSHA, so it should match", new[] { allocationRoute1 }, collection);

			loadAndDischargeFilter.ShowRelatedUNLOCOs = false;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("The collection should not load any routes as Show Related UNLOCOs has been set to false", 0, collection.Count);
		}

		#endregion
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AllocationRouteFilterBusinessObject();
		}
	}
}
