using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgAgentRelationshipFilterControl : ZFilterStripControl
	{
		public OrgAgentRelationshipFilterControl(IBusinessObjectCollection gridCollection, OrgAgentRelationshipFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitialiseForm();
		}

		protected void InitialiseForm()
		{
			SuspendLayout();
			try
			{
				InitializeComponent();
				BindingSource.DataSource = FilterBusinessObject;
				ProfitShareControl.SetAgreementGrid(FilteredGrid);
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));

				if (Splitter != null)
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - Splitter.SplitPosition - ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top, false);
				}

				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (FilteredGrid != null)
			{
				var innerSender = ((KSplitter)sender);
				int maxSplitterPosition = Math.Max(innerSender.MinSize, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				if (innerSender.SplitPosition > maxSplitterPosition)
				{
					innerSender.SplitPosition = maxSplitterPosition;
				}
			}
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			this.FilteredGrid.ListManager.CurrentChanged += new EventHandler(this.ListManager_CurrentChanged);
			this.FilteredGrid.ListManager.PositionChanged += new EventHandler(this.ListManager_PositionChanged);
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			RefreshProfitShareDetails();
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			RefreshProfitShareDetails();
		}

		void RefreshProfitShareDetails()
		{
			if (FilteredGrid.CurrentRowIndex >= 0)
			{
				var orgAgentRelationship = ((OrgAgentRelationshipCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
				((OrgAgentRelationshipFilterBusinessObject)FilterBusinessObject).RefreshProfitShareDetails(orgAgentRelationship);
			}
			else
			{
				((OrgAgentRelationshipFilterBusinessObject)FilterBusinessObject).RefreshProfitShareDetails(null);
			}
		}
	}
}
