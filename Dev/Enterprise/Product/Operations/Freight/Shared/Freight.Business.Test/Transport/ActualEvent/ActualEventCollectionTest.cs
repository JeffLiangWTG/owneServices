using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ActualEventCollection))]
	sealed class ActualEventCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ActualEventCollection>
	{
		public void TestAllowNew()
		{
			var collection = new ActualEventCollection(Factory);
			AssertEquals(false, collection.AllowNew);
		}

		#region Implementation

		protected override ActualEventCollection GetCollectionToTest()
		{
			return new ActualEventCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ActualEvent();
		}

		#endregion
	}
}
