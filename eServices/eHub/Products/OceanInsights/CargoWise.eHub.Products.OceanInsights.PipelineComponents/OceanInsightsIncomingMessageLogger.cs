using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using MSMQT;


namespace CargoWise.eHub.Products.OceanInsights.PipelineComponents
{
	[Guid("8B5B7AF8-F241-4A78-8F90-DDDE871ADFE4")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	public class OceanInsightsIncomingMessageLogger : ComponentBase, IComponent
	{
		#region Properties

		protected override Guid ClassID
		{
			get { return new Guid("8B5B7AF8-F241-4A78-8F90-DDDE871ADFE4"); }
		}

		protected override string DisplayName
		{
			get { return "Incoming Transactional Message Logger"; }
		}

		#endregion

		#region Execute

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message);

			try
			{
				LoggerHelpers.LogComponentStart(logger, this);

				var emailSubject = message.Context.ReadPropertyString<OverrideEmailSubject>();
				var fileName = message.Context.ReadPropertyString<OverrideFilename>();
				var messageTrackingID = new Guid(message.Context.ReadPropertyString<MessageTrackingID>());
				var clientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
				var senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
				var inboxPK = new Guid(message.Context.ReadPropertyString<InternalTrackingID>());
				var messageStreamPosition = message.BodyPart.GetOriginalDataStream().Position;

				using (var transactionScope = GetTransactionScope(pipelineContext))
				{
					logger.InfoFormat("Insert To Inbox, messageTrackingId: {0}, senderId: {1}, clientId: {2}, inboxPk: {3}", messageTrackingID, senderID, clientID, inboxPK);

					GetInboxAccessor().InsertToInbox(
						senderID,
						Guid.Empty,
						inboxPK,
						MessageStatus.Processing,
						new eHubGatewayMessage()
						{
							MessageTrackingID = messageTrackingID,
							ClientID = clientID,
							ApplicationCode = "BIZ",
							SchemaName = string.Empty,
							SchemaType = MessageSchemaType.Xml,
							EmailSubject = string.IsNullOrEmpty(emailSubject) ? string.Empty : emailSubject,
							FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName,
							MessageStream = message.BodyPart.GetOriginalDataStream().CompressAndEncode()
						});

					transactionScope.Complete();
				}

				message.BodyPart.GetOriginalDataStream().Position = messageStreamPosition;
				return message;
			}
			catch (Exception ex)
			{
				logger.Error("Exception: ", ex);
				throw new Exception(ex.ToString()) { Source = Name };
			}
			finally
			{
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}

		protected virtual TransactionScope GetTransactionScope(IPipelineContext context)
		{
			var transactionNative = (IDtcTransaction)((IPipelineContextEx)context).GetTransaction();

			return new TransactionScope(TransactionInterop.GetTransactionFromDtcTransaction(transactionNative));
		}

		#endregion

		#region Methods

		public virtual IInboxAccessor GetInboxAccessor()
		{
			return DataAccessFactories.NewInboxAccessorInstance();
		}

		#endregion
	}
}
