using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayJob))]
	public class WhsPutawayJobDeferrableTriggerAllowsUnfinalizingJob : DeferrableTriggerTestCase<WhsPutawayJob>
	{
	}
}
