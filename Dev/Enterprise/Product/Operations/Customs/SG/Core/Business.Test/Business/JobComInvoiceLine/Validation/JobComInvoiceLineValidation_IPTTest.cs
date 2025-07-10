namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_IPTTest : JobComInvoiceLineValidation_InwardTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}
	}
}
