using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeLaneWithDetailsControl : ZUserControl
	{
		public TradeLaneWithDetailsControl()
		{
			InitializeComponent();
		}

		#region DataBinding

		SalesHeader SalesHeader
		{
			get { return CurrentDataItem; }
		}

		public new SalesHeader CurrentDataItem
		{
			get { return (SalesHeader)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			associationsAllowed =
				CurrentDataItem == null
				|| CurrentDataItem.SalesProduct == null
				|| CurrentDataItem.SalesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales);

			if (!associationsAllowed)
			{
				SuspendLayout();

				Controls.Remove(associatedEntitiesGrid);
				associatedEntitiesGrid.Dispose();
				topSplitContainer.Panel2Collapsed = true;

				ResumeLayout();
			}
		}

		bool associationsAllowed;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			AddInnerControls();
		}

		#endregion

		#region Properties

		#region Collapsed

		[DefaultValue(false)]
		public bool Collapsed
		{
			get { return collapsed; }
			set
			{
				collapsed = value;
				mainSplitContainer.Panel2Collapsed = collapsed;
				ResizeControlToFitContents();
			}
		}
		bool collapsed;

		#endregion

		#region AutoSizeTradeLanesControl

		[DefaultValue(false)]
		public bool AutoSizeTradeLanesControl
		{
			get { return autoSizeTradeLanesControl; }
			set
			{
				autoSizeTradeLanesControl = value;
				mainSplitContainer.IsSplitterFixed = value;

				if (value)
				{
					ResizeControlToFitContents();
					AddAutoSizeTradeLanesControlHandler();
				}
				else
				{
					RemoveAutoSizeTradeLanesControlHandler();
				}
			}
		}
		bool autoSizeTradeLanesControl;

		#endregion

		#region ShowAssociatedActivities

		[DefaultValue(true)]
		public bool ShowAssociatedActivities
		{
			get { return showAssociatedActivities && associationsAllowed; }
			set
			{
				showAssociatedActivities = value;
				topSplitContainer.Panel2Collapsed = !ShowAssociatedActivities;

				if (AutoSizeTradeLanesControl)
				{
					ResizeControlToFitContents();
				}
			}
		}
		bool showAssociatedActivities;

		#endregion

		#region ShowActualsIfSupported

		public void ShowActualsIfSupported()
		{
			if (!showActualsIfSupported)
			{
				showActualsIfSupported = true;
				if (tradeLanesControl != null)
				{
					tradeLanesControl.ShowActualsIfSupported();
				}
			}
		}
		bool showActualsIfSupported;

		#endregion

		#endregion

		#region Inner Controls

		void AddInnerControls()
		{
			var salesHeader = SalesHeader;
			if (salesHeader != null)
			{
				var product = salesHeader.SalesProduct;
				tradeLanesControl = TradeLanesControl.New(product);
				tradeLanesControl.Dock = DockStyle.Fill;

				if (showActualsIfSupported)
				{
					tradeLanesControl.ShowActualsIfSupported();
				}

				BindingSource.SetBindingMember(tradeLanesControl, ".");
				topSplitContainer.Panel1.Controls.Add(tradeLanesControl);

				tradeDetailsControl = TradeDetailsControl.New(product);
				tradeDetailsPreferredHeight = tradeDetailsControl.Height + TradeDetailsHeightAdjustment;
				tradeDetailsControl.Dock = DockStyle.Fill;

				BindingSource.SetBindingMember(tradeDetailsControl, "FilterableEntitySalesCollection");
				mainSplitContainer.Panel2.Controls.Add(tradeDetailsControl);

				if (AutoSizeTradeLanesControl)
				{
					AddAutoSizeTradeLanesControlHandler();
				}
			}
		}

		#region TradeLanesControl

		public TradeLanesControl TradeLanesControl
		{
			get { return tradeLanesControl; }
		}
		TradeLanesControl tradeLanesControl;

		internal ZGrid TradeLanesGrid
		{
			get { return tradeLanesControl != null ? tradeLanesControl.TradeLanesGrid : null; }
		}

		#region Trade Lanes Control AutoSize

		void AddAutoSizeTradeLanesControlHandler()
		{
			if (TradeLanesGrid != null)
			{
				if (TradeLanesGrid.ListManager != null)
				{
					TradeLanesGrid.ListManager.ListChanged -= TradeLaneListManager_ListChanged;
					TradeLanesGrid.ListManager.ListChanged += TradeLaneListManager_ListChanged;
				}
				else
				{
					TradeLanesGrid.AfterBind -= TradeLanesGrid_AfterBind;
					TradeLanesGrid.AfterBind += TradeLanesGrid_AfterBind;
				}
			}
		}

		void RemoveAutoSizeTradeLanesControlHandler()
		{
			if (TradeLanesGrid != null)
			{
				TradeLanesGrid.AfterBind -= TradeLanesGrid_AfterBind;
				if (TradeLanesGrid.ListManager != null)
				{
					TradeLanesGrid.ListManager.ListChanged -= TradeLaneListManager_ListChanged;
				}
			}
		}

		void TradeLanesGrid_AfterBind(object sender, EventArgs e)
		{
			TradeLanesGrid.AfterBind -= TradeLanesGrid_AfterBind;
			if (TradeLanesGrid.ListManager != null)
			{
				ResizeControlToFitContents();
				TradeLanesGrid.ListManager.ListChanged += TradeLaneListManager_ListChanged;
			}
		}

		void TradeLaneListManager_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded ||
				e.ListChangedType == ListChangedType.ItemDeleted ||
				e.ListChangedType == ListChangedType.Reset)
			{
				ResizeControlToFitContents();
			}
		}

		void ResizeControlToFitContents()
		{
			if (autoSizeTradeLanesControl)
			{
				if (TradeLanesGrid == null)
				{
					return;
				}

				var tradeLanesGridRowCount = TradeLanesGrid.ListManager != null ? (TradeLanesGrid.ListManager.Count + 3) : 2;
				var tradeLanesGridPreferredHeight = TradeLanesGrid.PreferredRowHeight * tradeLanesGridRowCount;
				if (TradeLanesGrid.IsHorizontalScrollBarVisible)
				{
					tradeLanesGridPreferredHeight += SystemInformation.HorizontalScrollBarHeight;
				}

				if (!Collapsed)
				{
					tradeLanesGridPreferredHeight = Math.Max(tradeLanesGridPreferredHeight, ControlDpiScalingHelper.ScaleToCurrentDpiY(100));
				}

				tradeLanesGridPreferredHeight = Math.Min(tradeLanesGridPreferredHeight, TradeLanesGrid.PreferredRowHeight * 15);

				ControlDpiScalingHelper.SetHeight(this,
					tradeLanesGridPreferredHeight +
					mainSplitContainer.SplitterWidth +
					(Collapsed ? 0 : tradeDetailsPreferredHeight),
					false);
			}
		}

		#endregion

		#endregion

		#region TradeDetailsControl

		public TradeDetailsControl TradeDetailsControl
		{
			get { return tradeDetailsControl; }
		}
		TradeDetailsControl tradeDetailsControl;

		int tradeDetailsPreferredHeight;

		internal int TradeDetailsHeightAdjustment = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

		#endregion

		#region Trade Lanes / Details Context Menu

		public void SetupExpiryContextMenu()
		{
			if (TradeLanesGrid != null)
			{
				EstimateValueExpiryMenuItemBuilder.SetupContextMenu(TradeLanesGrid, GetSelectedTradeDetailsFromTradeLanesGrid);
			}

			if (TradeDetailsControl?.TradeDetailsGrid != null)
			{
				EstimateValueExpiryMenuItemBuilder.SetupContextMenu(TradeDetailsControl.TradeDetailsGrid, GetSelectedTradeDetailsFromTradeDetailsGrid);
			}
		}

		IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsFromTradeLanesGrid()
		{
			IEnumerable<OrgTradeDetail> selectedDetails = Enumerable.Empty<OrgTradeDetail>();
			if (TradeLanesGrid?.ListManager != null)
			{
				if (TradeLanesGrid.SelectedElements.Length > 0)
				{
					selectedDetails = TradeLanesGrid.SelectedElements.Cast<EntitySalesWrapper>().SelectMany(x => x.EntityTradeDetails);
				}
				else
				{
					var current = TradeLanesGrid.ListManager.GetCurrent() as EntitySalesWrapper;
					if (current != null)
					{
						selectedDetails = current.EntityTradeDetails;
					}
				}
			}

			return selectedDetails;
		}

		IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsFromTradeDetailsGrid()
		{
			IEnumerable<OrgTradeDetail> selectedDetails = Enumerable.Empty<OrgTradeDetail>();
			if (TradeDetailsControl?.TradeDetailsGrid?.ListManager != null)
			{
				var grid = TradeDetailsControl.TradeDetailsGrid;
				if (grid.SelectedElements.Length > 0)
				{
					selectedDetails = grid.SelectedElements.Cast<OrgTradeDetail>();
				}
				else
				{
					var current = grid.ListManager.GetCurrent() as OrgTradeDetail;
					if (current != null)
					{
						selectedDetails = new OrgTradeDetail[] { current };
					}
				}
			}

			return selectedDetails;
		}

		#endregion

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			RemoveAutoSizeTradeLanesControlHandler();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
