using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCollection))]
	sealed class ForwardingShipmentBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingShipmentCollection(Factory);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(ForwardingShipmentCollection), GetCollectionToTest().GetType());
		}

		public void TestAdditionalFilter()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Load();

			AssertEquals(1, collection.Count);
			AssertEquals(forwardingShipment.PK, collection.First().PK);
		}

		public void TestBizObjsFromCodeWithRelationshipFilter()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = false;

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var collection = new ForwardingShipmentCollection(Factory);
			var findBoxListProvider = new ForwardingShipmentFindBoxListProvider(collection);

			AssertEquals(ZGuid.Invalid, findBoxListProvider.PrimaryKeyFromCode(shipment.JS_UniqueConsignRef));
			AssertEquals(forwardingShipment.PK, findBoxListProvider.PrimaryKeyFromCode(forwardingShipment.JS_UniqueConsignRef));
		}
	}
}
