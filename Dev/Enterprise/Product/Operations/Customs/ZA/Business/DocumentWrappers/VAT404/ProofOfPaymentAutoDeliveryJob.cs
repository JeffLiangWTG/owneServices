using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	[System.Serializable]
	class ProofOfPaymentAutoDeliveryJob : AutoDocumentDeliveryJob
	{
		public ProofOfPaymentAutoDeliveryJob(VAT404Document parent, ZGuid documentCommandPK) : base(parent, false, documentCommandPK) { }

#if NETFRAMEWORK
		public ProofOfPaymentAutoDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		protected VAT404Document Parent => DocumentSupportable as VAT404Document;

		protected override DeliveryInstructions GetDeliveryInstructions(DocumentPack pack)
		{
			if (Parent != null)
			{
				var result = new DeliveryInstructions(pack);
				result.Destination = DeliveryInstructionDestination.TakenFromContact;
				result.PrinterDelivery.PrintQueuePK = Parent.PrinterPK;
				result.AllowAutoDelivery = false;
				RemoveAllRecipients(result);
				foreach (var recipient in Parent?.DeliveryContacts.OfType<DocDeliveryContact>() ?? System.Array.Empty<DocDeliveryContact>())
				{
					AddRecipientIfValid(result, recipient);
				}
				return result;
			}
			else
			{
				return base.GetDeliveryInstructions(pack);
			}
		}

		void AddRecipientIfValid(DeliveryInstructions instructions, DocDeliveryContact contact)
		{
			if (contact != null)
			{
				bool isSafe = false;
				switch (contact.DeliveryMethod)
				{
					case ContactNotifyModes.Email:
						isSafe = !contact.Email.IsEmpty;
						break;
					case ContactNotifyModes.Print:
						isSafe = instructions.PrinterDelivery.PrintQueuePK.IsValid;
						break;
					case ContactNotifyModes.Fax:
						isSafe = !contact.Fax.IsEmpty;
						break;
				}
				if (isSafe)
				{
					instructions.Recipients.Add(contact);
				}
			}
		}

		protected override DummyDocumentEvents GetNewDummyDocumentEvents(INotifications notifications)
		{
			var result = base.GetNewDummyDocumentEvents(notifications);
			result.DocumentPrinted += (s, e) =>
			{
				notifications.Add(NotificationType.Information, Res.GetString("856A1E98-2879-4E7F-B536-A8791B190F1B", "Document for {0} has been queued for Delivery", Parent?.ToString() ?? ZString.Empty));
			};
			return result;
		}

		protected override string MessageOfNoRecipient => Res.GetString("6C7DAC62-63AB-4833-AE40-3AC560663402", "Document for {0} not delivered: No valid recipient was found.", Parent?.ToString() ?? ZString.Empty);
	}
}
