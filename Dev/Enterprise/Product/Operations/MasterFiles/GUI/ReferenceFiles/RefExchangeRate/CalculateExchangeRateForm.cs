using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CalculateExchangeRateForm : ZChildForm
	{
		public CalculateExchangeRateForm()
			: base()
		{
		}

		public CalculateExchangeRateForm(RefExchangeRateCalculator rateCalculator)
			: base(rateCalculator)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void ButtonOK_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		public override string FormVerb => string.Empty;
	}
}
