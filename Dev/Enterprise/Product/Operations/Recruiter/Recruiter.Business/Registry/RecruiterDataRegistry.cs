using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public sealed class RecruiterDataRegistry : RegistryItemSet
	{
		RecruiterDataRegistry()
		{
		}

		#region Instance

		public static RecruiterDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new RecruiterDataRegistry();
				}

				return fInstance;
			}
		}
		[ThreadStatic]
		static RecruiterDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Recruiter_ApplicantEmailOptions { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("3539d2ed-d1b5-41b1-bd9c-9b70ff85362b", "Applicant Email Options")); } }
			public static MultilingualString Recruiter_Daxtra { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("8D00C7C9-8E03-4E2D-8FC5-E0D2D1770A7A", "{0} Service", DaxtraName)); } }
			public static MultilingualString Recruiter_Logging { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("D9B902F4-97A9-4AB6-BBB0-9D5A3A9FEDFB", "Logging")); } }
			public static MultilingualString Recruiter_EmailServiceTask { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("067F3487-1F84-46DB-AE46-52BF2C70FE9A", "HR Email Service Task")); } }
			public static MultilingualString Recruiter_EmailServiceTask_OAuth2 { get { return CombineCategories(Recruiter_EmailServiceTask, ResString.GetMultilingualString("76E77D68-9402-4503-A749-84BC033478B4", "{0} 2.0", "OAuth")); } }
			public static MultilingualString Recruiter_ReferringParty { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("3f6cc761-340f-4111-bdca-3b1fcab5bccc", "Recruitment Referring Party")); } }
			public static MultilingualString Recruiter_TalentManagementSystem { get { return CombineCategories(Recruiter, ResString.GetMultilingualString("1536c2ae-ea31-420c-9a5f-b7c57d883d5f", "Talent Management System")); } }
		}

		#endregion

		#region Test Types

		public RecruiterTestTypeRegistryItem RecruiterTestTypes
		{
			get
			{
				return GetItem("RecruiterTestTypes", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("57575086-4bc7-48d9-b87a-52ea58a08cf9", "Recruiter Test Types");
					MultilingualString hint = ResString.GetMultilingualString("83133f8e-5c44-41b3-8cdd-d02a8a6a39fb", @"The list of test types that are used for accrediting and evaluating the suitability of job applicants.

Category is used for determining the inclusion and grouping of test types on the Learning Center web portal. You can have more than one test types within the same category.
There are three categories to choose from which correspond to the sections within the Learning Center web portal: Accreditations, Assessments, and Skill Tests.
If category is not specified, the test will not be shown on the Learning Center web portal.

Notification Type determines the type of notification which is sent to the test participant when the test is completed.
If Deliver Document is specified, a Document can be appointed as the form of notification delivery to be delivered upon test completion (these Documents can be configured from Job Applicant -> Skills -> Skill Test Ratings -> Documents -> Customize Documents).
If Deliver Test Completion Email is specified, the email specified in the Test Completion Email Template Registry Item will be delivered upon test completion.
If Do Not Deliver is specified, notifications will not be delivered upon test completion.

If test note is specified, it will be shown to the test takers on the Start page of all Learning Centers. This note should be in HTML format.");

					RecruiterTestTypeCollection defaultValue = new RecruiterTestTypeCollection();
					defaultValue.Add("STD", string.Format("You can set these test types in the System Registry, under {0}/{1}", RawDataRegistry.Categories.Recruiter.GetUnresolvedString(), caption.GetUnresolvedString()));

					return new RecruiterTestTypeRegistryItem(
						"RecruiterTestTypes",
						RawDataRegistry.Categories.Recruiter,
						caption,
						hint,
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		#endregion

		#region Job Skill Types

		public CodeDescriptionBoolRegistryItem JobSkillTestVersionList
		{
			get
			{
				return GetItem("JobSkillTestVersionList", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("15AD5A29-64B3-4A46-A0CD-984B9FD5E6D4", "Job Skill Test Version List");

					var defaultValue = new CodeDescriptionBoolCollection();
					defaultValue.AddSystemDefined("STD", ResString.GetMultilingualString("A43D2FF3-FE66-4F6F-84F3-5C9B8E54FFDF", "Standard"), true);
					defaultValue.Add("REF", ResString.GetMultilingualString("7EEA5573-7F3D-4CD8-B6CF-24BFD8494439", "Refresher"), true);

					return new CodeDescriptionBoolRegistryItem(
						"JobSkillTestVersionList",
						RawDataRegistry.Categories.Recruiter,
						caption,
						ResString.GetMultilingualString("AC34BB67-ABF7-4CA9-9676-F4DB26C1D74A", "This list defines Job Skill Test Versions."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("432CC476-4990-4C32-8556-029C78A5E756", "Not Used"), false),
						defaultValue);
				});
			}
		}

		#endregion

		#region Application Statuses

		public ApplicationStatusRegistryItem ApplicationStatuses
		{
			get
			{
				return GetItem("RecruiterApplicationStatuses", delegate
				{
					ApplicationStatusCollection defaultValue = new ApplicationStatusCollection();

					defaultValue.AddPair("INP", (NoResString)"In Progress"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("ST1", (NoResString)"Assessment Stage 1"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("ST2", (NoResString)"Assessment Stage 2"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("ST3", (NoResString)"Assessment Stage 3"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("ST4", (NoResString)"Assessment Stage 4"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("DEF", (NoResString)"Deferred pending information"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("REJ", (NoResString)"Rejected"); // Default text for non-multilingual registry item should not be localized
					defaultValue.AddPair("ACC", (NoResString)"Accepted"); // Default text for non-multilingual registry item should not be localized

					return new ApplicationStatusRegistryItem(
						"RecruiterApplicationStatuses",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("13cebe56-429b-4610-8dff-9700570ae1b6", "Application Statuses"),
						ResString.GetMultilingualString("fb0a7ea5-f79d-4798-a931-f54474124490", "This list of valid statuses for a job application. You can assign an email template against the status to be sent out through the Job Openings screen."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#region Ad Placement Publications List

		public CodeDescriptionPairListRegistryItem AdPlacementPublicationsList
		{
			get
			{
				return GetItem("RecruiterAdPlacementPublicationsList", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("e120f8a4-3fcb-40da-8821-e58e03074c46", "Ad Placement Publications List");

					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair("STD", ResString.GetMultilingualString("2ec24d53-34ac-4af0-8551-c1fbe01150fa", "You can set these publication types in the System Registry, under {0}/{1}", RawDataRegistry.Categories.Recruiter, caption));

					return new CodeDescriptionPairListRegistryItem(
						"RecruiterAdPlacementPublicationsList",
						RawDataRegistry.Categories.Recruiter,
						caption,
						ResString.GetMultilingualString("e4565178-02d9-473c-b426-077501f78a5a", "This list of publications used to publish Job Advertisements."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#region Interview Statuses

		public CodeDescriptionPairListRegistryItem InterviewStatuses
		{
			get
			{
				return GetItem("RecruiterInterviewStatuses", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("NQR", ResString.GetMultilingualString("f0ba3650-bbe6-4e79-9718-7dfcc59e0a28", "Not Required"));
					defaultValue.AddPair("BKD", ResString.GetMultilingualString("faf0d994-4011-47f1-9a93-4e5bd7955d7b", "Booked"));
					defaultValue.AddPair("CMP", ResString.GetMultilingualString("aac8fff6-24f8-4fe8-9cbd-e95b3b937314", "Completed"));

					return new CodeDescriptionPairListRegistryItem(
						"RecruiterInterviewStatuses",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("2a5a6e47-f4c9-48fe-9837-2515b77a1bbb", "Interview Statuses"),
						ResString.GetMultilingualString("09400d90-0b69-4d6d-bbe8-4e57c8d47bde", "The list of valid entries for status of Job Interviews."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#region Applicant Email Options

		#region Email Subject

		public StringRegistryItem ApplicantEmailSubject
		{
			get
			{
				return GetItem("RecruiterApplicantEmailSubject", delegate
				{
					return new StringRegistryItem(
						"RecruiterApplicantEmailSubject",
						Categories.Recruiter_ApplicantEmailOptions,
						ResString.GetMultilingualString("0642c8a5-b297-4cd9-a6c6-74935d30ffa0", "Applicant Email Subject"),
						ResString.GetMultilingualString("fc3aa688-1859-473c-aafb-5bbb7b509b5f", "This is the default email subject that will be used when sending emails to job applicants."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						(NoResString)"Thank you for coming"); // Default text for non-multilingual registry item should not be localized
				});
			}
		}

		#endregion

		#region Email Body

		public StringRegistryItem ApplicantEmailBody
		{
			get
			{
				return GetItem("RecruiterApplicantEmailBody", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"RecruiterApplicantEmailBody",
						Categories.Recruiter_ApplicantEmailOptions,
						ResString.GetMultilingualString("21c220db-1bfe-452e-b35f-ff3b43bdfaba", "Applicant Email Body"),
						ResString.GetMultilingualString("5a7d10ee-bd06-4aaa-897c-4c3516a19838", "This is the default text of emails sent to job applicants."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						string.Format((NoResString)"Thank you for your recent response to our advertised position. We have received a considerable number of applicants. At this stage we are in receipt of applications from candidates that more closely match our selection criteria, so we will not be proceeding with your application at this time.{0}{1}We will keep your application on file and should a suitable position become available, we will be in further contact.{2}{3}We thank you for your interest and wish you well in your job search.{4}{5}Yours faithfully,", System.Environment.NewLine, System.Environment.NewLine, System.Environment.NewLine, System.Environment.NewLine, System.Environment.NewLine, System.Environment.NewLine)); // Default text for non-multilingual registry item should not be localized

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region AllowEditingLearningCentreTestResults

		public BooleanRegistryItem AllowEditingLearningCentreTestResults
		{
			get
			{
				return GetItem("AllowEditingLearningCentreTestResults", delegate
				{
					return new BooleanRegistryItem(
						"AllowEditingLearningCentreTestResults",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("f0a44d18-2e72-4174-b766-37c876db399f", "Allow Editing of Learning Center Result"),
						ResString.GetMultilingualString("cc4b67f1-8f8d-4a88-87e9-9eea3366d9ea", "This will allow users to edit Learning Center results from the Job Applicant module."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						false);
				});
			}
		}

		#endregion

		#region LearningCentreUserRegistrationNotificationGroup

		public GuidRegistryItem LearningCentreUserRegistrationNotificationGroup
		{
			get
			{
				return GetItem("LearningCentreUserRegistrationNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"LearningCentreUserRegistrationNotificationGroup",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("132dbf29-d248-4c50-b2d9-98f54dbe93b4", "Learning Center User Registration Notification Group"),
						ResString.GetMultilingualString("bd6d6f2c-43af-4827-b5e5-3695d8e71ba1", "Group to be notified when a new user registration is made via Learning Center site.\r\nWhen anonymous registration is disabled, this group is responsible in approving new registrations."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region LearningCentreSiteName

		public StringRegistryItem LearningCentreSiteName
		{
			get
			{
				return GetItem("LearningCentreSiteName", delegate
				{
					return new StringRegistryItem(
						"LearningCentreSiteName",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("10130946-2219-4865-96a2-9eac967379ac", "Learning Center Site Name"),
						ResString.GetMultilingualString("45f7358a-8cf6-46f0-ba16-2a2fb19fda7c", "Specify the site name for Learning Center"),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						(NoResString)"Learning Center"); // Default text for non-multilingual registry item should not be localized
				});
			}
		}

		#endregion

		#region LearningCentreRegistrationPage

		public StringRegistryItem LearningCentreRegistrationPage
		{
			get
			{
				return GetItem("LearningCentreRegistrationPage", delegate
				{
					return new StringRegistryItem(
						"LearningCentreRegistrationPage",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("ecc8ba1b-9378-40b8-a9a5-58c801933778", "Learning Center User Registration Page"),
						ResString.GetMultilingualString("f2036a19-933a-4b1a-8b01-7e8dca04df72", "Specify the URL of user registration page for Learning Center. Leave it blank to use the default page."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						"");
				});
			}
		}

		#endregion

		#region AllowAnonymousLearningCentreUserRegistration

		public BooleanRegistryItem AllowAnonymousLearningCentreUserRegistration
		{
			get
			{
				return GetItem("AllowAnonymousLearningCentreUserRegistration", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"AllowAnonymousLearningCentreUserRegistration",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("5f0008b9-e3d0-4bb8-b334-72e4800fb202", "Allow Anonymous Learning Center User Registration"),
						ResString.GetMultilingualString("8c4845fc-65ba-43b4-a0ac-a3b6f46f8f58", "When disabled, new user registrations will have to be approved by the notification group before web access is allowed."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						true);
					result.DataType = new AllowAnonymousCertificateUserRegistrationRegistryDataType();
					return result;
				});
			}
		}

#if DEBUG
		public
#endif
		class AllowAnonymousCertificateUserRegistrationRegistryDataType : BooleanRegistryDataType
		{
			protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				if (!proposedValue && Instance.LearningCentreUserRegistrationNotificationGroup.Value == Guid.Empty)
				{
					string errorMessage = Res.GetString("7b05608c-a61a-4970-ac99-e71d46492651", "Please specify a value for \"{0}\" before disabling the anonymous user registration",
						Instance.LearningCentreUserRegistrationNotificationGroup.Caption);
					throw new RegistryValidationException(errorMessage);
				}
			}
		}

		#endregion

		#region CompulsoryJobSkill

		public GuidRegistryItem CompulsoryJobSkillPK
		{
			get
			{
				return GetItem("CompulsoryJobSkill", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CompulsoryJobSkill",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("3d8875ba-5055-4b7e-a7b5-bcd86ffce7a0", "Compulsory Job Skill"),
						ResString.GetMultilingualString("0907f5b6-d16f-4fb8-9e83-d43426f6aaf6", "Specify a job skill code that is compulsory for all job applicants"),
						RegistryStorageFlags.System);
					return result;
				});
			}
		}

		#endregion

		#region Work Permit Status

		public CodeDescriptionBoolRegistryItem WorkPermitStatusList
		{
			get
			{
				return GetItem("WorkPermitStatus", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(5);
					defaultValue.Add(Core.Constants.WorkPermitStatuses.Residence, ResString.GetMultilingualString("14cace7b-1094-4364-8dfc-59aa72b034ba", "Resident of this Country/Region"), true);
					defaultValue.Add(Core.Constants.WorkPermitStatuses.SkillVisa, ResString.GetMultilingualString("1b3a751c-27f9-4248-b6e4-7c392bb5c0f7", "Skill Migration Visa"), true);
					defaultValue.Add(Core.Constants.WorkPermitStatuses.WorkPermit, ResString.GetMultilingualString("3cbd7669-ec58-4f81-8158-b538d2f0b9c8", "Work Permit"), true);
					defaultValue.Add(Core.Constants.WorkPermitStatuses.Student, ResString.GetMultilingualString("2305411e-d5e5-48b2-ba34-2affba70ccbd", "Student"), true);
					defaultValue.Add(Core.Constants.WorkPermitStatuses.None, ResString.GetMultilingualString("eabaae73-cf69-432d-adeb-732114b9f46b", "None"), true);

					return new CodeDescriptionBoolRegistryItem(
							"WorkPermitStatus",
							RawDataRegistry.Categories.Recruiter,
							ResString.GetMultilingualString("bb8548fa-1814-4b19-8ef4-db6dd6947b07", "Work Permit Status"),
							ResString.GetMultilingualString("eb10f73b-a1a7-4ecb-9f51-7e4078bf110f", "The list of valid entries for work permit status."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("fcb1483e-0732-4eaa-bbe4-c86472b6b8d6", "Enabled")),
							defaultValue);
				});
			}
		}

		#endregion

		#region Availability List

		public CodeDescriptionBoolRegistryItem AvailabilityList
		{
			get
			{
				return GetItem("AvailabilityList", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(4);
					defaultValue.Add(Core.Constants.Availabilties.Casual, ResString.GetMultilingualString("035ad76d-303f-4b1a-9d1a-8078b7d50776", "Casual"), true);
					defaultValue.Add(Core.Constants.Availabilties.Contract, ResString.GetMultilingualString("08180d2b-2b64-4a5d-a698-1fed771dabda", "Contract"), true);
					defaultValue.Add(Core.Constants.Availabilties.FullTime, ResString.GetMultilingualString("8ccb1342-341f-43c1-a04a-8e10f355678a", "Full Time"), true);
					defaultValue.Add(Core.Constants.Availabilties.PartTime, ResString.GetMultilingualString("f71b2b4c-3eb3-40a7-8fc2-39248be5185e", "Part Time"), true);

					return new CodeDescriptionBoolRegistryItem(
							"AvailabilityList",
							RawDataRegistry.Categories.Recruiter,
							ResString.GetMultilingualString("d074bacb-12dd-46ba-bfc5-3317da44a7e0", "Availabilities"),
							ResString.GetMultilingualString("15d1d921-8ad6-47fc-8bc4-dbb5debc94cb", "The list of valid entries for availabilities"),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("4887a119-ef5d-47c1-a076-8dcc44899fc6", "Enabled")),
							defaultValue);
				});
			}
		}

		#endregion

		#region Certificate Types

		public CodeDescriptionBoolRegistryItem CertificateTypesExtra
		{
			get
			{
				return GetItem("CertificateTypesExtra", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection();
					defaultValue.Add(Core.Constants.RefDocTypes.CustomsCertificate, ResString.GetMultilingualString("035ad76d-303f-4b1a-9d1a-8078b7d50771", "Customer's Certificate"), true);

					return new CodeDescriptionBoolRegistryItem(
							"CertificateTypesExtra",
							RawDataRegistry.Categories.Recruiter,
							ResString.GetMultilingualString("d074bacb-12dd-46ba-bfc5-3317da44a7e1", "Certificate Types"),
							ResString.GetMultilingualString("15d1d921-8ad6-47fc-8bc4-dbb5debc94c1", "The list of valid entries for applicant certificate or accreditation type."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("4887a119-ef5d-47c1-a076-8dcc44899fc1", "Enabled")),
							defaultValue);
				});
			}
		}

		#endregion

		#region CertificateCodeSpecialialisationMapping

		public CertificationCodeMappingRegistryItem CertificateCodeSpecialialisationMapping
		{
			get
			{
				return GetItem("CertificateCodeSpecialialisationMapping",
					() => new CertificationCodeMappingRegistryItem("CertificateCodeSpecialialisationMapping", RawDataRegistry.Categories.Recruiter));
			}
		}

		#endregion

		#region Test Expiry

		public NotificationEmailTemplateRegistryItem ReminderEmailTemplate
		{
			get
			{
				return GetItem("ReminderEmailTemplate", delegate
				{
					NotificationEmailTemplate defaultValue = new NotificationEmailTemplate();
					return new NotificationEmailTemplateRegistryItem(
						"ReminderEmailTemplate",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("8956AB4D-A233-418d-95E2-DC82B0254E18", "Reminder Email Template"),
						ResString.GetMultilingualString("BA6AD219-2976-47bb-BA5A-2007CB3AF88B", "Specify the email template that will be sent out to the candidates when they have not yet completed the required tests"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						typeof(DocHRJobApplicant),
						defaultReminderEmailSubject,
						defaultReminderEmailBody);
				});
			}
		}

		public IntRegistryItem RefresherAttemptGracePeriodMonths
		{
			get
			{
				return GetItem("RefresherAttemptGracePeriodMonths", delegate
				{
					return new IntRegistryItem(
						"RefresherAttemptGracePeriodMonths",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("40AEEEA3-87FD-4EE4-8E8C-46D62DAB60BC", "Refresher Attempt Grace Period"),
						ResString.GetMultilingualString("93DFBC10-98A6-4F06-AB39-8EECFB5073D5", "This registry allows you to set the number of months prior to the expiry of the current Refresher attempt in which a new subsequent refresher attempt will be created"),
						RegistryStorageFlags.System,
						4);
				});
			}
		}

		public IntRegistryItem TestExpiryPeriodInDays
		{
			get
			{
				return GetItem("TestExpiryPeriodInDays", delegate
				{
					return new IntRegistryItem(
						"TestExpiryPeriodInDays",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("A29246AD-2479-4b74-9478-A9D913CBF5BE", "Test Expiry Period in Days"),
						ResString.GetMultilingualString("C7F817A0-B171-4d2f-A043-D4BD32C64ED4", "Specify the number of days a recruitment test has to be completed by"),
						RegistryStorageFlags.System,
						7);
				});
			}
		}

		const string defaultReminderEmailSubject = "Your job application for (*CompanyName*)";

		const string defaultReminderEmailBody =
@"-----------------------------------------------------------------------------------
PLEASE NOTE - This is a system generated email, please do not reply.
-----------------------------------------------------------------------------------

This is a courtesy reminder in relation to your job application(s)
for (*CompanyName*).

You are currently applying for:
(*InProgressJobApplications*)

(*AllTestsSentNotCompleted*)

Regards,
(*CompanyName*)

";

		#endregion

		#region Exam Landing Page Footer Text

		public MultilingualStringRegistryItem ExamLandingPageFooterText
		{
			get
			{
				return GetItem("ExamLandingPageFooterText", () =>
					new MultilingualStringRegistryItem(
						"ExamLandingPageFooterText", RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("afb6cfb5-6e34-4fe4-845f-da501326a8ce", "Exam Landing Page Footer Text"),
						ResString.GetMultilingualString("d0356c4f-7de4-4452-b15e-684b9add923f", "Specifies the footer text for Exam"),
						RegistryStorageFlags.System)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) });
			}
		}

		public MultilingualStringRegistryItem ExamLandingPageFooterTextGroupedExam
		{
			get
			{
				return GetItem("ExamLandingPageFooterTextGroupedExam", () =>
					new MultilingualStringRegistryItem(
						"ExamLandingPageFooterTextGroupedExam", RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("386e4e64-5ab4-4def-9140-4c4c9efa6d9f", "Exam Landing Page Footer Text - Grouped Exam"),
						ResString.GetMultilingualString("d465f07c-99b0-4755-92d5-fbd0d2800ca7", "Specifies the footer text for Grouped Exam"),
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("4ca1fb1b-5541-4d35-8cc3-61bd7fbde0da", @"<b>IMPORTANT:</b> Please read this before taking the exam. <br>* Results will only be recorded and submitted when a section has been completed.<br>* Do not press the back button between sections.<br> * Once a section has been submitted, the results cannot be altered. You can only retake a section once the exam’s resit period has elapsed. <br>"))
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) });
			}
		}

		#endregion

		#region Learning Centre Certificate Document Menu Name

		public StringRegistryItem LearningCenterCertificateDocumentMenuName
		{
			get
			{
				return GetItem("LearningCenterCertificateDocumentMenuName", delegate
				{
					return new StringRegistryItem(
						"LearningCenterCertificateDocumentMenuName",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("e25f7d9c-82b4-4e83-b50e-1104d62befc2", "Learning Center Certificate Document Menu Name"),
						ResString.GetMultilingualString("d6ebeed7-903d-4f3f-8817-4c2fcea42a1a", "Specify the Document Menu name for Learning Center Certificate"),
						RegistryStorageFlags.System,
						(NoResString)"Certificate Document"); // Default text for non-multilingual registry item should not be localized
				});
			}
		}

		#endregion

		#region NewJobApplicationEmailTemplate

		public NotificationEmailTemplateRegistryItem NewJobApplicationEmailTemplate
		{
			get
			{
				return GetItem("NewJobApplicationEmailTemplate", delegate
				{
					NotificationEmailTemplate defaultValue = new NotificationEmailTemplate();
					return new NotificationEmailTemplateRegistryItem(
						"NewJobApplicationEmailTemplate",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("CDDF240F-48FD-4BCB-85A1-0A0551A3B02D", "New Job Application Email Template"),
						ResString.GetMultilingualString("DAFAF79D-97FD-4593-B8D4-5986EBCBE014", "Specify the email template that will be sent out to the candidates when they apply for a position"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						typeof(DocHRJobApplication),
						defaultNewJobApplicationEmailSubject,
						defaultNewJobApplicationEmailBody);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry default value")]
		const string defaultNewJobApplicationEmailSubject = "Your job application for (*CompanyName*)";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry default value")]
		const string defaultNewJobApplicationEmailBody =
@"-----------------------------------------------------------------------------------
PLEASE NOTE - This is a system generated email, please do not reply.
-----------------------------------------------------------------------------------

Thank you for your interest in the advertised position (*AdTitle*).

(*CompulsoryTestsToBeCompleted*)

We appreciate your application and encourage you to apply for other positions when they arise.

Regards,
(*CompanyName*)

";

		#endregion

		#region OnlineApplicationDocTypes

		public OnlineApplicationDocTypesRegistryItem OnlineApplicationDocTypes
		{
			get
			{
				return GetItem("OnlineApplicationDocTypes", delegate
				{
					return new OnlineApplicationDocTypesRegistryItem("OnlineApplicationDocTypes", RawDataRegistry.Categories.Recruiter);
				});
			}
		}

		public GuidRegistryItem DocTypeCV
		{
			get
			{
				return GetItem("DocTypeCV", delegate
				{
					var defaultCV = new BusinessObjectFactory().LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Resume));
					var result = new GuidRegistryItem(
						"DocTypeCV",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("6CC9DBB0-972F-4709-B3C4-6DCD7A8C6162", "Document Type Resume"),
						ResString.GetMultilingualString("AA51F8C8-5BF9-4907-9F4A-144EEA7B454A", "Document Type for Applicant Resume"),
						RegistryStorageFlags.System,
						defaultCV?.PK.ToGuid() ?? Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType);
					return result;
				});
			}
		}

		public IRefDocType GetDocTypeCV(BusinessObjectFactory factory)
		{
			return factory.Load<IRefDocType>(DocTypeCV.Value);
		}

		(Guid pk, string code) docTypeCache;
		public string DocTypeCVCode
		{
			get
			{
				var pk = DocTypeCV.Value;
				if (pk != docTypeCache.pk)
				{
					docTypeCache = (pk, GetDocTypeCV(new BusinessObjectFactory())?.RT_DocType ?? string.Empty);
				}

				return docTypeCache.code;
			}
		}

		public GuidRegistryItem DocTypeCoverLetter
		{
			get
			{
				return GetItem("DocTypeCoverLetter", delegate
				{
					var result = new GuidRegistryItem(
						"DocTypeCoverLetter",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("7988C0E0-3C8A-4242-B57A-99045417068B", "Document Type Cover Letter"),
						ResString.GetMultilingualString("1DF553D6-7508-44BC-89F1-F5B60E728A78", "Document Type for Applicant Cover Letter"),
						RegistryStorageFlags.System);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType);
					return result;
				});
			}
		}

		public IRefDocType GetDocTypeCoverLetter(BusinessObjectFactory factory)
		{
			return factory.Load<IRefDocType>(DocTypeCoverLetter.Value);
		}

		public GuidRegistryItem DocTypeReferringSource
		{
			get
			{
				return GetItem("DocTypeReferringSource", delegate
				{
					var result = new GuidRegistryItem(
						"DocTypeReferringSource",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("17629A03-A63D-459B-8DCB-CA86FCD2FE90", "Document Type Referring Source"),
						ResString.GetMultilingualString("C26C0FD9-8401-4443-B932-EA4CA3D085ED", "Document Type for application email content"),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefDocType);
					return result;
				});
			}
		}

		public IRefDocType GetDocTypeReferringSource(BusinessObjectFactory factory)
		{
			return factory.Load<IRefDocType>(DocTypeReferringSource.Value);
		}

		#endregion

		#region NewJobApplicationNotificationEmailTemplate

		public NotificationEmailTemplateRegistryItem NewJobApplicationNotificationEmailTemplate
		{
			get
			{
				return GetItem("NewJobApplicationNotificationEmailTemplate", delegate
				{
					NotificationEmailTemplate defaultValue = new NotificationEmailTemplate();
					return new NotificationEmailTemplateRegistryItem(
						"NewJobApplicationNotificationEmailTemplate",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("6a422633-dfc7-4ffb-88b3-6cfc89e8c8d9", "New Job Application Notification Email Template"),
						ResString.GetMultilingualString("aa859a90-6b2c-4212-8a45-9936c679f5db", "Specify the email template that will be sent out to the notification group when job applicants apply for a position."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						typeof(DocHRJobApplication),
						defaultNewJobApplicationNotificationEmailSubject,
						defaultNewJobApplicationNotificationEmailBody);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry default value")]
		const string defaultNewJobApplicationNotificationEmailSubject = "Job application for position (*AdTitle*)";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry default value")]
		const string defaultNewJobApplicationNotificationEmailBody = "(*ApplicantName*) ((*ApplicantEmailAddress*)) has applied for the advertised position (*AdTitle*).";

		#endregion

		#region NewJobApplicationNotificationGroup

		public GuidRegistryItem NewJobApplicationNotificationGroup
		{
			get
			{
				return GetItem("NewJobApplicationNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NewJobApplicationNotificationGroup",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("6d5da063-4aa7-4de3-81f5-e387df069aef", "New Job Application Notification Group"),
						ResString.GetMultilingualString("755831cc-4046-44cf-8cb1-c0f33a80c5a5", "Group to be notified when job applicants apply for a position."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Daxtra

		public BooleanRegistryItem DaxtraEnable
		{
			get
			{
				return GetItem("DaxtraEnable", delegate
				{
					return new BooleanRegistryItem(
						"DaxtraEnable",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("3E0873D8-48EB-48C0-8DAA-2197024AD8D0", "Enable {0} Service", DaxtraName),
						ResString.GetMultilingualString("A7D9FCD6-1F9B-4192-BD1D-CF034E56E2F2", "Enable a {0} service for parsing resumes in Job Applications.", DaxtraName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public StringRegistryItem DaxtraServiceUrl
		{
			get
			{
				return GetItem("DaxtraServiceUrl", delegate
				{
					return new StringRegistryItem(
						"DaxtraServiceUrl",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("AFA30F1A-C171-4AD2-969A-DEF83B075265", "{0} Service URL", DaxtraName),
						ResString.GetMultilingualString("BEFFA3C3-9503-408C-AF6D-C54726112F88", "A {0} service URL for resume parsing", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
				});
			}
		}

		public StringRegistryItem DaxtraAccountName
		{
			get
			{
				return GetItem("DaxtraAccountName", delegate
				{
					return new StringRegistryItem(
						"DaxtraAccountName",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("25AAD309-D882-41F9-8200-BB529ABAE6E6", "{0} Account Name", DaxtraName),
						ResString.GetMultilingualString("A92D801D-81E9-400F-933E-2E71B5CD8E8E", "A {0} service Account Name for resume parsing", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
				});
			}
		}

		public StringRegistryItem DaxtraSubjectRegex
		{
			get
			{
				return GetItem("DaxtraSubjectRegex", delegate
				{
					return new StringRegistryItem(
						"DaxtraSubjectRegex",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("959445F7-682C-46C9-9B51-D0B616D75701", "{0} Subject Regular Expression", DaxtraName),
						ResString.GetMultilingualString("E4D7CBC1-1DCE-4C8A-AA85-1E93CC36460B", "A regular expression for extracting Job Opening from the email subject. Please use {0} variable for the suggested Job Opening location.", AdTitleVariable),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						DaxtraRegexDefault);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is a regular expression and stays as it is")]
		const string DaxtraRegexDefault = "((Application for )(?<adTitle0>.*)( from .*))|((respond now:.*applied to )(?<adTitle1>.*)( on glassdoor))|((application received for )(?<adTitle2>.*))";

		public IntRegistryItem DaxtraTimeout
		{
			get
			{
				return GetItem("DaxtraTimeout", delegate
				{
					return new IntRegistryItem(
						"DaxtraTimeout",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("4D0FB562-B14E-40E5-B5F5-56CADF88BEEE", "{0} Call Timeout", DaxtraName),
						ResString.GetMultilingualString("9801558C-6670-43F1-9BA7-CAB3D0379258", "A timeout for the {0} web service call in seconds. Zero for the unlimited wait.", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						30);
				});
			}
		}

		public IntRegistryItem DaxtraBatchSize
		{
			get
			{
				return GetItem("DaxtraBatchSize", delegate
				{
					return new IntRegistryItem(
						"DaxtraBatchSize",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("F99470C1-7BD1-4166-A1C6-3475CDD1FDB3", "{0} Batch Size", DaxtraName),
						ResString.GetMultilingualString("EEA16B17-0CCB-4F0D-8E45-1C5141518529", "A number of files to send to {0} web service in one batch.", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						100);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem DaxtraResumeAcceptedFormats
		{
			get
			{
				return GetItem("DaxtraResumeAcceptedFormats", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(5);
					defaultValue.Add("PDF", ResString.GetMultilingualString("31D0D7CC-CCA2-4B1F-B9BC-F6CC946B80CF", "PDF file format"));
					defaultValue.Add("DOC", ResString.GetMultilingualString("7A34C7DE-D5BD-4B51-A97E-A74C7F33C7AB", "DOC file format"));
					defaultValue.Add("DOCX", ResString.GetMultilingualString("D39A93F6-CA28-4C97-87C5-BDE2311DCDCE", "DOCX file format"));
					defaultValue.Add("TXT", ResString.GetMultilingualString("11A2FD9B-BF24-49CC-8377-6801C2C25D43", "TXT file format"));
					defaultValue.Add("HTM", ResString.GetMultilingualString("CFC5B956-9D75-43C5-952A-65759CCE1954", "HTM file format"));
					defaultValue.Add("HTML", ResString.GetMultilingualString("D9A3F353-E89A-41C2-9750-EE8ACF2C146F", "HTML file format"));

					var result = new CodeDescriptionBoolRegistryItem(
						"DaxtraResumeAcceptedFormats",
						Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("0870E733-E937-401D-9508-431640E6AC27", "{0} Resume Accepted Formats", DaxtraName),
						ResString.GetMultilingualString("24CE7E06-2D3D-4DD3-943C-AA91336185A7", "Accepted file formats for the resumes parsed by {0} service.", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("0247C892-1126-4061-818B-E3F90B9F0B00", "Not Used"), false),
						defaultValue);

					return result;
				});
			}
		}

		public EmailParsingRuleRegistryItem DaxtraAutoParsingRules
		{
			get
			{
				return GetItem("DaxtraAutoParsingRules", delegate
				{
					var defaultValue = new EmailParsingRuleCollection();
					defaultValue.Add(new EmailParsingRule() { ReferringPartyCode = EmailParsingRuleReferringParties.Codes.Unknown, AllowParseAttachments = true, AllowFallbackToEmailBody = false });
					defaultValue.Add(new EmailParsingRule() { ReferringPartyCode = EmailParsingRuleReferringParties.Codes.Organization, AllowParseAttachments = true, AllowFallbackToEmailBody = true });
					defaultValue.Add(new EmailParsingRule() { ReferringPartyCode = EmailParsingRuleReferringParties.Codes.Staff, AllowParseAttachments = true, AllowFallbackToEmailBody = false });

					var result = new EmailParsingRuleRegistryItem(
						"DaxtraAutoParsingRules", Categories.Recruiter_Daxtra,
						ResString.GetMultilingualString("e5aee040-8aec-4ade-af34-773d3355c4a2", "{0} Auto Parsing Rules", DaxtraName),
						ResString.GetMultilingualString("d7a49b49-313d-4e95-9388-6908a8a4057f", "{0} Auto Parsing Rules", DaxtraName),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue);

					return result;
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cv parsing service name")]
		internal const string DaxtraName = "Daxtra";
		internal const string AdTitleVariable = "adTitle"; // regex variable name

		#endregion

		#region Logging

		public StringRegistryItem LoggingKafkaBrokers
		{
			get
			{
				return GetItem("LoggingKafkaBrokers",
					() => new StringRegistryItem(
						"LoggingKafkaBrokers",
						Categories.Recruiter_Logging,
						ResString.GetMultilingualString("31021766-c765-4900-a5a8-d7c4fbe588b2", "Kafka Target Brokers"),
						ResString.GetMultilingualString("f358d3a8-db9a-4fdb-83ce-9ccac8374d66", "Specifies the Kafka brokers for NLog Kafka target when logging parsing requests."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						(NoResString)"106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,108-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,109-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,110-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044"))
				;
			}
		}

		public StringRegistryItem LoggingKafkaTopic
		{
			get
			{
				return GetItem("LoggingKafkaTopic",
					() => new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"LoggingKafkaTopic",
							Categories.Recruiter_Logging,
							ResString.GetMultilingualString("010298e0-bd76-4082-af0b-585cc9bc6a09", "Kafka Target Topic"),
							ResString.GetMultilingualString("b2673657-9d52-40d6-b3cd-9c707f8433d2", "Specifies the Kafka topic for NLog Kafka target when logging parsing requests."),
							RegistryDataTypes.StringType,
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							LoggingKafkaTopicDefaultValueGetter))
					);
			}
		}

		string LoggingKafkaTopicDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.IsProductionSystem
				? "topic-au2-prod-hrm-logs-prod" // Constant string
				: "topic-au1-test-hrm-logs-test"; // Constant string
		}

		public StringRegistryItem LoggingKafkaTopicUsername
		{
			get
			{
				return GetItem("LoggingKafkaTopicUsername",
					() => new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"LoggingKafkaTopicUsername",
							Categories.Recruiter_Logging,
							ResString.GetMultilingualString("a497ab34-e8bb-41cf-a11e-67284e728c19", "Kafka Target Topic Username"),
							ResString.GetMultilingualString("b424137f-fb4f-4e66-9e93-a7c9b2eb5e3e", "Specifies the Kafka topic username for logging parsing requests."),
							RegistryDataTypes.StringType,
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							LoggingKafkaTopicUsernameDefaultValueGetter))
				);
			}
		}

		string LoggingKafkaTopicUsernameDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.IsProductionSystem
				? "topic-au2-prod-hrm-logs-prod" // Constant string
				: "topic-au1-test-hrm-logs-test"; // Constant string
		}

		public StringRegistryItem LoggingKafkaTopicPassword
		{
			get
			{
				return GetItem("LoggingKafkaTopicPassword",
					() => new StringRegistryItem(
						"LoggingKafkaTopicPassword",
						Categories.Recruiter_Logging,
						ResString.GetMultilingualString("a5ee76fe-3119-4a47-b6e8-d18fd6e0c107", "Kafka Target Topic Password"),
						ResString.GetMultilingualString("c053ed99-6de9-4f8f-9f5b-5e68dac80c6d", "Specifies the Kafka topic password for logging parsing requests."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty))
				;
			}
		}

		public CodePairRegistryItem LoggingMinLevel
		{
			get
			{
				return GetItem("LoggingMinLevel", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair((NoResString)"Trace", ResString.GetMultilingualString("79353cbe-88d3-492d-a5f2-db315ec155c7", "Most verbose level"));
						list.AddPair((NoResString)"Debug", ResString.GetMultilingualString("be18f665-2eb0-41d5-958d-72232f576f99", "Debugging level"));
						list.AddPair((NoResString)"Info", ResString.GetMultilingualString("754ff11a-8c5f-480e-8b16-79eefc5ffd82", "Information on application events"));
						list.AddPair((NoResString)"Warn", ResString.GetMultilingualString("08aa9541-5810-4043-ac13-06a4b82a5275", "Warnings about recoverable failures"));
						list.AddPair((NoResString)"Error", ResString.GetMultilingualString("51185364-030a-4a85-8c42-248ee12af253", "Errors about failures or caught exceptions"));
						list.AddPair((NoResString)"Fatal", ResString.GetMultilingualString("f70e039a-4d74-4e26-8b14-c7c7c6f37907", "Critical failures causing the application to abort"));
						list.AddPair((NoResString)"Off", ResString.GetMultilingualString("f3fb4c68-2be2-4c43-8ac5-950bd5186e7e", "Disable logging"));
						return list;
					});

					return new CodePairRegistryItem(
						"LoggingMinLevel",
						Categories.Recruiter_Logging,
						ResString.GetMultilingualString("a5d2e1b7-8d7a-4bd8-9397-e8cdfb392bb8", "Minimum Logging Level"),
						ResString.GetMultilingualString("35d810c5-c781-4588-854d-eedd5c407b22", "The default minimum logging level for Kafka logs."),
						listProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						NLog.LogLevel.Info.Name);
				});
			}
		}

		#endregion

		#region HREmailServiceTask

		readonly ResourceString pop3 = ResString.GetMultilingualString("8E748CCC-C9E9-46BF-BE21-38C19760060E", "POP3");

		public CodePairRegistryItem HREmailMailRetrievalProtocol
		{
			get
			{
				return GetItem("HREmailMailRetrievalProtocol", delegate
				{
					var mailRetrievalProtocolsListProvider = new CodeDescriptionPairListProvider(() => new MailRetrievalProtocols());

					return new CodePairRegistryItem(
						"HREmailMailRetrievalProtocol",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("43EB9878-17A2-4C27-AFA3-D816DD31185B", "Mail Retrieval Protocol"),
						ResString.GetMultilingualString("508286FC-8D32-4532-AF30-DB8F022C8FEC", "Protocol used to retrieve incoming mail"),
						mailRetrievalProtocolsListProvider,
						false, true, new ComboBoxRegistryEditorInfo(mailRetrievalProtocolsListProvider),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						MailRetrievalProtocols.POP3,
						false);
				});
			}
		}

		public IRegistryItem HREmailMailServer
		{
			get
			{
				return GetItem("HREmailMailServer", delegate
				{
					return new RawDataRegistry.PhysicalServerRegistryItem(
						"HREmailMailServer",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("487592B0-CC2E-455A-AC03-212C3EC69756", "Mail Server"),
						ResString.GetMultilingualString("EA3AD4F5-E401-405F-B516-E9E0C273E4F0", "Mail Server"),
						RegistryDataTypes.StringType,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						"");
				});
			}
		}

		public IRegistryItem HREmailMailServerPort
		{
			get
			{
				return GetItem("HREmailMailServerPort", delegate
				{
					return new RawDataRegistry.PhysicalServerRegistryItem(
						"HREmailMailServerPort",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("278FF990-EECB-4758-8621-DC76FE18F9D3", "Mail Server Port"),
						ResString.GetMultilingualString("C1B64067-2BAF-4159-A235-339AD368E51F", "Mail Server Port"),
						RegistryDataTypes.IntType,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						110);
				});
			}
		}

		public IRegistryItem HREmailMailboxUserName
		{
			get
			{
				return GetItem("HREmailMailboxUserName", delegate
				{
					return new RawDataRegistry.PhysicalServerRegistryItem(
						"HREmailMailboxUserName",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("5C66F9B9-7FD6-4D20-964A-77A26003713B", "Mailbox User Name"),
						ResString.GetMultilingualString("DC172939-805F-4508-8F34-DF3DFB154B2D", "e.g. user@example.com."),
						RegistryDataTypes.StringType,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						"");
				});
			}
		}

		public IRegistryItem HREmailMailboxPassword
		{
			get
			{
				return GetItem("HREmailMailboxPassword", delegate
				{
					var result = new RawDataRegistry.PhysicalServerRegistryItem(
						"HREmailMailboxPassword",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("BBAAA21B-9290-465F-B3E7-CAEB7AFD9EA2", "Mailbox Password"),
						ResString.GetMultilingualString("403D3565-782B-4E87-AC84-640605A407BA", "Mailbox Password"),
						RegistryDataTypes.StringType,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						"");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OAuth version")]
		const string oAuth = "OAuth 2.0";

		public BooleanRegistryItem HREUseOAuth2
		{
			get
			{
				return GetItem("HREUseOAuth2", delegate
				{
					return new BooleanRegistryItem(
						new RawDataRegistry.PhysicalServerRegistryItem("HREUseOAuth2",
							Categories.Recruiter_EmailServiceTask_OAuth2,
							ResString.GetMultilingualString("100D9F9C-A2CA-4FE7-B021-E49053D90BA2", "Enable {0} authentication", oAuth),
							ResString.GetMultilingualString("B3E424CC-F3DA-4CA2-9F44-1B925159C9E1", @"When enabled, {0} authentication will be used.", oAuth),
							RegistryDataTypes.BoolType,
							DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							false));
				});
			}
		}

		public StringRegistryItem HREMs365OAuth2TenantId
		{
			get
			{
				return GetItem("HREMs365OAuth2TenantId", () =>
				{
					var result = new StringRegistryItem("HREMs365OAuth2TenantId",
						Categories.Recruiter_EmailServiceTask_OAuth2,
						ResString.GetMultilingualString("8C3494E4-B195-49D4-B008-5EF59FE830FF", "Tenant ID"),
						ResString.GetMultilingualString("07598662-E8AF-421B-A8A8-65B58FCD250E", "The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						string.Empty);
					return result;
				});
			}
		}

		public StringRegistryItem HREMs365ApplicationId
		{
			get
			{
				return GetItem("HREMs365ApplicationId", () =>
				{
					var result = new StringRegistryItem("HREMs365ApplicationId",
						Categories.Recruiter_EmailServiceTask_OAuth2,
						ResString.GetMultilingualString("62094DAE-E4FC-4754-9CCD-116F1C4FB1C3", "Application ID"),
						ResString.GetMultilingualString("EA352277-8E42-4A0A-BAD8-0CB8C15FD07A", @"This is the Application ID registered in the Azure platform. It should be a unique identifier like '{0}'.

Important: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the {1} authentication.", "acc8304d-88d3-4caa-8452-2be8061d7fba", oAuth),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						string.Empty);
					return result;
				});
			}
		}

		public Ms365OAuth2TokenRegistryItem HREMs365OAuth2Token
		{
			get
			{
				return GetItem("HREMs365OAuth2Token", delegate
				{
					return new Ms365OAuth2TokenRegistryItem("HREMs365OAuth2Token",
						Categories.Recruiter_EmailServiceTask_OAuth2,
						ResString.GetMultilingualString("2E1DD996-45B1-4C69-A9E9-2C48F8F8FCB7", "{0} Access Token", oAuth),
						ResString.GetMultilingualString("377D9701-2A07-4BA6-8D5A-4165B5EE78F3", @"This setting stores the {0} Access Token that the HR Email Service Task will use during authentication.

Click on Grant Permissions to generate and store the {0} Access Token. If a token has already been generated, clicking on Grant Permissions will renew it. Click on the Clear button to clear the cached token.",
							oAuth),
						EmailType.Incoming,
						HREMs365OAuth2TenantId,
						HREMs365ApplicationId,
						null,
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden
					);
				});
			}
		}

		public CodePairRegistryItem HREmailIMAPSecureConnectionType
		{
			get
			{
				return GetItem("HREmailIMAPSecureConnectionType", delegate
				{
					var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

					return new CodePairRegistryItem(
						"HREmailIMAPSecureConnectionType",
						CombineCategories(Categories.Recruiter_EmailServiceTask, ResString.GetMultilingualString("672D30EE-5798-410B-A1CD-76D704EE8DE2", "IMAP")),
						ResString.GetMultilingualString("1B075ABA-FC17-4679-93F6-180DD01CF9C7", "IMAP Server Secure Connection"),
						ResString.GetMultilingualString("18E6B3E5-30B6-489F-9F07-611E4F363BF7", "The type of secure connection to use to connect to the IMAP mail server"),
						secureConnectionTypesListProvider,
						false, true, new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						SecureConnectionTypes.TLS,
						false);
				});
			}
		}

		public CodePairRegistryItem HREmailPOP3SecureConnectionType
		{
			get
			{
				return GetItem("HREmailPOP3SecureConnectionType", delegate
				{
					var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

					return new CodePairRegistryItem(
						"HREmailPOP3SecureConnectionType",
						CombineCategories(Categories.Recruiter_EmailServiceTask, pop3),
						ResString.GetMultilingualString("20AC4CED-AC83-41FD-B9F6-26B302841300", "POP3 Server Secure Connection"),
						ResString.GetMultilingualString("651A0E97-3664-4BBF-952E-0B2DAA884B3B", "The type of secure connection to use to connect to the POP3 mail server"),
						secureConnectionTypesListProvider,
						false, true, new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						SecureConnectionTypes.None,
						false);
				});
			}
		}

		public DateTimeRegistryItem HREmailStartDaxtra
		{
			get
			{
				return GetItem("HREmailStartDaxtra", delegate
				{
					return new DateTimeRegistryItem(
						"HREmailStartDaxtra",
						Categories.Recruiter_EmailServiceTask,
						ResString.GetMultilingualString("674ACC41-9A45-440D-BBA3-E578F30193F7", "Start date for HR emails processing"),
						ResString.GetMultilingualString("3E78E678-E07F-4289-B346-61460088231E", "Determine start date for backlog HR emails to be parsed. If the emails were received prior to this date, the application won't automatically create job application and job applicant."),
						RegistryStorageFlags.System,
						DaxtraEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		#endregion

		#region Referring Party

		public CodeDescriptionBoolRegistryItem ReferringSourcesTypes
		{
			get
			{
				return GetItem("ReferringSourcesTypes", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection();

					foreach (CodeDescriptionPair pair in new ReferringSourcesTypes())
					{
						defaultValue.AddSystemDefined(pair.Code, pair.MultilingualDescription, true);
					}

					return new CodeDescriptionBoolRegistryItem(
							"ReferringSourcesTypes",
							Categories.Recruiter_ReferringParty,
							ResString.GetMultilingualString("37dd356f-c5d3-418d-822e-181b57c46bf3", "Referring Source Types"),
							ResString.GetMultilingualString("ecda3ec8-ac56-4d06-813d-92df6a2c4ef3", "The list of Referring Source Types."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("a53f3dcb-d279-4016-bc63-d059484e959e", "Enabled")),
							defaultValue);
				});
			}
		}

		public GuidRegistryItem ReferringStaffMembersToIgnoreGroup
		{
			get
			{
				return GetItem("ReferringStaffMembersToIgnoreGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ReferringStaffMembersToIgnoreGroup",
						Categories.Recruiter_ReferringParty,
						ResString.GetMultilingualString("74f3fa25-18ea-49e6-873f-0044e0ffc4ce", "Referring Staff Members to Ignore"),
						ResString.GetMultilingualString("dda94f34-d1a5-484b-b11a-892d6bc50b87", "This allows internal staff members who work in the recruitment teams to forward emails to the HR Emails for registration but not be considered as a 'Referring Staff'."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public ReferringPartyConfigurationRegistryItem ReferringPartiesConfiguration
		{
			get
			{
				return GetItem("ReferringPartiesConfiguration", delegate
				{
					return new ReferringPartyConfigurationRegistryItem(
							"ReferringPartiesConfiguration",
							Categories.Recruiter_ReferringParty,
							ResString.GetMultilingualString("51f38318-10b3-4de6-b9a1-7ed29d43f3b7", "Recruitment Referring Parties"),
							ResString.GetMultilingualString("de5686a7-8b8a-46db-baf4-ddc6de144b0e", "The list of Recruitment Referring Parties."),
							RegistryStorageFlags.System,
							new ReferringPartyConfigurationCollection());
				});
			}
		}

		#endregion

		#region Simplified Online Application Page

		public BooleanRegistryItem SimplifiedOnlineApplicationPage
		{
			get
			{
				return GetItem("SimplifiedOnlineApplicationPage", delegate
				{
					return new BooleanRegistryItem(
						"SimplifiedOnlineApplicationPage",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("2DFA470E-0BC0-4629-B87A-C378894F6C21", "Simplified Online Application Page"),
						ResString.GetMultilingualString("7F578468-BED2-43EA-B7A7-825A7B2BB718", "Enable a simplified page in the Careers website."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region ExamMigrationStatus

		public CodeDescriptionBoolRegistryItem ExamMigrationStatus
		{
			get
			{
				return GetItem("ExamMigrationStatus", delegate
				{
					var defaults = new CodeDescriptionBoolCollection(11) {
						 { "CRT00000003", (NoResString)"WL Tariff Management and Quotations - Go Live", true },
						 { "CRT00000129", (NoResString)"WL General Functions - Go Live", true },
						 { "CRT00000131", (NoResString)"WL Generic Customs - Go Live", true },
						 { "CRT00000132", (NoResString)"WL Accounting: Operational Jobs - Go Live", true },
						 { "CRT00000133", (NoResString)"WL Product Warehouse - Go Live", true },
						 { "CRT00000134", (NoResString)"WL Orders - Go Live", true },
						 { "CRT00000135", (NoResString)"WL Web Tracker - Go Live", true },
						 { "CRT00000136", (NoResString)"WL Bookings - Go Live", true },
						 { "CRT00000139", (NoResString)"WL Organizations - Go Live", true },
						 { "CRT00000140", (NoResString)"WL Forwarding - Go Live", true },
						 { "CRT00000141", (NoResString)"WL Getting Started - Go Live", false },
						 { "CRT00000142", (NoResString)"WL US Customs: Exports - Go Live", true },
						 { "CRT00000143", (NoResString)"WL Transport Booking - Go Live", true },
						 { "CRT00000144", (NoResString)"WL Customize Documents - Config", true },
						 { "CRT00000145", (NoResString)"WL Accounting: Receivables - Go Live", true },
						 { "CRT00000146", (NoResString)"WL Accounting: Payables - Go Live", true },
						 { "CRT00000147", (NoResString)"WL Workflow - Go Live", true },
						 { "CRT00000148", (NoResString)"WL Documents - Generic - Go Live", true },
						 { "CRT00000149", (NoResString)"WL Liner & Agency - Go Live", true },
						 { "CRT00000150", (NoResString)"WL Accounting: Cash Books - Go Live", true },
						 { "CRT00000151", (NoResString)"WL Accounting: General Ledger - Go Live", true },
						 { "CRT00000152", (NoResString)"WL CFS - Go Live", true },
						 { "CRT00000153", (NoResString)"WL Sales & Marketing: CRM - Go Live", true },
						 { "CRT00000154", (NoResString)"WL US Customs: Import Declarations and ISF - Go Live", true },
						 { "CRT00000155", (NoResString)"WL US Customs: Inbond - Go Live", false },
						 { "CRT00000158", (NoResString)"WL Accounting: Job Costing - Go Live/Advanced", true },
						 { "CRT00000160", (NoResString)"WL CA Customs: Imports - Go Live", true },
						 { "CRT00000161", (NoResString)"WL TLX Transport", false },
						 { "CRT00000163", (NoResString)"WL TLX Accounts", false },
						 { "CRT00000164", (NoResString)"WL TLX Warehouse", false },
						 { "CRT00000165", (NoResString)"WL TLX Workshop and Purchase Orders", false },
						 { "CRT00000166", (NoResString)"WL Accounting: System Config - Go Live (Initial Setup)", true },
						 { "CRT00000167", (NoResString)"WL Core: System", true },
						 { "CRT00000168", (NoResString)"WL Core: Reference Files & Locations - Go Live", true },
						 { "CRT00000170", (NoResString)"WL Schedules - Go Live", true },
						 { "CRT00000171", (NoResString)"WL TLX Payroll", false },
						 { "CRT00000172", (NoResString)"WL TLX Mobility", false },
						 { "CRT00000173", (NoResString)"WL Warehouse Set Up - Config", true },
						 { "CRT00000174", (NoResString)"WL NZ Customs - Go Live", true },
						 { "CRT00000175", (NoResString)"WL AU Customs: Exports - Go Live", true },
						 { "CRT00000176", (NoResString)"WL Human Resources - Go Live", true },
						 { "CRT00000177", (NoResString)"WL Port Transport - Go Live", true },
						 { "CRT00000178", (NoResString)"WL AU Customs: Imports - Go Live", true },
						 { "CRT00000179", (NoResString)"WL Customize Reports - Config", true },
						 { "CRT00000180", (NoResString)"WL AU Customs: Air/Sea Cargo Report - Go Live", true },
						 { "CRT00000181", (NoResString)"Compliance Training:Whistleblower Protection Principles (Internal)", false },
						 { "CRT00000182", (NoResString)"General Data Protection Regulation (GDPR) (Internal)", false },
						 { "CRT00000184", (NoResString)"Compliance Training: Code of Conduct, Workplace Health & Safety, and Equal Opportunity Policy (Internal)", false },
						 { "CRT00000185", (NoResString)"Drum Buffer Rope (Internal)", false },
						 { "CRT00000186", (NoResString)"Critical Chain Project Management (Internal)", false },
						 { "CRT00000187", (NoResString)"Delivery Manager Role & Principles (Internal)", false },
						 { "CRT00000188", (NoResString)"WTG Culture (Internal)", false },
						 { "CRT00000189", (NoResString)"G", false },
						 { "CRT00000190", (NoResString)"WL Organizations - Advanced", false },
						 { "CRT00000191", (NoResString)"WL Forwarding - Advanced", false },
						 { "CRT00000193", (NoResString)"WL Generic Customs - Advanced", false },
						 { "CRT00000196", (NoResString)"WL ZA Customs - Go Live", true },
						 { "CRT00000197", (NoResString)"WL Sales & Marketing CRM - Advanced", false },
						 { "CRT00000198", (NoResString)"WL ZA Customs - Advanced", false },
						 { "CRT00000199", (NoResString)"WL GB Customs CCS UK - Go Live", true },
						 { "CRT00000200", (NoResString)"WL GB Customs: Exports - Go Live", false },
						 { "CRT00000201", (NoResString)"WL GB Customs: Imports - Go Live", true },
						 { "CRT00000202", (NoResString)"WL JP Customs AFR - Go Live", true },
						 { "CRT00000203", (NoResString)"WL CCP Re-Certification", true },
						 { "CRT00000205", (NoResString)"WL Product Warehouse - Advanced", false },
						 { "CRT00000206", (NoResString)"WL EDI (Electronic Messaging) - Go Live", true },
						 { "CRT00000207", (NoResString)"WL Accounting: Receivables - Advanced", false },
						 { "CRT00000208", (NoResString)"WL EU Customs - Go Live", false },
						 { "CRT00000209", (NoResString)"WL US Customs: Imports Declarations and ISF - Advanced", false },
						 { "CRT00000210", (NoResString)"WL Web Tracker - Advanced", false },
						 { "CRT00000211", (NoResString)"WL Core: Reference Files & Locations - Advanced", false },
						 { "CRT00000212", (NoResString)"WL GB Customs: Imports - Advanced", false },
						 { "CRT00000213", (NoResString)"WL US Customs: Expert and Config", false },
						 { "CRT00000214", (NoResString)"WL NZ Customs - Advanced", false },
						 { "CRT00000215", (NoResString)"WL Reports Generic - Go Live", false },
						 { "CRT00000216", (NoResString)"WL US Customs: Imports- Automated Manifest System", false },
						 { "CRT00000217", (NoResString)"WL Schedules - Advanced", false },
						 { "CRT00000218", (NoResString)"WL BorderWise - Go Live", false },
						 { "CRT00000219", (NoResString)"WL EU Customs - Expert and Config", false },
						 { "CRT00000220", (NoResString)"WL Core: User Admin - Config", true },
						 { "CRT00000221", (NoResString)"WL Warehouse RF - Go Live", false },
						 { "CRT00000222", (NoResString)"WL Warehouse RF - Advanced", false },
						 { "CRT00000223", (NoResString)"WL CA Customs: Exports - Go Live", true },
						 { "CRT00000225", (NoResString)"WL GB Customs CCS UK - Advanced", false },
						 { "CRT00000226", (NoResString)"WL Liner & Agency - Advanced", false },
						 { "CRT00000227", (NoResString)"WL GB Customs: Exports - Advanced", false },
						 { "CRT00000228", (NoResString)"WL Documents - Generic - Advanced", false },
						 { "CRT00000229", (NoResString)"WL Bookings - Advanced", false },
						 { "CRT00000230", (NoResString)"WL CA Customs: Imports - Advanced", false },
						 { "CRT00000231", (NoResString)"WL Warehouse - Expert", false },
						 { "CRT00000232", (NoResString)"WL CN Customs", false },
						 { "CRT00000233", (NoResString)"WL ExDocs - Go Live", false },
						 { "CRT00000234", (NoResString)"WL Web Tracker - Config and Expert", false },
						 { "CRT00000235", (NoResString)"WL Orders - Advanced", false },
						 { "CRT00000236", (NoResString)"Business Development Support Role (Internal)", false },
						 { "CRT00000237", (NoResString)"WL Language - Expert and Config", false },
						 { "CRT00000238", (NoResString)"WL CFS - Advanced", false },
						 { "CRT00000240", (NoResString)"WL BorderWise - Advanced", false },
						 { "CRT00000241", (NoResString)"WL GB Customs: Expert and Config", false },
						 { "CRT00000242", (NoResString)"WL US Customs: Exports - Advanced", false },
						 { "CRT00000243", (NoResString)"WL CCO Forwarding Re-Certification", true },
						 { "CRT00000244", (NoResString)"WL CCO Transport Re-Certification", true },
						 { "CRT00000245", (NoResString)"WL CCO Warehouse Re-Certification", true },
						 { "CRT00000246", (NoResString)"WL CCO Customs Re-Certification", true },
						 { "CRT00000247", (NoResString)"WL CCO Liner & Agency Re-Certification", true },
						 { "CRT00000248", (NoResString)"WL CCS Forwarding/Transport/Liner&Agency Re-Certification Exam", true },
						 { "CRT00000249", (NoResString)"WL CCS Forwarding/Customs/Transport Re-Certification Exam", true },
						 { "CRT00000250", (NoResString)"WL CCS Forwarding/Customs/Warehouse Re-Certification Exam", true },
						 { "CRT00000251", (NoResString)"WL CCS Forwarding/Warehouse/Transport Re-Certification Exam", true },
						 { "CRT00000252", (NoResString)"WL CCS Forwarding/Warehouse/Liner&Agency Re-Certification Exam", true },
						 { "CRT00000254", (NoResString)"WL CCS Warehouse/Transport/Liner&Agency Re-Certification Exam", true },
						 { "CRT00000255", (NoResString)"WL CCS Customs/Warehouse/Transport Re-Certification Exam", true },
						 { "CRT00000256", (NoResString)"WL CCS Customs/Transport/Liner&Agency Re-Certification Exam", true },
						 { "CRT00000257", (NoResString)"WL EU Customs - Advanced", false },
						 { "CRT00000259", (NoResString)"WL CCS Forwarding/Customs/Liner&Agency Re-Certification Exam", true },
						 { "CRT00000260", (NoResString)"WL Organizations - Expert", false },
						 { "CRT00000261", (NoResString)"PAVE - Beginners (Internal)", false },
						 { "CRT00000262", (NoResString)"WL Customs: Bonded Warehouse", false },
						 { "CRT00000263", (NoResString)"WL CA Customs - Config", false },
						 { "CRT00000264", (NoResString)"ASSESS: NUnit 3", false },
						 { "CRT00000265", (NoResString)"WL CargoSphere - Rate Search", false },
						 { "CRT00000266", (NoResString)"WL Getting Started - Advanced", false },
						 { "CRT00000268", (NoResString)"WL SG Customs - Go Live", false },
						 { "CRT00000269", (NoResString)"WL SG Customs - Config", false },
						 { "CRT00000270", (NoResString)"WL SG Customs - Advanced", false },
						 { "CRT00000271", (NoResString)"WL Accounting: Operational Jobs - Expert", false },
						 { "CRT00000272", (NoResString)"WL Accounting: Cash Book - Advanced", false },
						 { "CRT00000273", (NoResString)"WL Accounting: Payables - Advanced", false },
						 { "CRT00000274", (NoResString)"WL Accounting: Operational Jobs - Advanced", false },
						 { "CRT00000275", (NoResString)"WL AU Customs: Export ExDocs", false },
						 { "CRT00000276", (NoResString)"WL CSP Re-Certification", true },
						 { "CRT00000277", (NoResString)"ASSESS: CW1 Database Constraints", false },
						 { "CRT00000278", (NoResString)"WL Microlistics WMS Express Go Live - Desktop", false },
						 { "CRT00000279", (NoResString)"WL Microlistics WMS Express Go Live - RF", false },
						 { "CRT00000280", (NoResString)"WL SAS - Go Live", false },
						 { "CRT00000284", (NoResString)"WL SMF Go Live", false },
						 { "CRT00000286", (NoResString)"WL General Functions - Advanced", false },
						 { "CRT00000287", (NoResString)"WL CargoSphere - SUDS / Contract Management", false },
						 { "CRT00000288", (NoResString)"WL PTM - Parcel Shipping Go Live", false },
						 { "CRT00000289", (NoResString)"WL PTM - Personal Shipping Application - Go Live", false },
						 { "CRT00000290", (NoResString)"WL CC Notifications Vehicle Booking System - Transport Users", false },
						 { "CRT00000291", (NoResString)"WL Trinium - Order Management - Go Live", false },
						 { "CRT00000292", (NoResString)"WL CC Survey Handheld Device", false },
						 { "CRT00000293", (NoResString)"WL Cargoguide", false },
						 { "CRT00000294", (NoResString)"WL CargoSphere eSUDS", false },
						 { "CRT00000295", (NoResString)"WL Accounting: System Config - Advanced (Additional Setup)", false },
						 { "CRT00000296", (NoResString)"Anti-Bribery and Corruption Policy (Internal)", false },
						 { "CRT00000297", (NoResString)"WL CC Container Depot Management System (CDMS)", false },
						 { "CRT00000298", (NoResString)"WL AU Customs - Advanced", false },
						 { "CRT00000299", (NoResString)"WL SMF Configuration", false },
						 { "CRT00000300", (NoResString)"WL AU Customs: Duty Drawbacks", true },
						 { "CRT00000301", (NoResString)"WL Accounting: General Ledger - Advanced", true },
						 { "CRT00000302", (NoResString)"WL Web Portals - Go Live", false },
						 { "CRT00000303", (NoResString)"WL AU Customs - Expert and Config", false },
						 { "CRT00000304", (NoResString)"WL CargoSphere - System Configuration", false },
						 { "CRT00000305", (NoResString)"WL CargoSphere - Quote/RFQ Manager", false },
						 { "CRT00000306", (NoResString)"WL FR Customs: Import Declarations - Go Live", false },
						 { "CRT00000308", (NoResString)"WL CargoSphere - Rate Sharing advanced", false },
						 { "CRT00000309", (NoResString)"WL CargoSphere - Quote/RFQ Manager Advanced", false },
						 { "CRT00000310", (NoResString)"WL CargoSphere - SUDS / Contract Management Advanced", false },
						 { "CRT00000311", (NoResString)"WL General Functions - Expert", false },
						 { "CRT00000312", (NoResString)"WL Web Portals - Advanced", false },
						 { "CRT00000313", (NoResString)"WL Customs: Denied Party Screening", false },
						 { "CRT00000314", (NoResString)"WL AU Customs: Air/Sea Cargo Reporting - Advanced", false },
						 { "CRT00000315", (NoResString)"WL ZA Customs Bonded Warehouse", false },
						 { "CRT00000316", (NoResString)"WL CargoSphere - My Account", false },
						 { "CRT00000317", (NoResString)"WL CargoSphere - Tariff Rate Management", false },
						 { "CRT00000318", (NoResString)"WL Workflow - Config", false },
						 { "CRT00000319", (NoResString)"WL Transit Warehouse - Go Live", false },
						 { "CRT00000320", (NoResString)"WL Projects - Go Live", false },
						 { "CRT00000321", (NoResString)"WL Projects - Advanced", false },
						 { "CRT00000322", (NoResString)"WL Workflow - Advanced/Expert", false },
						 { "CRT00000323", (NoResString)"WL Trinium - Dispatch - Go Live", false },
						 { "CRT00000324", (NoResString)"WL CargoSphere - Rate Management Console", false },
						 { "CRT00000325", (NoResString)"WL Generic Customs - Config", false },
						 { "CRT00000326", (NoResString)"ASSESS: CW1 Customs - German Messaging", false },
						 { "CRT00000327", (NoResString)"WL CargoSphere - Rate Sharing - Administration", false },
						 { "CRT00000328", (NoResString)"ASSESS: SQL DATEADD", false },
						 { "CRT00000329", (NoResString)"ASSESS: Customs Repository Structure", false },
						 { "CRT00000330", (NoResString)"WL TW Customs - Config", false },
						 { "CRT00000331", (NoResString)"WL TW Customs - Advanced", false },
						 { "CRT00000332", (NoResString)"WL CargoSphere - Level 3 Surcharges Administration", false },
						 { "CRT00000333", (NoResString)"ASSESS: Service Tasks", false },
						 { "CRT00000334", (NoResString)"WL CargoSphere - eSUDS - Administration", false },
						 { "CRT00000335", (NoResString)"WL CargoSphere - Rate Management Console", false },
						 { "CRT00000336", (NoResString)"WL CargoSphere - Rate Search - Advanced/Expert", false },
						 { "CRT00000337", (NoResString)"WL Trinium - Setup and Configuration", false },
						 { "CRT00000338", (NoResString)"WL Tariffs & Rates - Company Tariff - Go Live", false },
						 { "CRT00000339", (NoResString)"WL Tariffs & Rates - Quotations - Go Live", false },
						 { "CRT00000340", (NoResString)"WL Tariffs & Rates - Costings - Go Live", false },
						 { "CRT00000341", (NoResString)"WL Tariffs & Rates - Calculators - Go Live", false },
						 { "CRT00000342", (NoResString)"WL Tariffs & Rates - Generic Rating - Go Live", false },
						 { "CRT00000343", (NoResString)"WL Trinium - Charges and Accessorials - Go Live", false },
						 { "CRT00000344", (NoResString)"WL Transit Warehouse - Advance", false },
						 { "CRT00000345", (NoResString)"ASSESS: CW1 Customs - Cluster Keys", false },
					};

					return new CodeDescriptionBoolRegistryItem(
							"ExamMigrationStatus",
							RawDataRegistry.Categories.Recruiter,
							(NoResString)"Exam Migration Status",
							(NoResString)"Tick exams, which are online in WTA. They will no longer be accessible in MyAccount.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Is Migrated", null, true, true, true),
							defaults);
				});
			}
		}

		#endregion

		#region Talent Management System

		public StringRegistryItem HackerRankPersonalAccessToken
		{
			get
			{
				return GetItem("HackerRankPersonalAccessToken",
					() => new StringRegistryItem(
							"HackerRankPersonalAccessToken",
							Categories.Recruiter_TalentManagementSystem,
							ResString.GetMultilingualString("323ee6b3-351a-4ab5-99cb-19291f4cd364", "HackerRank Personal Access Token"),
							ResString.GetMultilingualString("54a76206-bcb9-4d8d-b6f9-f6a65c4c13fc", "Personal access token to use the HackerRank for Work API. Generated through HackerRank for Work account."),
							new StringRegistryDataType(),
							new TextRegistryEditorInfo(TextEditorType.Password),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							string.Empty));
			}
		}

		public StringRegistryItem TalogyClientId
		{
			get
			{
				return GetItem("TalogyClientId",
					() => new StringRegistryItem(
							"TalogyClientId",
							Categories.Recruiter_TalentManagementSystem,
							ResString.GetMultilingualString("DD0A3F6F-81BD-4FC2-9088-21C34E928EC0", "Talogy Client Id"),
							ResString.GetMultilingualString("8C4AEBEB-E3F8-4820-90CD-9F5720C259D1", "Client Id to use Talogy for Work API. Generated through Talogy for Work account."),
							new StringRegistryDataType(),
							new TextRegistryEditorInfo(TextEditorType.Password),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							string.Empty));
			}
		}

		public StringRegistryItem TalogyClientSecret
		{
			get
			{
				return GetItem("TalogyClientSecret",
					() => new StringRegistryItem(
							"TalogyClientSecret",
							Categories.Recruiter_TalentManagementSystem,
							ResString.GetMultilingualString("B9FEB837-19A8-43A5-809A-39D4EE8C1AC9", "Talogy Client Secret"),
							ResString.GetMultilingualString("5DCA8A55-D025-4D0E-892D-22E9849E48A6", "Client Secret to use Talogy for Work API. Generated through Talogy for Work account."),
							new StringRegistryDataType(),
							new TextRegistryEditorInfo(TextEditorType.Password),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							string.Empty));
			}
		}

		public StringRegistryItem TalogyGrantType
		{
			get
			{
				return GetItem("TalogyGrantType",
					() => new StringRegistryItem(
							"TalogyGrantType",
							Categories.Recruiter_TalentManagementSystem,
							ResString.GetMultilingualString("3AAE16A1-E7FB-46E6-A2A6-B4E6ED309E58", "Talogy Grant Type"),
							ResString.GetMultilingualString("2A02AD8F-1ABF-4D6D-8B79-AA4A70B7BBC0", "Client Grant Type to use Talogy for Work API. Generated through Talogy for Work account."),
							new StringRegistryDataType(),
							new TextRegistryEditorInfo(TextEditorType.TextBox),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							"client_credentials"));
			}
		}

		#endregion

		public GuidRegistryItem JobApplicationParsingQueueNotificationGroup
		{
			get
			{
				return GetItem("JobApplicationParsingQueueNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"JobApplicationParsingQueueNotificationGroup",
						RawDataRegistry.Categories.Recruiter,
						ResString.GetMultilingualString("8DAF7F3F-D178-4AC3-817B-BC666DEB2863", "Job Application Parsing Queue Notification Group"),
						ResString.GetMultilingualString("6E228507-17B0-4687-A323-C156B84D6C06", "The staff group that will be notified when the Parsing Queue Service Task fails to save an Application."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}
	}
}
