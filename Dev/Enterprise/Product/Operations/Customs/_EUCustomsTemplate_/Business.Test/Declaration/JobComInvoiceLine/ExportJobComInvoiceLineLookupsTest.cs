using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineLookups))]
	class ExportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ExportJobComInvoiceLineLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobComInvoiceLineLookups GetLookups() => new ExportJobComInvoiceLineLookups(invoiceLine);
	}
}
