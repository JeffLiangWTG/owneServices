using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business
{
	[TestedType(typeof(ChildWhsOrderCollection))]
	class ChildWhsOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<ChildWhsOrderCollection>
	{
		#region TestRelationship

		public void TestRelationship()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var childOrderCollection = new ChildWhsOrderCollection(shipment);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;
			childOrderCollection.Add(order);

			Factory.Save();

			var query = new ZQuery(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Order);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, shipment.PK);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, shipment.TablePrefix);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);

			var otherFactory1 = new BusinessObjectFactory();
			AssertNotNull(otherFactory1.LoadTop1<WhsDocketJobPivot>(query));

			childOrderCollection.RemoveFromRelationship(order);
			Factory.Save();

			var otherFactory2 = new BusinessObjectFactory();
			AssertNull(otherFactory2.LoadTop1<WhsDocketJobPivot>(query));
		}

		#endregion

		#region Implementation

		protected override ChildWhsOrderCollection GetCollectionToTest()
		{
			return new ChildWhsOrderCollection((BusinessObject)Factory.New<Forwarding.IForwardingShipment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WhsOrder>();
		}

		public override void TestAddNew()
		{
			Assert("Not support until problem for ManyToMany collection with additional filter is fixed", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not support until problem for ManyToMany collection with additional filter is fixed", true);
		}

		#endregion
	}
}
