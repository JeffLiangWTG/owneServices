using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class WiseRatesSearchResultsChartUserControl : ZUserControl
	{
		public WiseRatesSearchResultsChartUserControl()
		{
			InitializeComponent();
			lblErrorsWarnings.LinkClicked += ShowWarningsLink;
			lblRawData.LinkClicked += ViewRawResponses;

			plotPanel.AllowOutsideOfParent();
		}

		WiseRatesSearchResultViewModel model;

		void ViewRawResponses(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (model == null)
			{
				return;
			}
			WiseRatesGUIHelper.ViewJSONWithDeveloperAuthentication(model.RawData);
		}

		void ShowWarningsLink(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (model == null)
			{
				return;
			}

			var sb = new ZStringBuilder();

			model.ErrorsOrWarnings.ToList().ForEach(x => sb.Append(x));
			Globals.Message.ShowInformation(sb.ToStringWithNewLineBetweenAppends(), model.MultilingualRatesSearchErrorsOrWarnings);
		}

		public void DisplayResults(RatesSearchResponseDTO dataSource, IEnumerable<string> warnings)
		{
			model = new WiseRatesSearchResultViewModel(dataSource, warnings);
#if !WINZOR
			var yPosition = 0;

			rowsPanel.Controls.Clear();

			foreach (var ratesServiceProviderOutcome in model.RatesServiceProviderOutcomes)
			{
				var row = new ProviderOutcomeRowControl(ratesServiceProviderOutcome);
				row.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, yPosition);
				yPosition += 20;
				rowsPanel.Controls.Add(row);
			}
#endif
			lblErrorsWarnings.Text = model.MultilingualRatesSearchErrorsOrWarnings;
			lblRawData.Text = model.MultilingualRawData;
			lblSearchResults.Visible = true;
			lblErrorsWarnings.Visible = true;
			lblRawData.Visible = true;
		}

		public void Reset()
		{
			DisplayResults(null, Enumerable.Empty<string>());
		}

		protected override void Dispose(bool disposing)
		{
			model = null;

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
