using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveConsignmentProcessTask))]
	class WhsItemReceiveConsignmentProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsItemReceiveConsignment>().WorkflowItems.AddNew();
		}
	}
}
