using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderValidation))]
	class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceHeaderValidation GetValidation() => new ExportJobComInvoiceHeaderValidation(invoice);
	}
}
