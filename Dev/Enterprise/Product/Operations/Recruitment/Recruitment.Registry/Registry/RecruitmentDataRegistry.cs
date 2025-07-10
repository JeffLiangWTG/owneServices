using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruitment.Registry
{
	public sealed class RecruitmentDataRegistry : RegistryItemSet
	{
		public static RecruitmentDataRegistry Instance => fInstance = fInstance ?? new RecruitmentDataRegistry();
		[ThreadStatic]
		static RecruitmentDataRegistry fInstance;

		public override bool IsForProductivityWise => false;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Candidate_Management => CombineCategories(Recruiter, ResString.GetMultilingualString("91BAA8E2-2F36-45C7-BA88-F8483730658E", "Candidate Management"));
			public static MultilingualString Candidate_Management_AutomatedRejection => CombineCategories(Candidate_Management, ResString.GetMultilingualString("24bbf993-e30b-479a-98f3-efcb649d4553", "Automated Rejection Emails"));
			public static MultilingualString Candidate_Management_Mail => CombineCategories(Candidate_Management, ResString.GetMultilingualString("D9A62D27-5906-486A-82D6-B10AE6516D2F", "Mail"));
			public static MultilingualString Candidate_Management_Mail_Outgoing => CombineCategories(Candidate_Management_Mail, ResString.GetMultilingualString("fc720268-86f3-451a-8028-69e01747c212", "Outgoing"));
			public static MultilingualString Candidate_Management_Mail_Incoming => CombineCategories(Candidate_Management_Mail, ResString.GetMultilingualString("4b33fd3f-cb95-4d7c-b706-b925088bbe12", "Incoming"));
			public static MultilingualString Candidate_Management_Mail_Incoming_IMAP => CombineCategories(Candidate_Management_Mail_Incoming, ResString.GetMultilingualString("05ccc9f5-bfb2-4749-8f5b-a7c1576ca79c", "IMAP"));
			public static MultilingualString Candidate_Management_Mail_Incoming_POP3 => CombineCategories(Candidate_Management_Mail_Incoming, ResString.GetMultilingualString("0e66e457-cef8-494d-b178-ddbceec8a182", "POP3"));
			public static MultilingualString Candidate_Management_Mail_MiddleMan => CombineCategories(Candidate_Management_Mail, ResString.GetMultilingualString("0FBC4D72-1809-4D3C-84E7-502D486585B0", "Middle Man Outgoing Mail Settings"));
		}

		public BooleanRegistryItem RecruitmentModuleEnabled => GetItem("RecruitmentModuleEnabled", delegate
		{
			return new BooleanRegistryItem(
				"RecruitmentModuleEnabled",
				Categories.Candidate_Management,
				ResString.GetMultilingualString("5C7BF906-3201-4220-8175-DFA6A624090C", "Enable Recruitment Module"),
				ResString.GetMultilingualString("3cd34cbe-71fe-45f7-a804-46a861aeff59", "Enables access to the Recruitment module, at Maintain -> [H] Human Resources"),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				false);
		});

		public WorkItemTemplatePropertiesRegistryItem WorkItemTemplateProperties => GetItem("WorkItemTemplateProperties", delegate
		{
			MultilingualString caption = ResString.GetMultilingualString("cce2282b-4e8c-451f-b4c5-d4b6b8a029de", "Work Item Template Properties");
			MultilingualString hint = ResString.GetMultilingualString("d6c4b120-1f1f-4f3c-908e-da0c423e8b06", "The list of work item templates that will be available to pick from on the 'New Work Item' drop down");

			return new WorkItemTemplatePropertiesRegistryItem(
				"WorkItemTemplateProperties",
				Categories.Candidate_Management,
				caption,
				hint,
				RegistryStorageFlags.System,
				new WorkItemTemplatePropertiesCollection());
		});

		public BooleanRegistryItem CreatePersonAPIEnabled
		{
			get
			{
				return GetItem("CreatePersonAPIEnabled", () => new BooleanRegistryItem(
					"CreatePersonAPIEnabled",
					Categories.Candidate_Management,
					ResString.GetMultilingualString("1e3777c0-1b6a-45ae-9ebe-a62840e9afaa", "Enable Account Creation for Careers Portal"),
					ResString.GetMultilingualString("60e3705f-a29c-4f9d-a3c5-ea33adbf3ad4", "Enable account creation to allow accounts to be created through the Careers Portal."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					false));
			}
		}

		public StringRegistryItem ConvertApiSecretKey => GetItem("ConvertApiSecretKey", delegate
		{
			return new StringRegistryItem(
				"ConvertApiSecretKey",
				Categories.Candidate_Management,
				ResString.GetMultilingualString("0a3a6ea6-f4a9-40a6-b2d3-b6500aa651e1", "ConvertApi Secret API Key"),
				ResString.GetMultilingualString("0d750275-b4be-46b6-a5c4-27224ce56281", "ConvertApi Secret API Key can be obtained at https://www.convertapi.com/a/signup"),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController);
		});

		public BooleanRegistryItem RunConvertApplicationStatusToCandidateRating => GetItem("RunConvertApplicationStatusToCandidateRating", delegate
		{
			return new BooleanRegistryItem(
				"RunConvertApplicationStatusToCandidateRating",
				Categories.Candidate_Management,
				ResString.GetMultilingualString("0a3a6ea6-f4a9-40a6-b2d3-b6500aa651e3", "Run the online transform to convert application status to candidate rating"),
				ResString.GetMultilingualString("0d750275-b4be-46b6-a5c4-27224ce56283", "If enabled, the transform will run on CW1 startup and then toggle this registry setting"),
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				true);
		});

		public DateTimeRegistryItem ResumeBacklogConversionHighWaterMark => GetItem("ResumeBacklogConversionHighWaterMark", delegate
		{
			return new DateTimeRegistryItem(
				"ResumeBacklogConversionHighWaterMark",
				Categories.Candidate_Management,
				ResString.GetMultilingualString("0a3a6ea6-f4a9-40a6-b2d3-b6500aa651e2", "Resume backlog conversion high watermark"),
				ResString.GetMultilingualString("0d750275-b4be-46b6-a5c4-27224ce56282", "How far back in time to parse applications that might have CVs/resumes attached but that haven't been converted to PDF"),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				ZDateTime.Now.AddDays(30).ToDateTime());
		});

		public StringRegistryItem ReceivedMailDocType =>
			GetItem("RecruitmentReceivedMailDocType", () =>
				new StringRegistryItem(
					"RecruitmentReceivedMailDocType",
					Categories.Candidate_Management_Mail,
					ResString.GetMultilingualString("083a0180-18c1-4daf-9aab-d00dfe0c16c1", "Mail Document Type"),
					ResString.GetMultilingualString("6756f56d-ba32-4805-ae4d-f53961ac0267", "The document type to attach any received emails as"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, factory => new RefDocTypeCollection(factory))
				});

		public StringRegistryItem SenderAddress =>
			GetItem("RecruitmentOutgoingSenderAddress", () =>
				new StringRegistryItem(
					"RecruitmentOutgoingSenderAddress",
					Categories.Candidate_Management_Mail_Outgoing,
					ResString.GetMultilingualString("bf2f6853-0c5e-4c33-8c1d-fb31d3be413f", "Sender Address"),
					ResString.GetMultilingualString("72f0b197-6768-4bee-ba4e-f8a49b4e8ce8", "The email address to use as the sender for outgoing messages"),
					new EmailStringRegistryDataType(),
					RegistryStorageFlags.System));

		#region MiddleMan Forwarding
		public StringRegistryItem MiddleMan_ForwardingAddress =>
			GetItem("MiddleMan_ForwardingAddress", () =>
				new StringRegistryItem(
					"MiddleMan_ForwardingAddress",
					Categories.Candidate_Management_Mail_MiddleMan,
					ResString.GetMultilingualString("c08fe411-b9b4-4657-b298-99373b0a68b4", "Middle Man Forwarding Address"),
					ResString.GetMultilingualString("b547cdad-01ff-4b27-989f-66da4fe9de9d", "The address of the alias and mailbox that is used as a middle man to handle email exchange between recruiter and candidate"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public StringRegistryItem MiddleMan_SMTPServerAddress
			=> GetItem("MiddleMan_SMTPServerAddress",
				() => new StringRegistryItem(
					"MiddleMan_SMTPServerAddress",
					Categories.Candidate_Management_Mail_MiddleMan,
					ResString.GetMultilingualString("a96f5298-ba33-423c-bd76-cd0d34090e75", "Mail Server"),
					ResString.GetMultilingualString("6628cb24-f179-476b-9a3d-6c1b3c4322f5", "The address for the SMTP server"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public IntRegistryItem MiddleMan_SMTPServerPort
			=> GetItem("MiddleMan_SMTPServerPort",
				() => new IntRegistryItem(
					"MiddleMan_SMTPServerPort",
					Categories.Candidate_Management_Mail_MiddleMan,
					ResString.GetMultilingualString("b5d758b6-639f-4260-bd78-a49e30f9fd00", "Mail Server Port"),
					ResString.GetMultilingualString("347e2369-4c32-4f67-8ef7-bed855de8b6f", "The port for the SMTP server"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					110));

		public StringRegistryItem MiddleMan_SMTPServerUsername
			=> GetItem("MiddleMan_SMTPServerUsername",
				() => new StringRegistryItem(
					"MiddleMan_SMTPServerUsername",
					Categories.Candidate_Management_Mail_MiddleMan,
					ResString.GetMultilingualString("0fa49207-22d1-4b49-af12-a9aa32567f52", "Mailbox User Name"),
					ResString.GetMultilingualString("4e4f9304-75b1-4acf-94b7-73f66871fd8f", "The user name for the SMTP server, e.g. user@example.com"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public StringRegistryItem MiddleMan_SMTPServerPassword
			=> GetItem("MiddleMan_SMTPServerPassword",
				() => new StringRegistryItem(
					"MiddleMan_SMTPServerPassword",
					Categories.Candidate_Management_Mail_MiddleMan,
					ResString.GetMultilingualString("94364038-492d-4ca6-8196-817dcdc0bd0f", "Mailbox Password"),
					ResString.GetMultilingualString("1f628826-b33b-4a91-9701-22d0f19552cc", "The password for the SMTP server"),
					new StringRegistryDataType(CharacterCase.Normal, 1, 16),
					new TextRegistryEditorInfo(TextEditorType.Password),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public StronglyTypedRegistryItem<string> MiddleMan_SMTPSecureConnection
			=> GetItem("MiddleMan_SMTPSecureConnection", () =>
				{
					var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

					return new CodePairWithAdditionalEventRegistryItem(
						"MiddleMan_SMTPSecureConnection",
						Categories.Candidate_Management_Mail_MiddleMan,
						ResString.GetMultilingualString("27fbd0e8-1af6-4af9-9f44-054314cc8c61", "SMTP Server Secure Connection"),
						ResString.GetMultilingualString("e1dfaf6d-c087-45e5-a85c-ac36aa554ab8", "The type of secure connection to use to connect to the SMTP server"),
						secureConnectionTypesListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
						(registryItem) => Utilities.IsConnectionAndPortChecked(registryItem, MiddleMan_SMTPServerPort),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						SecureConnectionTypes.None,
						false);
				});

		#endregion

		#region Hidden GUI Elements
		public BooleanRegistryItem IsExamsModuleEnabled
			=> CreateRecruiterBooleanRegistryItem(
				nameof(IsExamsModuleEnabled),
				ResString.GetMultilingualString("c0b1a3b4-65b4-4838-8938-11ef096b9e06", "Toggle whether the Exams related functionality is available."),
				ResString.GetMultilingualString("78c02d62-51e9-43cf-add9-343851eb6eb6", "The exams functionality is being phased out, due to the move to WTA. As such, related items are being hidden in the GUI. If you need access, override the default to true."));

		public BooleanRegistryItem IsAccreditationModuleEnabled
			=> CreateRecruiterBooleanRegistryItem(
				nameof(IsAccreditationModuleEnabled),
				ResString.GetMultilingualString("46d25479-1a9c-4674-ad65-2d3a892d6391", "Toggle whether the Accreditation related functionality is available."),
				ResString.GetMultilingualString("980cee9b-8415-4a24-83a9-f082724685ee", "The accreditation functionality is being phased out, due to the move to WTA. As such, related items are being hidden in the GUI. If you need access, override the default to true."));

		public BooleanRegistryItem IsLearningCentreModuleEnabled
			=> CreateRecruiterBooleanRegistryItem(
				nameof(IsLearningCentreModuleEnabled),
				ResString.GetMultilingualString("9590281b-4e28-4297-a783-593e8a5a10b3", "Toggle whether the Learning Center related functionality is available."),
				ResString.GetMultilingualString("bd9f4c85-5a84-482d-bb4f-8d72f16ddcb2", "The learning center functionality is being phased out, due to the move to WTA. As such, related items are being hidden in the GUI. If you need access, override the default to true."));

		BooleanRegistryItem CreateRecruiterBooleanRegistryItem(
			string propertyName,
			MultilingualString captionString,
			MultilingualString hintString) =>
				GetItem(propertyName,
					delegate
					{
						return new BooleanRegistryItem(
							propertyName,
							RawDataRegistry.Categories.Recruiter,
							captionString,
							hintString,
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForController,
							false);
					});
		#endregion

		public NotificationEmailTemplateRegistryItem AutomatedRejection_EmailTemplate =>
			GetItem("AutomatedRejection_EmailTemplate", () =>
				new NotificationEmailTemplateRegistryItem(
					"AutomatedRejection_EmailTemplate",
					Categories.Candidate_Management_AutomatedRejection,
					ResString.GetMultilingualString("85ac30d8-4963-415b-b248-3c1d56809067", "Email Template"),
					ResString.GetMultilingualString("3b5f7e82-b130-4fca-bd4a-bbf58292b1ad", "This text will be copied into an email that will be sent to a candidate informing them of application rejection."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					ObjectFactory.GetType("IDocRecruitmentAutoRejectionEmail"),
					DefaultSubject,
					DefaultEmailBody));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "default value shouldn't be translated")]
		public const string DefaultSubject = "Your application with (*CompanyName*)";
		public const string DefaultEmailBody = @"
<span style=""font-family: Arial; font-size: small; color: black;"">
	<p>Hi (*FirstName*),</p>
	<p>We have decided, reluctantly, not to proceed with your application at this time. While your application was strong, we have had to substantially narrow our list of candidates.</p>
	<p>Here at (*CompanyName*), our people are who we are. That&rsquo;s why we have a rigorous recruitment process. We want to ensure the people we hire are very likely to thrive with us.</p>
	<p>While you weren&rsquo;t successful this time, this does not mean we don&rsquo;t want you to apply again. We actually would love to hear from you in the future. Your qualifications and experience will change, and our needs will also evolve.</p>
	<p>If you haven&rsquo;t already, feel free to connect with us on social media. LinkedIn is a good place to start: <a href=""(*LinkedInURL*)"">(*LinkedInURL*)</a>.</p>
	<p>I would like to say thankyou for your interest in (*CompanyName*). All of us here wish you the very best, and wish you the best of luck with your job search.</p>
	<p>Kind regards,</p>
</span>
<table>
	<tbody>
		<tr>
			<td>
				<p style=""font-family: Arial; font-size: small; color: black;"">The Talent Team<br />(*CompanyName*)</p>
			</td>
			<td>
				(*CompanySignatureLogoHtml*)
			</td>
		</tr>
	</tbody>
</table>";

		public ImageRegistryItem AutomatedRejection_EmailSignatureImage =>
			GetItem("AutomatedRejection_EmailSignatureImage", () =>
				new ImageRegistryItem(
					"AutomatedRejection_EmailSignatureImage",
					Categories.Candidate_Management_AutomatedRejection,
					ResString.GetMultilingualString("fa17ab61-1f7c-40f9-afef-c770309649d9", "Email Signature Image"),
					ResString.GetMultilingualString("9f713232-7345-4755-8b4b-6c041902a1f5", "Company logo image used in the signature of the automated rejection emails."),
					RegistryStorageFlags.System,
					RegistryOptions.Default));

		public StringRegistryItem AutomatedRejection_LinkedInURL =>
			GetItem("AutomatedRejection_LinkedInURL", () =>
				new StringRegistryItem(
					"AutomatedRejection_LinkedInURL",
					Categories.Candidate_Management_AutomatedRejection,
					ResString.GetMultilingualString("deb56dbc-541e-4336-bad4-2ce035c0512e", "LinkedIn URL"),
					ResString.GetMultilingualString("4c5b2847-daaf-4544-8150-4ac722558ebd", "The URL to insert into the rejection email that directs to the WTG LinkedIn page."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					DefaultLinkedInURL));

		public const string DefaultLinkedInURL = @"https://www.linkedin.com/company/wisetech-global/";
		public const int DefaultRejectionEmailSendDelay = 3;

		public IntRegistryItem AutomatedRejection_DaysToDelaySendingEmail =>
			GetItem("AutomatedRejection_DaysToDelaySendingEmail", () =>
				new IntRegistryItem(
					"AutomatedRejection_DaysToDelaySendingEmail",
					Categories.Candidate_Management_AutomatedRejection,
					ResString.GetMultilingualString("548d7aca-a855-4573-802d-b06c2d0abafa", "Rejection Email Send Delay"),
					ResString.GetMultilingualString("a6cc416b-f996-4205-a289-e4c81e3a71a0", "The number of days to wait from rejecting a candidate to sending the rejection email. 0 days will be an immediate send."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					DefaultRejectionEmailSendDelay));
	}
}

