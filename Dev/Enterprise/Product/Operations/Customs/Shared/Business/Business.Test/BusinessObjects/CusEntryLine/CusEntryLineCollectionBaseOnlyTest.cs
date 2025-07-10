using System.Collections;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLineCollection<CusEntryLine>))]
	class CusEntryLineCollectionBaseOnlyTest : CusEntryLineCollectionAbstractTest<CusEntryLine, CusEntryLineCollection<CusEntryLine>>
	{
		public void TestCustomSort()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testCollection = new TestCusEntryLineCollection(entryHeader);

			var line1 = testCollection.AddNew();
			line1.CL_CustomsValue = 100;
			line1.CL_LineNumber = 1;
			var line2 = testCollection.AddNew();
			line2.CL_CustomsValue = 300;
			line2.CL_LineNumber = 2;
			var line3 = testCollection.AddNew();
			line3.CL_CustomsValue = 200;
			line3.CL_LineNumber = 3;

			CombineAssertions(() =>
			{
				AssertEquals("First line after sort", line1, testCollection[0]);
				AssertEquals("Second line after sort", line2, testCollection[1]);
				AssertEquals("Third line after sort", line3, testCollection[2]);

				testCollection.CustomSort(); //sort by LCT amount
				AssertEquals("First line after sort", line1, testCollection[0]);
				AssertEquals("Second line after sort", line3, testCollection[1]);
				AssertEquals("Third line after sort", line2, testCollection[2]);
			});
		}

		public void TestConstructor()
		{
			var entryHeader = Factory.New<BaseJobDeclaration>().CustomsEntryHeaders.AddNew();
			entryHeader.AllEntryLines.AddNew().CL_CustomsPostedStatus = EntryLineStatusList.Codes.Active;
			entryHeader.AllEntryLines.AddNew().CL_CustomsPostedStatus = EntryLineStatusList.Codes.Deleted;
			entryHeader.AllEntryLines.AddNew().CL_CustomsPostedStatus = EntryLineStatusList.Codes.DeletePending;
			AssertEquals(1, new CusEntryLineCollection<CusEntryLine>(entryHeader).Count);
			AssertEquals(2, new CusEntryLineCollection<CusEntryLine>(entryHeader, new string[] { EntryLineStatusList.Codes.Active, EntryLineStatusList.Codes.DeletePending }).Count);
		}

		protected override CusEntryLineCollection<CusEntryLine> GetCollectionToTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new CusEntryLineCollection<CusEntryLine>(entryHeader);
		}

		class TestCusEntryLineCollection : CusEntryLineCollection<CusEntryLine>
		{
			public TestCusEntryLineCollection(CusEntryHeader entryHeader)
				: base(entryHeader)
			{
			}

			public TestCusEntryLineCollection(CusEntryHeader entryHeader, string[] statusFilters)
				: base(entryHeader, statusFilters)
			{
			}

			class TestComparer : IComparer
			{
				public int Compare(object x, object y)
				{
					var lineX = (CusEntryLine)x;
					var lineY = (CusEntryLine)y;
					return lineX.CL_CustomsValue.CompareTo(lineY.CL_CustomsValue);
				}
			}

			protected override IComparer GetComparer() => new TestComparer();
		}
	}
}
