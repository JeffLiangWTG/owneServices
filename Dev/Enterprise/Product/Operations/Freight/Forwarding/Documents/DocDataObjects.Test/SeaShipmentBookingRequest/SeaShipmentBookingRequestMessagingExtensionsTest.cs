using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	class SeaShipmentBookingRequestMessagingExtensionsTest : TestCaseWithFactory
	{
		#region ContinueWithSendingMessage

		public void TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging()
		{
			var shipment = CreateStandAloneShipment();
			var shipmentBookingRequest = new SeaShipmentBookingRequest(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var notifications = new Mock<IUserNotifications>();
			var res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("disallow sending Booking Request when the carrier doesn't have messaging capability", false, res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to MR Carrier that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = false;
			shipment.BookedShippingLine.OH_RSL_ShippingLine = shippingLine.PK;

			notifications = new Mock<IUserNotifications>();
			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("disallow sending Booking Request when the carrier doesn't have messaging capability", false, res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to MR Carrier that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);

			shippingLine.RSL_BookingRequestAvailable = true;

			notifications = new Mock<IUserNotifications>();
			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertNull("not stopping sending Booking Request when the carrier has messaging capability", res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to MR Carrier that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Never);
		}

		public void TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging_NVO()
		{
			var shipment = CreateAttachedToConsolShipment();
			var shipmentBookingRequest = new SeaShipmentBookingRequest(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var notifications = new Mock<IUserNotifications>();
			var res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("disallow sending Booking Request when the carrier doesn't have messaging capability", false, res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to ABC Company that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_BookingRequestAvailable = false;
			shipment.Consols.Cast<ForwardingConsol>().First().Creditor.OH_RSL_ShippingLine = shippingLine.PK;

			notifications = new Mock<IUserNotifications>();
			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("disallow sending Booking Request when the carrier doesn't have messaging capability", false, res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to ABC Company that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);

			shippingLine.RSL_BookingRequestAvailable = true;

			notifications = new Mock<IUserNotifications>();
			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertNull("not stopping sending Booking Request when the carrier has messaging capability", res);
			notifications.Verify(fake => fake.ShowMessage(@"You are trying to send the Booking Request message to ABC Company that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Never);
		}

		public void TestContinueWithSendingMessage_WhenMessageAccepted_ThenNotifyError()
		{
			var shipment = CreateStandAloneShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.MessageAccepted);
			AssertEquals(1, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending Booking Request when message accepted", false, res);
			notifications.Verify(x => x.ShowMessage("An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.", "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessage_WhenInterchangeReceiptAcknowledged_ThenNotifyError()
		{
			var shipment = CreateStandAloneShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.InterchangeReceiptAcknowledged);
			AssertEquals(1, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var notifications = new Mock<IUserNotifications>();
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending Booking Request when interchange receipt acknowledged", false, res);
			notifications.Verify(x => x.ShowMessage("An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.", "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessage_WhenMessageWithdrawCancelAccepted_ThenNotifyError()
		{
			var shipment = CreateStandAloneShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.MessageWithdrawCancelAccepted);
			AssertEquals(1, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending Booking Request when withdraw/cancel accepted", false, res);
			notifications.Verify(x => x.ShowMessage("An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.", "Confirmation"), Times.Once);
		}

		#endregion

		#region ContinueWithSendingMessageAmendment

		#region Booking Reference

		public void TestContinueWithSendingMessageAmendment_WhenBookingReferenceIsEmpty_ThenNotifyError()
		{
			var shipment = GivenShipmentWithEmptyBookingReference();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("disallow sending Amendment Booking Request when carrier booking reference is empty", false, res);
			notifications.Verify(x => x.ShowMessage("You are trying to send an amendment message, carrier booking reference is mandatory to send an amendment message.", "Confirmation"), Times.Once);
		}

		#endregion

		#region Original Message

		public void TestContinueWithSendingMessageAmendment_WhenNoOriginalMessage_ThenNotifyError()
		{
			var shipment = CreateValidShipment();
			var documentData = PrepareDocumentData(shipment);

			AssertEquals(1, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));
		}

		public void TestContinueWithSendingMessageAmendment_WhenOriginalMessageNoReply_ThenNotifyError()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent);

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("disallow sending Amendment Booking Request when original message no reply", false, res);
			var message = @"An original message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			notifications.Verify(x => x.ShowConfirmation(message, "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_WhenAmendmentMessageHasBeenSent_ThenNotifyError()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageSent);

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("disallow sending Amendment Booking Request when amendment message has been sent", false, res);
			var message = @"An amendment message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			notifications.Verify(x => x.ShowConfirmation(message, "Confirmation"), Times.Once);
		}

		#endregion

		#region Carrier Changed

		public void TestContinueWithSendingMessageAmendment_WhenCarrierChanged_ThenNotifyError()
		{
			var shipment = GivenShipmentCarrierCodeDifferentToLastSentCarrierCode();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("disallow sending Amendment Booking Request when carrier changed", false, res);
			notifications.Verify(x => x.ShowMessage("You are trying to send a Booking Request amendment message to a new carrier. Please verify Co-Load With (or Planned Carrier) and Reset to Original if you are sending Booking Request to new carrier.", "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_WhenCarrierNotChanged_ThenPass()
		{
			var shipment = GivenShipmentCarrierCodeSameToLastSentCarrierCode();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertNull("allow sending Amendment Booking Request when carrier not changed", res);
			notifications.Verify(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_WhenOriginalMessageWithoutCMP_ThenPass()
		{
			var shipment = GivenShipmentOriginalMessageWithoutCMP();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertNull("allow sending Amendment Booking Request when original message without CMP", res);
			notifications.Verify(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		#endregion

		#region Previous Message Is WithdrawCancelRequest

		public void TestContinueWithSendingMessageAmendment_PreviousMessageIsWithdrawCancelRequest_NoResponse()
		{
			var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest);

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			AssertEquals(2, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			AssertEquals(false, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
			notifications.Verify(x => x.ShowConfirmation(message, "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_PreviousMessageIsWithdrawCancelRequest_Rejected()
		{
			var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.MessageRejected);

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			AssertEquals(2, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			AssertEquals(null, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, "Confirmation"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_PreviousMessageIsWithdrawCancelRequest_InterchangeRejected()
		{
			var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.InterchangeRejected);

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			AssertEquals(2, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));

			AssertEquals(null, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, "Confirmation"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_PreviousMessageIsWithdrawCancelRequest_Accepted()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted, AutoEvents.MessageWithdrawCancelRequest, AutoEvents.MessageWithdrawCancelAccepted);

			AssertEquals(1, documentData.CalculateDataVersion(ShipmentDocumentNames.BookingRequest, true));
		}

		#endregion

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		#region Booking Reference

		public void TestContinueWithSendingMessageWithdrawal_WhenBookingReferenceIsEmpty_ThenNotifyError()
		{
			var shipment = GivenShipmentWithEmptyBookingReference();

			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var notifications = new Mock<IUserNotifications>();
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("disallow sending Withdrawal Booking Request when carrier booking reference is empty", false, res);
			notifications.Verify(x => x.ShowMessage("You are trying to send a Withdrawal/Cancellation message, carrier booking reference is mandatory to send a Withdrawal/Cancellation message.", "Confirmation"), Times.Once);
		}

		#endregion

		#region Carrier Changed

		public void TestContinueWithSendingMessageWithdrawal_WhenCarrierChanged_ThenNotifyError()
		{
			var shipment = GivenShipmentCarrierCodeDifferentToLastSentCarrierCode();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("disallow sending Withdrawal Booking Request when carrier changed", false, res);
			notifications.Verify(x => x.ShowMessage("You are trying to send a Booking Request withdrawal message to a new carrier. Sending Booking Request withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_WhenCarrierNotChanged_ThenPass()
		{
			var shipment = GivenShipmentCarrierCodeSameToLastSentCarrierCode();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertNull("allow sending Withdrawal Booking Request when carrier not changed", res);
			notifications.Verify(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal_WhenOriginalMessageWithoutCMP_ThenPass()
		{
			var shipment = GivenShipmentOriginalMessageWithoutCMP();

			var notifications = new Mock<IUserNotifications>();
			var extensions = new SeaShipmentBookingRequestMessagingExtensions(shipment);

			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertNull("allow sending Withdrawal Booking Request when original message without CMP", res);
			notifications.Verify(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		#endregion

		#endregion

		#region GetCarrierCodeWithFallback

		public void TestGetCarrierCodeWithFallback_WhenShipmentAttachedToConsol_ReturnConsolCreditorUSCCC()
		{
			var shipment = CreateAttachedToConsolShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be console's coload carrier", carrier.OH_Code == "ABC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CUSC", Constants.CountryCodes.UnitedStates);
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "CC1C");

			AssertEquals("carrier code should be USCCC", "CUSC", shipment.GetCarrierCodeWithFallback());
		}

		public void TestGetCarrierCodeWithFallback_WhenShipmentAttachedToConsol_FallbackToConsolCreditorC1C()
		{
			var shipment = CreateAttachedToConsolShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be console's coload carrier", carrier.OH_Code == "ABC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "CC1C");

			AssertEquals("carrier code should be C1C", "CC1C", shipment.GetCarrierCodeWithFallback());
		}

		public void TestGetCarrierCodeWithFallback_WhenShipmentAttachedToConsol_ReturnEmpty()
		{
			var shipment = CreateAttachedToConsolShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be console's coload carrier", carrier.OH_Code == "ABC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.DEA, "CDEA");

			AssertEquals("carrier code should be Empty", ZString.Empty, shipment.GetCarrierCodeWithFallback());
		}

		public void TestGetCarrierCodeWithFallback_WhenShipmentHasNoConsol_ReturnPlannedCarrierUSCCC()
		{
			var shipment = CreateStandAloneShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be Planned Carrier", carrier.OH_Code == "MRC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "PUSC", Constants.CountryCodes.UnitedStates);
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "PC1C");

			AssertEquals("carrier code should be USCCC", "PUSC", shipment.GetCarrierCodeWithFallback());
		}
		public void TestGetCarrierCodeWithFallback_WhenShipmentHasNoConsol_FallbackToPlannedCarrierC1C()
		{
			var shipment = CreateStandAloneShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be Planned Carrier", carrier.OH_Code == "MRC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "PC1C");

			AssertEquals("carrier code should be C1C", "PC1C", shipment.GetCarrierCodeWithFallback());
		}
		public void TestGetCarrierCodeWithFallback_WhenShipmentHasNoConsol_ReturnEmpty()
		{
			var shipment = CreateStandAloneShipment();

			var carrier = shipment.GetCarrierWithFallback();
			Assert("shpiment's carrier should be Planned Carrier", carrier.OH_Code == "MRC");

			var carrierCusCodes = carrier.CustomsCodes;
			carrierCusCodes.RemoveAndDeleteAll();
			carrierCusCodes.AddNew(OrgCusCode.CodeTypes.DEA, "PDEA");

			AssertEquals("carrier code should be Empty", ZString.Empty, shipment.GetCarrierCodeWithFallback());
		}

		#endregion

		#region Implementation

		ForwardingShipment GivenShipmentWithEmptyBookingReference()
		{
			return CreateAttachedToConsolShipment();
		}

		ForwardingShipment CreateValidShipment()
		{
			var shipment = CreateAttachedToConsolShipment();
			PopulateBookingReference(shipment);

			return shipment;
		}

		ForwardingShipment GivenShipmentOriginalMessageWithoutCMP()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEvents(documentData, AutoEvents.MessageSent, AutoEvents.MessageAccepted);

			Factory.Save();

			return shipment;
		}

		ForwardingShipment GivenShipmentCarrierCodeDifferentToLastSentCarrierCode()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "OLD", AutoEvents.MessageSent, AutoEvents.MessageAccepted);

			Factory.Save();

			return shipment;
		}

		ForwardingShipment GivenShipmentCarrierCodeSameToLastSentCarrierCode()
		{
			var shipment = CreateValidShipment();

			var documentData = PrepareDocumentData(shipment);
			CreateEventsWithCompanyCode(documentData, "CUSC", AutoEvents.MessageSent, AutoEvents.MessageAccepted);

			Factory.Save();

			return shipment;
		}

		ForwardingShipment CreateStandAloneShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_UniqueConsignRef = "S00001000";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "SGSIN";

			shipment.JS_E_DEP = ZDateTime.UtcToday.AddDays(2);
			shipment.JS_E_ARV = ZDateTime.UtcToday.AddDays(4);

			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.UtcToday.AddDays(6);
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_AdditionalTerms = "Follow the white rabbit :)";

			var note = "simple note";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, note);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Unit 13";
			consignor.MainAddress.Address2 = "4 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2000";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var pickupCfs = Factory.New<OrgHeader>();
			pickupCfs.OH_FullName = "PickupFromCo";
			pickupCfs.OH_RL_NKClosestPort = "AUSYD";
			pickupCfs.MainAddress.Address1 = "Unit 13";
			pickupCfs.MainAddress.Address2 = "4 Lost Lane";
			pickupCfs.MainAddress.City = "Sydney";
			pickupCfs.MainAddress.Postcode = "2000";
			pickupCfs.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.JS_OA_ExportReceivingDepot = pickupCfs.MainAddress.PK;

			var pickupFrom = Factory.New<OrgHeader>();
			pickupFrom.OH_FullName = "PickupFromCo";
			pickupFrom.OH_RL_NKClosestPort = "AUSYD";
			pickupFrom.MainAddress.Address1 = "Unit 13";
			pickupFrom.MainAddress.Address2 = "4 Lost Lane";
			pickupFrom.MainAddress.City = "Sydney";
			pickupFrom.MainAddress.Postcode = "2000";
			pickupFrom.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;

			var deliverToCFS = Factory.New<OrgHeader>();
			deliverToCFS.OH_FullName = "DeliverCFSCo";
			deliverToCFS.OH_RL_NKClosestPort = "SGSIN";
			deliverToCFS.MainAddress.Address1 = "Unit 1";
			deliverToCFS.MainAddress.Address2 = "4 What Lane";
			deliverToCFS.MainAddress.City = "Auckland";
			deliverToCFS.MainAddress.Postcode = "5022";
			deliverToCFS.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.JS_OA_ImportReleaseDepot = deliverToCFS.MainAddress.PK;

			var deliverTo = Factory.New<OrgHeader>();
			deliverTo.OH_FullName = "DeliverToCo";
			deliverTo.OH_RL_NKClosestPort = "SGSIN";
			deliverTo.MainAddress.Address1 = "Unit 1";
			deliverTo.MainAddress.Address2 = "4 What Lane";
			deliverTo.MainAddress.City = "Auckland";
			deliverTo.MainAddress.Postcode = "5022";
			deliverTo.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliverTo.MainAddress.PK;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MR Carrier";
			carrier.OH_RL_NKClosestPort = "CNSHA";
			carrier.OH_Code = "MRC";
			carrier.MainAddress.Address1 = "Unit 1";
			carrier.MainAddress.Address2 = "4 What Lane";
			carrier.MainAddress.City = "Shanghay";
			carrier.MainAddress.Postcode = "5022";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			var carrierContractNumber = Factory.New<CusEntryNumber>();
			carrierContractNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			carrierContractNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "4350006342";
			shipment.Numbers.Add(carrierContractNumber);

			return shipment;
		}

		ForwardingShipment CreateAttachedToConsolShipment()
		{
			var shipment = CreateStandAloneShipment();

			var consol = CreateConsol(Core.Constants.TransportModes.Sea);
			shipment.Consols.Add(consol);

			return shipment;
		}

		void PopulateBookingReference(ForwardingShipment shipment)
		{
			var bookingReference = Factory.New<CusEntryNumber>();
			bookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			bookingReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			bookingReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			bookingReference.CE_EntryNum = "10207000067891";

			shipment.Numbers.Add(bookingReference);
		}

		ForwardingConsol CreateConsol(string receivingAgentName = "ReceivingAgent")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			PopulateConsolAddresses(consol, receivingAgentName);
			return consol;
		}

		void PopulateConsolAddresses(ForwardingConsol consol, string receivingAgentName = "ReceivingAgent")
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_FullName = "ABC Company";
			creditor.OH_Code = "ABC";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			var creditorCusCodes = creditor.CustomsCodes;
			creditorCusCodes.RemoveAndDeleteAll();
			creditorCusCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CUSC", Constants.CountryCodes.UnitedStates);
			creditorCusCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "CC1C");

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = receivingAgentName;
			receivingAgent.OH_RL_NKClosestPort = "BEANR";
			receivingAgent.MainAddress.CompanyName = receivingAgentName;
			receivingAgent.MainAddress.Address1 = "Unit 589";
			receivingAgent.MainAddress.Address2 = "625 Drive";
			receivingAgent.MainAddress.City = "unknown city";
			receivingAgent.MainAddress.Postcode = "9541";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
		}

		IVisualizerDocumentData PrepareDocumentData(ForwardingShipment shipment)
		{
			var documentData = shipment.Factory.New<VisualizerDocumentData>();

			using (documentData.SuspendSettingHasChanges())
			{
				documentData.Parent = shipment;
				documentData.JDD_Name = ShipmentDocumentDataStoreNames.BookingRequest;
			}

			return documentData;
		}

		void CreateEvents(IVisualizerDocumentData documentData, params Event[] events)
		{
			var logs = (documentData as IStmALogParent).Logs;
			foreach (var @event in events)
			{
				logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, DocumentMST);
				Thread.Sleep(10);
				Factory.Save();
			}
		}

		KeyValuePair<string, string> DocumentMST => new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.BookingRequest);

		void CreateEventsWithCompanyCode(IVisualizerDocumentData documentData, string companyCode, params Event[] events)
		{
			var logs = (documentData as IStmALogParent).Logs;
			foreach (var @event in events)
			{
				logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, GetParametersForEvent(@event.Code, companyCode));
				Thread.Sleep(10);
				Factory.Save();
			}
		}

		KeyValuePair<string, string>[] GetParametersForEvent(string eventCode, string companyCode)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(DocumentMST);

			HashSet<string> sentMessagesEventCodesHashSet = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
			if (eventCode != null && sentMessagesEventCodesHashSet.Contains(eventCode))
			{
				result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, companyCode));
			}

			return result.ToArray();
		}

		#endregion
	}
}
