using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Workflow.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(HRJobOpeningsForm))]
	public class HRJobOpeningsForm_Test : ZFormBasherTest
	{
		[UseSnapshotProtection]
		public void TestEmailApplicantsButton()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				campaignForm.EmailApplicantsButton.DropDownItems[0].PerformClick();
				AssertEquals("Campaign has changes, should show error message", HRJobOpeningsForm.SaveBeforeSendingErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Factory.Save();
			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				campaignForm.EmailApplicantsButton.DropDownItems[0].PerformClick();
				AssertEquals("No applicants exist, should show error message", HRJobOpeningsForm.InvalidApplicationErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			HRJobApplication application1 = campaign.Applications.AddNew();
			HRJobApplicant applicantA = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantA.HA_FullName = "A";
			applicantA.Person.UpdateFromApplicant(applicantA);
			application1.HP_HA = applicantA.PK;
			application1.HP_CurrentStatus = "INP";

			HRJobApplicant applicantB = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantB.HA_FullName = "B";
			applicantB.Person.UpdateFromApplicant(applicantB);
			HRJobApplication application2 = campaign.Applications.AddNew();
			application2.HP_HA = applicantB.PK;
			application2.HP_CurrentStatus = "INP";

			Factory.Save();
			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					campaignForm.EmailApplicantsButton.DropDownItems[0].PerformClick();
					AssertEquals("Form shown should be type of Email Multiple Contacts Form", typeof(EmailMultipleContactsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					EmailMultipleContactsForm emailForm = (EmailMultipleContactsForm)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("Should contain 2 email to contacts (No Applicants selected = email to all)", 2, ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts.Count);
					EmailToContactBusinessObjectCollection applicantEmailContacts = ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts;
					applicantEmailContacts.Sort(EmailWithAttachment.Schema.ToDisplayName, ListSortDirection.Ascending);
					AssertEquals("Should contain Email To Contact BizO for Applicant A", "A", applicantEmailContacts[0].ToDisplayName);
					AssertEquals("Should contain Email To Contact BizO for Applicant B", "B", applicantEmailContacts[1].ToDisplayName);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
					}
				}
			}

			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				int application1Index = campaignForm.ApplicationsGrid.ListManager.List.IndexOf(application1);
				int application2Index = campaignForm.ApplicationsGrid.ListManager.List.IndexOf(application2);
				campaignForm.ApplicationsGrid.Select(application1Index);
				AssertEquals("There should be 1 rows selected on the applications grid", 1, campaignForm.ApplicationsGrid.SelectedRowCount);
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					campaignForm.EmailApplicantsButton.DropDownItems[0].PerformClick();
					AssertEquals("Form shown should be type of Email Multiple Contacts Form", typeof(EmailMultipleContactsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					EmailMultipleContactsForm emailForm = (EmailMultipleContactsForm)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("Should contain 1 email to contact", 1, ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts.Count);
					AssertEquals("Email should be for ApplicantA", applicantA.HA_FullName, ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts[0].ToDisplayName);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEmailApplicantsButtonSendsToApplicantRightStatus()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();

			HRJobApplication application4 = campaign.Applications.AddNew();
			HRJobApplicant applicantA = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantA.HA_FullName = "A";
			applicantA.Person.UpdateFromApplicant(applicantA);
			application4.HP_HA = applicantA.PK;
			application4.HP_CurrentStatus = "ST1";

			HRJobApplicant applicantB = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantB.HA_FullName = "B";
			applicantB.Person.UpdateFromApplicant(applicantB);
			HRJobApplication application5 = campaign.Applications.AddNew();
			application5.HP_HA = applicantB.PK;
			application5.HP_CurrentStatus = "REJ";

			HRJobApplicant applicantC = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantC.HA_FullName = "C";
			applicantC.Person.UpdateFromApplicant(applicantB);
			HRJobApplication application6 = campaign.Applications.AddNew();
			application6.HP_HA = applicantC.PK;
			application6.HP_CurrentStatus = "ST1";
			Factory.Save();

			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				campaignForm.EmailApplicantsButton.DropDownItems[0].PerformClick();
				AssertEquals("No applicants exist, should show error message", HRJobOpeningsForm.InvalidApplicationErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					campaignForm.EmailApplicantsButton.DropDownItems[1].PerformClick();
					AssertEquals("Form shown should be type of Email Multiple Contacts Form", typeof(EmailMultipleContactsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					EmailMultipleContactsForm emailForm = (EmailMultipleContactsForm)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("Should contain 2 email to contact", 2, ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts.Count);

					EmailToContactBusinessObjectCollection applicantEmailContacts = ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts;
					applicantEmailContacts.Sort(EmailWithAttachment.Schema.ToDisplayName, ListSortDirection.Ascending);
					AssertEquals("Should contain Email To Contact BizO for Applicant A", "A", applicantEmailContacts[0].ToDisplayName);
					AssertEquals("Should contain Email To Contact BizO for Applicant C", "C", applicantEmailContacts[1].ToDisplayName);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
					}
				}
			}

			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				campaignForm.BottomTabControl.SelectedTab = campaignForm.ApplicantsTabPage;
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					campaignForm.EmailApplicantsButton.DropDownItems[6].PerformClick();
					AssertEquals("Form shown should be type of Email Multiple Contacts Form", typeof(EmailMultipleContactsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					EmailMultipleContactsForm emailForm = (EmailMultipleContactsForm)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("Should contain 1 email to contact", 1, ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts.Count);

					EmailToContactBusinessObjectCollection applicantEmailContacts = ((MultipleEmailToContactSender)ZFormModaliser.LastIBusinessShownOnDialogForTest).EmailsToContacts;
					applicantEmailContacts.Sort(EmailWithAttachment.Schema.ToDisplayName, ListSortDirection.Ascending);
					AssertEquals("Should contain Email To Contact BizO for Applicant B", "B", applicantEmailContacts[0].ToDisplayName);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
					}
				}
			}
		}

		public void TestEmailMenuItems()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			using (HRJobOpeningsForm campaignForm = new HRJobOpeningsForm(campaign))
			{
				campaignForm.Show();
				ApplicationStatusCollection collection = RecruiterDataRegistry.Instance.ApplicationStatuses.Value;
				AssertEquals(collection.Count, campaignForm.EmailApplicantsButton.DropDownItems.Count);
				string expected = string.Join(",", collection.Cast<ApplicationStatus>().Select(a => a.Description.ToString()).ToArray());
				string actual = string.Join(",", campaignForm.EmailApplicantsButton.DropDownItems.Cast<ToolStripItem>().Select(t => t.Text).ToArray());
				AssertEquals(expected, actual);
			}
		}

		public void TestAuditColumnExists()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				AssertColumn(form.ApplicationsGridExposed, "HP_SystemCreateUser", "Created By", false);
				AssertColumn(form.ApplicationsGridExposed, "HP_SystemCreateTimeUtc", "Created Time", false);
				AssertColumn(form.ApplicationsGridExposed, "HP_SystemLastEditUser", "Last Edit", false);
				AssertColumn(form.ApplicationsGridExposed, "HP_SystemLastEditTimeUtc", "Last Edited Time", false);
			}
		}

		public void TestReferringColumns()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				AssertColumn(form.ApplicationsGridExposed, "HP_SourceType", "Source", true);
				AssertColumn(form.ApplicationsGridExposed, "HP_SourceDetails", "Source Details", false);
				AssertColumn(form.ApplicationsGridExposed, "HP_OH_ReferringOrganisation", "Referring Org. Code", true);
				AssertColumn(form.ApplicationsGridExposed, "ReferringOrganisation+OH_FullName", "Referring Org. Name", false);
				AssertColumn(form.ApplicationsGridExposed, "HP_PER_ReferringPerson", "Referring Person", true);
				AssertColumn(form.ApplicationsGridExposed, "ReferringStaffCode", "Referring Staff", false);
			}
		}

		public void TestApplicantCountryColumnsExist()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				AssertColumn(form.ApplicationsGridExposed, "Applicant+HA_FullName", "Full Name", true);
				AssertColumn(form.ApplicationsGridExposed, "Applicant+HA_RN_NKCountry", "Country/Region", true);
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string caption, bool visible)
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
			AssertNotNull(columnName + " column should exist", columnStyle);
			AssertEquals(columnName + " Caption", caption, columnStyle.Caption ?? columnStyle.CaptionResourceString?.Caption);
			AssertEquals(columnName + " Visible", visible, columnStyle.IsVisible);
		}

		public void TestApplicationsGridDoubleClick()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				form.ApplicationsGridExposed.SetDataBinding(applicant.Applications, "");
				form.ShowControllerEditForm();
				AssertNotNull(form.JobApplicantControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(HRJobApplicantForm), form.JobApplicantControllerExposed.LastShownForm.GetType());
				HRJobApplicantForm activeForm = (HRJobApplicantForm)form.JobApplicantControllerExposed.LastShownForm;
				activeForm.Close();
			}

			applicant.Delete();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				form.ApplicationsGridExposed.SetDataBinding(applicant.Applications, "");
				form.ShowControllerEditForm();
				AssertNull("Form NOT Shown", form.JobApplicantControllerExposed.LastShownForm);
				AssertEquals("Msg shown about no item selected", "Ensure that a valid applicant and a valid application is selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestApplicationsGridDoubleClick_NullCheck()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				form.ApplicationsGridExposed.SetDataBinding(applicant.Applications, "");
				var newFactory = new BusinessObjectFactory()
				{ RefreshEnabled = false };
				newFactory.Load<HRJobApplicant>(applicant.PK).Delete();
				newFactory.Save();
				form.ShowControllerEditForm();
				AssertNull(form.JobApplicantControllerExposed.LastShownForm);
				AssertEquals("The selected record has been deleted by another user. It cannot be displayed.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestApplicationGridContextMenuPopup_Selected()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				var grid = form.ApplicationsGridExposed;
				grid.SetDataBinding(applicant.Applications, "");
				grid.SelectAllElements();
				form.EnableMenuItem();
				var menuItem = grid.ContextMenu.MenuItems.Find(ReapplyWorkflowTemplateMenuItemProvider.MenuItemName, false).Single();
				Assert(menuItem.Visible);
				Assert(menuItem.Enabled);
			}
		}

		public void TestApplicationGridContextMenuPopup_NonSelected()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				var grid = form.ApplicationsGridExposed;
				grid.SetDataBinding(applicant.Applications, "");
				form.EnableMenuItem();
				var menuItem = grid.ContextMenu.MenuItems.Find(ReapplyWorkflowTemplateMenuItemProvider.MenuItemName, false).Single();
				Assert(menuItem.Visible);
				Assert(!menuItem.Enabled);
			}
		}

		public void TestApplicationGridContextMenuPopup_NoApplicants()
		{
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				var grid = form.ApplicationsGridExposed;
				grid.SetDataBinding(null, "");
				grid.SelectAllElements();
				form.EnableMenuItem();
				var menuItem = grid.ContextMenu.MenuItems.Find(ReapplyWorkflowTemplateMenuItemProvider.MenuItemName, false).Single();
				Assert(menuItem.Visible);
				Assert(!menuItem.Enabled);
			}
		}

		public void TestApplicationsGridClickNew()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(jobCampaign))
			{
				form.Show();
				form.ShowControllerNewForm();
				AssertNotNull(form.JobApplicantControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(HRJobApplicantForm), form.JobApplicantControllerExposed.LastShownForm.GetType());
				HRJobApplicantForm activeForm = (HRJobApplicantForm)form.JobApplicantControllerExposed.LastShownForm;
				activeForm.Close();
			}
		}

		public void TestApplicationsGridAttach()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplicant applicantA = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantA.HA_FullName = "A";
			HRJobApplicant applicantB = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantB.HA_FullName = "B";
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(campaign))
			{
				form.Show();
				form.AttachApplicants();
				AssertApplicantExists(form, applicantA);
				AssertApplicantExists(form, applicantB);
				AssertEquals("Expect number of applications", 2, form.BusinessEntity.Applications.Count);
			}
		}

		public void TestApplicationsGridAttach_NoApplicants()
		{
			HRRecruitmentJobCampaign campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			Factory.Save();
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(campaign))
			{
				form.Show();
				form.AttachApplicants();
				AssertEquals("Expect number of applications", 0, form.BusinessEntity.Applications.Count);
			}
		}

		public void AssertApplicantExists(HRJobOpeningsForm form, HRJobApplicant applicant)
		{
			var application = form.BusinessEntity.Applications.FirstOrDefault(a => a.Applicant.HA_FullName == applicant.HA_FullName);
			AssertNotNull($"Application with applicant full name of {applicant.HA_FullName} does not exist", application);
		}

		public void AssertApplicantDoesNotExists(HRJobOpeningsForm form, HRJobApplicant applicant)
		{
			var application = form.BusinessEntity.Applications.FirstOrDefault(a => a.Applicant.HA_FullName == applicant.HA_FullName);
			AssertNull($"Application with applicant full name of {applicant.HA_FullName} exists", application);
		}

		public void TestApplicationsGridDetach()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplicant applicantA = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantA.HA_FullName = "A";
			var applicationA = applicantA.Applications.AddNew();
			applicationA.HP_HV = jobCampaign.PK;
			HRJobApplicant applicantB = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantB.HA_FullName = "B";
			var applicationB = applicantB.Applications.AddNew();
			applicationB.HP_HV = jobCampaign.PK;
			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var savedJobCampaign = newFactory.LoadTop1<HRRecruitmentJobCampaign>(new ZQuery(HRRecruitmentJobCampaignSchema.PK, jobCampaign.PK));
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(savedJobCampaign))
			{
				form.Show();
				form.ApplicationsGridExposed.Select(0);
				var selectedApplication = form.ApplicationsGridExposed.SelectedElements[0] as HRJobApplication;
				form.DetachApplicants();
				var applicantToKeep = selectedApplication.PK == applicationA.PK ? applicantB : applicantA;
				var applicantToRemove = selectedApplication.PK == applicationA.PK ? applicantA : applicantB;
				AssertApplicantDoesNotExists(form, applicantToRemove);
				AssertApplicantExists(form, applicantToKeep);
				AssertEquals("Expect number of applications", 1, form.BusinessEntity.Applications.Count);
			}
		}

		public void TestApplicationsGridDetach_NoneSelected()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplicant applicantA = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantA.HA_FullName = "A";
			var applicationA = applicantA.Applications.AddNew();
			applicationA.HP_HV = jobCampaign.PK;
			HRJobApplicant applicantB = Factory.NewWithValidTestData<HRJobApplicant>();
			applicantB.HA_FullName = "B";
			var applicationB = applicantB.Applications.AddNew();
			applicationB.HP_HV = jobCampaign.PK;
			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var savedJobCampaign = newFactory.LoadTop1<HRRecruitmentJobCampaign>(new ZQuery(HRRecruitmentJobCampaignSchema.PK, jobCampaign.PK));
			using (HRJobOpeningsFormForTest form = new HRJobOpeningsFormForTest(savedJobCampaign))
			{
				form.Show();
				form.DetachApplicants();
				AssertApplicantExists(form, applicantA);
				AssertApplicantExists(form, applicantB);
				AssertEquals("Expect number of applications", 2, form.BusinessEntity.Applications.Count);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new HRJobOpeningsForm(Factory.NewWithValidTestData<HRRecruitmentJobCampaign>());
		}

		class EmbeddedModulePopupTest : ZArchitecture.GUI.Internal.EmbeddedModulePopup
		{
			public EmbeddedModulePopupTest(ZFilterModule filterModule) : base(filterModule)
			{
			}

			public void ClickFindButton()
			{
				var stripControl = this.FindSingle<ZFilterStripBaseControl>();
				stripControl.Find();
			}

			public void SelectAllElementsOnGrid()
			{
				Module.DisplayGrid.SelectAllElements();
			}

			public void ClickOKButton()
			{
				var buttonControls = Controls.Find("OK_Button", true);
				if (buttonControls.Length > 0)
				{
					((ZButton)buttonControls[0]).PerformClick();
				}
			}
		}

		class HRJobOpeningsFormForTest : HRJobOpeningsForm
		{
			public HRJobOpeningsFormForTest(HRRecruitmentJobCampaign jobCampaign) : base(jobCampaign)
			{
			}

			internal ZGrid ApplicationsGridExposed
			{
				get
				{
					return ApplicationsGrid;
				}
			}

			internal ZController JobApplicantControllerExposed
			{
				get
				{
					return JobApplicantController;
				}
			}

			internal ToolStripItem AttachButton
			{
				get
				{
					return buttonsToolStripRight.Items["AttachApplicantButton"];
				}
			}

			internal EmbeddedModulePopupTest LastShownAttachPopup
			{
				get
				{
					return (EmbeddedModulePopupTest)lastShownAttachPopup;
				}
			}

			protected override void InstantiateEmbeddedModulePopup(ZFilterModule filterModule)
			{
				lastShownAttachPopup = new EmbeddedModulePopupTest(filterModule);
			}

			protected override void HandlePopupUserEntry()
			{
				lastShownAttachPopup.Show();
				Application.DoEvents();
				((EmbeddedModulePopupTest)lastShownAttachPopup).ClickFindButton();
				Application.DoEvents();
				((EmbeddedModulePopupTest)lastShownAttachPopup).SelectAllElementsOnGrid();
				Application.DoEvents();
				lastShownAttachPopup.IsSelectionMandatory = false;
				((EmbeddedModulePopupTest)lastShownAttachPopup).ClickOKButton();
				Application.DoEvents();
			}
		}
		#endregion
	}
}
