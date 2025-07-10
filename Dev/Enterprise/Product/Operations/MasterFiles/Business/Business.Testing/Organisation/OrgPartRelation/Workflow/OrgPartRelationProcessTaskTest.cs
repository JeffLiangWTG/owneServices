using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelationProcessTask))]
	class OrgPartRelationProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var orgPartRelation = Factory.NewWithValidTestData<OrgPartRelation>();
			var processTask = orgPartRelation.WorkflowItems.AddNew();
			AssertEquals(orgPartRelation, processTask.Parent);
			AssertEquals(ControllerIDs.WhsConfigProduct, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgPartRelation>().WorkflowItems.AddNew();
		}

		#endregion
	}
}
