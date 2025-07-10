using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using Events = Enterprise.ZArchitecture.Business.Events;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using SupplierBookingStatus = Enterprise.Core.Constants.SupplierBookingStatus;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region Shipment Window Mismatch

		public void TestShipmentWindowMismatchWhileBookingWithoutShipmentWindowDate_EnableAdvOrmFeature()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				AdvOrmFeatureHelper.RunTestWith(true, action: () =>
				{
					var shouldHasStartEvent = bookingStatus == SupplierBookingStatus.Placed;
					TestShipmentWindowMismatch("modify empty to valid date", ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, shouldHasStartEvent, shouldHasStartEvent);
					TestShipmentWindowMismatch("not modify date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch("modify valid date to different date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, shouldHasStartEvent, shouldHasStartEvent);
					TestShipmentWindowMismatch("modify valid date to empty date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false);
				});
			}
		}

		public void TestShipmentWindowMismatchWhileBookingWithoutShipmentWindowDate_DisableSupplierBooking()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				AdvOrmFeatureHelper.RunTestWith(false, action: () =>
				{
					TestShipmentWindowMismatch("modify empty to valid date", ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch("not modify date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch("modify valid date to different date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch("modify valid date to empty date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false);
				});
			}
		}

		public void TestShipmentWindowMismatchWhileBookingWithShipmentWindowDate_EnableAdvOrmFeature()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				AdvOrmFeatureHelper.RunTestWith(true, action: () =>
				{
					var shouldHasStartEvent = bookingStatus == SupplierBookingStatus.Placed;
					TestShipmentWindowMismatch("modify empty to valid date",ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, shouldHasStartEvent, shouldHasStartEvent);
					TestShipmentWindowMismatch("not modify date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false, true);
					TestShipmentWindowMismatch("modify valid date to different date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, shouldHasStartEvent, shouldHasStartEvent, true);
					TestShipmentWindowMismatch("modify valid date to empty date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false, true);
				});
			}
		}

		public void TestShipmentWindowMismatchWhileBookingWithShipmentWindowDate_DisableSupplierBooking()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				AdvOrmFeatureHelper.RunTestWith(false, action: () =>
				{
					TestShipmentWindowMismatch("modify empty to valid date", ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch("not modify date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false, true);
					TestShipmentWindowMismatch("modify valid date to different date",new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, false, false, true);
					TestShipmentWindowMismatch("modify valid date to empty date", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false, true);
				});
			}
		}

		void TestShipmentWindowMismatch(string message, ZDate originalShipmentWindowStart, ZDate originalShipmentWindowEnd, ZDate shipmentWindowStart, ZDate shipmentWindowEnd, string bookingStatus, bool hasStartEvent, bool hasEndEvent, bool bookingWithShipmentWindowDate = false)
		{
			Data.ConsigneeOrgCRAHOLSYD.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = Data.ConsigneeOrgCRAHOLSYD.PK;
			order.JD_ShipmentWindowStart = originalShipmentWindowStart;
			order.JD_ShipmentWindowEnd = originalShipmentWindowEnd;

			var orderline1 = order.OrderLines.AddNew();
			var orderline2 = order.OrderLines.AddNew();
			orderline1.FillWithValidTestData();
			orderline2.FillWithValidTestData();
			orderline2.JO_ShipmentWindowStart = new ZDate(2023, 10, 2);
			orderline2.JO_ShipmentWindowEnd = new ZDate(2023, 10, 3);
			Factory.SaveForTesting();

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_Status = bookingStatus;
			var bookingLine1 = booking.SupplierBookingLines.AddNew();
			var bookingLine2 = booking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderline1.PK;
			bookingLine2.JSL_JO_OrderLine = orderline2.PK;

			if (bookingWithShipmentWindowDate)
			{
				bookingLine1.JSL_ShipmentWindowStart = originalShipmentWindowStart;
				bookingLine1.JSL_ShipmentWindowEnd = originalShipmentWindowEnd;
				bookingLine2.JSL_ShipmentWindowStart = originalShipmentWindowStart;
				bookingLine2.JSL_ShipmentWindowEnd = originalShipmentWindowEnd;
			}
			Factory.SaveForTesting();

			var logs1 = orderline1.Logs.GetAllLogs();
			AssertEquals(0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

			var logs2 = orderline2.Logs.GetAllLogs();
			AssertEquals(0, logs2.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(0, logs2.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

			var logsOnBooking = booking.Logs.GetAllLogs();
			AssertEquals(0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

			var orderDataObject = GetNewOrderDataObject();
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.BuyerDocumentaryAddress);
			orderDataObject.Order = new UniversalOrder { OrderNumber = order.JD_OrderNumber };
			orderDataObject.SetDateCollection(() => new List<Date>());
			orderDataObject.DateCollection.Add(new Date { Type = DateType.ShipmentWindowStart, Value = shipmentWindowStart });
			orderDataObject.DateCollection.Add(new Date { Type = DateType.ShipmentWindowEnd, Value = shipmentWindowEnd });
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);

			AssertNoExceptionThrown(() => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());

			Factory.SaveForTesting();

			CombineAssertions($"{message} while booking status is {bookingStatus}", () =>
			{
				logs1 = orderline1.Logs.GetAllLogs();
				AssertEquals(hasStartEvent ? 1 : 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
				AssertEquals(hasEndEvent ? 1 : 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

				logs2 = orderline2.Logs.GetAllLogs();
				AssertEquals(0, logs2.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
				AssertEquals(0, logs2.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

				logsOnBooking = booking.Logs.GetAllLogs();
				AssertEquals(hasStartEvent ? 1 : 0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
				AssertEquals(hasStartEvent ? 1 : 0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());
			});
		}

		#endregion
		#region TestBasicFieldLevelMappings

		public void TestBasicFieldLevelMappings()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.AdditionalTerms = "ADDMYONE";
			orderDataObject.BookingConfirmationReference = "Booking Reference";
			orderDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
				{
					new CommercialInvoiceHeader
					{
						InvoiceNumber = "4321",
						InvoiceDate = new ZDateTime(2012, 1, 4)
					}
				}
			};
			orderDataObject.ContainerMode = new ContainerMode { Code = "LSE" };
			orderDataObject.CountryOfSupply = new Country { Code = "US" };
			orderDataObject.FirstBuyerContact = "Barney";
			orderDataObject.FreightRate = 10.3m;
			orderDataObject.FreightRateCurrency = new Currency { Code = "NZD" };
			orderDataObject.GoodsDescription = "Guitars";
			orderDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2012, 1, 6) };
			orderDataObject.Order = new UniversalOrder
			{
				OrderNumber = "ORDER1",
				OrderNumberSplit = new ZByte(2),
				Status = new CodeDescriptionPair { Code = "CNF" },
				IsReleased = true,
			};
			orderDataObject.OuterPacks = 2;
			orderDataObject.OuterPacksPackageType = new PackageType { Code = "PLT" };
			orderDataObject.PortOfDestination = new UNLOCO { Code = "NZAKL" };
			orderDataObject.PortOfDischarge = new UNLOCO { Code = "NZCHC" };
			orderDataObject.PortOfLoading = new UNLOCO { Code = "AUBNE" };
			orderDataObject.PortOfOrigin = new UNLOCO { Code = "AUMEL" };
			orderDataObject.SecondBuyerContact = "Fred";
			orderDataObject.ServiceLevel = new ServiceLevel { Code = "LIV" };
			orderDataObject.ShipmentIncoTerm = new IncoTerm { Code = "FCA" };
			orderDataObject.TotalVolume = 53.2m;
			orderDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			orderDataObject.TotalWeight = 32.4m;
			orderDataObject.TotalWeightUnit = new UnitOfWeight { Code = "KT" };
			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			orderDataObject.WayBillNumber = "ORDERHOUSE";
			orderDataObject.SetDateCollection(() => new List<Date>());
			orderDataObject.DateCollection.Add(DateType.ExWorksRequiredBy, false, new ZDateTime(2012, 1, 1));
			orderDataObject.DateCollection.Add(DateType.OrderDate, false, new ZDateTime(2012, 1, 2));
			orderDataObject.DateCollection.Add(DateType.BookingConfirmed, false, new ZDateTime(2012, 1, 3));
			orderDataObject.DateCollection.Add(DateType.FollowUp, false, new ZDateTime(2012, 1, 5));
			orderDataObject.DateCollection.Add(DateType.DepartureVesselCutoffDate, false, new ZDateTime(2012, 1, 7));
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowStart, false, new ZDate(2012, 1, 7));
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowEnd, false, new ZDate(2012, 1, 7));
			orderDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(1), VesselName = "BELINGA", VoyageFlightNo = "BL123", EstimatedDeparture = new ZDateTime(2012, 12, 31), EstimatedArrival = new ZDateTime(2013, 1, 1) });
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(2), VesselName = "BUNDAGO", VoyageFlightNo = "BN321", EstimatedDeparture = new ZDateTime(2013, 1, 2), EstimatedArrival = new ZDateTime(2013, 1, 3) });
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(3), VesselName = "WUNDAGO", VoyageFlightNo = "WN321", EstimatedDeparture = new ZDateTime(2013, 1, 4), EstimatedArrival = new ZDateTime(2013, 1, 5) });

			orderDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>()
			{
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "MASTERWAYBILL",
					BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList()),
				}
			});

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			CombineAssertions(delegate
			{
				AssertEquals("orderBO.JD_ActualVolume", 53.2m, orderBO.JD_ActualVolume);
				AssertEquals("orderBO.JD_ActualWeight", 32.4m, orderBO.JD_ActualWeight);
				AssertEquals("orderBO.JD_AdditionalTerms", "ADDMYONE", orderBO.JD_AdditionalTerms);
				AssertEquals("orderBO.JD_ArrivalVoyage", "WN321", orderBO.JD_ArrivalVoyage);
				AssertEquals("orderBO.JD_BookingConfDate", new ZDateTime(2012, 1, 3), orderBO.JD_BookingConfDate);
				AssertEquals("orderBO.JD_BookingConfRef", "Booking Reference", orderBO.JD_BookingConfRef);
				AssertEquals("orderBO.JD_ContainerMode", "LSE", orderBO.JD_ContainerMode);
				AssertEquals("orderBO.JD_DeliveryRequiredBy", new ZDateTime(2012, 1, 6), orderBO.JD_DeliveryRequiredBy);
				AssertEquals("orderBO.JD_DepartureVesselCutoffDate", new ZDateTime(2012, 1, 7), orderBO.JD_DepartureVesselCutoffDate);
				AssertEquals("orderBO.JD_DepartureVoyage", "BL123", orderBO.JD_DepartureVoyage);
				AssertEquals("orderBO.JD_Milestone_E_DEP", new ZDateTime(2012, 12, 31), orderBO.JD_Milestone_E_DEP);
				AssertEquals("orderBO.JD_E_ARV_1stIntermediate", new ZDateTime(2013, 1, 1), orderBO.JD_E_ARV_1stIntermediate);
				AssertEquals("orderBO.JD_E_ARV_2ndIntermediate", new ZDateTime(2013, 1, 3), orderBO.JD_E_ARV_2ndIntermediate);
				AssertEquals("orderBO.JD_E_DEP_2", new ZDateTime(2013, 1, 2), orderBO.JD_E_DEP_2);
				AssertEquals("orderBO.JD_E_DEP_3", new ZDateTime(2013, 1, 4), orderBO.JD_E_DEP_3);
				AssertEquals("orderBO.JD_Milestone_E_ARV", new ZDateTime(2013, 1, 5), orderBO.JD_Milestone_E_ARV);
				AssertEquals("orderBO.JD_ExWorksRequiredBy", new ZDateTime(2012, 1, 1), orderBO.JD_ExWorksRequiredBy);
				AssertEquals("orderBO.JD_F3_NKPackType", "PLT", orderBO.JD_F3_NKPackType);
				AssertEquals("orderBO.JD_FirstBuyerContact", "Barney", orderBO.JD_FirstBuyerContact);
				AssertEquals("orderBO.JD_FollowUpDate", new ZDateTime(2012, 1, 5), orderBO.JD_FollowUpDate);
				AssertEquals("orderBO.JD_ShipmentWindowStart", new ZDate(2012, 1, 7), orderBO.JD_ShipmentWindowStart);
				AssertEquals("orderBO.JD_ShipmentWindowEnd", new ZDate(2012, 1, 7), orderBO.JD_ShipmentWindowEnd);
				AssertEquals("orderBO.JD_IncoTerm", "FCA", orderBO.JD_IncoTerm);
				AssertEquals("orderBO.JD_IntermediateVoyage", "BN321", orderBO.JD_IntermediateVoyage);
				AssertEquals("orderBO.JD_InvoiceDate", new ZDateTime(2012, 1, 4), orderBO.JD_InvoiceDate);
				AssertEquals("orderBO.JD_InvoiceNumber", "4321", orderBO.JD_InvoiceNumber);
				AssertEquals("orderBO.JD_OrderDate", new ZDateTime(2012, 1, 2), orderBO.JD_OrderDate);
				AssertEquals("orderBO.JD_OrderGoodsDescription", "Guitars", orderBO.JD_OrderGoodsDescription);
				AssertEquals("orderBO.JD_OrderNumber", "ORDER1", orderBO.JD_OrderNumber);
				AssertEquals("orderBO.JD_OrderNumberSplit", new ZByte(2), orderBO.JD_OrderNumberSplit);
				AssertEquals("orderBO.JD_OrderStatus", "CNF", orderBO.JD_OrderStatus);
				AssertEquals("orderBO.JD_IsReleased", true, orderBO.JD_IsReleased);
				AssertEquals("orderBO.JD_Packs", 2, orderBO.JD_Packs);
				AssertEquals("orderBO.JD_RL_NKGoodsAvailableAt", "AUMEL", orderBO.JD_RL_NKGoodsAvailableAt);
				AssertEquals("orderBO.JD_RL_NKGoodsDeliveredTo", "NZAKL", orderBO.JD_RL_NKGoodsDeliveredTo);
				AssertEquals("orderBO.JD_RL_NKPortOfDischarge", "NZCHC", orderBO.JD_RL_NKPortOfDischarge);
				AssertEquals("orderBO.JD_RL_NKPortOfLoading", "AUBNE", orderBO.JD_RL_NKPortOfLoading);
				AssertEquals("orderBO.JD_RN_NKCountryOfSupply", "US", orderBO.JD_RN_NKCountryOfSupply);
				AssertEquals("orderBO.JD_RS_NKServiceLevel_NI", "LIV", orderBO.JD_RS_NKServiceLevel_NI);
				AssertEquals("orderBO.JD_RV_NKArrivalVessel", "WUNDAGO", orderBO.JD_RV_NKArrivalVessel);
				AssertEquals("orderBO.JD_RV_NKDepartureVessel", "BELINGA", orderBO.JD_RV_NKDepartureVessel);
				AssertEquals("orderBO.JD_RV_NKIntermediateVessel", "BUNDAGO", orderBO.JD_RV_NKIntermediateVessel);
				AssertEquals("orderBO.JD_RX_NKOrderCurrency", "NZD", orderBO.JD_RX_NKOrderCurrency);
				AssertEquals("orderBO.JD_SecondBuyerContact", "Fred", orderBO.JD_SecondBuyerContact);
				AssertEquals("orderBO.JD_TransportMode", "AIR", orderBO.JD_TransportMode);
				AssertEquals("orderBO.JD_UnitOfVolume", "CF", orderBO.JD_UnitOfVolume);
				AssertEquals("orderBO.JD_UnitOfWeight", "KT", orderBO.JD_UnitOfWeight);
				AssertEquals("orderBO.JD_Waybill", "ORDERHOUSE", orderBO.JD_Waybill);
				AssertEquals("orderBO.JD_MasterWaybill", "MASTERWAYBILL", orderBO.JD_MasterWaybill);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		public void TestContainerModeNotOverridenByTransportMode()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			orderDataObject.ContainerMode = new ContainerMode { Code = "OTH" };

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertEquals("Expected transport mode to be air", "AIR", orderBO.JD_TransportMode);
			AssertEquals("Expected container mode to be other not the default loose", "OTH", orderBO.JD_ContainerMode);

			orderDataObject = GetNewOrderDataObject();
			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			orderDataObject.ContainerMode = new ContainerMode { Code = "LCL" };
			reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			orderBO = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertEquals("Expected transport mode to be sea", "SEA", orderBO.JD_TransportMode);
			AssertEquals("Expected container mode to be lcl not the default fcl", "LCL", orderBO.JD_ContainerMode);
		}

		public void TestIncotermNotOverridenByBuyerSupplierLink()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var supplierDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress);

			var buyer = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var supplier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var buyerSupplierLink = buyer.SupplierLinks.AddNew();
			buyerSupplierLink.OL_OH_Supplier = supplier.PK;
			buyerSupplierLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "FOB";
			buyerSupplierLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = Constants.ContainerModes.Loose;
			buyerSupplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.Air;

			Factory.SaveForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			orderDataObject.ContainerMode = new ContainerMode { Code = "LSE" };

			orderDataObject.PortOfDestination = new UNLOCO { Code = "NZAKL" };
			orderDataObject.PortOfDischarge = new UNLOCO { Code = "NZCHC" };
			orderDataObject.PortOfLoading = new UNLOCO { Code = "AUBNE" };
			orderDataObject.PortOfOrigin = new UNLOCO { Code = "AUMEL" };

			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(supplierDataObject);

			orderDataObject.ShipmentIncoTerm = new IncoTerm { Code = "FCA" };

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertEquals("JD_Incoterm should be populated from ShipmentIncoTerm of xml", "FCA", orderBO.JD_IncoTerm);
		}

		public void TestMasterWayBill()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>()
			{
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "HOUSEWAYBILL",
					BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList()),
				},
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "Null WAYBILL"
				},
				new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "MASTERWAYBILL",
					BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList()),
				},
			});

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			Order orderBO = null;
			AssertNoExceptionThrown(() => orderBO = reader.ReadIntoBusinessObject());

			AssertNotNull(orderBO);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertEquals("orderBO.JD_MasterWaybill", "MASTERWAYBILL", orderBO.JD_MasterWaybill);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestCustomFields

		public void TestCustomFields()
		{
			var orderDataObject = GetNewOrderDataObject();
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.AddCustomLabel(Constants.CustomLabels.Order.CustomAttribute1, "What Makes You Happy?");
			buyer.AddCustomLabel(Constants.CustomLabels.Order.CustomDate1, "The Date You Are Happy");
			buyer.AddCustomLabel(Constants.CustomLabels.Order.CustomDecimal1, "The Happy Decimal");
			buyer.AddCustomLabel(Constants.CustomLabels.Order.CustomFlag1, "Are you Happy?");
			buyer.AddCustomLabel(Constants.CustomLabels.Order.UserTrackDate1, "Track Date");
			orderDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			orderDataObject.CustomizedFieldCollection.Add("What Makes You Happy?", new ZString("Lots Of Ice"));
			orderDataObject.CustomizedFieldCollection.Add("The Date You Are Happy", ZDateTime.BrettsBirthday);
			orderDataObject.CustomizedFieldCollection.Add("The Happy Decimal", new ZDecimal(7.7m));
			orderDataObject.CustomizedFieldCollection.Add("Are you Happy?", new ZBool(true));
			orderDataObject.CustomizedFieldCollection.Add("Estimated Track Date", new ZDateTime(2013, 1, 1));
			orderDataObject.CustomizedFieldCollection.Add("Actual Track Date", new ZDateTime(2013, 1, 2));

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);
			CombineAssertions(delegate
			{
				AssertEquals("orderBO.JD_CustomAttrib1", "Lots Of Ice", orderBO.JD_CustomAttrib1);
				AssertEquals("orderBO.JD_CustomDate1", ZDateTime.BrettsBirthday, orderBO.JD_CustomDate1);
				AssertEquals("orderBO.JD_CustomDecimal1", 7.7m, orderBO.JD_CustomDecimal1);
				AssertEquals("orderBO.JD_CustomFlag1", true, orderBO.JD_CustomFlag1);
				AssertEquals("orderBO.JD_EstimateUserDate1", new ZDateTime(2013, 1, 1), orderBO.JD_EstimateUserDate1);
				AssertEquals("orderBO.JD_ActualUserDate1", new ZDateTime(2013, 1, 2), orderBO.JD_ActualUserDate1);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		#endregion

		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			#region Setup Template

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "ORD";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			orderDataObject.CustomizedFieldCollection.Add("Textual context", new ZString("HELLO"));
			orderDataObject.CustomizedFieldCollection.Add("Customs are customary", new ZString("GOODBYE"));
			orderDataObject.CustomizedFieldCollection.Add("First Date", new ZDateTime(2011, 1, 1));
			orderDataObject.CustomizedFieldCollection.Add("Last Date", new ZDateTime(2011, 1, 2));
			orderDataObject.CustomizedFieldCollection.Add("Deci Deca", new ZDecimal(0.3));
			orderDataObject.CustomizedFieldCollection.Add("+ 1 point zero", new ZDecimal(1.3));
			orderDataObject.CustomizedFieldCollection.Add("Flagger", ZBool.True);
			orderDataObject.CustomizedFieldCollection.Add("Integer Mate", new ZInt(42));
			orderDataObject.CustomizedFieldCollection.Add("Integraler", ZInt.Zero);
			orderDataObject.CustomizedFieldCollection.Add("Bogus Custom Field", new ZString("I am BOGUS"));

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = order.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
				Assert("Custom Field 5 not found", customFieldsString.Contains("Textual context - HELLO"));

				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		#endregion

		#region ThrowsException

		#region TestThrowsExceptionIfNoBuyerAddressPresent

		public void TestThrowsExceptionIfNoBuyerAddressPresent()
		{
			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader1 = new OrderDataObjectReader(orderDataObject, Logger, new UniversalObjectFactory());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Order\r\nNo Buyer Address was provided.", () => reader1.ReadIntoBusinessObject());

			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var buyerDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Address1 = "123 Place",
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress)
			};
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);

			var reader2 = new OrderDataObjectReader(orderDataObject, Logger, new UniversalObjectFactory());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), @"Cannot Import Order
Unable to match Buyer Address, please make sure the supplied Buyer Address is valid. Details were:
Address1: 123 Place", () => reader2.ReadIntoBusinessObject());
		}

		#endregion

		#region TestThrowsExceptionIfMatchesOrderThatIsAttachedToShipment

		public void TestThrowsExceptionIfMatchesOrderThatIsAttachedToShipment()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000666";
			matchingOrder.JD_JS = shipment.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot update Order because: Order 'ORDERME-2' is not allowed for Organization 'CRAHOLSYD' and order is already attached to Shipment 'S00000666'.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestAllowUpdatingOrdersAttachedToBookings

		public void TestAllowUpdatingOrdersAttachedToBookings()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var booking = Factory.New<ForwardingShipment>();
			booking.JS_UniqueConsignRef = "S00000666";
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			matchingOrder.JD_JS = booking.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			orderDataObject.GoodsDescription = "MY_GOODS";
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown("Should not throw an exception", () =>
			{
				var order = reader.ReadIntoBusinessObject();
				AssertEquals("MY_GOODS", order.JD_OrderGoodsDescription);
			});
		}

		#endregion

		#region TestAllowXMLUpdateOfOrdersAttachedToShipments

		public void TestThrowExceptionWhenXMLUpdateIsAllowedButCutoffDateIsBlank()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000666";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;
			matchingOrder.JD_JS = shipment.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot update Order because: Order 'ORDERME-2' cut-off date is not set and order is already attached to Shipment 'S00000666'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestThrowExceptionWhenXMLUpdateIsAllowedAndCutOffDateIsSetButItsTooLate()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000666";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc = ZDateTime.UtcNow.AddDays(-2);
			matchingOrder.JD_JS = shipment.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot update Order because: Order 'ORDERME-2' cut-off date has passed and order is already attached to Shipment 'S00000666'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestAllowUpdatingOrdersAttachedToShipment_XMLUpdateIsAllowed_CutOffDateIsSetAndItIsNotLate()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000666";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc = ZDateTime.UtcNow.AddDays(2);
			matchingOrder.JD_JS = shipment.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			orderDataObject.GoodsDescription = "MY_GOODS";
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown("Should not throw an exception", () =>
			{
				var order = reader.ReadIntoBusinessObject();
				AssertEquals("MY_GOODS", order.JD_OrderGoodsDescription);
			});
		}

		#endregion

		#region TestRestrictOrderImport

		public void TestAllowOrderImport_AttachedSupplierBookingOfAllStatus()
		{
			var bookingStatuses = typeof(SupplierBookingStatus).GetFields();
			foreach (var status in bookingStatuses)
			{
				var bookingStatus = status.GetValue(null) as string;
				var bookingId = "SB001" + bookingStatus;
				var orderNo = "ORD001" + bookingStatus;
				var buyer = Data.ConsigneeOrgCRAHOLSYD;
				var (matchingOrder, _) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNo, bookingId, bookingStatus, 100);
				matchingOrder.BuyerPK = buyer.PK;
				buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = false;

				Factory.SaveForTesting();

				var orderDataObject = GetNewOrderDataObject();
				orderDataObject.Order = new UniversalOrder { OrderNumber = orderNo };
				orderDataObject.GoodsDescription = "MY_GOODS";

				Logger.ClearLogs();
				AssertNoExceptionThrown(bookingStatus, () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowOrderImport_WithoutAttachedSupplierBooking()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			foreach (var attachedOrderXMLUpdate in new[] { true, false })
			{
				buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = attachedOrderXMLUpdate;

				var matchingOrder = Factory.NewWithValidTestData<Order>();
				matchingOrder.JD_OrderNumber = "ORDERME" + attachedOrderXMLUpdate;
				matchingOrder.JD_OrderNumberSplit = new ZByte(2);
				matchingOrder.BuyerPK = buyer.PK;

				var orderline = matchingOrder.OrderLines.AddNew();
				orderline.FillWithValidTestData();

				Factory.SaveForTesting();

				var orderDataObject = GetNewOrderDataObject();
				orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME" + attachedOrderXMLUpdate, OrderNumberSplit = new ZByte(2) };
				orderDataObject.GoodsDescription = "MY_GOODS";

				var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);

				Order order = null;
				Logger.ClearLogs();
				AssertNoExceptionThrown("Update via XML is allowed", () => order = reader.ReadIntoBusinessObject());
				AssertEquals("MY_GOODS", order.JD_OrderGoodsDescription);
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowOrderImport_AllowUpdatingOrdersWithEventsRaised()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var sbkStatusArray = new List<string> { Constants.SupplierBookingStatus.Incomplete, Constants.SupplierBookingStatus.Rejected };
			var bookingStatuses = typeof(SupplierBookingStatus).GetFields().Select(status => status.GetValue(null) as string).ToArray();
			for (var i = 0; i < bookingStatuses.Length; i += 1)
			{
				var orderNum = "ORD" + i;
				var supplierBookingId = "SB" + i;
				var bookingStatus = bookingStatuses[i];

				var (matchingOrder, supplierBooking) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNum, supplierBookingId, bookingStatus, 100);
				matchingOrder.BuyerPK = buyer.PK;

				Factory.SaveForTesting();

				var orderDataObject = GetNewOrderDataObject();
				orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };
				orderDataObject.GoodsDescription = "MY_GOODS";

				Order orderOfTesting = null;
				Logger.ClearLogs();
				AssertNoExceptionThrown("Update via XML is allowed", () => orderOfTesting = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertEquals("MY_GOODS", orderOfTesting.JD_OrderGoodsDescription);
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowOrderImport_WithConcatedCaptions_WhenOneIncompleteBookingIsAttachedWithMultipleOrderLines()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD0";
			var supplierBookingId = "SB0";
			var bookingStatus = Constants.SupplierBookingStatus.Incomplete;

			var (matchingOrder, supplierBooking) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNum, supplierBookingId, bookingStatus, 100);
			matchingOrder.BuyerPK = buyer.PK;

			var orderLine = matchingOrder.OrderLines.AddNew();

			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_BookedQuantity = 100;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };

			Logger.ClearLogs();
			AssertNoExceptionThrown("Update via XML is allowed", () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
			AssertNullOrEmpty("should not have error", Logger.GetErrors());
		}

		public void TestAllowOrderImport_WhenMultipleOrderLinesAreAttachedToMultipleIncompleteBookings()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD0";
			var supplierBookingId = "SB0";
			var bookingStatus = Constants.SupplierBookingStatus.Incomplete;

			var (matchingOrder, supplierBooking1) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNum, supplierBookingId, bookingStatus, 100);
			matchingOrder.BuyerPK = buyer.PK;

			var orderLine1 = matchingOrder.OrderLines[0];
			var orderLine2 = matchingOrder.OrderLines.AddNew();

			var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking2.JSB_BookingId = "SB1";
			supplierBooking2.JSB_Status = Constants.SupplierBookingStatus.Incomplete;

			var bookingLine = supplierBooking2.SupplierBookingLines.AddNew();
			bookingLine.JSL_BookedQuantity = 100;
			bookingLine.JSL_JO_OrderLine = orderLine2.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };

			Logger.ClearLogs();
			AssertNoExceptionThrown("Update via XML is allowed", () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
			AssertNullOrEmpty("should not have error", Logger.GetErrors()); 
		}

		public void TestAllowOrderImport_IfRegistryIsTurnedOnAndLineReferenceIsPresent()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD0";
			var supplierBookingId = "SB0";
			var bookingStatus = Constants.SupplierBookingStatus.Incomplete;

			var (matchingOrder, supplierBooking) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNum, supplierBookingId, bookingStatus, 100);
			matchingOrder.BuyerPK = buyer.PK;

			var orderLine = matchingOrder.OrderLines[0];
			orderLine.JO_LineReference = "DummyLineReference";

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };

			using (var registry = OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Logger.ClearLogs();
				AssertNoExceptionThrown("Update via XML is allowed", () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowOrderImport_IfRegistryIsTurnedOnAndLineReferenceIsNotPresent()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD0";
			var supplierBookingId = "SB0";
			var bookingStatus = Constants.SupplierBookingStatus.Incomplete;

			var (matchingOrder, supplierBooking) = GenerateNewOrderAndAttachNewSBKWithGivenStatus(orderNum, supplierBookingId, bookingStatus, 100);
			matchingOrder.BuyerPK = buyer.PK;

			var orderLine = matchingOrder.OrderLines[0];
			orderLine.JO_LineReference = "";

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };

			using (var registry = OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Logger.ClearLogs();
				AssertNoExceptionThrown("Update via XML is allowed", () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowOrderImport_AttachedMultipleSupplierBookingWithAllStatus()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var bookingStatuses = typeof(SupplierBookingStatus).GetFields().Select(status => status.GetValue(null) as string).ToArray();
			for (var i = 0; i < bookingStatuses.Length; i += 1)
			{
				var orderNum = "ORD" + i;
				var bookingStatus = bookingStatuses[i];

				var matchingOrder = Factory.NewWithValidTestData<Order>();
				matchingOrder.JD_OrderNumber = orderNum;
				matchingOrder.BuyerPK = buyer.PK;

				var orderline1 = matchingOrder.OrderLines.AddNew();
				orderline1.JO_LineNo = 1;
				var supplierBooking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
				supplierBooking1.JSB_BookingId = "SB" + i + "1";
				supplierBooking1.JSB_Status = bookingStatus;
				var bookingLine1 = supplierBooking1.SupplierBookingLines.AddNew();
				bookingLine1.JSL_BookedQuantity = 100;
				bookingLine1.JSL_JO_OrderLine = orderline1.PK;

				var orderline2 = matchingOrder.OrderLines.AddNew();
				orderline2.JO_LineNo = 2;
				var supplierBooking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
				supplierBooking2.JSB_BookingId = "SB" + i + "2";
				supplierBooking2.JSB_Status = bookingStatus;
				var bookingLine2 = supplierBooking2.SupplierBookingLines.AddNew();
				bookingLine2.JSL_BookedQuantity = 50;
				bookingLine2.JSL_JO_OrderLine = orderline2.PK;

				Factory.SaveForTesting();

				var orderDataObject = GetNewOrderDataObject();
				orderDataObject.Order = new UniversalOrder { OrderNumber = matchingOrder.JD_OrderNumber };
				orderDataObject.GoodsDescription = "MY_GOODS";

				buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = false;
				Logger.ClearLogs();
				AssertNoExceptionThrown(bookingStatus, () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertNullOrEmpty("should not have error", Logger.GetErrors());

				buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;
				Logger.ClearLogs();
				AssertNoExceptionThrown(bookingStatus, () => new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject());
				AssertNullOrEmpty("should not have error", Logger.GetErrors());
			}
		}

		public void TestAllowUpdatingOrders_LessQuantityChanges_CANSBKLinked_BuyerXMLUpdateIsAllowed()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD123";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = orderNum;
			matchingOrder.BuyerPK = buyer.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc = ZDateTime.UtcNow.AddDays(2);
			matchingOrder.JD_JS = shipment.PK;

			var orderline = matchingOrder.OrderLines.AddNew();
			orderline.FillWithValidTestData();

			AttachSupplierBookingToGivenOrderLine(orderline, "SB111", Constants.SupplierBookingStatus.Cancelled, 500);

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = matchingOrder.JD_OrderNumber };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = orderline.JO_LineNo, OrderedQty = 100 });

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown("Should not throw an exception", () => reader.ReadIntoBusinessObject());
			AssertEquals(100m, orderline.JO_Quantity);
		}

		public void TestAllowUpdatingOrders_LessQuantityChanges_CNVSBKLinked_BuyerXMLUpdateIsAllowed()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD123";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = orderNum;
			matchingOrder.BuyerPK = buyer.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S";
			shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc = ZDateTime.UtcNow.AddDays(2);
			matchingOrder.JD_JS = shipment.PK;

			var orderline = matchingOrder.OrderLines.AddNew();
			orderline.FillWithValidTestData();

			AttachSupplierBookingToGivenOrderLine(orderline, "SB111", Constants.SupplierBookingStatus.Converted, 500);

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = matchingOrder.JD_OrderNumber };

			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = orderline.JO_LineNo, OrderedQty = 100 });

			Logger.ClearLogs();
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertNullOrEmpty("should not have error", Logger.GetErrors());
		}

		public void TestAllowUpdatingOrders_QtyLessThanPackedQtyWithNoActiveSBKLinked_BuyerXMLUpdateIsAllowed()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var orderNum = "ORD123";

			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = orderNum;
			matchingOrder.BuyerPK = buyer.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S";
			shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc = ZDateTime.UtcNow.AddDays(2);
			matchingOrder.JD_JS = shipment.PK;

			var orderline = matchingOrder.OrderLines.AddNew();

			var supplierBooking1 = AttachSupplierBookingToGivenOrderLine(orderline, "SB111", "CNV", 500);
			var supplierBooking2 = AttachSupplierBookingToGivenOrderLine(orderline, "SB222", "CAN", 500);
			var supplierBooking3 = AttachSupplierBookingToGivenOrderLine(orderline, "SB333", "CNV", 500);
			var supplierBooking4 = AttachSupplierBookingToGivenOrderLine(orderline, "SB444", "INC", 500);
			var supplierBooking5 = AttachSupplierBookingToGivenOrderLine(orderline, "SB555", "CAN", 500);
			var supplierBooking6 = AttachSupplierBookingToGivenOrderLine(orderline, "SB666", "INC", 500);

			Factory.SaveForTesting();
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = matchingOrder.JD_OrderNumber };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = orderline.JO_LineNo });

			var orderLineDataObject = orderDataObject.Order.OrderLineCollection.First();

			orderLineDataObject.OrderedQty = 500;
			Logger.ClearLogs();
			AssertNoExceptionThrown("Should not throw an exception", () => new OrderDataObjectReader(orderDataObject, Logger, new UniversalObjectFactory()).ReadIntoBusinessObject());
			AssertNullOrEmpty("should not have error", Logger.GetErrors());

			orderLineDataObject.OrderedQty = 2000;
			Logger.ClearLogs();
			AssertNoExceptionThrown("Should not throw an exception", () => new OrderDataObjectReader(orderDataObject, Logger, new UniversalObjectFactory()).ReadIntoBusinessObject());
			AssertNullOrEmpty("should not have error", Logger.GetErrors());
		}

		(Order, JobSupplierBooking) GenerateNewOrderAndAttachNewSBKWithGivenStatus(string orderNum, string supplierBookingId, string supplierBookingStatus, int bookingLineQuantity)
		{
			var matchingOrder = Factory.NewWithValidTestData<Order>();
			matchingOrder.JD_OrderNumber = orderNum;

			var orderline = matchingOrder.OrderLines.AddNew();

			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_BookingId = supplierBookingId;
			supplierBooking.JSB_Status = supplierBookingStatus;

			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_BookedQuantity = bookingLineQuantity;
			bookingLine.JSL_JO_OrderLine = orderline.PK;

			return (matchingOrder, supplierBooking);
		}

		JobSupplierBooking AttachSupplierBookingToGivenOrderLine(
			OrderLine matchingOrderLine,
			string supplierBookingId,
			string supplierBookingStatus,
			int bookingLineQuantity = 900)
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_BookingId = supplierBookingId;
			supplierBooking.JSB_Status = supplierBookingStatus;

			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = matchingOrderLine.PK;
			bookingLine.JSL_BookedQuantity = bookingLineQuantity;

			return supplierBooking;
		}

		#endregion

		#region TestAllowUpdatingOrdersAttachedToDeclaration

		public void TestShouldNotThrowExceptionWhenAttachToDeclaration_HasBeenAttachedToDeclaration_BuyerXMLUpdateIsNotAllowed()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			((Enterprise.Integration.Customs.IBaseJobDeclaration)declaration).JE_DeclarationReference = "B00000666";
			matchingOrder.JD_JE = declaration.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown("Can update Order 'ORDERME-2' even it is not allowed for Organization 'CRAHOLSYD' and it is already attached to Declaration 'B00000666'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestShouldNotThrowExceptionWhenAttachToDeclaration_HasBeenAttachedToDeclaration_BuyerXMLUpdateIsAllowed()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			((Enterprise.Integration.Customs.IBaseJobDeclaration)declaration).JE_DeclarationReference = "B00000666";
			matchingOrder.JD_JE = declaration.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown("Can update Order 'ORDERME-2' even it is already attached to Declaration 'B00000666'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestAllowUpdatingOrdersAttachedToDeclaration()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			((Enterprise.Integration.Customs.IBaseJobDeclaration)declaration).JE_DeclarationReference = "B00000666";

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			orderDataObject.GoodsDescription = "MY_GOODS";
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, (IAttachOrders)declaration);
			AssertNoExceptionThrown("Should not throw an exception", () =>
			{
				var order = reader.ReadIntoBusinessObject();
				AssertEquals("MY_GOODS", order.JD_OrderGoodsDescription);
			});
		}

		#endregion

		public void TestThrowsExceptionIfNewOrderLineQuantityIsLessThanTotalQuantities()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(1);
			var shipment = Factory.New<ForwardingShipment>();
			order1.JD_JS = shipment.PK;
			var order1Line = Factory.NewWithValidTestData<OrderLine>();
			order1Line.JO_LineNo = 6;
			order1Line.JO_Quantity = 2;
			order1Line.JO_QtyReceived = 2;
			order1.OrderLines.Add(order1Line);

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(2);
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			order2.JD_JE = declaration.PK;
			var order2Line = Factory.NewWithValidTestData<OrderLine>();
			order2Line.JO_LineNo = 6;
			order2Line.JO_Quantity = 3;
			order2Line.JO_QtyReceived = 3;
			order2.OrderLines.Add(order2Line);

			var order3 = Factory.New<Order>();
			order3.BuyerPK = buyer.PK;
			order3.JD_OrderNumber = "ORD0005";
			order3.JD_OrderNumberSplit = new ZByte(3);
			var order3Line = Factory.NewWithValidTestData<OrderLine>();
			order3Line.JO_LineNo = 6;
			order3Line.JO_Quantity = 5;
			order3.OrderLines.Add(order3Line);

			var order4 = Factory.New<Order>();
			order4.BuyerPK = buyer.PK;
			order4.JD_OrderNumber = "ORD0005";
			order4.JD_OrderNumberSplit = new ZByte(4);
			var order4Line = Factory.NewWithValidTestData<OrderLine>();
			order4Line.JO_LineNo = 6;
			order4Line.JO_Quantity = 7;
			order4.OrderLines.Add(order4Line);

			var order5 = Factory.New<Order>();
			order5.BuyerPK = buyer.PK;
			order5.JD_OrderNumber = "ORD0005";
			order5.JD_OrderNumberSplit = new ZByte(5);

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 6, OrderedQty = 4 });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot update Order 'ORD0005' - Line 6 as quantity is less than total quantities attached to shipment(s) and/or declaration(s).", () => reader.ReadIntoBusinessObject());    // It's unit test. No code smells should be done here.
		}

		public void TestShouldNotThrowExceptionWhenAllOrdersAreAttachedAndNoMatchedLine()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(1);
			var shipment = Factory.New<ForwardingShipment>();
			order1.JD_JS = shipment.PK;
			var order1Line = Factory.NewWithValidTestData<OrderLine>();
			order1Line.JO_LineNo = 6;
			order1Line.JO_Quantity = 2;
			order1.OrderLines.Add(order1Line);

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(2);
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			order2.JD_JE = declaration.PK;
			var order2Line = Factory.NewWithValidTestData<OrderLine>();
			order2Line.JO_LineNo = 6;
			order2Line.JO_Quantity = 3;
			order2.OrderLines.Add(order2Line);

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 8, OrderedQty = 4 });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);

			AssertNoExceptionThrown("Can update Order 'ORD0005-2' as even it is already attached to Declaration 'B00001000'.", () => reader.ReadIntoBusinessObject());  // It's unit test. No code smells should be done here.
		}

		public void TestShouldThrowExceptionWhenDeclarationHasCommencedEvent()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			((Enterprise.Integration.Customs.IBaseJobDeclaration)declaration).JE_DeclarationReference = "B00000666";
			matchingOrder.JD_JE = declaration.PK;

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_Table = declaration.TableName;
				eventLog.SL_Parent = declaration.PK;
				eventLog.SL_SE_NKEvent = Events.CustomsCommenced.Code;
			}

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot update Order ORDERME-2 as a 'customs commenced' event exists on Declaration 'B00000666'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestShouldThrowExceptionWhenDeletedLineIsAttachedToAnInvoiceLine()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;
			var orderLine1 = matchingOrder.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = matchingOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var declaration = (Enterprise.Integration.Customs.IBaseJobDeclaration)Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration.JE_DeclarationReference = "B00000666";
			var invoice = (Enterprise.Integration.Customs.AU.IJobComInvoiceHeader)Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobComInvoiceHeader>());
			invoice.JZ_JE = declaration.PK;
			var invoiceLine1 = (Enterprise.Integration.Customs.IBaseJobComInvoiceLine)Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>());
			invoiceLine1.JI_JZ = invoice.PK;
			invoiceLine1.JI_JO = orderLine2.PK;

			matchingOrder.JD_JE = ZGuid.Empty;
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, SubLineNumber = 0 });

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var exception = AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
			AssertEquals("Cannot update Order ORDERME-2.  Cannot delete Order Line 2 as it is attached to an Invoice Line on Declaration 'B00000666'", exception.Message);
		}

		public void TestImportShouldNotThrowException_NotUpdatedWhenChangingBuyerAddressToCauseUniqueIndexViolation()
		{
			var org1 = Data.ConsigneeOrgCRAHOLSYD;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Org2";

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OA_BuyerAddress = org1.MainAddress.PK;
			order1.JD_OrderNumber = "ORDERA";
			order1.JD_OrderNumberSplit = 0;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OA_BuyerAddress = org2.MainAddress.PK;
			order2.JD_OrderNumber = "ORDERA";
			order2.JD_OrderNumberSplit = 0;

			Factory.SaveForTesting();

			var dataObject = GetNewOrderDataObject();
			dataObject.Order = new UniversalOrder();
			dataObject.Order.OrderNumber = "ORDERA";
			dataObject.Order.OrderNumberSplit = new ZByte(0);

			dataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>()
				{
					new DataTarget() { Key = "ORDERA~0~Org2", Type = "OrderManagerOrder" }
				}
			};

			var reader = new OrderDataObjectReader(dataObject, Logger, Factory);

			AssertEquals("Precondition: dataObject context matches to Buyer 'Org2'", "Org2", reader.TryGetExistingBusinessObject().Buyer.OH_Code);
			AssertEquals("Precondition: dataObject's buyer to set is CRAHOLSYD", "CRAHOLSYD", dataObject.OrganizationAddressCollection[0].OrganizationCode);

			reader.ReadIntoBusinessObject();
			AssertContains("Cannot update Buyer Address as an Order with Order Number 'ORDERA' and Order Number Split '0' already exists for the Buyer Address in the import file", Logger.GetWarnings());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestImportShouldNotThrowException_NotUpdatedWhenChangingOrderNumberSplitToCauseUniqueIndexViolation()
		{
			var org = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OA_BuyerAddress = org.MainAddress.PK;
			order1.JD_OrderNumber = "ORDERA";
			order1.JD_OrderNumberSplit = 0;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OA_BuyerAddress = org.MainAddress.PK;
			order2.JD_OrderNumber = "ORDERA";
			order2.JD_OrderNumberSplit = 1;

			Factory.SaveForTesting();

			var dataObject = GetNewOrderDataObject();
			dataObject.Order = new UniversalOrder();
			dataObject.Order.OrderNumber = "ORDERA";
			dataObject.Order.OrderNumberSplit = new ZByte(0);

			dataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>()
				{
					new DataTarget() { Key = "ORDERA~1~CRAHOLSYD", Type = "OrderManagerOrder" }
				}
			};

			var reader = new OrderDataObjectReader(dataObject, Logger, Factory);

			AssertEquals("Precondition: dataObject context matches order with split '1'", new ZByte(1), reader.TryGetExistingBusinessObject().JD_OrderNumberSplit);
			AssertEquals("Precondition: dataObject's order number split to set is '0'", new ZByte(0), dataObject.Order.OrderNumberSplit);

			reader.ReadIntoBusinessObject();
			AssertContains("Cannot update Order Number Split as an Order with Order Number 'ORDERA' and Order Number Split '0' already exists for the Buyer Address in the import file", Logger.GetWarnings());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestImportShouldNotThrowException_NotUpdatedWhenChangingOrderNumberToCauseUniqueIndexViolation()
		{
			var org = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OA_BuyerAddress = org.MainAddress.PK;
			order1.JD_OrderNumber = "ORDERA";
			order1.JD_OrderNumberSplit = 0;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OA_BuyerAddress = org.MainAddress.PK;
			order2.JD_OrderNumber = "ORDERB";
			order2.JD_OrderNumberSplit = 0;

			Factory.SaveForTesting();

			var dataObject = GetNewOrderDataObject();
			dataObject.Order = new UniversalOrder();
			dataObject.Order.OrderNumber = "ORDERA";
			dataObject.Order.OrderNumberSplit = new ZByte(0);

			dataObject.DataContext = new DataContext()
			{
				DataTargetCollection = new List<DataTarget>()
				{
					new DataTarget() { Key = "ORDERB~0~CRAHOLSYD", Type = "OrderManagerOrder" }
				}
			};

			var reader = new OrderDataObjectReader(dataObject, Logger, Factory);

			AssertEquals("Precondition: dataObject context matches order with number 'ORDERB'", "ORDERB", reader.TryGetExistingBusinessObject().JD_OrderNumber);
			AssertEquals("Precondition: dataObject's order number to set is 'ORDERA'", "ORDERA", dataObject.Order.OrderNumber);

			reader.ReadIntoBusinessObject();
			AssertContains("Cannot update Order Number as an Order with Order Number 'ORDERA' and Order Number Split '0' already exists for the Buyer Address in the import file", Logger.GetWarnings());
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		//If this test failed it means the unique index NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress is changed. Please fix method EnsureUniqueIndexDoesNotConflict
		public void TestUniqueIndexNR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddressExists() => AssertNotNullOrEmpty(JobOrderHeaderSchema.Constants.Indexes.NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress);

		#endregion

		#region Matching

		#region TestMatchesOrderOnShipment

		public void TestMatchesOrderOnShipment_WithOrderNumberSplit()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var matchingOrder = shipment.AttachedOrders.AddNew();
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_OrderNumber = "ORDER ME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };
			orderDataObject.Order.OrderNumberSplit = new ZByte(2);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(matchingOrder, order);
			AssertEquals(false, Logger.HasErrors);
		}

		public void TestMatchesOrderOnShipment_WithOrderNumberSplit_WithCancelledOrder()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var order1 = shipment.AttachedOrders.AddNew();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORDER ME";
			order1.JD_OrderNumberSplit = new ZByte(2);
			order1.JD_IsCancelled = true;

			var order2 = shipment.AttachedOrders.AddNew();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORDER ME";
			order2.JD_OrderNumberSplit = new ZByte(2);

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };
			orderDataObject.Order.OrderNumberSplit = new ZByte(2);
			AssertEquals(order2, new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment).ReadIntoBusinessObject());
		}

		public void TestMatchesOrder_WithOrderNumberSplit_WithCancelledOrder()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORDER ME";
			order1.JD_OrderNumberSplit = new ZByte(2);
			order1.JD_IsCancelled = true;

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORDER ME";
			order2.JD_OrderNumberSplit = new ZByte(2);

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };
			AssertEquals(order2, new OrderDataObjectReader(orderDataObject, Logger, Factory, null).ReadIntoBusinessObject());
		}

		public void TestMatchesOrderOnShipment_WithoutOrderNumberSplit()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var matchingOrder = shipment.AttachedOrders.AddNew();
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_OrderNumber = "ORDER ME";

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var order = reader.ReadIntoBusinessObject();

			AssertNull(order);
			AssertNotEquals(matchingOrder, order);
			AssertEquals(true, Logger.HasErrors);
			AssertEquals(
				"Cannot populate Order because:\r\nThe order has been attach to Shipment or Declaration but it is missing 'OrderNumberSplit'.",
				Logger.GetErrors());
		}

		public void TestMatchesOrderOnShipmentWithEmptySplitNumber()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var firstOrder = Factory.New<Order>();
			firstOrder.BuyerPK = buyer.PK;
			firstOrder.JD_OrderNumber = "ORD0005";
			firstOrder.JD_OrderNumberSplit = new ZByte(0);
			Factory.SaveForTesting();

			var lastOrder = Factory.New<Order>();
			lastOrder.BuyerPK = buyer.PK;
			lastOrder.JD_OrderNumber = "ORD0005";
			lastOrder.JD_OrderNumberSplit = new ZByte(2);
			Factory.SaveForTesting();

			var secondOrder = Factory.New<Order>();
			secondOrder.BuyerPK = buyer.PK;
			secondOrder.JD_OrderNumber = "ORD0005";
			secondOrder.JD_OrderNumberSplit = new ZByte(1);
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORD0005" };
			var reader1 = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var processedOrder1 = reader1.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);
			AssertNotNull(processedOrder1);
			AssertEquals("ORD0005-2", processedOrder1.JD_OrderNumberAndSplit);
			AssertEquals("Should match this order with order with the greatest split number", lastOrder, processedOrder1);

			orderDataObject.Order.OrderNumberSplit = new ZByte(0);
			var reader2 = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var processedOrder2 = reader2.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);
			AssertNotNull(processedOrder2);
			AssertEquals("Should match the original order as the order split is 0", firstOrder, processedOrder2);
			AssertEquals("ORD0005", processedOrder2.JD_OrderNumberAndSplit);
		}

		public void TestUpdateOrderLinesOnFirstNonLinkedOrderSplitAndEraseDuplicateLines()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order5 = Factory.New<Order>();
			order5.BuyerPK = buyer.PK;
			order5.JD_OrderNumber = "ORD0005";
			order5.JD_OrderNumberSplit = new ZByte(4);
			var order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 6;
			order5Line.JO_Quantity = 11;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 8;
			order5Line.JO_Quantity = 17;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 2;
			order5Line.JO_Quantity = 40;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 11;
			order5Line.JO_Quantity = 40;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 12;
			order5Line.JO_Quantity = 40;
			Factory.SaveForTesting();

			var order4 = Factory.New<Order>();
			order4.BuyerPK = buyer.PK;
			order4.JD_OrderNumber = "ORD0005";
			order4.JD_OrderNumberSplit = new ZByte(3);
			var order4Line = order4.OrderLines.AddNew();
			order4Line.JO_LineNo = 6;
			order4Line.JO_Quantity = 7;
			Factory.SaveForTesting();

			var order3 = Factory.New<Order>();
			order3.BuyerPK = buyer.PK;
			order3.JD_OrderNumber = "ORD0005";
			order3.JD_OrderNumberSplit = new ZByte(2);
			var order3Line = order3.OrderLines.AddNew();
			order3Line.JO_LineNo = 3;
			order3Line.JO_Quantity = 5;
			order3Line = order3.OrderLines.AddNew();
			order3Line.JO_LineNo = 8;
			order3Line.JO_Quantity = 13;
			Factory.SaveForTesting();

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(1);
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			order2.JD_JE = declaration.PK;
			var order2Line = order2.OrderLines.AddNew();
			order2Line.JO_LineNo = 6;
			order2Line.JO_Quantity = 3;
			order2Line.JO_QtyReceived = 3;
			Factory.SaveForTesting();

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(0);
			var shipment = Factory.New<ForwardingShipment>();
			order1.JD_JS = shipment.PK;
			var order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 6;
			order1Line.JO_Quantity = 2;
			order1Line.JO_QtyReceived = 2;
			order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 8;
			order1Line.JO_Quantity = 1;
			order1Line.JO_QtyReceived = 1;
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 6, OrderedQty = 18 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 8, OrderedQty = 20 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 2, OrderedQty = 40 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 3, OrderedQty = 6 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 10, OrderedQty = 10 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 11 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 12, OrderedQty = 0 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { OrderedQty = 3 });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);

			AssertEquals(2, order3.OrderLines.Count);
			AssertEquals("No match",
				5m, order3.OrderLines.First(orderLine => orderLine.JO_LineNo == 3).JO_Quantity);
			AssertEquals("No match",
				13m, order3.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_Quantity);

			AssertEquals(1, order4.OrderLines.Count);
			AssertEquals("No match",
				7m, order4.OrderLines.First().JO_Quantity);

			AssertEquals("Order lines without line number should be skipped.", 8, order5.OrderLines.Count);

			AssertEquals("Match and update",
				40m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 2).JO_Quantity);
			AssertEquals("Match and update",
				40m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 11).JO_Quantity);
			AssertEquals("Match and update",
				0m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 12).JO_Quantity);
			AssertEquals("Match and update",
				13m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 6).JO_Quantity);
			AssertEquals("Match and update",
				19m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_Quantity);
			AssertEquals("Match and update",
				6m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 3).JO_Quantity);
			AssertEquals("Match and update",
				10m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 10).JO_Quantity);
		}

		public void TestImportOrderLineWhenOrderedQtyIsZeroOrNull()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORD0005";
			order.JD_OrderNumberSplit = new ZByte(0);
			var order1Line = order.OrderLines.AddNew();
			order1Line.JO_LineNo = 1;
			order1Line.JO_Quantity = 150;
			order1Line.JO_QtyReceived = 50;
			var order1Line2 = order.OrderLines.AddNew();
			order1Line2.JO_LineNo = 2;
			order1Line2.JO_Quantity = 200;
			order1Line2.JO_QtyReceived = 180;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 2 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 3, OrderedQty = 0 });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);

			AssertEquals(3, order.OrderLines.Count);
			AssertEquals("Unmatched", 150m, order.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
			AssertEquals("Matched but don't update JO_Quantity as OrderedQty is null", 200m, order.OrderLines.First(orderLine => orderLine.JO_LineNo == 2).JO_Quantity);
			AssertEquals("Not matched, add a new order line", 0m, order.OrderLines.First(orderLine => orderLine.JO_LineNo == 3).JO_Quantity);
		}

		public void TestUpdateOrderLinesBasedOnQtyReceived()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order3 = Factory.New<Order>();
			order3.BuyerPK = buyer.PK;
			order3.JD_OrderNumber = "ORD0005";
			order3.JD_OrderNumberSplit = new ZByte(2);
			var order3Line = order3.OrderLines.AddNew();
			order3Line.JO_LineNo = 1;
			order3Line.JO_Quantity = 61;
			Factory.SaveForTesting();

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(1);
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			order2.JD_JE = declaration.PK;
			var order2Line = order2.OrderLines.AddNew();
			order2Line.JO_LineNo = 1;
			order2Line.JO_Quantity = 100;
			order2Line.JO_QtyReceived = 40;
			Factory.SaveForTesting();

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(0);
			var shipment = Factory.New<ForwardingShipment>();
			order1.JD_JS = shipment.PK;
			var order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 1;
			order1Line.JO_Quantity = 150;
			order1Line.JO_QtyReceived = 50;
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, OrderedQty = 150 });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);

			AssertEquals(1, order3.OrderLines.Count);
			AssertEquals(60m, order3.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
		}

		public void TestUpdateOrderLineWithCancelledStatus()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(0);
			var shipment = Factory.New<ForwardingShipment>();
			order1.JD_JS = shipment.PK;
			var order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 6;
			order1Line.JO_Quantity = 2;
			order1Line.JO_LineStatus = "PLC";
			order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 8;
			order1Line.JO_Quantity = 1;
			order1Line.JO_LineStatus = "PLC";

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(1);
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			order2.JD_JE = declaration.PK;
			var order2Line = order2.OrderLines.AddNew();
			order2Line.JO_LineNo = 6;
			order2Line.JO_Quantity = 3;
			order2Line.JO_LineStatus = "PLC";

			var order3 = Factory.New<Order>();
			order3.BuyerPK = buyer.PK;
			order3.JD_OrderNumber = "ORD0005";
			order3.JD_OrderNumberSplit = new ZByte(2);
			var order3Line = order3.OrderLines.AddNew();
			order3Line.JO_LineNo = 3;
			order3Line.JO_Quantity = 5;
			order3Line = order3.OrderLines.AddNew();
			order3Line.JO_LineNo = 8;
			order3Line.JO_Quantity = 13;

			var order4 = Factory.New<Order>();
			order4.BuyerPK = buyer.PK;
			order4.JD_OrderNumber = "ORD0005";
			order4.JD_OrderNumberSplit = new ZByte(3);
			var order4Line = order4.OrderLines.AddNew();
			order4Line.JO_LineNo = 6;
			order4Line.JO_Quantity = 7;

			var order5 = Factory.New<Order>();
			order5.BuyerPK = buyer.PK;
			order5.JD_OrderNumber = "ORD0005";
			order5.JD_OrderNumberSplit = new ZByte(4);
			var order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 6;
			order5Line.JO_Quantity = 11;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 8;
			order5Line.JO_Quantity = 17;
			order5Line = order5.OrderLines.AddNew();
			order5Line.JO_LineNo = 2;
			order5Line.JO_Quantity = 40;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 6, OrderedQty = 4, Status = new CodeDescriptionPair { Code = "CAN" } });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 8, OrderedQty = 20, Status = new CodeDescriptionPair { Code = "CAN" } });
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());

			AssertEquals(true, Logger.HasErrors);
			AssertMultilineASCIIEquals("Incorrect results", @"Cannot update Order 'ORD0005' - Line 6 as it is already attached to Shipment 'S00001000'.
Cannot update Order 'ORD0005-1' - Line 6 as it is already attached to Declaration 'B00001000'.
Cannot update Order 'ORD0005' - Line 8 as it is already attached to Shipment 'S00001000'.", Logger.GetErrors());
			AssertEquals(2, order1.OrderLines.Count);
			AssertEquals("PLC", order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 6).JO_LineStatus);
			AssertEquals("PLC", order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_LineStatus);
			AssertEquals(1, order2.OrderLines.Count);
			AssertEquals("PLC", order2.OrderLines.First(orderLine => orderLine.JO_LineNo == 6).JO_LineStatus);
			AssertEquals(2, order3.OrderLines.Count);
			AssertEquals(13m, order3.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_Quantity);
			AssertEquals("INC", order3.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_LineStatus);
			AssertEquals(1, order4.OrderLines.Count);
			AssertEquals(7m, order4.OrderLines.First().JO_Quantity);
			AssertEquals("INC", order4.OrderLines.First().JO_LineStatus);
			AssertEquals(2, order5.OrderLines.Count);
			AssertEquals("CAN", order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 6).JO_LineStatus);
			AssertEquals(0m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 6).JO_Quantity);
			AssertEquals("CAN", order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_LineStatus);
			AssertEquals(0m, order5.OrderLines.First(orderLine => orderLine.JO_LineNo == 8).JO_Quantity);
		}

		public void TestMatchesOrderOnShipmentWithSplitNumber()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var originalOrder = Factory.New<Order>();
			originalOrder.JD_OrderNumber = "ORD0005";
			originalOrder.JD_OrderNumberSplit = (ZByte)0;
			originalOrder.BuyerPK = buyer.PK;

			var splitOrder1 = Factory.New<Order>();
			splitOrder1.JD_OrderNumber = "ORD0005";
			splitOrder1.JD_OrderNumberSplit = (ZByte)1;
			splitOrder1.BuyerPK = buyer.PK;

			var splitOrder2 = Factory.New<Order>();
			splitOrder2.JD_OrderNumber = "ORD0005";
			splitOrder2.JD_OrderNumberSplit = (ZByte)2;
			splitOrder2.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORD0005" };
			orderDataObject.Order.OrderNumberSplit = new ZByte(2);
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var processedOrder = reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);
			AssertNotNull(processedOrder);
			AssertEquals("ORD0005-2", processedOrder.JD_OrderNumberAndSplit);
			AssertEquals(splitOrder2, processedOrder);

			orderDataObject.Order.OrderNumberSplit = new ZByte(1);
			reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			processedOrder = reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);
			AssertNotNull(processedOrder);
			AssertEquals("ORD0005-1", processedOrder.JD_OrderNumberAndSplit);
			AssertEquals(splitOrder1, processedOrder);
		}

		public void TestProcessOrderLinesWithPartialAttribute()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(0);
			var order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 1;
			order1Line.JO_Quantity = 150;
			order1Line.JO_QtyReceived = 50;
			var order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_LineNo = 2;
			order1Line2.JO_Quantity = 200;
			order1Line2.JO_QtyReceived = 180;
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, OrderedQty = 160 });
			var secondOrderLineObject = OrderLineDataObjectReaderTest.GetNewOrderLineDataObject();
			secondOrderLineObject.LineNumber = 3;
			orderDataObject.Order.OrderLineCollection.Add(secondOrderLineObject);
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);

			AssertEquals(3, order1.OrderLines.Count);
			AssertEquals(160m, order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
			AssertEquals(200m, order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 2).JO_Quantity);
			AssertEquals(14.4m, order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 3).JO_Quantity);
		}

		public void TestProcessOrderLinesWithPartialAttribute_LocateLargestSplitNumber()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";
			order1.JD_OrderNumberSplit = new ZByte(0);
			var order1Line = order1.OrderLines.AddNew();
			order1Line.JO_LineNo = 1;
			order1Line.JO_Quantity = 150;
			order1Line.JO_QtyReceived = 50;
			var order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_LineNo = 2;
			order1Line2.JO_Quantity = 200;
			order1Line2.JO_QtyReceived = 200;

			var order2 = Factory.New<Order>();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "ORD0005";
			order2.JD_OrderNumberSplit = new ZByte(1);
			var order2Line = order2.OrderLines.AddNew();
			order2Line.JO_LineNo = 1;
			order2Line.JO_Quantity = 100;
			order2Line.JO_QtyReceived = 0;
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, OrderedQty = 160 });
			var secondOrderLineObject = OrderLineDataObjectReaderTest.GetNewOrderLineDataObject();
			secondOrderLineObject.LineNumber = 3;
			orderDataObject.Order.OrderLineCollection.Add(secondOrderLineObject);
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals(false, Logger.HasErrors);

			AssertEquals(2, order1.OrderLines.Count);
			AssertEquals(150m, order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
			AssertEquals(200m, order1.OrderLines.First(orderLine => orderLine.JO_LineNo == 2).JO_Quantity);

			AssertEquals(2, order2.OrderLines.Count);
			AssertEquals(160m, order2.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
			AssertEquals(14.4m, order2.OrderLines.First(orderLine => orderLine.JO_LineNo == 3).JO_Quantity);
		}

		#endregion

		#region TestCanMatchOrderNotOnShipment

		public void TestCanMatchOrderNotOnShipment_WithOrderNumberSplit()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var matchingOrder = Factory.New<Order>();
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_OrderNumber = "ORDER ME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };
			orderDataObject.Order.OrderNumberSplit = new ZByte(2);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var order = reader.ReadIntoBusinessObject();

			AssertEquals(matchingOrder, order);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestCanMatchOrderNotOnShipment_WithoutOrderNumberSplit()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var matchingOrder = Factory.New<Order>();
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_OrderNumber = "ORDER ME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var newOrder = reader.ReadIntoBusinessObject();

			AssertNull(newOrder);
			AssertNotEquals(matchingOrder, newOrder);
			AssertEquals(true, Logger.HasErrors);
			AssertEquals(
				"Cannot populate Order because:\r\nThe order has been attach to Shipment or Declaration but it is missing 'OrderNumberSplit'.",
				Logger.GetErrors());
		}

		#endregion

		#region TestLoadsOrderFromBuyerAndOrderNumberCombination

		public void TestLoadsOrderFromBuyerAndOrderNumberCombination()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var orderWithDifferentSplitNumber = Factory.New<Order>();
			orderWithDifferentSplitNumber.JD_OrderNumber = "ORDERME";
			orderWithDifferentSplitNumber.JD_OrderNumberSplit = new ZByte(3);
			orderWithDifferentSplitNumber.BuyerPK = buyer.PK;

			var orderWithDifferentBuyer = Factory.New<Order>();
			orderWithDifferentBuyer.JD_OrderNumber = "ORDERME";
			orderWithDifferentBuyer.JD_OrderNumberSplit = new ZByte(2);
			orderWithDifferentBuyer.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderWithDifferentOrderNumber = Factory.New<Order>();
			orderWithDifferentOrderNumber.JD_OrderNumber = "DONTORDERME";
			orderWithDifferentOrderNumber.JD_OrderNumberSplit = new ZByte(2);
			orderWithDifferentOrderNumber.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME", OrderNumberSplit = new ZByte(2) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(matchingOrder, order);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestLoadsOrderFromBuyerAndOrderNumberCombination_FindsOrderWithHighestSplitWhenSplitNotInXML

		public void TestLoadsOrderFromBuyerAndOrderNumberCombination_FindsOrderWithHighestSplitWhenSplitNotInXML()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDERME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(2);
			matchingOrder.BuyerPK = buyer.PK;

			var orderWithLowerSplitNumber = Factory.New<Order>();
			orderWithLowerSplitNumber.JD_OrderNumber = "ORDERME";
			orderWithLowerSplitNumber.JD_OrderNumberSplit = new ZByte(1);
			orderWithLowerSplitNumber.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDERME" };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(matchingOrder, order);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestLoadOrderFromReferences

		public void TestLoadOrderFromReferences()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_OrderNumber = "1";
			matchingOrder.JD_BookingConfRef = "REF123";
			matchingOrder.JD_InvoiceNumber = "INV123";
			matchingOrder.JD_Waybill = "HOUSE123";

			var nonMatchingOrder = Factory.New<Order>();
			nonMatchingOrder.BuyerPK = buyer.PK;
			nonMatchingOrder.JD_OrderNumber = "2";
			nonMatchingOrder.JD_BookingConfRef = "REF456";
			nonMatchingOrder.JD_InvoiceNumber = "INV123";
			nonMatchingOrder.JD_Waybill = "HOUSE123";

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.BookingConfirmationReference = "REF123";
			orderDataObject.WayBillNumber = "HOUSE123";
			orderDataObject.CommercialInfo = new CommercialInfo { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { new CommercialInvoiceHeader { InvoiceNumber = "INV123" } } };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(matchingOrder, order);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestMatchesOnOtherReferencesOnlyIfOrderNumberIsEmpty

		public void TestMatchesOnOtherReferencesOnlyIfOrderNumberIsEmpty()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "123";
			matchingOrder.JD_BookingConfRef = "REF123";
			matchingOrder.BuyerPK = buyer.PK;

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.BookingConfirmationReference = "REF123";
			orderDataObject.Order = new UniversalOrder { OrderNumber = "456" };

			Factory.SaveForTesting();

			var newOrder = new OrderDataObjectReader(orderDataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			AssertNotEquals(matchingOrder.PK, newOrder.PK);

			newOrder.Delete(); // clean up

			orderDataObject.Order = null;
			var loadedOrder = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(matchingOrder, loadedOrder);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#endregion

		#region TestOrderNotUpdatedEvenIfMatchedThroughParent

		public void TestOrderNotUpdatedEvenIfMatchedThroughParent()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipment = Factory.New<ForwardingShipment>();
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORDER ME", OrderNumberSplit = ZByte.Zero };
			orderDataObject.BookingConfirmationReference = "BOOK ME";
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(OrderLineDataObjectReaderTest.GetNewOrderLineDataObject());

			var reader1 = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var newOrder = reader1.ReadIntoBusinessObject();
			AssertNotNull(newOrder);
			AssertEquals("BOOK ME", newOrder.JD_BookingConfRef);
			OrderLineDataObjectReaderTest.AssertContents(newOrder.OrderLines[0]);

			newOrder.JD_JS = shipment.PK;
			Factory.SaveForTesting();

			orderDataObject.BookingConfirmationReference = "DONT BOOK ME";
			orderDataObject.Order.OrderLineCollection.Clear();
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 67, AdditionalTerms = "Something Different" });
			var reader2 = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipment);
			var matchedOrder = reader2.ReadIntoBusinessObject();
			AssertNotNull(matchedOrder);

			CombineAssertions(delegate
			{
				AssertEquals(newOrder, matchedOrder);
				AssertEquals("BOOK ME", matchedOrder.JD_BookingConfRef);
				AssertEquals(1, matchedOrder.OrderLines.Count);
				OrderLineDataObjectReaderTest.AssertContents(matchedOrder.OrderLines[0]);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals("Cannot populate Order because:\r\nOrder 'ORDER ME' is not allowed for Organization 'CRAHOLSYD' and order is already attached to this Shipment.", Logger.GetWarnings());
			});
		}

		#endregion

		#region TestOrderIgnoredWhenShipmentParentPassedThroughAndMatchedOrderIsAttachedToAnotherShipment

		public void TestOrderIgnoredWhenShipmentParentPassedThroughAndMatchedOrderIsAttachedToAnotherShipment()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipmentParent = Factory.New<ForwardingShipment>();
			var otherShipment = Factory.New<ForwardingShipment>();
			var matchingOrder = otherShipment.AttachedOrders.AddNew();
			matchingOrder.JD_BookingConfRef = "BOOK ME";
			matchingOrder.JD_OrderNumber = "ORDER ME";
			matchingOrder.JD_OrderNumberSplit = new ZByte(1);
			matchingOrder.BuyerPK = buyer.PK;

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.BookingConfirmationReference = "DONT BOOK ME";
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(1) };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipmentParent);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			CombineAssertions(delegate
			{
				AssertEquals(matchingOrder, order);
				AssertEquals("BOOK ME", matchingOrder.JD_BookingConfRef);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals("Cannot populate Order because:\r\nOrder 'ORDER ME-1' is attached to another Shipment. Order ignored.", Logger.GetWarnings());
			});
		}

		public void TestErrorLoggedWhenShipmentParentPassedButHasNoOrderNumberSplitValue()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var shipmentParent = Factory.New<ForwardingShipment>();
			var matchingOrder = shipmentParent.AttachedOrders.AddNew();
			matchingOrder.JD_BookingConfRef = "BOOK ME";
			matchingOrder.JD_OrderNumber = "ORDER ME";
			matchingOrder.BuyerPK = buyer.PK;

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.BookingConfirmationReference = "DONT BOOK ME";
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME" };
			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory, shipmentParent);
			var order = reader.ReadIntoBusinessObject();

			AssertNull(order);
			CombineAssertions(delegate
			{
				AssertEquals("BOOK ME", matchingOrder.JD_BookingConfRef);
				AssertEquals(true, Logger.HasErrors);
				AssertEquals(
					"Cannot populate Order because:\r\nThe order has been attach to Shipment or Declaration but it is missing 'OrderNumberSplit'.",
					Logger.GetErrors());
			});
		}

		#endregion

		public void TestImport_InactiveOrdersAreNotMatchedAndImportedAsNewOrder()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var matchingOrder = Factory.New<Order>();
			matchingOrder.JD_OrderNumber = "ORDER42";
			matchingOrder.JD_ActualVolume = 197.555;
			matchingOrder.JD_OrderNumberSplit = new ZByte(1);
			matchingOrder.BuyerPK = buyer.PK;
			matchingOrder.JD_IsCancelled = true;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.TotalVolume = 5000.300;

			orderDataObject.Order = new UniversalOrder
			{
				OrderNumber = "ORDER42",
				OrderNumberSplit = new ZByte(1),
			};

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			CombineAssertions(delegate
			{
				AssertNotEquals(matchingOrder.PK, order.PK);
				AssertEquals((ZDecimal)197.555, matchingOrder.JD_ActualVolume);
				AssertEquals(true, matchingOrder.JD_IsCancelled);
				AssertEquals((ZDecimal)5000.300, order.JD_ActualVolume);
				AssertEquals(false, order.JD_IsCancelled);
				AssertEquals(false, Logger.HasErrors);
				AssertNullOrEmpty(Logger.GetWarnings());
			});
		}

		#endregion

		#region Related Entities

		#region TestOrganisations

		public void TestOrganisations()
		{
			var testLogger = new TestErrorLogger();
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var supplierDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress);

			var mainAddress1 = new OrganisationDataObjectReader(buyerDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress1 = mainAddress1.Header.Addresses.AddNew();
			newAddress1.OA_Address1 = "124 Polo Street";
			newAddress1.OA_Code = "Polo1";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var deliverAddressDataObject = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsigneePickupDeliveryAddress)).GetDataObject(newAddress1);
			deliverAddressDataObject.Email = null;
			deliverAddressDataObject.Phone = null;
			deliverAddressDataObject.Mobile = null;
			deliverAddressDataObject.Fax = null;

			var mainAddress2 = new OrganisationDataObjectReader(supplierDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress2 = mainAddress2.Header.Addresses.AddNew();
			newAddress2.OA_Address1 = "17 Trogo Street";
			newAddress2.OA_Code = "FanTrog";

			var pickupAddressDataObject = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsignorPickupDeliveryAddress)).GetDataObject(newAddress2);
			pickupAddressDataObject.Email = null;
			pickupAddressDataObject.Phone = null;
			pickupAddressDataObject.Mobile = null;
			pickupAddressDataObject.Fax = null;

			var carrierDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.Carrier);
			var sendingAgentDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.SendingForwarderAddress);
			var receivingAgentDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ReceivingForwarderAddress);
			var controllingPartyDataObject = GetAddressData(nameof(DocAddressType.ControllingCustomer), "Moment", "AUSYD");
			var warehouseAddressDataObject = GetNewAddressData_WUFSHIJNB(DocAddressType.Warehouse);
			var notifyPartyAddressDataObject = GetNewAddressData_WUFSHIJNB(DocAddressType.NotifyParty);
			var notifyParty2AddressDataObject = GetAddressData(nameof(DocAddressType.NotifyParty2), "SGSIN Test", "SGSIN");
			var notifyParty3AddressDataObject = GetAddressData(nameof(DocAddressType.NotifyParty3), "CNSZX Test", "CNSZX");
			new OrganisationDataObjectReader(buyerDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			new OrganisationDataObjectReader(supplierDataObject, testLogger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(supplierDataObject);
			orderDataObject.OrganizationAddressCollection.Add(carrierDataObject);
			orderDataObject.OrganizationAddressCollection.Add(pickupAddressDataObject);
			orderDataObject.OrganizationAddressCollection.Add(deliverAddressDataObject);
			orderDataObject.OrganizationAddressCollection.Add(sendingAgentDataObject);
			orderDataObject.OrganizationAddressCollection.Add(receivingAgentDataObject);
			orderDataObject.OrganizationAddressCollection.Add(controllingPartyDataObject);
			orderDataObject.OrganizationAddressCollection.Add(warehouseAddressDataObject);
			orderDataObject.OrganizationAddressCollection.Add(notifyPartyAddressDataObject);
			orderDataObject.OrganizationAddressCollection.Add(notifyParty2AddressDataObject);
			orderDataObject.OrganizationAddressCollection.Add(notifyParty3AddressDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_CRAHOLSYD(order.Buyer.MainAddress);
				AssertAddressContentMatches_CRAHOLSYD(order.Carrier.MainAddress);
				AssertEquals("order.GoodsAvailableAtAddress.E2_Address1", "17 Trogo Street", order.GoodsAvailableAtAddress.E2_Address1);
				AssertEquals("order.GoodsAvailableAtAddress.E2_CompanyName", "In The Moment", order.GoodsAvailableAtAddress.E2_CompanyName);
				AssertAddressContentMatches_CRAHOLSYD(order.ReceivingAgent.MainAddress);
				AssertAddressContentMatches_INTHEMSYD(order.Supplier.MainAddress);
				AssertEquals("order.GoodsDeliveredToAddress.E2_Address1", "124 Polo Street", order.GoodsDeliveredToAddress.E2_Address1);
				AssertEquals("order.GoodsDeliveredToAddress.E2_CompanyName", "CRACKERJACK HOLDINGS", order.GoodsDeliveredToAddress.E2_CompanyName);
				AssertAddressContentMatches_INTHEMSYD(order.SendingAgent.MainAddress);
				AssertEquals("order.ControllingPartyDocAddress.E2_Address1", "123 Moment Street", order.ControllingCustomerDocAddress.E2_Address1);
				AssertEquals("order.ControllingPartyDocAddress.E2_CompanyName", "Moment Inc.", order.ControllingCustomerDocAddress.E2_CompanyName);
				AssertEquals("order.ControllingPartyDocAddress.E2_RN_NKCountryCode", "AU", order.ControllingCustomerDocAddress.E2_RN_NKCountryCode);
				AssertJobDocAddressContentMatches_WUFSHIJNB(order.WarehouseDocAddress);
				AssertJobDocAddressContentMatches_WUFSHIJNB(order.NotifyPartyDocAddress);

				AssertEquals("order.ControllingPartyDocAddress.E2_Address1", "123 SGSIN Test Street", order.NotifyParty2DocAddress.E2_Address1);
				AssertEquals("order.ControllingPartyDocAddress.E2_CompanyName", "SGSIN Test Inc.", order.NotifyParty2DocAddress.E2_CompanyName);
				AssertEquals("order.ControllingPartyDocAddress.E2_RN_NKCountryCode", "SG", order.NotifyParty2DocAddress.E2_RN_NKCountryCode);

				AssertEquals("order.ControllingPartyDocAddress.E2_Address1", "123 CNSZX Test Street", order.NotifyParty3DocAddress.E2_Address1);
				AssertEquals("order.ControllingPartyDocAddress.E2_CompanyName", "CNSZX Test Inc.", order.NotifyParty3DocAddress.E2_CompanyName);
				AssertEquals("order.ControllingPartyDocAddress.E2_RN_NKCountryCode", "CN", order.NotifyParty3DocAddress.E2_RN_NKCountryCode);

				AssertEquals(false, Logger.HasErrors);
			});
		}

		public void TestOrganisations_BuyerWithoutConsignee()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.BuyerDocumentaryAddress);
			new OrganisationDataObjectReader(buyerDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);

			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertNull(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertNull(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});
		}

		public void TestOrganisations_BuyerWithConsignee()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.BuyerDocumentaryAddress);
			new OrganisationDataObjectReader(buyerDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var consigneeDataObject = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeDocumentaryAddress));

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(consigneeDataObject);

			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertJobDocAddressContentMatches_WUFSHIJNB(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertNull(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});
		}

		public void TestOrganisations_ConsigneeWithNoBuyer()
		{
			var consigneeDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			new OrganisationDataObjectReader(consigneeDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(consigneeDataObject);

			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertNull(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var orderBO = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

				AssertAddressContentMatches_CRAHOLSYD(orderBO.BuyerAddress);
				AssertNull(orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
			});
		}

		public void TestControllingCustomer()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			new OrganisationDataObjectReader(buyerDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var controllingCustomerDataObject = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ControllingCustomer));
			var legacyControllingPartyDataObject = GetNewAddressData_INTHEMSYD(LegacyUniversalAddressTypes.LegacyOrderControllingPartyAddressType);

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(controllingCustomerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(legacyControllingPartyDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			var jobDocAddress = orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
			AssertJobDocAddressContentMatches_WUFSHIJNB(jobDocAddress);
			AssertEquals("E2_AddressType", "SCP", jobDocAddress.E2_AddressType);
		}

		public void TestLegacyControllingParty()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			new OrganisationDataObjectReader(buyerDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var legacyControllingPartyDataObject = GetNewAddressData_INTHEMSYD(LegacyUniversalAddressTypes.LegacyOrderControllingPartyAddressType);

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(legacyControllingPartyDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			var jobDocAddress = orderBO.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddress);
			AssertEquals("E2_AddressType", "SCP", jobDocAddress.E2_AddressType);
		}

		public void TestManufacturer()
		{
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			new OrganisationDataObjectReader(buyerDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			var manufacturerDataObject = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.Manufacturer));
			var legacyControllingPartyDataObject = GetNewAddressData_INTHEMSYD(LegacyUniversalAddressTypes.LegacyOrderControllingPartyAddressType);

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(manufacturerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(legacyControllingPartyDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			var jobDocAddress = orderBO.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
			AssertJobDocAddressContentMatches_WUFSHIJNB(jobDocAddress);
			AssertEquals("E2_AddressType", "MAN", jobDocAddress.E2_AddressType);
		}

		public void TestDeliveryAddressFallbackToBuyerAddress()
		{
			var testLogger = new TestErrorLogger();
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var supplierDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var mainAddress1 = new OrganisationDataObjectReader(buyerDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress1 = mainAddress1.Header.Addresses.AddNew();
			newAddress1.OA_Address1 = "124 Polo Street";
			newAddress1.OA_Code = "Polo1";

			var mainAddress2 = new OrganisationDataObjectReader(supplierDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress2 = mainAddress2.Header.Addresses.AddNew();
			newAddress2.OA_Address1 = "17 Trogo Street";
			newAddress2.OA_Code = "FanTrog";

			Factory.SaveForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(supplierDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_CRAHOLSYD(order.Buyer.MainAddress);
				AssertAddressContentMatches_CRAHOLSYD(order.GoodsDeliveredToAddress.Address);
			});

			AssertEquals(false, Logger.HasErrors);
		}

		public void TestPickupAddressFallbackToSupplierAddress()
		{
			var testLogger = new TestErrorLogger();
			var buyerDataObject = GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var supplierDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var mainAddress1 = new OrganisationDataObjectReader(buyerDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress1 = mainAddress1.Header.Addresses.AddNew();
			newAddress1.OA_Address1 = "124 Polo Street";
			newAddress1.OA_Code = "Polo1";

			var mainAddress2 = new OrganisationDataObjectReader(supplierDataObject, testLogger, Factory).GetMatchedOrNewForTesting();
			var newAddress2 = mainAddress2.Header.Addresses.AddNew();
			newAddress2.OA_Address1 = "17 Trogo Street";
			newAddress2.OA_Code = "FanTrog";

			Factory.SaveForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(buyerDataObject);
			orderDataObject.OrganizationAddressCollection.Add(supplierDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(order.Supplier.MainAddress);
				AssertAddressContentMatches_INTHEMSYD(order.GoodsAvailableAtAddress.Address);
			});

			AssertEquals(false, Logger.HasErrors);
		}

		#endregion

		#region TestWithNotes

		public void TestWithNotes()
		{
			var orderDataObject = GetNewOrderDataObject();
			var noteDataObject = new Note();
			noteDataObject.Description = "DOG FLOGGER!!";
			noteDataObject.Visibility = new CodeDescriptionPair() { Code = nameof(CargoWise.Definitions.StmNoteVisibility.PUB), Description = "Public" };
			noteDataObject.NoteContext = new NoteContext() { Code = "BEB", Description = "Baby Eats Banana" };
			noteDataObject.IsCustomDescription = false;
			noteDataObject.NoteText = "111";

			orderDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			orderDataObject.NoteCollection.Add(noteDataObject);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);

			var note = orderBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "DOG FLOGGER!!", note[0].ST_Description);
				AssertEquals("noteBO.ST_NoteContext", "BEB", note[0].ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "PUB", note[0].ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertEquals("noteBO.ST_NoteDataAsText", "111", note[0].ST_NoteDataAsText);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(true, Logger.HasWarnings);
				AssertEquals("Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.", Logger.GetWarnings());
			});
		}

		public void TestWithNotes_IgnoreUpdateHistoryNote()
		{
			var orderDataObject = GetNewOrderDataObject();

			var orderManagementUpdateNoteDO = new Note();
			orderManagementUpdateNoteDO.Description = "Order Management Update";
			orderManagementUpdateNoteDO.IsCustomDescription = false;
			orderManagementUpdateNoteDO.NoteText = "111";

			var orderUpdateHistoryNoteDO = new Note();
			orderUpdateHistoryNoteDO.Description = "Order Update History";
			orderUpdateHistoryNoteDO.IsCustomDescription = false;
			orderUpdateHistoryNoteDO.NoteText = "222";

			orderDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			orderDataObject.NoteCollection.Add(orderManagementUpdateNoteDO);
			orderDataObject.NoteCollection.Add(orderUpdateHistoryNoteDO);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull(orderBO);
			AssertEquals("orderBO.Notes.GetAllNotes()", 2, orderBO.Notes.GetAllNotes().Count);

			var orderManagementUpdateNoteBO = orderBO.Notes.FindByDescription("Order Management Update").First();
			AssertNotNull("Note Collection contains 'Order Management Update' note.", orderManagementUpdateNoteBO);
			Assert("orderManagementUpdateNoteBO.IsInDatabase", orderManagementUpdateNoteBO.IsInDatabase);

			var orderUpdateHistoryNoteBO = orderBO.Notes.FindByDescription("Order Update History").First();
			AssertNotNull("Note Collection contains 'Order Update History.", orderUpdateHistoryNoteBO);
			Assert("orderUpdateHistoryNoteBO.IsInDatabase", !orderUpdateHistoryNoteBO.IsInDatabase);
			Assert(!Logger.HasWarnings);
		}

		public void TestWithNotes_IgnoreNoteWithEmptyText()
		{
			var orderDataObject = GetNewOrderDataObject();

			var orderManagementUpdateNote = new Note();
			orderManagementUpdateNote.Description = "Order Management Update";
			orderManagementUpdateNote.IsCustomDescription = false;
			orderManagementUpdateNote.NoteText = "111";

			var orderCustomNote1 = new Note();
			orderCustomNote1.Description = "Order Custom Note 1";
			orderCustomNote1.IsCustomDescription = true;
			orderCustomNote1.NoteText = ZString.Empty;

			var orderCustomNote2 = new Note();
			orderCustomNote2.Description = "Order Custom Note 2";
			orderCustomNote2.IsCustomDescription = true;
			orderCustomNote2.NoteText = "       ";

			var orderCustomNote3 = new Note();
			orderCustomNote3.Description = "Order Custom Note 3";
			orderCustomNote3.IsCustomDescription = true;
			orderCustomNote3.NoteText = "333";

			var orderCustomNote4 = new Note();
			orderCustomNote4.Description = "Order Custom Note 4";
			orderCustomNote4.IsCustomDescription = true;

			orderDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			orderDataObject.NoteCollection.Add(orderManagementUpdateNote);
			orderDataObject.NoteCollection.Add(orderCustomNote1);
			orderDataObject.NoteCollection.Add(orderCustomNote2);
			orderDataObject.NoteCollection.Add(orderCustomNote3);
			orderDataObject.NoteCollection.Add(orderCustomNote4);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull(orderBO);
			AssertEquals("orderBO.Notes.GetAllNotes()", 3, orderBO.Notes.GetAllNotes().Count);

			var orderManagementUpdateNoteBO = orderBO.Notes.FindByDescription("Order Management Update").FirstOrDefault();
			AssertNotNull("Note Collection contains 'Order Management Update' note.", orderManagementUpdateNoteBO);
			Assert("orderManagementUpdateNoteBO.IsInDatabase", orderManagementUpdateNoteBO.IsInDatabase);

			var orderUpdateHistoryNoteBO = orderBO.Notes.FindByDescription("Order Update History").FirstOrDefault();
			AssertNotNull("Note Collection contains 'Order Update History'.", orderUpdateHistoryNoteBO);
			Assert("orderUpdateHistoryNoteBO.IsInDatabase should be false", !orderUpdateHistoryNoteBO.IsInDatabase);

			var orderCustomNote1BO = orderBO.Notes.FindByDescription("Order Custom Note 1").FirstOrDefault();
			AssertNull("Note Collection should not contain 'Order Custom Note 1'.", orderCustomNote1BO);

			var orderCustomNote2BO = orderBO.Notes.FindByDescription("Order Custom Note 2").FirstOrDefault();
			AssertNull("Note Collection should not 'Order Custom Note 2'.", orderCustomNote2BO);

			var orderCustomNote3BO = orderBO.Notes.FindByDescription("Order Custom Note 3").FirstOrDefault();
			AssertNotNull("Note Collection contains 'Order Custom Note 3'.", orderCustomNote3BO);
			Assert("orderCustomNote3BO.IsInDatabase", orderCustomNote3BO.IsInDatabase);

			var orderCustomNote4BO = orderBO.Notes.FindByDescription("Order Custom Note 4").FirstOrDefault();
			AssertNull("Note Collection should not 'Order Custom Note 4'.", orderCustomNote4BO);

			Assert(Logger.HasWarnings);
			AssertEquals(@"Cannot import the following notes with empty text:
Order Custom Note 1,Order Custom Note 2,Order Custom Note 4", Logger.GetWarnings());
		}

		public void TestWithNotes_WithCompleteContent()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";

			var note1 = order1.Notes.AddNew();
			note1.ST_Description = "Custom Note";
			note1.ST_IsCustomDescription = true;

			var note2 = order1.Notes.AddNew();
			note2.ST_Description = "Agent Note";
			note2.ST_IsCustomDescription = false;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORD0005" };

			var orderManagementUpdateNoteDO = new Note();
			orderManagementUpdateNoteDO.Description = "Order Management Update";
			orderManagementUpdateNoteDO.IsCustomDescription = false;
			orderManagementUpdateNoteDO.NoteText = "333";

			var orderUpdateHistoryNoteDO = new Note();
			orderUpdateHistoryNoteDO.Description = "Order Update History";
			orderUpdateHistoryNoteDO.IsCustomDescription = false;
			orderUpdateHistoryNoteDO.NoteText = "444";

			var orderCustomNote1 = new Note();
			orderCustomNote1.Description = "Order Custom Note 1";
			orderCustomNote1.IsCustomDescription = true;
			orderCustomNote1.NoteText = ZString.Empty;

			var orderCustomNote2 = new Note();
			orderCustomNote2.Description = "Order Custom Note 2";
			orderCustomNote2.IsCustomDescription = true;
			orderCustomNote2.NoteText = "       ";

			var orderCustomNote3 = new Note();
			orderCustomNote3.Description = "Order Custom Note 3";
			orderCustomNote3.IsCustomDescription = true;

			orderDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			orderDataObject.NoteCollection.Content = CollectionContent.Complete;
			orderDataObject.NoteCollection.Add(orderManagementUpdateNoteDO);
			orderDataObject.NoteCollection.Add(orderUpdateHistoryNoteDO);
			orderDataObject.NoteCollection.Add(orderCustomNote1);
			orderDataObject.NoteCollection.Add(orderCustomNote2);
			orderDataObject.NoteCollection.Add(orderCustomNote3);

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull(orderBO);
			AssertEquals("orderBO.Notes.GetAllNotes()", 2, orderBO.Notes.GetAllNotes().Count);

			var orderManagementUpdateNoteBO = orderBO.Notes.FindByDescription("Order Management Update").FirstOrDefault();
			AssertNotNull("Note Collection contains 'Order Management Update'.", orderManagementUpdateNoteBO);
			Assert("orderManagementUpdateNoteBO.IsInDatabase should be true", orderManagementUpdateNoteBO.IsInDatabase);
			AssertEquals("ST_NoteText should be updated", "333", orderManagementUpdateNoteBO.ST_NoteText);

			var orderUpdateHistoryNoteBO = orderBO.Notes.FindByDescription("Order Update History").FirstOrDefault();
			AssertNotNull("Note Collection contains 'Order Update History'.", orderUpdateHistoryNoteBO);
			Assert("orderUpdateHistoryNoteBO.IsInDatabase should be false", !orderUpdateHistoryNoteBO.IsInDatabase);

			var customNote = orderBO.Notes.FindByDescription("Custom Note").FirstOrDefault();
			AssertNull("Note Collection shouldn't contain 'Custom Note'.", customNote);

			var agentNote = orderBO.Notes.FindByDescription("Agent Note").FirstOrDefault();
			AssertNull("Note Collection shouldn't contain 'Agent Note'.", agentNote);

			var orderCustomNote1BO = orderBO.Notes.FindByDescription("Order Custom Note 1").FirstOrDefault();
			AssertNull("Note Collection shouldn't contain 'Order Custom Note 1'.", orderCustomNote1BO);

			var orderCustomNote2BO = orderBO.Notes.FindByDescription("Order Custom Note 2").FirstOrDefault();
			AssertNull("Note Collection shouldn't contain 'Order Custom Note 2'.", orderCustomNote2BO);

			var orderCustomNote3BO = orderBO.Notes.FindByDescription("Order Custom Note 3").FirstOrDefault();
			AssertNull("Note Collection shouldn't contain 'Order Custom Note 3'.", orderCustomNote3BO);

			Assert(Logger.HasWarnings);
			AssertEquals(@"Cannot import the following notes with empty text:
Order Custom Note 1,Order Custom Note 2,Order Custom Note 3", Logger.GetWarnings());
		}

		public void TestWithNotes_RemoveAllNotes()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;

			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "ORD0005";

			var note1 = order1.Notes.AddNew();
			note1.ST_Description = "Custom Note";
			note1.ST_IsCustomDescription = true;

			var note2 = order1.Notes.AddNew();
			note2.ST_Description = "Agent Note";
			note2.ST_IsCustomDescription = false;

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder { OrderNumber = "ORD0005" };

			orderDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			orderDataObject.NoteCollection.Content = CollectionContent.Complete;

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			AssertNotNull(orderBO);
			AssertEquals("orderBO.Notes.GetAllNotes()", 0, orderBO.Notes.GetAllNotes().Count);
			Assert(!Logger.HasWarnings);
		}

		#endregion

		#region TestWithContainers

		public void TestWithContainers()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			orderDataObject.ContainerCollection.Add(OrderContainerDataObjectReaderTest.GetNewContainerDataObject());

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(1, order.PlannedContainers.Count);

			CombineAssertions(delegate
			{
				var container = order.PlannedContainers[0];
				OrderContainerDataObjectReaderTest.AssertContents(container);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		#endregion

		#region TestWithOrderLines

		public void TestWithOrderLines()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(OrderLineDataObjectReaderTest.GetNewOrderLineDataObject());

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var order = reader.ReadIntoBusinessObject();

			AssertNotNull(order);
			AssertEquals(1, order.OrderLines.Count);

			CombineAssertions(delegate
			{
				var orderLine = order.OrderLines[0];
				OrderLineDataObjectReaderTest.AssertContents(orderLine);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		public void TestAddNewOrderLineWithNoException()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ACCGRESHA";
			buyer.AddCustomLabel(Constants.CustomLabels.Order.UserTrackDate1, Constants.CustomLabels.Order.UserTrackDate1);

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = buyer.PK;
			orderBO.JD_OrderNumber = "P000004";
			orderBO.JD_OrderNumberSplit = 1;

			Factory.SaveForTesting();

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);

			#region XML Message

			var orderMsg =
$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OrderManagerOrder</Type>
          <Key>P000004~0~ACCGRESHA</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Order>
      <OrderNumber>ORD2</OrderNumber>
      <ClientReference></ClientReference>
      <OrderNumberSplit>0</OrderNumberSplit>
      <Status>
        <Code>PLC</Code>
        <Description>Placed</Description>
      </Status>
      <OrderLineCollection Content=""Complete"">
        <OrderLine>
          <AdditionalInformation></AdditionalInformation>
          <AdditionalTerms></AdditionalTerms>
          <CommercialInvoiceNumber></CommercialInvoiceNumber>
          <ConfirmationNumber></ConfirmationNumber>
          <ContainerNumber></ContainerNumber>
          <ContainerPackingOrder>0</ContainerPackingOrder>
          <CustomsData>
          </CustomsData>
          <EarlyShipmentLimitDays>0</EarlyShipmentLimitDays>
          <ExpectedQuantity>0.00000</ExpectedQuantity>
          <ExtendedLinePrice>10000.0000</ExtendedLinePrice>
          <IncoTerm>
            <Code></Code>
          </IncoTerm>
          <InnerPacksQty>100.000</InnerPacksQty>
          <InnerPacksQtyUnit>
            <Code>BAG</Code>
            <Description>Bag</Description>
          </InnerPacksQtyUnit>
          <LateShipmentLimitDays>0</LateShipmentLimitDays>
          <LineNumber>1</LineNumber>
          <LineReference>coat123</LineReference>
          <LineSplitNumber>0</LineSplitNumber>
          <OrderedQty>-10.00000</OrderedQty>
          <OrderedQtyUnit>
            <Code>CTN</Code>
            <Description>Carton</Description>
          </OrderedQtyUnit>
          <OverQuantityPercentageLimit>0.000</OverQuantityPercentageLimit>
          <PackageHeight>0.000</PackageHeight>
          <PackageLength>0.000</PackageLength>
          <PackageLengthUnit>
            <Code></Code>
            <Description></Description>
          </PackageLengthUnit>
          <PackageQty>1000.000</PackageQty>
          <PackageQtyUnit>
            <Code>PLT</Code>
            <Description>Pallet</Description>
          </PackageQtyUnit>
          <PackageWidth>0.000</PackageWidth>
          <PartAttribute1></PartAttribute1>
          <PartAttribute2></PartAttribute2>
          <PartAttribute3></PartAttribute3>
          <Product>
            <Code>ORD2-1</Code>
            <Description>blue coat</Description>
          </Product>
          <QtyBooked>4000.00000</QtyBooked>
          <QtyPacked>0.00000</QtyPacked>
          <QuantityMet>0.00000</QuantityMet>
          <RequiredExWorks></RequiredExWorks>
          <RequiredInStore></RequiredInStore>
          <SerialNumber></SerialNumber>
          <ShipmentWindowEnd></ShipmentWindowEnd>
          <ShipmentWindowStart></ShipmentWindowStart>
          <SpecialInstructions></SpecialInstructions>
          <Status>
            <Code>PLC</Code>
            <Description>Placed</Description>
          </Status>
          <SubLineNumber>1</SubLineNumber>
          <SupplierConfirmedAcceptance></SupplierConfirmedAcceptance>
          <UnderQuantityPercentageLimit>0.000</UnderQuantityPercentageLimit>
          <UnitPriceRecommended>1.0000</UnitPriceRecommended>
          <Volume>10.000</Volume>
          <VolumeUnit>
            <Code>M3</Code>
            <Description>Cubic Metres</Description>
          </VolumeUnit>
          <Weight>1000.000</Weight>
          <WeightUnit>
            <Code>KG</Code>
            <Description>Kilograms</Description>
          </WeightUnit>
        </OrderLine>
      </OrderLineCollection>
    </Order>
  </Shipment>
</UniversalShipment>
";

			#endregion

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(orderMsg)))
			{
				var logger = new TestErrorLogger();
				ObjectFactory.Get<IXmlReader>().ReadXML(orderData, stream, logger);
				Assert(!logger.HasWarnings);
			}

			var readerLogger = new TestErrorLogger();
			var reader = new OrderDataObjectReader(orderData, readerLogger, Factory);

			AssertNoExceptionThrown(() =>
			{
				reader.PopulateBO(orderBO, false);
			});

			AssertEquals(1, orderBO.OrderLines.Count);
		}

		#endregion

		#region Transport Legs

		public void TestOneLeg()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.ContainerMode = new ContainerMode { Code = "LSE" };
			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			orderDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(1), VesselName = "BELINGA", VoyageFlightNo = "BL123", EstimatedDeparture = new ZDateTime(2012, 12, 31), EstimatedArrival = new ZDateTime(2013, 1, 1) });

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			CombineAssertions(delegate
			{
				AssertEquals("orderBO.JD_TransportMode", "AIR", orderBO.JD_TransportMode);
				AssertEquals("orderBO.JD_ContainerMode", "LSE", orderBO.JD_ContainerMode);
				AssertEquals("orderBO.JD_DepartureVoyage", "BL123", orderBO.JD_DepartureVoyage);
				AssertEquals("orderBO.JD_ArrivalVoyage", "BL123", orderBO.JD_ArrivalVoyage);
				AssertEquals("orderBO.JD_Milestone_E_DEP", new ZDateTime(2012, 12, 31), orderBO.JD_Milestone_E_DEP);
				AssertEquals("orderBO.JD_Milestone_E_ARV", new ZDateTime(2013, 1, 1), orderBO.JD_Milestone_E_ARV);
				AssertEquals("orderBO.JD_RV_NKDepartureVessel", "BELINGA", orderBO.JD_RV_NKDepartureVessel);
				AssertEquals("orderBO.JD_RV_NKArrivalVessel", "BELINGA", orderBO.JD_RV_NKArrivalVessel);

				AssertEquals("orderBO.JD_E_ARV_1stIntermediate", ZDateTime.Empty, orderBO.JD_E_ARV_1stIntermediate);
				AssertEquals("orderBO.JD_E_DEP_3", ZDateTime.Empty, orderBO.JD_E_DEP_3);

				AssertEquals("orderBO.JD_IntermediateVoyage", ZString.Empty, orderBO.JD_IntermediateVoyage);
				AssertEquals("orderBO.JD_RV_NKIntermediateVessel", ZString.Empty, orderBO.JD_RV_NKIntermediateVessel);
				AssertEquals("orderBO.JD_E_DEP_2", ZDateTime.Empty, orderBO.JD_E_DEP_2);
				AssertEquals("orderBO.JD_E_ARV_2ndIntermediate", ZDateTime.Empty, orderBO.JD_E_ARV_2ndIntermediate);

				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		public void TestTwoLegs()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.ContainerMode = new ContainerMode { Code = "LSE" };
			orderDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			orderDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(1), VesselName = "BELINGA", VoyageFlightNo = "BL123", EstimatedDeparture = new ZDateTime(2012, 12, 31), EstimatedArrival = new ZDateTime(2013, 1, 1) });
			orderDataObject.TransportLegCollection.Add(new TransportLeg { LegOrder = new ZByte(2), VesselName = "WUNDAGO", VoyageFlightNo = "WN321", EstimatedDeparture = new ZDateTime(2013, 1, 4), EstimatedArrival = new ZDateTime(2013, 1, 5) });

			var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
			var orderBO = reader.ReadIntoBusinessObject();

			AssertNotNull(orderBO);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			CombineAssertions(delegate
			{
				AssertEquals("orderBO.JD_TransportMode", "AIR", orderBO.JD_TransportMode);
				AssertEquals("orderBO.JD_ContainerMode", "LSE", orderBO.JD_ContainerMode);
				AssertEquals("orderBO.JD_DepartureVoyage", "BL123", orderBO.JD_DepartureVoyage);
				AssertEquals("orderBO.JD_ArrivalVoyage", "WN321", orderBO.JD_ArrivalVoyage);
				AssertEquals("orderBO.JD_Milestone_E_DEP", new ZDateTime(2012, 12, 31), orderBO.JD_Milestone_E_DEP);
				AssertEquals("orderBO.JD_E_ARV_1stIntermediate", new ZDateTime(2013, 1, 1), orderBO.JD_E_ARV_1stIntermediate);
				AssertEquals("orderBO.JD_E_DEP_3", new ZDateTime(2013, 1, 4), orderBO.JD_E_DEP_3);
				AssertEquals("orderBO.JD_Milestone_E_ARV", new ZDateTime(2013, 1, 5), orderBO.JD_Milestone_E_ARV);
				AssertEquals("orderBO.JD_RV_NKDepartureVessel", "BELINGA", orderBO.JD_RV_NKDepartureVessel);
				AssertEquals("orderBO.JD_RV_NKArrivalVessel", "WUNDAGO", orderBO.JD_RV_NKArrivalVessel);

				AssertEquals("orderBO.JD_IntermediateVoyage", ZString.Empty, orderBO.JD_IntermediateVoyage);
				AssertEquals("orderBO.JD_RV_NKIntermediateVessel", ZString.Empty, orderBO.JD_RV_NKIntermediateVessel);
				AssertEquals("orderBO.JD_E_DEP_2", ZDateTime.Empty, orderBO.JD_E_DEP_2);
				AssertEquals("orderBO.JD_E_ARV_2ndIntermediate", ZDateTime.Empty, orderBO.JD_E_ARV_2ndIntermediate);

				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		#endregion

		#endregion

		#region TestWarningLogs

		public void TestLogging_PackTypeValues()
		{
			TestLogging(
				(orderDataObject) =>
				{
					orderDataObject.OuterPacks = 10;
					orderDataObject.OuterPacksPackageType = new PackageType { Code = string.Empty };
				},
				"Order Pack Type is required when Packs is greater than zero.");
		}

		public void TestLogging_WeightValues()
		{
			TestLogging(
				(orderDataObject) =>
				{
					orderDataObject.TotalWeight = 10;
					orderDataObject.TotalWeightUnit = new UnitOfWeight { Code = string.Empty };
				},
				"Order Weight Unit is required when Actual Weight is greater than zero.");
		}

		public void TestLogging_VolumeValues()
		{
			TestLogging(
				(orderDataObject) =>
				{
					orderDataObject.TotalVolume = 10;
					orderDataObject.TotalVolumeUnit = new UnitOfVolume { Code = string.Empty };
				},
				"Order Volume Unit is required when Actual Volume is greater than zero.");
		}

		public void TestLogging_DateValues()
		{
			TestLogging(
				(orderDataObject) =>
				{
					orderDataObject.SetDateCollection(() => new List<Date>());
					orderDataObject.DateCollection.Add(DateType.ExWorksRequiredBy, false, new ZDateTime(2012, 1, 6));
					orderDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2012, 1, 1) };
				},
				"Order Ex Works Date should be before the Required In Store Date.");
		}

		public void TestLogging_CurrencyValue()
		{
			var orderLineDataObject = OrderLineDataObjectReaderTest.GetNewOrderLineDataObject();
			orderLineDataObject.UnitPriceRecommended = 15.0;

			TestLogging(
				(orderDataObject) =>
				{
					orderDataObject.FreightRateCurrency = new Currency { Code = string.Empty };
					orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance);
					orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
					orderDataObject.Order.OrderLineCollection.Add(orderLineDataObject);
				},
				"Order Currency is required when an attached Order Line has an Item Price set.");
		}

		void TestLogging(Action<UniversalShipment> modification, string expectedMessage)
		{
			var orderDataObject = GetNewOrderDataObject();
			modification(orderDataObject);

			new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
			Assert("Expecting Logger to have warnings", Logger.HasWarnings);

			var warnings = Logger.GetWarnings().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertEquals("Warning Count: ", 1, warnings.Length);
			AssertEquals("Warning Message: ", expectedMessage, warnings[0]);
		}

		#endregion

		#region Implementation

		#region GetNewOrderDataObject

		UniversalShipment GetNewOrderDataObject()
		{
			Data.CreateConsigneeAddressCRAHOLSYDInDB();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(Data.ConsigneeAddressCRAHOLSYDDataObject);
			return orderDataObject;
		}

		#endregion

		#region Data

		UniversalTestData Data
		{
			get { return data ?? (data = new UniversalTestData(Factory)); }
		}

		UniversalTestData data;

		#endregion

		#endregion

		#region Test GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORD0001";
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0001" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = null, OrderedQty = 3 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 0, OrderedQty = 18 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = -1, OrderedQty = 20 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, OrderedQty = 40 });

			Logger.ClearLogs();
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertEquals(true, Logger.HasErrors);
				AssertContains("Cannot populate OrderLine because:\r\nThe line number must be greater than 0.", Logger.GetErrors());

				AssertEquals("The line number must be greater than 0.", 1, order.OrderLines.Count);
				AssertEquals("Import order lines with line number greater than 0.", 40m, order.OrderLines.First(orderLine => orderLine.JO_LineNo == 1).JO_Quantity);
			}
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_DuplicatedLineReference()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORD0001";

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_Quantity = 40;
			orderLine1.JO_LineReference = "LR0001";

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;
			orderLine2.JO_Quantity = 60;
			orderLine2.JO_LineReference = "LR0002";

			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0001" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>() { Content = CollectionContent.Partial });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 3, OrderedQty = 80, LineReference = "LR0001" });

			Logger.ClearLogs();
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertEquals(true, Logger.HasErrors);
				AssertContains("Cannot populate OrderLine because:\r\nNew order line cannot be created because Line Reference LR0001 already exists on another order line.", Logger.GetErrors());
			}

			Logger.ClearLogs();
			orderDataObject.Order.OrderLineCollection.Clear();
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 2, OrderedQty = 80, LineReference = "LR0001" });
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertEquals(true, Logger.HasErrors);
				AssertContains("Cannot populate OrderLine because:\r\nMatching order line's (order line 2 and sub-line 1) Line Reference cannot be updated to LR0001 because LR0001 already exists on another order line.", Logger.GetErrors());
			}
		}

		#endregion

		public void TestCreatingNewObject_WhenLineReferenceNotProvidedShouldSetDefaultSubLineNo()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORD0001";
			Factory.SaveForTesting();

			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0001" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = null, OrderedQty = 20, LineReference = "" });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { LineNumber = 1, OrderedQty = 60, LineReference = "LR002" });

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderDataObjectReader(orderDataObject, Logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertEquals(false, Logger.HasErrors);

				var twentyOrderLine = order.OrderLines.First(orderLine => orderLine.JO_LineNo == 1);
				var sixtyOrderLine = order.OrderLines.First(orderLine => orderLine.JO_LineNo == 2);

				AssertEquals(20m, twentyOrderLine.JO_Quantity);
				AssertEquals(1, twentyOrderLine.JO_LineNo);
				AssertEquals("", twentyOrderLine.JO_LineReference);

				AssertEquals(60m, sixtyOrderLine.JO_Quantity);
				AssertEquals(2, sixtyOrderLine.JO_LineNo);
				AssertEquals("LR002", sixtyOrderLine.JO_LineReference);
			}
		}

		public void TestDuplicateJO_JD_JO_LineNo_JO_SubLineNo()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0005" };
			orderDataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>());
			orderDataObject.Order.OrderLineCollection.Content = CollectionContent.Partial;
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { SubLineNumber = 2 });
			orderDataObject.Order.OrderLineCollection.Add(new UniversalOrderLine { SubLineNumber = 2 });
			var order = new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			AssertEquals(1, order.OrderLines[0].JO_LineNo);
			AssertEquals(2, order.OrderLines[1].JO_LineNo);
		}

		public void TestImportOrderCustomFieldTruncatedWarning()
		{
			var invalidString = new ZString('a', 61);
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTORGBUYER";
			buyer.AddCustomLabel(Constants.CustomLabels.Order.UserTrackDate1, Constants.CustomLabels.Order.UserTrackDate1);

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = buyer.PK;
			orderBO.JD_OrderNumber = "111111111122222222223333333333XXXXX";
			orderBO.JD_OrderNumberSplit = 1;

			Factory.SaveForTesting();

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);

			#region XML Message

			var orderMsg =
$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OrderManagerOrder</Type>
          <Key>111111111122222222223333333333XXXXX~1~TESTORGBUYER</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Estimated OrderHeader.UserTrackDate1</Key>
        <Value>2024-03-22T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>{invalidString}</Key>
        <Value>newValue</Value>
      </CustomizedField>
    </CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>
";
			#endregion

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(orderMsg)))
			{
				var logger = new TestErrorLogger();
				ObjectFactory.Get<IXmlReader>().ReadXML(orderData, stream, logger);
				Assert(!logger.HasWarnings);
			}

			var readerLogger = new TestErrorLogger();
			var reader = new OrderDataObjectReader(orderData, readerLogger, Factory);
			reader.PopulateBO(orderBO, false);
			Assert(readerLogger.HasWarnings);
			AssertContains($"Warning - Attempted to insert 61 characters into Field [{invalidString}] which has a maximum length of 60 characters. Field was truncated.", readerLogger.Logs);
		}

		public void TestEmptyImportOrderCustomLabelNoTruncatedWarning()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "TESTORGBUYER";
			buyer.AddCustomLabel(Constants.CustomLabels.Order.UserTrackDate1, Constants.CustomLabels.Order.UserTrackDate1);

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.BuyerPK = buyer.PK;
			orderBO.JD_OrderNumber = "111111111122222222223333333333XXXXX";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.JD_EstimateUserDate1 = DateTime.Now;

			Factory.SaveForTesting();

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);

			#region XML Message

			var orderMsg =
$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OrderManagerOrder</Type>
          <Key>111111111122222222223333333333XXXXX~1~TESTORGBUYER</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Estimated OrderHeader.UserTrackDate1</Key>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>
";
			#endregion

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(orderMsg)))
			{
				var logger = new TestErrorLogger();
				ObjectFactory.Get<IXmlReader>().ReadXML(orderData, stream, logger);
				Assert(!logger.HasWarnings);
			}

			var readerLogger = new TestErrorLogger();
			var reader = new OrderDataObjectReader(orderData, readerLogger, Factory);
			reader.PopulateBO(orderBO, false);

			Factory.SaveForTesting();

			orderBO.Reload();

			AssertEquals(ZDateTime.Empty, orderBO.JD_EstimateUserDate1);
			Assert(!readerLogger.HasWarnings);
		}

		#region ShipmentWindowEndTest

		public void TestLogging_ShipmentWindowEnd()
		{
			var orderDataObject = GetNewOrderDataObject();
			orderDataObject.SetDateCollection(() => new List<Date>());
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowStart, false, new ZDate(2012, 1, 6));
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowEnd, false, new ZDate(2011, 1, 6));

			new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

			Assert("Expecting Logger to have errors", Logger.HasErrors);

			var errors = Logger.GetErrors().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertEquals("Error Count: ", 1, errors.Length);
			AssertEquals("Error Message: ", "Order Ship Window Start date by must be earlier than or equal to Ship Window End date.", errors[0]);
		}

		public void TestNoLogging_No_ShipmentWindowStart_And_No_ShipmentWindowEnd()
		{
			var orderDataObject = GetNewOrderDataObject();

			new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		public void TestNoLogging_No_ShipmentWindowStart_But_Have_ShipmentWindowEnd()
		{
			var orderDataObject = GetNewOrderDataObject();

			orderDataObject.SetDateCollection(() => new List<Date>());
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowEnd, false, new ZDate(2011, 1, 6));

			new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		public void TestNoLogging_Have_ShipmentWindowStart_But_No_ShipmentWindowEnd()
		{
			var orderDataObject = GetNewOrderDataObject();

			orderDataObject.SetDateCollection(() => new List<Date>());
			orderDataObject.DateCollection.Add(DateType.ShipmentWindowStart, false, new ZDate(2011, 1, 6));

			new OrderDataObjectReader(orderDataObject, Logger, Factory).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		#endregion

	}
}
