using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SequenceNumberGeneratorTest : TestCaseWithFactory
	{
		public void TestReOrderLineNumbers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();

			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoice2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line5 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("PreCondition", (short)1, line1.JI_LineNo);
			AssertEquals("PreCondition", (short)2, line2.JI_LineNo);
			AssertEquals("PreCondition", (short)3, line3.JI_LineNo);
			AssertEquals("PreCondition", (short)1, line4.JI_LineNo);
			AssertEquals("PreCondition", (short)2, line5.JI_LineNo);

			line2.JI_JZ = invoice2.PK;
			AssertEquals((short)1, line1.JI_LineNo);
			AssertEquals((short)2, line3.JI_LineNo);

			AssertEquals((short)1, line4.JI_LineNo);
			AssertEquals((short)2, line5.JI_LineNo);
			AssertEquals((short)3, line2.JI_LineNo);

			line1.JI_JZ = invoice2.PK;
			AssertEquals((short)1, line3.JI_LineNo);

			AssertEquals((short)1, line4.JI_LineNo);
			AssertEquals((short)2, line5.JI_LineNo);
			AssertEquals((short)3, line2.JI_LineNo);
			AssertEquals((short)4, line1.JI_LineNo);

			line5.JI_LineNo = 3;
			AssertEquals((short)1, line4.JI_LineNo);
			AssertEquals((short)3, line5.JI_LineNo);
			AssertEquals((short)2, line2.JI_LineNo);
			AssertEquals((short)4, line1.JI_LineNo);

			line2.Delete();
			AssertEquals((short)1, line4.JI_LineNo);
			AssertEquals((short)2, line5.JI_LineNo);
			AssertEquals((short)3, line1.JI_LineNo);
		}

		public void TestGetSuspender()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine.JI_LineNo = 999;
			}
			AssertEquals("Suspender should have worked and stop renumbering... Suspender is used for data transfer", (short)999, invoiceLine.JI_LineNo);
		}

		public void TestRecalculateAll()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();

			using (invoice.GetLineNumberRenumberingSuspender())
			{
				invoiceLine2.Delete();
			}
			AssertEquals("Suspender should have worked and stop renumbering... Suspender is used for data transfer", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("Suspender should have worked and stop renumbering... Suspender is used for data transfer", (short)4, invoiceLine4.JI_LineNo);
			invoice.InvoiceLineLineNumberGenerator.ReCalculateAll();
			AssertEquals((short)1, invoiceLine1.JI_LineNo);
			AssertEquals((short)2, invoiceLine3.JI_LineNo);
			AssertEquals((short)3, invoiceLine4.JI_LineNo);
		}

		public void TestExcessiveInvoiceLinesRecalculation()
		{
			var errMsgLineExceedingMaxValue = $"Please enter a valid Line Number; valid number should be greater than zero and no greater than {short.MaxValue}.";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.AddNew();
			var collection = new InvoiceLineViewCollection<BaseJobComInvoiceLine>(declaration);
			var maxLineNumber = short.MaxValue + 1;

			AssertNoExceptionThrown(() =>
			{
				using (collection.SuspendAdditionallyForImport())
				{
					for (int i = 1; i <= maxLineNumber; i++)
					{
						collection.AddNew();
					}
				}
			});

			AssertEquals("Count", maxLineNumber, collection.Count);

			var excessiveLine = collection[short.MaxValue];
			AssertEquals("Excessive line", ZShort.Zero, excessiveLine.JI_LineNo);
			excessiveLine.Validation.ValidateJI_LineNo();
			AssertHasMessageError("Has message error", excessiveLine.JI_LineNoInfo, errMsgLineExceedingMaxValue);

			var excessiveLine2 = collection.AddNew();
			AssertEquals("Another excessive line", ZShort.Zero, excessiveLine2.JI_LineNo);

			var validLine = collection[short.MaxValue - 1];
			AssertNotEquals("Not Zero", ZShort.Zero, validLine.JI_LineNo);
			validLine.Validation.ValidateJI_LineNo();
			AssertNoMessageError("No message error", validLine.JI_LineNoInfo, errMsgLineExceedingMaxValue);
		}

		public void TestEnsureValidSequenceNumber()
		{
			AssertEquals("reset to 0 if less than 0.", (short)0, ShortSequenceNumberGenerator.EnsureValidSequenceNumber(-1));
			AssertEquals("assign the sequence number as it is", (short)123, ShortSequenceNumberGenerator.EnsureValidSequenceNumber(123));
			AssertEquals("assign the sequence number as it is", short.MaxValue, ShortSequenceNumberGenerator.EnsureValidSequenceNumber(short.MaxValue));
			AssertEquals("reset to 0 if exceeding max sequence number", (short)0, ShortSequenceNumberGenerator.EnsureValidSequenceNumber(short.MaxValue + 1));

			AssertEquals("reset to 0 if less than 0.", 0, HugeSequenceNumberGenerator.EnsureValidSequenceNumber(-1));
			AssertEquals("assign the sequence number as it is", 123, HugeSequenceNumberGenerator.EnsureValidSequenceNumber(123));
			AssertEquals("assign the sequence number as it is", int.MaxValue, HugeSequenceNumberGenerator.EnsureValidSequenceNumber(int.MaxValue));
			AssertEquals("reset to 0 if exceeding max sequence number", 0, HugeSequenceNumberGenerator.EnsureValidSequenceNumber(long.Parse(int.MaxValue.ToString()) + 1));
		}

		public void TestSequenceStartingNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var sequenceGenerator = new ShortSequenceNumberGenerator(declaration);
			AssertEquals(1, sequenceGenerator.SequenceStartingNumber);
		}
	}
}
