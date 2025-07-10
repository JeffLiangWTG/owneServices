using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineValidation))]
	sealed class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceLineValidation GetValidation() => new JobComInvoiceLineValidation(invoiceLine);
	}
}
