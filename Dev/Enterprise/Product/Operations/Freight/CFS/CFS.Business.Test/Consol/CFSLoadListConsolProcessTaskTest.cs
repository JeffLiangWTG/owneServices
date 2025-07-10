using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolProcessTask))]
	class CFSLoadListConsolProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var task = Factory.New<CFSLoadListConsolProcessTask>();
			AssertEquals(ControllerIDs.LoadListConsol, task.ParentControllerID);
		}

		public void TestParent()
		{
			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			var task = ((IWorkflowProvider)loadList).WorkflowItems.AddNew();
			AssertEquals(loadList, task.Parent);
			Factory.Save();

			AssertEquals("Type decided correctly", typeof(CFSLoadListConsolProcessTask), new BusinessObjectFactory().Load<ProcessTask>(task.PK).GetType());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			return ((IWorkflowProvider)loadList).WorkflowItems.AddNew();
		}

		#endregion

	}
}
