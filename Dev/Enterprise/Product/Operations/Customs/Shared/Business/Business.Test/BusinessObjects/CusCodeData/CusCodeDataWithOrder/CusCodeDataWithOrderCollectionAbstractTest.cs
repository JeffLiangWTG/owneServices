using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCodeDataWithOrderCollectionForTest))]
	public abstract class CusCodeDataWithOrderCollectionAbstractTest : CusCodeDataCollectionTest<CusCodeDataWithOrderForTest>
	{
		public void TestAsString()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			AssertEquals(0, collection.Count);
			collection.AsString = "QWER,TYUI,ASDF";
			AssertEquals(3, collection.Count);
			collection.AddNew("GHJK");
			AssertEquals("QWER,TYUI,ASDF,GHJK", collection.AsString);
		}

		public void TestGetAllCodes()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			AssertEquals("When the CusCodeDataWithOrderCollection has no elements, GetAllCodes() Count", 0, collection.GetAllCodes().Count());

			collection.AddNew("1111");
			collection.AddNew("2222");
			collection.AddNew("");
			collection.AddNew("2222");
			collection.AddNew(" ");
			collection.AddNew("3333");
			AssertContainsExactElementsInAnyOrder("When the CusCodeDataWithOrderCollection has elements, GetAllCodes()", new string[] { "1111", "2222", "2222", "3333" }, collection.GetAllCodes());
		}

		public void TestDefaultOfCY_Order()
		{
			var collection = (CusCodeDataWithOrderCollectionForTest)GetCusCodeDataCollection();
			AssertEquals((short)1, collection.AddNew().CY_Order);
			AssertEquals((short)2, collection.AddNew().CY_Order);
			AssertEquals((short)3, collection.AddNew().CY_Order);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusCodeDataWithOrderForTest>();
			result.CY_Order = 3;
			return result;
		}
	}

	public class CusCodeDataWithOrderCollectionForTest : CusCodeDataWithOrderCollection<CusCodeDataWithOrderForTest>
	{
		public CusCodeDataWithOrderCollectionForTest(ZPropertyInfo info, short size, short startOrder = 1) : base(info, "TES", size, startOrder)
		{
		}
	}
}
