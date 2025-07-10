using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(BookingConfirmationCollection))]
	sealed class BookingConfirmationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BookingConfirmationCollection>
	{
		public void TestAllowNew()
		{
			var collection = new BookingConfirmationCollection(Factory);
			AssertEquals(false, collection.AllowNew);
		}

		#region Implementation

		protected override BookingConfirmationCollection GetCollectionToTest()
		{
			return new BookingConfirmationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BookingConfirmation();
		}

		#endregion
	}
}
