using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Integration;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	abstract class ILCustomCommandBaseTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("shipment is must", () => CreateCommand(null));
		}

		public void TestId()
		{
			AssertEquals(CommandId, command.Id);
		}

		public void TestCaption()
		{
			AssertEquals(CommandCaption, command.Caption);
		}

		protected (Mock<IDocumentInfo>, Mock<IUserNotificationService>, EventBroker) CreateDocumentInfo(bool canSendMessage, bool hasChanges, bool hasErrors, bool hasMessageErrors)
		{
			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(canSendMessage);

			dynamicData.SetupGet(d => d.HasChanges).Returns(hasChanges);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var dataObject = GetMessageDocDataObject(shipment);

			var documentData = Factory.New<VisualizerDocumentData>();

			documentData.JDD_Name = "BaseCustomCommandTest";
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_ParentTableCode = shipment.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);
			document.SetupGet(di => di.Name).Returns(GetDocumentName());
			document.SetupGet(di => di.DataContext).Returns(GetDataContext());
			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			if (hasErrors)
			{
				var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, DocumentVisualizer.Core.NotificationType.Error, "Error") };
				document.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			if (hasMessageErrors)
			{
				var documentNotifications = new Notification[] { new Notification(new Mock<INotificationSource>().Object, DocumentVisualizer.Core.NotificationType.MessageError, "Message Error") };
				document.SetupGet(d => d.Notifications).Returns(documentNotifications);
			}

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);

			dynamicData.SetupGet(di => di.Value).Returns(dataObject);

			return (documentInfo, notificationService, broker);
		}

		protected bool SaveAndInvokeCommand()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.IL.IILCustomsDataRegistry>().SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				shipment.Factory.Save();
				var result = command.Invoke();
				return result;
			}
		}

		protected abstract string GetDocumentName();

		protected abstract string GetDataContext();

		protected abstract DocDataObject GetMessageDocDataObject(ForwardingShipment shipment);

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			command = CreateCommand(shipment);
		}

		protected abstract ILCustomCommandBase CreateCommand(ForwardingShipment shipment);

		protected abstract string CommandId { get; }

		protected abstract string CommandCaption { get; }

		protected abstract void UpdateMessageReference(string reference);

		protected string GetEventParameters()
			=> $"|DEP=Customs|MST={GetDocumentName()}" + GetRFN();

		protected abstract ZString MessageReference { get; }

		protected ForwardingShipment shipment;
		protected ILCustomCommandBase command;

		string GetRFN()
			=> MessageReference.IsEmpty ? string.Empty : $"|RFN={MessageReference}";
	}
}
