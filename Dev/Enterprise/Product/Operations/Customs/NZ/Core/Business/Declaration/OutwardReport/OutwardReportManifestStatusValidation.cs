using System.Collections.Generic;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	public class OutwardReportManifestStatusValidation : OutwardReportValidation
	{
		public OutwardReportManifestStatusValidation(OutwardReportManifestStatus parent)
			: base(parent)
		{
			deliveryNotificationParty = parent.DeliveryNotificationParty;
		}

		readonly DeliveryNotificationParty deliveryNotificationParty;

		#region Overriden Methods

		public override List<string> CheckErrorsBeforeGeneratingMessage(MessageBuilder.MessageTypes messageType)
		{
			ErrorList = base.CheckErrorsBeforeGeneratingMessage(messageType);
			if (!deliveryNotificationParty.Validation.CheckOutwardReportNotificationParty())
			{
				ErrorList.Add(NotifyPartyHasNoCode);
			}
			CheckOutwardReportNotificationPartyName();
			CheckOutwardReportNotificationPartyEmail();
			CheckOrgOrPartyAndEmailShouldBeEntered();

			return ErrorList;
		}

		#endregion

		#region Implementation
		const string NotifyPartyHasNoCode = "Delivery Notification Organization does not have a valid TSW Identification code configured.";

		internal void CheckOutwardReportNotificationPartyName()
		{
			if (deliveryNotificationParty.DeliveryNotificationPartyName.IsEmpty && !deliveryNotificationParty.DeliveryNotificationPartyEmail.IsEmpty)
			{
				ErrorList.Add(NameRequired);
			}
		}
		const string NameRequired = "You have entered a notification email address but no Delivery notification party name.";

		internal void CheckOutwardReportNotificationPartyEmail()
		{
			if (deliveryNotificationParty.DeliveryNotificationPartyEmail.IsEmpty && !deliveryNotificationParty.DeliveryNotificationPartyName.IsEmpty)
			{
				ErrorList.Add(EmailAddressRequired);
			}
		}
		const string EmailAddressRequired = "Email address is required when a Delivery Notification Party name is entered.";

		void CheckOrgOrPartyAndEmailShouldBeEntered()
		{
			deliveryNotificationParty.ClearAllPartiesAreBlank();
			if (deliveryNotificationParty.AllPartiesAreBlank)
			{
				ErrorList.Add(deliveryNotificationParty.Validation.AtLeastOnePartyShouldBeEnteredMessage);
			}
		}

		#endregion
	}
}
