using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(eManifestProcessTask))]
	sealed class eManifestProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOverrides()
		{
			var processTask = (ProcessTask)GetNewBusinessObject();
			AssertEquals("ParentType", typeof(Trip), processTask.Parent.GetType());
			AssertEquals("ParentControllerID", ControllerIDs.Customs.US.eManifest, processTask.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<Trip>().WorkflowItems.AddNew();
	}
}
