using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderLookups))]
	class ImportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceHeaderLookups GetLookups() => new ImportJobComInvoiceHeaderLookups(invoice);
	}
}
