namespace Enterprise.Recruiter.Business.Testing
{
	sealed class DocHRJobApplicantTest : DocRecruiterTestCase<DocHRJobApplicant>
	{
		protected override DocHRJobApplicant GetDocumentWrapper()
		{
			return DocHRJobApplicant.New(Applicant, Applicant.Factory);
		}
	}
}
