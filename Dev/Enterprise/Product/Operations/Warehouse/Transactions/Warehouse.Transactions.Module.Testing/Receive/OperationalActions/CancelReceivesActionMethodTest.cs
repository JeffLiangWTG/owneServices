using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CancelReceivesActionMethod))]
	class CancelReceivesActionMethodTest : CancelDocketActionMethodTest<CancelReceivesActionMethod, WhsReceive>
	{
		protected override string GetExpectedNameAndDescription() => "Cancel Receives";

		protected override CancelReceivesActionMethod NewMethod()
		{
			return new CancelReceivesActionMethod();
		}
	}
}
