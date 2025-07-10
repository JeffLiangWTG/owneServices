using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLookups))]
	sealed class JobComInvoiceHeaderLookupsBaseOnlyTest : JobComInvoiceHeaderLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceHeaderLookups GetLookups() => new JobComInvoiceHeaderLookups(invoice);
	}
}
