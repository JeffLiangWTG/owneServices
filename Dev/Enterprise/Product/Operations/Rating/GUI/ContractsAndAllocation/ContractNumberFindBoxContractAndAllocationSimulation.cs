using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	class ContractNumberFindBoxContractAndAllocationSimulation : IRatingContractSimulationFormActions, IRatingContractSimulationFilterDefaults
	{
		readonly IContractNumberFindBoxPopupSupport contractInfo;

		public ContractNumberFindBoxContractAndAllocationSimulation(IContractNumberFindBoxPopupSupport contractInfo)
		{
			this.contractInfo = contractInfo;
		}

		internal void ShowPopup()
		{
			var formActions = this as IRatingContractSimulationFormActions;
			var filterDefaultsProvider = this as IRatingContractSimulationFilterDefaults;
			var configurationFactory = new RatingContractAllocationConfiguration(formActions, filterDefaultsProvider);
			var formFactory = ObjectFactory.Get<IContractAndAllocationsAttachFormFactory>();
			var form = formFactory.CreateForm(configurationFactory) as ZForm;
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		RateEntry RateEntry => (RateEntry)contractInfo.RateEntry;

		#region IRatingContractSimulationFormActions

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToContract => true;

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToRoute => false;

		bool IRatingContractSimulationFormActions.TryAllocateToAllocationRoute(IRatingContractAllocationLine allocationRoute)
		{
			throw new NotImplementedException("IsEnabledAllocationToRoute should stop this from being called");
		}

		bool IRatingContractSimulationFormActions.TryAllocateToContract(IRatingContract contract)
		{
			contractInfo.ContractNumber = contract.RCT_ContractNumber;
			return true;
		}

		#endregion

		#region IRatingContractSimulationFilterDefaults

		FilterBusinessObjectDefaults IRatingContractSimulationFilterDefaults.GetFilterDefaultsForContract()
		{
			var defaults = new FilterBusinessObjectDefaults();
			var costingPK = RateEntry.Parent.TH_OH;
			var transportMode = GetTransportMode(RateEntry.TI_Mode);

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "Property2", RateEntry.TI_RateStartDate, true));

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "Property1", RateEntry.TI_RateEndDate, true));

			if (!costingPK.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ServiceProvider, "Property", costingPK, true));
			}

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "Property", (ZString)contractInfo.ContractNumber, true));

			if (!transportMode.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.TransportMode, "Property", transportMode, true));
			}

			defaults.Add(FilterBusinessObjectDefault.Create(
				CarrierContractFilterConstants.AllocationRoutes,
				(NoResString)"Property",
				childDefaults: GetFilterDefaultsForAllocationRoute(),
				comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch));

			if (RateEntry.Container != null)
			{
				// Filters with the same category colour are OR'd together
				defaults.Add(FilterBusinessObjectDefault.Create(CarrierContractFilterConstants.ContainerType, (NoResString)"Property", RateEntry.Container.RC_ContainerType, category: FilterOrCategory.Green, comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact, instance: 1));
				defaults.Add(FilterBusinessObjectDefault.Create(CarrierContractFilterConstants.ContainerType, (NoResString)"Property", category: FilterOrCategory.Green, comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank, instance: 2));
			}

			if (RateEntry.CommodityCode?.RH_IsHazardous ?? false)
			{
				defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.AllowHazardousCommodities,  "Property0", (ZBool)true, true));
			}

			return defaults;
		}

		FilterBusinessObjectDefaults GetFilterDefaultsForAllocationRoute()
		{
			var defaults = new FilterBusinessObjectDefaults();

			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "Property2", RateEntry.TI_RateStartDate, true));

			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "Property1", RateEntry.TI_RateEndDate, true));

			if (!RateEntry.TI_OriginLRC.IsEmpty || !RateEntry.TI_DestinationLRC.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property1", RateEntry.TI_OriginLRC));
				defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property2", RateEntry.TI_DestinationLRC));
			}

			return defaults;
		}

		ZString GetTransportMode(ZString rateMode)
		{
			// RatingsContracts only supports SEA and AIR. Need to translate
			switch (rateMode)
			{
				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.SEA:
					return Core.Constants.RateMode.SEA;

				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.AIR:
					return Core.Constants.RateMode.AIR;

				default:
					return rateMode;
			}
		}

		#endregion
	}
}
