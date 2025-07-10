using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(EntryLineStatusFilterCollection))]
	sealed class EntryLineStatusFilterCollectionTest : SubsetBusinessObjectCollectionTestCase<EntryLineStatusFilterCollection, CusEntryLine>
	{
		public void TestRebuildOnConstruction()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CH = EntryHeader.PK;
			entryLine.CL_CustomsPostedStatus = "AAA";

			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_CH = EntryHeader.PK;
			entryLine2.CL_CustomsPostedStatus = "BBB";

			var collection = new EntryLineStatusFilterCollection(EntryHeader, "AAA");
			AssertEquals("Collection should have 1 item", 1, collection.Count);
			AssertEquals("Collection should have 1 item", entryLine, collection[0]);
		}

		public void TestSetCollectionRelationship()
		{
			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_CustomsPostedStatus = "AAA";

			var collection = new EntryLineStatusFilterCollection(EntryHeader, new[] { "BBB", "CCC" });
			collection.Add(entryLine1);

			AssertEquals("Customs posted status should be set", "BBB", entryLine1.CL_CustomsPostedStatus);
			AssertEquals("CL_CH should be set", EntryHeader.PK, entryLine1.CL_CH);

			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_CustomsPostedStatus = "CCC";
			collection.Add(entryLine2);
			AssertEquals("Customs posted status should be set", "CCC", entryLine2.CL_CustomsPostedStatus);

			var entryLine3 = collection.AddNew();
			AssertEquals("Customs posted status should be set", "BBB", entryLine3.CL_CustomsPostedStatus);
		}

		public void TestFindByLineNumber()
		{
			var collection = new EntryLineStatusFilterCollection(EntryHeader, "AAA");
			var entryLine = collection.AddNew();
			entryLine.CL_LineNumber = (short)1;

			var entryLine2 = collection.AddNew();
			entryLine2.CL_LineNumber = (short)3;

			AssertEquals("FindByLineNumber", entryLine, collection.FindByLineNumber(1));
			AssertEquals("FindByLineNumber", null, collection.FindByLineNumber(2));
		}

		public void TestIsThisPartOfTheCollection()
		{
			var entryLine1 = EntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_CustomsPostedStatus = "AAA";
			var entryLine2 = EntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_CustomsPostedStatus = "BBB";
			var entryLine3 = EntryHeader.AllEntryLines.AddNew();
			entryLine3.CL_CustomsPostedStatus = "CCC";
			var entryLine4 = EntryHeader.AllEntryLines.AddNew();
			entryLine4.CL_CustomsPostedStatus = "DDD";

			var collection = new EntryLineStatusFilterCollection(EntryHeader, new[] { "BBB", "CCC" });
			AssertEquals("Collection should contain only entry lines where CL_CustomsPostedStatus is part of StatusFilters", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { entryLine2, entryLine3 }, collection);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CH = EntryHeader.PK;
			entryLine.CL_CustomsPostedStatus = "ZZZ";
			return entryLine;
		}

		protected override EntryLineStatusFilterCollection GetCollectionToTest()
		{
			return new EntryLineStatusFilterCollection(EntryHeader, "ZZZ");
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<BaseJobDeclaration>();
				}
				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		#endregion
	}
}
