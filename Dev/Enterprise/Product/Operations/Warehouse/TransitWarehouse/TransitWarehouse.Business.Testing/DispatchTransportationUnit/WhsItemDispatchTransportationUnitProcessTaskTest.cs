using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitProcessTask))]
	class WhsItemDispatchTransportationUnitProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsItemDispatchTransportationUnit>().WorkflowItems.AddNew();
		}
	}
}
