using System;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferUnAssignAllLinesPickOnlyActionMethod))]
	public class TransferUnAssignAllLinesPickOnlyActionMethodTest : UnAssignAllLinesActionMethodTest<TransferUnAssignAllLinesPickOnlyActionMethod, WhsTransfer>
	{
		protected override TransferUnAssignAllLinesPickOnlyActionMethod NewMethod() => new TransferUnAssignAllLinesPickOnlyActionMethod();

		protected override Type ApplicatorType => typeof(TransferUnAssignAllLinesActionMethodApplicator);

		protected override string OperationalActionName => "Un-assign Transfer Lines (Pick only)";
	}
}
