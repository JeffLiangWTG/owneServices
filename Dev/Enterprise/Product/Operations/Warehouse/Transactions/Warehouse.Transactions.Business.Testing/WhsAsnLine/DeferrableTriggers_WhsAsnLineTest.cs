using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAsnLine))]
	class DeferrableTriggers_WhsAsnLineTest : DeferrableTriggerTestCase<WhsAsnLine>
	{
	}
}
