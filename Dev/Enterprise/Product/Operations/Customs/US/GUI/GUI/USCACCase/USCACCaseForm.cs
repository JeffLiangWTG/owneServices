using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCACCaseForm : ZTemplateForm
	{
		public USCACCaseForm()
		{
		}

		public USCACCaseForm(USCACCase adc)
			: base(adc)
		{
		}

		USCACCase ACCase
		{
			get { return (USCACCase)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "AD/CVD Case"; }
		}

		public override string FormVerb
		{
			get { return "View"; }
		}

		protected override bool AllowActionDataMenuItem
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (!TariffsTabPage.Text.Contains("("))
			{
				this.TariffsTabPage.Text += "(" + ACCase.CaseTariffs.Count + ")";
			}

			if (!RatesTabPage.Text.Contains("("))
			{
				this.RatesTabPage.Text += "(" + ACCase.CaseRates.Count + ")";
			}

			if (!EventsTabPage.Text.Contains("("))
			{
				this.EventsTabPage.Text += "(" + ACCase.CaseEvents.Count + ")";
			}

			if (!BondCashTabPage.Text.Contains("("))
			{
				this.BondCashTabPage.Text += "(" + ACCase.BondCashIndicators.Count + ")";
			}

			if (!LiqSuspensionsTabPage.Text.Contains("("))
			{
				this.LiqSuspensionsTabPage.Text += "(" + ACCase.LiqSuspensions.Count + ")";
			}
		}
	}
}
