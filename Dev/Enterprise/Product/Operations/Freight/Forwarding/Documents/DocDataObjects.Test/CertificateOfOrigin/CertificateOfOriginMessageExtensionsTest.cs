using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin
{
	sealed class CertificateOfOriginMessageExtensionsTest : TestCaseWithFactory
	{
		const string CertificateOfOriginNumber = "62.000000000.2024.000111";

		public void TestSendingAmendment()
		{
			var shipment = CreateShipment();

			var notifications = new Mock<IUserNotifications>();

			var extensions = new CertificateOfOriginMessagingExtensions(shipment, ShipmentDocumentNames.NZCFTACertificateOfOrigin);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can be sent when no message sent", true, res);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2023, 10, 17),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			AssertEquals(false, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can not be sent when waiting for a response", false, res);
			notifications.Verify(x => x.ShowMessage("This application cannot be sent for Certification as the application review is already in progress.", "Information"), Times.Once);

			shipment.Logs.ClearAllNotifications();

			CreateLog(shipment,
				Events.InterchangeSent,
				new ZDateTime(2023, 10, 17),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			AssertEquals(false, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can not be sent when waiting for a response", false, res);
			notifications.Verify(x => x.ShowMessage("This application cannot be sent for Certification as the application review is already in progress.", "Information"), Times.Exactly(2));
		}

		public void TestSendingAmendment_When_CooMessageRejected()
		{
			var shipment = CreateShipment();

			var notifications = new Mock<IUserNotifications>();

			var extensions = new CertificateOfOriginMessagingExtensions(shipment, ShipmentDocumentNames.NZCFTACertificateOfOrigin);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2023, 10, 17, 12, 13, 14),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			CreateLog(shipment,
				Events.MessageRejected,
				new ZDateTime(2023, 10, 17, 12, 14, 15),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			AssertEquals(false, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can be sent after a rejection received", true, res);
		}

		public void TestSendingAmendment_When_CooMessageAccepted()
		{
			var shipment = CreateShipment();
			var notifications = new Mock<IUserNotifications>();
			notifications
				.Setup(x => x.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);

			var extensions = new CertificateOfOriginMessagingExtensions(shipment, ShipmentDocumentNames.NZCFTACertificateOfOrigin);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can be sent, initially", true, res);

			AssertEquals("Message should not be an amendment, initially", false, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);
			AssertEquals("Amendment cannot be sent before a message has been accepted", false, res);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2023, 10, 17, 12, 13, 14),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			CreateLog(shipment,
				Events.MessageAccepted,
				new ZDateTime(2023, 10, 17, 12, 14, 15),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, CertificateOfOriginNumber));

			AssertEquals("Message should be an amendment after message accepted", true, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can be resent after a message has been accepted", true, res);
			notifications.Verify(x =>
				x.ShowConfirmation(
					$"An NZCFTA Certificate of Origin {CertificateOfOriginNumber} has already been issued for this shipment. Submitting this new application will cancel and replace your previous application, invalidating the existing certificate.\r\n\r\nThis operation must only be done if the Cancel and Replace implications are understood.",
					"Critical Warning",
					"Please type the following to continue:",
					"I understand the impact"),
				Times.Once);
		}

		public void TestReSending_When_CooMessageWithdrawn()
		{
			var shipment = CreateShipment();

			var notifications = new Mock<IUserNotifications>();

			var extensions = new CertificateOfOriginMessagingExtensions(shipment, ShipmentDocumentNames.NZCFTACertificateOfOrigin);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			CreateLog(shipment,
				Events.MessageSent,
				new ZDateTime(2023, 10, 17, 12, 13, 14),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			AssertEquals("Message cannot be resent until withdrawl", false, extensions.ContinueWithSendingMessage(notifications.Object));

			CreateLog(shipment,
				Events.MessageWithdrawCancelAccepted,
				new ZDateTime(2023, 10, 17, 12, 13, 42),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ShipmentDocumentNames.NZCFTACertificateOfOrigin));

			AssertEquals(false, extensions.IsSendingAmendment());

			res = extensions.ContinueWithSendingMessage(notifications.Object);
			AssertEquals("Message can be resent after withdrawl", true, res);
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "CNSZX";
			shipment.JS_UniqueConsignRef = "S00001527";
			CreateAddresses(shipment);
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			return shipment;
		}

		void CreateAddresses(ForwardingShipment shipment)
		{
			var consignorDocumentaryAddress = Factory.New<OrgHeader>();
			consignorDocumentaryAddress.OH_FullName = "Consignor";
			consignorDocumentaryAddress.OH_RL_NKClosestPort = "NZAKL";
			consignorDocumentaryAddress.MainAddress.Address1 = "83 Ulster road";
			consignorDocumentaryAddress.MainAddress.Address2 = "Unit52";
			consignorDocumentaryAddress.MainAddress.City = "New Zealand";
			consignorDocumentaryAddress.MainAddress.Postcode = "1010";
			consignorDocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorDocumentaryAddress.MainAddress.PK;

			var consigneeDocumentaryAddress = Factory.New<OrgHeader>();
			consigneeDocumentaryAddress.OH_FullName = "Consignee";
			consigneeDocumentaryAddress.OH_RL_NKClosestPort = "CNSZX";
			consigneeDocumentaryAddress.MainAddress.Address1 = "Unit 100";
			consigneeDocumentaryAddress.MainAddress.Address2 = "11 Why Street";
			consigneeDocumentaryAddress.MainAddress.City = "Beijing";
			consigneeDocumentaryAddress.MainAddress.Postcode = "999";
			consigneeDocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.MainAddress.PK;

			var manufacturerDocumentaryAddress = Factory.New<OrgHeader>();
			manufacturerDocumentaryAddress.OH_FullName = "Manufacturer";
			manufacturerDocumentaryAddress.OH_RL_NKClosestPort = "NZAKL";
			manufacturerDocumentaryAddress.MainAddress.Address1 = "83 Ulster road";
			manufacturerDocumentaryAddress.MainAddress.Address2 = "Unit52";
			manufacturerDocumentaryAddress.MainAddress.City = "New Zealand";
			manufacturerDocumentaryAddress.MainAddress.Postcode = "1010";
			manufacturerDocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ManufacturerDocAddress.E2_OA_Address = manufacturerDocumentaryAddress.MainAddress.PK;
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "CNSZX";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 01, 01, 00, 00, 00);
			transport.JW_ETA = new ZDateTime(2020, 02, 01, 00, 00, 00);
			transport.JW_VoyageFlight = "KH6754";
			transport.JW_Vessel = "VesselData";
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "CNSZX";

			return consol;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}
	}
}
