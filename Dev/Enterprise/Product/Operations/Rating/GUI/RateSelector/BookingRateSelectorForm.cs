using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.UIControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector
{
	public partial class BookingRateSelectorForm : ZChildForm
	{
		public BookingRateSelectorForm(BookingRatesViewModel viewModel)
		{
			InitializeComponent();
			ViewModel = Argument.NotNull(viewModel, nameof(viewModel));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

				var rsc = new RateSelectorControl(ViewModel);
				rsc.Dock = DockStyle.Fill;
				tableLayoutPanel.Controls.Add(rsc, 0, 0);
			}
		}

		void ViewModelOnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(BookingRatesViewModel.SelectedRate))
			{
				btnBook.Enabled = ViewModel.SelectedRate != null;
			}
		}

		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		void BtnBookClick(object sender, EventArgs e)
		{
			SelectedBookingRate = ViewModel.SelectedRate?.Rate;
			DialogResult = DialogResult.OK;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();

				if (ViewModel != null)
				{
					ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
					ViewModel.Dispose();
					ViewModel = null;
				}
			}

			base.Dispose(disposing);
		}

		public BookingRatesViewModel ViewModel { get; private set; }
		public IBookingRate SelectedBookingRate { get; private set; }
	}
}
