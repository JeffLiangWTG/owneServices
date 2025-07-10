using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchConsignmentProcessTask))]
	class WhsItemDispatchConsignmentProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsItemDispatchConsignment>().WorkflowItems.AddNew();
		}
	}
}
