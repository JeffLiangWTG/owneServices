using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineValidation))]
	class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ExportJobComInvoiceLineValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobComInvoiceLineValidation GetValidation() => new ExportJobComInvoiceLineValidation(invoiceLine);
	}
}
