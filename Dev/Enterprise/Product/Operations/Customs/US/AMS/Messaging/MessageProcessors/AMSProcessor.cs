using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: CBPMessageProcessorProvider(typeof(Enterprise.Customs.US.AMS.Messaging.Business.AMSProcessor))]
namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Instantiated through reflection")]
	public abstract class AMSProcessor : Processor<APLACR, APLACR, APLZCR>, IKeysForBlockingParallelProcessingProvider
	{
		protected AMSProcessor()
		{
		}

		public new AMSEDIMessage Message
		{
			get { return (AMSEDIMessage)base.Message; }
			set { base.Message = value; }
		}

		protected IManifestMessageAttachee LinkToMessageAttacheeAndUpdateMessageSubType(ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch)
		{
			return LinkToMessageAttacheeAndUpdateMessageSubType(carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, "", movementMatch);
		}

		protected IManifestMessageAttachee LinkToMessageAttacheeAndUpdateMessageSubType(ZString carrierCode, ZString manifestSequenceNumber, ZString vesselName, ZString voyageNumber, ZString districtPortOfUnladingCode, ZDate estimatedDate, ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString refNumQualifier, ZString refNum, ZString inBondNumber, Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch)
		{
			var message = Message;
			var result = message.EM_LinkedObject as IManifestMessageAttachee;
			if (result == null)
			{
				var linker = (IManifestMessageAttacheeMessageLinker)ObjectFactory.Get("ManifestMessageAttacheeMessageLinker");
				result = linker.MatchManifestAndLink(message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, districtPortOfUnladingCode, estimatedDate, billOfLadingIssuerCode, billOfLadingNumber, refNumQualifier, refNum, inBondNumber, movementMatch, new AMSBillMatchingComparer());
				if (result != null && result.Branch is GlbBranch branch && branch.PK is ZGuid branchPK && message.EM_GB != branchPK)
				{
					message.EM_GB = branchPK;
					messageBranchEnvironment = DisposableEnvironment.ForBranch(branchPK.ToGuid());
				}
			}
			return result;
		}
		IDisposable messageBranchEnvironment;

		public sealed override void Process()
		{
			try
			{
				ProcessCore();
			}
			finally
			{
				messageBranchEnvironment?.Dispose();
				messageBranchEnvironment = null;
			}
		}

		protected abstract void ProcessCore();

		#region Notification Email

		protected override void GenerateHtmlEmailAndSendToOriginalOrGroup(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, bool hasWarning = false, string warningMessage = "")
		{
			EmailDef email;
			new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, out email, branchForEmailLogo, hasWarning, warningMessage);
			email.SetupBusinessEntityInfo(sourceBusinessObject);

			var branch = branchForEmailLogo ?? GlbBranch.CurrentBranch;
			var notificationRegistryItem = FreightDataRegistry.Instance.USAMSGroupNotification;

			var parent = ((Customs.Business.CusInBondMoveHeader)sourceBusinessObject)?.Header;
			if (parent != null)
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, parent.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.TransferredCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "TYP=HVL");

				if (Factory.Exists(typeof(StmALog), query))
				{
					notificationRegistryItem = FreightDataRegistry.Instance.USHVLVAMSGroupNotification;
				}
			}

			var emailNotificationOptions = notificationRegistryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branchForEmailLogo != null ? branch.PK.ToGuid() : Guid.Empty, Guid.Empty);

			var sendErrorOnly = emailNotificationOptions.SendErrorOnly;
			var emailSendMode = emailNotificationOptions.SendMode;
			var emailGroup = emailNotificationOptions.SendGroupPK;

			if (!sendErrorOnly || (sendErrorOnly && isFailure))
			{
				var emailRepCal = new EmailRecipientCalculator(emailSendMode, emailGroup, GetEmailAddressToSendTo(), ZGuid.Empty);
				emailRepCal.SendNotifications(Factory, email, notificationRegistryItem);
			}
		}

		protected override IRegistryItem GetEmailGroupRegistryItem()
		{
			return null;
		}

		#endregion

		#region IKeysForBlockingParallelProcessingProvider

		protected abstract ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(EDIMessage message);

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			if (message is not AMSEDIMessage)
			{
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, UCMPMessageProcessorFactory.GetMessageProcessorCannotProcessMessage(message));
			}
			return TryFindLinkedObject(message);
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var keys = new HashSet<string>();
			var linkedObject = QueryLinkedObject(message);
			if (linkedObject is IManifestMessageAttachee messageAttachee)
			{
				var inbondNumber = messageAttachee.InBondNumber;
				if (string.IsNullOrEmpty(inbondNumber))
				{
					keys.Add(messageAttachee.JobNumber);
				}
				else
				{
					keys.Add(inbondNumber);
				}
			}
			if (keys.Count == 0 || keys.All(string.IsNullOrEmpty))
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		protected BusinessObject QueryLinkedObject(EDIMessage message)
		{
			var linkedObject = message.EM_LinkedObject;
			var messageNum = message.EM_MessageNum;
			if (!messageNum.IsEmpty
				&& QueryOriginMessage(message) is CBPEDIMessage originalMessage
				&& originalMessage.EM_LinkedObject != null)
			{
				linkedObject = originalMessage.EM_LinkedObject;
			}
			return linkedObject;
		}

		protected EDIMessage QueryOriginMessage(EDIMessage message) => new CBPEDIMessage.Loader(message.Factory).LoadTop1WithDirectionOrderByCreatTime(message.EM_ApplicationCode, message.EM_MessageNum, CBPEDIMessage.Direction.Transmit, null);
	}

	#endregion
}
