using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class PlugInsExtensionsTest : TestCaseWithFactory
	{
		public void TestAddJobInvoicingPlugIn()
		{
			var dummy = Factory.New<DummyJobHeaderParent>();
			using (var plugIns = new PlugIns(dummy, (ZTabControl)null))
			{
				var securityCheckpoint = new SecurityCheckpoint("Root", (NoResString)"Root", null, null, false);

				var jobInvoicingSupporter = new Mock<IJobInvoicingSupporter>();
				jobInvoicingSupporter.Setup(m => m.JobInvoicingSecurity).Returns(securityCheckpoint);

				plugIns.AddJobInvoicing(jobInvoicingSupporter.Object);

				var plugin = plugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNotNull(plugin);
				AssertEquals(securityCheckpoint, plugin.SecurityCheckpoint);
			}
		}
	}
}
