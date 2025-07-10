using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UpdateOpportunityDateForExchangeRateForm : ZChildForm
	{
		public UpdateOpportunityDateForExchangeRateForm(UpdateP8_DateForExchangeRateAction updateAction)
			: base(updateAction)
		{
			InitializeComponent();
		}

		public new UpdateP8_DateForExchangeRateAction BusinessEntity
		{
			get { return (UpdateP8_DateForExchangeRateAction)base.BusinessEntity; }
		}

		#region Buttons

		#region UpdateDateButton

		protected internal ZButton UpdateDateButton
		{
			get { return updateDateButton; }
		}

		void UpdateDateButton_Click(object sender, EventArgs e)
		{
			BusinessEntityForValidation.RunPreSaveValidation();
			if (!BusinessEntityForValidation.HasErrors())
			{
				BusinessEntity.Execute();
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#endregion

		#region UseExistingDateButton

		protected internal ZButton UseExistingDateButton
		{
			get { return useExistingDateButton; }
		}

		void UseExistingDateButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Event Handlers

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				this.Close();
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		#endregion
	}
}
