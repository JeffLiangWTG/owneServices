using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitProcessTask))]
	class WhsItemReceiveTransportationUnitProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsItemReceiveTransportationUnit>().WorkflowItems.AddNew();
		}
	}
}
