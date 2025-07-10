using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AccountingIntegrationOptionsUserControl))]
	sealed class AccountingIntegrationOptionsUserControlTest : RegistryZUserControlTestCase
	{
		public void TestPostGroupBoxEnabled()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(options, null);
				control.ReadOnly = true;
				AssertEquals(false, control.EnableIntegrationCheckBox.Enabled);
				AssertEquals(false, control.PreAppovalJobCheckBox.Enabled);
				control.ReadOnly = false;
				AssertEquals("Enable Accounting Integration should be enabled", true, control.EnableIntegrationCheckBox.Enabled);
				AssertEquals(false, control.PreAppovalJobCheckBox.Enabled);
				AssertEquals(false, control.DSBChargesGroupBox.Enabled);
				AssertEquals(false, control.CDSStatusCodesTextBox.Enabled);
				AssertEquals(false, control.ChiefStatusCodesTextBox.Enabled);
				AssertEquals(false, control.EUStatusCodesTextBox.Enabled);
				options.EnableAccountingIntegration = true;
				AssertEquals(true, control.PreAppovalJobCheckBox.Enabled);
				AssertEquals(true, control.DSBChargesGroupBox.Enabled);
				AssertEquals(true, control.CDSStatusCodesTextBox.Enabled);
				AssertEquals(true, control.ChiefStatusCodesTextBox.Enabled);
				AssertEquals(true, control.EUStatusCodesTextBox.Enabled);
			}

			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(options, null);
				AssertEquals(true, control.PreAppovalJobCheckBox.Enabled);
				AssertEquals(true, control.DSBChargesGroupBox.Enabled);
				AssertEquals(true, control.CDSStatusCodesTextBox.Enabled);
				AssertEquals(true, control.ChiefStatusCodesTextBox.Enabled);
				AssertEquals(true, control.EUStatusCodesTextBox.Enabled);
			}
		}

		public void TestPreApprovalBillingJobVisibility()
		{
			GlbCompany usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(option, null);
				AssertEquals(false, control.PreAppovalJobCheckBox.Visible);
			}

			AccountingIntegrationOptions optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(usCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			optionCloned.EnableAccountingIntegration = true;
			optionCloned.APPostDSB = true;
			optionCloned.PreApprovalBillingJob = true;
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(optionCloned, null);
				AssertEquals(true, control.PreAppovalJobCheckBox.Visible);
			}
		}

		public void TestStatusCodesVisibility()
		{
			GlbCompany gbCompany = Factory.NewWithValidTestData<GlbCompany>();
			gbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			Factory.Save();
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(option, null);
				AssertEquals(false, control.CDSStatusCodesTextBox.Visible);
				AssertEquals(false, control.ChiefStatusCodesTextBox.Visible);
			}

			AccountingIntegrationOptions optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(gbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			optionCloned.EnableAccountingIntegration = true;
			optionCloned.APPostDSB = true;
			optionCloned.PreApprovalBillingJob = true;
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(optionCloned, null);
				AssertEquals(true, control.CDSStatusCodesTextBox.Visible);
				AssertEquals(true, control.ChiefStatusCodesTextBox.Visible);
			}
		}

		public void TestEUStatusCodesVisibility()
		{
			GlbCompany gbCompany = Factory.NewWithValidTestData<GlbCompany>();
			gbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
			Factory.Save();
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(option, null);
				AssertEquals(false, control.EUStatusCodesTextBox.Visible);
			}

			AccountingIntegrationOptions optionCloned = (AccountingIntegrationOptions)((IRegistryBusiness)(option)).Clone(new FallbackLevel(gbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			optionCloned.EnableAccountingIntegration = true;
			optionCloned.APPostDSB = true;
			optionCloned.PreApprovalBillingJob = true;
			using (ZForm form = new ZForm())
			using (AccountingIntegrationOptionsUserControl control = new AccountingIntegrationOptionsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(optionCloned, null);
				AssertEquals(true, control.EUStatusCodesTextBox.Visible);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new AccountingIntegrationOptions();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var optionUserControl = (AccountingIntegrationOptionsUserControl)control;
			return !optionUserControl.EnableIntegrationCheckBox.Enabled;
		}
	}
}
