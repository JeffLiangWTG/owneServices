using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderLookups))]
	sealed class ExportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override JobComInvoiceHeaderLookups GetLookups() => new ExportJobComInvoiceHeaderLookups(invoice);
	}
}
