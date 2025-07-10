using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class ForwardingBookingDataObjectWriterTest : BaseShipmentDataObjectWriterTest
	{
		public void TestContainerModeOverride()
		{
			const string containerModeOverride = Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			bookingBO.ContainerPackModeOverride = containerModeOverride;
			var shipmentData = GetShipmentData(bookingBO);
			Factory.SaveForTesting();
			AssertEquals("ContainerPackModeOverride", containerModeOverride, shipmentData.HBLContainerPackModeOverride);
		}

		public void TestBasicFieldMappings()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			#region Setup shipmentBO
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House
			shipmentBO.JS_UniqueConsignRef = "S00005000";
			shipmentBO.JS_AdditionalTerms = "Add Me Some Terms";
			shipmentBO.JS_BookingReference = "BOOK ME";
			shipmentBO.JS_CartageWaybill = "CARTAGE BILL";
			shipmentBO.JS_CFSReference = "CFS Book Ref";
			shipmentBO.JS_DocumentedVolume = 3.45m;
			shipmentBO.JS_DocumentedWeight = 4.56m;
			shipmentBO.JS_UnitFreightRate = 67.89m;
			shipmentBO.JS_RX_NKFrtRateCurrency = Enterprise.Core.Constants.CurrencyCodes.CzechRepublic;
			shipmentBO.JS_GoodsDescription = "RAT HATS";
			shipmentBO.JS_GoodsValue = 5.67m;
			shipmentBO.JS_RX_NKGoodsValueCurr = Enterprise.Core.Constants.CurrencyCodes.Ghana;
			shipmentBO.JS_HBLAWBChargesDisplay = "SHW";
			shipmentBO.JS_HBLContainerPackModeOverride = "FAR";
			shipmentBO.JS_InsuranceValue = 6.78m;
			shipmentBO.JS_RX_NKInsuranceCurrency = Enterprise.Core.Constants.CurrencyCodes.Kenya;
			shipmentBO.JS_InterimReceipt = "IR Text";
			shipmentBO.JS_ManifestedVolume = 8.90;
			shipmentBO.JS_ManifestedWeight = 9.01;
			shipmentBO.JS_OuterPacks = 44;
			shipmentBO.JS_F3_NKPackType = "VF";
			shipmentBO.JS_OverrideWaybillDefaults = true;
			shipmentBO.JS_PackingOrder = 1;
			shipmentBO.JS_ReleaseType = "CAD"; // Cash Against Documents
			shipmentBO.JS_ScreeningStatus = "UNK"; // Unknown
			shipmentBO.JS_RS_NKServiceLevel = "STD"; // Standard
			shipmentBO.JS_INCO = "CIF"; // Cost, Insurance And Freight
			shipmentBO.JS_ShippedOnBoard = "LDN"; // Laden
			shipmentBO.JS_ShipperCODAmount = 12.34m;
			shipmentBO.JS_ShipperCODPayMethod = "COC"; // Company Check
			shipmentBO.JS_TotalPackageCount = 45;
			shipmentBO.JS_F3_NKTotalCountPackType = "KEG"; // Keg
			shipmentBO.JS_ActualVolume = 23.45m;
			shipmentBO.JS_UnitOfVolume = "CF"; // Cubic Feet
			shipmentBO.JS_ActualWeight = 34.56m;
			shipmentBO.JS_UnitOfWeight = "KT"; // Kilotons
			shipmentBO.JS_TranshipToOtherCFS = true;
			var consolBO = shipmentBO.Consols.AddNew();
			shipmentBO.JS_RL_NKOrigin = "NZDUD"; // Dunedin
			consolBO.JK_RL_NKLoadPort = "NZCHC"; // Christchurch
			consolBO.JK_RL_NKPortOfFirstArrival = "AUNTL"; // Newcastle
			consolBO.JK_RL_NKDischargePort = "AUSYD"; // Sydney
			shipmentBO.JS_RL_NKDestination = "AUBDG"; // Bendigo
			shipmentBO.JS_RL_NKLoadPort = "NZCHC";
			shipmentBO.JS_RL_NKDischargePort = "AUSYD";
			var leg = consolBO.Transports[0];
			leg.JW_Vessel = "BUNGA DELIMA";
			leg.JW_VoyageFlight = "343L";
			leg.JW_ETD = new ZDateTime(2011, 3, 4);
			leg.JW_ETA = new ZDateTime(2011, 3, 6);
			shipmentBO.JS_HouseBill = "MYHOUSE";
			shipmentBO.JS_ActualChargeable = 123.45m;
			shipmentBO.JS_DocumentedChargeable = 2.34m;
			shipmentBO.JS_ManifestedChargeable = 7.89;
			shipmentBO.JS_NoCopyBills = new ZByte(3);
			shipmentBO.JS_NoOriginalBills = new ZByte(4);
			shipmentBO.JS_PL_NKCarrierServiceLevel = "STD";
			shipmentBO.JS_CompanyTariffLevelOverride = 1;
			#endregion
			var shipmentData = GetShipmentData(bookingBO);
			AssertNotNull("shipmentData", shipmentData);
			#region Check Contents of shipmentData object
			CombineAssertions(() =>
			{
				AssertEquals("shipmentData.ActualChargeable", 123.45m, shipmentData.ActualChargeable);
				AssertEquals("shipmentData.AdditionalTerms", "Add Me Some Terms", shipmentData.AdditionalTerms);
				AssertEquals("shipmentData.AgentsReference", "BOOK ME", shipmentData.AgentsReference);
				AssertEquals("shipmentData.CoLoadBookingConfirmationReference", "S00005000", shipmentData.CoLoadBookingConfirmationReference);
				AssertEquals("shipmentData.AWBServiceLevel.Code", "STD", shipmentData.AWBServiceLevel.Code);
				AssertEquals("shipmentData.AWBServiceLevel.Description", "Standard", shipmentData.AWBServiceLevel.Description);
				AssertEquals("shipmentData.BookingConfirmationReference", "BOOK ME", shipmentData.BookingConfirmationReference);
				AssertEquals("shipmentData.CartageWaybillNumber", null, shipmentData.CartageWaybillNumber);
				AssertEquals("shipmentData.CFSReference", "CFS Book Ref", shipmentData.CFSReference);
				AssertEquals("shipmentData.ConsolidatedCargoStatus", null, shipmentData.ConsolidatedCargoStatus);
				AssertEquals("shipmentData.ContainerCount", 0, shipmentData.ContainerCount);
				AssertEquals("shipmentData.ContainerMode.Code", "LCL", shipmentData.ContainerMode.Code);
				AssertEquals("shipmentData.ContainerMode.Description", "Less Container Load", shipmentData.ContainerMode.Description);
				AssertEquals("shipmentData.CountryOfSupply", null, shipmentData.CountryOfSupply);
				AssertEquals("shipmentData.DocumentedChargeable", null, shipmentData.DocumentedChargeable);
				AssertEquals("shipmentData.DocumentedVolume", null, shipmentData.DocumentedVolume);
				AssertEquals("shipmentData.DocumentedWeight", null, shipmentData.DocumentedWeight);
				AssertEquals("shipmentData.EFTMode", null, shipmentData.EFTMode);
				AssertEquals("shipmentData.EntryStatus", null, shipmentData.EntryStatus);
				AssertEquals("shipmentData.ExportGoodsType", null, shipmentData.ExportGoodsType);
				AssertEquals("shipmentData.FirstBuyerContact", null, shipmentData.FirstBuyerContact);
				AssertEquals("shipmentData.Folio", null, shipmentData.Folio);
				AssertEquals("shipmentData.FreightRate", 67.89m, shipmentData.FreightRate);
				AssertEquals("shipmentData.FreightRateCurrency.Code", "CZK", shipmentData.FreightRateCurrency.Code);
				AssertEquals("shipmentData.FreightRateCurrency.Description", "Czech Koruna", shipmentData.FreightRateCurrency.Description);
				AssertEquals("shipmentData.GoodsDescription", "RAT HATS", shipmentData.GoodsDescription);
				AssertEquals("shipmentData.GoodsValue", 5.67m, shipmentData.GoodsValue);
				AssertEquals("shipmentData.GoodsValueCurrency.Code", "GHS", shipmentData.GoodsValueCurrency.Code);
				AssertEquals("shipmentData.GoodsValueCurrency.Description", "Ghana Cedi", shipmentData.GoodsValueCurrency.Description);
				AssertEquals("shipmentData.HBLAWBChargesDisplay.Code", "SHW", shipmentData.HBLAWBChargesDisplay.Code);
				AssertEquals("shipmentData.HBLAWBChargesDisplay.Description", "Show Collect Charges", shipmentData.HBLAWBChargesDisplay.Description);
				AssertEquals("shipmentData.HBLContainerPackModeOverride", "FAR", shipmentData.HBLContainerPackModeOverride);
				AssertEquals("shipmentData.InsuranceValue", 6.78m, shipmentData.InsuranceValue);
				AssertEquals("shipmentData.InsuranceValueCurrency.Code", "KES", shipmentData.InsuranceValueCurrency.Code);
				AssertEquals("shipmentData.InsuranceValueCurrency.Description", "Kenyan Shilling", shipmentData.InsuranceValueCurrency.Description);
				AssertEquals("shipmentData.InterimReceiptNumber", "IR Text", shipmentData.InterimReceiptNumber);
				AssertEquals("shipmentData.IsBooking", null, shipmentData.IsBooking);
				AssertEquals("shipmentData.IsCFSRegistered", null, shipmentData.IsCFSRegistered);
				AssertEquals("shipmentData.IsDirectBooking", false, shipmentData.IsDirectBooking);
				AssertEquals("shipmentData.IsForwardRegistered", ZBool.False, shipmentData.IsForwardRegistered);
				AssertEquals("shipmentData.IsNeutralMaster", null, shipmentData.IsNeutralMaster);
				AssertEquals("shipmentData.IsPersonalEffects", null, shipmentData.IsPersonalEffects);
				AssertEquals("shipmentData.IsShipping", null, shipmentData.IsShipping);
				AssertEquals("shipmentData.IsSplitShipment", null, shipmentData.IsSplitShipment);
				AssertEquals("shipmentData.LloydsIMO", "8907993", shipmentData.LloydsIMO);
				AssertEquals("shipmentData.ManifestedChargeable", null, shipmentData.ManifestedChargeable);
				AssertEquals("shipmentData.ManifestedVolume", null, shipmentData.ManifestedVolume);
				AssertEquals("shipmentData.ManifestedWeight", null, shipmentData.ManifestedWeight);
				AssertEquals("shipmentData.MergeBy", null, shipmentData.MergeBy);
				AssertEquals("shipmentData.MessageStatus", null, shipmentData.MessageStatus);
				AssertEquals("shipmentData.MessageSubType", null, shipmentData.MessageSubType);
				AssertEquals("shipmentData.MessageType", null, shipmentData.MessageType);
				AssertEquals("shipmentData.NoCopyBills", null, shipmentData.NoCopyBills);
				AssertEquals("shipmentData.NoOriginalBills", null, shipmentData.NoOriginalBills);
				AssertEquals("shipmentData.OperationalStatus", null, shipmentData.OperationalStatus);
				AssertEquals("shipmentData.OuterPacks", 44, shipmentData.OuterPacks);
				AssertEquals("shipmentData.OuterPacksPackageType.Code", "VF", shipmentData.OuterPacksPackageType.Code);
				AssertEquals("shipmentData.OwnerRef", null, shipmentData.OwnerRef);
				AssertEquals("shipmentData.PackingOrder", 1, shipmentData.PackingOrder);
				AssertEquals("shipmentData.PaymentMethod", null, shipmentData.PaymentMethod);
				AssertEquals("shipmentData.QuoteNumber", null, shipmentData.QuoteNumber);
				AssertEquals("shipmentData.ReleaseType.Code", "CAD", shipmentData.ReleaseType.Code);
				AssertEquals("shipmentData.ReleaseType.Description", "Cash Against Documents", shipmentData.ReleaseType.Description);
				AssertEquals("shipmentData.ScreeningStatus", null, shipmentData.ScreeningStatus);
				AssertEquals("shipmentData.SecondBuyerContact", null, shipmentData.SecondBuyerContact);
				AssertEquals("shipmentData.ServiceLevel.Code", "STD", shipmentData.ServiceLevel.Code);
				AssertEquals("shipmentData.ServiceLevel.Description", "Standard", shipmentData.ServiceLevel.Description);
				AssertEquals("shipmentData.ShipmentIncoTerm.Code", "CIF", shipmentData.ShipmentIncoTerm.Code);
				AssertEquals("shipmentData.ShipmentIncoTerm.Description", "Cost, Insurance And Freight", shipmentData.ShipmentIncoTerm.Description);
				AssertEquals("shipmentData.ShipmentStatus", null, shipmentData.ShipmentStatus);
				AssertEquals("shipmentData.ShipmentType", null, shipmentData.ShipmentType);
				AssertEquals("shipmentData.ShippedOnBoard.Code", "LDN", shipmentData.ShippedOnBoard.Code);
				AssertEquals("shipmentData.ShippedOnBoard.Description", "Laden", shipmentData.ShippedOnBoard.Description);
				AssertEquals("shipmentData.ShipperCODAmount", null, shipmentData.ShipperCODAmount);
				AssertEquals("shipmentData.ShipperCODPayMethod", null, shipmentData.ShipperCODPayMethod);
				AssertEquals("shipmentData.TotalNoOfPacks", null, shipmentData.TotalNoOfPacks);
				AssertEquals("shipmentData.TotalNoOfPacksDecimal", null, shipmentData.TotalNoOfPacksDecimal);
				AssertEquals("shipmentData.TotalNoOfPacksPackageType", null, shipmentData.TotalNoOfPacksPackageType);
				AssertEquals("shipmentData.TotalNoOfPieces", null, shipmentData.TotalNoOfPieces);
				AssertEquals("shipmentData.TotalVolume", 23.45m, shipmentData.TotalVolume);
				AssertEquals("shipmentData.TotalVolumeUnit.Code", "CF", shipmentData.TotalVolumeUnit.Code);
				AssertEquals("shipmentData.TotalVolumeUnit.Description", "Cubic Feet", shipmentData.TotalVolumeUnit.Description);
				AssertEquals("shipmentData.TotalWeight", 34.56m, shipmentData.TotalWeight);
				AssertEquals("shipmentData.TotalWeightUnit.Code", "KT", shipmentData.TotalWeightUnit.Code);
				AssertEquals("shipmentData.TotalWeightUnit.Description", "Kilotons", shipmentData.TotalWeightUnit.Description);
				AssertEquals("shipmentData.TranshipToOtherCFS", null, shipmentData.TranshipToOtherCFS);
				AssertEquals("shipmentData.TransportMode.Code", "SEA", shipmentData.TransportMode.Code);
				AssertEquals("shipmentData.TransportMode.Description", "Sea Freight", shipmentData.TransportMode.Description);
				AssertEquals("shipmentData.PortOfOrigin.Code", "NZDUD", shipmentData.PortOfOrigin.Code);
				AssertEquals("shipmentData.PortOfOrigin.Name", "Dunedin", shipmentData.PortOfOrigin.Name);
				AssertEquals("shipmentData.PortOfLoading.Code", "NZCHC", shipmentData.PortOfLoading.Code);
				AssertEquals("shipmentData.PortOfLoading.Name", "Christchurch", shipmentData.PortOfLoading.Name);
				AssertEquals("shipmentData.PortOfFirstArrival", null, shipmentData.PortOfFirstArrival);
				AssertEquals("shipmentData.PortOfDischarge.Code", "AUSYD", shipmentData.PortOfDischarge.Code);
				AssertEquals("shipmentData.PortOfDischarge.Name", "Sydney", shipmentData.PortOfDischarge.Name);
				AssertEquals("shipmentData.PortOfDestination.Code", "AUBDG", shipmentData.PortOfDestination.Code);
				AssertEquals("shipmentData.PortOfDestination.Name", "Bendigo", shipmentData.PortOfDestination.Name);
				AssertEquals("shipmentData.VesselName", "BUNGA DELIMA", shipmentData.VesselName);
				AssertEquals("shipmentData.VoyageFlightNo", "343L", shipmentData.VoyageFlightNo);
				AssertEquals("shipmentData.WarehouseReleaseStatus", null, shipmentData.WarehouseReleaseStatus);
				AssertEquals("shipmentData.WayBillNumber", "MYHOUSE", shipmentData.WayBillNumber);
				AssertEquals("shipmentData.WayBillType.Code", "HWB", shipmentData.WayBillType.Code);
				AssertEquals("shipmentData.WayBillType.Description", "House Waybill", shipmentData.WayBillType.Description);
				AssertEquals("shipmentData.CarrierServiceLevel.Code", "STD", shipmentData.CarrierServiceLevel.Code);
				AssertEquals("shipmentData.CarrierServiceLevel.Description", "Standard", shipmentData.CarrierServiceLevel.Description);
				AssertEquals("CompanyTariffLevelOverride should be exported", (ZByte)1, shipmentData.CompanyTariffLevelOverride);
			}

			);
			#endregion
		}

		public void TestWorkflowCustomFields()
		{
			testHelper.AssertWorkflowCustomFields(QuoteBookingType.QuickBooking);
		}

		public void TestWorkflowCustomFields_FromMatchedWorkflow()
		{
			testHelper.AssertWorkflowCustomFields_FromMatchedWorkflow(QuoteBookingType.QuickBooking, QuotedBooking.QuickBookingCode, (quotedBooking) => quotedBooking.Booking);
		}

		public void TestLocalProcessing()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			#region Setup shipmentBO
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House
			var docsBO = shipmentBO.DocsAndCartage;
			docsBO.JP_FCLPickupEquipmentNeeded = "TRL";
			docsBO.JP_EstimatedPickup = new ZDateTime(2011, 5, 1);
			docsBO.JP_PickupRequiredBy = new ZDateTime(2011, 5, 2);
			docsBO.JP_PickupCartageAdvised = new ZDateTime(2011, 5, 3);
			docsBO.JP_ArrivalCartageRef = "ARRCARTREF";
			docsBO.JP_PickupCartageCompleted = new ZDateTime(2011, 5, 4);
			docsBO.JP_PickupLabourTime = new ZDateTime(2011, 5, 5);
			docsBO.JP_PickupLabourCharge = 1.11m;
			docsBO.JP_PickupTruckWaitTime = new ZDateTime(2011, 5, 6);
			docsBO.JP_PickupTruckWaitCharge = 2.22m;
			docsBO.JP_PrintOptionForPackagesOnAWB = "ALL";
			docsBO.JP_FCLDeliveryEquipmentNeeded = "WUP";
			docsBO.JP_FCLAvailable = new ZDateTime(2011, 5, 7);
			docsBO.JP_FCLStorageCommences = new ZDateTime(2011, 5, 8);
			docsBO.JP_LCLAvailable = new ZDateTime(2011, 5, 9);
			docsBO.JP_LCLStorageCommences = new ZDateTime(2011, 5, 10);
			docsBO.JP_LCLAirStorageDaysOrHours = new ZByte(12);
			docsBO.JP_LCLAirStorageCharge = 3.33m;
			docsBO.JP_EstimatedDelivery = new ZDateTime(2011, 5, 11);
			docsBO.JP_DeliveryRequiredBy = new ZDateTime(2011, 5, 12);
			docsBO.JP_DeliveryCartageAdvised = new ZDateTime(2011, 5, 13);
			docsBO.JP_DeliveryCartageCompleted = new ZDateTime(2011, 5, 14);
			docsBO.JP_DeliveryLabourTime = new ZDateTime(2011, 5, 15);
			docsBO.JP_DeliveryLabourCharge = 4.44m;
			docsBO.JP_DeliveryTruckWaitTime = new ZDateTime(2011, 5, 16);
			docsBO.JP_DeliveryTruckWaitCharge = 5.55m;
			docsBO.JP_HasProhibitedPackaging = ZBool.True;
			docsBO.JP_InsuranceRequired = ZBool.False;
			docsBO.JP_IsContingencyRelease = ZBool.True;
			docsBO.JP_LCLDatesOverrideConsol = ZBool.True;
			docsBO.JP_ExportStatement = "DEF";
			var orderBO = docsBO.OrderItems.AddNew();
			orderBO.JT_Sequence = 1;
			orderBO.JT_OrderReference = "ORDER_FF";
			var serviceBO = docsBO.Services.AddNew();
			serviceBO.ES_ServiceId = "WTLKKK00000043";
			serviceBO.ES_ExternalServiceId = "WTLKKK00000044";
			serviceBO.ES_ServiceCode = "TAI";
			serviceBO.ES_Booked = new ZDateTime(2011, 6, 1);
			serviceBO.ES_Completed = new ZDateTime(2011, 6, 2);
			serviceBO.ES_Duration = new ZDateTime(2011, 6, 3);
			serviceBO.ES_ServiceCount = 1.23m;
			serviceBO.ES_ServiceNote = "BLAH BLAH BLAH BLAH Hotdog BLAH BLAH BLAH.";
			serviceBO.ES_References = "GREAT!";
			serviceBO.ES_OH_Contractor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			#endregion
			var shipmentData = GetShipmentData(bookingBO);
			#region Check Contents of shipmentData object
			AssertNotNull("shipmentData", shipmentData);
			var localProcessing = shipmentData.LocalProcessing;
			AssertNotNull("shipmentData.LocalProcessing", localProcessing);
			CombineAssertions("Checking all fields on LocalProcessing", () =>
			{
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Code", "TRL", localProcessing.FCLPickupEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Description", "Drop Trailer", localProcessing.FCLPickupEquipmentNeeded.Description);
				AssertEquals("localProcessing.EstimatedPickup", new ZDateTime(2011, 5, 1), localProcessing.EstimatedPickup);
				AssertEquals("localProcessing.PickupRequiredBy", new ZDateTime(2011, 5, 2), localProcessing.PickupRequiredBy);
				AssertEquals("localProcessing.PickupCartageAdvised", null, localProcessing.PickupCartageAdvised);
				AssertEquals("localProcessing.ArrivalCartageRef", null, localProcessing.ArrivalCartageRef);
				AssertEquals("localProcessing.PickupCartageCompleted", null, localProcessing.PickupCartageCompleted);
				AssertEquals("localProcessing.PickupLabourTime", null, localProcessing.PickupLabourTime);
				AssertEquals("localProcessing.PickupLabourCharge", null, localProcessing.PickupLabourCharge);
				AssertEquals("localProcessing.DemurrageOnPickupTime", null, localProcessing.DemurrageOnPickupTime);
				AssertEquals("localProcessing.PickupTruckWaitTime", null, localProcessing.PickupTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnPickupCharge", null, localProcessing.DemurrageOnPickupCharge);
				AssertEquals("localProcessing.PickupTruckWaitCharge", null, localProcessing.PickupTruckWaitCharge);
				AssertEquals("localProcessing.PrintOptionForPackagesOnAWB", null, localProcessing.PrintOptionForPackagesOnAWB);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Code", "WUP", localProcessing.FCLDeliveryEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Description", "Wait for Pack/Unpack", localProcessing.FCLDeliveryEquipmentNeeded.Description);
				AssertEquals("localProcessing.FCLAvailable", null, localProcessing.FCLAvailable);
				AssertEquals("localProcessing.FCLStorageCommences", null, localProcessing.FCLStorageCommences);
				AssertEquals("localProcessing.LCLAvailable", null, localProcessing.LCLAvailable);
				AssertEquals("localProcessing.LCLStorageCommences", null, localProcessing.LCLStorageCommences);
				AssertEquals("localProcessing.LCLAirStorageDaysOrHours", null, localProcessing.LCLAirStorageDaysOrHours);
				AssertEquals("localProcessing.LCLAirStorageCharge", null, localProcessing.LCLAirStorageCharge);
				AssertEquals("localProcessing.EstimatedDelivery", new ZDateTime(2011, 5, 11), localProcessing.EstimatedDelivery);
				AssertEquals("localProcessing.DeliveryRequiredBy", new ZDateTime(2011, 5, 12), localProcessing.DeliveryRequiredBy);
				AssertEquals("localProcessing.DeliveryCartageAdvised", null, localProcessing.DeliveryCartageAdvised);
				AssertEquals("localProcessing.DeliveryCartageCompleted", null, localProcessing.DeliveryCartageCompleted);
				AssertEquals("localProcessing.DeliveryLabourTime", null, localProcessing.DeliveryLabourTime);
				AssertEquals("localProcessing.DeliveryLabourCharge", null, localProcessing.DeliveryLabourCharge);
				AssertEquals("localProcessing.DemurrageOnDeliveryTime", null, localProcessing.DemurrageOnDeliveryTime);
				AssertEquals("localProcessing.DeliveryTruckWaitTime", null, localProcessing.DeliveryTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnDeliveryCharge", null, localProcessing.DemurrageOnDeliveryCharge);
				AssertEquals("localProcessing.DeliveryTruckWaitCharge", null, localProcessing.DeliveryTruckWaitCharge);
				AssertEquals("localProcessing.HasProhibitedPackaging", null, localProcessing.HasProhibitedPackaging);
				AssertEquals("localProcessing.InsuranceRequired", ZBool.False, localProcessing.InsuranceRequired);
				AssertEquals("localProcessing.IsContingencyRelease", null, localProcessing.IsContingencyRelease);
				AssertEquals("localProcessing.LCLDatesOverrideConsol", null, localProcessing.LCLDatesOverrideConsol);
				AssertEquals("localProcessing.ExportStatement", null, localProcessing.ExportStatement);
				AssertNotNull("localProcessing.OrderNumberCollection", localProcessing.OrderNumberCollection);
				AssertNotNull("localProcessing.AdditionalServiceCollection", localProcessing.AdditionalServiceCollection);
			}

			);
			AssertEquals("localProcessing.OrderNumberCollection.Count", 1, localProcessing.OrderNumberCollection.Count);
			var orderNumber = localProcessing.OrderNumberCollection[0];
			AssertEquals("orderNumber.Sequence", new ZShort(1), orderNumber.Sequence);
			AssertEquals("orderNumber.OrderReference", "ORDER_FF", orderNumber.OrderReference);
			AssertEquals("localProcessing.AdditionalServiceCollection.Count", 1, localProcessing.AdditionalServiceCollection.Count);
			var additionalService = localProcessing.AdditionalServiceCollection[0];
			CombineAssertions("Checking all fields on AdditionalService", () =>
			{
				AssertEquals("additionalService.ServiceId", "WTLKKK00000043", additionalService.ServiceId);
				AssertEquals("additionalService.ExternalServiceId", "WTLKKK00000044", additionalService.ExternalServiceId);
				AssertEquals("additionalService.ServiceCode.Code", "TAI", additionalService.ServiceCode.Code);
				AssertEquals("additionalService.ServiceCode.Description", "Tailgate", additionalService.ServiceCode.Description);
				AssertEquals("additionalService.Booked", new ZDateTime(2011, 6, 1), additionalService.Booked);
				AssertEquals("additionalService.Completed", new ZDateTime(2011, 6, 2), additionalService.Completed);
				AssertEquals("additionalService.Duration", null, additionalService.Duration);
				AssertEquals("additionalService.ServiceCount", null, additionalService.ServiceCount);
				AssertEquals("additionalService.ServiceNote", null, additionalService.ServiceNote);
				AssertEquals("additionalService.References", null, additionalService.References);
			}

			);
			AssertOrganizationBO_WUFSHIJNB("additionalService.Contractor", additionalService.Contractor, "Contractor");
			#endregion
		}

		public void TestWithContainers()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House
			var commodity1 = GetCommodity(Factory.BOFactory);
			var container1 = bookingBO.QuotedBookingContainers.AddNew();
			PopulateContainer1(container1, commodity1, Factory.BOFactory);
			var shipmentData = GetShipmentData(bookingBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("shipmentData.ContainerCollection", shipmentData.ContainerCollection);
			AssertEquals("shipmentData.ContainerCollection.Count", 1, shipmentData.ContainerCollection.Count);
			CombineAssertions(() =>
			{
				AssertContainer1(shipmentData.ContainerCollection[0]);
			});
		}

		public void TestForwardingOrders()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			var warehouseOrder = (BusinessObject)Factory.BOFactory.New<IWhsOrder>();
			warehouseOrder.FillWithValidTestData();
			shipmentBO.AttachedWarehouseOrders.Add(warehouseOrder);
			var order = shipmentBO.AttachedOrders.AddNew();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = new ZByte(2);
			var shipmentData = GetShipmentData(bookingBO);
			AssertNotNull(shipmentData);
			AssertNotNull(shipmentData.RelatedShipmentCollection);
			AssertEquals(1, shipmentData.RelatedShipmentCollection.Count);
			var orderData = shipmentData.RelatedShipmentCollection[0];
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(2), orderData.Order.OrderNumberSplit);
		}

		public void TestDates()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			#region Setup shipmentBO
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House
			shipmentBO.JS_A_BKD = new ZDateTime(2011, 3, 11);
			shipmentBO.JS_A_RCV = new ZDateTime(2011, 3, 12);
			shipmentBO.JS_E_DEP = new ZDateTime(2011, 3, 13);
			shipmentBO.JS_E_ARV = new ZDateTime(2011, 3, 14);
			shipmentBO.JS_ClientRequestedETA = new ZDateTime(2011, 3, 15);
			shipmentBO.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "For Test";
			shipmentBO.JS_DeliveryDueDate = new ZDateTime(2012, 4, 15);
			shipmentBO.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2012, 4, 16);

			#endregion
			var shipmentData = GetShipmentData(bookingBO);
			#region Check Contents of shipmentData object
			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.DateCollection", shipmentData.DateCollection);
			CombineAssertions("Checking all dates in DateCollection", () =>
			{
				shipmentData.DateCollection.AssertDateExists(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 3, 11));
				shipmentData.DateCollection.AssertDateExists(DateType.Received, ZBool.False, new ZDateTime(2011, 3, 12));
				shipmentData.DateCollection.AssertDateExists(DateType.Departure, ZBool.True, new ZDateTime(2011, 3, 13));
				shipmentData.DateCollection.AssertDateExists(DateType.Arrival, ZBool.True, new ZDateTime(2011, 3, 14));
				shipmentData.DateCollection.AssertDateExists(DateType.ClientRequestedETA, ZBool.True, new ZDateTime(2011, 3, 15));
				shipmentData.DateCollection.AssertDateExists(DateType.DeliveryDueDate, ZBool.False, new ZDateTime(2012, 4, 15));
				shipmentData.DateCollection.AssertDateExists(DateType.RevisedDeliveryDueDate, ZBool.False, new ZDateTime(2012, 4, 16));
				AssertEquals("Checking dates left, and found dates not expected.", 0, shipmentData.DateCollection.Count);
			}

			);
			#endregion
		}

		public void TestCarrier()
		{
			testHelper.AssertCarrier(QuoteBookingType.QuickBooking, (quotedBooking) =>
			{
				quotedBooking.Booking.JS_OA_BookedShippingLineAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;
			});
		}

		public void TestCreditor()
		{
			testHelper.AssertCreditor(QuoteBookingType.BookingWithQuote, (quotedBooking) =>
			{
				var creditor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				creditor.OH_IsCreditor = true;

				quotedBooking.Booking.JS_OH_Creditor = creditor.PK;
			});
		}

		public void TestNotes()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			testHelper.AssertNotes(bookingBO);
		}

		public void TestClientAddress()
		{
			testHelper.AssertClientAddress(QuoteBookingType.BookingWithQuote);
		}

		public void TestOrganizations()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			#region Setup shipmentBO
			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House
			shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			var pickupAddress = shipmentBO.ConsignorPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_Address1 = "FOO";
			pickupAddress.E2_Address2 = "FIGHTERS";
			pickupAddress.E2_City = "ROCK THIS TOWN";
			pickupAddress.E2_State = "BLOWNAWAY";
			pickupAddress.E2_Postcode = "2344";
			pickupAddress.E2_RN_NKCountryCode = "DE";
			shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
			var deliveryAddress = shipmentBO.ConsigneeDeliveryAddress;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_Contact = "JOHNNY ROTTEN";
			deliveryAddress.E2_Email = "rotten@sexpistols.com";
			deliveryAddress.E2_Phone = "432890234";
			deliveryAddress.E2_Fax = "54309823";
			deliveryAddress.E2_Mobile = "3452432890";
			shipmentBO.CreateShipmentJobHeaderWithMutex();
			shipmentBO.ShipmentJobHeader.Dispose(); // Unlocks Mutex
			shipmentBO.ShipmentJobHeader.JH_OA_AgentCollectAddr = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;
			#endregion
			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);
			#region Check Contents of shipmentData object
			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertAddress(shipmentData.OrganizationAddressCollection, 0, "ConsignorDocumentaryAddress", "WUFSHIJNB", "WUFU SHIPPING LINE", false, "Level 2, Building G", "34 Dock Lane", "Johannesburg", "", "12345", "ZA", "Benny Banana", "benny.banana@wufu.co.za", "0011 54 392 2921", "0011 289 392 2900", "0011 54 392 2900");
			AssertAddress(shipmentData.OrganizationAddressCollection, 1, "ConsignorPickupDeliveryAddress", null, "WUFU SHIPPING LINE", true, "FOO", "FIGHTERS", "ROCK THIS TOWN", "BLOWNAWAY", "2344", "DE", "Benny Banana", "benny.banana@wufu.co.za", "0011 54 392 2921", "0011 289 392 2900", "0011 54 392 2900");
			AssertAddress(shipmentData.OrganizationAddressCollection, 2, "ConsigneeDocumentaryAddress", "CRAHOLSYD", "CRACKERJACK HOLDINGS", false, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU", "Rob Anybody", "rob.anybody@cjh.com.au", "02 6392 2921", "0421 392 290", "02 6392 2900");
			AssertAddress(shipmentData.OrganizationAddressCollection, 3, "ConsigneePickupDeliveryAddress", null, "CRACKERJACK HOLDINGS", true, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU", "JOHNNY ROTTEN", "rotten@sexpistols.com", "54309823", "3452432890", "432890234");
			AssertAddress(shipmentData.OrganizationAddressCollection, 4, "NotifyParty", "CRAHOLSYD", "CRACKERJACK HOLDINGS", false, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU", "Rob Anybody", "rob.anybody@cjh.com.au", "02 6392 2921", "0421 392 290", "02 6392 2900");
			AssertAddress(shipmentData.OrganizationAddressCollection, 5, "SendersLocalClient", "CRAHOLSYD", "CRACKERJACK HOLDINGS", false, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU", null, "", "", null, "");
			AssertAddress(shipmentData.OrganizationAddressCollection, 6, "SendersOverseasAgent", "INTHEMSYD", "In The Moment", false, "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU", null, "s.m@moment.com.au", "234098234", null, "1239813209");
			#endregion
		}

		public void TestIDocAddressesGetWrittenOnce()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			var shipmentBO = bookingBO.Booking;

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			var notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var notifyParty2Org = Factory.NewWithValidTestData<OrgHeader>();
			var notifyParty3Org = Factory.NewWithValidTestData<OrgHeader>();

			shipmentBO.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeOrg.MainAddress.PK;
			shipmentBO.ConsignorDocumentaryAddress.E2_OA_Address = consignorOrg.MainAddress.PK;
			var notifyPartyDocAddress = ((IDocAddresses)bookingBO).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyPartyDocAddress.E2_OA_Address = notifyPartyOrg.MainAddress.PK;
			var notifyParty2DocAddress = ((IDocAddresses)bookingBO).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty2);
			notifyParty2DocAddress.E2_OA_Address = notifyParty2Org.MainAddress.PK;
			var notifyParty3DocAddress = ((IDocAddresses)bookingBO).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty3);
			notifyParty3DocAddress.E2_OA_Address = notifyParty3Org.MainAddress.PK;

			var shipmentData = GetShipmentData(bookingBO);
			AssertEquals(5, shipmentData.OrganizationAddressCollection.Count);
			var docAddressTypes = shipmentData.OrganizationAddressCollection.Select(docAddress => docAddress.AddressType);
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				nameof(DocAddressType.ConsigneeDocumentaryAddress),
				nameof(DocAddressType.ConsignorDocumentaryAddress),
				nameof(DocAddressType.NotifyParty),
				nameof(DocAddressType.NotifyParty2),
				nameof(DocAddressType.NotifyParty3)
			}, docAddressTypes);
		}

		public void TestWithFMCTariffID()
		{
			var bwq = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bwq.FMCTariffID = "abcd";
			var shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"abcd", shipmentData.FMCTariffID);
			bwq.FMCTariffID = "";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"", shipmentData.FMCTariffID);

			bwq = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			bwq.FMCTariffID = "1234";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"1234", shipmentData.FMCTariffID);
			bwq.FMCTariffID = "";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"", shipmentData.FMCTariffID);
		}

		public void TestWithCommodity()
		{
			var bwq = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bwq.Commodity = "SHIP";
			var shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"SHIP", shipmentData.RateCommodity.Code);
			bwq.Commodity = "";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"", shipmentData.RateCommodity.Code);

			bwq = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			bwq.Commodity = "CHEM";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"CHEM", shipmentData.RateCommodity.Code);
			bwq.Commodity = "";
			shipmentData = GetShipmentData(bwq);
			AssertEquals((ZString)"", shipmentData.RateCommodity.Code);
		}

		public void TestExportBookingToUniversalShipment_ShouldHaveMilestonesInformation()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var milestone = bookingBO.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "ADD";
			milestone.P9_Description = "QBK, ALL, QBN, global, cp";
			milestone.P9_IsPublished = true;

			Factory.SaveForTesting();

			var manager = bookingBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, bookingBO)));
			var shipmentData = (Shipment)writer.GetDataObject(bookingBO);
			AssertEquals(1, shipmentData.MilestoneCollection.Count);
			AssertEquals("ADD", shipmentData.MilestoneCollection[0].EventCode);
			AssertEquals("QBK, ALL, QBN, global, cp", shipmentData.MilestoneCollection[0].Description);
		}

		public void TestExportingBookingToXML_ShouldDetermineCFSAddress_BasedOnMode()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingBO.ExportReceivingDepot = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;
			bookingBO.Mode = Constants.RateMode.LSE;
			var shipmentData = GetShipmentData(bookingBO);
			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);
			AssertEquals("DepartureCFSAddress", shipmentData.OrganizationAddressCollection[0].AddressType);
		}

		public void TestExportingBookingToXML_ShouldDetermineCTOAddress_BasedOnMode()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingBO.ExportReceivingDepot = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;
			bookingBO.Mode = Constants.RateMode.COU;
			var shipmentData = GetShipmentData(bookingBO);
			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);
			AssertEquals("DepartureCTOAddress", shipmentData.OrganizationAddressCollection[0].AddressType);
		}

		public void TestExportingBookingToXML_ShouldHaveInspectionType()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			var shipmentData = GetShipmentData(bookingBO);
			AssertEquals("AviationSecurityInspectionType.Code", "UNK", shipmentData.AviationSecurityInspectionType.Code);
			AssertEquals("AviationSecurityInspectionType.Description", FreightDataRegistry.AviationSecurity_Unknown_Description, shipmentData.AviationSecurityInspectionType.Description);

			bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingBO.Booking.JS_InspectionTypeCode = "APP";
			shipmentData = GetShipmentData(bookingBO);
			AssertEquals("AviationSecurityInspectionType.Code", "APP", shipmentData.AviationSecurityInspectionType.Code);
			AssertEquals("AviationSecurityInspectionType.Description", FreightUtilities.InspectionType_Approved_Description, shipmentData.AviationSecurityInspectionType.Description);
		}

		public void TestGreenhouseGasEmission()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingBO.SetTotalCO2e(1m);
			bookingBO.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipmentData = GetShipmentData(bookingBO);
			AssertNotNull("shipmentData.GreenhouseGasEmission", shipmentData.GreenhouseGasEmission);
			AssertEquals(1m, shipmentData.GreenhouseGasEmission.CO2e);
			CombineAssertions("GreenhouseGasEmission.CO2eUnit", () =>
			{
				AssertEquals("Code", "KG", shipmentData.GreenhouseGasEmission.CO2eUnit.Code);
				AssertEquals("Description", "Kilograms", shipmentData.GreenhouseGasEmission.CO2eUnit.Description);
			});
			AssertNull("Should not write into GreenhouseGasEmission.CO2eStatus", shipmentData.GreenhouseGasEmission.CO2eStatus);
			CombineAssertions("GreenhouseGasEmission.CO2eDescriptiveStatus", () =>
			{
				AssertEquals("Code", CO2eStatusList.Codes.Current, shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Code);
				AssertEquals("Description", CO2eHelper.GetCO2eStatusShortDescription(CO2eStatusList.Codes.Current), shipmentData.GreenhouseGasEmission.CO2eDescriptiveStatus.Description);
			});
		}

		public void TestQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP()
		{
			testHelper.AssertQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory), GetShipmentData);
		}

		public void TestCarrierContractNumberIsWritten()
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			bookingBO.CarrierContractNumber = "ABC123";

			var shipmentData = GetShipmentData(bookingBO);
			AssertEquals("Contract Number is written", "ABC123", shipmentData.CarrierContractNumber);
		}

		#region Implementation
		static RefCommodityCode GetCommodity(BusinessObjectFactory factory)
		{
			var commodity1 = factory.New<RefCommodityCode>();
			commodity1.RH_Code = "FGFG";
			commodity1.RH_Description = "Fudge Guts Fingers Gone";
			commodity1.RH_IsShipping = true;
			commodity1.RH_IsForwarding = true;
			commodity1.RH_IsLandTransport = false;
			return commodity1;
		}

		static void PopulateContainer1(ForwardingContainer container1, RefCommodityCode commodity1, BusinessObjectFactory factory)
		{
			container1.JC_AirVentFlow = 12.3m;
			container1.JC_AirVentFlowRateUnit = "MQH"; // Cubic meters per hour
			container1.JC_ArrivalCartageAdvised = new ZDateTime(2011, 1, 1);
			container1.JC_ArrivalCartageComplete = new ZDateTime(2011, 1, 2);
			container1.ArrivalTruckWaitCost = 23.45m;
			container1.ArrivalTruckWaitTime = new ZDateTime(2011, 1, 3);
			container1.JC_ArrivalCartageRef = "ARRCARTREF";
			container1.JC_ArrivalDeliveryRequiredBy = new ZDateTime(2011, 1, 4);
			container1.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 1, 5);
			container1.JC_ArrivalPickupByRail = ZBool.True;
			container1.JC_ArrivalSlotDateTime = new ZDateTime(2011, 1, 6);
			container1.JC_ArrivalSlotReference = "ARRSLOTREF";
			container1.JC_RH_NKContainerCommodityCode = commodity1.RH_Code;
			container1.JC_ContainerCount = 4;
			container1.ArrivalCarrierDetentionCost = 34.56m;
			container1.ArrivalCarrierDetentionDays = new ZByte(2);
			container1.JC_ContainerImportDORelease = "CTRIMPDOR";
			container1.JC_ContainerNum = "OOCL0000027";
			container1.JC_ContainerYardEmptyPickupGateOut = new ZDateTime(2011, 1, 7);
			container1.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2011, 1, 8);
			container1.JC_ContainerQuality = "HID"; // Hides
			container1.JC_ContainerStatus = "INS"; // To Be Inspected
			var containerType = factory.New<RefContainer>();
			containerType.RC_Code = "ZW0W";
			containerType.RC_Description = "Container Type WOW!!";
			containerType.RC_ISOType = "21G5";
			container1.JC_RC = containerType.PK;
			container1.JC_DeliveryMode = "DLVMODE";
			container1.JC_DeliverySequence = 3;
			container1.JC_DepartureCartageAdvised = new ZDateTime(2011, 1, 9);
			container1.JC_DepartureCartageComplete = new ZDateTime(2011, 1, 10);
			container1.DepartureTruckWaitCost = 45.67m;
			container1.DepartureTruckWaitTime = new ZDateTime(2011, 1, 11);
			container1.JC_DepartureCartageRef = "DEPCARTREF";
			container1.JC_DepartureDeliveryByRail = ZBool.False;
			container1.JC_DepartureEstimatedPickup = new ZDateTime(2011, 1, 12);
			container1.JC_DepartureSlotDateTime = new ZDateTime(2011, 1, 13);
			container1.JC_DepartureSlotReference = "DEPSLOTREF";
			container1.JC_DunnageWeight = 56.78m;
			container1.JC_EmptyReadyForReturn = new ZDateTime(2011, 1, 14);
			container1.JC_EmptyRequired = new ZDateTime(2011, 1, 15);
			container1.JC_EmptyReturnedBy = new ZDateTime(2011, 1, 16);
			container1.JC_ExportDepotCustomsReference = "EXPDEPCUSR";
			container1.JC_ContainerMode = Constants.ContainerModes.LCL; // Less Container Load (FCL_LCL_AIR)
			container1.JC_OverrideFCLAvailableStorage = ZBool.True; // Not sure if this should be here at all.
			container1.JC_FCLAvailable = new ZDateTime(2011, 1, 17);
			container1.JC_FCLHeldInTransitStaging = ZBool.True;
			container1.JC_FCLOnBoardVessel = new ZDateTime(2011, 1, 18);
			container1.JC_FCLStorageArrivedUnderbond = ZBool.False;
			container1.ArrivalCTOStorageCost = 67.89m;
			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(2011, 1, 19);
			container1.ArrivalCTOStorageDays = new ZByte(4);
			container1.JC_FCLStorageModuleOnlyMaster = "FCLSTOMAST";
			container1.JC_FCLStorageUnderbondCleared = new ZDateTime(2011, 1, 20);
			container1.JC_FCLUnloadFromVessel = new ZDateTime(2011, 1, 21);
			container1.JC_FCLWharfGateIn = new ZDateTime(2011, 1, 22);
			container1.JC_FCLWharfGateOut = new ZDateTime(2011, 1, 23);
			container1.JC_GrossWeight = 78.90m;
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			container1.JC_HumidityPercent = new ZByte(5);
			container1.JC_IsCFSRegistered = ZBool.False;
			container1.JC_IsControlledAtmosphere = ZBool.True;
			container1.JC_IsDamaged = ZBool.True;
			container1.JC_IsEmptyContainer = ZBool.False;
			container1.JC_IsSealOk = ZBool.True;
			container1.JC_IsShipperOwned = ZBool.False;
			container1.JC_OverrideLCLAvailableStorage = ZBool.True; // Not sure if this should be here at all.
			container1.JC_LCLAvailable = new ZDateTime(2011, 1, 24);
			container1.JC_LCLStorageCommences = new ZDateTime(2011, 1, 25);
			container1.JC_LCLUnpack = new ZDateTime(2011, 1, 26);
			container1.JC_PackDate = new ZDateTime(2011, 1, 27);
			container1.JC_RefrigGeneratorID = "REFGENID";
			container1.JC_ReleaseNum = "RELNUM";
			container1.JC_SealNum = "SEAL1";
			container1.JC_AdditionalSealNum = "SEAL2";
			container1.JC_Additional2SealNum = "SEAL3";
			container1.JC_SetPointTemp = 89.01m;
			container1.JC_SetPointTempUnit = "F"; // Fahrenheit
			container1.JC_TareWeight = 90.12m;
			container1.JC_TempRecorderSerialNo = "TEMPRECSERNO";
			container1.JC_TotalHeight = 123.45m;
			container1.JC_TotalLength = 234.56m;
			container1.JC_TotalUnitOfMeasure = Constants.Length.Centimetres; // UnitOfLength.Empty;
			container1.JC_TotalWidth = 345.67m;
			container1.JC_TrainWagonNumber = "TRNWAGNO";
			container1.JC_UnpackGang = "UNPKGANG";
			container1.JC_UnpackShed = "UNPKSHED";
			container1.JC_VolumeCapacity = 456.78m;
			container1.JC_VolumeCapacityUQ = Constants.Volume.CubicInches; // UnitOfVolume.Empty;
			container1.JC_WeightCapacity = 567.89m;
			container1.WeightUnitForBinding = Constants.Weight.Pounds; // UnitOfWeight.Empty;
			container1.JC_SealParty = "CAR";
			container1.JC_AdditionalSealParty = "CRD";
			container1.JC_Additional2SealParty = "CUS";
		}

		static void AssertContainer1(Container containerData1)
		{
			AssertEquals("containerData1.AirVentFlow", 12.3m, containerData1.AirVentFlow);
			AssertEquals("containerData1.AirVentFlowRateUnit", null, containerData1.AirVentFlowRateUnit);
			AssertEquals("containerData1.ArrivalCartageAdvised", null, containerData1.ArrivalCartageAdvised);
			AssertEquals("containerData1.ArrivalCartageComplete", null, containerData1.ArrivalCartageComplete);
			AssertEquals("containerData1.ArrivalCartageDemurrageCharge", null, containerData1.ArrivalCartageDemurrageCharge);
			AssertEquals("containerData1.ArrivalCartageDemurrageTime", null, containerData1.ArrivalCartageDemurrageTime);
			AssertEquals("containerData1.ArrivalCartageRef", null, containerData1.ArrivalCartageRef);
			AssertEquals("containerData1.ArrivalDeliveryRequiredBy", null, containerData1.ArrivalDeliveryRequiredBy);
			AssertEquals("containerData1.ArrivalEstimatedDelivery", null, containerData1.ArrivalEstimatedDelivery);
			AssertEquals("containerData1.ArrivalPickupByRail", null, containerData1.ArrivalPickupByRail);
			AssertEquals("containerData1.ArrivalSlotDateTime", null, containerData1.ArrivalSlotDateTime);
			AssertEquals("containerData1.ArrivalSlotReference", null, containerData1.ArrivalSlotReference);
			AssertEquals("containerData1.Commodity.Code", "FGFG", containerData1.Commodity.Code);
			AssertEquals("containerData1.Commodity.Description", "Fudge Guts Fingers Gone", containerData1.Commodity.Description);
			AssertEquals("containerData1.ContainerCount", 4, containerData1.ContainerCount);
			AssertEquals("containerData1.ContainerDetentionCharge", null, containerData1.ContainerDetentionCharge);
			AssertEquals("containerData1.ContainerDetentionDays", null, containerData1.ContainerDetentionDays);
			AssertEquals("containerData1.ContainerImportDORelease", null, containerData1.ContainerImportDORelease);
			AssertEquals("containerData1.ContainerNumber", "OOCL0000027", containerData1.ContainerNumber);
			AssertEquals("containerData1.ContainerParkEmptyPickupGateOut", null, containerData1.ContainerParkEmptyPickupGateOut);
			AssertEquals("containerData1.ContainerParkEmptyReturnGateIn", null, containerData1.ContainerParkEmptyReturnGateIn);
			AssertEquals("containerData1.ContainerQuality", null, containerData1.ContainerQuality);
			AssertEquals("containerData1.ContainerType.Code", "ZW0W", containerData1.ContainerType.Code);
			AssertEquals("containerData1.ContainerType.Description", "Container Type WOW!!", containerData1.ContainerType.Description);
			AssertEquals("containerData1.ContainerType.ISOCode", "21G5", containerData1.ContainerType.ISOCode);
			AssertEquals("containerData1.ContainerStatus", null, containerData1.ContainerStatus);
			AssertEquals("containerData1.DeliveryMode", "DLVMODE", containerData1.DeliveryMode);
			AssertEquals("containerData1.DeliverySequence", null, containerData1.DeliverySequence);
			AssertEquals("containerData1.DepartureCartageAdvised", null, containerData1.DepartureCartageAdvised);
			AssertEquals("containerData1.DepartureCartageComplete", null, containerData1.DepartureCartageComplete);
			AssertEquals("containerData1.DepartureCartageDemurrageCharge", null, containerData1.DepartureCartageDemurrageCharge);
			AssertEquals("containerData1.DepartureCartageDemurrageTime", null, containerData1.DepartureCartageDemurrageTime);
			AssertEquals("containerData1.DepartureCartageRef", null, containerData1.DepartureCartageRef);
			AssertEquals("containerData1.DepartureDeliveryByRail", null, containerData1.DepartureDeliveryByRail);
			AssertEquals("containerData1.DepartureEstimatedPickup", new ZDateTime(2011, 1, 12), containerData1.DepartureEstimatedPickup);
			AssertEquals("containerData1.DepartureSlotDateTime", new ZDateTime(2011, 1, 13), containerData1.DepartureSlotDateTime);
			AssertEquals("containerData1.DepartureSlotReference", "DEPSLOTREF", containerData1.DepartureSlotReference);
			AssertEquals("containerData1.DunnageWeight", null, containerData1.DunnageWeight);
			AssertEquals("containerData1.EmptyReadyForReturn", null, containerData1.EmptyReadyForReturn);
			AssertEquals("containerData1.EmptyRequired", new ZDateTime(2011, 1, 15), containerData1.EmptyRequired);
			AssertEquals("containerData1.EmptyReturnedBy", null, containerData1.EmptyReturnedBy);
			AssertEquals("containerData1.ExportDepotCustomsReference", null, containerData1.ExportDepotCustomsReference);
			AssertEquals("containerData1.FCL_LCL_AIR.Code", "LCL", containerData1.FCL_LCL_AIR.Code);
			AssertEquals("containerData1.FCL_LCL_AIR.Description", "Less Container Load", containerData1.FCL_LCL_AIR.Description);
			AssertEquals("containerData1.FCLAvailable", null, containerData1.FCLAvailable);
			AssertEquals("containerData1.FCLHeldInTransitStaging", null, containerData1.FCLHeldInTransitStaging);
			AssertEquals("containerData1.FCLOnBoardVessel", null, containerData1.FCLOnBoardVessel);
			AssertEquals("containerData1.FCLStorageArrivedUnderbond", null, containerData1.FCLStorageArrivedUnderbond);
			AssertEquals("containerData1.FCLStorageCharge", null, containerData1.FCLStorageCharge);
			AssertEquals("containerData1.FCLStorageCommences", null, containerData1.FCLStorageCommences);
			AssertEquals("containerData1.FCLStorageDays", null, containerData1.FCLStorageDays);
			AssertEquals("containerData1.FCLStorageModuleOnlyMaster", null, containerData1.FCLStorageModuleOnlyMaster);
			AssertEquals("containerData1.FCLStorageUnderbondCleared", null, containerData1.FCLStorageUnderbondCleared);
			AssertEquals("containerData1.FCLUnloadFromVessel", null, containerData1.FCLUnloadFromVessel);
			AssertEquals("containerData1.FCLWharfGateIn", null, containerData1.FCLWharfGateIn);
			AssertEquals("containerData1.FCLWharfGateOut", null, containerData1.FCLWharfGateOut);
			AssertEquals("containerData1.GrossWeight", null, containerData1.GrossWeight);
			AssertEquals("containerData1.GoodsWeight", null, containerData1.GoodsWeight);
			AssertEquals("containerData1.HumidityPercent", new ZByte(5), containerData1.HumidityPercent);
			AssertEquals("containerData1.IsCFSRegistered", null, containerData1.IsCFSRegistered);
			AssertEquals("containerData1.IsControlledAtmosphere", null, containerData1.IsControlledAtmosphere);
			AssertEquals("containerData1.IsDamaged", null, containerData1.IsDamaged);
			AssertEquals("containerData1.IsEmptyContainer", null, containerData1.IsEmptyContainer);
			AssertEquals("containerData1.IsSealOk", null, containerData1.IsSealOk);
			AssertEquals("containerData1.IsShipperOwned", false, containerData1.IsShipperOwned);
			AssertEquals("containerData1.LCLAvailable", null, containerData1.LCLAvailable);
			AssertEquals("containerData1.LCLStorageCommences", null, containerData1.LCLStorageCommences);
			AssertEquals("containerData1.LCLUnpack", null, containerData1.LCLUnpack);
			AssertEquals("containerData1.OverrideFCLAvailableStorage", null, containerData1.OverrideFCLAvailableStorage);
			AssertEquals("containerData1.OverrideLCLAvailableStorage", null, containerData1.OverrideLCLAvailableStorage);
			AssertEquals("containerData1.PackDate", null, containerData1.PackDate);
			AssertEquals("containerData1.RefrigGeneratorID", null, containerData1.RefrigGeneratorID);
			AssertEquals("containerData1.ReleaseNum", "RELNUM", containerData1.ReleaseNum);
			AssertEquals("containerData1.Seal", null, containerData1.Seal);
			AssertEquals("containerData1.SecondSeal", null, containerData1.SecondSeal);
			AssertEquals("containerData1.SetPointTemp", 89.01m, containerData1.SetPointTemp);
			AssertEquals("containerData1.SetPointTempUnit", null, containerData1.SetPointTempUnit);
			AssertEquals("containerData1.TareWeight", null, containerData1.TareWeight);
			AssertEquals("containerData1.TempRecorderSerialNo", "TEMPRECSERNO", containerData1.TempRecorderSerialNo);
			AssertEquals("containerData1.ThirdSeal", null, containerData1.ThirdSeal);
			AssertEquals("containerData1.TotalHeight", null, containerData1.TotalHeight);
			AssertEquals("containerData1.TotalLength", null, containerData1.TotalLength);
			AssertEquals("containerData1.LengthUnit", null, containerData1.LengthUnit);
			AssertEquals("containerData1.TotalWidth", null, containerData1.TotalWidth);
			AssertEquals("containerData1.TrainWagonNumber", null, containerData1.TrainWagonNumber);
			AssertEquals("containerData1.UnpackGang", null, containerData1.UnpackGang);
			AssertEquals("containerData1.UnpackShed", null, containerData1.UnpackShed);
			AssertEquals("containerData1.VolumeCapacity", null, containerData1.VolumeCapacity);
			AssertEquals("containerData1.VolumeUnit", null, containerData1.VolumeUnit);
			AssertEquals("containerData1.WeightCapacity", null, containerData1.WeightCapacity);
			AssertEquals("containerData1.WeightUnit", null, containerData1.WeightUnit);
			AssertEquals("containerData1.SealPartyType", null, containerData1.SealPartyType);
			AssertEquals("containerData1.SecondSealPartyType", null, containerData1.SecondSealPartyType);
			AssertEquals("containerData1.ThirdSealPartyType", null, containerData1.ThirdSealPartyType);
		}

		Shipment GetShipmentData(QuotedBooking quotedBooking)
		{
			return GetShipmentData(quotedBooking, RecipientRoleType.ORP);
		}

		Shipment GetShipmentData(QuotedBooking quotedBooking, RecipientRoleType recipientRoleType)
		{
			var writer = new ForwardingBookingDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, quotedBooking)), quotedBooking);
			return writer.GetDataObject(quotedBooking.Booking);
		}

		#endregion
		protected override BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO)
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			return new ForwardingBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)), bookingBO);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testHelper = new QuotedBookingDataObjectWriterTestHelper(Factory, GetShipmentData);
		}

		QuotedBookingDataObjectWriterTestHelper testHelper;
	}
}
