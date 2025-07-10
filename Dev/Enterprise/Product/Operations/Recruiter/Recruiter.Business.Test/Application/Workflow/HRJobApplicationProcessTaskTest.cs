using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationProcessTask))]
	sealed class HRJobApplicationProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			HRJobApplication application = Factory.NewWithValidTestData<HRJobApplication>();
			return ((IWorkflowProvider)application).WorkflowItems
				.AddNew();
		}
	}
}
