using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportDEAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_CountryOfShipment()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Header.US_CountryOfShipmentInfo, "~~", "US");
		}

		public void TestCheckUS_PermitNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.US_PermitNumberInfo);

			Header.US_PermitNumber = "123ABC";
			AssertHasMessageError(Header.US_PermitNumberInfo, USImportDEAHeaderAddInfoValidation.PermitNumberFormat);

			Header.US_PermitNumber = "123-ABC";
			AssertHasMessageError(Header.US_PermitNumberInfo, USImportDEAHeaderAddInfoValidation.PermitNumberFormat);

			Header.US_PermitNumber = "123-ABCD";
			AssertNoMessageError(Header.US_PermitNumberInfo, USImportDEAHeaderAddInfoValidation.PermitNumberFormat);
		}

		public void TestCheckUS_RegistrationNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.US_RegistrationNumberInfo);

			Header.US_RegistrationNumber = "1234ABCD";
			AssertHasMessageError(Header.US_RegistrationNumberInfo, USImportDEAHeaderAddInfoValidation.RegistrationNumberFormat);

			Header.US_RegistrationNumber = "1234ABCD-";
			AssertHasMessageError(Header.US_RegistrationNumberInfo, USImportDEAHeaderAddInfoValidation.RegistrationNumberFormat);

			Header.US_RegistrationNumber = "1234ABCDE";
			AssertNoMessageError(Header.US_RegistrationNumberInfo, USImportDEAHeaderAddInfoValidation.RegistrationNumberFormat);
		}

		public void TestCheckUS_FormID()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Header.US_FormIDInfo, "~~", "DEA-236");
		}

		DEAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.DEAHeaders.AddNew();
				}
				return header;
			}
		}
		DEAHeader header;
	}
}
