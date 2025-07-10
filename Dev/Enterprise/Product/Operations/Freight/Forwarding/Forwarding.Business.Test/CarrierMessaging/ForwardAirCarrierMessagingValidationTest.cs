using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardAirCarrierMessagingValidation))]
	sealed class ForwardAirCarrierMessagingValidationTest : CarrierMessagingValidationTest
	{
		protected override CarrierMessagingValidation GetNewValidation(ForwardingConsol consol)
		{
			return new ForwardAirCarrierMessagingValidation(consol);
		}

		public void TestDoNotAllowToSendMessageFromConsolWithEmptyMasterBillNumberAndNoAvailableNumberRange()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = string.Empty;
			consol.JK_AWBServiceLevel = "PUC";
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Shipments.AddNew();

			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);

			var expectedMessage = @"Message cannot be sent when Master Bill Number is empty and we cannot allocate one Forward Air Bill Number from the consol's carrier.
Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges.";
			AssertContains(expectedMessage, notifications.AsString);

			var stmNums = carrier.OrgFountains.AddNew();
			stmNums.SN_Type = "FWA";
			stmNums.SN_Prefix = string.Empty;
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_MaximumValue = 10;
			stmNums.SN_Value = 10;

			Factory.Save();

			notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);
			AssertNotContains(expectedMessage, notifications.AsString);
		}

		public void TestDoNotAllowToSendMessageFromConsolWithCarrierInvalidRegistrationCode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "123456";
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;

			var shipment = consol.Shipments.AddNew();

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrierOrg.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.CAIMP;

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;

			Factory.Save();
			AssertValidationMessageForInvalidRegistrationCode(consol);

			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			Factory.Save();
			AssertValidationMessageForInvalidRegistrationCode(consol);

			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;
			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();

			validation.Validate(notifications);
			AssertNotContains("validation message", "The carrier is not registered as Forward Air.", notifications.AsString);
		}

		public void TestConfirmProcessingServiceLevels()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "987654";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "CARRIER";
			OrgCusCode carrierCode = carrierOrg.CustomsCodes.AddNew();
			carrierCode = carrierOrg.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_AWBServiceLevel = "ABC";

			var shipment = consol.Shipments.AddNew();

			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.Transports[0].JW_ETA = ZDateTime.Now;

			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);

			var message = @"The carrier service level does not correspond to Forward Air delivery methods and therefore no special door pickup or delivery instructions will be send to Forward Air. Do you want to proceed?

If you need to request pickup or delivery service, please configure your Forward Air Organization
(Organization -> Carrier -> Service Level) with the following service levels: PUC-Pickup, PUD-Delivery, PAD-Pickup and Delivery.";

			AssertContains(message, notifications.LastQueryUserMessage);

			notifications.Clear();

			consol.JK_AWBServiceLevel = "PUC";
			Factory.Save();

			notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);

			AssertNotContains(message, notifications.LastQueryUserMessage);
		}

		public void TestDoNotAllowToSendMessageFromConsolWhenMasterBillIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_MasterBillNum = ZString.Empty;

			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();
			validation.ValidateMasterBillNumber(notifications);

			AssertContains("validation message", "Message cannot be sent when Master Bill Number is empty.", notifications.AsString);

			consol.JK_MasterBillNum = "0987654343";
			Factory.Save();

			notifications = new NotificationBufferTestClass();
			validation.ValidateMasterBillNumber(notifications);

			AssertNotContains("validation message", "Message cannot be sent when Master Bill Number is empty.", notifications.AsString);
		}

		public void TestDoNotAllowToSendMessageWhenNoShipmentAttachedToConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_MasterBillNum = "MAS0888444";

			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);

			AssertContains("validation message", "There is no shipment attached to the consol.", notifications.AsString);

			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);
			AssertNotContains("validation message", "There is no shipment attached to the consol.", notifications.AsString);
		}

		public void TestDoNotAllowToSendMessageWhen_OneShipmentAttached_AndNoReceivingAgentOrDeliverTo()
		{
			#region Setup

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_MasterBillNum = "3434444";
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrier.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			#endregion

			var serviceLevels = new[] { Delivery, Pickup, PickupAndDelivery, Standard };

			foreach (var serviceLevel in serviceLevels)
			{
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.Shipments[0].ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				consol.JK_AWBServiceLevel = serviceLevel;

				AssertEquals(1, consol.Shipments.Count);
				CombineAssertions("Pre-conditions", delegate
				{
					AssertEquals(ZGuid.Empty, consol.JK_OA_ReceivingForwarderAddress);
					Assert(!consol.Shipments[0].ConsigneeDeliveryAddress.IsValidAddress);
				});

				Factory.Save();

				var validation = GetNewValidation(consol);
				var notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Receiving Agent or the Shipment's Deliver To", notifications.AsString.Trim());

				var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Receiving Agent or the Shipment's Deliver To", notifications.AsString.Trim());

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.Shipments[0].ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Receiving Agent or the Shipment's Deliver To", notifications.AsString.Trim());
			}
		}

		public void TestDoNotAllowToSendMessageWhen_OneShipmentAttached_AndNoSendingAgentOrPickUpFrom()
		{
			#region Setup

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_MasterBillNum = "3434444";
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrier.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			#endregion

			var serviceLevels = new[] { Delivery, Pickup, PickupAndDelivery, Standard };

			foreach (var serviceLevel in serviceLevels)
			{
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.Shipments[0].ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				consol.JK_AWBServiceLevel = serviceLevel;

				AssertEquals(1, consol.Shipments.Count);
				CombineAssertions("Pre-conditions", delegate
				{
					AssertEquals(ZGuid.Empty, consol.JK_OA_SendingForwarderAddress);
					Assert(!consol.Shipments[0].ConsignorPickupAddress.IsValidAddress);
				});

				Factory.Save();

				var validation = GetNewValidation(consol);
				var notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Sending Agent or the Shipment's Pick Up From", notifications.AsString.Trim());

				var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Sending Agent or the Shipment's Pick Up From", notifications.AsString.Trim());

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.Shipments[0].ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in either the Consol Sending Agent or the Shipment's Pick Up From", notifications.AsString.Trim());
			}
		}

		public void TestDoNotAllowToSendMessageWhen_TwoOrMoreShipmentAttached_AndNoReceivingAgent()
		{
			#region Setup

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_MasterBillNum = "3434444";
			consol.JK_AgentType = Constants.AgentType.Direct;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrier.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			for (int i = 0; i < new Random().Next(2, 5); i++)
			{
				consol.Shipments.AddNew().DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			}

			AssertGreaterThanOrEqualTo("Pre-condition", consol.Shipments.Count, 2);

			#endregion

			var serviceLevels = new[] { Delivery, Pickup, PickupAndDelivery, Standard };

			foreach (var serviceLevel in serviceLevels)
			{
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.JK_AWBServiceLevel = serviceLevel;
				Factory.Save();

				AssertEquals(ZGuid.Empty, consol.JK_OA_ReceivingForwarderAddress);

				var validation = GetNewValidation(consol);
				var notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertEquals($"Service Level: {serviceLevel}", "Organization must be entered in the Consol's Receiving Agent field", notifications.AsString.Trim());

				var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);
				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in the Consol's Receiving Agent field", notifications.AsString.Trim());
			}
		}

		public void TestDoNotAllowToSendMessageWhen_TwoOrMoreShipmentAttached_AndNoSendingAgent()
		{
			#region Setup

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_MasterBillNum = "3434444";
			consol.JK_AgentType = Constants.AgentType.Direct;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrier.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			for (int i = 0; i < new Random().Next(2, 5); i++)
			{
				consol.Shipments.AddNew().DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;
			}

			AssertGreaterThanOrEqualTo("Pre-condition", consol.Shipments.Count, 2);

			#endregion

			var serviceLevels = new[] { Delivery, Pickup, PickupAndDelivery, Standard };

			foreach (var serviceLevel in serviceLevels)
			{
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_AWBServiceLevel = serviceLevel;
				Factory.Save();

				AssertEquals(ZGuid.Empty, consol.JK_OA_SendingForwarderAddress);

				var validation = GetNewValidation(consol);
				var notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);

				AssertEquals($"Service Level: {serviceLevel}", "Organization must be entered in the Consol's Sending Agent field", notifications.AsString.Trim());

				var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				Factory.Save();

				notifications = new NotificationBufferTestClass();
				validation.Validate(notifications);
				AssertNotEquals($"Service Level: {serviceLevel}", "Organization must be entered in the Consol's Sending Agent field", notifications.AsString.Trim());
			}
		}

		public void TestDoNotAllowToSendMessageWhenDeliveryDateIsNotSpecified_AIR()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "123456";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "PUD";
			consol.JK_AgentType = Constants.AgentType.Direct;

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "CARRIER";

			OrgCusCode carrierCode = carrierOrg.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_IsDirectBooking = true;

			var transport = consol.Transports.AddNew("AUSYD", "NZAKL");
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol);

			JobDocsAndCartage cartage = shipment.DocsAndCartage;
			cartage.JP_EstimatedDelivery = ZDateTime.Now;
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol, true);

			cartage.JP_EstimatedDelivery = ZDateTime.Empty;
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol);

			shipment.JS_E_ARV = ZDateTime.Now;
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol, true);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol);

			transport.JW_ETA = ZDateTime.Now;
			Factory.Save();

			AssertValidationMessageForArrivalDate(consol, true);
		}

		public void TestDoNotAllowToSendMessageFromConsolWhenMultipleCarrierContractNumber()
		{
			AssertValidationMessageIsContainsMultipleCarrierContractNumber("XXX", "YYY", true);
			AssertValidationMessageIsContainsMultipleCarrierContractNumber("XXX", "", false);
		}

		void AssertValidationMessageIsContainsMultipleCarrierContractNumber(string entryNumber1, string entryNumber2, bool shouldPromptError)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TEST" + entryNumber1 + entryNumber2;
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = string.Empty;
			consol.JK_AWBServiceLevel = "PUC";
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Shipments.AddNew();

			var stmNums = carrier.OrgFountains.AddNew();
			stmNums.SN_Type = "FWA";
			stmNums.SN_Prefix = string.Empty;
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_MaximumValue = 10;
			stmNums.SN_Value = 10;

			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = entryNumber1;
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			if (!entryNumber2.IsNullOrEmpty())
			{
				var number2 = consol.Numbers.AddNew();
				number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
				number2.CE_EntryNum = entryNumber2;
				number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			}

			Factory.Save();

			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);

			var expectedMessage = "You cannot have multiple Forward Air contract numbers registered on the same job. Please remove those that don't apply.";
			if (shouldPromptError)
			{
				AssertContains(expectedMessage, notifications.AsString);
			}
			else
			{
				AssertNotContains(expectedMessage, notifications.AsString);
			}

			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			Factory.Save();

			notifications = new NotificationBufferTestClass();
			validation.Validate(notifications);
			AssertNotContains(expectedMessage, notifications.AsString);
		}

		void AssertValidationMessageForInvalidRegistrationCode(ForwardingConsol consol)
		{
			var validation = GetNewValidation(consol);

			var notifications = new NotificationBufferTestClass();

			validation.Validate(notifications);

			AssertContains("validation message", "The carrier is not registered as Forward Air.", notifications.AsString);
		}

		void AssertValidationMessageForArrivalDate(ForwardingConsol consol, bool notContains = false)
		{
			var validation = GetNewValidation(consol);
			var notifications = new NotificationBufferTestClass();

			validation.Validate(notifications);

			if (notContains)
			{
				AssertNotContains("validation message", "The Arrival date is not specified for the consol.", notifications.AsString);
			}
			else
			{
				AssertContains("validation message", "The Arrival date is not specified for the consol.", notifications.AsString);
			}
		}

		const string Pickup = "PUC";
		const string Delivery = "PUD";
		const string PickupAndDelivery = "PAD";
		const string Standard = "STD";
	}
}
