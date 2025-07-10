using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ScreenedPartyUserControl : ZUserControl
	{
		public ScreenedPartyUserControl()
		{
			InitializeComponent();
			SaveButton.Text = Enterprise.DeniedPartyScreening.GUI.Res.GetString("6c779742-7d1b-47ad-9a2c-a4dca828334e", "Save");

#if DEBUG
			TypeDescriptor.AddAttributes(NavigationLabel, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(PartyName, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(ParentsDescriptionLabel, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(PotentialMatchesCountLabel, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(PotentialMatchesLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		public ScreenedPartyWinModel ScreenedPartyWinModel { get; private set; }

		protected PotentialMatchListItemUserControl SelectedPotentialMatchListItemUserControl { get; private set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is ScreenedPartyWinModel screenedPartyWinModel)
			{
				ScreenedPartyWinModel = screenedPartyWinModel;
				base.SetDataBinding(ScreenedPartyWinModel, "");

				InitControls();
			}
		}

		void InitControls()
		{
			if (ScreenedPartyWinModel == null)
			{
				return;
			}

			ScreeningStatusControl.SetDataBinding(ScreenedPartyWinModel.ScreeningStatusWinModel, "");
			UpdatePotentialMatchItemsList();
			UpdateSelectedPotentialMatchItem();
			UpdateSaveButtonEnableDisable();

			if (ScreenedPartyWinModel.PersistentTotalRecordsCount <= 1)
			{
				NavigationPanel.Visible = false;
			}

			if (ScreenedPartyWinModel.StandAlone)
			{
				MainTableLayoutPanel.RowStyles[3].Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			}

			EntityIcon.Image = ScreenedPartyWinModel.EntityTypeIcon.ToBitmap();
			PartyName.Text = ScreenedPartyWinModel.PartyName;
			ParentsDescriptionLabel.Text = ScreenedPartyWinModel.ParentsDescription;
			NavigationLabel.Text = ScreenedPartyWinModel.NavigationLabel;

			PotentialMatchesLabel.Text = ScreenedPartyWinModel.PotentialMatchesText;
			PotentialMatchesCountLabel.Text = ScreenedPartyWinModel.PotentialMatchWinModelsCount.ToString();

			ScreenedPartyWinModel.RegisterNotifyPropertyChangeEvent(nameof(ScreenedPartyWinModel.SelectedPotentialMatchWinModel), UpdateSelectedPotentialMatchItem, clearPreviousEvents: true);
			ScreenedPartyWinModel.RegisterNotifyPropertyChangeEvent(nameof(ScreenedPartyWinModel.SaveButtonEnabled), UpdateSaveButtonEnableDisable, clearPreviousEvents: true);
		}

		void UpdateSaveButtonEnableDisable()
		{
			if (ScreenedPartyWinModel == null)
			{
				return;
			}

			SaveButton.Visible = !ScreenedPartyWinModel.StandAlone;
			SaveButton.Enabled = ScreenedPartyWinModel.SaveButtonEnabled;
			SaveButton.BackColor = SaveButton.Enabled ? WinformConstants.EnableButtonColor : WinformConstants.DisableButtonColor;
			SaveButton.ForeColor = SaveButton.Enabled ? Color.White : Color.Black;
		}

		void UpdatePotentialMatchItemsList()
		{
			if (ScreenedPartyWinModel == null)
			{
				return;
			}

			PotentialMatchItemsPanel.Controls.RemoveAndDisposeAll();

			foreach (var potentialMatchWinModel in ScreenedPartyWinModel.PotentialMatchWinModels)
			{
				var control = new PotentialMatchListItemUserControl();
				control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				control.SetDataBinding(potentialMatchWinModel, "");
				control.SetUserControlClickable(control.ContentPanel, PotentialMatchListItemUserControl_Click, GetRawBackAndBorderColor);
				PotentialMatchItemsPanel.Controls.Add(control);
			}
		}

		void UpdateSelectedPotentialMatchItem()
		{
			if (ScreenedPartyWinModel == null)
			{
				return;
			}

			foreach (var itemUserControl in PotentialMatchItemsPanel.Controls.OfType<PotentialMatchListItemUserControl>())
			{
				if (itemUserControl.CurrentDataItem is PotentialMatchWinModel potentialMatchWinModel && potentialMatchWinModel == ScreenedPartyWinModel.SelectedPotentialMatchWinModel)
				{
					if (itemUserControl != SelectedPotentialMatchListItemUserControl)
					{
						SelectedPotentialMatchListItemUserControl?.SetUnselected(SelectedPotentialMatchListItemUserControl.ContentPanel);
						PotentialMatchControl.SetDataBinding(ScreenedPartyWinModel.SelectedPotentialMatchWinModel, "");
						SelectedPotentialMatchListItemUserControl = itemUserControl;
						SelectedPotentialMatchListItemUserControl.SetSelected(SelectedPotentialMatchListItemUserControl.ContentPanel);
					}

					return;
				}
			}
		}

		(Color RawBackColor, Color RawBorderColor) GetRawBackAndBorderColor(ZUserControl control)
		{
			if (control is PotentialMatchListItemUserControl potentialMatchListItemUserControl
				&& SelectedPotentialMatchListItemUserControl == potentialMatchListItemUserControl)
			{
				return (WinformConstants.SelectedColor, WinformConstants.BorderColor);
			}

			return (WinformConstants.UnselectedColor, WinformConstants.UnselectedColor);
		}

		protected void PotentialMatchListItemUserControl_Click(object sender, EventArgs e)
		{
			if ((sender as PotentialMatchListItemUserControl)?.CurrentDataItem is PotentialMatchWinModel potentialMatchWinModel
				&& ScreenedPartyWinModel != null
				&& ScreenedPartyWinModel.SelectedPotentialMatchWinModel != potentialMatchWinModel)
			{
				ScreenedPartyWinModel.SelectedPotentialMatchWinModel = potentialMatchWinModel;
			}
		}

		protected void NextButton_Click(object sender, EventArgs e)
		{
			ScreenedPartyWinModel?.NavigateUp();
		}

		protected void PreviousButton_Click(object sender, EventArgs e)
		{
			ScreenedPartyWinModel?.NavigateDown();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (ScreenedPartyWinModel == null || !ScreenedPartyWinModel.SaveButtonEnabled)
			{
				return;
			}

			ScreenedPartyWinModel.ExecuteSaveCommand();
		}
	}
}
