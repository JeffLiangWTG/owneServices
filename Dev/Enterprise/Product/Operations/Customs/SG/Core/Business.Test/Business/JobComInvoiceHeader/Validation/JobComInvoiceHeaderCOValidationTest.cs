using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderCOValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.COO;
			}
		}

		public void TestInvoiceNumber()
		{
			Declaration.SG_Cert1Type = "1";
			Validation.ValidateJZ_InvoiceNumber();
			AssertEquals(true, InvoiceHeader.JZ_InvoiceNumberInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
			Declaration.SG_Cert1Type = "4";
			Validation.ValidateJZ_InvoiceNumber();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceNumberInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
			InvoiceHeader.JZ_InvoiceNumber = "123";
			Validation.ValidateJZ_InvoiceNumber();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceNumberInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
		}

		public void TestInvoiceDate()
		{
			Declaration.SG_Cert1Type = "1";
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			Validation.ValidateJZ_InvoiceDate();
			AssertEquals(true, InvoiceHeader.JZ_InvoiceDateInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
			Declaration.SG_Cert1Type = "4";
			Validation.ValidateJZ_InvoiceDate();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceDateInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			Validation.ValidateJZ_InvoiceDate();
			AssertEquals(false, InvoiceHeader.JZ_InvoiceDateInfo.HasWarning("You may optionally specify Invoice details for this certificate type."));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SGCertificateTypeHelper.CreateCertificateTypes(Factory);
			Factory.Save();
		}
	}
}
