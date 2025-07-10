using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusHAWBProcessTask))]
	sealed class CusHAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var cusHAWB = Factory.New<CusHAWB>();
			var task = (CusHAWBProcessTask)((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
			AssertEquals(cusHAWB, task.Parent);
		}

		public override void TestCanExportEDocViaUniversalXml()
		{
			var processTask = (CusHAWBProcessTask)BusinessObject;
			((CusHAWB)processTask.Parent).CS_ApplicationCode = "CMR"; // to support CusHAWBProcessTaskLoadStrategy
			base.TestCanExportEDocViaUniversalXml();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusHAWB = Factory.New<CusHAWB>();
			return ((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
		}
	}
}
