using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	[DefaultDataSourceBindingMember(null)]
	public partial class DynamicTradeLanesControl : ZUserControl
	{
		public DynamicTradeLanesControl()
		{
			InitializeComponent();
		}

		#region DataBinding

		public new OpportunitySalesValueAnalysis CurrentDataItem
		{
			get { return (OpportunitySalesValueAnalysis)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ShowControlFor(CurrentDataItem);
			}
		}

		#endregion

		#region InnerControl

		void ShowControlFor(OpportunitySalesValueAnalysis salesValueAnalysis)
		{
			var salesHeader = salesValueAnalysis.SalesHeader;

			if (salesHeader != null)
			{
				TradeLanesControl control = null;

				if (salesHeader != null)
				{
					if (!InnerControlsByProductPk.TryGetValue(salesHeader.SalesProduct.PK, out control))
					{
						control = TradeLanesControl.New(salesHeader.SalesProduct);
						control.Dock = DockStyle.Fill;
						control.SetDataBinding(salesHeader, "");
						Controls.Add(control);

						InnerControlsByProductPk[salesHeader.SalesProduct.PK] = control;
					}
					else if (control.CurrentDataItem != salesHeader)
					{
						control.SetDataBinding(salesHeader, "");
					}
				}

				if (CurrentlyVisibleInnerControl != control)
				{
					SuspendLayout();
					try
					{
						if (CurrentlyVisibleInnerControl != null)
						{
							CurrentlyVisibleInnerControl.Visible = false;
						}

						CurrentlyVisibleInnerControl = control;

						if (CurrentlyVisibleInnerControl != null)
						{
							CurrentlyVisibleInnerControl.Visible = true;
						}
					}
					finally
					{
						ResumeLayout();
					}
				}
				else
				{
					if (CurrentlyVisibleInnerControl != null)
					{
						CurrentlyVisibleInnerControl.Visible = true;
					}
				}
			}
			else
			{
				if (CurrentlyVisibleInnerControl != null)
				{
					CurrentlyVisibleInnerControl.Visible = false;
				}
			}
		}

		protected TradeLanesControl CurrentlyVisibleInnerControl;
		readonly Dictionary<ZGuid, TradeLanesControl> InnerControlsByProductPk = new Dictionary<ZGuid, TradeLanesControl>();

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

			foreach (var control in InnerControlsByProductPk.Values)
			{
				control.Dispose();
			}
			InnerControlsByProductPk.Clear();

			base.Dispose(disposing);
		}

		#endregion
	}
}
