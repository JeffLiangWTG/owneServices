using CargoWise.EntityFramework.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportConfirmationCollectionTest<TBusinessObject, TCollection> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TBusinessObject : DtbTransportConfirmation
			where TCollection : DtbTransportConfirmationCollection<TBusinessObject>
	{
		#region TestAddNew

		public void TestAddNewWithConfirmationType()
		{
			var collection = GetCollectionToTest();

			AssertEquals("", collection.AddNew("").KK_ConfirmationType);
			AssertEquals(ConfirmationTypes.Codes.PickUp, collection.AddNew(ConfirmationTypes.Codes.PickUp).KK_ConfirmationType);
			AssertEquals(ConfirmationTypes.Codes.Delivery, collection.AddNew(ConfirmationTypes.Codes.Delivery).KK_ConfirmationType);
		}

		#endregion

		#region TestHasDelivery

		public void TestHasDelivery()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.HasDelivery);

			collection.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals(false, collection.HasDelivery);

			collection.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(true, collection.HasDelivery);
		}

		#endregion

		#region TestHasPickup

		public void TestHasPickup()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.HasPickup);

			collection.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(false, collection.HasPickup);

			collection.AddNew(ConfirmationTypes.Codes.PickUp);
			AssertEquals(true, collection.HasPickup);
		}

		#endregion
	}
}
