using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommunicationActionSupporter))]
	sealed class CommunicationActionSupporterTest : OperationalActionSupporterTest<CommunicationActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Communication; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("communication", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("communications", Supporter.PluralElementNoun);
		}
	}
}
