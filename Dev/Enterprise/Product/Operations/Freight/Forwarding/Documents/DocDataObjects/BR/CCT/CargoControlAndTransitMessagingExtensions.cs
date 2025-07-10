using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitMessagingExtensions : BaseMessagingExtensions
	{
		public CargoControlAndTransitMessagingExtensions(IDocument document)
		{
			cargoControlAndTransit = document.Data?.Value as CargoControlAndTransit;
			Argument.NotNull(cargoControlAndTransit, nameof(cargoControlAndTransit));
		}

		readonly CargoControlAndTransit cargoControlAndTransit;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (!CargoControlAndTransitHelper.CheckStaffCertificateValidAndShowMessage(notifications))
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

		public override string GetXmlNamespace() => "/CCTShipmentReport/1";
	}
}
