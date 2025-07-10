using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	public class SimplifiedEntryProcessor : ACEABIProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void Process()
		{
			var messageLinkedObject = OriginalMessageLinker.Link(Message) as ISimplifiedMessageLinkedObject;
			var messageLinkedParentBO = messageLinkedObject?.ParentBusinessObject;
			var entrySummaryEntry = messageLinkedParentBO?.EntrySummaryEntry;
			var cargoReleaseEntry = messageLinkedParentBO?.SimplifiedEntry;
			var tempDeclaration = messageLinkedParentBO as JobDeclaration;

			var creator = new HtmlTableCreator(new string[] { "Line No.", "Message Identifier Code", "Error Message", "Description" });
			var isPendingReview = false;
			ASESE40 previousLineBlock = null;
			var lastSE90 = (ASESE90)messageBlocks.FindLast(x => x is ASESE90);
			var isFailure = lastSE90 != null && IsRejected(lastSE90.MessageTypeCode);

			var relatedObjectExist = tempDeclaration != null && entrySummaryEntry != null;
			foreach (var block in messageBlocks)
			{
				if (block is ASESE40 se40)
				{
					previousLineBlock = se40;
				}
				else if (block is ASESE90 se90)
				{
					isPendingReview |= se90.MessageTypeCode == SimplifiedEntryMessageTypeCodesList.Codes.CancellationRequestPending;
					var lineNo = previousLineBlock != null && se90.MessageTypeCode != SimplifiedEntryMessageTypeCodesList.Codes.MessageRejected ? previousLineBlock.LineItemIdentifier : ZInt.Zero;
					creator.WriteRow(lineNo.IsEmpty ? string.Empty : lineNo.ToString(), se90.MessageIdentifierCode, se90.NarrativeMessageText, GetDetailedNarrativeText(se90.MessageIdentifierCode));

					if (relatedObjectExist && !lineNo.IsEmpty && IsValidQuataBlock(se90.MessageIdentifierCode))
					{
						var entryLine = entrySummaryEntry.MergedLines.FindByLineNumber(lineNo);
						if (entryLine != null)
						{
							entryLine.QuotaDispositions.AddQuotaDisposition(se90.MessageIdentifierCode, Message.EM_MessageType, Message.EM_MessageDateTime, se90.NarrativeMessageText);
						}
					}
				}
			}

			var html = new StringBuilder();
			html.Append(creator.ToHtml());

			if (cargoReleaseEntry != null)
			{
				Message.EM_LinkedObject = cargoReleaseEntry.LinkedObject;
				messageLinkedObject = cargoReleaseEntry;
			}

			SetMessageSubTypeIfRequired();

			if (messageLinkedParentBO != null && tempDeclaration != null)
			{
				var pgaLinesDataCorrectionManager = new PGALinesDataCorrectionManager(Message, tempDeclaration);
				pgaLinesDataCorrectionManager.UpdatePGALines(isFailure);
			}

			var jobNumber = messageLinkedParentBO?.ReferenceNumber ?? "Unknown";
			var url = string.Empty;
			var branch = messageLinkedObject?.Branch ?? GlbBranch.CurrentBranch;

			if (messageLinkedObject != null)
			{
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create((IControllerIDProvider)messageLinkedObject.LinkedObject);
				var entryBeingCertified = entrySummaryEntry != null && entrySummaryEntry.IsCargoReleaseBeingCertified ? entrySummaryEntry : messageLinkedObject;
				if (entryBeingCertified.IsCargoReleaseBeingCertified)
				{
					entryBeingCertified.CargoReleaseCertifiedStatus = isFailure ? "" : CargoReleaseCertificationStatusList.Codes.Certified;
				}

				var status = isFailure ? ABIResponseStatus.Rejected :
					(isPendingReview ? ABIResponseStatus.PendingReview : ABIResponseStatus.Cleared);

				new SimplifiedEntryMessageStatusCalculator(messageLinkedObject).CalculateStatus(Message, status);

				LogMessageStatusChangeEventAgainstTopLevelBusinessObject(messageLinkedObject, messageLinkedObject.GetMSCEventReferenceForCargoReleaseResponse(Message));

				EmailDef email;
				if (new HtmlResponseEmailGenerator().TryGenerateEmail(url, jobNumber, "ACE Cargo Release", html.ToString(), "", isFailure, out email, branch))
				{
					var registryItem = GetEmailGroupRegistryItem() as Registry.Business.Customs.ManifestGroupNotificationRegistryItem;
					var groupNotification = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

					var shouldSendErrorNotificationsOnly = groupNotification.SendErrorOnly;
					if (!shouldSendErrorNotificationsOnly || (shouldSendErrorNotificationsOnly && isFailure))
					{
						var messages = entryBeingCertified.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease || x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary);
						var emailAddresses = entryBeingCertified.ParentBusinessObject.GetEmailRecipients(messages);

						var recipientCalculator = new EmailRecipientCalculator(groupNotification.SendMode, groupNotification.SendGroupPK, emailAddresses, ZGuid.Empty);
						recipientCalculator.SendNotifications(Factory, email, registryItem);
					}
				}

				if (!isFailure && tempDeclaration != null)
				{
					CargoManifestStatusQuerySender.MarkForAutoSendingIfEligible(tempDeclaration, true);
				}
			}
			else
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "ACE Cargo Release", html.ToString(), isFailure, branch, null);
			}
		}

		string GetDetailedNarrativeText(string code)
		{
			var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ACECargoReleaseSEInputValidationRules, ZDateTime.Now);
			return cusCodeList == null ? ZString.Empty : cusCodeList.ZZD_Description;
		}

		void SetMessageSubTypeIfRequired()
		{
			var outgoing = Message.OriginalMessage;
			//certified from entry summary
			if (outgoing != null && outgoing.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary)
			{
				var se10 = (ASESE10)Message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE10);

				if (se10 != null)
				{
					switch (se10.UpdateActionCode.ToString())
					{
						case UpdateActionCodeConverter.AddCode:
							Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
							break;
						case UpdateActionCodeConverter.UpdateCode:
							Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
							break;
						case UpdateActionCodeConverter.ReplaceCode:
							Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
							break;
						case UpdateActionCodeConverter.DeleteCode:
							Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
							break;
					}
				}
			}
		}

		bool IsRejected(string msgTypeCode)
		{
			return msgTypeCode != SimplifiedEntryMessageTypeCodesList.Codes.MessageAccepted &&
				msgTypeCode != SimplifiedEntryMessageTypeCodesList.Codes.MessageAcceptedWithWarning &&
				msgTypeCode != SimplifiedEntryMessageTypeCodesList.Codes.CancellationRequestPending;
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			if (Message.EM_LinkTable == CusUSLVConsignmentSchema.Constants.TableName)
			{
				return USCustomsDataRegistry.Instance.LowValueEntriesReleaseMessages;
			}

			return base.GetEmailGroupRegistryItem();
		}

		bool IsValidQuataBlock(ZString messageIdentifierCode)
		{
			return !messageIdentifierCode.IsEmpty
				&& QuotaDispositionCodeList.GetQuotaDispositionCodeList(Factory).Any(a => a.Code.Equals(messageIdentifierCode));
		}
	}
}
