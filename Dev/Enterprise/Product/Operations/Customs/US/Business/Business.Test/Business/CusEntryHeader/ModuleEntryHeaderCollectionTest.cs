using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ModuleEntryHeaderCollection))]
	sealed class ModuleEntryHeaderCollectionTest : Customs.Business.Testing.ModuleEntryHeaderCollectionTest<ModuleEntryHeaderCollection>
	{
		public override void TestEntryHeaderCollection()
		{
			CusEntryHeader entry1 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			entry1.CH_BGMReference = string.Empty;
			CusEntryHeader entry2 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			entry2.CH_BGMReference = "US_REF";
			Factory.Save();
			var entries = GetCollectionToTest();
			Assert("Should contain entry 1", entries.Contains(entry1));
			Assert("Should contain entry 2", entries.Contains(entry2));
		}

		public void TestRelationshipFilter()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			Factory.Save();
			ModuleEntryHeaderCollection entries = new ModuleEntryHeaderCollection(new BusinessObjectFactory());
			AssertEquals(true, entries.Contains(entry1));
			AssertEquals(false, entries.Contains(entry2));
		}

		protected override ModuleEntryHeaderCollection GetCollectionToTest() => new ModuleEntryHeaderCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			return entry;
		}
	}
}
