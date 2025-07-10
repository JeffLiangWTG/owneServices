namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineValidation_OUTTest : AddInfoJobComInvoiceLineValidation_OutwardTest
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
