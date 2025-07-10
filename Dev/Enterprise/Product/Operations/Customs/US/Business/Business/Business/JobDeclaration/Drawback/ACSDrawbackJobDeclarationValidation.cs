
namespace Enterprise.Customs.US.Business
{
	public class ACSDrawbackJobDeclarationValidation : CommonDrawbackJobDeclarationValidation
	{
		public ACSDrawbackJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_GoodsDescription()
		{
			base.CheckJE_GoodsDescription();
			if (!Parent.JE_GoodsDescription.IsEmpty && Parent.JE_GoodsDescription.Length < 6)
			{
				Parent.JE_GoodsDescriptionInfo.AddMessageError(GoodsDescriptionMessage);
			}
		}
		internal const string GoodsDescriptionMessage = "Description must be blank or more than 5 characters.";
	}
}
