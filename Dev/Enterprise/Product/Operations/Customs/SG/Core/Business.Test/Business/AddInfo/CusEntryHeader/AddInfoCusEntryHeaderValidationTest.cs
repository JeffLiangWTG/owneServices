namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoCusEntryHeaderValidationTest : SGAddInfoValidationTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoCusEntryHeader(Factory.New<CusEntryHeader>().CH_AddInfoInfo);
		}
	}
}
