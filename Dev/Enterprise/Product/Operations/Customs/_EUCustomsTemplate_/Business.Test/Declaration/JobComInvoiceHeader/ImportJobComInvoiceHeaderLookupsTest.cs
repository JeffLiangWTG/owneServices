using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderLookups))]
	sealed class ImportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest<ImportJobComInvoiceHeaderLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override ImportJobComInvoiceHeaderLookups GetLookups() => new ImportJobComInvoiceHeaderLookups(invoice);
	}
}
