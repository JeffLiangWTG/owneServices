using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineValidation))]
	sealed class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceLineValidation GetValidation() => new ExportJobComInvoiceLineValidation(invoiceLine);
	}
}
