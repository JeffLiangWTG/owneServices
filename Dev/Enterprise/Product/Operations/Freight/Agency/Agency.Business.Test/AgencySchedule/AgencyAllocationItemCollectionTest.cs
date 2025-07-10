using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyAllocationItemCollectionTest<T1, T2, T3> : NonPersistentBusinessObjectCollectionTestCase<T1> where T1 : AgencyAllocationItemCollection<T2, T3> where T2 : AgencyAllocationItem<T3> where T3 : BusinessObject
	{
		public void TestCollectionsRemainInSync()
		{
			BusinessObjectCollection innerCollection = GetCollectToWrap();
			AssertEquals("InnerCollection should be empty", 0, innerCollection.Count);
			BusinessObject dummy0 = GetNewInnerElement();
			innerCollection.Add(dummy0);
			T1 outerCollection = WrapCollection(innerCollection);
			outerCollection.Load();
			AssertEquals("InnerCollection should have a single element", 1, innerCollection.Count);
			AssertEquals("OuterCollection should have a single element", 1, outerCollection.Count);
			AssertSame("The element in the outer collection should wrap the element in the inner collection", dummy0, outerCollection[0].BizObj);
			BusinessObject dummy1 = GetNewInnerElement();
			innerCollection.Add(dummy1);
			AssertEquals("InnerCollection should have 2 elements.", 2, innerCollection.Count);
			AssertEquals("OuterCollection should have 2 elements.", 2, outerCollection.Count);
			AssertSame("The second element in the outer collection should wrap the second element in the inner collection", dummy1, outerCollection[1].BizObj);
			innerCollection.Remove(dummy0);
			AssertEquals("InnerCollection should have an element.", 1, innerCollection.Count);
			AssertEquals("OuterCollection should have an element.", 1, outerCollection.Count);
			AssertSame("The element in the outer collection should wrap the element in the inner collection", dummy1, outerCollection[0].BizObj);
		}

		#region Implementation
		protected abstract T3 GetNewInnerElement();
		protected abstract BusinessObjectCollection GetCollectToWrap();
		protected abstract T1 WrapCollection(BusinessObjectCollection collectionToWrap);
		protected override T1 GetCollectionToTest()
		{
			return WrapCollection(GetCollectToWrap());
		}
		#endregion
	}
}
