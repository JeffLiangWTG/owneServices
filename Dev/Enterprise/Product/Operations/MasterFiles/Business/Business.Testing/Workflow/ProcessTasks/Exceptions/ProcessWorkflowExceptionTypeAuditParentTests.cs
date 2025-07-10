using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Workflow.ProcessTasks.Exceptions
{
	[TestedType(typeof(ProcessWorkflowExceptionType))]
	internal class ProcessWorkflowExceptionTypeAuditParentTests : AuditParentTest<ProcessWorkflowExceptionType>
	{
		protected override ProcessWorkflowExceptionType NewTestAuditParent()
		{
			return Factory.New<ProcessWorkflowExceptionType>();
		}

		public void TestCorrectRelationship()
		{
			Assert((Factory.New<ProcessWorkflowExceptionType>() as IAuditParent).RelatedAuditChildren.Any(
				aCI => (aCI.KeyColumn, aCI.InfoColumn) == (ProcessWorkflowExceptionCauseSchema.WEC_WET_Type, ProcessWorkflowExceptionCauseSchema.WEC_Code)
			));
			Assert((Factory.New<ProcessWorkflowExceptionType>() as IAuditParent).RelatedAuditChildren.Any(aCI =>
				(aCI.KeyColumn, aCI.InfoColumn) == (ProcessWorkflowExceptionResolutionSchema.WER_WET_Type,
					ProcessWorkflowExceptionResolutionSchema.WER_Code)));
		}
	}
}
