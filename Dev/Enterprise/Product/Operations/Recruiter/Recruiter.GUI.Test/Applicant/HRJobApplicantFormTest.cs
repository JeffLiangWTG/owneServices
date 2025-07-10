using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRJobApplicantForm))]
	public class HRJobApplicantFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			return new HRJobApplicantForm(applicant);
		}

		public void TestFormCaption()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			using (HRJobApplicantForm form = new HRJobApplicantForm(applicant))
			{
				form.Show();
				AssertEquals("Form caption ", "Applicant", form.FormCaption);
			}

			applicant.HA_FullName = "William Smith";
			using (HRJobApplicantForm form = new HRJobApplicantForm(applicant))
			{
				form.Show();
				AssertEquals("Form caption ", "Applicant : William Smith", form.FormCaption);
			}
		}

		public void TestAuditColumnExists()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
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
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
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

		void AssertColumn(ZGrid grid, string columnName, string caption, bool visible)
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
			AssertNotNull(columnName + " column should exist", columnStyle);
			AssertEquals(columnName + " Caption", caption, columnStyle.Caption ?? columnStyle.CaptionResourceString?.Caption);
			AssertEquals(columnName + " Visible", visible, columnStyle.IsVisible);
		}

		public void TestApplicationsGridDoubleClick()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = applicant.Applications.AddNew();
			application1.HP_HV = jobCampaign.PK;
			Factory.Save();
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				form.ApplicationsGridExposed.SetDataBinding(applicant.Applications, "");
				form.ShowControllerEditForm();
				AssertNotNull(form.JobApplicationControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(HRJobApplicationForm), form.JobApplicationControllerExposed.LastShownForm.GetType());
				HRJobApplicationForm activeForm = (HRJobApplicationForm)form.JobApplicationControllerExposed.LastShownForm;
				activeForm.Close();
			}

			applicant.Applications.DeleteAll();
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				form.ApplicationsGridExposed.SetDataBinding(applicant.Applications, "");
				form.ShowControllerEditForm();
				AssertNull("Form NOT Shown", form.JobApplicationControllerExposed.LastShownForm);
				AssertEquals("Msg shown about saving an applicant first", "Cannot edit application values until the applicant is saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestApplicationsGridAllowAddNew()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				form.ApplicationsGridExposed.SetDataBinding(applicant, "Applications");
				form.Show();
				AssertEquals("Should be able to add a row to the grid", form.ApplicationsGridExposed.List.AllowNew, true);
			}
		}

		public void TestPhoneNumberControls()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			using (var form = new HRJobApplicantFormForTest(applicant))
			{
				Assert(form.HomePhoneNumberControl_Exposed.EnableValidStateColor);
				Assert(form.HomePhoneNumberControl_Exposed.ShowDiallerControl);
				Assert(form.HomePhoneNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!form.HomePhoneNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(!form.HomePhoneNumberControl_Exposed.ShowToolTip);
				Assert(form.WorkPhoneNumberControl_Exposed.EnableValidStateColor);
				Assert(form.WorkPhoneNumberControl_Exposed.ShowDiallerControl);
				Assert(form.WorkPhoneNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!form.WorkPhoneNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(!form.WorkPhoneNumberControl_Exposed.ShowToolTip);
				Assert(form.MobilePhoneNumberControl_Exposed.EnableValidStateColor);
				Assert(form.MobilePhoneNumberControl_Exposed.ShowDiallerControl);
				Assert(form.MobilePhoneNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!form.MobilePhoneNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(!form.MobilePhoneNumberControl_Exposed.ShowToolTip);
				Assert(form.FaxNumberControl_Exposed.EnableValidStateColor);
				Assert(!form.FaxNumberControl_Exposed.ShowDiallerControl);
				Assert(!form.FaxNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!form.FaxNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(!form.FaxNumberControl_Exposed.ShowToolTip);
			}
		}

		public void TestApplicationsSecurity()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				Env.Security.HRJobApplicationView.IsAllowed = true;
				form.ApplicationsGridExposed.SetDataBinding(applicant, "Applications");
				form.Show();
				AssertNotEquals("Should be able to open Applications tab page", "coveringLabel", form.ApplicationsTabPage_Exposed.Controls[0].Name);
			}

			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				Env.Security.HRJobApplicationView.IsAllowed = false;
				form.ApplicationsGridExposed.SetDataBinding(applicant, "Applications");
				form.Show();
				AssertEquals("Should not be able to open Applications tab page", "coveringLabel", form.ApplicationsTabPage_Exposed.Controls[0].Name);
			}

			using (HRJobApplicantFormForTest form = new HRJobApplicantFormForTest(applicant))
			{
				Env.Security.HRJobApplicationView.IsAllowed = true;
				Env.Security.HRJobApplicationEdit.IsAllowed = false;
				form.ApplicationsGridExposed.SetDataBinding(applicant, "Applications");
				form.Show();
				AssertNotEquals("Should be able to open Applications tab page", "coveringLabel", form.ApplicationsTabPage_Exposed.Controls[0].Name);
				foreach (ZGridColumnInfo column in form.ApplicationsGridExposed.ColumnStyles)
				{
					Assert("Should be able to open Applications tab page", column.IsReadOnly);
				}
			}
		}

		public void TestEditPersonButtonAvailability()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				AssertEquals(false, form.EditPersonButton_Exposed.Available);
			}

			Factory.Save();
			using (var form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				AssertEquals(true, form.EditPersonButton_Exposed.Available);
			}

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				AssertEquals(false, form.EditPersonButton_Exposed.Available);
			}
		}

		public void TestNoSaveButtonWhenOpenForm()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "test";
			applicant.HA_EmailAddress = "j@gmail.com";
			applicant.HA_HomePhone = "+ 61 4 10797921";
			applicant.HA_MobilePhone = "+ 61 4 10797921";
			applicant.HA_WorkPhone = "+610432345678";
			Factory.Save();
			using (var form = new HRJobApplicantFormForTest(applicant))
			{
				form.Show();
				AssertEquals("Should not have Save button enabled when opening", ODisplayMode.Browse, form.DisplayMode);
				applicant.HA_WorkPhone_Formatted = "+61 422 345 679";
				AssertEquals("Should have Save button enabled after changing number", ODisplayMode.Edit, form.DisplayMode);
				Factory.Save();
				AssertEquals("Should not have Save button enabled after saving", ODisplayMode.Browse, form.DisplayMode);
				applicant.HA_WorkPhone_Formatted = "+61 0422 345 679";
				AssertEquals("Should not have Save button enabled after adding leading 0", ODisplayMode.Browse, form.DisplayMode);
			}
		}

		class HRJobApplicantFormForTest : HRJobApplicantForm
		{
			public HRJobApplicantFormForTest(HRJobApplicant applicant) : base(applicant)
			{
			}

			internal TabControl MainTabControlExposed
			{
				get
				{
					return MainTabControl;
				}
			}

			internal ZGrid ApplicationsGridExposed
			{
				get
				{
					return ApplicationsGrid;
				}
			}

			internal ZController JobApplicationControllerExposed
			{
				get
				{
					return JobApplicationController;
				}
			}

			internal new void ShowControllerEditForm()
			{
				base.ShowControllerEditForm();
			}

			internal PhoneNumberUserControl HomePhoneNumberControl_Exposed
			{
				get
				{
					return HomePhoneNumberControl;
				}
			}

			internal PhoneNumberUserControl WorkPhoneNumberControl_Exposed
			{
				get
				{
					return WorkPhoneNumberControl;
				}
			}

			internal PhoneNumberUserControl MobilePhoneNumberControl_Exposed
			{
				get
				{
					return MobilePhoneNumberControl;
				}
			}

			internal PhoneNumberUserControl FaxNumberControl_Exposed
			{
				get
				{
					return FaxNumberControl;
				}
			}

			internal ZTabPage ApplicationsTabPage_Exposed
			{
				get
				{
					return ApplicationsTabPage;
				}
			}

			internal ZToolStripButton EditPersonButton_Exposed => (Controls.Find("EditToolStrip", true).Single() as ZToolStrip).Items.Find("EditPersonButton", true).Single() as ZToolStripButton;
		}
	}
}
