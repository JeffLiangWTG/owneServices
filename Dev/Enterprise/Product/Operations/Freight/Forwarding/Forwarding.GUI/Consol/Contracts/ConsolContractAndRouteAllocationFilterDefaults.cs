using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class ConsolContractAndRouteAllocationFilterDefaults : IRatingContractSimulationFilterDefaults
	{
		public ConsolContractAndRouteAllocationFilterDefaults(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));
			this.consol = consol;

			mainTransportLeg = GetMainTransportLeg(consol);
		}

		public ConsolContractAndRouteAllocationFilterDefaults(ForwardingContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
			consol = Argument.NotNull(container.Consol, nameof(container.Consol));

			mainTransportLeg = GetMainTransportLeg(consol);
		}

		static Transport GetMainTransportLeg(ForwardingConsol consol)
		{
			return consol.Transports
				.OfType<Transport>()
				.FirstOrDefault(leg => leg.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel
									&& leg.JW_TransportMode == consol.JK_TransportMode);
		}

		readonly ForwardingConsol consol;
		readonly ForwardingContainer container;
		readonly Transport mainTransportLeg;

		FilterBusinessObjectDefaults IRatingContractSimulationFilterDefaults.GetFilterDefaultsForContract()
		{
			var contractDefaults = new FilterBusinessObjectDefaults();

			var contractNumber = consol.JK_CarrierContractNumber;
			if (!contractNumber.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "Property", contractNumber, true));
			}

			var transportMode = consol.JK_TransportMode;
			if (!transportMode.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.TransportMode, "Property", transportMode, true));
			}

			var etd = mainTransportLeg?.JW_ETD.Date ?? ZDate.Empty;
			if (!etd.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "Property2", etd, true));

				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.Green, 1, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.Green, 2, true));
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "Property1", etd, FilterOrCategory.Green, 2, true));
			}

			var carrier = consol.ShippingLinePK;
			if (!carrier.IsEmpty)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ServiceProvider, "Property", carrier, true));
			}

			var commodity = consol.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, consol.JK_RH_NKConsolCommodity);
			if (commodity?.RH_IsHazardous ?? false)
			{
				contractDefaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.AllowHazardousCommodities, "Property2", (ZBool)true, true));
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
				childDefaults: GetFilterDefaultsForAllocationRoute_WhereMatching(),
				category: FilterOrCategory.Red,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch,
				instance: 2));
		}

		FilterBusinessObjectDefaults GetFilterDefaultsForAllocationRoute_WhereMatching()
		{
			var routeDefaultsWhereMatching = new FilterBusinessObjectDefaults();
			var allocationLine = container?.AllocationLine ?? consol.AllocationLine;

			var allocationID = allocationLine?.RCA_AllocationLineID ?? ZString.Empty;

			if (!allocationID.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.AllocationRouteID, "Property", allocationID, true));
			}

			var etd = mainTransportLeg?.JW_ETD.Date ?? ZDate.Empty;
			if (!etd.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.Green, 1, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.Green, 2, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "Property2", etd, FilterOrCategory.Green, 2, true));

				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.HasNoDateEntered, FilterOrCategory.Brown, 1, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, FilterOrCategory.Brown, 2, true));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "Property1", etd, FilterOrCategory.Brown, 2, true));
			}

			var loadPort = mainTransportLeg?.JW_RL_NKLoadPort ?? consol.JK_RL_NKLoadPort;
			var dischargePort = mainTransportLeg?.JW_RL_NKDiscPort ?? consol.JK_RL_NKDischargePort;
			if (!loadPort.IsEmpty || !dischargePort.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property1", loadPort));
				routeDefaultsWhereMatching.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property2", dischargePort));
			}

			var voyage = mainTransportLeg?.JW_VoyageFlight ?? ZString.Empty;
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

			var vessel = mainTransportLeg?.JW_Vessel ?? ZString.Empty;
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

			var serviceString = mainTransportLeg?.JW_ServiceString ?? ZString.Empty;
			if (!serviceString.IsEmpty)
			{
				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.ServiceString,
					propertyName: (NoResString)"Property",
					category: FilterOrCategory.Tan,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
					instance: 1));

				routeDefaultsWhereMatching.Add(FilterBusinessObjectDefault.Create(
					filterName: AllocationRouteFilterConstants.ServiceString,
					propertyName: (NoResString)"Property",
					value: serviceString,
					category: FilterOrCategory.Tan,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
					instance: 2));
			}

			return routeDefaultsWhereMatching;
		}
	}
}
