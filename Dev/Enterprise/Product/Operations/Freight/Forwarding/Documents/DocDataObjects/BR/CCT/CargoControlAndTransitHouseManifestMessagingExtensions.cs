using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitHouseManifestMessagingExtensions : BaseMessagingExtensions
	{
		public CargoControlAndTransitHouseManifestMessagingExtensions(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public override string GetXmlNamespace() => "/CCTHouseCheckList/1";

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (!CargoControlAndTransitHelper.CheckStaffCertificateValidAndShowMessage(notifications))
			{
				return false;
			}

			if (!CargoControlAndTransitHelper.CheckShipmentIssueDateValidAndShowMessage(consol, notifications))
			{
				return false;
			}

			return null;
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			if (!CargoControlAndTransitHelper.CheckStaffCertificateValidAndShowMessage(notifications))
			{
				return false;
			}

			if (!CargoControlAndTransitHelper.CheckShipmentIssueDateValidAndShowMessage(consol, notifications))
			{
				return false;
			}

			return null;
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (!CargoControlAndTransitHelper.CheckStaffCertificateValidAndShowMessage(notifications))
			{
				return false;
			}

			return null;
		}
	}
}
