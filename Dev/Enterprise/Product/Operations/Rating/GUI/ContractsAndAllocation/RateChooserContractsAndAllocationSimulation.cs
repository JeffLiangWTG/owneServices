using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Rating.GUI
{
	internal class RateChooserContractsAndAllocationSimulation : IRatingContractSimulationFilterDefaults
	{
		readonly ZString contractNumber;
		readonly ZGuid serviceProviderPK;
		readonly ZString origin;
		readonly ZString destination;
		readonly ZDateTime effectiveDate;
		readonly RefCommodityCode commodity;

	public RateChooserContractsAndAllocationSimulation(string contractNumber, ZGuid serviceProviderPK, string origin, string destination, ZDateTime effectiveDate, RefCommodityCode commodity = default)
		{
			this.contractNumber = contractNumber;
			this.serviceProviderPK = serviceProviderPK;
			this.origin = origin;
			this.destination = destination;
			this.effectiveDate = effectiveDate;
			this.commodity = commodity;
		}

		FilterBusinessObjectDefaults IRatingContractSimulationFilterDefaults.GetFilterDefaultsForContract()
		{
			var defaults = new FilterBusinessObjectDefaults();

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.StartDate, "Property2", this.effectiveDate, true));

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ExpiryDate, "Property1", this.effectiveDate, true));

			if (!serviceProviderPK.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ServiceProvider, "Property", serviceProviderPK, true));
			}

			if (commodity?.RH_IsHazardous ?? false)
			{
				defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.AllowHazardousCommodities, "Property2", (ZBool)true, true));
			}

			defaults.Add(new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "Property", contractNumber, true));

			defaults.Add(FilterBusinessObjectDefault.Create(
				CarrierContractFilterConstants.AllocationRoutes,
				(NoResString)"Property",
				childDefaults: GetFilterDefaultsForAllocationRoute(),
				comparisonOperator: ModuleTextFilter.ComparisonConstants.AnyMatch));

			return defaults;
		}

		FilterBusinessObjectDefaults GetFilterDefaultsForAllocationRoute()
		{
			var defaults = new FilterBusinessObjectDefaults();

			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.StartDate, "Property2", this.effectiveDate, true));

			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "PropertySearch", new ZString((NoResString)"Date Range"), true));
			defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.ExpiryDate, "Property1", this.effectiveDate, true));

			if (!origin.IsEmpty || !destination.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property1", origin));
				defaults.Add(new FilterBusinessObjectDefault(AllocationRouteFilterConstants.LoadDischargePort, "Property2", destination));
			}

			return defaults;
		}

		internal void ShowPopup()
		{
			var formActions = null as IRatingContractSimulationFormActions;
			var filterDefaultsProvider = this as IRatingContractSimulationFilterDefaults;
			var configurationFactory = new RatingContractAllocationConfiguration(formActions, filterDefaultsProvider);
			var formFactory = ObjectFactory.Get<IContractAndAllocationsAttachFormFactory>();
			var form = formFactory.CreateForm(configurationFactory) as ZForm;
			ZFormModaliser.ShowDialogAndDispose(form);
		}
	}
}
