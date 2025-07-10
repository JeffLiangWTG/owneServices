using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ChassisJobComInvLineRefsCollection))]
	sealed class ChassisJobComInvLineRefsCollectionTest : JobComInvLineRefsCollectionTest<ChassisJobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<ChassisJobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new ChassisJobComInvLineRefsCollection(jobComInvoice);
		}
	}
}
