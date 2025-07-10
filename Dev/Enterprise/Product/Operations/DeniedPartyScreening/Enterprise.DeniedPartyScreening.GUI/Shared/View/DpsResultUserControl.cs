using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class DpsResultUserControl : ZUserControl
	{
		public DpsResultUserControl()
		{
			InitializeComponent();
		}

		DpsResultWinModel ResultWinModel { get; set; }

		ScreenedPartyListItemUserControl SelectedScreenedPartyListItemUserControl { get; set; }

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (DataSource is DpsResultWinModel resultWinModel)
			{
				ResultWinModel = resultWinModel;

				SetDataBindingForControl();

				ResultWinModel.RegisterNotifyPropertyChangeEvent(nameof(DpsResultWinModel.ScreenedParties), UpdateScreenedPartiesList, clearPreviousEvents: true);
				ResultWinModel.RegisterNotifyPropertyChangeEvent(nameof(DpsResultWinModel.ScreenedPartiesCount), UpdatePartiesCount, clearPreviousEvents: true);
				ResultWinModel.RegisterNotifyPropertyChangeEvent(nameof(DpsResultWinModel.SelectedScreenedParty), UpdateSelectedParty, clearPreviousEvents: true);
			}
		}

		void UpdateScreenedPartiesList()
		{
			if (ResultWinModel == null)
			{
				return;
			}

			ScreenedItemsPanel.Controls.RemoveAndDisposeAll();

			foreach (var party in ResultWinModel.ScreenedParties)
			{
				var control = new ScreenedPartyListItemUserControl();
				control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				control.SetDataBinding(party, "");
				ScreenedItemsPanel.Controls.Add(control);
				control.SetUserControlClickable(control.ContentPanel, ScreenedPartyListItemUserControl_Click, GetRawBackAndBorderColor);
			}
		}

		void UpdatePartiesCount()
		{
			if (ResultWinModel == null || DpsResultTableLayoutPanel.ColumnCount == 1)
			{
				return;
			}

			MatchesNumberLabel.Text = ResultWinModel.ScreenedPartiesCount.ToString();
			ShowMoreUserControl.VerticalScreeningPartiesNumberLabel.Text = ResultWinModel.ScreenedPartiesCount.ToString();
		}

		void UpdateSelectedParty()
		{
			if (ResultWinModel == null)
			{
				return;
			}

			foreach (var itemUserControl in ScreenedItemsPanel.Controls.OfType<ScreenedPartyListItemUserControl>())
			{
				if (itemUserControl.CurrentDataItem is ScreenedPartyWinModel screenedPartyWinModel && screenedPartyWinModel == ResultWinModel.SelectedScreenedParty)
				{
					if (itemUserControl != SelectedScreenedPartyListItemUserControl)
					{
						SelectedScreenedPartyListItemUserControl?.SetUnselected(SelectedScreenedPartyListItemUserControl.ContentPanel);
						ScreenedPartyControl.SetDataBinding(ResultWinModel.SelectedScreenedParty, "");
						SelectedScreenedPartyListItemUserControl = itemUserControl;
						SelectedScreenedPartyListItemUserControl.SetSelected(SelectedScreenedPartyListItemUserControl.ContentPanel);
					}

					return;
				}
			}
		}

		void SetDataBindingForControl()
		{
			if (ResultWinModel == null)
			{
				return;
			}

			UpdateScreenedPartiesList();
			UpdateSelectedParty();

			if (ResultWinModel.ScreenedParties.Count <= 1)
			{
				DpsResultTableLayoutPanel.Controls.Remove(ScreenedPartyLeftPanel);
				DpsResultTableLayoutPanel.ColumnCount = 1;
			}
			else
			{
				InitScreenedPartiesListControl();
			}
		}

		(Color, Color) GetRawBackAndBorderColor(ZUserControl control)
		{
			if (control is ScreenedPartyListItemUserControl screenedPartyListItemUserControl
				&& SelectedScreenedPartyListItemUserControl == screenedPartyListItemUserControl)
			{
				return (WinformConstants.SelectedColor, WinformConstants.BorderColor);
			}

			return (WinformConstants.UnselectedColor, WinformConstants.UnselectedColor);
		}

		void ScreenedPartyListItemUserControl_Click(object sender, EventArgs e)
		{
			if (sender is ScreenedPartyListItemUserControl screenedPartyListItemUserControl
				&& screenedPartyListItemUserControl.CurrentDataItem is ScreenedPartyWinModel screenedPartyWinModel
				&& ResultWinModel != null
				&& ResultWinModel.SelectedScreenedParty != screenedPartyWinModel)
			{
				ResultWinModel.SelectedScreenedParty = screenedPartyWinModel;
			}
		}

		bool isCollapsed = true;
		ShowMoreUserControl showMoreUserControl;
		ShowMoreUserControl ShowMoreUserControl
		{
			get
			{
				if (showMoreUserControl == null)
				{
					showMoreUserControl = new ShowMoreUserControl();
					showMoreUserControl.SwitchBarPictureBox.Click += SwitchBarPictureBox_Click;
					showMoreUserControl.Dock = DockStyle.Fill;
					showMoreUserControl.TabStop = false;
				}

				return showMoreUserControl;
			}
		}

		protected void SwitchBarPictureBox_Click(object sender, EventArgs e)
		{
			isCollapsed = !isCollapsed;
			InitScreenedPartiesListControl();
		}

		void InitScreenedPartiesListControl()
		{
			ScreenedPartiesLabel.Text = ResultWinModel.ScreenedPartiesText;

			if (isCollapsed)
			{
				DpsResultTableLayoutPanel.ColumnStyles[0].Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
				DpsResultTableLayoutPanel.Controls.Remove(ScreenedPartyLeftPanel);
				DpsResultTableLayoutPanel.Controls.Add(ShowMoreUserControl);
				ShowMoreUserControl.VerticalScreenedPartiesLabel.Text = ResultWinModel.ScreenedPartiesText;
			}
			else
			{
				DpsResultTableLayoutPanel.ColumnStyles[0].Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
				DpsResultTableLayoutPanel.Controls.Remove(ShowMoreUserControl);
				DpsResultTableLayoutPanel.Controls.Add(ScreenedPartyLeftPanel);
			}

			UpdatePartiesCount();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			ScreenedPartyLeftPanel.Dispose();
			showMoreUserControl?.Dispose();
		}
	}
}
