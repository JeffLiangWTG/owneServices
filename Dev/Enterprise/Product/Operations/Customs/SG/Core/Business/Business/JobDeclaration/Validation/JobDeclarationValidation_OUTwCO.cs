

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation_OUTwCO : JobDeclarationValidation_OUT
	{
		public JobDeclarationValidation_OUTwCO(JobDeclaration parent)
			: base(parent)
		{
			cOValidation = new COValidation(parent);
		}

		protected COValidation cOValidation;
	}
}
