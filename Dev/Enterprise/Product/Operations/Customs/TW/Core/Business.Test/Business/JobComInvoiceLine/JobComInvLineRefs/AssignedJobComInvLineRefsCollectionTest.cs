using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AssignedJobComInvLineRefsCollection))]
	sealed class AssignedJobComInvLineRefsCollectionTest : JobComInvLineRefsCollectionTest<AssignedJobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<AssignedJobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new AssignedJobComInvLineRefsCollection(jobComInvoice);
		}
	}
}
