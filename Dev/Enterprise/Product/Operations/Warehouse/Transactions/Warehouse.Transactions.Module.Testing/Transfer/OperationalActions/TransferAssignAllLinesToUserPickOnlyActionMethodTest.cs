using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferAssignAllLinesToUserPickOnlyActionMethod))]
	class TransferAssignAllLinesToUserPickOnlyActionMethodTest : AssignAllLinesToUserActionMethodTest<TransferAssignAllLinesToUserPickOnlyActionMethod, WhsTransfer>
	{
		protected override string GetExpectedNameAndDescription() => "Assign Transfer Lines to User (Pick only)";

		protected override TransferAssignAllLinesToUserPickOnlyActionMethod NewMethod()
		{
			return new TransferAssignAllLinesToUserPickOnlyActionMethod();
		}
	}
}
