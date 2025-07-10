using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineValidation))]
	class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceLineValidation GetValidation() => new ExportJobComInvoiceLineValidation(invoiceLine);

		public void TestCheckJI_PreviousEntryNumber()
		{
			SetIsPreviousEntryAvailableToTrue();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryNumberInfo);
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			SetIsPreviousEntryAvailableToTrue();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo);
		}
	}
}
