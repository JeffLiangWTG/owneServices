using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvLineRefsCollection<JobComInvLineRefs>))]
	sealed class JobComInvLineRefsCollectionBaseOnlyTest : JobComInvLineRefsCollectionTest<JobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<JobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			return new JobComInvLineRefsCollection<JobComInvLineRefs>(Factory.New<JobComInvoiceLine>(), "ABC");
		}
	}
}
