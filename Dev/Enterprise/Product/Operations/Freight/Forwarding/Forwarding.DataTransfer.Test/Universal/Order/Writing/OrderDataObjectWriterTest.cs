using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class OrderDataObjectWriterTest : OrganizationAddressTestHelper
	{
		#region TestBasicFieldLevelMappings

		public void TestBasicFieldLevelMappings()
		{
			var orderBO = Factory.New<Order>();
			orderBO.JD_OrderNumber = "ORDER1";
			orderBO.JD_OrderNumberSplit = new ZByte(2);
			orderBO.JD_BookingConfRef = "Booking Reference";
			orderBO.JD_OrderStatus = "CNF"; // Confirmed
			orderBO.JD_IsReleased = true;
			orderBO.JD_InvoiceNumber = "4321";
			orderBO.JD_FirstBuyerContact = "Barney";
			orderBO.JD_SecondBuyerContact = "Fred";
			orderBO.JD_ExWorksRequiredBy = new ZDateTime(2012, 1, 1);
			orderBO.JD_OrderDate = new ZDateTime(2012, 1, 2);
			orderBO.JD_BookingConfDate = new ZDateTime(2012, 1, 3);
			orderBO.JD_InvoiceDate = new ZDateTime(2012, 1, 4);
			orderBO.JD_FollowUpDate = new ZDateTime(2012, 1, 5);
			orderBO.JD_DeliveryRequiredBy = new ZDateTime(2012, 1, 6);
			orderBO.JD_OrderGoodsDescription = "Guitars";
			orderBO.JD_RX_NKOrderCurrency = "NZD";
			orderBO.JD_EstimatedExchangeRate = 10.3m;
			orderBO.JD_RS_NKServiceLevel_NI = "STD";
			orderBO.JD_IncoTerm = "FCA";
			orderBO.JD_AdditionalTerms = "ADDMYONE";
			orderBO.JD_TransportMode = "AIR";
			orderBO.JD_ContainerMode = "LSE";
			orderBO.JD_RN_NKCountryOfSupply = "US";
			orderBO.JD_DepartureVesselCutoffDate = new ZDateTime(2012, 1, 7);
			orderBO.JD_Waybill = "ORDERHOUSE";
			orderBO.JD_MasterWaybill = "ORDERMASTER";
			orderBO.JD_RV_NKDepartureVessel = "BELINGA";
			orderBO.JD_DepartureVoyage = "BL123";
			orderBO.JD_RV_NKIntermediateVessel = "BUNDAGO";
			orderBO.JD_IntermediateVoyage = "BN321";
			orderBO.JD_RV_NKArrivalVessel = "WUNDAGO";
			orderBO.JD_ArrivalVoyage = "WN321";
			orderBO.JD_RL_NKGoodsAvailableAt = "AUMEL";
			orderBO.JD_RL_NKGoodsDeliveredTo = "NZAKL";
			orderBO.JD_RL_NKPortOfLoading = "AUBNE";
			orderBO.JD_RL_NKPortOfDischarge = "NZCHC";
			orderBO.JD_Packs = 2;
			orderBO.JD_F3_NKPackType = "PLT";
			orderBO.JD_ActualVolume = 53.2m;
			orderBO.JD_UnitOfVolume = "CF";
			orderBO.JD_ActualWeight = 32.4m;
			orderBO.JD_UnitOfWeight = "KT";
			orderBO.JD_E_ARV_1stIntermediate = new ZDateTime(2013, 1, 1);
			orderBO.JD_E_DEP_2 = new ZDateTime(2013, 1, 2);
			orderBO.JD_E_ARV_2ndIntermediate = new ZDateTime(2013, 1, 3);
			orderBO.JD_E_DEP_3 = new ZDateTime(2013, 1, 4);
			orderBO.JD_Milestone_A_EXW = new ZDateTime(2013, 1, 5); // Estimated Ex Works (Ex Factory) Milestone
			orderBO.JD_Milestone_E_EXW = new ZDateTime(2013, 1, 6); // Actual Ex Works (Ex Factory) Milestone
			orderBO.JD_Milestone_A_ARV = new ZDateTime(2013, 1, 7); // Actual Arrival Milestone
			orderBO.JD_Milestone_E_ARV = new ZDateTime(2013, 1, 8); // Estimated Arrival Milestone
			orderBO.JD_Milestone_A_CAV = new ZDateTime(2013, 1, 9); // Actual Cargo Available (Unpacked) Milestone
			orderBO.JD_Milestone_E_CAV = new ZDateTime(2013, 1, 10); // Estimated Cargo Available (Unpacked) Milestone
			orderBO.JD_Milestone_A_CCC = new ZDateTime(2013, 1, 11); // Actual Customs Commenced (Clearance Commenced) Milestone
			orderBO.JD_Milestone_E_CCC = new ZDateTime(2013, 1, 12); // Estimated Customs Commenced (Clearance Commenced) Milestone
			orderBO.JD_Milestone_A_CLR = new ZDateTime(2013, 1, 13); // Actual Customs Cleared (Clearance Finalized) Milestone
			orderBO.JD_Milestone_E_CLR = new ZDateTime(2013, 1, 14); // Estimated Customs Cleared (Clearance Finalized) Milestone
			orderBO.JD_Milestone_A_DCA = new ZDateTime(2013, 1, 15); // Actual Delivery Cartage Advised (Estimated Pickup) Milestone
			orderBO.JD_Milestone_E_DCA = new ZDateTime(2013, 1, 16); // Estimated Delivery Cartage Advised (Estimated Pickup) Milestone
			orderBO.JD_Milestone_A_DCF = new ZDateTime(2013, 1, 17); // Actual Delivery Cartage Complete/Finalised (Order Delivered) Milestone
			orderBO.JD_Milestone_E_DCF = new ZDateTime(2013, 1, 18); // Estimated Delivery Cartage Complete/Finalised (Order Delivered) Milestone
			orderBO.JD_Milestone_A_DEP = new ZDateTime(2013, 1, 19); // Actual Departure Milestone
			orderBO.JD_Milestone_E_DEP = new ZDateTime(2013, 1, 20); // Estimated Departure Milestone
			orderBO.JD_Milestone_A_GIW = new ZDateTime(2013, 1, 21); // Actual Gate In Wharf (Origin Receival) Milestone
			orderBO.JD_Milestone_E_GIW = new ZDateTime(2013, 1, 22); // Estimated Gate In Wharf (Origin Receival) Milestone
			orderBO.JD_ShipmentWindowStart = new ZDate(2012, 1, 5);
			orderBO.JD_ShipmentWindowEnd = new ZDate(2012, 1, 5);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderDataObject = writer.GetDataObject(orderBO);
			AssertNotNull(orderDataObject);

			CombineAssertions(delegate
			{
				AssertEquals("orderDataObject.ActualChargeable", null, orderDataObject.ActualChargeable);
				AssertEquals("orderDataObject.AdditionalTerms", "ADDMYONE", orderDataObject.AdditionalTerms);
				AssertEquals("orderDataObject.AgentsReference", null, orderDataObject.AgentsReference);
				AssertEquals("orderDataObject.AWBServiceLevel", null, orderDataObject.AWBServiceLevel);
				AssertEquals("orderDataObject.BookingConfirmationReference", "Booking Reference", orderDataObject.BookingConfirmationReference);
				AssertEquals("orderDataObject.Branch", null, orderDataObject.Branch);
				AssertEquals("orderDataObject.CarrierServiceLevel", null, orderDataObject.CarrierServiceLevel);
				AssertEquals("orderDataObject.CartageWaybillNumber", null, orderDataObject.CartageWaybillNumber);
				AssertEquals("orderDataObject.CFSReference", null, orderDataObject.CFSReference);
				AssertEquals("orderDataObject.ConsolidatedCargoStatus", null, orderDataObject.ConsolidatedCargoStatus);
				AssertEquals("orderDataObject.ContainerCount", null, orderDataObject.ContainerCount);
				AssertEquals("orderDataObject.ContainerMode.Code", "LSE", orderDataObject.ContainerMode.Code);
				AssertEquals("orderDataObject.ContainerMode.Description", "Loose", orderDataObject.ContainerMode.Description);
				AssertEquals("orderDataObject.CountryOfSupply.Code", "US", orderDataObject.CountryOfSupply.Code);
				AssertEquals("orderDataObject.CountryOfSupply.Name", "United States", orderDataObject.CountryOfSupply.Name);
				AssertEquals("orderDataObject.CustomsBroker", null, orderDataObject.CustomsBroker);
				AssertEquals("orderDataObject.CustomsContainerMode", null, orderDataObject.CustomsContainerMode);
				AssertEquals("orderDataObject.DocumentedChargeable", null, orderDataObject.DocumentedChargeable);
				AssertEquals("orderDataObject.DocumentedVolume", null, orderDataObject.DocumentedVolume);
				AssertEquals("orderDataObject.DocumentedWeight", null, orderDataObject.DocumentedWeight);
				AssertEquals("orderDataObject.EFTMode", null, orderDataObject.EFTMode);
				AssertEquals("orderDataObject.EntryStatus", null, orderDataObject.EntryStatus);
				AssertEquals("orderDataObject.ExportGoodsType", null, orderDataObject.ExportGoodsType);
				AssertEquals("orderDataObject.FirstBuyerContact", "Barney", orderDataObject.FirstBuyerContact);
				AssertEquals("orderDataObject.Folio", null, orderDataObject.Folio);
				AssertEquals("orderDataObject.FreightRate", 10.3m, orderDataObject.FreightRate);
				AssertEquals("orderDataObject.FreightRateCurrency.Code", "NZD", orderDataObject.FreightRateCurrency.Code);
				AssertEquals("orderDataObject.FreightRateCurrency.Description", "New Zealand Dollar", orderDataObject.FreightRateCurrency.Description);
				AssertEquals("orderDataObject.GoodsDescription", "Guitars", orderDataObject.GoodsDescription);
				AssertEquals("orderDataObject.GoodsValue", null, orderDataObject.GoodsValue);
				AssertEquals("orderDataObject.GoodsValueCurrency", null, orderDataObject.GoodsValueCurrency);
				AssertEquals("orderDataObject.HBLAWBChargesDisplay", null, orderDataObject.HBLAWBChargesDisplay);
				AssertEquals("orderDataObject.HBLContainerPackModeOverride", null, orderDataObject.HBLContainerPackModeOverride);
				AssertEquals("orderDataObject.InsuranceValue", null, orderDataObject.InsuranceValue);
				AssertEquals("orderDataObject.InsuranceValueCurrency", null, orderDataObject.InsuranceValueCurrency);
				AssertEquals("orderDataObject.InterimReceiptNumber", null, orderDataObject.InterimReceiptNumber);
				AssertEquals("orderDataObject.IsBooking", null, orderDataObject.IsBooking);
				AssertEquals("orderDataObject.IsCFSRegistered", null, orderDataObject.IsCFSRegistered);
				AssertEquals("orderDataObject.IsDirectBooking", null, orderDataObject.IsDirectBooking);
				AssertEquals("orderDataObject.IsForwardRegistered", null, orderDataObject.IsForwardRegistered);
				AssertEquals("orderDataObject.IsNeutralMaster", null, orderDataObject.IsNeutralMaster);
				AssertEquals("orderDataObject.IsPersonalEffects", null, orderDataObject.IsPersonalEffects);
				AssertEquals("orderDataObject.IsShipping", null, orderDataObject.IsShipping);
				AssertEquals("orderDataObject.IsSplitShipment", null, orderDataObject.IsSplitShipment);
				AssertEquals("orderDataObject.LloydsIMO", null, orderDataObject.LloydsIMO);
				AssertEquals("orderDataObject.LocalTransportEquipmentNeeded", null, orderDataObject.LocalTransportEquipmentNeeded);
				AssertEquals("orderDataObject.LocalTransportJobType", null, orderDataObject.LocalTransportJobType);
				AssertEquals("orderDataObject.ManifestedChargeable", null, orderDataObject.ManifestedChargeable);
				AssertEquals("orderDataObject.ManifestedVolume", null, orderDataObject.ManifestedVolume);
				AssertEquals("orderDataObject.ManifestedWeight", null, orderDataObject.ManifestedWeight);
				AssertEquals("orderDataObject.MergeBy", null, orderDataObject.MergeBy);
				AssertEquals("orderDataObject.MessageStatus", null, orderDataObject.MessageStatus);
				AssertEquals("orderDataObject.MessageSubType", null, orderDataObject.MessageSubType);
				AssertEquals("orderDataObject.MessageType", null, orderDataObject.MessageType);
				AssertEquals("orderDataObject.NoCopyBills", null, orderDataObject.NoCopyBills);
				AssertEquals("orderDataObject.NoOriginalBills", null, orderDataObject.NoOriginalBills);
				AssertEquals("orderDataObject.OperationalStatus", null, orderDataObject.OperationalStatus);
				AssertEquals("orderDataObject.OuterPacks", 2, orderDataObject.OuterPacks);
				AssertEquals("orderDataObject.OuterPacksPackageType.Code", "PLT", orderDataObject.OuterPacksPackageType.Code);
				AssertEquals("orderDataObject.OuterPacksPackageType.Description", "Pallet", orderDataObject.OuterPacksPackageType.Description);
				AssertEquals("orderDataObject.OwnerRef", null, orderDataObject.OwnerRef);
				AssertEquals("orderDataObject.PackingOrder", null, orderDataObject.PackingOrder);
				AssertEquals("orderDataObject.PaymentMethod", null, orderDataObject.PaymentMethod);
				AssertEquals("orderDataObject.PortMessaging", null, orderDataObject.PortMessaging);
				AssertEquals("orderDataObject.PortOfDestination.Code", "NZAKL", orderDataObject.PortOfDestination.Code);
				AssertEquals("orderDataObject.PortOfDestination.Name", "Auckland", orderDataObject.PortOfDestination.Name);
				AssertEquals("orderDataObject.PortOfDischarge.Code", "NZCHC", orderDataObject.PortOfDischarge.Code);
				AssertEquals("orderDataObject.PortOfDischarge.Name", "Christchurch", orderDataObject.PortOfDischarge.Name);
				AssertEquals("orderDataObject.PortOfFirstArrival", null, orderDataObject.PortOfFirstArrival);
				AssertEquals("orderDataObject.PortOfLoading.Code", "AUBNE", orderDataObject.PortOfLoading.Code);
				AssertEquals("orderDataObject.PortOfLoading.Name", "Brisbane", orderDataObject.PortOfLoading.Name);
				AssertEquals("orderDataObject.PortOfOrigin.Code", "AUMEL", orderDataObject.PortOfOrigin.Code);
				AssertEquals("orderDataObject.PortOfOrigin.Name", "Melbourne", orderDataObject.PortOfOrigin.Name);
				AssertEquals("orderDataObject.QuoteNumber", null, orderDataObject.QuoteNumber);
				AssertEquals("orderDataObject.ReleaseType", null, orderDataObject.ReleaseType);
				AssertEquals("orderDataObject.ScreeningStatus", null, orderDataObject.ScreeningStatus);
				AssertEquals("orderDataObject.SecondBuyerContact", "Fred", orderDataObject.SecondBuyerContact);
				AssertEquals("orderDataObject.ServiceLevel.Code", "STD", orderDataObject.ServiceLevel.Code);
				AssertEquals("orderDataObject.ServiceLevel.Description", "Standard", orderDataObject.ServiceLevel.Description);
				AssertEquals("orderDataObject.ShipmentIncoTerm.Code", "FCA", orderDataObject.ShipmentIncoTerm.Code);
				AssertEquals("orderDataObject.ShipmentIncoTerm.Description", "FCA - Free Carrier (seller is responsible for origin, buyer for loading)", orderDataObject.ShipmentIncoTerm.Description);
				AssertEquals("orderDataObject.ShipmentStatus", null, orderDataObject.ShipmentStatus);
				AssertEquals("orderDataObject.ShipmentSubType", null, orderDataObject.ShipmentSubType);
				AssertEquals("orderDataObject.ShipmentType", null, orderDataObject.ShipmentType);
				AssertEquals("orderDataObject.ShippedOnBoard", null, orderDataObject.ShippedOnBoard);
				AssertEquals("orderDataObject.ShipperCODAmount", null, orderDataObject.ShipperCODAmount);
				AssertEquals("orderDataObject.ShipperCODPayMethod", null, orderDataObject.ShipperCODPayMethod);
				AssertEquals("orderDataObject.TotalNoOfPacks", null, orderDataObject.TotalNoOfPacks);
				AssertEquals("orderDataObject.TotalNoOfPacksDecimal", null, orderDataObject.TotalNoOfPacksDecimal);
				AssertEquals("orderDataObject.TotalNoOfPacksPackageType", null, orderDataObject.TotalNoOfPacksPackageType);
				AssertEquals("orderDataObject.TotalNoOfPieces", null, orderDataObject.TotalNoOfPieces);
				AssertEquals("orderDataObject.TotalVolume", 53.2m, orderDataObject.TotalVolume);
				AssertEquals("orderDataObject.TotalVolumeUnit.Code", "CF", orderDataObject.TotalVolumeUnit.Code);
				AssertEquals("orderDataObject.TotalVolumeUnit.Description", "Cubic Feet", orderDataObject.TotalVolumeUnit.Description);
				AssertEquals("orderDataObject.TotalWeight", 32.4m, orderDataObject.TotalWeight);
				AssertEquals("orderDataObject.TotalWeightUnit.Code", "KT", orderDataObject.TotalWeightUnit.Code);
				AssertEquals("orderDataObject.TotalWeightUnit.Description", "Kilotons", orderDataObject.TotalWeightUnit.Description);
				AssertEquals("orderDataObject.TranshipToOtherCFS", null, orderDataObject.TranshipToOtherCFS);
				AssertEquals("orderDataObject.TransportBookingDirection", null, orderDataObject.TransportBookingDirection);
				AssertEquals("orderDataObject.TransportMode.Code", "AIR", orderDataObject.TransportMode.Code);
				AssertEquals("orderDataObject.TransportMode.Description", "Air Freight", orderDataObject.TransportMode.Description);
				AssertEquals("orderDataObject.VesselName", null, orderDataObject.VesselName);
				AssertEquals("orderDataObject.VoyageFlightNo", null, orderDataObject.VoyageFlightNo);
				AssertEquals("orderDataObject.WarehouseLocation", null, orderDataObject.WarehouseLocation);
				AssertEquals("orderDataObject.WarehouseReleaseStatus", null, orderDataObject.WarehouseReleaseStatus);
				AssertEquals("orderDataObject.WayBillNumber", "ORDERHOUSE", orderDataObject.WayBillNumber);
				AssertEquals("orderDataObject.WayBillType.Code", "HWB", orderDataObject.WayBillType.Code);
				AssertEquals("orderDataObject.WayBillType.Description", "House Waybill", orderDataObject.WayBillType.Description);

				AssertNotNull("Invoice Number and Date should be exported",
					orderDataObject.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(o => o.InvoiceNumber.GetValueOrDefault() == "4321" && o.InvoiceDate.GetValueOrDefault() == new ZDateTime(2012, 1, 4)));

				AssertEquals("orderDataObject.Order.AddPalletWeightToOrder", null, orderDataObject.Order.AddPalletWeightToOrder);
				AssertEquals("orderDataObject.Order.ClientReference", "Booking Reference", orderDataObject.Order.ClientReference);
				AssertEquals("orderDataObject.Order.DropMode", null, orderDataObject.Order.DropMode);
				AssertEquals("orderDataObject.Order.FulfillmentRule", null, orderDataObject.Order.FulfillmentRule);
				AssertEquals("orderDataObject.Order.IsReleased", true, orderDataObject.Order.IsReleased);
				AssertEquals("orderDataObject.Order.LocalCartageInsuranceValue", null, orderDataObject.Order.LocalCartageInsuranceValue);
				AssertEquals("orderDataObject.Order.OrderNumber", "ORDER1", orderDataObject.Order.OrderNumber);
				AssertEquals("orderDataObject.Order.OrderNumberSplit", new ZByte(2), orderDataObject.Order.OrderNumberSplit);
				AssertEquals("orderDataObject.Order.PalletsSent", null, orderDataObject.Order.PalletsSent);
				AssertEquals("orderDataObject.Order.PickOption", null, orderDataObject.Order.PickOption);
				AssertEquals("orderDataObject.Order.StagingArea", null, orderDataObject.Order.StagingArea);
				AssertEquals("orderDataObject.Order.Status.Code", "CNF", orderDataObject.Order.Status.Code);
				AssertEquals("orderDataObject.Order.Status.Description", "Confirmed", orderDataObject.Order.Status.Description);
				AssertEquals("orderDataObject.Order.TotalLineVolume", null, orderDataObject.Order.TotalLineVolume);
				AssertEquals("orderDataObject.Order.TotalLineWeight", null, orderDataObject.Order.TotalLineWeight);
				AssertEquals("orderDataObject.Order.TotalNetWeightSent", null, orderDataObject.Order.TotalNetWeightSent);
				AssertEquals("orderDataObject.Order.TotalUnits", null, orderDataObject.Order.TotalUnits);
				AssertEquals("orderDataObject.Order.TransportReference", null, orderDataObject.Order.TransportReference);
				AssertEquals("orderDataObject.Order.Type", null, orderDataObject.Order.Type);
				AssertEquals("orderDataObject.Order.UnitsSent", null, orderDataObject.Order.UnitsSent);
				AssertEquals("orderDataObject.Order.Warehouse", null, orderDataObject.Order.Warehouse);

				var transportLegs = orderDataObject.TransportLegCollection;
				AssertEquals(3, transportLegs.Count);

				var departureLeg = transportLegs.ToList().Find(o => o.VesselName.GetValueOrDefault() == "BELINGA" && o.VoyageFlightNo.GetValueOrDefault() == "BL123");
				AssertNotNull("Departure Leg should be exported", departureLeg);
				AssertEquals(TransportMode.Air, departureLeg.TransportMode);
				AssertEquals(new ZByte(1), departureLeg.LegOrder);
				AssertEquals(new ZDateTime(2013, 1, 20), departureLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 1), departureLeg.EstimatedArrival);

				var intermediateLeg = transportLegs.ToList().Find(o => o.VesselName.GetValueOrDefault() == "BUNDAGO" && o.VoyageFlightNo.GetValueOrDefault() == "BN321");
				AssertNotNull("Intermediate Leg should be exported", intermediateLeg);
				AssertEquals(TransportMode.Air, intermediateLeg.TransportMode);
				AssertEquals(new ZByte(2), intermediateLeg.LegOrder);
				AssertEquals(new ZDateTime(2013, 1, 2), intermediateLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 3), intermediateLeg.EstimatedArrival);

				var arrivalLeg = transportLegs.ToList().Find(o => o.VesselName.GetValueOrDefault() == "WUNDAGO" && o.VoyageFlightNo.GetValueOrDefault() == "WN321");
				AssertNotNull("Arrival Leg should be exported", arrivalLeg);
				AssertEquals(TransportMode.Air, arrivalLeg.TransportMode);
				AssertEquals(new ZByte(3), arrivalLeg.LegOrder);
				AssertEquals(new ZDateTime(2013, 1, 4), arrivalLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 8), arrivalLeg.EstimatedArrival);

				var localProcessing = orderDataObject.LocalProcessing;
				AssertEquals(new ZDateTime(2012, 1, 6), localProcessing.DeliveryRequiredBy);

				AssertEquals(7, orderDataObject.DateCollection.Count);
				orderDataObject.DateCollection.AssertDateExists(DateType.ExWorksRequiredBy, false, new ZDateTime(2012, 1, 1));
				orderDataObject.DateCollection.AssertDateExists(DateType.OrderDate, false, new ZDateTime(2012, 1, 2));
				orderDataObject.DateCollection.AssertDateExists(DateType.BookingConfirmed, false, new ZDateTime(2012, 1, 3));
				orderDataObject.DateCollection.AssertDateExists(DateType.FollowUp, false, new ZDateTime(2012, 1, 5));
				orderDataObject.DateCollection.AssertDateExists(DateType.DepartureVesselCutoffDate, false, new ZDateTime(2012, 1, 7));
				orderDataObject.DateCollection.AssertDateExists(DateType.ShipmentWindowStart, false, new ZDate(2012, 1, 5));
				orderDataObject.DateCollection.AssertDateExists(DateType.ShipmentWindowEnd, false, new ZDate(2012, 1, 5));

				AssertNotNull("orderDataObject.AdditionalBillCollection is not null", orderDataObject.AdditionalBillCollection);
				AssertEquals("orderDataObject.AdditionalBillCollection count", 1, orderDataObject.AdditionalBillCollection.Count);
				var additionalBill = orderDataObject.AdditionalBillCollection[0];
				AssertEquals("additionalBill.BillNumber", "ORDERMASTER", additionalBill.BillNumber);
				AssertEquals("additionalBill.BillType.Code", WayBillTypeList.Codes.Master, additionalBill.BillType.Code);
				AssertEquals("additionalBill.BillType.Description", "Master Waybill", additionalBill.BillType.Description);
			});
		}

		#endregion

		#region Collections

		#region TestOrderLines

		public void TestOrderLines()
		{
			var orderLine = OrderLineDataObjectWriterTest.GetOrderLine(Factory.BOFactory);
			var order = orderLine.Order;

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)));
			var orderDataObject = writer.GetDataObject(order);
			AssertNotNull(orderDataObject);
			AssertNotNull(orderDataObject.Order);
			AssertNotNull(orderDataObject.Order.OrderLineCollection);
			AssertEquals(1, orderDataObject.Order.OrderLineCollection.Count);

			CombineAssertions(delegate
			{
				var orderLineDataObject = orderDataObject.Order.OrderLineCollection[0];
				OrderLineDataObjectWriterTest.AssertContents(orderLineDataObject);
			});
		}

		#endregion

		#region TestWorkflowCustomFieldsAreExported

		public void TestWorkflowCustomFieldsAreExported()
		{
			var order = Factory.New<Order>();

			order.SetUserDefinedValue("Are you Happy?", ZBool.True);
			order.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			order.SetUserDefinedValue("The Happy Number", new ZInt(42));
			order.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			order.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order)));
			var orderDataObject = writer.GetDataObject(order);
			AssertNotNull("Precondition: orderDataObject", orderDataObject);

			var customFields = orderDataObject.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		#endregion

		#region TestOrganisationCustomFieldsOnOrderAreExported

		public void TestOrganisationCustomFieldsOnOrderAreExported()
		{
			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.CustomAttribute1, "What Makes You Happy?");
			orderBO.JD_CustomAttrib1 = "Lots Of Ice";
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.CustomDate1, "The Date You Are Happy");
			orderBO.JD_CustomDate1 = ZDateTime.BrettsBirthday;
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.CustomDecimal1, "The Happy Decimal");
			orderBO.JD_CustomDecimal1 = 7.7m;
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.CustomFlag1, "Are you Happy?");
			orderBO.JD_CustomFlag1 = true;
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.CustomContact1, "Your favourite friend");
			orderBO.JD_FirstBuyerContact = "Barney the Dinosaur";
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.UserTrackDate1, "Track Date");
			orderBO.JD_EstimateUserDate1 = new ZDateTime(2013, 1, 1);
			orderBO.JD_ActualUserDate1 = new ZDateTime(2013, 1, 2);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);
			AssertNotNull("Precondition: orderData", orderData);

			var customFields = orderData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 7, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.String, "Your favourite friend", "Barney the Dinosaur");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "Estimated Track Date", new ZDateTime(2013, 1, 1).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "Actual Track Date", new ZDateTime(2013, 1, 2).ToISO8601String());
			});
		}

		public void TestCustomLabelsAreNotTruncatedOnExport()
		{
			var recipient = Factory.New<OrgHeader>();
			recipient.OH_Code = "NEWORG";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.AddCustomLabel(Core.Constants.CustomLabels.Order.UserTrackDate1, Core.Constants.CustomLabels.Order.UserTrackDate1);
			orderBO.JD_EstimateUserDate1 = new ZDateTime(2013, 1, 1);
			orderBO.JD_ActualUserDate1 = new ZDateTime(2013, 1, 2);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(orderData, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertContains("Estimated " + Core.Constants.CustomLabels.Order.UserTrackDate1, result);
					AssertContains("Actual " + Core.Constants.CustomLabels.Order.UserTrackDate1, result);
				}
			}
		}

		#endregion

		#region TestOrganisations

		public void TestOrganisations()
		{
			var orderBO = Factory.New<Order>();
			orderBO.BuyerPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			orderBO.SupplierPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.JD_OH_Carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
			orderBO.JD_OH_SendingAgent = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
			orderBO.JD_OH_ReceivingAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.ControllingCustomerDocAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.WarehouseDocAddress.OrganisationPK = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;

			orderBO.GoodsAvailableAtAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.GoodsDeliveredToAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);
			AssertNotNull("orderData", orderData);
			AssertEquals("orderData.OrganizationAddressCollection.Count", 12, orderData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_INTHEMSYD("Warehouse", orderData.OrganizationAddressCollection[0], "Warehouse");
			AssertOrganizationBO_CRAHOLSYD("GoodsDeliveredToAddress", orderData.OrganizationAddressCollection[1], "GoodsDeliveredTo", true);
			AssertOrganizationBO_WUFSHIJNB("GoodsAvailableAtAddress", orderData.OrganizationAddressCollection[2], "GoodsAvailableAt", true);
			AssertOrganizationBO_WUFSHIJNB("ControllingCustomer", orderData.OrganizationAddressCollection[3], "ControllingCustomer");
			AssertOrganizationBO_WUFSHIJNB("ConsignorDocumentaryAddress", orderData.OrganizationAddressCollection[4], "ConsignorDocumentaryAddress");
			AssertOrganizationBO_INTHEMSYD("Carrier", orderData.OrganizationAddressCollection[5], "Carrier");
			AssertOrganizationBO_INTHEMSYD("SendingForwarderAddress", orderData.OrganizationAddressCollection[6], "SendingForwarderAddress");
			AssertOrganizationBO_WUFSHIJNB("ReceivingForwarderAddress", orderData.OrganizationAddressCollection[7], "ReceivingForwarderAddress");
			AssertOrganizationBO_CRAHOLSYD("ConsigneePickupDeliveryAddress", orderData.OrganizationAddressCollection[8], "ConsigneePickupDeliveryAddress", true);
			AssertOrganizationBO_WUFSHIJNB("ConsignorPickupDeliveryAddress", orderData.OrganizationAddressCollection[9], "ConsignorPickupDeliveryAddress", true);
			AssertOrganizationBO_WUFSHIJNB("OrderControllingParty", orderData.OrganizationAddressCollection[10], "OrderControllingParty");
			AssertOrganizationBO_CRAHOLSYD("ConsigneeDocumentaryAddress", orderData.OrganizationAddressCollection[11], "ConsigneeDocumentaryAddress");
		}

		public void TestOrganisations_BuyerWithoutConsignee()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			order.BuyerPK = buyer.PK;
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			var buyerDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "BuyerDocumentaryAddress");
			var consigneeDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "ConsigneeDocumentaryAddress");
			AssertEquals(buyer.OH_Code, consigneeDocumentaryAddress.OrganizationCode);
			AssertNull(buyerDocumentaryAddress);
		}

		public void TestOrganisations_BuyerWithConsignee()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.MainAddress.OA_Code = "BUY";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.OA_Code = "CON";
			order.BuyerPK = buyer.PK;
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			order.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			order.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			var buyerDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "BuyerDocumentaryAddress");
			var consigneeDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "ConsigneeDocumentaryAddress");
			AssertEquals(1, orderData.OrganizationAddressCollection.Count(a => a.AddressType.GetValueOrDefault() == "ConsigneeDocumentaryAddress"));
			AssertNotEquals(buyerDocumentaryAddress.OrganizationCode, consigneeDocumentaryAddress.OrganizationCode);
			AssertNotEquals(buyerDocumentaryAddress.AddressShortCode, consigneeDocumentaryAddress.AddressShortCode);
			AssertEquals(consignee.OH_Code, consigneeDocumentaryAddress.OrganizationCode);
			AssertEquals(consignee.MainAddress.OA_Code, consigneeDocumentaryAddress.AddressShortCode);
			AssertEquals(buyer.OH_Code, buyerDocumentaryAddress.OrganizationCode);
			AssertEquals(buyer.MainAddress.OA_Code, buyerDocumentaryAddress.AddressShortCode);
		}

		public void TestOrganisations_BuyerSelectedAddressIsPopulated()
		{
			var order = Factory.New<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var buyerMainAddress = buyer.Addresses.FirstOrDefault() as OrgAddress;
			buyerMainAddress.CompanyName = "Main Address";
			buyerMainAddress.Address1 = "123 Main Address";
			buyerMainAddress.Address2 = "Main line 2";

			var buyerNotMainAddress = Factory.New<OrgAddress>();
			buyer.Addresses.Add(buyerNotMainAddress);
			buyerNotMainAddress.CompanyName = "Other Address";
			buyerNotMainAddress.Address1 = "123 Not Main Address";
			buyerNotMainAddress.Address2 = "Not Main Line 2";

			order.BuyerPK = buyer.PK;
			order.JD_OA_BuyerAddress = buyerNotMainAddress.PK;

			AssertEquals("Precondition: buyer has two addresses", 2, buyer.Addresses.Count);
			AssertEquals("Precondition: The main address is buyerMainAddress", buyerMainAddress.PK, buyer.MainAddress.PK);

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertNotNull("Order data has been populated", orderData);

			var consigneeDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "ConsigneeDocumentaryAddress");
			AssertNotNull("Consignee Documentary Address type has been populated", consigneeDocumentaryAddress);

			CombineAssertions("Consignee Documentary Address in orderData is selected address", () =>
			{
				AssertEquals("Other Address", consigneeDocumentaryAddress.CompanyName);
				AssertEquals("123 Not Main Address", consigneeDocumentaryAddress.Address1);
				AssertEquals("Not Main Line 2", consigneeDocumentaryAddress.Address2);
			});
		}

		public void TestOrganisations_SupplierSelectedAddressIsPopulated()
		{
			var order = Factory.New<Order>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var supplierMainAddress = supplier.Addresses.FirstOrDefault() as OrgAddress;
			supplierMainAddress.CompanyName = "Main Address";
			supplierMainAddress.Address1 = "123 Main Address";
			supplierMainAddress.Address2 = "Main line 2";

			var supplierNotMainAddress = Factory.New<OrgAddress>();
			supplier.Addresses.Add(supplierNotMainAddress);
			supplierNotMainAddress.CompanyName = "Other Address";
			supplierNotMainAddress.Address1 = "123 Not Main Address";
			supplierNotMainAddress.Address2 = "Not Main Line 2";

			order.SupplierPK = supplier.PK;
			order.JD_OA_SupplierAddress = supplierNotMainAddress.PK;

			AssertEquals("Precondition: supplier has two addresses", 2, supplier.Addresses.Count);
			AssertEquals("Precondition: The main address is supplierMainAddress", supplierMainAddress.PK, supplier.MainAddress.PK);

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, order))).GetDataObject(order);
			AssertNotNull("Order data has been populated", orderData);

			var consignorDocumentaryAddress = orderData.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == "ConsignorDocumentaryAddress");
			AssertNotNull("Consignor Documentary Address type has been populated", consignorDocumentaryAddress);

			CombineAssertions("Consignor Documentary Address in orderData is selected address", () =>
			{
				AssertEquals("Other Address", consignorDocumentaryAddress.CompanyName);
				AssertEquals("123 Not Main Address", consignorDocumentaryAddress.Address1);
				AssertEquals("Not Main Line 2", consignorDocumentaryAddress.Address2);
			});
		}

		public void TestControllingCustomer_2011_11()
		{
			var orderBO = Factory.New<Order>();
			orderBO.ControllingCustomerDocAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);
				AssertNotNull("orderData", orderData);
				AssertEquals("orderData.OrganizationAddressCollection.Count", 2, orderData.OrganizationAddressCollection.Count);
				AssertOrganizationBO_WUFSHIJNB("ControllingCustomer", orderData.OrganizationAddressCollection[0], "ControllingCustomer");
				AssertOrganizationBO_WUFSHIJNB("OrderControllingParty", orderData.OrganizationAddressCollection[1], "OrderControllingParty");
			}
		}

		public void TestControllingCustomer_2012_11()
		{
			var orderBO = Factory.New<Order>();
			orderBO.ControllingCustomerDocAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);
				AssertNotNull("orderData", orderData);
				AssertEquals("orderData.OrganizationAddressCollection.Count", 1, orderData.OrganizationAddressCollection.Count);
				AssertOrganizationBO_WUFSHIJNB("ControllingCustomer", orderData.OrganizationAddressCollection[0], "ControllingCustomer");
			}
		}

		public void TestNotyfyParty()
		{
			var orderBO = Factory.New<Order>();
			orderBO.NotifyPartyDocAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.NotifyParty2DocAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			orderBO.NotifyParty3DocAddress.OrganisationPK = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);
			AssertNotNull("orderData", orderData);
			AssertEquals("orderData.OrganizationAddressCollection.Count", 3, orderData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("NotifyParty", orderData.OrganizationAddressCollection[0], "NotifyParty");
			AssertOrganizationBO_CRAHOLSYD("NotifyParty2", orderData.OrganizationAddressCollection[1], "NotifyParty2");
			AssertOrganizationBO_INTHEMSYD("NotifyParty3", orderData.OrganizationAddressCollection[2], "NotifyParty3");
		}

		public void TestManufacturer()
		{
			var orderBO = Factory.New<Order>();
			var manAddressBO = Factory.New<JobDocAddress>();
			manAddressBO.E2_AddressType = "MAN";
			manAddressBO.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			orderBO.DocAddresses.Add(manAddressBO);

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);
			AssertNotNull("orderData", orderData);
			AssertEquals("orderData.OrganizationAddressCollection.Count", 1, orderData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("NotifyParty", orderData.OrganizationAddressCollection[0], "Manufacturer");
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			var orderBO = Factory.New<Order>();
			orderBO.PlannedContainers.Add(OrderContainerDataObjectWriterTest.SetupContainer(Factory.BOFactory));
			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderData = writer.GetDataObject(orderBO);
			AssertNotNull(orderData);

			CombineAssertions(delegate
			{
				AssertEquals(1, orderData.ContainerCollection.Count);
				var containerData = orderData.ContainerCollection[0];
				OrderContainerDataObjectWriterTest.AssertContents(containerData);
			});
		}

		#endregion

		#region TestNotes

		public void TestNotes()
		{
			var orderBO = Factory.New<Order>();
			orderBO.JD_TransportMode = "SEA";
			orderBO.JD_ContainerMode = "LCL";

			var noteBO1 = orderBO.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			var noteBO2 = orderBO.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";

			var orderData = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO))).GetDataObject(orderBO);

			AssertNotNull("orderData", orderData);
			AssertNotNull("orderData.NoteCollection", orderData.NoteCollection);
			AssertEquals("orderData.NoteCollection.Count", 2, orderData.NoteCollection.Count);

			var note1 = orderData.NoteCollection[0];
			var note2 = orderData.NoteCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);

				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});
		}

		#endregion

		#region Transport Legs

		public void TestOneLeg()
		{
			var orderBO = Factory.New<Order>();
			orderBO.JD_TransportMode = "AIR";
			orderBO.JD_ContainerMode = "LSE";
			orderBO.JD_RV_NKDepartureVessel = "BELINGA";
			orderBO.JD_DepartureVoyage = "BL123";
			orderBO.JD_RV_NKArrivalVessel = "BELINGA";
			orderBO.JD_ArrivalVoyage = "BL123";
			orderBO.JD_Milestone_E_DEP = new ZDateTime(2013, 1, 20);
			orderBO.JD_Milestone_E_ARV = new ZDateTime(2013, 1, 28);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderDataObject = writer.GetDataObject(orderBO);
			AssertNotNull(orderDataObject);

			CombineAssertions(delegate
			{
				var transportLegs = orderDataObject.TransportLegCollection;
				AssertEquals(1, transportLegs.Count);

				var departureLeg = transportLegs.FirstOrDefault();
				AssertNotNull("Departure Leg should be exported", departureLeg);
				AssertEquals(TransportMode.Air, departureLeg.TransportMode);
				AssertEquals(new ZByte(1), departureLeg.LegOrder);
				AssertEquals("BELINGA", departureLeg.VesselName);
				AssertEquals("BL123", departureLeg.VoyageFlightNo);
				AssertEquals(new ZDateTime(2013, 1, 20), departureLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 28), departureLeg.EstimatedArrival);
			});
		}

		public void TestTwoLegs()
		{
			var orderBO = Factory.New<Order>();
			orderBO.JD_TransportMode = "AIR";
			orderBO.JD_ContainerMode = "LSE";
			orderBO.JD_RV_NKDepartureVessel = "BELINGA";
			orderBO.JD_DepartureVoyage = "BL123";
			orderBO.JD_RV_NKArrivalVessel = "WUNDAGO";
			orderBO.JD_ArrivalVoyage = "WN321";

			orderBO.JD_Milestone_E_DEP = new ZDateTime(2013, 1, 20);
			orderBO.JD_E_ARV_1stIntermediate = new ZDateTime(2013, 1, 23);
			orderBO.JD_E_DEP_3 = new ZDateTime(2013, 1, 24);
			orderBO.JD_Milestone_E_ARV = new ZDateTime(2013, 1, 28);

			var writer = new OrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderBO)));
			var orderDataObject = writer.GetDataObject(orderBO);
			AssertNotNull(orderDataObject);

			CombineAssertions(delegate
			{
				var transportLegs = orderDataObject.TransportLegCollection;
				AssertEquals(2, transportLegs.Count);

				var departureLeg = transportLegs.FirstOrDefault();
				AssertNotNull("Departure Leg should be exported", departureLeg);
				AssertEquals(TransportMode.Air, departureLeg.TransportMode);
				AssertEquals(new ZByte(1), departureLeg.LegOrder);
				AssertEquals("BELINGA", departureLeg.VesselName);
				AssertEquals("BL123", departureLeg.VoyageFlightNo);
				AssertEquals(new ZDateTime(2013, 1, 20), departureLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 23), departureLeg.EstimatedArrival);

				var arrivalLeg = transportLegs.LastOrDefault();
				AssertNotNull("Arrival Leg should be exported", arrivalLeg);
				AssertEquals(TransportMode.Air, arrivalLeg.TransportMode);
				AssertEquals(new ZByte(2), arrivalLeg.LegOrder);
				AssertEquals("WUNDAGO", arrivalLeg.VesselName);
				AssertEquals("WN321", arrivalLeg.VoyageFlightNo);
				AssertEquals(new ZDateTime(2013, 1, 24), arrivalLeg.EstimatedDeparture);
				AssertEquals(new ZDateTime(2013, 1, 28), arrivalLeg.EstimatedArrival);
			});
		}

		#endregion

		#endregion
	}
}
