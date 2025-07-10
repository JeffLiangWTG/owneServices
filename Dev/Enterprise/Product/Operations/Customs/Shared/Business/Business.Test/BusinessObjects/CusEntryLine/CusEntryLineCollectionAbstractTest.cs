using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusEntryLineCollectionAbstractTest<TCusEntryLine, TCusEntryLineCollection> : SubsetBusinessObjectCollectionTestCase<TCusEntryLineCollection, CusEntryLine>
		where TCusEntryLine : CusEntryLine
		where TCusEntryLineCollection : CusEntryLineCollection<TCusEntryLine>
	{
		public void TestAllowNewAndReadOnly()
		{
			var testCollection = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("AllowNew", false, testCollection.AllowNew);
				AssertEquals("ReadOnly, should not be readonly as there are a couple of properties which should be editible in AU like Description", false, testCollection.ReadOnly);
			});
		}

		public void TestFindByLineNumber()
		{
			var testCollection = GetCollectionToTest();
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
				AssertNull("Shouldn't find anything", testCollection.FindByLineNumber(100));
				AssertEquals("Found correct line 1", line3, testCollection.FindByLineNumber(3));
				AssertEquals("Found correct line 2", line2, testCollection.FindByLineNumber(2));
				AssertEquals("Found correct line 3", line1, testCollection.FindByLineNumber(1));
			});
		}

		public void TestSort()
		{
			var testCollection = GetCollectionToTest();
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
				testCollection.Sort(CusEntryLine.Schema.CL_CustomsValue, System.ComponentModel.ListSortDirection.Ascending);
				AssertEquals("PreCondition:First line after sort", line1, testCollection[0]);
				AssertEquals("PreCondition:Second line after sort", line3, testCollection[1]);
				AssertEquals("PreCondition:Third line after sort", line2, testCollection[2]);

				testCollection.CustomSort();//sort by line number
				AssertEquals("First line after sort", line1, testCollection[0]);
				AssertEquals("Second line after sort", line2, testCollection[1]);
				AssertEquals("Third line after sort", line3, testCollection[2]);
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New(typeof(CusEntryLine));
	}
}
