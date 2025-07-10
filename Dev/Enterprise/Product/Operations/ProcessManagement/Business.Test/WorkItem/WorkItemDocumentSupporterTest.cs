using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkItemDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals(Env.Security.WorkItemCustomiseDocuments, workItem.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals(true, workItem.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessObject()
		{
			var workItem = Factory.New<WorkItem>();
			AssertEquals(BusinessContext.WorkItem, workItem.DocumentSupporter.BusinessContext);
		}
	}
}
