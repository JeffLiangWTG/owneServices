using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerReleaseMessagingExtensions : BaseMessagingExtensions, ICustomMessageWithdrawalSupporter
	{
		public SecureContainerReleaseMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			scr = document?.Data.Value as SecureContainerRelease;
			this.consol = consol;

			Argument.NotNull(scr, nameof(scr));
		}

		readonly SecureContainerRelease scr;
#pragma warning disable IDE0052 // Remove unread private members
		readonly ForwardingConsol consol;
#pragma warning restore IDE0052 // Remove unread private members

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => true;

		#region ICustomMessageWithdrawalSupporterMembers

		object ICustomMessageWithdrawalSupporter.GetMessageWithdrawalReason() => string.Empty;

		bool ICustomMessageWithdrawalSupporter.PopulateMessageWithdrawalReason(IDataObject dataObject, object reasonForSending) => true;

		#endregion
	}
}
