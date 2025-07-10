using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class AccountingIntegrationOptionsUserControl : RegistryZUserControl
	{
		public AccountingIntegrationOptionsUserControl()
		{
			InitializeComponent();

			PreAppovalJobCheckBox.Enabled = false;
			DSBChargesGroupBox.Enabled = false;
			ChiefStatusCodesTextBox.Enabled = false;
			CDSStatusCodesTextBox.Enabled = false;
			EUStatusCodesTextBox.Enabled = false;

			EnableIntegrationCheckBox.CheckedChanged += new EventHandler(EnableIntegrationCheckBox_CheckedChanged);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var options = (AccountingIntegrationOptions)CurrentDataItem;
			var isPreApprovalBillingJob = false;
			var isCustomsStatusCodes = false;
			var isEUCustomsStatusCodes = false;
			if (options != null)
			{
				isPreApprovalBillingJob = options.IsPreApprovalBillingJobSupported();
				isCustomsStatusCodes = options.IsCustomsStatusCodesSupported();
				isEUCustomsStatusCodes = options.IsEUCustomsStatusCodesSupported();
			}
			PreAppovalJobCheckBox.Visible = isPreApprovalBillingJob;
			ChiefStatusCodesTextBox.Visible = isCustomsStatusCodes;
			CDSStatusCodesTextBox.Visible = isCustomsStatusCodes;
			EUStatusCodesTextBox.Visible = isEUCustomsStatusCodes;
		}

		void EnableIntegrationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PreAppovalJobCheckBox.Enabled = EnableIntegrationCheckBox.Checked;
			DSBChargesGroupBox.Enabled = EnableIntegrationCheckBox.Checked;
			ChiefStatusCodesTextBox.Enabled = EnableIntegrationCheckBox.Checked;
			CDSStatusCodesTextBox.Enabled = EnableIntegrationCheckBox.Checked;
			EUStatusCodesTextBox.Enabled = EnableIntegrationCheckBox.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			EnableIntegrationCheckBox.Enabled = !readOnly;
		}
	}
}
