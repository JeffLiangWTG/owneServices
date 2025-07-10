using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderValidation))]
	sealed class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceHeaderValidation GetValidation() => new ImportJobComInvoiceHeaderValidation(invoice);
	}
}
