namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentDocumentSupporterForTest : JobRequiredDocumentDocumentSupporter
	{
		public JobRequiredDocumentDocumentSupporterForTest(IHaveRequiredDocuments parent) : base(parent)
		{
		}

		public JobRequiredDocumentDocumentSupporterForTest(JobRequiredDocument jobRequiredDocument) : base(jobRequiredDocument)
		{
		}

		public OrgHeader OrgHeaderForTest => OrgHeader;
		public JobRequiredDocument RequiredDocumentForTest => RequiredDocument;
	}
}
