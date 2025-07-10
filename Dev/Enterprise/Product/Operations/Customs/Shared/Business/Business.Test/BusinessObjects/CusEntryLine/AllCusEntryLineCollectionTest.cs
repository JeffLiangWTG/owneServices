using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AllCusEntryLineCollection<CusEntryLine>))]
	public class AllCusEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindByLineNumber()
		{
			var testCollection = GetCollectionToTest() as IAllCusEntryLineCollection<CusEntryLine>;
			var entryLine1 = testCollection.AddNew();
			entryLine1.CL_LineNumber = (short)1;
			var entryLine2 = testCollection.AddNew();
			entryLine2.CL_LineNumber = (short)2;
			AssertEquals("FindByLineNumber", entryLine2, testCollection.FindByLineNumber(2));
			AssertNull("FindByLineNumber", testCollection.FindByLineNumber(3));
		}

		public void TestAllowNew()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals("AllowNew", false, testCollection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AllCusEntryLineCollection<CusEntryLine>(EntryHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusEntryLine>();
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

		protected CusEntryHeader EntryHeader
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
	}
}
