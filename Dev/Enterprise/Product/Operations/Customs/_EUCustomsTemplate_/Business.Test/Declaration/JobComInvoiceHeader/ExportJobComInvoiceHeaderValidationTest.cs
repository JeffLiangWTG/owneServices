using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderValidation))]
	class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest<ExportJobComInvoiceHeaderValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobComInvoiceHeaderValidation GetValidation() => new ExportJobComInvoiceHeaderValidation(invoice);
	}
}
