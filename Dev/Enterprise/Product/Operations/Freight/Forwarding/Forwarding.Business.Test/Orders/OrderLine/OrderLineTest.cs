using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLine))]
	sealed class OrderLineTest : BusinessObjectWithCustomLabelsTestCase
	{
		public void TestLogEventForMismatchedShipmentWindow()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Approved,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Cancelled,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Incomplete,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Placed,2);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Planned,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Rejected,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Converted,0);
				AssertLogEventForMismatchedShipmentWindow(SupplierBookingStatus.Shipped,0);
			});
		}

		void AssertLogEventForMismatchedShipmentWindow(string status, int count)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.LogEventForMismatchedShipWindow(false);
			orderLine.JO_Quantity = 5;
			AddNewBookingLineForTest(orderLine, status, 1);

			var bookingLine = orderLine.SupplierBookingLines[0];
			bookingLine.JSL_ShipmentWindowStart = new ZDate(2022, 7, 7);
			bookingLine.JSL_ShipmentWindowEnd = new ZDate(2022, 7, 7);
			Factory.Save();

			order.JD_ShipmentWindowStart = new ZDate(2022, 7, 8);
			order.JD_ShipmentWindowEnd = new ZDate(2022, 7, 8);
			orderLine.LogEventForMismatchedShipWindow(false);
			AssertEquals(0, orderLine.Logs.GetAllLogs().Cast<AutoStmALog>().Count(log => log.SL_SE_NKEvent == Events.ExceptionRaisedCode));

			orderLine.LogEventForMismatchedShipWindow(true);
			orderLine.LogEventForMismatchedShipWindow(true);
			AssertEquals(count, orderLine.Logs.GetAllLogs().Cast<AutoStmALog>().Count(log => log.SL_SE_NKEvent == Events.ExceptionRaisedCode));

			Factory.Save();
			orderLine.JO_ShipmentWindowStart = new ZDate(2022, 7, 8);
			orderLine.JO_ShipmentWindowEnd = new ZDate(2022, 7, 8);
			orderLine.LogEventForMismatchedShipWindow(false);
			orderLine.LogEventForMismatchedShipWindow(false);
			AssertEquals(count * 2, orderLine.Logs.GetAllLogs().Cast<AutoStmALog>().Count(log => log.SL_SE_NKEvent == Events.ExceptionRaisedCode));
		}

		public void TestSetOrderLineToDeliveredIfWithinTolerance()
		{
			var order = Factory.New<Order>();

			var line = order.OrderLines.AddNew();
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 0, 0, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 10, 0, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 15, 0, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 20, 0, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 100, 0, OrderStatus.Delivered);

			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 0, 25, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 10, 25, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 15, 25, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 20, 25, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 100, 25, OrderStatus.Delivered);

			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 0, 50, OrderStatus.Confirmed);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 10, 50, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 15, 50, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 20, 50, OrderStatus.Delivered);
			TestSetOrderLinesToDeliveredIfWithinTolerance(line, 20, 100, 50, OrderStatus.Delivered);
		}

		void TestSetOrderLinesToDeliveredIfWithinTolerance(OrderLine line, ZDecimal quantity, ZDecimal packedQuantity, ZDecimal underQuantityPercentageLimit, ZString expectedOrderStatus)
		{
			line.JO_Quantity = quantity;
			line.JO_QtyPacked = packedQuantity;
			line.JO_UnderQuantityPercentageLimit = underQuantityPercentageLimit;
			line.JO_LineStatus = OrderStatus.Confirmed;
			line.SetOrderLineToDeliveredIfWithinTolerance();
			AssertEquals(expectedOrderStatus, line.JO_LineStatus);
		}

		public void TestOrderAndOrderLineNumber()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "ORDER1";
			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LineNo = 1;

			AssertEquals("OrderAndOrderLineNumber", "ORDER1 - 1", line1.OrderAndOrderLineNumber);

			order.JD_OrderNumber = "ORDER";
			line1.JO_LineNo = 10;
			AssertEquals("OrderAndOrderLineNumber", "ORDER - 10", line1.OrderAndOrderLineNumber);
		}

		public void TestLinePrice_IsOnlySetOnce_WhenSetManually()
		{
			Order order = Factory.New<Order>();
			OrderLine line = order.OrderLines.AddNew();
			line.JO_Quantity = 960;

			line.JO_LinePrice = 2501.04;
			AssertEquals("Line Price should not have been recalculated", 2501.04m, line.JO_LinePrice);
		}

		public void TestQtyReceivedToDate()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "ORDER1";
			order.BuyerPK = org.PK;
			order.JD_OrderNumberSplit = 1;

			Order orderSplit = Factory.New<Order>();
			orderSplit.JD_OrderNumber = "ORDER1";
			orderSplit.BuyerPK = org.PK;
			orderSplit.JD_OrderNumberSplit = 2;

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "ORDER2";
			order2.BuyerPK = org.PK;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LineNo = 1;
			line1.JO_QtyReceived = 30m;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineNo = 1;
			line2.JO_QtyReceived = 80m;

			OrderLine line3 = order.OrderLines.AddNew();
			line3.JO_LineNo = 2;
			line3.JO_QtyReceived = 97m;

			OrderLine line4 = orderSplit.OrderLines.AddNew();
			line4.JO_LineNo = 1;
			line4.JO_QtyReceived = 30m;

			OrderLine line5 = order2.OrderLines.AddNew();
			line5.JO_LineNo = 1;
			line5.JO_QtyReceived = 20m;

			AssertEquals(140m, line1.QtyReceivedToDate);
			AssertEquals(140m, line2.QtyReceivedToDate);
			AssertEquals(97m, line3.QtyReceivedToDate);
			AssertEquals(140m, line4.QtyReceivedToDate);
			AssertEquals(20m, line5.QtyReceivedToDate);

			line2.JO_F3_NKPackType = "XYZ";
			AssertEquals("Different units - cannot convert", 0m, line1.QtyReceivedToDate);
			AssertEquals("Different units - cannot convert", 0m, line2.QtyReceivedToDate);
			AssertEquals(97m, line3.QtyReceivedToDate);
			AssertEquals("Different units - cannot convert", 0m, line4.QtyReceivedToDate);
			AssertEquals(20m, line5.QtyReceivedToDate);
		}

		public void TestReceiveAllIfNonReceived()
		{
			BO.JO_Quantity = 100m;
			BO.JO_QtyReceived = 10m;
			BO.JO_QtyInvoiced = 9m;

			BO.ReceiveAllIfNonReceived();
			AssertEquals(100m, BO.JO_QtyReceived);
			AssertEquals(100m, BO.JO_QtyInvoiced);
		}

		#region TestSetDefaultTolerances

		public void TestSetDefaultTolerances()
		{
			OrderLine orderLine;

			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetBuyerAddress();

				BO.SetDefaultTolerances();
				CombineAssertions("Tolerances should be 0 when no SupplierBuyerLink is found.", () =>
				{
					AssertNotNull(nameof(BO.Order), BO.Order);
					AssertNull(nameof(BO.Order.SupplierBuyerLinkFromBuyerAddressCountry), BO.Order.SupplierBuyerLinkFromBuyerAddressCountry);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, BO.JO_LateShipmentLimitDays);
				});

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = BO.Order.BuyerPK;
				link.OL_OH_Supplier = BO.Order.SupplierPK;
				link.OL_RN_NKImporterCountry = BO.Order.BuyerAddress.Country.Code;

				Factory.Save();

				BO.SetDefaultTolerances();
				CombineAssertions("Tolerances should be 0 when a SupplierBuyerLinkTolerance does not exist for TransportMode + PartNumber.", () =>
				{
					AssertNotNull(nameof(BO.Order.SupplierBuyerLinkFromBuyerAddressCountry), BO.Order.SupplierBuyerLinkFromBuyerAddressCountry);
					AssertEquals("Tolerances Count", 0, BO.Order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, BO.JO_LateShipmentLimitDays);
				});

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				BO.SetDefaultTolerances();
				CombineAssertions("Tolerances should be set using the matching SupplierBuyerLinkTolerance.", () =>
				{
					AssertEquals("Tolerances Count", 1, BO.Order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, BO.JO_LateShipmentLimitDays);
				});

				orderLine = Factory.New<OrderLine>();
				orderLine.SetDefaultTolerances();
				CombineAssertions("Tolerances should be 0 when no Order is found.", () =>
				{
					AssertNull(nameof(orderLine.Order), orderLine.Order);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
				});
			}

			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				orderLine.JO_JD = BO.JO_JD;
				orderLine.SetDefaultTolerances();
				CombineAssertions("Tolerances should remain the same when 'Enable Order Line Shipping Tolerance entry' is unchecked.", () =>
				{
					AssertEquals("Tolerances Count", 1, orderLine.Order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, orderLine.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, orderLine.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, orderLine.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, orderLine.JO_LateShipmentLimitDays);
				});
			}
		}

		public void TestSetDefaultTolerances_WhenPartNumberChanges()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetBuyerAddress();

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = BO.Order.BuyerPK;
				link.OL_OH_Supplier = BO.Order.SupplierPK;
				link.OL_RN_NKImporterCountry = BO.Order.BuyerAddress.Country.Code;

				var part = Factory.NewWithValidTestData<OrgSupplierPart>();
				part.OP_PartNum = "PartNum";
				part.RelatedOrganisations.AddOrganisationIfNotExist(link.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
				part.RelatedOrganisations.AddOrganisationIfNotExist(link.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
				part.OP_IsActive = true;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = part.OP_PartNum;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				AssertEquals(1, BO.Order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
				CombineAssertions("Tolerances before PartNumber changes.", () =>
				{
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, BO.JO_LateShipmentLimitDays);
				});

				BO.JO_Partno = part.OP_PartNum;
				CombineAssertions("Tolerances after PartNumber changes.", () =>
				{
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, BO.JO_LateShipmentLimitDays);
				});
			}
		}

		public void TestSetDefaultTolerances_WhenOrderChanges()
		{
			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetBuyerAddress();

				var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = BO.Order.BuyerPK;
				link.OL_OH_Supplier = BO.Order.SupplierPK;
				link.OL_RN_NKImporterCountry = BO.Order.BuyerAddress.Country.Code;

				var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
				tolerance.OLT_OL_SupplierBuyerLink = link.PK;
				tolerance.OLT_TransportMode = Constants.TransportModes.All;
				tolerance.OLT_PartNumber = ZString.Empty;
				tolerance.OLT_UnderQuantityPercentageLimit = 1m;
				tolerance.OLT_OverQuantityPercentageLimit = 2m;
				tolerance.OLT_EarlyShipmentLimitDays = 3;
				tolerance.OLT_LateShipmentLimitDays = 4;

				Factory.Save();

				var orderPK = BO.JO_JD;
				BO.JO_JD = ZGuid.Empty;
				CombineAssertions("Tolerances before Order changes.", () =>
				{
					AssertNull(nameof(BO.Order), BO.Order);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), 0m, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), 0m, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), (byte)0, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), (byte)0, BO.JO_LateShipmentLimitDays);
				});

				BO.JO_JD = orderPK;
				CombineAssertions("Tolerances after Order changes.", () =>
				{
					AssertEquals("Tolerances Count", 1, BO.Order.SupplierBuyerLinkFromBuyerAddressCountry.Tolerances.Count);
					AssertEquals(nameof(BO.JO_UnderQuantityPercentageLimit), tolerance.OLT_UnderQuantityPercentageLimit, BO.JO_UnderQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_OverQuantityPercentageLimit), tolerance.OLT_OverQuantityPercentageLimit, BO.JO_OverQuantityPercentageLimit);
					AssertEquals(nameof(BO.JO_EarlyShipmentLimitDays), tolerance.OLT_EarlyShipmentLimitDays, BO.JO_EarlyShipmentLimitDays);
					AssertEquals(nameof(BO.JO_LateShipmentLimitDays), tolerance.OLT_LateShipmentLimitDays, BO.JO_LateShipmentLimitDays);
				});
			}
		}

		void SetBuyerAddress()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();

			var buyerAddress = BO.Order.Buyer.Addresses.AddNew();
			buyerAddress.FillWithValidTestData();
			buyerAddress.OA_RN_NKCountryCode = country.RN_Code;

			BO.Order.JD_OA_BuyerAddress = buyerAddress.PK;
		}

		#endregion

		public void TestIsLineStatusCancelled()
		{
			BO.JO_LineStatus = Constants.OrderStatus.Cancelled;
			AssertEquals(true, BO.IsLineStatusCancelled);

			BO.JO_LineStatus = "";
			AssertEquals(false, BO.IsLineStatusCancelled);
		}

		public void TestCustomLabelsList()
		{
			var provider = OrderLine.NewCustomLabelsProvider(BO.Order);
			var list = provider.GetCustomFields(null, Factory);
			AssertEquals("Should have 22 items", 22, list.Count);

			provider = OrderLine.NewCustomLabelsProvider(BO.Order);
			list = provider.GetCustomFields(GlbBranch.CurrentBranch.OrgProxy, Factory);
			AssertEquals("Should have 25 items", 25, list.Count);
		}

		public void TestPopulateDefaultsFromJO_Partno()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "xxx";
			product.OP_Desc = "desc";
			product.OP_OrderMultipleQty = 1;
			product.OP_VendorPackQty = 2;
			product.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.SupplierPK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			BO.JO_Partno = "garbage";
			Assert("Description not populated yet", BO.JO_Description.IsEmpty);

			BO.JO_Partno = "xxx";
			AssertEquals("JO_Description should be that of OP_Desc", "desc", BO.JO_Description);
			AssertEquals("JO_InnerPacks should be that of OP_OrderMultipleQty", new ZDecimal(1), BO.JO_InnerPacks);
			AssertEquals("JO_OuterPacks should be that of OP_VendorPackQty", new ZDecimal(2), BO.JO_OuterPacks);

			BO.JO_Partno = "splat";
			BO.JO_Description = "user_entered_desc";
			BO.JO_Partno = "xxx";
			AssertEquals("Description should still be the same as it was already entered", "user_entered_desc", BO.JO_Description);
		}

		public void TestPopulateDefaultsFromJO_Partno_UNDG()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "xxx";
			product.OP_Desc = "desc";
			product.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.SupplierPK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			BO.JO_Partno = "garbage";
			AssertEquals("DG not populated yet", 0, BO.UNDGs.Count);

			BO.JO_Partno = "xxx";
			AssertEquals("DG not populated yet, even though xxx is valid", 0, BO.UNDGs.Count);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var dG = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			dG.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = product.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			Factory.Save();

			BO.JO_Partno = "xxx";
			AssertEquals("Added", 1, BO.UNDGs.Count);
			AssertEquals("DG should be copied from the product", dG.DG_Code, BO.UNDGs[0].Substance.DG_Code);
		}

		public void TestPopulateDefaultsFromJO_Partno_IsCopying()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "xxx";
			product.OP_Desc = "desc";
			product.OP_OrderMultipleQty = 1;
			product.OP_VendorPackQty = 2;
			product.OP_StockKeepingUnit = "b";
			product.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.SupplierPK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			BO.JO_Partno = "xxx";
			AssertEquals("JO_Description should be that of OP_Desc", "desc", BO.JO_Description);
			AssertEquals("JO_InnerPacks should be that of OP_OrderMultipleQty", 1m, BO.JO_InnerPacks);
			AssertEquals("JO_OuterPacks should be that of OP_VendorPackQty", 2m, BO.JO_OuterPacks);

			BO.JO_Description = "other";
			BO.JO_InnerPacks = 3m;
			BO.JO_OuterPacks = 3m;
			BO.JO_F3_NKPackType = "a";

			OrderLine newOrderLine = (OrderLine)BO.Clone();
			AssertEquals("other", newOrderLine.JO_Description);
			AssertEquals("a", newOrderLine.JO_F3_NKPackType);
		}

		public void TestPopulateDefaultsFromJO_OpenQuantity_IsCopying()
		{
			BO.JO_OpenQuantity = 200m;

			var newOrderLine = (OrderLine)BO.Clone();
			AssertEquals("Copy operation should not copy JO_OpenQuantity", ZDecimal.Zero, newOrderLine.JO_OpenQuantity);
		}

		public void TestPopulateDimensions_IsCopying()
		{
			BO.JO_ActualVolume = 40;

			((IBusinessObjectInternals)BO).IsCopying = true;
			BO.JO_OuterPacks = 10;
			BO.JO_OuterPackHeight = 50000m;
			BO.JO_OuterPackLength = 60000m;
			BO.JO_OuterPackWidth = 90000m;
			BO.JO_OuterPackUnitOfDimension = Constants.Length.Metres;
			BO.JO_UnitOfVolume = Constants.Volume.CubicMetres;

			AssertEquals(40m, BO.JO_ActualVolume);
		}

		public void TestPopulateItemAndLinePrices()
		{
			BO.JO_Quantity = 0;
			BO.JO_ItemPrice = 2;
			BO.JO_LinePrice = 5;
			AssertEquals("Shouldn't populate if quantity is 0", new ZDecimal(5), BO.JO_LinePrice);

			BO.JO_Quantity = 2;
			AssertEquals("Quantity populates line price", new ZDecimal(4), BO.JO_LinePrice);

			BO.JO_LinePrice = 12;
			AssertEquals("Line price populates item price", new ZDecimal(6), BO.JO_ItemPrice);

			BO.JO_ItemPrice = 5;
			AssertEquals("Item price populates line price", new ZDecimal(10), BO.JO_LinePrice);
		}

		public void TestInvoiceAmountQtyAndReceived()
		{
			BO.JO_Quantity = 0;
			BO.JO_QtyInvoiced = 0;
			BO.JO_QtyReceived = 0;
			AssertEquals("Quantity Remaining should be 0", (ZDecimal)0, BO.JO_QuantityRemaining);

			BO.JO_Quantity = 10;
			BO.JO_QtyInvoiced = 10;
			AssertEquals("Qty Invoiced should update Qty Received", (ZDecimal)10, BO.JO_QtyReceived);
			AssertEquals("Qty Remaining should be equal to Ordered - Received", BO.JO_Quantity - BO.JO_QtyReceived, BO.JO_QuantityRemaining);

			BO.JO_QtyReceived = 7;
			AssertEquals("Qty Received shouldn't change Qty Invoiced", false, BO.JO_QtyReceived == BO.JO_QtyInvoiced);
			AssertEquals("Qty Remaining should be equal to Ordered - Received", BO.JO_Quantity - BO.JO_QtyReceived, BO.JO_QuantityRemaining);

			OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Qty Remaining should be equal to Ordered - Invoiced", BO.JO_Quantity - BO.JO_QtyInvoiced, BO.JO_QuantityRemaining);

			BO.JO_QtyInvoiced = 11;
			AssertEquals("Qty Invoiced should not update Qty Received", (ZDecimal)7, BO.JO_QtyReceived);

			BO.JO_QtyReceived = 1;
			AssertEquals("Qty Received should not update Qty Invoiced", (ZDecimal)11, BO.JO_QtyInvoiced);
		}

		public void TestJO_ContainersVisible()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>().PK;
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>().PK;

			BO.Order.BuyerPK = buyer1;
			BO.JO_ContainersVisible = false;
			BO.Order.BuyerPK = buyer2;
			BO.JO_ContainersVisible = true;

			BO.Order.BuyerPK = buyer1;
			AssertEquals("false", false, BO.JO_ContainersVisible);
			BO.Order.BuyerPK = buyer2;
			AssertEquals("true", true, BO.JO_ContainersVisible);
		}

		public void TestUnitOfQtyDefaultsFromProduct()
		{
			Order order1 = Factory.New<Order>();
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = buyer.PK;
			order1.JD_OrderNumber = "1";
			OrderLine line1 = order1.OrderLines.AddNew();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			ZString partNo = "PartA";
			ZString stockKeepingUnit = "BAG";
			part.OP_PartNum = partNo;
			part.OP_StockKeepingUnit = stockKeepingUnit;
			part.RelatedOrganisations.RemoveAndDeleteAll();
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = buyer.PK;
			Factory.Save();
			line1.JO_Partno = partNo;
			AssertEquals("Qty type should default from Product", stockKeepingUnit, order1.OrderLines[0].JO_F3_NKPackType);
		}

		public void TestDefaultSubLineNo()
		{
			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "Test1";
			order1.BuyerPK = org1.PK;
			order1.SupplierPK = org1.PK;

			OrderLine line1 = order1.OrderLines.AddNew();
			AssertEquals("Orderline Line No. should default to 1", 1, line1.JO_LineNo);
			AssertEquals("OrderLine SubLine No. should default to (ZShort)1", 1, line1.JO_SubLineNo);

			OrderLine line2 = order1.OrderLines.AddNew();
			AssertEquals("OrderLine 2 Line No. should default to 2", 2, line2.JO_LineNo);
			AssertEquals("OrderLine 2 SubLine No. should default to 1", 1, line2.JO_SubLineNo);

			line2.JO_LineNo = 1;
			AssertEquals("OrderLine 2 SubLine No. should default to greatest Subline No. with the same line No.", 2, line2.JO_SubLineNo);

			line1.Delete();
			Factory.Save();

			OrderLine line3 = order1.OrderLines.AddNew();
			line3.JO_LineNo = 1;
			AssertEquals("OrderLine 3 SubLine No. should default to greatest Subline No. with the same line No.", 3, line3.JO_SubLineNo);

			Order order2 = Factory.New<Order>();
			OrderLine line4 = order2.OrderLines.AddNew();
			AssertEquals("Line should have subline no of 1", 1, line4.JO_SubLineNo);
			line4.JO_LineNo = 1;
			AssertEquals("Line should have subline no of 1", 1, line4.JO_SubLineNo);
		}

		public void TestJO_Calc_OrderLineNoAndSubLineNo()
		{
			Order order = Factory.New<Order>();
			OrderLine line = order.OrderLines.AddNew();

			line.JO_LineNo = 1;
			line.JO_SubLineNo = 1;

			AssertEquals("Calculated field should be just be line no", "1", line.JO_Calc_OrderLineNoAndSubLineNo);

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineNo = 1;

			AssertEquals("Calculated field should contain subline no", "1 - 2", line2.JO_Calc_OrderLineNoAndSubLineNo);
		}

		public void TestJO_QtyBooked()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 5;
			orderLine.JO_OpenQuantity = 3;

			AssertEquals(2m, orderLine.JO_QtyBooked);
		}

		public void TestQuantityUpdatesOpenQuantity()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.SupplierBooking.JSB_Status = "PLC";
			bookingLine.JSL_BookedQuantity = 20;

			AssertEquals("Precondition: JO_OpenQuantity", -20m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.OrderLine.JO_Quantity = 100;
			AssertEquals("JO_OpenQuantity should have updated when increasing the total order quantity", 80m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.OrderLine.JO_Quantity = 20;
			AssertEquals("JO_OpenQuantity should have updated when reducing the total order quantity to 20", 0m, bookingLine.OrderLine.JO_OpenQuantity);
		}

		public void TestNoBusinessObjectsLoadedOnSaveIfNoChangesMade()
		{
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.Deliveries.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrderLine loadedOrderLine = newFactory.Load<OrderLine>(orderLine.PK);
			AssertBusinessObjectTypesNotCreatedOrLoaded(newFactory, typeof(OrderLineDelivery));
		}

		public void TestJO_LineNoAndSplitAndSubLine()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			AssertEquals("1", orderLine.JO_LineNoAndSplitAndSubLine);

			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;
			AssertEquals("1 sub 2", orderLine.JO_LineNoAndSplitAndSubLine);

			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;
			orderLine.JO_LineSplitNumber = 3;
			AssertEquals("1.3 sub 2", orderLine.JO_LineNoAndSplitAndSubLine);
		}

		public void TestJO_LineStatus_ReadOnly()
		{
			OrdersDataRegistry.Instance.OrderLineStatusEditable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrderLine orderLine = Factory.NewWithValidTestData<OrderLine>();
			AssertEquals("Line Status not read-only by default", false, orderLine.JO_LineStatusInfo.ReadOnly);

			OrdersDataRegistry.Instance.OrderLineStatusEditable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			orderLine = Factory.NewWithValidTestData<OrderLine>();
			AssertEquals("Line Status read-only when registry item set", true, orderLine.JO_LineStatusInfo.ReadOnly);
		}

		public void TestPackProductsDetachingOnDeletion()
		{
			Order order1 = Factory.New<Order>();
			Order order2 = Factory.New<Order>();

			OrderLine line11 = order1.OrderLines.AddNew();
			line11.JO_Partno = "Product11";
			line11.JO_Quantity = 11m;
			line11.JO_F3_NKPackType = "BOX";

			OrderLine line12 = order1.OrderLines.AddNew();
			line12.JO_Partno = "Product12";
			line12.JO_Quantity = 12m;
			line12.JO_F3_NKPackType = "PLT";

			OrderLine line21 = order2.OrderLines.AddNew();
			line21.JO_Partno = "Product21";
			line21.JO_Quantity = 21m;
			line21.JO_F3_NKPackType = "UNT";

			ForwardingPackLine packline1 = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct product11 = packline1.Products.AddNew();
			PackProduct product12 = packline1.Products.AddNew();
			product11.D2_JO = line11.PK;
			product12.D2_JO = line12.PK;

			ForwardingPackLine packline2 = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct product21 = packline1.Products.AddNew();
			product21.D2_JO = line11.PK;

			line11.Delete();
			AssertEquals(ZGuid.Empty, product11.D2_JO);
			AssertEquals(line12.PK, product12.D2_JO);
			AssertEquals(ZGuid.Empty, product21.D2_JO);
		}

		[TestDate(2008, 3, 10)]
		public void TestConfirmationNumUpdatesConfirmationDate()
		{
			OrderLine line = Factory.New<OrderLine>();
			line.JO_ConfirmationDate = new ZDateTime(2011, 1, 5);
			line.JO_ConfirmationNum = "XYZ";

			AssertEquals("Confirmation Date is not updated if already present", line.JO_ConfirmationDate, new ZDateTime(2011, 1, 5));

			OrderLine line2 = Factory.New<OrderLine>();
			AssertEquals("Pre-condition", line2.JO_ConfirmationDate, ZDateTime.Empty);

			line2.JO_ConfirmationNum = "ABC";
			AssertEquals("Event date set to the current date", new ZDateTime(2008, 3, 10), line2.JO_ConfirmationDate);
		}

		public void TestOrderLineConfirmationDatesUpdateOrderHeaderConfirmationDate()
		{
			ZDateTime testDate = ZDateTime.Now.AddDays(-1);
			ZDateTime testDate2 = ZDateTime.Now.AddDays(1);

			Order order = Factory.NewWithValidTestData<Order>();
			OrderLine line1 = order.OrderLines.AddNew();

			order.JD_BookingConfDate = testDate;
			line1.JO_ConfirmationDate = ZDateTime.Now;
			Factory.Save();

			AssertEquals("Order Confirmation Date is not updated if it is already present", testDate, order.JD_BookingConfDate);

			order = Factory.NewWithValidTestData<Order>();
			line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();

			line1.JO_ConfirmationDate = ZDateTime.Now;
			Factory.Save();

			AssertEquals("Order Confirmation Date is not updated as not all lines have a confirmation date", ZDateTime.Empty, order.JD_BookingConfDate);

			order = Factory.NewWithValidTestData<Order>();
			line1 = order.OrderLines.AddNew();
			line2 = order.OrderLines.AddNew();

			line1.JO_ConfirmationDate = ZDateTime.Now;
			line2.JO_ConfirmationDate = testDate2;

			Factory.Save();

			AssertEquals("Order Confirmation Date is updated to the latest Line Confirmation Date", testDate2, order.JD_BookingConfDate);
		}

		public void TestSetDescriptionOfOrderLineToMaxLengthAndReadback()
		{
			var order = Factory.NewWithValidTestData<OrderLine>();
			var maxLength = OrderLine.Schema.JO_DescriptionMaxLength;

			AssertEquals("Max length of description should be 128", 128, maxLength);
			AssertNoExceptionThrown(() => order.JO_Description = new string('a', maxLength));

			var description = order.JO_Description;

			AssertEquals("Description can be set to max length and read back as max number of chars", maxLength, description.Length);
		}

		#region Test Manufacturer

		public void TestManufacturerAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var orderLine = Factory.New<OrderLine>();
			var manufacturerQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, orderLine.PK);
			manufacturerQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, "MAN");

			AssertEquals("No Manufacturer Address were found", false, Factory.Exists(typeof(JobDocAddress), manufacturerQuery));

			orderLine.ManufacturerNameOrPK = org.PK.ToString();
			AssertEquals("Manufacturer Address was found", 1, Factory.Load<JobDocAddress>(manufacturerQuery).Length);
			AssertEquals("ManufacturerCode", org.OH_Code, orderLine.ManufacturerAddress.Organisation.OH_Code);
			AssertEquals("Manufacturer AdressCode", address.OA_Address1, orderLine.ManufacturerAddress.E2_Address1);
		}

		#endregion

		#region TestGoodsAvailableAtAddress

		public void TestGoodsAvailableAtAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var orderLine = Factory.New<OrderLine>();
			var goodsAvailableAtQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, orderLine.PK);
			goodsAvailableAtQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.GoodsAvailableAt);

			AssertEquals("No GoodsAvailableAt Address were found", false, Factory.Exists(typeof(JobDocAddress), goodsAvailableAtQuery));

			orderLine.GoodsAvailableAtNameOrPK = org.PK.ToString();
			AssertEquals("GoodsAvailableAt Address was found", 1, Factory.Load<JobDocAddress>(goodsAvailableAtQuery).Length);
			AssertEquals("GoodsAvailableAtCode", org.OH_Code, orderLine.GoodsAvailableAtAddress.Organisation.OH_Code);
			AssertEquals("GoodsAvailableAt AdressCode", address.OA_Address1, orderLine.GoodsAvailableAtAddress.E2_Address1);
		}

		#endregion

		#region TestGoodsDeliveredToAddress

		public void TestGoodsDeliveredToAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var orderLine = Factory.New<OrderLine>();
			var goodsDeliveredToQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, orderLine.PK);
			goodsDeliveredToQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.GoodsDeliveredTo);
			AssertEquals("No GoodsDeliveredTo Address were found", false, Factory.Exists(typeof(JobDocAddress), goodsDeliveredToQuery));

			orderLine.GoodsDeliveredToNameOrPK = org.PK.ToString();
			AssertEquals("GoodsDeliveredTo Address was found", 1, Factory.Load<JobDocAddress>(goodsDeliveredToQuery).Length);
			AssertEquals("GoodsDeliveredTo Code", org.OH_Code, orderLine.GoodsDeliveredToAddress.Organisation.OH_Code);
			AssertEquals("GoodsDeliveredTo AdressCode", address.OA_Address1, orderLine.GoodsDeliveredToAddress.E2_Address1);
		}

		#endregion

		#region IDocAddresses

		public void TestDocAddresses()
		{
			var orderLine = Factory.New<OrderLine>();
			AssertEquals(typeof(JobDocAddressDependentCollection), orderLine.DocAddresses.GetType());

			var docAddresses = orderLine as IDocAddresses;
			Assert("Manufactuer should be a supported type", docAddresses.SupportedAddressTypes.Contains(DocAddressType.Manufacturer));
			Assert("GoodsAvailableAt should be a supported type", docAddresses.SupportedAddressTypes.Contains(DocAddressType.GoodsAvailableAt));
			Assert("GoodsDeliveredTo should be a supported type", docAddresses.SupportedAddressTypes.Contains(DocAddressType.GoodsDeliveredTo));
		}

		public void TestSupportedAddressTypes()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				IDocAddresses orderLine = Factory.New<OrderLine>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.Manufacturer,
					DocAddressType.GoodsAvailableAt,
					DocAddressType.GoodsDeliveredTo,
					DocAddressType.ConsigneeDocumentaryAddress,
				}, orderLine.SupportedAddressTypes);
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				IDocAddresses orderLine = Factory.New<OrderLine>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.Manufacturer,
					DocAddressType.GoodsAvailableAt,
					DocAddressType.GoodsDeliveredTo,
				}, orderLine.SupportedAddressTypes);
			});
		}

		#endregion

		#region Test New()

		public void TestNew()
		{
			OrderLine line = OrderLine.New(Factory);

			AssertNotNull("OrderLine.New(Factory) returned null.", line);
			AssertEquals("OrderLine.New(Factory) did not return a typeof(OrderLine).", typeof(OrderLine), line.GetType());
		}

		#endregion

		#region Related Business Objects

		public void TestDeliveries()
		{
			AssertEquals("Deliveries", 0, BO.Deliveries.Count);
			AssertEquals("Deliveries", typeof(OrderLineDelivery), BO.Deliveries.AddNew().GetType());
		}

		public void TestProduct()
		{
			AssertNull("No part number, so no part initially", BO.Product);

			// put duplicate part numbers, but different owner/supplier relations to ensure the conflict is resolved in the supplier's favour
			OrgSupplierPart ownerPart = Factory.New<OrgSupplierPart>();
			ownerPart.OP_PartNum = "THEPRT";
			ownerPart.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.BuyerPK, OrgPartRelation.RelationshipTypes.Owner);
			OrgSupplierPart supplierPart = Factory.New<OrgSupplierPart>();
			supplierPart.OP_PartNum = "THEPRT";
			supplierPart.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.SupplierPK, OrgPartRelation.RelationshipTypes.Supplier);

			// we have to save here because we're using a ZDBOnlyFilter
			Factory.Save();

			AssertNull("No part number, so no part initially", BO.Product);
			BO.JO_Partno = "THEPRT";
			AssertEquals("Correct part number", "THEPRT", BO.Product.OP_PartNum);
			AssertEquals("Should use the owner part", ownerPart, BO.Product);

			BO.JO_Partno = "splaty";
			AssertNull("No valid product number, so no product", BO.Product);
		}

		public void TestSupplierPartWithTheSameBuyerButDifferentSupplierWhenCheckingExact()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var testPart = Factory.New<OrgSupplierPart>();
				testPart.OP_PartNum = "TESTPRT";
				testPart.RelatedOrganisations.AddOrganisationIfNotExist(BO.Order.BuyerPK, OrgPartRelation.RelationshipTypes.Owner);

				var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
				testPart.RelatedOrganisations.AddOrganisationIfNotExist(supplierOrg.PK, OrgPartRelation.RelationshipTypes.Supplier);

				Factory.Save();

				var parts = BO.JO_Partno_List;
				parts.Load();

				Assert(!parts.Contains(testPart));
			}
		}

		#endregion

		#region List Properties

		[ExpectNoExceptions]
		public void TestJO_Partno_List()
		{
			OrgSupplierPartCollection parts = BO.JO_Partno_List;
			parts.Load();
		}

		public void TestJO_LineStatus_List()
		{
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			CodeDescriptionPairList testOrderCodeDescPairList = new CodeDescriptionPairList();
			testOrderCodeDescPairList.AddPair("RGL", "Reg Status Description");
			Env.Registry.OrderLineStatusList = testOrderCodeDescPairList;
			order.BuyerPK = buyer.PK;
			buyer.MiscServ.OrderLineStatusList = TestCodeDescPairList.ToXMLByteArray();
			testOrderCodeDescPairList.Clear();
			testOrderCodeDescPairList.AddPair("PLX", "Proxy Org Status Description");
			GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderLineStatusList = testOrderCodeDescPairList.ToXMLByteArray();
			AssertEquals("Line Status List should include proxy organisation codes", true, orderLine.JO_LineStatus_List.ContainsCode("PLX"));
			AssertEquals("Line Status List should include the default list", true, orderLine.JO_LineStatus_List.ContainsCode(Constants.OrderStatus.Open));
			AssertEquals("Line Status List should include the list from the buyer", true, orderLine.JO_LineStatus_List.ContainsCode("XXX"));
			AssertEquals("Line Status List should include registry codes", true, orderLine.JO_LineStatus_List.ContainsCode("RGL"));
		}

		public void TestContainerNumbersList()
		{
			var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());

			Order testOrder = Factory.New<Order>();
			testOrder.BuyerPK = buyer.PK;
			testOrder.JD_OrderNumber = "Rand101";

			OrderContainer container1 = testOrder.PlannedContainers.AddNew();
			OrderContainer container2 = testOrder.PlannedContainers.AddNew();
			OrderContainer container3 = testOrder.PlannedContainers.AddNew();
			container1.J1_ContainerNumber = "123";
			container1.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container2.J1_ContainerNumber = "321";
			container3.J1_ContainerNumber = "DUPLICATE";

			OrderLine line = testOrder.OrderLines.AddNew();

			AssertEquals(3, line.ContainerNumbersList.Count);
			AssertEquals("123", line.ContainerNumbersList[0].Code);
			AssertEquals("20GP", line.ContainerNumbersList[0].Description);
			AssertEquals("321", line.ContainerNumbersList[1].Code);
			AssertEquals("", line.ContainerNumbersList[1].Description);
			AssertEquals("DUPLICATE", line.ContainerNumbersList[2].Code);

			testOrder.PlannedContainers.AddNew().J1_ContainerNumber = "456";
			testOrder.PlannedContainers.AddNew().J1_ContainerNumber = "789";

			Factory.Save();

			AssertEquals(5, line.ContainerNumbersList.Count);

			testOrder.JD_JS = Factory.New<ForwardingShipment>().PK;
			ForwardingConsol consol = testOrder.Shipment.Consols.AddNew();
			ForwardingContainer consolContainer1 = consol.Containers.AddNew();
			ForwardingContainer consolContainer2 = consol.Containers.AddNew();
			ForwardingContainer consolContainer3 = consol.Containers.AddNew();
			ForwardingContainer consolContainer4 = consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "111";
			consolContainer2.JC_ContainerNum = "222";
			consolContainer3.JC_ContainerNum = "333";
			consolContainer4.JC_ContainerNum = "DUPLICATE";
			Factory.Save();

			AssertEquals(8, line.ContainerNumbersList.Count);

			CodeDescriptionPair[] expectedList = {
					new CodeDescriptionPair("111", ""),
					new CodeDescriptionPair("222", ""),
					new CodeDescriptionPair("333", ""),
					new CodeDescriptionPair("DUPLICATE", ""),
					new CodeDescriptionPair("123", "20GP"),
					new CodeDescriptionPair("321", ""),
					new CodeDescriptionPair("456", ""),
					new CodeDescriptionPair("789", "") };

			AssertContainsExactElementsInAnyOrder(expectedList, line.ContainerNumbersList);
		}

		CodeDescriptionPairList TestCodeDescPairList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("XXX", "Description");
				return result;
			}
		}

		#endregion

		#region ICustomFieldProvider

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = BO as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);

			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		public void TestOrgnizationCustomFields_ControllingCustomer()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var customLabel1 = controllingCustomer.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomText1;
			customLabel1.OT_Caption = "Custom Text 1";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var customLabel2 = buyer.CustomLabels.AddNew();
			customLabel2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomFlag2;
			customLabel2.OT_Caption = "Custom Flag 2";

			var order = Factory.New<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;

			order.BuyerPK = buyer.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;

			var provider = orderLine1 as ICustomFieldProvider;
			AssertNotNull(provider);

			var customBusinessObject = provider.GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertNotNull(customBusinessObject);

			var propertyNames = customBusinessObject.PropertyNames;
			Assert("Do not contain buyer's custom field", !propertyNames.Contains("__JO`=CUSTOMFLAG2__prop__ZBool"));
			Assert("Only contain controlling customer's custom field", propertyNames.Contains("__JO`=CUSTOMTEXTBLOB1__prop__ZString"));
		}

		public void TestOrgnizationCustomFields_Buyer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var customLabel1 = buyer.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomFlag2;
			customLabel1.OT_Caption = "Custom Flag 2";
			var customLabel2 = buyer.CustomLabels.AddNew();
			customLabel2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDate5;
			customLabel2.OT_Caption = "Custom Date 5";

			var order = Factory.New<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;

			order.BuyerPK = buyer.PK;

			var provider = orderLine1 as ICustomFieldProvider;
			AssertNotNull(provider);

			var customBusinessObject = provider.GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertNotNull(customBusinessObject);

			var propertyNames = customBusinessObject.PropertyNames;
			Assert("Contain buyer's custom field: Custom Flag 2", propertyNames.Contains("__JO`=CUSTOMFLAG2__prop__ZBool"));
			Assert("Contain buyer's custom field: Custom Date 5", propertyNames.Contains("__JO`=CUSTOMDATE5__prop__ZDateTime"));
		}

		public void TestPartAttributesShouldNotBeShown()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var customLabel1 = buyer.CustomLabels.AddNew();
			customLabel1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomFlag2;
			customLabel1.OT_Caption = "Custom Flag 2";
			buyer.MiscServ.OM_IMPartAttrib1Name = "Color";
			buyer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			buyer.MiscServ.OM_IMPartAttrib2Name = "Size";
			buyer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			buyer.MiscServ.OM_IMPartAttrib3Name = "Width";
			buyer.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;

			var order = Factory.New<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;

			order.BuyerPK = buyer.PK;

			var provider = orderLine1 as ICustomFieldProvider;
			AssertNotNull(provider);

			var customBusinessObject = provider.GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertNotNull(customBusinessObject);

			var propertyNames = customBusinessObject.PropertyNames;
			Assert("Buyer's custom field should include Custom Flag 2", propertyNames.Contains("__JO`=CUSTOMFLAG2__prop__ZBool"));
			Assert("Buyer's custom field should not include Part Attribute1", !propertyNames.Contains("__JO`=PARTATTRIB1__prop__ZString"));
			Assert("Buyer's custom field should not include Part Attribute2", !propertyNames.Contains("__JO`=PARTATTRIB2__prop__ZString"));
			Assert("Buyer's custom field should not include Part Attribute3", !propertyNames.Contains("__JO`=PARTATTRIB3__prop__ZString"));
		}

		#endregion

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			var workflowProvider = BO as IWorkflowProvider;
			AssertNotNull(workflowProvider);
			AssertEquals(WorkflowDescriptors.OrderLineWorkflowDescriptorCode, workflowProvider.WorkflowType);

			var workflowItems = workflowProvider.WorkflowItems;
			AssertNotNull(workflowItems);
			AssertEquals(0, workflowItems.Count);

			BO.Order.JD_RL_NKPortOfLoading = "AUSYD";
			BO.Order.JD_RL_NKPortOfDischarge = "NZAKL";
			var workflowInformationProvider = workflowProvider.GetWorkflowInformationProvider();
			AssertNotNull(workflowInformationProvider);
			AssertEquals("Sydney", workflowInformationProvider.Origin);
			AssertEquals("Auckland", workflowInformationProvider.Destination);
			AssertEquals(TrackingConstants.BusinessContext.Order, workflowInformationProvider.BusinessContext);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;

			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_JD = order.PK;
			orderLine.JO_LineNo = 99;

			var ranker = (orderLine as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			var subType1List = ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1);
			AssertEquals(2, subType1List.Length);
			AssertEquals(Core.Constants.TransportModes.Sea, subType1List[0]);
			AssertEquals(ZString.Empty, subType1List[1]);

			var valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(4, valueList.Length);
			AssertEquals(controllingCustomer.PK, valueList[0]);
			AssertEquals(buyer.PK, valueList[1]);
			AssertEquals(supplier.PK, valueList[2]);
			AssertEquals(ZGuid.Empty, valueList[3]);

			order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "NZAKL";
			ranker = (orderLine as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(4, valueList.Length);
			AssertEquals(controllingCustomer.PK, valueList[0]);
			AssertEquals(supplier.PK, valueList[1]);
			AssertEquals(buyer.PK, valueList[2]);
			AssertEquals(ZGuid.Empty, valueList[3]);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertNotNull(clientInTemplateSelectionCriteria);
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode(WorkflowSelectionOrgTypeCodes.ControllingCustomer));
			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);
			Factory.Save();

			ranker = (orderLine as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(3, valueList.Length);
			AssertEquals(supplier.PK, valueList[0]);
			AssertEquals(buyer.PK, valueList[1]);
			AssertEquals(ZGuid.Empty, valueList[2]);
		}

		#endregion

		#region IJobNumber

		public void TestJobNumber()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "OrderNumber";
			order.JD_OrderNumberSplit = 2;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;

			AssertEquals("OrderNumber-2", ((IJobNumber)orderLine).JobNumber);
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider

		public void TestICustomLabelsConfigOrgProvider_ConfigOrg()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.BuyerPK = buyer.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;

			var org = (orderLine as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Controlling customer will be matched.", controllingCustomer, org);

			order.ControllingCustomerDocAddress.E2_AddressOverride = true;
			order.ControllingCustomerDocAddress.E2_CompanyName = "XYZ IMPORT CO";
			order.ControllingCustomerDocAddress.E2_Address1 = "33 Pitt Street";

			org = (orderLine as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Buyer will be matched if controlling customer becomes overridden.", buyer, org);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertNotNull(clientInTemplateSelectionCriteria);
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode(WorkflowSelectionOrgTypeCodes.ControllingCustomer));
			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);
			Factory.Save();

			org = (order as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Buyer will be matched after controlling customer is removed.", buyer, org);
		}

		public void TestICustomLabelsConfigOrgProvider_ConfigOrgChanged()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerConfigOrgChangedCalled = false;

			var order1 = Factory.New<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;

			((ICustomLabelsConfigOrgProvider)orderLine1).ConfigOrgChanged += (s, e) => buyerConfigOrgChangedCalled = true;
			order1.BuyerPK = buyer.PK;
			AssertEquals("Buyer ConfigOrgChanged called", true, buyerConfigOrgChangedCalled);
			AssertEquals(buyer, ((ICustomLabelsConfigOrgProvider)orderLine1).ConfigOrg);

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerConfigOrgChangedCalled = false;

			var order2 = Factory.New<Order>();
			var orderLine2 = order2.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			((ICustomLabelsConfigOrgProvider)orderLine2).ConfigOrgChanged += (s, e) => controllingCustomerConfigOrgChangedCalled = true;
			order2.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;
			AssertEquals("Controlling Customer ConfigOrgChanged called", true, controllingCustomerConfigOrgChangedCalled);
			AssertEquals(controllingCustomer, ((ICustomLabelsConfigOrgProvider)orderLine2).ConfigOrg);
		}

		#endregion

		#region GetValidAddressWithFallbackToOrder

		public void TestGetValidAddressWithFallbackToOrder()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();

			AssertNull(orderLine.GetValidAddressWithFallbackToOrder(DocAddressType.GoodsAvailableAt));

			var notifyPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var notifyPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			order.GoodsAvailableAtAddress.E2_OA_Address = notifyPartyOrg1.MainAddress.PK;
			AssertEquals(notifyPartyOrg1.MainAddress.PK, orderLine.GetValidAddressWithFallbackToOrder(DocAddressType.GoodsAvailableAt).E2_OA_Address);

			orderLine.GoodsAvailableAtAddress.E2_OA_Address = notifyPartyOrg2.MainAddress.PK;
			AssertEquals(notifyPartyOrg2.MainAddress.PK, orderLine.GetValidAddressWithFallbackToOrder(DocAddressType.GoodsAvailableAt).E2_OA_Address);

			orderLine.GoodsAvailableAtAddress.E2_AddressOverride = true;
			AssertEquals(notifyPartyOrg1.MainAddress.PK, orderLine.GetValidAddressWithFallbackToOrder(DocAddressType.GoodsAvailableAt).E2_OA_Address);

			order.GoodsAvailableAtAddress.E2_AddressOverride = true;
			AssertNull(orderLine.GetValidAddressWithFallbackToOrder(DocAddressType.GoodsAvailableAt));
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_JD = order.PK;
			Factory.Save();

			AssertEquals("X125", orderLine.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			AssertEquals(ExternalRequestTypes.Codes.OrderLine, orderLine.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;
			order.JD_OA_SupplierAddress = reviewerOrg.MainAddress.PK;
			order.JD_OC_SupplierContact = reviewerOrg.Contacts[0].PK;
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(order, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_JD = order.PK;
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(orderLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, orderLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion

		#region Implementation

		OrderLine BO;

		protected override void SetUp()
		{
			base.SetUp();
			BO = (OrderLine)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = Factory.New<Order>();

			order.JD_OrderNumber = "x";
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;

			OrderLine line = Factory.New<OrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 99;
			return line;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();

			var order = factory.New<Order>();

			order.JD_OrderNumber = "x";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = buyer.PK;

			var line = factory.New<OrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 99;

			return line;
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			OrderLine line = (OrderLine)bO;
			return OrderLine.NewCustomLabelsProvider(line.Order);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new OrderLineLightValidationTester(bizObjToTest);
		}

		void AddNewBookingLineForTest(OrderLine orderLine, string supplierBookingStatus, int bookedQuantity)
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_Status = supplierBookingStatus;

			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_JSB_Booking = supplierBooking.PK;
			bookingLine.JSL_BookedQuantity = bookedQuantity;
		}

		class OrderLineLightValidationTester : LightValidationTester
		{
			public OrderLineLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != "E2_AddressType" && propertyName != "E2_AddressOverride" && propertyName != "E2_AddressSequence" && propertyName != "E2_OA_Address";
			}
		}

		#endregion
	}
}
