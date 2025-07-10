using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ContainerLoadPlanMessagingExtensions : BaseMessagingExtensions
	{
		public ContainerLoadPlanMessagingExtensions(IDocument document)
		{
			containerLoadPlan = document?.Data.Value as ContainerLoadPlan;
			Argument.NotNull(containerLoadPlan, nameof(containerLoadPlan));
		}

		readonly ContainerLoadPlan containerLoadPlan;

		#region ContinueWithSendingMessageAmendment

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			var message = Res.GetString("ed015416-0b57-49a8-ad49-51cf666a4175", "A message for this or a different container has previously been sent. Ningbo EDI Center does not support amendments or cancellation. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.");
			var information = Res.GetString("2c4f6fd7-fa1e-471a-849f-dc86b4b0afbe", "Information");
			notifications?.ShowMessage(message, information);
			return false;
		}

		#endregion
	}
}
