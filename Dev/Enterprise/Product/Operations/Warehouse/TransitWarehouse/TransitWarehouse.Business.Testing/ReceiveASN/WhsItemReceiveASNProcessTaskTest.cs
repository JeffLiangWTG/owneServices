using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveASNProcessTask))]
	class WhsItemReceiveASNProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsItemReceiveASN>().WorkflowItems.AddNew();
		}
	}
}
