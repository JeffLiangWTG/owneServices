using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class DynamicTradeLaneWithDetailsControl : ZUserControl
	{
		public DynamicTradeLaneWithDetailsControl()
		{
			InitializeComponent();
		}

		#region Properties

		#region ShowActualsIfSupported

		public void ShowActualsIfSupported()
		{
			if (!showActualsIfSupported)
			{
				showActualsIfSupported = true;
				if (CurrentlyVisibleInnerControl != null)
				{
					CurrentlyVisibleInnerControl.ShowActualsIfSupported();
				}
			}
		}
		bool showActualsIfSupported;

		#endregion

		#endregion

		#region DataBinding

		public new SalesHeader CurrentDataItem
		{
			get { return (SalesHeader)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			ShowControlFor(CurrentDataItem);
		}

		#endregion

		#region InnerControl

		void ShowControlFor(SalesHeader salesHeader)
		{
			TradeLaneWithDetailsControl control = null;

			if (salesHeader != null)
			{
				if (!InnerControlsByProductCode.TryGetValue(salesHeader.SalesProductCode, out control))
				{
					control = new TradeLaneWithDetailsControl();
					control.Dock = DockStyle.Fill;
					if (showActualsIfSupported)
					{
						control.ShowActualsIfSupported();
					}
					control.SetDataBinding(salesHeader, null);
					Controls.Add(control);
					control.SetupExpiryContextMenu();

					InnerControlsByProductCode[salesHeader.SalesProductCode] = control;
				}
				else
				{
					control.SetDataBinding(salesHeader, null);
				}
			}

			if (CurrentlyVisibleInnerControl != control)
			{
				SuspendLayout();
				try
				{
					if (CurrentlyVisibleInnerControl != null)
					{
						UnhookDoubleClickedEvent(CurrentlyVisibleInnerControl);
						CurrentlyVisibleInnerControl.Visible = false;
					}

					CurrentlyVisibleInnerControl = control;

					if (CurrentlyVisibleInnerControl != null)
					{
						CurrentlyVisibleInnerControl.Visible = true;
						HookDoubleClickedEvent(CurrentlyVisibleInnerControl);
					}
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		protected TradeLaneWithDetailsControl CurrentlyVisibleInnerControl;
		readonly Dictionary<ZString, TradeLaneWithDetailsControl> InnerControlsByProductCode = new Dictionary<ZString, TradeLaneWithDetailsControl>();

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			foreach (var control in InnerControlsByProductCode.Values)
			{
				control.Dispose();
			}
			InnerControlsByProductCode.Clear();

			base.Dispose(disposing);
		}

		#endregion

		#region Trade Lane Grid Double Click

		void HookDoubleClickedEvent(TradeLaneWithDetailsControl tradeLaneControl)
		{
			if (tradeLaneControl != null && tradeLaneControl.TradeLanesGrid != null)
			{
				tradeLaneControl.TradeLanesGrid.MouseDoubleClick += TradeLanesGrid_DoubleClick;
			}
		}

		void UnhookDoubleClickedEvent(TradeLaneWithDetailsControl tradeLaneControl)
		{
			if (tradeLaneControl != null && tradeLaneControl.TradeLanesGrid != null)
			{
				tradeLaneControl.TradeLanesGrid.MouseDoubleClick -= TradeLanesGrid_DoubleClick;
			}
		}

		void TradeLanesGrid_DoubleClick(object sender, MouseEventArgs e)
		{
			if (TradeLaneGridRowDoubleClicked != null)
			{
				var grid = (ZGrid)sender;
				if (grid.HitTest(e.X, e.Y).Row > -1)
				{
					var sales = grid.ListManager != null ? grid.ListManager.GetCurrent() as EntitySalesWrapper : null;
					TradeLaneGridRowDoubleClicked(this, new EntitySalesEventArgs(sales));
				}
			}
		}

		public event EventHandler<EntitySalesEventArgs> TradeLaneGridRowDoubleClicked;

		#endregion
	}

	public class EntitySalesEventArgs : EventArgs
	{
		public EntitySalesEventArgs(EntitySalesWrapper entitySales)
		{
			this.EntitySales = entitySales;
		}

		public readonly EntitySalesWrapper EntitySales;
	}
}
