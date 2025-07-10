using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WorkOrderOperationalActionSupporter))]
	public class WorkOrderOperationalActionSupporterTest : WhsOperationalActionSupporterTest<WorkOrderOperationalActionSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsWorkOrder;

		#endregion
	}
}
