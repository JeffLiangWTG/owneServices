using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingLineDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappings()
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);

			var writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, supplierBooking)));
			var dataObject = writer.GetDataObject(supplierBooking);

			AssertNotNull(dataObject);
			AssertEquals(1, dataObject.SubShipmentCollection?.Count);
			var universalSubShipment = dataObject.SubShipmentCollection[0];

			AssertEquals(supplierBooking.JSB_BookingId, dataObject.DataContext?.DataSourceCollection?.FirstOrDefault(source => source.Type.GetValueOrDefault() == nameof(DataContextType.JobSupplierBooking)).Key);

			var universalOrder = universalSubShipment.Order;
			var universalOrderLine = universalOrder?.OrderLineCollection?.FirstOrDefault();
			var universalPackLine = universalSubShipment.PackingLineCollection?.FirstOrDefault();

			AssertNotNull(universalOrder);
			AssertNotNull(universalOrderLine);
			AssertNotNull(universalPackLine);

			AssertNotNull(universalSubShipment.AddInfoCollection);
			var orderContextKey = universalSubShipment.AddInfoCollection.First();
			var product = universalOrderLine.Product;
			AssertNotNull(product);

			CombineAssertions(() =>
			{
				AssertEquals(7.1m, universalSubShipment.TotalNoOfPacksDecimal);
				AssertEquals("PLT", universalSubShipment.TotalNoOfPacksPackageType.Code);
				AssertEquals("Pallet", universalSubShipment.TotalNoOfPacksPackageType.Description);
				AssertEquals("ORD0001", universalOrder.OrderNumber);
				AssertEquals((ZByte)1, universalOrder.OrderNumberSplit);
				AssertEquals("OrderContextKey", orderContextKey.Key);
				AssertEquals("ORD0001~1~Buyer", orderContextKey.Value);
				AssertEquals((ZInt)1, universalOrderLine.LineNumber);
				AssertEquals((ZInt)2, universalOrderLine.SubLineNumber);
				AssertEquals("ME100770267", product.Code);
				AssertEquals("T Shirts", product.Description);
				AssertEquals("ORL001", universalOrderLine.LineReference);
				AssertEquals((ZDecimal)5.5732, universalOrderLine.UnitPriceRecommended);
				AssertEquals((ZDecimal)2507.9544, universalOrderLine.ExtendedLinePrice);
				AssertEquals(new ZDateTime(2022, 3, 27), universalOrderLine.RequiredExWorks);
				AssertEquals(new ZDateTime(2022, 4, 27), universalOrderLine.RequiredInStore);
				AssertEquals("JSL001", universalPackLine.PackingLineID);
				AssertEquals((ZLong)6, universalPackLine.PackQty);
				AssertEquals("PLT", universalPackLine.PackType.Code);
				AssertEquals(12m, universalPackLine.Weight);
				AssertEquals("KG", universalPackLine.WeightUnit.Code);
				AssertEquals(15m, universalPackLine.Volume);
				AssertEquals("M3", universalPackLine.VolumeUnit.Code);
				AssertEquals("Desc", universalPackLine.GoodsDescription);
				AssertEquals("Line Marks&Num", universalPackLine.MarksAndNos);
				AssertEquals("GEN", universalPackLine.Commodity.Code);
				AssertEquals("HC001", universalPackLine.HarmonisedCode);
				AssertEquals(6m, universalPackLine.ReceivedQuantity);
				AssertEquals("PLT", universalSubShipment.TotalNoOfPacksPackageType.Code);
				AssertEquals("Pallet", universalSubShipment.TotalNoOfPacksPackageType.Description);
				AssertEquals(5, universalPackLine.ReceivedPacks);
				AssertEquals("PLT", universalPackLine.PackType.Code);
				AssertEquals(11m, universalPackLine.ReceivedWeight);
				AssertEquals("KG", universalPackLine.WeightUnit.Code);
				AssertEquals(14m, universalPackLine.ReceivedVolume);
				AssertEquals("M3", universalPackLine.VolumeUnit.Code);
				AssertEquals(new ZDateTime(2023, 10, 01, 11, 0, 0), universalPackLine.FirstCFSReceiptDate);
				AssertEquals(new ZDateTime(2023, 10, 10, 11, 0, 0), universalPackLine.LastCFSReceiptDate);
				AssertEquals(new ZDate(2023, 1, 20), universalOrderLine.ShipmentWindowStart);
				AssertEquals(new ZDate(2023, 2, 20), universalOrderLine.ShipmentWindowEnd);
			});

			var buyerAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressType(universalSubShipment, DocAddressType.BuyerDocumentaryAddress);
			AssertNotNull(buyerAddress);
			AssertEquals("Buyer", buyerAddress.OrganizationCode);

			var manufacturerAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressType(universalSubShipment, DocAddressType.Manufacturer);
			AssertNotNull(manufacturerAddress);
			AssertEquals("MFAKL", manufacturerAddress.OrganizationCode);

			var shipmentWindowStart = universalSubShipment.DateCollection.Find(date => date.Type == UniversalDateType.ShipmentWindowStart);
			var shipmentWindowEnd = universalSubShipment.DateCollection.Find(date => date.Type == UniversalDateType.ShipmentWindowEnd);
			AssertEquals(new ZDateTime(2023, 9, 1), shipmentWindowStart.Value);
			AssertEquals(new ZDateTime(2023, 9, 3), shipmentWindowEnd.Value);
		}

		#region Related Entity Collection

		public void TestPackingLineRelatedEntityCollection_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var (_, orderLine, booking, bookingLine, loadListHeader, loadListLine1, consol, container) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var outerPackLine = shipment.OuterPackLines.AddNew();
				loadListLine1.CLL_JL_PackLine = outerPackLine.PK;
				loadListLine1.CLL_JSL_BookingLine = bookingLine.PK;
				outerPackLine.JL_JC = loadListLine1.CLL_JC_Container;

				var container2 = consol.Containers.AddNew();
				container2.FillWithValidTestData();
				container2.JC_JSB_SupplierBooking = booking.PK;
				var loadListLine2 = loadListHeader.LoadListLines.AddNew();
				loadListLine2.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine2.CLL_JC_Container = container2.PK;

				Factory.SaveForTesting();

				var writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
				var dataObject = writer.GetDataObject(booking);

				AssertNotNull(dataObject);
				AssertEquals(1, dataObject.SubShipmentCollection?.Count);
				var universalSubShipment = dataObject.SubShipmentCollection[0];

				AssertEquals(booking.JSB_BookingId, dataObject.DataContext?.DataSourceCollection?.FirstOrDefault(source => source.Type.GetValueOrDefault() == nameof(DataContextType.JobSupplierBooking)).Key);

				AssertEquals(1, universalSubShipment.PackingLineCollection.Count);

				var universalPackLine = universalSubShipment.PackingLineCollection.First(line => line.RelatedEntityCollection.Count == 2);
				AssertNotNull(universalPackLine);

				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(universalPackLine.RelatedEntityCollection[0].EntityKeyCollection, count: 1, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);

				var loadListLine1Data = universalPackLine.RelatedEntityCollection[1].RelatedEntityCollection.First(line => line.RelatedEntityCollection != null);
				var loadListLine2Data = universalPackLine.RelatedEntityCollection[1].RelatedEntityCollection.First(line => line.RelatedEntityCollection == null);
				AssertNotNull("Packed Container Load List Line exist", loadListLine1Data);
				AssertNotNull("Unpacked Container Load List Line exist", loadListLine2Data);

				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(loadListLine1Data.EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container.JC_ContainerNum);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(loadListLine1Data.RelatedEntityCollection[0].EntityKeyCollection, count: 2, packingLineKey: outerPackLine.JL_PackLineId, forwardingShipmentKey: shipment.JS_UniqueConsignRef);

				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(loadListLine2Data.EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader.CLH_LoadListId, containerNumber: container2.JC_ContainerNum);
				AssertNull(loadListLine2Data.RelatedEntityCollection);

				loadListLine1.Delete();
				loadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				Factory.SaveForTesting();
				writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
				dataObject = writer.GetDataObject(booking);
				AssertNotNull(dataObject);
				AssertEquals(1, dataObject.SubShipmentCollection?.Count);
				universalSubShipment = dataObject.SubShipmentCollection[0];
				AssertEquals(1, universalSubShipment.PackingLineCollection.Count);
				universalPackLine = universalSubShipment.PackingLineCollection[0];
				AssertNotNull(universalPackLine);
				AssertNotNull(universalPackLine.RelatedEntityCollection);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(universalPackLine.RelatedEntityCollection[0].EntityKeyCollection, count: 1, orderLineKey: orderLine.GetUniversalDataContextManager().DataContextKey);
			});
		}

		public void TestPackingLineRelatedEntityCollection_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var (order, orderLine, booking, bookingLine, loadListHeader, loadListLine1, consol, container) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var outerPackLine = shipment.OuterPackLines.AddNew();
				loadListLine1.CLL_JL_PackLine = outerPackLine.PK;
				loadListLine1.CLL_JSL_BookingLine = bookingLine.PK;
				outerPackLine.JL_JC = loadListLine1.CLL_JC_Container;

				var container2 = consol.Containers.AddNew();
				container2.FillWithValidTestData();
				container2.JC_JSB_SupplierBooking = booking.PK;
				var loadListLine2 = loadListHeader.LoadListLines.AddNew();
				loadListLine2.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine2.CLL_JC_Container = container2.PK;

				Factory.SaveForTesting();

				var writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
				var dataObject = writer.GetDataObject(booking);

				AssertNotNull(dataObject);
				AssertEquals(1, dataObject.SubShipmentCollection?.Count);
				var universalSubShipment = dataObject.SubShipmentCollection[0];

				AssertEquals(booking.JSB_BookingId, dataObject.DataContext?.DataSourceCollection?.FirstOrDefault(source => source.Type.GetValueOrDefault() == nameof(DataContextType.JobSupplierBooking)).Key);

				AssertEquals(1, universalSubShipment.PackingLineCollection.Count);
				AssertNull(universalSubShipment.PackingLineCollection[0].RelatedEntityCollection);
			});
		}

		#endregion

		#region Shipment Window Date

		public void TesShipmentWindowWithValidDate()
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			var writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, supplierBooking)));
			var dataObject = writer.GetDataObject(supplierBooking);

			var universalSubShipment = dataObject.SubShipmentCollection[0];
			var shipmentWindowStart = universalSubShipment.DateCollection.Find(date => date.Type == UniversalDateType.ShipmentWindowStart);
			var shipmentWindowEnd = universalSubShipment.DateCollection.Find(date => date.Type == UniversalDateType.ShipmentWindowEnd);
			AssertEquals(new ZDateTime(2023, 9, 1), shipmentWindowStart.Value);
			AssertEquals(new ZDateTime(2023, 9, 3), shipmentWindowEnd.Value);
		}

		public void TesShipmentWindowWithEmptyDate()
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			supplierBooking.SupplierBookingLines[0].JSL_ShipmentWindowStart = ZDate.Empty;
			supplierBooking.SupplierBookingLines[0].JSL_ShipmentWindowEnd = ZDate.Empty;
			var writer = new JobSupplierBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, supplierBooking)));
			var dataObject = writer.GetDataObject(supplierBooking);

			var universalSubShipment = dataObject.SubShipmentCollection[0];
			var shipmentWindowStart = universalSubShipment.DateCollection.FirstOrDefault(date => date.Type == UniversalDateType.ShipmentWindowStart);
			var shipmentWindowEnd = universalSubShipment.DateCollection.FirstOrDefault(date => date.Type == UniversalDateType.ShipmentWindowEnd);
			AssertNull(shipmentWindowStart);
			AssertNull(shipmentWindowEnd);
		}

		#endregion
	}
}
