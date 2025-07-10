using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ForwardingPackLineWithPortMessagingCollection))]
	sealed class ForwardingPackLineWithPortMessagingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest() as ForwardingPackLineWithPortMessagingCollection;
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest() as ForwardingPackLineWithPortMessagingCollection;
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestCollectionIsReloadingOnShipmentPacklinesCountChanged()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			var collection = new ForwardingPackLineWithPortMessagingCollection(shipment);
			collection.Load();
			AssertEquals(2, collection.Count);

			shipment.OuterPackLines.AddNew();
			AssertEquals(3, collection.Count);

			shipment.OuterPackLines.RemoveAndDelete(shipment.OuterPackLines[0]);
			AssertEquals(2, collection.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new ForwardingPackLineWithPortMessagingCollection(shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var packLine = Factory.New<ForwardingPackLineWithPortMessaging>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;

			return packLine;
		}

		#endregion
	}
}
