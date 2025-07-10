using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Recruiter.AutomatedRejection
{
	[EmailGenerator(typeof(IDocRecruitmentAutoRejectionEmailGenerator))]
	public class DocRecruitmentAutoRejectionEmail : DocRecruitmentRejectionEmail
	{
		DocRecruitmentAutoRejectionEmail(EmailGenerationLookup source, BusinessObjectFactory factory)
		: base(source, factory)
		{
		}

		public static DocRecruitmentAutoRejectionEmail New(EmailGenerationLookup objectToWrap, BusinessObjectFactory factory)
			=> new DocRecruitmentAutoRejectionEmail(objectToWrap, factory);

		public new EmailGenerationLookup WrappedObject => (EmailGenerationLookup)base.WrappedObject;
	}
}
