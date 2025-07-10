using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessEstimateLog))]
	class ProcessEstimateLogTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyProcessEstimateLog = Factory.New<ProcessEstimateLog>();
			dummyProcessEstimateLog.P9E_ParentTableCode = ProcessTasksSchema.Constants.Prefix;
			dummyProcessEstimateLog.P9E_LogDateTime = ZDateTimeOffset.UtcNow;
			return dummyProcessEstimateLog;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
