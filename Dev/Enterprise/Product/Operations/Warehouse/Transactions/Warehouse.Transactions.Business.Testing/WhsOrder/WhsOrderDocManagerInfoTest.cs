using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderDocManagerInfo))]
	class WhsOrderDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = order.WD_DocketID;

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = order.WD_DocketType;
			pivot.WV_WD_Docket = order.PK;

			var orderDocManagerInfo = new WhsOrderDocManagerInfo(order, ((IDocManagerSupport)order).DocManagerInfo.DocManagerCode);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { cartageJob, shipment }, orderDocManagerInfo.RelatedObjects);
		}

		public void TestGetRelatedObjects_ShouldIncludeRelatedBookingAndTransportConsignmentAndActions()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var bookingHelper = new TransportBookingSharedTestHelper(Factory);
			var consolidation = bookingHelper.CreateConsolidation(order);
			var booking = bookingHelper.CreateBooking(consolidation);

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_KM_Booking = booking.PK;
			var landTransportConsignmentBusinessObject = (BusinessObject)landTransportConsignment;
			landTransportConsignmentBusinessObject.FillWithValidTestData();

			var addressQuery = new ZQuery(DtbConsignmentAddressSchema.LTS_LTC_Consignment, landTransportConsignment.PK);
			var landTransportAddress = Factory.Load<IDtbConsignmentAddress>(addressQuery)[0];

			var landTransportAction = Factory.New<IDtbConsignmentAction>();
			landTransportAction.LTA_LTS_ConsignmentAddress = landTransportAddress.PK;
			var landTransportActionBusinessObject = (BusinessObject)landTransportAction;
			landTransportActionBusinessObject.FillWithValidTestData();

			Factory.Save();

			var relatedObjects = order.DocManagerInfo.RelatedObjects;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				(BusinessObject)consolidation,
				(BusinessObject)booking,
				landTransportConsignmentBusinessObject,
				landTransportActionBusinessObject,
			}, relatedObjects);
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsOrder>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var order = Factory.New<WhsOrder>();
			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = order.WD_DocketID;

			return order;
		}

		#endregion
	}
}
