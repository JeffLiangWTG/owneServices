using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeaderWithDeactivatedCollection))]
	sealed class EntryHeaderWithDeactivatedCollectionTest : ActiveBusinessObjectCollectionTestCase<EntryHeaderWithDeactivatedCollection>
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var collection = declaration.EntryHeadersWithOptionalDeactivated;
			IBindingList list = collection;
			AssertEquals(false, list.AllowNew);
		}

		public void TestIncludeAndExcludeDeactivated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ShowDeactivatedEntries = false;
			EntryHeaderWithDeactivatedCollection collection = declaration.EntryHeadersWithOptionalDeactivated;
			AssertEquals(0, collection.Count);
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(1, collection.Count);
			AssertEquals(entry1, collection[0]);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(2, collection.Count);
			AssertCollectionContains(entry1, collection);
			AssertCollectionContains(entry2, collection);
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(3, collection.Count);
			AssertCollectionContains(entry1, collection);
			AssertCollectionContains(entry2, collection);
			AssertCollectionContains(entry3, collection);
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry2.US_IsDeactivated = true;
			AssertEquals(2, collection.Count);
			AssertCollectionContains(entry1, collection);
			AssertCollectionContains(entry3, collection);
			declaration.ShowDeactivatedEntries = true;
			AssertEquals(3, collection.Count);
			AssertCollectionContains(entry1, collection);
			AssertCollectionContains(entry2, collection);
			AssertCollectionContains(entry3, collection);
		}

		protected override EntryHeaderWithDeactivatedCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return new EntryHeaderWithDeactivatedCollection(declaration);
		}
	}
}
