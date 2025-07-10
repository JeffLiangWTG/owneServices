using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(DynamicWorkOrderOperationalActionSupporter))]
	public class DynamicWorkOrderOperationalActionSupporterTest : WhsOperationalActionSupporterTest<DynamicWorkOrderOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsDynamicWorkOrder;
	}
}
