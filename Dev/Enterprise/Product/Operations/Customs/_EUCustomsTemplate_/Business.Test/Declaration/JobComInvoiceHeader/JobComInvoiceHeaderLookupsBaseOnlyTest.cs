using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLookups))]
	class JobComInvoiceHeaderLookupsBaseOnlyTest : JobComInvoiceHeaderLookupsAbstractTest<JobComInvoiceHeaderLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceHeaderLookups GetLookups() => new JobComInvoiceHeaderLookups(invoice);
	}
}
