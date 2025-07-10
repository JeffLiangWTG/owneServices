using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NotificationsHandler = Enterprise.DocumentVisualizer.Business.NotificationsHandler;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class PublishHouseBillCommand : CustomCommand
	{
		public PublishHouseBillCommand(ForwardingShipment parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));

			isEnabled = !new[] { FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished,
				FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication,
				FreightConstants.BillOfLadingBillStatus.Codes.Surrendered,
				FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper,
				FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred }.Contains(parent.JS_ElectronicBillOfLadingStatus.ToString());
		}

		readonly ForwardingShipment parent;

		public override string Id => CommandIds.SendMessage;

		public override string Caption => Res.GetString("96dff096-6a50-4bb6-b269-d3d4a0b7341c", "Publish");

		public override bool IsEnabled => isEnabled;
		bool isEnabled;

		public override bool IsVisible => parent.IsEditingElectronicBOL;

		public override bool Invoke()
		{
			var sent = false;
			var documentData = documentInfo.DocumentData?.Parent;

			if (documentData is ForwardingShipment shipment)
			{
				var userNotification = documentInfo.Services.Resolve<IUserNotificationService>();
				if (CheckAllowSendMessage() && IsValid(documentInfo.Document, shipment, userNotification))
				{
					var menuItem = shipment.Factory.Load<VisualizerMenuItem>(ShipmentSystemFormMenuItems.BillOfLadingPK);
					var notificationsHandler = new NotificationsHandler();

					shipment.JS_ElectronicBillOfLadingVersion++;

					var sender = new HouseBillNotificationMessageSender();
					sent = sender.SendMessage(shipment, menuItem, notificationsHandler);

					if (sent)
					{
						AddCopyToEDocs(shipment);

						shipment.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication;
						ZExceptionReporting.ProcessWithSaveExceptionHandling(shipment.Factory.Save, null);

						isEnabled = false;
						userNotification.ShowMessage(Res.GetString("c707cc79-658e-4db2-8f5e-c7da0fd9e14e", "Published successfully."), Res.GetString("72fa7d03-2ced-4c8d-ac65-4df374d199c7", "Successful"));
					}
					else
					{
						userNotification.ShowMessage(Res.GetString("a4ea08cc-9b3c-4c98-a9ed-223ff4d41d46", "This document contains message errors. Please fix all message errors before publishing."), Res.GetString("209a74f5-1681-4c59-b01b-f0c33b7432de", "Publishing"));
					}

					Notify(new MessageSentEvent(documentInfo.Document));
				}
			}

			return sent;
		}

		void AddCopyToEDocs(ForwardingShipment shipment)
		{
			var query = new ZQuery();
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, "SEHB");

			var sEHBDocType = shipment.Factory.LoadTop1<RefDocType>(query);

			if (sEHBDocType?.RT_LogSystemCreatedDocsToEDocs ?? false)
			{
				Notify(new ProgressInfoEvent(Res.GetString("51a6286e-fd61-498d-bf9a-50852acab8e0", "Attaching copy to eDocs")));

				var eDocDeliveryParameters = CreateEDocsDeliveryParameters(shipment, sEHBDocType);
				documentInfo.Document.AddCopyToEDocs(eDocDeliveryParameters);
			}
		}

		IEDocsDeliveryParameters CreateEDocsDeliveryParameters(ForwardingShipment shipment, RefDocType refDocType)
		{
			return new EDocsDeliveryParameters
			{
				DocumentName = documentInfo.Descriptor?.Name,
				DocumentTitle = documentInfo.Descriptor?.PrintInstructions?.Title,
				DocumentType = refDocType.RT_DocType,
				AttachedFileName = shipment.JS_ElectronicBillOfLadingHouseBill + "_" + shipment.JS_ElectronicBillOfLadingVersion,
				BusinessObject = shipment
			};
		}

		bool CheckAllowSendMessage()
		{
			var security = documentInfo?.Services?.Resolve<IDocumentSecurityService>();

			if (!security?.CanSendMessage ?? false)
			{
				security.ShowSendMessageError();
				return false;
			}

			return true;
		}

		bool IsValid(IDocument document, ForwardingShipment shipment, IUserNotificationService notificationService)
		{
			var data = document.Data;

			var caption = Res.GetString("3a36405a-a01b-4bfb-911b-099c8014ac66", "Sending Message");

			if (data == null)
			{
				notificationService.ShowMessage(Res.GetString("6a0d580b-2940-40f7-ab1c-765e4dd0b70c", "Document data could not be found."), caption);
				return false;
			}

			if (data.HasChanges)
			{
				notificationService.ShowMessage(Res.GetString("19662f62-4dbd-4288-8e4f-142b2f22d694", "Please save all changes before sending."), caption);
				return false;
			}

			if (!shipment.CheckElectronicBOLMinimumRequirements())
			{
				notificationService.ShowMessage(Res.GetString("0014dcae-c77b-4ca7-b27e-85871524ea6c", "There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing."), caption);
				return false;
			}

			return true;
		}

		void Notify<T>(T obj) where T : class
		{
			if (obj != null)
			{
				var broker = documentInfo?.Services?.Resolve<IEventBroker>();
				broker?.Publish(obj);
			}
		}
	}
}
