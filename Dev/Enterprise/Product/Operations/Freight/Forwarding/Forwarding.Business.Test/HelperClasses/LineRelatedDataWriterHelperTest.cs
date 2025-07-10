using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class LineRelatedDataWriterHelperTest : TestCaseWithFactory
	{
		public void TestLineRelatedDataWriterHelper_Shipment_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var packLine = shipment.OuterPackLines.AddNew();
				Factory.Save();
				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);

				var (order, orderLine, booking, bookingLine, loadListHeader, loadListLine, _, container) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory);
				loadListLine.CLL_JL_PackLine = packLine.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);

				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking.JSB_BookingId, bookingLineKey: bookingLine.JSL_BookingLineId);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);

				loadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking.JSB_BookingId, bookingLineKey: bookingLine.JSL_BookingLineId);
				AssertNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection);

				booking.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
				AssertNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection);

				order.JD_OrderStatus = Constants.OrderStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);
			});
		}

		public void TestLineRelatedDataWriterHelper_SupplierBooking_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var order = Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				Factory.Save();
				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(booking);
				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, bookingLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container = consol.Containers.AddNew();
				container.FillWithValidTestData();

				var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = booking.PK;
				var loadListLine = loadListHeader.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(booking);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, bookingLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(2, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 1, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);
				AssertNull(dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].RelatedEntityCollection);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var packLine = shipment.OuterPackLines.AddNew();
				loadListLine.CLL_JL_PackLine = packLine.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(booking);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, bookingLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(2, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 1, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);
				AssertNotNull(dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[1].RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, packingLineKey: packLine.JL_PackLineId, forwardingShipmentKey: shipment.JS_UniqueConsignRef);

				loadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(booking);
				dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, bookingLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 1, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
			});
		}

		public void TestLineRelatedDataWriterHelper_Order_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var order = Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();
				Factory.Save();
				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalOrderLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);

				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking.JSB_BookingId, bookingLineKey: bookingLine.JSL_BookingLineId);
				AssertNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container = consol.Containers.AddNew();
				container.FillWithValidTestData();

				var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = booking.PK;
				var loadListLine = loadListHeader.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);
				AssertNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var packLine = shipment.OuterPackLines.AddNew();
				loadListLine.CLL_JL_PackLine = packLine.PK;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNotNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);
				AssertNotNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection);
				AssertEquals(1, dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(dataObject.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, packingLineKey: packLine.JL_PackLineId, forwardingShipmentKey: shipment.JS_UniqueConsignRef);

				loadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				dataObject = new UniversalOrderLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection[0].RelatedEntityCollection);

				booking.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
				Factory.Save();
				lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				dataObject = new UniversalOrderLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);
			});
		}

		public void TestLineRelatedDataWriterHelper_Shipment_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var packLine = shipment.OuterPackLines.AddNew();
				var (_, _, _, _, _, loadListLine, _, _) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory);
				loadListLine.CLL_JL_PackLine = packLine.PK;
				Factory.Save();

				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(shipment);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, packLine, dataObject);

				AssertNull(dataObject.RelatedEntityCollection);
			});
		}

		public void TestLineRelatedDataWriterHelper_SupplierBooking_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var order = Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container = consol.Containers.AddNew();
				container.FillWithValidTestData();
				var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = booking.PK;
				var loadListLine = loadListHeader.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container.PK;
				Factory.Save();

				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(booking);
				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalPackingLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, bookingLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);
			});
		}

		public void TestLineRelatedDataWriterHelper_Order_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var order = Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				Factory.Save();

				var lineRelatedDataWriterHelper = new LineRelatedDataWriterHelper(order);
				var dataObjectWriterStrategy = DefaultDataObjectWriterStrategy.TestInstance;
				var dataObject = new UniversalOrderLine(dataObjectWriterStrategy);
				lineRelatedDataWriterHelper.SetRelatedEntityCollection(dataObjectWriterStrategy, orderLine, dataObject);
				AssertNull(dataObject.RelatedEntityCollection);
			});
		}
	}
}
