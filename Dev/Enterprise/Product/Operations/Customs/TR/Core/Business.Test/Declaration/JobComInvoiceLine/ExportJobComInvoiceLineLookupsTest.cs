using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineLookups))]
	class ExportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceLineLookups GetLookups() => new ExportJobComInvoiceLineLookups(invoiceLine);
	}
}
