namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderValidation_OUTTest : JobComInvoiceHeaderValidationTest
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
