using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AESEntryCollection))]
	sealed class AESEntryCollectionTest : BusinessObjectCollectionViewTestCase<AESEntryCollection>
	{
		public void TestIsThisPartOfCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry1.US_IsDeactivated = true;
			AssertEquals(true, entry1.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry1.HasBeenWithdrawn);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry2.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry2.HasBeenWithdrawn);

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry3.HasBeenLodgedAtCustoms);
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			entry3.US_IsDeactivated = true;
			AssertEquals(true, entry3.HasBeenWithdrawn);

			var entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry4.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals(false, entry4.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry4.HasBeenWithdrawn);

			Factory.Save();
			var collection = new AESEntryCollection(declaration);
			AssertEquals(3, collection.Count);

			AssertCollectionContains("Deactivated lodged entry", entry1, collection);
			AssertCollectionContains("entry for amendment", entry2, collection);
			AssertCollectionContains("not lodged entry", entry4, collection);
		}

		public void TestAddAndRemove()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			var aesDeclaration = new AESDeclaration(declaration);
			var entries = aesDeclaration.Entries;
			AssertEquals(typeof(AESEntryCollection), entries.GetType());
			AssertEquals(1, entries.Count);
			AssertEquals("AllowNew", false, entries.AllowNew);
			AssertEquals("AllowRemove", false, entries.AllowRemove);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusEntryHeader>();

		protected override AESEntryCollection GetCollectionToTest() => new AESEntryCollection(Declaration);
	}
}
