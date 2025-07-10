using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOSendOriginalApplicator : EIDOBaseApplicator
	{
		public EIDOSendOriginalApplicator(ReleaseImportOrderSettings settings)
			: base(Res.GetString("cec7a4fe-1711-4d96-afee-4a8fc3965b36", "E-IDO: Send Original"), settings) { }

		protected override void ApplyShipments(IOperationalActionSectionLog log, Dictionary<BillOfLading, List<BillOfLadingContainer>> targets)
		{
			log.SetSectionProgressMax(targets.Count + 1);

			IEIDOMessageBuilder builder = EIDOMessageBuilderFactory.GetNewBuilder();
			ContainerPinGenerator generator = new ContainerPinGenerator();

			log.BumpSectionProgress();

			foreach (KeyValuePair<BillOfLading, List<BillOfLadingContainer>> pair in targets)
			{
				if (ValidateSet(log, pair))
				{
					generator.PopulateEmptyPins(pair.Value);

					foreach (BillOfLadingContainer container in pair.Value)
					{
						if (!AllowSendWhileResponsePending && IsPendingResponse(container))
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Error,
								Res.GetString("c0f57ecf-601f-4b6d-9647-356030c27406", "{0:G}-{1:G} is still waiting on a response."),
								pair.Key.JS_UniqueConsignRef, HyperlinkHelper.Link(container));
						}
						else
						{
							IEIDOMessagingData data = EIDOShipmentMessagingData.NewOriginal(container);
							EIDOMessage message = EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
							message.SetEventToAddOnSaving(container.GetType(), Events.MessageSent, GetParamtersForEvent(Events.MessageSent));
						}
					}
				}

				log.BumpSectionProgress();
			}
		}
	}
}


