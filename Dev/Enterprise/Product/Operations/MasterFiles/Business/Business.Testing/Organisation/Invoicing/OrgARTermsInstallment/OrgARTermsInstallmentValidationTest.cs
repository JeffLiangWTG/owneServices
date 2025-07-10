using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARTermsInstallmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckML_SequenceNumber()
		{
			AssertEquals("Existing installment should have a sequence #1", (ZByte)1, arTermsInstallment.ML_SequenceNumber);
			AssertNoErrors(arTermsInstallment.ML_SequenceNumberInfo);

			var installment2 = arTerms.ARTermsInstallments.AddNew();
			AssertEquals("New installment should have a sequence #2", (ZByte)2, installment2.ML_SequenceNumber);

			var installment3 = arTerms.ARTermsInstallments.AddNew();
			installment3.ML_SequenceNumber = 1;
			AssertHasError(installment3.ML_SequenceNumberInfo, "Sequence number already exists.");

			installment3.ML_SequenceNumber = 3;
			AssertNoErrors(installment3.ML_SequenceNumberInfo);
		}

		public void TestCheckML_DaysFromInvoiceDate()
		{
			AssertNoErrors(arTermsInstallment.ML_DaysFromInvoiceDateInfo);

			arTermsInstallment.ML_DaysFromInvoiceDate = 0;
			AssertNoErrors(arTermsInstallment.ML_DaysFromInvoiceDateInfo);

			arTermsInstallment.ML_DaysFromInvoiceDate = 10;
			AssertNoErrors(arTermsInstallment.ML_DaysFromInvoiceDateInfo);
		}

		public void TestCheckML_SplitPercentage()
		{
			var expectedError = "Percentage must be greater than zero.";
			AssertNoErrors(arTermsInstallment.ML_SplitPercentageInfo);
			arTermsInstallment.ML_SplitPercentage = 0;
			AssertHasError(arTermsInstallment.ML_SplitPercentageInfo, expectedError);
			arTermsInstallment.ML_SplitPercentage = -1;
			AssertHasError(arTermsInstallment.ML_SplitPercentageInfo, expectedError);

			expectedError = "Please check the percentage, to continue saving the sum of percentages present in rows must be equal to 100%.";
			arTermsInstallment.ML_SplitPercentage = 33.33;
			AssertHasRowError(arTermsInstallment, expectedError);

			var installment2 = arTerms.ARTermsInstallments.AddNew();
			installment2.ML_SplitPercentage = 33.33;
			AssertHasRowError(arTermsInstallment, expectedError);

			var installment3 = arTerms.ARTermsInstallments.AddNew();
			installment3.ML_SplitPercentage = 33.34;
			AssertNoRowError(arTermsInstallment, expectedError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			arTerms = Factory.New<OrgARTerms>();
			arTerms.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			arTermsInstallment = arTerms.ARTermsInstallments.AddNew();
		}

		OrgARTerms arTerms;
		OrgARTermsInstallment arTermsInstallment;
	}
}
