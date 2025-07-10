using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderLineDataObjectWriterTest : OrganizationAddressTestHelper
	{
		#region TestBasicOrderLineLevelFieldMappings

		public void TestBasicOrderLineLevelFieldMappings()
		{
			var orderLine = GetOrderLine(Factory.BOFactory);
			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLine.Order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderLineData = writer.GetDataObject(orderLine);

			CombineAssertions(delegate { AssertContents(orderLineData); });
		}

		internal static OrderLine GetOrderLine(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();
			var order = factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 2;
			orderLine.JO_LineReference = "Line Reference";
			orderLine.JO_Partno = "Guitars";
			orderLine.JO_Description = "Electric Guitars";
			orderLine.JO_InnerPacks = 12.3m;
			orderLine.JO_InnerPacksUQ = "BOX";
			orderLine.JO_OuterPacks = 11.2m;
			orderLine.JO_OuterPacksUQ = "PLT";
			orderLine.JO_OuterPackLength = 20m;
			orderLine.JO_OuterPackHeight = 30m;
			orderLine.JO_OuterPackWidth = 20m;
			orderLine.JO_OuterPackUnitOfDimension = "CM";
			orderLine.JO_Quantity = 14.4m;
			orderLine.JO_F3_NKPackType = "KEG";
			orderLine.JO_LinePrice = 132.48m;
			orderLine.JO_ItemPrice = 9.2m;
			orderLine.JO_CommercialInvoiceNo = "COM1";
			orderLine.JO_RN_NKCountryOfOrigin = "NZ";
			orderLine.JO_QtyInvoiced = 5.5m;
			orderLine.JO_QtyReceived = 6.6m;
			orderLine.JO_PartAttrib1 = "Red";
			orderLine.JO_PartAttrib2 = "Medium";
			orderLine.JO_PartAttrib3 = "Great";
			orderLine.JO_SerialNumber = "SSNN";
			orderLine.JO_UnitOfWeight = "LB";
			orderLine.JO_ActualWeight = 18.1m;
			orderLine.JO_AdditionalInformation = "Additional Stuff";
			orderLine.JO_AdditionalTerms = "No terms";
			orderLine.JO_ConfirmationDate = new ZDateTime(2013, 1, 1);
			orderLine.JO_ConfirmationNum = "CONF123";
			orderLine.JO_ContainerNumber = "ABC1";
			orderLine.JO_ContainerPackingOrder = 4;
			orderLine.JO_ExWorksDate = new ZDateTime(2013, 1, 2);
			orderLine.JO_INCO = "FOB";
			orderLine.JO_LineSplitNumber = new ZShort(3);
			orderLine.JO_LineStatus = "PLC";
			orderLine.JO_LineDropDate = new ZDateTime(2013, 1, 3); // Required In Store Date
			orderLine.JO_SpecialInstructions = "Do Something Special";
			orderLine.JO_SubLineNo = 5;
			orderLine.JO_UnitOfVolume = "CF";
			orderLine.JO_ActualVolume = 4.7m;
			orderLine.JO_LateShipmentLimitDays = 1;
			orderLine.JO_EarlyShipmentLimitDays = 4;
			orderLine.JO_OverQuantityPercentageLimit = 10.5;
			orderLine.JO_UnderQuantityPercentageLimit = 20.5;
			orderLine.JO_QtyPacked = 6;
			orderLine.JO_OpenQuantity = 8;
			orderLine.JO_ShipmentWindowStart = new ZDate(2012, 1, 5);
			orderLine.JO_ShipmentWindowEnd = new ZDate(2012, 1, 5);
			orderLine.JO_HSCode = "HS008";
			orderLine.JO_RH_NKCommodityCode = "CC01";

			return orderLine;
		}

		internal static void AssertContents(UniversalOrderLine orderLineData)
		{
			var customsData = orderLineData.CustomsData;
			AssertEquals("customsData.CountryOfOrigin.Code", "NZ", customsData.CountryOfOrigin.Code);
			AssertEquals("customsData.CountryOfOrigin.Name", "New Zealand", customsData.CountryOfOrigin.Name);

			AssertEquals("orderLineData.AdditionalInformation", "Additional Stuff", orderLineData.AdditionalInformation);
			AssertEquals("orderLineData.ExtendedLinePrice", 132.48m, orderLineData.ExtendedLinePrice);
			AssertEquals("orderLineData.ExpectedQuantity", 5.5m, orderLineData.ExpectedQuantity);
			AssertEquals("orderLineData.LineNumber", 2, orderLineData.LineNumber);
			AssertEquals("orderLineData.LineReference", "Line Reference", orderLineData.LineReference);
			AssertEquals("orderLineData.OrderedQty", 14.4m, orderLineData.OrderedQty);
			AssertEquals("orderLineData.OrderedQtyUnit.Code", "KEG", orderLineData.OrderedQtyUnit.Code);
			AssertEquals("orderLineData.OrderedQtyUnit.Description", "Keg", orderLineData.OrderedQtyUnit.Description);
			AssertEquals("orderLineData.PackageQty", 11.2m, orderLineData.PackageQty);
			AssertEquals("orderLineData.PackageQtyUnit.Code", "PLT", orderLineData.PackageQtyUnit.Code);
			AssertEquals("orderLineData.PackageDimensions.Length", 20m, orderLineData.PackageLength);
			AssertEquals("orderLineData.PackageDimensions.Height", 30m, orderLineData.PackageHeight);
			AssertEquals("orderLineData.PackageDimensions.Width", 20m, orderLineData.PackageWidth);
			AssertEquals("orderLineData.PackageDimensions.Unit", "CM", orderLineData.PackageLengthUnit.Code);
			AssertEquals("orderLineData.PackageQtyUnit.Description", "Pallet", orderLineData.PackageQtyUnit.Description);
			AssertEquals("orderLineData.PartAttribute1", "Red", orderLineData.PartAttribute1);
			AssertEquals("orderLineData.PartAttribute2", "Medium", orderLineData.PartAttribute2);
			AssertEquals("orderLineData.PartAttribute3", "Great", orderLineData.PartAttribute3);
			AssertEquals("orderLineData.SerialNumber", "SSNN", orderLineData.SerialNumber);
			AssertEquals("orderLineData.Product.Code", "Guitars", orderLineData.Product.Code);
			AssertEquals("orderLineData.Product.Description", "Electric Guitars", orderLineData.Product.Description);
			AssertEquals("orderLineData.QuantityMet", 6.6m, orderLineData.QuantityMet);
			AssertEquals("orderLineData.Status.Code", "PLC", orderLineData.Status.Code);
			AssertEquals("orderLineData.Status.Description", "Placed", orderLineData.Status.Description);
			AssertEquals("orderLineData.SubLineNumber", 5, orderLineData.SubLineNumber);
			AssertEquals("orderLineData.UnitPriceRecommended", 9.2m, orderLineData.UnitPriceRecommended);
			AssertEquals("orderLineData.InnerPacksQty", 12.3m, orderLineData.InnerPacksQty);
			AssertEquals("orderLineData.InnerPacksQtyUnit.Code", "BOX", orderLineData.InnerPacksQtyUnit.Code);
			AssertEquals("orderLineData.InnerPacksQtyUnit.Description", "Box", orderLineData.InnerPacksQtyUnit.Description);
			AssertEquals("orderLineData.CommercialInvoiceNumber", "COM1", orderLineData.CommercialInvoiceNumber);
			AssertEquals("orderLineData.Weight", 18.1m, orderLineData.Weight);
			AssertEquals("orderLineData.WeightUnit.Code", "LB", orderLineData.WeightUnit.Code);
			AssertEquals("orderLineData.WeightUnit.Description", "Pounds", orderLineData.WeightUnit.Description);
			AssertEquals("orderLineData.Volume", 4.7m, orderLineData.Volume);
			AssertEquals("orderLineData.VolumeUnit.Code", "CF", orderLineData.VolumeUnit.Code);
			AssertEquals("orderLineData.VolumeUnit.Description", "Cubic Feet", orderLineData.VolumeUnit.Description);
			AssertEquals("orderLineData.SupplierConfirmedAcceptance", new ZDateTime(2013, 1, 1), orderLineData.SupplierConfirmedAcceptance);
			AssertEquals("orderLineData.ConfirmationNumber", "CONF123", orderLineData.ConfirmationNumber);
			AssertEquals("orderLineData.ContainerNumber", "ABC1", orderLineData.ContainerNumber);
			AssertEquals("orderLineData.ContainerPackingOrder", 4, orderLineData.ContainerPackingOrder);
			AssertEquals("orderLineData.RequiredExWorks", new ZDateTime(2013, 1, 2), orderLineData.RequiredExWorks);
			AssertEquals("orderLineData.IncoTerm.Code", "FOB", orderLineData.IncoTerm.Code);
			AssertEquals("orderLineData.IncoTerm.Description", "Free On Board", orderLineData.IncoTerm.Description);
			AssertEquals("orderLineData.LineSplitNumber", new ZShort(3), orderLineData.LineSplitNumber);
			AssertEquals("orderLineData.RequiredInStore", new ZDateTime(2013, 1, 3), orderLineData.RequiredInStore);
			AssertEquals("orderLineData.SpecialInstructions", "Do Something Special", orderLineData.SpecialInstructions);
			AssertEquals("orderLineData.LateShipmentLimitDays", new ZByte(1), orderLineData.LateShipmentLimitDays);
			AssertEquals("orderLineData.EarlyShipmentLimitDays", new ZByte(4), orderLineData.EarlyShipmentLimitDays);
			AssertEquals("orderLineData.OverQuantityPercentageLimit", 10.5m, orderLineData.OverQuantityPercentageLimit);
			AssertEquals("orderLineData.UnderQuantityPercentageLimit", 20.5m, orderLineData.UnderQuantityPercentageLimit);
			AssertEquals("orderLineData.QtyPacked", 6m, orderLineData.QtyPacked);
			AssertEquals("orderLineData.QtyBooked", 6.4m, orderLineData.QtyBooked);
			AssertEquals("orderLineData.ShipmentWindowStart", new ZDate(2012, 1, 5), orderLineData.ShipmentWindowStart);
			AssertEquals("orderLineData.ShipmentWindowEnd", new ZDate(2012, 1, 5), orderLineData.ShipmentWindowEnd);
			AssertEquals("orderLineData.HarmonisedCode", "HS008", orderLineData.HarmonisedCode);
			AssertEquals("orderLineData.Commodity.Code", "CC01", orderLineData.Commodity.Code);
		}

		#endregion

		#region TestOrderNumberDeliveryPointAndRequiredByAreExportedIfDeliveryPointIsSetOnOrderLine

		public void TestOrderNumberDeliveryPointAndRequiredByAreExportedIfDeliveryPointIsSetOnOrderLine()
		{
			var buyer = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORDER123";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineDropDate = new ZDateTime(2013, 1, 3); // Required In Store Date

			var writer1 = new OrderLineDataObjectWriter(new DataWritingManager(new DummyActionInfo()), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderLineData1 = writer1.GetDataObject(orderLine);
			AssertEquals(null, orderLineData1.Consignee);
			AssertEquals(null, orderLineData1.RequiredBy);
			AssertEquals(null, orderLineData1.CrossDockOrderNumber);

			var delivery = orderLine.Deliveries.AddNew();
			delivery.J4_OA_NKDeliveryPoint = buyer.MainAddress.OA_Code;

			var writer2 = new OrderLineDataObjectWriter(new DataWritingManager(new DummyActionInfo()), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderLineData2 = writer2.GetDataObject(orderLine);

			OrganizationAddressTestHelper.AssertOrganizationBO_CRAHOLSYD("ConsigneeAddress", orderLineData2.Consignee, "ConsigneeAddress");
			AssertEquals(new ZDateTimeOffset(2013, 1, 3), orderLineData2.RequiredBy);
			AssertEquals("ORDER123", orderLineData2.CrossDockOrderNumber);
		}

		#endregion

		#region TestCustomFields

		public void TestCustomFields()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			var orderLine = order.OrderLines.AddNew();
			order.AddCustomLabel(Core.Constants.CustomLabels.OrderLine.CustomAttribute1, "What Makes You Happy?");
			orderLine.JO_CustomAttrib1 = "Lots Of Ice";
			order.AddCustomLabel(Core.Constants.CustomLabels.OrderLine.CustomDate1, "The Date You Are Happy");
			orderLine.JO_CustomDate1 = ZDateTime.BrettsBirthday;
			order.AddCustomLabel(Core.Constants.CustomLabels.OrderLine.CustomDecimal1, "The Happy Decimal");
			orderLine.JO_CustomDecimal1 = 7.7m;
			order.AddCustomLabel(Core.Constants.CustomLabels.OrderLine.CustomFlag1, "Are you Happy?");
			orderLine.JO_CustomFlag1 = true;
			order.AddCustomLabel(Core.Constants.CustomLabels.OrderLine.CustomText1, "Some Blob of Happiness");
			orderLine.JO_CustomTextBlob1 = "BLOBO";

			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderData = writer.GetDataObject(orderLine);
			AssertNotNull("Precondition: orderData", orderData);

			var customFields = orderData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.String, "Some Blob of Happiness", "BLOBO");
			});
		}

		public void TestWorkflowCustomFields()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.SetUserDefinedValue("Are you Happy?", ZBool.True);
			orderLine.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			orderLine.SetUserDefinedValue("The Happy Number", new ZInt(42));
			orderLine.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			orderLine.SetUserDefinedValue("The Date You Are Happy", new ZDateTime(2021, 4, 15));

			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderData = writer.GetDataObject(orderLine);
			AssertNotNull("Precondition: orderData", orderData);

			var customFields = orderData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", new ZDateTime(2021, 4, 15).ToISO8601String());
			});
		}

		#endregion

		#region TestDangerousGoods

		public void TestDangerousGoods()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var contact = buyer.Contacts.AddNew();
			contact.OC_ContactName = "JohnSmith";
			contact.OC_Phone = "0298983232";

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			var actualSubs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0004", "A", "IMO").First();
			orderLine.UNDGs.UNDGSubstanceManagerGuid.Value = actualSubs.PK;
			orderLine.UNDGs.UNDGIsCombustibleManager.Value = "true";
			orderLine.UNDGs.UNDGFlashPointManager.Value = "-90.1";
			orderLine.UNDGs.UNDGContactManager.Value = contact.PK;

			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderLineData = writer.GetDataObject(orderLine);

			AssertNotNull(orderLineData);
			AssertNotNull(orderLineData.UNDGCollection);
			AssertEquals(1, orderLineData.UNDGCollection.Count);

			var undgItem = orderLineData.UNDGCollection[0];
			AssertEquals("undgItem.Contact.FullName", "JohnSmith", undgItem.Contact.FullName);
			AssertEquals("undgItem.Contact.Phone", "0298983232", undgItem.Contact.Phone);
			AssertEquals("undgItem.FlashPoint", "-90.1", undgItem.FlashPoint);
			AssertEquals("undgItem.UNDGCode", "0004a", undgItem.UNDGCode);
		}

		public void TestDangerousGoodsWithGuid()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var contact = buyer.Contacts.AddNew();
			contact.OC_ContactName = "JohnSmith";
			contact.OC_Phone = "0298983232";

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var actualSubs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0004", "a", "IMO").First();

			var orderLine = order.OrderLines.AddNew();
			orderLine.UNDGs.UNDGSubstanceManagerGuid.Value = actualSubs.PK;
			orderLine.UNDGs.UNDGIsCombustibleManager.Value = "true";
			orderLine.UNDGs.UNDGFlashPointManager.Value = "-90.1";
			orderLine.UNDGs.UNDGContactManager.Value = contact.PK;

			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLine.Order));
			var orderLineData = writer.GetDataObject(orderLine);

			AssertNotNull(orderLineData);
			AssertNotNull(orderLineData.UNDGCollection);
			AssertEquals(1, orderLineData.UNDGCollection.Count);

			var undgItem = orderLineData.UNDGCollection[0];
			AssertEquals("undgItem.Contact.FullName", "JohnSmith", undgItem.Contact.FullName);
			AssertEquals("undgItem.Contact.Phone", "0298983232", undgItem.Contact.Phone);
			AssertEquals("undgItem.FlashPoint", "-90.1", undgItem.FlashPoint);
			AssertEquals("undgItem.UNDGCode", "0004a", undgItem.UNDGCode);
		}

		#endregion

		#region TestManufacturer

		public void TestManufacturer()
		{
			TestOrganizationAddress(DocAddressType.Manufacturer, DocAddressType.Manufacturer);
		}

		public void TestNoOrganisations()
		{
			var orderBo = Factory.New<Order>();
			var orderLineBO = orderBo.OrderLines.AddNew();
			var orderLineData = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLineBO)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLineBO.Order)).GetDataObject(orderLineBO);
			AssertNotNull("orderLineData", orderLineData);
			AssertNull("orderLineData.OrganizationAddressCollection", orderLineData.OrganizationAddressCollection);
		}

		#endregion

		#region TestGoodsAvailableAt

		public void TestGoodsAvailableAt()
		{
			TestOrganizationAddress(DocAddressType.GoodsAvailableAt, DocAddressType.ConsignorPickupDeliveryAddress);
		}

		#endregion

		#region TestGoodsDeliveredTo

		public void TestGoodsDeliveredTo()
		{
			TestOrganizationAddress(DocAddressType.GoodsDeliveredTo, DocAddressType.ConsigneePickupDeliveryAddress);
		}

		#endregion

		#region TestConsigneeDocumentary

		public void TestConsigneeDocumentary()
		{
			TestOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress);
		}

		#endregion

		void TestOrganizationAddress(DocAddressType docAddressType, DocAddressType universalAddressType)
		{
			var orderBo = Factory.New<Order>();
			var orderLineBO = orderBo.OrderLines.AddNew();
			var addressBO = Factory.New<JobDocAddress>();
			addressBO.E2_AddressType = DocAddressTypes.GetCode(Factory.BOFactory, docAddressType);
			addressBO.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderLineBO.DocAddresses.Add(addressBO);

			var orderLineData = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLineBO)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(orderLineBO.Order)).GetDataObject(orderLineBO);
			AssertNotNull("orderLineData", orderLineData);
			AssertEquals("orderLineData.OrganizationAddressCollection.Count", 1, orderLineData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB(universalAddressType.ToString(), orderLineData.OrganizationAddressCollection[0], universalAddressType.ToString());
		}

		#region Related Entity Collection

		public void TestOrderLineRelatedEntityCollection_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var order = Factory.New<Order>();
				var buyer = Factory.NewWithValidTestData<OrgHeader>();
				order.BuyerPK = buyer.PK;
				var orderLine1 = order.OrderLines.AddNew();
				var orderLine2 = order.OrderLines.AddNew();
				var orderLine3 = order.OrderLines.AddNew();
				var orderLine4 = order.OrderLines.AddNew();
				var orderLine5 = order.OrderLines.AddNew();
				var orderLineBookingLineMap = new Dictionary<OrderLine, List<BookingLineForOrderLine>>();
				var bookingLineLoadListLineMap = new Dictionary<JobSupplierBookingLine, List<PackingLineForSupplierBookingLine>>();
				UpdateOrderLinesRelatedData(orderLineBookingLineMap, bookingLineLoadListLineMap, orderLine1, orderLine2, orderLine3, orderLine4);
				Factory.SaveForTesting();

				var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(order));
				var orderLineData1 = writer.GetDataObject(orderLine1);
				var orderLineData2 = writer.GetDataObject(orderLine2);
				var orderLineData3 = writer.GetDataObject(orderLine3);
				var orderLineData4 = writer.GetDataObject(orderLine4);
				var orderLineData5 = writer.GetDataObject(orderLine5);

				AssertEquals(3, orderLineData1.RelatedEntityCollection.Count);
				orderLineData1.RelatedEntityCollection.ForEach(collection => CheckBookingLineRelatedEntityCollection(collection, orderLineBookingLineMap[orderLine1], bookingLineLoadListLineMap, out _));

				AssertEquals(3, orderLineData2.RelatedEntityCollection.Count);
				orderLineData2.RelatedEntityCollection.ForEach(collection => CheckBookingLineRelatedEntityCollection(collection, orderLineBookingLineMap[orderLine2], bookingLineLoadListLineMap, out _));

				AssertEquals(1, orderLineData3.RelatedEntityCollection.Count);
				CheckBookingLineRelatedEntityCollection(orderLineData3.RelatedEntityCollection[0], orderLineBookingLineMap[orderLine3], bookingLineLoadListLineMap, out _);

				AssertEquals(1, orderLineData4.RelatedEntityCollection.Count);
				CheckBookingLineRelatedEntityCollection(orderLineData4.RelatedEntityCollection[0], orderLineBookingLineMap[orderLine4], bookingLineLoadListLineMap, out var relatedBookingLineForOrderLine);

				AssertNull(orderLineData5.RelatedEntityCollection);

				TestOrderLineEntityKeyCollection_Cancelled(order, orderLine4, bookingLineLoadListLineMap, relatedBookingLineForOrderLine);
			});
		}

		public void TestOrderLineRelatedEntityCollection_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var order = Factory.New<Order>();
				var buyer = Factory.NewWithValidTestData<OrgHeader>();
				order.BuyerPK = buyer.PK;
				var orderLine1 = order.OrderLines.AddNew();
				var orderLine2 = order.OrderLines.AddNew();
				var orderLine3 = order.OrderLines.AddNew();
				var orderLine4 = order.OrderLines.AddNew();
				var orderLine5 = order.OrderLines.AddNew();
				var orderLineBookingLineMap = new Dictionary<OrderLine, List<BookingLineForOrderLine>>();
				var bookingLineLoadListLineMap = new Dictionary<JobSupplierBookingLine, List<PackingLineForSupplierBookingLine>>();
				UpdateOrderLinesRelatedData(orderLineBookingLineMap, bookingLineLoadListLineMap, orderLine1, orderLine2, orderLine3, orderLine4);
				Factory.SaveForTesting();

				var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(order));
				var orderLineData1 = writer.GetDataObject(orderLine1);
				var orderLineData2 = writer.GetDataObject(orderLine2);
				var orderLineData3 = writer.GetDataObject(orderLine3);
				var orderLineData4 = writer.GetDataObject(orderLine4);
				var orderLineData5 = writer.GetDataObject(orderLine5);

				AssertNull(orderLineData1.RelatedEntityCollection);
				AssertNull(orderLineData2.RelatedEntityCollection);
				AssertNull(orderLineData3.RelatedEntityCollection);
				AssertNull(orderLineData4.RelatedEntityCollection);
				AssertNull(orderLineData5.RelatedEntityCollection);
			});
		}

		void TestOrderLineEntityKeyCollection_Cancelled(Order order, OrderLine orderLine, Dictionary<JobSupplierBookingLine, List<PackingLineForSupplierBookingLine>> bookingLineLoadListLineMap, BookingLineForOrderLine relatedBookingLineForOrderLine)
		{
			bookingLineLoadListLineMap.TryGetValue(relatedBookingLineForOrderLine.BookingLine, out var expectedPackingLines);
			expectedPackingLines[0].LoadListLine.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
			Factory.SaveForTesting();
			var writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(order));
			writer.GetDataObject(orderLine);
			var orderLineData = writer.GetDataObject(orderLine);
			AssertEquals(1, orderLineData.RelatedEntityCollection.Count);
			AssertNull(orderLineData.RelatedEntityCollection[0].RelatedEntityCollection);

			relatedBookingLineForOrderLine.BookingLine.SupplierBooking.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
			Factory.SaveForTesting();
			writer = new OrderLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)), new OrderLineLinkManager(), new LineRelatedDataWriterHelper(order));
			writer.GetDataObject(orderLine);
			orderLineData = writer.GetDataObject(orderLine);
			AssertNull(orderLineData.RelatedEntityCollection);
		}

		void CheckBookingLineRelatedEntityCollection(Entity bookingLineObjectData, List<BookingLineForOrderLine> expectedBookingLine, Dictionary<JobSupplierBookingLine, List<PackingLineForSupplierBookingLine>> bookingLineLoadListLineMap, out BookingLineForOrderLine relatedBookingLineForOrderLine)
		{
			var entityKeyCollection = bookingLineObjectData.EntityKeyCollection;
			AssertEquals(2, entityKeyCollection.Count);

			var bookineLineID = entityKeyCollection.Find(one => one.Type.ToString() == "BookingLineID").Key;
			AssertNotNull(bookineLineID);
			relatedBookingLineForOrderLine = expectedBookingLine.First(bookingLineForOrderLine => bookingLineForOrderLine.BookingLine.JSL_BookingLineId.Equals(bookineLineID));
			AssertNotNull(relatedBookingLineForOrderLine);

			var bookingID = entityKeyCollection.Find(one => one.Type.ToString() == "SupplierBooking").Key;
			AssertEquals(relatedBookingLineForOrderLine.BookingLine.SupplierBooking.JSB_BookingId, bookingID);

			var loadListLineObjectData = bookingLineObjectData.RelatedEntityCollection;
			if (relatedBookingLineForOrderLine.HasLoadListLine)
			{
				AssertEquals(relatedBookingLineForOrderLine.HasMultipleLine ? 2 : 1, loadListLineObjectData.Count);
				bookingLineLoadListLineMap.TryGetValue(relatedBookingLineForOrderLine.BookingLine, out var expectedPackingLines);
				AssertNotNull(expectedPackingLines);
				loadListLineObjectData.ForEach(packingLine => CheckLoadListLineRelatedEntityInformation(packingLine, expectedPackingLines));
			}
			else
			{
				AssertNull(loadListLineObjectData);
			}
		}

		void CheckLoadListLineRelatedEntityInformation(Entity loadListLineObjectData, List<PackingLineForSupplierBookingLine> expectedPackingLines)
		{
			var containerNumber = loadListLineObjectData.EntityKeyCollection.Find(one => one.Type.ToString() == "ContainerNumber").Key;
			AssertNotNull(containerNumber);
			var relatedContainer = expectedPackingLines.Find(entity => entity.Container != null && entity.Container.JC_ContainerNum.Equals(containerNumber));
			AssertNotNull(relatedContainer);

			OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(loadListLineObjectData.EntityKeyCollection, count: 2, containerLoadListKey: relatedContainer.LoadListLine.LoadListHeader.CLH_LoadListId, containerNumber: relatedContainer.Container.JC_ContainerNum);
			if (relatedContainer.IsPacked)
			{
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(loadListLineObjectData.RelatedEntityCollection[0].EntityKeyCollection, count: 2, packingLineKey: relatedContainer.PackingLine.JL_PackLineId, forwardingShipmentKey: relatedContainer.PackingLine.Shipment.JS_UniqueConsignRef);
			}
			else
			{
				AssertNull(loadListLineObjectData.RelatedEntityCollection);
			}
		}

		void UpdateOrderLinesRelatedData(Dictionary<OrderLine, List<BookingLineForOrderLine>> orderLineBookingLineMap, Dictionary<JobSupplierBookingLine, List<PackingLineForSupplierBookingLine>> bookingLineLoadListLineMap, OrderLine orderLine1, OrderLine orderLine2, OrderLine orderLine3, OrderLine orderLine4)
		{
			orderLineBookingLineMap.Add(orderLine1, new List<BookingLineForOrderLine>());
			CreateOrderLineRelatedData(orderLine1, true, true);
			CreateOrderLineRelatedData(orderLine1, true, true, true);
			CreateOrderLineRelatedData(orderLine1, true, false, true);

			orderLineBookingLineMap.Add(orderLine2, new List<BookingLineForOrderLine>());
			CreateOrderLineRelatedData(orderLine2, true, true);
			CreateOrderLineRelatedData(orderLine2, true, false);
			CreateOrderLineRelatedData(orderLine2, false, false);

			orderLineBookingLineMap.Add(orderLine3, new List<BookingLineForOrderLine>());
			CreateOrderLineRelatedData(orderLine3, false, false);

			orderLineBookingLineMap.Add(orderLine4, new List<BookingLineForOrderLine>());
			CreateOrderLineRelatedData(orderLine4, true, false);

			void CreateOrderLineRelatedData(OrderLine orderLine, bool hasContainerLoadList, bool hasPacked, bool hasMultipleLine = false)
			{
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();

				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.FillWithValidTestData();
				bookingLine.JSL_JSB_Booking = booking.PK;
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				orderLineBookingLineMap[orderLine].Add(new BookingLineForOrderLine(hasContainerLoadList, hasMultipleLine, bookingLine));
				if (!hasContainerLoadList)
				{
					return;
				}
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container1 = consol.Containers.AddNew();
				container1.FillWithValidTestData();
				container1.JC_JSB_SupplierBooking = booking.PK;

				var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = booking.PK;
				loadListHeader.CLH_Status = "SHP";

				var loadListLine1 = loadListHeader.LoadListLines.AddNew();
				loadListLine1.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine1.CLL_JC_Container = container1.PK;
				var packingLine1 = new PackingLineForSupplierBookingLine
				{
					IsPacked = hasPacked,
					LoadListLine = loadListLine1,
					Container = container1,
				};
				if (hasPacked)
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					var outerPackLine = shipment.OuterPackLines.AddNew();
					loadListLine1.CLL_JL_PackLine = outerPackLine.PK;
					outerPackLine.JL_JC = loadListLine1.CLL_JC_Container;
					packingLine1.PackingLine = outerPackLine;
				}
				bookingLineLoadListLineMap.Add(bookingLine, new List<PackingLineForSupplierBookingLine>() { packingLine1 });
				if (hasMultipleLine)
				{
					var container2 = consol.Containers.AddNew();
					container2.FillWithValidTestData();
					container2.JC_JSB_SupplierBooking = booking.PK;
					var loadListLine2 = loadListHeader.LoadListLines.AddNew();
					loadListLine2.CLL_JSL_BookingLine = bookingLine.PK;
					loadListLine2.CLL_JC_Container = container2.PK;
					var packingLine2 = new PackingLineForSupplierBookingLine
					{
						IsPacked = false,
						LoadListLine = loadListLine2,
						Container = container2,
					};
					bookingLineLoadListLineMap[bookingLine].Add(packingLine2);
				}
			}
		}

		class BookingLineForOrderLine
		{
			public bool HasLoadListLine { get; set; }
			public bool HasMultipleLine { get; set; }
			public JobSupplierBookingLine BookingLine { get; set; }

			public BookingLineForOrderLine(bool hasLoadListLine, bool hasMultipleLine, JobSupplierBookingLine bookingLine)
			{
				HasLoadListLine = hasLoadListLine;
				HasMultipleLine = hasMultipleLine;
				BookingLine = bookingLine;
			}
		}

		class PackingLineForSupplierBookingLine
		{
			public bool IsPacked { get; set; }
			public ForwardingPackLine PackingLine { get; set; }
			public ContainerLoadListLine LoadListLine { get; set; }
			public ForwardingContainer Container { get; set; }
		}

		#endregion
	}
}
