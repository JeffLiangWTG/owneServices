using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderLine))]
	class DeferrableTriggers_WhsWorkOrderLineTest : DeferrableTriggerTestCase<WhsWorkOrderLine>
	{
	}
}
