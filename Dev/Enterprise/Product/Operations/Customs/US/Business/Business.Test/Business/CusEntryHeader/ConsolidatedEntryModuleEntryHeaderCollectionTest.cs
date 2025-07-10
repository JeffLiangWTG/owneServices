using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ConsolidatedEntryModuleEntryHeaderCollection))]
	sealed class ConsolidatedEntryModuleEntryHeaderCollectionTest : Customs.Business.Testing.ModuleEntryHeaderCollectionTest<ConsolidatedEntryModuleEntryHeaderCollection>
	{
		public override void TestEntryHeaderCollection()
		{
			var entry1 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			var entry2 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			Factory.Save();
			var entries = GetCollectionToTest();
			Assert("Should contain entry 1", entries.Contains(entry1));
			Assert("Should contain entry 2", entries.Contains(entry2));
		}

		public void TestRelationshipFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			var entries = new ConsolidatedEntryModuleEntryHeaderCollection(new BusinessObjectFactory());
			AssertEquals(true, entries.Contains(entry1));
			AssertEquals(false, entries.Contains(entry2));
		}

		protected override ConsolidatedEntryModuleEntryHeaderCollection GetCollectionToTest() => new ConsolidatedEntryModuleEntryHeaderCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Factory.Save();
			return entry;
		}
	}
}
