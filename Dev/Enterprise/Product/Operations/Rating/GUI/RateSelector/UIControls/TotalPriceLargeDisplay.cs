using System.ComponentModel;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class TotalPriceLargeDisplay : ViewModelBasedControl
	{
		public TotalPriceLargeDisplay()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(totalPriceStringLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			if (CurrentViewModel != null)
			{
				warningPictureBox.Visible = !string.IsNullOrEmpty(CurrentViewModel.TotalPriceError);
				warningPictureBox.SetTooltip(CurrentViewModel.TotalPriceError);
			}
		}

		BookingEngineRateViewModel CurrentViewModel => BindingSource.Current as BookingEngineRateViewModel;

#if DEBUG
		public ZPictureBox warningPictureBoxForTest => warningPictureBox;
#endif
	}
}
