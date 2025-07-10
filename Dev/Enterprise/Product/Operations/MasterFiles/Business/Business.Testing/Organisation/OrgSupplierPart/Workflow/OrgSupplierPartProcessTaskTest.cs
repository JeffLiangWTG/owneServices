using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartProcessTask))]
	class OrgSupplierPartProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var processTask = orgSupplierPart.WorkflowItems.AddNew();
			AssertEquals(orgSupplierPart, processTask.Parent);
			AssertEquals(ControllerIDs.WhsConfigProduct, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgSupplierPart>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
