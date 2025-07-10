using System;
using System.Windows.Forms;

namespace Enterprise.DeniedPartyScreening.GUI.Shared.View
{
	public partial class SummaryViewItemUserControl : UserControl
	{
		public SummaryViewItemUserControl()
		{
			InitializeComponent();
		}

		SummaryItemViewModel _itemVm;
		public SummaryItemViewModel ViewModel
		{
			get => _itemVm;
			set
			{
				_itemVm = value;
				ViewModelChanged();
			}
		}

		void ViewModelChanged()
		{
			nameLinkLabel.Text = ViewModel.Name;
			statusLabel.Text = ViewModel.ScreeningStatus;
			statusLabel.ForeColor = ColorConverter.MapScreeningStatus(ViewModel.ScreeningStatus);
		}

		void nameLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ViewModel.OpenPartyFormCommand.Execute(null);
		}

		void nameLinkLabel_SizeChanged(object sender, EventArgs e)
		{
			var nameLinkLabelOriginalHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			var thisControlOriginalHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(32);

			var heightChangeFromText = nameLinkLabel.Size.Height - nameLinkLabelOriginalHeight;
			if (heightChangeFromText > 1)
			{
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(Size.Width, thisControlOriginalHeight + heightChangeFromText, false);
			}
		}

		void SummaryViewItemUserControl_SizeChanged(object sender, EventArgs e)
		{
			var availableWidth = statusLabel.Location.X - nameLinkLabel.Location.X;
			nameLinkLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(availableWidth, 0, false);
		}
	}
}
