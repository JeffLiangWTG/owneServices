using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QuotaCusDispositionCollection))]
	sealed class QuotaCusDispositionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompleteFilter()
		{
			var collection = GetCollectionToTest();
			var completeFilter = collection.CompleteFilter.LiteralTextSqlFormatted;
			AssertContains("CDI_ParentID =", completeFilter);
			AssertContains("CDI_Type = 'QTA'", completeFilter);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			return new QuotaCusDispositionCollection(cusEntryLine);
		}
	}
}
