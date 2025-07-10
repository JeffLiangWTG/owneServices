using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class ChargesControl : TemplateBasedControl, IItemTemplateControl
	{
		public ChargesControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblGroupName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalPriceCurrency, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalPriceString, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return ViewModel?.ChargeGroupsView;
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (ViewModel != null)
			{
				cbIsAnyOptionalActive.Enabled = ViewModel.IsAnyOptionalActiveEnabled;
				cbIsAnyOptionalActive.Visible = ViewModel.IsActiveVisibility;
				lblTotalPriceCurrency.Visible = ViewModel.TotalPriceVisibility;
				lblTotalPriceString.Visible = ViewModel.TotalPriceVisibility;
				pbErrorIcon.Visible = ViewModel.TotalPriceErrorVisibility;

				// As PictureBox is not bindable, we have to set it's image here.
				pbErrorIcon.Image = RateChooserImageRepository.ErrorIcon;
			}
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			switch (e.PropertyName)
			{
				case nameof(ViewModel.TotalPriceVisibility):
					lblTotalPriceString.Visible = ViewModel.TotalPriceVisibility;
					lblTotalPriceCurrency.Visible = ViewModel.TotalPriceVisibility;
					break;

				case nameof(ViewModel.TotalPriceErrorVisibility):
					pbErrorIcon.Visible = ViewModel.TotalPriceErrorVisibility;
					break;

				case nameof(ViewModel.IsAnyOptionalActive):
					cbIsAnyOptionalActive.Checked = ViewModel.IsAnyOptionalActive;
					break;

				case nameof(ViewModel.TotalPriceString):
					lblTotalPriceString.Text = ViewModel.TotalPriceString;
					break;
			}
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ChargesChargeGroupTemplate();
		}

		#region Tooltips

		void pbErrorIcon_MouseHover(object sender, EventArgs e)
		{
			pbErrorIcon.SetTooltip(ViewModel?.TotalPriceErrorString);
		}
		#endregion

		#region IItemTemplateControl

		public void DataBind(object data)
		{
			if (data is ChargesViewModel viewModel)
			{
				BindingSource.DataSource = viewModel;
			}
		}

		public void ClearSelection()
		{
		}

		public bool IsSelected { get; set; }
		public EventHandler SelectionChanged { get; set; }

		#endregion

		ChargesViewModel ViewModel => CurrentDataItem as ChargesViewModel;
	}
}
