using System;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class EmailToApplicantBusinessObject : HtmlFormatEmailToContactBusinessObject
	{
		public EmailToApplicantBusinessObject(HRJobApplication application)
			: this(application.Applicant)
		{
			parseFunc = template => new DocumentParser<HRJobApplication, DocHRJobApplication>(Factory).Parse(application, template);
			this.application = application;
		}

		public EmailToApplicantBusinessObject(HRJobApplicant applicant)
			: base(applicant, applicant.FromAddress, applicant.FromDisplayName)
		{
			parseFunc = template => new DocumentParser<HRJobApplicant, DocHRJobApplicant>(Factory).Parse(applicant, template);
			SetDefaultTo();
		}

		void SetDefaultTo()
		{
			ToDisplayName = BusinessObjectSendingEmail.HA_FullName;
			ToEmailAddress = BusinessObjectSendingEmail.HA_EmailAddress;
		}

		protected override EmailDef GetEmailCore()
		{
			EmailDef result = base.GetEmailCore();
			result.Subject = parseFunc(Subject);
			result.Body = parseFunc(result.Body);
			return result;
		}

		protected override void SetupDefaultFromAddressCore()
		{
			UseCurrentUsersNameAndTitle = false;
			UseCurrentUsersEmailAddress = false;
		}

		protected override void LoadHtmlUsingTemplate(HtmlEmailDef emailDef)
		{
			string preformattedBody = (NoResString)"<pre style='font-family:verdana,arial;font-size:12px'><br/><br/>" + Body + (NoResString)"</pre>"; // preformatted html
			emailDef.LoadHtmlUsingTemplate(preformattedBody);
		}

		protected override string GetEmailBodyForNote(EmailDef email)
		{
			return email.Body;
		}

		public new HRJobApplicant BusinessObjectSendingEmail
		{
			get { return (HRJobApplicant)base.BusinessObjectSendingEmail; }
		}

		readonly Func<string, string> parseFunc;
		public readonly HRJobApplication application;
	}
}
