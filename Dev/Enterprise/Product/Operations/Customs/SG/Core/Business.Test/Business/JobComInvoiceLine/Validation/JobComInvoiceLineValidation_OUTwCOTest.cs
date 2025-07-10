using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_OUTwCOTest : JobComInvoiceLineValidation_OUTTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		public void TestCertItemDescription()
		{
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			Validation.ValidateCertItemDescription();
			AssertEquals(true, InvoiceLine.CertItemDescriptionInfo.HasMessageErrors());
			InvoiceLine.CertItemDescription = "TEST";
			Validation.ValidateCertItemDescription();
			AssertEquals(false, InvoiceLine.CertItemDescriptionInfo.HasMessageErrors());
		}
	}
}
