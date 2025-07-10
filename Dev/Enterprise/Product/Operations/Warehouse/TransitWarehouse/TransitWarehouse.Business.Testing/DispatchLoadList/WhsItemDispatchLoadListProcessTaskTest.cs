using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListProcessTask))]
	public class WhsItemDispatchLoadListProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			return loadList.WorkflowItems.AddNew();
		}
	}
}
