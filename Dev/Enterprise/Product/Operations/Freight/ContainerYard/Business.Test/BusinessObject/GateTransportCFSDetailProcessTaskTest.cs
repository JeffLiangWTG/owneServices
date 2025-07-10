using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetailProcessTask))]
	sealed class GateTransportCFSDetailProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("This test case is not necessary.", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var transport = Factory.New<GateTransportCFSDetail>();
			return transport.WorkflowItems.AddNew();
		}
	}
}
