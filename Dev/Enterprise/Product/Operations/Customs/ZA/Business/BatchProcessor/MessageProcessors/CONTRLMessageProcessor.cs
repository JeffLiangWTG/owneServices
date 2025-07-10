using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class CONTRLMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public CONTRLMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "CONTRL Message";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SARSEDIMessage.MessageTypes.CONTRL };

		protected override bool RequiresPreProcessingCore => true;

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message)
		{
			var branchPK = message.EM_GB;
			BusinessObject linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			CONTRLMessageHelper helper = null;

			if (message is CONTRLEDIMessage contrlMessage)
			{
				helper = CONTRLMessageHelper.New(contrlMessage);
				if (helper is not null)
				{
					var outGoingMessage = LocateOutGoingMessage(message.Factory, helper);
					if (outGoingMessage?.EM_LinkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider linkedEDIFACTMessageAttachee)
					{
						linkedObject = (BusinessObject)linkedEDIFACTMessageAttachee;
						Logger.Log(Res.GetString("61590CEB-B010-4E7A-BF2E-E7F0F308AD5F", "Linking {2} Message: #{0}/{3} to job: {1}", message.EM_MessageNum, linkedEDIFACTMessageAttachee.JobIdentification, "CONTRL", message.Interchange?.EI_InterchangeNum));
						branchPK = outGoingMessage.EM_GB;
					}
				}
			}

			if (linkedObject == null)
			{
				discardReason = GetUnableToFindTheLinkedJobMessage("CONTRL", message);
			}
			return (branchPK, linkedObject, discardReason, helper);
		}

		protected override void ProcessMessageMain(EDIMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider edifactMessageAttacchee)
			{
				edifactMessageAttacchee.AddMessage(message);

				messagePK = ZGuid.Empty;
				message.Factory.Saved -= BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				var helper = CONTRLMessageHelper.New(message as ZAMessage);
				if (helper != null)
				{
					// TODO: Use EM_EM_RequestMessage to load outGoingMessage
					var outGoingMessage = LocateOutGoingMessage(message.Factory, helper);
					if (outGoingMessage != null)
					{
						successful = true;
						UpdateStatus(edifactMessageAttacchee, helper, outGoingMessage, message);
						message.EM_MessageInterpretation = helper.InterpretContrlFor(outGoingMessage);
					}
				}
			}
			message.EM_Status = successful ? ZAMessage.Status.ProcessedOK : ZAMessage.Status.Discarded;
		}

		bool IsMessageRelevant(ZAMessage outGoingMessage)
		{
			var cusdecMessage = outGoingMessage as CUSDECEDIMessage;
			return cusdecMessage != null;
		}

		ZGuid messagePK;
		ZString messagePK_EM_MessageSubType;
		BondedWarehouseMessageProcessorCreator BondedWarehouseMessageProcessorCreator => bondedWarehouseMessageProcessorCreator ??= new BondedWarehouseMessageProcessorCreator(Logger, GetNewBondedWarehouseMessageProcessor, GetFallbackNotificationGroupPK);
		BondedWarehouseMessageProcessorCreator bondedWarehouseMessageProcessorCreator;

		BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
		{
			return new BondedWarehouseMessageProcessor(messagePK, null, sendEmail, messagePK_EM_MessageSubType != MessageSubTypeCodes.Codes.Change);
		}

		bool ShouldUpdateWarehouse(ZString whsStatus)
		{
			return !whsStatus.IsEmpty && (WarehouseTransactionStatusList.IsPendingInward(whsStatus) || WarehouseTransactionStatusList.IsPendingOutward(whsStatus));
		}

		static ZAMessage LocateOutGoingMessage(BusinessObjectFactory factory, CONTRLMessageHelper helper)
		{
			var refNo = helper.MessageReferenceNumber;
			var outGoingMessageQuery = helper.GetOutgoingMessageQuery(refNo);
			return outGoingMessageQuery != null ? factory.Load<ZAMessage>(outGoingMessageQuery).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault() : null;
		}

		void UpdateStatus(IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider parentOfMessage, CONTRLMessageHelper helper, ZAMessage outGoingMessage, EDIMessage incomingMessage)
		{
			if (outGoingMessage is not REQDOCEDIMessage)
			{
				var actionCoded = helper.ActionCodedForMessage;
				if (actionCoded != null)
				{
					var calculator = parentOfMessage.GetCalculator(Core.Constants.CountryCodes.SouthAfrica);

					if (calculator != null)
					{
						var statusCode = ZString.Empty;
						var entry = parentOfMessage as CusEntryHeader;
						if (actionCoded == ActionCodedList.ThisLevelAndAllLowerLevelsRejected)
						{
							statusCode = calculator.GetMessageRejectedStatus(outGoingMessage);
							if (outGoingMessage.EM_MessageSubType != MessageSubTypeCodes.Codes.Cancellation)
							{
								ZAPermitHelper.UpdatePendingTransactions(entry, incomingMessage, outGoingMessage, Logger, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
							}
						}
						else if (actionCoded == ActionCodedList.ThisLevelAcknowledgedNextLowerLevelAcknowledgedIfNotExplicitlyRejected)
						{
							statusCode = calculator.GetMessageAcknowledgedStatus(outGoingMessage);
						}

						if (!statusCode.IsEmpty)
						{
							parentOfMessage.MessageStatus = statusCode;
							ReCalculateHeaderMessageStatus(parentOfMessage);
							Logger.Log(Res.GetString("9A489F7B-60D0-49A2-909A-3CDD365EA3D8", "Message Status of job:{0} has been updated to '{1}'.", parentOfMessage.JobIdentification, statusCode));
							if (Common.ZA.ZAMessageStatusList.IsError(statusCode))
							{
								if (entry != null && entry.SupportsBondedWarehousing && ShouldUpdateWarehouse(entry.CH_WarehouseTransactionStatus) && IsMessageRelevant(outGoingMessage))
								{
									messagePK = outGoingMessage.PK;
									messagePK_EM_MessageSubType = outGoingMessage.EM_MessageSubType;
									entry.Factory.Saved -= BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
									entry.Factory.Saved += BondedWarehouseMessageProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
								}
							}
						}
					}
				}
			}
		}

		void ReCalculateHeaderMessageStatus(IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider parentOfMessage)
		{
			if (parentOfMessage is Integration.Customs.ASYCUDA.IAsycudaBill asycudaBill)
			{
				var header = parentOfMessage.Factory.Load<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(asycudaBill.ABL_AMA);
				if (header != null)
				{
					var bills = LoadBills(parentOfMessage.Factory, header);
					var billsMessages = bills.Where(x => !x.ABL_MessageStatus.IsEmpty).Select(x => x.ABL_MessageStatus);

					var messageStatus = header.AMA_MessageStatus;
					if (billsMessages.Any(x => x == ZAMessageStatusList.Codes.AwaitingResponse))
					{
						messageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
					}
					else if (billsMessages.All(x => x == ZAMessageStatusList.Codes.Acknowledged))
					{
						messageStatus = ZAMessageStatusList.Codes.Acknowledged;
					}
					else if (billsMessages.Any(x => x == ZAMessageStatusList.Codes.Error))
					{
						messageStatus = ZAMessageStatusList.Codes.Error;
					}
					header.AMA_MessageStatus = messageStatus;
				}
			}
		}

		Integration.Customs.ASYCUDA.IAsycudaBill[] LoadBills(BusinessObjectFactory factory, Integration.Customs.ASYCUDA.IAsycudaManifestHeader sourceHeader)
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, sourceHeader.PK)
				.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);

			return factory.Load<Integration.Customs.ASYCUDA.IAsycudaBill>(query);
		}

		Guid GetFallbackNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return ZACustomsRegistry.Instance.FallbackNotificationGroup.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}
	}
}
