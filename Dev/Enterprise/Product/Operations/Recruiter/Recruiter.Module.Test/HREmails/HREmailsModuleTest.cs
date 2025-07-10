using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HREmailsModule))]
	public class HREmailsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.HREmails;
		}

		public void TestCheckpoints()
		{
			using (var module = new HREmailsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.HREmails, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new HREmailsModule())
			{
				Assert(!module.AllowNew);
			}
		}

		public void TestAllowDelete()
		{
			Env.Security.HREmailsDelete.IsAllowed = false;
			using (var module = new HREmailsModule())
			{
				Assert(module.AllowDelete);
			}

			Env.Security.HREmailsDelete.IsAllowed = true;
			using (var module = new HREmailsModule())
			{
				Assert(module.AllowDelete);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new HREmailsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Correct control type", filterControl is HREmailsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestNewStandardMenuItems()
		{
			using (var form = new ZForm())
			using (var module = (HREmailsModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Reply"));
				AssertNotNull(module.ToolBarButtons.FindByText("Reply All"));
				AssertNotNull(module.ToolBarButtons.FindByText("Forward"));
				var jobApplicationButton = module.ToolBarButtons.FindByText("Job Application");
				AssertNotNull(jobApplicationButton);
				AssertNotNull(jobApplicationButton.DropDownMenu.MenuItems.FindByText("Create Job Application"));
				AssertNotNull(jobApplicationButton.DropDownMenu.MenuItems.FindByText("Attach to Job Application"));
			}
		}

		public void TestViewInOutlookExpress()
		{
			var item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = HREmails.Code;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "test@cargowise.com";
			Factory.Save();
			var mockModule = new Mock<HREmailsModuleForTest>();
			mockModule.CallBase = true;
			var mailShownInOutlook = false;
			mockModule.Protected()
				.Setup("ShowMailInOutlookExpress", ItExpr.IsAny<MailItem>())
				.Callback(delegate
				{ mailShownInOutlook = true; });
			using (var module = mockModule.Object)
			{
				var result = module.ShowViewForm_Exposed(item);
				AssertEquals(true, mailShownInOutlook);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(result);
				mailShownInOutlook = false;
				mockModule.Protected()
					.Setup("ShowMailInOutlookExpress", ItExpr.IsAny<MailItem>())
					.Throws(new InvalidOperationException("Some test exception"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				result = module.ShowViewForm_Exposed(item);
				AssertEquals(false, mailShownInOutlook);
				AssertEquals("There was a problem showing this email in Microsoft Outlook Express.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(result);
				result.Dispose();
			}
			mockModule.VerifyAll();
		}

		public void TestMenuItemSecurity()
		{
			using (var form = new ZForm())
			using (var module = (HREmailsModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Env.Security.HREmailsReply.IsAllowed = false;
				var reply = (ZToolBarButton)module.ToolBarButtons.FindByText("Reply");
				reply.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Recruiter -> HR Emails -> Reply", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				var replyAll = (ZToolBarButton)module.ToolBarButtons.FindByText("Reply All");
				replyAll.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Recruiter -> HR Emails -> Reply", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				Env.Security.HREmailsForward.IsAllowed = false;
				var forward = (ZToolBarButton)module.ToolBarButtons.FindByText("Forward");
				forward.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Recruiter -> HR Emails -> Forward", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				Env.Security.HRJobApplicationNew.IsAllowed = false;
				var createJobApplication = module.ToolBarButtons.FindByText("Job Application").DropDownMenu.MenuItems.FindByText("Create Job Application");
				createJobApplication.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Recruiter -> Job Application -> New", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				Env.Security.HRJobApplicationEdit.IsAllowed = false;
				var attachJobApplication = module.ToolBarButtons.FindByText("Job Application").DropDownMenu.MenuItems.FindByText("Attach to Job Application");
				attachJobApplication.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Recruiter -> Job Application -> Edit", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		[StressTest]
		public override void TestModuleShowsAndCanSearch()
		{
			base.TestModuleShowsAndCanSearch();
		}

		public void TestCreateJobApplication()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
			var hrEmail = Factory.NewWithValidTestData<HREmails>();
			hrEmail.MI_Application = HREmails.Code;
			hrEmail.MI_Direction = MailDirection.Receive;
			var emailForTest = new EmailBuilderForTesting();
			emailForTest.From("test@cargowise.com");
			emailForTest.Subject("attach this to CS00000101");
			hrEmail.RawMIMEString = emailForTest.GetEmail();
			Factory.Save();
			using (var module = new HREmailsModuleForTest())
			{
				try
				{
					using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						Env.Security.HRJobApplicationNew.IsAllowed = true;
						var createJobApplication = module.ToolBarButtons.FindByText("Job Application").DropDownMenu.MenuItems.FindByText("Create Job Application");
						createJobApplication.PerformClick();
						UserIdleWorker.Flush();
						AssertNotNull(module.CreateJobApplicationFormForTest);
						var application = (HRJobApplication)module.CreateJobApplicationFormForTest.BusinessEntity;
						AssertEquals("test@cargowise.com", application.Applicant.HA_EmailAddress);
						AssertEquals(1, application.DocManagerInfo.Files.Count);
					}
				}
				finally
				{
					module.CreateJobApplicationFormForTest?.Dispose();
				}
			}
		}

		public void TestCreateJobApplication_EmptyEmail()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(Business.Testing.HRJobApplicantTest).Assembly))
			{
				var rules = new EmailParsingRuleCollection();
				var rule = rules.AddNew();
				rule.ReferringPartyCode = "UNK";
				rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
				RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
				var hrEmail = Factory.NewWithValidTestData<HREmails>();
				hrEmail.MI_Application = HREmails.Code;
				hrEmail.MI_Direction = MailDirection.Receive;
				var emailForTest = new EmailBuilderForTesting();
				emailForTest.From("test@cargowise.com");
				emailForTest.Subject("attach this to CS00000101");
				var emptyPdfPath = resourceRetriever.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.pdf", "empty.pdf");
				emailForTest.WithAttachment(emptyPdfPath);
				hrEmail.RawMIMEString = emailForTest.GetEmail();
				Factory.Save();
				using (var module = new HREmailsModuleForTest())
				{
					try
					{
						module.TestEmptyEmailAddress = true;
						using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
						{
							Env.Security.HRJobApplicationNew.IsAllowed = true;
							var createJobApplication = module.ToolBarButtons.FindByText("Job Application").DropDownMenu.MenuItems.FindByText("Create Job Application");
							createJobApplication.PerformClick();
							UserIdleWorker.Flush();
							AssertNotNull(module.CreateJobApplicationFormForTest);
							var application = (HRJobApplication)module.CreateJobApplicationFormForTest.BusinessEntity;
							AssertNull(application.Applicant); // It's null because the form was closed without saving and the Applicant had errors
							AssertNotNull(ZFormModaliser.LastFormShownDialogForTest as HRJobApplicantForm);
						}
					}
					finally
					{
						module.CreateJobApplicationFormForTest?.Dispose();
					}
				}
			}
		}

		#region Implementation
		public class HREmailsModuleForTest : HREmailsModule
		{
			public bool TestEmptyEmailAddress;
			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}

			public IZForm ShowViewForm_Exposed(BusinessObject selectedBusinessObject)
			{
				return base.ShowViewForm(selectedBusinessObject);
			}

			protected override HREmails[] GetSelectedEmails() => Factory.Load<HREmails>(new ZQuery(MailDBItemsSchema.MI_Application, HREmails.Code));
			protected override HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(MailItem mailItem, DaxtraResumeParser daxtraResumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail)
			{
				var parser = new HRJobApplicationEmailParserForTest(mailItem, daxtraResumeParser, factory, createApplicantWithEmptyEmail);
				parser.TestCouldNotParseResume = false;
				parser.TestApplicantEmailAddress = mailItem.MI_From;
				parser.TestEmptyEmailAddress = TestEmptyEmailAddress;
				return parser;
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var item = collection.Factory.NewWithValidTestData<HREmails>();
			item.MI_Direction = MailDirection.Receive;
			item.MI_Application = "HRE";
			collection.Factory.Save();
			base.AddTestObjects(collection);
		}
		#endregion
	}

	public sealed class HRJobApplicationEmailParserForTest : HRJobApplicationEmailParser
	{
		public bool TestCouldNotParseResume;
		public string TestApplicantEmailAddress;
		public bool TestEmptyEmailAddress;
		readonly bool blowup;
		readonly bool hasError;

		public HRJobApplicationEmailParserForTest(MailItem mailItem, IApplicantResumeParser resumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail, bool blowup = false, bool hasError = false)
			: base(mailItem, resumeParser, factory, createApplicantWithEmptyEmail, allowMatchJobOpening: false)
		{
			this.blowup = blowup;
			this.hasError = hasError;
		}

		protected override IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
		{
			if (TestCouldNotParseResume)
			{
				return new ApplicantResumeParseResult();
			}

			var applicant = new ApplicantResume();
			applicant.Name = "Test FullName";
			if (TestEmptyEmailAddress)
			{
				applicant.EmailAddress = string.Empty;
			}
			else
			{
				applicant.EmailAddress = string.IsNullOrEmpty(TestApplicantEmailAddress) ? "match.applicant@gmail.com" : TestApplicantEmailAddress;
			}
			return new ApplicantResumeParseResult() { ParsedResume = applicant };
		}

		protected override List<ParseDataToBeProcessed> ParseMailItemCore(EmailData emailData, HRRecruitmentJobCampaign jobOpening, string filename = null)
		{
			if (blowup)
			{
				throw new Exception("boom");
			}

			var result = base.ParseMailItemCore(emailData, jobOpening, filename);

			if (hasError)
			{
				Parent.ApplicationOverallRatingDescription = "Wrong one";
			}

			return result;
		}
	}
}
