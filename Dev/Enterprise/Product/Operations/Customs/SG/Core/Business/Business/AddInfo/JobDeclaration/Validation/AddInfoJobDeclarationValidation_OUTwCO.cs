namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation_OUTwCO : AddInfoJobDeclarationValidation_OUT
	{
		public AddInfoJobDeclarationValidation_OUTwCO(AddInfoJobDeclaration parent)
			: base(parent)
		{
			this.Add(new AddInfoCOValidation(parent));
		}
	}
}
