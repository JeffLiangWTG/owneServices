using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolProcessTaskCollection))]
	public class CFSLoadListConsolProcessTaskCollectionTest : ProcessTaskCollectionTest<CFSLoadListConsolProcessTaskCollection>
	{
		public void TestParent()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			AssertEquals(loadList, ((IWorkflowProvider)loadList).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var collection = ((IWorkflowProvider)loadList).WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		public void TestLoad_DoNotReturnResultIfLoadListIsAlsoForwardRegistered()
		{
			var loadList1 = Factory.New<CFSLoadListConsol>();
			loadList1.JK_IsForwarding = false;
			var loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.JK_IsForwarding = true;

			var task1a = Factory.New<CFSLoadListConsolProcessTask>();
			task1a.P9_ParentID = loadList1.PK;
			var task1b = Factory.New<CFSLoadListConsolProcessTask>();
			task1b.P9_ParentID = loadList1.PK;
			var task2a = Factory.New<ProcessTask>();
			task2a.P9_ParentID = loadList2.PK;
			var task2b = Factory.New<ProcessTask>();
			task2b.P9_ParentID = loadList2.PK;

			AssertEquals(2, ((IWorkflowProvider)loadList1).WorkflowItems.Count);
			AssertEquals(0, ((IWorkflowProvider)loadList2).WorkflowItems.Count);
		}

		public void TestOriginCountry()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_RL_NKLoadPort = "MYPKG";

			var collection = new CFSLoadListConsolProcessTaskCollection(loadList);
			AssertEquals("MY", collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_RL_NKDischargePort = "MYPKG";

			var collection = new CFSLoadListConsolProcessTaskCollection(loadList);
			AssertEquals("MY", collection.DestinationCountry);
		}

		protected override CFSLoadListConsolProcessTaskCollection GetCollectionToTestCore()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			return new CFSLoadListConsolProcessTaskCollection(loadList);
		}
	}
}
