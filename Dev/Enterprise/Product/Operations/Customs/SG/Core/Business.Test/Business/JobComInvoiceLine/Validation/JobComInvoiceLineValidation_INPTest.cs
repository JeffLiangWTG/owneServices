namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_INPTest : JobComInvoiceLineValidation_InwardTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.INP;
			}
		}
	}
}
