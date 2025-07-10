namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	sealed class EmailForTest : HRServiceEmail
	{
		public EmailForTest(byte[] eml)
			: base(eml)
		{ }
	}
}
