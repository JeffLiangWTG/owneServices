using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DynamicWorkOrderParentLinesGridUserControl : DynamicWorkOrderLinesGridUserControl
	{
		public DynamicWorkOrderParentLinesGridUserControl()
		{
			InitializeComponent();
			LinesGrid.ColumnLayoutContext = nameof(DocketLinesGridContext.DynamicWorkOrderParent);
		}

		public void SetVisibilityForAssembly(bool isAssembly)
		{
			LinesGrid.SetAvailability(!isAssembly, nameof(WhsDynamicWorkOrderLine.SumOfUnitsMet));
			LinesGrid.SetAvailability(isAssembly, nameof(WhsDynamicWorkOrderLine.IsSecondaryInwardProcessedItem));
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RefreshAllocationKeyVisibility();
			Docket.WarehouseChanged += OnAllocationKeyFieldChanged;
			Docket.DocketSubTypeChanged += OnAllocationKeyFieldChanged;
		}

		protected void OnAllocationKeyFieldChanged(object sender, EventArgs e)
			=> RefreshAllocationKeyVisibility();

		void RefreshAllocationKeyVisibility()
		{
			var displayAllocationKey =
				Docket.DocketTypeSupportsAllocationKey
				&& ObjectFactory.Get<Customs.ISupportedForProcessing>().IsSupportedForProcessing();

			LinesGrid.SetAvailability(displayAllocationKey, new[] { WhsDocketLineSchema.Constants.WE_AllocationKey });
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && Docket != null)
			{
				Docket.WarehouseChanged -= OnAllocationKeyFieldChanged;
				Docket.DocketSubTypeChanged -= OnAllocationKeyFieldChanged;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
