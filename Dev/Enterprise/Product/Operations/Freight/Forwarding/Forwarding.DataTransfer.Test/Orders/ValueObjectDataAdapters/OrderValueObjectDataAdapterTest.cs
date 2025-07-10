using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderValueObjectDataAdapter))]
	public class OrderValueObjectDataAdapterTest : ValueObjectDataAdapterTest<Order, Xsd.Order>
	{
		public void TestEDICodeMappingOnImport()
		{
			OrgPatternMatchOverride orgMatch1 = Factory.New<OrgPatternMatchOverride>();
			orgMatch1.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgMatch1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.PackageType;
			orgMatch1.OO_ForeignCode = "PK";
			orgMatch1.OO_LocalCode = Core.Constants.PkgUnit.Package;
			OrgPatternMatchOverride orgMatch2 = Factory.New<OrgPatternMatchOverride>();
			orgMatch2.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgMatch2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.PackageType;
			orgMatch2.OO_ForeignCode = Core.Constants.Volume.CubicMetres;
			orgMatch2.OO_LocalCode = Core.Constants.PkgUnit.Box;

			Xsd.Order orderValue = new Xsd.Order();

			Xsd.OrderOrderLine orderLineValue1 = orderValue.OrderLines.AddNew();
			orderLineValue1.OrderLineNo = 1;
			orderLineValue1.OrderLineDetail.QtyOrdered.Value = 5;
			orderLineValue1.OrderLineDetail.QtyOrdered.DimensionType = "PK";
			orderLineValue1.OrderLineDetail.InnerPacks.Value = 5;
			orderLineValue1.OrderLineDetail.InnerPacks.DimensionType = "PK";
			orderLineValue1.OrderLineDetail.OuterPacks.Value = 5;
			orderLineValue1.OrderLineDetail.OuterPacks.DimensionType = "PK";

			Xsd.OrderOrderLine orderLineValue2 = orderValue.OrderLines.AddNew();
			orderLineValue2.OrderLineNo = 2;
			orderLineValue2.OrderLineDetail.QtyOrdered.Value = 3;
			orderLineValue2.OrderLineDetail.QtyOrdered.DimensionType = Core.Constants.Volume.CubicMetres;

			Order order = Factory.New<Order>();
			DataAdapter.ImportFromValueObject(order, orderValue, Context);

			AssertEquals("Imported order should have 2 OrderLines", 2, order.OrderLines.Count);
			AssertEquals("QtyOrdered PackType for order.OrderLines[0] should be converted to PKG", Core.Constants.PkgUnit.Package, order.OrderLines[0].JO_F3_NKPackType);
			AssertEquals("InnerPacks PackType for order.OrderLines[0] should be converted to PKG", Core.Constants.PkgUnit.Package, order.OrderLines[0].JO_InnerPacksUQ);
			AssertEquals("OuterPacks PackType for order.OrderLines[0] should be converted to PKG", Core.Constants.PkgUnit.Package, order.OrderLines[0].JO_OuterPacksUQ);
			AssertEquals("QtyOrdered PackType for order.OrderLines[1] should be converted to BOX", Core.Constants.PkgUnit.Box, order.OrderLines[1].JO_F3_NKPackType);
		}

		public void TestImportDeliverPoint_TextOnly()
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = Buyer.PK;
			OrgAddress address2 = Buyer.Addresses.AddNew();
			address2.FillWithValidTestData();
			address2.OA_Code = "ADD2";
			Buyer.MainAddress.OA_Code = "ADD1";
			order.JD_OrderNumber = "A1000";
			OrderLine orderLine = order.OrderLines.AddNew();

			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			delivery.J4_OA_NKDeliveryPoint = "text adr";
			delivery.J4_Allocated = 10m;

			OrderLineDelivery decoyDelivery = orderLine.Deliveries.AddNew();
			decoyDelivery.J4_RL_NKDestinationPort = "AUPER";
			decoyDelivery.J4_OA_NKDeliveryPoint = "text adr2";

			Factory.Save();

			ZGuid oldDeliveryPK = delivery.PK;

			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "A1000";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			Xsd.OrderOrderLine orderLineValue = orderValue.OrderLines.AddNew();
			orderLineValue.OrderLineNo = 1;

			Xsd.OrderOrderLineOrderLineDelivery deliveryValue = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue.DeliveryDetails.DelPort.Value = "AUSYD";
			deliveryValue.DeliveryDetails.AddressFreeText = "text adr";
			deliveryValue.DeliveryDetails.QtyAllocated = 13m;

			Xsd.OrderOrderLineOrderLineDelivery deliveryValue2 = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue2.DeliveryDetails.DelPort.Value = "AUBNE";
			deliveryValue2.DeliveryDetails.AddressFreeText = "xsdAdr2";
			deliveryValue2.DeliveryDetails.QtyAllocated = 26m;

			DataAdapter.ImportFromValueObject(order, orderValue, Context);

			AssertEquals("Only 2 deliveries for the order line", 2, orderLine.Deliveries.Count);

			AssertEquals("old delivery", true, order.OrderLines[0].Deliveries[0].IsInDatabase);
			AssertEquals("Should be the same delivery", oldDeliveryPK, order.OrderLines[0].Deliveries[0].PK);
			AssertEquals("Should be the same delivery", "AUSYD", order.OrderLines[0].Deliveries[0].J4_RL_NKDestinationPort);
			AssertEquals("should be the same address text", "text adr", order.OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint);
			AssertEquals("Should be the updated qty", 13m, order.OrderLines[0].Deliveries[0].J4_Allocated);

			AssertEquals("new delivery", false, order.OrderLines[0].Deliveries[1].IsInDatabase);
			AssertEquals("Should be the same delivery", "AUBNE", order.OrderLines[0].Deliveries[1].J4_RL_NKDestinationPort);
			AssertEquals("should be the same address text", "xsdAdr2", order.OrderLines[0].Deliveries[1].J4_OA_NKDeliveryPoint);
			AssertEquals("Should be the correct qty", 26m, order.OrderLines[0].Deliveries[1].J4_Allocated);
		}

		public void TestImportDeliverPoint_UpdatesExistingAndRemovesOld()
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = Buyer.PK;
			OrgAddress address2 = Buyer.Addresses.AddNew();
			address2.FillWithValidTestData();
			address2.OA_Code = "ADD2";
			Buyer.MainAddress.OA_Code = "ADD1";
			Buyer.MainAddress.OA_RL_NKRelatedPortCode = "UAIEV";
			order.JD_OrderNumber = "A1000";
			OrderLine orderLine = order.OrderLines.AddNew();

			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			delivery.J4_OA_DeliveryAddr = Buyer.MainAddress.PK;
			delivery.J4_Allocated = 10m;

			OrderLineDelivery decoyDelivery = orderLine.Deliveries.AddNew();
			decoyDelivery.J4_RL_NKDestinationPort = "AUPER";
			decoyDelivery.J4_OA_DeliveryAddr = address2.PK;

			Factory.Save();

			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "A1000";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			Xsd.OrderOrderLine orderLineValue = orderValue.OrderLines.AddNew();
			orderLineValue.OrderLineNo = 1;

			Xsd.OrderOrderLineOrderLineDelivery deliveryValue = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue.DeliveryDetails.DelPort.Value = "AUSYD";
			deliveryValue.DeliveryDetails.Address.AddressSequenceRef = 1;
			deliveryValue.DeliveryDetails.QtyAllocated = 13m;

			Xsd.OrderOrderLineOrderLineDelivery deliveryValue2 = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue2.DeliveryDetails.DelPort.Value = "AUBNE";
			deliveryValue2.DeliveryDetails.Address.AddressSequenceRef = 2;
			deliveryValue2.DeliveryDetails.QtyAllocated = 26m;

			DataAdapter.ImportFromValueObject(order, orderValue, Context);

			AssertEquals("Only 2 deliveries for the order line", 2, orderLine.Deliveries.Count);

			CombineAssertions(delegate
			{
				AssertEquals("Should be the same delivery", true, order.OrderLines[0].Deliveries[0].PK == delivery.PK);
				AssertEquals("Should be the same delivery", "AUSYD", order.OrderLines[0].Deliveries[0].J4_RL_NKDestinationPort);
				AssertEquals("Should be the updated qty", 13m, order.OrderLines[0].Deliveries[0].J4_Allocated);

				AssertEquals("new delivery", false, order.OrderLines[0].Deliveries[1].IsInDatabase);
				AssertEquals("Should be the same delivery", "AUBNE", order.OrderLines[0].Deliveries[1].J4_RL_NKDestinationPort);
				AssertEquals("Should be the correct qty", 26m, order.OrderLines[0].Deliveries[1].J4_Allocated);
			}
			);
		}

		public void TestImportDeliverPoint()
		{
			DeliveryAddress.Factory.Save();
			Xsd.Order xsdOrder = GetXsdOrder(true);

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);

			ZQuery filter = new ZQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, Buyer.MainAddress.PK);
			filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, "1000");
			Order loadedOrder = Factory.LoadTop1<Order>(filter);
			AssertNotNull("Order should not be null", loadedOrder);

			OrderLine orderLine = loadedOrder.OrderLines[0];
			Assert("Order line has deliveries", orderLine.Deliveries.Count > 0);

			OrgAddress deliverPoint = orderLine.Deliveries[0].DeliveryPoint;
			AssertNotNull("Should have a deliver point", deliverPoint);
			AssertEquals("Imported Deliver Point", "WETHERILL", deliverPoint.OA_Code);
		}

		public void TestImportDeliverPoint_NotFound()
		{
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@edi.com.au";
			NotificationDataRegistry.Instance.OrderImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Xsd.Order xsdOrder = GetXsdOrder(false);

			DeliveryAddress.Factory.Save();
			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);

			Factory.Save();

			AssertEquals("No delivery point matched", true, order.OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint.IsEmpty);
			Assert("One email should have been sent", Env.OutgoingMailManager.EmailsCreated.Count > 0);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains(TestOrderValueObjectDataAdapter.DeliverPointAddressErrorMessage, email.Body);
			AssertContains("Order: 1000 ordered by " + Buyer.OH_FullName + " (Code = 'BUYERSYD'), Organization Name: Delivery Point Org Name, Address Code: WRONG CODE", email.Body);

			StmNote[] notes = order.Notes.FindByDescription(OrderValueObjectDataAdapter.UnmatchedOrderLineDeliverPointNote);
			AssertNotNull("Unmatched note should have been created", notes.Length > 0);
			AssertEquals("Order Line 1:\r\nAddress Code: WRONG CODE\r\nAddress: WRONG ADDRESS 1, WRONG ADDRESS 2, , , \r\n", notes[0].ST_NoteDataAsText);
		}

		public void TestImportDeliverPointOnlyUsesAddressFromBuyer()
		{
			OrgHeader dummyOrg = Factory.New<OrgHeader>();
			dummyOrg.CopyPersistentValuesFrom(Buyer);
			dummyOrg.OH_Code = "oDUMMYo";

			OrgAddress dummyAddress = dummyOrg.MainAddress;
			dummyAddress.CopyPersistentValuesFrom(DeliveryAddress);
			dummyAddress.OA_Code = "aDUMMYa";
			dummyAddress.OA_OH = dummyOrg.PK;

			foreach (OrgPatternMatch match in Buyer.PatternMatchesForThisOrg)
			{
				match.OS_OH = dummyOrg.PK;
				match.OS_OA = dummyOrg.MainAddress.PK;
			}

			Factory.Save();

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order order = Factory.New<Order>();
			Xsd.Order xsdOrder = GetXsdOrder(true);
			Xsd.Organisation xsdOrg = xsdOrder.OrderLines[0].OrderLineDeliveries[0].DeliveryDetails.Address.Organisation;
			xsdOrg.OwnerCode = Buyer.OH_Code;
			foreach (Xsd.AddressBase xsdAddress in xsdOrg.OrganisationDetails.Addresses)
			{
				xsdAddress.AddressCode = ZString.Empty;
			}

			adapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals("DeliveryPoint.OA_Code", "WETHERILL", order.OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint);
		}

		public void TestImportDeliveryPoint_ContainerDeliveriesNotRemoved()
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = Buyer.PK;
			OrgAddress address2 = Buyer.Addresses.AddNew();
			address2.FillWithValidTestData();
			address2.OA_Code = "ADD2";
			Buyer.MainAddress.OA_Code = "ADD1";
			order.JD_OrderNumber = "A1000";

			var plannedContainer = order.PlannedContainers.AddNew();
			plannedContainer.J1_ContainerNumber = "C44";
			plannedContainer.J1_ContainerCount = 1;
			plannedContainer.J1_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			OrderLine orderLine = order.OrderLines.AddNew();

			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			delivery.J4_OA_NKDeliveryPoint = "text adr";
			delivery.J4_Allocated = 10m;

			var containerDelivery = delivery.Containers.AddNew();
			containerDelivery.J5_ContainerNum = "C44";

			Factory.Save();

			ZGuid oldDeliveryPK = delivery.PK;

			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "A1000";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			Xsd.OrderOrderLine orderLineValue = orderValue.OrderLines.AddNew();
			orderLineValue.OrderLineNo = 1;

			Xsd.OrderOrderLineOrderLineDelivery deliveryValue = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue.DeliveryDetails.DelPort.Value = "AUSYD";
			deliveryValue.DeliveryDetails.AddressFreeText = "text adr";
			deliveryValue.DeliveryDetails.QtyAllocated = 13m;

			DataAdapter.ImportFromValueObject(order, orderValue, Context);

			AssertEquals("Only 1 delivery for the order line", 1, orderLine.Deliveries.Count);

			AssertEquals("old delivery", true, order.OrderLines[0].Deliveries[0].IsInDatabase);
			AssertEquals("Should be the same delivery", oldDeliveryPK, order.OrderLines[0].Deliveries[0].PK);

			AssertEquals("Container deliveries were not removed", 1, order.OrderLines[0].Deliveries[0].Containers.Count);
		}

		public void TestImportDecimals_OutOfRange()
		{
			var outOfSqlRangeDecimal = 9876543210.1M;
			var bigOutOfSqlRangeDecimal = Decimal.MaxValue;
			var expectedValue = 0M;
			var expectedExchangeRate = 1M;

			var orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "A1000";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;

			orderValue.OrderDetail.ShipmentPlanning.Volume.Value = outOfSqlRangeDecimal;
			orderValue.OrderDetail.ShipmentPlanning.Weight.Value = outOfSqlRangeDecimal;
			orderValue.OrderDetail.Custom.Decimal1 = outOfSqlRangeDecimal;
			orderValue.OrderDetail.Custom.Decimal2 = outOfSqlRangeDecimal;
			orderValue.OrderDetail.Custom.Decimal3 = outOfSqlRangeDecimal;
			orderValue.OrderDetail.Custom.Decimal4 = outOfSqlRangeDecimal;
			orderValue.OrderDetail.Custom.Decimal5 = outOfSqlRangeDecimal;
			orderValue.OrderDetail.ExchangeRate = outOfSqlRangeDecimal;

			var orderLineValue = orderValue.OrderLines.AddNew();
			orderLineValue.OrderLineNo = 1;
			orderLineValue.OrderLineDetail.Volume = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Weight = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Custom.Decimal1 = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Custom.Decimal2 = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Custom.Decimal3 = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Custom.Decimal4 = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.Custom.Decimal5 = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.InnerPacks.Value = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.OuterPacks.Value = outOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.QtyOrdered.Value = bigOutOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.ItemPrice.Value = bigOutOfSqlRangeDecimal;
			orderLineValue.OrderLineDetail.LinePrice.Value = bigOutOfSqlRangeDecimal;

			var deliveryValue = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue.DeliveryDetails.DelPort.Value = "AUSYD";
			deliveryValue.DeliveryDetails.Custom.Decimal1 = outOfSqlRangeDecimal;
			deliveryValue.DeliveryDetails.Custom.Decimal2 = outOfSqlRangeDecimal;
			deliveryValue.DeliveryDetails.Custom.Decimal3 = outOfSqlRangeDecimal;
			deliveryValue.DeliveryDetails.Custom.Decimal4 = outOfSqlRangeDecimal;
			deliveryValue.DeliveryDetails.Custom.Decimal5 = outOfSqlRangeDecimal;

			var deliveryContainerValue = deliveryValue.DeliveryContainers.AddNew();
			deliveryContainerValue.Weight.Value = outOfSqlRangeDecimal;
			deliveryContainerValue.Volume.Value = outOfSqlRangeDecimal;

			var order = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);

			AssertEquals("JD_ActualVolume", expectedValue, order.JD_ActualVolume);
			AssertEquals("JD_ActualWeight", expectedValue, order.JD_ActualWeight);
			AssertEquals("JD_CustomDecimal1", expectedValue, order.JD_CustomDecimal1);
			AssertEquals("JD_CustomDecimal2", expectedValue, order.JD_CustomDecimal2);
			AssertEquals("JD_CustomDecimal3", expectedValue, order.JD_CustomDecimal3);
			AssertEquals("JD_CustomDecimal4", expectedValue, order.JD_CustomDecimal4);
			AssertEquals("JD_CustomDecimal5", expectedValue, order.JD_CustomDecimal5);
			AssertEquals("JD_EstimatedExchangeRate", expectedExchangeRate, order.JD_EstimatedExchangeRate);

			var orderLine = order.OrderLines[0];

			AssertEquals("JO_ActualVolume", expectedValue, orderLine.JO_ActualVolume);
			AssertEquals("JO_ActualWeight", expectedValue, orderLine.JO_ActualWeight);
			AssertEquals("JO_CustomDecimal1", expectedValue, orderLine.JO_CustomDecimal1);
			AssertEquals("JO_CustomDecimal2", expectedValue, orderLine.JO_CustomDecimal2);
			AssertEquals("JO_CustomDecimal3", expectedValue, orderLine.JO_CustomDecimal3);
			AssertEquals("JO_CustomDecimal4", expectedValue, orderLine.JO_CustomDecimal4);
			AssertEquals("JO_CustomDecimal5", expectedValue, orderLine.JO_CustomDecimal5);
			AssertEquals("JO_InnerPacks", expectedValue, orderLine.JO_InnerPacks);
			AssertEquals("JO_OuterPacks", expectedValue, orderLine.JO_OuterPacks);
			AssertEquals("JO_Quantity", expectedValue, orderLine.JO_Quantity);
			AssertEquals("JO_ItemPrice", expectedValue, orderLine.JO_ItemPrice);
			AssertEquals("JO_LinePrice", expectedValue, orderLine.JO_LinePrice);

			var orderLineDelivery = orderLine.Deliveries[0];

			AssertEquals("J4_CustomDecimal1", expectedValue, orderLineDelivery.J4_CustomDecimal1);
			AssertEquals("J4_CustomDecimal2", expectedValue, orderLineDelivery.J4_CustomDecimal2);
			AssertEquals("J4_CustomDecimal3", expectedValue, orderLineDelivery.J4_CustomDecimal3);
			AssertEquals("J4_CustomDecimal4", expectedValue, orderLineDelivery.J4_CustomDecimal4);
			AssertEquals("J4_CustomDecimal5", expectedValue, orderLineDelivery.J4_CustomDecimal5);

			var orderLineDeliveryContainer = orderLineDelivery.Containers[0];

			AssertEquals("J5_Weight", expectedValue, orderLineDeliveryContainer.J5_Weight);
			AssertEquals("J5_Volume", expectedValue, orderLineDeliveryContainer.J5_Volume);
		}

		public void TestImportDatesOutOfSmallDateTimeRange_ShouldRaiseXmlException()
		{
			var outOfSqlRangeDatePast = ZDateTime.BrettsBirthday.AddYears(-100);
			var outOfSqlRangeDateFuture = ZDateTime.BrettsBirthday.AddYears(110);
			AssertOutOfRangeDateShouldBeIgnored(outOfSqlRangeDatePast);
			AssertOutOfRangeDateShouldBeIgnored(outOfSqlRangeDateFuture);
		}

		void AssertOutOfRangeDateShouldBeIgnored(ZDateTime outOfSqlRangeDate)
		{
			var expectedValue = ZDateTime.Empty;
			var inRangeDate = ZDateTime.BrettsBirthday;

			var orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "A1000";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;

			orderValue.OrderDetail.ConfirmDate = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderValue.OrderDetail.ConfirmDate), outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.ConfirmDate = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.InvoiceDate = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderValue.OrderDetail.InvoiceDate), outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.InvoiceDate = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.OrderDateTime = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderValue.OrderDetail.OrderDateTime), outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.OrderDateTime = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.DeliveryRequiredBy = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderValue.OrderDetail.DeliveryRequiredBy), outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.DeliveryRequiredBy = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.ExWorksRequiredBy = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderValue.OrderDetail.ExWorksRequiredBy), outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.ExWorksRequiredBy = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.Custom.Date1 = outOfSqlRangeDate;
			AssertXmlExceptionThrown("Custom Date1", outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.Custom.Date1 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderValue.OrderDetail.Custom.Date2 = outOfSqlRangeDate;
			AssertXmlExceptionThrown("Custom Date2", outOfSqlRangeDate, orderValue);
			orderValue.OrderDetail.Custom.Date2 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			var orderLineValue = orderValue.OrderLines.AddNew();
			orderLineValue.OrderLineNo = 1;

			orderLineValue.OrderLineDetail.DropDate = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.DropDate), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.DropDate = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.ConfirmDate = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.ConfirmDate), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.ConfirmDate = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.ExWorksRequiredBy = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.ExWorksRequiredBy), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.ExWorksRequiredBy = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.Custom.Date1 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.Custom.Date1), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.Custom.Date1 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.Custom.Date2 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.Custom.Date2), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.Custom.Date2 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.Custom.Date3 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.Custom.Date3), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.Custom.Date3 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.Custom.Date4 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.Custom.Date4), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.Custom.Date4 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			orderLineValue.OrderLineDetail.Custom.Date5 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(orderLineValue.OrderLineDetail.Custom.Date5), outOfSqlRangeDate, orderValue);
			orderLineValue.OrderLineDetail.Custom.Date5 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			var deliveryValue = orderLineValue.OrderLineDeliveries.AddNew();
			deliveryValue.DeliveryDetails.DelPort.Value = "AUSYD";

			deliveryValue.DeliveryDetails.Custom.Date1 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryValue.DeliveryDetails.Custom.Date1), outOfSqlRangeDate, orderValue);
			deliveryValue.DeliveryDetails.Custom.Date1 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			deliveryValue.DeliveryDetails.Custom.Date2 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryValue.DeliveryDetails.Custom.Date2), outOfSqlRangeDate, orderValue);
			deliveryValue.DeliveryDetails.Custom.Date2 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			deliveryValue.DeliveryDetails.Custom.Date3 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryValue.DeliveryDetails.Custom.Date3), outOfSqlRangeDate, orderValue);
			deliveryValue.DeliveryDetails.Custom.Date3 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			deliveryValue.DeliveryDetails.Custom.Date4 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryValue.DeliveryDetails.Custom.Date4), outOfSqlRangeDate, orderValue);
			deliveryValue.DeliveryDetails.Custom.Date4 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			deliveryValue.DeliveryDetails.Custom.Date5 = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryValue.DeliveryDetails.Custom.Date5), outOfSqlRangeDate, orderValue);
			deliveryValue.DeliveryDetails.Custom.Date5 = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			var deliveryContainerValue = deliveryValue.DeliveryContainers.AddNew();

			deliveryContainerValue.ETA = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryContainerValue.ETA), outOfSqlRangeDate, orderValue);
			deliveryContainerValue.ETA = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			deliveryContainerValue.ETD = outOfSqlRangeDate;
			AssertXmlExceptionThrown(nameof(deliveryContainerValue.ETD), outOfSqlRangeDate, orderValue);
			deliveryContainerValue.ETD = inRangeDate;
			AssertNoExceptionThrown(() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));

			var order = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("JD_BookingConfDate", inRangeDate, order.JD_BookingConfDate);
			AssertEquals("JD_CustomDate1", inRangeDate, order.JD_CustomDate1);
			AssertEquals("JD_CustomDate2", inRangeDate, order.JD_CustomDate2);

			AssertEquals("JD_OrderDate", inRangeDate, order.JD_OrderDate);
			AssertEquals("JD_InvoiceDate", inRangeDate, order.JD_InvoiceDate);
			AssertEquals("JD_DeliveryRequiredBy", inRangeDate, order.JD_DeliveryRequiredBy);
			AssertEquals("JD_ExWorksRequiredBy", inRangeDate, order.JD_ExWorksRequiredBy);
			AssertEquals("JD_BookingConfDate", inRangeDate, order.JD_BookingConfDate);

			var orderLine = order.OrderLines[0];

			AssertEquals("JO_CustomDate1", inRangeDate, orderLine.JO_CustomDate1);
			AssertEquals("JO_CustomDate2", inRangeDate, orderLine.JO_CustomDate2);
			AssertEquals("JO_CustomDate3", inRangeDate, orderLine.JO_CustomDate3);
			AssertEquals("JO_CustomDate4", inRangeDate, orderLine.JO_CustomDate4);
			AssertEquals("JO_CustomDate5", inRangeDate, orderLine.JO_CustomDate5);

			AssertEquals("JO_LineDropDate", inRangeDate, orderLine.JO_LineDropDate);
			AssertEquals("JO_ConfirmationDate", inRangeDate, orderLine.JO_ConfirmationDate);
			AssertEquals("JO_ExWorksDate", inRangeDate, orderLine.JO_ExWorksDate);

			var orderLineDelivery = orderLine.Deliveries[0];

			AssertEquals("J4_CustomDate1", inRangeDate, orderLineDelivery.J4_CustomDate1);
			AssertEquals("J4_CustomDate2", inRangeDate, orderLineDelivery.J4_CustomDate2);
			AssertEquals("J4_CustomDate3", inRangeDate, orderLineDelivery.J4_CustomDate3);
			AssertEquals("J4_CustomDate4", inRangeDate, orderLineDelivery.J4_CustomDate4);
			AssertEquals("J4_CustomDate5", inRangeDate, orderLineDelivery.J4_CustomDate5);

			var orderLineDeliveryContainer = orderLineDelivery.Containers[0];
			AssertEquals("J5_ETA", inRangeDate, orderLineDeliveryContainer.J5_ETA);
			AssertEquals("J5_ETD", inRangeDate, orderLineDeliveryContainer.J5_ETD);
		}

		void AssertXmlExceptionThrown(string propertyName, ZDateTime outOfSqlRangeDate, Xsd.Order orderValue)
		{
			AssertExceptionThrown<System.Xml.XmlException>($"{propertyName} import should throw XmlException",
				$"{propertyName} field is invalid. {outOfSqlRangeDate.Day:00}/{outOfSqlRangeDate.Month:00}/{outOfSqlRangeDate.Year:0000} 12:00:00 AM is not in range [01-Jan-1900 - 06-Jun-2079].",
				() => DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context));
		}

		public void TestUpdatingExistingOrderLines()
		{
			Xsd.Order xsdOrder = GetXsdOrder();
			Xsd.OrderOrderLine xsdOrderLine1 = xsdOrder.OrderLines.AddNew();
			Xsd.OrderOrderLine xsdOrderLine2 = xsdOrder.OrderLines.AddNew();
			Xsd.OrderOrderLine xsdOrderLine3 = xsdOrder.OrderLines.AddNew();
			xsdOrderLine1.OrderLineNo = 1;
			xsdOrderLine2.OrderLineNo = 2;

			xsdOrderLine2.OrderSubLineNo = 1;
			xsdOrderLine2.OrderSubLineNoSpecified = true;

			xsdOrderLine3.OrderLineNo = 2;
			xsdOrderLine3.OrderSubLineNo = 2;
			xsdOrderLine3.OrderSubLineNoSpecified = true;
			xsdOrderLine3.OrderLineSplitNo = 3;
			xsdOrderLine3.OrderLineSplitNoSpecified = true;

			Order order = Factory.New<Order>();
			DataAdapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals("3 order lines imported", 3, order.OrderLines.Count);

			OrderLine orderLine1 = order.OrderLines[0];
			OrderLine orderLine2 = order.OrderLines[1];
			OrderLine orderLine3 = order.OrderLines[2];

			AssertEquals(1, orderLine1.JO_LineNo);
			AssertEquals(1, orderLine1.JO_SubLineNo);

			AssertEquals(2, orderLine2.JO_LineNo);
			AssertEquals(1, orderLine2.JO_SubLineNo);

			AssertEquals(2, orderLine3.JO_LineNo);
			AssertEquals(2, orderLine3.JO_SubLineNo);
			AssertEquals((short)3, orderLine3.JO_LineSplitNumber);

			xsdOrderLine1.OrderLineDetail.Description = "Modified orderLine1";
			xsdOrderLine2.OrderLineDetail.Description = "Modified orderLine2";
			xsdOrderLine3.OrderLineDetail.Description = "Modified orderLine3";
			Xsd.OrderOrderLine xsdNewOrderLine = xsdOrder.OrderLines.AddNew();
			xsdNewOrderLine.OrderLineDetail.Description = "New orderLine";

			xsdNewOrderLine.OrderLineNo = 2;
			xsdNewOrderLine.OrderSubLineNo = 3;
			DataAdapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals("Order line 1 updated", "Modified orderLine1", orderLine1.JO_Description);
			AssertEquals("Order line 2 updated", "Modified orderLine2", orderLine2.JO_Description);
			AssertEquals("Order line 3 updated", "Modified orderLine3", orderLine3.JO_Description);
			AssertEquals("New order created", "New orderLine", order.OrderLines[3].JO_Description);
		}

		public void TestOnlyOperationalEventsAreImported()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";

			// Create events (2 operational and 2 admin)
			Xsd.Events xsdEvents = new Xsd.Events();

			// DataExport Event
			Xsd.Event xsdEvent = xsdEvents.Event.AddNew();
			xsdEvent.Code = Events.DataExport.Code;
			xsdEvent.DateTime = ZDateTime.Now;
			// OrderShipped Event
			Xsd.Event shippedEvent = xsdEvents.Event.AddNew();
			shippedEvent.Code = Events.OrderShipped.Code;
			shippedEvent.DateTime = ZDateTime.Now;

			// Add Event
			Xsd.Event addEvent = xsdEvents.Event.AddNew();
			addEvent.Code = Events.AddedARecordToTheSystem.Code;
			addEvent.DateTime = ZDateTime.Now;
			// Edit Event
			Xsd.Event editEvent = xsdEvents.Event.AddNew();
			editEvent.Code = Events.EditedARecord.Code;
			editEvent.DateTime = ZDateTime.Now;

			xsdOrder.Events = xsdEvents;

			Order order = Factory.New<Order>();

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();

			adapter.ImportFromValueObject(order, xsdOrder, Context);

			StmALog[] dataExportLog = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DataExport Event should have been imported", 1, dataExportLog.Length);
			StmALog[] orderShippedLog = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.OrderShipped.Code));
			AssertEquals("orderShipped Event should have been imported", 1, orderShippedLog.Length);

			StmALog[] addLog = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code));
			AssertEquals("Add Event should NOT have been imported", 0, addLog.Length);
			StmALog[] editLog = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Edit Event should NOT have been imported", 0, editLog.Length);
		}

		public void TestFindBusinessObject_SplitNumber_Zero()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "splaty";
			order1.JD_OrderNumberSplit = 0;
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order1.Buyer.OH_Code = "order1";

			Factory.Save();

			var adapter = new TestOrderValueObjectDataAdapter();
			var orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "splaty";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = order1.Buyer.OH_Code;

			var orderFound = adapter.FindBusinessObject(orderValue, Context);
			AssertEquals("Should find the Order with the highest split number with single split.", order1.PK, orderFound.PK);
		}

		public void TestFindBusinessObject_SplitNumber_One()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "splaty";
			order1.JD_OrderNumberSplit = 0;
			order1.BuyerPK = header.PK;
			order1.Buyer.OH_Code = "order1";

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "splaty";
			order2.JD_OrderNumberSplit = 1;
			order2.BuyerPK = header.PK;
			order2.Buyer.OH_Code = "order1";

			Factory.Save();

			var adapter = new TestOrderValueObjectDataAdapter();
			var orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "splaty";
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = order2.Buyer.OH_Code;

			var orderFound = adapter.FindBusinessObject(orderValue, Context);
			AssertEquals("Should find the Order with the highest split number with two splits.", order2.PK, orderFound.PK);
		}

		public void TestFindBusinessObject()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "splaty";
			order1.JD_OrderNumberSplit = 1;
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order1.Buyer.OH_Code = "order1";

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "splaty";
			order2.JD_OrderNumberSplit = 1;
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order2.Buyer.OH_Code = "order2";

			var order3 = Factory.NewWithValidTestData<Order>();
			order3.JD_OrderNumber = "splaty";
			order3.JD_OrderNumberSplit = 0;
			order3.BuyerPK = order2.BuyerPK;

			Factory.Save();

			var adapter = new TestOrderValueObjectDataAdapter();
			var orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "splaty";
			orderValue.OrderIdentifier.OrderNumberSplit = 1;
			orderValue.OrderDetail = new Xsd.OrderOrderDetail();
			orderValue.OrderDetail.Buyer = new Xsd.Organisation();
			orderValue.OrderDetail.Buyer.EDICode = order2.Buyer.OH_Code;

			var orderFound = adapter.FindBusinessObject(orderValue, Context);
			AssertEquals("Should find the Order with the correct buyer.", order2.PK, orderFound.PK);
		}

		public void TestFindBusinessObject_ByConsigneeOrganisationType()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.Delete();
			order.BuyerPK = Buyer.PK;
			order.JD_OrderNumber = "splaty";
			order.JD_OrderNumberSplit = 0;
			Factory.Save(); // required so that OrgPatternMatch is updated with matching data

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "splaty";
			orderValue.OrderIdentifier.OrderNumberSplit = 0;
			orderValue.OrderDetail.Buyer.EDICode = "buyer";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Name = "Buyer";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			Xsd.OrgAddress address = orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "Buyer";
			address.CityOrSuburb = "City";

			Order orderFound = adapter.FindBusinessObject(orderValue, Context);
			AssertEquals("Should find the Order with the correct buyer", order.PK, orderFound.PK);
		}

		public void TestFindBusinessObjects()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Buyer.Delete();
			order.BuyerPK = Buyer.PK;
			order.JD_OrderNumber = "splaty";
			order.JD_OrderNumberSplit = 0;
			order.JD_OrderStatus = Constants.OrderStatus.Cancelled;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.Buyer.Delete();
			order2.BuyerPK = Buyer.PK;
			order2.JD_OrderNumber = "splaty";
			order2.JD_OrderNumberSplit = 1;
			order2.JD_OrderStatus = Constants.OrderStatus.Incomplete;

			Factory.Save(); // required so that OrgPatternMatch is updated with matching data

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			orderValue.OrderIdentifier.OrderNumber = "splaty";
			orderValue.OrderDetail.Buyer.EDICode = "buyer";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Name = "Buyer";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			Xsd.OrgAddress address = orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "Buyer";
			address.CityOrSuburb = "City";

			Order[] ordersFound = adapter.FindBusinessObjects(orderValue, Context);
			AssertNotNull(ordersFound);
			AssertEquals("Should find 1 order", 1, ordersFound.Length);
			AssertEquals("Should find the Order with status not Cancelled", order2.PK, ordersFound[0].PK);
		}

		public void TestImportOrderNumberAndSplit()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			xsdOrder.OrderIdentifier.OrderNumber = "O11";
			xsdOrder.OrderIdentifier.OrderNumberSplit = 1;

			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderNumber", "O11", orderBizObj.JD_OrderNumber);
			AssertEquals("OrderNumberSplit", (ZByte)1, orderBizObj.JD_OrderNumberSplit);
		}

		public void TestDontOverrideSendingAgentOnBuyerSupplier()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OrganisationTypes = OrganisationTypes.Consignee;
			buyer.OH_FullName = "Buyer";
			buyer.Addresses[0].OA_Address1 = "Buyer";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OrganisationTypes = OrganisationTypes.Consignor;
			supplier.OH_FullName = "Supplier";
			supplier.Addresses[0].OA_Address1 = "Supplier";

			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "Test Sending Agent";
			sendingAgent.OH_Code = "TSAAU";
			sendingAgent.Addresses[0].OA_Address1 = "SendingAgent";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "Test Receiving Agent";
			receivingAgent.OH_Code = "TRAAU";
			receivingAgent.Addresses[0].OA_Address1 = "ReceivingAgent";

			ZString localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			buyer.OH_RL_NKClosestPort = localPort;
			supplier.OH_RL_NKClosestPort = localPort;

			OrgSupplierBuyerLink supplierLink = buyer.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = supplier.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_OH_ReceivingAgent = receivingAgent.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_OH_SendingAgent = sendingAgent.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.Sea;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = Constants.ContainerModes.FCL;

			Xsd.Order orderValue = new Xsd.Order();

			orderValue.OrderDetail.Buyer.OrganisationDetails.Name = "Buyer";
			orderValue.OrderDetail.Supplier.OrganisationDetails.Name = "Supplier";
			orderValue.OrderDetail.Buyer.EDICode = buyer.OH_Code;
			orderValue.OrderDetail.Buyer.OwnerCode = buyer.OH_Code;
			orderValue.OrderDetail.Supplier.EDICode = supplier.OH_Code;
			orderValue.OrderDetail.Supplier.OwnerCode = supplier.OH_Code;
			orderValue.OrderDetail.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.OrderTransportMode.SEA;
			orderValue.OrderDetail.ContainerMode = Enterprise.DataTransfer.Xml.XsdVersion1.OrderContainerMode.FCL;

			OrgPatternMatchOverride orgMatch = Factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgMatch.OO_ForeignCode = orderValue.OrderDetail.Buyer.OwnerCode;
			orgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_LocalGuid = buyer.PK;

			orgMatch = Factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgMatch.OO_ForeignCode = orderValue.OrderDetail.Supplier.OwnerCode;
			orgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_LocalGuid = supplier.PK;

			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Buyer", buyer.PK, order.BuyerPK);
			AssertEquals("Supplier", supplier.PK, order.SupplierPK);
			AssertEquals("Should set Receiving Agent from Supplier Link", receivingAgent.PK, order.JD_OH_ReceivingAgent);
			AssertEquals("Should set Sending Agent from Buyer Link", sendingAgent.PK, order.JD_OH_SendingAgent);
		}

		public void TestImportOrderBuyerSupplier()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			Assert("Invalid/abset buyer must raise DataErrorPreventSave error", Notifications.ContainsNotificationType(ErrorType.DataErrorPreventSave));
			AssertEquals("Buyer.IsValid", false, orderBizObj.BuyerPK.IsValid);
			AssertEquals("Supplier", ZGuid.Empty, orderBizObj.SupplierPK);

			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Buyer.IsValid", true, orderBizObj.BuyerPK.IsValid);

			var buyer = Factory.Load<OrgHeader>(orderBizObj.BuyerPK);
			if (buyer.SupplierLinks != null && buyer.SupplierLinks.Count > 0)
			{
				AssertEquals("Supplier", buyer.SupplierLinks[0].OL_OH_Supplier, orderBizObj.SupplierPK);
			}
			else
			{
				AssertEquals("Supplier", ZGuid.Empty, orderBizObj.SupplierPK);

				OrgSupplierBuyerLink supplierLink = buyer.SupplierLinks.AddNew();
				supplierLink.OL_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, buyer.PK)).PK;
				adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

				AssertEquals("Buyer.SupplierLinks.Count", 1, buyer.SupplierLinks.Count);
				AssertEquals("Supplier not specified", ZGuid.Empty, orderBizObj.SupplierPK);
				Assert("No supplier specified notification", Notifications.AsString.Contains("No supplier specified"));
			}
		}

		public void TestXMLImportWithNoSupplierNoInco()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail.Supplier.OrganisationDetails.Name = "Supplier";
			xsdOrder.OrderDetail.Incoterm = "INT";
			OrderValueObjectDataAdapter adapter = new OrderValueObjectDataAdapter();

			Order orderBizObj = Factory.New<Order>();
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Supplier.OwnerCode);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("Supplier has value, should import", xsdOrder.OrderDetail.Supplier.OrganisationDetails.Name, orderBizObj.Supplier.OH_FullName);
			AssertEquals("INCO term has value, should import", xsdOrder.OrderDetail.Incoterm, orderBizObj.JD_IncoTerm);

			xsdOrder.OrderDetail.Supplier = null;
			xsdOrder.OrderDetail.Incoterm = null;
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Supplier has no value, should not import", "Supplier", orderBizObj.Supplier.OH_FullName);
			AssertEquals("INCO term has no value, should not import", "INT", orderBizObj.JD_IncoTerm);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Supplier has no value, should import null", null, orderBizObj.Supplier);
			AssertEquals("INCO term has no value, should import Empty", ZString.Empty, orderBizObj.JD_IncoTerm);
		}

		public void TestImportOrderWithNoContainerMode()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.TransportMode = Enterprise.DataTransfer.Xml.XsdVersion1.OrderTransportMode.SEA;

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("SEA", orderBizObj.JD_TransportMode);
			AssertEquals("Container Mode correctly taken from 1st item in list", "FCL", orderBizObj.JD_ContainerMode);
		}

		public void TestImportOrderWithInvalidCurrency()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.OrderTotal = new Xsd.FinancialValue();
			xsdOrder.OrderDetail.OrderTotal.Value = 33m;
			xsdOrder.OrderDetail.OrderTotal.CurrencyCode = "XYZ";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertNull(orderBizObj.OrderCurrency);
			AssertContains("Invalid currency specified - XYZ", Notifications.AsString);

			Notifications.Clear();
			xsdOrder.OrderDetail.OrderTotal.CurrencyCode = "INR";
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("INR", orderBizObj.JD_RX_NKOrderCurrency);
			AssertNotContains("Invalid currency specified", Notifications.AsString);
		}

		public void TestImportOrderShipmentPlanning()
		{
			var xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			var goodAvailableAtAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodAvailableAtAddress.OA_Code = "AvailAt";

			var goodDeliveredToAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodDeliveredToAddress.OA_Code = "DeliverTo";

			xsdOrder.OrderDetail.ShipmentPlanning = new Xsd.OrderOrderDetailShipmentPlanning();
			xsdOrder.OrderDetail.ShipmentPlanning.HouseBill = "W22";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			xsdOrder.OrderDetail.ShipmentPlanning.Packs = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(11), "PK");
			xsdOrder.OrderDetail.ShipmentPlanning.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(22), "KG");
			xsdOrder.OrderDetail.ShipmentPlanning.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(33), "M3");
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVessel = "Vessel";
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVoyageFlight = "V33";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsAvailAt = "AvailAt";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDelivTo = "DeliverTo";

			var orderBizObj = Factory.New<Order>();
			var adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("Waybill", "W22", orderBizObj.JD_Waybill);
			AssertEquals("Goods Available At", "AUSYD", orderBizObj.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Goods Delivered To", "AUMEL", orderBizObj.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("Packs", 11, orderBizObj.JD_Packs);
			AssertEquals("Actual Weight", 22M, orderBizObj.JD_ActualWeight);
			AssertEquals("Actual Volume", 33M, orderBizObj.JD_ActualVolume);
			AssertEquals("Departure Vessel", "Vessel", orderBizObj.JD_RV_NKDepartureVessel);
			AssertEquals("Departure Voyage", "V33", orderBizObj.JD_DepartureVoyage);
			AssertNotNull(orderBizObj.GoodsAvailableAtAddress.Address);
			AssertEquals("Avail At", "AvailAt", orderBizObj.GoodsAvailableAtAddress.Address.OA_Code);
			AssertNotNull(orderBizObj.GoodsDeliveredToAddress.Address);
			AssertEquals("Deliver To", "DeliverTo", orderBizObj.GoodsDeliveredToAddress.Address.OA_Code);
		}

		public void TestImportOrderShipmentPlanning_WithNumberOfPackToBig()
		{
			var xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			var goodAvailableAtAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodAvailableAtAddress.OA_Code = "AvailAt";

			var goodDeliveredToAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodDeliveredToAddress.OA_Code = "DeliverTo";

			xsdOrder.OrderDetail.ShipmentPlanning = new Xsd.OrderOrderDetailShipmentPlanning();
			xsdOrder.OrderDetail.ShipmentPlanning.HouseBill = "W22";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			xsdOrder.OrderDetail.ShipmentPlanning.Packs = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(3333333333333333333), "PK");
			xsdOrder.OrderDetail.ShipmentPlanning.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(22), "KG");
			xsdOrder.OrderDetail.ShipmentPlanning.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(33), "M3");
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVessel = "Vessel";
			xsdOrder.OrderDetail.ShipmentPlanning.DepartureVoyageFlight = "V33";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsAvailAt = "AvailAt";
			xsdOrder.OrderDetail.ShipmentPlanning.GoodsDelivTo = "DeliverTo";

			var orderBizObj = Factory.New<Order>();
			orderBizObj.JD_OrderNumber = "ORDER_123";
			var adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("Should have raised error", "Error: Unsupported number of packs '3333333333333333333'; Order ORDER_123", Context.LastNotificationMessage);
		}

		public void TestImportOrderPlannedContainers()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderDetail.ShipmentPlanning = new Xsd.OrderOrderDetailShipmentPlanning();
			xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers = new Xsd.PlannedContainerCollection();
			Xsd.PlannedContainer plannedContainer = xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers.AddNew();
			plannedContainer.Number = "C44";
			plannedContainer.Quantity = 55;
			plannedContainer.Type = new Xsd.ContainerType();
			plannedContainer.Type.ISOCode = "2263";
			plannedContainer.Type.ContainerCode = "20FR";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("PlannedContainers.Count", 1, orderBizObj.PlannedContainers.Count);
			AssertEquals("Container Number", "C44", orderBizObj.PlannedContainers[0].J1_ContainerNumber);
			AssertEquals("Container Count", (ZShort)55, orderBizObj.PlannedContainers[0].J1_ContainerCount);
			AssertEquals("Container Type", "20FR", orderBizObj.PlannedContainers[0].Container.RC_Code);
		}

		public void TestImportOrderMilestones()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderDetail.Milestones = new Xsd.OrderOrderDetailMilestones();
			xsdOrder.OrderDetail.Milestones.Arrival = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 01), new ZDateTime(2005, 03, 02));
			xsdOrder.OrderDetail.Milestones.CartageAdvised = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 03), new ZDateTime(2005, 03, 04));
			xsdOrder.OrderDetail.Milestones.CustomsCommenced = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 05), new ZDateTime(2005, 03, 06));
			xsdOrder.OrderDetail.Milestones.CustomsFinalised = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 07), new ZDateTime(2005, 03, 08));
			xsdOrder.OrderDetail.Milestones.Delivery = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 09), new ZDateTime(2005, 03, 10));
			xsdOrder.OrderDetail.Milestones.Departure = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 11), new ZDateTime(2005, 03, 12));
			xsdOrder.OrderDetail.Milestones.ExFactory = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 13), new ZDateTime(2005, 03, 14));
			xsdOrder.OrderDetail.Milestones.OriginReceival = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 15), new ZDateTime(2005, 03, 16));
			xsdOrder.OrderDetail.Milestones.Unpacked = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 17), new ZDateTime(2005, 03, 18));

			xsdOrder.OrderDetail.Milestones.UserDate = new Xsd.MilestoneDatesCollection();
			Xsd.MilestoneDates userDate1 = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 19), new ZDateTime(2005, 03, 20));
			xsdOrder.OrderDetail.Milestones.UserDate.Add(userDate1);
			Xsd.MilestoneDates userDate2 = Xsd.MilestoneDates.FromEstimatedAndActual(new ZDateTime(2005, 03, 21), new ZDateTime(2005, 03, 22));
			xsdOrder.OrderDetail.Milestones.UserDate.Add(userDate2);

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("E Arrival", new DateTime(2005, 03, 01), orderBizObj.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
			AssertEquals("A Arrival", new DateTime(2005, 03, 02), orderBizObj.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals("E DeliveryCartageAdvised", new DateTime(2005, 03, 03), orderBizObj.GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("A DeliveryCartageAdvised", new DateTime(2005, 03, 04), orderBizObj.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime());
			AssertEquals("E CustomsCommenced", new DateTime(2005, 03, 05), orderBizObj.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("A CustomsCommenced", new DateTime(2005, 03, 06), orderBizObj.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime());
			AssertEquals("E CustomsCleared", new DateTime(2005, 03, 07), orderBizObj.GetMilestoneEstimatedDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("A CustomsCleared", new DateTime(2005, 03, 08), orderBizObj.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime());
			AssertEquals("E DeliveryCartageCompleteFinalised", new DateTime(2005, 03, 09), orderBizObj.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("A DeliveryCartageCompleteFinalised", new DateTime(2005, 03, 10), orderBizObj.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime());
			AssertEquals("E Departure", new DateTime(2005, 03, 11), orderBizObj.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals("A Departure", new DateTime(2005, 03, 12), orderBizObj.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals("E ExWorks", new DateTime(2005, 03, 13), orderBizObj.GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime());
			AssertEquals("A ExWorks", new DateTime(2005, 03, 14), orderBizObj.GetMilestoneActualDate(Events.ExWorks).ToZDateTime());
			AssertEquals("E GateIn", new DateTime(2005, 03, 15), orderBizObj.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime());
			AssertEquals("A GateIn", new DateTime(2005, 03, 16), orderBizObj.GetMilestoneActualDate(Events.GateIn).ToZDateTime());
			AssertEquals("E CargoAvailable", new DateTime(2005, 03, 17), orderBizObj.GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("A CargoAvailable", new DateTime(2005, 03, 18), orderBizObj.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime());
			AssertEquals("Custom Date 1 Est", new DateTime(2005, 03, 19), orderBizObj.JD_EstimateUserDate1);
			AssertEquals("Custom Date 2 Est", new DateTime(2005, 03, 21), orderBizObj.JD_EstimateUserDate2);
			AssertEquals("Custom Date 1 Actual", new DateTime(2005, 03, 20), orderBizObj.JD_ActualUserDate1);
			AssertEquals("Custom Date 2 Actual", new DateTime(2005, 03, 22), orderBizObj.JD_ActualUserDate2);
		}

		public void TestImportOrderExchangeRate()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderDetail.ExchangeRate = 1.23M;
			xsdOrder.OrderDetail.ExchangeRateSpecified = true;
			xsdOrder.OrderDetail.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.F;
			xsdOrder.OrderDetail.ExchRateBasisSpecified = true;

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Estimated Exchange Rate", 1.23M, orderBizObj.JD_EstimatedExchangeRate);

			xsdOrder.OrderDetail.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.L;
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Estimated Exchange Rate", Math.Round(1 / 1.23M, 8), orderBizObj.JD_EstimatedExchangeRate);

			xsdOrder.OrderDetail.ExchangeRate = 0M;
			Notifications.Clear();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Notify.HasErrors", true, Notifications.HasErrors);
			AssertEquals("Estimated Exchange Rate", Math.Round(1 / 1.23M, 8), orderBizObj.JD_EstimatedExchangeRate);

			xsdOrder.OrderDetail.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.F;
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("Estimated Exchange Rate", 0M, orderBizObj.JD_EstimatedExchangeRate);
		}

		public void TestImportOtherOrderDetails_DateEmptyOrInvalide()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.InvoiceDate = ZDateTime.Empty;
			xsdOrder.OrderDetail.OrderDateTime = ZDateTime.Invalid;

			Order orderBizObj = Factory.New<Order>();
			orderBizObj.JD_InvoiceDate = ZDateTime.BrettsBirthday;
			orderBizObj.JD_OrderDate = ZDateTime.BrettsBirthday.AddDays(1);
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("Invoice Date", ZDateTime.BrettsBirthday, orderBizObj.JD_InvoiceDate);
			AssertEquals("Order Date", ZDateTime.BrettsBirthday.AddDays(1), orderBizObj.JD_OrderDate);
		}

		public void TestImportOtherOrderDetails()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderDetail.ConfirmNumber = "R55";
			xsdOrder.OrderDetail.ContainerMode = Xsd.OrderContainerMode.FCL;
			xsdOrder.OrderDetail.ContainerModeSpecified = true;
			xsdOrder.OrderDetail.Description = "Goods Description";
			xsdOrder.OrderDetail.Incoterm = "I66";
			xsdOrder.OrderDetail.AdditionalTerms = "555";
			xsdOrder.OrderDetail.InvoiceNumber = "I77";
			xsdOrder.OrderDetail.InvoiceDate = new DateTime(2005, 03, 21);
			xsdOrder.OrderDetail.OrderDateTime = new DateTime(2005, 03, 23);
			xsdOrder.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Cancelled;
			xsdOrder.OrderDetail.OrderStatusSpecified = true;
			xsdOrder.OrderDetail.OrderTotal = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1000M), "AUD");
			xsdOrder.OrderDetail.TransportMode = Xsd.OrderTransportMode.SEA;
			xsdOrder.OrderDetail.TransportModeSpecified = true;
			xsdOrder.OrderDetail.CountryOfOrigin = "US";

			Xsd.DocAddress docAddress = xsdOrder.OrderDetail.DocAddresses.DocAddress.AddNew();
			docAddress.AddressLine1 = "1 STREET LINE 1";
			docAddress.AddressLine2 = "STREET LINE 2";
			docAddress.AddressType = Xsd.DocAddressAddressType.SCP;
			docAddress.CityOrSuburb = "CITY";
			docAddress.CompanyName = "Company Name";
			docAddress.ContactName = "JAY ALL BAH";
			docAddress.CountryCode = "AU";
			docAddress.Email = "email@domain.com";
			docAddress.PostCode = "01010";
			docAddress.StateOrProvince = "State";
			var telephoneNumber = docAddress.TelephoneNumbers.AddNew();
			telephoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephoneNumber.Value = "123 1233 12333";

			var goodAvailableAtDocAddress = xsdOrder.OrderDetail.DocAddresses.DocAddress.AddNew();
			goodAvailableAtDocAddress.AddressLine1 = "10 Pitt St";
			goodAvailableAtDocAddress.AddressLine2 = "Pitt St LINE 2";
			goodAvailableAtDocAddress.AddressType = Xsd.DocAddressAddressType.GAA;
			goodAvailableAtDocAddress.CityOrSuburb = "Sydney1";
			goodAvailableAtDocAddress.CompanyName = "Company1";
			goodAvailableAtDocAddress.ContactName = "Person1";
			goodAvailableAtDocAddress.CountryCode = "AU";
			goodAvailableAtDocAddress.Email = "email@company1.com";
			goodAvailableAtDocAddress.PostCode = "2000";
			goodAvailableAtDocAddress.StateOrProvince = "NSW";
			var telephoneNumber2 = goodAvailableAtDocAddress.TelephoneNumbers.AddNew();
			telephoneNumber2.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephoneNumber2.Value = "02 3333 3333";

			var goodDeliveredToDocAddress = xsdOrder.OrderDetail.DocAddresses.DocAddress.AddNew();
			goodDeliveredToDocAddress.AddressLine1 = "11 Pitt St";
			goodDeliveredToDocAddress.AddressLine2 = "Pitt St LINE 3";
			goodDeliveredToDocAddress.AddressType = Xsd.DocAddressAddressType.GDT;
			goodDeliveredToDocAddress.CityOrSuburb = "Sydney2";
			goodDeliveredToDocAddress.CompanyName = "Company2";
			goodDeliveredToDocAddress.ContactName = "Person2";
			goodDeliveredToDocAddress.CountryCode = "AU";
			goodDeliveredToDocAddress.Email = "email@company2.com";
			goodDeliveredToDocAddress.PostCode = "2001";
			goodDeliveredToDocAddress.StateOrProvince = "NSW";
			var telephoneNumber3 = goodDeliveredToDocAddress.TelephoneNumbers.AddNew();
			telephoneNumber3.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephoneNumber3.Value = "02 5555 5555";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("BookingConfRef", "R55", orderBizObj.JD_BookingConfRef);
			AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, orderBizObj.JD_ContainerMode);
			AssertEquals("GoodsDescription", "Goods Description", orderBizObj.JD_OrderGoodsDescription);
			AssertEquals("Inco Term", "I66", orderBizObj.JD_IncoTerm);
			AssertEquals("Additional Terms", "555", orderBizObj.JD_AdditionalTerms);
			AssertEquals("Invoice Number", "I77", orderBizObj.JD_InvoiceNumber);
			AssertEquals("Invoice Date", new DateTime(2005, 03, 21), orderBizObj.JD_InvoiceDate);
			AssertEquals("Order Date", new DateTime(2005, 03, 23), orderBizObj.JD_OrderDate);
			AssertEquals("Order Status", "CAN", orderBizObj.JD_OrderStatus);
			AssertEquals("Currency Code", "AUD", orderBizObj.JD_RX_NKOrderCurrency);
			AssertEquals("Country of Origin", "US", orderBizObj.JD_RN_NKCountryOfSupply);

			AssertEquals("DocAdressses DocAddress AddressLine1", "1 STREET LINE 1", orderBizObj.ControllingCustomerDocAddress.E2_Address1);
			AssertEquals("DocAdressses DocAddress AddressLine2", "STREET LINE 2", orderBizObj.ControllingCustomerDocAddress.E2_Address2);
			AssertEquals("DocAdressses DocAddress AddressType", "SCP", orderBizObj.ControllingCustomerDocAddress.E2_AddressType);
			AssertEquals("DocAdressses DocAddress CityOrSuburb", "CITY", orderBizObj.ControllingCustomerDocAddress.E2_City);
			AssertEquals("DocAdressses DocAddress CompanyName", "Company Name", orderBizObj.ControllingCustomerDocAddress.E2_CompanyName);
			AssertEquals("DocAdressses DocAddress ContactName", "JAY ALL BAH", orderBizObj.ControllingCustomerDocAddress.E2_Contact);
			AssertEquals("DocAdressses DocAddress CountryCode", "AU", orderBizObj.ControllingCustomerDocAddress.Country.Code);
			AssertEquals("DocAdressses DocAddress Email", "email@domain.com", orderBizObj.ControllingCustomerDocAddress.E2_Email);
			AssertEquals("DocAdressses DocAddress PostCode", "1 STREET LINE 1", orderBizObj.ControllingCustomerDocAddress.E2_Address1);
			AssertEquals("DocAdressses DocAddress StateOrProvince", "01010", orderBizObj.ControllingCustomerDocAddress.E2_Postcode);
			AssertEquals("DocAdressses DocAddress telephoneNumber", "123 1233 12333", orderBizObj.ControllingCustomerDocAddress.E2_Phone);

			AssertEquals("goodAvailableAtDocAddress AddressLine1", "10 Pitt St", orderBizObj.GoodsAvailableAtAddress.Address1);
			AssertEquals("goodAvailableAtDocAddress PostCode", "10 Pitt St", orderBizObj.GoodsAvailableAtAddress.E2_Address1);
			AssertEquals("goodAvailableAtDocAddress AddressLine2", "Pitt St LINE 2", orderBizObj.GoodsAvailableAtAddress.E2_Address2);
			AssertEquals("goodAvailableAtDocAddress AddressType", "GAA", orderBizObj.GoodsAvailableAtAddress.E2_AddressType);
			AssertEquals("goodAvailableAtDocAddress CityOrSuburb", "Sydney1", orderBizObj.GoodsAvailableAtAddress.E2_City);
			AssertEquals("goodAvailableAtDocAddress CompanyName", "Company1", orderBizObj.GoodsAvailableAtAddress.E2_CompanyName);
			AssertEquals("goodAvailableAtDocAddress ContactName", "Person1", orderBizObj.GoodsAvailableAtAddress.E2_Contact);
			AssertEquals("goodAvailableAtDocAddress CountryCode", "AU", orderBizObj.GoodsAvailableAtAddress.Country.Code);
			AssertEquals("goodAvailableAtDocAddress Email", "email@company1.com", orderBizObj.GoodsAvailableAtAddress.E2_Email);
			AssertEquals("goodAvailableAtDocAddress StateOrProvince", "2000", orderBizObj.GoodsAvailableAtAddress.E2_Postcode);
			AssertEquals("goodAvailableAtDocAddress telephoneNumber", "02 3333 3333", orderBizObj.GoodsAvailableAtAddress.E2_Phone);

			AssertEquals("goodDeliveredToDocAddress AddressLine1", "11 Pitt St", orderBizObj.GoodsDeliveredToAddress.Address1);
			AssertEquals("goodDeliveredToDocAddress PostCode", "11 Pitt St", orderBizObj.GoodsDeliveredToAddress.E2_Address1);
			AssertEquals("goodDeliveredToDocAddress AddressLine2", "Pitt St LINE 3", orderBizObj.GoodsDeliveredToAddress.E2_Address2);
			AssertEquals("goodDeliveredToDocAddress AddressType", "GDT", orderBizObj.GoodsDeliveredToAddress.E2_AddressType);
			AssertEquals("goodDeliveredToDocAddress CityOrSuburb", "Sydney2", orderBizObj.GoodsDeliveredToAddress.E2_City);
			AssertEquals("goodDeliveredToDocAddress CompanyName", "Company2", orderBizObj.GoodsDeliveredToAddress.E2_CompanyName);
			AssertEquals("goodDeliveredToDocAddress ContactName", "Person2", orderBizObj.GoodsDeliveredToAddress.E2_Contact);
			AssertEquals("goodDeliveredToDocAddress CountryCode", "AU", orderBizObj.GoodsDeliveredToAddress.Country.Code);
			AssertEquals("goodDeliveredToDocAddress Email", "email@company2.com", orderBizObj.GoodsDeliveredToAddress.E2_Email);
			AssertEquals("goodDeliveredToDocAddress StateOrProvince", "2001", orderBizObj.GoodsDeliveredToAddress.E2_Postcode);
			AssertEquals("goodDeliveredToDocAddress telephoneNumber", "02 5555 5555", orderBizObj.GoodsDeliveredToAddress.E2_Phone);
		}

		public void TestImportOtherOrderDetailsWithNoContMode()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderDetail.ConfirmNumber = "R55";
			xsdOrder.OrderDetail.ContainerModeSpecified = false;
			xsdOrder.OrderDetail.Description = "Goods Description";
			xsdOrder.OrderDetail.Incoterm = "I66";
			xsdOrder.OrderDetail.InvoiceNumber = "I77";
			xsdOrder.OrderDetail.OrderDateTime = new DateTime(2005, 03, 23);
			xsdOrder.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Cancelled;
			xsdOrder.OrderDetail.OrderStatusSpecified = true;
			xsdOrder.OrderDetail.OrderTotal = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(1000M), "AUD");
			xsdOrder.OrderDetail.TransportMode = Xsd.OrderTransportMode.SEA;
			xsdOrder.OrderDetail.TransportModeSpecified = true;
			xsdOrder.OrderDetail.CountryOfOrigin = "US";

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);

			AssertEquals("FCL", order.JD_ContainerMode);
		}

		public void TestImportCustomOrderDetails()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			ZDateTime now = ZDateTime.Now;
			xsdOrder.OrderDetail.Custom.Date1 = now;
			xsdOrder.OrderDetail.Custom.Date2 = now.AddDays(1);
			xsdOrder.OrderDetail.Custom.Decimal1 = 1.1m;
			xsdOrder.OrderDetail.Custom.Decimal1Specified = true;
			xsdOrder.OrderDetail.Custom.Decimal2 = 2.2m;
			xsdOrder.OrderDetail.Custom.Decimal2Specified = true;
			xsdOrder.OrderDetail.Custom.Decimal3 = 3.3m;
			xsdOrder.OrderDetail.Custom.Decimal3Specified = true;
			xsdOrder.OrderDetail.Custom.Decimal4 = 4.4m;
			xsdOrder.OrderDetail.Custom.Decimal4Specified = true;
			xsdOrder.OrderDetail.Custom.Decimal5 = 5.5m;
			xsdOrder.OrderDetail.Custom.Decimal5Specified = true;
			xsdOrder.OrderDetail.Custom.Flag1 = true;
			xsdOrder.OrderDetail.Custom.Flag1Specified = true;
			xsdOrder.OrderDetail.Custom.Flag2 = false;
			xsdOrder.OrderDetail.Custom.Flag2Specified = true;
			xsdOrder.OrderDetail.Custom.Flag3 = true;
			xsdOrder.OrderDetail.Custom.Flag3Specified = true;
			xsdOrder.OrderDetail.Custom.Flag4 = false;
			xsdOrder.OrderDetail.Custom.Flag4Specified = true;
			xsdOrder.OrderDetail.Custom.Flag5 = true;
			xsdOrder.OrderDetail.Custom.Flag5Specified = true;
			xsdOrder.OrderDetail.Custom.Text1 = "aa";
			xsdOrder.OrderDetail.Custom.Text2 = "bb";
			xsdOrder.OrderDetail.Custom.Text3 = "cc";
			xsdOrder.OrderDetail.Custom.Text4 = "dd";
			xsdOrder.OrderDetail.Custom.Text5 = "ee";
			xsdOrder.OrderDetail.Custom.Contact1 = "Contact1";
			xsdOrder.OrderDetail.Custom.Contact2 = "Contact2";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("CustomDate1", now.ToDateTime(), orderBizObj.JD_CustomDate1.ToDateTime());
			AssertEquals("CustomDate2", now.AddDays(1).ToDateTime(), orderBizObj.JD_CustomDate2.ToDateTime());
			AssertEquals("CustomDecimal1", 1.1m, orderBizObj.JD_CustomDecimal1);
			AssertEquals("CustomDecimal2", 2.2m, orderBizObj.JD_CustomDecimal2);
			AssertEquals("CustomDecimal3", 3.3m, orderBizObj.JD_CustomDecimal3);
			AssertEquals("CustomDecimal4", 4.4m, orderBizObj.JD_CustomDecimal4);
			AssertEquals("CustomDecimal5", 5.5m, orderBizObj.JD_CustomDecimal5);
			AssertEquals("CustomFlag1", true, orderBizObj.JD_CustomFlag1);
			AssertEquals("CustomFlag2", false, orderBizObj.JD_CustomFlag2);
			AssertEquals("CustomFlag3", true, orderBizObj.JD_CustomFlag3);
			AssertEquals("CustomFlag4", false, orderBizObj.JD_CustomFlag4);
			AssertEquals("CustomFlag5", true, orderBizObj.JD_CustomFlag5);
			AssertEquals("CustomAttrib1", "aa", orderBizObj.JD_CustomAttrib1);
			AssertEquals("CustomAttrib2", "bb", orderBizObj.JD_CustomAttrib2);
			AssertEquals("CustomAttrib3", "cc", orderBizObj.JD_CustomAttrib3);
			AssertEquals("CustomAttrib4", "dd", orderBizObj.JD_CustomAttrib4);
			AssertEquals("CustomAttrib5", "ee", orderBizObj.JD_CustomAttrib5);
			AssertEquals("Contact1", "Contact1", orderBizObj.JD_FirstBuyerContact);
			AssertEquals("Contact2", "Contact2", orderBizObj.JD_SecondBuyerContact);
		}

		public void TestImportDuplicateProduct()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "TestSupplier";
			testSupplier.OH_RL_NKClosestPort = "NZAKL";
			testSupplier.OH_Code = "TestSupplier";
			testSupplier.MainAddress.OA_Address1 = "Supplier";
			testSupplier.MainAddress.OA_Code = "ABCD";
			testSupplier.MainAddress.OA_City = "City";
			testSupplier.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			testSupplier.OH_IsConsignee = true;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P99";
			part.OP_Desc = "Part 99 Description";
			var buyerRelation = part.RelatedOrganisations.AddNew();
			buyerRelation.OU_OH = Buyer.PK;
			buyerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var supplierRelation = part.RelatedOrganisations.AddNew();
			supplierRelation.OU_OH = Supplier.PK;
			supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var createMissingProductRegistry = SystemDataRegistry.Instance.CreateMissingProductWithRelationship;
			using (createMissingProductRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CreateMissingProductsInfo() { IsOverrideToYes = true, DefaultRelationship = DefaultRalationshipCodes.Both }))
			{
				var xsdOrder = CreateXsdOrder("Order1", 1, "P99", 1, 1, 0, 0);
				xsdOrder.OrderDetail.Supplier = new Xsd.Organisation
				{
					OwnerCode = testSupplier.OH_Code
				};
				OrgMatch = SetOrgMatch(testSupplier, testSupplier.OH_Code);

				var order = Factory.New<Order>();
				var adapter = new TestOrderValueObjectDataAdapter();
				adapter.ImportFromValueObject(order, xsdOrder, Context);

				AssertContains("Warning: Could not add Product P99 as it would result in a duplicate.", ((NotificationBuffer)Context.Notifications).AsString);
			}
		}

		public void TestCalculateLineTotal()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";
			xsdOrderLine.OrderLineDetail.DropDate = new DateTime(2005, 03, 24);
			xsdOrderLine.OrderLineDetail.InnerPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(45M), "BAG");
			xsdOrderLine.OrderLineDetail.OuterPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(50M), "CTN");
			xsdOrderLine.OrderLineDetail.Product = "P99";

			decimal itemPrice = 22M;
			xsdOrderLine.OrderLineDetail.ItemPrice.Value = itemPrice;
			xsdOrderLine.OrderLineDetail.ItemPrice.IsSpecified = true;

			decimal qtyOrder = 10M;
			xsdOrderLine.OrderLineDetail.QtyOrdered = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(qtyOrder), "");
			xsdOrderLine.OrderLineDetail.QtyOrdered.IsSpecified = true;

			xsdOrderLine.OrderLineDetail.LineStatus = Core.Constants.OrderStatus.Delivered;
			xsdOrderLine.OrderLineDetail.LineStatusSpecified = true;
			xsdOrderLine.OrderLineDetail.PartAttrib1 = "PartAttrib1";
			xsdOrderLine.OrderLineDetail.PartAttrib2 = "PartAttrib2";
			xsdOrderLine.OrderLineDetail.PartAttrib3 = "PartAttrib3";
			xsdOrderLine.OrderLineDetail.ContainerNumber = "ContainerNo";
			xsdOrderLine.OrderLineDetail.CommercialInvoiceNo = "123";
			xsdOrderLine.OrderLineDetail.ContainerPackingOrder = 1;
			xsdOrderLine.OrderLineDetail.CountryOfOrigin = "AU";
			Xsd.HazardousGoods haz = xsdOrderLine.OrderLineDetail.DangerousGoods.AddNew();
			haz.FlashPoint = "0.1";
			haz.UNDGCode = Substance.DG_Code;

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals(itemPrice * qtyOrder, order.OrderLines[0].JO_LinePrice);
		}

		public void TestCalculateItemPrice()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";
			xsdOrderLine.OrderLineDetail.DropDate = new DateTime(2005, 03, 24);
			xsdOrderLine.OrderLineDetail.InnerPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(45M), "BAG");
			ZDecimal linePrice = 9500M;
			xsdOrderLine.OrderLineDetail.LinePrice.Value = linePrice;
			xsdOrderLine.OrderLineDetail.LinePrice.IsSpecified = true;
			xsdOrderLine.OrderLineDetail.OuterPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(50M), "CTN");
			xsdOrderLine.OrderLineDetail.Product = "P99";
			ZDecimal qtyOrdered = 1000M;
			xsdOrderLine.OrderLineDetail.QtyOrdered = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(qtyOrdered), "");
			xsdOrderLine.OrderLineDetail.LineStatus = Core.Constants.OrderStatus.Delivered;
			xsdOrderLine.OrderLineDetail.LineStatusSpecified = true;
			xsdOrderLine.OrderLineDetail.PartAttrib1 = "PartAttrib1";
			xsdOrderLine.OrderLineDetail.PartAttrib2 = "PartAttrib2";
			xsdOrderLine.OrderLineDetail.PartAttrib3 = "PartAttrib3";
			xsdOrderLine.OrderLineDetail.ContainerNumber = "ContainerNo";
			xsdOrderLine.OrderLineDetail.CommercialInvoiceNo = "123";
			xsdOrderLine.OrderLineDetail.ContainerPackingOrder = 1;
			xsdOrderLine.OrderLineDetail.CountryOfOrigin = "AU";

			Xsd.HazardousGoods haz = xsdOrderLine.OrderLineDetail.DangerousGoods.AddNew();
			haz.FlashPoint = "0.1";
			haz.UNDGCode = Substance.DG_Code;

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals(linePrice / qtyOrdered, order.OrderLines[0].JO_ItemPrice);
		}

		public void TestCalculateQuantity()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";
			xsdOrderLine.OrderLineDetail.DropDate = new DateTime(2005, 03, 24);
			xsdOrderLine.OrderLineDetail.InnerPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(45M), "BAG");
			ZDecimal linePrice = 9500M;
			xsdOrderLine.OrderLineDetail.LinePrice.Value = linePrice;
			xsdOrderLine.OrderLineDetail.LinePrice.IsSpecified = true;
			xsdOrderLine.OrderLineDetail.OuterPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(50M), "CTN");
			xsdOrderLine.OrderLineDetail.Product = "P99";
			decimal itemPrice = 100M;
			xsdOrderLine.OrderLineDetail.ItemPrice.Value = itemPrice;
			xsdOrderLine.OrderLineDetail.ItemPrice.IsSpecified = true;
			xsdOrderLine.OrderLineDetail.LineStatus = Core.Constants.OrderStatus.Delivered;
			xsdOrderLine.OrderLineDetail.LineStatusSpecified = true;
			xsdOrderLine.OrderLineDetail.PartAttrib1 = "PartAttrib1";
			xsdOrderLine.OrderLineDetail.PartAttrib2 = "PartAttrib2";
			xsdOrderLine.OrderLineDetail.PartAttrib3 = "PartAttrib3";
			xsdOrderLine.OrderLineDetail.ContainerNumber = "ContainerNo";
			xsdOrderLine.OrderLineDetail.CommercialInvoiceNo = "123";
			xsdOrderLine.OrderLineDetail.ContainerPackingOrder = 1;
			xsdOrderLine.OrderLineDetail.CountryOfOrigin = "AU";

			Xsd.HazardousGoods haz = xsdOrderLine.OrderLineDetail.DangerousGoods.AddNew();
			haz.FlashPoint = "0.1";
			haz.UNDGCode = Substance.DG_Code;

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);
			AssertEquals(linePrice / itemPrice, order.OrderLines[0].JO_Quantity);
		}

		public void TestImportOrderLines()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";
			xsdOrderLine.OrderLineDetail.DropDate = new DateTime(2005, 03, 24);
			xsdOrderLine.OrderLineDetail.InnerPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(45M), "BAG");
			xsdOrderLine.OrderLineDetail.ItemPrice.Value = 1235M;
			xsdOrderLine.OrderLineDetail.ItemPrice.IsSpecified = true;
			xsdOrderLine.OrderLineDetail.LinePrice.Value = 9500M;
			xsdOrderLine.OrderLineDetail.LinePrice.IsSpecified = true;
			xsdOrderLine.OrderLineDetail.OuterPacks = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(50M), "CTN");
			xsdOrderLine.OrderLineDetail.Weight = 33.3m;
			xsdOrderLine.OrderLineDetail.WeightType = "KG";
			xsdOrderLine.OrderLineDetail.WeightSpecified = true;
			xsdOrderLine.OrderLineDetail.WeightTypeSpecified = true;
			xsdOrderLine.OrderLineDetail.Volume = 44.4m;
			xsdOrderLine.OrderLineDetail.VolumeType = "M3";
			xsdOrderLine.OrderLineDetail.VolumeSpecified = true;
			xsdOrderLine.OrderLineDetail.VolumeTypeSpecified = true;
			xsdOrderLine.OrderLineDetail.Product = "P99";
			xsdOrderLine.OrderLineDetail.QtyOrdered = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(1000M), "");
			xsdOrderLine.OrderLineDetail.LineStatus = Core.Constants.OrderStatus.Delivered;
			xsdOrderLine.OrderLineDetail.LineStatusSpecified = true;
			xsdOrderLine.OrderLineDetail.PartAttrib1 = "PartAttrib1";
			xsdOrderLine.OrderLineDetail.PartAttrib2 = "PartAttrib2";
			xsdOrderLine.OrderLineDetail.PartAttrib3 = "PartAttrib3";
			xsdOrderLine.OrderLineDetail.ContainerNumber = "ContainerNo";
			xsdOrderLine.OrderLineDetail.CommercialInvoiceNo = "123";
			xsdOrderLine.OrderLineDetail.SpecialInstructions = "SpecialInstructions";
			xsdOrderLine.OrderLineDetail.AdditionalInformation = "AdditionalInformation";
			xsdOrderLine.OrderLineDetail.ContainerPackingOrder = 1;
			xsdOrderLine.OrderLineDetail.CountryOfOrigin = "AU";

			Xsd.HazardousGoods haz = xsdOrderLine.OrderLineDetail.DangerousGoods.AddNew();
			haz.FlashPoint = "0.1";
			haz.UNDGCode = Substance.DG_Code;

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("Line No", 200, orderBizObjLine.JO_LineNo);
			AssertEquals("Line Description", "Line Description", orderBizObjLine.JO_Description);
			AssertEquals("Required Date", new DateTime(2005, 03, 24), orderBizObjLine.JO_LineDropDate);
			AssertEquals("Inner Packs", 45M, orderBizObjLine.JO_InnerPacks);
			AssertEquals("Inner Packs Type", "BAG", orderBizObjLine.JO_InnerPacksUQ);
			AssertEquals("Item Price", 1235M, orderBizObjLine.JO_ItemPrice);
			AssertEquals("Line Price", 9500M, orderBizObjLine.JO_LinePrice);
			AssertEquals("Outer Packs", 50M, orderBizObjLine.JO_OuterPacks);
			AssertEquals("Outer Packs Type", "CTN", orderBizObjLine.JO_OuterPacksUQ);
			AssertEquals("Weight", 33.3M, orderBizObjLine.JO_ActualWeight);
			AssertEquals("Volume", 44.4M, orderBizObjLine.JO_ActualVolume);
			AssertEquals("KG", orderBizObjLine.JO_UnitOfWeight);
			AssertEquals("M3", orderBizObjLine.JO_UnitOfVolume);

			AssertEquals("Part No", "P99", orderBizObjLine.JO_Partno);
			AssertEquals("Quantity", 1000M, orderBizObjLine.JO_Quantity);
			AssertEquals("SpecialInstructions", "SpecialInstructions", orderBizObjLine.JO_SpecialInstructions);
			AssertEquals("AdditionalInformation", "AdditionalInformation", orderBizObjLine.JO_AdditionalInformation);
			AssertEquals("Line Status", Core.Constants.OrderStatus.Delivered, orderBizObjLine.JO_LineStatus);
			AssertEquals("Part Attrib 1", "PartAttrib1", orderBizObjLine.JO_PartAttrib1);
			AssertEquals("Part Attrib 2", "PartAttrib2", orderBizObjLine.JO_PartAttrib2);
			AssertEquals("Part Attrib 3", "PartAttrib3", orderBizObjLine.JO_PartAttrib3);

			AssertEquals("Part Attrib 3", "ContainerNo", orderBizObjLine.JO_ContainerNumber);
			AssertEquals("Part Attrib 3", "123", orderBizObjLine.JO_CommercialInvoiceNo);
			AssertEquals("Part Attrib 3", 1, orderBizObjLine.JO_ContainerPackingOrder);
			AssertEquals("Part Attrib 3", "AU", orderBizObjLine.JO_RN_NKCountryOfOrigin);
			AssertEquals("Part Attrib 3", 0.1m, orderBizObjLine.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("Part Attrib 3", Substance.DG_Code, orderBizObjLine.UNDGs[0].Substance.DG_Code);
		}

		public void TestImportOrderLines_WithInvalidOrderLineNo()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();

			xsdOrderLine.OrderLineNo = 0;

			Order order = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, xsdOrder, Context);

			AssertEquals("Should have raised Line Number error", Context.LastNotificationMessage, "Warning: Line Number is not greater than or equal to 1 in Order Line. It will take the next valid Line Number.");
		}

		public void TestImportOrderLines_WithInvalidUNDG()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";

			Xsd.HazardousGoods haz = xsdOrderLine.OrderLineDetail.DangerousGoods.AddNew();
			haz.UNDGCode = "ZUB12";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("Line No", 200, orderBizObjLine.JO_LineNo);
			AssertEquals("Line Description", "Line Description", orderBizObjLine.JO_Description);
			AssertEquals("UNDG Code should be blank when it was invalid", orderBizObjLine.UNDGs.Count, 0);
			AssertEquals("1 exception generated for invalid UNDG", 1, ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions.Count);
			AssertEquals("Correct exception description for invalid UNDG", "Invalid or multiple UNDG for Line 200 - 'ZUB12'", ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_Description);
			AssertEquals("Correct exception description for invalid UNDG", "Invalid or multiple UNDG for Line 200 - 'ZUB12'", ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_Notes.ToAscii());
			AssertEquals("Correct exception event", ProcessWorkflowExceptionType.ExceptionDataConversionIssue, ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
		}

		public void TestImportOrderLines_WithInvalidUNDG_Legacy()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			xsdOrderLine.OrderLineDetail.Description = "Line Description";
			xsdOrderLine.OrderLineDetail.DGSubstanceCode = "ZUB12";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("Line No", 200, orderBizObjLine.JO_LineNo);
			AssertEquals("Line Description", "Line Description", orderBizObjLine.JO_Description);
			AssertEquals("UNDG Code should be blanked when it was invalid", 0, orderBizObjLine.UNDGs.Count);
			AssertEquals("1 exception generated for invalid UNDG", 1, ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions.Count);
			AssertEquals("Correct exception description for invalid UNDG", "Invalid or multiple UNDG for Line 200 - 'ZUB12'", ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_Description);
			AssertEquals("Correct exception description for invalid UNDG", "Invalid or multiple UNDG for Line 200 - 'ZUB12'", ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_Notes.ToAscii());
			AssertEquals("Correct exception event", ProcessWorkflowExceptionType.ExceptionDataConversionIssue, ((IWorkflowProvider)orderBizObjLine.Order).WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
		}

		public void TestImportCustomOrderlineDetails()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			ZDateTime now = ZDateTime.Now;
			xsdOrderLine.OrderLineDetail.Custom.Date1 = now;
			xsdOrderLine.OrderLineDetail.Custom.Date2 = now.AddDays(1);
			xsdOrderLine.OrderLineDetail.Custom.Date3 = now.AddDays(2);
			xsdOrderLine.OrderLineDetail.Custom.Date4 = now.AddDays(3);
			xsdOrderLine.OrderLineDetail.Custom.Date5 = now.AddDays(4);
			xsdOrderLine.OrderLineDetail.Custom.Decimal1 = 1.1m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal1Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal2 = 2.2m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal2Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal3 = 3.3m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal3Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal4 = 4.4m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal4Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal5 = 5.5m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal5Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag1 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag1Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag2 = false;
			xsdOrderLine.OrderLineDetail.Custom.Flag2Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag3 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag3Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag4 = false;
			xsdOrderLine.OrderLineDetail.Custom.Flag4Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag5 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag5Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Text1 = "aa";
			xsdOrderLine.OrderLineDetail.Custom.Text2 = "bb";
			xsdOrderLine.OrderLineDetail.Custom.Text3 = "cc";
			xsdOrderLine.OrderLineDetail.Custom.Text4 = "dd";
			xsdOrderLine.OrderLineDetail.Custom.Text5 = "ee";
			xsdOrderLine.OrderLineDetail.Custom.Text6 = "ff";
			xsdOrderLine.OrderLineDetail.Custom.CustomText1 = "CT1";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("CustomDate1", now.ToDateTime(), orderBizObjLine.JO_CustomDate1.ToDateTime());
			AssertEquals("CustomDate2", now.AddDays(1).ToDateTime(), orderBizObjLine.JO_CustomDate2.ToDateTime());
			AssertEquals("CustomDate3", now.AddDays(2).ToDateTime(), orderBizObjLine.JO_CustomDate3.ToDateTime());
			AssertEquals("CustomDate4", now.AddDays(3).ToDateTime(), orderBizObjLine.JO_CustomDate4.ToDateTime());
			AssertEquals("CustomDate5", now.AddDays(4).ToDateTime(), orderBizObjLine.JO_CustomDate5.ToDateTime());
			AssertEquals("CustomDecimal1", 1.1m, orderBizObjLine.JO_CustomDecimal1);
			AssertEquals("CustomDecimal2", 2.2m, orderBizObjLine.JO_CustomDecimal2);
			AssertEquals("CustomDecimal3", 3.3m, orderBizObjLine.JO_CustomDecimal3);
			AssertEquals("CustomDecimal4", 4.4m, orderBizObjLine.JO_CustomDecimal4);
			AssertEquals("CustomDecimal5", 5.5m, orderBizObjLine.JO_CustomDecimal5);
			AssertEquals("CustomFlag1", true, orderBizObjLine.JO_CustomFlag1);
			AssertEquals("CustomFlag2", false, orderBizObjLine.JO_CustomFlag2);
			AssertEquals("CustomFlag3", true, orderBizObjLine.JO_CustomFlag3);
			AssertEquals("CustomFlag4", false, orderBizObjLine.JO_CustomFlag4);
			AssertEquals("CustomFlag5", true, orderBizObjLine.JO_CustomFlag5);
			AssertEquals("CustomAttrib1", "aa", orderBizObjLine.JO_CustomAttrib1);
			AssertEquals("CustomAttrib2", "bb", orderBizObjLine.JO_CustomAttrib2);
			AssertEquals("CustomAttrib3", "cc", orderBizObjLine.JO_CustomAttrib3);
			AssertEquals("CustomAttrib4", "dd", orderBizObjLine.JO_CustomAttrib4);
			AssertEquals("CustomAttrib5", "ee", orderBizObjLine.JO_CustomAttrib5);
			AssertEquals("CustomAttrib6", "ff", orderBizObjLine.JO_CustomAttrib6);
			AssertEquals("CustomTextBlob1", "CT1", orderBizObjLine.JO_CustomTextBlob1);
		}

		public void TestImportCustomOrderlineDetailsMissingCustom5()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			ZDateTime now = ZDateTime.Now;
			xsdOrderLine.OrderLineDetail.Custom.Date1 = now;
			xsdOrderLine.OrderLineDetail.Custom.Date2 = now.AddDays(1);
			xsdOrderLine.OrderLineDetail.Custom.Date3 = now.AddDays(2);
			xsdOrderLine.OrderLineDetail.Custom.Date4 = now.AddDays(3);
			xsdOrderLine.OrderLineDetail.Custom.Date5 = now.AddDays(4);
			xsdOrderLine.OrderLineDetail.Custom.Decimal1 = 1.1m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal1Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal2 = 2.2m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal2Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal3 = 3.3m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal3Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal4 = 4.4m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal4Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Decimal5 = 5.5m;
			xsdOrderLine.OrderLineDetail.Custom.Decimal5Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag1 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag1Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag2 = false;
			xsdOrderLine.OrderLineDetail.Custom.Flag2Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag3 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag3Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag4 = false;
			xsdOrderLine.OrderLineDetail.Custom.Flag4Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag5 = true;
			xsdOrderLine.OrderLineDetail.Custom.Flag5Specified = true;
			xsdOrderLine.OrderLineDetail.Custom.Text1 = "aa";
			xsdOrderLine.OrderLineDetail.Custom.Text2 = "bb";
			xsdOrderLine.OrderLineDetail.Custom.Text3 = "cc";
			xsdOrderLine.OrderLineDetail.Custom.Text4 = "dd";
			xsdOrderLine.OrderLineDetail.Custom.Text6 = "ff";
			xsdOrderLine.OrderLineDetail.Custom.CustomText1 = "CT1";

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("CustomDate1", now.ToDateTime(), orderBizObjLine.JO_CustomDate1.ToDateTime());
			AssertEquals("CustomDate2", now.AddDays(1).ToDateTime(), orderBizObjLine.JO_CustomDate2.ToDateTime());
			AssertEquals("CustomDate3", now.AddDays(2).ToDateTime(), orderBizObjLine.JO_CustomDate3.ToDateTime());
			AssertEquals("CustomDate4", now.AddDays(3).ToDateTime(), orderBizObjLine.JO_CustomDate4.ToDateTime());
			AssertEquals("CustomDate5", now.AddDays(4).ToDateTime(), orderBizObjLine.JO_CustomDate5.ToDateTime());
			AssertEquals("CustomDecimal1", 1.1m, orderBizObjLine.JO_CustomDecimal1);
			AssertEquals("CustomDecimal2", 2.2m, orderBizObjLine.JO_CustomDecimal2);
			AssertEquals("CustomDecimal3", 3.3m, orderBizObjLine.JO_CustomDecimal3);
			AssertEquals("CustomDecimal4", 4.4m, orderBizObjLine.JO_CustomDecimal4);
			AssertEquals("CustomDecimal5", 5.5m, orderBizObjLine.JO_CustomDecimal5);
			AssertEquals("CustomFlag1", true, orderBizObjLine.JO_CustomFlag1);
			AssertEquals("CustomFlag2", false, orderBizObjLine.JO_CustomFlag2);
			AssertEquals("CustomFlag3", true, orderBizObjLine.JO_CustomFlag3);
			AssertEquals("CustomFlag4", false, orderBizObjLine.JO_CustomFlag4);
			AssertEquals("CustomFlag5", true, orderBizObjLine.JO_CustomFlag5);
			AssertEquals("CustomAttrib1", "aa", orderBizObjLine.JO_CustomAttrib1);
			AssertEquals("CustomAttrib2", "bb", orderBizObjLine.JO_CustomAttrib2);
			AssertEquals("CustomAttrib3", "cc", orderBizObjLine.JO_CustomAttrib3);
			AssertEquals("CustomAttrib4", "dd", orderBizObjLine.JO_CustomAttrib4);
			AssertEquals("CustomAttrib5", "", orderBizObjLine.JO_CustomAttrib5);
			AssertEquals("CustomAttrib6", "ff", orderBizObjLine.JO_CustomAttrib6);
			AssertEquals("CustomTextBlob1", "CT1", orderBizObjLine.JO_CustomTextBlob1);
		}

		public void TestImportOrderLineDeliveries()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "Buyer";
			org.MainAddress.OA_Address1 = "123 XYZ STREET";
			Factory.Save();

			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineDeliveries = new Xsd.OrderOrderLineOrderLineDeliveryCollection();
			Xsd.OrderOrderLineOrderLineDelivery xsdDelivery = xsdOrderLine.OrderLineDeliveries.AddNew();
			xsdDelivery.DeliveryDetails = new Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetails();
			xsdDelivery.DeliveryDetails.Address = new Xsd.AddressReference();
			xsdDelivery.DeliveryDetails.Address.AddressSequenceRef = 1;
			xsdDelivery.DeliveryDetails.Address.Organisation = new Xsd.Organisation();
			xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails = new Xsd.OrganisationDetail();
			xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Name = "Address Organisation";
			xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress xsdAddress = xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.AddNew();
			xsdAddress.Sequence = 1;
			xsdAddress.SequenceSpecified = true;
			Xsd.AddressCapability capability = xsdAddress.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			capability.AddressTypeSpecified = true;
			xsdAddress.AddressLine1 = "123 XYZ STREET";
			xsdDelivery.DeliveryDetails.DelPort = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			xsdDelivery.DeliveryDetails.QtyAllocated = 2000M;

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("Deliveries Count", 1, orderBizObjLine.Deliveries.Count);
			OrderLineDelivery bizObjDelivery = orderBizObjLine.Deliveries[0];
			AssertEquals("Delivery Point", "123 XYZ STREET", bizObjDelivery.J4_OA_NKDeliveryPoint);
			AssertEquals("Delivery Port", "AUSYD", bizObjDelivery.J4_RL_NKDestinationPort);
			AssertEquals("Allocated", 2000M, bizObjDelivery.J4_Allocated);
		}

		public void TestImportOrderLineDeliveryCustomDetails()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineNo = 200;
			xsdOrderLine.OrderSubLineNo = 1;

			xsdOrderLine.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();
			Xsd.OrderOrderLineOrderLineDelivery xsdDeliveryLine = xsdOrderLine.OrderLineDeliveries.AddNew();
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal1 = 0.5m;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal1Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal2 = 0.4m;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal2Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal3 = 0.3m;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal3Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal4 = 0.2m;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal4Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal5 = 0.1m;
			xsdDeliveryLine.DeliveryDetails.Custom.Decimal5Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Text1 = "Text1";
			xsdDeliveryLine.DeliveryDetails.Custom.Text2 = "Text2";
			xsdDeliveryLine.DeliveryDetails.Custom.Text3 = "Text3";
			xsdDeliveryLine.DeliveryDetails.Custom.Text4 = "Text4";
			xsdDeliveryLine.DeliveryDetails.Custom.Text5 = "Text5";
			xsdDeliveryLine.DeliveryDetails.Custom.Flag1 = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag1Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag2 = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag2Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag3 = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag3Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag4 = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag4Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag5 = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Flag5Specified = true;
			xsdDeliveryLine.DeliveryDetails.Custom.Date1 = new ZDateTime(2009, 1, 1);
			xsdDeliveryLine.DeliveryDetails.Custom.Date2 = new ZDateTime(2009, 2, 2);
			xsdDeliveryLine.DeliveryDetails.Custom.Date3 = new ZDateTime(2009, 3, 3);
			xsdDeliveryLine.DeliveryDetails.Custom.Date4 = new ZDateTime(2009, 4, 4);
			xsdDeliveryLine.DeliveryDetails.Custom.Date5 = new ZDateTime(2009, 5, 5);

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(xsdOrder, new ValueObjectImportContext(Factory, Notifications));

			AssertNotNull("order is created", order);
			AssertEquals("no of order lines", 1, order.OrderLines.Count);
			AssertEquals("no of order delivery", 1, order.OrderLines[0].Deliveries.Count);

			OrderLineDelivery deliveryLine = order.OrderLines[0].Deliveries[0];
			AssertEquals("Custom Decimal 1", xsdDeliveryLine.DeliveryDetails.Custom.Decimal1, deliveryLine.J4_CustomDecimal1);
			AssertEquals("Custom Decimal 2", xsdDeliveryLine.DeliveryDetails.Custom.Decimal2, deliveryLine.J4_CustomDecimal2);
			AssertEquals("Custom Decimal 3", xsdDeliveryLine.DeliveryDetails.Custom.Decimal3, deliveryLine.J4_CustomDecimal3);
			AssertEquals("Custom Decimal 4", xsdDeliveryLine.DeliveryDetails.Custom.Decimal4, deliveryLine.J4_CustomDecimal4);
			AssertEquals("Custom Decimal 5", xsdDeliveryLine.DeliveryDetails.Custom.Decimal5, deliveryLine.J4_CustomDecimal5);
			AssertEquals("Custom Text 1", xsdDeliveryLine.DeliveryDetails.Custom.Text1, deliveryLine.J4_CustomAttribute1);
			AssertEquals("Custom Text 2", xsdDeliveryLine.DeliveryDetails.Custom.Text2, deliveryLine.J4_CustomAttribute2);
			AssertEquals("Custom Text 3", xsdDeliveryLine.DeliveryDetails.Custom.Text3, deliveryLine.J4_CustomAttribute3);
			AssertEquals("Custom Text 4", xsdDeliveryLine.DeliveryDetails.Custom.Text4, deliveryLine.J4_CustomAttribute4);
			AssertEquals("Custom Text 5", xsdDeliveryLine.DeliveryDetails.Custom.Text5, deliveryLine.J4_CustomAttribute5);
			AssertEquals("Custom Flag 1", xsdDeliveryLine.DeliveryDetails.Custom.Flag1, deliveryLine.J4_CustomFlag1);
			AssertEquals("Custom Flag 2", xsdDeliveryLine.DeliveryDetails.Custom.Flag2, deliveryLine.J4_CustomFlag2);
			AssertEquals("Custom Flag 3", xsdDeliveryLine.DeliveryDetails.Custom.Flag3, deliveryLine.J4_CustomFlag3);
			AssertEquals("Custom Flag 4", xsdDeliveryLine.DeliveryDetails.Custom.Flag4, deliveryLine.J4_CustomFlag4);
			AssertEquals("Custom Flag 5", xsdDeliveryLine.DeliveryDetails.Custom.Flag5, deliveryLine.J4_CustomFlag5);
			AssertEquals("Custom Date 1", xsdDeliveryLine.DeliveryDetails.Custom.Date1, new ZDateTime(2009, 1, 1));
			AssertEquals("Custom Date 2", xsdDeliveryLine.DeliveryDetails.Custom.Date2, new ZDateTime(2009, 2, 2));
			AssertEquals("Custom Date 3", xsdDeliveryLine.DeliveryDetails.Custom.Date3, new ZDateTime(2009, 3, 3));
			AssertEquals("Custom Date 4", xsdDeliveryLine.DeliveryDetails.Custom.Date4, new ZDateTime(2009, 4, 4));
			AssertEquals("Custom Date 5", xsdDeliveryLine.DeliveryDetails.Custom.Date5, new ZDateTime(2009, 5, 5));
		}

		public void TestImportOrderLineDeliveryContainers()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			xsdOrder.OrderLines = new Xsd.OrderOrderLineCollection();
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines.AddNew();
			xsdOrderLine.OrderLineDeliveries = new Xsd.OrderOrderLineOrderLineDeliveryCollection();
			Xsd.OrderOrderLineOrderLineDelivery xsdDelivery = xsdOrderLine.OrderLineDeliveries.AddNew();
			xsdDelivery.DeliveryContainers = new Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainerCollection();
			Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainer xsdContainer = xsdDelivery.DeliveryContainers.AddNew();
			xsdContainer.Container = new Xsd.Container();
			xsdContainer.Container.ContainerNumber = "C00";
			xsdContainer.Container.ContainerType = new Xsd.ContainerType();
			xsdContainer.Container.ContainerType.ISOCode = "22P0";
			xsdContainer.Container.PackingMode = Xsd.ContainerMode.FCL;
			xsdContainer.Container.Seal = "S11";
			xsdContainer.ETA = new DateTime(2005, 03, 25);
			xsdContainer.ETD = new DateTime(2005, 03, 26);
			xsdContainer.LoadPort = Xsd.UNLOCO.FromPortCode(Factory, "AUMEL");
			xsdContainer.MasterBillNo = "M44";
			xsdContainer.PackCount = 28;
			xsdContainer.PackCountSpecified = true;
			xsdContainer.PackType = "PKG";
			xsdContainer.Vessel = "Container Vessel";
			xsdContainer.Voyage = "V22";
			xsdContainer.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(77M), "M3");
			xsdContainer.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(1300M), "KG");

			Order orderBizObj = Factory.New<Order>();
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);

			AssertEquals("OrderLines Count", 1, orderBizObj.OrderLines.Count);
			OrderLine orderBizObjLine = orderBizObj.OrderLines[0];
			AssertEquals("Deliveries Count", 1, orderBizObjLine.Deliveries.Count);
			OrderLineDelivery bizObjDelivery = orderBizObjLine.Deliveries[0];
			AssertEquals("Containers Count", 1, bizObjDelivery.Containers.Count);
			OrderLineDeliverContainer bizObjContainer = bizObjDelivery.Containers[0];

			AssertEquals("Container No", "C00", bizObjContainer.J5_ContainerNum);
			AssertEquals("Container Type", "20PL", bizObjContainer.J5_RC_NKContainerType);
			AssertEquals("Container Seal", "S11", bizObjContainer.J5_ContainerSeal);
			AssertEquals("ETA", new DateTime(2005, 03, 25), bizObjContainer.J5_ETA);
			AssertEquals("ETD", new DateTime(2005, 03, 26), bizObjContainer.J5_ETD);
			AssertEquals("Load Port", "AUMEL", bizObjContainer.J5_RL_NKLoadPort);
			AssertEquals("Master Bill", "M44", bizObjContainer.J5_MasterBill);
			AssertEquals("Pack Count", (ZShort)28, bizObjContainer.J5_PackCount);
			AssertEquals("Pack UQ", "PKG", bizObjContainer.J5_F3_NKPackType);
			AssertEquals("Arrival Vessel", "Container Vessel", bizObjContainer.J5_RV_NKArrivalVessel);
			AssertEquals("Voyage", "V22", bizObjContainer.J5_Voyage);
			AssertEquals("Volume", 77M, bizObjContainer.J5_Volume);
			AssertEquals("Volume UQ", "M3", bizObjContainer.J5_VolumeUQ);
			AssertEquals("Weight", 1300M, bizObjContainer.J5_Weight);
			AssertEquals("Weight UQ", "KG", bizObjContainer.J5_WeightUQ);
		}

		public void TestImportOrderGetsLinkedToShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_UniqueConsignRef = "S0000001";
			Factory.Save();

			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail.ReferenceNumber.Value = "S0000001";
			xsdOrder.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Shipment;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			AssertEquals("Order was linked to Shipment S0000001", order.JD_JS, shipment.PK);
		}

		public void TestImportOrderDoesntExceedShipmentsOrderLimit()
		{
			const string expectedError = @"Error: The number of Orders on a Shipment is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT123";

			TestParentOrderCollectionLimit(shipment, "SHIPMENT123", Xsd.OrderOrderDetailReferenceNumberType.Shipment, FreightDataRegistry.Instance.OrdersPerShipmentLimit, FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC, expectedError);
		}

		public void TestImportOrderGetsLinkedToJobDeclaration()
		{
			BusinessObject jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			jobDeclaration[JobDeclarationSchema.JE_DeclarationReference.Name] = "B0000001";
			Factory.Save();

			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail.ReferenceNumber.Value = "B0000001";
			xsdOrder.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Declaration;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			AssertEquals("Order was not linked to a Declaration B0000001", order.JD_JE, jobDeclaration.PK);
		}

		public void TestImportOrderDoesntExceedDeclarationsOrderLimit()
		{
			const string expectedError = @"Error: The number of Orders on a Declaration is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "DEC123";
			((BusinessObject)declaration)[JobDeclarationSchema.JE_OH_Importer] = Factory.NewWithValidTestData<OrgHeader>().PK;

			TestParentOrderCollectionLimit((IAttachOrders)declaration, "DEC123", Xsd.OrderOrderDetailReferenceNumberType.Declaration, CustomsDataRegistry.Instance.OrdersPerDeclarationLimit, CustomsDataRegistry.Instance.OrdersPerDeclarationLimitIntroductionTimeUTC, expectedError);
		}

		void TestParentOrderCollectionLimit(IAttachOrders parent, string jobNumber, Xsd.OrderOrderDetailReferenceNumberType refType, IntRegistryItem limitRegistry, DateTimeRegistryItem introductionTimeRegistry, string expectedError)
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			var order2 = Factory.NewWithValidTestData<Order>();
			parent.AttachedOrders.Add(order1);
			parent.AttachedOrders.Add(order2);

			Factory.Save();

			var xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail.ReferenceNumber.Value = jobNumber;
			xsdOrder.OrderDetail.ReferenceNumber.Type = refType;

			using (limitRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (introductionTimeRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var adapter = new TestOrderValueObjectDataAdapter();
				adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);

				AssertCollectionContains(expectedError, Notifications.GetEventsByType(ErrorType.DataErrorPreventSave).Select(x => x.Message));
			}
		}

		public void TestImportOrderWithInvalidReferenceNumber()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderDetail.ReferenceNumber.Value = "blah";
			xsdOrder.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Declaration;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			AssertEquals("Order should not be linked to a Shipment", ZGuid.Empty, order.JD_JS);
			AssertEquals("Order should not be linked to a Declaration", ZGuid.Empty, order.JD_JE);
			ZString message = "Warning: No Declaration exists with Declaration Number: blah";
			Assert("Notification Message: <" + message + "> was not Found", ((ZString)Notifications.AsString).Contains(message));

			xsdOrder.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Shipment;
			adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			message = "Warning: No Shipment exists with Shipment Number: blah";
			Assert("Notification Message: <" + message + "> was not Found", ((ZString)Notifications.AsString).Contains(message));
		}

		public void TestOrderDoesNotGetUpdatedIfItIsAttachedToAShipment()
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderIdentifier.OrderNumber = "ORDERNUMBER";
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			xsdOrder.OrderDetail.ConfirmNumber = "R55";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			Order orderBizObj = Factory.New<Order>();
			orderBizObj.JD_OrderNumber = "ORDERNUMBER";
			orderBizObj.BuyerPK = Context.FindOrganisationPK(xsdOrder.OrderDetail.Buyer, orderBizObj, OrganisationTypes.Consignee);
			CommonShipment shipment = CommonShipment.New(Factory);
			orderBizObj.JD_JS = shipment.PK;
			Factory.Save();

			xsdOrder.OrderDetail.ReferenceNumber.Value = shipment.JS_UniqueConsignRef;
			xsdOrder.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Shipment;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			AssertEquals("HasChanges", false, orderBizObj.HasChanges);
			AssertEquals("BookingConfRef", "", orderBizObj.JD_BookingConfRef);
			AssertEquals("Notification:", true, Notifications.AsString.IndexOf("Order ORDERNUMBER is linked to a shipment and cannot be updated.") > -1);

			orderBizObj.JD_JS = ZGuid.Empty;
			Factory.Save();

			Notifications.Clear();
			adapter.ImportFromValueObject(orderBizObj, xsdOrder, Context);
			AssertEquals("HasChanges", true, orderBizObj.HasChanges);
			AssertEquals("BookingConfRef", "R55", orderBizObj.JD_BookingConfRef);
			AssertEquals("Notification:", true, Notifications.AsString.IndexOf("Order ORDERNUMBER updated") > -1);
		}

		public void TestImportDoesNotUpdateAttachedOrders()
		{
			OrgHeader buyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			OrgHeader supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";

			Order orderBizObj = Factory.New<Order>();
			orderBizObj.JD_OrderNumber = "ORDERNUMBER";
			orderBizObj.JD_JS = shipment.PK;
			orderBizObj.BuyerPK = buyer.PK;
			orderBizObj.SupplierPK = supplier.PK;
			Factory.Save();

			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderIdentifier.OrderNumber = "ORDERNUMBER";
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.EDICode = buyer.OH_Code;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Order loadedOrder = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);

			AssertEquals("Order should not have been updated", false, loadedOrder.HasChanges);
			Assert("Notification should have warnings:", Notifications.HasWarnings);
			AssertEquals("Warning should show:", true, Notifications.AsString.IndexOf("Order ORDERNUMBER is linked to a shipment and cannot be updated.") > -1);
		}

		public void TestExportOrderNumberAndSplit()
		{
			Order orderBizObj = Factory.New<Order>();
			orderBizObj.JD_OrderNumber = "O11";
			orderBizObj.JD_OrderNumberSplit = 1;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("Order Number", "O11", xsdOrder.OrderIdentifier.OrderNumber);
			AssertEquals("Order Number Split", (byte)1, xsdOrder.OrderIdentifier.OrderNumberSplit);
		}

		public void TestExportOrderDetails()
		{
			var orderBizObj = Factory.New<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			orderBizObj.BuyerPK = buyer.PK;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Supplier";
			orderBizObj.SupplierPK = supplier.PK;
			orderBizObj.JD_BookingConfRef = "R55";
			orderBizObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBizObj.JD_OrderGoodsDescription = "Goods Description";
			orderBizObj.JD_EstimatedExchangeRate = 1.23M;
			orderBizObj.JD_IncoTerm = "I66";
			orderBizObj.JD_InvoiceNumber = "I77";
			orderBizObj.JD_OrderDate = new DateTime(2005, 03, 23);
			orderBizObj.JD_OrderStatus = "CAN";
			orderBizObj.OrderLines.AddNew();
			orderBizObj.OrderLines[0].JO_LinePrice = 1235000M;
			orderBizObj.JD_RX_NKOrderCurrency = "AUD";
			orderBizObj.JD_RN_NKCountryOfSupply = "US";

			var adapter = new TestOrderValueObjectDataAdapter();
			var xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("Buyer Name", "Buyer", xsdOrder.OrderDetail.Buyer.OrganisationDetails.Name);
			AssertEquals("Supplier Name", "Supplier", xsdOrder.OrderDetail.Supplier.OrganisationDetails.Name);
			AssertEquals("Confirm Number", "R55", xsdOrder.OrderDetail.ConfirmNumber);
			AssertEquals("Container Mode Specified", true, xsdOrder.OrderDetail.ContainerModeSpecified);
			AssertEquals("Container Mode", Xsd.OrderContainerMode.FCL, xsdOrder.OrderDetail.ContainerMode);
			AssertEquals("Description", "Goods Description", xsdOrder.OrderDetail.Description);
			AssertEquals("Exchange Rate Specified", true, xsdOrder.OrderDetail.ExchangeRateSpecified);
			AssertEquals("Exchange Rate", 1.23M, xsdOrder.OrderDetail.ExchangeRate);
			AssertEquals("Exch Rate Basis Specified", true, xsdOrder.OrderDetail.ExchRateBasisSpecified);
			AssertEquals("Exch Rate Basis", Xsd.OrderOrderDetailExchRateBasis.F, xsdOrder.OrderDetail.ExchRateBasis);
			AssertEquals("Inco term", "I66", xsdOrder.OrderDetail.Incoterm);
			AssertEquals("Invoice Number", "I77", xsdOrder.OrderDetail.InvoiceNumber);
			AssertEquals("Order DateTime Specified", true, xsdOrder.OrderDetail.OrderDateTime.IsValid);
			AssertEquals("Order DateTime", new DateTime(2005, 03, 23), xsdOrder.OrderDetail.OrderDateTime);
			AssertEquals("Order Status Specified", true, xsdOrder.OrderDetail.OrderStatusSpecified);
			AssertEquals("Order Status", Core.Constants.OrderStatus.Cancelled, xsdOrder.OrderDetail.OrderStatus);
			AssertEquals("Order Total Value", 1235000M, xsdOrder.OrderDetail.OrderTotal.Value);
			AssertEquals("Order TotalCurrencyCode", "AUD", xsdOrder.OrderDetail.OrderTotal.CurrencyCode);
			AssertEquals("Transport Mode Specified", true, xsdOrder.OrderDetail.TransportModeSpecified);
			AssertEquals("Transport Mode", Xsd.OrderTransportMode.SEA, xsdOrder.OrderDetail.TransportMode);
			AssertEquals("Country of Origin Specified", true, xsdOrder.OrderDetail.CountryOfOriginSpecified);
			AssertEquals("Country of Origin", "US", xsdOrder.OrderDetail.CountryOfOrigin);
		}

		public void TestExportCustomOrderDetails()
		{
			Order orderBizObj = Factory.New<Order>();
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			orderBizObj.BuyerPK = buyer.PK;

			OrgCustomLabels customDecimal1Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal2Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal3Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal4Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal5Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag1Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag2Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag3Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag4Label = buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag5Label = buyer.CustomLabels.AddNew();

			customDecimal1Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal1;
			customDecimal2Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal2;
			customDecimal3Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal3;
			customDecimal4Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal4;
			customDecimal5Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal5;
			customFlag1Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag1;
			customFlag2Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag2;
			customFlag3Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag3;
			customFlag4Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag4;
			customFlag5Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag5;

			customDecimal1Label.OT_Caption = "Caption";
			customDecimal2Label.OT_Caption = "Caption";
			customDecimal3Label.OT_Caption = "Caption";
			customDecimal4Label.OT_Caption = "Caption";
			customDecimal5Label.OT_Caption = "Caption";
			customFlag1Label.OT_Caption = "Caption";
			customFlag2Label.OT_Caption = "Caption";
			customFlag3Label.OT_Caption = "Caption";
			customFlag4Label.OT_Caption = "Caption";
			customFlag5Label.OT_Caption = "Caption";

			orderBizObj.JD_CustomAttrib1 = "aa";
			orderBizObj.JD_CustomAttrib2 = "bb";
			orderBizObj.JD_CustomAttrib3 = "cc";
			orderBizObj.JD_CustomAttrib4 = "dd";
			orderBizObj.JD_CustomAttrib5 = "ee";

			orderBizObj.JD_FirstBuyerContact = "contact1";
			orderBizObj.JD_SecondBuyerContact = "contact2";

			ZDateTime now = ZDateTime.Now;
			orderBizObj.JD_CustomDate1 = now;
			orderBizObj.JD_CustomDate2 = now.AddDays(1);
			orderBizObj.JD_CustomDecimal1 = 1.1m;
			orderBizObj.JD_CustomDecimal2 = 2.2m;
			orderBizObj.JD_CustomDecimal3 = 3.3m;
			orderBizObj.JD_CustomDecimal4 = 4.4m;
			orderBizObj.JD_CustomDecimal5 = 5.5m;
			orderBizObj.JD_CustomFlag1 = true;
			orderBizObj.JD_CustomFlag2 = false;
			orderBizObj.JD_CustomFlag3 = true;
			orderBizObj.JD_CustomFlag4 = false;
			orderBizObj.JD_CustomFlag5 = true;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("CustomAttrib1", "aa", xsdOrder.OrderDetail.Custom.Text1);
			AssertEquals("CustomAttrib2", "bb", xsdOrder.OrderDetail.Custom.Text2);
			AssertEquals("CustomAttrib3", "cc", xsdOrder.OrderDetail.Custom.Text3);
			AssertEquals("CustomAttrib4", "dd", xsdOrder.OrderDetail.Custom.Text4);
			AssertEquals("CustomAttrib5", "ee", xsdOrder.OrderDetail.Custom.Text5);
			AssertEquals("contact1", "contact1", xsdOrder.OrderDetail.Custom.Contact1);
			AssertEquals("contact2", "contact2", xsdOrder.OrderDetail.Custom.Contact2);
			AssertEquals("CustomDate1", now, xsdOrder.OrderDetail.Custom.Date1);
			AssertEquals("CustomDate2", now.AddDays(1), xsdOrder.OrderDetail.Custom.Date2);
			AssertEquals("CustomDecimal1", 1.1m, xsdOrder.OrderDetail.Custom.Decimal1);
			AssertEquals("CustomDecimal2", 2.2m, xsdOrder.OrderDetail.Custom.Decimal2);
			AssertEquals("CustomDecimal3", 3.3m, xsdOrder.OrderDetail.Custom.Decimal3);
			AssertEquals("CustomDecimal4", 4.4m, xsdOrder.OrderDetail.Custom.Decimal4);
			AssertEquals("CustomDecimal5", 5.5m, xsdOrder.OrderDetail.Custom.Decimal5);
			AssertEquals("CustomFlag1", true, xsdOrder.OrderDetail.Custom.Flag1);
			AssertEquals("CustomFlag2", false, xsdOrder.OrderDetail.Custom.Flag2);
			AssertEquals("CustomFlag3", true, xsdOrder.OrderDetail.Custom.Flag3);
			AssertEquals("CustomFlag4", false, xsdOrder.OrderDetail.Custom.Flag4);
			AssertEquals("CustomFlag5", true, xsdOrder.OrderDetail.Custom.Flag5);
		}

		public void TestExportOrderShipmentPlanning()
		{
			var orderBizObj = Factory.New<Order>();
			orderBizObj.JD_Waybill = "W22";
			orderBizObj.JD_RL_NKGoodsAvailableAt = "AUSYD";
			orderBizObj.JD_RL_NKGoodsDeliveredTo = "AUMEL";
			orderBizObj.JD_Packs = 11;
			orderBizObj.JD_ActualWeight = 22M;
			orderBizObj.JD_ActualVolume = 33M;
			orderBizObj.JD_RV_NKDepartureVessel = "Vessel";
			orderBizObj.JD_DepartureVoyage = "V33";

			var goodAvailableAtAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodAvailableAtAddress.OA_Code = "AvailAt";
			orderBizObj.GoodsAvailableAtAddress.E2_OA_Address = goodAvailableAtAddress.PK;

			var goodDeliveredToAddress = Factory.NewWithValidTestData<OrgAddress>();
			goodDeliveredToAddress.OA_Code = "DeliverTo";
			orderBizObj.GoodsDeliveredToAddress.E2_OA_Address = goodDeliveredToAddress.PK;

			var adapter = new TestOrderValueObjectDataAdapter();
			var xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			Xsd.OrderOrderDetailShipmentPlanning xsdShipmentPlanning = xsdOrder.OrderDetail.ShipmentPlanning;
			AssertEquals("House Bill", "W22", xsdShipmentPlanning.HouseBill);
			AssertEquals("Goods Origin", "AUSYD", xsdShipmentPlanning.GoodsOrigin.Value);
			AssertEquals("Goods Destination", "AUMEL", xsdShipmentPlanning.GoodsDestination.Value);
			AssertEquals("Packs", 11M, xsdShipmentPlanning.Packs.Value);
			AssertEquals("Weight", 22M, xsdShipmentPlanning.Weight.Value);
			AssertEquals("Volume", 33M, xsdShipmentPlanning.Volume.Value);
			AssertEquals("Departure Vessel", "Vessel", xsdShipmentPlanning.DepartureVessel);
			AssertEquals("Avail At", "AvailAt", xsdShipmentPlanning.GoodsAvailAt);
			AssertEquals("Deliver To", "DeliverTo", xsdShipmentPlanning.GoodsDelivTo);
			AssertEquals("Departure Voyage Flight", "V33", xsdShipmentPlanning.DepartureVoyageFlight);
		}

		public void TestExportOrderPlannedContainers()
		{
			Order orderBizObj = Factory.New<Order>();
			OrderContainer container = orderBizObj.PlannedContainers.AddNew();
			container.J1_ContainerNumber = "C44";
			container.J1_ContainerCount = 55;
			container.J1_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("PlannedContainers.Count", 1, xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers.Count);
			Xsd.PlannedContainer xsdPlannedContainer = xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers[0];
			AssertEquals("Container Number", "C44", xsdPlannedContainer.Number);
			AssertEquals("Container Quantity", (ZShort)55, xsdPlannedContainer.Quantity);
			AssertEquals("Container Type", "22P1", xsdPlannedContainer.Type.ISOCode);
		}

		public void TestExportOrderMilestones()
		{
			Order orderBizObj = Factory.New<Order>();
			orderBizObj.UpdateEventEstimate(Events.Arrival, new DateTime(2005, 03, 01));
			orderBizObj.UpdateEvent(Events.Arrival, new DateTime(2005, 03, 02));
			orderBizObj.UpdateEventEstimate(Events.DeliveryCartageAdvised, new DateTime(2005, 03, 03));
			orderBizObj.UpdateEvent(Events.DeliveryCartageAdvised, new DateTime(2005, 03, 04));
			orderBizObj.UpdateEventEstimate(Events.CustomsCommenced, new DateTime(2005, 03, 05));
			orderBizObj.UpdateEvent(Events.CustomsCommenced, new DateTime(2005, 03, 06));
			orderBizObj.UpdateEventEstimate(Events.CustomsCleared, new DateTime(2005, 03, 07));
			orderBizObj.UpdateEvent(Events.CustomsCleared, new DateTime(2005, 03, 08));
			orderBizObj.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, new DateTime(2005, 03, 09));
			orderBizObj.UpdateEvent(Events.DeliveryCartageCompleteFinalised, new DateTime(2005, 03, 10));
			orderBizObj.UpdateEventEstimate(Events.Departure, new DateTime(2005, 03, 11));
			orderBizObj.UpdateEvent(Events.Departure, new DateTime(2005, 03, 12));
			orderBizObj.UpdateEventEstimate(Events.ExWorks, new DateTime(2005, 03, 13));
			orderBizObj.UpdateEvent(Events.ExWorks, new DateTime(2005, 03, 14));
			orderBizObj.UpdateEventEstimate(Events.GateIn, new DateTime(2005, 03, 15));
			orderBizObj.UpdateEvent(Events.GateIn, new DateTime(2005, 03, 16));
			orderBizObj.UpdateEventEstimate(Events.CargoAvailable, new DateTime(2005, 03, 17));
			orderBizObj.UpdateEvent(Events.CargoAvailable, new DateTime(2005, 03, 18));
			orderBizObj.JD_EstimateUserDate1 = new DateTime(2005, 03, 19);
			orderBizObj.JD_EstimateUserDate2 = new DateTime(2005, 03, 21);
			orderBizObj.JD_ActualUserDate1 = new DateTime(2005, 03, 20);
			orderBizObj.JD_ActualUserDate2 = new DateTime(2005, 03, 22);

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			Xsd.OrderOrderDetailMilestones milestones = xsdOrder.OrderDetail.Milestones;
			AssertEquals("Estimated Arrival", new DateTime(2005, 03, 01), milestones.Arrival.Estimated);
			AssertEquals("Actual Arrival Specified", true, milestones.Arrival.Actual.IsValid);
			AssertEquals("Actual Arrival", new DateTime(2005, 03, 02), milestones.Arrival.Actual);
			AssertEquals("Estimated	Cartage	Advised", new DateTime(2005, 03, 03), milestones.CartageAdvised.Estimated);
			AssertEquals("Actual Cartage Advised Specified", true, milestones.CartageAdvised.Actual.IsValid);
			AssertEquals("Actual Cartage Advised", new DateTime(2005, 03, 04), milestones.CartageAdvised.Actual);
			AssertEquals("Estimated	Customs	Commenced", new DateTime(2005, 03, 05), milestones.CustomsCommenced.Estimated);
			AssertEquals("Actual Customs Commenced Specified", true, milestones.CustomsCommenced.Actual.IsValid);
			AssertEquals("Actual Customs Commenced", new DateTime(2005, 03, 06), milestones.CustomsCommenced.Actual);
			AssertEquals("Estimated	Customs	Finalised", new DateTime(2005, 03, 07), milestones.CustomsFinalised.Estimated);
			AssertEquals("Actual Customs Finalised Specified", true, milestones.CustomsFinalised.Actual.IsValid);
			AssertEquals("Actual Customs Finalised", new DateTime(2005, 03, 08), milestones.CustomsFinalised.Actual);
			AssertEquals("Estimated	Delivery", new DateTime(2005, 03, 09), milestones.Delivery.Estimated);
			AssertEquals("Actual Delivery	Specified", true, milestones.Delivery.Actual.IsValid);
			AssertEquals("Actual Delivery", new DateTime(2005, 03, 10), milestones.Delivery.Actual);
			AssertEquals("Estimated	Departure", new DateTime(2005, 03, 11), milestones.Departure.Estimated);
			AssertEquals("Actual Departure Specified", true, milestones.Departure.Actual.IsValid);
			AssertEquals("Actual Departure", new DateTime(2005, 03, 12), milestones.Departure.Actual);
			AssertEquals("Estimated	ExFactory", new DateTime(2005, 03, 13), milestones.ExFactory.Estimated);
			AssertEquals("Actual ExFactory Specified", true, milestones.ExFactory.Actual.IsValid);
			AssertEquals("Actual ExFactory", new DateTime(2005, 03, 14), milestones.ExFactory.Actual);
			AssertEquals("Estimated Origin Receival", new DateTime(2005, 03, 15), milestones.OriginReceival.Estimated);
			AssertEquals("Actual Origin Receival Specified", true, milestones.OriginReceival.Actual.IsValid);
			AssertEquals("Actual Origin Receival", new DateTime(2005, 03, 16), milestones.OriginReceival.Actual);
			AssertEquals("Estimated Unpacked", new DateTime(2005, 03, 17), milestones.Unpacked.Estimated);
			AssertEquals("Actual Unpacked Specified", true, milestones.Unpacked.Actual.IsValid);
			AssertEquals("Actual Unpacked", new DateTime(2005, 03, 18), milestones.Unpacked.Actual);

			AssertEquals("UserDate Count", 2, milestones.UserDate.Count);

			AssertEquals("Estimated UserDate1", new DateTime(2005, 03, 19), milestones.UserDate[0].Estimated);
			AssertEquals("Actual UserDate1 Specified", new DateTime(2005, 03, 20), milestones.UserDate[0].Actual);

			AssertEquals("Estimated UserDate2", new DateTime(2005, 03, 21), milestones.UserDate[1].Estimated);
			AssertEquals("Actual UserDate2 Specified", new DateTime(2005, 03, 22), milestones.UserDate[1].Actual);
		}

		public void TestExportOrderLines()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "P99";
			part.OP_Desc = "Part 99 Description";
			OrgPartRelation partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_OH = Buyer.PK;
			partRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			Order orderBizObj = Factory.New<Order>();
			orderBizObj.JD_RN_NKCountryOfSupply = "NZ";
			orderBizObj.BuyerPK = Buyer.PK;

			OrderLine orderBizObjLine = orderBizObj.OrderLines.AddNew();
			orderBizObjLine.JO_LineNo = 200;
			orderBizObjLine.JO_Partno = "P99";
			orderBizObjLine.JO_F3_NKPackType = "UNT";
			orderBizObjLine.JO_LineDropDate = new DateTime(2005, 03, 24);
			orderBizObjLine.JO_InnerPacks = 45M;
			orderBizObjLine.JO_InnerPacksUQ = "BAG";
			orderBizObjLine.JO_ItemPrice = 1235M;
			orderBizObjLine.JO_LinePrice = 9500M;
			orderBizObjLine.JO_OuterPacks = 50M;
			orderBizObjLine.JO_OuterPacksUQ = "CTN";
			orderBizObjLine.JO_ActualWeight = 75M;
			orderBizObjLine.JO_ActualVolume = 100M;
			orderBizObjLine.JO_UnitOfWeight = "KG";
			orderBizObjLine.JO_UnitOfVolume = "M3";
			orderBizObjLine.JO_Quantity = 1000M;
			orderBizObjLine.JO_QtyInvoiced = 666M;
			orderBizObjLine.JO_QtyReceived = 600M;
			orderBizObjLine.JO_LineStatus = Core.Constants.OrderStatus.Delivered;
			orderBizObjLine.JO_PartAttrib1 = "PartAttrib1";
			orderBizObjLine.JO_PartAttrib2 = "PartAttrib2";
			orderBizObjLine.JO_PartAttrib3 = "PartAttrib3";
			orderBizObjLine.JO_SerialNumber = "SerialNumber";
			orderBizObjLine.JO_LineSplitNumber = 4;
			orderBizObjLine.JO_INCO = "FOB";
			orderBizObjLine.JO_AdditionalTerms = "AdditionalTerms";
			orderBizObjLine.JO_ConfirmationDate = new ZDateTime(2008, 3, 10);
			orderBizObjLine.JO_ConfirmationNum = "ABC1";
			orderBizObjLine.JO_ExWorksDate = new ZDateTime(2011, 1, 5);

			UNDGDataItem dgItem = orderBizObjLine.UNDGs.AddNew();
			dgItem.DI_DG = Substance.PK;
			dgItem.DI_DGFlashPoint = 0.1m;
			orderBizObjLine.JO_ContainerNumber = "ContainerNo";
			orderBizObjLine.JO_CommercialInvoiceNo = "1223";
			orderBizObjLine.JO_ContainerPackingOrder = 2;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 1, xsdOrder.OrderLines.Count);
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines[0];
			AssertEquals("Order Line No", 200, xsdOrderLine.OrderLineNo);
			AssertEquals("Order Sub Line No is specified", true, xsdOrderLine.OrderSubLineNoSpecified);
			AssertEquals("Order Sub Line No", 1, xsdOrderLine.OrderSubLineNo);
			AssertEquals("Description from Product if line description blank", "Part 99 Description", xsdOrderLine.OrderLineDetail.Description);
			AssertEquals("Drop Date Specified", true, xsdOrderLine.OrderLineDetail.DropDate.IsValid);
			AssertEquals("Drop Date", new DateTime(2005, 03, 24), xsdOrderLine.OrderLineDetail.DropDate);
			AssertEquals("Inner Packs Specified", true, xsdOrderLine.OrderLineDetail.InnerPacksSpecified);
			AssertEquals("Inner Packs", 45M, xsdOrderLine.OrderLineDetail.InnerPacks.Value);
			AssertEquals("Inner Packs type", "BAG", xsdOrderLine.OrderLineDetail.InnerPacks.DimensionType);
			AssertEquals("Item Price Specified", true, xsdOrderLine.OrderLineDetail.ItemPrice.IsSpecified);
			AssertEquals("Item Price", 1235M, xsdOrderLine.OrderLineDetail.ItemPrice.Value);
			AssertEquals("Line Price Specified", true, xsdOrderLine.OrderLineDetail.LinePrice.IsSpecified);
			AssertEquals("Line Price", 1235000M, xsdOrderLine.OrderLineDetail.LinePrice.Value);
			AssertEquals("Outer Packs Specified", true, xsdOrderLine.OrderLineDetail.OuterPacksSpecified);
			AssertEquals("Outer Packs", 50M, xsdOrderLine.OrderLineDetail.OuterPacks.Value);
			AssertEquals("Outer Packs type", "CTN", xsdOrderLine.OrderLineDetail.OuterPacks.DimensionType);
			AssertEquals("Weight Specified", true, xsdOrderLine.OrderLineDetail.WeightSpecified);
			AssertEquals("Weight", 75M, xsdOrderLine.OrderLineDetail.Weight);
			AssertEquals("Weight Type Specified", true, xsdOrderLine.OrderLineDetail.WeightTypeSpecified);
			AssertEquals("Unit of Weight", "KG", xsdOrderLine.OrderLineDetail.WeightType);
			AssertEquals("Volume Specified", true, xsdOrderLine.OrderLineDetail.VolumeSpecified);
			AssertEquals("Volume", 100M, xsdOrderLine.OrderLineDetail.Volume);
			AssertEquals("Volume Type Specified", true, xsdOrderLine.OrderLineDetail.VolumeTypeSpecified);
			AssertEquals("Unit of Volume", "M3", xsdOrderLine.OrderLineDetail.VolumeType);
			AssertEquals("Product", "P99", xsdOrderLine.OrderLineDetail.Product);
			AssertEquals("Qty Ordered Value", 1000M, xsdOrderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals("Qty Ordered Type", "UNT", xsdOrderLine.OrderLineDetail.QtyOrdered.DimensionType);
			AssertEquals("Qty Invoiced Value", 666M, xsdOrderLine.OrderLineDetail.QtyInvoiced.Value);
			AssertEquals("Qty Received Value", 600M, xsdOrderLine.OrderLineDetail.QtyReceived.Value);
			AssertEquals("Qty Received Type", "UNT", xsdOrderLine.OrderLineDetail.QtyReceived.DimensionType);
			AssertEquals("Line Status Specified", true, xsdOrderLine.OrderLineDetail.LineStatusSpecified);
			AssertEquals("Line Status", Core.Constants.OrderStatus.Delivered, xsdOrderLine.OrderLineDetail.LineStatus);
			AssertEquals("Part Attrib 1", "PartAttrib1", xsdOrderLine.OrderLineDetail.PartAttrib1);
			AssertEquals("Part Attrib 2", "PartAttrib2", xsdOrderLine.OrderLineDetail.PartAttrib2);
			AssertEquals("Part Attrib 3", "PartAttrib3", xsdOrderLine.OrderLineDetail.PartAttrib3);
			AssertEquals("Split Line Num", (ZShort)4, xsdOrderLine.OrderLineSplitNo);
			AssertEquals("INCO Term", "FOB", xsdOrderLine.OrderLineDetail.Incoterm);
			AssertEquals("Additional Terms", "AdditionalTerms", xsdOrderLine.OrderLineDetail.AdditionalTerms);
			AssertEquals("Confirmation Date", new ZDateTime(2008, 3, 10), xsdOrderLine.OrderLineDetail.ConfirmDate);
			AssertEquals("Confirmation Number", "ABC1", xsdOrderLine.OrderLineDetail.ConfirmNumber);
			AssertEquals("Ex Works Date", new ZDateTime(2011, 1, 5), xsdOrderLine.OrderLineDetail.ExWorksRequiredBy);

			AssertEquals("Xsd ContainerNumber", "ContainerNo", xsdOrderLine.OrderLineDetail.ContainerNumber);
			AssertEquals("Xsd CommercialInvoiceNo", "1223", xsdOrderLine.OrderLineDetail.CommercialInvoiceNo);
			AssertEquals("Xsd ContainerPackingOrder", 2, xsdOrderLine.OrderLineDetail.ContainerPackingOrder);
			AssertEquals("Xsd CountryOfOrigin", "NZ", xsdOrderLine.OrderLineDetail.CountryOfOrigin);
			AssertEquals("Xsd DGFlashPoint", "0.1", xsdOrderLine.OrderLineDetail.DangerousGoods[0].FlashPoint);
			AssertEquals("Xsd DGSubstanceCode", Substance.DG_Code, xsdOrderLine.OrderLineDetail.DangerousGoods[0].UNDGCode);

			orderBizObjLine.JO_RN_NKCountryOfOrigin = "AU";
			orderBizObjLine.JO_Description = "Line Description";
			adapter = new TestOrderValueObjectDataAdapter();
			xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			AssertEquals("Xsd CountryOfOrigin", "AU", xsdOrder.OrderLines[0].OrderLineDetail.CountryOfOrigin);
			AssertEquals("Description comes from Line itself", "Line Description", xsdOrder.OrderLines[0].OrderLineDetail.Description);
		}

		public void TestExportOrderLines_QtyReceivedToDate()
		{
			Order orderBizObj = Factory.New<Order>();

			OrderLine orderBizObjLine = orderBizObj.OrderLines.AddNew();
			orderBizObjLine.JO_LineNo = 200;
			orderBizObjLine.JO_QtyReceived = 600M;

			OrderLine orderBizObjLine2 = orderBizObj.OrderLines.AddNew();
			orderBizObjLine2.JO_LineNo = 200;
			orderBizObjLine2.JO_QtyReceived = 100M;

			OrderLine orderBizObjLine3 = orderBizObj.OrderLines.AddNew();
			orderBizObjLine3.JO_LineNo = 300;
			orderBizObjLine3.JO_QtyReceived = 300M;

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 3, xsdOrder.OrderLines.Count);
			AssertEquals(600M, xsdOrder.OrderLines[0].OrderLineDetail.QtyReceived.Value);
			AssertEquals(100M, xsdOrder.OrderLines[1].OrderLineDetail.QtyReceived.Value);
			AssertEquals(300M, xsdOrder.OrderLines[2].OrderLineDetail.QtyReceived.Value);

			AssertEquals(700M, xsdOrder.OrderLines[0].OrderLineDetail.QtyReceivedToDate.Value);
			AssertEquals(700M, xsdOrder.OrderLines[1].OrderLineDetail.QtyReceivedToDate.Value);
			AssertEquals(300M, xsdOrder.OrderLines[2].OrderLineDetail.QtyReceivedToDate.Value);

			OrderLine orderBizObjLine4 = orderBizObj.OrderLines.AddNew();
			orderBizObjLine4.JO_LineNo = 200;
			orderBizObjLine4.JO_QtyReceived = 150M;
			orderBizObjLine4.JO_F3_NKPackType = "XYZ";

			xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 4, xsdOrder.OrderLines.Count);
			AssertEquals(600M, xsdOrder.OrderLines[0].OrderLineDetail.QtyReceived.Value);
			AssertEquals(100M, xsdOrder.OrderLines[1].OrderLineDetail.QtyReceived.Value);
			AssertEquals(300M, xsdOrder.OrderLines[2].OrderLineDetail.QtyReceived.Value);
			AssertEquals(150M, xsdOrder.OrderLines[3].OrderLineDetail.QtyReceived.Value);

			AssertEquals(0M, xsdOrder.OrderLines[0].OrderLineDetail.QtyReceivedToDate.Value);
			AssertEquals(0M, xsdOrder.OrderLines[1].OrderLineDetail.QtyReceivedToDate.Value);
			AssertEquals(300M, xsdOrder.OrderLines[2].OrderLineDetail.QtyReceivedToDate.Value);
			AssertEquals(0M, xsdOrder.OrderLines[3].OrderLineDetail.QtyReceivedToDate.Value);
		}

		public void TestExportOrderLineDeliveryCustomDetails()
		{
			var orderBizObj = Factory.New<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			orderBizObj.BuyerPK = buyer.PK;

			var orderBizObjLine = orderBizObj.OrderLines.AddNew();
			orderBizObjLine.JO_LineNo = 200;

			var delivery = orderBizObjLine.Deliveries.AddNew();

			var customDecimal1Label = buyer.CustomLabels.AddNew();
			var customDecimal2Label = buyer.CustomLabels.AddNew();
			var customDecimal3Label = buyer.CustomLabels.AddNew();
			var customDecimal4Label = buyer.CustomLabels.AddNew();
			var customDecimal5Label = buyer.CustomLabels.AddNew();
			var customFlag1Label = buyer.CustomLabels.AddNew();
			var customFlag2Label = buyer.CustomLabels.AddNew();
			var customFlag3Label = buyer.CustomLabels.AddNew();
			var customFlag4Label = buyer.CustomLabels.AddNew();
			var customFlag5Label = buyer.CustomLabels.AddNew();
			var customText1Label = buyer.CustomLabels.AddNew();
			var customText2Label = buyer.CustomLabels.AddNew();
			var customText3Label = buyer.CustomLabels.AddNew();
			var customText4Label = buyer.CustomLabels.AddNew();
			var customText5Label = buyer.CustomLabels.AddNew();
			var customDate1Label = buyer.CustomLabels.AddNew();
			var customDate2Label = buyer.CustomLabels.AddNew();
			var customDate3Label = buyer.CustomLabels.AddNew();
			var customDate4Label = buyer.CustomLabels.AddNew();
			var customDate5Label = buyer.CustomLabels.AddNew();

			customDecimal1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDecimal1;
			customDecimal2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDecimal2;
			customDecimal3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDecimal3;
			customDecimal4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDecimal4;
			customDecimal5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDecimal5;

			customFlag1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomFlag1;
			customFlag2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomFlag2;
			customFlag3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomFlag3;
			customFlag4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomFlag4;
			customFlag5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomFlag5;

			customText1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute1;
			customText2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute2;
			customText3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute3;
			customText4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute4;
			customText5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomAttribute5;

			customDate1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDate1;
			customDate2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDate2;
			customDate3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDate3;
			customDate4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDate4;
			customDate5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLineDelivery.CustomDate5;

			customDecimal1Label.OT_Caption = "Caption";
			customDecimal2Label.OT_Caption = "Caption";
			customDecimal3Label.OT_Caption = "Caption";
			customDecimal4Label.OT_Caption = "Caption";
			customDecimal5Label.OT_Caption = "Caption";
			customFlag1Label.OT_Caption = "Caption";
			customFlag2Label.OT_Caption = "Caption";
			customFlag3Label.OT_Caption = "Caption";
			customFlag4Label.OT_Caption = "Caption";
			customFlag5Label.OT_Caption = "Caption";
			customText1Label.OT_Caption = "Caption";
			customText2Label.OT_Caption = "Caption";
			customText3Label.OT_Caption = "Caption";
			customText4Label.OT_Caption = "Caption";
			customText5Label.OT_Caption = "Caption";
			customDate1Label.OT_Caption = "Caption";
			customDate2Label.OT_Caption = "Caption";
			customDate3Label.OT_Caption = "Caption";
			customDate4Label.OT_Caption = "Caption";
			customDate5Label.OT_Caption = "Caption";

			delivery.J4_CustomAttribute1 = "aa";
			delivery.J4_CustomAttribute2 = "bb";
			delivery.J4_CustomAttribute3 = "cc";
			delivery.J4_CustomAttribute4 = "dd";
			delivery.J4_CustomAttribute5 = "ee";

			var now = ZDateTime.Now;
			delivery.J4_CustomDate1 = now;
			delivery.J4_CustomDate2 = now.AddDays(1);
			delivery.J4_CustomDate3 = now.AddDays(2);
			delivery.J4_CustomDate4 = now.AddDays(3);
			delivery.J4_CustomDate5 = now.AddDays(4);

			delivery.J4_CustomDecimal1 = 1.1m;
			delivery.J4_CustomDecimal2 = 2.2m;
			delivery.J4_CustomDecimal3 = 3.3m;
			delivery.J4_CustomDecimal4 = 4.4m;
			delivery.J4_CustomDecimal5 = 5.5m;

			delivery.J4_CustomFlag1 = true;
			delivery.J4_CustomFlag2 = false;
			delivery.J4_CustomFlag3 = true;
			delivery.J4_CustomFlag4 = false;
			delivery.J4_CustomFlag5 = true;

			var adapter = new TestOrderValueObjectDataAdapter();
			var customXSD = adapter.ExportOrderLineDeliveryCustomDetails(delivery);

			AssertEquals("CustomAttrib1", "aa", customXSD.Text1);
			AssertEquals("CustomAttrib2", "bb", customXSD.Text2);
			AssertEquals("CustomAttrib3", "cc", customXSD.Text3);
			AssertEquals("CustomAttrib4", "dd", customXSD.Text4);
			AssertEquals("CustomAttrib5", "ee", customXSD.Text5);

			AssertEquals("CustomDate1", now, customXSD.Date1);
			AssertEquals("CustomDate2", now.AddDays(1), customXSD.Date2);
			AssertEquals("CustomDate3", now.AddDays(2), customXSD.Date3);
			AssertEquals("CustomDate4", now.AddDays(3), customXSD.Date4);
			AssertEquals("CustomDate5", now.AddDays(4), customXSD.Date5);

			AssertEquals("CustomDecimal1", 1.1m, customXSD.Decimal1);
			AssertEquals("CustomDecimal2", 2.2m, customXSD.Decimal2);
			AssertEquals("CustomDecimal3", 3.3m, customXSD.Decimal3);
			AssertEquals("CustomDecimal4", 4.4m, customXSD.Decimal4);
			AssertEquals("CustomDecimal5", 5.5m, customXSD.Decimal5);

			AssertEquals("CustomFlag1", true, customXSD.Flag1);
			AssertEquals("CustomFlag2", false, customXSD.Flag2);
			AssertEquals("CustomFlag3", true, customXSD.Flag3);
			AssertEquals("CustomFlag4", false, customXSD.Flag4);
			AssertEquals("CustomFlag5", true, customXSD.Flag5);
		}

		public void TestExportCustomOrderLineDetails()
		{
			var orderBizObj = Factory.New<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			orderBizObj.BuyerPK = buyer.PK;
			var orderBizObjLine = orderBizObj.OrderLines.AddNew();

			var customOLDecimal1Label = buyer.CustomLabels.AddNew();
			var customOLDecimal2Label = buyer.CustomLabels.AddNew();
			var customOLDecimal3Label = buyer.CustomLabels.AddNew();
			var customOLDecimal4Label = buyer.CustomLabels.AddNew();
			var customOLDecimal5Label = buyer.CustomLabels.AddNew();
			var customOLFlag1Label = buyer.CustomLabels.AddNew();
			var customOLFlag2Label = buyer.CustomLabels.AddNew();
			var customOLFlag3Label = buyer.CustomLabels.AddNew();
			var customOLFlag4Label = buyer.CustomLabels.AddNew();
			var customOLFlag5Label = buyer.CustomLabels.AddNew();

			customOLDecimal1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal1;
			customOLDecimal2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal2;
			customOLDecimal3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal3;
			customOLDecimal4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal4;
			customOLDecimal5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal5;
			customOLFlag1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag1;
			customOLFlag2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag2;
			customOLFlag3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag3;
			customOLFlag4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag4;
			customOLFlag5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag5;

			customOLDecimal1Label.OT_Caption = "Caption";
			customOLDecimal2Label.OT_Caption = "Caption";
			customOLDecimal3Label.OT_Caption = "Caption";
			customOLDecimal4Label.OT_Caption = "Caption";
			customOLDecimal5Label.OT_Caption = "Caption";
			customOLFlag1Label.OT_Caption = "Caption";
			customOLFlag2Label.OT_Caption = "Caption";
			customOLFlag3Label.OT_Caption = "Caption";
			customOLFlag4Label.OT_Caption = "Caption";
			customOLFlag5Label.OT_Caption = "Caption";

			orderBizObjLine.JO_CustomAttrib1 = "aa";
			orderBizObjLine.JO_CustomAttrib2 = "bb";
			orderBizObjLine.JO_CustomAttrib3 = "cc";
			orderBizObjLine.JO_CustomAttrib4 = "dd";
			orderBizObjLine.JO_CustomAttrib5 = "ee";
			orderBizObjLine.JO_CustomAttrib6 = "ff";
			orderBizObjLine.JO_CustomTextBlob1 = "TB1";
			var now = ZDateTime.Now;
			orderBizObjLine.JO_CustomDate1 = now;
			orderBizObjLine.JO_CustomDate2 = now.AddDays(1);
			orderBizObjLine.JO_CustomDate3 = now.AddDays(2);
			orderBizObjLine.JO_CustomDate4 = now.AddDays(3);
			orderBizObjLine.JO_CustomDate5 = now.AddDays(4);
			orderBizObjLine.JO_CustomDecimal1 = 1.1m;
			orderBizObjLine.JO_CustomDecimal2 = 2.2m;
			orderBizObjLine.JO_CustomDecimal3 = 3.3m;
			orderBizObjLine.JO_CustomDecimal4 = 4.4m;
			orderBizObjLine.JO_CustomDecimal5 = 5.5m;
			orderBizObjLine.JO_CustomFlag1 = true;
			orderBizObjLine.JO_CustomFlag2 = false;
			orderBizObjLine.JO_CustomFlag3 = true;
			orderBizObjLine.JO_CustomFlag4 = false;
			orderBizObjLine.JO_CustomFlag5 = true;

			var adapter = new TestOrderValueObjectDataAdapter();
			var xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 1, xsdOrder.OrderLines.Count);
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines[0];
			AssertEquals("CustomAttrib1", "aa", xsdOrderLine.OrderLineDetail.Custom.Text1);
			AssertEquals("CustomAttrib2", "bb", xsdOrderLine.OrderLineDetail.Custom.Text2);
			AssertEquals("CustomAttrib3", "cc", xsdOrderLine.OrderLineDetail.Custom.Text3);
			AssertEquals("CustomAttrib4", "dd", xsdOrderLine.OrderLineDetail.Custom.Text4);
			AssertEquals("CustomAttrib5", "ee", xsdOrderLine.OrderLineDetail.Custom.Text5);
			AssertEquals("CustomAttrib6", "ff", xsdOrderLine.OrderLineDetail.Custom.Text6);
			AssertEquals("CustomText1", "TB1", xsdOrderLine.OrderLineDetail.Custom.CustomText1);
			AssertEquals("CustomDate1", now, xsdOrderLine.OrderLineDetail.Custom.Date1);
			AssertEquals("CustomDate2", now.AddDays(1), xsdOrderLine.OrderLineDetail.Custom.Date2);
			AssertEquals("CustomDate3", now.AddDays(2), xsdOrderLine.OrderLineDetail.Custom.Date3);
			AssertEquals("CustomDate4", now.AddDays(3), xsdOrderLine.OrderLineDetail.Custom.Date4);
			AssertEquals("CustomDate5", now.AddDays(4), xsdOrderLine.OrderLineDetail.Custom.Date5);
			AssertEquals("CustomDecimal1", 1.1m, xsdOrderLine.OrderLineDetail.Custom.Decimal1);
			AssertEquals("CustomDecimal2", 2.2m, xsdOrderLine.OrderLineDetail.Custom.Decimal2);
			AssertEquals("CustomDecimal3", 3.3m, xsdOrderLine.OrderLineDetail.Custom.Decimal3);
			AssertEquals("CustomDecimal4", 4.4m, xsdOrderLine.OrderLineDetail.Custom.Decimal4);
			AssertEquals("CustomDecimal5", 5.5m, xsdOrderLine.OrderLineDetail.Custom.Decimal5);
			AssertEquals("CustomFlag1", true, xsdOrderLine.OrderLineDetail.Custom.Flag1);
			AssertEquals("CustomFlag2", false, xsdOrderLine.OrderLineDetail.Custom.Flag2);
			AssertEquals("CustomFlag3", true, xsdOrderLine.OrderLineDetail.Custom.Flag3);
			AssertEquals("CustomFlag4", false, xsdOrderLine.OrderLineDetail.Custom.Flag4);
			AssertEquals("CustomFlag5", true, xsdOrderLine.OrderLineDetail.Custom.Flag5);
		}

		public void TestExportOrderLineDeliveries()
		{
			var orderBizObj = Factory.New<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			orderBizObj.BuyerPK = buyer.PK;
			var orderBizObjLine = orderBizObj.OrderLines.AddNew();
			var buyerAddress = buyer.MainAddress;
			buyerAddress.OA_Address1 = "123 XYZ STREET";
			var bizObjDelivery = orderBizObjLine.Deliveries.AddNew();
			bizObjDelivery.J4_OA_NKDeliveryPoint = "123 XYZ STREET";
			bizObjDelivery.J4_RL_NKDestinationPort = "AAA";
			bizObjDelivery.J4_Allocated = 2000M;

			var adapter = new TestOrderValueObjectDataAdapter();
			var xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 1, xsdOrder.OrderLines.Count);
			var xsdOrderLine = xsdOrder.OrderLines[0];

			AssertEquals("OrderLineDeliveries Count", 1, xsdOrderLine.OrderLineDeliveries.Count);
			var xsdDelivery = xsdOrderLine.OrderLineDeliveries[0];
			AssertEquals("Address Sequence Ref", 1, xsdDelivery.DeliveryDetails.Address.AddressSequenceRef);
			AssertEquals("Organisation Name", "Buyer", xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Name);

			AssertEquals("Addresses Count", 1, xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.Count);
			var xsdAddress = xsdDelivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses[0];

			AssertEquals("Sequence Specified", true, xsdAddress.SequenceSpecified);
			AssertEquals("Sequence", 1, xsdAddress.Sequence);
			AssertEquals("Address Type Specified", true, xsdAddress.AddressCapabilityTypeSpecified);
			AssertEquals("Address Type", true, xsdAddress.AddressCapabilities.HasCapabilityOfType(Xsd.AddressCapabilityAddressType.MAIN));
			AssertEquals("Address Line 1", "123 XYZ STREET", xsdAddress.AddressLine1);
			AssertEquals("Del Port", false, xsdDelivery.DeliveryDetails.DelPort.IsSpecified);
			AssertEquals("Qty Allocated", 2000M, xsdDelivery.DeliveryDetails.QtyAllocated);
		}

		public void TestExportOrderLineDeliveryContainers()
		{
			Order orderBizObj = Factory.New<Order>();
			OrderLine orderBizObjLine = orderBizObj.OrderLines.AddNew();
			OrderLineDelivery bizObjDelivery = orderBizObjLine.Deliveries.AddNew();
			OrderLineDeliverContainer bizObjContainer = bizObjDelivery.Containers.AddNew();
			bizObjContainer.J5_ContainerNum = "C00";
			bizObjContainer.J5_RC_NKContainerType = "20FR";
			bizObjContainer.J5_ContainerSeal = "S11";
			bizObjContainer.J5_ETA = new DateTime(2005, 03, 25);
			bizObjContainer.J5_ETD = new DateTime(2005, 03, 26);
			bizObjContainer.J5_RL_NKLoadPort = "LLL";
			bizObjContainer.J5_MasterBill = "M44";
			bizObjContainer.J5_PackCount = 28;
			bizObjContainer.J5_F3_NKPackType = "PKG";
			bizObjContainer.J5_RV_NKArrivalVessel = "Container Vessel";
			bizObjContainer.J5_Voyage = "V22";
			bizObjContainer.J5_Volume = 77M;
			bizObjContainer.J5_VolumeUQ = "M3";
			bizObjContainer.J5_Weight = 1300M;
			bizObjContainer.J5_WeightUQ = "KG";

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			AssertEquals("OrderLines Count", 1, xsdOrder.OrderLines.Count);
			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines[0];
			AssertEquals("OrderLineDeliveries Count", 1, xsdOrderLine.OrderLineDeliveries.Count);
			Xsd.OrderOrderLineOrderLineDelivery xsdDelivery = xsdOrderLine.OrderLineDeliveries[0];
			AssertEquals("Delivery Containers Count", 1, xsdDelivery.DeliveryContainers.Count);
			Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainer xsdContainer = xsdDelivery.DeliveryContainers[0];
			AssertEquals("Container Number", "C00", xsdContainer.Container.ContainerNumber);
			AssertEquals("Container Type", "22P1", xsdContainer.Container.ContainerType.ISOCode);
			AssertEquals("Packing Mode", Xsd.ContainerMode.FCL, xsdContainer.Container.PackingMode);
			AssertEquals("Seal", "S11", xsdContainer.Container.Seal);
			AssertEquals("ETA Specified", true, xsdContainer.ETA.IsValid);
			AssertEquals("ETA", new DateTime(2005, 03, 25), xsdContainer.ETA);
			AssertEquals("ETD Specified", true, xsdContainer.ETD.IsValid);
			AssertEquals("ETD", new DateTime(2005, 03, 26), xsdContainer.ETD);
			AssertEquals("Load Port", false, xsdContainer.LoadPort.IsSpecified);
			AssertEquals("Master Bill No", "M44", xsdContainer.MasterBillNo);
			AssertEquals("Pack Count Specified", true, xsdContainer.PackCountSpecified);
			AssertEquals("Pack Count", 28, xsdContainer.PackCount);
			AssertEquals("Pack Type", "PKG", xsdContainer.PackType);
			AssertEquals("Vessel", "Container Vessel", xsdContainer.Vessel);
			AssertEquals("Voyage", "V22", xsdContainer.Voyage);
			AssertEquals("Value", 77M, xsdContainer.Volume.Value);
			AssertEquals("Dimension	Type", "M3", xsdContainer.Volume.DimensionType);
			AssertEquals("Value", 1300M, xsdContainer.Weight.Value);
			AssertEquals("Dimension	Type", "KG", xsdContainer.Weight.DimensionType);
		}

		public void TestEmptyPartNoExport()
		{
			Order orderBizObj = Factory.New<Order>();
			OrderLine orderBizObjLine = orderBizObj.OrderLines.AddNew();
			orderBizObjLine.JO_Partno = "P99";

			OrderLine orderBizObjLine2 = orderBizObj.OrderLines.AddNew();

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));

			Xsd.OrderOrderLine xsdOrderLine = xsdOrder.OrderLines[0];
			AssertEquals("Product", "P99", xsdOrderLine.OrderLineDetail.Product);

			Xsd.OrderOrderLine xsdOrderLine2 = xsdOrder.OrderLines[1];
			AssertEquals("Product", "*EMPTY*", xsdOrderLine2.OrderLineDetail.Product);
		}

		public void TestExportReferenceNumberAlwaysEmpty()
		{
			Order orderBizObj = Factory.New<Order>();
			BusinessObject linkedBizObj = CommonShipment.New(Factory);
			linkedBizObj[JobShipmentSchema.JS_UniqueConsignRef.Name] = "S0000001";
			orderBizObj.JD_JS = linkedBizObj.PK;
			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			AssertEquals("Reference Number should not be specified", false, xsdOrder.OrderDetail.ReferenceNumber.IsSpecified);
			AssertEquals("Reference Number should be empty", ZString.Empty, xsdOrder.OrderDetail.ReferenceNumber.Value);

			orderBizObj.JD_JS = ZGuid.Empty;
			linkedBizObj = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			linkedBizObj[JobDeclarationSchema.JE_DeclarationReference.Name] = "B0000001";
			orderBizObj.JD_JE = linkedBizObj.PK;
			xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			AssertEquals("Reference Number should not be specified", false, xsdOrder.OrderDetail.ReferenceNumber.IsSpecified);
			AssertEquals("Reference Number should be empty", ZString.Empty, xsdOrder.OrderDetail.ReferenceNumber.Value);

			orderBizObj.JD_JE = ZGuid.Empty;
			xsdOrder = adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			AssertEquals("Reference Number should NOT be specified", false, xsdOrder.OrderDetail.ReferenceNumber.IsSpecified);
		}

		public void TestOrganisationTypeOnImport()
		{
			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderDetail.Buyer = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Buyer", OrganisationTypes.Consignee);
			orderValue.OrderDetail.Supplier = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Supplier", OrganisationTypes.Consignor);
			orderValue.OrderDetail.ShipmentPlanning.SendingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "SendingAgent", OrganisationTypes.Forwarder);
			orderValue.OrderDetail.ShipmentPlanning.ReceivingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "ReceivingAgent", OrganisationTypes.Forwarder);

			Order importedOrder = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Buyer", "Buyer", importedOrder.Buyer.OH_FullName);
			AssertEquals("Buyer is consignee", true, importedOrder.Buyer.OH_IsConsignee);
			AssertEquals("Supplier", "Supplier", importedOrder.Supplier.OH_FullName);
			AssertEquals("Supplier is consignor", true, importedOrder.Supplier.OH_IsConsignor);
			AssertEquals("SendingAgent", "SendingAgent", importedOrder.SendingAgent.OH_FullName);
			AssertEquals("SendingAgent is forwarder", true, importedOrder.SendingAgent.OH_IsForwarder);
			AssertEquals("ReceivingAgent", "ReceivingAgent", importedOrder.ReceivingAgent.OH_FullName);
			AssertEquals("ReceivingAgent is forwarder", true, importedOrder.ReceivingAgent.OH_IsForwarder);
		}

		public void TestImportedExchangeRateBasis()
		{
			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();

			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderDetail.ExchangeRate = 2;
			orderValue.OrderDetail.ExchangeRateSpecified = true;

			orderValue.OrderDetail.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.L;
			Order orderWithLowBasis = adapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Exchange rate with L", 0.5m, orderWithLowBasis.JD_EstimatedExchangeRate);

			orderValue.OrderDetail.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.F;
			Order orderWithHightBasis = adapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Exchange rate with F", 2m, orderWithHightBasis.JD_EstimatedExchangeRate);
		}

		public void TestMoreThan2MilestoneUserDatesValidation()
		{
			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderDetail.Milestones.UserDate.Add(new Xsd.MilestoneDates());
			orderValue.OrderDetail.Milestones.UserDate.Add(new Xsd.MilestoneDates());
			orderValue.OrderDetail.Milestones.UserDate.Add(new Xsd.MilestoneDates());

			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Should have an error indicating more than 2 user milestone dates isn't supported", true, Notifications.AsString.IndexOf("Only 2 custom dates are supported") != -1);
		}

		public void TestCannotHaveContainerPackingModeDifferentToOrderPackingModeValidation()
		{
			Xsd.Order orderValue = new Xsd.Order();
			orderValue.OrderDetail.ContainerMode = Xsd.OrderContainerMode.AIR;
			orderValue.OrderDetail.ContainerModeSpecified = true;
			Xsd.OrderOrderLine orderLine = orderValue.OrderLines.AddNew();
			Xsd.OrderOrderLineOrderLineDelivery delivery = orderLine.OrderLineDeliveries.AddNew();
			Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainer container = delivery.DeliveryContainers.AddNew();
			container.Container.PackingMode = Xsd.ContainerMode.FCL;

			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();
			Order order = adapter.CreateOrUpdateFromValueObject(orderValue, Context);
			AssertEquals("Should have an error indicating packing mode on container/order inconsistent", true, Notifications.AsString.IndexOf("You cannot have a packing mode on the container that differs from that of the order") != -1);
		}

		public void TestSupplierNotPopulatedFromBuyerSupplierLink()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "buyercode";
			var linkedSupplier = Factory.NewWithValidTestData<OrgHeader>();
			buyer.SupplierLinks.AddNew(linkedSupplier);

			var notLinkedSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.NewWithValidTestData<Order>();
			order.SupplierPK = notLinkedSupplier.PK;

			Factory.Save();

			var orderValue = new Xsd.Order();
			orderValue.OrderDetail.Buyer.EDICode = "buyercode";

			var adapter = new OrderValueObjectDataAdapter();
			adapter.ImportFromValueObject(order, orderValue, Context);

			AssertEquals("Supplier should NOT come from link on buyer", notLinkedSupplier.PK, order.SupplierPK);
		}

		public void TestAddImportEvent()
		{
			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderIdentifier.OrderNumber = "O11111111";
			xsdOrder.OrderIdentifier.IsSpecified = true;
			xsdOrder.OrderDetail.IsSpecified = true;
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			OrgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);

			Order importedOrder = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			StmALog[] dataImportEvents = importedOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new Order", 1, dataImportEvents.Length);

			Factory.Save();

			xsdOrder.OrderDetail.ConfirmNumber = "C22222222";
			importedOrder = adapter.CreateOrUpdateFromValueObject(xsdOrder, Context);
			dataImportEvents = importedOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to existing Order", 2, dataImportEvents.Length);
		}

		public void TestAddExportEvent()
		{
			OrderValueObjectDataAdapter<Order, Xsd.Order> adapter = new OrderValueObjectDataAdapter();
			var orderBizObj = Factory.NewWithValidTestData<Order>();
			StmALog[] dataExportEvents = orderBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to order", 0, dataExportEvents.Length);

			adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			dataExportEvents = orderBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to order", 1, dataExportEvents.Length);

			adapter.ExportToValueObject(orderBizObj, new ValueObjectExportContext(Notifications));
			dataExportEvents = orderBizObj.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to order", 2, dataExportEvents.Length);
		}

		public void TestFromXmlInterchange_NoError()
		{
			bool valueBefore = OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value;
			try
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				ZString orderNo = "splaty";
				ZString product = "Part1";
				Xsd.Orders xsdOrders = new Xsd.Orders();
				xsdOrders.IsSpecified = true;
				xsdOrders.Order.IsSpecified = true;

				Order order1 = CreateOrder(orderNo, 1, Constants.OrderStatus.Cancelled);

				Xsd.Order orderValue = CreateXsdOrder(orderNo, 0, product, 1, 0, 2, 0);
				orderValue.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Incomplete;
				xsdOrders.Order.Add(orderValue);

				Order order2 = CreateOrder(orderNo, 0, Constants.OrderStatus.Incomplete);

				CommonShipment shipment = CommonShipment.New(Factory);
				shipment.JS_UniqueConsignRef = "S0000001";
				orderValue = CreateXsdOrder(orderNo, 0, product, 1, 0, 2, 0);
				orderValue.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Delivered;
				orderValue.OrderDetail.ReferenceNumber.Value = "S0000001";
				orderValue.OrderDetail.ReferenceNumber.Type = Xsd.OrderOrderDetailReferenceNumberType.Shipment;
				xsdOrders.Order.Add(orderValue);

				ZString orderNo2 = "splaty2";

				Order order3 = CreateOrder(orderNo2, 2, Constants.OrderStatus.Incomplete);
				order3.JD_JS = shipment.PK;

				orderValue = CreateXsdOrder(orderNo2, 2, product, 1, 0, 2, 0);
				orderValue.OrderDetail.OrderStatusSpecified = true;
				orderValue.OrderIdentifier.OrderNumberSplitSpecified = true;
				orderValue.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Delivered;
				xsdOrders.Order.Add(orderValue);

				BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				Order order4 = CreateOrder(orderNo2, 3, Constants.OrderStatus.Incomplete);
				order4.JD_JE = declaration.PK;

				orderValue = CreateXsdOrder(orderNo2, 3, product, 1, 0, 2, 0);
				orderValue.OrderDetail.OrderStatusSpecified = true;
				orderValue.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Delivered;
				xsdOrders.Order.Add(orderValue);

				Factory.Save();

				TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();

				Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
				interchange.InterchangeInfo = new Xsd.InterchangeInfo();
				interchange.Payload.Data = xsdOrders.Order;
				interchange.Payload.DataAdapter = adapter;
				interchange.Version = "";

				AssertEquals("PreCondition: Order1's should have 0 Lines", 0, order1.OrderLines.Count);
				AssertEquals("PreCondition: Order2's should have 0 Lines", 0, order2.OrderLines.Count);
				AssertEquals("PreCondition: Order3's should have 0 Lines", 0, order3.OrderLines.Count);
				AssertEquals("PreCondition: Order4's should have 0 Lines", 0, order4.OrderLines.Count);

				ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, Notifications);
				adapter.FromXmlInterchange(null, context);

				AssertEquals("Buffer contains: " + Notifications.AsString, false, Notifications.HasErrors);

				AssertEquals("Order1's Status should have been set to Delivered with higher split", Constants.OrderStatus.Delivered, order1.JD_OrderStatus);
				AssertEquals("Order1's should have 2 Lines", 2, order1.OrderLines.Count);

				AssertEquals("Order2's Status should not have been set to Delivered", Constants.OrderStatus.Incomplete, order2.JD_OrderStatus);
				AssertEquals("Order2's should have 0 Lines", 0, order2.OrderLines.Count);

				AssertEquals("Order3's Status shouldn't change as it linked to a shipment", Constants.OrderStatus.Incomplete, order3.JD_OrderStatus);
				AssertEquals("Order3's should have 0 Line", 0, order3.OrderLines.Count);
				AssertEquals("Shouldn't try to update this order because of lower split number -- Buffer contains: " + System.Environment.NewLine + System.Environment.NewLine + Notifications.AsString, true, Notifications.AsString.IndexOf(order3.HumanReadableName + " is linked to a shipment and cannot be updated.") >= 0);

				AssertEquals("Order4's Status shouldn't change as it linked to a declaration", Constants.OrderStatus.Incomplete, order4.JD_OrderStatus);
				AssertEquals("Order4's should have 0 Line", 0, order4.OrderLines.Count);
				AssertEquals("Buffer contains: " + Notifications.AsString, true, Notifications.AsString.IndexOf(order4.HumanReadableName + " is linked to a declaration and cannot be updated.") >= 0);
			}
			finally
			{
				OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, valueBefore);
			}
		}

		public void TestFromXmlInterchange_CancelOrders()
		{
			ZString orderNo = "splaty";
			ZString product = "Part1";
			Order order1 = CreateOrder(orderNo, 0, Constants.OrderStatus.Cancelled);

			Order order2 = CreateOrder(orderNo, 1, Constants.OrderStatus.Incomplete);

			CommonShipment shipment = CommonShipment.New(Factory);
			Order order3 = CreateOrder(orderNo, 2, Constants.OrderStatus.Incomplete);
			order3.JD_JS = shipment.PK;

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			Order order4 = CreateOrder(orderNo, 3, Constants.OrderStatus.Incomplete);
			order4.JD_JE = declaration.PK;

			Factory.Save();

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();

			Xsd.Orders xsdOrders = new Xsd.Orders();
			xsdOrders.IsSpecified = true;
			xsdOrders.Order.IsSpecified = true;
			Xsd.Order orderValue = CreateXsdOrder(orderNo, 0, "", 0, 0, 0, 0);
			orderValue.OrderDetail.OrderStatusSpecified = true;
			orderValue.OrderDetail.OrderStatus = Core.Constants.OrderStatus.Cancelled;
			xsdOrders.Order.Add(orderValue);

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo = new Xsd.InterchangeInfo();
			interchange.Payload.Data = xsdOrders.Order;
			interchange.Payload.DataAdapter = adapter;
			interchange.Version = "";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, Notifications);
			adapter.FromXmlInterchange(null, context);
			AssertEquals("Buffer contains: " + Notifications.AsString, false, Notifications.HasErrors);
			AssertEquals("Order splaty should be active", ZBool.False, order1.JD_IsCancelled);
			AssertEquals("Order splaty should have cancelled status", Constants.OrderStatus.Cancelled, order1.JD_OrderStatus);
			AssertEquals("Order splaty-1 should have been cancelled", ZBool.True, order2.JD_IsCancelled);
			AssertEquals("Order splaty-2 should not be inactive as it is linked to a shipment", ZBool.False, order3.JD_IsCancelled);
			AssertContains("Order splaty-2 is linked to a shipment and cannot be canceled.", Notifications.AsString);
			AssertEquals("Order splaty-3 should not be inactive as it is linked to a declaration", ZBool.False, order4.JD_IsCancelled);
			AssertContains("Order splaty-3 is linked to a declaration and cannot be canceled.", Notifications.AsString);
		}

		public void TestSublineOfZeroIsConvertedToOne()
		{
			Xsd.Orders xsdOrders = new Xsd.Orders();
			xsdOrders.IsSpecified = true;
			xsdOrders.Order.IsSpecified = true;
			Xsd.Order orderValue = CreateXsdOrder("orderNo", 0, "prod", 1, 0, 0, 0);
			xsdOrders.Order.Add(orderValue);

			orderValue.OrderLines[0].OrderSubLineNoSpecified = true;

			Factory.Save();

			TestOrderValueObjectDataAdapter adapter = new TestOrderValueObjectDataAdapter();

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo = new Xsd.InterchangeInfo();
			interchange.Payload.Data = xsdOrders.Order;
			interchange.Payload.DataAdapter = adapter;
			interchange.Version = "";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, Notifications);
			Order[] impOrders = adapter.FromXmlInterchange(null, context);
			OrderLine importedLine1 = impOrders[0].OrderLines[0];
			AssertEquals("import should convert subline 0 to 1", 1, importedLine1.JO_SubLineNo);
		}

		[ExpectNoExceptions()]
		public void TestSublineOfZeroFindsSublineOfOne()
		{
			Order myOrder = CreateOrder("orderNo", 0, Constants.OrderStatus.Confirmed, "prod1", 1, 0, 0, 0);
			AssertEquals("prod1", myOrder.OrderLines[0].JO_Partno);
			TestSublineOfZeroIsConvertedToOne();
			AssertEquals("prod", myOrder.OrderLines[0].JO_Partno);
		}

		#region Order Import With Nillable Split

		public void TestImportOrderWithSplit_0_UpdatesFirstOrder()
		{
			Order orderSplit0 = CreateOrderFromXML(OrderImportSplitZeroTestFileName, 0);
			Order orderSplit1 = CreateOrderFromXML(OrderImportSplitOneTestFileName, 1);
			orderSplit0.JD_IncoTerm = "EXW";
			orderSplit1.JD_IncoTerm = "EXW";

			Factory.Save();

			CreateOrderFromXML(OrderImportSplitZeroTestFileName, 0);
			AssertEquals("Import should update order with split 0", "FOB", orderSplit0.JD_IncoTerm);
			AssertEquals("Import should not update order with split 1", "EXW", orderSplit1.JD_IncoTerm);
		}

		public void TestImportOrderWithSplit_1_UpdatesSecondOrder()
		{
			TestImportOrderWithSplit_0_UpdatesFirstOrder();
			Order orderSplit1 = CreateOrderFromXML(OrderImportSplitOneTestFileName, 1);
			AssertEquals("Import should update order with split 1", "FOB", orderSplit1.JD_IncoTerm);
		}

		public void TestImportOrderWithMissingSplitUpdatesSecondOrder()
		{
			TestImportOrderWithSplit_0_UpdatesFirstOrder();
			Order orderSplit1 = CreateOrderFromXML(OrderImportSplitMissingTestFileName, 1);
			AssertEquals("Import should update order with split 1 when split number is missing", "FOB", orderSplit1.JD_IncoTerm);
		}

		public void TestImportOrderWithSplit_2_CreatesNewOrder()
		{
			TestImportOrderWithMissingSplitUpdatesSecondOrder();
			AssertEquals("Should create only two orders", 2, Factory.Load<Order>(new ZQuery()).Length);

			Order newOrder = CreateOrderFromXML(OrderImportSplitTwoTestFileName, 2);
			Factory.Save();

			AssertEquals("Should create a new order", 3, Factory.Load<Order>(new ZQuery()).Length);
			AssertEquals("new order should get the new split", (byte)2, newOrder.JD_OrderNumberSplit);
		}

		public void TestImportOrderWithSplit_Missing_CreatesNewOrder()
		{
			Order newOrder = CreateOrderFromXML(OrderImportSplitMissingTestFileName, 0);
			AssertEquals("Should create a new order", 1, Factory.Load<Order>(new ZQuery()).Length);
			AssertEquals("new order should get the new split", (byte)0, newOrder.JD_OrderNumberSplit);
		}

		public void TestImportOrderWithOutOfMaxRange()
		{
			var newOrder = CreateOrderFromXML(OrderImportWithOutOfRangeDate1TestFileName, 0);
			AssertNull(newOrder);

			var orders = Factory.Load<Order>(new ZQuery());
			AssertEquals(1, orders.Length);

			var order1 = orders[0];
			Assert("JD_ExWorksRequiredBy should be set only when ExWorksRequiredBy is small date time", order1.JD_ExWorksRequiredBy.IsEmpty);
			Assert("JD_DeliveryRequiredBy should be set only when DeliveryRequiredBy is small date time", order1.JD_DeliveryRequiredBy.IsEmpty);
		}

		public void TestImportOrderWithOutOfMinRange()
		{
			var newOrder = CreateOrderFromXML(OrderImportWithOutOfRangeDate2TestFileName, 0);
			AssertNull(newOrder);

			var orders = Factory.Load<Order>(new ZQuery());
			AssertEquals(1, orders.Length);

			var order1 = orders[0];
			Assert("JD_ExWorksRequiredBy should be set only when ExWorksRequiredBy is small date time", order1.JD_ExWorksRequiredBy.IsEmpty);
			Assert("JD_DeliveryRequiredBy should be set only when DeliveryRequiredBy is small date time", order1.JD_DeliveryRequiredBy.IsEmpty);
		}

		Order CreateOrderFromXML(string xmlFileName, byte split)
		{
			var serializer = new OrderXMLValueObjectSerializer();
			Order result = null;
			using (var fileStream = resourceRetriever.Value.GetStream($"Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.{xmlFileName}"))
			{
				var orders = new OrderCollection(Factory);
				serializer.ImportXmlData(fileStream, DataAdapter, orders, null, Notifications);
				foreach (var order in orders)
				{
					if (order.JD_OrderNumberSplit == split)
					{
						result = order;
						break;
					}
				}
				return result;
			}
		}

		const string OrderImportSplitZeroTestFileName = "11_SplitNumber0.xml";
		const string OrderImportSplitOneTestFileName = "11_SplitNumber1.xml";
		const string OrderImportSplitTwoTestFileName = "11_SplitNumber2.xml";
		const string OrderImportSplitMissingTestFileName = "11_NoSplitNumber.xml";
		const string OrderImportWithOutOfRangeDate1TestFileName = "OrderWithOutOfRangeDate1.xml";
		const string OrderImportWithOutOfRangeDate2TestFileName = "OrderWithOutOfRangeDate2.xml";

		#endregion

		#region Overrides for base test

		Guid originalNotificationGroup;

		protected override void SetUp()
		{
			base.SetUp();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			ImportedOrderChangesNotificationGroup.Factory.Save();
			originalNotificationGroup = NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ImportedOrderChangesNotificationGroup.PK.ToGuid());
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalNotificationGroup);
			if (OrgMatch != null && GlbCompany.CurrentCompany.OrgProxy.PatternMatchOverrides_ForBinding.Contains(OrgMatch.PK))
			{
				GlbCompany.CurrentCompany.OrgProxy.PatternMatchOverrides_ForBinding.Remove(OrgMatch.PK);
			}
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		GlbGroup ImportedOrderChangesNotificationGroup
		{
			get
			{
				if (importedOrderChangesNotificationGroup == null)
				{
					importedOrderChangesNotificationGroup = Factory.New<GlbGroup>();
					GlbStaff staff = importedOrderChangesNotificationGroup.Staff.AddNew();
					staff.GS_Code = "CRV";
					staff.GS_EmailAddress = "clinton.volzke@cargowise.com";
				}
				return importedOrderChangesNotificationGroup;
			}
		}
		GlbGroup importedOrderChangesNotificationGroup;

		protected override ValueObjectDataAdapter<Order, Xsd.Order> GetNewBizObjXmlDataAdapter()
		{
			return new OrderValueObjectDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Orders"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Order"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyOrder = Factory.New<Order>();
			emptyOrder.JD_OrderNumberSplit = 3;
			emptyOrder.BuyerPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			emptyOrder.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			emptyOrder.JD_OrderDate = new ZDateTime(2005, 1, 1);

			var emptyOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.EmptyOrder.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyOrder, emptyOrderPath, ValidationKind.None, "Empty order");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);

			ZQuery filter = new ZQuery(RefContainerSchema.RC_ShippingMode, "AIR");
			RefContainerCollection containers = new RefContainerCollection(Factory);
			containers.AdditionalFilter = filter;
			containers.DeleteAll();
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG500";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Code = "TEST500";
			address1.OA_Address1 = "Address1";
			address1.OA_OH = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG501";

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "TEST501";
			address2.OA_Address1 = "Address2";
			address2.OA_OH = org2.PK;

			Order order = Factory.NewWithValidTestData<Order>(TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply);
			order.JD_OrderNumberSplit = 8;
			order.Buyer.OH_Code = "BUYER";
			order.Buyer.OH_FullName = "BUYER";
			order.Buyer.PrimaryRegistrationNumber.Number = "BUYER";

			order.Supplier.OH_Code = "SUPPLI";
			order.Supplier.OH_FullName = "SUPPLI";
			order.Supplier.PrimaryRegistrationNumber.Number = "SUPPLI";

			order.GoodsDeliveredToAddress.E2_OA_Address = address1.PK;
			order.GoodsAvailableAtAddress.E2_OA_Address = address2.PK;

			order.SendingAgent.OH_Code = "SENDIN";
			order.SendingAgent.OH_FullName = "SENDIN";
			order.SendingAgent.PrimaryRegistrationNumber.Number = "SENDIN";

			order.ReceivingAgent.OH_Code = "RECEIV";
			order.ReceivingAgent.OH_FullName = "RECEIV";
			order.ReceivingAgent.PrimaryRegistrationNumber.Number = "RECEIV";

			order.OrderLines[0].JO_Partno = "part";

			var newContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			order.PlannedContainers[0].J1_RC = newContainer.PK;
			var populatedOrderWithEmptyFieldsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.PopulatedOrderWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(order, populatedOrderWithEmptyFieldsPath, ValidationKind.None, "Populated order with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var order = NewOrderWithValidTestData(Constants.TransportModes.Air);
			var airOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.AirOrder.xml");
			return new BusinessObjectAndExpectedOutputFileName(order, airOrderPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Air");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var seaOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.SeaOrder.xml");
			var railOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.RailOrder.xml");
			var result = new ArrayList
			{
				new BusinessObjectAndExpectedOutputFileName(NewOrderWithValidTestData(Constants.TransportModes.Sea), seaOrderPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Sea"),
				new BusinessObjectAndExpectedOutputFileName(NewOrderWithValidTestData(Constants.TransportModes.Rail), railOrderPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Rail")
			};
			return (BusinessObjectAndExpectedOutputFileName[])result.ToArray(typeof(BusinessObjectAndExpectedOutputFileName));
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// handled by other data adapters
					"Events",
					"OrderDetail/Buyer",
					"OrderDetail/Supplier",
					"OrderDetail/DocAddresses/DocAddress",
					"OrderLines/OrderLineDeliveries/DeliveryDetails/Address/Organisation",
					"OrderDetail/ShipmentPlanning/PlannedContainers/Type/USContainerCode",
					"OrderDetail/ShipmentPlanning/SendingAgent",
					"OrderDetail/ShipmentPlanning/ReceivingAgent",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/Container",
					"Notes",
					"CustomValues",
					"OrderDetail/CustomValues",

					// shouldn't be used
					"OrderDetail/Milestones/UserDate/Actual",
					"OrderLines/OrderLineDetail/UnitQty",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/Container/IsShipperOwnedContainer",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/Container/Weight",

					// TODO
					"OrderLines/OrderLineDeliveries/DeliveryContainers/CustomFlag",
					"OrderLines/OrderLineDeliveries/DeliveryDetails/Custom",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/CustomText",

					// maybe todo
					"OrderDetail/ShipmentPlanning/Weight/Description",
					"OrderDetail/ShipmentPlanning/Volume/Description",
					"OrderDetail/ShipmentPlanning/Packs/Description",
					"OrderDetail/ShipmentPlanning/GoodsAvailAt",
					"OrderDetail/ShipmentPlanning/GoodsDelivTo",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/Volume/Description",
					"OrderLines/OrderLineDeliveries/DeliveryContainers/Weight/Description",
					"OrderLines/OrderLineDetail/QtyOrdered/Description",
					"OrderLines/OrderLineDetail/QtyInvoiced/Description",
					"OrderLines/OrderLineDetail/QtyReceived/Description",
					"OrderLines/OrderLineDetail/QtyReceivedToDate/Description",
					"OrderLines/OrderLineDetail/QtyReceived/DimensionType",
					"OrderLines/OrderLineDetail/InnerPacks/Description",
					"OrderLines/OrderLineDetail/OuterPacks/Description",

					// this cant be covered because some milestones will not be populated
					// depending on which BusinessObject is attached to the order
					"OrderDetail/ReferenceNumber",

					//only use for import 
					"OrderLines/OrderLineDetail/Custom/Text6",

					// tested in DG adapter
					"OrderLines/OrderLineDetail/DangerousGoods",
					"OrderLines/OrderLineDetail/Standard",
					"OrderLines/OrderLineDetail/UniqueRecordId"
				};
			}
		}

		#endregion

		#region Implementation

		Xsd.Order GetXsdOrder(bool hasValidDeliverPoint)
		{
			Xsd.Order xsdOrder = new Xsd.Order();
			xsdOrder.OrderIdentifier = new Xsd.OrderOrderIdentifier();
			xsdOrder.OrderIdentifier.OrderNumber = "1000";
			xsdOrder.OrderIdentifier.OrderNumberSplit = 0;
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			Xsd.OrderOrderLine xsdLine = xsdOrder.OrderLines.AddNew();
			xsdLine.OrderLineNo = 1;
			Xsd.OrderOrderLineOrderLineDelivery xsdDelivery = xsdLine.OrderLineDeliveries.AddNew();

			xsdDelivery.DeliveryDetails.DelPort.Value = "AUSYD";
			Xsd.AddressReference reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = Buyer.OH_Code;
			reference.Organisation.OrganisationDetails.Name = "Delivery Point Org Name";
			Xsd.OrgAddressCollection addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			Xsd.OrgAddress xsdAddress1 = addressCollection.AddNew();
			xsdAddress1.AddressLine1 = "Test Address1";
			xsdAddress1.Sequence = 1;

			Xsd.OrgAddress xsdAddress = addressCollection.AddNew();
			if (hasValidDeliverPoint)
			{
				xsdAddress.AddressLine1 = " 3 / 1075  Beaudesert";
				xsdAddress.AddressLine2 = "WEtheRIll parK";
				xsdAddress.AddressCode = "WETHERILL";
				xsdAddress.AddressType = Xsd.OrgAddressAddressType.DLV;
			}
			else
			{
				xsdAddress.AddressLine1 = "WRONG ADDRESS 1";
				xsdAddress.AddressLine2 = "WRONG ADDRESS 2";
				xsdAddress.AddressCode = "WRONG CODE";
			}
			xsdAddress.Sequence = 2;

			xsdDelivery.DeliveryDetails.Address = reference;
			reference.AddressSequenceRef = 2;

			return xsdOrder;
		}

		Xsd.Order GetXsdOrder()
		{
			Xsd.Order result = new Xsd.Order();
			result.OrderDetail = new Xsd.OrderOrderDetail();
			result.OrderDetail.Buyer = new Xsd.Organisation();
			result.OrderDetail.Buyer.OwnerCode = "XYZ";
			result.OrderIdentifier.OrderNumber = "ORDERNUMBER";
			result.OrderDetail.ConfirmNumber = "R55";
			if (OrgMatch == null)
			{
				OrgMatch = SetOrgMatch(result.OrderDetail.Buyer.OwnerCode);
			}
			return result;
		}

		OrgAddress DeliveryAddress
		{
			get
			{
				if (deliveryAddress == null)
				{
					deliveryAddress = Buyer.Addresses.AddNew();
					deliveryAddress.OA_Address1 = "3/1075 BEAUDESERT RD";
					deliveryAddress.OA_Address2 = "WETHERILL PARK D.C., NSW";
					deliveryAddress.OA_Code = "WETHERILL";
					deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return deliveryAddress;
			}
		}
		OrgAddress deliveryAddress;

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		TestOrderValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new TestOrderValueObjectDataAdapter()); }
		}
		TestOrderValueObjectDataAdapter dataAdapter;

		ValueObjectImportContext Context
		{
			get
			{
				if (fContext == null)
				{
					fContext = new ValueObjectImportContext(Factory, Notifications);
				}
				return fContext;
			}
		}
		ValueObjectImportContext fContext;

		OrgPatternMatchOverride OrgMatch;

		Order NewOrderWithValidTestData(string transportMode)
		{
			BusinessObjectFactory savingFactory = new BusinessObjectFactory();

			OrgHeader buyer = OrgHeader.LoadFromCode(savingFactory, transportMode + "BUYER");
			if (buyer == null)
			{
				buyer = savingFactory.NewWithValidTestData<OrgHeader>();
				buyer.OH_Code = transportMode + "BUYER";
				buyer.OH_FullName = transportMode + " Buyer";
				buyer.MainAddress.OA_Address1 = transportMode + " Buyer";
				buyer.OH_IsConsignee = true;
				savingFactory.Save();
			}
			buyer = Factory.Load<OrgHeader>(buyer.PK);

			var order = Factory.NewWithValidTestData<Order>();

			order.JD_OrderNumber = "OrderNumber";
			order.JD_OrderNumberSplit = 7;
			order.JD_BookingConfRef = "BookingConfRef";
			order.JD_BookingConfDate = new ZDateTime(2006, 12, 25);
			order.JD_InvoiceNumber = "InvoiceNumber";
			order.JD_InvoiceDate = new ZDateTime(2005, 12, 13);
			order.JD_OrderStatus = Core.Constants.OrderStatus.Confirmed;
			order.JD_ExWorksRequiredBy = new ZDateTime(2005, 1, 1);
			order.JD_DeliveryRequiredBy = new ZDateTime(2005, 2, 2);
			order.JD_OrderGoodsDescription = "OrderGoodsDescription";
			order.JD_OrderDate = new ZDateTime(2005, 3, 3);
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.China;
			order.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order.JD_AdditionalTerms = "MAGIC";

			order.BuyerPK = buyer.PK;

			order.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.Supplier.OH_Code = transportMode + "SUPSUP";
			order.Supplier.OH_FullName = transportMode + " Supplier";
			order.Supplier.MainAddress.OA_Address1 = transportMode + " Supplier";

			order.JD_EstimatedExchangeRate = 1.2m; //set here - setting supplier defaults rate

			order.UpdateEventEstimate(Events.Arrival, new ZDateTimeOffset(2005, 1, 1));
			order.UpdateEvent(Events.Arrival, new ZDateTimeOffset(2005, 2, 1));
			order.UpdateEventEstimate(Events.DeliveryCartageAdvised, new ZDateTimeOffset(2005, 1, 2));
			order.UpdateEvent(Events.DeliveryCartageAdvised, new ZDateTimeOffset(2005, 2, 2));
			order.UpdateEventEstimate(Events.CustomsCommenced, new ZDateTimeOffset(2005, 1, 3));
			order.UpdateEvent(Events.CustomsCommenced, new ZDateTimeOffset(2005, 2, 3));
			order.UpdateEventEstimate(Events.CustomsCleared, new ZDateTimeOffset(2005, 1, 4));
			order.UpdateEvent(Events.CustomsCleared, new ZDateTimeOffset(2005, 2, 4));
			order.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2005, 1, 5));
			order.UpdateEvent(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2005, 2, 5));
			order.UpdateEventEstimate(Events.Departure, new ZDateTimeOffset(2005, 1, 6));
			order.UpdateEvent(Events.Departure, new ZDateTimeOffset(2005, 2, 6));
			order.UpdateEventEstimate(Events.ExWorks, new ZDateTimeOffset(2005, 1, 7));
			order.UpdateEvent(Events.ExWorks, new ZDateTimeOffset(2005, 2, 7));
			order.UpdateEventEstimate(Events.GateIn, new ZDateTimeOffset(2005, 1, 8));
			order.UpdateEvent(Events.GateIn, new ZDateTimeOffset(2005, 2, 8));
			order.UpdateEventEstimate(Events.CargoAvailable, new ZDateTimeOffset(2005, 1, 9));
			order.UpdateEvent(Events.CargoAvailable, new ZDateTimeOffset(2005, 2, 9));
			order.JD_EstimateUserDate1 = new ZDateTime(2005, 1, 10);
			order.JD_ActualUserDate1 = new ZDateTime(2005, 2, 10);
			order.JD_EstimateUserDate2 = new ZDateTime(2005, 1, 11);
			order.JD_ActualUserDate2 = new ZDateTime(2005, 2, 11);
			order.JD_EstimateUserDate3 = new ZDateTime(2005, 1, 12);
			order.JD_ActualUserDate3 = new ZDateTime(2005, 2, 12);
			order.JD_EstimateUserDate4 = new ZDateTime(2005, 1, 13);
			order.JD_ActualUserDate4 = new ZDateTime(2005, 2, 13);
			order.JD_RN_NKCountryOfSupply = Core.Constants.CountryCodes.Australia;

			OrgCustomLabels customDecimal1Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal2Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal3Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal4Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customDecimal5Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag1Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag2Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag3Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag4Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customFlag5Label = order.Buyer.CustomLabels.AddNew();

			customDecimal1Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal1;
			customDecimal2Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal2;
			customDecimal3Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal3;
			customDecimal4Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal4;
			customDecimal5Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDecimal5;
			customFlag1Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag1;
			customFlag2Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag2;
			customFlag3Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag3;
			customFlag4Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag4;
			customFlag5Label.OT_FieldName = Core.Constants.CustomLabels.Order.CustomFlag5;

			customDecimal1Label.OT_Caption = "Caption";
			customDecimal2Label.OT_Caption = "Caption";
			customDecimal3Label.OT_Caption = "Caption";
			customDecimal4Label.OT_Caption = "Caption";
			customDecimal5Label.OT_Caption = "Caption";
			customFlag1Label.OT_Caption = "Caption";
			customFlag2Label.OT_Caption = "Caption";
			customFlag3Label.OT_Caption = "Caption";
			customFlag4Label.OT_Caption = "Caption";
			customFlag5Label.OT_Caption = "Caption";

			order.JD_CustomAttrib1 = "aa";
			order.JD_CustomAttrib2 = "bb";
			order.JD_CustomAttrib3 = "cc";
			order.JD_CustomAttrib4 = "dd";
			order.JD_CustomAttrib5 = "ee";
			order.JD_CustomDate1 = new ZDateTime(2006, 1, 1);
			order.JD_CustomDate2 = new ZDateTime(2006, 1, 2);
			order.JD_CustomDecimal1 = 1.1m;
			order.JD_CustomDecimal2 = 2.2m;
			order.JD_CustomDecimal3 = 3.3m;
			order.JD_CustomDecimal4 = 4.4m;
			order.JD_CustomDecimal5 = 5.5m;
			order.JD_CustomFlag1 = true;
			order.JD_CustomFlag2 = false;
			order.JD_CustomFlag3 = true;
			order.JD_CustomFlag4 = false;
			order.JD_CustomFlag5 = true;
			order.JD_FirstBuyerContact = "contact1";
			order.JD_SecondBuyerContact = "contact2";

			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RL_NKPortOfLoading = "AUBNE";
			order.JD_RL_NKPortOfDischarge = "AUPER";
			order.JD_Waybill = "Waybill";
			order.JD_Packs = 3;
			order.JD_F3_NKPackType = Constants.PkgUnit.Coil;
			order.JD_ActualWeight = 2;
			order.JD_UnitOfWeight = Core.Constants.Weight.ShortTons;
			order.JD_ActualVolume = 3;
			order.JD_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			order.JD_RV_NKDepartureVessel = "DepVessel";
			order.JD_DepartureVoyage = "DepVoyage";
			order.JD_RV_NKIntermediateVessel = "IntVessel";
			order.JD_IntermediateVoyage = "IntVoyage";
			order.JD_RV_NKArrivalVessel = "ArvVessel";
			order.JD_ArrivalVoyage = "ArvVoyage";

			OrderContainer orderContainer = order.PlannedContainers.AddNew();
			orderContainer.J1_ContainerNumber = "C123";
			orderContainer.J1_ContainerCount = 1;
			orderContainer.J1_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40FR").PK;

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 2;
			orderLine.JO_SubLineNo = 3;
			orderLine.JO_LineSplitNumber = 4;
			orderLine.JO_Partno = "Partno";
			orderLine.JO_Description = "OrderLineDescription";
			orderLine.JO_Quantity = 5;
			orderLine.JO_QtyInvoiced = 6;
			orderLine.JO_QtyReceived = 5;
			orderLine.JO_InnerPacks = 2;
			orderLine.JO_InnerPacksUQ = Constants.PkgUnit.Bag;
			orderLine.JO_OuterPacks = 4;
			orderLine.JO_OuterPacksUQ = Constants.PkgUnit.Pallet;
			orderLine.JO_ActualVolume = 15;
			orderLine.JO_UnitOfVolume = Constants.Volume.Litre;
			orderLine.JO_ActualWeight = 10;
			orderLine.JO_UnitOfWeight = Constants.Weight.Kilograms;
			orderLine.JO_ItemPrice = 2;
			orderLine.JO_LinePrice = 10;
			orderLine.JO_LineStatus = Core.Constants.OrderStatus.Delivered;
			orderLine.JO_PartAttrib1 = "PartAttrib1";
			orderLine.JO_PartAttrib2 = "PartAttrib2";
			orderLine.JO_PartAttrib3 = "PartAttrib3";
			orderLine.JO_SerialNumber = "SerialNumber";
			orderLine.JO_LineDropDate = new ZDateTime(2005, 9, 9);
			orderLine.JO_INCO = "FOB";
			orderLine.JO_AdditionalTerms = "AdditionalTerms";
			orderLine.JO_ExWorksDate = new ZDateTime(2011, 6, 21);
			orderLine.JO_ConfirmationDate = new ZDateTime(2011, 6, 22);
			orderLine.JO_ConfirmationNum = "LineConf";
			orderLine.JO_SpecialInstructions = "DontDrop";
			orderLine.JO_AdditionalInformation = "MagicSmokeInside";

			UNDGDataItem dgItem = orderLine.UNDGs.AddNew();
			dgItem.DI_DG = Substance.PK;
			dgItem.DI_DGFlashPoint = 0.1m;
			orderLine.JO_ContainerNumber = "ContainerNo";
			orderLine.JO_CommercialInvoiceNo = "1223";
			orderLine.JO_ContainerPackingOrder = 2;
			orderLine.JO_RN_NKCountryOfOrigin = "AU";

			OrgCustomLabels customOLDecimal1Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLDecimal2Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLDecimal3Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLDecimal4Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLDecimal5Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLFlag1Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLFlag2Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLFlag3Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLFlag4Label = order.Buyer.CustomLabels.AddNew();
			OrgCustomLabels customOLFlag5Label = order.Buyer.CustomLabels.AddNew();

			customOLDecimal1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal1;
			customOLDecimal2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal2;
			customOLDecimal3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal3;
			customOLDecimal4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal4;
			customOLDecimal5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDecimal5;
			customOLFlag1Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag1;
			customOLFlag2Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag2;
			customOLFlag3Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag3;
			customOLFlag4Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag4;
			customOLFlag5Label.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomFlag5;

			customOLDecimal1Label.OT_Caption = "Caption";
			customOLDecimal2Label.OT_Caption = "Caption";
			customOLDecimal3Label.OT_Caption = "Caption";
			customOLDecimal4Label.OT_Caption = "Caption";
			customOLDecimal5Label.OT_Caption = "Caption";
			customOLFlag1Label.OT_Caption = "Caption";
			customOLFlag2Label.OT_Caption = "Caption";
			customOLFlag3Label.OT_Caption = "Caption";
			customOLFlag4Label.OT_Caption = "Caption";
			customOLFlag5Label.OT_Caption = "Caption";

			orderLine.JO_CustomAttrib1 = "aa";
			orderLine.JO_CustomAttrib2 = "bb";
			orderLine.JO_CustomAttrib3 = "cc";
			orderLine.JO_CustomAttrib4 = "dd";
			orderLine.JO_CustomAttrib5 = "ee";
			orderLine.JO_CustomDate1 = new ZDateTime(2006, 2, 1);
			orderLine.JO_CustomDate2 = new ZDateTime(2006, 2, 2);
			orderLine.JO_CustomDate3 = new ZDateTime(2006, 2, 3);
			orderLine.JO_CustomDate4 = new ZDateTime(2006, 2, 4);
			orderLine.JO_CustomDate5 = new ZDateTime(2006, 2, 5);
			orderLine.JO_CustomDecimal1 = 1.1m;
			orderLine.JO_CustomDecimal2 = 2.2m;
			orderLine.JO_CustomDecimal3 = 3.3m;
			orderLine.JO_CustomDecimal4 = 4.4m;
			orderLine.JO_CustomDecimal5 = 5.5m;
			orderLine.JO_CustomFlag1 = true;
			orderLine.JO_CustomFlag2 = false;
			orderLine.JO_CustomFlag3 = true;
			orderLine.JO_CustomFlag4 = false;
			orderLine.JO_CustomFlag5 = true;
			orderLine.JO_CustomTextBlob1 = "text1";

			OrderLineDelivery delivery1 = orderLine.Deliveries.AddNew();
			delivery1.J4_RL_NKDestinationPort = "AUADL";
			delivery1.J4_OA_NKDeliveryPoint = order.Buyer.MainAddress.OA_Code;
			delivery1.J4_Allocated = 5;

			OrderLineDelivery delivery2 = orderLine.Deliveries.AddNew();
			delivery2.J4_RL_NKDestinationPort = "AUADL";
			delivery2.J4_OA_NKDeliveryPoint = "TextAdr";
			delivery2.J4_Allocated = 5;

			OrderLineDeliverContainer deliveryContainer = delivery1.Containers.AddNew();
			deliveryContainer.J5_ContainerNum = "C123";
			deliveryContainer.J5_ContainerSeal = "Seal";
			deliveryContainer.J5_MasterBill = "MasterBill";
			deliveryContainer.J5_RL_NKLoadPort = "AUSYD";
			deliveryContainer.J5_RV_NKArrivalVessel = "ArrivalVessel";
			deliveryContainer.J5_Voyage = "Voyage";
			deliveryContainer.J5_ETA = new ZDateTime(2005, 3, 3);
			deliveryContainer.J5_ETD = new ZDateTime(2005, 4, 4);
			deliveryContainer.J5_PackCount = 7;
			deliveryContainer.J5_F3_NKPackType = Constants.PkgUnit.Box;
			deliveryContainer.J5_Volume = 5;
			deliveryContainer.J5_VolumeUQ = Core.Constants.Volume.CubicFeet;
			deliveryContainer.J5_Weight = 6;
			deliveryContainer.J5_WeightUQ = Core.Constants.Weight.ShortTons;
			deliveryContainer.J5_RC_NKContainerType = ZString.Empty;

			deliveryContainer = delivery2.Containers.AddNew();
			deliveryContainer.J5_ContainerNum = "C123";
			deliveryContainer.J5_ContainerSeal = "Seal";
			deliveryContainer.J5_MasterBill = "MasterBill";
			deliveryContainer.J5_RL_NKLoadPort = "AUSYD";
			deliveryContainer.J5_RV_NKArrivalVessel = "ArrivalVessel";
			deliveryContainer.J5_Voyage = "Voyage";
			deliveryContainer.J5_ETA = new ZDateTime(2005, 3, 3);
			deliveryContainer.J5_ETD = new ZDateTime(2005, 4, 4);
			deliveryContainer.J5_PackCount = 7;
			deliveryContainer.J5_F3_NKPackType = Constants.PkgUnit.Box;
			deliveryContainer.J5_Volume = 5;
			deliveryContainer.J5_VolumeUQ = Core.Constants.Volume.CubicFeet;
			deliveryContainer.J5_Weight = 6;
			deliveryContainer.J5_WeightUQ = Core.Constants.Weight.ShortTons;
			deliveryContainer.J5_RC_NKContainerType = ZString.Empty;

			((IBusinessObjectInternals)order).IsCopying = true;
			order.JD_TransportMode = transportMode;
			((IBusinessObjectInternals)order).IsCopying = false;

			order.JD_OH_SendingAgent = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.SendingAgent.OH_Code = transportMode + "SNDAG";
			order.SendingAgent.OH_FullName = transportMode + " SendingAgent";
			order.SendingAgent.MainAddress.OA_Address1 = transportMode + " SendingAgent";

			order.JD_OH_ReceivingAgent = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.ReceivingAgent.OH_Code = transportMode + "RCVAG";
			order.ReceivingAgent.OH_FullName = transportMode + " ReceivingAgent";
			order.ReceivingAgent.MainAddress.OA_Address1 = transportMode + " ReceivingAgent";

			deliveryContainer.J5_RC_NKContainerType = ZString.Empty;

			return order;
		}

		/// <remarks>
		/// This method sets up a matching for the given code to a random organization.<br/>
		/// Consider using <see cref="SetOrgMatch(OrgHeader,ZString)"/> overload with organization parameter instead.
		/// </remarks>
		protected OrgPatternMatchOverride SetOrgMatch(string code)
		{
			OrgMatch = Factory.New<OrgPatternMatchOverride>();
			OrgMatch.OO_OH = GlbCompany.GetCurrentCompany(Factory).OrgProxy.PK;
			OrgMatch.OO_ForeignCode = code;
			OrgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			var localOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.OrgProxy.PK));
			OrgMatch.OO_LocalGuid = localOrg.PK;

			return OrgMatch;
		}

		protected OrgPatternMatchOverride SetOrgMatch(OrgHeader localOrg, ZString foreignCode)
		{
			OrgMatch = Factory.New<OrgPatternMatchOverride>();
			OrgMatch.OO_OH = GlbCompany.GetCurrentCompany(Factory).OrgProxy.PK;
			OrgMatch.OO_ForeignCode = foreignCode;
			OrgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			OrgMatch.OO_LocalGuid = localOrg.PK;
			return OrgMatch;
		}

		UNDGSubstance Substance
		{
			get
			{
				if (substance == null)
				{
					substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_UNNO, "2000"));
					AssertNotNull(substance);
				}
				return substance;
			}
		}
		UNDGSubstance substance;

		class TestOrderValueObjectDataAdapter : OrderValueObjectDataAdapter<Order, Xsd.Order>
		{
			public TestOrderValueObjectDataAdapter()
			{
			}

			public TestOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
				: base(triggeredByEvents)
			{
			}

			public new Order FindBusinessObject(Xsd.Order value, IValueObjectImportContext context)
			{
				return base.FindBusinessObject(value, context);
			}

			public new Order[] FindBusinessObjects(Xsd.Order xsdOrder, IValueObjectImportContext context)
			{
				return base.FindBusinessObjects(xsdOrder, context);
			}

			public new Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom ExportOrderLineDeliveryCustomDetails(OrderLineDelivery delivery)
			{
				return base.ExportOrderLineDeliveryCustomDetails(delivery);
			}
		}

		protected OrgHeader Buyer
		{
			get
			{
				if (fBuyer == null)
				{
					fBuyer = Factory.NewWithValidTestData<OrgHeader>();
					fBuyer.OH_FullName = "Buyer";
					fBuyer.OH_RL_NKClosestPort = "AUSYD";
					fBuyer.OH_Code = "buyer";
					fBuyer.MainAddress.OA_Address1 = "Buyer";
					fBuyer.MainAddress.OA_Code = "ABCD";
					fBuyer.MainAddress.OA_City = "City";
					fBuyer.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					fBuyer.OH_IsConsignee = true;
				}

				return fBuyer;
			}
		}
		OrgHeader fBuyer;

		protected OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = Factory.NewWithValidTestData<OrgHeader>();
					fSupplier.OH_FullName = "Supplier";
					fSupplier.OH_RL_NKClosestPort = "NZAKL";
					fSupplier.OH_Code = "supplier";
					fSupplier.MainAddress.OA_Address1 = "Supplier";
					fSupplier.MainAddress.OA_City = "City";
					fSupplier.OH_IsConsignor = true;
				}

				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		protected Order CreateOrder(ZString orderNo, ZByte split, ZString orderStatus)
		{
			return CreateOrder(orderNo, split, orderStatus, "", 0, 0, 0, 0);
		}

		protected Order CreateOrder(ZString orderNo, ZByte split, ZString orderStatus, ZString product, ZShort lineNo, ZShort subLineNo, ZShort lineNo2, ZShort subLineNo2)
		{
			Order result = Factory.New<Order>();

			result.BuyerPK = Buyer.PK;
			result.SupplierPK = Supplier.PK;
			result.JD_OrderNumber = orderNo;
			result.JD_OrderNumberSplit = split;
			result.JD_OrderStatus = orderStatus;

			if (lineNo > 0)
			{
				result.OrderLines.Add(CreateOrderLine(product, lineNo, subLineNo));
			}

			if (lineNo2 > 0)
			{
				result.OrderLines.Add(CreateOrderLine(product, lineNo2, subLineNo2));
			}

			return result;
		}

		protected OrderLine CreateOrderLine(ZString product, ZShort lineNo, ZShort subLineNo)
		{
			OrderLine result = Factory.New<OrderLine>();

			result.JO_LineNo = lineNo;
			result.JO_SubLineNo = subLineNo;
			result.JO_Partno = product;

			return result;
		}

		protected Xsd.Order CreateXsdOrder(ZString orderNo, ZByte split, ZString product, ZShort lineNo, ZShort subLineNo, ZShort lineNo2, ZShort subLineNo2)
		{
			Xsd.Order result = new Xsd.Order();

			result.Events.IsSpecified = false;
			result.OrderIdentifier.IsSpecified = true;
			result.OrderIdentifier.OrderNumber = orderNo;
			result.OrderIdentifier.OrderNumberSplit = split;
			result.OrderIdentifier.OrderNumberSplitSpecified = false;

			result.OrderDetail.IsSpecified = true;
			result.OrderDetail.Buyer.IsSpecified = true;
			result.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			result.OrderDetail.Supplier.IsSpecified = true;
			result.OrderDetail.Supplier.EDICode = Supplier.OH_Code;

			if (lineNo > 0)
			{
				result.OrderLines.Add(CreateXsdOrderLine(product, lineNo, subLineNo));
			}

			if (lineNo2 > 0)
			{
				result.OrderLines.Add(CreateXsdOrderLine(product, lineNo2, subLineNo2));
			}

			result.OrderLines.IsSpecified = result.OrderLines.Count > 0;

			return result;
		}

		protected Xsd.OrderOrderLine CreateXsdOrderLine(ZString product, ZShort lineNo, ZShort subLineNo)
		{
			Xsd.OrderOrderLine result = new Xsd.OrderOrderLine();

			result.OrderLineNo = lineNo;
			if (subLineNo > 0)
			{
				result.OrderSubLineNo = subLineNo;
			}

			result.OrderLineDetail.IsSpecified = true;
			result.OrderLineDetail.Product = product;

			return result;
		}

		#endregion
	}
}
