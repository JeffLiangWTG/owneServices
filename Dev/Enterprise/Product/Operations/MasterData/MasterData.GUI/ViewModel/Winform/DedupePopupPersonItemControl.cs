using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DedupePopupPersonItemControl : ZUserControl
	{
		internal readonly DedupPopupBizoDataSource parentDataSource;
		internal readonly DeduplicationResultsListDataSource resultItem;

		public DedupePopupPersonItemControl(DedupPopupBizoDataSource parentDataSource, DeduplicationResultsListDataSource resultItem)
		{
			InitializeComponent();

			this.parentDataSource = parentDataSource;
			this.resultItem = resultItem;
		}

#if DEBUG
		protected virtual
#endif
		DedupePopupPersonControl PersonControl => Parent.Parent as DedupePopupPersonControl;

		void SetContainerInnerBackColor(Color color) => containerInnerPanel.BackColor = color;

		void SetContainerBorderColor(Color color) => containerPanel.BackColor = color;

		Color UnselectedItemColor => Color.White;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetPanelContent();
			BindEventsToControlsAndSetTooltip(this);
		}

		void SetPanelContent()
		{
			mainInfoLabel.Text = resultItem.MainInfo;
			personTypeLabel.Text = resultItem.PersonType;
			fullnameLabel.Text = resultItem.FullName;

			var rowCount = contentTableLayoutPanel.RowCount;

			if (resultItem.EmailPanelVisible)
			{
				var emailLabel = CreateLabel("emailLabel", resultItem.Email, false);
				contentTableLayoutPanel.Controls.Add(emailLabel, 1, ++rowCount - 1);
			}

			if (resultItem.PhonePanelVisible)
			{
				var phoneLabel = CreateLabel("phoneLabel", resultItem.Phone, false);
				contentTableLayoutPanel.Controls.Add(phoneLabel, 1, ++rowCount - 1);
			}

			if (resultItem.AssociationsPanelVisible)
			{
				var infoCaptionLabel = CreateLabel("infoCaptionLabel", resultItem.InfoCaption, true);
				contentTableLayoutPanel.Controls.Add(infoCaptionLabel, 0, ++rowCount - 1);

				var associationsLabel = CreateLabel("associationsLabel", resultItem.Associations, false);
				contentTableLayoutPanel.Controls.Add(associationsLabel, 1, rowCount - 1);
			}

			confidenceScorePanel.BackColor = ColorHelper.GetConfidenceRatingColor(resultItem.Confidence);
		}

		ZLabel CreateLabel(string name, string text, bool setBold)
		{
			var label = new ZLabel();
			label.IsFontBold = setBold;
			label.Name = name;
			label.AutoEllipsis = true;
			label.Dock = DockStyle.Fill;
			label.Text = text;
			label.MinimumSize = ControlDpiScalingHelper.NewScaledSize(0, 16, true);
			label.Margin = ControlDpiScalingHelper.NewScaledPadding(0, true);
			label.Padding = ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2, true);

			return label;
		}

		protected void DoubleClickGrid(object sender, EventArgs ev)
		{
			parentDataSource.SelectedResult = resultItem;
			PersonControl.ActiveItemControl = this;
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
			if (PersonControl.ActiveItemControl != null)
			{
				if (PersonControl.ActiveItemControl == this)
				{
					SetContainerInnerBackColor(ColorHelper.HighlightedItemColor);
					parentDataSource.SelectedResult = null;
					PersonControl.ActiveItemControl = null;
					return;
				}

				PersonControl.ActiveItemControl.SetContainerInnerBackColor(UnselectedItemColor);
				PersonControl.ActiveItemControl.SetContainerBorderColor(UnselectedItemColor);
			}

			SetContainerBorderColor(ColorHelper.SelectedItemColor);
			parentDataSource.SelectedResult = resultItem;
			PersonControl.ActiveItemControl = this;
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
