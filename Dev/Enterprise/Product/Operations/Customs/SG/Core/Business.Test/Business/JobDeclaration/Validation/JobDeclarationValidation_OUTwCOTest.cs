namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobDeclarationValidation_OUTwCOTest : JobDeclarationValidation_OUTTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}
	}
}
