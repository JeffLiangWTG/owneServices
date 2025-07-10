using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestClientPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.ClientPK = org.PK;
			AssertNoErrors(booking.ClientPKInfo);
			booking.ClientPK = new Guid("8dc0afc8-12df-4ee4-857a-1cc8dfc989e9");
			AssertHasError(booking.ClientPKInfo, "Enter a valid selection.");
			booking.ClientPK = ZGuid.Empty;
			AssertNoErrors(booking.ClientPKInfo);
		}

		public void TestExportReceivingDepot()
		{
			var cto = Factory.New<OrgHeader>();
			cto.OH_IsSeaCTO = true;
			cto.OH_IsAirCTO = true;
			cto.OH_IsMiscFreightServices = true;
			var depot = Factory.New<OrgHeader>();
			depot.OH_IsPackDepot = true;
			depot.OH_IsMiscFreightServices = true;
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.ExportReceivingDepot = ZGuid.Invalid;
			booking.Validation.ValidateExportReceivingDepot();
			AssertHasError(booking.ExportReceivingDepotInfo, "Enter a valid Pickup CFS Address.");
			booking.Mode = Core.Constants.RateMode.FCL;
			booking.ExportReceivingDepot = cto.MainAddress.PK;
			booking.Validation.ValidateExportReceivingDepot();
			AssertNoErrors(booking.ExportReceivingDepotInfo);
			booking.Mode = Core.Constants.RateMode.LSE;
			booking.ExportReceivingDepot = depot.MainAddress.PK;
			booking.Validation.ValidateExportReceivingDepot();
			AssertNoErrors(booking.ExportReceivingDepotInfo);
		}

		public void TestImportReleaseDepot()
		{
			var cto = Factory.New<OrgHeader>();
			cto.OH_IsSeaCTO = true;
			cto.OH_IsAirCTO = true;
			cto.OH_IsMiscFreightServices = true;
			var depot = Factory.New<OrgHeader>();
			depot.OH_IsUnpackDepot = true;
			depot.OH_IsMiscFreightServices = true;
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.ImportReleaseDepot = ZGuid.Invalid;
			booking.Validation.ValidateImportReleaseDepot();
			AssertHasError(booking.ImportReleaseDepotInfo, "Enter a valid Delivery CFS Address.");
			booking.Mode = Core.Constants.RateMode.FCL;
			booking.ImportReleaseDepot = cto.MainAddress.PK;
			booking.Validation.ValidateImportReleaseDepot();
			AssertNoErrors(booking.ImportReleaseDepotInfo);
			booking.Mode = Core.Constants.RateMode.LSE;
			booking.ImportReleaseDepot = depot.MainAddress.PK;
			booking.Validation.ValidateImportReleaseDepot();
			AssertNoErrors(booking.ImportReleaseDepotInfo);
		}

		public void TestCheckPickUpEquipment()
		{
			QuotedBookingValidationForTest bookingValidation = new QuotedBookingValidationForTest(QB);
			QB.Mode = Core.Constants.RateMode.SEA;
			QB.PickupEquipment = Core.Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertNoErrors(QB.PickupEquipmentInfo);
		}

		public void TestCheckDeliveryEquipment()
		{
			QuotedBookingValidationForTest bookingValidation = new QuotedBookingValidationForTest(QB);
			QB.Mode = Core.Constants.RateMode.SEA;
			QB.DeliveryEquipment = Core.Constants.FCLEquipmentNeeded.WaitForUnpack;
			AssertNoErrors(QB.DeliveryEquipmentInfo);
		}

		public void TestOneOffQuoteTransportModeAndContainerMode()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Validation.ValidateAll();
			AssertHasErrors(quotedBooking.TransportModeInfo);
			AssertHasErrors(quotedBooking.ContainerModeInfo);
			AssertHasErrors(quotedBooking.ModeInfo);

			quotedBooking.ContainerMode = "LSE";
			AssertHasErrors(quotedBooking.TransportModeInfo);
			AssertHasErrors(quotedBooking.ContainerModeInfo);
			AssertHasErrors(quotedBooking.ModeInfo);

			quotedBooking.TransportMode = "AIR";
			AssertNoErrors(quotedBooking.TransportModeInfo);
			AssertNoErrors(quotedBooking.ContainerModeInfo);
			AssertNoErrors(quotedBooking.ModeInfo);

			quotedBooking.ContainerMode = "LSE";
			AssertNoErrors(quotedBooking.TransportModeInfo);
			AssertNoErrors(quotedBooking.ContainerModeInfo);
			AssertNoErrors(quotedBooking.ModeInfo);

			quotedBooking.TransportMode = "XXX";
			AssertHasErrors(quotedBooking.TransportModeInfo);
			AssertHasErrors(quotedBooking.ContainerModeInfo);
			AssertHasErrors(quotedBooking.ModeInfo);
		}

		public void TestContainerPackModeOverride()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.TransportMode = "AIR";
			quotedBooking.Validation.ValidateAll();
			AssertNoErrors(quotedBooking.ContainerPackModeOverrideInfo);

			quotedBooking.ContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			AssertNoErrors(quotedBooking.ContainerPackModeOverrideInfo);

			quotedBooking.ContainerPackModeOverride = "ToMyHome";
			AssertHasErrors(quotedBooking.ContainerPackModeOverrideInfo);
		}

		public void TestModeForBookingWithQuote()
		{
			QB.IsCompareMode = ZBool.False;
			QB.Mode = ZString.Empty;
			QB.Validation.ValidateAll();
			AssertHasErrors(QB.ModeInfo);
			AssertHasErrors(QB.TransportModeInfo);
			QB.IsCompareMode = ZBool.True;
			AssertEquals(0, QB.SelectedComparisonModes.Count);
			QB.Validation.ValidateAll();
			AssertHasErrors(QB.ModeInfo);
			AssertHasErrors(QB.TransportModeInfo);
			QB.ComparisonModes[0].Bool = ZBool.True;
			AssertNotEquals(0, QB.SelectedComparisonModes.Count);
			QB.Validation.ValidateAll();
			AssertNoErrors(QB.ModeInfo);
			AssertNoErrors(QB.TransportModeInfo);
		}

		public void TestCheckJS_A_RCV_CanBeInTheFuture()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);

			var pickupCFSAddress = Factory.New<OrgHeader>();
			pickupCFSAddress.OH_Code = "CNRORG";
			pickupCFSAddress.MainAddress.OA_Address1 = "Main st";
			pickupCFSAddress.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			var pickupCFSPort = ((ILocation)pickupCFSAddress.MainAddress).UNLOCO;
			booking.JS_OA_ExportReceivingDepot = pickupCFSAddress.MainAddress.PK;
			booking.JS_A_RCV = pickupCFSPort.LocationDateTime.AddMinutes(1);
			AssertNoError("For booking this validation shouldn't be applied", booking.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.MainAddress.PK;
			shipment.JS_A_RCV = pickupCFSPort.LocationDateTime.AddMinutes(1);
			AssertHasError("Only for shipment this validation should be applied", shipment.JS_A_RCVInfo, "Interim Receipt Date cannot be in the future.");
		}

		public void TestCheckJS_DeliveryDueDate_Warning_PickupRequiredBy_Scheduled()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var booking = QuotedBooking.CreateNewBooking(Factory);
				booking.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertCheckHasValidDate(booking, Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
					booking.DocsAndCartage.JP_PickupRequiredByInfo,
					"Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var booking2 = QuotedBooking.CreateNewBooking(Factory);
				booking2.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertCheckHasValidDate(booking2, Constants.HBLDeliveryModes.Codes.DOOR_CFS,
					booking2.DocsAndCartage.JP_PickupRequiredByInfo,
					"Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var booking3 = QuotedBooking.CreateNewBooking(Factory);
				booking3.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertCheckHasValidDate(booking3, Constants.HBLDeliveryModes.Codes.CFS_DOOR,
					booking3.DocsAndCartage.JP_PickupRequiredByInfo,
					"Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var booking4 = QuotedBooking.CreateNewBooking(Factory);
				booking4.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertCheckHasValidDate(booking4, Constants.HBLDeliveryModes.Codes.CFS_CFS,
					booking4.DocsAndCartage.JP_PickupRequiredByInfo,
					"Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");

				var booking5 = QuotedBooking.CreateNewBooking(Factory);
				booking5.JS_TransportMode = Core.Constants.TransportModes.Air;
				booking5.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.ARPT_ARPT;
				booking5.JS_A_RCV = ZDateTime.Empty;
				booking5.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
				booking5.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
				booking5.Validation.ValidateJS_DeliveryDueDate();
				AssertNoWarning(booking5.JS_DeliveryDueDateInfo, "Pickup Required By is mandatory for DDD (Delivery Due Date) calculation.");
			}
		}

		void AssertCheckHasValidDate(ForwardingShipment shipment, ZString deliveryMode, ZPropertyInfo valueField, ZString expectedMessage)
		{
			shipment.JS_HBLContainerPackModeOverride = deliveryMode;
			valueField.Value = ZDateTime.Empty;
			shipment.Validation.ValidateJS_DeliveryDueDate();
			AssertHasWarning(shipment.JS_DeliveryDueDateInfo, expectedMessage);

			valueField.Value = ZDateTime.Today;
			shipment.Validation.ValidateJS_DeliveryDueDate();
			AssertNoWarning(shipment.JS_DeliveryDueDateInfo, expectedMessage);
		}

		public void TestShipmentStatus()
		{
			QB.ShipmentStatus = "XYZ";
			AssertHasError(QB.ShipmentStatusInfo, "Enter a valid Status.");
			QB.ShipmentStatus = ZString.Empty;
			AssertNoError(QB.ShipmentStatusInfo, "Enter a valid Status.");
			QB.ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertHasError(QB.ShipmentStatusInfo, "Enter a valid Status.");
			QB.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertNoError(QB.ShipmentStatusInfo, "Enter a valid Status.");
			QB.ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			AssertHasError("Web status cannot be manually selected", QB.ShipmentStatusInfo, "Enter a valid Status.");
			Factory.Save();
			QB.Validation.ValidateShipmentStatus();
			AssertNoError("Web status is OK if booking already saved", QB.ShipmentStatusInfo, "Enter a valid Status.");
		}

		public void TestFrequencyUnit()
		{
			QB.FrequencyUnit = "xtr";
			QB.Validation.ValidateAll();
			AssertHasErrors(QB.FrequencyUnitInfo);
			QB.Mode = "LCL";
			QB.FrequencyUnit = QB.FrequencyUnits[1].Code;
			QB.Validation.ValidateAll();
			AssertNoErrors(QB.FrequencyUnitInfo);
		}

		public void TestFrequency()
		{
			QB.Frequency = 0;
			QB.Validation.ValidateAll();
			AssertNoErrors(QB.FrequencyInfo);
			QB.Mode = "LCL";
			QB.Quote.CurrentOneOffQuote.TT_TransportMode = "SEA";
			QB.Quote.CurrentOneOffQuote.TT_ContainerMode = "LCL";
			QB.FrequencyUnit = QB.FrequencyUnits[1].Code;
			QB.Frequency = 0;
			QB.Validation.ValidateAll();
			AssertHasErrors(QB.FrequencyInfo);
			QB.Frequency = 12;
			QB.Validation.ValidateAll();
			AssertNoErrors(QB.FrequencyInfo);
		}

		public void TestTransitTime()
		{
			QB.Mode = "LCL";
			QB.Quote.CurrentOneOffQuote.TT_TransportMode = "SEA";
			QB.Quote.CurrentOneOffQuote.TT_ContainerMode = "LCL";
			QB.TransitTime = "xtr";
			QB.Validation.ValidateAll();
			AssertHasErrors(QB.TransitTimeInfo);
			QB.TransitTime = QB.TransitTimesList[1].Code;
			QB.Validation.ValidateAll();
			AssertNoErrors(QB.TransitTimeInfo);
		}

		public void TestWight()
		{
			AssertEquals("Pre-Condition", QuotedBookingState.AcceptedBookingWithQuote, QB.ObjectState);
			QB.Mode = "FCL";
			QB.Weight = 0m;
			QB.WeightUnit = Core.Constants.Weight.Kilograms;
			QB.Volume = 0m;
			QB.VolumeUnit = Core.Constants.Volume.CubicMetres;
			QB.Validation.ValidateAll();
			AssertNoErrors("Should not have any error because Weight and volume are mandatory only when mode is LCL or LSE", QB.WeightInfo);
			QB.Mode = "LCL";
			QB.Validation.ValidateAll();
			AssertHasError("There should be an error Since Mode is LCL and both weight and volume are zero", QB.WeightInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");
			QB.Volume = 10m;
			QB.Validation.ValidateAll();
			AssertNoErrors("Should not have error since volume has value", QB.WeightInfo);
			QB.Mode = "LSE";
			QB.Volume = 0m;
			QB.Weight = 0m;
			QB.Validation.ValidateAll();
			AssertHasError("Should have error since neither weight nor volume has value", QB.WeightInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");
			QB.Volume = 6m;
			QB.Weight = 10000m;
			var outer = (PackLine)QB.Booking.OuterPackLines[0];
			outer.JL_PackageCount = 2;
			outer.JL_ActualVolume = 5m;
			outer.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			outer.JL_ActualWeight = 2500m;
			outer.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			QB.Validation.ValidateAll();
			AssertHasError(QB.WeightInfo, "The weight you have specified does not match the details specified for Loose Cargo. Please check your calculations.");
			outer.JL_ActualWeight = 10000m;
			QB.Validation.ValidateAll();
			AssertNoErrors("Weight matches with Loose Cargo details", QB.WeightInfo);
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Mode = "LCL";
			quickBooking.Weight = 0m;
			quickBooking.Volume = 0m;
			quickBooking.Validation.ValidateAll();
			AssertNoErrors("Should not have error since this a quick booking and doesn't have any quote and weight and volume are mandatory only when there is a quote", quickBooking.VolumeInfo);
			AssertHasWarning("should apply shipment validation", quickBooking.VolumeInfo, "You have not entered a Shipment Volume.");
		}

		public void TestVolume()
		{
			AssertEquals("Pre-Condition", QuotedBookingState.AcceptedBookingWithQuote, QB.ObjectState);
			QB.Mode = "ULD";
			QB.Weight = 0m;
			QB.WeightUnit = Core.Constants.Weight.Kilograms;
			QB.Volume = 0m;
			QB.VolumeUnit = Core.Constants.Volume.CubicMetres;
			QB.Validation.ValidateAll();
			AssertNoErrors("Should not have any error because Weight and volume are mandatory only when mode is LCL or LSE", QB.VolumeInfo);
			QB.Mode = "LSE";
			QB.Validation.ValidateAll();
			AssertHasError("There should be an error Since Mode is LSE and both weight and volume are zero", QB.VolumeInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");
			QB.Weight = 20m;
			QB.Validation.ValidateAll();
			AssertNoErrors("Should not have error since volume has value", QB.VolumeInfo);
			QB.Mode = "LCL";
			QB.Volume = 0m;
			QB.Weight = 0m;
			QB.Validation.ValidateAll();
			AssertHasError("Should have error since neither weight nor volume has value", QB.VolumeInfo, "You must specify either a weight or volume if your shipment is LSE or LCL.");
			QB.Volume = 10m;
			QB.Weight = 5000m;
			var outer = (PackLine)QB.Booking.OuterPackLines[0];
			outer.JL_PackageCount = 1;
			outer.JL_ActualVolume = 5m;
			outer.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			outer.JL_ActualWeight = 100m;
			outer.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			QB.Validation.ValidateAll();
			AssertHasError(QB.VolumeInfo, "The volume you have specified does not match the details specified for Loose Cargo. Please check your calculations.");
			outer.JL_ActualVolume = 10m;
			QB.Validation.ValidateAll();
			AssertNoErrors("Volume matches with Loose Cargo details", QB.VolumeInfo);
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.Mode = "LSE";
			quickBooking.Weight = 0m;
			quickBooking.Volume = 0m;
			quickBooking.Validation.ValidateAll();
			AssertNoErrors("Should not have error since this a quick booking and doesn't have any quote and weight and volume are mandatory only when there is a quote", quickBooking.VolumeInfo);
			AssertHasWarning("should apply shipment validation", quickBooking.VolumeInfo, "You have not entered a Shipment Volume.");
		}

		#region TestValidatePaymentTerms
		[TestDate(2020, 1, 1)]
		public void TestValidatePaymentTermsQuote_2020()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertValidatePaymentTerms(quotedBooking, "Enter a valid Quotation Incoterm.");
		}

		[TestDate(2020, 1, 1)]
		public void TestValidatePaymentTermsBooking_2020()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			AssertValidatePaymentTerms(quotedBooking, "Enter a valid Incoterm.");
		}

		[TestDate(2020, 1, 1)]
		public void TestValidatePaymentTermsQuotedBooking_2020()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			AssertValidatePaymentTerms(quotedBooking, "Enter a valid Incoterm.");
		}

		void AssertValidatePaymentTerms(QuotedBooking quotedBooking, string invalidErrorMessage)
		{
			quotedBooking.PaymentTerms = "XXX";
			AssertHasError(quotedBooking.PaymentTermsInfo, invalidErrorMessage);
			quotedBooking.PaymentTerms = "DDU";
			AssertHasError(quotedBooking.PaymentTermsInfo, invalidErrorMessage);
			quotedBooking.PaymentTerms = "DAT";
			AssertHasWarning(quotedBooking.PaymentTermsInfo, "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules.");
			quotedBooking.PaymentTerms = "FOB";
			AssertNoNotifications(quotedBooking.PaymentTermsInfo);
		}

		#endregion
		public void TestOH_Carrier()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			Factory.Save();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.OH_Carrier = org.PK;
			booking.Validation.ValidateOH_Carrier();
			AssertHasError(booking.OH_CarrierInfo, "Enter a valid Carrier.");
			booking.OH_Carrier = carrier.PK;
			booking.Validation.ValidateOH_Carrier();
			AssertNoErrors(booking.OH_CarrierInfo);
			var quote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quote.OH_Carrier = org.PK;
			quote.Validation.ValidateOH_Carrier();
			AssertHasError(quote.OH_CarrierInfo, "Enter a valid Carrier.");
			quote.OH_Carrier = carrier.PK;
			quote.Validation.ValidateOH_Carrier();
			AssertNoErrors(quote.OH_CarrierInfo);
			QB.OH_Carrier = org.PK;
			QB.Validation.ValidateOH_Carrier();
			AssertHasError(QB.OH_CarrierInfo, "Enter a valid Carrier.");
			QB.OH_Carrier = carrier.PK;
			QB.Validation.ValidateOH_Carrier();
			AssertNoErrors(QB.OH_CarrierInfo);
		}

		public void TestValidateCarrierServiceLevel()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingProvider = true;
			var slv1 = carrier1.MiscServ.CarrierServiceLevels.AddNew();
			slv1.PL_Code = "SL1";
			slv1.PL_CarrierServiceLevelDescription = "SL1";
			var slv2 = carrier1.MiscServ.CarrierServiceLevels.AddNew();
			slv2.PL_Code = "SL2";
			slv2.PL_CarrierServiceLevelDescription = "SL2";
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingProvider = true;
			Factory.Save();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.OH_Carrier = carrier1.PK;
			booking.CarrierServiceLevel = "SL1";
			booking.Validation.ValidateCarrierServiceLevel();
			AssertNoErrors(booking.CarrierServiceLevelInfo);
			booking.CarrierServiceLevel = "SL2";
			booking.Validation.ValidateCarrierServiceLevel();
			AssertNoErrors(booking.CarrierServiceLevelInfo);
			booking.CarrierServiceLevel = "SL3";
			booking.Validation.ValidateCarrierServiceLevel();
			AssertHasError(booking.CarrierServiceLevelInfo, "Enter a valid Carrier Service Level.");
			booking.OH_Carrier = carrier2.PK;
			booking.CarrierServiceLevel = "SL1";
			booking.Validation.ValidateCarrierServiceLevel();
			AssertHasError(booking.CarrierServiceLevelInfo, "Enter a valid Carrier Service Level.");
		}

		public void TestIsForwardRegistered()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Validation.ValidateIsForwardRegistered();
			Assert("Precondition", !booking.IsForwardRegistered);
			AssertNoErrors(booking.IsForwardRegisteredInfo);
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var shipment = anotherFactory.Load<CommonShipment>(booking.Booking.PK);
			shipment.JS_IsForwardRegistered = true;
			anotherFactory.Save();
			booking.Validation.ValidateIsForwardRegistered();
			Assert("IsForwardRegistered is set to true", booking.IsForwardRegistered);
			AssertHasError(booking.IsForwardRegisteredInfo, "The Quick Booking has been Consolidated and can no longer be edited.");
		}

		public void TestCheckHBLAWBChargesDisplay()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quickBooking.HBLAWBChargesDisplay = "PPD";
			AssertNoErrors(quickBooking.HBLAWBChargesDisplayInfo);
			AssertNoErrors(quickBooking.Booking.JS_HBLAWBChargesDisplayInfo);
			quickBooking.HBLAWBChargesDisplay = "ABC";
			AssertHasErrors(quickBooking.HBLAWBChargesDisplayInfo);
			AssertHasErrors(quickBooking.Booking.JS_HBLAWBChargesDisplayInfo);
		}

		public void TestLoadAndDischargePortValidation_SamePort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.LoadPort = "AUSYD";
			booking.DischargePort = "AUSYD";
			booking.Validation.ValidateAll();
			AssertHasError(booking.LoadPortInfo, "Load and Discharge Ports cannot be the same.");
			AssertHasError(booking.DischargePortInfo, "Load and Discharge Ports cannot be the same.");
		}

		public void TestLoadPortValidation_IsSea_NoSeaPort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var notSeaportUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasSeaport, false));
			booking.LoadPort = notSeaportUnloco.Code;
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Validation.ValidateAll();
			AssertHasWarning(booking.LoadPortInfo, $"{notSeaportUnloco.Code} does not have a sea port.");
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			booking.Validation.ValidateAll();
			AssertNoWarning(booking.LoadPortInfo, $"{notSeaportUnloco.Code} does not have a sea port.");
		}

		public void TestLoadPortValidation_IsAir_NoAirPort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var notAirportUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, false));
			booking.LoadPort = notAirportUnloco.Code;
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			booking.Validation.ValidateAll();
			AssertHasWarning(booking.LoadPortInfo, $"{notAirportUnloco.Code} does not have an air port.");
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Validation.ValidateAll();
			AssertNoWarning(booking.LoadPortInfo, $"{notAirportUnloco.Code} does not have an air port.");
		}

		public void TestDischargePortValidation_IsSea_NoSeaPort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var notSeaportUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasSeaport, false));
			booking.DischargePort = notSeaportUnloco.Code;
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Validation.ValidateAll();
			AssertHasWarning(booking.DischargePortInfo, $"{notSeaportUnloco.Code} does not have a sea port.");
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			booking.Validation.ValidateAll();
			AssertNoWarning(booking.DischargePortInfo, $"{notSeaportUnloco.Code} does not have a sea port.");
		}

		public void TestDischargePortValidation_IsAir_NoAirPort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var notAirportUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_HasAirport, false));
			booking.DischargePort = notAirportUnloco.Code;
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			booking.Validation.ValidateAll();
			AssertHasWarning(booking.DischargePortInfo, $"{notAirportUnloco.Code} does not have an air port.");
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Validation.ValidateAll();
			AssertNoWarning(booking.DischargePortInfo, $"{notAirportUnloco.Code} does not have an air port.");
		}

		public void TestETDAndETAValidation()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.ETD = new DateTime(2019, 5, 2);
			booking.ETA = new DateTime(2019, 5, 6);
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var origin = Factory.Load<VoyageOrigin>(sailing.JX_JA);
			var destination = Factory.Load<VoyageDestination>(sailing.JX_JB);
			origin.JA_E_DEP = new DateTime(2019, 5, 1);
			destination.JB_E_ARV = new DateTime(2019, 5, 7);
			Factory.Save();
			((ISailingChooserParent)booking).SailingJX = sailing.PK;
			booking.Validation.ValidateAll();
			AssertHasError(booking.ETDInfo, "Sailing Schedule ETD cannot be earlier than Shipment ETD. Please correct.");
			AssertHasWarning(booking.ETAInfo, "Shipment ETA is earlier than Sailing Schedule ETA. Are you sure you want to continue?");
			booking.ETD = new DateTime(2019, 4, 30);
			booking.ETA = new DateTime(2019, 5, 8);
			booking.Validation.ValidateAll();
			AssertNoError(booking.ETDInfo, "Sailing Schedule ETD cannot be earlier than Shipment ETD. Please correct.");
			AssertNoWarning(booking.ETAInfo, "Shipment ETA is earlier than Sailing Schedule ETA. Are you sure you want to continue?");
		}

		public void TestValidateHBLAWBChargesDisplay_HandlesNullBooking()
		{
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking1 = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);
			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking2 = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking2 = QuotedBooking.New(quote2.PK, booking2.PK, Factory);
			AssertNoExceptionThrown(() => quotedBooking1.Validation.ValidateHBLAWBChargesDisplay());
			AssertNoExceptionThrown(() => quotedBooking2.Validation.ValidateHBLAWBChargesDisplay());
		}

		public void TestDeliveryDueDate()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var deliveryDueDateCalculatorManagerMock = DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(ZDateTime.Today);
				deliveryDueDateCalculatorManagerMock.Setup(m => m.Calculate(It.IsAny<ForwardingShipment>())).Returns(DeliveryDueDateCalculationResult.Failure("Something went wrong", ZString.Empty));
				var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				booking.TransportMode = Core.Constants.TransportModes.Air;
				booking.Booking.CalculateDeliveryDueDate();
				booking.Validation.ValidateDeliveryDueDate();
				AssertHasWarning(booking.DeliveryDueDateInfo, "Something went wrong");
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		public void TestValidateCompanyTariffLevel()
		{
			Factory.New<GlobalTariff>();
			Factory.New<GlobalTariff>();
			Factory.Save();

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
				quotedBooking.CompanyTariffLevel = "2";
				AssertNoErrors("If Company Tariff Level Override is disabled, we will not check Company Tariff Level Override.", quotedBooking.CompanyTariffLevelInfo);
				quotedBooking.CompanyTariffLevel = "3";
				AssertNoErrors("If Company Tariff Level Override is disabled, we will not check Company Tariff Level Override.", quotedBooking.CompanyTariffLevelInfo);
			}

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
				quotedBooking.CompanyTariffLevel = "2";
				AssertNoErrors("Company Tariff Level Override should not exceed the max level of Company Tariff.", quotedBooking.CompanyTariffLevelInfo);
				quotedBooking.CompanyTariffLevel = "3";
				AssertHasErrors("Company Tariff Level Override should not exceed the max level of Company Tariff.", quotedBooking.CompanyTariffLevelInfo);
			}
		}

		#region Booking-CCA Validation

		#region Contracts

		public void TestValidateBookingAndContractsNumberDisabledRegistry()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.CarrierContractNumber = "INVALID";
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertHasWarning(quotedBooking.CarrierContractNumberInfo, "This number does not have a corresponding Carrier Contract & Allocations record.");
		}

		public void TestValidateBookingAndContractsNumber()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.CarrierContractNumber = "INVALID";
			AssertHasWarning(quotedBooking.CarrierContractNumberInfo, "This number does not have a corresponding Carrier Contract & Allocations record.");

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
			AssertNoWarning(quotedBooking.CarrierContractNumberInfo, "This number does not have a corresponding Carrier Contract & Allocations record.");
		}

		public void TestValidateBookingAndContractsETD()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory); 
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			contract.RCT_StartDate = ZDate.Today.AddDays(-5);
			contract.RCT_EndDate = ZDate.Today.AddDays(5);

			quotedBooking.ETD = ZDate.Today.AddDays(-10);
			quotedBooking.Validation.ValidateCarrierContractNumber();
			var expectedLateMessage = $"Start Date ({contract.RCT_StartDate.ToString()}) of Carrier Contract {contract.RCT_ContractNumber} is later than the ETD ({quotedBooking.ETD.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Carrier Contract.";

			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedLateMessage);

			quotedBooking.ETD = ZDate.Today.AddDays(10);
			quotedBooking.Validation.ValidateCarrierContractNumber();
			var expectedEarlyMessage = $"Expiry Date ({contract.RCT_EndDate.ToString()}) of Carrier Contract {contract.RCT_ContractNumber} is earlier than the ETD ({quotedBooking.ETD.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Carrier Contract.";
			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedEarlyMessage);

			quotedBooking.ETD = ZDate.Today.AddDays(-5);
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedLateMessage);
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedEarlyMessage);

			quotedBooking.ETD = ZDate.Today;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedLateMessage);
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedEarlyMessage);
		}

		public void TestValidateBookingAndContractsMode()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			quotedBooking.Mode = Core.Constants.RateMode.LRO;
			contract.RCT_TransportMode = Core.Constants.TransportModes.Air;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			var expectedMessage = $"Mode '{quotedBooking.Mode}' of this Booking is NOT under or aligned with the Transport mode '{contract.RCT_TransportMode}' of Carrier Contract for allocation.";
			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			quotedBooking.Mode = Constants.RateMode.ULD;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);
		}

		public void TestValidateBookingAndContractsCarrier()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var allocationRoute = contract.Allocations.AddNew();
			quotedBooking.AllocationLinePK = allocationRoute.PK;

			quotedBooking.OH_Carrier = org1.PK;
			contract.RCT_OH = org2.PK;
			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
			var expectedMessage = $"Carrier '{quotedBooking.Carrier.OH_Code}' of Booking does not match Service Provider(s) of any Carrier Contract(s) with this number.";

			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedMessage);
			quotedBooking.OH_Carrier = org2.PK;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);
		}

		public void TestValidateBookingAndContractsCustomer()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var customer1 = SetQuotedBookingControllingCustomerForTest(quotedBooking);
			var customer2 = Factory.NewWithValidTestData<OrgHeader>();
			var expectedMessage = $"At least one of the Booking clients should match a Named Account of Carrier Contract {contract.RCT_ContractNumber} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";

			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoWarning(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			contract.NamedAccountPivots.AddChild(customer2);
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertHasWarning(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			quotedBooking.ControllingCustomerDocumentaryAddress.OrganisationPK = customer2.PK;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoWarning(quotedBooking.CarrierContractNumberInfo, expectedMessage);
		}

		public OrgHeader SetQuotedBookingControllingCustomerForTest(QuotedBooking quotedBooking)
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ControllingCustomerDocumentaryAddress.OrganisationPK = customer.PK;
			return customer;
		}

		public void TestValidateBookingAndContractsContainerType()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			quotedBooking.Booking.JS_UniqueConsignRef = "CONEHEAD";
			var commonValidationData = quotedBooking as ICCACommonAssignmentValidationData;

			var expectedMessage = $"NOT all Containers on Booking {commonValidationData.UniqueConsignRef} share the same Container Type (DRY) of the selected Carrier Contract {contract.RCT_ContractNumber}. Booking Container(s) and selected Carrier Contract should share the same Container Type.";

			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			var refcontainer1 = Factory.NewWithValidTestData<RefContainer>();
			container1.JC_RC = refcontainer1.PK;
			container1.RefContainer.RC_ContainerType = "TOP";

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			var refcontainer2 = Factory.NewWithValidTestData<RefContainer>();
			container2.JC_RC = refcontainer2.PK;
			container2.RefContainer.RC_ContainerType = "TOP";

			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			contract.RCT_ContainerType = "DRY";
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			container1.RefContainer.RC_ContainerType = "DRY";
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			container2.RefContainer.RC_ContainerType = "DRY";
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);
		}

		public void TestValidateBookingAndContractsHazardous()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupQuotedBookingContract(quotedBooking, contract);

			contract.RCT_AllowHazardousCommodities = false;

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			var refCommodityCode1 = Factory.NewWithValidTestData<RefCommodityCode>();
			container1.JC_RH_NKContainerCommodityCode = refCommodityCode1.RH_Code;
			container1.ContainerCommodityCode.RH_IsHazardous = false;

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			var refCommodityCode2 = Factory.NewWithValidTestData<RefCommodityCode>();
			container2.JC_RH_NKContainerCommodityCode = refCommodityCode2.RH_Code;
			container2.ContainerCommodityCode.RH_IsHazardous = false;
			container2.JC_ContainerNum = "KOALA";

			var expectedMessage = $"Hazardous Commodities are not allowed for Carrier Contract {contract.RCT_ContractNumber} but Container {container2.JC_ContainerNum} has Commodity {container2.ContainerCommodityCode.RH_Code} with 'Is this Commodity Hazardous' checked. Only Booking(s) and Container(s) without Hazardous Commodities can be allocated.";
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			container2.ContainerCommodityCode.RH_IsHazardous = true;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			var actual = quotedBooking.CarrierContractNumberInfo.Notifications.First().Message;

			var check = expectedMessage == actual;
			AssertHasError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			contract.RCT_AllowHazardousCommodities = true;
			quotedBooking.Validation.ValidateCarrierContractNumber();
			AssertNoError(quotedBooking.CarrierContractNumberInfo, expectedMessage);

			quotedBooking.QuotedBookingContainers.FirstOrDefault(container => ((ForwardingContainer)container).ContainerCommodityCode?.RH_IsHazardous ?? false);
		}

		#endregion

		#region AllocationRoute

		public void TestValidateAllocationLinePK_RestrictsValidationsIfNoParentContract()
		{
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "SHREKTRACT";

			var route = contract.Allocations.AddNew();
			route.RCA_StorageOrFreightRateClass = "DNKY";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RC = Factory.New<RefContainer>().PK;
			SetupQuotedBookingContract(quotedBooking, contract);

			quotedBooking.AllocationLinePK = route.PK;
			AssertHasErrorContaining(quotedBooking.AllocationLinePKInfo, "should match the selected Allocation Route's Container Class");

			quotedBooking.CarrierContractNumber = "beep boop";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoErrorContaining(quotedBooking.AllocationLinePKInfo, "should match the selected Allocation Route's Container Class");
		}

		public void TestValidateBookingAllocationRouteInContract()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var invalidAllocation = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			quotedBooking.CarrierContractNumber = ZString.Empty;
			quotedBooking.AllocationLinePK = invalidAllocation.PK;
			AssertHasError(quotedBooking.AllocationLinePKInfo, "This Booking doesn't have any Carrier Contracts allocated. Allocation Route ID selected for the Booking and/or its Containers should be from the same Carrier Contract that this Booking is allocated to.");

			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
			var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
			quotedBooking.AllocationLinePK = nonExistentAllocationLineGuid;
			AssertHasError(quotedBooking.AllocationLinePKInfo, $"Allocation Route ID selected for the Booking and/or its Containers should be valid and from the same Carrier Contract ({contract.RCT_ContractNumber}) that this Booking is allocated to.");

			quotedBooking.AllocationLinePK = invalidAllocation.PK;
			var expectedMessage = $"Allocation Route ID selected for the Booking and/or its Containers should be from the same Carrier Contract ({quotedBooking.CarrierContractNumber}) that this Booking is allocated to.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedMessage);

			var validAllocation = contract.Allocations.AddNew();
			quotedBooking.AllocationLinePK = validAllocation.PK;
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedMessage);
		}

		public void TestValidateBookingAllocationRouteETD()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			quotedBooking.AllocationLinePK = allocationRoute.PK;

			quotedBooking.ETD = ZDate.Today.AddDays(-10);
			quotedBooking.Validation.ValidateAllocationLinePK();
			var expectedLateMessage = $"Start Date ({allocationRoute.RCA_StartDate.ToString()}) of Allocation Route {allocationRoute.RCA_AllocationLineID} is later than the ETD ({quotedBooking.ETD.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedLateMessage);

			quotedBooking.ETD = ZDate.Today.AddDays(10);
			quotedBooking.Validation.ValidateAllocationLinePK();
			var expectedEarlyMessage = $"Expiry Date ({allocationRoute.RCA_ExpiryDate.ToString()}) of Allocation Route {allocationRoute.RCA_AllocationLineID} is earlier than the ETD ({quotedBooking.ETD.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedEarlyMessage);

			quotedBooking.ETD = ZDate.Today.AddDays(-5);
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedLateMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedEarlyMessage);

			quotedBooking.ETD = ZDate.Today;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedLateMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedEarlyMessage);
		}

		public void TestValidateBookingAllocationPorts()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();

			quotedBooking.LoadPort = "AUSYD";
			allocationRoute.RCA_LoadLocation = "NZAKL";

			quotedBooking.AllocationLinePK = allocationRoute.PK;
			var loadMessage = $"Load Port ({allocationRoute.RCA_LoadLocation}) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Load Port ({quotedBooking.LoadPort}) of this Booking. Booking Load Port should match the selected Allocation Route's Load Port.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, loadMessage);

			quotedBooking.DischargePort = "NZAKL";
			allocationRoute.RCA_DischargeLocation = "AUSYD";
			quotedBooking.Validation.ValidateAllocationLinePK();
			var dischargeMessage = $"Discharge Port ({allocationRoute.RCA_DischargeLocation}) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Discharge Port ({quotedBooking.DischargePort}) of this Booking. Booking Discharge Port should match the selected Allocation Route's Discharge Port.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, dischargeMessage);

			quotedBooking.LoadPort = "NZAKL";
			quotedBooking.DischargePort = "AUSYD";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, loadMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, dischargeMessage);
		}

		public void TestValidateBookingAllocationVoyageNumber()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_VoyageNumber = ZString.Empty;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			quotedBooking.Booking.JS_JX = sailing.PK;
			var testJobVoyage = Factory.New<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "0007";
			var testJobVoyOrigin = Factory.New<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			sailing.JX_JA = testJobVoyOrigin.PK;

			quotedBooking.AllocationLinePK = allocationRoute.PK;
			var expectedMessage = $"Voyage (SHREK123) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Voyage ({sailing.JX_JV_VoyageFlight}) of this Booking. Booking Voyage should match the selected Allocation Route's Voyage.";
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedMessage);

			allocationRoute.RCA_VoyageNumber = "SHREK123";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedMessage);

			allocationRoute.RCA_VoyageNumber = "0007";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedMessage);
		}

		public void TestValidateBookingAllocationVessel()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_RV_NKVessel = ZString.Empty;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			quotedBooking.Booking.JS_JX = sailing.PK;
			var testJobVoyage = Factory.New<JobVoyage>();
			testJobVoyage.JV_RV_NKVessel = "GWEN";
			var testJobVoyOrigin = Factory.New<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			sailing.JX_JA = testJobVoyOrigin.PK;

			quotedBooking.AllocationLinePK = allocationRoute.PK;
			var expectedMessage = $"Vessel (MILES) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Vessel ({sailing.JX_JV_NKVessel}) of this Booking. Booking Vessel should match the selected Allocation Route's Vessel.";
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedMessage);

			allocationRoute.RCA_RV_NKVessel = "MILES";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedMessage);

			allocationRoute.RCA_RV_NKVessel = "GWEN";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedMessage);
		}

		public void TestValidateBookingAllocationContainer()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_RC_ContainerType = ZGuid.Empty;
			allocationRoute.RCA_StorageOrFreightRateClass = ZString.Empty;

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			var refcontainer1 = Factory.NewWithValidTestData<RefContainer>();
			container1.JC_RC = refcontainer1.PK;
			refcontainer1.RC_Code = "BROOKE";
			container1.RefContainer.RC_ContainerType = "KID";
			container1.RefContainer.RC_StorageClass = "XXX";
			container1.RefContainer.RC_FreightRateClass = "YYY";

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			var refcontainer2 = Factory.NewWithValidTestData<RefContainer>();
			container2.JC_RC = refcontainer2.PK;
			refcontainer2.RC_Code = "FRANKY";
			container2.RefContainer.RC_ContainerType = "LAW";
			container2.RefContainer.RC_StorageClass = "ZZZ";
			container2.RefContainer.RC_FreightRateClass = "WWW";

			quotedBooking.AllocationLinePK = allocationRoute.PK;
			var expectedTypeMessage = $"NOT all Containers on this Booking share the same Container Code (BROOKE) of the selected Allocation Route {allocationRoute.RCA_AllocationLineID}. Booking Container Codes should match the selected Allocation Route's Container Code.";
			var expectedClassMessage = $"NOT all Containers on this Booking share the same Container Class (YYY) of the selected Allocation Route {allocationRoute.RCA_AllocationLineID}. Container Class of Booking Containers should match the selected Allocation Route's Container Class.";
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedTypeMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedClassMessage);

			allocationRoute.RCA_StorageOrFreightRateClass = "YYY";
			quotedBooking.Validation.ValidateAllocationLinePK();
			expectedClassMessage = $"NOT all Containers on this Booking share the same Container Class ({allocationRoute.RCA_StorageOrFreightRateClass}) of the selected Allocation Route {allocationRoute.RCA_AllocationLineID}. Container Class of Booking Containers should match the selected Allocation Route's Container Class.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedClassMessage);

			allocationRoute.RCA_StorageOrFreightRateClass = "ZZZ";
			quotedBooking.Validation.ValidateAllocationLinePK();
			expectedClassMessage = $"NOT all Containers on this Booking share the same Container Class ({allocationRoute.RCA_StorageOrFreightRateClass}) of the selected Allocation Route {allocationRoute.RCA_AllocationLineID}. Container Class of Booking Containers should match the selected Allocation Route's Container Class.";
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedClassMessage);

			container1.RefContainer.RC_StorageClass = "ZZZ";
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedClassMessage);

			allocationRoute.RCA_StorageOrFreightRateClass = ZString.Empty;
			allocationRoute.RCA_RC_ContainerType = refcontainer1.PK;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedTypeMessage);

			container2.JC_RC = refcontainer1.PK;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedTypeMessage);
		}

		public void TestValidateBookingAllocationCustomer()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AAAH";
			var customer1 = SetQuotedBookingControllingCustomerForTest(quotedBooking);
			var customer2 = Factory.NewWithValidTestData<OrgHeader>();
			var expectedMessage = $"At least one of the Booking clients should match a Named Account of Carrier Contract {contract.RCT_ContractNumber} and Allocation Route {allocationRoute.RCA_AllocationLineID} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";

			quotedBooking.AllocationLinePK = allocationRoute.PK;
			AssertNoWarning(quotedBooking.AllocationLinePKInfo, expectedMessage);
			allocationRoute.NamedAccountPivots.AddChild(customer2);
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasWarning(quotedBooking.AllocationLinePKInfo, expectedMessage);

			quotedBooking.ControllingCustomerDocumentaryAddress.OrganisationPK = customer2.PK;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoWarning(quotedBooking.AllocationLinePKInfo, expectedMessage);
		}

		public void TestValidateBookingAllocationBookingLimit()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(quotedBooking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocatedQuantity = 2;
			allocationRoute.RCA_HasBookingLimit = true;
			quotedBooking.AllocationLinePK = allocationRoute.PK;

			var refcontainer1 = Factory.NewWithValidTestData<RefContainer>();
			refcontainer1.RC_TEU = 1;

			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			var expectedCNMessage = $"Number of Containers to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute.RCA_AllocationLineID} by 1. Number of Containers should be within the Booking Limit of the selected Allocation Route.";
			var expectedTEUMessage = $"Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute.RCA_AllocationLineID} by 1 TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.";

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			container1.JC_RC = refcontainer1.PK;
			container1.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedCNMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedTEUMessage);

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_RC = refcontainer1.PK;
			container2.JC_RCA_AllocationLine = allocationRoute.PK;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedCNMessage);
			AssertNoError(quotedBooking.AllocationLinePKInfo, expectedTEUMessage);

			var container3 = quotedBooking.QuotedBookingContainers.AddNew();
			container3.JC_RC = refcontainer1.PK;
			container3.JC_RCA_AllocationLine = allocationRoute.PK;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedCNMessage);

			allocationRoute.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
			quotedBooking.Validation.ValidateAllocationLinePK();
			AssertHasError(quotedBooking.AllocationLinePKInfo, expectedTEUMessage);
		}

		public void TestIsPlaceOfReceiptMatchLoadPort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupQuotedBookingContract(booking, contract);
			var allocationRoute = contract.Allocations.AddNew();

			booking.AllocationLinePK = allocationRoute.PK;
			booking.LoadPort = "AUSYD";
			booking.DischargePort = "CNSHA";
			booking.Booking.JS_UniqueConsignRef = "S0000001";
			allocationRoute.RCA_AllowRelatedPorts = true;

			var expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				allocationRoute.RCA_PlaceOfReceipt = "CNSHA";
				expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfReceipt = "CN";
				expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfReceipt = "AUSYD";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);

				allocationRoute.RCA_PlaceOfReceipt = "AU";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);
			}	
		}

		public void TestIsPlaceOfDelieveryMatchDischargePort()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var contract = Factory.NewWithValidTestData<RatingContract>();

			SetupQuotedBookingContract(booking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllowRelatedPorts = true;

			booking.AllocationLinePK = allocationRoute.PK;
			booking.LoadPort = "AUSYD";
			booking.DischargePort = "CNSHA";
			booking.Booking.JS_UniqueConsignRef = "S0000001";

			var expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				allocationRoute.RCA_PlaceOfDelivery = "AUSYD";
				expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfDelivery = "AU";
				expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfDelivery = "CNSHA";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);

				allocationRoute.RCA_PlaceOfDelivery = "CN";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);
			}	
		}

		public void TestIsPlaceOfReceiptMatchOrigin_LoadPortEmpty()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Origin = "AUSYD";
			booking.Destination = "CNSHA";
			booking.LoadPort = ZString.Empty;
			booking.DischargePort = ZString.Empty;
			var contract = Factory.NewWithValidTestData<RatingContract>();

			Assert("Precondition: Load Port should be empty", booking.LoadPort.IsEmpty);

			SetupQuotedBookingContract(booking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			booking.AllocationLinePK = allocationRoute.PK;
			booking.Booking.JS_UniqueConsignRef = "S0000001";
			allocationRoute.RCA_AllowRelatedPorts = true;

			var expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				allocationRoute.RCA_PlaceOfReceipt = "CNSHA";
				expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfReceipt = "CN";
				expectedErrorMessage = $"Mismatch: ‘Place of receipt’ ({allocationRoute.RCA_PlaceOfReceipt}) must match ‘Load port’ ({booking.LoadPort}) or ‘Origin’ ({booking.Origin}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfReceipt = "AUSYD";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);

				allocationRoute.RCA_PlaceOfReceipt = "AU";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);
			}
		}

		public void TestIsPlaceOfDelieveryMatchDestination_DischargePortEmpty()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Origin = "AUSYD";
			booking.Destination = "CNSHA";
			booking.LoadPort = string.Empty;
			booking.DischargePort = string.Empty;
			var contract = Factory.NewWithValidTestData<RatingContract>();

			Assert("Precondition: Discharge Port should be empty", booking.DischargePort.IsEmpty);

			SetupQuotedBookingContract(booking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			booking.AllocationLinePK = allocationRoute.PK;
			booking.Booking.JS_UniqueConsignRef = "S0000001";
			allocationRoute.RCA_AllowRelatedPorts = true;

			var expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				allocationRoute.RCA_PlaceOfDelivery = "AUSYD";
				expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfDelivery = "AU";
				expectedErrorMessage = $"Mismatch: ‘Place of delivery’ ({allocationRoute.RCA_PlaceOfDelivery}) must match ‘Discharge port’ ({booking.DischargePort}) or ‘Destination’ ({booking.Destination}) in ‘Allocated booking’ ({booking.Booking.JS_UniqueConsignRef}).";
				booking.Validation.ValidateAllocationLinePK();
				AssertHasError(booking.AllocationLinePKInfo, expectedErrorMessage);

				allocationRoute.RCA_PlaceOfDelivery = "CNSHA";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);

				allocationRoute.RCA_PlaceOfDelivery = "CN";
				booking.Validation.ValidateAllocationLinePK();
				AssertNoErrors(booking.AllocationLinePKInfo);
			}
		}

		void SetupQuotedBookingContract(QuotedBooking quotedBooking, RatingContract contract)
		{
			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.OH_Carrier = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;

			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
		}

		#endregion

		#endregion

		#region Implementation
		QuotedBooking QB
		{
			get
			{
				if (fQB == null)
				{
					fQB = QuotedBooking.New(QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
				}

				return fQB;
			}
		}

		QuotedBooking fQB;
		#endregion
	}
}
