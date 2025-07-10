namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration
{
	public class JobDeclarationValidation : EU.Business.Declaration.JobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;
	}
}
