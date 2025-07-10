using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser
{
	public partial class RateChooserCardsControl : TemplateBasedControl
	{
		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		public RateChooserCardsControl(RateChooserViewModel parentFormViewModel, ChooserContainerCommodityViewModel viewModel) : base()
		{
			InitializeComponent();

			BindingSource.DataSource = viewModel;
			ContainerViewModel = viewModel;
			ParentFormViewModel = parentFormViewModel;
			ParentFormViewModel.AllRatesRefreshed += AllRatesRefreshedOnParentForm;

#if DEBUG
			TypeDescriptor.AddAttributes(lblNoRatesFound, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new RateChooserCardTemplate(ContainerViewModel);
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return ContainerViewModel?.Rates;
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (ContainerViewModel == null)
			{
				return;
			}

			lblNoRatesFound.Visible = ContainerViewModel.NoRateVisibility;
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (ContainerViewModel == null)
			{
				return;
			}

			switch (e.PropertyName)
			{
				case nameof(ChooserContainerCommodityViewModel.NoRateVisibility):
					lblNoRatesFound.Visible = ContainerViewModel.NoRateVisibility;
					break;

				case nameof(ChooserContainerCommodityViewModel.RateVisibility):
					pnlItemsContainer.Visible = ContainerViewModel.RateVisibility;
					break;
			}
		}

		protected override bool RenderLayoutOnCurrentDataItemChanged => false;

		void AllRatesRefreshedOnParentForm(object sender, EventArgs e)
		{
			pnlItemsContainer.SuspendLayout();
			RenderLayout();
			pnlItemsContainer.ResumeLayout();
		}

		ChooserContainerCommodityViewModel ContainerViewModel { get; }
		RateChooserViewModel ParentFormViewModel { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ParentFormViewModel.AllRatesRefreshed -= AllRatesRefreshedOnParentForm;

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
