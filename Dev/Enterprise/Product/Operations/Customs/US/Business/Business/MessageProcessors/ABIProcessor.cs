using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: CBPMessageProcessorProvider(typeof(Enterprise.Customs.US.Business.MessageProcessors.ACSABIProcessor))]
namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public abstract class ACSABIProcessor : ABIProcessor<APLA, APLB, APLY>
	{
	}

	public abstract class ACEABIProcessor : ABIProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : Processor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>, IKeysForBlockingParallelProcessingProvider
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		#region IKeysForBlockingParallelProcessingProvider

		protected virtual ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(CBPEDIMessage message)
		{
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)string.Empty);
		}

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(Enterprise.Messaging.Business.EDIMessage message, LoggingInformation logger)
		{
			if (message is CBPEDIMessage cpbMessage)
			{
				return TryFindLinkedObject(cpbMessage);
			}
			else
			{
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, UCMPMessageProcessorFactory.GetMessageProcessorCannotProcessMessage(message));
			}
		}

		public ProcessingResult<ZGuid> GetBranch(Enterprise.Messaging.Business.EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

		protected virtual HashSet<string> TryFindSerializationKeys(CBPEDIMessage message, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData) => null;

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(Enterprise.Messaging.Business.EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var keys = TryFindSerializationKeys((CBPEDIMessage)message, linkedBusinessObjectMetaData);
			if (keys == null || keys.Count == 0 || keys.All(string.IsNullOrEmpty))
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		#endregion

		protected ABIProcessor()
		{
		}

		public new MQEDIMessage Message
		{
			get { return (MQEDIMessage)base.Message; }
			set { base.Message = value; }
		}

		protected virtual bool IsRelatedToSingleRecord
		{
			get { return true; }
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => USCustomsDataRegistry.Instance.ABIMessagesGroup;

		protected new MessageErrorCalculator MessageCalculator
		{
			get { return (MessageErrorCalculator)base.MessageCalculator; }
		}

		protected override Messaging.Business.MessageErrorCalculator GetNewMessageCalculator()
		{
			return new MessageErrorCalculator(Factory);
		}
		internal const string Reprocessing = "Reprocessing";

		protected void AddAIIDetailsToEmailIfRequired(CusEntryHeader entry, bool isAIIRecordsRequired, bool isAIIRecordsMayBeRequired, ZStringBuilder messageBody)
		{
			var declaration = entry != null ? entry.Declaration : null;

			var isPaperlessReleaseCodeOverridden = IsPaperlessReleaseCodeOverridden(entry);
			if (isAIIRecordsRequired)
			{
				messageBody.Append(@"<b><font color=""red"">" + Constants.RequestFullAII + "</font></b><br>");
				messageBody.Append(@"<b><font color=""red"">" + Constants.AII48Hours + "</font></b><br><br>");

				if (declaration != null && declaration.AreAllElectronicInvoicesClear())
				{
					messageBody.Append(@"<b><font color=""red"">" + AllAIIClear + "</font></b><br><br>");
				}
			}
			else if ((entry == null || entry.IsElectronicInvoicing) && (isAIIRecordsMayBeRequired || isPaperlessReleaseCodeOverridden))
			{
				var warningText = entry == null && isPaperlessReleaseCodeOverridden ? Constants.RequestAIIForLegacyEntry : Constants.RequestAII;
				messageBody.Append(@"<b><font color=""red"">" + warningText + "</font></b><br><br>");
			}
		}
		internal const string AllAIIClear = "However all the invoices on the declaration have already been sent and accepted by Customs. Please confirm with Customs and override 'AII Requested' indicator on Customs Declaration > Status Tab if no further Electronic Invoice Data is needed.";
		internal const string BelatedDispositionReleaseNotificationWarning = "The system has already processed a Release Notification message with more current details. Release details on the Declaration have not been updated as a result.";
		internal const string PNCNumbersReceived = "Prior Notice Confirmation Numbers received and attached to the Declaration";

		protected void MarkAsEmailSent()
		{
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageNum, Message.EM_MessageNum);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			EDIMessage[] responses = Factory.Load<EDIMessage>(query);
			if (responses != null)
			{
				foreach (EDIMessage response in responses)
				{
					response.EM_ApplicationReference = EmailSent;
				}
			}
		}
		internal const string EmailSent = "EmailSent";

		protected bool IsReferenceRequestedByServiceTask
		{
			get
			{
				EDIMessage originalMessage = Message.OriginalMessage;
				return (originalMessage != null && originalMessage.EM_SystemCreateUser == User.ServiceUserCode);
			}
		}

		bool IsPaperlessReleaseCodeOverridden(CusEntryHeader entry)
		{
			var result = false;

			var dispositionProviders = messageBlocks.OfType<IDispositionDetailProvider>();
			var dispositionBlock = dispositionProviders.FirstOrDefault(x => x.DispositionCode == CargoReleaseProcessingResultList.Codes.OverrideToIntensive);
			if (dispositionBlock != null)
			{
				if (entry != null)
				{
					foreach (MQEDIMessage message in entry.Declaration.Messages)
					{
						if (message != Message)
						{
							var dispositionBlocksFromPreviouseMessage = message.MessageBlock.MessageBlocks.OfType<IDispositionDetailProvider>();
							if (dispositionBlocksFromPreviouseMessage.Any(x => x.DispositionCode == CargoReleaseProcessingResultList.Codes.PaperlessEntry))
							{
								result = true;
								break;
							}
						}
					}
				}
				else
				{
					result = true;
				}
			}
			return result;
		}
	}
}
