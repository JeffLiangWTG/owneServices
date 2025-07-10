using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse, Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	public class InBondProcessor : ACEABIProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void Process()
		{
			var delayEmailReport = false;
			emailReportThatHasBeenDelayed = null;
			IMessageAttachee linkedHeader = GetLinkedHeader();
			bool hasLinesCleared = false;
			bool hasLinesRejected = false;

			var currentBillNumber = ZString.Empty;
			var hasQt95Blocks = messageBlocks.Any(x => x is IINBQT95);
			var htmlTable = hasQt95Blocks ? new HtmlTableCreator(new string[] { "Master Bill Number", "ID", "Message" }) : new HtmlTableCreator(new string[] { "Error Narrative Message" });

			for (int index = 0; index < messageBlocks.Count; index++)
			{
				MessageBlock block = messageBlocks[index];
				var qt95 = block as IINBQT95;
				if (qt95 != null)
				{
					MessageBlock headerIdentifyingBlock = GetHeaderIdentifyingBlockQT95RespondingTo(index);

					if (headerIdentifyingBlock != null)
					{
						IMessageAttachee header = GetHeader(headerIdentifyingBlock, linkedHeader);

						if (header != null)
						{
							var status = qt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected ? ABIResponseStatus.Rejected : ABIResponseStatus.Cleared;

							hasLinesCleared |= status == ABIResponseStatus.Cleared;
							hasLinesRejected |= status == ABIResponseStatus.Rejected;

							CalculateStatus(header, status);
						}
					}

					var errorCode = qt95.Code;
					ZString description;

					if (CommonErrors.GetShortDescription(errorCode).IsEmpty)
					{
						description = qt95.NarrativeMessage;
					}
					else
					{
						description = CommonErrors.GetShortDescription(errorCode) + "\r\n" + CommonErrors.GetLongDescription(errorCode);
					}

					htmlTable.WriteRow(currentBillNumber, errorCode, description);
				}
				else
				{
					if (block is IINBQP10 || block is IINBQP20)
					{
						currentBillNumber = ZString.Empty;
					}
					else
					{
						var inbqp30 = block as IINBQP30;
						if (inbqp30 != null)
						{
							currentBillNumber = inbqp30.MasterBillNumber;
						}
						else
						{
							if (!hasQt95Blocks)
							{
								var errorsBlocks = block as IInBondErrors;
								if (errorsBlocks != null)
								{
									var narrativeMessage = errorsBlocks.NarrativeMessage;
									if (!narrativeMessage.IsEmpty)
									{
										hasLinesRejected = true;

										var header = GetLinkedHeader();
										if (header != null)
										{
											CalculateStatus(header, ABIResponseStatus.Rejected);
										}
										htmlTable.WriteRow(CommonErrors.GetMessageTextFromDescription(narrativeMessage));
									}
								}
							}
						}
					}
				}
			}

			ABIResponseStatus entryLevelStatus = GetAccumulatedStatusForEntry(hasLinesCleared, hasLinesRejected);

			string textForPendingMessage = "";
			string jobNumber = "", uri;
			bool isFailure = entryLevelStatus == ABIResponseStatus.Rejected;

			if (linkedHeader != null)
			{
				CalculateStatus(linkedHeader, entryLevelStatus);

				textForPendingMessage = new InBondPendingMessageManager().Process(linkedHeader, entryLevelStatus);
				if (linkedHeader is IInBondQPHeader inBondHeader)
				{
					jobNumber = inBondHeader.JobNumber;
				}
				else if (linkedHeader is IInBondQPBill inBondBill)
				{
					jobNumber = inBondBill.Header?.JobNumber ?? ZString.Empty;
				}
				uri = !string.IsNullOrEmpty(jobNumber) ? ObjectFactory.Get<IShowEditFormUrlCreator>().Create(linkedHeader) : "";
				var supporter = linkedHeader as IInBondWarehouseIntegrationSupporter;
				if (supporter != null && supporter.ShouldUpdate(isFailure, Message.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDepartureDelete))
				{
					delayEmailReport = entryLevelStatus != ABIResponseStatus.Cleared || string.IsNullOrEmpty(textForPendingMessage);
				}
			}
			else
			{
				if (hasQt95Blocks)
				{
					htmlTable.WriteRow("-", NoEntryFoundBody);
				}
				else
				{
					htmlTable.WriteRow(NoEntryFoundBody);
				}

				jobNumber = "Unknown";
				uri = "";
			}

			GlbBranch branch = linkedHeader != null ? linkedHeader.Branch : null;
			string emailBody = htmlTable.ToHtml();
			if (!string.IsNullOrEmpty(textForPendingMessage))
			{
				emailBody += textForPendingMessage;
			}
			if (delayEmailReport)
			{
				Message.Factory.Saved -= Factory_Saved;
				Message.Factory.Saved += Factory_Saved;
				new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, "InBond Departure", emailBody + EmailDefBuilder.HtmlTemplates.DynamicHtml1 + EmailDefBuilder.HtmlTemplates.DynamicHtml2 + EmailDefBuilder.HtmlTemplates.DynamicHtml3, isFailure, out emailReportThatHasBeenDelayed, branch);
			}
			else
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "InBond Departure", emailBody, isFailure, branch, linkedHeader as BusinessObject);
			}
		}

		#region Inventory Management
		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}
			var supporter = GetLinkedHeader() as IInBondWarehouseIntegrationSupporter;
			if (supporter != null)
			{
				new BondedWarehouseInBondMessageProcessor(Message.PK, emailReportThatHasBeenDelayed, SendEmailToOriginalSenderOrGroupIfSenderInvalid).ProcessAfterSaved(savedSuccessfully);
			}
		}

		void SendEmailToOriginalSenderOrGroupIfSenderInvalid(IInBondWarehouseIntegrationSupporter supporter, EmailDef email, bool isFailure)
		{
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, supporter?.Branch ?? GlbBranch.CurrentBranch, isFailure);
			if (object.ReferenceEquals(email, emailReportThatHasBeenDelayed))
			{
				emailReportThatHasBeenDelayed = null;
			}
		}
		EmailDef emailReportThatHasBeenDelayed;
		#endregion

		IMessageAttachee GetLinkedHeader()
		{
			IMessageAttachee result = null;
			if (Message.OriginalMessage != null)
			{
				var linkedObject = Message.OriginalMessage.EM_LinkedObject;
				result = linkedObject as IMessageAttachee;
				if (result != null)
				{
					Message.EM_LinkedObject = linkedObject;
					Message.EM_LinkTable = linkedObject.TableName;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string NoEntryFoundBody = "No InBond entry was found to match the attached message and CargoWise One cannot process the message.";

		ABIResponseStatus GetAccumulatedStatusForEntry(bool hasLinesCleared, bool hasLinesRejected)
		{
			if (hasLinesCleared && !hasLinesRejected)
			{
				return ABIResponseStatus.Cleared;
			}
			else if (hasLinesCleared && hasLinesRejected)
			{
				return ABIResponseStatus.PartialCleared;
			}
			else if (!hasLinesCleared && hasLinesRejected)
			{
				return ABIResponseStatus.Rejected;
			}
			else
			{
				return ABIResponseStatus.Undefined;
			}
		}

		IMessageAttachee GetHeader(MessageBlock block, IMessageAttachee linkedHeader)
		{
			IMessageAttachee result = null;

			if (linkedHeader is IInBondQPHeader inbondHeader)
			{
				if (block.MandatoryCharacters == "10" || block.MandatoryCharacters == "20")
				{
					result = linkedHeader;
				}
				else if (block.MandatoryCharacters == "30" && linkedHeader != null)
				{
					var qp30 = block as IINBQP30;
					if (qp30 != null && !qp30.SequenceNumber.IsEmpty)
					{
						result = inbondHeader.FindBillMatchingSeqNo(qp30.SequenceNumber);
					}
				}
			}
			else
			{
				result = linkedHeader;
			}

			return result;
		}

		MessageBlock GetHeaderIdentifyingBlockQT95RespondingTo(int indexOfQT95)
		{
			for (int index = indexOfQT95 - 1; index >= 0 && index < messageBlocks.Count; index--)
			{
				MessageBlock currentBlock = messageBlocks[index];
				if (currentBlock.MandatoryCharacters == "10" ||
					currentBlock.MandatoryCharacters == "20")
				{
					return currentBlock;
				}
				else if (currentBlock.MandatoryCharacters == "30")
				{
					var qp30 = currentBlock as IINBQP30;
					if (qp30 != null && !qp30.SequenceNumber.IsEmpty)
					{
						return currentBlock;
					}
				}
			}
			return null;
		}

		void CalculateStatus(IMessageAttachee header, ABIResponseStatus status)
		{
			if (header != null)
			{
				new InBondMessageStatusCalculator(header).CalculateStatus(Message, status);
			}
		}

		InBondCommonErrors CommonErrors
		{
			get
			{
				if (fCommonErrors == null)
				{
					fCommonErrors = new InBondCommonErrors();
				}
				return fCommonErrors;
			}
		}
		InBondCommonErrors fCommonErrors;
	}
}
