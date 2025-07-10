using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AESCusDispositionCollection))]
	sealed class AESCusDispositionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompleteFilter()
		{
			var collection = GetCollectionToTest();
			var completeFilter = collection.CompleteFilter.LiteralTextSqlFormatted;
			AssertContains("CDI_ParentID =", completeFilter);
			AssertContains("CDI_Type = 'AES'", completeFilter);
		}

		public void TestAddNewChild()
		{
			var collection = GetCollectionToTest();
			var child = collection.AddNew() as CusDisposition;
			AssertEquals("AES", child.CDI_Type);
			AssertEquals(cusEntryHeader.PK, child.CDI_ParentID);
			AssertEquals(CusEntryHeaderSchema.Constants.Prefix, child.CDI_ParentTableCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			cusEntryHeader = Factory.New<CusEntryHeader>();
			return new AESCusDispositionCollection(cusEntryHeader);
		}

		CusEntryHeader cusEntryHeader;
	}
}
