using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class RateChooserSummaryCardsControl : TemplateBasedControl
	{
		public RateChooserSummaryCardsControl()
		{
			InitializeComponent();
		}

		public void SetViewModel(RateChooserViewModel viewModel)
		{
			ViewModel = viewModel;
			BindingSource.DataSource = viewModel;
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new RateChooserSummaryTemplate(ViewModel);
		}

		protected override IEnumerable<object> GetItemsData()
		{
			return ViewModel.SelectedRows;
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (e.PropertyName == nameof(ViewModel.SelectedRows))
			{
				pnlItemsContainer.SuspendLayout();
				RenderLayout();
				pnlItemsContainer.ResumeLayout();
			}
		}

		protected override bool RenderLayoutOnCurrentDataItemChanged => false;

		public RateChooserViewModel ViewModel { get; private set; }
	}
}
