using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class DynamicTradedSalesAnalysisControl : ZUserControl
	{
		public DynamicTradedSalesAnalysisControl()
		{
			InitializeComponent();
		}

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

		#region ViewpointgOrg

		public OrgHeader ViewpointOrg
		{
			get { return viewpointOrg; }
		}
		OrgHeader viewpointOrg;

		public void SetViewpointOrg(OrgHeader org)
		{
			viewpointOrg = org;
			if (CurrentlyVisibleInnerControl != null)
			{
				CurrentlyVisibleInnerControl.SalesAnalysis.SetViewpointOrg(org);
			}
		}

		public TradedSalesAnalysisControl InnerControl
		{
			get { return CurrentlyVisibleInnerControl; }
		}

		#endregion

		#region InnerControl

		void ShowControlFor(SalesHeader salesHeader)
		{
			TradedSalesAnalysisControl control = null;

			if (salesHeader != null)
			{
				if (!InnerControlsByProductCode.TryGetValue(salesHeader, out control))
				{
					var analysis = new TradedSalesAnalysis(salesHeader);
					analysis.SetViewpointOrg(ViewpointOrg);
					control = new TradedSalesAnalysisControl(analysis);
					control.Dock = DockStyle.Fill;
					Controls.Add(control);

					InnerControlsByProductCode[salesHeader] = control;
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
						CurrentlyVisibleInnerControl.SalesAnalysis.SetViewpointOrg(ViewpointOrg);
					}
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		protected TradedSalesAnalysisControl CurrentlyVisibleInnerControl;
		readonly Dictionary<SalesHeader, TradedSalesAnalysisControl> InnerControlsByProductCode = new Dictionary<SalesHeader, TradedSalesAnalysisControl>();

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
	}
}
