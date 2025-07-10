using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}

		public void TestEntryLineNo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22030010";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
			for (int i = 1; i < 50; i++)
			{
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "22030010";
			}

			AssertEquals("just checking", (ZShort)50, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = declaration.ActiveEntryHeaders[0];
			AssertEquals("Merged lines on merging", 50, entryHeader.MergedLines.Count);
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22030010";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals("Another invoice line added", (ZShort)51, invoiceLine.JI_LineNo);
			AssertHasMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("Merged lines on merging", 51, entryHeader.MergedLines.Count);
			var cusEntryLine = entryHeader.MergedLines[50];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			var expectedError = string.Format(CultureInfo.CurrentCulture, SGConstants.MaxLinesValidation.MaxEntryLimitForDeclaration + "\r\nThe merged line count is currently {0} entry lines.", declaration.MergedLinesCount);
			AssertHasMessageError("Merged lines also now exceed the maximum number of lines allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("Merged lines on merging now all merge into 1 line", 1, entryHeader.MergedLines.Count);
			cusEntryLine = entryHeader.MergedLines[0];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			AssertNoMessageError("Merged lines are now within the maximum number allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);
		}
	}
}
