namespace Enterprise.MasterFiles.Business
{
	public interface IGlbStaffEmailValidationSender
	{
		void GenerateValidationEmail(GlbStaff staff, string email);
	}
}
