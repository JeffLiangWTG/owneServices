using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CommissionAgreementControlTest : TestCaseWithFactory
	{
		#region Navigate

		public void TestNavigateToCommissionAgreementRecipientRate()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_GS_NKStaff = "SCW";

			recipient1.CAR_IsCommissionRateOverriden = true;
			recipient2.CAR_IsCommissionRateOverriden = true;

			var rate1A = recipient1.Rates.AddNew();
			var rate1B = recipient1.Rates.AddNew();
			var rate2A = recipient2.Rates.AddNew();
			var rate2B = recipient2.Rates.AddNew();

			Factory.Save();

			using (var form = new ZForm(agreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.NavigateToCommissionAgreementRecipientRate(rate2B);

				var recipientsGridCurrent = (BusinessObject)control.RecipientsGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(recipient2.PK, recipientsGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(recipient2, control.RecipientsGrid_Exposed.SelectedElements);

				var ratesGridCurrent = (BusinessObject)control.RatesGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(rate2B.PK, ratesGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(rate2B, control.RatesGrid_Exposed.SelectedElements);

				control.NavigateToCommissionAgreementRecipientRate(rate1A);

				recipientsGridCurrent = (BusinessObject)control.RecipientsGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(recipient1.PK, recipientsGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(recipient1, control.RecipientsGrid_Exposed.SelectedElements);

				ratesGridCurrent = (BusinessObject)control.RatesGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(rate1A.PK, ratesGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(rate1A, control.RatesGrid_Exposed.SelectedElements);
			}
		}

		public void TestNavigateToCommissionAgreementRecipientRate_WhenBoundToDraftVersions()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_GS_NKStaff = "ADL";

			recipient1.CAR_IsCommissionRateOverriden = true;
			recipient2.CAR_IsCommissionRateOverriden = true;

			var rate1A = recipient1.Rates.AddNew();
			var rate1B = recipient1.Rates.AddNew();
			var rate2A = recipient2.Rates.AddNew();
			var rate2B = recipient2.Rates.AddNew();

			var draftAgreement = agreement.CreateDraft();

			Factory.Save();

			using (var form = new ZForm(draftAgreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var draftRecipient1 = draftAgreement.Recipients.Single(x => x.GetMainVersion() == recipient1);
				var draftRecipient2 = draftAgreement.Recipients.Single(x => x.GetMainVersion() == recipient2);
				var draftRate1A = draftRecipient1.Rates.Single(x => x.GetMainVersion() == rate1A);
				var draftRate2B = draftRecipient2.Rates.Single(x => x.GetMainVersion() == rate2B);

				control.NavigateToCommissionAgreementRecipientRate(rate2B);

				var recipientsGridCurrent = (BusinessObject)control.RecipientsGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(draftRecipient2.PK, recipientsGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(draftRecipient2, control.RecipientsGrid_Exposed.SelectedElements);

				var ratesGridCurrent = (BusinessObject)control.RatesGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(draftRate2B.PK, ratesGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(draftRate2B, control.RatesGrid_Exposed.SelectedElements);

				control.NavigateToCommissionAgreementRecipientRate(rate1A);

				recipientsGridCurrent = (BusinessObject)control.RecipientsGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(draftRecipient1.PK, recipientsGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(draftRecipient1, control.RecipientsGrid_Exposed.SelectedElements);

				ratesGridCurrent = (BusinessObject)control.RatesGrid_Exposed.ListManager.GetCurrent();
				AssertEquals(draftRate1A.PK, ratesGridCurrent.PK);
				AssertContainsExactElementsInAnyOrder(draftRate1A, control.RatesGrid_Exposed.SelectedElements);
			}
		}

		#endregion

		#region Agreement Details

		[TestDate(2002, 2, 2)]
		public void TestAgreementDetails_StatusDescriptionLabel()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			var agreement2 = opportunity.CommissionAgreementsForEdit.AddNew();

			using (var form = new ZForm(agreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
				agreement.CA0_EffectiveDate = ZDate.Empty;
				AssertEquals(Color.Yellow, control.StatusDescriptionLabel_Exposed.BackColor);
				AssertEquals(Color.Black, control.StatusDescriptionLabel_Exposed.ForeColor);

				agreement.CA0_EffectiveDate = new ZDate(2001, 1, 1);
				AssertEquals(Color.LimeGreen, control.StatusDescriptionLabel_Exposed.BackColor);
				AssertEquals(Color.White, control.StatusDescriptionLabel_Exposed.ForeColor);

				agreement.Expire(new ZDate(2001, 1, 1));
				AssertEquals(Color.LightGray, control.StatusDescriptionLabel_Exposed.BackColor);
				AssertEquals(Color.Black, control.StatusDescriptionLabel_Exposed.ForeColor);

				agreement.Reverse();
				AssertEquals(Color.Red, control.StatusDescriptionLabel_Exposed.BackColor);
				AssertEquals(Color.White, control.StatusDescriptionLabel_Exposed.ForeColor);

				control.SetDataBinding(agreement2, "");
				AssertEquals(Color.Yellow, control.StatusDescriptionLabel_Exposed.BackColor);
				AssertEquals(Color.Black, control.StatusDescriptionLabel_Exposed.ForeColor);
			}
		}

		public void TestAgreementDetails_HideBasisWhenSecurityDenied()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementNotForCurrentStaff = opportunity.CommissionAgreementsForEdit.AddNew();
			agreementNotForCurrentStaff.CA0_OH_Customer = org.PK;
			agreementNotForCurrentStaff.FillWithValidTestData();
			var recipientNotForCurrentStaff = agreementNotForCurrentStaff.Recipients.AddNew();
			recipientNotForCurrentStaff.CAR_OH_Party = org.PK;
			recipientNotForCurrentStaff.FillWithValidTestData();
			var agreementForCurrentStaff = opportunity.CommissionAgreementsForEdit.AddNew();
			agreementForCurrentStaff.CA0_OH_Customer = org.PK;
			agreementForCurrentStaff.FillWithValidTestData();
			var recipientForCurrentStaff = agreementForCurrentStaff.Recipients.AddNew();
			recipientForCurrentStaff.CAR_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			Env.Security.CommissionAgreementViewAny.IsAllowed = false;
			AssertEquals("Precondition", false, agreementNotForCurrentStaff.IsViewRecipientsAdditionalInformationAllowed);
			AssertEquals("Precondition", true, agreementForCurrentStaff.IsViewRecipientsAdditionalInformationAllowed);

			using (var form = new ZForm(agreementForCurrentStaff))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.CA0_CommissionBasisDropEdit_Exposed.Visible);

				control.SetDataBinding(agreementNotForCurrentStaff, "");
				AssertEquals(false, control.CA0_CommissionBasisDropEdit_Exposed.Visible);
			}
		}

		#endregion

		#region Recipients Grid

		public void TestRecipientsGrid_DoNotHideAnyColumnsIfCanView()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementNotForCurrentStaff = opportunity.CommissionAgreementsForEdit.AddNew();
			agreementNotForCurrentStaff.CA0_OH_Customer = org.PK;
			agreementNotForCurrentStaff.FillWithValidTestData();
			var recipientNotForCurrentStaff = agreementNotForCurrentStaff.Recipients.AddNew();
			recipientNotForCurrentStaff.CAR_OH_Party = org.PK;
			recipientNotForCurrentStaff.FillWithValidTestData();
			var agreementForCurrentStaff = opportunity.CommissionAgreementsForEdit.AddNew();
			agreementForCurrentStaff.CA0_OH_Customer = org.PK;
			agreementForCurrentStaff.FillWithValidTestData();
			var recipientForCurrentStaff = agreementForCurrentStaff.Recipients.AddNew();
			recipientForCurrentStaff.CAR_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			Env.Security.CommissionAgreementViewAny.IsAllowed = false;
			AssertEquals("Precondition", false, agreementNotForCurrentStaff.IsViewRecipientsAdditionalInformationAllowed);
			AssertEquals("Precondition", true, agreementForCurrentStaff.IsViewRecipientsAdditionalInformationAllowed);

			using (var form = new ZForm(agreementForCurrentStaff))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var columnNames = control.RecipientsGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_CommissionType, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_Share, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.SharePercentage, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_Comment, columnNames);

				control.SetDataBinding(agreementNotForCurrentStaff, "");

				columnNames = control.RecipientsGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_CommissionType, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_Share, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.SharePercentage, columnNames);
				AssertCollectionContains(OrgCommissionAgreementRecipient.Schema.CAR_Comment, columnNames);
			}
		}

		public void TestRecipientsGrid_ContextMenus()
		{
			TestGridRateContextMenus((x) => x.RecipientsGrid_Exposed);
		}

		#endregion

		#region Rates Grid

		public void TestRatesGrid_HideWhenSecurityDenied()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			agreement.CA0_OH_Customer = org.PK;
			agreement.FillWithValidTestData();
			var recipientNotForCurrentStaff = agreement.Recipients.AddNew();
			recipientNotForCurrentStaff.CAR_OH_Party = org.PK;
			var recipientForCurrentStaff = agreement.Recipients.AddNew();
			recipientForCurrentStaff.CAR_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			Env.Security.CommissionAgreementViewAny.IsAllowed = false;
			AssertEquals("Precondition", false, recipientNotForCurrentStaff.IsViewRatesAllowed);
			AssertEquals("Precondition", true, recipientForCurrentStaff.IsViewRatesAllowed);

			using (var form = new ZForm(agreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.RecipientsGrid_Exposed.SelectSingleElement(recipientForCurrentStaff);
				AssertEquals(true, control.RatesGrid_Exposed.Visible);
				AssertEquals(true, control.CAR_EndDateEdit_Exposed.Visible);

				control.RecipientsGrid_Exposed.SelectSingleElement(recipientNotForCurrentStaff);
				AssertEquals(false, control.RatesGrid_Exposed.Visible);
				AssertEquals(false, control.CAR_EndDateEdit_Exposed.Visible);
			}
		}

		public void TestRatesGrid_RevalidateWhenEffectiveDateOrCommissionTriggerChanged()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.ProductItems.AddNew(true, OrgCommissionAgreementItemLookups.AllProductsCode);
			agreement.CA0_CommissionTriggerType = "1AR";

			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12).IsEnabled = true;
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24).IsEnabled = false;
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			var rate1A = recipient1.Rates.AddNew();
			rate1A.CAT_CommissionPercentage = 9;
			rate1A.CAT_CommissionPeriod = "0-12";

			recipient1.CAR_IsCommissionRateOverriden = true;

			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;

			agreement.EffectiveDate = ZDate.Today;

			Factory.Save();

			int refreshCount = 0;

			using (var form = new ZForm(agreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.RatesGrid_Exposed.Invalidated += (o, e) => refreshCount++;

				AssertEquals("Refresh", 0, refreshCount);

				agreement.EffectiveDate = ZDate.Today.AddDays(7);

				AssertEquals("Refresh", 1, refreshCount);

				agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;

				AssertEquals("Refresh", 2, refreshCount);
			}
		}

		public void TestRatesGrid_ContextMenus()
		{
			TestGridRateContextMenus((x) => x.RatesGrid_Exposed);
		}

		#endregion

		#region Context Menu Common

		void TestGridRateContextMenus(Func<CommissionAgreementControlForTest, ZGrid> gridGetter)
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			var recipient = agreement.Recipients.AddNew();

			using (var form = new ZForm(agreement))
			using (var control = new CommissionAgreementControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = gridGetter(control);
				var contextMenuItems = grid.ContextMenu.MenuItems;
				var commissionRatesMenuItem = contextMenuItems.Cast<ZMenuItem>().FirstOrDefault(x => x.Caption == "Commission Rates");
				AssertNotNull(commissionRatesMenuItem);

				var rateMenuItems = commissionRatesMenuItem.MenuItems.Cast<ZMenuItem>();
				AssertArrayEqualsByElements(
					new[]
					{
						"Override",
						"Use Default"
					},
					rateMenuItems.Select(x => x.Caption.ToString()).ToArray());

				var overrideMenuItem = rateMenuItems.First();
				var useDefaultMenuItem = rateMenuItems.Skip(1).First();

				grid.OnPopup_CallForTesting();
				AssertEquals(false, commissionRatesMenuItem.Enabled);
				AssertEquals(false, overrideMenuItem.Enabled);
				AssertEquals(false, useDefaultMenuItem.Enabled);

				recipient.CAR_GS_NKStaff = "ADL";

				grid.OnPopup_CallForTesting();
				AssertEquals(true, commissionRatesMenuItem.Enabled);
				AssertEquals(true, overrideMenuItem.Enabled);
				AssertEquals(true, useDefaultMenuItem.Enabled);

				recipient.CAR_IsCommissionRateOverriden = false;
				grid.OnPopup_CallForTesting();
				AssertEquals(false, overrideMenuItem.Checked);
				AssertEquals(true, useDefaultMenuItem.Checked);

				recipient.CAR_IsCommissionRateOverriden = true;
				grid.OnPopup_CallForTesting();
				AssertEquals(true, overrideMenuItem.Checked);
				AssertEquals(false, useDefaultMenuItem.Checked);

				recipient.CAR_GS_NKStaff = "";
				recipient.CAR_OH_Party = Factory.New<OrgHeader>().PK;

				grid.OnPopup_CallForTesting();
				AssertEquals(false, commissionRatesMenuItem.Enabled);
				AssertEquals(false, overrideMenuItem.Enabled);
				AssertEquals(false, useDefaultMenuItem.Enabled);
			}
		}

		#endregion

		#region Classes

		class CommissionAgreementControlForTest : CommissionAgreementControl
		{
			public ZDropEdit CA0_CommissionBasisDropEdit_Exposed
			{
				get { return CA0_CommissionBasisDropEdit; }
			}

			public ZLabel StatusDescriptionLabel_Exposed
			{
				get { return StatusDescriptionLabel; }
			}

			public ZGrid RecipientsGrid_Exposed
			{
				get { return RecipientsGrid; }
			}

			public ZGrid RatesGrid_Exposed
			{
				get { return RatesGrid; }
			}

			public ZDateEdit CAR_EndDateEdit_Exposed
			{
				get { return CAR_EndDateEdit; }
			}
		}

		#endregion
	}
}
