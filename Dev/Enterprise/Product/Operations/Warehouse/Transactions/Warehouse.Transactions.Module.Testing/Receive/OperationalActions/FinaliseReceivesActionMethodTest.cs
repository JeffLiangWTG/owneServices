using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseReceivesActionMethod))]
	class FinaliseReceivesActionMethodTest : FinalizeDocketActionMethodTest<FinaliseReceivesActionMethod, WhsReceive>
	{
		protected override string ExpectedOperationalActionName => "Finalize Receives";
	}
}
