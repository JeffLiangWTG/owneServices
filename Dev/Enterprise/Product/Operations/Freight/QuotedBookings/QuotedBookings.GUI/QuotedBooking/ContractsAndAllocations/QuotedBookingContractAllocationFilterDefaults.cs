using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	sealed class QuotedBookingContractAllocationFilterDefaults : IRatingContractSimulationFilterDefaults
	{
		public QuotedBookingContractAllocationFilterDefaults(QuotedBooking quotedBooking)
		{
			this.quotedBooking = Argument.NotNull(quotedBooking, nameof(quotedBooking));
			containers = quotedBooking.QuotedBookingContainers.ToArray<ForwardingContainer>();
		}

		public QuotedBookingContractAllocationFilterDefaults(ForwardingContainer container)
		{
			quotedBooking = Argument.NotNull(container.QuotedBooking as QuotedBooking, nameof(container.QuotedBooking));
			containers = new ForwardingContainer[] { container };
			parentContainer = container;
		}

		readonly QuotedBooking quotedBooking;
		readonly IReadOnlyCollection<ForwardingContainer> containers;
		readonly ForwardingContainer parentContainer;

		public FilterBusinessObjectDefaults GetFilterDefaultsForContract()
		{
			var contractDefaults = new FilterBusinessObjectDefaults();

			var contractNumber = quotedBooking.CarrierContractNumber;
			if (!contractNumber.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "Property", contractNumber, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.StartsWith)));
			}

			var etd = quotedBooking.ETD;
			if (!etd.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "Property2", etd, true));

				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.Green, 1, true));

				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.Green, 2, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "Property1", etd, FilterOrCategory.Green, 2, true));
			}

			var transportMode = quotedBooking.TransportMode;
			if (!transportMode.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.TransportMode, "Property", transportMode, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.TransportMode, "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.Exact)));
			}

			var carrier = quotedBooking.OH_Carrier;
			if (!carrier.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ServiceProvider, "Property", carrier, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ServiceProvider, "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.Exact)));
			}

			var allContainers = quotedBooking.QuotedBookingContainers.ToArray<ForwardingContainer>();
			if (allContainers.Length > 0)
			{
				var containerTypesAreAllTheSame = allContainers.AllSame(container => container.Container?.RC_ContainerType ?? ZString.Empty);
				if (containerTypesAreAllTheSame)
				{
					contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContainerType, "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.IsBlank), FilterOrCategory.Aqua, instance: 1, true));

					contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContainerType, "Property", allContainers[0].Container?.RC_ContainerType, FilterOrCategory.Aqua, instance: 2, true));
					contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContainerType, "ComparisonOperator", new ZString(ModuleTextFilter.ComparisonConstants.StartsWith), FilterOrCategory.Aqua, instance: 2, true));
				}

				var foundHazardousCommodity = allContainers.Any(container => container.ContainerCommodityCode?.RH_IsHazardous ?? false);

				if (foundHazardousCommodity)
				{
					contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.AllowHazardousCommodities, "Property0", ZBool.True, ZBool.True));
				}
			}

			AddAllocationRouteDefaults(contractDefaults);

			return contractDefaults;
		}

		void AddAllocationRouteDefaults(FilterBusinessObjectDefaults parentDefaults)
		{
			parentDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.HasAllocationRoutes, "Property0", ZBool.False, FilterOrCategory.Red, 1, ZBool.True));

			parentDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: CarrierContractFilterConstants.AllocationRoutes,
				propertyName: (NoResString)"Property",
				childDefaults: GetFilterDefaultsForAllocationRouteWhereMatching(),
				category: FilterOrCategory.Red,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
				instance: 2));
		}

		FilterBusinessObjectDefaults GetFilterDefaultsForAllocationRouteWhereMatching()
		{
			var routeDefaultsWhereMatching = new FilterBusinessObjectDefaults();
			var etd = quotedBooking.ETD;
			if (!etd.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.LightPink, 1, true));

				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.LightPink, 2, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "Property2", etd, FilterOrCategory.LightPink, 2, true));

				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.Green, 1, true));

				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.Green, 2, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "Property1", etd, FilterOrCategory.Green, 2, true));
			}

			var voyage = quotedBooking.ScheduleChooser?.Sailing?.JX_JV_VoyageFlight ?? string.Empty;
			if (!voyage.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.VoyageNumber,
					propertyName: (NoResString)"Property",
					category: FilterOrCategory.Red,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
					instance: 1));

				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.VoyageNumber,
					propertyName: (NoResString)"Property",
					value: voyage,
					category: FilterOrCategory.Red,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
					instance: 2));
			}

			var vessel = quotedBooking.ScheduleChooser?.Sailing?.JX_JV_NKVessel ?? string.Empty;
			if (!vessel.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.Vessel,
					propertyName: (NoResString)"Property",
					category: FilterOrCategory.Blue,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
					instance: 1));

				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.Vessel,
					propertyName: (NoResString)"Property",
					value: vessel,
					category: FilterOrCategory.Blue,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
					instance: 2));
			}

			if (containers.Count > 0)
			{
				var allSame = containers.AllSame(container => container.JC_RC);
				var storageClassSame = containers.AllSame(container => container.Container?.RC_StorageClass);
				var freightRateClassSame = containers.AllSame(container => container.Container?.RC_FreightRateClass);
				if (allSame)
				{
					AddFilterContainerTypeAllSame(routeDefaultsWhereMatching);
				}
				else if (!allSame && storageClassSame)
				{
					AddFilterDifferentContainerTypeSameStorageClass(routeDefaultsWhereMatching);
				}
				else if (!allSame && freightRateClassSame)
				{
					AddFilterDifferentContainerTypeSameFreightRateClass(routeDefaultsWhereMatching);
				}
			}

			var commonCCAData = quotedBooking as ICCACommonAssignmentValidationData;
			var allocationRoute = parentContainer != null
				? parentContainer.Factory.Load<IRatingContractAllocationLine>(parentContainer.JC_RCA_AllocationLine)
				: commonCCAData?.AllocationRoute;

			var allocationId = allocationRoute?.RCA_AllocationLineID ?? ZString.Empty;

			if (!allocationId.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.AllocationRouteID, "Property", allocationId, true));
			}

			return routeDefaultsWhereMatching;
		}

		void AddFilterContainerTypeAllSame(FilterBusinessObjectDefaults routeDefaultsWhereMatching)
		{
			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.ContainerType,
				propertyName: (NoResString)"Property",
				category: FilterOrCategory.AntiqueWhite,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
				instance: 1));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.ContainerType,
				propertyName: (NoResString)"Property",
				value: containers.First().JC_RC,
				category: FilterOrCategory.AntiqueWhite,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
				instance: 2));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				category: FilterOrCategory.AntiqueWhite,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
				instance: 3));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				value: containers.First().Container?.RC_StorageClass,
				category: FilterOrCategory.AntiqueWhite,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
				instance: 4));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				value: containers.First().Container?.RC_FreightRateClass,
				category: FilterOrCategory.AntiqueWhite,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
				instance: 5));
		}

		void AddFilterDifferentContainerTypeSameStorageClass(FilterBusinessObjectDefaults routeDefaultsWhereMatching)
		{
			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				category: FilterOrCategory.Magenta,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
				instance: 1));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				value: containers.First().Container?.RC_StorageClass,
				category: FilterOrCategory.Magenta,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
				instance: 2));
		}

		void AddFilterDifferentContainerTypeSameFreightRateClass(FilterBusinessObjectDefaults routeDefaultsWhereMatching)
		{
			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				category: FilterOrCategory.LimeGreen,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
				instance: 1));

			routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
				filterName: AllocationRouteFilterConstants.StorageOrFreightRateClass,
				propertyName: (NoResString)"Property",
				value: containers.First().Container.RC_FreightRateClass,
				category: FilterOrCategory.LimeGreen,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
				instance: 2));
		}
	}
}
