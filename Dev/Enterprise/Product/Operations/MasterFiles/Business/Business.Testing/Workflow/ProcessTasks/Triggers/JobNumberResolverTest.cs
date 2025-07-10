using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobNumberResolverTest : TestCaseWithFactory
	{
		public void TestGetJobNumber()
		{
			var iJobNumber = new Mock<NonPersistentBusinessObject>().As<IJobNumber>();
			var iJobNumberForWorkflow = new Mock<NonPersistentBusinessObject>().As<IJobNumberForWorkflow>();
			var iBothJobNumber = new Mock<NonPersistentBusinessObject>().As<IJobNumber>();
			var iBothJobNumberForWorkflow = iBothJobNumber.As<IJobNumberForWorkflow>();

			iJobNumber.Setup(m => m.JobNumber).Returns("J1");
			iJobNumberForWorkflow.Setup(m => m.JobNumber).Returns("J2");
			iBothJobNumber.Setup(m => m.JobNumber).Returns("J3");
			iBothJobNumberForWorkflow.Setup(m => m.JobNumber).Returns("J4");

			AssertEquals("J1", JobNumberResolver.GetJobNumber((BusinessObject)iJobNumber.Object));
			AssertEquals("J2", JobNumberResolver.GetJobNumber((BusinessObject)iJobNumberForWorkflow.Object));
			AssertEquals("J4", JobNumberResolver.GetJobNumber((BusinessObject)iBothJobNumber.Object));
		}
	}
}
