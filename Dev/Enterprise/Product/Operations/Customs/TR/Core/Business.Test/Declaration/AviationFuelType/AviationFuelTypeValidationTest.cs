using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class AviationFuelTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateReferenceNumber2()
		{
			CombineAssertions("Test validation of CSI_ReferenceNumber2", () =>
			{
				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.Validation.ValidateCSI_ReferenceNumber2();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = ZString.Empty;
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.Validation.ValidateCSI_ReferenceNumber2();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = ZString.Empty;
				aviationFuelType.CSI_Value = ZDecimal.Zero;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.Validation.ValidateCSI_ReferenceNumber2();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_ReferenceNumber2 = "12345";
				AssertHasMessageErrorContaining("Should 11 digit", aviationFuelType.CSI_ReferenceNumber2Info, "Tax ID must be 11 digit long");

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_ReferenceNumber2 = "123456789AB";
				AssertHasMessageErrorContaining("Digit only", aviationFuelType.CSI_ReferenceNumber2Info, "Tax ID must be 11 digit long");

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
				AssertNoMessageErrorContaining("Should not have 'mandatory' message error", aviationFuelType.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("Should not have 'Digit only' message error", aviationFuelType.CSI_ReferenceNumber2Info, "Tax ID must be 11 digit long");
			});
		}

		public void TestValidateDateOfIssue()
		{
			CombineAssertions("Test validation of CSI_DateOfIssue", () =>
			{
				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.Validation.ValidateCSI_DateOfIssue();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = ZString.Empty;
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.Validation.ValidateCSI_DateOfIssue();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				AssertNoMessageErrorContaining("Should not have message", aviationFuelType.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestValidateReferenceNumber()
		{
			CombineAssertions("Test validation of CSI_ReferenceNumber", () =>
			{
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_Value = ZDecimal.Zero;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				AssertNoMessageErrorContaining("Should not have message", aviationFuelType.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestValidateValue()
		{
			CombineAssertions("Test validation of CSI_Value", () =>
			{
				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.Validation.ValidateCSI_Value();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ValueInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = ZString.Empty;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.Validation.ValidateCSI_Value();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_ValueInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_Value = 1234567890123.12m;
				AssertNoMessageErrorContaining("Should not have message", aviationFuelType.CSI_ValueInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestValidateDescription()
		{
			CombineAssertions("Test validation of CSI_Description", () =>
			{
				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = ZString.Empty;
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining("Should have YouHaveNotEntered", aviationFuelType.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				aviationFuelType.CSI_ReferenceNumber = "12345678901234567890";
				aviationFuelType.CSI_Value = 1234567890123.12m;
				aviationFuelType.CSI_ReferenceNumber2 = "12345678901";
				aviationFuelType.CSI_DateOfIssue = new ZDateTime("01/01/2021");
				aviationFuelType.CSI_Description = "testtest";
				AssertNoMessageErrorContaining("Should not have message", aviationFuelType.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			aviationFuelType = invoiceLine.AviationFuelTypeCollection.AddNew();
		}
		AviationFuelType aviationFuelType;
	}
}
