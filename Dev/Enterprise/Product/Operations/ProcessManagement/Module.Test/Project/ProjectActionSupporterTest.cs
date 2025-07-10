using Enterprise.ProcessManagement.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(ProjectActionSupporter))]
	public class ProjectActionSupporterTest : OperationalActionSupporterTest<ProjectActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Project; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals(Project.SingularName, Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Projects", Supporter.PluralElementNoun);
		}
	}
}
