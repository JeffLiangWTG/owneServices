using Enterprise.ProcessManagement.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(WorkItemActionSupporter))]
	public class WorkItemActionSupporterTest : OperationalActionSupporterTest<WorkItemActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WorkItem; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals(WorkItem.SingularName, Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Work Items", Supporter.PluralElementNoun);
		}
	}
}
