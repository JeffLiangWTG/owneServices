using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ReceivablesInvoiceDetailsPageControlTest : TestCaseWithFactory
	{
		public void TestCashAdvanceTabVisibility()
		{
			var mockFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			mockFunctionalityChecker.Setup(x => x.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(false);
			ObjectFactory.Substitute(mockFunctionalityChecker.Object);

			using (var form = new ZForm(Org))
			{
				using (var userControl = new ReceivablesInvoiceDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();
					AssertEquals(false, userControl.ARCashAdvanceTabPage.TabVisible);
				}
			}

			mockFunctionalityChecker.Setup(x => x.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			using (var form = new ZForm(Org))
			{
				using (var userControl = new ReceivablesInvoiceDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();
					AssertEquals(true, userControl.ARCashAdvanceTabPage.TabVisible);
				}
			}
		}

		public void TestClientOverrideExchageRatesGroupBoxVisibility()
		{
			CombineAssertions(() =>
			{
				using (ZForm testForm = new ZForm(Org))
				{
					using (ReceivablesInvoiceDetailsUserControl testControl = new ReceivablesInvoiceDetailsUserControl())
					{
						testForm.Controls.Add(testControl);
						testForm.Show();
						testControl.SetClientOverrideExchageRatesGroupBoxVisibility(Factory.IsLocalClientExchangeRatefieldNeeded());
						testControl.ExchangeRatesTab.Show();
						var exRateGB = (ZGroupBox)testControl.Controls.Find("ClientOverrideExchageRatesGroupBox", true).FirstOrDefault();
						AssertEquals("ClientOverrideExchageRatesGroupBox shouldn't be visible", false, exRateGB.Visible);

						var exRate = Factory.New<RefExchangeRate>();
						exRate.RE_OH_Client = OrgHeader.DefaultOrg.PK;
						Factory.ClearCachedValue<bool>("IsLocalClientExchangeRatefieldNeeded");

						testControl.SetClientOverrideExchageRatesGroupBoxVisibility(Factory.IsLocalClientExchangeRatefieldNeeded());
						exRateGB = (ZGroupBox)testControl.Controls.Find("ClientOverrideExchageRatesGroupBox", true).FirstOrDefault();
						AssertEquals("ClientOverrideExchageRatesGroupBox should be visible", true, exRateGB.Visible);
					}
				}
			});
		}

		public void TestInvoiceBatchingGridReadOnly()
		{
			Data.OB_IsCreditor = true;
			Data.OB_IsDebtor = true;
			Factory.Save();
			bool previousOrgReceivablesModifySettings = Env.Security.OrgReceivablesModify.IsAllowed;
			bool previousOrgReceivablesModifyInvoiceBatchingSettings = Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed;
			bool previousOrgReceivablesModifyExchangeRatesSettings = Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModify.IsAllowed = true;
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = true;
				Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed = true;
				AssertInvoiceGridReadOnly(false, false);

				Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed = false;
				AssertInvoiceGridReadOnly(false, true);

				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = false;
				AssertInvoiceGridReadOnly(true, true);
			}
			finally
			{
				Env.Security.OrgReceivablesModify.IsAllowed = previousOrgReceivablesModifySettings;
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = previousOrgReceivablesModifyInvoiceBatchingSettings;
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = previousOrgReceivablesModifyExchangeRatesSettings;
			}
		}

		void AssertInvoiceGridReadOnly(bool batchingGridShouldBeReadOnly, bool exchangesRatesGridShouldBeReadOnly)
		{
			using (ZForm testForm = new ZForm(Org))
			{
				using (ReceivablesInvoiceDetailsUserControl testControl = new ReceivablesInvoiceDetailsUserControl())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					testControl.SetControlsReadOnlyForSecurity(batchingGridShouldBeReadOnly, exchangesRatesGridShouldBeReadOnly);
					AssertEquals("The Invoice Batching Grid should " + (!batchingGridShouldBeReadOnly ? "NOT " : "") + "be read-only", batchingGridShouldBeReadOnly, testControl.InvoiceBatchingGrid.ReadOnly);
					AssertEquals("The Invoice Batching Grid should " + (!exchangesRatesGridShouldBeReadOnly ? "NOT " : "") + "be read-only", exchangesRatesGridShouldBeReadOnly, testControl.ExchangeRatesGrid.ReadOnly);
				}
			}
		}

		public void TestColumnsAddedCorrectly()
		{
			AssertColumnAddedCorrectly("PI_RS_NKServiceLevel");
		}

		void AssertColumnAddedCorrectly(string columnName)
		{
			using (var form = new ZForm(Org))
			{
				using (var userControl = new ReceivablesInvoiceDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var expectedColumn = columnName;
					var columns = userControl.InvoiceBatchingGrid.ColumnStyles.Cast<ZGridColumnInfo>();
					Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
					Assert("New column should be visibled.", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
					Assert("New column should not be read only.", !columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
				}
			}
		}

		public void TestControlsAddedCorrectly()
		{
			TestControlAddedCorrectly("zDropEditServiceLevel");
		}

		public void TestControlAddedCorrectly(string controlName)
		{
			using (var form = new ZForm(Org))
			{
				using (var userControl = new ReceivablesInvoiceDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var expectedControlName = controlName;
					var controls = userControl.InvoiceBatchingGroupBox.Controls.Cast<Control>();
					Assert("New control should be added.", controls.Any(x => x.Name == expectedControlName));
					Assert("New control should be visibled.", controls.FirstOrDefault(x => x.Name == expectedControlName).Visible);
					Assert("New control should be enabled.", controls.FirstOrDefault(x => x.Name == expectedControlName).Enabled);
				}
			}
		}

		[RequiresSTA]
		public void TestWarehouseOptionsLinkLabel_LinkClicked()
		{
			using (ZOrganisationsForm form = new ZOrganisationsForm(Org))
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.ReceivablesTabPage;
				form.ReceivablesControl.ARTabControl.SelectedTab = form.ReceivablesControl.InvoiceDetailsTabPage;
				form.ReceivablesControl.InvoiceDetailsTabPage.Show();
				form.ReceivablesControl.ReceivablesInvoiceDetailsControl.WarehouseOptionsLinkLabel_LinkClicked(null, null);
				AssertEquals(form.WhsFacilityTabPage, form.OrganisationsTabControl.SelectedTab);
				AssertEquals(form.WhsFacilityUserControl.WarehouseUserControl.InvoicingTabPage, form.WhsFacilityUserControl.WarehouseUserControl.WarehouseTabControl.SelectedTab);
			}
		}

		public void TestInvoiceCurrencyCodeFindBoxBindingMember()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();

			using (var form = new ZForm(orgHeader))
			using (var userControl = new ReceivablesInvoiceDetailsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();

				var invoiceCurrencyCodeFindBox = form.FindSingleOrDefault<ZCodeFindBox>("InvoiceCurrencyCodeFindBox");
				var groupChargesGrid = form.FindSingleOrDefault<ZGrid>("GroupChargesGrid");
				AssertNotNull(invoiceCurrencyCodeFindBox);
				AssertNotNull(groupChargesGrid);
				AssertEquals("CompanyData+InvoiceRollupOrGroups.PG_RX_NKInvoicePostingCurrency", invoiceCurrencyCodeFindBox.GetBindingMember());

				var invoiceRollupOrGroup1 = orgHeader.CompanyData.InvoiceRollupOrGroups.AddNew();
				var invoiceRollupOrGroup2 = orgHeader.CompanyData.InvoiceRollupOrGroups.AddNew();
				AssertEquals(2, orgHeader.CompanyData.InvoiceRollupOrGroups.Count);
				AssertEquals("", invoiceRollupOrGroup1.PG_RX_NKInvoicePostingCurrency);
				AssertEquals("", invoiceRollupOrGroup2.PG_RX_NKInvoicePostingCurrency);

				groupChargesGrid.ListManager.Position = 1;
				AssertEquals(invoiceRollupOrGroup2, groupChargesGrid.ListManager.GetCurrent());

				invoiceCurrencyCodeFindBox.CodeBox.Text = "AUD";
				KEndCurrentEdit.EndCurrentEdit(invoiceCurrencyCodeFindBox.CodeBox);
				AssertEquals("", invoiceRollupOrGroup1.PG_RX_NKInvoicePostingCurrency);
				AssertEquals("AUD", invoiceRollupOrGroup2.PG_RX_NKInvoicePostingCurrency);

				groupChargesGrid.ListManager.Position = 0;
				AssertEquals(invoiceRollupOrGroup1, groupChargesGrid.ListManager.GetCurrent());

				invoiceCurrencyCodeFindBox.CodeBox.Text = "CNY";
				KEndCurrentEdit.EndCurrentEdit(invoiceCurrencyCodeFindBox.CodeBox);
				AssertEquals("CNY", invoiceRollupOrGroup1.PG_RX_NKInvoicePostingCurrency);
				AssertEquals("AUD", invoiceRollupOrGroup2.PG_RX_NKInvoicePostingCurrency);
			}
		}

		public void TestARInvTemplateTabVisible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
				using (var form = new ZForm(Org))
				{
					using (var userControl = new ReceivablesInvoiceDetailsUserControl())
					{
						form.Controls.Add(userControl);
						form.Show();
						var aRInvTemplateTabPages = userControl.Controls.Find("ARInvTemplateTabPage", true);
						AssertEquals(nameof(aRInvTemplateTabPages.Length), 0, aRInvTemplateTabPages.Length);
					}
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
				using (var form = new ZForm(Org))
				{
					using (var userControl = new ReceivablesInvoiceDetailsUserControl())
					{
						form.Controls.Add(userControl);
						form.Show();
						var aRInvTemplateTabPages = userControl.Controls.Find("ARInvTemplateTabPage", true);
						AssertEquals(nameof(aRInvTemplateTabPages.Length), 0, aRInvTemplateTabPages.Length);
					}
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
				using (var form = new ZForm(Org))
				{
					using (var userControl = new ReceivablesInvoiceDetailsUserControl())
					{
						form.Controls.Add(userControl);
						form.Show();
						var aRInvTemplateTabPages = userControl.Controls.Find("ARInvTemplateTabPage", true);
						AssertEquals(nameof(aRInvTemplateTabPages.Length), 1, aRInvTemplateTabPages.Length);
						var aRInvTemplateTabPage = aRInvTemplateTabPages[0] as ZTabPage;
						AssertEquals(nameof(aRInvTemplateTabPage.TabVisible), true, aRInvTemplateTabPage.TabVisible);
					}
				}
			}
		}

		#region Set Up

		OrgHeader Org;
		OrgCompanyData Data;

		protected override void SetUp()
		{
			base.SetUp();

			Org = Factory.NewWithValidTestData<OrgHeader>();
			Data = Org.CompanyData;
		}

		#endregion
	}
}
