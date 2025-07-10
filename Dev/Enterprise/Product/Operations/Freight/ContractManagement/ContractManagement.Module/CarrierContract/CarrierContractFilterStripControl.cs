using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ContractManagement.Business;
using Enterprise.ContractManagement.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ContractManagement.Module
{
	public partial class CarrierContractFilterStripControl : ZFilterStripCommonControl, IAllocationGridProvider
	{
		public CarrierContractFilterStripControl(IContractSimulationFormConfiguration formConfiguration)
			: base(new CarrierContractFilterStripBusinessObject())
		{
			this.formConfiguration = formConfiguration;
			FilterBusinessObject.SetExternalDefaults(GetDefaults(formConfiguration.FilterDefaults));
			InitializeComponentAndPerformSearch();
		}

		readonly IContractSimulationFormConfiguration formConfiguration;

		public CarrierContractFilterStripControl() : base(new CarrierContractFilterStripBusinessObject())
		{
			InitializeComponentAndPerformSearch();
		}

		public override void Find(bool isManualSearch = true)
		{
			try
			{
				FirePerformSearch(Visible && isManualSearch, isManualSearch);
			}
			finally
			{
				ToolStrip.Enabled = true;
			}
		}

		void InitializeComponentAndPerformSearch()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				InitializeComponent();
				PerformSearch += FilterControl_PerformSearch;
				ContractFilterGridPanel.AllowOverlap(ToolStrip);
				ContractFilterGridPanel.AllowOverlap(ToolStripHelp);
				ContractFilterGridPanel.ContractFilterGrid.AfterBind += ContractFilterGrid_AfterBind;
			}
		}

		void FilterControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			if ((DataSource as ViewCarrierContractsManager)?.CarrierContracts is CarrierContractForUtilizationSimulationCollection contracts)
			{
				var factory = SearchManager.GetNewFactory();
				var result = SearchManager.PerformSearch(factory, typeof(CarrierContractForUtilizationSimulation), FilterBusinessObject.Filter);
				if (result.Type == PerformSearchResultType.Success)
				{
					relatedRoutesQuery = null;
					SearchManager.PushItemsIntoCollection(contracts, result, new SortInfo(RatingContract.Schema.RCT_ContractNumber, ListSortDirection.Descending));
					SetNotificationsOnRowsAsAppropriate(contracts);
				}
			}
		}

		void ContractFilterGrid_AfterBind(object sender, EventArgs e)
		{
			if (ContractFilterGridPanel?.ContractFilterGrid?.ListManager != null)
			{
				ContractFilterGridPanel.ContractFilterGrid.ListManager.CurrentChanged -= FilterSelectedContractRoutes;
				ContractFilterGridPanel.ContractFilterGrid.ListManager.CurrentChanged += FilterSelectedContractRoutes;
			}
		}

		void FilterSelectedContractRoutes(object sender, EventArgs e)
		{
			var currentlySelectedContract = ContractFilterGridPanel.ContractFilterGrid.ListManager.GetCurrent() as CarrierContractForUtilizationSimulation;
			if (currentlySelectedContract == null)
			{
				return;
			}

			currentlySelectedContract.AllocationRoutesForUtilizationSimulation.AdditionalFilter = RelatedRoutesQuery;
		}

		ZQuery GetRelatedRoutesQuery()
		{
			var relatedRoutesFilterStrips = FilterBusinessObject.ActiveModuleFilters.OfType<RelatedAllocationsOfContractFilter>();
			var groupedRoutesFilterStrips = relatedRoutesFilterStrips.GroupBy(x => x.OrCategory);

			var relatedRoutesOfContractQuery = new ZQuery();
			foreach (var routeFilterStripGroup in groupedRoutesFilterStrips)
			{
				var joinCondition = routeFilterStripGroup.Key == FilterOrCategory.None ? JoinCondition.And : JoinCondition.Or;

				var orGroupQuery = new ZQuery();
				foreach (var routeFilterStrip in routeFilterStripGroup)
				{
					orGroupQuery.AddToFilter(routeFilterStrip.SelectedFilters.Filter, joinCondition);
				}

				relatedRoutesOfContractQuery.AddToFilter(orGroupQuery, JoinCondition.And);
			}

			return relatedRoutesOfContractQuery;
		}

		ZQuery RelatedRoutesQuery => relatedRoutesQuery ??= GetRelatedRoutesQuery();
		ZQuery relatedRoutesQuery;

		void SetNotificationsOnRowsAsAppropriate(CarrierContractForUtilizationSimulationCollection contracts)
		{
			if (formConfiguration.NotificationProvider == null)
			{
				return;
			}

			foreach (var contract in contracts)
			{
				var notification = formConfiguration.NotificationProvider.GetContractAllocationNotification(contract);
				if (notification != null)
				{
					using (contract.ResumeValidationTemporarily())
					{
						contract.AddRowNotification(notification);
					}
				}

				var allocationRoutes = contract.AllocationRoutesForUtilizationSimulation;
				foreach (var allocationRoute in allocationRoutes)
				{
					var allocationRouteNotification = formConfiguration.NotificationProvider.GetRouteAllocationNotification(allocationRoute);
					if (allocationRouteNotification != null)
					{
						using (allocationRoute.ResumeValidationTemporarily())
						{
							allocationRoute.AddRowNotification(allocationRouteNotification);
						}
					}
				}
			}
		}

		protected override Control ControlForLayout
		{
			get
			{
				return IsAllocationsModuleEnabled
					? ContractFilterGridPanel
					: base.ControlForLayout;
			}
		}

		protected override ZFilterGrid GetNewFilteredGrid() => IsAllocationsModuleEnabled
			? ContractFilterGridPanel.ContractFilterGrid
			: base.GetNewFilteredGrid();

		protected override void HandleGridSizing()
		{
			base.HandleGridSizing();

			if (IsAllocationsModuleEnabled)
			{
				ContractFilterGridPanel.ContractFilterGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			}
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			if (IsAllocationsModuleEnabled)
			{
				grid.Dock = DockStyle.Fill;
			}
			else
			{
				grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			}
		}

		protected override bool GetDefaultShouldRunSearchOnStripsInitialized() => true;

		protected override void Dispose(bool disposing)
		{
			PerformSearch -= FilterControl_PerformSearch;
			SearchManager?.Dispose();
			contractFilterGridPanel?.Dispose();
			base.Dispose(disposing);
		}

		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
		int RecommendedRowsToLoad => EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids;

		FilteredGridLoader SearchManager => searchManager ??= CreateSearchManager();
		FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, new ResultCountMessage(this, MaxRowsToLoad, RecommendedRowsToLoad), false, ModuleIDs.CarrierContractAndAllocations, () => new BusinessObjectFactory(), typeof(CarrierContractForUtilizationSimulation));

		ContractAndAllocationsGridPanel ContractFilterGridPanel => contractFilterGridPanel ??= new ContractAndAllocationsGridPanel();
		ContractAndAllocationsGridPanel contractFilterGridPanel;

		FilterBusinessObjectDefaults GetDefaults(IRatingContractSimulationFilterDefaults defaultFilterProvider)
			=> defaultFilterProvider?.GetFilterDefaultsForContract() ?? new FilterBusinessObjectDefaults();

		bool IsAllocationsModuleEnabled => ObjectFactory.Get<IContractPermissions>().IsAllocationsVisible();

		ZGrid IAllocationGridProvider.AllocationRouteGrid => ContractFilterGridPanel.AllocationRouteGrid;
	}
}
