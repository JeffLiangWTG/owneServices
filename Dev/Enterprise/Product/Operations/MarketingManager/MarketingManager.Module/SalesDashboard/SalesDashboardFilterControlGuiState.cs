using CargoWise.Types;
using Enterprise.MasterFiles.GUI.Organisation;

namespace Enterprise.MarketingManager.Module
{
	public class SalesDashboardFilterControlGuiState
	{
		public SalesDashboardFilterControlGuiState(SalesDashboardFilterControl salesDashboardFilterControl)
		{
			this.salesDashboardFilterControl = salesDashboardFilterControl;
		}

		readonly SalesDashboardFilterControl salesDashboardFilterControl;

		public ZInt MainSplitterPosition
		{
			get { return OrganisationGuiState.LoadInteger(salesDashboardFilterControl, Schema.MainSplitterPosition); }
			set { OrganisationGuiState.SaveInteger(salesDashboardFilterControl, Schema.MainSplitterPosition, value); }
		}

		public ZInt PreviewSplitterDistance
		{
			get { return OrganisationGuiState.LoadInteger(salesDashboardFilterControl, Schema.PreviewSplitterDistance); }
			set { OrganisationGuiState.SaveInteger(salesDashboardFilterControl, Schema.PreviewSplitterDistance, value); }
		}

		public ZInt SalesRelationSplitterDistance
		{
			get { return OrganisationGuiState.LoadInteger(salesDashboardFilterControl, Schema.SalesRelationSplitterDistance); }
			set { OrganisationGuiState.SaveInteger(salesDashboardFilterControl, Schema.SalesRelationSplitterDistance, value); }
		}

		public ZInt TasksSplitterDistance
		{
			get { return OrganisationGuiState.LoadInteger(salesDashboardFilterControl, Schema.TasksSplitterDistance); }
			set { OrganisationGuiState.SaveInteger(salesDashboardFilterControl, Schema.TasksSplitterDistance, value); }
		}

		static class Schema
		{
			public const string MainSplitterPosition = "MainSplitterPosition";
			public const string PreviewSplitterDistance = "PreviewSplitterDistance";
			public const string SalesRelationSplitterDistance = "SalesRelationSplitterDistance";
			public const string TasksSplitterDistance = "TasksSplitterDistance";
		}
	}
}

#region
#endregion
