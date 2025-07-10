using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HRJobApplicationForm))]
	sealed class HRJobApplicationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "don antonio";
			applicant.HA_EmailAddress = "email@email.com";
			application.HP_HA = applicant.PK;
			return new HRJobApplicationForm(application);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get
			{
				return true;
			}
		}

		public void TestFormCaption()
		{
			var application = Factory.New<HRJobApplication>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";
			application.HP_HA = applicant.PK;
			Factory.Save();
			using (var form = new HRJobApplicationForm(application))
			{
				form.Show();
				AssertEquals("Form caption ", "Application : name", form.FormCaption);
			}

			application.Applicant.HA_FullName = "William Smith";
			using (var form = new HRJobApplicationForm(application))
			{
				form.Show();
				AssertEquals("Form caption ", "Application : William Smith", form.FormCaption);
			}
		}

		public void TestMainTabControl_TabPageOrder()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();
				form.MainTabControlExposed.SelectedIndex = 0;
				AssertEquals("Interviews tab page should be first", "Details", form.MainTabControlExposed.SelectedTab.Text);
				form.MainTabControlExposed.SelectedIndex = 1;
				AssertEquals("Workflow and Tracking tab page should be second", "Workflow && Tracking", form.MainTabControlExposed.SelectedTab.Text);
				form.MainTabControlExposed.SelectedIndex = 2;
				AssertEquals("eDocs tab page should be third", "eDocs", form.MainTabControlExposed.SelectedTab.Text);
				form.MainTabControlExposed.SelectedIndex = 3;
				AssertEquals("Notes tab page should be fourth", "Notes", form.MainTabControlExposed.SelectedTab.Text);
				form.MainTabControlExposed.SelectedIndex = 4;
				AssertEquals("Logs tab page should be fifth", "Logs", form.MainTabControlExposed.SelectedTab.Text);
			}
		}

		public void TestApplicantReadOnly()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				AssertEquals("When application not saved applicant should not be read only", false, form.ApplicantGuidFindBoxExposed.ReadOnly);
			}

			Factory.Save();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				AssertEquals("When application saved applicant should be read only", true, form.ApplicantGuidFindBoxExposed.ReadOnly);
			}
		}

		public void TestResetControlsVisibility()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = "STF";
			Factory.Save();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("ReferringOrgGuidFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringPersonGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ReferringStaffFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringOrgGuidZGuidDropEdit", true).Single().Visible);
				application.HP_SourceType = "MAN";
				AssertEquals(true, form.Controls.Find("ReferringOrgGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ReferringPersonGuidFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringStaffFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringOrgGuidZGuidDropEdit", true).Single().Visible);
				application.HP_SourceType = "WEB";
				AssertEquals(false, form.Controls.Find("ReferringOrgGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ReferringPersonGuidFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringStaffFindBox", true).Single().Visible);
				AssertEquals(false, form.Controls.Find("ReferringOrgGuidZGuidDropEdit", true).Single().Visible);
			}
		}

		public void TestProcessResumeDragDropWithoutDaxtra()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			Factory.Save();
			var configs = new ReferringPartyConfigurationCollection();
			var config1 = configs.AddNew();
			config1.Domain = "@wisetechglobal.com";
			config1.ReferringParty = "OH";
			config1.DefaultReferringSource = "AGT";
			config1.OrganizationPK = org.PK;
			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
				form.Show();
				form.OnDragDrop_Expose(args);
				AssertEquals("AGT", application.HP_SourceType);
				AssertEquals(org.PK, application.HP_OH_ReferringOrganisation);
			}
		}

		public void TestReferringPersonGuidFindBoxModuleID()
		{
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_SourceType = "MAN";
			Factory.Save();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();
				AssertEquals(ModuleIDs.GlbPerson, form.Controls.Find("ReferringPersonGuidFindBox", true).OfType<ZGuidFindBox>().Single().ModuleID);
			}
		}

		public void TestDocumentAddedToParsingQueue()
		{
			SetupDocType();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			var emptyDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.docx", "empty.docx");
			application.DocManagerInfo.AddFileOrDocument(emptyDocxPath, "CVR");
			Factory.Save();
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();
				var donAntonioResumeDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio resume.docx", "Don Antonio resume.docx");
				var document = application.DocManagerInfo.AddFileOrDocument(donAntonioResumeDocxPath, "CVR");
				var donAntonioCoverLetterDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio Cover letter.docx", "Don Antonio Cover letter.docx");
				application.DocManagerInfo.AddFileOrDocument(donAntonioCoverLetterDocxPath, "MSC");
				form.FireSaveButton();
				var parsingQueue = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery());
				AssertEquals("Only just added resume and cover letter should be added to the parsing queue", 1, parsingQueue.Length);
				AssertEquals(document.UniqueKey, parsingQueue[0].HPQ_StorageDocReference);
				AssertEquals("CVR", document.DocType);
				AssertEquals("Don Antonio resume.docx", document.FileName);
			}
		}

		public void TestDragDropResumeCallsConversion_OnDetailsTabPage()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			Factory.Save();
			var convertedApplications = new List<HRJobApplication>();
			var mock = new Mock<IResumeConverter>();
			mock.Setup(m => m.ConvertAvailableResumes(It.IsAny<HRJobApplication>()))
				.Callback<HRJobApplication>((app) =>
				{
					convertedApplications.Add(app);
				});

			using (ObjectFactory.Substitute(mock.Object))
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();
				Application.DoEvents();
				var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
				form.OnDragDrop_Expose(args);
				AssertEquals("The convert method should be called for the current application", application.PK, convertedApplications.Single().PK);
				mock.VerifyAll();
			}
		}

		public void TestDragDropCallsBaseDragAndDrop_OnEDocsTabPage()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			Factory.Save();
			var convertedApplications = new List<HRJobApplication>();
			var mock = new Mock<IResumeConverter>();
			_ = mock.Setup(m => m.ConvertAvailableResumes(It.IsAny<HRJobApplication>()))
				.Callback<HRJobApplication>((app) =>
				{
					convertedApplications.Add(app);
				});

			using (ObjectFactory.Substitute(mock.Object))
			using (var form = new HRJobApplicationFormForTest(application))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl", 1);
				var eDocTabPage = (ZTabPage)mainTabControl.TabPages["eDocsTabPage"];
				mainTabControl.SelectedTab = eDocTabPage;

				Application.DoEvents();
				var args = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[] { EmptyMsgPath }), 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
				form.OnDragDrop_Expose(args);

				AssertEquals(0, convertedApplications.Count);
				AssertEquals(1, ((IDocManagerSupport)application).DocManagerInfo.AllEDocs.Count);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Business.Testing.EmailParsingRuleTest).Assembly));

		string emptyMsgPath;
		string EmptyMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyMsgPath))
				{
					emptyMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.msg", "empty.msg");
				}
				return emptyMsgPath;
			}
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		void SetupDocType()
		{
			var cvr = Factory.New<RefDocType>();
			cvr.RT_DocType = "CVR";
			cvr.RT_Desc = "resume";
			cvr.RT_ReferenceType = "ALL";
			var cvl = Factory.New<RefDocType>();
			cvl.RT_DocType = "CVL";
			cvl.RT_Desc = "cover";
			cvl.RT_ReferenceType = "ALL";
			Factory.Save();
			RecruiterDataRegistry.Instance.DocTypeCV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cvr.PK.ToGuid());
			RecruiterDataRegistry.Instance.DocTypeCoverLetter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cvl.PK.ToGuid());
		}

		class HRJobApplicationFormForTest : HRJobApplicationForm
		{
			public HRJobApplicationFormForTest(HRJobApplication application) : base(application)
			{
			}

			internal TabControl MainTabControlExposed
			{
				get
				{
					return MainTabControl;
				}
			}

			internal TabControl BottomTabControlExposed
			{
				get
				{
					return BottomTabControl;
				}
			}

			internal ZGuidFindBox ApplicantGuidFindBoxExposed
			{
				get
				{
					return ApplicantGuidFindBox;
				}
			}

			public void OnDragDrop_Expose(DragEventArgs dragEvent)
			{
				base.OnDragDrop(dragEvent);
			}
		}
	}
}
