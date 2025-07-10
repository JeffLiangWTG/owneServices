namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoCusEntryLineValidationTest : SGAddInfoValidationTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoCusEntryLine(Factory.New<CusEntryLine>().CL_AddInfoInfo);
		}
	}
}
