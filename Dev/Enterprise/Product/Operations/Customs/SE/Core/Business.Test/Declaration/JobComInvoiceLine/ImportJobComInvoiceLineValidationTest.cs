using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceLineValidation))]
	sealed class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceLineValidation GetValidation() => new ImportJobComInvoiceLineValidation(invoiceLine);
	}
}
