using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse, Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	public class InBondArrivalOrFDAPriorNoticeProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			var parent = GetLinkedHeader();

			var hasError = false;
			var hasClear = false;

			var htmlBody = new ZStringBuilder();
			HtmlTableCreator htmlTable = null;
			string lineWithError = null;
			var hasWTBlocks = messageBlocks.Any(x => x is IINBWT95);
			var isPriorNotice = false;

			foreach (MessageBlock block in messageBlocks)
			{
				var inbfd01 = block as IINBFD01BTAPriorNotice;
				if (inbfd01 != null)
				{
					isPriorNotice = true;
					if (htmlTable != null)
					{
						htmlBody.Append(htmlTable.ToHtml());
						htmlTable = null;
					}

					lineWithError = inbfd01.FDALineNumber.ToString();
				}
				else
				{
					var wt95 = block as IINBWT95;
					if (wt95 != null)
					{
						hasError |= wt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected;
						hasClear |= wt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Accepted;

						if (htmlTable == null)
						{
							var secondColumnHeader = hasClear ? "Narrative" : "Error Description";
							if (lineWithError != null && !WholeMessageNarrative(wt95.Code))
							{
								secondColumnHeader += " for FDA Line " + lineWithError;
							}

							htmlTable = new HtmlTableCreator(new string[] { "ID", secondColumnHeader });
						}

						ZString description;
						if (CommonErrors.GetShortDescription(wt95.Code).IsEmpty)
						{
							description = wt95.NarrativeMessage;
						}
						else
						{
							description = CommonErrors.GetShortDescription(wt95.Code) + "<p />" + CommonErrors.GetLongDescription(wt95.Code);
						}

						htmlTable.WriteRow(wt95.Code, description);
					}
					else
					{
						var errorsBlocks = block as IInBondErrors;
						if (!hasWTBlocks && errorsBlocks != null)
						{
							if (htmlTable == null)
							{
								htmlTable = new HtmlTableCreator(new string[] { "Error Narrative Message" });
							}

							var narrativeMessage = errorsBlocks.NarrativeMessage;
							if (!narrativeMessage.IsEmpty)
							{
								hasError = true;
								htmlTable.WriteRow(CommonErrors.GetMessageTextFromDescription(narrativeMessage));
							}
						}
					}
				}
			}

			if (htmlTable != null)
			{
				htmlBody.Append(htmlTable.ToHtml());
			}

			var status = GetStatus(hasError, hasClear);

			if (parent != null && ShouldCalculateStatus())
			{
				new InBondMessageStatusCalculator(parent).CalculateStatus(Message, status);
			}

			if (isPriorNotice)
			{
				SetFDAStatus(parent as IPriorNoticeHeader, hasError);
			}

			var inbondHeader = parent as IInBondArriveExportTOLHeader;
			if (inbondHeader != null)
			{
				SendEmailReport(status, inbondHeader.JobNumber, inbondHeader.EntryNumber, inbondHeader, htmlBody.ToString(), isPriorNotice);
				if (!hasError)
				{
					inbondHeader.UpdateLinkBusinessObject(Message);
				}
			}
			else
			{
				var attacheeInDeclaration = parent as IMessageAttacheeInDeclaration;
				if (attacheeInDeclaration != null)
				{
					SendEmailReport(status, attacheeInDeclaration.JobReferenceNumber, attacheeInDeclaration.EntryNumber, attacheeInDeclaration, htmlBody.ToString(), isPriorNotice);
				}
				else
				{
					SendEmailReport(status, ZString.Empty, ZString.Empty, null, htmlBody.ToString(), isPriorNotice);
				}
			}
		}

		#region Implementation

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

		void SetFDAStatus(IPriorNoticeHeader header, bool hasError)
		{
			if (header != null)
			{
				header.FDAMsgStatus = hasError ? FDAStatusList.Codes.REQ : FDAStatusList.Codes.ACR;
			}
		}

		ABIResponseStatus GetStatus(bool hasError, bool hasClear)
		{
			var result = ABIResponseStatus.Undefined;

			if (hasError && !hasClear)
			{
				result = ABIResponseStatus.Rejected;
			}
			else if (!hasError && hasClear)
			{
				result = ABIResponseStatus.Cleared;
			}
			else if (hasError && hasClear)
			{
				result = ABIResponseStatus.PartialCleared;
			}

			return result;
		}

		void SendEmailReport(ABIResponseStatus status, ZString jobReferenceNumber, ZString entryNumber, IControllerIDProvider controllerIDProvider, string htmlBody, bool isPriorNotice)
		{
			var jobNumber = "Unknown";
			var uri = "";
			if (!jobReferenceNumber.IsEmpty)
			{
				jobNumber = jobReferenceNumber;
				var additionalSubjectLineDetails = "";
				if (isPriorNotice)
				{
					additionalSubjectLineDetails = entryNumber;
				}
				else
				{
					var qp10 = Message.MessageBlock.MessageBlocks.OfType<IINBWP10>().FirstOrDefault();
					additionalSubjectLineDetails = qp10 != null && !qp10.InbondNumber.IsEmpty ? qp10.InbondNumber : entryNumber;
				}

				if (!string.IsNullOrEmpty(additionalSubjectLineDetails))
				{
					jobNumber = jobNumber + " / " + additionalSubjectLineDetails;
				}

				if (controllerIDProvider != null)
				{
					uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerIDProvider);
				}
			}

			var subject = isPriorNotice ? "FDA Prior Notice" : "In-Bond Arrival";
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, subject, htmlBody, status == ABIResponseStatus.Rejected, branch, null);
		}

		bool WholeMessageNarrative(string narrativeIdentifier)
		{
			return narrativeIdentifier == "270" || narrativeIdentifier == "271";
		}

		bool ShouldCalculateStatus()
		{
			var originalMessage = Message.OriginalMessage;
			return originalMessage == null
				|| originalMessage.EM_MessageType != ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability
				|| originalMessage.EM_MessageSubType != EM_MessageSubTypeList.Codes.InBondDiversionRequest;
		}

		#endregion

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
