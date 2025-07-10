using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OpportunityCommissionAgreementsTabTest : TestCaseWithFactory
	{
		#region Agreements Grid

		[RequiresSTA]
		public void TestAddNewAgreementButton_ReadOnlyWhenSecurityDenied()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;

			using (var form = new ZFormForTest(opportunity))
			using (var control = new OpportunityCommissionAgreementsTabForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.NewAgreementButton_Exposed.Enabled);

				Factory.Save();
				form.FireSaved_Exposed();

				AssertEquals(false, control.NewAgreementButton_Exposed.Enabled);
			}
		}

		#region AgreementsGrid Context Menu

		public void TestAgreementsGrid_DisableContextMenu()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();

			using (var form = new ZForm(opportunity))
			using (var control = new OpportunityCommissionAgreementsTab())
			{
				form.Controls.Add(control);

				form.Show();

				var agreementMenuItems = control.AgreementsGrid.ContextMenu.MenuItems.Cast<ZMenuItem>();
				var disableMenuItem = agreementMenuItems.FirstOrDefault(x => x.Caption == "Disable");
				AssertNotNull(disableMenuItem);
				{
					OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint.IsAllowed = false;
					control.AgreementsGrid.SelectSingleElement(agreement);
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessages();

					disableMenuItem.PerformClick();
					AssertEquals(OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}

				{
					OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint.IsAllowed = true;
					control.AgreementsGrid.SelectSingleElement(agreement);
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessages();

					disableMenuItem.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType(typeof(ExpireCommissionAgreementForm), ZFormModaliser.LastFormShownDialogForTest);
				}

				{
					agreement.Reverse();
					control.AgreementsGrid.SelectSingleElement(agreement);
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessages();

					disableMenuItem.PerformClick();
					AssertEquals("All selected agreements have already been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Cannot Disable", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestAgreementsGrid_ReverseContextMenu()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();

			using (var form = new ZForm(opportunity))
			using (var control = new OpportunityCommissionAgreementsTab())
			{
				form.Controls.Add(control);

				form.Show();

				var agreementMenuItems = control.AgreementsGrid.ContextMenu.MenuItems.Cast<ZMenuItem>();
				var reverseMenuItem = agreementMenuItems.FirstOrDefault(x => x.Caption == "Reverse");
				AssertNotNull(reverseMenuItem);
				{
					OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint.IsAllowed = false;
					control.AgreementsGrid.SelectSingleElement(agreement);
					UnitTestUserNotification.Instance.ClearMessages();

					reverseMenuItem.PerformClick();
					AssertEquals(false, agreement.IsReversed);
					AssertEquals(OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				{
					OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint.IsAllowed = true;
					control.AgreementsGrid.SelectSingleElement(agreement);
					UnitTestUserNotification.Instance.ClearMessages();

					reverseMenuItem.PerformClick();
					AssertEquals(true, agreement.IsReversed);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				{
					control.AgreementsGrid.SelectSingleElement(agreement);
					UnitTestUserNotification.Instance.ClearMessages();

					reverseMenuItem.PerformClick();
					AssertEquals("All selected agreements have already been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Cannot Reverse", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		#endregion

		#endregion

		#region CheckOverlappingCommissionAgreements

		public void TestCheckOverlappingCommissionAgreements()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
				adlStaff.GS_Code = "ADL";
				adlStaff.GS_IsSalesRep = true;
				adlStaff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
				var risStaff = Factory.NewWithValidTestData<GlbStaff>();
				risStaff.GS_Code = "RIS";
				risStaff.GS_IsSalesRep = true;
				risStaff.GS_EmailAddress = "richard.smith@wisetechglobal.com";
				var customer = Factory.NewWithValidTestData<OrgHeader>();

				var opportunity = NewSavableOpportunity();
				opportunity.P8_OpportunityID = "O0001001";

				var moreGenericAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
				moreGenericAgreement.CA0_OH_Customer = customer.PK;
				moreGenericAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
				moreGenericAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
				moreGenericAgreement.Recipients.AddNew(adlStaff);
				moreGenericAgreement.Recipients.AddNew(risStaff);
				OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreGenericAgreement, "ALL", "ALL", "ALL");
				moreGenericAgreement.Recipients[0].CAR_Share = 1;
				moreGenericAgreement.Recipients[0].CAR_CommissionType = CommissionTypes.Codes.PCT;
				moreGenericAgreement.Recipients[0].Rates.AddNew();

				opportunity.RunPreSaveValidation();
				AssertContainsExactElementsInAnyOrder("Precondition: the opportunity should be able to be saved on a form", Enumerable.Empty<string>(), opportunity.Notifications.Select(x => x.Message));

				Factory.Save();

				using (var form = new ZForm(opportunity))
				using (var control = new OpportunityCommissionAgreementsTab())
				{
					form.Controls.Add(control);
					form.Show();

					var moreSpecificAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
					moreSpecificAgreement.CA0_OH_Customer = customer.PK;
					moreSpecificAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
					moreSpecificAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
					OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreSpecificAgreement, "SHP", "ALL", "ALL");

					var formSaved = false;
					form.Saved += (sender, e) => formSaved = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					form.FireSaveButton();

					CombineAssertions(() =>
					{
						AssertEquals("LastMessage.Caption", "More specific commission agreement has been added",
							UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertMultilineASCIIEquals("LastMessage.Text", string.Format(
								@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

{0} will take commission from {1}
   SHP | ALL > ALL | ALL

An email will be sent to all members of the existing commission agreement wolf packs. Are you sure you want to continue?",
								moreSpecificAgreement.AgreementId, moreGenericAgreement.AgreementId),
							UnitTestUserNotification.Instance.LastMessage.Text);
					});

					AssertEquals("Should have cancelled save", false, formSaved);
					AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					form.FireSaveButton();

					AssertEquals("Should have saved", true, formSaved);
					AssertContainsExactElementsInAnyOrder("RecipientsAsDelimitedString",
						new[]
						{
							"andrew.luong@wisetechglobal.com",
							"richard.smith@wisetechglobal.com",
						},
						Env.OutgoingMailManager.EmailsCreated.Select(email => email.Recipients.RecipientsAsDelimitedString()));
					AssertContains("Subject", "Commission Agreement O0001001#1", Env.OutgoingMailManager.EmailsCreated[0].Subject);

					moreSpecificAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

					formSaved = false;
					Env.OutgoingMailManager.EmailsCreated.Clear();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.FireSaveButton();

					AssertEquals("Should not try to send email again", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Should have saved", true, formSaved);
					AssertEquals("Should have not sent email again", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestOverallpingCommissionAgreementsEmail_OpportunityIdOnEmailIsCorrectForNewlyAddedOpportunities()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			adlStaff.GS_IsSalesRep = true;
			adlStaff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			var risStaff = Factory.NewWithValidTestData<GlbStaff>();
			risStaff.GS_Code = "RIS";
			risStaff.GS_IsSalesRep = true;
			risStaff.GS_EmailAddress = "richard.smith@wisetechglobal.com";
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var existingOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var moreGenericAgreement = existingOpportunity.CommissionAgreementsForEdit.AddNew();
			moreGenericAgreement.CA0_OH_Customer = customer.PK;
			moreGenericAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			moreGenericAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			moreGenericAgreement.Recipients.AddNew(adlStaff);
			moreGenericAgreement.Recipients.AddNew(risStaff);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreGenericAgreement, "ALL", "ALL", "ALL");

			Factory.Save();

			var newOpportunity = NewSavableOpportunity();
			newOpportunity.P8_OpportunityID = ZString.Empty;
			newOpportunity.RunPreSaveValidation();
			AssertContainsExactElementsInAnyOrder("Precondition: the opportunity should be able to be saved on a form", Enumerable.Empty<string>(), newOpportunity.Notifications.Select(x => x.Message));

			using (var form = new ZForm(newOpportunity))
			using (var control = new OpportunityCommissionAgreementsTab())
			{
				form.Controls.Add(control);
				form.Show();

				var moreSpecificAgreement = newOpportunity.CommissionAgreementsForEdit.AddNew();
				moreSpecificAgreement.CA0_OH_Customer = customer.PK;
				moreSpecificAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
				moreSpecificAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
				OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreSpecificAgreement, "SHP", "ALL", "ALL");

				var formSaved = false;
				form.Saved += (sender, e) => formSaved = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var notificationEmailTemplate = new NotificationEmailTemplate(typeof(DocCommissionAgreementConflictEmailCreator), "", "(*SpecificAgreementIDHyperlink*)");
				OrganisationRegistry.Instance.CommissionAgreementConflictRecipientsEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationEmailTemplate);

				form.FireSaveButton();

				AssertEquals("Precondition: Should have saved", true, formSaved);
				AssertNotEquals("Precondition: Should have populated opportunity id", "", newOpportunity.P8_OpportunityID);
				AssertContains("Body", newOpportunity.P8_OpportunityID, Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestShouldNotSendOverlappingCommissionAgreementsEmailWhenSaveFails()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			adlStaff.GS_IsSalesRep = true;
			adlStaff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			var risStaff = Factory.NewWithValidTestData<GlbStaff>();
			risStaff.GS_Code = "RIS";
			risStaff.GS_IsSalesRep = true;
			risStaff.GS_EmailAddress = "richard.smith@wisetechglobal.com";
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = NewSavableOpportunity();
			opportunity.P8_OpportunityID = "O0001001";
			opportunity.P8_Status = "CRT";

			var moreGenericAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
			moreGenericAgreement.CA0_OH_Customer = customer.PK;
			moreGenericAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			moreGenericAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			moreGenericAgreement.Recipients.AddNew(adlStaff);
			moreGenericAgreement.Recipients.AddNew(risStaff);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreGenericAgreement, "ALL", "ALL", "ALL");
			moreGenericAgreement.Recipients[0].CAR_Share = 1;
			moreGenericAgreement.Recipients[0].CAR_CommissionType = CommissionTypes.Codes.PCT;
			moreGenericAgreement.Recipients[0].Rates.AddNew();

			opportunity.RunPreSaveValidation();
			AssertContainsExactElementsInAnyOrder("Precondition: the opportunity should be able to be saved on a form", Enumerable.Empty<string>(), opportunity.Notifications.Select(x => x.Message));

			Factory.Save();

			using (var form = new ZForm(opportunity))
			using (var control = new OpportunityCommissionAgreementsTab())
			{
				form.Controls.Add(control);
				form.Show();

				var moreSpecificAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
				moreSpecificAgreement.CA0_OH_Customer = customer.PK;
				moreSpecificAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
				moreSpecificAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
				OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(moreSpecificAgreement, "SHP", "ALL", "ALL");

				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(otherFactory))
				{
					var opportunityInOtherFactory = otherFactory.Load<OrgOpportunity>(opportunity.PK);
					opportunityInOtherFactory.P8_Status = "LOS";
					otherFactory.Save();
				}

				opportunity.P8_Status = "WON";

				var formSaved = false;
				form.Saved += (sender, e) => formSaved = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FireSaveButton();

				AssertContains("Precondition: Should have concurrency exception", "While you have been working with this form, another user has made changes", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Precondition: Should not have saved due to concurrency exception", false, formSaved);
				AssertEquals("Should not have sent email if save failed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly()
		{
			using (var agreementsTab = new OpportunityCommissionAgreementsTabForTest())
			{
				IReadOnlyToggleControl readOnlyToggle = agreementsTab;

				readOnlyToggle.ReadOnly = false;
				AssertEquals(true, agreementsTab.NewAgreementButton_Exposed.Enabled);

				readOnlyToggle.ReadOnly = true;
				AssertEquals(false, agreementsTab.NewAgreementButton_Exposed.Enabled);
			}
		}

		#endregion

		#region Implementation

		OrgOpportunity NewSavableOpportunity()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityDescription = "Test Opportunity";
			opportunity.P8_OpportunityType = OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.Cast<ICodeDescriptionBool>().First().Code;
			opportunity.P8_Stage = OrganisationsDataRegistry.Instance.OpportunityStages.Value.Cast<ICodeDescriptionBool>().First().Code;
			opportunity.P8_Status = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.Cast<ICodeDescriptionBool>().First().Code;

			opportunity.RunPreSaveValidation();
			AssertContainsExactElementsInAnyOrder("Precondition: the opportunity should be able to be saved on a form", Enumerable.Empty<string>(), opportunity.Notifications.Select(x => x.Message));

			return opportunity;
		}

		#endregion

		#region Classes

		class ZFormForTest : ZForm
		{
			public ZFormForTest(object dataSource)
				: base(dataSource)
			{
			}

			public void FireSaved_Exposed()
			{
				base.FireSaved();
			}
		}

		class OpportunityCommissionAgreementsTabForTest : OpportunityCommissionAgreementsTab
		{
			public ZToolStripButton NewAgreementButton_Exposed
			{
				get { return NewAgreementButton; }
			}
		}

		#endregion
	}
}
