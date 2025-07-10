using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(EntryCollectionWithPassedEntries))]
	sealed class EntryCollectionWithPassedEntriesTest : BusinessObjectCollectionTestCase
	{
		public override void TestLoad()
		{
			CusEntryHeader entry = TestDec.CustomsEntryHeaders.AddNew();
			EntryCollectionWithPassedEntries testCollection = new EntryCollectionWithPassedEntries(new CusEntryHeader[] { entry }, Factory);

			bool notSupportedExceptionHappened = false;
			try
			{
				testCollection.Load();
			}
			catch (NotSupportedException)
			{
				notSupportedExceptionHappened = true;
			}
			Assert(notSupportedExceptionHappened);
		}

		public void TestContruction()
		{
			CusEntryHeader entry = TestDec.CustomsEntryHeaders.AddNew();
			EntryCollectionWithPassedEntries testCollection = new EntryCollectionWithPassedEntries(new CusEntryHeader[] { entry }, Factory);
			AssertEquals("TestCollection should have 1 item", 1, testCollection.Count);
			AssertEquals("It should be Entry", entry, testCollection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EntryCollectionWithPassedEntries(Array.Empty<CusEntryHeader>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TestDec.CustomsEntryHeaders.AddNew();
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = BaseJobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;
	}
}
