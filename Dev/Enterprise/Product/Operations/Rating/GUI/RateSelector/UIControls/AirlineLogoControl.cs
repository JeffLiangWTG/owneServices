using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class AirlineLogoControl : ViewModelBasedControl
	{
		public AirlineLogoControl()
		{
			InitializeComponent();

			// This enables the transparency on the label
			carrierCodeLabel.Parent = fallbackPictureBox;
#if DEBUG
			TypeDescriptor.AddAttributes(carrierCodeLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (CurrentViewModel != null)
			{
				zPictureBox2.Image = CurrentViewModel.AirlineIconBitmap;
			}

			zPictureBox2.Visible = zPictureBox2.Image != null;

			fallbackPictureBox.Visible = !zPictureBox2.Visible;
			carrierCodeLabel.Visible = fallbackPictureBox.Visible;
		}

		protected BookingEngineRateViewModel CurrentViewModel => BindingSource.DataSource as BookingEngineRateViewModel;

#if DEBUG
		public bool LogoVisibleForTest => zPictureBox2.Visible;
		public bool FallbackLogoVisibleForTest => fallbackPictureBox.Visible;
#endif
	}
}
