using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessFieldChangeRule))]
	public class ProcessFieldChangeRuleAuditParentTests : AuditParentTest<ProcessFieldChangeRule>
	{
		protected override ProcessFieldChangeRule NewTestAuditParent()
		{
			return Factory.New<ProcessFieldChangeRule>();
		}

		public void TestCorrectRelationship()
		{
			Assert((Factory.New<ProcessFieldChangeRule>() as IAuditParent).RelatedAuditChildren.Any(
				aCI => (aCI.KeyColumn, aCI.InfoColumn) == (ProcessFieldChangeRuleFieldSchema.PFL_PFR, ProcessFieldChangeRuleFieldSchema.PFL_FieldName)
				));
		}
	}
}
