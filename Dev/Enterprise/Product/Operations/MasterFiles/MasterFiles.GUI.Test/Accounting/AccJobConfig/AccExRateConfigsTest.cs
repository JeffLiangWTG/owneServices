using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccExRateConfigs))]
	public class AccExRateConfigsTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		public void TestSettingBusinessContext()
		{
			BusinessObjectFactory formFactory;
			using (var form = CreateFormForTest())
			{
				formFactory = ((IBusiness)form.DataSource).Factory;
				form.Show();
				Assert("Must have PermittedToDeleteJobExchangeRateConfig business context", formFactory.HasContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig));
			}
			Assert("Must NOT have PermittedToDeleteJobExchangeRateConfig business context after the form is closed", !formFactory.HasContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig));
		}

		public void TestInvoiceCurrencyTypeAtSystemLevel()
		{
			var configs = new AccExchangeRateConfigurationCollection(Factory);

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(configs, ".");
				form.Show();

				Assert("Column removed at system level", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}
		}

		public void TestInvoiceCurrencyTypeVisibilityAtCompanyLevel()
		{
			// The registry item should be checked against the company passed to the control, as it could differ from the login company
			var company = CreateTestCompany();

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column shown when enabled in registry", !control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column removed when disabled in registry", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column removed by default", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}
		}

		public void TestInvoiceCurrencyTypeVisibilityAtDebtorGroupLevel()
		{
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(debtorGroup, nameof(OrgDebtorGroup.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column shown when enabled in registry", !control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(debtorGroup, nameof(OrgDebtorGroup.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column removed when disabled in registry", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(debtorGroup, nameof(OrgDebtorGroup.AccExchangeRateConfigurations));
				form.Show();

				Assert("Column removed by default", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}
		}

		public void TestInvoiceCurrencyTypeVisibilityAtDebtorOrgLevel()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			organisation.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(organisation, $"{nameof(OrgHeader.CompanyData)}.{nameof(OrgCompanyData.AccARExchangeRateConfigurations)}");
				form.Show();

				Assert("Column shown when enabled in registry", !control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(organisation, $"{nameof(OrgHeader.CompanyData)}.{nameof(OrgCompanyData.AccARExchangeRateConfigurations)}");
				form.Show();

				Assert("Column removed when disabled in registry", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}

			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.SetDataBinding(organisation, $"{nameof(OrgHeader.CompanyData)}.{nameof(OrgCompanyData.AccARExchangeRateConfigurations)}");
				form.Show();

				Assert("Column removed by default", control.MasterGrid.GetColumnStyle(AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType).IsUnavailable);
			}
		}

		public void TestAccExRateConfigsControlsDefaultOrder()
		{
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(10, control.MasterGrid.ColumnStyles.Count);
				AssertEquals("LevelName", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("JCE_InvoiceCurrencyType", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[1]).ColumnName);
				AssertEquals("JCE_JobType", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[2]).ColumnName);
				AssertEquals("JCE_Ledger", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[3]).ColumnName);
				AssertEquals("JCE_TransportMode", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[4]).ColumnName);
				AssertEquals("JCE_ServiceDirection", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[5]).ColumnName);
				AssertEquals("JCE_Calc_CurrencyType", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[6]).ColumnName);
				AssertEquals("JCE_Preference", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[7]).ColumnName);
				AssertEquals("JCE_Offset", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[8]).ColumnName);
				AssertEquals("JCE_Prompt", ((ZGridColumnInfo)control.MasterGrid.ColumnStyles[9]).ColumnName);
			}
		}

		public void TestAccExchangeRateCurrencyGrid()
		{
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(4, control.CurrencyGrid.ColumnStyles.Count);
				AssertEquals("JCT_Code", ((ZGridColumnInfo)control.CurrencyGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("JCT_ExRateType", ((ZGridColumnInfo)control.CurrencyGrid.ColumnStyles[1]).ColumnName);
				AssertEquals("JCT_StartDate", ((ZGridColumnInfo)control.CurrencyGrid.ColumnStyles[2]).ColumnName);
				AssertEquals("JCT_ExpiryDate", ((ZGridColumnInfo)control.CurrencyGrid.ColumnStyles[3]).ColumnName);
			}
		}

		public void TestCurrencyGroupBoxCaption()
		{
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Exchange Rate Source", control.CurrencyGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestAccExRateConfigsSplitContainer()
		{
			using (var form = new ZForm())
			using (var control = new AccExRateConfigs())
			{
				form.Controls.Add(control);
				form.Show();

				var container = (KSplitContainer)control.GetField("AccExRateConfigsSplitContainer");
				Assert("Panel1 should have MasterGrid.", container.Panel1.Controls.Count == 1 && container.Panel1.Controls[0] == control.MasterGrid);
				Assert("Panel2 should have CurrencyGroupBox.", container.Panel2.Controls.Count == 1 && container.Panel2.Controls[0] == control.CurrencyGroupBox);
			}
		}

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var company = CreateTestCompany();
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccExRateConfigs();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.AccExchangeRateConfigurations));
			return form;
		}

		GlbCompany CreateTestCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true
			return company;
		}

		#endregion
	}
}
