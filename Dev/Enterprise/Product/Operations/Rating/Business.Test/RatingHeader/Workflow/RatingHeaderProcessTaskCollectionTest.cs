using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Rating.Business
{
	public abstract class RatingHeaderProcessTaskCollectionTest<T, P> : ProcessTaskCollectionTest<P>
		where T : RatingHeader
		where P : ProcessTaskCollection
	{
		public void TestParent()
		{
			AssertEquals(typeof(T), Collection.Parent.GetType());
		}

		#region Implementation

		protected T RatingHeader
		{
			get { return raringHeader ?? (raringHeader = Factory.NewWithValidTestData<T>()); }
		}
		T raringHeader;

		#endregion
	}
}
