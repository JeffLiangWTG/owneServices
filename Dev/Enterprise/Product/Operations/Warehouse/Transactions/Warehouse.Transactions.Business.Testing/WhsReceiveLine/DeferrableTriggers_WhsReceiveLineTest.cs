using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveLine))]
	class DeferrableTriggers_WhsReceiveLineTest : DeferrableTriggerTestCase<WhsReceiveLine>
	{
	}
}
