using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTaskTemplateOperationalActionSupporter))]
	sealed class ProcessTaskTemplateOperationalActionSupporterTest : OperationalActionSupporterTest<ProcessTaskTemplateOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessTemplates; }
		}

		public override bool ShouldSupportDocuments => false;

		public void TestSingularElementNoun()
		{
			AssertEquals("Workflow Template", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Workflow Templates", Supporter.PluralElementNoun);
		}
	}
}
