namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is base validation class for common JobDeclaration and EMCSJobDeclaration
	/// </summary>
	public class JobDeclarationValidation : AutoJobDeclarationValidation
	{
		public JobDeclarationValidation(AutoJobDeclaration parent)
			: base(parent)
		{
		}
	}
}
