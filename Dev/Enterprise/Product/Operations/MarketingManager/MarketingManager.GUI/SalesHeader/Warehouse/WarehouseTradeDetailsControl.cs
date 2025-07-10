using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseTradeDetailsControl : NonGroupingTradeDetailsControl
	{
		public WarehouseTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}

		public override ZGrid TradeDetailsGrid
		{
			get { return CurrentlyVisibleGridControl != null ? CurrentlyVisibleGridControl.TradeDetailsGrid : null; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null
				&& (currentlyHookedSalesForServiceChange == null || currentlyHookedSalesForServiceChange.IsDeleted || currentlyHookedSalesForServiceChange.OW_Service != CurrentDataItem.OW_Service))
			{
				OnServiceChanged();
			}

			HookServiceChangedEvent();
		}

		void HookServiceChangedEvent()
		{
			var currentSales = CurrentDataItem;
			if (currentlyHookedSalesForServiceChange != currentSales)
			{
				if (currentlyHookedSalesForServiceChange != null)
				{
					currentlyHookedSalesForServiceChange.OW_ServiceInfo.ValueChanged -= OW_ServiceInfo_ValueChanged;
				}

				currentlyHookedSalesForServiceChange = currentSales;

				if (currentlyHookedSalesForServiceChange != null)
				{
					currentlyHookedSalesForServiceChange.OW_ServiceInfo.ValueChanged += OW_ServiceInfo_ValueChanged;
				}
			}
		}
		OrgSales currentlyHookedSalesForServiceChange;

		void OW_ServiceInfo_ValueChanged(object sender, EventArgs e)
		{
			OnServiceChanged();
		}

		void OnServiceChanged()
		{
			var service = CurrentDataItem != null ? CurrentDataItem.OW_Service : ZString.Empty;
			WarehouseTradeDetailsGridControl gridControl;
			if (!gridControlByService.TryGetValue(service, out gridControl))
			{
				gridControl = WarehouseTradeDetailsGridControl.New(SalesProduct, service);
				gridControl.Dock = DockStyle.Fill;
				AddCustomColumns(gridControl.TradeDetailsGrid);
				topSplitContainer.Panel1.Controls.Add(gridControl);

				gridControlByService[service] = gridControl;
			}

			if (CurrentlyVisibleGridControl != gridControl)
			{
				SuspendLayout();
				try
				{
					if (CurrentlyVisibleGridControl != null)
					{
						CurrentlyVisibleGridControl.Visible = false;
					}

					CurrentlyVisibleGridControl = gridControl;

					if (CurrentlyVisibleGridControl != null)
					{
						CurrentlyVisibleGridControl.Visible = true;
					}
				}
				finally
				{
					ResumeLayout();
				}
			}
			else if (CurrentlyVisibleGridControl != null)
			{
				CurrentlyVisibleGridControl.Visible = true;
			}
		}

		protected WarehouseTradeDetailsGridControl CurrentlyVisibleGridControl;
		readonly Dictionary<ZString, WarehouseTradeDetailsGridControl> gridControlByService = new Dictionary<ZString, WarehouseTradeDetailsGridControl>();
	}
}
