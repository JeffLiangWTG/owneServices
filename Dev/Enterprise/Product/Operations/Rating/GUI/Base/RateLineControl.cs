using System.Linq;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	[DefaultDataSourceBindingMember(null)]
	public partial class RateLineControl : ZUserControl
	{
		public RateLineControl()
		{
			InitializeComponent();
		}

		#region Visible

		public bool CalculatorPanelVisible
		{
			get { return CalculatorPanel.Visible; }
			set { CalculatorPanel.Visible = value; }
		}

		public bool CalculatorPanelCalculatorDropEditVisible
		{
			get { return CalculatorPanel.CalculatorDropEditVisible; }
			set { CalculatorPanel.CalculatorDropEditVisible = value; }
		}

		public bool CalculatorPanelAgentRatesCheckBoxVisible
		{
			get { return CalculatorPanel.AgentRatesCheckBoxVisible; }
			set { CalculatorPanel.AgentRatesCheckBoxVisible = value; }
		}

		#endregion

		public object BindingDataSource
		{
			get { return BindingSource.DataSource; }
			set { BindingSource.DataSource = value; }
		}

		#region Event Handlers

		void OverrideDescCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			var updater = this.BindingSource.DataSource as BulkRateUpdater;
			if (updater != null && OverrideDescCheckBox.Checked)
			{
				var includedEntries = updater.Entries.Cast<RateEntry>().Where(entry => entry.IncludeInUpdate).ToArray();
				if (includedEntries.Any(entry => entry.IsClientRate()) && !Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed)
				{
					Env.Security.ClientRatesChargeDescriptionOverride.ShowError();
					OverrideDescCheckBox.Checked = false;
				}
				if (includedEntries.Any(entry => entry.IsCompanyTariff()) && !Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed)
				{
					Env.Security.CompanyTariffRatesChargeDescriptionOverride.ShowError();
					OverrideDescCheckBox.Checked = false;
				}
				if (includedEntries.Any(entry => entry.IsCosting()) && !Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed)
				{
					Env.Security.CostingRatesChargeDescriptionOverride.ShowError();
					OverrideDescCheckBox.Checked = false;
				}
				if (includedEntries.Any(entry => entry.IsQuote()) && !Env.Security.QuotationChargeDescriptionOverride.IsAllowed)
				{
					Env.Security.QuotationChargeDescriptionOverride.ShowError();
					OverrideDescCheckBox.Checked = false;
				}
				if (includedEntries.Any(entry => entry.IsIntercompanyTariff()) && !Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed)
				{
					Env.Security.IntercompanyTariffsChargeDescriptionOverride.ShowError();
					OverrideDescCheckBox.Checked = false;
				}
			}
		}

		#endregion

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

