using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	internal class EX1MessageProcessor : DeclarationMessageProcessor<EX1Response>
	{
		public EX1MessageProcessor(LoggingInformation logger)
			: base(logger, "Export")
		{
		}

		#region Overrides

		protected override bool IsWarningAboutTotalAmountInconsistencyRequired
		{
			get { return false; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendAcknowledgements.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendErrors.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ExportDeclarationsSendImpediments.Value; }
		}

		protected override DeclarationResponse PreviousResponsesWithSameMessageType
		{
			get
			{
				if (fPreviousResponsesWithSameMessageType == null)
				{
					var responseMessages = EntryHeader.Messages.OfType<TSWMessage>()
											.Where(message => message.EM_ReceiveTransmit == Enterprise.Messaging.Business.EDIInterchange.Direction.Receive)
											.OrderByDescending(message => message.EM_SystemCreateTimeUtc);

					var lastestMessageWithSameSubType = responseMessages.FirstOrDefault(message => message.EM_MessageSubType == Response.IncomingTSWMessage.EM_MessageSubType);
					BaseTSWResponse baseTSWResponse = null;

					if (BaseTSWResponse.TryParse(lastestMessageWithSameSubType, out baseTSWResponse))
					{
						fPreviousResponsesWithSameMessageType = new EX1Response(baseTSWResponse);
					}
				}

				return fPreviousResponsesWithSameMessageType;
			}
		}
		DeclarationResponse fPreviousResponsesWithSameMessageType;

		#endregion
	}
}
