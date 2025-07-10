using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderLookups))]
	sealed class ImportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceHeaderLookups GetLookups() => new ImportJobComInvoiceHeaderLookups(invoice);
	}
}
