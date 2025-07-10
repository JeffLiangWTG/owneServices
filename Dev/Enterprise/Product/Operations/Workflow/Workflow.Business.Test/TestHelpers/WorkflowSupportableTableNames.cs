using System.Linq;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowSupportableTableNamesWithDummy : WorkflowSupportableTableNames
	{
		public override string[] GetTableNames()
		{
			return base.GetTableNames().Concat(new[] { DummyBizoSchema.Constants.TableName }).ToArray();
		}
	}
}
