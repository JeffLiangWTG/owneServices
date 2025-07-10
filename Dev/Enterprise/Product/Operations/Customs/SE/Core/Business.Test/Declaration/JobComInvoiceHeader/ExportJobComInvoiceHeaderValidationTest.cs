using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderValidation))]
	sealed class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceHeaderValidation GetValidation() => new ExportJobComInvoiceHeaderValidation(invoice);
	}
}
