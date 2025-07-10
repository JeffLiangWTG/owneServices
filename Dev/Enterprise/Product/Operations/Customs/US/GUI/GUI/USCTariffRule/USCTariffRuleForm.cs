using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCTariffRuleForm : ZTemplateForm
	{
		public USCTariffRuleForm(USCTariffRule tariffRule)
			: base(tariffRule)
		{
			this.tariffRule = tariffRule;

			this.tariffRule.RegisterChildrenAsEditableChild();

			if (this.tariffRule != null)
			{
				RefreshCaptionAndHideShowTabPages();
				this.tariffRule.U1_RuleCodeInfo.ValueChanged += new EventHandler(U1_RuleCodeInfo_ValueChanged);
				this.tariffRule.U1_TariffInfo.ValueChanged += new EventHandler(U1_TariffInfo_ValueChanged);
				this.tariffRule.U1_TariffToInfo.ValueChanged += new EventHandler(U1_TariffToInfo_ValueChanged);
			}
		}
		readonly USCTariffRule tariffRule;

		void U1_TariffToInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptionAndHideShowTabPages();
		}

		void U1_TariffInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptionAndHideShowTabPages();
		}

		void U1_RuleCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptionAndHideShowTabPages();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			AddTariffRuleUserControl();
		}

		void AddTariffRuleUserControl()
		{
			USCTariffRuleUserControl control = new USCTariffRuleUserControl();
			control.Dock = DockStyle.Fill;
			this.MainTabControl.TabPages[0].Controls.Add(control);
		}

		public override string FormCaption
		{
			get
			{
				StringBuilder caption = new StringBuilder("Tariff Rule");
				if (tariffRule != null)
				{
					if (!tariffRule.FormattedTariff.IsEmpty)
					{
						caption.Append(" ");
						caption.Append(tariffRule.FormattedTariff);

						if (!tariffRule.FormattedTariffTo.IsEmpty)
						{
							caption.Append(" - ");
							caption.Append(tariffRule.FormattedTariffTo);
						}
					}
				}
				return caption.ToString();
			}
		}

		internal ZTemplateTabControl MainTabControlExposedForTesting => MainTabControl;

		void RefreshCaptionAndHideShowTabPages()
		{
			SuspendLayout();
			try
			{
				AssociatedTarrifsTabPage.TabVisible = tariffRule.EligibleForAssociatedSecondaryTariffs;
				RefreshCaption();
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			if ((result == ContinueWithSave.Yes) && (tariffRule.ShouldDeleteSecondaryTariffs))
			{
				if (Globals.Message.Show("You have changed a rule code from 'STN' to a different one and system will delete all the existing associated secondary tariffs. Are you sure you wish to save the changes?", "Save Changes and Delete Associated Secondary Tariffs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
				{
					result = ContinueWithSave.No;
				}
				else
				{
					tariffRule.SecondaryTariffs.DeleteAll();
				}
			}

			return result;
		}
	}
}
