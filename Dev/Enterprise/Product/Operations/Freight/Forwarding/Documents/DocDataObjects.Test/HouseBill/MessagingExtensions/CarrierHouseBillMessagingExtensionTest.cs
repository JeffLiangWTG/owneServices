using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CarrierHouseBillMessagingExtensionTest : TestCaseWithFactory
	{
		public void TestGetXmlNamespace()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBill("zzz", "zzz");
			houseBill.IsDraft = true;

			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData.SetupGet(d => d.Value).Returns(houseBill);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(d => d.DocumentName).Returns("Bill Of Lading");

			shipment.IsEditingElectronicBOL = false;
			AssertNull(HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object).GetXmlNamespace());

			shipment.IsEditingElectronicBOL = true;
			AssertEquals(DocDataConstants.XmlNamespaces.BLData, HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object).GetXmlNamespace());
		}

		public void TestGetDocumentaryOverrideDocumentName()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBill("zzz", "zzz");
			houseBill.IsDraft = true;

			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData.SetupGet(d => d.Value).Returns(houseBill);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(d => d.DocumentName).Returns("Bill Of Lading");

			shipment.IsEditingElectronicBOL = false;
			AssertNull(HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object).GetDocumentaryOverrideDocumentName());

			shipment.IsEditingElectronicBOL = true;
			AssertEquals(ShipmentDocumentNames.DraftBill, HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object).GetDocumentaryOverrideDocumentName());
		}

		public void TestContinueWithSendingMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBill("zzz", "zzz");
			houseBill.IsDraft = true;

			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData.SetupGet(d => d.Value).Returns(houseBill);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(d => d.DocumentName).Returns("Bill Of Lading");

			var extensions = HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Message can only be sent when Shipping Instruction received.", false, res);
			notifications.Verify(n => n.ShowMessage("The Draft Bill of Lading cannot be sent until Shipping Instruction received from Booking Party.", "Information"), Times.Once);

			var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
			log.UpdateReference($"|{Params.Type}={Core.Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicShippingInstruction}");

			extensions = HouseBillMessagingExtensionCreator.New(shipment, document.Object, messageInstructions.Object);
			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Message can be sent.", null, res);
		}
	}
}
