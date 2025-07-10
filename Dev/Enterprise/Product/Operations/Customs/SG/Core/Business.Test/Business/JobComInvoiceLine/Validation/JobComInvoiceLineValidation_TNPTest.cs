namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_TNPTest : JobComInvoiceLineValidationTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.TNP;
			}
		}
	}
}
