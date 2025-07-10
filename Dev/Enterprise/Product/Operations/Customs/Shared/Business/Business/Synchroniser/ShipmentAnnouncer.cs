using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	class ShipmentAnnouncer : IShipmentAnnouncer
	{
		public void Announce(BusinessObject shipment1)
		{
			ForwardingShipment shipment = shipment1 as ForwardingShipment;
			foreach (BaseJobDeclaration declaration in shipment.Declarations)
			{
				if (declaration.IsNotificationRequiredIfDeliveryAddressChangedByFreight)
				{
					EmailDef email = new EmailDef();
					email.Subject = Res.GetString("b4c5a127-5e43-4e7d-b0d1-7ce9104280b5", "Freight Department has changed the delivery Address on Shipment: {0}", shipment.JobNumber);
					email.Body = Res.GetString("4e3c02ec-6813-45a0-bd5d-cf2921988281", "The freight department has changed the delivery address on shipment: {0}. These changes will be reflected on the delivery address of the Declaration. {1}", shipment.JobNumber, declaration.AdditionalMailTextWhenDeliveryAddressChangedByFreight);
					foreach (string mailAddress in declaration.MailRecipentsWhenDeliveryAddressChangedByFreight)
					{
						email.AddRecipientForSystemCommunication(mailAddress);
					}
					try
					{
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}
			}
		}

		public bool IsNotificationRequiredIfDeliveryAddressChangedByFreight(BusinessObject shipment1)
		{
			bool result = false;
			ForwardingShipment shipment = shipment1 as ForwardingShipment;
			foreach (BaseJobDeclaration declaration in shipment.Declarations)
			{
				if (declaration.IsNotificationRequiredIfDeliveryAddressChangedByFreight)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public string AdditionalNotificationText
		{
			get
			{
				return Res.GetString("e35fbf84-e84e-4dda-b471-4dc88db49353", "An email will be sent to the staff member responsible for the Brokerage Job.");
			}
		}
	}
}
