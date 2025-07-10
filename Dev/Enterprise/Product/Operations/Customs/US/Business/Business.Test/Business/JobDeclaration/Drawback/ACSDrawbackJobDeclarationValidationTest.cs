namespace Enterprise.Customs.US.Business.Testing
{
	public class ACSDrawbackJobDeclarationValidationTest : CommonDrawbackJobDeclarationValidationTest
	{
		public void TestCheckJE_GoodsDescription()
		{
			declaration.JE_GoodsDescription = "ABCDEF";
			AssertNoMessageErrorContaining(declaration.JE_GoodsDescriptionInfo, ACSDrawbackJobDeclarationValidation.GoodsDescriptionMessage);
			declaration.JE_GoodsDescription = "ABCDE";
			AssertHasMessageErrorContaining(declaration.JE_GoodsDescriptionInfo, ACSDrawbackJobDeclarationValidation.GoodsDescriptionMessage);
			declaration.JE_GoodsDescription = "";
			AssertNoMessageErrorContaining(declaration.JE_GoodsDescriptionInfo, ACSDrawbackJobDeclarationValidation.GoodsDescriptionMessage);
		}
	}
}
