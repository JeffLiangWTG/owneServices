using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderValidation))]
	sealed class JobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceHeaderValidation GetValidation() => new JobComInvoiceHeaderValidation(invoice);
	}
}
