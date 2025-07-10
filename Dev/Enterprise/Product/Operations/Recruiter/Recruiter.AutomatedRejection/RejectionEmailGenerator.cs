using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.AutomatedRejection
{
	public sealed class RejectionEmailGenerator : IDocRecruitmentAutoRejectionEmailGenerator
	{
		const string EmailSignatureImageCID = "emailSignatureImageCID";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Null strings")]
		public const string NullString = @"<null>";

		readonly DocRecruitmentAutoRejectionEmailParser parser;

		public RejectionEmailGenerator()
			: this(new DocRecruitmentAutoRejectionEmailParser())
		{
		}

		public RejectionEmailGenerator(DocRecruitmentAutoRejectionEmailParser parser)
		{
			this.parser = parser;
		}

		Type IEmailGenerator.DocSourceType => typeof(DocRecruitmentAutoRejectionEmail);

		EmailDef IEmailGenerator.BuildEmail(string recipientEmailAddress, NotificationEmailTemplate template, BusinessObject wrapper)
			=> BuildEmail(recipientEmailAddress, template, (EmailGenerationLookup)wrapper);

		public BusinessObject CreatePreviewSample()
		{
			var factory = new BusinessObjectFactory();
			factory.Saving += (f) => throw new InvalidOperationException("Sample data should not be saved");

			var applicant = factory.New<HRJobApplicant>();
			applicant.HA_NameSuffix = "Mr.";
			applicant.HA_FullName = (NoResString)"John Smith";
			applicant.HA_EmailAddress = "john.smith@example.com";

			var application = applicant.Applications.AddNew();

			return CreateLookup(application);
		}

		public EmailGenerationLookup CreateLookup(HRJobApplication application)
		{
			var linkedInURL = RecruitmentDataRegistry.Instance.AutomatedRejection_LinkedInURL.Value;
			if (string.IsNullOrEmpty(linkedInURL))
			{
				linkedInURL = RecruitmentDataRegistry.Instance.AutomatedRejection_LinkedInURL.DefaultValue;
			}

			return new EmailGenerationLookup
			{
				CompanyName = GlbCompany.CurrentCompany.CompanyName,
				FirstName = FirstName(application),
				LinkedInURL = new Uri(linkedInURL),
				CompanySignatureLogoHtml = string.Empty,
			};
		}

		public EmailDef BuildEmail(string recipientEmailAddress, NotificationEmailTemplate template, EmailGenerationLookup lookup)
		{
			var email = new EmailDef();
			email.ContentType = EmailContentTypes.HTML;
			email.AddRecipientForUserCommunication(recipientEmailAddress);
			email.FromAddress = $"{GlbCompany.CurrentCompany.CompanyName + " " + Res.GetString("0a69b9a2-69bf-4cdf-a259-a53cd2bf0288", "Recruitment")}<{RecruitmentDataRegistry.Instance.SenderAddress.Value}>";

			var img = RecruitmentDataRegistry.Instance.AutomatedRejection_EmailSignatureImage.Value;
			if (AddImageToEmail(email, img))
			{
				lookup.CompanySignatureLogoHtml = FormattableString.Invariant($@"<img src=""cid:{EmailSignatureImageCID}"" width=""{img.Width}"" height=""{img.Height}"" />");
			}

			email.Subject = parser.Parse(lookup, template.EmailSubject);
			email.Body = parser.Parse(lookup, template.EmailBody);

			return email;
		}

		public static bool AddImageToEmail(EmailDef email, Image img)
		{
			if (email == null || img == null)
			{
				return false;
			}

			using (var imageStream = new MemoryStream())
			{
				img.Save(imageStream, ImageFormat.Jpeg);
				_ = email.Attachments.Add(new AttachmentDef(EmailSignatureImageCID, imageStream.ToArray()));
			}

			return true;
		}

		public static string FirstName(HRJobApplication application)
		{
			try
			{
				return application?.Applicant?.Name?.Split(' ').First() ?? NullString;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return application.Applicant.Name;
			}
		}
	}
}
