using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderLookups))]
	class ExportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceHeaderLookups GetLookups() => new ExportJobComInvoiceHeaderLookups(invoice);
	}
}
