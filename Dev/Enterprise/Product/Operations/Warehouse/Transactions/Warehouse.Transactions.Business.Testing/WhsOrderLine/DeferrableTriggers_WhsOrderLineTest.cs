using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLine))]
	class DeferrableTriggers_WhsOrderLineTest : DeferrableTriggerTestCase<WhsOrderLine>
	{
	}
}
