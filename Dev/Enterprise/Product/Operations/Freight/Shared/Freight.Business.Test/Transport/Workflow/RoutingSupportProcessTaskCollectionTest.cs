using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class RoutingSupportProcessTaskCollectionTest<T> : ProcessTaskCollectionTest<T> where T : RoutingSupportProcessTaskCollection
	{
		public void TestSupportsReferenceCode()
		{
			AssertEquals(true, Collection.RequiresReferenceCode);
		}

		public void TestReferenceCodeCaption()
		{
			AssertEquals("Leg", Collection.ReferenceCodeCaption);
		}
	}
}
