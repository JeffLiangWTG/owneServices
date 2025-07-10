using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(RefContainerStockProcessTask))]
	internal class RefContainerStockProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			RefContainerStockProcessTask task = Factory.New<RefContainerStockProcessTask>();
			AssertEquals(ControllerIDs.AgencyContainerManager, task.ParentControllerID);
		}

		public void TestParent()
		{
			RefContainerStock refContainerStock = Factory.New<RefContainerStock>();
			RefContainerStockProcessTask task = (RefContainerStockProcessTask)((IWorkflowProvider)refContainerStock).WorkflowItems.AddNew();
			AssertEquals(refContainerStock, task.Parent);
		}

		#region Implementation
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (RefContainerStockProcessTask)GetNewBusinessObject();
			result.Parent.R6_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var refContainerStock = Factory.New<RefContainerStock>();
			return ((IWorkflowProvider)refContainerStock).WorkflowItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var refContainer = factory.NewWithValidTestData<RefContainer>();
			var stock = factory.New<RefContainerStock>();
			stock.R6_RC = refContainer.PK;
			return ((IWorkflowProvider)stock).WorkflowItems.AddNew();
		}
		#endregion
	}
}
