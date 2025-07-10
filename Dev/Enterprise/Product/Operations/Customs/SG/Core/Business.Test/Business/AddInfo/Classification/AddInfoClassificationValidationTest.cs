namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoClassificationValidationTest : SGAddInfoValidationTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoClassification(Factory.New<Classification>().CC_AddInfoInfo);
		}
	}
}
