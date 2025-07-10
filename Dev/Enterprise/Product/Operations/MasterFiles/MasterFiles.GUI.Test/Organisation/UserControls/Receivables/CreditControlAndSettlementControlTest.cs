using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CreditControlAndSettlementControlTest : TestCaseWithFactory
	{
		public void TestGlobalTabPageTabVisibility()
		{
			var org = Factory.New<OrgHeader>();
			var oldSecurityValue = Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = false;

				using (var form = new ZForm(org))
				{
					using (var control = new DetailsControlForTest())
					{
						form.Controls.Add(control);
						form.Show();
						control.SetDataBinding(org, "");
						var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Edit -> Receivables -> Modify Credit Control and Settlement -> Modify Credit Control -> Global Credit Control";

						AssertContains(expectedMessage, control.TabCoveringText);
					}
				}
			}
			finally
			{
				Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = oldSecurityValue;
			}
		}

		#region Credit Reports

		public void TestReportReadOnlyScoresExist()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(org))
			using (var detailsControl = new DetailsControlForTest())
			{
				form.Controls.Add(detailsControl);
				form.Show();
				var dnbRatingTextBox = (ZTextBox)form.Controls.Find("DnbRatingTextBox", true)[0];
				var creditLatePaymentTextBox = (ZTextBox)form.Controls.Find("CreditLatePaymentTextBox", true)[0];
				var creditFailureRiskTextBox = (ZTextBox)form.Controls.Find("CreditFailureRiskTextBox", true)[0];

				AssertEquals(true, dnbRatingTextBox.ReadOnly);
				AssertEquals(true, creditLatePaymentTextBox.ReadOnly);
				AssertEquals(true, creditFailureRiskTextBox.ReadOnly);
			}
		}

		public void TestCreditReportsVisibility()
		{
			var org = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertCreditReportsVisibility(false, true, true, true, org);
				AssertCreditReportsVisibility(false, true, false, true, org);
				AssertCreditReportsVisibility(false, false, true, true, org);
				AssertCreditReportsVisibility(false, false, false, true, org);
				AssertCreditReportsVisibility(true, true, true, false, org);
				AssertCreditReportsVisibility(true, true, false, true, org);
				AssertCreditReportsVisibility(true, false, true, true, org);
				AssertCreditReportsVisibility(true, false, false, true, org);
			});
		}

		void AssertCreditReportsVisibility(bool hasUrl, bool enableCreditReports, bool currentCompanyCountryAvailable, bool expectedPanel1Collapsed, OrgHeader org)
		{
			var creditReportItemCollection = new CreditReportItemCollection() {
				CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = currentCompanyCountryAvailable;

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			if (hasUrl)
			{
				collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });
			}

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCreditReports))
			using (var form = new ZForm(org))
			using (var detailsControl = new DetailsControlForTest())
			{
				form.Controls.Add(detailsControl);
				form.Show();
				AssertEquals(expectedPanel1Collapsed, detailsControl.LocalSplitContainerForTest.Panel1Collapsed);
			}
		}

		public void TestCreditScoresVisibility()
		{
			var org = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertCreditScoresVisibility("https://www.baidu.com", true, true, true, org);
				AssertCreditScoresVisibility("https://www.baidu.com", true, false, false, org);
				AssertCreditScoresVisibility("https://www.baidu.com", false, true, false, org);
				AssertCreditScoresVisibility("https://www.baidu.com", false, false, false, org);
			});
		}

		void AssertCreditScoresVisibility(string creditCheckServiceURL, bool enableCreditReports, bool currentCompanyCountryAvailable, bool expectedVisible, OrgHeader org)
		{
			var creditReportItemCollection = new CreditReportItemCollection() {
				CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = currentCompanyCountryAvailable;

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)creditCheckServiceURL, Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCreditReports))
			using (var form = new ZForm(org))
			using (var detailsControl = new DetailsControlForTest())
			{
				form.Controls.Add(detailsControl);
				form.Show();
				AssertEquals(expectedVisible, detailsControl.ARCreditReportDetailsGroupBoxForTest.Visible);
			}
		}

		[RequiresSTA]
		public void TestCreditScoresValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgMiscServ = Factory.New<OrgMiscServ>();
			orgMiscServ.OM_OH = org.PK;
			orgMiscServ.OM_CCCreditRating = "XXXXXX";
			orgMiscServ.OM_CCFailureRiskScore = 75;
			orgMiscServ.OM_CCLatePaymentScore = 99;
			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() {
				CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var detailsControl = new DetailsControlForTest())
			{
				form.Controls.Add(detailsControl);
				form.Show();

				AssertEquals("CreditFailureRiskTextBox shows OM_CCFailureRiskScore", "75", detailsControl.CreditFailureRiskTextBoxForTest.Text);
				AssertEquals("CreditLatePaymentTextBox shows OM_CCLatePaymentScore", "99", detailsControl.CreditLatePaymentTextBoxForTest.Text);
				AssertEquals("DnbRatingTextBox shows OM_CCCreditRating", "XXXXXX", detailsControl.DnbRatingTextBoxForTest.Text);
			}
		}

		#endregion

		public class DetailsControlForTest : CreditControlAndSettlementControl
		{
			public string TabCoveringText => base.GlobalTabPage.Controls[0].Text;
			internal CargoWise.Windows.UI.KSplitContainer LocalSplitContainerForTest => base.LocalSplitContainer;
			internal ZGroupBox ARCreditReportDetailsGroupBoxForTest => base.ARCreditReportDetailsGroupBox;
			internal ZTextBox CreditFailureRiskTextBoxForTest => base.CreditFailureRiskTextBox;
			internal ZTextBox CreditLatePaymentTextBoxForTest => base.CreditLatePaymentTextBox;
			internal ZTextBox DnbRatingTextBoxForTest => base.DnbRatingTextBox;
		}
	}
}
