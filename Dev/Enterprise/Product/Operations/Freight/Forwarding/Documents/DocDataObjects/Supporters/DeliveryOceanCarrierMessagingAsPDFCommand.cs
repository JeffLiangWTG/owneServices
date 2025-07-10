using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents
{
	sealed class DeliveryOceanCarrierMessagingAsPDFCommand : CustomCommand
	{
		public DeliveryOceanCarrierMessagingAsPDFCommand(ForwardingConsol consol, string dataContext)
		{
			this.consol = consol;
			this.dataContext = dataContext;
		}

		public override string Id => CommandIds.SendMessage;

		public override string Caption => CommandResources.Captions.SendMessage;

		public override bool IsEnabled
		{
			get
			{
				var data = GetContactAndEmail();
				return data.hasData && !data.Email.IsEmpty && !data.Contact.IsEmpty;
			}
		}

		public override bool IsVisible => true;

		public override bool Invoke()
		{
			var sent = false;
			switch (dataContext)
			{
				case DataContext.BookingRequest:
				case DataContext.ShippingInstruction:
					{
						if (documentInfo?.Document?.Data?.Value is CarrierMessageData data)
						{
							sent = Deliver(data);
						}
						break;
					}
				case DataContext.VerifiedGrossMass:
					{
						if (documentInfo?.Document?.Data?.Value is VerifiedGrossMass data)
						{
							sent = Deliver(data);
						}
						break;
					}
				case DataContext.ShippingOrder:
					{
						if (documentInfo?.Document?.Data?.Value is ShippingOrder data)
						{
							sent = Deliver(data);
						}
						break;
					}
			}

			return sent;
		}

		void Log(IServiceContainer services, IDocument document)
		{
			services.Resolve<IEventBroker>().Publish(new MessageSentEvent(document));
		}

		bool Deliver(object data)
		{
			bool sent = false;
			if (data != null && (documentInfo.Document is IDocument document))
			{
				var userNotification = documentInfo.Services.Resolve<IUserNotificationService>();
				if (IsValid(document, userNotification))
				{
					var menuName = GetMenuName(dataContext);
					var result = Globals.Message.Show(Res.GetString("37BB3C3B-44BA-41B3-BCE0-548167A4731F", "The {0} will be sent to Carrier’s contact {1}’s email address {2}.\r\nDo you want to continue to send this {3}?", menuName, Contact, Email, menuName), Res.GetString("a2e6c5a8-158f-4ad4-89c0-f22252122c86", "Please confirm the email address"), ZMessageBoxButtons.OKCancel, ZDialogResult.Cancel);

					if (result == ZDialogResult.OK)
					{
						var service = documentInfo.Services;
						Deliver(service, consol, document);
						Log(service, document);
						userNotification.ShowMessage(Res.GetString("6159ee60-26b6-4afc-8498-3c40ec36f871", "Sent successfully."), Res.GetString("8958813d-4287-4a4f-9627-9bb6a6c96e80", "Successful"));
						sent = true;
					}
					else
					{
						userNotification.ShowMessage(Res.GetString("bf198065-4dfc-4f77-96a0-cde0bf9ee79e", "Sent failed."), Res.GetString("81ef5431-1722-4df0-9e18-62b73b73b29b", "Failed"));
					}
				}
			}
			return sent;
		}

		void Deliver(IServiceContainer services, IDocumentSupportable parent, IDocument primaryDocument)
		{
			using (var pack = new DocumentPack())
			{
				var contact = new DocDeliveryContact(consol.Factory);
				contact.Name = Contact;
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = "PDF";
				contact.Email = Email;

				var printDocumentDelivery = new PrintDocumentDelivery(documentInfo, "ALL");

				string emailSubject = $"[ediDocManager CON {consol.JK_UniqueConsignRef}]";

				services.Resolve<IDocumentDeliveryService>().Deliver(parent, emailSubject, new IDocumentDelivery[] { printDocumentDelivery }, contact, false);
			}
		}

		bool IsValid(IDocument document, IUserNotificationService notificationService)
		{
#if DEBUG
			if (Globals.IsTest && SuspendValidation)
			{
				return true;
			}
#endif

			var data = document.Data;

			var caption = Res.GetString("C2C6EDA8-B370-40BF-A213-0AAA70F132C7", "Sending Message");

			if (data == null)
			{
				notificationService.ShowMessage(Res.GetString("EC4D455E-8CC7-412B-BE66-F8CFBC06F3F2", "Document data could not be found."), caption);
				return false;
			}

			if (data.HasChanges)
			{
				notificationService.ShowMessage(Res.GetString("4E5B0B90-A61B-493D-A840-649F7233A273", "Please save all changes before sending."), caption);
				return false;
			}

			if (document.HasErrors())
			{
				notificationService.ShowMessage(Res.GetString("31A5C46A-94B3-445F-AA17-4B2B0478F398", "This document contains errors. Please fix all errors before sending."), caption);
				return false;
			}

			if (document.HasMessageErrors())
			{
				notificationService.ShowMessage(Res.GetString("1E0E1669-F632-4682-B284-63B3FA4C0223", "This document contains message errors. Please fix all message errors before sending."), caption);
				return false;
			}

			return true;
		}

		(ZString Contact, ZString Email, bool hasData) GetContactAndEmail()
		{
			switch (dataContext)
			{
				case DataContext.BookingRequest:
				case DataContext.ShippingInstruction:
					{
						if (documentInfo?.Document?.Data?.Value is CarrierMessageData data)
						{
							return (data.Recipient.Contact, data.Recipient.Email, true);
						}
						break;
					}
				case DataContext.VerifiedGrossMass:
					{
						if (documentInfo?.Document?.Data?.Value is VerifiedGrossMass data)
						{
							return consol.IsCoLoad ? (data.FreightForwarder.Contact, data.FreightForwarder.Email, true) : (data.Carrier.Contact, data.Carrier.Email, true);
						}
						break;
					}
				case DataContext.ShippingOrder:
					{
						if (documentInfo?.Document?.Data?.Value is ShippingOrder data)
						{
							return (data.Carrier.Contact, data.Carrier.Email, true);
						}
						break;
					}
			}
			return (string.Empty, string.Empty, false);
		}

		string GetMenuName(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.BookingRequest:
					return Res.GetString("0b85dcdc-d196-4ec8-a1de-f7f669a447a2", "Booking Request");
				case DataContext.ShippingOrder:
					return Res.GetString("30775214-3866-4ac2-b1b6-f93f79d4a26d", "Shipping Order");
				case DataContext.VerifiedGrossMass:
					return Res.GetString("373ac874-7ecd-4377-ab3e-63ee47e9b67d", "Verified Gross Container Weight");
				case DataContext.ShippingInstruction:
					return Res.GetString("5be0ed79-573c-4207-aede-9f438c25dfcd", "Shipping Instruction");
				default:
					return string.Empty;
			}
		}

		public ZString Email
		{
			get
			{
				return GetContactAndEmail().Email;
			}
		}

		public ZString Contact
		{
			get
			{
				return GetContactAndEmail().Contact;
			}
		}

		readonly ForwardingConsol consol;

		readonly string dataContext;

#if DEBUG
		public bool SuspendValidation { get; set; }
#endif

	}
}
