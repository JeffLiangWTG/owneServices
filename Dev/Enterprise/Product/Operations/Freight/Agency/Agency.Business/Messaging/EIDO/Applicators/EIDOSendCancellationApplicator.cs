using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOSendCancellationApplicator : EIDOBaseApplicator
	{
		public EIDOSendCancellationApplicator(ReleaseImportOrderSettings settings)
			: base(Res.GetString("4202078e-5024-4cbf-98f6-8fe1a24ca283", "E-IDO: Send Cancellation"), settings) { }

		protected override void ApplyShipments(IOperationalActionSectionLog log, Dictionary<BillOfLading, List<BillOfLadingContainer>> targets)
		{
			log.SetSectionProgressMax(targets.Count + 1);

			IEIDOMessageBuilder builder = EIDOMessageBuilderFactory.GetNewBuilder();

			log.BumpSectionProgress();

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in targets)
			{
				if (ValidateSet(log, pair))
				{
					foreach (BillOfLadingContainer container in pair.Value)
					{
						if (!AllowSendWhileResponsePending && IsPendingResponse(container))
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Error,
								Res.GetString("228d759e-3449-41c5-a45b-88594bab42ed", "{0:G}-{1:G} is still waiting on a response."),
								pair.Key.JS_UniqueConsignRef, HyperlinkHelper.Link(container));
						}
						else if (HasAttachedEIDOMessages(container))
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Debug,
								Res.GetString("61b2ed86-6dc5-424b-9609-18f258456c7f", "Sending cancellation. ({0:G}-{1:G})"),
								pair.Key.JS_UniqueConsignRef, HyperlinkHelper.Link(container));

							IEIDOMessagingData data = EIDOShipmentMessagingData.NewCancellation(container);
							EIDOMessage message = EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
							message.SetEventToAddOnSaving(container.GetType(), Events.MessageWithdrawCancelRequest, GetParamtersForEvent(Events.MessageWithdrawCancelRequest));
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Debug,
								Res.GetString("09222956-3cad-4f54-aad2-43c81dec4a55", "Skipping container as it has no E-IDO messages to withdraw. ({0:G}-{1:G})"),
								pair.Key.JS_UniqueConsignRef, HyperlinkHelper.Link(container));
						}
					}
				}

				log.BumpSectionProgress();
			}
		}

		static bool HasAttachedEIDOMessages(BillOfLadingContainer container)
		{
			foreach (EDIMessage message in container.Messages)
			{
				if (message.EM_ApplicationCode == EDIMessage.ApplicationCodes.EIDO)
				{
					return true;
				}
			}

			return false;
		}
	}
}


