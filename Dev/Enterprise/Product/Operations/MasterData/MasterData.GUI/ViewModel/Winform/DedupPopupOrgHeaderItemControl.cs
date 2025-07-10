using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DedupPopupOrgHeaderItemControl : ZUserControl
	{
		internal readonly DedupPopupBizoDataSource parentDataSource;
		internal readonly DeduplicationResultsListDataSource resultItem;

		public DedupPopupOrgHeaderItemControl(DedupPopupBizoDataSource parentDataSource, DeduplicationResultsListDataSource resultItem)
		{
			InitializeComponent();
			confidenceScorePanel.AllowOverlap(detailTableLayoutPanel);
			this.parentDataSource = parentDataSource;
			this.resultItem = resultItem;
		}

#if DEBUG
		protected virtual
#endif
		DedupPopupOrgHeaderControl HeaderControl => Parent.Parent as DedupPopupOrgHeaderControl;

		Color UnselectedItemColor => Color.White;

		void SetContainerInnerBackColor(Color color) => containerInnerPanel.BackColor = color;

		void SetContainerBorderColor(Color color) => containerPanel.BackColor = color;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetDataBinding(resultItem, string.Empty);
			PopulateDisplayedResultItem();
			BindEventsToControlsAndSetTooltip(this);
		}

		void PopulateDisplayedResultItem()
		{
			fullNameCaptionLabel.Text = resultItem.FullNameCaption;
			fullNameValueLabel.Text = resultItem.FullName;
			codeCaptionLabel.Text = resultItem.InfoCaption;
			codeValueLabel.Text = resultItem.Info;

			confidenceScorePanel.BackColor = ColorHelper.GetConfidenceRatingColor(resultItem.Confidence);
		}

		protected void DoubleClickGrid(object sender, EventArgs ev)
		{
			parentDataSource.SelectedResult = resultItem;
			HeaderControl.ActiveItemControl = this;
			OpenDetailsForm();
		}

		protected void MouseEnterGrid(object sender, EventArgs ev)
		{
			SetContainerInnerBackColor(ColorHelper.HighlightedItemColor);
			SetContainerBorderColor(ColorHelper.SelectedItemColor);
		}

		protected void MouseLeaveGrid(object sender, EventArgs ev)
		{
			SetContainerInnerBackColor(parentDataSource.SelectedResult == resultItem ? ColorHelper.HighlightedItemColor : Color.White);
			SetContainerBorderColor(parentDataSource.SelectedResult == resultItem ? ColorHelper.SelectedItemColor : Color.White);
		}

		protected void ClickGrid(object sender, EventArgs ev)
		{
			if (HeaderControl.ActiveItemControl != null)
			{
				if (HeaderControl.ActiveItemControl == this)
				{
					SetContainerInnerBackColor(ColorHelper.HighlightedItemColor);
					parentDataSource.SelectedResult = null;
					HeaderControl.ActiveItemControl = null;
					return;
				}

				HeaderControl.ActiveItemControl.SetContainerInnerBackColor(UnselectedItemColor);
				HeaderControl.ActiveItemControl.SetContainerBorderColor(UnselectedItemColor);
			}

			SetContainerBorderColor(ColorHelper.SelectedItemColor);
			parentDataSource.SelectedResult = resultItem;
			HeaderControl.ActiveItemControl = this;
		}

		protected virtual void OpenDetailsForm() => parentDataSource.OpenDetailsFormAction();

		void BindEventsToControlsAndSetTooltip(Control control)
		{
			foreach (Control childControl in control.Controls)
			{
				childControl.Click += ClickGrid;
				childControl.DoubleClick += DoubleClickGrid;
				childControl.MouseEnter += MouseEnterGrid;
				childControl.MouseLeave += MouseLeaveGrid;

				if (childControl is ZLabel || childControl is KTableLayoutPanel)
				{
					scoreToolTip.SetToolTip(childControl, resultItem.ToolTip);
				}

				BindEventsToControlsAndSetTooltip(childControl);
			}
		}
	}
}
