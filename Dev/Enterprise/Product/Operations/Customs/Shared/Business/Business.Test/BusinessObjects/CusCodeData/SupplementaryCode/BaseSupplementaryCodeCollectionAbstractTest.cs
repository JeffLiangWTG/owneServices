using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseSupplementaryCodeCollection<>))]
	[TestsSubclassesOf(typeof(BaseSupplementaryCodeCollection<>))]
	public abstract class BaseSupplementaryCodeCollectionAbstractTest<TSupplementaryCode> : CusCodeDataCollectionTest<TSupplementaryCode>
		where TSupplementaryCode : BaseSupplementaryCode
	{
		public void TestGetAllCodes()
		{
			var supplementaryCodeCollection = (BaseSupplementaryCodeCollection<TSupplementaryCode>)GetCusCodeDataCollection();
			AssertEquals("When the BaseSupplementaryCodeCollection has no elements, GetAllCodes() Count", 0, supplementaryCodeCollection.GetAllCodes().Count());

			supplementaryCodeCollection.AddNew("1111");
			supplementaryCodeCollection.AddNew("2222");
			supplementaryCodeCollection.AddNew("");
			supplementaryCodeCollection.AddNew("2222");
			supplementaryCodeCollection.AddNew(" ");
			supplementaryCodeCollection.AddNew("3333");
			AssertContainsExactElementsInAnyOrder("When the BaseSupplementaryCodeCollection has elements, GetAllCodes()", new string[] { "1111", "2222", "2222", "3333" }, supplementaryCodeCollection.GetAllCodes());
		}
	}
}
