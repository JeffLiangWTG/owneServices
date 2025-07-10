using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferLine))]
	class DeferrableTriggers_WhsTransferLineTest : DeferrableTriggerTestCase<WhsTransferLine>
	{
	}
}
