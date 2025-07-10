using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineLookups))]
	sealed class ExportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ExportJobComInvoiceLineLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobComInvoiceLineLookups GetLookups() => new ExportJobComInvoiceLineLookups(invoiceLine);
	}
}
