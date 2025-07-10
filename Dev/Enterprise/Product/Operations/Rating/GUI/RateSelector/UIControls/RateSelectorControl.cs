using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class RateSelectorControl : ZUserControl
	{
		public RateSelectorControl()
		{
			InitializeComponent();
			SwitchToCardsView();

			tbLogs.CharacterCasing = CharacterCasing.Normal;

#if DEBUG
			TypeDescriptor.AddAttributes(lblStatusText, new SuppressControlRequiresTextBasherAttribute());
#endif

			// Control is read-only but still enabled so users can highlight
			// the text (for copy/paste into issue reports). Therefore we
			// forcibly set the back colour to avoid unsightly changes on focus
			var darkGray = Color.FromArgb(51, 51, 51);
			tbLogs.ColorChanger.ForceBackColor(darkGray);
		}

		public RateSelectorControl(SortableRatesViewModel viewModel) : this()
		{
			ViewModel = viewModel;
			BindingSource.DataSource = viewModel;
			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			if (viewModel is NonContainerizedRatesViewModel nonContainerizedViewModel)
			{
				var cardsControl = new RatesCardControl(viewModel);
				pnlCardsContainer.Controls.Add(cardsControl);
				cardsControl.Dock = DockStyle.Fill;
			}
			else if (viewModel is BookingRatesViewModel bookingRatesViewModel)
			{
				var cardsControl = new RatesCardControl(bookingRatesViewModel);
				pnlCardsContainer.Controls.Add(cardsControl);
				cardsControl.Dock = DockStyle.Fill;
			}
			else if (viewModel is ContainerizedRatesViewModel containerizedRatesViewModel)
			{
				var cardsControl = new ContainerizedRatesCardControl(containerizedRatesViewModel);
				pnlCardsContainer.Controls.Add(cardsControl);
				cardsControl.Dock = DockStyle.Fill;
			}
		}

		void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ViewModel.LogText))
			{
				tbLogs.InvokeSafe(UpdateLogsTextBox);
			}

			void UpdateLogsTextBox()
			{
				if (tbLogs.Visible && tbLogs.IsHandleCreated && !tbLogs.IsDisposed)
				{
					tbLogs.Text = ViewModel.LogText;
				}
			}
		}

		readonly SortableRatesViewModel ViewModel;

		void btnWarnings_Click(object sender, EventArgs e)
		{
			SwitchToWarningsView();
		}

		void btnCardView_Click(object sender, EventArgs e)
		{
			SwitchToCardsView();
		}

		void SwitchToCardsView()
		{
			btnCardView.BackColor = SystemColors.ControlDark;
			btnWarnings.BackColor = SystemColors.Control;
			tbLogs.Visible = false;
			pnlCardsContainer.Visible = true;
			pnlCardsContainer.Dock = DockStyle.Fill;
		}

		void SwitchToWarningsView()
		{
			btnCardView.BackColor = SystemColors.Control;
			btnWarnings.BackColor = SystemColors.ControlDark;
			tbLogs.Text = ViewModel?.LogText;
			tbLogs.Visible = true;
			pnlCardsContainer.Visible = false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ViewModel != null)
				{
					ViewModel.PropertyChanged -= ViewModelPropertyChanged;
				}

				components?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
