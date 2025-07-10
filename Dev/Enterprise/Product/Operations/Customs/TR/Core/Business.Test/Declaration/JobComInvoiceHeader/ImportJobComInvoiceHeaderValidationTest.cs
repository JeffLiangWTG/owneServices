using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderValidation))]
	class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceHeaderValidation GetValidation() => new ImportJobComInvoiceHeaderValidation(invoice);
	}
}
