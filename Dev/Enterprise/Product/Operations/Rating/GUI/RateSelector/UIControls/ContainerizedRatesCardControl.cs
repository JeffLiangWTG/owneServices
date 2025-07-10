using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class ContainerizedRatesCardControl : ViewModelBasedControl
	{
		public ContainerizedRatesCardControl()
		{
			InitializeComponent();
		}

		public ContainerizedRatesCardControl(ContainerizedRatesViewModel viewModel) : this()
		{
			BindingSource.DataSource = viewModel;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem is ContainerizedRatesViewModel oldModel)
			{
				oldModel.SearchCompleted -= SearchCompletedHandler;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			tabControl.TabPages.Clear();

			if (CurrentDataItem is ContainerizedRatesViewModel newModel)
			{
				newModel.SearchCompleted += SearchCompletedHandler;

				InitTabs(newModel.ContainerGroups);
			}
		}

		void SearchCompletedHandler(object sender, EventArgs e)
		{
			tabControl.TabPages.Clear();
			if (Data != null)
			{
				InitTabs(Data.ContainerGroups);
			}
		}

		void InitTabs(IEnumerable<ContainerGroupViewModel> tabs)
		{
			var tabSelected = false;

			foreach (var viewModel in tabs)
			{
				var tabPage = new ZTabPage();
				tabPage.Text = viewModel.Header;

				tabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
				tabPage.UseVisualStyleBackColor = true;

				var rateCardsControl = new RatesCardControl(viewModel);
				tabPage.Controls.Add(rateCardsControl);
				rateCardsControl.Dock = DockStyle.Fill;

				tabControl.TabPages.Add(tabPage);

				if (viewModel.Rates.Any())
				{
					tabPage.Text = viewModel.Header + " " + Res.GetString("FED690D7-5877-494F-974B-2A937CBABFF2", "- {0} Rate(s)", viewModel.Rates.Count); //SupressCodeSmell Reason = Formatting.
					if (!tabSelected)
					{
						tabControl.SelectedTab = tabPage;
						tabSelected = true;
					}
				}
			}
		}

		ContainerizedRatesViewModel Data => CurrentDataItem as ContainerizedRatesViewModel;
	}
}
