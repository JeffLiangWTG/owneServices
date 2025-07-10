using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobProcessHeaderUpdaterTest : TestCaseWithFactory
	{
		public void TestDeletedObjects()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			var system = (BMSystem)helper.CreateSystem(Factory, "DUM");

			var job = Factory.New<ExplodingDummyWithWorkflow>();
			var header = ProcessJobHeaderProvider.GetForParent(job, Factory);
			job.Delete();
			new JobProcessHeaderUpdater(Factory).ProcessChanges(new[] { job });
			AssertEquals(true, job.IsDeleted);
			AssertEquals(true, header.IsDeleted);
		}

		class ExplodingDummyWithWorkflow : DummyWithWorkflow
		{
			public ExplodingDummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetWorkflowType()
			{
				_ = this.Z0_Code; // Access so we crash if deleted.
				return base.GetWorkflowType();
			}
		}
	}
}
