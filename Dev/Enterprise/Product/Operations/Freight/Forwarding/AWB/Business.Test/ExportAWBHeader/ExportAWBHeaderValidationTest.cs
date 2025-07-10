using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	public class ExportAWBHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEH_AWBOriginCode()
		{
			AWBHeader.EH_AWBOriginCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBOriginCodeInfo);

			AWBHeader.EH_AWBOriginCode = "SY";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBOriginCodeInfo);

			AWBHeader.EH_AWBOriginCode = "123";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBOriginCodeInfo);

			AWBHeader.EH_AWBOriginCode = "SY(";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBOriginCodeInfo);

			AWBHeader.EH_AWBOriginCode = "SYD";
			AssertNoNotifications(AWBHeader.EH_AWBOriginCodeInfo);
		}

		public void TestCheckEH_AirportOfDestinationCode()
		{
			AWBHeader.EH_AirportOfDestinationCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AirportOfDestinationCodeInfo);

			AWBHeader.EH_AirportOfDestinationCode = "SY";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AirportOfDestinationCodeInfo);

			AWBHeader.EH_AirportOfDestinationCode = "123";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AirportOfDestinationCodeInfo);

			AWBHeader.EH_AirportOfDestinationCode = "SYD";
			AssertNoNotifications(AWBHeader.EH_AirportOfDestinationCodeInfo);
		}

		public void TestCheckEH_TotalGrossWeight()
		{
			AWBHeader.Validation.ValidateEH_TotalGrossWeight();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_TotalGrossWeightInfo);

			PopulateRateLine(0, "10", "K", 2980M, "U", "", 2400M, 7571M);
			AWBHeader.Validation.ValidateEH_TotalGrossWeight();
			AssertNoNotifications(AWBHeader.EH_TotalGrossWeightInfo);
		}

		void PopulateRateLine(int index, string noPieces, string weightUnit, ZDecimal grossWeight, string rateClass, string commodityItemNumber, ZDecimal chargeableWeight, ZDecimal rateChargeOrDiscount)
		{
			ExportAWBRateLine rateLine = AWBHeader.AWBRateLines[index];
			rateLine.ER_NoOfPiecesOrRCP = noPieces;
			rateLine.ER_WeightInLBsOrKGs = weightUnit;
			rateLine.ER_GrossWeight = grossWeight;
			rateLine.ER_RateClass = rateClass;
			rateLine.ER_CommodityItemNumber = commodityItemNumber;
			rateLine.ER_ChargeableWeight = chargeableWeight;
			rateLine.ER_RateChargeOrDiscount = rateChargeOrDiscount;
		}

		public void TestCheckEH_By1st()
		{
			const string messageError = "The code you have selected is not in the list.";

			AWBHeader.EH_By1st = "00";
			AssertHasMessageError(AWBHeader.EH_By1stInfo, messageError);

			AWBHeader.EH_By1st = "QF";
			AssertNoMessageError(AWBHeader.EH_By1stInfo, messageError);
		}

		public void TestCheckEH_TotalNoOfPieces()
		{
			AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "0";
			AWBHeader.Validation.ValidateEH_TotalNoOfPieces();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_TotalNoOfPiecesInfo);

			AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "6";
			AWBHeader.Validation.ValidateEH_TotalNoOfPieces();
			AssertNoNotifications(AWBHeader.EH_TotalNoOfPiecesInfo);
		}

		public void TestCheckEH_AsAgreed1st()
		{
			AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertNoNotifications(AWBHeader.EH_AsAgreed1stInfo);

			AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertNoNotifications(AWBHeader.EH_AsAgreed1stInfo);

			AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertNoNotifications(AWBHeader.EH_AsAgreed1stInfo);

			AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertHasErrors(AWBHeader.EH_AsAgreed1stInfo);

			AWBHeader.EH_AsAgreed1st = "AAA";
			AssertHasErrors(AWBHeader.EH_AsAgreed1stInfo);

			AWBHeader.EH_AsAgreed1st = "";
			AssertHasErrors(AWBHeader.EH_AsAgreed1stInfo);
		}

		public void TestCheckEH_AsAgreed2nd()
		{
			AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertNoNotifications(AWBHeader.EH_AsAgreed2ndInfo);

			AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertNoNotifications(AWBHeader.EH_AsAgreed2ndInfo);

			AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_AsAgreed2ndInfo);

			AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertHasErrors(AWBHeader.EH_AsAgreed2ndInfo);

			AWBHeader.EH_AsAgreed2nd = "AAA";
			AssertHasErrors(AWBHeader.EH_AsAgreed2ndInfo);

			AWBHeader.EH_AsAgreed2nd = "";
			AssertHasErrors(AWBHeader.EH_AsAgreed2ndInfo);
		}

		public void TestCheckEH_AgentApprovalNumber()
		{
			AWBHeader.EH_AgentApprovalNumber = "";

			var line = AWBHeader.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			line.EAS_ApprovalNumber = "000";

			AssertNoMessageError(AWBHeader.EH_AgentApprovalNumberInfo, "Regulated Agent Identifier is same as the Approval Number of the Known Party.");

			line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			AWBHeader.EH_AgentApprovalNumber = "000";

			AWBHeader.Validation.ValidateEH_AgentApprovalNumber();
			AssertHasMessageError(AWBHeader.EH_AgentApprovalNumberInfo, "Regulated Agent Identifier is same as the Approval Number of the Known Party.");
		}

		#region Shipper

		public virtual void TestCheckEH_ShipperCountryCode()
		{
			AWBHeader.EH_ShipperCountryCode = "12";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "A(";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "A";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "AU";
			AssertNoNotifications(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "";
			AssertNoNotifications(AWBHeader.EH_ShipperCountryCodeInfo);
		}

		public void TestCheckEH_ShipperContactCode()
		{
			AWBHeader.EH_ShipperContactDetail = "Description";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperContactCodeInfo);

			AWBHeader.EH_ShipperContactCode = "TEL";
			AssertHasNotifications(AWBHeader.EH_ShipperContactCodeInfo);

			AWBHeader.EH_ShipperContactCode = "TE";
			AssertNoNotifications(AWBHeader.EH_ShipperContactCodeInfo);

			AWBHeader.EH_ShipperContactCode = "#*-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperContactCodeInfo);

			AWBHeader.EH_ShipperContactDetail = "";
			AWBHeader.EH_ShipperContactCode = "";
			AssertNoNotifications(AWBHeader.EH_ShipperContactCodeInfo);
		}

		public void TestCheckEH_ShipperContactDetail()
		{
			AWBHeader.EH_ShipperContactCode = "ABC";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperContactDetailInfo);

			AWBHeader.EH_ShipperContactDetail = "Description";
			AssertNoNotifications(AWBHeader.EH_ShipperContactDetailInfo);

			AWBHeader.EH_ShipperContactDetail = "&*@*!$-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperContactDetailInfo);

			AWBHeader.EH_ShipperContactCode = "";
			AssertNoNotifications(AWBHeader.EH_ShipperContactDetailInfo);
		}

		public void TestCheckShipperAddressFields()
		{
			CombineAssertions(() =>
			{
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ShipperNameInfo, "Shipper Name is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ShipperAddressInfo, "Shipper Address is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ShipperAddress2Info, "Shipper Address is not valid.");

				AssertAddressWithLongerLength(AWBHeader.EH_ShipperNameInfo, $"Shipper Name is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_ShipperAddressInfo, $"Shipper Address is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_ShipperAddress2Info, $"Shipper Address is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");

				AssertAddressWithInvalidCharacters(AWBHeader.EH_ShipperPlaceInfo, "Shipper City is not valid.");
				AssertState(AWBHeader.EH_ShipperStateInfo, "Shipper State is not valid.");
				AssertPostCode(AWBHeader.EH_ShipperPostCodeInfo, "Shipper Post Code is not valid.");
			});
		}

		public void TestCheckShipperTraderTypeAndNo()
		{
			AWBHeader.EH_ShipperTraderNoType = "USC";
			AWBHeader.EH_ShipperTraderNo = "12345678901234567890123456789012";
			AssertHasWarning(AWBHeader.EH_ShipperTraderNoTypeInfo, "Shipper Trader No Type and Shipper Trader No must be less than or equal to 35 characters in length.");
			AssertHasWarning(AWBHeader.EH_ShipperTraderNoInfo, "Shipper Trader No Type and Shipper Trader No must be less than or equal to 35 characters in length.");

			AWBHeader.EH_ShipperTraderNo = "1234567890123456789012345678901";
			AssertNoWarnings(AWBHeader.EH_ShipperTraderNoTypeInfo);
			AssertNoWarnings(AWBHeader.EH_ShipperTraderNoInfo);
		}

		public void TestCheckEH_ShipperTraderNo_Exceed35CharactersValidation()
		{
			var awbHeader = Factory.New<ExportAWBHeaderForTest>();
			awbHeader.IsShipperTraderNoExceedingMaxLengthForTest = true;
			awbHeader.Validation.ValidateEH_ShipperTraderNo();
			AssertHasWarning(awbHeader.EH_ShipperTraderNoInfo, "Tax number cannot be included as it exceeds the character limit of 35.");

			awbHeader.IsAWBOverriddenForTest = true;
			awbHeader.Validation.ValidateEH_ShipperTraderNo();
			AssertNoWarnings(AWBHeader.EH_ShipperTraderNoInfo);

			awbHeader.IsAWBOverriddenForTest = false;
			awbHeader.IsShipperTraderNoExceedingMaxLengthForTest = false;
			awbHeader.Validation.ValidateEH_ShipperTraderNo();
			AssertNoWarnings(AWBHeader.EH_ShipperTraderNoInfo);
		}

		#endregion

		#region Consignee

		public virtual void TestCheckEH_ConsigneeCountryCode()
		{
			AWBHeader.EH_ConsigneeCountryCode = "12";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "A-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "A";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "AU";
			AssertNoNotifications(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "";
			AssertNoNotifications(AWBHeader.EH_ConsigneeCountryCodeInfo);
		}

		public void TestCheckConsigneeAddressFields()
		{
			CombineAssertions(() =>
			{
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ConsigneeNameInfo, "Consignee Name is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ConsigneeAddressInfo, "Consignee Address is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_ConsigneeAddress2Info, "Consignee Address is not valid.");

				AssertAddressWithLongerLength(AWBHeader.EH_ConsigneeNameInfo, $"Consignee Name is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_ConsigneeAddressInfo, $"Consignee Address is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_ConsigneeAddress2Info, $"Consignee Address is too long, only the first {AWBHeader.AirMessageMaxNameAddressLength} characters can be sent.");

				AssertAddressWithInvalidCharacters(AWBHeader.EH_ConsigneePlaceInfo, "Consignee City is not valid.");
				AssertState(AWBHeader.EH_ConsigneeStateInfo, "Consignee State is not valid.");
				AssertPostCode(AWBHeader.EH_ConsigneePostCodeInfo, "Consignee Post Code is not valid.");
			});
		}

		public void TestCheckConsigneeTraderTypeAndNo()
		{
			AWBHeader.EH_ConsigneeTraderNoType = "USC";
			AWBHeader.EH_ConsigneeTraderNo = "12345678901234567890123456789012";
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "Consignee Trader No Type and Consignee Trader No must be less than or equal to 35 characters in length.");
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "Consignee Trader No Type and Consignee Trader No must be less than or equal to 35 characters in length.");

			AWBHeader.EH_ConsigneeTraderNo = "1234567890123456789012345678901";
			AssertNoWarnings(AWBHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(AWBHeader.EH_ConsigneeTraderNoInfo);
		}

		public void TestCheckEH_ConsigneeTraderNo_Exceed35CharactersValidation()
		{
			var awbHeader = Factory.New<ExportAWBHeaderForTest>();
			awbHeader.IsConsigneeTraderNoExceedingMaxLengthForTest = true;
			awbHeader.Validation.ValidateEH_ConsigneeTraderNo();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, "Tax number cannot be included as it exceeds the character limit of 35.");

			awbHeader.IsAWBOverriddenForTest = true;
			awbHeader.Validation.ValidateEH_ConsigneeTraderNo();
			AssertNoWarnings(AWBHeader.EH_ConsigneeTraderNoInfo);

			awbHeader.IsAWBOverriddenForTest = false;
			awbHeader.IsConsigneeTraderNoExceedingMaxLengthForTest = false;
			awbHeader.Validation.ValidateEH_ConsigneeTraderNo();
			AssertNoWarnings(AWBHeader.EH_ConsigneeTraderNoInfo);
		}

		public void TestCheckEH_ConsigneeContactCode()
		{
			var awbHeader = Factory.New<ExportAWBHeaderForTest>();

			awbHeader.EH_ConsigneeContactDetail = "Description";
			AssertHasNotifications(awbHeader.EH_ConsigneeContactCodeInfo);

			awbHeader.EH_ConsigneeContactCode = "TEL";
			AssertHasNotifications(awbHeader.EH_ConsigneeContactCodeInfo);

			awbHeader.EH_ConsigneeContactCode = "TE";
			AssertNoNotifications(awbHeader.EH_ConsigneeContactCodeInfo);

			awbHeader.EH_ConsigneeContactCode = "#*-";
			AssertHasNotifications(awbHeader.EH_ConsigneeContactCodeInfo);

			awbHeader.EH_ConsigneeContactDetail = "";
			awbHeader.EH_ConsigneeContactCode = "";
			AssertNoNotifications(awbHeader.EH_ConsigneeContactCodeInfo);
		}

		public void TestCheckEH_ConsigneeContactDetail()
		{
			var awbHeader = Factory.New<ExportAWBHeaderForTest>();

			awbHeader.EH_ConsigneeContactCode = "ABC";
			AssertHasNotifications(awbHeader.EH_ConsigneeContactDetailInfo);

			awbHeader.EH_ConsigneeContactDetail = "Description";
			AssertNoNotifications(awbHeader.EH_ConsigneeContactDetailInfo);

			awbHeader.EH_ConsigneeContactDetail = "*(#$#-";
			AssertHasNotifications(awbHeader.EH_ConsigneeContactDetailInfo);

			awbHeader.EH_ConsigneeContactCode = "";
			AssertNoNotifications(awbHeader.EH_ConsigneeContactDetailInfo);
		}

		#endregion

		#region Notify Party

		public void TestCheckAlsoNotifyTraderTypeAndNo()
		{
			AWBHeader.EH_AlsoNotifyTraderNoType = "USC";
			AWBHeader.EH_AlsoNotifyTraderNo = "12345678901234567890123456789012";
			AssertHasWarning(AWBHeader.EH_AlsoNotifyTraderNoTypeInfo, "Also Notify Trader No Type and Also Notify Trader No must be less than or equal to 35 characters in length.");
			AssertHasWarning(AWBHeader.EH_AlsoNotifyTraderNoInfo, "Also Notify Trader No Type and Also Notify Trader No must be less than or equal to 35 characters in length.");

			AWBHeader.EH_AlsoNotifyTraderNo = "1234567890123456789012345678901";
			AssertNoWarnings(AWBHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(AWBHeader.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestCheckEH_AlsoNotifyTraderNo_Exceed35CharactersValidation()
		{
			var awbHeader = Factory.New<ExportAWBHeaderForTest>();
			awbHeader.IsAlsoNotifyTraderNoExceedingMaxLengthForTest = true;
			awbHeader.Validation.ValidateEH_AlsoNotifyTraderNo();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, "Tax number cannot be included as it exceeds the character limit of 35.");

			awbHeader.IsAWBOverriddenForTest = true;
			awbHeader.Validation.ValidateEH_AlsoNotifyTraderNo();
			AssertNoWarnings(AWBHeader.EH_AlsoNotifyTraderNoInfo);

			awbHeader.IsAWBOverriddenForTest = false;
			awbHeader.IsAlsoNotifyTraderNoExceedingMaxLengthForTest = false;
			awbHeader.Validation.ValidateEH_AlsoNotifyTraderNo();
			AssertNoWarnings(AWBHeader.EH_AlsoNotifyTraderNoInfo);
		}

		#endregion

		#region TestIssuingAgentNameAndAddressValidation

		public void TestIssuingAgentNameValidation()
		{
			AWBHeader.EH_IssuingAgentName = "XYZ CORPOATE SOLUTIONS";
			AWBHeader.Validation.ValidateEH_IssuingAgentName();
			AssertNoWarnings("No warning message", AWBHeader.EH_IssuingAgentNameInfo);

			AWBHeader.EH_IssuingAgentName = "XXYZ CORPOATE SOLUTIONS XXYZ CORPOATE SOLUTIONS XXYZ XXYZ";
			AWBHeader.Validation.ValidateEH_IssuingAgentName();
			AssertHasWarning(AWBHeader.EH_IssuingAgentNameInfo, "Value exceeded maximum number of allowed characters and has been truncated");
		}

		public void TestIssuingAgentAddress1Validation()
		{
			AWBHeader.EH_IssuingAgentAddress1 = "XYZ STREET";
			AWBHeader.Validation.ValidateEH_IssuingAgentAddress1();
			AssertNoWarnings("No warning message", AWBHeader.EH_IssuingAgentAddress1Info);

			AWBHeader.EH_IssuingAgentAddress1 = "XXYZ STREET STREET XXYZ STREET STREET XXYZ STREET STREET XXYZ STREET STREET";
			AWBHeader.Validation.ValidateEH_IssuingAgentAddress1();
			AssertHasWarning(AWBHeader.EH_IssuingAgentAddress1Info, "Value exceeded maximum number of allowed characters and has been truncated");
		}

		public void TestIssuingAgentAddress2Validation()
		{
			AWBHeader.EH_IssuingAgentAddress2 = "GARDENERS ROAD, BRISBANE, QUEENSLAND, 160098, AU";
			AWBHeader.Validation.ValidateEH_IssuingAgentAddress2();
			AssertNoWarnings("No warning message", AWBHeader.EH_IssuingAgentAddress2Info);

			AWBHeader.EH_IssuingAgentAddress2 = "GARDENERS ROAD KINGS ROAD, BRISBANE, QUEENSLAND, 160098, AUSTRALIA";
			AWBHeader.Validation.ValidateEH_IssuingAgentAddress2();
			AssertHasWarning(AWBHeader.EH_IssuingAgentAddress2Info, "Value exceeded maximum number of allowed characters and has been truncated");
		}

		#endregion

		public void TestValidateEH_WeightPrepaidCollect()
		{
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = "X";
			AssertHasErrors(AWBHeader.EH_WeightPrepaidCollectInfo);
		}

		#region Flight Bookings Tests

		public void TestBookingsCrossValidation()
		{
			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking1stFlight = "";
			AWBHeader.EH_Booking1stFlightDate = "";
			AWBHeader.EH_Booking2ndCarrier = "";
			AWBHeader.EH_Booking2ndFlight = "";
			AWBHeader.EH_Booking2ndFlightDate = "";

			AssertNoNotifications("Precondition1", AWBHeader.EH_Booking1stCarrierInfo);
			AssertNoNotifications("Precondition2", AWBHeader.EH_Booking1stFlightDateInfo);
			AssertNoNotifications("Precondition3", AWBHeader.EH_Booking1stFlightInfo);
			AssertNoNotifications("Precondition4", AWBHeader.EH_Booking2ndCarrierInfo);
			AssertNoNotifications("Precondition5", AWBHeader.EH_Booking2ndFlightDateInfo);
			AssertNoNotifications("Precondition6", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking1stCarrier = "A";
			AssertHasNotification(AWBHeader.EH_Booking1stCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stCarrier = "QF";
			AssertNoNotifications("Valid 1st Carrier", AWBHeader.EH_Booking1stCarrierInfo);
			AssertHasNotification(AWBHeader.EH_Booking1stFlightInfo, "First Requested Flight must consist of Flight Number (up to 4 numeric) and an optional Operational Suffix (1 alphabetic).", ExpectedBookingsNotificationType);
			AssertHasNotification(AWBHeader.EH_Booking1stFlightDateInfo, "First Requested Flight Date must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking1stFlight = "001A";
			AWBHeader.EH_Booking1stFlightDate = "12";
			AssertHasNotification(AWBHeader.EH_Booking1stCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);
			AssertNoNotifications("Valid 1st Flight", AWBHeader.EH_Booking1stFlightInfo);
			AssertNoNotifications("Valid 1st Flight date", AWBHeader.EH_Booking1stFlightDateInfo);

			AWBHeader.EH_Booking1stCarrier = "AA";
			AssertNoNotifications("Valid First Booking details 1", AWBHeader.EH_Booking1stCarrierInfo);
			AssertNoNotifications("Valid First Booking details 2", AWBHeader.EH_Booking1stFlightDateInfo);
			AssertNoNotifications("Valid First Booking details 3", AWBHeader.EH_Booking1stFlightInfo);
			AssertNoNotifications("Valid First Booking details 4", AWBHeader.EH_Booking2ndCarrierInfo);
			AssertNoNotifications("Valid First Booking details 5", AWBHeader.EH_Booking2ndFlightDateInfo);
			AssertNoNotifications("Valid First Booking details 6", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking2ndCarrier = "A";
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndCarrier = "QF";
			AssertNoNotifications("Valid 2nd Carrier", AWBHeader.EH_Booking2ndCarrierInfo);
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, "Second Requested Flight must consist of Flight Number (up to 4 numeric) and an optional Operational Suffix (1 alphabetic).", ExpectedBookingsNotificationType);
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightDateInfo, "Second Requested Flight Date must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndCarrier = "";
			AWBHeader.EH_Booking2ndFlight = "001A";
			AWBHeader.EH_Booking2ndFlightDate = "12";
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);
			AssertNoNotifications("Valid 2nd Flight", AWBHeader.EH_Booking2ndFlightInfo);
			AssertNoNotifications("Valid 2nd Flight date", AWBHeader.EH_Booking2ndFlightDateInfo);

			AWBHeader.EH_Booking2ndCarrier = "BA";
			AssertNoNotifications("Valid First and Second Booking details 1", AWBHeader.EH_Booking1stCarrierInfo);
			AssertNoNotifications("Valid First and Second Booking details 2", AWBHeader.EH_Booking1stFlightDateInfo);
			AssertNoNotifications("Valid First and Second Booking details 3", AWBHeader.EH_Booking1stFlightInfo);
			AssertNoNotifications("Valid First and Second Booking details 4", AWBHeader.EH_Booking2ndCarrierInfo);
			AssertNoNotifications("Valid First and Second Booking details 5", AWBHeader.EH_Booking2ndFlightDateInfo);
			AssertNoNotifications("Valid First and Second Booking details 6", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking1stFlight = "";
			AWBHeader.EH_Booking1stFlightDate = "";
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "2nd Booking details cannot be entered unless 1st Booking details have been completed.", ExpectedBookingsNotificationType);
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, "2nd Booking details cannot be entered unless 1st Booking details have been completed.", ExpectedBookingsNotificationType);
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightDateInfo, "2nd Booking details cannot be entered unless 1st Booking details have been completed.", ExpectedBookingsNotificationType);
		}

		public void TestCheckEH_Booking1stCarrier()
		{
			AWBHeader.EH_Booking1stCarrier = "";
			AssertNoNotifications("1st Booking is not required", AWBHeader.EH_Booking1stCarrierInfo);

			AWBHeader.EH_Booking1stCarrier = "Q";
			AssertEquals(1, AWBHeader.EH_Booking1stCarrierInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stCarrier = "QF";
			AssertNoNotifications("Valid 1st Booking", AWBHeader.EH_Booking1stCarrierInfo);

			AWBHeader.EH_Booking1stCarrier = "Q-";
			AssertEquals(1, AWBHeader.EH_Booking1stCarrierInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stCarrierInfo, "Carrier must consist of alphanumeric characters only", ExpectedBookingsNotificationType);
		}

		public void TestCheckEH_Booking1stFlightDate()
		{
			AWBHeader.EH_Booking1stFlightDate = "";
			AWBHeader.EH_Booking1stCarrier = "";
			AssertNoNotifications("1st Booking is not required", AWBHeader.EH_Booking1stFlightDateInfo);

			AWBHeader.EH_Booking1stCarrier = "AA";
			AssertHasNotification(AWBHeader.EH_Booking1stFlightDateInfo, "First Requested Flight Date must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlightDate = "5";
			AssertEquals("Left padded", "05", AWBHeader.EH_Booking1stFlightDate);
			AssertNoNotifications("1st Flight Date is valid", AWBHeader.EH_Booking1stFlightDateInfo);

			AWBHeader.EH_Booking1stFlightDate = "01";
			AssertNoNotifications("1st Flight Date is valid", AWBHeader.EH_Booking1stFlightDateInfo);

			AWBHeader.EH_Booking1stFlightDate = "A1";
			AssertHasNotification(AWBHeader.EH_Booking1stFlightDateInfo, "First Requested Flight Date must consist of numeric characters only", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlightDate = "0";
			AssertHasWarning(AWBHeader.EH_Booking1stFlightDateInfo, "Date value should be between 1 and 31.");
		}

		public void TestCheckEH_Booking1stFlight()
		{
			string expectedMessage = "First Requested Flight must consist of Flight Number (up to 4 numeric) and an optional Operational Suffix (1 alphabetic).";
			AWBHeader.EH_Booking1stFlight = "111";
			AssertNoNotifications("Valid flight number", AWBHeader.EH_Booking1stFlightInfo);

			AWBHeader.EH_Booking1stFlight = "G01";
			AssertEquals("G01 is not a valid flight number", 1, AWBHeader.EH_Booking1stFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlight = "001DB";
			AssertEquals("001DB is not a valid flight number", 1, AWBHeader.EH_Booking1stFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlight = "00922";
			AssertEquals("00922 is not a valid flight number", 1, AWBHeader.EH_Booking1stFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlight = "9999D";
			AssertNoNotifications("9999D is a valid flight number", AWBHeader.EH_Booking1stFlightInfo);

			AWBHeader.EH_Booking1stFlight = "888C";
			AssertNoNotifications("888C is a valid flight number", AWBHeader.EH_Booking1stFlightInfo);

			AWBHeader.EH_Booking1stFlight = "S9001";
			AssertEquals("S9001 is not a valid flight number", 1, AWBHeader.EH_Booking1stFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking1stFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stFlight = "08";
			AssertNoNotifications("08 is a valid flight number", AWBHeader.EH_Booking1stFlightInfo);
		}

		public void TestCheckEH_Booking2ndCarrier()
		{
			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking2ndCarrier = "AA";
			AssertEquals("1st booking must be entered", 1, AWBHeader.EH_Booking2ndCarrierInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "2nd Booking details cannot be entered unless 1st Booking details have been completed.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stCarrier = "XX";
			AssertNoNotifications("AA is valid", AWBHeader.EH_Booking1stCarrierInfo);

			AWBHeader.EH_Booking2ndCarrier = "Q";
			AssertEquals("Q is invalid", 1, AWBHeader.EH_Booking2ndCarrierInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "Carrier must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndCarrier = "Q-";
			AssertEquals(1, AWBHeader.EH_Booking2ndCarrierInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndCarrierInfo, "Carrier must consist of alphanumeric characters only", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndCarrier = "qa";
			AssertNoNotifications("Lower case characters are converted on transmission", AWBHeader.EH_Booking2ndCarrierInfo);

			AWBHeader.EH_Booking2ndCarrier = "";
			AssertNoNotifications("2nd Carrier is not required", AWBHeader.EH_Booking2ndCarrierInfo);
		}

		public void TestCheckEH_Booking2ndFlightDate()
		{
			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking2ndCarrier = "";
			AWBHeader.EH_Booking2ndFlightDate = "12";
			AssertEquals("1st booking must be entered", 1, AWBHeader.EH_Booking2ndFlightDateInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightDateInfo, "2nd Booking details cannot be entered unless 1st Booking details have been completed.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking1stCarrier = "XX";
			AssertNoNotifications("12 is valid", AWBHeader.EH_Booking2ndFlightDateInfo);

			AWBHeader.EH_Booking2ndFlightDate = "5";
			AssertEquals("Left padded", "05", AWBHeader.EH_Booking2ndFlightDate);
			AssertNoNotifications("Left padded", AWBHeader.EH_Booking2ndFlightDateInfo);

			AWBHeader.EH_Booking2ndFlightDate = "";
			AssertNoNotifications("2nd flight date not required", AWBHeader.EH_Booking2ndFlightDateInfo);

			AWBHeader.EH_Booking2ndCarrier = "QF";
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightDateInfo, "Second Requested Flight Date must be 2 characters in length.", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlightDate = "A1";
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightDateInfo, "Second Requested Flight Date must consist of numeric characters only", ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlightDate = "0";
			AssertHasWarning(AWBHeader.EH_Booking2ndFlightDateInfo, "Date value should be between 1 and 31.");
		}

		public void TestCheckEH_Booking2ndFlight()
		{
			string expectedMessage = "Second Requested Flight must consist of Flight Number (up to 4 numeric) and an optional Operational Suffix (1 alphabetic).";

			AWBHeader.EH_Booking2ndFlight = "";
			AssertNoNotifications("2nd booking data is optional", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking1stCarrier = "";
			AWBHeader.EH_Booking2ndFlight = "111";
			AssertEquals("1st booking must be entered", 1, AWBHeader.EH_Booking2ndFlightInfo.Notifications.Count());

			AWBHeader.EH_Booking1stCarrier = "XX";
			AssertNoNotifications("111 is valid", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking2ndFlight = "G01";
			AssertEquals("G01 is not a valid flight number", 1, AWBHeader.EH_Booking2ndFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlight = "001DB";
			AssertEquals("001DB is not a valid flight number", 1, AWBHeader.EH_Booking2ndFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlight = "00922";
			AssertEquals("00922 is not a valid flight number", 1, AWBHeader.EH_Booking2ndFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlight = "9999D";
			AssertNoNotifications("9999D is a valid flight number", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking2ndFlight = "888C";
			AssertNoNotifications("888C is a valid flight number", AWBHeader.EH_Booking2ndFlightInfo);

			AWBHeader.EH_Booking2ndFlight = "S9001";
			AssertEquals("S9001 is not a valid flight number", 1, AWBHeader.EH_Booking2ndFlightInfo.Notifications.Count());
			AssertHasNotification(AWBHeader.EH_Booking2ndFlightInfo, expectedMessage, ExpectedBookingsNotificationType);

			AWBHeader.EH_Booking2ndFlight = "08";
			AssertNoNotifications("08 is a valid flight number", AWBHeader.EH_Booking2ndFlightInfo);
		}

		#endregion

		#region Agent

		public void TestValidateEH_AgentName()
		{
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentNameCalled);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "";
			ValidationForAgentTests.ValidateEH_AgentName();
			Assert("Check method should be empty in base", !AWBHeader.EH_AgentNameInfo.HasWarnings());
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentNameCalled);
		}

		public void TestValidateEH_AgentPlace()
		{
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentPlaceCalled);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "";
			ValidationForAgentTests.ValidateEH_AgentPlace();
			Assert("Check method should be empty in base", !AWBHeader.EH_AgentPlaceInfo.HasWarnings());
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentPlaceCalled);
		}

		public void TestValidateEH_AgentIATACodeFormatted()
		{
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentIATACodeCalled);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			ValidationForAgentTests.ValidateEH_AgentIATACode();
			Assert("Check method should be empty in base", !AWBHeader.EH_AgentIATACodeFormattedInfo.HasWarnings());
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentIATACodeCalled);
		}

		public void TestValidateEH_AgentAccountNo()
		{
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentAccountNoCalled);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			ValidationForAgentTests.ValidateEH_AgentAccountNo();
			Assert("Check method should be empty in base", !AWBHeader.EH_AgentAccountNoInfo.HasWarnings());
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentAccountNoCalled);
		}

		public void TestValidateAllCallsValidateAgentMethods()
		{
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentNameCalled);
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentPlaceCalled);
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentIATACodeCalled);
			Assert("Pre-condition", !ValidationForAgentTests.CheckEH_AgentAccountNoCalled);

			ValidationForAgentTests.ValidateAll();

			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentNameCalled);
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentPlaceCalled);
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentIATACodeCalled);
			Assert("Should be called", ValidationForAgentTests.CheckEH_AgentAccountNoCalled);
		}

		ExportAWBHeaderValidationForTest ValidationForAgentTests
		{
			get
			{
				if (fValidationForAgentTests == null)
				{
					fValidationForAgentTests = new ExportAWBHeaderValidationForTest(AWBHeader);
				}
				return fValidationForAgentTests;
			}
		}

		ExportAWBHeaderValidationForTest fValidationForAgentTests;

		#endregion

		public void TestValidateAll()
		{
			AWBHeader.EH_WayBillNumber = "";
			AWBHeader.ClearAllNotifications();
			AWBHeader.Validation.ValidateAll();
			AssertHasNotifications("Should be calling base, so that the ShipperName validation is called", AWBHeader.EH_AWBOriginCodeInfo);
			if (AWBHeader.Validation.NotificationLevelForFieldsRequiredForElectronicTransmission == CargoWise.EntityFramework.NotificationType.Warning)
			{
				AssertHasWarnings(AWBHeader.EH_AirlinePrefixInfo);
			}
			else if (AWBHeader.Validation.NotificationLevelForFieldsRequiredForElectronicTransmission == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				AssertHasMessageErrors(AWBHeader.EH_AirlinePrefixInfo);
			}
			AssertHasWarnings(AWBHeader.EH_AWBSerialNoInfo);
			AssertHasNotifications(AWBHeader.EH_TotalGrossWeightInfo);
		}

		protected void AssertAddressWithInvalidCharacters(ZPropertyInfo info, string expectedError)
		{
			var validValues = new List<ZString> { "AAA", "BB", "XX" };
			var invalidValues = new List<ZString> { "TBA", ".", "..", "X", "-" };

			AssertAWBValidation(info, validValues, invalidValues, expectedError);
		}

		protected void AssertAddressWithLongerLength(ZPropertyInfo info, string expectedWarning)
		{
			var validValuesWithProperLength = new List<ZString> { BuildCharactersWithSpecifiedLength(AWBHeader.AirMessageMaxNameAddressLength) };
			var invalidValuesWithLongerLength = new List<ZString> { BuildCharactersWithSpecifiedLength(AWBHeader.AirMessageMaxNameAddressLength + 1) };

			foreach (var validValue in validValuesWithProperLength)
			{
				info.Value = validValue;
				AssertNoWarning(info, expectedWarning);
			}

			foreach (var invalidValue in invalidValuesWithLongerLength)
			{
				info.Value = invalidValue;
				AssertHasWarning(info, expectedWarning);
			}

			info.ClearValue();
		}

		ZString BuildCharactersWithSpecifiedLength(int length)
		{
			var sb = new ZStringBuilder();
			for (int i = 0; i < length; i++)
			{
				sb.Append("A");
			}

			return sb.ToString();
		}

		protected void AssertState(ZPropertyInfo info, string expectedError)
		{
			var validValues = new List<ZString> { "", "Guangzhou", "Jiangsu", "1" };
			var invalidValue = new List<ZString> { ".", "-." };

			AssertAWBValidation(info, validValues, invalidValue, expectedError);
		}

		protected void AssertPostCode(ZPropertyInfo info, string errorMessage)
		{
			var validValues = new List<ZString> { "", "abc-123", "111125" };
			var invalidValue = new List<ZString> { ".", "-." };

			AssertAWBValidation(info, validValues, invalidValue, errorMessage);
		}

		void AssertAWBValidation(ZPropertyInfo info, IEnumerable<ZString> validValues, IEnumerable<ZString> invalidValues, string expectedError)
		{
			foreach (var validValue in validValues)
			{
				info.Value = validValue;
				AssertNoError(info, expectedError);
			}

			foreach (var invalidValue in invalidValues)
			{
				info.Value = invalidValue;
				AssertHasNotification(info, expectedError, ExpectedAWBHeaderNotificationType);
			}

			info.ClearValue();
		}

		#region Implementation

		protected ExportAWBHeader AWBHeader;

		protected override void SetUp()
		{
			AWBHeader = GetNewAWBHeader();
			AWBHeader.EH_WayBillNumber = "020-12345678";
		}

		protected virtual ExportAWBHeader GetNewAWBHeader()
		{
			return Factory.New<ExportAWBHeaderForTest>();
		}

		protected void AssertHasNotificationsForElectronicTransmission(ZPropertyInfo propertyInfo)
		{
			if (ExpectedAWBHeaderNotificationType == CargoWise.EntityFramework.NotificationType.Warning)
			{
				AssertHasWarnings(propertyInfo);
			}
			else if (ExpectedAWBHeaderNotificationType == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				AssertHasMessageErrors(propertyInfo);
			}
			else
			{
				AssertHasNotifications(propertyInfo);
			}
		}

		void AssertHasNotification(ZPropertyInfo info, string notification, INotificationType notificationType)
		{
			if (notificationType == CargoWise.EntityFramework.NotificationType.Warning)
			{
				AssertHasWarning(info, notification);
			}
			else if (notificationType == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				AssertHasMessageError(info, notification);
			}
		}

		protected virtual INotificationType ExpectedAWBHeaderNotificationType
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		protected virtual INotificationType ExpectedBookingsNotificationType
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		class ExportAWBHeaderForTest : ExportAWBHeader
		{
			public ExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ExportAWBHeaderValidation GetNewValidation()
			{
				return new ExportAWBHeaderValidationForTest(this);
			}

			public bool IsShipperTraderNoExceedingMaxLengthForTest
			{
				get => IsShipperTraderNoExceedingMaxLength;
				set => IsShipperTraderNoExceedingMaxLength = value;
			}

			public bool IsConsigneeTraderNoExceedingMaxLengthForTest
			{
				get => IsConsigneeTraderNoExceedingMaxLength;
				set => IsConsigneeTraderNoExceedingMaxLength = value;
			}

			public bool IsAlsoNotifyTraderNoExceedingMaxLengthForTest
			{
				get => IsAlsoNotifyTraderNoExceedingMaxLength;
				set => IsAlsoNotifyTraderNoExceedingMaxLength = value;
			}

			public bool IsAWBOverriddenForTest { get; set; }
			public override bool IsAWBOverridden => IsAWBOverriddenForTest;

			public new ExportAWBHeaderValidationForTest Validation
			{
				get { return (ExportAWBHeaderValidationForTest)base.Validation; }
			}
		}

		#region class ExportAWBHeaderValidationForTest

		class ExportAWBHeaderValidationForTest : ExportAWBHeaderValidation
		{
			public ExportAWBHeaderValidationForTest(ExportAWBHeader aWBHeader)
				: base(aWBHeader)
			{
			}

			protected override void CheckEH_AgentName()
			{
				base.CheckEH_AgentName();
				CheckEH_AgentNameCalled = true;
			}

			protected override void CheckEH_AgentPlace()
			{
				base.CheckEH_AgentPlace();
				CheckEH_AgentPlaceCalled = true;
			}

			protected override void CheckEH_AgentIATACode()
			{
				base.CheckEH_AgentIATACode();
				CheckEH_AgentIATACodeCalled = true;
			}

			protected override void CheckEH_AgentAccountNo()
			{
				base.CheckEH_AgentAccountNo();
				CheckEH_AgentAccountNoCalled = true;
			}

			public bool CheckEH_AgentNameCalled;
			public bool CheckEH_AgentPlaceCalled;
			public bool CheckEH_AgentAccountNoCalled;
			public bool CheckEH_AgentIATACodeCalled;

			public override INotificationType NotificationLevelForFieldsRequiredForElectronicTransmission => CargoWise.EntityFramework.NotificationType.Warning;
		}

		#endregion

		#endregion
	}
}
