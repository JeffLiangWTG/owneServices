using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestsSubclassesOf(typeof(EntryHeaderOperationalActionSupporter))]
	public abstract class EntryHeaderOperationalActionSupporterAbstractTest<T> : OperationalActionSupporterTest<T>
		where T : EntryHeaderOperationalActionSupporter
	{
		public void TestBusinessContext()
		{
			AssertEquals(nameof(entryHeaderOperationalActionSupporter.BusinessContext), BusinessContext.CusEntryHeader, entryHeaderOperationalActionSupporter.BusinessContext);
		}

		public void TestBaseCheckpoint()
		{
			AssertEquals(nameof(entryHeaderOperationalActionSupporter.BaseCheckpoint), Env.Security.CustomsDeclarationEnquiry, entryHeaderOperationalActionSupporter.BaseCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeaderOperationalActionSupporter = new EntryHeaderOperationalActionSupporter();
		}

		protected EntryHeaderOperationalActionSupporter entryHeaderOperationalActionSupporter;

		protected sealed override ModuleIdentifier ModuleID => ModuleIDs.Customs.EntryHeader;

		protected override SecurityCheckpoint ExpectedCheckpointGrandparent => Module.SecurityCheckpoint.Parent;
	}

	[TestedType(typeof(EntryHeaderOperationalActionSupporter))]
	sealed class EntryHeaderOperationalActionSupporterBaseOnlyTest : EntryHeaderOperationalActionSupporterAbstractTest<EntryHeaderOperationalActionSupporter>
	{
		public void TestRootType()
		{
			AssertEquals(nameof(EntryHeaderOperationalActionSupporter.RootType), typeof(CusEntryHeader), entryHeaderOperationalActionSupporter.RootType);
		}
	}
}
